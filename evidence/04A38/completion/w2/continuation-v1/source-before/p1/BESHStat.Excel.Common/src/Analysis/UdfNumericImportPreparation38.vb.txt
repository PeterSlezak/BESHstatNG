Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Namespace ExcelCommon
    Public NotInheritable Class UdfNumericImportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function TryStrictVector(count As Integer, blank As Func(Of Integer, Boolean),
                                                numeric As Func(Of Integer, Double?), ByRef values As Double()) As Boolean
            values = Nothing
            If count < 1 Then Return False
            Dim last As Integer = count - 1
            While last >= 0 AndAlso blank(last)
                last -= 1
            End While
            If last < 0 Then Return False
            Dim start As Integer = 0
            If Not numeric(0).HasValue Then
                If last = 0 Then Return False
                start = 1
            End If
            If start > last Then Return False
            Dim result As New List(Of Double)()
            For index As Integer = start To last
                If blank(index) Then Return False
                Dim value As Double? = numeric(index)
                If Not value.HasValue Then Return False
                result.Add(value.Value)
            Next
            If result.Count = 0 Then Return False
            values = result.ToArray()
            Return True
        End Function

        Public Shared Function TryExtractNumericGroupsFromMatrix(rows As Integer, cols As Integer, startRow As Integer, hasHeader As Boolean, defaultPrefix As String, numeric As Func(Of Integer, Integer, Double?), text As Func(Of Integer, Integer, String), ByRef groups()() As Double, ByRef names() As String) As Boolean
        groups = Nothing
        names = Nothing

        If rows < 1 OrElse cols < 1 Then Return False

        Dim groupList As New List(Of Double())()
        Dim nameList As New List(Of String)()

        For c As Integer = 0 To cols - 1
            Dim values As New List(Of Double)(Math.Max(0, rows - startRow))
            For r As Integer = startRow To rows - 1
                Dim d As Double? = numeric(r, c)
                If d.HasValue Then values.Add(d.Value)
            Next

            If values.Count > 0 Then
                groupList.Add(values.ToArray())
                If hasHeader Then
                    nameList.Add(text(0, c))
                Else
                    nameList.Add(defaultPrefix & (groupList.Count).ToString(CultureInfo.InvariantCulture))
                End If
            End If
        Next

        groups = groupList.ToArray()
        names = nameList.ToArray()
        Return True
        End Function

        Public Shared Function ExtractCompleteNumericMatrixCompleteCases(r As Integer, c As Integer, numeric As Func(Of Integer, Integer, Double?), ByRef mat As Double(,), ByRef noRows As Integer, ByRef noCols As Integer) As Boolean
        mat = Nothing
        noRows = 0
        noCols = 0

        Dim rows As New List(Of Double())()

        For i As Integer = 0 To r - 1
            Dim rowVals(c - 1) As Double
            Dim ok As Boolean = True

            For j As Integer = 0 To c - 1
                Dim d As Double? = numeric(i, j)
                If Not d.HasValue Then
                    ok = False
                    Exit For
                End If
                rowVals(j) = d.Value
            Next

            If ok Then rows.Add(rowVals)
        Next

        noRows = rows.Count
        noCols = c
        If noRows < 2 Then Return True

        mat = New Double(noRows - 1, c - 1) {}
        For i As Integer = 0 To noRows - 1
            For j As Integer = 0 To c - 1
                mat(i, j) = rows(i)(j)
            Next
        Next

        Return True
        End Function

        Public Shared Function TryReadCompleteNumericMatrixWithHeaders(rows As Integer, cols As Integer, numeric As Func(Of Integer, Integer, Double?), text As Func(Of Integer, Integer, String), ByRef mat As Double(,), ByRef names() As String) As Boolean
        mat = Nothing
        names = Nothing

        Dim numericCols As Integer() = Enumerable.Range(0, cols).ToArray()
        Dim hasHeader As Boolean = UdfRangePreparation38.LooksLikeHeaderRow(rows, numericCols, Function(row, column) numeric(row, column).HasValue)
        Dim startRow As Integer = If(hasHeader, 1, 0)

        names = New String(cols - 1) {}
        For c As Integer = 0 To cols - 1
            names(c) = If(hasHeader, text(0, c), "Condition " & (c + 1).ToString())
        Next

        Dim keepRows As New List(Of Double())()
        For r As Integer = startRow To rows - 1
            Dim row(cols - 1) As Double
            Dim ok As Boolean = True

            For c As Integer = 0 To cols - 1
                Dim d As Double? = numeric(r, c)
                If Not d.HasValue Then
                    ok = False
                    Exit For
                End If
                row(c) = d.Value
            Next

            If ok Then keepRows.Add(row)
        Next

        If keepRows.Count < 1 Then Return False

        mat = New Double(keepRows.Count - 1, cols - 1) {}
        For r As Integer = 0 To keepRows.Count - 1
            For c As Integer = 0 To cols - 1
                mat(r, c) = keepRows(r)(c)
            Next
        Next

        Return True
        End Function
    End Class
End Namespace
