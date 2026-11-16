Option Strict On
Option Explicit On
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Test.Harness46
Imports Microsoft.VisualStudio.TestTools.UnitTesting
<TestClass, DoNotParallelize>
Public Class GuiWiring46PolicyTests
 <DataTestMethod>
 <DataRow("", True)> <DataRow("invalid", True)> <DataRow("0", True)> <DataRow("-1", True)> <DataRow("0.1", False)> <DataRow("1,25", True)>
 Public Sub UnpairedMarginRejectionUsesItsActualInvariantParser(text As String, rejected As Boolean)
  Dim f As New UibyIDProjection46 With {.Tag = HelpTopic.UnpairedTTest}
  f.optByID.Checked = True : f.optHypothesisNonInferiority_UTT.Checked = True : f.tbMargin_UTT.Text = text
  FakeMessages46.Messages.Clear()
  Assert.IsTrue(CBool(GuiWiring46StateTests.CallMember(f, "ValidateUnpairedTtestOptionInputs")))
  Assert.IsTrue(FakeMessages46.Messages(0).Contains("By Column"), "Input orientation precedes margin parsing")
  f.optByID.Checked = False : f.optByColumn.Checked = True
  Assert.AreEqual(rejected, CBool(GuiWiring46StateTests.CallMember(f, "ValidateUnpairedTtestOptionInputs")))
 End Sub
 <DataTestMethod>
 <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)>
 <DataRow(8)> <DataRow(9)> <DataRow(10)> <DataRow(11)> <DataRow(12)> <DataRow(13)> <DataRow(14)> <DataRow(15)>
 Public Sub AllPresentHomogeneityCheckboxCombinationsKeepSourceFlags(bits As Integer)
  Dim f As New UibyIDProjection46
  f.ckFlignerKilleen.Checked = (bits And 1) <> 0 : f.ckLevene.Checked = (bits And 2) <> 0
  f.ckSquaredRanks.Checked = (bits And 4) <> 0 : f.ckBartlett.Checked = (bits And 8) <> 0
  Dim expected = HomogeneityMethods.None
  If (bits And 1) <> 0 Then expected = expected Or HomogeneityMethods.FlignerKilleen
  If (bits And 2) <> 0 Then expected = expected Or HomogeneityMethods.LeveneMedian
  If (bits And 4) <> 0 Then expected = expected Or HomogeneityMethods.SquaredRanks
  If (bits And 8) <> 0 Then expected = expected Or HomogeneityMethods.Bartlett
  Assert.AreEqual(expected, CType(GuiWiring46StateTests.CallMember(f, "GetHomogeneityMethods"), HomogeneityMethods))
 End Sub
 <DataTestMethod>
 <DataRow(2)> <DataRow(3)> <DataRow(8)> <DataRow(9)>
 Public Sub NormalityAvailabilityUsesAcceptedAdapterWithoutNativeOutput(n As Integer)
  Dim values = Enumerable.Range(1, n).Select(Function(i) Math.Sin(i * 1.7) + i * 0.11 + (i Mod 5) * 0.2).Reverse().ToArray()
  Dim input As New MultiGroupsUnpairedData With {.X = {values}, .varNames = {"source-label"}}
  Dim report = WindowsAssumptionReportAdapter.Prepare(input, New AssumptionReportOptions(AssumptionReportKind.Normality))
  Assert.AreEqual(1, report.Tables.Count)
  Assert.AreEqual(n < 4, report.Computation.Validation.Issues.Any(Function(i) i.Code = "NORMALITY_SW_NOT_APPLICABLE"))
  Assert.AreEqual(n < 9, report.Computation.Validation.Issues.Any(Function(i) i.Code = "NORMALITY_DA_NOT_APPLICABLE"))
  ' Availability/shape evidence only;no numerical reference value or scientific golden.
 End Sub
 <DataTestMethod>
 <DataRow(0)> <DataRow(1)> <DataRow(2)>
 Public Sub PairedDemingBootstrapAndModelControlsReverse(model As Integer)
  Dim f As New UiTwoInputRefeditsProjection46 With {.Tag = HelpTopic.DemingRegression}
  f.ckDemingFitIntercept.Checked = True : f.cmbDemingVarianceModel.SelectedIndex = model
  Assert.AreEqual(model = 0, f.spinBtnErrorRatio.Enabled)
  Assert.AreEqual(model = 1, f.spinBtnDemingCVx.Enabled) : Assert.AreEqual(model = 2, f.RefEditDemingSDx.Enabled)
  f.optDemingBootstrapBCa.Checked = True : Assert.IsTrue(f.spinBtnBootstrapReplicatesDeming.Enabled)
  f.optDemingBootstrapBCa.Checked = False : Assert.IsFalse(f.spinBtnBootstrapReplicatesDeming.Enabled)
  f.optLinCCCBootstrapPercentile.Checked = True : Assert.IsTrue(f.spinBtnBootstrapReplicatesLinCCC.Enabled)
  f.optLinCCCBootstrapPercentile.Checked = False : Assert.IsFalse(f.spinBtnBootstrapReplicatesLinCCC.Enabled)
  f.optKappaBootstrapBCa.Checked = True : Assert.IsTrue(f.spinBtnBootstrapReplicatesKappa.Enabled)
  f.optKappaBootstrapBCa.Checked = False : Assert.IsFalse(f.spinBtnBootstrapReplicatesKappa.Enabled)
 End Sub
 Private Shared Sub SelectCombo(Of T)(form As Object, control As FakeControl46, value As T)
  Dim item = form.GetType().GetNestedType("ComboItem`1", BindingFlags.NonPublic).MakeGenericType(GetType(T))
  control.SelectedItem = Activator.CreateInstance(item, New Object() {"synthetic choice", value})
 End Sub
 <DataTestMethod>
 <DataRow(False)> <DataRow(True)>
 Public Sub BulletDumbbellAndSankeyBuildExactIndependentOptions(alternate As Boolean)
  Dim bullet As New Ui01BulletChartProjection46
  Assert.ThrowsException(Of TargetInvocationException)(Sub() GuiWiring46StateTests.CallMember(bullet, "GetPlotOptions"))
  SelectCombo(bullet, bullet.cmbDefaultDirection, If(alternate, BulletChartDirection.LowerIsBetter, BulletChartDirection.HigherIsBetter))
  bullet.ckOmitIncompleteRows.Checked = alternate : bullet.nudTargetMajorTickCount.Value = If(alternate, 8, 5)
  Dim b = CType(GuiWiring46StateTests.CallMember(bullet, "GetPlotOptions"), BulletChartOptions)
  Assert.AreEqual(alternate, b.OmitIncompleteRows) : Assert.AreEqual(If(alternate, 8, 5), b.TargetMajorTickCount)
  Assert.AreEqual(If(alternate, BulletChartDirection.LowerIsBetter, BulletChartDirection.HigherIsBetter), b.DefaultDirection)
  Dim dumbbell As New Ui01DumbbellPlotProjection46
  SelectCombo(dumbbell, dumbbell.cmbSortMode, If(alternate, DumbbellPlotSortMode.DifferenceDescending, DumbbellPlotSortMode.InputOrder))
  dumbbell.ckReverseCategoryOrder.Checked = alternate : dumbbell.ckOmitIncompleteRows.Checked = alternate : dumbbell.nudAddObsJitter.Value = 0.2D
  Dim d = CType(GuiWiring46StateTests.CallMember(dumbbell, "GetPlotOptions"), DumbbellPlotOptions)
  Assert.AreEqual(alternate, d.ReverseCategoryOrder) : Assert.AreEqual(alternate, d.OmitIncompleteRows) : Assert.AreEqual(0.2, d.AuxiliaryJitterFraction)
  Dim sankey As New Ui01SankeyPlotProjection46
  sankey.nudNodeGap.Value = 15 : sankey.cmbNodeOrder.SelectedIndex = If(alternate, 2, 1)
  sankey.cmbVerticalAlignment.SelectedIndex = If(alternate, 1, 2) : sankey.ckConnectAcrossMissingStages.Checked = alternate
  Dim s = CType(GuiWiring46StateTests.CallMember(sankey, "GetPlotOptions"), SankeyPlotOptions)
  Assert.AreEqual(0.15, s.NodeGapFraction) : Assert.AreEqual(alternate, s.ConnectAcrossMissingStages)
  Assert.AreEqual(If(alternate, SankeyNodeOrderMode.Alphabetical, SankeyNodeOrderMode.FirstAppearance), s.NodeOrder)
 End Sub
End Class
