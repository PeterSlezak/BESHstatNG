Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Udfs = BESHStatNG.BESHStatNG.WorksheetFunctions

' Batch 3 UDF tests.
' This file assumes the helpers from src\UdfTests.vb already exist:
'   - UdfTestData
'   - UdfAssert
'   - ExcelDnaCompat

Friend Module UdfBatch3Data

    Public Function NormalitySampleWithHeader() As Object(,)
        Return UdfTestData.ColUdfs(
            "Sample",
            -1.2R, -0.7R, -0.3R, 0.0R, 0.15R,
            0.32R, 0.5R, 0.9R, 1.1R, 1.3R,
            -0.1R, 0.22R, 0.45R, -0.55R, 0.78R,
            1.05R, -0.95R, 0.6R, -0.4R, 0.12R)
    End Function

    Public Function VarianceGroupsWithHeaders() As Object(,)
        Return UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {"G1", "G2", "G3"},
                New Object() {10.2R, 10.1R, 9.5R},
                New Object() {9.8R, 10.0R, 9.7R},
                New Object() {10.0R, 9.9R, 9.8R},
                New Object() {10.5R, 10.2R, 9.6R},
                New Object() {9.7R, 10.3R, 9.9R},
                New Object() {10.1R, 9.8R, 9.4R},
                New Object() {10.3R, 10.1R, 9.6R},
                New Object() {9.9R, 10.0R, 9.7R}
            })
    End Function

    Public Function RepeatedMeasuresDataWithHeaders() As Object(,)
        Return UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {"C1", "C2", "C3"},
                New Object() {10.0R, 11.0R, 12.0R},
                New Object() {10.0R, 12.0R, 12.0R},
                New Object() {9.0R, 11.0R, 13.0R},
                New Object() {11.0R, 10.0R, 12.0R},
                New Object() {10.0R, 11.0R, 11.0R},
                New Object() {9.0R, 10.0R, 12.0R}
            })
    End Function

    Public Function IndependentGroupsForTTests() As Tuple(Of Object(,), Object(,))
        Dim x As Object(,) = UdfTestData.ColUdfs("Control", 1.0R, 2.0R, 3.0R, 4.0R, 5.0R)
        Dim y As Object(,) = UdfTestData.ColUdfs("Treatment", 2.0R, 3.0R, 4.0R, 5.0R, 6.0R)
        Return Tuple.Create(x, y)
    End Function

    Public Function PairedSamplesForTTests() As Tuple(Of Object(,), Object(,))
        Dim x As Object(,) = UdfTestData.ColUdfs("Before", 10.0R, 11.0R, 12.0R, 13.0R, 14.0R)
        Dim y As Object(,) = UdfTestData.ColUdfs("After", 10.5R, 11.5R, 12.5R, 12.0R, 14.0R)
        Return Tuple.Create(x, y)
    End Function

    Public Function MismatchedPairedSamples() As Tuple(Of Object(,), Object(,))
        Dim x As Object(,) = UdfTestData.ColUdfs("Before", 10.0R, 11.0R, 12.0R, 13.0R)
        Dim y As Object(,) = UdfTestData.ColUdfs("After", 10.5R, 11.5R, 12.5R)
        Return Tuple.Create(x, y)
    End Function

    Public Function NestedAnovaDataWithHeaders() As Object(,)
        Return UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {"Group", "Subgroup", "Response"},
                New Object() {"A", "A1", 10.0R},
                New Object() {"A", "A1", 11.0R},
                New Object() {"A", "A1", 12.0R},
                New Object() {"A", "A2", 13.0R},
                New Object() {"A", "A2", 14.0R},
                New Object() {"A", "A2", 15.0R},
                New Object() {"B", "B1", 20.0R},
                New Object() {"B", "B1", 21.0R},
                New Object() {"B", "B1", 22.0R},
                New Object() {"B", "B2", 23.0R},
                New Object() {"B", "B2", 24.0R},
                New Object() {"B", "B2", 25.0R}
            })
    End Function

    Public Function BoxMDataMatrix() As Object(,)
        Return UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {"X1", "X2"},
                New Object() {1.0R, 2.0R},
                New Object() {2.0R, 1.5R},
                New Object() {1.5R, 2.2R},
                New Object() {2.2R, 1.8R},
                New Object() {1.8R, 2.4R},
                New Object() {3.0R, 2.9R},
                New Object() {2.8R, 3.2R},
                New Object() {3.3R, 2.7R},
                New Object() {3.1R, 3.4R},
                New Object() {2.9R, 3.0R},
                New Object() {4.8R, 5.2R},
                New Object() {5.1R, 4.9R},
                New Object() {5.3R, 5.4R},
                New Object() {4.9R, 5.0R},
                New Object() {5.2R, 5.1R}
            })
    End Function

    Public Function BoxMGroups() As Object(,)
        Return UdfTestData.ColUdfs(
            "Group",
            "A", "A", "A", "A", "A",
            "B", "B", "B", "B", "B",
            "C", "C", "C", "C", "C")
    End Function

    Public Function GrubbsDataWithHeader() As Object(,)
        Return UdfTestData.ColUdfs(
            "Value",
            10.0R, 12.0R, 12.0R, 13.0R, 12.0R,
            11.0R, 10.0R, 12.0R, 13.0R, 12.0R,
            11.0R, 10.0R, 12.0R, 13.0R, 12.0R,
            11.0R, 10.0R, 12.0R, 13.0R, 100.0R)
    End Function

    Public Function RosnerDataWithHeader() As Object(,)
        Return UdfTestData.ColUdfs(
            "Value",
            10.0R, 11.0R, 12.0R, 11.0R, 10.0R,
            12.0R, 13.0R, 11.0R, 10.0R, 12.0R,
            11.0R, 12.0R, 10.0R, 11.0R, 12.0R,
            11.0R, 10.0R, 12.0R, 13.0R, 11.0R,
            10.0R, 12.0R, 11.0R, 50.0R, 100.0R)
    End Function

    Public Function SmallGroup1() As Object(,)
        Return UdfTestData.ColUdfs("G1", 1.0R, 2.0R, 3.0R)
    End Function

    Public Function SmallGroup2() As Object(,)
        Return UdfTestData.ColUdfs("G2", 4.0R, 5.0R, 6.0R)
    End Function

    Public Function LargeGroup(startValue As Integer, count As Integer) As Object(,)
        Dim values(count) As Object
        values(0) = "G"
        For i As Integer = 1 To count
            values(i) = CDbl(startValue + i - 1)
        Next
        Dim arr(values.Length - 1, 0) As Object
        For i As Integer = 0 To values.Length - 1
            arr(i, 0) = values(i)
        Next
        Return arr
    End Function

    Public Function CorrelationX() As Object(,)
        Return UdfTestData.ColUdfs("X", 1.0R, 2.0R, 3.0R, 4.0R, 5.0R, 6.0R)
    End Function

    Public Function CorrelationYPerfect() As Object(,)
        Return UdfTestData.ColUdfs("Y", 10.0R, 20.0R, 30.0R, 40.0R, 50.0R, 60.0R)
    End Function

End Module

Friend Module UdfBatch3TableHelpers

    Public Function FindRowIndexByLabel(table As Object(,), label As String) As Integer
        For r As Integer = 0 To table.GetLength(0) - 1
            Dim s As String = Convert.ToString(table(r, 0), CultureInfo.InvariantCulture)
            If String.Equals(s, label, StringComparison.OrdinalIgnoreCase) Then
                Return r
            End If
        Next
        Assert.Fail($"Could not find row label '{label}'.")
        Return -1
    End Function

    Public Function FindRowValue(table As Object(,), label As String) As Object
        Return table(FindRowIndexByLabel(table, label), 1)
    End Function

End Module

<TestClass>
Public Class AssumptionsUdfBatch3Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SHAPIRO_WILK_with_header_matches_core_result()
        Dim data As Object(,) = UdfBatch3Data.NormalitySampleWithHeader()
        Dim result As Object = Udfs.AssumptionsUDFs.SHAPIRO_WILK(data)
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Dim sample() As Double = {-1.2R, -0.7R, -0.3R, 0.0R, 0.15R, 0.32R, 0.5R, 0.9R, 1.1R, 1.3R,
                                  -0.1R, 0.22R, 0.45R, -0.55R, 0.78R, 1.05R, -0.95R, 0.6R, -0.4R, 0.12R}
        Dim err As String = String.Empty
        Dim core = Global.BESHStatNG.assumptions.Assumptions.ShapiroWilk(sample, err)

        Assert.AreEqual("Shapiro-Wilk Test", Convert.ToString(tbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("W statistic", Convert.ToString(tbl(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Two-sided p-value", Convert.ToString(tbl(2, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(core.TestStatistics1, Convert.ToDouble(tbl(1, 1), CultureInfo.InvariantCulture), 0.0000000001R)
        Assert.AreEqual(core.Pvalue, Convert.ToDouble(tbl(2, 1), CultureInfo.InvariantCulture), 0.0000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub LEVENE_mean_and_median_centers_return_valid_tables_and_invalid_center_returns_value()
        Dim groups As Object(,) = UdfBatch3Data.VarianceGroupsWithHeaders()

        Dim meanTbl As Object(,) = UdfAssert.AsTable(Udfs.AssumptionsUDFs.LEVENE(groups, "mean"))
        Dim medianTbl As Object(,) = UdfAssert.AsTable(Udfs.AssumptionsUDFs.LEVENE(groups, "brown-forsythe"))

        Assert.AreEqual("Levene Test", Convert.ToString(meanTbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Brown-Forsythe Test", Convert.ToString(medianTbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("F statistic", Convert.ToString(meanTbl(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("P-value", Convert.ToString(meanTbl(2, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("F statistic", Convert.ToString(medianTbl(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("P-value", Convert.ToString(medianTbl(2, 0), CultureInfo.InvariantCulture))

        Dim pMean As Double = Convert.ToDouble(meanTbl(2, 1), CultureInfo.InvariantCulture)
        Dim pMedian As Double = Convert.ToDouble(medianTbl(2, 1), CultureInfo.InvariantCulture)
        Assert.IsTrue(pMean >= 0.0R AndAlso pMean <= 1.0R)
        Assert.IsTrue(pMedian >= 0.0R AndAlso pMedian <= 1.0R)
        Assert.AreNotEqual(Convert.ToDouble(meanTbl(1, 1), CultureInfo.InvariantCulture),
                           Convert.ToDouble(medianTbl(1, 1), CultureInfo.InvariantCulture))

        UdfAssert.IsExcelError(Udfs.AssumptionsUDFs.LEVENE(groups, "not-a-center"), "ExcelErrorValue")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub BOX_M_returns_valid_table_for_multivariate_grouped_input()
        Dim result As Object = Udfs.AssumptionsUDFs.BOX_M(UdfBatch3Data.BoxMDataMatrix(), UdfBatch3Data.BoxMGroups())
        Dim tbl As Object(,) = UdfAssert.AsTable(result)

        Assert.AreEqual("Box's Test of Equality of Covariance Matrices", Convert.ToString(tbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("M statistic", Convert.ToString(tbl(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("P-value", Convert.ToString(tbl(2, 0), CultureInfo.InvariantCulture))
        Assert.IsTrue(Convert.ToDouble(tbl(1, 1), CultureInfo.InvariantCulture) >= 0.0R)

        Dim p As Double = Convert.ToDouble(tbl(2, 1), CultureInfo.InvariantCulture)
        Assert.IsTrue(p >= 0.0R AndAlso p <= 1.0R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MAUCHLY_matches_core_result_for_repeated_measures_matrix()
        Dim data As Object(,) = UdfBatch3Data.RepeatedMeasuresDataWithHeaders()
        Dim tbl As Object(,) = UdfAssert.AsTable(Udfs.AssumptionsUDFs.MAUCHLY(data))

        Dim mat(,) As Double = {
            {10.0R, 11.0R, 12.0R},
            {10.0R, 12.0R, 12.0R},
            {9.0R, 11.0R, 13.0R},
            {11.0R, 10.0R, 12.0R},
            {10.0R, 11.0R, 11.0R},
            {9.0R, 10.0R, 12.0R}}
        Dim core = Global.BESHStatNG.assumptions.Assumptions.MauchlyTest(mat)

        Assert.AreEqual("Mauchly's Test of Sphericity", Convert.ToString(tbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(core.TestStatistics1, Convert.ToDouble(tbl(1, 1), CultureInfo.InvariantCulture), 0.000000001R)
        Assert.AreEqual(core.Pvalue, Convert.ToDouble(tbl(2, 1), CultureInfo.InvariantCulture), 0.000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub GRUBBS_and_ROSNER_detect_outliers_and_validate_alpha_or_sample_size()
        Dim grubbs As Object(,) = UdfAssert.AsTable(Udfs.AssumptionsUDFs.GRUBBS(UdfBatch3Data.GrubbsDataWithHeader(), 0.05R))
        Assert.AreEqual("Grubbs Test", Convert.ToString(grubbs(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Alpha", Convert.ToString(grubbs(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Critical statistic", Convert.ToString(grubbs(2, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Observed statistic", Convert.ToString(grubbs(3, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Result", Convert.ToString(grubbs(4, 0), CultureInfo.InvariantCulture))
        Assert.IsTrue(Convert.ToDouble(grubbs(3, 1), CultureInfo.InvariantCulture) > Convert.ToDouble(grubbs(2, 1), CultureInfo.InvariantCulture))
        StringAssert.Contains(Convert.ToString(grubbs(4, 1), CultureInfo.InvariantCulture), "outlier")
        UdfAssert.IsExcelError(Udfs.AssumptionsUDFs.GRUBBS(UdfBatch3Data.GrubbsDataWithHeader(), 1.0R), "ExcelErrorNum")

        Dim rosner As Object(,) = UdfAssert.AsTable(Udfs.AssumptionsUDFs.ROSNER(UdfBatch3Data.RosnerDataWithHeader(), 0.05R))
        Assert.AreEqual("Rosner Generalized ESD Test", Convert.ToString(rosner(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(2.0R, Convert.ToDouble(UdfBatch3TableHelpers.FindRowValue(rosner, "Number of outliers"), CultureInfo.InvariantCulture), 0.0R)

        Dim few As Object(,) = UdfTestData.ColUdfs("Value", 1.0R, 2.0R, 3.0R, 4.0R, 5.0R)
        UdfAssert.IsExcelError(Udfs.AssumptionsUDFs.ROSNER(few, 0.05R), "ExcelErrorNum")
    End Sub

End Class

<TestClass>
Public Class ParametricUdfBatch3Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ANOVA1_WELCH_matches_core_summary_contract()
        Dim groups As Object(,) = UdfBatch3Data.VarianceGroupsWithHeaders()
        Dim tbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.ANOVA1_WELCH(groups))

        Dim g1() As Double = {10.2R, 9.8R, 10.0R, 10.5R, 9.7R, 10.1R, 10.3R, 9.9R}
        Dim g2() As Double = {10.1R, 10.0R, 9.9R, 10.2R, 10.3R, 9.8R, 10.1R, 10.0R}
        Dim g3() As Double = {9.5R, 9.7R, 9.8R, 9.6R, 9.9R, 9.4R, 9.6R, 9.7R}
        Dim mdl As New Global.BESHStatNG.parametric.OneWayANOVA(New Double()() {g1, g2, g3}, New String() {"G1", "G2", "G3"})
        mdl.compute()
        Dim welch = mdl.WelshANOVA()

        Assert.AreEqual("Source", Convert.ToString(tbl(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("Welch ANOVA", Convert.ToString(tbl(1, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(2.0R, Convert.ToDouble(tbl(1, 1), CultureInfo.InvariantCulture), 0.0R)
        Assert.AreEqual(welch.DF1, Convert.ToDouble(tbl(1, 2), CultureInfo.InvariantCulture), 0.000000000001R)
        Assert.AreEqual(welch.TestStatistics1, Convert.ToDouble(tbl(1, 3), CultureInfo.InvariantCulture), 0.000000000001R)
        Assert.AreEqual(welch.Pvalue, Convert.ToDouble(tbl(1, 4), CultureInfo.InvariantCulture), 0.000000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub RMANOVA1_and_RMANOVA1_MCP_output_selectors_work()
        Dim data As Object(,) = UdfBatch3Data.RepeatedMeasuresDataWithHeaders()

        Dim noneTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.RMANOVA1(data, Nothing, "none"))
        Dim bothTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.RMANOVA1(data, Nothing, "both"))
        Assert.IsTrue(bothTbl.GetLength(0) >= noneTbl.GetLength(0))
        Assert.IsTrue(bothTbl.GetLength(1) >= noneTbl.GetLength(1))
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.RMANOVA1(data, Nothing, "bogus"), "ExcelErrorValue")

        Dim defaultMcp As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.RMANOVA1_MCP(data, Nothing, Nothing, 0.05R))
        Dim allMcp As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.RMANOVA1_MCP(data, Nothing, "all", 0.05R))
        Assert.IsTrue(defaultMcp.GetLength(0) > 0 AndAlso defaultMcp.GetLength(1) >= 4)
        Assert.IsTrue(allMcp.GetLength(0) > defaultMcp.GetLength(0))
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.RMANOVA1_MCP(data, Nothing, "unknown", 0.05R), "ExcelErrorValue")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub ANOVA1_MCP_and_ANOVA2_NESTED_output_selectors_work()
        Dim groups As Object(,) = UdfBatch3Data.VarianceGroupsWithHeaders()

        Dim tukeyTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.ANOVA1_MCP(groups, Nothing, "tukey", 0.05R))
        Dim allTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.ANOVA1_MCP(groups, Nothing, "all", 0.05R))
        Assert.IsTrue(tukeyTbl.GetLength(0) > 0 AndAlso tukeyTbl.GetLength(1) >= 4)
        Assert.IsTrue(allTbl.GetLength(0) > tukeyTbl.GetLength(0))
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.ANOVA1_MCP(groups, Nothing, "tukey", 1.0R), "ExcelErrorNum")
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.ANOVA1_MCP(groups, Nothing, "unknown", 0.05R), "ExcelErrorValue")

        Dim nested As Object(,) = UdfBatch3Data.NestedAnovaDataWithHeaders()
        Dim mainTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.ANOVA2_NESTED(nested, Nothing, "main"))
        Dim bothNested As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.ANOVA2_NESTED(nested, Nothing, "both"))
        Assert.IsTrue(mainTbl.GetLength(0) > 0 AndAlso mainTbl.GetLength(1) >= 4)
        Assert.IsTrue(bothNested.GetLength(0) >= mainTbl.GetLength(0))
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.ANOVA2_NESTED(nested, Nothing, "???"), "ExcelErrorValue")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub TTEST_UNPAIRED_and_TTEST_PAIRED_contracts_work()
        Dim ind = UdfBatch3Data.IndependentGroupsForTTests()
        Dim bothTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.TTEST_UNPAIRED(ind.Item1, ind.Item2, Nothing, "both", 0.05R))
        Dim equalTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.TTEST_UNPAIRED(ind.Item1, ind.Item2, Nothing, "equal", 0.05R))
        Dim welchTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.TTEST_UNPAIRED(ind.Item1, ind.Item2, Nothing, "welch", 0.05R))
        Assert.IsTrue(equalTbl.GetLength(0) > 0)
        Assert.IsTrue(welchTbl.GetLength(0) > 0)
        Assert.IsTrue(bothTbl.GetLength(0) > equalTbl.GetLength(0))
        Assert.IsTrue(bothTbl.GetLength(0) > welchTbl.GetLength(0))
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.TTEST_UNPAIRED(ind.Item1, ind.Item2, Nothing, "welch", 1.0R), "ExcelErrorNum")
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.TTEST_UNPAIRED(ind.Item1, ind.Item2, Nothing, "bogus", 0.05R), "ExcelErrorValue")

        Dim paired = UdfBatch3Data.PairedSamplesForTTests()
        Dim pairedTbl As Object(,) = UdfAssert.AsTable(Udfs.ParametricUDFs.TTEST_PAIRED(paired.Item1, paired.Item2, "Before,After"))
        Assert.IsTrue(pairedTbl.GetLength(0) >= 2 AndAlso pairedTbl.GetLength(1) >= 2)

        Dim badPairs = UdfBatch3Data.MismatchedPairedSamples()
        UdfAssert.IsExcelError(Udfs.ParametricUDFs.TTEST_PAIRED(badPairs.Item1, badPairs.Item2, Nothing), "ExcelErrorValue")
    End Sub

End Class

<TestClass>
Public Class NonparametricUdfBatch3Tests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub MW_P_EXACT_matches_core_result_and_validates_side_and_sample_limit()
        Dim g1 As Object(,) = UdfBatch3Data.SmallGroup1()
        Dim g2 As Object(,) = UdfBatch3Data.SmallGroup2()

        Dim mw As New nonparametric.MannWhitney(New Double()() {New Double() {1.0R, 2.0R, 3.0R}, New Double() {4.0R, 5.0R, 6.0R}}, "g1", "g2")
        Dim res As TestResult = mw.Compute()

        Assert.AreEqual(res.PvalueExact, UdfAssert.AsDouble(Udfs.NonparametricUDFs.MW_P_EXACT(g1, g2, "two")), 0.000000000001R)
        Assert.AreEqual(res.pValueExactLowerSide, UdfAssert.AsDouble(Udfs.NonparametricUDFs.MW_P_EXACT(g1, g2, "lower")), 0.000000000001R)
        Assert.AreEqual(res.pValueExactUpperSide, UdfAssert.AsDouble(Udfs.NonparametricUDFs.MW_P_EXACT(g1, g2, "upper")), 0.000000000001R)
        Assert.AreEqual(res.Pvalue, UdfAssert.AsDouble(Udfs.NonparametricUDFs.MW_P_NORM(g1, g2)), 0.000000000001R)
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.MW_P_EXACT(g1, g2, "bad-side"), "ExcelErrorValue")

        Dim large1 As Object(,) = UdfBatch3Data.LargeGroup(1, 26)
        Dim large2 As Object(,) = UdfBatch3Data.LargeGroup(101, 26)
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.MW_P_EXACT(large1, large2, "two"), "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub WILCOX_P_NORM_and_EXACT_return_valid_probs_and_validate_pairing()
        Dim paired = UdfBatch3Data.PairedSamplesForTTests()

        Dim pNorm As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.WILCOX_P_NORM(paired.Item1, paired.Item2))
        Dim pExact As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.WILCOX_P_EXACT(paired.Item1, paired.Item2, "two"))
        Assert.IsTrue(pNorm >= 0.0R AndAlso pNorm <= 1.0R)
        Assert.IsTrue(pExact >= 0.0R AndAlso pExact <= 1.0R)
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.WILCOX_P_EXACT(paired.Item1, paired.Item2, "wrong"), "ExcelErrorValue")

        Dim badPairs = UdfBatch3Data.MismatchedPairedSamples()
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.WILCOX_P_NORM(badPairs.Item1, badPairs.Item2), "ExcelErrorValue")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub SPEARMAN_and_KENDALL_return_perfect_monotone_association_and_validate_alpha()
        Dim x As Object(,) = UdfBatch3Data.CorrelationX()
        Dim y As Object(,) = UdfBatch3Data.CorrelationYPerfect()

        Dim rho As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.SPEARMAN_RHO(x, y, 0.05R))
        Dim pRho As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.SPEARMAN_P(x, y, 0.05R))
        Assert.AreEqual(1.0R, rho, 0.000000000001R)
        Assert.IsTrue(pRho >= 0.0R AndAlso pRho <= 1.0R)

        Dim tau As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.KENDALL_TAU(x, y, 0.05R))
        Dim pTau As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.KENDALL_P(x, y, 0.05R))
        Assert.AreEqual(1.0R, tau, 0.000000000001R)
        Assert.IsTrue(pTau >= 0.0R AndAlso pTau <= 1.0R)

        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.SPEARMAN_RHO(x, y, 1.0R), "ExcelErrorNum")
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.KENDALL_P(x, y, 1.0R), "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub KW_STAT_and_P_follow_chi_square_relationship_and_MCP_returns_table()
        Dim groups As Object(,) = UdfBatch3Data.VarianceGroupsWithHeaders()

        Dim hCor As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.KW_STAT(groups, "Hcor"))
        Dim pCor As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.KW_P(groups, "Hcor"))
        Dim expectedP As Double = 1.0R - Global.BESHStatNG.distributions.ChiSquareCDF(hCor, 2.0R)

        Assert.IsTrue(hCor >= 0.0R)
        Assert.AreEqual(expectedP, pCor, 0.0000000001R)
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.KW_STAT(groups, "bad"), "ExcelErrorValue")

        Dim dunnTbl As Object(,) = UdfAssert.AsTable(Udfs.NonparametricUDFs.KW_MCP(groups, Nothing, 0.05R))
        Assert.IsTrue(dunnTbl.GetLength(0) > 0 AndAlso dunnTbl.GetLength(1) >= 4)
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.KW_MCP(groups, Nothing, 1.0R), "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub FRIEDMAN_STAT_and_P_return_valid_values_and_MCP_method_selector_works()
        Dim data As Object(,) = UdfBatch3Data.RepeatedMeasuresDataWithHeaders()

        Dim t1 As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.FRIEDMAN_STAT(data, "T1"))
        Dim p1 As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.FRIEDMAN_P(data, "T1"))
        Dim t2 As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.FRIEDMAN_STAT(data, "T2"))
        Dim p2 As Double = UdfAssert.AsDouble(Udfs.NonparametricUDFs.FRIEDMAN_P(data, "T2"))

        Assert.IsTrue(t1 >= 0.0R)
        Assert.IsTrue(t2 >= 0.0R)
        Assert.IsTrue(p1 >= 0.0R AndAlso p1 <= 1.0R)
        Assert.IsTrue(p2 >= 0.0R AndAlso p2 <= 1.0R)

        Dim dunnTbl As Object(,) = UdfAssert.AsTable(Udfs.NonparametricUDFs.FRIEDMAN_MCP(data, Nothing, "dunn", 0.05R))
        Dim allTbl As Object(,) = UdfAssert.AsTable(Udfs.NonparametricUDFs.FRIEDMAN_MCP(data, Nothing, "all", 0.05R))
        Assert.IsTrue(dunnTbl.GetLength(0) > 0 AndAlso dunnTbl.GetLength(1) >= 4)
        Assert.IsTrue(allTbl.GetLength(0) > dunnTbl.GetLength(0))
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.FRIEDMAN_MCP(data, Nothing, "bad-method", 0.05R), "ExcelErrorValue")
        UdfAssert.IsExcelError(Udfs.NonparametricUDFs.FRIEDMAN_MCP(data, Nothing, "dunn", 1.0R), "ExcelErrorNum")
    End Sub

End Class
