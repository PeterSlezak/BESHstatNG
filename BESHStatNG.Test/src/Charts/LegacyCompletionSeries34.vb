Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch34SeriesFixtures
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
Public Module LegacyDumbbellSeries34
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
Public Module LegacyLadderSeries34
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
Public Module LegacyAgreementSeries34
Public Function BaseSpan(values As Double()) As ChartAxisScaleResult
Dim xMin As Double = values.Min()
Dim xMax As Double = values.Max()
If xMin = xMax Then
xMin -= 0.5
xMax += 0.5
End If
Return New ChartAxisScaleResult(xMin,xMax,0.0R)
End Function

End Module
Public Module LegacyComparisonSeries34
        Public Function SuggestedHeight(data As parametric.MultipleComparisonPlotData) As Double
            If data Is Nothing OrElse data.Estimates Is Nothing Then Return 320.0
            Return Math.Max(320.0, Math.Min(720.0, 150.0 + 32.0 * data.Estimates.Length))
        End Function
End Module
Public Module LegacyCdfSeries34
Public Function PercentileReference(xCorner As Double, yCorner As Double, minimumX As Double, minimumY As Double) As SeriesPayload34
If Double.IsNaN(xCorner) OrElse Double.IsInfinity(xCorner) OrElse Double.IsNaN(yCorner) OrElse Double.IsInfinity(yCorner) Then Return Nothing
Dim xValues() As Double = {minimumX, xCorner, xCorner}
Dim yValues() As Double = {yCorner, yCorner, minimumY}
Return New SeriesPayload34(xValues,yValues)
End Function

End Module
End Namespace
