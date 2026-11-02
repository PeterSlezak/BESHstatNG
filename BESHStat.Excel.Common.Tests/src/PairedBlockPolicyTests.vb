Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PairedBlockPolicyTests
    Private Shared Function Frame(values As Double(,)) As SpreadsheetRangeData
        Dim f As New SpreadsheetRangeData With {.RowCount = values.GetLength(0), .ColumnCount = values.GetLength(1),
            .StartRow = 20, .StartColumn = 4, .WorkbookId = "book", .WorksheetId = "sheet"}
        For r = 0 To f.RowCount - 1
            Dim row As New List(Of SpreadsheetCellValue)
            For c = 0 To f.ColumnCount - 1
                row.Add(SpreadsheetCellValue.NumberCell(values(r, c)))
            Next
            f.Rows.Add(row)
        Next
        Return f
    End Function

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub Pairs_WholePairsAndNoncontiguousRows(skip As Boolean)
        Dim f = Frame(New Double(,) {{1, 10}, {2, 20}, {3, 30}, {4, 40}})
        f.Rows(1)(0) = SpreadsheetCellValue.BlankCell()
        f.Rows(2)(1) = SpreadsheetCellValue.BlankCell()
        Dim result = PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Missing = If(skip, SpreadsheetIncompleteObservationPolicy.Skip, SpreadsheetIncompleteObservationPolicy.Reject)}, {20, 24, 28, 31})
        Assert.AreEqual(skip, result.Success)
        If skip Then
            CollectionAssert.AreEqual(New Double() {1, 4}, result.Data.FirstValues)
            CollectionAssert.AreEqual(New Double() {10, 40}, result.Data.SecondValues)
            CollectionAssert.AreEqual(New Integer() {20, 31}, result.Data.Pairs.Select(Function(p) p.First.WorksheetRow).ToArray())
            Assert.AreEqual(-9.0, result.Data.Pairs(0).First.Value - result.Data.Pairs(0).Second.Value)
        End If
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.SelectedRow = 2 AndAlso i.WorksheetRow = 24))
    End Sub

    <DataTestMethod> <DataRow("text")> <DataRow("error")> <DataRow("boolean")> <DataRow("nan")> <DataRow("infinity")>
    Public Sub Pairs_InvalidCellsRemainDiagnostics(kind As String)
        Dim f = Frame(New Double(,) {{1, 2}, {3, 4}})
        Select Case kind
            Case "text" : f.Rows(0)(0) = SpreadsheetCellValue.TextCell("2026-11-02")
            Case "error" : f.Rows(0)(0) = SpreadsheetCellValue.ErrorCell("#VALUE!", "15")
            Case "boolean" : f.Rows(0)(0) = SpreadsheetCellValue.BooleanCell(True)
            Case "nan" : f.Rows(0)(0) = SpreadsheetCellValue.NumberCell(Double.NaN)
            Case "infinity" : f.Rows(0)(0) = SpreadsheetCellValue.NumberCell(Double.PositiveInfinity)
        End Select
        Dim result = PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsFalse(result.Success)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Severity = SpreadsheetValidationSeverity.Error AndAlso i.WorksheetRow = 20))
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub Pairs_HeaderModesAndRows(mode As Integer)
        Dim f = Frame(New Double(,) {{1, 2}, {3, 4}})
        f.Rows.Insert(0, New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("0"), SpreadsheetCellValue.TextCell("test")})
        f.RowCount += 1
        Dim header = CType(mode, SpreadsheetHeaderMode)
        Dim result = PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Header = header})
        If header = SpreadsheetHeaderMode.None Then
            Assert.IsFalse(result.Success)
        Else
            Assert.IsTrue(result.Success)
            Assert.AreEqual("0", result.Data.FirstName)
            Assert.AreEqual(21, result.Data.Pairs(0).First.WorksheetRow)
        End If
    End Sub

    <TestMethod>
    Public Sub Pairs_TwoRowsPreservesEachCellCoordinate()
        Dim f = Frame(New Double(,) {{1, 2, 3}, {10, 20, 30}})
        Dim parsed = PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Orientation = SpreadsheetPairOrientation.TwoRows}, {30, 80})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(30, parsed.Data.Pairs(1).First.WorksheetRow)
        Assert.AreEqual(80, parsed.Data.Pairs(1).Second.WorksheetRow)
        Assert.AreEqual(5, parsed.Data.Pairs(1).Second.WorksheetColumn)
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub Blocks_DoNotShiftTreatments(skip As Boolean)
        Dim f = Frame(New Double(,) {{1, 2, 3}, {4, 5, 6}, {7, 8, 9}})
        f.Rows(1)(1) = SpreadsheetCellValue.BlankCell()
        Dim result = PairedBlockInputPolicy.ParseBlocks(f, New SpreadsheetBlockParseOptions With {.Missing = If(skip, SpreadsheetIncompleteObservationPolicy.Skip, SpreadsheetIncompleteObservationPolicy.Reject)}, {21, 40, 99}, {"C", "A", "B"})
        Assert.AreEqual(skip, result.Success)
        If skip Then
            CollectionAssert.AreEqual(New String() {"C", "A", "B"}, result.Data.Names.ToArray())
            Assert.AreEqual(2, result.Data.Blocks.Count)
            Assert.AreEqual(99, result.Data.Blocks(1)(1).WorksheetRow)
            Assert.AreEqual(8.0, result.Data.Matrix(1, 1))
            Dim copy = result.Data.Matrix : copy(0, 0) = 999
            Assert.AreEqual(1.0, result.Data.Matrix(0, 0))
        End If
    End Sub

    <DataTestMethod> <DataRow("none")> <DataRow("one")> <DataRow("two-treatments")> <DataRow("duplicates")> <DataRow("options")> <DataRow("rows")> <DataRow("shape")>
    Public Sub Blocks_NegativeContracts(kind As String)
        Dim f = Frame(New Double(,) {{1, 2, 3}, {4, 5, 6}})
        Dim options As New SpreadsheetBlockParseOptions
        Dim ids As Integer() = {20, 21}
        Select Case kind
            Case "none" : f.Rows(0)(0) = SpreadsheetCellValue.BlankCell() : f.Rows(1)(0) = SpreadsheetCellValue.BlankCell() : options.Missing = SpreadsheetIncompleteObservationPolicy.Skip
            Case "one" : options.MinimumBlocks = 3
            Case "two-treatments" : options.MinimumTreatments = 4
            Case "duplicates" : options.RequireUniqueNames = True
            Case "options" : options.Missing = CType(99, SpreadsheetIncompleteObservationPolicy)
            Case "rows" : ids = {20, 20}
            Case "shape" : f.Rows(0).RemoveAt(0)
        End Select
        Assert.IsFalse(PairedBlockInputPolicy.ParseBlocks(f, options, ids, {"same", "same", "C"}).Success)
    End Sub

    <TestMethod>
    Public Sub Blocks_ExplicitTextHeadersIncludingZeroAndDuplicateLabels()
        Dim f = Frame(New Double(,) {{0, 0, 0}, {1, 2, 3}})
        f.Rows(0) = New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("0"), SpreadsheetCellValue.TextCell("a"), SpreadsheetCellValue.TextCell("a")}
        Dim parsed = PairedBlockInputPolicy.ParseBlocks(f, New SpreadsheetBlockParseOptions With {.Header = SpreadsheetHeaderMode.Required})
        Assert.IsTrue(parsed.Success)
        CollectionAssert.AreEqual(New String() {"0", "a", "a"}, parsed.Data.Names.ToArray())
    End Sub

    <DataTestMethod> <DataRow("identity")> <DataRow("rows")> <DataRow("empty")> <DataRow("columns")> <DataRow("missing-keys")> <DataRow("duplicate-keys")>
    Public Sub IndependentAlignment_RejectsAmbiguousInputs(kind As String)
        Dim a = Frame(New Double(,) {{1, 2}, {3, 4}}), b = Frame(New Double(,) {{5, 6}, {7, 8}})
        Dim ar As Integer() = {20, 30}, br As Integer() = {30, 20}
        Dim ak As String() = Nothing, bk As String() = Nothing
        Select Case kind
            Case "identity" : b.WorksheetId = "other"
            Case "rows" : br = {30, 30}
            Case "empty" : br = {40, 50}
            Case "columns" : b = Frame(New Double(,) {{1}, {2}})
            Case "missing-keys" : ak = {"a", "b"}
            Case "duplicate-keys" : ak = {"a", "a"} : bk = {"a", "b"}
        End Select
        Assert.IsFalse(PairedBlockInputPolicy.AlignRows(a, ar, b, br, ak, bk).Success)
    End Sub

    <TestMethod>
    Public Sub IndependentAlignment_StableFirstOrder_AndExplicitSubjectKeys()
        Dim a = Frame(New Double(,) {{1}, {2}, {3}}), b = Frame(New Double(,) {{9}, {8}, {7}})
        Dim joined = PairedBlockInputPolicy.AlignRows(a, {60, 20, 90}, b, {20, 60, 80})
        Assert.IsTrue(joined.Success)
        CollectionAssert.AreEqual(New Integer() {1, 2}, joined.Data.FirstRows.ToArray())
        CollectionAssert.AreEqual(New Integer() {2, 1}, joined.Data.SecondRows.ToArray())
        Assert.AreEqual(2, joined.Validation.Issues.Count)
        b.WorkbookId = "other" ' Subject identity is explicitly different from physical identity.
        joined = PairedBlockInputPolicy.AlignRows(a, {60, 20, 90}, b, {20, 60, 80}, {"b", "a", "c"}, {"a", "b", "z"})
        Assert.IsTrue(joined.Success)
        CollectionAssert.AreEqual(New Integer() {2, 1}, joined.Data.SecondRows.ToArray())
    End Sub

    <TestMethod>
    Public Sub Nested_MembershipUsesParentAndChild_PreservesRepetitionsAndGaps()
        Dim f = Frame(New Double(,) {{1, 1, 10}, {1, 1, 11}, {2, 1, 20}, {2, 1, 21}})
        f.Rows(1)(1) = SpreadsheetCellValue.BlankCell()
        Dim result = PairedBlockInputPolicy.ParseNested(f, {20, 21, 28, 90})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New String() {"1", "2", "2"}, result.Data.GroupLabels.ToArray())
        CollectionAssert.AreEqual(New String() {"1", "1", "1"}, result.Data.NestedLabels.ToArray())
        Assert.AreEqual(28, result.Data.Responses.Blocks(1)(0).WorksheetRow)
        Assert.AreEqual(3, result.Data.Responses.Blocks(1)(0).SelectedColumn)
        Assert.AreEqual(6, result.Data.Responses.Blocks(1)(0).WorksheetColumn)
        Assert.IsFalse(PairedBlockInputPolicy.ParseNested(f, {20, 21, 28, 90}, SpreadsheetIncompleteObservationPolicy.Reject).Success)
    End Sub

    <DataTestMethod> <DataRow(0.0)> <DataRow(1.0)> <DataRow(-1.0)> <DataRow(Double.NaN)> <DataRow(Double.PositiveInfinity)>
    Public Sub Options_InvalidAlpha(alpha As Double)
        Assert.IsTrue(PairedOptionPolicy.Validate(alpha, PairedAlternative.TwoSided, PairedDifferenceDirection.FirstMinusSecond).HasErrors)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)>
    Public Sub Bland_FiveScaleUnitsAndLegacyProjection(scale As Integer)
        Dim opts = PairedOptionPolicy.ProjectBland(0, scale, True, True, True)
        Assert.AreEqual(CType(scale, Agreement.BlandAltmanScale), opts.Scale)
        Assert.AreEqual(Agreement.AgreementCiMethod.BootstrapPercentile, opts.CiMethod)
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {1.0, 2.0}, {2.0, 3.0}).IsValid)
        Assert.IsTrue(PairedOptionPolicy.ValidateLimits(True, -10, 10).IsValid)
        Assert.IsTrue(PairedOptionPolicy.ValidateLimits(True, 1, 1).IsValid)
        Assert.IsTrue(PairedOptionPolicy.ValidateLimits(False, Double.NaN, Double.NaN).IsValid)
        Assert.IsTrue(PairedOptionPolicy.ValidateLimits(True, 2, 1).HasErrors)
        Assert.IsTrue(PairedOptionPolicy.ValidateLimits(True, Double.NaN, 1).HasErrors)
    End Sub

    <DataTestMethod> <DataRow(1, -1.0, 1.0)> <DataRow(2, 0.0, 1.0)> <DataRow(3, 1.0, 0.0)> <DataRow(4, -1.0, 1.0)>
    Public Sub Bland_InvalidScaleDomain(scale As Integer, x As Double, y As Double)
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(PairedOptionPolicy.ProjectBland(1, scale, False, False, False), {x}, {y}).HasErrors)
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub CultureAndFreshInstanceIsolation(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim f = Frame(New Double(,) {{1, 2}, {3, 4}})
            f.Rows(0)(0) = SpreadsheetCellValue.TextCell(If(culture = "", "1.5", "1,5"))
            Dim opts As New SpreadsheetPairedParseOptions With {.CellPolicy = New SpreadsheetCellPolicyOptions With {.NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture, .NumericCultureName = If(culture = "", "en-US", culture)}}
            Assert.AreEqual(1.5, PairedBlockInputPolicy.ParsePairs(f, opts).Data.FirstValues(0))
            Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions()).Success)
            Assert.AreEqual(SpreadsheetCellKind.Text, f.Rows(0)(0).Kind)
            Dim projected = PairedOptionPolicy.ProjectBland(99, 99, False, False, False)
            Assert.AreEqual(Agreement.BlandAltmanScale.RawDifference, projected.Scale)
            projected.Alpha = -1
            Assert.IsTrue(PairedOptionPolicy.ProjectBland(0, 0, False, False, False).Alpha > 0)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub Options_EnumsFallbackAndRepeatedSubjectsRemainExplicit()
        Assert.IsTrue(PairedOptionPolicy.Validate(0.05, CType(99, PairedAlternative), PairedDifferenceDirection.FirstMinusSecond).HasErrors)
        Assert.IsTrue(PairedOptionPolicy.Validate(0.05, PairedAlternative.TwoSided, CType(99, PairedDifferenceDirection)).HasErrors)
        Dim opts = PairedOptionPolicy.ProjectBland(2, 0, False, False, False)
        opts.AllowFallbackToSimple = False
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {1.0}, {2.0}).HasErrors)
        opts.AllowFallbackToSimple = True
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {1.0}, {2.0}).IsValid)
        opts.SubjectIds = {CObj("s"), CObj("s")}
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {1.0}, {2.0}).HasErrors)
        opts.SubjectIds = {CObj("s")}
        opts.Scale = CType(99, Agreement.BlandAltmanScale)
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {1.0}, {2.0}).HasErrors)
        Assert.AreEqual(Agreement.AgreementCiMethod.BootstrapBCa, PairedOptionPolicy.ProjectBland(0, 0, False, True, True).CiMethod)
        Assert.AreEqual(Agreement.AgreementCiMethod.Jackknife, PairedOptionPolicy.ProjectBland(0, 0, False, False, True).CiMethod)
    End Sub

    <TestMethod>
    Public Sub Blocks_AllBlankAndInvalidCellsHaveOriginalCoordinates()
        Dim f = Frame(New Double(,) {{1, 2, 3}, {4, 5, 6}})
        f.Rows(0) = New List(Of SpreadsheetCellValue) From {Nothing, Nothing, Nothing}
        f.Rows(1)(2) = SpreadsheetCellValue.ErrorCell("#N/A", "2042")
        Dim result = PairedBlockInputPolicy.ParseBlocks(f, New SpreadsheetBlockParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}, {40, 80})
        Assert.IsFalse(result.Success)
        Dim errorCell = result.Validation.Issues.Single(Function(i) i.Code = "blocks.numeric")
        Assert.AreEqual(80, errorCell.WorksheetRow)
        Assert.AreEqual(6, errorCell.WorksheetColumn)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "blocks.too-few"))
    End Sub

    <TestMethod>
    Public Sub PairOptionsAndRowMapMustBeValidBeforeParsing()
        Dim f = Frame(New Double(,) {{1, 2}, {3, 4}})
        Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.Orientation = CType(99, SpreadsheetPairOrientation)}).Success)
        Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.FirstIndex = 1, .SecondIndex = 1}).Success)
        Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions, {20}).Success)
        Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions, {20, 20}).Success)
        Assert.IsFalse(PairedBlockInputPolicy.ParsePairs(f, New SpreadsheetPairedParseOptions With {.MinimumPairs = 3}).Success)
    End Sub

    <TestMethod>
    Public Sub Bland_MissingSubjectIsFilteredBeforeScaleDomain_WhileSimplePairsKeepsIt()
        Dim opts = PairedOptionPolicy.ProjectBland(0, 4, False, False, False)
        opts.SubjectIds = {CObj(""), CObj("s")}
        Dim report = PairedOptionPolicy.ValidateBland(opts, {-1.0, 2.0}, {1.0, 3.0})
        Assert.IsTrue(report.IsValid)
        Assert.IsTrue(report.Issues.Any(Function(i) i.Code = "bland.subject.filtered" AndAlso i.SelectedRow = 1))
        opts.Mode = Agreement.RepeatedBlandAltmanMode.SimplePairs
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(opts, {-1.0, 2.0}, {1.0, 3.0}).HasErrors)
    End Sub
End Class
