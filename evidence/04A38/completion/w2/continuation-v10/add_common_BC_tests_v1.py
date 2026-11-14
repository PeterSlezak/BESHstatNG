"""Focused PCA/FA report-shape, type, exceptional and lazy native-option contracts."""
from common import *
path='BESHStat.Excel.Common.Tests/src/UdfContinuation10Tests.vb';before=(ROOT/path).read_text(encoding='utf-8-sig');position=before.rfind('End Class')
tests='''    Private Shared Function Continuation10Data() As Double(,)
        Return New Double(,) {{1, 2, 5}, {2, 1, 7}, {3, 4, 6}, {4, 3, 8}, {5, 6, 9}, {6, 5, 10}, {7, 8, 11}, {8, 7, 12}}
    End Function

    Private Shared Function Continuation10Pca() As Multivariate.PCA
        Dim model As New Multivariate.PCA
        model.dataInputs(Continuation10Data(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(250, 0.000001R, "Correlation")
        model.Calculate("Fixed", 2)
        Return model
    End Function

    Private Shared Function Continuation10Factor() As Multivariate.FactorAnalysis
        Dim model As New Multivariate.FactorAnalysis
        model.dataInputs(Continuation10Data(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(extractionMethod:=Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents,
                             retentionMethod:=Multivariate.FactorAnalysisRetentionMethod.Fixed, retentionValue:=2)
        model.Calculate()
        Return model
    End Function

    <TestMethod>
    Public Sub PcaSummaryPreservesHeaderRowsAndTypedRetentionValue()
        Dim model = Continuation10Pca(), ids = New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, names = New String() {"duplicate", "duplicate", "Z"}
        Dim headed = UdfMultivariateReportPreparation38.BuildPcaSummary(model, ids, names, "Correlation", "Fixed", 2.0R, True)
        Dim plain = UdfMultivariateReportPreparation38.BuildPcaSummary(model, ids, names, "Correlation", "Fixed", 2.0R, False)
        Assert.AreEqual(7, headed.GetLength(0)) : Assert.AreEqual(6, plain.GetLength(0))
        Assert.AreEqual("Setting", headed(0, 0)) : Assert.AreEqual(8, plain(1, 1))
        Assert.AreEqual(2, plain(3, 1)) : Assert.IsInstanceOfType(plain(5, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub PcaEigenPreservesComponentOrderValuesAndRetainedFlags()
        Dim model = Continuation10Pca()
        Dim body = UdfMultivariateReportPreparation38.BuildPcaEigen(model.Eigenvalues, model.PercentExpl, model.PercentExplCum, model, True)
        Assert.AreEqual(4, body.GetLength(0)) : Assert.AreEqual(5, body.GetLength(1))
        Assert.AreEqual("PC1", body(1, 0)) : Assert.AreEqual("PC3", body(3, 0))
        Assert.AreEqual(True, body(2, 4)) : Assert.AreEqual(False, body(3, 4))
        Assert.IsInstanceOfType(body(1, 1), GetType(Double))
        Assert.AreEqual(model.Eigenvalues(0), CDbl(body(1, 1)))
    End Sub

    <TestMethod>
    Public Sub PcaEigenKeepsOriginalMismatchedArrayFailure()
        Dim model = Continuation10Pca()
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfMultivariateReportPreparation38.BuildPcaEigen(New Double() {2, 1}, New Double() {60}, New Double() {60, 100}, model, False))
    End Sub

    <TestMethod>
    Public Sub PcaEigenEmptyArraysDoNotEagerlyReadModel()
        Dim body = UdfMultivariateReportPreparation38.BuildPcaEigen(New Double() {}, Nothing, Nothing, Nothing, True)
        Assert.AreEqual(1, body.GetLength(0))
        Assert.AreEqual("Component", body(0, 0))
        body = UdfMultivariateReportPreparation38.BuildPcaEigen(New Double() {}, Nothing, Nothing, Nothing, False)
        Assert.AreEqual(0, body.GetLength(0))
    End Sub

    <TestMethod>
    Public Sub PcaEigenDoesNotInventNonfiniteCellNormalization()
        Dim model = Continuation10Pca()
        Dim body = UdfMultivariateReportPreparation38.BuildPcaEigen(New Double() {Double.NaN, Double.PositiveInfinity}, New Double() {Double.NaN, Double.PositiveInfinity}, New Double() {0, 0}, model, False)
        Assert.IsTrue(Double.IsNaN(CDbl(body(0, 1))))
        Assert.IsTrue(Double.IsPositiveInfinity(CDbl(body(1, 1))))
        Assert.IsInstanceOfType(body(0, 4), GetType(Boolean))
    End Sub

    <TestMethod>
    Public Sub FactorSummaryKeepsInclusiveArrayBoundsAndConvergenceTypes()
        Dim model = Continuation10Factor(), names = New String() {"duplicate", "duplicate", "Z"}
        Dim headed = UdfMultivariateReportPreparation38.BuildFactorSummary(model, names, "Correlation", "PrincipalComponents", "None", "Regression", True)
        Dim plain = UdfMultivariateReportPreparation38.BuildFactorSummary(model, names, "Correlation", "PrincipalComponents", "None", "Regression", False)
        Assert.AreEqual(14, headed.GetLength(0)) : Assert.AreEqual(13, plain.GetLength(0))
        Assert.AreEqual("RMSR", headed(13, 0)) : Assert.AreEqual(8, plain(4, 1))
        Assert.IsInstanceOfType(plain(8, 1), GetType(Boolean))
        Assert.AreEqual(model.RMSR, CDbl(plain(12, 1)))
    End Sub

    <TestMethod>
    Public Sub FactorEigenPreservesOriginalHeaderCallbackAndBlankCells()
        Dim model = Continuation10Factor(), reads As Integer = 0, slot As Object = New Object()
        Dim body = UdfMultivariateReportPreparation38.BuildFactorEigen(model, slot,
            Function(value, defaultValue)
                Assert.AreSame(slot, value) : Assert.IsTrue(defaultValue) : reads += 1
                Return True
            End Function)
        Assert.AreEqual(1, reads) : Assert.AreEqual(4, body.GetLength(0)) : Assert.AreEqual(10, body.GetLength(1))
        Assert.AreEqual("Factor", body(0, 0)) : Assert.AreEqual(1, body(1, 0))
        Assert.IsNull(body(3, 4)) : Assert.IsInstanceOfType(body(1, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub FactorEigenModelFailurePrecedesHeaderConversion()
        Dim reads As Integer = 0, slot As Object = New Object()
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildFactorEigen(Nothing, slot,
            Function(value, defaultValue)
                reads += 1
                Return True
            End Function))
        Assert.AreEqual(0, reads)
    End Sub

    <TestMethod>
    Public Sub FactorEigenPropagatesHeaderExceptionAtTheOriginalLateBoundary()
        Dim model = Continuation10Factor(), reads As Integer = 0, slot As Object = New Object()
        Assert.ThrowsException(Of InvalidOperationException)(Sub() UdfMultivariateReportPreparation38.BuildFactorEigen(model, slot,
            Function(value, defaultValue) As Boolean
                reads += 1
                Throw New InvalidOperationException("header conversion")
            End Function))
        Assert.AreEqual(1, reads)
    End Sub

    <TestMethod>
    Public Sub FactorEigenByRefHeaderSlotKeepsReentrantAliasWrites()
        Dim model = Continuation10Factor(), slot As Object = New Object()
        Dim body = UdfMultivariateReportPreparation38.BuildFactorEigen(model, slot,
            Function(value, defaultValue)
                slot = "callback write"
                Return False
            End Function)
        Assert.AreEqual("callback write", CStr(slot))
        Assert.AreEqual(3, body.GetLength(0))
        Assert.AreEqual(1, body(0, 0))
    End Sub
'''
write_source(path,before[:position]+tests+before[position:]);save('common-BC-tests-v1.json',dict(source=pin(ROOT/path),new_methods=10,original_queries=0))
print('Ten PCA/FA Common edge/ordering/type tests added')
