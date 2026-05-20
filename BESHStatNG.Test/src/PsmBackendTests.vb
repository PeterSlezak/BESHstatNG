Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.CausalInference

<TestClass()>
Public Class PsmBackendTests

    Private Const TOL As Double = 0.000000001

    Private Shared Function IsFiniteValue(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Sub AssertFinite(value As Double, Optional message As String = "")
        Assert.IsTrue(IsFiniteValue(value), If(String.IsNullOrWhiteSpace(message), "Expected a finite numeric value.", message))
    End Sub

    Private Shared Sub AssertVectorFinite(values() As Double, Optional message As String = "")
        Assert.IsNotNull(values, "Vector is Nothing. " & message)
        For i As Integer = 0 To values.Length - 1
            AssertFinite(values(i), $"{message} Non-finite value at index {i}.")
        Next
    End Sub

    Private Shared Function MakeSuppliedScoreInput() As PsmInputData
        ' Four treated rows and eight controls. Scores/exact groups are set so 1:1 ATT
        ' nearest-neighbour matching should produce four deterministic pairs.
        Return New PsmInputData With {
            .Ids = New String() {"T1", "T2", "T3", "T4", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8"},
            .Treatment = New Double() {1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0},
            .Outcome = New Double() {10, 9, 11, 7, 7, 6, 8, 6, 5, 4, 6, 5},
            .Covariates = New Double(,) {
                {8.0, 1.0},
                {6.0, 2.0},
                {7.5, 1.5},
                {5.0, 2.5},
                {7.8, 1.1},
                {6.2, 2.2},
                {7.4, 1.6},
                {4.8, 2.4},
                {3.0, 3.0},
                {2.0, 3.5},
                {3.5, 3.2},
                {2.5, 3.7}
            },
            .CovariateNames = New String() {"x1", "x2"},
            .SuppliedPropensityScores = New Double() {0.80, 0.60, 0.75, 0.50, 0.78, 0.62, 0.74, 0.48, 0.30, 0.22, 0.35, 0.25},
            .ExactGroupLabels = New String() {"A", "A", "B", "B", "A", "A", "B", "B", "A", "A", "B", "B"}
        }
    End Function

    Private Shared Function MakeLogisticInput(Optional n As Integer = 60) As PsmInputData
        Dim ids(n - 1) As String
        Dim treatment(n - 1) As Double
        Dim outcome(n - 1) As Double
        Dim cov(n - 1, 2) As Double

        For i As Integer = 0 To n - 1
            Dim x1 As Double = -2.0 + 4.0 * CDbl(i) / CDbl(n - 1)
            Dim x2 As Double = Math.Sin(CDbl(i) * 0.45)
            Dim x3 As Double = If((i Mod 3) = 0, 1.0, 0.0)
            Dim lp As Double = -0.15 + 0.75 * x1 - 0.35 * x2 + 0.25 * x3
            Dim p As Double = 1.0 / (1.0 + Math.Exp(-lp))
            Dim pseudoUniform As Double = CDbl((i * 37 + 11) Mod 100) / 100.0

            ids(i) = "R" & (i + 1).ToString()
            treatment(i) = If(pseudoUniform < p, 1.0, 0.0)
            outcome(i) = 2.0 + 1.4 * treatment(i) + 0.5 * x1 - 0.2 * x2 + 0.1 * x3
            cov(i, 0) = x1
            cov(i, 1) = x2
            cov(i, 2) = x3
        Next

        ' Guard against a pathological deterministic split if this helper is edited later.
        If treatment.All(Function(v) v >= 0.5) OrElse treatment.All(Function(v) v < 0.5) Then
            For i As Integer = 0 To n - 1
                treatment(i) = If((i Mod 2) = 0, 1.0, 0.0)
            Next
        End If

        Return New PsmInputData With {
            .Ids = ids,
            .Treatment = treatment,
            .Outcome = outcome,
            .Covariates = cov,
            .CovariateNames = New String() {"x1", "sin_index", "group_indicator"}
        }
    End Function

    Private Shared Function SuppliedOptions(Optional method As PsmMatchingMethod = PsmMatchingMethod.NearestNeighbor) As PsmOptions
        Return New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.Supplied,
            .MatchingMethod = method,
            .Estimand = PsmEstimand.ATT,
            .DistanceMetric = PsmDistanceMetric.PropensityScore,
            .MatchingRatio = 1,
            .WithReplacement = False,
            .CaliperScale = PsmCaliperScale.RawPropensityScore,
            .Caliper = 0.05,
            .CommonSupport = PsmCommonSupportMode.None,
            .TrimPropensityLower = 0.0,
            .TrimPropensityUpper = 1.0,
            .MatchingOrder = PsmMatchingOrder.PropensityDescending
        }
    End Function

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub SuppliedScoreNearestNeighbor_Att_MatchesExactGroupsAndEstimatesExpectedEffect()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        Dim options As PsmOptions = SuppliedOptions()

        Dim result As PsmResult = PsmBackend.Fit(input, options)

        Assert.IsNotNull(result)
        Assert.IsNotNull(result.ScoreModel)
        Assert.AreEqual(PsmScoreMethod.Supplied, result.ScoreModel.Method)
        Assert.AreEqual(input.RowCount, result.ScoreModel.Scores.Length)
        Assert.AreEqual(4, result.Matches.Count)
        Assert.AreEqual(4, result.SampleSize.MatchedSets)
        Assert.AreEqual(4, result.SampleSize.MatchedTreatedRows)
        Assert.AreEqual(4, result.SampleSize.MatchedControlRows)

        For Each m As PsmMatchLink In result.Matches
            Assert.AreEqual(input.ExactGroupLabels(m.TreatedRowIndex), input.ExactGroupLabels(m.ControlRowIndex), "Exact-group restriction was not respected.")
            Assert.IsTrue(m.PropensityDistance <= options.Caliper + TOL, "A matched pair exceeded the raw propensity-score caliper.")
        Next

        Assert.IsNotNull(result.MatchedEffect)
        Assert.AreEqual(2.5, result.MatchedEffect.Estimate, TOL)
        Assert.AreEqual(4, result.MatchedEffect.MatchedSets)
        Assert.IsTrue(result.Balance.Any(Function(r) r.Sample = PsmBalanceSample.Before))
        Assert.IsTrue(result.Balance.Any(Function(r) r.Sample = PsmBalanceSample.AfterMatching))
        Assert.IsTrue(result.Balance.Any(Function(r) r.Sample = PsmBalanceSample.AfterWeighting))
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub LogisticRegressionScoreModel_UsesGlmPathAndHonorsRidgeOption()
        Dim input As PsmInputData = MakeLogisticInput()
        Dim options As New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.LogisticRegression,
            .MatchingMethod = PsmMatchingMethod.None,
            .Estimand = PsmEstimand.ATE,
            .IncludeIntercept = True,
            .StandardizeCovariates = True,
            .LogisticMaxIterations = 100,
            .LogisticTolerance = 0.0000001,
            .LogisticRidgePenalty = 0.0001,
            .CommonSupport = PsmCommonSupportMode.None,
            .TrimPropensityLower = 0.0,
            .TrimPropensityUpper = 1.0
        }

        Dim score As PsmScoreModelResult = PsmPropensityEstimator.FitLogisticRegression(input, options)

        Assert.IsNotNull(score)
        Assert.AreEqual(PsmScoreMethod.LogisticRegression, score.Method)
        Assert.AreEqual(input.RowCount, score.Scores.Length)
        Assert.AreEqual(input.RowCount, score.LinearPredictor.Length)
        Assert.IsNotNull(score.Coefficients)
        Assert.AreEqual(input.CovariateCount + 1, score.Coefficients.Length, "Intercept plus covariate coefficients expected.")
        AssertVectorFinite(score.Scores, "Logistic propensity scores")
        AssertVectorFinite(score.LinearPredictor, "Logistic linear predictor")
        AssertVectorFinite(score.Coefficients, "Logistic coefficients")
        Assert.IsTrue(score.Scores.All(Function(p) p > 0.0 AndAlso p < 1.0), "All fitted propensity scores should be strictly inside (0,1).")
        Assert.IsFalse(score.Warnings.Any(Function(w) w.IndexOf("ridge", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso w.IndexOf("ignored", StringComparison.OrdinalIgnoreCase) >= 0),
                       "Direct GLM ridge support should not warn that ridge was ignored.")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub ComprehensiveWeightingOnly_ReturnsDiagnosticsLovePlotAndAipw()
        Dim input As PsmInputData = MakeLogisticInput()
        Dim options As New PsmOptions With {
            .ScoreMethod = PsmScoreMethod.LogisticRegression,
            .MatchingMethod = PsmMatchingMethod.None,
            .Estimand = PsmEstimand.ATE,
            .LogisticRidgePenalty = 0.0001,
            .CommonSupport = PsmCommonSupportMode.None,
            .TrimPropensityLower = 0.0,
            .TrimPropensityUpper = 1.0,
            .NormalizeWeightsToSampleSize = True
        }
        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = options,
            .RunMethod = PsmBackendRunMethod.WeightingOnly,
            .IncludeDoublyRobustEstimate = True,
            .IncludeOverlapDiagnostics = True,
            .IncludeWeightDiagnostics = True,
            .IncludeLovePlotRows = True,
            .OverlapBinCount = 8
        }

        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, fitOptions)

        Assert.IsNotNull(fit)
        Assert.IsNotNull(fit.Result)
        Assert.AreEqual(PsmBackendRunMethod.WeightingOnly, fit.RunMethod)
        Assert.IsNotNull(fit.Result.WeightedEffect)
        AssertFinite(fit.Result.WeightedEffect.Estimate, "Weighted effect estimate")
        Assert.IsNotNull(fit.DoublyRobustResult)
        Assert.IsNotNull(fit.DoublyRobustResult.Effect)
        AssertFinite(fit.DoublyRobustResult.Effect.Estimate, "AIPW estimate")
        Assert.IsNotNull(fit.OverlapDiagnostics)
        Assert.IsTrue(fit.OverlapDiagnostics.Bins.Count > 0, "Overlap histogram bins should be populated.")
        Assert.IsTrue(fit.WeightDiagnostics.Count >= 1, "Weight diagnostics should include at least one row.")
        Assert.IsTrue(fit.LovePlotRows.Count >= input.CovariateCount, "Love-plot rows should be available for covariates.")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub Subclassification_SuppliedScoresBuildsStrataAndFiniteEffect()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        Dim options As PsmOptions = SuppliedOptions(PsmMatchingMethod.Subclassification)
        options.CaliperScale = PsmCaliperScale.None
        options.Caliper = Double.NaN
        options.SubclassificationStrata = 3

        Dim result As PsmResult = PsmBackend.Fit(input, options)

        Assert.IsNotNull(result.Subclasses)
        Assert.IsTrue(result.Subclasses.Count > 0, "Subclassification rows should be returned.")
        Assert.IsTrue(result.Subclasses.Any(Function(r) r.TreatedN > 0 AndAlso r.ControlN > 0), "At least one usable subclass should contain both groups.")
        Assert.IsNotNull(result.SubclassificationEffect)
        AssertFinite(result.SubclassificationEffect.Estimate, "Subclassification effect")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub CoarsenedExactMatching_ReturnsRetainedRowsAndWeightedEffect()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        Dim options As PsmOptions = SuppliedOptions()
        options.MatchingMethod = PsmMatchingMethod.None
        options.CaliperScale = PsmCaliperScale.None
        options.Caliper = Double.NaN
        options.Estimand = PsmEstimand.ATT

        Dim coarsening As New PsmCoarseningSpec With {
            .BinCounts = New Integer() {2, 2},
            .IncludePropensityScore = False,
            .Estimand = PsmEstimand.ATT,
            .NormalizeWeightsToSampleSize = True
        }
        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = options,
            .RunMethod = PsmBackendRunMethod.CoarsenedExactMatching,
            .CoarseningSpec = coarsening,
            .IncludeDoublyRobustEstimate = False,
            .IncludeOverlapDiagnostics = False,
            .IncludeWeightDiagnostics = False,
            .IncludeLovePlotRows = False
        }

        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, fitOptions)

        Assert.IsNotNull(fit.CoarsenedExactResult)
        Assert.IsNotNull(fit.CoarsenedExactResult.Weights)
        Assert.AreEqual(input.RowCount, fit.CoarsenedExactResult.Weights.Length)
        Assert.IsTrue(fit.CoarsenedExactResult.Strata.Count > 0, "CEM strata should be populated.")
        Assert.IsTrue(fit.CoarsenedExactResult.RetainedRows > 0, "At least one CEM stratum should be retained.")
        Assert.IsNotNull(fit.Result.WeightedEffect)
        AssertFinite(fit.Result.WeightedEffect.Estimate, "CEM weighted effect")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub OptimalPairMatching_ReturnsOneToOnePairsWithoutReplacement()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        Dim options As PsmOptions = SuppliedOptions()
        options.MatchingRatio = 1
        options.WithReplacement = False

        Dim fitOptions As New PsmComprehensiveFitOptions With {
            .StandardOptions = options,
            .RunMethod = PsmBackendRunMethod.OptimalPairMatching,
            .IncludeDoublyRobustEstimate = False,
            .IncludeOverlapDiagnostics = False,
            .IncludeWeightDiagnostics = False,
            .IncludeLovePlotRows = False
        }

        Dim fit As PsmComprehensiveResult = PsmComprehensiveBackend.Fit(input, fitOptions)

        Assert.IsNotNull(fit.Result)
        Assert.IsNotNull(fit.Result.Matches)
        Assert.AreEqual(4, fit.Result.Matches.Count)
        Assert.AreEqual(fit.Result.Matches.Count, fit.Result.Matches.Select(Function(m) m.ControlRowIndex).Distinct().Count(), "Controls should not be reused in no-replacement optimal matching.")
        Assert.IsNotNull(fit.Result.MatchedEffect)
        AssertFinite(fit.Result.MatchedEffect.Estimate, "Optimal-pair matched effect")
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub RosenbaumSensitivity_ReturnsGammaRowsAndSummaryTable()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        Dim result As PsmResult = PsmBackend.Fit(input, SuppliedOptions())

        Dim sensitivity As PsmRosenbaumSensitivityResult = PsmSensitivityAnalysis.RosenbaumMatchedPairs(
            input,
            result.Matches,
            maxGamma:=2.0,
            gammaStep:=0.5,
            alpha:=0.05,
            alternative:=PsmSensitivityAlternative.TwoSided)

        Assert.IsNotNull(sensitivity)
        Assert.AreEqual(3, sensitivity.Rows.Count)
        Assert.AreEqual(4, sensitivity.InformativePairs)
        Assert.IsTrue(sensitivity.PositiveDifferences > 0)

        Dim table As Object(,) = PsmSensitivityTables.RosenbaumTable(sensitivity)
        Assert.AreEqual("Gamma", CStr(table(0, 0)))
        Assert.AreEqual(sensitivity.Rows.Count + 1, table.GetLength(0))
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub PsmData_RawMatrixImportBuildsReusableInputForGuiAndUdfs()
        Dim modelRaw As Object(,) = ToObjectMatrix(New Double(,) {
            {1, 8.0, 1.0},
            {1, 6.0, 2.0},
            {1, 7.5, 1.5},
            {1, 5.0, 2.5},
            {0, 7.8, 1.1},
            {0, 6.2, 2.2},
            {0, 7.4, 1.6},
            {0, 4.8, 2.4}
        })
        Dim outcomeRaw As Object(,) = ToObjectColumn(New Object() {10.0, 9.0, 11.0, 7.0, 7.0, 6.0, 8.0, 6.0})
        Dim scoreRaw As Object(,) = ToObjectColumn(New Object() {0.80, 0.60, 0.75, 0.50, 0.78, 0.62, 0.74, 0.48})
        Dim idRaw As Object(,) = ToObjectColumn(New Object() {"T1", "T2", "T3", "T4", "C1", "C2", "C3", "C4"})
        Dim exactRaw As Object(,) = ToObjectColumn(New Object() {"A", "A", "B", "B", "A", "A", "B", "B"})

        Dim spec As New PsmDataRawMatrixSpec With {
            .ModelRawInput = modelRaw,
            .ModelVariableNames = New String() {"treat", "x1", "x2"},
            .TreatmentKey = "treat",
            .OutcomeRawInput = outcomeRaw,
            .OutcomeVariableNames = New String() {"outcome"},
            .SelectedCovariateKeys = New List(Of String) From {"x1", "x2"},
            .ScoreMethod = PsmScoreMethod.Supplied,
            .SuppliedScoreRawInput = scoreRaw,
            .SuppliedScoreVariableNames = New String() {"ps"},
            .IdRawInput = idRaw,
            .IdVariableNames = New String() {"id"},
            .ExactGroupRawInput = exactRaw,
            .ExactGroupVariableNames = New String() {"exact"},
            .FirstSourceRow = 1
        }

        Dim importer As New psmData()
        importer.DataImportFromRawMatrices(spec)

        Assert.IsFalse(importer.bZeroValid, "Raw-matrix import should keep valid rows.")
        Assert.IsNotNull(importer.Input)
        Assert.AreEqual(8, importer.Input.RowCount)
        Assert.AreEqual(2, importer.Input.CovariateCount)
        Assert.AreEqual("x1", importer.Input.CovariateNames(0))
        Assert.AreEqual("x2", importer.Input.CovariateNames(1))
        Assert.AreEqual("T1", importer.Input.Ids(0))
        Assert.AreEqual("A", importer.Input.ExactGroupLabels(0))
        Assert.AreEqual(0.8, importer.Input.SuppliedPropensityScores(0), TOL)

        Dim backendResult As PsmResult = PsmBackend.Fit(importer.Input, SuppliedOptions())
        Assert.AreEqual(4, backendResult.Matches.Count)
    End Sub

    <TestMethod()>
    <TestCategory("PropensityScoreMatching")>
    Public Sub ValidationRejectsNonBinaryTreatmentAndInvalidSuppliedScores()
        Dim input As PsmInputData = MakeSuppliedScoreInput()
        input.Treatment(0) = 2.0
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(SuppliedOptions()))

        input = MakeSuppliedScoreInput()
        input.SuppliedPropensityScores(0) = 1.0
        Assert.ThrowsException(Of ArgumentException)(Sub() input.Validate(SuppliedOptions()))
    End Sub

    Private Shared Function ToObjectMatrix(values(,) As Double) As Object(,)
        Dim rows As Integer = values.GetLength(0)
        Dim cols As Integer = values.GetLength(1)
        Dim out(rows - 1, cols - 1) As Object
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                out(i, j) = values(i, j)
            Next
        Next
        Return out
    End Function

    Private Shared Function ToObjectColumn(values() As Object) As Object(,)
        Dim out(values.Length - 1, 0) As Object
        For i As Integer = 0 To values.Length - 1
            out(i, 0) = values(i)
        Next
        Return out
    End Function

End Class
