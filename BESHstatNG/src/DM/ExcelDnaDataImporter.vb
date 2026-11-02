Option Explicit On
Option Strict Off
Option Infer On

Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Excel-DNA/Excel Interop adapter that imports worksheet ranges into the portable <see cref="CoreDataTable"/> model.
''' Keep Excel object-model code here rather than in <see cref="DataObj"/>.
''' </summary>
Public Class ExcelDnaDataImporter
    ' Weak-key metadata sidecar: no Excel objects, no public Core schema change.
    Private Shared ReadOnly physicalEvidence As New System.Runtime.CompilerServices.ConditionalWeakTable(Of CoreDataTable, Global.BESHStatNG.ExcelCommon.SpreadsheetTypedParseResult(Of Global.BESHStatNG.ExcelCommon.SpreadsheetTransportFrame))

    ''' <summary>Lossless snapshot; COM acquisition remains Windows-owned.</summary>
    Public Shared Function AcquireRangeData(source As Range) As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeData
        Return ExcelDnaSharedApplicationAdapter.ToRangeData(source)
    End Function

    Public Shared Function AcquireRangeSetData(source As Range) As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeSetData
        Return ExcelDnaSharedApplicationAdapter.ToRangeSetData(New Range() {source})
    End Function

    Public Shared Function AcquireRangeSetData(sources As IEnumerable(Of Range)) As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeSetData
        Return ExcelDnaSharedApplicationAdapter.ToRangeSetData(sources)
    End Function

    Public Shared Function AcquireRangeSetData(selection As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeSelection,
                                               book As Workbook) As Global.BESHStatNG.ExcelCommon.SpreadsheetRangeSetData
        Return ExcelDnaSharedApplicationAdapter.ToRangeSetData(selection, book)
    End Function

    Public Shared Function Import(ref As String, Optional bStartRow As Boolean = False) As CoreDataTable
        If String.IsNullOrWhiteSpace(ref) Then CoreServices.Errors.LogAndThrow(New ArgumentException("Excel range reference cannot be empty.", NameOf(ref)))

        Dim ws As Worksheet = WorksheetFromRefAdress(ref)
        If ws Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("Unable to resolve worksheet from reference: " & ref))

        Dim fullString() As String = ColumListFromRefAdress(ref)
        ' Owner-authorized 04A.27 physical-area switch; native obligations remain OPEN.
        If bStartRow Then Return ImportPhysicalAreas(ws, ref, fullString)
        Dim nCols As Integer = 0
        Dim nRows As Integer = 0
        Dim startRow As Integer = 1
        Dim testVal As Range = Nothing
        Dim maxRows As Integer

        With ws
            For i As Integer = 0 To UBound(fullString, 1)
                nCols += .Range(fullString(i)).Columns.Count
            Next
            If nCols <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Reference does not contain any columns: " & ref))

            testVal = .Range(fullString(0))
            maxRows = MaxRowsInSheet(ws)

            If testVal.Rows.Count = .Cells.Rows.Count Then
                nRows = testVal(maxRows, 1).End(XlDirection.xlUp).Row
            Else
                nRows = testVal.Rows.Count
            End If
            If nRows <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Reference does not contain any rows: " & ref))

            If bStartRow Then
                startRow = ResolveStartRow(fullString(0))
            Else
                startRow = 1
            End If

            Dim values(nRows - 1, nCols - 1) As Object
            Dim columnNames(nCols - 1) As String
            Dim currentCol As Integer = 0

            For areaIndex As Integer = 0 To UBound(fullString, 1)
                Dim rangeAddress As String = BuildConcreteRangeAddress(CStr(fullString(areaIndex)), startRow, nRows)
                testVal = .Range(rangeAddress)

                For k As Integer = 1 To testVal.Columns.Count
                    Dim headerValue As Object = NormalizeExcelValue(testVal(1, k).Value)
                    If headerValue Is Nothing Then
                        columnNames(currentCol) = "Var" & ColNumber2Letter(testVal.Column + k - 1)
                    Else
                        columnNames(currentCol) = CStr(headerValue)
                    End If

                    For rowIndex As Integer = 1 To nRows
                        values(rowIndex - 1, currentCol) = NormalizeExcelValue(testVal(rowIndex, k).Value)
                    Next
                    currentCol += 1
                Next
            Next

            Dim source As New DataSourceInfo With {
                .SourceKind = "WorksheetRange",
                .Address = ref,
                .SheetName = ws.Name,
                .FirstSourceRow = startRow,
                .ColumnNames = columnNames
            }

            Return CoreDataTable.FromObjectMatrix(values, columnNames, firstSourceRow:=startRow, sourceInfo:=source, copyValues:=False)
        End With
    End Function

    Private Shared Function ImportPhysicalAreas(ws As Worksheet, original As String, addresses As String()) As CoreDataTable
        Return ImportPhysicalSelection(original, addresses,
            Function(selected)
                Dim areas As New List(Of Range)
                For Each address As String In selected
                    ' Resolve each actual selected rectangle independently; never rewrite rows.
                    Dim area As Range = ws.Range(address)
                    ValidatePhysicalMergeState(area.MergeCells)
                    areas.Add(area)
                Next
                Return AcquireRangeSetData(areas)
            End Function)
    End Function

    ''' <summary>Operation-local acquisition boundary used by the shipping True path.
    ''' Tests supply snapshots; no global Excel object or mutable host override.</summary>
    Friend Shared Function ImportPhysicalSelection(original As String, addresses As IEnumerable(Of String),
        acquire As Func(Of IReadOnlyList(Of String), Global.BESHStatNG.ExcelCommon.SpreadsheetRangeSetData)) As CoreDataTable
        If addresses Is Nothing Then Throw New ArgumentNullException(NameOf(addresses))
        If acquire Is Nothing Then Throw New ArgumentNullException(NameOf(acquire))
        Dim selected = addresses.ToArray()
        If selected.Length = 0 OrElse selected.Any(Function(a) String.IsNullOrWhiteSpace(a)) Then Throw New ArgumentException("Selected area addresses are required.")
        Dim snapshots = acquire(Array.AsReadOnly(selected))
        Dim joined = Global.BESHStatNG.ExcelCommon.SpreadsheetSelectionTransport.JoinColumns(snapshots,
            Global.BESHStatNG.ExcelCommon.SpreadsheetAreaAlignment.PhysicalIntersection)
        If Not joined.Success Then Throw New Global.BESHStatNG.ExcelCommon.SpreadsheetInputValidationException(joined.Validation)
        Dim frame = joined.Data
        Dim first = frame.Data
        Dim names(first.ColumnCount - 1) As String
        For c As Integer = 0 To names.Length - 1
            Dim cell = first.Rows(0)(c)
            Select Case cell.Kind
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Text
                    names(c) = cell.TextValue
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Number
                    names(c) = cell.NumberValue.ToString(Globalization.CultureInfo.CurrentCulture)
                Case Global.BESHStatNG.ExcelCommon.SpreadsheetCellKind.Bool
                    names(c) = cell.BooleanValue.ToString()
                Case Else
                    names(c) = "Var" & Global.BESHStatNG.ExcelCommon.SpreadsheetRangeAddress.ColumnLetters(frame.SourceAt(0, c).WorksheetColumn)
            End Select
        Next
        Dim policy As New Global.BESHStatNG.ExcelCommon.SpreadsheetCellPolicyOptions With {
            .Errors = Global.BESHStatNG.ExcelCommon.SpreadsheetErrorPolicy.TreatAsMissing,
            .MissingText = Global.BESHStatNG.ExcelCommon.SpreadsheetMissingTextPolicy.BlankOrWhitespaceText,
            .NumericText = If(Globalization.CultureInfo.CurrentCulture.Equals(Globalization.CultureInfo.InvariantCulture),
                Global.BESHStatNG.ExcelCommon.SpreadsheetNumericTextPolicy.Invariant, Global.BESHStatNG.ExcelCommon.SpreadsheetNumericTextPolicy.ExplicitCulture),
            .NumericCultureName = Globalization.CultureInfo.CurrentCulture.Name}
        Dim result = frame.ToCoreDataTable(names, policy)
        result.SourceInfo.Address = original
        ' ImportInto transfers this very table (cloneTable=False), so both the
        ' original area/row sidecar and exclusion warnings remain retrievable.
        physicalEvidence.Add(result, joined)
        Return result
    End Function

    Friend Shared Sub ValidatePhysicalMergeState(merged As Object)
        If Not TypeOf merged Is Boolean OrElse CBool(merged) Then Throw New ArgumentException("Merged or unverifiable merge regions are not supported by this transport.")
    End Sub

    Friend Shared Function GetPhysicalImportEvidence(table As CoreDataTable) As Global.BESHStatNG.ExcelCommon.SpreadsheetTypedParseResult(Of Global.BESHStatNG.ExcelCommon.SpreadsheetTransportFrame)
        If table Is Nothing Then Return Nothing
        Dim evidence As Global.BESHStatNG.ExcelCommon.SpreadsheetTypedParseResult(Of Global.BESHStatNG.ExcelCommon.SpreadsheetTransportFrame) = Nothing
        If physicalEvidence.TryGetValue(table, evidence) Then Return evidence
        Return Nothing
    End Function

    Public Shared Sub ImportInto(target As DataObj,
                                 ref As String,
                                 Optional bStartRow As Boolean = False,
                                 Optional CharCols As Integer = -1,
                                 Optional SkipRow As Integer = 0)
        If target Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(target)))
        Dim table As CoreDataTable = Import(ref, bStartRow)
        target.LoadCoreDataTable(table, CharCols:=CharCols, SkipRow:=SkipRow, cloneTable:=False)
    End Sub

    Friend Shared Function NormalizeExcelValue(value As Object) As Object
        If value Is Nothing Then Return Nothing
        If TypeOf value Is ExcelEmpty OrElse TypeOf value Is ExcelMissing OrElse TypeOf value Is ExcelError Then Return Nothing
        Return value
    End Function

    Private Shared Function ResolveStartRow(rangeAddress As String) As Integer
        If InStr(rangeAddress, ":") = 0 Then
            Dim digits As String = removeNonDigits(CStr(rangeAddress))
            If digits = String.Empty Then Return 1
            Return CLng(digits)
        End If

        Dim parts As Object = Split(rangeAddress, ":")
        If UBound(parts) >= 1 Then
            Dim digits As String = removeNonDigits(CStr(parts(0)))
            If digits = String.Empty Then Return 1
            Return CLng(digits)
        End If

        Return 1
    End Function

    Private Shared Function BuildConcreteRangeAddress(rangeAddress As String, startRow As Integer, nRows As Integer) As String
        If InStr(rangeAddress, ":") = 0 Then Return rangeAddress

        Dim partString As Object = Split(rangeAddress, ":")
        Dim str1 As Object = Split(partString(0), StrReverse(Val(StrReverse(partString(0)))))
        Dim str2 As Object = Split(partString(1), StrReverse(Val(StrReverse(partString(1)))))
        Return CStr(str1(0)) & CStr(startRow) & ":" & CStr(str2(0)) & CStr(nRows + (startRow - 1))
    End Function
End Class
