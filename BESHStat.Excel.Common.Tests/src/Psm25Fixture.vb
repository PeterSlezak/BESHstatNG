Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon

Public NotInheritable Class Psm25Fixture
    Public Shared Function SourceRows(n As Integer) As Integer()
        Return Enumerable.Range(0, n).Select(Function(i) 101 + 3 * i).ToArray()
    End Function
    Public Shared Function Input(profile As Integer) As PsmInputData
        Const n = 12
        Dim x(n - 1, 1) As Double
        For i = 0 To n - 1 : x(i, 0) = i / 10.0R : x(i, 1) = Math.Sin(i * 1.1R) : Next
        Dim result As New PsmInputData With {.Treatment = Enumerable.Range(0, n).Select(Function(i) CDbl(1 - i Mod 2)).ToArray(),
            .Outcome = Enumerable.Range(0, n).Select(Function(i) 5.0R + i / 3.0R + 2 * (1 - i Mod 2)).ToArray(),
            .Covariates = x, .CovariateNames = {"x", "site(B):x"},
            .Ids = Enumerable.Range(0, n).Select(Function(i) If(i Mod 3 = 0, "duplicate", If(i Mod 3 = 1, "", "id-" & i.ToString()))).ToArray(),
            .ExactGroupLabels = Enumerable.Range(0, n).Select(Function(i) If(profile Mod 3 = 0, "", If(i \ 4 Mod 2 = 0, "Ω | ", "Ω | B"))).ToArray(),
            .SuppliedPropensityScores = Enumerable.Range(0, n).Select(Function(i) 0.15R + (i \ 2) * 0.12R + If(i Mod 2 = 0, 0.01R, 0.0R)).ToArray()}
        Select Case profile
            Case 16 : result.SuppliedPropensityScores(0) = Double.NaN
            Case 17 : result.SuppliedPropensityScores(0) = 1
            Case 18 : result.Treatment(0) = 2
            Case 19 : result.Treatment = Enumerable.Repeat(1.0R, n).ToArray()
            Case 20 : result.Treatment = Enumerable.Repeat(0.0R, n).ToArray()
            Case 21 : result.Covariates(0, 0) = Double.PositiveInfinity
            Case 22 : result.Outcome(0) = Double.NaN
            Case 23 : result.ExactGroupLabels = Nothing
            Case 24 : result.ExactGroupLabels(0) = Nothing
            Case 25 : result.Ids = Nothing
            Case 26 : result.SuppliedPropensityScores = Nothing
            Case 27 : result.Outcome = Nothing
            Case 28 : result.ExactGroupLabels = Enumerable.Range(0, n).Select(Function(i) i.ToString()).ToArray()
            Case 29 : result.SuppliedPropensityScores = Enumerable.Repeat(0.5R, n).ToArray()
            Case 30 : result.Ids(0) = "Unicode Ω"
            Case 31 : result.Covariates(0, 1) = Double.NaN
            Case 32 : result.Treatment(0) = Double.NegativeInfinity
            Case 33 : result.SuppliedPropensityScores(0) = -0.1R
            Case 34 : result.Ids = {"wrong length"}
            Case 35 : result.ExactGroupLabels = {"wrong length"}
            Case 36 : result.SuppliedPropensityScores = Enumerable.Repeat(0.5R, n).ToArray()
            Case 40 : result.ExactGroupLabels = Enumerable.Range(0, n).Select(Function(i) If(i < 4, "only treated", "both Ω")).ToArray()
        End Select
        Return result
    End Function
    Public Shared Function Options(method As Integer, estimand As Integer, profile As Integer) As PsmComprehensiveFitOptions
        Dim standard As New PsmOptions With {.ScoreMethod = CType(profile Mod 2, PsmScoreMethod), .Estimand = CType(estimand, PsmEstimand),
            .DistanceMetric = CType(profile Mod 4, PsmDistanceMetric), .MatchingRatio = If(profile Mod 3 = 0, 3, 1), .WithReplacement = profile Mod 2 = 1,
            .CaliperScale = CType((profile \ 2) Mod 5, PsmCaliperScale), .Caliper = If(profile Mod 7 = 0, Double.NaN, If(profile Mod 7 = 1, 0.0R, If(profile Mod 7 = 2, -0.1R, 0.2R))),
            .MatchingOrder = CType(profile Mod 5, PsmMatchingOrder), .CommonSupport = CType(profile Mod 4, PsmCommonSupportMode),
            .IncludeIntercept = True, .StandardizeCovariates = profile Mod 3 <> 0, .LogisticRidgePenalty = 0.0000001R,
            .LogisticMaxIterations = If(profile = 15, 1, 100), .TrimPropensityLower = If(profile Mod 4 = 0, 0.2R, 0.0R), .TrimPropensityUpper = If(profile Mod 4 = 0, 0.8R, 1.0R)}
        If profile = 12 Then standard.Caliper = Double.PositiveInfinity
        If profile = 13 Then standard.LogisticTolerance = -1
        If profile = 14 Then standard.TrimPropensityLower = 0.6R
        Dim fit As New PsmComprehensiveFitOptions With {.StandardOptions = standard, .RunMethod = CType(method, PsmBackendRunMethod)}
        If profile >= 36 Then
            standard = New PsmOptions With {.ScoreMethod = PsmScoreMethod.Supplied, .LogisticRidgePenalty = 0.0000001R}
            fit.StandardOptions = standard
            Select Case profile
                Case 37 : standard.WithReplacement = True : standard.MatchingRatio = 3
                Case 38 : standard.MatchingRatio = 20
                Case 39 : standard.CaliperScale = PsmCaliperScale.RawPropensityScore : standard.Caliper = 0
                Case 41 : standard.CommonSupport = PsmCommonSupportMode.DropOutsideOverlapRange
                Case 42 : standard.TrimPropensityLower = 0.2R : standard.TrimPropensityUpper = 0.8R
                Case 43 : standard.Estimand = PsmEstimand.ATC
                Case 44 : standard.MatchingOrder = PsmMatchingOrder.Random
                Case 45 : standard.DistanceMetric = PsmDistanceMetric.Mahalanobis
                Case 46 : standard.DistanceMetric = PsmDistanceMetric.MahalanobisWithinPropensityCaliper : standard.CaliperScale = PsmCaliperScale.StandardizedLogitPropensityScore : standard.Caliper = 0.2R
                Case 47 : standard.ScoreMethod = PsmScoreMethod.LogisticRegression
            End Select
        End If
        If method = 4 Then fit.CoarseningSpec = New PsmCoarseningSpec With {.DefaultCovariateBins = 5, .PropensityScoreBins = 5, .Estimand = standard.Estimand, .NormalizeWeightsToSampleSize = standard.NormalizeWeightsToSampleSize}
        Return fit
    End Function
    Public Shared Function LegacyTables(section As PsmReportSection, input As PsmInputData, rows As Integer(), result As PsmComprehensiveResult, options As PsmComprehensiveFitOptions, summary As Object(,)) As List(Of ResultTable)
        Select Case section
            Case PsmReportSection.InputData : Return New List(Of ResultTable) From {PsmFormattedResultTables.AnalyzedInputDataTable(input, rows)}
            Case PsmReportSection.Results : Return PsmFormattedResultTables.GeneralResultTables(result, options, summary)
            Case PsmReportSection.Diagnostics : Return PsmFormattedResultTables.DiagnosticsTables(input, result, includeDefaultSensitivity:=True)
            Case PsmReportSection.RowAudit : Return PsmFormattedResultTables.RowAuditTables(input, result, rows)
            Case PsmReportSection.MatchedPairs : Return PsmFormattedResultTables.MatchedPairsTables(input, result)
            Case PsmReportSection.MatchedData : Return PsmFormattedResultTables.MatchedDatasetTables(input, result, rows)
            Case PsmReportSection.Cem : Return PsmFormattedResultTables.CoarsenedExactTables(input, result)
            Case Else : Return New List(Of ResultTable)
        End Select
    End Function
    Public Shared Function LoveRows(id As Integer) As List(Of PsmLovePlotRow)
        If id = 0 Then Return Nothing
        If id = 1 Then Return New List(Of PsmLovePlotRow)
        Dim values = {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, -0.4R, 0.0R, 0.2R, -0.1R, Double.MaxValue}
        Dim names = {"a", "A", "Ω", "x:y", " ", "", Nothing, "a"}
        Dim thresholds = {Double.NaN, -0.2R, 0.0R, Double.PositiveInfinity, 0.1R, 0.3R, Double.MaxValue}
        Dim rows As New List(Of PsmLovePlotRow)
        For i = 0 To If(id = 2, 0, 8)
            rows.Add(New PsmLovePlotRow With {.VariableName = names((i + id) Mod names.Length), .SmdBefore = values((i + id) Mod values.Length), .SmdAfterMatching = values((i * 3 + id \ 2) Mod values.Length), .SmdAfterWeighting = values((i * 5 + id \ 3) Mod values.Length), .Threshold = thresholds((i + id) Mod thresholds.Length), .Flag = If(i Mod 2 = 0, "OK", "imbalance Ω")})
        Next
        If id = 3 Then rows = New List(Of PsmLovePlotRow) From {Nothing, New PsmLovePlotRow With {.VariableName = " "}}
        If id = 4 Then rows = New List(Of PsmLovePlotRow) From {New PsmLovePlotRow With {.VariableName = "only", .SmdBefore = Double.NaN, .SmdAfterMatching = Double.NaN, .SmdAfterWeighting = Double.NaN, .Threshold = 10}}
        rows.Add(Nothing)
        Return rows
    End Function
End Class
