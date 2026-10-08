Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Matrix

<TestClass>
Public Class MatrixCoreDecompositionTests

    Private Const Tol As Double = 0.0000001R

    <TestMethod>
    Public Sub Svd_SquareMatrix_ReconstructsInput()
        Dim a(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 3.0R}
        }

        Dim svd = MatrixDecompositionCore.DecomposeSvd(a)
        Dim reconstructed = ReconstructFromSvd(svd)

        AssertMatrixAlmostEqual(a, reconstructed, Tol)
        Assert.AreEqual(2, svd.U.GetLength(0))
        Assert.AreEqual(2, svd.U.GetLength(1))
        Assert.AreEqual(2, svd.SingularValues.Length)
        Assert.AreEqual(2, svd.V.GetLength(0))
        Assert.AreEqual(2, svd.V.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub Svd_TallMatrix_ReconstructsInputAndPreservesEconomyCompatibleDimensions()
        Dim a(,) As Double = {
            {1.0R, 0.0R},
            {0.0R, 1.0R},
            {1.0R, 1.0R}
        }

        Dim svd = MatrixDecompositionCore.DecomposeSvd(a)
        Dim reconstructed = ReconstructFromSvd(svd)

        AssertMatrixAlmostEqual(a, reconstructed, Tol)
        Assert.AreEqual(3, svd.U.GetLength(0))
        Assert.AreEqual(2, svd.U.GetLength(1))
        Assert.AreEqual(2, svd.SingularValues.Length)
        Assert.AreEqual(2, svd.SingularValueMatrix.GetLength(0))
        Assert.AreEqual(2, svd.SingularValueMatrix.GetLength(1))
        Assert.AreEqual(2, svd.V.GetLength(0))
        Assert.AreEqual(2, svd.V.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub Svd_WideMatrix_ReconstructsInputAndPreservesLegacyDimensions()
        Dim a(,) As Double = {
            {1.0R, 2.0R, 3.0R},
            {4.0R, 5.0R, 6.0R}
        }

        Dim svd = MatrixDecompositionCore.DecomposeSvd(a)
        Dim reconstructed = ReconstructFromSvd(svd)

        AssertMatrixAlmostEqual(a, reconstructed, Tol)
        Assert.AreEqual(2, svd.U.GetLength(0))
        Assert.AreEqual(3, svd.U.GetLength(1))
        Assert.AreEqual(3, svd.SingularValues.Length)
        Assert.AreEqual(3, svd.SingularValueMatrix.GetLength(0))
        Assert.AreEqual(3, svd.SingularValueMatrix.GetLength(1))
        Assert.AreEqual(3, svd.V.GetLength(0))
        Assert.AreEqual(3, svd.V.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub Svd_RankDeficientMatrix_ReconstructsAndContainsNearZeroSingularValue()
        Dim a(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 4.0R},
            {3.0R, 6.0R}
        }

        Dim svd = MatrixDecompositionCore.DecomposeSvd(a)
        AssertMatrixAlmostEqual(a, ReconstructFromSvd(svd), Tol)

        Dim minimumAbsoluteSingularValue As Double = Double.PositiveInfinity
        For Each value As Double In svd.SingularValues
            minimumAbsoluteSingularValue = Math.Min(minimumAbsoluteSingularValue, Math.Abs(value))
        Next

        Assert.IsTrue(minimumAbsoluteSingularValue < 0.0000000001R,
                      $"Expected a near-zero singular value, got {minimumAbsoluteSingularValue}.")
    End Sub

    <TestMethod>
    Public Sub PseudoInverse_RankDeficientMatrix_SatisfiesPenroseReconstructionProperty()
        Dim a(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 4.0R},
            {3.0R, 6.0R}
        }

        Dim aPlus = MatrixDecompositionCore.ComputePseudoInverse(a)
        Dim aAPlusA = MatrixArithmeticCore.Multiply(
            MatrixArithmeticCore.Multiply(a, aPlus), a)

        Assert.AreEqual(2, aPlus.GetLength(0))
        Assert.AreEqual(3, aPlus.GetLength(1))
        AssertMatrixAlmostEqual(a, aAPlusA, Tol)
    End Sub

    <TestMethod>
    Public Sub PseudoInverse_FullColumnRank_APlusTimesAIsIdentity()
        Dim a(,) As Double = {
            {1.0R, 0.0R},
            {0.0R, 1.0R},
            {1.0R, 1.0R}
        }

        Dim aPlus = MatrixDecompositionCore.ComputePseudoInverse(a)
        Dim aPlusA = MatrixArithmeticCore.Multiply(aPlus, a)

        AssertMatrixAlmostEqual(MatrixArithmeticCore.IdentityMatrix(1), aPlusA, Tol)
    End Sub

    <TestMethod>
    Public Sub PseudoInverse_WideMatrix_HasTransposedDimensionsAndSatisfiesPenroseProperty()
        Dim a(,) As Double = {
            {1.0R, 2.0R, 3.0R},
            {4.0R, 5.0R, 6.0R}
        }

        Dim aPlus = MatrixDecompositionCore.ComputePseudoInverse(a)
        Dim aAPlusA = MatrixArithmeticCore.Multiply(
            MatrixArithmeticCore.Multiply(a, aPlus), a)

        Assert.AreEqual(3, aPlus.GetLength(0))
        Assert.AreEqual(2, aPlus.GetLength(1))
        AssertMatrixAlmostEqual(a, aAPlusA, Tol)
    End Sub

    <TestMethod>
    Public Sub Qr_TallMatrix_ReturnsThinQAndSquareR_WithOrthonormalColumns()
        Dim a(,) As Double = {
            {1.0R, 0.0R},
            {0.0R, 1.0R},
            {1.0R, 1.0R},
            {2.0R, -1.0R}
        }

        Dim qr = MatrixDecompositionCore.DecomposeQr(a)

        Assert.AreEqual(4, qr.Q.GetLength(0))
        Assert.AreEqual(2, qr.Q.GetLength(1))
        Assert.AreEqual(2, qr.R.GetLength(0))
        Assert.AreEqual(2, qr.R.GetLength(1))
        AssertMatrixAlmostEqual(a, MatrixArithmeticCore.Multiply(qr.Q, qr.R), Tol)

        Dim qtq = MatrixArithmeticCore.Multiply(MatrixArithmeticCore.Transpose(qr.Q), qr.Q)
        AssertMatrixAlmostEqual(MatrixArithmeticCore.IdentityMatrix(1), qtq, Tol)
    End Sub

    <TestMethod>
    Public Sub Qr_SquareMatrix_ReturnsSquareQAndR()
        Dim a(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 3.0R}
        }

        Dim qr = MatrixDecompositionCore.DecomposeQr(a)

        Assert.AreEqual(2, qr.Q.GetLength(0))
        Assert.AreEqual(2, qr.Q.GetLength(1))
        Assert.AreEqual(2, qr.R.GetLength(0))
        Assert.AreEqual(2, qr.R.GetLength(1))
        AssertMatrixAlmostEqual(a, MatrixArithmeticCore.Multiply(qr.Q, qr.R), Tol)

        Dim qtq = MatrixArithmeticCore.Multiply(MatrixArithmeticCore.Transpose(qr.Q), qr.Q)
        AssertMatrixAlmostEqual(MatrixArithmeticCore.IdentityMatrix(1), qtq, Tol)
    End Sub

    <TestMethod>
    Public Sub Qr_WideMatrix_ThrowsDocumentedDimensionError()
        Dim wide(,) As Double = {
            {1.0R, 2.0R, 3.0R},
            {4.0R, 5.0R, 6.0R}
        }

        Dim ex = Assert.ThrowsException(Of ArgumentException)(
            Sub() MatrixDecompositionCore.DecomposeQr(wide))

        Assert.AreEqual("QRdecomp requires rows >= columns (m >= n).", ex.Message)
    End Sub

    <TestMethod>
    Public Sub QrSolve_SquareSystem_MatchesKnownSolution()
        Dim a(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 3.0R}
        }
        Dim b(,) As Double = {
            {1.0R},
            {2.0R}
        }

        Dim qr = MatrixDecompositionCore.DecomposeQr(a)
        Dim solution = MatrixDecompositionCore.SolveQr(qr, b)

        Assert.AreEqual(0.2R, solution(0, 0), Tol)
        Assert.AreEqual(0.6R, solution(1, 0), Tol)
    End Sub

    <TestMethod>
    Public Sub QrSolve_TallConsistentSystem_RecoversCoefficients()
        Dim a(,) As Double = {
            {1.0R, 0.0R},
            {0.0R, 1.0R},
            {1.0R, 1.0R}
        }
        Dim b(,) As Double = {
            {2.0R},
            {-1.0R},
            {1.0R}
        }

        Dim qr = MatrixDecompositionCore.DecomposeQr(a)
        Dim solution = MatrixDecompositionCore.SolveQr(qr, b)

        Assert.AreEqual(2.0R, solution(0, 0), Tol)
        Assert.AreEqual(-1.0R, solution(1, 0), Tol)
    End Sub

    <TestMethod>
    Public Sub Invert_Svd_FullRankMatrixProducesIdentity()
        Dim a(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 3.0R}
        }
        Dim errorCode As Integer = 0

        Dim inverse = MatrixDecompositionCore.InvertMatrix(a, "SVD", errorCode)
        Dim product = MatrixArithmeticCore.Multiply(a, inverse)

        Assert.AreEqual(0, errorCode)
        AssertMatrixAlmostEqual(MatrixArithmeticCore.IdentityMatrix(1), product, Tol)
    End Sub

    <TestMethod>
    Public Sub Invert_Svd_RankDeficientMatrixSetsErrorAndThrows()
        Dim a(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 4.0R}
        }
        Dim errorCode As Integer = 0

        Dim ex = Assert.ThrowsException(Of ApplicationException)(
            Sub() MatrixDecompositionCore.InvertMatrix(a, "SVD", errorCode))

        Assert.AreEqual(1, errorCode)
        StringAssert.Contains(ex.Message, "singular or numerically rank-deficient")
    End Sub

    <TestMethod>
    Public Sub Invert_LuAndCholesky_MatchKnownInverse()
        Dim a(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 3.0R}
        }
        Dim expected(,) As Double = {
            {0.6R, -0.2R},
            {-0.2R, 0.4R}
        }

        Dim luError As Integer = 0
        Dim cholError As Integer = 0

        AssertMatrixAlmostEqual(expected, MatrixDecompositionCore.InvertMatrix(a, "LU", luError), Tol)
        AssertMatrixAlmostEqual(expected, MatrixDecompositionCore.InvertMatrix(a, "CHOL", cholError), Tol)
        Assert.AreEqual(0, luError)
        Assert.AreEqual(0, cholError)
    End Sub

    Private Shared Function ReconstructFromSvd(svd As MatrixDecompositionCore.SvdResult) As Double(,)
        Return MatrixArithmeticCore.Multiply(
            MatrixArithmeticCore.Multiply(svd.U, svd.SingularValueMatrix),
            MatrixArithmeticCore.Transpose(svd.V))
    End Function

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double,
                                                actual(,) As Double,
                                                tolerance As Double)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count differs.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count differs.")

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tolerance, $"Mismatch at ({i},{j}).")
            Next
        Next
    End Sub

End Class
