Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Udfs = BESHStatNG.BESHStatNG.WorksheetFunctions

' Add this file to the BESHStatNG.Test project.
' With the current source layout, the UDF namespace resolves to:
'   BESHStatNG.BESHStatNG.WorksheetFunctions
' because the project RootNamespace is BESHStatNG and the files also declare
' Namespace BESHStatNG.WorksheetFunctions.
'
' This version intentionally avoids a direct compile-time reference to
' ExcelDna.Integration from the test project.

Friend Module UdfTestData

    Public Function ColUdfs(ParamArray values() As Object) As Object(,)
        Dim arr(values.Length - 1, 0) As Object
        For i As Integer = 0 To values.Length - 1
            arr(i, 0) = values(i)
        Next
        Return arr
    End Function

    Public Function RowUdfs(ParamArray values() As Object) As Object(,)
        Dim arr(0, values.Length - 1) As Object
        For j As Integer = 0 To values.Length - 1
            arr(0, j) = values(j)
        Next
        Return arr
    End Function

    Public Function MatrixForUdfs(rows As Object()()) As Object(,)
        If rows Is Nothing OrElse rows.Length = 0 Then Throw New ArgumentException("rows must not be empty")

        Dim r As Integer = rows.Length
        Dim c As Integer = rows(0).Length
        Dim arr(r - 1, c - 1) As Object

        For i As Integer = 0 To r - 1
            If rows(i) Is Nothing Then Throw New ArgumentException($"RowUdfs {i} is Nothing.")
            If rows(i).Length <> c Then Throw New ArgumentException("All rows must have the same length.")

            For j As Integer = 0 To c - 1
                arr(i, j) = rows(i)(j)
            Next
        Next

        Return arr
    End Function

End Module

Friend Module ExcelDnaCompat

    Private Const ExcelErrorTypeName As String = "ExcelDna.Integration.ExcelError, ExcelDna.Integration"
    Private Const ExcelEmptyTypeName As String = "ExcelDna.Integration.ExcelEmpty, ExcelDna.Integration"
    Private Const ExcelMissingTypeName As String = "ExcelDna.Integration.ExcelMissing, ExcelDna.Integration"

    Public Function CreateExcelErrorValue(name As String) As Object
        Dim t As Type = Type.GetType(ExcelErrorTypeName, throwOnError:=False)
        If t Is Nothing Then Return Nothing
        Return [Enum].Parse(t, name)
    End Function

    Public Function CreateExcelEmptyValue() As Object
        Return CreateSingletonValue(ExcelEmptyTypeName)
    End Function

    Public Function CreateExcelMissingValue() As Object
        Return CreateSingletonValue(ExcelMissingTypeName)
    End Function

    Private Function CreateSingletonValue(typeName As String) As Object
        Dim t As Type = Type.GetType(typeName, throwOnError:=False)
        If t Is Nothing Then Return Nothing

        Dim valueProp As PropertyInfo = t.GetProperty("Value", BindingFlags.Public Or BindingFlags.Static)
        If valueProp IsNot Nothing Then
            Return valueProp.GetValue(Nothing, Nothing)
        End If

        Dim valueField As FieldInfo = t.GetField("Value", BindingFlags.Public Or BindingFlags.Static)
        If valueField IsNot Nothing Then
            Return valueField.GetValue(Nothing)
        End If

        Return Nothing
    End Function

    Public Function IsExcelError(actual As Object, expectedName As String) As Boolean
        If actual Is Nothing Then Return False

        Dim t As Type = actual.GetType()
        If t Is Nothing Then Return False
        If String.Equals(t.FullName, "ExcelDna.Integration.ExcelError", StringComparison.Ordinal) Then
            Return String.Equals(actual.ToString(), expectedName, StringComparison.Ordinal)
        End If

        Return False
    End Function

End Module

Friend Module UdfAssert

    Public Function AsTable(actual As Object) As Object(,)
        Assert.IsNotNull(actual)
        Assert.IsInstanceOfType(actual, GetType(Object(,)))
        Return CType(actual, Object(,))
    End Function

    Public Function AsDouble(actual As Object) As Double
        Assert.IsNotNull(actual)
        Assert.IsFalse(ExcelDnaCompat.IsExcelError(actual, "ExcelErrorValue"), $"Expected numeric result but received {actual}.")
        Assert.IsFalse(ExcelDnaCompat.IsExcelError(actual, "ExcelErrorNum"), $"Expected numeric result but received {actual}.")
        Assert.IsFalse(ExcelDnaCompat.IsExcelError(actual, "ExcelErrorNA"), $"Expected numeric result but received {actual}.")
        Return Convert.ToDouble(actual)
    End Function

    Public Sub IsExcelError(actual As Object, expectedName As String)
        Assert.IsTrue(ExcelDnaCompat.IsExcelError(actual, expectedName),
                      $"Expected Excel error {expectedName}, but got: {If(actual, "<Nothing>")}.")
    End Sub

End Module

<TestClass>
Public Class UdfHelpersTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub TryGetDouble_rejects_missing_text_and_boolean_inputs()
        Assert.IsFalse(UDFhelpers.TryGetDouble(Nothing).HasValue)

        Dim emptyMarker As Object = ExcelDnaCompat.CreateExcelEmptyValue()
        If emptyMarker IsNot Nothing Then
            Assert.IsFalse(UDFhelpers.TryGetDouble(emptyMarker).HasValue)
        End If

        Dim missingMarker As Object = ExcelDnaCompat.CreateExcelMissingValue()
        If missingMarker IsNot Nothing Then
            Assert.IsFalse(UDFhelpers.TryGetDouble(missingMarker).HasValue)
        End If

        Assert.IsFalse(UDFhelpers.TryGetDouble("12.5").HasValue)
        Assert.IsFalse(UDFhelpers.TryGetDouble(True).HasValue)

        Dim errorMarker As Object = ExcelDnaCompat.CreateExcelErrorValue("ExcelErrorValue")
        If errorMarker IsNot Nothing Then
            Assert.IsFalse(UDFhelpers.TryGetDouble(errorMarker).HasValue)
        End If
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GetOptionalBool_parses_common_aliases()
        Assert.IsTrue(UDFhelpers.GetOptionalBool("yes", False))
        Assert.IsTrue(UDFhelpers.GetOptionalBool("1", False))
        Assert.IsTrue(UDFhelpers.GetOptionalBool("TRUE", False))

        Assert.IsFalse(UDFhelpers.GetOptionalBool("no", True))
        Assert.IsFalse(UDFhelpers.GetOptionalBool("0", True))
        Assert.IsFalse(UDFhelpers.GetOptionalBool("FALSE", True))

        Assert.IsTrue(UDFhelpers.GetOptionalBool("maybe", True))

        Dim missingMarker As Object = ExcelDnaCompat.CreateExcelMissingValue()
        If missingMarker IsNot Nothing Then
            Assert.IsFalse(UDFhelpers.GetOptionalBool(missingMarker, False))
        End If
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GetVarNames_accepts_csv_and_falls_back_when_count_is_wrong()
        CollectionAssert.AreEqual(
            New String() {"dose", "age", "stage"},
            UDFhelpers.GetVarNames("dose, age, stage", 3))

        CollectionAssert.AreEqual(
            New String() {"X1", "X2", "X3"},
            UDFhelpers.GetVarNames("dose, age", 3))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub TryReadNumericColumn_skips_header_trims_blanks_and_marks_invalid_cells_as_nan()
        Dim trailingBlank As Object = ExcelDnaCompat.CreateExcelEmptyValue()
        If trailingBlank Is Nothing Then trailingBlank = Nothing

        Dim src As Object(,) = UdfTestData.ColUdfs("Weight", 10.0R, 11.5R, "oops", 13.0R, trailingBlank, Nothing)

        Dim values As List(Of Double) = Nothing
        Dim ok As Boolean = UDFhelpers.TryReadNumericColumn(src, values)

        Assert.IsTrue(ok)
        Assert.IsNotNull(values)
        Assert.AreEqual(4, values.Count)
        Assert.AreEqual(10.0R, values(0), 0.000000000001)
        Assert.AreEqual(11.5R, values(1), 0.000000000001)
        Assert.IsTrue(Double.IsNaN(values(2)))
        Assert.AreEqual(13.0R, values(3), 0.000000000001)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub TryReadNumericMatrix_skips_header_and_marks_invalid_cells_as_nan()
        Dim trailingBlank As Object = ExcelDnaCompat.CreateExcelEmptyValue()
        If trailingBlank Is Nothing Then trailingBlank = Nothing

        Dim src As Object(,) = UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {"X1", "X2"},
                New Object() {1.0R, 2.0R},
                New Object() {3.0R, "bad"},
                New Object() {5.0R, 6.0R},
                New Object() {trailingBlank, trailingBlank}
            })

        Dim mat(,) As Double = Nothing
        Dim rows As Integer = 0
        Dim cols As Integer = 0

        Dim ok As Boolean = UDFhelpers.TryReadNumericMatrix(src, mat, rows, cols)

        Assert.IsTrue(ok)
        Assert.AreEqual(3, rows)
        Assert.AreEqual(2, cols)
        Assert.AreEqual(1.0R, mat(0, 0), 0.000000000001)
        Assert.AreEqual(2.0R, mat(0, 1), 0.000000000001)
        Assert.AreEqual(3.0R, mat(1, 0), 0.000000000001)
        Assert.IsTrue(Double.IsNaN(mat(1, 1)))
        Assert.AreEqual(5.0R, mat(2, 0), 0.000000000001)
        Assert.AreEqual(6.0R, mat(2, 1), 0.000000000001)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ParseFormulaAddressingMode_recognizes_aliases_and_defaults()
        Assert.AreEqual("relative", UDFhelpers.ParseFormulaAddressingMode("rel", "names"))
        Assert.AreEqual("absolute", UDFhelpers.ParseFormulaAddressingMode("worksheet", "names"))
        Assert.AreEqual("names", UDFhelpers.ParseFormulaAddressingMode("variables", "relative"))
        Assert.AreEqual("relative", UDFhelpers.ParseFormulaAddressingMode("something-else", "relative"))
    End Sub

End Class

<TestClass>
Public Class SampleSizeUdfTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SSIZE_TTEST_PAIRED_returns_expected_table()
        Dim result As Object = Udfs.SampleSizeUDFs.SSIZE_TTEST_PAIRED(5.0R, 10.0R, 0.05R, 0.2R)
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Assert.AreEqual(2, tbl.GetLength(0))
        Assert.AreEqual(2, tbl.GetLength(1))
        Assert.AreEqual("Metric", CStr(tbl(0, 0)))
        Assert.AreEqual("Value", CStr(tbl(0, 1)))
        Assert.AreEqual("Required pairs", CStr(tbl(1, 0)))
        Assert.AreEqual(34, Convert.ToInt32(tbl(1, 1)))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SSIZE_TTEST_PAIRED_invalid_alpha_returns_num()
        Dim result As Object = Udfs.SampleSizeUDFs.SSIZE_TTEST_PAIRED(5.0R, 10.0R, 1.0R, 0.2R)
        UdfAssert.IsExcelError(result, "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SSIZE_TTEST_PAIRED_non_numeric_input_returns_value()
        Dim result As Object = Udfs.SampleSizeUDFs.SSIZE_TTEST_PAIRED("x", 10.0R, 0.05R, 0.2R)
        UdfAssert.IsExcelError(result, "ExcelErrorValue")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SSIZE_TTEST_UNPAIRED_returns_expected_group_table()
        Dim result As Object = Udfs.SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(5.0R, 10.0R, 1.0R, 0.05R, 0.2R)
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Assert.AreEqual("Group", CStr(tbl(0, 0)))
        Assert.AreEqual("Required subjects", CStr(tbl(0, 1)))
        Assert.AreEqual("Controls", CStr(tbl(1, 0)))
        Assert.AreEqual(64, Convert.ToInt32(tbl(1, 1)))
        Assert.AreEqual("Experimental", CStr(tbl(2, 0)))
        Assert.AreEqual(64, Convert.ToInt32(tbl(2, 1)))
    End Sub

End Class

<TestClass>
Public Class DistributionUdfTests

    Private Const TOL As Double = 0.0000000001

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub F_PDF_matches_core_distribution()
        Dim actual As Double = UdfAssert.AsDouble(Udfs.DistributionUDFs.F_PDF(3.2R, 5.0R, 10.0R))
        Dim expected As Double = distributions.F_PDF(3.2R, 5.0R, 10.0R)

        Assert.AreEqual(expected, actual, TOL)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub F_PDF_handles_x_zero_special_cases()
        Assert.AreEqual(0.0R, UdfAssert.AsDouble(Udfs.DistributionUDFs.F_PDF(0.0R, 5.0R, 10.0R)), 0.0R)
        Assert.AreEqual(1.0R, UdfAssert.AsDouble(Udfs.DistributionUDFs.F_PDF(0.0R, 2.0R, 10.0R)), 0.0R)
        UdfAssert.IsExcelError(Udfs.DistributionUDFs.F_PDF(0.0R, 1.5R, 10.0R), "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub PRTRNG_and_tail_sum_to_one()
        Dim cdf As Double = UdfAssert.AsDouble(Udfs.DistributionUDFs.PRTRNG(3.5R, 20.0R, 5.0R))
        Dim tail As Double = UdfAssert.AsDouble(Udfs.DistributionUDFs.PRTRNG_TAIL(3.5R, 20.0R, 5.0R))

        Assert.AreEqual(1.0R, cdf + tail, 0.000000000001)
        Assert.IsTrue(cdf >= 0.0R AndAlso cdf <= 1.0R)
        Assert.IsTrue(tail >= 0.0R AndAlso tail <= 1.0R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub PRTRNG_invalid_inputs_return_num()
        UdfAssert.IsExcelError(Udfs.DistributionUDFs.PRTRNG(-1.0R, 20.0R, 5.0R), "ExcelErrorNum")
        UdfAssert.IsExcelError(Udfs.DistributionUDFs.PRTRNG(3.5R, 0.0R, 5.0R), "ExcelErrorNum")
        UdfAssert.IsExcelError(Udfs.DistributionUDFs.PRTRNG(3.5R, 20.0R, 1.0R), "ExcelErrorNum")
    End Sub

End Class

<TestClass>
Public Class RegressionFormulaUdfTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub FORMULA_VALIDATE_names_mode_accepts_single_quoted_variable_names()
        Dim x As Object(,) = UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {1.0R, 2.0R, 9.0R},
                New Object() {3.0R, 4.0R, 8.0R},
                New Object() {5.0R, 6.0R, 7.0R},
                New Object() {7.0R, 8.0R, 6.0R}
            })

        Dim result As Object = Udfs.RegressionFormulaUDFs.FORMULA_VALIDATE("'prison' + 'dose' + 'dose'^2", x, "prison,dose,clinic", "names")

        Assert.AreEqual(True, result)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub FORMULA_VALIDATE_names_mode_rejects_bare_column_letters()
        Dim x As Object(,) = UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {1.0R, 2.0R, 9.0R},
                New Object() {3.0R, 4.0R, 8.0R},
                New Object() {5.0R, 6.0R, 7.0R},
                New Object() {7.0R, 8.0R, 6.0R}
            })

        Dim result As Object = Udfs.RegressionFormulaUDFs.FORMULA_VALIDATE("A + 'dose'", x, "prison,dose,clinic", "names")

        Assert.IsInstanceOfType(result, GetType(String))
        StringAssert.Contains(CStr(result), "Unknown variable reference 'A'")
    End Sub

End Class

<TestClass>
Public Class LinearModelUdfTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub LM_handle_lifecycle_works()
        Dim y As Object(,) = UdfTestData.ColUdfs(2.0R, 4.0R, 5.0R, 8.0R, 11.0R)
        Dim x As Object(,) = UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {1.0R, 0.0R},
                New Object() {2.0R, 1.0R},
                New Object() {3.0R, 0.0R},
                New Object() {4.0R, 1.0R},
                New Object() {5.0R, 2.0R}
            })

        Dim handleObj As Object = Udfs.LinearModelUDFs.LM_FIT(y, x, "dose,age")

        Assert.IsInstanceOfType(handleObj, GetType(String), $"LM_FIT returned unexpected value: {If(handleObj, "<Nothing>")}")

        Dim handle As String = CStr(handleObj)
        Assert.IsFalse(String.IsNullOrWhiteSpace(handle))

        Dim summary As Object = Udfs.LinearModelUDFs.LM_SUMMARY(handle)
        Dim summaryTable As Object(,) = UdfAssert.AsTable(summary)
        Assert.AreEqual("Parameter", CStr(summaryTable(0, 0)))

        Dim dropResult As Object = Udfs.LinearModelUDFs.LM_DROP(handle)
        Assert.AreEqual(True, dropResult)

        Dim afterDrop As Object = Udfs.LinearModelUDFs.LM_SUMMARY(handle)
        UdfAssert.IsExcelError(afterDrop, "ExcelErrorNA")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub LM_SUMMARY_invalid_handle_returns_na()
        UdfAssert.IsExcelError(Udfs.LinearModelUDFs.LM_SUMMARY("does-not-exist"), "ExcelErrorNA")
    End Sub

End Class
