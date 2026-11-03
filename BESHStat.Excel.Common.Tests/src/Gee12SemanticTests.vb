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
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Gee12SemanticTests
    <DataTestMethod><DataRow(False)><DataRow(True)><DataRow(True, True)>
    Public Sub ActualImportSortMatchesCoreUtilityWithoutResortingTies(hasTime As Boolean, Optional allEqual As Boolean = False)
        Dim clusters As Object() = {2.0, 1.0, 2.0, 1.0, 2.0, 1.0}, times = {9.0, 4.0, 1.0, 2.0, 1.0, 2.0}
        If allEqual Then times = New Double() {3, 3, 3, 3, 3, 3}
        Dim input(5, If(hasTime, 5, 4)) As Object
        For i = 0 To 5
            input(i, 0) = CDbl(i + 1) : input(i, 1) = CDbl(i) : input(i, 2) = clusters(i)
            Dim c = 3
            If hasTime Then input(i, c) = times(i) : c += 1
            input(i, c) = 0.1 * i : input(i, c + 1) = CDbl(i + 1)
        Next
        Dim raw As New geeData With {.bTime = hasTime, .bOffset = True, .bWeights = True}
        raw.DataImportRawMatrix(input, If(hasTime, {"y", "x", "cluster", "time", "offset", "weight"}, {"y", "x", "cluster", "offset", "weight"}), 31)
        Dim ids = Enumerable.Range(31, 6).ToArray(), permutation = GeeWorkflowPolicy.ImportSortPermutation(clusters, ids, If(hasTime, times, Nothing))
        CollectionAssert.AreEqual(permutation.Select(Function(i) ids(i)).ToArray(), raw.RowIds)
        For i = 0 To 5
            Assert.AreEqual(CDbl(permutation(i) + 1), CDbl(raw.FinalData(i, 0)))
            Assert.AreEqual(0.1 * permutation(i), raw.OffsetData(i))
            Assert.AreEqual(CDbl(permutation(i) + 1), raw.WeightData(i))
        Next
        Dim selections As New Dictionary(Of Integer, Integer) From {{5, 801}, {1, 83}, {3, 271}}
        Dim before = raw.ClusterIdData.ToArray()
        raw.SubsetByRowIdValues(selections)
        CollectionAssert.AreEqual(New Integer() {801, 83, 271}, raw.RowIds)
        CollectionAssert.AreEqual(New Object() {before(5), before(1), before(3)}, raw.ClusterIdData)
    End Sub

    <TestMethod>
    Public Sub NumericAndTextClusterAcquisitionLimitationsAreCharacterized()
        Dim input As Object(,) = {{1.0, 0.0, "z"}, {2.0, 1.0, "a"}, {3.0, 2.0, "z"}, {4.0, 3.0, "a"}}
        Dim rejected As New geeData
        rejected.DataImportRawMatrix(input, {"y", "x", "cluster"}, 9)
        Assert.IsTrue(rejected.bZeroValid, "The unchanged GUI default numeric importer drops nonnumeric clusters; do not claim original text labels were fitted.")
        Dim permitted As New geeData
        permitted.DataImportRawMatrix(input, {"y", "x", "cluster"}, 9, CharCols:=2)
        Assert.AreEqual(4, permitted.nRows)
        CollectionAssert.AreEqual(New Object() {"a", "a", "z", "z"}, permitted.ClusterIdData)
        Dim options As New RegressionOptions With {.Family = RegressionFamily.Gaussian, .Link = RegressionLink.Identity}
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(Gee12Fixture.Matrix(permitted), permitted.varNames, permitted.RowIds,
            New RegressionEffectModel({New RegressionAuthoredEffect("x", Nothing)}), {"y", "x"}), options)
        Dim intent = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("cluster", permitted.ClusterIdData, permitted.RowIds))
        Dim fit = intent.CreateCoreModel() : fit.Fit(False)
        Assert.AreEqual(2, fit.NoGroup)
        CollectionAssert.AreEqual(permitted.RowIds, Gee12PolicyTests.Report(intent, fit).Rows.RowIds)
    End Sub

    <TestMethod>
    Public Sub CategoricalPolynomialAndInteractionUseActualCoreExpansionAndParameterCount()
        Dim source = Gee12Fixture.Raw("Gaussian|Identity|Independence|Robust|time").CoreTable.ObjectMatrix
        Dim input(source.GetLength(0) - 1, 6) As Object
        For i = 0 To source.GetLength(0) - 1
            input(i, 0) = source(i, 0) : input(i, 1) = source(i, 1) : input(i, 2) = CDbl(i Mod 3)
            For j = 2 To 5 : input(i, j + 1) = source(i, j) : Next
        Next
        Dim raw As New geeData With {.bTime = True, .bOffset = True, .bWeights = True}
        raw.DataImportRawMatrix(input, {"y", "x", "factor", "cluster", "visit", "offset", "weight"}, 17)
        Dim model As New RegressionEffectModel, catalog = {"x", "factor"}
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {0}, model, RegressionEffectOperation.ContinuousMain).Model
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {1}, model, RegressionEffectOperation.CategoricalMain).Model
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {0}, model, RegressionEffectOperation.Polynomial, 2).Model
        model = RegressionEffectAuthoringPolicy.Apply(catalog, {0, 1}, model, RegressionEffectOperation.CustomInteraction).Model
        Dim data(,) As Double = Nothing, names() As String = Nothing
        RegressionDesignCore.BuildExpandedRegressionDataMatrix(raw, "y", model.Effects.Select(Function(e) e.Key).ToArray(), model.ToTermSpecs(), GeeWorkflowPolicy.OmitCategoricalReference, data, names)
        Assert.IsTrue(names.Length > raw.nCols)
        Dim o As New RegressionOptions With {.Family = RegressionFamily.Gaussian, .Link = RegressionLink.Identity, .MaximumIterations = 50}
        Dim keys = New String() {"y", "x", "factor"}
        For Each difference In {0, -1, 1}
            Dim starts(names.Length + difference - 1) As Double
            Dim r As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(data, names, raw.RowIds, model, keys, starts), o)
            Dim intent = GeeWorkflowPolicy.Project(r, New GeeClusterVector("cluster", raw.ClusterIdData, raw.RowIds), New RegressionVector("visit", raw.TimeData, raw.RowIds))
            Assert.AreEqual(data.GetLength(1), intent.ExpectedParameterCount)
            Assert.AreEqual(difference <> 0, intent.Validation.Diagnostics.Any(Function(d) d.Code.EndsWith("initialValues.count", StringComparison.Ordinal)))
        Next
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y", New RegressionComponent(data, names, raw.RowIds, model, keys), o)
        Dim request = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("cluster", raw.ClusterIdData, raw.RowIds))
        Dim fit = request.CreateCoreModel() : fit.Fit(False)
        Assert.AreEqual(names.Length, fit.Nparams)
        Assert.AreEqual(names.Length, fit.results.Coeffs_est.Length)
    End Sub

    <TestMethod>
    Public Sub CorrelationCovarianceUsePAndWeightsRetainActualCoreSemantics()
        Dim r = Gee12PolicyTests.Request("Gaussian|Identity|Exchangable|Robust|time"), o = r.Options : o.UseP = True
        Dim intent = GeeWorkflowPolicy.Project(r.MeanRequest, r.Cluster, r.Time, o), fit = intent.CreateCoreModel()
        fit.Fit(False) : Assert.IsTrue(fit.UseP)
        Dim mean = r.MeanRequest
        Dim unweighted As New RegressionRequest(mean.Kind, mean.ResponseKey, mean.Primary, mean.Options, mean.Offset, Nothing)
        Dim noWeights = GeeWorkflowPolicy.Project(unweighted, r.Cluster, r.Time, r.Options), baseline = r.CreateCoreModel(), other = noWeights.CreateCoreModel()
        baseline.Fit(False) : other.Fit(False)
        CollectionAssert.AreEqual(baseline.results.Coeffs_est, other.results.Coeffs_est)
        CollectionAssert.AreEqual(baseline.results.Coeffs_SEs, other.results.Coeffs_SEs)
        Assert.AreNotEqual(1.0, baseline.ObservationWeights.Max())
        Assert.IsTrue(other.ObservationWeights.All(Function(v) v = 1))
        For Each se In GeeWorkflowPolicy.StandardErrorNames()
            Assert.AreEqual(baseline.Nparams, baseline.GetParameterCovarianceMatrix(se).GetLength(0))
        Next
    End Sub

    <TestMethod>
    Public Sub OptionalSectionsNonfiniteResultsAndSingleClusterAreNotInvented()
        Dim r = Gee12PolicyTests.Request("Gaussian|Identity|Independence|Robust|time"), o = r.MeanRequest.Options
        o.Residuals = False : o.IterationDetails = False
        Dim mean As New RegressionRequest(r.MeanRequest.Kind, "y", r.MeanRequest.Primary, o)
        Dim intent = GeeWorkflowPolicy.Project(mean, r.Cluster, r.Time), fit = intent.CreateCoreModel()
        fit.Fit(False)
        Dim report = GeeReportPolicy.Compose(intent, fit.wrapResults(), fit.PredictedResponses, Nothing, fit.ObservedResponses, fit.ObservationWeights)
        Assert.AreEqual(5, report.LegacyTables().Count) : Assert.IsNull(report.Rows.Residuals)
        Dim clusters = Enumerable.Repeat(CType("only", Object), r.MeanRequest.Primary.RowIds.Length).ToArray()
        Dim one = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("subject", clusters, r.MeanRequest.Primary.RowIds))
        Assert.IsTrue(one.Validation.Diagnostics.Any(Function(d) d.Code = "gee.cluster.single"))
        Dim singleton = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("subject", r.MeanRequest.Primary.RowIds.Select(Function(v) CType(v, Object)).ToArray(), r.MeanRequest.Primary.RowIds))
        Assert.IsTrue(singleton.Validation.Diagnostics.Any(Function(d) d.Code = "gee.cluster.singleton"))
        Dim predictions = Enumerable.Repeat(Double.NaN, r.MeanRequest.Primary.RowIds.Length).ToArray()
        Assert.IsTrue(GeeReportPolicy.Predictions(r, predictions).All(Function(v) Double.IsNaN(v)))
    End Sub

    <TestMethod>
    Public Sub ExistingContrastProfilesAreTypedObservedRowsWithoutNewGuiOrInference()
        Dim design As Double(,) = {{1, 0, 2}, {1, 0, 4}, {1, 1, 8}, {1, 1, 10}}
        Dim names = {"Intercept", "factor[1]", "x"}, times = {0.0, 1, 0, 1}
        Dim components As New List(Of GeeContrastComponent) From {
            New GeeContrastComponent("difference", -1, {New KeyValuePair(Of Integer, Double)(1, 0)}),
            New GeeContrastComponent("DIFFERENCE", 1, {New KeyValuePair(Of Integer, Double)(1, 1)}),
            New GeeContrastComponent("at visit", 1, Nothing, 1), New GeeContrastComponent("zero", 0, Nothing)}
        Dim plan = GeeContrastPolicy.Project(design, names, components, times)
        Assert.AreEqual("NO_SHIPPING_GUI_CONTRAST_PATH", plan.ShippingGuiPath)
        CollectionAssert.AreEqual(New String() {"difference", "at visit"}, plan.Labels)
        CollectionAssert.AreEqual(New Integer() {2, 2, 2}, plan.MatchedCounts)
        CollectionAssert.AreEqual(New Double() {0, 1, 6, 1, 0.5, 7}, plan.Contrast.Cast(Of Double)().ToArray())
        design(0, 0) = 99 : names(0) = "changed" : components.Clear()
        Assert.AreEqual("Intercept", plan.ParameterNames(0))
        Dim copy = plan.Contrast : copy(0, 0) = 42 : Assert.AreEqual(0.0, plan.Contrast(0, 0))
        Dim unmatched = GeeContrastPolicy.Project(New Double(,) {{1, 2}}, {"Intercept", "x"}, {New GeeContrastComponent("absent", 1, Nothing, 999)})
        Assert.IsTrue(unmatched.Diagnostics.Any(Function(d) d.Code = "gee.contrast.unmatched"))
        Assert.AreEqual(0, unmatched.Contrast.GetLength(0))
        Assert.AreEqual(AnalysisDocumentStatus.Failed, GeeContrastPolicy.Response(Nothing).Status)
    End Sub

    <TestMethod>
    Public Sub PortableDefaultsDoNotRoundTripNumbersThroughLocalizedStrings()
        For Each culture In {"", "sk-SK"}
            Dim saved = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim r = Gee12PolicyTests.Request("Gaussian|Power|Independence|Robust|time")
                Assert.AreEqual(0.1, r.EffectiveFitAlpha) : Assert.AreEqual(0.5, r.MeanRequest.Options.Power.Value)
                Assert.AreEqual(0.000001, r.MeanRequest.Options.Epsilon)
                Assert.AreEqual(50, r.MeanRequest.Options.MaximumIterations)
                Assert.AreEqual(GeeStandardError.Robust, r.Options.StandardError)
            Finally
                CultureInfo.CurrentCulture = saved
            End Try
        Next
    End Sub
End Class
