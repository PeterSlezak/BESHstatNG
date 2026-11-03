Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Namespace ExcelCommon
    Public Enum RegressionEffectOperation
        ContinuousMain
        CategoricalMain
        Polynomial
        PairwiseInteractions
        CustomInteraction
    End Enum

    Public Enum RegressionEffectSeverity
        Warning
        [Error]
    End Enum

    ''' <summary>Opt-in portable authoring budgets; shipping compatibility calls do not impose new caps.</summary>
    Public NotInheritable Class RegressionEffectAuthoringLimits
        Public Property MaximumCatalogSize As Integer = 2048
        Public Property MaximumInteractionWidth As Integer = 32
        Public Property MaximumDegree As Integer = 1024
        Public Property MaximumKeyLength As Integer = 32768
    End Class

    ''' <summary>Authoring diagnostics have keys, not invented spreadsheet coordinates.</summary>
    Public NotInheritable Class RegressionEffectDiagnostic
        Public Sub New(code As String, message As String, Optional rawKey As String = Nothing,
                       Optional effectKey As String = Nothing,
                       Optional severity As RegressionEffectSeverity = RegressionEffectSeverity.Error,
                       Optional presentationSnapshot As RegressionEffectModel = Nothing)
            Me.Code = code : Me.Message = message : Me.RawKey = rawKey : Me.EffectKey = effectKey : Me.Severity = severity
            Me.PresentationSnapshot = presentationSnapshot
        End Sub
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property RawKey As String
        Public ReadOnly Property EffectKey As String
        Public ReadOnly Property Severity As RegressionEffectSeverity
        ''' <summary>Frozen operation state at the diagnostic boundary, before later selections are processed.</summary>
        Public ReadOnly Property PresentationSnapshot As RegressionEffectModel
    End Class

    ''' <summary>Frozen effect identity. Core's mutable metadata is copied on entry and every access.</summary>
    Public NotInheritable Class RegressionAuthoredEffect
        Private ReadOnly _spec As TermSpec
        Public Sub New(key As String, spec As TermSpec)
            If key Is Nothing Then Throw New ArgumentNullException(NameOf(key))
            Me.Key = key : _spec = CopySpec(spec)
        End Sub
        Public ReadOnly Property Key As String
        Public ReadOnly Property Spec As TermSpec
            Get
                Return CopySpec(_spec)
            End Get
        End Property
        Friend Shared Function CopySpec(s As TermSpec) As TermSpec
            If s Is Nothing Then Return Nothing
            Return New TermSpec With {.Kind = s.Kind, .BaseVarKeys = If(s.BaseVarKeys Is Nothing, Nothing, New List(Of String)(s.BaseVarKeys)),
                .BaseVarDisplayNames = If(s.BaseVarDisplayNames Is Nothing, Nothing, New List(Of String)(s.BaseVarDisplayNames)),
                .Degree = s.Degree, .DisplayNameForCoef = s.DisplayNameForCoef, .Order = s.Order, .Scale = s.Scale,
                .ReferenceValue = s.ReferenceValue, .ObservedLevels = If(s.ObservedLevels Is Nothing, Nothing, New List(Of Double)(s.ObservedLevels))}
        End Function
    End Class

    Public NotInheritable Class RegressionEffectModel
        Public Sub New(Optional effects As IEnumerable(Of RegressionAuthoredEffect) = Nothing, Optional includeIntercept As Boolean = True)
            Dim copied As New List(Of RegressionAuthoredEffect)
            For Each e In If(effects, Enumerable.Empty(Of RegressionAuthoredEffect)())
                If e Is Nothing Then Throw New ArgumentException("Null effect.", NameOf(effects))
                copied.Add(New RegressionAuthoredEffect(e.Key, e.Spec))
            Next
            Me.Effects = copied.AsReadOnly() : Me.IncludeIntercept = includeIntercept
        End Sub
        Public ReadOnly Property Effects As IReadOnlyList(Of RegressionAuthoredEffect)
        Public ReadOnly Property IncludeIntercept As Boolean
        Public Function ToTermSpecs() As Dictionary(Of String, TermSpec)
            Dim result As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
            For Each e In Effects
                If e.Spec IsNot Nothing Then result.Add(e.Key, e.Spec)
            Next
            Return result
        End Function
        Public Shared Function FromProjection(keys As IEnumerable(Of String), specs As IDictionary(Of String, TermSpec),
                                              Optional includeIntercept As Boolean = True) As RegressionEffectModel
            Dim effects As New List(Of RegressionAuthoredEffect)
            For Each key In If(keys, Enumerable.Empty(Of String)())
                Dim s As TermSpec = Nothing
                If specs IsNot Nothing Then specs.TryGetValue(key, s)
                effects.Add(New RegressionAuthoredEffect(key, s))
            Next
            Return New RegressionEffectModel(effects, includeIntercept)
        End Function
    End Class

    Public NotInheritable Class RegressionEffectResult
        Public Sub New(model As RegressionEffectModel, diagnostics As IEnumerable(Of RegressionEffectDiagnostic), Optional changed As Boolean = False)
            Me.Model = model : Me.Diagnostics = New List(Of RegressionEffectDiagnostic)(diagnostics).AsReadOnly()
            Me.Changed = changed
        End Sub
        Public ReadOnly Property Model As RegressionEffectModel
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionEffectDiagnostic)
        Public ReadOnly Property Changed As Boolean
        Public ReadOnly Property PartialSuccess As Boolean
            Get
                Return Changed AndAlso Not Success
            End Get
        End Property
        Public ReadOnly Property Success As Boolean
            Get
                Return Not Diagnostics.Any(Function(d) d.Severity = RegressionEffectSeverity.Error)
            End Get
        End Property
    End Class

    ''' <summary>Legacy-permissive effect authoring. Core owns keys, parsing, coding and design expansion.
    ''' No upper degree guard is imposed on the shipping compatibility path (all Int32 degrees >=2
    ''' were accepted). Optional budgets are applied only by the portable caller that requests them.</summary>
    Public NotInheritable Class RegressionEffectAuthoringPolicy
        Private Sub New()
        End Sub

        Public Shared Function Apply(catalog As IEnumerable(Of String), selectedIndices As IEnumerable(Of Integer),
                                     existing As RegressionEffectModel, operation As RegressionEffectOperation,
                                     Optional degree As Integer = 2, Optional maximumEffects As Integer = Integer.MaxValue,
                                     Optional limits As RegressionEffectAuthoringLimits = Nothing) As RegressionEffectResult
            Dim model = If(existing, New RegressionEffectModel())
            Dim effects = model.Effects.ToList()
            Dim issues As New List(Of RegressionEffectDiagnostic)
            Dim vars = If(catalog, Enumerable.Empty(Of String)()).ToList()
            Dim indices = If(selectedIndices, Enumerable.Empty(Of Integer)()).Distinct().OrderBy(Function(i) i).ToList()
            If operation = RegressionEffectOperation.Polynomial AndAlso degree < 2 Then
                issues.Add(New RegressionEffectDiagnostic("degree", "Polynomial degree must be >= 2."))
                Return New RegressionEffectResult(model, issues)
            End If
            Dim interaction = operation = RegressionEffectOperation.PairwiseInteractions OrElse operation = RegressionEffectOperation.CustomInteraction
            If indices.Count < If(interaction, 2, 1) Then
                issues.Add(New RegressionEffectDiagnostic("selection", If(interaction, "Please select at least two variables.", "Please select variable(s).")))
                Return New RegressionEffectResult(model, issues)
            End If
            If Not [Enum].IsDefined(GetType(RegressionEffectOperation), operation) OrElse maximumEffects < 0 Then
                issues.Add(New RegressionEffectDiagnostic("operation", "Invalid authoring operation or budget."))
                Return New RegressionEffectResult(model, issues)
            End If
            If limits IsNot Nothing AndAlso (vars.Count > limits.MaximumCatalogSize OrElse
                vars.Any(Function(k) k IsNot Nothing AndAlso k.Length > limits.MaximumKeyLength) OrElse
                (operation = RegressionEffectOperation.CustomInteraction AndAlso indices.Count > limits.MaximumInteractionWidth) OrElse
                (operation = RegressionEffectOperation.Polynomial AndAlso degree > limits.MaximumDegree)) Then
                issues.Add(New RegressionEffectDiagnostic("input-budget", "Authoring input budget exceeded."))
                Return New RegressionEffectResult(model, issues)
            End If
            Dim selected As New List(Of String)
            For Each i In indices
                If i < 0 OrElse i >= vars.Count OrElse vars(i) Is Nothing OrElse vars(i).Length = 0 Then
                    issues.Add(New RegressionEffectDiagnostic("unknown-variable", "Selected variable is missing from the catalog."))
                Else
                    selected.Add(vars(i))
                End If
            Next
            ' A malformed interaction selection must not silently become a narrower interaction.
            If interaction AndAlso issues.Count > 0 Then Return New RegressionEffectResult(model, issues)
            For Each key In selected
                If interaction Then Exit For
                Dim termKey = key
                Dim kind = "MainEffect"
                Dim scale = PredictorScale.Continuous
                Dim label = RegressionDesignCore.GetCoefBaseName(key)
                Dim categorical = HasCategorical(effects, key)
                Select Case operation
                    Case RegressionEffectOperation.ContinuousMain
                        If categorical Then
                            issues.Add(New RegressionEffectDiagnostic("categorical-conflict", "Variable '" & label & "' is already marked as categorical. Remove it first if you want to use it as continuous.", key, presentationSnapshot:=New RegressionEffectModel(effects, model.IncludeIntercept)))
                            Continue For
                        End If
                    Case RegressionEffectOperation.CategoricalMain
                        termKey = RegressionDesignCore.MakeCategoricalEffectKey(key)
                        If effects.Any(Function(e) e.Spec IsNot Nothing AndAlso e.Spec.BaseVarKeys IsNot Nothing AndAlso e.Spec.BaseVarKeys.Contains(key, StringComparer.Ordinal)) AndAlso Not categorical Then
                            issues.Add(New RegressionEffectDiagnostic("used-conflict", "Variable '" & label & "' is already used in the model. Remove its existing term(s) before adding it as categorical.", key, presentationSnapshot:=New RegressionEffectModel(effects, model.IncludeIntercept)))
                            Continue For
                        End If
                        scale = PredictorScale.Categorical
                    Case RegressionEffectOperation.Polynomial
                        If categorical Then
                            issues.Add(New RegressionEffectDiagnostic("categorical-polynomial", "Polynomial terms are not supported for categorical predictors.", key, presentationSnapshot:=New RegressionEffectModel(effects, model.IncludeIntercept)))
                            Continue For
                        End If
                        termKey = RegressionDesignCore.MakePolynomialEffectKey(key, degree)
                        kind = "Polynomial" : label &= "^" & degree.ToString(CultureInfo.InvariantCulture)
                End Select
                AddEffect(effects, termKey, New TermSpec With {.Kind = kind, .BaseVarKeys = New List(Of String) From {key},
                    .Degree = If(kind = "Polynomial", degree, 1), .DisplayNameForCoef = label, .Scale = scale}, issues, maximumEffects)
            Next
            If interaction Then
                If operation = RegressionEffectOperation.CustomInteraction Then
                    AddInteraction(effects, selected, issues, maximumEffects)
                Else
                    For a = 0 To selected.Count - 2
                        For b = a + 1 To selected.Count - 1
                            AddInteraction(effects, New List(Of String) From {selected(a), selected(b)}, issues, maximumEffects)
                        Next
                    Next
                End If
            End If
            Return New RegressionEffectResult(New RegressionEffectModel(effects, model.IncludeIntercept), issues, effects.Count <> model.Effects.Count)
        End Function

        Private Shared Function HasCategorical(effects As IEnumerable(Of RegressionAuthoredEffect), key As String) As Boolean
            Return effects.Any(Function(e) String.Equals(e.Key, RegressionDesignCore.MakeCategoricalEffectKey(key), StringComparison.Ordinal) AndAlso e.Spec IsNot Nothing)
        End Function
        Private Shared Sub AddInteraction(effects As List(Of RegressionAuthoredEffect), keys As List(Of String), issues As List(Of RegressionEffectDiagnostic), limit As Integer)
            AddEffect(effects, RegressionDesignCore.MakeInteractionEffectKey(keys), New TermSpec With {
                .Kind = "Interaction", .BaseVarKeys = keys, .Degree = 1,
                .DisplayNameForCoef = RegressionDesignCore.MakeInteractionCoefName(keys),
                .Scale = If(keys.Any(Function(k) HasCategorical(effects, k)), PredictorScale.Categorical, PredictorScale.Continuous)}, issues, limit)
        End Sub
        Private Shared Sub AddEffect(effects As List(Of RegressionAuthoredEffect), key As String, spec As TermSpec,
                                     issues As List(Of RegressionEffectDiagnostic), limit As Integer)
            If effects.Any(Function(e) String.Equals(e.Key, key, StringComparison.Ordinal)) Then Return
            If effects.Count >= limit Then
                issues.Add(New RegressionEffectDiagnostic("effect-budget", "Effect budget exceeded.", effectKey:=key)) : Return
            End If
            spec.Order = effects.Count
            effects.Add(New RegressionAuthoredEffect(key, spec))
        End Sub

        ''' <summary>Optional inspection only: no parent insertion and no authoring rejection.</summary>
        Public Shared Function InspectHierarchy(model As RegressionEffectModel) As IReadOnlyList(Of RegressionEffectDiagnostic)
            Dim issues As New List(Of RegressionEffectDiagnostic)
            If model Is Nothing Then Return issues.AsReadOnly()
            For Each e In model.Effects
                Dim s = e.Spec
                If s Is Nothing OrElse s.Kind <> "Interaction" OrElse s.BaseVarKeys Is Nothing Then Continue For
                For Each key In s.BaseVarKeys
                    Dim present = model.Effects.Any(Function(p) p.Spec IsNot Nothing AndAlso p.Spec.Kind = "MainEffect" AndAlso p.Spec.BaseVarKeys IsNot Nothing AndAlso p.Spec.BaseVarKeys.SequenceEqual({key}, StringComparer.Ordinal))
                    issues.Add(New RegressionEffectDiagnostic(If(present, "parent-available", "parent-missing"),
                        If(present, "Parent main effect is available.", "Interaction has no parent main effect."), key, e.Key, RegressionEffectSeverity.Warning))
                Next
            Next
            Return issues.AsReadOnly()
        End Function

        ''' <summary>Inspection is separate from legacy authoring. Observed data are not inferred.
        ''' A missing reference in a supplied observed-level set is reported, never recoded.</summary>
        Public Shared Function Inspect(model As RegressionEffectModel, catalog As IEnumerable(Of String)) As IReadOnlyList(Of RegressionEffectDiagnostic)
            Dim issues As New List(Of RegressionEffectDiagnostic)
            If model Is Nothing Then
                issues.Add(New RegressionEffectDiagnostic("model", "Model snapshot is required."))
                Return issues.AsReadOnly()
            End If
            Dim keys As New HashSet(Of String)(If(catalog, Enumerable.Empty(Of String)()), StringComparer.Ordinal)
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For i = 0 To model.Effects.Count - 1
                Dim e = model.Effects(i), s = e.Spec
                If Not seen.Add(e.Key) Then issues.Add(New RegressionEffectDiagnostic("duplicate-effect", "Duplicate effect identity.", effectKey:=e.Key))
                If s Is Nothing OrElse s.BaseVarKeys Is Nothing OrElse s.BaseVarKeys.Count = 0 Then
                    issues.Add(New RegressionEffectDiagnostic("term-metadata", "Missing term metadata.", effectKey:=e.Key)) : Continue For
                End If
                If s.Order <> i Then issues.Add(New RegressionEffectDiagnostic("term-order", "Term order differs from effect position.", effectKey:=e.Key))
                For Each key In s.BaseVarKeys
                    If key Is Nothing OrElse Not keys.Contains(key) Then issues.Add(New RegressionEffectDiagnostic("unknown-variable", "Raw variable is missing or renamed.", key, e.Key))
                Next
                If s.BaseVarKeys.Distinct(StringComparer.Ordinal).Count() <> s.BaseVarKeys.Count Then issues.Add(New RegressionEffectDiagnostic("repeated-base", "Repeated interaction base.", effectKey:=e.Key))
                If s.ReferenceValue.HasValue AndAlso (Double.IsNaN(s.ReferenceValue.Value) OrElse Double.IsInfinity(s.ReferenceValue.Value)) Then issues.Add(New RegressionEffectDiagnostic("reference", "Reference must be finite.", effectKey:=e.Key))
                If s.ObservedLevels IsNot Nothing AndAlso s.ReferenceValue.HasValue AndAlso Not s.ObservedLevels.Contains(s.ReferenceValue.Value) Then issues.Add(New RegressionEffectDiagnostic("observed-reference", "Reference is absent from supplied observed levels; actual data validation remains Core-owned.", effectKey:=e.Key))
            Next
            Return issues.AsReadOnly()
        End Function

        ''' <summary>Legacy removal cascades from text-classified main effects. Preserves its exact
        ''' punctuation/whitespace semantics; changing this to typed identity requires separate review.</summary>
        Public Shared Function Remove(model As RegressionEffectModel, selectedKeys As IEnumerable(Of String), Optional clear As Boolean = False) As RegressionEffectResult
            model = If(model, New RegressionEffectModel())
            Dim issues As New List(Of RegressionEffectDiagnostic)
            If clear Then Return New RegressionEffectResult(New RegressionEffectModel(includeIntercept:=model.IncludeIntercept), issues, model.Effects.Count > 0)
            Dim selected As New HashSet(Of String)(If(selectedKeys, Enumerable.Empty(Of String)()), StringComparer.Ordinal)
            If selected.Count = 0 AndAlso model.Effects.Count > 0 Then
                issues.Add(New RegressionEffectDiagnostic("remove-selection", "Please select variable(s) to remove."))
                Return New RegressionEffectResult(model, issues)
            End If
            Dim bases As New HashSet(Of String)(StringComparer.Ordinal)
            For Each key In selected
                If Not IsDerived(key) Then bases.UnionWith(LegacyBases(key).Where(Function(k) k <> String.Empty))
            Next
            Dim kept As New List(Of RegressionAuthoredEffect)
            For Each e In model.Effects
                If selected.Contains(e.Key) OrElse LegacyBases(e.Key).Any(Function(k) bases.Contains(k)) Then Continue For
                Dim s = e.Spec
                If s IsNot Nothing Then s.Order = kept.Count
                kept.Add(New RegressionAuthoredEffect(e.Key, s))
            Next
            Return New RegressionEffectResult(New RegressionEffectModel(kept, model.IncludeIntercept), issues, kept.Count <> model.Effects.Count)
        End Function
        Private Shared Function IsDerived(key As String) As Boolean
            Dim s = key.Trim()
            If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then Return False
            Dim p = s.LastIndexOf("^"c), n As Integer
            If p >= 0 AndAlso p < s.Length - 1 AndAlso Integer.TryParse(s.Substring(p + 1), n) Then Return True
            Return s.Contains(":") AndAlso s.Contains("""") AndAlso RegressionDesignCore.ExtractBaseKeysFromEffectText(s).Count >= 2
        End Function
        Private Shared Function LegacyBases(key As String) As List(Of String)
            Dim s = key.Trim(), p = s.LastIndexOf("^"c), n As Integer
            If p >= 0 AndAlso p < s.Length - 1 AndAlso Integer.TryParse(s.Substring(p + 1), n) Then
                Return New List(Of String) From {RegressionDesignCore.StripOuterDoubleQuotesPublic(s.Substring(0, p).Trim())}
            End If
            If s.Contains(":") AndAlso s.Contains("""") Then
                Dim parts = s.Split(":"c).Select(Function(k) RegressionDesignCore.StripOuterDoubleQuotesPublic(k.Trim())).Where(Function(k) k <> String.Empty).ToList()
                If parts.Count >= 2 Then Return parts
            End If
            If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then s = s.Substring(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX.Length).Trim()
            Return New List(Of String) From {s}
        End Function
    End Class
End Namespace
