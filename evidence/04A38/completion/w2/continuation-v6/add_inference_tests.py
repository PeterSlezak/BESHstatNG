from common import *
common=r'''
    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Inference38KeepsReportChoicesNamesAndPairOwnership(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Assert.AreEqual(0, UdfInferencePreparation38.NestedTableSelection("BOTH"))
            Assert.AreEqual(1, UdfInferencePreparation38.NestedTableSelection("CLASSICAL"))
            Assert.AreEqual(2, UdfInferencePreparation38.NestedTableSelection("SW"))
            Assert.AreEqual(-1, UdfInferencePreparation38.NestedTableSelection("unknown"))
            Assert.AreEqual(2, UdfInferencePreparation38.IndependentTTableSelection("ＷＥＬＣＨ"))
            CollectionAssert.AreEqual(New String() {"G1", "G2", "G3"}, UdfInferencePreparation38.KruskalGroupNames(3))
            CollectionAssert.AreEqual(New String() {"G1", "G2", "G3"}, UdfInferencePreparation38.FriedmanGroupNames(3))
            Dim pairs As Double(,) = {{1.0R, 9.0R}, {3.0R, 2.0R}, {7.0R, 5.0R}}
            Dim x() As Double = Nothing, y() As Double = Nothing
            UdfInferencePreparation38.SplitCorrelationColumns(pairs, x, y)
            CollectionAssert.AreEqual(New Double() {1.0R, 3.0R, 7.0R}, x)
            CollectionAssert.AreEqual(New Double() {9.0R, 2.0R, 5.0R}, y)
            x(0) = 99.0R
            Assert.AreEqual(1.0R, pairs(0, 0))
            Dim lower = DirectCast(Array.CreateInstance(GetType(Double), {2, 2}, {1, 1}), Double(,))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfInferencePreparation38.SplitCorrelationColumns(lower, x, y))
            Assert.AreEqual(2, x.Length)
            Assert.AreEqual(2, y.Length)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <TestMethod>
    Public Sub Inference38MeanReportBodiesKeepLabelsRawNumbersAndCiHelpers()
        Dim a() As Double = {1.0R, 2.0R, 4.0R, 8.0R}
        Dim b() As Double = {2.0R, 4.0R, 7.0R, 11.0R}
        Dim ni = Global.BESHStatNG.equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiority(a, b, 1.0R, 0.025R, False)
        Dim body = UdfInferencePreparation38.MeanNonInferiorityBody(ni, {"Control", "Trial"})
        Assert.AreEqual(22, body.GetLength(0))
        Assert.AreEqual(2, body.GetLength(1))
        Assert.AreEqual("Control", body(0, 1))
        Assert.AreEqual("Trial", body(1, 1))
        Assert.AreEqual("One-sided p-value", body(14, 0))
        Assert.AreEqual(ni.PValue, body(14, 1))
        Assert.AreEqual(UdfOutputTablesReports38.SafeCiLabel(ni.TwoSidedEquivalentConfidenceInterval), body(16, 0))
        Dim eq = Global.BESHStatNG.equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalence(a, b, -2.0R, 2.0R, 0.025R, False)
        body = UdfInferencePreparation38.MeanEquivalenceBody(eq, {"Control", "Trial"})
        Assert.AreEqual(25, body.GetLength(0))
        Assert.AreEqual(eq.TostPValue, body(17, 1))
        Assert.AreEqual("Conclusion", body(24, 0))
    End Sub
'''
native=r'''
    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Inference38ShippingFamiliesRetainErrorsShapesAndPairedRows(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim data As Object(,) = {{"A", "B", "C"}, {1.0R, 4.0R, 3.0R}, {2.0R, 3.0R, 5.0R}, {4.0R, 8.0R, 6.0R}, {3.0R, 6.0R, 9.0R}}
            Assert.IsInstanceOfType(ParametricUDFs.RMANOVA1(data, correction:="both"), GetType(Object(,)))
            Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.RMANOVA1(data, correction:="unknown"))
            Assert.IsInstanceOfType(ParametricUDFs.RMANOVA1_MCP(data, method:="all"), GetType(Object(,)))
            Assert.IsInstanceOfType(NonparametricUDFs.KW_MCP(data), GetType(Object(,)))
            Assert.IsInstanceOfType(NonparametricUDFs.FRIEDMAN_MCP(data, method:="all"), GetType(Object(,)))
            Dim x As Object(,) = {{"X"}, {1.0R}, {2.0R}, {4.0R}, {8.0R}}
            Dim y As Object(,) = {{"Y"}, {2.0R}, {4.0R}, {7.0R}, {11.0R}}
            Assert.IsInstanceOfType(ParametricUDFs.TTEST_PAIRED(x, y), GetType(Object(,)))
            Assert.IsInstanceOfType(ParametricUDFs.TTEST_UNPAIRED(x, y, outputType:="welch"), GetType(Object(,)))
            Dim ni = DirectCast(ParametricUDFs.TTEST_UNPAIRED_NI(x, y, 1.0R), Object(,))
            Dim eq = DirectCast(ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 2.0R), Object(,))
            Assert.AreEqual(23, ni.GetLength(0))
            Assert.AreEqual(26, eq.GetLength(0))
            Assert.AreEqual("One-sided p-value", ni(15, 0))
            Assert.IsInstanceOfType(NonparametricUDFs.SPEARMAN_RHO(x, y), GetType(Double))
            Assert.IsInstanceOfType(NonparametricUDFs.KENDALL_P(x, y), GetType(Double))
            Assert.AreEqual(ExcelError.ExcelErrorNum, NonparametricUDFs.KENDALL_P(New Object(,) {{1.0R}, {2.0R}, {3.0R}}, New Object(,) {{2.0R}, {4.0R}, {3.0R}}))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub
'''
for path,addition in [('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',common),('BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',native)]:
 text=(ROOT/path).read_text(encoding='utf-8-sig');assert text.count('\nEnd Class')==1
 before=HERE/'intermediate-preimages/inference-tests'/(path+'.txt');before.parent.mkdir(parents=True,exist_ok=True)
 with before.open('xb') as f:f.write((ROOT/path).read_bytes())
 write_source(path,text.replace('\nEnd Class','\n'+addition+'\nEnd Class'))
print('Inference family/culture/shape and report regression tests added')
