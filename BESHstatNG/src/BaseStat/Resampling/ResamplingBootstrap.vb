Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Shared bootstrap sampling helpers used by method-specific statistics code.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is intentionally focused on generating index vectors and simple resampled arrays rather than
    ''' computing method-specific statistics. The design goal is to keep the resampling infrastructure generic while
    ''' leaving the statistic formulas inside the relevant analysis classes.
    ''' </para>
    ''' <para>
    ''' The public API therefore exposes:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>ordinary row-level bootstrap sampling</description></item>
    '''   <item><description>cluster bootstrap sampling that resamples whole subjects/clusters with replacement</description></item>
    '''   <item><description>simple projection helpers that materialize arrays from an index vector</description></item>
    ''' </list>
    ''' <para>
    ''' These helpers are intended to be used by Bland–Altman, Lin concordance, weighted kappa, weighted Deming,
    ''' and later by additional resampling-enabled methods across the project.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingBootstrap

        ''' <summary>
        ''' Creates a bootstrap run-info object together with a seeded random-number generator.
        ''' </summary>
        ''' <param name="opts">Bootstrap options controlling alpha, replicate count, and seed handling.</param>
        ''' <param name="methodLabel">Optional descriptive label for the calling method.</param>
        ''' <returns>
        ''' A tuple containing the initialized <see cref="ResamplingRunInfo"/> and the <see cref="Random"/>
        ''' instance that should be used for the bootstrap run.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is the preferred bootstrap entry point for method code because it guarantees that the RNG seed,
        ''' resolved alpha, and requested replicate count are recorded in one consistent place.
        ''' </para>
        ''' </remarks>
        Public Function CreateBootstrapContext(opts As BootstrapOptions,
                                               Optional methodLabel As String = "") As (Info As ResamplingRunInfo, Rng As Random)
            Dim normalized As BootstrapOptions = ResamplingCore.NormalizeBootstrapOptions(opts)
            Dim rngCtx = ResamplingCore.CreateRandomWithResolvedSeed(normalized.RandomSeed)
            Dim info As ResamplingRunInfo = ResamplingCore.CreateRunInfo(methodLabel, normalized.Replicates,
                                                                         rngCtx.SeedUsed, normalized.Alpha)
            Return (info, rngCtx.Rng)
        End Function

        ''' <summary>
        ''' Draws one ordinary bootstrap resample of row indices.
        ''' </summary>
        ''' <param name="sampleSize">Number of original observations.</param>
        ''' <param name="rng">Random-number generator used to draw the resample.</param>
        ''' <returns>
        ''' An integer array of length <paramref name="sampleSize"/> whose elements are sampled with replacement
        ''' from the closed interval [0, <paramref name="sampleSize"/> - 1].
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The returned array can be fed directly into method-specific statistics code, or used with
        ''' <see cref="TakeByIndices(Of T)(T(), Integer())"/> to materialize the resampled observations.
        ''' </para>
        ''' </remarks>
        Public Function DrawBootstrapIndices(sampleSize As Integer,
                                             rng As Random) As Integer()
            ValidateSampleSize(sampleSize)
            If rng Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            Dim indices(sampleSize - 1) As Integer
            For i As Integer = 0 To sampleSize - 1
                indices(i) = rng.Next(0, sampleSize)
            Next
            Return indices
        End Function

        ''' <summary>
        ''' Generates a sequence of ordinary bootstrap index resamples.
        ''' </summary>
        ''' <param name="sampleSize">Number of original observations.</param>
        ''' <param name="replicates">Number of bootstrap resamples to generate.</param>
        ''' <param name="rng">Random-number generator used to draw the resamples.</param>
        ''' <returns>
        ''' An iterator over bootstrap index vectors.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This iterator performs no method-specific error handling. If a particular bootstrap replicate leads to a
        ''' singular fit or an otherwise invalid statistic, the calling method should catch the exception and decide
        ''' whether to skip that replicate or abort the run.
        ''' </para>
        ''' </remarks>
        Public Iterator Function BootstrapIndices(sampleSize As Integer,
                                                  replicates As Integer,
                                                  rng As Random) As IEnumerable(Of Integer())
            ValidateSampleSize(sampleSize)
            ValidatePositiveReplicates(replicates, NameOf(replicates), 1)
            If rng Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            For rep As Integer = 1 To replicates
                Yield DrawBootstrapIndices(sampleSize, rng)
            Next
        End Function

        ''' <summary>
        ''' Builds cluster-membership blocks from a vector of cluster identifiers.
        ''' </summary>
        ''' <param name="clusterIds">
        ''' Cluster labels aligned row-by-row with the underlying observations.
        ''' </param>
        ''' <returns>
        ''' A list of integer arrays, one array per distinct cluster, preserving the first-seen order of the clusters
        ''' and the original within-cluster row order.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is useful for clustered bootstrap and repeated-measures methods. Every distinct non-missing
        ''' cluster identifier contributes one block of observation indices.
        ''' </para>
        ''' <para>
        ''' Cluster identifiers are normalized to stable string keys using invariant-culture formatting for numeric
        ''' values and trimmed text for string values.
        ''' </para>
        ''' </remarks>
        Public Function BuildClusterIndexBlocks(clusterIds As Object()) As List(Of Integer())
            If clusterIds Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(clusterIds)))
            If clusterIds.Length = 0 Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("At least one cluster identifier is required.", NameOf(clusterIds)))

            Dim order As New List(Of String)()
            Dim blocks As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)

            For i As Integer = 0 To clusterIds.Length - 1
                Dim key As String = NormalizeClusterKey(clusterIds(i), NameOf(clusterIds))
                If Not blocks.ContainsKey(key) Then
                    blocks.Add(key, New List(Of Integer)())
                    order.Add(key)
                End If
                blocks(key).Add(i)
            Next

            Dim result As New List(Of Integer())(order.Count)
            For Each key As String In order
                result.Add(blocks(key).ToArray())
            Next
            Return result
        End Function

        ''' <summary>
        ''' Draws one clustered bootstrap resample that resamples whole clusters with replacement.
        ''' </summary>
        ''' <param name="clusterIds">
        ''' Cluster labels aligned row-by-row with the original observations.
        ''' </param>
        ''' <param name="rng">Random-number generator used to draw the resample.</param>
        ''' <returns>
        ''' An index vector representing one clustered bootstrap sample.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' If there are <c>G</c> distinct clusters, this method samples <c>G</c> clusters with replacement.
        ''' Every time a cluster is selected, all rows belonging to that cluster are appended to the resampled index
        ''' vector in their original within-cluster order.
        ''' </para>
        ''' <para>
        ''' The resulting vector may therefore be longer than the number of clusters and, in general, equal in length
        ''' to the sum of the sizes of the sampled clusters.
        ''' </para>
        ''' </remarks>
        Public Function DrawClusterBootstrapIndices(clusterIds As Object(),
                                                    rng As Random) As Integer()
            If rng Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            Dim blocks As List(Of Integer()) = BuildClusterIndexBlocks(clusterIds)
            Return DrawClusterBootstrapIndices(blocks, rng)
        End Function

        ''' <summary>
        ''' Draws one clustered bootstrap resample from precomputed cluster-membership blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">
        ''' Precomputed cluster-membership blocks as returned by <see cref="BuildClusterIndexBlocks(Object())"/>.
        ''' </param>
        ''' <param name="rng">Random-number generator used to draw the resample.</param>
        ''' <returns>
        ''' An index vector representing one clustered bootstrap sample.
        ''' </returns>
        Public Function DrawClusterBootstrapIndices(clusterBlocks As List(Of Integer()),
                                                    rng As Random) As Integer()
            ValidateClusterBlocks(clusterBlocks)
            If rng Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            Dim out As New List(Of Integer)()
            For draw As Integer = 1 To clusterBlocks.Count
                Dim blockIndex As Integer = rng.Next(0, clusterBlocks.Count)
                Dim block As Integer() = clusterBlocks(blockIndex)
                If block Is Nothing OrElse block.Length = 0 Then
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New InvalidOperationException("Cluster blocks must not contain empty blocks."))
                End If
                out.AddRange(block)
            Next
            Return out.ToArray()
        End Function

        ''' <summary>
        ''' Generates a sequence of clustered bootstrap index resamples.
        ''' </summary>
        ''' <param name="clusterIds">Cluster labels aligned row-by-row with the original observations.</param>
        ''' <param name="replicates">Number of clustered bootstrap resamples to generate.</param>
        ''' <param name="rng">Random-number generator used to draw the resamples.</param>
        ''' <returns>
        ''' An iterator over clustered bootstrap index vectors.
        ''' </returns>
        Public Iterator Function ClusterBootstrapIndices(clusterIds As Object(),
                                                         replicates As Integer,
                                                         rng As Random) As IEnumerable(Of Integer())
            Dim blocks As List(Of Integer()) = BuildClusterIndexBlocks(clusterIds)
            For Each sample As Integer() In ClusterBootstrapIndices(blocks, replicates, rng)
                Yield sample
            Next
        End Function

        ''' <summary>
        ''' Generates a sequence of clustered bootstrap index resamples from precomputed cluster blocks.
        ''' </summary>
        ''' <param name="clusterBlocks">Precomputed cluster-membership blocks.</param>
        ''' <param name="replicates">Number of clustered bootstrap resamples to generate.</param>
        ''' <param name="rng">Random-number generator used to draw the resamples.</param>
        ''' <returns>
        ''' An iterator over clustered bootstrap index vectors.
        ''' </returns>
        Public Iterator Function ClusterBootstrapIndices(clusterBlocks As List(Of Integer()),
                                                         replicates As Integer,
                                                         rng As Random) As IEnumerable(Of Integer())
            ValidateClusterBlocks(clusterBlocks)
            ValidatePositiveReplicates(replicates, NameOf(replicates), 1)
            If rng Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            For rep As Integer = 1 To replicates
                Yield DrawClusterBootstrapIndices(clusterBlocks, rng)
            Next
        End Function

        ''' <summary>
        ''' Projects an input array onto a supplied index vector.
        ''' </summary>
        ''' <typeparam name="T">Element type of the source array.</typeparam>
        ''' <param name="values">Source array to resample.</param>
        ''' <param name="indices">Index vector describing the desired rows.</param>
        ''' <returns>
        ''' A new array whose elements are <paramref name="values"/> looked up in the order specified by
        ''' <paramref name="indices"/>.
        ''' </returns>
        ''' <remarks>
        ''' This helper is useful after <see cref="DrawBootstrapIndices(Integer, Random)"/> or
        ''' <see cref="DrawClusterBootstrapIndices(Object(), Random)"/> has been called.
        ''' </remarks>
        Public Function TakeByIndices(Of T)(values As T(),
                                            indices As Integer()) As T()
            If values Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(values)))
            If indices Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(indices)))

            Dim out(indices.Length - 1) As T
            For i As Integer = 0 To indices.Length - 1
                Dim idx As Integer = indices(i)
                If idx < 0 OrElse idx >= values.Length Then
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(indices), $"Index {idx} is outside the valid range 0 to {values.Length - 1}."))
                End If
                out(i) = values(idx)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Projects two aligned arrays onto a shared index vector.
        ''' </summary>
        ''' <typeparam name="T1">Element type of the first source array.</typeparam>
        ''' <typeparam name="T2">Element type of the second source array.</typeparam>
        ''' <param name="values1">First source array.</param>
        ''' <param name="values2">Second source array.</param>
        ''' <param name="indices">Index vector describing the desired rows.</param>
        ''' <returns>
        ''' A tuple containing the resampled first array and the resampled second array.
        ''' </returns>
        ''' <remarks>
        ''' This overload is especially convenient for paired resampling workflows where two aligned vectors must be
        ''' resampled together.
        ''' </remarks>
        Public Function TakeByIndices(Of T1, T2)(values1 As T1(),
                                                 values2 As T2(),
                                                 indices As Integer()) As (Values1 As T1(), Values2 As T2())
            If values1 Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(values1)))
            If values2 Is Nothing Then Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(values2)))
            If values1.Length <> values2.Length Then
                Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("The aligned source arrays must have the same length."))
            End If

            Return (TakeByIndices(values1, indices), TakeByIndices(values2, indices))
        End Function

        ''' <summary>
        ''' Validates that the supplied sample size is suitable for ordinary bootstrap sampling.
        ''' </summary>
        ''' <param name="sampleSize">Sample size to validate.</param>
        Public Sub ValidateSampleSize(sampleSize As Integer)
            ValidatePositiveReplicates(sampleSize, NameOf(sampleSize), 1)
        End Sub

        Private Function NormalizeClusterKey(value As Object,
                                             paramName As String) As String
            If value Is Nothing OrElse Convert.IsDBNull(value) Then
                Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("Cluster identifiers must not contain missing values.", paramName))
            End If

            If TypeOf value Is String Then
                Dim s As String = CStr(value).Trim()
                If s.Length = 0 Then
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("Cluster identifiers must not contain blank strings.", paramName))
                End If
                Return s
            End If

            If TypeOf value Is Double Then
                Dim d As Double = CDbl(value)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("Cluster identifiers must be finite.", paramName))
                End If
                Return d.ToString("R", CultureInfo.InvariantCulture)
            End If

            If TypeOf value Is Single Then
                Dim sng As Single = CSng(value)
                If Single.IsNaN(sng) OrElse Single.IsInfinity(sng) Then
                    Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("Cluster identifiers must be finite.", paramName))
                End If
                Return sng.ToString("R", CultureInfo.InvariantCulture)
            End If

            If TypeOf value Is IFormattable Then
                Return DirectCast(value, IFormattable).ToString(Nothing, CultureInfo.InvariantCulture)
            End If

            Dim text As String = Convert.ToString(value, CultureInfo.InvariantCulture)
            If String.IsNullOrWhiteSpace(text) Then
                Global.BESHStatNG.AppInfrastructure.CoreServices.Errors.LogAndThrow(New ArgumentException("Cluster identifiers must not normalize to an empty key.", paramName))
            End If
            Return text.Trim()
        End Function

    End Module

End Namespace