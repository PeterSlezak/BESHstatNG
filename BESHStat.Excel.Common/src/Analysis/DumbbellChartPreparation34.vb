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
Public NotInheritable Class DumbbellChartSettings34
Public ReadOnly Property XAxisPaddingFraction As Double
Public ReadOnly Property XAxisMajorUnit As Nullable(Of Double)
Public ReadOnly Property XAxisMinimum As Nullable(Of Double)
Public ReadOnly Property XAxisMaximum As Nullable(Of Double)
Public ReadOnly Property XAxisNumberFormat As String
Public Sub New(Optional inputXAxisPaddingFraction As Double = 0.04R, Optional inputXAxisMajorUnit As Nullable(Of Double) = Nothing, Optional inputXAxisMinimum As Nullable(Of Double) = Nothing, Optional inputXAxisMaximum As Nullable(Of Double) = Nothing, Optional inputXAxisNumberFormat As String = "")
Me.XAxisPaddingFraction = inputXAxisPaddingFraction
Me.XAxisMajorUnit = inputXAxisMajorUnit
Me.XAxisMinimum = inputXAxisMinimum
Me.XAxisMaximum = inputXAxisMaximum
Me.XAxisNumberFormat = inputXAxisNumberFormat
End Sub
End Class
Public Module DumbbellChartPreparation34

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
Private _XNumberFormat As String
Public Property XNumberFormat As String
Get
Return _XNumberFormat
End Get
Friend Set(value As String)
_XNumberFormat = value
End Set
End Property
End Class

    Public Function ResolveAxisContext(result As DumbbellPlotResult,
                                               appearance As DumbbellChartSettings34) As AxisContext
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
    Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Private Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module
End Namespace
