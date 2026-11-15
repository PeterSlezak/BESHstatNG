Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.Agreement
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.SampleSizeCalc
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfAgreementSizing41Tests
    Private Shared Function First() As Double()
        Return New Double() {1, 2, 3, 4, 6, 7, 9, 11, 13, 16, 17, 20}
    End Function
    Private Shared Function Second() As Double()
        Return New Double() {1.2, 2.3, 3.1, 4.7, 6.3, 7.6, 9.1, 11.5, 13.9, 16.3, 17.5, 20.9}
    End Function
    Private Shared Function Number(value As Object) As Double?
        If TypeOf value Is Double Then Return CDbl(value)
        Return Nothing
    End Function
    Private Shared Function OpenUnit(value As Double) As Boolean
        Return value > 0 AndAlso value < 1
    End Function
    Private Shared Sub EqualCI(a As ConfidenceIntervalResult, b As ConfidenceIntervalResult)
        Assert.AreEqual(a.Estimate, b.Estimate)
        Assert.AreEqual(a.LowerLimit, b.LowerLimit)
        Assert.AreEqual(a.UpperLimit, b.UpperLimit)
        Assert.AreEqual(a.CIlabel, b.CIlabel)
    End Sub

    <DataTestMethod>
    <DataRow(False)>
    <DataRow(True)>
    Public Sub ActualDemingFitRetainsDistinctUdfConfigurationPaths(configureFields As Boolean)
        Dim options As New DemingOptions With {.Alpha = 0.1, .Lambda = 2, .CiMethod = AgreementCiMethod.Jackknife}
        Dim expected As New WeightedDemingRegression(First(), Second(), "Reference", "Test", options)
        If configureFields Then
            expected.alpha = options.Alpha
            If options.VarianceModel = DemingVarianceModel.ConstantLambda Then expected.Lambda = options.Lambda
        End If
        expected.Fit(Nothing, 173)
        Dim actual = UdfAgreementReportPreparation38.FitDeming(First(), Second(), "Reference", "Test", options, 173, configureFields)
        EqualCI(expected.SlopeCI, actual.SlopeCI)
        EqualCI(expected.InterceptCI, actual.InterceptCI)
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub ActualBlandAndLinResultsKeepSeedAndIntervals(culture As String)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim options As New BlandAltmanOptions With {.Alpha = 0.1}
            Dim expected As New BlandAltmanAgreement(First(), Second(), "A", "B", options)
            Dim a = expected.Fit(173), b As BlandAltmanResult = Nothing, reads As Integer = 0
            Dim actual = UdfAgreementReportPreparation38.FitBlandAltman(First(), Second(), "A", "B", options,
                Function()
                    reads += 1
                    Return 173
                End Function, b)
            Assert.AreEqual(1, reads)
            EqualCI(a.BiasCI, b.BiasCI) : EqualCI(a.LowerLoACI, b.LowerLoACI) : EqualCI(a.UpperLoACI, b.UpperLoACI)
            Dim linOptions As New LinConcordanceOptions With {.Alpha = 0.1, .CiMethod = AgreementCiMethod.Analytical}
            Dim lin As New LinConcordanceCorrelation(First(), Second(), "A", "B", linOptions)
            Dim linExpected = lin.Fit(Nothing, 173), linActual As LinConcordanceResult = Nothing
            UdfAgreementReportPreparation38.FitLinConcordance(First(), Second(), "A", "B", linOptions, Function() 173, linActual)
            EqualCI(linExpected.ConcordanceCI, linActual.ConcordanceCI)
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub KappaActualEngineRetainsCategoricalOrderAndWeighting()
        Dim x() As Object = {"a", "a", "b", "b", "c", "c", "c", "a"}
        Dim y() As Object = {"a", "b", "b", "b", "c", "a", "c", "a"}
        Dim options As New KappaOptions With {.Alpha = 0.1, .Weighting = KappaWeightingScheme.Quadratic}
        Dim expected As New WeightedKappaAgreement(x, y, "Rater 1", "Rater 2", options)
        Dim a = expected.Fit(Nothing, 173), b As KappaResult = Nothing
        UdfAgreementReportPreparation38.FitKappa(x, y, "Rater 1", "Rater 2", options, Function() 173, b)
        EqualCI(a.KappaCI, b.KappaCI)
        CollectionAssert.AreEqual(New Object() {"a", "a", "b", "b", "c", "c", "c", "a"}, x)
    End Sub

    <TestMethod>
    Public Sub ConstructorFailurePrecedesSeedReader()
        Dim read As Boolean = False, failure As Exception = Nothing, result As LinConcordanceResult = Nothing
        Try
            UdfAgreementReportPreparation38.FitLinConcordance(Nothing, Second(), "A", "B", New LinConcordanceOptions(),
                Function()
                    read = True
                    Throw New InvalidOperationException("seed reader must not execute")
                End Function, result)
        Catch ex As Exception
            failure = ex
        End Try
        Assert.IsNotNull(failure)
        Assert.IsFalse(read)
        Assert.IsNull(result)
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub PairedSampleSizeCallsRealEngineWithSameRounding(culture As String)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim expected = SampleSizeCalculator.CalculatePairedTTest(1, 2, 0.05, 0.2)
            Dim errorKind As UdfSampleSizeError38 = UdfSampleSizeError38.Numeric
            Dim report = UdfSampleSizePreparation38.PreparePaired(1.0, 2.0, 0.05, 0.2, AddressOf Number, AddressOf OpenUnit, errorKind)
            Assert.IsNotNull(report)
            Assert.AreEqual(expected.NumberOfPairs, CInt(report.ToOutputModel().Values(1, 1)))
            Assert.AreEqual("Required pairs", CStr(report.ToOutputModel().Values(1, 0)))
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub PairedSizingReadsAllFourArgumentsBeforeMissingAndNumericRejection()
        Dim seen As New List(Of Object), intervalCalls As Integer = 0
        Dim reader As Func(Of Object, Double?) = Function(value)
                                                   seen.Add(value)
                                                   Return Number(value)
                                               End Function
        Dim interval As Func(Of Double, Boolean) = Function(value)
                                                       intervalCalls += 1
                                                       Return OpenUnit(value)
                                                   End Function
        Dim errorKind As UdfSampleSizeError38
        Assert.IsNull(UdfSampleSizePreparation38.PreparePaired("missing", 2.0, 0.05, 0.2, reader, interval, errorKind))
        Assert.AreEqual(UdfSampleSizeError38.Value, errorKind)
        Assert.AreEqual(4, seen.Count) : Assert.AreEqual(0, intervalCalls)
        seen.Clear()
        Assert.IsNull(UdfSampleSizePreparation38.PreparePaired(0.0, 2.0, 0.05, 0.2, reader, interval, errorKind))
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind)
        Assert.AreEqual(4, seen.Count) : Assert.AreEqual(0, intervalCalls)
    End Sub

    <TestMethod>
    Public Sub RestrictedIccInfinityTimingRemainsUncorrected()
        Dim errorKind As UdfSampleSizeError38
        Assert.ThrowsException(Of OverflowException)(Sub() UdfSampleSizePreparation38.PrepareIcc(0.2, 0.6, Double.PositiveInfinity, 0.05, 0.2,
                                                                                                AddressOf Number, AddressOf OpenUnit, errorKind))
        Assert.IsNull(UdfSampleSizePreparation38.PrepareIcc(0.2, 0.6, Double.PositiveInfinity, 0.0, 0.2,
                                                        AddressOf Number, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind, "Bad alpha still rejects before the overflowing count conversion.")
    End Sub
End Class
