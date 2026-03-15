Option Explicit On
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel

Friend Module SheetManagement
    ''' <summary>
    ''' Returns the maximum number of rows available in the specified Excel worksheet.
    ''' </summary>
    ''' <param name="ws">
    ''' The <see cref="Worksheet"/> object representing the Excel sheet to query.
    ''' </param>
    ''' <returns>
    ''' The maximum number of rows supported by the worksheet.  
    ''' Returns 65,536 if the query fails (legacy XLS limit).
    ''' </returns>
    ''' <remarks>
    ''' - For modern XLSX files, Excel supports up to 1,048,576 rows.  
    ''' - This function queries the count of rows in column A.  
    ''' - If an error occurs (e.g., worksheet not accessible), it defaults to 65,536.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: get maximum rows in active sheet
    ''' Dim ws As Worksheet = Globals.ThisWorkbook.ActiveSheet
    ''' Dim maxRows As Integer = MaxRowsInSheet(ws)
    ''' Console.WriteLine("Max rows: " + maxRows)
    ''' </example>
    Public Function MaxRowsInSheet(ws As Worksheet) As Integer
        Try
            MaxRowsInSheet = ws.Range("A:A").Rows.Count ' modern Excel: 1,048,576
        Catch
            MaxRowsInSheet = 65536 ' legacy Excel (XLS) limit
        End Try
        Return MaxRowsInSheet
    End Function


    ''' <summary>
    ''' Finds the last non-empty column in the specified Excel worksheet.
    ''' </summary>
    ''' <param name="ws">
    ''' The <see cref="Worksheet"/> object representing the Excel sheet to query.
    ''' </param>
    ''' <returns>
    ''' The index of the last column that contains data in the worksheet.
    ''' </returns>
    ''' <remarks>
    ''' - The search is limited to the first 500 rows to avoid empty headers.  
    ''' - For legacy XLS files (65,536 rows, 255 columns), the maximum column index is 255.  
    ''' - For modern XLSX files (1,048,576 rows, 16,384 columns), the maximum column index is 16,384.  
    ''' - If an error occurs when querying XLSX limits, the function falls back to the XLS limit.  
    ''' - The worksheet is activated before scanning.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: get the last column with data in active sheet
    ''' Dim ws As Worksheet = Globals.ThisWorkbook.ActiveSheet
    ''' Dim lastCol As Integer = LastColumnInSheet(ws)
    ''' Console.WriteLine("Last column with data: " + lastCol)
    ''' </example>
    Public Function LastColumnInSheet(ws As Worksheet) As Integer
        Dim i As Integer, lTemp As Integer, FinalCol As Integer, MaxRows As Integer
        Const rowsToCheck As Integer = 500

        ws.Activate()
        MaxRows = MaxRowsInSheet(ws)
        If MaxRows = 65536 Then 'xls
            For i = 1 To rowsToCheck
                lTemp = ws.Cells(i, 255).End(XlDirection.xlToLeft).Column '255 is max amount of columns in xls
                If lTemp > FinalCol Then
                    FinalCol = lTemp
                End If
            Next
        Else 'xlsx
            For i = 1 To rowsToCheck
                Try
                    lTemp = ws.Cells(i, 16384).End(XlDirection.xlToLeft).Column '16384 is max amount of columns in xlsx
                Catch
                    lTemp = ws.Cells(i, 255).End(XlDirection.xlToLeft).Column
                End Try
                If lTemp > FinalCol Then FinalCol = lTemp
            Next
        End If

        Return FinalCol
    End Function


    ''' <summary>
    ''' Counts the number of non-missing cells in a specified Excel range.
    ''' </summary>
    ''' <param name="r">
    ''' The <see cref="Range"/> object representing the cells to evaluate.
    ''' </param>
    ''' <param name="bNumeric_only">
    ''' If <c>True</c>, counts only numeric values using <c>WorksheetFunction.Count</c>.  
    ''' If <c>False</c>, counts all non-empty cells using <c>WorksheetFunction.CountA</c>.
    ''' </param>
    ''' <returns>
    ''' The number of non-missing cells in the range, either numeric-only or all non-empty depending on <paramref name="bNumeric_only"/>.
    ''' </returns>
    ''' <remarks>
    ''' - <c>Count</c> counts only numeric values (ignores text, logicals, errors).  
    ''' - <c>CountA</c> counts all non-empty cells (including text, logicals, errors).  
    ''' - Useful for quickly determining data completeness in a worksheet range.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: count numeric values in column A
    ''' Dim rng As Range = ws.Range("A1:A100")
    ''' Dim nNumeric As Integer = CountNonmissing(rng, True)
    ''' 
    ''' ' Example: count all non-empty cells in column B
    ''' Dim rng2 As Range = ws.Range("B1:B100")
    ''' Dim nAll As Integer = CountNonmissing(rng2, False)
    ''' </example>
    Public Function CountNonmissing(r As Range, bNumeric_only As Boolean) As Integer
        If bNumeric_only Then
            Return BESHstatGlobals.app.WorksheetFunction.Count(r)
        Else
            Return BESHstatGlobals.app.WorksheetFunction.CountA(r)
        End If
    End Function


    ''' <summary>
    ''' Returns the column letter of the specified Excel range.
    ''' </summary>
    ''' <param name="rng">
    ''' The <see cref="Range"/> object representing the Excel cell or range.
    ''' </param>
    ''' <returns>
    ''' A string containing the column letter corresponding to the range address.
    ''' </returns>
    ''' <remarks>
    ''' - Uses the address of <paramref name="rng"/> anchored to row 1 to extract the column letter.  
    ''' - Example: if <paramref name="rng"/> is in column 5, the function returns "E".  
    ''' - Useful for userform initialization or reporting where column letters are needed instead of indices.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: get the column letter of a cell
    ''' Dim ws As Worksheet = Globals.ThisWorkbook.ActiveSheet
    ''' Dim rng As Range = ws.Range("D10")
    ''' Dim colLetter As String = ColName(rng)
    ''' ' colLetter = "D"
    ''' Console.WriteLine("Column letter: " + colLetter)
    ''' </example>
    Public Function ColName(rng As Range) As String
        Return Left(rng.Range("A1").Address(True, False), InStr(1, rng.Range("A1").Address(True, False), "$", 1) - 1)
    End Function


    ''' <summary>
    ''' Converts a given column number into its corresponding Excel column letter reference.
    ''' </summary>
    ''' <param name="ColumnNumber">
    ''' The numeric index of the column (1-based).  
    ''' Example: 1 → "A", 26 → "Z", 27 → "AA".
    ''' </param>
    ''' <returns>
    ''' A string containing the Excel column letter corresponding to <paramref name="ColumnNumber"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Uses the <see cref="Range.Address"/> property of the active sheet to extract the column letter.  
    ''' - Works for both legacy XLS (max 255 columns) and modern XLSX (max 16,384 columns).  
    ''' - Useful for converting numeric indices into human-readable Excel references.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert column numbers to letters
    ''' Dim colA As String = ColNumber2Letter(1)   ' Returns "A"
    ''' Dim colZ As String = ColNumber2Letter(26)  ' Returns "Z"
    ''' Dim colAA As String = ColNumber2Letter(27) ' Returns "AA"
    ''' Console.WriteLine(colA + ", " + colZ + ", " + colAA)
    ''' </example>
    Public Function ColNumber2Letter(ColumnNumber As Integer) As String
        Dim ColumnLetter As String = String.Empty
        Try
            If BESHstatGlobals.app.ActiveSheet.Type = XlSheetType.xlWorksheet Then
                ColumnLetter = Split(BESHstatGlobals.app.ActiveSheet.Cells(1, ColumnNumber).Address, "$")(1)
            Else
                For Each ws As Worksheet In BESHstatGlobals.app.ActiveWorkbook.Worksheets
                    If ws.Type = XlSheetType.xlWorksheet Then
                        ColumnLetter = Split(BESHstatGlobals.app.ActiveSheet.Cells(1, ColumnNumber).Address, "$")(1)
                        Exit For
                    End If
                Next ws
            End If

        Catch
        End Try
        Return ColumnLetter
    End Function


    ''' <summary>
    ''' Converts an Excel column letter into its corresponding numeric index.
    ''' </summary>
    ''' <param name="InputLetter">
    ''' The column letter to convert (e.g., "A", "Z", "AA", "ZZ").  
    ''' Case-insensitive.
    ''' </param>
    ''' <returns>
    ''' The numeric index of the column (1-based).  
    ''' Example: "A" → 1, "Z" → 26, "AA" → 27, "ZZ" → 702.
    ''' </returns>
    ''' <remarks>
    ''' - Works for both legacy XLS (max 255 columns) and modern XLSX (max 16,384 columns).  
    ''' - Uses ASCII conversion to calculate the numeric index.  
    ''' - Useful for round-trip conversion with <c>ColNumber2Letter</c>.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: convert column letters to numbers
    ''' Dim colA As Integer = ColumnLetterToNumber("A")   ' Returns 1
    ''' Dim colZ As Integer = ColumnLetterToNumber("Z")   ' Returns 26
    ''' Dim colAA As Integer = ColumnLetterToNumber("AA") ' Returns 27
    ''' Dim colZZ As Integer = ColumnLetterToNumber("ZZ") ' Returns 702
    ''' Console.WriteLine(colA + ", " + colZ + ", " + colAA + ", " + colZZ)
    ''' </example>
    Public Function ColumnLetterToNumber(InputLetter As Object) As Integer
        Dim OutputNumber As Integer
        For i = 1 To Len(InputLetter)
            OutputNumber = (Asc(UCase$(Mid$(InputLetter, i, 1))) - 64) + OutputNumber * 26
        Next
        Return OutputNumber
    End Function

    ''' <summary>
    ''' Extracts the worksheet name from a reference string and returns the corresponding <see cref="Worksheet"/> object.
    ''' </summary>
    ''' <param name="strAddr">
    ''' A reference string containing a worksheet name and cell/range reference (e.g., "Sheet1!A1", "'My Sheet'!B2").
    ''' </param>
    ''' <returns>
    ''' The <see cref="Worksheet"/> object corresponding to the extracted worksheet name.  
    ''' Returns <c>Nothing</c> if the reference string is invalid.
    ''' </returns>
    ''' <remarks>
    ''' - Handles worksheet names with spaces or special characters enclosed in apostrophes.  
    ''' - Throws <see cref="ApplicationException"/> if the reference string is invalid or unrecognized.  
    ''' - Uses <c>app.ActiveWorkbook.Worksheets</c> to resolve the worksheet object.  
    ''' - Logs errors via <c>gLogger</c> for unexpected conditions.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: get worksheet from reference string
    ''' Dim ws As Worksheet = WorksheetFromRefAdress("Sheet1!A1")
    ''' Console.WriteLine("Worksheet name: " + ws.Name)
    ''' 
    ''' Dim ws2 As Worksheet = WorksheetFromRefAdress("'Data Sheet'!B10")
    ''' Console.WriteLine("Worksheet name: " + ws2.Name)
    ''' </example>
    Function WorksheetFromRefAdress(strAddr As String, Optional WB As Excel.Workbook = Nothing) As Worksheet
        Dim nIndex As Integer, wks As String = String.Empty, countExclm As Integer
        WorksheetFromRefAdress = Nothing
        'If the name of the worksheet contains a single apostrophe, there are spaces in the name.
        'The apostrophe must be added back prior to removing the sheet name from the reference.

        'Try to extract worksheet name from the RefEditValue and create a worksheet object
        If strAddr = String.Empty Or InStr(1, strAddr, "!") = 0 Then
            BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("Cannot get worksheet name. Probably an invalid reference string. strAddr=" & strAddr))
        End If

        countExclm = Len(strAddr) - Len(Replace(strAddr, "!", String.Empty))

        If InStr(1, strAddr, "'!") > 0 Then 'worksheet name containing space or other special characters
            nIndex = InStr(1, strAddr, "'!")
            wks = Mid$(strAddr, 2, nIndex - 2)
        ElseIf countExclm > 0 And InStr(1, strAddr, "'") = 0 Then
            nIndex = InStr(1, strAddr, "!")
            wks = Left$(strAddr, nIndex - 1)
        ElseIf countExclm > 1 And InStr(1, strAddr, "'") = 0 Then
            'we should be having apostroph if name contains !
            BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("This should not happen .condition 3.. Unrecognized reference range adress string = " & strAddr))
        ElseIf countExclm > 1 And InStr(1, strAddr, "'") > 0 Then
            'This should not happen. In this case string should contain "'!" which is our first condition.
            BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("This should not happen .condition 4.. Unrecognized reference range adress string = " & strAddr))
        Else
            BESHstatGlobals.BSerr.LogAndThrow(New ApplicationException("This should not happen .else condition.. Unrecognized reference range adress string = " & strAddr))
        End If

        If WB Is Nothing Then
            Return BESHstatGlobals.app.ActiveWorkbook.Worksheets(wks)
        Else
            Return WB.Worksheets(wks)
        End If

    End Function

    ''' <summary>
    ''' Creates an Excel column reference string based on a variable name and its mapping in a dictionary.
    ''' </summary>
    ''' <param name="ws">
    ''' The <see cref="Worksheet"/> object representing the Excel sheet.
    ''' </param>
    ''' <param name="var">
    ''' The variable name to look up in <paramref name="VarList"/>.
    ''' </param>
    ''' <param name="VarList">
    ''' A dictionary mapping column numbers to metadata arrays.  
    ''' The fourth element (<c>colinfo(3)</c>) is expected to contain the variable name.
    ''' </param>
    ''' <returns>
    ''' A string containing the Excel column reference (e.g., "$A:$A") corresponding to the variable name.  
    ''' Returns <c>String.Empty</c> if the variable name is not found.
    ''' </returns>
    ''' <remarks>
    ''' - Iterates through <paramref name="VarList"/> to find the column number associated with <paramref name="var"/>.  
    ''' - Uses <c>Range.AddressLocal</c> to extract the column letter.  
    ''' - Constructs a reference string in the form "$ColLetter:$ColLetter".  
    ''' - Useful for building dynamic references in regression userforms or other automation tasks.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: create a reference for variable "Age"
    ''' Dim dict As New Dictionary(Of Integer, Object())
    ''' dict.Add(2, New Object() {"meta1", "meta2", "meta3", "Age"})
    ''' Dim ws As Worksheet = Globals.ThisWorkbook.ActiveSheet
    ''' Dim refStr As String = CreateReference(ws, "Age", dict)
    ''' ' refStr = "$B:$B"
    ''' Console.WriteLine(refStr)
    ''' </example>
    Function CreateReference(ws As Worksheet, var As String, VarList As Dictionary(Of String, VarColumnInfo)) As String

        Dim info As VarColumnInfo = Nothing
        If Not VarList.TryGetValue(CStr(var), info) OrElse info Is Nothing Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Variable not found in VarList: '{var}'."))
        End If

        Dim col As String = info.ColumnLetter
        Return "$" & col & ":$" & col
    End Function

    ''' <summary>
    ''' Build a comma-separated Excel reference string in the form:
    '''   'SheetName'!$A:$A, $B:$B, $C:$C
    ''' from a sequence of variable keys (listbox item strings).
    ''' </summary>
    Public Function BuildExcelRefList(ws As Worksheet, varKeys As IEnumerable(Of String),
                                      varList As Dictionary(Of String, VarColumnInfo),
                                      Optional skipEmpty As Boolean = True) As String

        If ws Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(ws)))
        If varKeys Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(varKeys)))
        If varList Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(varList)))

        Dim parts As New List(Of String)

        For Each kObj As String In varKeys
            Dim k As String = If(kObj, String.Empty)
            If skipEmpty AndAlso k.Trim() = String.Empty Then Continue For
            parts.Add(CreateReference(ws, k, varList))
        Next

        If parts.Count = 0 Then
            Return String.Empty
        End If

        'Only the first reference is sheet-qualified (Excel accepts the rest as relative to it)
        Dim ref As String = "'" & ws.Name & "'!" & parts(0)

        For i As Integer = 1 To parts.Count - 1
            ref &= ", " & parts(i)
        Next

        Return ref
    End Function

    ''' <summary>
    ''' Extracts the worksheet name from a reference string and optionally includes apostrophes.
    ''' </summary>
    ''' <param name="strAddr">
    ''' A reference string containing a worksheet name and cell/range reference (e.g., "Sheet1!A1", "'Data Sheet'!B2").
    ''' </param>
    ''' <param name="bIncludeApos">
    ''' If <c>True</c>, returns the worksheet name wrapped in apostrophes if the original reference contained them.  
    ''' If <c>False</c>, returns the plain worksheet name without apostrophes.
    ''' </param>
    ''' <returns>
    ''' The worksheet name extracted from <paramref name="strAddr"/>.  
    ''' Returns the name with or without apostrophes depending on <paramref name="bIncludeApos"/>.
    ''' </returns>
    ''' <remarks>
    ''' - Calls <c>WorksheetFromRefAdress</c> to resolve the worksheet object.  
    ''' - Apostrophes are used in Excel references when worksheet names contain spaces or special characters.  
    ''' - Useful for building dynamic references or userform initialization.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: extract worksheet name from reference string
    ''' Dim wsName1 As String = WorksheetNameFromRefAdress("Sheet1!A1", False)
    ''' ' wsName1 = "Sheet1"
    ''' 
    ''' Dim wsName2 As String = WorksheetNameFromRefAdress("'Data Sheet'!B10", True)
    ''' ' wsName2 = "'Data Sheet'"
    ''' </example>
    Function WorksheetNameFromRefAdress(strAddr As String, bIncludeApos As Boolean, Optional WB As Excel.Workbook = Nothing) As String
        Dim strName As String

        If bIncludeApos Then
            If InStr(1, strAddr, "'") Then
                strName = "'" & WorksheetFromRefAdress(strAddr, WB).Name & "'"
            Else
                strName = WorksheetFromRefAdress(strAddr, WB).Name
            End If
        Else
            strName = WorksheetFromRefAdress(strAddr, WB).Name
        End If
        Return strName
    End Function

    ''' <summary>
    ''' Extracts a list of full column reference strings from a given Excel reference address.
    ''' </summary>
    ''' <param name="ref">
    ''' A reference string containing a worksheet name and range (e.g., "Sheet1!A:C", "'Data Sheet'!B2:D10").
    ''' </param>
    ''' <returns>
    ''' An array of strings, each representing a full column reference (e.g., "$A:$A", "$B:$B", "$C:$C").
    ''' </returns>
    ''' <remarks>
    ''' - Handles references that span multiple columns or multiple comma-separated ranges.  
    ''' - Expands each range into its constituent columns, returning full column references from the first row to the last row.  
    ''' - Uses <c>MaxRowsInSheet</c> to ensure the row limit matches the worksheet type (XLS vs. XLSX).  
    ''' - Calls <c>WorksheetNameFromRefAdress</c> and <c>WorksheetFromRefAdress</c> to resolve the worksheet context.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: extract column references from a range
    ''' Dim refs() As String = ColumListFromRefAdress("Sheet1!A:C")
    ''' ' refs = {"$A:$A", "$B:$B", "$C:$C"}
    ''' 
    ''' Dim refs2() As String = ColumListFromRefAdress("'Data Sheet'!B2:D10")
    ''' ' refs2 = {"$B:$B", "$C:$C", "$D:$D"}
    ''' </example>
    Function ColumListFromRefAdress(ref As String, Optional WB As Excel.Workbook = Nothing) As String()
        Dim wks As String, temp As Object, FullString As Object, ws As Worksheet, i As Integer, j As Integer
        Dim rRange As Range, strUpdatedAdrr As String, lastRow As Integer
        strUpdatedAdrr = String.Empty
        wks = WorksheetNameFromRefAdress(ref, True, WB) & "!"
        ws = WorksheetFromRefAdress(ref, WB)
        temp = Replace(Replace(ref, wks, String.Empty), "$", String.Empty) 'Remove "Sheet1!" and "$" from string

        FullString = Split(temp, ",") 'String reference
        For i = 0 To UBound(FullString)
            'Break the 1st range reference into two parts (column refs)
            If LTrim$(FullString(i)) <> String.Empty Then
                rRange = ws.Range(wks & FullString(i))
                For j = 1 To rRange.Columns.Count
                    lastRow = Math.Min(MaxRowsInSheet(ws), rRange.Row + rRange.Rows.Count - 1)
                    If i = 0 And j = 1 Then
                        strUpdatedAdrr = ws.Range(ws.Cells(rRange.Row, rRange(rRange.Row, j).Column),
                                              ws.Cells(lastRow, rRange(rRange.Row, j).Column)).AddressLocal
                    Else
                        strUpdatedAdrr = strUpdatedAdrr & ", " & ws.Range(ws.Cells(rRange.Row, rRange(rRange.Row, j).Column),
                                                                      ws.Cells(lastRow, rRange(rRange.Row, j).Column)).AddressLocal
                    End If
                Next j
            End If
        Next i
        Return Split(strUpdatedAdrr, ",")
    End Function

    ''' <summary>
    ''' Removes all non-digit characters from a string and returns only the numeric characters.
    ''' </summary>
    ''' <param name="str">
    ''' The input string that may contain digits and non-digit characters.
    ''' </param>
    ''' <returns>
    ''' A string consisting only of the digits extracted from <paramref name="str"/>.  
    ''' Returns <c>String.Empty</c> if no digits are found.
    ''' </returns>
    ''' <remarks>
    ''' - Uses <c>IsNumeric</c> to test each character.  
    ''' - Preserves the order of digits as they appear in the input string.  
    ''' - Useful for extracting numbers from mixed alphanumeric strings (e.g., "AB123CD" → "123").  
    ''' </remarks>
    ''' <example>
    ''' ' Example: remove non-digits from a string
    ''' Dim result1 As String = removeNonDigits("AB123CD")
    ''' ' result1 = "123"
    ''' 
    ''' Dim result2 As String = removeNonDigits("Phone: +421-987-654")
    ''' ' result2 = "421987654"
    ''' </example>
    Function removeNonDigits(str As String) As String
        Dim tmp As String = String.Empty
        For i = 1 To Len(str)
            If IsNumeric(Mid$(str, i, 1)) Then tmp += Mid$(str, i, 1)
        Next
        Return tmp
    End Function

    ''' <summary>
    ''' Expands a multi-column range reference into a standardized reference string
    ''' suitable for regression routines (e.g., RMANOVA, Hotelling's T²).
    ''' </summary>
    ''' <param name="strRefid">
    ''' A reference string containing a worksheet name and range (e.g., "Sheet1!A:C", "'Data Sheet'!B2:D10").
    ''' </param>
    ''' <returns>
    ''' A string containing the worksheet name followed by a comma-separated list of
    ''' fully qualified column references (e.g., "'Sheet1'!$A:$A, $B:$B, $C:$C").
    ''' </returns>
    ''' <remarks>
    ''' - Calls <c>WorksheetNameFromRefAdress</c> to resolve the worksheet name.  
    ''' - Calls <c>ColumListFromRefAdress</c> to expand the range into individual column references.  
    ''' - Useful for statistical routines that require explicit column references rather than multi-column ranges.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: expand a multi-column reference
    ''' Dim refStr As String = prepareRef2D("Sheet1!A:C")
    ''' ' refStr = "'Sheet1'!$A:$A, $B:$B, $C:$C"
    ''' 
    ''' Dim refStr2 As String = prepareRef2D("'Data Sheet'!B2:D10")
    ''' ' refStr2 = "'Data Sheet'!$B:$B, $C:$C, $D:$D"
    ''' </example>
    Function prepareRef2D(strRefid As String, Optional WB As Excel.Workbook = Nothing) As String
        'if one range of multiple columns (e.g. for RMANOVA, Hotelling's T2)then update it so we can use standard Reg rutines
        Dim ref As String = WorksheetNameFromRefAdress(strRefid, True, WB) & "!"
        Dim colList() As String = ColumListFromRefAdress(strRefid, WB)
        For i = 0 To UBound(colList)
            If i = 0 Then
                ref += LTrim$(colList(i))
            Else
                ref += ", " & LTrim$(colList(i))
            End If
        Next
        Return ref
    End Function
End Module
