Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Pure legacy numeric text decisions. Hosts acquire cultures and retain
    ''' fallback evaluation order, field-specific errors, clocks and controls.</summary>
    Public NotInheritable Class NumberTextPolicy
        Private Sub New()
        End Sub

        Public Shared Function TryDouble(text As String, culture As CultureInfo, ByRef value As Double) As Boolean
            Return Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, culture, value)
        End Function

        Public Shared Function TryIntegerValue(value As Double, ByRef result As Integer) As Boolean
            If value < Integer.MinValue OrElse value > Integer.MaxValue OrElse value <> Math.Truncate(value) Then
                result = 0
                Return False
            End If
            result = CInt(value)
            Return True
        End Function

        Public Shared Function FormatGeneral(value As Double, culture As CultureInfo) As String
            Return value.ToString("G", culture)
        End Function

        Public Shared Function FormatProgressElapsedMilliseconds(elapsedMs As Double) As String
            If Double.IsNaN(elapsedMs) OrElse Double.IsInfinity(elapsedMs) OrElse elapsedMs < 0.0 Then Return String.Empty
            Return (elapsedMs / 1000.0).ToString("0.00", CultureInfo.InvariantCulture) & " s"
        End Function

        Public Shared Function FormatProgressDouble(value As Double) As String
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return String.Empty
            Dim absValue As Double = Math.Abs(value)
            If absValue > 0.0 AndAlso (absValue < 0.0001 OrElse absValue >= 1000000.0) Then
                Return value.ToString("0.###E+0", CultureInfo.InvariantCulture)
            End If
            Return value.ToString("0.######", CultureInfo.InvariantCulture)
        End Function
    End Class
End Namespace
