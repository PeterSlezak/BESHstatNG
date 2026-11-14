Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation9Tests
    Private Shared Function Interval(value As Double) As ConfidenceIntervalResult
        Return New ConfidenceIntervalResult With {.Estimate = value, .LowerLimit = value - 1, .UpperLimit = value + 1, .StdErr = 0.25}
    End Function

    Private Shared Function Finite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    <TestMethod>
    Public Sub CoefficientsRetainLabelsTypesAndConfidenceMetadata()
        Dim slope = Interval(2), intercept = Interval(3)
        Dim model = UdfAgreementReportPreparation38.BuildRegressionCoefficients(slope, intercept, "Coefficients").ToOutputModel()
        Dim r = model.TitleRows + model.HeaderTopRows, c = model.HeaderLeftColumns
        Assert.AreEqual(2.0R, CDbl(model.Values(r, c)))
        Assert.AreEqual(3.0R, CDbl(model.Values(r + 1, c)))
        Assert.AreEqual("Slope", CStr(model.Values(r, 0)))
        Assert.AreEqual("Intercept", CStr(model.Values(r + 1, 0)))
        Assert.IsInstanceOfType(model.Values(r, c + 2), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub CoefficientsPreserveAliasedCachedIntervalInstances()
        Dim ci = Interval(2)
        ci.strConfidenceInterval = "retained cached text"
        Dim m = UdfAgreementReportPreparation38.BuildRegressionCoefficients(ci, ci, "Alias").ToOutputModel()
        Dim r = m.TitleRows + m.HeaderTopRows, c = m.HeaderLeftColumns
        Assert.AreEqual("retained cached text", CStr(m.Values(r, c + 1)))
        Assert.AreEqual("retained cached text", CStr(m.Values(r + 1, c + 1)))
        Assert.AreEqual("retained cached text", ci.strConfidenceInterval)
    End Sub

    <TestMethod>
    Public Sub MissingSlopeFailsBeforeInterceptFormatting()
        Dim intercept = Interval(4)
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfAgreementReportPreparation38.BuildRegressionCoefficients(Nothing, intercept, "Failure"))
        intercept.Estimate = 7
        Assert.IsTrue(intercept.strConfidenceInterval.StartsWith("7", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub BlandAltmanStatisticsKeepBlankSpacerCellsAndRowOrder()
        Dim res As New Agreement.BlandAltmanResult With {.BiasCI = Interval(1), .LowerLoACI = Interval(2), .UpperLoACI = Interval(3), .SdDifference = 4, .RepeatabilityCoefficient = 5}
        Dim m = UdfAgreementReportPreparation38.BuildBlandAltmanStatistics(res).ToOutputModel()
        Dim r = m.TitleRows + m.HeaderTopRows, c = m.HeaderLeftColumns
        Assert.AreEqual(1.0R, CDbl(m.Values(r, c)))
        Assert.AreEqual(5.0R, CDbl(m.Values(r + 4, c)))
        Assert.AreEqual("", CStr(m.Values(r + 3, c + 1)))
        Assert.AreEqual("Repeatability coefficient", CStr(m.Values(r + 4, 0)))
    End Sub

    <TestMethod>
    Public Sub RepeatabilityKeepsSemAndOriginalHeaderBoundary()
        Dim ci = Interval(4)
        Dim m = UdfAgreementReportPreparation38.BuildRepeatability(ci, "ICC(2,1)").ToOutputModel()
        Dim r = m.TitleRows + m.HeaderTopRows
        Assert.AreEqual("ICC(2,1)", CStr(m.Values(0, 0)))
        Assert.AreEqual(4.0R, CDbl(m.Values(r, 1)))
        Assert.AreEqual(0.25R, CDbl(m.Values(r + 2, 1)))
    End Sub

    <TestMethod>
    Public Sub ConfidenceFormattingUsesCurrentCultureWithoutEagerCopies()
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sk-SK")
            Dim ci = Interval(1.25)
            Dim m = UdfAgreementReportPreparation38.BuildRegressionCoefficients(ci, Interval(2), "Culture").ToOutputModel()
            Dim r = m.TitleRows + m.HeaderTopRows, c = m.HeaderLeftColumns
            Assert.AreEqual(ci.strConfidenceInterval(CIformat.LL_to_UL), CStr(m.Values(r, c + 1)))
            Assert.IsTrue(CStr(m.Values(r, c + 1)).Contains(","))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub ConfusionRowsRetainThresholdTiesAndNumericClassOrder()
        Dim summary = BinaryClassificationCore.ComputeBinarySummary({0.0R, 1.0R, 1.0R, 0.0R}, {0.5R, 0.5R, 0.8R, 0.2R}, 0.5R)
        Dim output = DirectCast(UdfClassificationReportPreparation38.BuildBinaryCrosstabUdfOutput(summary, New Object(), True), Object(,))
        Assert.AreEqual(5, output.GetLength(0))
        Assert.AreEqual(4, output.GetLength(1))
        Assert.AreEqual(0, CInt(output(0, 1)))
        Assert.AreEqual(1, CInt(output(0, 2)))
        Assert.AreEqual(summary.FP, CDbl(output(1, 2)))
        Assert.AreEqual(summary.TP, CDbl(output(2, 2)))
        Assert.AreEqual(0.5R, CDbl(output(4, 1)))
    End Sub

    <TestMethod>
    Public Sub MissingPercentRetainsOpaqueSentinelAndOverflowBehavior()
        Dim sentinel As New Object()
        Dim summary As New BinaryClassificationSummary With {.Specificity = Double.NaN, .Sensitivity = Double.PositiveInfinity, .Accuracy = Double.MaxValue}
        Dim output = DirectCast(UdfClassificationReportPreparation38.BuildBinaryCrosstabUdfOutput(summary, sentinel, False), Object(,))
        Assert.AreSame(sentinel, output(0, 3))
        Assert.AreSame(sentinel, output(1, 3))
        Assert.IsTrue(Double.IsPositiveInfinity(CDbl(output(2, 3))))
    End Sub

    <TestMethod>
    Public Sub EmptyThresholdAndCalibrationOutputsKeepTheSameSentinel()
        Dim sentinel As New Object()
        Assert.AreSame(sentinel, UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(Nothing, sentinel))
        Assert.AreSame(sentinel, UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(New List(Of BinaryThresholdRow), sentinel, False))
        Assert.AreSame(sentinel, UdfClassificationReportPreparation38.BuildCalibrationTableUdfOutput(Nothing, sentinel))
        Assert.AreSame(sentinel, UdfClassificationReportPreparation38.BuildCalibrationTableUdfOutput(New List(Of CalibrationBinSummary), sentinel, False))
    End Sub

    <TestMethod>
    Public Sub ThresholdReportsKeepCoreOrderingAndFourteenColumns()
        Dim rows = BinaryClassificationCore.BuildThresholdTable({0.0R, 1.0R, 1.0R}, {0.2R, 0.5R, 0.5R}, {0.5R, 0.2R, 0.5R})
        Dim output = DirectCast(UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(rows, New Object(), False), Object(,))
        Assert.AreEqual(rows.Count, output.GetLength(0))
        Assert.AreEqual(14, output.GetLength(1))
        For i = 0 To rows.Count - 1
            Assert.AreEqual(rows(i).Threshold, CDbl(output(i, 0)))
            Assert.AreEqual(rows(i).TP, CDbl(output(i, 1)))
        Next
    End Sub

    <TestMethod>
    Public Sub CalibrationKeepsRawNanLimitsAndWeightedCounts()
        Dim rows As New List(Of CalibrationBinSummary) From {New CalibrationBinSummary With {.BinIndex = 3, .N = 1.5, .MeanPredicted = 0.4, .ObservedRate = 0.5, .LowerCI = Double.NaN, .UpperCI = Double.PositiveInfinity}}
        Dim output = DirectCast(UdfClassificationReportPreparation38.BuildCalibrationTableUdfOutput(rows, New Object(), False), Object(,))
        Assert.AreEqual(3, CInt(output(0, 0)))
        Assert.AreEqual(1.5R, CDbl(output(0, 1)))
        Assert.IsTrue(Double.IsNaN(CDbl(output(0, 4))))
        Assert.IsTrue(Double.IsPositiveInfinity(CDbl(output(0, 5))))
    End Sub

    <TestMethod>
    Public Sub BrierRowsOmitOnlyNanOptionalScalars()
        Dim output = DirectCast(UdfClassificationReportPreparation38.BuildBrierScoreUdfOutput(0.25, New Object(), Double.NaN, Double.NaN, False), Object(,))
        Assert.AreEqual(1, output.GetLength(0))
        Assert.AreEqual("BrierScore", CStr(output(0, 0)))
        output = DirectCast(UdfClassificationReportPreparation38.BuildBrierScoreUdfOutput(0.25, New Object(), Double.PositiveInfinity, -1, True), Object(,))
        Assert.AreEqual(4, output.GetLength(0))
        Assert.IsTrue(Double.IsPositiveInfinity(CDbl(output(2, 1))))
        Assert.AreEqual(-1.0R, CDbl(output(3, 1)))
    End Sub

    <TestMethod>
    Public Sub ReportsReturnFreshArraysWithoutChangingSourceRows()
        Dim rows As New List(Of BinaryThresholdRow) From {New BinaryThresholdRow With {.Threshold = 0.4}}
        Dim first = DirectCast(UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(rows, New Object()), Object(,))
        first(1, 0) = "changed"
        Dim second = DirectCast(UdfClassificationReportPreparation38.BuildThresholdTableUdfOutput(rows, New Object()), Object(,))
        Assert.AreEqual(0.4R, CDbl(second(1, 0)))
        Assert.AreEqual(0.4R, rows(0).Threshold)
    End Sub

    <TestMethod>
    Public Sub ChiSquaredBodyRetainsCoreStatisticDfAndRawPValue()
        Dim tab As Integer(,) = {{8, 2}, {3, 7}}
        Dim expected = contingencytable.Chi2TESTindependence(tab)
        Dim result = UdfContingencyPreparation38.ChiSquaredBody(tab, AddressOf Finite)
        Assert.AreEqual(expected.Item1.TestStatistics1, CDbl(result(0, 1)))
        Assert.AreEqual(WorksheetCompatibility.UdfNumericPreparation38.EffectiveDf(tab), CInt(result(1, 1)))
        Assert.AreEqual(expected.Item1.Pvalue, CDbl(result(2, 1)))
        Assert.IsInstanceOfType(result(2, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub ContingencyFiniteGuardIsLazyAndExceptionsPropagate()
        Dim seen As New List(Of Double)
        Dim guard As Func(Of Double, Boolean) = Function(value)
                                                       seen.Add(value)
                                                       Return False
                                                   End Function
        Assert.IsNull(UdfContingencyPreparation38.ChiSquaredBody(New Integer(,) {{8, 2}, {3, 7}}, guard))
        Assert.AreEqual(1, seen.Count)
        Dim throwing As Func(Of Double, Boolean) = Function(value)
                                                          Throw New InvalidOperationException("direct guard failure")
                                                      End Function
        Assert.ThrowsException(Of InvalidOperationException)(Sub() UdfContingencyPreparation38.ChiSquaredBody(New Integer(,) {{8, 2}, {3, 7}}, throwing))
    End Sub

    <TestMethod>
    Public Sub NominalAssociationKeepsEveryCoreCoefficient()
        Dim tab As Integer(,) = {{8, 2, 1}, {3, 7, 2}}
        Dim expected = contingencytable.Chi2TESTindependence(tab)
        Dim result = UdfContingencyPreparation38.NominalAssociationBody(tab)
        Assert.AreEqual(expected.Item2, CDbl(result(0, 1)))
        Assert.AreEqual(expected.Item3, CDbl(result(1, 1)))
        Assert.AreEqual(expected.Item4, CDbl(result(2, 1)))
    End Sub

    <TestMethod>
    Public Sub FisherBodyKeepsOriginalOrientationAndMidPFields()
        Dim tab As Integer(,) = {{2, 7}, {8, 3}}
        Dim expected = contingencytable.FisherExact2x2(2, 7, 8, 3)
        Dim result = UdfContingencyPreparation38.Fisher2x2Body(tab, AddressOf Finite)
        Assert.AreEqual(expected.PvalueLowerSide, CDbl(result(0, 1)))
        Assert.AreEqual(expected.Pvalue, CDbl(result(1, 1)))
        Assert.AreEqual(expected.pValueExactLowerSide, CDbl(result(2, 1)))
        Assert.AreEqual(expected.Pvalue2, CDbl(result(3, 1)))
    End Sub

    <TestMethod>
    Public Sub FreemanHaltonUsesTheExistingCoreEngine()
        Dim tab As Integer(,) = {{2, 1, 3}, {1, 4, 2}}
        Dim expected As New contingencytable.FisherExactEngine(tab)
        expected.Run()
        Dim result = UdfContingencyPreparation38.FreemanHaltonBody(tab, AddressOf Finite)
        Assert.AreEqual(expected.PObserved, CDbl(result(0, 1)))
        Assert.AreEqual(expected.PValue, CDbl(result(1, 1)))
    End Sub
End Class
