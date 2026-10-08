Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelCovarianceNumericalCoreTests

    Private Const Tol As Double = 0.000000001R

    <TestMethod>
    Public Sub BuildVi_NoRandomEffects_EqualsResidualCovariance()
        Dim data As MixedModelBlockData = CreateRandomInterceptData()
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)

        Dim vi As Double(,) = MixedModelCovariance.BuildVi(block,
                                                            data,
                                                            New NoRandomEffects(),
                                                            New IdentityR(),
                                                            Array.Empty(Of Double)(),
                                                            {Math.Log(0.5R)})

        AssertMatrixAlmostEqual({{0.5R, 0.0R},
                                 {0.0R, 0.5R}}, vi, Tol)
    End Sub

    <TestMethod>
    Public Sub BuildVi_RandomIntercept_AddsZiGZiTToResidualCovariance()
        Dim data As MixedModelBlockData = CreateRandomInterceptData()
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)

        Dim vi As Double(,) = MixedModelCovariance.BuildVi(block,
                                                            data,
                                                            New RandomIntercept(),
                                                            New IdentityR(),
                                                            {Math.Log(2.0R)},
                                                            {Math.Log(0.5R)})

        AssertMatrixAlmostEqual({{2.5R, 2.0R},
                                 {2.0R, 2.5R}}, vi, Tol)
    End Sub

    <TestMethod>
    Public Sub CholeskySolveInverseAndQuadraticForm_MatchKnownValues()
        Dim a(,) As Double = {{4.0R, 2.0R},
                              {2.0R, 3.0R}}
        Dim chol(,) As Double = Nothing

        Assert.IsTrue(MixedModelCovariance.TryCholesky(a, chol))
        Assert.IsNotNull(chol)
        Assert.AreEqual(Math.Log(8.0R), MixedModelCovariance.LogDetFromCholesky(chol), Tol)

        Dim solution() As Double = MixedModelCovariance.SolveSPDVector(a, {6.0R, 5.0R})
        AssertVectorAlmostEqual({1.0R, 1.0R}, solution, Tol)

        Dim inverse(,) As Double = MixedModelCovariance.InverseSPD(a)
        AssertMatrixAlmostEqual({{0.375R, -0.25R},
                                 {-0.25R, 0.5R}}, inverse, Tol)

        Dim q As Double = MixedModelCovariance.QuadraticForm(a, {6.0R, 5.0R})
        Assert.AreEqual(11.0R, q, Tol)
    End Sub

    <TestMethod>
    Public Sub TryCholeskyWithJitter_SingularMatrix_UsesPositiveJitter()
        Dim singular(,) As Double = {{1.0R, 1.0R},
                                     {1.0R, 1.0R}}
        Dim chol(,) As Double = Nothing
        Dim usedJitter As Double = Double.NaN

        Assert.IsTrue(MixedModelCovariance.TryCholeskyWithJitter(singular,
                                                                  chol,
                                                                  usedJitter,
                                                                  maxAttempts:=3,
                                                                  jitterStart:=0.000001R,
                                                                  jitterMultiplier:=10.0R))
        Assert.IsNotNull(chol)
        Assert.IsTrue(usedJitter > 0.0R)
    End Sub

    <TestMethod>
    Public Sub MarginalMeanResidualAndBlup_MatchDirectCalculation()
        Dim data As MixedModelBlockData = CreateRandomInterceptData()
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)
        Dim beta() As Double = {1.0R}

        Dim mean() As Double = MixedModelCovariance.ComputeMarginalMean(block, beta)
        Dim residual() As Double = MixedModelCovariance.ComputeMarginalResidual(block, beta)
        AssertVectorAlmostEqual({1.0R, 1.0R}, mean, Tol)
        AssertVectorAlmostEqual({0.0R, 2.0R}, residual, Tol)

        Dim g(,) As Double = {{2.0R}}
        Dim vi(,) As Double = {{2.5R, 2.0R},
                               {2.0R, 2.5R}}
        Dim vInv(,) As Double = MixedModelCovariance.InverseSPD(vi)
        Dim blup() As Double = MixedModelCovariance.ComputeBLUP(block, beta, g, vInv)

        Assert.IsNotNull(blup)
        Assert.AreEqual(1, blup.Length)
        Assert.AreEqual(0.88888888888888884R, blup(0), Tol)
    End Sub

    <TestMethod>
    Public Sub AccumulateProfileCrossProducts_MatchesDirectTwoObservationCalculation()
        Dim data As MixedModelBlockData = CreateRandomInterceptData()
        Dim block As MixedModelSubjectBlock = data.GetBlock(0)
        Dim vi(,) As Double = {{2.5R, 2.0R},
                               {2.0R, 2.5R}}
        Dim chol(,) As Double = MixedModelCovariance.CholeskyStrict(vi)

        Dim xtVinvX(,) As Double = Nothing
        Dim xtVinvY() As Double = Nothing
        Dim yVinvY As Double = 0.0R
        MixedModelCovariance.AccumulateProfileCrossProducts(block, chol, xtVinvX, xtVinvY, yVinvY)

        Assert.AreEqual(1, xtVinvX.GetLength(0))
        Assert.AreEqual(1, xtVinvX.GetLength(1))
        Assert.AreEqual(0.44444444444444442R, xtVinvX(0, 0), Tol)
        Assert.AreEqual(0.88888888888888884R, xtVinvY(0), Tol)
        Assert.AreEqual(5.7777777777777777R, yVinvY, Tol)
    End Sub

    <TestMethod>
    Public Sub TryInvertSymmetric_SpdMatrix_UsesCholesky()
        Dim a(,) As Double = {{4.0R, 2.0R},
                              {2.0R, 3.0R}}
        Dim inv(,) As Double = Nothing
        Dim diagnostic As String = Nothing
        Dim detail As New MixedModelNumericalInverseResult()

        Assert.IsTrue(MixedModelNumericalDiagnostics.TryInvertSymmetric(a,
                                                                        inv,
                                                                        diagnostic,
                                                                        inverseResult:=detail))
        Assert.IsTrue(detail.Success)
        Assert.AreEqual("Cholesky", detail.Method)
        Assert.IsFalse(detail.UsedPseudoInverse)
        Assert.AreEqual(2, detail.Rank)
        AssertMatrixAlmostEqual({{0.375R, -0.25R},
                                 {-0.25R, 0.5R}}, inv, Tol)
        StringAssert.Contains(diagnostic, "Inversion method=Cholesky")
    End Sub

    <TestMethod>
    Public Sub TryInvertSymmetric_RankDeficientMatrix_UsesSvdPseudoinverseWhenAllowed()
        Dim a(,) As Double = {{1.0R, 1.0R},
                              {1.0R, 1.0R}}
        Dim inv(,) As Double = Nothing
        Dim diagnostic As String = Nothing
        Dim detail As New MixedModelNumericalInverseResult()

        Assert.IsTrue(MixedModelNumericalDiagnostics.TryInvertSymmetric(a,
                                                                        inv,
                                                                        diagnostic,
                                                                        allowPseudoInverse:=True,
                                                                        inverseResult:=detail))
        Assert.IsTrue(detail.Success)
        Assert.IsTrue(detail.UsedPseudoInverse)
        Assert.AreEqual("SVD pseudoinverse", detail.Method)
        Assert.AreEqual(1, detail.Rank)
        AssertMatrixAlmostEqual({{0.25R, 0.25R},
                                 {0.25R, 0.25R}}, inv, 0.00000001R)
    End Sub

    <TestMethod>
    Public Sub TryInvertSymmetric_RankDeficientMatrix_FailsWhenPseudoinverseDisabled()
        Dim a(,) As Double = {{1.0R, 1.0R},
                              {1.0R, 1.0R}}
        Dim inv(,) As Double = Nothing
        Dim diagnostic As String = Nothing

        Assert.IsFalse(MixedModelNumericalDiagnostics.TryInvertSymmetric(a,
                                                                         inv,
                                                                         diagnostic,
                                                                         allowPseudoInverse:=False))
        Assert.IsNull(inv)
        StringAssert.Contains(diagnostic, "pseudoinverse fallback is disabled")
    End Sub

    <TestMethod>
    Public Sub SvdDiagnostics_ReportExpectedRankAndConditionNumber()
        Dim a(,) As Double = {{4.0R, 0.0R},
                              {0.0R, 2.0R}}

        Assert.AreEqual(2, MixedModelNumericalDiagnostics.NumericRankBySvd(a))
        Assert.AreEqual(2.0R, MixedModelNumericalDiagnostics.EstimateConditionNumberBySvd(a), Tol)

        Dim rankOne(,) As Double = {{1.0R, 1.0R},
                                    {1.0R, 1.0R}}
        Assert.AreEqual(1, MixedModelNumericalDiagnostics.NumericRankBySvd(rankOne))
    End Sub

    <TestMethod>
    Public Sub DiagnosticHelpers_BuildStableSignatureAndAvoidDuplicateWarnings()
        Dim a(,) As Double = {{1.0R, 2.0R},
                              {3.0R, 4.0R}}
        Assert.AreEqual("2x2:1;2;3;4", MixedModelNumericalDiagnostics.BuildMatrixSignature(a, 12))

        Dim warnings As New List(Of String)()
        MixedModelNumericalDiagnostics.AddUniqueWarning(warnings, "warning")
        MixedModelNumericalDiagnostics.AddUniqueWarning(warnings, "warning")
        Assert.AreEqual(1, warnings.Count)

        Dim warning As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("A", Double.PositiveInfinity)
        StringAssert.Contains(warning, "rank deficient")
    End Sub

    Private Shared Function CreateRandomInterceptData() As MixedModelBlockData
        Dim y() As Double = {1.0R, 3.0R, 2.0R, 4.0R}
        Dim x(3, 0) As Double
        Dim z(3, 0) As Double
        Dim subjectId() As Object = {"S1", "S1", "S2", "S2"}
        Dim visit() As Double = {0.0R, 1.0R, 0.0R, 1.0R}

        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0R
            z(i, 0) = 1.0R
        Next

        Return MixedModelBlockData.FromArrays(y:=y,
                                              x:=x,
                                              subjectId:=subjectId,
                                              z:=z,
                                              visit:=visit,
                                              sortWithinSubjectByVisit:=True)
    End Function

    Private Shared Sub AssertVectorAlmostEqual(expected() As Double, actual() As Double, tolerance As Double)
        Assert.IsNotNull(actual)
        Assert.AreEqual(expected.Length, actual.Length)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tolerance, $"Mismatch at index {i}.")
        Next
    End Sub

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double, actual(,) As Double, tolerance As Double)
        Assert.IsNotNull(actual)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "Row count differs.")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "Column count differs.")

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tolerance, $"Mismatch at ({i},{j}).")
            Next
        Next
    End Sub

End Class
