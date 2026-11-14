Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public NotInheritable Class UdfClassLabelPreparation38
        Private Sub New()
        End Sub

        Public Shared Function GuessPositiveClass(explicitText As String, rowCount As Func(Of Integer),
                                                   text As Func(Of Integer, String), binary As Func(Of Integer, Boolean),
                                                   ByRef positiveLabel As String) As Boolean
            positiveLabel = Nothing
            If Not String.IsNullOrWhiteSpace(explicitText) Then
                positiveLabel = explicitText
                Return True
            End If
            Dim distinctTokens As New List(Of String)()
            Dim distinctNormalized As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim allBinary01 As Boolean = True
            For i As Integer = 0 To rowCount() - 1
                Dim token As String = text(i)
                If String.IsNullOrWhiteSpace(token) Then Continue For
                If Not binary(i) Then allBinary01 = False
                Dim normalized As String = UdfDataImportOptions38.NormalizeOptionKey(token)
                If Not distinctNormalized.Contains(normalized) Then
                    distinctNormalized.Add(normalized)
                    distinctTokens.Add(token)
                End If
            Next
            If allBinary01 Then
                positiveLabel = "1"
                Return True
            End If
            If distinctTokens.Count = 2 Then
                Dim norms As String() = distinctTokens.Select(Function(token) UdfDataImportOptions38.NormalizeOptionKey(token)).ToArray()
                For i As Integer = 0 To norms.Length - 1
                    Select Case LegacyUdfText38.CaseToken(norms(i), {"positive", "pos", "case", "cases", "event", "events", "yes", "true"})
                        Case "positive", "pos", "case", "cases", "event", "events", "yes", "true"
                            positiveLabel = distinctTokens(i)
                            Return True
                    End Select
                Next
                positiveLabel = distinctTokens(0)
                Return True
            End If
            Return False
        End Function

        ' The native wrapper has already reset its output before converting the
        ' explicit-positive cell. False/throw must retain conversion/callback aliases.
        ' Existing callers keep the original resetting GuessPositiveClass contract.
        Public Shared Function GuessPositiveClassAfterConversion(explicitText As String, rowCount As Func(Of Integer),
                                                                  text As Func(Of Integer, String), binary As Func(Of Integer, Boolean),
                                                                  ByRef positiveLabel As String) As Boolean
            Dim prepared As String = Nothing
            Dim result As Boolean = GuessPositiveClass(explicitText, rowCount, text, binary, prepared)
            If result Then positiveLabel = prepared
            Return result
        End Function
    End Class
End Namespace
