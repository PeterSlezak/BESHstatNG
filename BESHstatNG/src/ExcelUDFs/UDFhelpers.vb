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

        AppGlobals.BSlogg.Error(ex, logMessage)

        If String.IsNullOrWhiteSpace(uiPrefix) Then Return fallback

        Return uiPrefix & ex.Message
    End Function

    Friend Function LoggedUdfExceptionText(functionName As String, ex As Exception) As String
        AppGlobals.BSlogg.Error(ex, functionName & " failed")
        Return ex.GetType().Name & ": " & ex.Message
    End Function
End Module
