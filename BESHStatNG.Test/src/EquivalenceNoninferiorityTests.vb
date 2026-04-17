Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass()>
Public Class EquivalenceNonInferiorityMethods_Tests

    Private Const TOL As Double = 1.0E-6
    Private Const TOL_LOOSE As Double = 1.0E-5

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, message As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{message}: expected {expected} but got {actual}.")
        End If

        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{message}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub

    Private Shared Function BuildSimplePairsResult(referenceValues As Double(), testValues As Double()) As Agreement.BlandAltmanResult
        Dim opts As New Agreement.BlandAltmanOptions With {
            .Alpha = 0.05,
            .CiMethod = Agreement.AgreementCiMethod.Analytical,
            .UseTDistribution = True,
            .Mode = Agreement.RepeatedBlandAltmanMode.SimplePairs,
            .Scale = Agreement.BlandAltmanScale.RawDifference
        }

        Dim ba As New Agreement.BlandAltmanAgreement(referenceValues, testValues, "Reference", "Test", opts)
        Return ba.Fit()
    End Function

    Private Shared Function BuildRepeatedPercentResult(referenceValues As Double(),
                                                       testValues As Double(),
                                                       subjectIds As Object()) As Agreement.BlandAltmanResult
        Dim opts As New Agreement.BlandAltmanOptions With {
            .Alpha = 0.05,
            .CiMethod = Agreement.AgreementCiMethod.Analytical,
            .UseTDistribution = True,
            .Mode = Agreement.RepeatedBlandAltmanMode.RepeatedBySubject,
            .AllowFallbackToSimple = False,
            .Scale = Agreement.BlandAltmanScale.PercentOfMean,
            .SubjectIds = subjectIds,
            .MinSubjects = 2,
            .MinPairsPerSubject = 2
        }

        Dim ba As New Agreement.BlandAltmanAgreement(referenceValues, testValues, "Reference", "Test", opts)
        Return ba.Fit()
    End Function

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub AssessConfidenceIntervalAgainstMargins_full_interval_inside_limits()
        Dim ci As New ConfidenceIntervalResult With {
            .Estimate = 1.0#,
            .LowerLimit = 0.2#,
            .UpperLimit = 1.8#,
            .StdErr = 0.4#,
            .alpha = 0.05#
        }

        Dim result = equivalencetests.EquivalenceNonInferiorityMethods.AssessConfidenceIntervalAgainstMargins(ci, 0.0#, 2.0#)

        Assert.IsTrue(result.IsPointEstimateWithinMargins)
        Assert.IsTrue(result.IsConfidenceIntervalWithinMargins)
        Assert.IsTrue(result.SupportsLowerNonInferiority)
        Assert.IsTrue(result.SupportsUpperNonInferiority)
        Assert.AreEqual("The full confidence interval lies inside the decision limits.", result.Conclusion)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
<TestMethod()>
Public Sub AssessConfidenceIntervalAgainstMargins_lower_bound_noninferiority_conclusion()
    Dim ci As New ConfidenceIntervalResult With {
        .Estimate = 0.3#,
        .LowerLimit = -0.1#,
        .UpperLimit = 0.9#,
        .StdErr = 0.2#,
        .alpha = 0.05#
    }

    Dim result = equivalencetests.EquivalenceNonInferiorityMethods.AssessConfidenceIntervalAgainstMargins(ci, -0.5#, Double.PositiveInfinity)

    Assert.IsTrue(result.SupportsLowerNonInferiority)
    Assert.IsTrue(result.SupportsUpperNonInferiority)
        Assert.AreEqual("The full confidence interval lies inside the decision limits.", result.Conclusion)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestUnpairedMeansNonInferiorityFromSummary_matches_reference_values()
        Dim result = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiorityFromSummary(
            100.0#, 15.0#, 30,
            103.0#, 14.0#, 32,
            5.0#, 0.025#, False)

        Assert.AreEqual(30, result.NumberOfControls)
        Assert.AreEqual(32, result.NumberOfExperimental)
        AssertAlmostEqual(3.0#, result.DifferenceExperimentalMinusControl, TOL, "Mean NI difference")
        AssertAlmostEqual(3.69120576505835#, result.StandardError, TOL_LOOSE, "Mean NI SE")
        AssertAlmostEqual(58.9365885150831#, result.DegreesOfFreedom, TOL_LOOSE, "Mean NI df")
        AssertAlmostEqual(2.16731347673151#, result.TestStatistic, TOL_LOOSE, "Mean NI t statistic")
        AssertAlmostEqual(0.0171316530760939#, result.PValue, TOL_LOOSE, "Mean NI p-value")
        AssertAlmostEqual(-4.38625195460417#, result.LowerOneSidedConfidenceLimit, TOL_LOOSE, "Mean NI lower one-sided limit")
        Assert.IsTrue(result.SupportsNonInferiority)
        Assert.IsTrue(result.CiAssessment.SupportsLowerNonInferiority)
        Assert.AreEqual("Supports non-inferiority: the lower one-sided confidence limit is above the non-inferiority limit.", result.Conclusion)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestUnpairedMeansNonInferiority_sample_wrapper_matches_summary()
        Dim controlSample As Double() = {10.0#, 12.0#, 11.0#, 9.0#, 10.0#}
        Dim experimentalSample As Double() = {12.0#, 13.0#, 12.0#, 11.0#, 14.0#}

        Dim fromSamples = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiority(controlSample,
                                                                                                             experimentalSample,
                                                                                                             1.5#,
                                                                                                             0.025#,
                                                                                                             False)

        Dim fromSummary = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansNonInferiorityFromSummary(
            10.4#, 1.14017542509914#, 5,
            12.4#, 1.14017542509914#, 5,
            1.5#, 0.025#, False)

        AssertAlmostEqual(fromSummary.DifferenceExperimentalMinusControl, fromSamples.DifferenceExperimentalMinusControl, TOL, "Sample vs summary NI difference")
        AssertAlmostEqual(fromSummary.StandardError, fromSamples.StandardError, TOL, "Sample vs summary NI SE")
        AssertAlmostEqual(fromSummary.DegreesOfFreedom, fromSamples.DegreesOfFreedom, TOL, "Sample vs summary NI df")
        AssertAlmostEqual(fromSummary.PValue, fromSamples.PValue, TOL, "Sample vs summary NI p-value")
        Assert.AreEqual(fromSummary.SupportsNonInferiority, fromSamples.SupportsNonInferiority)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestUnpairedMeansEquivalenceFromSummary_matches_reference_values()
        Dim result = equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalenceFromSummary(
            100.0#, 8.0#, 50,
            101.0#, 8.0#, 50,
            -5.0#, 5.0#,
            0.025#, True)

        Assert.AreEqual(50, result.NumberOfControls)
        Assert.AreEqual(50, result.NumberOfExperimental)
        AssertAlmostEqual(1.0#, result.DifferenceExperimentalMinusControl, TOL, "Mean equivalence difference")
        AssertAlmostEqual(1.6#, result.StandardError, TOL, "Mean equivalence SE")
        AssertAlmostEqual(98.0#, result.DegreesOfFreedom, TOL, "Mean equivalence df")
        AssertAlmostEqual(3.75#, result.LowerComponentStatistic, TOL, "Mean equivalence lower statistic")
        AssertAlmostEqual(0.000149661573621396#, result.LowerComponentPValue, TOL_LOOSE, "Mean equivalence lower p-value")
        AssertAlmostEqual(-2.5#, result.UpperComponentStatistic, TOL, "Mean equivalence upper statistic")
        AssertAlmostEqual(0.00703987768738599#, result.UpperComponentPValue, TOL_LOOSE, "Mean equivalence upper p-value")
        AssertAlmostEqual(0.00703987768738599#, result.TostPValue, TOL_LOOSE, "Mean equivalence TOST p-value")
        Assert.IsTrue(result.SupportsEquivalence)
        Assert.IsTrue(result.CiAssessment.IsConfidenceIntervalWithinMargins)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestUnpairedMeansEquivalence_difference_outside_margins_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   equivalencetests.EquivalenceNonInferiorityMethods.TestUnpairedMeansEquivalenceFromSummary(
                                                                       100.0#, 10.0#, 20,
                                                                       107.0#, 10.0#, 20,
                                                                       -5.0#, 5.0#,
                                                                       0.025#, True)
                                                               End Sub)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestIndependentProportionsNonInferiority_matches_reference_values()
        Dim result = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsNonInferiority(
            180, 300,
            176, 300,
            0.1#, 0.025#)

        Assert.AreEqual(300, result.NumberOfControls)
        Assert.AreEqual(300, result.NumberOfExperimental)
        AssertAlmostEqual(0.6#, result.ControlProportion, TOL, "Proportion NI control proportion")
        AssertAlmostEqual(0.586666666666667#, result.ExperimentalProportion, TOL_LOOSE, "Proportion NI experimental proportion")
        AssertAlmostEqual(-0.0133333333333333#, result.DifferenceExperimentalMinusControl, TOL_LOOSE, "Proportion NI difference")
        AssertAlmostEqual(0.0401035696203754#, result.StandardError, TOL_LOOSE, "Proportion NI SE")
        AssertAlmostEqual(2.16107113374352#, result.ZStatistic, TOL_LOOSE, "Proportion NI z statistic")
        AssertAlmostEqual(0.0153449224973425#, result.PValue, TOL_LOOSE, "Proportion NI p-value")
        AssertAlmostEqual(-0.0919348854407637#, result.LowerOneSidedConfidenceLimit, TOL_LOOSE, "Proportion NI lower one-sided limit")
        Assert.IsTrue(result.SupportsNonInferiority)
        Assert.IsTrue(result.CiAssessment.SupportsLowerNonInferiority)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestIndependentProportionsEquivalence_matches_reference_values()
        Dim result = equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsEquivalence(
            180, 300,
            183, 300,
            -0.1#, 0.1#,
            0.025#)

        Assert.AreEqual(300, result.NumberOfControls)
        Assert.AreEqual(300, result.NumberOfExperimental)
        AssertAlmostEqual(0.01#, result.DifferenceExperimentalMinusControl, TOL_LOOSE, "Proportion equivalence difference")
        AssertAlmostEqual(0.0399124040869502#, result.StandardError, TOL_LOOSE, "Proportion equivalence SE")
        AssertAlmostEqual(2.75603543601037#, result.LowerComponentStatistic, TOL_LOOSE, "Proportion equivalence lower statistic")
        AssertAlmostEqual(0.00292533290412278#, result.LowerComponentPValue, TOL_LOOSE, "Proportion equivalence lower p-value")
        AssertAlmostEqual(-2.25493808400849#, result.UpperComponentStatistic, TOL_LOOSE, "Proportion equivalence upper statistic")
        AssertAlmostEqual(0.0120686077099803#, result.UpperComponentPValue, TOL_LOOSE, "Proportion equivalence upper p-value")
        AssertAlmostEqual(0.0120686077099803#, result.TostPValue, TOL_LOOSE, "Proportion equivalence TOST p-value")
        Assert.IsTrue(result.SupportsEquivalence)
        Assert.IsTrue(result.CiAssessment.IsConfidenceIntervalWithinMargins)
    End Sub

    <TestCategory("EquivalenceNoninferiority")>
    <TestMethod()>
    Public Sub TestIndependentProportionsEquivalence_difference_outside_margins_throws()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   equivalencetests.EquivalenceNonInferiorityMethods.TestIndependentProportionsEquivalence(
                                                                       180, 300,
                                                                       240, 300,
                                                                       -0.1#, 0.1#,
                                                                       0.025#)
                                                               End Sub)
    End Sub

    <TestCategory("Agreement")>
    <TestMethod()>
    Public Sub AssessAllowableBias_fitted_result_overload_matches_raw_array_overload_for_simple_pairs()
        Dim referenceValues As Double() = {100.0#, 102.0#, 98.0#, 101.0#, 99.0#, 103.0#}
        Dim testValues As Double() = {101.0#, 101.0#, 99.0#, 100.0#, 100.0#, 104.0#}

        Dim fromArrays = equivalencetests.EquivalenceNonInferiorityMethods.AssessAllowableBias(referenceValues,
                                                                                                testValues,
                                                                                                -2.0#,
                                                                                                2.0#,
                                                                                                0.05#)

        Dim fit = BuildSimplePairsResult(referenceValues, testValues)
        Dim fromFit = equivalencetests.EquivalenceNonInferiorityMethods.AssessAllowableBias(fit, -2.0#, 2.0#)

        Assert.AreEqual(fromArrays.IsPointEstimateWithinMargins, fromFit.IsPointEstimateWithinMargins)
        Assert.AreEqual(fromArrays.IsConfidenceIntervalWithinMargins, fromFit.IsConfidenceIntervalWithinMargins)
        Assert.AreEqual(fromArrays.SupportsLowerNonInferiority, fromFit.SupportsLowerNonInferiority)
        Assert.AreEqual(fromArrays.SupportsUpperNonInferiority, fromFit.SupportsUpperNonInferiority)
        AssertAlmostEqual(fromArrays.ConfidenceInterval.Estimate, fromFit.ConfidenceInterval.Estimate, TOL, "Allowable bias estimate")
        AssertAlmostEqual(fromArrays.ConfidenceInterval.LowerLimit, fromFit.ConfidenceInterval.LowerLimit, TOL, "Allowable bias lower CI")
        AssertAlmostEqual(fromArrays.ConfidenceInterval.UpperLimit, fromFit.ConfidenceInterval.UpperLimit, TOL, "Allowable bias upper CI")
    End Sub

    <TestCategory("Agreement")>
    <TestMethod()>
    Public Sub AssessBlandAltmanAgainstDecisionLimits_fitted_result_overload_matches_raw_array_overload_for_simple_pairs()
        Dim referenceValues As Double() = {100.0#, 102.0#, 98.0#, 101.0#, 99.0#, 103.0#}
        Dim testValues As Double() = {101.0#, 101.0#, 99.0#, 100.0#, 100.0#, 104.0#}

        Dim fromArrays = equivalencetests.EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(referenceValues,
                                                                                                                    testValues,
                                                                                                                    -5.0#,
                                                                                                                    5.0#,
                                                                                                                    0.05#)

        Dim fit = BuildSimplePairsResult(referenceValues, testValues)
        Dim fromFit = equivalencetests.EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(fit, -5.0#, 5.0#)

        Assert.AreEqual(fromArrays.AreObservedLoAWithinAllowableLimits, fromFit.AreObservedLoAWithinAllowableLimits)
        Assert.AreEqual(fromArrays.AreLoAConfidenceIntervalsWithinAllowableLimits, fromFit.AreLoAConfidenceIntervalsWithinAllowableLimits)
        Assert.AreEqual(fromArrays.BiasAssessment.IsConfidenceIntervalWithinMargins, fromFit.BiasAssessment.IsConfidenceIntervalWithinMargins)
        AssertAlmostEqual(fromArrays.BlandAltman.BiasCI.Estimate, fromFit.BlandAltman.BiasCI.Estimate, TOL, "Bland-Altman decision bias estimate")
        AssertAlmostEqual(fromArrays.BlandAltman.LowerLoACI.Estimate, fromFit.BlandAltman.LowerLoACI.Estimate, TOL, "Bland-Altman decision lower LoA")
        AssertAlmostEqual(fromArrays.BlandAltman.UpperLoACI.Estimate, fromFit.BlandAltman.UpperLoACI.Estimate, TOL, "Bland-Altman decision upper LoA")
    End Sub

    <TestCategory("Agreement")>
    <TestMethod()>
    Public Sub AssessBlandAltmanAgainstDecisionLimits_supports_repeated_and_transformed_scale_results()
        Dim referenceValues As Double() = {100.0#, 102.0#, 101.0#,
                                           110.0#, 111.0#, 109.0#,
                                           90.0#, 92.0#, 91.0#}
        Dim testValues As Double() = {101.0#, 103.0#, 102.0#,
                                      112.0#, 110.0#, 110.0#,
                                      89.0#, 93.0#, 92.0#}
        Dim subjectIds As Object() = {"A", "A", "A", "B", "B", "B", "C", "C", "C"}

        Dim fit = BuildRepeatedPercentResult(referenceValues, testValues, subjectIds)
        Dim biasAssessment = equivalencetests.EquivalenceNonInferiorityMethods.AssessAllowableBias(fit, -5.0#, 5.0#)
        Dim decision = equivalencetests.EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(fit, -8.0#, 8.0#)

        Assert.IsNotNull(fit)
        Assert.IsTrue(fit.UsedRepeatedModel)
        Assert.IsNotNull(fit.BiasCI)
        Assert.IsNotNull(fit.LowerLoACI)
        Assert.IsNotNull(fit.UpperLoACI)
        Assert.IsNotNull(biasAssessment)
        Assert.IsNotNull(decision)
        Assert.AreEqual(-8.0#, decision.LowerAllowableLimit)
        Assert.AreEqual(8.0#, decision.UpperAllowableLimit)
        Assert.AreEqual(biasAssessment.IsConfidenceIntervalWithinMargins, decision.BiasAssessment.IsConfidenceIntervalWithinMargins)
        Assert.AreEqual((fit.LowerLoACI.Estimate >= -8.0# AndAlso fit.UpperLoACI.Estimate <= 8.0#), decision.AreObservedLoAWithinAllowableLimits)
        Assert.AreEqual((fit.LowerLoACI.LowerLimit >= -8.0# AndAlso fit.UpperLoACI.UpperLimit <= 8.0#), decision.AreLoAConfidenceIntervalsWithinAllowableLimits)
        Assert.IsFalse(String.IsNullOrWhiteSpace(decision.Conclusion))
    End Sub

    <TestCategory("Agreement")>
    <TestMethod()>
    Public Sub AssessBlandAltmanAgainstDecisionLimits_invalid_limit_order_throws()
        Dim fit = BuildSimplePairsResult(
            New Double() {100.0#, 101.0#, 99.0#, 98.0#},
            New Double() {101.0#, 100.0#, 100.0#, 99.0#})

        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub()
                                                                   equivalencetests.EquivalenceNonInferiorityMethods.AssessBlandAltmanAgainstDecisionLimits(fit, 1.0#, -1.0#)
                                                               End Sub)
    End Sub
End Class
