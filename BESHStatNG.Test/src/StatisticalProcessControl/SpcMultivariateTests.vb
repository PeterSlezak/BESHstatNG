Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.StatisticalProcessControl

<TestClass>
Public Class SpcMultivariateTests

    Private Shared Function Identity(size As Integer) As Double(,)
        Dim result(size - 1, size - 1) As Double
        For i As Integer = 0 To size - 1
            result(i, i) = 1.0
        Next
        Return result
    End Function

    Private Shared Function IndividualRequest(chartType As SpcChartType,
                                              Optional pcaComponents As Nullable(Of Integer) = Nothing,
                                              Optional useCorrelation As Boolean = False) As SpcMultivariateRequest
        Dim data As Double(,) = IndividualMultivariateData()
        Dim phases As SpcPhase() = Nothing
        Dim stages As String() = Nothing
        MultivariatePhasesAndStages(data.GetLength(0), 36, phases, stages)
        Return New SpcMultivariateRequest(
            chartType,
            data,
            variableNames:={"Temperature", "Pressure", "Flow"},
            labels:=Labels(data.GetLength(0), "M"),
            phases:=phases,
            stageIds:=stages,
            sequenceValues:=Sequence(data.GetLength(0)),
            pcaComponentCount:=pcaComponents,
            pcaUseCorrelationMatrix:=useCorrelation)
    End Function

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub Request_SnapshotsArraysAndValidatesChartSpecificContracts()
        Dim data As Double(,) = IndividualMultivariateData()
        Dim request As New SpcMultivariateRequest(
            SpcChartType.HotellingT2, data,
            variableNames:={"A", "B", "C"}, requestLabel:=" MV ")
        data(0, 0) = 999.0
        Assert.AreNotEqual(999.0, request.Measurements(0, 0))
        Assert.AreEqual("A", request.VariableNames(0))
        Assert.AreEqual("MV", request.RequestLabel)

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                Dim unused = New SpcMultivariateRequest(
                    SpcChartType.XBar, IndividualMultivariateData())
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcMultivariateRequest(
                    SpcChartType.GeneralizedVariance, IndividualMultivariateData())
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcMultivariateRequest(
                    SpcChartType.PcaT2, IndividualMultivariateData(),
                    subgroupIds:=Enumerable.Repeat("G", 48).ToArray())
            End Sub)
        AssertThrowsAssignable(Of ArgumentException)(
            Sub()
                Dim unused = New SpcMultivariateRequest(
                    SpcChartType.HotellingT2, IndividualMultivariateData(),
                    missingValuePolicy:=SpcMissingValuePolicy.UseAvailableMeasurements)
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub IndividualHotelling_FitsPhaseIModelAndReturnsContributionDiagnostics()
        Dim result As SpcMultivariateFitResult =
            SpcMultivariate.Fit(IndividualRequest(SpcChartType.HotellingT2))
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.HotellingT2)
        Assert.IsNotNull(panel)
        Assert.AreEqual(48, panel.PointCount)
        Assert.AreEqual(36, result.Model.BaselineObservationCount)
        Assert.AreEqual(3, result.Model.EffectiveDimension)
        Assert.AreEqual(48, result.Diagnostics.Length)
        AssertFiniteVector(result.Model.ProcessMean)
        For Each point As SpcPointResult In panel.Points
            Assert.IsTrue(point.Value >= 0.0)
            Assert.IsTrue(point.UpperControlLimit > 0.0)
            Dim diagnostic As SpcMultivariatePointDiagnostic =
                result.GetDiagnostic(point.PointIndex)
            Assert.IsNotNull(diagnostic)
            AssertClose(point.Value, diagnostic.Statistic, 0.000001)
            AssertClose(diagnostic.Statistic, diagnostic.ContributionTotal, 0.000001)
        Next
        Assert.IsTrue(panel.Points.Skip(36).Any(Function(p) p.IsSignalled),
                      "The synthetic Phase-II location shift should be detected.")
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub HistoricalHotelling_WithIdentityCovarianceEqualsSquaredEuclideanDistance()
        Dim data As Double(,) = {{1.0, 2.0}, {0.0, 0.0}, {5.0, 5.0}}
        Dim request As New SpcMultivariateRequest(
            SpcChartType.HotellingT2,
            data,
            modelSource:=SpcMultivariateModelSource.UseHistoricalParameters,
            historicalMean:={0.0, 0.0},
            historicalCovariance:=Identity(2),
            controlLimitAlpha:=0.01)
        Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(request)
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.HotellingT2)
        AssertClose(5.0, panel.Points(0).Value)
        AssertClose(0.0, panel.Points(1).Value)
        AssertClose(50.0, panel.Points(2).Value)
        Assert.AreEqual(SpcMultivariateModelSource.UseHistoricalParameters,
                        result.Model.Source)
        Assert.AreEqual(3, result.Model.BaselineObservationCount)
        Assert.IsTrue(panel.Points(2).IsSignalled)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub GroupedHotellingAndGeneralizedVariance_ReturnSubgroupPanelsAndCovarianceDiagnostics()
        Dim data As Double(,) = Nothing
        Dim ids As String() = Nothing
        Dim phases As SpcPhase() = Nothing
        Dim stages As String() = Nothing
        GroupedMultivariateData(data, ids, phases, stages)

        Dim hotelling As SpcMultivariateFitResult = SpcMultivariate.Fit(
            New SpcMultivariateRequest(
                SpcChartType.HotellingT2, data,
                subgroupIds:=ids, phases:=phases, stageIds:=stages))
        Dim hPanel As SpcPanelResult = hotelling.GetPanel(SpcPanelType.HotellingT2)
        Assert.AreEqual(14, hPanel.PointCount)
        Assert.AreEqual(10, hotelling.Model.BaselineSubgroupCount)
        Assert.IsTrue(hotelling.Diagnostics.All(
            Function(d) d.SubgroupCovariance IsNot Nothing))

        Dim generalized As SpcMultivariateFitResult = SpcMultivariate.Fit(
            New SpcMultivariateRequest(
                SpcChartType.GeneralizedVariance, data,
                subgroupIds:=ids, phases:=phases, stageIds:=stages))
        Dim gPanel As SpcPanelResult =
            generalized.GetPanel(SpcPanelType.GeneralizedVariance)
        Assert.AreEqual(14, gPanel.PointCount)
        Assert.IsTrue(gPanel.Points.All(Function(p) p.Value > 0.0))
        Assert.IsTrue(generalized.Diagnostics.All(
            Function(d) d.SubgroupCovariance IsNot Nothing))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub PcaT2AndQ_RetainRequestedComponentsAndPartitionDiagnostics()
        For Each chartType As SpcChartType In {SpcChartType.PcaT2, SpcChartType.PcaQ}
            Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(
                IndividualRequest(chartType, pcaComponents:=2, useCorrelation:=True))
            Dim expectedPanel As SpcPanelType = If(
                chartType = SpcChartType.PcaT2, SpcPanelType.PcaT2, SpcPanelType.PcaQ)
            Dim panel As SpcPanelResult = result.GetPanel(expectedPanel)
            Assert.IsNotNull(panel)
            Assert.AreEqual(2, result.Model.RetainedComponentCount)
            Assert.AreEqual(3, result.Model.Eigenvalues.Length)
            Assert.AreEqual(48, result.Diagnostics.Length)
            For Each diagnostic As SpcMultivariatePointDiagnostic In result.Diagnostics
                Assert.AreEqual(2, diagnostic.ComponentScores.Length)
                Assert.AreEqual(3, diagnostic.ResidualVector.Length)
                Assert.AreEqual(3, diagnostic.Contributions.Length)
                AssertClose(diagnostic.Statistic,
                            diagnostic.ContributionTotal,
                            0.000001,
                            chartType.ToString())
            Next
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub MewmaAndMcusum_ProduceStateDiagnosticsAndDetectLargeShift()
        Dim source As Double(,) = IndividualMultivariateData()
        Dim mean As Double() = {0.0, 0.0, 0.0}
        Dim covariance As Double(,) = Identity(3)
        For Each chartType As SpcChartType In {SpcChartType.Mewma, SpcChartType.Mcusum}
            Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(
                New SpcMultivariateRequest(
                    chartType,
                    source,
                    modelSource:=SpcMultivariateModelSource.UseHistoricalParameters,
                    historicalMean:=mean,
                    historicalCovariance:=covariance,
                    mewmaLambda:=0.25,
                    mewmaControlLimit:=8.0,
                    mcusumReferenceValue:=0.5,
                    mcusumDecisionInterval:=4.0))
            Dim panelType As SpcPanelType = If(
                chartType = SpcChartType.Mewma, SpcPanelType.Mewma, SpcPanelType.Mcusum)
            Dim panel As SpcPanelResult = result.GetPanel(panelType)
            Assert.IsNotNull(panel)
            Assert.AreEqual(source.GetLength(0), panel.PointCount)
            Assert.IsTrue(result.Diagnostics.All(
                Function(d) d.StateVector IsNot Nothing AndAlso d.StateVector.Length = 3))
            Assert.IsTrue(panel.Points.Any(Function(p) p.IsSignalled),
                          chartType.ToString() & " should signal on the synthetic shift.")
        Next
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub MissingValueOmitPoint_PreservesOriginalPointIndicesAndAuditableExclusions()
        Dim data As Double(,) = IndividualMultivariateData()
        data(5, 1) = Double.NaN
        Dim phases As SpcPhase() = Nothing
        Dim stages As String() = Nothing
        MultivariatePhasesAndStages(data.GetLength(0), 36, phases, stages)
        Dim scopes(data.GetLength(0) - 1) As SpcExclusionScope
        Dim reasons(data.GetLength(0) - 1) As String
        scopes(6) = SpcExclusionScope.EstimationAndRules
        reasons(6) = "Known setup event"

        Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(
            New SpcMultivariateRequest(
                SpcChartType.HotellingT2, data,
                phases:=phases, stageIds:=stages,
                exclusionScopes:=scopes,
                exclusionReasons:=reasons,
                missingValuePolicy:=SpcMissingValuePolicy.OmitPoint))
        Dim panel As SpcPanelResult = result.GetPanel(SpcPanelType.HotellingT2)
        Assert.AreEqual(data.GetLength(0) - 1, panel.PointCount)
        Assert.IsNull(panel.GetPoint(5))
        Dim excluded As SpcPointResult = panel.GetPoint(6)
        Assert.IsNotNull(excluded)
        Assert.IsTrue(excluded.IsExplicitlyExcluded)
        Assert.IsFalse(excluded.IncludedInParameterEstimation)
        Assert.IsFalse(excluded.IncludedInRuleEvaluation)
        Assert.AreEqual("Known setup event", excluded.ExclusionReason)
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub SingularCovariance_UsesPseudoInverseWhenAllowed()
        Dim data(29, 2) As Double
        For i As Integer = 0 To 29
            Dim x As Double = Math.Sin(CDbl(i + 1) * 0.3)
            data(i, 0) = x
            data(i, 1) = 2.0 * x
            data(i, 2) = -3.0 * x
        Next
        Dim result As SpcMultivariateFitResult = SpcMultivariate.Fit(
            New SpcMultivariateRequest(
                SpcChartType.HotellingT2, data,
                allowPseudoInverse:=True))
        Assert.IsTrue(result.Model.UsedPseudoInverse)
        Assert.AreEqual(1, result.Model.EffectiveDimension)
        Assert.IsTrue(result.Warnings.Any(
            Function(w) w.IndexOf("pseudo", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                        w.IndexOf("rank", StringComparison.OrdinalIgnoreCase) >= 0))
    End Sub

    <TestMethod>
    <TestCategory("SPC-Multivariate")>
    Public Sub MultivariateCancellation_IsObservedBeforeFitting()
        Assert.ThrowsException(Of OperationCanceledException)(
            Sub() SpcMultivariate.Fit(
                IndividualRequest(SpcChartType.HotellingT2),
                Function() True))
    End Sub

End Class
