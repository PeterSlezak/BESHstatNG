Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SpreadsheetGroupedValidationTests

    Private Shared Function Build(data As Object()(),
                                  Optional sourceRow As Integer = 0,
                                  Optional sourceColumn As Integer = 0) As SpreadsheetRangeData
        Dim rows As New List(Of List(Of SpreadsheetCellValue))()
        For Each values As Object() In data
            Dim cells As New List(Of SpreadsheetCellValue)()
            For Each value As Object In values
                If value Is Nothing Then
                    cells.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf value Is Boolean Then
                    cells.Add(SpreadsheetCellValue.BooleanCell(CBool(value)))
                ElseIf TypeOf value Is Double OrElse TypeOf value Is Integer Then
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                ElseIf TypeOf value Is SpreadsheetCellValue Then
                    cells.Add(DirectCast(value, SpreadsheetCellValue))
                Else
                    cells.Add(SpreadsheetCellValue.TextCell(CStr(value)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {
            .Rows = rows, .RowCount = rows.Count, .ColumnCount = If(rows.Count > 0, rows(0).Count, 0),
            .WorksheetName = "Study Data", .StartRow = sourceRow, .StartColumn = sourceColumn
        }
    End Function

    Private Shared Function GetIssue(result As SpreadsheetGroupedParseResult, code As String) As SpreadsheetValidationIssue
        Return result.Validation.Issues.FirstOrDefault(Function(issue) issue.Code = code)
    End Function

    <TestMethod>
    Public Sub LegacyPreset_MatchesUnchangedParser_AndGoldenAnova()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B", "C"},
            New Object() {1.0R, 2.0R, 5.0R},
            New Object() {2.0R, 3.0R, 6.0R},
            New Object() {3.0R, 4.0R, 7.0R}})
        Dim legacy As GroupedNumericData = GroupedNumericRangeParser.Parse(input)
        Dim configured As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, SpreadsheetGroupedParseOptions.LegacyWideHeaders())
        Assert.IsTrue(configured.Success)
        Assert.IsFalse(configured.Validation.HasErrors)
        CollectionAssert.AreEqual(legacy.GroupNames, configured.Data.GroupNames)
        CollectionAssert.AreEqual(legacy.GroupCounts, configured.Data.GroupCounts)
        For i As Integer = 0 To legacy.Groups.Length - 1
            CollectionAssert.AreEqual(legacy.Groups(i), configured.Data.Groups(i))
        Next
        Dim anova As OneWayAnovaApplicationResult = OneWayAnovaApplicationService.Run(configured.Data)
        Assert.AreEqual(13.0R, anova.FStatistic, 0.000000000001R)
        Assert.AreEqual(0.006591796875R, anova.PValue, 0.000000000001R)
    End Sub

    <TestMethod>
    Public Sub NoHeader_WideColumns_UsesGeneratedNamesAndIncludesFirstRow()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {1, 10}, New Object() {2, 11}, New Object() {3, 12}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {.Header = SpreadsheetHeaderMode.None})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"Group 1", "Group 2"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {1, 2, 3}, result.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub RowsWithHeaders_UseFirstColumnAndSkipMissing()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"Alpha", 1, 2, 3}, New Object() {"Beta", 5, Nothing, 7}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {.Orientation = SpreadsheetGroupedOrientation.GroupsInRows})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"Alpha", "Beta"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {5, 7}, result.Data.Groups(1))
    End Sub

    <TestMethod>
    Public Sub RowsWithoutHeaders_PreserveAllObservations()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {1, 2, 3}, New Object() {4, 5, 6}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {
                .Orientation = SpreadsheetGroupedOrientation.GroupsInRows,
                .Header = SpreadsheetHeaderMode.None})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"Group 1", "Group 2"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {4, 5, 6}, result.Data.Groups(1))
    End Sub

    <TestMethod>
    Public Sub LongFormat_Headers_KeepFirstAppearanceOrderAndRaggedGroups()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"Treatment", "Response"},
            New Object() {"B", 7}, New Object() {"A", 1},
            New Object() {"B", 8}, New Object() {"A", 2}, New Object() {"A", 3}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {.Orientation = SpreadsheetGroupedOrientation.LongFormat})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"B", "A"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Integer() {2, 3}, result.Data.GroupCounts)
        CollectionAssert.AreEqual(New Double() {7, 8}, result.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub LongFormat_AutoWithoutHeaderAndSwappedColumns()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {10, "East", "x"}, New Object() {20, "West", "y"},
            New Object() {11, "East", "z"}, New Object() {21, "West", "t"}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {
                .Orientation = SpreadsheetGroupedOrientation.LongFormat,
                .Header = SpreadsheetHeaderMode.Auto,
                .CategoryColumnIndex = 1, .ValueColumnIndex = 0})
        Assert.IsTrue(result.Success)
        Assert.IsNotNull(GetIssue(result, "header.inferred"))
        CollectionAssert.AreEqual(New String() {"East", "West"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {10, 11}, result.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub AutoHeader_DetectsTextLabelsWithoutDroppingNumericObservations()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"T", "C"}, New Object() {1, 3}, New Object() {2, 4}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"T", "C"}, result.Data.GroupNames)
        Assert.IsNotNull(GetIssue(result, "header.inferred"))
    End Sub

    <TestMethod>
    Public Sub AutoHeader_MixedFirstRowIsAmbiguousNotSilentlyDropped()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", 1}, New Object() {2, 3}, New Object() {4, 5}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(
            input, New SpreadsheetGroupedParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsFalse(result.Success)
        Assert.IsNotNull(GetIssue(result, "header.ambiguous"))
    End Sub

    <TestMethod>
    Public Sub Errors_ReportBothSelectedAndWorksheetCoordinates()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 2},
            New Object() {SpreadsheetCellValue.ErrorCell("#VALUE!"), 3},
            New Object() {4, 5}}, 8, 2)
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input)
        Assert.IsFalse(result.Success)
        Dim issue As SpreadsheetValidationIssue = GetIssue(result, "cell.excel-error")
        Assert.IsNotNull(issue)
        Assert.AreEqual(3, issue.SelectedRow)
        Assert.AreEqual(1, issue.SelectedColumn)
        Assert.AreEqual(10, issue.WorksheetRow)
        Assert.AreEqual(2, issue.WorksheetColumn)
        StringAssert.Contains(issue.FieldPath, "observations")
        Assert.AreEqual(SpreadsheetValidationSeverity.[Error], issue.Severity)
    End Sub

    <TestMethod>
    Public Sub MissingPolicy_ExplicitEmptyTextAndWarnings()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 4},
            New Object() {"", Nothing}, New Object() {2, 5}})
        Dim strict As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input)
        Assert.IsFalse(strict.Success)
        Assert.IsNotNull(GetIssue(strict, "cell.not-numeric"))
        Dim options As New SpreadsheetGroupedParseOptions With {
            .WarnOnSkippedObservations = True,
            .CellPolicy = New SpreadsheetCellPolicyOptions With {
                .MissingText = SpreadsheetMissingTextPolicy.BlankOrEmptyText}}
        Dim relaxed As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input, options)
        Assert.IsTrue(relaxed.Success)
        Assert.IsTrue(relaxed.Validation.HasWarnings)
        CollectionAssert.AreEqual(New Integer() {2, 2}, relaxed.Data.GroupCounts)
        Assert.IsNotNull(GetIssue(relaxed, "cell.missing-skipped"))
    End Sub

    <TestMethod>
    Public Sub NumericTextOnlyConvertedWithExplicitCulture()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {"2,5", 1},
            New Object() {"3,5", 2}})
        Assert.IsFalse(GroupedNumericRangeParser.ParseWithDiagnostics(input).Success)
        Dim options As New SpreadsheetGroupedParseOptions With {
            .CellPolicy = New SpreadsheetCellPolicyOptions With {
                .NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture, .NumericCultureName = "sk-SK"}}
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input, options)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2.5R, result.Data.Groups(0)(0), 0.0000001R)
    End Sub

    <TestMethod>
    Public Sub LongFormat_RejectsMissingCategoryAndTypedLabelCollisions()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"G", "Y"}, New Object() {"2", 1}, New Object() {2, 2},
            New Object() {Nothing, 3}, New Object() {"X", 4}, New Object() {"X", 5}})
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input,
            New SpreadsheetGroupedParseOptions With {.Orientation = SpreadsheetGroupedOrientation.LongFormat})
        Assert.IsFalse(result.Success)
        Assert.IsNotNull(GetIssue(result, "groups.category-collision"))
        Assert.IsNotNull(GetIssue(result, "groups.category-missing"))
    End Sub

    <TestMethod>
    Public Sub InvalidShapeAndColumnOptions_ReturnIssuesInsteadOfExceptions()
        Dim bad As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 2}})
        bad.ColumnCount = 3
        Assert.IsNotNull(GetIssue(GroupedNumericRangeParser.ParseWithDiagnostics(bad), "range.not-rectangular"))
        bad.ColumnCount = 2
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(bad,
            New SpreadsheetGroupedParseOptions With {
                .Orientation = SpreadsheetGroupedOrientation.LongFormat,
                .CategoryColumnIndex = 1, .ValueColumnIndex = 1})
        Assert.IsNotNull(GetIssue(result, "options.long-columns"))
    End Sub

    <TestMethod>
    Public Sub ConfiguredThrowingFacade_ExposesValidationReport()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 4}, New Object() {"bad", 5}})
        Dim ex As SpreadsheetInputValidationException = Assert.ThrowsException(Of SpreadsheetInputValidationException)(
            Function() GroupedNumericRangeParser.Parse(input, New SpreadsheetGroupedParseOptions()))
        Assert.IsTrue(ex.Report.HasErrors)
        Assert.IsNotNull(ex.Report.Issues.FirstOrDefault(Function(issue) issue.Code = "cell.not-numeric"))
    End Sub

    <TestMethod>
    Public Sub Diagnostics_AreImmutableAndDoNotMutateSourceCells()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 5}, New Object() {2, 6}})
        Dim original As SpreadsheetCellValue = input.Rows(1)(0)
        Dim result As SpreadsheetGroupedParseResult = GroupedNumericRangeParser.ParseWithDiagnostics(input)
        Assert.IsTrue(result.Success)
        Assert.AreSame(original, input.Rows(1)(0))
        Dim report As New SpreadsheetValidationReport(New SpreadsheetValidationIssue() {
            New SpreadsheetValidationIssue("test.issue", SpreadsheetValidationSeverity.Warning, "note", "header")})
        Dim collection As IList(Of SpreadsheetValidationIssue) = DirectCast(report.Issues, IList(Of SpreadsheetValidationIssue))
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() collection.Add(New SpreadsheetValidationIssue("x", SpreadsheetValidationSeverity.Warning, "x", "x")))
    End Sub

    <TestMethod>
    Public Sub LegacyRunStillRejectsMixedTypesAndHasNoNewMetadataRequirements()
        Dim input As SpreadsheetRangeData = Build(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 4}, New Object() {2, 5}})
        Assert.AreEqual(2, GroupedNumericRangeParser.Parse(input).Groups.Length)
        input.Rows(2)(1) = SpreadsheetCellValue.TextCell("#N/A")
        Assert.ThrowsException(Of ArgumentException)(Function() GroupedNumericRangeParser.Parse(input))
    End Sub
End Class
