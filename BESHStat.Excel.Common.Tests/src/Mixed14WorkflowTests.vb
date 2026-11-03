Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Mixed14Requests
    Public Shared Function Effects(intercept As Boolean, Optional randomInterceptOnly As Boolean = False) As RegressionEffectModel
        Return New RegressionEffectModel(If(randomInterceptOnly, Array.Empty(Of RegressionAuthoredEffect)(),
            New RegressionAuthoredEffect() {New RegressionAuthoredEffect("visit", New TermSpec With {.Kind = "MainEffect", .BaseVarKeys = New List(Of String) From {"visit"}, .Degree = 1})}), intercept)
    End Function
    Public Shared Function Options(key As String) As MixedModelWorkflowOptions
        Dim p = key.Split("|"c)
        Return New MixedModelWorkflowOptions With {.ResidualStructure = If(p(0) = "MMRM", p(1), "Identity"), .RandomStructure = p(1),
            .FitMethod = p(3), .Inference = p(2), .MaximumIterations = If(p(4) = "limit", 1, 50), .Trace = True}
    End Function
    Public Shared Function Request(key As String, Optional options As MixedModelWorkflowOptions = Nothing, Optional input As MixedModelWorkflowData = Nothing) As MixedModelWorkflowRequest
        Dim p = key.Split("|"c), f = Mixed14Fixture.Load(key), kind = If(p(0) = "MMRM", MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM)
        Return New MixedModelWorkflowRequest(kind, If(input, MixedModelWorkflowData.FromArrays(kind, f.Y, f.X, f.Subjects, f.Visit, f.Rows, f.Z)),
            "response", "subject", If(f.Visit Is Nothing, "", "visit"), f.FixedNames, f.RandomNames, Effects(True),
            If(kind = MixedModelWorkflowKind.MMRM, Nothing, Effects(True, f.RandomNames.Length = 1)),
            "visit", If(kind = MixedModelWorkflowKind.MMRM, "", If(f.RandomNames.Length = 1, "Random intercept only", "visit")), If(options, Mixed14Requests.Options(key)))
    End Function
End Class

<TestClass>
Public Class Mixed14WorkflowTests
    <DataTestMethod><DynamicData(NameOf(Mixed14Fixture.Cases), GetType(Mixed14Fixture), DynamicDataSourceType.Method)>
    Public Sub CommonRequestsFitsAndReportsMatchMergedCoreCaptures(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim intent = Mixed14Requests.Request(key)
                Assert.IsTrue(intent.Validation.IsValid, String.Join(";", intent.Validation.Diagnostics.Select(Function(d) d.Code)))
                Dim req = intent.CreateCoreRequest(), result = Mixed14Fixture.Fit(key, req)
                Dim report = MixedModelReportPolicy.Project(intent.Kind, result, New MixedModelReportOptions With {.IterationDetails = True, .Diagnostics = False})
                Dim actual = Mixed14Fixture.Capture(key, req, result, report.LegacyTables())
                Dim path = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A14", "baselines", Mixed14Fixture.Name(key, culture))
                Regression10Fixture.AssertCapture(File.ReadAllLines(path), actual, key)
                Assert.AreEqual(result.Converged, report.Converged.Value)
                Assert.AreEqual(result.Cancelled, report.Cancelled.Value)
                Assert.AreEqual(result.Interrupted, report.Interrupted.Value)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Public Shared Function ParserTokens() As IEnumerable(Of Object())
        Return New String() {Nothing, "", "ML", "Maximum Likelihood", "REML", "Restricted maximum likelihood", "KR", "Kenward-Roger", "Satterthwaite", "Satterthwaite DF",
            "Between-within", "BW", "Residual DF", "Normal", "Large-sample normal", "unknown", "AI", "Average Information", "Fisher scoring", "AI/Fisher scoring (default)",
            "BFGS", "BFGS analytic", "Projected BFGS (auto gradient)", "Projected BFGS (analytic gradient)", "Projected BFGS (finite-difference gradient)",
            "Auto (analytic where available)", "Analytic score", "Analytic score + finite-difference validation", "Numerical finite difference", "Analytic validation"}.
            Select(Function(s) New Object() {s})
    End Function
    <DataTestMethod><DynamicData(NameOf(ParserTokens), DynamicDataSourceType.Method)>
    Public Sub ExtractedOptionParsersMatchExactOriginalAliases(token As String)
        For Each culture In {"en-US", "sk-SK"}
            Dim saved = CultureInfo.CurrentCulture
            Try
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Assert.AreEqual(LegacyMixed14Options.ParseFitMethod(token), MixedModelOptionPolicy.ParseFitMethod(token))
                Assert.AreEqual(LegacyMixed14Options.ParseInferenceMethod(token), MixedModelOptionPolicy.ParseInferenceMethod(token))
                For Each fallback In {MixedModelCovarianceOptimizerMode.AverageInformationReml, MixedModelCovarianceOptimizerMode.ProjectedBfgs}
                    Assert.AreEqual(LegacyMixed14Options.ParseCovarianceOptimizerMode(token, fallback), MixedModelOptionPolicy.ParseCovarianceOptimizerMode(token, fallback))
                Next
                Assert.AreEqual(LegacyMixed14Options.ParseCovarianceGradientMode(token), MixedModelOptionPolicy.ParseCovarianceGradientMode(token))
                Assert.AreEqual(LegacyMixed14Options.NormalizeToken(token), MixedModelOptionPolicy.NormalizeToken(token))
            Finally
                CultureInfo.CurrentCulture = saved
            End Try
        Next
    End Sub

    <DataTestMethod><DataRow(Nothing)><DataRow("unknown")><DataRow("kenward-roger")><DataRow("Kenward-Roger")><DataRow("Large-sample normal")><DataRow("Satterthwaite")>
    Public Sub GuiInferenceFallbackAndKrMlGuardRemainDistinct(text As String)
        For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
            Dim choice = MixedModelWorkflowPolicy.DecideFit(kind, "ml", text)
            Dim coerce = String.Equals(text, "Kenward-Roger", StringComparison.OrdinalIgnoreCase)
            Assert.AreEqual(coerce, choice.CoerceToReml)
            Assert.AreEqual(If(coerce, MixedModelFitMethod.REML, MixedModelFitMethod.ML), choice.EffectiveMethod)
            If text = "kenward-roger" OrElse text Is Nothing OrElse text = "unknown" Then Assert.AreEqual(If(kind = MixedModelWorkflowKind.MMRM, MixedModelFixedInferenceMethod.BetweenWithin, MixedModelFixedInferenceMethod.WaldNormal), choice.EffectiveInference)
            If text = "Kenward-Roger" Then Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, choice.EffectiveInference)
        Next
    End Sub

    <DataTestMethod><DataRow("AI/Fisher scoring (default)")><DataRow("Projected BFGS (auto gradient)")><DataRow("Projected BFGS (analytic gradient)")><DataRow("Projected BFGS (finite-difference gradient)")><DataRow("unknown")><DataRow(Nothing)>
    Public Sub StructControlAssignmentOptimizerOverridesAndTraceSurvive(text As String)
        For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
            Dim options = Mixed14Requests.Options("MMRM|CS|Large-sample normal|REML|toy")
            options.Optimizer = text : options.Gradient = "Analytic score + finite-difference validation"
            options.IterationDetails = True : options.Trace = False : options.Epsilon = 0.0001 : options.MaximumIterations = 17
            Dim key = If(kind = MixedModelWorkflowKind.MMRM, "MMRM|CS|Large-sample normal|REML|toy", "LMM|Random Intercept|Large-sample normal|REML|toy")
            If kind = MixedModelWorkflowKind.LMM Then options.ResidualStructure = "Identity" : options.RandomStructure = "Random Intercept"
            Dim req = Mixed14Requests.Request(key, options).CreateCoreRequest()
            Assert.AreEqual(17, req.Control.MaxIter) : Assert.AreEqual(0.0001, req.Control.Epsilon)
            Assert.AreEqual(req.Control.Epsilon, req.Control.StepTolerance) : Assert.AreEqual(req.Control.Epsilon, req.Control.FunctionTolerance)
            Assert.IsTrue(req.Control.Trace)
            If text = "Projected BFGS (analytic gradient)" Then Assert.AreEqual(MixedModelCovarianceGradientMode.AnalyticScore, req.Control.CovarianceGradientMode)
            If text = "Projected BFGS (finite-difference gradient)" Then Assert.AreEqual(MixedModelCovarianceGradientMode.NumericalFiniteDifference, req.Control.CovarianceGradientMode)
            If text = "unknown" Then Assert.AreEqual(If(kind = MixedModelWorkflowKind.MMRM, MixedModelCovarianceOptimizerMode.ProjectedBfgs, MixedModelCovarianceOptimizerMode.AverageInformationReml), req.Control.CovarianceOptimizerMode)
        Next
    End Sub

    <DataTestMethod><DataRow("Projected BFGS (auto gradient)")><DataRow("Projected BFGS (analytic gradient)")><DataRow("Projected BFGS (finite-difference gradient)")>
    Public Sub BfgsFitsUseSameOriginalCoreConfiguration(text As String)
        Dim key = "MMRM|CS|Satterthwaite|REML|toy", f = Mixed14Fixture.Load(key), options = Mixed14Requests.Options(key)
        options.Optimizer = text
        Dim expected = Mixed14Fixture.LegacyRequest(key, f), ctl = expected.Control
        ctl.CovarianceOptimizerMode = LegacyMixed14Options.ParseCovarianceOptimizerMode(text, MixedModelCovarianceOptimizerMode.ProjectedBfgs)
        ctl.CovarianceGradientMode = LegacyMixed14Options.ParseCovarianceGradientMode(options.Gradient)
        If text = "Projected BFGS (analytic gradient)" Then ctl.CovarianceGradientMode = MixedModelCovarianceGradientMode.AnalyticScore
        If text = "Projected BFGS (finite-difference gradient)" Then ctl.CovarianceGradientMode = MixedModelCovarianceGradientMode.NumericalFiniteDifference
        expected.Control = ctl
        Dim actual = Mixed14Requests.Request(key, options).CreateCoreRequest()
        Regression10Fixture.AssertCapture(Mixed14Fixture.Capture(key, expected), Mixed14Fixture.Capture(key, actual), text)
    End Sub

    <TestMethod>
    Public Sub ImmutableBlockSnapshotPreservesUnsortedSourceRowsAndTiePermutation()
        Dim y = New Double() {5, 6, 8, 9}, x = New Double(,) {{1, 2}, {1, 0}, {1, 0}, {1, 0}}, subjects = New Object() {"z", "A", "z", "z"}, visits = New Double() {2, 0, 0, 0}, rows = New Integer() {107, 101, 109, 103}
        Dim data = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, y, x, subjects, visits, rows)
        CollectionAssert.AreEqual({101, 103, 109, 107}, data.BlockRowIds)
        CollectionAssert.AreEqual({1, 3, 2, 0}, data.BlockToInputIndices)
        y(0) = 100 : x(0, 0) = 100 : rows(0) = 0 : subjects(0) = "changed" : visits(0) = 100
        Assert.AreEqual(5.0, data.CoreData.GetBlock(1).Y(2))
        Assert.AreEqual(107, data.InputRowIds(0)) : Assert.AreEqual("z", CStr(data.SubjectIds(0)))
        data.CoreData.GetBlock(0).X(0, 0) = 200 : Assert.AreEqual(1.0, data.CoreData.GetBlock(0).X(0, 0))
        Dim noVisit = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.LMM, {1.0, 2.0}, New Double(,) {{1}, {1}}, {"b", "b"}, Nothing, {107, 101}, New Double(,) {{1}, {1}})
        CollectionAssert.AreEqual({101, 107}, noVisit.BlockRowIds)
    End Sub

    <TestMethod>
    Public Sub InvalidShapesNonfiniteRowsAndSubjectCollisionsStayBoundedDiagnostics()
        Dim f = Mixed14Fixture.Load("MMRM|CS|Large-sample normal|REML|toy")
        Dim candidates = New MixedModelWorkflowData() {
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, Nothing, Nothing, Nothing, Nothing, Nothing),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {}, New Double(0, 0) {}, {}, {}, {}),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, f.Y, New Double(1, 1) {}, f.Subjects, f.Visit, f.Rows),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.LMM, f.Y, f.X, f.Subjects, f.Visit, f.Rows),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, f.Y, f.X, f.Subjects, Nothing, f.Rows),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {Double.NaN}, New Double(,) {{1}}, {"A"}, {0.0}, {101}),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {1.0}, New Double(,) {{Double.PositiveInfinity}}, {"A"}, {0.0}, {101}),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {1.0}, New Double(,) {{1}}, {Nothing}, {0.0}, {101}),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {1.0, 2.0}, New Double(,) {{1}, {1}}, {1.0, "1"}, {0.0, 1.0}, {101, 103}),
            MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {1.0, 2.0}, New Double(,) {{1}, {1}}, {"a", "b"}, {0.0, 1.0}, {101, 101})}
        For Each candidate In candidates
            Assert.IsNull(candidate.CoreData)
            Assert.IsTrue(candidate.Diagnostics.Any(Function(d) d.Blocking))
            Assert.IsTrue(candidate.Diagnostics.Count <= MixedModelWorkflowPolicy.MaximumDiagnostics)
        Next
    End Sub

    <DataTestMethod><DataRow(0, 0.000001)><DataRow(-1, 0.000001)><DataRow(50, 0.0)><DataRow(50, -1.0)><DataRow(50, Double.NaN)><DataRow(50, Double.PositiveInfinity)>
    Public Sub StricterPortableControlChecksDoNotActivateNewGuiRefusals(iterations As Integer, epsilon As Double)
        Dim options = Mixed14Requests.Options("MMRM|CS|Large-sample normal|REML|toy")
        options.MaximumIterations = iterations : options.Epsilon = epsilon
        Dim request = Mixed14Requests.Request("MMRM|CS|Large-sample normal|REML|toy", options)
        Assert.IsFalse(request.Validation.IsValid)
        Assert.ThrowsException(Of ArgumentException)(Function() request.CreateCoreRequest())
        Assert.IsNotNull(request.CreateAcquiredCoreRequest(), "Shipping compatibility seam does not add a new strict refusal.")
        Assert.AreEqual(epsilon <= 0, MixedModelWorkflowPolicy.LegacyRejectEpsilon(epsilon))
    End Sub

    <TestMethod>
    Public Sub BadCovarianceRolesAndAuthoredDimensionsDoNotBecomeValidModels()
        Dim key = "LMM|Random Intercept + Slope|Large-sample normal|REML|toy", o = Mixed14Requests.Options(key)
        o.RandomStructure = "Random Intercept" : o.ResidualStructure = "unknown"
        Dim r = Mixed14Requests.Request(key, o)
        Assert.IsFalse(r.Validation.IsValid)
        Assert.IsTrue(r.Validation.Diagnostics.Any(Function(d) d.Code = "mixed.random.intercept"))
        Assert.IsTrue(r.Validation.Diagnostics.Any(Function(d) d.Code = "mixed.residual.structure"))
        o.ResidualStructure = "AR1"
        Assert.IsFalse(Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|novisit", o).Validation.IsValid)
        o.RandomStructure = "none" : o.ResidualStructure = "Identity"
        Assert.IsFalse(Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy", o).Validation.IsValid)
    End Sub

    <TestMethod>
    Public Sub RandomAuthoringPromotesExactlyTheOriginalSimpleStructures()
        Dim intercept = Mixed14Requests.Effects(True, True), slope = Mixed14Requests.Effects(True)
        Assert.AreEqual("Random Intercept", MixedModelWorkflowPolicy.PromotedRandomStructure("Random Intercept", intercept, False))
        Assert.AreEqual("Random Intercept + Slope", MixedModelWorkflowPolicy.PromotedRandomStructure("Random Intercept", slope, False))
        Assert.AreEqual("Variance Components (VC/Diag)", MixedModelWorkflowPolicy.PromotedRandomStructure("Random Intercept", Mixed14Requests.Effects(False), False))
        Assert.AreEqual("Variance Components (VC/Diag)", MixedModelWorkflowPolicy.PromotedRandomStructure("Random Intercept + Slope", slope, True))
        Assert.AreEqual("Identity", MixedModelWorkflowPolicy.PromotedRandomStructure("Identity", slope, True))
        Dim orphan As New RegressionEffectModel({New RegressionAuthoredEffect("orphan", New TermSpec With {.Kind = "Interaction"})}, True)
        Assert.AreEqual("Variance Components (VC/Diag)", MixedModelWorkflowPolicy.PromotedRandomStructure("Random Intercept", orphan, False, 0), "Original helper scans all dictionary specs, including orphan entries.")
    End Sub

    <TestMethod>
    Public Sub RequestAndReportCopiesDoNotHoldControlsOrMutableCoreResults()
        Dim key = "LMM|Random Intercept|Large-sample normal|REML|toy", options = Mixed14Requests.Options(key), request = Mixed14Requests.Request(key, options)
        options.Epsilon = -1 : request.Options.MaximumIterations = -1 : request.FixedNames(0) = "changed"
        Assert.AreEqual(50, request.Options.MaximumIterations) : Assert.AreEqual("Intercept", request.FixedNames(0))
        Dim result = Mixed14Fixture.Fit(key, request.CreateCoreRequest())
        Dim report = MixedModelReportPolicy.Project(request.Kind, result, New MixedModelReportOptions With {.Diagnostics = False})
        Dim initial = report.Document.Tables(0).Model.Values(0, 0)
        report.Document.Tables(0).Model.Values(0, 0) = "changed"
        result.AdditionalResultTables.Clear() : result.Beta(0) = 100
        Assert.AreEqual(initial, report.Document.Tables(0).Model.Values(0, 0))
        Dim unavailable = MixedModelReportPolicy.Project(request.Kind, Nothing, New MixedModelReportOptions)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, unavailable.Document.Status) : Assert.IsFalse(unavailable.Converged.HasValue)
    End Sub

    <TestMethod>
    Public Sub CancellationInterruptionAndNonconvergenceComeFromCore()
        Dim key = "LMM|Random Intercept|Large-sample normal|REML|toy", req = Mixed14Requests.Request(key).CreateCoreRequest()
        req.CancellationRequested = Function() True
        Dim result = Mixed14Fixture.Fit(key, req)
        Assert.IsTrue(result.Cancelled) : Assert.IsFalse(result.Converged)
        req = Mixed14Requests.Request(key).CreateCoreRequest() : req.InterruptionRequested = Function() True
        result = Mixed14Fixture.Fit(key, req)
        Assert.IsTrue(result.Interrupted) : Assert.IsFalse(result.Converged)
        result = Mixed14Fixture.Fit(key, Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|limit").CreateCoreRequest())
        Assert.IsFalse(result.Converged)
        result.ReleaseLargePostEstimationWorkspaces()
        Assert.AreEqual(0, result.AdditionalResultTables.Count)
    End Sub

    <DataTestMethod><DataRow("singular")><DataRow("near-singular")><DataRow("constant-response")><DataRow("constant-covariate")><DataRow("tied-visit")>
    Public Sub DifficultDesignsPreserveActualCoreOutcomes(scenario As String)
        Dim key = "MMRM|CS|Satterthwaite|REML|toy", f = Mixed14Fixture.Load(key)
        For i = 0 To f.Y.Length - 1
            Select Case scenario
                Case "singular" : f.X(i, 1) = f.X(i, 0)
                Case "near-singular" : f.X(i, 1) = 1 + f.Visit(i) * 0.000000000001
                Case "constant-response" : f.Y(i) = 8
                Case "constant-covariate" : f.X(i, 1) = 0
                Case "tied-visit" : f.Visit(i) = 0
            End Select
        Next
        Dim original = Mixed14Fixture.LegacyRequest(key, f)
        Dim data = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, f.Y, f.X, f.Subjects, f.Visit, f.Rows)
        Dim current = Mixed14Requests.Request(key, input:=data).CreateCoreRequest()
        Regression10Fixture.AssertCapture(Mixed14Fixture.Capture(key, original), Mixed14Fixture.Capture(key, current), scenario)
        ' A matching original fault/nonconverged/undefined outcome is characterization,
        ' never proof of scientifically valid inference for a deficient design.
    End Sub

    <TestMethod>
    Public Sub EmptyAuthoringBadRoleMetadataAndMissingPredictorsAreExplicit()
        Dim normal = Mixed14Requests.Request("LMM|Random Intercept + Slope|Large-sample normal|REML|toy"), o = normal.Options
        o.FixedIntercept = False : o.RandomIntercept = False
        Dim request As New MixedModelWorkflowRequest(normal.Kind, normal.Data, "", "", "", {"x", "x"}, {"", "z"},
            New RegressionEffectModel(includeIntercept:=False), New RegressionEffectModel(includeIntercept:=False), "", "", o)
        Assert.IsFalse(request.Validation.IsValid)
        For Each code In {"mixed.roles", "mixed.fixed.empty", "mixed.fixed.names", "mixed.random.names", "mixed.random.slope"}
            Assert.IsTrue(request.Validation.Diagnostics.Any(Function(d) d.Code = code))
        Next
        Assert.AreEqual(500, MixedModelWorkflowOptions.Defaults(MixedModelWorkflowKind.MMRM).MaximumIterations)
        Assert.AreEqual("Identity", MixedModelWorkflowOptions.Defaults(MixedModelWorkflowKind.LMM).ResidualStructure)
        Dim bad = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.LMM, {1.0, 2.0}, New Double(,) {{1}, {1}},
            {"s", "s"}, {0.0, 1.0}, {101, 103}, New Double(,) {{Double.NaN}})
        Assert.IsNull(bad.CoreData)
        Dim count = 10000, rows = Enumerable.Range(0, count).Select(Function(i) 101 + i * 3).ToArray(), x(count - 1, 0) As Double
        Dim missing = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, New Double(count - 1) {}, x,
            New Object(count - 1) {}, New Double(count - 1) {}, rows)
        Assert.IsNull(missing.CoreData)
        Assert.AreEqual(MixedModelWorkflowPolicy.MaximumDiagnostics, missing.Diagnostics.Count)
    End Sub
End Class
