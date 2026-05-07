Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

' Mixed-model data import helpers for worksheet UDFs.
' Keeps MMRM-specific option/name parsing behind the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports a one-dimensional MMRM predictor-name list from comma-separated text or a one-row / one-column worksheet range.
    ''' Range blanks are preserved so callers can apply positional fallback names.
    ''' </summary>
    Friend Function TryGetMmrmNameList(arg As Object,
                                       ByRef names() As String) As Boolean
        names = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts As String() = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries).
                Select(Function(part) If(part, String.Empty).Trim()).
                ToArray()
            If parts.Length > 0 Then
                names = parts
                Return True
            End If
        End If

        Dim arr As Object(,) = UDFhelpers.Get2D(arg)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)()

        If rows = 1 AndAlso cols >= 1 Then
            For j As Integer = 0 To cols - 1
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, j)) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(arr(0, j), CultureInfo.InvariantCulture).Trim())
                End If
            Next
        ElseIf cols = 1 AndAlso rows >= 1 Then
            For i As Integer = 0 To rows - 1
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(i, 0)) Then
                    list.Add(String.Empty)
                Else
                    list.Add(Convert.ToString(arr(i, 0), CultureInfo.InvariantCulture).Trim())
                End If
            Next
        Else
            Return False
        End If

        names = list.ToArray()
        Return names.Length > 0
    End Function

End Module