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
Public Enum LadderEndpointLabelContent
    Value
    RecordLabel
    RecordLabelAndValue
End Enum
Public Enum LadderColorMode
    ''' <summary>Every paired observation uses the same colour.</summary>
    SingleColor

    ''' <summary>Each paired observation cycles through the configured colour palette.</summary>
    ByObservation

    ''' <summary>Observations use the colour assigned to their grouping level.</summary>
    ByGroup
End Enum
Public NotInheritable Class LadderChartSettings34
Public ReadOnly Property YAxisPaddingFraction As Double
Public ReadOnly Property YAxisMajorUnit As Nullable(Of Double)
Public ReadOnly Property YAxisMinimum As Nullable(Of Double)
Public ReadOnly Property YAxisIncludeZero As Boolean
Public ReadOnly Property YAxisMaximum As Nullable(Of Double)
Public ReadOnly Property YAxisNumberFormat As String
Public ReadOnly Property EndpointValueDisplayFormat As String
Public ReadOnly Property EndpointLabelContent As LadderEndpointLabelContent
Public ReadOnly Property EndpointLabelSeparator As String
Public ReadOnly Property ColorMode As LadderColorMode
Public ReadOnly Property SeriesName As String
Public ReadOnly Property ShowFirstEndpointLabels As Boolean
Public ReadOnly Property ShowSecondEndpointLabels As Boolean
Public Sub New(Optional inputYAxisPaddingFraction As Double = 0.04R, Optional inputYAxisMajorUnit As Nullable(Of Double) = Nothing, Optional inputYAxisMinimum As Nullable(Of Double) = Nothing, Optional inputYAxisIncludeZero As Boolean = False, Optional inputYAxisMaximum As Nullable(Of Double) = Nothing, Optional inputYAxisNumberFormat As String = "", Optional inputEndpointValueDisplayFormat As String = "", Optional inputEndpointLabelContent As LadderEndpointLabelContent = LadderEndpointLabelContent.RecordLabelAndValue, Optional inputEndpointLabelSeparator As String = ", ", Optional inputColorMode As LadderColorMode = LadderColorMode.SingleColor, Optional inputSeriesName As String = "Data", Optional inputShowFirstEndpointLabels As Boolean = False, Optional inputShowSecondEndpointLabels As Boolean = False)
Me.YAxisPaddingFraction = inputYAxisPaddingFraction
Me.YAxisMajorUnit = inputYAxisMajorUnit
Me.YAxisMinimum = inputYAxisMinimum
Me.YAxisIncludeZero = inputYAxisIncludeZero
Me.YAxisMaximum = inputYAxisMaximum
Me.YAxisNumberFormat = inputYAxisNumberFormat
Me.EndpointValueDisplayFormat = inputEndpointValueDisplayFormat
Me.EndpointLabelContent = inputEndpointLabelContent
Me.EndpointLabelSeparator = inputEndpointLabelSeparator
Me.ColorMode = inputColorMode
Me.SeriesName = inputSeriesName
Me.ShowFirstEndpointLabels = inputShowFirstEndpointLabels
Me.ShowSecondEndpointLabels = inputShowSecondEndpointLabels
End Sub
End Class
Public Module LadderChartPreparation34
    Private Const PlotXMinimum As Double = 0.5R
    Private Const PlotXMaximum As Double = 2.5R
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
End Class

    Public Function ResolveAxisContext(result As LadderPlotResult,
                                               appearance As LadderChartSettings34) As AxisContext
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
                                               appearance As LadderChartSettings34) As String
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
                                                         appearance As LadderChartSettings34) As String
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
                                          Optional appearance As LadderChartSettings34 = Nothing) As Double
        Dim resolved As LadderChartSettings34 = If(appearance, New LadderChartSettings34())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then Return 820.0R
        Return 620.0R
    End Function
    Public Function SuggestedHeight(result As LadderPlotResult,
                                           Optional appearance As LadderChartSettings34 = Nothing) As Double
        If result Is Nothing OrElse result.ObservationCount < 1 Then Return 420.0R
        Dim resolved As LadderChartSettings34 = If(appearance, New LadderChartSettings34())
        If resolved.ShowFirstEndpointLabels OrElse resolved.ShowSecondEndpointLabels Then
            Return Math.Max(420.0R, Math.Min(1200.0R, 210.0R + 17.0R * result.ObservationCount))
        End If
        Return 440.0R
    End Function
    Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Private Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
End Namespace
