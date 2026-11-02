Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Value-only input roles; address resolution and control state belong to the host.</summary>
    Public NotInheritable Class UnivariateInputRoles
        Friend Sub New(firstRequired As Boolean, singleColumn As Boolean)
            Me.FirstInputRequired = firstRequired
            Me.InputsMustBeSingleColumn = singleColumn
        End Sub
        Public ReadOnly Property FirstInputRequired As Boolean
        Public ReadOnly Property InputsMustBeSingleColumn As Boolean
    End Class

    ''' <summary>Immutable capture of one legacy-imported column; no DataObj or host state retained.</summary>
    Public NotInheritable Class UnivariateImportedColumn
        Friend Sub New(name As String, values As IEnumerable(Of Double), sources As IEnumerable(Of UnivariateObservationSource), hasData As Boolean)
            Me.Name = name
            Me.Values = New ReadOnlyCollection(Of Double)(values.ToList())
            Me.Sources = New ReadOnlyCollection(Of UnivariateObservationSource)(sources.ToList())
            Me.HasData = hasData
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property Values As IReadOnlyList(Of Double)
        Public ReadOnly Property Sources As IReadOnlyList(Of UnivariateObservationSource)
        Public ReadOnly Property HasData As Boolean
    End Class

    ''' <summary>One accepted observation's source, separate from GroupedNumericData.
    ''' Coordinates are one-based; zero means unavailable. Strings are captured host metadata.
    ''' </summary>
    Public NotInheritable Class UnivariateObservationSource
        Public Sub New(groupIndex As Integer, observationIndex As Integer, selectedRow As Integer,
                       selectedColumn As Integer, worksheetRow As Integer, worksheetColumn As Integer,
                       workbookId As String, worksheetId As String, worksheetName As String, address As String)
            Me.GroupIndex = groupIndex
            Me.ObservationIndex = observationIndex
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            Me.WorksheetRow = worksheetRow
            Me.WorksheetColumn = worksheetColumn
            Me.WorkbookId = If(workbookId, String.Empty)
            Me.WorksheetId = If(worksheetId, String.Empty)
            Me.WorksheetName = If(worksheetName, String.Empty)
            Me.Address = If(address, String.Empty)
        End Sub
        Public ReadOnly Property GroupIndex As Integer
        Public ReadOnly Property ObservationIndex As Integer
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
        Public ReadOnly Property WorkbookId As String
        Public ReadOnly Property WorksheetId As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property Address As String
    End Class

    Public NotInheritable Class UnivariateSelectionResult
        Friend Sub New(data As GroupedNumericData, validation As SpreadsheetValidationReport,
                       sources As IEnumerable(Of UnivariateObservationSource),
                       Optional sourceErrors As IEnumerable(Of SpreadsheetFrameCell) = Nothing)
            Me.Data = data
            Me.Validation = validation
            Me.Sources = New ReadOnlyCollection(Of UnivariateObservationSource)(sources.ToList())
            Me.SourceErrors = New ReadOnlyCollection(Of SpreadsheetFrameCell)(
                If(sourceErrors, Enumerable.Empty(Of SpreadsheetFrameCell)()).ToList())
        End Sub
        Public ReadOnly Property Data As GroupedNumericData
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Sources As IReadOnlyList(Of UnivariateObservationSource)
        ''' <summary>Lossless immutable error-cell metadata, including errors explicitly skipped.
        ''' Legacy DataObj imports have already normalized host errors and cannot recover it.
        ''' </summary>
        Public ReadOnly Property SourceErrors As IReadOnlyList(Of SpreadsheetFrameCell)
        Public ReadOnly Property Success As Boolean
            Get
                Return Data IsNot Nothing AndAlso Validation.IsValid
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Univariate input-role selection and validation. Explicit raw snapshots compose the
    ''' existing configurable parser. Imported compatibility entry points reuse DataObj's
    ''' established cleaning/grouping; they intentionally do not impose raw-parser defaults.
    ''' </summary>
    Public NotInheritable Class UnivariateSelectionPolicy
        Private Sub New()
        End Sub

        Public Shared Function InputRoles(twoSample As Boolean, byId As Boolean) As UnivariateInputRoles
            Return New UnivariateInputRoles(twoSample OrElse byId, twoSample OrElse byId)
        End Function

        ''' <summary>
        ''' groupIdentifiers, when present, is a separate single-column snapshot aligned to
        ''' the single-column data snapshot. Otherwise the explicit parser column roles apply.
        ''' physicalRowIds optionally supplies original physical rows for a compacted snapshot.
        ''' </summary>
        Public Shared Function Parse(data As SpreadsheetRangeData, options As SpreadsheetGroupedParseOptions,
                                     Optional groupIdentifiers As SpreadsheetRangeData = Nothing,
                                     Optional physicalRowIds As IReadOnlyList(Of Integer) = Nothing,
                                     Optional groupPhysicalRowIds As IReadOnlyList(Of Integer) = Nothing) As UnivariateSelectionResult
            Dim issues As New List(Of SpreadsheetValidationIssue)()
            If options Is Nothing Then
                Return Failure("options.invalid", "Explicit univariate parsing options are required.", "options")
            End If
            Dim policy As SpreadsheetCellPolicyOptions = options.CellPolicy
            If policy IsNot Nothing Then
                If Not [Enum].IsDefined(GetType(SpreadsheetMissingTextPolicy), policy.MissingText) OrElse
                   Not [Enum].IsDefined(GetType(SpreadsheetErrorPolicy), policy.Errors) OrElse
                   Not [Enum].IsDefined(GetType(SpreadsheetNumericTextPolicy), policy.NumericText) Then
                    Return Failure("options.invalid", "The cell interpretation options are invalid.", "options.cellPolicy")
                End If
                Try
                    policy.GetNumericCulture()
                Catch ex As ArgumentException
                    Return Failure("options.invalid", "The numeric culture is invalid.", "options.cellPolicy.numericCulture")
                End Try
            End If
            If data IsNot Nothing AndAlso (data.StartRow < 0 OrElse data.StartColumn < 0) Then
                Return Failure("range.shape", "Source coordinates must be nonnegative.", "source")
            End If
            If physicalRowIds IsNot Nothing Then
                If data Is Nothing OrElse physicalRowIds.Count <> data.RowCount OrElse
                   physicalRowIds.Where(Function(row, index) row < 1 OrElse (index > 0 AndAlso row <= physicalRowIds(index - 1))).Any() Then
                    Return Failure("source.row-ids", "Source rows must match the snapshot and be positive and strictly increasing.", "source.rowIds")
                End If
            End If
            Dim input As SpreadsheetRangeData = data
            If groupPhysicalRowIds IsNot Nothing AndAlso groupIdentifiers Is Nothing Then
                Return Failure("roles.rows", "Group source rows require a group-identifier snapshot.", "roles")
            End If
            If groupIdentifiers IsNot Nothing Then
                If options.Orientation <> SpreadsheetGroupedOrientation.LongFormat OrElse
                   options.CategoryColumnIndex <> 0 OrElse options.ValueColumnIndex <> 1 Then
                    Return Failure("options.long-columns", "Separate ranges require long-format category role 0 and value role 1.", "options.columns")
                End If
                If Not IsColumn(data) OrElse Not IsColumn(groupIdentifiers) Then
                    Return Failure("roles.shape", "Data and group identifiers must each be one rectangular column.", "roles")
                End If
                If Not SameIdentity(data.WorkbookId, groupIdentifiers.WorkbookId) OrElse
                   Not SameIdentity(data.WorkbookName, groupIdentifiers.WorkbookName) OrElse
                   Not SameIdentity(data.WorksheetId, groupIdentifiers.WorksheetId) OrElse
                   Not SameIdentity(data.WorksheetName, groupIdentifiers.WorksheetName) Then
                    Return Failure("roles.worksheet", "Data and group identifiers must be on the same worksheet.", "roles")
                End If
                If data.RowCount <> groupIdentifiers.RowCount OrElse data.StartRow <> groupIdentifiers.StartRow Then
                    Return Failure("roles.rows", "Data and group identifiers must have matching source rows.", "roles")
                End If
                If groupIdentifiers.StartRow < 0 OrElse groupIdentifiers.StartColumn < 0 Then
                    Return Failure("range.shape", "Source coordinates must be nonnegative.", "source")
                End If
                If groupPhysicalRowIds IsNot Nothing Then
                    Dim expected As IEnumerable(Of Integer) = physicalRowIds
                    If expected Is Nothing AndAlso data.StartRow > 0 Then expected = Enumerable.Range(data.StartRow, data.RowCount)
                    If expected Is Nothing OrElse Not groupPhysicalRowIds.SequenceEqual(expected) Then
                        Return Failure("roles.rows", "Data and group identifiers must have matching physical source rows.", "roles")
                    End If
                End If
                input = New SpreadsheetRangeData With {
                    .RowCount = data.RowCount, .ColumnCount = 2, .StartRow = data.StartRow,
                    .WorkbookId = data.WorkbookId, .WorksheetId = data.WorksheetId,
                    .WorksheetName = data.WorksheetName, .Address = data.Address}
                For r As Integer = 0 To data.RowCount - 1
                    input.Rows.Add(New List(Of SpreadsheetCellValue) From {groupIdentifiers.Rows(r)(0), data.Rows(r)(0)})
                Next
            End If
            Dim parsed As SpreadsheetGroupedParseResult = ConfigurableGroupedNumericParser.Parse(input, options)
            For Each issue As SpreadsheetValidationIssue In parsed.Validation.Issues
                Dim origin As SpreadsheetRangeData = data
                Dim selectedColumn As Integer = issue.SelectedColumn
                If groupIdentifiers IsNot Nothing AndAlso selectedColumn > 0 Then
                    origin = If(selectedColumn = 1, groupIdentifiers, data)
                    selectedColumn = 1
                End If
                Dim physicalRow As Integer = issue.WorksheetRow
                If issue.SelectedRow > 0 AndAlso physicalRowIds IsNot Nothing Then physicalRow = physicalRowIds(issue.SelectedRow - 1)
                Dim physicalColumn As Integer = issue.WorksheetColumn
                If groupIdentifiers IsNot Nothing AndAlso selectedColumn > 0 Then
                    physicalColumn = If(origin.StartColumn > 0, origin.StartColumn, 0)
                End If
                issues.Add(New SpreadsheetValidationIssue(issue.Code, issue.Severity, issue.Message, issue.FieldPath,
                    issue.SelectedRow, selectedColumn, physicalRow, physicalColumn))
            Next
            Dim sources As New List(Of UnivariateObservationSource)()
            Dim sourceErrors As New List(Of SpreadsheetFrameCell)()
            CaptureErrors(data, physicalRowIds, sourceErrors)
            If groupIdentifiers IsNot Nothing Then CaptureErrors(groupIdentifiers, physicalRowIds, sourceErrors)
            If parsed.Success Then
                For Each source As UnivariateObservationSource In parsed.ObservationSources
                    Dim column As Integer = If(groupIdentifiers Is Nothing, source.SelectedColumn, 1)
                    Dim row As Integer = If(physicalRowIds Is Nothing, source.WorksheetRow, physicalRowIds(source.SelectedRow - 1))
                    sources.Add(New UnivariateObservationSource(source.GroupIndex, source.ObservationIndex,
                        source.SelectedRow, column, row, If(data.StartColumn > 0, data.StartColumn + column - 1, 0),
                        data.WorkbookId, data.WorksheetId, data.WorksheetName, data.Address))
                Next
            End If
            Return New UnivariateSelectionResult(parsed.Data, New SpreadsheetValidationReport(issues), sources, sourceErrors)
        End Function

        ''' <summary>Preserves the form's per-column import, independent missing rows and names.
        ''' No new finite/group-size rules are imposed on the existing Windows route.
        ''' </summary>
        Public Shared Function SelectImportedColumns(columns As IReadOnlyList(Of Global.BESHStatNG.DataObj)) As UnivariateSelectionResult
            If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
            Return SelectCapturedColumns(columns.Select(AddressOf CaptureImportedColumn).ToArray())
        End Function

        ''' <summary>Capture immediately after each import to preserve conversion/exception timing
        ''' and avoid retaining all of the importer's raw matrices during acquisition.</summary>
        Public Shared Function CaptureImportedColumn(column As Global.BESHStatNG.DataObj) As UnivariateImportedColumn
            If column Is Nothing Then Throw New ArgumentNullException(NameOf(column))
            If column.FinalData Is Nothing Then
                Return New UnivariateImportedColumn(Nothing, Array.Empty(Of Double)(), Array.Empty(Of UnivariateObservationSource)(), False)
            End If
            Dim values As Double() = Global.BESHStatNG.DataManagement.ArrayUtilities.GetColumn(column.DataDbl, 0)
            Dim sources As New List(Of UnivariateObservationSource)()
            For i As Integer = 0 To values.Length - 1
                sources.Add(ImportedSource(column, 0, i, i, 1))
            Next
            Return New UnivariateImportedColumn(column.varNames(0), values, sources, True)
        End Function

        Public Shared Function SelectCapturedColumns(columns As IReadOnlyList(Of UnivariateImportedColumn)) As UnivariateSelectionResult
            If columns Is Nothing Then Throw New ArgumentNullException(NameOf(columns))
            Dim names As New List(Of String)()
            Dim groups As New List(Of Double())()
            Dim sources As New List(Of UnivariateObservationSource)()
            For Each column As UnivariateImportedColumn In columns
                If column Is Nothing Then Throw New ArgumentException("An imported column cannot be null.", NameOf(columns))
                If Not column.HasData Then Continue For
                Dim groupIndex As Integer = groups.Count
                groups.Add(column.Values.ToArray())
                names.Add(column.Name)
                For Each source As UnivariateObservationSource In column.Sources
                    sources.Add(New UnivariateObservationSource(groupIndex, source.ObservationIndex,
                        source.SelectedRow, source.SelectedColumn, source.WorksheetRow, source.WorksheetColumn,
                        source.WorkbookId, source.WorksheetId, source.WorksheetName, source.Address))
                Next
            Next
            If groups.Count = 0 Then Return Failure("groups.no-valid-data", "No valid data", "data")
            Return Valid(New GroupedNumericData(names.ToArray(), groups.ToArray()), sources)
        End Function

        ''' <summary>Reuses Core's Object.Equals grouping and current-culture conversion verbatim.
        ''' This compatibility entry point expects the existing two-column cleaned import.
        ''' </summary>
        Public Shared Function SelectImportedById(imported As Global.BESHStatNG.DataObj) As UnivariateSelectionResult
            If imported Is Nothing Then Throw New ArgumentNullException(NameOf(imported))
            Dim values As Double()() = imported.DataByID2ByColumn()
            Dim ids As Object() = Global.BESHStatNG.DataManagement.ArrayUtilities.GetColumn(imported.FinalData, 0).Distinct().ToArray()
            Dim names As String() = Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(ids)
            Dim sources As New List(Of UnivariateObservationSource)()
            For g As Integer = 0 To ids.Length - 1
                Dim observation As Integer = 0
                For r As Integer = 0 To imported.FinalData.GetLength(0) - 1
                    If Object.Equals(ids(g), imported.FinalData(r, 0)) Then
                        sources.Add(ImportedSource(imported, g, observation, r, 2))
                        observation += 1
                    End If
                Next
            Next
            Return Valid(New GroupedNumericData(names, values), sources)
        End Function

        Private Shared Function ImportedSource(imported As Global.BESHStatNG.DataObj, group As Integer,
                                               observation As Integer, rowIndex As Integer, column As Integer) As UnivariateObservationSource
            Dim source As Global.BESHStatNG.DataSourceInfo = imported.SourceInfo
            Dim physicalRow As Integer = If(imported.RowIds IsNot Nothing AndAlso rowIndex < imported.RowIds.Length, imported.RowIds(rowIndex), 0)
            Dim selectedRow As Integer = If(physicalRow > 0 AndAlso source IsNot Nothing AndAlso source.FirstSourceRow > 0,
                                            physicalRow - source.FirstSourceRow + 1, 0)
            ' The legacy importer does not capture workbook IDs or physical source columns.
            Return New UnivariateObservationSource(group, observation, selectedRow, column, physicalRow, 0,
                Nothing, Nothing, If(source Is Nothing, Nothing, source.SheetName), If(source Is Nothing, Nothing, source.Address))
        End Function

        Private Shared Function Valid(data As GroupedNumericData, sources As IEnumerable(Of UnivariateObservationSource)) As UnivariateSelectionResult
            Return New UnivariateSelectionResult(data, New SpreadsheetValidationReport(Array.Empty(Of SpreadsheetValidationIssue)()), sources)
        End Function
        Private Shared Function Failure(code As String, message As String, field As String) As UnivariateSelectionResult
            Return New UnivariateSelectionResult(Nothing, New SpreadsheetValidationReport({
                New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.[Error], message, field)}),
                Array.Empty(Of UnivariateObservationSource)())
        End Function
        Private Shared Function IsColumn(input As SpreadsheetRangeData) As Boolean
            Return input IsNot Nothing AndAlso input.ColumnCount = 1 AndAlso input.RowCount > 0 AndAlso
                input.Rows IsNot Nothing AndAlso input.Rows.Count = input.RowCount AndAlso
                input.Rows.All(Function(row) row IsNot Nothing AndAlso row.Count = 1)
        End Function
        Private Shared Function SameIdentity(first As String, second As String) As Boolean
            Return String.Equals(If(first, String.Empty), If(second, String.Empty), StringComparison.Ordinal)
        End Function
        Private Shared Sub CaptureErrors(input As SpreadsheetRangeData, physicalRows As IReadOnlyList(Of Integer),
                                         errors As List(Of SpreadsheetFrameCell))
            If input Is Nothing OrElse input.Rows Is Nothing Then Return
            For r As Integer = 0 To input.Rows.Count - 1
                If input.Rows(r) Is Nothing Then Continue For
                For c As Integer = 0 To input.Rows(r).Count - 1
                    Dim cell As SpreadsheetCellValue = input.Rows(r)(c)
                    If cell IsNot Nothing AndAlso cell.Kind = SpreadsheetCellKind.CellError Then
                        errors.Add(New SpreadsheetFrameCell(cell, r + 1, c + 1,
                            If(physicalRows IsNot Nothing AndAlso r < physicalRows.Count, physicalRows(r),
                               If(input.StartRow > 0, input.StartRow + r, 0)),
                            If(input.StartColumn > 0, input.StartColumn + c, 0)))
                    End If
                Next
            Next
        End Sub
    End Class
End Namespace
