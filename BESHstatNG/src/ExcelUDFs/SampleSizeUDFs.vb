Option Explicit On
Option Strict On

Imports System
Imports ExcelDna.Integration
Imports BESHStatNG.SampleSizeCalc
Imports BESHStatNG.AppInfrastructure

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for common sample-size planning scenarios.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions are intended for study planning before data collection.
    ''' Each function returns the required sample size as a small spill range with headers,
    ''' making the result easy to read directly on the worksheet.
    ''' </para>
    ''' <para>
    ''' The functions require a significance level <c>alpha</c> and a type II error rate <c>beta</c>.
    ''' The statistical power is therefore <c>1 - beta</c>. For example, <c>beta = 0.20</c>
    ''' corresponds to 80% power.
    ''' </para>
    ''' <para>
    ''' All returned counts are rounded up to whole subjects or subject-pairs.
    ''' </para>
    ''' </remarks>
    Public Module SampleSizeUDFs

        Private Const HypothesisSuperiority As String = "superiority"
        Private Const HypothesisNonInferiority As String = "noninferiority"
        Private Const HypothesisEquivalence As String = "equivalence"

        ' -------------------------------------------------------------------------------------------------------------
        ' Paired t-test
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the number of paired observations required for a paired two-sided t-test.
        ''' </summary>
        ''' <param name="meanDifference">
        ''' The expected mean of the paired differences.
        ''' This is the effect size on the original measurement scale after subtracting one paired measurement from the other.
        ''' The value must be non-zero.
        ''' </param>
        ''' <param name="sdDifference">
        ''' The expected standard deviation of the paired differences.
        ''' This is not the standard deviation of the raw measurements; it is the standard deviation of the within-pair differences.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level.
        ''' Common choices are 0.05 or 0.01.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' For example, <c>beta = 0.20</c> corresponds to 80% power.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range with headers that reports the required number of pairs.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function for matched or repeated-measures designs where each subject contributes a pair of observations,
        ''' such as before/after measurements or measurements from matched units.
        ''' </para>
        ''' <para>
        ''' The calculation assumes a two-sided hypothesis test and refines the required size using the t distribution.
        ''' The result is the number of complete pairs, not the number of individual observations.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.TTEST_PAIRED(2, 5, 0.05, 0.2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.TTEST_PAIRED",
            Category:="BESHStatNG - Sample Size",
            Description:="Required number of pairs for a paired two-sided t-test.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_TTEST_PAIRED(
            <ExcelArgument(Name:="meanDifference", Description:="Expected mean paired difference (must be non-zero).")> meanDifference As Object,
            <ExcelArgument(Name:="sdDifference", Description:="Expected SD of the paired differences (must be > 0).")> sdDifference As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim diff As Double? = TryGetDouble(meanDifference)
                Dim sd As Double? = TryGetDouble(sdDifference)
                Dim a As Double? = TryGetDouble(alpha)
                Dim b As Double? = TryGetDouble(beta)

                If Not diff.HasValue OrElse Not sd.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                    Return ExcelError.ExcelErrorValue
                End If
                If diff.Value = 0 OrElse sd.Value <= 0 OrElse Not IsOpenUnitInterval(a.Value) OrElse Not IsOpenUnitInterval(b.Value) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim result As PairedTTestSampleSizeResult = SampleSizeCalculator.CalculatePairedTTest(diff.Value, sd.Value, a.Value, b.Value)
                Return MakeMetricValueTable("Required pairs", result.NumberOfPairs)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.SSIZE_TTEST_PAIRED", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Unpaired t-test
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required group sizes for an unpaired two-sample t-test, non-inferiority test, or equivalence test.
        ''' </summary>
        ''' <param name="meanDifference">
        ''' The expected mean difference on the scale <c>experimental - control</c>.
        ''' For superiority this is the target difference to detect.
        ''' For non-inferiority and equivalence this expected difference must lie on the favorable side of the supplied margin(s).
        ''' </param>
        ''' <param name="commonSd">
        ''' The expected common standard deviation for the outcome variable.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">
        ''' The planned allocation ratio defined as
        ''' <c>number of control subjects / number of experimental subjects</c>.
        ''' A value of 1 means equal group sizes, 2 means twice as many controls as experimental subjects,
        ''' and 0.5 means half as many controls as experimental subjects.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Significance level used for planning.
        ''' For <c>hypothesisType="superiority"</c>, this is the usual two-sided alpha.
        ''' For <c>hypothesisType="noninferiority"</c> and <c>"equivalence"</c>, this is the one-sided alpha.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <param name="hypothesisType">
        ''' Optional hypothesis selector: <c>"superiority"</c> (default), <c>"noninferiority"</c>, or <c>"equivalence"</c>.
        ''' Common short aliases such as <c>"ni"</c> and <c>"eq"</c> are also accepted.
        ''' </param>
        ''' <param name="margin">
        ''' Optional positive margin magnitude used only when <paramref name="hypothesisType"/> is <c>"noninferiority"</c> or <c>"equivalence"</c>.
        ''' For non-inferiority, the function interprets this as the absolute size of the lower margin on the
        ''' <c>experimental - control</c> scale, so a value of <c>0.5</c> means the experimental mean may be up to 0.5 units lower than control.
        ''' For equivalence, the function uses symmetric margins <c>-margin</c> and <c>+margin</c>.
        ''' </param>
        ''' <returns>
        ''' For superiority and non-inferiority, returns a three-row spill range with the required control and experimental group sizes.
        ''' For equivalence, returns a larger spill range showing the lower-bound and upper-bound TOST components and the final driving result.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function extends the original two-sided superiority planning workflow by optionally supporting
        ''' non-inferiority and symmetric equivalence planning through the same worksheet function.
        ''' Existing formulas that omit <paramref name="hypothesisType"/> and <paramref name="margin"/> continue to work as before.
        ''' </para>
        ''' <para>
        ''' For equivalence, the returned table includes separate results for the lower and upper TOST components,
        ''' plus the final group sizes determined by the driving bound.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.TTEST_UNPAIRED(2, 5, 1, 0.05, 0.2)
        ''' =BESH.SSIZE.TTEST_UNPAIRED(0, 5, 1, 0.025, 0.2, "noninferiority", 1)
        ''' =BESH.SSIZE.TTEST_UNPAIRED(0, 5, 1, 0.025, 0.2, "equivalence", 1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.TTEST_UNPAIRED",
            Category:="BESHStatNG - Sample Size",
            Description:="Required group sizes for unpaired superiority, non-inferiority, or equivalence t-test planning.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_TTEST_UNPAIRED(
            <ExcelArgument(Name:="meanDifference", Description:="Expected mean difference (experimental - control).")> meanDifference As Object,
            <ExcelArgument(Name:="commonSd", Description:="Expected common SD (must be > 0).")> commonSd As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects (must be > 0).")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Alpha: two-sided for superiority, one-sided for noninferiority/equivalence.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object,
            <ExcelArgument(Name:="hypothesisType", Description:="Optional: superiority (default), noninferiority/ni, or equivalence/eq.")> Optional hypothesisType As Object = Nothing,
            <ExcelArgument(Name:="margin", Description:="Optional positive NI or symmetric equivalence margin magnitude.")> Optional margin As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareUnpaired(
                        meanDifference, commonSd, controlToExperimentalRatio, alpha, beta, hypothesisType, margin, AddressOf TryGetDouble, AddressOf ResolveSampleSizeHypothesisKind38, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.SSIZE_TTEST_UNPAIRED", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' One-sample proportion
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required sample size for a one-sample test of a proportion.
        ''' </summary>
        ''' <param name="anticipatedProportion">
        ''' The proportion expected under the alternative hypothesis.
        ''' This is typically the proportion that the study is designed to detect.
        ''' The value must satisfy <c>0 &lt;= anticipatedProportion &lt;= 1</c>.
        ''' </param>
        ''' <param name="nullProportion">
        ''' The reference proportion under the null hypothesis.
        ''' The value must satisfy <c>0 &lt;= nullProportion &lt;= 1</c> and must differ from <paramref name="anticipatedProportion"/>.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided significance level.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range with headers that reports the required number of subjects.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function when the primary analysis compares a single population proportion
        ''' against a prespecified reference value.
        ''' </para>
        ''' <para>
        ''' The calculation uses a normal approximation and rounds the result up to the next whole subject.
        ''' When the anticipated proportion is very close to the null proportion, the required sample size can become very large.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.PROP_SINGLE(0.6, 0.5, 0.05, 0.2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.PROP_SINGLE",
            Category:="BESHStatNG - Sample Size",
            Description:="Required sample size for a one-sample two-sided proportion test.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_PROP_SINGLE(
            <ExcelArgument(Name:="anticipatedProportion", Description:="Proportion expected under the alternative hypothesis (0 to 1).")> anticipatedProportion As Object,
            <ExcelArgument(Name:="nullProportion", Description:="Reference proportion under the null hypothesis (0 to 1 and different from anticipated proportion).")> nullProportion As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareSingleProportion(
                        anticipatedProportion, nullProportion, alpha, beta, AddressOf TryGetDouble, AddressOf IsOpenUnitInterval, AddressOf IsClosedUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.SSIZE_PROP_SINGLE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Two independent proportions
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required group sizes for superiority, non-inferiority, or equivalence comparisons of two independent proportions.
        ''' </summary>
        ''' <param name="controlProportion">
        ''' The anticipated proportion in the control group.
        ''' The value must satisfy <c>0 &lt;= controlProportion &lt;= 1</c>.
        ''' </param>
        ''' <param name="experimentalProportion">
        ''' The anticipated proportion in the experimental group.
        ''' The value must satisfy <c>0 &lt;= experimentalProportion &lt;= 1</c>.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">
        ''' The planned allocation ratio defined as
        ''' <c>number of control subjects / number of experimental subjects</c>.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Significance level used for planning.
        ''' For <c>hypothesisType="superiority"</c>, this is the usual two-sided alpha.
        ''' For <c>hypothesisType="noninferiority"</c> and <c>"equivalence"</c>, this is the one-sided alpha.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' </param>
        ''' <param name="hypothesisType">
        ''' Optional hypothesis selector: <c>"superiority"</c> (default), <c>"noninferiority"</c>, or <c>"equivalence"</c>.
        ''' Common short aliases such as <c>"ni"</c> and <c>"eq"</c> are also accepted.
        ''' </param>
        ''' <param name="margin">
        ''' Optional positive margin magnitude used only when <paramref name="hypothesisType"/> is <c>"noninferiority"</c> or <c>"equivalence"</c>.
        ''' For non-inferiority, the function interprets this as the absolute size of the lower margin on the
        ''' <c>experimental - control</c> scale, so a value of <c>0.1</c> means the experimental proportion may be up to 0.1 lower than control.
        ''' For equivalence, the function uses symmetric margins <c>-margin</c> and <c>+margin</c>.
        ''' </param>
        ''' <returns>
        ''' For superiority and non-inferiority, returns a spill range showing both uncorrected chi-square and corrected chi-square / Fisher exact recommendations.
        ''' For equivalence, returns a larger spill range showing lower-bound and upper-bound TOST components and the final driving recommendations.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Existing formulas that omit <paramref name="hypothesisType"/> and <paramref name="margin"/> continue to use the original superiority calculation.
        ''' </para>
        ''' <para>
        ''' The function returns both uncorrected and corrected/Fisher-style recommendations because the required sample size depends on the intended test framework.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.PROP_INDEP(0.3, 0.5, 1, 0.05, 0.2)
        ''' =BESH.SSIZE.PROP_INDEP(0.5, 0.5, 1, 0.025, 0.2, "noninferiority", 0.1)
        ''' =BESH.SSIZE.PROP_INDEP(0.5, 0.5, 1, 0.025, 0.2, "equivalence", 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.PROP_INDEP",
            Category:="BESHStatNG - Sample Size",
            Description:="Required group sizes for superiority, non-inferiority, or equivalence comparisons of two independent proportions.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_PROP_INDEP(
            <ExcelArgument(Name:="controlProportion", Description:="Anticipated control-group proportion (0 to 1).")> controlProportion As Object,
            <ExcelArgument(Name:="experimentalProportion", Description:="Anticipated experimental-group proportion (0 to 1).")> experimentalProportion As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects (must be > 0).")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Alpha: two-sided for superiority, one-sided for noninferiority/equivalence.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object,
            <ExcelArgument(Name:="hypothesisType", Description:="Optional: superiority (default), noninferiority/ni, or equivalence/eq.")> Optional hypothesisType As Object = Nothing,
            <ExcelArgument(Name:="margin", Description:="Optional positive NI or symmetric equivalence margin magnitude.")> Optional margin As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareIndependentProportions(
                        controlProportion, experimentalProportion, controlToExperimentalRatio, alpha, beta, hypothesisType, margin, AddressOf TryGetDouble, AddressOf ResolveSampleSizeHypothesisKind38, AddressOf IsOpenUnitInterval, AddressOf IsClosedUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.SSIZE_PROP_INDEP", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the required number of events and subjects for a two-group log-rank comparison.
        ''' </summary>
        ''' <param name="hazardRatio">
        ''' Anticipated hazard ratio for the experimental group relative to the control group.
        ''' Values below 1 indicate a lower event rate in the experimental group, and values above 1 indicate a higher event rate.
        ''' The value must be strictly positive and must differ from 1.
        ''' </param>
        ''' <param name="controlEventProportion">
        ''' Expected event proportion in the control group during the full study window.
        ''' This should be the cumulative proportion of subjects expected to experience the event by the end of follow-up.
        ''' The value must satisfy <c>0 &lt; p &lt; 1</c>.
        ''' </param>
        ''' <param name="experimentalEventProportion">
        ''' Expected event proportion in the experimental group during the full study window.
        ''' This should be on the same study horizon as the control-group event proportion.
        ''' The value must satisfy <c>0 &lt; p &lt; 1</c>.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">
        ''' Allocation ratio expressed as control subjects divided by experimental subjects.
        ''' Use <c>1</c> for equal allocation, <c>2</c> for twice as many control subjects as experimental subjects, and so on.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Type I error rate for the planned comparison.
        ''' When <paramref name="twoSided"/> is TRUE this is interpreted as a two-sided alpha;
        ''' when <paramref name="twoSided"/> is FALSE it is interpreted as a one-sided alpha.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' For example, <c>beta = 0.20</c> corresponds to 80% power.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <param name="twoSided">
        ''' Optional logical flag indicating whether the design is two-sided.
        ''' TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
        ''' If omitted, a two-sided design is used.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range reporting the required number of events, the planned control and experimental group sizes,
        ''' the total number of subjects, the allocation proportions implied by the ratio, and the weighted average event proportion.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function when planning a two-group time-to-event study analyzed with a log-rank test.
        ''' The function first estimates how many events are needed to achieve the requested alpha and power,
        ''' then inflates that event count to a total sample size using the expected event proportions in the two study arms.
        ''' </para>
        ''' <para>
        ''' The event proportions should reflect the anticipated follow-up duration, accrual pattern, and censoring context for the study.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.LOGRANK(0.7, 0.30, 0.22, 1, 0.05, 0.20)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.LOGRANK",
            Category:="BESHStatNG - Sample Size",
            Description:="Required events and subjects for a two-group log-rank design.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/sample-size-log-rank/",
            IsThreadSafe:=True)>
        Public Function SSIZE_LOGRANK(
            <ExcelArgument(Name:="hazardRatio", Description:="Anticipated hazard ratio, experimental / control. Must be > 0 and not equal to 1.")> hazardRatio As Object,
            <ExcelArgument(Name:="controlEventProportion", Description:="Expected control-group event proportion during the study window, 0 < p < 1.")> controlEventProportion As Object,
            <ExcelArgument(Name:="experimentalEventProportion", Description:="Expected experimental-group event proportion during the study window, 0 < p < 1.")> experimentalEventProportion As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects. Must be > 0.")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Type I error rate, 0 < alpha < 1. Interpreted as two-sided unless twoSided=FALSE.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object,
            <ExcelArgument(Name:="twoSided", Description:="Optional TRUE/FALSE flag. Defaults to TRUE for a two-sided design.")> Optional twoSided As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareLogRank(
                        hazardRatio, controlEventProportion, experimentalEventProportion, controlToExperimentalRatio, alpha, beta, twoSided, AddressOf TryGetDouble, AddressOf GetOptionalBool, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.LOGRANK", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the required number of events for a Cox proportional hazards design with a binary covariate,
        ''' and optionally converts that event count to a total sample size.
        ''' </summary>
        ''' <param name="hazardRatio">
        ''' Anticipated hazard ratio associated with the binary covariate of interest.
        ''' In a two-arm treatment study this is commonly the hazard ratio for experimental versus control.
        ''' The value must be strictly positive and must differ from 1.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">
        ''' Allocation ratio expressed as control subjects divided by experimental subjects.
        ''' Use <c>1</c> for equal allocation.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Type I error rate for the planned covariate test.
        ''' When <paramref name="twoSided"/> is TRUE this is interpreted as a two-sided alpha;
        ''' when <paramref name="twoSided"/> is FALSE it is interpreted as a one-sided alpha.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <param name="rSquaredWithOtherCovariates">
        ''' Optional proportion of variance in the binary covariate explained by the remaining covariates in the model.
        ''' Use <c>0</c> when planning an unadjusted effect or when no meaningful inflation is needed.
        ''' The value must satisfy <c>0 &lt;= R^2 &lt; 1</c>.
        ''' If omitted, <c>0</c> is used.
        ''' </param>
        ''' <param name="overallEventProportion">
        ''' Optional overall event proportion expected in the full study cohort during follow-up.
        ''' When supplied, the function also reports an estimated total number of subjects.
        ''' The value must satisfy <c>0 &lt; p &lt; 1</c>.
        ''' If omitted, only the required event count is reported.
        ''' </param>
        ''' <param name="twoSided">
        ''' Optional logical flag indicating whether the design is two-sided.
        ''' TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
        ''' If omitted, a two-sided design is used.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range reporting the required event count, an optional estimated total sample size,
        ''' the log hazard ratio, the effective covariate variance determined by the allocation ratio,
        ''' and the assumed <c>R^2</c> with the other covariates.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function for planning Cox regression when the covariate of primary interest is binary,
        ''' such as treatment assignment, exposure status, or membership in one of two groups.
        ''' </para>
        ''' <para>
        ''' The function can be used either for pure event-count planning or, when an overall event proportion is available,
        ''' for an approximate total-sample-size calculation.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.COX_BINARY(0.7, 1, 0.05, 0.20, 0, 0.26, TRUE)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.COX_BINARY",
            Category:="BESHStatNG - Sample Size",
            Description:="Required events for a Cox design with a binary covariate, with optional total-sample estimate.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/sample-size-cox-regression/",
            IsThreadSafe:=True)>
        Public Function SSIZE_COX_BINARY(
            <ExcelArgument(Name:="hazardRatio", Description:="Anticipated hazard ratio for the binary covariate. Must be > 0 and not equal to 1.")> hazardRatio As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects. Must be > 0.")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Type I error rate, 0 < alpha < 1. Interpreted as two-sided unless twoSided=FALSE.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object,
            <ExcelArgument(Name:="rSquaredWithOtherCovariates", Description:="Optional R-squared with the remaining covariates, 0 <= R^2 < 1. Defaults to 0.")> Optional rSquaredWithOtherCovariates As Object = Nothing,
            <ExcelArgument(Name:="overallEventProportion", Description:="Optional overall event proportion during follow-up, 0 < p < 1.")> Optional overallEventProportion As Object = Nothing,
            <ExcelArgument(Name:="twoSided", Description:="Optional TRUE/FALSE flag. Defaults to TRUE for a two-sided design.")> Optional twoSided As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareCoxBinary(
                        hazardRatio, controlToExperimentalRatio, alpha, beta, rSquaredWithOtherCovariates, overallEventProportion, twoSided, AddressOf TryGetDouble, AddressOf IsMissingArg, AddressOf GetOptionalBool, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.COX_BINARY", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the required number of events for a Cox proportional hazards design with a continuous covariate,
        ''' and optionally converts that event count to a total sample size.
        ''' </summary>
        ''' <param name="hazardRatioPerUnit">
        ''' Anticipated hazard ratio for a one-unit increase in the covariate.
        ''' Values above 1 indicate an increased hazard per unit increase, and values below 1 indicate a decreased hazard per unit increase.
        ''' The value must be strictly positive and must differ from 1.
        ''' </param>
        ''' <param name="covariateSd">
        ''' Expected standard deviation of the covariate in the target population.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Type I error rate for the planned covariate test.
        ''' When <paramref name="twoSided"/> is TRUE this is interpreted as a two-sided alpha;
        ''' when <paramref name="twoSided"/> is FALSE it is interpreted as a one-sided alpha.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <param name="rSquaredWithOtherCovariates">
        ''' Optional proportion of variance in the covariate explained by the remaining covariates in the model.
        ''' The value must satisfy <c>0 &lt;= R^2 &lt; 1</c>.
        ''' If omitted, <c>0</c> is used.
        ''' </param>
        ''' <param name="overallEventProportion">
        ''' Optional overall event proportion expected in the full study cohort during follow-up.
        ''' When supplied, the function also reports an estimated total number of subjects.
        ''' The value must satisfy <c>0 &lt; p &lt; 1</c>.
        ''' If omitted, only the required event count is reported.
        ''' </param>
        ''' <param name="twoSided">
        ''' Optional logical flag indicating whether the design is two-sided.
        ''' TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
        ''' If omitted, a two-sided design is used.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range reporting the required event count, an optional estimated total sample size,
        ''' the log hazard ratio for a one-unit covariate increase, the effective variance after accounting for the covariate spread,
        ''' and the assumed <c>R^2</c> with the other covariates.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function when the primary Cox-regression predictor is continuous,
        ''' such as age, biomarker level, dose, or another quantitative measurement.
        ''' </para>
        ''' <para>
        ''' The hazard ratio is interpreted per one-unit increase, so the units of the covariate must be chosen carefully.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.COX_CONTINUOUS(1.25, 2.5, 0.05, 0.20, 0.10, 0.30, TRUE)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.COX_CONTINUOUS",
            Category:="BESHStatNG - Sample Size",
            Description:="Required events for a Cox design with a continuous covariate, with optional total-sample estimate.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/sample-size-cox-regression/",
            IsThreadSafe:=True)>
        Public Function SSIZE_COX_CONTINUOUS(
            <ExcelArgument(Name:="hazardRatioPerUnit", Description:="Anticipated hazard ratio per one-unit increase in the covariate. Must be > 0 and not equal to 1.")> hazardRatioPerUnit As Object,
            <ExcelArgument(Name:="covariateSd", Description:="Expected standard deviation of the covariate. Must be > 0.")> covariateSd As Object,
            <ExcelArgument(Name:="alpha", Description:="Type I error rate, 0 < alpha < 1. Interpreted as two-sided unless twoSided=FALSE.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object,
            <ExcelArgument(Name:="rSquaredWithOtherCovariates", Description:="Optional R-squared with the remaining covariates, 0 <= R^2 < 1. Defaults to 0.")> Optional rSquaredWithOtherCovariates As Object = Nothing,
            <ExcelArgument(Name:="overallEventProportion", Description:="Optional overall event proportion during follow-up, 0 < p < 1.")> Optional overallEventProportion As Object = Nothing,
            <ExcelArgument(Name:="twoSided", Description:="Optional TRUE/FALSE flag. Defaults to TRUE for a two-sided design.")> Optional twoSided As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareCoxContinuous(
                        hazardRatioPerUnit, covariateSd, alpha, beta, rSquaredWithOtherCovariates, overallEventProportion, twoSided, AddressOf TryGetDouble, AddressOf IsMissingArg, AddressOf GetOptionalBool, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.COX_CONTINUOUS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the number of subjects required to test whether an intraclass correlation exceeds a minimum acceptable value.
        ''' </summary>
        ''' <param name="nullIcc">
        ''' Minimum acceptable intraclass correlation under the null hypothesis.
        ''' The value must satisfy <c>0 &lt;= ICC &lt; 1</c>.
        ''' </param>
        ''' <param name="alternativeIcc">
        ''' Target intraclass correlation under the alternative hypothesis.
        ''' The value must satisfy <c>0 &lt;= ICC &lt; 1</c> and must be greater than <paramref name="nullIcc"/>.
        ''' </param>
        ''' <param name="observationsPerSubject">
        ''' Number of repeated measurements or raters per subject.
        ''' The value must be an integer greater than or equal to 2.
        ''' </param>
        ''' <param name="alpha">
        ''' One-sided type I error rate for the reliability test.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range reporting the required number of subjects,
        ''' the number of observations per subject, the null and alternative ICC values,
        ''' and the achieved power at the final rounded sample size.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function when planning a reliability study in which each subject is assessed repeatedly,
        ''' or by multiple raters, and the goal is to demonstrate that the intraclass correlation exceeds a pre-specified minimum.
        ''' </para>
        ''' <para>
        ''' The design is based on a one-way random-effects testing framework and requires at least two observations per subject.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.ICC(0.5, 0.75, 3, 0.05, 0.20)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.ICC",
            Category:="BESHStatNG - Sample Size",
            Description:="Required subjects for a reliability study based on an intraclass-correlation target.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/sample-size-icc/",
            IsThreadSafe:=True)>
        Public Function SSIZE_ICC(
            <ExcelArgument(Name:="nullIcc", Description:="Minimum acceptable ICC under the null hypothesis, 0 <= ICC < 1.")> nullIcc As Object,
            <ExcelArgument(Name:="alternativeIcc", Description:="Target ICC under the alternative hypothesis, 0 <= ICC < 1 and greater than nullIcc.")> alternativeIcc As Object,
            <ExcelArgument(Name:="observationsPerSubject", Description:="Number of repeated observations or raters per subject. Integer >= 2.")> observationsPerSubject As Object,
            <ExcelArgument(Name:="alpha", Description:="One-sided type I error rate, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareIcc(
                        nullIcc, alternativeIcc, observationsPerSubject, alpha, beta, AddressOf TryGetDouble, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.ICC", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Estimates the number of paired measurements required so that the confidence interval around either limit of agreement
        ''' has a desired half-width in a Bland-Altman agreement study.
        ''' </summary>
        ''' <param name="sdDifference">
        ''' Expected standard deviation of the paired differences.
        ''' This is the standard deviation of the measurement differences, not the standard deviation of the raw measurements.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="desiredHalfWidth">
        ''' Desired half-width of the confidence interval around a limit of agreement, expressed on the original measurement scale.
        ''' Smaller values require larger samples.
        ''' The value must be strictly positive.
        ''' </param>
        ''' <param name="alpha">
        ''' Two-sided alpha used for the confidence interval around each limit of agreement.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="loaMultiplier">
        ''' Optional multiplier used to define the limits of agreement.
        ''' The conventional value is <c>1.96</c> for 95% limits of agreement.
        ''' The value must be strictly positive.
        ''' If omitted, <c>1.96</c> is used.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range reporting the required number of pairs, the expected standard deviation of the paired differences,
        ''' the requested half-width, the achieved half-width at the final rounded sample size,
        ''' the alpha level, and the limits-of-agreement multiplier.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function when planning an agreement study in which the main precision goal is the width of the confidence interval
        ''' around the Bland-Altman limits of agreement.
        ''' </para>
        ''' <para>
        ''' The result is the number of complete pairs of measurements required.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.BLANDALTMAN(5, 2, 0.05, 1.96)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.BLANDALTMAN",
            Category:="BESHStatNG - Sample Size",
            Description:="Required pairs for a Bland-Altman agreement study with a target limits-of-agreement precision.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/methods/sample-size-bland-altman/",
            IsThreadSafe:=True)>
        Public Function SSIZE_BLANDALTMAN(
            <ExcelArgument(Name:="sdDifference", Description:="Expected SD of the paired differences. Must be > 0.")> sdDifference As Object,
            <ExcelArgument(Name:="desiredHalfWidth", Description:="Desired CI half-width around a limit of agreement. Must be > 0.")> desiredHalfWidth As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided alpha for the LoA confidence interval, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="loaMultiplier", Description:="Optional LoA multiplier. Defaults to 1.96 and must be > 0 if supplied.")> Optional loaMultiplier As Object = Nothing
        ) As Object
            Try
                Dim errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38
                Dim report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38 =
                    Global.BESHStatNG.ExcelCommon.UdfSampleSizePreparation38.PrepareBlandAltman(
                        sdDifference, desiredHalfWidth, alpha, loaMultiplier, AddressOf TryGetDouble, AddressOf IsMissingArg, AddressOf IsOpenUnitInterval, errorKind)
                Return MaterializeSampleSizeResult38(report, errorKind)
            Catch ex As Exception
                Return LoggedUdfError("BESH.SSIZE.BLANDALTMAN", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        Private Function MakeMetricValueTable(metric As String, value As Integer) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeMetricValueTable(metric, value)), Object(,))
        End Function

        Private Function MakeTwoGroupTable(metric As String, nControls As Integer, nExperimental As Integer) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeTwoGroupTable(metric, nControls, nExperimental)), Object(,))
        End Function

        Private Function MakeEquivalenceTwoGroupTable(result As EquivalenceUnpairedTTestSampleSizeResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeEquivalenceTwoGroupTable(result)), Object(,))
        End Function

        Private Function MakeIndependentProportionsTable(result As IndependentProportionsSampleSizeResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeIndependentProportionsTable(result)), Object(,))
        End Function

        Private Function MakeEquivalenceIndependentProportionsTable(result As EquivalenceIndependentProportionsSampleSizeResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeEquivalenceIndependentProportionsTable(result)), Object(,))
        End Function

        Private Function ResolveSampleSizeHypothesisType(value As Object) As String
            Return Global.BESHStatNG.ExcelCommon.SampleSizeUDFsOptions38.ResolveSampleSizeHypothesisType(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(value))
        End Function

        Private Function MakeLogRankPlanningTable(result As LogRankSampleSizeResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeLogRankPlanningTable(result)), Object(,))
        End Function

        Private Function MakeCoxPlanningTable(modelLabel As String, result As CoxEventCountPlanningResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeCoxPlanningTable(modelLabel, result)), Object(,))
        End Function

        Private Function MakeIccPlanningTable(result As IccHypothesisTestSampleSizeResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeIccPlanningTable(result)), Object(,))
        End Function

        Private Function MakeBlandAltmanPlanningTable(result As BlandAltmanAgreementStudyPlanningResult) As Object(,)
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.SampleSizeUDFsReports38.MakeBlandAltmanPlanningTable(result)), Object(,))
        End Function


        Private Function ResolveSampleSizeHypothesisKind38(value As Object) As Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38
            Dim hypothesis As String = ResolveSampleSizeHypothesisType(value)
            If hypothesis Is Nothing Then Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Invalid
            Select Case hypothesis
                Case HypothesisSuperiority
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Superiority
                Case HypothesisNonInferiority
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.NonInferiority
                Case HypothesisEquivalence
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Equivalence
                Case Else
                    Return Global.BESHStatNG.ExcelCommon.UdfSampleSizeHypothesis38.Invalid
            End Select
        End Function

        Private Function MaterializeSampleSizeResult38(report As Global.BESHStatNG.ExcelCommon.UdfReportProjection38,
                                                       errorKind As Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38) As Object
            If report Is Nothing Then
                If errorKind = Global.BESHStatNG.ExcelCommon.UdfSampleSizeError38.Numeric Then Return ExcelError.ExcelErrorNum
                Return ExcelError.ExcelErrorValue
            End If
            Return DirectCast(MaterializePortableReport38(report), Object(,))
        End Function

    End Module

End Namespace
