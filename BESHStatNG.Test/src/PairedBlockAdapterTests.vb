Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class PairedBlockAdapterTests
    Private Shared Function Imported(values As Object(,), names As String(), Optional labels As Integer = -1, Optional start As Integer = 20) As DataObj
        Dim data As New DataObj
        data.DataImportRawMatrix(values, names, firstSourceRow:=start, CharCols:=labels)
        Return data
    End Function

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub PairedProducerToCommonToCore_MatchesExistingReference(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim data = Imported(New Object(,) {{"pre", "post"}, {1, 1}, {2, 1}, {Nothing, 2}, {3, 2}, {4, 3}, {5, 5}}, {"pre", "post"})
            Dim errorText As String = ""
            Dim adapted = WindowsPairedInputAdapter.SelectPairs(data, errorText)
            Assert.AreEqual("", errorText)
            CollectionAssert.AreEqual(data.DataDbl.Cast(Of Double)().ToArray(), adapted.X.Cast(Of Double)().ToArray())
            Assert.AreEqual("pre", adapted.name1)
            Dim result = New parametric.PairedTtest(adapted.X, {adapted.name1, adapted.name2}).compute()
            ' Existing ParametricTests.PairedTtest_compute_matches_reference, not a new golden.
            Assert.AreEqual(2.4494897427831779, result.TestStatistics1, 0.000000001)
            Assert.AreEqual(0.070483996910219962, result.Pvalue, 0.00000005)
            Dim parsed = PairedBlockInputPolicy.ParsePairs(WindowsPairedInputAdapter.Capture(data), New SpreadsheetPairedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip}, data.CoreTable.RowIds)
            CollectionAssert.AreEqual(New Integer() {21, 22, 24, 25, 26}, parsed.Data.Pairs.Select(Function(p) p.First.WorksheetRow).ToArray())
            Dim before = New nonparametric.WilcoxonTest(data.DataDbl, "pre", "post")
            Dim after = New nonparametric.WilcoxonTest(adapted.X, "pre", "post")
            CollectionAssert.AreEqual(before.Differences, after.Differences)
            Assert.AreEqual(before.Compute().Pvalue, after.Compute().Pvalue, 0.000000000001)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub BlocksProducer_CompleteRowsAndLegacyNumericCulture(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim data = Imported(New Object(,) {{"A", "B", "C"}, {1, 2, 3}, {2, 2, 4}, {9, True, 9}, {3, 4, 5}, {4, 3, 6}, {Nothing, Nothing, Nothing}}, {"A", "B", "C"})
            Dim errorText As String = ""
            Dim adapted = WindowsPairedInputAdapter.SelectBlocks(data, errorText)
            Assert.AreEqual("", errorText)
            CollectionAssert.AreEqual(data.DataDbl.Cast(Of Double)().ToArray(), adapted.X.Cast(Of Double)().ToArray())
            Dim result = New parametric.OneWayRmANOVA(adapted.X, adapted.varNames).compute()
            Assert.AreEqual(15.545454545454524, CDbl(result(0, 3)), 0.000000001)
            Assert.AreEqual(0.0042330297170771669, CDbl(result(0, 4)), 0.00000005)
            Dim beforeF = New nonparametric.Friedman(data.DataDbl, data.varNames).compute()
            Dim afterF = New nonparametric.Friedman(adapted.X, adapted.varNames).compute()
            Assert.AreEqual(beforeF.TestStatistics1, afterF.TestStatistics1, 0.000000000001)
            Assert.AreEqual(beforeF.Pvalue, afterF.Pvalue, 0.000000000001)
            Dim numeric = Imported(New Object(,) {{If(culture = "", "1.5", "1,5"), 2}, {3, 4}}, {"X", "Y"})
            Assert.AreEqual(1.5, WindowsPairedInputAdapter.SelectPairs(numeric, errorText).X(0, 0))
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub Hotelling_RealProducerRowsIntersectInFirstSourceOrder()
        Dim a = Imported(New Object(,) {{1, 2}, {Nothing, 9}, {3, 4}, {5, 6}}, {"a", "b"}, start:=20)
        Dim b = Imported(New Object(,) {{9, 10}, {11, 12}, {Nothing, 8}}, {"c", "d"}, start:=21)
        Dim errorText As String = ""
        Dim rows = WindowsPairedInputAdapter.AlignHotelling(a, b, "book", "sheet", "book", "sheet", errorText)
        CollectionAssert.AreEqual(a.RowIds.Intersect(b.RowIds).ToArray(), rows)
        CollectionAssert.AreEqual(New Integer() {22}, rows)
        Assert.IsNull(WindowsPairedInputAdapter.AlignHotelling(a, b, "book", "sheet", "other", "sheet", errorText))
        Assert.IsTrue(errorText.Length > 0)
    End Sub

    <TestMethod>
    Public Sub NestedProducer_ExactOriginalTypesAndExtraResponseOrder()
        Dim data = Imported(New Object(,) {{"B", "b1", 10, 20}, {"A", "a1", 2, 3}, {"A", Nothing, 9, 9}, {"B", "b1", 11, 21}}, {"parent", "child", "Y", "extra"}, labels:=1)
        Dim errorText As String = ""
        Dim adapted = WindowsPairedInputAdapter.SelectNested(data, errorText)
        CollectionAssert.AreEqual(data.FinalData.Cast(Of Object)().ToArray(), adapted.X.Cast(Of Object)().ToArray())
        CollectionAssert.AreEqual(data.varNames, adapted.varNames)
        adapted.X(0, 0) = "mutated"
        Assert.AreEqual("B", data.FinalData(0, 0))
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub InvalidInputAbortsAtAdapter_NoWriteCapability(block As Boolean)
        Dim empty = Imported(New Object(,) {{Nothing, Nothing}, {"header", "header"}}, {"X", "Y"})
        Dim errorText As String = "prior worksheet diagnostic"
        If block Then
            Assert.IsNull(WindowsPairedInputAdapter.SelectBlocks(empty, errorText))
        Else
            Assert.IsNull(WindowsPairedInputAdapter.SelectPairs(empty, errorText))
        End If
        Assert.IsTrue(errorText.Length > 0)
        Assert.IsNull(WindowsPairedInputAdapter.SelectPairs(Nothing, errorText))
    End Sub

    <TestMethod>
    Public Sub AdapterDoesNotClearExistingIdentityDiagnostic_AndSnapshotsAreDetached()
        Dim data = Imported(New Object(,) {{1, 2}, {3, 4}}, {"X", "Y"})
        Dim snapshot = WindowsPairedInputAdapter.Capture(data)
        Dim errorText As String = "existing worksheet mismatch"
        Assert.IsNotNull(WindowsPairedInputAdapter.SelectPairs(data, errorText))
        Assert.AreEqual("existing worksheet mismatch", errorText)
        data.CoreTable.NumericMatrix(0, 0) = 99
        Assert.AreEqual(1.0, snapshot.Rows(0)(0).NumberValue)
        Assert.AreEqual(0, snapshot.StartColumn)
    End Sub

    <TestMethod>
    Public Sub NestedBeforeAfter_CoreSemanticTableParity()
        Dim raw As Object(,) = {{"A", "A1", 10}, {"A", "A1", 12}, {"A", "A2", 13}, {"A", "A2", 15},
                               {"B", "B1", 20}, {"B", "B1", 22}, {"B", "B2", 23}, {"B", "B2", 25}}
        Dim data = Imported(raw, {"group", "nested", "Y"}, labels:=1)
        Dim errorText As String = ""
        Dim adapted = WindowsPairedInputAdapter.SelectNested(data, errorText)
        Dim before = New parametric.TwoWayNestedANOVA(DirectCast(data.FinalData.Clone(), Object(,)), data.varNames).compute()
        Dim after = New parametric.TwoWayNestedANOVA(adapted.X, adapted.varNames).compute()
        CollectionAssert.AreEqual(before.Cast(Of Object)().ToArray(), after.Cast(Of Object)().ToArray())
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)>
    Public Sub BlandProjection_PreservesExistingCoreResultAndTableStructure(scale As Integer)
        Dim x As Double() = {10, 20, 30, 40, 50}, y As Double() = {11, 22, 31, 42, 53}
        Dim before As New Agreement.BlandAltmanOptions With {.Mode = Agreement.RepeatedBlandAltmanMode.SimplePairs, .Scale = CType(scale, Agreement.BlandAltmanScale), .Alpha = 0.05}
        Dim after = PairedOptionPolicy.ProjectBland(1, scale, False, False, False)
        after.Alpha = before.Alpha
        Dim a As New Agreement.BlandAltmanAgreement(x, y, "reference", "test", before)
        Dim b As New Agreement.BlandAltmanAgreement(x, y, "reference", "test", after)
        Dim fitA = a.Fit(), fitB = b.Fit()
        Assert.AreEqual(fitA.BiasCI.Estimate, fitB.BiasCI.Estimate, 0.000000000001)
        Assert.AreEqual(fitA.LowerLoACI.Estimate, fitB.LowerLoACI.Estimate, 0.000000000001)
        Assert.AreEqual(fitA.UpperLoACI.Estimate, fitB.UpperLoACI.Estimate, 0.000000000001)
        Dim tablesA = a.wrapResults(), tablesB = b.wrapResults()
        Assert.AreEqual(tablesA.Count, tablesB.Count)
        For i = 0 To tablesA.Count - 1
            CollectionAssert.AreEqual(tablesA(i).returnSelf().Cast(Of Object)().ToArray(), tablesB(i).returnSelf().Cast(Of Object)().ToArray())
        Next
        Assert.IsTrue(PairedOptionPolicy.ValidateBland(after, x, y).IsValid)
    End Sub
End Class
