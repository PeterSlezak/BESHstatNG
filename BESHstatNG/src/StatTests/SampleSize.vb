Option Explicit On

Namespace SampleSizeCalc

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

    End Module

End Namespace