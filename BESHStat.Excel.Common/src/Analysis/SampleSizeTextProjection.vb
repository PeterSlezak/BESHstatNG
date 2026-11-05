Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.SampleSizeCalc

Namespace ExcelCommon
    Public NotInheritable Class SampleSizeTextProjection
        Private Sub New()
        End Sub
        Public Shared Function LogRankSampleSize(request As SampleSizePlanningRequest, result As LogRankSampleSizeResult) As String
            Dim hazardRatio As Double = request.Value(0)
            Dim controlEventProportion As Double = request.Value(1)
            Dim experimentalEventProportion As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: Hazard ratio={hazardRatio}; Control event proportion={controlEventProportion}; Experimental event proportion={experimentalEventProportion}; Ratio of control to experimental subjects={kappa}.{PlanningTextNewLine.Value}" &
            $"alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"Required events:{result.RequiredEvents}{PlanningTextNewLine.Value}" &
            $"Average event proportion={result.AverageEventProportion}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{result.NumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Estimated Total Number of Subjects:{result.TotalNumberOfSubjects}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return out
        End Function

        Public Shared Function CoxEventCountContinuousCovariate(request As SampleSizePlanningRequest, result As CoxEventCountPlanningResult) As String
            Dim hazardRatioPerUnit As Double = request.Value(0)
            Dim covariateSd As Double = request.Value(1)
            Dim rSquaredWithOtherCovariates As Double = request.Value(2)
            Dim overallEventProportion As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim subjectsText As String = If(result.EstimatedNumberOfSubjects > 0,
                                         result.EstimatedNumberOfSubjects.ToString(), "not estimated (overall event proportion not provided)")

            Dim overallEventText As String = If(Double.IsNaN(result.OverallEventProportion),
                                               "not provided", result.OverallEventProportion.ToString())

            Dim out As String =
                $"Inputs: Hazard ratio per unit={hazardRatioPerUnit}; Covariate SD={covariateSd}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={overallEventText}.{PlanningTextNewLine.Value}" &
                $"alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
                $"Required events:{result.RequiredEvents}{PlanningTextNewLine.Value}" &
                $"Estimated Number of Subjects:{subjectsText}{PlanningTextNewLine.Value}" &
                $"log(HR)={result.LogHazardRatio}; Effective variance={result.EffectiveVariance}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                        Return out
        End Function

        Public Shared Function CoxEventCountBinaryCovariate(request As SampleSizePlanningRequest, binaryResult As CoxEventCountPlanningResult) As String
            Dim hazardRatio As Double = request.Value(0)
            Dim kappa As Double = request.Value(1)
            Dim rSquaredWithOtherCovariates As Double = request.Value(2)
            Dim overallEventProportion As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim binarySubjectsText As String = If(binaryResult.EstimatedNumberOfSubjects > 0,
                                               binaryResult.EstimatedNumberOfSubjects.ToString(), "not estimated (overall event proportion not provided)")

        Dim binaryOverallEventText As String = If(Double.IsNaN(binaryResult.OverallEventProportion),
                                                   "not provided", binaryResult.OverallEventProportion.ToString())

        Dim binaryOut As String =
            $"Inputs: Hazard ratio={hazardRatio}; Ratio of control to experimental subjects={kappa}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={binaryOverallEventText}.{PlanningTextNewLine.Value}" &
            $"alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"Required events:{binaryResult.RequiredEvents}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Subjects:{binarySubjectsText}{PlanningTextNewLine.Value}" &
            $"log(HR)={binaryResult.LogHazardRatio}; Effective variance={binaryResult.EffectiveVariance}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return binaryOut
        End Function

        Public Shared Function IccHypothesisTestSampleSize(request As SampleSizePlanningRequest, result As IccHypothesisTestSampleSizeResult) As String
            Dim nullIcc As Double = request.Value(0)
            Dim alternativeIcc As Double = request.Value(1)
            Dim observationsPerSubjectValue As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: Null ICC={nullIcc}; Alternative ICC={alternativeIcc}; Observations per subject={request.ObservationsPerSubject}.{PlanningTextNewLine.Value}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"Estimated Number of Subjects:{result.NumberOfSubjects}{PlanningTextNewLine.Value}" &
            $"Achieved power={result.AchievedPower}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return out
        End Function

        Public Shared Function BlandAltmanLoASampleSize(request As SampleSizePlanningRequest, result As BlandAltmanAgreementStudyPlanningResult) As String
            Dim sdDifference As Double = request.Value(0)
            Dim desiredHalfWidth As Double = request.Value(1)
            Dim loaMultiplier As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: SD of differences={sdDifference}; Desired LoA CI half-width={desiredHalfWidth}; LoA multiplier={loaMultiplier}; alpha={Alpha}.{PlanningTextNewLine.Value}" &
            $"Estimated Number of Pairs:{result.NumberOfPairs}{PlanningTextNewLine.Value}" &
            $"Achieved LoA CI half-width={result.AchievedHalfWidth}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return out
        End Function

        Public Shared Function NonInferiorityIndependentProportions(request As SampleSizePlanningRequest, result As IndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; Ratio of control to experimental subjects={kappa}.{PlanningTextNewLine.Value}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"For uncorrected chi-square test:{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{result.UncorrectedNumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{result.UncorrectedNumberOfExperimental} {PlanningTextNewLine.Value}" &
            $"For corrected chi-square or Fisher's exact test:{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{result.CorrectedNumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{result.CorrectedNumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                        Return out
        End Function

        Public Shared Function EquivalenceIndependentProportions(request As SampleSizePlanningRequest, result As EquivalenceIndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; Ratio of control to experimental subjects={kappa}.{PlanningTextNewLine.Value}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"Lower-bound requirement (uncorrected): Controls={result.LowerBoundUncorrectedNumberOfControls}; Experimental={result.LowerBoundUncorrectedNumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Upper-bound requirement (uncorrected): Controls={result.UpperBoundUncorrectedNumberOfControls}; Experimental={result.UpperBoundUncorrectedNumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Lower-bound requirement (corrected/Fisher): Controls={result.LowerBoundCorrectedNumberOfControls}; Experimental={result.LowerBoundCorrectedNumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Upper-bound requirement (corrected/Fisher): Controls={result.UpperBoundCorrectedNumberOfControls}; Experimental={result.UpperBoundCorrectedNumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Driving bound: {result.DrivingBound}{PlanningTextNewLine.Value}" &
            $"Final uncorrected requirement: Controls={result.UncorrectedNumberOfControls}; Experimental={result.UncorrectedNumberOfExperimental}{PlanningTextNewLine.Value}" &
            $"Final corrected/Fisher requirement: Controls={result.CorrectedNumberOfControls}; Experimental={result.CorrectedNumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                        Return out
        End Function

        Public Shared Function IndependentProportions(request As SampleSizePlanningRequest, resultSuperiority As IndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim outSuperiority As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Ratio of control to experimental subjects={kappaSuperiority}.{PlanningTextNewLine.Value}" &
            $"alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}For uncorrected chi-square test:{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{resultSuperiority.UncorrectedNumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.UncorrectedNumberOfExperimental}{PlanningTextNewLine.Value}For corrected chi-square or Fisher's exact test:{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{resultSuperiority.CorrectedNumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.CorrectedNumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return outSuperiority
        End Function

        Public Shared Function SingleProportion(request As SampleSizePlanningRequest, result As SingleProportionSampleSizeResult) As String
            Dim prop As Double = request.Value(0)
            Dim H0Prop As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String = $"Inputs: Proportion={prop}; Null Hypothesis Proportion={H0Prop}; alpha={Alpha}; beta={Beta}. {PlanningTextNewLine.Value}" &
                        $"Est. Number of Subjects:{result.NumberOfSubjects} {PlanningTextNewLine.Value} {PlanningTextNewLine.Value}"

                    Return out
        End Function

        Public Shared Function PairedTTest(request As SampleSizePlanningRequest, result As PairedTTestSampleSizeResult) As String
            Dim diff As Double = request.Value(0)
            Dim sd As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; alpha={Alpha}; beta={Beta}. {PlanningTextNewLine.Value}" &
                        $"Est. Number of Pairs:{result.NumberOfPairs} {PlanningTextNewLine.Value} {PlanningTextNewLine.Value}"

                    Return out
        End Function

        Public Shared Function NonInferiorityUnpairedTTest(request As SampleSizePlanningRequest, result As UnpairedTTestSampleSizeResult) As String
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; SD={sd}; Ratio of control to experimental subjects={kappa}.{PlanningTextNewLine.Value}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{PlanningTextNewLine.Value}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                        Return out
        End Function

        Public Shared Function EquivalenceUnpairedTTest(request As SampleSizePlanningRequest, result As EquivalenceUnpairedTTestSampleSizeResult) As String
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; SD={sd}; Ratio of control to experimental subjects={kappa}.{PlanningTextNewLine.Value}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
                $"Lower-bound requirement: Controls={result.LowerBoundNumberOfControls}; Experimental={result.LowerBoundNumberOfExperimental}{PlanningTextNewLine.Value}" &
                $"Upper-bound requirement: Controls={result.UpperBoundNumberOfControls}; Experimental={result.UpperBoundNumberOfExperimental}{PlanningTextNewLine.Value}" &
                $"Driving bound: {result.DrivingBound}{PlanningTextNewLine.Value}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{PlanningTextNewLine.Value}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                        Return out
        End Function

        Public Shared Function UnpairedTTest(request As SampleSizePlanningRequest, resultSuperiority As UnpairedTTestSampleSizeResult) As String
            Dim diff As Double = request.Value(0)
            Dim sdSuperiority As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim outSuperiority As String =
            $"Inputs: Mean difference={diff}; SD={sdSuperiority}; Ratio of control to experimental subjects={kappaSuperiority}.{PlanningTextNewLine.Value}" &
            $"alpha={Alpha}; beta={Beta}.{PlanningTextNewLine.Value}" &
            $"Estimated Number of Controls:{resultSuperiority.NumberOfControls}{PlanningTextNewLine.Value}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.NumberOfExperimental}{PlanningTextNewLine.Value}{PlanningTextNewLine.Value}"

                    Return outSuperiority
        End Function
    End Class
End Namespace
