Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Determines which node supplies a Sankey ribbon's fill color.
''' </summary>
Public Enum SankeyRibbonColorMode
    SourceNode
    TargetNode
    Neutral
End Enum

''' <summary>
''' Text shown beside each Sankey node.
''' </summary>
Public Enum SankeyNodeLabelMode
    NameOnly
    NameAndValue
    NameAndPercentage
    NamePercentageAndValue
End Enum

''' <summary>
''' Denominator used when Sankey node labels include percentages.
''' </summary>
Public Enum SankeyPercentageBasis
    ''' <summary>
    ''' Uses <see cref="SankeyPlotResult.ReferenceTotal"/>, i.e. the largest stage total.
    ''' This is useful when the chart represents one common overall population.
    ''' </summary>
    ReferenceTotal

    ''' <summary>
    ''' Uses the sum of node values in the node's own stage.
    ''' </summary>
    StageTotal
End Enum

''' <summary>
''' Optional color override for a Sankey category. The same category color is reused
''' when the category appears at more than one stage.
''' </summary>
Public Class SankeyCategoryAppearance
    Public Property CategoryName As String
    Public Property NodeFillColor As Nullable(Of Integer)
    Public Property RibbonFillColor As Nullable(Of Integer)

    Friend Function Copy() As SankeyCategoryAppearance
        Return New SankeyCategoryAppearance With {
            .CategoryName = CategoryName,
            .NodeFillColor = NodeFillColor,
            .RibbonFillColor = RibbonFillColor
        }
    End Function
End Class

''' <summary>
''' Excel-specific display settings for <see cref="SankeyPlotExcel"/>.
''' Colors are Excel OLE RGB integers, consistent with the other BESHStatNG graphics
''' classes.
''' </summary>
Public Class SankeyPlotAppearance
    Public Property ChartTitle As String = "Sankey chart"
    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property TextColor As Integer = &H202020

    ''' <summary>
    ''' Tableau-10-style default category palette used by newer BESHStatNG graphics.
    ''' </summary>
    Public Property NodeColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    ''' <summary>Width of each vertical node block in points. Default: 18.</summary>
    Public Property NodeWidth As Double = 18.0R

    Public Property NodeOutlineColor As Integer = &H707070
    Public Property NodeOutlineWeight As Single = 0.75F

    ''' <summary>
    ''' Selects source-category, target-category, or neutral ribbon coloring.
    ''' </summary>
    Public Property RibbonColorMode As SankeyRibbonColorMode = SankeyRibbonColorMode.SourceNode

    ''' <summary>Used when <see cref="RibbonColorMode"/> is Neutral.</summary>
    Public Property NeutralRibbonColor As Integer = &HBEBEBE

    ''' <summary>Ribbon transparency from 0 (opaque) to 1 (fully transparent).</summary>
    Public Property RibbonTransparency As Single = 0.25F

    Public Property ShowRibbonOutline As Boolean = False
    Public Property RibbonOutlineColor As Integer = &HB0B0B0
    Public Property RibbonOutlineWeight As Single = 0.5F

    ''' <summary>
    ''' Fraction of the horizontal source-to-target distance used for each Bezier
    ''' control-point offset. The default 0.5 gives a smooth conventional Sankey curve.
    ''' </summary>
    Public Property RibbonCurvature As Double = 0.5R

    Public Property ShowNodeLabels As Boolean = True
    Public Property NodeLabelMode As SankeyNodeLabelMode = SankeyNodeLabelMode.NameOnly
    Public Property PercentageBasis As SankeyPercentageBasis = SankeyPercentageBasis.ReferenceTotal
    Public Property ValueNumberFormat As String = "0.###"
    Public Property PercentageNumberFormat As String = "0%"
    Public Property NodeLabelFontSize As Double = 9.0R
    Public Property NodeLabelWidth As Double = 100.0R
    Public Property NodeLabelGap As Double = 5.0R

    Public Property ShowStageLabels As Boolean = True
    Public Property StageLabelFontSize As Double = 9.0R

    ''' <summary>
    ''' Draws a custom category legend at the right of the chart. The default is False
    ''' because node labels generally make a Sankey self-explanatory.
    ''' </summary>
    Public Property ShowLegend As Boolean = False
    Public Property LegendTitle As String = "Legend"
    Public Property LegendFontSize As Double = 8.0R
    Public Property LegendColumnWidth As Double = 110.0R
    Public Property LegendRowHeight As Double = 16.0R
    Public Property LegendSwatchSize As Double = 9.0R
    Public Property LegendGap As Double = 10.0R

    ''' <summary>Outer drawing padding inside the embedded chart, in points.</summary>
    Public Property ChartPadding As Double = 14.0R

    ''' <summary>Height reserved for a non-empty custom chart title.</summary>
    Public Property ChartTitleHeight As Double = 24.0R

    ''' <summary>Height reserved below the Sankey for optional stage names.</summary>
    Public Property StageLabelHeight As Double = 22.0R

    Public Property CategoryOverrides As SankeyCategoryAppearance() = New SankeyCategoryAppearance() {}

    Friend Function Copy() As SankeyPlotAppearance
        Dim copyOverrides As SankeyCategoryAppearance()
        If CategoryOverrides Is Nothing Then
            copyOverrides = New SankeyCategoryAppearance() {}
        Else
            copyOverrides = CategoryOverrides.Where(Function(x) x IsNot Nothing).
                Select(Function(x) x.Copy()).ToArray()
        End If

        Return New SankeyPlotAppearance With {
            .ChartTitle = ChartTitle,
            .BackgroundColor = BackgroundColor,
            .TextColor = TextColor,
            .NodeColors = If(NodeColors Is Nothing, Nothing, DirectCast(NodeColors.Clone(), Integer())),
            .NodeWidth = NodeWidth,
            .NodeOutlineColor = NodeOutlineColor,
            .NodeOutlineWeight = NodeOutlineWeight,
            .RibbonColorMode = RibbonColorMode,
            .NeutralRibbonColor = NeutralRibbonColor,
            .RibbonTransparency = RibbonTransparency,
            .ShowRibbonOutline = ShowRibbonOutline,
            .RibbonOutlineColor = RibbonOutlineColor,
            .RibbonOutlineWeight = RibbonOutlineWeight,
            .RibbonCurvature = RibbonCurvature,
            .ShowNodeLabels = ShowNodeLabels,
            .NodeLabelMode = NodeLabelMode,
            .PercentageBasis = PercentageBasis,
            .ValueNumberFormat = ValueNumberFormat,
            .PercentageNumberFormat = PercentageNumberFormat,
            .NodeLabelFontSize = NodeLabelFontSize,
            .NodeLabelWidth = NodeLabelWidth,
            .NodeLabelGap = NodeLabelGap,
            .ShowStageLabels = ShowStageLabels,
            .StageLabelFontSize = StageLabelFontSize,
            .ShowLegend = ShowLegend,
            .LegendTitle = LegendTitle,
            .LegendFontSize = LegendFontSize,
            .LegendColumnWidth = LegendColumnWidth,
            .LegendRowHeight = LegendRowHeight,
            .LegendSwatchSize = LegendSwatchSize,
            .LegendGap = LegendGap,
            .ChartPadding = ChartPadding,
            .ChartTitleHeight = ChartTitleHeight,
            .StageLabelHeight = StageLabelHeight,
            .CategoryOverrides = copyOverrides
        }
    End Function
End Class

''' <summary>
''' Excel renderer for the Excel-independent <see cref="SankeyPlotResult"/> backend.
'''
''' The chart itself is an embedded XY-scatter container with hidden series and axes.
''' Ribbons, nodes, labels, stage names, and the optional legend are native vector
''' shapes in Chart.Shapes, so they move/export with the chart. A WithEvents helper
''' redraws the normalized backend geometry after Chart.Resize.
''' </summary>
''' <remarks>
''' Microsoft.Office.Core is intentionally not a compile-time project reference in
''' BESHStatNG. FreeformBuilder, Shape, TextFrame2, and the Office enum values needed
''' by this renderer are therefore accessed through narrowly scoped late binding, as in
''' ViolinPlotExcel and the histogram box-plot overlay.
''' </remarks>
Public NotInheritable Class SankeyPlotExcel
    Private Sub New()
    End Sub

    Private Const ShapePrefix As String = "BESH_Sankey_"

    'Stable Office enum values used only through late binding.
    Private Const MsoEditingCorner As Integer = 1
    Private Const MsoSegmentLine As Integer = 0
    Private Const MsoSegmentCurve As Integer = 1
    Private Const MsoSendToBack As Integer = 1
    Private Const MsoShapeRectangle As Integer = 1
    Private Const MsoTextOrientationHorizontal As Integer = 1
    Private Const MsoFalse As Integer = 0
    Private Const MsoTrue As Integer = -1
    Private Const MsoAnchorMiddle As Integer = 3
    Private Const MsoAlignLeft As Integer = 1
    Private Const MsoAlignCenter As Integer = 2

    Private NotInheritable Class ResolvedCategoryStyle
        Friend NodeColor As Integer
        Friend RibbonColor As Integer
    End Class

    Private NotInheritable Class DrawingLayout
        Friend ChartWidth As Double
        Friend ChartHeight As Double
        Friend PlotLeft As Double
        Friend PlotTop As Double
        Friend PlotWidth As Double
        Friend PlotHeight As Double
        Friend StageCentreLeft As Double
        Friend StageCentreRight As Double
        Friend LegendLeft As Double
        Friend LegendTop As Double
        Friend LegendWidth As Double
        Friend LegendHeight As Double
        Friend LegendColumns As Integer
        Friend LegendRowsPerColumn As Integer
        Friend FinalLabelReserve As Double
    End Class

    Private NotInheritable Class LegendItem
        Friend ColorKey As String
        Friend Label As String
        Friend Color As Integer
        Friend FirstAppearanceIndex As Integer
    End Class

    'Keep chart event sinks alive for as long as their embedded charts are usable.
    'Stale handlers are pruned whenever another Sankey chart is attached.
    Private Shared ReadOnly pOverlays As New List(Of SankeyChartOverlay)()

    ''' <summary>
    ''' Adds an embedded vector Sankey chart to <paramref name="ws"/>.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As SankeyPlotResult,
                                    Optional appearance As SankeyPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 760.0R,
                                    Optional height As Double = 460.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.NodeCount < 1 Then Throw New ArgumentException("The Sankey result contains no nodes.", NameOf(result))
        If result.LinkCount < 1 Then Throw New ArgumentException("The Sankey result contains no links.", NameOf(result))
        If result.StageCount < 2 Then Throw New ArgumentException("A Sankey chart requires at least two stages.", NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width must be finite and positive.")
        End If
        If Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(height), "Chart height must be finite and positive.")
        End If

        Dim resolvedAppearance As SankeyPlotAppearance = If(appearance, New SankeyPlotAppearance()).Copy()
        ValidateAppearance(resolvedAppearance)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter, left, top, width, height)
            Dim chart As Chart = chartShape.Chart

            ConfigureChartContainer(chart, resolvedAppearance)
            AttachOverlay(chart, result, resolvedAppearance)
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

    ''' <summary>
    ''' Forces an immediate redraw for a chart previously created by AddChart.
    ''' Normally this is unnecessary because Chart.Resize is handled automatically.
    ''' </summary>
    Public Shared Sub Redraw(chart As Chart)
        If chart Is Nothing Then Throw New ArgumentNullException(NameOf(chart))

        PruneDeadOverlays()
        For Each overlay As SankeyChartOverlay In pOverlays
            If overlay.OwnsChart(chart) Then
                overlay.Draw()
                Return
            End If
        Next

        Throw New ArgumentException("The chart is not registered as a BESHStatNG Sankey chart.", NameOf(chart))
    End Sub

    Private Shared Sub ConfigureChartContainer(chart As Object, appearance As SankeyPlotAppearance)
        chart.ChartType = XlChartType.xlXYScatter
        chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
        chart.PlotVisibleOnly = False
        chart.ChartArea.AutoScaleFont = False

        Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
        DeleteAllSeries(seriesCollection)

        'A fully invisible anchor series keeps Excel from showing an empty-chart
        'placeholder and gives the embedded Chart a stable plotting container.
        '
        'Do not rely on HasAxis = False here. Some Excel builds recreate the
        'scatter-chart axes (and their major gridlines) when the first series is
        'added/refreshed. The axes are therefore kept but explicitly made invisible.
        Dim anchorSeries As Object = seriesCollection.NewSeries()
        With anchorSeries
            'A blank series name also prevents Excel from using the internal anchor
            'name as an automatically generated chart title on some chart styles.
            .Name = " "
            .ChartType = XlChartType.xlXYScatter
            .XValues = New Double() {0.0R, 1.0R}
            .Values = New Double() {0.0R, 1.0R}
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
        End With

        'Adding the first series can cause Excel to reapply parts of the chart's
        'default layout, including a title and major horizontal gridlines. Suppress
        'those elements only after the anchor series exists.
        SuppressAnchorChartFurniture(chart)

        With chart.ChartArea.Format.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = appearance.BackgroundColor
            .Transparency = 0.0F
        End With
        chart.ChartArea.Format.Line.Visible = False

        Try
            chart.PlotArea.Format.Fill.Visible = False
            chart.PlotArea.Format.Line.Visible = False
        Catch
            'Some Excel builds do not expose all PlotArea format members until after
            'the first refresh. The custom shapes do not depend on them.
        End Try

        chart.Refresh()

        'Refresh can also restore chart-style defaults in some Excel versions.
        'Repeat the suppression once more so the internal anchor never leaks into
        'the user-visible Sankey chart.
        SuppressAnchorChartFurniture(chart)
    End Sub

    ''' <summary>
    ''' Removes all Excel-native title/legend/axis furniture belonging to the hidden
    ''' anchor series. This is repeated after Refresh because some Excel chart styles
    ''' can restore default elements when the chart is recalculated.
    ''' </summary>
    Private Shared Sub SuppressAnchorChartFurniture(chart As Object)
        Try
            chart.HasLegend = False
        Catch
        End Try
        Try
            chart.HasTitle = False
        Catch
        End Try
        HideAnchorAxes(chart)
    End Sub

    ''' <summary>
    ''' Keeps the XY-scatter anchor axes available for Excel internally while making
    ''' all Cartesian chart furniture invisible. This is more reliable than deleting
    ''' the axes because Excel may recreate them when the anchor series is refreshed.
    ''' </summary>
    Private Shared Sub HideAnchorAxes(chart As Object)
        HideAnchorAxis(chart, XlAxisType.xlCategory)
        HideAnchorAxis(chart, XlAxisType.xlValue)
    End Sub

    Private Shared Sub HideAnchorAxis(chart As Object, axisType As XlAxisType)
        Try
            Dim axis As Axis = DirectCast(chart.Axes(axisType, XlAxisGroup.xlPrimary), Axis)

            With axis
                .HasTitle = False
                .HasMajorGridlines = False
                .HasMinorGridlines = False
                .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                .MajorTickMark = XlTickMark.xlTickMarkNone
                .MinorTickMark = XlTickMark.xlTickMarkNone
            End With

            'Delete any gridline objects that may already have been created by the
            'chart style before HasMajorGridlines/HasMinorGridlines were cleared.
            Try
                axis.MajorGridlines.Delete()
            Catch
            End Try
            Try
                axis.MinorGridlines.Delete()
            Catch
            End Try

            'Keep Microsoft.Office.Core out of the compile-time references, as in
            'the rest of this renderer.
            Try
                Dim axisObject As Object = axis
                axisObject.Format.Line.Visible = False
            Catch
            End Try
        Catch
            'If Excel reports an axis as unavailable there is nothing visible to hide.
        End Try
    End Sub

    Private Shared Sub AttachOverlay(chart As Chart,
                                     result As SankeyPlotResult,
                                     appearance As SankeyPlotAppearance)
        PruneDeadOverlays()

        Dim overlay As New SankeyChartOverlay(chart, result, appearance)
        overlay.Draw()
        pOverlays.Add(overlay)
    End Sub

    Private Shared Sub PruneDeadOverlays()
        For i As Integer = pOverlays.Count - 1 To 0 Step -1
            If Not pOverlays(i).IsChartAvailable Then
                pOverlays(i).Detach()
                pOverlays.RemoveAt(i)
            End If
        Next
    End Sub

    Private Shared Sub ValidateAppearance(appearance As SankeyPlotAppearance)
        If appearance.NodeColors Is Nothing OrElse appearance.NodeColors.Length = 0 Then
            Throw New ArgumentException("NodeColors must contain at least one color.", NameOf(appearance.NodeColors))
        End If
        If Not IsFinitePositive(appearance.NodeWidth) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeWidth), "NodeWidth must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.NodeOutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeOutlineWeight), "NodeOutlineWeight must be finite and positive.")
        End If
        If Not IsFiniteFraction(appearance.RibbonTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.RibbonTransparency), "RibbonTransparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.RibbonOutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.RibbonOutlineWeight), "RibbonOutlineWeight must be finite and positive.")
        End If
        If Not IsFinite(appearance.RibbonCurvature) OrElse appearance.RibbonCurvature < 0.05R OrElse appearance.RibbonCurvature > 0.95R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.RibbonCurvature), "RibbonCurvature must be between 0.05 and 0.95.")
        End If
        If Not [Enum].IsDefined(GetType(SankeyRibbonColorMode), appearance.RibbonColorMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.RibbonColorMode))
        End If
        If Not [Enum].IsDefined(GetType(SankeyNodeLabelMode), appearance.NodeLabelMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeLabelMode))
        End If
        If Not [Enum].IsDefined(GetType(SankeyPercentageBasis), appearance.PercentageBasis) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.PercentageBasis))
        End If
        If Not IsFinitePositive(appearance.NodeLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeLabelFontSize))
        End If
        If Not IsFinitePositive(appearance.NodeLabelWidth) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeLabelWidth))
        End If
        If Not IsFinite(appearance.NodeLabelGap) OrElse appearance.NodeLabelGap < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.NodeLabelGap))
        End If
        If Not IsFinitePositive(appearance.StageLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.StageLabelFontSize))
        End If
        If Not IsFinitePositive(appearance.LegendFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LegendFontSize))
        End If
        If Not IsFinitePositive(appearance.LegendColumnWidth) OrElse
           Not IsFinitePositive(appearance.LegendRowHeight) OrElse
           Not IsFinitePositive(appearance.LegendSwatchSize) Then
            Throw New ArgumentOutOfRangeException("Legend dimensions", "Legend dimensions must be finite and positive.")
        End If
        If Not IsFinite(appearance.LegendGap) OrElse appearance.LegendGap < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LegendGap))
        End If
        If Not IsFinite(appearance.ChartPadding) OrElse appearance.ChartPadding < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ChartPadding))
        End If
        If Not IsFinite(appearance.ChartTitleHeight) OrElse appearance.ChartTitleHeight < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ChartTitleHeight))
        End If
        If Not IsFinite(appearance.StageLabelHeight) OrElse appearance.StageLabelHeight < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.StageLabelHeight))
        End If

        ValidateNumberFormat(appearance.ValueNumberFormat, "ValueNumberFormat", 1234.5R)
        ValidateNumberFormat(appearance.PercentageNumberFormat, "PercentageNumberFormat", 0.375R)
    End Sub

    Private Shared Sub ValidateNumberFormat(format As String,
                                            parameterName As String,
                                            sampleValue As Double)
        If String.IsNullOrWhiteSpace(format) Then
            Throw New ArgumentException(parameterName & " cannot be blank.", parameterName)
        End If
        Try
            Dim unused As String = sampleValue.ToString(format, CultureInfo.CurrentCulture)
        Catch ex As FormatException
            Throw New ArgumentException(parameterName & " is not a valid numeric format string.", parameterName, ex)
        End Try
    End Sub

    Private Shared Function BuildDrawingLayout(chart As Chart,
                                               result As SankeyPlotResult,
                                               appearance As SankeyPlotAppearance,
                                               legendItems As List(Of LegendItem)) As DrawingLayout
        Dim chartWidth As Double
        Dim chartHeight As Double
        GetChartSize(chart, chartWidth, chartHeight)

        Dim padding As Double = appearance.ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(appearance.ChartTitle), 0.0R, appearance.ChartTitleHeight)
        Dim stageLabelHeight As Double = If(appearance.ShowStageLabels, appearance.StageLabelHeight, 0.0R)

        Dim provisionalPlotHeight As Double = chartHeight - 2.0R * padding - titleHeight - stageLabelHeight
        If provisionalPlotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw the Sankey diagram. Increase its height.")
        End If

        Dim legendColumns As Integer = 0
        Dim legendRowsPerColumn As Integer = 0
        Dim legendWidth As Double = 0.0R
        If appearance.ShowLegend AndAlso legendItems.Count > 0 Then
            Dim legendTitleRows As Integer = If(String.IsNullOrWhiteSpace(appearance.LegendTitle), 0, 1)
            Dim availableRowsHeight As Double = provisionalPlotHeight - legendTitleRows * appearance.LegendRowHeight
            legendRowsPerColumn = Math.Max(1, CInt(Math.Floor(availableRowsHeight / appearance.LegendRowHeight)))
            legendColumns = CInt(Math.Ceiling(CDbl(legendItems.Count) / CDbl(legendRowsPerColumn)))
            legendWidth = appearance.LegendGap + legendColumns * appearance.LegendColumnWidth
        End If

        Dim finalLabelReserve As Double = If(appearance.ShowNodeLabels,
                                              appearance.NodeLabelGap + appearance.NodeLabelWidth,
                                              0.0R)

        Dim plotLeft As Double = padding
        Dim plotTop As Double = padding + titleHeight
        Dim plotRight As Double = chartWidth - padding - legendWidth - finalLabelReserve
        Dim plotBottom As Double = chartHeight - padding - stageLabelHeight
        Dim plotWidth As Double = plotRight - plotLeft
        Dim plotHeight As Double = plotBottom - plotTop

        If plotWidth < Math.Max(100.0R, appearance.NodeWidth * result.StageCount * 1.5R) Then
            Throw New InvalidOperationException("The embedded chart is too narrow to draw this Sankey diagram. Increase its width or hide node labels/legend.")
        End If
        If plotHeight < 60.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw this Sankey diagram. Increase its height.")
        End If

        Dim layout As New DrawingLayout With {
            .ChartWidth = chartWidth,
            .ChartHeight = chartHeight,
            .PlotLeft = plotLeft,
            .PlotTop = plotTop,
            .PlotWidth = plotWidth,
            .PlotHeight = plotHeight,
            .StageCentreLeft = plotLeft + appearance.NodeWidth / 2.0R,
            .StageCentreRight = plotRight - appearance.NodeWidth / 2.0R,
            .LegendLeft = chartWidth - padding - legendWidth + appearance.LegendGap,
            .LegendTop = plotTop,
            .LegendWidth = Math.Max(0.0R, legendWidth - appearance.LegendGap),
            .LegendHeight = plotHeight,
            .LegendColumns = legendColumns,
            .LegendRowsPerColumn = legendRowsPerColumn,
            .FinalLabelReserve = finalLabelReserve
        }

        If layout.StageCentreRight <= layout.StageCentreLeft Then
            Throw New InvalidOperationException("The chart does not have enough horizontal space for the Sankey nodes.")
        End If
        Return layout
    End Function

    Private Shared Sub GetChartSize(chart As Chart,
                                    ByRef width As Double,
                                    ByRef height As Double)
        width = 0.0R
        height = 0.0R

        Try
            width = CDbl(chart.ChartArea.Width)
            height = CDbl(chart.ChartArea.Height)
        Catch
        End Try

        If width <= 0.0R OrElse height <= 0.0R Then
            Try
                Dim parent As Object = chart.Parent
                width = CDbl(parent.Width)
                height = CDbl(parent.Height)
            Catch
            End Try
        End If

        If width <= 0.0R OrElse height <= 0.0R OrElse Not IsFinite(width) OrElse Not IsFinite(height) Then
            Throw New InvalidOperationException("Excel returned an invalid chart size while drawing the Sankey diagram.")
        End If
    End Sub

    Private Shared Function ResolveCategoryStyles(result As SankeyPlotResult,
                                                   appearance As SankeyPlotAppearance,
                                                   ByRef legendItems As List(Of LegendItem)) As Dictionary(Of String, ResolvedCategoryStyle)
        Dim styles As New Dictionary(Of String, ResolvedCategoryStyle)(StringComparer.Ordinal)
        legendItems = New List(Of LegendItem)()

        Dim nodes As SankeyNode() = result.Nodes.OrderBy(Function(n) n.FirstAppearanceIndex).ToArray()
        Dim paletteIndex As Integer = 0

        For Each node As SankeyNode In nodes
            If styles.ContainsKey(node.ColorKey) Then Continue For

            Dim nodeColor As Integer = appearance.NodeColors(paletteIndex Mod appearance.NodeColors.Length)
            Dim ribbonColor As Integer = nodeColor
            paletteIndex += 1

            Dim overrideItem As SankeyCategoryAppearance = FindCategoryOverride(node.Label, appearance.CategoryOverrides)
            If overrideItem IsNot Nothing Then
                If overrideItem.NodeFillColor.HasValue Then nodeColor = overrideItem.NodeFillColor.Value
                If overrideItem.RibbonFillColor.HasValue Then
                    ribbonColor = overrideItem.RibbonFillColor.Value
                Else
                    ribbonColor = nodeColor
                End If
            End If

            styles.Add(node.ColorKey,
                       New ResolvedCategoryStyle With {
                           .NodeColor = nodeColor,
                           .RibbonColor = ribbonColor
                       })

            legendItems.Add(New LegendItem With {
                .ColorKey = node.ColorKey,
                .Label = node.Label,
                .Color = nodeColor,
                .FirstAppearanceIndex = node.FirstAppearanceIndex
            })
        Next

        legendItems = legendItems.OrderBy(Function(x) x.FirstAppearanceIndex).ToList()
        Return styles
    End Function

    Private Shared Function FindCategoryOverride(categoryName As String,
                                                 _overrides As SankeyCategoryAppearance()) As SankeyCategoryAppearance
        If _overrides Is Nothing Then Return Nothing
        For Each item As SankeyCategoryAppearance In _overrides
            If item Is Nothing OrElse String.IsNullOrWhiteSpace(item.CategoryName) Then Continue For
            If String.Equals(item.CategoryName.Trim(),
                             categoryName,
                             StringComparison.CurrentCultureIgnoreCase) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Shared Sub DrawSankey(chart As Chart,
                                  result As SankeyPlotResult,
                                  appearance As SankeyPlotAppearance)
        Dim legendItems As List(Of LegendItem) = Nothing
        Dim styles As Dictionary(Of String, ResolvedCategoryStyle) = ResolveCategoryStyles(result, appearance, legendItems)
        Dim layout As DrawingLayout = BuildDrawingLayout(chart, result, appearance, legendItems)

        Dim chartObject As Object = chart
        Dim chartShapes As Object = chartObject.Shapes

        DrawTitle(chartShapes, appearance, layout)

        Dim nodes As SankeyNode() = result.Nodes
        Dim links As SankeyLink() = result.Links
        Dim nodeById As New Dictionary(Of String, SankeyNode)(StringComparer.Ordinal)
        For Each node As SankeyNode In nodes
            nodeById(node.Id) = node
        Next

        'Ribbons first, then node blocks and text. This ensures labels remain legible
        'and node edges stay crisp even where many flows meet.
        For linkIndex As Integer = 0 To links.Length - 1
            Dim link As SankeyLink = links(linkIndex)
            Dim sourceNode As SankeyNode = nodeById(link.SourceNodeId)
            Dim targetNode As SankeyNode = nodeById(link.TargetNodeId)
            DrawRibbon(chartShapes,
                       link,
                       sourceNode,
                       targetNode,
                       styles,
                       appearance,
                       layout,
                       linkIndex)
        Next

        For nodeIndex As Integer = 0 To nodes.Length - 1
            DrawNode(chartShapes,
                     nodes(nodeIndex),
                     styles(nodes(nodeIndex).ColorKey),
                     appearance,
                     layout,
                     nodeIndex)
        Next

        If appearance.ShowNodeLabels Then
            Dim stageX As Double() = BuildStageCentres(result.StageCount, layout)
            For nodeIndex As Integer = 0 To nodes.Length - 1
                DrawNodeLabel(chartShapes,
                              nodes(nodeIndex),
                              result,
                              appearance,
                              layout,
                              stageX,
                              nodeIndex)
            Next
        End If

        If appearance.ShowStageLabels Then
            DrawStageLabels(chartShapes, result, appearance, layout)
        End If

        If appearance.ShowLegend AndAlso legendItems.Count > 0 Then
            DrawLegend(chartShapes, legendItems, appearance, layout)
        End If
    End Sub

    Private Shared Sub DrawTitle(chartShapes As Object,
                                 appearance As SankeyPlotAppearance,
                                 layout As DrawingLayout)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) OrElse appearance.ChartTitleHeight <= 0.0R Then Return

        Dim titleWidth As Double = layout.ChartWidth - 2.0R * appearance.ChartPadding
        If layout.LegendWidth > 0.0R Then
            titleWidth = Math.Max(30.0R, layout.LegendLeft - appearance.ChartPadding)
        End If

        AddTextShape(chartShapes,
                     ShapePrefix & "Title",
                     appearance.ChartTitle.Trim(),
                     appearance.ChartPadding,
                     appearance.ChartPadding,
                     titleWidth,
                     appearance.ChartTitleHeight,
                     appearance.TextColor,
                     13.0R,
                     True,
                     MsoAlignCenter)
    End Sub

    Private Shared Sub DrawRibbon(chartShapes As Object,
                                  link As SankeyLink,
                                  sourceNode As SankeyNode,
                                  targetNode As SankeyNode,
                                  styles As Dictionary(Of String, ResolvedCategoryStyle),
                                  appearance As SankeyPlotAppearance,
                                  layout As DrawingLayout,
                                  linkIndex As Integer)
        Dim sourceCentreX As Double = NormalizedXToChartX(sourceNode.X, layout)
        Dim targetCentreX As Double = NormalizedXToChartX(targetNode.X, layout)
        Dim sourceX As Double = sourceCentreX + appearance.NodeWidth / 2.0R
        Dim targetX As Double = targetCentreX - appearance.NodeWidth / 2.0R

        If targetX <= sourceX Then
            'The backend guarantees forward stages; this guard protects against an
            'impossibly narrow chart after aggressive manual resizing.
            Return
        End If

        Dim sourceTop As Double = NormalizedYToChartY(link.SourceTop, layout)
        Dim sourceBottom As Double = NormalizedYToChartY(link.SourceTop + link.Thickness, layout)
        Dim targetTop As Double = NormalizedYToChartY(link.TargetTop, layout)
        Dim targetBottom As Double = NormalizedYToChartY(link.TargetTop + link.Thickness, layout)

        Dim dx As Double = targetX - sourceX
        Dim controlOffset As Double = dx * appearance.RibbonCurvature
        Dim c1x As Double = sourceX + controlOffset
        Dim c2x As Double = targetX - controlOffset

        Dim builder As Object = chartShapes.BuildFreeform(MsoEditingCorner,
                                                           CSng(sourceX),
                                                           CSng(sourceTop))

        'Upper Bezier edge: source -> target.
        builder.AddNodes(MsoSegmentCurve,
                         MsoEditingCorner,
                         CSng(c1x), CSng(sourceTop),
                         CSng(c2x), CSng(targetTop),
                         CSng(targetX), CSng(targetTop))

        'Target-side vertical edge.
        builder.AddNodes(MsoSegmentLine,
                         MsoEditingCorner,
                         CSng(targetX), CSng(targetBottom))

        'Lower Bezier edge: target -> source.
        builder.AddNodes(MsoSegmentCurve,
                         MsoEditingCorner,
                         CSng(c2x), CSng(targetBottom),
                         CSng(c1x), CSng(sourceBottom),
                         CSng(sourceX), CSng(sourceBottom))

        'Close along the source-side node edge.
        builder.AddNodes(MsoSegmentLine,
                         MsoEditingCorner,
                         CSng(sourceX), CSng(sourceTop))

        Dim ribbonShape As Object = builder.ConvertToShape()
        ribbonShape.Name = ShapePrefix & "Ribbon_" & (linkIndex + 1).ToString(CultureInfo.InvariantCulture)

        Dim ribbonColor As Integer
        Select Case appearance.RibbonColorMode
            Case SankeyRibbonColorMode.SourceNode
                ribbonColor = styles(sourceNode.ColorKey).RibbonColor
            Case SankeyRibbonColorMode.TargetNode
                ribbonColor = styles(targetNode.ColorKey).RibbonColor
            Case SankeyRibbonColorMode.Neutral
                ribbonColor = appearance.NeutralRibbonColor
            Case Else
                ribbonColor = styles(sourceNode.ColorKey).RibbonColor
        End Select

        With ribbonShape.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = ribbonColor
            .Transparency = appearance.RibbonTransparency
        End With
        With ribbonShape.Line
            .Visible = appearance.ShowRibbonOutline
            If appearance.ShowRibbonOutline Then
                .ForeColor.RGB = appearance.RibbonOutlineColor
                .Weight = appearance.RibbonOutlineWeight
                .Transparency = Math.Min(0.8F, appearance.RibbonTransparency)
            End If
        End With

        Try
            ribbonShape.ZOrder(MsoSendToBack)
        Catch
            'Z-order is only a safeguard; ribbons are already created before nodes.
        End Try
    End Sub

    Private Shared Sub DrawNode(chartShapes As Object,
                                node As SankeyNode,
                                style As ResolvedCategoryStyle,
                                appearance As SankeyPlotAppearance,
                                layout As DrawingLayout,
                                nodeIndex As Integer)
        Dim centreX As Double = NormalizedXToChartX(node.X, layout)
        Dim nodeTop As Double = NormalizedYToChartY(node.Y, layout)
        Dim nodeHeight As Double = Math.Max(0.5R, node.Height * layout.PlotHeight)
        Dim nodeLeft As Double = centreX - appearance.NodeWidth / 2.0R

        Dim nodeShape As Object = chartShapes.AddShape(MsoShapeRectangle,
                                                       CSng(nodeLeft),
                                                       CSng(nodeTop),
                                                       CSng(appearance.NodeWidth),
                                                       CSng(nodeHeight))
        nodeShape.Name = ShapePrefix & "Node_" & (nodeIndex + 1).ToString(CultureInfo.InvariantCulture)

        With nodeShape.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = style.NodeColor
            .Transparency = 0.0F
        End With
        With nodeShape.Line
            .Visible = True
            .ForeColor.RGB = appearance.NodeOutlineColor
            .Weight = appearance.NodeOutlineWeight
            .Transparency = 0.0F
        End With
    End Sub

    Private Shared Sub DrawNodeLabel(chartShapes As Object,
                                     node As SankeyNode,
                                     result As SankeyPlotResult,
                                     appearance As SankeyPlotAppearance,
                                     layout As DrawingLayout,
                                     stageX As Double(),
                                     nodeIndex As Integer)
        Dim centreX As Double = NormalizedXToChartX(node.X, layout)
        Dim nodeRight As Double = centreX + appearance.NodeWidth / 2.0R
        Dim labelLeft As Double = nodeRight + appearance.NodeLabelGap
        Dim labelWidth As Double = appearance.NodeLabelWidth

        If node.StageIndex < stageX.Length - 1 Then
            Dim nextNodeLeft As Double = stageX(node.StageIndex + 1) - appearance.NodeWidth / 2.0R
            Dim interStageWidth As Double = nextNodeLeft - labelLeft - 3.0R
            If interStageWidth > 20.0R Then
                labelWidth = Math.Min(labelWidth, interStageWidth)
            Else
                labelWidth = Math.Max(20.0R, interStageWidth)
            End If
        End If

        Dim nodeTop As Double = NormalizedYToChartY(node.Y, layout)
        Dim nodeHeight As Double = Math.Max(0.5R, node.Height * layout.PlotHeight)
        Dim centreY As Double = nodeTop + nodeHeight / 2.0R
        Dim labelHeight As Double = Math.Max(18.0R, appearance.NodeLabelFontSize * 2.5R)
        Dim labelTop As Double = Math.Max(layout.PlotTop,
                                         Math.Min(layout.PlotTop + layout.PlotHeight - labelHeight,
                                                  centreY - labelHeight / 2.0R))

        AddTextShape(chartShapes,
                     ShapePrefix & "NodeLabel_" & (nodeIndex + 1).ToString(CultureInfo.InvariantCulture),
                     FormatNodeLabel(node, result, appearance),
                     labelLeft,
                     labelTop,
                     labelWidth,
                     labelHeight,
                     appearance.TextColor,
                     appearance.NodeLabelFontSize,
                     False,
                     MsoAlignLeft)
    End Sub

    Private Shared Function FormatNodeLabel(node As SankeyNode,
                                            result As SankeyPlotResult,
                                            appearance As SankeyPlotAppearance) As String
        Dim valueText As String = node.Value.ToString(appearance.ValueNumberFormat, CultureInfo.CurrentCulture)
        Dim denominator As Double

        Select Case appearance.PercentageBasis
            Case SankeyPercentageBasis.ReferenceTotal
                denominator = result.ReferenceTotal
            Case SankeyPercentageBasis.StageTotal
                Dim totals As Double() = result.StageTotals
                If node.StageIndex >= 0 AndAlso node.StageIndex < totals.Length Then
                    denominator = totals(node.StageIndex)
                End If
        End Select

        Dim percentage As Double = If(denominator > 0.0R, node.Value / denominator, 0.0R)
        Dim percentageText As String = percentage.ToString(appearance.PercentageNumberFormat, CultureInfo.CurrentCulture)

        Select Case appearance.NodeLabelMode
            Case SankeyNodeLabelMode.NameOnly
                Return node.Label
            Case SankeyNodeLabelMode.NameAndValue
                Return node.Label & " (" & valueText & ")"
            Case SankeyNodeLabelMode.NameAndPercentage
                Return node.Label & " " & percentageText
            Case SankeyNodeLabelMode.NamePercentageAndValue
                Return node.Label & " " & percentageText & " (" & valueText & ")"
            Case Else
                Return node.Label
        End Select
    End Function

    Private Shared Sub DrawStageLabels(chartShapes As Object,
                                       result As SankeyPlotResult,
                                       appearance As SankeyPlotAppearance,
                                       layout As DrawingLayout)
        Dim names As String() = result.StageNames
        Dim stageX As Double() = BuildStageCentres(result.StageCount, layout)
        Dim stepWidth As Double = If(result.StageCount <= 1,
                                     layout.PlotWidth,
                                     (layout.StageCentreRight - layout.StageCentreLeft) / CDbl(result.StageCount - 1))
        Dim labelWidth As Double = Math.Max(50.0R, Math.Min(150.0R, stepWidth * 0.9R))
        Dim top As Double = layout.PlotTop + layout.PlotHeight + 1.0R

        For i As Integer = 0 To names.Length - 1
            If String.IsNullOrWhiteSpace(names(i)) Then Continue For

            AddTextShape(chartShapes,
                         ShapePrefix & "StageLabel_" & (i + 1).ToString(CultureInfo.InvariantCulture),
                         names(i).Trim(),
                         stageX(i) - labelWidth / 2.0R,
                         top,
                         labelWidth,
                         appearance.StageLabelHeight,
                         appearance.TextColor,
                         appearance.StageLabelFontSize,
                         True,
                         MsoAlignCenter)
        Next
    End Sub

    Private Shared Sub DrawLegend(chartShapes As Object,
                                  items As List(Of LegendItem),
                                  appearance As SankeyPlotAppearance,
                                  layout As DrawingLayout)
        If layout.LegendColumns <= 0 OrElse layout.LegendRowsPerColumn <= 0 Then Return

        Dim titleOffset As Double = 0.0R
        If Not String.IsNullOrWhiteSpace(appearance.LegendTitle) Then
            AddTextShape(chartShapes,
                         ShapePrefix & "LegendTitle",
                         appearance.LegendTitle.Trim(),
                         layout.LegendLeft,
                         layout.LegendTop,
                         layout.LegendWidth,
                         appearance.LegendRowHeight,
                         appearance.TextColor,
                         appearance.LegendFontSize,
                         True,
                         MsoAlignLeft)
            titleOffset = appearance.LegendRowHeight
        End If

        For i As Integer = 0 To items.Count - 1
            Dim column As Integer = i \ layout.LegendRowsPerColumn
            Dim row As Integer = i Mod layout.LegendRowsPerColumn
            Dim itemLeft As Double = layout.LegendLeft + column * appearance.LegendColumnWidth
            Dim itemTop As Double = layout.LegendTop + titleOffset + row * appearance.LegendRowHeight
            Dim swatchTop As Double = itemTop + (appearance.LegendRowHeight - appearance.LegendSwatchSize) / 2.0R

            Dim swatch As Object = chartShapes.AddShape(MsoShapeRectangle,
                                                        CSng(itemLeft),
                                                        CSng(swatchTop),
                                                        CSng(appearance.LegendSwatchSize),
                                                        CSng(appearance.LegendSwatchSize))
            swatch.Name = ShapePrefix & "LegendSwatch_" & (i + 1).ToString(CultureInfo.InvariantCulture)
            With swatch.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = items(i).Color
                .Transparency = 0.0F
            End With
            With swatch.Line
                .Visible = True
                .ForeColor.RGB = appearance.NodeOutlineColor
                .Weight = 0.5F
            End With

            Dim textLeft As Double = itemLeft + appearance.LegendSwatchSize + 4.0R
            Dim textWidth As Double = Math.Max(20.0R,
                                               appearance.LegendColumnWidth - appearance.LegendSwatchSize - 6.0R)
            AddTextShape(chartShapes,
                         ShapePrefix & "LegendLabel_" & (i + 1).ToString(CultureInfo.InvariantCulture),
                         items(i).Label,
                         textLeft,
                         itemTop,
                         textWidth,
                         appearance.LegendRowHeight,
                         appearance.TextColor,
                         appearance.LegendFontSize,
                         False,
                         MsoAlignLeft)
        Next
    End Sub

    Private Shared Function BuildStageCentres(stageCount As Integer,
                                              layout As DrawingLayout) As Double()
        Dim result(stageCount - 1) As Double
        If stageCount = 1 Then
            result(0) = (layout.StageCentreLeft + layout.StageCentreRight) / 2.0R
            Return result
        End If

        For i As Integer = 0 To stageCount - 1
            result(i) = layout.StageCentreLeft +
                        CDbl(i) / CDbl(stageCount - 1) *
                        (layout.StageCentreRight - layout.StageCentreLeft)
        Next
        Return result
    End Function

    Private Shared Function NormalizedXToChartX(value As Double,
                                                layout As DrawingLayout) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return layout.StageCentreLeft + clipped * (layout.StageCentreRight - layout.StageCentreLeft)
    End Function

    Private Shared Function NormalizedYToChartY(value As Double,
                                                layout As DrawingLayout) As Double
        Dim clipped As Double = Math.Max(0.0R, Math.Min(1.0R, value))
        Return layout.PlotTop + clipped * layout.PlotHeight
    End Function

    Private Shared Function AddTextShape(chartShapes As Object,
                                         shapeName As String,
                                         text As String,
                                         left As Double,
                                         top As Double,
                                         width As Double,
                                         height As Double,
                                         color As Integer,
                                         fontSize As Double,
                                         bold As Boolean,
                                         alignment As Integer) As Object
        Dim textShape As Object = chartShapes.AddTextbox(MsoTextOrientationHorizontal,
                                                         CSng(left),
                                                         CSng(top),
                                                         CSng(Math.Max(1.0R, width)),
                                                         CSng(Math.Max(1.0R, height)))
        textShape.Name = shapeName
        textShape.Fill.Visible = False
        textShape.Line.Visible = False

        With textShape.TextFrame2
            .MarginLeft = 0.0F
            .MarginRight = 0.0F
            .MarginTop = 0.0F
            .MarginBottom = 0.0F
            .WordWrap = MsoTrue
            .VerticalAnchor = MsoAnchorMiddle
            .TextRange.Text = If(text, String.Empty)
            .TextRange.ParagraphFormat.Alignment = alignment
            With .TextRange.Font
                .Name = "Calibri"
                .Size = CSng(fontSize)
                .Bold = If(bold, MsoTrue, MsoFalse)
                .Fill.Visible = True
                .Fill.Solid()
                .Fill.ForeColor.RGB = color
            End With
        End With

        Return textShape
    End Function

    Private Shared Sub DeleteSankeyShapes(chart As Chart)
        Dim chartObject As Object = chart
        Dim chartShapes As Object = chartObject.Shapes

        For i As Integer = CInt(chartShapes.Count) To 1 Step -1
            Try
                Dim shp As Object = chartShapes.Item(i)
                Dim shapeName As String = Convert.ToString(shp.Name, CultureInfo.InvariantCulture)
                If shapeName IsNot Nothing AndAlso
                   shapeName.StartsWith(ShapePrefix, StringComparison.Ordinal) Then
                    shp.Delete()
                End If
            Catch
                'A shape can disappear while Excel is processing a resize. Continue
                'with the remaining BESHStatNG shapes and recreate the full layer.
            End Try
        Next
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function SameChart(first As Chart,
                                      second As Chart) As Boolean
        If first Is Nothing OrElse second Is Nothing Then Return False
        If Object.ReferenceEquals(first, second) Then Return True

        Try
            If Not String.Equals(first.Name, second.Name, StringComparison.Ordinal) Then Return False

            'COM can return a new RCW for the same ChartObject/Worksheet, so reference
            'identity is not a reliable comparison. Compare the containing worksheet
            'and workbook names when they are available.
            Dim firstChartObject As Object = first.Parent
            Dim secondChartObject As Object = second.Parent
            Dim firstSheet As Object = firstChartObject.Parent
            Dim secondSheet As Object = secondChartObject.Parent

            If Not String.Equals(Convert.ToString(firstSheet.Name, CultureInfo.InvariantCulture),
                                 Convert.ToString(secondSheet.Name, CultureInfo.InvariantCulture),
                                 StringComparison.Ordinal) Then Return False

            Dim firstBook As Object = firstSheet.Parent
            Dim secondBook As Object = secondSheet.Parent
            Return String.Equals(Convert.ToString(firstBook.Name, CultureInfo.InvariantCulture),
                                 Convert.ToString(secondBook.Name, CultureInfo.InvariantCulture),
                                 StringComparison.Ordinal)
        Catch
            Return False
        End Try
    End Function

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    Private Shared Function IsFiniteFraction(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso
               Not Single.IsInfinity(value) AndAlso
               value >= 0.0F AndAlso value <= 1.0F
    End Function

    ''' <summary>
    ''' Owns the chart-contained Sankey layer and reprojects the normalized backend
    ''' result whenever the embedded chart is resized.
    ''' </summary>
    Private NotInheritable Class SankeyChartOverlay
        Private WithEvents pChart As Chart
        Private ReadOnly pResult As SankeyPlotResult
        Private ReadOnly pAppearance As SankeyPlotAppearance
        Private pIsDrawing As Boolean

        Friend Sub New(chart As Chart,
                       result As SankeyPlotResult,
                       appearance As SankeyPlotAppearance)
            pChart = chart
            pResult = result
            pAppearance = appearance.Copy()
        End Sub

        Friend ReadOnly Property IsChartAvailable As Boolean
            Get
                If pChart Is Nothing Then Return False
                Try
                    Dim unused As String = pChart.Name
                    Return True
                Catch
                    Return False
                End Try
            End Get
        End Property

        Friend Function OwnsChart(chart As Chart) As Boolean
            Return SameChart(pChart, chart)
        End Function

        Friend Sub Detach()
            pChart = Nothing
        End Sub

        Friend Sub Draw()
            If pChart Is Nothing OrElse pIsDrawing Then Exit Sub

            pIsDrawing = True
            Try
                DeleteSankeyShapes(pChart)
                DrawSankey(pChart, pResult, pAppearance)
                pChart.Refresh()
                SuppressAnchorChartFurniture(pChart)
            Finally
                pIsDrawing = False
            End Try
        End Sub

        Private Sub Chart_Resize() Handles pChart.Resize
            If pChart Is Nothing OrElse pIsDrawing Then Exit Sub

            Try
                Draw()
            Catch
                'During workbook close/delete or very rapid resize, Excel can briefly
                'invalidate ChartArea/Shapes COM objects. A later resize or explicit
                'Redraw will recreate the full layer.
            End Try
        End Sub
    End Class
End Class
