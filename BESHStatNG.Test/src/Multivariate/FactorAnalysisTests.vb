Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Multivariate

<TestClass>
Public Class FactorAnalysisTests

    Private Shared Function BuildFactorDataset(Optional includeMissing As Boolean = False) As (data As Double(,), rowIds As Integer(), varNames As String())
        Const n As Integer = 36
        Const p As Integer = 6

        Dim data(n - 1, p - 1) As Double
        Dim rowIds(n - 1) As Integer
        Dim varNames() As String = {"X1", "X2", "X3", "X4", "X5", "X6"}

        For i As Integer = 0 To n - 1
            rowIds(i) = i + 1

            Dim trend As Double = (i - 17.5) / 6.0
            Dim wave As Double = Math.Sin((i + 1) * 0.55) + ((i Mod 5) - 2) / 3.0
            Dim f1 As Double = trend + 0.15 * Math.Cos(i * 0.3)
            Dim f2 As Double = 0.35 * f1 + 0.9 * wave

            Dim e1 As Double = (((i * 5) Mod 7) - 3) / 40.0
            Dim e2 As Double = (((i * 3) Mod 11) - 5) / 45.0
            Dim e3 As Double = (((i * 4) Mod 9) - 4) / 42.0
            Dim e4 As Double = (((i * 6) Mod 10) - 5) / 38.0
            Dim e5 As Double = (((i * 2) Mod 8) - 4) / 44.0
            Dim e6 As Double = (((i * 7) Mod 12) - 6) / 48.0

            data(i, 0) = 0.88 * f1 + 0.1 * f2 + e1
            data(i, 1) = 0.78 * f1 - 0.12 * f2 + e2
            data(i, 2) = 0.18 * f1 + 0.86 * f2 + e3
            data(i, 3) = -0.08 * f1 + 0.76 * f2 + e4
            data(i, 4) = 0.58 * f1 + 0.46 * f2 + e5
            data(i, 5) = 0.52 * f1 - 0.4 * f2 + e6
        Next

        If includeMissing Then
            data(3, 1) = Double.NaN
            data(20, 3) = Double.NaN
        End If

        Return (data, rowIds, varNames)
    End Function

    Private Shared Function BuildFactorAnalysis(loaded As (data As Double(,), rowIds As Integer(), varNames As String()),
                                                extraction As FactorAnalysisExtractionMethod,
                                                Optional rotation As FactorAnalysisRotationMethod = FactorAnalysisRotationMethod.None,
                                                Optional scores As FactorAnalysisScoreMethod = FactorAnalysisScoreMethod.None,
                                                Optional missingPolicy As FactorAnalysisMissingValuePolicy = FactorAnalysisMissingValuePolicy.ErrorOnMissing,
                                                Optional retentionMethod As FactorAnalysisRetentionMethod = FactorAnalysisRetentionMethod.Fixed,
                                                Optional retentionValue As Double = 2.0,
                                                Optional matrixType As FactorAnalysisMatrixType = FactorAnalysisMatrixType.Correlation) As FactorAnalysis
        Dim fa As New FactorAnalysis()
        fa.dataInputs(loaded.data, loaded.rowIds, loaded.varNames, "Unit Test")
        fa.settingsInputs(maximumIteration:=600,
                          dEps:=0.0000001,
                          analyzedMatrixType:=matrixType,
                          extractionMethod:=extraction,
                          retentionMethod:=retentionMethod,
                          retentionValue:=retentionValue,
                          rotationMethod:=rotation,
                          scoreMethod:=scores,
                          communalityInitialization:=FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation,
                          missingValuePolicy:=missingPolicy,
                          useKaiserNormalization:=True,
                          promaxPower:=4.0)
        Return fa
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Dim diff As Double = Math.Abs(expected - actual)
        Dim ok As Boolean = diff <= absTol
        If Not ok AndAlso relTol > 0 Then
            Dim denom As Double = Math.Max(Math.Abs(expected), Math.Abs(actual))
            If denom > 0 Then ok = (diff / denom) <= relTol
        End If
        If Not ok Then
            Assert.Fail($"{msg} Expected {expected:R}, got {actual:R}, diff={diff:R}")
        End If
    End Sub

    Private Shared Sub AssertMatrixClose(expected(,) As Double, actual(,) As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), $"{msg} Row count mismatch")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), $"{msg} Column count mismatch")
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                AssertClose(expected(i, j), actual(i, j), absTol, relTol, $"{msg} [i={i},j={j}]")
            Next
        Next
    End Sub

    Private Shared Function Identity(n As Integer) As Double(,)
        Dim eye(n - 1, n - 1) As Double
        For i As Integer = 0 To n - 1
            eye(i, i) = 1.0
        Next
        Return eye
    End Function

    Private Shared Function AddMatrices(a(,) As Double, b(,) As Double) As Double(,)
        Dim out(a.GetLength(0) - 1, a.GetLength(1) - 1) As Double
        For i As Integer = 0 To a.GetLength(0) - 1
            For j As Integer = 0 To a.GetLength(1) - 1
                out(i, j) = a(i, j) + b(i, j)
            Next
        Next
        Return out
    End Function

    Private Shared Sub AssertFiniteVector(values() As Double, Optional msg As String = "")
        Assert.IsNotNull(values, msg & " Vector should not be null.")
        For i As Integer = 0 To values.Length - 1
            If Double.IsNaN(values(i)) OrElse Double.IsInfinity(values(i)) Then
                Assert.Fail($"{msg} Non-finite value at index {i}: {values(i)}")
            End If
        Next
    End Sub

    Private Shared Sub AssertFiniteMatrix(values(,) As Double, Optional msg As String = "")
        Assert.IsNotNull(values, msg & " Matrix should not be null.")
        For i As Integer = 0 To values.GetLength(0) - 1
            For j As Integer = 0 To values.GetLength(1) - 1
                If Double.IsNaN(values(i, j)) OrElse Double.IsInfinity(values(i, j)) Then
                    Assert.Fail($"{msg} Non-finite value at [{i},{j}]: {values(i, j)}")
                End If
            Next
        Next
    End Sub

    Private Shared Function GetTitleText(tbl As ResultTable) As String
        Dim m(,) As Object = tbl.returnSelf()
        If m Is Nothing Then Return String.Empty
        If m.GetLength(0) = 0 OrElse m.GetLength(1) = 0 Then Return String.Empty
        Return If(m(0, 0), String.Empty).ToString()
    End Function

    Private Shared Function FindTableByTitle(tables As List(Of ResultTable), exactTitle As String) As ResultTable
        For Each tbl As ResultTable In tables
            If String.Equals(GetTitleText(tbl), exactTitle, StringComparison.OrdinalIgnoreCase) Then
                Return tbl
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function FindTableByTitlePrefix(tables As List(Of ResultTable), prefix As String) As ResultTable
        For Each tbl As ResultTable In tables
            If GetTitleText(tbl).StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                Return tbl
            End If
        Next
        Return Nothing
    End Function

    <TestMethod>
    Public Sub FactorAnalysis_PrincipalAxis_Varimax_ComputesConsistentCorrelationScaleSolution()
        Dim loaded = BuildFactorDataset()
        Dim fa = BuildFactorAnalysis(loaded,
                                     extraction:=FactorAnalysisExtractionMethod.PrincipalAxis,
                                     rotation:=FactorAnalysisRotationMethod.Varimax,
                                     scores:=FactorAnalysisScoreMethod.Regression)

        fa.Calculate()

        Assert.AreEqual(2, fa.NumberOfFactors)
        Assert.AreEqual(loaded.data.GetLength(1), fa.PatternMatrix.GetLength(0))
        Assert.AreEqual(2, fa.PatternMatrix.GetLength(1))
        Assert.AreEqual(loaded.data.GetLength(1), fa.UnrotatedLoadings.GetLength(0))
        Assert.AreEqual(2, fa.UnrotatedLoadings.GetLength(1))
        Assert.AreEqual(loaded.data.GetLength(0), fa.Scores.GetLength(0))
        Assert.AreEqual(2, fa.Scores.GetLength(1))

        AssertFiniteMatrix(fa.WorkingMatrix, "Working matrix")
        AssertFiniteMatrix(fa.PatternMatrix, "Pattern matrix")
        AssertFiniteMatrix(fa.StructureMatrix, "Structure matrix")
        AssertFiniteMatrix(fa.ReproducedMatrix, "Reproduced matrix")
        AssertFiniteMatrix(fa.ResidualMatrix, "Residual matrix")
        AssertFiniteMatrix(fa.ScoreCoefficientMatrix, "Score coefficients")
        AssertFiniteMatrix(fa.Scores, "Factor scores")
        AssertFiniteVector(fa.Communalities, "Communalities")
        AssertFiniteVector(fa.Uniquenesses, "Uniquenesses")

        For i As Integer = 0 To fa.Communalities.Length - 1
            Assert.IsTrue(fa.Communalities(i) >= -1.0E-8, $"Communality should be non-negative for variable {i + 1}.")
            Assert.IsTrue(fa.Communalities(i) <= 1.000001, $"Communality should not exceed 1 on the correlation scale for variable {i + 1}.")
            AssertClose(1.0 - fa.Communalities(i), fa.Uniquenesses(i), 1.0E-6, 0.0, $"Uniqueness identity for variable {i + 1}")
        Next

        Dim contrib = fa.CommunalityContributionsByFactor()
        Assert.AreEqual(loaded.data.GetLength(1), contrib.GetLength(0))
        Assert.AreEqual(2, contrib.GetLength(1))
        For i As Integer = 0 To contrib.GetLength(0) - 1
            Dim rowSum As Double = 0.0
            For j As Integer = 0 To contrib.GetLength(1) - 1
                rowSum += contrib(i, j)
            Next
            AssertClose(fa.Communalities(i), rowSum, 1.0E-6, 0.0, $"Communality contributions should sum to extracted communality for variable {i + 1}")
        Next

        AssertMatrixClose(fa.WorkingMatrix,
                          AddMatrices(fa.ReproducedMatrix, fa.ResidualMatrix),
                          1.0E-6,
                          0.0,
                          "Working matrix should equal reproduced + residual")

        AssertMatrixClose(Identity(2), fa.FactorCorrelationMatrix, 1.0E-6, 0.0, "Varimax should remain orthogonal")
        Assert.IsTrue(fa.KmoOverall >= 0.0 AndAlso fa.KmoOverall <= 1.0, "KMO must lie in [0,1].")
        Assert.IsTrue(fa.RMSR >= 0.0, "RMSR should be non-negative.")
        Assert.IsTrue(fa.BartlettDegreesOfFreedom > 0, "Bartlett df should be positive.")
    End Sub

    <TestMethod>
    Public Sub FactorAnalysis_ListwiseDeletion_RemovesRowsAndKeepsScoresAligned()
        Dim loaded = BuildFactorDataset(includeMissing:=True)
        Dim fa = BuildFactorAnalysis(loaded,
                                     extraction:=FactorAnalysisExtractionMethod.PrincipalAxis,
                                     rotation:=FactorAnalysisRotationMethod.None,
                                     scores:=FactorAnalysisScoreMethod.Regression,
                                     missingPolicy:=FactorAnalysisMissingValuePolicy.ListwiseDeletion)

        fa.Calculate()

        Assert.AreEqual(2, fa.RemovedRowCount)
        Assert.AreEqual(loaded.data.GetLength(0) - 2, fa.AnalysisData.GetLength(0))
        Assert.AreEqual(loaded.data.GetLength(0) - 2, fa.AnalysisRowIds.Length)
        Assert.AreEqual(loaded.data.GetLength(0) - 2, fa.Scores.GetLength(0))
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36}, fa.AnalysisRowIds)
    End Sub

    <TestMethod>
    Public Sub FactorAnalysis_ErrorOnMissing_ThrowsArgumentException()
        Dim loaded = BuildFactorDataset(includeMissing:=True)
        Dim fa = BuildFactorAnalysis(loaded,
                                     extraction:=FactorAnalysisExtractionMethod.PrincipalAxis,
                                     missingPolicy:=FactorAnalysisMissingValuePolicy.ErrorOnMissing)

        Assert.ThrowsException(Of ArgumentException)(Sub() fa.Calculate())
    End Sub

    <TestMethod>
    Public Sub FactorAnalysis_AdvancedExtractionFamilies_ProduceFiniteTwoFactorSolutions()
        Dim loaded = BuildFactorDataset()
        Dim methods = New FactorAnalysisExtractionMethod() {
            FactorAnalysisExtractionMethod.MaximumLikelihood,
            FactorAnalysisExtractionMethod.GeneralizedLeastSquares,
            FactorAnalysisExtractionMethod.Image,
            FactorAnalysisExtractionMethod.Alpha
        }

        For Each method As FactorAnalysisExtractionMethod In methods
            Dim fa = BuildFactorAnalysis(loaded,
                                         extraction:=method,
                                         rotation:=FactorAnalysisRotationMethod.None,
                                         scores:=FactorAnalysisScoreMethod.None)

            Try
                fa.Calculate()
            Catch ex As Exception
                Assert.Fail($"Advanced extraction method {method} threw an exception: {ex.Message}")
            End Try

            Assert.AreEqual(2, fa.NumberOfFactors, $"Unexpected factor count for {method}")
            Assert.AreEqual(loaded.data.GetLength(1), fa.PatternMatrix.GetLength(0), $"Pattern row count mismatch for {method}")
            Assert.AreEqual(2, fa.PatternMatrix.GetLength(1), $"Pattern column count mismatch for {method}")
            Assert.AreEqual(2, fa.ExtractionSumsOfSquares.Length, $"Extraction SS length mismatch for {method}")
            Assert.AreEqual(2, fa.FactorCorrelationMatrix.GetLength(0), $"Phi row count mismatch for {method}")
            Assert.AreEqual(2, fa.FactorCorrelationMatrix.GetLength(1), $"Phi column count mismatch for {method}")
            Assert.IsTrue(fa.ExtractionIterations >= 1, $"Extraction iterations should be >= 1 for {method}")

            AssertFiniteMatrix(fa.UnrotatedLoadings, $"Unrotated loadings for {method}")
            AssertFiniteMatrix(fa.PatternMatrix, $"Pattern matrix for {method}")
            AssertFiniteMatrix(fa.StructureMatrix, $"Structure matrix for {method}")
            AssertFiniteMatrix(fa.ReproducedMatrix, $"Reproduced matrix for {method}")
            AssertFiniteMatrix(fa.ResidualMatrix, $"Residual matrix for {method}")
            AssertFiniteVector(fa.Communalities, $"Communalities for {method}")
            AssertFiniteVector(fa.Uniquenesses, $"Uniquenesses for {method}")
            AssertFiniteVector(fa.ExtractionSumsOfSquares, $"Extraction SS for {method}")

            For i As Integer = 0 To fa.Communalities.Length - 1
                Assert.IsTrue(fa.Communalities(i) >= -1.0E-6, $"Communality should be non-negative for {method}, variable {i + 1}.")
                Assert.IsTrue(fa.Uniquenesses(i) >= -1.0E-6, $"Uniqueness should be non-negative for {method}, variable {i + 1}.")
            Next

            AssertMatrixClose(fa.WorkingMatrix,
                              AddMatrices(fa.ReproducedMatrix, fa.ResidualMatrix),
                              1.0E-6,
                              0.0,
                              $"Working matrix decomposition for {method}")
        Next
    End Sub

    <TestMethod>
    Public Sub FactorAnalysis_Promax_CommunalityContributionsRemainConsistentForObliqueSolution()
        Dim loaded = BuildFactorDataset()
        Dim fa = BuildFactorAnalysis(loaded,
                                     extraction:=FactorAnalysisExtractionMethod.PrincipalAxis,
                                     rotation:=FactorAnalysisRotationMethod.Promax,
                                     scores:=FactorAnalysisScoreMethod.None)

        fa.Calculate()

        Dim phi = fa.FactorCorrelationMatrix
        Assert.AreEqual(2, phi.GetLength(0))
        Assert.AreEqual(2, phi.GetLength(1))
        AssertClose(1.0, phi(0, 0), 1.0E-6)
        AssertClose(1.0, phi(1, 1), 1.0E-6)
        AssertClose(phi(0, 1), phi(1, 0), 1.0E-6)

        Dim contrib = fa.CommunalityContributionsByFactor()
        For i As Integer = 0 To contrib.GetLength(0) - 1
            Dim rowSum As Double = 0.0
            For j As Integer = 0 To contrib.GetLength(1) - 1
                rowSum += contrib(i, j)
            Next
            AssertClose(fa.Communalities(i), rowSum, 1.0E-6, 0.0, $"Promax communality decomposition for variable {i + 1}")
        Next
    End Sub

    <TestMethod>
    Public Sub FactorAnalysis_WrapResults_ExposeCommunalityContributionsAndScoreTables()
        Dim loaded = BuildFactorDataset()
        Dim fa = BuildFactorAnalysis(loaded,
                                     extraction:=FactorAnalysisExtractionMethod.MaximumLikelihood,
                                     rotation:=FactorAnalysisRotationMethod.Varimax,
                                     scores:=FactorAnalysisScoreMethod.Regression)

        fa.Calculate()
        Dim tables = fa.wrapResults()

        Dim titles = tables.Select(Function(t) GetTitleText(t)).ToList()
        CollectionAssert.Contains(titles, "Factor Analysis Summary - Unit Test")
        CollectionAssert.Contains(titles, "Communalities")
        CollectionAssert.Contains(titles, "Unrotated Loadings")
        CollectionAssert.Contains(titles, "Factor Correlation Matrix")
        CollectionAssert.Contains(titles, "Rotation Transformation Matrix")
        CollectionAssert.Contains(titles, "Reproduced Matrix")
        CollectionAssert.Contains(titles, "Residual Matrix")
        Assert.IsTrue(titles.Any(Function(x) x.StartsWith("Rotated Pattern Matrix (Varimax", StringComparison.OrdinalIgnoreCase)))
        Assert.IsTrue(titles.Any(Function(x) x.StartsWith("Structure Matrix (Varimax", StringComparison.OrdinalIgnoreCase)))
        Assert.IsTrue(titles.Any(Function(x) x.StartsWith("Factor Score Coefficients (Regression)", StringComparison.OrdinalIgnoreCase)))
        Assert.IsTrue(titles.Any(Function(x) x.StartsWith("Factor Scores (Regression)", StringComparison.OrdinalIgnoreCase)))

        Dim communalityTable = FindTableByTitle(tables, "Communalities")
        Assert.IsNotNull(communalityTable, "Communalities table should be present.")
        Assert.AreEqual(6, communalityTable.TotalCols, "Communalities table should expose Variable, Initial, two factor contributions, Extracted, and Uniqueness columns.")

        Dim scoreTable = FindTableByTitlePrefix(tables, "Factor Scores (Regression)")
        Assert.IsNotNull(scoreTable, "Factor scores table should be present.")
        Assert.AreEqual(3, scoreTable.TotalCols, "Factor scores table should contain Observation plus two retained factors.")

        Dim varianceTable = FindTableByTitle(tables, "Variance Explained")
        Assert.IsNotNull(varianceTable, "Variance explained table should be present.")
        Assert.AreEqual(10, varianceTable.TotalCols, "Variance explained table should retain the standard ten output columns.")
    End Sub

End Class
