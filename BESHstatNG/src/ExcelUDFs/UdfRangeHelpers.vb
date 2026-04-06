Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Shared helpers for reading Excel ranges into typed vectors/matrices and parsing common UDF options.
    ''' Intended to be reused by regression-model UDFs (Cox, GLM, Linear Model, GEE, ...).
    ''' </summary>
    Friend Module UdfRangeHelpers

        Public Function AsString(v As Object) As String
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing
            Return Convert.ToString(v).Trim()
        End Function

        Public Function GetOptionalBool(v As Object, defaultValue As Boolean) As Boolean
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            If TypeOf v Is Boolean Then Return CBool(v)
            Dim s As String = Convert.ToString(v).Trim()
            If String.IsNullOrEmpty(s) Then Return defaultValue
            Select Case s.ToLowerInvariant()
                Case "true", "t", "yes", "y", "1"
                    Return True
                Case "false", "f", "no", "n", "0"
                    Return False
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Function GetOptionalInt(v As Object, defaultValue As Integer) As Integer
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            Dim d As Double?
            d = TryGetDouble(v)
            If d.HasValue Then
                Return CInt(Math.Truncate(d.Value))
            End If
            Return defaultValue
        End Function

        Public Function GetOptionalDouble(v As Object, defaultValue As Double) As Double
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            Dim d As Double?
            d = TryGetDouble(v)
            If d.HasValue Then Return d.Value
            Return defaultValue
        End Function

        Public Function ParseTieMethod(v As Object, defaultValue As TieMethod) As TieMethod
            Dim s As String = AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return defaultValue
            Select Case s.ToLowerInvariant()
                Case "breslow"
                    Return TieMethod.Breslow
                Case "efron"
                    Return TieMethod.Efron
                Case "exact"
                    Return TieMethod.Exact
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Function GetVarNames(varNames As Object, p As Integer) As String()
            Dim fallback(p - 1) As String
            For i As Integer = 0 To p - 1
                fallback(i) = "X" & (i + 1).ToString()
            Next

            If varNames Is Nothing OrElse TypeOf varNames Is ExcelEmpty OrElse TypeOf varNames Is ExcelMissing Then
                Return fallback
            End If

            Dim s As String = TryCast(varNames, String)
            If s IsNot Nothing Then
                Dim parts = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length = p Then
                    For i As Integer = 0 To p - 1
                        parts(i) = parts(i).Trim()
                    Next
                    Return parts
                End If
            End If

            Dim arr As Object(,) = Get2D(varNames)
            If arr Is Nothing Then Return fallback

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)

            Dim list As New List(Of String)

            If rows = 1 AndAlso cols >= 1 Then
                For j As Integer = 0 To cols - 1
                    Dim v = arr(0, j)
                    If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
                        list.Add("")
                    Else
                        list.Add(Convert.ToString(v).Trim())
                    End If
                Next

            ElseIf cols = 1 AndAlso rows >= 1 Then
                For i As Integer = 0 To rows - 1
                    Dim v = arr(i, 0)
                    If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
                        list.Add("")
                    Else
                        list.Add(Convert.ToString(v).Trim())
                    End If
                Next
            End If

            If list.Count = p Then
                For i As Integer = 0 To p - 1
                    If String.IsNullOrWhiteSpace(list(i)) Then list(i) = fallback(i)
                Next
                Return list.ToArray()
            End If

            Return fallback
        End Function

        Public Function Get2D(v As Object) As Object(,)
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing
            If TypeOf v Is Object(,) Then Return CType(v, Object(,))
            Return Nothing
        End Function

        Public Function TryReadNumericColumn(v As Object, ByRef values As List(Of Double)) As Boolean
            values = New List(Of Double)
            Dim arr As Object(,) = Get2D(v)
            If arr Is Nothing Then Return False
            If arr.GetLength(1) <> 1 Then Return False

            For i As Integer = 0 To arr.GetLength(0) - 1
                Dim d As Double? = TryGetDouble(arr(i, 0))
                If d.HasValue AndAlso Not Double.IsNaN(d.Value) AndAlso Not Double.IsInfinity(d.Value) Then
                    values.Add(d.Value)
                Else
                    values.Add(Double.NaN)
                End If
            Next
            Return True
        End Function

        Public Function TryReadBinary01Column(v As Object, ByRef values As List(Of Integer)) As Boolean
            values = New List(Of Integer)
            Dim arr As Object(,) = Get2D(v)
            If arr Is Nothing Then Return False
            If arr.GetLength(1) <> 1 Then Return False

            For i As Integer = 0 To arr.GetLength(0) - 1
                Dim cell = arr(i, 0)
                If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then
                    values.Add(-1)
                    Continue For
                End If

                Dim d As Double? = TryGetDouble(cell)
                If d.HasValue Then
                    Dim iv As Integer = CInt(Math.Truncate(d.Value))
                    If iv = 0 OrElse iv = 1 Then
                        values.Add(iv)
                    Else
                        values.Add(-1)
                    End If
                ElseIf TypeOf cell Is Boolean Then
                    values.Add(If(CBool(cell), 1, 0))
                Else
                    values.Add(-1)
                End If
            Next
            Return True
        End Function

        Public Function TryReadTextColumn(v As Object, ByRef values As List(Of String)) As Boolean
            values = New List(Of String)
            Dim arr As Object(,) = Get2D(v)
            If arr Is Nothing Then Return False
            If arr.GetLength(1) <> 1 Then Return False

            For i As Integer = 0 To arr.GetLength(0) - 1
                Dim cell = arr(i, 0)
                If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then
                    values.Add("")
                Else
                    values.Add(Convert.ToString(cell).Trim())
                End If
            Next
            Return True
        End Function

        Public Function TryReadNumericMatrix(v As Object, ByRef mat As Double(,), ByRef rows As Integer, ByRef cols As Integer) As Boolean
            mat = Nothing : rows = 0 : cols = 0
            Dim arr As Object(,) = Get2D(v)
            If arr Is Nothing Then Return False
            rows = arr.GetLength(0)
            cols = arr.GetLength(1)
            If rows < 1 OrElse cols < 1 Then Return False

            ' Detect a header row (any non-numeric in first row and numeric below).
            Dim header As Boolean = False
            If rows >= 2 Then
                Dim anyNonNum As Boolean = False
                For j As Integer = 0 To cols - 1
                    Dim d = TryGetDouble(arr(0, j))
                    If Not d.HasValue Then
                        anyNonNum = True
                        Exit For
                    End If
                Next
                If anyNonNum Then
                    Dim belowNumeric As Boolean = True
                    For j As Integer = 0 To cols - 1
                        Dim d = TryGetDouble(arr(1, j))
                        If Not d.HasValue Then
                            belowNumeric = False
                            Exit For
                        End If
                    Next
                    header = belowNumeric
                End If
            End If

            Dim startRow As Integer = If(header, 1, 0)
            Dim outRows As Integer = rows - startRow
            If outRows < 1 Then Return False

            mat = New Double(outRows - 1, cols - 1) {}
            For i As Integer = 0 To outRows - 1
                For j As Integer = 0 To cols - 1
                    Dim d As Double? = TryGetDouble(arr(startRow + i, j))
                    If d.HasValue AndAlso Not Double.IsNaN(d.Value) AndAlso Not Double.IsInfinity(d.Value) Then
                        mat(i, j) = d.Value
                    Else
                        mat(i, j) = Double.NaN
                    End If
                Next
            Next

            rows = outRows
            Return True
        End Function

        ''' <summary>
        ''' Inverts a square matrix using the shared regression-model matrix inversion routine.
        ''' </summary>
        Public Function TryInvertMatrix(a As Double(,), ByRef inv As Double(,)) As Boolean
            inv = Nothing
            If a Is Nothing Then Return False
            If a.Rank <> 2 Then Return False

            Dim nRows As Integer = a.GetLength(0)
            Dim nCols As Integer = a.GetLength(1)
            If nRows <> nCols OrElse nRows = 0 Then Return False

            Try
                Dim iErr As Integer = 0
                inv = Global.BESHStatNG.Matrix.Matrix.MatInv(a, "LU", iErr, False)

                If iErr <> 0 OrElse inv Is Nothing Then
                    inv = Nothing
                    Return False
                End If

                If inv.GetLength(0) <> nRows OrElse inv.GetLength(1) <> nCols Then
                    inv = Nothing
                    Return False
                End If

                Return True

            Catch
                inv = Nothing
                Return False
            End Try
        End Function


    End Module

End Namespace
