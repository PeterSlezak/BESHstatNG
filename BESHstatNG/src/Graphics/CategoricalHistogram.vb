Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Selects one of the three filled multi-sample histogram presentations used by
''' <see cref="CategoricalHistogram"/>.
''' </summary>
Public Enum CategoricalHistogramPlotType
    ''' <summary>
    ''' Side-by-side bars with one independently normalised density histogram per group.
    ''' This corresponds to the Matplotlib "bars with legend" example.
    ''' </summary>
    BarsWithLegend

    ''' <summary>
    ''' Stacked bars. Group contributions are normalised together so that the total
    ''' stacked histogram integrates to one. This corresponds to the Matplotlib
    ''' "stacked bar" example with density=True and stacked=True.
    ''' </summary>
    StackedBar

    ''' <summary>
    ''' Side-by-side raw frequency counts. This is useful when the groups have different
    ''' sample sizes and corresponds to the Matplotlib "different sample sizes" example.
    ''' </summary>
    DifferentSampleSizes
End Enum

''' <summary>
''' Histogram bin-size rules supported by the ordinary BESHStatNG histogram.
''' </summary>
Public Enum CategoricalHistogramBinningRule
    Sturges
    Doane
    Scott
    FreedmanDiaconis
End Enum

''' <summary>
''' Numerical options for a categorical histogram.
''' </summary>
Public Class CategoricalHistogramOptions
    Public Property PlotType As CategoricalHistogramPlotType = CategoricalHistogramPlotType.BarsWithLegend
    Public Property BinningRule As CategoricalHistogramBinningRule = CategoricalHistogramBinningRule.Sturges

    ''' <summary>
    ''' Creates an independent copy of the options.
    ''' </summary>
    Friend Function Copy() As CategoricalHistogramOptions
        Return New CategoricalHistogramOptions With {
            .PlotType = PlotType,
            .BinningRule = BinningRule
        }
    End Function
End Class

''' <summary>
''' Histogram values for one categorical level.
''' </summary>
Public NotInheritable Class CategoricalHistogramSeries
    Private ReadOnly _name As String
    Private ReadOnly _groupValue As Object
    Private ReadOnly _sampleSize As Integer
    Private ReadOnly _counts As Double()
    Private ReadOnly _perGroupDensity As Double()
    Private ReadOnly _pooledDensityContribution As Double()
    Private ReadOnly _plotValues As Double()

    Friend Sub New(name As String,
                   groupValue As Object,
                   sampleSize As Integer,
                   counts As Double(),
                   perGroupDensity As Double(),
                   pooledDensityContribution As Double(),
                   plotValues As Double())
        _name = name
        _groupValue = groupValue
        _sampleSize = sampleSize
        _counts = DirectCast(counts.Clone(), Double())
        _perGroupDensity = DirectCast(perGroupDensity.Clone(), Double())
        _pooledDensityContribution = DirectCast(pooledDensityContribution.Clone(), Double())
        _plotValues = DirectCast(plotValues.Clone(), Double())
    End Sub

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    Public ReadOnly Property SampleSize As Integer
        Get
            Return _sampleSize
        End Get
    End Property

    ''' <summary>Raw frequency in each common bin.</summary>
    Public ReadOnly Property Counts As Double()
        Get
            Return DirectCast(_counts.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Density values normalised within this group: count / (group n * bin width).
    ''' </summary>
    Public ReadOnly Property PerGroupDensity As Double()
        Get
            Return DirectCast(_perGroupDensity.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Density contribution normalised against all usable observations:
    ''' count / (pooled n * bin width). Summing these contributions across groups gives
    ''' a density histogram with total area one.
    ''' </summary>
    Public ReadOnly Property PooledDensityContribution As Double()
        Get
            Return DirectCast(_pooledDensityContribution.Clone(), Double())
        End Get
    End Property

    ''' <summary>Values selected by the requested plot type and supplied to Excel.</summary>
    Public ReadOnly Property PlotValues As Double()
        Get
            Return DirectCast(_plotValues.Clone(), Double())
        End Get
    End Property
End Class

''' <summary>
''' Immutable numerical result returned by <see cref="CategoricalHistogram.Compute"/>.
''' </summary>
Public NotInheritable Class CategoricalHistogramResult
    Private ReadOnly _binMidpoints As Double()
    Private ReadOnly _series As CategoricalHistogramSeries()
    Private ReadOnly _options As CategoricalHistogramOptions
    Private ReadOnly _binWidth As Double
    Private ReadOnly _binMinimum As Double
    Private ReadOnly _binMaximum As Double
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _usableObservationCount As Integer
    Private ReadOnly _excludedObservationCount As Integer

    Friend Sub New(binMidpoints As Double(),
                   series As CategoricalHistogramSeries(),
                   options As CategoricalHistogramOptions,
                   binWidth As Double,
                   binMinimum As Double,
                   binMaximum As Double,
                   sourceObservationCount As Integer,
                   usableObservationCount As Integer,
                   excludedObservationCount As Integer)
        _binMidpoints = DirectCast(binMidpoints.Clone(), Double())
        _series = DirectCast(series.Clone(), CategoricalHistogramSeries())
        _options = options.Copy()
        _binWidth = binWidth
        _binMinimum = binMinimum
        _binMaximum = binMaximum
        _sourceObservationCount = sourceObservationCount
        _usableObservationCount = usableObservationCount
        _excludedObservationCount = excludedObservationCount
    End Sub

    Public ReadOnly Property BinMidpoints As Double()
        Get
            Return DirectCast(_binMidpoints.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Series As CategoricalHistogramSeries()
        Get
            Return DirectCast(_series.Clone(), CategoricalHistogramSeries())
        End Get
    End Property

    Public ReadOnly Property Options As CategoricalHistogramOptions
        Get
            Return _options.Copy()
        End Get
    End Property

    Public ReadOnly Property BinWidth As Double
        Get
            Return _binWidth
        End Get
    End Property

    Public ReadOnly Property BinMinimum As Double
        Get
            Return _binMinimum
        End Get
    End Property

    Public ReadOnly Property BinMaximum As Double
        Get
            Return _binMaximum
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property UsableObservationCount As Integer
        Get
            Return _usableObservationCount
        End Get
    End Property

    Public ReadOnly Property ExcludedObservationCount As Integer
        Get
            Return _excludedObservationCount
        End Get
    End Property

    Public ReadOnly Property GroupCount As Integer
        Get
            Return _series.Length
        End Get
    End Property

    Public ReadOnly Property BinCount As Integer
        Get
            Return _binMidpoints.Length
        End Get
    End Property
End Class

''' <summary>
''' Computes grouped histograms from one continuous variable and one categorical variable.
''' All groups always share one set of bins calculated from the pooled continuous data.
''' </summary>
Public NotInheritable Class CategoricalHistogram
    Private Sub New()
    End Sub

    Private NotInheritable Class WorkingGroup
        Friend Key As String
        Friend Name As String
        Friend Value As Object
        Friend Values As New List(Of Double)()
    End Class

    ''' <summary>
    ''' Computes a categorical histogram.
    ''' </summary>
    ''' <param name="values">
    ''' Continuous observations. Missing continuous values can be represented by NaN.
    ''' </param>
    ''' <param name="groups">
    ''' Categorical observations aligned row-by-row with <paramref name="values"/>.
    ''' Text and numeric group values are supported. Missing/blank group values are omitted.
    ''' </param>
    ''' <param name="options">Plot and binning options.</param>
    Public Shared Function Compute(values As Double(),
                                   groups As Array,
                                   Optional options As CategoricalHistogramOptions = Nothing) As CategoricalHistogramResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))
        If groups.Rank <> 1 Then
            Throw New ArgumentException("The categorical variable must be a one-dimensional array.", NameOf(groups))
        End If
        If groups.Length <> values.Length Then
            Throw New ArgumentException("The continuous and categorical variables must contain the same number of rows.",
                                        NameOf(groups))
        End If
        If values.Length = 0 Then
            Throw New ArgumentException("At least one observation is required.", NameOf(values))
        End If

        Dim resolvedOptions As CategoricalHistogramOptions = If(options, New CategoricalHistogramOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim orderedGroups As New List(Of WorkingGroup)()
        Dim groupsByKey As New Dictionary(Of String, WorkingGroup)(StringComparer.Ordinal)
        Dim pooledValues As New List(Of Double)()
        Dim excludedCount As Integer = 0
        Dim groupLowerBound As Integer = groups.GetLowerBound(0)

        For i As Integer = 0 To values.Length - 1
            Dim x As Double = values(i)
            Dim groupValue As Object = groups.GetValue(groupLowerBound + i)

            If Double.IsNaN(x) OrElse IsMissingGroupValue(groupValue) Then
                excludedCount += 1
                Continue For
            End If
            If Double.IsInfinity(x) Then
                Throw New ArgumentOutOfRangeException(NameOf(values),
                                                      "Continuous observation at row " & (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                      " is infinite.")
            End If

            Dim key As String = BuildGroupKey(groupValue)
            Dim working As WorkingGroup = Nothing
            If Not groupsByKey.TryGetValue(key, working) Then
                working = New WorkingGroup With {
                    .Key = key,
                    .Name = FormatGroupName(groupValue),
                    .Value = groupValue
                }
                groupsByKey.Add(key, working)
                orderedGroups.Add(working)
            End If

            working.Values.Add(x)
            pooledValues.Add(x)
        Next

        If pooledValues.Count = 0 Then
            Throw New ArgumentException("No usable paired continuous/categorical observations were found.")
        End If
        If orderedGroups.Count = 0 Then
            Throw New ArgumentException("No usable categorical levels were found.", NameOf(groups))
        End If

        Dim pooled() As Double = pooledValues.ToArray()
        Dim binTable As Object(,) = graphics.ChartingFunc.HistogramBinsComputation(pooled,
                                                                      BinningRuleToLegacyText(resolvedOptions.BinningRule))
        Dim binCount As Integer = binTable.GetLength(0)
        If binCount < 1 Then Throw New InvalidOperationException("Histogram binning returned no bins.")

        Dim midpoints(binCount - 1) As Double
        For binIndex As Integer = 0 To binCount - 1
            midpoints(binIndex) = CDbl(binTable(binIndex, 0))
        Next

        Dim binWidth As Double = ResolveBinWidth(midpoints, pooled)
        Dim binMinimum As Double = midpoints(0) - binWidth / 2.0R
        Dim binMaximum As Double = midpoints(midpoints.Length - 1) + binWidth / 2.0R
        Dim totalN As Integer = pooled.Length

        Dim outputSeries(orderedGroups.Count - 1) As CategoricalHistogramSeries
        For groupIndex As Integer = 0 To orderedGroups.Count - 1
            Dim working As WorkingGroup = orderedGroups(groupIndex)
            Dim counts As Double() = CountBins(working.Values, binMinimum, binMaximum, binWidth, binCount)
            Dim perGroupDensity(binCount - 1) As Double
            Dim pooledDensityContribution(binCount - 1) As Double
            Dim plotValues(binCount - 1) As Double

            For binIndex As Integer = 0 To binCount - 1
                perGroupDensity(binIndex) = counts(binIndex) / (CDbl(working.Values.Count) * binWidth)
                pooledDensityContribution(binIndex) = counts(binIndex) / (CDbl(totalN) * binWidth)

                Select Case resolvedOptions.PlotType
                    Case CategoricalHistogramPlotType.BarsWithLegend
                        plotValues(binIndex) = perGroupDensity(binIndex)
                    Case CategoricalHistogramPlotType.StackedBar
                        plotValues(binIndex) = pooledDensityContribution(binIndex)
                    Case CategoricalHistogramPlotType.DifferentSampleSizes
                        plotValues(binIndex) = counts(binIndex)
                    Case Else
                        Throw New ArgumentOutOfRangeException(NameOf(resolvedOptions.PlotType))
                End Select
            Next

            outputSeries(groupIndex) = New CategoricalHistogramSeries(working.Name,
                                                                       working.Value,
                                                                       working.Values.Count,
                                                                       counts,
                                                                       perGroupDensity,
                                                                       pooledDensityContribution,
                                                                       plotValues)
        Next

        Return New CategoricalHistogramResult(midpoints,
                                              outputSeries,
                                              resolvedOptions,
                                              binWidth,
                                              binMinimum,
                                              binMaximum,
                                              values.Length,
                                              totalN,
                                              excludedCount)
    End Function

    Private Shared Sub ValidateOptions(options As CategoricalHistogramOptions)
        If Not [Enum].IsDefined(GetType(CategoricalHistogramPlotType), options.PlotType) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.PlotType), "The categorical histogram plot type is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(CategoricalHistogramBinningRule), options.BinningRule) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.BinningRule), "The histogram binning rule is not defined.")
        End If
    End Sub

    Private Shared Function BinningRuleToLegacyText(rule As CategoricalHistogramBinningRule) As String
        Select Case rule
            Case CategoricalHistogramBinningRule.Sturges
                Return "(Sturges)"
            Case CategoricalHistogramBinningRule.Doane
                Return "(Doane)"
            Case CategoricalHistogramBinningRule.Scott
                Return "(Scott)"
            Case CategoricalHistogramBinningRule.FreedmanDiaconis
                Return "(Freedman-Diaconis)"
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(rule))
        End Select
    End Function

    Private Shared Function CountBins(values As IEnumerable(Of Double),
                                      binMinimum As Double,
                                      binMaximum As Double,
                                      binWidth As Double,
                                      binCount As Integer) As Double()
        Dim counts(binCount - 1) As Double

        For Each x As Double In values
            Dim index As Integer
            If x >= binMaximum Then
                index = binCount - 1
            Else
                index = CInt(Math.Floor((x - binMinimum) / binWidth))
                If index < 0 Then index = 0
                If index > binCount - 1 Then index = binCount - 1
            End If
            counts(index) += 1.0R
        Next

        Return counts
    End Function

    Private Shared Function ResolveBinWidth(midpoints As Double(), pooled As Double()) As Double
        If midpoints.Length > 1 Then
            Dim width As Double = midpoints(1) - midpoints(0)
            If IsFinitePositive(width) Then Return width
        End If

        'The ordinary histogram exposes midpoints/frequencies rather than the breaks.
        'For the uncommon one-bin case, reconstruct the same pretty-step logic closely
        'enough to preserve the current histogram behaviour and obtain a valid density.
        Dim dMin As Double = pooled.Min()
        Dim dMax As Double = pooled.Max()
        If dMax > dMin Then
            Return NiceStep125(dMax - dMin)
        End If

        Dim stepSize As Double = NiceStep125(Math.Abs(dMin))
        If Not IsFinitePositive(stepSize) Then stepSize = 1.0R
        Return 2.0R * stepSize
    End Function

    Private Shared Function NiceStep125(rawStep As Double) As Double
        rawStep = Math.Abs(rawStep)
        If rawStep = 0.0R OrElse Double.IsNaN(rawStep) OrElse Double.IsInfinity(rawStep) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
        Dim fraction As Double = rawStep / (10.0R ^ exponent)
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

        Return niceFraction * (10.0R ^ exponent)
    End Function

    Private Shared Function IsMissingGroupValue(value As Object) As Boolean
        If value Is Nothing OrElse value Is DBNull.Value Then Return True

        If TypeOf value Is String Then
            Return String.IsNullOrWhiteSpace(DirectCast(value, String))
        End If

        If IsNumeric(value) Then
            Dim d As Double = CDbl(value)
            Return Double.IsNaN(d) OrElse Double.IsInfinity(d)
        End If

        Return False
    End Function

    Private Shared Function BuildGroupKey(value As Object) As String
        If IsNumeric(value) Then
            Return "N:" & CDbl(value).ToString("R", CultureInfo.InvariantCulture)
        End If
        Return "S:" & Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function FormatGroupName(value As Object) As String
        If IsNumeric(value) Then
            Return Convert.ToDouble(value, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0R
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

        Dim chartType As XlChartType = If(result.Options.PlotType = CategoricalHistogramPlotType.StackedBar,
                                          XlChartType.xlColumnStacked,
                                          XlChartType.xlColumnClustered)

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
                AddColumnSeries(seriesCollection,
                                sourceSeries.Name,
                                binMidpoints,
                                sourceSeries.PlotValues,
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
        valueAxis.MinimumScale = 0.0R
        valueAxis.HasTitle = True
        valueAxis.AxisTitle.Text = ResolveYAxisTitle(result, appearance)

        If valueAxis.HasMajorGridlines Then
            If appearance.ShowHorizontalGridlines Then
                valueAxis.MajorGridlines.Format.Line.Visible = True
            Else
                valueAxis.MajorGridlines.Delete()
            End If
        End If

        Dim chartGroup As ChartGroup = DirectCast(chart.ChartGroups(1), ChartGroup)
        chartGroup.GapWidth = appearance.GapWidth
        If result.Options.PlotType <> CategoricalHistogramPlotType.StackedBar Then
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
            Case Else
                Return String.Empty
        End Select
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
