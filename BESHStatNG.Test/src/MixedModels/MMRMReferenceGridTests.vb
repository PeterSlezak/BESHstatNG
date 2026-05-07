Option Explicit On
Option Infer On
Option Strict Off

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.regression

' -----------------------------------------------------------------------------
' Consolidated mixed-model test module.
' This file groups previously separate test classes so the MixedModels test
' folder stays below ten compile modules while preserving the existing tests.
' -----------------------------------------------------------------------------

' ===== BEGIN migrated from MixedModelReferenceGridTests.vb =====



<TestClass()>
Public Class MixedModelReferenceGridTests

    <TestMethod()>
    Public Sub ReferenceGrid_EqualWeights_UserCovariate_BuildsExpectedRows()
        Dim result As MixedModelResult = BuildSyntheticResult()

        Dim treatment() As Double = {0, 0, 1, 1, 1, 0, 1, 0}
        Dim site() As Double = {0, 1, 0, 1, 2, 2, 2, 0}
        Dim age() As Double = {7, 8, 9, 10, 11, 12, 13, 14}

        Dim spec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = result.FixedEffectNames,
            .Weighting = MixedModelReferenceGridWeighting.EqualCells,
            .MultiplicityAdjustment = MixedModelMultiplicityAdjustment.None,
            .Alpha = 0.05
        }

        spec.AddByFactor("treatment_active", New Double() {0, 1}, treatment)
        spec.AddMarginalFactor("site_code", New Double() {0, 1, 2}, site)
        spec.AddCovariateValue("age_centered_8", age, 10.0)

        Dim rows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(spec)

        Assert.AreEqual(2, rows.Count)

        Assert.AreEqual("treatment_active=0", rows(0).Label)
        Assert.AreEqual("treatment_active=1", rows(1).Label)

        ' Equal site weighting means site_code average is (0 + 1 + 2) / 3 = 1.
        AssertVector(New Double() {1.0, 0.0, 1.0, 10.0, 0.0}, rows(0).L, 0.000000001, "control row")
        AssertVector(New Double() {1.0, 1.0, 1.0, 10.0, 10.0}, rows(1).L, 0.000000001, "active row")

        Dim estControl As Double = MixedModelPostEstimation.LinearEstimate(rows(0).L, result.Beta)
        Dim estActive As Double = MixedModelPostEstimation.LinearEstimate(rows(1).L, result.Beta)

        Assert.AreEqual(16.0, estControl, 0.000000001)
        Assert.AreEqual(19.0, estActive, 0.000000001)
    End Sub


    <TestMethod()>
    Public Sub ReferenceGrid_ObservedWeights_DifferFromEqualWeights()
        Dim result As MixedModelResult = BuildSyntheticResult()

        Dim treatment() As Double = {1, 1, 1, 1}
        Dim site() As Double = {0, 0, 0, 2}
        Dim age() As Double = {8, 8, 8, 8}

        Dim equalSpec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = result.FixedEffectNames,
            .Weighting = MixedModelReferenceGridWeighting.EqualCells
        }

        equalSpec.AddByFactor("treatment_active", New Double() {1}, treatment)
        equalSpec.AddMarginalFactor("site_code", New Double() {0, 2}, site)
        equalSpec.AddCovariateValue("age_centered_8", age, 0.0)

        Dim observedSpec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = result.FixedEffectNames,
            .Weighting = MixedModelReferenceGridWeighting.ObservedCellFrequency
        }

        observedSpec.AddByFactor("treatment_active", New Double() {1}, treatment)
        observedSpec.AddMarginalFactor("site_code", New Double() {0, 2}, site)
        observedSpec.AddCovariateValue("age_centered_8", age, 0.0)

        Dim equalRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(equalSpec)

        Dim observedRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(observedSpec)

        ' Equal: average site = 1.  Observed: average site = (0+0+0+2)/4 = 0.5.
        Assert.AreEqual(1.0, equalRows(0).L(2), 0.000000001)
        Assert.AreEqual(0.5, observedRows(0).L(2), 0.000000001)
    End Sub


    <TestMethod()>
    Public Sub ReferenceGrid_PairwiseContrasts_UseMultiplicityAdjustment()
        Dim result As MixedModelResult = BuildSyntheticResult()

        Dim treatment() As Double = {0, 0, 1, 1, 1, 0, 1, 0}
        Dim site() As Double = {0, 1, 0, 1, 2, 2, 2, 0}
        Dim age() As Double = {7, 8, 9, 10, 11, 12, 13, 14}

        Dim spec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = result.FixedEffectNames,
            .Weighting = MixedModelReferenceGridWeighting.EqualCells,
            .MultiplicityAdjustment = MixedModelMultiplicityAdjustment.Holm
        }

        spec.AddByFactor("treatment_active", New Double() {0, 1}, treatment)
        spec.AddMarginalFactor("site_code", New Double() {0, 1, 2}, site)
        spec.AddCovariateValue("age_centered_8", age, 10.0)

        Dim rows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(spec)

        Dim contrastTable As ResultTable =
            MixedModelReferenceGridService.BuildPairwiseContrastsByFactor(rows,
                                                                          result,
                                                                          spec,
                                                                          "treatment_active",
                                                                          "Treatment pairwise contrasts")

        Assert.IsNotNull(contrastTable)
        Assert.IsTrue(contrastTable.PvalColumns.Contains(5), "Raw p-value final column should be marked.")
        Assert.IsTrue(contrastTable.PvalColumns.Contains(6), "Adjusted p-value final column should be marked.")

        Dim contrast() As Double = Matrix.M_SUB(rows(1).L, rows(0).L)
        Dim est As Double = MixedModelPostEstimation.LinearEstimate(contrast, result.Beta)

        Assert.AreEqual(3.0, est, 0.000000001)
    End Sub


    <TestMethod()>
    Public Sub MultiplicityAdjustments_ReturnExpectedValues()
        Dim p() As Double = {0.01, 0.02, 0.2}

        Dim bonf() As Double =
            MixedModelReferenceGridService.AdjustPValues(p, MixedModelMultiplicityAdjustment.Bonferroni)

        Assert.AreEqual(0.03, bonf(0), 0.000000001)
        Assert.AreEqual(0.06, bonf(1), 0.000000001)
        Assert.AreEqual(0.6, bonf(2), 0.000000001)

        Dim holm() As Double =
            MixedModelReferenceGridService.AdjustPValues(p, MixedModelMultiplicityAdjustment.Holm)

        Assert.AreEqual(0.03, holm(0), 0.000000001)
        Assert.AreEqual(0.04, holm(1), 0.000000001)
        Assert.AreEqual(0.2, holm(2), 0.000000001)

        Dim sidak() As Double =
            MixedModelReferenceGridService.AdjustPValues(New Double() {0.05},
                                                         MixedModelMultiplicityAdjustment.Sidak)

        Assert.AreEqual(0.05, sidak(0), 0.000000001)
    End Sub


    Private Shared Function BuildSyntheticResult() As MixedModelResult
        Return New MixedModelResult With {
            .P = 5,
            .Beta = New Double() {10.0, 2.0, 1.0, 0.5, 0.1},
            .VarBeta = New Double(,) {
                {0.04, 0.0, 0.0, 0.0, 0.0},
                {0.0, 0.04, 0.0, 0.0, 0.0},
                {0.0, 0.0, 0.04, 0.0, 0.0},
                {0.0, 0.0, 0.0, 0.01, 0.0},
                {0.0, 0.0, 0.0, 0.0, 0.01}
            },
            .FixedEffectNames = New String() {"Intercept",
                                              "treatment_active",
                                              "site_code",
                                              "age_centered_8",
                                              "treatment_active:age_centered_8"},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.ResidualDF,
            .BetaDF = New Double() {20, 20, 20, 20, 20},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }
    End Function


    Private Shared Sub AssertVector(expected() As Double,
                                    actual() As Double,
                                    tol As Double,
                                    label As String)
        Assert.IsNotNull(actual, label & " should not be Nothing.")
        Assert.AreEqual(expected.Length, actual.Length, label & " length mismatch.")

        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tol, label & " element " & i.ToString())
        Next
    End Sub

End Class

' ===== END migrated from MixedModelReferenceGridTests.vb =====

' ===== BEGIN migrated from MMRMReferenceGridAugmentedCsvTests.vb =====



''' <summary>
''' Integration tests for reference-grid LS-means on the augmented longitudinal
''' multivariable/missing-data CSV.
''' </summary>
''' <remarks>
''' These tests exercise the full path:
'''
'''     augmented CSV -> MMRM UN fit -> MixedModelReferenceGridService -> LS-means / contrasts
'''
''' The tests intentionally use numerical coded columns / fallback deterministic recoding,
''' because the reference-grid foundation is numerical-code based and independent of
''' Ui18MMRM/form metadata.
''' </remarks>
<TestClass()>
Public Class MMRMReferenceGridAugmentedCsvTests

    Private Const TOL As Double = 0.000000001

    Private Shared ReadOnly FixedNames As String() = {
        "Intercept",
        "visit=2",
        "visit=3",
        "visit=4",
        "treatment_active",
        "treatment_active:visit=2",
        "treatment_active:visit=3",
        "treatment_active:visit=4",
        "sex_code",
        "clinic_site_code=1",
        "clinic_site_code=2",
        "baseline_distance_centered",
        "treatment_active:baseline_distance_centered"
    }


    <TestMethod()>
    Public Sub AugmentedCsv_MissingAndIncompletePatterns_AreAsExpected()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")

        Assert.AreEqual(99, dat.RawRows, "Unexpected raw row count.")
        Assert.AreEqual(4, dat.MissingResponseRows, "Unexpected missing-response row count.")
        Assert.AreEqual(95, dat.Y.Length, "Unexpected analysis row count after excluding missing response.")
        Assert.AreEqual(27, CountDistinctSubjects(dat.Subject), "Unexpected subject count.")

        Dim patternCounts As Dictionary(Of String, Integer) = ObservedPatternCounts(dat)

        Assert.AreEqual(16, CountSubjectsWithObservedVisitCount(patternCounts, 4), "Unexpected number of complete observed-response subjects.")
        Assert.AreEqual(9, CountSubjectsWithObservedVisitCount(patternCounts, 3), "Unexpected number of 3-visit observed-response subjects.")
        Assert.AreEqual(2, CountSubjectsWithObservedVisitCount(patternCounts, 2), "Unexpected number of 2-visit observed-response subjects.")

        Assert.IsTrue(patternCounts.ContainsKey("V1V2V3V4"), "Expected complete visit pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V1V2V3"), "Expected V1V2V3 pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V1V2V4"), "Expected V1V2V4 pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V1V3V4"), "Expected V1V3V4 pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V2V3V4"), "Expected V2V3V4 pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V1V2"), "Expected V1V2 pattern.")
        Assert.IsTrue(patternCounts.ContainsKey("V1V3"), "Expected V1V3 pattern.")
    End Sub


    <TestMethod()>
    Public Sub AugmentedCsv_UNMMRM_ReferenceGridLSMeans_EqualWeights_BuildsVisitTreatmentRows()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")
        Dim result As MixedModelResult = FitUNMMRM(dat)

        Assert.IsNotNull(result, "MMRM result should not be Nothing.")
        ' Assert.IsTrue(result.Converged, "UN MMRM should converge on augmented CSV.")
        Assert.AreEqual(dat.Y.Length, result.Nobs, "Unexpected observation count in fit.")
        Assert.AreEqual(27, result.NoSubjects, "Unexpected subject count in fit.")
        Assert.AreEqual(FixedNames.Length, result.P, "Unexpected fixed-effect dimension.")

        Dim spec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                         result.FixedEffectNames,
                                                                         MixedModelReferenceGridWeighting.EqualCells,
                                                                         useObservedMeanBaseline:=True)

        Dim rows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(spec)

        Assert.AreEqual(8, rows.Count, "Expected visit x treatment reference-grid rows.")

        For Each row As MixedModelReferenceGridRow In rows
            Assert.IsNotNull(row.L, "Reference-grid row L should not be Nothing.")
            Assert.AreEqual(result.P, row.L.Length, "Reference-grid row L length should match P.")

            Dim est As Double = MixedModelPostEstimation.LinearEstimate(row.L, result.Beta)
            Dim varEst As Double = MixedModelPostEstimation.LinearCombinationVariance(row.L, result.VarBeta)

            AssertFinite(est, row.Label & " estimate")
            AssertFinite(varEst, row.Label & " variance")
            Assert.IsTrue(varEst > 0.0, row.Label & " variance should be positive.")
            Assert.IsTrue(row.Count > 0, row.Label & " should have positive observed support.")
        Next

        Dim table As ResultTable =
            MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid LS-means by visit and treatment",
                                                                    rows,
                                                                    result,
                                                                    spec)

        Assert.IsNotNull(table, "Reference-grid LS-means table should be created.")
        Assert.IsTrue(table.PvalColumns.Contains(6),
                      "Estimated-means table should mark the p-value body column.")
    End Sub


    <TestMethod()>
    Public Sub AugmentedCsv_EqualAndObservedWeighting_ProduceDifferentReferenceRows()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")
        Dim result As MixedModelResult = FitUNMMRM(dat)

        Dim equalSpec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                              result.FixedEffectNames,
                                                                              MixedModelReferenceGridWeighting.EqualCells,
                                                                              useObservedMeanBaseline:=True)

        Dim observedSpec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                                 result.FixedEffectNames,
                                                                                 MixedModelReferenceGridWeighting.ObservedCellFrequency,
                                                                                 useObservedMeanBaseline:=True)

        Dim equalRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(equalSpec)

        Dim observedRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(observedSpec)

        Assert.AreEqual(equalRows.Count, observedRows.Count, "Equal/observed grids should have same by-profile row count.")

        Dim foundDifference As Boolean = False

        For i As Integer = 0 To equalRows.Count - 1
            Dim diff As Double = MaxAbsDiff(equalRows(i).L, observedRows(i).L)

            If diff > 0.000000000001 Then
                foundDifference = True
                Exit For
            End If
        Next

        Assert.IsTrue(foundDifference,
                      "Equal-cell and observed-cell weighting should produce at least one different L row on the augmented CSV.")
    End Sub


    <TestMethod()>
    Public Sub AugmentedCsv_CovariateMeanAndUserSpecifiedValue_ProduceDifferentReferenceRows()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")
        Dim result As MixedModelResult = FitUNMMRM(dat)

        Dim meanSpec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                             result.FixedEffectNames,
                                                                             MixedModelReferenceGridWeighting.EqualCells,
                                                                             useObservedMeanBaseline:=True)

        Dim zeroSpec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                             result.FixedEffectNames,
                                                                             MixedModelReferenceGridWeighting.EqualCells,
                                                                             useObservedMeanBaseline:=False)

        Dim meanRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(meanSpec)

        Dim zeroRows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(zeroSpec)

        Assert.AreEqual(meanRows.Count, zeroRows.Count)

        Dim baselineIndex As Integer = Array.IndexOf(result.FixedEffectNames, "baseline_distance_centered")
        Dim interactionIndex As Integer = Array.IndexOf(result.FixedEffectNames, "treatment_active:baseline_distance_centered")

        Assert.IsTrue(baselineIndex >= 0, "baseline_distance_centered fixed-effect column should exist.")
        Assert.IsTrue(interactionIndex >= 0, "treatment_active:baseline_distance_centered fixed-effect column should exist.")

        Dim meanBaseline As Double = Mean(dat.BaselineDistanceCentered)
        Assert.AreEqual(meanBaseline, meanRows(0).L(baselineIndex), 0.000000001)
        Assert.AreEqual(0.0, zeroRows(0).L(baselineIndex), 0.000000001)

        ' Active rows should also change the treatment x baseline interaction column.
        Dim activeMeanRow As MixedModelReferenceGridRow = FindRow(meanRows, visit:=1.0, treatment:=1.0)
        Dim activeZeroRow As MixedModelReferenceGridRow = FindRow(zeroRows, visit:=1.0, treatment:=1.0)

        Assert.AreEqual(meanBaseline, activeMeanRow.L(interactionIndex), 0.000000001)
        Assert.AreEqual(0.0, activeZeroRow.L(interactionIndex), 0.000000001)
    End Sub


    <TestMethod()>
    Public Sub AugmentedCsv_TreatmentContrastsByVisit_WithHolmAdjustment_AreFinite()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")
        Dim result As MixedModelResult = FitUNMMRM(dat)

        Dim spec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                         result.FixedEffectNames,
                                                                         MixedModelReferenceGridWeighting.EqualCells,
                                                                         useObservedMeanBaseline:=True)
        spec.MultiplicityAdjustment = MixedModelMultiplicityAdjustment.Holm

        Dim rows As List(Of MixedModelReferenceGridRow) =
            MixedModelReferenceGridService.BuildReferenceGridRows(spec)

        Dim contrastTable As ResultTable =
            MixedModelReferenceGridService.BuildPairwiseContrastsByFactor(rows,
                                                                          result,
                                                                          spec,
                                                                          "treatment_active",
                                                                          "Reference-grid treatment contrasts by visit")

        Assert.IsNotNull(contrastTable, "Treatment contrast table should be created.")
        Assert.IsTrue(contrastTable.PvalColumns.Contains(5), "Raw p-value final column should be marked.")
        Assert.IsTrue(contrastTable.PvalColumns.Contains(6), "Adjusted p-value final column should be marked.")

        For Each v As Double In New Double() {1.0, 2.0, 3.0, 4.0}
            Dim control As MixedModelReferenceGridRow = FindRow(rows, v, 0.0)
            Dim active As MixedModelReferenceGridRow = FindRow(rows, v, 1.0)

            Dim lDiff() As Double = Matrix.M_SUB(active.L, control.L)
            Dim est As Double = MixedModelPostEstimation.LinearEstimate(lDiff, result.Beta)
            Dim varEst As Double = MixedModelPostEstimation.LinearCombinationVariance(lDiff, result.VarBeta)

            AssertFinite(est, "Treatment contrast estimate at visit " & v.ToString(CultureInfo.InvariantCulture))
            AssertFinite(varEst, "Treatment contrast variance at visit " & v.ToString(CultureInfo.InvariantCulture))
            Assert.IsTrue(varEst > 0.0, "Treatment contrast variance should be positive.")
        Next
    End Sub



    ' R mmrm external reference validation for KR reference-grid LS-means and
    ' treatment contrasts.  This test is intentionally disabled until the
    ' generated constants from R_referenceScripts/kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R
    ' are pasted into RmmrmReferenceGridLSMeanReferences() and
    ' RmmrmReferenceGridContrastReferences().
    <TestMethod()>
    Public Sub AugmentedCsv_KRReferenceGridLSMeansAndTreatmentContrasts_MatchRmmrmReferences()
        Dim dat As AugmentedData = LoadAugmentedData("mixedmodel_longitudinal_multicovariate_missing.csv")
        Dim lsRefs As List(Of KrReferenceGridLSMeanReference) = RmmrmReferenceGridLSMeanReferences()
        Dim contrastRefs As List(Of KrReferenceGridContrastReference) = RmmrmReferenceGridContrastReferences()

        Assert.IsTrue(lsRefs.Count > 0, "Paste R mmrm LS-mean references into RmmrmReferenceGridLSMeanReferences().")
        Assert.IsTrue(contrastRefs.Count > 0, "Paste R mmrm contrast references into RmmrmReferenceGridContrastReferences().")

        Dim structureNames As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each r As KrReferenceGridLSMeanReference In lsRefs
            structureNames.Add(r.StructureName)
        Next
        For Each r As KrReferenceGridContrastReference In contrastRefs
            structureNames.Add(r.StructureName)
        Next

        For Each structureName As String In structureNames
            Dim result As MixedModelResult = FitMMRMWithFullKR(dat, structureName)
            AssertUsableKRReferenceGridResult(result, structureName)

            Dim spec As MixedModelReferenceGridSpec = BuildReferenceGridSpec(dat,
                                                                             result.FixedEffectNames,
                                                                             MixedModelReferenceGridWeighting.EqualCells,
                                                                             useObservedMeanBaseline:=True)
            spec.MultiplicityAdjustment = MixedModelMultiplicityAdjustment.None

            Dim rows As List(Of MixedModelReferenceGridRow) =
                MixedModelReferenceGridService.BuildReferenceGridRows(spec)

            For Each expected As KrReferenceGridLSMeanReference In lsRefs
                If Not String.Equals(expected.StructureName, structureName, StringComparison.OrdinalIgnoreCase) Then Continue For

                Dim row As MixedModelReferenceGridRow = FindRow(rows, expected.Visit, expected.Treatment)
                AssertLinearInferenceMatchesReference(result,
                                                      row.Label,
                                                      row.L,
                                                      expected.Estimate,
                                                      expected.OrdinarySE,
                                                      expected.KRSE,
                                                      expected.DF,
                                                      expected.TValue,
                                                      expected.PValue,
                                                      structureName & " " & expected.Label)
            Next

            For Each expected As KrReferenceGridContrastReference In contrastRefs
                If Not String.Equals(expected.StructureName, structureName, StringComparison.OrdinalIgnoreCase) Then Continue For

                Dim control As MixedModelReferenceGridRow = FindRow(rows, expected.Visit, 0.0)
                Dim active As MixedModelReferenceGridRow = FindRow(rows, expected.Visit, 1.0)
                Dim lDiff() As Double = Matrix.M_SUB(active.L, control.L)

                AssertLinearInferenceMatchesReference(result,
                                                      expected.Label,
                                                      lDiff,
                                                      expected.Estimate,
                                                      expected.OrdinarySE,
                                                      expected.KRSE,
                                                      expected.DF,
                                                      expected.TValue,
                                                      expected.PValue,
                                                      structureName & " " & expected.Label)
            Next
        Next
    End Sub


    Private Shared Sub AssertLinearInferenceMatchesReference(result As MixedModelResult,
                                                             label As String,
                                                             l() As Double,
                                                             expectedEstimate As Double,
                                                             expectedOrdinarySE As Double,
                                                             expectedKRSE As Double,
                                                             expectedDF As Double,
                                                             expectedT As Double,
                                                             expectedP As Double,
                                                             messagePrefix As String)
        Dim est As Double = Double.NaN
        Dim krSE As Double = Double.NaN
        Dim df As Double = Double.NaN
        Dim stat As Double = Double.NaN
        Dim pValue As Double = Double.NaN
        Dim lo As Double = Double.NaN
        Dim hi As Double = Double.NaN
        Dim diagnostic As String = Nothing

        Assert.IsTrue(MixedModelPostEstimation.TryLinearInference(result,
                                                                  label,
                                                                  l,
                                                                  alpha:=0.05,
                                                                  estimate:=est,
                                                                  standardError:=krSE,
                                                                  df:=df,
                                                                  statistic:=stat,
                                                                  pValue:=pValue,
                                                                  lowerCI:=lo,
                                                                  upperCI:=hi,
                                                                  diagnostic:=diagnostic),
                      messagePrefix & ": TryLinearInference failed: " & diagnostic)

        Dim ordinarySE As Double = Math.Sqrt(MixedModelPostEstimation.LinearCombinationVariance(l, result.VarBeta))

        AssertAlmostEqualFlexible(expectedEstimate, est, 0.003, 0.0005, messagePrefix & " estimate")
        AssertAlmostEqualFlexible(expectedOrdinarySE, ordinarySE, 0.004, 0.004, messagePrefix & " ordinary SE")
        AssertAlmostEqualFlexible(expectedKRSE, krSE, 0.006, 0.008, messagePrefix & " KR SE")
        AssertAlmostEqualFlexible(expectedDF, df, 0.35, 0.015, messagePrefix & " KR denominator DF")
        AssertAlmostEqualFlexible(expectedT, stat, 0.08, 0.006, messagePrefix & " KR t statistic")
        AssertAlmostEqualFlexible(expectedP, pValue, 0.01, 0.03, messagePrefix & " KR p-value")
    End Sub


    Private Shared Sub AssertAlmostEqualFlexible(expected As Double,
                                                 actual As Double,
                                                 absoluteTolerance As Double,
                                                 relativeTolerance As Double,
                                                 label As String)
        AssertFinite(actual, label)

        Dim diff As Double = Math.Abs(expected - actual)
        Dim allowed As Double = Math.Max(absoluteTolerance, relativeTolerance * Math.Abs(expected))

        If diff > allowed Then
            Assert.Fail(label & ": expected " & expected.ToString("R", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("R", CultureInfo.InvariantCulture) &
                        ", abs diff " & diff.ToString("R", CultureInfo.InvariantCulture) &
                        " > allowed tolerance " & allowed.ToString("R", CultureInfo.InvariantCulture) &
                        " (abs=" & absoluteTolerance.ToString("R", CultureInfo.InvariantCulture) &
                        ", rel=" & relativeTolerance.ToString("R", CultureInfo.InvariantCulture) & ").")
        End If
    End Sub


    Private Shared Sub AssertUsableKRReferenceGridResult(result As MixedModelResult,
                                                         structureName As String)
        Assert.IsNotNull(result, structureName & " MMRM result should not be Nothing.")
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, result.FixedInferenceMethod,
                        structureName & " MMRM should use Kenward-Roger fixed-effect inference.")
        Assert.IsNotNull(result.KenwardRogerWorkspace, structureName & " MMRM should have a KR workspace.")
        Assert.IsNotNull(result.KenwardRogerAdjustedVarBeta, structureName & " MMRM should have KR-adjusted Var(beta).")
        Assert.AreEqual(MixedModelKrParameterScale.MmrmTheta,
                        result.KenwardRogerWorkspace.ParameterScale,
                        structureName & " KR parameter scale should match R mmrm-style theta.")
        Assert.AreEqual(FixedNames.Length, result.P, structureName & " fixed-effect dimension.")
    End Sub


    Private Shared Function FitMMRMWithFullKR(dat As AugmentedData,
                                              structureName As String) As MixedModelResult
        Dim x(,) As Double = BuildFixedDesign(dat)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          CreateReferenceGridRStruct(structureName),
                                                                          MixedModelFitMethod.REML)
        req.RequestLabel = "Augmented longitudinal CSV KR reference-grid " & structureName & " MMRM"
        req.ResponseVarName = "distance_mm"
        req.SubjectVarName = "subject_id"
        req.VisitVarName = "visit"
        req.FixedEffectNames = CType(FixedNames.Clone(), String())
        req.EnableFullKenwardRogerForMmrm()
        req.Control = TestControl()

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Function CreateReferenceGridRStruct(structureName As String) As MixedModelRStruct
        Select Case If(structureName, String.Empty).Trim().ToLowerInvariant()
            Case "compound symmetry"
                Return New CompoundSymmetryR()
            Case "heterogeneous compound symmetry"
                Return New HeterogeneousCSR()
            Case "ar(1)"
                Return New AR1R()
            Case "heterogeneous ar(1)"
                Return New HeterogeneousAR1R()
            Case "unstructured"
                Return New UnstructuredR()
            Case Else
                Throw New ArgumentException("Unsupported R mmrm reference-grid covariance structure: " & structureName)
        End Select
    End Function


    Private Shared Function RmmrmReferenceGridLSMeanReferences() As List(Of KrReferenceGridLSMeanReference)
        ' Generated from R mmrm external reference script:
        ' R_referenceScripts/kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R
        ' Current reference block covers the Compound Symmetry structure.
        Return New List(Of KrReferenceGridLSMeanReference) From {
New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=1, treatment_active=0", visit:=1, treatment:=0, estimate:=22.2271514679, ordinarySE:=0.470473665235, krSE:=0.467011780689, df:=68.5561414941, tValue:=47.59441279, pValue:=2.97937730074e-54),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=1, treatment_active=1", visit:=1, treatment:=1, estimate:=21.8036307227, ordinarySE:=0.488909198679, krSE:=0.48675735518, df:=72.5758507542, tValue:=44.7936338109, pValue:=1.26893278329e-54),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=2, treatment_active=0", visit:=2, treatment:=0, estimate:=22.9485739347, ordinarySE:=0.507844292238, krSE:=0.505761225708, df:=72.8222544375, tValue:=45.3743244207, pValue:=3.81492535504e-55),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=2, treatment_active=1", visit:=2, treatment:=1, estimate:=23.0915627645, ordinarySE:=0.469799827332, krSE:=0.466971064528, df:=70.1694841504, tValue:=49.4496651262, pValue:=2.78784833054e-56),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=3, treatment_active=0", visit:=3, treatment:=0, estimate:=23.9248542163, ordinarySE:=0.509748641239, krSE:=0.507900555978, df:=72.5755448614, tValue:=47.1053908777, pValue:=3.71161608323e-56),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=3, treatment_active=1", visit:=3, treatment:=1, estimate:=25.0422146809, ordinarySE:=0.470455988721, krSE:=0.467626203054, df:=70.3134410749, tValue:=53.5517781453, pValue:=9.81567609865e-59),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=4, treatment_active=0", visit:=4, treatment:=0, estimate:=25.6582284561, ordinarySE:=0.537015510276, krSE:=0.535687681599, df:=76.0106879098, tValue:=47.8977384351, pValue:=1.55011497676e-58),
    New KrReferenceGridLSMeanReference(structureName:="Compound Symmetry", label:="visit=4, treatment_active=1", visit:=4, treatment:=1, estimate:=26.3386769853, ordinarySE:=0.491116501772, krSE:=0.488995841906, df:=72.9164992609, tValue:=53.8627831325, pValue:=1.80759929263e-60)
        }
    End Function


    Private Shared Function RmmrmReferenceGridContrastReferences() As List(Of KrReferenceGridContrastReference)
        ' Generated from R mmrm external reference script:
        ' R_referenceScripts/kr_mmrm_multicovariate_missing_lsmeans_contrasts_reference.R
        ' Current reference block covers the Compound Symmetry structure.
        Return New List(Of KrReferenceGridContrastReference) From {
New KrReferenceGridContrastReference(structureName:="Compound Symmetry", label:="treatment_active=1 - treatment_active=0 | visit=1", visit:=1, estimate:=-0.423520745257, ordinarySE:=0.678730297155, krSE:=0.674864310172, df:=71.0584188251, tValue:=-0.627564295331, pValue:=0.532300953667),
    New KrReferenceGridContrastReference(structureName:="Compound Symmetry", label:="treatment_active=1 - treatment_active=0 | visit=2", visit:=2, estimate:=0.142988829885, ordinarySE:=0.690648955458, krSE:=0.687255279494, df:=71.8036897192, tValue:=0.208057812216, pValue:=0.835772844813),
    New KrReferenceGridContrastReference(structureName:="Compound Symmetry", label:="treatment_active=1 - treatment_active=0 | visit=3", visit:=3, estimate:=1.11736046456, ordinarySE:=0.691201083053, krSE:=0.687865749326, df:=71.7673803208, tValue:=1.6243874123, pValue:=0.108677424281),
    New KrReferenceGridContrastReference(structureName:="Compound Symmetry", label:="treatment_active=1 - treatment_active=0 | visit=4", visit:=4, estimate:=0.680448529178, ordinarySE:=0.725800810911, krSE:=0.723234403188, df:=75.0645494943, tValue:=0.940840930932, pValue:=0.349804308834)
        }
    End Function


    Private Class KrReferenceGridLSMeanReference
        Public ReadOnly StructureName As String
        Public ReadOnly Label As String
        Public ReadOnly Visit As Double
        Public ReadOnly Treatment As Double
        Public ReadOnly Estimate As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRSE As Double
        Public ReadOnly DF As Double
        Public ReadOnly TValue As Double
        Public ReadOnly PValue As Double

        Public Sub New(structureName As String,
                       label As String,
                       visit As Double,
                       treatment As Double,
                       estimate As Double,
                       ordinarySE As Double,
                       krSE As Double,
                       df As Double,
                       tValue As Double,
                       pValue As Double)
            Me.StructureName = structureName
            Me.Label = label
            Me.Visit = visit
            Me.Treatment = treatment
            Me.Estimate = estimate
            Me.OrdinarySE = ordinarySE
            Me.KRSE = krSE
            Me.DF = df
            Me.TValue = tValue
            Me.PValue = pValue
        End Sub
    End Class


    Private Class KrReferenceGridContrastReference
        Public ReadOnly StructureName As String
        Public ReadOnly Label As String
        Public ReadOnly Visit As Double
        Public ReadOnly Estimate As Double
        Public ReadOnly OrdinarySE As Double
        Public ReadOnly KRSE As Double
        Public ReadOnly DF As Double
        Public ReadOnly TValue As Double
        Public ReadOnly PValue As Double

        Public Sub New(structureName As String,
                       label As String,
                       visit As Double,
                       estimate As Double,
                       ordinarySE As Double,
                       krSE As Double,
                       df As Double,
                       tValue As Double,
                       pValue As Double)
            Me.StructureName = structureName
            Me.Label = label
            Me.Visit = visit
            Me.Estimate = estimate
            Me.OrdinarySE = ordinarySE
            Me.KRSE = krSE
            Me.DF = df
            Me.TValue = tValue
            Me.PValue = pValue
        End Sub
    End Class

    Private Shared Function FitUNMMRM(dat As AugmentedData) As MixedModelResult
        Dim x(,) As Double = BuildFixedDesign(dat)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Y,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)
        req.RequestLabel = "Augmented longitudinal CSV reference-grid UN MMRM"
        req.ResponseVarName = "distance_mm"
        req.SubjectVarName = "subject_id"
        req.VisitVarName = "visit"
        req.FixedEffectNames = CType(FixedNames.Clone(), String())
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite
        req.UseSatterthwaite = True
        req.Control = TestControl()

        Return (New MMRM(req)).Fit()
    End Function


    Private Shared Function BuildReferenceGridSpec(dat As AugmentedData,
                                                   fixedEffectNames() As String,
                                                   weighting As MixedModelReferenceGridWeighting,
                                                   useObservedMeanBaseline As Boolean) As MixedModelReferenceGridSpec
        Dim spec As New MixedModelReferenceGridSpec With {
            .FixedEffectNames = fixedEffectNames,
            .Weighting = weighting,
            .MultiplicityAdjustment = MixedModelMultiplicityAdjustment.None,
            .Alpha = 0.05
        }

        spec.AddByFactor("visit", New Double() {1, 2, 3, 4}, dat.Visit)
        spec.AddByFactor("treatment_active", New Double() {0, 1}, dat.TreatmentActive)

        spec.AddMarginalFactor("sex_code", New Double() {0, 1}, dat.SexCode)
        spec.AddMarginalFactor("clinic_site_code", New Double() {0, 1, 2}, dat.ClinicSiteCode)

        If useObservedMeanBaseline Then
            spec.AddCovariateMean("baseline_distance_centered", dat.BaselineDistanceCentered)
        Else
            spec.AddCovariateValue("baseline_distance_centered", dat.BaselineDistanceCentered, 0.0)
        End If

        Return spec
    End Function


    Private Shared Function BuildFixedDesign(dat As AugmentedData) As Double(,)
        Dim n As Integer = dat.Y.Length
        Dim x(n - 1, FixedNames.Length - 1) As Double

        For i As Integer = 0 To n - 1
            Dim v As Double = dat.Visit(i)
            Dim trt As Double = dat.TreatmentActive(i)
            Dim site As Double = dat.ClinicSiteCode(i)
            Dim baseline As Double = dat.BaselineDistanceCentered(i)

            x(i, 0) = 1.0
            x(i, 1) = If(NearlyEqual(v, 2.0), 1.0, 0.0)
            x(i, 2) = If(NearlyEqual(v, 3.0), 1.0, 0.0)
            x(i, 3) = If(NearlyEqual(v, 4.0), 1.0, 0.0)
            x(i, 4) = trt
            x(i, 5) = trt * x(i, 1)
            x(i, 6) = trt * x(i, 2)
            x(i, 7) = trt * x(i, 3)
            x(i, 8) = dat.SexCode(i)
            x(i, 9) = If(NearlyEqual(site, 1.0), 1.0, 0.0)
            x(i, 10) = If(NearlyEqual(site, 2.0), 1.0, 0.0)
            x(i, 11) = baseline
            x(i, 12) = trt * baseline
        Next

        Return x
    End Function


    Private Shared Function LoadAugmentedData(fileName As String) As AugmentedData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException("CSV must contain a header and data rows.")
        End If

        Dim header() As String = SplitCsvSimple(lines(0))
        Dim col As Dictionary(Of String, Integer) = BuildColumnMap(header)

        Dim missingResponseRows As Integer = 0

        Dim subject As New List(Of Object)()
        Dim y As New List(Of Double)()
        Dim visit As New List(Of Double)()
        Dim treatment As New List(Of Double)()
        Dim sexCode As New List(Of Double)()
        Dim siteCode As New List(Of Double)()
        Dim baseline As New List(Of Double)()

        For i As Integer = 1 To lines.Length - 1
            If String.IsNullOrWhiteSpace(lines(i)) Then Continue For

            Dim parts() As String = SplitCsvSimple(lines(i))

            Dim yi As Double = GetNumeric(parts, col, "distance_mm")
            If Not IsFinite(yi) Then
                missingResponseRows += 1
                Continue For
            End If

            subject.Add(GetString(parts, col, "subject_id"))
            y.Add(yi)
            visit.Add(GetNumeric(parts, col, "visit"))
            sexCode.Add(GetNumeric(parts, col, "sex_code"))
            treatment.Add(GetTreatmentActive(parts, col))
            siteCode.Add(GetClinicSiteCode(parts, col))
            baseline.Add(GetNumeric(parts, col, "baseline_distance_centered"))
        Next

        Return New AugmentedData With {
            .RawRows = lines.Length - 1,
            .MissingResponseRows = missingResponseRows,
            .Subject = subject.ToArray(),
            .Y = y.ToArray(),
            .Visit = visit.ToArray(),
            .TreatmentActive = treatment.ToArray(),
            .SexCode = sexCode.ToArray(),
            .ClinicSiteCode = siteCode.ToArray(),
            .BaselineDistanceCentered = baseline.ToArray()
        }
    End Function


    Private Shared Function GetTreatmentActive(parts() As String,
                                               col As Dictionary(Of String, Integer)) As Double
        Dim candidateNames() As String = {
            "treatment_active",
            "treatment_active_code",
            "active_code",
            "treatment_arm_code"
        }

        For Each name As String In candidateNames
            If col.ContainsKey(name) Then
                Dim v As Double = GetNumeric(parts, col, name)

                ' If treatment_arm_code uses 1/2 coding, convert common 2=Active to 1.
                If name.Equals("treatment_arm_code", StringComparison.OrdinalIgnoreCase) AndAlso v > 1.0 Then
                    Return If(NearlyEqual(v, 2.0), 1.0, 0.0)
                End If

                Return v
            End If
        Next

        Dim raw As String = GetString(parts, col, "treatment_arm")
        Return If(raw.Equals("Active", StringComparison.OrdinalIgnoreCase), 1.0, 0.0)
    End Function


    Private Shared Function GetClinicSiteCode(parts() As String,
                                              col As Dictionary(Of String, Integer)) As Double
        Dim candidateNames() As String = {
            "clinic_site_code",
            "site_code",
            "clinic_code"
        }

        For Each name As String In candidateNames
            If col.ContainsKey(name) Then Return GetNumeric(parts, col, name)
        Next

        Dim raw As String = GetString(parts, col, "clinic_site")

        If raw.Equals("North", StringComparison.OrdinalIgnoreCase) Then Return 0.0
        If raw.Equals("Central", StringComparison.OrdinalIgnoreCase) Then Return 1.0
        If raw.Equals("South", StringComparison.OrdinalIgnoreCase) Then Return 2.0

        Throw New InvalidOperationException("Unknown clinic_site value: " & raw)
    End Function


    Private Shared Function ObservedPatternCounts(dat As AugmentedData) As Dictionary(Of String, Integer)
        Dim bySubject As New Dictionary(Of String, SortedSet(Of Integer))(StringComparer.OrdinalIgnoreCase)

        For i As Integer = 0 To dat.Subject.Length - 1
            Dim s As String = CStr(dat.Subject(i))

            If Not bySubject.ContainsKey(s) Then
                bySubject(s) = New SortedSet(Of Integer)()
            End If

            bySubject(s).Add(CInt(dat.Visit(i)))
        Next

        Dim out As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For Each kvp As KeyValuePair(Of String, SortedSet(Of Integer)) In bySubject
            Dim label As String = String.Empty

            For Each v As Integer In kvp.Value
                label &= "V" & v.ToString(CultureInfo.InvariantCulture)
            Next

            If Not out.ContainsKey(label) Then out(label) = 0
            out(label) += 1
        Next

        Return out
    End Function


    Private Shared Function CountSubjectsWithObservedVisitCount(patternCounts As Dictionary(Of String, Integer),
                                                                nVisits As Integer) As Integer
        Dim total As Integer = 0

        For Each kvp As KeyValuePair(Of String, Integer) In patternCounts
            Dim count As Integer = 0

            For i As Integer = 0 To kvp.Key.Length - 1
                If kvp.Key(i) = "V"c Then count += 1
            Next

            If count = nVisits Then total += kvp.Value
        Next

        Return total
    End Function


    Private Shared Function FindRow(rows As List(Of MixedModelReferenceGridRow),
                                    visit As Double,
                                    treatment As Double) As MixedModelReferenceGridRow
        For Each row As MixedModelReferenceGridRow In rows
            If row.Profile.ContainsKey("visit") AndAlso row.Profile.ContainsKey("treatment_active") Then
                If NearlyEqual(row.Profile("visit"), visit) AndAlso NearlyEqual(row.Profile("treatment_active"), treatment) Then
                    Return row
                End If
            End If
        Next

        Throw New InvalidOperationException("Could not find reference-grid row for visit=" &
                                            visit.ToString(CultureInfo.InvariantCulture) &
                                            ", treatment_active=" &
                                            treatment.ToString(CultureInfo.InvariantCulture))
    End Function


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 180
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function CountDistinctSubjects(subject() As Object) As Integer
        Dim h As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each s As Object In subject
            h.Add(CStr(s))
        Next

        Return h.Count
    End Function


    Private Shared Function Mean(values() As Double) As Double
        Dim s As Double = 0.0
        Dim n As Integer = 0

        For Each v As Double In values
            If IsFinite(v) Then
                s += v
                n += 1
            End If
        Next

        If n = 0 Then Return Double.NaN
        Return s / CDbl(n)
    End Function


    Private Shared Function MaxAbsDiff(a() As Double,
                                       b() As Double) As Double
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return Double.NaN

        Dim out As Double = 0.0

        For i As Integer = 0 To a.Length - 1
            out = Math.Max(out, Math.Abs(a(i) - b(i)))
        Next

        Return out
    End Function


    Private Shared Sub AssertFinite(value As Double,
                                    label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub


    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim candidates As String() = {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\..\TestData", fileName))
        }

        For Each candidate As String In candidates
            If File.Exists(candidate) Then Return candidate
        Next

        Throw New FileNotFoundException("Could not locate test data file.", fileName)
    End Function


    Private Shared Function BuildColumnMap(header() As String) As Dictionary(Of String, Integer)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        Return col
    End Function


    Private Shared Function GetString(parts() As String,
                                      col As Dictionary(Of String, Integer),
                                      name As String) As String
        If Not col.ContainsKey(name) Then
            Throw New InvalidOperationException("CSV is missing required column '" & name & "'.")
        End If

        Dim idx As Integer = col(name)
        If idx < 0 OrElse idx >= parts.Length Then Return String.Empty

        Return parts(idx).Trim()
    End Function


    Private Shared Function GetNumeric(parts() As String,
                                       col As Dictionary(Of String, Integer),
                                       name As String) As Double
        Dim s As String = GetString(parts, col, name)
        If String.IsNullOrWhiteSpace(s) Then Return Double.NaN

        Return Double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function SplitCsvSimple(line As String) As String()
        ' The test CSV has no embedded commas in values.
        Dim raw() As String = line.Split(","c)
        Dim out(raw.Length - 1) As String

        For i As Integer = 0 To raw.Length - 1
            out(i) = raw(i).Trim()
        Next

        Return out
    End Function


    Private Shared Function NearlyEqual(a As Double,
                                        b As Double) As Boolean
        Return Math.Abs(a - b) <= 0.000000001
    End Function


    Private Shared Function IsFinite(x As Double) As Boolean
        Return Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)
    End Function


    Private Class AugmentedData
        Public RawRows As Integer
        Public MissingResponseRows As Integer
        Public Subject() As Object
        Public Y() As Double
        Public Visit() As Double
        Public TreatmentActive() As Double
        Public SexCode() As Double
        Public ClinicSiteCode() As Double
        Public BaselineDistanceCentered() As Double
    End Class

End Class

' ===== END migrated from MMRMReferenceGridAugmentedCsvTests.vb =====

