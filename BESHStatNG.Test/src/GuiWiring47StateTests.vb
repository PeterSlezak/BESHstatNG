Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Xml.Linq
Imports BESHStatNG.Test.Harness46
Imports BESHStatNG.Test.Harness47
Imports Microsoft.VisualStudio.TestTools.UnitTesting
<TestClass, DoNotParallelize>
Public Class GuiWiring47StateTests
 Friend Shared Function Create(owner As String) As Object
  Return Activator.CreateInstance(GetType(UiGLMProjection47).Assembly.GetType("BESHStatNG.Test.Harness47." & owner & "Projection47", True), True)
 End Function
 Friend Shared Function Control(form As Object, name As String) As FakeControl47
  Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic Or BindingFlags.Public
  Dim p = form.GetType().GetProperty(name, flags)
  Return DirectCast(If(p Is Nothing, form.GetType().GetField(name, flags).GetValue(form), p.GetValue(form)), FakeControl47)
 End Function
 Friend Shared Function Invoke(form As Object, name As String, ParamArray values() As Object) As Object
  Return form.GetType().GetMethod(name, BindingFlags.Instance Or BindingFlags.Static Or BindingFlags.NonPublic Or BindingFlags.Public).Invoke(form, values)
 End Function
 Public Shared Iterator Function Bindings() As IEnumerable(Of Object())
  For Each e In XDocument.Load(Path.Combine(Environment.GetEnvironmentVariable("BESHSTAT_04A02_SOURCE_ROOT"), "evidence/04A47/fixtures/bindings-a-v1.xml")).Root.Elements()
   Yield New Object() {CStr(e.Attribute("owner")), CStr(e.Attribute("handler")), CStr(e.Attribute("control")), CStr(e.Attribute("event"))}
  Next
 End Function
 <DataTestMethod, DynamicData(NameOf(Bindings), DynamicDataSourceType.Method)>
 Public Sub SourceHandlesAndDynamicSubscriptionsActuallyDeliver(owner As String, handler As String, source As String, eventName As String)
  Dim form = Create(owner) : FakeEvents46.Delivered.Clear()
  Control(form, source).RaiseNamed(eventName)
  CollectionAssert.Contains(FakeEvents46.Delivered, owner & "Projection47." & handler)
 End Sub
 <DataTestMethod>
 <DataRow("UiGLM", "Gaussian")> <DataRow("UiGLM", "Binomial")> <DataRow("UiGLM", "Poisson")> <DataRow("UiGLM", "Gamma")> <DataRow("UiGLM", "unknown")>
 <DataRow("Ui13GEE", "Gaussian")> <DataRow("Ui13GEE", "Binomial")> <DataRow("Ui13GEE", "Poisson")> <DataRow("Ui13GEE", "Gamma")> <DataRow("Ui13GEE", "unknown")>
 Public Sub FamilyLinkAndClassificationEventsPreserveReversibleState(owner As String, family As String)
  Dim form = Create(owner), tag = form.GetType().GetField("Tag", BindingFlags.Instance Or BindingFlags.NonPublic)
  tag.SetValue(form, If(owner = "UiGLM", HelpTopic.GeneralizedLinearModelsGLM, HelpTopic.GeneralizedEstimatingEquationsGEE))
  Control(form, "cbFamily").SelectedItem = family : Control(form, "cbLink").SelectedIndex = -1
  Control(form, "cbFamily").RaiseNamed("SelectedIndexChanged")
  Assert.AreEqual(family = "Binomial", Control(form, "cbPerformClasification").Enabled)
  If family = "unknown" Then
   Assert.AreEqual(0, Control(form, "cbLink").Items.Count)
  Else
   Assert.IsTrue(Control(form, "cbLink").Items.Count > 0)
  End If
  For Each state In {True, False, True}
   Control(form, "cbOutputCalibrationTable").Checked = state
   Assert.AreEqual(state AndAlso family = "Binomial", Control(form, "spinBtnCallibrationBinsN").Enabled)
   Control(form, "cbPerformClasification").Checked = state
   Assert.AreEqual(state AndAlso family = "Binomial", Control(form, "grpClassification").Enabled)
  Next
  Control(form, "cbLink").SelectedItem = "Power" : Control(form, "cbLink").RaiseNamed("SelectedIndexChanged")
  Assert.IsTrue(Control(form, "tbPower").Enabled)
  Control(form, "cbLink").SelectedItem = "Log" : Control(form, "cbLink").RaiseNamed("SelectedIndexChanged")
  Assert.IsFalse(Control(form, "tbPower").Enabled)
 End Sub
 <DataTestMethod> <DataRow("UiGLM", "")> <DataRow("UiGLM", "en-US")> <DataRow("UiGLM", "sk-SK")> <DataRow("Ui13GEE", "")> <DataRow("Ui13GEE", "en-US")> <DataRow("Ui13GEE", "sk-SK")>
 Public Sub ThresholdDefaultDomainAndLegacyNanRemainAsShipped(owner As String, culture As String)
  Dim saved = CultureInfo.CurrentCulture, form = Create(owner)
  Try
   CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture) : SourceNumbers46.HostCulture = CultureInfo.CurrentCulture
   Assert.AreEqual(0.5, CDbl(Invoke(form, "GetClassificationThresholdUiValue")))
   For Each v In {0.0, 1.0, 0.25}
    Control(form, "tbClassificationTreshold").Text = v.ToString(CultureInfo.CurrentCulture)
    Assert.AreEqual(v, CDbl(Invoke(form, "GetClassificationThresholdUiValue")))
   Next
   Control(form, "tbClassificationTreshold").Text = "NaN"
   Assert.IsTrue(Double.IsNaN(CDbl(Invoke(form, "GetClassificationThresholdUiValue"))))
   For Each invalid In {"bad", "-1", "2"}
    Control(form, "tbClassificationTreshold").Text = invalid
    Dim ex = Assert.ThrowsException(Of TargetInvocationException)(Sub() Invoke(form, "GetClassificationThresholdUiValue"))
    Assert.IsInstanceOfType(ex.InnerException, GetType(FormatException))
   Next
  Finally
   CultureInfo.CurrentCulture = saved : SourceNumbers46.HostCulture = CultureInfo.InvariantCulture
  End Try
 End Sub
 <DataTestMethod> <DataRow(False)> <DataRow(True)>
 Public Sub MixedReferenceGridContrastAndBaselineStatesReverse(means As Boolean)
  Dim f As New Ui18MMRMProjection47
  f.cbMMRMGroupingFactor.SelectedItem = "treatment" : f.cbMMRMLSMeansMode.SelectedItem = "Reference grid"
  f.ckMMRMEstimatedMeans.Checked = means : f.ckMMRMChangeFromBaseline.Checked = True
  f.cbMMRMContrastMode.SelectedItem = "Selected comparison only" : f.cbMMRMContrastMode.RaiseNamed("SelectedIndexChanged")
  Assert.AreEqual(means, f.cbMMRMComparisonLevel.Enabled) : Assert.AreEqual(means, f.cbMMRMBaselineVisit.Enabled)
  Assert.AreEqual(means, f.cbMMRMRefGridWeighting.Enabled) : Assert.AreEqual(means, f.cbMMRMMultiplicity.Enabled)
  f.cbMMRMGroupingFactor.SelectedItem = "(None)" : f.cbMMRMGroupingFactor.RaiseNamed("SelectedIndexChanged")
  Assert.IsFalse(f.cbMMRMComparisonLevel.Enabled) : Assert.IsFalse(f.cbMMRMMultiplicity.Enabled)
  f.cbMMRMGroupingFactor.SelectedItem = "treatment" : f.cbMMRMContrastMode.SelectedItem = "None" : f.cbMMRMContrastMode.RaiseNamed("SelectedIndexChanged")
  Assert.IsFalse(f.cbMMRMControlLevel.Enabled) : Assert.IsFalse(f.ckMMRMDiffInChange.Enabled)
  f.ckMMRMEstimatedMeans.Checked = Not means : Assert.AreEqual(Not means, f.cbMMRMLSMeansMode.Enabled)
 End Sub
 <DataTestMethod> <DataRow("CS")> <DataRow("AR(1)")> <DataRow("unknown")>
 Public Sub LmmCovarianceAuthoringControlsPreserveShippingAvailability(covariance As String)
  Dim f As New Ui19LMMProjection47
  f.btnRandom2Interactions.Enabled = False : f.cbCovarStruct.SelectedItem = covariance
  f.cbCovarStruct.RaiseNamed("SelectedIndexChanged")
  Assert.IsTrue(f.btnRandom2Interactions.Enabled) : Assert.IsTrue(f.spinBtnRandomPoly.Enabled)
  Dim control As New regression.MixedModelControl
  Invoke(f, "ApplyLMMCovarianceOptimizerSelections", control)
  Assert.IsNotNull(control)
 End Sub
 <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)>
 Public Sub MultivariateOptionBindingsAndReversalsUseActualChoicePolicies(index As Integer)
  Dim f As New Ui11PCAProjection47 With {.Tag = HelpTopic.DiscriminantAnalysis}
  f.cbFARotation.SelectedIndex = index
  Dim rotation = CType(Invoke(f, "GetSelectedFARotationMethod"), Multivariate.FactorAnalysisRotationMethod)
  Assert.AreEqual(rotation <> Multivariate.FactorAnalysisRotationMethod.None, f.ckFAKaiserNormalization.Enabled)
  Assert.AreEqual(rotation = Multivariate.FactorAnalysisRotationMethod.Promax, f.nudFAPromaxPower.Enabled)
  f.optFAExtractEigen.Checked = True : Assert.IsTrue(f.nudFAEigen.Enabled)
  f.optFAExtractEigen.Checked = False : Assert.IsFalse(f.nudFAEigen.Enabled)
  f.optHierarchicalCutByClusters.Checked = True : Assert.IsTrue(f.nudHierarchicalClusters.Enabled) : Assert.IsFalse(f.tbHierarchicalCutHeight.Enabled)
  f.optHierarchicalCutByClusters.Checked = False : Assert.IsFalse(f.nudHierarchicalClusters.Enabled) : Assert.IsTrue(f.tbHierarchicalCutHeight.Enabled)
  f.cbDAPriors.SelectedItem = "User specified" : f.cbDAPriors.RaiseNamed("SelectedIndexChanged")
  Dim prior = CType(Invoke(f, "GetSelectedDiscriminantPriorMode"), Multivariate.DiscriminantPriorMode)
  Assert.AreEqual(prior = Multivariate.DiscriminantPriorMode.UserSpecified, f.tbDAUserPriors.Enabled)
  f.cbDAValidation.SelectedItem = "K-fold" : f.cbDAValidation.RaiseNamed("SelectedIndexChanged")
  Dim mode = CType(Invoke(f, "GetSelectedDiscriminantValidationMode"), Multivariate.DiscriminantValidationMode)
  Assert.AreEqual(mode = Multivariate.DiscriminantValidationMode.KFold, f.nudDAFolds.Enabled)
 End Sub
End Class
