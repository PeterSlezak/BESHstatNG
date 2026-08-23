Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcCapabilityTests

    Private Shared Function CapabilityValues() As Double()
        Dim pattern As Double() = {9.0, 9.5, 10.0, 10.5, 11.0, 10.5, 10.0, 9.5}
        Dim values(63) As Double
        For i As Integer = 0 To values.Length - 1
            values(i) = pattern(i Mod pattern.Length)
        Next
        Return values
    End Function

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub NormalCapability_IndicesReconcileWithReturnedMeanAndSigma()
        Dim specs As New SpcSpecificationLimits(7.0, 10.0, 13.0)
        Dim request As New SpcContinuousCapabilityRequest(
            CapabilityValues(), specs,
            method:=SpcContinuousCapabilityMethod.Normal,
            withinSigmaEstimator:=SpcWithinSigmaEstimator.MovingRange)
        Dim result As SpcContinuousCapabilityResult = SpcCapability.Analyze(request)

        Assert.AreEqual(64, result.SampleCount)
        Assert.IsTrue(result.WithinSigma.HasValue)
        Assert.IsTrue(result.WithinSigmaDegreesOfFreedom.HasValue)
        Assert.IsTrue(result.OverallSigmaDegreesOfFreedom.HasValue)
        Dim cp As SpcCapabilityIndexResult = result.GetIndex("Cp")
        Dim cpk As SpcCapabilityIndexResult = result.GetIndex("Cpk")
        Dim pp As SpcCapabilityIndexResult = result.GetIndex("Pp")
        Dim ppk As SpcCapabilityIndexResult = result.GetIndex("Ppk")
        Dim cpm As SpcCapabilityIndexResult = result.GetIndex("Cpm")
        Assert.IsNotNull(cp)
        Assert.IsNotNull(cpk)
        Assert.IsNotNull(pp)
        Assert.IsNotNull(ppk)
        Assert.IsNotNull(cpm)

        AssertClose(6.0 / (6.0 * result.WithinSigma.Value), cp.Estimate)
        Dim expectedCpk As Double = Math.Min(
            (result.ProcessMean - 7.0) / (3.0 * result.WithinSigma.Value),
            (13.0 - result.ProcessMean) / (3.0 * result.WithinSigma.Value))
        AssertClose(expectedCpk, cpk.Estimate)
        AssertClose(6.0 / (6.0 * result.OverallSigma), pp.Estimate)
        Assert.IsTrue(cp.HasConfidenceInterval)
        Assert.IsTrue(cpk.HasConfidenceInterval)
        Assert.IsTrue(pp.HasConfidenceInterval)
        Assert.IsTrue(ppk.HasConfidenceInterval)
        Assert.AreEqual(3, result.Performance.Length)
        Assert.AreEqual(SpcCapabilityPerformanceBasis.Observed, result.Performance(0).Basis)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub SubgroupedNormalCapability_AutomaticEstimatorUsesPooledWithinSigma()
        Dim values As Double() = CapabilityValues()
        Dim ids(values.Length - 1) As String
        For i As Integer = 0 To ids.Length - 1
            ids(i) = "G" & (i \ 4 + 1).ToString("00")
        Next
        Dim request As New SpcContinuousCapabilityRequest(
            values,
            New SpcSpecificationLimits(7.0, 10.0, 13.0),
            subgroupIds:=ids)
        Dim result As SpcContinuousCapabilityResult = SpcCapability.AnalyzeContinuous(request)
        Assert.IsTrue(result.WithinSigma.HasValue)
        Assert.IsTrue(result.WithinSigmaMethod.IndexOf(
            "Pooled", StringComparison.OrdinalIgnoreCase) >= 0)
        Assert.IsTrue(result.GetIndex("Cp").Estimate > 0.0)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub HistoricalSigmaAndDegreesOfFreedom_ControlIntervalsAndReportedMethods()
        Dim request As New SpcContinuousCapabilityRequest(
            CapabilityValues(),
            New SpcSpecificationLimits(7.0, 10.0, 13.0),
            processMean:=10.0,
            withinProcessSigma:=0.75,
            withinSigmaDegreesOfFreedom:=40.0,
            overallProcessSigma:=0.9,
            overallSigmaDegreesOfFreedom:=50.0)
        Dim result As SpcContinuousCapabilityResult = SpcCapability.Analyze(request)
        AssertClose(10.0, result.ProcessMean)
        AssertClose(0.75, result.WithinSigma.Value)
        AssertClose(0.9, result.OverallSigma)
        AssertClose(40.0, result.WithinSigmaDegreesOfFreedom.Value)
        AssertClose(50.0, result.OverallSigmaDegreesOfFreedom.Value)
        AssertClose(6.0 / (6.0 * 0.75), result.GetIndex("Cp").Estimate)
        Assert.IsTrue(result.GetIndex("Cp").HasConfidenceInterval)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub NonnormalPercentileCapability_ReturnsEmpiricalIndicesAndWarnings()
        Dim request As New SpcContinuousCapabilityRequest(
            CapabilityValues(),
            New SpcSpecificationLimits(7.0, 10.0, 13.0),
            method:=SpcContinuousCapabilityMethod.NonnormalPercentile,
            lowerPercentileProbability:=0.05,
            upperPercentileProbability:=0.95)
        Dim result As SpcContinuousCapabilityResult = SpcCapability.Analyze(request)
        Assert.IsTrue(result.LowerPercentile.HasValue)
        Assert.IsTrue(result.UpperPercentile.HasValue)
        Assert.IsTrue(result.LowerPercentile.Value < result.UpperPercentile.Value)
        Assert.IsNotNull(result.GetIndex("Cnp"))
        Assert.IsNotNull(result.GetIndex("Cnpl"))
        Assert.IsNotNull(result.GetIndex("Cnpu"))
        Assert.IsNotNull(result.GetIndex("Cnpk"))
        Assert.IsTrue(result.Warnings.Any(
            Function(w) w.IndexOf("bootstrap", StringComparison.OrdinalIgnoreCase) >= 0))
        Assert.AreEqual(1, result.Performance.Length)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub MissingValues_AreOmittedOrRejectedAccordingToRequest()
        Dim values As Double() = CapabilityValues()
        values(5) = Double.NaN
        Dim omitted As SpcContinuousCapabilityResult = SpcCapability.Analyze(
            New SpcContinuousCapabilityRequest(
                values, New SpcSpecificationLimits(7.0, upperSpecificationLimit:=13.0),
                omitMissing:=True))
        Assert.AreEqual(values.Length - 1, omitted.SampleCount)
        Assert.IsTrue(omitted.Warnings.Any(
            Function(w) w.IndexOf("omitted", StringComparison.OrdinalIgnoreCase) >= 0))
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcContinuousCapabilityRequest(
                    values,
                    New SpcSpecificationLimits(7.0, upperSpecificationLimit:=13.0),
                    omitMissing:=False)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub BinomialCapability_ReproducesAggregatedRateYieldAndExactInterval()
        Dim request As New SpcBinomialCapabilityRequest(
            {1, 2, 0, 3}, {100, 120, 80, 100}, requestLabel:="Binomial")
        Dim result As SpcAttributeCapabilityResult = SpcCapability.Analyze(request)
        Assert.AreEqual(SpcAttributeCapabilityModel.Binomial, result.Model)
        Assert.AreEqual(4, result.RowCount)
        Assert.AreEqual(6L, result.TotalEvents)
        AssertClose(400.0, result.TotalOpportunity)
        AssertClose(0.015, result.Rate)
        AssertClose(0.985, result.YieldProbability)
        Assert.IsTrue(result.LowerConfidenceLimit <= result.Rate)
        Assert.IsTrue(result.UpperConfidenceLimit >= result.Rate)
        AssertClose(15000.0, result.PartsPerMillion)
        Assert.IsTrue(result.ZBench.HasValue)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub BinomialCapability_HandlesBoundaryEventCounts()
        Dim zero As SpcAttributeCapabilityResult = SpcCapability.AnalyzeBinomial(
            New SpcBinomialCapabilityRequest({0, 0}, {50, 50}))
        AssertClose(0.0, zero.Rate)
        AssertClose(0.0, zero.LowerConfidenceLimit)
        AssertClose(1.0, zero.YieldProbability)
        Assert.IsFalse(zero.ZBench.HasValue)

        Dim all As SpcAttributeCapabilityResult = SpcCapability.AnalyzeBinomial(
            New SpcBinomialCapabilityRequest({50, 50}, {50, 50}))
        AssertClose(1.0, all.Rate)
        AssertClose(1.0, all.UpperConfidenceLimit)
        AssertClose(0.0, all.YieldProbability)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub PoissonCapability_ReproducesRateYieldAndExactInterval()
        Dim request As New SpcPoissonCapabilityRequest(
            {2, 0, 3, 1}, {1.0, 2.0, 1.5, 0.5}, requestLabel:="Poisson")
        Dim result As SpcAttributeCapabilityResult = SpcCapability.Analyze(request)
        Assert.AreEqual(SpcAttributeCapabilityModel.Poisson, result.Model)
        Assert.AreEqual(6L, result.TotalEvents)
        AssertClose(5.0, result.TotalOpportunity)
        AssertClose(1.2, result.Rate)
        AssertClose(Math.Exp(-1.2), result.YieldProbability)
        Assert.IsTrue(result.LowerConfidenceLimit <= result.Rate)
        Assert.IsTrue(result.UpperConfidenceLimit >= result.Rate)
        AssertClose(1200000.0, result.PartsPerMillion)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub CapabilityRequests_ValidateSpecificationsCountsExposureAndProbabilities()
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcContinuousCapabilityRequest(
                    {1.0, 2.0}, New SpcSpecificationLimits())
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcContinuousCapabilityRequest(
                    {1.0, 2.0}, New SpcSpecificationLimits(0.0),
                    confidenceLevel:=1.0)
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcBinomialCapabilityRequest({3}, {2})
            End Sub)
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcPoissonCapabilityRequest({1}, {0.0})
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Capability")>
    Public Sub CapabilityCancellation_IsObservedBeforeComputation()
        Dim request As New SpcContinuousCapabilityRequest(
            CapabilityValues(), New SpcSpecificationLimits(7.0, upperSpecificationLimit:=13.0))
        Assert.ThrowsException(Of OperationCanceledException)(
            Sub() SpcCapability.Analyze(request, Function() True))
    End Sub

End Class
