Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.parametric
Imports BESHStatNG.nonparametric
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfInference41Tests
    Private Shared Function Matrix() As Double(,)
        Return New Double(,) {{1, 2, 4}, {2, 4, 5}, {4, 5, 8}, {5, 8, 9}, {7, 8, 12}, {8, 12, 13}}
    End Function

    Private Shared Sub EqualTables(expected As ResultTable, actual As ResultTable)
        Dim a = expected.ToOutputModel(), b = actual.ToOutputModel()
        Assert.AreEqual(a.HeaderTopRows, b.HeaderTopRows)
        Assert.AreEqual(a.HeaderLeftColumns, b.HeaderLeftColumns)
        Assert.AreEqual(a.Values.GetLength(0), b.Values.GetLength(0))
        Assert.AreEqual(a.Values.GetLength(1), b.Values.GetLength(1))
        For i = 0 To a.Values.GetLength(0) - 1
            For j = 0 To a.Values.GetLength(1) - 1
                If TypeOf a.Values(i, j) Is Double AndAlso TypeOf b.Values(i, j) Is Double Then
                    Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(a.Values(i, j))), BitConverter.DoubleToInt64Bits(CDbl(b.Values(i, j))))
                Else
                    Assert.AreEqual(a.Values(i, j), b.Values(i, j))
                End If
            Next
        Next
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub RealPairedAndIndependentEnginesKeepLabelsAndAlpha(culture As String)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim paired As New PairedTtest(Matrix(), {"Before", "Before"})
            paired.compute()
            EqualTables(paired.wrapResults()(0), UdfInferencePreparation38.BuildPairedTTables(Matrix(), {"Before", "Before"})(0))
            Dim groups()() As Double = {New Double() {1, 2, 2, 4, 5}, New Double() {2, 3, 5, 6, 9}}
            Dim independent As New UnpairedTtest(groups, {"Control", "Experimental"})
            independent.compute(0.1)
            Dim actual = UdfInferencePreparation38.BuildIndependentTTables(groups, {"Control", "Experimental"}, 0.1)
            For i = 0 To independent.wrapResults().Count - 1
                EqualTables(independent.wrapResults()(i), actual(i))
            Next
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub RepeatedCorrectionReadOccursAfterComputeAndRejectsUnknownOption()
        Dim read As Boolean = False, failure As Exception = Nothing
        Try
            UdfInferencePreparation38.BuildRepeatedAnovaTables(Nothing, {"A", "B"}, Function()
                                                                                                  read = True
                                                                                                  Return "both"
                                                                                              End Function)
        Catch ex As Exception
            failure = ex
        End Try
        Assert.IsNotNull(failure)
        Assert.IsFalse(read, "Acquired-data engine failure precedes reading the host option.")
        Assert.IsNull(UdfInferencePreparation38.BuildRepeatedAnovaTables(Matrix(), {"A", "B", "C"}, Function() "invalid"))
        Dim model As New OneWayRmANOVA(Matrix(), {"A", "B", "C"})
        model.compute() : model.GreenhouseGeisser() : model.HuyhnFeldt()
        Dim actual = UdfInferencePreparation38.BuildRepeatedAnovaTables(Matrix(), {"A", "B", "C"}, Function() "both")
        EqualTables(model.wrapResults()(0), actual(0))
    End Sub

    <TestMethod>
    Public Sub RankEnginesRetainTiesExactAvailabilityAndTailValues()
        Dim groups()() As Double = {New Double() {1, 2, 2, 4}, New Double() {2, 3, 5, 5}}
        Dim expected = New MannWhitney(groups, "group1", "group2").Compute()
        Dim actual = UdfInferencePreparation38.ComputeMannWhitney(groups(0), groups(1))
        Assert.AreEqual(expected.Pvalue, actual.Pvalue)
        Assert.AreEqual(expected.PvalueExact, actual.PvalueExact)
        Assert.AreEqual(expected.pValueExactLowerSide, actual.pValueExactLowerSide)
        Assert.AreEqual(expected.pValueExactUpperSide, actual.pValueExactUpperSide)
        Assert.AreEqual(expected.bExactAvailable, actual.bExactAvailable)
        Dim paired = Matrix()
        expected = New WilcoxonTest(paired, "x", "y").Compute()
        actual = UdfInferencePreparation38.ComputeWilcoxon(paired)
        Assert.AreEqual(expected.Pvalue, actual.Pvalue)
        Assert.AreEqual(expected.PvalueExact, actual.PvalueExact)
        Assert.AreEqual(expected.bExactAvailable, actual.bExactAvailable)
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub NormalityResultsRemainSameEngineCompatibility(culture As String)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim sample() As Double = {1, 2, 3, 4, 6, 7, 8, 10, 11, 12, 15, 19}
            Dim message As String = ""
            Dim expected = assumptions.ShapiroWilk(CType(sample.Clone(), Double()), message)
            Dim actual = AssumptionsUDFsReports38.PrepareShapiroWilk(CType(sample.Clone(), Double()))
            Assert.AreEqual(UdfReportFault38.None, actual.Fault)
            Dim values = actual.ToOutputModel().Values
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.TestStatistics1), BitConverter.DoubleToInt64Bits(CDbl(values(1, 1))))
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Pvalue), BitConverter.DoubleToInt64Bits(CDbl(values(2, 1))))
            expected = assumptions.DAgostino(CType(sample.Clone(), Double()), message)
            values = AssumptionsUDFsReports38.PrepareDAgostino(CType(sample.Clone(), Double())).ToOutputModel().Values
            Assert.AreEqual(expected.TestStatistics1, CDbl(values(1, 1)))
            Assert.AreEqual(expected.Pvalue, CDbl(values(2, 1)))
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub ComputedTwoValueLayoutReusesPvalueAddressAndPreservesIEEEBits()
        Dim report = UdfSpillPreparation38.TwoValueTable("Exact labels", "Statistic", -0.0R, "P-value", 0.001R)
        Dim model = report.ToOutputModel()
        Assert.AreEqual(UdfReportPreparation38.NormalizeBlanks, report.Preparation)
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(-0.0R), BitConverter.DoubleToInt64Bits(CDbl(model.Values(1, 1))))
        Assert.AreEqual(0.001R, CDbl(model.Values(2, 1)))
        Assert.AreEqual(1, model.PvalueCells.Count)
        Assert.AreEqual(2, model.PvalueCells(0).BodyRow)
        model.Values(2, 1) = 0.8R
        Assert.AreEqual(0.001R, CDbl(report.ToOutputModel().Values(2, 1)), "Returned models are detached copies.")
    End Sub

    <TestMethod>
    Public Sub UndefinedNormalityOutputsRetainNumericFaultAndBartlettShortGroupRejects()
        Assert.AreEqual(UdfReportFault38.InvalidNumber, AssumptionsUDFsReports38.PrepareAndersonDarling(New Double() {4, 4, 4, 4}).Fault)
        Assert.AreEqual(UdfReportFault38.InvalidNumber, AssumptionsUDFsReports38.PrepareBartlett(New Double()() {New Double() {1}, New Double() {2, 3}}).Fault)
        Dim eligibility As New NormalityReportEligibility(5000)
        Assert.IsFalse(eligibility.Shapiro, "The restricted GUI/UDF n=5000 disagreement is not corrected here.")
    End Sub
End Class
