Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Matrix

<TestClass>
Public Class MatrixCoreStatisticsTests

    Private Const Tol As Double = 0.000000001R

    <TestMethod>
    Public Sub SampleCovariance_MatchesKnownResult()
        Dim data(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 0.0R},
            {3.0R, 1.0R}
        }
        Dim expected(,) As Double = {
            {1.0R, -0.5R},
            {-0.5R, 1.0R}
        }

        AssertMatrixAlmostEqual(expected, MatrixStatisticsCore.SampleCovariance(data), Tol)
    End Sub

    <TestMethod>
    Public Sub SampleCovariance_ZeroRows_PreservesLegacyAverageFailure()
        Dim data As Double(,) = DirectCast(Array.CreateInstance(GetType(Double), 0, 2), Double(,))

        Assert.ThrowsException(Of InvalidOperationException)(
            Sub() MatrixStatisticsCore.SampleCovariance(data))
    End Sub

    <TestMethod>
    Public Sub SampleCovariance_OneObservation_PreservesLegacyZeroMatrixBehavior()
        Dim data(,) As Double = {{10.0R, -3.0R, 5.0R}}
        Dim covariance(,) As Double = MatrixStatisticsCore.SampleCovariance(data)

        Assert.AreEqual(3, covariance.GetLength(0))
        Assert.AreEqual(3, covariance.GetLength(1))
        AssertMatrixAlmostEqual(New Double(,) {
            {0.0R, 0.0R, 0.0R},
            {0.0R, 0.0R, 0.0R},
            {0.0R, 0.0R, 0.0R}
        }, covariance, 0.0R)
    End Sub

    <TestMethod>
    Public Sub DoubleCenter_MatchesDefinition()
        Dim matrix(,) As Double = {
            {1.0R, 2.0R},
            {4.0R, 8.0R}
        }
        Dim expected(,) As Double = {
            {0.75R, -0.75R},
            {-0.75R, 0.75R}
        }

        AssertMatrixAlmostEqual(expected, MatrixStatisticsCore.DoubleCenter(matrix), Tol)
    End Sub

    <TestMethod>
    Public Sub DoubleCenter_NonSquareMatrix_Throws()
        Dim matrix(,) As Double = {
            {1.0R, 2.0R, 3.0R},
            {4.0R, 5.0R, 6.0R}
        }

        Assert.ThrowsException(Of ArgumentException)(
            Sub() MatrixStatisticsCore.DoubleCenter(matrix))
    End Sub

    <TestMethod>
    Public Sub EigenJk_DiagonalMatrix_ReturnsKnownEigenvaluesAndOrthonormalVectors()
        Dim matrix(,) As Double = {
            {2.0R, 0.0R},
            {0.0R, 3.0R}
        }

        Dim result = MatrixStatisticsCore.EigenJk(matrix, 5, 0.000000000001R)
        Dim eigenvalues() As Double = DirectCast(result.Eigenvalues.Clone(), Double())
        Array.Sort(eigenvalues)

        Assert.AreEqual(2.0R, eigenvalues(0), Tol)
        Assert.AreEqual(3.0R, eigenvalues(1), Tol)

        Dim vtV(,) As Double = MatrixArithmeticCore.Multiply(
            MatrixArithmeticCore.Transpose(result.Eigenvectors),
            result.Eigenvectors)
        AssertMatrixAlmostEqual(MatrixArithmeticCore.IdentityMatrix(1), vtV, Tol)
    End Sub

    <TestMethod>
    Public Sub EigenJk_SymmetricMatrix_ReturnsKnownEigenvalues()
        Dim matrix(,) As Double = {
            {2.0R, 1.0R},
            {1.0R, 2.0R}
        }

        Dim result = MatrixStatisticsCore.EigenJk(matrix, 50, 0.000000000001R)
        Dim eigenvalues() As Double = DirectCast(result.Eigenvalues.Clone(), Double())
        Array.Sort(eigenvalues)

        Assert.AreEqual(1.0R, eigenvalues(0), 0.00000001R)
        Assert.AreEqual(3.0R, eigenvalues(1), 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub EigenJk_DoesNotMutateInput()
        Dim matrix(,) As Double = {
            {4.0R, 1.0R},
            {1.0R, 3.0R}
        }
        Dim original(,) As Double = DirectCast(matrix.Clone(), Double(,))

        MatrixStatisticsCore.EigenJk(matrix)

        AssertMatrixAlmostEqual(original, matrix, 0.0R)
    End Sub

    <TestMethod>
    Public Sub LinearRegression_WithIntercept_RecoversExactLine()
        Dim x(,) As Double = {
            {0.0R},
            {1.0R},
            {2.0R},
            {3.0R}
        }
        Dim y() As Double = {1.0R, 3.0R, 5.0R, 7.0R}

        Dim result(,) As Double = MatrixStatisticsCore.FitLinearRegression(y, x, True)

        Assert.AreEqual(2, result.GetLength(0))
        Assert.AreEqual(2, result.GetLength(1))
        Assert.AreEqual(1.0R, result(0, 0), 0.00000001R)
        Assert.AreEqual(2.0R, result(1, 0), 0.00000001R)
        Assert.IsTrue(result(0, 1) >= 0.0R)
        Assert.IsTrue(result(1, 1) >= 0.0R)
    End Sub

    <TestMethod>
    Public Sub LinearRegression_WithoutIntercept_RecoversKnownSlope()
        Dim x(,) As Double = {
            {1.0R},
            {2.0R},
            {3.0R}
        }
        Dim y() As Double = {2.0R, 4.0R, 6.0R}

        Dim result(,) As Double = MatrixStatisticsCore.FitLinearRegression(y, x, False)

        Assert.AreEqual(1, result.GetLength(0))
        Assert.AreEqual(2.0R, result(0, 0), 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub WeightedLeastSquares_ZeroWeight_RemovesObservationInfluence()
        Dim design(,) As Double = {
            {1.0R, 0.0R},
            {1.0R, 1.0R},
            {1.0R, 2.0R}
        }
        Dim y() As Double = {1.0R, 3.0R, 100.0R}
        Dim weights() As Double = {1.0R, 1.0R, 0.0R}

        Dim result(,) As Double = MatrixStatisticsCore.FitWeightedLeastSquares(y, design, weights)

        Assert.AreEqual(1.0R, result(0, 0), 0.00000001R)
        Assert.AreEqual(2.0R, result(1, 0), 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub WeightedLeastSquares_NegativeWeight_PreservesLegacyZeroWeightTreatment()
        Dim design(,) As Double = {
            {1.0R, 0.0R},
            {1.0R, 1.0R},
            {1.0R, 2.0R}
        }
        Dim y() As Double = {1.0R, 3.0R, -500.0R}
        Dim weights() As Double = {1.0R, 1.0R, -2.0R}

        Dim result(,) As Double = MatrixStatisticsCore.FitWeightedLeastSquares(y, design, weights)

        Assert.AreEqual(1.0R, result(0, 0), 0.00000001R)
        Assert.AreEqual(2.0R, result(1, 0), 0.00000001R)
    End Sub

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double,
                                               actual(,) As Double,
                                               tolerance As Double)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count mismatch.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count mismatch.")

        For row As Integer = 0 To expected.GetLength(0) - 1
            For column As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(row, column), actual(row, column), tolerance,
                                $"Mismatch at ({row}, {column}).")
            Next
        Next
    End Sub

End Class
