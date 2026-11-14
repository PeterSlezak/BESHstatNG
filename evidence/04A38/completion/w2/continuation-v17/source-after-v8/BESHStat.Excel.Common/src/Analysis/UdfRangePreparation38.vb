Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    Public NotInheritable Class UdfRangePreparation38
        Private Sub New()
        End Sub

        Public Shared Function FindLastNonBlankRow(rows As Integer, columns As Integer, blank As Func(Of Integer, Integer, Boolean)) As Integer

        For i As Integer = rows - 1 To 0 Step -1
            For j As Integer = 0 To columns - 1
                If Not blank(i, j) Then
                    Return i
                End If
            Next
        Next

        Return -1
        End Function

        Public Shared Function HasNumericColumnHeader(lastRow As Integer, columns As Integer, blank As Func(Of Integer, Integer, Boolean), numeric As Func(Of Integer, Integer, Boolean)) As Boolean
        If lastRow < 1 Then Return False
        If blank(0, 0) Then Return False
        Dim firstIsNumeric As Boolean = numeric(0, 0)
        Dim secondIsNumeric As Boolean = numeric(1, 0)
        Return (Not firstIsNumeric) AndAlso secondIsNumeric
        End Function

        Public Shared Function HasBinaryColumnHeader(lastRow As Integer, binary As Func(Of Integer, Integer, Boolean)) As Boolean
        If lastRow < 1 Then Return False

        Dim firstIsBinary As Boolean = binary(0, 0)
        Dim secondIsBinary As Boolean = binary(1, 0)
        Return (Not firstIsBinary) AndAlso secondIsBinary
        End Function

        Public Shared Function HasNumericMatrixHeader(lastRow As Integer, columns As Integer, blank As Func(Of Integer, Integer, Boolean), numeric As Func(Of Integer, Integer, Boolean)) As Boolean
        If lastRow < 1 Then Return False

        Dim cols As Integer = columns
        Dim anyNonBlankNonNumericFirstRow As Boolean = False
        Dim anyNumericFirstRow As Boolean = False
        For j As Integer = 0 To cols - 1
            If blank(0, j) Then Continue For

            If numeric(0, j) Then
                anyNumericFirstRow = True
            Else
                anyNonBlankNonNumericFirstRow = True
            End If
        Next

        ' A real numeric-matrix header should look like labels, not like a data row
        ' with one or more missing numeric values. This prevents explicit data ranges
        ' whose first observation has missing values from being shortened before row-wise
        ' complete-case filtering aligns y, x, subject, and visit.
        If Not anyNonBlankNonNumericFirstRow Then Return False
        If anyNumericFirstRow Then Return False

        For j As Integer = 0 To cols - 1
            If Not numeric(1, j) Then
                Return False
            End If
        Next

        Return True
        End Function

        Public Shared Function LooksLikeHeaderRow(rows As Integer, numericCols As Integer(), numeric As Func(Of Integer, Integer, Boolean)) As Boolean
        If rows < 2 Then Return False

        Dim anyNonNumeric As Boolean = False
        For Each c In numericCols
            If Not numeric(0, c) Then
                anyNonNumeric = True
                Exit For
            End If
        Next
        If Not anyNonNumeric Then Return False

        For Each c In numericCols
            Dim foundNumericBelow As Boolean = False
            For r As Integer = 1 To rows - 1
                If numeric(r, c) Then
                    foundNumericBelow = True
                    Exit For
                End If
            Next
            If Not foundNumericBelow Then Return False
        Next

        Return True
        End Function

        Public Shared Function LooksLikeSingleColumnHeader(rows As Integer, numeric As Func(Of Integer, Integer, Boolean)) As Boolean

        If rows < 2 Then Return False
        If numeric(0, 0) Then Return False

        For r As Integer = 1 To rows - 1
            If numeric(r, 0) Then Return True
        Next

        Return False
        End Function
    End Class
End Namespace
