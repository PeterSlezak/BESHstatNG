Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RectangularCategoricalParserTests

    Private Shared Function Source(ParamArray data As Object()()) As SpreadsheetRangeData
        Dim rows As New List(Of List(Of SpreadsheetCellValue))()
        For Each values As Object() In data
            Dim cells As New List(Of SpreadsheetCellValue)()
            For Each value As Object In values
                If value Is Nothing Then
                    cells.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf value Is SpreadsheetCellValue Then
                    cells.Add(DirectCast(value, SpreadsheetCellValue))
                ElseIf TypeOf value Is Boolean Then
                    cells.Add(SpreadsheetCellValue.BooleanCell(CBool(value)))
                ElseIf TypeOf value Is Double OrElse TypeOf value Is Integer OrElse TypeOf value Is Long Then
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                Else
                    cells.Add(SpreadsheetCellValue.TextCell(Convert.ToString(value, CultureInfo.InvariantCulture)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {
            .WorkbookId = "workbook-id", .WorkbookName = "Study.xlsx",
            .WorksheetId = "sheet-id", .WorksheetName = "Study", .Address = "B12:C20",
            .StartRow = 12, .StartColumn = 2, .Rows = rows, .RowCount = rows.Count,
            .ColumnCount = If(rows.Count = 0, 0, rows(0).Count),
            .SelectionId = "selection-a", .DateSystem = SpreadsheetDateSystem.Excel1904}
    End Function

    Private Shared Function Issue(Of T As SpreadsheetAnalysisInput)(result As SpreadsheetTypedParseResult(Of T), code As String) As SpreadsheetValidationIssue
        Return result.Validation.Issues.FirstOrDefault(Function(item) item.Code = code)
    End Function

    <TestMethod>
    Public Sub Frame_RequiredHeaders_MixedTypesAndProvenance()
        Dim dateLike As SpreadsheetCellValue = SpreadsheetCellValue.NumberCell(45000)
        dateLike.NumberFormat = "m/d/yyyy"
        Dim input = Source(New Object() {"Value", "Label", "Flag", "Error"},
                           New Object() {dateLike, "#N/A", True, SpreadsheetCellValue.ErrorCell("#N/A")})
        Dim result = RectangularCategoricalParsers.ParseDataFrame(input)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(1, result.Data.RowCount)
        Assert.AreEqual(4, result.Data.ColumnCount)
        Assert.AreEqual("Value", result.Data.Columns(0).Name)
        Assert.AreEqual("workbook-id", result.Data.WorkbookId)
        Assert.AreEqual("selection-a", result.Data.SelectionId)
        Assert.AreEqual(SpreadsheetDateSystem.Excel1904, result.Data.DateSystem)
        Assert.AreEqual("m/d/yyyy", result.Data.Cell(0, 0).NumberFormat)
        Assert.AreEqual(SpreadsheetCellKind.Number, result.Data.Cell(0, 0).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Text, result.Data.Cell(0, 1).Kind)
        Assert.AreEqual(SpreadsheetCellKind.Bool, result.Data.Cell(0, 2).Kind)
        Assert.AreEqual(SpreadsheetCellKind.CellError, result.Data.Cell(0, 3).Kind)
        Assert.AreEqual(13, result.Data.Cell(0, 3).WorksheetRow)
        Assert.AreEqual(5, result.Data.Cell(0, 3).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Frame_DefensiveSnapshotSurvivesMutationOfOriginal()
        Dim input = Source(New Object() {"X"}, New Object() {12})
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(input)
        input.Rows(1)(0).NumberValue = 99
        input.Rows(1).Clear()
        Assert.AreEqual(12.0R, parsed.Data.Cell(0, 0).NumberValue)
        Dim editable = parsed.Data.Cell(0, 0).ToCellValue()
        editable.NumberValue = 777
        Assert.AreEqual(12.0R, parsed.Data.Cell(0, 0).NumberValue)
    End Sub

    <TestMethod>
    Public Sub Frame_NoHeaderGeneratesNames()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {1, "a"}),
            New SpreadsheetDataFrameParseOptions With {.Header = SpreadsheetHeaderMode.None})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("Column 1", parsed.Data.Columns(0).Name)
        Assert.AreEqual(1, parsed.Data.RowCount)
    End Sub

    <TestMethod>
    Public Sub Frame_AutoHeadersForTextAboveNumeric()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {"A", "B"}, New Object() {1, 2}),
            New SpreadsheetDataFrameParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsTrue(parsed.Success)
        Assert.IsNotNull(Issue(parsed, "header.inferred"))
        Assert.AreEqual("B", parsed.Data.Columns(1).Name)
    End Sub

    <TestMethod>
    Public Sub Frame_AutoRejectsCategoricalAmbiguity()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {"M", "F"}, New Object() {"F", "M"}),
            New SpreadsheetDataFrameParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsNotNull(Issue(parsed, "header.ambiguous"))
    End Sub

    <TestMethod>
    Public Sub Frame_DuplicateHeadingsCaseInsensitiveRejected()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {"Group", "group"}, New Object() {1, 2}))
        Assert.IsNotNull(Issue(parsed, "header.duplicate"))
        Assert.AreEqual(1, Issue(parsed, "header.duplicate").SelectedRow)
        Assert.AreEqual(2, Issue(parsed, "header.duplicate").SelectedColumn)
    End Sub

    <TestMethod>
    Public Sub Frame_EmptyHeaderRejected()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {Nothing, "Y"}, New Object() {1, 2}))
        Assert.IsNotNull(Issue(parsed, "header.invalid"))
    End Sub

    <TestMethod>
    Public Sub Frame_NonFiniteNumberRejectedWithAbsoluteCoordinates()
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(Source(New Object() {"Value"}, New Object() {Double.NaN}))
        Dim issueFound = Issue(parsed, "cell.invalid-value")
        Assert.IsNotNull(issueFound)
        Assert.AreEqual(2, issueFound.SelectedRow)
        Assert.AreEqual(13, issueFound.WorksheetRow)
        Assert.AreEqual(2, issueFound.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Frame_NonrectangularRangeReturnsValidationIssue()
        Dim input = Source(New Object() {1, 2}, New Object() {3})
        Dim parsed = RectangularCategoricalParsers.ParseDataFrame(input)
        Assert.IsNotNull(Issue(parsed, "range.not-rectangular"))
    End Sub

    <TestMethod>
    Public Sub Category_VectorPreservesDistinctTextNumericAndBooleanLevels()
        Dim input = Source(New Object() {1}, New Object() {"1"}, New Object() {True}, New Object() {1})
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(input)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(4, parsed.Data.Count)
        Assert.AreEqual(3, parsed.Data.Levels.Count)
        Assert.AreNotEqual(parsed.Data.Levels(0).Key, parsed.Data.Levels(1).Key)
        Assert.AreNotEqual(parsed.Data.Levels(1).Key, parsed.Data.Levels(2).Key)
        Assert.AreEqual(15, parsed.Data.Observations(3).WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Category_RowOrientationWithRequiredHeader()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {"Group", "a", "b", "a"}),
            New SpreadsheetCategoricalVectorParseOptions With {
                .Orientation = SpreadsheetVectorOrientation.Row, .Header = SpreadsheetHeaderMode.Required})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("Group", parsed.Data.Name)
        Assert.AreEqual(3, parsed.Data.Count)
        Assert.AreEqual(2, parsed.Data.Levels.Count)
        Assert.AreEqual(5, parsed.Data.Observations(2).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Category_InputConstructorAssignsNameRatherThanParameter()
        Dim named As New SpreadsheetCategoricalVectorInput(
            "Group", Array.Empty(Of SpreadsheetCategoricalObservation)(), Array.Empty(Of SpreadsheetCategoryValue)())
        Assert.AreEqual("Group", named.Name)

        Dim unnamed As New SpreadsheetCategoricalVectorInput(
            Nothing, Array.Empty(Of SpreadsheetCategoricalObservation)(), Array.Empty(Of SpreadsheetCategoryValue)())
        Assert.AreEqual(String.Empty, unnamed.Name)
    End Sub

    <TestMethod>
    Public Sub Category_TrueBlankSkipIsNotEmptyStringSkipByDefault()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {Nothing}, New Object() {""}, New Object() {"A"}))
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.Count)
        Assert.AreEqual(2, parsed.Data.Levels.Count)
        Assert.IsNotNull(Issue(parsed, "category.missing-skipped"))
    End Sub

    <TestMethod>
    Public Sub Category_ExplicitWhitespaceMissingPolicy()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {"   "}, New Object() {"A"}),
            New SpreadsheetCategoricalVectorParseOptions With {
                .CellPolicy = New SpreadsheetCellPolicyOptions With {
                    .MissingText = SpreadsheetMissingTextPolicy.BlankOrWhitespaceText}})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(1, parsed.Data.Count)
    End Sub

    <TestMethod>
    Public Sub Category_LiteralErrorLikeTextRemainsCategory()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {"#N/A"}, New Object() {"A"}))
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("#N/A", parsed.Data.Levels(0).DisplayText)
    End Sub

    <TestMethod>
    Public Sub Category_ExcelErrorRejectedAndReported()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {SpreadsheetCellValue.ErrorCell("#N/A")}, New Object() {"A"}))
        Assert.IsNotNull(Issue(parsed, "cell.excel-error"))
        Assert.AreEqual(12, Issue(parsed, "cell.excel-error").WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub Category_ExcelErrorCanBeExplicitlySkipped()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {SpreadsheetCellValue.ErrorCell("#N/A")}, New Object() {"A"}),
            New SpreadsheetCategoricalVectorParseOptions With {
                .CellPolicy = New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing}})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(1, parsed.Data.Count)
    End Sub

    <TestMethod>
    Public Sub Category_AutoHeaderIsRejectedBecauseTextCanBeData()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {"Group"}, New Object() {"A"}),
            New SpreadsheetCategoricalVectorParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsNotNull(Issue(parsed, "header.ambiguous"))
    End Sub

    <TestMethod>
    Public Sub Category_ExplicitMissingRejectsInsteadOfShifting()
        Dim parsed = RectangularCategoricalParsers.ParseCategoricalVector(
            Source(New Object() {"A"}, New Object() {Nothing}),
            New SpreadsheetCategoricalVectorParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Reject})
        Assert.IsNotNull(Issue(parsed, "category.missing"))
    End Sub

    <TestMethod>
    Public Sub CountTable_LabelledMatrixAndTotals()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "Yes", "No"},
            New Object() {"Control", 4, 6},
            New Object() {"Treated", 8, 2}))
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.RowCount)
        Assert.AreEqual(2, parsed.Data.ColumnCount)
        Assert.AreEqual(4L, parsed.Data.Count(0, 0))
        Assert.AreEqual(20L, parsed.Data.GrandTotal)
        CollectionAssert.AreEqual(New Long() {10, 10}, parsed.Data.RowTotals)
        CollectionAssert.AreEqual(New Long() {12, 8}, parsed.Data.ColumnTotals)
    End Sub

    <TestMethod>
    Public Sub CountTable_DefensiveCopiesDoNotChangeContract()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", 1, 2}, New Object() {"Y", 3, 4}))
        Dim copy = parsed.Data.Counts
        copy(0)(0) = 9999
        Assert.AreEqual(1L, parsed.Data.Count(0, 0))
    End Sub

    <TestMethod>
    Public Sub CountTable_RejectsNegativeFractionalTextAndExcelErrors()
        Dim negative = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", -1, 2}, New Object() {"Y", 3, 4}))
        Assert.IsNotNull(Issue(negative, "contingency.invalid-count"))
        Dim fractional = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", 1.5R, 2}, New Object() {"Y", 3, 4}))
        Assert.IsNotNull(Issue(fractional, "contingency.invalid-count"))
        Dim textValue = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", "1", 2}, New Object() {"Y", 3, 4}))
        Assert.IsNotNull(Issue(textValue, "contingency.invalid-count"))
        Dim excelError = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", SpreadsheetCellValue.ErrorCell("#N/A"), 2}, New Object() {"Y", 3, 4}))
        Assert.IsNotNull(Issue(excelError, "cell.excel-error"))
    End Sub

    <TestMethod>
    Public Sub CountTable_OptionalInvariantNumericText()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", "2", 1}, New Object() {"Y", 1, 2}),
            New SpreadsheetContingencyCountParseOptions With {
                .CellPolicy = New SpreadsheetCellPolicyOptions With {.NumericText = SpreadsheetNumericTextPolicy.Invariant}})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2L, parsed.Data.Count(0, 0))
    End Sub

    <TestMethod>
    Public Sub CountTable_BlankNotZeroWithoutOptIn()
        Dim input = Source(New Object() {Nothing, "A", "B"},
                           New Object() {"X", Nothing, 2}, New Object() {"Y", 3, 4})
        Dim reject = RectangularCategoricalParsers.ParseContingencyCounts(input)
        Assert.IsNotNull(Issue(reject, "contingency.missing"))
        Dim optIn = RectangularCategoricalParsers.ParseContingencyCounts(input,
            New SpreadsheetContingencyCountParseOptions With {.TreatMissingAsZero = True})
        Assert.IsTrue(optIn.Success)
        Assert.IsNotNull(Issue(optIn, "contingency.missing-zero"))
        Assert.AreEqual(0L, optIn.Data.Count(0, 0))
    End Sub

    <TestMethod>
    Public Sub CountTable_RejectsDuplicateLabels()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "A"}, New Object() {"X", 1, 2}, New Object() {"Y", 3, 4}))
        Assert.IsNotNull(Issue(parsed, "category.duplicate"))
    End Sub

    <TestMethod>
    Public Sub CountTable_ZeroGrandTotalRejected()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"}, New Object() {"X", 0, 0}, New Object() {"Y", 0, 0}))
        Assert.IsNotNull(Issue(parsed, "contingency.empty"))
    End Sub

    <TestMethod>
    Public Sub CountTable_UnlabelledCountsGetGeneratedLevels()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {1, 2}, New Object() {3, 4}),
            New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("Row 1", parsed.Data.RowLevels(0).DisplayText)
        Assert.AreEqual("Column 2", parsed.Data.ColumnLevels(1).DisplayText)
    End Sub

    <TestMethod>
    Public Sub CountTable_ExcessivelyPreciseCountsRejected()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyCounts(Source(
            New Object() {Nothing, "A", "B"},
            New Object() {"X", 9007199254740992.0R, 1}, New Object() {"Y", 1, 1}))
        Assert.IsNotNull(Issue(parsed, "contingency.invalid-count"))
    End Sub

    <TestMethod>
    Public Sub Pairs_StableFirstAppearanceRowsAndColumns()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {"X", "Yes"}, New Object() {"Y", "No"},
            New Object() {"X", "No"}, New Object() {"Y", "Yes"}, New Object() {"X", "Yes"}))
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("X", parsed.Data.RowLevels(0).DisplayText)
        Assert.AreEqual("Yes", parsed.Data.ColumnLevels(0).DisplayText)
        Assert.AreEqual(2L, parsed.Data.Count(0, 0))
        Assert.AreEqual(1L, parsed.Data.Count(0, 1))
        Assert.AreEqual(5L, parsed.Data.GrandTotal)
    End Sub

    <TestMethod>
    Public Sub Pairs_IncompletePairSkippedWithoutRepacking()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {"A", "Yes"}, New Object() {Nothing, "No"},
            New Object() {"B", "No"}, New Object() {"A", Nothing}),
            New SpreadsheetContingencyPairsParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2L, parsed.Data.GrandTotal)
        Assert.AreEqual(2, parsed.Validation.Issues.Where(Function(i) i.Code = "contingency.incomplete-skipped").Count())
    End Sub

    <TestMethod>
    Public Sub Pairs_IncompleteRejectedByDefault()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {"A", "Yes"}, New Object() {Nothing, "No"}, New Object() {"B", "No"}))
        Assert.IsNotNull(Issue(parsed, "contingency.incomplete"))
    End Sub

    <TestMethod>
    Public Sub Pairs_HeaderSkippedWhenExplicitlyRequested()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {"Arm", "Response"}, New Object() {"A", "Yes"}, New Object() {"B", "No"}),
            New SpreadsheetContingencyPairsParseOptions With {.Header = SpreadsheetHeaderMode.Required})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2L, parsed.Data.GrandTotal)
    End Sub

    <TestMethod>
    Public Sub Pairs_DuplicateColumnIndicesRejected()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(New Object() {"A", "X"}),
            New SpreadsheetContingencyPairsParseOptions With {.SecondColumnIndex = 0})
        Assert.IsNotNull(Issue(parsed, "options.invalid"))
    End Sub

    <TestMethod>
    Public Sub Pairs_DoesNotMergeTypedNumericAndTextCategories()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {1, "Y"}, New Object() {"1", "N"}, New Object() {1, "N"}, New Object() {"1", "Y"}))
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(2, parsed.Data.RowCount)
        Assert.AreNotEqual(parsed.Data.RowLevels(0).Key, parsed.Data.RowLevels(1).Key)
    End Sub

    <TestMethod>
    Public Sub Pairs_ErrorRejectedNotSilentlyOmitted()
        Dim parsed = RectangularCategoricalParsers.ParseContingencyPairs(Source(
            New Object() {"A", "Y"}, New Object() {SpreadsheetCellValue.ErrorCell("#REF!"), "N"},
            New Object() {"B", "N"}))
        Assert.IsNotNull(Issue(parsed, "cell.excel-error"))
    End Sub
End Class
