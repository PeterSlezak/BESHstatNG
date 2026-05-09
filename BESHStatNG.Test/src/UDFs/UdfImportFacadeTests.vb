Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfImportFacadeTests

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Histogram_bin_rule_normalization_accepts_common_aliases()
        Assert.AreEqual("(Sturges)", UdfDataImport.GetHistogramBinRule(Nothing))
        Assert.AreEqual("(Doane)", UdfDataImport.GetHistogramBinRule("DOANE"))
        Assert.AreEqual("(Scott)", UdfDataImport.GetHistogramBinRule(" scott "))
        Assert.AreEqual("(Freedman-Diaconis)", UdfDataImport.GetHistogramBinRule("Freedman Diaconis"))
        Assert.AreEqual("(Freedman-Diaconis)", UdfDataImport.GetHistogramBinRule("freedman-diaconis"))
        Assert.AreEqual("(Freedman-Diaconis)", UdfDataImport.GetHistogramBinRule("FD"))
        Assert.AreEqual("(Sturges)", UdfDataImport.GetHistogramBinRule("unknown"))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Optional_probability_threshold_import_sorts_and_deduplicates_vectors()
        Dim thresholds() As Double = Nothing
        Dim src As Object(,) = UdfTestData.ColUdfs(0.75R, 0.25R, Nothing, 0.5R, 0.2500000000001R)

        Assert.IsTrue(BESHStatNG.WorksheetFunctions.TryGetOptionalThresholdVector(src, thresholds))

        CollectionAssert.AreEqual(New Double() {0.25R, 0.5R, 0.75R}, thresholds)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Optional_probability_threshold_import_rejects_matrix_and_out_of_range_values()
        Dim thresholds() As Double = Nothing

        Assert.IsFalse(BESHStatNG.WorksheetFunctions.TryGetOptionalThresholdVector(UdfTestData.MatrixForUdfs(
            New Object()() {
                New Object() {0.25R, 0.5R},
                New Object() {0.75R, 0.9R}
            }), thresholds))

        Assert.IsFalse(BESHStatNG.WorksheetFunctions.TryGetOptionalThresholdVector(UdfTestData.ColUdfs(0.25R, 1.2R), thresholds))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Udf_linear_algebra_inverts_square_matrix_and_rejects_singular_matrix()
        Dim a(,) As Double = {
            {4.0R, 7.0R},
            {2.0R, 6.0R}
        }

        Dim inv As Double(,) = Nothing
        Assert.IsTrue(UdfLinearAlgebra.TryInvertMatrix(a, inv))
        Assert.IsNotNull(inv)
        Assert.AreEqual(0.6R, inv(0, 0), 0.000000000001R)
        Assert.AreEqual(-0.7R, inv(0, 1), 0.000000000001R)
        Assert.AreEqual(-0.2R, inv(1, 0), 0.000000000001R)
        Assert.AreEqual(0.4R, inv(1, 1), 0.000000000001R)

        Dim singular(,) As Double = {
            {1.0R, 2.0R},
            {2.0R, 4.0R}
        }
        Assert.IsFalse(UdfLinearAlgebra.TryInvertMatrix(singular, inv))
        Assert.IsNull(inv)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Udf_output_tables_prepare_tables_and_strip_headers()
        Dim raw(,) As Object = {
            {"Name", "Value"},
            {"A", Nothing},
            {"B", DBNull.Value}
        }

        Dim prepared As Object(,) = UdfOutputTables.PrepareResultTableForUdf(raw)
        Assert.AreEqual("Name", Convert.ToString(prepared(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(String.Empty, Convert.ToString(prepared(1, 1), CultureInfo.InvariantCulture))
        Assert.AreEqual(String.Empty, Convert.ToString(prepared(2, 1), CultureInfo.InvariantCulture))

        Dim withoutHeader As Object(,) = CType(UdfOutputTables.PrepareExistingObjectTableForUdf(raw, includeHeader:=False), Object(,))
        Assert.AreEqual(2, withoutHeader.GetLength(0))
        Assert.AreEqual("A", Convert.ToString(withoutHeader(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual("B", Convert.ToString(withoutHeader(1, 0), CultureInfo.InvariantCulture))
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Udf_regression_output_builds_residual_vectors_and_linear_predictors()
        Dim residuals() As Double = {1.25R, -0.5R}
        Dim out As Object(,) = CType(UdfRegressionOutput.BuildResidualVectorOutput(residuals, "Residual", includeHeader:=True), Object(,))

        Assert.AreEqual("Residual", Convert.ToString(out(0, 0), CultureInfo.InvariantCulture))
        Assert.AreEqual(1.25R, Convert.ToDouble(out(1, 0), CultureInfo.InvariantCulture), 0.0R)
        Assert.AreEqual(-0.5R, Convert.ToDouble(out(2, 0), CultureInfo.InvariantCulture), 0.0R)

        Dim expandedX(,) As Double = {{2.0R, 3.0R}}
        Dim beta() As Double = {1.0R, 0.5R, -0.25R}
        Dim offsetVals() As Double = {0.1R}
        Dim eta As Double = UdfRegressionOutput.ComputeLinearPredictor(expandedX, 0, beta, includeIntercept:=True, offsetVals:=offsetVals)
        Assert.AreEqual(1.35R, eta, 0.000000000001R)

        UdfAssert.IsExcelError(UdfRegressionOutput.SafeExcelNumber(Double.PositiveInfinity), "ExcelErrorNum")
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Udf_data_import_options_parse_alpha_and_equivalence_margins()
        Dim alpha As Double = 0.0R
        Assert.IsTrue(UdfDataImport.TryParseAlpha(Nothing, alpha))
        Assert.AreEqual(0.05R, alpha, 0.0R)

        Assert.IsTrue(UdfDataImport.TryParseAlpha("0.10", alpha))
        Assert.AreEqual(0.1R, alpha, 0.000000000001R)
        Assert.IsFalse(UdfDataImport.TryParseAlpha(1.0R, alpha))

        Dim lower As Double = 0.0R
        Dim upper As Double = 0.0R
        Assert.IsTrue(UdfDataImport.TryGetEquivalenceMargins(0.2R, Nothing, lower, upper))
        Assert.AreEqual(-0.2R, lower, 0.000000000001R)
        Assert.AreEqual(0.2R, upper, 0.000000000001R)

        Assert.IsTrue(UdfDataImport.TryGetEquivalenceMargins(-0.3R, 0.4R, lower, upper))
        Assert.AreEqual(-0.3R, lower, 0.000000000001R)
        Assert.AreEqual(0.4R, upper, 0.000000000001R)
    End Sub

    <TestMethod>
    <TestCategory("UDF")>
    Public Sub Udf_data_import_agreement_options_parse_aliases_and_reject_invalid_values()
        Assert.AreEqual(Global.BESHStatNG.Agreement.AgreementCiMethod.Jackknife,
                        UdfDataImport.ParseAgreementCiMethod("jack", Global.BESHStatNG.Agreement.AgreementCiMethod.Analytical))
        Assert.AreEqual(Global.BESHStatNG.Agreement.AgreementCiMethod.BootstrapBCa,
                        UdfDataImport.ParseAgreementCiMethod("bootstrap_bca", Global.BESHStatNG.Agreement.AgreementCiMethod.Analytical))

        Assert.AreEqual(Global.BESHStatNG.Agreement.RepeatedBlandAltmanMode.RepeatedBySubject,
                        UdfDataImport.ParseBlandAltmanMode("subject", Global.BESHStatNG.Agreement.RepeatedBlandAltmanMode.Auto))
        Assert.AreEqual(Global.BESHStatNG.Agreement.BlandAltmanScale.PercentOfReference,
                        UdfDataImport.ParseBlandAltmanScale("pct-ref", Global.BESHStatNG.Agreement.BlandAltmanScale.RawDifference))
        Assert.AreEqual(Global.BESHStatNG.Agreement.BlandAltmanXAxisMode.TestMethod,
                        UdfDataImport.ParseBlandAltmanXAxisMode("Y", Global.BESHStatNG.Agreement.BlandAltmanXAxisMode.MeanOfMethods))
        Assert.AreEqual(Global.BESHStatNG.Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans,
                        UdfDataImport.ParseBlandAltmanPlotMode("all and means", Global.BESHStatNG.Agreement.RepeatedBlandAltmanPlotMode.AllObservations))

        Assert.AreEqual(Global.BESHStatNG.Agreement.KappaWeightingScheme.CicchettiAllison,
                        UdfDataImport.ParseKappaWeighting("CA", Global.BESHStatNG.Agreement.KappaWeightingScheme.Quadratic))
        Assert.AreEqual(Global.BESHStatNG.Agreement.DemingVarianceModel.ConstantCV,
                        UdfDataImport.ParseDemingVarianceModel("constant cv", Global.BESHStatNG.Agreement.DemingVarianceModel.ConstantLambda))

        Assert.AreEqual("ICC3K", UdfDataImport.ParseIccModel("3k", "ICC21"))
        Assert.IsTrue(UdfDataImport.IsOneWayIcc("ICC11"))
        Assert.IsFalse(UdfDataImport.IsOneWayIcc("ICC21"))
        Assert.AreEqual("% of reference method",
                        UdfDataImport.DescribeBlandAltmanScale(Global.BESHStatNG.Agreement.BlandAltmanScale.PercentOfReference))

        Assert.AreEqual(Integer.MinValue, UdfDataImport.ParseOptionalSeed(Nothing))
        Assert.AreEqual(123, UdfDataImport.ParseOptionalSeed(123.9R))
        Assert.IsTrue(Double.IsNaN(UdfDataImport.ParseOptionalNullableDouble(Nothing)))
        Assert.AreEqual(0.25R, UdfDataImport.ParseOptionalNullableDouble(0.25R), 0.0R)

        Dim threw As Boolean = False
        Try
            UdfDataImport.ParseBlandAltmanScale("not-a-scale", Global.BESHStatNG.Agreement.BlandAltmanScale.RawDifference)
        Catch ex As ArgumentException
            threw = True
        End Try
        Assert.IsTrue(threw, "Invalid Bland-Altman scale should raise ArgumentException.")
    End Sub

End Class