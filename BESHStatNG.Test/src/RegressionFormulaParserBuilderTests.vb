Option Explicit On
Option Strict On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG

<TestClass()>
Public Class RegressionFormulaParserBuilderTests

    Private Const TOL As Double = 0.000000000001

    Private Shared Function MakeRawX2() As Double(,)
        ' columns: prison, dose, clinic
        Return New Double(,) {
            {1.0, 2.0, 1.0},
            {3.0, 4.0, 2.0},
            {5.0, 6.0, 1.0},
            {7.0, 8.0, 3.0}
        }
    End Function

    Private Shared Function MakeRawXFactor() As Double(,)
        ' columns: age, stage
        Return New Double(,) {
            {10.0, 1.0},
            {20.0, 2.0},
            {30.0, 3.0},
            {40.0, 1.0}
        }
    End Function

    Private Shared Sub AssertStringArrayEqual(expected() As String, actual() As String, Optional msg As String = "")
        Assert.IsNotNull(actual, "Actual string array is Nothing. " & msg)
        Assert.AreEqual(expected.Length, actual.Length, "String-array length mismatch. " & msg)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), $"String mismatch at index {i}. {msg}")
        Next
    End Sub

    Private Shared Sub AssertVectorAlmostEqual(expected() As Double, actual() As Double, tol As Double, Optional msg As String = "")
        Assert.IsNotNull(actual, "Actual vector is Nothing. " & msg)
        Assert.AreEqual(expected.Length, actual.Length, "Vector length mismatch. " & msg)
        For i As Integer = 0 To expected.Length - 1
            Assert.AreEqual(expected(i), actual(i), tol, $"Mismatch at index {i}. {msg}")
        Next
    End Sub

    Private Shared Sub AssertMatrixAlmostEqual(expected(,) As Double, actual(,) As Double, tol As Double, Optional msg As String = "")
        Assert.IsNotNull(actual, "Actual matrix is Nothing. " & msg)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), "MatrixForUdfs row count mismatch. " & msg)
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), "MatrixForUdfs column count mismatch. " & msg)

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), tol, $"Mismatch at ({i},{j}). {msg}")
            Next
        Next
    End Sub

    Private Shared Function GetColumn(mat(,) As Double, col As Integer) As Double()
        Dim n As Integer = mat.GetLength(0)
        Dim out(n - 1) As Double
        For i As Integer = 0 To n - 1
            out(i) = mat(i, col)
        Next
        Return out
    End Function

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub VariableCatalog_resolves_single_quoted_variable_name_in_names_mode()
        Dim rawX = MakeRawX2()
        Dim catalog = RegressionFormulaDesignService.BuildVariableCatalogFromRawPredictors(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            allowRelativeColumnLetters:=False,
            allowAbsoluteColumnLetters:=False,
            allowQuotedVariableNames:=True)

        Dim entry As RegressionVariableCatalogEntry = Nothing
        Dim tokenKind As RegressionFormulaTokenKind
        Dim err As String = Nothing

        Dim ok As Boolean = catalog.TryResolveToken("'dose'", entry, tokenKind, err)

        Assert.IsTrue(ok, err)
        Assert.IsNotNull(entry)
        Assert.AreEqual("B", entry.BaseKey)
        Assert.AreEqual("dose", entry.DisplayName)
        Assert.AreEqual(RegressionFormulaTokenKind.VariableName, tokenKind)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub ParseFormula_names_mode_uses_single_quoted_names_and_preserves_display_names()
        Dim rawX = MakeRawX2()
        Dim catalog = RegressionFormulaDesignService.BuildVariableCatalogFromRawPredictors(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            allowRelativeColumnLetters:=False,
            allowAbsoluteColumnLetters:=False,
            allowQuotedVariableNames:=True)

        Dim spec = RegressionFormulaParser.ParseFormulaToDesignSpec("'prison' + 'dose' + 'dose'^2", catalog)

        Assert.AreEqual("'prison' + 'dose' + 'dose'^2", spec.NormalizedFormulaText)
        Assert.AreEqual(3, spec.EffectItems.Count)

        Assert.AreEqual("prison", spec.TermSpecs(spec.EffectItems(0)).DisplayNameForCoef)
        Assert.AreEqual("dose", spec.TermSpecs(spec.EffectItems(1)).DisplayNameForCoef)
        Assert.AreEqual("dose^2", spec.TermSpecs(spec.EffectItems(2)).DisplayNameForCoef)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub ParseFormula_conflicting_factor_reference_levels_returns_error()
        Dim rawX = MakeRawXFactor()
        Dim catalog = RegressionFormulaDesignService.BuildVariableCatalogFromRawPredictors(
            rawX:=rawX,
            predictorNames:=New String() {"age", "stage"})

        Dim spec As RegressionFormulaDesignSpec = Nothing
        Dim err As String = Nothing

        Dim ok As Boolean = RegressionFormulaParser.TryParseFormulaToDesignSpec(
            formulaText:="factor(B, ref=1) + factor(B, ref=2)",
            variableCatalog:=catalog,
            designSpec:=spec,
            errorMessage:=err)

        Assert.IsFalse(ok, "Expected conflicting factor references to fail.")
        Assert.IsTrue(Not String.IsNullOrWhiteSpace(err), "Expected parser error message.")
        StringAssert.Contains(err, "conflicting duplicate term")
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub ParseFormula_repeated_variable_inside_interaction_returns_error()
        Dim rawX = MakeRawX2()
        Dim catalog = RegressionFormulaDesignService.BuildVariableCatalogFromRawPredictors(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"})

        Dim spec As RegressionFormulaDesignSpec = Nothing
        Dim err As String = Nothing

        Dim ok As Boolean = RegressionFormulaParser.TryParseFormulaToDesignSpec(
            formulaText:="A:B:A",
            variableCatalog:=catalog,
            designSpec:=spec,
            errorMessage:=err)

        Assert.IsFalse(ok, "Expected repeated variable inside one interaction term to fail.")
        Assert.IsTrue(Not String.IsNullOrWhiteSpace(err), "Expected parser error message.")
        StringAssert.Contains(err, "repeats variable")
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_blank_formula_returns_raw_matrix_and_user_names()
        Dim rawX = MakeRawX2()
        Dim result = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:=Nothing)

        AssertStringArrayEqual(New String() {"prison", "dose", "clinic"}, result.ExpandedPredictorNames)
        AssertMatrixAlmostEqual(rawX, result.ExpandedPredictorMatrix, TOL)
        AssertStringArrayEqual(New String() {"A", "B", "C"}, result.FullRawPredictorKeys)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_relative_formula_builds_expected_columns_and_names()
        Dim rawX = MakeRawX2()
        Dim result = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:="A + B + B^2 + A:B")

        AssertStringArrayEqual(New String() {"prison", "dose", "dose^2", "prison:dose"}, result.ExpandedPredictorNames)

        Dim expected(,) As Double = {
            {1.0, 2.0, 4.0, 2.0},
            {3.0, 4.0, 16.0, 12.0},
            {5.0, 6.0, 36.0, 30.0},
            {7.0, 8.0, 64.0, 56.0}
        }
        AssertMatrixAlmostEqual(expected, result.ExpandedPredictorMatrix, TOL)
        Assert.AreEqual("A + B + B^2 + A:B", result.DesignSpec.NormalizedFormulaText)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_names_mode_builds_expected_columns_and_names()
        Dim rawX = MakeRawX2()
        Dim result = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:="'prison' + 'dose' + 'dose'^2",
            allowRelativeColumnLetters:=False,
            allowAbsoluteColumnLetters:=False,
            allowQuotedVariableNames:=True)

        AssertStringArrayEqual(New String() {"prison", "dose", "dose^2"}, result.ExpandedPredictorNames)

        Dim expected(,) As Double = {
            {1.0, 2.0, 4.0},
            {3.0, 4.0, 16.0},
            {5.0, 6.0, 36.0},
            {7.0, 8.0, 64.0}
        }
        AssertMatrixAlmostEqual(expected, result.ExpandedPredictorMatrix, TOL)
        Assert.AreEqual("'prison' + 'dose' + 'dose'^2", result.DesignSpec.NormalizedFormulaText)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_names_only_mode_rejects_bare_column_letters()
        Dim rawX = MakeRawX2()
        Dim result As RegressionFormulaMatrixBuildResult = Nothing
        Dim err As String = Nothing

        Dim ok As Boolean = RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            result:=result,
            errorMessage:=err,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:="A + 'dose'",
            allowRelativeColumnLetters:=False,
            allowAbsoluteColumnLetters:=False,
            allowQuotedVariableNames:=True)

        Assert.IsFalse(ok, "Expected bare letters to fail in names-only mode.")
        Assert.IsTrue(Not String.IsNullOrWhiteSpace(err), "Expected error message.")
        StringAssert.Contains(err, "Unknown variable reference 'A'")
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_absolute_mode_uses_absolute_column_letters()
        Dim rawX = MakeRawX2()
        Dim result = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:="C + F^2",
            absoluteColumnLetters:=New String() {"C", "F", "J"},
            allowRelativeColumnLetters:=False,
            allowAbsoluteColumnLetters:=True,
            allowQuotedVariableNames:=True)

        AssertStringArrayEqual(New String() {"prison", "dose^2"}, result.ExpandedPredictorNames)

        Dim expected(,) As Double = {
            {1.0, 4.0},
            {3.0, 16.0},
            {5.0, 36.0},
            {7.0, 64.0}
        }
        AssertMatrixAlmostEqual(expected, result.ExpandedPredictorMatrix, TOL)
        Assert.AreEqual("C + F^2", result.DesignSpec.NormalizedFormulaText)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrix_factor_reference_omits_reference_level_and_uses_display_names()
        Dim rawX = MakeRawXFactor()
        Dim result = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"age", "stage"},
            formulaText:="A + factor(B, ref=2)",
            omitCategoricalReference:=True)

        AssertStringArrayEqual(New String() {"age", "stage[1]", "stage[3]"}, result.ExpandedPredictorNames)

        Dim expected(,) As Double = {
            {10.0, 1.0, 0.0},
            {20.0, 0.0, 0.0},
            {30.0, 0.0, 1.0},
            {40.0, 1.0, 0.0}
        }
        AssertMatrixAlmostEqual(expected, result.ExpandedPredictorMatrix, TOL)
        Assert.AreEqual("A + factor(B, ref=2)", result.DesignSpec.NormalizedFormulaText)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub BuildExpandedPredictorMatrixFromDesignSpec_reproduces_formula_build()
        Dim rawX = MakeRawX2()

        Dim fitBuild = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromFormula(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"},
            formulaText:="A + B^2 + A:B")

        Dim predBuild = RegressionFormulaDesignService.BuildExpandedPredictorMatrixFromDesignSpec(
            rawX:=rawX,
            fullRawPredictorKeys:=fitBuild.FullRawPredictorKeys,
            designSpec:=fitBuild.DesignSpec)

        AssertStringArrayEqual(fitBuild.ExpandedPredictorNames, predBuild.ExpandedPredictorNames)
        AssertMatrixAlmostEqual(fitBuild.ExpandedPredictorMatrix, predBuild.ExpandedPredictorMatrix, TOL)
    End Sub

    <TestMethod()>
    <TestCategory("RegressionFormula")>
    Public Sub ParseFormula_deduplicates_compatible_duplicates_in_normalized_formula_text()
        Dim rawX = MakeRawX2()
        Dim catalog = RegressionFormulaDesignService.BuildVariableCatalogFromRawPredictors(
            rawX:=rawX,
            predictorNames:=New String() {"prison", "dose", "clinic"})

        Dim spec = RegressionFormulaParser.ParseFormulaToDesignSpec("A + A + B + B", catalog)

        Assert.AreEqual(2, spec.EffectItems.Count)
        Assert.AreEqual("A + B", spec.NormalizedFormulaText)
    End Sub

End Class
