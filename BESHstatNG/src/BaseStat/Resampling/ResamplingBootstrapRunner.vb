Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Provides generic scalar and vector bootstrap runners that execute statistic delegates against ordinary
    ''' or clustered bootstrap index samples.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The shared bootstrap infrastructure in <see cref="ResamplingBootstrap"/> already knows how to generate
    ''' ordinary and clustered bootstrap index vectors. This module sits one level above that infrastructure and
    ''' handles the common execution pattern that previously lived inside each individual statistical method:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>create and normalize bootstrap options</description></item>
    '''   <item><description>create run metadata and the seeded random-number generator</description></item>
    '''   <item><description>evaluate the observed statistic on the original sample</description></item>
    '''   <item><description>iterate over bootstrap resamples and collect successful replicate statistics</description></item>
    '''   <item><description>count failed/discarded replicates consistently</description></item>
    '''   <item><description>return a shared <see cref="ScalarResamplingResult"/> or <see cref="VectorResamplingResult"/></description></item>
    ''' </list>
    ''' <para>
    ''' The statistic-specific formulas remain outside this module. Callers provide those formulas via delegates that
    ''' consume an integer index vector and return either a scalar statistic or a parameter vector.
    ''' </para>
    ''' <para>
    ''' The runner deliberately treats non-finite replicate statistics as failed replicates. This mirrors the current
    ''' method-specific behavior where singular fits or invalid bootstrap replicates are skipped rather than stored as
    ''' <c>NaN</c> values in the result arrays.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingBootstrapRunner

        ''' <summary>
        ''' Runs an ordinary row-level bootstrap for a scalar statistic.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <param name="statistic">
        ''' Delegate that computes the statistic of interest from a resampled index vector.
        ''' The same delegate is also evaluated once on the original identity index vector to obtain the observed statistic.
        ''' </param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">
        ''' Minimum number of successful bootstrap replicates required for the run to be accepted.
        ''' </param>
        ''' <param name="progressCallback">
        ''' Optional callback receiving the number of attempted replicates completed and the total number requested.
        ''' </param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarBootstrap(sampleSize As Integer,
                                           statistic As Func(Of Integer(), Double),
                                           opts As BootstrapOptions,
                                           Optional statisticLabel As String = "",
                                           Optional methodLabel As String = "",
                                           Optional minimumSuccessfulReplicates As Integer = 1,
                                           Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult

            ValidatePositiveReplicates(sampleSize, NameOf(sampleSize), 1)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim ctx = ResamplingBootstrap.CreateBootstrapContext(opts, methodLabel)
            Dim observedIndices As Integer() = BuildIdentityIndices(sampleSize, 1)
            Dim observedStatistic As Double = statistic(observedIndices)
            ValidateFiniteScalar(observedStatistic, "observed statistic")

            Return ExecuteScalarBootstrap(observedStatistic,
                                          ResamplingBootstrap.BootstrapIndices(sampleSize, ctx.Info.ReplicatesRequested, ctx.Rng),
                                          statistic,
                                          ctx.Info,
                                          ResamplingCore.NormalizeBootstrapOptions(opts).MaxFailures,
                                          minimumSuccessfulReplicates,
                                          statisticLabel,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs an ordinary row-level bootstrap for a vector statistic.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <param name="statistic">
        ''' Delegate that computes the parameter vector of interest from a resampled index vector.
        ''' </param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="parameterLabels">Optional labels for the parameters returned by <paramref name="statistic"/>.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">
        ''' Minimum number of successful bootstrap replicates required for the run to be accepted.
        ''' </param>
        ''' <param name="progressCallback">
        ''' Optional callback receiving the number of attempted replicates completed and the total number requested.
        ''' </param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorBootstrap(sampleSize As Integer,
                                           statistic As Func(Of Integer(), Double()),
                                           opts As BootstrapOptions,
                                           Optional parameterLabels As String() = Nothing,
                                           Optional methodLabel As String = "",
                                           Optional minimumSuccessfulReplicates As Integer = 1,
                                           Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult

            ValidatePositiveReplicates(sampleSize, NameOf(sampleSize), 1)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim ctx = ResamplingBootstrap.CreateBootstrapContext(opts, methodLabel)
            Dim observedIndices As Integer() = BuildIdentityIndices(sampleSize, 1)
            Dim observedVector As Double() = statistic(observedIndices)
            ValidateFiniteVector(observedVector, "observed statistic vector")
            ValidateParameterLabels(parameterLabels, observedVector.Length)

            Return ExecuteVectorBootstrap(observedVector,
                                          ResamplingBootstrap.BootstrapIndices(sampleSize, ctx.Info.ReplicatesRequested, ctx.Rng),
                                          statistic,
                                          ctx.Info,
                                          ResamplingCore.NormalizeBootstrapOptions(opts).MaxFailures,
                                          minimumSuccessfulReplicates,
                                          parameterLabels,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered bootstrap for a scalar statistic using raw cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <param name="statistic">Delegate that computes the statistic from a resampled index vector.</param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful bootstrap replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarClusterBootstrap(clusterIds As Object(),
                                                  statistic As Func(Of Integer(), Double),
                                                  opts As BootstrapOptions,
                                                  Optional statisticLabel As String = "",
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult
            Dim blocks As List(Of Integer()) = ResamplingBootstrap.BuildClusterIndexBlocks(clusterIds)
            Return RunScalarClusterBootstrap(blocks, statistic, opts, statisticLabel, methodLabel, minimumSuccessfulReplicates, progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered bootstrap for a scalar statistic using precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <param name="statistic">Delegate that computes the statistic from a resampled index vector.</param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the stored statistic.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful bootstrap replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="ScalarResamplingResult"/>.</returns>
        Public Function RunScalarClusterBootstrap(clusterBlocks As List(Of Integer()),
                                                  statistic As Func(Of Integer(), Double),
                                                  opts As BootstrapOptions,
                                                  Optional statisticLabel As String = "",
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As ScalarResamplingResult

            ValidateClusterBlocks(clusterBlocks)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim ctx = ResamplingBootstrap.CreateBootstrapContext(opts, methodLabel)
            Dim observedIndices As Integer() = BuildOriginalIndicesFromBlocks(clusterBlocks, 1)
            Dim observedStatistic As Double = statistic(observedIndices)
            ValidateFiniteScalar(observedStatistic, "observed statistic")

            Return ExecuteScalarBootstrap(observedStatistic,
                                          ResamplingBootstrap.ClusterBootstrapIndices(clusterBlocks, ctx.Info.ReplicatesRequested, ctx.Rng),
                                          statistic,
                                          ctx.Info,
                                          ResamplingCore.NormalizeBootstrapOptions(opts).MaxFailures,
                                          minimumSuccessfulReplicates,
                                          statisticLabel,
                                          progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered bootstrap for a vector statistic using raw cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <param name="statistic">Delegate that computes the parameter vector from a resampled index vector.</param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="parameterLabels">Optional labels for the returned parameter vector.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful bootstrap replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorClusterBootstrap(clusterIds As Object(),
                                                  statistic As Func(Of Integer(), Double()),
                                                  opts As BootstrapOptions,
                                                  Optional parameterLabels As String() = Nothing,
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult
            Dim blocks As List(Of Integer()) = ResamplingBootstrap.BuildClusterIndexBlocks(clusterIds)
            Return RunVectorClusterBootstrap(blocks, statistic, opts, parameterLabels, methodLabel, minimumSuccessfulReplicates, progressCallback)
        End Function

        ''' <summary>
        ''' Runs a clustered bootstrap for a vector statistic using precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <param name="statistic">Delegate that computes the parameter vector from a resampled index vector.</param>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, seed handling, and failure tolerance.</param>
        ''' <param name="parameterLabels">Optional labels for the returned parameter vector.</param>
        ''' <param name="methodLabel">Optional descriptive label recorded in <see cref="ResamplingRunInfo.MethodLabel"/>.</param>
        ''' <param name="minimumSuccessfulReplicates">Minimum number of successful bootstrap replicates required for the run to be accepted.</param>
        ''' <param name="progressCallback">Optional progress callback.</param>
        ''' <returns>A populated <see cref="VectorResamplingResult"/>.</returns>
        Public Function RunVectorClusterBootstrap(clusterBlocks As List(Of Integer()),
                                                  statistic As Func(Of Integer(), Double()),
                                                  opts As BootstrapOptions,
                                                  Optional parameterLabels As String() = Nothing,
                                                  Optional methodLabel As String = "",
                                                  Optional minimumSuccessfulReplicates As Integer = 1,
                                                  Optional progressCallback As Action(Of Integer, Integer) = Nothing) As VectorResamplingResult

            ValidateClusterBlocks(clusterBlocks)
            ValidateStatisticDelegate(statistic, NameOf(statistic))
            ValidateMinimumSuccessfulReplicates(minimumSuccessfulReplicates)

            Dim ctx = ResamplingBootstrap.CreateBootstrapContext(opts, methodLabel)
            Dim observedIndices As Integer() = BuildOriginalIndicesFromBlocks(clusterBlocks, 1)
            Dim observedVector As Double() = statistic(observedIndices)
            ValidateFiniteVector(observedVector, "observed statistic vector")
            ValidateParameterLabels(parameterLabels, observedVector.Length)

            Return ExecuteVectorBootstrap(observedVector,
                                          ResamplingBootstrap.ClusterBootstrapIndices(clusterBlocks, ctx.Info.ReplicatesRequested, ctx.Rng),
                                          statistic,
                                          ctx.Info,
                                          ResamplingCore.NormalizeBootstrapOptions(opts).MaxFailures,
                                          minimumSuccessfulReplicates,
                                          parameterLabels,
                                          progressCallback)
        End Function

        Private Function ExecuteScalarBootstrap(observedStatistic As Double,
                                                resamples As IEnumerable(Of Integer()),
                                                statistic As Func(Of Integer(), Double),
                                                info As ResamplingRunInfo,
                                                maxFailures As Integer,
                                                minimumSuccessfulReplicates As Integer,
                                                statisticLabel As String,
                                                progressCallback As Action(Of Integer, Integer)) As ScalarResamplingResult
            If resamples Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(resamples)))
            If info Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))

            Dim estimates As New List(Of Double)(Math.Max(1, info.ReplicatesRequested))
            Dim failed As Integer = 0
            Dim attempted As Integer = 0
            Dim firstFailure As Exception = Nothing

            For Each idx As Integer() In resamples
                attempted += 1
                Try
                    Dim value As Double = statistic(idx)
                    ValidateFiniteScalar(value, "bootstrap replicate statistic")
                    estimates.Add(value)
                Catch ex As Exception
                    failed += 1
                    If firstFailure Is Nothing Then firstFailure = ex
                    If failed > maxFailures Then
                        Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException($"Bootstrap aborted after {failed} failed replicates exceeded the allowed maximum of {maxFailures}.", firstFailure))
                    End If
                End Try

                ReportProgress(progressCallback, attempted, info.ReplicatesRequested)
            Next

            If estimates.Count < minimumSuccessfulReplicates Then
                Dim msg As String = $"Too few successful bootstrap replicates were obtained ({estimates.Count} < {minimumSuccessfulReplicates})."
                Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException(msg, firstFailure))
            End If

            ResamplingCore.CompleteRunInfo(info, estimates.Count, failed)
            If failed > 0 Then ResamplingCore.AppendNote(info, $"Failed/discarded resamples = {failed}.")

            Return New ScalarResamplingResult With {
                .StatisticLabel = If(statisticLabel, String.Empty),
                .ObservedStatistic = observedStatistic,
                .ResampledStatistics = estimates.ToArray(),
                .RunInfo = info
            }
        End Function

        Private Function ExecuteVectorBootstrap(observedVector As Double(),
                                                resamples As IEnumerable(Of Integer()),
                                                statistic As Func(Of Integer(), Double()),
                                                info As ResamplingRunInfo,
                                                maxFailures As Integer,
                                                minimumSuccessfulReplicates As Integer,
                                                parameterLabels As String(),
                                                progressCallback As Action(Of Integer, Integer)) As VectorResamplingResult
            If resamples Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(resamples)))
            If info Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))

            Dim estimates As New List(Of Double())(Math.Max(1, info.ReplicatesRequested))
            Dim failed As Integer = 0
            Dim attempted As Integer = 0
            Dim firstFailure As Exception = Nothing
            Dim parameterCount As Integer = observedVector.Length

            For Each idx As Integer() In resamples
                attempted += 1
                Try
                    Dim value As Double() = statistic(idx)
                    ValidateFiniteVector(value, "bootstrap replicate statistic vector")
                    If value.Length <> parameterCount Then
                        Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException($"Bootstrap replicate vector length {value.Length} does not match the observed parameter count {parameterCount}."))
                    End If
                    estimates.Add(DirectCast(value.Clone(), Double()))
                Catch ex As Exception
                    failed += 1
                    If firstFailure Is Nothing Then firstFailure = ex
                    If failed > maxFailures Then
                        Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException($"Bootstrap aborted after {failed} failed replicates exceeded the allowed maximum of {maxFailures}.", firstFailure))
                    End If
                End Try

                ReportProgress(progressCallback, attempted, info.ReplicatesRequested)
            Next

            If estimates.Count < minimumSuccessfulReplicates Then
                Dim msg As String = $"Too few successful bootstrap replicates were obtained ({estimates.Count} < {minimumSuccessfulReplicates})."
                Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException(msg, firstFailure))
            End If

            ResamplingCore.CompleteRunInfo(info, estimates.Count, failed)
            If failed > 0 Then ResamplingCore.AppendNote(info, $"Failed/discarded resamples = {failed}.")

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