Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Output28PresentationTests
    Public Property TestContext As TestContext
    Public Shared Iterator Function Shapes() As IEnumerable(Of Object())
        For titles As Integer = 0 To 2 : For top As Integer = 0 To 2 : For leftCount As Integer = 0 To 2 : For foot As Integer = 0 To 2
            Yield New Object() {titles, top, leftCount, foot}
        Next : Next : Next : Next
    End Function
    Public Shared Function Model(titles As Integer, top As Integer, left As Integer, foot As Integer) As ResultTableOutputModel
        Dim values(titles + top + 3 + foot - 1, left + 3 - 1) As Object
        For row = 0 To values.GetLength(0) - 1
            For column = 0 To values.GetLength(1) - 1
                values(row, column) = row * 100 + column
            Next
        Next
        Return New ResultTableOutputModel(values, top, left, foot, {2, 0, 2, 99}, titles, True,
            {New ResultTableCellAddress(1, 1), New ResultTableCellAddress(3, 3), New ResultTableCellAddress(0, 0), New ResultTableCellAddress(1, 1)})
    End Function
    <DataTestMethod, DynamicData(NameOf(Shapes), DynamicDataSourceType.Method)>
    Public Sub AllSectionAxesMatchCanonicalRolesAndRetainLegacyFormatting(titles As Integer, top As Integer, left As Integer, foot As Integer)
        Dim model = Output28PresentationTests.Model(titles, top, left, foot)
        Dim intent = ResultTablePresentationIntent.Capture(New ResultTableOutputBlock(3, 4, model), "table-7", 7)
        Dim document As New OutputDocument({New OutputTableArtifact("table-7", model)})
        Dim semantic = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Assert.AreEqual(1 + top + titles, intent.FirstBodyRow)
        Assert.AreEqual(left - foot, intent.LegacyLastLeftHeaderColumn, "Preserved native footer/left-header coupling")
        Assert.AreEqual(model.RowCount - foot + 1, intent.FirstFooterRow(model.RowCount))
        Assert.AreEqual(model.RowCount - foot, intent.LastBodyRow(model.RowCount))
        Assert.AreEqual(3, intent.StartRow) : Assert.AreEqual(4, intent.StartColumn)
        Assert.AreEqual("table-7", intent.ArtifactId) : Assert.AreEqual(7, intent.SourceOrdinal.Value)
        For Each cell In semantic.Artifacts(0).Cells
            Assert.AreEqual(cell.Role, intent.RoleAt(cell.Row + 1, cell.Column + 1))
        Next
        CollectionAssert.AreEqual(New Integer() {2, 0, 2, 99}, intent.PvalueColumns.ToArray())
        Assert.AreEqual(4, intent.PvalueCells.Count)
        Assert.AreEqual(1 + top + titles, intent.PvalueRow(1))
        Assert.AreEqual(left + 3, intent.PvalueColumn(3))
        Assert.AreEqual(3 + titles + top, intent.SheetRow(intent.FirstBodyRow))
        Assert.AreEqual(4 + left, intent.SheetColumn(intent.PvalueColumn(1)))
        model.PvalueColumns.Clear() : model.PvalueCells.Clear() : model.Values(0, 0) = "mutation"
        Assert.AreEqual(4, intent.PvalueColumns.Count) : Assert.AreEqual(4, intent.PvalueCells.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(intent.PvalueColumns, IList(Of Integer)).Clear())
    End Sub
    <DataTestMethod>
    <DataRow(0, 0, 0, 0)> <DataRow(-1, -2, -3, -4)> <DataRow(99, 50, 40, 30)>
    Public Sub LegacyMetadataIsNotTurnedIntoNewGuiValidation(top As Integer, left As Integer, foot As Integer, titles As Integer)
        Dim model As New ResultTableOutputModel(New Object(,) {{New Object()}}, top, left, foot, {0, -3, 999}, titles)
        Dim intent = ResultTablePresentationIntent.Capture(New ResultTableOutputBlock(-1, 0, model))
        Assert.AreEqual(top, intent.HeaderTopRows) : Assert.AreEqual(left, intent.HeaderLeftColumns)
        Assert.AreEqual(foot, intent.FooterRows) : Assert.AreEqual(titles, intent.TitleRows)
        Assert.AreEqual(-1, intent.StartRow) : Assert.AreEqual(0, intent.StartColumn)
        Assert.AreEqual(3, intent.PvalueColumns.Count)
    End Sub
    <TestMethod>
    Public Sub OverflowRemainsLazyAndShapeDoesNotEnumerateCells()
        Dim intent As New ResultTablePresentationIntent(Integer.MaxValue, Integer.MaxValue, 0, 1)
        Assert.ThrowsException(Of OverflowException)(Sub()
                                                       Dim ignored = intent.FirstBodyRow
                                                   End Sub)
        Assert.ThrowsException(Of OverflowException)(Sub()
                                                       Dim ignored = intent.PvalueColumn(1)
                                                   End Sub)
        Dim empty As New ResultTableOutputModel(New Object(-1, -1) {})
        Dim captured = ResultTablePresentationIntent.Capture(New ResultTableOutputBlock(1, 1, empty))
        Assert.AreEqual(0, captured.RowCount) : Assert.AreEqual(0, captured.ColumnCount)
    End Sub
    <DataTestMethod>
    <DataRow(2, 0, False)> <DataRow(3, 1, True)> <DataRow(8, 2, False)> <DataRow(16, 0, True)>
    Public Sub SharedSettingsRemainExactCoreFormatsAndNumericValues(decimals As Integer, mode As Integer, upper As Boolean)
        Dim model As New ResultTableOutputModel(New Object(,) {{0.000000000000001R, 0.001R, 0.05R, 1.0R, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, True, "0,05", Nothing}}, pvalueColumns:={1, 2, 3})
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = decimals, .SmallValueDisplay = CType(mode, PValueSmallValueDisplay), .UseUpperBound = upper}
        Dim expectedFormat = PValuePresentation.BuildExcelNumberFormat(settings)
        Dim request = ResultTableWriteRequestFactory.Create(model, settings, 0.05, New SpreadsheetOutputTarget())
        Assert.AreEqual(PValuePresentation.BuildExcelNumberFormat(settings), request.PvalueNumberFormat)
        Assert.AreEqual(0.05, request.PvalueHighlightAlpha)
        Assert.AreEqual(GetType(Double), request.Values(0)(0).GetType())
        Assert.AreEqual(0.000000000000001R, CDbl(request.Values(0)(0)), 0.0)
        CollectionAssert.AreEqual(New Object() {"#N/A", "#Pinf", "#Ninf", True, "0,05", Nothing}, request.Values(0).Skip(4).ToArray())
        settings.DecimalPlaces = 4
        Assert.AreEqual(expectedFormat, request.PvalueNumberFormat)
        Assert.AreNotEqual(PValuePresentation.BuildExcelNumberFormat(settings), request.PvalueNumberFormat)
        If decimals = 2 AndAlso mode = 0 AndAlso Not upper Then Assert.AreEqual("[<0.01]""<0.01"";0.00", request.PvalueNumberFormat)
        If decimals = 8 AndAlso mode = 2 AndAlso Not upper Then Assert.AreEqual("0.00000000", request.PvalueNumberFormat)
    End Sub
    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub SupplementaryAndChartSectionsPreserveCanonicalOrderAndNoSpareRow(destination As Integer)
        Dim source As New OutputDocument({New OutputTableArtifact("first", New ResultTableOutputModel(New Object(,) {{1, 2}, {3, 4}})),
            New OutputDiagnosticArtifact("note", OutputDiagnosticSeverity.Warning, "bounded", "Synthetic warning"),
            New OutputTableArtifact("empty", New ResultTableOutputModel(New Object(-1, -1) {})),
            New OutputTableArtifact("last", New ResultTableOutputModel(New Object(,) {{5}}))})
        Dim target As New SpreadsheetOutputTarget With {.Destination = CType(destination, SpreadsheetOutputDestination), .StartRow = 3, .StartColumn = 4, .WorksheetName = "Result"}
        Dim layout = OutputLayoutPlanner.Create(source, target)
        Dim extras = {New ResultTableOutputModel(New Object(,) {{6, 7, 8}}), New ResultTableOutputModel(New Object(,) {{9}, {10}})}
        Dim bounds = ResultTableSequencePolicy.PreflightBounds(layout, extras, 1048576, 16384)
        Assert.AreEqual("R3C4:R11C6", bounds.ToString())
        Dim regions = ResultTableSequencePolicy.OrderedRegions(layout, extras, {"chart"}, 1048576, 16384)
        CollectionAssert.AreEqual(New String() {"first", "last", "supplementary-0", "supplementary-1", "chart"}, regions.Select(Function(x) x.ArtifactId).ToArray())
        Assert.AreEqual("R8C4:R8C6", regions(2).Bounds.ToString())
        Assert.AreEqual("R10C4:R11C4", regions(3).Bounds.ToString())
        Assert.IsNull(regions(4).Bounds) : Assert.IsTrue(regions(4).MayMutate)
        Assert.ThrowsException(Of ArgumentException)(Sub() ResultTableSequencePolicy.PreflightBounds(layout, extras, 11, 16384))
        Assert.ThrowsException(Of ArgumentException)(Sub() ResultTableSequencePolicy.OrderedRegions(layout, extras, Nothing, 11, 16384))
        Assert.ThrowsException(Of ArgumentException)(Sub() ResultTableSequencePolicy.PreflightBounds(layout, {CType(Nothing, ResultTableOutputModel)}, 1048576, 16384))
    End Sub

    <TestMethod>
    Public Sub LastValidCellAndOverflowRemainDifferentSharedAndLegacyContracts()
        Dim document As New OutputDocument({New OutputTableArtifact("last", New ResultTableOutputModel(New Object(,) {{1}}))})
        Dim target As New SpreadsheetOutputTarget With {.StartRow = 1048576, .StartColumn = 16384}
        Dim layout = OutputLayoutPlanner.Create(document, target)
        Assert.AreEqual("R1048576C16384:R1048576C16384", layout.UsedBounds.ToString())
        Assert.ThrowsException(Of ArgumentException)(Sub() ResultTableSequencePolicy.PreflightBounds(layout, Nothing, 1048576, 16384))
        target.StartRow += 1
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() OutputLayoutPlanner.Create(document, target))
        Dim plan = ResultTablePresentationIntent.Capture(New ResultTableOutputBlock(3, 4, New ResultTableOutputModel(New Object(,) {{1}})))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() plan.RoleAt(0, 1))
    End Sub
End Class
