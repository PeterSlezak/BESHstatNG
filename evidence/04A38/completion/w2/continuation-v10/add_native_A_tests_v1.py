"""Focused real Windows-adapter regression cases; historical assertions unchanged."""
from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';before=(ROOT/path).read_text(encoding='utf-8-sig');assert before.count('End Class')>=1;index=before.rfind('End Class')
tests='''    <TestMethod>
    Public Sub Continuation10ContingencyImportAndAlphaPrecedenceRemainNative()
        Dim probe As New Continuation8TextProbe
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_SINGLE_PROPORTION("invalid", 10, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_MCNEMAR_EXACT(New Object(,) {{1.0R, 2.0R, 3.0R}}, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_NI("invalid", 10, 2, 10, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_SINGLE_PROPORTION(1, 10, 1.0R))
    End Sub

    <TestMethod>
    Public Sub Continuation10ContingencyHeaderLowerBoundsAndErrorCellsRemainNative()
        Dim lower = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {2, 2}, New Integer() {1, 1}), Object(,))
        lower(1, 1) = 8.0R : lower(1, 2) = 2.0R : lower(2, 1) = 3.0R : lower(2, 2) = 7.0R
        Dim expected = DirectCast(ContingencyTableUDFs.CT_ODDS_RATIO(New Object(,) {{8.0R, 2.0R}, {3.0R, 7.0R}}), Object(,))
        Dim actual = DirectCast(ContingencyTableUDFs.CT_ODDS_RATIO(lower), Object(,))
        CollectionAssert.AreEqual(expected.Cast(Of Object)().ToArray(), actual.Cast(Of Object)().ToArray())
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_RISK_RATIO(New Object(,) {{ExcelError.ExcelErrorNA, 1.0R}, {2.0R, 3.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_MANTEL_HAENSZEL(New Object(,) {{1.5R, 1.0R}, {2.0R, 3.0R}}))
    End Sub

    <TestMethod>
    Public Sub Continuation10ProportionOverflowAndMarginBoundariesRemainNative()
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_PAIRED_PROPORTIONS(Integer.MaxValue, Integer.MaxValue, 0, 1))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_NI(3, 10, 4, 10, 0.0R))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV(3, 10, 4, 10, 0.2R, -0.2R))
        Assert.IsInstanceOfType(ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV(30, 100, 35, 100, 0.2R), GetType(Object(,)))
        Assert.IsInstanceOfType(ContingencyTableUDFs.CT_PAIRED_PROPORTIONS(10, 1, 2, 3), GetType(Object(,)))
    End Sub

    <TestMethod>
    Public Sub Continuation10RealContingencyConsumersReachAllElevenPreparedPhases()
        For Each name In New String() {"CT_MCNEMAR_EXACT", "CT_ODDS_RATIO", "CT_RISK_RATIO", "CT_TREND", "CT_ORDINAL_ASSOC", "CT_MANTEL_HAENSZEL", "CT_SINGLE_PROPORTION", "CT_TWO_INDEPENDENT_PROPORTIONS", "CT_PAIRED_PROPORTIONS", "CT_TWO_INDEPENDENT_PROPORTIONS_NI", "CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV"}
            Assert.IsTrue(Calls(GetType(ContingencyTableUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfContingencyPreparation38)), name)
        Next
    End Sub

'''
write_source(path,before[:index]+tests+before[index:]);save('native-A-tests-v1.json',dict(source=pin(ROOT/path),new_methods=4,historical_assertions_changed=0,original_queries=0))
for name in ['build_v1.py','focused_v3.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
    if name=='focused_v3.py':source=source.replace('FullyQualifiedName~Agreement|FullyQualifiedName~BinaryClassification|FullyQualifiedName~Contingency','FullyQualifiedName~Contingency').replace('UdfContinuation9Tests','UdfContinuation10Tests').replace('FullyQualifiedName~Continuation9','FullyQualifiedName~Continuation10')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Four real Windows adapter tests added; offline focused Debug runners pinned')
