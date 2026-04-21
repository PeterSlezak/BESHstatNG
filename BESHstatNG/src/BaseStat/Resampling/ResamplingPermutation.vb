Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Shared permutation and randomization helpers used by resampling-based inference methods.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module centralizes the common infrastructure needed for exact and Monte Carlo permutation tests
    ''' without embedding any method-specific statistic formulas. The intended usage pattern is:
    ''' </para>
    ''' <list type="number">
    '''   <item><description>the calling method computes the observed statistic on the original data,</description></item>
    '''   <item><description>this module generates the null-reference permutations or sign-flip patterns,</description></item>
    '''   <item><description>the calling method evaluates its statistic on each null sample, and</description></item>
    '''   <item><description><see cref="BuildPermutationResult(Double, Double(), PermutationOptions, String, ResamplingRunInfo)"/> converts the null distribution into a shared result payload.</description></item>
    ''' </list>
    ''' <para>
    ''' The design goal is to keep the resampling layer generic while leaving statistic-specific logic inside
    ''' the relevant analysis classes.
    ''' </para>
    ''' </remarks>
    Public Module ResamplingPermutation

        ''' <summary>
        ''' Creates a permutation run-info object together with a seeded random-number generator.
        ''' </summary>
        ''' <param name="opts">Permutation options controlling alpha, Monte Carlo replicate count, exact-enumeration limits, and seed handling.</param>
        ''' <param name="methodLabel">Optional descriptive label for the calling method.</param>
        ''' <returns>
        ''' A tuple containing the initialized <see cref="ResamplingRunInfo"/>, the <see cref="Random"/>
        ''' instance that should be used for Monte Carlo sampling, and the normalized permutation options.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is the preferred permutation entry point for method code because it guarantees that the RNG seed,
        ''' effective alpha, and requested replicate count are recorded in one consistent place.
        ''' </para>
        ''' <para>
        ''' The returned RNG is harmless for exact-enumeration workflows and is still supplied so the caller can use one uniform setup path.
        ''' </para>
        ''' </remarks>
        Public Function CreatePermutationContext(opts As PermutationOptions,
                                                 Optional methodLabel As String = "") As (Info As ResamplingRunInfo, Rng As Random, Options As PermutationOptions)
            Dim normalized As PermutationOptions = ResamplingCore.NormalizePermutationOptions(opts)
            Dim rngCtx = ResamplingCore.CreateRandomWithResolvedSeed(normalized.RandomSeed)
            Dim requested As Integer = If(normalized.Mode = PermutationMode.MonteCarlo, normalized.MonteCarloReplicates, 0)
            Dim info As ResamplingRunInfo = ResamplingCore.CreateRunInfo(methodLabel, requested,
                                                                         rngCtx.SeedUsed, normalized.Alpha)
            Return (info, rngCtx.Rng, normalized)
        End Function

        ''' <summary>
        ''' Returns <c>True</c> when an exact permutation enumeration is allowed under the supplied limit.
        ''' </summary>
        ''' <param name="expectedPermutations">Expected number of exact permutations or randomization patterns.</param>
        ''' <param name="opts">Permutation options controlling the maximum allowed exact enumeration size.</param>
        ''' <returns>
        ''' <c>True</c> if the expected exact workload does not exceed <see cref="PermutationOptions.MaxExactEnumerations"/>;
        ''' otherwise <c>False</c>.
        ''' </returns>
        Public Function CanEnumerateExactly(expectedPermutations As Long, opts As PermutationOptions) As Boolean
            If expectedPermutations < 0L Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(expectedPermutations), "The expected number of permutations must be non-negative."))
            End If
            Dim normalized As PermutationOptions = ResamplingCore.NormalizePermutationOptions(opts)
            Return expectedPermutations <= normalized.MaxExactEnumerations
        End Function

        ''' <summary>
        ''' Computes <c>n!</c> for small to moderate <paramref name="n"/> values.
        ''' </summary>
        ''' <param name="n">Non-negative integer.</param>
        ''' <returns>The factorial of <paramref name="n"/> as a <see cref="Long"/>.</returns>
        ''' <remarks>
        ''' <para>
        ''' This helper throws if the result would overflow <see cref="Long"/>.
        ''' </para>
        ''' </remarks>
        Public Function Factorial(n As Integer) As Long
            If n < 0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(n), "Factorial is undefined for negative integers."))
            End If

            Dim result As Long = 1L
            For i As Integer = 2 To n
                If result > Long.MaxValue \ i Then
                    AppGlobals.BSerr.LogAndThrow(New OverflowException($"Factorial({n}) exceeds the range of Int64."))
                End If
                result *= i
            Next
            Return result
        End Function

        ''' <summary>
        ''' Computes the number of unique permutations when ties are present.
        ''' </summary>
        ''' <param name="data">Numeric data whose repeated values induce duplicate permutations.</param>
        ''' <returns>
        ''' The count <c>n! / Π(freq_i!)</c> as a <see cref="Long"/>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This is useful when an exact permutation test wants to deduplicate tied permutations before statistic evaluation.
        ''' </para>
        ''' </remarks>
        Public Function ExpectedUniquePermutations(data As Double()) As Long
            If data Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(data)))
            If data.Length = 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least one value is required.", NameOf(data)))

            Dim freq As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For i As Integer = 0 To data.Length - 1
                If Double.IsNaN(data(i)) OrElse Double.IsInfinity(data(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Permutation values must be finite.", NameOf(data)))
                End If
                Dim key As String = data(i).ToString("G17", Globalization.CultureInfo.InvariantCulture)
                If freq.ContainsKey(key) Then
                    freq(key) += 1
                Else
                    freq.Add(key, 1)
                End If
            Next

            Dim denominator As Long = 1L
            For Each kvp As KeyValuePair(Of String, Integer) In freq
                Dim f As Long = Factorial(kvp.Value)
                If denominator > Long.MaxValue \ f Then
                    AppGlobals.BSerr.LogAndThrow(New OverflowException("The unique permutation denominator exceeds the range of Int64."))
                End If
                denominator *= f
            Next

            Return Factorial(data.Length) \ denominator
        End Function

        ''' <summary>
        ''' Returns <c>True</c> when the supplied numeric array contains tied values.
        ''' </summary>
        ''' <param name="data">Numeric data to inspect.</param>
        ''' <returns><c>True</c> if any value occurs more than once; otherwise <c>False</c>.</returns>
        Public Function HasTies(data As Double()) As Boolean
            If data Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(data)))
            If data.Length < 2 Then Return False

            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For i As Integer = 0 To data.Length - 1
                If Double.IsNaN(data(i)) OrElse Double.IsInfinity(data(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Permutation values must be finite.", NameOf(data)))
                End If
                Dim key As String = data(i).ToString("G17", Globalization.CultureInfo.InvariantCulture)
                If seen.Contains(key) Then Return True
                seen.Add(key)
            Next
            Return False
        End Function

        ''' <summary>
        ''' Creates a canonical string key for a numeric permutation.
        ''' </summary>
        ''' <param name="values">Permuted numeric values.</param>
        ''' <returns>A stable invariant-culture key suitable for tie-aware deduplication.</returns>
        Public Function GetPermutationKey(values As Double()) As String
            If values Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(values)))
            Dim sb As New StringBuilder(values.Length * 8)
            For i As Integer = 0 To values.Length - 1
                If Double.IsNaN(values(i)) OrElse Double.IsInfinity(values(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Permutation values must be finite.", NameOf(values)))
                End If
                sb.Append(values(i).ToString("G17", Globalization.CultureInfo.InvariantCulture))
                sb.Append(";"c)
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Draws one Monte Carlo permutation of the integers 0..n-1.
        ''' </summary>
        ''' <param name="sampleSize">Permutation size.</param>
        ''' <param name="rng">Random-number generator used to shuffle the indices.</param>
        ''' <returns>A new shuffled integer vector.</returns>
        Public Function DrawShuffledIndices(sampleSize As Integer, rng As Random) As Integer()
            ValidatePermutationSampleSize(sampleSize)
            If rng Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            Dim out(sampleSize - 1) As Integer
            For i As Integer = 0 To sampleSize - 1
                out(i) = i
            Next
            ShuffleInPlace(out, rng)
            Return out
        End Function

        ''' <summary>
        ''' Generates a sequence of Monte Carlo permutations of 0..n-1.
        ''' </summary>
        ''' <param name="sampleSize">Permutation size.</param>
        ''' <param name="replicates">Number of Monte Carlo permutations to generate.</param>
        ''' <param name="rng">Random-number generator used to shuffle the indices.</param>
        ''' <returns>An iterator over shuffled index vectors.</returns>
        Public Iterator Function MonteCarloPermutations(sampleSize As Integer,
                                                        replicates As Integer,
                                                        rng As Random) As IEnumerable(Of Integer())
            ValidatePermutationSampleSize(sampleSize)
            ValidatePositiveReplicates(replicates, NameOf(replicates), 1)
            If rng Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            For i As Integer = 1 To replicates
                Yield DrawShuffledIndices(sampleSize, rng)
            Next
        End Function

        ''' <summary>
        ''' Generates the full exact permutation sequence of 0..n-1 using Heap's algorithm.
        ''' </summary>
        ''' <param name="sampleSize">Permutation size.</param>
        ''' <returns>An iterator over all permutations of the indices 0..n-1.</returns>
        ''' <remarks>
        ''' <para>
        ''' The returned permutations are not deduplicated. If the downstream statistic is tie-sensitive and the permuted values contain ties,
        ''' use <see cref="GetPermutationKey(Double())"/> or <see cref="EnumerateUniqueValuePermutations(Double())"/> to avoid duplicate evaluations.
        ''' </para>
        ''' </remarks>
        Public Iterator Function EnumeratePermutations(sampleSize As Integer) As IEnumerable(Of Integer())
            ValidatePermutationSampleSize(sampleSize)

            Dim c(sampleSize - 1) As Integer
            Dim arr(sampleSize - 1) As Integer
            For i As Integer = 0 To sampleSize - 1
                arr(i) = i
            Next

            Yield DirectCast(arr.Clone(), Integer())

            Dim idx As Integer = 0
            While idx < sampleSize
                If c(idx) < idx Then
                    Dim j As Integer = If(idx Mod 2 = 0, 0, c(idx))
                    Dim tmp As Integer = arr(idx)
                    arr(idx) = arr(j)
                    arr(j) = tmp
                    Yield DirectCast(arr.Clone(), Integer())
                    c(idx) += 1
                    idx = 0
                Else
                    c(idx) = 0
                    idx += 1
                End If
            End While
        End Function

        ''' <summary>
        ''' Generates the full exact sequence of unique permutations of a numeric vector, deduplicating tied arrangements.
        ''' </summary>
        ''' <param name="values">Numeric values to permute.</param>
        ''' <returns>An iterator over unique value permutations.</returns>
        ''' <remarks>
        ''' <para>
        ''' This helper is especially useful for exact correlation tests where the response vector contains ties and duplicate permutations would otherwise repeat the same statistic value.
        ''' </para>
        ''' </remarks>
        Public Iterator Function EnumerateUniqueValuePermutations(values As Double()) As IEnumerable(Of Double())
            If values Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(values)))
            If values.Length = 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least one value is required.", NameOf(values)))
            For i As Integer = 0 To values.Length - 1
                If Double.IsNaN(values(i)) OrElse Double.IsInfinity(values(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Permutation values must be finite.", NameOf(values)))
                End If
            Next

            Dim sorted As Double() = DirectCast(values.Clone(), Double())
            Array.Sort(sorted)

            Do
                Yield DirectCast(sorted.Clone(), Double())
            Loop While NextLexicographicPermutation(sorted)
        End Function

        ''' <summary>
        ''' Draws one Monte Carlo sign-flip pattern for a paired/randomization test.
        ''' </summary>
        ''' <param name="sampleSize">Number of paired differences or signed observations.</param>
        ''' <param name="rng">Random-number generator used to draw the sign pattern.</param>
        ''' <returns>
        ''' An integer vector whose entries are either -1 or +1.
        ''' </returns>
        Public Function DrawSignFlipPattern(sampleSize As Integer, rng As Random) As Integer()
            ValidatePermutationSampleSize(sampleSize)
            If rng Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            Dim signs(sampleSize - 1) As Integer
            For i As Integer = 0 To sampleSize - 1
                signs(i) = If(rng.NextDouble() < 0.5, -1, 1)
            Next
            Return signs
        End Function

        ''' <summary>
        ''' Generates Monte Carlo sign-flip patterns for paired/randomization tests.
        ''' </summary>
        ''' <param name="sampleSize">Number of paired differences or signed observations.</param>
        ''' <param name="replicates">Number of Monte Carlo sign-flip patterns to generate.</param>
        ''' <param name="rng">Random-number generator used to draw the sign patterns.</param>
        ''' <returns>An iterator over sign-flip vectors.</returns>
        Public Iterator Function MonteCarloSignFlipPatterns(sampleSize As Integer,
                                                            replicates As Integer,
                                                            rng As Random) As IEnumerable(Of Integer())
            ValidatePermutationSampleSize(sampleSize)
            ValidatePositiveReplicates(replicates, NameOf(replicates), 1)
            If rng Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            For i As Integer = 1 To replicates
                Yield DrawSignFlipPattern(sampleSize, rng)
            Next
        End Function

        ''' <summary>
        ''' Generates the full exact set of sign-flip patterns for a paired/randomization test.
        ''' </summary>
        ''' <param name="sampleSize">Number of paired differences or signed observations.</param>
        ''' <returns>An iterator over all 2^n sign patterns.</returns>
        Public Iterator Function EnumerateSignFlipPatterns(sampleSize As Integer) As IEnumerable(Of Integer())
            ValidatePermutationSampleSize(sampleSize)
            If sampleSize >= 31 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(sampleSize), "Exact sign-flip enumeration is limited to sample sizes below 31 to avoid integer overflow."))
            End If

            Dim total As Integer = 1 << sampleSize
            For mask As Integer = 0 To total - 1
                Dim signs(sampleSize - 1) As Integer
                For i As Integer = 0 To sampleSize - 1
                    signs(i) = If((mask And (1 << i)) = 0, -1, 1)
                Next
                Yield signs
            Next
        End Function

        ''' <summary>
        ''' Builds a shared permutation-test result object from an observed statistic and its null distribution.
        ''' </summary>
        ''' <param name="observedStatistic">Observed test statistic computed on the original data.</param>
        ''' <param name="nullStatistics">Null-reference statistics computed on permuted or randomized data.</param>
        ''' <param name="opts">Permutation options controlling p-value interpretation.</param>
        ''' <param name="statisticLabel">Optional descriptive label for the tested statistic.</param>
        ''' <param name="runInfo">Optional run metadata. If <c>Nothing</c>, a default instance is created.</param>
        ''' <returns>A populated <see cref="PermutationResamplingResult"/>.</returns>
        Public Function BuildPermutationResult(observedStatistic As Double,
                                               nullStatistics As Double(),
                                               opts As PermutationOptions,
                                               Optional statisticLabel As String = "",
                                               Optional runInfo As ResamplingRunInfo = Nothing) As PermutationResamplingResult
            Dim normalized As PermutationOptions = ResamplingCore.NormalizePermutationOptions(opts)
            Dim info As ResamplingRunInfo = If(runInfo, New ResamplingRunInfo())
            If Double.IsNaN(info.AlphaUsed) Then info.AlphaUsed = normalized.Alpha

            ValidateFiniteStatistics(nullStatistics, NameOf(nullStatistics))
            Dim p = ResamplingResults.ComputeEmpiricalTailPValues(observedStatistic, nullStatistics,
                                                                  normalized.Alternative,
                                                                  normalized.UseAddOneCorrection)
            If info.ReplicatesUsed <= 0 Then info.ReplicatesUsed = nullStatistics.Length

            Return New PermutationResamplingResult With {
                .StatisticLabel = statisticLabel,
                .ObservedStatistic = observedStatistic,
                .NullStatistics = DirectCast(nullStatistics.Clone(), Double()),
                .LowerTailPValue = p.Lower,
                .UpperTailPValue = p.Upper,
                .TwoSidedPValue = p.TwoSided,
                .Alternative = normalized.Alternative,
                .RunInfo = info
            }
        End Function

        ''' <summary>
        ''' Randomly shuffles an integer vector in place using the Fisher–Yates algorithm.
        ''' </summary>
        ''' <param name="values">Vector to shuffle.</param>
        ''' <param name="rng">Random-number generator used to drive the shuffle.</param>
        Public Sub ShuffleInPlace(values As Integer(), rng As Random)
            If values Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(values)))
            If rng Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rng)))

            For i As Integer = values.Length - 1 To 1 Step -1
                Dim j As Integer = rng.Next(0, i + 1)
                Dim tmp As Integer = values(i)
                values(i) = values(j)
                values(j) = tmp
            Next
        End Sub

        ''' <summary>
        ''' Validates that a permutation sample size is positive.
        ''' </summary>
        ''' <param name="sampleSize">Permutation size to validate.</param>
        Public Sub ValidatePermutationSampleSize(sampleSize As Integer)
            ValidatePositiveReplicates(sampleSize, NameOf(sampleSize), 1)
        End Sub

        Private Function NextLexicographicPermutation(values As Double()) As Boolean
            Dim i As Integer = values.Length - 2
            While i >= 0 AndAlso values(i) >= values(i + 1)
                i -= 1
            End While
            If i < 0 Then Return False

            Dim j As Integer = values.Length - 1
            While values(j) <= values(i)
                j -= 1
            End While

            Dim tmp As Double = values(i)
            values(i) = values(j)
            values(j) = tmp

            Array.Reverse(values, i + 1, values.Length - (i + 1))
            Return True
        End Function

    End Module

End Namespace
