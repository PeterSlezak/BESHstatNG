Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.IO
Imports System.Text.Json.Nodes
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UnivariateSelectionPolicyTests
    Private Shared Function Snapshot(ParamArray rows As Object()()) As SpreadsheetRangeData
        Dim input As New SpreadsheetRangeData With {.StartRow = 10, .StartColumn = 4,
            .WorkbookId = "book-1", .WorksheetId = "sheet-1", .WorksheetName = "Study",
            .Address = "D10", .RowCount = rows.Length, .ColumnCount = If(rows.Length = 0, 0, rows(0).Length)}
        For Each row In rows
            input.Rows.Add(row.Select(Function(value) Cell(value)).ToList())
        Next
        Return input
    End Function

    Private Shared Function Cell(value As Object) As SpreadsheetCellValue
        If value Is Nothing Then Return SpreadsheetCellValue.BlankCell()
        If TypeOf value Is SpreadsheetCellValue Then Return DirectCast(value, SpreadsheetCellValue)
        If TypeOf value Is String Then Return SpreadsheetCellValue.TextCell(DirectCast(value, String))
        If TypeOf value Is Boolean Then Return SpreadsheetCellValue.BooleanCell(CBool(value))
        Return SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture))
    End Function

    Private Shared Function Options(Optional orientation As SpreadsheetGroupedOrientation = SpreadsheetGroupedOrientation.GroupsInColumns,
                                    Optional header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required,
                                    Optional minimum As Integer = 1) As SpreadsheetGroupedParseOptions
        Return New SpreadsheetGroupedParseOptions With {.Orientation = orientation, .Header = header,
            .MinimumGroups = minimum, .MinimumObservationsPerGroup = 1}
    End Function

    Private Shared Sub HasCode(result As UnivariateSelectionResult, code As String)
        Assert.IsFalse(result.Success)
        Assert.IsNull(result.Data)
        Assert.AreEqual(0, result.Sources.Count)
        Assert.IsTrue(result.Validation.Issues.Any(Function(issue) issue.Code = code),
            String.Join(", ", result.Validation.Issues.Select(Function(issue) issue.Code)))
    End Sub

    <DataTestMethod>
    <DataRow(False, False, False)>
    <DataRow(False, True, True)>
    <DataRow(True, False, True)>
    <DataRow(True, True, True)>
    Public Sub InputRoles_MatchFrozenCheckInputsTruthTable(twoSample As Boolean, byId As Boolean, first As Boolean)
        Dim roles = UnivariateSelectionPolicy.InputRoles(twoSample, byId)
        Assert.AreEqual(first, roles.FirstInputRequired)
        Assert.AreEqual(first, roles.InputsMustBeSingleColumn)
    End Sub

    <DataTestMethod>
    <DataRow(2)> <DataRow(3)>
    Public Sub Wide_GroupsAreSelectedColumns_AndIndependentMissingRows(count As Integer)
        Dim header = Enumerable.Range(1, count).Select(Function(i) CObj("G" & i.ToString(CultureInfo.InvariantCulture))).ToArray()
        Dim row1 = Enumerable.Range(1, count).Select(Function(i) CObj(i)).ToArray()
        Dim row2 = Enumerable.Range(1, count).Select(Function(i) If(i = 1, Nothing, CObj(i * 10))).ToArray()
        Dim parsed = UnivariateSelectionPolicy.Parse(Snapshot(header, row1, row2), Options())
        Assert.IsTrue(parsed.Success)
        CollectionAssert.AreEqual(header.Cast(Of String)().ToArray(), parsed.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {1}, parsed.Data.Groups(0))
        Assert.AreEqual(count * 2 - 1, parsed.Sources.Count)
        Assert.AreEqual(11, parsed.Sources(0).WorksheetRow)
        Assert.AreEqual(4, parsed.Sources(0).WorksheetColumn)
        Assert.AreEqual("book-1", parsed.Sources(0).WorkbookId)
    End Sub

    <DataTestMethod>
    <DataRow(SpreadsheetHeaderMode.Required)> <DataRow(SpreadsheetHeaderMode.Auto)>
    Public Sub NumericLookingTextHeaders_AreExplicitWithStrictNumericPolicy(header As SpreadsheetHeaderMode)
        Dim result = UnivariateSelectionPolicy.Parse(Snapshot({"0", "1"}, {2, 3}), Options(header:=header))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"0", "1"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {2}, result.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub HeaderNone_PreservesFirstNumericRow_AndDuplicateWideLabelsAreNotMerged()
        Dim result = UnivariateSelectionPolicy.Parse(Snapshot({1, 2}, {3, 4}), Options(header:=SpreadsheetHeaderMode.None))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {1, 3}, result.Data.Groups(0))
        Dim duplicates = UnivariateSelectionPolicy.Parse(Snapshot({"A", "A"}, {1, 2}), Options())
        Assert.IsTrue(duplicates.Success)
        CollectionAssert.AreEqual(New String() {"A", "A"}, duplicates.Data.GroupNames)
        Assert.AreEqual(2, duplicates.Data.Groups.Length)
    End Sub

    <TestMethod>
    Public Sub RowOrientation_ExplicitSnapshotOnly_UsesParserAndCoordinates()
        Dim result = UnivariateSelectionPolicy.Parse(Snapshot({"B", 7, Nothing, 8}, {"A", 1, 2, 3}),
            Options(SpreadsheetGroupedOrientation.GroupsInRows))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"B", "A"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {7, 8}, result.Data.Groups(0))
        Assert.AreEqual(7, result.Sources(1).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Long_FirstObservationOrder_OrdinalCase_TrimmedLabels_AndRaggedCounts()
        Dim result = UnivariateSelectionPolicy.Parse(Snapshot({"ID", "Value"}, {" B ", 8}, {"a", 1},
            {"B", Nothing}, {"A", 2}, {"B", 9}), Options(SpreadsheetGroupedOrientation.LongFormat))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"B", "a", "A"}, result.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {8, 9}, result.Data.Groups(0))
        Assert.AreEqual(15, result.Sources.Last().WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub SeparateRoles_AlignRowsAndPreserveDataCoordinatesAndGaps()
        Dim values = Snapshot({1}, {2}, {Nothing}, {3})
        Dim ids = Snapshot({"B"}, {"A"}, {"B"}, {"B"})
        ids.StartColumn = 2
        Dim result = UnivariateSelectionPolicy.Parse(values, Options(SpreadsheetGroupedOrientation.LongFormat,
            SpreadsheetHeaderMode.None), ids, New Integer() {101, 103, 107, 109})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"B", "A"}, result.Data.GroupNames)
        Dim bRows = result.Sources.Where(Function(source) source.GroupIndex = 0).Select(Function(source) source.WorksheetRow).ToArray()
        CollectionAssert.AreEqual(New Integer() {101, 109}, bRows)
        Assert.IsTrue(result.Sources.All(Function(source) source.WorksheetColumn = 4 AndAlso source.SelectedColumn = 1))
        Dim sources = DirectCast(result.Sources, IList(Of UnivariateObservationSource))
        Assert.ThrowsException(Of NotSupportedException)(Sub() sources.Clear())
        ids.Rows(0)(0).TextValue = "changed"
        CollectionAssert.AreEqual(New String() {"B", "A"}, result.Data.GroupNames)
    End Sub

    <DataTestMethod>
    <DataRow("rows", "roles.rows")> <DataRow("start", "roles.rows")>
    <DataRow("workbook", "roles.worksheet")> <DataRow("worksheet", "roles.worksheet")>
    <DataRow("columns", "roles.shape")> <DataRow("ragged", "roles.shape")>
    Public Sub SeparateRoles_RejectMismatch(kind As String, code As String)
        Dim values = Snapshot({1}, {2})
        Dim ids = Snapshot({"A"}, {"B"})
        Select Case kind
            Case "rows" : ids.Rows.RemoveAt(1) : ids.RowCount = 1
            Case "start" : ids.StartRow += 1
            Case "workbook" : ids.WorkbookId = "other"
            Case "worksheet" : ids.WorksheetId = "other"
            Case "columns" : ids.ColumnCount = 2
            Case "ragged" : ids.Rows(1) = Nothing
        End Select
        HasCode(UnivariateSelectionPolicy.Parse(values, Options(SpreadsheetGroupedOrientation.LongFormat,
            SpreadsheetHeaderMode.None), ids), code)
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub SourceRows_RejectShortDuplicateAndNegative(mode As Integer)
        Dim rows = If(mode = 0, New Integer() {1}, If(mode = 1, New Integer() {2, 2}, New Integer() {-1, 2}))
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({1}, {2}), Options(header:=SpreadsheetHeaderMode.None),
            physicalRowIds:=rows), "source.row-ids")
    End Sub

    <TestMethod>
    Public Sub SeparateRoles_RejectDifferentPhysicalRowIds_EvenWhenDimensionsMatch()
        Dim values = Snapshot({1}, {2})
        Dim ids = Snapshot({"A"}, {"B"})
        HasCode(UnivariateSelectionPolicy.Parse(values, Options(SpreadsheetGroupedOrientation.LongFormat,
            SpreadsheetHeaderMode.None), ids, New Integer() {101, 103}, New Integer() {101, 104}), "roles.rows")
    End Sub

    <TestMethod>
    Public Sub LongRoles_CannotCollideOrUseExcelErrorOrNonFiniteCategory()
        Dim opts = Options(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.None)
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"1", 1}, {1, 2}), opts), "groups.category-collision")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({SpreadsheetCellValue.ErrorCell("#REF!", "2023"), 1}), opts), "groups.category-error")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({Double.NaN, 1}), opts), "groups.category-invalid")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({1}), opts), "options.long-columns")
        opts.ValueColumnIndex = 5
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A", 1}), opts), "options.long-columns")
    End Sub

    <TestMethod>
    Public Sub BadCellPolicyCultureAndCoordinates_AreStructuredDiagnostics()
        Dim opts = Options()
        opts.CellPolicy.Errors = CType(99, SpreadsheetErrorPolicy)
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({1}), opts), "options.invalid")
        opts.CellPolicy.Errors = SpreadsheetErrorPolicy.Reject
        opts.CellPolicy.NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture
        opts.CellPolicy.NumericCultureName = ""
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({1}), opts), "options.invalid")
        Dim input = Snapshot({1}) : input.StartRow = -1
        HasCode(UnivariateSelectionPolicy.Parse(input, Options()), "range.shape")
    End Sub

    <DataTestMethod>
    <DataRow("", "cell.not-numeric")> <DataRow("sk-SK", "cell.not-numeric")>
    Public Sub StrictCells_ReportStableCodesAndSelectedPhysicalCoordinates(culture As String, code As String)
        Dim before = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = If(culture = "", CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo(culture))
            Dim input = Snapshot({"A"}, {1}, {"oops"})
            Dim result = UnivariateSelectionPolicy.Parse(input, Options(), physicalRowIds:=New Integer() {100, 104, 108})
            HasCode(result, code)
            Dim issue = result.Validation.Issues.Single(Function(i) i.Code = code)
            Assert.AreEqual(3, issue.SelectedRow)
            Assert.AreEqual(108, issue.WorksheetRow)
            Assert.AreEqual(4, issue.WorksheetColumn)
            Assert.AreEqual("observations", issue.FieldPath)
        Finally
            CultureInfo.CurrentCulture = before
        End Try
    End Sub

    <TestMethod>
    Public Sub SeparateRoleErrors_MapCategoryAndValueToTheirOwnPhysicalColumn()
        Dim values = Snapshot({1}, {SpreadsheetCellValue.ErrorCell("#N/A", "2042")})
        Dim ids = Snapshot({"A"}, {"B"}) : ids.StartColumn = 2
        Dim opts = Options(SpreadsheetGroupedOrientation.LongFormat, SpreadsheetHeaderMode.None)
        Dim errorResult = UnivariateSelectionPolicy.Parse(values, opts, ids)
        HasCode(errorResult, "cell.excel-error")
        Assert.AreEqual(4, errorResult.Validation.Issues.Single(Function(i) i.Code = "cell.excel-error").WorksheetColumn)
        Assert.AreEqual("2042", values.Rows(1)(0).NativeErrorCode)
        Assert.AreEqual("2042", errorResult.SourceErrors.Single().NativeErrorCode)
        values.Rows(1)(0).NativeErrorCode = "mutated"
        Assert.AreEqual("2042", errorResult.SourceErrors.Single().NativeErrorCode)
        values.Rows(1)(0) = SpreadsheetCellValue.NumberCell(2)
        ids.Rows(1)(0) = SpreadsheetCellValue.BlankCell()
        Dim missing = UnivariateSelectionPolicy.Parse(values, opts, ids)
        HasCode(missing, "groups.category-missing")
        Assert.AreEqual(2, missing.Validation.Issues.Single(Function(i) i.Code = "groups.category-missing").WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub ErrorsAndBlanks_ExplicitSkipPolicy_DoesNotMutateErrorMetadata()
        Dim input = Snapshot({"A"}, {Nothing}, {SpreadsheetCellValue.ErrorCell("#VALUE!", "2015")}, {2}, {Nothing})
        Dim opts = Options() : opts.WarnOnSkippedObservations = True
        opts.CellPolicy.Errors = SpreadsheetErrorPolicy.TreatAsMissing
        Dim result = UnivariateSelectionPolicy.Parse(input, opts)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(3, result.Validation.Issues.Count)
        Assert.AreEqual(13, result.Sources.Single().WorksheetRow)
        Assert.AreEqual("2015", input.Rows(2)(0).NativeErrorCode)
        Assert.AreEqual("2015", result.SourceErrors.Single().NativeErrorCode)
    End Sub

    <DataTestMethod>
    <DataRow("NaN")> <DataRow("Infinity")> <DataRow("-Infinity")>
    Public Sub NonFiniteNumbers_AreDiagnostics(token As String)
        Dim number = Double.Parse(token, CultureInfo.InvariantCulture)
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A"}, {number}), Options()), "cell.invalid-value")
    End Sub

    <TestMethod>
    Public Sub BooleanAndNumericText_AreOptIn_DateFormatDoesNotInventADateType()
        Dim input = Snapshot({"A"}, {True}, {"1,5"}, {46000})
        input.Rows(3)(0).NumberFormat = "yyyy-mm-dd"
        HasCode(UnivariateSelectionPolicy.Parse(input, Options()), "cell.not-numeric")
        Dim opts = Options()
        opts.CellPolicy.AllowBooleanNumbers = True
        opts.CellPolicy.NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture
        opts.CellPolicy.NumericCultureName = "sk-SK"
        Dim result = UnivariateSelectionPolicy.Parse(input, opts)
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {1, 1.5, 46000}, result.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub ZeroUsableGroups_AllBlankOrHeaderOnly_AndMinimumObservations()
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A"}, {Nothing}), Options()), "groups.too-few-observations")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A"}), Options()), "groups.too-few-observations")
        Dim opts = Options() : opts.MinimumObservationsPerGroup = 2
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A"}, {1}), opts), "groups.too-few-observations")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot(New Object() {Nothing, Nothing}), Options(header:=SpreadsheetHeaderMode.None)), "groups.too-few-observations")
    End Sub

    <TestMethod>
    Public Sub AnalysisGroupConstraints_OneGroupOrExactlyTwo_AreExplicit()
        Assert.IsTrue(UnivariateSelectionPolicy.Parse(Snapshot({"A"}, {1}), Options()).Success)
        Dim two = Options(minimum:=2) : two.MaximumGroups = 2
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A"}, {1}), two), "range.too-few-groups")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A", "B", "C"}, {1, 2, 3}), two), "groups.too-many")
    End Sub

    <DataTestMethod>
    <DataRow("orientation")> <DataRow("header")> <DataRow("min")> <DataRow("max")>
    <DataRow("roles")> <DataRow("policy")>
    Public Sub InvalidOptions_ProduceDiagnostics(kind As String)
        Dim opts = Options()
        Select Case kind
            Case "orientation" : opts.Orientation = CType(99, SpreadsheetGroupedOrientation)
            Case "header" : opts.Header = CType(99, SpreadsheetHeaderMode)
            Case "min" : opts.MinimumGroups = 0
            Case "max" : opts.MaximumGroups = 0
            Case "roles" : opts.Orientation = SpreadsheetGroupedOrientation.LongFormat : opts.ValueColumnIndex = 0
            Case "policy" : opts.CellPolicy = Nothing
        End Select
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A", "B"}, {1, 2}), opts),
            If(kind = "roles", "options.long-columns", "options.invalid"))
    End Sub

    <TestMethod>
    Public Sub NullRaggedAndAmbiguousInputs_AreDiagnostics()
        HasCode(UnivariateSelectionPolicy.Parse(Nothing, Options()), "range.null")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({1}), Nothing), "options.invalid")
        Dim ragged = Snapshot({1, 2}, {3})
        HasCode(UnivariateSelectionPolicy.Parse(ragged, Options()), "range.not-rectangular")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({"A", 1}, {2, 3}), Options(header:=SpreadsheetHeaderMode.Auto)), "header.ambiguous")
        HasCode(UnivariateSelectionPolicy.Parse(Snapshot({Nothing, 1}), Options(SpreadsheetGroupedOrientation.LongFormat,
            SpreadsheetHeaderMode.None)), "groups.category-missing")
    End Sub

    <TestMethod>
    Public Sub RawDiagnostics_MatchExistingParser_WithoutLocalizationOrDefaultDrift()
        Dim input = Snapshot({"A", "B"}, {1, SpreadsheetCellValue.ErrorCell("#REF!")}, {"bad", 2})
        Dim opts = Options(minimum:=2)
        Dim before = ConfigurableGroupedNumericParser.Parse(input, opts)
        Dim after = UnivariateSelectionPolicy.Parse(input, opts)
        CollectionAssert.AreEqual(before.Validation.Issues.Select(Function(i) i.Code & "|" & i.Message & "|" & i.FieldPath & "|" & i.WorksheetRow & "|" & i.WorksheetColumn).ToArray(),
                                  after.Validation.Issues.Select(Function(i) i.Code & "|" & i.Message & "|" & i.FieldPath & "|" & i.WorksheetRow & "|" & i.WorksheetColumn).ToArray())
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA", "")> <DataRow("C30-SYN-ANOVA", "sk-SK")>
    <DataRow("C30-SYN-KW", "")> <DataRow("C30-SYN-KW", "sk-SK")>
    Public Sub AcceptedCandidateFixtures_ExactLabelsValuesAndSeparatePhysicalRowProvenance(id As String, culture As String)
        Dim beforeCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = If(culture = "", CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo(culture))
            Dim fixture = MigrationFixture04A02.Load(id)
            Dim original = MigrationFixture04A02.Grouped(fixture)
            Dim input As New SpreadsheetRangeData With {.ColumnCount = 2, .StartColumn = 1, .StartRow = 1}
            Dim physical As New List(Of Integer)()
            For g As Integer = 0 To original.Groups.Length - 1
                For r As Integer = 0 To original.Groups(g).Length - 1
                    input.Rows.Add(New List(Of SpreadsheetCellValue) From {
                        SpreadsheetCellValue.TextCell(original.GroupNames(g)), SpreadsheetCellValue.NumberCell(original.Groups(g)(r))})
                    physical.Add(fixture("dataset")("row_ids")(g)(r).GetValue(Of Integer)())
                Next
            Next
            input.RowCount = input.Rows.Count
            Dim result = UnivariateSelectionPolicy.Parse(input, Options(SpreadsheetGroupedOrientation.LongFormat,
                SpreadsheetHeaderMode.None, 2), physicalRowIds:=physical)
            Assert.IsTrue(result.Success)
            CollectionAssert.AreEqual(original.GroupNames, result.Data.GroupNames)
            For g As Integer = 0 To original.Groups.Length - 1
                CollectionAssert.AreEqual(original.Groups(g), result.Data.Groups(g))
            Next
            CollectionAssert.AreEqual(physical.ToArray(), result.Sources.Select(Function(s) s.WorksheetRow).ToArray())
            If id = "C30-SYN-ANOVA" Then
                Dim oldOutput = OneWayAnovaApplicationService.Run(original)
                Dim newOutput = OneWayAnovaApplicationService.Run(result.Data)
                Assert.AreEqual(oldOutput.FStatistic, newOutput.FStatistic)
                Assert.AreEqual(oldOutput.PValue, newOutput.PValue)
            Else
                Dim oldOutput = KruskalWallisApplicationService.Run(original)
                Dim newOutput = KruskalWallisApplicationService.Run(result.Data)
                Assert.AreEqual(oldOutput.PValue, newOutput.PValue)
            End If
            Assert.AreEqual("CANDIDATE_ONLY", fixture("output_status").GetValue(Of String)())
            ' Compare every serialized metric/table/CLR/null/p-value annotation with
            ' hash-bound accepted C14 CANDIDATE evidence, never a promoted golden.
            Dim manifestPath = Path.Combine(MigrationFixture04A02.SourceRoot(), "docs/architecture/04A02/04A02-C14-current-Core-reference-comparisons-v1.json")
            Dim manifest = JsonNode.Parse(File.ReadAllText(manifestPath))
            Dim row = manifest("rows").AsArray().First(Function(n) n("case_id").GetValue(Of String)() = id AndAlso
                n("configuration").GetValue(Of String)() = "Debug" AndAlso n("culture").GetValue(Of String)() = If(culture = "", "invariant", culture))
            Dim candidatePath = Path.Combine(MigrationFixture04A02.SourceRoot(), row("candidate_path").GetValue(Of String)())
            Assert.AreEqual(row("candidate_sha256").GetValue(Of String)(), MigrationFixture04A02.FileHash(candidatePath))
            Dim observed = MigrationFixture04A02.Observe(fixture)
            Dim serialized = MigrationFixture04A02.Canonical(observed)
            Assert.AreEqual(MigrationFixture04A02.Canonical(JsonNode.Parse(File.ReadAllText(candidatePath))), serialized)
            Dim output = Environment.GetEnvironmentVariable("BESHSTAT_04A03_OBSERVATIONS")
            If Not String.IsNullOrEmpty(output) Then
                Dim full = Path.GetFullPath(output)
                Dim allowed = Path.Combine(MigrationFixture04A02.SourceRoot(), "TestResults", "04A-03-")
                Assert.IsTrue(full.StartsWith(allowed, StringComparison.OrdinalIgnoreCase))
                Directory.CreateDirectory(full)
                Dim filename = id & "-" & If(culture = "", "invariant", culture) & ".candidate.json"
                Using stream As New FileStream(Path.Combine(full, filename), FileMode.CreateNew, FileAccess.Write)
                    Using writer As New StreamWriter(stream, New System.Text.UTF8Encoding(False))
                        writer.Write(serialized & vbLf)
                    End Using
                End Using
            End If
        Finally
            CultureInfo.CurrentCulture = beforeCulture
        End Try
    End Sub
End Class
