Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.survival
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Survival13Requests
    Public Shared Function Input(records As List(Of SurvivalRecord), mode As SurvivalAnalysisMode, Optional grouped As Boolean = True,
                                 Optional stratified As Boolean = False) As SurvivalInput
        Dim ids = records.Select(Function(r) r.Index).ToArray()
        Return New SurvivalInput(New RegressionVector("time", records.Select(Function(r) r.Time).ToArray(), ids),
            New RegressionVector("status", records.Select(Function(r) CDbl(r.Censorship)).ToArray(), ids),
            If(grouped AndAlso mode <> SurvivalAnalysisMode.Cox, New SurvivalLabels("group", records.Select(Function(r) r.strGroup).ToArray(), ids), Nothing),
            If(stratified, New SurvivalLabels("stratum", records.Select(Function(r) r.strStratum).ToArray(), ids), Nothing))
    End Function
    Public Shared Function Km(key As String) As SurvivalWorkflowIntent
        Dim p = key.Split("|"c), records = Survival13Fixture.Records(key), code As SurvivalWeight
        Select Case p(1)
            Case "Logrank" : code = SurvivalWeight.Logrank
            Case "Tarone-Ware" : code = SurvivalWeight.TaroneWare
            Case "Gehan-Breslow" : code = SurvivalWeight.GehanBreslow
            Case "Peto" : code = SurvivalWeight.Peto
            Case "modified Peto" : code = SurvivalWeight.ModifiedPeto
        End Select
        Dim grouped = records.Select(Function(r) r.strGroup).Distinct().Count() > 1
        Dim mode = If(p(2) = "strata", SurvivalAnalysisMode.LogRank, SurvivalAnalysisMode.KaplanMeier)
        Return New SurvivalWorkflowIntent(mode, Input(records, mode, grouped, p(2) = "strata"),
            New SurvivalOptions With {.Alpha = If(p(2) = "alpha10", 0.1, 0.05), .Weight = code,
                .DetailedTable = p(2) <> "optional-off", .FixedTimeComparison = p(2) <> "optional-off", .EqualityOfMedians = p(2) <> "optional-off"})
    End Function
    Public Shared Function Cox(key As String) As CoxWorkflowIntent
        Dim p = key.Split("|"c), records = Survival13Fixture.Records(key), x(records.Count - 1, 1) As Double
        For i = 0 To records.Count - 1
            For j = 0 To 1 : x(i, j) = records(i).Covariates(j) : Next
        Next
        Dim options As New CoxOptions With {.Ties = DirectCast([Enum].Parse(GetType(TieMethod), p(1)), TieMethod),
            .MaximumIterations = If(p(2) = "limit", 1, 50), .RobustVariance = p(2) = "robust",
            .AllResiduals = True, .ReturnCovariance = True, .PHScoreTest = True, .IterationDetails = True}
        Return New CoxWorkflowIntent(Input(records, SurvivalAnalysisMode.Cox, False, p(2) = "strata" OrElse p(2) = "emptystratum"),
            New RegressionComponent(x, {"x1", "x2"}, records.Select(Function(r) r.Index).ToArray(), Nothing, {"x1", "x2"},
                If(p(2) = "starts", New Double() {0.1, -0.1}, Nothing)), options)
    End Function
End Class

<TestClass>
Public Class Survival13WorkflowTests
    <DataTestMethod><DynamicData(NameOf(Survival13Fixture.Cases), GetType(Survival13Fixture), DynamicDataSourceType.Method)>
    Public Sub ActualCommonCoreReportRouteMatchesPrechange(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim actual As String() = Nothing
                Try
                    If key.StartsWith("KM|", StringComparison.Ordinal) Then
                        Dim intent = Survival13Requests.Km(key)
                        Assert.IsTrue(intent.Validation.IsValid)
                        Dim model = intent.Execute()
                        Dim report = SurvivalReportPolicy.Compose(intent, model.wrapResults())
                        actual = Survival13Fixture.Snapshot(key, kmModel:=model, projectedTables:=report.LegacyTables())
                        Assert.AreEqual(1, report.BlankRowsBetweenTables)
                    Else
                        Dim intent = Survival13Requests.Cox(key)
                        Assert.IsTrue(intent.Validation.IsValid)
                        Dim model = intent.CreateCoreModel(), result = model.Fit(intent.Options.Ties)
                        Dim report = CoxReportPolicy.Compose(intent, model.wrapResults(If(intent.Input.Strata Is Nothing, Nothing, "stratum"), intent.Options.Alpha), result)
                        Assert.AreEqual(result.Converged, report.Converged.Value)
                        CollectionAssert.AreEqual({"Cox Regression", "Adjusted Curves", "Residuals"}, report.SheetOrder.ToArray())
                        actual = Survival13Fixture.Snapshot(key, coxModel:=model, projectedTables:=report.Report.LegacyTables())
                    End If
                Catch ex As OperationCanceledException
                    Throw
                Catch ex As Exception
                    ' Only the same captured Core failure is acceptable, never fabricated success.
                    actual = {"04A13-Core-NOT_GOLDEN", key, "CORE_FAILURE|" & ex.GetType().FullName}
                End Try
                Dim file = Path.Combine(Regression09Fixture.Root(), "evidence", "04A13", "baselines", Survival13Fixture.FileName(key, culture))
                Regression10Fixture.AssertCapture(IO.File.ReadAllLines(file), actual, key)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(0, "Logrank")><DataRow(1, "Tarone-Ware")><DataRow(2, "Gehan-Breslow")><DataRow(3, "Peto")><DataRow(4, "modified Peto")>
    Public Sub EveryWeightMapsToExactCoreName(code As Integer, expected As String)
        Dim name As String = "prior"
        Assert.IsTrue(SurvivalWorkflowPolicy.TryWeightName(CType(code, SurvivalWeight), name))
        Assert.AreEqual(expected, name)
        Assert.IsFalse(SurvivalWorkflowPolicy.TryWeightName(CType(99, SurvivalWeight), name))
        Assert.IsNull(name)
    End Sub

    <DataTestMethod><DataRow(-1.0)><DataRow(2.0)><DataRow(0.5)><DataRow(Double.NaN)><DataRow(Double.PositiveInfinity)>
    Public Sub ExactStatusDiagnosticsNeverRoundIntoEvents(code As Double)
        Dim input As New SurvivalInput(New RegressionVector("t", {0.0, 1.0}, {101, 107}), New RegressionVector("s", {code, 1.0}, {101, 107}))
        Dim intent As New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, input)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.AreEqual(101, intent.Validation.Diagnostics.First(Function(d) d.Code = "survival.status").SourceRowId.Value)
        Assert.ThrowsException(Of ArgumentException)(Function() intent.Execute())
    End Sub

    <DataTestMethod><DataRow(-1.0)><DataRow(Double.NaN)><DataRow(Double.PositiveInfinity)><DataRow(Double.NegativeInfinity)>
    Public Sub TimeMustBeFiniteAndNonnegative(value As Double)
        Dim input As New SurvivalInput(New RegressionVector("t", {value}, {103}), New RegressionVector("s", {1.0}, {103}))
        Assert.IsFalse(New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, input).Validation.IsValid)
    End Sub

    <DataTestMethod><DataRow(0.0)><DataRow(1.0)><DataRow(-0.05)><DataRow(Double.NaN)><DataRow(Double.PositiveInfinity)>
    Public Sub InvalidAlphaAndUnknownChoicesAreDiagnostic(alpha As Double)
        Dim normal = Survival13Requests.Km("KM|Logrank|single")
        Dim intent As New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, normal.Input,
            New SurvivalOptions With {.Alpha = alpha, .Weight = CType(99, SurvivalWeight), .OutputTarget = CType(99, SurvivalOutputTarget)})
        Assert.IsFalse(intent.Validation.IsValid)
        For Each code In {"survival.alpha", "survival.weight", "survival.outputTarget"}
            Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = code))
        Next
    End Sub

    <TestMethod>
    Public Sub FiveDistinctRoleStatesAndLiteralLabelOrder()
        Dim recs = Survival13Fixture.Records("KM|Logrank|three"), input = Survival13Requests.Input(recs, SurvivalAnalysisMode.KaplanMeier)
        CollectionAssert.AreEqual({"Z", "a", "A"}, input.GroupLabels)
        Dim valid = New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, input)
        Assert.IsTrue(valid.Validation.IsValid)
        Assert.IsTrue(valid.RunWeightedTest) : Assert.IsTrue(valid.RunMedianTest) : Assert.IsFalse(valid.RunFixedTimeTest)
        Dim none = Survival13Requests.Input(recs, SurvivalAnalysisMode.KaplanMeier, False)
        Assert.IsTrue(New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, none).Validation.IsValid)
        Assert.IsFalse(New SurvivalWorkflowIntent(SurvivalAnalysisMode.LogRank, none).Validation.IsValid)
        Assert.IsTrue(New SurvivalWorkflowIntent(SurvivalAnalysisMode.LogRank, input).Validation.IsValid)
        Dim strat = Survival13Requests.Input(recs, SurvivalAnalysisMode.LogRank, True, True)
        Assert.IsTrue(New SurvivalWorkflowIntent(SurvivalAnalysisMode.LogRank, strat).Validation.IsValid)
        Assert.IsFalse(New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, strat).Validation.IsValid)
        Dim cox = Survival13Requests.Cox("Cox|Breslow|strata")
        Assert.IsTrue(cox.Validation.IsValid)
        Dim mapped = SurvivalWorkflowPolicy.CreateRecords(strat, SurvivalAnalysisMode.LogRank)
        CollectionAssert.AreEqual(recs.Select(Function(r) r.Index).ToArray(), mapped.Select(Function(r) r.Index).ToArray())
        CollectionAssert.AreEqual({0, 1, 2}, mapped.Take(3).Select(Function(r) r.Group).ToArray())
        CollectionAssert.AreEqual(cox.Input.Strata.Values, CoxWorkflowPolicy.BuildExpandedRecords(cox.Input, cox.Expanded.Data).Select(Function(r) r.Stratum).ToArray())
    End Sub

    <TestMethod>
    Public Sub ShapeMissingLabelsAndSourceAxesAreNotSilentlyRepaired()
        Dim cases As SurvivalInput() = {
            Nothing,
            New SurvivalInput(Nothing, Nothing),
            New SurvivalInput(New RegressionVector("t", {}, {}), New RegressionVector("s", {}, {})),
            New SurvivalInput(New RegressionVector("t", {1.0, 2.0}, {101, 101}), New RegressionVector("s", {0.0, 1.0}, {101, 101})),
            New SurvivalInput(New RegressionVector("t", {1.0, 2.0}, {101, 103}), New RegressionVector("s", {1.0}, {101})),
            New SurvivalInput(New RegressionVector("t", {1.0}, {101}), New RegressionVector("s", {1.0}, {103})),
            New SurvivalInput(New RegressionVector("t", {1.0}, {101}), New RegressionVector("s", {1.0}, {101}), New SurvivalLabels("g", {Nothing}, {101})),
            New SurvivalInput(New RegressionVector("t", {1.0}, {101}), New RegressionVector("s", {1.0}, {101}), strata:=New SurvivalLabels("s", {""}, {101})),
            New SurvivalInput(New RegressionVector("t", {1.0}, {101}), New RegressionVector("s", {1.0}, {101}), New SurvivalLabels("g", {"B"}, {103}))}
        For Each item As SurvivalInput In cases
            Assert.IsFalse(New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, item).Validation.IsValid)
        Next
        Assert.IsFalse(New SurvivalWorkflowIntent(CType(99, SurvivalAnalysisMode), Nothing).Validation.IsValid)
    End Sub

    <TestMethod>
    Public Sub DefensiveCopiesCoverRolesOptionsRecordsAndReports()
        Dim values = New Double() {0, 2}, ids = New Integer() {101, 107}, labels = New String() {" b ", "A"}
        Dim options As New SurvivalOptions With {.Alpha = 0.1}
        Dim input As New SurvivalInput(New RegressionVector("t", values, ids), New RegressionVector("s", {1.0, 0.0}, ids), New SurvivalLabels("g", labels, ids))
        Dim intent As New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, input, options)
        values(0) = -100 : ids(0) = 0 : labels(0) = "changed" : options.Alpha = -1
        Assert.AreEqual(0.0, input.Time.Values(0)) : Assert.AreEqual(101, input.Time.RowIds(0))
        Assert.AreEqual(" b ", input.Groups.Values(0)) : Assert.AreEqual(0.1, intent.Options.Alpha)
        intent.Options.Alpha = 1 : Assert.AreEqual(0.1, intent.Options.Alpha)
        Dim model = intent.Execute(), report = SurvivalReportPolicy.Compose(intent, model.wrapResults())
        Dim first = report.Document.Tables(0).Model.Values(0, 0)
        report.Document.Tables(0).Model.Values(0, 0) = "mutated"
        Assert.AreEqual(first, report.Document.Tables(0).Model.Values(0, 0))
        Dim r = Survival13Fixture.Records("Cox|Efron|plain"), clone = SurvivalWorkflowPolicy.CopyRecords(r)
        clone(0).Covariates(0) = 123 : Assert.AreNotEqual(123.0, r(0).Covariates(0))
    End Sub

    <DataTestMethod><DataRow("single")><DataRow("groups")><DataRow("events")><DataRow("censored")><DataRow("mixed")><DataRow("zero")><DataRow("alpha10")>
    Public Sub PlotSnapshotPreservesStepCensorAxesAndRequestedPresentation(scenario As String)
        Dim key = "KM|Logrank|" & scenario, intent = Survival13Requests.Km(key), model = intent.Execute()
        Dim options As New SurvivalOptions With {.Alpha = intent.Options.Alpha, .PlotConfidenceIntervals = False, .ShowLegend = False, .DisplayTitle = True, .Title = "Custom title", .XUnits = "years"}
        If scenario = "events" Then
            Assert.ThrowsException(Of InvalidOperationException)(Function() model.GetKaplanMeierPlotData(intent.Options.Alpha))
            Assert.ThrowsException(Of InvalidOperationException)(Function() SurvivalReportPolicy.Plot(model, intent.Options.Alpha, options))
            Return
        End If
        Dim payload = SurvivalReportPolicy.Plot(model, intent.Options.Alpha, options)
        Dim expected As New List(Of String), actual As New List(Of String)
        Survival13Fixture.AddPlot(expected, model.GetKaplanMeierPlotData(intent.Options.Alpha))
        Survival13Fixture.AddPlot(actual, payload.ToCoreData())
        CollectionAssert.AreEqual(expected, actual)
        options.Title = "later"
        Assert.AreEqual("Custom title", payload.Options.Title) : Assert.AreEqual("years", payload.Options.XUnits)
        Assert.AreEqual(intent.GroupCount * 2, payload.SeriesOrder.Count)
        Dim core = payload.ToCoreData()
        core.GroupLabels(0) = "mutation" : Assert.AreNotEqual("mutation", payload.ToCoreData().GroupLabels(0))
        If core.CensoredCountByGroup.Length <> intent.GroupCount Then Assert.IsTrue(payload.Diagnostics.Any(Function(d) d.Code = "survival.plot.censor-axis"))
        ' Core values are preserved, including undefined boundary CI. Each finite survival
        ' column remains nonincreasing; repeated times retain pre/post-event step values.
        core = payload.ToCoreData()
        For g = 0 To core.SurvivalProbability.GetLength(1) - 1
            Dim last = 1.0
            For i = 0 To core.SurvivalProbability.GetLength(0) - 1
                Dim value = core.SurvivalProbability(i, g)
                If Double.IsNaN(value) Then Continue For
                Assert.IsTrue(value <= last + 0.000000000001)
                last = value
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub OptionalCalculationAndLegacyBlankTableAreDistinct()
        Dim intent = Survival13Requests.Km("KM|Logrank|optional-off")
        Assert.IsFalse(intent.RunFixedTimeTest) : Assert.IsFalse(intent.RunMedianTest)
        Assert.AreEqual("LEGACY_HEADER_ONLY", SurvivalReportPolicy.FixedTimeDisposition(intent))
        Dim model = intent.Execute(), report = SurvivalReportPolicy.Compose(intent, model.wrapResults())
        Assert.AreEqual(3, report.Document.Tables.Count)
        Dim noMedian As New SurvivalWorkflowIntent(intent.Mode, intent.Input, New SurvivalOptions With {
            .MedianConfidenceInterval = False, .DetailedTable = False, .EqualityOfMedians = False, .FixedTimeComparison = False})
        Assert.AreEqual(2, SurvivalReportPolicy.Compose(noMedian, noMedian.Execute().wrapResults()).Document.Tables.Count)
        Assert.AreEqual("NOT_APPLICABLE", SurvivalReportPolicy.FixedTimeDisposition(Survival13Requests.Km("KM|Logrank|single")))
        Assert.AreEqual("CORE_COMPUTED", SurvivalReportPolicy.FixedTimeDisposition(Survival13Requests.Km("KM|Logrank|groups")))
    End Sub

    <DataTestMethod><DataRow("Breslow")><DataRow("Efron")><DataRow("Exact")>
    Public Sub CoxCurveAndResidualProjectionPreservesOriginalAxes(tie As String)
        Dim intent = Survival13Requests.Cox("Cox|" & tie & "|strata"), model = intent.CreateCoreModel(), result = model.Fit(intent.Options.Ties)
        For Each pair In model.ComputeBaseline(False)
            Dim curve = CoxReportPolicy.Curve(model, CStr(pair.Key), pair.Value)
            CollectionAssert.AreEqual(pair.Value, curve.Baseline)
            CollectionAssert.AreEqual(model.BaseSurvivalForPloting(pair.Value), curve.StepValues)
            Assert.AreEqual(curve.StepValues.GetLength(0), curve.Times.Length)
            Assert.AreEqual("Time_" & CStr(pair.Key), curve.ColumnNames(0))
            StringAssert.Contains(curve.AdjustedSeriesIntent, "first stratum")
            curve.StepValues(0, 1) = 42 : Assert.AreEqual(1.0, curve.StepValues(0, 1))
        Next
        Dim original = model.wrapResiduals(), payload = CoxReportPolicy.Residuals(intent, original), blocks = payload.LegacyBlocks()
        Assert.AreEqual(original.Count, blocks.Count)
        For i = 0 To blocks.Count - 1 : CollectionAssert.AreEqual(original(i), blocks(i)) : Next
        CollectionAssert.AreEqual(intent.Input.Time.RowIds, payload.RowIds)
        CollectionAssert.AreEqual(intent.Expanded.Data, payload.Predictors)
        CollectionAssert.AreEqual(intent.Input.Time.RowIds, Enumerable.Range(2, blocks(0).GetLength(0) - 2).Select(Function(i) CInt(blocks(0)(i, 0))).ToArray())
        blocks(0)(0, 0) = "changed" : Assert.AreNotEqual("changed", payload.LegacyBlocks()(0)(0, 0))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(3)>
    Public Sub StartsValidatedAgainstExpandedCoefficients(count As Integer)
        Dim normal = Survival13Requests.Cox("Cox|Breslow|plain"), x = normal.Expanded
        Dim start(count - 1) As Double
        Dim intent As New CoxWorkflowIntent(normal.Input, New RegressionComponent(x.Data, x.Names, x.RowIds, Nothing, x.SourceKeys, start))
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "survival.cox.starts.count"))
        Assert.IsTrue(CoxWorkflowPolicy.InitialValuesCountMatches(Nothing, 2))
        Assert.IsFalse(CoxWorkflowPolicy.InitialValuesCountMatches(start, 2))
    End Sub

    <TestMethod>
    Public Sub CoxBadDesignOptionsAndStartsRemainDiagnostics()
        Dim normal = Survival13Requests.Cox("Cox|Breslow|plain"), x = normal.Expanded.Data
        x(0, 0) = Double.NaN
        Dim intent As New CoxWorkflowIntent(normal.Input, New RegressionComponent(x, {"x1", "x1"}, {101}, Nothing, {"x"}, {Double.PositiveInfinity, 0.0}),
            New CoxOptions With {.Ties = CType(99, TieMethod), .MaximumIterations = 0, .Epsilon = Double.NaN})
        For Each code In {"ties", "iterations", "epsilon", "names", "rows", "predictors.finite", "starts.finite"}
            Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "survival.cox." & code))
        Next
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.ThrowsException(Of ArgumentException)(Function() intent.CreateCoreModel())
        Assert.IsFalse(New CoxWorkflowIntent(normal.Input, Nothing).Validation.IsValid)
    End Sub

    <TestMethod>
    Public Sub CoxConvergenceFaultAndCallbackSemanticsAreTruthful()
        Dim intent = Survival13Requests.Cox("Cox|Breslow|limit"), model = intent.CreateCoreModel(), result = model.Fit(intent.Options.Ties)
        Assert.IsFalse(result.Converged)
        Assert.IsFalse(CoxReportPolicy.Compose(intent, model.wrapResults(), result).Converged.Value)
        Dim fault = CoxReportPolicy.Fault(intent, "survival.core.failure", "Core returned no fit.")
        Assert.IsFalse(fault.Converged.HasValue) : Assert.AreEqual(AnalysisDocumentStatus.Failed, fault.Report.Document.Status)
        Assert.AreEqual(0, fault.Report.Document.Tables.Count)
        Try
            CoreServices.Regression.SetCallbacks(Function() True, Function() False)
            Assert.ThrowsException(Of OperationCanceledException)(Function() intent.CreateCoreModel().Fit())
        Finally
            CoreServices.Regression.ClearCallbacks()
        End Try
        Try
            CoreServices.Regression.SetCallbacks(Function() False, Function() True)
            result = Survival13Requests.Cox("Cox|Breslow|plain").CreateCoreModel().Fit()
            Assert.IsFalse(result.Converged)
        Finally
            CoreServices.Regression.ClearCallbacks()
        End Try
        Assert.IsTrue(Survival13Requests.Cox("Cox|Breslow|plain").CreateCoreModel().Fit().Converged)
    End Sub

    <TestMethod>
    Public Sub LargeDiagnosticsAreBoundedWithoutDroppingOrRecodingRows()
        Dim ids = Enumerable.Range(0, 10000).Select(Function(i) 101 + i * 3).ToArray()
        Dim input As New SurvivalInput(New RegressionVector("t", Enumerable.Repeat(-1.0, ids.Length).ToArray(), ids),
            New RegressionVector("s", Enumerable.Repeat(2.0, ids.Length).ToArray(), ids))
        Dim intent As New SurvivalWorkflowIntent(SurvivalAnalysisMode.KaplanMeier, input)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.AreEqual(SurvivalWorkflowPolicy.MaximumDiagnostics, intent.Validation.Diagnostics.Count)
        Assert.AreEqual(10000, intent.Input.Time.RowIds.Length)
    End Sub
End Class
