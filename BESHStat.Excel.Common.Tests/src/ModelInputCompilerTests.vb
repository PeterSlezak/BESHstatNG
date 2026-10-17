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
Public Class ModelInputCompilerTests
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
                ElseIf TypeOf value Is Integer OrElse TypeOf value Is Double OrElse TypeOf value Is Long Then
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                Else
                    cells.Add(SpreadsheetCellValue.TextCell(Convert.ToString(value, CultureInfo.InvariantCulture)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {
            .WorkbookId = "wb-1", .WorkbookName = "Trial.xlsx", .WorksheetId = "sh-1",
            .WorksheetName = "Clinical", .SelectionRevision = "rev-4", .StartRow = 20, .StartColumn = 4,
            .Address = "D20:F99", .Rows = rows, .RowCount = rows.Count,
            .ColumnCount = If(rows.Count = 0, 0, rows(0).Count)}
    End Function

    Private Shared Function Issue(result As SpreadsheetTypedParseResult(Of SpreadsheetModelInput), code As String) As SpreadsheetValidationIssue
        Return result.Validation.Issues.FirstOrDefault(Function(i) i.Code = code)
    End Function

    Private Shared Function Simple() As SpreadsheetRangeData
        Return Source(New Object() {"Y", "Dose", "Arm"}, New Object() {2, 1, "Control"},
                      New Object() {4, 2, "Active"}, New Object() {5, 3, "Control"},
                      New Object() {9, 4, "Active"}, New Object() {10, 5, "Control"})
    End Function

    <TestMethod>
    Public Sub NumericMainEffect_HostNeutralCoordinatesAndIntercept()
        Dim parsed = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "A"})
        Assert.IsTrue(parsed.Success, String.Join(" | ", parsed.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual(5, parsed.Data.ObservationCount)
        Assert.AreEqual(2, parsed.Data.DesignColumnCount)
        Assert.AreEqual(2, parsed.Data.MatrixRank)
        Assert.AreEqual("(Intercept)", parsed.Data.PredictorNames(0))
        Assert.AreEqual("Dose", parsed.Data.PredictorNames(1))
        Assert.AreEqual(4.0R, parsed.Data.DesignMatrix(3, 1))
        Assert.AreEqual(9.0R, parsed.Data.Response(3))
        Assert.AreEqual(3, parsed.Data.SourceRows(1).SelectedRow)
        Assert.AreEqual(21, parsed.Data.SourceRows(0).WorksheetRow)
        Assert.AreEqual("wb-1", parsed.Data.WorkbookId)
        Assert.AreEqual("sh-1", parsed.Data.WorksheetId)
        Assert.AreEqual("rev-4", parsed.Data.SelectionRevision)
    End Sub

    <TestMethod>
    Public Sub QuotedNameAndSubset_DoesNotReadUnreferencedBadCells()
        Dim srce = Source(New Object() {"Target", "Dose mg", "Ignored"},
                            New Object() {1, 10, SpreadsheetCellValue.ErrorCell("HOST_ERR", "n47")},
                            New Object() {2, 20, SpreadsheetCellValue.ErrorCell("#N/A")},
                            New Object() {3, 30, Nothing})
        Dim parsed = SpreadsheetModelInputCompiler.Compile(srce, New SpreadsheetModelParseOptions With {
            .FormulaText = "'Dose mg'"})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(3, parsed.Data.ObservationCount)
        Assert.AreEqual(1, parsed.Data.RequiredPredictorNames.Count)
        Assert.AreEqual("Dose mg", parsed.Data.RequiredPredictorNames(0))
    End Sub

    <TestMethod>
    Public Sub TextFactor_TypedEncodingAndReferenceOmission()
        Dim parsed = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "A + factor(B)", .CategoricalColumnIndices = New Integer() {2}})
        Assert.IsTrue(parsed.Success, String.Join(" | ", parsed.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual(3, parsed.Data.DesignColumnCount)
        Assert.AreEqual(1, parsed.Data.CategoricalColumns.Count)
        Dim factor = parsed.Data.CategoricalColumns(0)
        Assert.AreEqual("Arm", factor.ColumnName)
        Assert.AreEqual(2, factor.Levels.Count)
        Assert.AreEqual("Control", factor.Levels(0).Value.TextValue)
        Assert.AreEqual(1.0R, factor.Levels(0).Code)
        Assert.AreEqual("Active", factor.Levels(1).Value.TextValue)
        Assert.AreEqual(2.0R, factor.Levels(1).Code)
        Assert.AreEqual(0.0R, parsed.Data.DesignMatrix(0, 2))
        Assert.AreEqual(1.0R, parsed.Data.DesignMatrix(1, 2))
        Assert.AreEqual(1.0R, parsed.Data.RawPredictorMatrix(0, 1))
    End Sub

    <TestMethod>
    Public Sub FactorWithNoInterceptRetainsAllLevels()
        Dim result = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "factor(B)", .CategoricalColumnIndices = New Integer() {2},
            .IncludeIntercept = False})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(2, result.Data.DesignColumnCount)
        Assert.AreEqual(1.0R, result.Data.DesignMatrix(0, 0))
        Assert.AreEqual(0.0R, result.Data.DesignMatrix(0, 1))
    End Sub

    <TestMethod>
    Public Sub FactorInteractionUsesCoreDesignExpansion()
        Dim data = Source(New Object() {"Y", "X", "Group"},
                          New Object() {1, 1, "A"}, New Object() {3, 2, "A"},
                          New Object() {5, 3, "A"}, New Object() {2, 1, "B"},
                          New Object() {4, 2, "B"}, New Object() {6, 3, "B"})
        Dim result = SpreadsheetModelInputCompiler.Compile(data, New SpreadsheetModelParseOptions With {
            .FormulaText = "A + factor(B) + A:factor(B)", .CategoricalColumnIndices = New Integer() {2}})
        Assert.IsTrue(result.Success, String.Join(" | ", result.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual(4, result.Data.DesignColumnCount)
        Assert.AreEqual(4, result.Data.MatrixRank)
        Assert.AreEqual(0.0R, result.Data.DesignMatrix(0, 3))
        Assert.AreEqual(2.0R, result.Data.DesignMatrix(4, 3))
    End Sub

    <TestMethod>
    Public Sub PolynomialTermReusesCoreParser()
        Dim result = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "A + A^2"})
        Assert.IsTrue(result.Success)
        Assert.AreEqual(3, result.Data.DesignColumnCount)
        Assert.AreEqual(9.0R, result.Data.DesignMatrix(2, 2))
    End Sub

    <TestMethod>
    Public Sub MissingCompleteCasesKeepOriginalSourceRowIds()
        Dim data = Source(New Object() {"Y", "X", "Other"},
                          New Object() {1, 1, 9}, New Object() {2, Nothing, 9},
                          New Object() {3, 3, 9}, New Object() {4, 4, 9})
        Dim options As New SpreadsheetModelParseOptions With {
            .FormulaText = "A", .Missing = SpreadsheetIncompleteObservationPolicy.Skip}
        Dim parsed = SpreadsheetModelInputCompiler.Compile(data, options)
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual(3, parsed.Data.ObservationCount)
        Assert.IsNotNull(Issue(parsed, "model.incomplete-skipped"))
        Assert.AreEqual(22, Issue(parsed, "model.incomplete-skipped").WorksheetRow)
        Assert.AreEqual(23, parsed.Data.SourceRows(1).WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub MissingDefaultRejectsWithCoordinates()
        Dim data = Source(New Object() {"Y", "X"}, New Object() {1, 1},
                          New Object() {2, Nothing}, New Object() {3, 3})
        Dim parsed = SpreadsheetModelInputCompiler.Compile(data, New SpreadsheetModelParseOptions With {.FormulaText = "A"})
        Assert.IsFalse(parsed.Success)
        Assert.AreEqual(22, Issue(parsed, "model.incomplete").WorksheetRow)
    End Sub

    <TestMethod>
    Public Sub ErrorCellNotSilentlyCoercedToTextFactor()
        Dim data = Source(New Object() {"Y", "Group"},
                          New Object() {1, "A"}, New Object() {2, SpreadsheetCellValue.ErrorCell("HOST_47", "47")},
                          New Object() {3, "B"})
        Dim parsed = SpreadsheetModelInputCompiler.Compile(data, New SpreadsheetModelParseOptions With {
            .FormulaText = "factor(A)", .CategoricalColumnIndices = New Integer() {1},
            .Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsFalse(parsed.Success)
        Assert.AreEqual(22, Issue(parsed, "model.cell-error").WorksheetRow)
        Assert.AreEqual(5, Issue(parsed, "model.cell-error").WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub CategoryRequiresExplicitFactorRoleInFormula()
        Dim parsed = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "B", .CategoricalColumnIndices = New Integer() {2}})
        Assert.IsFalse(parsed.Success)
        Assert.IsNotNull(Issue(parsed, "model.factor-required"))
    End Sub

    <TestMethod>
    Public Sub WrongOrUnknownFormulaCannotLeakCoreException()
        Dim unknown = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {.FormulaText = "Q"})
        Assert.IsFalse(unknown.Success)
        Assert.IsNotNull(Issue(unknown, "model.formula"))
        Dim leftHand = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {.FormulaText = "Y ~ A"})
        Assert.IsFalse(leftHand.Success)
        Assert.IsNotNull(Issue(leftHand, "model.rhs-only"))
    End Sub

    <TestMethod>
    Public Sub InvalidRolesAreRejectedBeforeCompilation()
        Dim overlap = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .PredictorColumnIndices = New Integer() {0, 1}})
        Assert.IsNotNull(Issue(overlap, "model.predictor-role"))
        Dim duplicate = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .PredictorColumnIndices = New Integer() {1, 1}})
        Assert.IsNotNull(Issue(duplicate, "model.predictor-role"))
        Dim wrongFactor = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .CategoricalColumnIndices = New Integer() {0}})
        Assert.IsNotNull(Issue(wrongFactor, "model.category-role"))
    End Sub

    <TestMethod>
    Public Sub RankDeficiencyWarnsOrRejectsAsRequested()
        Dim srce = Source(New Object() {"Y", "X", "Duplicate"}, New Object() {1, 1, 2},
                            New Object() {2, 2, 4}, New Object() {3, 3, 6}, New Object() {4, 4, 8})
        Dim warning = SpreadsheetModelInputCompiler.Compile(srce)
        Assert.IsTrue(warning.Success)
        Assert.AreEqual(2, warning.Data.MatrixRank)
        Assert.AreEqual(3, warning.Data.DesignColumnCount)
        Assert.AreEqual(SpreadsheetValidationSeverity.Warning, Issue(warning, "model.rank-deficient").Severity)
        Dim rejected = SpreadsheetModelInputCompiler.Compile(srce,
            New SpreadsheetModelParseOptions With {.RejectRankDeficiency = True})
        Assert.IsFalse(rejected.Success)
        Assert.IsNotNull(Issue(rejected, "model.rank-deficient"))
    End Sub

    <TestMethod>
    Public Sub DefensiveSnapshotMatricesCannotBeMutatedThroughPublicProperties()
        Dim parsed = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {.FormulaText = "A"})
        Assert.IsTrue(parsed.Success)
        Dim matrix = parsed.Data.DesignMatrix
        Dim response = parsed.Data.Response
        Dim raw = parsed.Data.RawPredictorMatrix
        matrix(0, 1) = 500.0R
        response(0) = 500.0R
        raw(0, 0) = 500.0R
        Assert.AreEqual(1.0R, parsed.Data.DesignMatrix(0, 1))
        Assert.AreEqual(2.0R, parsed.Data.Response(0))
        Assert.AreEqual(1.0R, parsed.Data.RawPredictorMatrix(0, 0))
    End Sub

    <TestMethod>
    Public Sub NumericStringCoercionIsOptInAndCultureExplicit()
        Dim data = Source(New Object() {"Y", "Dose"}, New Object() {1, "1,5"},
                          New Object() {2, "2,5"}, New Object() {3, "3,5"})
        Dim refused = SpreadsheetModelInputCompiler.Compile(data)
        Assert.IsNotNull(Issue(refused, "model.not-numeric"))
        Dim policy As New SpreadsheetCellPolicyOptions With {
            .NumericText = SpreadsheetNumericTextPolicy.ExplicitCulture, .NumericCultureName = "sk-SK"}
        Dim accepted = SpreadsheetModelInputCompiler.Compile(data,
            New SpreadsheetModelParseOptions With {.CellPolicy = policy})
        Assert.IsTrue(accepted.Success)
        Assert.AreEqual(2.5R, accepted.Data.DesignMatrix(1, 1), 0.000001R)
    End Sub

    <TestMethod>
    Public Sub NoHeaderAndSwappedResponseRoles()
        Dim data = Source(New Object() {1, 10}, New Object() {2, 20}, New Object() {3, 30})
        Dim parsed = SpreadsheetModelInputCompiler.Compile(data, New SpreadsheetModelParseOptions With {
            .Header = SpreadsheetHeaderMode.None, .ResponseColumnIndex = 1, .FormulaText = "A"})
        Assert.IsTrue(parsed.Success)
        Assert.AreEqual("Column 2", parsed.Data.ResponseName)
        Assert.AreEqual(20.0R, parsed.Data.Response(1))
        Assert.AreEqual(1, parsed.Data.SourceRows(0).SelectedRow)
    End Sub

    <TestMethod>
    Public Sub AbsoluteWorksheetLettersMustBeExplicitlyEnabled()
        Dim off = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "E", .AllowRelativeColumnLetters = False})
        Assert.IsNotNull(Issue(off, "model.formula"))
        Dim onResult = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "E", .AllowRelativeColumnLetters = False, .AllowAbsoluteColumnLetters = True})
        Assert.IsTrue(onResult.Success)
        Assert.AreEqual("Dose", onResult.Data.PredictorNames(1))
    End Sub

    <TestMethod>
    Public Sub ExplicitFactorReferenceUsesThePreservedCodebook()
        Dim result = SpreadsheetModelInputCompiler.Compile(Simple(), New SpreadsheetModelParseOptions With {
            .FormulaText = "factor(B, ref=2)", .CategoricalColumnIndices = New Integer() {2}})
        Assert.IsTrue(result.Success, String.Join(" | ", result.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual("Active", result.Data.CategoricalColumns(0).Levels(1).Value.TextValue)
        Assert.AreEqual(1.0R, result.Data.DesignMatrix(0, 1))
        Assert.AreEqual(0.0R, result.Data.DesignMatrix(1, 1))
    End Sub

    <TestMethod>
    Public Sub NumericFactorsPreserveOriginalValuesAndCoreReference()
        Dim data = Source(New Object() {"Y", "Class"},
                          New Object() {1, 10}, New Object() {2, 5},
                          New Object() {3, 10}, New Object() {4, 5})
        Dim result = SpreadsheetModelInputCompiler.Compile(data, New SpreadsheetModelParseOptions With {
            .FormulaText = "factor(A, ref=5)", .CategoricalColumnIndices = New Integer() {1}})
        Assert.IsTrue(result.Success, String.Join(" | ", result.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual(10.0R, result.Data.CategoricalColumns(0).Levels(0).Code)
        Assert.AreEqual(5.0R, result.Data.CategoricalColumns(0).Levels(1).Code)
        Assert.AreEqual(1.0R, result.Data.DesignMatrix(0, 1))
        Assert.AreEqual(0.0R, result.Data.DesignMatrix(1, 1))
    End Sub

    <TestMethod>
    Public Sub MinimumRowsAndDesignWidthAreBounded()
        Dim rows = Source(New Object() {"Y", "X"}, New Object() {1, 1}, New Object() {2, 2})
        Dim few = SpreadsheetModelInputCompiler.Compile(rows, New SpreadsheetModelParseOptions With {
            .MinimumCompleteRows = 3})
        Assert.IsNotNull(Issue(few, "model.too-few"))
        Dim width = SpreadsheetModelInputCompiler.Compile(rows, New SpreadsheetModelParseOptions With {
            .MaximumExpandedColumns = 1})
        Assert.IsNotNull(Issue(width, "model.design-width"))
    End Sub
End Class
