Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.SampleSizeCalc

Namespace ExcelCommon
    Public NotInheritable Class SampleSizePlanningPolicy
        Private Sub New()
        End Sub
        Public Shared Function Calculate(request As SampleSizePlanningRequest) As SampleSizePlanningReport
            Select Case request.Method
                Case SampleSizePlanningMethod.LogRankSampleSize
            Dim hazardRatio As Double = request.Value(0)
            Dim controlEventProportion As Double = request.Value(1)
            Dim experimentalEventProportion As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateLogRankSampleSize(hazardRatio,
                                                        controlEventProportion, experimentalEventProportion,
                                                        kappa, Alpha, Beta, True)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.LogRankSampleSize(request, result))
                Case SampleSizePlanningMethod.CoxEventCountContinuousCovariate
            Dim hazardRatioPerUnit As Double = request.Value(0)
            Dim covariateSd As Double = request.Value(1)
            Dim rSquaredWithOtherCovariates As Double = request.Value(2)
            Dim overallEventProportion As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateCoxEventCountContinuousCovariate(hazardRatioPerUnit, covariateSd, Alpha, Beta,
                                                                           rSquaredWithOtherCovariates, overallEventProportion, True)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.CoxEventCountContinuousCovariate(request, result))
                Case SampleSizePlanningMethod.CoxEventCountBinaryCovariate
            Dim hazardRatio As Double = request.Value(0)
            Dim kappa As Double = request.Value(1)
            Dim rSquaredWithOtherCovariates As Double = request.Value(2)
            Dim overallEventProportion As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim binaryResult = SampleSizeCalculator.CalculateCoxEventCountBinaryCovariate(hazardRatio, kappa, Alpha, Beta,
                                                                       rSquaredWithOtherCovariates,
                                                                       overallEventProportion, True)
                    Return New SampleSizePlanningReport(request, binaryResult, SampleSizeTextProjection.CoxEventCountBinaryCovariate(request, binaryResult))
                Case SampleSizePlanningMethod.IccHypothesisTestSampleSize
            Dim nullIcc As Double = request.Value(0)
            Dim alternativeIcc As Double = request.Value(1)
            Dim observationsPerSubjectValue As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateIccHypothesisTestSampleSize(nullIcc, alternativeIcc,
                                                                     request.ObservationsPerSubject, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.IccHypothesisTestSampleSize(request, result))
                Case SampleSizePlanningMethod.BlandAltmanLoASampleSize
            Dim sdDifference As Double = request.Value(0)
            Dim desiredHalfWidth As Double = request.Value(1)
            Dim loaMultiplier As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateBlandAltmanLoASampleSize(sdDifference, desiredHalfWidth, Alpha, loaMultiplier)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.BlandAltmanLoASampleSize(request, result))
                Case SampleSizePlanningMethod.NonInferiorityIndependentProportions
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateNonInferiorityIndependentProportions(controlProp, experimentalProp,
                                                                              -Math.Abs(marginAbsolute),
                                                                              kappa, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.NonInferiorityIndependentProportions(request, result))
                Case SampleSizePlanningMethod.EquivalenceIndependentProportions
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateEquivalenceIndependentProportions(controlProp, experimentalProp,
                                                                           -Math.Abs(marginAbsolute),
                                                                           Math.Abs(marginAbsolute),
                                                                           kappa, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.EquivalenceIndependentProportions(request, result))
                Case SampleSizePlanningMethod.IndependentProportions
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim resultSuperiority = SampleSizeCalculator.CalculateIndependentProportions(controlProp, experimentalProp, kappaSuperiority, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, resultSuperiority, SampleSizeTextProjection.IndependentProportions(request, resultSuperiority))
                Case SampleSizePlanningMethod.SingleProportion
            Dim prop As Double = request.Value(0)
            Dim H0Prop As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateSingleProportion(prop, H0Prop, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.SingleProportion(request, result))
                Case SampleSizePlanningMethod.PairedTTest
            Dim diff As Double = request.Value(0)
            Dim sd As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculatePairedTTest(diff, sd, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.PairedTTest(request, result))
                Case SampleSizePlanningMethod.NonInferiorityUnpairedTTest
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateNonInferiorityUnpairedTTest(expectedDifference,
                                                                     -Math.Abs(marginAbsolute), sd, kappa, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.NonInferiorityUnpairedTTest(request, result))
                Case SampleSizePlanningMethod.EquivalenceUnpairedTTest
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim result = SampleSizeCalculator.CalculateEquivalenceUnpairedTTest(expectedDifference, -Math.Abs(marginAbsolute),
                                                                  Math.Abs(marginAbsolute), sd, kappa, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, result, SampleSizeTextProjection.EquivalenceUnpairedTTest(request, result))
                Case SampleSizePlanningMethod.UnpairedTTest
            Dim diff As Double = request.Value(0)
            Dim sdSuperiority As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta
Dim resultSuperiority = SampleSizeCalculator.CalculateUnpairedTTest(diff, sdSuperiority, kappaSuperiority, Alpha, Beta)
                    Return New SampleSizePlanningReport(request, resultSuperiority, SampleSizeTextProjection.UnpairedTTest(request, resultSuperiority))
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(request))
            End Select
        End Function
    End Class
End Namespace
