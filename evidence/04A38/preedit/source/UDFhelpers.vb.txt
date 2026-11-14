Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Module UDFhelpers
    Friend Function LoggedUdfError(functionName As String,
                                   ex As Exception,
                                   fallback As Object,
                                   Optional uiPrefix As String = Nothing) As Object
        Dim logMessage As String = functionName & " failed"
        If Not String.IsNullOrWhiteSpace(uiPrefix) Then logMessage &= ". " & uiPrefix.Trim()

        CoreServices.Logger.Error(ex, logMessage)

        If String.IsNullOrWhiteSpace(uiPrefix) Then Return fallback

        Return uiPrefix & ex.Message
    End Function

    Friend Function LoggedUdfExceptionText(functionName As String, ex As Exception) As String
        CoreServices.Logger.Error(ex, functionName & " failed")
        Return ex.GetType().Name & ": " & ex.Message
    End Function

    ''' <summary>
    ''' Converts host-neutral binary-classification output values into Excel worksheet values.
    ''' </summary>
    ''' <remarks>
    ''' The statistical core returns <c>BinaryClassificationReporting.MissingOutputValue</c>
    ''' instead of referencing Excel-DNA directly. Excel UDFs call this helper at the boundary
    ''' so the sentinel is rendered as <c>#N/A</c> in worksheet output.
    ''' </remarks>
    Friend Function ConvertBinaryClassificationOutputForExcel(value As Object) As Object
        If Global.BESHStatNG.regression.BinaryClassificationReporting.IsMissingOutputValue(value) Then
            Return ExcelDna.Integration.ExcelError.ExcelErrorNA
        End If

        Dim table As Object(,) = TryCast(value, Object(,))
        If table Is Nothing Then Return value

        Dim rows As Integer = table.GetLength(0)
        Dim cols As Integer = table.GetLength(1)
        Dim out(rows - 1, cols - 1) As Object

        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                Dim cellValue As Object = table(r, c)
                If Global.BESHStatNG.regression.BinaryClassificationReporting.IsMissingOutputValue(cellValue) Then
                    out(r, c) = ExcelDna.Integration.ExcelError.ExcelErrorNA
                Else
                    out(r, c) = cellValue
                End If
            Next
        Next

        Return out
    End Function

End Module
