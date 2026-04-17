Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.SampleSizeCalc

<TestClass()>
Public Class SampleSizeCalculator_Tests

    Private Const DOUBLE_TOL As Double = 1.0E-8

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(5.0#, 10.0#, 0.05#, 0.2#, 34)>
    <DataRow(2.5#, 4.0#, 0.01#, 0.1#, 42)>
    Public Sub CalculatePairedTTest_matches_reference(diff As Double,
                                                      sd As Double,
                                                      alpha As Double,
                                                      beta As Double,
                                                      expectedPairs As Integer)
        Dim result As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(diff, sd, alpha, beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedPairs, result.NumberOfPairs)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculatePairedTTest_smaller_effect_requires_more_pairs()
        Dim largerEffect As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(5.0#, 10.0#, 0.05#, 0.2#)
        Dim smallerEffect As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(4.0#, 10.0#, 0.05#, 0.2#)

        Assert.IsTrue(smallerEffect.NumberOfPairs > largerEffect.NumberOfPairs)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(5.0#, 10.0#, 1.0#, 0.05#, 0.2#, 64, 64)>
    <DataRow(3.0#, 6.0#, 2.0#, 0.05#, 0.1#, 128, 64)>
    Public Sub CalculateUnpairedTTest_matches_reference(diff As Double,
                                                        sd As Double,
                                                        kappa As Double,
                                                        alpha As Double,
                                                        beta As Double,
                                                        expectedControls As Integer,
                                                        expectedExperimental As Integer)
        Dim result As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(diff, sd, kappa, alpha, beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedControls, result.NumberOfControls)
        Assert.AreEqual(expectedExperimental, result.NumberOfExperimental)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateUnpairedTTest_control_count_tracks_kappa()
        Dim balanced As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(5.0#, 10.0#, 1.0#, 0.05#, 0.2#)
        Dim twoToOne As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(3.0#, 6.0#, 2.0#, 0.05#, 0.1#)

        Assert.AreEqual(balanced.NumberOfExperimental, balanced.NumberOfControls)
        Assert.AreEqual(twoToOne.NumberOfExperimental * 2, twoToOne.NumberOfControls)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.6#, 0.5#, 0.05#, 0.2#, 189)>
    <DataRow(0.3#, 0.15#, 0.01#, 0.1#, 139)>
    Public Sub CalculateSingleProportion_matches_reference(prop As Double,
                                                           h0Prop As Double,
                                                           alpha As Double,
                                                           beta As Double,
                                                           expectedSubjects As Integer)
        Dim result As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(prop, h0Prop, alpha, beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedSubjects, result.NumberOfSubjects)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateSingleProportion_farther_from_null_requires_fewer_subjects()
        Dim nearNull As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(0.6#, 0.5#, 0.05#, 0.2#)
        Dim fartherFromNull As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(0.7#, 0.5#, 0.05#, 0.2#)

        Assert.IsTrue(fartherFromNull.NumberOfSubjects < nearNull.NumberOfSubjects)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.4#, 0.6#, 1.0#, 0.05#, 0.2#, 97, 97, 107, 107)>
    <DataRow(0.25#, 0.4#, 2.0#, 0.05#, 0.1#, 302, 151, 322, 161)>
    Public Sub CalculateIndependentProportions_matches_reference(controlProp As Double,
                                                                 experimentalProp As Double,
                                                                 kappa As Double,
                                                                 alpha As Double,
                                                                 beta As Double,
                                                                 expectedUncorrectedControls As Integer,
                                                                 expectedUncorrectedExperimental As Integer,
                                                                 expectedCorrectedControls As Integer,
                                                                 expectedCorrectedExperimental As Integer)
        Dim result As IndependentProportionsSampleSizeResult = SampleSizeCalculator.CalculateIndependentProportions(controlProp, experimentalProp, kappa, alpha, beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedUncorrectedControls, result.UncorrectedNumberOfControls)
        Assert.AreEqual(expectedUncorrectedExperimental, result.UncorrectedNumberOfExperimental)
        Assert.AreEqual(expectedCorrectedControls, result.CorrectedNumberOfControls)
        Assert.AreEqual(expectedCorrectedExperimental, result.CorrectedNumberOfExperimental)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateIndependentProportions_corrected_counts_are_not_smaller_than_uncorrected()
        Dim result As IndependentProportionsSampleSizeResult = SampleSizeCalculator.CalculateIndependentProportions(0.4#, 0.6#, 1.0#, 0.05#, 0.2#)

        Assert.IsTrue(result.CorrectedNumberOfControls >= result.UncorrectedNumberOfControls)
        Assert.IsTrue(result.CorrectedNumberOfExperimental >= result.UncorrectedNumberOfExperimental)
    End Sub

    ' -----------------------------------------------------------------------------------------
    ' New survival planning methods
    ' -----------------------------------------------------------------------------------------

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.7#, 0.4#, 0.3#, 1.0#, 0.05#, 0.2#, True, 247, 353, 353, 706, 0.35#)>
    <DataRow(0.75#, 0.5#, 0.35#, 2.0#, 0.05#, 0.1#, True, 572, 848, 424, 1272, 0.45#)>
    Public Sub CalculateLogRankSampleSize_matches_reference(hazardRatio As Double,
                                                            controlEventProportion As Double,
                                                            experimentalEventProportion As Double,
                                                            controlToExperimentalRatio As Double,
                                                            alpha As Double,
                                                            beta As Double,
                                                            twoSided As Boolean,
                                                            expectedEvents As Integer,
                                                            expectedControls As Integer,
                                                            expectedExperimental As Integer,
                                                            expectedTotal As Integer,
                                                            expectedAverageEventProportion As Double)
        Dim result As LogRankSampleSizeResult =
            SampleSizeCalculator.CalculateLogRankSampleSize(hazardRatio,
                                                            controlEventProportion,
                                                            experimentalEventProportion,
                                                            controlToExperimentalRatio,
                                                            alpha,
                                                            beta,
                                                            twoSided)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedEvents, result.RequiredEvents)
        Assert.AreEqual(expectedControls, result.NumberOfControls)
        Assert.AreEqual(expectedExperimental, result.NumberOfExperimental)
        Assert.AreEqual(expectedTotal, result.TotalNumberOfSubjects)
        Assert.AreEqual(expectedAverageEventProportion, result.AverageEventProportion, DOUBLE_TOL)
        Assert.AreEqual(CDbl(expectedControls) / expectedTotal, result.ControlAllocationProportion, DOUBLE_TOL)
        Assert.AreEqual(CDbl(expectedExperimental) / expectedTotal, result.ExperimentalAllocationProportion, DOUBLE_TOL)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateLogRankSampleSize_stronger_effect_requires_fewer_events()
        Dim weaker As LogRankSampleSizeResult = SampleSizeCalculator.CalculateLogRankSampleSize(0.8#, 0.4#, 0.3#, 1.0#, 0.05#, 0.2#)
        Dim stronger As LogRankSampleSizeResult = SampleSizeCalculator.CalculateLogRankSampleSize(0.6#, 0.4#, 0.3#, 1.0#, 0.05#, 0.2#)

        Assert.IsTrue(stronger.RequiredEvents < weaker.RequiredEvents)
        Assert.IsTrue(stronger.TotalNumberOfSubjects < weaker.TotalNumberOfSubjects)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateLogRankSampleSize_invalid_hazard_ratio_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   SampleSizeCalculator.CalculateLogRankSampleSize(1.0#, 0.4#, 0.3#, 1.0#, 0.05#, 0.2#)
                                                               End Sub)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.7#, 1.0#, 0.05#, 0.2#, 0.0#, 0.35#, True, 247, 706, 0.25#)>
    <DataRow(1.5#, 2.0#, 0.01#, 0.1#, 0.25#, 0.4#, False, 476, 1190, 0.2222222222222222#)>
    Public Sub CalculateCoxEventCountBinaryCovariate_matches_reference(hazardRatio As Double,
                                                                        controlToExperimentalRatio As Double,
                                                                        alpha As Double,
                                                                        beta As Double,
                                                                        rSquaredWithOtherCovariates As Double,
                                                                        overallEventProportion As Double,
                                                                        twoSided As Boolean,
                                                                        expectedEvents As Integer,
                                                                        expectedSubjects As Integer,
                                                                        expectedEffectiveVariance As Double)
        Dim result As CoxEventCountPlanningResult =
            SampleSizeCalculator.CalculateCoxEventCountBinaryCovariate(hazardRatio,
                                                                       controlToExperimentalRatio,
                                                                       alpha,
                                                                       beta,
                                                                       rSquaredWithOtherCovariates,
                                                                       overallEventProportion,
                                                                       twoSided)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedEvents, result.RequiredEvents)
        Assert.AreEqual(expectedSubjects, result.EstimatedNumberOfSubjects)
        Assert.AreEqual(expectedEffectiveVariance, result.EffectiveVariance, DOUBLE_TOL)
        Assert.AreEqual(Math.Log(hazardRatio), result.LogHazardRatio, DOUBLE_TOL)
        Assert.AreEqual(rSquaredWithOtherCovariates, result.RSquaredWithOtherCovariates, DOUBLE_TOL)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateCoxEventCountBinaryCovariate_without_overall_event_only_returns_event_count()
        Dim result As CoxEventCountPlanningResult =
            SampleSizeCalculator.CalculateCoxEventCountBinaryCovariate(0.7#, 1.0#, 0.05#, 0.2#)

        Assert.AreEqual(247, result.RequiredEvents)
        Assert.AreEqual(0, result.EstimatedNumberOfSubjects)
        Assert.IsTrue(Double.IsNaN(result.OverallEventProportion))
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(1.2#, 0.8#, 0.05#, 0.2#, 0.0#, 0.3#, True, 369, 1230, 0.64#)>
    <DataRow(0.85#, 1.5#, 0.01#, 0.1#, 0.3#, 0.45#, False, 313, 696, 2.25#)>
    Public Sub CalculateCoxEventCountContinuousCovariate_matches_reference(hazardRatioPerUnit As Double,
                                                                            covariateSd As Double,
                                                                            alpha As Double,
                                                                            beta As Double,
                                                                            rSquaredWithOtherCovariates As Double,
                                                                            overallEventProportion As Double,
                                                                            twoSided As Boolean,
                                                                            expectedEvents As Integer,
                                                                            expectedSubjects As Integer,
                                                                            expectedEffectiveVariance As Double)
        Dim result As CoxEventCountPlanningResult =
            SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(hazardRatioPerUnit,
                                                                           covariateSd,
                                                                           alpha,
                                                                           beta,
                                                                           rSquaredWithOtherCovariates,
                                                                           overallEventProportion,
                                                                           twoSided)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedEvents, result.RequiredEvents)
        Assert.AreEqual(expectedSubjects, result.EstimatedNumberOfSubjects)
        Assert.AreEqual(expectedEffectiveVariance, result.EffectiveVariance, DOUBLE_TOL)
        Assert.AreEqual(Math.Log(hazardRatioPerUnit), result.LogHazardRatio, DOUBLE_TOL)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateCoxEventCountContinuousCovariate_higher_r_squared_requires_more_events()
        Dim unadjusted As CoxEventCountPlanningResult =
            SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(1.2#, 0.8#, 0.05#, 0.2#, 0.0#, 0.3#)
        Dim adjusted As CoxEventCountPlanningResult =
            SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(1.2#, 0.8#, 0.05#, 0.2#, 0.5#, 0.3#)

        Assert.IsTrue(adjusted.RequiredEvents > unadjusted.RequiredEvents)
        Assert.IsTrue(adjusted.EstimatedNumberOfSubjects > unadjusted.EstimatedNumberOfSubjects)
    End Sub

    ' -----------------------------------------------------------------------------------------
    ' New non-inferiority / equivalence methods
    ' -----------------------------------------------------------------------------------------

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(1.0#, -0.5#, 2.0#, 1.0#, 0.025#, 0.2#, 29, 29)>
    <DataRow(0.8#, -0.2#, 1.5#, 2.0#, 0.025#, 0.1#, 74, 37)>
    Public Sub CalculateNonInferiorityUnpairedTTest_matches_reference(expectedDifference As Double,
                                                                      nonInferiorityMargin As Double,
                                                                      sd As Double,
                                                                      controlToExperimentalRatio As Double,
                                                                      alphaOneSided As Double,
                                                                      beta As Double,
                                                                      expectedControls As Integer,
                                                                      expectedExperimental As Integer)
        Dim result As UnpairedTTestSampleSizeResult =
            SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(expectedDifference,
                                                                      nonInferiorityMargin,
                                                                      sd,
                                                                      controlToExperimentalRatio,
                                                                      alphaOneSided,
                                                                      beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedControls, result.NumberOfControls)
        Assert.AreEqual(expectedExperimental, result.NumberOfExperimental)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateNonInferiorityUnpairedTTest_stricter_margin_requires_more_subjects()
        Dim relaxed As UnpairedTTestSampleSizeResult =
            SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(1.0#, -1.0#, 2.0#, 1.0#, 0.025#, 0.2#)
        Dim stricter As UnpairedTTestSampleSizeResult =
            SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(1.0#, -0.2#, 2.0#, 1.0#, 0.025#, 0.2#)

        Assert.IsTrue(stricter.NumberOfControls > relaxed.NumberOfControls)
        Assert.IsTrue(stricter.NumberOfExperimental > relaxed.NumberOfExperimental)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.1#, -0.5#, 0.5#, 1.8#, 1.0#, 0.05#, 0.2#, 113, 113, 252, 252, 252, 252, "Upper bound")>
    <DataRow(-0.1#, -0.5#, 0.5#, 1.8#, 1.0#, 0.05#, 0.2#, 252, 252, 113, 113, 252, 252, "Lower bound")>
    Public Sub CalculateEquivalenceUnpairedTTest_matches_reference(expectedDifference As Double,
                                                                   lowerMargin As Double,
                                                                   upperMargin As Double,
                                                                   sd As Double,
                                                                   controlToExperimentalRatio As Double,
                                                                   alphaOneSided As Double,
                                                                   beta As Double,
                                                                   expectedLowerControls As Integer,
                                                                   expectedLowerExperimental As Integer,
                                                                   expectedUpperControls As Integer,
                                                                   expectedUpperExperimental As Integer,
                                                                   expectedControls As Integer,
                                                                   expectedExperimental As Integer,
                                                                   expectedDrivingBound As String)
        Dim result As EquivalenceUnpairedTTestSampleSizeResult =
            SampleSizeCalculator.CalculateEquivalenceUnpairedTTest(expectedDifference,
                                                                  lowerMargin,
                                                                  upperMargin,
                                                                  sd,
                                                                  controlToExperimentalRatio,
                                                                  alphaOneSided,
                                                                  beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedLowerControls, result.LowerBoundNumberOfControls)
        Assert.AreEqual(expectedLowerExperimental, result.LowerBoundNumberOfExperimental)
        Assert.AreEqual(expectedUpperControls, result.UpperBoundNumberOfControls)
        Assert.AreEqual(expectedUpperExperimental, result.UpperBoundNumberOfExperimental)
        Assert.AreEqual(expectedControls, result.NumberOfControls)
        Assert.AreEqual(expectedExperimental, result.NumberOfExperimental)
        Assert.AreEqual(expectedDrivingBound, result.DrivingBound)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.6#, 0.62#, -0.1#, 1.0#, 0.025#, 0.2#, 267, 267, 284, 284)>
    <DataRow(0.4#, 0.43#, -0.08#, 2.0#, 0.025#, 0.1#, 620, 310, 648, 324)>
    Public Sub CalculateNonInferiorityIndependentProportions_matches_reference(controlProp As Double,
                                                                               experimentalProp As Double,
                                                                               nonInferiorityMargin As Double,
                                                                               controlToExperimentalRatio As Double,
                                                                               alphaOneSided As Double,
                                                                               beta As Double,
                                                                               expectedUncorrectedControls As Integer,
                                                                               expectedUncorrectedExperimental As Integer,
                                                                               expectedCorrectedControls As Integer,
                                                                               expectedCorrectedExperimental As Integer)
        Dim result As IndependentProportionsSampleSizeResult =
            SampleSizeCalculator.CalculateNonInferiorityIndependentProportions(controlProp,
                                                                              experimentalProp,
                                                                              nonInferiorityMargin,
                                                                              controlToExperimentalRatio,
                                                                              alphaOneSided,
                                                                              beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedUncorrectedControls, result.UncorrectedNumberOfControls)
        Assert.AreEqual(expectedUncorrectedExperimental, result.UncorrectedNumberOfExperimental)
        Assert.AreEqual(expectedCorrectedControls, result.CorrectedNumberOfControls)
        Assert.AreEqual(expectedCorrectedExperimental, result.CorrectedNumberOfExperimental)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.5#, 0.52#, -0.1#, 0.1#, 1.0#, 0.05#, 0.2#,
             214, 214, 231, 231,
             482, 482, 507, 507,
             482, 482, 507, 507,
             "Upper bound")>
    <DataRow(0.5#, 0.48#, -0.1#, 0.1#, 1.0#, 0.05#, 0.2#,
             480, 480, 505, 505,
             212, 212, 229, 229,
             480, 480, 505, 505,
             "Lower bound")>
    Public Sub CalculateEquivalenceIndependentProportions_matches_reference(controlProp As Double,
                                                                            experimentalProp As Double,
                                                                            lowerMargin As Double,
                                                                            upperMargin As Double,
                                                                            controlToExperimentalRatio As Double,
                                                                            alphaOneSided As Double,
                                                                            beta As Double,
                                                                            expectedLowerUncorrectedControls As Integer,
                                                                            expectedLowerUncorrectedExperimental As Integer,
                                                                            expectedLowerCorrectedControls As Integer,
                                                                            expectedLowerCorrectedExperimental As Integer,
                                                                            expectedUpperUncorrectedControls As Integer,
                                                                            expectedUpperUncorrectedExperimental As Integer,
                                                                            expectedUpperCorrectedControls As Integer,
                                                                            expectedUpperCorrectedExperimental As Integer,
                                                                            expectedUncorrectedControls As Integer,
                                                                            expectedUncorrectedExperimental As Integer,
                                                                            expectedCorrectedControls As Integer,
                                                                            expectedCorrectedExperimental As Integer,
                                                                            expectedDrivingBound As String)
        Dim result As EquivalenceIndependentProportionsSampleSizeResult =
            SampleSizeCalculator.CalculateEquivalenceIndependentProportions(controlProp,
                                                                           experimentalProp,
                                                                           lowerMargin,
                                                                           upperMargin,
                                                                           controlToExperimentalRatio,
                                                                           alphaOneSided,
                                                                           beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedLowerUncorrectedControls, result.LowerBoundUncorrectedNumberOfControls)
        Assert.AreEqual(expectedLowerUncorrectedExperimental, result.LowerBoundUncorrectedNumberOfExperimental)
        Assert.AreEqual(expectedLowerCorrectedControls, result.LowerBoundCorrectedNumberOfControls)
        Assert.AreEqual(expectedLowerCorrectedExperimental, result.LowerBoundCorrectedNumberOfExperimental)
        Assert.AreEqual(expectedUpperUncorrectedControls, result.UpperBoundUncorrectedNumberOfControls)
        Assert.AreEqual(expectedUpperUncorrectedExperimental, result.UpperBoundUncorrectedNumberOfExperimental)
        Assert.AreEqual(expectedUpperCorrectedControls, result.UpperBoundCorrectedNumberOfControls)
        Assert.AreEqual(expectedUpperCorrectedExperimental, result.UpperBoundCorrectedNumberOfExperimental)
        Assert.AreEqual(expectedUncorrectedControls, result.UncorrectedNumberOfControls)
        Assert.AreEqual(expectedUncorrectedExperimental, result.UncorrectedNumberOfExperimental)
        Assert.AreEqual(expectedCorrectedControls, result.CorrectedNumberOfControls)
        Assert.AreEqual(expectedCorrectedExperimental, result.CorrectedNumberOfExperimental)
        Assert.AreEqual(expectedDrivingBound, result.DrivingBound)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateEquivalenceIndependentProportions_invalid_difference_outside_margins_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   SampleSizeCalculator.CalculateEquivalenceIndependentProportions(0.5#, 0.62#, -0.1#, 0.1#, 1.0#, 0.05#, 0.2#)
                                                               End Sub)
    End Sub

    ' -----------------------------------------------------------------------------------------
    ' New agreement / reliability planning methods
    ' -----------------------------------------------------------------------------------------

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(0.4#, 0.7#, 3, 0.05#, 0.2#, 20)>
    <DataRow(0.5#, 0.75#, 4, 0.05#, 0.1#, 26)>
    Public Sub CalculateIccHypothesisTestSampleSize_matches_reference(nullIcc As Double,
                                                                      alternativeIcc As Double,
                                                                      observationsPerSubject As Integer,
                                                                      alpha As Double,
                                                                      beta As Double,
                                                                      expectedSubjects As Integer)
        Dim result As IccHypothesisTestSampleSizeResult =
            SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(nullIcc,
                                                                     alternativeIcc,
                                                                     observationsPerSubject,
                                                                     alpha,
                                                                     beta)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedSubjects, result.NumberOfSubjects)
        Assert.AreEqual(observationsPerSubject, result.NumberOfObservationsPerSubject)
        Assert.AreEqual(nullIcc, result.NullIcc, DOUBLE_TOL)
        Assert.AreEqual(alternativeIcc, result.AlternativeIcc, DOUBLE_TOL)
        Assert.IsTrue(result.AchievedPower >= 1.0# - beta)
        Assert.IsTrue(result.AchievedPower < 1.0#)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateIccHypothesisTestSampleSize_larger_separation_requires_fewer_subjects()
        Dim smallerSeparation As IccHypothesisTestSampleSizeResult =
            SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(0.4#, 0.7#, 3, 0.05#, 0.2#)
        Dim largerSeparation As IccHypothesisTestSampleSizeResult =
            SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(0.4#, 0.8#, 3, 0.05#, 0.2#)

        Assert.IsTrue(largerSeparation.NumberOfSubjects < smallerSeparation.NumberOfSubjects)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateIccHypothesisTestSampleSize_invalid_order_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(0.6#, 0.6#, 3, 0.05#, 0.2#)
                                                               End Sub)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    <DataRow(12.0#, 4.0#, 0.05#, 1.96#, 105)>
    <DataRow(8.0#, 3.0#, 0.1#, 1.645#, 48)>
    Public Sub CalculateBlandAltmanLoASampleSize_matches_reference(sdDifference As Double,
                                                                   desiredHalfWidth As Double,
                                                                   alpha As Double,
                                                                   loaMultiplier As Double,
                                                                   expectedPairs As Integer)
        Dim result As BlandAltmanAgreementStudyPlanningResult =
            SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(sdDifference,
                                                                   desiredHalfWidth,
                                                                   alpha,
                                                                   loaMultiplier)

        Assert.IsNotNull(result)
        Assert.AreEqual(expectedPairs, result.NumberOfPairs)
        Assert.AreEqual(sdDifference, result.ExpectedSdOfDifferences, DOUBLE_TOL)
        Assert.AreEqual(desiredHalfWidth, result.DesiredHalfWidth, DOUBLE_TOL)
        Assert.AreEqual(alpha, result.Alpha, DOUBLE_TOL)
        Assert.AreEqual(loaMultiplier, result.LoAMultiplier, DOUBLE_TOL)
        Assert.IsTrue(result.AchievedHalfWidth <= desiredHalfWidth)
    End Sub

    <TestCategory("SampleSize")>
    <TestMethod()>
    Public Sub CalculateBlandAltmanLoASampleSize_tighter_precision_requires_more_pairs()
        Dim looser As BlandAltmanAgreementStudyPlanningResult =
            SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(12.0#, 5.0#, 0.05#)
        Dim tighter As BlandAltmanAgreementStudyPlanningResult =
            SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(12.0#, 4.0#, 0.05#)

        Assert.IsTrue(tighter.NumberOfPairs > looser.NumberOfPairs)
        Assert.IsTrue(tighter.AchievedHalfWidth <= tighter.DesiredHalfWidth)
    End Sub

End Class
