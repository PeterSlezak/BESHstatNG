Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Multivariate

''' <summary>
''' Direct .NET 8 tests for the portable PCA model. Independent reference values
''' are the same R/prcomp benchmark exercised by the legacy Framework tests.
''' The Framework tests are deliberately retained as assembly-boundary tests.
''' </summary>
<TestClass>
Public Class PcaCoreTests

    Private Shared Function ReferenceData() As Double(,)
        Return New Double(,) {
            {2.0R, 0.0R, 1.0R},
            {3.0R, 1.0R, 0.0R},
            {4.0R, 0.0R, 2.0R},
            {5.0R, 1.0R, 1.0R},
            {6.0R, 2.0R, 1.0R},
            {7.0R, 1.0R, 3.0R},
            {8.0R, 2.0R, 2.0R},
            {9.0R, 3.0R, 2.0R},
            {10.0R, 2.0R, 4.0R},
            {11.0R, 3.0R, 3.0R}}
    End Function

    Private Shared Function FitReference(matrixType As String,
                                         Optional extractionMethod As String = "Fixed",
                                         Optional extractionCoefficient As Double = 3.0R) As PCA
        Dim model As New PCA()
        model.dataInputs(ReferenceData(),
                         New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10},
                         New String() {"A", "B", "C"}, "Core test")
        model.settingsInputs(2000, 0.000000000001R, matrixType)
        model.Calculate(extractionMethod, extractionCoefficient)
        Return model
    End Function

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CovariancePca_MatchesRReferenceMatrix()
        Dim model As PCA = FitReference("Covariance")
        Dim expected As Double(,) = {
            {9.1666666666666661R, 2.833333333333333R, 2.833333333333333R},
            {2.833333333333333R, 1.166666666666667R, 0.5R},
            {2.833333333333333R, 0.5R, 1.4333333333333329R}}
        AssertMatrixNear(expected, model.VarCovarMat, 0.00000001R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CorrelationPca_MatchesRReferenceMatrix()
        Dim model As PCA = FitReference("Correlation")
        Dim expected As Double(,) = {
            {1.0R, 0.866400225443963R, 0.781660828194291R},
            {0.866400225443963R, 1.0R, 0.386654448216613R},
            {0.781660828194291R, 0.386654448216613R, 1.0R}}
        AssertMatrixNear(expected, model.VarCovarMat, 0.00000001R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CovariancePca_MatchesRReferenceEigenvaluesAndVariance()
        Dim model As PCA = FitReference("Covariance")
        AssertVectorNear(New Double() {10.92627272R, 0.81886844R, 0.02152551R}, model.Eigenvalues, 0.000005R)
        AssertVectorNear(New Double() {92.85784182R, 6.95922185R, 0.18293633R}, model.PercentExpl, 0.00005R)
        AssertNear(100.0R, model.PercentExplCum(2), 0.000001R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CorrelationPca_MatchesRReferenceEigenvaluesAndVariance()
        Dim model As PCA = FitReference("Correlation")
        AssertVectorNear(New Double() {2.37546206R, 0.61595097R, 0.00858697R}, model.Eigenvalues, 0.000005R)
        AssertVectorNear(New Double() {79.18206872R, 20.53169897R, 0.28623231R}, model.PercentExpl, 0.00005R)
        AssertNear(100.0R, model.PercentExpl.Sum(), 0.000001R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CovariancePca_LoadingsMatchRReferenceSignConvention()
        Dim model As PCA = FitReference("Covariance")
        Dim expected As Double(,) = {
            {0.91559231R, -0.0671046R, -0.39646904R},
            {0.28056504R, -0.59969846R, 0.7494298R},
            {0.28805206R, 0.79740751R, 0.53025209R}}
        AssertMatrixNear(expected, model.GetLoadings, 0.00005R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CorrelationPca_LoadingsMatchRReferenceSignConvention()
        Dim model As PCA = FitReference("Correlation")
        Dim expected As Double(,) = {
            {0.64682908R, -0.04334726R, 0.7614021R},
            {0.55459291R, -0.65857364R, -0.50863294R},
            {0.52348719R, 0.75126678R, -0.4019445R}}
        AssertMatrixNear(expected, model.GetLoadings, 0.00005R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub EigenvalueExtraction_SelectsOneComponent()
        Dim model As PCA = FitReference("Covariance", "Eigenvalue", 1.0R)
        Assert.AreEqual(1, model.NoExtractComponents)
        Assert.AreEqual(1, model.ReducedDataset.GetLength(1))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CumulativeVarianceExtraction_SelectsTwoComponents()
        Dim model As PCA = FitReference("Covariance", "Variance", 95.0R)
        Assert.AreEqual(2, model.NoExtractComponents)
        Assert.AreEqual(2, model.GetLoadings.GetLength(1))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub FixedExtraction_ClampsComponentCountToVariableCount()
        Assert.AreEqual(1, FitReference("Correlation", "Fixed", 0.0R).NoExtractComponents)
        Assert.AreEqual(3, FitReference("Correlation", "Fixed", 99.0R).NoExtractComponents)
        Assert.AreEqual(2, FitReference("Correlation", "Fixed", 2.0R).NoExtractComponents)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub ScoresMatchStandardizedDataTimesLoadings()
        Dim model As PCA = FitReference("Correlation", "Fixed", 2.0R)
        AssertScores(model.StandardizedData, model.GetLoadings, model.ReducedDataset)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CovarianceScoresMatchCenteredDataTimesLoadings()
        Dim model As PCA = FitReference("Covariance", "Fixed", 2.0R)
        Dim centered As Double(,) = ReferenceData()
        For j As Integer = 0 To centered.GetLength(1) - 1
            Dim mean As Double = 0.0R
            For i As Integer = 0 To centered.GetLength(0) - 1
                mean += centered(i, j)
            Next
            mean /= centered.GetLength(0)
            For i As Integer = 0 To centered.GetLength(0) - 1
                centered(i, j) -= mean
            Next
        Next
        AssertScores(centered, model.GetLoadings, model.ReducedDataset)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CorrelationPca_RejectsConstantColumn()
        Dim data As Double(,) = {
            {1.0R, 4.0R},
            {2.0R, 4.0R},
            {3.0R, 4.0R}}
        Dim model As New PCA()
        model.dataInputs(data, New Integer() {1, 2, 3}, New String() {"X", "Constant"})
        model.settingsInputs(2000, 0.000000000001R, "Correlation")
        Assert.ThrowsException(Of ArgumentException)(Sub() model.Calculate("Fixed", 1.0R))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub CollinearCorrelation_HasOneNonzeroEigenvalue()
        Dim model As New PCA()
        Dim data As Double(,) = {{1.0R, 2.0R}, {2.0R, 4.0R}, {3.0R, 6.0R}, {4.0R, 8.0R}, {5.0R, 10.0R}}
        model.dataInputs(data, New Integer() {1, 2, 3, 4, 5}, New String() {"X", "Y"})
        model.settingsInputs(2000, 0.000000000001R, "Correlation")
        model.Calculate("Eigenvalue", 1.0R)
        AssertNear(2.0R, model.Eigenvalues(0), 0.000005R)
        AssertNear(0.0R, model.Eigenvalues(1), 0.000005R)
        Assert.AreEqual(1, model.NoExtractComponents)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub ModelExposesOriginalRowAndVariableLabels()
        Dim model As PCA = FitReference("Covariance", "Fixed", 2.0R)
        Assert.AreEqual(10, model.ObservationCount)
        Assert.AreEqual(3, model.VariableCount)
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}, model.RowIds)
        CollectionAssert.AreEqual(New String() {"A", "B", "C"}, model.VariableNames)
        CollectionAssert.AreEqual(New String() {"PC1", "PC2"}, model.PCnames("PC"))
        CollectionAssert.AreEqual(New String() {"Standardized_A", "Standardized_B"},
                                  model.StandardizedVarNames(New String() {"A", "B"}))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-PCA")>
    Public Sub WrapResults_ProducesFourHostNeutralTablesWithExpectedDimensions()
        Dim model As PCA = FitReference("Correlation", "Fixed", 2.0R)
        Dim tables = model.wrapResults()
        Assert.AreEqual(4, tables.Count)
        For Each t In tables
            Assert.IsNotNull(t)
            Dim output As Object(,) = t.returnSelf()
            Assert.IsNotNull(output)
            Assert.AreEqual(t.TotalRows, output.GetLength(0))
            Assert.AreEqual(t.TotalCols, output.GetLength(1))
        Next
        Assert.AreEqual(1, tables(3).HeadersLeftCount)
        Assert.AreEqual(3, tables(3).TotalCols) ' Left header plus two retained components
    End Sub

    Private Shared Sub AssertScores(data As Double(,), loadings As Double(,), scores As Double(,))
        Assert.AreEqual(data.GetLength(0), scores.GetLength(0))
        Assert.AreEqual(loadings.GetLength(1), scores.GetLength(1))
        For i As Integer = 0 To data.GetLength(0) - 1
            For j As Integer = 0 To loadings.GetLength(1) - 1
                Dim expected As Double = 0.0R
                For k As Integer = 0 To data.GetLength(1) - 1
                    expected += data(i, k) * loadings(k, j)
                Next
                AssertNear(expected, scores(i, j), 0.000001R)
            Next
        Next
    End Sub

    Private Shared Sub AssertMatrixNear(expected As Double(,), actual As Double(,), tolerance As Double)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                AssertNear(expected(i, j), actual(i, j), tolerance)
            Next
        Next
    End Sub

    Private Shared Sub AssertVectorNear(expected As Double(), actual As Double(), tolerance As Double)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            AssertNear(expected(i), actual(i), tolerance)
        Next
    End Sub

    Private Shared Sub AssertNear(expected As Double, actual As Double, tolerance As Double)
        Assert.AreEqual(expected, actual, tolerance)
    End Sub

End Class
