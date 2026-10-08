Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Matrix

<TestClass>
Public Class MatrixCoreFactorizationTests

    Private Const Tol As Double = 0.0000000001R

    <TestMethod>
    Public Sub Cholesky_PositiveDefinite_ReconstructsAndSolves()
        Dim a(,) As Double = {{4.0R, 2.0R}, {2.0R, 3.0R}}
        Dim fault As Integer = 0

        Dim lower = MatrixFactorizationCore.Cholesky(a, fault, True)
        Assert.AreEqual(0, fault)

        Dim reconstructed = MatrixArithmeticCore.Multiply(lower, MatrixArithmeticCore.Transpose(lower))
        AssertMatrixAlmostEqual(a, reconstructed, Tol)

        Dim x = MatrixFactorizationCore.CholeskySolve(lower, {6.0R, 5.0R})
        CollectionAssert.AreEqual(New Double() {1.0R, 1.0R}, RoundVector(x, 10))
    End Sub

    <TestMethod>
    Public Sub Cholesky_NotPositiveDefinite_ErrorRaiseFalse_SetsFaultAndReturnsPartialFactor()
        Dim a(,) As Double = {{1.0R, 2.0R}, {2.0R, 1.0R}}
        Dim fault As Integer = 0

        Dim lower = MatrixFactorizationCore.Cholesky(a, fault, False)

        Assert.AreEqual(2, fault)
        Assert.AreEqual(1.0R, lower(0, 0), 0.0R)
        Assert.AreEqual(2.0R, lower(1, 0), 0.0R)
        Assert.AreEqual(-3.0R, lower(1, 1), 0.0R)
    End Sub

    <TestMethod>
    Public Sub Cholesky_NotPositiveDefinite_ErrorRaiseTrue_Throws()
        Dim a(,) As Double = {{1.0R, 2.0R}, {2.0R, 1.0R}}
        Dim fault As Integer = 0

        Dim ex = Assert.ThrowsException(Of ApplicationException)(
            Sub() MatrixFactorizationCore.Cholesky(a, fault, True))

        Assert.AreEqual(2, fault)
        StringAssert.Contains(ex.Message, "MatrixType not positive-definite.")
    End Sub

    <TestMethod>
    Public Sub CholeskyInverse_MatchesKnownInverse()
        Dim a(,) As Double = {{2.0R, 1.0R}, {1.0R, 3.0R}}
        Dim fault As Integer = 0
        Dim lower = MatrixFactorizationCore.Cholesky(a, fault, True)
        Dim inverse = MatrixFactorizationCore.CholeskyInverse(lower)

        AssertMatrixAlmostEqual({{0.6R, -0.2R}, {-0.2R, 0.4R}}, inverse, Tol)
    End Sub

    <TestMethod>
    Public Sub LUDecompose_AndSolve_MatchKnownSolution_WithoutMutatingInputMatrix()
        Dim a(,) As Double = {{2.0R, 1.0R}, {1.0R, 3.0R}}
        Dim original = DirectCast(a.Clone(), Double(,))
        Dim parity As Double = 1.0R
        Dim errorCode As Integer = 0

        Dim lu = MatrixFactorizationCore.LUDecompose(a, parity, errorCode)
        Assert.AreEqual(0, errorCode)
        AssertMatrixAlmostEqual(original, a, 0.0R)

        Dim rhs() As Double = {5.0R, 7.0R}
        Dim solution = MatrixFactorizationCore.LUSolve(lu, rhs)
        AssertVectorAlmostEqual({1.6R, 1.8R}, solution, Tol)
    End Sub

    <TestMethod>
    Public Sub LUDecompose_ZeroRow_SetsErrorCodeAndThrowsSingularMatrix()
        Dim a(,) As Double = {{1.0R, 2.0R}, {0.0R, 0.0R}}
        Dim parity As Double = 1.0R
        Dim errorCode As Integer = 0

        Dim ex = Assert.ThrowsException(Of ApplicationException)(
            Sub() MatrixFactorizationCore.LUDecompose(a, parity, errorCode))

        Assert.AreEqual(2, errorCode)
        Assert.AreEqual("Singular matrix.", ex.Message)
    End Sub

    <TestMethod>
    Public Sub LUDecompose_LaterZeroPivot_PreservesTinyPivotBehavior()
        Dim a(,) As Double = {{1.0R, 2.0R}, {2.0R, 4.0R}}
        Dim parity As Double = 1.0R
        Dim errorCode As Integer = 0

        Dim lu = MatrixFactorizationCore.LUDecompose(a, parity, errorCode)

        Assert.AreEqual(0, errorCode)
        Assert.AreEqual(0.000000000000002R, lu.Factors(1, 1), 0.0R)
    End Sub

    <TestMethod>
    Public Sub Determinant_RowSwap_HasNegativeParity()
        Dim a(,) As Double = {{0.0R, 1.0R}, {1.0R, 0.0R}}
        Assert.AreEqual(-1.0R, MatrixFactorizationCore.Determinant(a), Tol)
    End Sub

    <TestMethod>
    Public Sub Determinant_NonSquare_ReturnsNaN()
        Dim a(,) As Double = {{1.0R, 2.0R, 3.0R}, {4.0R, 5.0R, 6.0R}}
        Assert.IsTrue(Double.IsNaN(MatrixFactorizationCore.Determinant(a)))
    End Sub

    <TestMethod>
    Public Sub Determinant_SingularMatrix_RemainsNumericallyZeroAtLegacyTolerance()
        Dim a(,) As Double = {{1.0R, 2.0R}, {2.0R, 4.0R}}
        Assert.AreEqual(0.0R, MatrixFactorizationCore.Determinant(a), 0.000000000001R)
    End Sub

    Private Shared Function RoundVector(values() As Double, digits As Integer) As Double()
        Dim output(values.Length - 1) As Double
        For i As Integer = 0 To values.Length - 1
            output(i) = Math.Round(values(i), digits)
        Next
        Return output
    End Function

    Private Shared Sub AssertVectorAlmostEqual(expected() As Double, actual() As Double, tolerance As Double)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tolerance, $"Mismatch at index {i}.")
        Next
    End Sub

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double, actual(,) As Double, tolerance As Double)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count differs.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count differs.")

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tolerance, $"Mismatch at ({i},{j}).")
            Next
        Next
    End Sub

End Class
