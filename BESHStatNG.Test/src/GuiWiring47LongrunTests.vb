Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Xml.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.CausalInference
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.Test.Harness46
Imports BESHStatNG.Test.Harness47
Imports Microsoft.VisualStudio.TestTools.UnitTesting
<TestClass, DoNotParallelize>
Public Class GuiWiring47LongrunTests
 <DataTestMethod> <DataRow(False)> <DataRow(True)>
 Public Sub PsmLifecycleRecordsCurrentSynchronousProgressAndPumpFaultBoundary(pumpFailure As Boolean)
  Dim f As New Ui20PropensityScoreMatchingProjection47
  GuiWiring47StateTests.Invoke(f, "BeginPsmComputation") : Assert.IsFalse(f.btCalculate.Enabled)
  f.Pump.Fail = pumpFailure
  If pumpFailure Then
   Assert.IsInstanceOfType(Assert.ThrowsException(Of TargetInvocationException)(Sub() GuiWiring47StateTests.Invoke(f, "FinishPsmComputation", "failed")).InnerException, GetType(InvalidOperationException))
  Else
   GuiWiring47StateTests.Invoke(f, "FinishPsmComputation", "failed")
  End If
  Assert.AreEqual("failed", f.lblProgress.Text)
  GuiWiring47StateTests.Invoke(f, "EndPsmComputation") : Assert.IsTrue(f.btCalculate.Enabled)
 End Sub
 <DataTestMethod> <DataRow("Ui21ControlCharts")> <DataRow("Ui22MultivariateControlCharts")>
 Public Sub DisabledSpcRolesActuallyClearStaleAssignmentsAndCanBeEnabledAgain(owner As String)
  Dim f = GuiWiring47StateTests.Create(owner), list As New FakeControl47, label As New FakeControl47, add As New FakeControl47, remove As New FakeControl47
  list.Items.Add("row101")
  GuiWiring47StateTests.Invoke(f, "SetRoleEnabled", list, label, add, remove, False)
  Assert.AreEqual(0, list.Items.Count) : Assert.IsFalse(list.Enabled) : Assert.IsFalse(add.UserClick())
  GuiWiring47StateTests.Invoke(f, "SetRoleEnabled", list, label, add, remove, True)
  Assert.IsTrue(list.Enabled) : Assert.IsTrue(remove.Enabled)
 End Sub
 Public Shared Iterator Function Bindings() As IEnumerable(Of Object())
  For Each e In XDocument.Load(Path.Combine(Environment.GetEnvironmentVariable("BESHSTAT_04A02_SOURCE_ROOT"), "evidence/04A47/fixtures/bindings-b-v2.xml")).Root.Elements()
   Yield New Object() {CStr(e.Attribute("owner")), CStr(e.Attribute("handler")), CStr(e.Attribute("control")), CStr(e.Attribute("event"))}
  Next
 End Function
 <DataTestMethod, DynamicData(NameOf(Bindings), DynamicDataSourceType.Method)>
 Public Sub LongrunSourceBindingsActuallyDeliver(owner As String, handler As String, source As String, eventName As String)
  Dim tests As New GuiWiring47StateTests()
  tests.SourceHandlesAndDynamicSubscriptionsActuallyDeliver(owner, handler, source, eventName)
 End Sub
 <DataTestMethod> <DataRow("UiGLM", False)> <DataRow("UiGLM", True)> <DataRow("Ui13GEE", False)> <DataRow("Ui13GEE", True)> <DataRow("Ui4Cox", False)> <DataRow("Ui4Cox", True)>
 Public Sub ShippingLifecycleEventsRetainDisableInterruptAndCallbackOwnership(owner As String, pumpFailure As Boolean)
  Dim f = GuiWiring47StateTests.Create(owner), other = GuiWiring47StateTests.Create(owner)
  Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic
  Dim pump = DirectCast(f.GetType().GetField("Pump", flags).GetValue(f), FakePump47), bridge = DirectCast(f.GetType().GetField("Bridge", flags).GetValue(f), FakeRegressionBridge47)
  pump.Fail = pumpFailure
  GuiWiring47StateTests.Invoke(f, "BeginRegressionComputation") : GuiWiring47StateTests.Invoke(other, "BeginRegressionComputation")
  Assert.IsFalse(GuiWiring47StateTests.Control(f, "btCalculate").Enabled)
  Assert.IsFalse(bridge.Cancel()) : Assert.IsFalse(bridge.Interrupt())
  GuiWiring47StateTests.Control(f, "btInterrupt").FireClick()
  Assert.IsTrue(bridge.Interrupt()) : Assert.IsFalse(GuiWiring47StateTests.Control(f, "btInterrupt").Enabled)
  Assert.IsFalse(GuiWiring47StateTests.Control(f, "btInterrupt").UserClick())
  Assert.IsFalse(CBool(GuiWiring47StateTests.Invoke(other, "IsRegressionInterruptionRequested")))
  f.GetType().GetField("pRegressionCancelRequested", flags).SetValue(f, True)
  Assert.IsTrue(bridge.Cancel()) : Assert.IsFalse(bridge.Interrupt())
  GuiWiring47StateTests.Invoke(f, "FinishRegressionComputation", "synthetic failure")
  Assert.AreEqual("synthetic failure", GuiWiring47StateTests.Control(f, "lblProgress").Text)
  GuiWiring47StateTests.Invoke(f, "EndRegressionComputation") : GuiWiring47StateTests.Invoke(other, "EndRegressionComputation")
  Assert.IsNull(bridge.Cancel) : Assert.AreEqual(1, bridge.Sets) : Assert.AreEqual(1, bridge.Clears)
  Assert.IsTrue(GuiWiring47StateTests.Control(f, "btCalculate").Enabled)
 End Sub
 <DataTestMethod> <DataRow("Ui18MMRM", "pMmrmCalculationRunning", "pMmrmInterruptRequested")> <DataRow("Ui19LMM", "pLmmCalculationRunning", "plmmInterruptRequested")>
 Public Sub MixedInterruptOnlyChangesFlagsAtCurrentSupportedBoundary(owner As String, running As String, interrupted As String)
  Dim f = GuiWiring47StateTests.Create(owner), flags = BindingFlags.Instance Or BindingFlags.NonPublic
  GuiWiring47StateTests.Control(f, "btInterrupt").FireClick()
  Assert.IsFalse(CBool(f.GetType().GetField(interrupted, flags).GetValue(f)))
  f.GetType().GetField(running, flags).SetValue(f, True)
  GuiWiring47StateTests.Control(f, "btInterrupt").FireClick()
  Assert.IsTrue(CBool(f.GetType().GetField(interrupted, flags).GetValue(f)))
  Assert.IsFalse(GuiWiring47StateTests.Control(f, "btInterrupt").Enabled)
 End Sub
 <DataTestMethod> <DataRow(False)> <DataRow(True)>
 Public Sub SpcPhasePcaAndDiagnosticsTogglesReverse(withPhaseII As Boolean)
  Dim f As New Ui21ControlChartsProjection47
  f.rbPhaseIThenPhaseII.Checked = withPhaseII : f.rbSinglePhaseI.RaiseNamed("CheckedChanged")
  Assert.AreEqual(withPhaseII, f.spinLastPhaseIPoint.Enabled)
  f.rbPhaseIThenPhaseII.Checked = Not withPhaseII : Assert.AreEqual(Not withPhaseII, f.lblLastPhaseIPoint.Enabled)
  Dim m As New Ui22MultivariateControlChartsProjection47
  m.rbPcaFixedComponents.Checked = True : Assert.IsTrue(m.spinPcaComponentCount.Enabled)
  m.rbPcaVarianceSelection.Checked = True : Assert.IsTrue(m.spinPcaCumulativeVariance.Enabled)
  m.rbPcaFixedComponents.Checked = False : Assert.IsFalse(m.spinPcaComponentCount.Enabled)
  For Each state In {True, False, True}
   m.chkWriteDiagnostics.Checked = state : Assert.AreEqual(state, m.cbDiagnosticsScope.Enabled)
  Next
 End Sub
 <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
 Public Sub SpcStackedLogicalPointsKeepPhysicalRowsAndRejectInvalidPhaseEdges(caseId As Integer)
  Dim map = SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, {101, 105, 118, 131}, {"A", "B", "A", "B"})
  Assert.AreEqual(2, map.PointCount) : CollectionAssert.AreEqual({0, 2}, map.Points(0).InputRowIndices.ToArray())
  Dim valid = SpcPhaseStagePolicy.Quick(map.PointCount, False, 1, SpcParameterSource.EstimateFromPhaseI).ToArray()
  Assert.AreEqual(2, valid.Length) : Assert.AreEqual(2, valid(1).FirstPoint)
  Dim invalid = If(caseId = 0, 0, If(caseId = 1, 2, 4))
  Assert.ThrowsException(Of ArgumentException)(Sub() SpcPhaseStagePolicy.Quick(map.PointCount, False, invalid, SpcParameterSource.EstimateFromPhaseI).ToArray())
  Assert.ThrowsException(Of ArgumentException)(Sub() SpcPhaseStagePolicy.Collapse(map, {"first", "second", "wrong", "second"}, Nothing))
  Assert.ThrowsException(Of ArgumentException)(Sub() SpcPhaseStagePolicy.Collapse(map, {"", "second", "", "second"}, Nothing))
 End Sub
 <DataTestMethod> <DataRow("StandardNearestNeighbor", "LogisticRegression")> <DataRow("StandardNearestNeighbor", "Supplied")> <DataRow("CoarsenedExactMatching", "Supplied")>
 Public Sub PsmSourceDefaultsOptionsAndBackToBackRequestsStayIndependent(method As String, score As String)
  Dim f As New Ui20PropensityScoreMatchingProjection47
  GuiWiring47StateTests.Invoke(f, "InitializeDefaultUiValues")
  f.cbScoreMethod.SelectedItem = score : f.cbScoreMethod.Text = score : f.cbRunMethod.SelectedItem = method : f.cbRunMethod.Text = method
  f.cbRunMethod.RaiseNamed("SelectedIndexChanged")
  Assert.AreEqual(score = "Supplied", f.lbScore.Enabled) : Assert.AreEqual(score <> "Supplied", f.tbTolerance.Enabled)
  Dim first = DirectCast(GuiWiring47StateTests.Invoke(f, "BuildFitOptions"), PsmComprehensiveFitOptions)
  Assert.AreEqual(CType([Enum].Parse(GetType(PsmBackendRunMethod), method), PsmBackendRunMethod), first.RunMethod)
  f.tbMatchingRatio.Text = "3" : f.tbLoveThreshold.Text = "0.2"
  Dim second = DirectCast(GuiWiring47StateTests.Invoke(f, "BuildFitOptions"), PsmComprehensiveFitOptions)
  Assert.AreEqual(0.1, first.LovePlotThreshold) : Assert.AreEqual(0.2, second.LovePlotThreshold)
  Assert.AreNotSame(first.StandardOptions, second.StandardOptions)
  f.tbTolerance.Text = "invalid"
  Assert.IsInstanceOfType(Assert.ThrowsException(Of TargetInvocationException)(Sub() GuiWiring47StateTests.Invoke(f, "BuildFitOptions")).InnerException, GetType(FormatException))
 End Sub
 <DataTestMethod> <DataRow(False)> <DataRow(True)>
 Public Sub KmCensorTimeGroupStrataRolesAndOutputCancelRemainShipping(requiredGroup As Boolean)
  Dim f As New Ui4KMandLogRankProjection47 With {.Tag = If(requiredGroup, HelpTopic.LogrankTest, HelpTopic.KaplanMeierPlot)}
  f.RefEdit1_SurvivalTime.Address = "time" : f.RefEdit2_Censor.Address = "censor" : f.RefEdit3_GroupID.Address = If(requiredGroup, "group", "") : f.RefEdit4_StrataID.Address = If(requiredGroup, "stratum", "")
  FakeRanges46.Reset() : Assert.IsFalse(CBool(GuiWiring47StateTests.Invoke(f, "checkInputs")))
  CollectionAssert.AreEqual(If(requiredGroup, {"time", "censor", "group", "stratum"}, {"time", "censor"}), FakeRanges46.Calls.Select(Function(c) c.Item1).ToArray())
  FakeRanges46.Reject.Add("censor") : Assert.IsTrue(CBool(GuiWiring47StateTests.Invoke(f, "checkInputs")))
  Assert.AreEqual("time", f.RefEdit1_SurvivalTime.Address) : Assert.AreEqual("", f.RefEdit2_Censor.Address)
  f.optWorksheet.FireClick() : Assert.IsFalse(f.RefEditOutput.Enabled)
  f.optOutputRange.FireClick() : Assert.IsTrue(f.RefEditOutput.Enabled) : Assert.IsTrue(f.RefEditOutput.txtAddress.Selections > 0)
 End Sub
 <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)>
 Public Sub ActualExecutionCoordinatorKeepsFaultCancelAndCompletedWritePriority(value As Integer)
  Dim state = CType(value, OutputWriteState), dispatcher As New WindowsExecutionDispatcher
  Using run = dispatcher.TryBegin()
   run.Execution.Start() : Assert.IsNull(dispatcher.TryBegin())
   AddHandler run.Execution.Changed, Sub(snapshot) Throw New InvalidOperationException("observer failure")
   Dim called As Integer
   Try
    run.Write(Function()
               called += 1 : run.Execution.RequestCancellation() : Return state
              End Function)
   Catch ex As OperationCanceledException
    Assert.AreEqual(OutputWriteState.CancelledByUser, state)
   End Try
   Assert.AreEqual(1, called)
   Dim expected = If(state = OutputWriteState.Completed, ExecutionState.Completed, If(state = OutputWriteState.CancelledByUser, ExecutionState.Cancelled, ExecutionState.Failed))
   Assert.AreEqual(expected, run.Execution.Snapshot.State)
   Assert.IsFalse(run.Execution.TryFail(New InvalidOperationException("late fault")))
   run.Execution.Report(1) : Assert.AreEqual(expected, run.Execution.Snapshot.State)
  End Using
  Using nextRun = dispatcher.TryBegin()
   Assert.IsNotNull(nextRun)
  End Using
 End Sub
End Class
