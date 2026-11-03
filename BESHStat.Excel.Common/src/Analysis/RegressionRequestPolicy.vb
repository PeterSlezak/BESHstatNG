Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum RegressionAnalysisKind
        LinearModel
        GeneralizedLinearModel
        NegativeBinomial2
        ZeroInflatedPoisson
        Ordinal
        Multinomial
    End Enum

    Public Enum RegressionFamily
        Binomial
        Poisson
        NegativeBinomial
        Gaussian
        Gamma
    End Enum

    Public Enum RegressionLink
        Identity
        Log
        Logit
        Probit
        Sqrt
        Inverse
        Power
    End Enum

    ''' <summary>Caller-owned options builder. Requests retain and return independent copies.
    ''' Nothing is defaulted only when absent; explicitly invalid values remain diagnostic inputs.</summary>
    Public NotInheritable Class RegressionOptions
        Private _provided As New HashSet(Of String)(StringComparer.Ordinal)
        Private _alpha As Double = 0.05
        Private _epsilon As Double = 0.000001
        Private _maximumIterations As Integer = 100
        Private _emIterations As Integer = 100
        Private _threshold As Double = 0.5
        Public Property Family As Nullable(Of RegressionFamily)
        Public Property Link As Nullable(Of RegressionLink)
        Public Property Power As Nullable(Of Double)
        Public Property Dispersion As Nullable(Of Double)
        Public Property Alpha As Double
            Get
                Return _alpha
            End Get
            Set(value As Double)
                _alpha = value : _provided.Add("alpha")
            End Set
        End Property
        Public Property Epsilon As Double
            Get
                Return _epsilon
            End Get
            Set(value As Double)
                _epsilon = value : _provided.Add("epsilon")
            End Set
        End Property
        Public Property MaximumIterations As Integer
            Get
                Return _maximumIterations
            End Get
            Set(value As Integer)
                _maximumIterations = value : _provided.Add("maximumIterations")
            End Set
        End Property
        Public Property EmIterations As Integer
            Get
                Return _emIterations
            End Get
            Set(value As Integer)
                _emIterations = value : _provided.Add("emIterations")
            End Set
        End Property
        Public Property IterationDetails As Boolean
        Public Property Covariance As Boolean
        Public Property Residuals As Boolean
        Public Property Reference As ReferenceCategory = ReferenceCategory.Last
        Public Property Classification As Boolean
        Public Property Threshold As Double
            Get
                Return _threshold
            End Get
            Set(value As Double)
                _threshold = value : _provided.Add("threshold")
            End Set
        End Property
        Public Property ThresholdTable As Boolean
        Public Property Calibration As Boolean
        Public Property CalibrationBins As Integer = 10
        Public Property Brier As Boolean
        Public Property PValuePresentation As OutputPValuePolicy = New OutputPValuePolicy(Nothing)

        Public Function Copy() As RegressionOptions
            Dim result = DirectCast(MemberwiseClone(), RegressionOptions)
            result._provided = New HashSet(Of String)(_provided, StringComparer.Ordinal)
            Return result
        End Function
        Public Function WasSupplied(field As String) As Boolean
            Return _provided.Contains(field)
        End Function
    End Class

    Public NotInheritable Class RegressionDiagnostic
        Public Sub New(code As String, fieldPath As String, message As String,
                       Optional sourceRowId As Nullable(Of Integer) = Nothing,
                       Optional columnKey As String = Nothing, Optional blocking As Boolean = True)
            Me.Code = code : Me.FieldPath = fieldPath : Me.Message = message
            Me.SourceRowId = sourceRowId : Me.ColumnKey = columnKey : Me.Blocking = blocking
        End Sub
        Public ReadOnly Property Code As String
        Public ReadOnly Property FieldPath As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property SourceRowId As Nullable(Of Integer)
        Public ReadOnly Property ColumnKey As String
        Public ReadOnly Property Blocking As Boolean
    End Class

    ''' <summary>Already expanded by RegressionDesignCore. Never expands formulas or holds a DataObj.
    ''' Null initial values mean not supplied; an empty supplied array stays empty and fails validation.</summary>
    Public NotInheritable Class RegressionComponent
        Private ReadOnly _data As Double(,)
        Private ReadOnly _names, _sourceKeys As String()
        Private ReadOnly _rows As Integer()
        Private ReadOnly _initial As Double()
        Public Sub New(data(,) As Double, names() As String, rows() As Integer, effects As RegressionEffectModel,
                       sourceKeys() As String, Optional initialValues() As Double = Nothing,
                       Optional responseSources As IEnumerable(Of SpreadsheetTransportCoordinate) = Nothing)
            _data = If(data Is Nothing, Nothing, DirectCast(data.Clone(), Double(,)))
            _names = If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
            _rows = If(rows Is Nothing, Nothing, DirectCast(rows.Clone(), Integer()))
            _sourceKeys = If(sourceKeys Is Nothing, Nothing, DirectCast(sourceKeys.Clone(), String()))
            _initial = If(initialValues Is Nothing, Nothing, DirectCast(initialValues.Clone(), Double()))
            Me.Effects = effects
            Me.ResponseSources = If(responseSources Is Nothing, Nothing, New List(Of SpreadsheetTransportCoordinate)(responseSources).AsReadOnly())
        End Sub
        Public ReadOnly Property Effects As RegressionEffectModel
        Public ReadOnly Property ResponseSources As IReadOnlyList(Of SpreadsheetTransportCoordinate)
        Public ReadOnly Property Data As Double(,)
            Get
                Return If(_data Is Nothing, Nothing, DirectCast(_data.Clone(), Double(,)))
            End Get
        End Property
        Public ReadOnly Property Names As String()
            Get
                Return If(_names Is Nothing, Nothing, DirectCast(_names.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property RowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
        Public ReadOnly Property SourceKeys As String()
            Get
                Return If(_sourceKeys Is Nothing, Nothing, DirectCast(_sourceKeys.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property InitialValues As Double()
            Get
                Return If(_initial Is Nothing, Nothing, DirectCast(_initial.Clone(), Double()))
            End Get
        End Property
    End Class

    ''' <summary>Named imported vector with exact source identity, before any Core fitted-order projection.</summary>
    Public NotInheritable Class RegressionVector
        Private ReadOnly _values As Double()
        Private ReadOnly _rows As Integer()
        Public Sub New(key As String, values() As Double, rows() As Integer)
            Me.Key = key
            _values = If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
            _rows = If(rows Is Nothing, Nothing, DirectCast(rows.Clone(), Integer()))
        End Sub
        Public ReadOnly Property Key As String
        Public ReadOnly Property Values As Double()
            Get
                Return If(_values Is Nothing, Nothing, DirectCast(_values.Clone(), Double()))
            End Get
        End Property
        Public ReadOnly Property RowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
    End Class

    ''' <summary>Immutable Common-owned snapshot. Host owns acquisition, culture parsing, cancellation
    ''' and fitting lifetime. No callbacks or workbook snapshot are retained.</summary>
    Public NotInheritable Class RegressionRequest
        Private ReadOnly _options As RegressionOptions
        Public Sub New(kind As RegressionAnalysisKind, responseKey As String, primary As RegressionComponent,
                       Optional options As RegressionOptions = Nothing, Optional offset As RegressionVector = Nothing,
                       Optional weights As RegressionVector = Nothing, Optional logistic As RegressionComponent = Nothing)
            Me.Kind = kind : Me.ResponseKey = responseKey : Me.Primary = primary : Me.Logistic = logistic
            Me.Offset = offset : Me.Weights = weights : Me.OptionsDefaulted = options Is Nothing
            _options = If(options, New RegressionOptions()).Copy()
        End Sub
        Public ReadOnly Property Kind As RegressionAnalysisKind
        Public ReadOnly Property ResponseKey As String
        Public ReadOnly Property Primary As RegressionComponent
        Public ReadOnly Property Logistic As RegressionComponent
        Public ReadOnly Property Offset As RegressionVector
        Public ReadOnly Property Weights As RegressionVector
        Public ReadOnly Property OptionsDefaulted As Boolean
        Public ReadOnly Property Options As RegressionOptions
            Get
                Return _options.Copy()
            End Get
        End Property
    End Class

    Public NotInheritable Class RegressionValidation
        Public Sub New(diagnostics As IEnumerable(Of RegressionDiagnostic), primaryCount As Integer, logisticCount As Integer,
                       observedClasses As IEnumerable(Of Integer))
            Me.Diagnostics = New List(Of RegressionDiagnostic)(diagnostics).AsReadOnly()
            Me.PrimaryParameterCount = primaryCount : Me.LogisticParameterCount = logisticCount
            Me.ObservedClasses = New List(Of Integer)(observedClasses).AsReadOnly()
        End Sub
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionDiagnostic)
        Public ReadOnly Property PrimaryParameterCount As Integer
        Public ReadOnly Property LogisticParameterCount As Integer
        ''' <summary>First-observed numeric Core category identity; never sorted display strings.</summary>
        Public ReadOnly Property ObservedClasses As IReadOnlyList(Of Integer)
        Public ReadOnly Property IsValid As Boolean
            Get
                Return Not Diagnostics.Any(Function(d) d.Blocking)
            End Get
        End Property
    End Class

    Public NotInheritable Class RegressionRequestPolicy
        Private Sub New()
        End Sub
        Public Shared Function Validate(request As RegressionRequest) As RegressionValidation
            Dim issues As New List(Of RegressionDiagnostic), classes As New List(Of Integer)
            If request Is Nothing Then
                issues.Add(New RegressionDiagnostic("regression.request.required", "request", "Regression request is required."))
                Return New RegressionValidation(issues, 0, 0, classes)
            End If
            Dim o = request.Options
            Dim defaults As New List(Of String) From {"alpha"}
            If request.Kind <> RegressionAnalysisKind.LinearModel Then defaults.AddRange({"epsilon", "maximumIterations"})
            If request.Kind = RegressionAnalysisKind.ZeroInflatedPoisson Then defaults.Add("emIterations")
            If o.Classification Then defaults.Add("threshold")
            For Each field In defaults
                If Not o.WasSupplied(field) Then issues.Add(New RegressionDiagnostic("regression.options." & field & ".defaulted", "options." & field, "Default " & field & " was used.", blocking:=False))
            Next
            If Not [Enum].IsDefined(GetType(RegressionAnalysisKind), request.Kind) Then Add(issues, "kind", "Unknown regression family.")
            If String.IsNullOrWhiteSpace(request.ResponseKey) Then Add(issues, "response", "Dependent variable is missing.")
            If request.OptionsDefaulted Then issues.Add(New RegressionDiagnostic("regression.options.defaulted", "options", "Default regression options were used.", blocking:=False))
            If Not Finite(o.Alpha) OrElse o.Alpha <= 0 OrElse o.Alpha >= 1 Then Add(issues, "options.alpha", "Alpha must be finite and between 0 and 1.")
            If Not Finite(o.Epsilon) OrElse o.Epsilon <= 0 Then Add(issues, "options.epsilon", "Convergence epsilon must be positive and finite.")
            If o.MaximumIterations <= 0 Then Add(issues, "options.maximumIterations", "Maximum iterations must be positive.")
            If request.Kind = RegressionAnalysisKind.ZeroInflatedPoisson AndAlso o.EmIterations <= 0 Then Add(issues, "options.emIterations", "EM iterations must be positive.")
            If Not [Enum].IsDefined(GetType(ReferenceCategory), o.Reference) Then Add(issues, "options.reference", "Invalid reference category.")
            If o.PValuePresentation Is Nothing Then Add(issues, "options.pValuePresentation", "P-value presentation metadata is required.")
            Dim family As Family = Nothing
            If request.Kind = RegressionAnalysisKind.GeneralizedLinearModel OrElse request.Kind = RegressionAnalysisKind.NegativeBinomial2 Then
                If request.Kind = RegressionAnalysisKind.GeneralizedLinearModel AndAlso (Not o.Family.HasValue OrElse Not [Enum].IsDefined(GetType(RegressionFamily), o.Family.Value)) Then
                    Add(issues, "options.family", "A GLM family is required.")
                Else
                    family = FamilyUtils.createFamily(If(request.Kind = RegressionAnalysisKind.NegativeBinomial2, "NegativeBinomial", o.Family.Value.ToString()))
                End If
                If request.Kind = RegressionAnalysisKind.NegativeBinomial2 AndAlso o.Family.HasValue AndAlso o.Family.Value <> RegressionFamily.NegativeBinomial Then Add(issues, "options.family", "NB2 uses the Core negative-binomial family.")
                If Not o.Link.HasValue OrElse Not [Enum].IsDefined(GetType(RegressionLink), o.Link.Value) Then
                    Add(issues, "options.link", "A supported link is required.")
                ElseIf family IsNot Nothing AndAlso Not family.testLink(o.Link.Value.ToString()) Then
                    Add(issues, "options.link", "Link is not supported for the selected family.")
                End If
                If o.Link = RegressionLink.Power AndAlso (Not o.Power.HasValue OrElse Not Finite(o.Power.Value)) Then Add(issues, "options.power", "Power link parameter must be finite.")
            ElseIf o.Family.HasValue OrElse o.Link.HasValue Then
                Add(issues, "options.family", "Family/link options apply only to GLM and NB2.")
            End If
            If o.Dispersion.HasValue AndAlso (Not Finite(o.Dispersion.Value) OrElse o.Dispersion.Value <= 0 OrElse request.Kind <> RegressionAnalysisKind.GeneralizedLinearModel OrElse o.Family <> RegressionFamily.NegativeBinomial) Then Add(issues, "options.dispersion", "Dispersion applies to a negative-binomial GLM and must be positive and finite; NB2 estimates dispersion in Core.")
            If o.Power.HasValue AndAlso o.Link <> RegressionLink.Power Then Add(issues, "options.power", "Power parameter requires the power link.")
            If o.Classification AndAlso (request.Kind <> RegressionAnalysisKind.GeneralizedLinearModel OrElse o.Family <> RegressionFamily.Binomial) Then Add(issues, "options.classification", "Classification requires a binomial GLM.")
            If o.Classification Then
                If Not Finite(o.Threshold) OrElse o.Threshold < 0 OrElse o.Threshold > 1 Then Add(issues, "options.threshold", "Classification threshold must be between 0 and 1.")
                If o.Calibration AndAlso o.CalibrationBins < 2 Then Add(issues, "options.calibrationBins", "Calibration bin count must be at least 2 under the Core contract.")
            End If
            Dim p = ValidateComponent(request.Primary, "primary", request.ResponseKey, issues, classes, family, request.Kind)
            Dim q = 0
            If request.Primary IsNot Nothing Then
                ValidateVector(request.Offset, request.Primary.RowIds, "offset", False, request.Kind, issues)
                ValidateVector(request.Weights, request.Primary.RowIds, "weights", True, request.Kind, issues)
            End If
            If request.Offset IsNot Nothing AndAlso request.Weights IsNot Nothing AndAlso request.Offset.Key = request.Weights.Key Then issues.Add(New RegressionDiagnostic("regression.roles.shared-key", "roles", "Offset and weights use the same source key; legacy role selection is retained.", blocking:=False))
            If request.Kind = RegressionAnalysisKind.LinearModel AndAlso request.Offset IsNot Nothing Then Add(issues, "offset", "LM does not consume an offset.")
            If request.Kind = RegressionAnalysisKind.ZeroInflatedPoisson Then
                Dim unused As New List(Of Integer)
                q = ValidateComponent(request.Logistic, "logistic", request.ResponseKey, issues, unused, Nothing, request.Kind)
                If request.Weights IsNot Nothing Then Add(issues, "weights", "The shipping ZIP model does not consume observation weights.")
                If request.Primary IsNot Nothing AndAlso request.Logistic IsNot Nothing Then
                    Dim a = request.Primary.RowIds, b = request.Logistic.RowIds
                    If a Is Nothing OrElse b Is Nothing OrElse Not a.SequenceEqual(b) Then Add(issues, "logistic.rowIds", "ZIP component row identities and ordering must match.")
                    Dim x = request.Primary.Data, z = request.Logistic.Data
                    If x IsNot Nothing AndAlso z IsNot Nothing AndAlso x.GetLength(1) > 0 AndAlso z.GetLength(1) > 0 AndAlso x.GetLength(0) = z.GetLength(0) Then
                        For i = 0 To x.GetLength(0) - 1
                            If x(i, 0) <> z(i, 0) Then Add(issues, "logistic.response", "ZIP components must share the same response.") : Exit For
                        Next
                    End If
                End If
            ElseIf request.Logistic IsNot Nothing Then
                Add(issues, "logistic", "Separate logistic effects apply only to ZIP.")
            End If
            If request.Kind = RegressionAnalysisKind.Ordinal Then p += Math.Max(0, classes.Count - 1)
            If request.Kind = RegressionAnalysisKind.Multinomial Then p *= Math.Max(0, classes.Count - 1)
            If (request.Kind = RegressionAnalysisKind.Ordinal OrElse request.Kind = RegressionAnalysisKind.Multinomial) AndAlso classes.Count < 2 Then Add(issues, "primary.response.categories", "At least two observed response categories are required.")
            ValidateInitial(request.Primary, p, "primary.initialValues", issues)
            ValidateInitial(request.Logistic, q, "logistic.initialValues", issues)
            Return New RegressionValidation(issues, p, q, classes)
        End Function

        Private Shared Function ValidateComponent(c As RegressionComponent, path As String, response As String,
                issues As List(Of RegressionDiagnostic), classes As List(Of Integer), family As Family, kind As RegressionAnalysisKind) As Integer
            If c Is Nothing Then
                Add(issues, path, "Expanded model component is required.") : Return 0
            End If
            Dim data = c.Data, names = c.Names, rows = c.RowIds, keys = c.SourceKeys
            If data Is Nothing OrElse data.GetLength(0) = 0 OrElse data.GetLength(1) = 0 Then
                Add(issues, path & ".data", "Expanded data must contain a response and rows.") : Return 0
            End If
            If data.GetLowerBound(0) <> 0 OrElse data.GetLowerBound(1) <> 0 Then
                Add(issues, path & ".data.bounds", "Expanded arrays must be zero based.") : Return 0
            End If
            If kind = RegressionAnalysisKind.LinearModel AndAlso data.GetLength(0) < 2 Then Add(issues, path & ".data", "LM requires at least two rows under the existing Core contract.")
            If names Is Nothing OrElse names.Length <> data.GetLength(1) OrElse names.Length = 0 OrElse (names(0) <> response AndAlso names(0) <> RegressionDesignCore.GetCoefBaseName(response)) Then Add(issues, path & ".names", "Expanded names must match the matrix with response first.")
            If rows Is Nothing OrElse rows.Length <> data.GetLength(0) OrElse rows.Distinct().Count() <> rows.Length Then Add(issues, path & ".rowIds", "Source RowIds must be present, unique and aligned with the expanded matrix.")
            If c.ResponseSources IsNot Nothing AndAlso (rows Is Nothing OrElse c.ResponseSources.Count <> rows.Length OrElse c.ResponseSources.Where(Function(s, i) s Is Nothing OrElse s.WorksheetRow <> rows(i)).Any()) Then Add(issues, path & ".provenance", "Response transport provenance does not match selected source rows.")
            If keys Is Nothing OrElse keys.Any(Function(k) String.IsNullOrWhiteSpace(k)) OrElse keys.Distinct(StringComparer.Ordinal).Count() <> keys.Length Then Add(issues, path & ".sourceKeys", "Source keys must be present and unambiguous.")
            If c.Effects Is Nothing Then
                Add(issues, path & ".effects", "Typed effects snapshot is required.")
            Else
                If c.Effects.Effects.Count = 0 AndAlso Not c.Effects.IncludeIntercept Then Add(issues, path & ".effects", "No intercept and effects were specified.")
                Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
                For Each e In c.Effects.Effects
                    If Not seen.Add(e.Key) Then Add(issues, path & ".effects", "Duplicate effect key.")
                    Dim spec = e.Spec
                    Dim bases = If(spec Is Nothing, New List(Of String) From {e.Key}, spec.BaseVarKeys)
                    If bases Is Nothing OrElse bases.Count = 0 OrElse bases.Any(Function(k) keys Is Nothing OrElse Not keys.Contains(k, StringComparer.Ordinal)) Then Add(issues, path & ".effects.sourceKeys", "Effect source key is missing or renamed.")
                Next
                For Each diagnostic In RegressionEffectAuthoringPolicy.Inspect(c.Effects, keys)
                    If diagnostic.Code = "term-metadata" Then
                        ' Core supports a plain raw key without optional TermSpec metadata.
                        issues.Add(New RegressionDiagnostic("regression.effects.legacy-metadata", path & ".effects", "Plain source-key effect uses the existing Core metadata fallback.", columnKey:=diagnostic.RawKey, blocking:=False))
                    Else
                        issues.Add(New RegressionDiagnostic("regression." & path & ".effects." & diagnostic.Code, path & ".effects." & diagnostic.Code, diagnostic.Message, columnKey:=diagnostic.RawKey))
                    End If
                Next
            End If
            For i = 0 To data.GetLength(0) - 1
                For j = 0 To data.GetLength(1) - 1
                    If Not Finite(data(i, j)) Then issues.Add(New RegressionDiagnostic("regression.numeric.nonfinite", path & ".data", "Expanded data must be finite; rows are never silently dropped.", If(rows IsNot Nothing AndAlso i < rows.Length, CType(rows(i), Integer?), Nothing), If(names IsNot Nothing AndAlso j < names.Length, names(j), Nothing)))
                Next
                Dim y = data(i, 0)
                If family IsNot Nothing AndAlso Finite(y) AndAlso Not family.validata(y) Then Add(issues, path & ".response", "Response is outside the existing Core family domain.")
                If kind = RegressionAnalysisKind.ZeroInflatedPoisson AndAlso y < 0 Then Add(issues, path & ".response", "ZIP response must be nonnegative.")
                If kind = RegressionAnalysisKind.Ordinal OrElse kind = RegressionAnalysisKind.Multinomial Then
                    If Finite(y) AndAlso Math.Round(y) >= Integer.MinValue AndAlso Math.Round(y) <= Integer.MaxValue Then
                        Dim category = Convert.ToInt32(Math.Round(y)) ' Existing Core coding; do not introduce new coercion.
                        If Not classes.Contains(category) Then classes.Add(category)
                    Else
                        Add(issues, path & ".response.categories", "Category cannot be represented by the existing Core integer coding.")
                    End If
                End If
            Next
            Return data.GetLength(1) - 1 + If(c.Effects IsNot Nothing AndAlso c.Effects.IncludeIntercept AndAlso kind <> RegressionAnalysisKind.Ordinal, 1, 0)
        End Function

        Private Shared Sub ValidateInitial(c As RegressionComponent, expected As Integer, path As String, issues As List(Of RegressionDiagnostic))
            If c Is Nothing Then Return
            Dim v = c.InitialValues
            If v Is Nothing Then Return
            If v.Length <> expected Then Add(issues, path & ".count", $"Expected {expected} initial values, received {v.Length}.")
            If v.Any(Function(x) Not Finite(x)) Then Add(issues, path, "Initial values must be finite.")
        End Sub

        Private Shared Sub ValidateVector(v As RegressionVector, rows() As Integer, path As String, weights As Boolean, kind As RegressionAnalysisKind, issues As List(Of RegressionDiagnostic))
            If v Is Nothing Then Return
            Dim values = v.Values, ids = v.RowIds
            If String.IsNullOrWhiteSpace(v.Key) Then Add(issues, path & ".key", "Imported vector name is required.")
            If rows Is Nothing OrElse values Is Nothing OrElse ids Is Nothing OrElse values.Length <> rows.Length OrElse Not ids.SequenceEqual(rows) Then
                Add(issues, path & ".alignment", "Vector lengths, RowIds and ordering must match the source rows.") : Return
            End If
            For i = 0 To values.Length - 1
                If Not Finite(values(i)) OrElse (weights AndAlso (values(i) < 0 OrElse (kind = RegressionAnalysisKind.LinearModel AndAlso values(i) = 0))) Then issues.Add(New RegressionDiagnostic("regression." & path & ".value", path, If(weights, "Invalid weight under the existing Core contract.", "Offset must be finite."), rows(i), v.Key))
            Next
        End Sub
        Friend Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
        Private Shared Sub Add(issues As List(Of RegressionDiagnostic), path As String, message As String)
            issues.Add(New RegressionDiagnostic("regression." & path, path, message))
        End Sub
    End Class
End Namespace
