Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Udfs = BESHStatNG.WorksheetFunctions.GEEUDFs

<TestClass>
Public Class Gee12AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class

    <DataTestMethod><DynamicData(NameOf(Gee12Fixture.Cases), GetType(Gee12Fixture), DynamicDataSourceType.Method)>
    Public Sub ActualShippingCaptureCoreReportAndWriterSeam(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim raw = Gee12Fixture.Raw(key), r = Gee12PolicyTests.Request(key, raw), p = r.MeanRequest.Primary
                Dim intent = WindowsGeeWorkflowAdapter.Capture("y", raw, p.Data, p.Names, p.Effects, r.MeanRequest.Options, r.Options, p.InitialValues)
                CollectionAssert.AreEqual(raw.RowIds, intent.MeanRequest.Primary.RowIds)
                CollectionAssert.AreEqual(raw.ClusterIdData, intent.Cluster.Values)
                CollectionAssert.AreEqual(raw.OffsetData, intent.MeanRequest.Offset.Values)
                CollectionAssert.AreEqual(raw.WeightData, intent.MeanRequest.Weights.Values)
                Dim fit = intent.CreateCoreModel()
                If key.EndsWith("|start", StringComparison.Ordinal) Then
                    Assert.ThrowsException(Of NullReferenceException)(Sub() fit.Fit(True)) : Continue For
                End If
                fit.Fit(False)
                Dim residuals = GeeReportPolicy.Residuals(intent, fit.AllResiduals)
                Dim report = WindowsGeeWorkflowAdapter.Report(intent, fit.wrapResults(), fit.PredictedResponses, residuals,
                    fit.ObservedResponses, fit.ObservationWeights, fit.WorkingCorrelationMatrix)
                Gee12PolicyTests.SameTables(fit.wrapResults(), report.LegacyTables())
                Dim before As New MemoryWriter, after As New MemoryWriter
                Dim originalProcessor As New ProcessListofResultTables(fit.wrapResults())
                Dim projectedProcessor As New ProcessListofResultTables(report.LegacyTables())
                originalProcessor.writeToSheet(before, True) : projectedProcessor.writeToSheet(after, True)
                SameBlocks(before.Blocks, after.Blocks)
                before = New MemoryWriter : after = New MemoryWriter
                before.write(fit.PredictedResponses, bTall:=True)
                after.write(GeeReportPolicy.Predictions(intent, fit.PredictedResponses), bTall:=True)
                before.write(fit.AllResiduals) : after.write(report.Rows.Residuals)
                SameBlocks(before.Blocks, after.Blocks)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Private Shared Sub SameBlocks(a As List(Of ResultTableOutputBlock), b As List(Of ResultTableOutputBlock))
        Assert.AreEqual(a.Count, b.Count)
        For i = 0 To a.Count - 1
            Assert.AreEqual(a(i).StartRow, b(i).StartRow) : Assert.AreEqual(a(i).StartColumn, b(i).StartColumn)
            Assert.AreEqual(a(i).Model.TitleRows, b(i).Model.TitleRows)
            Assert.AreEqual(a(i).Model.HeaderTopRows, b(i).Model.HeaderTopRows)
            Assert.AreEqual(a(i).Model.HeaderLeftColumns, b(i).Model.HeaderLeftColumns)
            Assert.AreEqual(a(i).Model.FooterRows, b(i).Model.FooterRows)
            CollectionAssert.AreEqual(a(i).Model.PvalueColumns, b(i).Model.PvalueColumns)
            CollectionAssert.AreEqual(a(i).Model.PvalueCells, b(i).Model.PvalueCells)
            CollectionAssert.AreEqual(a(i).Model.Values.Cast(Of Object)().ToArray(), b(i).Model.Values.Cast(Of Object)().ToArray())
        Next
    End Sub

    <TestMethod>
    Public Sub PhysicalTransportCoordinatesFollowClusterSortAndFilteredRows()
        Dim values As Object(,) = {{1, 1, 2, 3}, {2, 2, 1, 2}, {Nothing, 3, 1, 1}, {3, 4, 2, 1}, {4, 5, 1, 1}, {5, 6, 2, 2}}
        Dim table = ExcelDnaDataImporter.ImportPhysicalSelection("'Data'!A17:D22", {"A17:D22"},
            Function(addresses)
                Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "gee12-transport"}
                selection.Ranges.Add(SpreadsheetRangeReference.FromAddress("A17:D22", "Data", "Fixture.xlsx"))
                Return ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
                    Function(reference) ExcelDnaSharedApplicationAdapter.SnapshotValues(reference, values, Function(row, column) "General", Nothing, SpreadsheetDateSystem.Excel1900))
            End Function)
        Dim raw As New geeData With {.bTime = True}
        raw.LoadCoreDataTable(table, cloneTable:=False)
        Dim options As New RegressionOptions With {.Family = RegressionFamily.Gaussian, .Link = RegressionLink.Identity}
        Dim effects As New RegressionEffectModel({New RegressionAuthoredEffect(raw.varNames(1), Nothing)})
        Dim intent = WindowsGeeWorkflowAdapter.Capture(raw.varNames(0), raw, Gee12Fixture.Matrix(raw), raw.varNames, effects, options, New GeeWorkflowOptions)
        Assert.AreEqual(5, intent.MeanRequest.Primary.ResponseSources.Count)
        For i = 0 To raw.nRows - 1
            Assert.AreEqual(raw.RowIds(i), intent.MeanRequest.Primary.ResponseSources(i).WorksheetRow)
            Assert.AreEqual(1, intent.MeanRequest.Primary.ResponseSources(i).WorksheetColumn)
        Next
        Assert.IsFalse(intent.MeanRequest.Primary.RowIds.Contains(19))
        Assert.IsTrue(intent.Order.PredictionOrderMatchesInput)
    End Sub

    <TestMethod>
    Public Sub LegacyPostFitControlsAreFreshAndChartsStayOutsideCommon()
        Dim key = "Binomial|Logit|Independence|Robust|time", raw = Gee12Fixture.Raw(key), initial = Gee12PolicyTests.Request(key, raw)
        Dim intent = WindowsGeeWorkflowAdapter.Capture("y", raw, initial.MeanRequest.Primary.Data, raw.varNames, initial.MeanRequest.Primary.Effects, initial.MeanRequest.Options, initial.Options)
        Dim fit = intent.CreateCoreModel() : fit.Fit(False)
        Dim report = WindowsGeeWorkflowAdapter.Report(intent, fit.wrapResults(), fit.PredictedResponses, fit.AllResiduals, fit.ObservedResponses, fit.ObservationWeights)
        Dim postFit As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True,
            .Threshold = 0.25, .ThresholdTable = True, .Calibration = True, .CalibrationBins = 4, .Brier = True}
        Dim y = fit.ObservedResponses, p = fit.PredictedResponses, w = fit.ObservationWeights
        Dim eventRate = BESHStatNG.WorksheetFunctions.ComputeWeightedEventRate(y, w)
        Dim cls = GeeReportPolicy.Classification(report, postFit, eventRate)
        Dim original = regression.BinaryClassificationReporting.WrapResults(
            regression.BinaryClassificationReporting.ComputeBinarySummary(y, p, 0.25, w),
            regression.BinaryClassificationReporting.BuildThresholdTable(y, p, Nothing, w),
            regression.BinaryClassificationReporting.BuildCalibrationBins(y, p, 4, w, "quantile"),
            regression.BinaryClassificationReporting.ComputeBrierScore(y, p, w), eventRate, "GEE Binary Classification")
        Gee12PolicyTests.SameTables(original, cls.LegacyTables())
        Dim before As New MemoryWriter, after As New MemoryWriter
        Dim originalProcessor As New ProcessListofResultTables(original), projectedProcessor As New ProcessListofResultTables(cls.LegacyTables())
        originalProcessor.writeToSheet(before, True) : projectedProcessor.writeToSheet(after, True)
        SameBlocks(before.Blocks, after.Blocks)
        CollectionAssert.AreEqual(New String() {"ROC", "Calibration"}, cls.ChartHooks.ToArray())
        postFit.Classification = False
        Assert.AreEqual(0, GeeReportPolicy.Classification(report, postFit).ChartHooks.Count)
        Assert.AreEqual(0.25, cls.Options.Threshold)
        Assert.AreEqual(0.5, intent.MeanRequest.Options.Threshold)
    End Sub

    <TestMethod>
    Public Sub ChangedPostFitFamilyIsCorrectInCommonAndQuarantinedInWindows()
        Dim raw = Gee12Fixture.Raw("Binomial|Logit|Independence|Robust|time")
        Dim request = Gee12PolicyTests.Request("Gaussian|Identity|Independence|Robust|time", raw)
        Dim fit = request.CreateCoreModel() : fit.Fit(False)
        Dim report = WindowsGeeWorkflowAdapter.Report(request, fit.wrapResults(), fit.PredictedResponses, fit.AllResiduals, fit.ObservedResponses, fit.ObservationWeights)
        Dim selectedAfterFit As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .ThresholdTable = True}
        Assert.AreEqual(0, GeeReportPolicy.Classification(report, selectedAfterFit).LegacyTables().Count, "Changing a selection does not change the fitted family.")
        Dim old = regression.BinaryClassificationReporting.WrapResults(
            regression.BinaryClassificationReporting.ComputeBinarySummary(fit.ObservedResponses, fit.PredictedResponses, 0.5, fit.ObservationWeights),
            regression.BinaryClassificationReporting.BuildThresholdTable(fit.ObservedResponses, fit.PredictedResponses, Nothing, fit.ObservationWeights),
            analysisLabel:="GEE Binary Classification")
        Dim legacy = WindowsGeeWorkflowAdapter.DeferredPostFitFamilyReport(report, selectedAfterFit, Double.NaN)
        Gee12PolicyTests.SameTables(old, legacy.LegacyTables())
        Assert.IsTrue(legacy.LegacyTables().Count > 0, "Original Windows mismatch path remains pending a native timing decision, not permanent Common semantics.")
    End Sub

    <TestMethod>
    Public Sub ExistingPostFitUdfAndContrastContractsExecuteWithoutCallingFitOrExcel()
        Dim key = "Gaussian|Identity|Independence|Robust|time", raw = Gee12Fixture.Raw(key), intent = Gee12PolicyTests.Request(key, raw)
        Dim fit = intent.CreateCoreModel() : fit.Fit(False)
        Dim moduleType = GetType(BESHStatNG.WorksheetFunctions.GEEUDFs)
        Dim handleType = moduleType.GetNestedType("GeeHandle", BindingFlags.NonPublic)
        Dim handle = Activator.CreateInstance(handleType, nonPublic:=True), id = "gee12-host-free-post-fit-" & Guid.NewGuid().ToString("N")
        Dim design(raw.nRows - 1, raw.nCols - 1) As Double
        For i = 0 To raw.nRows - 1 : design(i, 0) = 1 : design(i, 1) = CDbl(raw.FinalData(i, 1)) : Next
        Dim setValue = Sub(name As String, value As Object) handleType.GetProperty(name).SetValue(handle, value, Nothing)
        Dim build As RegressionFormulaRegressionDataBuildResult = Nothing, errorMessage As String = Nothing
        Assert.IsTrue(RegressionFormulaDesignService.TryBuildExpandedRegressionDataMatrixFromFormula(raw, "y", build, errorMessage), errorMessage)
        setValue("Handle", id) : setValue("Model", fit) : setValue("LinkObject", intent.CreateLink())
        setValue("VarNames", fit.results.varNames) : setValue("FittedDesign", design) : setValue("FittedTimeValues", raw.TimeData)
        setValue("StandardErrorType", "Robust") : setValue("Alpha", 0.1) : setValue("RawVarNames", New String() {"x"})
        setValue("RawPredictorKeys", New String() {"x"}) : setValue("RequiredRawPredictorKeys", New String() {"x"})
        setValue("ExpandedPredictorNames", New String() {"x"}) : setValue("HasOffset", True)
        setValue("OmitCategoricalReference", True)
        setValue("DesignSpec", build.DesignSpec)
        setValue("RawVarNames", build.FullRawPredictorNames) : setValue("RawPredictorKeys", build.FullRawPredictorKeys)
        setValue("RequiredRawPredictorKeys", build.RequiredRawPredictorKeys)
        Dim cache = moduleType.GetField("_geeCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing)
        Assert.IsTrue(CBool(cache.GetType().GetMethod("TryAdd").Invoke(cache, {id, handle})))
        Try
            Dim summary = DirectCast(Udfs.GEE_SUMMARY(id), Object(,))
            Assert.AreEqual(fit.results.Coeffs_est(0), CDbl(summary(1, 2)))
            Dim corr = DirectCast(Udfs.GEE_WCORR(id, False), Object(,))
            Assert.AreEqual(fit.WorkingCorrelationMatrix.GetLength(0), corr.GetLength(0))
            Dim cov = DirectCast(Udfs.GEE_VCOV(id, "naive", False), Object(,))
            CollectionAssert.AreEqual(fit.GetParameterCovarianceMatrix("Naive").Cast(Of Double)().ToArray(), cov.Cast(Of Double)().ToArray())
            Dim residual = DirectCast(Udfs.GEE_RESID(id, "all", True), Object(,))
            Assert.AreEqual(raw.nRows + 1, residual.GetLength(0))
            Dim newX(raw.nRows - 1, 0), offsets(raw.nRows - 1, 0) As Object
            For i = 0 To raw.nRows - 1 : newX(i, 0) = raw.FinalData(i, 1) : offsets(i, 0) = raw.OffsetData(i) : Next
            Dim predictions = DirectCast(Udfs.GEE_PRED(id, newX, offsets, False), Object(,))
            For i = 0 To raw.nRows - 1 : Assert.AreEqual(fit.PredictedResponses(i), CDbl(predictions(i, 0)), 0.000000000001) : Next
            Dim spec As Object(,) = {{"label", "weight", "time"}, {"change", -1.0, 0.0}, {"CHANGE", 1.0, 1.0}}
            Dim contrast = DirectCast(Udfs.GEE_LSMESTIMATE(id, spec, "robust", 0.1), Object(,))
            Dim plan = GeeContrastPolicy.Project(design, {"Intercept", "x"}, {
                New GeeContrastComponent("change", -1, Nothing, 0), New GeeContrastComponent("CHANGE", 1, Nothing, 1)}, raw.TimeData)
            Dim row = Enumerable.Range(0, 2).Select(Function(j) plan.Contrast(0, j)).ToArray()
            Assert.AreEqual(BESHStatNG.Matrix.DotProduct(row, fit.results.Coeffs_est), CDbl(contrast(1, 1)), 0.000000001)
            Assert.AreEqual("90% CI Lower", CStr(contrast(0, 5)))
            Assert.AreEqual("Link", CStr(contrast(1, 7)))
            Assert.AreEqual("NO_SHIPPING_GUI_CONTRAST_PATH", plan.ShippingGuiPath)
        Finally
            Assert.AreEqual(True, Udfs.GEE_DROP(id))
        End Try
    End Sub
End Class
