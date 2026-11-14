"""Actual cached Windows PCA/FA consumers, without FunctionWizard/native Excel execution."""
from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';before=(ROOT/path).read_text(encoding='utf-8-sig');position=before.rfind('End Class')
tests='''    Private Shared Function Continuation10NativeData() As Double(,)
        Return New Double(,) {{1, 2, 5}, {2, 1, 7}, {3, 4, 6}, {4, 3, 8}, {5, 6, 9}, {6, 5, 10}, {7, 8, 11}, {8, 7, 12}}
    End Function

    Private Shared Sub WithContinuation10Handle(family As String, model As Object, action As Action(Of String))
        Dim owner = GetType(MultivariateAnalysisUDFs), isPca = (family = "PCA")
        Dim handleType = owner.GetNestedType(If(isPca, "PcaHandle", "FactorAnalysisHandle"), BindingFlags.NonPublic)
        Dim info = Activator.CreateInstance(handleType, True), key = family & ":" & Guid.NewGuid().ToString("N")
        handleType.GetProperty("Handle").SetValue(info, key, Nothing)
        handleType.GetProperty("Model").SetValue(info, model, Nothing)
        handleType.GetProperty("VariableNames").SetValue(info, New String() {"duplicate", "duplicate", "Z"}, Nothing)
        handleType.GetProperty("MatrixType").SetValue(info, "Correlation", Nothing)
        If isPca Then
            handleType.GetProperty("RowIds").SetValue(info, New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, Nothing)
            handleType.GetProperty("RetentionMethod").SetValue(info, "Fixed", Nothing)
            handleType.GetProperty("RetentionValue").SetValue(info, 2.0R, Nothing)
        Else
            handleType.GetProperty("ExtractionMethod").SetValue(info, "PrincipalComponents", Nothing)
            handleType.GetProperty("RotationMethod").SetValue(info, "None", Nothing)
            handleType.GetProperty("ScoreMethod").SetValue(info, "Regression", Nothing)
        End If
        Dim cache = owner.GetField(If(isPca, "_pcaCache", "_faCache"), BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing)
        cache.GetType().GetProperty("Item").SetValue(cache, info, New Object() {key})
        Try
            action(key)
        Finally
            Dim remove = cache.GetType().GetMethods().Single(Function(m) m.Name = "TryRemove" AndAlso m.GetParameters().Length = 2)
            remove.Invoke(cache, New Object() {key, Nothing})
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaCachedConsumersKeepNamesRowOrderAndShapes()
        Dim model As New BESHStatNG.Multivariate.PCA
        model.dataInputs(Continuation10NativeData(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(250, 0.000001R, "Correlation") : model.Calculate("Fixed", 2)
        WithContinuation10Handle("PCA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.PCA_SUMMARY(" " & key.ToUpperInvariant() & " ", False), Object(,))
                Assert.AreEqual(6, summary.GetLength(0)) : Assert.AreEqual(8, CInt(summary(1, 1)))
                Dim eigen = DirectCast(MultivariateAnalysisUDFs.PCA_EIGEN(key, True), Object(,))
                Assert.AreEqual(4, eigen.GetLength(0)) : Assert.AreEqual("PC1", eigen(1, 0))
                Dim loadings = DirectCast(MultivariateAnalysisUDFs.PCA_LOADINGS(key, False), Object(,))
                Assert.AreEqual("duplicate", loadings(0, 0)) : Assert.AreEqual("duplicate", loadings(1, 0))
                Dim scores = DirectCast(MultivariateAnalysisUDFs.PCA_SCORES(key, False), Object(,))
                Assert.AreEqual(17, CInt(scores(0, 0))) : Assert.AreEqual(3, CInt(scores(1, 0)))
                Dim matrix = DirectCast(MultivariateAnalysisUDFs.PCA_MATRIX(key, False), Object(,))
                Assert.AreEqual(3, matrix.GetLength(0)) : Assert.AreEqual(4, matrix.GetLength(1))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10FactorCachedConsumersKeepMatricesAndSummaryBounds()
        Dim model As New BESHStatNG.Multivariate.FactorAnalysis
        model.dataInputs(Continuation10NativeData(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(extractionMethod:=BESHStatNG.Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents,
                             retentionMethod:=BESHStatNG.Multivariate.FactorAnalysisRetentionMethod.Fixed, retentionValue:=2)
        model.Calculate()
        WithContinuation10Handle("FA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.FA_SUMMARY(key, False), Object(,))
                Assert.AreEqual(13, summary.GetLength(0)) : Assert.AreEqual("RMSR", summary(12, 0))
                Dim eigen = DirectCast(MultivariateAnalysisUDFs.FA_EIGEN(key), Object(,))
                Assert.AreEqual(4, eigen.GetLength(0)) : Assert.AreEqual(10, eigen.GetLength(1))
                For Each value In New Object() {MultivariateAnalysisUDFs.FA_MATRIX(key, False), MultivariateAnalysisUDFs.FA_LOADINGS(key, False), MultivariateAnalysisUDFs.FA_STRUCTURE(key, False)}
                    Dim table = DirectCast(value, Object(,))
                    Assert.AreEqual(3, table.GetLength(0)) : Assert.AreEqual("duplicate", table(0, 0))
                Next
                Dim correlations = DirectCast(MultivariateAnalysisUDFs.FA_FACTORCORR(key, False), Object(,))
                Assert.AreEqual(2, correlations.GetLength(0)) : Assert.AreEqual(3, correlations.GetLength(1))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaAndFactorUnknownHandlesPrecedeHeaderReads()
        Dim probe As New Continuation8TextProbe
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.PCA_SUMMARY("C10 missing handle", probe))
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.FA_EIGEN("C10 missing handle", probe))
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <TestMethod>
    Public Sub Continuation10FactorDerivedFailurePrecedesNativeHeaderConversion()
        Dim probe As New Continuation8TextProbe
        WithContinuation10Handle("FA", New BESHStatNG.Multivariate.FactorAnalysis(),
            Sub(key)
                Dim result = MultivariateAnalysisUDFs.FA_EIGEN(key, probe)
                Assert.IsInstanceOfType(result, GetType(String))
                Assert.AreEqual(0, probe.Reads)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaFactorCompiledConsumersKeepPortableAndHeaderTargets()
        For Each name In New String() {"PCA_SUMMARY", "PCA_EIGEN", "FA_SUMMARY", "FA_EIGEN"}
            Assert.IsTrue(Calls(GetType(MultivariateAnalysisUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfMultivariateReportPreparation38)), name)
        Next
        Dim fa = GetType(MultivariateAnalysisUDFs).GetMethod("FA_EIGEN")
        Assert.IsTrue(Calls(fa).Any(Function(target) target.Name = "GetOptionalBool" AndAlso target.DeclaringType Is GetType(ExcelArgNumeric)))
        Dim api = GetType(UdfMultivariateReportPreparation38).GetMethod("BuildFactorEigen")
        Assert.IsTrue(api.GetParameters()(1).ParameterType.IsByRef)
        Assert.IsFalse(Calls(fa).Any(Function(target) target.Name = "GetObjectValue"))
    End Sub

'''
write_source(path,before[:position]+tests+before[position:]);save('native-BC-tests-v1.json',dict(source=pin(ROOT/path),new_methods=5,original_queries=0,scope='Managed cache fixtures use current Core models and remove only their unique keys in Finally. No FunctionWizard/PCA_FIT/FA_FIT/native Excel call.'))
source=(HERE/'focused_v3.py').read_text(encoding='utf-8').replace('focus-v3','focus-bc-v1').replace('focused-v3-','focused-bc-v1-').replace("'FullyQualifiedName~Contingency'","'FullyQualifiedName~PcaCoreTests|FullyQualifiedName~FactorAnalysisCoreTests'").replace("'FullyQualifiedName~UdfContinuation10Tests'","'FullyQualifiedName~UdfContinuation10Tests.Pca|FullyQualifiedName~UdfContinuation10Tests.Factor'").replace("'FullyQualifiedName~Continuation10'","'FullyQualifiedName~Continuation10Pca|FullyQualifiedName~Continuation10Factor'")
with (HERE/'focused_BC_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
print('Five managed real cached Windows consumer tests added; focused BC selector versioned')
