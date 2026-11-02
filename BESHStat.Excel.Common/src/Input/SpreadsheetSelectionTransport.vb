Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum SpreadsheetAreaAlignment
        RequireIdenticalRows = 0
        PhysicalIntersection = 1
    End Enum

    ''' <summary>One original cell; coordinates zero mean genuinely unavailable.</summary>
    Public NotInheritable Class SpreadsheetTransportCoordinate
        Friend Sub New(area As Integer, selectedRow As Integer, selectedColumn As Integer,
                       physicalRow As Integer, physicalColumn As Integer, source As SpreadsheetRangeData)
            AreaIndex = area
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            WorksheetRow = physicalRow
            WorksheetColumn = physicalColumn
            WorkbookName = If(source.WorkbookName, String.Empty)
            WorksheetName = If(source.WorksheetName, String.Empty)
            WorksheetId = If(source.WorksheetId, String.Empty)
            Address = If(source.Address, String.Empty)
        End Sub
        Public ReadOnly Property AreaIndex As Integer
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
        Public ReadOnly Property WorkbookName As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property WorksheetId As String
        Public ReadOnly Property Address As String
    End Class

    ''' <summary>Detached frame and explicit origin sidecar. Joined logical columns are
    ''' never represented as an invented contiguous worksheet rectangle.</summary>
    Public NotInheritable Class SpreadsheetTransportFrame
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly snapshot As SpreadsheetRangeData
        Private ReadOnly origins As SpreadsheetRangeData()
        Private ReadOnly rows As Integer()
        Private ReadOnly sourceRows As Integer(,)
        Private ReadOnly areaColumns As Integer()
        Private ReadOnly localColumns As Integer()

        Friend Sub New(data As SpreadsheetRangeData, areas As SpreadsheetRangeData(), rowIds As Integer(),
                       selectedRows As Integer(,), columnAreas As Integer(), columnOffsets As Integer())
            snapshot = SpreadsheetSelectionTransport.Copy(data)
            origins = areas.Select(Function(a) SpreadsheetSelectionTransport.Copy(a)).ToArray()
            rows = CType(rowIds.Clone(), Integer())
            sourceRows = CType(selectedRows.Clone(), Integer(,))
            areaColumns = CType(columnAreas.Clone(), Integer())
            localColumns = CType(columnOffsets.Clone(), Integer())
        End Sub
        Public ReadOnly Property Data As SpreadsheetRangeData
            Get
                Return SpreadsheetSelectionTransport.Copy(snapshot)
            End Get
        End Property
        Public ReadOnly Property RowIds As IReadOnlyList(Of Integer)
            Get
                Return Array.AsReadOnly(rows)
            End Get
        End Property
        Public ReadOnly Property Areas As IReadOnlyList(Of SpreadsheetRangeData)
            Get
                Return Array.AsReadOnly(origins.Select(Function(a) SpreadsheetSelectionTransport.Copy(a)).ToArray())
            End Get
        End Property
        Public Function SourceAt(rowIndex As Integer, columnIndex As Integer) As SpreadsheetTransportCoordinate
            If rowIndex < 0 OrElse rowIndex >= rows.Length OrElse columnIndex < 0 OrElse columnIndex >= areaColumns.Length Then Throw New ArgumentOutOfRangeException()
            Dim area = areaColumns(columnIndex), column = localColumns(columnIndex)
            Return New SpreadsheetTransportCoordinate(area, sourceRows(rowIndex, area), column + 1, rows(rowIndex),
                If(origins(area).StartColumn > 0, origins(area).StartColumn + column, 0), origins(area))
        End Function

        Public Function ToCoreDataTable(names As String(), Optional policy As SpreadsheetCellPolicyOptions = Nothing) As Global.BESHStatNG.CoreDataTable
            If rows.Any(Function(r) r <= 0) Then Throw New ArgumentException("Physical row provenance is required for this Core bridge.")
            Dim issues As New List(Of SpreadsheetValidationIssue)
            Dim options = If(policy, New SpreadsheetCellPolicyOptions())
            For r = 0 To snapshot.RowCount - 1
                For c = 0 To snapshot.ColumnCount - 1
                    Try
                        Dim cell = SpreadsheetCellValuePolicy.Evaluate(snapshot.Rows(r)(c), options)
                        If cell.IsError AndAlso options.Errors = SpreadsheetErrorPolicy.Reject Then Throw New ArgumentException("A source error cell is not permitted.")
                    Catch ex As ArgumentException
                        Dim source = SourceAt(r, c)
                        issues.Add(New SpreadsheetValidationIssue("transport.cell", SpreadsheetValidationSeverity.Error, ex.Message,
                            "ranges[" & source.AreaIndex.ToString(Globalization.CultureInfo.InvariantCulture) & "].cells",
                            source.SelectedRow, source.SelectedColumn, source.WorksheetRow, source.WorksheetColumn))
                    End Try
                Next
            Next
            If issues.Count > 0 Then Throw New SpreadsheetInputValidationException(New SpreadsheetValidationReport(issues))
            Dim result = SpreadsheetCoreTableBridge.ToCoreDataTable(snapshot, names, policy)
            result.RowIds = CType(rows.Clone(), Integer())
            result.FirstSourceRow = rows(0)
            result.SourceInfo.FirstSourceRow = rows(0)
            result.FirstSourceColumn = snapshot.StartColumn
            result.SourceInfo.FirstSourceColumn = snapshot.StartColumn
            Return result
        End Function
    End Class

    ''' <summary>Canonical structural transport. Interpretation remains in existing
    ''' typed parsers/cell policies; no numeric conversion, header guessing or row filtering.</summary>
    Public NotInheritable Class SpreadsheetSelectionTransport
        Private Sub New()
        End Sub

        Public Shared Function Capture(input As SpreadsheetRangeData, Optional rowIds As IReadOnlyList(Of Integer) = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If Not Shape(input, issues, "input") Then Return Result(Nothing, issues)
            Dim rows = If(rowIds Is Nothing, Enumerable.Range(0, input.RowCount).Select(Function(r) If(input.StartRow > 0, input.StartRow + r, 0)).ToArray(), rowIds.ToArray())
            If rows.Length <> input.RowCount OrElse rows.Any(Function(r) r < 0) OrElse
                (rows.Any(Function(r) r > 0) AndAlso (rows.Any(Function(r) r = 0) OrElse rows.Distinct().Count() <> rows.Length)) Then
                Add(issues, "transport.row-map", "Row identities must match the frame and be unique positive rows, or all unknown.", "rowIds")
                Return Result(Nothing, issues)
            End If
            Dim selected(input.RowCount - 1, 0) As Integer
            For r = 0 To input.RowCount - 1
                selected(r, 0) = r + 1
            Next
            Return Result(New SpreadsheetTransportFrame(input, {input}, rows, selected, New Integer(input.ColumnCount - 1) {}, Enumerable.Range(0, input.ColumnCount).ToArray()), issues)
        End Function

        Public Shared Function JoinColumns(input As SpreadsheetRangeSetData, alignment As SpreadsheetAreaAlignment,
                                           Optional limits As InputSizeLimits = Nothing) As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            If input Is Nothing OrElse input.Ranges Is Nothing OrElse input.Ranges.Count = 0 Then
                Add(issues, "transport.ranges", "At least one captured area is required.", "ranges")
                Return Result(Nothing, issues)
            End If
            If Not [Enum].IsDefined(GetType(SpreadsheetAreaAlignment), alignment) Then
                Add(issues, "transport.alignment", "An explicit supported physical-row alignment is required.", "alignment")
                Return Result(Nothing, issues)
            End If
            Dim budget As New InputAcquisitionBudget(limits)
            For i = 0 To input.Ranges.Count - 1
                Dim area = input.Ranges(i)
                If Shape(area, issues, "ranges[" & i.ToString(Globalization.CultureInfo.InvariantCulture) & "]") Then
                    Try
                        budget.AddRange(area.RowCount, area.ColumnCount)
                    Catch ex As SpreadsheetInputValidationException
                        Return New SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)(Nothing, ex.Report)
                    End Try
                    If area.StartRow <= 0 OrElse area.StartColumn <= 0 OrElse String.IsNullOrWhiteSpace(area.WorkbookName) OrElse String.IsNullOrWhiteSpace(area.WorksheetName) Then
                        Add(issues, "transport.identity", "A physical join requires captured workbook, worksheet and positive coordinates.", "ranges", area)
                    End If
                End If
            Next
            If issues.Count > 0 Then Return Result(Nothing, issues)
            Dim first = input.Ranges(0)
            For i = 1 To input.Ranges.Count - 1
                Dim area = input.Ranges(i)
                If Not SameSource(first, area) OrElse first.DateSystem <> area.DateSystem OrElse Not Nullable.Equals(first.DateSerialOrigin, area.DateSerialOrigin) Then
                    Add(issues, "transport.identity", "Areas do not have the same captured workbook/worksheet/date context.", "ranges", area)
                End If
            Next
            If issues.Count > 0 Then Return Result(Nothing, issues)
            Dim start = input.Ranges.Max(Function(a) a.StartRow)
            Dim finish = input.Ranges.Min(Function(a) a.StartRow + a.RowCount - 1)
            If alignment = SpreadsheetAreaAlignment.RequireIdenticalRows AndAlso input.Ranges.Any(Function(a) a.StartRow <> first.StartRow OrElse a.RowCount <> first.RowCount) Then
                Add(issues, "transport.row-alignment", "Selected areas have different physical rows; ordinal pairing is forbidden.", "ranges")
                Return Result(Nothing, issues)
            End If
            If finish < start Then
                Add(issues, "transport.no-common-rows", "Selections have no common physical worksheet rows.", "ranges")
                Return Result(Nothing, issues)
            End If
            Dim occupied As New HashSet(Of Integer)
            Dim columnAreas As New List(Of Integer), columnOffsets As New List(Of Integer)
            For i = 0 To input.Ranges.Count - 1
                Dim area = input.Ranges(i)
                For c = 0 To area.ColumnCount - 1
                    If Not occupied.Add(area.StartColumn + c) Then Add(issues, "transport.duplicate-cell", "Overlapping/repeated selected cells are not independent columns.", "ranges", area)
                    columnAreas.Add(i) : columnOffsets.Add(c)
                Next
                If area.StartRow <> start OrElse area.RowCount <> finish - start + 1 Then
                    Add(issues, "transport.rows-excluded", "Only explicitly requested common physical rows are retained; original areas remain in the sidecar.", "ranges", area, SpreadsheetValidationSeverity.Warning)
                End If
            Next
            If issues.Any(Function(i) i.Severity = SpreadsheetValidationSeverity.Error) Then Return Result(Nothing, issues)
            Dim rowIds = Enumerable.Range(start, finish - start + 1).ToArray()
            Dim selected(rowIds.Length - 1, input.Ranges.Count - 1) As Integer
            Dim data As New SpreadsheetRangeData With {.RowCount = rowIds.Length, .ColumnCount = columnAreas.Count,
                .WorkbookName = first.WorkbookName, .WorkbookId = first.WorkbookId, .WorksheetName = first.WorksheetName,
                .WorksheetId = first.WorksheetId, .DateSystem = first.DateSystem, .DateSerialOrigin = first.DateSerialOrigin,
                .SelectionId = input.SelectionId, .AddressDialect = SpreadsheetAddressDialect.Unknown}
            For r = 0 To rowIds.Length - 1
                Dim cells As New List(Of SpreadsheetCellValue)
                For a = 0 To input.Ranges.Count - 1
                    selected(r, a) = rowIds(r) - input.Ranges(a).StartRow + 1
                Next
                For c = 0 To columnAreas.Count - 1
                    cells.Add(CloneCell(input.Ranges(columnAreas(c)).Rows(selected(r, columnAreas(c)) - 1)(columnOffsets(c))))
                Next
                data.Rows.Add(cells)
            Next
            Return Result(New SpreadsheetTransportFrame(data, input.Ranges.ToArray(), rowIds, selected, columnAreas.ToArray(), columnOffsets.ToArray()), issues)
        End Function

        Private Shared Function SameSource(a As SpreadsheetRangeData, b As SpreadsheetRangeData) As Boolean
            Return String.Equals(a.WorkbookName, b.WorkbookName, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(a.WorksheetName, b.WorksheetName, StringComparison.OrdinalIgnoreCase) AndAlso
                String.Equals(If(a.WorkbookId, ""), If(b.WorkbookId, ""), StringComparison.Ordinal) AndAlso
                String.Equals(If(a.WorksheetId, ""), If(b.WorksheetId, ""), StringComparison.Ordinal) AndAlso
                String.Equals(If(a.SelectionRevision, ""), If(b.SelectionRevision, ""), StringComparison.Ordinal)
        End Function

        Private Shared Function Shape(input As SpreadsheetRangeData, issues As List(Of SpreadsheetValidationIssue), path As String) As Boolean
            If input Is Nothing OrElse input.RowCount < 1 OrElse input.ColumnCount < 1 OrElse input.StartRow < 0 OrElse input.StartColumn < 0 OrElse
                input.Rows Is Nothing OrElse input.Rows.Count <> input.RowCount OrElse
                input.Rows.Any(Function(r) r Is Nothing OrElse r.Count <> input.ColumnCount) Then
                Add(issues, "transport.shape", "A complete rectangular snapshot with valid coordinates is required.", path)
                Return False
            End If
            If CLng(input.StartRow) + input.RowCount - 1 > Integer.MaxValue OrElse CLng(input.StartColumn) + input.ColumnCount - 1 > Integer.MaxValue Then
                Add(issues, "transport.coordinates", "Source coordinates overflow the transport boundary.", path)
                Return False
            End If
            Return True
        End Function

        Private Shared Sub Add(issues As List(Of SpreadsheetValidationIssue), code As String, message As String, path As String,
                              Optional source As SpreadsheetRangeData = Nothing, Optional severity As SpreadsheetValidationSeverity = SpreadsheetValidationSeverity.Error)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, path,
                worksheetRow:=If(source Is Nothing, 0, source.StartRow), worksheetColumn:=If(source Is Nothing, 0, source.StartColumn)))
        End Sub

        Private Shared Function Result(data As SpreadsheetTransportFrame, issues As IEnumerable(Of SpreadsheetValidationIssue)) As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)
            Return New SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)(data, New SpreadsheetValidationReport(issues))
        End Function

        Friend Shared Function CloneCell(cell As SpreadsheetCellValue) As SpreadsheetCellValue
            If cell Is Nothing Then Return Nothing
            Return New SpreadsheetCellValue With {.Kind = cell.Kind, .NumberValue = cell.NumberValue, .TextValue = cell.TextValue,
                .BooleanValue = cell.BooleanValue, .NativeErrorCode = cell.NativeErrorCode, .NumberFormat = cell.NumberFormat}
        End Function

        Friend Shared Function Copy(input As SpreadsheetRangeData) As SpreadsheetRangeData
            Return New SpreadsheetRangeData With {.RowCount = input.RowCount, .ColumnCount = input.ColumnCount,
                .StartRow = input.StartRow, .StartColumn = input.StartColumn, .WorkbookName = input.WorkbookName,
                .WorkbookId = input.WorkbookId, .WorksheetName = input.WorksheetName, .WorksheetId = input.WorksheetId,
                .Address = input.Address, .AddressDialect = input.AddressDialect, .SelectionId = input.SelectionId,
                .SelectionRevision = input.SelectionRevision, .DateSystem = input.DateSystem, .DateSerialOrigin = input.DateSerialOrigin,
                .Rows = input.Rows.Select(Function(r) r.Select(Function(c) CloneCell(c)).ToList()).ToList()}
        End Function
    End Class
End Namespace
