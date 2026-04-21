Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Shared jackknife helpers used by method-specific statistics code.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module provides generic leave-one-out index generation and a small set of scalar jackknife summary
    ''' utilities. The design mirrors <c>ResamplingBootstrap</c>: the infrastructure is responsible for generating
    ''' deterministic resampling index sets, while the calling statistical method remains responsible for computing
    ''' the actual statistic from those indices.
    ''' </para>
    ''' <para>
    ''' The public API therefore exposes:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>ordinary leave-one-out row-index generation</description></item>
    '''   <item><description>cluster leave-one-out index generation for repeated-measures or grouped data</description></item>
    '''   <item><description>jackknife pseudo-values, bias, and standard-error helpers for scalar statistics</description></item>
    ''' </list>
    ''' <para>
    ''' These helpers are intended to be used by Bland–Altman, weighted Deming, and other methods that currently
    ''' carry their own explicit leave-one-out loops.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingJackknife

        ''' <summary>
        ''' Creates a jackknife run-info object for a deterministic leave-one-out run.
        ''' </summary>
        ''' <param name="replicates">
        ''' Number of leave-one-out replicates expected for the run. For ordinary jackknife this is typically the
        ''' sample size; for cluster jackknife it is the number of clusters.
        ''' </param>
        ''' <param name="opts">Jackknife options controlling alpha handling.</param>
        ''' <param name="methodLabel">Optional descriptive label for the calling method.</param>
        ''' <returns>
        ''' A <see cref="ResamplingRunInfo"/> object initialized with the resolved alpha, replicate count, and a
        ''' deterministic “no RNG used” seed marker.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Jackknife is deterministic, so no pseudo-random generator is created. The run metadata therefore stores
        ''' <see cref="Integer.MinValue"/> in <see cref="ResamplingRunInfo.SeedUsed"/> and records that no RNG was used.
        ''' </para>
        ''' </remarks>
        Public Function CreateJackknifeContext(replicates As Integer,
                                               opts As JackknifeOptions,
                                               Optional methodLabel As String = "") As ResamplingRunInfo
            ValidatePositiveReplicates(replicates, NameOf(replicates), 1)
            Dim normalized As JackknifeOptions = ResamplingCore.NormalizeJackknifeOptions(opts)
            Dim info As ResamplingRunInfo = ResamplingCore.CreateRunInfo(methodLabel, replicates, Integer.MinValue, normalized.Alpha)
            ResamplingCore.AppendNote(info, "Jackknife is deterministic; no random-number generator was used.")
            Return info
        End Function

        ''' <summary>
        ''' Builds one ordinary leave-one-out index vector.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <param name="excludedIndex">Zero-based row index to exclude from the resample.</param>
        ''' <returns>
        ''' An integer array containing all row indices from <c>0</c> to <c>sampleSize - 1</c> except
        ''' <paramref name="excludedIndex"/>.
        ''' </returns>
        ''' <remarks>
        ''' This helper is useful when a method wants a single specific leave-one-out replicate rather than an
        ''' iterator over all such replicates.
        ''' </remarks>
        Public Function DrawLeaveOneOutIndices(sampleSize As Integer, excludedIndex As Integer) As Integer()
            ValidateLeaveOneOutSampleSize(sampleSize)
            If excludedIndex < 0 OrElse excludedIndex >= sampleSize Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(excludedIndex), $"excludedIndex must be between 0 and {sampleSize - 1}."))
            End If

            Dim out(sampleSize - 2) As Integer
            Dim t As Integer = 0
            For i As Integer = 0 To sampleSize - 1
                If i = excludedIndex Then Continue For
                out(t) = i
                t += 1
            Next
            Return out
        End Function

        ''' <summary>
        ''' Generates the full sequence of ordinary leave-one-out index vectors.
        ''' </summary>
        ''' <param name="sampleSize">Number of observations in the original sample.</param>
        ''' <returns>
        ''' An iterator over all leave-one-out index vectors in ascending order of the excluded row.
        ''' </returns>
        Public Iterator Function LeaveOneOutIndices(sampleSize As Integer) As IEnumerable(Of Integer())
            ValidateLeaveOneOutSampleSize(sampleSize)
            For excluded As Integer = 0 To sampleSize - 1
                Yield DrawLeaveOneOutIndices(sampleSize, excluded)
            Next
        End Function

        ''' <summary>
        ''' Builds one cluster leave-one-out index vector from precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">
        ''' Precomputed cluster-membership blocks as returned by <see cref="ResamplingBootstrap.BuildClusterIndexBlocks(Object())"/>.
        ''' </param>
        ''' <param name="excludedClusterIndex">Zero-based index of the cluster block to exclude.</param>
        ''' <returns>
        ''' An integer array containing the indices from all clusters except the excluded cluster.
        ''' </returns>
        Public Function DrawClusterLeaveOneOutIndices(clusterBlocks As List(Of Integer()), excludedClusterIndex As Integer) As Integer()
            ValidateClusterBlocks(clusterBlocks, 2)

            Dim out As New List(Of Integer)()
            For i As Integer = 0 To clusterBlocks.Count - 1
                If i = excludedClusterIndex Then Continue For
                Dim block As Integer() = clusterBlocks(i)
                If block Is Nothing OrElse block.Length = 0 Then
                    AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Cluster blocks must not contain empty blocks."))
                End If
                out.AddRange(block)
            Next
            Return out.ToArray()
        End Function

        ''' <summary>
        ''' Generates the full sequence of cluster leave-one-out index vectors from raw cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <returns>
        ''' An iterator over cluster leave-one-out index vectors.
        ''' </returns>
        Public Iterator Function ClusterLeaveOneOutIndices(clusterIds As Object()) As IEnumerable(Of Integer())
            Dim blocks As List(Of Integer()) = ResamplingBootstrap.BuildClusterIndexBlocks(clusterIds)
            For Each sample As Integer() In ClusterLeaveOneOutIndices(blocks)
                Yield sample
            Next
        End Function

        ''' <summary>
        ''' Generates the full sequence of cluster leave-one-out index vectors from precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <returns>
        ''' An iterator over cluster leave-one-out index vectors.
        ''' </returns>
        Public Iterator Function ClusterLeaveOneOutIndices(clusterBlocks As List(Of Integer())) As IEnumerable(Of Integer())
            ValidateClusterBlocks(clusterBlocks, 2)

            For excluded As Integer = 0 To clusterBlocks.Count - 1
                Yield DrawClusterLeaveOneOutIndices(clusterBlocks, excluded)
            Next
        End Function

        ''' <summary>
        ''' Computes jackknife pseudo-values for a scalar statistic.
        ''' </summary>
        ''' <param name="observedStatistic">Statistic computed on the full original sample.</param>
        ''' <param name="leaveOneOutEstimates">Leave-one-out estimates of the same statistic.</param>
        ''' <returns>
        ''' An array of pseudo-values defined by <c>n * theta_hat - (n - 1) * theta_(i)</c>.
        ''' </returns>
        ''' <remarks>
        ''' Pseudo-values are useful when reproducing legacy Linnet-style or other jackknife-based analytical summaries.
        ''' </remarks>
        Public Function JackknifePseudoValues(observedStatistic As Double, leaveOneOutEstimates As Double()) As Double()
            ValidateLeaveOneOutEstimates(leaveOneOutEstimates, NameOf(leaveOneOutEstimates))

            Dim n As Integer = leaveOneOutEstimates.Length
            Dim out(n - 1) As Double
            For i As Integer = 0 To n - 1
                out(i) = n * observedStatistic - (n - 1) * leaveOneOutEstimates(i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the ordinary jackknife standard error for a scalar statistic from leave-one-out estimates.
        ''' </summary>
        ''' <param name="leaveOneOutEstimates">Leave-one-out estimates of the statistic.</param>
        ''' <returns>
        ''' The standard jackknife standard error
        ''' <c>sqrt(((n-1)/n) * sum((theta_(i) - mean(theta_.))^2))</c>.
        ''' </returns>
        Public Function JackknifeStandardError(leaveOneOutEstimates As Double()) As Double
            ValidateLeaveOneOutEstimates(leaveOneOutEstimates, NameOf(leaveOneOutEstimates))

            Dim n As Integer = leaveOneOutEstimates.Length
            Dim meanTheta As Double = 0.0
            For i As Integer = 0 To n - 1
                meanTheta += leaveOneOutEstimates(i)
            Next
            meanTheta /= n

            Dim ss As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim d As Double = leaveOneOutEstimates(i) - meanTheta
                ss += d * d
            Next
            Return Math.Sqrt(((n - 1.0) / n) * ss)
        End Function

        ''' <summary>
        ''' Computes the ordinary jackknife bias estimate for a scalar statistic.
        ''' </summary>
        ''' <param name="observedStatistic">Statistic computed on the full original sample.</param>
        ''' <param name="leaveOneOutEstimates">Leave-one-out estimates of the same statistic.</param>
        ''' <returns>
        ''' The ordinary jackknife bias estimate <c>(n - 1) * (mean(theta_.) - theta_hat)</c>.
        ''' </returns>
        Public Function JackknifeBias(observedStatistic As Double, leaveOneOutEstimates As Double()) As Double
            ValidateLeaveOneOutEstimates(leaveOneOutEstimates, NameOf(leaveOneOutEstimates))

            Dim n As Integer = leaveOneOutEstimates.Length
            Dim meanTheta As Double = 0.0
            For i As Integer = 0 To n - 1
                meanTheta += leaveOneOutEstimates(i)
            Next
            meanTheta /= n
            Return (n - 1.0) * (meanTheta - observedStatistic)
        End Function

        ''' <summary>
        ''' Validates that a sample size is large enough for ordinary leave-one-out jackknife generation.
        ''' </summary>
        ''' <param name="sampleSize">Sample size to validate.</param>
        Public Sub ValidateLeaveOneOutSampleSize(sampleSize As Integer)
            ValidatePositiveReplicates(sampleSize, NameOf(sampleSize), 2)
        End Sub

    End Module

End Namespace
