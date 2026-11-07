Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34.LadderSeries
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
Public Module LadderSeriesPreparation34
Public Function ObservationCoordinates(observation As LadderPlotObservation) As SeriesPayload34
        Dim xValues() As Double = {observation.FirstXPosition, observation.SecondXPosition}
        Dim yValues() As Double = {observation.FirstValue, observation.SecondValue}

Return New SeriesPayload34(xValues, yValues, source:=New Integer(){observation.SourceIndex,observation.SourceIndex})
End Function

Public Function EndpointValues(observations As LadderPlotObservation(), useFirstSide As Boolean) As SeriesPayload34
        Dim actualValues(observations.Length - 1) As Double
        For i As Integer = 0 To observations.Length - 1
            actualValues(i) = If(useFirstSide, observations(i).FirstValue, observations(i).SecondValue)
        Next

Return New SeriesPayload34(New Double(){}, actualValues, source:=observations.Select(Function(c) c.SourceIndex).ToArray())
End Function

Public Function EndpointAnchors(observations As LadderPlotObservation(), useFirstSide As Boolean, minimum As Double, maximum As Double, gap As Double) As SeriesPayload34
        Dim maximumObservedJitter As Double = 0.0R
        For i As Integer = 0 To observations.Length - 1
            Dim offset As Double = If(useFirstSide,
                                      Math.Abs(observations(i).FirstJitterOffset),
                                      Math.Abs(observations(i).SecondJitterOffset))
            maximumObservedJitter = Math.Max(maximumObservedJitter, offset)
        Next

        Dim anchorX As Double
        If useFirstSide Then
            anchorX = LadderPlotCore.FirstBaseXPosition - maximumObservedJitter - gap
            anchorX = Math.Max(minimum + 0.02R, anchorX)
        Else
            anchorX = LadderPlotCore.SecondBaseXPosition + maximumObservedJitter + gap
            anchorX = Math.Min(maximum - 0.02R, anchorX)
        End If

        Dim xValues(observations.Length - 1) As Double
        For i As Integer = 0 To xValues.Length - 1
            xValues(i) = anchorX
        Next

Return New SeriesPayload34(xValues, New Double(){}, source:=observations.Select(Function(c) c.SourceIndex).ToArray())
End Function

Public Function SideLabels(minimum As Double, firstLabel As String, secondLabel As String) As SeriesPayload34
        Dim xValues() As Double = {LadderPlotCore.FirstBaseXPosition, LadderPlotCore.SecondBaseXPosition}
        Dim yValues() As Double = {minimum, minimum}
        Dim labels() As String = {
            If(firstLabel, String.Empty),
            If(secondLabel, String.Empty)
        }

Return New SeriesPayload34(xValues, yValues, labels:=labels)
End Function

Public Function MinimumSeparation(plotHeight As Double, requestedChartHeight As Double, minimum As Double, maximum As Double, minimumPoints As Double, fontSize As Double) As Double
        If Not IsFinitePositive(plotHeight) Then
            plotHeight = Math.Max(120.0R, requestedChartHeight * 0.7R)
        End If

        Dim desiredPoints As Double = Math.Max(minimumPoints,
                                               fontSize * 1.25R)
        Dim axisSpan As Double = maximum - minimum
        If Not IsFinitePositive(axisSpan) Then Return 0.0R

        Return axisSpan * desiredPoints / plotHeight

End Function
Private Function IsFinitePositive(value As Double) As Boolean
Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0R
End Function

End Module
End Namespace
