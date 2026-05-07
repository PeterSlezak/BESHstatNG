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

' ===== BEGIN migrated from MixedModelResultKRTermLevelWrapResultsTests.vb =====



<TestClass()>
Public Class MixedModelResultKRTermLevelWrapResultsTests

    <TestMethod()>
    Public Sub WrapResults_KRInference_IncludesTermLevelFTestsByDefault()
        Dim res As MixedModelResult = BuildNamedKrResult()

        Dim tables As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=False)

        Assert.IsNotNull(tables)
        Assert.IsTrue(ContainsTableTitle(tables, "Kenward-Roger term-level F tests"),
                      "KR term-level F-test table should be included automatically for Kenward-Roger fixed inference.")
    End Sub


    <TestMethod()>
    Public Sub WrapResults_KRTermTestsFlagTrue_DoesNotDuplicateDefaultTable()
        Dim res As MixedModelResult = BuildNamedKrResult()

        Dim tablesWithout As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=False)
        Dim tablesWith As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)

        Assert.IsNotNull(tablesWith)
        Assert.IsTrue(ContainsTableTitle(tablesWith, "Kenward-Roger term-level F tests"),
                      "KR term-level F-test table should be included when explicitly requested.")

        Assert.AreEqual(tablesWithout.Count,
                        tablesWith.Count,
                        "Explicitly requesting the KR term-level table should not duplicate the default KR output table.")

        Dim termTable As ResultTable = FindTableByTitle(tablesWith, "Kenward-Roger term-level F tests")
        Assert.IsNotNull(termTable)
        Assert.IsTrue(termTable.PvalColumns.Contains(4), "Term-level F-test table should mark Pr(>F) as a p-value column.")

        Dim tableText(,) As Object = termTable.returnSelf()
        Assert.IsTrue(ArrayContainsText(tableText, "Unscaled F"), "Term-level F-test table should expose the unscaled Wald F diagnostic.")
        Assert.IsTrue(ArrayContainsText(tableText, "F scaling"), "Term-level F-test table should expose the KR F-scaling factor.")
        Assert.IsTrue(ArrayContainsText(tableText, "Rank reduced"), "Term-level F-test table should expose rank-reduction diagnostics.")

        Dim arr(,) As Object = termTable.returnSelf()
        Assert.IsTrue(ArrayContainsText(arr, "visit"), "Term table should contain visit term.")
        Assert.IsTrue(ArrayContainsText(arr, "treatment_active"), "Term table should contain treatment term.")
        Assert.IsTrue(ArrayContainsText(arr, "treatment_active:visit"), "Term table should contain interaction term.")
    End Sub


    <TestMethod()>
    Public Sub WrapResults_KRTermTestsFlagTrue_NonKRResultDoesNotIncludeTermLevelFTests()
        Dim res As MixedModelResult = BuildNamedKrResult()
        res.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal

        Dim tables As List(Of ResultTable) = res.wrapResults(includeKenwardRogerTermTests:=True)

        Assert.IsNotNull(tables)
        Assert.IsFalse(ContainsTableTitle(tables, "Kenward-Roger term-level F tests"),
                       "KR term-level F-test table should not appear for non-KR fixed inference.")
    End Sub


    Private Shared Function ContainsTableTitle(tables As List(Of ResultTable),
                                               title As String) As Boolean
        Return FindTableByTitle(tables, title) IsNot Nothing
    End Function


    Private Shared Function FindTableByTitle(tables As List(Of ResultTable),
                                             title As String) As ResultTable
        If tables Is Nothing Then Return Nothing

        For Each t As ResultTable In tables
            If t Is Nothing Then Continue For

            Dim arr(,) As Object = t.returnSelf()
            If ArrayContainsText(arr, title) Then Return t
        Next

        Return Nothing
    End Function


    Private Shared Function ArrayContainsText(arr(,) As Object,
                                              text As String) As Boolean
        If arr Is Nothing Then Return False

        For i As Integer = 0 To arr.GetLength(0) - 1
            For j As Integer = 0 To arr.GetLength(1) - 1
                If arr(i, j) Is Nothing Then Continue For

                If String.Equals(CStr(arr(i, j)),
                                 text,
                                 StringComparison.OrdinalIgnoreCase) Then
                    Return True
                End If
            Next
        Next

        Return False
    End Function


    Private Shared Function BuildNamedKrResult() As MixedModelResult
        Dim names() As String = {
            "(Intercept)",
            "visit=2",
            "visit=3",
            "treatment_active",
            "treatment_active:visit=2",
            "treatment_active:visit=3"
        }

        Dim p As Integer = names.Length
        Dim k As Integer = 2

        Dim phi(p - 1, p - 1) As Double
        Dim adjusted(p - 1, p - 1) As Double

        For j As Integer = 0 To p - 1
            phi(j, j) = 1.0 + CDbl(j)
            adjusted(j, j) = 1.5 + CDbl(j)
        Next

        Dim thetaCov(k - 1, k - 1) As Double
        thetaCov(0, 0) = 0.02
        thetaCov(1, 1) = 0.04
        thetaCov(0, 1) = 0.005
        thetaCov(1, 0) = 0.005

        Dim pMats(k - 1, p - 1, p - 1) As Double

        For j As Integer = 0 To p - 1
            pMats(0, j, j) = 0.1 + 0.01 * CDbl(j)
            pMats(1, j, j) = 0.05 + 0.005 * CDbl(j)
        Next

        Dim beta() As Double = {10.0, 1.0, 2.0, 3.0, 0.5, 0.75}
        Dim se(p - 1) As Double
        Dim stat(p - 1) As Double
        Dim pv(p - 1) As Double
        Dim df(p - 1) As Double

        For j As Integer = 0 To p - 1
            se(j) = Math.Sqrt(adjusted(j, j))
            stat(j) = beta(j) / se(j)
            pv(j) = 0.5
            df(j) = 100.0
        Next

        Dim ws As New MixedModelKrWorkspace With {
            .P = p,
            .K = k,
            .VarBeta = phi,
            .ThetaCovariance = thetaCov,
            .Pmats = pMats,
            .AdjustedVarBeta = adjusted,
            .ParameterScale = MixedModelKrParameterScale.Covariance
        }

        Dim inf As New MixedModelInferenceWorkspace With {
            .P = p,
            .K = k,
            .VarBeta = phi,
            .AdjustedVarBeta = adjusted,
            .ThetaCovariance = thetaCov,
            .KR_P = pMats
        }

        Return New MixedModelResult With {
            .Converged = True,
            .Message = "synthetic KR result",
            .FitMethod = MixedModelFitMethod.REML,
            .Nobs = 40,
            .NoSubjects = 10,
            .P = p,
            .Q = 0,
            .Beta = beta,
            .BetaSE = se,
            .BetaStatistic = stat,
            .BetaZ = stat,
            .BetaP = pv,
            .BetaDF = df,
            .VarBeta = phi,
            .FixedEffectNames = names,
            .FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger,
            .BetaStatisticLabel = "t",
            .BetaPValueLabel = "Pr(>|t|)",
            .KenwardRogerWorkspace = ws,
            .KenwardRogerAdjustedVarBeta = adjusted,
            .KenwardRogerParameterScale = MixedModelKrParameterScale.Covariance,
            .InferenceWorkspace = inf,
            .Theta = Array.Empty(Of Double)(),
            .ThetaG = Array.Empty(Of Double)(),
            .ThetaR = Array.Empty(Of Double)()
        }
    End Function

End Class

' ===== END migrated from MixedModelResultKRTermLevelWrapResultsTests.vb =====



' ===== BEGIN MMRM worksheet UDF extractor tests =====

<TestClass()>
Public Class MixedModelMmrmUdfExtractorTests

    <TestInitialize()>
    Public Sub ClearHandlesBeforeTest()
        BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_CLEAR_ALL()
    End Sub


    <TestCleanup()>
    Public Sub ClearHandlesAfterTest()
        BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_CLEAR_ALL()
    End Sub


    <TestMethod()>
    Public Sub MMRM_UDF_CoreExtractors_ReturnExpectedTables()
        Dim handle As String = CreateSyntheticMmrmHandle()

        Dim allResults(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_RESULTS(handle))
        AssertTableContains(allResults, "Fixed effects")
        AssertTableContains(allResults, "Fit statistics")

        Dim coef(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_COEF(handle))
        AssertTableContains(coef, "Fixed effects")
        AssertTableContains(coef, "treatment_active")

        Dim type3(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_TYPE3(handle))
        AssertTableContains(type3, "Kenward-Roger term-level F tests")
        AssertTableContains(type3, "treatment_active")

        Dim covparms(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_COVPARMS(handle))
        AssertTableContains(covparms, "Covariance parameters")

        Dim rcov(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_R_COV(handle))
        AssertTableContains(rcov, "Estimated R covariance matrix")

        Dim rcorr(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_R_CORR(handle))
        AssertTableContains(rcorr, "Estimated R correlation matrix")

        Dim fitstats(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_FITSTATS(handle))
        AssertTableContains(fitstats, "Fit statistics")
    End Sub


    <TestMethod()>
    Public Sub MMRM_UDF_LSMeansContrastsFittedAndResidualExtractors_ReturnTables()
        Dim handle As String = CreateSyntheticMmrmHandle()

        Dim visitMeans(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_LSMEANS(handle))
        AssertTableContains(visitMeans, "Estimated marginal means by visit")
        AssertTableHasAtLeastRows(visitMeans, 4, "MMRM_LSMEANS by visit")

        Dim groupedMeans(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_LSMEANS(handle, "treatment_active"))
        AssertTableContains(groupedMeans, "Estimated marginal means by visit and treatment_active")
        AssertTableHasAtLeastRows(groupedMeans, 4, "MMRM_LSMEANS by visit and treatment_active")

        Dim contrasts(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_CONTRASTS(
                handle,
                "treatment_active",
                BESHStatNG.regression.MMRMPostEstimation.MODE_CONTROL,
                0.0,
                Nothing,
                BESHStatNG.regression.MMRMPostEstimation.DIR_TREATMENT_MINUS_CONTROL))
        AssertTableContains(contrasts, "MMRM group differences vs control by visit")
        AssertTableHasAtLeastRows(contrasts, 4, "MMRM_CONTRASTS")

        Dim fitted(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_FITTED(handle))
        Assert.AreEqual("Row", CStr(fitted(0, 0)))
        Assert.AreEqual("Fitted", CStr(fitted(0, 1)))
        Assert.IsTrue(fitted.GetLength(0) > 10, "Fitted-values table should include retained data rows.")

        Dim residuals(,) As Object = RequireTable(
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_RESID(handle))
        Assert.AreEqual("Row", CStr(residuals(0, 0)))
        Assert.AreEqual("Fitted", CStr(residuals(0, 1)))
        Assert.AreEqual("Residual", CStr(residuals(0, 2)))
    End Sub


    <TestMethod()>
    Public Sub MMRM_UDF_DropAndClearAll_RemoveCachedHandles()
        Dim handle1 As String = CreateSyntheticMmrmHandle()
        Dim handle2 As String = CreateSyntheticMmrmHandle()

        Assert.AreEqual(True, BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_DROP(handle1),
                        "Dropping an existing handle should return TRUE.")

        Dim droppedResult As Object = BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_COEF(handle1)
        Assert.IsFalse(TypeOf droppedResult Is Object(,),
                       "Extracting from a dropped handle should not return a result table.")

        Dim cleared As Object = BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_CLEAR_ALL()
        Assert.IsTrue(CInt(cleared) >= 1, "At least one remaining handle should be cleared.")

        Dim clearedResult As Object = BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_COEF(handle2)
        Assert.IsFalse(TypeOf clearedResult Is Object(,),
                       "Extracting from a handle after MMRM_CLEAR_ALL should not return a result table.")
    End Sub


    Private Shared Function CreateSyntheticMmrmHandle() As String
        Dim y(,) As Object = Nothing
        Dim x(,) As Object = Nothing
        Dim subject(,) As Object = Nothing
        Dim visit(,) As Object = Nothing
        Dim names(,) As Object = Nothing

        BuildSyntheticMmrmInputs(y, x, subject, visit, names)

        Dim handleObj As Object =
            BESHStatNG.WorksheetFunctions.MixedModelUDFs.MMRM_FIT(
                y:=y,
                x:=x,
                subject:=subject,
                visit:=visit,
                varNames:=names,
                covariance:="CS",
                fitMethod:="REML",
                inference:="KR",
                includeIntercept:=True,
                alpha:=0.05,
                maxIter:=200,
                trace:=False)

        Assert.IsInstanceOfType(handleObj, GetType(String), "MMRM_FIT should return a text handle.")

        Dim handle As String = CStr(handleObj)
        Assert.IsFalse(handle.StartsWith("BESH.REGR.MMRM_FIT error:", StringComparison.OrdinalIgnoreCase),
                       "MMRM_FIT returned an error: " & handle)
        Assert.IsTrue(handle.StartsWith("MMRM:", StringComparison.OrdinalIgnoreCase),
                      "MMRM_FIT should return an MMRM handle.")

        Return handle
    End Function


    Private Shared Sub BuildSyntheticMmrmInputs(ByRef y(,) As Object,
                                                ByRef x(,) As Object,
                                                ByRef subject(,) As Object,
                                                ByRef visit(,) As Object,
                                                ByRef names(,) As Object)
        Dim subjects As Integer = 18
        Dim visits As Integer = 3
        Dim n As Integer = subjects * visits

        ReDim y(n - 1, 0)
        ReDim x(n - 1, 4)
        ReDim subject(n - 1, 0)
        ReDim visit(n - 1, 0)
        ReDim names(0, 4)

        names(0, 0) = "treatment_active"
        names(0, 1) = "visit2"
        names(0, 2) = "visit3"
        names(0, 3) = "treatment_active:visit2"
        names(0, 4) = "treatment_active:visit3"

        Dim r As Integer = 0

        For sid As Integer = 1 To subjects
            Dim trt As Double = If(sid Mod 2 = 0, 1.0, 0.0)
            Dim subjectOffset As Double = 0.15 * CDbl((sid Mod 5) - 2)

            For v As Integer = 1 To visits
                Dim v2 As Double = If(v = 2, 1.0, 0.0)
                Dim v3 As Double = If(v = 3, 1.0, 0.0)
                Dim deterministicNoise As Double = 0.03 * CDbl(((sid + v) Mod 4) - 1)

                Dim response As Double =
                    20.0 +
                    1.4 * trt +
                    0.8 * CDbl(v) +
                    0.35 * trt * CDbl(v) +
                    subjectOffset +
                    deterministicNoise

                y(r, 0) = response
                x(r, 0) = trt
                x(r, 1) = v2
                x(r, 2) = v3
                x(r, 3) = trt * v2
                x(r, 4) = trt * v3
                subject(r, 0) = "S" & sid.ToString("00", CultureInfo.InvariantCulture)
                visit(r, 0) = CDbl(v)

                r += 1
            Next
        Next
    End Sub


    Private Shared Function RequireTable(value As Object) As Object(,)
        Assert.IsInstanceOfType(value, GetType(Object(,)), "Expected a worksheet result table but got: " & Convert.ToString(value, CultureInfo.InvariantCulture))
        Return DirectCast(value, Object(,))
    End Function




    Private Shared Sub AssertTableHasAtLeastRows(table(,) As Object,
                                                minimumRows As Integer,
                                                label As String)
        Assert.IsNotNull(table)
        Assert.IsTrue(table.GetLength(0) >= minimumRows,
                      label & " table should contain at least " &
                      minimumRows.ToString(CultureInfo.InvariantCulture) &
                      " rows, but contained " &
                      table.GetLength(0).ToString(CultureInfo.InvariantCulture) & ".")
    End Sub

    Private Shared Sub AssertTableContains(table(,) As Object,
                                           expectedText As String)
        Assert.IsNotNull(table)

        For i As Integer = 0 To table.GetLength(0) - 1
            For j As Integer = 0 To table.GetLength(1) - 1
                Dim s As String = Convert.ToString(table(i, j), CultureInfo.InvariantCulture)
                If String.Equals(s, expectedText, StringComparison.OrdinalIgnoreCase) Then Return
            Next
        Next

        Assert.Fail("Expected table to contain text: " & expectedText)
    End Sub

End Class

' ===== END MMRM worksheet UDF extractor tests =====
