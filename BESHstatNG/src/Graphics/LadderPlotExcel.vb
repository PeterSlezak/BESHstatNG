Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Determines how ladder-plot observations are coloured in the Excel renderer.
''' </summary>
Public Enum LadderColorMode
    ''' <summary>Every paired observation uses the same colour.</summary>
    SingleColor

    ''' <summary>Each paired observation cycles through the configured colour palette.</summary>
    ByObservation

    ''' <summary>Observations use the colour assigned to their grouping level.</summary>
    ByGroup
End Enum

''' <summary>
''' Line patterns available for ladder-plot connecting lines.
''' </summary>
''' <remarks>
''' A BESHStatNG-specific enum is used so the public appearance object does not need
''' to expose Microsoft.Office.Core types.
''' </remarks>
Public Enum LadderLineStyle
    Solid
    Dash
    Dot
    DashDot
    DashDotDot
End Enum

''' <summary>
''' Text displayed beside the two endpoints when endpoint labels are enabled.
''' </summary>
Public Enum LadderEndpointLabelContent
    Value
    RecordLabel
    RecordLabelAndValue
End Enum

''' <summary>
''' Excel-specific appearance options for <see cref="LadderPlotExcel"/>.
''' </summary>
''' <remarks>
''' Colours are OLE RGB integers as expected by the Excel object model. Numerical
''' preparation, including optional collision-aware horizontal jitter, is performed by
''' <see cref="LadderPlot"/> and is not modified by this class.
''' </remarks>
Public Class LadderPlotAppearance
    Public Property ChartTitle As String = "Ladder plot"
    Public Property YAxisTitle As String = String.Empty
    Public Property FirstAxisLabel As String = "Before"
    Public Property SecondAxisLabel As String = "After"
    Public Property SeriesName As String = "Data"

    Public Property ColorMode As LadderColorMode = LadderColorMode.SingleColor

    ''' <summary>OLE RGB colour used when <see cref="ColorMode"/> is SingleColor.</summary>
    Public Property SingleColor As Integer = &HB4771F

    ''' <summary>
    ''' Tableau-10-style OLE RGB palette used by ByObservation and ByGroup modes.
    ''' </summary>
    Public Property PaletteColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property ShowLines As Boolean = True
    Public Property LineWeight As Single = 1.5F
    Public Property LineTransparency As Single = 0.0F
    Public Property LineStyle As LadderLineStyle = LadderLineStyle.Solid

    Public Property ShowMarkers As Boolean = True
    Public Property MarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
    Public Property MarkerSize As Integer = 6

    Public Property ShowLegend As Boolean = False
    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionBottom
    Public Property LegendFontSize As Double = 9.0R

    Public Property ShowFirstEndpointLabels As Boolean = False
    Public Property ShowSecondEndpointLabels As Boolean = False
    Public Property EndpointLabelContent As LadderEndpointLabelContent = LadderEndpointLabelContent.RecordLabelAndValue
    Public Property EndpointLabelSeparator As String = ", "
    Public Property EndpointValueDisplayFormat As String = String.Empty
    Public Property EndpointLabelFontSize As Double = 9.0R
    Public Property ColorEndpointLabelsBySeries As Boolean = True

    ''' <summary>
    ''' When True, endpoint label anchors are moved vertically in chart coordinates when
    ''' necessary to reduce label overlap. The plotted observations themselves are not moved.
    ''' </summary>
    Public Property AvoidEndpointLabelOverlap As Boolean = True

    ''' <summary>
    ''' Minimum desired vertical distance between endpoint labels in chart points. The
    ''' renderer converts this to Y-axis units after Excel has created the plot area.
    ''' </summary>
    Public Property EndpointLabelMinimumSeparationPoints As Double = 12.0R

    ''' <summary>
    ''' Horizontal gap, in X-axis coordinate units, between the outermost possible jittered
    ''' point and the invisible helper point used to anchor an endpoint label.
    ''' </summary>
    Public Property EndpointLabelAnchorGap As Double = 0.035R

    Public Property ShowSideLabels As Boolean = True
    Public Property SideLabelFontSize As Double = 10.0R
    Public Property SideLabelBold As Boolean = True

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property TextColor As Integer = &H404040
    Public Property GridlineColor As Integer = &HE6E6E6
    Public Property GridlineWeight As Single = 0.75F
    Public Property AxisLineColor As Integer = &H808080
    Public Property AxisLineWeight As Single = 0.75F

    Public Property ShowHorizontalGridlines As Boolean = True
    Public Property ShowXAxisLine As Boolean = True
    Public Property ShowYAxisLine As Boolean = True

    Public Property YAxisLabelFontSize As Double = 9.0R
    Public Property ChartTitleFontSize As Double = 12.0R

    ''' <summary>Optional manual Y-axis minimum.</summary>
    Public Property YAxisMinimum As Nullable(Of Double) = Nothing

    ''' <summary>Optional manual Y-axis maximum.</summary>
    Public Property YAxisMaximum As Nullable(Of Double) = Nothing

    ''' <summary>Optional manual Y-axis major interval.</summary>
    Public Property YAxisMajorUnit As Nullable(Of Double) = Nothing

    ''' <summary>Optional Excel number format for Y-axis tick labels.</summary>
    Public Property YAxisNumberFormat As String = String.Empty

    ''' <summary>Fraction of the observed Y span padded at automatic axis ends.</summary>
    Public Property YAxisPaddingFraction As Double = 0.04R

    ''' <summary>
    ''' When True, an automatic Y axis includes zero whenever all observed values are on
    ''' the same side of zero. Manual limits always take precedence.
    ''' </summary>
    Public Property YAxisIncludeZero As Boolean = False

    Friend Function Copy() As LadderPlotAppearance
        Return New LadderPlotAppearance With {
            .ChartTitle = ChartTitle,
            .YAxisTitle = YAxisTitle,
            .FirstAxisLabel = FirstAxisLabel,
            .SecondAxisLabel = SecondAxisLabel,
            .SeriesName = SeriesName,
            .ColorMode = ColorMode,
            .SingleColor = SingleColor,
            .PaletteColors = If(PaletteColors Is Nothing, Nothing, DirectCast(PaletteColors.Clone(), Integer())),
            .ShowLines = ShowLines,
            .LineWeight = LineWeight,
            .LineTransparency = LineTransparency,
            .LineStyle = LineStyle,
            .ShowMarkers = ShowMarkers,
            .MarkerStyle = MarkerStyle,
            .MarkerSize = MarkerSize,
            .ShowLegend = ShowLegend,
            .LegendPosition = LegendPosition,
            .LegendFontSize = LegendFontSize,
            .ShowFirstEndpointLabels = ShowFirstEndpointLabels,
            .ShowSecondEndpointLabels = ShowSecondEndpointLabels,
            .EndpointLabelContent = EndpointLabelContent,
            .EndpointLabelSeparator = EndpointLabelSeparator,
            .EndpointValueDisplayFormat = EndpointValueDisplayFormat,
            .EndpointLabelFontSize = EndpointLabelFontSize,
            .ColorEndpointLabelsBySeries = ColorEndpointLabelsBySeries,
            .AvoidEndpointLabelOverlap = AvoidEndpointLabelOverlap,
            .EndpointLabelMinimumSeparationPoints = EndpointLabelMinimumSeparationPoints,
            .EndpointLabelAnchorGap = EndpointLabelAnchorGap,
            .ShowSideLabels = ShowSideLabels,
            .SideLabelFontSize = SideLabelFontSize,
            .SideLabelBold = SideLabelBold,
            .BackgroundColor = BackgroundColor,
            .TextColor = TextColor,
            .GridlineColor = GridlineColor,
            .GridlineWeight = GridlineWeight,
            .AxisLineColor = AxisLineColor,
            .AxisLineWeight = AxisLineWeight,
            .ShowHorizontalGridlines = ShowHorizontalGridlines,
            .ShowXAxisLine = ShowXAxisLine,
            .ShowYAxisLine = ShowYAxisLine,
            .YAxisLabelFontSize = YAxisLabelFontSize,
            .ChartTitleFontSize = ChartTitleFontSize,
            .YAxisMinimum = YAxisMinimum,
            .YAxisMaximum = YAxisMaximum,
            .YAxisMajorUnit = YAxisMajorUnit,
            .YAxisNumberFormat = YAxisNumberFormat,
            .YAxisPaddingFraction = YAxisPaddingFraction,
            .YAxisIncludeZero = YAxisIncludeZero
        }
    End Function
End Class

''' <summary>
''' Renders a <see cref="LadderPlotResult"/> as an embedded Excel XY-scatter chart.
''' </summary>
''' <remarks>
''' Each paired observation is a two-point XY series. This keeps the connection between
''' its First/Before and Second/After values explicit and allows collision-aware horizontal
''' jitter prepared by <see cref="LadderPlot"/> to be honoured independently at either end.
'''
''' Endpoint text is carried by invisible helper series. Their Y coordinates can be spread
''' to reduce text collisions without changing the actual data geometry. Because the helper
''' positions are expressed in chart coordinates, the layout remains stable when the chart
''' is resized more reliably than worksheet text boxes would.
''' </remarks>
Public NotInheritable Class LadderPlotExcel
    Private Sub New()
    End Sub

    'Stable Office MsoLineDashStyle values used only through late binding.
    Private Const MsoLineSolid As Integer = 1
    Private Const MsoLineSquareDot As Integer = 2
    Private Const MsoLineDash As Integer = 4
    Private Const MsoLineDashDot As Integer = 5
    Private Const MsoLineDashDotDot As Integer = 6

    'The data occupy X=1 and X=2. The fixed outer limits leave space for the maximum
    'backend jitter (+/-0.25) and for endpoint labels on both sides.
    Private Const PlotXMinimum As Double = 0.5R
    Private Const PlotXMaximum As Double = 2.5R

    Private NotInheritable Class AxisContext
        Friend XMinimum As Double
        Friend XMaximum As Double
        Friend YMinimum As Double
        Friend YMaximum As Double
        Friend YMajorUnit As Double
        Friend YNumberFormat As String
    End Class

    Private NotInheritable Class SeriesStyleInfo
        Friend Series As Series
        Friend Color As Integer
    End Class

    ''' <summary>
    ''' Adds a ladder plot to an Excel worksheet.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As LadderPlotResult,
                                    Optional appearance As LadderPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 760.0R,
                                    Optional height As Double = 480.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.ObservationCount < 1 Then
            Throw New ArgumentException("The ladder result contains no observations.", NameOf(result))
        End If
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width must be finite and positive.")
        End If
        If Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(height), "Chart height must be finite and positive.")
        End If

        Dim resolvedAppearance As LadderPlotAppearance = If(appearance, New LadderPlotAppearance()).Copy()
        ValidateAppearance(resolvedAppearance)
        ValidateResultCompatibility(result, resolvedAppearance)

        Dim observations As LadderPlotObservation() = result.Observations
        Dim axisContext As AxisContext = ResolveAxisContext(result, resolvedAppearance)
        Dim chartShape As Shape = Nothing

        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatterLines, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatterLines
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim dataSeries As New List(Of SeriesStyleInfo)()
            Dim seenLegendGroups As New HashSet(Of Integer)()

            For i As Integer = 0 To observations.Length - 1
                Dim observation As LadderPlotObservation = observations(i)
                Dim seriesColor As Integer = ResolveObservationColor(observation, resolvedAppearance)
                Dim seriesName As String = ResolveObservationSeriesName(observation, resolvedAppearance)

                Dim series As Series = AddObservationSeries(seriesCollection,
                                                            observation,
                                                            seriesName,
                                                            seriesColor,
                                                            resolvedAppearance)
                dataSeries.Add(New SeriesStyleInfo With {.Series = series, .Color = seriesColor})

                Dim seriesIndex As Integer = seriesCollection.Count
                If ShouldKeepLegendEntry(observation, i, resolvedAppearance, seenLegendGroups) Then
                    legendSeriesIndices.Add(seriesIndex)
                End If
            Next

            If resolvedAppearance.ShowSideLabels Then
                AddSideLabelSeries(seriesCollection, axisContext, resolvedAppearance)
            End If

            ConfigureAxes(chart, axisContext, resolvedAppearance)
            chart.Refresh()

            'Excel can reapply a chart style during the first refresh. Reassert the data
            'series formatting before labels are added.
            For Each styleInfo As SeriesStyleInfo In dataSeries
                ApplyObservationSeriesAppearance(styleInfo.Series, styleInfo.Color, resolvedAppearance)
            Next
            ConfigureAxes(chart, axisContext, resolvedAppearance)

            If resolvedAppearance.ShowFirstEndpointLabels Then
                AddEndpointLabelSeries(chart,
                                       seriesCollection,
                                       observations,
                                       True,
                                       result,
                                       axisContext,
                                       resolvedAppearance,
                                       height)
            End If

            If resolvedAppearance.ShowSecondEndpointLabels Then
                AddEndpointLabelSeries(chart,
                                       seriesCollection,
                                       observations,
                                       False,
                                       result,
                                       axisContext,
                                       resolvedAppearance,
                                       height)
            End If

            chart.Refresh()
            ConfigureAxes(chart, axisContext, resolvedAppearance)
            ConfigureLegend(chart, legendSeriesIndices, resolvedAppearance)

            'A final data-series pass prevents some Excel chart styles from reintroducing
            'default marker fills or line weights after helper label series are created.
            For Each styleInfo As SeriesStyleInfo In dataSeries
                ApplyObservationSeriesAppearance(styleInfo.Series, styleInfo.Color, resolvedAppearance)
            Next

            Return chart
        Catch
            If chartShape IsNot Nothing Then
                Try
                    chartShape.Delete()
                Catch
                End Try
            End If
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Suggested chart width. Endpoint labels need more horizontal room than a plain
    ''' before/after ladder chart.
    ''' </summary>
    Public Shared Function SuggestedWidth(result As LadderPlotResult,
                                          Optional appearance As LadderPlotAppearance = Nothing) As Double
        Dim resolved As LadderPlotAppearance = If(appearance, New LadderPlotAppearance())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then Return 820.0R
        Return 620.0R
    End Function

    ''' <summary>
    ''' Suggested chart height based primarily on the number of visible endpoint labels.
    ''' </summary>
    Public Shared Function SuggestedHeight(result As LadderPlotResult,
                                           Optional appearance As LadderPlotAppearance = Nothing) As Double
        If result Is Nothing OrElse result.ObservationCount < 1 Then Return 420.0R
        Dim resolved As LadderPlotAppearance = If(appearance, New LadderPlotAppearance())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then
            Return Math.Max(420.0R, Math.Min(1200.0R, 210.0R + 17.0R * result.ObservationCount))
        End If
        Return 440.0R
    End Function

    Private Shared Function AddObservationSeries(seriesCollection As SeriesCollection,
                                                 observation As LadderPlotObservation,
                                                 seriesName As String,
                                                 seriesColor As Integer,
                                                 appearance As LadderPlotAppearance) As Series
        Dim xValues() As Double = {observation.FirstXPosition, observation.SecondXPosition}
        Dim yValues() As Double = {observation.FirstValue, observation.SecondValue}

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatterLines
            .XValues = xValues
            .Values = yValues
        End With

        ApplyObservationSeriesAppearance(DirectCast(series, Series), seriesColor, appearance)
        Return DirectCast(series, Series)
    End Function

    Private Shared Sub ApplyObservationSeriesAppearance(series As Series,
                                                        seriesColor As Integer,
                                                        appearance As LadderPlotAppearance)
        If series Is Nothing Then Return
        Dim seriesObject As Object = series

        Try
            series.ChartType = XlChartType.xlXYScatterLines
        Catch
        End Try

        If appearance.ShowMarkers Then
            Try
                series.MarkerStyle = appearance.MarkerStyle
                series.MarkerSize = appearance.MarkerSize
                series.MarkerForegroundColor = seriesColor
                series.MarkerBackgroundColor = seriesColor
            Catch
            End Try
        Else
            Try
                series.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            Catch
            End Try
        End If

        If appearance.ShowLines Then
            Try
                With seriesObject.Format.Line
                    .Visible = True
                    .ForeColor.RGB = seriesColor
                    .Weight = appearance.LineWeight
                    .Transparency = appearance.LineTransparency
                    .DashStyle = ToMsoDashStyle(appearance.LineStyle)
                End With
            Catch
                Try
                    seriesObject.Border.Color = seriesColor
                    seriesObject.Border.Weight = appearance.LineWeight
                Catch
                End Try
            End Try
        Else
            Try
                seriesObject.Format.Line.Visible = False
            Catch
            End Try
            Try
                seriesObject.Border.LineStyle = Constants.xlNone
            Catch
            End Try
        End If
    End Sub

    Private Shared Sub AddSideLabelSeries(seriesCollection As SeriesCollection,
                                         axisContext As AxisContext,
                                         appearance As LadderPlotAppearance)
        If String.IsNullOrWhiteSpace(appearance.FirstAxisLabel) AndAlso
           String.IsNullOrWhiteSpace(appearance.SecondAxisLabel) Then Return

        Dim xValues() As Double = {LadderPlot.FirstBaseXPosition, LadderPlot.SecondBaseXPosition}
        Dim yValues() As Double = {axisContext.YMinimum, axisContext.YMinimum}
        Dim labels() As String = {
            If(appearance.FirstAxisLabel, String.Empty),
            If(appearance.SecondAxisLabel, String.Empty)
        }

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = "Ladder side labels"
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
            .MarkerSize = 2
            .MarkerForegroundColor = appearance.BackgroundColor
            .MarkerBackgroundColor = appearance.BackgroundColor
            .Format.Line.Visible = False
        End With

        Try
            For i As Integer = 0 To 1
                If String.IsNullOrWhiteSpace(labels(i)) Then Continue For
                Dim point As Object = series.Points(i + 1)
                point.HasDataLabel = True
                With point.DataLabel
                    .Text = labels(i).Trim()
                    .Position = XlDataLabelPosition.xlLabelPositionBelow
                    .Font.Size = appearance.SideLabelFontSize
                    .Font.Color = appearance.TextColor
                    .Font.Bold = appearance.SideLabelBold
                End With
            Next
        Catch
            'Axis-side labels are cosmetic; retain the chart if a particular Excel build
            'rejects custom helper labels.
        End Try
    End Sub

    Private Shared Sub AddEndpointLabelSeries(chart As Chart,
                                             seriesCollection As SeriesCollection,
                                             observations As LadderPlotObservation(),
                                             useFirstSide As Boolean,
                                             result As LadderPlotResult,
                                             axisContext As AxisContext,
                                             appearance As LadderPlotAppearance,
                                             requestedChartHeight As Double)
        If observations Is Nothing OrElse observations.Length = 0 Then Return

        Dim actualValues(observations.Length - 1) As Double
        For i As Integer = 0 To observations.Length - 1
            actualValues(i) = If(useFirstSide, observations(i).FirstValue, observations(i).SecondValue)
        Next

        Dim labelValues As Double() = DirectCast(actualValues.Clone(), Double())
        If appearance.AvoidEndpointLabelOverlap AndAlso observations.Length > 1 Then
            Dim separation As Double = ResolveMinimumLabelSeparation(chart,
                                                                    axisContext,
                                                                    appearance,
                                                                    requestedChartHeight)
            labelValues = SpreadLabelPositions(actualValues,
                                               axisContext.YMinimum,
                                               axisContext.YMaximum,
                                               separation)
        End If

        Dim maximumObservedJitter As Double = 0.0R
        For i As Integer = 0 To observations.Length - 1
            Dim offset As Double = If(useFirstSide,
                                      Math.Abs(observations(i).FirstJitterOffset),
                                      Math.Abs(observations(i).SecondJitterOffset))
            maximumObservedJitter = Math.Max(maximumObservedJitter, offset)
        Next

        Dim anchorX As Double
        If useFirstSide Then
            anchorX = LadderPlot.FirstBaseXPosition - maximumObservedJitter - appearance.EndpointLabelAnchorGap
            anchorX = Math.Max(axisContext.XMinimum + 0.02R, anchorX)
        Else
            anchorX = LadderPlot.SecondBaseXPosition + maximumObservedJitter + appearance.EndpointLabelAnchorGap
            anchorX = Math.Min(axisContext.XMaximum - 0.02R, anchorX)
        End If

        Dim xValues(observations.Length - 1) As Double
        For i As Integer = 0 To xValues.Length - 1
            xValues(i) = anchorX
        Next

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = If(useFirstSide, "First endpoint labels", "Second endpoint labels")
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = labelValues

            'A tiny background-coloured marker gives Excel a concrete anchor for Left/Right
            'label positioning while remaining visually invisible.
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
            .MarkerSize = 2
            .MarkerForegroundColor = appearance.BackgroundColor
            .MarkerBackgroundColor = appearance.BackgroundColor
            .Format.Line.Visible = False
        End With

        Try
            For i As Integer = 0 To observations.Length - 1
                Dim observation As LadderPlotObservation = observations(i)
                Dim point As Object = series.Points(i + 1)
                point.HasDataLabel = True
                With point.DataLabel
                    .Text = BuildEndpointLabel(observation, useFirstSide, appearance)
                    .Position = If(useFirstSide,
                                   XlDataLabelPosition.xlLabelPositionLeft,
                                   XlDataLabelPosition.xlLabelPositionRight)
                    .Font.Size = appearance.EndpointLabelFontSize
                    .Font.Color = If(appearance.ColorEndpointLabelsBySeries,
                                     ResolveObservationColor(observation, appearance),
                                     appearance.TextColor)
                End With
            Next
        Catch
            'Endpoint labels are optional. Do not discard a valid chart if an older Excel
            'build rejects point-level data-label formatting.
        End Try
    End Sub

    ''' <summary>
    ''' Converts a desired physical label separation to Y-axis units using Excel's current
    ''' plot-area height. This makes the collision avoidance responsive to chart size.
    ''' </summary>
    Private Shared Function ResolveMinimumLabelSeparation(chart As Chart,
                                                          axisContext As AxisContext,
                                                          appearance As LadderPlotAppearance,
                                                          requestedChartHeight As Double) As Double
        Dim plotHeight As Double = 0.0R
        Try
            plotHeight = CDbl(chart.PlotArea.InsideHeight)
        Catch
        End Try
        If Not IsFinitePositive(plotHeight) Then
            plotHeight = Math.Max(120.0R, requestedChartHeight * 0.7R)
        End If

        Dim desiredPoints As Double = Math.Max(appearance.EndpointLabelMinimumSeparationPoints,
                                               appearance.EndpointLabelFontSize * 1.25R)
        Dim axisSpan As Double = axisContext.YMaximum - axisContext.YMinimum
        If Not IsFinitePositive(axisSpan) Then Return 0.0R

        Return axisSpan * desiredPoints / plotHeight
    End Function

    ''' <summary>
    ''' Spreads close label anchors vertically while preserving their data order and keeping
    ''' them inside the visible Y range. Values that already have sufficient separation are
    ''' left at their original coordinates whenever possible.
    ''' </summary>
    Private Shared Function SpreadLabelPositions(values As Double(),
                                                 minimum As Double,
                                                 maximum As Double,
                                                 requestedSeparation As Double) As Double()
        Dim result As Double() = DirectCast(values.Clone(), Double())
        If values.Length < 2 OrElse Not IsFinitePositive(requestedSeparation) Then Return result

        Dim span As Double = maximum - minimum
        If Not IsFinitePositive(span) Then Return result

        Dim orderedIndices As Integer() = Enumerable.Range(0, values.Length).
                                                   OrderBy(Function(i) values(i)).
                                                   ThenBy(Function(i) i).
                                                   ToArray()

        'If the requested physical spacing cannot fit all labels, use the largest feasible
        'spacing instead of forcing anchors outside the plot.
        Dim separation As Double = requestedSeparation
        If values.Length > 1 Then
            separation = Math.Min(separation, span / CDbl(values.Length - 1))
        End If
        If separation <= 0.0R Then Return result

        Dim lowerBound As Double = minimum
        Dim upperBound As Double = maximum
        If separation < span Then
            lowerBound = minimum + 0.5R * separation
            upperBound = maximum - 0.5R * separation
        End If

        Dim adjusted(values.Length - 1) As Double
        adjusted(0) = Clamp(values(orderedIndices(0)), lowerBound, upperBound)
        For j As Integer = 1 To orderedIndices.Length - 1
            Dim desired As Double = Clamp(values(orderedIndices(j)), lowerBound, upperBound)
            adjusted(j) = Math.Max(desired, adjusted(j - 1) + separation)
        Next

        If adjusted(adjusted.Length - 1) > upperBound Then
            adjusted(adjusted.Length - 1) = upperBound
            For j As Integer = adjusted.Length - 2 To 0 Step -1
                Dim desired As Double = Clamp(values(orderedIndices(j)), lowerBound, upperBound)
                adjusted(j) = Math.Min(desired, adjusted(j + 1) - separation)
            Next
        End If

        If adjusted(0) < lowerBound Then
            Dim shift As Double = lowerBound - adjusted(0)
            For j As Integer = 0 To adjusted.Length - 1
                adjusted(j) += shift
            Next
        End If

        'One last backward pass protects the upper edge after a possible bottom shift.
        If adjusted(adjusted.Length - 1) > upperBound Then
            adjusted(adjusted.Length - 1) = upperBound
            For j As Integer = adjusted.Length - 2 To 0 Step -1
                adjusted(j) = Math.Min(adjusted(j), adjusted(j + 1) - separation)
            Next
        End If

        For j As Integer = 0 To orderedIndices.Length - 1
            result(orderedIndices(j)) = Clamp(adjusted(j), minimum, maximum)
        Next
        Return result
    End Function

    Private Shared Function BuildEndpointLabel(observation As LadderPlotObservation,
                                               useFirstSide As Boolean,
                                               appearance As LadderPlotAppearance) As String
        Dim value As Double = If(useFirstSide, observation.FirstValue, observation.SecondValue)
        Dim valueText As String = FormatEndpointValue(value, appearance.EndpointValueDisplayFormat)

        Select Case appearance.EndpointLabelContent
            Case LadderEndpointLabelContent.Value
                Return valueText
            Case LadderEndpointLabelContent.RecordLabel
                Return observation.RecordLabel
            Case LadderEndpointLabelContent.RecordLabelAndValue
                Dim separator As String = If(appearance.EndpointLabelSeparator, String.Empty)
                Return observation.RecordLabel & separator & valueText
            Case Else
                Return observation.RecordLabel & ", " & valueText
        End Select
    End Function

    Private Shared Function FormatEndpointValue(value As Double, requestedFormat As String) As String
        If Not String.IsNullOrWhiteSpace(requestedFormat) Then
            Try
                Return value.ToString(requestedFormat.Trim(), CultureInfo.CurrentCulture)
            Catch ex As FormatException
                'Fall through to a compact default rather than failing chart creation for
                'an invalid cosmetic display format.
            End Try
        End If
        Return value.ToString("0.###", CultureInfo.CurrentCulture)
    End Function

    Private Shared Function ResolveObservationColor(observation As LadderPlotObservation,
                                                    appearance As LadderPlotAppearance) As Integer
        Select Case appearance.ColorMode
            Case LadderColorMode.SingleColor
                Return appearance.SingleColor
            Case LadderColorMode.ByObservation
                Return appearance.PaletteColors(observation.DisplayIndex Mod appearance.PaletteColors.Length)
            Case LadderColorMode.ByGroup
                Return appearance.PaletteColors(observation.GroupLevelIndex Mod appearance.PaletteColors.Length)
            Case Else
                Return appearance.SingleColor
        End Select
    End Function

    Private Shared Function ResolveObservationSeriesName(observation As LadderPlotObservation,
                                                         appearance As LadderPlotAppearance) As String
        Select Case appearance.ColorMode
            Case LadderColorMode.ByObservation
                If Not String.IsNullOrWhiteSpace(observation.RecordLabel) Then Return observation.RecordLabel
                Return "Observation " & (observation.DisplayIndex + 1).ToString(CultureInfo.CurrentCulture)
            Case LadderColorMode.ByGroup
                If Not String.IsNullOrWhiteSpace(observation.GroupLabel) Then Return observation.GroupLabel
                Return "Group " & (observation.GroupLevelIndex + 1).ToString(CultureInfo.CurrentCulture)
            Case Else
                If String.IsNullOrWhiteSpace(appearance.SeriesName) Then Return "Data"
                Return appearance.SeriesName.Trim()
        End Select
    End Function

    Private Shared Function ShouldKeepLegendEntry(observation As LadderPlotObservation,
                                                  observationIndex As Integer,
                                                  appearance As LadderPlotAppearance,
                                                  seenLegendGroups As HashSet(Of Integer)) As Boolean
        If Not appearance.ShowLegend Then Return False

        Select Case appearance.ColorMode
            Case LadderColorMode.SingleColor
                Return observationIndex = 0
            Case LadderColorMode.ByObservation
                Return True
            Case LadderColorMode.ByGroup
                If seenLegendGroups.Add(observation.GroupLevelIndex) Then Return True
                Return False
            Case Else
                Return observationIndex = 0
        End Select
    End Function

    Private Shared Function ResolveAxisContext(result As LadderPlotResult,
                                               appearance As LadderPlotAppearance) As AxisContext
        Dim dataMinimum As Double = result.MinimumValue
        Dim dataMaximum As Double = result.MaximumValue
        If Not IsFinite(dataMinimum) OrElse Not IsFinite(dataMaximum) Then
            Throw New ArgumentException("The ladder result does not contain a finite Y range.", NameOf(result))
        End If

        Dim span As Double = dataMaximum - dataMinimum
        Dim fallbackSpan As Double = If(span > 0.0R,
                                        span,
                                        Math.Max(1.0R, Math.Abs(dataMinimum) * 0.1R))
        If Not IsFinitePositive(fallbackSpan) Then fallbackSpan = 1.0R

        Dim padding As Double = fallbackSpan * appearance.YAxisPaddingFraction
        Dim paddedMinimum As Double = dataMinimum - padding
        Dim paddedMaximum As Double = dataMaximum + padding
        If paddedMinimum >= paddedMaximum Then
            paddedMinimum = dataMinimum - 0.5R * fallbackSpan
            paddedMaximum = dataMaximum + 0.5R * fallbackSpan
        End If

        Dim majorUnit As Double
        If appearance.YAxisMajorUnit.HasValue Then
            majorUnit = appearance.YAxisMajorUnit.Value
        Else
            majorUnit = NiceStep125((paddedMaximum - paddedMinimum) / 5.0R)
        End If
        If Not IsFinitePositive(majorUnit) Then majorUnit = 1.0R

        Dim yMinimum As Double
        If appearance.YAxisMinimum.HasValue Then
            yMinimum = appearance.YAxisMinimum.Value
        Else
            yMinimum = FloorToStep(paddedMinimum, majorUnit)
            If appearance.YAxisIncludeZero AndAlso dataMinimum >= 0.0R Then yMinimum = Math.Min(0.0R, yMinimum)
        End If

        Dim yMaximum As Double
        If appearance.YAxisMaximum.HasValue Then
            yMaximum = appearance.YAxisMaximum.Value
        Else
            yMaximum = CeilingToStep(paddedMaximum, majorUnit)
            If appearance.YAxisIncludeZero AndAlso dataMaximum <= 0.0R Then yMaximum = Math.Max(0.0R, yMaximum)
        End If

        If yMinimum >= yMaximum Then
            If Not appearance.YAxisMinimum.HasValue AndAlso Not appearance.YAxisMaximum.HasValue Then
                yMinimum = FloorToStep(dataMinimum - 0.5R * fallbackSpan, majorUnit)
                yMaximum = CeilingToStep(dataMaximum + 0.5R * fallbackSpan, majorUnit)
                If yMinimum >= yMaximum Then yMaximum = yMinimum + majorUnit
            Else
                Throw New ArgumentException("The resolved Y-axis minimum must be smaller than the Y-axis maximum.", NameOf(appearance))
            End If
        End If

        Return New AxisContext With {
            .XMinimum = PlotXMinimum,
            .XMaximum = PlotXMaximum,
            .YMinimum = yMinimum,
            .YMaximum = yMaximum,
            .YMajorUnit = majorUnit,
            .YNumberFormat = ResolveYAxisNumberFormat(appearance.YAxisNumberFormat)
        }
    End Function

    Private Shared Function ResolveYAxisNumberFormat(requestedFormat As String) As String
        If String.IsNullOrWhiteSpace(requestedFormat) Then Return "0.###"
        Return requestedFormat.Trim()
    End Function

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     axisContext As AxisContext,
                                     appearance As LadderPlotAppearance)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
        With xAxis
            .MinimumScale = axisContext.XMinimum
            .MaximumScale = axisContext.XMaximum
            .MajorUnit = 0.5R
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
            .HasTitle = False
            .HasMajorGridlines = False
            .HasMinorGridlines = False
            .Crosses = XlAxisCrosses.xlAxisCrossesMinimum
        End With
        FormatAxisLine(xAxis, appearance.ShowXAxisLine, appearance)

        Dim yAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)
        With yAxis
            .MinimumScale = axisContext.YMinimum
            .MaximumScale = axisContext.YMaximum
            .MajorUnit = axisContext.YMajorUnit
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionLow
            .TickLabels.Font.Size = appearance.YAxisLabelFontSize
            .TickLabels.Font.Color = appearance.TextColor
            .TickLabels.NumberFormat = axisContext.YNumberFormat
            .HasTitle = Not String.IsNullOrWhiteSpace(appearance.YAxisTitle)
            If .HasTitle Then
                .AxisTitle.Text = appearance.YAxisTitle.Trim()
                .AxisTitle.Font.Color = appearance.TextColor
            End If
            .HasMajorGridlines = appearance.ShowHorizontalGridlines
            .HasMinorGridlines = False
            .Crosses = XlAxisCrosses.xlAxisCrossesMinimum
        End With
        FormatGridlines(yAxis, appearance.ShowHorizontalGridlines, appearance)
        FormatAxisLine(yAxis, appearance.ShowYAxisLine, appearance)
    End Sub

    Private Shared Sub FormatGridlines(axis As Object,
                                       showGridlines As Boolean,
                                       appearance As LadderPlotAppearance)
        If Not showGridlines Then Return
        Try
            With axis.MajorGridlines.Format.Line
                .Visible = True
                .ForeColor.RGB = appearance.GridlineColor
                .Weight = appearance.GridlineWeight
            End With
        Catch
            Try
                axis.MajorGridlines.Border.Color = appearance.GridlineColor
                axis.MajorGridlines.Border.Weight = appearance.GridlineWeight
            Catch
            End Try
        End Try
    End Sub

    Private Shared Sub FormatAxisLine(axis As Object,
                                      showLine As Boolean,
                                      appearance As LadderPlotAppearance)
        Try
            With axis.Format.Line
                .Visible = showLine
                If showLine Then
                    .ForeColor.RGB = appearance.AxisLineColor
                    .Weight = appearance.AxisLineWeight
                End If
            End With
        Catch
        End Try
    End Sub

    Private Shared Sub ConfigureTitle(chart As Chart, appearance As LadderPlotAppearance)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) Then
            chart.HasTitle = False
            Return
        End If

        chart.HasTitle = True
        chart.ChartTitle.Text = appearance.ChartTitle.Trim()
        chart.ChartTitle.Font.Size = appearance.ChartTitleFontSize
        chart.ChartTitle.Font.Color = appearance.TextColor
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As LadderPlotAppearance)
        With chart.ChartArea.Format.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = appearance.BackgroundColor
        End With
        chart.ChartArea.Format.Line.Visible = False

        Try
            With chart.PlotArea.Format.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = appearance.BackgroundColor
            End With
            chart.PlotArea.Format.Line.Visible = False
        Catch
        End Try
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       appearance As LadderPlotAppearance)
        If Not appearance.ShowLegend OrElse keepSeriesIndices Is Nothing OrElse keepSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = appearance.LegendPosition

        Dim keep As New HashSet(Of Integer)(keepSeriesIndices)
        Try
            Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)
            For i As Integer = entries.Count To 1 Step -1
                If Not keep.Contains(i) Then
                    DirectCast(entries.Item(i), LegendEntry).Delete()
                End If
            Next
            chart.Legend.Font.Size = appearance.LegendFontSize
            chart.Legend.Font.Color = appearance.TextColor
        Catch
            'Legend cleanup is cosmetic. Do not fail a successfully rendered chart.
        End Try
    End Sub

    Private Shared Function NiceStep125(rawStep As Double) As Double
        If Not IsFinitePositive(rawStep) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
        Dim scale As Double = Math.Pow(10.0R, exponent)
        Dim fraction As Double = rawStep / scale
        Dim niceFraction As Double

        If fraction <= 1.0R Then
            niceFraction = 1.0R
        ElseIf fraction <= 2.0R Then
            niceFraction = 2.0R
        ElseIf fraction <= 5.0R Then
            niceFraction = 5.0R
        Else
            niceFraction = 10.0R
        End If

        Return niceFraction * scale
    End Function

    Private Shared Function FloorToStep(value As Double, stepSize As Double) As Double
        Return Math.Floor(value / stepSize) * stepSize
    End Function

    Private Shared Function CeilingToStep(value As Double, stepSize As Double) As Double
        Return Math.Ceiling(value / stepSize) * stepSize
    End Function

    Private Shared Function Clamp(value As Double, minimum As Double, maximum As Double) As Double
        If value < minimum Then Return minimum
        If value > maximum Then Return maximum
        Return value
    End Function

    Private Shared Function ToMsoDashStyle(style As LadderLineStyle) As Integer
        Select Case style
            Case LadderLineStyle.Solid
                Return MsoLineSolid
            Case LadderLineStyle.Dash
                Return MsoLineDash
            Case LadderLineStyle.Dot
                Return MsoLineSquareDot
            Case LadderLineStyle.DashDot
                Return MsoLineDashDot
            Case LadderLineStyle.DashDotDot
                Return MsoLineDashDotDot
            Case Else
                Return MsoLineSolid
        End Select
    End Function

    Private Shared Sub ValidateResultCompatibility(result As LadderPlotResult,
                                                   appearance As LadderPlotAppearance)
        If appearance.ColorMode = LadderColorMode.ByGroup AndAlso Not result.HasGroups Then
            Throw New ArgumentException("ByGroup colouring requires a grouping variable in the ladder result.", NameOf(appearance))
        End If
    End Sub

    Private Shared Sub ValidateAppearance(appearance As LadderPlotAppearance)
        If Not [Enum].IsDefined(GetType(LadderColorMode), appearance.ColorMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ColorMode), "The ladder colour mode is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(LadderLineStyle), appearance.LineStyle) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LineStyle), "The ladder line style is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(LadderEndpointLabelContent), appearance.EndpointLabelContent) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.EndpointLabelContent), "The endpoint label content is not defined.")
        End If

        If appearance.PaletteColors Is Nothing OrElse appearance.PaletteColors.Length = 0 Then
            Throw New ArgumentException("At least one palette colour is required.", NameOf(appearance.PaletteColors))
        End If

        If Not appearance.ShowLines AndAlso Not appearance.ShowMarkers Then
            Throw New ArgumentException("At least one of ShowLines or ShowMarkers must be enabled.", NameOf(appearance))
        End If
        If appearance.ShowLines Then
            If Not IsFinitePositive(appearance.LineWeight) Then
                Throw New ArgumentOutOfRangeException(NameOf(appearance.LineWeight), "Line weight must be finite and positive.")
            End If
            If Not IsFiniteUnitInterval(appearance.LineTransparency) Then
                Throw New ArgumentOutOfRangeException(NameOf(appearance.LineTransparency), "Line transparency must be in the interval [0, 1].")
            End If
        End If
        If appearance.ShowMarkers Then
            If appearance.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone Then
                Throw New ArgumentException("A visible marker style is required when ShowMarkers=True.", NameOf(appearance.MarkerStyle))
            End If
            If appearance.MarkerSize < 2 OrElse appearance.MarkerSize > 72 Then
                Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerSize), "Marker size must be between 2 and 72 points.")
            End If
        End If

        If Not IsFinitePositive(appearance.LegendFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LegendFontSize), "Legend font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.EndpointLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.EndpointLabelFontSize), "Endpoint label font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.EndpointLabelMinimumSeparationPoints) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.EndpointLabelMinimumSeparationPoints), "Endpoint label minimum separation must be finite and positive.")
        End If
        If Not IsFinite(appearance.EndpointLabelAnchorGap) OrElse
           appearance.EndpointLabelAnchorGap < 0.0R OrElse appearance.EndpointLabelAnchorGap > 0.25R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.EndpointLabelAnchorGap), "Endpoint label anchor gap must be finite and between 0 and 0.25.")
        End If
        If Not IsFinitePositive(appearance.SideLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SideLabelFontSize), "Side label font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.YAxisLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisLabelFontSize), "Y-axis label font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.ChartTitleFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ChartTitleFontSize), "Chart title font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.GridlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GridlineWeight), "Gridline weight must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.AxisLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.AxisLineWeight), "Axis line weight must be finite and positive.")
        End If

        If Double.IsNaN(appearance.YAxisPaddingFraction) OrElse Double.IsInfinity(appearance.YAxisPaddingFraction) OrElse
           appearance.YAxisPaddingFraction < 0.0R OrElse appearance.YAxisPaddingFraction > 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisPaddingFraction),
                                                  "YAxisPaddingFraction must be finite and in the interval [0, 0.5].")
        End If

        If appearance.YAxisMinimum.HasValue AndAlso Not IsFinite(appearance.YAxisMinimum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisMinimum), "Y-axis minimum must be finite.")
        End If
        If appearance.YAxisMaximum.HasValue AndAlso Not IsFinite(appearance.YAxisMaximum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisMaximum), "Y-axis maximum must be finite.")
        End If
        If appearance.YAxisMinimum.HasValue AndAlso appearance.YAxisMaximum.HasValue AndAlso
           appearance.YAxisMinimum.Value >= appearance.YAxisMaximum.Value Then
            Throw New ArgumentException("YAxisMinimum must be smaller than YAxisMaximum.", NameOf(appearance))
        End If
        If appearance.YAxisMajorUnit.HasValue AndAlso Not IsFinitePositive(appearance.YAxisMajorUnit.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisMajorUnit), "Y-axis major unit must be finite and positive.")
        End If
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    Private Shared Function IsFinitePositive(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value > 0.0F
    End Function

    Private Shared Function IsFiniteUnitInterval(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value >= 0.0F AndAlso value <= 1.0F
    End Function
End Class
