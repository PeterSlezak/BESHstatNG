Option Explicit On
Option Strict On
Option Infer On
Imports System
Namespace ExcelCommon
    ''' <summary>Fresh-mode numeric defaults, separate from disabled/retained UI text.
    ''' Layout-only independent-proportion defaults precede the constructor's final override.</summary>
    Public NotInheritable Class SampleSizePlanningDefaults
        Private Sub New()
        End Sub
        Public Shared Function Inputs(method As SampleSizePlanningMethod, Optional layoutOnly As Boolean = False) As Double()
            Select Case method
                Case SampleSizePlanningMethod.PairedTTest : Return {5.0R, 10.0R}
                Case SampleSizePlanningMethod.UnpairedTTest : Return {5.0R, 10.0R, 1.0R}
                Case SampleSizePlanningMethod.NonInferiorityUnpairedTTest, SampleSizePlanningMethod.EquivalenceUnpairedTTest : Return {0.0R, 5.0R, 10.0R, 1.0R}
                Case SampleSizePlanningMethod.SingleProportion : Return {0.5R, 0.4R}
                Case SampleSizePlanningMethod.IndependentProportions : Return If(layoutOnly, New Double() {0.4R, 0.5R, 1.0R}, New Double() {0.5R, 0.4R, 1.0R})
                Case SampleSizePlanningMethod.NonInferiorityIndependentProportions, SampleSizePlanningMethod.EquivalenceIndependentProportions : Return {0.4R, 0.4R, 0.1R, 1.0R}
                Case SampleSizePlanningMethod.LogRankSampleSize : Return {0.75R, 0.4R, 0.3R, 1.0R}
                Case SampleSizePlanningMethod.CoxEventCountBinaryCovariate : Return {0.75R, 1.0R, 0.0R, 0.4R}
                Case SampleSizePlanningMethod.CoxEventCountContinuousCovariate : Return {1.25R, 1.0R, 0.0R, 0.4R}
                Case SampleSizePlanningMethod.IccHypothesisTestSampleSize : Return {0.6R, 0.8R, 2.0R}
                Case SampleSizePlanningMethod.BlandAltmanLoASampleSize : Return {10.0R, 5.0R, 1.96R}
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(method))
            End Select
        End Function
    End Class
End Namespace
