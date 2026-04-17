Option Explicit On

Namespace SampleSizeCalc

    ''' <summary>
    ''' Result of a two-sample log-rank sample size calculation.
    ''' </summary>
    Public Class LogRankSampleSizeResult
        Public RequiredEvents As Integer
        Public NumberOfControls As Integer
        Public NumberOfExperimental As Integer
        Public TotalNumberOfSubjects As Integer
        Public ControlAllocationProportion As Double
        Public ExperimentalAllocationProportion As Double
        Public AverageEventProportion As Double
    End Class

    ''' <summary>
    ''' Result of a Cox proportional hazards event-count planning calculation.
    ''' </summary>
    Public Class CoxEventCountPlanningResult
        Public RequiredEvents As Integer
        Public EstimatedNumberOfSubjects As Integer
        Public OverallEventProportion As Double
        Public LogHazardRatio As Double
        Public EffectiveVariance As Double
        Public RSquaredWithOtherCovariates As Double
    End Class

    ''' <summary>
    ''' Result of an equivalence sample size calculation for two unpaired means.
    ''' </summary>
    Public Class EquivalenceUnpairedTTestSampleSizeResult
        Public LowerBoundNumberOfControls As Integer
        Public LowerBoundNumberOfExperimental As Integer
        Public UpperBoundNumberOfControls As Integer
        Public UpperBoundNumberOfExperimental As Integer
        Public NumberOfControls As Integer
        Public NumberOfExperimental As Integer
        Public DrivingBound As String
    End Class

    ''' <summary>
    ''' Result of an equivalence sample size calculation for two independent proportions.
    ''' </summary>
    Public Class EquivalenceIndependentProportionsSampleSizeResult
        Public LowerBoundUncorrectedNumberOfControls As Integer
        Public LowerBoundUncorrectedNumberOfExperimental As Integer
        Public LowerBoundCorrectedNumberOfControls As Integer
        Public LowerBoundCorrectedNumberOfExperimental As Integer

        Public UpperBoundUncorrectedNumberOfControls As Integer
        Public UpperBoundUncorrectedNumberOfExperimental As Integer
        Public UpperBoundCorrectedNumberOfControls As Integer
        Public UpperBoundCorrectedNumberOfExperimental As Integer

        Public UncorrectedNumberOfControls As Integer
        Public UncorrectedNumberOfExperimental As Integer
        Public CorrectedNumberOfControls As Integer
        Public CorrectedNumberOfExperimental As Integer
        Public DrivingBound As String
    End Class

    ''' <summary>
    ''' Result of an ICC study-planning calculation based on the one-way random-effects F test.
    ''' </summary>
    Public Class IccHypothesisTestSampleSizeResult
        Public NumberOfSubjects As Integer
        Public NumberOfObservationsPerSubject As Integer
        Public NullIcc As Double
        Public AlternativeIcc As Double
        Public AchievedPower As Double
    End Class

    ''' <summary>
    ''' Result of a Bland-Altman limits-of-agreement precision planning calculation.
    ''' </summary>
    Public Class BlandAltmanAgreementStudyPlanningResult
        Public NumberOfPairs As Integer
        Public ExpectedSdOfDifferences As Double
        Public DesiredHalfWidth As Double
        Public AchievedHalfWidth As Double
        Public Alpha As Double
        Public LoAMultiplier As Double
    End Class

    ''' <summary>
    ''' Result of a paired t-test sample size calculation.
    ''' </summary>
    Public Class PairedTTestSampleSizeResult

        ''' <summary>
        ''' Gets or sets the estimated number of paired observations required.
        ''' </summary>
        Public NumberOfPairs As Integer

    End Class

    ''' <summary>
    ''' Result of an unpaired t-test sample size calculation.
    ''' </summary>
    Public Class UnpairedTTestSampleSizeResult

        ''' <summary>
        ''' Gets or sets the estimated number of subjects required in the control group.
        ''' </summary>
        Public NumberOfControls As Integer

        ''' <summary>
        ''' Gets or sets the estimated number of subjects required in the experimental group.
        ''' </summary>
        Public NumberOfExperimental As Integer

    End Class

    ''' <summary>
    ''' Result of a one-sample proportion sample size calculation.
    ''' </summary>
    Public Class SingleProportionSampleSizeResult

        ''' <summary>
        ''' Gets or sets the estimated total number of subjects required.
        ''' </summary>
        Public NumberOfSubjects As Integer

    End Class

    ''' <summary>
    ''' Result of an independent proportions sample size calculation.
    ''' </summary>
    Public Class IndependentProportionsSampleSizeResult

        ''' <summary>
        ''' Gets or sets the estimated number of control subjects for the uncorrected chi-square test.
        ''' </summary>
        Public UncorrectedNumberOfControls As Integer

        ''' <summary>
        ''' Gets or sets the estimated number of experimental subjects for the uncorrected chi-square test.
        ''' </summary>
        Public UncorrectedNumberOfExperimental As Integer

        ''' <summary>
        ''' Gets or sets the estimated number of control subjects for the corrected chi-square or Fisher's exact test.
        ''' </summary>
        Public CorrectedNumberOfControls As Integer

        ''' <summary>
        ''' Gets or sets the estimated number of experimental subjects for the corrected chi-square or Fisher's exact test.
        ''' </summary>
        Public CorrectedNumberOfExperimental As Integer

    End Class

    ''' <summary>
    ''' Provides sample size calculation helpers used by the sample size UI.
    ''' </summary>
    ''' <remarks>
    ''' These routines perform the statistical calculations only.
    ''' Input parsing and validation are expected to be handled by the caller,
    ''' typically the UI form before invoking this module.
    ''' </remarks>
    Public Module SampleSizeCalculator

        ' -----------------------------------------------------------------------------------------------------
        ' Survival / time-to-event planning
        ' -----------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required number of events and total sample size for a two-sample log-rank comparison.
        ''' </summary>
        ''' <param name="hazardRatio">Anticipated hazard ratio (experimental / control). Must be positive and not equal to 1.</param>
        ''' <param name="controlEventProportion">Expected event proportion in the control arm during the study window.</param>
        ''' <param name="experimentalEventProportion">Expected event proportion in the experimental arm during the study window.</param>
        ''' <param name="controlToExperimentalRatio">Planned allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alpha">Type I error rate. For the default two-sided design, alpha is the usual two-sided alpha (for example 0.05).</param>
        ''' <param name="beta">Type II error rate. Power = 1 - beta.</param>
        ''' <param name="twoSided">If True, uses a two-sided design; otherwise uses a one-sided design.</param>
        ''' <returns>A result containing the required events and the corresponding rounded control / experimental sample sizes.</returns>
        Public Function CalculateLogRankSampleSize(hazardRatio As Double,
                                                   controlEventProportion As Double,
                                                   experimentalEventProportion As Double,
                                                   controlToExperimentalRatio As Double,
                                                   alpha As Double,
                                                   beta As Double,
                                                   Optional twoSided As Boolean = True) As LogRankSampleSizeResult

            ValidateOpenUnitInterval(controlEventProportion, NameOf(controlEventProportion))
            ValidateOpenUnitInterval(experimentalEventProportion, NameOf(experimentalEventProportion))
            ValidateOpenUnitInterval(alpha, NameOf(alpha))
            ValidateOpenUnitInterval(beta, NameOf(beta))
            ValidatePositive(controlToExperimentalRatio, NameOf(controlToExperimentalRatio))
            ValidatePositive(hazardRatio, NameOf(hazardRatio))

            If hazardRatio = 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(hazardRatio), "Hazard ratio must differ from 1.0 for sample size planning.")
            End If

            Dim pControl As Double
            Dim pExperimental As Double
            GetAllocationProportions(controlToExperimentalRatio, pControl, pExperimental)

            Dim requiredEvents As Integer = CInt(Math.Ceiling(CalculateRequiredEventsFromHazardRatio(hazardRatio,
                                                                                                     pControl,
                                                                                                     pExperimental,
                                                                                                     alpha,
                                                                                                     beta,
                                                                                                     twoSided,
                                                                                                     0.0)))

            Dim averageEventProportion As Double = (pControl * controlEventProportion) + (pExperimental * experimentalEventProportion)
            ValidateOpenUnitInterval(averageEventProportion, "averageEventProportion")

            Dim totalSubjects As Integer = CInt(Math.Ceiling(requiredEvents / averageEventProportion))
            Dim experimentalSubjects As Integer = CInt(Math.Ceiling(totalSubjects / (1.0 + controlToExperimentalRatio)))
            Dim controlSubjects As Integer = CInt(Math.Ceiling(experimentalSubjects * controlToExperimentalRatio))

            Return New LogRankSampleSizeResult With {
                .RequiredEvents = requiredEvents,
                .NumberOfControls = controlSubjects,
                .NumberOfExperimental = experimentalSubjects,
                .TotalNumberOfSubjects = controlSubjects + experimentalSubjects,
                .ControlAllocationProportion = pControl,
                .ExperimentalAllocationProportion = pExperimental,
                .AverageEventProportion = averageEventProportion
            }
        End Function

        ''' <summary>
        ''' Estimates the required number of events for a Cox proportional hazards model with a binary covariate
        ''' (for example treatment group), and optionally inflates this to a total sample size when an overall
        ''' event proportion is supplied.
        ''' </summary>
        ''' <param name="hazardRatio">Anticipated hazard ratio for the covariate of interest. Must be positive and not equal to 1.</param>
        ''' <param name="controlToExperimentalRatio">Allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alpha">Type I error rate.</param>
        ''' <param name="beta">Type II error rate.</param>
        ''' <param name="rSquaredWithOtherCovariates">
        ''' Proportion of variance in the binary covariate explained by the remaining covariates.
        ''' Typical value is 0 when planning an unadjusted or weakly correlated treatment effect.
        ''' </param>
        ''' <param name="overallEventProportion">
        ''' Optional overall event proportion during the study window. If omitted (NaN), only the required event count is returned.
        ''' </param>
        ''' <param name="twoSided">If True, uses a two-sided design; otherwise uses a one-sided design.</param>
        Public Function CalculateCoxEventCountBinaryCovariate(hazardRatio As Double,
                                                              controlToExperimentalRatio As Double,
                                                              alpha As Double,
                                                              beta As Double,
                                                              Optional rSquaredWithOtherCovariates As Double = 0.0,
                                                              Optional overallEventProportion As Double = Double.NaN,
                                                              Optional twoSided As Boolean = True) As CoxEventCountPlanningResult

            ValidatePositive(hazardRatio, NameOf(hazardRatio))
            ValidatePositive(controlToExperimentalRatio, NameOf(controlToExperimentalRatio))
            ValidateOpenUnitInterval(alpha, NameOf(alpha))
            ValidateOpenUnitInterval(beta, NameOf(beta))
            ValidateUnitIntervalExcludingOne(rSquaredWithOtherCovariates, NameOf(rSquaredWithOtherCovariates))

            If hazardRatio = 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(hazardRatio), "Hazard ratio must differ from 1.0 for event-count planning.")
            End If

            Dim pControl As Double
            Dim pExperimental As Double
            GetAllocationProportions(controlToExperimentalRatio, pControl, pExperimental)

            Dim effectiveVariance As Double = pControl * pExperimental
            Dim requiredEvents As Integer = CInt(Math.Ceiling(CalculateRequiredEventsFromHazardRatio(hazardRatio,
                                                                                                     pControl,
                                                                                                     pExperimental,
                                                                                                     alpha,
                                                                                                     beta,
                                                                                                     twoSided,
                                                                                                     rSquaredWithOtherCovariates)))

            Dim nSubjects As Integer = 0
            If Not Double.IsNaN(overallEventProportion) Then
                ValidateOpenUnitInterval(overallEventProportion, NameOf(overallEventProportion))
                nSubjects = CInt(Math.Ceiling(requiredEvents / overallEventProportion))
            End If

            Return New CoxEventCountPlanningResult With {
                .RequiredEvents = requiredEvents,
                .EstimatedNumberOfSubjects = nSubjects,
                .OverallEventProportion = If(Double.IsNaN(overallEventProportion), Double.NaN, overallEventProportion),
                .LogHazardRatio = Math.Log(hazardRatio),
                .EffectiveVariance = effectiveVariance,
                .RSquaredWithOtherCovariates = rSquaredWithOtherCovariates
            }
        End Function

        ''' <summary>
        ''' Estimates the required number of events for a Cox proportional hazards model with a continuous covariate,
        ''' and optionally inflates this to a total sample size when an overall event proportion is supplied.
        ''' </summary>
        ''' <param name="hazardRatioPerUnit">Anticipated hazard ratio for a one-unit increase in the covariate. Must be positive and not equal to 1.</param>
        ''' <param name="covariateSd">Standard deviation of the covariate in the target population. Must be positive.</param>
        ''' <param name="alpha">Type I error rate.</param>
        ''' <param name="beta">Type II error rate.</param>
        ''' <param name="rSquaredWithOtherCovariates">Proportion of covariate variance explained by the remaining covariates.</param>
        ''' <param name="overallEventProportion">Optional overall event proportion during follow-up. If omitted (NaN), only the event count is returned.</param>
        ''' <param name="twoSided">If True, uses a two-sided design; otherwise uses a one-sided design.</param>
        Public Function CalculateCoxEventCountContinuousCovariate(hazardRatioPerUnit As Double,
                                                                  covariateSd As Double,
                                                                  alpha As Double,
                                                                  beta As Double,
                                                                  Optional rSquaredWithOtherCovariates As Double = 0.0,
                                                                  Optional overallEventProportion As Double = Double.NaN,
                                                                  Optional twoSided As Boolean = True) As CoxEventCountPlanningResult

            ValidatePositive(hazardRatioPerUnit, NameOf(hazardRatioPerUnit))
            ValidatePositive(covariateSd, NameOf(covariateSd))
            ValidateOpenUnitInterval(alpha, NameOf(alpha))
            ValidateOpenUnitInterval(beta, NameOf(beta))
            ValidateUnitIntervalExcludingOne(rSquaredWithOtherCovariates, NameOf(rSquaredWithOtherCovariates))

            If hazardRatioPerUnit = 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(hazardRatioPerUnit), "Hazard ratio must differ from 1.0 for event-count planning.")
            End If

            Dim zAlpha As Double = GetCriticalNormal(alpha, twoSided)
            Dim zBeta As Double = distributions.NormSInv(1.0 - beta)
            Dim logHr As Double = Math.Log(hazardRatioPerUnit)
            Dim effectiveVariance As Double = covariateSd * covariateSd
            Dim attenuation As Double = 1.0 - rSquaredWithOtherCovariates

            Dim requiredEventsRaw As Double = ((zAlpha + zBeta) * (zAlpha + zBeta)) /
                                              (attenuation * effectiveVariance * logHr * logHr)

            Dim requiredEvents As Integer = CInt(Math.Ceiling(requiredEventsRaw))

            Dim nSubjects As Integer = 0
            If Not Double.IsNaN(overallEventProportion) Then
                ValidateOpenUnitInterval(overallEventProportion, NameOf(overallEventProportion))
                nSubjects = CInt(Math.Ceiling(requiredEvents / overallEventProportion))
            End If

            Return New CoxEventCountPlanningResult With {
                .RequiredEvents = requiredEvents,
                .EstimatedNumberOfSubjects = nSubjects,
                .OverallEventProportion = If(Double.IsNaN(overallEventProportion), Double.NaN, overallEventProportion),
                .LogHazardRatio = logHr,
                .EffectiveVariance = effectiveVariance,
                .RSquaredWithOtherCovariates = rSquaredWithOtherCovariates
            }
        End Function

        ' -----------------------------------------------------------------------------------------------------
        ' Equivalence / non-inferiority planning
        ' -----------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required group sizes for a non-inferiority comparison of two independent means.
        ''' </summary>
        ''' <param name="expectedDifference">Expected mean difference (experimental - control).</param>
        ''' <param name="nonInferiorityMargin">
        ''' Non-inferiority margin on the same difference scale. The function tests that
        ''' expectedDifference is greater than this margin using a one-sided alpha.
        ''' </param>
        ''' <param name="sd">Common standard deviation.</param>
        ''' <param name="controlToExperimentalRatio">Allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alphaOneSided">One-sided type I error rate.</param>
        ''' <param name="beta">Type II error rate.</param>
        Public Function CalculateNonInferiorityUnpairedTTest(expectedDifference As Double,
                                                             nonInferiorityMargin As Double,
                                                             sd As Double,
                                                             controlToExperimentalRatio As Double,
                                                             alphaOneSided As Double,
                                                             beta As Double) As UnpairedTTestSampleSizeResult

            Dim distanceToMargin As Double = expectedDifference - nonInferiorityMargin
            If distanceToMargin <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(expectedDifference), "Expected difference must lie on the favorable side of the non-inferiority margin.")
            End If

            Return SampleSizeCalculator.CalculateUnpairedTTest(distanceToMargin,
                                                               sd,
                                                               controlToExperimentalRatio,
                                                               OneSidedAlphaToTwoSidedEquivalent(alphaOneSided),
                                                               beta)
        End Function

        ''' <summary>
        ''' Estimates the required group sizes for a TOST-style equivalence comparison of two independent means.
        ''' </summary>
        ''' <param name="expectedDifference">Expected mean difference (experimental - control).</param>
        ''' <param name="lowerMargin">Lower equivalence margin.</param>
        ''' <param name="upperMargin">Upper equivalence margin.</param>
        ''' <param name="sd">Common standard deviation.</param>
        ''' <param name="controlToExperimentalRatio">Allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alphaOneSided">One-sided alpha for each TOST component.</param>
        ''' <param name="beta">Type II error rate.</param>
        Public Function CalculateEquivalenceUnpairedTTest(expectedDifference As Double,
                                                          lowerMargin As Double,
                                                          upperMargin As Double,
                                                          sd As Double,
                                                          controlToExperimentalRatio As Double,
                                                          alphaOneSided As Double,
                                                          beta As Double) As EquivalenceUnpairedTTestSampleSizeResult

            If lowerMargin >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerMargin), "Lower equivalence margin must be less than upper equivalence margin.")
            End If
            If expectedDifference <= lowerMargin OrElse expectedDifference >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(expectedDifference), "Expected difference must lie strictly inside the equivalence margins.")
            End If

            Dim lowerResult As UnpairedTTestSampleSizeResult =
                CalculateNonInferiorityUnpairedTTest(expectedDifference,
                                                     lowerMargin,
                                                     sd,
                                                     controlToExperimentalRatio,
                                                     alphaOneSided,
                                                     beta)

            Dim upperDistanceToMargin As Double = upperMargin - expectedDifference
            Dim upperResult As UnpairedTTestSampleSizeResult =
                SampleSizeCalculator.CalculateUnpairedTTest(upperDistanceToMargin,
                                                           sd,
                                                           controlToExperimentalRatio,
                                                           OneSidedAlphaToTwoSidedEquivalent(alphaOneSided),
                                                           beta)

            Dim finalControls As Integer = Math.Max(lowerResult.NumberOfControls, upperResult.NumberOfControls)
            Dim finalExperimental As Integer = Math.Max(lowerResult.NumberOfExperimental, upperResult.NumberOfExperimental)

            Return New EquivalenceUnpairedTTestSampleSizeResult With {
                .LowerBoundNumberOfControls = lowerResult.NumberOfControls,
                .LowerBoundNumberOfExperimental = lowerResult.NumberOfExperimental,
                .UpperBoundNumberOfControls = upperResult.NumberOfControls,
                .UpperBoundNumberOfExperimental = upperResult.NumberOfExperimental,
                .NumberOfControls = finalControls,
                .NumberOfExperimental = finalExperimental,
                .DrivingBound = If(finalExperimental = lowerResult.NumberOfExperimental AndAlso finalControls = lowerResult.NumberOfControls,
                                   "Lower bound",
                                   "Upper bound")
            }
        End Function

        ''' <summary>
        ''' Estimates the required group sizes for a non-inferiority comparison of two independent proportions.
        ''' </summary>
        ''' <param name="controlProp">Expected control-group proportion.</param>
        ''' <param name="experimentalProp">Expected experimental-group proportion.</param>
        ''' <param name="nonInferiorityMargin">
        ''' Non-inferiority margin on the difference scale (experimental - control).
        ''' For example, -0.10 means the experimental proportion may be up to 10 percentage points lower than control.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">Allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alphaOneSided">One-sided alpha.</param>
        ''' <param name="beta">Type II error rate.</param>
        Public Function CalculateNonInferiorityIndependentProportions(controlProp As Double,
                                                                     experimentalProp As Double,
                                                                     nonInferiorityMargin As Double,
                                                                     controlToExperimentalRatio As Double,
                                                                     alphaOneSided As Double,
                                                                     beta As Double) As IndependentProportionsSampleSizeResult

            Return CalculateMarginBasedIndependentProportions(controlProp,
                                                             experimentalProp,
                                                             nonInferiorityMargin,
                                                             controlToExperimentalRatio,
                                                             alphaOneSided,
                                                             beta)
        End Function

        ''' <summary>
        ''' Estimates the required group sizes for a TOST-style equivalence comparison of two independent proportions.
        ''' </summary>
        ''' <param name="controlProp">Expected control-group proportion.</param>
        ''' <param name="experimentalProp">Expected experimental-group proportion.</param>
        ''' <param name="lowerMargin">Lower equivalence margin on the difference scale (experimental - control).</param>
        ''' <param name="upperMargin">Upper equivalence margin on the difference scale (experimental - control).</param>
        ''' <param name="controlToExperimentalRatio">Allocation ratio: controls / experimental subjects.</param>
        ''' <param name="alphaOneSided">One-sided alpha for each TOST component.</param>
        ''' <param name="beta">Type II error rate.</param>
        Public Function CalculateEquivalenceIndependentProportions(controlProp As Double,
                                                                  experimentalProp As Double,
                                                                  lowerMargin As Double,
                                                                  upperMargin As Double,
                                                                  controlToExperimentalRatio As Double,
                                                                  alphaOneSided As Double,
                                                                  beta As Double) As EquivalenceIndependentProportionsSampleSizeResult

            If lowerMargin >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(lowerMargin), "Lower equivalence margin must be less than upper equivalence margin.")
            End If

            Dim observedDifference As Double = experimentalProp - controlProp
            If observedDifference <= lowerMargin OrElse observedDifference >= upperMargin Then
                Throw New ArgumentOutOfRangeException(NameOf(experimentalProp), "Expected proportion difference must lie strictly inside the equivalence margins.")
            End If

            Dim lower As IndependentProportionsSampleSizeResult =
                CalculateMarginBasedIndependentProportions(controlProp,
                                                          experimentalProp,
                                                          lowerMargin,
                                                          controlToExperimentalRatio,
                                                          alphaOneSided,
                                                          beta)

            ' For the upper-bound TOST component, test -(experimental - control) > -upperMargin
            ' by swapping the groups and inverting the allocation ratio.
            Dim upperSwapped As IndependentProportionsSampleSizeResult =
                CalculateMarginBasedIndependentProportions(experimentalProp,
                                                          controlProp,
                                                          -upperMargin,
                                                          1.0 / controlToExperimentalRatio,
                                                          alphaOneSided,
                                                          beta)

            Dim upperMapped As New IndependentProportionsSampleSizeResult With {
                .UncorrectedNumberOfControls = upperSwapped.UncorrectedNumberOfExperimental,
                .UncorrectedNumberOfExperimental = upperSwapped.UncorrectedNumberOfControls,
                .CorrectedNumberOfControls = upperSwapped.CorrectedNumberOfExperimental,
                .CorrectedNumberOfExperimental = upperSwapped.CorrectedNumberOfControls
            }

            Return New EquivalenceIndependentProportionsSampleSizeResult With {
                .LowerBoundUncorrectedNumberOfControls = lower.UncorrectedNumberOfControls,
                .LowerBoundUncorrectedNumberOfExperimental = lower.UncorrectedNumberOfExperimental,
                .LowerBoundCorrectedNumberOfControls = lower.CorrectedNumberOfControls,
                .LowerBoundCorrectedNumberOfExperimental = lower.CorrectedNumberOfExperimental,
                .UpperBoundUncorrectedNumberOfControls = upperMapped.UncorrectedNumberOfControls,
                .UpperBoundUncorrectedNumberOfExperimental = upperMapped.UncorrectedNumberOfExperimental,
                .UpperBoundCorrectedNumberOfControls = upperMapped.CorrectedNumberOfControls,
                .UpperBoundCorrectedNumberOfExperimental = upperMapped.CorrectedNumberOfExperimental,
                .UncorrectedNumberOfControls = Math.Max(lower.UncorrectedNumberOfControls, upperMapped.UncorrectedNumberOfControls),
                .UncorrectedNumberOfExperimental = Math.Max(lower.UncorrectedNumberOfExperimental, upperMapped.UncorrectedNumberOfExperimental),
                .CorrectedNumberOfControls = Math.Max(lower.CorrectedNumberOfControls, upperMapped.CorrectedNumberOfControls),
                .CorrectedNumberOfExperimental = Math.Max(lower.CorrectedNumberOfExperimental, upperMapped.CorrectedNumberOfExperimental),
                .DrivingBound = DetermineDrivingEquivalenceBound(lower, upperMapped)
            }
        End Function

        ' -----------------------------------------------------------------------------------------------------
        ' Reliability / agreement planning
        ' -----------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required number of subjects for testing whether an ICC exceeds a minimum acceptable value
        ''' using the one-way random-effects F-test framework.
        ''' </summary>
        ''' <param name="nullIcc">Minimum acceptable ICC under the null hypothesis.</param>
        ''' <param name="alternativeIcc">Target ICC under the alternative hypothesis. Must be greater than nullIcc.</param>
        ''' <param name="observationsPerSubject">Number of raters / repeated observations per subject. Must be at least 2.</param>
        ''' <param name="alpha">One-sided type I error rate.</param>
        ''' <param name="beta">Type II error rate. Power = 1 - beta.</param>
        Public Function CalculateIccHypothesisTestSampleSize(nullIcc As Double,
                                                             alternativeIcc As Double,
                                                             observationsPerSubject As Integer,
                                                             alpha As Double,
                                                             beta As Double) As IccHypothesisTestSampleSizeResult

            ValidateIccValue(nullIcc, NameOf(nullIcc))
            ValidateIccValue(alternativeIcc, NameOf(alternativeIcc))
            ValidateOpenUnitInterval(alpha, NameOf(alpha))
            ValidateOpenUnitInterval(beta, NameOf(beta))

            If observationsPerSubject < 2 Then
                Throw New ArgumentOutOfRangeException(NameOf(observationsPerSubject), "At least two observations per subject are required.")
            End If
            If alternativeIcc <= nullIcc Then
                Throw New ArgumentOutOfRangeException(NameOf(alternativeIcc), "Alternative ICC must be greater than the null ICC.")
            End If

            Dim targetPower As Double = 1.0 - beta
            Dim low As Integer = 2
            Dim high As Integer = 4

            Do While ComputeIccOneWayTestPower(high, observationsPerSubject, nullIcc, alternativeIcc, alpha) < targetPower
                high *= 2
                If high > 1000000 Then
                    Throw New InvalidOperationException("Unable to bracket the ICC sample size within the search limit.")
                End If
            Loop

            While low < high
                Dim mid As Integer = low + ((high - low) \ 2)
                Dim power As Double = ComputeIccOneWayTestPower(mid, observationsPerSubject, nullIcc, alternativeIcc, alpha)

                If power >= targetPower Then
                    high = mid
                Else
                    low = mid + 1
                End If
            End While

            Dim achievedPower As Double = ComputeIccOneWayTestPower(low, observationsPerSubject, nullIcc, alternativeIcc, alpha)

            Return New IccHypothesisTestSampleSizeResult With {
                .NumberOfSubjects = low,
                .NumberOfObservationsPerSubject = observationsPerSubject,
                .NullIcc = nullIcc,
                .AlternativeIcc = alternativeIcc,
                .AchievedPower = achievedPower
            }
        End Function

        ''' <summary>
        ''' Estimates the number of paired measurements required so that the confidence interval around either
        ''' Bland-Altman limit of agreement has a desired half-width, using the same approximate LoA standard error
        ''' already used in the agreement backend.
        ''' </summary>
        ''' <param name="sdDifference">Expected standard deviation of paired differences.</param>
        ''' <param name="desiredHalfWidth">Desired half-width of the confidence interval around a LoA, on the original measurement scale.</param>
        ''' <param name="alpha">Two-sided alpha used for the LoA confidence interval.</param>
        ''' <param name="loaMultiplier">LoA multiplier, typically 1.96 for 95% limits of agreement.</param>
        Public Function CalculateBlandAltmanLoASampleSize(sdDifference As Double,
                                                          desiredHalfWidth As Double,
                                                          alpha As Double,
                                                          Optional loaMultiplier As Double = 1.96) As BlandAltmanAgreementStudyPlanningResult

            ValidatePositive(sdDifference, NameOf(sdDifference))
            ValidatePositive(desiredHalfWidth, NameOf(desiredHalfWidth))
            ValidateOpenUnitInterval(alpha, NameOf(alpha))
            ValidatePositive(loaMultiplier, NameOf(loaMultiplier))

            Dim low As Integer = 3
            Dim high As Integer = 6

            Do While EstimateBlandAltmanLoAHalfWidth(high, sdDifference, alpha, loaMultiplier) > desiredHalfWidth
                high *= 2
                If high > 1000000 Then
                    Throw New InvalidOperationException("Unable to bracket the Bland-Altman sample size within the search limit.")
                End If
            Loop

            While low < high
                Dim mid As Integer = low + ((high - low) \ 2)
                Dim halfWidth As Double = EstimateBlandAltmanLoAHalfWidth(mid, sdDifference, alpha, loaMultiplier)

                If halfWidth <= desiredHalfWidth Then
                    high = mid
                Else
                    low = mid + 1
                End If
            End While

            Dim achievedHalfWidth As Double = EstimateBlandAltmanLoAHalfWidth(low, sdDifference, alpha, loaMultiplier)

            Return New BlandAltmanAgreementStudyPlanningResult With {
                .NumberOfPairs = low,
                .ExpectedSdOfDifferences = sdDifference,
                .DesiredHalfWidth = desiredHalfWidth,
                .AchievedHalfWidth = achievedHalfWidth,
                .Alpha = alpha,
                .LoAMultiplier = loaMultiplier
            }
        End Function


        ''' <summary>
        ''' Estimates the number of pairs required for a paired t-test.
        ''' </summary>
        ''' <param name="diff">
        ''' The expected mean difference between paired measurements.
        ''' </param>
        ''' <param name="sd">
        ''' The standard deviation of the paired differences.
        ''' </param>
        ''' <param name="alpha">
        ''' The two-sided type I error rate.
        ''' </param>
        ''' <param name="beta">
        ''' The type II error rate.
        ''' </param>
        ''' <returns>
        ''' A <see cref="PairedTTestSampleSizeResult"/> containing the estimated number of pairs.
        ''' </returns>
        ''' <remarks>
        ''' The calculation starts from a normal-approximation estimate and then iteratively
        ''' refines the required sample size using the t distribution.
        ''' The caller is expected to supply valid, non-degenerate inputs.
        ''' </remarks>
        Public Function CalculatePairedTTest(diff As Double, sd As Double, alpha As Double, beta As Double) As PairedTTestSampleSizeResult
            Dim crit As Double

            Dim nEst As Double = (sd * (distributions.NormSInv(1.0 - alpha / 2.0) + distributions.NormSInv(1.0 - beta)) / diff) ^ 2
            nEst = RoundUp(nEst, 0)

            Dim n As Integer = Int(nEst)

            If n > 1 Then
                For i = 0 To 1000
                    crit = (distributions.T_Inv(alpha / 2, n - 1) + distributions.T_Inv(beta, n - 1)) ^ 2 / (diff / sd) ^ 2
                    If CDbl(n) > crit Then Exit For
                    n += 1
                Next
            End If

            Dim result As New PairedTTestSampleSizeResult
            result.NumberOfPairs = n
            Return result
        End Function

        ''' <summary>
        ''' Estimates the group sizes required for an unpaired two-sample t-test.
        ''' </summary>
        ''' <param name="diff">
        ''' The expected mean difference between the two groups.
        ''' </param>
        ''' <param name="sd">
        ''' The common standard deviation assumed for both groups.
        ''' </param>
        ''' <param name="kappa">
        ''' The ratio of control subjects to experimental subjects.
        ''' </param>
        ''' <param name="alpha">
        ''' The two-sided type I error rate.
        ''' </param>
        ''' <param name="beta">
        ''' The type II error rate.
        ''' </param>
        ''' <returns>
        ''' A <see cref="UnpairedTTestSampleSizeResult"/> containing the estimated
        ''' control and experimental group sizes.
        ''' </returns>
        ''' <remarks>
        ''' The calculation starts from a normal-approximation estimate for the experimental group
        ''' and then iteratively refines it using the t distribution and the implied degrees of freedom.
        ''' The number of controls is derived from the final experimental count and <paramref name="kappa"/>.
        ''' </remarks>
        Public Function CalculateUnpairedTTest(diff As Double, sd As Double, kappa As Double, alpha As Double, beta As Double) As UnpairedTTestSampleSizeResult
            Dim crit As Double

            Dim nEst As Double = (1.0 + 1.0 / kappa) * (sd * (distributions.NormSInv(1.0 - alpha / 2.0) + distributions.NormSInv(1.0 - beta)) / diff) ^ 2
            nEst = RoundUp(nEst, 0)

            Dim nExperimental As Integer = Int(nEst)

            If nExperimental > 1 Then
                For i = 0 To 1000
                    crit = (1 + 1 / kappa) * (distributions.T_Inv(alpha / 2, nExperimental * (kappa + 1) - 2) + distributions.T_Inv(beta, nExperimental * (kappa + 1) - 2)) ^ 2 / (diff / sd) ^ 2
                    If CDbl(nExperimental) > crit Then Exit For
                    nExperimental += 1
                Next
            End If

            Dim result As New UnpairedTTestSampleSizeResult
            result.NumberOfControls = Int(nExperimental * kappa)
            result.NumberOfExperimental = nExperimental
            Return result
        End Function

        ''' <summary>
        ''' Estimates the required sample size for a one-sample proportion test.
        ''' </summary>
        ''' <param name="prop">
        ''' The anticipated population proportion under the alternative hypothesis.
        ''' </param>
        ''' <param name="h0Prop">
        ''' The null-hypothesis proportion to test against.
        ''' </param>
        ''' <param name="alpha">
        ''' The two-sided type I error rate.
        ''' </param>
        ''' <param name="beta">
        ''' The type II error rate.
        ''' </param>
        ''' <returns>
        ''' A <see cref="SingleProportionSampleSizeResult"/> containing the estimated number of subjects.
        ''' </returns>
        ''' <remarks>
        ''' This routine uses the normal approximation for a single proportion and rounds the result up
        ''' to the next whole subject count.
        ''' </remarks>
        Public Function CalculateSingleProportion(prop As Double, h0Prop As Double, alpha As Double, beta As Double) As SingleProportionSampleSizeResult
            Dim nEst As Double = prop * (1.0 - prop) * ((distributions.NormSInv(1.0 - alpha / 2.0) + distributions.NormSInv(1.0 - beta)) / (prop - h0Prop)) ^ 2
            nEst = RoundUp(nEst, 0)

            Dim result As New SingleProportionSampleSizeResult
            result.NumberOfSubjects = Int(nEst)
            Return result
        End Function

        ''' <summary>
        ''' Estimates the required group sizes for comparing two independent proportions.
        ''' </summary>
        ''' <param name="controlProp">
        ''' The anticipated proportion in the control group.
        ''' </param>
        ''' <param name="experimentalProp">
        ''' The anticipated proportion in the experimental group.
        ''' </param>
        ''' <param name="kappa">
        ''' The ratio of control subjects to experimental subjects.
        ''' </param>
        ''' <param name="alpha">
        ''' The two-sided type I error rate.
        ''' </param>
        ''' <param name="beta">
        ''' The type II error rate.
        ''' </param>
        ''' <returns>
        ''' An <see cref="IndependentProportionsSampleSizeResult"/> containing both
        ''' uncorrected and corrected sample size estimates for the two groups.
        ''' </returns>
        ''' <remarks>
        ''' The returned result includes:
        ''' <list type="bullet">
        '''   <item><description>Uncorrected estimates for the chi-square test.</description></item>
        '''   <item><description>Corrected estimates for the corrected chi-square or Fisher's exact test.</description></item>
        ''' </list>
        ''' The control counts are derived from the experimental counts and <paramref name="kappa"/>.
        ''' </remarks>
        Public Function CalculateIndependentProportions(controlProp As Double, experimentalProp As Double, kappa As Double, alpha As Double, beta As Double) As IndependentProportionsSampleSizeResult
            Dim pooledProp As Double = (controlProp + experimentalProp / kappa) / (1 + 1 / kappa)

            Dim uncorrectedNExperimental As Double = distributions.NormSInv(1.0 - alpha / 2.0) * Math.Sqrt((1.0 + kappa) * pooledProp * (1.0 - pooledProp))
            uncorrectedNExperimental = (uncorrectedNExperimental + (distributions.NormSInv(1.0 - beta) * Math.Sqrt(controlProp * (1.0 - controlProp) + kappa * experimentalProp * (1.0 - experimentalProp)))) ^ 2
            uncorrectedNExperimental = (uncorrectedNExperimental / (experimentalProp - controlProp) ^ 2) / kappa
            uncorrectedNExperimental = RoundUp(uncorrectedNExperimental, 0)

            Dim uncorrectedExperimental As Integer = Int(uncorrectedNExperimental)
            Dim correctedNExperimental As Double = (uncorrectedExperimental / 4.0) * (1.0 + Math.Sqrt(1.0 + (2.0 * (kappa + 1.0)) / (CDbl(uncorrectedExperimental) * kappa * Math.Abs(controlProp - experimentalProp)))) ^ 2
            Dim correctedExperimental As Integer = Int(RoundUp(correctedNExperimental, 0))

            Dim result As New IndependentProportionsSampleSizeResult
            result.UncorrectedNumberOfControls = Int(uncorrectedExperimental * kappa)
            result.UncorrectedNumberOfExperimental = uncorrectedExperimental
            result.CorrectedNumberOfControls = Int(correctedExperimental * kappa)
            result.CorrectedNumberOfExperimental = correctedExperimental
            Return result
        End Function

        ' -----------------------------------------------------------------------------------------------------
        ' Private helpers
        ' -----------------------------------------------------------------------------------------------------

        Private Function CalculateRequiredEventsFromHazardRatio(hazardRatio As Double,
                                                                pControl As Double,
                                                                pExperimental As Double,
                                                                alpha As Double,
                                                                beta As Double,
                                                                twoSided As Boolean,
                                                                rSquaredWithOtherCovariates As Double) As Double

            Dim zAlpha As Double = GetCriticalNormal(alpha, twoSided)
            Dim zBeta As Double = distributions.NormSInv(1.0 - beta)
            Dim logHr As Double = Math.Log(hazardRatio)
            Dim attenuation As Double = 1.0 - rSquaredWithOtherCovariates
            Dim informationPerEvent As Double = pControl * pExperimental

            Return ((zAlpha + zBeta) * (zAlpha + zBeta)) / (attenuation * informationPerEvent * logHr * logHr)
        End Function

        Private Function CalculateMarginBasedIndependentProportions(controlProp As Double,
                                                                   experimentalProp As Double,
                                                                   nullMargin As Double,
                                                                   controlToExperimentalRatio As Double,
                                                                   alphaOneSided As Double,
                                                                   beta As Double) As IndependentProportionsSampleSizeResult

            ValidateOpenUnitInterval(controlProp, NameOf(controlProp))
            ValidateOpenUnitInterval(experimentalProp, NameOf(experimentalProp))
            ValidatePositive(controlToExperimentalRatio, NameOf(controlToExperimentalRatio))
            ValidateOpenUnitInterval(alphaOneSided, NameOf(alphaOneSided))
            ValidateOpenUnitInterval(beta, NameOf(beta))

            Dim effectDistance As Double = (experimentalProp - controlProp) - nullMargin
            If effectDistance <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(experimentalProp), "The expected treatment effect must lie on the favorable side of the margin.")
            End If

            Dim nullExperimentalProp As Double = controlProp + nullMargin
            ValidateOpenUnitInterval(nullExperimentalProp, "nullBoundaryExperimentalProp")

            Dim pooledNull As Double = (controlProp + (nullExperimentalProp / controlToExperimentalRatio)) / (1.0 + (1.0 / controlToExperimentalRatio))
            ValidateOpenUnitInterval(pooledNull, "pooledNull")

            Dim zAlpha As Double = distributions.NormSInv(1.0 - alphaOneSided)
            Dim zBeta As Double = distributions.NormSInv(1.0 - beta)

            Dim uncorrectedNExperimental As Double = zAlpha * Math.Sqrt((1.0 + controlToExperimentalRatio) * pooledNull * (1.0 - pooledNull))
            uncorrectedNExperimental = (uncorrectedNExperimental + (zBeta * Math.Sqrt(controlProp * (1.0 - controlProp) + controlToExperimentalRatio * experimentalProp * (1.0 - experimentalProp)))) ^ 2
            uncorrectedNExperimental = (uncorrectedNExperimental / (effectDistance * effectDistance)) / controlToExperimentalRatio

            Dim uncorrectedExperimental As Integer = CInt(Math.Ceiling(uncorrectedNExperimental))
            Dim correctedNExperimental As Double = (uncorrectedExperimental / 4.0) * (1.0 + Math.Sqrt(1.0 + (2.0 * (controlToExperimentalRatio + 1.0)) / (CDbl(uncorrectedExperimental) * controlToExperimentalRatio * Math.Abs(effectDistance)))) ^ 2
            Dim correctedExperimental As Integer = CInt(Math.Ceiling(correctedNExperimental))

            Return New IndependentProportionsSampleSizeResult With {
                .UncorrectedNumberOfControls = CInt(Math.Ceiling(uncorrectedExperimental * controlToExperimentalRatio)),
                .UncorrectedNumberOfExperimental = uncorrectedExperimental,
                .CorrectedNumberOfControls = CInt(Math.Ceiling(correctedExperimental * controlToExperimentalRatio)),
                .CorrectedNumberOfExperimental = correctedExperimental
            }
        End Function

        Private Function ComputeIccOneWayTestPower(numberOfSubjects As Integer,
                                                   observationsPerSubject As Integer,
                                                   nullIcc As Double,
                                                   alternativeIcc As Double,
                                                   alpha As Double) As Double

            Dim df1 As Integer = numberOfSubjects - 1
            Dim df2 As Integer = numberOfSubjects * (observationsPerSubject - 1)

            Dim cNull As Double = 1.0 + (observationsPerSubject * nullIcc) / (1.0 - nullIcc)
            Dim cAlt As Double = 1.0 + (observationsPerSubject * alternativeIcc) / (1.0 - alternativeIcc)
            Dim scaling As Double = cNull / cAlt

            Dim fCritical As Double = distributions.F_Inv(1.0 - alpha, df1, df2)
            Dim threshold As Double = scaling * fCritical

            Return 1.0 - distributions.F_CDF(threshold, df1, df2)
        End Function

        Private Function EstimateBlandAltmanLoAHalfWidth(numberOfPairs As Integer,
                                                         sdDifference As Double,
                                                         alpha As Double,
                                                         loaMultiplier As Double) As Double

            If numberOfPairs < 3 Then
                Throw New ArgumentOutOfRangeException(NameOf(numberOfPairs), "At least three paired observations are required.")
            End If

            Dim crit As Double = distributions.T_Inv(1.0 - alpha / 2.0, Math.Max(1, numberOfPairs - 1))
            Dim seLoA As Double = sdDifference * Math.Sqrt((1.0 / numberOfPairs) + ((loaMultiplier * loaMultiplier) / (2.0 * Math.Max(1.0, numberOfPairs - 1.0))))

            Return crit * seLoA
        End Function

        Private Function DetermineDrivingEquivalenceBound(lower As IndependentProportionsSampleSizeResult,
                                                          upper As IndependentProportionsSampleSizeResult) As String
            Dim lowerTotal As Integer = lower.CorrectedNumberOfControls + lower.CorrectedNumberOfExperimental
            Dim upperTotal As Integer = upper.CorrectedNumberOfControls + upper.CorrectedNumberOfExperimental

            If lowerTotal >= upperTotal Then
                Return "Lower bound"
            Else
                Return "Upper bound"
            End If
        End Function

        Private Sub GetAllocationProportions(controlToExperimentalRatio As Double,
                                             ByRef pControl As Double,
                                             ByRef pExperimental As Double)
            pExperimental = 1.0 / (1.0 + controlToExperimentalRatio)
            pControl = controlToExperimentalRatio / (1.0 + controlToExperimentalRatio)
        End Sub

        Private Function GetCriticalNormal(alpha As Double, twoSided As Boolean) As Double
            If twoSided Then
                Return distributions.NormSInv(1.0 - alpha / 2.0)
            Else
                Return distributions.NormSInv(1.0 - alpha)
            End If
        End Function

        Private Function OneSidedAlphaToTwoSidedEquivalent(alphaOneSided As Double) As Double
            ValidateOpenUnitInterval(alphaOneSided, NameOf(alphaOneSided))

            Dim twoSidedEquivalent As Double = 2.0 * alphaOneSided
            If twoSidedEquivalent >= 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(alphaOneSided), "One-sided alpha must be below 0.5.")
            End If

            Return twoSidedEquivalent
        End Function

        Private Sub ValidatePositive(value As Double, paramName As String)
            If value <= 0.0 OrElse Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(paramName, "Value must be finite and > 0.")
            End If
        End Sub

        Private Sub ValidateUnitIntervalExcludingOne(value As Double, paramName As String)
            If value < 0.0 OrElse value >= 1.0 OrElse Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(paramName, "Value must satisfy 0 <= value < 1.")
            End If
        End Sub

        Private Sub ValidateIccValue(value As Double, paramName As String)
            If value < 0.0 OrElse value >= 1.0 OrElse Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(paramName, "ICC values for planning must satisfy 0 <= ICC < 1.")
            End If
        End Sub
    End Module

End Namespace