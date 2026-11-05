Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.SampleSizeCalc

Namespace ExcelCommon
    Public NotInheritable Class SampleSizePlanningDescriptions
        Private Sub New()
        End Sub
        Public Shared Function Describe(method As SampleSizePlanningMethod) As SampleSizeMethodDescription
            Select Case method
                Case SampleSizePlanningMethod.LogRankSampleSize : Return New SampleSizeMethodDescription(SampleSizeFamily.LogRank, "LogRankSampleSize", New String() {"hazardRatio", "controlEventProportion", "experimentalEventProportion", "kappa"})
                Case SampleSizePlanningMethod.CoxEventCountContinuousCovariate : Return New SampleSizeMethodDescription(SampleSizeFamily.Cox, "CoxEventCountContinuousCovariate", New String() {"hazardRatioPerUnit", "covariateSd", "rSquaredWithOtherCovariates", "overallEventProportion"})
                Case SampleSizePlanningMethod.CoxEventCountBinaryCovariate : Return New SampleSizeMethodDescription(SampleSizeFamily.Cox, "CoxEventCountBinaryCovariate", New String() {"hazardRatio", "kappa", "rSquaredWithOtherCovariates", "overallEventProportion"})
                Case SampleSizePlanningMethod.IccHypothesisTestSampleSize : Return New SampleSizeMethodDescription(SampleSizeFamily.Icc, "IccHypothesisTestSampleSize", New String() {"nullIcc", "alternativeIcc", "observationsPerSubjectValue"})
                Case SampleSizePlanningMethod.BlandAltmanLoASampleSize : Return New SampleSizeMethodDescription(SampleSizeFamily.BlandAltman, "BlandAltmanLoASampleSize", New String() {"sdDifference", "desiredHalfWidth", "loaMultiplier"})
                Case SampleSizePlanningMethod.NonInferiorityIndependentProportions : Return New SampleSizeMethodDescription(SampleSizeFamily.IndependentProportions, "NonInferiorityIndependentProportions", New String() {"controlProp", "experimentalProp", "marginAbsolute", "kappa"})
                Case SampleSizePlanningMethod.EquivalenceIndependentProportions : Return New SampleSizeMethodDescription(SampleSizeFamily.IndependentProportions, "EquivalenceIndependentProportions", New String() {"controlProp", "experimentalProp", "marginAbsolute", "kappa"})
                Case SampleSizePlanningMethod.IndependentProportions : Return New SampleSizeMethodDescription(SampleSizeFamily.IndependentProportions, "IndependentProportions", New String() {"controlProp", "experimentalProp", "kappaSuperiority"})
                Case SampleSizePlanningMethod.SingleProportion : Return New SampleSizeMethodDescription(SampleSizeFamily.SingleProportion, "SingleProportion", New String() {"prop", "H0Prop"})
                Case SampleSizePlanningMethod.PairedTTest : Return New SampleSizeMethodDescription(SampleSizeFamily.PairedT, "PairedTTest", New String() {"diff", "sd"})
                Case SampleSizePlanningMethod.NonInferiorityUnpairedTTest : Return New SampleSizeMethodDescription(SampleSizeFamily.UnpairedT, "NonInferiorityUnpairedTTest", New String() {"expectedDifference", "marginAbsolute", "sd", "kappa"})
                Case SampleSizePlanningMethod.EquivalenceUnpairedTTest : Return New SampleSizeMethodDescription(SampleSizeFamily.UnpairedT, "EquivalenceUnpairedTTest", New String() {"expectedDifference", "marginAbsolute", "sd", "kappa"})
                Case SampleSizePlanningMethod.UnpairedTTest : Return New SampleSizeMethodDescription(SampleSizeFamily.UnpairedT, "UnpairedTTest", New String() {"diff", "sdSuperiority", "kappaSuperiority"})
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(method))
            End Select
        End Function
    End Class
End Namespace
