Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic

Namespace StatisticalProcessControl

    ''' <summary>Descriptive statistics for one rational subgroup.</summary>
    Public NotInheritable Class SpcSubgroupStatistics
        Public Sub New(count As Integer,
                       mean As Double,
                       minimum As Double,
                       maximum As Double,
                       range As Double,
                       sampleStandardDeviation As Double)
            If count < 1 Then Throw New ArgumentOutOfRangeException(NameOf(count))
            ValidateFinite(mean, NameOf(mean))
            ValidateFinite(minimum, NameOf(minimum))
            ValidateFinite(maximum, NameOf(maximum))
            ValidateFinite(range, NameOf(range))
            If maximum < minimum OrElse range < 0.0 Then
                Throw New ArgumentException("The subgroup extrema or range are inconsistent.")
            End If
            If count = 1 Then
                If Not Double.IsNaN(sampleStandardDeviation) Then
                    Throw New ArgumentException(
                        "A one-observation subgroup has no sample standard deviation.",
                        NameOf(sampleStandardDeviation))
                End If
            Else
                ValidateFinite(sampleStandardDeviation, NameOf(sampleStandardDeviation))
                If sampleStandardDeviation < 0.0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(sampleStandardDeviation))
                End If
            End If

            Me.Count = count
            Me.Mean = mean
            Me.Minimum = minimum
            Me.Maximum = maximum
            Me.Range = range
            Me.SampleStandardDeviation = sampleStandardDeviation
        End Sub

        Public ReadOnly Property Count As Integer
        Public ReadOnly Property Mean As Double
        Public ReadOnly Property Minimum As Double
        Public ReadOnly Property Maximum As Double
        Public ReadOnly Property Range As Double
        Public ReadOnly Property SampleStandardDeviation As Double

        Private Shared Sub ValidateFinite(value As Double, parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(parameterName, "The value must be finite.")
            End If
        End Sub
    End Class

    ''' <summary>Bias and Shewhart constants for a specified subgroup size.</summary>
    Public NotInheritable Class SpcControlChartConstants
        Friend Sub New(subgroupSize As Integer,
                       c4 As Double,
                       d2 As Double,
                       d3 As Double)
            Me.SubgroupSize = subgroupSize
            Me.C4 = c4
            Me.D2 = d2
            Me.D3 = d3

            A2 = 3.0 / (d2 * Math.Sqrt(CDbl(subgroupSize)))
            A3 = 3.0 / (c4 * Math.Sqrt(CDbl(subgroupSize)))
            B3 = Math.Max(0.0, 1.0 - 3.0 * Math.Sqrt(1.0 - c4 * c4) / c4)
            B4 = 1.0 + 3.0 * Math.Sqrt(1.0 - c4 * c4) / c4
            D3Limit = Math.Max(0.0, 1.0 - 3.0 * d3 / d2)
            D4Limit = 1.0 + 3.0 * d3 / d2
        End Sub

        Public ReadOnly Property SubgroupSize As Integer
        Public ReadOnly Property C4 As Double
        Public ReadOnly Property D2 As Double
        Public ReadOnly Property D3 As Double
        Public ReadOnly Property A2 As Double
        Public ReadOnly Property A3 As Double
        Public ReadOnly Property B3 As Double
        Public ReadOnly Property B4 As Double

        ''' <summary>Gets the lower R-chart multiplier, conventionally denoted D3.</summary>
        Public ReadOnly Property D3Limit As Double

        ''' <summary>Gets the upper R-chart multiplier, conventionally denoted D4.</summary>
        Public ReadOnly Property D4Limit As Double
    End Class

    ''' <summary>Result of a within-process standard-deviation estimate.</summary>
    Public NotInheritable Class SpcSigmaEstimate
        Public Sub New(value As Double,
                       estimator As SpcWithinSigmaEstimator,
                       method As String,
                       contributingPointCount As Integer)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(value))
            End If
            If Not [Enum].IsDefined(GetType(SpcWithinSigmaEstimator), estimator) OrElse
               estimator = SpcWithinSigmaEstimator.Automatic Then
                Throw New ArgumentOutOfRangeException(NameOf(estimator))
            End If
            If String.IsNullOrWhiteSpace(method) Then Throw New ArgumentException("A method name is required.", NameOf(method))
            If contributingPointCount < 1 Then Throw New ArgumentOutOfRangeException(NameOf(contributingPointCount))

            Me.Value = value
            Me.Estimator = estimator
            Me.Method = method.Trim()
            Me.ContributingPointCount = contributingPointCount
        End Sub

        Public ReadOnly Property Value As Double
        Public ReadOnly Property Estimator As SpcWithinSigmaEstimator
        Public ReadOnly Property Method As String
        Public ReadOnly Property ContributingPointCount As Integer
    End Class

    ''' <summary>
    ''' Shared, host-neutral numerical routines used by SPC chart calculators.
    ''' </summary>
    Public NotInheritable Class SpcStatistics
        Private Sub New()
        End Sub

        ' E(R) and SD(R) for samples from a standard normal distribution. Values
        ' through n=25 are the conventional independently tabulated SPC constants.
        Private Shared ReadOnly RangeConstants As Double(,) = {
            {2.0, 1.128, 0.853}, {3.0, 1.693, 0.888}, {4.0, 2.059, 0.880},
            {5.0, 2.326, 0.864}, {6.0, 2.534, 0.848}, {7.0, 2.704, 0.833},
            {8.0, 2.847, 0.820}, {9.0, 2.970, 0.808}, {10.0, 3.078, 0.797},
            {11.0, 3.173, 0.787}, {12.0, 3.258, 0.778}, {13.0, 3.336, 0.770},
            {14.0, 3.407, 0.763}, {15.0, 3.472, 0.756}, {16.0, 3.532, 0.750},
            {17.0, 3.588, 0.744}, {18.0, 3.640, 0.739}, {19.0, 3.689, 0.734},
            {20.0, 3.735, 0.729}, {21.0, 3.778, 0.724}, {22.0, 3.819, 0.720},
            {23.0, 3.858, 0.716}, {24.0, 3.895, 0.712}, {25.0, 3.931, 0.708}
        }

        ''' <summary>Calculates stable descriptive statistics, omitting NaN values when requested.</summary>
        Public Shared Function CalculateSubgroup(values As Double(),
                                                 Optional omitMissing As Boolean = False) As SpcSubgroupStatistics
            Dim clean As Double() = GetFiniteValues(values, omitMissing, NameOf(values))
            Dim count As Integer = clean.Length
            Dim mean As Double = clean.Average()
            Dim minimum As Double = clean(0)
            Dim maximum As Double = clean(0)
            Dim sumSquares As Double = 0.0

            For i As Integer = 0 To count - 1
                minimum = Math.Min(minimum, clean(i))
                maximum = Math.Max(maximum, clean(i))
                Dim delta As Double = clean(i) - mean
                sumSquares += delta * delta
            Next

            Dim sampleSd As Double = Double.NaN
            If count > 1 Then sampleSd = Math.Sqrt(Math.Max(0.0, sumSquares / CDbl(count - 1)))
            Return New SpcSubgroupStatistics(count, mean, minimum, maximum,
                                             maximum - minimum, sampleSd)
        End Function

        ''' <summary>Calculates statistics for each row of a wide subgroup matrix.</summary>
        Public Shared Function CalculateWideSubgroups(values As Double(,),
                                                      missingValuePolicy As SpcMissingValuePolicy) As SpcSubgroupStatistics()
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
            If Not [Enum].IsDefined(GetType(SpcMissingValuePolicy), missingValuePolicy) Then
                Throw New ArgumentOutOfRangeException(NameOf(missingValuePolicy))
            End If

            Dim rows As Integer = values.GetLength(0)
            Dim columns As Integer = values.GetLength(1)
            If rows = 0 OrElse columns = 0 Then Throw New ArgumentException("The subgroup matrix must not be empty.", NameOf(values))

            Dim results As New List(Of SpcSubgroupStatistics)(rows)
            For row As Integer = 0 To rows - 1
                Dim rowValues(columns - 1) As Double
                Dim hasMissing As Boolean = False
                For column As Integer = 0 To columns - 1
                    rowValues(column) = values(row, column)
                    If Double.IsNaN(rowValues(column)) Then hasMissing = True
                Next

                If hasMissing AndAlso missingValuePolicy = SpcMissingValuePolicy.OmitPoint Then Continue For
                results.Add(CalculateSubgroup(rowValues,
                    missingValuePolicy = SpcMissingValuePolicy.UseAvailableMeasurements))
            Next
            Return results.ToArray()
        End Function

        ''' <summary>Returns c4(n), the normal-theory bias correction for sample SD.</summary>
        Public Shared Function C4(subgroupSize As Integer) As Double
            If subgroupSize < 2 Then Throw New ArgumentOutOfRangeException(NameOf(subgroupSize), "The subgroup size must be at least two.")
            Dim n As Double = CDbl(subgroupSize)
            Dim logC4 As Double = 0.5 * Math.Log(2.0 / (n - 1.0)) +
                                  LogGamma(n / 2.0) - LogGamma((n - 1.0) / 2.0)
            Return Math.Exp(logC4)
        End Function

        ''' <summary>Returns the standard d2 and d3 range constants for n=2,...,25.</summary>
        Public Shared Function GetControlChartConstants(subgroupSize As Integer) As SpcControlChartConstants
            If subgroupSize < 2 OrElse subgroupSize > 25 Then
                Throw New ArgumentOutOfRangeException(NameOf(subgroupSize),
                    "Range-based constants are supported for subgroup sizes from 2 through 25.")
            End If
            Dim row As Integer = subgroupSize - 2
            Return New SpcControlChartConstants(subgroupSize, C4(subgroupSize),
                                                RangeConstants(row, 1), RangeConstants(row, 2))
        End Function

        ''' <summary>Calculates overlapping moving ranges of the requested length.</summary>
        Public Shared Function MovingRanges(values As Double(),
                                            Optional movingRangeLength As Integer = 2,
                                            Optional breakAtMissing As Boolean = True) As Double()
            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
            If movingRangeLength < 2 OrElse movingRangeLength > 25 Then
                Throw New ArgumentOutOfRangeException(NameOf(movingRangeLength))
            End If
            If values.Length < movingRangeLength Then Return Array.Empty(Of Double)()

            Dim ranges As New List(Of Double)()
            For last As Integer = movingRangeLength - 1 To values.Length - 1
                Dim minimum As Double = Double.PositiveInfinity
                Dim maximum As Double = Double.NegativeInfinity
                Dim valid As Boolean = True
                For i As Integer = last - movingRangeLength + 1 To last
                    Dim value As Double = values(i)
                    If Double.IsInfinity(value) Then Throw New ArgumentException("Values must not contain infinity.", NameOf(values))
                    If Double.IsNaN(value) Then
                        valid = False
                        Exit For
                    End If
                    minimum = Math.Min(minimum, value)
                    maximum = Math.Max(maximum, value)
                Next
                If valid Then
                    ranges.Add(maximum - minimum)
                ElseIf Not breakAtMissing Then
                    Continue For
                End If
            Next
            Return ranges.ToArray()
        End Function

        ''' <summary>Estimates within-process sigma from ordered individual observations.</summary>
        Public Shared Function EstimateSigmaFromIndividuals(values As Double(),
                                                            estimator As SpcWithinSigmaEstimator,
                                                            movingRangeLength As Integer,
                                                            useBiasCorrection As Boolean) As SpcSigmaEstimate
            Dim clean As Double() = GetFiniteValues(values, True, NameOf(values))
            Dim selected As SpcWithinSigmaEstimator = estimator
            If selected = SpcWithinSigmaEstimator.Automatic Then selected = SpcWithinSigmaEstimator.MovingRange

            Select Case selected
                Case SpcWithinSigmaEstimator.MovingRange, SpcWithinSigmaEstimator.MedianMovingRange
                    Dim ranges As Double() = MovingRanges(values, movingRangeLength)
                    If ranges.Length = 0 Then Throw New ArgumentException("At least one complete moving range is required.", NameOf(values))
                    Dim scale As Double = If(selected = SpcWithinSigmaEstimator.MedianMovingRange,
                                             Median(ranges), ranges.Average)
                    Dim divisor As Double = 1.0
                    If useBiasCorrection Then
                        If selected = SpcWithinSigmaEstimator.MedianMovingRange Then
                            If movingRangeLength <> 2 Then
                                Throw New ArgumentException(
                                    "The bias-corrected median moving-range estimator requires a moving-range length of two.",
                                    NameOf(movingRangeLength))
                            End If
                            ' median(|X2-X1|) for independent normal observations
                            divisor = Math.Sqrt(2.0) * 0.6744897501960817
                        Else
                            divisor = GetControlChartConstants(movingRangeLength).D2
                        End If
                    End If
                    Dim name As String = If(selected = SpcWithinSigmaEstimator.MedianMovingRange,
                                            "Median moving range", "Average moving range")
                    Return New SpcSigmaEstimate(scale / divisor, selected, name, ranges.Length)

                Case SpcWithinSigmaEstimator.SampleStandardDeviation
                    Dim stats As SpcSubgroupStatistics = CalculateSubgroup(clean)
                    If stats.Count < 2 Then Throw New ArgumentException("At least two observations are required.", NameOf(values))
                    Dim divisor As Double = If(useBiasCorrection, C4(stats.Count), 1.0)
                    Return New SpcSigmaEstimate(stats.SampleStandardDeviation / divisor,
                                                selected, "Sample standard deviation", stats.Count)

                Case SpcWithinSigmaEstimator.MedianAbsoluteDeviation
                    Dim mad As Double = MedianAbsoluteDeviation(clean)
                    Dim divisor As Double = If(useBiasCorrection, 0.6744897501960817, 1.0)
                    Return New SpcSigmaEstimate(mad / divisor, selected,
                                                "Median absolute deviation", clean.Length)
                Case Else
                    Throw New ArgumentException("The selected estimator is not applicable to individual observations.", NameOf(estimator))
            End Select
        End Function

        ''' <summary>Estimates within-process sigma from rational-subgroup summaries.</summary>
        Public Shared Function EstimateSigmaFromSubgroups(subgroups As SpcSubgroupStatistics(),
                                                         estimator As SpcWithinSigmaEstimator,
                                                         useBiasCorrection As Boolean) As SpcSigmaEstimate
            If subgroups Is Nothing Then Throw New ArgumentNullException(NameOf(subgroups))
            If subgroups.Length = 0 Then Throw New ArgumentException("At least one subgroup is required.", NameOf(subgroups))
            For Each subgroup As SpcSubgroupStatistics In subgroups
                If subgroup Is Nothing Then Throw New ArgumentException("Subgroups must not contain null entries.", NameOf(subgroups))
            Next

            Dim selected As SpcWithinSigmaEstimator = estimator
            If selected = SpcWithinSigmaEstimator.Automatic Then selected = SpcWithinSigmaEstimator.AverageRange

            Select Case selected
                Case SpcWithinSigmaEstimator.AverageRange
                    Dim weightedTotal As Double = 0.0
                    Dim weight As Double = 0.0
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        If subgroup.Count < 2 Then Continue For
                        Dim divisor As Double = If(useBiasCorrection, GetControlChartConstants(subgroup.Count).D2, 1.0)
                        weightedTotal += subgroup.Range / divisor
                        weight += 1.0
                    Next
                    Return MakeSigmaEstimate(weightedTotal, weight, selected, "Average range", subgroups.Length)

                Case SpcWithinSigmaEstimator.AverageStandardDeviation
                    Dim total As Double = 0.0
                    Dim count As Integer = 0
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        If subgroup.Count < 2 Then Continue For
                        total += subgroup.SampleStandardDeviation /
                                 If(useBiasCorrection, C4(subgroup.Count), 1.0)
                        count += 1
                    Next
                    Return MakeSigmaEstimate(total, CDbl(count), selected,
                                             "Average subgroup standard deviation", count)

                Case SpcWithinSigmaEstimator.PooledStandardDeviation
                    Dim sumDegrees As Integer = 0
                    Dim sumSquares As Double = 0.0
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        If subgroup.Count < 2 Then Continue For
                        Dim degrees As Integer = subgroup.Count - 1
                        sumDegrees += degrees
                        sumSquares += CDbl(degrees) * subgroup.SampleStandardDeviation * subgroup.SampleStandardDeviation
                    Next
                    If sumDegrees < 1 Then Throw New ArgumentException("No subgroup has at least two observations.", NameOf(subgroups))
                    Dim sigma As Double = Math.Sqrt(sumSquares / CDbl(sumDegrees))
                    If useBiasCorrection Then sigma /= C4(sumDegrees + 1)
                    Return New SpcSigmaEstimate(sigma, selected, "Pooled subgroup standard deviation", subgroups.Length)
                Case Else
                    Throw New ArgumentException("The selected estimator is not applicable to rational subgroups.", NameOf(estimator))
            End Select
        End Function

        ''' <summary>Returns the median without modifying the supplied array.</summary>
        Public Shared Function Median(values As Double()) As Double
            Dim clean As Double() = GetFiniteValues(values, False, NameOf(values))
            Return StatFunc.Median(clean)
        End Function

        ''' <summary>Returns the median absolute deviation about the sample median.</summary>
        Public Shared Function MedianAbsoluteDeviation(values As Double()) As Double
            Dim clean As Double() = GetFiniteValues(values, False, NameOf(values))
            Dim center As Double = Median(clean)
            Dim deviations(clean.Length - 1) As Double
            For i As Integer = 0 To clean.Length - 1
                deviations(i) = Math.Abs(clean(i) - center)
            Next
            Return Median(deviations)
        End Function

        Private Shared Function MakeSigmaEstimate(total As Double,
                                                  count As Double,
                                                  estimator As SpcWithinSigmaEstimator,
                                                  method As String,
                                                  contributingCount As Integer) As SpcSigmaEstimate
            If count <= 0.0 Then Throw New ArgumentException("No eligible subgroup statistics are available.")
            Return New SpcSigmaEstimate(total / count, estimator, method, contributingCount)
        End Function

        Private Shared Function GetFiniteValues(values As Double(),
                                                omitNaN As Boolean,
                                                parameterName As String) As Double()
            If values Is Nothing Then Throw New ArgumentNullException(parameterName)
            Dim result As New List(Of Double)(values.Length)
            For Each value As Double In values
                If Double.IsInfinity(value) Then Throw New ArgumentException("Values must not contain infinity.", parameterName)
                If Double.IsNaN(value) Then
                    If omitNaN Then Continue For
                    Throw New ArgumentException("Values must not contain missing values.", parameterName)
                End If
                result.Add(value)
            Next
            If result.Count = 0 Then Throw New ArgumentException("At least one finite value is required.", parameterName)
            Return result.ToArray()
        End Function

    End Class

End Namespace
