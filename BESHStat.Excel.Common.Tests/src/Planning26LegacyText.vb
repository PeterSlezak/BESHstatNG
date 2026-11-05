Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.SampleSizeCalc
Imports BESHStatNG.ExcelCommon
Public NotInheritable Class Planning26LegacyText
        Public Shared Function LogRankSampleSize(request As SampleSizePlanningRequest, result As LogRankSampleSizeResult) As String
            Dim hazardRatio As Double = request.Value(0)
            Dim controlEventProportion As Double = request.Value(1)
            Dim experimentalEventProportion As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: Hazard ratio={hazardRatio}; Control event proportion={controlEventProportion}; Experimental event proportion={experimentalEventProportion}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Required events:{result.RequiredEvents}{vbNewLine}" &
            $"Average event proportion={result.AverageEventProportion}{vbNewLine}" &
            $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}" &
            $"Estimated Total Number of Subjects:{result.TotalNumberOfSubjects}{vbNewLine}{vbNewLine}"

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
                $"Inputs: Hazard ratio per unit={hazardRatioPerUnit}; Covariate SD={covariateSd}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={overallEventText}.{vbNewLine}" &
                $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Required events:{result.RequiredEvents}{vbNewLine}" &
                $"Estimated Number of Subjects:{subjectsText}{vbNewLine}" &
                $"log(HR)={result.LogHazardRatio}; Effective variance={result.EffectiveVariance}{vbNewLine}{vbNewLine}"

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
            $"Inputs: Hazard ratio={hazardRatio}; Ratio of control to experimental subjects={kappa}; R-squared with other covariates={rSquaredWithOtherCovariates}; Overall event proportion={binaryOverallEventText}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Required events:{binaryResult.RequiredEvents}{vbNewLine}" &
            $"Estimated Number of Subjects:{binarySubjectsText}{vbNewLine}" &
            $"log(HR)={binaryResult.LogHazardRatio}; Effective variance={binaryResult.EffectiveVariance}{vbNewLine}{vbNewLine}"

                    Return binaryOut
        End Function

        Public Shared Function IccHypothesisTestSampleSize(request As SampleSizePlanningRequest, result As IccHypothesisTestSampleSizeResult) As String
            Dim nullIcc As Double = request.Value(0)
            Dim alternativeIcc As Double = request.Value(1)
            Dim observationsPerSubjectValue As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: Null ICC={nullIcc}; Alternative ICC={alternativeIcc}; Observations per subject={request.ObservationsPerSubject}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Estimated Number of Subjects:{result.NumberOfSubjects}{vbNewLine}" &
            $"Achieved power={result.AchievedPower}{vbNewLine}{vbNewLine}"

                    Return out
        End Function

        Public Shared Function BlandAltmanLoASampleSize(request As SampleSizePlanningRequest, result As BlandAltmanAgreementStudyPlanningResult) As String
            Dim sdDifference As Double = request.Value(0)
            Dim desiredHalfWidth As Double = request.Value(1)
            Dim loaMultiplier As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String =
            $"Inputs: SD of differences={sdDifference}; Desired LoA CI half-width={desiredHalfWidth}; LoA multiplier={loaMultiplier}; alpha={Alpha}.{vbNewLine}" &
            $"Estimated Number of Pairs:{result.NumberOfPairs}{vbNewLine}" &
            $"Achieved LoA CI half-width={result.AchievedHalfWidth}{vbNewLine}{vbNewLine}"

                    Return out
        End Function

        Public Shared Function NonInferiorityIndependentProportions(request As SampleSizePlanningRequest, result As IndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"For uncorrected chi-square test:{vbNewLine}" &
            $"Estimated Number of Controls:{result.UncorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.UncorrectedNumberOfExperimental} {vbNewLine}" &
            $"For corrected chi-square or Fisher's exact test:{vbNewLine}" &
            $"Estimated Number of Controls:{result.CorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{result.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

                        Return out
        End Function

        Public Shared Function EquivalenceIndependentProportions(request As SampleSizePlanningRequest, result As EquivalenceIndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim marginAbsolute As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
            $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Lower-bound requirement (uncorrected): Controls={result.LowerBoundUncorrectedNumberOfControls}; Experimental={result.LowerBoundUncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Upper-bound requirement (uncorrected): Controls={result.UpperBoundUncorrectedNumberOfControls}; Experimental={result.UpperBoundUncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Lower-bound requirement (corrected/Fisher): Controls={result.LowerBoundCorrectedNumberOfControls}; Experimental={result.LowerBoundCorrectedNumberOfExperimental}{vbNewLine}" &
            $"Upper-bound requirement (corrected/Fisher): Controls={result.UpperBoundCorrectedNumberOfControls}; Experimental={result.UpperBoundCorrectedNumberOfExperimental}{vbNewLine}" &
            $"Driving bound: {result.DrivingBound}{vbNewLine}" &
            $"Final uncorrected requirement: Controls={result.UncorrectedNumberOfControls}; Experimental={result.UncorrectedNumberOfExperimental}{vbNewLine}" &
            $"Final corrected/Fisher requirement: Controls={result.CorrectedNumberOfControls}; Experimental={result.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

                        Return out
        End Function

        Public Shared Function IndependentProportions(request As SampleSizePlanningRequest, resultSuperiority As IndependentProportionsSampleSizeResult) As String
            Dim controlProp As Double = request.Value(0)
            Dim experimentalProp As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim outSuperiority As String =
            $"Inputs: Control Group Proportion={controlProp}; Experimental Group Proportion={experimentalProp}; Ratio of control to experimental subjects={kappaSuperiority}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}For uncorrected chi-square test:{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.UncorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.UncorrectedNumberOfExperimental}{vbNewLine}For corrected chi-square or Fisher's exact test:{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.CorrectedNumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.CorrectedNumberOfExperimental}{vbNewLine}{vbNewLine}"

                    Return outSuperiority
        End Function

        Public Shared Function SingleProportion(request As SampleSizePlanningRequest, result As SingleProportionSampleSizeResult) As String
            Dim prop As Double = request.Value(0)
            Dim H0Prop As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String = $"Inputs: Proportion={prop}; Null Hypothesis Proportion={H0Prop}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Subjects:{result.NumberOfSubjects} {vbNewLine} {vbNewLine}"

                    Return out
        End Function

        Public Shared Function PairedTTest(request As SampleSizePlanningRequest, result As PairedTTestSampleSizeResult) As String
            Dim diff As Double = request.Value(0)
            Dim sd As Double = request.Value(1)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim out As String = $"Inputs: Mean difference={diff}; SD={sd}; alpha={Alpha}; beta={Beta}. {vbNewLine}" &
                        $"Est. Number of Pairs:{result.NumberOfPairs} {vbNewLine} {vbNewLine}"

                    Return out
        End Function

        Public Shared Function NonInferiorityUnpairedTTest(request As SampleSizePlanningRequest, result As UnpairedTTestSampleSizeResult) As String
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Noninferiority margin=-{Math.Abs(marginAbsolute)} on (Experimental - Control) scale; SD={sd}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}{vbNewLine}"

                        Return out
        End Function

        Public Shared Function EquivalenceUnpairedTTest(request As SampleSizePlanningRequest, result As EquivalenceUnpairedTTestSampleSizeResult) As String
            Dim expectedDifference As Double = request.Value(0)
            Dim marginAbsolute As Double = request.Value(1)
            Dim sd As Double = request.Value(2)
            Dim kappa As Double = request.Value(3)
            Dim Alpha = request.Alpha, Beta = request.Beta


            Dim out As String =
                $"Inputs: Expected mean difference={expectedDifference}; Equivalence bounds=[-{Math.Abs(marginAbsolute)}, +{Math.Abs(marginAbsolute)}]; SD={sd}; Ratio of control to experimental subjects={kappa}.{vbNewLine}" &
                $"one-sided alpha={Alpha}; beta={Beta}.{vbNewLine}" &
                $"Lower-bound requirement: Controls={result.LowerBoundNumberOfControls}; Experimental={result.LowerBoundNumberOfExperimental}{vbNewLine}" &
                $"Upper-bound requirement: Controls={result.UpperBoundNumberOfControls}; Experimental={result.UpperBoundNumberOfExperimental}{vbNewLine}" &
                $"Driving bound: {result.DrivingBound}{vbNewLine}" &
                $"Estimated Number of Controls:{result.NumberOfControls}{vbNewLine}" &
                $"Estimated Number of Experimental subjects:{result.NumberOfExperimental}{vbNewLine}{vbNewLine}"

                        Return out
        End Function

        Public Shared Function UnpairedTTest(request As SampleSizePlanningRequest, resultSuperiority As UnpairedTTestSampleSizeResult) As String
            Dim diff As Double = request.Value(0)
            Dim sdSuperiority As Double = request.Value(1)
            Dim kappaSuperiority As Double = request.Value(2)
            Dim Alpha = request.Alpha, Beta = request.Beta


        Dim outSuperiority As String =
            $"Inputs: Mean difference={diff}; SD={sdSuperiority}; Ratio of control to experimental subjects={kappaSuperiority}.{vbNewLine}" &
            $"alpha={Alpha}; beta={Beta}.{vbNewLine}" &
            $"Estimated Number of Controls:{resultSuperiority.NumberOfControls}{vbNewLine}" &
            $"Estimated Number of Experimental subjects:{resultSuperiority.NumberOfExperimental}{vbNewLine}{vbNewLine}"

                    Return outSuperiority
        End Function
End Class
