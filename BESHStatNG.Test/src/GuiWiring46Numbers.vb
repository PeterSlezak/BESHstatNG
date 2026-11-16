Option Strict On
Option Explicit On
Imports System
Imports System.Globalization
Namespace Harness46
Friend Module SourceNumbers46
 Friend HostCulture As CultureInfo = CultureInfo.InvariantCulture
 Friend Function GetExcelNumberCulture() As CultureInfo
  Return HostCulture
 End Function
    Public Function TryParseUiDouble(text As String, ByRef value As Double) As Boolean
        Dim s As String = If(text, String.Empty).Trim()

        If s = String.Empty Then
            value = 0
            Return False
        End If

        Return ExcelCommon.NumberTextPolicy.TryDouble(s, GetExcelNumberCulture(), value) OrElse
           ExcelCommon.NumberTextPolicy.TryDouble(s, Globalization.CultureInfo.CurrentCulture, value) OrElse
           ExcelCommon.NumberTextPolicy.TryDouble(s, Globalization.CultureInfo.InvariantCulture, value)
    End Function
    Public Function ParseUiDouble(text As String, Optional fieldName As String = "numeric value") As Double
        Dim value As Double
        If Not TryParseUiDouble(text, value) Then
            Throw New FormatException($"Cannot parse {fieldName}: ""{text}"".")
        End If
        Return value
    End Function
    Public Function FormatUiDouble(value As Double) As String
        Return ExcelCommon.NumberTextPolicy.FormatGeneral(value, GetExcelNumberCulture())
    End Function
    Public Function CheckNumeric(ByRef txtData As FakeControl46,
                             Optional ByRef dResult As Double = Double.NaN,
                             Optional ByRef sError As String = "") As Boolean

        'check for valid entry
        If txtData.text = String.Empty Then 'allow empty
            dResult = 0
            sError = String.Empty
            CheckNumeric = True

            'and give the text box its usual background
            txtData.BackColor = System.Drawing.Color.White

        ElseIf TryParseUiDouble(CStr(txtData.text), dResult) Then
            sError = String.Empty
            CheckNumeric = True

            'and give the text box its usual background
            txtData.BackColor = System.Drawing.Color.White

        Else 'not numeric
            dResult = 0
            sError = "Entry is not a number."
            CheckNumeric = False

            'give the text box a red background
            txtData.BackColor = System.Drawing.Color.Red
            'add the error message to the tooltip
            'txtData.ControlTipText = txtData.ControlTipText + sErr + sError

        End If
    End Function
End Module
End Namespace
