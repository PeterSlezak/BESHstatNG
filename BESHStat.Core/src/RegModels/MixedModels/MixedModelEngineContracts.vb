Option Explicit On
Option Strict On

Namespace regression

    ''' <summary>
    ''' Internal profiled-likelihood evaluation shared by the mixed-model fitting pipeline.
    ''' </summary>
    Friend Structure MixedModelProfileEvaluation
        Public Success As Boolean
        Public Message As String
        Public Criterion As Double
        Public LogLik As Double
        Public Beta() As Double
        Public VarBeta(,) As Double
        Public XtVinvX(,) As Double
        Public XtVinvY() As Double
        Public QForm As Double
        Public LogDetV As Double
        Public LogDetXtVinvX As Double
        Public Sigma2Profile As Double
    End Structure

End Namespace
