"""Append focused shipping tests; preserve every existing historical test byte."""
from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
source=(ROOT/path).read_text(encoding='utf-8-sig')
assert pin(ROOT/path)==json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'][2]['live']
tests='''
    Private NotInheritable Class Continuation8TextProbe
        Public Reads As Integer
        Public Overrides Function ToString() As String
            Reads += 1
            Return "NI"
        End Function
    End Class

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Continuation8PlanningAdaptersKeepErrorPrecedenceAndReportShapes(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim probe As New Continuation8TextProbe()
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 0.0R, 1.0R, 0.05R, 0.2R, probe, 0.0R))
            Assert.AreEqual(0, probe.Reads)
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, probe, 0.0R))
            Assert.AreEqual(1, probe.Reads)
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, "unknown", 0.0R))
            Dim unpaired = DirectCast(SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(0.2R, 1.0R, 1.0R, 0.05R, 0.2R, "eq", 1.0R), Object(,))
            Assert.AreEqual(4, unpaired.GetLength(0)) : Assert.AreEqual(4, unpaired.GetLength(1))
            Dim proportions = DirectCast(SampleSizeUDFs.SSIZE_PROP_INDEP(0.4R, 0.45R, 1.0R, 0.05R, 0.2R, "NI", 0.2R), Object(,))
            Assert.AreEqual(3, proportions.GetLength(0)) : Assert.AreEqual(3, proportions.GetLength(1))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(Double.NaN, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(Double.PositiveInfinity, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(True, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE("0.6", 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_LOGRANK(ExcelError.ExcelErrorNA, 0.3R, 0.2R, 1.0R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_LOGRANK(0.7R, 0.0R, 0.2R, 1.0R, 0.05R, 0.2R))
            Dim binary = DirectCast(SampleSizeUDFs.SSIZE_COX_BINARY(0.7R, 1.0R, 0.05R, 0.2R, ExcelMissing.Value, ExcelEmpty.Value, False), Object(,))
            Assert.AreEqual("Binary covariate", binary(1, 1)) : Assert.AreEqual("Not supplied", binary(4, 1))
            Dim continuous = DirectCast(SampleSizeUDFs.SSIZE_COX_CONTINUOUS(0.8R, 1.0R, 0.05R, 0.2R, 0.0R, 0.3R), Object(,))
            Assert.AreEqual("Continuous covariate", continuous(1, 1))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_COX_BINARY(0.7R, 1.0R, 0.05R, 0.2R, 1.0R, ExcelError.ExcelErrorNA))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_ICC(0.5R, 0.75R, 2.5R, 0.05R, 0.2R))
            Dim icc = DirectCast(SampleSizeUDFs.SSIZE_ICC(0.5R, 0.75R, 3.0R, 0.05R, 0.2R), Object(,))
            Assert.AreEqual(6, icc.GetLength(0))
            Dim bland = DirectCast(SampleSizeUDFs.SSIZE_BLANDALTMAN(2.0R, 1.0R, 0.05R, ExcelMissing.Value), Object(,))
            Assert.AreEqual(7, bland.GetLength(0)) : Assert.AreEqual(1.96R, bland(6, 1))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation8ImportParentsKeepAlignmentFlagsAndFailureResets()
        Dim y As Object(,) = {{"Y"}, {1.0R}, {2.0R}, {3.0R}, {4.0R}}
        Dim x As Object(,) = {{"X"}, {2.0R}, {4.0R}, {6.0R}, {8.0R}}
        Dim offset As Object(,) = {{"Offset"}, {-1.0R}, {0.0R}, {1.0R}, {2.0R}}
        Dim weights As Object(,) = {{"Weight"}, {1.0R}, {1.0R}, {2.0R}, {2.0R}}
        Dim groups As Object(,) = {{"ID"}, {"a"}, {"b"}, {"a"}, {"b"}}
        Dim time As Object(,) = {{"Time"}, {1.0R}, {2.0R}, {3.0R}, {4.0R}}
        Dim glm As BESHStatNG.glmData = Nothing
        Assert.IsTrue(UdfDataImport.TryBuildGlmDataFromUdfArgs(y, x, Nothing, offset, weights, glm))
        Assert.AreEqual(4, glm.nRows) : Assert.IsTrue(glm.bOffset) : Assert.IsTrue(glm.bWeights)
        CollectionAssert.AreEqual(New Double() {-1.0R, 0.0R, 1.0R, 2.0R}, glm.OffsetData)
        Assert.AreEqual("Offset", glm.OffsetVarName) : Assert.AreEqual("Weight", glm.WeightVarName)
        Assert.IsFalse(UdfDataImport.TryBuildGlmDataFromUdfArgs(y, New Object(,) {{1.0R}, {2.0R}}, Nothing, Nothing, Nothing, glm))
        Assert.IsNull(glm)
        Assert.IsTrue(UdfDataImport.TryBuildPredictorDataFromUdfArgs(x, {"X"}, offset, True, glm))
        Assert.IsTrue(glm.bOffset) : Assert.IsFalse(glm.bWeights)
        Assert.IsFalse(UdfDataImport.TryBuildPredictorDataFromUdfArgs(x, {"X"}, ExcelMissing.Value, True, glm))
        Assert.IsNull(glm)
        Dim gee As BESHStatNG.geeData = Nothing
        Assert.IsTrue(UdfDataImport.TryBuildGeeDataFromUdfArgs(y, x, groups, time, Nothing, offset, weights, gee))
        Assert.AreEqual(4, gee.nRows) : Assert.IsTrue(gee.bTime) : Assert.IsTrue(gee.bOffset) : Assert.IsTrue(gee.bWeights)
        Assert.AreEqual(1.0R, Convert.ToDouble(gee.ClusterIdData(0)))
        Assert.AreEqual(2.0R, Convert.ToDouble(gee.ClusterIdData(1)))
        Dim cox As BESHStatNG.CoxPHData = Nothing
        Assert.IsTrue(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, groups, cox))
        Assert.IsTrue(cox.bStrata) : Assert.AreEqual(4, cox.nRows)
        Assert.AreEqual("a", cox.StrataData(0)) : Assert.AreEqual(1, cox.CensorData(1))
        Dim column(,) As Object = Nothing, name As String = Nothing
        Assert.IsTrue(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"ID"}, {"a"}, {"b"}}, 2, column, name))
        Assert.AreEqual("ID", name) : Assert.AreEqual("b", column(1, 0))
        Assert.IsFalse(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"a"}, {"b"}}, 3, column, name))
        Assert.IsNull(column) : Assert.IsNull(name)
        Assert.IsTrue(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"ID"}}, 0, column, name))
        Assert.AreEqual(0, column.GetLength(0))
        Dim inferred As String() = {"duplicate", "duplicate"}
        Assert.AreSame(inferred, UdfDataImport.ResolveImportedPredictorNames(ExcelMissing.Value, inferred))
    End Sub

    <TestMethod>
    Public Sub Continuation8CompiledShippingConsumersReachTheirPreparedApis()
        For Each member In New String() {"SSIZE_TTEST_UNPAIRED", "SSIZE_PROP_SINGLE", "SSIZE_PROP_INDEP", "SSIZE_LOGRANK", "SSIZE_COX_BINARY", "SSIZE_COX_CONTINUOUS", "SSIZE_ICC", "SSIZE_BLANDALTMAN"}
            Dim method = GetType(SampleSizeUDFs).GetMethod(member, BindingFlags.Public Or BindingFlags.Static)
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfSampleSizePreparation38)), member)
        Next
        For Each member In New String() {"TryBuildGeeDataFromUdfArgs", "TryBuildGlmDataFromUdfArgs", "TryBuildPredictorDataFromUdfArgs", "TryBuildCoxDataFromUdfArgs"}
            Dim method = GetType(UdfDataImport).GetMethod(member, BindingFlags.NonPublic Or BindingFlags.Static)
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfModelImportPreparation38)), member)
        Next
    End Sub
'''
assert source.count('End Class')==1
write_source(path,source.replace('End Class',tests+'\nEnd Class'))
save('adapter-test-addition-v1.json',dict(source=pin(ROOT/path),before=json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'][2],added_methods=4,original_queries=0))
print('Appended4 focused native adapter cases; historical assertions unchanged')
