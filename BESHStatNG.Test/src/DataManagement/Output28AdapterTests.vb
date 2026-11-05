Option Explicit On
Option Strict On
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.AppInfrastructure

<TestClass>
Public Class Output28AdapterTests
    Private Class NativeRecorder
        Inherits ExcelDnaResultWriter
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Public ReadOnly Presentations As New List(Of ResultTablePresentationIntent)
        Public ReadOnly Formats As New List(Of String)
        Public ReadOnly Alphas As New List(Of Double?)
        Public ReadOnly Fits As New List(Of Boolean)
        Public Sub New()
            MyBase.New(3, 4)
        End Sub
        Friend Overrides Sub EnsureOutputWorksheet()
        End Sub
        Friend Overrides Sub WriteNativeOutputBlock(block As ResultTableOutputBlock, presentation As ResultTablePresentationIntent,
            numberFormat As String, alpha As Double?, autoFit As Boolean, Optional bounds As OutputRectangle = Nothing)
            Blocks.Add(block) : Presentations.Add(presentation)
            Formats.Add(numberFormat) : Alphas.Add(alpha) : Fits.Add(autoFit)
            Assert.AreEqual(block.StartRow, presentation.StartRow)
            Assert.AreEqual(block.StartColumn, presentation.StartColumn)
            Assert.AreEqual(block.Model.RowCount, presentation.RowCount)
        End Sub
    End Class

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub RealCoreWriterConvertsNonzeroBoundArraysAndPreservesPointers(tall As Boolean)
        Dim writer As New NativeRecorder
        Dim vector = Array.CreateInstance(GetType(Object), {3}, {5})
        vector.SetValue(2.0, 5) : vector.SetValue(True, 6) : vector.SetValue(Double.NaN, 7)
        writer.write(vector, tall)
        Dim model = writer.Blocks(0).Model
        Assert.AreEqual(If(tall, 3, 1), model.RowCount)
        Assert.AreEqual(If(tall, 1, 3), model.ColumnCount)
        Assert.AreEqual(If(tall, 6, 4), writer.RowID) : Assert.AreEqual(4, writer.ColID)
        Assert.AreEqual(2.0, model.Values(0, 0))
        Assert.AreEqual(True, model.Values(If(tall, 1, 0), If(tall, 0, 1)))
        Assert.AreEqual("#N/A", model.Values(If(tall, 2, 0), If(tall, 0, 2)))
        Dim matrix = Array.CreateInstance(GetType(Object), {2, 2}, {2, -1})
        matrix.SetValue("a", 2, -1) : matrix.SetValue("b", 2, 0) : matrix.SetValue(7, 3, -1) : matrix.SetValue(False, 3, 0)
        writer.write(matrix)
        CollectionAssert.AreEqual(New Object() {"a", "b", 7, False}, writer.Blocks(1).Model.Values.Cast(Of Object)().ToArray())
        writer.write(Nothing) : writer.write("scalar")
        Assert.AreEqual(4, writer.Blocks.Count)
        Assert.AreEqual("scalar", writer.Blocks(3).Model.Values(0, 0))
    End Sub

    <TestMethod>
    Public Sub SingleSharedRequestSnapshotsPresentationSettingsAndAdvancesOnlyOnce()
        Dim model = Output28PresentationTests.Model(1, 1, 1, 1)
        Dim settings As New PValuePresentationSettings With {.DecimalPlaces = 3}
        Dim request = ResultTableWriteRequestFactory.Create(model, settings, 0.01, New SpreadsheetOutputTarget())
        request.AutoFit = False
        Dim expected = request.PvalueNumberFormat
        settings.DecimalPlaces = 9
        Dim writer As New NativeRecorder : writer.WriteSharedRequest(request)
        Assert.AreEqual(expected, writer.Formats(0)) : Assert.AreEqual(0.01, writer.Alphas(0).Value)
        Assert.IsFalse(writer.Fits(0))
        Assert.AreEqual(3 + model.RowCount, writer.RowID)
        Assert.AreEqual(1, writer.Presentations(0).HeaderLeftColumns)
        Assert.AreEqual(4, writer.Presentations(0).PvalueCells.Count)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub ShippingSharedRunConsumesCommonCommandsSupplementAndChart(destination As Integer)
        Dim target As New SpreadsheetOutputTarget With {.Destination = CType(destination, SpreadsheetOutputDestination), .WorksheetName = "Resolved", .StartRow = 3, .StartColumn = 4}
        Dim model = Output28PresentationTests.Model(2, 2, 2, 2)
        Dim result As New SpreadsheetAnalysisResult(SpreadsheetAnalysisKind.OneWayAnova, New List(Of ResultTableOutputModel) From {model, Output28PresentationTests.Model(0, 0, 0, 0)}, New Dictionary(Of String, Double), Nothing)
        Dim writer As New NativeRecorder
        Dim chartWrites As Integer = 0, reads As Integer = 0
        Dim host As New ExcelDnaSharedOutputHost With {
            .Acquire = Function(run) writer, .Target = Function(w) target,
            .Identities = Function(w) Tuple.Create("book", "sheet"),
            .Charts = Function(w, width) New List(Of Action) From {Sub() chartWrites += 1},
            .InspectCharts = Function(w, intent) New ExcelDnaChartSafetySnapshot(0, "shapes"),
            .ConfirmCharts = Function(observation) False,
            .Inspect = Function(w, intent)
                           reads += 1
                           Return New WriteInspection(intent, OutputOccupancy.Empty, "same", Nothing)
                       End Function,
            .Confirm = Function(observation) False}
        Dim extra As New ResultTable : extra.SetBody(New Object(,) {{"supplement", 0.05}})
        Dim session As New ExcelDnaSharedOutputRun
        Dim outcome = session.Execute(result, New List(Of ResultTable) From {extra}, New PValuePresentationSettings, 0.05, host)
        Assert.AreEqual(OutputWriteState.Completed, outcome.State, outcome.Message)
        Assert.AreEqual(3, writer.Blocks.Count) : Assert.AreEqual(1, chartWrites)
        Assert.AreEqual("table-0", writer.Presentations(0).ArtifactId)
        Assert.AreEqual(2, writer.Presentations(0).TitleRows)
        Assert.AreEqual(4, writer.Presentations(0).PvalueCells.Count)
        Assert.AreEqual(writer.Blocks.Last().EndRow + 1, writer.RowID)
        Assert.AreEqual(2, reads)
        Assert.IsFalse(outcome.RollbackAttempted)
    End Sub

    <DataTestMethod> <DataRow("", "0.05")> <DataRow("sk-SK", "0,05")>
    Public Sub LegacyTextCoercionRemainsDifferentFromTypedProbability(cultureName As String, text As String)
        Dim culture = If(cultureName = "", CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo(cultureName))
        Assert.IsTrue(ExcelDnaResultWriter.LegacyPValueIsHighlighted(text, 0.05, culture))
        Dim probability As Double
        Assert.IsFalse(OutputCell.TextCell(text).TryGetProbability(probability))
        Assert.IsTrue(ExcelDnaResultWriter.LegacyPValueIsHighlighted(0.05, 0.05, culture))
        Assert.IsFalse(ExcelDnaResultWriter.LegacyPValueIsHighlighted(0.05 + 0.00000001, 0.05, culture))
        For Each value In New Object() {Nothing, True, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, "#N/A"}
            Assert.IsFalse(ExcelDnaResultWriter.LegacyPValueIsHighlighted(value, 0.05, culture))
        Next
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub LegacyMultiTableInsertionKeepsTitlesFootersAndNullSeparators(custom As Boolean)
        Dim first As New ResultTable : first.SetBody(New Object(,) {{0.05, "text"}, {0.01, True}})
        first.AddTitle("Title one") : first.AddTitle("Title two") : first.AddHeaderTopRow({"P", "Value"})
        first.AddHeaderLeftRow({"a", "b"}) : first.AddFootnote("Footnote") : first.AddPvalueCellToFormat(1, 1)
        Dim last As New ResultTable : last.SetBody(New Object(,) {{7}})
        Dim processor As New ProcessListofResultTables(New List(Of ResultTable) From {first, Nothing, last})
        Assert.AreEqual(first.TotalRows + 1, processor.TotRows("below"))
        Assert.AreEqual(first.TotalRows, processor.TotRows("beside"))
        Assert.AreEqual(first.TotalCols, processor.TotCols("below"))
        Assert.AreEqual(first.TotalCols + 1, processor.TotCols("beside"))
        If custom Then processor.SetSeparators(New List(Of Object(,)) From {New Object(,) {{"separator"}}, New Object(,) {{"separator2"}}})
        Dim writer As New NativeRecorder : processor.writeToSheet(writer, True)
        Assert.AreEqual(If(custom, 5, 3), writer.Blocks.Count)
        Assert.AreEqual(2, writer.Presentations(0).TitleRows)
        Assert.AreEqual(1, writer.Presentations(0).FooterRows)
        Assert.AreEqual(6, writer.Presentations(0).SheetRow(writer.Presentations(0).PvalueRow(1)))
        Assert.AreEqual(3 + first.TotalRows + 4, writer.RowID)
        Assert.AreEqual("Footnote", writer.Blocks(0).Model.Values(first.TotalRows - 1, 0))
    End Sub

    <TestMethod>
    Public Sub EmptyHomogeneityIsAnEmptyReportWithUnchangedLegacyMeasurement()
        Dim data As New MultiGroupsUnpairedData With {.X = {New Double() {1, 2, 4}, New Double() {2, 3, 5}}, .varNames = {"first", "second"}}
        Dim report = WindowsAssumptionReportAdapter.Prepare(data, New AssumptionReportOptions(AssumptionReportKind.Homogeneity, homogeneity:=HomogeneityMethods.None))
        Assert.AreEqual(0, report.Tables.Count)
        Dim processor As New ProcessListofResultTables(report.Tables)
        Assert.AreEqual(0, processor.TotRows) : Assert.AreEqual(-1, processor.TotRows + report.Tables.Count - 1)
        Dim writer As New NativeRecorder : processor.writeToSheet(writer, True)
        Assert.AreEqual(0, writer.Blocks.Count) : Assert.AreEqual(3, writer.RowID)
    End Sub

    <TestMethod>
    Public Sub CovarianceHeaderCorrectionStaysQuarantinedIncludingDuplicateAndMissingNames()
        Dim fixture = Regression09Fixture.Load("glm_gaussian_full.csv")
        Dim request = Regression10PolicyTests.Request(fixture, False, False, False)
        For Each names In New String()() {New String() {"y", "x", "x"}, New String() {Nothing, "x"}, New String() {}}
            Dim before = DirectCast(names.Clone(), String())
            Dim actual = WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(request, names)
            CollectionAssert.AreEqual(before, names)
            If actual.Length > 0 Then
                Assert.AreEqual("Intercept", actual(0))
                CollectionAssert.AreEqual(before.Skip(1).ToArray(), actual.Skip(1).ToArray())
            End If
        Next
        Assert.IsNull(WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(request, Nothing))
    End Sub
End Class
