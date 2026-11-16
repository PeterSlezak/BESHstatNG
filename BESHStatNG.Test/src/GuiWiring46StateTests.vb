Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Xml.Linq
Imports BESHStatNG.Test.Harness46
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class GuiWiring46StateTests
 Friend Shared Function Create(owner As String) As Object
  Return Activator.CreateInstance(GetType(UibyIDProjection46).Assembly.GetType("BESHStatNG.Test.Harness46." & owner & "Projection46", True), True)
 End Function
 Friend Shared Function Control(form As Object, name As String) As FakeControl46
  Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public
  Dim propertyInfo = form.GetType().GetProperty(name, flags)
  If propertyInfo IsNot Nothing Then Return DirectCast(propertyInfo.GetValue(form), FakeControl46)
  Return DirectCast(form.GetType().GetField(name, flags).GetValue(form), FakeControl46)
 End Function
 Friend Shared Function CallMember(form As Object, name As String, ParamArray args() As Object) As Object
  Return form.GetType().GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public).Invoke(form, args)
 End Function
 Public Shared Iterator Function Bindings() As IEnumerable(Of Object())
  Dim root = Environment.GetEnvironmentVariable("BESHSTAT_04A02_SOURCE_ROOT")
  For Each entry In XDocument.Load(Path.Combine(root, "evidence/04A46/fixtures/bindings-wave-a-v1.xml")).Root.Elements()
   Yield New Object() {CStr(entry.Attribute("owner")), CStr(entry.Attribute("method")), CStr(entry.Attribute("source")), CStr(entry.Attribute("event"))}
  Next
 End Function
 <DataTestMethod, DynamicData(NameOf(Bindings), DynamicDataSourceType.Method)>
 Public Sub EverySelectedHandlesBindingRaisesTheOriginalCopiedHandler(owner As String, method As String, source As String, eventName As String)
  Dim form = Create(owner) : FakeEvents46.Delivered.Clear()
  Control(form, source).RaiseNamed(eventName)
  Dim expected = owner & "Projection46." & method
  If FakeEvents46.Delivered.Contains(expected) Then Return
  ' VB adapts parameterless Handles methods with a generated forwarding delegate.
  ' The delegate actually ran;verify its emitted call to the original copied method.
  Dim original = form.GetType().GetMethod(method, BindingFlags.NonPublic Or BindingFlags.Instance)
  Dim forwarded As Boolean
  For Each candidate In form.GetType().GetMethods(BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.NonPublic)
   If Not FakeEvents46.Delivered.Contains(candidate.DeclaringType.Name & "." & candidate.Name) Then Continue For
   Dim body = candidate.GetMethodBody()
   If body Is Nothing Then Continue For
   Dim bytes = body.GetILAsByteArray()
   For i = 0 To bytes.Length - 5
    If (bytes(i) = &H28 OrElse bytes(i) = &H6F) AndAlso BitConverter.ToInt32(bytes, i + 1) = original.MetadataToken Then forwarded = True
   Next
  Next
  Assert.IsTrue(forwarded, "Missing delivered source handler or compiler forwarding bridge")
 End Sub
 <DataTestMethod>
 <DataRow("UibyID")> <DataRow("Ui0OneRefeditMulticol")> <DataRow("UiTwoInputRefedits")> <DataRow("Ui82x2")> <DataRow("Ui8Proportions")> <DataRow("Ui9ANOVA2nested")> <DataRow("Ui01BulletChart")> <DataRow("Ui01ConvexHullPlot")> <DataRow("Ui01DumbbellPlot")> <DataRow("Ui01LadderPlot")> <DataRow("Ui01PolarPlot")> <DataRow("Ui01SankeyPlot")>
 Public Sub OutputChoicesDisableAndRestoreNestedReference(owner As String)
  Dim form = Create(owner), output = DirectCast(Control(form, "RefEditOutput"), FakeRefEdit46)
  Control(form, "optOutputRange").Checked = True : Control(form, "optOutputRange").FireClick()
  Assert.IsTrue(output.Enabled) : Assert.IsTrue(output.txtAddress.Selections > 0)
  Control(form, "optOutputRange").Checked = False : Control(form, "optWorksheet").Checked = True : Control(form, "optWorksheet").FireClick()
  Assert.IsFalse(output.Enabled)
  Control(form, "optWorkbook").Checked = True : Control(form, "optWorkbook").FireClick() : Assert.IsFalse(output.Enabled)
  Control(form, "optOutputRange").Checked = True : Control(form, "optOutputRange").FireClick() : Assert.IsTrue(output.Enabled)
 End Sub
 <DataTestMethod>
 <DataRow("NormalityTests")> <DataRow("HomogeneityOfVariance")> <DataRow("MannWhitneyTest")> <DataRow("UnpairedTTest")> <DataRow("ROCCurve")>
 Public Sub GroupedOrientationUsesExistingInputRolePolicy(topic As String)
  Dim form As New UibyIDProjection46 With {.Tag = CType([Enum].Parse(GetType(HelpTopic), topic), HelpTopic)}
  form.RefEdit1.Address = "group" : form.RefEdit2.Address = "values" : form.RefEditOutput.Address = "output"
  form.optByID.Checked = True : form.optByID.FireClick() : form.optOutputRange.Checked = True
  FakeRanges46.Reset() : Assert.IsFalse(CBool(CallMember(form, "checkInputs")))
  CollectionAssert.AreEqual(New String() {"group", "values", "output"}, FakeRanges46.Calls.Select(Function(c) c.Item1).ToArray())
  Assert.IsTrue(FakeRanges46.Calls(0).Item2) : Assert.IsTrue(FakeRanges46.Calls(1).Item2)
  form.optByID.Checked = False : form.optByColumn.Checked = True : form.optByColumn.FireClick()
  FakeRanges46.Reset() : CallMember(form, "checkInputs")
  Dim two = form.Tag = HelpTopic.MannWhitneyTest OrElse form.Tag = HelpTopic.UnpairedTTest OrElse form.Tag = HelpTopic.ROCCurve
  Assert.AreEqual(If(two, 3, 2), FakeRanges46.Calls.Count)
  form.optByID.Checked = True : form.optByID.FireClick() : Assert.IsTrue(form.RefEdit1.Enabled)
 End Sub
 <DataTestMethod>
 <DataRow("UiTwoInputRefedits", "PairedSingleSampleTTests")> <DataRow("Ui9ANOVA2nested", "TwoWayNestedANOVA")>
 Public Sub InputRoleRejectionsResetOnlyRejectedSelection(owner As String, topic As String)
  Dim form = Create(owner)
  form.GetType().GetField("Tag", BindingFlags.NonPublic Or BindingFlags.Instance).SetValue(form, [Enum].Parse(GetType(HelpTopic), topic))
  Dim names = If(owner = "UiTwoInputRefedits", New String() {"RefEdit1", "RefEdit2"}, New String() {"RefEdit1_Group", "RefEdit2_nested", "RefEdit3_Data"})
  For i = 0 To names.Length - 1
   DirectCast(Control(form, names(i)), FakeRefEdit46).Address = "role" & i.ToString()
  Next
  FakeRanges46.Reset() : FakeRanges46.Reject.Add("role1")
  Assert.IsTrue(CBool(CallMember(form, "checkInputs")))
  Assert.AreEqual("role0", DirectCast(Control(form, names(0)), FakeRefEdit46).Address)
  Assert.AreEqual("", DirectCast(Control(form, names(1)), FakeRefEdit46).Address)
  If owner = "Ui9ANOVA2nested" Then Assert.IsFalse(FakeRanges46.Calls(2).Item2)
 End Sub
 <TestMethod>
 Public Sub AssumptionChoicesProjectExactExistingFlags()
  Dim form As New UibyIDProjection46
  form.ckLevene.Checked = True : form.ckBartlett.Checked = True
  Assert.AreEqual(ExcelCommon.HomogeneityMethods.LeveneMedian Or ExcelCommon.HomogeneityMethods.Bartlett, CType(CallMember(form, "GetHomogeneityMethods"), ExcelCommon.HomogeneityMethods))
  form.ckLevene.Checked = False : form.ckFlignerKilleen.Checked = True : form.ckSquaredRanks.Checked = True
  Assert.AreEqual(ExcelCommon.HomogeneityMethods.FlignerKilleen Or ExcelCommon.HomogeneityMethods.SquaredRanks Or ExcelCommon.HomogeneityMethods.Bartlett, CType(CallMember(form, "GetHomogeneityMethods"), ExcelCommon.HomogeneityMethods))
 End Sub
 <DataTestMethod>
 <DataRow(0)> <DataRow(1)> <DataRow(2)>
 Public Sub CdfModesPercentilesAndLegendReverse(mode As Integer)
  Dim form As New UibyIDProjection46 With {.Tag = HelpTopic.CDFplot}
  form.cmbCDFPlotType.SelectedIndex = mode
  Assert.AreEqual(mode <> 0, form.cmbCDFDistribution.Enabled) : Assert.AreEqual(mode <> 2, form.cmbCDFEmpiricalMethod.Enabled)
  form.ckCDFPercentileLines.Checked = True : Assert.IsTrue(form.txtCDFPercentiles.Enabled)
  form.ckCDFPercentileLines.Checked = False : Assert.IsFalse(form.txtCDFPercentiles.Enabled)
  form.ckCDFShowLegend.Checked = True : Assert.IsTrue(form.cmbCDFLegendMode.Enabled)
  form.ckCDFShowLegend.Checked = False : Assert.IsFalse(form.cmbCDFLegendMode.Enabled)
 End Sub
 <TestMethod>
 Public Sub BoxViolinAndCategoricalStatePreservesValuesWhenDisabled()
  Dim form As New UibyIDProjection46 With {.Tag = HelpTopic.BoxAndWhiskers}
  form.cmdBoxMean.Checked = True : form.cmbBoxPalette.SelectedIndex = 4
  Assert.IsFalse(form.cmdBoxMean.Enabled) : Assert.IsTrue(form.cmdBoxMean.Checked) : Assert.IsFalse(form.ckBoxConnectMeans.Enabled)
  form.cmbBoxPalette.SelectedIndex = 0 : Assert.IsTrue(form.ckBoxConnectMeans.Enabled)
  form.Tag = HelpTopic.ViolinPlot : form.cmbViolinBandwidth.SelectedIndex = 2
  Assert.AreEqual(0.000001D, form.nudViolinBandwidthAdjustment.Minimum) : Assert.AreEqual(6, form.nudViolinBandwidthAdjustment.DecimalPlaces)
  form.cmbViolinBandwidth.SelectedIndex = 0 : Assert.AreEqual(100D, form.nudViolinBandwidthAdjustment.Maximum)
  form.Tag = HelpTopic.CategoricalHistogram : form.optCatHistStackedBar.Checked = True : Assert.IsFalse(form.nudCatHistSeriesOverlap.Enabled)
  form.optCatHistStackedBar.Checked = False : Assert.IsTrue(form.nudCatHistSeriesOverlap.Enabled)
 End Sub
 <TestMethod>
 Public Sub CategoricalWorksheetAndScreenInputsReverse()
  Dim form As New Ui82x2Projection46
  form.optWorksheetData.Checked = True : form.optWorksheetData.FireClick()
  Assert.IsTrue(form.RefEdit1_WorksheetData.Enabled) : Assert.IsFalse(form.spinBtnA.Enabled)
  form.optScreenData.FireClick() : Assert.IsFalse(form.RefEdit1_WorksheetData.Enabled) : Assert.IsTrue(form.spinBtnD.Enabled)
  form.optWorksheetData.FireClick() : Assert.IsFalse(form.spinBtnC.Enabled)
 End Sub
 <TestMethod>
 Public Sub ProportionModeAndDynamicHypothesisHandlerPreserveLabels()
  Dim form As New Ui8ProportionsProjection46
  form.optIndependent.Checked = True : form.cbHypothesisType.SelectedItem = "Noninferiority" : form.cbHypothesisType.SelectedIndex = 1
  Assert.AreEqual("One-sided alpha", form.lblAlpha.Text) : Assert.IsTrue(form.spinBtnMargin_IndProp.Visible)
  form.cbHypothesisType.SelectedItem = "Superiority" : form.cbHypothesisType.SelectedIndex = 0 : Assert.IsFalse(form.spinBtnMargin_IndProp.Visible)
  form.optPaired.Checked = True : Assert.IsTrue(form.lbl3.Text.Contains("2nd category"))
  form.optSingle.Checked = True : Assert.IsFalse(form.spinBtnD.Visible)
 End Sub
 <TestMethod>
 Public Sub BulletLadderSankeyIndependentOptionVectors()
  Dim bullet As New Ui01BulletChartProjection46
  bullet.ckShowScaleLabels.Checked = True : Assert.IsTrue(bullet.txtScaleNumberFormat.Enabled)
  bullet.ckShowActualValueLabels.Checked = True : Assert.IsTrue(bullet.txtValueNumberFormat.Enabled)
  bullet.ckShowActualValueLabels.Checked = False : Assert.IsFalse(bullet.txtValueNumberFormat.Enabled)
  Dim ladder As New Ui01LadderPlotProjection46
  ladder.ckShowLines.Checked = True : ladder.ckShowMarkers.Checked = False : Assert.IsTrue(ladder.nudConnectorWidth.Enabled) : Assert.IsFalse(ladder.nudMarkerSize.Enabled)
  ladder.ckShowMarkers.Checked = True : ladder.ckShowLines.Checked = False : Assert.IsTrue(ladder.nudMarkerSize.Enabled) : Assert.IsFalse(ladder.nudConnectorWidth.Enabled)
  ladder.ckShowFirstEndpointLabels.Checked = True : Assert.IsTrue(ladder.txtEndpointValueFormat.Enabled)
  ladder.ckShowFirstEndpointLabels.Checked = False : Assert.IsFalse(ladder.txtEndpointValueFormat.Enabled)
  Dim sankey As New Ui01SankeyPlotProjection46
  sankey.ckShowNodeLabels.Checked = True : sankey.cmbNodeLabelMode.SelectedIndex = 2 : Assert.IsTrue(sankey.cmbPercentageBasis.Enabled)
  sankey.cmbNodeLabelMode.SelectedIndex = 0 : Assert.IsFalse(sankey.cmbPercentageBasis.Enabled)
 End Sub
 <TestMethod>
 Public Sub XyzResetAndSuppressedRecalculationUseClosedSink()
  Dim form As New Ui3XYZplotProjection46
  form.pFigure = New Object() : form.pbPreventEvents = True : form.spinBtnRotationX.Value = 20
  Assert.AreEqual(0, form.Recalculations)
  form.btResetView.FireClick()
  Assert.AreEqual(120D, form.spinBtnRotationX.Value) : Assert.AreEqual(60D, form.spinBtnRotationZ.Value) : Assert.AreEqual(50D, form.spinBtnShiftX.Value) : Assert.AreEqual(1, form.Recalculations)
 End Sub
End Class
