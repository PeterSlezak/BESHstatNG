Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.SampleSizeCalc

<TestClass()>
Public Class SampleSizeCalculator_Tests

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

End Class
