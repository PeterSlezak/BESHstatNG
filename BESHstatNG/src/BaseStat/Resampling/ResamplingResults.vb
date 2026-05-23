Option Explicit On

Imports System
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Stores the observed statistic, bootstrap/jackknife replicate statistics, and run metadata for a scalar resampling analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This container is intended for methods whose resampling target is a single numeric statistic, such as a concordance
    ''' coefficient, a weighted kappa coefficient, a Bland–Altman bias, or a single regression parameter.
    ''' </para>
    ''' <para>
    ''' The class does not prescribe how confidence intervals are constructed. Instead, it stores the observed value and
    ''' the successful replicate values, and provides helper methods to compute common summaries and to convert selected
    ''' limits into the project's existing <see cref="Global.BESHStatNG.ConfidenceIntervalResult"/> type.
    ''' </para>
    ''' </remarks>
    Public Class ScalarResamplingResult

        ''' <summary>
        ''' Gets or sets a short descriptive label for the statistic represented by this result.
        ''' </summary>
        Public Property StatisticLabel As String = String.Empty

        ''' <summary>
        ''' Gets or sets the statistic computed on the original, unresampled data.
        ''' </summary>
        Public Property ObservedStatistic As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the successful replicate statistics.
        ''' </summary>
        ''' <remarks>
        ''' The engine should store only successful, finite replicate values in this array. Failed replicates should be counted in
        ''' <see cref="RunInfo"/> rather than represented by <c>NaN</c> entries.
        ''' </remarks>
        Public Property ResampledStatistics As Double() = Nothing

        ''' <summary>
        ''' Gets or sets metadata describing how the resampling run was executed.
        ''' </summary>
        Public Property RunInfo As ResamplingRunInfo = New ResamplingRunInfo()

        ''' <summary>
        ''' Gets the number of successful replicate statistics stored in <see cref="ResampledStatistics"/>.
        ''' </summary>
        Public ReadOnly Property ReplicateCount As Integer
            Get
                If ResampledStatistics Is Nothing Then Return 0
                Return ResampledStatistics.Length
            End Get
        End Property

        ''' <summary>
        ''' Returns a defensive copy of the stored replicate statistics.
        ''' </summary>
        ''' <returns>
        ''' A new array containing the successful replicate statistics, or an empty array if no replicate values are present.
        ''' </returns>
        Public Function CloneResampledStatistics() As Double()
            If ResampledStatistics Is Nothing Then Return Array.Empty(Of Double)()
            Return DirectCast(ResampledStatistics.Clone(), Double())
        End Function

        ''' <summary>
        ''' Computes the mean of the successful replicate statistics.
        ''' </summary>
        ''' <returns>The arithmetic mean of the replicate statistics, or <see cref="Double.NaN"/> if no replicate values are present.</returns>
        Public Function MeanResampledStatistic() As Double
            If ResampledStatistics Is Nothing OrElse ResampledStatistics.Length = 0 Then Return Double.NaN
            ValidateFiniteStatistics(ResampledStatistics, NameOf(ResampledStatistics))
            Return ResampledStatistics.Average()
        End Function

        ''' <summary>
        ''' Computes the standard deviation of the successful replicate statistics.
        ''' </summary>
        ''' <param name="useSampleStandardDeviation">
        ''' If <c>True</c>, the sample standard deviation with denominator <c>n-1</c> is used; otherwise the population standard deviation is used.
        ''' </param>
        ''' <returns>
        ''' The requested standard deviation of the replicate statistics, or <see cref="Double.NaN"/> if insufficient replicate values are present.
        ''' </returns>
        Public Function StdDevResampledStatistic(Optional useSampleStandardDeviation As Boolean = True) As Double
            If ResampledStatistics Is Nothing OrElse ResampledStatistics.Length = 0 Then Return Double.NaN
            ValidateFiniteStatistics(ResampledStatistics, NameOf(ResampledStatistics))
            Return StatFunc.stDev(ResampledStatistics, useSampleStandardDeviation)
        End Function

        ''' <summary>
        ''' Returns a sorted copy of the successful replicate statistics.
        ''' </summary>
        ''' <returns>A new ascending array of replicate statistics.</returns>
        Public Function SortedResampledStatistics() As Double()
            Dim out As Double() = CloneResampledStatistics()
            If out.Length > 1 Then Array.Sort(out)
            Return out
        End Function

        ''' <summary>
        ''' Evaluates an empirical percentile from the replicate statistics.
        ''' </summary>
        ''' <param name="probability">Requested percentile probability in the closed interval [0,1].</param>
        ''' <returns>The interpolated percentile of the replicate statistics.</returns>
        Public Function Percentile(probability As Double) As Double
            ValidateProbability(probability, NameOf(probability))
            Dim sorted As Double() = SortedResampledStatistics()
            If sorted.Length = 0 Then Return Double.NaN
            Return QuantileSorted(sorted, probability)
        End Function

        ''' <summary>
        ''' Converts the supplied lower and upper limits into the project's standard confidence-interval result type.
        ''' </summary>
        ''' <param name="alpha">Two-sided significance level associated with the interval.</param>
        ''' <param name="lowerLimit">Lower limit of the interval.</param>
        ''' <param name="upperLimit">Upper limit of the interval.</param>
        ''' <param name="stdErr">Optional standard error associated with the interval estimate.</param>
        ''' <returns>A populated <see cref="Global.BESHStatNG.ConfidenceIntervalResult"/> instance.</returns>
        Public Function ToConfidenceIntervalResult(alpha As Double,
                                                   lowerLimit As Double,
                                                   upperLimit As Double,
                                                   Optional stdErr As Double = Double.NaN) As Global.BESHStatNG.ConfidenceIntervalResult
            ValidateAlpha(alpha)
            If Double.IsNaN(ObservedStatistic) OrElse Double.IsInfinity(ObservedStatistic) Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("ObservedStatistic must be finite before a confidence interval can be created."))
            End If

            Dim ci As New Global.BESHStatNG.ConfidenceIntervalResult With {
                .Estimate = ObservedStatistic,
                .alpha = alpha,
                .StdErr = stdErr,
                .LowerLimit = lowerLimit,
                .UpperLimit = upperLimit
            }
            Return ci
        End Function

        ''' <summary>
        ''' Builds a percentile-based confidence interval directly from the stored replicate statistics.
        ''' </summary>
        ''' <param name="alpha">Two-sided significance level associated with the interval.</param>
        ''' <returns>A percentile-based <see cref="Global.BESHStatNG.ConfidenceIntervalResult"/>.</returns>
        Public Function ToPercentileConfidenceInterval(alpha As Double) As Global.BESHStatNG.ConfidenceIntervalResult
            ValidateAlpha(alpha)
            Dim sorted As Double() = SortedResampledStatistics()
            If sorted.Length = 0 Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("At least one resampled statistic is required to build a percentile confidence interval."))
            End If

            Dim lower As Double = QuantileSorted(sorted, alpha / 2.0)
            Dim upper As Double = QuantileSorted(sorted, 1.0 - alpha / 2.0)
            Return ToConfidenceIntervalResult(alpha, lower, upper, StdDevResampledStatistic())
        End Function

        Public Function ToBcaConfidenceInterval(alpha As Double, jackknifeStatistics As Double()) As Global.BESHStatNG.ConfidenceIntervalResult
            ValidateAlpha(alpha)
            Dim sorted As Double() = SortedResampledStatistics()
            If sorted.Length = 0 Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("At least one resampled statistic is required to build a BCa confidence interval."))
            End If

            ValidateFiniteStatistics(jackknifeStatistics, NameOf(jackknifeStatistics))
            If jackknifeStatistics.Length < 2 Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("At least two jackknife replicate statistics are required to build a BCa confidence interval."))
            End If

            Dim z0 As Double = ComputeBcaBiasCorrectionZ0(ObservedStatistic, sorted)
            Dim acceleration As Double = ComputeBcaAcceleration(jackknifeStatistics)
            Dim probs = ComputeBcaAdjustedProbabilities(alpha, z0, acceleration, sorted.Length)

            Dim lower As Double = QuantileSorted(sorted, probs.Lower)
            Dim upper As Double = QuantileSorted(sorted, probs.Upper)
            Return ToConfidenceIntervalResult(alpha, lower, upper, StdDevResampledStatistic())
        End Function
    End Class

    ''' <summary>
    ''' Stores observed parameter values, replicate parameter values, and run metadata for a multi-parameter resampling analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This container is intended for methods whose resampling target is a vector of parameters, such as an intercept/slope pair
    ''' in regression or a bias/lower-LoA/upper-LoA trio in Bland–Altman analysis.
    ''' </para>
    ''' <para>
    ''' Replicate vectors are stored row-wise: each element of <see cref="ResampledStatistics"/> is one successful resample and must
    ''' have the same length as <see cref="ObservedStatistics"/>.
    ''' </para>
    ''' </remarks>
    Public Class VectorResamplingResult

        ''' <summary>
        ''' Gets or sets optional labels for the parameters stored in <see cref="ObservedStatistics"/>.
        ''' </summary>
        Public Property ParameterLabels As String() = Nothing

        ''' <summary>
        ''' Gets or sets the observed parameter vector computed on the original, unresampled data.
        ''' </summary>
        Public Property ObservedStatistics As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the successful replicate parameter vectors.
        ''' </summary>
        Public Property ResampledStatistics As Double()() = Nothing

        ''' <summary>
        ''' Gets or sets metadata describing how the resampling run was executed.
        ''' </summary>
        Public Property RunInfo As ResamplingRunInfo = New ResamplingRunInfo()

        ''' <summary>
        ''' Gets the number of parameters in the observed vector.
        ''' </summary>
        Public ReadOnly Property ParameterCount As Integer
            Get
                If ObservedStatistics Is Nothing Then Return 0
                Return ObservedStatistics.Length
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of successful replicate parameter vectors.
        ''' </summary>
        Public ReadOnly Property ReplicateCount As Integer
            Get
                If ResampledStatistics Is Nothing Then Return 0
                Return ResampledStatistics.Length
            End Get
        End Property

        ''' <summary>
        ''' Returns a defensive copy of the observed parameter vector.
        ''' </summary>
        Public Function CloneObservedStatistics() As Double()
            If ObservedStatistics Is Nothing Then Return Array.Empty(Of Double)()
            Return DirectCast(ObservedStatistics.Clone(), Double())
        End Function

        ''' <summary>
        ''' Returns a defensive copy of the replicate parameter matrix.
        ''' </summary>
        Public Function CloneResampledStatistics() As Double()()
            Return CloneJaggedMatrix(ResampledStatistics)
        End Function

        ''' <summary>
        ''' Extracts the successful replicate values for a single parameter.
        ''' </summary>
        ''' <param name="parameterIndex">Zero-based parameter index.</param>
        ''' <returns>A new array containing the replicate values for the requested parameter.</returns>
        Public Function GetParameterReplicates(parameterIndex As Integer) As Double()
            ValidateVectorShape(ObservedStatistics, ResampledStatistics, ParameterLabels)
            If parameterIndex < 0 OrElse parameterIndex >= ObservedStatistics.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(parameterIndex), $"parameterIndex must be between 0 and {ObservedStatistics.Length - 1}."))
            End If

            Dim out(ResampledStatistics.Length - 1) As Double
            For i As Integer = 0 To ResampledStatistics.Length - 1
                out(i) = ResampledStatistics(i)(parameterIndex)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Projects one parameter from the vector result into a scalar resampling result.
        ''' </summary>
        ''' <param name="parameterIndex">Zero-based parameter index.</param>
        ''' <returns>A <see cref="ScalarResamplingResult"/> representing the selected parameter.</returns>
        Public Function ToScalar(parameterIndex As Integer) As ScalarResamplingResult
            ValidateVectorShape(ObservedStatistics, ResampledStatistics, ParameterLabels)
            Dim label As String = String.Empty
            If ParameterLabels IsNot Nothing AndAlso parameterIndex >= 0 AndAlso parameterIndex < ParameterLabels.Length Then
                label = ParameterLabels(parameterIndex)
            End If

            Return New ScalarResamplingResult With {
                .StatisticLabel = label,
                .ObservedStatistic = ObservedStatistics(parameterIndex),
                .ResampledStatistics = GetParameterReplicates(parameterIndex),
                .RunInfo = RunInfo
            }
        End Function

    End Class

    ''' <summary>
    ''' Stores the observed test statistic, permutation/null statistics, empirical p-values, and run metadata for a resampling-based hypothesis test.
    ''' </summary>
    Public Class PermutationResamplingResult

        ''' <summary>
        ''' Gets or sets a short descriptive label for the tested statistic.
        ''' </summary>
        Public Property StatisticLabel As String = String.Empty

        ''' <summary>
        ''' Gets or sets the observed statistic computed on the original, unpermuted data.
        ''' </summary>
        Public Property ObservedStatistic As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the permutation or randomization reference distribution.
        ''' </summary>
        Public Property NullStatistics As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the lower-tail empirical p-value.
        ''' </summary>
        Public Property LowerTailPValue As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the upper-tail empirical p-value.
        ''' </summary>
        Public Property UpperTailPValue As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the two-sided empirical p-value.
        ''' </summary>
        Public Property TwoSidedPValue As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the alternative hypothesis used to interpret the permutation results.
        ''' </summary>
        Public Property Alternative As AlternativeHypothesis = AlternativeHypothesis.TwoSided

        ''' <summary>
        ''' Gets or sets metadata describing how the resampling run was executed.
        ''' </summary>
        Public Property RunInfo As ResamplingRunInfo = New ResamplingRunInfo()

        ''' <summary>
        ''' Gets the number of permutation/null statistics stored in <see cref="NullStatistics"/>.
        ''' </summary>
        Public ReadOnly Property ReplicateCount As Integer
            Get
                If NullStatistics Is Nothing Then Return 0
                Return NullStatistics.Length
            End Get
        End Property

        ''' <summary>
        ''' Converts this permutation result to the project's standard <see cref="Global.BESHStatNG.TestResult"/> type.
        ''' </summary>
        ''' <returns>A populated <see cref="Global.BESHStatNG.TestResult"/> instance.</returns>
        Public Function ToTestResult() As Global.BESHStatNG.TestResult
            Dim tr As New Global.BESHStatNG.TestResult With {
                .Pvalue = TwoSidedPValue,
                .PvalueLowerSide = LowerTailPValue,
                .PvalueUpperSide = UpperTailPValue,
                .TestStatistics1 = ObservedStatistic,
                .strSpecialInformation = BuildSpecialInformation()
            }
            Return tr
        End Function

        Private Function BuildSpecialInformation() As String
            Dim parts As New List(Of String)
            If Not String.IsNullOrWhiteSpace(StatisticLabel) Then parts.Add($"Statistic = {StatisticLabel}")
            If RunInfo IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(RunInfo.MethodLabel) Then parts.Add($"Resampling method = {RunInfo.MethodLabel}")
            If RunInfo IsNot Nothing Then
                If RunInfo.ReplicatesUsed > 0 Then parts.Add($"Replicates used = {RunInfo.ReplicatesUsed}")
                If RunInfo.SeedUsed <> Integer.MinValue Then parts.Add($"Seed used = {RunInfo.SeedUsed}")
            End If
            Return String.Join("; ", parts)
        End Function

    End Class

    ''' <summary>
    ''' Shared helpers for validating, summarizing, and converting resampling result payloads.
    ''' </summary>
    Public Module ResamplingResults

        ''' <summary>
        ''' Evaluates an interpolated empirical quantile from an ascending sorted numeric vector.
        ''' </summary>
        ''' <param name="sortedValues">Ascending sorted values.</param>
        ''' <param name="probability">Requested quantile probability in the closed interval [0,1].</param>
        ''' <returns>The interpolated empirical quantile.</returns>
        Public Function QuantileSorted(sortedValues As Double(), probability As Double) As Double
            ValidateFiniteStatistics(sortedValues, NameOf(sortedValues))
            ValidateProbability(probability, NameOf(probability))

            If sortedValues.Length = 1 Then Return sortedValues(0)
            If probability <= 0.0 Then Return sortedValues(0)
            If probability >= 1.0 Then Return sortedValues(sortedValues.Length - 1)

            Dim h As Double = (sortedValues.Length - 1) * probability
            Dim lo As Integer = CInt(Math.Floor(h))
            Dim hi As Integer = CInt(Math.Ceiling(h))
            If lo = hi Then Return sortedValues(lo)

            Dim frac As Double = h - lo
            Return sortedValues(lo) + frac * (sortedValues(hi) - sortedValues(lo))
        End Function

        Friend Function ComputeBcaBiasCorrectionZ0(observedStatistic As Double, bootstrapStatistics As Double()) As Double
            If Double.IsNaN(observedStatistic) OrElse Double.IsInfinity(observedStatistic) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Observed statistic must be finite.", NameOf(observedStatistic)))
            End If
            ValidateFiniteStatistics(bootstrapStatistics, NameOf(bootstrapStatistics))

            Dim lessCount As Integer = 0
            Dim equalCount As Integer = 0
            For i As Integer = 0 To bootstrapStatistics.Length - 1
                If bootstrapStatistics(i) < observedStatistic Then
                    lessCount += 1
                ElseIf bootstrapStatistics(i) = observedStatistic Then
                    equalCount += 1
                End If
            Next

            Dim p As Double = (lessCount + 0.5 * equalCount) / bootstrapStatistics.Length
            p = ClampOpenProbability(p, bootstrapStatistics.Length)
            Return Global.BESHStatNG.distributions.NormSInv(p)
        End Function

        Friend Function ComputeBcaAcceleration(jackknifeStatistics As Double()) As Double
            ValidateFiniteStatistics(jackknifeStatistics, NameOf(jackknifeStatistics))
            If jackknifeStatistics.Length < 2 Then Return 0.0

            Dim thetaDot As Double = jackknifeStatistics.Average()
            Dim sumSq As Double = 0.0
            Dim sumCube As Double = 0.0
            For i As Integer = 0 To jackknifeStatistics.Length - 1
                Dim u As Double = thetaDot - jackknifeStatistics(i)
                sumSq += u * u
                sumCube += u * u * u
            Next

            If sumSq <= 0.0 Then Return 0.0
            Dim denom As Double = 6.0 * Math.Pow(sumSq, 1.5)
            If denom = 0.0 OrElse Double.IsNaN(denom) OrElse Double.IsInfinity(denom) Then Return 0.0
            Return sumCube / denom
        End Function

        Friend Function ComputeBcaAdjustedProbabilities(alpha As Double, z0 As Double, acceleration As Double, bootstrapCount As Integer) As (Lower As Double, Upper As Double)
            ValidateAlpha(alpha)
            If bootstrapCount < 1 Then
                CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(bootstrapCount), "bootstrapCount must be positive."))
            End If

            Dim zLower As Double = Global.BESHStatNG.distributions.NormSInv(alpha / 2.0)
            Dim zUpper As Double = Global.BESHStatNG.distributions.NormSInv(1.0 - alpha / 2.0)

            Dim lowerProb As Double = ComputeBcaAdjustedProbability(z0, acceleration, zLower, bootstrapCount)
            Dim upperProb As Double = ComputeBcaAdjustedProbability(z0, acceleration, zUpper, bootstrapCount)

            If lowerProb > upperProb Then
                Dim tmp As Double = lowerProb
                lowerProb = upperProb
                upperProb = tmp
            End If

            Return (lowerProb, upperProb)
        End Function

        Friend Function ComputeBcaAdjustedProbability(z0 As Double, acceleration As Double, zAlpha As Double, bootstrapCount As Integer) As Double
            Dim denom As Double = 1.0 - acceleration * (z0 + zAlpha)
            Dim adjustedZ As Double
            If Math.Abs(denom) < 0.000000000001 Then
                adjustedZ = If((z0 + zAlpha) >= 0.0, Double.PositiveInfinity, Double.NegativeInfinity)
            Else
                adjustedZ = z0 + (z0 + zAlpha) / denom
            End If

            Dim p As Double
            If Double.IsPositiveInfinity(adjustedZ) Then
                p = 1.0
            ElseIf Double.IsNegativeInfinity(adjustedZ) Then
                p = 0.0
            Else
                p = Global.BESHStatNG.distributions.PNorm(adjustedZ)
            End If

            Return ClampOpenProbability(p, bootstrapCount)
        End Function

        Friend Function ClampOpenProbability(probability As Double, sampleSize As Integer) As Double
            If sampleSize <= 0 Then Return probability
            Dim eps As Double = 0.5 / sampleSize
            If Double.IsNaN(probability) Then Return eps
            If probability <= 0.0 Then Return eps
            If probability >= 1.0 Then Return 1.0 - eps
            Return Math.Min(1.0 - eps, Math.Max(eps, probability))
        End Function

        ''' <summary>
        ''' Computes empirical lower-tail, upper-tail, and two-sided p-values from a resampling null distribution.
        ''' </summary>
        ''' <param name="observedStatistic">Observed test statistic.</param>
        ''' <param name="nullStatistics">Permutation/bootstrap null statistics.</param>
        ''' <param name="alternative">Alternative hypothesis used to interpret the null distribution.</param>
        ''' <param name="useAddOneCorrection">
        ''' If <c>True</c>, the common add-one correction is applied to the empirical counts.
        ''' </param>
        ''' <returns>
        ''' A tuple containing the lower-tail p-value, upper-tail p-value, and two-sided p-value.
        ''' </returns>
        Public Function ComputeEmpiricalTailPValues(observedStatistic As Double,
                                                    nullStatistics As Double(),
                                                    alternative As AlternativeHypothesis,
                                                    Optional useAddOneCorrection As Boolean = True) As (Lower As Double, Upper As Double, TwoSided As Double)
            If Double.IsNaN(observedStatistic) OrElse Double.IsInfinity(observedStatistic) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Observed statistic must be finite.", NameOf(observedStatistic)))
            End If
            ValidateFiniteStatistics(nullStatistics, NameOf(nullStatistics))

            Dim lowerCount As Integer = 0
            Dim upperCount As Integer = 0
            For i As Integer = 0 To nullStatistics.Length - 1
                If nullStatistics(i) <= observedStatistic Then lowerCount += 1
                If nullStatistics(i) >= observedStatistic Then upperCount += 1
            Next

            Dim n As Integer = nullStatistics.Length
            Dim add As Integer = If(useAddOneCorrection, 1, 0)
            Dim denom As Double = n + If(useAddOneCorrection, 1, 0)

            Dim pLower As Double = (lowerCount + add) / denom
            Dim pUpper As Double = (upperCount + add) / denom
            Dim pTwoSided As Double = Math.Min(1.0, 2.0 * Math.Min(pLower, pUpper))

            Select Case alternative
                Case AlternativeHypothesis.Less
                    pTwoSided = pLower
                Case AlternativeHypothesis.Greater
                    pTwoSided = pUpper
            End Select

            Return (pLower, pUpper, pTwoSided)
        End Function

        ''' <summary>
        ''' Creates a deep clone of a jagged matrix of doubles.
        ''' </summary>
        ''' <param name="matrix">Matrix to clone.</param>
        ''' <returns>A deep-cloned jagged matrix, or <c>Nothing</c> if the input is <c>Nothing</c>.</returns>
        Public Function CloneJaggedMatrix(matrix As Double()()) As Double()()
            If matrix Is Nothing Then Return Nothing
            Dim out(matrix.Length - 1)() As Double
            For i As Integer = 0 To matrix.Length - 1
                If matrix(i) Is Nothing Then
                    out(i) = Nothing
                Else
                    out(i) = DirectCast(matrix(i).Clone(), Double())
                End If
            Next
            Return out
        End Function

        ''' <summary>
        ''' Validates the shape of a vector resampling result.
        ''' </summary>
        ''' <param name="observed">Observed parameter vector.</param>
        ''' <param name="replicates">Replicate parameter vectors.</param>
        ''' <param name="parameterLabels">Optional parameter labels.</param>
        Friend Sub ValidateVectorShape(observed As Double(), replicates As Double()(), parameterLabels As String())
            If observed Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(observed)))
            If observed.Length = 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("At least one observed parameter is required.", NameOf(observed)))
            ValidateFiniteStatistics(observed, NameOf(observed))

            If parameterLabels IsNot Nothing AndAlso parameterLabels.Length > 0 AndAlso parameterLabels.Length <> observed.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("ParameterLabels must be Nothing or have the same length as ObservedStatistics.", NameOf(parameterLabels)))
            End If

            If replicates Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(replicates)))
            If replicates.Length = 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("At least one replicate vector is required.", NameOf(replicates)))

            For i As Integer = 0 To replicates.Length - 1
                If replicates(i) Is Nothing Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException("Replicate vectors must not be Nothing.", NameOf(replicates)))
                End If
                If replicates(i).Length <> observed.Length Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException("Each replicate vector must have the same length as ObservedStatistics.", NameOf(replicates)))
                End If
                ValidateFiniteStatistics(replicates(i), NameOf(replicates))
            Next
        End Sub

    End Module

End Namespace
