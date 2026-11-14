Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Explicit historical Windows Option Compare Text semantics for normalized UDF options.</summary>
    Public NotInheritable Class LegacyUdfText38
        Private Sub New()
        End Sub

        Public Shared Function EqualText(left As String, right As String) As Boolean
            Dim options As CompareOptions = CompareOptions.IgnoreCase Or CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreWidth
            Return CultureInfo.CurrentCulture.CompareInfo.Compare(If(left, String.Empty), If(right, String.Empty), options) = 0
        End Function

        Public Shared Function CaseToken(value As String, choices As String()) As String
            For Each choice As String In choices
                If EqualText(value, choice) Then Return choice
            Next
            Return value
        End Function
    End Class
End Namespace
