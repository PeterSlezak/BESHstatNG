Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.regression
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfPayload42Tests
    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ClassificationActualEnginesKeepWeightedCountsTiesAndCallerArrays(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim y() As Double = {0, 1, 1, 0, 1, 0}, p() As Double = {0, 0.5, 1, 0.5, 0.8, 0.1}, w() As Double = {0, 2, 1, 1, 3, 1}
            Dim before = DirectCast(p.Clone(), Double()), missing As New Object()
            Dim expected = UdfClassificationReportPreparation38.BuildBinaryCrosstabUdfOutput(BinaryClassificationCore.ComputeBinarySummary(y, p, 0.5, w), missing, True)
            Dim actual = UdfClassificationReportPreparation38.ComputeConfusionOutput(y, p, 0.5, w, missing, True)
            UdfMultivariate42Tests.EqualArrays(DirectCast(expected, Object(,)), DirectCast(actual, Object(,)))
            DirectCast(actual, Object(,))(0, 0) = "mutation"
            UdfMultivariate42Tests.EqualArrays(DirectCast(expected, Object(,)), DirectCast(UdfClassificationReportPreparation38.ComputeConfusionOutput(y, p, 0.5, w, missing, True), Object(,)))
            CollectionAssert.AreEqual(before, p)
            Dim cuts() As Double = {0.5, 0.5, 1, 0}
            expected = UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(BinaryClassificationCore.BuildThresholdTable(y, p, cuts, w), missing, False)
            actual = UdfClassificationReportPreparation38.ComputeThresholdOutput(y, p, cuts, w, missing, False)
            UdfMultivariate42Tests.EqualArrays(DirectCast(expected, Object(,)), DirectCast(actual, Object(,)))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <DataTestMethod, DataRow("quantile"), DataRow("equalwidth")>
    Public Sub CalibrationAndBrierKeepEngineBinsWeightsAndLabels(method As String)
        Dim y() As Double = {0, 1, 0, 1}, p() As Double = {0.1, 0.5, 0.5, 0.9}, w() As Double = {1, 0, 2, 1}, missing As New Object()
        Dim rows = BinaryClassificationCore.BuildCalibrationBins(y, p, 5, w, method)
        UdfMultivariate42Tests.EqualArrays(DirectCast(UdfClassificationReportPreparation38.BuildCalibrationTableUdfOutput(rows, missing), Object(,)), DirectCast(UdfClassificationReportPreparation38.ComputeCalibrationOutput(y, p, 5, w, method, missing, True), Object(,)))
        Assert.AreEqual(rows.Count, UdfPlotDataPreparation38.ComputeCalibrationRows(y, p, 5, w, method).Count)
        Dim score = BinaryClassificationCore.ComputeBrierScore(y, p, w), rate = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeWeightedEventRate(y, w)
        UdfMultivariate42Tests.EqualArrays(DirectCast(UdfClassificationReportPreparation38.BuildBrierScoreUdfOutput(score, missing, w.Sum(), rate), Object(,)), DirectCast(UdfClassificationReportPreparation38.ComputeBrierOutput(y, p, w, missing, True), Object(,)))
        Assert.ThrowsException(Of ArgumentException)(Sub() UdfPlotDataPreparation38.ComputeCalibrationRows({0, Double.NaN}, {0.2, 0.8}, 2, Nothing, method))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub KaplanMeierUsesSameRealEngineAndRetainsSteps(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim err As String = Nothing
            Dim records = survival.Survival.CreatSurvivalData({1.0, 2, 2, 4, 5, 6}, {1, 0, 1, 1, 0, 1}, {"B", "B", "B", "A", "A", "A"}, {"ALL", "ALL", "ALL", "ALL", "ALL", "ALL"}, err)
            Dim km As New survival.Survival_KM_LR(records)
            Dim curves = UdfPlotDataPreparation38.ComputeKaplanMeierCurves(records, 0.05)
            Assert.AreEqual(km.SurvivalCurveTabularOutput(0.05).Length, curves.Length)
            Assert.AreEqual(6, records.Count)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <DataTestMethod, DataRow("scores"), DataRow("weights"), DataRow("matcheddata"), DataRow("loveplot")>
    Public Sub PsmRealFitKeepsSourceIdsAndOrderedPayloadsWithoutRefitting(table As String)
        Dim input = Psm25Fixture.Input(36), options = Psm25Fixture.Options(0, 0, 36), rows = Psm25Fixture.SourceRows(12)
        options.StandardOptions.RandomSeed = 173
        Dim result = PsmComprehensiveBackend.Fit(input, options), importReads As Integer = 0
        Dim expected As Object(,) = If(table = "loveplot", PsmAdvancedTables.LovePlotTable(result.LovePlotRows), If(table = "matcheddata", PsmFrontEndTables.MatchedDatasetTable(input, result.Result, rows), PsmFrontEndTables.RowLevelAuditTable(input, result.Result, rows)))
        Dim actual = UdfPsmDataPreparation38.BuildNamedTable(result, options, Function() input, Function() rows, table, Function()
                                                                                                                       importReads += 1
                                                                                                                       Return New Object(,) {{"import"}}
                                                                                                                   End Function)
        UdfMultivariate42Tests.EqualArrays(expected, actual)
        actual(0, 0) = "mutation"
        UdfMultivariate42Tests.EqualArrays(expected, UdfPsmDataPreparation38.BuildNamedTable(result, options, Function() input, Function() rows, table, Function() Nothing))
        Assert.AreEqual(0, importReads)
        CollectionAssert.AreEqual(Psm25Fixture.SourceRows(12), rows)
    End Sub

    <TestMethod>
    Public Sub PsmImportAndUnknownDoNotEagerlyReadUnneededData()
        Dim dataReads As Integer = 0
        Dim readInput As Func(Of PsmInputData) = Function()
                                                    dataReads += 1
                                                    Return Nothing
                                                End Function
        Dim import As Object(,) = {{"source", "fixture"}}
        Assert.AreSame(import, UdfPsmDataPreparation38.BuildNamedTable(Nothing, Nothing, readInput, Function() Nothing, "import", Function() import))
        Assert.IsNotNull(UdfPsmDataPreparation38.BuildNamedTable(Nothing, Nothing, readInput, Function() Nothing, "not a table", Function() Nothing))
        Assert.AreEqual(0, dataReads)
    End Sub
End Class
