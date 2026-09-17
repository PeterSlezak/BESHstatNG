Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Line patterns available for the dumbbell connector.
''' </summary>
''' <remarks>
''' A small BESHStatNG-specific enum is used so this renderer does not require a
''' compile-time Microsoft.Office.Core reference merely to expose MsoLineDashStyle.
''' </remarks>
Public Enum DumbbellConnectorLineStyle
    Solid
    Dash
    Dot
    DashDot
    DashDotDot
End Enum

''' <summary>
''' Excel appearance settings for <see cref="DumbbellPlotExcel"/>.
''' </summary>
''' <remarks>
''' Colors are OLE RGB integers used by the Excel object model. The defaults are
''' intentionally restrained and can be replaced by the future Dumbbell Plot UI.
''' </remarks>
Public Class DumbbellPlotAppearance
    Public Property ChartTitle As String = "Dumbbell plot"
    Public Property XAxisTitle As String = String.Empty

    Public Property FirstSeriesName As String = "First"
    Public Property SecondSeriesName As String = "Second"
    Public Property AuxiliarySeriesName As String = "Observations"

    Public Property ShowLegend As Boolean = True
    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionBottom
    Public Property LegendFontSize As Double = 9.0R

    Public Property ShowConnectors As Boolean = True
    Public Property ShowAuxiliaryObservations As Boolean = True
    Public Property ShowCategoryLabels As Boolean = True
    Public Property ShowEndpointValueLabels As Boolean = False

    Public Property FirstMarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
    Public Property SecondMarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
    Public Property AuxiliaryMarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle

    Public Property FirstMarkerSize As Integer = 8
    Public Property SecondMarkerSize As Integer = 8
    Public Property AuxiliaryMarkerSize As Integer = 5

    'OLE RGB/BGR integers: orange, blue-violet, and coral/red.
    Public Property FirstMarkerColor As Integer = &H328BED
    Public Property SecondMarkerColor As Integer = &HC9525B
    Public Property AuxiliaryMarkerColor As Integer = &H6269DF

    ''' <summary>
    ''' Transparency of optional auxiliary observations, from 0 (opaque) to 1
    ''' (fully transparent). Excel support for marker transparency varies slightly
    ''' between versions, so this setting is applied on a best-effort basis.
    ''' </summary>
    Public Property AuxiliaryMarkerTransparency As Single = 0.45F

    Public Property ConnectorColor As Integer = &HA8A8A8
    Public Property ConnectorWeight As Single = 1.5F
    Public Property ConnectorTransparency As Single = 0.15F
    Public Property ConnectorLineStyle As DumbbellConnectorLineStyle = DumbbellConnectorLineStyle.Solid

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property TextColor As Integer = &H404040
    Public Property GridlineColor As Integer = &HE6E6E6
    Public Property GridlineWeight As Single = 0.75F

    ''' <summary>Vertical gridlines originate from major ticks on the numeric X axis.</summary>
    Public Property ShowVerticalGridlines As Boolean = True

    ''' <summary>Horizontal gridlines are aligned with the artificial category Y positions.</summary>
    Public Property ShowHorizontalGridlines As Boolean = True

    Public Property CategoryLabelFontSize As Double = 9.0R

    ''' <summary>
    ''' Extra leftward offset, in chart points, applied to category data labels after
    ''' Excel has completed chart layout. This keeps the labels clear of the first
    ''' vertical gridline/plot boundary.
    ''' </summary>
    Public Property CategoryLabelOffsetPoints As Double = 14.0R

    Public Property XAxisLabelFontSize As Double = 9.0R
    Public Property ChartTitleFontSize As Double = 12.0R

    ''' <summary>
    ''' Optional manual X-axis minimum. Date axes use Excel/OA date serial values.
    ''' </summary>
    Public Property XAxisMinimum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Optional manual X-axis maximum. Date axes use Excel/OA date serial values.
    ''' </summary>
    Public Property XAxisMaximum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Optional major tick interval. On a date axis the unit is days because Excel
    ''' stores dates as serial day values.
    ''' </summary>
    Public Property XAxisMajorUnit As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Optional Excel number format for X-axis tick labels. When blank, the renderer
    ''' uses "dd-mmm" for a detected date axis and "0.###" for a numeric axis.
    ''' </summary>
    Public Property XAxisNumberFormat As String = String.Empty

    ''' <summary>
    ''' Fraction of the observed X span added at automatic axis ends. Default: 4%.
    ''' Manual axis bounds are not padded.
    ''' </summary>
    Public Property XAxisPaddingFraction As Double = 0.04R

    ''' <summary>
    ''' Optional .NET numeric/date display format used only when endpoint value labels
    ''' are enabled. Leave blank for the renderer's compact default.
    ''' </summary>
    Public Property EndpointValueDisplayFormat As String = String.Empty

    Friend Function Copy() As DumbbellPlotAppearance
        Return New DumbbellPlotAppearance With {
            .ChartTitle = ChartTitle,
            .XAxisTitle = XAxisTitle,
            .FirstSeriesName = FirstSeriesName,
            .SecondSeriesName = SecondSeriesName,
            .AuxiliarySeriesName = AuxiliarySeriesName,
            .ShowLegend = ShowLegend,
            .LegendPosition = LegendPosition,
            .LegendFontSize = LegendFontSize,
            .ShowConnectors = ShowConnectors,
            .ShowAuxiliaryObservations = ShowAuxiliaryObservations,
            .ShowCategoryLabels = ShowCategoryLabels,
            .ShowEndpointValueLabels = ShowEndpointValueLabels,
            .FirstMarkerStyle = FirstMarkerStyle,
            .SecondMarkerStyle = SecondMarkerStyle,
            .AuxiliaryMarkerStyle = AuxiliaryMarkerStyle,
            .FirstMarkerSize = FirstMarkerSize,
            .SecondMarkerSize = SecondMarkerSize,
            .AuxiliaryMarkerSize = AuxiliaryMarkerSize,
            .FirstMarkerColor = FirstMarkerColor,
            .SecondMarkerColor = SecondMarkerColor,
            .AuxiliaryMarkerColor = AuxiliaryMarkerColor,
            .AuxiliaryMarkerTransparency = AuxiliaryMarkerTransparency,
            .ConnectorColor = ConnectorColor,
            .ConnectorWeight = ConnectorWeight,
            .ConnectorTransparency = ConnectorTransparency,
            .ConnectorLineStyle = ConnectorLineStyle,
            .BackgroundColor = BackgroundColor,
            .TextColor = TextColor,
            .GridlineColor = GridlineColor,
            .GridlineWeight = GridlineWeight,
            .ShowVerticalGridlines = ShowVerticalGridlines,
            .ShowHorizontalGridlines = ShowHorizontalGridlines,
            .CategoryLabelFontSize = CategoryLabelFontSize,
            .CategoryLabelOffsetPoints = CategoryLabelOffsetPoints,
            .XAxisLabelFontSize = XAxisLabelFontSize,
            .ChartTitleFontSize = ChartTitleFontSize,
            .XAxisMinimum = XAxisMinimum,
            .XAxisMaximum = XAxisMaximum,
            .XAxisMajorUnit = XAxisMajorUnit,
            .XAxisNumberFormat = XAxisNumberFormat,
            .XAxisPaddingFraction = XAxisPaddingFraction,
            .EndpointValueDisplayFormat = EndpointValueDisplayFormat
        }
    End Function
End Class

''' <summary>
''' Renders an Excel-independent <see cref="DumbbellPlotResult"/> as an embedded
''' Excel XY-scatter chart.
''' </summary>
''' <remarks>
''' The renderer deliberately uses only a small number of Excel series:
''' one hidden-marker series with custom positive X error bars for all connectors,
''' one series for each endpoint, one optional series for all auxiliary observations,
''' and one invisible helper series carrying category data labels.
'''
''' Series returned directly by SeriesCollection.NewSeries are used throughout; the
''' implementation does not depend on SeriesCollection(SeriesCollection.Count)
''' indexing, which is unreliable in some Excel/Interop combinations.
''' </remarks>
Public NotInheritable Class DumbbellPlotExcel
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
        Friend YMinimum As Double
        Friend YMaximum As Double
        Friend XNumberFormat As String
    End Class

    ''' <summary>
    ''' Adds a dumbbell chart to an Excel worksheet.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As DumbbellPlotResult,
                                    Optional appearance As DumbbellPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 700.0R,
                                    Optional height As Double = 460.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.CategoryCount < 1 Then
            Throw New ArgumentException("The dumbbell result contains no categories.", NameOf(result))
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

        Dim resolvedAppearance As DumbbellPlotAppearance = If(appearance, New DumbbellPlotAppearance()).Copy()
        ValidateAppearance(resolvedAppearance)

        Dim categories As DumbbellPlotCategory() = result.Categories
        Dim axisContext As AxisContext = ResolveAxisContext(result, resolvedAppearance)
        Dim chartShape As Shape = Nothing

        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance)

            'Keep only the semantic data series in the legend. The connector and
            'category-label helpers are intentionally removed later.
            Dim legendSeriesIndices As New List(Of Integer)()

            If resolvedAppearance.ShowConnectors Then
                AddConnectorSeries(seriesCollection, categories, resolvedAppearance)
            End If

            Dim firstSeries As Series = AddEndpointSeries(seriesCollection,
                                                          categories,
                                                          True,
                                                          ResolveSeriesName(resolvedAppearance.FirstSeriesName, "First"),
                                                          resolvedAppearance.FirstMarkerStyle,
                                                          resolvedAppearance.FirstMarkerSize,
                                                          resolvedAppearance.FirstMarkerColor)
            legendSeriesIndices.Add(seriesCollection.Count)

            Dim secondSeries As Series = AddEndpointSeries(seriesCollection,
                                                           categories,
                                                           False,
                                                           ResolveSeriesName(resolvedAppearance.SecondSeriesName, "Second"),
                                                           resolvedAppearance.SecondMarkerStyle,
                                                           resolvedAppearance.SecondMarkerSize,
                                                           resolvedAppearance.SecondMarkerColor)
            legendSeriesIndices.Add(seriesCollection.Count)

            Dim auxiliarySeries As Series = Nothing
            If resolvedAppearance.ShowAuxiliaryObservations AndAlso result.HasAuxiliaryObservations Then
                auxiliarySeries = AddAuxiliarySeries(seriesCollection,
                                                     result.AuxiliaryObservations,
                                                     ResolveSeriesName(resolvedAppearance.AuxiliarySeriesName, "Observations"),
                                                     resolvedAppearance)
                legendSeriesIndices.Add(seriesCollection.Count)
            End If

            If resolvedAppearance.ShowEndpointValueLabels Then
                AddEndpointValueLabels(firstSeries, categories, True, result.AxisValueKind, resolvedAppearance)
                AddEndpointValueLabels(secondSeries, categories, False, result.AxisValueKind, resolvedAppearance)
            End If

            Dim categoryLabelSeries As Series = Nothing
            If resolvedAppearance.ShowCategoryLabels Then
                categoryLabelSeries = AddCategoryLabelSeries(seriesCollection, categories, axisContext, resolvedAppearance)
            End If

            ConfigureAxes(chart, result.CategoryCount, axisContext, resolvedAppearance)
            chart.Refresh()

            'Excel can reapply chart-style defaults during the first refresh. Reassert
            'the axis settings and the auxiliary marker formatting once the chart is
            'fully materialized, then prune helper entries from the legend exactly once.
            ConfigureAxes(chart, result.CategoryCount, axisContext, resolvedAppearance)
            If auxiliarySeries IsNot Nothing Then
                ApplyAuxiliaryMarkerAppearance(auxiliarySeries, resolvedAppearance)
            End If
            ConfigureLegend(chart, legendSeriesIndices, resolvedAppearance)
            If categoryLabelSeries IsNot Nothing Then
                ApplyCategoryLabelOffset(categoryLabelSeries, resolvedAppearance.CategoryLabelOffsetPoints)
            End If

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
    ''' Suggests a chart height that keeps category rows readable. The caller may
    ''' override this and pass an explicit height to <see cref="AddChart"/>.
    ''' </summary>
    Public Shared Function SuggestedHeight(result As DumbbellPlotResult) As Double
        If result Is Nothing OrElse result.CategoryCount < 1 Then Return 360.0R
        Return Math.Max(340.0R, Math.Min(1400.0R, 145.0R + 22.0R * result.CategoryCount))
    End Function

    Private Shared Sub AddConnectorSeries(seriesCollection As SeriesCollection,
                                          categories As DumbbellPlotCategory(),
                                          appearance As DumbbellPlotAppearance)
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

        'If every pair is identical there is no connector geometry to draw.
        If Not hasVisibleConnector Then Return

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = "Dumbbell connectors"
            .ChartType = XlChartType.xlXYScatter
            .XValues = xStarts
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
            .ErrorBar(Direction:=XlErrorBarDirection.xlX,
                      Include:=Constants.xlPlusValues,
                      Type:=XlErrorBarType.xlErrorBarTypeCustom,
                      Amount:=connectorLengths)
        End With

        Try
            With series.ErrorBars
                .EndStyle = XlEndStyleCap.xlNoCap
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = appearance.ConnectorColor
                .Format.Line.Weight = appearance.ConnectorWeight
                .Format.Line.Transparency = appearance.ConnectorTransparency
            End With

            Dim errorBarsObject As Object = series.ErrorBars
            errorBarsObject.Format.Line.DashStyle = ToMsoDashStyle(appearance.ConnectorLineStyle)
        Catch
            'Formatting is cosmetic. The custom X error bars themselves are the
            'important connector geometry and remain usable on older Excel builds.
        End Try
    End Sub

    Private Shared Function AddEndpointSeries(seriesCollection As SeriesCollection,
                                              categories As DumbbellPlotCategory(),
                                              useFirstEndpoint As Boolean,
                                              seriesName As String,
                                              markerStyle As XlMarkerStyle,
                                              markerSize As Integer,
                                              markerColor As Integer) As Series
        Dim n As Integer = categories.Length
        Dim xValues(n - 1) As Double
        Dim yValues(n - 1) As Double

        For i As Integer = 0 To n - 1
            xValues(i) = If(useFirstEndpoint, categories(i).FirstValue, categories(i).SecondValue)
            yValues(i) = categories(i).YPosition
        Next

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = markerStyle
            .MarkerSize = markerSize
            .MarkerForegroundColor = markerColor
            .MarkerBackgroundColor = markerColor
            .Format.Line.Visible = False
        End With
        Return series
    End Function

    Private Shared Function AddAuxiliarySeries(seriesCollection As SeriesCollection,
                                               observations As DumbbellAuxiliaryObservation(),
                                               seriesName As String,
                                               appearance As DumbbellPlotAppearance) As Series
        If observations Is Nothing OrElse observations.Length = 0 Then Return Nothing

        Dim xValues(observations.Length - 1) As Double
        Dim yValues(observations.Length - 1) As Double
        For i As Integer = 0 To observations.Length - 1
            xValues(i) = observations(i).Value
            yValues(i) = observations(i).YPosition
        Next

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .Format.Line.Visible = False
        End With

        ApplyAuxiliaryMarkerAppearance(series, appearance)
        Return series
    End Function

    Private Shared Function AddCategoryLabelSeries(seriesCollection As SeriesCollection,
                                                   categories As DumbbellPlotCategory(),
                                                   axisContext As AxisContext,
                                                   appearance As DumbbellPlotAppearance) As Series
        Dim n As Integer = categories.Length
        Dim xValues(n - 1) As Double
        Dim yValues(n - 1) As Double

        For i As Integer = 0 To n - 1
            xValues(i) = axisContext.XMinimum
            yValues(i) = categories(i).YPosition
        Next

        Dim series As Object = seriesCollection.NewSeries()
        With series
            .Name = "Category labels"
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues

            'A tiny background-coloured marker gives Excel a concrete anchor for
            'xlLabelPositionLeft. With MarkerStyle=None, some Excel builds centre
            'data labels over the helper point and the text can straddle the first
            'vertical gridline.
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
            .MarkerSize = 2
            .MarkerForegroundColor = appearance.BackgroundColor
            .MarkerBackgroundColor = appearance.BackgroundColor
            .Format.Line.Visible = False
        End With

        Try
            For i As Integer = 0 To n - 1
                Dim point As Point = DirectCast(series.Points(i + 1), Point)
                point.HasDataLabel = True
                With point.DataLabel
                    .Text = categories(i).CategoryLabel
                    .Position = XlDataLabelPosition.xlLabelPositionLeft
                    .Font.Size = appearance.CategoryLabelFontSize
                    .Font.Color = appearance.TextColor
                End With
            Next
        Catch
            'The data remain correctly plotted even if an older Excel build rejects
            'individual helper-label formatting.
        End Try

        Return DirectCast(series, Series)
    End Function

    ''' <summary>
    ''' Moves category labels a few chart points farther left after Excel has laid out
    ''' the plot. DataLabel.Position=Left alone places the label edge directly against
    ''' the helper point at XMinimum, which can visually collide with the first vertical
    ''' gridline.
    ''' </summary>
    Private Shared Sub ApplyCategoryLabelOffset(series As Series, offsetPoints As Double)
        If series Is Nothing OrElse Not IsFinite(offsetPoints) OrElse offsetPoints <= 0.0R Then Return

        Try
            Dim points As Object = series.Points()
            For i As Integer = 1 To CInt(points.Count)
                Dim point As Object = points.Item(i)
                If CBool(point.HasDataLabel) Then
                    Dim label As Object = point.DataLabel

                    'First let Excel calculate a valid left-of-point position, then
                    'switch the label to custom positioning before applying the extra
                    'offset. Leaving Position=Left can cause Excel to snap the label
                    'back against/over the helper point during chart layout.
                    label.Position = XlDataLabelPosition.xlLabelPositionLeft
                    Dim resolvedLeft As Double = CDbl(label.Left)
                    Dim resolvedTop As Double = CDbl(label.Top)
                    Try
                        label.Position = XlDataLabelPosition.xlLabelPositionCustom
                    Catch
                        'Older Excel versions may not accept the Custom enum here;
                        'Left/Top assignment below still works on those versions.
                    End Try
                    label.Left = resolvedLeft - offsetPoints
                    label.Top = resolvedTop
                End If
            Next
        Catch
            'Label spacing is cosmetic; retain a valid chart if a particular Excel
            'version does not allow explicit DataLabel.Left positioning.
        End Try
    End Sub

    Private Shared Sub AddEndpointValueLabels(series As Series,
                                              categories As DumbbellPlotCategory(),
                                              useFirstEndpoint As Boolean,
                                              axisValueKind As DumbbellAxisValueKind,
                                              appearance As DumbbellPlotAppearance)
        Try
            For i As Integer = 0 To categories.Length - 1
                Dim point As Point = DirectCast(series.Points(i + 1), Point)
                Dim value As Double = If(useFirstEndpoint, categories(i).FirstValue, categories(i).SecondValue)
                point.HasDataLabel = True
                With point.DataLabel
                    .Text = FormatEndpointValue(value, axisValueKind, appearance.EndpointValueDisplayFormat)
                    .Position = If(useFirstEndpoint,
                                   XlDataLabelPosition.xlLabelPositionAbove,
                                   XlDataLabelPosition.xlLabelPositionBelow)
                    .Font.Size = Math.Max(7.0R, appearance.CategoryLabelFontSize - 1.0R)
                    .Font.Color = appearance.TextColor
                End With
            Next
        Catch
            'Value labels are optional; a COM formatting limitation should not prevent
            'the principal dumbbell chart from being returned.
        End Try
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     categoryCount As Integer,
                                     axisContext As AxisContext,
                                     appearance As DumbbellPlotAppearance)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
        With xAxis
            .MinimumScale = axisContext.XMinimum
            .MaximumScale = axisContext.XMaximum
            .MajorUnit = axisContext.XMajorUnit
            .HasTitle = Not String.IsNullOrWhiteSpace(appearance.XAxisTitle)
            If .HasTitle Then
                .AxisTitle.Text = appearance.XAxisTitle.Trim()
                .AxisTitle.Font.Color = appearance.TextColor
            End If
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionLow
            .TickLabels.Font.Size = appearance.XAxisLabelFontSize
            .TickLabels.Font.Color = appearance.TextColor
            .TickLabels.NumberFormat = axisContext.XNumberFormat
            .HasMajorGridlines = appearance.ShowVerticalGridlines
            .HasMinorGridlines = False

            'Crosses is defined on the axis being crossed. For the horizontal X
            'axis this controls where the vertical Y axis is drawn, so keep that
            'crossing at the left edge of the plot.
            .Crosses = XlAxisCrosses.xlAxisCrossesMinimum
        End With
        FormatGridlines(xAxis, appearance.ShowVerticalGridlines, appearance)

        Dim yAxis As Object = DirectCast(chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)
        With yAxis
            .MinimumScale = axisContext.YMinimum
            .MaximumScale = axisContext.YMaximum
            .MajorUnit = 1.0R
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
            .HasTitle = False
            .HasMajorGridlines = appearance.ShowHorizontalGridlines
            .HasMinorGridlines = False

            'The horizontal X axis crosses the vertical value axis. Setting the
            'vertical axis crossing to its minimum places the X axis at the bottom
            'of the plot. The previous CrossesAt=XMinimum mixed X and Y coordinate
            'systems and could put the X axis through the middle of the dumbbells.
            .Crosses = XlAxisCrosses.xlAxisCrossesMinimum
        End With
        FormatGridlines(yAxis, appearance.ShowHorizontalGridlines, appearance)

        'The numerical Y axis is only a coordinate scaffold for the categories.
        'Hide its line while retaining its major gridlines if requested.
        Try
            yAxis.Format.Line.Visible = False
        Catch
        End Try
    End Sub

    Private Shared Sub FormatGridlines(axis As Object,
                                       showGridlines As Boolean,
                                       appearance As DumbbellPlotAppearance)
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

    Private Shared Sub ConfigureTitle(chart As Chart, appearance As DumbbellPlotAppearance)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) Then
            chart.HasTitle = False
            Return
        End If

        chart.HasTitle = True
        chart.ChartTitle.Text = appearance.ChartTitle.Trim()
        chart.ChartTitle.Font.Size = appearance.ChartTitleFontSize
        chart.ChartTitle.Font.Color = appearance.TextColor
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As DumbbellPlotAppearance)
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
                                       appearance As DumbbellPlotAppearance)
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
            'If legend-entry deletion is not available, prefer returning the chart
            'rather than failing a successful render.
        End Try
    End Sub

    Private Shared Function ResolveAxisContext(result As DumbbellPlotResult,
                                               appearance As DumbbellPlotAppearance) As AxisContext
        Dim dataMinimum As Double = result.MinimumValue
        Dim dataMaximum As Double = result.MaximumValue
        If Not IsFinite(dataMinimum) OrElse Not IsFinite(dataMaximum) Then
            Throw New ArgumentException("The dumbbell result does not contain a finite X range.", NameOf(result))
        End If

        Dim span As Double = dataMaximum - dataMinimum
        Dim fallbackSpan As Double
        If span > 0.0R Then
            fallbackSpan = span
        ElseIf result.AxisValueKind = DumbbellAxisValueKind.DateValue Then
            fallbackSpan = 2.0R
        Else
            fallbackSpan = Math.Max(1.0R, Math.Abs(dataMinimum) * 0.1R)
        End If

        Dim padding As Double = fallbackSpan * appearance.XAxisPaddingFraction
        If result.AxisValueKind = DumbbellAxisValueKind.DateValue Then
            padding = Math.Max(padding, 0.5R)
        End If

        Dim paddedMinimum As Double = dataMinimum - padding
        Dim paddedMaximum As Double = dataMaximum + padding
        If paddedMinimum >= paddedMaximum Then
            paddedMinimum = dataMinimum - 0.5R * fallbackSpan
            paddedMaximum = dataMaximum + 0.5R * fallbackSpan
        End If

        'Use a 1-2-5 "nice" step, mirroring the histogram bin-step logic, so the
        'automatic gridlines land on human-friendly values instead of starting at
        'an arbitrary padded minimum such as 13.012. Aim for about five intervals.
        Dim majorUnit As Double
        If appearance.XAxisMajorUnit.HasValue Then
            majorUnit = appearance.XAxisMajorUnit.Value
        Else
            majorUnit = NiceStep125((paddedMaximum - paddedMinimum) / 5.0R)
            If result.AxisValueKind = DumbbellAxisValueKind.DateValue Then
                'The default date tick format displays days, so sub-day automatic
                'ticks would only create repeated date labels.
                majorUnit = Math.Max(1.0R, majorUnit)
            End If
        End If

        If Not IsFinitePositive(majorUnit) Then majorUnit = 1.0R

        Dim xMinimum As Double
        If appearance.XAxisMinimum.HasValue Then
            xMinimum = appearance.XAxisMinimum.Value
        Else
            xMinimum = FloorToStep(paddedMinimum, majorUnit)
        End If

        Dim xMaximum As Double
        If appearance.XAxisMaximum.HasValue Then
            xMaximum = appearance.XAxisMaximum.Value
        Else
            xMaximum = CeilingToStep(paddedMaximum, majorUnit)
        End If

        If xMinimum >= xMaximum Then
            If Not appearance.XAxisMinimum.HasValue AndAlso Not appearance.XAxisMaximum.HasValue Then
                xMinimum = FloorToStep(dataMinimum - 0.5R * fallbackSpan, majorUnit)
                xMaximum = CeilingToStep(dataMaximum + 0.5R * fallbackSpan, majorUnit)
                If xMinimum >= xMaximum Then xMaximum = xMinimum + majorUnit
            Else
                Throw New ArgumentException("The resolved X-axis minimum must be smaller than the X-axis maximum.", NameOf(appearance))
            End If
        End If

        Return New AxisContext With {
            .XMinimum = xMinimum,
            .XMaximum = xMaximum,
            .XMajorUnit = majorUnit,
            .YMinimum = 0.0R,
            .YMaximum = result.CategoryCount + 1.0R,
            .XNumberFormat = ResolveXAxisNumberFormat(result.AxisValueKind, appearance.XAxisNumberFormat)
        }
    End Function

    ''' <summary>
    ''' Returns a human-friendly 1-2-5 step greater than or equal to the requested
    ''' raw interval. This mirrors the rule already used by the histogram bin logic.
    ''' </summary>
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

    Private Shared Function ResolveXAxisNumberFormat(axisValueKind As DumbbellAxisValueKind,
                                                     requestedFormat As String) As String
        If Not String.IsNullOrWhiteSpace(requestedFormat) Then Return requestedFormat.Trim()

        Select Case axisValueKind
            Case DumbbellAxisValueKind.DateValue
                Return "dd-mmm"
            Case DumbbellAxisValueKind.Numeric
                Return "0.###"
            Case Else
                Return "General"
        End Select
    End Function

    Private Shared Function FormatEndpointValue(value As Double,
                                                axisValueKind As DumbbellAxisValueKind,
                                                requestedFormat As String) As String
        If axisValueKind = DumbbellAxisValueKind.DateValue Then
            Try
                Dim dt As DateTime = DateTime.FromOADate(value)
                Dim fmt As String = If(String.IsNullOrWhiteSpace(requestedFormat), "dd-MMM-yyyy", requestedFormat.Trim())
                Return dt.ToString(fmt, CultureInfo.CurrentCulture)
            Catch
                Return value.ToString("G", CultureInfo.CurrentCulture)
            End Try
        End If

        If Not String.IsNullOrWhiteSpace(requestedFormat) Then
            Try
                Return value.ToString(requestedFormat.Trim(), CultureInfo.CurrentCulture)
            Catch
            End Try
        End If
        Return value.ToString("0.###", CultureInfo.CurrentCulture)
    End Function

    ''' <summary>
    ''' Applies auxiliary marker style, size, colour and visual transparency while
    ''' explicitly keeping the auxiliary series marker-only (no connecting line).
    ''' </summary>
    Private Shared Sub ApplyAuxiliaryMarkerAppearance(series As Series,
                                                       appearance As DumbbellPlotAppearance)
        If series Is Nothing Then Return

        'Excel's scatter-marker transparency support varies considerably between
        'versions. Applying Point.Format.Line/Fill can also turn the point series
        'into visible connecting segments. To keep the observations strictly as
        'unconnected markers, approximate the requested transparency by blending
        'the selected marker colour with the chart background, then use the normal
        'MarkerForeground/MarkerBackground properties only.
        Dim displayColor As Integer = BlendOleColor(appearance.AuxiliaryMarkerColor,
                                                    appearance.BackgroundColor,
                                                    appearance.AuxiliaryMarkerTransparency)

        Dim seriesObject As Object = series
        With series
            .ChartType = XlChartType.xlXYScatter
            .MarkerStyle = appearance.AuxiliaryMarkerStyle
            .MarkerSize = appearance.AuxiliaryMarkerSize
            .MarkerForegroundColor = displayColor
            .MarkerBackgroundColor = displayColor
        End With

        'Disable the series line through both the modern Format API and the legacy
        'Border API. The latter is important on Excel builds that may re-enable a
        'scatter-series line after point-level formatting or chart-style refresh.
        Try
            seriesObject.Format.Line.Visible = False
        Catch
        End Try
        Try
            seriesObject.Border.LineStyle = Constants.xlNone
        Catch
        End Try

        Try
            Dim points As Object = series.Points()
            For i As Integer = 1 To CInt(points.Count)
                Dim point As Object = points.Item(i)
                Try
                    point.MarkerStyle = appearance.AuxiliaryMarkerStyle
                    point.MarkerSize = appearance.AuxiliaryMarkerSize
                    point.MarkerForegroundColor = displayColor
                    point.MarkerBackgroundColor = displayColor
                Catch
                End Try

                'Do not set Point.Format.Line.Visible=True here: on scatter series
                'Excel can interpret that as the segment leading to the point rather
                'than merely the marker outline, which joins successive observations.
                Try
                    point.Format.Line.Visible = False
                Catch
                End Try
            Next
        Catch
            'Series-level marker formatting above is sufficient if Points is unavailable.
        End Try

        'Reassert marker-only chart semantics after point formatting.
        Try
            series.ChartType = XlChartType.xlXYScatter
            seriesObject.Format.Line.Visible = False
            seriesObject.Border.LineStyle = Constants.xlNone
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Blends an Excel/OLE RGB colour toward the background to emulate marker
    ''' transparency without invoking Excel's inconsistent marker Fill API.
    ''' transparency=0 returns foreground; transparency=1 returns background.
    ''' </summary>
    Private Shared Function BlendOleColor(foreground As Integer,
                                          background As Integer,
                                          transparency As Single) As Integer
        Dim t As Double = Math.Max(0.0R, Math.Min(1.0R, CDbl(transparency)))
        Dim keep As Double = 1.0R - t

        Dim fr As Integer = foreground And &HFF
        Dim fg As Integer = (foreground >> 8) And &HFF
        Dim fb As Integer = (foreground >> 16) And &HFF

        Dim br As Integer = background And &HFF
        Dim bg As Integer = (background >> 8) And &HFF
        Dim bb As Integer = (background >> 16) And &HFF

        Dim r As Integer = CInt(Math.Round(fr * keep + br * t))
        Dim g As Integer = CInt(Math.Round(fg * keep + bg * t))
        Dim b As Integer = CInt(Math.Round(fb * keep + bb * t))

        Return (b << 16) Or (g << 8) Or r
    End Function

    Private Shared Function ToMsoDashStyle(style As DumbbellConnectorLineStyle) As Integer
        Select Case style
            Case DumbbellConnectorLineStyle.Solid
                Return MsoLineSolid
            Case DumbbellConnectorLineStyle.Dash
                Return MsoLineDash
            Case DumbbellConnectorLineStyle.Dot
                Return MsoLineSquareDot
            Case DumbbellConnectorLineStyle.DashDot
                Return MsoLineDashDot
            Case DumbbellConnectorLineStyle.DashDotDot
                Return MsoLineDashDotDot
            Case Else
                Return MsoLineSolid
        End Select
    End Function

    Private Shared Function ResolveSeriesName(value As String, fallback As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return fallback
        Return value.Trim()
    End Function

    Private Shared Sub ValidateAppearance(appearance As DumbbellPlotAppearance)
        ValidateMarker(appearance.FirstMarkerStyle, appearance.FirstMarkerSize, NameOf(appearance.FirstMarkerStyle))
        ValidateMarker(appearance.SecondMarkerStyle, appearance.SecondMarkerSize, NameOf(appearance.SecondMarkerStyle))
        ValidateMarker(appearance.AuxiliaryMarkerStyle, appearance.AuxiliaryMarkerSize, NameOf(appearance.AuxiliaryMarkerStyle))

        If Not IsFinitePositive(appearance.ConnectorWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ConnectorWeight), "Connector weight must be finite and positive.")
        End If
        If Not IsFiniteUnitInterval(appearance.ConnectorTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ConnectorTransparency), "Connector transparency must be in the interval [0, 1].")
        End If
        If Not IsFiniteUnitInterval(appearance.AuxiliaryMarkerTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.AuxiliaryMarkerTransparency), "Auxiliary marker transparency must be in the interval [0, 1].")
        End If
        If Not [Enum].IsDefined(GetType(DumbbellConnectorLineStyle), appearance.ConnectorLineStyle) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ConnectorLineStyle), "The connector line style is not defined.")
        End If

        If Not IsFinitePositive(appearance.CategoryLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CategoryLabelFontSize), "Category label font size must be finite and positive.")
        End If
        If Not IsFinite(appearance.CategoryLabelOffsetPoints) OrElse appearance.CategoryLabelOffsetPoints < 0.0R OrElse appearance.CategoryLabelOffsetPoints > 100.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CategoryLabelOffsetPoints), "Category label offset must be finite and between 0 and 100 points.")
        End If
        If Not IsFinitePositive(appearance.XAxisLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisLabelFontSize), "X-axis label font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.ChartTitleFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ChartTitleFontSize), "Chart title font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.LegendFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LegendFontSize), "Legend font size must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.GridlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GridlineWeight), "Gridline weight must be finite and positive.")
        End If

        If Double.IsNaN(appearance.XAxisPaddingFraction) OrElse Double.IsInfinity(appearance.XAxisPaddingFraction) OrElse
           appearance.XAxisPaddingFraction < 0.0R OrElse appearance.XAxisPaddingFraction > 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisPaddingFraction),
                                                  "XAxisPaddingFraction must be finite and in the interval [0, 0.5].")
        End If

        If appearance.XAxisMinimum.HasValue AndAlso Not IsFinite(appearance.XAxisMinimum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisMinimum), "X-axis minimum must be finite.")
        End If
        If appearance.XAxisMaximum.HasValue AndAlso Not IsFinite(appearance.XAxisMaximum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisMaximum), "X-axis maximum must be finite.")
        End If
        If appearance.XAxisMinimum.HasValue AndAlso appearance.XAxisMaximum.HasValue AndAlso
           appearance.XAxisMinimum.Value >= appearance.XAxisMaximum.Value Then
            Throw New ArgumentException("XAxisMinimum must be smaller than XAxisMaximum.", NameOf(appearance))
        End If
        If appearance.XAxisMajorUnit.HasValue AndAlso Not IsFinitePositive(appearance.XAxisMajorUnit.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.XAxisMajorUnit), "X-axis major unit must be finite and positive.")
        End If
    End Sub

    Private Shared Sub ValidateMarker(style As XlMarkerStyle, size As Integer, parameterName As String)
        If style = XlMarkerStyle.xlMarkerStyleNone Then
            Throw New ArgumentException("Dumbbell marker styles must be visible.", parameterName)
        End If
        If size < 2 OrElse size > 72 Then
            Throw New ArgumentOutOfRangeException(parameterName, "Marker size must be between 2 and 72 points.")
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
