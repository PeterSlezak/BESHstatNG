Option Explicit On
Option Strict On

Imports System.Globalization
Imports BESHStatNG.AppInfrastructure

''' <summary>
''' Provides the common presentation rules for p-values while preserving their numeric values.
''' </summary>
''' <remarks>
''' Excel writers use <see cref="BuildExcelNumberFormat"/> so worksheet cells remain numeric.
''' Report-style presenters that cannot apply a number format can use
''' <see cref="FormatForDisplay"/> to produce equivalent display text.
''' </remarks>
Public NotInheritable Class PValuePresentation

    Private Sub New()
    End Sub

    Public Shared Function LowerThreshold(settings As PValuePresentationSettings) As Double
        Dim safeSettings As PValuePresentationSettings = GetSafeSettings(settings)
        Return Math.Pow(10.0, -safeSettings.DecimalPlaces)
    End Function

    Public Shared Function UpperThreshold(settings As PValuePresentationSettings) As Double
        Return 1.0 - LowerThreshold(settings)
    End Function

    ''' <summary>
    ''' Builds an invariant Excel <c>NumberFormat</c> string for the selected presentation rules.
    ''' </summary>
    Public Shared Function BuildExcelNumberFormat(settings As PValuePresentationSettings) As String
        Dim safeSettings As PValuePresentationSettings = GetSafeSettings(settings)
        Dim decimals As Integer = safeSettings.DecimalPlaces
        Dim fixedFormat As String = BuildFixedFormat(decimals)
        Dim lowerText As String = Math.Pow(10.0, -decimals).ToString("F" & decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
        Dim upperText As String = (1.0 - Math.Pow(10.0, -decimals)).ToString("F" & decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)

        Select Case safeSettings.SmallValueDisplay
            Case PValueSmallValueDisplay.LessThanThreshold
                Dim lowerSection As String = "[<" & lowerText & "]""<" & lowerText & """"
                If safeSettings.UseUpperBound Then
                    Return lowerSection & ";[>" & upperText & "]"">" & upperText & """;" & fixedFormat
                End If
                Return lowerSection & ";" & fixedFormat

            Case PValueSmallValueDisplay.ScientificNotation
                Dim lowerSection As String = "[<" & lowerText & "]" & BuildScientificFormat(decimals)
                If safeSettings.UseUpperBound Then
                    Return lowerSection & ";[>" & upperText & "]"">" & upperText & """;" & fixedFormat
                End If
                Return lowerSection & ";" & fixedFormat

            Case Else
                If safeSettings.UseUpperBound Then
                    Return "[>" & upperText & "]"">" & upperText & """;" & fixedFormat
                End If
                Return fixedFormat
        End Select
    End Function

    ''' <summary>
    ''' Formats a p-value for report-style outputs that cannot apply a numeric cell format.
    ''' </summary>
    Public Shared Function FormatForDisplay(value As Double,
                                            settings As PValuePresentationSettings,
                                            Optional formatProvider As IFormatProvider = Nothing) As String
        Dim safeSettings As PValuePresentationSettings = GetSafeSettings(settings)
        Dim provider As IFormatProvider = If(formatProvider, CultureInfo.CurrentCulture)
        Dim decimals As Integer = safeSettings.DecimalPlaces
        Dim lower As Double = Math.Pow(10.0, -decimals)
        Dim upper As Double = 1.0 - lower
        Dim fixedSpecifier As String = "F" & decimals.ToString(CultureInfo.InvariantCulture)

        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
            Return value.ToString(provider)
        End If

        If value < lower Then
            Select Case safeSettings.SmallValueDisplay
                Case PValueSmallValueDisplay.LessThanThreshold
                    Return "<" & lower.ToString(fixedSpecifier, provider)
                Case PValueSmallValueDisplay.ScientificNotation
                    Return value.ToString("E" & decimals.ToString(CultureInfo.InvariantCulture), provider)
            End Select
        End If

        If safeSettings.UseUpperBound AndAlso value > upper Then
            Return ">" & upper.ToString(fixedSpecifier, provider)
        End If

        Return value.ToString(fixedSpecifier, provider)
    End Function

    Private Shared Function GetSafeSettings(settings As PValuePresentationSettings) As PValuePresentationSettings
        If settings Is Nothing Then Return New PValuePresentationSettings()

        If settings.DecimalPlaces >= PValuePresentationSettings.MinimumDecimalPlaces AndAlso
           settings.DecimalPlaces <= PValuePresentationSettings.MaximumDecimalPlaces AndAlso
           [Enum].IsDefined(GetType(PValueSmallValueDisplay), settings.SmallValueDisplay) Then
            Return settings
        End If

        Dim safeSettings As New PValuePresentationSettings With {
            .DecimalPlaces = settings.DecimalPlaces,
            .SmallValueDisplay = settings.SmallValueDisplay,
            .UseUpperBound = settings.UseUpperBound
        }
        safeSettings.EnsureDefaults()
        Return safeSettings
    End Function

    Private Shared Function BuildFixedFormat(decimalPlaces As Integer) As String
        If decimalPlaces <= 0 Then Return "0"
        Return "0." & New String("0"c, decimalPlaces)
    End Function

    Private Shared Function BuildScientificFormat(decimalPlaces As Integer) As String
        If decimalPlaces <= 0 Then Return "0E+00"
        Return "0." & New String("0"c, decimalPlaces) & "E+00"
    End Function
End Class
