Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Excel-specific display settings for <see cref="BulletChartExcel"/>.
''' Colors are Excel OLE RGB integers, consistent with the other BESHStatNG graphics classes.
''' </summary>
Public Class BulletChartAppearance
    ''' <summary>Custom chart title. Set to an empty string to omit the title.</summary>
    Public Property ChartTitle As String = "Bullet chart"

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property TextColor As Integer = &H404040

    ''' <summary>
    ''' Qualitative-range colors ordered from least desirable to most desirable.
    ''' When an item contains fewer colors than this palette, colors are sampled evenly across it.
    ''' The backend desirability rank automatically reverses the visual order for LowerIsBetter items.
    ''' </summary>
    Public Property BandColorsByDesirability As Integer() = {
        &H9C9C9C, &HB7B7B7, &HD0D0D0, &HE1E1E1, &HECECEC
    }

    Public Property ShowQualitativeBandOutlines As Boolean = False
    Public Property QualitativeBandOutlineColor As Integer = &HC0C0C0
    Public Property QualitativeBandOutlineWeight As Single = 0.5F

    ''' <summary>Preferred qualitative-band height in points. It is reduced automatically if required.</summary>
    Public Property QualitativeBandHeight As Double = 26.0R

    Public Property ActualBarColor As Integer = &H3F3F3F
    Public Property ShowActualBarOutline As Boolean = False
    Public Property ActualBarOutlineColor As Integer = &H303030
    Public Property ActualBarOutlineWeight As Single = 0.5F

    ''' <summary>Actual-bar height as a fraction of the qualitative-band height.</summary>
    Public Property ActualBarHeightFraction As Double = 0.36R

    Public Property TargetMarkerColor As Integer = &H3F3F3F
    Public Property TargetMarkerWeight As Single = 2.25F

    ''' <summary>Target-marker height as a fraction of the qualitative-band height.</summary>
    Public Property TargetMarkerHeightFraction As Double = 0.78R

    Public Property ShowMeasureLabels As Boolean = True
    Public Property ShowSubtitles As Boolean = True
    Public Property ShowScaleLabels As Boolean = True
    Public Property ShowScaleTickMarks As Boolean = False
    Public Property ShowActualValueLabels As Boolean = False
    Public Property ShowTargetValueLabels As Boolean = False

    Public Property FontName As String = "Calibri"
    Public Property MeasureLabelFontSize As Double = 10.0R
    Public Property SubtitleFontSize As Double = 8.0R
    Public Property ScaleLabelFontSize As Double = 8.0R
    Public Property ValueLabelFontSize As Double = 8.0R
    Public Property ChartTitleFontSize As Double = 12.0R

    ''' <summary>
    ''' Optional .NET numeric format for the per-item scales. Leave blank for an automatic format
    ''' derived from each item's resolved major interval.
    ''' </summary>
    Public Property ScaleNumberFormat As String = String.Empty

    ''' <summary>
    ''' Optional .NET numeric format for actual/target value labels. Leave blank to reuse each
    ''' item's automatic scale format.
    ''' </summary>
    Public Property ValueNumberFormat As String = String.Empty

    ''' <summary>Width reserved for the left-side measure/subtitle labels, in points.</summary>
    Public Property LabelAreaWidth As Double = 112.0R

    ''' <summary>Gap between the left labels and bullet plotting area, in points.</summary>
    Public Property LabelGap As Double = 10.0R

    ''' <summary>Outer drawing padding inside the embedded chart, in points.</summary>
    Public Property ChartPadding As Double = 12.0R

    ''' <summary>Height reserved for a non-empty custom chart title.</summary>
    Public Property ChartTitleHeight As Double = 24.0R

    ''' <summary>Vertical gap assigned to each bullet row, in points.</summary>
    Public Property RowGap As Double = 8.0R

    ''' <summary>Height reserved below each qualitative band for scale labels.</summary>
    Public Property ScaleLabelHeight As Double = 13.0R

    ''' <summary>Gap between a qualitative band and its scale labels/ticks.</summary>
    Public Property ScaleLabelGap As Double = 2.0R

    Public Property ScaleTickLength As Double = 3.0R
    Public Property ScaleTickWeight As Single = 0.5F

    ''' <summary>Width of optional actual/target value-label text boxes.</summary>
    Public Property ValueLabelWidth As Double = 54.0R

    ''' <summary>Height of optional actual/target value-label text boxes.</summary>
    Public Property ValueLabelHeight As Double = 12.0R

    Public Property ValueLabelGap As Double = 3.0R

    Friend Function Copy() As BulletChartAppearance
        Return New BulletChartAppearance With {
            .ChartTitle = ChartTitle,
            .BackgroundColor = BackgroundColor,
            .TextColor = TextColor,
            .BandColorsByDesirability = If(BandColorsByDesirability Is Nothing,
                                            Nothing,
                                            DirectCast(BandColorsByDesirability.Clone(), Integer())),
            .ShowQualitativeBandOutlines = ShowQualitativeBandOutlines,
            .QualitativeBandOutlineColor = QualitativeBandOutlineColor,
            .QualitativeBandOutlineWeight = QualitativeBandOutlineWeight,
            .QualitativeBandHeight = QualitativeBandHeight,
            .ActualBarColor = ActualBarColor,
            .ShowActualBarOutline = ShowActualBarOutline,
            .ActualBarOutlineColor = ActualBarOutlineColor,
            .ActualBarOutlineWeight = ActualBarOutlineWeight,
            .ActualBarHeightFraction = ActualBarHeightFraction,
            .TargetMarkerColor = TargetMarkerColor,
            .TargetMarkerWeight = TargetMarkerWeight,
            .TargetMarkerHeightFraction = TargetMarkerHeightFraction,
            .ShowMeasureLabels = ShowMeasureLabels,
            .ShowSubtitles = ShowSubtitles,
            .ShowScaleLabels = ShowScaleLabels,
            .ShowScaleTickMarks = ShowScaleTickMarks,
            .ShowActualValueLabels = ShowActualValueLabels,
            .ShowTargetValueLabels = ShowTargetValueLabels,
            .FontName = FontName,
            .MeasureLabelFontSize = MeasureLabelFontSize,
            .SubtitleFontSize = SubtitleFontSize,
            .ScaleLabelFontSize = ScaleLabelFontSize,
            .ValueLabelFontSize = ValueLabelFontSize,
            .ChartTitleFontSize = ChartTitleFontSize,
            .ScaleNumberFormat = ScaleNumberFormat,
            .ValueNumberFormat = ValueNumberFormat,
            .LabelAreaWidth = LabelAreaWidth,
            .LabelGap = LabelGap,
            .ChartPadding = ChartPadding,
            .ChartTitleHeight = ChartTitleHeight,
            .RowGap = RowGap,
            .ScaleLabelHeight = ScaleLabelHeight,
            .ScaleLabelGap = ScaleLabelGap,
            .ScaleTickLength = ScaleTickLength,
            .ScaleTickWeight = ScaleTickWeight,
            .ValueLabelWidth = ValueLabelWidth,
            .ValueLabelHeight = ValueLabelHeight,
            .ValueLabelGap = ValueLabelGap
        }
    End Function
End Class

''' <summary>
''' Excel renderer for the Excel-independent <see cref="BulletChartResult"/> backend.
'''
''' The embedded chart is an XY-scatter container with a fully invisible anchor series and axes.
''' Qualitative ranges, actual bars, target markers, item labels, and independent per-row scales are
''' native vector shapes in Chart.Shapes. A WithEvents helper redraws the shape layer after Chart.Resize,
''' preserving alignment when the chart is stretched or exported.
''' </summary>
''' <remarks>
''' Microsoft.Office.Core is intentionally not a compile-time project reference in BESHStatNG.
''' Chart-contained rectangle/line/text shapes and the Office enum values needed by this renderer are
''' therefore accessed through narrowly scoped late binding, following the existing Sankey and histogram
''' overlay implementations.
''' </remarks>
Public NotInheritable Class BulletChartExcel
    Private Sub New()
    End Sub

    Private Const ShapePrefix As String = "BESH_Bullet_"

    'Stable Office enum values used only through late binding.
    Private Const MsoShapeRectangle As Integer = 1
    Private Const MsoTextOrientationHorizontal As Integer = 1
    Private Const MsoFalse As Integer = 0
    Private Const MsoTrue As Integer = -1
    Private Const MsoAnchorMiddle As Integer = 3
    Private Const MsoAlignLeft As Integer = 1
    Private Const MsoAlignCenter As Integer = 2
    Private Const MsoAlignRight As Integer = 3

    Private Const MinimumPlotWidth As Double = 120.0R
    Private Const MinimumBandHeight As Double = 8.0R
    Private Const MinimumRowHeight As Double = 20.0R

    Private NotInheritable Class DrawingLayout
        Friend ChartWidth As Double
        Friend ChartHeight As Double
        Friend PlotLeft As Double
        Friend PlotTop As Double
        Friend PlotWidth As Double
        Friend PlotHeight As Double
        Friend RowHeight As Double
        Friend BandHeight As Double
        Friend LeftLabelsVisible As Boolean
    End Class

    Private NotInheritable Class RowLayout
        Friend RowTop As Double
        Friend RowBottom As Double
        Friend BandTop As Double
        Friend BandBottom As Double
        Friend BandCentreY As Double
        Friend ScaleLabelTop As Double
        Friend TargetValueLabelTop As Double
    End Class

    'Keep chart event sinks alive for as long as their embedded charts remain usable.
    'Stale handlers are pruned whenever another bullet chart is attached.
    Private Shared ReadOnly pOverlays As New List(Of BulletChartOverlay)()

    ''' <summary>
    ''' Adds an embedded bullet chart to <paramref name="ws"/>.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As BulletChartResult,
                                    Optional appearance As BulletChartAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 760.0R,
                                    Optional height As Double = 460.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.ItemCount < 1 Then
            Throw New ArgumentException("The bullet-chart result contains no items.", NameOf(result))
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

        Dim resolvedAppearance As BulletChartAppearance = If(appearance, New BulletChartAppearance()).Copy()
        ValidateAppearance(resolvedAppearance)
        ValidateResult(result)

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
    ''' Forces an immediate redraw for a chart previously created by <see cref="AddChart"/>.
    ''' Normally this is unnecessary because Chart.Resize is handled automatically.
    ''' </summary>
    Public Shared Sub Redraw(chart As Chart)
        If chart Is Nothing Then Throw New ArgumentNullException(NameOf(chart))

        PruneDeadOverlays()
        For Each overlay As BulletChartOverlay In pOverlays
            If overlay.OwnsChart(chart) Then
                overlay.Draw()
                Return
            End If
        Next

        Throw New ArgumentException("The chart is not registered as a BESHStatNG bullet chart.", NameOf(chart))
    End Sub

    Private Shared Sub ConfigureChartContainer(chart As Object,
                                               appearance As BulletChartAppearance)
        chart.ChartType = XlChartType.xlXYScatter
        chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
        chart.PlotVisibleOnly = False
        chart.ChartArea.AutoScaleFont = False

        Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
        DeleteAllSeries(seriesCollection)

        'The invisible anchor series prevents Excel's empty-chart placeholder while the user-visible
        'bullet chart remains entirely shape based.
        Dim anchorSeries As Object = seriesCollection.NewSeries()
        With anchorSeries
            .Name = " "
            .ChartType = XlChartType.xlXYScatter
            .XValues = New Double() {0.0R, 1.0R}
            .Values = New Double() {0.0R, 1.0R}
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
        End With

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
            'Some Excel versions do not expose all PlotArea formatting members until refresh.
        End Try

        chart.Refresh()
        SuppressAnchorChartFurniture(chart)
    End Sub

    Private Shared Sub SuppressAnchorChartFurniture(chart As Object)
        Try
            chart.HasLegend = False
        Catch
        End Try
        Try
            chart.HasTitle = False
        Catch
        End Try
        HideAnchorAxis(chart, XlAxisType.xlCategory)
        HideAnchorAxis(chart, XlAxisType.xlValue)
    End Sub

    Private Shared Sub HideAnchorAxis(chart As Object,
                                      axisType As XlAxisType)
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

            Try
                axis.MajorGridlines.Delete()
            Catch
            End Try
            Try
                axis.MinorGridlines.Delete()
            Catch
            End Try
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
                                     result As BulletChartResult,
                                     appearance As BulletChartAppearance)
        PruneDeadOverlays()

        Dim overlay As New BulletChartOverlay(chart, result, appearance)
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

    Private Shared Sub ValidateResult(result As BulletChartResult)
        For Each item As BulletChartItem In result.Items
            If item Is Nothing Then
                Throw New ArgumentException("The bullet-chart result contains a missing item.", NameOf(result))
            End If
            If Not IsFinitePositive(item.ScaleMaximum) Then
                Throw New ArgumentException("A bullet-chart item has an invalid scale maximum.", NameOf(result))
            End If
            If Not IsFinitePositive(item.MajorInterval) Then
                Throw New ArgumentException("A bullet-chart item has an invalid major interval.", NameOf(result))
            End If
            If Not IsFinite(item.ActualValue) OrElse item.ActualValue < 0.0R OrElse item.ActualValue > item.ScaleMaximum * (1.0R + 1.0E-10R) Then
                Throw New ArgumentException("A bullet-chart item has an actual value outside its prepared scale.", NameOf(result))
            End If
            If Not IsFinite(item.TargetValue) OrElse item.TargetValue < 0.0R OrElse item.TargetValue > item.ScaleMaximum * (1.0R + 1.0E-10R) Then
                Throw New ArgumentException("A bullet-chart item has a target value outside its prepared scale.", NameOf(result))
            End If
            If item.QualitativeRangeCount < BulletChart.MinimumQualitativeRanges OrElse
               item.QualitativeRangeCount > BulletChart.MaximumQualitativeRanges Then
                Throw New ArgumentException("A bullet-chart item has an unsupported qualitative-range count.", NameOf(result))
            End If
        Next
    End Sub

    Private Shared Sub ValidateAppearance(appearance As BulletChartAppearance)
        If appearance.BandColorsByDesirability Is Nothing OrElse appearance.BandColorsByDesirability.Length = 0 Then
            Throw New ArgumentException("BandColorsByDesirability must contain at least one color.", NameOf(appearance.BandColorsByDesirability))
        End If
        If String.IsNullOrWhiteSpace(appearance.FontName) Then
            Throw New ArgumentException("FontName cannot be blank.", NameOf(appearance.FontName))
        End If
        If Not IsFinitePositive(appearance.QualitativeBandOutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.QualitativeBandOutlineWeight))
        End If
        If Not IsFinitePositive(appearance.QualitativeBandHeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.QualitativeBandHeight))
        End If
        If Not IsFinitePositive(appearance.ActualBarOutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ActualBarOutlineWeight))
        End If
        If Not IsFiniteFractionExclusiveZero(appearance.ActualBarHeightFraction) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ActualBarHeightFraction),
                                                  "ActualBarHeightFraction must be greater than zero and at most one.")
        End If
        If Not IsFinitePositive(appearance.TargetMarkerWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.TargetMarkerWeight))
        End If
        If Not IsFiniteFractionExclusiveZero(appearance.TargetMarkerHeightFraction) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.TargetMarkerHeightFraction),
                                                  "TargetMarkerHeightFraction must be greater than zero and at most one.")
        End If
        ValidatePositiveFont(appearance.MeasureLabelFontSize, NameOf(appearance.MeasureLabelFontSize))
        ValidatePositiveFont(appearance.SubtitleFontSize, NameOf(appearance.SubtitleFontSize))
        ValidatePositiveFont(appearance.ScaleLabelFontSize, NameOf(appearance.ScaleLabelFontSize))
        ValidatePositiveFont(appearance.ValueLabelFontSize, NameOf(appearance.ValueLabelFontSize))
        ValidatePositiveFont(appearance.ChartTitleFontSize, NameOf(appearance.ChartTitleFontSize))

        If Not IsFinitePositive(appearance.LabelAreaWidth) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LabelAreaWidth))
        End If
        ValidateNonNegative(appearance.LabelGap, NameOf(appearance.LabelGap))
        ValidateNonNegative(appearance.ChartPadding, NameOf(appearance.ChartPadding))
        ValidateNonNegative(appearance.ChartTitleHeight, NameOf(appearance.ChartTitleHeight))
        ValidateNonNegative(appearance.RowGap, NameOf(appearance.RowGap))
        If Not IsFinitePositive(appearance.ScaleLabelHeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ScaleLabelHeight))
        End If
        ValidateNonNegative(appearance.ScaleLabelGap, NameOf(appearance.ScaleLabelGap))
        ValidateNonNegative(appearance.ScaleTickLength, NameOf(appearance.ScaleTickLength))
        If Not IsFinitePositive(appearance.ScaleTickWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ScaleTickWeight))
        End If
        If Not IsFinitePositive(appearance.ValueLabelWidth) OrElse Not IsFinitePositive(appearance.ValueLabelHeight) Then
            Throw New ArgumentOutOfRangeException("Value label dimensions", "Value-label dimensions must be finite and positive.")
        End If
        ValidateNonNegative(appearance.ValueLabelGap, NameOf(appearance.ValueLabelGap))

        ValidateOptionalNumberFormat(appearance.ScaleNumberFormat, NameOf(appearance.ScaleNumberFormat), 12.5R)
        ValidateOptionalNumberFormat(appearance.ValueNumberFormat, NameOf(appearance.ValueNumberFormat), 12.5R)
    End Sub

    Private Shared Sub ValidatePositiveFont(value As Double,
                                            parameterName As String)
        If Not IsFinitePositive(value) Then
            Throw New ArgumentOutOfRangeException(parameterName, "Font size must be finite and positive.")
        End If
    End Sub

    Private Shared Sub ValidateNonNegative(value As Double,
                                           parameterName As String)
        If Not IsFinite(value) OrElse value < 0.0R Then
            Throw New ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.")
        End If
    End Sub

    Private Shared Sub ValidateOptionalNumberFormat(format As String,
                                                    parameterName As String,
                                                    sampleValue As Double)
        If String.IsNullOrWhiteSpace(format) Then Return
        Try
            Dim unused As String = sampleValue.ToString(format, CultureInfo.CurrentCulture)
        Catch ex As FormatException
            Throw New ArgumentException(parameterName & " is not a valid .NET numeric format string.", parameterName, ex)
        End Try
    End Sub

    Private Shared Function BuildDrawingLayout(chart As Chart,
                                               result As BulletChartResult,
                                               appearance As BulletChartAppearance) As DrawingLayout
        Dim chartWidth As Double
        Dim chartHeight As Double
        GetChartSize(chart, chartWidth, chartHeight)

        Dim padding As Double = appearance.ChartPadding
        Dim titleHeight As Double = If(String.IsNullOrWhiteSpace(appearance.ChartTitle), 0.0R, appearance.ChartTitleHeight)
        Dim leftLabelsVisible As Boolean = appearance.ShowMeasureLabels OrElse
                                           (appearance.ShowSubtitles AndAlso result.HasSubtitles)
        Dim labelReserve As Double = If(leftLabelsVisible,
                                        appearance.LabelAreaWidth + appearance.LabelGap,
                                        0.0R)

        Dim plotLeft As Double = padding + labelReserve
        Dim plotTop As Double = padding + titleHeight
        Dim plotWidth As Double = chartWidth - padding - plotLeft
        Dim plotHeight As Double = chartHeight - padding - plotTop

        If plotWidth < MinimumPlotWidth Then
            Throw New InvalidOperationException("The embedded chart is too narrow to draw the bullet chart. Increase its width or reduce the label area.")
        End If
        If plotHeight <= 0.0R Then
            Throw New InvalidOperationException("The embedded chart is too short to draw the bullet chart. Increase its height.")
        End If

        Dim rowHeight As Double = plotHeight / CDbl(result.ItemCount)
        If rowHeight < MinimumRowHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the number of bullet-chart items. Increase its height.")
        End If

        Dim scaleReserve As Double = 0.0R
        If appearance.ShowScaleLabels OrElse appearance.ShowScaleTickMarks Then
            scaleReserve = appearance.ScaleLabelGap
            If appearance.ShowScaleTickMarks Then scaleReserve += appearance.ScaleTickLength
            If appearance.ShowScaleLabels Then scaleReserve += appearance.ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(appearance.ShowTargetValueLabels,
                                              appearance.ValueLabelHeight + appearance.ValueLabelGap,
                                              0.0R)

        Dim bandAvailable As Double = rowHeight - appearance.RowGap - scaleReserve - targetLabelReserve
        If bandAvailable < MinimumBandHeight Then
            Throw New InvalidOperationException("The embedded chart is too short for the selected bullet-chart labels and spacing. Increase its height or hide value/scale labels.")
        End If

        Return New DrawingLayout With {
            .ChartWidth = chartWidth,
            .ChartHeight = chartHeight,
            .PlotLeft = plotLeft,
            .PlotTop = plotTop,
            .PlotWidth = plotWidth,
            .PlotHeight = plotHeight,
            .RowHeight = rowHeight,
            .BandHeight = Math.Min(appearance.QualitativeBandHeight, bandAvailable),
            .LeftLabelsVisible = leftLabelsVisible
        }
    End Function

    Private Shared Function BuildRowLayout(layout As DrawingLayout,
                                           appearance As BulletChartAppearance,
                                           rowIndex As Integer) As RowLayout
        Dim rowTop As Double = layout.PlotTop + CDbl(rowIndex) * layout.RowHeight
        Dim rowBottom As Double = rowTop + layout.RowHeight

        Dim scaleReserve As Double = 0.0R
        If appearance.ShowScaleLabels OrElse appearance.ShowScaleTickMarks Then
            scaleReserve = appearance.ScaleLabelGap
            If appearance.ShowScaleTickMarks Then scaleReserve += appearance.ScaleTickLength
            If appearance.ShowScaleLabels Then scaleReserve += appearance.ScaleLabelHeight
        End If
        Dim targetLabelReserve As Double = If(appearance.ShowTargetValueLabels,
                                              appearance.ValueLabelHeight + appearance.ValueLabelGap,
                                              0.0R)
        Dim contentHeight As Double = targetLabelReserve + layout.BandHeight + scaleReserve
        Dim contentTop As Double = rowTop + (layout.RowHeight - contentHeight) / 2.0R

        Dim targetValueTop As Double = contentTop
        Dim bandTop As Double = contentTop + targetLabelReserve
        Dim bandBottom As Double = bandTop + layout.BandHeight
        Dim scaleTop As Double = bandBottom + appearance.ScaleLabelGap +
                                 If(appearance.ShowScaleTickMarks, appearance.ScaleTickLength, 0.0R)

        Return New RowLayout With {
            .RowTop = rowTop,
            .RowBottom = rowBottom,
            .BandTop = bandTop,
            .BandBottom = bandBottom,
            .BandCentreY = (bandTop + bandBottom) / 2.0R,
            .ScaleLabelTop = scaleTop,
            .TargetValueLabelTop = targetValueTop
        }
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
            Throw New InvalidOperationException("Excel returned an invalid chart size while drawing the bullet chart.")
        End If
    End Sub

    Private Shared Sub DrawBulletChart(chart As Chart,
                                       result As BulletChartResult,
                                       appearance As BulletChartAppearance)
        Dim layout As DrawingLayout = BuildDrawingLayout(chart, result, appearance)
        Dim chartObject As Object = chart
        Dim chartShapes As Object = chartObject.Shapes

        DrawTitle(chartShapes, appearance, layout)

        Dim items As BulletChartItem() = result.Items
        For rowIndex As Integer = 0 To items.Length - 1
            Dim item As BulletChartItem = items(rowIndex)
            Dim rowLayout As RowLayout = BuildRowLayout(layout, appearance, rowIndex)

            DrawQualitativeBands(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            DrawActualBar(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            DrawTargetMarker(chartShapes, item, appearance, layout, rowLayout, rowIndex)

            If layout.LeftLabelsVisible Then
                DrawLeftLabels(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            End If

            If appearance.ShowScaleTickMarks OrElse appearance.ShowScaleLabels Then
                DrawScale(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            End If

            If appearance.ShowActualValueLabels Then
                DrawActualValueLabel(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            End If
            If appearance.ShowTargetValueLabels Then
                DrawTargetValueLabel(chartShapes, item, appearance, layout, rowLayout, rowIndex)
            End If
        Next
    End Sub

    Private Shared Sub DrawTitle(chartShapes As Object,
                                 appearance As BulletChartAppearance,
                                 layout As DrawingLayout)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) Then Return

        AddTextShape(chartShapes,
                     ShapePrefix & "Title",
                     appearance.ChartTitle.Trim(),
                     appearance.ChartPadding,
                     appearance.ChartPadding,
                     Math.Max(1.0R, layout.ChartWidth - 2.0R * appearance.ChartPadding),
                     appearance.ChartTitleHeight,
                     appearance.TextColor,
                     appearance.ChartTitleFontSize,
                     True,
                     MsoAlignCenter,
                     appearance.FontName,
                     False)
    End Sub

    Private Shared Sub DrawQualitativeBands(chartShapes As Object,
                                            item As BulletChartItem,
                                            appearance As BulletChartAppearance,
                                            layout As DrawingLayout,
                                            rowLayout As RowLayout,
                                            rowIndex As Integer)
        Dim bands As BulletChartBand() = item.Bands

        For bandIndex As Integer = 0 To bands.Length - 1
            Dim band As BulletChartBand = bands(bandIndex)
            Dim x1 As Double = NormalizedToChartX(band.NormalizedStart, layout)
            Dim x2 As Double = NormalizedToChartX(band.NormalizedEnd, layout)
            Dim bandWidth As Double = Math.Max(0.5R, x2 - x1)

            Dim bandShape As Object = chartShapes.AddShape(MsoShapeRectangle,
                                                           CSng(x1),
                                                           CSng(rowLayout.BandTop),
                                                           CSng(bandWidth),
                                                           CSng(layout.BandHeight))
            bandShape.Name = ShapePrefix & "Band_" &
                             (rowIndex + 1).ToString(CultureInfo.InvariantCulture) & "_" &
                             (bandIndex + 1).ToString(CultureInfo.InvariantCulture)

            With bandShape.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = ResolveBandColor(band, item, appearance)
                .Transparency = 0.0F
            End With

            With bandShape.Line
                If appearance.ShowQualitativeBandOutlines Then
                    .Visible = True
                    .ForeColor.RGB = appearance.QualitativeBandOutlineColor
                    .Transparency = 0.0F
                    .Weight = appearance.QualitativeBandOutlineWeight
                Else
                    .Visible = False
                End If
            End With
        Next
    End Sub

    Private Shared Function ResolveBandColor(band As BulletChartBand,
                                             item As BulletChartItem,
                                             appearance As BulletChartAppearance) As Integer
        Dim palette As Integer() = appearance.BandColorsByDesirability
        If palette.Length = 1 OrElse item.QualitativeRangeCount <= 1 Then Return palette(0)

        Dim fraction As Double = CDbl(band.DesirabilityRank) /
                                 CDbl(item.QualitativeRangeCount - 1)
        Dim paletteIndex As Integer = CInt(Math.Round(fraction * CDbl(palette.Length - 1),
                                                      MidpointRounding.AwayFromZero))
        paletteIndex = Math.Max(0, Math.Min(palette.Length - 1, paletteIndex))
        Return palette(paletteIndex)
    End Function

    Private Shared Sub DrawActualBar(chartShapes As Object,
                                     item As BulletChartItem,
                                     appearance As BulletChartAppearance,
                                     layout As DrawingLayout,
                                     rowLayout As RowLayout,
                                     rowIndex As Integer)
        Dim normalized As Double = Clip01(item.ActualNormalized)
        If normalized <= 0.0R Then Return

        Dim barHeight As Double = Math.Max(1.0R, layout.BandHeight * appearance.ActualBarHeightFraction)
        Dim barTop As Double = rowLayout.BandCentreY - barHeight / 2.0R
        Dim barWidth As Double = Math.Max(0.5R, normalized * layout.PlotWidth)

        Dim barShape As Object = chartShapes.AddShape(MsoShapeRectangle,
                                                      CSng(layout.PlotLeft),
                                                      CSng(barTop),
                                                      CSng(barWidth),
                                                      CSng(barHeight))
        barShape.Name = ShapePrefix & "Actual_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture)

        With barShape.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = appearance.ActualBarColor
            .Transparency = 0.0F
        End With
        With barShape.Line
            If appearance.ShowActualBarOutline Then
                .Visible = True
                .ForeColor.RGB = appearance.ActualBarOutlineColor
                .Transparency = 0.0F
                .Weight = appearance.ActualBarOutlineWeight
            Else
                .Visible = False
            End If
        End With
    End Sub

    Private Shared Sub DrawTargetMarker(chartShapes As Object,
                                        item As BulletChartItem,
                                        appearance As BulletChartAppearance,
                                        layout As DrawingLayout,
                                        rowLayout As RowLayout,
                                        rowIndex As Integer)
        Dim x As Double = NormalizedToChartX(item.TargetNormalized, layout)
        Dim markerHeight As Double = Math.Max(2.0R,
                                             layout.BandHeight * appearance.TargetMarkerHeightFraction)
        Dim y1 As Double = rowLayout.BandCentreY - markerHeight / 2.0R
        Dim y2 As Double = rowLayout.BandCentreY + markerHeight / 2.0R

        Dim marker As Object = chartShapes.AddLine(CSng(x), CSng(y1), CSng(x), CSng(y2))
        marker.Name = ShapePrefix & "Target_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture)
        With marker.Line
            .Visible = True
            .ForeColor.RGB = appearance.TargetMarkerColor
            .Transparency = 0.0F
            .Weight = appearance.TargetMarkerWeight
        End With
    End Sub

    Private Shared Sub DrawLeftLabels(chartShapes As Object,
                                      item As BulletChartItem,
                                      appearance As BulletChartAppearance,
                                      layout As DrawingLayout,
                                      rowLayout As RowLayout,
                                      rowIndex As Integer)
        Dim left As Double = appearance.ChartPadding
        Dim width As Double = appearance.LabelAreaWidth
        Dim showMain As Boolean = appearance.ShowMeasureLabels
        Dim showSubtitle As Boolean = appearance.ShowSubtitles AndAlso item.HasSubtitle

        If Not showMain AndAlso Not showSubtitle Then Return

        Dim labelRight As Double = layout.PlotLeft - appearance.LabelGap
        width = Math.Max(1.0R, labelRight - left)

        If showMain AndAlso showSubtitle Then
            Dim mainHeight As Double = Math.Max(12.0R, appearance.MeasureLabelFontSize * 1.35R)
            Dim subtitleHeight As Double = Math.Max(10.0R, appearance.SubtitleFontSize * 1.35R)
            Dim totalHeight As Double = mainHeight + subtitleHeight
            Dim top As Double = rowLayout.BandCentreY - totalHeight / 2.0R

            AddTextShape(chartShapes,
                         ShapePrefix & "Measure_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture),
                         item.MeasureLabel,
                         left,
                         top,
                         width,
                         mainHeight,
                         appearance.TextColor,
                         appearance.MeasureLabelFontSize,
                         False,
                         MsoAlignRight,
                         appearance.FontName,
                         True)

            AddTextShape(chartShapes,
                         ShapePrefix & "Subtitle_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture),
                         item.Subtitle,
                         left,
                         top + mainHeight,
                         width,
                         subtitleHeight,
                         appearance.TextColor,
                         appearance.SubtitleFontSize,
                         False,
                         MsoAlignRight,
                         appearance.FontName,
                         True)
        Else
            Dim fontSize As Double = If(showMain,
                                        appearance.MeasureLabelFontSize,
                                        appearance.SubtitleFontSize)
            Dim text As String = If(showMain, item.MeasureLabel, item.Subtitle)
            Dim boxHeight As Double = Math.Max(12.0R, fontSize * 1.45R)
            Dim top As Double = rowLayout.BandCentreY - boxHeight / 2.0R

            AddTextShape(chartShapes,
                         ShapePrefix & If(showMain, "Measure_", "Subtitle_") &
                         (rowIndex + 1).ToString(CultureInfo.InvariantCulture),
                         text,
                         left,
                         top,
                         width,
                         boxHeight,
                         appearance.TextColor,
                         fontSize,
                         False,
                         MsoAlignRight,
                         appearance.FontName,
                         True)
        End If
    End Sub

    Private Shared Sub DrawScale(chartShapes As Object,
                                 item As BulletChartItem,
                                 appearance As BulletChartAppearance,
                                 layout As DrawingLayout,
                                 rowLayout As RowLayout,
                                 rowIndex As Integer)
        Dim ticks As Double() = item.ScaleTicks
        If ticks Is Nothing OrElse ticks.Length = 0 Then Return

        Dim numberFormat As String = ResolveNumberFormat(item, appearance.ScaleNumberFormat)
        Dim nominalWidth As Double
        If ticks.Length <= 1 Then
            nominalWidth = 48.0R
        Else
            nominalWidth = Math.Max(24.0R,
                                    Math.Min(64.0R,
                                             layout.PlotWidth / CDbl(ticks.Length - 1) * 0.92R))
        End If

        For tickIndex As Integer = 0 To ticks.Length - 1
            Dim tickValue As Double = ticks(tickIndex)
            Dim normalized As Double = If(item.ScaleMaximum <= 0.0R,
                                          0.0R,
                                          tickValue / item.ScaleMaximum)
            Dim x As Double = NormalizedToChartX(normalized, layout)

            If appearance.ShowScaleTickMarks AndAlso appearance.ScaleTickLength > 0.0R Then
                Dim tickShape As Object = chartShapes.AddLine(CSng(x),
                                                              CSng(rowLayout.BandBottom),
                                                              CSng(x),
                                                              CSng(rowLayout.BandBottom + appearance.ScaleTickLength))
                tickShape.Name = ShapePrefix & "ScaleTick_" &
                                 (rowIndex + 1).ToString(CultureInfo.InvariantCulture) & "_" &
                                 (tickIndex + 1).ToString(CultureInfo.InvariantCulture)
                With tickShape.Line
                    .Visible = True
                    .ForeColor.RGB = appearance.TextColor
                    .Transparency = 0.0F
                    .Weight = appearance.ScaleTickWeight
                End With
            End If

            If appearance.ShowScaleLabels Then
                Dim textLeft As Double
                Dim alignment As Integer

                If tickIndex = 0 Then
                    textLeft = layout.PlotLeft
                    alignment = MsoAlignLeft
                ElseIf tickIndex = ticks.Length - 1 Then
                    textLeft = layout.PlotLeft + layout.PlotWidth - nominalWidth
                    alignment = MsoAlignRight
                Else
                    textLeft = x - nominalWidth / 2.0R
                    alignment = MsoAlignCenter
                End If

                textLeft = Math.Max(layout.PlotLeft,
                                    Math.Min(layout.PlotLeft + layout.PlotWidth - nominalWidth, textLeft))

                AddTextShape(chartShapes,
                             ShapePrefix & "ScaleLabel_" &
                             (rowIndex + 1).ToString(CultureInfo.InvariantCulture) & "_" &
                             (tickIndex + 1).ToString(CultureInfo.InvariantCulture),
                             FormatNumeric(tickValue, numberFormat),
                             textLeft,
                             rowLayout.ScaleLabelTop,
                             nominalWidth,
                             appearance.ScaleLabelHeight,
                             appearance.TextColor,
                             appearance.ScaleLabelFontSize,
                             False,
                             alignment,
                             appearance.FontName,
                             False)
            End If
        Next
    End Sub

    Private Shared Sub DrawActualValueLabel(chartShapes As Object,
                                            item As BulletChartItem,
                                            appearance As BulletChartAppearance,
                                            layout As DrawingLayout,
                                            rowLayout As RowLayout,
                                            rowIndex As Integer)
        Dim actualX As Double = NormalizedToChartX(item.ActualNormalized, layout)
        Dim labelWidth As Double = Math.Min(appearance.ValueLabelWidth, layout.PlotWidth)
        Dim left As Double
        Dim alignment As Integer

        If actualX + appearance.ValueLabelGap + labelWidth <= layout.PlotLeft + layout.PlotWidth Then
            left = actualX + appearance.ValueLabelGap
            alignment = MsoAlignLeft
        Else
            left = Math.Max(layout.PlotLeft, actualX - appearance.ValueLabelGap - labelWidth)
            alignment = MsoAlignRight
        End If

        Dim numberFormat As String = ResolveNumberFormat(item, appearance.ValueNumberFormat, item.ActualValue)
        AddTextShape(chartShapes,
                     ShapePrefix & "ActualValue_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture),
                     FormatNumeric(item.ActualValue, numberFormat),
                     left,
                     rowLayout.BandCentreY - appearance.ValueLabelHeight / 2.0R,
                     labelWidth,
                     appearance.ValueLabelHeight,
                     appearance.TextColor,
                     appearance.ValueLabelFontSize,
                     False,
                     alignment,
                     appearance.FontName,
                     False)
    End Sub

    Private Shared Sub DrawTargetValueLabel(chartShapes As Object,
                                            item As BulletChartItem,
                                            appearance As BulletChartAppearance,
                                            layout As DrawingLayout,
                                            rowLayout As RowLayout,
                                            rowIndex As Integer)
        Dim targetX As Double = NormalizedToChartX(item.TargetNormalized, layout)
        Dim labelWidth As Double = Math.Min(appearance.ValueLabelWidth, layout.PlotWidth)
        Dim left As Double = targetX - labelWidth / 2.0R
        left = Math.Max(layout.PlotLeft,
                        Math.Min(layout.PlotLeft + layout.PlotWidth - labelWidth, left))

        Dim numberFormat As String = ResolveNumberFormat(item, appearance.ValueNumberFormat, item.TargetValue)
        AddTextShape(chartShapes,
                     ShapePrefix & "TargetValue_" & (rowIndex + 1).ToString(CultureInfo.InvariantCulture),
                     FormatNumeric(item.TargetValue, numberFormat),
                     left,
                     rowLayout.TargetValueLabelTop,
                     labelWidth,
                     appearance.ValueLabelHeight,
                     appearance.TargetMarkerColor,
                     appearance.ValueLabelFontSize,
                     False,
                     MsoAlignCenter,
                     appearance.FontName,
                     False)
    End Sub

    Private Shared Function NormalizedToChartX(value As Double,
                                               layout As DrawingLayout) As Double
        Return layout.PlotLeft + Clip01(value) * layout.PlotWidth
    End Function

    Private Shared Function Clip01(value As Double) As Double
        If value < 0.0R Then Return 0.0R
        If value > 1.0R Then Return 1.0R
        Return value
    End Function

    Private Shared Function ResolveNumberFormat(item As BulletChartItem,
                                                requestedFormat As String,
                                                Optional additionalValue As Nullable(Of Double) = Nothing) As String
        If Not String.IsNullOrWhiteSpace(requestedFormat) Then Return requestedFormat.Trim()
        Return BuildAutomaticNumberFormat(item.MajorInterval, item.ScaleMaximum, additionalValue)
    End Function

    Private Shared Function BuildAutomaticNumberFormat(stepValue As Double,
                                                       scaleMaximum As Double,
                                                       additionalValue As Nullable(Of Double)) As String
        Dim values As New List(Of Double) From {Math.Abs(stepValue), Math.Abs(scaleMaximum)}
        If additionalValue.HasValue Then values.Add(Math.Abs(additionalValue.Value))

        For Each value As Double In values
            If Not IsFinite(value) Then Return "0.###"
            If value > 0.0R AndAlso (value >= 1.0E8R OrElse value < 1.0E-6R) Then
                Return "0.###E+0"
            End If
        Next

        Dim requiredDecimals As Integer = 0
        For Each value As Double In values
            requiredDecimals = Math.Max(requiredDecimals, RequiredDecimalPlaces(value))
        Next

        If requiredDecimals <= 0 Then Return "0"
        If requiredDecimals <= 8 Then Return "0." & New String("0"c, requiredDecimals)
        Return "0.########"
    End Function

    Private Shared Function RequiredDecimalPlaces(value As Double) As Integer
        If value = 0.0R Then Return 0

        For decimals As Integer = 0 To 8
            Dim scale As Double = Math.Pow(10.0R, decimals)
            Dim scaled As Double = value * scale
            If Math.Abs(scaled - Math.Round(scaled)) <= 1.0E-9R * Math.Max(1.0R, Math.Abs(scaled)) Then
                Return decimals
            End If
        Next
        Return 8
    End Function

    Private Shared Function FormatNumeric(value As Double,
                                          format As String) As String
        Dim cleanValue As Double = value
        If Math.Abs(cleanValue) < 1.0E-14R Then cleanValue = 0.0R
        Return cleanValue.ToString(format, CultureInfo.CurrentCulture)
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
                                         alignment As Integer,
                                         fontName As String,
                                         wordWrap As Boolean) As Object
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
            .WordWrap = If(wordWrap, MsoTrue, MsoFalse)
            .VerticalAnchor = MsoAnchorMiddle
            .TextRange.Text = If(text, String.Empty)
            .TextRange.ParagraphFormat.Alignment = alignment
            With .TextRange.Font
                .Name = fontName
                .Size = CSng(fontSize)
                .Bold = If(bold, MsoTrue, MsoFalse)
                .Fill.Visible = True
                .Fill.Solid()
                .Fill.ForeColor.RGB = color
            End With
        End With

        Return textShape
    End Function

    Private Shared Sub DeleteBulletShapes(chart As Chart)
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
                'A shape can disappear while Excel is processing a resize. Continue with the
                'remaining BESHStatNG shapes and recreate the complete layer.
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

            'COM can return a new RCW for the same ChartObject/Worksheet, so compare the
            'containing sheet/workbook identities rather than relying only on reference equality.
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

    Private Shared Function IsFiniteFractionExclusiveZero(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R AndAlso value <= 1.0R
    End Function

    ''' <summary>
    ''' Owns the chart-contained bullet layer and reprojects the prepared normalized geometry
    ''' whenever the embedded chart is resized.
    ''' </summary>
    Private NotInheritable Class BulletChartOverlay
        Private WithEvents pChart As Chart
        Private ReadOnly pResult As BulletChartResult
        Private ReadOnly pAppearance As BulletChartAppearance
        Private pIsDrawing As Boolean

        Friend Sub New(chart As Chart,
                       result As BulletChartResult,
                       appearance As BulletChartAppearance)
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
                DeleteBulletShapes(pChart)
                DrawBulletChart(pChart, pResult, pAppearance)
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
                'Workbook close/delete and rapid chart resizing can briefly invalidate ChartArea
                'or Shapes COM objects. A later resize or explicit Redraw will recreate the layer.
            End Try
        End Sub
    End Class
End Class
