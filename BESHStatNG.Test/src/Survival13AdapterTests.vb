Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.survival
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Survival13AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    Private Shared Sub SameWrittenTables(original As List(Of ResultTable), projected As List(Of ResultTable))
        Dim a As New MemoryWriter, b As New MemoryWriter
        Dim before As New ProcessListofResultTables(original), after As New ProcessListofResultTables(projected)
        before.writeToSheet(a, True) : after.writeToSheet(b, True)
        Assert.AreEqual(a.Blocks.Count, b.Blocks.Count)
        For i = 0 To a.Blocks.Count - 1
            Assert.AreEqual(a.Blocks(i).StartRow, b.Blocks(i).StartRow)
            Assert.AreEqual(a.Blocks(i).StartColumn, b.Blocks(i).StartColumn)
            CollectionAssert.AreEqual(a.Blocks(i).Model.Values, b.Blocks(i).Model.Values)
            CollectionAssert.AreEqual(a.Blocks(i).Model.PvalueColumns, b.Blocks(i).Model.PvalueColumns)
            CollectionAssert.AreEqual(a.Blocks(i).Model.PvalueCells, b.Blocks(i).Model.PvalueCells)
        Next
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub ActualKmAdapterDelegatesWithExactWriterAndPlotPayload(stratified As Boolean)
        For Each culture In {"en-US", "sk-SK"}
            Dim saved = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim records = Survival13Fixture.Records("KM|Peto|" & If(stratified, "strata", "groups"))
                Dim values(records.Count - 1, 3) As Object
                For i = 0 To records.Count - 1
                    values(i, 0) = records(i).strGroup : values(i, 1) = records(i).strStratum
                    values(i, 2) = records(i).Time : values(i, 3) = records(i).Censorship
                Next
                Dim table = CoreDataTable.FromObjectMatrix(values, {"g", "stratum", "time", "status"})
                table.RowIds = records.Select(Function(r) r.Index).ToArray()
                Dim raw As New DataObj : raw.LoadCoreDataTable(table, CharCols:=2)
                Dim intent = WindowsSurvivalWorkflowAdapter.Capture(raw, 2, records.Select(Function(r) r.strGroup).ToArray(),
                    records.Select(Function(r) r.strStratum).ToArray(), True, stratified,
                    If(stratified, SurvivalAnalysisMode.LogRank, SurvivalAnalysisMode.KaplanMeier), New SurvivalOptions With {.Weight = SurvivalWeight.Peto})
                Assert.IsTrue(intent.Validation.IsValid)
                CollectionAssert.AreEqual(raw.RowIds, intent.Input.Time.RowIds)
                Assert.IsNull(intent.Input.Provenance, "Raw arrays must not invent worksheet coordinates.")
                Dim model = intent.ExecuteAcquiredRecords(records), tables = model.wrapResults()
                SameWrittenTables(tables, SurvivalReportPolicy.Compose(intent, tables).LegacyTables())
                Dim expected As New List(Of String), actual As New List(Of String)
                Survival13Fixture.AddPlot(expected, model.GetKaplanMeierPlotData())
                Survival13Fixture.AddPlot(actual, SurvivalReportPolicy.Plot(model, 0.05, intent.Options).ToCoreData())
                CollectionAssert.AreEqual(expected, actual)
            Finally
                CultureInfo.CurrentCulture = saved
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub KmLegacyFractionalConversionIsObservedWithoutNewShippingRefusal()
        Dim raw As New DataObj
        raw.DataImportRawMatrix(New Object(,) {{1.0, 0.6}, {2.0, 0.0}, {3.0, 1.0}}, {"time", "status"}, firstSourceRow:=101, CharCols:=0)
        Dim input = WindowsSurvivalWorkflowAdapter.Capture(raw, 0, {"0", "0", "0"}, {"0", "0", "0"}, False, False,
            SurvivalAnalysisMode.KaplanMeier, New SurvivalOptions)
        Assert.IsFalse(input.Validation.IsValid)
        Dim err = String.Empty
        Dim converted = Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(raw.FinalData, 1))
        Assert.AreEqual(1, converted(0), "Historical GUI rounding is not portable status validation.")
        Dim records = Survival.CreatSurvivalData({1.0, 2.0, 3.0}, converted, {"0", "0", "0"}, {"0", "0", "0"}, err)
        Assert.AreEqual("", err)
        Assert.IsNotNull(input.ExecuteAcquiredRecords(records))
    End Sub

    <TestMethod>
    Public Sub SparseAreaPhysicalCoordinatesAreRetainedWithoutFirstAreaGeometry()
        Dim time(,) As Object = {{1.0}, {2.0}, {3.0}, {4.0}, {5.0}}
        Dim status(,) As Object = {{1.0}, {Nothing}, {0.0}, {1.0}}
        Dim table = ExcelDnaDataImporter.ImportPhysicalSelection("'Data'!A101:A105,D102:D105", {"A101:A105", "D102:D105"},
            Function(addresses)
                Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "survival13-physical"}
                selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("A101:A105", "Data", "Synthetic.xlsx"))
                selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("D102:D105", "Data", "Synthetic.xlsx"))
                Return ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
                    Function(reference) ExcelDnaSharedApplicationAdapter.SnapshotValues(reference,
                        If(reference.StartColumn = 1, time, status), Function(row, column) "General", Nothing, SpreadsheetDateSystem.Excel1900))
            End Function)
        Dim raw As New DataObj : raw.LoadCoreDataTable(table, CharCols:=0, cloneTable:=False)
        Dim intent = WindowsSurvivalWorkflowAdapter.Capture(raw, 0, {"0", "0", "0"}, {"0", "0", "0"}, False, False,
            SurvivalAnalysisMode.KaplanMeier, New SurvivalOptions)
        CollectionAssert.AreEqual({102, 104, 105}, intent.Input.Time.RowIds)
        Assert.IsNotNull(intent.Input.Provenance)
        Dim frame = intent.Input.Provenance, retainedIndex = frame.RowIds.ToList().IndexOf(104)
        Assert.AreEqual(1, frame.SourceAt(retainedIndex, 0).WorksheetColumn)
        Assert.AreEqual(4, frame.SourceAt(retainedIndex, 1).WorksheetColumn)
        Assert.AreEqual(104, frame.SourceAt(retainedIndex, 1).WorksheetRow)
        Assert.AreEqual(1, intent.Input.SourceAt(104, SurvivalRole.Time).WorksheetColumn)
        Assert.AreEqual(4, intent.Input.SourceAt(104, SurvivalRole.Status).WorksheetColumn)
        Assert.IsNull(intent.Input.SourceAt(104, SurvivalRole.Group))
        Assert.IsNull(intent.Input.SourceAt(999, SurvivalRole.Time))
    End Sub

    <TestMethod>
    Public Sub CoxRawAliasEffectsExpansionAndRetainedRowsStayAligned()
        Dim raw As New CoxPHData With {.bStrata = True}
        raw.DataImportRawMatrix(New Object(,) {{1.0, 1.0, "Z", 1.0, 0.2}, {2.0, 0.0, "a", 2.0, Nothing},
            {3.0, 1.0, "Z", 3.0, 0.6}, {4.0, 1.0, "a", 2.0, 0.8}, {5.0, 0.0, "a", 1.0, 1.0}},
            {"time", "status", "site", "cat", "dose"}, firstSourceRow:=101)
        Dim catalog = New String() {"cat | VarD", "dose | VarE"}
        Dim model = RegressionEffectAuthoringPolicy.Apply(catalog, {0}, New RegressionEffectModel(includeIntercept:=False), RegressionEffectOperation.CategoricalMain).Model
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {1}, model, RegressionEffectOperation.ContinuousMain).Model
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {0, 1}, model, RegressionEffectOperation.CustomInteraction).Model
        Dim effects = model.Effects.Select(Function(e) e.Key).ToArray(), specs = model.ToTermSpecs()
        Dim keys = RegressionDesignCore.GetRequiredRawVarKeys(effects, specs), x(,) As Double = Nothing, names() As String = Nothing
        RegressionDesignCore.BuildExpandedPredictorMatrix(raw.DataDbl, keys, effects, specs, CoxWorkflowPolicy.OmitCategoricalReference, x, names)
        Dim intent = WindowsCoxWorkflowAdapter.Capture(raw, x, names, model, keys.ToArray(), New CoxOptions)
        Assert.IsTrue(intent.Validation.IsValid)
        Assert.IsTrue(names.Length > catalog.Length)
        CollectionAssert.AreEqual({101, 103, 104, 105}, intent.Expanded.RowIds)
        CollectionAssert.AreEqual(catalog, intent.Expanded.SourceKeys)
        Dim records = CoxWorkflowPolicy.BuildExpandedRecords(intent.Input, x)
        CollectionAssert.AreEqual(raw.StrataData, records.Select(Function(r) r.Stratum).ToArray())
        CollectionAssert.AreEqual(raw.RowIds, records.Select(Function(r) r.Index).ToArray())
        For i = 0 To records.Count - 1
            For j = 0 To names.Length - 1 : Assert.AreEqual(x(i, j), records(i).Covariates(j)) : Next
        Next
        Assert.IsFalse(CoxWorkflowPolicy.InitialValuesCountMatches({0.0, 0.0}, names.Length))
    End Sub

    <DataTestMethod><DataRow("Breslow")><DataRow("Efron")><DataRow("Exact")>
    Public Sub ActualCoxAdapterFitWriterAndResidualSeams(tie As String)
        Dim key = "Cox|" & tie & "|robust", request = Survival13Requests.Cox(key), records = Survival13Fixture.Records(key)
        Dim values(records.Count - 1, 3) As Object
        For i = 0 To records.Count - 1
            values(i, 0) = records(i).Time : values(i, 1) = records(i).Censorship
            values(i, 2) = records(i).Covariates(0) : values(i, 3) = records(i).Covariates(1)
        Next
        Dim table = CoreDataTable.FromObjectMatrix(values, {"time", "status", "x1", "x2"})
        table.RowIds = records.Select(Function(r) r.Index).ToArray()
        Dim raw As New CoxPHData : raw.LoadCoreDataTable(table)
        Dim intent = WindowsCoxWorkflowAdapter.Capture(raw, request.Expanded.Data, request.Expanded.Names, Nothing, {"x1", "x2"}, request.Options)
        Dim expandedRecords = CoxWorkflowPolicy.BuildExpandedRecords(intent.Input, intent.Expanded.Data)
        Dim fit = intent.CreateAcquiredModel(expandedRecords), result = fit.Fit(intent.Options.Ties)
        Dim tables = fit.wrapResults(), report = CoxReportPolicy.Compose(intent, tables, result)
        SameWrittenTables(tables, report.Report.LegacyTables())
        Dim residuals = CoxReportPolicy.Residuals(intent, fit.wrapResiduals())
        CollectionAssert.AreEqual(raw.RowIds, residuals.RowIds)
        CollectionAssert.AreEqual(raw.TimeData, residuals.Time)
        CollectionAssert.AreEqual(raw.CensorData, residuals.Status)
        CollectionAssert.AreEqual(request.Expanded.Data, residuals.Predictors)
    End Sub

    Private Shared Function ImportCall(name As String, args() As Object) As Boolean
        Dim moduleType = GetType(WindowsCoxWorkflowAdapter).Assembly.GetType("BESHStatNG.UdfDataImport", True)
        Dim method = moduleType.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static)
        Assert.IsNotNull(method)
        Return CBool(method.Invoke(Nothing, args))
    End Function

    <TestMethod>
    Public Sub ExistingKmUdfAndGuiCoreVectorsAgreeForIdenticalLabels()
        Dim times As Object(,) = {{1.0}, {2.0}, {2.0}, {4.0}, {5.0}, {6.0}}
        Dim status As Object(,) = {{1.0}, {0.0}, {1.0}, {1.0}, {0.0}, {1.0}}
        Dim groups As Object(,) = {{"B"}, {"A"}, {"B"}, {"A"}, {"B"}, {"A"}}
        Dim args As Object() = {times, status, groups, Nothing}
        Assert.IsTrue(ImportCall("TryGetKaplanMeierRecords", args))
        Dim udf = DirectCast(args(3), List(Of SurvivalRecord)), err = ""
        Dim gui = Survival.CreatSurvivalData(Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(times, 0)), Matrix.Array2intArray(Matrix.GetColumnFrom2Darray(status, 0)), {"B", "A", "B", "A", "B", "A"}, Enumerable.Repeat("0", 6).ToArray(), err)
        Dim a As New Survival_KM_LR(gui), b As New Survival_KM_LR(udf)
        a.SurvivalCurveTabularOutput() : b.SurvivalCurveTabularOutput()
        CollectionAssert.AreEqual(Survival13Fixture.Tables(a.wrapResults(), {}), Survival13Fixture.Tables(b.wrapResults(), {}))
        ' The KM_TABLE UDF intentionally merges ordinal-ignore-case labels; GUI does not.
        groups(0, 0) = "b" : groups(2, 0) = "B"
        args = {times, status, groups, Nothing} : Assert.IsTrue(ImportCall("TryGetKaplanMeierRecords", args))
        udf = DirectCast(args(3), List(Of SurvivalRecord))
        Assert.AreEqual(udf(0).Group, udf(2).Group)
        Assert.AreEqual("b", udf(0).strGroup) : Assert.AreEqual("B", udf(2).strGroup)
    End Sub

    <TestMethod>
    Public Sub InvalidUdfRowsAreFilteredWhilePortableDiagnosticsPreserveThem()
        Dim args As Object() = {New Object(,) {{1.0}, {-1.0}, {2.0}, {3.0}, {4.0}},
            New Object(,) {{1.0}, {1.0}, {0.5}, {2.0}, {0.0}}, ExcelMissing.Value, Nothing}
        Assert.IsTrue(ImportCall("TryGetKaplanMeierRecords", args))
        Dim records = DirectCast(args(3), List(Of SurvivalRecord))
        Assert.AreEqual(3, records.Count)
        CollectionAssert.AreEqual({1.0, 2.0, 4.0}, records.Select(Function(r) r.Time).ToArray())
        Assert.IsTrue(records.All(Function(r) r.Index = 0), "UDF arrays expose positional input, not original worksheet coordinates.")
    End Sub

    <TestMethod>
    Public Sub CoxUdfAndRawGuiImportShareNumericsButNotWorksheetCoordinates()
        Dim times As Object(,) = {{1.0}, {2.0}, {3.0}, {4.0}}
        Dim status As Object(,) = {{1.0}, {0.0}, {1.0}, {1.0}}
        Dim x As Object(,) = {{1.0}, {Nothing}, {2.0}, {3.0}}, strata As Object(,) = {{"Z"}, {"a"}, {"a"}, {"Z"}}
        Dim args As Object() = {times, status, x, New Object(,) {{"x"}}, strata, Nothing}
        Assert.IsTrue(ImportCall("TryGetCoxData", args))
        Dim udf = DirectCast(args(5), CoxPHData), gui As New CoxPHData With {.bStrata = True}
        gui.DataImportRawMatrix(New Object(,) {{1.0, 1.0, "Z", 1.0}, {2.0, 0.0, "a", Nothing}, {3.0, 1.0, "a", 2.0}, {4.0, 1.0, "Z", 3.0}},
            {"time", "status", "stratum", "x"}, firstSourceRow:=101)
        CollectionAssert.AreEqual(gui.TimeData, udf.TimeData) : CollectionAssert.AreEqual(gui.CensorData, udf.CensorData)
        CollectionAssert.AreEqual(gui.StrataData, udf.StrataData) : CollectionAssert.AreEqual(gui.DataDbl, udf.DataDbl)
        CollectionAssert.AreEqual({101, 103, 104}, gui.RowIds)
        Assert.IsFalse(gui.RowIds.SequenceEqual(udf.RowIds))
        ' The real GUI acquisition uses LoadCoreDataTable(default CharCols=-1), not the
        ' raw-matrix override above. Character strata are dropped there; retain the discrepancy.
        Dim nativeImport As New CoxPHData With {.bStrata = True}
        nativeImport.LoadCoreDataTable(CoreDataTable.FromObjectMatrix(New Object(,) {{1.0, 1.0, "Z", 1.0}}, {"t", "s", "site", "x"}))
        Assert.IsTrue(nativeImport.bZeroValid)
    End Sub
End Class
