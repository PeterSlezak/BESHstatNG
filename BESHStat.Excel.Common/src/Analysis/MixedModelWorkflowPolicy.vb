Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum MixedModelWorkflowKind
        MMRM
        LMM
    End Enum

    ''' <summary>Caller-owned option builder. Requests retain and return independent copies.
    ''' Raw GUI display choices are retained alongside exact legacy effective choices.</summary>
    Public NotInheritable Class MixedModelWorkflowOptions
        Public Property ResidualStructure As String = "Unstructured"
        Public Property RandomStructure As String = "Random Intercept"
        Public Property FitMethod As String = "REML"
        Public Property Inference As String = "Kenward-Roger"
        Public Property Optimizer As String = "AI/Fisher scoring (default)"
        Public Property Gradient As String = "Auto (analytic where available)"
        Public Property MaximumIterations As Integer = 500
        Public Property Epsilon As Double = 0.000001
        Public Property Trace As Boolean
        Public Property IterationDetails As Boolean
        Public Property FixedIntercept As Boolean = True
        Public Property RandomIntercept As Boolean = True
        Public Shared Function Defaults(kind As MixedModelWorkflowKind) As MixedModelWorkflowOptions
            Return New MixedModelWorkflowOptions With {.ResidualStructure = If(kind = MixedModelWorkflowKind.LMM, "Identity", "Unstructured"),
                .RandomIntercept = kind = MixedModelWorkflowKind.LMM}
        End Function
        Public Function Copy() As MixedModelWorkflowOptions
            Return DirectCast(MemberwiseClone(), MixedModelWorkflowOptions)
        End Function
    End Class

    Public NotInheritable Class MixedModelFitDecision
        Friend Sub New(kind As MixedModelWorkflowKind, fit As String, inference As String)
            RequestedMethod = If(String.Equals(fit, "ML", StringComparison.OrdinalIgnoreCase), MixedModelFitMethod.ML, MixedModelFitMethod.REML)
            ' Deliberately different comparisons in the original GUI: the KR timing guard
            ' ignores case; the inference Select Case is exact and may take its fallback.
            CoerceToReml = RequestedMethod = MixedModelFitMethod.ML AndAlso String.Equals(inference, "Kenward-Roger", StringComparison.OrdinalIgnoreCase)
            EffectiveMethod = If(CoerceToReml, MixedModelFitMethod.REML, RequestedMethod)
            Select Case inference
                Case "Large-sample normal" : EffectiveInference = MixedModelFixedInferenceMethod.WaldNormal
                Case "Residual DF" : EffectiveInference = MixedModelFixedInferenceMethod.ResidualDF
                Case "Satterthwaite" : EffectiveInference = MixedModelFixedInferenceMethod.Satterthwaite
                Case "Kenward-Roger" : EffectiveInference = MixedModelFixedInferenceMethod.KenwardRoger
                Case Else
                    EffectiveInference = If(kind = MixedModelWorkflowKind.MMRM, MixedModelFixedInferenceMethod.BetweenWithin, MixedModelFixedInferenceMethod.WaldNormal)
                    InferenceFallback = inference <> "Between-within DF" OrElse kind <> MixedModelWorkflowKind.MMRM
            End Select
        End Sub
        Public ReadOnly Property RequestedMethod As MixedModelFitMethod
        Public ReadOnly Property EffectiveMethod As MixedModelFitMethod
        Public ReadOnly Property EffectiveInference As MixedModelFixedInferenceMethod
        Public ReadOnly Property CoerceToReml As Boolean
        Public ReadOnly Property InferenceFallback As Boolean
    End Class

    ''' <summary>Reuse Core's immutable subject-block snapshot: one existing block copy of
    ''' Y/X/Z, O(n) row/subject sidecars, no second long-lived n-by-p or n-by-q matrix.
    ''' Core sorts subject keys ordinally and each block by visit then source row, or row
    ''' only when no visit is supplied. Those positions are never worksheet coordinates.</summary>
    Public NotInheritable Class MixedModelWorkflowData
        Private ReadOnly _rows As Integer()
        Private ReadOnly _subjects As Object()
        Private Sub New(data As MixedModelBlockData, rows() As Integer, subjects() As Object,
                        issues As IEnumerable(Of RegressionDiagnostic), provenance As SpreadsheetTransportFrame)
            CoreData = data : _rows = If(rows Is Nothing, Nothing, DirectCast(rows.Clone(), Integer()))
            _subjects = If(subjects Is Nothing, Nothing, subjects.Select(Function(v) If(v Is Nothing OrElse Type.GetTypeCode(v.GetType()) <> TypeCode.Object, v, Nothing)).ToArray())
            Diagnostics = New List(Of RegressionDiagnostic)(issues).AsReadOnly() : Me.Provenance = provenance
        End Sub
        Public ReadOnly Property CoreData As MixedModelBlockData
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionDiagnostic)
        Public ReadOnly Property Provenance As SpreadsheetTransportFrame
        Public ReadOnly Property InputRowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
        Public ReadOnly Property SubjectIds As Object()
            Get
                Return If(_subjects Is Nothing, Nothing, DirectCast(_subjects.Clone(), Object()))
            End Get
        End Property
        Public ReadOnly Property BlockRowIds As Integer()
            Get
                Return If(CoreData Is Nothing, Array.Empty(Of Integer)(), CoreData.Blocks.SelectMany(Function(b) b.RowIndices).ToArray())
            End Get
        End Property
        Public ReadOnly Property BlockToInputIndices As Integer()
            Get
                If _rows Is Nothing Then Return Array.Empty(Of Integer)()
                Dim positions As New Dictionary(Of Integer, Integer)
                For i = 0 To _rows.Length - 1
                    If Not positions.ContainsKey(_rows(i)) Then positions.Add(_rows(i), i)
                Next
                Return BlockRowIds.Select(Function(id) If(positions.ContainsKey(id), positions(id), -1)).ToArray()
            End Get
        End Property
        Public Shared Function FromArrays(kind As MixedModelWorkflowKind, y() As Double, x(,) As Double, subjects() As Object,
                                          visit() As Double, rows() As Integer, Optional z(,) As Double = Nothing,
                                          Optional provenance As SpreadsheetTransportFrame = Nothing) As MixedModelWorkflowData
            Dim issues As New List(Of RegressionDiagnostic)
            If y Is Nothing OrElse x Is Nothing OrElse subjects Is Nothing OrElse rows Is Nothing Then
                MixedModelWorkflowPolicy.Issue(issues, "data.required", "Response, expanded design, subjects and source RowIds are required.")
            Else
                If y.Length = 0 OrElse x.GetLength(1) = 0 Then MixedModelWorkflowPolicy.Issue(issues, "data.empty", "At least one observation and fixed-effect column are required.")
                If x.GetLength(0) <> y.Length OrElse subjects.Length <> y.Length OrElse rows.Length <> y.Length OrElse
                    (visit IsNot Nothing AndAlso visit.Length <> y.Length) OrElse (z IsNot Nothing AndAlso z.GetLength(0) <> y.Length) Then MixedModelWorkflowPolicy.Issue(issues, "data.shape", "Response, subject, visit, RowId, X and Z row axes must agree.")
                If rows.Distinct().Count() <> rows.Length Then MixedModelWorkflowPolicy.Issue(issues, "rows.duplicate", "Source RowIds must be distinct.")
                For Each array As Array In New Array() {y, x, z, visit}
                    If array Is Nothing Then Continue For
                    For Each value As Double In array
                        If Not MixedModelWorkflowPolicy.Finite(value) Then MixedModelWorkflowPolicy.Issue(issues, "data.finite", "Numeric model arrays must be finite.") : Exit For
                    Next
                Next
                ValidateSubjects(subjects, issues)
                If z IsNot Nothing AndAlso z.GetLength(1) = 0 Then MixedModelWorkflowPolicy.Issue(issues, "random.empty", "An expanded random design cannot have zero columns.")
                If kind = MixedModelWorkflowKind.LMM AndAlso (z Is Nothing OrElse z.GetLength(1) = 0) Then MixedModelWorkflowPolicy.Issue(issues, "random.empty", "LMM requires an expanded random design.")
                If kind = MixedModelWorkflowKind.MMRM AndAlso visit Is Nothing Then MixedModelWorkflowPolicy.Issue(issues, "visit.required", "MMRM requires a visit role.")
            End If
            Dim data As MixedModelBlockData = Nothing
            If Not issues.Any(Function(i) i.Blocking) Then data = MixedModelBlockData.FromArrays(y, x, subjects, z, visit, kind = MixedModelWorkflowKind.MMRM OrElse visit IsNot Nothing, rows)
            Return New MixedModelWorkflowData(data, rows, subjects, issues, provenance)
        End Function
        ''' <summary>The real form has already called the original Core builder, so original
        ''' acquisition/build errors and timing are untouched. Further diagnostics stay observational.</summary>
        Public Shared Function FromAcquiredBlocks(data As MixedModelBlockData, rows() As Integer, subjects() As Object,
                                                 Optional provenance As SpreadsheetTransportFrame = Nothing) As MixedModelWorkflowData
            Dim issues As New List(Of RegressionDiagnostic)
            ValidateSubjects(subjects, issues)
            If data Is Nothing OrElse rows Is Nothing OrElse rows.Length <> data.Nobs Then
                MixedModelWorkflowPolicy.Issue(issues, "rows.shape", "Core blocks and retained RowIds must agree.")
            Else
                If subjects Is Nothing OrElse subjects.Length <> data.Nobs Then MixedModelWorkflowPolicy.Issue(issues, "subject.shape", "Subject sidecar and Core blocks must align.")
                If rows.Distinct().Count() <> rows.Length Then MixedModelWorkflowPolicy.Issue(issues, "rows.duplicate", "Source RowIds must be distinct.")
                If Not data.Blocks.SelectMany(Function(b) b.RowIndices).OrderBy(Function(id) id).SequenceEqual(rows.OrderBy(Function(id) id)) Then MixedModelWorkflowPolicy.Issue(issues, "rows.mapping", "Core block rows must equal retained source rows.")
            End If
            Return New MixedModelWorkflowData(data, rows, subjects, issues, provenance)
        End Function
        Private Shared Sub ValidateSubjects(subjects() As Object, issues As List(Of RegressionDiagnostic))
            If subjects Is Nothing Then
                MixedModelWorkflowPolicy.Issue(issues, "subject.required", "Subject identities are required.") : Return
            End If
            Dim identities As New Dictionary(Of String, Object)(StringComparer.Ordinal)
            For Each value In subjects
                If value Is Nothing OrElse Type.GetTypeCode(value.GetType()) = TypeCode.Object OrElse String.IsNullOrWhiteSpace(Convert.ToString(value, CultureInfo.InvariantCulture)) Then
                    MixedModelWorkflowPolicy.Issue(issues, "subject.missing", "Subjects must be nonmissing immutable scalar labels.")
                ElseIf (TypeOf value Is Double AndAlso Not MixedModelWorkflowPolicy.Finite(DirectCast(value, Double))) OrElse
                       (TypeOf value Is Single AndAlso (Single.IsNaN(DirectCast(value, Single)) OrElse Single.IsInfinity(DirectCast(value, Single)))) Then
                    MixedModelWorkflowPolicy.Issue(issues, "subject.missing", "Numeric subject identities must be finite.")
                Else
                    Dim key = Convert.ToString(value, CultureInfo.InvariantCulture)
                    If identities.ContainsKey(key) AndAlso Not Object.Equals(identities(key), value) Then MixedModelWorkflowPolicy.Issue(issues, "subject.ambiguous", "Distinct typed subjects collapse to the same Core text identity.")
                    If Not identities.ContainsKey(key) Then identities.Add(key, value)
                End If
                If issues.Count >= MixedModelWorkflowPolicy.MaximumDiagnostics Then Exit For
            Next
        End Sub
    End Class

    Public NotInheritable Class MixedModelWorkflowRequest
        Private ReadOnly _options As MixedModelWorkflowOptions
        Private ReadOnly _fixedNames, _randomNames As String()
        Public Sub New(kind As MixedModelWorkflowKind, data As MixedModelWorkflowData, responseKey As String, subjectKey As String, visitKey As String,
                       fixedNames() As String, randomNames() As String, fixedEffects As RegressionEffectModel, randomEffects As RegressionEffectModel,
                       fixedText As String, randomText As String, options As MixedModelWorkflowOptions,
                       Optional sourceResponseKey As String = Nothing, Optional sourceSubjectKey As String = Nothing, Optional sourceVisitKey As String = Nothing)
            Me.Kind = kind : Me.Data = data : Me.ResponseKey = responseKey : Me.SubjectKey = subjectKey : Me.VisitKey = visitKey
            Me.SourceResponseKey = If(sourceResponseKey, responseKey) : Me.SourceSubjectKey = If(sourceSubjectKey, subjectKey)
            Me.SourceVisitKey = If(sourceVisitKey, visitKey)
            _fixedNames = If(fixedNames Is Nothing, Nothing, DirectCast(fixedNames.Clone(), String()))
            _randomNames = If(randomNames Is Nothing, Nothing, DirectCast(randomNames.Clone(), String()))
            Me.FixedEffects = fixedEffects : Me.RandomEffects = randomEffects : Me.FixedText = fixedText : Me.RandomText = randomText
            _options = If(options, MixedModelWorkflowOptions.Defaults(kind)).Copy()
            Fitting = New MixedModelFittingOptions(kind, _options)
            Decision = Fitting.Decision
            Validation = MixedModelWorkflowPolicy.Validate(Me)
        End Sub
        Public ReadOnly Property Kind As MixedModelWorkflowKind
        Public ReadOnly Property Data As MixedModelWorkflowData
        ' Preserve original authoring/selection IDs separately from legacy Core captions.
        Public ReadOnly Property SourceResponseKey As String
        Public ReadOnly Property SourceSubjectKey As String
        Public ReadOnly Property SourceVisitKey As String
        Public ReadOnly Property ResponseKey As String
        Public ReadOnly Property SubjectKey As String
        Public ReadOnly Property VisitKey As String
        Public ReadOnly Property FixedEffects As RegressionEffectModel
        Public ReadOnly Property RandomEffects As RegressionEffectModel
        Public ReadOnly Property FixedText As String
        Public ReadOnly Property RandomText As String
        Public ReadOnly Property Decision As MixedModelFitDecision
        Public ReadOnly Property Fitting As MixedModelFittingOptions
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Options As MixedModelWorkflowOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property FixedNames As String()
            Get
                Return If(_fixedNames Is Nothing, Nothing, DirectCast(_fixedNames.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property RandomNames As String()
            Get
                Return If(_randomNames Is Nothing, Nothing, DirectCast(_randomNames.Clone(), String()))
            End Get
        End Property
        Public Function CreateCoreRequest() As MixedModelFitRequest
            If Not Validation.IsValid Then Throw New ArgumentException("mixed.request.invalid: Resolve portable request diagnostics.")
            Return CreateAcquiredCoreRequest()
        End Function
        Public Function CreateAcquiredCoreRequest(Optional residual As MixedModelRStruct = Nothing, Optional random As MixedModelGStruct = Nothing) As MixedModelFitRequest
            residual = If(residual, Fitting.Residual.CreateResidualStructure())
            Dim req = If(Kind = MixedModelWorkflowKind.MMRM,
                MixedModelFitRequest.CreateMMRM(Data.CoreData, residual, Decision.EffectiveMethod),
                MixedModelFitRequest.CreateLMM(Data.CoreData, residual, If(random, Fitting.Random.CreateRandomStructure()), Decision.EffectiveMethod))
            req.ResponseVarName = ResponseKey : req.SubjectVarName = SubjectKey : req.VisitVarName = If(VisitKey, String.Empty)
            req.FixedEffectNames = FixedNames : req.RandomEffectNames = RandomNames
            req.FixedFormulaText = FixedText : req.RandomFormulaText = If(Kind = MixedModelWorkflowKind.MMRM, String.Empty, RandomText)
            req.RequestLabel = Kind.ToString() : req.FixedInferenceMethod = Decision.EffectiveInference
            If Decision.EffectiveInference = MixedModelFixedInferenceMethod.Satterthwaite Then req.UseSatterthwaite = True
            If Decision.EffectiveInference = MixedModelFixedInferenceMethod.KenwardRoger Then
                If Kind = MixedModelWorkflowKind.MMRM Then req.EnableFullKenwardRogerForMmrm() Else req.EnableFullKenwardRogerForLmm()
            End If
            Dim ctl = req.Control
            Fitting.ApplyControl(ctl)
            req.Control = ctl ' MixedModelControl is a value type: assigning back is essential.
            Return req
        End Function
    End Class

    Public NotInheritable Class MixedModelWorkflowPolicy
        Private Sub New()
        End Sub
        Public Const MaximumDiagnostics As Integer = 128
        Public Shared Function LegacyRejectEpsilon(value As Double) As Boolean
            Return value <= 0 ' Preserve existing GUI NaN comparison; finite checks remain candidate diagnostics.
        End Function
        Public Shared Function LegacyRejectIterations(value As Integer) As Boolean
            Return value <= 0
        End Function
        Public Shared Function EmptyEffectSelection(count As Integer, intercept As Boolean) As Boolean
            Return count = 0 AndAlso Not intercept
        End Function
        Friend Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
        Friend Shared Sub Issue(issues As List(Of RegressionDiagnostic), field As String, message As String, Optional blocking As Boolean = True)
            If issues.Count < MaximumDiagnostics Then issues.Add(New RegressionDiagnostic("mixed." & field, field, message, blocking:=blocking))
        End Sub
        Public Shared Function DecideFit(kind As MixedModelWorkflowKind, fit As String, inference As String) As MixedModelFitDecision
            Return New MixedModelFitDecision(kind, fit, inference)
        End Function
        Public Shared Sub ApplyOptimizer(kind As MixedModelWorkflowKind, ByRef control As MixedModelControl, optimizer As String, gradient As String)
            MixedModelFittingOptions.ProjectOptimizer(kind, control, optimizer, gradient)
        End Sub
        Public Shared Function PromotedRandomStructure(current As String, effects As RegressionEffectModel, generalOperation As Boolean, Optional authoredCount As Nullable(Of Integer) = Nothing) As String
            Dim count = If(authoredCount.HasValue, authoredCount.Value, If(effects Is Nothing, 0, effects.Effects.Count)), intercept = effects IsNot Nothing AndAlso effects.IncludeIntercept
            Dim general = generalOperation OrElse (Not intercept AndAlso count > 0) OrElse count + If(intercept, 1, 0) > 2
            If effects IsNot Nothing Then general = general OrElse effects.Effects.Any(Function(e) e.Spec IsNot Nothing AndAlso
                (e.Spec.Scale = PredictorScale.Categorical OrElse Not String.Equals(e.Spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) OrElse e.Spec.Degree > 1))
            Dim simple = String.Equals(current, "Random Intercept", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(current, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase)
            If general AndAlso simple Then Return "Variance Components (VC/Diag)"
            If count = 1 AndAlso intercept AndAlso String.Equals(current, "Random Intercept", StringComparison.OrdinalIgnoreCase) Then Return "Random Intercept + Slope"
            Return current
        End Function
        Public Shared Function Validate(request As MixedModelWorkflowRequest) As RegressionValidation
            Dim issues As New List(Of RegressionDiagnostic)
            If request Is Nothing Then
                Issue(issues, "request.required", "A mixed-model request is required.") : Return New RegressionValidation(issues, 0, 0, {})
            End If
            Dim o = request.Options, d = If(request.Data Is Nothing, Nothing, request.Data.CoreData)
            If request.Data IsNot Nothing Then issues.AddRange(request.Data.Diagnostics)
            If Not [Enum].IsDefined(GetType(MixedModelWorkflowKind), request.Kind) Then Issue(issues, "kind", "Unknown mixed-model kind.")
            If d Is Nothing Then Issue(issues, "data.required", "Usable subject-block data are required.")
            If String.IsNullOrWhiteSpace(request.ResponseKey) OrElse String.IsNullOrWhiteSpace(request.SubjectKey) Then Issue(issues, "roles", "Response and subject keys are required.")
            If request.Kind = MixedModelWorkflowKind.MMRM AndAlso String.IsNullOrWhiteSpace(request.VisitKey) Then Issue(issues, "visit.required", "MMRM requires a named visit role.")
            If o.MaximumIterations <= 0 Then Issue(issues, "iterations", "Maximum iterations must be positive.")
            If Not Finite(o.Epsilon) OrElse o.Epsilon <= 0 Then Issue(issues, "epsilon", "All three convergence tolerances require a finite positive epsilon.")
            If request.Decision.InferenceFallback Then Issue(issues, "inference.fallback", "Unrecognized GUI inference retains the model-specific legacy fallback.", False)
            If request.Decision.CoerceToReml Then Issue(issues, "fit.reml", "The existing KR guard changes ML to REML; Windows owns the information message.", False)
            If Not o.FixedIntercept AndAlso (request.FixedEffects Is Nothing OrElse request.FixedEffects.Effects.Count = 0) Then Issue(issues, "fixed.empty", "No fixed effects and fixed intercept is disabled.")
            Dim residual = request.Fitting.Residual
            If Not residual.IsSupported Then
                Issue(issues, "residual.structure", "Unsupported R-side covariance selection.")
            ElseIf residual.UsesVisitIndex AndAlso (d Is Nothing OrElse Not d.HasVisit OrElse String.IsNullOrWhiteSpace(request.VisitKey)) Then
                Issue(issues, "visit.required", "The selected R structure requires visit indices.")
            End If
            If d IsNot Nothing Then
                If request.FixedNames IsNot Nothing AndAlso (request.FixedNames.Any(Function(n) String.IsNullOrWhiteSpace(n)) OrElse request.FixedNames.Distinct(StringComparer.Ordinal).Count() <> request.FixedNames.Length) Then Issue(issues, "fixed.names", "Expanded fixed names must be nonblank and unambiguous.")
                If request.FixedNames Is Nothing OrElse request.FixedNames.Length <> d.P Then Issue(issues, "fixed.names", "Expanded fixed names must match X columns.")
                If request.Kind = MixedModelWorkflowKind.LMM Then
                    If request.RandomNames IsNot Nothing AndAlso (request.RandomNames.Any(Function(n) String.IsNullOrWhiteSpace(n)) OrElse request.RandomNames.Distinct(StringComparer.Ordinal).Count() <> request.RandomNames.Length) Then Issue(issues, "random.names", "Expanded random names must be nonblank and unambiguous.")
                    If d.Q <= 0 OrElse request.RandomNames Is Nothing OrElse request.RandomNames.Length <> d.Q Then Issue(issues, "random.names", "Expanded random names must match Z columns.")
                    Dim authored = If(request.RandomEffects Is Nothing, 0, request.RandomEffects.Effects.Count)
                    If Not o.RandomIntercept AndAlso authored = 0 Then Issue(issues, "random.empty", "No random effects and random intercept is disabled.")
                    If String.Equals(o.RandomStructure, "Random Intercept", StringComparison.OrdinalIgnoreCase) AndAlso (d.Q <> 1 OrElse Not o.RandomIntercept OrElse authored <> 0) Then Issue(issues, "random.intercept", "Random Intercept requires intercept-only authoring and one expanded column.")
                    If String.Equals(o.RandomStructure, "Random Intercept + Slope", StringComparison.OrdinalIgnoreCase) AndAlso (d.Q <> 2 OrElse Not o.RandomIntercept OrElse authored <> 1) Then Issue(issues, "random.slope", "Random Intercept + Slope requires intercept and exactly one authored effect/two expanded columns.")
                    Dim random = request.Fitting.Random
                    If Not random.IsSupported Then
                        Issue(issues, "random.structure", "Unsupported G-side covariance selection.")
                    ElseIf random.IsZeroRandom Then
                        Issue(issues, "random.structure", "LMM requires a G-side random structure.")
                    End If
                ElseIf d.Q <> 0 Then
                    Issue(issues, "random.mmrm", "MMRM has no random design.")
                End If
            End If
            issues.AddRange(request.Fitting.DesignDiagnostics(d))
            Return New RegressionValidation(issues.Take(MaximumDiagnostics), If(d Is Nothing, 0, d.P), If(d Is Nothing, 0, d.Q), {})
        End Function
    End Class
End Namespace
