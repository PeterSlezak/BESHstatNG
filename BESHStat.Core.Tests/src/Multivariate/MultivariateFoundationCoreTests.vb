Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Multivariate

<TestClass>
Public Class MultivariateFoundationCoreTests

    Private Const Tol As Double = 0.00000001R

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub InputValidation_AcceptsRectangularMatricesAndRejectsNullOrEmptyData()
        MultivariateInputHelpers.ValidateRectangularData(New Double(,) {{1.0R, 2.0R}, {3.0R, 4.0R}})

        Assert.ThrowsException(Of ArgumentException)(
            Sub() MultivariateInputHelpers.ValidateRectangularData(Nothing))

        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() MultivariateInputHelpers.ValidateRectangularData(Nothing, nullMessage:=Nothing, nullParamName:="data"))

        Dim emptyRows As Double(,) = DirectCast(Array.CreateInstance(GetType(Double), 0, 2), Double(,))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() MultivariateInputHelpers.ValidateRectangularData(emptyRows))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub InputLabels_DefaultNamesRespectRowAndColumnConventions()
        CollectionAssert.AreEqual(New String() {"Row 1", "Row 2", "Row 3"},
                                   MultivariateInputHelpers.NormalizeRowLabels(Nothing, 3))
        CollectionAssert.AreEqual(New String() {"X1", "X2"},
                                   MultivariateInputHelpers.NormalizeVarNames(Nothing, 2))
        CollectionAssert.AreEqual(New String() {"Measure 1", "Measure 2"},
                                   MultivariateInputHelpers.NormalizeVarNames(Nothing, 2,
                                       defaultPrefix:="Measure", useSpaceSeparator:=True))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub InputLabels_PreserveInputsByCopyingAndHandleMismatchedLengths()
        Dim source As String() = {"Original", "Second"}
        Dim normalized As String() = MultivariateInputHelpers.NormalizeRowLabels(source, 2)
        source(0) = "Changed"
        Assert.AreEqual("Original", normalized(0))

        Assert.ThrowsException(Of ArgumentException)(
            Sub() MultivariateInputHelpers.NormalizeVarNames(New String() {"Only one"}, 2))

        CollectionAssert.AreEqual(New String() {"Var1", "Var2"},
                                   MultivariateInputHelpers.NormalizeVarNames(
                                       New String() {"Only one"}, 2,
                                       defaultPrefix:="Var", allowDefaultOnLengthMismatch:=True))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub Center_PreservesRelativeDifferencesAndDoesNotMutateInput()
        Dim source As Double() = {1.0R, 2.0R, 3.0R}
        Dim result As Double() = MultivariateShared.Center(source)
        AssertArrayClose(New Double() {-1.0R, 0.0R, 1.0R}, result)
        AssertArrayClose(New Double() {1.0R, 2.0R, 3.0R}, source)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub Standardize_UsesSampleStandardDeviationAndRejectsConstantData()
        AssertArrayClose(New Double() {-1.0R, 0.0R, 1.0R},
                         MultivariateShared.Standardize(New Double() {1.0R, 2.0R, 3.0R}))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() MultivariateShared.Standardize(New Double() {7.0R, 7.0R, 7.0R}))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub EigenpairSorting_PreservesEigenvectorColumnAlignment()
        Dim eigenvalues As Double() = {1.0R, 4.0R, 2.0R}
        Dim eigenvectors As Double(,) = {
            {1.0R, 0.0R, 0.0R},
            {0.0R, 1.0R, 0.0R},
            {0.0R, 0.0R, 1.0R}}

        Dim sorted = MultivariateShared.SortEigenpairsDescending(eigenvalues, eigenvectors)
        AssertArrayClose(New Double() {4.0R, 2.0R, 1.0R}, sorted.Item1)
        AssertClose(1.0R, sorted.Item2(1, 0))
        AssertClose(1.0R, sorted.Item2(2, 1))
        AssertClose(1.0R, sorted.Item2(0, 2))
        AssertArrayClose(New Double() {1.0R, 4.0R, 2.0R}, eigenvalues)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub FactorLoadings_UseSquareRootEigenvalueAndClampNegativeValues()
        Dim eigenvalues As Double() = {9.0R, -0.00000001R}
        Dim eigenvectors As Double(,) = {{1.0R, 0.0R}, {0.0R, 1.0R}}
        Dim loadings As Double(,) = MultivariateShared.BuildLoadingsFromEigenpairs(eigenvalues, eigenvectors, 2)
        Assert.AreEqual(2, loadings.GetLength(0))
        Assert.AreEqual(2, loadings.GetLength(1))
        AssertClose(3.0R, loadings(0, 0))
        AssertClose(0.0R, loadings(0, 1))
        AssertClose(0.0R, loadings(1, 1))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub DiagonalHelpers_UseExistingCoreDiagonalConstructor()
        Dim diagonal As Double() = {2.0R, 3.0R, 4.0R}
        Dim matrix As Double(,) = MultivariateShared.DiagonalMatrix(diagonal)
        Assert.AreEqual(3, matrix.GetLength(0))
        Assert.AreEqual(3, matrix.GetLength(1))
        For row As Integer = 0 To 2
            For column As Integer = 0 To 2
                AssertClose(If(row = column, diagonal(row), 0.0R), matrix(row, column))
            Next
        Next
        AssertArrayClose(diagonal, MultivariateShared.DiagonalValues(matrix))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub ClampAndMaxAbsDifference_MatchLegacyUtilitySemantics()
        AssertClose(0.0R, MultivariateShared.Clamp(-1.0R, 0.0R, 5.0R))
        AssertClose(3.0R, MultivariateShared.Clamp(3.0R, 0.0R, 5.0R))
        AssertClose(5.0R, MultivariateShared.Clamp(7.0R, 0.0R, 5.0R))
        AssertClose(4.0R, MultivariateShared.MaxAbsDifference(
            New Double() {1.0R, -1.0R}, New Double() {2.0R, 3.0R}))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Foundation")>
    Public Sub SafeInverse_CholeskyAndLuPaths_AgreeForPositiveDefiniteMatrix()
        Dim matrix As Double(,) = {{4.0R, 1.0R}, {1.0R, 3.0R}}
        Dim chol As Double(,) = MultivariateShared.SafeInverse(matrix, preferCholesky:=True)
        Dim lu As Double(,) = MultivariateShared.SafeInverse(matrix, preferCholesky:=False)

        AssertClose(3.0R / 11.0R, chol(0, 0))
        AssertClose(-1.0R / 11.0R, chol(0, 1))
        AssertClose(-1.0R / 11.0R, chol(1, 0))
        AssertClose(4.0R / 11.0R, chol(1, 1))
        For row As Integer = 0 To 1
            For column As Integer = 0 To 1
                AssertClose(chol(row, column), lu(row, column))
            Next
        Next
    End Sub

    Private Shared Sub AssertArrayClose(expected As Double(), actual As Double())
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i))
        Next
    End Sub

    Private Shared Sub AssertClose(expected As Double, actual As Double)
        Assert.AreEqual(expected, actual, Tol)
    End Sub
End Class
