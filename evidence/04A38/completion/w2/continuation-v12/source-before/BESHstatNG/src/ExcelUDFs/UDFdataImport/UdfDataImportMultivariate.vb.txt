Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

' Multivariate data import helpers for worksheet UDFs.
' Keeps correspondence-analysis and multiple-correspondence-analysis UDFs routed through the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports a non-negative integer contingency table for simple correspondence analysis.
    ''' Allows the same optional header-row and label-name behavior as the previous local CA importer.
    ''' </summary>
    Friend Function TryGetCorrespondenceInput(input As Object,
                                              rowNamesArg As Object,
                                              colNamesArg As Object,
                                              ByRef table(,) As Integer,
                                              ByRef rowNames() As String,
                                              ByRef colNames() As String) As Boolean
        table = Nothing
        rowNames = Nothing
        colNames = Nothing

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim hasHeader As Boolean = HasNumericMatrixHeader(arr, lastRow)

        Dim raw(,) As Double = Nothing
        Dim rows As Integer = 0
        Dim bodyCols As Integer = 0
        If Not TryReadNumericMatrix(input, raw, rows, bodyCols) Then Return False
        If rows < 1 OrElse bodyCols < 1 Then Return False

        ReDim table(rows - 1, bodyCols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To bodyCols - 1
                Dim x As Double = raw(i, j)
                If Double.IsNaN(x) OrElse Double.IsInfinity(x) OrElse x < 0.0R Then Return False
                Dim rounded As Double = Math.Round(x)
                If Math.Abs(x - rounded) > 0.0000001R Then Return False
                table(i, j) = CInt(rounded)
            Next
        Next

        Dim inferredCols(bodyCols - 1) As String
        For j As Integer = 0 To bodyCols - 1
            inferredCols(j) = "Col " & (j + 1).ToString(CultureInfo.InvariantCulture)
            If hasHeader AndAlso Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, j)) Then
                inferredCols(j) = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j))
            End If
        Next

        rowNames = ResolveImportLabelNames(rowNamesArg, rows, "Row ")
        colNames = ResolveImportLabelNames(colNamesArg, bodyCols, "Col ", inferredCols)
        Return True
    End Function

    ''' <summary>
    ''' Imports a categorical observation-by-variable matrix for multiple correspondence analysis.
    ''' A header row may be inferred or controlled by hasHeaderArg; explicit varNames override inferred headers.
    ''' </summary>
    Friend Function TryGetCategoricalMatrix(input As Object,
                                            varNames As Object,
                                            hasHeaderArg As Object,
                                            ByRef data(,) As String,
                                            ByRef resolvedNames() As String) As Boolean
        data = Nothing
        resolvedNames = Nothing

        Dim arr As Object(,) = Get2D(input)
        If arr Is Nothing Then Return False

        Dim cols As Integer = arr.GetLength(1)
        If cols < 1 Then Return False

        Dim lastRow As Integer = FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim explicitNames As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(varNames)
        Dim assumeHeader As Boolean = If(Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(hasHeaderArg),
                                         Not explicitNames,
                                         Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.GetOptionalBool(hasHeaderArg, Not explicitNames))
        Dim startRow As Integer = If(assumeHeader, 1, 0)
        Dim usedRows As Integer = lastRow + 1
        Dim rows As Integer = usedRows - startRow
        If rows < 1 Then Return False

        Dim inferred(cols - 1) As String
        For j As Integer = 0 To cols - 1
            inferred(j) = "Variable " & (j + 1).ToString(CultureInfo.InvariantCulture)
            If assumeHeader AndAlso Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(0, j)) Then
                inferred(j) = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j))
            End If
        Next
        resolvedNames = ResolveImportLabelNames(varNames, cols, "Variable ", inferred)

        ReDim data(rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                data(i, j) = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(startRow + i, j))
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' Imports a one-dimensional text vector from a worksheet argument for multivariate UDF options.
    ''' Accepts a delimited string or a one-row / one-column worksheet range.
    ''' </summary>
    Friend Function TryGetStringVector(arg As Object, ByRef values() As String) As Boolean
        values = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts = s.Split({","c, ";"c, ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length > 0 Then
                ReDim values(parts.Length - 1)
                For i As Integer = 0 To parts.Length - 1
                    values(i) = parts(i).Trim()
                Next
                Return True
            End If
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim list As New List(Of String)()
        If rows = 1 Then
            For j As Integer = 0 To cols - 1
                list.Add(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j)))
            Next
        ElseIf cols = 1 Then
            For i As Integer = 0 To rows - 1
                list.Add(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(i, 0)))
            Next
        Else
            Return False
        End If

        values = list.ToArray()
        Return values.Length > 0
    End Function

    ''' <summary>
    ''' Imports a one-dimensional finite numeric vector from a worksheet argument for multivariate UDF options.
    ''' Accepts a delimited string or a one-row / one-column worksheet range.
    ''' </summary>
    Friend Function TryGetDoubleVector(arg As Object, ByRef values() As Double) As Boolean
        values = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim separators() As Char
            If s.IndexOf(";"c) >= 0 OrElse s.IndexOf(ControlChars.Lf) >= 0 OrElse s.IndexOf(ControlChars.Cr) >= 0 Then
                separators = New Char() {";"c, ControlChars.Lf, ControlChars.Cr}
            Else
                separators = New Char() {","c}
            End If

            Dim parts = s.Split(separators, StringSplitOptions.RemoveEmptyEntries)
            Dim list As New List(Of Double)()
            For Each token As String In parts
                Dim parsed As Double
                If Double.TryParse(token.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, parsed) OrElse
                   Double.TryParse(token.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then
                    list.Add(parsed)
                Else
                    Return False
                End If
            Next

            If list.Count > 0 Then
                values = list.ToArray()
                Return True
            End If
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim vals As New List(Of Double)()
        If rows = 1 Then
            For j As Integer = 0 To cols - 1
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(0, j))
                If Not d.HasValue Then Return False
                vals.Add(d.Value)
            Next
        ElseIf cols = 1 Then
            For i As Integer = 0 To rows - 1
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(arr(i, 0))
                If Not d.HasValue Then Return False
                vals.Add(d.Value)
            Next
        Else
            Return False
        End If

        values = vals.ToArray()
        Return values.Length > 0
    End Function

    Private Function ResolveImportLabelNames(arg As Object,
                                             expectedCount As Integer,
                                             fallbackPrefix As String,
                                             Optional inferred() As String = Nothing) As String()
        Dim fallback(expectedCount - 1) As String
        For i As Integer = 0 To expectedCount - 1
            If inferred IsNot Nothing AndAlso i < inferred.Length AndAlso Not String.IsNullOrWhiteSpace(inferred(i)) Then
                fallback(i) = inferred(i)
            Else
                fallback(i) = fallbackPrefix & (i + 1).ToString(CultureInfo.InvariantCulture)
            End If
        Next

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return fallback

        Dim s As String = TryCast(arg, String)
        If s IsNot Nothing Then
            Dim parts = s.Split({","c}, StringSplitOptions.None)
            If parts.Length = expectedCount Then
                For i As Integer = 0 To expectedCount - 1
                    Dim name As String = parts(i).Trim()
                    fallback(i) = If(String.IsNullOrWhiteSpace(name), fallback(i), name)
                Next
                Return fallback
            End If
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return fallback

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        Dim names As New List(Of String)()

        If rows = 1 AndAlso cols >= 1 Then
            For j As Integer = 0 To cols - 1
                names.Add(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j)))
            Next
        ElseIf cols = 1 AndAlso rows >= 1 Then
            For i As Integer = 0 To rows - 1
                names.Add(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(i, 0)))
            Next
        End If

        If names.Count = expectedCount Then
            For i As Integer = 0 To expectedCount - 1
                If Not String.IsNullOrWhiteSpace(names(i)) Then fallback(i) = names(i)
            Next
        End If

        Return fallback
    End Function

End Module
