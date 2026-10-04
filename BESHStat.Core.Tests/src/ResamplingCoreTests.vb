Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.Resampling

<TestClass>
Public Class ResamplingCoreTests

    <TestMethod>
    Public Sub Bootstrap_ExplicitSeed_ReproducesIndexSequence()
        Dim opts1 As New BootstrapOptions With {
            .Alpha = 0.05R,
            .Replicates = 8,
            .RandomSeed = 12345,
            .MaxFailures = 0
        }
        Dim opts2 As New BootstrapOptions With {
            .Alpha = 0.05R,
            .Replicates = 8,
            .RandomSeed = 12345,
            .MaxFailures = 0
        }

        Dim ctx1 = ResamplingBootstrap.CreateBootstrapContext(opts1, "seed-test")
        Dim ctx2 = ResamplingBootstrap.CreateBootstrapContext(opts2, "seed-test")

        Assert.AreEqual(12345, ctx1.Info.SeedUsed)
        Assert.AreEqual(12345, ctx2.Info.SeedUsed)
        Assert.AreEqual(8, ctx1.Info.ReplicatesRequested)
        Assert.AreEqual(0.05R, ctx1.Info.AlphaUsed, 0.0R)

        Dim draws1 As New List(Of String)()
        Dim draws2 As New List(Of String)()

        For Each idx As Integer() In ResamplingBootstrap.BootstrapIndices(5, 8, ctx1.Rng)
            draws1.Add(IndexKey(idx))
        Next
        For Each idx As Integer() In ResamplingBootstrap.BootstrapIndices(5, 8, ctx2.Rng)
            draws2.Add(IndexKey(idx))
        Next

        CollectionAssert.AreEqual(draws1.ToArray(), draws2.ToArray())
    End Sub

    <TestMethod>
    Public Sub BootstrapRunner_CountsFailedReplicatesAndKeepsSuccessfulStatistics()
        Dim callNumber As Integer = 0
        Dim statistic As Func(Of Integer(), Double) =
            Function(indices As Integer()) As Double
                callNumber += 1
                If callNumber = 3 OrElse callNumber = 5 Then Return Double.NaN

                Dim total As Double = 0.0R
                For Each idx As Integer In indices
                    total += idx
                Next
                Return total / indices.Length
            End Function

        Dim opts As New BootstrapOptions With {
            .Alpha = 0.05R,
            .Replicates = 5,
            .RandomSeed = 24680,
            .MaxFailures = 5
        }

        Dim result As ScalarResamplingResult =
            ResamplingBootstrapRunner.RunScalarBootstrap(4,
                                                         statistic,
                                                         opts,
                                                         statisticLabel:="mean index",
                                                         methodLabel:="failure-count test",
                                                         minimumSuccessfulReplicates:=3)

        Assert.AreEqual(1.5R, result.ObservedStatistic, 0.0R)
        Assert.AreEqual(3, result.ReplicateCount)
        Assert.AreEqual(5, result.RunInfo.ReplicatesRequested)
        Assert.AreEqual(3, result.RunInfo.ReplicatesUsed)
        Assert.AreEqual(2, result.RunInfo.FailedReplicates)
        Assert.AreEqual(24680, result.RunInfo.SeedUsed)
        Assert.IsTrue(result.RunInfo.Notes.Count > 0)
        Assert.IsTrue(result.RunInfo.Notes(result.RunInfo.Notes.Count - 1).Contains("Failed/discarded resamples = 2"))
    End Sub

    <TestMethod>
    Public Sub JackknifeRunner_IsDeterministicAndProducesExpectedLeaveOneOutMeans()
        Dim data() As Double = {2.0R, 4.0R, 8.0R, 10.0R}
        Dim statistic As Func(Of Integer(), Double) =
            Function(indices As Integer()) As Double
                Dim total As Double = 0.0R
                For Each idx As Integer In indices
                    total += data(idx)
                Next
                Return total / indices.Length
            End Function

        Dim result As ScalarResamplingResult =
            ResamplingJackknifeRunner.RunScalarJackknife(data.Length,
                                                         statistic,
                                                         New JackknifeOptions With {.Alpha = 0.05R},
                                                         statisticLabel:="mean",
                                                         methodLabel:="jackknife-test")

        Assert.AreEqual(6.0R, result.ObservedStatistic, 0.0R)
        Assert.AreEqual(4, result.ReplicateCount)
        Assert.AreEqual(22.0R / 3.0R, result.ResampledStatistics(0), 0.000000000001R)
        Assert.AreEqual(20.0R / 3.0R, result.ResampledStatistics(1), 0.000000000001R)
        Assert.AreEqual(16.0R / 3.0R, result.ResampledStatistics(2), 0.000000000001R)
        Assert.AreEqual(14.0R / 3.0R, result.ResampledStatistics(3), 0.000000000001R)
        Assert.AreEqual(Integer.MinValue, result.RunInfo.SeedUsed)
        Assert.AreEqual(4, result.RunInfo.ReplicatesRequested)
        Assert.AreEqual(4, result.RunInfo.ReplicatesUsed)
        Assert.AreEqual(0, result.RunInfo.FailedReplicates)
        Assert.IsTrue(result.RunInfo.Notes.Count > 0)
        Assert.IsTrue(result.RunInfo.Notes(0).Contains("deterministic"))

        Assert.AreEqual(Math.Sqrt(10.0R / 3.0R),
                        ResamplingJackknife.JackknifeStandardError(result.ResampledStatistics),
                        0.000000000001R)
    End Sub

    <TestMethod>
    Public Sub ScalarResult_PercentileAndBcaIntervals_MatchDeterministicReference()
        Dim result As New ScalarResamplingResult With {
            .StatisticLabel = "skewed statistic",
            .ObservedStatistic = 1.0R,
            .ResampledStatistics = New Double() {
                0.2R, 0.35R, 0.5R, 0.6R, 0.72R, 0.85R, 0.93R,
                1.05R, 1.12R, 1.25R, 1.4R, 1.8R, 2.4R, 3.0R
            }
        }
        Dim jackknife() As Double = {0.88R, 0.91R, 0.95R, 0.98R, 1.02R, 1.08R, 1.15R}

        Dim percentileCi As BESHStatNG.ConfidenceIntervalResult = result.ToPercentileConfidenceInterval(0.05R)
        Dim bcaCi As BESHStatNG.ConfidenceIntervalResult = result.ToBcaConfidenceInterval(0.05R, jackknife)

        Assert.AreEqual(0.24875R, percentileCi.LowerLimit, 0.000000000001R)
        Assert.AreEqual(2.805R, percentileCi.UpperLimit, 0.000000000001R)
        Assert.AreEqual(0.26964285714285713R, bcaCi.LowerLimit, 0.0000000001R)
        Assert.AreEqual(2.721428571428572R, bcaCi.UpperLimit, 0.0000000001R)
        Assert.AreEqual(1.0R, bcaCi.Estimate, 0.0R)
        Assert.AreEqual(0.05R, bcaCi.alpha, 0.0R)
    End Sub

    <TestMethod>
    Public Sub Permutation_ExactEnumeration_UsesAllUniqueArrangementsWithinLimit()
        Dim opts As New PermutationOptions With {
            .Alpha = 0.05R,
            .Mode = PermutationMode.ExactEnumeration,
            .MonteCarloReplicates = 20,
            .RandomSeed = 111,
            .MaxExactEnumerations = 6
        }
        Dim ctx = ResamplingPermutation.CreatePermutationContext(opts, "exact-test")

        Assert.AreEqual(0, ctx.Info.ReplicatesRequested)
        Assert.IsTrue(ResamplingPermutation.CanEnumerateExactly(6L, opts))
        Assert.IsFalse(ResamplingPermutation.CanEnumerateExactly(7L, opts))

        Dim keys As New HashSet(Of String)(StringComparer.Ordinal)
        Dim count As Integer = 0
        For Each permutation As Integer() In ResamplingPermutation.EnumeratePermutations(3)
            count += 1
            keys.Add(IndexKey(permutation))
        Next

        Assert.AreEqual(6, count)
        Assert.AreEqual(6, keys.Count)
        Assert.AreEqual(3L, ResamplingPermutation.ExpectedUniquePermutations(New Double() {1.0R, 1.0R, 2.0R}))
    End Sub

    <TestMethod>
    Public Sub Permutation_MonteCarloPath_IsReproducibleWithFixedSeed()
        Dim opts1 As New PermutationOptions With {
            .Alpha = 0.05R,
            .Mode = PermutationMode.MonteCarlo,
            .MonteCarloReplicates = 10,
            .RandomSeed = 97531,
            .MaxExactEnumerations = 100
        }
        Dim opts2 As New PermutationOptions With {
            .Alpha = 0.05R,
            .Mode = PermutationMode.MonteCarlo,
            .MonteCarloReplicates = 10,
            .RandomSeed = 97531,
            .MaxExactEnumerations = 100
        }
        Dim ctx1 = ResamplingPermutation.CreatePermutationContext(opts1, "mc-test")
        Dim ctx2 = ResamplingPermutation.CreatePermutationContext(opts2, "mc-test")

        Assert.AreEqual(10, ctx1.Info.ReplicatesRequested)
        Assert.AreEqual(97531, ctx1.Info.SeedUsed)

        Dim draws1 As New List(Of String)()
        Dim draws2 As New List(Of String)()
        For Each permutation As Integer() In ResamplingPermutation.MonteCarloPermutations(5, 10, ctx1.Rng)
            draws1.Add(IndexKey(permutation))
        Next
        For Each permutation As Integer() In ResamplingPermutation.MonteCarloPermutations(5, 10, ctx2.Rng)
            draws2.Add(IndexKey(permutation))
        Next

        Assert.AreEqual(10, draws1.Count)
        CollectionAssert.AreEqual(draws1.ToArray(), draws2.ToArray())
    End Sub

    <TestMethod>
    Public Sub PermutationResult_ComputesEmpiricalTailsAndStandardTestResult()
        Dim opts As New PermutationOptions With {
            .Alpha = 0.05R,
            .Alternative = AlternativeHypothesis.TwoSided,
            .Mode = PermutationMode.ExactEnumeration,
            .UseAddOneCorrection = False
        }
        Dim info As New ResamplingRunInfo With {
            .MethodLabel = "exact permutation",
            .ReplicatesUsed = 5
        }

        Dim result As PermutationResamplingResult =
            ResamplingPermutation.BuildPermutationResult(2.0R,
                                                         New Double() {-2.0R, -1.0R, 0.0R, 1.0R, 2.0R},
                                                         opts,
                                                         statisticLabel:="T",
                                                         runInfo:=info)

        Assert.AreEqual(1.0R, result.LowerTailPValue, 0.0R)
        Assert.AreEqual(0.2R, result.UpperTailPValue, 0.000000000000001R)
        Assert.AreEqual(0.4R, result.TwoSidedPValue, 0.000000000000001R)

        Dim standardResult As BESHStatNG.TestResult = result.ToTestResult()
        Assert.AreEqual(0.4R, standardResult.Pvalue, 0.000000000000001R)
        Assert.AreEqual(1.0R, standardResult.PvalueLowerSide, 0.0R)
        Assert.AreEqual(0.2R, standardResult.PvalueUpperSide, 0.000000000000001R)
        Assert.AreEqual(2.0R, standardResult.TestStatistics1, 0.0R)
    End Sub

    Private Shared Function IndexKey(indices As Integer()) As String
        If indices Is Nothing OrElse indices.Length = 0 Then Return String.Empty
        Dim parts(indices.Length - 1) As String
        For i As Integer = 0 To indices.Length - 1
            parts(i) = indices(i).ToString(Globalization.CultureInfo.InvariantCulture)
        Next
        Return String.Join(",", parts)
    End Function

End Class
