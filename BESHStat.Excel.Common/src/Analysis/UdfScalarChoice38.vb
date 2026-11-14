Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Legacy decisions on already normalized scalar values; no Excel sentinel or coercion types.</summary>
    Public NotInheritable Class UdfScalarChoice38
        Private Sub New()
        End Sub

        Public Shared Function OptionalBoolean(text As String, defaultValue As Boolean) As Boolean
            If String.IsNullOrEmpty(text) Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(text.ToLowerInvariant(), {"true", "t", "yes", "y", "1", "false", "f", "no", "n", "0"})
                Case "true", "t", "yes", "y", "1" : Return True
                Case "false", "f", "no", "n", "0" : Return False
                Case Else : Return defaultValue
            End Select
        End Function

        Public Shared Function OptionalInteger(value As Double?, defaultValue As Integer) As Integer
            If value.HasValue Then Return CInt(Math.Truncate(value.Value))
            Return defaultValue
        End Function

        Public Shared Function OptionalDouble(value As Double?, defaultValue As Double) As Double
            If value.HasValue Then Return value.Value
            Return defaultValue
        End Function

        Public Shared Function IsOpenProbability(value As Double) As Boolean
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return False
            If value <= 0.0 OrElse value >= 1.0 Then Return False
            Return True
        End Function

        Public Shared Function TryAlphaText(text As String, ByRef alpha As Double) As Boolean
            If Not Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, alpha) AndAlso
               Not Double.TryParse(text, alpha) Then Return False
            Return IsOpenProbability(alpha)
        End Function

        Public Shared Function TryKwType(text As String, defaultTieCorrected As Boolean, ByRef result As Integer) As Boolean
            result = 0
            If String.IsNullOrWhiteSpace(text) Then
                result = If(defaultTieCorrected, 1, 0)
                Return True
            End If
            Select Case LegacyUdfText38.CaseToken(text.Trim().ToUpperInvariant(),
                {"H", "UNCOR", "UNCORRECTED", "RAW", "HCOR", "H_COR", "TIE", "TIES", "COR", "CORR", "CORRECTED"})
                Case "H", "UNCOR", "UNCORRECTED", "RAW" : Return True
                Case "HCOR", "H_COR", "TIE", "TIES", "COR", "CORR", "CORRECTED"
                    result = 1
                    Return True
            End Select
            Return False
        End Function

        Public Shared Function FriedmanType(text As String, defaultToT1 As Boolean) As Integer
            If String.IsNullOrWhiteSpace(text) Then Return If(defaultToT1, 1, 2)
            Select Case LegacyUdfText38.CaseToken(text, {"T2", "F", "FAPPROX", "IMAN", "IMAN-DAVENPORT", "DAVENPORT"})
                Case "T2", "F", "FAPPROX", "IMAN", "IMAN-DAVENPORT", "DAVENPORT" : Return 2
            End Select
            Return 1
        End Function

        Public Shared Function TryBinary(value As Double?, logicalValue As Boolean?, ByRef result As Integer) As Boolean
            result = -1
            Return TryBinaryPrepared(value, logicalValue, result)
        End Function

        Public Shared Function TryBinaryPrepared(value As Double?, logicalValue As Boolean?, ByRef result As Integer) As Boolean
            If value.HasValue Then
                Dim integerValue As Integer = CInt(Math.Truncate(value.Value))
                If integerValue = 0 OrElse integerValue = 1 Then
                    result = integerValue
                    Return True
                End If
                Return False
            End If
            If logicalValue.HasValue Then
                result = If(logicalValue.Value, 1, 0)
                Return True
            End If
            Return False
        End Function

        Public Shared Function TryWholeNumber(value As Double?, ByRef result As Integer) As Boolean
            result = 0
            Return TryWholeNumberPrepared(value, result)
        End Function

        Public Shared Function TryWholeNumberPrepared(value As Double?, ByRef result As Integer) As Boolean
            If Not value.HasValue Then Return False
            If Double.IsNaN(value.Value) OrElse Double.IsInfinity(value.Value) Then Return False
            Dim rounded As Double = Math.Round(value.Value)
            If Math.Abs(value.Value - rounded) > 0.0000001R Then Return False
            If rounded < Integer.MinValue OrElse rounded > Integer.MaxValue Then Return False
            result = CInt(rounded)
            Return True
        End Function

        Public Shared Function TryFiniteText(text As String, ByRef result As Double) As Boolean
            If String.IsNullOrWhiteSpace(text) Then Return False
            If Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, result) Then
                Return Not Double.IsNaN(result) AndAlso Not Double.IsInfinity(result)
            End If
            If Double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, result) Then
                Return Not Double.IsNaN(result) AndAlso Not Double.IsInfinity(result)
            End If
            Return False
        End Function

        Public Shared Function TryPositiveInteger(value As Double?, missing As Boolean, ByRef result As Integer,
                                                   defaultValue As Integer, minimum As Integer) As Boolean
            result = defaultValue
            If missing Then Return True
            If Not value.HasValue Then Return False
            Dim number As Double = value.Value
            If Double.IsNaN(number) OrElse Double.IsInfinity(number) Then Return False
            Return TryPositiveIntegerPrepared(number, result, minimum)
        End Function

        Public Shared Function TryPositiveIntegerPrepared(number As Double, ByRef result As Integer, minimum As Integer) As Boolean
            Dim rounded As Double = Math.Round(number)
            If Math.Abs(number - rounded) > 0.0000001R Then Return False
            If rounded < minimum OrElse rounded > Integer.MaxValue Then Return False
            result = CInt(rounded)
            Return True
        End Function
    End Class
End Namespace
