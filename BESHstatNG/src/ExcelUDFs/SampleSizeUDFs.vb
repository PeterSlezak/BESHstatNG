Option Explicit On
Option Strict On

Imports System
Imports ExcelDna.Integration
Imports BESHStatNG.SampleSizeCalc

Namespace BESHStatNG.WorksheetFunctions

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
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/sample-size/",
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
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Unpaired t-test
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required group sizes for an unpaired two-sided t-test.
        ''' </summary>
        ''' <param name="meanDifference">
        ''' The expected difference in means between the two groups on the original measurement scale.
        ''' The value must be non-zero.
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
        ''' Two-sided significance level.
        ''' The value must satisfy <c>0 &lt; alpha &lt; 1</c>.
        ''' </param>
        ''' <param name="beta">
        ''' Type II error rate used for planning.
        ''' Statistical power equals <c>1 - beta</c>.
        ''' The value must satisfy <c>0 &lt; beta &lt; 1</c>.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range with headers that reports the required number of control and experimental subjects.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function for two independent groups when the primary endpoint is approximately continuous
        ''' and the design is planned with a two-sided t-test.
        ''' </para>
        ''' <para>
        ''' The reported counts are rounded up to whole subjects. When the allocation ratio is not 1,
        ''' the function preserves the requested control-to-experimental ratio as closely as possible after rounding.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.TTEST_UNPAIRED(2, 5, 1, 0.05, 0.2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.TTEST_UNPAIRED",
            Category:="BESHStatNG - Sample Size",
            Description:="Required control and experimental group sizes for an unpaired two-sided t-test.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_TTEST_UNPAIRED(
            <ExcelArgument(Name:="meanDifference", Description:="Expected difference in means (must be non-zero).")> meanDifference As Object,
            <ExcelArgument(Name:="commonSd", Description:="Expected common SD (must be > 0).")> commonSd As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects (must be > 0).")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim diff As Double? = TryGetDouble(meanDifference)
                Dim sd As Double? = TryGetDouble(commonSd)
                Dim kappa As Double? = TryGetDouble(controlToExperimentalRatio)
                Dim a As Double? = TryGetDouble(alpha)
                Dim b As Double? = TryGetDouble(beta)

                If Not diff.HasValue OrElse Not sd.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                    Return ExcelError.ExcelErrorValue
                End If
                If diff.Value = 0 OrElse sd.Value <= 0 OrElse kappa.Value <= 0 OrElse Not IsOpenUnitInterval(a.Value) OrElse Not IsOpenUnitInterval(b.Value) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim result As UnpairedTTestSampleSizeResult = SampleSizeCalculator.CalculateUnpairedTTest(diff.Value, sd.Value, kappa.Value, a.Value, b.Value)
                Return MakeTwoGroupTable("Required subjects", result.NumberOfControls, result.NumberOfExperimental)
            Catch
                Return ExcelError.ExcelErrorValue
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
        ''' The value must satisfy <c>0 ≤ anticipatedProportion ≤ 1</c>.
        ''' </param>
        ''' <param name="nullProportion">
        ''' The reference proportion under the null hypothesis.
        ''' The value must satisfy <c>0 ≤ nullProportion ≤ 1</c> and must differ from <paramref name="anticipatedProportion"/>.
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
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_PROP_SINGLE(
            <ExcelArgument(Name:="anticipatedProportion", Description:="Proportion expected under the alternative hypothesis (0 to 1).")> anticipatedProportion As Object,
            <ExcelArgument(Name:="nullProportion", Description:="Reference proportion under the null hypothesis (0 to 1 and different from anticipated proportion).")> nullProportion As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim prop As Double? = TryGetDouble(anticipatedProportion)
                Dim h0 As Double? = TryGetDouble(nullProportion)
                Dim a As Double? = TryGetDouble(alpha)
                Dim b As Double? = TryGetDouble(beta)

                If Not prop.HasValue OrElse Not h0.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                    Return ExcelError.ExcelErrorValue
                End If
                If Not IsClosedUnitInterval(prop.Value) OrElse Not IsClosedUnitInterval(h0.Value) OrElse prop.Value = h0.Value OrElse Not IsOpenUnitInterval(a.Value) OrElse Not IsOpenUnitInterval(b.Value) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim result As SingleProportionSampleSizeResult = SampleSizeCalculator.CalculateSingleProportion(prop.Value, h0.Value, a.Value, b.Value)
                Return MakeMetricValueTable("Required subjects", result.NumberOfSubjects)
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        ' -------------------------------------------------------------------------------------------------------------
        ' Two independent proportions
        ' -------------------------------------------------------------------------------------------------------------

        ''' <summary>
        ''' Estimates the required sample sizes for comparing two independent proportions.
        ''' </summary>
        ''' <param name="controlProportion">
        ''' The anticipated proportion in the control group.
        ''' The value must satisfy <c>0 ≤ controlProportion ≤ 1</c>.
        ''' </param>
        ''' <param name="experimentalProportion">
        ''' The anticipated proportion in the experimental group.
        ''' The value must satisfy <c>0 ≤ experimentalProportion ≤ 1</c> and must differ from <paramref name="controlProportion"/>.
        ''' </param>
        ''' <param name="controlToExperimentalRatio">
        ''' The planned allocation ratio defined as
        ''' <c>number of control subjects / number of experimental subjects</c>.
        ''' The value must be strictly positive.
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
        ''' A three-column spill range with headers.
        ''' The first result row contains the required control and experimental group sizes for the uncorrected chi-square approach.
        ''' The second result row contains the required control and experimental group sizes for the corrected chi-square or Fisher exact approach.
        ''' Returns <c>#VALUE!</c> when an argument is missing or non-numeric.
        ''' Returns <c>#NUM!</c> when the supplied values are outside the valid statistical domain.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Use this function to plan a study that compares two independent proportions,
        ''' for example response rates, event rates, or prevalences in two groups.
        ''' </para>
        ''' <para>
        ''' Two sets of recommendations are returned because the required sample size depends on the intended test framework.
        ''' The corrected/Fisher-style recommendation is usually at least as large as the uncorrected chi-square recommendation.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SSIZE.PROP_INDEP(0.3, 0.5, 1, 0.05, 0.2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SSIZE.PROP_INDEP",
            Category:="BESHStatNG - Sample Size",
            Description:="Required group sizes for comparing two independent proportions.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/sample-size/",
            IsThreadSafe:=True)>
        Public Function SSIZE_PROP_INDEP(
            <ExcelArgument(Name:="controlProportion", Description:="Anticipated control-group proportion (0 to 1).")> controlProportion As Object,
            <ExcelArgument(Name:="experimentalProportion", Description:="Anticipated experimental-group proportion (0 to 1 and different from control proportion).")> experimentalProportion As Object,
            <ExcelArgument(Name:="controlToExperimentalRatio", Description:="Allocation ratio: controls / experimental subjects (must be > 0).")> controlToExperimentalRatio As Object,
            <ExcelArgument(Name:="alpha", Description:="Two-sided significance level, 0 < alpha < 1.")> alpha As Object,
            <ExcelArgument(Name:="beta", Description:="Type II error rate, 0 < beta < 1. Power = 1 - beta.")> beta As Object
        ) As Object
            Try
                Dim cProp As Double? = TryGetDouble(controlProportion)
                Dim eProp As Double? = TryGetDouble(experimentalProportion)
                Dim kappa As Double? = TryGetDouble(controlToExperimentalRatio)
                Dim a As Double? = TryGetDouble(alpha)
                Dim b As Double? = TryGetDouble(beta)

                If Not cProp.HasValue OrElse Not eProp.HasValue OrElse Not kappa.HasValue OrElse Not a.HasValue OrElse Not b.HasValue Then
                    Return ExcelError.ExcelErrorValue
                End If
                If Not IsClosedUnitInterval(cProp.Value) OrElse Not IsClosedUnitInterval(eProp.Value) OrElse cProp.Value = eProp.Value OrElse kappa.Value <= 0 OrElse Not IsOpenUnitInterval(a.Value) OrElse Not IsOpenUnitInterval(b.Value) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim result As IndependentProportionsSampleSizeResult = SampleSizeCalculator.CalculateIndependentProportions(cProp.Value, eProp.Value, kappa.Value, a.Value, b.Value)
                Return MakeIndependentProportionsTable(result)
            Catch
                Return ExcelError.ExcelErrorValue
            End Try
        End Function

        Private Function IsOpenUnitInterval(value As Double) As Boolean
            Return value > 0.0 AndAlso value < 1.0 AndAlso Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Private Function IsClosedUnitInterval(value As Double) As Boolean
            Return value >= 0.0 AndAlso value <= 1.0 AndAlso Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Private Function MakeMetricValueTable(metric As String, value As Integer) As Object(,)
            Dim out(1, 1) As Object
            out(0, 0) = "Metric"
            out(0, 1) = "Value"
            out(1, 0) = metric
            out(1, 1) = value
            Return out
        End Function

        Private Function MakeTwoGroupTable(metric As String, nControls As Integer, nExperimental As Integer) As Object(,)
            Dim out(2, 1) As Object
            out(0, 0) = "Group"
            out(0, 1) = metric
            out(1, 0) = "Controls"
            out(1, 1) = nControls
            out(2, 0) = "Experimental"
            out(2, 1) = nExperimental
            Return out
        End Function

        Private Function MakeIndependentProportionsTable(result As IndependentProportionsSampleSizeResult) As Object(,)
            Dim out(2, 2) As Object
            out(0, 0) = "Method"
            out(0, 1) = "Controls"
            out(0, 2) = "Experimental"
            out(1, 0) = "Uncorrected chi-square"
            out(1, 1) = result.UncorrectedNumberOfControls
            out(1, 2) = result.UncorrectedNumberOfExperimental
            out(2, 0) = "Corrected chi-square / Fisher exact"
            out(2, 1) = result.CorrectedNumberOfControls
            out(2, 2) = result.CorrectedNumberOfExperimental
            Return out
        End Function

    End Module

End Namespace
