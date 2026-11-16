Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.CausalInference
Imports Microsoft.VisualStudio.TestTools.UnitTesting
<TestClass, DoNotParallelize>
Public Class GuiWiring47ContractTests
 <DataTestMethod> <DataRow("")> <DataRow("en-US")> <DataRow("sk-SK")>
 Public Sub SuppliedGeeStartsRetainExistingQicInitializationFailure(culture As String)
  Dim saved = CultureInfo.CurrentCulture
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
   Dim key = "Gaussian|Identity|Independence|Robust|start", raw = Gee12Fixture.Raw(key), intent = Gee12PolicyTests.Request(key, raw)
   Dim fit = intent.CreateCoreModel()
   Assert.ThrowsException(Of NullReferenceException)(Sub() fit.Fit(True))
  Finally
   CultureInfo.CurrentCulture = saved
  End Try
 End Sub
 <DataTestMethod> <DataRow("")> <DataRow("bad")> <DataRow("17")> <DataRow("2.5")>
 Public Sub MultivariateSeedAndRegularizationUseExactExistingTextBoundaries(text As String)
  Dim f As New Harness47.Ui11PCAProjection47
  f.tbDASeed.Text = text
  If text = "bad" OrElse text = "2.5" Then
   Assert.IsInstanceOfType(Assert.ThrowsException(Of Reflection.TargetInvocationException)(Sub() GuiWiring47StateTests.Invoke(f, "GetSelectedDiscriminantRandomSeed")).InnerException, GetType(FormatException))
  Else
   Assert.AreEqual(If(text = "", Integer.MinValue, 17), CInt(GuiWiring47StateTests.Invoke(f, "GetSelectedDiscriminantRandomSeed")))
  End If
  f.tbDARegularization.Text = "" : Assert.AreEqual(0.00000001, CDbl(GuiWiring47StateTests.Invoke(f, "GetSelectedDiscriminantRegularization")))
 End Sub
 <DataTestMethod> <DataRow(False, "")> <DataRow(True, "")> <DataRow(False, "en-US")> <DataRow(True, "en-US")> <DataRow(False, "sk-SK")> <DataRow(True, "sk-SK")>
 Public Sub LmCurrentAlphaHeadersAndSourceRowsStayRestrictedAtFakeWriteBoundary(covariance As Boolean, culture As String)
  Dim saved = CultureInfo.CurrentCulture
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
   Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), r = Regression10PolicyTests.Request(f, True, False, False)
   Dim options = r.Options : options.Covariance = covariance
   Dim request As New RegressionRequest(r.Kind, r.ResponseKey, r.Primary, options)
   Dim fit = Regression11DefectTests.FitLinear(LinearBinomialRegressionPolicy.Project(request, alphaMode:=LinearFitAlphaMode.NativeLegacyDefault))
   Assert.AreEqual(0.05, fit.Alpha) : Assert.AreEqual(0.1, request.Options.Alpha)
   Dim names = DirectCast(f.Names.Clone(), String()), headers = WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(request, names)
   CollectionAssert.AreEqual(f.Names, names) : CollectionAssert.AreEqual(f.Ids, request.Primary.RowIds)
   Assert.AreEqual(f.Names(0), headers(0), "The dormant writer substitution does not apply to the LM request kind.")
   Dim glmRequest As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, r.ResponseKey, r.Primary, options)
   Dim glmHeaders = WindowsRegressionRequestAdapter.DeferredCovarianceDataHeaders(glmRequest, names)
   Assert.AreEqual(If(covariance, "Intercept", f.Names(0)), glmHeaders(0))
   Using run = New WindowsExecutionDispatcher().TryBegin()
    run.Execution.Start()
    Dim writes As Integer
    Assert.ThrowsException(Of InvalidOperationException)(Sub() run.Write(Function()
                                                                         writes += 1 : Throw New InvalidOperationException("synthetic report writer failure")
                                                                        End Function))
    run.Execution.TryFail(New InvalidOperationException("synthetic report writer failure"))
    Assert.AreEqual(1, writes) : Assert.AreEqual(ExecutionState.Failed, run.Execution.Snapshot.State)
   End Using
  Finally
   CultureInfo.CurrentCulture = saved
  End Try
 End Sub
 <DataTestMethod> <DataRow(0.0, False)> <DataRow(1.0, False)> <DataRow(0.0, True)> <DataRow(1.0, True)>
 Public Sub ActiveConstantBinomialSafeguardCannotManufactureFittedRowsOrSuccess(response As Double, cancelFirst As Boolean)
  Dim called = 0, writes = 0
  Using run = New WindowsExecutionDispatcher().TryBegin()
   run.Execution.Start()
   If cancelFirst Then run.Execution.RequestCancellation()
   Try
    Dim model = run.InvokeStage(ExecutionStage.Analysis, Function()
                                                        called += 1
                                                        Return Regression11DefectTests.ConstantModel(response, False, True, True)
                                                       End Function)
    Assert.IsFalse(model.Converged) : Assert.IsNull(model.PredictedResponses) : Assert.IsNull(model.AllResiduals)
    Dim data(23, 1) As Double
    For i = 0 To 23 : data(i, 0) = response : data(i, 1) = i - 11.5 : Next
    Dim raw As New glmData With {.RowIds = Enumerable.Range(0, 24).Select(Function(i) 101 + 2 * i).ToArray()}
    Dim request = WindowsRegressionRequestAdapter.Capture(RegressionAnalysisKind.GeneralizedLinearModel, "y", raw, data, {"y", "x"}, Regression09PolicyTests.Effects(), New RegressionOptions With {.Family = RegressionFamily.Binomial, .Link = RegressionLink.Logit, .Residuals = True})
    Dim report = WindowsRegressionRequestAdapter.Report(LinearBinomialRegressionPolicy.Project(request), model.wrapResults(Nothing, Nothing), model.PredictedResponses, model.AllResiduals, model.Converged)
    Assert.AreEqual(RegressionConvergence.NotConverged, report.Convergence) : Assert.IsNull(report.Rows.Fitted)
    CollectionAssert.AreEqual(raw.RowIds, report.Rows.RowIds) : Assert.AreEqual(1, report.LegacyTables().Count)
    run.Execution.TryFail(New InvalidOperationException("fit unavailable"))
   Catch ex As OperationCanceledException
    Assert.IsTrue(cancelFirst)
   End Try
   Assert.AreEqual(If(cancelFirst, 0, 1), called) : Assert.AreEqual(0, writes)
   Assert.AreEqual(If(cancelFirst, ExecutionState.Cancelled, ExecutionState.Failed), run.Execution.Snapshot.State)
  End Using
 End Sub
 <DataTestMethod> <DataRow("Gaussian|Identity|Independence|Robust|time", False)> <DataRow("Gaussian|Identity|Independence|Robust|time", True)> <DataRow("Binomial|Logit|Independence|Robust|time", False)> <DataRow("Binomial|Logit|Independence|Robust|time", True)>
 Public Sub GeeCurrentRequestAxesAndPostFitSelectedFamilyMismatchRemainSoftwareOnly(key As String, cancelBeforeWrite As Boolean)
  Dim raw = Gee12Fixture.Raw("Binomial|Logit|Independence|Robust|time"), intent = Gee12PolicyTests.Request(key, raw)
  CollectionAssert.AreEqual(raw.RowIds, intent.MeanRequest.Primary.RowIds)
  CollectionAssert.AreEqual(raw.ClusterIdData, intent.Cluster.Values)
  Dim fit = intent.CreateCoreModel()
  fit.Fit(False)
  Dim report = WindowsGeeWorkflowAdapter.Report(intent, fit.wrapResults(), fit.PredictedResponses, fit.AllResiduals, fit.ObservedResponses, fit.ObservationWeights)
  Dim postFit As New RegressionOptions With {.Family = RegressionFamily.Binomial, .Classification = True, .ThresholdTable = True}
  If key.StartsWith("Gaussian", StringComparison.Ordinal) Then
   Assert.AreEqual(0, GeeReportPolicy.Classification(report, postFit).LegacyTables().Count)
   Assert.IsTrue(WindowsGeeWorkflowAdapter.DeferredPostFitFamilyReport(report, postFit, Double.NaN).LegacyTables().Count > 0)
  Else
   Assert.IsTrue(GeeReportPolicy.Classification(report, postFit).LegacyTables().Count > 0)
  End If
  Dim dispatcher As New WindowsExecutionDispatcher
  Using run = dispatcher.TryBegin()
   run.Execution.Start()
   If cancelBeforeWrite Then run.Execution.RequestCancellation()
   Dim written As Integer
   Try
    run.Write(Function()
               written += 1 : Return OutputWriteState.RejectedBeforeWrite
              End Function)
   Catch ex As OperationCanceledException
    Assert.IsTrue(cancelBeforeWrite)
   End Try
   Assert.AreEqual(If(cancelBeforeWrite, 0, 1), written)
  End Using
 End Sub
 <DataTestMethod> <DataRow(1)> <DataRow(2)> <DataRow(3)>
 Public Sub PsmSnapshotQueriesAreDetachedButLeaseIsolationDoesNotClaimWorkbookIsolation(ratio As Integer)
  Dim options As New PsmComprehensiveFitOptions With {.StandardOptions = New PsmOptions With {.MatchingRatio = ratio}}
  Dim selection As New PsmRequestSelection(options)
  options.StandardOptions.MatchingRatio = 99
  Dim one = selection.CreateEffectiveOptions(), two = selection.CreateEffectiveOptions()
  Assert.AreEqual(ratio, one.StandardOptions.MatchingRatio) : Assert.AreEqual(ratio, two.StandardOptions.MatchingRatio)
  one.StandardOptions.MatchingRatio = 88 : Assert.AreEqual(ratio, two.StandardOptions.MatchingRatio)
  Dim lease As New RefEditEventLease(Of String), oldCallback As Action(Of String) = Nothing, fresh As Action(Of String) = Nothing, result = ""
  lease.Rebind(Function(cb)
                oldCallback = cb : Return Sub()
                                         End Sub
               End Function, Sub(v) result = v)
  oldCallback("source row 101") : lease.Invalidate() : oldCallback("stale row 999")
  Assert.AreEqual("source row 101", result)
  lease.Rebind(Function(cb)
                fresh = cb : Return Sub()
                                   End Sub
               End Function, Sub(v) result = v)
  fresh("source row 202") : oldCallback("stale row 999") : Assert.AreEqual("source row 202", result)
  lease.Dispose() : fresh("after close") : Assert.AreEqual("source row 202", result)
 End Sub
 <DataTestMethod> <DataRow("GeneralizedLinearModelsGLM")> <DataRow("GeneralizedEstimatingEquationsGEE")> <DataRow("MixedModelsForRepeatedMeasuresMMRM")> <DataRow("LinearMixedModelsLMM")> <DataRow("PrincipalComponentAnalysis")> <DataRow("FactorAnalysis")> <DataRow("DiscriminantAnalysis")> <DataRow("ControlCharts")> <DataRow("MultivariateControlCharts")> <DataRow("PropensityScoreMatching")> <DataRow("CoxRegression")> <DataRow("LogrankTest")>
 Public Sub HelpIntentResolvesMetadataOnlyAndUnknownDestinationFailsClosed(key As String)
  Dim target = HelpDestination.ForMethodKey(key)
  Assert.IsFalse(target.IsFallback)
  Assert.AreEqual(target.Url, HelpLaunchTarget.SafeOpenUrl(target.Url))
  Assert.AreEqual(GeneratedMethodMetadata.HomeUrl, HelpLaunchTarget.SafeOpenUrl("unapproved destination"))
 End Sub
End Class
