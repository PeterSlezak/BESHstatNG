Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Provides generic scalar and vector jackknife runners that execute statistic delegates against ordinary
    ''' or clustered leave-one-out index samples.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is the jackknife analogue of <see cref="ResamplingBootstrapRunner"/>. The shared jackknife
    ''' infrastructure in <see cref="ResamplingJackknife"/> already knows how to generate ordinary and clustered
    ''' leave-one-out index vectors. This runner sits one level above that infrastructure and handles the common
    ''' execution pattern that previously lived inside individual statistical methods:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>normalize jackknife options and create deterministic run metadata</description></item>
    '''   <item><description>evaluate the observed statistic on the full original sample</description></item>
    '''   <item><description>iterate over leave-one-out resamples and collect successful replicate statistics</description></item>
    '''   <item><description>count failed/discarded replicates consistently</description></item>
    '''   <item><description>return a shared <see cref="ScalarResamplingResult"/> or <see cref="VectorResamplingResult"/></description></item>
    ''' </list>
    ''' <para>
    ''' The statistic-specific formulas remain outside this module. Callers provide those formulas via delegates that
    ''' consume an integer index vector and return either a scalar statistic or a parameter vector.
    ''' </para>
    ''' <para>
    ''' The runner treats non-finite leave-one-out statistics as failed replicates. Because jackknife is deterministic,
    ''' there is no maximum-failure threshold option; the run succeeds as long as at least the requested minimum number
    ''' of successful replicates is obtained.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingJackknifeRunner

        ''' <summary>
        ''' Runs an ordinary row-level jackknife for a scalar statistic.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <param name="statistic">
        ''' Delegate that computes the statistic of interest from a leave-one-out index vector.
        ''' The same delegate is also evaluated once on the original identity index vector to obtain the observed statistic.
        ''' </param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">
        ''' Minimum number of successful leave-one-out replicates required for the run to be accepted.
        ''' </param>
        ''' <param name="progressCallback">
        ''' Optional callback receiving the number of attempted replicates completed and the total number requested.
        ''' </param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarJackknife(sampleSize As Integer,
                                           statistic As Func(Of Integer(), Double),
                                           opts As JackknifeOptions,
                                           Optional statisticLabel As String = "",
                                           Optional methodLabel As String = "",
                                           Optional minimumSuccessfulReplicates As Integer = 1,
                                           Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult

            ResamplingJackknife.ValidateLeaveOneOutSampleSize(sampleSize)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim info As ResamplingRunInfo = ResamplingJackknife.CreateJackknifeContext(sampleSize, opts, methodLabel)
            Dim observedIndices As Integer() = BuildIdentityIndices(sampleSize, 2)
            Dim observedStatistic As Double = statistic(observedIndices)
            ValidateFiniteScalar(observedStatistic, "observed statistic")

            Return ExecuteScalarJackknife(observedStatistic,
                                          ResamplingJackknife.LeaveOneOutIndices(sampleSize),
                                          statistic,
                                          info,
                                          minimumSuccessfulReplicates,
                                          statisticLabel,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs an ordinary row-level jackknife for a vector statistic.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <param name="statistic">Delegate that computes the parameter vector of interest from a leave-one-out index vector.</param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="parameterLabels">Optional labels for the parameters returned by <paramref name="statistic"/>.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful leave-one-out replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorJackknife(sampleSize As Integer,
                                           statistic As Func(Of Integer(), Double()),
                                           opts As JackknifeOptions,
                                           Optional parameterLabels As String() = Nothing,
                                           Optional methodLabel As String = "",
                                           Optional minimumSuccessfulReplicates As Integer = 1,
                                           Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult

            ResamplingJackknife.ValidateLeaveOneOutSampleSize(sampleSize)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim info As ResamplingRunInfo = ResamplingJackknife.CreateJackknifeContext(sampleSize, opts, methodLabel)
            Dim observedIndices As Integer() = BuildIdentityIndices(sampleSize, 2)
            Dim observedVector As Double() = statistic(observedIndices)
            ValidateFiniteVector(observedVector, "observed statistic vector")
            ValidateParameterLabels(parameterLabels, observedVector.Length)

            Return ExecuteVectorJackknife(observedVector,
                                          ResamplingJackknife.LeaveOneOutIndices(sampleSize),
                                          statistic,
                                          info,
                                          minimumSuccessfulReplicates,
                                          parameterLabels,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered jackknife for a scalar statistic using raw cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <param name="statistic">Delegate that computes the statistic from a leave-one-out index vector.</param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful cluster leave-one-out replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarClusterJackknife(clusterIds As Object(),
                                                  statistic As Func(Of Integer(), Double),
                                                  opts As JackknifeOptions,
                                                  Optional statisticLabel As String = "",
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult
            Dim blocks As List(Of Integer()) = ResamplingBootstrap.BuildClusterIndexBlocks(clusterIds)
            Return RunScalarClusterJackknife(blocks, statistic, opts, statisticLabel, methodLabel, minimumSuccessfulReplicates, progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered jackknife for a scalar statistic using precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <param name="statistic">Delegate that computes the statistic from a leave-one-out index vector.</param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful cluster leave-one-out replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarClusterJackknife(clusterBlocks As List(Of Integer()),
                                                  statistic As Func(Of Integer(), Double),
                                                  opts As JackknifeOptions,
                                                  Optional statisticLabel As String = "",
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult

            ValidateClusterBlocks(clusterBlocks)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim info As ResamplingRunInfo = ResamplingJackknife.CreateJackknifeContext(clusterBlocks.Count, opts, methodLabel)
            Dim observedIndices As Integer() = BuildOriginalIndicesFromBlocks(clusterBlocks, 2)
            Dim observedStatistic As Double = statistic(observedIndices)
            ValidateFiniteScalar(observedStatistic, "observed statistic")

            Return ExecuteScalarJackknife(observedStatistic,
                                          ResamplingJackknife.ClusterLeaveOneOutIndices(clusterBlocks),
                                          statistic,
                                          info,
                                          minimumSuccessfulReplicates,
                                          statisticLabel,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered jackknife for a vector statistic using raw cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <param name="statistic">Delegate that computes the parameter vector from a leave-one-out index vector.</param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="parameterLabels">Optional labels for the returned parameter vector.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful cluster leave-one-out replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorClusterJackknife(clusterIds As Object(),
                                                  statistic As Func(Of Integer(), Double()),
                                                  opts As JackknifeOptions,
                                                  Optional parameterLabels As String() = Nothing,
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult
            Dim blocks As List(Of Integer()) = ResamplingBootstrap.BuildClusterIndexBlocks(clusterIds)
            Return RunVectorClusterJackknife(blocks, statistic, opts, parameterLabels, methodLabel, minimumSuccessfulReplicates, progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered jackknife for a vector statistic using precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <param name="statistic">Delegate that computes the parameter vector from a leave-one-out index vector.</param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="parameterLabels">Optional labels for the returned parameter vector.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful cluster leave-one-out replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorClusterJackknife(clusterBlocks As List(Of Integer()),
                                                  statistic As Func(Of Integer(), Double()),
                                                  opts As JackknifeOptions,
                                                  Optional parameterLabels As String() = Nothing,
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult

            ValidateClusterBlocks(clusterBlocks)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim info As ResamplingRunInfo = ResamplingJackknife.CreateJackknifeContext(clusterBlocks.Count, opts, methodLabel)
            Dim observedIndices As Integer() = BuildOriginalIndicesFromBlocks(clusterBlocks, 2)
            Dim observedVector As Double() = statistic(observedIndices)
            ValidateFiniteVector(observedVector, "observed statistic vector")
            ValidateParameterLabels(parameterLabels, observedVector.Length)

            Return ExecuteVectorJackknife(observedVector,
                                          ResamplingJackknife.ClusterLeaveOneOutIndices(clusterBlocks),
                                          statistic,
                                          info,
                                          minimumSuccessfulReplicates,
                                          parameterLabels,
                                          progressCallback)
        End Function

        Private Function ExecuteScalarJackknife(observedStatistic As Double,
                                                resamples As IEnumerable(Of Integer()),
                                                statistic As Func(Of Integer(), Double),
                                                info As ResamplingRunInfo,
                                                minimumSuccessfulReplicates As Integer,
                                                statisticLabel As String,
                                                progressCallback As Action(Of Integer, Integer)) As ScalarResamplingResult
            If resamples Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(resamples)))
            If info Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))

            Dim estimates As New List(Of Double)(Math.Max(1, info.ReplicatesRequested))
            Dim failed As Integer = 0
            Dim attempted As Integer = 0
            Dim firstFailure As Exception = Nothing

            For Each idx As Integer() In resamples
                attempted += 1
                Try
                    Dim value As Double = statistic(idx)
                    ValidateFiniteScalar(value, "jackknife replicate statistic")
                    estimates.Add(value)
                Catch ex As Exception
                    failed += 1
                    If firstFailure Is Nothing Then firstFailure = ex
                End Try

                ReportProgress(progressCallback, attempted, info.ReplicatesRequested)
            Next

            If estimates.Count < minimumSuccessfulReplicates Then
                Dim msg As String = $"Too few successful jackknife replicates were obtained ({estimates.Count} < {minimumSuccessfulReplicates})."
                CoreServices.Errors.LogAndThrow(New InvalidOperationException(msg, firstFailure))
            End If

            ResamplingCore.CompleteRunInfo(info, estimates.Count, failed)
            If failed > 0 Then ResamplingCore.AppendNote(info, $"Failed/discarded jackknife replicates = {failed}.")

            Return New ScalarResamplingResult With {
                .StatisticLabel = If(statisticLabel, String.Empty),
                .ObservedStatistic = observedStatistic,
                .ResampledStatistics = estimates.ToArray(),
                .RunInfo = info
            }
        End Function

        Private Function ExecuteVectorJackknife(observedVector As Double(),
                                                resamples As IEnumerable(Of Integer()),
                                                statistic As Func(Of Integer(), Double()),
                                                info As ResamplingRunInfo,
                                                minimumSuccessfulReplicates As Integer,
                                                parameterLabels As String(),
                                                progressCallback As Action(Of Integer, Integer)) As VectorResamplingResult
            If resamples Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(resamples)))
            If info Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))

            Dim estimates As New List(Of Double())(Math.Max(1, info.ReplicatesRequested))
            Dim failed As Integer = 0
            Dim attempted As Integer = 0
            Dim firstFailure As Exception = Nothing
            Dim parameterCount As Integer = observedVector.Length

            For Each idx As Integer() In resamples
                attempted += 1
                Try
                    Dim value As Double() = statistic(idx)
                    ValidateFiniteVector(value, "jackknife replicate statistic vector")
                    If value.Length <> parameterCount Then
                        CoreServices.Errors.LogAndThrow(New InvalidOperationException($"Jackknife replicate vector length {value.Length} does not match the observed parameter count {parameterCount}."))
                    End If
                    estimates.Add(DirectCast(value.Clone(), Double()))
                Catch ex As Exception
                    failed += 1
                    If firstFailure Is Nothing Then firstFailure = ex
                End Try

                ReportProgress(progressCallback, attempted, info.ReplicatesRequested)
            Next

            If estimates.Count < minimumSuccessfulReplicates Then
                Dim msg As String = $"Too few successful jackknife replicates were obtained ({estimates.Count} < {minimumSuccessfulReplicates})."
                CoreServices.Errors.LogAndThrow(New InvalidOperationException(msg, firstFailure))
            End If

            ResamplingCore.CompleteRunInfo(info, estimates.Count, failed)
            If failed > 0 Then ResamplingCore.AppendNote(info, $"Failed/discarded jackknife replicates = {failed}.")

            Dim result As New VectorResamplingResult With {
                .ObservedStatistics = DirectCast(observedVector.Clone(), Double()),
                .ResampledStatistics = estimates.ToArray(),
                .RunInfo = info
            }
            If parameterLabels IsNot Nothing Then result.ParameterLabels = DirectCast(parameterLabels.Clone(), String())
            Return result
        End Function

    End Module

End Namespace
