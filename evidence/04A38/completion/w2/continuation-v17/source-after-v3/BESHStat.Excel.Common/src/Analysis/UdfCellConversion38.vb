Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Legacy UDF conversion operations after host sentinel screening. Callers own exception handling. ByRef avoids an extra boxed-value copy; these methods never assign the input.</summary>
    Public NotInheritable Class UdfCellConversion38
        Private Sub New()
        End Sub

        Public Shared Function CellToTrimmedText(ByRef v As Object) As String
            If TypeOf v Is String Then
                Return DirectCast(v, String).Trim()
            End If

            If TypeOf v Is Double OrElse
               TypeOf v Is Single OrElse
               TypeOf v Is Decimal OrElse
               TypeOf v Is Integer OrElse
               TypeOf v Is Long OrElse
               TypeOf v Is Short Then

                Dim d As Double = Convert.ToDouble(v, CultureInfo.InvariantCulture)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return ""

                If Math.Abs(d - Math.Round(d)) < 0.000000000001R Then
                    Return CLng(Math.Round(d)).ToString(CultureInfo.InvariantCulture)
                End If

                Return d.ToString(CultureInfo.InvariantCulture)
            End If

            Dim s As String = Convert.ToString(v, CultureInfo.InvariantCulture)
            If s Is Nothing Then Return ""
            Return s.Trim()
        End Function

        Public Shared Function ConvertFiniteNumber(ByRef v As Object) As Double?
            Dim d As Double = Convert.ToDouble(v)
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
            Return d
        End Function

        Public Shared Function IsNumericTypeCode(code As TypeCode) As Boolean
            Select Case code
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                     TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                     TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
                Case Else
                    Return False
            End Select
        End Function
    End Class
End Namespace
