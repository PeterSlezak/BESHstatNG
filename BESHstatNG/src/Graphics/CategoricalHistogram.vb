Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Backwards-compatible Windows entry point for categorical histogram calculations.
''' The options, series and numerical result contracts live in BESHStat.Core.
''' </summary>
Public NotInheritable Class CategoricalHistogram
    Private Sub New()
    End Sub

    Public Shared Function Compute(values As Double(),
                                   groups As Array,
                                   Optional options As CategoricalHistogramOptions = Nothing) As CategoricalHistogramResult
        Return CategoricalHistogramCore.Compute(values, groups, options)
    End Function
End Class

''' <summary>
''' Optional appearance override for one categorical level.
''' </summary>
Public Class CategoricalHistogramSeriesAppearance
    Public Property SeriesName As String
    Public Property FillColor As Nullable(Of Integer)
    Public Property FillTransparency As Nullable(Of Single)
    Public Property OutlineColor As Nullable(Of Integer)
    Public Property OutlineWeight As Nullable(Of Single)
End Class

''' <summary>
''' Excel-specific display settings for <see cref="CategoricalHistogramExcel"/>.
''' </summary>
Public Class CategoricalHistogramAppearance
    Public Property ChartTitle As String = "Categorical histogram"
    Public Property XAxisTitle As String = String.Empty
    Public Property YAxisTitle As String = String.Empty
    Public Property ShowLegend As Boolean = True
    Public Property ShowHorizontalGridlines As Boolean = False
    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionRight

    ''' <summary>
    ''' Excel column-chart gap width, from 0 (widest columns) to 500 (narrowest).
    ''' </summary>
    Public Property GapWidth As Integer = 30

    ''' <summary>
    ''' Excel series overlap for clustered columns, from -100 to 100. Zero gives
    ''' conventional side-by-side columns.
    ''' </summary>
    Public Property SeriesOverlap As Integer = 0

    Public Property FillTransparency As Single = 0.0F
    Public Property OutlineWeight As Single = 0.75F
    Public Property OutlineColor As Nullable(Of Integer) = Nothing

    Public Property SeriesColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property SeriesOverrides As CategoricalHistogramSeriesAppearance() = New CategoricalHistogramSeriesAppearance() {}
End Class

''' <summary>
''' Creates an embedded Excel chart from a <see cref="CategoricalHistogramResult"/>.
''' </summary>
Public NotInheritable Class CategoricalHistogramExcel
    Private Sub New()
    End Sub

    Private NotInheritable Class ResolvedSeriesStyle
        Friend FillColor As Integer
        Friend FillTransparency As Single
        Friend OutlineColor As Integer
        Friend OutlineWeight As Single
    End Class

    Public Shared Function AddChart(ws As Worksheet,
                                    result As CategoricalHistogramResult,
                                    Optional appearance As CategoricalHistogramAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 720.0R,
                                    Optional height As Double = 440.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If result.Series.Length > 255 Then
            Throw New ArgumentException("Excel charts support at most 255 data series for this chart.", NameOf(result))
        End If
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As CategoricalHistogramAppearance = If(appearance, New CategoricalHistogramAppearance())
        ValidateAppearance(resolvedAppearance)

        Dim chartType As XlChartType = ResolveChartType(result.Options)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(chartType, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = chartType
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)

            Dim binMidpoints As Double() = result.BinMidpoints
            Dim histogramSeries As CategoricalHistogramSeries() = result.Series

            For seriesIndex As Integer = 0 To histogramSeries.Length - 1
                Dim sourceSeries As CategoricalHistogramSeries = histogramSeries(seriesIndex)
                Dim style As ResolvedSeriesStyle = ResolveStyle(sourceSeries.Name,
                                                                seriesIndex,
                                                                resolvedAppearance)
                Dim plottedValues As Double() = ResolvePlottedValues(result.Options, sourceSeries, seriesIndex)
                AddColumnSeries(seriesCollection,
                                sourceSeries.Name,
                                binMidpoints,
                                plottedValues,
                                chartType,
                                style)
            Next

            ConfigureChart(chart, result, resolvedAppearance)
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

    Private Shared Sub ConfigureChart(chart As Chart,
                                      result As CategoricalHistogramResult,
                                      appearance As CategoricalHistogramAppearance)
        chart.HasTitle = Not String.IsNullOrWhiteSpace(appearance.ChartTitle)
        If chart.HasTitle Then chart.ChartTitle.Text = appearance.ChartTitle

        chart.HasLegend = appearance.ShowLegend
        If chart.HasLegend Then chart.Legend.Position = appearance.LegendPosition

        Dim categoryAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary), Axis)
        categoryAxis.HasTitle = Not String.IsNullOrWhiteSpace(appearance.XAxisTitle)
        If categoryAxis.HasTitle Then categoryAxis.AxisTitle.Text = appearance.XAxisTitle

        Dim valueAxis As Object = DirectCast(chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary), Axis)
        valueAxis.HasTitle = True
        valueAxis.AxisTitle.Text = ResolveYAxisTitle(result, appearance)

        If result.Options.PlotType = CategoricalHistogramPlotType.Comparative Then
            ConfigureComparativeAxes(categoryAxis, valueAxis, result)
        Else
            valueAxis.MinimumScale = 0.0R
        End If

        If valueAxis.HasMajorGridlines Then
            If appearance.ShowHorizontalGridlines Then
                valueAxis.MajorGridlines.Format.Line.Visible = True
            Else
                valueAxis.MajorGridlines.Delete()
            End If
        End If

        Dim chartGroup As ChartGroup = DirectCast(chart.ChartGroups(1), ChartGroup)
        chartGroup.GapWidth = appearance.GapWidth
        If result.Options.PlotType = CategoricalHistogramPlotType.Comparative Then
            'The two groups must occupy the same bin position and extend in opposite
            'directions from zero.
            chartGroup.Overlap = 100
        ElseIf result.Options.PlotType <> CategoricalHistogramPlotType.StackedBar Then
            chartGroup.Overlap = appearance.SeriesOverlap
        End If
    End Sub

    Private Shared Function ResolveYAxisTitle(result As CategoricalHistogramResult,
                                               appearance As CategoricalHistogramAppearance) As String
        If Not String.IsNullOrWhiteSpace(appearance.YAxisTitle) Then Return appearance.YAxisTitle

        Select Case result.Options.PlotType
            Case CategoricalHistogramPlotType.BarsWithLegend, CategoricalHistogramPlotType.StackedBar
                Return "Density"
            Case CategoricalHistogramPlotType.DifferentSampleSizes
                Return "Frequency"
            Case CategoricalHistogramPlotType.Comparative
                Return "Percent"
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Shared Function ResolveChartType(options As CategoricalHistogramOptions) As XlChartType
        If options.PlotType = CategoricalHistogramPlotType.StackedBar Then
            Return XlChartType.xlColumnStacked
        End If

        If options.PlotType = CategoricalHistogramPlotType.Comparative AndAlso
           options.ComparativeOrientation = CategoricalHistogramComparativeOrientation.Horizontal Then
            Return XlChartType.xlBarClustered
        End If

        Return XlChartType.xlColumnClustered
    End Function

    Private Shared Function ResolvePlottedValues(options As CategoricalHistogramOptions,
                                                 sourceSeries As CategoricalHistogramSeries,
                                                 seriesIndex As Integer) As Double()
        Dim values As Double() = sourceSeries.PlotValues
        If options.PlotType <> CategoricalHistogramPlotType.Comparative Then Return values

        'For vertical comparative histograms group 1 is above zero and group 2 below.
        'For horizontal comparative histograms group 1 is left of zero and group 2 right.
        Dim negate As Boolean
        If options.ComparativeOrientation = CategoricalHistogramComparativeOrientation.Horizontal Then
            negate = (seriesIndex = 0)
        Else
            negate = (seriesIndex = 1)
        End If

        If negate Then
            For i As Integer = 0 To values.Length - 1
                values(i) = -values(i)
            Next
        End If

        Return values
    End Function

    Private Shared Sub ConfigureComparativeAxes(categoryAxis As Axis,
                                                valueAxis As Axis,
                                                result As CategoricalHistogramResult)
        Dim axisLimit As Double = ResolveComparativeAxisLimit(result)
        valueAxis.MinimumScale = -axisLimit
        valueAxis.MaximumScale = axisLimit
        valueAxis.MajorUnit = ResolveComparativeMajorUnit(axisLimit)

        'Negative values are only a plotting device. Display both sides as positive
        'percentages, as is conventional for back-to-back histograms.
        valueAxis.TickLabels.NumberFormat = "0.##;0.##;0"

        'Keep bin labels at the outer edge (bottom for columns, left for bars) even
        'though the category-axis line itself crosses through zero in the centre.
        categoryAxis.TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionLow

        'CrossesAt is set on the value axis and controls where the category/baseline
        'axis crosses it. Keeping this at zero produces the central mirror baseline.
        valueAxis.CrossesAt = 0.0R
    End Sub

    Private Shared Function ResolveComparativeAxisLimit(result As CategoricalHistogramResult) As Double
        Dim maximumPercent As Double = 0.0R

        For Each item As CategoricalHistogramSeries In result.Series
            For Each value As Double In item.PerGroupPercent
                maximumPercent = Math.Max(maximumPercent, Math.Abs(value))
            Next
        Next

        If maximumPercent <= 0.0R Then Return 1.0R

        Dim rawMajorUnit As Double = maximumPercent / 5.0R
        Dim majorUnit As Double = NiceCeiling(rawMajorUnit)
        Return majorUnit * Math.Ceiling(maximumPercent / majorUnit)
    End Function

    Private Shared Function ResolveComparativeMajorUnit(axisLimit As Double) As Double
        If axisLimit <= 0.0R Then Return 1.0R
        Return NiceCeiling(axisLimit / 5.0R)
    End Function

    Private Shared Function NiceCeiling(value As Double) As Double
        If value <= 0.0R OrElse Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(value))
        Dim scale As Double = Math.Pow(10.0R, exponent)
        Dim fraction As Double = value / scale
        Dim niceFraction As Double

        If fraction <= 1.0R Then
            niceFraction = 1.0R
        ElseIf fraction <= 2.0R Then
            niceFraction = 2.0R
        ElseIf fraction <= 2.5R Then
            niceFraction = 2.5R
        ElseIf fraction <= 5.0R Then
            niceFraction = 5.0R
        Else
            niceFraction = 10.0R
        End If

        Return niceFraction * scale
    End Function

    Private Shared Sub AddColumnSeries(seriesCollection As SeriesCollection,
                                       seriesName As String,
                                       categories As Double(),
                                       values As Double(),
                                       chartType As XlChartType,
                                       style As ResolvedSeriesStyle)
        seriesCollection.NewSeries()

        'Keep the same late-bound SeriesCollection access pattern currently used by
        'BESHStatNG's KiteChart renderer. It avoids the COM type-mismatch observed with
        'SeriesCollection(SeriesCollection.Count) in affected Excel installations.
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = chartType
            .XValues = categories
            .Values = values
            .Format.Fill.Visible = True
            .Format.Fill.Solid()
            .Format.Fill.ForeColor.RGB = style.FillColor
            .Format.Fill.Transparency = style.FillTransparency
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = style.OutlineColor
            .Format.Line.Weight = style.OutlineWeight
            .Border.Color = style.OutlineColor
        End With
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Sub ValidateAppearance(appearance As CategoricalHistogramAppearance)
        If appearance.SeriesColors Is Nothing OrElse appearance.SeriesColors.Length = 0 Then
            Throw New ArgumentException("SeriesColors must contain at least one color.", NameOf(appearance.SeriesColors))
        End If
        If appearance.GapWidth < 0 OrElse appearance.GapWidth > 500 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GapWidth), "GapWidth must be between 0 and 500.")
        End If
        If appearance.SeriesOverlap < -100 OrElse appearance.SeriesOverlap > 100 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SeriesOverlap), "SeriesOverlap must be between -100 and 100.")
        End If
        If Not IsFiniteFraction(appearance.FillTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.FillTransparency), "Fill transparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.OutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.OutlineWeight), "Outline weight must be finite and positive.")
        End If

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As CategoricalHistogramSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing Then Continue For
                If item.FillTransparency.HasValue AndAlso Not IsFiniteFraction(item.FillTransparency.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.FillTransparency), "Series fill transparency must be between zero and one.")
                End If
                If item.OutlineWeight.HasValue AndAlso Not IsFinitePositive(item.OutlineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.OutlineWeight), "Series outline weight must be finite and positive.")
                End If
            Next
        End If
    End Sub

    Private Shared Function ResolveStyle(seriesName As String,
                                         seriesIndex As Integer,
                                         appearance As CategoricalHistogramAppearance) As ResolvedSeriesStyle
        Dim fillColor As Integer = appearance.SeriesColors(seriesIndex Mod appearance.SeriesColors.Length)
        Dim fillTransparency As Single = appearance.FillTransparency
        Dim outlineColor As Integer = If(appearance.OutlineColor.HasValue,
                                         appearance.OutlineColor.Value,
                                         fillColor)
        Dim outlineWeight As Single = appearance.OutlineWeight

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As CategoricalHistogramSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing OrElse String.IsNullOrWhiteSpace(item.SeriesName) Then Continue For
                If String.Equals(item.SeriesName.Trim(), seriesName, StringComparison.CurrentCultureIgnoreCase) Then
                    If item.FillColor.HasValue Then fillColor = item.FillColor.Value
                    If item.FillTransparency.HasValue Then fillTransparency = item.FillTransparency.Value
                    If item.OutlineColor.HasValue Then outlineColor = item.OutlineColor.Value
                    If item.OutlineWeight.HasValue Then outlineWeight = item.OutlineWeight.Value
                    Exit For
                End If
            Next
        End If

        Return New ResolvedSeriesStyle With {
            .FillColor = fillColor,
            .FillTransparency = fillTransparency,
            .OutlineColor = outlineColor,
            .OutlineWeight = outlineWeight
        }
    End Function

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
