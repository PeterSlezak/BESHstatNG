Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test.Harness46
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class GuiWiring46ExecutionTests
 Private Shared ReadOnly Titles As String() = {"Paired T-test", "Unpaired T-test", "Unpaired T-test", "Unpaired T-test", "Single Proportion", "Independent Proportions", "Independent Proportions", "Independent Proportions", "Log-rank Test", "Cox Regression", "Cox Regression", "Intraclass Correlation (ICC)", "Agreement (Bland-Altman)"}
 Private Shared ReadOnly Choices As String() = {"", "Superiority", "Noninferiority", "Equivalence", "", "Superiority", "Noninferiority", "Equivalence", "", "Binary Covariate", "Continuous Covariate", "", ""}
 Private Shared ReadOnly Methods As SampleSizePlanningMethod() = {SampleSizePlanningMethod.PairedTTest, SampleSizePlanningMethod.UnpairedTTest, SampleSizePlanningMethod.NonInferiorityUnpairedTTest, SampleSizePlanningMethod.EquivalenceUnpairedTTest, SampleSizePlanningMethod.SingleProportion, SampleSizePlanningMethod.IndependentProportions, SampleSizePlanningMethod.NonInferiorityIndependentProportions, SampleSizePlanningMethod.EquivalenceIndependentProportions, SampleSizePlanningMethod.LogRankSampleSize, SampleSizePlanningMethod.CoxEventCountBinaryCovariate, SampleSizePlanningMethod.CoxEventCountContinuousCovariate, SampleSizePlanningMethod.IccHypothesisTestSampleSize, SampleSizePlanningMethod.BlandAltmanLoASampleSize}
 Public Shared Iterator Function PlanningVectors() As IEnumerable(Of Object())
  For i = 0 To Titles.Length - 1
   For Each culture In New String() {"", "en-US", "sk-SK"}
    For vectorIndex = 0 To 1
     Yield New Object() {i, culture, vectorIndex}
    Next
   Next
  Next
 End Function
 Private Shared Function Values(index As Integer, vectorIndex As Integer) As Double()
  Select Case index
   Case 0 : Return {5 + vectorIndex, 10, 0, 0}
   Case 1 : Return {5 + vectorIndex, 10, 1, 0}
   Case 2, 3 : Return {0 + vectorIndex, 5, 10, 1}
   Case 4 : Return {0.3 + vectorIndex * 0.1, 0.5, 0, 0}
   Case 5 : Return {0.3 + vectorIndex * 0.1, 0.6, 1, 0}
   Case 6, 7 : Return {0.3 + vectorIndex * 0.1, 0.6, 0.1, 1}
   Case 8 : Return {If(vectorIndex = 0, 0.7, 0.8), 0.4, 0.6, 1}
   Case 9, 10 : Return {If(vectorIndex = 0, 0.7, 0.8), 1, 0.4, 0.1}
   Case 11 : Return {0.1, If(vectorIndex = 0, 0.7, 0.8), 2, 0}
   Case Else : Return {10 + vectorIndex, 2, 1.96, 0}
  End Select
 End Function
 Private Shared Function PlanningForm(index As Integer, vectorIndex As Integer) As Ui12SampleSizeTtestSinglePropProjection46
  Dim f As New Ui12SampleSizeTtestSinglePropProjection46 With {.Text = "Sample Size - " & Titles(index)}
  f.cbHypothesisType.SelectedItem = Choices(index)
  ' Explicit supplied state,not a claim that the native form constructor ran.
  f.tbKappa.Visible = index <> 0 AndAlso index <> 4
  f.tbCustom4.Visible = index = 2 OrElse index = 3 OrElse index = 6 OrElse index = 7 OrElse index = 8 OrElse index = 9 OrElse index = 10
  f.tbKappa.Text = "1" ' Existing numeric ratio before selecting NI/equivalence.
  f.cbHypothesisType.SelectedIndex = 0
  Dim fields As FakeControl46() = {f.tbMeanDiff, f.tbSD, f.tbKappa, f.tbCustom4}, v = Values(index, vectorIndex)
  For i = 0 To 3
   fields(i).Text = v(i).ToString("G", SourceNumbers46.HostCulture)
  Next
  f.spinBtnAlpha.Value = 0.05D : f.spinBtnBeta.Value = 0.2D
  Return f
 End Function
 <DataTestMethod, DynamicData(NameOf(PlanningVectors), DynamicDataSourceType.Method)>
 Public Sub PlanningClickBuildsExactAcceptedRequestWithoutEngineOrWriter(index As Integer, culture As String, vectorIndex As Integer)
  Dim saved = CultureInfo.CurrentCulture
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture) : SourceNumbers46.HostCulture = CultureInfo.CurrentCulture
   FakeOutput46.Reset() : Dim f = PlanningForm(index, vectorIndex)
   f.btCompute.FireClick()
   Assert.AreEqual(0, FakeFaults46.Errors.Count) : Assert.AreEqual(1, FakePlanning46.Requests.Count)
   Dim request = FakePlanning46.Requests.Single(), values = GuiWiring46ExecutionTests.Values(index, vectorIndex)
   Assert.AreEqual(Methods(index), request.Method) : Assert.AreEqual(0.05, request.Alpha)
   If index = 12 Then Assert.IsTrue(Double.IsNaN(request.Beta)) Else Assert.AreEqual(0.2, request.Beta)
   Dim expected As Double()
   If index = 9 OrElse index = 10 Then
    expected = {values(0), values(1), values(3), values(2)}
   Else
    Dim count = If(index = 0 OrElse index = 4, 2, If(index = 1 OrElse index = 5 OrElse index = 11 OrElse index = 12, 3, 4))
    expected = values.Take(count).ToArray()
   End If
   CollectionAssert.AreEqual(expected, request.Inputs())
   If index = 11 Then Assert.AreEqual(2, request.ObservationsPerSubject)
   Assert.AreEqual("synthetic planning response", f.tbOutput.Text)
   Assert.AreEqual(0, FakeOutput46.Writes)
  Finally
   CultureInfo.CurrentCulture = saved : SourceNumbers46.HostCulture = CultureInfo.InvariantCulture
  End Try
 End Sub
 <DataTestMethod, DynamicData(NameOf(PlanningVectors), DynamicDataSourceType.Method)>
 Public Sub EveryPlanningBranchRejectsBadNumericOrDomainBeforeRequest(index As Integer, culture As String, vectorIndex As Integer)
  Dim saved = CultureInfo.CurrentCulture
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture) : SourceNumbers46.HostCulture = CultureInfo.CurrentCulture
   FakeOutput46.Reset() : Dim f = PlanningForm(index, 0)
   If vectorIndex = 0 Then
    f.tbMeanDiff.Text = "invalid-number"
   Else
    Select Case index
     Case 2, 3 : f.tbSD.Text = "0"
     Case 8, 9, 10 : f.tbMeanDiff.Text = "1"
     Case 11 : f.tbKappa.Text = "1.5"
     Case Else : f.tbMeanDiff.Text = "0"
    End Select
   End If
   f.btCompute.FireClick()
   Assert.AreEqual(0, FakePlanning46.Requests.Count) : Assert.AreEqual(0, FakeOutput46.Writes)
   Assert.IsTrue(FakeMessages46.Messages.Count > 0)
   If vectorIndex = 0 Then Assert.AreEqual(System.Drawing.Color.Red, f.tbMeanDiff.BackColor)
  Finally
   CultureInfo.CurrentCulture = saved : SourceNumbers46.HostCulture = CultureInfo.InvariantCulture
  End Try
 End Sub
 <TestMethod>
 Public Sub SampleLayoutReversalPreservesExistingTextAndExposesLegacyParseException()
  Dim f = PlanningForm(1, 0)
  f.cbHypothesisType.SelectedItem = "Noninferiority" : f.cbHypothesisType.SelectedIndex = 1
  Assert.AreEqual("Noninferiority Margin", f.lblSD.Text) : Assert.AreEqual("10", f.tbSD.Text)
  f.cbHypothesisType.SelectedItem = "Superiority" : f.cbHypothesisType.SelectedIndex = 0
  Assert.IsFalse(f.tbCustom4.Visible) : Assert.AreEqual("5", f.tbMeanDiff.Text)
  Dim p = PlanningForm(5, 0) : p.tbKappa.Text = ""
  p.cbHypothesisType.SelectedItem = "Noninferiority"
  Assert.ThrowsException(Of FormatException)(Sub() p.cbHypothesisType.SelectedIndex = 1)
  ' Current source parses the blank ratio before SetDefaultIfBlank;do not repair it.
 End Sub
 <DataTestMethod>
 <DataRow("1,25", "", 125.0)> <DataRow("1,25", "sk-SK", 1.25)> <DataRow("", "", 0.0)>
 Public Sub SourceNumericHelperKeepsCultureAndBlankDistinction(text As String, culture As String, expected As Double)
  Dim old = CultureInfo.CurrentCulture
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture) : SourceNumbers46.HostCulture = CultureInfo.CurrentCulture
   Dim control As New FakeControl46 With {.Text = text}, value As Double, message = ""
   Assert.IsTrue(SourceNumbers46.CheckNumeric(control, value, message)) : Assert.AreEqual(expected, value)
   If text = "" Then Assert.ThrowsException(Of FormatException)(Sub() SourceNumbers46.ParseUiDouble(text))
  Finally
   CultureInfo.CurrentCulture = old : SourceNumbers46.HostCulture = CultureInfo.InvariantCulture
  End Try
 End Sub
 Private Shared Function TableForm() As Ui82x2Projection46
  Dim f As New Ui82x2Projection46
  f.optScreenData.Checked = True : f.spinBtnAlpha.Value = 0.05D
  f.spinBtnA.Value = 12 : f.spinBtnB.Value = 5 : f.spinBtnC.Value = 7 : f.spinBtnD.Value = 19
  f.ckChi2.Checked = True : f.ckOR.Checked = True : f.RefEditOutput.Address = "synthetic-output"
  Return f
 End Function
 <DataTestMethod>
 <DataRow(0)> <DataRow(1)> <DataRow(2)>
 Public Sub CategoricalComputeUsesOriginalDestinationPriorityAndRealTableDimensions(destination As Integer)
  FakeOutput46.Reset() : Dim f = TableForm()
  f.optOutputRange.Checked = destination = 0 : f.optWorksheet.Checked = destination = 1 : f.optWorkbook.Checked = destination = 2
  f.btCompute.FireClick()
  Assert.AreEqual(0, FakeFaults46.Errors.Count) : Assert.AreEqual(1, FakeOutput46.Writes) : Assert.IsTrue(FakeOutput46.TableCount > 0)
  If destination = 0 Then
   Assert.IsTrue(FakeOutput46.Events.Contains("range:synthetic-output")) : Assert.AreEqual(7, FakeOutput46.LastRow) : Assert.AreEqual(3, FakeOutput46.LastColumn)
  Else
   Assert.IsTrue(FakeOutput46.Events.Contains(If(destination = 1, "worksheet-add", "workbook-add")))
  End If
 End Sub
 <DataTestMethod>
 <DataRow("negative")> <DataRow("fraction")> <DataRow("no-source")> <DataRow("overwrite-no")> <DataRow("disabled")>
 Public Sub CategoricalRejectionAndNoOverwriteNeverWrite(kind As String)
  FakeOutput46.Reset() : Dim f = TableForm()
  Select Case kind
   Case "negative" : f.spinBtnA.Value = -1
   Case "fraction" : f.spinBtnA.Value = 1.5D
   Case "no-source" : f.optScreenData.Checked = False
   Case "overwrite-no" : FakeOutput46.Occupied = True : FakeMessages46.Response = 7
   Case "disabled" : f.btCompute.Enabled = False
  End Select
  f.btCompute.UserClick()
  Assert.AreEqual(0, FakeOutput46.Writes)
  If kind <> "disabled" Then Assert.IsTrue(FakeMessages46.Messages.Count > 0)
 End Sub
 <DataTestMethod>
 <DataRow("valid")> <DataRow("missing")> <DataRow("nonnumeric")> <DataRow("bad-shape")>
 Public Sub WorksheetDescriptorKeepsAcceptedRawRowIdentityAndRejectsWithoutWriter(kind As String)
  FakeOutput46.Reset() : Dim f = TableForm(), raw As Object(,) = {{12, 5}, {7, 19}}
  If kind = "missing" Then raw(0, 0) = Nothing
  If kind = "nonnumeric" Then raw(0, 0) = "invalid-cell"
  If kind = "bad-shape" Then raw = New Object(,) {{1, 2, 3}, {4, 5, 6}}
  Dim imported As New DataObj : imported.DataImportRawMatrix(raw, If(kind = "bad-shape", New String() {"A", "B", "C"}, New String() {"A", "B"}), firstSourceRow:=20)
  FakeOutput46.Imported = imported : f.optScreenData.Checked = False : f.optWorksheetData.Checked = True
  f.btCompute.FireClick() : Assert.AreEqual(1, FakeOutput46.ImportCalls)
  Assert.AreEqual(If(kind = "valid", 1, 0), FakeOutput46.Writes)
  If kind = "valid" Then
   Dim parsed = WindowsContingencyInputAdapter.ParseWorksheet(imported, New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False}, True)
   CollectionAssert.AreEqual(New Integer() {20, 20, 21, 21}, parsed.Data.Sources.Select(Function(s) s.WorksheetRow).ToArray())
   CollectionAssert.AreEqual(New Integer() {12, 5, 7, 19}, parsed.Data.CoreCounts.Cast(Of Integer)().ToArray())
  End If
 End Sub
 <DataTestMethod>
 <DataRow(0)> <DataRow(1)> <DataRow(2)>
 Public Sub ProportionEventDispatchRetainsPairedAndIndependentCountAxes(mode As Integer)
  FakeOutput46.Reset() : Dim f As New Ui8ProportionsProjection46
  f.spinBtnAlpha.Value = 0.05D : f.spinBtnA.Value = 100 : f.spinBtnB.Value = 20 : f.spinBtnC.Value = If(mode = 1, 80, 15) : f.spinBtnD.Value = If(mode = 1, 30, 40)
  f.optSingle.Checked = mode = 0 : f.optIndependent.Checked = mode = 1 : f.optPaired.Checked = mode = 2
  f.btCompute.FireClick() : Assert.AreEqual(0, FakeFaults46.Errors.Count) : Assert.AreEqual(1, FakeOutput46.Writes)
  f.spinBtnB.Value = 1000 : f.btCompute.FireClick() : Assert.AreEqual(1, FakeOutput46.Writes, "Rejected second action must not add a write")
 End Sub
 <TestMethod>
 Public Sub PlotRequestsKeepActualPrecedenceAndRejectAbsentChoice()
  Dim polar As New Ui01PolarPlotProjection46
  Assert.ThrowsException(Of TargetInvocationException)(Sub() GuiWiring46StateTests.CallMember(polar, "GetPlotOptions"))
  polar.optRadians.Checked = True : polar.optClockwise.Checked = True : polar.optNorth.Checked = True : polar.ckConnectPoints.Checked = True
  Dim a = CType(GuiWiring46StateTests.CallMember(polar, "GetPlotOptions"), PolarPlotOptions)
  Assert.AreEqual(PolarAngleUnit.Radians, a.AngleUnit) : Assert.IsTrue(a.ConnectPoints)
  polar.optRadians.Checked = False : polar.optDegrees.Checked = True : polar.optClockwise.Checked = False : polar.optCounterClockwise.Checked = True
  Dim b = CType(GuiWiring46StateTests.CallMember(polar, "GetPlotOptions"), PolarPlotOptions)
  Assert.AreEqual(PolarAngleUnit.Degrees, b.AngleUnit) : Assert.AreEqual(PolarRotation.Counterclockwise, b.Rotation)
  Dim hull As New Ui01ConvexHullPlotProjection46
  hull.tbCollinearityTolerance.Text = "0.0001" : hull.nudPaddingPercentX.Value = 5 : hull.nudPaddingPercentY.Value = 7 : hull.ckIncludeCollinearBoundaryPoints.Checked = True
  Dim h = CType(GuiWiring46StateTests.CallMember(hull, "GetPlotOptions"), ConvexHullPlotOptions)
  Assert.AreEqual(5.0, h.PaddingPercentX) : Assert.AreEqual(7.0, h.PaddingPercentY) : Assert.IsTrue(h.IncludeCollinearBoundaryPoints)
  hull.tbCollinearityTolerance.Text = "invalid" : Assert.ThrowsException(Of TargetInvocationException)(Sub() GuiWiring46StateTests.CallMember(hull, "GetPlotOptions"))
  Dim ladder As New Ui01LadderPlotProjection46
  ladder.nudHorizontalJitter.Value = 0.15D : ladder.ckHorizontalJitter.Checked = False
  Assert.AreEqual(0.0, CType(GuiWiring46StateTests.CallMember(ladder, "GetPlotOptions"), LadderPlotOptions).HorizontalJitterFraction)
  ladder.ckHorizontalJitter.Checked = True
  Assert.AreEqual(0.15, CType(GuiWiring46StateTests.CallMember(ladder, "GetPlotOptions"), LadderPlotOptions).HorizontalJitterFraction)
 End Sub
 <TestMethod>
 Public Sub GroupedLeaseInvalidationBlocksOldSelectionButAllowsNewGeneration()
  Dim lease As New RefEditEventLease(Of String), first As Action(Of String) = Nothing, second As Action(Of String) = Nothing, selection = "", detached = 0
  lease.Rebind(Function(cb)
                   first = cb : Return Sub() detached += 1
               End Function, Sub(value) selection = value)
  first("group-physical-row20") : lease.Invalidate() : first("late-row999") : Assert.AreEqual("group-physical-row20", selection)
  lease.Rebind(Function(cb)
                   second = cb : Return Sub() detached += 1
               End Function, Sub(value) selection = value)
  first("stale-nested-role") : second("nested-physical-row23") : Assert.AreEqual("nested-physical-row23", selection)
  lease.Dispose() : second("after-cancel") : Assert.AreEqual("nested-physical-row23", selection) : Assert.AreEqual(2, detached)
 End Sub
End Class
