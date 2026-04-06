Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Udfs = BESHStatNG.BESHStatNG.WorksheetFunctions

' Batch 2 UDF tests.
' This file assumes the existing src\UdfTests.vb from batch 1 is already present,
' because it reuses:
'   - UdfAssert
'   - ExcelDnaCompat
'
' Add this file to the BESHStatNG.Test project, for example:
'   src\UdfTests_Batch2.vb

Friend Module UdfBatch2Data

    Private ReadOnly Invariant As CultureInfo = CultureInfo.InvariantCulture

    Public Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory

        Dim c1 As String = Path.Combine(baseDir, fileName)
        If File.Exists(c1) Then Return c1

        Dim c2 As String = Path.Combine(baseDir, "TestData", fileName)
        If File.Exists(c2) Then Return c2

        Dim c3 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName))
        If File.Exists(c3) Then Return c3

        Dim c4 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName))
        If File.Exists(c4) Then Return c4

        Throw New FileNotFoundException("Test data file not found.", fileName)
    End Function

    Private Function ParseDouble(s As String) As Double
        Return Double.Parse(s.Trim(), NumberStyles.Float Or NumberStyles.AllowThousands, Invariant)
    End Function

    Private Function ParseInt(s As String) As Integer
        Return Integer.Parse(s.Trim(), NumberStyles.Integer, Invariant)
    End Function

    Private Function ReadCsvLines(fileName As String) As String()
        Return File.ReadAllLines(GetTestDataPath(fileName))
    End Function

    Public Sub LoadSurvivalUdfArgs(fileName As String,
                                   ByRef time As Object(,),
                                   ByRef status As Object(,),
                                   ByRef group As Object(,),
                                   ByRef strata As Object(,))
        Dim lines() As String = ReadCsvLines(fileName)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain header + rows.")

        Dim n As Integer = lines.Length - 1
        time = New Object(n - 1, 0) {}
        status = New Object(n - 1, 0) {}
        group = New Object(n - 1, 0) {}
        strata = New Object(n - 1, 0) {}

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            time(r - 1, 0) = ParseDouble(parts(idx("time")))
            status(r - 1, 0) = ParseInt(parts(idx("status")))
            group(r - 1, 0) = parts(idx("group")).Trim()
            strata(r - 1, 0) = parts(idx("stratum")).Trim()
        Next
    End Sub

    Public Sub LoadCoxUdfArgs(fileName As String,
                              ByRef time As Object(,),
                              ByRef status As Object(,),
                              ByRef x As Object(,),
                              ByRef strata As Object(,))
        Dim lines() As String = ReadCsvLines(fileName)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain header + rows.")

        Dim n As Integer = lines.Length - 1
        time = New Object(n - 1, 0) {}
        status = New Object(n - 1, 0) {}
        x = New Object(n - 1, 1) {}
        strata = New Object(n - 1, 0) {}

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            time(r - 1, 0) = ParseDouble(parts(idx("time")))
            status(r - 1, 0) = ParseInt(parts(idx("status")))
            x(r - 1, 0) = ParseDouble(parts(idx("x1")))
            x(r - 1, 1) = ParseDouble(parts(idx("x2")))
            strata(r - 1, 0) = parts(idx("stratum")).Trim()
        Next
    End Sub

    Public Sub LoadOrdinalUdfArgs(fileName As String,
                                  ByRef y As Object(,),
                                  ByRef x As Object(,),
                                  ByRef offset As Object(,),
                                  ByRef weights As Object(,))
        Dim lines() As String = ReadCsvLines(fileName)
        If lines.Length < 2 Then Throw New InvalidOperationException("CSV must contain header + rows.")

        Dim n As Integer = lines.Length - 1
        y = New Object(n - 1, 0) {}
        x = New Object(n - 1, 1) {}
        offset = New Object(n - 1, 0) {}
        weights = New Object(n - 1, 0) {}

        Dim header() As String = lines(0).Split(","c).Select(Function(z) z.Trim()).ToArray()
        Dim idx As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 0 To header.Length - 1
            idx(header(i)) = i
        Next

        For r As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(r).Split(","c)
            y(r - 1, 0) = ParseInt(parts(idx("y")))
            x(r - 1, 0) = ParseDouble(parts(idx("x1")))
            x(r - 1, 1) = ParseDouble(parts(idx("x2")))
            offset(r - 1, 0) = ParseDouble(parts(idx("offset")))
            weights(r - 1, 0) = ParseDouble(parts(idx("w")))
        Next
    End Sub

    Public Sub LoadMultinomialUdfArgs(fileName As String,
                                      ByRef y As Object(,),
                                      ByRef x As Object(,),
                                      ByRef offset As Object(,),
                                      ByRef weights As Object(,))
        LoadOrdinalUdfArgs(fileName, y, x, offset, weights)
    End Sub

    Public Function TakeFirstRows(source As Object(,), count As Integer) As Object(,)
        Dim rowCount As Integer = source.GetLength(0)
        Dim colCount As Integer = source.GetLength(1)
        Dim keep As Integer = Math.Min(count, rowCount)

        Dim result(keep - 1, colCount - 1) As Object
        For i As Integer = 0 To keep - 1
            For j As Integer = 0 To colCount - 1
                result(i, j) = source(i, j)
            Next
        Next

        Return result
    End Function

End Module

<TestClass>
Public Class UdfHelpersBatch2Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GetOptionalInt_and_double_parse_numeric_inputs_and_fall_back_on_invalid()
        Assert.AreEqual(12, UDFhelpers.GetOptionalInt(12.9R, -1))
        Assert.AreEqual(-1, UDFhelpers.GetOptionalInt("abc", -1))

        Assert.AreEqual(3.5R, UDFhelpers.GetOptionalDouble(3.5R, -1.0R), 0.0R)
        Assert.AreEqual(-1.0R, UDFhelpers.GetOptionalDouble("abc", -1.0R), 0.0R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ExpForDisplay_handles_nan_overflow_and_underflow()
        UdfAssert.IsExcelError(UDFhelpers.ExpForDisplay(Double.NaN), "ExcelErrorNum")
        Assert.AreEqual("Inf", CStr(UDFhelpers.ExpForDisplay(Double.PositiveInfinity)))
        Assert.AreEqual(0.0R, Convert.ToDouble(UDFhelpers.ExpForDisplay(Double.NegativeInfinity)), 0.0R)

        Dim finite As Object = UDFhelpers.ExpForDisplay(1.25R)
        Assert.AreEqual(Math.Exp(1.25R), Convert.ToDouble(finite), 0.000000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ParseTieMethod_recognizes_supported_values_and_defaults_unknown()
        Assert.AreEqual(Global.BESHStatNG.TieMethod.Breslow,
                        UDFhelpers.ParseTieMethod("breslow", Global.BESHStatNG.TieMethod.Exact))

        Assert.AreEqual(Global.BESHStatNG.TieMethod.Efron,
                        UDFhelpers.ParseTieMethod("efron", Global.BESHStatNG.TieMethod.Breslow))

        Assert.AreEqual(Global.BESHStatNG.TieMethod.Exact,
                        UDFhelpers.ParseTieMethod("exact", Global.BESHStatNG.TieMethod.Breslow))

        Assert.AreEqual(Global.BESHStatNG.TieMethod.Breslow,
                        UDFhelpers.ParseTieMethod("unknown", Global.BESHStatNG.TieMethod.Breslow))
    End Sub

End Class

<TestClass>
Public Class SurvivalUdfBatch2Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub LOGRANK_P_and_STAT_agree_with_chi_square_relationship_for_two_groups()
        Dim time As Object(,) = Nothing
        Dim status As Object(,) = Nothing
        Dim group As Object(,) = Nothing
        Dim strata As Object(,) = Nothing

        UdfBatch2Data.LoadSurvivalUdfArgs("survival_dataset_2group.csv", time, status, group, strata)

        Dim stat As Double = UdfAssert.AsDouble(Udfs.SurvivalUDFs.LOGRANK_STAT(time, status, group, strata, "logrank"))
        Dim pval As Double = UdfAssert.AsDouble(Udfs.SurvivalUDFs.LOGRANK_P(time, status, group, strata, "logrank"))

        Dim expectedP As Double = 1.0R - Global.BESHStatNG.distributions.ChiSquareCDF(stat, 1.0R)

        Assert.IsTrue(stat >= 0.0R)
        Assert.IsTrue(pval >= 0.0R AndAlso pval <= 1.0R)
        Assert.AreEqual(expectedP, pval, 0.0000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MEDIAN_CI_grouped_returns_one_row_per_group_and_four_columns()
        Dim time As Object(,) = Nothing
        Dim status As Object(,) = Nothing
        Dim group As Object(,) = Nothing
        Dim strata As Object(,) = Nothing

        UdfBatch2Data.LoadSurvivalUdfArgs("survival_dataset_2group.csv", time, status, group, strata)

        Dim result As Object = Udfs.SurvivalUDFs.MEDIAN_CI(time, status, group, 0.05R)
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Assert.AreEqual(2, tbl.GetLength(0))
        Assert.AreEqual(4, tbl.GetLength(1))
        Assert.IsFalse(String.IsNullOrWhiteSpace(Convert.ToString(tbl(0, 0))))
        Assert.IsFalse(String.IsNullOrWhiteSpace(Convert.ToString(tbl(1, 0))))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub KM_TABLE_grouped_returns_seven_columns_and_valid_probability_bounds()
        Dim time As Object(,) = Nothing
        Dim status As Object(,) = Nothing
        Dim group As Object(,) = Nothing
        Dim strata As Object(,) = Nothing

        UdfBatch2Data.LoadSurvivalUdfArgs("survival_dataset_2group.csv", time, status, group, strata)

        Dim result As Object = Udfs.SurvivalUDFs.KM_TABLE(time, status, group, 0.05R)
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Assert.AreEqual(7, tbl.GetLength(1))
        Assert.IsTrue(tbl.GetLength(0) >= 6)

        For r As Integer = 0 To tbl.GetLength(0) - 1
            Dim surv As Double = Convert.ToDouble(tbl(r, 3), CultureInfo.InvariantCulture)
            Dim lcl As Double = Convert.ToDouble(tbl(r, 5), CultureInfo.InvariantCulture)
            Dim ucl As Double = Convert.ToDouble(tbl(r, 6), CultureInfo.InvariantCulture)

            Assert.IsTrue(surv >= 0.0R AndAlso surv <= 1.0R, $"Survival out of range at row {r}.")
            Assert.IsTrue(lcl >= 0.0R AndAlso lcl <= 1.0R, $"LCL out of range at row {r}.")
            Assert.IsTrue(ucl >= 0.0R AndAlso ucl <= 1.0R, $"UCL out of range at row {r}.")
        Next
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MEDIAN_CI_invalid_alpha_returns_num()
        Dim time As Object(,) = Nothing
        Dim status As Object(,) = Nothing
        Dim group As Object(,) = Nothing
        Dim strata As Object(,) = Nothing

        UdfBatch2Data.LoadSurvivalUdfArgs("survival_dataset_2group.csv", time, status, group, strata)

        Dim result As Object = Udfs.SurvivalUDFs.MEDIAN_CI(time, status, group, 1.0R)
        UdfAssert.IsExcelError(result, "ExcelErrorNum")
    End Sub

End Class

<TestClass>
Public Class CoxUdfBatch2Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub COX_handle_lifecycle_with_tests_baseline_and_predictions_works()
        Dim time As Object(,) = Nothing
        Dim status As Object(,) = Nothing
        Dim x As Object(,) = Nothing
        Dim strata As Object(,) = Nothing

        UdfBatch2Data.LoadCoxUdfArgs("coxph_dataset_strata_ties.csv", time, status, x, strata)

        Dim handleObj As Object = Udfs.CoxUDFs.COX_FIT(
            time,
            status,
            x,
            "x1,x2",
            Nothing,
            "efron",
            False,
            Nothing,
            Nothing,
            200,
            0.000000000001R)

        Assert.IsInstanceOfType(handleObj, GetType(String), $"COX_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Assert.IsFalse(String.IsNullOrWhiteSpace(handle))

        Dim summary As Object(,) = UdfAssert.AsTable(Udfs.CoxUDFs.COX_SUMMARY(handle))
        Assert.AreEqual("Variable", CStr(summary(0, 0)))

        Dim testsTbl As Object(,) = UdfAssert.AsTable(Udfs.CoxUDFs.COX_TESTS(handle))
        Assert.AreEqual("Item", CStr(testsTbl(0, 0)))

        Dim baselineTbl As Object(,) = UdfAssert.AsTable(Udfs.CoxUDFs.COX_BASELINE(handle, "table"))
        Assert.AreEqual("Stratum", CStr(baselineTbl(0, 0)))
        Assert.AreEqual("Time", CStr(baselineTbl(0, 1)))
        Assert.AreEqual("Survival", CStr(baselineTbl(0, 2)))
        Assert.AreEqual("CumHazard", CStr(baselineTbl(0, 3)))

        Dim newX As Object(,) = UdfBatch2Data.TakeFirstRows(x, 3)
        Dim riskTbl As Object(,) = UdfAssert.AsTable(Udfs.CoxUDFs.COX_PRED(handle, newX, "risk"))
        Assert.AreEqual("Subject", CStr(riskTbl(0, 0)))
        Assert.AreEqual("Risk", CStr(riskTbl(0, 1)))
        Assert.IsTrue(Convert.ToDouble(riskTbl(1, 1), CultureInfo.InvariantCulture) > 0.0R)

        Dim timeGrid As Object(,) = UdfBatch2Data.TakeFirstRows(time, 2)
        Dim survPred As Object(,) = UdfAssert.AsTable(Udfs.CoxUDFs.COX_PRED(handle, UdfBatch2Data.TakeFirstRows(x, 2), "survival", timeGrid))
        Assert.AreEqual("Subject", CStr(survPred(0, 0)))
        Assert.AreEqual("Time", CStr(survPred(0, 1)))
        Assert.AreEqual("Survival", CStr(survPred(0, 2)))
        Assert.AreEqual(5, survPred.GetLength(0))
        Assert.AreEqual(3, survPred.GetLength(1))

        Assert.AreEqual(True, Udfs.CoxUDFs.COX_DROP(handle))
        UdfAssert.IsExcelError(Udfs.CoxUDFs.COX_SUMMARY(handle), "ExcelErrorNA")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub COX_invalid_handle_returns_na()
        UdfAssert.IsExcelError(Udfs.CoxUDFs.COX_SUMMARY("no-such-handle"), "ExcelErrorNA")
        UdfAssert.IsExcelError(Udfs.CoxUDFs.COX_TESTS("no-such-handle"), "ExcelErrorNA")
        UdfAssert.IsExcelError(Udfs.CoxUDFs.COX_BASELINE("no-such-handle", "table"), "ExcelErrorNA")
    End Sub

End Class

<TestClass>
Public Class OrdinalLogitUdfBatch2Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ORDLOGIT_handle_lifecycle_with_classification_and_prediction_works()
        Dim y As Object(,) = Nothing
        Dim x As Object(,) = Nothing
        Dim offset As Object(,) = Nothing
        Dim weights As Object(,) = Nothing

        UdfBatch2Data.LoadOrdinalUdfArgs("ordinal_logit_dataset_basic.csv", y, x, offset, weights)

        Dim handleObj As Object = Udfs.OrdinalLogitUDFs.ORDLOGIT_FIT(
            y,
            x,
            "x1,x2",
            offset,
            weights,
            "last",
            Nothing,
            Nothing,
            200,
            0.00000001R,
            0.05R)

        Assert.IsInstanceOfType(handleObj, GetType(String), $"ORDLOGIT_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Assert.IsFalse(String.IsNullOrWhiteSpace(handle))

        Dim summary As Object(,) = UdfAssert.AsTable(Udfs.OrdinalLogitUDFs.ORDLOGIT_SUMMARY(handle))
        Assert.AreEqual("Parameter", CStr(summary(0, 0)))

        Dim classTbl As Object(,) = UdfAssert.AsTable(Udfs.OrdinalLogitUDFs.ORDLOGIT_CLASS(handle))
        Assert.AreEqual("Observed \ Predicted", CStr(classTbl(0, 0)))

        Dim newX As Object(,) = UdfBatch2Data.TakeFirstRows(x, 3)
        Dim newOffset As Object(,) = UdfBatch2Data.TakeFirstRows(offset, 3)
        Dim pred As Object(,) = UdfAssert.AsTable(Udfs.OrdinalLogitUDFs.ORDLOGIT_PRED(handle, newX, newOffset, True))

        Assert.AreEqual("PredictedCategory", CStr(pred(0, 0)))
        Assert.AreEqual("LinearPredictor", CStr(pred(0, 1)))
        Assert.AreEqual(4, pred.GetLength(0))
        Assert.AreEqual(5, pred.GetLength(1))

        For r As Integer = 1 To pred.GetLength(0) - 1
            Dim p1 As Double = Convert.ToDouble(pred(r, 2), CultureInfo.InvariantCulture)
            Dim p2 As Double = Convert.ToDouble(pred(r, 3), CultureInfo.InvariantCulture)
            Dim p3 As Double = Convert.ToDouble(pred(r, 4), CultureInfo.InvariantCulture)
            Assert.AreEqual(1.0R, p1 + p2 + p3, 0.00000001R)
        Next

        Assert.AreEqual(True, Udfs.OrdinalLogitUDFs.ORDLOGIT_DROP(handle))
        UdfAssert.IsExcelError(Udfs.OrdinalLogitUDFs.ORDLOGIT_SUMMARY(handle), "ExcelErrorNA")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ORDLOGIT_invalid_handle_returns_na()
        UdfAssert.IsExcelError(Udfs.OrdinalLogitUDFs.ORDLOGIT_SUMMARY("missing"), "ExcelErrorNA")
        UdfAssert.IsExcelError(Udfs.OrdinalLogitUDFs.ORDLOGIT_CLASS("missing"), "ExcelErrorNA")
    End Sub

End Class

<TestClass>
Public Class MultinomialLogitUdfBatch2Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MNLOGIT_handle_lifecycle_with_classification_and_prediction_works()
        Dim y As Object(,) = Nothing
        Dim x As Object(,) = Nothing
        Dim offset As Object(,) = Nothing
        Dim weights As Object(,) = Nothing

        UdfBatch2Data.LoadMultinomialUdfArgs("mlogit_dataset_grouped_basic.csv", y, x, offset, weights)

        Dim handleObj As Object = Udfs.MultinomialLogitUDFs.MNLOGIT_FIT(
            y,
            x,
            "x1,x2",
            offset,
            weights,
            "last",
            True,
            Nothing,
            Nothing,
            200,
            0.00000001R,
            0.05R)

        Assert.IsInstanceOfType(handleObj, GetType(String), $"MNLOGIT_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Assert.IsFalse(String.IsNullOrWhiteSpace(handle))

        Dim summary As Object(,) = UdfAssert.AsTable(Udfs.MultinomialLogitUDFs.MNLOGIT_SUMMARY(handle))
        Assert.AreEqual("Parameter", CStr(summary(0, 0)))

        Dim classTbl As Object(,) = UdfAssert.AsTable(Udfs.MultinomialLogitUDFs.MNLOGIT_CLASS(handle))
        Assert.AreEqual("Observed \ Predicted", CStr(classTbl(0, 0)))

        Dim newX As Object(,) = UdfBatch2Data.TakeFirstRows(x, 3)
        Dim newOffset As Object(,) = UdfBatch2Data.TakeFirstRows(offset, 3)
        Dim pred As Object(,) = UdfAssert.AsTable(Udfs.MultinomialLogitUDFs.MNLOGIT_PRED(handle, newX, newOffset, True))

        Assert.AreEqual("PredictedCategory", CStr(pred(0, 0)))
        Assert.AreEqual(4, pred.GetLength(0))
        Assert.AreEqual(6, pred.GetLength(1))

        For r As Integer = 1 To pred.GetLength(0) - 1
            Dim p1 As Double = Convert.ToDouble(pred(r, 3), CultureInfo.InvariantCulture)
            Dim p2 As Double = Convert.ToDouble(pred(r, 4), CultureInfo.InvariantCulture)
            Dim p3 As Double = Convert.ToDouble(pred(r, 5), CultureInfo.InvariantCulture)
            Assert.AreEqual(1.0R, p1 + p2 + p3, 0.00000001R)
        Next

        Assert.AreEqual(True, Udfs.MultinomialLogitUDFs.MNLOGIT_DROP(handle))
        UdfAssert.IsExcelError(Udfs.MultinomialLogitUDFs.MNLOGIT_SUMMARY(handle), "ExcelErrorNA")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MNLOGIT_invalid_handle_returns_na()
        UdfAssert.IsExcelError(Udfs.MultinomialLogitUDFs.MNLOGIT_SUMMARY("missing"), "ExcelErrorNA")
        UdfAssert.IsExcelError(Udfs.MultinomialLogitUDFs.MNLOGIT_CLASS("missing"), "ExcelErrorNA")
    End Sub

End Class
