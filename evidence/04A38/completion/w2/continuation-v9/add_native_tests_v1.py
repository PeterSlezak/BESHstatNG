"""Append six current adapter regressions; preserve every historical assertion."""
from common_v2 import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';source=(ROOT/path).read_text(encoding='utf-8-sig');index=source.rfind('\nEnd Class');assert index>0
added='''
    <TestMethod>
    Public Sub Continuation9CoefficientAdaptersKeepRowsAndTypedEstimates()
        Dim x As Object(,) = {{1.0R}, {2.0R}, {4.0R}, {5.0R}, {7.0R}, {8.0R}}
        Dim y As Object(,) = {{2.0R}, {3.0R}, {3.5R}, {5.6R}, {6.0R}, {8.0R}}
        For Each result In New Object() {AgreementMethodUDFs.PASSINGBABLOK_COEF(x, y), AgreementMethodUDFs.DEMING_COEF(x, y, ciMethod:="analytical")}
            Assert.IsInstanceOfType(result, GetType(Object(,)))
            Dim table = DirectCast(result, Object(,)), found As Boolean = False
            For row = 0 To table.GetLength(0) - 2
                If Object.Equals(table(row, 0), "Slope") Then
                    Assert.AreEqual("Intercept", CStr(table(row + 1, 0)))
                    Assert.IsInstanceOfType(table(row, 1), GetType(Double))
                    found = True
                End If
            Next
            Assert.IsTrue(found)
        Next
    End Sub

    <TestMethod>
    Public Sub Continuation9BlandAltmanAndRepeatabilityKeepNativeMaterialization()
        Dim x As Object(,) = {{1.0R}, {2.0R}, {4.0R}, {5.0R}, {7.0R}, {8.0R}}
        Dim y As Object(,) = {{2.0R}, {3.0R}, {3.5R}, {5.6R}, {6.0R}, {8.0R}}
        Dim stats = DirectCast(AgreementMethodUDFs.BLANDALTMAN_STATS(x, y), Object(,))
        Assert.IsTrue(stats.Cast(Of Object)().Any(Function(value) Object.Equals(value, "Repeatability coefficient")))
        Dim rc = DirectCast(AgreementMethodUDFs.ICC_RC(New Object(,) {{1.0R, 2.0R}, {2.0R, 3.2R}, {4.0R, 4.4R}, {5.0R, 5.7R}}), Object(,))
        Assert.AreEqual("SEM (standard error of measurement)", CStr(rc(rc.GetLength(0) - 1, 0)))
        Assert.IsInstanceOfType(rc(rc.GetLength(0) - 1, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub Continuation9ClassificationKeepsRowVectorsHeadersAndMissingErrors()
        Dim y As Object(,) = {{"Y", 0.0R, 1.0R, 1.0R, 0.0R}}
        Dim p As Object(,) = {{"P", 0.2R, 0.5R, 0.8R, 0.4R}}
        Dim table = DirectCast(ClassificationReportUDFs.CLASS_CONFUSION(y, p), Object(,))
        Assert.AreEqual(5, table.GetLength(0))
        Assert.AreEqual(4, table.GetLength(1))
        Assert.AreEqual(0, CInt(table(0, 1)))
        Assert.AreEqual(1, CInt(table(0, 2)))
        Dim missing = DirectCast(ClassificationReportUDFs.CLASS_CONFUSION(New Object(,) {{0.0R, 0.0R}}, New Object(,) {{0.1R, 0.2R}}), Object(,))
        Assert.AreEqual(ExcelError.ExcelErrorNA, missing(2, 3))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_THRESH(y, p), GetType(Object(,)))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_CALIB(y, p), GetType(Object(,)))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_BRIER(y, p), GetType(Object(,)))
    End Sub

    <TestMethod>
    Public Sub Continuation9BrierValidationPrecedesHeaderAndLowerBoundsStayNative()
        Dim probe As New Continuation8TextProbe
        Dim result = ClassificationReportUDFs.CLASS_BRIER(New Object(,) {{0.0R, 1.0R}}, New Object(,) {{0.2R, 0.8R}}, New Object(,) {{-1.0R, 1.0R}}, probe)
        Assert.IsInstanceOfType(result, GetType(String))
        Assert.IsTrue(CStr(result).StartsWith("ArgumentException:", StringComparison.Ordinal))
        Assert.AreEqual(0, probe.Reads)
        Dim lower = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {2, 1}, New Integer() {1, 1}), Object(,))
        lower(1, 1) = 0.0R : lower(2, 1) = 1.0R
        result = ClassificationReportUDFs.CLASS_BRIER(lower, New Object(,) {{0.2R}, {0.8R}})
        Assert.IsInstanceOfType(result, GetType(String))
        Assert.IsTrue(CStr(result).StartsWith("IndexOutOfRangeException:", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub Continuation9ContingencyKeepsShapeFailuresAndPValueTypes()
        Dim oneRow As Object(,) = {{1.0R, 2.0R, 3.0R}}
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_CHI2(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_NOMINAL_ASSOC(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_FISHER_2X2(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_FFH_EXACT(oneRow))
        Dim tab As Object(,) = {{8.0R, 2.0R}, {3.0R, 7.0R}}
        For Each result In New Object() {ContingencyTableUDFs.CT_CHI2(tab), ContingencyTableUDFs.CT_NOMINAL_ASSOC(tab), ContingencyTableUDFs.CT_FISHER_2X2(tab), ContingencyTableUDFs.CT_FFH_EXACT(tab)}
            Assert.IsInstanceOfType(result, GetType(Object(,)))
        Next
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_CHI2(New Object(,) {{ExcelError.ExcelErrorNA, 1.0R}, {2.0R, 3.0R}}))
    End Sub

    <TestMethod>
    Public Sub Continuation9RealCompiledConsumersCallEachPortableFamily()
        For Each name In New String() {"PASSINGBABLOK_COEF", "DEMING_COEF", "BLANDALTMAN_STATS", "ICC_RC"}
            Assert.IsTrue(Calls(GetType(AgreementMethodUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfAgreementReportPreparation38)), name)
        Next
        For Each name In New String() {"CT_CHI2", "CT_NOMINAL_ASSOC", "CT_FISHER_2X2", "CT_FFH_EXACT"}
            Assert.IsTrue(Calls(GetType(ContingencyTableUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfContingencyPreparation38)), name)
        Next
        For Each name In New String() {"BuildBinaryCrosstabUdfOutput", "BuildThresholdTableUdfOutput", "BuildCalibrationTableUdfOutput", "BuildBrierScoreUdfOutput"}
            Assert.IsTrue(Calls(GetType(BESHStatNG.regression.BinaryClassificationReporting).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfClassificationReportPreparation38)), name)
        Next
    End Sub
'''
write_source(path,source[:index]+added+source[index:]);save('new-native-tests-v1.json',dict(path=path,new_test_methods=6,after=pin(ROOT/path),historical_assertions_changed=0))
print('Six focused current native adapter tests appended')
