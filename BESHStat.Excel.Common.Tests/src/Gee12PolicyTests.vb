Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Gee12PolicyTests
    Public Shared Function Request(key As String, Optional raw As geeData = Nothing) As GeeWorkflowIntent
        raw = If(raw, Gee12Fixture.Raw(key))
        Dim parts = key.Split("|"c)
        Dim effects As New RegressionEffectModel(If(raw.nCols > 1, New RegressionAuthoredEffect() {New RegressionAuthoredEffect("x", Nothing)}, Array.Empty(Of RegressionAuthoredEffect)()))
        Dim options As New RegressionOptions With {.Family = CType([Enum].Parse(GetType(RegressionFamily), parts(0)), RegressionFamily),
            .Link = CType([Enum].Parse(GetType(RegressionLink), parts(1)), RegressionLink),
            .Power = If(parts(1) = "Power", CType(0.5, Double?), Nothing), .Dispersion = 0.8, .Alpha = 0.1,
            .MaximumIterations = If(parts(4) = "limit", 0, 50), .Epsilon = 0.000001, .Residuals = True, .IterationDetails = True}
        Dim starts As Double() = If(parts(4) = "start", New Double() {2.0, 0.3}, Nothing)
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y",
            New RegressionComponent(Gee12Fixture.Matrix(raw), raw.varNames, raw.RowIds, effects, If(raw.nCols > 1, {"y", "x"}, {"y"}), starts), options,
            New RegressionVector("offset", raw.OffsetData, raw.RowIds), New RegressionVector("weight", raw.WeightData, raw.RowIds))
        Return GeeWorkflowPolicy.Project(mean, New GeeClusterVector("cluster", raw.ClusterIdData, raw.RowIds),
            If(raw.bTime, New RegressionVector("visit", raw.TimeData, raw.RowIds), Nothing),
            New GeeWorkflowOptions With {.Correlation = parts(2), .StandardError = CType(Array.IndexOf(New String() {"Robust", "Naive", "Bias Reduced"}, parts(3)), GeeStandardError)})
    End Function

    Public Shared Function Report(intent As GeeWorkflowIntent, fit As GEE) As GeeReport
        Return GeeReportPolicy.Compose(intent, fit.wrapResults(), fit.PredictedResponses,
            If(fit.bComputeResiduals, fit.AllResiduals, Nothing), fit.ObservedResponses, fit.ObservationWeights, fit.WorkingCorrelationMatrix)
    End Function

    <DataTestMethod><DynamicData(NameOf(Gee12Fixture.Cases), GetType(Gee12Fixture), DynamicDataSourceType.Method)>
    Public Sub PortableIntentUsesRealCoreAndMatchesFrozenInputTablesVectors(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim raw = Gee12Fixture.Raw(key), intent = Request(key, raw), fit = intent.CreateCoreModel()
                Assert.AreEqual(raw.nCols, intent.ExpectedParameterCount)
                CollectionAssert.AreEqual(raw.RowIds, intent.Order.InputRowIds)
                Assert.IsTrue(intent.Order.PredictionOrderMatchesInput)
                Dim actual = Gee12Fixture.Execute(key, fit, raw)
                Regression10Fixture.AssertCapture(File.ReadAllLines(Path.Combine(Gee12Fixture.BaselineRoot(), Gee12Fixture.FileName(key, culture))), actual, key)
                If actual.Last().StartsWith("CORE_FAILURE|", StringComparison.Ordinal) Then Continue For
                Dim report = Gee12PolicyTests.Report(intent, fit)
                Assert.AreEqual("regression.gee", report.Document.AnalysisId)
                CollectionAssert.AreEqual(raw.RowIds, report.Rows.RowIds)
                CollectionAssert.AreEqual(fit.PredictedResponses, report.CorePredictions)
                CollectionAssert.AreEqual(fit.PredictedResponses, report.Rows.Fitted.Cast(Of Double)().ToArray())
                SameTables(fit.wrapResults(), report.LegacyTables())
                CollectionAssert.AreEqual(fit.AllResiduals.Cast(Of Object)().ToArray(), report.Rows.Residuals.Cast(Of Object)().ToArray())
                Assert.AreEqual(0.1, fit.results.alpha)
                If key.EndsWith("|limit", StringComparison.Ordinal) Then Assert.AreEqual(RegressionConvergence.NotConverged, report.Convergence)
                Assert.AreNotEqual(RegressionConvergence.NotIterative, report.Convergence)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Public Shared Sub SameTables(before As List(Of ResultTable), after As List(Of ResultTable))
        Assert.AreEqual(before.Count, after.Count)
        For i = 0 To before.Count - 1
            Dim x = before(i).ToOutputModel(), y = after(i).ToOutputModel()
            Assert.AreEqual(x.TitleRows, y.TitleRows) : Assert.AreEqual(x.HeaderTopRows, y.HeaderTopRows)
            Assert.AreEqual(x.HeaderLeftColumns, y.HeaderLeftColumns) : Assert.AreEqual(x.FooterRows, y.FooterRows)
            CollectionAssert.AreEqual(x.PvalueColumns, y.PvalueColumns) : CollectionAssert.AreEqual(x.PvalueCells, y.PvalueCells)
            CollectionAssert.AreEqual(x.Values.Cast(Of Object)().ToArray(), y.Values.Cast(Of Object)().ToArray())
        Next
    End Sub

    <DataTestMethod><DataRow("cs", "Exchangable")><DataRow("AR(1)", "Autoregressive")><DataRow("stationary", "Toeplitz")><DataRow("UN", "Unstructured")><DataRow("", "Independence")>
    Public Sub CorrelationAliasesDelegateToExistingCore(aliasName As String, expected As String)
        Dim r = Request("Gaussian|Identity|Independence|Robust|time")
        Dim options = r.Options : options.Correlation = aliasName
        Dim intent = GeeWorkflowPolicy.Project(r.MeanRequest, r.Cluster, r.Time, options)
        Assert.AreEqual(expected, intent.CreateCorrelation().GetType().Name)
    End Sub

    <TestMethod>
    Public Sub ActualUiListsAndImplicitInterceptRemainExplicit()
        CollectionAssert.AreEqual(Family.FamiliesList, GeeWorkflowPolicy.FamilyNames().ToArray())
        CollectionAssert.AreEqual(GEEcovStruct.CovStructsList, GeeWorkflowPolicy.CorrelationNames().ToArray())
        CollectionAssert.AreEqual(New String() {"Robust", "Naive", "Bias Reduced"}, GeeWorkflowPolicy.StandardErrorNames().ToArray())
        Assert.AreEqual("Binomial", GeeWorkflowPolicy.FamilyNames()(0))
        Assert.AreEqual("NegativeBinomial", GeeWorkflowPolicy.FamilyCode("Negative Binomial"))
        For Each code In Family.FamiliesCodes
            Dim family = FamilyUtils.createFamily(code)
            CollectionAssert.AreEqual(Link.LinkList.Values.Where(Function(v) family.testLink(v)).ToArray(), GeeWorkflowPolicy.SupportedLinks(code).ToArray())
        Next
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), options = r.Options
        options.RequestedIntercept = False : options.TraceRequested = True
        Dim intent = GeeWorkflowPolicy.Project(r.MeanRequest, r.Cluster, r.Time, options)
        Assert.IsTrue(intent.EffectiveIntercept)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.intercept.implicit"))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.trace.ui-only"))
        Assert.IsFalse(intent.WeightsEnterEstimatingEquations)
        Assert.AreEqual(2, intent.ExpectedParameterCount)
    End Sub

    <TestMethod>
    Public Sub ImportedOrderAndExplicitCorePredictionPermutationAreDistinct()
        Dim data As Double(,) = {{1, 0}, {2, 1}, {3, 2}, {4, 3}, {5, 4}, {6, 5}}
        Dim rows = {91, 7, 63, 12, 44, 5}, clusters As Object() = {"z", "a", "z", "a", "z", "a"}, times = {8.0, 3.0, 2.0, 1.0, 8.0, 1.0}
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(data, {"y", "x"}, rows,
            New RegressionEffectModel({New RegressionAuthoredEffect("x", Nothing)}), {"y", "x"}),
            New RegressionOptions With {.Family = RegressionFamily.Gaussian, .Link = RegressionLink.Identity})
        Dim intent = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("subject", clusters, rows), New RegressionVector("visit", times, rows))
        CollectionAssert.AreEqual(New Integer() {0, 2, 4, 1, 3, 5}, intent.Order.ClusteredToInputIndices)
        CollectionAssert.AreEqual(New Integer() {91, 63, 44, 7, 12, 5}, intent.Order.CorePredictionRowIds)
        CollectionAssert.AreEqual(New Double() {10, 40, 20, 50, 30, 60}, GeeReportPolicy.Predictions(intent, {10, 20, 30, 40, 50, 60}))
        CollectionAssert.AreEqual(rows, intent.MeanRequest.Primary.RowIds)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.order.cluster-concatenated"))
        Dim permutation = GeeWorkflowPolicy.ImportSortPermutation(clusters, rows, times)
        Assert.AreEqual(rows.Length, permutation.Distinct().Count())
        CollectionAssert.AreEqual(New Integer() {5, 1, 3, 4, 2, 0}, GeeWorkflowPolicy.ImportSortPermutation(clusters, rows))
    End Sub

    <TestMethod>
    Public Sub BoundariesCloneAllInputOptionsRowsAndResultPayloads()
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), matrix = r.MeanRequest.Primary.Data
        Dim initial = matrix(0, 0) : matrix(0, 0) = -999
        Assert.AreEqual(initial, r.MeanRequest.Primary.Data(0, 0))
        Dim ids = r.Cluster.Values : ids(0) = "changed"
        Assert.AreNotEqual("changed", r.Cluster.Values(0))
        Dim options = r.Options : options.UseP = True
        Assert.IsFalse(r.Options.UseP)
        Dim fit = r.CreateCoreModel() : fit.Fit(False)
        Dim report = Gee12PolicyTests.Report(r, fit), predicted = report.CorePredictions, old = predicted(0)
        predicted(0) = -99 : Assert.AreEqual(old, report.CorePredictions(0))
        Dim covariance = report.WorkingCorrelation : covariance(0, 0) = -5
        Assert.AreNotEqual(-5.0, report.WorkingCorrelation(0, 0))
        Dim tables = report.LegacyTables() : tables(0).AddFootnote("Caller mutation")
        Assert.AreNotEqual(tables(0).ToOutputModel().FooterRows, report.LegacyTables()(0).ToOutputModel().FooterRows)
    End Sub

    <DataTestMethod><DataRow("cluster")><DataRow("time")><DataRow("offset")><DataRow("weights")><DataRow("starts")><DataRow("family")><DataRow("correlation")><DataRow("overflow")><DataRow("names")><DataRow("empty")>
    Public Sub MalformedTypedRequestsHaveScopedDiagnosticsWithoutActivatingARefusal(field As String)
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), p = r.MeanRequest.Primary, o = r.MeanRequest.Options
        Dim cluster = r.Cluster, time = r.Time, offset = r.MeanRequest.Offset, weights = r.MeanRequest.Weights, options = r.Options
        Dim data = p.Data, names = p.Names, rows = p.RowIds, starts As Double() = Nothing
        Select Case field
            Case "cluster" : cluster = New GeeClusterVector("", New Object() {}, New Integer() {})
            Case "time" : time = New RegressionVector("visit", New Double() {}, New Integer() {})
            Case "offset" : offset = New RegressionVector("offset", {Double.NaN}, {rows(0)})
            Case "weights" : weights = New RegressionVector("weight", {Double.PositiveInfinity}, {rows(0)})
            Case "starts" : starts = New Double() {}
            Case "family" : o.Family = RegressionFamily.Binomial : o.Link = RegressionLink.Power : o.Power = Nothing
            Case "correlation" : options.Correlation = "invalid"
            Case "overflow" : o.MaximumIterations = Integer.MaxValue
            Case "names" : names = {"duplicate", "duplicate"}
            Case "empty" : data = New Double(,) {} : rows = New Integer() {}
        End Select
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(data, names, rows, p.Effects, p.SourceKeys, starts), o, offset, weights)
        Dim intent = GeeWorkflowPolicy.Project(mean, cluster, time, options)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Blocking AndAlso Not String.IsNullOrEmpty(d.FieldPath)))
        Assert.IsNotNull(intent, "Common diagnostics are a plan, not an automatic shipping exception.")
    End Sub

    <TestMethod>
    Public Sub MissingResponseClusterTimeCollisionsAndNonfiniteValuesAreNotCoerced()
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), p = r.MeanRequest.Primary
        Dim data = p.Data, rows = p.RowIds, clusters = r.Cluster.Values, time = r.Time.Values
        data(0, 0) = Double.NaN : data(1, 1) = Double.PositiveInfinity : rows(1) = rows(0) : time(2) = Double.NaN
        clusters(0) = 1 : clusters(1) = "1"
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "", New RegressionComponent(data, p.Names, rows, p.Effects, {"x", "x"}), r.MeanRequest.Options)
        Dim intent = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("cluster", clusters, rows), New RegressionVector("visit", time, rows))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.cluster.text-collision"))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.time.nonfinite"))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "regression.numeric.nonfinite"))
        Assert.IsTrue(Double.IsNaN(intent.MeanRequest.Primary.Data(0, 0)))
    End Sub

    <TestMethod>
    Public Sub OutputLengthHeadersAxesAndUnavailableFitAreExplicit()
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), n = r.MeanRequest.Primary.RowIds.Length
        Assert.ThrowsException(Of ArgumentException)(Sub() GeeReportPolicy.Predictions(r, New Double(n) {}))
        Assert.ThrowsException(Of ArgumentException)(Sub() GeeReportPolicy.Residuals(r, New Object(n, 4) {}))
        Assert.ThrowsException(Of ArgumentException)(Sub() GeeReportPolicy.Residuals(r, New Object(n, 5) {}))
        Dim empty = GeeReportPolicy.Compose(r, New List(Of ResultTable), Nothing, Nothing, Nothing, Nothing)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, empty.Document.Status)
        Assert.AreEqual(RegressionConvergence.Unknown, empty.Convergence)
        Assert.IsNull(empty.Rows.Fitted) : Assert.IsNull(empty.Rows.Residuals)
        Dim cls = GeeReportPolicy.Classification(empty, New RegressionOptions With {.Classification = True})
        Assert.AreEqual(0, cls.LegacyTables().Count)
        Assert.AreEqual(0, cls.ChartHooks.Count)
    End Sub

    <TestMethod>
    Public Sub ExistingValidStartFailureAndInsufficientDataRemainTruthful()
        Dim r = Request("Gaussian|Identity|Independence|Robust|start"), fit = r.CreateCoreModel()
        Dim ex = Assert.ThrowsException(Of NullReferenceException)(Sub() fit.Fit(True))
        Assert.IsTrue(ex.StackTrace.Contains("EstimateQIC"))
        Dim empty = GeeReportPolicy.Compose(r, New List(Of ResultTable), Nothing, Nothing, Nothing, Nothing)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, empty.Document.Status)
        Dim shortMean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y",
            New RegressionComponent(New Double(,) {{1, 2}}, {"y", "x"}, {17}, r.MeanRequest.Primary.Effects, {"y", "x"}), r.MeanRequest.Options)
        Dim intent = GeeWorkflowPolicy.Project(shortMean, New GeeClusterVector("cluster", {"a"}, {17}))
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "gee.data.insufficient"))
        Assert.ThrowsException(Of ArgumentException)(Sub() intent.CreateCoreModel())
    End Sub

    <TestMethod>
    Public Sub CancellationAndSubsequentFitIsolationKeepCoreCallbacksOwnedByCaller()
        Dim r = Request("Gaussian|Identity|Independence|Robust|time"), fit = r.CreateCoreModel()
        Try
            CoreServices.Regression.SetCallbacks(Function() True, Nothing)
            Assert.ThrowsException(Of OperationCanceledException)(Sub() fit.Fit(False))
        Finally
            CoreServices.Regression.ClearCallbacks()
        End Try
        fit = r.CreateCoreModel() : fit.Fit(False)
        Assert.AreEqual(48, fit.ObservedResponses.Length)
        Assert.AreNotEqual(RegressionConvergence.Unknown, Gee12PolicyTests.Report(r, fit).Convergence)
    End Sub

    <DataTestMethod><DataRow(False, False, False)><DataRow(True, False, False)><DataRow(True, True, True)>
    Public Sub PostFitClassificationOptionsTablesBrierAndChartOrder(enabled As Boolean, calibration As Boolean, brier As Boolean)
        Dim r = Request("Binomial|Logit|Independence|Robust|time"), fit = r.CreateCoreModel() : fit.Fit(False)
        Dim report = Gee12PolicyTests.Report(r, fit)
        Dim options As New RegressionOptions With {.Classification = enabled, .Threshold = 0.35, .ThresholdTable = True, .Calibration = calibration, .CalibrationBins = 4, .Brier = brier}
        Dim actual = GeeReportPolicy.Classification(report, options, 0.5)
        Dim expectedOptions = options.Copy() : expectedOptions.Family = RegressionFamily.Binomial
        Dim expected = RegressionClassificationPolicy.Compose(fit.ObservedResponses, fit.PredictedResponses, fit.ObservationWeights, expectedOptions, 0.5, "GEE Binary Classification")
        SameTables(expected.LegacyTables(), actual.LegacyTables())
        options.Threshold = 0.9 : Assert.AreEqual(0.35, actual.Options.Threshold)
        CollectionAssert.AreEqual(report.Rows.RowIds, actual.RowIds)
        CollectionAssert.AreEqual(If(Not enabled, New String() {}, If(calibration, New String() {"ROC", "Calibration"}, New String() {"ROC"})), actual.ChartHooks.ToArray())
        If enabled Then Assert.ThrowsException(Of ArgumentException)(Sub() GeeReportPolicy.Classification(report, New RegressionOptions With {.Classification = True, .Threshold = Double.NaN}))
        Dim nonbin = Request("Gaussian|Identity|Independence|Robust|time"), g = nonbin.CreateCoreModel() : g.Fit(False)
        Assert.AreEqual(0, GeeReportPolicy.Classification(Gee12PolicyTests.Report(nonbin, g), expectedOptionsWithFamily(nonbin)).LegacyTables().Count)
    End Sub
    Private Shared Function expectedOptionsWithFamily(intent As GeeWorkflowIntent) As RegressionOptions
        Return New RegressionOptions With {.Family = intent.MeanRequest.Options.Family, .Classification = True}
    End Function
End Class
