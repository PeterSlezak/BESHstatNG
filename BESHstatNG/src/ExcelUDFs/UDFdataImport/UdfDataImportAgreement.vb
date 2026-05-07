Option Explicit On
Option Strict On

Imports System.Linq
Imports ExcelDna.Integration

' Agreement and method-comparison data import helpers for worksheet UDFs.
' Kept in a separate partial module so callers can use the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    Friend Function TryGetAlignedNumericWithOptionalCategory(x As Object,
                                                               y As Object,
                                                               category As Object,
                                                               requireCategory As Boolean) As (X As Double(), Y As Double(), Category As Object(), DetectedNames As String(), [Error] As ExcelError?)
        Dim ax As Object(,) = UDFhelpers.Get2D(x)
        Dim ay As Object(,) = UDFhelpers.Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim names() As String = {
                If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
                If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
            }

        Dim ac As Object(,) = Nothing
        Dim useCategory As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(category)
        If useCategory Then
            ac = UDFhelpers.Get2D(category)
            If ac Is Nothing OrElse ac.GetLength(1) <> 1 OrElse ac.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderC As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ac)
            If hasHeaderC <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If

        Dim xv As New List(Of Double)
        Dim yv As New List(Of Double)
        Dim cv As New List(Of Object)
        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim dx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ax(r, 0))
            Dim dy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ay(r, 0))
            If dx.HasValue AndAlso dy.HasValue Then
                xv.Add(dx.Value)
                yv.Add(dy.Value)
                If useCategory Then
                    Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ac(r, 0))
                    If s = "" Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    cv.Add(s)
                End If
            End If
        Next
        If requireCategory AndAlso Not useCategory Then Return (Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim catOut() As Object = If(useCategory, cv.ToArray(), Nothing)
        Return (xv.ToArray(), yv.ToArray(), catOut, names, Nothing)
    End Function

    Friend Function TryGetCategoryList(arg As Object, ByRef categories() As Object) As Boolean
        categories = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return False

        If TypeOf arg Is String Then
            Dim s As String = Convert.ToString(arg).Trim()
            If s = "" Then Return False
            Dim parts = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries).Select(Function(t) CType(t.Trim(), Object)).ToArray()
            If parts.Length = 0 Then Return False
            categories = parts
            Return True
        End If

        Dim arr As Object(,) = UDFhelpers.Get2D(arg)
        If arr Is Nothing Then Return False
        Dim vals As New List(Of Object)
        If arr.GetLength(0) = 1 Then
            For j As Integer = 0 To arr.GetLength(1) - 1
                Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(0, j))
                If s <> "" Then vals.Add(s)
            Next
        ElseIf arr.GetLength(1) = 1 Then
            For i As Integer = 0 To arr.GetLength(0) - 1
                Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(arr(i, 0))
                If s <> "" Then vals.Add(s)
            Next
        Else
            Return False
        End If
        If vals.Count = 0 Then Return False
        categories = vals.ToArray()
        Return True
    End Function

    Friend Function TryGetPairedCategoricalColumns(x As Object, y As Object) As (X As Object(), Y As Object(), DetectedNames As String(), [Error] As ExcelError?)
        Dim err As ExcelError? = Nothing
        Dim ax As Object(,) = UDFhelpers.Get2D(x)
        Dim ay As Object(,) = UDFhelpers.Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim names() As String = {
            If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Rater 1"),
            If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Rater 2")
        }
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim xs As New List(Of Object)
        Dim ys As New List(Of Object)
        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim sx As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ax(r, 0))
            Dim sy As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(ay(r, 0))
            If sx <> "" AndAlso sy <> "" Then
                xs.Add(sx)
                ys.Add(sy)
            End If
        Next
        If xs.Count = 0 Then Return (Nothing, Nothing, names, ExcelError.ExcelErrorNum)
        Return (xs.ToArray(), ys.ToArray(), names, Nothing)
    End Function

    Friend Function TryGetAlignedDemingInputs(x As Object, y As Object, sdX As Object, sdY As Object) As (X As Double(), Y As Double(), SDx As Double(), SDy As Double(), DetectedNames As String(), [Error] As ExcelError?)
        Dim ax As Object(,) = UDFhelpers.Get2D(x)
        Dim ay As Object(,) = UDFhelpers.Get2D(y)
        If ax Is Nothing OrElse ay Is Nothing Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(1) <> 1 OrElse ay.GetLength(1) <> 1 Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        If ax.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)

        Dim hasHeaderX As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ax)
        Dim hasHeaderY As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(ay)
        If hasHeaderX <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        Dim startRow As Integer = If(hasHeaderX, 1, 0)
        Dim names() As String = {
            If(hasHeaderX, Convert.ToString(ax(0, 0)).Trim(), "Reference"),
            If(hasHeaderY, Convert.ToString(ay(0, 0)).Trim(), "Test")
        }

        Dim useSdx As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(sdX)
        Dim useSdy As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(sdY)
        Dim asx As Object(,) = Nothing
        Dim asy As Object(,) = Nothing
        If useSdx Then
            asx = UDFhelpers.Get2D(sdX)
            If asx Is Nothing OrElse asx.GetLength(1) <> 1 OrElse asx.GetLength(0) <> ax.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderSdx As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(asx)
            If hasHeaderSdx <> hasHeaderX Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If
        If useSdy Then
            asy = UDFhelpers.Get2D(sdY)
            If asy Is Nothing OrElse asy.GetLength(1) <> 1 OrElse asy.GetLength(0) <> ay.GetLength(0) Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
            Dim hasHeaderSdy As Boolean = UDFhelpers.LooksLikeSingleColumnHeader(asy)
            If hasHeaderSdy <> hasHeaderY Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
        End If

        Dim xv As New List(Of Double)
        Dim yv As New List(Of Double)
        Dim sdxv As New List(Of Double)
        Dim sdyv As New List(Of Double)

        For r As Integer = startRow To ax.GetLength(0) - 1
            Dim dx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ax(r, 0))
            Dim dy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(ay(r, 0))
            If dx.HasValue AndAlso dy.HasValue Then
                xv.Add(dx.Value)
                yv.Add(dy.Value)
                If useSdx Then
                    Dim sx = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(asx(r, 0))
                    If Not sx.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    sdxv.Add(sx.Value)
                End If
                If useSdy Then
                    Dim sy = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(asy(r, 0))
                    If Not sy.HasValue Then Return (Nothing, Nothing, Nothing, Nothing, Nothing, ExcelError.ExcelErrorValue)
                    sdyv.Add(sy.Value)
                End If
            End If
        Next

        Dim sdxOut() As Double = If(useSdx, sdxv.ToArray(), Nothing)
        Dim sdyOut() As Double = If(useSdy, sdyv.ToArray(), Nothing)
        Return (xv.ToArray(), yv.ToArray(), sdxOut, sdyOut, names, Nothing)
    End Function


    Friend Function TryGetOneWayIccGroups(input As Object, ByRef groups()() As Double) As Boolean
        groups = Nothing
        Dim arr As Object(,) = UDFhelpers.Get2D(input)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Return False

        Dim lastRow As Integer = UDFhelpers.FindLastNonBlankRow(arr)
        If lastRow < 0 Then Return False

        Dim lastCol As Integer = FindLastNonBlankCol(arr, lastRow)
        If lastCol < 0 Then Return False

        Dim numericCols As Integer() = Enumerable.Range(0, lastCol + 1).ToArray()
        Dim hasHeader As Boolean = UDFhelpers.LooksLikeHeaderRow(arr, numericCols)
        Dim startRow As Integer = If(hasHeader, 1, 0)
        If startRow > lastRow Then Return False

        Dim out As New List(Of Double())
        For r As Integer = startRow To lastRow
            Dim rowVals As New List(Of Double)
            Dim sawAnyCell As Boolean = False
            For c As Integer = 0 To lastCol
                Dim cell As Object = arr(r, c)
                If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Continue For
                sawAnyCell = True
                Dim d As Double? = Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble(cell)
                If Not d.HasValue Then Return False
                rowVals.Add(d.Value)
            Next
            If sawAnyCell AndAlso rowVals.Count > 0 Then out.Add(rowVals.ToArray())
        Next

        If out.Count < 2 Then Return False
        groups = out.ToArray()
        Return True
    End Function

    Private Function FindLastNonBlankCol(arr As Object(,), lastRow As Integer) As Integer
        For c As Integer = arr.GetLength(1) - 1 To 0 Step -1
            For r As Integer = 0 To lastRow
                If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(arr(r, c)) Then Return c
            Next
        Next
        Return -1
    End Function

End Module
