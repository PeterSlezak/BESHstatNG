Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace ExcelCommon.Chart34
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
Public NotInheritable Class CdfChartSettings34
Public ReadOnly Property XAxisMajorUnit As Nullable(Of Double)
Public ReadOnly Property XAxisNumberFormat As String
Public ReadOnly Property PercentileValueFormat As String
Public ReadOnly Property IncludeSeriesNameInPercentileLabels As Boolean
Public ReadOnly Property ChartTitle As String
Public ReadOnly Property LegendMode As CumulativeDistributionLegendMode
Public Sub New(Optional inputXAxisMajorUnit As Nullable(Of Double) = Nothing, Optional inputXAxisNumberFormat As String = "", Optional inputPercentileValueFormat As String = "0.###", Optional inputIncludeSeriesNameInPercentileLabels As Boolean = True, Optional inputChartTitle As String = "", Optional inputLegendMode As CumulativeDistributionLegendMode = CumulativeDistributionLegendMode.GroupsOnly)
Me.XAxisMajorUnit = inputXAxisMajorUnit
Me.XAxisNumberFormat = inputXAxisNumberFormat
Me.PercentileValueFormat = inputPercentileValueFormat
Me.IncludeSeriesNameInPercentileLabels = inputIncludeSeriesNameInPercentileLabels
Me.ChartTitle = inputChartTitle
Me.LegendMode = inputLegendMode
End Sub
End Class
Public Module CdfChartPreparation34

Public NotInheritable Class AxisContext
Friend Sub New()
End Sub
Private _XMinimum As Double
Public Property XMinimum As Double
Get
Return _XMinimum
End Get
Friend Set(value As Double)
_XMinimum = value
End Set
End Property
Private _XMaximum As Double
Public Property XMaximum As Double
Get
Return _XMaximum
End Get
Friend Set(value As Double)
_XMaximum = value
End Set
End Property
Private _XMajorUnit As Double
Public Property XMajorUnit As Double
Get
Return _XMajorUnit
End Get
Friend Set(value As Double)
_XMajorUnit = value
End Set
End Property
Private _XNumberFormat As String
Public Property XNumberFormat As String
Get
Return _XNumberFormat
End Get
Friend Set(value As String)
_XNumberFormat = value
End Set
End Property
Private _YMinimum As Double
Public Property YMinimum As Double
Get
Return _YMinimum
End Get
Friend Set(value As Double)
_YMinimum = value
End Set
End Property
Private _YMaximum As Double
Public Property YMaximum As Double
Get
Return _YMaximum
End Get
Friend Set(value As Double)
_YMaximum = value
End Set
End Property
Private _YMajorUnit As Double
Public Property YMajorUnit As Double
Get
Return _YMajorUnit
End Get
Friend Set(value As Double)
_YMajorUnit = value
End Set
End Property
Private _YNumberFormat As String
Public Property YNumberFormat As String
Get
Return _YNumberFormat
End Get
Friend Set(value As String)
_YNumberFormat = value
End Set
End Property
Private _YAxisTitle As String
Public Property YAxisTitle As String
Get
Return _YAxisTitle
End Get
Friend Set(value As String)
_YAxisTitle = value
End Set
End Property
End Class

    Public Function ResolveAxisContext(results() As CumulativeDistributionPlotResult,
                                               appearance As CdfChartSettings34) As AxisContext
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
                                                  appearance As CdfChartSettings34,
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
                                              appearance As CdfChartSettings34,
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
                                                       appearance As CdfChartSettings34,
                                                       resultCount As Integer) As String
        Dim groupName As String = ResolveGroupLegendName(result)
        If appearance.LegendMode = CumulativeDistributionLegendMode.GroupsOnly Then Return groupName
        Return groupName & " - ECDF"
    End Function
    Public Function ResolveFittedSeriesName(result As CumulativeDistributionPlotResult,
                                                    appearance As CdfChartSettings34,
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
    Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Private Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
End Namespace
