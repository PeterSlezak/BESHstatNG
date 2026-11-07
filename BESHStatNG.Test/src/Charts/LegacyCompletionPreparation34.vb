Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.graphics
Namespace Global.Batch34PreparationFixtures
Public Module LegacyBulletPreparation34


    Public Function ResolveNumberFormat(item As BulletChartItem,
                                                requestedFormat As String,
                                                Optional additionalValue As Nullable(Of Double) = Nothing) As String
        If Not String.IsNullOrWhiteSpace(requestedFormat) Then Return requestedFormat.Trim()
        Return BuildAutomaticNumberFormat(item.MajorInterval, item.ScaleMaximum, additionalValue)
    End Function
    Public Function BuildAutomaticNumberFormat(stepValue As Double,
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
    Public Function RequiredDecimalPlaces(value As Double) As Integer
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
    Public Function FormatNumeric(value As Double,
                                          format As String) As String
        Dim cleanValue As Double = value
        If Math.Abs(cleanValue) < 1.0E-14R Then cleanValue = 0.0R
        Return cleanValue.ToString(format, CultureInfo.CurrentCulture)
    End Function
    Public Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
End Module
Public Module LegacySankeyPreparation34


    Public Function FormatNodeLabel(node As SankeyNode,
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
End Module
Public Module LegacyCdfPreparation34

Public NotInheritable Class AxisContext
        Public XMinimum As Double
        Public XMaximum As Double
        Public XMajorUnit As Double
        Public XNumberFormat As String
        Public YMinimum As Double
        Public YMaximum As Double
        Public YMajorUnit As Double
        Public YNumberFormat As String
        Public YAxisTitle As String
    End Class
    Public Function ResolveAxisContext(results() As CumulativeDistributionPlotResult,
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

        Dim scale As ChartAxisScaleResult = ChartAxisScalingCore.Compute(xMinimum, xMaximum)
        Dim resolvedMinimum As Double = If(explicitMinimum.HasValue, explicitMinimum.Value, scale.Min)
        Dim resolvedMaximum As Double = If(explicitMaximum.HasValue, explicitMaximum.Value, scale.Max)

        If resolvedMaximum <= resolvedMinimum Then
            Throw New ArgumentException("The resolved CDF X-axis maximum must be larger than its minimum.", NameOf(results))
        End If

        Dim majorUnit As Double
        If appearance.XAxisMajorUnit.HasValue Then
            majorUnit = appearance.XAxisMajorUnit.Value
        Else
            Dim resolvedScale As ChartAxisScaleResult = ChartAxisScalingCore.Compute(resolvedMinimum, resolvedMaximum)
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
    Public Function FormatPercentileLabel(percentile As CumulativeDistributionPercentile,
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
    Public Function ResolveChartTitle(results() As CumulativeDistributionPlotResult,
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
    Public Function ResolveEmpiricalSeriesName(result As CumulativeDistributionPlotResult,
                                                       appearance As CumulativeDistributionPlotAppearance,
                                                       resultCount As Integer) As String
        Dim groupName As String = ResolveGroupLegendName(result)
        If appearance.LegendMode = CumulativeDistributionLegendMode.GroupsOnly Then Return groupName
        Return groupName & " - ECDF"
    End Function
    Public Function ResolveFittedSeriesName(result As CumulativeDistributionPlotResult,
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
    Public Function ResolveGroupLegendName(result As CumulativeDistributionPlotResult) As String
        If result Is Nothing OrElse String.IsNullOrWhiteSpace(result.SeriesName) Then Return "Data"
        Return result.SeriesName.Trim()
    End Function
    Public Function DistributionShortName(kind As CumulativeDistributionKind) As String
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
    Public Function AutomaticNumberFormat(majorUnit As Double) As String
        If Not IsFinitePositive(majorUnit) Then Return "0.###"

        Dim absoluteUnit As Double = Math.Abs(majorUnit)
        If absoluteUnit >= 1.0R Then Return "0.###"
        If absoluteUnit >= 0.1R Then Return "0.0##"
        If absoluteUnit >= 0.01R Then Return "0.00##"
        If absoluteUnit >= 0.001R Then Return "0.000##"
        Return "0.#####"
    End Function
    Public Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Public Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
Public Module LegacyDumbbellPreparation34

Public NotInheritable Class AxisContext
        Public XMinimum As Double
        Public XMaximum As Double
        Public XMajorUnit As Double
        Public YMinimum As Double
        Public YMaximum As Double
        Public XNumberFormat As String
    End Class
    Public Function ResolveAxisContext(result As DumbbellPlotResult,
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
    Public Function NiceStep125(rawStep As Double) As Double
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
    Public Function FloorToStep(value As Double, stepSize As Double) As Double
        Return Math.Floor(value / stepSize) * stepSize
    End Function
    Public Function CeilingToStep(value As Double, stepSize As Double) As Double
        Return Math.Ceiling(value / stepSize) * stepSize
    End Function
    Public Function ResolveXAxisNumberFormat(axisValueKind As DumbbellAxisValueKind,
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
    Public Function ResolveSeriesName(value As String, fallback As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return fallback
        Return value.Trim()
    End Function
    Public Function SuggestedHeight(result As DumbbellPlotResult) As Double
        If result Is Nothing OrElse result.CategoryCount < 1 Then Return 360.0R
        Return Math.Max(340.0R, Math.Min(1400.0R, 145.0R + 22.0R * result.CategoryCount))
    End Function
    Public Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Public Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
Public Module LegacyLadderPreparation34
    Private Const PlotXMinimum As Double = 0.5R
    Private Const PlotXMaximum As Double = 2.5R
Public NotInheritable Class AxisContext
        Public XMinimum As Double
        Public XMaximum As Double
        Public YMinimum As Double
        Public YMaximum As Double
        Public YMajorUnit As Double
        Public YNumberFormat As String
    End Class
    Public Function ResolveAxisContext(result As LadderPlotResult,
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
    Public Function ResolveYAxisNumberFormat(requestedFormat As String) As String
        If String.IsNullOrWhiteSpace(requestedFormat) Then Return "0.###"
        Return requestedFormat.Trim()
    End Function
    Public Function NiceStep125(rawStep As Double) As Double
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
    Public Function FloorToStep(value As Double, stepSize As Double) As Double
        Return Math.Floor(value / stepSize) * stepSize
    End Function
    Public Function CeilingToStep(value As Double, stepSize As Double) As Double
        Return Math.Ceiling(value / stepSize) * stepSize
    End Function
    Public Function BuildEndpointLabel(observation As LadderPlotObservation,
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
    Public Function FormatEndpointValue(value As Double, requestedFormat As String) As String
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
    Public Function ResolveObservationSeriesName(observation As LadderPlotObservation,
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
    Public Function SuggestedWidth(result As LadderPlotResult,
                                          Optional appearance As LadderPlotAppearance = Nothing) As Double
        Dim resolved As LadderPlotAppearance = If(appearance, New LadderPlotAppearance())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then Return 820.0R
        Return 620.0R
    End Function
    Public Function SuggestedHeight(result As LadderPlotResult,
                                           Optional appearance As LadderPlotAppearance = Nothing) As Double
        If result Is Nothing OrElse result.ObservationCount < 1 Then Return 420.0R
        Dim resolved As LadderPlotAppearance = If(appearance, New LadderPlotAppearance())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then
            Return Math.Max(420.0R, Math.Min(1200.0R, 210.0R + 17.0R * result.ObservationCount))
        End If
        Return 440.0R
    End Function
    Public Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Public Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
Public Module LegacyHullPreparation34


    Public Function ResolveSeriesName(group As ConvexHullPlotGroup,
                                              groupIndex As Integer,
                                              hasGrouping As Boolean,
                                              appearance As ConvexHullPlotAppearance) As String
        If hasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName), "Data", appearance.SeriesName.Trim())
    End Function
End Module
Public Module LegacyPolarPreparation34


    Public Function ResolveSeriesName(group As PolarPlotGroupSeries,
                                              groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarPlotAppearance) As String
        If result.HasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(Globalization.CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName),
                  "Data",
                  appearance.SeriesName.Trim())
    End Function
End Module
Public Module LegacySpcPreparation34


        Public Function NormalizeText(value As String) As String
            Return If(value, String.Empty).Trim()
        End Function
        Public Function NormalizeNumberFormat(value As String) As String
            Dim normalized As String = NormalizeText(value)
            If normalized.Length = 0 Then Return "0.####"
            Return normalized
        End Function
        Public Function ResolveChartTitle(
            result As SpcFitResult,
            panel As SpcPanelResult,
            options As SpcControlChartAppearanceOptions,
            panelCount As Integer) As String

            Dim baseTitle As String = options.ChartTitle
            If baseTitle.Length = 0 Then baseTitle = result.ChartTitle
            If panelCount <= 1 Then Return baseTitle
            Return baseTitle & " - " & panel.DisplayName
        End Function
        Public Function ResolveValueAxisTitle(
            result As SpcFitResult,
            panel As SpcPanelResult) As String
            If panel.ValueAxisTitle.Length > 0 Then Return panel.ValueAxisTitle
            If result.Request.ValueAxisTitle.Length > 0 Then Return result.Request.ValueAxisTitle
            Return panel.DisplayName
        End Function
        Public Function PanelSupportsSpecificationLimits(
            panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.Run,
                     SpcPanelType.IndividualValue,
                     SpcPanelType.SubgroupMean,
                     SpcPanelType.Ewma,
                     SpcPanelType.MovingAverage
                    Return True
                Case Else
                    Return False
            End Select
        End Function
End Module
End Namespace
