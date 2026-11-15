Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfHandlePolicyTests
    <DataTestMethod, DataRow("LM"), DataRow("GLM"), DataRow("GLMNB"), DataRow("ZIP"), DataRow("GEE"), DataRow("COX"), DataRow("ORDLOGIT"), DataRow("MNLOGIT"), DataRow("LMM"), DataRow("MMRM"), DataRow("PCA"), DataRow("FA"), DataRow("KMEANS"), DataRow("HCLUST"), DataRow("CA"), DataRow("MCA"), DataRow("DA"), DataRow("PSM")>
    Public Sub GeneratedFamilyDescriptionDoesNotRestrictLegacyLookup(prefix As String)
        Dim key = prefix & ":0123456789abcdef0123456789abcdef"
        Assert.AreEqual(prefix, UdfModelHandlePolicy.IdentifyFamily(key.ToLowerInvariant()).ToString())
        Assert.AreEqual(UdfHandleSyntax.GeneratedShape, UdfModelHandlePolicy.InspectSyntax(key))
        Assert.IsTrue(UdfModelHandlePolicy.HasLookupKey(prefix & ":not-a-guid"))
        Assert.AreEqual(UdfHandleSyntax.Malformed, UdfModelHandlePolicy.InspectSyntax(prefix & ":not-a-guid"))
    End Sub

    <DataTestMethod, DataRow(Nothing, False), DataRow("", False), DataRow("  ", False), DataRow("unknown", True), DataRow("OTHER:opaque", True), DataRow("17", True), DataRow(" odd key ", True)>
    Public Sub ConvertedKeyEligibilityRetainsNonblankStrings(key As String, expected As Boolean)
        Assert.AreEqual(expected, UdfModelHandlePolicy.HasLookupKey(key))
    End Sub

    <DataTestMethod, DataRow(Nothing, True), DataRow("", True), DataRow(" ", True), DataRow("aLl", True), DataRow("ALL ", False), DataRow("PSM:opaque", False)>
    Public Sub CleanupReceivesAlreadyConvertedText(key As String, expected As Boolean)
        Assert.AreEqual(expected, UdfModelHandlePolicy.IsClearAllRequest(key, "ALL"))
    End Sub

    <TestMethod>
    Public Sub UnknownHostScopeCannotBecomeWorkbookOrSessionEvidence()
        Dim d As New UdfModelHandleDescriptor("LM:opaque", UdfHandleFamily.LM, sessionEpoch:=1, workbookEpoch:=10)
        Assert.AreEqual(UdfHandleValidity.Available, UdfModelHandlePolicy.Validity(d, UdfHandleFamily.LM, True, 2, 11))
        For Each signal In {UdfHandleEvent.WorkbookClose, UdfHandleEvent.WorkbookReopen, UdfHandleEvent.SessionEnd}
            Dim decision = UdfModelHandlePolicy.Plan(d, signal, 10, True)
            Assert.AreEqual(UdfHandleAction.NativeScopeNotVerified, decision.Action)
            Assert.IsFalse(decision.PerformsStorage)
            Assert.AreEqual(UdfHandleState.Unobserved, d.State)
        Next
    End Sub

    <TestMethod>
    Public Sub VerifiedSimulatedGenerationsDetectWrongSessionAndReopenedWorkbook()
        Dim d As New UdfModelHandleDescriptor("PCA:opaque", UdfHandleFamily.PCA, sessionEpoch:=1, sessionEvidence:=UdfHandleScopeEvidence.VerifiedByHost, workbookEpoch:=10, workbookEvidence:=UdfHandleScopeEvidence.VerifiedByHost)
        Assert.AreEqual(UdfHandleValidity.WrongVerifiedSession, UdfModelHandlePolicy.Validity(d, UdfHandleFamily.PCA, True, 2, 10))
        Assert.AreEqual(UdfHandleValidity.WrongVerifiedWorkbook, UdfModelHandlePolicy.Validity(d, UdfHandleFamily.PCA, True, 1, 11))
        Assert.AreEqual(UdfHandleAction.RecommendInvalidation, UdfModelHandlePolicy.Plan(d, UdfHandleEvent.WorkbookClose, 10, True).Action)
        Assert.AreEqual(UdfHandleAction.RecommendInvalidation, UdfModelHandlePolicy.Plan(d, UdfHandleEvent.WorkbookReopen, 11, True).Action)
        Assert.AreEqual(UdfHandleAction.None, UdfModelHandlePolicy.Plan(d, UdfHandleEvent.WorkbookSwitch, 11, True).Action)
        Assert.AreEqual(UdfHandleAction.NativeScopeNotVerified, UdfModelHandlePolicy.Plan(d, UdfHandleEvent.WorkbookClose, 11, True).Action)
    End Sub

    <TestMethod>
    Public Sub ImmutableTransitionsDoNotRemoveRebindOrRefitAnything()
        Dim first As New UdfModelHandleDescriptor("PSM:first", UdfHandleFamily.PSM)
        Dim fit = UdfModelHandlePolicy.Plan(first, UdfHandleEvent.RepeatedFit)
        Assert.AreEqual(UdfHandleAction.CreateIndependentHandle, fit.Action)
        Assert.AreEqual(first.State, fit.NextState)
        Dim removed = first.WithState(UdfModelHandlePolicy.Plan(first, UdfHandleEvent.ExplicitDrop).NextState)
        Assert.AreNotSame(first, removed)
        Assert.AreEqual(UdfHandleState.Unobserved, first.State)
        Assert.AreEqual(UdfHandleValidity.ExplicitlyInvalidated, UdfModelHandlePolicy.Validity(removed, UdfHandleFamily.PSM, False))
        Assert.AreEqual(UdfHandleState.FamilyCleared, UdfModelHandlePolicy.Plan(first, UdfHandleEvent.FamilyClear).NextState)
        Assert.AreEqual(UdfHandleValidity.NotFound, UdfModelHandlePolicy.Validity(first, UdfHandleFamily.PSM, False))
        Assert.AreEqual(UdfHandleValidity.HostLookupRequired, UdfModelHandlePolicy.Validity(first, UdfHandleFamily.PSM))
    End Sub

    <TestMethod>
    Public Sub AliasSubstitutionAndWrongFamilyFailRelationalValidation()
        Dim d As New UdfModelHandleDescriptor("LM:first", UdfHandleFamily.LM)
        Assert.IsTrue(UdfModelHandlePolicy.ReferenceMatches(d, "lm:FIRST", UdfHandleFamily.LM, True))
        Assert.IsFalse(UdfModelHandlePolicy.ReferenceMatches(d, "LM:second", UdfHandleFamily.LM, True))
        Assert.IsFalse(UdfModelHandlePolicy.ReferenceMatches(d, "LM:first", UdfHandleFamily.PCA, True))
        Assert.IsFalse(UdfModelHandlePolicy.ReferenceMatches(d, "LM:first", UdfHandleFamily.LM, False))
        Assert.AreEqual(UdfHandleValidity.FamilyMismatchReportOnly, UdfModelHandlePolicy.Validity(d, UdfHandleFamily.PCA, True))
        Assert.IsTrue(UdfModelHandlePolicy.HasLookupKey(d.ConvertedKey), "Diagnostic grammar/family findings never activate a new host error.")
        Assert.ThrowsException(Of ArgumentNullException)(Sub() UdfModelHandlePolicy.Plan(Nothing, UdfHandleEvent.WorkbookClose))
    End Sub
End Class
