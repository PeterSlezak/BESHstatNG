Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34.DumbbellSeries
Public NotInheritable Class SeriesPayload34
Private ReadOnly _x As Double(), _y As Double(), _amounts As Double()
Private ReadOnly _labels As String(), _source As Integer()
Public ReadOnly Property Visible As Boolean
Public Sub New(x As Double(), y As Double(), Optional amounts As Double() = Nothing, Optional labels As String() = Nothing, Optional source As Integer() = Nothing, Optional isVisible As Boolean = True)
_x = DirectCast(x.Clone(), Double())
_y = DirectCast(y.Clone(), Double())
_amounts = If(amounts Is Nothing, New Double(){}, DirectCast(amounts.Clone(), Double()))
_labels = If(labels Is Nothing, New String(){}, DirectCast(labels.Clone(), String()))
_source = If(source Is Nothing, New Integer(){}, DirectCast(source.Clone(), Integer()))
Visible = isVisible
End Sub
Public ReadOnly Property XValues As Double()
Get
Return DirectCast(_x.Clone(), Double())
End Get
End Property
Public ReadOnly Property YValues As Double()
Get
Return DirectCast(_y.Clone(), Double())
End Get
End Property
Public ReadOnly Property Amounts As Double()
Get
Return DirectCast(_amounts.Clone(), Double())
End Get
End Property
Public ReadOnly Property Labels As String()
Get
Return DirectCast(_labels.Clone(), String())
End Get
End Property
Public ReadOnly Property SourceOrdinals As Integer()
Get
Return DirectCast(_source.Clone(), Integer())
End Get
End Property
End Class
Public Module DumbbellSeriesPreparation34
Public Function Connectors(categories As DumbbellPlotCategory()) As SeriesPayload34
        Dim n As Integer = categories.Length
        Dim xStarts(n - 1) As Double
        Dim yValues(n - 1) As Double
        Dim connectorLengths(n - 1) As Double
        Dim hasVisibleConnector As Boolean = False

        For i As Integer = 0 To n - 1
            xStarts(i) = categories(i).ConnectorStart
            yValues(i) = categories(i).YPosition
            connectorLengths(i) = categories(i).ConnectorLength
            If connectorLengths(i) > 0.0R Then hasVisibleConnector = True
        Next

Return New SeriesPayload34(xStarts, yValues, connectorLengths, source:=categories.Select(Function(c) c.SourceIndex).ToArray(), isVisible:=hasVisibleConnector)
End Function

Public Function Endpoints(categories As DumbbellPlotCategory(), useFirstEndpoint As Boolean) As SeriesPayload34
        Dim n As Integer = categories.Length
        Dim xValues(n - 1) As Double
        Dim yValues(n - 1) As Double

        For i As Integer = 0 To n - 1
            xValues(i) = If(useFirstEndpoint, categories(i).FirstValue, categories(i).SecondValue)
            yValues(i) = categories(i).YPosition
        Next

Return New SeriesPayload34(xValues, yValues, source:=categories.Select(Function(c) c.SourceIndex).ToArray())
End Function

Public Function Auxiliary(observations As DumbbellAuxiliaryObservation()) As SeriesPayload34
        If observations Is Nothing OrElse observations.Length = 0 Then Return Nothing

        Dim xValues(observations.Length - 1) As Double
        Dim yValues(observations.Length - 1) As Double
        For i As Integer = 0 To observations.Length - 1
            xValues(i) = observations(i).Value
            yValues(i) = observations(i).YPosition
        Next

Return New SeriesPayload34(xValues, yValues, source:=observations.Select(Function(c) c.SourceIndex).ToArray())
End Function

Public Function CategoryLabels(categories As DumbbellPlotCategory(), minimum As Double) As SeriesPayload34
        Dim n As Integer = categories.Length
        Dim xValues(n - 1) As Double
        Dim yValues(n - 1) As Double

        For i As Integer = 0 To n - 1
            xValues(i) = minimum
            yValues(i) = categories(i).YPosition
        Next

Return New SeriesPayload34(xValues, yValues, labels:=categories.Select(Function(c) c.CategoryLabel).ToArray(), source:=categories.Select(Function(c) c.SourceIndex).ToArray())
End Function

Public Function NumericEndpoint(value As Double, requestedFormat As String) As String
        If Not String.IsNullOrWhiteSpace(requestedFormat) Then
            Try
                Return value.ToString(requestedFormat.Trim(), CultureInfo.CurrentCulture)
            Catch
            End Try
        End If
        Return value.ToString("0.###", CultureInfo.CurrentCulture)

End Function

End Module
End Namespace
