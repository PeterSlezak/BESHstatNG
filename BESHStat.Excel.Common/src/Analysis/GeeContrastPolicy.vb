Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Already resolved fitted-design column indices, not a new formula/profile grammar.
    ''' Existing GEE_LSMESTIMATE supplies weighted observed-design profiles and optional time.</summary>
    Public NotInheritable Class GeeContrastComponent
        Public Sub New(label As String, weight As Double, profile As IEnumerable(Of KeyValuePair(Of Integer, Double)),
                       Optional time As Nullable(Of Double) = Nothing)
            Me.Label = label : Me.Weight = weight : Me.Time = time
            Me.Profile = New List(Of KeyValuePair(Of Integer, Double))(If(profile, Enumerable.Empty(Of KeyValuePair(Of Integer, Double))())).AsReadOnly()
        End Sub
        Public ReadOnly Property Label As String
        Public ReadOnly Property Weight As Double
        Public ReadOnly Property Time As Nullable(Of Double)
        Public ReadOnly Property Profile As IReadOnlyList(Of KeyValuePair(Of Integer, Double))
    End Class

    Public NotInheritable Class GeeContrastPlan
        Private ReadOnly _labels, _names As String()
        Private ReadOnly _contrast As Double(,)
        Private ReadOnly _counts As Integer()
        Friend Sub New(labels() As String, names() As String, contrast(,) As Double, counts() As Integer,
                       diagnostics As IEnumerable(Of RegressionDiagnostic))
            _labels = DirectCast(labels.Clone(), String()) : _names = DirectCast(names.Clone(), String())
            _contrast = DirectCast(contrast.Clone(), Double(,)) : _counts = DirectCast(counts.Clone(), Integer())
            Me.Diagnostics = New List(Of RegressionDiagnostic)(diagnostics).AsReadOnly()
        End Sub
        Public ReadOnly Property ShippingGuiPath As String
            Get
                Return "NO_SHIPPING_GUI_CONTRAST_PATH"
            End Get
        End Property
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionDiagnostic)
        Public ReadOnly Property Labels As String()
            Get
                Return DirectCast(_labels.Clone(), String())
            End Get
        End Property
        Public ReadOnly Property ParameterNames As String()
            Get
                Return DirectCast(_names.Clone(), String())
            End Get
        End Property
        Public ReadOnly Property Contrast As Double(,)
            Get
                Return DirectCast(_contrast.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property MatchedCounts As Integer()
            Get
                Return DirectCast(_counts.Clone(), Integer())
            End Get
        End Property
    End Class

    ''' <summary>Portable orchestration of existing UDF observed-profile selection and L rows.
    ''' No Wald inference, inverse link, estimator or public UDF export is implemented here.
    ''' AT/name/range parsing and actual statistical inference remain in the existing UDF.</summary>
    Public NotInheritable Class GeeContrastPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(design(,) As Double, names() As String,
                                       components As IEnumerable(Of GeeContrastComponent),
                                       Optional times() As Double = Nothing) As GeeContrastPlan
            Dim issues As New List(Of RegressionDiagnostic), labels As New List(Of String), counts As New List(Of Integer)
            Dim contrasts As New Dictionary(Of String, Double())(StringComparer.OrdinalIgnoreCase)
            Dim add = Sub(code As String, message As String)
                          issues.Add(New RegressionDiagnostic("gee.contrast." & code, "contrast", message))
                      End Sub
            Dim p = If(design Is Nothing, 0, design.GetLength(1)), parameterNames = If(names, Array.Empty(Of String)())
            If design Is Nothing OrElse design.GetLowerBound(0) <> 0 OrElse design.GetLowerBound(1) <> 0 OrElse parameterNames.Length <> p Then
                add("design", "Usable fitted design and exact parameter-name axis are required.")
            ElseIf components Is Nothing Then
                add("components.missing", "Contrast components were not supplied.")
            Else
                Dim supplied = components.ToList()
                If supplied.Count = 0 Then add("components.empty", "Supplied contrast selection is empty.")
                For Each component In supplied
                    If component Is Nothing OrElse String.IsNullOrWhiteSpace(component.Label) Then
                        add("label", "Each component requires a label.") : Continue For
                    End If
                    If Not RegressionRequestPolicy.Finite(component.Weight) Then add("weight", "Contrast weights must be finite.") : Continue For
                    If Math.Abs(component.Weight) <= 0.0 Then Continue For
                    Dim sums(p - 1) As Double, matched As Integer = 0
                    For i = 0 To design.GetLength(0) - 1
                        If component.Time.HasValue AndAlso (times Is Nothing OrElse times.Length <> design.GetLength(0) OrElse Not NearlyEqual(times(i), component.Time.Value)) Then Continue For
                        Dim match As Boolean = True
                        For Each value In component.Profile
                            If value.Key < 0 OrElse value.Key >= p OrElse Not NearlyEqual(design(i, value.Key), value.Value) Then match = False : Exit For
                        Next
                        If Not match Then Continue For
                        matched += 1
                        For j = 0 To p - 1 : sums(j) += design(i, j) : Next
                    Next
                    counts.Add(matched)
                    If matched <= 0 Then add("unmatched", "No observed design rows matched a supplied profile/time.") : Continue For
                    If Not contrasts.ContainsKey(component.Label) Then
                        contrasts.Add(component.Label, New Double(p - 1) {}) : labels.Add(component.Label)
                    End If
                    For j = 0 To p - 1
                        sums(j) /= CDbl(matched)
                        contrasts(component.Label)(j) += component.Weight * sums(j)
                    Next
                Next
            End If
            If labels.Count = 0 Then add("no-rows", "No non-zero matched contrast rows were produced; no estimate is invented.")
            Dim matrix(labels.Count - 1, p - 1) As Double
            For i = 0 To labels.Count - 1
                For j = 0 To p - 1 : matrix(i, j) = contrasts(labels(i))(j) : Next
            Next
            Return New GeeContrastPlan(labels.ToArray(), parameterNames, matrix, counts.ToArray(), issues)
        End Function
        Private Shared Function NearlyEqual(a As Double, b As Double) As Boolean
            Return Math.Abs(a - b) <= 0.00000001 * Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)))
        End Function
        Public Shared Function Response(tables As IEnumerable(Of ResultTable)) As AnalysisResultDocument
            If tables Is Nothing Then Return AnalysisResultDocument.Failure("regression.gee.contrast", New AnalysisFault("gee.contrast.unavailable", "No existing inference result was supplied."))
            Return New AnalysisResultDocument("regression.gee.contrast", tables.Select(Function(t) t.ToOutputModel()), Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing)
        End Function
    End Class
End Namespace
