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
Public Module BulletChartPreparation34


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
    Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
End Module
End Namespace
