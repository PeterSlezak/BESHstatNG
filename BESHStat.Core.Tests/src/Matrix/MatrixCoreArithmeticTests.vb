Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Matrix

<TestClass>
Public Class MatrixCoreArithmeticTests

    Private Const Tol As Double = 0.0000000001R

    <TestMethod>
    Public Sub OuterProduct_Double_MatchesKnownResult()
        Dim a() As Double = {1.5R, -2.0R}
        Dim b() As Double = {3.0R, 4.0R, -1.0R}
        Dim expected(,) As Double = {
            {4.5R, 6.0R, -1.5R},
            {-6.0R, -8.0R, 2.0R}
        }

        AssertMatrixAlmostEqual(expected, MatrixArithmeticCore.OuterProduct(a, b), 0.0R)
    End Sub

    <TestMethod>
    Public Sub MatrixMultiply_MatrixMatrix_MatchesKnownResult()
        Dim a(,) As Double = {{2.0R, 1.0R}, {1.0R, 3.0R}}
        Dim b(,) As Double = {{1.0R, 2.0R}, {3.0R, 4.0R}}
        Dim expected(,) As Double = {{5.0R, 8.0R}, {10.0R, 14.0R}}

        AssertMatrixAlmostEqual(expected, MatrixArithmeticCore.Multiply(a, b), 0.0R)
    End Sub

    <TestMethod>
    Public Sub MatrixMultiply_VectorAndMatrix_OverloadsMatchKnownResults()
        Dim v() As Double = {1.0R, 2.0R}
        Dim a(,) As Double = {{2.0R, 1.0R}, {1.0R, 3.0R}}

        AssertMatrixAlmostEqual({{4.0R, 7.0R}}, MatrixArithmeticCore.Multiply(v, a), 0.0R)
        AssertMatrixAlmostEqual({{4.0R}, {7.0R}}, MatrixArithmeticCore.Multiply(a, v), 0.0R)
    End Sub

    <TestMethod>
    Public Sub AddSubtractDivide_MatrixMatrix_MatchLegacySemantics()
        Dim a(,) As Double = {{1.0R, 2.0R}, {3.0R, 4.0R}}
        Dim b(,) As Double = {{10.0R, 20.0R}, {30.0R, 40.0R}}

        AssertMatrixAlmostEqual({{11.0R, 22.0R}, {33.0R, 44.0R}}, MatrixArithmeticCore.Add(a, b), 0.0R)
        AssertMatrixAlmostEqual({{9.0R, 18.0R}, {27.0R, 36.0R}}, MatrixArithmeticCore.Subtract(b, a), 0.0R)

        Dim trace As String = String.Empty
        AssertMatrixAlmostEqual({{10.0R, 10.0R}, {10.0R, 10.0R}}, MatrixArithmeticCore.Divide(b, a, trace), 0.0R)
        Assert.AreEqual(String.Empty, trace)
    End Sub

    <TestMethod>
    Public Sub DotProduct_AndTranspose_MatchKnownResults()
        Assert.AreEqual(3.5R,
                        MatrixArithmeticCore.DotProduct({1.0R, 2.0R, 3.0R}, {4.0R, -1.0R, 0.5R}),
                        0.0R)

        Dim matrix(,) As Double = {{1.0R, 2.0R, 3.0R}, {4.0R, 5.0R, 6.0R}}
        AssertMatrixAlmostEqual({{1.0R, 4.0R}, {2.0R, 5.0R}, {3.0R, 6.0R}},
                                MatrixArithmeticCore.Transpose(matrix),
                                0.0R)
    End Sub

    <TestMethod>
    Public Sub IdentityAndConstantVector_PreserveHighestIndexContract()
        AssertMatrixAlmostEqual({{1.0R, 0.0R}, {0.0R, 1.0R}}, MatrixArithmeticCore.IdentityMatrix(1), 0.0R)
        CollectionAssert.AreEqual(New Double() {5.0R, 5.0R, 5.0R}, MatrixArithmeticCore.ConstantVector(2, 5.0R))
    End Sub

    <TestMethod>
    Public Sub Multiply_DimensionMismatch_ThrowsLegacyArgumentException()
        Dim left(,) As Double = {{1.0R, 2.0R, 3.0R}}
        Dim right(,) As Double = {{1.0R, 2.0R}, {3.0R, 4.0R}}

        Dim ex = Assert.ThrowsException(Of ArgumentException)(
            Sub() MatrixArithmeticCore.Multiply(left, right))

        StringAssert.Contains(ex.Message, "Inapropriate matrix dimensions in input matrix.")
    End Sub

    <TestMethod>
    Public Sub DotProduct_DimensionMismatch_ThrowsLegacyApplicationException()
        Dim ex = Assert.ThrowsException(Of ApplicationException)(
            Sub() MatrixArithmeticCore.DotProduct({1.0R, 2.0R}, {1.0R}))

        Assert.AreEqual("DotProduct vectors must have the same length.", ex.Message)
    End Sub

    <TestMethod>
    Public Sub MatrixVectorMultiply_DimensionMismatch_ThrowsLegacyApplicationException()
        Dim matrix(,) As Double = {{1.0R, 2.0R}}

        Dim ex = Assert.ThrowsException(Of ApplicationException)(
            Sub() MatrixArithmeticCore.MultiplyVector(matrix, {1.0R}))

        StringAssert.Contains(ex.Message, "matrix columns=2, vector length=1")
    End Sub

    <TestMethod>
    Public Sub MatrixAndVectorFinitePredicates_DetectNaNAndInfinity()
        Assert.IsTrue(MatrixArithmeticCore.MatrixIsFinite({{1.0R, 2.0R}, {3.0R, 4.0R}}))
        Assert.IsFalse(MatrixArithmeticCore.MatrixIsFinite({{1.0R, Double.NaN}}))
        Assert.IsFalse(MatrixArithmeticCore.MatrixIsFinite({{Double.PositiveInfinity}}))

        Assert.IsTrue(MatrixArithmeticCore.VectorIsFinite({1.0R, -2.0R}))
        Assert.IsFalse(MatrixArithmeticCore.VectorIsFinite({1.0R, Double.NegativeInfinity}))
    End Sub

    <TestMethod>
    Public Sub MatrixIsFiniteAndSymmetric_ReportsNonFiniteBeforeSymmetryFailure()
        Dim message As String = String.Empty
        Dim matrix(,) As Double = {{1.0R, Double.NaN}, {2.0R, 3.0R}}

        Assert.IsFalse(MatrixArithmeticCore.MatrixIsFiniteAndSymmetric(matrix, 0.000001R, message))
        StringAssert.Contains(message, "non-finite value")
    End Sub

    <TestMethod>
    Public Sub Divide_ByZero_PreservesZeroOutputAndTraceWarning()
        Dim trace As String = String.Empty
        Dim actual = MatrixArithmeticCore.Divide({10.0R, 20.0R}, {2.0R, 0.0R}, trace)

        CollectionAssert.AreEqual(New Double() {5.0R, 0.0R}, actual)
        StringAssert.Contains(trace, "WARNING: M_DIV Division by zero")
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
