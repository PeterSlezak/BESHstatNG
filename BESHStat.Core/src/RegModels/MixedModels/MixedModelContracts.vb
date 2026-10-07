Option Explicit On
Option Strict On

Namespace regression

    ''' <summary>
    ''' Selects how the covariance-parameter gradient is supplied to the projected BFGS optimizer.
    ''' </summary>
    Public Enum MixedModelCovarianceGradientMode
        ''' <summary>Use the current central finite-difference gradient inside MixedModelOptimizer.</summary>
        NumericalFiniteDifference = 0

        ''' <summary>Use an analytic score for the profiled ML/REML objective when a provider is available.</summary>
        AnalyticScore = 1

        ''' <summary>Use the analytic score and validate it against finite differences when validation is implemented.</summary>
        AnalyticScoreWithFiniteDifferenceValidation = 2

        ''' <summary>Automatically use the analytic score for supported structures and finite differences otherwise.</summary>
        Auto = 3
    End Enum

End Namespace
