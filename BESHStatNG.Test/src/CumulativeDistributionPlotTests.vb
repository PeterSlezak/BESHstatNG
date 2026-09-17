Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class CumulativeDistributionPlotTests

    Private Const TightTolerance As Double = 1.0E-10R
    Private Const FitTolerance As Double = 1.0E-6R

    <TestMethod>
    <TestCategory("CDF-ECDF")>
    Public Sub ExactEcdf_CollapsesTiesAndBuildsRightContinuousStep()
        Dim values As Double() = {4.0R, 1.0R, 1.0R, 2.0R}
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
            .YScale = CumulativeDistributionYScale.Percent,
            .EmpiricalMethod = CumulativeDistributionEmpiricalMethod.ExactEcdf
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(values, "Sample", options)

        CollectionAssert.AreEqual(New Double() {1.0R, 1.0R, 2.0R, 4.0R}, result.SortedData)
        CollectionAssert.AreEqual(New Double() {1.0R, 1.0R, 2.0R, 2.0R, 4.0R, 4.0R},
                                  result.EmpiricalX)
        AssertArrayClose(New Double() {0.0R, 50.0R, 50.0R, 75.0R, 75.0R, 100.0R},
                         result.EmpiricalY)

        Assert.AreEqual(4, result.SourceCount)
        Assert.AreEqual(4, result.ValidCount)
        Assert.AreEqual(0, result.OmittedCount)
        Assert.AreEqual("Sample", result.SeriesName)
        Assert.IsNull(result.Fit)
        Assert.AreEqual(0, result.FittedX.Length)
        Assert.AreEqual(0, result.FittedY.Length)
    End Sub

    <TestMethod>
    <TestCategory("CDF-ECDF")>
    Public Sub ExactEcdf_ProbabilityScaleRunsFromZeroToOne()
        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                New Double() {10.0R, 20.0R, 30.0R, 40.0R},
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
                    .YScale = CumulativeDistributionYScale.Probability
                })

        AssertClose(0.0R, result.EmpiricalY.First())
        AssertClose(1.0R, result.EmpiricalY.Last())
        Assert.IsTrue(result.EmpiricalY.All(Function(y) y >= 0.0R AndAlso y <= 1.0R))
    End Sub

    <TestMethod>
    <TestCategory("CDF-ECDF")>
    Public Sub MinitabMedianRank_UsesDocumentedPlottingPositions()
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
            .YScale = CumulativeDistributionYScale.Probability,
            .EmpiricalMethod = CumulativeDistributionEmpiricalMethod.MinitabMedianRank
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(New Double() {1.0R, 2.0R, 3.0R, 4.0R},
                                               options:=options)

        Dim denominator As Double = 4.4R
        Dim expected As Double() = {
            0.0R, 0.7R / denominator,
            0.7R / denominator, 1.7R / denominator,
            1.7R / denominator, 2.7R / denominator,
            2.7R / denominator, 3.7R / denominator
        }

        AssertArrayClose(expected, result.EmpiricalY)
        Assert.IsTrue(result.EmpiricalY.Last() < 1.0R,
                      "Median-rank plotting positions should not force the final point to 1.")
    End Sub

    <TestMethod>
    <TestCategory("CDF-Percentiles")>
    Public Sub Percentiles_UseGeneralizedInverseEcdfAndAreSortedDeduplicated()
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
            .YScale = CumulativeDistributionYScale.Percent,
            .Percentiles = New Double() {75.0R, 25.0R, 50.0R, 25.0R}
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                New Double() {8.0R, 1.0R, 7.0R, 2.0R, 6.0R, 3.0R, 5.0R, 4.0R},
                options:=options)

        Dim q As CumulativeDistributionPercentile() = result.Percentiles
        Assert.AreEqual(3, q.Length)

        AssertClose(25.0R, q(0).Percent)
        AssertClose(2.0R, q(0).XValue)
        AssertClose(25.0R, q(0).YValue)

        AssertClose(50.0R, q(1).Percent)
        AssertClose(4.0R, q(1).XValue)
        AssertClose(50.0R, q(1).YValue)

        AssertClose(75.0R, q(2).Percent)
        AssertClose(6.0R, q(2).XValue)
        AssertClose(75.0R, q(2).YValue)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Input")>
    Public Sub InvalidRows_AreRejectedByDefaultAndCanBeOmitted()
        Dim values As Object() = {1.0R, Nothing, "not numeric", Double.NaN, 2.0R}

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                CumulativeDistributionPlot.Compute(
                    values,
                    options:=New CumulativeDistributionPlotOptions With {
                        .Mode = CumulativeDistributionPlotMode.EmpiricalOnly
                    })
            End Sub)

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                values,
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
                    .OmitInvalidRows = True
                })

        Assert.AreEqual(5, result.SourceCount)
        Assert.AreEqual(2, result.ValidCount)
        Assert.AreEqual(3, result.OmittedCount)
        CollectionAssert.AreEqual(New Double() {1.0R, 2.0R}, result.SortedData)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Input")>
    Public Sub InputShapeAndOptions_AreValidated()
        Dim matrix As Double(,) = {{1.0R, 2.0R}, {3.0R, 4.0R}}
        Assert.ThrowsException(Of ArgumentException)(
            Sub() CumulativeDistributionPlot.Compute(matrix))

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                CumulativeDistributionPlot.Compute(
                    New Double() {1.0R, 2.0R},
                    options:=New CumulativeDistributionPlotOptions With {
                        .FittedCurvePointCount = 49
                    })
            End Sub)

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                CumulativeDistributionPlot.Compute(
                    New Double() {1.0R, 2.0R},
                    options:=New CumulativeDistributionPlotOptions With {
                        .Percentiles = New Double() {0.0R}
                    })
            End Sub)

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                CumulativeDistributionPlot.Compute(
                    New Double() {1.0R, 2.0R},
                    options:=New CumulativeDistributionPlotOptions With {
                        .XMinimum = 10.0R,
                        .XMaximum = 5.0R
                    })
            End Sub)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Grouped")>
    Public Sub ComputeGrouped_PreservesFirstAppearanceOrderAndNames()
        Dim values As Double() = {20.0R, 10.0R, 21.0R, 30.0R, 11.0R}
        Dim groups As String() = {"B", "A", "B", "C", "A"}
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalOnly
        }

        Dim result As CumulativeDistributionPlotSetResult =
            CumulativeDistributionPlot.ComputeGrouped(values, groups, "Cycle time", options)

        Assert.AreEqual(3, result.Count)
        Dim series As CumulativeDistributionPlotResult() = result.Series

        Assert.AreEqual("Cycle time - B", series(0).SeriesName)
        Assert.AreEqual("Cycle time - A", series(1).SeriesName)
        Assert.AreEqual("Cycle time - C", series(2).SeriesName)

        CollectionAssert.AreEqual(New Double() {20.0R, 21.0R}, series(0).SortedData)
        CollectionAssert.AreEqual(New Double() {10.0R, 11.0R}, series(1).SortedData)
        CollectionAssert.AreEqual(New Double() {30.0R}, series(2).SortedData)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Grouped")>
    Public Sub ComputeGrouped_ValidatesRowCountsAndIncompleteRows()
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                CumulativeDistributionPlot.ComputeGrouped(
                    New Double() {1.0R, 2.0R},
                    New String() {"A"})
            End Sub)

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                CumulativeDistributionPlot.ComputeGrouped(
                    New Object() {1.0R, 2.0R},
                    New Object() {"A", Nothing},
                    options:=New CumulativeDistributionPlotOptions With {
                        .Mode = CumulativeDistributionPlotMode.EmpiricalOnly
                    })
            End Sub)

        Dim omitted As CumulativeDistributionPlotSetResult =
            CumulativeDistributionPlot.ComputeGrouped(
                New Object() {1.0R, 2.0R, 3.0R},
                New Object() {"A", Nothing, "B"},
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
                    .OmitInvalidRows = True
                })

        Assert.AreEqual(2, omitted.Count)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub NormalFit_UsesSampleMeanAndSampleStandardDeviation()
        Dim data As Double() = {1.0R, 2.0R, 3.0R, 4.0R, 5.0R}
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution,
            .Distribution = CumulativeDistributionKind.Normal,
            .YScale = CumulativeDistributionYScale.Probability
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(data, options:=options)

        Assert.IsNotNull(result.Fit)
        Assert.IsTrue(result.Fit.Succeeded)
        Assert.IsTrue(result.Fit.Converged)
        Assert.AreEqual(CumulativeDistributionKind.Normal, result.Fit.Distribution)
        Assert.AreEqual(241, result.FittedX.Length)
        Assert.AreEqual(241, result.FittedY.Length)

        Dim mean As Double
        Dim sd As Double
        Assert.IsTrue(result.Fit.TryGetParameter("Mean", mean))
        Assert.IsTrue(result.Fit.TryGetParameter("Standard deviation", sd))
        AssertClose(3.0R, mean)
        AssertClose(Math.Sqrt(2.5R), sd)

        AssertClose(0.5R, CumulativeDistributionPlot.EvaluateFittedCdf(mean, result.Fit),
                    1.0E-8R)
        AssertMonotoneUnitInterval(result.FittedY)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub ExponentialFit_UsesSampleMeanAsScale()
        Dim data As Double() = {1.0R, 2.0R, 3.0R, 4.0R}
        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                data,
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.FittedDistributionOnly,
                    .Distribution = CumulativeDistributionKind.Exponential,
                    .YScale = CumulativeDistributionYScale.Probability
                })

        Assert.AreEqual(0, result.EmpiricalX.Length)
        Assert.AreEqual(0, result.EmpiricalY.Length)
        Assert.IsTrue(result.Fit.Succeeded)

        Dim scale As Double
        Assert.IsTrue(result.Fit.TryGetParameter("Scale", scale))
        AssertClose(2.5R, scale)
        AssertClose(1.0R - Math.Exp(-1.0R),
                    CumulativeDistributionPlot.EvaluateFittedCdf(scale, result.Fit),
                    1.0E-10R)
        AssertMonotoneUnitInterval(result.FittedY)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub TwoParameterExponential_FindsBoundaryThresholdAndScale()
        Dim data As Double() = {5.0R, 6.0R, 8.0R, 10.0R}
        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                data,
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution,
                    .Distribution = CumulativeDistributionKind.TwoParameterExponential,
                    .YScale = CumulativeDistributionYScale.Probability
                })

        Assert.IsTrue(result.Fit.Succeeded)

        Dim threshold As Double
        Dim scale As Double
        Assert.IsTrue(result.Fit.TryGetParameter("Threshold", threshold))
        Assert.IsTrue(result.Fit.TryGetParameter("Scale", scale))
        AssertClose(5.0R, threshold)
        AssertClose((0.0R + 1.0R + 3.0R + 5.0R) / 4.0R, scale)

        AssertClose(0.0R, CumulativeDistributionPlot.EvaluateFittedCdf(threshold, result.Fit))
        Assert.IsTrue(CumulativeDistributionPlot.EvaluateFittedCdf(10.0R, result.Fit) > 0.0R)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub PositiveSupportDistributions_RejectNonpositiveDataWithoutCrashingEcdf()
        Dim positiveSupportKinds As CumulativeDistributionKind() = {
            CumulativeDistributionKind.Lognormal,
            CumulativeDistributionKind.Gamma,
            CumulativeDistributionKind.Weibull,
            CumulativeDistributionKind.Loglogistic
        }

        For Each kind As CumulativeDistributionKind In positiveSupportKinds
            Dim result As CumulativeDistributionPlotResult =
                CumulativeDistributionPlot.Compute(
                    New Double() {0.0R, 1.0R, 2.0R, 3.0R},
                    options:=New CumulativeDistributionPlotOptions With {
                        .Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution,
                        .Distribution = kind
                    })

            Assert.IsNotNull(result.Fit, kind.ToString())
            Assert.IsFalse(result.Fit.Succeeded, kind.ToString())
            Assert.AreEqual(0, result.FittedX.Length, kind.ToString())
            Assert.AreEqual(0, result.FittedY.Length, kind.ToString())
            Assert.IsTrue(result.EmpiricalX.Length > 0, kind.ToString())
            StringAssert.Contains(result.Fit.Message, "greater than zero")
        Next
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub ConstantSample_NormalFitFailsGracefullyButEmpiricalCurveRemainsAvailable()
        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                New Double() {7.0R, 7.0R, 7.0R, 7.0R},
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution,
                    .Distribution = CumulativeDistributionKind.Normal
                })

        Assert.IsFalse(result.Fit.Succeeded)
        StringAssert.Contains(result.Fit.Message, "zero variability")
        Assert.AreEqual(2, result.EmpiricalX.Length)
        Assert.AreEqual(2, result.EmpiricalY.Length)
        Assert.AreEqual(0, result.FittedX.Length)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Fits")>
    Public Sub NumericalFits_SucceedAndProduceMonotoneFiniteCdfs()
        'Well-spread deterministic positive data exercises all numerical fitting families
        'without relying on random-number generation.
        Dim data As Double() = {
            8.4R, 9.1R, 10.2R, 11.0R, 12.7R, 13.4R, 14.1R, 15.8R,
            16.3R, 17.9R, 18.6R, 20.2R, 21.7R, 23.1R, 24.8R, 26.0R,
            27.6R, 29.4R, 31.0R, 33.7R, 36.2R, 39.5R, 43.1R, 47.8R
        }

        Dim numericalKinds As CumulativeDistributionKind() = {
            CumulativeDistributionKind.ThreeParameterLognormal,
            CumulativeDistributionKind.Gamma,
            CumulativeDistributionKind.ThreeParameterGamma,
            CumulativeDistributionKind.SmallestExtremeValue,
            CumulativeDistributionKind.Weibull,
            CumulativeDistributionKind.ThreeParameterWeibull,
            CumulativeDistributionKind.LargestExtremeValue,
            CumulativeDistributionKind.Logistic,
            CumulativeDistributionKind.Loglogistic,
            CumulativeDistributionKind.ThreeParameterLoglogistic
        }

        For Each kind As CumulativeDistributionKind In numericalKinds
            Dim result As CumulativeDistributionPlotResult =
                CumulativeDistributionPlot.Compute(
                    data,
                    options:=New CumulativeDistributionPlotOptions With {
                        .Mode = CumulativeDistributionPlotMode.FittedDistributionOnly,
                        .Distribution = kind,
                        .YScale = CumulativeDistributionYScale.Probability,
                        .FittedCurvePointCount = 101,
                        .MaximumFitIterations = 2500,
                        .FitTolerance = 1.0E-8R
                    })

            Assert.IsNotNull(result.Fit, kind.ToString())
            Assert.IsTrue(result.Fit.Succeeded,
                          kind.ToString() & ": " & result.Fit.Message)
            Assert.IsTrue(Double.IsNaN(result.Fit.LogLikelihood) = False,
                          kind.ToString() & " returned NaN log-likelihood.")
            Assert.AreEqual(101, result.FittedX.Length, kind.ToString())
            Assert.AreEqual(101, result.FittedY.Length, kind.ToString())
            AssertMonotoneUnitInterval(result.FittedY, kind.ToString())
        Next
    End Sub

    <TestMethod>
    <TestCategory("CDF-Bounds")>
    Public Sub ExplicitXBounds_AreRespectedExactly()
        Dim options As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution,
            .Distribution = CumulativeDistributionKind.Normal,
            .XMinimum = -10.0R,
            .XMaximum = 20.0R,
            .FittedCurvePointCount = 51
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(New Double() {1.0R, 2.0R, 3.0R, 4.0R},
                                               options:=options)

        AssertClose(-10.0R, result.XMinimum)
        AssertClose(20.0R, result.XMaximum)
        AssertClose(-10.0R, result.FittedX.First())
        AssertClose(20.0R, result.FittedX.Last())
        Assert.AreEqual(51, result.FittedX.Length)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Contracts")>
    Public Sub ResultsAndOptions_ReturnDefensiveCopies()
        Dim sourceOptions As New CumulativeDistributionPlotOptions With {
            .Mode = CumulativeDistributionPlotMode.EmpiricalOnly,
            .Percentiles = New Double() {25.0R, 75.0R}
        }

        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(New Double() {1.0R, 2.0R, 3.0R, 4.0R},
                                               options:=sourceOptions)

        sourceOptions.Percentiles(0) = 99.0R
        sourceOptions.Mode = CumulativeDistributionPlotMode.FittedDistributionOnly

        Dim xCopy As Double() = result.EmpiricalX
        xCopy(0) = 999.0R
        AssertClose(1.0R, result.EmpiricalX(0))

        Dim sortedCopy As Double() = result.SortedData
        sortedCopy(0) = 999.0R
        AssertClose(1.0R, result.SortedData(0))

        Dim optionCopy As CumulativeDistributionPlotOptions = result.Options
        optionCopy.Percentiles(0) = 99.0R
        optionCopy.Mode = CumulativeDistributionPlotMode.FittedDistributionOnly

        Assert.AreEqual(CumulativeDistributionPlotMode.EmpiricalOnly, result.Options.Mode)
        AssertClose(25.0R, result.Options.Percentiles(0))
        AssertClose(25.0R, result.Percentiles(0).Percent)
    End Sub

    <TestMethod>
    <TestCategory("CDF-Contracts")>
    Public Sub FitParameterArrays_AreDefensiveAndNamedLookupIsCaseInsensitive()
        Dim result As CumulativeDistributionPlotResult =
            CumulativeDistributionPlot.Compute(
                New Double() {1.0R, 2.0R, 3.0R, 4.0R, 5.0R},
                options:=New CumulativeDistributionPlotOptions With {
                    .Mode = CumulativeDistributionPlotMode.FittedDistributionOnly,
                    .Distribution = CumulativeDistributionKind.Normal
                })

        Dim originalMean As Double
        Assert.IsTrue(result.Fit.TryGetParameter("mean", originalMean))

        Dim values As Double() = result.Fit.ParameterValues
        Dim names As String() = result.Fit.ParameterNames
        values(0) = 999.0R
        names(0) = "Changed"

        Dim meanAgain As Double
        Assert.IsTrue(result.Fit.TryGetParameter("MEAN", meanAgain))
        AssertClose(originalMean, meanAgain)
        Assert.AreEqual("Mean", result.Fit.ParameterNames(0))
    End Sub

    Private Shared Sub AssertMonotoneUnitInterval(values As Double(),
                                                  Optional message As String = "")
        Assert.IsNotNull(values)
        Assert.IsTrue(values.Length > 0, message)

        Dim previous As Double = Double.NegativeInfinity
        For i As Integer = 0 To values.Length - 1
            Dim value As Double = values(i)
            Assert.IsFalse(Double.IsNaN(value) OrElse Double.IsInfinity(value),
                           message & " Non-finite CDF value at index " & i.ToString() & ".")
            Assert.IsTrue(value >= -FitTolerance AndAlso value <= 1.0R + FitTolerance,
                          message & " CDF value outside [0,1] at index " & i.ToString() & ".")
            Assert.IsTrue(value + FitTolerance >= previous,
                          message & " CDF is not monotone at index " & i.ToString() & ".")
            previous = value
        Next
    End Sub

    Private Shared Sub AssertArrayClose(expected As Double(),
                                        actual As Double(),
                                        Optional tolerance As Double = TightTolerance)
        Assert.AreEqual(expected.Length, actual.Length, "Array lengths differ.")
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i), tolerance,
                        "Mismatch at index " & i.ToString() & ".")
        Next
    End Sub

    Private Shared Sub AssertClose(expected As Double,
                                   actual As Double,
                                   Optional tolerance As Double = TightTolerance,
                                   Optional message As String = Nothing)
        Assert.IsFalse(Double.IsNaN(actual),
                       If(message, String.Empty) & " Actual value is NaN.")
        Assert.AreEqual(expected, actual, tolerance, message)
    End Sub

End Class
