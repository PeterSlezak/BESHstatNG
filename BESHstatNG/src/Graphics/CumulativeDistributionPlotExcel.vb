Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Controls which entries are retained in the Excel legend when an empirical CDF
''' and fitted theoretical CDF are displayed together.
''' </summary>
Public Enum CumulativeDistributionLegendMode
    ''' <summary>
    ''' Keep one legend entry per sample/group. The step and fitted line share the
    ''' same colour, so the legend identifies samples rather than curve types.
    ''' </summary>
    GroupsOnly = 0

    ''' <summary>
    ''' Keep separate legend entries for the empirical and fitted curves.
    ''' </summary>
    AllCurves = 1
End Enum

''' <summary>
''' Line patterns available to the fitted CDF and percentile reference lines.
''' A BESHStatNG enum is used so this renderer does not require a compile-time
''' Microsoft.Office.Core reference merely to expose MsoLineDashStyle.
''' </summary>
Public Enum CumulativeDistributionLineStyle
    Solid = 0
    Dash = 1
    Dot = 2
    DashDot = 3
    DashDotDot = 4
End Enum

''' <summary>
''' Excel-specific display settings for <see cref="CumulativeDistributionPlotExcel"/>.
''' Numerical preparation remains in <see cref="CumulativeDistributionPlot"/>.
''' </summary>
Public Class CumulativeDistributionPlotAppearance
    ''' <summary>
    ''' Custom chart title. Leave blank for an automatic title based on the selected
    ''' plot mode, fitted distribution, and (for a single sample) series name.
    ''' </summary>
    Public Property ChartTitle As String = String.Empty

    ''' <summary>Optional X-axis title. Leave blank for "Value".</summary>
    Public Property XAxisTitle As String = String.Empty

    Public Property ShowLegend As Boolean = True
    Public Property LegendMode As CumulativeDistributionLegendMode = CumulativeDistributionLegendMode.GroupsOnly
    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionRight
    Public Property LegendFontSize As Double = 9.0R

    ''' <summary>
    ''' A single ungrouped series uses this colour. Grouped/overlaid plots use
    ''' <see cref="PaletteColors"/>.
    ''' </summary>
    Public Property SingleSeriesColor As Integer = &H404040

    ''' <summary>
    ''' OLE RGB/BGR colour values used in order for grouped/overlaid samples.
    ''' The palette repeats when there are more groups than colours.
    ''' </summary>
    Public Property PaletteColors As Integer() = {
        &HD59B5B,  'RGB(91,155,213)
        &H317DED,  'RGB(237,125,49)
        &HA5A5A5,  'RGB(165,165,165)
        &HC0FF,    'RGB(255,192,0)
        &HC47244,  'RGB(68,114,196)
        &H47AD70,  'RGB(112,173,71)
        &H89A64F,  'RGB(79,166,137)
        &H7960A5,  'RGB(165,96,121)
        &H8C6D46,  'RGB(70,109,140)
        &H6B9EC9   'RGB(201,158,107)
    }

    Public Property EmpiricalLineWeight As Single = 1.15F
    Public Property EmpiricalLineTransparency As Single = 0.0F
    Public Property EmpiricalLineStyle As CumulativeDistributionLineStyle = CumulativeDistributionLineStyle.Solid

    Public Property FittedLineWeight As Single = 1.75F
    Public Property FittedLineTransparency As Single = 0.0F
    Public Property FittedLineStyle As CumulativeDistributionLineStyle = CumulativeDistributionLineStyle.Solid

    ''' <summary>
    ''' Extend the empirical step horizontally to the resolved X-axis minimum and
    ''' maximum. This gives the familiar ECDF tails F(x)=0 below the first observation
    ''' and F(x)=1 (or the last plotting probability) above the last observation.
    ''' </summary>
    Public Property ExtendEmpiricalToAxisBounds As Boolean = True

    ''' <summary>Draw reference-line L shapes for percentiles prepared by the backend.</summary>
    Public Property ShowPercentileReferenceLines As Boolean = True

    Public Property PercentileLineWeight As Single = 0.85F
    Public Property PercentileLineTransparency As Single = 0.3F
    Public Property PercentileLineStyle As CumulativeDistributionLineStyle = CumulativeDistributionLineStyle.Dash

    ''' <summary>Label percentile intersections, for example "75% = 63.5".</summary>
    Public Property ShowPercentileLabels As Boolean = True

    ''' <summary>
    ''' On overlaid charts, prefix percentile labels with the series/group name.
    ''' </summary>
    Public Property IncludeSeriesNameInPercentileLabels As Boolean = True

    ''' <summary>.NET numeric format used for percentile X values.</summary>
    Public Property PercentileValueFormat As String = "0.###"

    Public Property PercentileLabelFontSize As Double = 8.0R

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property TextColor As Integer = &H404040
    Public Property GridlineColor As Integer = &HE6E6E6
    Public Property GridlineWeight As Single = 0.75F
    Public Property AxisLineColor As Integer = &H808080
    Public Property AxisLineWeight As Single = 0.75F

    Public Property ShowHorizontalGridlines As Boolean = True
    Public Property ShowVerticalGridlines As Boolean = False

    Public Property FontName As String = "Calibri"
    Public Property AxisLabelFontSize As Double = 9.0R
    Public Property AxisTitleFontSize As Double = 10.0R
    Public Property ChartTitleFontSize As Double = 12.0R

    ''' <summary>
    ''' Optional X major interval. Leave Nothing to obtain a visually clean interval
    ''' from BESHStatNG's existing chart-scaling helper.
    ''' </summary>
    Public Property XAxisMajorUnit As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Optional Excel number format for X-axis tick labels. Leave blank for an
    ''' automatic compact format derived from the resolved major interval.
    ''' </summary>
    Public Property XAxisNumberFormat As String = String.Empty

    Friend Function Copy() As CumulativeDistributionPlotAppearance
        Return New CumulativeDistributionPlotAppearance With {
            .ChartTitle = ChartTitle,
            .XAxisTitle = XAxisTitle,
            .ShowLegend = ShowLegend,
            .LegendMode = LegendMode,
            .LegendPosition = LegendPosition,
            .LegendFontSize = LegendFontSize,
            .SingleSeriesColor = SingleSeriesColor,
            .PaletteColors = If(PaletteColors Is Nothing, Nothing, DirectCast(PaletteColors.Clone(), Integer())),
            .EmpiricalLineWeight = EmpiricalLineWeight,
            .EmpiricalLineTransparency = EmpiricalLineTransparency,
            .EmpiricalLineStyle = EmpiricalLineStyle,
            .FittedLineWeight = FittedLineWeight,
            .FittedLineTransparency = FittedLineTransparency,
            .FittedLineStyle = FittedLineStyle,
            .ExtendEmpiricalToAxisBounds = ExtendEmpiricalToAxisBounds,
            .ShowPercentileReferenceLines = ShowPercentileReferenceLines,
            .PercentileLineWeight = PercentileLineWeight,
            .PercentileLineTransparency = PercentileLineTransparency,
            .PercentileLineStyle = PercentileLineStyle,
            .ShowPercentileLabels = ShowPercentileLabels,
            .IncludeSeriesNameInPercentileLabels = IncludeSeriesNameInPercentileLabels,
            .PercentileValueFormat = PercentileValueFormat,
            .PercentileLabelFontSize = PercentileLabelFontSize,
            .BackgroundColor = BackgroundColor,
            .TextColor = TextColor,
            .GridlineColor = GridlineColor,
            .GridlineWeight = GridlineWeight,
            .AxisLineColor = AxisLineColor,
            .AxisLineWeight = AxisLineWeight,
            .ShowHorizontalGridlines = ShowHorizontalGridlines,
            .ShowVerticalGridlines = ShowVerticalGridlines,
            .FontName = FontName,
            .AxisLabelFontSize = AxisLabelFontSize,
            .AxisTitleFontSize = AxisTitleFontSize,
            .ChartTitleFontSize = ChartTitleFontSize,
            .XAxisMajorUnit = XAxisMajorUnit,
            .XAxisNumberFormat = XAxisNumberFormat
        }
    End Function
End Class

''' <summary>
''' Excel renderer for the Excel-independent <see cref="CumulativeDistributionPlotResult"/>
''' and <see cref="CumulativeDistributionPlotSetResult"/> backends.
'''
''' Empirical CDFs are native XY-scatter step lines; fitted theoretical CDFs use
''' the dense renderer-ready coordinates prepared by the backend. Straight XY segments
''' preserve monotonicity and avoid Excel spline overshoot. Percentile reference lines are
''' also chart series, so
''' they remain aligned when the embedded chart is moved or resized.
''' </summary>
''' <remarks>
''' Series returned directly by SeriesCollection.NewSeries are used throughout. The
''' implementation intentionally does not retrieve a newly-created series via
''' SeriesCollection(SeriesCollection.Count), which is unreliable in some Excel/Interop
''' combinations.
''' </remarks>
Public NotInheritable Class CumulativeDistributionPlotExcel
    Private Sub New()
    End Sub

    'Stable Office MsoLineDashStyle values used only through late binding.
    Private Const MsoLineSolid As Integer = 1
    Private Const MsoLineSquareDot As Integer = 2
    Private Const MsoLineDash As Integer = 4
    Private Const MsoLineDashDot As Integer = 5
    Private Const MsoLineDashDotDot As Integer = 6

    Private NotInheritable Class AxisContext
        Friend XMinimum As Double
        Friend XMaximum As Double
        Friend XMajorUnit As Double
        Friend XNumberFormat As String
        Friend YMinimum As Double
        Friend YMaximum As Double
        Friend YMajorUnit As Double
        Friend YNumberFormat As String
        Friend YAxisTitle As String
    End Class

    ''' <summary>
    ''' Adds one CDF/ECDF chart for a single calculated sample.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As CumulativeDistributionPlotResult,
                                    Optional appearance As CumulativeDistributionPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 700.0R,
                                    Optional height As Double = 460.0R) As Chart
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        Return AddChartCore(ws,
                            New CumulativeDistributionPlotResult() {result},
                            appearance,
                            left,
                            top,
                            width,
                            height,
                            False)
    End Function

    ''' <summary>
    ''' Adds one overlaid chart for all groups/samples in a grouped backend result.
    ''' </summary>
    Public Shared Function AddOverlayChart(ws As Worksheet,
                                           resultSet As CumulativeDistributionPlotSetResult,
                                           Optional appearance As CumulativeDistributionPlotAppearance = Nothing,
                                           Optional left As Double = 20.0R,
                                           Optional top As Double = 20.0R,
                                           Optional width As Double = 760.0R,
                                           Optional height As Double = 480.0R) As Chart
        If resultSet Is Nothing Then Throw New ArgumentNullException(NameOf(resultSet))
        If resultSet.Count < 1 Then
            Throw New ArgumentException("The cumulative-distribution result set contains no series.", NameOf(resultSet))
        End If

        Return AddChartCore(ws,
                            resultSet.Series,
                            appearance,
                            left,
                            top,
                            width,
                            height,
                            True)
    End Function

    ''' <summary>
    ''' Adds one chart per group/sample, stacked vertically on the supplied worksheet.
    ''' The returned charts are in the same order as <see cref="CumulativeDistributionPlotSetResult.Series"/>.
    ''' </summary>
    Public Shared Function AddSeparateCharts(ws As Worksheet,
                                             resultSet As CumulativeDistributionPlotSetResult,
                                             Optional appearance As CumulativeDistributionPlotAppearance = Nothing,
                                             Optional left As Double = 20.0R,
                                             Optional top As Double = 20.0R,
                                             Optional width As Double = 700.0R,
                                             Optional height As Double = 440.0R,
                                             Optional verticalGap As Double = 18.0R) As Chart()
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If resultSet Is Nothing Then Throw New ArgumentNullException(NameOf(resultSet))
        If resultSet.Count < 1 Then
            Throw New ArgumentException("The cumulative-distribution result set contains no series.", NameOf(resultSet))
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
        If Not IsFinite(verticalGap) OrElse verticalGap < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(verticalGap), "Vertical gap must be finite and non-negative.")
        End If

        Dim results() As CumulativeDistributionPlotResult = resultSet.Series
        Dim charts(results.Length - 1) As Chart
        Dim nextTop As Double = top

        For i As Integer = 0 To results.Length - 1
            charts(i) = AddChart(ws,
                                 results(i),
                                 appearance,
                                 left,
                                 nextTop,
                                 width,
                                 height)
            nextTop += height + verticalGap
        Next

        Return charts
    End Function

    Private Shared Function AddChartCore(ws As Worksheet,
                                         results() As CumulativeDistributionPlotResult,
                                         appearance As CumulativeDistributionPlotAppearance,
                                         left As Double,
                                         top As Double,
                                         width As Double,
                                         height As Double,
                                         isOverlay As Boolean) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If results Is Nothing OrElse results.Length = 0 Then
            Throw New ArgumentException("At least one cumulative-distribution result is required.", NameOf(results))
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

        Dim resolvedAppearance As CumulativeDistributionPlotAppearance =
            If(appearance Is Nothing, New CumulativeDistributionPlotAppearance(), appearance.Copy())
        ValidateAppearance(resolvedAppearance)
        ValidateResults(results)

        Dim axisContext As AxisContext = ResolveAxisContext(results, resolvedAppearance)
        Dim chartShape As Shape = Nothing

        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatterLinesNoMarkers,
                                            left,
                                            top,
                                            width,
                                            height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatterLinesNoMarkers
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)

            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart,
                           ResolveChartTitle(results, resolvedAppearance, isOverlay),
                           resolvedAppearance)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim groupCount As Integer = results.Length

            For i As Integer = 0 To results.Length - 1
                Dim result As CumulativeDistributionPlotResult = results(i)
                Dim groupColor As Integer = ResolveSeriesColor(i, groupCount, resolvedAppearance)
                Dim options As CumulativeDistributionPlotOptions = result.Options
                Dim empiricalSeries As Series = Nothing
                Dim fittedSeries As Series = Nothing

                If result.EmpiricalX.Length > 0 Then
                    empiricalSeries = AddEmpiricalSeries(seriesCollection,
                                                         result,
                                                         groupColor,
                                                         axisContext,
                                                         resolvedAppearance,
                                                         results.Length)
                End If

                If result.FittedX.Length > 0 Then
                    fittedSeries = AddFittedSeries(seriesCollection,
                                                   result,
                                                   groupColor,
                                                   resolvedAppearance,
                                                   results.Length)
                End If

                RegisterLegendSeries(seriesCollection,
                                     empiricalSeries,
                                     fittedSeries,
                                     result,
                                     options,
                                     resolvedAppearance,
                                     legendSeriesIndices)

                If resolvedAppearance.ShowPercentileReferenceLines AndAlso result.Percentiles.Length > 0 Then
                    AddPercentileReferenceSeries(seriesCollection,
                                                 result,
                                                 groupColor,
                                                 axisContext,
                                                 resolvedAppearance,
                                                 results.Length > 1)
                End If
            Next

            ConfigureAxes(chart, axisContext, resolvedAppearance)
            chart.Refresh()

            'Excel can reapply chart-style defaults on the first materialisation.
            'Reassert the axis/background settings before pruning helper legend entries.
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureAxes(chart, axisContext, resolvedAppearance)
            ConfigureLegend(chart, legendSeriesIndices, resolvedAppearance)

            chart.Refresh()
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

    Private Shared Function AddEmpiricalSeries(seriesCollection As SeriesCollection,
                                               result As CumulativeDistributionPlotResult,
                                               seriesColor As Integer,
                                               axisContext As AxisContext,
                                               appearance As CumulativeDistributionPlotAppearance,
                                               resultCount As Integer) As Series
        Dim sourceX() As Double = result.EmpiricalX
        Dim sourceY() As Double = result.EmpiricalY
        Dim xValues() As Double = sourceX
        Dim yValues() As Double = sourceY

        If appearance.ExtendEmpiricalToAxisBounds AndAlso sourceX.Length > 0 Then
            ExtendStepToAxisBounds(sourceX,
                                   sourceY,
                                   axisContext.XMinimum,
                                   axisContext.XMaximum,
                                   xValues,
                                   yValues)
        End If

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = ResolveEmpiricalSeriesName(result, appearance, resultCount)
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = seriesColor
            .Format.Line.Weight = appearance.EmpiricalLineWeight
            .Format.Line.Transparency = appearance.EmpiricalLineTransparency
        End With

        Try
            series.Smooth = False
        Catch
        End Try
        Try
            Dim lineObject As Object = series.Format.Line
            lineObject.DashStyle = ToMsoDashStyle(appearance.EmpiricalLineStyle)
        Catch
        End Try

        Return DirectCast(series, Series)
    End Function

    Private Shared Function AddFittedSeries(seriesCollection As SeriesCollection,
                                            result As CumulativeDistributionPlotResult,
                                            seriesColor As Integer,
                                            appearance As CumulativeDistributionPlotAppearance,
                                            resultCount As Integer) As Series
        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = ResolveFittedSeriesName(result, appearance, resultCount)
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = result.FittedX
            .Values = result.FittedY
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = seriesColor
            .Format.Line.Weight = appearance.FittedLineWeight
            .Format.Line.Transparency = appearance.FittedLineTransparency
        End With

        Try
            'The backend normally supplies 241 fitted points. Keep Excel smoothing off:
            'its spline can overshoot a monotone CDF between points.
            series.Smooth = False
        Catch
        End Try
        Try
            Dim lineObject As Object = series.Format.Line
            lineObject.DashStyle = ToMsoDashStyle(appearance.FittedLineStyle)
        Catch
        End Try

        Return DirectCast(series, Series)
    End Function

    Private Shared Sub AddPercentileReferenceSeries(seriesCollection As SeriesCollection,
                                                     result As CumulativeDistributionPlotResult,
                                                     seriesColor As Integer,
                                                     axisContext As AxisContext,
                                                     appearance As CumulativeDistributionPlotAppearance,
                                                     isMultiSeries As Boolean)
        Dim percentiles() As CumulativeDistributionPercentile = result.Percentiles

        For Each percentile As CumulativeDistributionPercentile In percentiles
            Dim xCorner As Double = percentile.XValue
            Dim yCorner As Double = percentile.YValue

            If Not IsFinite(xCorner) OrElse Not IsFinite(yCorner) Then Continue For

            Dim xValues() As Double = {axisContext.XMinimum, xCorner, xCorner}
            Dim yValues() As Double = {yCorner, yCorner, axisContext.YMinimum}

            Dim series As Object = seriesCollection.NewSeries()
            With series
                .Name = "Percentile reference"
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                .XValues = xValues
                .Values = yValues
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = seriesColor
                .Format.Line.Weight = appearance.PercentileLineWeight
                .Format.Line.Transparency = appearance.PercentileLineTransparency
            End With

            Try
                Dim lineObject As Object = series.Format.Line
                lineObject.DashStyle = ToMsoDashStyle(appearance.PercentileLineStyle)
            Catch
            End Try

            If appearance.ShowPercentileLabels Then
                AddPercentileLabel(series,
                                   percentile,
                                   result.SeriesName,
                                   seriesColor,
                                   appearance,
                                   isMultiSeries)
            End If
        Next
    End Sub

    Private Shared Sub AddPercentileLabel(series As Object,
                                          percentile As CumulativeDistributionPercentile,
                                          seriesName As String,
                                          seriesColor As Integer,
                                          appearance As CumulativeDistributionPlotAppearance,
                                          isMultiSeries As Boolean)
        Try
            Dim point As Object = series.Points(2)
            point.ApplyDataLabels()

            Dim labelText As String = FormatPercentileLabel(percentile,
                                                            seriesName,
                                                            appearance,
                                                            isMultiSeries)
            With point.DataLabel
                .Text = labelText
                .Position = XlDataLabelPosition.xlLabelPositionAbove
                .Font.Name = appearance.FontName
                .Font.Size = appearance.PercentileLabelFontSize
                .Font.Color = seriesColor
                .Font.Bold = False
            End With

            Try
                point.DataLabel.Format.Fill.Visible = False
                point.DataLabel.Format.Line.Visible = False
            Catch
            End Try
        Catch
            'Percentile lines are the statistical content. If an older Excel build
            'refuses a point-level data label, keep the reference line and continue.
        End Try
    End Sub

    Private Shared Function FormatPercentileLabel(percentile As CumulativeDistributionPercentile,
                                                  seriesName As String,
                                                  appearance As CumulativeDistributionPlotAppearance,
                                                  isMultiSeries As Boolean) As String
        Dim percentText As String
        If Math.Abs(percentile.Percent - Math.Round(percentile.Percent)) < 1.0E-10R Then
            percentText = Math.Round(percentile.Percent).ToString("0", CultureInfo.CurrentCulture) & "%"
        Else
            percentText = percentile.Percent.ToString("0.###", CultureInfo.CurrentCulture) & "%"
        End If

        Dim valueFormat As String = If(String.IsNullOrWhiteSpace(appearance.PercentileValueFormat),
                                       "0.###",
                                       appearance.PercentileValueFormat.Trim())
        Dim valueText As String = percentile.XValue.ToString(valueFormat, CultureInfo.CurrentCulture)
        Dim body As String = percentText & " = " & valueText

        If isMultiSeries AndAlso appearance.IncludeSeriesNameInPercentileLabels AndAlso
           Not String.IsNullOrWhiteSpace(seriesName) Then
            Return seriesName.Trim() & ": " & body
        End If

        Return body
    End Function

    Private Shared Sub RegisterLegendSeries(seriesCollection As SeriesCollection,
                                            empiricalSeries As Series,
                                            fittedSeries As Series,
                                            result As CumulativeDistributionPlotResult,
                                            options As CumulativeDistributionPlotOptions,
                                            appearance As CumulativeDistributionPlotAppearance,
                                            legendSeriesIndices As IList(Of Integer))
        If Not appearance.ShowLegend Then Return

        If appearance.LegendMode = CumulativeDistributionLegendMode.AllCurves Then
            If empiricalSeries IsNot Nothing Then legendSeriesIndices.Add(empiricalSeries.PlotOrder)
            If fittedSeries IsNot Nothing Then legendSeriesIndices.Add(fittedSeries.PlotOrder)
            Return
        End If

        'GroupsOnly: when both curves are present, the fitted curve is the legend
        'representative because it carries the fully opaque group colour. For ECDF-only
        'or fitted-only charts, keep whichever curve exists.
        If fittedSeries IsNot Nothing Then
            fittedSeries.Name = ResolveGroupLegendName(result)
            legendSeriesIndices.Add(fittedSeries.PlotOrder)
        ElseIf empiricalSeries IsNot Nothing Then
            empiricalSeries.Name = ResolveGroupLegendName(result)
            legendSeriesIndices.Add(empiricalSeries.PlotOrder)
        End If
    End Sub

    Private Shared Sub ExtendStepToAxisBounds(sourceX() As Double,
                                              sourceY() As Double,
                                              xMinimum As Double,
                                              xMaximum As Double,
                                              ByRef xOut() As Double,
                                              ByRef yOut() As Double)
        If sourceX Is Nothing OrElse sourceY Is Nothing OrElse sourceX.Length = 0 OrElse
           sourceX.Length <> sourceY.Length Then
            xOut = sourceX
            yOut = sourceY
            Return
        End If

        Dim prepend As Boolean = xMinimum < sourceX(0)
        Dim append As Boolean = xMaximum > sourceX(sourceX.Length - 1)
        Dim extra As Integer = If(prepend, 1, 0) + If(append, 1, 0)

        If extra = 0 Then
            xOut = sourceX
            yOut = sourceY
            Return
        End If

        ReDim xOut(sourceX.Length + extra - 1)
        ReDim yOut(sourceY.Length + extra - 1)

        Dim targetIndex As Integer = 0
        If prepend Then
            xOut(targetIndex) = xMinimum
            yOut(targetIndex) = sourceY(0)
            targetIndex += 1
        End If

        For i As Integer = 0 To sourceX.Length - 1
            xOut(targetIndex) = sourceX(i)
            yOut(targetIndex) = sourceY(i)
            targetIndex += 1
        Next

        If append Then
            xOut(targetIndex) = xMaximum
            yOut(targetIndex) = sourceY(sourceY.Length - 1)
        End If
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     axisContext As AxisContext,
                                     appearance As CumulativeDistributionPlotAppearance)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
        Dim yAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)

        With xAxis
            .MinimumScale = axisContext.XMinimum
            .MaximumScale = axisContext.XMaximum
            .MajorUnit = axisContext.XMajorUnit
            .TickLabels.NumberFormat = axisContext.XNumberFormat
            .TickLabels.Font.Name = appearance.FontName
            .TickLabels.Font.Size = appearance.AxisLabelFontSize
            .TickLabels.Font.Color = appearance.TextColor
            .MajorTickMark = XlTickMark.xlTickMarkOutside
            .MinorTickMark = XlTickMark.xlTickMarkNone
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNextToAxis
            .HasTitle = True
            .AxisTitle.Text = If(String.IsNullOrWhiteSpace(appearance.XAxisTitle),
                                 "Value",
                                 appearance.XAxisTitle.Trim())
            .AxisTitle.Font.Name = appearance.FontName
            .AxisTitle.Font.Size = appearance.AxisTitleFontSize
            .AxisTitle.Font.Color = appearance.TextColor
            .HasMajorGridlines = appearance.ShowVerticalGridlines
        End With

        With yAxis
            .MinimumScale = axisContext.YMinimum
            .MaximumScale = axisContext.YMaximum
            .MajorUnit = axisContext.YMajorUnit
            .TickLabels.NumberFormat = axisContext.YNumberFormat
            .TickLabels.Font.Name = appearance.FontName
            .TickLabels.Font.Size = appearance.AxisLabelFontSize
            .TickLabels.Font.Color = appearance.TextColor
            .MajorTickMark = XlTickMark.xlTickMarkOutside
            .MinorTickMark = XlTickMark.xlTickMarkNone
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNextToAxis
            .HasTitle = True
            .AxisTitle.Text = axisContext.YAxisTitle
            .AxisTitle.Font.Name = appearance.FontName
            .AxisTitle.Font.Size = appearance.AxisTitleFontSize
            .AxisTitle.Font.Color = appearance.TextColor
            .HasMajorGridlines = appearance.ShowHorizontalGridlines
        End With

        'Keep the Cartesian axes at the lower/left plot boundaries.
        '
        'Excel's CrossesAt value belongs to the axis being crossed, not to the
        'axis that is moved. Therefore the value axis controls where the
        'horizontal (category/X) axis crosses, while the category/X axis controls
        'where the vertical (value/Y) axis crosses. Keep these assignments in this
        'order so the horizontal axis is always drawn at YMinimum (0% or 0.0).
        Try
            yAxis.CrossesAt = axisContext.YMinimum
        Catch
        End Try
        Try
            xAxis.CrossesAt = axisContext.XMinimum
        Catch
        End Try

        FormatAxisLine(xAxis, appearance)
        FormatAxisLine(yAxis, appearance)
        FormatGridlines(xAxis, appearance)
        FormatGridlines(yAxis, appearance)
    End Sub

    Private Shared Sub FormatAxisLine(axis As Object,
                                      appearance As CumulativeDistributionPlotAppearance)
        Try
            With axis.Format.Line
                .Visible = True
                .ForeColor.RGB = appearance.AxisLineColor
                .Weight = appearance.AxisLineWeight
            End With
        Catch
        End Try
    End Sub

    Private Shared Sub FormatGridlines(axis As Object,
                                       appearance As CumulativeDistributionPlotAppearance)
        Try
            If axis.HasMajorGridlines Then
                With axis.MajorGridlines.Format.Line
                    .Visible = True
                    .ForeColor.RGB = appearance.GridlineColor
                    .Weight = appearance.GridlineWeight
                End With
            End If
        Catch
        End Try
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object,
                                           appearance As CumulativeDistributionPlotAppearance)
        Try
            With chart.ChartArea.Format.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = appearance.BackgroundColor
                .Transparency = 0.0F
            End With
            chart.ChartArea.Format.Line.Visible = False
        Catch
        End Try

        Try
            With chart.PlotArea.Format.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = appearance.BackgroundColor
                .Transparency = 0.0F
            End With
            chart.PlotArea.Format.Line.Visible = False
        Catch
        End Try
    End Sub

    Private Shared Sub ConfigureTitle(chart As Chart,
                                      title As String,
                                      appearance As CumulativeDistributionPlotAppearance)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
            Return
        End If

        chart.HasTitle = True
        chart.ChartTitle.Text = title
        chart.ChartTitle.Font.Name = appearance.FontName
        chart.ChartTitle.Font.Size = appearance.ChartTitleFontSize
        chart.ChartTitle.Font.Color = appearance.TextColor
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       appearance As CumulativeDistributionPlotAppearance)
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
            chart.Legend.Font.Name = appearance.FontName
            chart.Legend.Font.Size = appearance.LegendFontSize
            chart.Legend.Font.Color = appearance.TextColor
        Catch
            'Legend formatting is cosmetic. Return the statistical chart even on an
            'Excel build that does not expose deletable LegendEntries reliably.
        End Try
    End Sub

    Private Shared Function ResolveAxisContext(results() As CumulativeDistributionPlotResult,
                                               appearance As CumulativeDistributionPlotAppearance) As AxisContext
        Dim xMinimum As Double = Double.PositiveInfinity
        Dim xMaximum As Double = Double.NegativeInfinity
        Dim explicitMinimum As Nullable(Of Double) = Nothing
        Dim explicitMaximum As Nullable(Of Double) = Nothing
        Dim yScale As CumulativeDistributionYScale = results(0).Options.YScale

        For Each result As CumulativeDistributionPlotResult In results
            xMinimum = Math.Min(xMinimum, result.XMinimum)
            xMaximum = Math.Max(xMaximum, result.XMaximum)

            Dim options As CumulativeDistributionPlotOptions = result.Options
            If options.XMinimum.HasValue Then explicitMinimum = options.XMinimum.Value
            If options.XMaximum.HasValue Then explicitMaximum = options.XMaximum.Value
        Next

        If Not IsFinite(xMinimum) OrElse Not IsFinite(xMaximum) OrElse xMaximum <= xMinimum Then
            Throw New ArgumentException("The cumulative-distribution result does not contain a finite X range.", NameOf(results))
        End If

        Dim scale As graphics.CHARTscale = graphics.ChartingFunc.ChartScaling(xMinimum, xMaximum)
        Dim resolvedMinimum As Double = If(explicitMinimum.HasValue, explicitMinimum.Value, scale.Min)
        Dim resolvedMaximum As Double = If(explicitMaximum.HasValue, explicitMaximum.Value, scale.Max)

        If resolvedMaximum <= resolvedMinimum Then
            Throw New ArgumentException("The resolved CDF X-axis maximum must be larger than its minimum.", NameOf(results))
        End If

        Dim majorUnit As Double
        If appearance.XAxisMajorUnit.HasValue Then
            majorUnit = appearance.XAxisMajorUnit.Value
        Else
            Dim resolvedScale As graphics.CHARTscale = graphics.ChartingFunc.ChartScaling(resolvedMinimum, resolvedMaximum)
            majorUnit = resolvedScale.Scale
        End If

        If Not IsFinitePositive(majorUnit) Then
            majorUnit = Math.Max((resolvedMaximum - resolvedMinimum) / 5.0R, 1.0R)
        End If

        Dim xNumberFormat As String = If(String.IsNullOrWhiteSpace(appearance.XAxisNumberFormat),
                                         AutomaticNumberFormat(majorUnit),
                                         appearance.XAxisNumberFormat.Trim())

        Dim context As New AxisContext With {
            .XMinimum = resolvedMinimum,
            .XMaximum = resolvedMaximum,
            .XMajorUnit = majorUnit,
            .XNumberFormat = xNumberFormat
        }

        If yScale = CumulativeDistributionYScale.Percent Then
            context.YMinimum = 0.0R
            context.YMaximum = 100.0R
            context.YMajorUnit = 20.0R
            context.YNumberFormat = "0"
            context.YAxisTitle = "Percent"
        Else
            context.YMinimum = 0.0R
            context.YMaximum = 1.0R
            context.YMajorUnit = 0.2R
            context.YNumberFormat = "0.0"
            context.YAxisTitle = "Cumulative probability"
        End If

        Return context
    End Function

    Private Shared Function ResolveChartTitle(results() As CumulativeDistributionPlotResult,
                                              appearance As CumulativeDistributionPlotAppearance,
                                              isOverlay As Boolean) As String
        If Not String.IsNullOrWhiteSpace(appearance.ChartTitle) Then Return appearance.ChartTitle.Trim()

        Dim options As CumulativeDistributionPlotOptions = results(0).Options
        Dim baseTitle As String

        Select Case options.Mode
            Case CumulativeDistributionPlotMode.EmpiricalOnly
                baseTitle = "Empirical cumulative distribution"
            Case CumulativeDistributionPlotMode.FittedDistributionOnly
                baseTitle = DistributionShortName(options.Distribution) & " cumulative distribution"
            Case Else
                baseTitle = "Empirical CDF with " & DistributionShortName(options.Distribution) & " fit"
        End Select

        If Not isOverlay AndAlso results.Length = 1 AndAlso Not String.IsNullOrWhiteSpace(results(0).SeriesName) Then
            baseTitle &= " of " & results(0).SeriesName.Trim()
        End If

        Return baseTitle
    End Function

    Private Shared Function ResolveSeriesColor(index As Integer,
                                               seriesCount As Integer,
                                               appearance As CumulativeDistributionPlotAppearance) As Integer
        If seriesCount <= 1 Then Return appearance.SingleSeriesColor

        If appearance.PaletteColors Is Nothing OrElse appearance.PaletteColors.Length = 0 Then
            Return graphics.ChartingFunc.GetColor(index + 1)
        End If

        Return appearance.PaletteColors(index Mod appearance.PaletteColors.Length)
    End Function

    Private Shared Function ResolveEmpiricalSeriesName(result As CumulativeDistributionPlotResult,
                                                       appearance As CumulativeDistributionPlotAppearance,
                                                       resultCount As Integer) As String
        Dim groupName As String = ResolveGroupLegendName(result)
        If appearance.LegendMode = CumulativeDistributionLegendMode.GroupsOnly Then Return groupName
        Return groupName & " - ECDF"
    End Function

    Private Shared Function ResolveFittedSeriesName(result As CumulativeDistributionPlotResult,
                                                    appearance As CumulativeDistributionPlotAppearance,
                                                    resultCount As Integer) As String
        Dim groupName As String = ResolveGroupLegendName(result)
        If appearance.LegendMode = CumulativeDistributionLegendMode.GroupsOnly Then Return groupName

        Dim fitName As String
        If result.Fit IsNot Nothing Then
            fitName = DistributionShortName(result.Fit.Distribution)
        Else
            fitName = DistributionShortName(result.Options.Distribution)
        End If
        Return groupName & " - " & fitName & " fit"
    End Function

    Private Shared Function ResolveGroupLegendName(result As CumulativeDistributionPlotResult) As String
        If result Is Nothing OrElse String.IsNullOrWhiteSpace(result.SeriesName) Then Return "Data"
        Return result.SeriesName.Trim()
    End Function

    Private Shared Function DistributionShortName(kind As CumulativeDistributionKind) As String
        Select Case kind
            Case CumulativeDistributionKind.Normal
                Return "Normal"
            Case CumulativeDistributionKind.Lognormal
                Return "Lognormal"
            Case CumulativeDistributionKind.ThreeParameterLognormal
                Return "3-parameter lognormal"
            Case CumulativeDistributionKind.Gamma
                Return "Gamma"
            Case CumulativeDistributionKind.ThreeParameterGamma
                Return "3-parameter gamma"
            Case CumulativeDistributionKind.Exponential
                Return "Exponential"
            Case CumulativeDistributionKind.TwoParameterExponential
                Return "2-parameter exponential"
            Case CumulativeDistributionKind.SmallestExtremeValue
                Return "Smallest extreme value"
            Case CumulativeDistributionKind.Weibull
                Return "Weibull"
            Case CumulativeDistributionKind.ThreeParameterWeibull
                Return "3-parameter Weibull"
            Case CumulativeDistributionKind.LargestExtremeValue
                Return "Largest extreme value"
            Case CumulativeDistributionKind.Logistic
                Return "Logistic"
            Case CumulativeDistributionKind.Loglogistic
                Return "Loglogistic"
            Case CumulativeDistributionKind.ThreeParameterLoglogistic
                Return "3-parameter loglogistic"
            Case Else
                Return kind.ToString()
        End Select
    End Function

    Private Shared Function AutomaticNumberFormat(majorUnit As Double) As String
        If Not IsFinitePositive(majorUnit) Then Return "0.###"

        Dim absoluteUnit As Double = Math.Abs(majorUnit)
        If absoluteUnit >= 1.0R Then Return "0.###"
        If absoluteUnit >= 0.1R Then Return "0.0##"
        If absoluteUnit >= 0.01R Then Return "0.00##"
        If absoluteUnit >= 0.001R Then Return "0.000##"
        Return "0.#####"
    End Function

    Private Shared Sub ValidateResults(results() As CumulativeDistributionPlotResult)
        If results Is Nothing OrElse results.Length = 0 Then
            Throw New ArgumentException("At least one cumulative-distribution result is required.", NameOf(results))
        End If

        Dim firstOptions As CumulativeDistributionPlotOptions = Nothing
        Dim failures As New List(Of String)()

        For i As Integer = 0 To results.Length - 1
            Dim result As CumulativeDistributionPlotResult = results(i)
            If result Is Nothing Then
                Throw New ArgumentException("The cumulative-distribution result set contains a Nothing entry.", NameOf(results))
            End If
            If result.ValidCount < 1 Then
                Throw New ArgumentException("A cumulative-distribution result contains no valid observations.", NameOf(results))
            End If
            If Not IsFinite(result.XMinimum) OrElse Not IsFinite(result.XMaximum) OrElse result.XMaximum <= result.XMinimum Then
                Throw New ArgumentException("A cumulative-distribution result contains invalid X-axis bounds.", NameOf(results))
            End If

            ValidateCoordinatePair(result.EmpiricalX, result.EmpiricalY, "empirical CDF")
            ValidateCoordinatePair(result.FittedX, result.FittedY, "fitted CDF")

            Dim options As CumulativeDistributionPlotOptions = result.Options
            If firstOptions Is Nothing Then
                firstOptions = options
            ElseIf options.YScale <> firstOptions.YScale Then
                Throw New ArgumentException("All overlaid cumulative-distribution results must use the same Y scale.", NameOf(results))
            End If

            If options.Mode = CumulativeDistributionPlotMode.EmpiricalOnly Then
                If result.EmpiricalX.Length = 0 Then
                    failures.Add(ResolveGroupLegendName(result) & ": empirical coordinates are unavailable.")
                End If
            ElseIf options.Mode = CumulativeDistributionPlotMode.FittedDistributionOnly Then
                If result.FittedX.Length = 0 Then
                    Dim fitMessage As String = If(result.Fit Is Nothing,
                                                  "fitted coordinates are unavailable.",
                                                  result.Fit.Message)
                    failures.Add(ResolveGroupLegendName(result) & ": " & fitMessage)
                End If
            Else
                If result.EmpiricalX.Length = 0 Then
                    failures.Add(ResolveGroupLegendName(result) & ": empirical coordinates are unavailable.")
                ElseIf result.FittedX.Length = 0 Then
                    Dim fitMessage As String = If(result.Fit Is Nothing,
                                                  "fitted coordinates are unavailable.",
                                                  result.Fit.Message)
                    failures.Add(ResolveGroupLegendName(result) & ": " & fitMessage)
                End If
            End If
        Next

        If failures.Count > 0 Then
            Throw New InvalidOperationException("The CDF/ECDF chart cannot be rendered: " & String.Join(" ", failures.ToArray()))
        End If
    End Sub

    Private Shared Sub ValidateCoordinatePair(xValues() As Double,
                                              yValues() As Double,
                                              label As String)
        If xValues Is Nothing OrElse yValues Is Nothing Then
            Throw New ArgumentException("The " & label & " coordinate arrays cannot be Nothing.")
        End If
        If xValues.Length <> yValues.Length Then
            Throw New ArgumentException("The " & label & " X and Y arrays must have the same length.")
        End If

        For i As Integer = 0 To xValues.Length - 1
            If Not IsFinite(xValues(i)) OrElse Not IsFinite(yValues(i)) Then
                Throw New ArgumentException("The " & label & " coordinates must be finite.")
            End If
        Next
    End Sub

    Private Shared Sub ValidateAppearance(appearance As CumulativeDistributionPlotAppearance)
        If appearance Is Nothing Then Throw New ArgumentNullException(NameOf(appearance))

        If appearance.PaletteColors IsNot Nothing AndAlso appearance.PaletteColors.Length = 0 Then
            Throw New ArgumentException("PaletteColors must contain at least one colour when supplied.", NameOf(appearance))
        End If

        ValidatePositive(appearance.LegendFontSize, NameOf(appearance.LegendFontSize))
        ValidatePositive(appearance.EmpiricalLineWeight, NameOf(appearance.EmpiricalLineWeight))
        ValidatePositive(appearance.FittedLineWeight, NameOf(appearance.FittedLineWeight))
        ValidatePositive(appearance.PercentileLineWeight, NameOf(appearance.PercentileLineWeight))
        ValidatePositive(appearance.PercentileLabelFontSize, NameOf(appearance.PercentileLabelFontSize))
        ValidatePositive(appearance.GridlineWeight, NameOf(appearance.GridlineWeight))
        ValidatePositive(appearance.AxisLineWeight, NameOf(appearance.AxisLineWeight))
        ValidatePositive(appearance.AxisLabelFontSize, NameOf(appearance.AxisLabelFontSize))
        ValidatePositive(appearance.AxisTitleFontSize, NameOf(appearance.AxisTitleFontSize))
        ValidatePositive(appearance.ChartTitleFontSize, NameOf(appearance.ChartTitleFontSize))

        ValidateTransparency(appearance.EmpiricalLineTransparency, NameOf(appearance.EmpiricalLineTransparency))
        ValidateTransparency(appearance.FittedLineTransparency, NameOf(appearance.FittedLineTransparency))
        ValidateTransparency(appearance.PercentileLineTransparency, NameOf(appearance.PercentileLineTransparency))

        If appearance.XAxisMajorUnit.HasValue AndAlso Not IsFinitePositive(appearance.XAxisMajorUnit.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisMajorUnit), "XAxisMajorUnit must be finite and positive when supplied.")
        End If

        If String.IsNullOrWhiteSpace(appearance.FontName) Then
            Throw New ArgumentException("FontName cannot be empty.", NameOf(appearance.FontName))
        End If
    End Sub

    Private Shared Sub ValidatePositive(value As Double, parameterName As String)
        If Not IsFinitePositive(value) Then
            Throw New ArgumentOutOfRangeException(parameterName, "The value must be finite and positive.")
        End If
    End Sub

    Private Shared Sub ValidateTransparency(value As Single, parameterName As String)
        If Single.IsNaN(value) OrElse Single.IsInfinity(value) OrElse value < 0.0F OrElse value > 1.0F Then
            Throw New ArgumentOutOfRangeException(parameterName, "Transparency must be between 0 and 1.")
        End If
    End Sub

    Private Shared Function ToMsoDashStyle(style As CumulativeDistributionLineStyle) As Integer
        Select Case style
            Case CumulativeDistributionLineStyle.Dash
                Return MsoLineDash
            Case CumulativeDistributionLineStyle.Dot
                Return MsoLineSquareDot
            Case CumulativeDistributionLineStyle.DashDot
                Return MsoLineDashDot
            Case CumulativeDistributionLineStyle.DashDotDot
                Return MsoLineDashDotDot
            Case Else
                Return MsoLineSolid
        End Select
    End Function

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            seriesCollection.Item(1).Delete()
        Loop
    End Sub

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Class
