Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>Shallow detached context over immutable Core models; never refits or detects signals.</summary>
    Public NotInheritable Class SpcMultivariateResultContext
        Friend Sub New(result As SpcMultivariateFitResult)
            Model = result.Model
            Panels = Array.AsReadOnly(result.Panels)
            Diagnostics = Array.AsReadOnly(result.Diagnostics)
            VariableNames = Array.AsReadOnly(result.Request.VariableNames)
        End Sub
        Public ReadOnly Property Model As SpcMultivariateModelResult
        Public ReadOnly Property Panels As IReadOnlyList(Of SpcPanelResult)
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of SpcMultivariatePointDiagnostic)
        Public ReadOnly Property VariableNames As IReadOnlyList(Of String)
        Public Function SelectDiagnostics(signalOnly As Boolean) As IReadOnlyList(Of SpcMultivariatePointDiagnostic)
            Dim panel = Panels(0)
            Dim selected As New List(Of SpcMultivariatePointDiagnostic)
            For Each diagnostic In Diagnostics
                Dim point = panel.GetPoint(diagnostic.PointIndex)
                If Not signalOnly OrElse (point IsNot Nothing AndAlso point.IsSignalled) Then selected.Add(diagnostic)
            Next
            Return selected.AsReadOnly()
        End Function
    End Class

    Public NotInheritable Class SpcMultivariateResultAxisPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(result As SpcMultivariateFitResult) As SpcMultivariateResultContext
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            Return New SpcMultivariateResultContext(result)
        End Function
        ''' <summary>Existing renderer wrapper input, preserving first source-row fallback only here.</summary>
        Public Shared Function RenderingInput(panel As SpcPanelResult) As SpcInputData
            If panel Is Nothing Then Throw New ArgumentNullException(NameOf(panel))
            Dim points = panel.Points
            Dim values(points.Length - 1) As Double
            Dim labels(points.Length - 1) As String
            Dim sequences(points.Length - 1) As Double
            Dim sourceRows(points.Length - 1) As Integer
            For i As Integer = 0 To points.Length - 1
                values(i) = points(i).Value
                labels(i) = points(i).Label
                sequences(i) = If(points(i).SequenceValue.HasValue, points(i).SequenceValue.Value, Double.NaN)
                Dim rows = points(i).SourceRowIndices
                sourceRows(i) = If(rows.Length > 0, rows(0), points(i).PointIndex)
            Next
            Return SpcInputData.FromIndividualSequence(values, labels, sequences, sourceRows, "Statistic")
        End Function
    End Class
End Namespace
