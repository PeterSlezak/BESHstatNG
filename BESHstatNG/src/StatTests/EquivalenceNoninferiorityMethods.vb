Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure


Namespace equivalencetests

    ''' <summary>
    ''' Summarizes how a confidence interval compares with pre-specified lower and upper decision limits.
    ''' </summary>
    Public Class MarginCiAssessmentResult
        ''' <summary>Point estimate of the quantity being compared with the limits.</summary>
        Public Property Estimate As Double
        ''' <summary>Confidence interval used for the assessment.</summary>
        Public Property ConfidenceInterval As ConfidenceIntervalResult
        ''' <summary>Lower decision limit.</summary>
        Public Property LowerMargin As Double
        ''' <summary>Upper decision limit.</summary>
        Public Property UpperMargin As Double
        ''' <summary>True when the point estimate lies between the two limits.</summary>
        Public Property IsPointEstimateWithinMargins As Boolean
        ''' <summary>True when the entire confidence interval lies between the two limits.</summary>
        Public Property IsConfidenceIntervalWithinMargins As Boolean
        ''' <summary>True when the lower confidence bound is above the lower limit.</summary>
        Public Property SupportsLowerNonInferiority As Boolean
        ''' <summary>True when the upper confidence bound is below the upper limit.</summary>
        Public Property SupportsUpperNonInferiority As Boolean
        ''' <summary>Short text summary of the interval-based decision.</summary>
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Result of a one-sided non-inferiority comparison for two independent means.
    ''' </summary>
    Public Class MeanNonInferiorityResult
        Public Property NumberOfControls As Integer
        Public Property NumberOfExperimental As Integer
        Public Property MeanControl As Double
        Public Property MeanExperimental As Double
        Public Property DifferenceExperimentalMinusControl As Double
        Public Property StandardError As Double
        Public Property DegreesOfFreedom As Double
        Public Property AssumeEqualVariances As Boolean
        Public Property NonInferiorityMargin As Double
        Public Property NonInferiorityLimit As Double
        Public Property AlphaOneSided As Double
        Public Property TestStatistic As Double
        Public Property PValue As Double
        Public Property LowerOneSidedConfidenceLimit As Double
        Public Property TwoSidedEquivalentConfidenceInterval As ConfidenceIntervalResult
        Public Property SupportsNonInferiority As Boolean
        Public Property CiAssessment As MarginCiAssessmentResult
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Result of a TOST-style equivalence comparison for two independent means.
    ''' </summary>
    Public Class MeanEquivalenceResult
        Public Property NumberOfControls As Integer
        Public Property NumberOfExperimental As Integer
        Public Property MeanControl As Double
        Public Property MeanExperimental As Double
        Public Property DifferenceExperimentalMinusControl As Double
        Public Property StandardError As Double
        Public Property DegreesOfFreedom As Double
        Public Property AssumeEqualVariances As Boolean
        Public Property LowerMargin As Double
        Public Property UpperMargin As Double
        Public Property AlphaOneSided As Double
        Public Property LowerComponentStatistic As Double
        Public Property LowerComponentPValue As Double
        Public Property UpperComponentStatistic As Double
        Public Property UpperComponentPValue As Double
        Public Property TostPValue As Double
        Public Property EquivalentConfidenceInterval As ConfidenceIntervalResult
        Public Property SupportsEquivalence As Boolean
        Public Property CiAssessment As MarginCiAssessmentResult
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Result of a one-sided non-inferiority comparison for two independent proportions.
    ''' </summary>
    Public Class ProportionNonInferiorityResult
        Public Property NumberOfControls As Integer
        Public Property NumberOfExperimental As Integer
        Public Property ControlResponders As Integer
        Public Property ExperimentalResponders As Integer
        Public Property ControlProportion As Double
        Public Property ExperimentalProportion As Double
        Public Property DifferenceExperimentalMinusControl As Double
        Public Property StandardError As Double
        Public Property NonInferiorityMargin As Double
        Public Property NonInferiorityLimit As Double
        Public Property AlphaOneSided As Double
        Public Property ZStatistic As Double
        Public Property PValue As Double
        Public Property LowerOneSidedConfidenceLimit As Double
        Public Property TwoSidedEquivalentConfidenceInterval As ConfidenceIntervalResult
        Public Property SupportsNonInferiority As Boolean
        Public Property CiAssessment As MarginCiAssessmentResult
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Result of a TOST-style equivalence comparison for two independent proportions.
    ''' </summary>
    Public Class ProportionEquivalenceResult
        Public Property NumberOfControls As Integer
        Public Property NumberOfExperimental As Integer
        Public Property ControlResponders As Integer
        Public Property ExperimentalResponders As Integer
        Public Property ControlProportion As Double
        Public Property ExperimentalProportion As Double
        Public Property DifferenceExperimentalMinusControl As Double
        Public Property StandardError As Double
        Public Property LowerMargin As Double
        Public Property UpperMargin As Double
        Public Property AlphaOneSided As Double
        Public Property LowerComponentStatistic As Double
        Public Property LowerComponentPValue As Double
        Public Property UpperComponentStatistic As Double
        Public Property UpperComponentPValue As Double
        Public Property TostPValue As Double
        Public Property EquivalentConfidenceInterval As ConfidenceIntervalResult
        Public Property SupportsEquivalence As Boolean
        Public Property CiAssessment As MarginCiAssessmentResult
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Summarizes whether bias and limits of agreement are acceptable relative to pre-specified decision limits.
    ''' </summary>
    Public Class BlandAltmanDecisionLimitAssessmentResult
        ''' <summary>Full Bland–Altman result used for the assessment.</summary>
        Public Property BlandAltman As Agreement.BlandAltmanResult
        ''' <summary>Assessment of the mean bias confidence interval versus the allowable region.</summary>
        Public Property BiasAssessment As MarginCiAssessmentResult
        ''' <summary>Lower acceptable difference limit on the original scale.</summary>
        Public Property LowerAllowableLimit As Double
        ''' <summary>Upper acceptable difference limit on the original scale.</summary>
        Public Property UpperAllowableLimit As Double
        ''' <summary>True when the observed lower and upper limits of agreement lie within the allowable region.</summary>
        Public Property AreObservedLoAWithinAllowableLimits As Boolean
        ''' <summary>True when the confidence interval for the lower limit of agreement stays above the lower allowable limit and the confidence interval for the upper limit stays below the upper allowable limit.</summary>
        Public Property AreLoAConfidenceIntervalsWithinAllowableLimits As Boolean
        ''' <summary>Short text summary of the combined bias / agreement decision.</summary>
        Public Property Conclusion As String
    End Class

    ''' <summary>
    ''' Back-end routines for non-inferiority testing, TOST-style equivalence testing,
    ''' confidence-interval-based equivalence reporting, and agreement decision-limit assessment.
    ''' </summary>
    Public Module EquivalenceNonInferiorityMethods

        ''' <summary>
        ''' Performs a one-sided non-inferiority comparison of two independent means using the difference
        ''' <c>(experimental - control)</c> and a positive non-inferiority margin.
        ''' </summary>
        ''' <param name="controlSample">Numeric observations for the control or reference group.</param>
        ''' <param name="experimentalSample">Numeric observations for the experimental or test group.</param>
        ''' <param name="nonInferiorityMargin">Positive margin magnitude. The null limit is <c>-margin</c> on the difference scale.</param>
        ''' <param name="alphaOneSided">One-sided type I error rate.</param>
        ''' <param name="assumeEqualVariances">If <c>True</c>, uses the pooled-variance t test; otherwise uses Welch's unequal-variance method.</param>
        ''' <returns>A result object containing the one-sided test, a matching CI-based assessment, and a text conclusion.</returns>
        Public Function TestUnpairedMeansNonInferiority(controlSample As Double(),
                                                       experimentalSample As Double(),
                                                       nonInferiorityMargin As Double,
                                                       Optional alphaOneSided As Double = 0.025,
                                                       Optional assumeEqualVariances As Boolean = False) As MeanNonInferiorityResult

            Dim controlStats = ComputeSampleMoments(controlSample, NameOf(controlSample))
            Dim experimentalStats = ComputeSampleMoments(experimentalSample, NameOf(experimentalSample))
            Return TestUnpairedMeansNonInferiorityFromSummary(controlStats.Mean,
                                                              controlStats.StandardDeviation,
                                                              controlStats.Count,
                                                              experimentalStats.Mean,
                                                              experimentalStats.StandardDeviation,
                                                              experimentalStats.Count,
                                                              nonInferiorityMargin,
                                                              alphaOneSided,
                                                              assumeEqualVariances)
        End Function

        ''' <summary>
        ''' Performs a one-sided non-inferiority comparison of two independent means from summary statistics.
        ''' </summary>
        Public Function TestUnpairedMeansNonInferiorityFromSummary(controlMean As Double,
                                                                  controlSd As Double,
                                                                  controlN As Integer,
                                                                  experimentalMean As Double,
                                                                  experimentalSd As Double,
                                                                  experimentalN As Integer,
                                                                  nonInferiorityMargin As Double,
                                                                  Optional alphaOneSided As Double = 0.025,
                                                                  Optional assumeEqualVariances As Boolean = False) As MeanNonInferiorityResult

            ValidateSummaryInputs(controlSd, controlN, NameOf(controlSd), NameOf(controlN))
            ValidateSummaryInputs(experimentalSd, experimentalN, NameOf(experimentalSd), NameOf(experimentalN))
            ValidatePositive(nonInferiorityMargin, NameOf(nonInferiorityMargin))
            ValidateAlphaOneSided(alphaOneSided, NameOf(alphaOneSided))

            Dim comparison = ComputeUnpairedMeanComparison(controlMean, controlSd, controlN,
                                                           experimentalMean, experimentalSd, experimentalN,
                                                           assumeEqualVariances)

            Dim limit As Double = -nonInferiorityMargin
            Dim tStatistic As Double = (comparison.Difference - limit) / comparison.StandardError
            Dim pValue As Double = 1.0 - distributions.T_CDF(tStatistic, comparison.DegreesOfFreedom)
            Dim oneSidedCrit As Double = distributions.T_Inv(1.0 - alphaOneSided, comparison.DegreesOfFreedom)
            Dim lowerOneSidedLimit As Double = comparison.Difference - (oneSidedCrit * comparison.StandardError)
            Dim ci As ConfidenceIntervalResult = BuildMeanDifferenceConfidenceInterval(comparison.Difference,
                                                                                      comparison.StandardError,
                                                                                      comparison.DegreesOfFreedom,
                                                                                      2.0 * alphaOneSided)
            Dim ciAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(ci, limit, Double.PositiveInfinity)
            Dim supportsNi As Boolean = (pValue <= alphaOneSided)

            Return New MeanNonInferiorityResult With {
                .NumberOfControls = controlN,
                .NumberOfExperimental = experimentalN,
                .MeanControl = controlMean,
                .MeanExperimental = experimentalMean,
                .DifferenceExperimentalMinusControl = comparison.Difference,
                .StandardError = comparison.StandardError,
                .DegreesOfFreedom = comparison.DegreesOfFreedom,
                .AssumeEqualVariances = assumeEqualVariances,
                .NonInferiorityMargin = nonInferiorityMargin,
                .NonInferiorityLimit = limit,
                .AlphaOneSided = alphaOneSided,
                .TestStatistic = tStatistic,
                .PValue = pValue,
                .LowerOneSidedConfidenceLimit = lowerOneSidedLimit,
                .TwoSidedEquivalentConfidenceInterval = ci,
                .SupportsNonInferiority = supportsNi,
                .CiAssessment = ciAssessment,
                .Conclusion = If(supportsNi,
                                 "Supports non-inferiority: the lower one-sided confidence limit is above the non-inferiority limit.",
                                 "Does not support non-inferiority at the requested alpha.")
            }
        End Function

        ''' <summary>
        ''' Performs a TOST-style equivalence comparison of two independent means using the difference
        ''' <c>(experimental - control)</c> and user-specified lower and upper equivalence margins.
        ''' </summary>
        Public Function TestUnpairedMeansEquivalence(controlSample As Double(),
                                                     experimentalSample As Double(),
                                                     lowerMargin As Double,
                                                     upperMargin As Double,
                                                     Optional alphaOneSided As Double = 0.025,
                                                     Optional assumeEqualVariances As Boolean = False) As MeanEquivalenceResult

            Dim controlStats = ComputeSampleMoments(controlSample, NameOf(controlSample))
            Dim experimentalStats = ComputeSampleMoments(experimentalSample, NameOf(experimentalSample))

            Return TestUnpairedMeansEquivalenceFromSummary(controlStats.Mean,
                                                           controlStats.StandardDeviation,
                                                           controlStats.Count,
                                                           experimentalStats.Mean,
                                                           experimentalStats.StandardDeviation,
                                                           experimentalStats.Count,
                                                           lowerMargin,
                                                           upperMargin,
                                                           alphaOneSided,
                                                           assumeEqualVariances)
        End Function

        ''' <summary>
        ''' Performs a TOST-style equivalence comparison of two independent means from summary statistics.
        ''' </summary>
        Public Function TestUnpairedMeansEquivalenceFromSummary(controlMean As Double,
                                                                controlSd As Double,
                                                                controlN As Integer,
                                                                experimentalMean As Double,
                                                                experimentalSd As Double,
                                                                experimentalN As Integer,
                                                                lowerMargin As Double,
                                                                upperMargin As Double,
                                                                Optional alphaOneSided As Double = 0.025,
                                                                Optional assumeEqualVariances As Boolean = False) As MeanEquivalenceResult

            ValidateSummaryInputs(controlSd, controlN, NameOf(controlSd), NameOf(controlN))
            ValidateSummaryInputs(experimentalSd, experimentalN, NameOf(experimentalSd), NameOf(experimentalN))
            ValidateMargins(lowerMargin, upperMargin)
            ValidateAlphaOneSided(alphaOneSided, NameOf(alphaOneSided))

            Dim comparison = ComputeUnpairedMeanComparison(controlMean, controlSd, controlN,
                                                           experimentalMean, experimentalSd, experimentalN,
                                                           assumeEqualVariances)

            If comparison.Difference <= lowerMargin OrElse comparison.Difference >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(experimentalMean), "The expected or observed mean difference must lie strictly inside the equivalence margins.")
            End If

            Dim lowerT As Double = (comparison.Difference - lowerMargin) / comparison.StandardError
            Dim lowerP As Double = 1.0 - distributions.T_CDF(lowerT, comparison.DegreesOfFreedom)
            Dim upperT As Double = (comparison.Difference - upperMargin) / comparison.StandardError
            Dim upperP As Double = distributions.T_CDF(upperT, comparison.DegreesOfFreedom)
            Dim tostP As Double = Math.Max(lowerP, upperP)
            Dim ci As ConfidenceIntervalResult = BuildMeanDifferenceConfidenceInterval(comparison.Difference,
                                                                                      comparison.StandardError,
                                                                                      comparison.DegreesOfFreedom,
                                                                                      2.0 * alphaOneSided)
            Dim ciAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(ci, lowerMargin, upperMargin)
            Dim supportsEquivalence As Boolean = (lowerP <= alphaOneSided AndAlso upperP <= alphaOneSided)

            Return New MeanEquivalenceResult With {
                .NumberOfControls = controlN,
                .NumberOfExperimental = experimentalN,
                .MeanControl = controlMean,
                .MeanExperimental = experimentalMean,
                .DifferenceExperimentalMinusControl = comparison.Difference,
                .StandardError = comparison.StandardError,
                .DegreesOfFreedom = comparison.DegreesOfFreedom,
                .AssumeEqualVariances = assumeEqualVariances,
                .LowerMargin = lowerMargin,
                .UpperMargin = upperMargin,
                .AlphaOneSided = alphaOneSided,
                .LowerComponentStatistic = lowerT,
                .LowerComponentPValue = lowerP,
                .UpperComponentStatistic = upperT,
                .UpperComponentPValue = upperP,
                .TostPValue = tostP,
                .EquivalentConfidenceInterval = ci,
                .SupportsEquivalence = supportsEquivalence,
                .CiAssessment = ciAssessment,
                .Conclusion = If(supportsEquivalence,
                                 "Supports equivalence: the TOST components are both significant and the confidence interval lies within the equivalence margins.",
                                 "Does not support equivalence at the requested alpha.")
            }
        End Function

        ''' <summary>
        ''' Performs a one-sided non-inferiority comparison for two independent proportions using the difference
        ''' <c>(experimental proportion - control proportion)</c> and a positive non-inferiority margin.
        ''' </summary>
        Public Function TestIndependentProportionsNonInferiority(controlResponders As Integer,
                                                                 controlTotal As Integer,
                                                                 experimentalResponders As Integer,
                                                                 experimentalTotal As Integer,
                                                                 nonInferiorityMargin As Double,
                                                                 Optional alphaOneSided As Double = 0.025) As ProportionNonInferiorityResult

            ValidateCounts(controlResponders, controlTotal, NameOf(controlResponders), NameOf(controlTotal))
            ValidateCounts(experimentalResponders, experimentalTotal, NameOf(experimentalResponders), NameOf(experimentalTotal))
            ValidatePositive(nonInferiorityMargin, NameOf(nonInferiorityMargin))
            ValidateAlphaOneSided(alphaOneSided, NameOf(alphaOneSided))

            Dim pControl As Double = controlResponders / CDbl(controlTotal)
            Dim pExperimental As Double = experimentalResponders / CDbl(experimentalTotal)
            Dim diff As Double = pExperimental - pControl
            Dim se As Double = Math.Sqrt((pControl * (1.0 - pControl) / controlTotal) + (pExperimental * (1.0 - pExperimental) / experimentalTotal))
            If se <= 0.0 OrElse Double.IsNaN(se) OrElse Double.IsInfinity(se) Then
                Throw New InvalidOperationException("A nonzero standard error is required for proportion non-inferiority testing.")
            End If

            Dim limit As Double = -nonInferiorityMargin
            Dim z As Double = (diff - limit) / se
            Dim pValue As Double = 1.0 - distributions.PNorm(z)
            Dim zCrit As Double = distributions.NormSInv(1.0 - alphaOneSided)
            Dim lowerOneSided As Double = diff - (zCrit * se)
            Dim ci As ConfidenceIntervalResult = contingencytable.TwoIndependentProportions(experimentalResponders,
                                                                                            experimentalTotal,
                                                                                            controlResponders,
                                                                                            controlTotal,
                                                                                            2.0 * alphaOneSided)
            Dim ciAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(ci, limit, Double.PositiveInfinity)
            Dim supportsNi As Boolean = (pValue <= alphaOneSided)

            Return New ProportionNonInferiorityResult With {
                .NumberOfControls = controlTotal,
                .NumberOfExperimental = experimentalTotal,
                .ControlResponders = controlResponders,
                .ExperimentalResponders = experimentalResponders,
                .ControlProportion = pControl,
                .ExperimentalProportion = pExperimental,
                .DifferenceExperimentalMinusControl = diff,
                .StandardError = se,
                .NonInferiorityMargin = nonInferiorityMargin,
                .NonInferiorityLimit = limit,
                .AlphaOneSided = alphaOneSided,
                .ZStatistic = z,
                .PValue = pValue,
                .LowerOneSidedConfidenceLimit = lowerOneSided,
                .TwoSidedEquivalentConfidenceInterval = ci,
                .SupportsNonInferiority = supportsNi,
                .CiAssessment = ciAssessment,
                .Conclusion = If(supportsNi,
                                 "Supports non-inferiority: the lower one-sided confidence limit is above the non-inferiority limit.",
                                 "Does not support non-inferiority at the requested alpha.")
            }
        End Function

        ''' <summary>
        ''' Performs a TOST-style equivalence comparison for two independent proportions using the difference
        ''' <c>(experimental proportion - control proportion)</c> and user-specified lower and upper margins.
        ''' </summary>
        Public Function TestIndependentProportionsEquivalence(controlResponders As Integer,
                                                              controlTotal As Integer,
                                                              experimentalResponders As Integer,
                                                              experimentalTotal As Integer,
                                                              lowerMargin As Double,
                                                              upperMargin As Double,
                                                              Optional alphaOneSided As Double = 0.025) As ProportionEquivalenceResult

            ValidateCounts(controlResponders, controlTotal, NameOf(controlResponders), NameOf(controlTotal))
            ValidateCounts(experimentalResponders, experimentalTotal, NameOf(experimentalResponders), NameOf(experimentalTotal))
            ValidateMargins(lowerMargin, upperMargin)
            ValidateAlphaOneSided(alphaOneSided, NameOf(alphaOneSided))

            Dim pControl As Double = controlResponders / CDbl(controlTotal)
            Dim pExperimental As Double = experimentalResponders / CDbl(experimentalTotal)
            Dim diff As Double = pExperimental - pControl

            If diff <= lowerMargin OrElse diff >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(experimentalResponders), "The observed proportion difference must lie strictly inside the equivalence margins.")
            End If

            Dim se As Double = Math.Sqrt((pControl * (1.0 - pControl) / controlTotal) + (pExperimental * (1.0 - pExperimental) / experimentalTotal))
            If se <= 0.0 OrElse Double.IsNaN(se) OrElse Double.IsInfinity(se) Then
                Throw New InvalidOperationException("A nonzero standard error is required for proportion equivalence testing.")
            End If

            Dim zLower As Double = (diff - lowerMargin) / se
            Dim pLower As Double = 1.0 - distributions.PNorm(zLower)
            Dim zUpper As Double = (diff - upperMargin) / se
            Dim pUpper As Double = distributions.PNorm(zUpper)
            Dim tostP As Double = Math.Max(pLower, pUpper)
            Dim ci As ConfidenceIntervalResult = contingencytable.TwoIndependentProportions(experimentalResponders,
                                                                                            experimentalTotal,
                                                                                            controlResponders,
                                                                                            controlTotal,
                                                                                            2.0 * alphaOneSided)
            Dim ciAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(ci, lowerMargin, upperMargin)
            Dim supportsEquivalence As Boolean = (pLower <= alphaOneSided AndAlso pUpper <= alphaOneSided)

            Return New ProportionEquivalenceResult With {
                .NumberOfControls = controlTotal,
                .NumberOfExperimental = experimentalTotal,
                .ControlResponders = controlResponders,
                .ExperimentalResponders = experimentalResponders,
                .ControlProportion = pControl,
                .ExperimentalProportion = pExperimental,
                .DifferenceExperimentalMinusControl = diff,
                .StandardError = se,
                .LowerMargin = lowerMargin,
                .UpperMargin = upperMargin,
                .AlphaOneSided = alphaOneSided,
                .LowerComponentStatistic = zLower,
                .LowerComponentPValue = pLower,
                .UpperComponentStatistic = zUpper,
                .UpperComponentPValue = pUpper,
                .TostPValue = tostP,
                .EquivalentConfidenceInterval = ci,
                .SupportsEquivalence = supportsEquivalence,
                .CiAssessment = ciAssessment,
                .Conclusion = If(supportsEquivalence,
                                 "Supports equivalence: the TOST components are both significant and the confidence interval lies within the equivalence margins.",
                                 "Does not support equivalence at the requested alpha.")
            }
        End Function

        ''' <summary>
        ''' Builds a confidence interval for the mean difference <c>(experimental - control)</c> from two independent samples.
        ''' This helper is useful for CI-based equivalence reporting without running the formal TOST.
        ''' </summary>
        Public Function GetUnpairedMeanDifferenceConfidenceInterval(controlSample As Double(),
                                                                   experimentalSample As Double(),
                                                                   Optional alphaTwoSided As Double = 0.05,
                                                                   Optional assumeEqualVariances As Boolean = False) As ConfidenceIntervalResult
            Dim controlStats = ComputeSampleMoments(controlSample, NameOf(controlSample))
            Dim experimentalStats = ComputeSampleMoments(experimentalSample, NameOf(experimentalSample))
            Return GetUnpairedMeanDifferenceConfidenceIntervalFromSummary(controlStats.Mean,
                                                                         controlStats.StandardDeviation,
                                                                         controlStats.Count,
                                                                         experimentalStats.Mean,
                                                                         experimentalStats.StandardDeviation,
                                                                         experimentalStats.Count,
                                                                         alphaTwoSided,
                                                                         assumeEqualVariances)
        End Function

        ''' <summary>
        ''' Builds a confidence interval for the mean difference <c>(experimental - control)</c> from summary statistics.
        ''' </summary>
        Public Function GetUnpairedMeanDifferenceConfidenceIntervalFromSummary(controlMean As Double,
                                                                              controlSd As Double,
                                                                              controlN As Integer,
                                                                              experimentalMean As Double,
                                                                              experimentalSd As Double,
                                                                              experimentalN As Integer,
                                                                              Optional alphaTwoSided As Double = 0.05,
                                                                              Optional assumeEqualVariances As Boolean = False) As ConfidenceIntervalResult
            ValidateSummaryInputs(controlSd, controlN, NameOf(controlSd), NameOf(controlN))
            ValidateSummaryInputs(experimentalSd, experimentalN, NameOf(experimentalSd), NameOf(experimentalN))
            ValidateOpenUnitInterval(alphaTwoSided, NameOf(alphaTwoSided))

            Dim comparison = ComputeUnpairedMeanComparison(controlMean, controlSd, controlN,
                                                           experimentalMean, experimentalSd, experimentalN,
                                                           assumeEqualVariances)
            Return BuildMeanDifferenceConfidenceInterval(comparison.Difference,
                                                         comparison.StandardError,
                                                         comparison.DegreesOfFreedom,
                                                         alphaTwoSided)
        End Function

        ''' <summary>
        ''' Builds a confidence interval for the independent proportion difference <c>(experimental - control)</c>.
        ''' This reuses the existing Wilson/Newcombe-style interval already available in the project.
        ''' </summary>
        Public Function GetIndependentProportionDifferenceConfidenceInterval(controlResponders As Integer,
                                                                             controlTotal As Integer,
                                                                             experimentalResponders As Integer,
                                                                             experimentalTotal As Integer,
                                                                             Optional alphaTwoSided As Double = 0.05) As ConfidenceIntervalResult
            ValidateCounts(controlResponders, controlTotal, NameOf(controlResponders), NameOf(controlTotal))
            ValidateCounts(experimentalResponders, experimentalTotal, NameOf(experimentalResponders), NameOf(experimentalTotal))
            ValidateOpenUnitInterval(alphaTwoSided, NameOf(alphaTwoSided))

            Return contingencytable.TwoIndependentProportions(experimentalResponders,
                                                              experimentalTotal,
                                                              controlResponders,
                                                              controlTotal,
                                                              alphaTwoSided)
        End Function

        ''' <summary>
        ''' Evaluates whether a confidence interval supports non-inferiority or equivalence relative to pre-specified margins.
        ''' This routine can be used with mean differences, proportion differences, bias estimates, or other quantities
        ''' that are interpreted on a single numeric scale.
        ''' </summary>
        ''' <param name="ci">Confidence interval to assess.</param>
        ''' <param name="lowerMargin">Lower decision limit.</param>
        ''' <param name="upperMargin">Upper decision limit.</param>
        Public Function AssessConfidenceIntervalAgainstMargins(ci As ConfidenceIntervalResult, lowerMargin As Double, upperMargin As Double) As MarginCiAssessmentResult
            If ci Is Nothing Then Throw New ArgumentNullException(NameOf(ci))
            If lowerMargin > upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerMargin), "Lower margin must not exceed upper margin.")
            End If

            Dim pointInside As Boolean = (ci.Estimate >= lowerMargin AndAlso ci.Estimate <= upperMargin)
            Dim ciInside As Boolean = (ci.LowerLimit >= lowerMargin AndAlso ci.UpperLimit <= upperMargin)
            Dim lowerSupported As Boolean = (ci.LowerLimit >= lowerMargin)
            Dim upperSupported As Boolean = (ci.UpperLimit <= upperMargin)

            Dim conclusion As String
            If Double.IsNegativeInfinity(lowerMargin) AndAlso Double.IsPositiveInfinity(upperMargin) Then
                conclusion = "No finite decision limits were supplied."
            ElseIf ciInside Then
                conclusion = "The full confidence interval lies inside the decision limits."
            ElseIf lowerSupported AndAlso Double.IsPositiveInfinity(upperMargin) Then
                conclusion = "The confidence interval supports a lower-bound non-inferiority claim."
            ElseIf upperSupported AndAlso Double.IsNegativeInfinity(lowerMargin) Then
                conclusion = "The confidence interval supports an upper-bound non-inferiority claim."
            ElseIf pointInside Then
                conclusion = "The point estimate lies inside the decision limits, but the full confidence interval does not."
            Else
                conclusion = "The confidence interval does not support the requested margin-based conclusion."
            End If

            Return New MarginCiAssessmentResult With {
                .Estimate = ci.Estimate,
                .ConfidenceInterval = ci,
                .LowerMargin = lowerMargin,
                .UpperMargin = upperMargin,
                .IsPointEstimateWithinMargins = pointInside,
                .IsConfidenceIntervalWithinMargins = ciInside,
                .SupportsLowerNonInferiority = lowerSupported,
                .SupportsUpperNonInferiority = upperSupported,
                .Conclusion = conclusion
            }
        End Function

        ''' <summary>
        ''' Assesses whether the mean bias between two paired methods is acceptable relative to allowable-bias limits.
        ''' The assessment is based on the confidence interval for the average paired difference <c>(test - reference)</c>.
        ''' </summary>
        Public Function AssessAllowableBias(referenceValues As Double(),
                                            testValues As Double(),
                                            lowerAllowableBias As Double,
                                            upperAllowableBias As Double,
                                            Optional alphaTwoSided As Double = 0.05) As MarginCiAssessmentResult

            If referenceValues Is Nothing Then Throw New ArgumentNullException(NameOf(referenceValues))
            If testValues Is Nothing Then Throw New ArgumentNullException(NameOf(testValues))
            If referenceValues.Length <> testValues.Length Then Throw New ArgumentException("Reference and test arrays must have the same length.")
            If lowerAllowableBias > upperAllowableBias Then Throw New ArgumentOutOfRangeException(NameOf(lowerAllowableBias), "Lower allowable bias must not exceed the upper allowable bias.")
            ValidateOpenUnitInterval(alphaTwoSided, NameOf(alphaTwoSided))

            Dim differences As Double() = New Double(referenceValues.Length - 1) {}
            For i As Integer = 0 To referenceValues.Length - 1
                If Double.IsNaN(referenceValues(i)) OrElse Double.IsInfinity(referenceValues(i)) Then
                    Throw New ArgumentOutOfRangeException(NameOf(referenceValues), "Reference values must be finite.")
                End If
                If Double.IsNaN(testValues(i)) OrElse Double.IsInfinity(testValues(i)) Then
                    Throw New ArgumentOutOfRangeException(NameOf(testValues), "Test values must be finite.")
                End If
                differences(i) = testValues(i) - referenceValues(i)
            Next

            Dim diffStats = ComputeSampleMoments(differences, "pairedDifferences")
            Dim se As Double = diffStats.StandardDeviation / Math.Sqrt(diffStats.Count)
            If se <= 0.0 Then Throw New InvalidOperationException("A nonzero standard error is required for allowable-bias assessment.")

            Dim ci As New ConfidenceIntervalResult With {
                .alpha = alphaTwoSided,
                .Estimate = diffStats.Mean,
                .LowerLimit = diffStats.Mean - (distributions.T_Inv_2T(alphaTwoSided, diffStats.Count - 1) * se),
                .UpperLimit = diffStats.Mean + (distributions.T_Inv_2T(alphaTwoSided, diffStats.Count - 1) * se),
                .StdErr = se
            }

            Return AssessConfidenceIntervalAgainstMargins(ci, lowerAllowableBias, upperAllowableBias)
        End Function

        ''' <summary>
        ''' Assesses Bland–Altman bias and classical 95% limits of agreement against pre-specified acceptable limits.
        ''' This is useful when method-comparison studies define an allowable error band and require both the mean bias
        ''' and the limits of agreement to stay within that band.
        ''' </summary>
        ''' <param name="referenceValues">Reference-method observations.</param>
        ''' <param name="testValues">Test-method observations aligned with <paramref name="referenceValues"/>.</param>
        ''' <param name="lowerAllowableLimit">Lower acceptable difference on the measurement scale.</param>
        ''' <param name="upperAllowableLimit">Upper acceptable difference on the measurement scale.</param>
        ''' <param name="alphaTwoSided">Two-sided alpha for the confidence intervals.</param>
        Public Function AssessBlandAltmanAgainstDecisionLimits(referenceValues As Double(),
                                                               testValues As Double(),
                                                               lowerAllowableLimit As Double,
                                                               upperAllowableLimit As Double,
                                                               Optional alphaTwoSided As Double = 0.05) As BlandAltmanDecisionLimitAssessmentResult

            If lowerAllowableLimit > upperAllowableLimit Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerAllowableLimit), "Lower allowable limit must not exceed the upper allowable limit.")
            End If
            ValidateOpenUnitInterval(alphaTwoSided, NameOf(alphaTwoSided))

            Dim opts As New Agreement.BlandAltmanOptions With {
                .Alpha = alphaTwoSided,
                .CiMethod = Agreement.AgreementCiMethod.Analytical,
                .UseTDistribution = True
            }

            Dim ba As New Agreement.BlandAltmanAgreement(referenceValues, testValues, "Reference", "Test", opts)
            Dim fit As Agreement.BlandAltmanResult = ba.Fit()
            Dim biasAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(fit.BiasCI, lowerAllowableLimit, upperAllowableLimit)

            Dim observedLoAOk As Boolean = (fit.LowerLoACI.Estimate >= lowerAllowableLimit AndAlso fit.UpperLoACI.Estimate <= upperAllowableLimit)
            Dim loaCiOk As Boolean = (fit.LowerLoACI.LowerLimit >= lowerAllowableLimit AndAlso fit.UpperLoACI.UpperLimit <= upperAllowableLimit)

            Dim conclusion As String
            If loaCiOk AndAlso biasAssessment.IsConfidenceIntervalWithinMargins Then
                conclusion = "Bias and limits of agreement, together with their confidence intervals, are within the allowable region."
            ElseIf observedLoAOk AndAlso biasAssessment.IsPointEstimateWithinMargins Then
                conclusion = "Point estimates are within the allowable region, but the confidence intervals are wider than the limits allow."
            Else
                conclusion = "The observed bias and/or limits of agreement exceed the allowable region."
            End If

            Return New BlandAltmanDecisionLimitAssessmentResult With {
                .BlandAltman = fit,
                .BiasAssessment = biasAssessment,
                .LowerAllowableLimit = lowerAllowableLimit,
                .UpperAllowableLimit = upperAllowableLimit,
                .AreObservedLoAWithinAllowableLimits = observedLoAOk,
                .AreLoAConfidenceIntervalsWithinAllowableLimits = loaCiOk,
                .Conclusion = conclusion
            }
        End Function

        ''' <summary>
        ''' Assesses whether the fitted Bland–Altman bias confidence interval is acceptable relative to allowable-bias limits.
        ''' This overload reuses an already fitted Bland–Altman result, so it also supports repeated-measures and
        ''' transformed-scale analyses.
        ''' </summary>
        ''' <param name="blandAltmanResult">Already fitted Bland–Altman result.</param>
        ''' <param name="lowerAllowableBias">Lower allowable limit on the active Bland–Altman analysis scale.</param>
        ''' <param name="upperAllowableBias">Upper allowable limit on the active Bland–Altman analysis scale.</param>
        Public Function AssessAllowableBias(blandAltmanResult As Agreement.BlandAltmanResult,
                                            lowerAllowableBias As Double,
                                            upperAllowableBias As Double) As MarginCiAssessmentResult

            If blandAltmanResult Is Nothing Then Throw New ArgumentNullException(NameOf(blandAltmanResult))
            If blandAltmanResult.BiasCI Is Nothing Then Throw New ArgumentException("The Bland–Altman result does not contain a fitted bias confidence interval.", NameOf(blandAltmanResult))
            If lowerAllowableBias > upperAllowableBias Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerAllowableBias), "Lower allowable bias must not exceed the upper allowable bias.")
            End If

            Return AssessConfidenceIntervalAgainstMargins(blandAltmanResult.BiasCI, lowerAllowableBias, upperAllowableBias)
        End Function

        ''' <summary>
        ''' Assesses Bland–Altman bias and limits of agreement against pre-specified acceptable limits.
        ''' This overload reuses an already fitted Bland–Altman result, so it supports ordinary paired,
        ''' repeated-measures, and transformed-scale analyses without forcing the decision assessment back
        ''' to the raw-difference simple-pairs model.
        ''' </summary>
        ''' <param name="blandAltmanResult">Already fitted Bland–Altman result.</param>
        ''' <param name="lowerAllowableLimit">Lower acceptable limit on the active Bland–Altman analysis scale.</param>
        ''' <param name="upperAllowableLimit">Upper acceptable limit on the active Bland–Altman analysis scale.</param>
        Public Function AssessBlandAltmanAgainstDecisionLimits(blandAltmanResult As Agreement.BlandAltmanResult,
                                                               lowerAllowableLimit As Double,
                                                               upperAllowableLimit As Double) As BlandAltmanDecisionLimitAssessmentResult

            If blandAltmanResult Is Nothing Then Throw New ArgumentNullException(NameOf(blandAltmanResult))
            If blandAltmanResult.BiasCI Is Nothing Then Throw New ArgumentException("The Bland–Altman result does not contain a fitted bias confidence interval.", NameOf(blandAltmanResult))
            If blandAltmanResult.LowerLoACI Is Nothing OrElse blandAltmanResult.UpperLoACI Is Nothing Then
                Throw New ArgumentException("The Bland–Altman result does not contain fitted limits-of-agreement confidence intervals.", NameOf(blandAltmanResult))
            End If
            If lowerAllowableLimit > upperAllowableLimit Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerAllowableLimit), "Lower allowable limit must not exceed the upper allowable limit.")
            End If

            Dim biasAssessment As MarginCiAssessmentResult = AssessConfidenceIntervalAgainstMargins(blandAltmanResult.BiasCI, lowerAllowableLimit, upperAllowableLimit)

            Dim observedLoAOk As Boolean = (blandAltmanResult.LowerLoACI.Estimate >= lowerAllowableLimit AndAlso
                                    blandAltmanResult.UpperLoACI.Estimate <= upperAllowableLimit)

            Dim loaCiOk As Boolean = (blandAltmanResult.LowerLoACI.LowerLimit >= lowerAllowableLimit AndAlso
                              blandAltmanResult.UpperLoACI.UpperLimit <= upperAllowableLimit)

            Dim conclusion As String
            If loaCiOk AndAlso biasAssessment.IsConfidenceIntervalWithinMargins Then
                conclusion = "Bias and limits of agreement, together with their confidence intervals, are within the allowable region."
            ElseIf observedLoAOk AndAlso biasAssessment.IsPointEstimateWithinMargins Then
                conclusion = "Point estimates are within the allowable region, but the confidence intervals are wider than the limits allow."
            Else
                conclusion = "The observed bias and/or limits of agreement exceed the allowable region."
            End If

            Return New BlandAltmanDecisionLimitAssessmentResult With {
                .BlandAltman = blandAltmanResult,
                .BiasAssessment = biasAssessment,
                .LowerAllowableLimit = lowerAllowableLimit,
                .UpperAllowableLimit = upperAllowableLimit,
                .AreObservedLoAWithinAllowableLimits = observedLoAOk,
                .AreLoAConfidenceIntervalsWithinAllowableLimits = loaCiOk,
                .Conclusion = conclusion
            }
        End Function

        ' ---------------------------------------------------------------------------------
        ' Internal helpers
        ' ---------------------------------------------------------------------------------
        Private Structure SampleMoments
            Public Count As Integer
            Public Mean As Double
            Public StandardDeviation As Double
        End Structure

        Private Structure MeanComparisonSummary
            Public Difference As Double
            Public StandardError As Double
            Public DegreesOfFreedom As Double
        End Structure

        Private Function ComputeSampleMoments(values As Double(), argumentName As String) As SampleMoments
            If values Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(argumentName))

            Dim cleaned As New List(Of Double)()
            For Each value As Double In values
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(argumentName, "All observations must be finite numeric values."))
                End If
                cleaned.Add(value)
            Next

            If cleaned.Count < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two observations are required.", argumentName))

            Return New SampleMoments With {
                .Count = cleaned.Count,
                .Mean = cleaned.Average(),
                .StandardDeviation = StatFunc.stDev(cleaned.ToArray())
            }
        End Function

        Private Function ComputeUnpairedMeanComparison(controlMean As Double, controlSd As Double, controlN As Integer,
                                                       experimentalMean As Double, experimentalSd As Double, experimentalN As Integer,
                                                       assumeEqualVariances As Boolean) As MeanComparisonSummary

            Dim diff As Double = experimentalMean - controlMean
            Dim se As Double
            Dim df As Double

            If assumeEqualVariances Then
                Dim pooledVariance As Double = (((controlN - 1) * controlSd * controlSd) + ((experimentalN - 1) * experimentalSd * experimentalSd)) / (controlN + experimentalN - 2.0)
                se = Math.Sqrt(pooledVariance * ((1.0 / controlN) + (1.0 / experimentalN)))
                df = controlN + experimentalN - 2.0
            Else
                Dim vControl As Double = (controlSd * controlSd) / controlN
                Dim vExperimental As Double = (experimentalSd * experimentalSd) / experimentalN
                se = Math.Sqrt(vControl + vExperimental)
                df = ((vControl + vExperimental) ^ 2) / (((vControl * vControl) / (controlN - 1.0)) + ((vExperimental * vExperimental) / (experimentalN - 1.0)))
            End If

            If se <= 0.0 OrElse Double.IsNaN(se) OrElse Double.IsInfinity(se) Then
                Throw New InvalidOperationException("A nonzero standard error is required for margin-based mean comparisons.")
            End If
            If df <= 0.0 OrElse Double.IsNaN(df) OrElse Double.IsInfinity(df) Then
                Throw New InvalidOperationException("Unable to determine valid degrees of freedom for the mean comparison.")
            End If

            Return New MeanComparisonSummary With {
                .Difference = diff,
                .StandardError = se,
                .DegreesOfFreedom = df
            }
        End Function

        Private Function BuildMeanDifferenceConfidenceInterval(difference As Double, standardError As Double, degreesOfFreedom As Double,
                                                               alphaTwoSided As Double) As ConfidenceIntervalResult
            ValidateOpenUnitInterval(alphaTwoSided, NameOf(alphaTwoSided))
            Dim tCrit As Double = distributions.T_Inv_2T(alphaTwoSided, degreesOfFreedom)

            Return New ConfidenceIntervalResult With {
                .alpha = alphaTwoSided,
                .Estimate = difference,
                .LowerLimit = difference - (tCrit * standardError),
                .UpperLimit = difference + (tCrit * standardError),
                .StdErr = standardError
            }
        End Function

        Private Sub ValidateSummaryInputs(sd As Double, n As Integer, sdName As String, nName As String)
            If n < 2 Then Throw New ArgumentOutOfRangeException(nName, "At least two observations are required.")
            If Double.IsNaN(sd) OrElse Double.IsInfinity(sd) OrElse sd < 0.0 Then
                Throw New ArgumentOutOfRangeException(sdName, "Standard deviation must be a finite non-negative value.")
            End If
        End Sub

        Private Sub ValidateCounts(responders As Integer, total As Integer, respondersName As String, totalName As String)
            If total <= 0 Then Throw New ArgumentOutOfRangeException(totalName, "Total must be positive.")
            If responders < 0 OrElse responders > total Then
                Throw New ArgumentOutOfRangeException(respondersName, "Responder count must be between 0 and the total.")
            End If
        End Sub

        Private Sub ValidateMargins(lowerMargin As Double, upperMargin As Double)
            If Double.IsNaN(lowerMargin) OrElse Double.IsInfinity(lowerMargin) Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerMargin), "Lower margin must be finite.")
            End If
            If Double.IsNaN(upperMargin) OrElse Double.IsInfinity(upperMargin) Then
                Throw New ArgumentOutOfRangeException(NameOf(upperMargin), "Upper margin must be finite.")
            End If
            If lowerMargin >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerMargin), "Lower margin must be less than the upper margin.")
            End If
        End Sub

    End Module
End Namespace
