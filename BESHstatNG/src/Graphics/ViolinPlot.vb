Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Windows compatibility entry point for the portable violin KDE and plot geometry.
''' Numerical contracts and calculations are owned by BESHStat.Core.
''' </summary>
Public NotInheritable Class ViolinPlot
    Private Sub New()
    End Sub

    Public Shared Function Compute(values As Double(),
                                   groups As Array,
                                   Optional options As ViolinPlotOptions = Nothing) As ViolinPlotResult
        Return ViolinPlotCore.Compute(values, groups, options)
    End Function
End Class

''' <summary>
''' Optional appearance override for one categorical level in a violin chart.
''' </summary>
Public Class ViolinSeriesAppearance
    Public Property SeriesName As String
    Public Property FillColor As Nullable(Of Integer)
    Public Property FillTransparency As Nullable(Of Single)
    Public Property OutlineColor As Nullable(Of Integer)
    Public Property OutlineWeight As Nullable(Of Single)
    Public Property PointColor As Nullable(Of Integer)
End Class

''' <summary>
''' Excel-specific display settings for <see cref="ViolinPlotExcel"/>.
''' </summary>
Public Class ViolinPlotAppearance
    Public Property ChartTitle As String = "Violin plot"
    Public Property XAxisTitle As String = String.Empty
    Public Property YAxisTitle As String = String.Empty
    Public Property ShowHorizontalGridlines As Boolean = True

    ''' <summary>Chart and plot-area background color as an Excel OLE RGB integer.</summary>
    Public Property BackgroundColor As Integer = &HFFFFFF

    ''' <summary>
    ''' Tableau-10-style palette already used by newer BESHStatNG graphics classes.
    ''' Colors are Excel OLE RGB integers.
    ''' </summary>
    Public Property SeriesColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    ''' <summary>Default violin fill transparency, from 0 (opaque) to 1 (transparent).</summary>
    Public Property FillTransparency As Single = 0.2F

    Public Property ShowOutline As Boolean = True
    Public Property OutlineColor As Nullable(Of Integer) = Nothing
    Public Property OutlineWeight As Single = 0.75F

    ''' <summary>Draws a narrow Q1-Q3 box and Tukey whiskers inside each violin.</summary>
    Public Property ShowInnerBox As Boolean = True

    ''' <summary>Draws a horizontal median line. Default: True.</summary>
    Public Property ShowMedian As Boolean = True

    ''' <summary>Draws a small cross at the group mean. Default: False.</summary>
    Public Property ShowMean As Boolean = False

    ''' <summary>Plots the original observations with deterministic horizontal jitter.</summary>
    Public Property ShowIndividualObservations As Boolean = False

    ''' <summary>Half-width of the inner Q1-Q3 box in category-coordinate units.</summary>
    Public Property InnerBoxHalfWidth As Double = 0.055R

    ''' <summary>Half-width of Tukey whisker end caps in category-coordinate units.</summary>
    Public Property WhiskerCapHalfWidth As Double = 0.035R

    Public Property InnerBoxFillColor As Integer = &HFFFFFF
    Public Property InnerBoxFillTransparency As Single = 0.1F
    Public Property InnerLineColor As Integer = &H202020
    Public Property InnerLineWeight As Single = 1.0F

    ''' <summary>Marker size for optional raw observations.</summary>
    Public Property ObservationMarkerSize As Integer = 4

    ''' <summary>
    ''' Maximum absolute horizontal jitter for individual observations, in category
    ''' coordinate units.
    ''' </summary>
    Public Property ObservationJitterHalfWidth As Double = 0.12R

    ''' <summary>Color override for raw observations; Nothing uses the violin color.</summary>
    Public Property ObservationColor As Nullable(Of Integer) = Nothing

    ''' <summary>Point size used for category labels implemented as XY data labels.</summary>
    Public Property CategoryLabelFontSize As Double = 9.0R

    ''' <summary>Size in points of each arm of the optional mean cross.</summary>
    Public Property MeanMarkerHalfSize As Single = 3.0F

    Public Property YAxisMinimum As Nullable(Of Double) = Nothing
    Public Property YAxisMaximum As Nullable(Of Double) = Nothing
    Public Property YAxisMajorUnit As Nullable(Of Double) = Nothing

    Public Property SeriesOverrides As ViolinSeriesAppearance() = New ViolinSeriesAppearance() {}
End Class

''' <summary>
''' Renders a <see cref="ViolinPlotResult"/> as an embedded Excel XY-scatter chart.
''' The Cartesian chart supplies axes/category labels; violins, inner summaries, and
''' optional raw observations are chart-contained shapes positioned using PlotArea
''' coordinates so the custom geometry resizes as one visual layer.
''' </summary>
''' <remarks>
''' Microsoft.Office.Core is intentionally not a compile-time project reference in
''' BESHStatNG. FreeformBuilder and the few Office enum values required by the drawing
''' layer are therefore invoked through narrowly scoped late binding, following the
''' same compatibility approach used by other BESHStatNG graphics classes.
''' </remarks>
Public NotInheritable Class ViolinPlotExcel
    Private Sub New()
    End Sub

    'Stable numeric Office enum values used only through late binding.
    Private Const MsoEditingAuto As Integer = 0
    Private Const MsoEditingCorner As Integer = 1
    Private Const MsoSegmentLine As Integer = 0
    Private Const MsoSendToBack As Integer = 1
    Private Const MsoShapeRectangle As Integer = 1
    Private Const MsoShapeOval As Integer = 9

    Private NotInheritable Class ResolvedSeriesStyle
        Friend FillColor As Integer
        Friend FillTransparency As Single
        Friend OutlineColor As Integer
        Friend OutlineWeight As Single
        Friend PointColor As Integer
    End Class

    Private NotInheritable Class AxisContext
        Friend XMinimum As Double
        Friend XMaximum As Double
        Friend YMinimum As Double
        Friend YMaximum As Double
    End Class

    ''' <summary>
    ''' Adds a categorical violin chart to <paramref name="ws"/>.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As ViolinPlotResult,
                                    Optional appearance As ViolinPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 720.0R,
                                    Optional height As Double = 440.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.GroupCount < 1 Then Throw New ArgumentException("The violin result contains no groups.", NameOf(result))
        If result.GroupCount > 255 Then
            Throw New ArgumentException("Excel charts support at most 255 data series for this chart.", NameOf(result))
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

        Dim resolvedAppearance As ViolinPlotAppearance = If(appearance, New ViolinPlotAppearance())
        ValidateAppearance(resolvedAppearance, result)
        Dim axisContext As AxisContext = ResolveAxisContext(result, resolvedAppearance)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False
            chart.HasLegend = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance.ChartTitle)
            ConfigureAxes(chart, axisContext, resolvedAppearance)

            'An invisible anchor series both keeps the scatter axes alive and supplies
            'categorical labels through per-point data labels.
            AddCategoryLabelSeries(seriesCollection, result.Series, axisContext, resolvedAppearance)
            chart.Refresh()

            DrawViolins(chart, result, axisContext, resolvedAppearance)

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

    Private Shared Sub ValidateAppearance(appearance As ViolinPlotAppearance,
                                          result As ViolinPlotResult)
        If appearance.SeriesColors Is Nothing OrElse appearance.SeriesColors.Length = 0 Then
            Throw New ArgumentException("SeriesColors must contain at least one color.", NameOf(appearance.SeriesColors))
        End If
        If Not IsFiniteFraction(appearance.FillTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.FillTransparency),
                                                  "FillTransparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.OutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.OutlineWeight),
                                                  "OutlineWeight must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.InnerLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.InnerLineWeight),
                                                  "InnerLineWeight must be finite and positive.")
        End If
        If Not IsFiniteFraction(appearance.InnerBoxFillTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.InnerBoxFillTransparency),
                                                  "InnerBoxFillTransparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.InnerBoxHalfWidth) OrElse appearance.InnerBoxHalfWidth >= 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.InnerBoxHalfWidth),
                                                  "InnerBoxHalfWidth must be finite, positive, and smaller than 0.5 category units.")
        End If
        If Not IsFinitePositive(appearance.WhiskerCapHalfWidth) OrElse appearance.WhiskerCapHalfWidth >= 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.WhiskerCapHalfWidth),
                                                  "WhiskerCapHalfWidth must be finite, positive, and smaller than 0.5 category units.")
        End If
        If appearance.ObservationMarkerSize < 2 OrElse appearance.ObservationMarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ObservationMarkerSize),
                                                  "ObservationMarkerSize must be between 2 and 72 points.")
        End If
        If appearance.ObservationJitterHalfWidth < 0.0R OrElse
           Not IsFinite(appearance.ObservationJitterHalfWidth) OrElse
           appearance.ObservationJitterHalfWidth >= 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.ObservationJitterHalfWidth),
                                                  "ObservationJitterHalfWidth must be finite, non-negative, and smaller than 0.5 category units.")
        End If
        If Not IsFinitePositive(appearance.CategoryLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CategoryLabelFontSize),
                                                  "CategoryLabelFontSize must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.MeanMarkerHalfSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MeanMarkerHalfSize),
                                                  "MeanMarkerHalfSize must be finite and positive.")
        End If

        ValidateAxisLimits(appearance.YAxisMinimum, appearance.YAxisMaximum)
        If appearance.YAxisMajorUnit.HasValue AndAlso Not IsFinitePositive(appearance.YAxisMajorUnit.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.YAxisMajorUnit),
                                                  "YAxisMajorUnit must be finite and positive.")
        End If

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As ViolinSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing Then Continue For
                If item.FillTransparency.HasValue AndAlso Not IsFiniteFraction(item.FillTransparency.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.FillTransparency),
                                                          "Series fill transparency must be between zero and one.")
                End If
                If item.OutlineWeight.HasValue AndAlso Not IsFinitePositive(item.OutlineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.OutlineWeight),
                                                          "Series outline weight must be finite and positive.")
                End If
            Next
        End If

        If appearance.ShowInnerBox Then
            Dim maximumViolinHalfWidth As Double = result.Options.MaximumHalfWidth
            If appearance.InnerBoxHalfWidth >= maximumViolinHalfWidth Then
                Throw New ArgumentOutOfRangeException(NameOf(appearance.InnerBoxHalfWidth),
                                                      "InnerBoxHalfWidth should be smaller than the violin MaximumHalfWidth.")
            End If
        End If
    End Sub

    Private Shared Sub ValidateAxisLimits(minimum As Nullable(Of Double),
                                          maximum As Nullable(Of Double))
        If minimum.HasValue AndAlso Not IsFinite(minimum.Value) Then
            Throw New ArgumentOutOfRangeException("YAxisMinimum", "Y-axis limits must be finite.")
        End If
        If maximum.HasValue AndAlso Not IsFinite(maximum.Value) Then
            Throw New ArgumentOutOfRangeException("YAxisMaximum", "Y-axis limits must be finite.")
        End If
        If minimum.HasValue AndAlso maximum.HasValue AndAlso minimum.Value >= maximum.Value Then
            Throw New ArgumentException("Y-axis minimum must be smaller than its maximum.")
        End If
    End Sub

    Private Shared Function ResolveAxisContext(result As ViolinPlotResult,
                                               appearance As ViolinPlotAppearance) As AxisContext
        Dim axis As New AxisContext With {
            .XMinimum = 0.5R,
            .XMaximum = result.GroupCount + 0.5R
        }

        If appearance.YAxisMinimum.HasValue Then
            axis.YMinimum = appearance.YAxisMinimum.Value
        End If
        If appearance.YAxisMaximum.HasValue Then
            axis.YMaximum = appearance.YAxisMaximum.Value
        End If

        If Not appearance.YAxisMinimum.HasValue OrElse Not appearance.YAxisMaximum.HasValue Then
            Dim autoMinimum As Double
            Dim autoMaximum As Double
            ResolveAutomaticYLimits(result.YMinimum, result.YMaximum, autoMinimum, autoMaximum)
            If Not appearance.YAxisMinimum.HasValue Then axis.YMinimum = autoMinimum
            If Not appearance.YAxisMaximum.HasValue Then axis.YMaximum = autoMaximum
        End If

        If axis.YMinimum >= axis.YMaximum Then
            Throw New ArgumentException("Resolved Y-axis minimum must be smaller than its maximum.")
        End If
        Return axis
    End Function

    Private Shared Sub ResolveAutomaticYLimits(dataMinimum As Double,
                                               dataMaximum As Double,
                                               ByRef axisMinimum As Double,
                                               ByRef axisMaximum As Double)
        If IsFinite(dataMinimum) AndAlso IsFinite(dataMaximum) AndAlso dataMaximum > dataMinimum Then
            Dim scale As graphics.CHARTscale = graphics.ChartingFunc.ChartScaling(dataMinimum, dataMaximum)
            If scale IsNot Nothing AndAlso IsFinite(scale.Min) AndAlso IsFinite(scale.Max) AndAlso scale.Max > scale.Min Then
                axisMinimum = scale.Min
                axisMaximum = scale.Max
                Return
            End If
        End If

        Dim centre As Double = If(IsFinite(dataMinimum), dataMinimum, 0.0R)
        Dim halfRange As Double = Math.Max(1.0R, Math.Abs(centre) * 0.1R)
        axisMinimum = centre - halfRange
        axisMaximum = centre + halfRange
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As ViolinPlotAppearance)
        chart.ChartArea.Format.Fill.Visible = True
        chart.ChartArea.Format.Fill.Solid()
        chart.ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
        chart.PlotArea.Format.Fill.Visible = True
        chart.PlotArea.Format.Fill.Solid()
        chart.PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
    End Sub

    Private Shared Sub ConfigureTitle(chart As Chart, title As String)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = title.Trim()
        End If
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     axisContext As AxisContext,
                                     appearance As ViolinPlotAppearance)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
        With xAxis
            .MinimumScale = axisContext.XMinimum
            .MaximumScale = axisContext.XMaximum
            .MajorUnit = 1.0R
            .Crosses = XlAxisCrosses.xlAxisCrossesMinimum
            .HasTitle = Not String.IsNullOrWhiteSpace(appearance.XAxisTitle)
            If .HasTitle Then .AxisTitle.Text = appearance.XAxisTitle.Trim()
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
            .MajorTickMark = XlTickMark.xlTickMarkNone
            .MinorTickMark = XlTickMark.xlTickMarkNone
            .HasMajorGridlines = False
            .HasMinorGridlines = False
        End With

        Dim yAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)
        With yAxis
            .MinimumScale = axisContext.YMinimum
            .MaximumScale = axisContext.YMaximum
            If appearance.YAxisMajorUnit.HasValue Then .MajorUnit = appearance.YAxisMajorUnit.Value
            .HasTitle = Not String.IsNullOrWhiteSpace(appearance.YAxisTitle)
            If .HasTitle Then .AxisTitle.Text = appearance.YAxisTitle.Trim()
            .HasMajorGridlines = appearance.ShowHorizontalGridlines
            .HasMinorGridlines = False
        End With
    End Sub

    Private Shared Sub AddCategoryLabelSeries(seriesCollection As SeriesCollection,
                                              series As ViolinPlotSeries(),
                                              axisContext As AxisContext,
                                              appearance As ViolinPlotAppearance)
        Dim xValues(series.Length - 1) As Double
        Dim yValues(series.Length - 1) As Double
        Dim anchorY As Double = axisContext.YMinimum + 0.012R * (axisContext.YMaximum - axisContext.YMinimum)

        For i As Integer = 0 To series.Length - 1
            xValues(i) = i + 1.0R
            yValues(i) = anchorY
        Next

        seriesCollection.NewSeries()
        Dim labelSeries As Object = seriesCollection(seriesCollection.Count - 1)
        With labelSeries
            .Name = "Category labels"
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
            .ApplyDataLabels(XlDataLabelsType.xlDataLabelsShowValue)
        End With

        For i As Integer = 0 To series.Length - 1
            Dim point As Object = labelSeries.Points(i + 1)
            point.DataLabel.Text = series(i).Name
            point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionBelow
            point.DataLabel.Font.Size = appearance.CategoryLabelFontSize
        Next
    End Sub

    Private Shared Sub DrawViolins(chart As Chart,
                                   result As ViolinPlotResult,
                                   axisContext As AxisContext,
                                   appearance As ViolinPlotAppearance)
        Dim plotArea As PlotArea = chart.PlotArea
        Dim insideLeft As Double = plotArea.InsideLeft
        Dim insideTop As Double = plotArea.InsideTop
        Dim insideWidth As Double = plotArea.InsideWidth
        Dim insideHeight As Double = plotArea.InsideHeight

        If insideWidth <= 0.0R OrElse insideHeight <= 0.0R Then
            Throw New InvalidOperationException("Excel returned an invalid plot-area size while drawing violins.")
        End If

        Dim chartObject As Object = chart
        Dim chartShapes As Object = chartObject.Shapes
        Dim allSeries As ViolinPlotSeries() = result.Series

        For groupIndex As Integer = 0 To allSeries.Length - 1
            Dim source As ViolinPlotSeries = allSeries(groupIndex)
            Dim style As ResolvedSeriesStyle = ResolveStyle(source.Name, groupIndex, appearance)
            Dim groupX As Double = groupIndex + 1.0R

            Dim evaluationPoints() As Double = source.EvaluationPoints
            Dim halfWidths() As Double = source.ScaledHalfWidths
            DrawViolinFreeform(chartShapes,
                               groupX,
                               evaluationPoints,
                               halfWidths,
                               axisContext,
                               insideLeft,
                               insideTop,
                               insideWidth,
                               insideHeight,
                               style,
                               groupIndex)

            'Keep raw observations in the same chart-shape coordinate system as the
            'violin and box. Native XY-scatter markers use the live plot-area transform
            'and therefore drift relative to chart-contained freeforms after a chart is
            'stretched/resized.
            If appearance.ShowIndividualObservations Then
                DrawObservationShapes(chartShapes,
                                      source,
                                      groupX,
                                      axisContext,
                                      insideLeft,
                                      insideTop,
                                      insideWidth,
                                      insideHeight,
                                      appearance,
                                      style,
                                      groupIndex)
            End If

            If appearance.ShowInnerBox OrElse appearance.ShowMedian OrElse appearance.ShowMean Then
                DrawInnerSummary(chartShapes,
                                 source,
                                 groupX,
                                 axisContext,
                                 insideLeft,
                                 insideTop,
                                 insideWidth,
                                 insideHeight,
                                 appearance,
                                 groupIndex)
            End If
        Next
    End Sub

    Private Shared Sub DrawViolinFreeform(chartShapes As Object,
                                          groupX As Double,
                                          evaluationPoints As Double(),
                                          halfWidths As Double(),
                                          axisContext As AxisContext,
                                          insideLeft As Double,
                                          insideTop As Double,
                                          insideWidth As Double,
                                          insideHeight As Double,
                                          style As ResolvedSeriesStyle,
                                          groupIndex As Integer)
        If evaluationPoints.Length <> halfWidths.Length OrElse evaluationPoints.Length < 2 Then
            Throw New ArgumentException("Violin evaluation points and widths are inconsistent.")
        End If

        Dim pointCount As Integer = evaluationPoints.Length * 2
        Dim xs(pointCount - 1) As Double
        Dim ys(pointCount - 1) As Double
        Dim k As Integer = 0

        'Right side, bottom to top.
        For i As Integer = 0 To evaluationPoints.Length - 1
            xs(k) = DataXToPlotX(groupX + halfWidths(i), axisContext, insideLeft, insideWidth)
            ys(k) = DataYToPlotY(evaluationPoints(i), axisContext, insideTop, insideHeight)
            k += 1
        Next

        'Left side, top to bottom.
        For i As Integer = evaluationPoints.Length - 1 To 0 Step -1
            xs(k) = DataXToPlotX(groupX - halfWidths(i), axisContext, insideLeft, insideWidth)
            ys(k) = DataYToPlotY(evaluationPoints(i), axisContext, insideTop, insideHeight)
            k += 1
        Next

        Dim builder As Object = chartShapes.BuildFreeform(MsoEditingCorner, CSng(xs(0)), CSng(ys(0)))
        For i As Integer = 1 To pointCount - 1
            builder.AddNodes(MsoSegmentLine, MsoEditingAuto, CSng(xs(i)), CSng(ys(i)))
        Next
        builder.AddNodes(MsoSegmentLine, MsoEditingAuto, CSng(xs(0)), CSng(ys(0)))

        Dim violinShape As Object = builder.ConvertToShape()
        violinShape.Name = "BESH_Violin_" & (groupIndex + 1).ToString(CultureInfo.InvariantCulture)
        With violinShape.Fill
            .Visible = True
            .Solid()
            .ForeColor.RGB = style.FillColor
            .Transparency = style.FillTransparency
        End With
        With violinShape.Line
            .Visible = style.OutlineWeight > 0.0F
            If style.OutlineWeight > 0.0F Then
                .ForeColor.RGB = style.OutlineColor
                .Weight = style.OutlineWeight
            End If
        End With
        violinShape.ZOrder(MsoSendToBack)
    End Sub

    Private Shared Sub DrawInnerSummary(chartShapes As Object,
                                        source As ViolinPlotSeries,
                                        groupX As Double,
                                        axisContext As AxisContext,
                                        insideLeft As Double,
                                        insideTop As Double,
                                        insideWidth As Double,
                                        insideHeight As Double,
                                        appearance As ViolinPlotAppearance,
                                        groupIndex As Integer)
        Dim xCentre As Double = DataXToPlotX(groupX, axisContext, insideLeft, insideWidth)
        Dim sourceHalfWidths() As Double = source.ScaledHalfWidths
        Dim displayedMaximumHalfWidth As Double = sourceHalfWidths.Max()
        Dim effectiveBoxHalfWidth As Double = Math.Min(appearance.InnerBoxHalfWidth, displayedMaximumHalfWidth * 0.45R)
        Dim effectiveCapHalfWidth As Double = Math.Min(appearance.WhiskerCapHalfWidth, displayedMaximumHalfWidth * 0.35R)
        Dim q1Y As Double = DataYToPlotY(source.Q1, axisContext, insideTop, insideHeight)
        Dim q3Y As Double = DataYToPlotY(source.Q3, axisContext, insideTop, insideHeight)
        Dim medianY As Double = DataYToPlotY(source.Median, axisContext, insideTop, insideHeight)
        Dim whiskerLowY As Double = DataYToPlotY(source.WhiskerLow, axisContext, insideTop, insideHeight)
        Dim whiskerHighY As Double = DataYToPlotY(source.WhiskerHigh, axisContext, insideTop, insideHeight)

        Dim boxLeft As Double = DataXToPlotX(groupX - effectiveBoxHalfWidth,
                                            axisContext,
                                            insideLeft,
                                            insideWidth)
        Dim boxRight As Double = DataXToPlotX(groupX + effectiveBoxHalfWidth,
                                             axisContext,
                                             insideLeft,
                                             insideWidth)
        Dim capLeft As Double = DataXToPlotX(groupX - effectiveCapHalfWidth,
                                            axisContext,
                                            insideLeft,
                                            insideWidth)
        Dim capRight As Double = DataXToPlotX(groupX + effectiveCapHalfWidth,
                                             axisContext,
                                             insideLeft,
                                             insideWidth)

        If appearance.ShowInnerBox Then
            Dim whiskerLine As Object = chartShapes.AddLine(CSng(xCentre),
                                                            CSng(whiskerHighY),
                                                            CSng(xCentre),
                                                            CSng(whiskerLowY))
            FormatInnerLine(whiskerLine, appearance)

            Dim upperCap As Object = chartShapes.AddLine(CSng(capLeft),
                                                         CSng(whiskerHighY),
                                                         CSng(capRight),
                                                         CSng(whiskerHighY))
            FormatInnerLine(upperCap, appearance)

            Dim lowerCap As Object = chartShapes.AddLine(CSng(capLeft),
                                                         CSng(whiskerLowY),
                                                         CSng(capRight),
                                                         CSng(whiskerLowY))
            FormatInnerLine(lowerCap, appearance)

            Dim boxTop As Double = Math.Min(q1Y, q3Y)
            Dim boxBottom As Double = Math.Max(q1Y, q3Y)
            Dim boxHeight As Double = Math.Max(1.0R, boxBottom - boxTop)
            Dim boxShape As Object = chartShapes.AddShape(MsoShapeRectangle,
                                                          CSng(boxLeft),
                                                          CSng(boxTop),
                                                          CSng(Math.Max(1.0R, boxRight - boxLeft)),
                                                          CSng(boxHeight))
            boxShape.Name = "BESH_ViolinBox_" & (groupIndex + 1).ToString(CultureInfo.InvariantCulture)
            With boxShape.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = appearance.InnerBoxFillColor
                .Transparency = appearance.InnerBoxFillTransparency
            End With
            With boxShape.Line
                .Visible = True
                .ForeColor.RGB = appearance.InnerLineColor
                .Weight = appearance.InnerLineWeight
            End With
        End If

        If appearance.ShowMedian Then
            Dim medianLine As Object = chartShapes.AddLine(CSng(boxLeft),
                                                           CSng(medianY),
                                                           CSng(boxRight),
                                                           CSng(medianY))
            FormatInnerLine(medianLine, appearance)
        End If

        If appearance.ShowMean Then
            Dim meanY As Double = DataYToPlotY(source.Mean, axisContext, insideTop, insideHeight)
            Dim d As Single = appearance.MeanMarkerHalfSize
            Dim line1 As Object = chartShapes.AddLine(CSng(xCentre - d),
                                                      CSng(meanY - d),
                                                      CSng(xCentre + d),
                                                      CSng(meanY + d))
            FormatInnerLine(line1, appearance)
            Dim line2 As Object = chartShapes.AddLine(CSng(xCentre - d),
                                                      CSng(meanY + d),
                                                      CSng(xCentre + d),
                                                      CSng(meanY - d))
            FormatInnerLine(line2, appearance)
        End If
    End Sub

    Private Shared Sub FormatInnerLine(lineShape As Object,
                                       appearance As ViolinPlotAppearance)
        With lineShape.Line
            .Visible = True
            .ForeColor.RGB = appearance.InnerLineColor
            .Weight = appearance.InnerLineWeight
        End With
    End Sub

    Private Shared Sub DrawObservationShapes(chartShapes As Object,
                                                    source As ViolinPlotSeries,
                                                    groupX As Double,
                                                    axisContext As AxisContext,
                                                    insideLeft As Double,
                                                    insideTop As Double,
                                                    insideWidth As Double,
                                                    insideHeight As Double,
                                                    appearance As ViolinPlotAppearance,
                                                    style As ResolvedSeriesStyle,
                                                    groupIndex As Integer)
        Dim observations() As Double = source.Observations
        Dim markerSize As Double = CDbl(appearance.ObservationMarkerSize)
        Dim markerRadius As Double = markerSize / 2.0R

        For i As Integer = 0 To observations.Length - 1
            Dim observation As Double = observations(i)

            'Match Excel's normal series clipping behaviour when custom Y limits are
            'used: do not draw a marker whose centre lies outside the plot range.
            If observation < axisContext.YMinimum OrElse observation > axisContext.YMaximum Then
                Continue For
            End If

            Dim jitter As Double = DeterministicJitter(i, appearance.ObservationJitterHalfWidth)
            Dim plotX As Double = DataXToPlotX(groupX + jitter,
                                               axisContext,
                                               insideLeft,
                                               insideWidth)
            Dim plotY As Double = DataYToPlotY(observation,
                                               axisContext,
                                               insideTop,
                                               insideHeight)

            Dim pointShape As Object = chartShapes.AddShape(MsoShapeOval,
                                                            CSng(plotX - markerRadius),
                                                            CSng(plotY - markerRadius),
                                                            CSng(markerSize),
                                                            CSng(markerSize))
            pointShape.Name = "BESH_ViolinObs_" &
                              (groupIndex + 1).ToString(CultureInfo.InvariantCulture) & "_" &
                              (i + 1).ToString(CultureInfo.InvariantCulture)

            'Preserve a circular marker as far as Excel allows when the containing
            'chart is resized non-proportionally.
            Try
                pointShape.LockAspectRatio = True
            Catch
                'Some Excel versions expose this property inconsistently for chart
                'shapes; marker placement still remains correct without it.
            End Try


            With pointShape.Fill
                .Visible = True
                .Solid()
                .ForeColor.RGB = style.PointColor
                .Transparency = 0.0F
            End With
            With pointShape.Line
                .Visible = False
            End With
        Next
    End Sub

    Private Shared Function DeterministicJitter(index As Integer,
                                                halfWidth As Double) As Double
        If halfWidth <= 0.0R Then Return 0.0R
        'Low-discrepancy golden-ratio sequence: stable across redraws and does not
        'require shared Random state.
        Const GoldenFraction As Double = 0.6180339887498949R
        Dim u As Double = ((index + 1) * GoldenFraction) Mod 1.0R
        Return (2.0R * u - 1.0R) * halfWidth
    End Function

    Private Shared Function ResolveStyle(seriesName As String,
                                         seriesIndex As Integer,
                                         appearance As ViolinPlotAppearance) As ResolvedSeriesStyle
        Dim fillColor As Integer = appearance.SeriesColors(seriesIndex Mod appearance.SeriesColors.Length)
        Dim fillTransparency As Single = appearance.FillTransparency
        Dim outlineColor As Integer = If(appearance.OutlineColor.HasValue,
                                         appearance.OutlineColor.Value,
                                         fillColor)
        Dim outlineWeight As Single = If(appearance.ShowOutline, appearance.OutlineWeight, 0.0F)
        Dim pointColor As Integer = If(appearance.ObservationColor.HasValue,
                                       appearance.ObservationColor.Value,
                                       fillColor)

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As ViolinSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing OrElse String.IsNullOrWhiteSpace(item.SeriesName) Then Continue For
                If String.Equals(item.SeriesName.Trim(), seriesName, StringComparison.CurrentCultureIgnoreCase) Then
                    If item.FillColor.HasValue Then fillColor = item.FillColor.Value
                    If item.FillTransparency.HasValue Then fillTransparency = item.FillTransparency.Value
                    If item.OutlineColor.HasValue Then outlineColor = item.OutlineColor.Value
                    If item.OutlineWeight.HasValue Then outlineWeight = item.OutlineWeight.Value
                    If item.PointColor.HasValue Then pointColor = item.PointColor.Value
                    Exit For
                End If
            Next
        End If

        If Not appearance.ShowOutline Then outlineWeight = 0.0F

        Return New ResolvedSeriesStyle With {
            .FillColor = fillColor,
            .FillTransparency = fillTransparency,
            .OutlineColor = outlineColor,
            .OutlineWeight = outlineWeight,
            .PointColor = pointColor
        }
    End Function

    Private Shared Function DataXToPlotX(value As Double,
                                         axisContext As AxisContext,
                                         insideLeft As Double,
                                         insideWidth As Double) As Double
        Return insideLeft +
               (value - axisContext.XMinimum) /
               (axisContext.XMaximum - axisContext.XMinimum) * insideWidth
    End Function

    Private Shared Function DataYToPlotY(value As Double,
                                         axisContext As AxisContext,
                                         insideTop As Double,
                                         insideHeight As Double) As Double
        Dim clippedValue As Double = Math.Max(axisContext.YMinimum, Math.Min(axisContext.YMaximum, value))
        Return insideTop +
               (1.0R - (clippedValue - axisContext.YMinimum) /
               (axisContext.YMaximum - axisContext.YMinimum)) * insideHeight
    End Function

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

    Private Shared Function IsFiniteFraction(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value >= 0.0F AndAlso value <= 1.0F
    End Function
End Class
