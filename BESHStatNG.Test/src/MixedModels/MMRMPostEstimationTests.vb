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

' ===== BEGIN migrated from MixedModelPostEstimationTests.vb =====



<TestClass()>
Public Class MixedModelPostEstimation_Tests

    Private Const TOL As Double = 0.000000001

    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tol As Double,
                                         Optional message As String = "")
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{message}: expected {expected} but got {actual}.")
        End If

        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{message}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub


    <TestMethod()>
    Public Sub UniqueSortedFiniteValues_DropsNonFiniteAndMergesNearDuplicates()
        Dim values() As Double = {2.0, Double.NaN, 1.0, Double.PositiveInfinity, 2.0 + 0.0000000001, -1.0}
        Dim actual() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(values)

        Assert.AreEqual(3, actual.Length)
        AssertAlmostEqual(-1.0, actual(0), TOL, "first unique value")
        AssertAlmostEqual(1.0, actual(1), TOL, "second unique value")
        AssertAlmostEqual(2.0, actual(2), TOL, "third unique value")
    End Sub


    <TestMethod()>
    Public Sub AverageDesignRowForProfile_ComputesObservedGridMean()
        Dim x(,) As Double = {
            {1.0, 8.0, 0.0, 0.0},
            {1.0, 8.0, 1.0, 8.0},
            {1.0, 10.0, 0.0, 0.0},
            {1.0, 10.0, 1.0, 10.0},
            {1.0, 11.0, 1.0, 11.0},
            {1.0, 12.0, 1.0, 12.0}
        }

        Dim visit() As Double = {1.0, 1.0, 2.0, 2.0, 2.0, 3.0}
        Dim group() As Double = {0.0, 1.0, 0.0, 1.0, 1.0, 1.0}

        Dim actual() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x:=x,
                                                                                     visit:=visit,
                                                                                     groupValues:=group,
                                                                                     targetVisit:=2.0,
                                                                                     targetGroup:=1.0,
                                                                                     rowMask:=Nothing)

        Assert.IsNotNull(actual)
        Assert.AreEqual(4, actual.Length)

        AssertAlmostEqual(1.0, actual(0), TOL, "intercept")
        AssertAlmostEqual(10.5, actual(1), TOL, "age average")
        AssertAlmostEqual(1.0, actual(2), TOL, "group average")
        AssertAlmostEqual(10.5, actual(3), TOL, "interaction average")

        Dim n As Integer = MixedModelPostEstimation.CountProfileRows(visit, group, 2.0, 1.0, Nothing)
        Assert.AreEqual(2, n)
    End Sub


    <TestMethod()>
    Public Sub DirectionHelpers_CreateExpectedContrastsAndLabels()
        Dim treatment() As Double = {1.0, 2.0, 3.0}
        Dim control() As Double = {0.5, 1.0, 1.5}

        Dim diffTC() As Double = MixedModelPostEstimation.MakeDirectedDifference(treatment,
                                                                                 control,
                                                                                 "Treatment - control",
                                                                                 "Treatment - control",
                                                                                 "Control - treatment")

        AssertAlmostEqual(0.5, diffTC(0), TOL, "TC first")
        AssertAlmostEqual(1.0, diffTC(1), TOL, "TC second")
        AssertAlmostEqual(1.5, diffTC(2), TOL, "TC third")

        Dim diffCT() As Double = MixedModelPostEstimation.MakeDirectedDifference(treatment,
                                                                                 control,
                                                                                 "Control - treatment",
                                                                                 "Treatment - control",
                                                                                 "Control - treatment")

        AssertAlmostEqual(-0.5, diffCT(0), TOL, "CT first")
        AssertAlmostEqual(-1.0, diffCT(1), TOL, "CT second")
        AssertAlmostEqual(-1.5, diffCT(2), TOL, "CT third")

        Dim label As String = MixedModelPostEstimation.DirectedComparisonLabel("Group",
                                                                               treatmentLevel:=1.0,
                                                                               controlLevel:=0.0,
                                                                               direction:="Control - treatment",
                                                                               treatmentMinusControlText:="Treatment - control",
                                                                               controlMinusTreatmentText:="Control - treatment")

        Assert.AreEqual("Group=0 - Group=1", label)
    End Sub


    <TestMethod()>
    Public Sub SatterthwaiteDF_FromResultDirectFields_MatchesFormula()
        Dim res As New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {5.0, 2.0},
            .VarBeta = New Double(,) {{4.0, 0.0}, {0.0, 1.0}},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite,
            .BetaDF = New Double() {25.0, 25.0},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }

        res.SatterthwaiteThetaCovariance = New Double(,) {{1.0, 0.0}, {0.0, 1.0}}

        Dim grad(1, 1, 1) As Double
        grad(0, 0, 0) = 0.1
        grad(1, 0, 0) = 0.2
        res.SatterthwaiteVarBetaGradient = grad

        Dim linearRow() As Double = {1.0, 0.0}
        Dim df As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, linearRow)

        ' v = 4; grad(v) = (0.1, 0.2); Var(v) = 0.1^2 + 0.2^2 = 0.05
        ' df = 2 * 4^2 / 0.05 = 640
        AssertAlmostEqual(640.0, df, 0.0000001, "Satterthwaite df")
    End Sub


    <TestMethod()>
    Public Sub SatterthwaiteDF_FromInferenceWorkspaceFallback_MatchesFormula()
        Dim ws As New MixedModelInferenceWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = New Double(,) {{4.0, 0.0}, {0.0, 1.0}},
            .ThetaCovariance = New Double(,) {{1.0, 0.0}, {0.0, 1.0}}
        }

        Dim grad(1, 1, 1) As Double
        grad(0, 0, 0) = 0.1
        grad(1, 0, 0) = 0.2
        ws.VarBetaGradient = grad

        Dim res As New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {5.0, 2.0},
            .VarBeta = ws.VarBeta,
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite,
            .BetaDF = New Double() {25.0, 25.0},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .InferenceWorkspace = ws
        }

        Dim linearRow() As Double = {1.0, 0.0}
        Dim df As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, linearRow)

        AssertAlmostEqual(640.0, df, 0.0000001, "workspace Satterthwaite df")
    End Sub


    <TestMethod()>
    Public Sub BuildLinearEstimateAndContrastTables_ReturnExpectedPValueColumns()
        Dim res As New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {5.0, 2.0},
            .VarBeta = New Double(,) {{4.0, 0.0}, {0.0, 1.0}},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.ResidualDF,
            .BetaDF = New Double() {20.0, 20.0},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }

        Dim lRows As New List(Of Double()) From {
            New Double() {1.0, 0.0},
            New Double() {1.0, 1.0}
        }

        Dim labels() As String = {"Intercept", "Intercept + slope"}
        Dim counts As New List(Of Integer) From {10, 10}

        Dim estimates As ResultTable = MixedModelPostEstimation.BuildLinearEstimateResultTable(title:="Test estimated means",
                                                                                                rowLabels:=labels,
                                                                                                lRows:=lRows,
                                                                                                counts:=counts,
                                                                                                result:=res,
                                                                                                alpha:=0.05,
                                                                                                footnote:="unit test")

        Assert.IsNotNull(estimates)
        Assert.AreEqual(1, estimates.HeadersTopCount)
        Assert.AreEqual(1, estimates.HeadersLeftCount)
        Assert.IsTrue(estimates.PvalColumns.Contains(6), "Estimated-mean p-value body column should be 6.")

        Dim contrasts As ResultTable = MixedModelPostEstimation.BuildLinearContrastResultTable(title:="Test contrasts",
                                                                                                rowLabels:=labels,
                                                                                                lRows:=lRows,
                                                                                                result:=res,
                                                                                                alpha:=0.05,
                                                                                                footnote:="unit test")

        Assert.IsNotNull(contrasts)
        Assert.AreEqual(1, contrasts.HeadersTopCount)
        Assert.AreEqual(1, contrasts.HeadersLeftCount)
        Assert.IsTrue(contrasts.PvalColumns.Contains(5), "Contrast p-value body column should be 5.")
    End Sub

End Class

' ===== END migrated from MixedModelPostEstimationTests.vb =====

' ===== BEGIN migrated from MMRMPostEstimationModuleTests.vb =====



<TestClass()>
Public Class MMRMPostEstimationModule_Tests

    Private Shared Function BuildSimpleResult() As MixedModelResult
        Return New MixedModelResult With {
            .P = 4,
            .Beta = New Double() {10.0, 1.0, 2.0, 0.5},
            .VarBeta = New Double(,) {
                {0.04, 0.0, 0.0, 0.0},
                {0.0, 0.04, 0.0, 0.0},
                {0.0, 0.0, 0.01, 0.0},
                {0.0, 0.0, 0.0, 0.01}
            },
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.ResidualDF,
            .BetaDF = New Double() {20.0, 20.0, 20.0, 20.0},
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)"
        }
    End Function


    Private Shared Sub BuildDesign(ByRef x(,) As Double,
                                   ByRef visit() As Double,
                                   ByRef groupValues() As Double)
        visit = New Double() {1.0, 1.0, 2.0, 2.0, 3.0, 3.0}
        groupValues = New Double() {0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        ReDim x(visit.Length - 1, 3)
        For i As Integer = 0 To visit.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = groupValues(i)
            x(i, 2) = visit(i)
            x(i, 3) = groupValues(i) * visit(i)
        Next
    End Sub


    <TestMethod()>
    Public Sub BuildEstimatedMeansTables_CreateExpectedResultTables()
        Dim res As MixedModelResult = BuildSimpleResult()
        Dim x(,) As Double = Nothing
        Dim visit() As Double = Nothing
        Dim groupValues() As Double = Nothing
        BuildDesign(x, visit, groupValues)

        Dim byVisit As ResultTable = MMRMPostEstimation.BuildEstimatedMeansByVisitTable(res, x, visit, 0.05)
        Assert.IsNotNull(byVisit)
        Assert.IsTrue(byVisit.PvalColumns.Contains(6), "Estimated means table p-value column should be 6.")

        Dim byVisitGroup As ResultTable = MMRMPostEstimation.BuildEstimatedMeansByVisitAndGroupTable(res, x, visit, groupValues, "Group", 0.05)
        Assert.IsNotNull(byVisitGroup)
        Assert.IsTrue(byVisitGroup.PvalColumns.Contains(6), "Estimated means by group table p-value column should be 6.")
    End Sub


    <TestMethod()>
    Public Sub BuildControlledGroupDifference_UsesSelectedComparisonAndDirection()
        Dim res As MixedModelResult = BuildSimpleResult()
        Dim x(,) As Double = Nothing
        Dim visit() As Double = Nothing
        Dim groupValues() As Double = Nothing
        BuildDesign(x, visit, groupValues)

        Dim t As ResultTable = MMRMPostEstimation.BuildVisitGroupDifferencesTableControlled(result:=res,
                                                                                            x:=x,
                                                                                            visit:=visit,
                                                                                            groupValues:=groupValues,
                                                                                            groupName:="Group",
                                                                                            alpha:=0.05,
                                                                                            contrastMode:=MMRMPostEstimation.MODE_SELECTED,
                                                                                            controlLevel:=0.0,
                                                                                            comparisonLevel:=1.0,
                                                                                            direction:=MMRMPostEstimation.DIR_TREATMENT_MINUS_CONTROL)

        Assert.IsNotNull(t)
        Assert.IsTrue(t.PvalColumns.Contains(5), "Contrast table p-value column should be 5.")
    End Sub


    <TestMethod()>
    Public Sub BuildChangeFromBaselineTables_CreateExpectedResultTables()
        Dim res As MixedModelResult = BuildSimpleResult()
        Dim x(,) As Double = Nothing
        Dim visit() As Double = Nothing
        Dim groupValues() As Double = Nothing
        BuildDesign(x, visit, groupValues)

        Dim change As ResultTable = MMRMPostEstimation.BuildChangeFromBaselineTableControlled(res, x, visit, baseline:=1.0, alpha:=0.05)
        Assert.IsNotNull(change)
        Assert.IsTrue(change.PvalColumns.Contains(5), "Change-from-baseline p-value column should be 5.")

        Dim changeGroup As ResultTable = MMRMPostEstimation.BuildChangeFromBaselineByGroupTableControlled(res, x, visit, groupValues, "Group", baseline:=1.0, alpha:=0.05)
        Assert.IsNotNull(changeGroup)
        Assert.IsTrue(changeGroup.PvalColumns.Contains(5), "Change-from-baseline by group p-value column should be 5.")
    End Sub


    <TestMethod()>
    Public Sub BuildDifferenceInChange_SelectedComparison_CreateExpectedResultTable()
        Dim res As MixedModelResult = BuildSimpleResult()
        Dim x(,) As Double = Nothing
        Dim visit() As Double = Nothing
        Dim groupValues() As Double = Nothing
        BuildDesign(x, visit, groupValues)

        Dim t As ResultTable = MMRMPostEstimation.BuildDifferenceInChangeFromBaselineTableControlled(result:=res,
                                                                                                      x:=x,
                                                                                                      visit:=visit,
                                                                                                      groupValues:=groupValues,
                                                                                                      groupName:="Group",
                                                                                                      baseline:=1.0,
                                                                                                      alpha:=0.05,
                                                                                                      contrastMode:=MMRMPostEstimation.MODE_SELECTED,
                                                                                                      controlLevel:=0.0,
                                                                                                      comparisonLevel:=1.0,
                                                                                                      direction:=MMRMPostEstimation.DIR_TREATMENT_MINUS_CONTROL)

        Assert.IsNotNull(t)
        Assert.IsTrue(t.PvalColumns.Contains(5), "Difference-in-change p-value column should be 5.")
    End Sub

End Class

' ===== END migrated from MMRMPostEstimationModuleTests.vb =====

' ===== BEGIN migrated from MMRMOrthodontPostEstimationTests.vb =====



''' <summary>
''' End-to-end MMRM post-estimation tests using the Orthodont / Potthoff-Roy data.
''' </summary>
''' <remarks>
''' These tests intentionally use the numerical engine and reusable post-estimation
''' helpers directly, not Ui18MMRM.  They protect the future UDF path:
'''
'''     data -> MixedModelBlockData -> MMRM -> MixedModelResult -> MixedModelPostEstimation
'''
''' The GUI should remain only a form-control/workbook-output layer on top of this path.
''' </remarks>
<TestClass()>
Public Class MMRMOrthodontPostEstimationTests

    Private Const TOL_BETA As Double = 0.001
    Private Const TOL_DF As Double = 0.000001

    <TestMethod()>
    Public Sub Orthodont_UN_BetweenWithin_FixedEffectsAndRMatrix_AreStable()
        Dim dat As OrthodontData = LoadOrthodontData("mmrm_orthodont_potthoffroy_long.csv")
        Dim x(,) As Double = BuildOrthodontX(dat)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Distance,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)
        req.RequestLabel = "Orthodont MMRM UN post-estimation reference"
        req.ResponseVarName = "distance"
        req.SubjectVarName = "Subject"
        req.VisitVarName = "visit"
        req.FixedEffectNames = {"Intercept", "SexCode", "age", "SexCode:age"}
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.BetweenWithin
        req.Control = IntegrationControl()

        Dim fit As New MMRM(req)
        Dim res As MixedModelResult = fit.Fit()

        AssertBasicMMRMResult(res, expectedP:=4, expectedN:=dat.Distance.Length, expectedSubjects:=27)

        ' Balanced Orthodont design.  The fixed-effect estimates match the standard
        ' SAS/R model distance = Intercept + SexCode + age + SexCode:age for this coding:
        ' Female = 1, Male = 0.
        Assert.AreEqual(15.842346, res.Beta(0), TOL_BETA, "Intercept mismatch.")
        Assert.AreEqual(1.583018, res.Beta(1), TOL_BETA, "SexCode mismatch.")
        Assert.AreEqual(0.826797, res.Beta(2), TOL_BETA, "age mismatch.")
        Assert.AreEqual(-0.350432, res.Beta(3), TOL_BETA, "SexCode:age mismatch.")
        Assert.AreEqual(0.9723186, res.BetaSE(0), TOL_BETA, "SE Intercept mismatch.")
        Assert.AreEqual(1.5233306, res.BetaSE(1), TOL_BETA, "SE SexCode mismatch.")
        Assert.AreEqual(0.0822176, res.BetaSE(2), TOL_BETA, "SE age mismatch.")
        Assert.AreEqual(0.1288102, res.BetaSE(3), TOL_BETA, "SE SexCode:age mismatch.")

        Assert.IsNotNull(res.BetaDF, "BetaDF should be populated for Between-within inference.")
        Assert.AreEqual(4, res.BetaDF.Length, "Unexpected BetaDF length.")

        ' R mmrm-style Between-within df does not apply the historical SAS-style
        ' unstructured-covariance exception.  The intercept and terms that vary
        ' within subject use within-subject df; subject-constant terms use
        ' between-subject df.  For the Orthodont design this gives:
        '   Intercept     -> within  = 108 - 27 - 2 = 79
        '   SexCode       -> between = 27 - 2 = 25
        '   age           -> within  = 79
        '   SexCode:age   -> within  = 79
        Dim expectedDf() As Double = {79.0, 25.0, 79.0, 79.0}
        For j As Integer = 0 To res.BetaDF.Length - 1
            Assert.AreEqual(expectedDf(j), res.BetaDF(j), TOL_DF,
                            "R mmrm-style Between-within DF mismatch for " & req.FixedEffectNames(j) & ".")
        Next

        Assert.AreEqual("t", res.BetaStatisticLabel, "Between-within inference should report t statistics.")
        Assert.AreEqual("Pr(>|t|)", res.BetaPValueLabel, "Between-within inference should report t p-values.")

        Assert.IsNotNull(res.ResidualCovarianceUserScale, "User-scale R covariance matrix should be populated.")
        Assert.AreEqual(4, res.ResidualCovarianceUserScale.GetLength(0), "Unexpected R covariance row dimension.")
        Assert.AreEqual(4, res.ResidualCovarianceUserScale.GetLength(1), "Unexpected R covariance column dimension.")

        Assert.IsNotNull(res.ResidualCorrelationUserScale, "User-scale R correlation matrix should be populated.")
        For i As Integer = 0 To 3
            Assert.AreEqual(1.0, res.ResidualCorrelationUserScale(i, i), 0.0000001,
                            "Residual correlation diagonal should be one.")
        Next
    End Sub


    <TestMethod()>
    Public Sub Orthodont_PostEstimation_LSMeansAndContrasts_UseReusableBackend()
        Dim dat As OrthodontData = LoadOrthodontData("mmrm_orthodont_potthoffroy_long.csv")
        Dim x(,) As Double = BuildOrthodontX(dat)

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=dat.Distance,
                                                                              x:=x,
                                                                              subjectId:=dat.Subject,
                                                                              z:=Nothing,
                                                                              visit:=dat.Visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New UnstructuredR(),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "SexCode", "age", "SexCode:age"}
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.BetweenWithin
        req.Control = IntegrationControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()
        AssertBasicMMRMResult(res, expectedP:=4, expectedN:=dat.Distance.Length, expectedSubjects:=27)

        Dim visits() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(dat.Visit)
        Dim groups() As Double = MixedModelPostEstimation.UniqueSortedFiniteValues(dat.SexCode)

        Assert.AreEqual(4, visits.Length, "Orthodont should have four visits.")
        Assert.AreEqual(2, groups.Length, "Orthodont SexCode should have two levels.")

        Dim rowLabels As New List(Of String)
        Dim lRows As New List(Of Double())
        Dim counts As New List(Of Integer)

        For Each v As Double In visits
            For Each g As Double In groups
                Dim l() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x:=x,
                                                                                        visit:=dat.Visit,
                                                                                        groupValues:=dat.SexCode,
                                                                                        targetVisit:=v,
                                                                                        targetGroup:=g,
                                                                                        rowMask:=Nothing)
                Assert.IsNotNull(l, "Expected an LS-mean row for visit/group profile.")

                rowLabels.Add("visit=" & MixedModelPostEstimation.FormatProfileValue(v) &
                              ", SexCode=" & MixedModelPostEstimation.FormatProfileValue(g))
                lRows.Add(l)
                counts.Add(MixedModelPostEstimation.CountProfileRows(dat.Visit, dat.SexCode, v, g, Nothing))

                Dim est As Double = MixedModelPostEstimation.LinearEstimate(l, res.Beta)
                AssertFinite(est, "linear estimate for " & rowLabels(rowLabels.Count - 1))

                Dim varEst As Double = MixedModelPostEstimation.LinearCombinationVariance(l, res.VarBeta)
                Assert.IsTrue(varEst > 0.0, "Linear-estimate variance should be positive.")
            Next
        Next

        Dim meansTable As ResultTable = MixedModelPostEstimation.BuildLinearEstimateResultTable(
            title:="Orthodont estimated marginal means by visit and SexCode",
            rowLabels:=rowLabels.ToArray(),
            lRows:=lRows,
            counts:=counts,
            result:=res,
            alpha:=0.05,
            footnote:="unit-test observed design grid")

        Assert.IsNotNull(meansTable, "Estimated means table should be created.")
        Assert.IsTrue(meansTable.PvalColumns.Contains(6),
                      "Estimated means table should mark the p-value body column.")

        ' Group difference at visit 1: Female - Male, using the observed design rows.
        Dim lMaleVisit1() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, dat.Visit, dat.SexCode, 1.0, 0.0, Nothing)
        Dim lFemaleVisit1() As Double = MixedModelPostEstimation.AverageDesignRowForProfile(x, dat.Visit, dat.SexCode, 1.0, 1.0, Nothing)
        Dim diffRow() As Double = Matrix.M_SUB(lFemaleVisit1, lMaleVisit1)

        Dim diffEstimate As Double = MixedModelPostEstimation.LinearEstimate(diffRow, res.Beta)

        ' For visit 1, age = 8, so Female - Male = beta_SexCode + 8 * beta_SexCode:age.
        Dim expectedDiff As Double = res.Beta(1) + 8.0 * res.Beta(3)
        Assert.AreEqual(expectedDiff, diffEstimate, 0.0000001,
                        "Female-Male contrast at visit 1 should equal beta_SexCode + age*interaction.")

        Dim contrastRows As New List(Of Double()) From {diffRow}
        Dim contrastLabels() As String = {"Visit 1: SexCode=1 - SexCode=0"}

        Dim contrastTable As ResultTable = MixedModelPostEstimation.BuildLinearContrastResultTable(
            title:="Orthodont selected group difference by visit",
            rowLabels:=contrastLabels,
            lRows:=contrastRows,
            result:=res,
            alpha:=0.05,
            footnote:="unit-test contrast")

        Assert.IsNotNull(contrastTable, "Contrast table should be created.")
        Assert.IsTrue(contrastTable.PvalColumns.Contains(5),
                      "Contrast table should mark the p-value body column.")
    End Sub


    <TestMethod()>
    Public Sub SimpleMMRM_Satterthwaite_PopulatesInferenceWorkspace_AndLinearDF()
        Dim y() As Double = {1.1, 2.9,
                             0.8, 3.2,
                             1.1, 2.9,
                             1.0, 3.1}

        Dim subjectId() As Object = {"S1", "S1", "S2", "S2", "S3", "S3", "S4", "S4"}
        Dim visit() As Double = {0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0}

        Dim x(y.Length - 1, 1) As Double
        For i As Integer = 0 To y.Length - 1
            x(i, 0) = 1.0
            x(i, 1) = visit(i)
        Next

        Dim blockData As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                              x:=x,
                                                                              subjectId:=subjectId,
                                                                              z:=Nothing,
                                                                              visit:=visit,
                                                                              sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(blockData,
                                                                          New IdentityR(),
                                                                          MixedModelFitMethod.REML)
        req.FixedEffectNames = {"Intercept", "Visit"}
        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite
        req.UseSatterthwaite = True
        req.Control = IntegrationControl()

        Dim res As MixedModelResult = (New MMRM(req)).Fit()

        AssertBasicMMRMResult(res, expectedP:=2, expectedN:=y.Length, expectedSubjects:=4)
        Assert.AreEqual(MixedModelFixedInferenceMethod.Satterthwaite, res.FixedInferenceMethod,
                        "Result should report Satterthwaite inference.")

        Assert.IsNotNull(res.InferenceWorkspace,
                         "Satterthwaite fit should populate the universal inference workspace.")
        Assert.IsNotNull(res.SatterthwaiteThetaCovariance,
                         "Satterthwaite fit should populate theta covariance.")
        Assert.IsNotNull(res.SatterthwaiteVarBetaGradient,
                         "Satterthwaite fit should populate Var(beta) gradient matrices.")

        Dim lIntercept() As Double = {1.0, 0.0}
        Dim df As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, lIntercept)

        AssertFinite(df, "Satterthwaite linear-combination DF")
        Assert.IsTrue(df > 0.0, "Satterthwaite linear-combination DF should be positive.")
    End Sub


    Private Shared Function IntegrationControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 120
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function BuildOrthodontX(dat As OrthodontData) As Double(,)
        Dim n As Integer = dat.Distance.Length
        Dim x(n - 1, 3) As Double

        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.SexCode(i)
            x(i, 2) = dat.Age(i)
            x(i, 3) = dat.SexCode(i) * dat.Age(i)
        Next

        Return x
    End Function


    Private Shared Sub AssertBasicMMRMResult(res As MixedModelResult,
                                             expectedP As Integer,
                                             expectedN As Integer,
                                             expectedSubjects As Integer)
        Assert.IsNotNull(res, "MMRM result should not be Nothing.")
        Assert.AreEqual(expectedP, res.P, "Unexpected fixed-effect dimension.")
        Assert.AreEqual(expectedN, res.Nobs, "Unexpected observation count.")
        Assert.AreEqual(expectedSubjects, res.NoSubjects, "Unexpected subject count.")

        Assert.IsNotNull(res.Beta, "Beta should not be Nothing.")
        Assert.AreEqual(expectedP, res.Beta.Length, "Unexpected beta length.")

        Assert.IsNotNull(res.BetaSE, "BetaSE should not be Nothing.")
        Assert.AreEqual(expectedP, res.BetaSE.Length, "Unexpected beta SE length.")

        For i As Integer = 0 To res.Beta.Length - 1
            AssertFinite(res.Beta(i), "Beta[" & i.ToString(CultureInfo.InvariantCulture) & "]")
            AssertFinite(res.BetaSE(i), "BetaSE[" & i.ToString(CultureInfo.InvariantCulture) & "]")
        Next

        AssertFinite(res.LogLik, "LogLik")
        AssertFinite(res.Objective, "Objective")
    End Sub


    Private Shared Sub AssertFinite(value As Double, label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub


    Private Shared Function LoadOrthodontData(fileName As String) As OrthodontData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException("Orthodont CSV must contain a header and data rows.")
        End If

        Dim header() As String = lines(0).Split(","c)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        RequireColumn(col, "Subject", fileName)
        RequireColumn(col, "SexCode", fileName)
        RequireColumn(col, "visit", fileName)
        RequireColumn(col, "age", fileName)
        RequireColumn(col, "distance", fileName)

        Dim n As Integer = lines.Length - 1
        Dim subject(n - 1) As Object
        Dim sexCode(n - 1) As Double
        Dim visit(n - 1) As Double
        Dim age(n - 1) As Double
        Dim distance(n - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts() As String = lines(i + 1).Split(","c)

            subject(i) = parts(col("Subject")).Trim()
            sexCode(i) = ParseD(parts(col("SexCode")))
            visit(i) = ParseD(parts(col("visit")))
            age(i) = ParseD(parts(col("age")))
            distance(i) = ParseD(parts(col("distance")))
        Next

        Return New OrthodontData With {
            .Subject = subject,
            .SexCode = sexCode,
            .Visit = visit,
            .Age = age,
            .Distance = distance
        }
    End Function


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


    Private Shared Sub RequireColumn(col As Dictionary(Of String, Integer),
                                     columnName As String,
                                     fileName As String)
        If Not col.ContainsKey(columnName) Then
            Throw New InvalidOperationException("CSV file '" & fileName & "' must contain column '" & columnName & "'.")
        End If
    End Sub


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Class OrthodontData
        Public Subject() As Object
        Public SexCode() As Double
        Public Visit() As Double
        Public Age() As Double
        Public Distance() As Double
    End Class

End Class

' ===== END migrated from MMRMOrthodontPostEstimationTests.vb =====

' ===== BEGIN migrated from MixedModelPostEstimationKRLinearInferenceTests.vb =====



<TestClass()>
Public Class MixedModelPostEstimationKRLinearInferenceTests

    <TestMethod()>
    Public Sub TryLinearInference_KenwardRoger_UsesAdjustedVariance()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim est As Double = Double.NaN
        Dim se As Double = Double.NaN
        Dim df As Double = Double.NaN
        Dim stat As Double = Double.NaN
        Dim pv As Double = Double.NaN
        Dim lo As Double = Double.NaN
        Dim hi As Double = Double.NaN
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelPostEstimation.TryLinearInference(res,
                                                                  "x",
                                                                  New Double() {0.0, 1.0},
                                                                  alpha:=0.05,
                                                                  estimate:=est,
                                                                  standardError:=se,
                                                                  df:=df,
                                                                  statistic:=stat,
                                                                  pValue:=pv,
                                                                  lowerCI:=lo,
                                                                  upperCI:=hi,
                                                                  diagnostic:=msg), msg)

        Assert.AreEqual(2.0, est, 0.0000000001)

        ' Ordinary variance for beta_1 is 4.0, but KR adjusted variance is 9.0.
        Assert.AreEqual(3.0, se, 0.0000000001)
        Assert.AreEqual(est / se, stat, 0.0000000001)

        Assert.IsFalse(Double.IsNaN(df), "KR denominator DF should be finite.")
        Assert.IsFalse(Double.IsInfinity(df), "KR denominator DF should be finite.")
        Assert.IsTrue(df > 0.0, "KR denominator DF should be positive.")

        Assert.IsFalse(Double.IsNaN(pv), "KR p-value should be finite.")
        Assert.IsTrue(pv >= 0.0 AndAlso pv <= 1.0, "KR p-value should be in [0,1].")
    End Sub


    <TestMethod()>
    Public Sub ResolveLinearEstimateDF_KenwardRoger_UsesLinearRowSpecificDF()
        Dim res As MixedModelResult = BuildTwoCoefficientKrResult()

        Dim df1 As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, New Double() {1.0, 0.0})
        Dim df2 As Double = MixedModelPostEstimation.ResolveLinearEstimateDF(res, New Double() {0.0, 1.0})

        Assert.IsFalse(Double.IsNaN(df1))
        Assert.IsFalse(Double.IsNaN(df2))
        Assert.IsTrue(df1 > 0.0)
        Assert.IsTrue(df2 > 0.0)

        ' The P matrices are intentionally different for beta0 and beta1, so
        ' row-specific KR DF should differ.
        Assert.AreNotEqual(df1, df2, 0.000001)
    End Sub


    Private Shared Function BuildTwoCoefficientKrResult() As MixedModelResult
        Dim phi(1, 1) As Double
        phi(0, 0) = 1.0
        phi(1, 1) = 4.0

        Dim adjusted(1, 1) As Double
        adjusted(0, 0) = 1.5
        adjusted(1, 1) = 9.0

        Dim thetaCov(1, 1) As Double
        thetaCov(0, 0) = 0.02
        thetaCov(1, 1) = 0.04
        thetaCov(0, 1) = 0.005
        thetaCov(1, 0) = 0.005

        Dim pMats(1, 1, 1) As Double
        pMats(0, 0, 0) = 0.3
        pMats(0, 1, 1) = 0.1
        pMats(1, 0, 0) = 0.05
        pMats(1, 1, 1) = 0.4

        Dim ws As New MixedModelKrWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = 2,
            .K = 2,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .P = 2,
            .Beta = New Double() {10.0, 2.0},
            .VarBeta = phi,
            .BetaDF = New Double() {100.0, 100.0},
            .FixedEffectNames = New String() {"Intercept", "x"},
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger,
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .InferenceWorkspace = inf
        }
    End Function

End Class

' ===== END migrated from MixedModelPostEstimationKRLinearInferenceTests.vb =====

' ===== BEGIN migrated from MixedModelKRPostEstimationModelReferenceTests.vb =====



' Model-level validation for KR-aware post-estimation linear inference.
'
' Uses the deterministic non-balanced sleepstudy random-slope data and R
' lme4 + pbkrtest reference constants already generated for the fixed-effect
' KR adjusted covariance validation.
'
' The selected post-estimation rows deliberately avoid the fixed-effect covariance
' off-diagonal:
'
'   LSMean days=0      L = [1, 0]
'   Change 9 - 0 days  L = [0, 9]
'   Change 3 - 0 days  L = [0, 3]
'
' so their R references can be computed directly from:
'
'   beta_intercept, beta_days
'   sqrt(diag(pbkrtest::vcovAdj(fit)))
'
' This validates that MixedModelPostEstimation.TryLinearInference and the
' post-estimation table builders use KRAdjustedVarBeta when the result's fixed
' inference method is KenwardRoger.
<TestClass()>
Public Class MixedModelKRPostEstimationModelReferenceTests

    Private Const ESTIMATE_TOL As Double = 0.0002
    Private Const SE_TOL As Double = 0.002

    <TestMethod()>
    Public Sub SleepstudyUnbalanced_PostEstimationLinearRows_MatchPbkrtestReference()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlopeWithKRFixedInference(dat)

        AssertUsableKRFit(res)

        Dim refs As List(Of LinearReferenceRow) = GetHardCodedPostEstimationRows()

        For Each expected As LinearReferenceRow In refs
            Dim est As Double = Double.NaN
            Dim se As Double = Double.NaN
            Dim df As Double = Double.NaN
            Dim stat As Double = Double.NaN
            Dim pValue As Double = Double.NaN
            Dim lo As Double = Double.NaN
            Dim hi As Double = Double.NaN
            Dim msg As String = Nothing

            Assert.IsTrue(MixedModelPostEstimation.TryLinearInference(res,
                                                                      expected.Label,
                                                                      expected.L,
                                                                      alpha:=0.05,
                                                                      estimate:=est,
                                                                      standardError:=se,
                                                                      df:=df,
                                                                      statistic:=stat,
                                                                      pValue:=pValue,
                                                                      lowerCI:=lo,
                                                                      upperCI:=hi,
                                                                      diagnostic:=msg),
                          "TryLinearInference failed for " & expected.Label & ": " & msg)

            AssertAlmostEqual(expected.Estimate, est, ESTIMATE_TOL, expected.Label & " estimate")
            AssertAlmostEqual(expected.KRAdjustedSE, se, SE_TOL, expected.Label & " KR adjusted SE")

            AssertFinite(df, expected.Label & " KR approximate df")
            Assert.IsTrue(df > 0.0, expected.Label & " KR approximate df should be positive.")

            AssertFinite(stat, expected.Label & " t statistic")
            AssertFinite(pValue, expected.Label & " p-value")
            Assert.IsTrue(pValue >= 0.0 AndAlso pValue <= 1.0, expected.Label & " p-value should be in [0,1].")
        Next
    End Sub


    <TestMethod()>
    Public Sub SleepstudyUnbalanced_PostEstimationTables_BuildWithKRLabelsAndPColumns()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlopeWithKRFixedInference(dat)

        AssertUsableKRFit(res)

        Dim rows As List(Of LinearReferenceRow) = GetHardCodedPostEstimationRows()

        Dim lsLabels() As String = {rows(0).Label}
        Dim lsRows As New List(Of Double()) From {rows(0).L}
        Dim counts As New List(Of Integer) From {res.Nobs}

        Dim lsTable As ResultTable =
            MixedModelPostEstimation.BuildLinearEstimateResultTable("KR post-estimation LS-mean reference check",
                                                                    lsLabels,
                                                                    lsRows,
                                                                    counts,
                                                                    res,
                                                                    alpha:=0.05,
                                                                    footnote:="Test table.")

        Assert.IsNotNull(lsTable, "LS-mean table should be built.")
        Assert.IsTrue(lsTable.PvalColumns.Contains(6), "LS-mean table should mark the p-value column.")
        Assert.IsTrue(lsTable.FootersCount > 0, "LS-mean table should include KR footnote text.")

        Dim contrastLabels() As String = {rows(1).Label, rows(2).Label}
        Dim contrastRows As New List(Of Double()) From {rows(1).L, rows(2).L}

        Dim contrastTable As ResultTable =
            MixedModelPostEstimation.BuildLinearContrastResultTable("KR post-estimation contrast reference check",
                                                                    contrastLabels,
                                                                    contrastRows,
                                                                    res,
                                                                    alpha:=0.05,
                                                                    footnote:="Test table.")

        Assert.IsNotNull(contrastTable, "Contrast table should be built.")
        Assert.IsTrue(contrastTable.PvalColumns.Contains(5), "Contrast table should mark the p-value column.")
        Assert.IsTrue(contrastTable.FootersCount > 0, "Contrast table should include KR footnote text.")

        Dim assembled As Object(,) = contrastTable.returnSelf()
        Assert.IsNotNull(assembled, "Assembled contrast table should not be Nothing.")
        Assert.IsTrue(assembled.GetLength(0) > 2, "Assembled contrast table should contain headers and body.")
        Assert.IsTrue(assembled.GetLength(1) >= 8, "Assembled contrast table should contain left header plus inference columns.")
    End Sub

    <TestMethod()>
    Public Sub SleepstudyUnbalanced_JointInterceptAndSlope_KRMultiDfF_IsFinite()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlopeWithKRFixedInference(dat)

        AssertUsableKRFit(res)

        Dim l(1, 1) As Double
        l(0, 0) = 1.0
        l(1, 1) = 1.0

        Dim fTest As MixedModelKenwardRogerMultiDfInference = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                      "joint intercept and days",
                                                                      l,
                                                                      fTest,
                                                                      alpha:=0.05,
                                                                      diagnostic:=msg), msg)

        Assert.IsNotNull(fTest)
        Assert.AreEqual(2.0, fTest.NumDF, 0.0000000001)
        AssertFinite(fTest.DenDF, "KR multi-df denominator DF")
        Assert.IsTrue(fTest.DenDF > 0.0, "KR multi-df denominator DF should be positive.")
        AssertFinite(fTest.FStatistic, "KR multi-df F statistic")
        Assert.IsTrue(fTest.FStatistic >= 0.0, "KR multi-df F statistic should be nonnegative.")
        AssertFinite(fTest.PValue, "KR multi-df p-value")
        Assert.IsTrue(fTest.PValue >= 0.0 AndAlso fTest.PValue <= 1.0, "KR multi-df p-value should be in [0,1].")
        AssertFinite(fTest.Scaling, "KR multi-df F scaling")
        Assert.IsTrue(fTest.Scaling > 0.0, "KR multi-df F scaling should be positive.")
    End Sub


    <TestMethod()>
    Public Sub SleepstudyUnbalanced_TermHypothesisBuilder_CreatesDaysFTest()
        Dim dat As SleepstudyData = LoadSleepstudyCsv("mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv")
        Dim res As MixedModelResult = FitSleepstudyRandomSlopeWithKRFixedInference(dat)

        AssertUsableKRFit(res)

        Dim h As MixedModelMultiDfHypothesis = Nothing
        Dim msg As String = Nothing

        Assert.IsTrue(MixedModelHypothesisBuilder.TryBuildTermHypothesis(res.FixedEffectNames,
                                                                     "days",
                                                                     h,
                                                                     diagnostic:=msg), msg)

        Assert.IsNotNull(h)
        Assert.AreEqual("days", h.Label)
        Assert.AreEqual(1, h.L.GetLength(0))
        Assert.AreEqual(2, h.L.GetLength(1))
        Assert.AreEqual(0.0, h.L(0, 0), 0.0)
        Assert.AreEqual(1.0, h.L(0, 1), 0.0)

        Dim fTest As MixedModelKenwardRogerMultiDfInference = Nothing

        Assert.IsTrue(MixedModelKenwardRogerInference.TryComputeKrFTest(res,
                                                                      h.Label,
                                                                      h.L,
                                                                      fTest,
                                                                      alpha:=0.05,
                                                                      diagnostic:=msg), msg)

        Assert.IsNotNull(fTest)
        Assert.AreEqual(1.0, fTest.NumDF, 0.0000000001)
        AssertFinite(fTest.DenDF, "days term denominator DF")
        AssertFinite(fTest.FStatistic, "days term F statistic")
        AssertFinite(fTest.PValue, "days term p-value")
    End Sub

    Private Shared Function FitSleepstudyRandomSlopeWithKRFixedInference(dat As SleepstudyData) As MixedModelResult
        Dim x(,) As Double = BuildFixedDesign(dat)
        Dim z(,) As Double = BuildRandomSlopeDesign(dat)

        Dim blockData As MixedModelBlockData =
            MixedModelBlockData.FromArrays(y:=dat.Reaction,
                                           x:=x,
                                           subjectId:=dat.Subject,
                                           z:=z,
                                           visit:=dat.Days,
                                           sortWithinSubjectByVisit:=True)

        Dim req As MixedModelFitRequest =
            MixedModelFitRequest.CreateLMM(blockData,
                                           New IdentityR(),
                                           New RandomInterceptSlope(),
                                           MixedModelFitMethod.REML)

        req.RequestLabel = "sleepstudy unbalanced KR post-estimation validation"
        req.ResponseVarName = "reaction"
        req.SubjectVarName = "subject"
        req.VisitVarName = "days"
        req.FixedEffectNames = {"(Intercept)", "days"}
        req.RandomEffectNames = {"(Intercept)", "days"}

        req.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger
        req.BuildKenwardRogerWorkspace = True
        req.BuildKenwardRogerSecondDerivatives = True
        req.Control = TestControl()

        req.StartThetaG = {Math.Log(24.7405), Math.Log(5.9221), Atanh(0.066)}
        req.StartThetaR = {Math.Log(654.941)}

        Return (New LMM(req)).Fit()
    End Function


    Private Shared Function GetHardCodedPostEstimationRows() As List(Of LinearReferenceRow)
        ' R source:
        '
        '   lme4::lmer(reaction ~ days + (days | subject), data = unbalanced_sleepstudy, REML = TRUE)
        '   vc_kr <- as.matrix(pbkrtest::vcovAdj(fit))
        '
        ' Existing R constants for this dataset:
        '
        '   beta_intercept = 248.8345407093
        '   beta_days      =  11.1253591171
        '   KR SE intercept=   6.83613875675
        '   KR SE days     =   1.62280741609
        '
        ' For rows [0, c], KR SE = abs(c) * KR SE(days), so no fixed-effect
        ' off-diagonal covariance is needed for these reference checks.
        Dim beta0 As Double = 248.8345407093
        Dim betaDays As Double = 11.1253591171
        Dim se0 As Double = 6.83613875675
        Dim seDays As Double = 1.62280741609

        Return New List(Of LinearReferenceRow) From {
            New LinearReferenceRow("LSMean days=0",
                                   New Double() {1.0, 0.0},
                                   beta0,
                                   se0),
            New LinearReferenceRow("Change days 9 - 0",
                                   New Double() {0.0, 9.0},
                                   9.0 * betaDays,
                                   9.0 * seDays),
            New LinearReferenceRow("Change days 3 - 0",
                                   New Double() {0.0, 3.0},
                                   3.0 * betaDays,
                                   3.0 * seDays)
        }
    End Function


    Private Shared Sub AssertUsableKRFit(res As MixedModelResult)
        Assert.IsNotNull(res, "LMM result should not be Nothing.")
        Assert.IsTrue(res.Converged, "Unbalanced sleepstudy random-slope LMM should converge.")
        Assert.AreEqual(2, res.P, "Unexpected fixed-effect dimension.")
        Assert.AreEqual(165, res.Nobs, "Unexpected observation count.")
        Assert.AreEqual(18, res.NoSubjects, "Unexpected subject count.")
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, res.FixedInferenceMethod)

        Assert.IsNotNull(res.KenwardRogerWorkspace, "KR workspace should be available.")
        Assert.AreEqual(MixedModelKrParameterScale.Covariance,
                        res.KenwardRogerWorkspace.ParameterScale,
                        "Random-slope LMM should use covariance-parameter KR scale.")

        Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(res)
        Assert.IsNotNull(adjusted, "KR adjusted Var(beta) should be available.")
    End Sub


    Private Shared Sub AssertAlmostEqual(expected As Double,
                                         actual As Double,
                                         tolerance As Double,
                                         label As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail(label & ": actual value is not finite. Expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " & actual.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If

        Dim diff As Double = Math.Abs(expected - actual)

        If diff > tolerance Then
            Assert.Fail(label & ": expected " &
                        expected.ToString("G17", CultureInfo.InvariantCulture) &
                        ", actual " &
                        actual.ToString("G17", CultureInfo.InvariantCulture) &
                        ", abs diff " &
                        diff.ToString("G17", CultureInfo.InvariantCulture) &
                        " > tolerance " &
                        tolerance.ToString("G17", CultureInfo.InvariantCulture) & ".")
        End If
    End Sub


    Private Shared Sub AssertFinite(value As Double,
                                    label As String)
        Assert.IsFalse(Double.IsNaN(value), label & " should not be NaN.")
        Assert.IsFalse(Double.IsInfinity(value), label & " should not be infinite.")
    End Sub


    Private Shared Function TestControl() As MixedModelControl
        Dim ctl As MixedModelControl = MixedModelControl.CreateDefault()
        ctl.MaxIter = 280
        ctl.Epsilon = 0.000001
        ctl.StepTolerance = 0.0000001
        ctl.FunctionTolerance = 0.0000001
        ctl.Trace = False
        ctl.ProfileFixedEffects = True
        Return ctl
    End Function


    Private Shared Function BuildFixedDesign(dat As SleepstudyData) As Double(,)
        Dim n As Integer = dat.Reaction.Length
        Dim x(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            x(i, 0) = 1.0
            x(i, 1) = dat.Days(i)
        Next

        Return x
    End Function


    Private Shared Function BuildRandomSlopeDesign(dat As SleepstudyData) As Double(,)
        Dim n As Integer = dat.Reaction.Length
        Dim z(n - 1, 1) As Double

        For i As Integer = 0 To n - 1
            z(i, 0) = 1.0
            z(i, 1) = dat.Days(i)
        Next

        Return z
    End Function


    Private Shared Function LoadSleepstudyCsv(fileName As String) As SleepstudyData
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)

        If lines.Length < 2 Then
            Throw New InvalidOperationException("sleepstudy CSV must contain a header and data rows.")
        End If

        Dim header() As String = lines(0).Split(","c)
        Dim col As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)

        For j As Integer = 0 To header.Length - 1
            col(header(j).Trim()) = j
        Next

        RequireColumn(col, "reaction", fileName)
        RequireColumn(col, "days", fileName)
        RequireColumn(col, "subject", fileName)

        Dim n As Integer = lines.Length - 1
        Dim reaction(n - 1) As Double
        Dim days(n - 1) As Double
        Dim subject(n - 1) As Object

        For i As Integer = 0 To n - 1
            Dim parts() As String = lines(i + 1).Split(","c)

            reaction(i) = ParseD(parts(col("reaction")))
            days(i) = ParseD(parts(col("days")))
            subject(i) = parts(col("subject")).Trim()
        Next

        Return New SleepstudyData With {
            .Reaction = reaction,
            .Days = days,
            .Subject = subject
        }
    End Function


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


    Private Shared Sub RequireColumn(col As Dictionary(Of String, Integer),
                                     columnName As String,
                                     fileName As String)
        If Not col.ContainsKey(columnName) Then
            Throw New InvalidOperationException("CSV file '" & fileName & "' must contain column '" & columnName & "'.")
        End If
    End Sub


    Private Shared Function ParseD(text As String) As Double
        Return Double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture)
    End Function


    Private Shared Function Atanh(x As Double) As Double
        Return 0.5 * Math.Log((1.0 + x) / (1.0 - x))
    End Function


    Private Class LinearReferenceRow
        Public ReadOnly Label As String
        Public ReadOnly L() As Double
        Public ReadOnly Estimate As Double
        Public ReadOnly KRAdjustedSE As Double

        Public Sub New(label As String,
                       l() As Double,
                       estimate As Double,
                       krAdjustedSE As Double)
            Me.Label = label
            Me.L = l
            Me.Estimate = estimate
            Me.KRAdjustedSE = krAdjustedSE
        End Sub
    End Class


    Private Class SleepstudyData
        Public Reaction() As Double
        Public Days() As Double
        Public Subject() As Object
    End Class

End Class

' ===== END migrated from MixedModelKRPostEstimationModelReferenceTests.vb =====

