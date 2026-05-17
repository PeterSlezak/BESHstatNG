Option Explicit On
Option Strict On

Imports BESHStatNG.WorksheetFunctions

Module UdfScalarUtilities

    ''' <summary>
    ''' Normalizes a string for token or key matching by trimming whitespace, optionally changing case,
    ''' and removing selected separator characters.
    ''' </summary>
    Friend Function NormalizeMatchToken(value As String, Optional toUpper As Boolean = True, Optional removeUnderscore As Boolean = False) As String
        If value Is Nothing Then Return String.Empty

        Dim normalized As String = value.Trim()
        normalized = If(toUpper, normalized.ToUpperInvariant(), normalized.ToLowerInvariant())
        normalized = normalized.Replace(" ", String.Empty).Replace("-", String.Empty)
        If removeUnderscore Then normalized = normalized.Replace("_", String.Empty)
        Return normalized
    End Function

    ''' <summary>
    ''' Normalizes an optional text argument for case-insensitive method matching.
    ''' </summary>
    Friend Function NormalizeText(v As Object) As String
        Return NormalizeMatchToken(AsString(v), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes an optional token-style worksheet argument for case-insensitive option parsing.
    ''' </summary>
    Friend Function NormalizeToken(arg As Object) As String
        Return NormalizeMatchToken(AsString(arg), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes a key for case-insensitive dictionary or option matching.
    ''' </summary>
    Friend Function NormalizeKey(value As String) As String
        Return NormalizeMatchToken(value, toUpper:=False, removeUnderscore:=True)
    End Function


    ''' <summary>
    ''' Normalizes a mixed-model worksheet option for strict UDF parsing.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing an option name or alias.</param>
    ''' <returns>Uppercase letters and digits only. Blanks and missing values return an empty string.</returns>
    Friend Function NormalizeMixedModelOptionToken(arg As Object) As String
        Dim raw As String = ExcelArgReaders.AsString(arg)
        If String.IsNullOrWhiteSpace(raw) Then Return String.Empty
        Return New String(raw.Trim().ToUpperInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
    End Function

    ''' <summary>
    ''' Strictly parses a mixed-model likelihood method from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing REML or ML.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected likelihood method. Blank input returns REML.</returns>
    Friend Function ParseMixedModelFitMethodStrict(arg As Object, analysisLabel As String) As regression.MixedModelFitMethod
        Dim token As String = NormalizeMixedModelOptionToken(arg)
        If token = String.Empty OrElse token = "REML" OrElse token = "RESTRICTEDML" OrElse token = "RESTRICTEDMAXIMUMLIKELIHOOD" Then
            Return regression.MixedModelFitMethod.REML
        End If
        If token = "ML" OrElse token = "MAXIMUMLIKELIHOOD" Then Return regression.MixedModelFitMethod.ML

        Throw New ArgumentException("Unsupported " & analysisLabel & " fitMethod. Use REML or ML.")
    End Function

    ''' <summary>
    ''' Strictly parses a mixed-model fixed-effect inference method from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the inference-method name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected inference method. Blank input returns Kenward-Roger.</returns>
    Friend Function ParseMixedModelInferenceMethodStrict(arg As Object, analysisLabel As String) As regression.MixedModelFixedInferenceMethod
        Dim token As String = NormalizeMixedModelOptionToken(arg)
        If token = String.Empty OrElse token = "KR" OrElse token = "KENWARDROGER" OrElse token = "KENWARDROGERDF" OrElse token = "KENWARDROGERF" Then
            Return regression.MixedModelFixedInferenceMethod.KenwardRoger
        End If
        If token = "SAT" OrElse token = "SATTERTHWAITE" OrElse token = "SATTERTHWAITEDF" Then Return regression.MixedModelFixedInferenceMethod.Satterthwaite
        If token = "BW" OrElse token = "BETWEENWITHIN" OrElse token = "BETWEENWITHINDF" Then Return regression.MixedModelFixedInferenceMethod.BetweenWithin
        If token = "RESID" OrElse token = "RESIDUAL" OrElse token = "RESIDUALDF" Then Return regression.MixedModelFixedInferenceMethod.ResidualDF
        If token = "WALD" OrElse token = "NORMAL" OrElse token = "WALDNORMAL" OrElse token = "Z" OrElse token = "LARGESAMPLENORMAL" Then Return regression.MixedModelFixedInferenceMethod.WaldNormal

        Throw New ArgumentException("Unsupported " & analysisLabel & " inference method. Use KR, Satterthwaite, BetweenWithin, ResidualDF, or Wald.")
    End Function

    ''' <summary>
    ''' Strictly parses the mixed-model covariance optimizer option from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the covariance optimizer name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected covariance optimizer. Blank input returns Average Information / Fisher scoring.</returns>
    Friend Function ParseMixedModelCovarianceOptimizerModeStrict(arg As Object, analysisLabel As String) As regression.MixedModelCovarianceOptimizerMode
        Dim token As String = NormalizeMixedModelOptionToken(arg)
        If token = String.Empty OrElse token = "AI" OrElse token = "AIREML" OrElse token = "AVERAGEINFORMATION" OrElse
           token = "AVERAGEINFORMATIONREML" OrElse token = "FISHER" OrElse token = "FISHERSCORING" OrElse
           token = "FISHERSCORE" OrElse token = "SAS" OrElse token = "PROCMIXED" OrElse token = "PROCMIXEDSTYLE" OrElse
           token = "AIFISHERSCORINGDEFAULT" Then
            Return regression.MixedModelCovarianceOptimizerMode.AverageInformationReml
        End If

        If token = "BFGS" OrElse token = "PROJECTEDBFGS" OrElse token = "PROJECTEDBFGSAUTO" OrElse
           token = "BFGSAUTOGRADIENT" OrElse token = "PROJECTEDBFGSAUTOGRADIENT" Then
            Return regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
        End If

        If token = "BFGSANALYTIC" OrElse token = "PROJECTEDBFGSANALYTIC" OrElse
           token = "PROJECTEDBFGSANALYTICGRADIENT" OrElse token = "ANALYTICBFGS" Then
            Return regression.MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
        End If

        If token = "BFGSNUMERICAL" OrElse token = "PROJECTEDBFGSNUMERICAL" OrElse
           token = "BFGSFINITE" OrElse token = "BFGSFINITEDIFFERENCE" OrElse token = "NUMERICALBFGS" Then
            Return regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
        End If

        Throw New ArgumentException("Unsupported " & analysisLabel & " covOptimizerMode. Use AI, AverageInformation, FisherScoring, SAS, BFGS, BFGS_ANALYTIC, or BFGS_NUMERICAL.")
    End Function

    ''' <summary>
    ''' Strictly parses the mixed-model covariance-gradient option from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the gradient mode name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected covariance-gradient mode. Blank input returns automatic selection.</returns>
    Friend Function ParseMixedModelCovarianceGradientModeStrict(arg As Object, analysisLabel As String) As regression.MixedModelCovarianceGradientMode
        Dim token As String = NormalizeMixedModelOptionToken(arg)
        If token = String.Empty OrElse token = "AUTO" OrElse token = "AUTOMATIC" OrElse token = "AUTOANALYTICWHEREAVAILABLE" Then
            Return regression.MixedModelCovarianceGradientMode.Auto
        End If
        If token = "ANALYTIC" OrElse token = "ANALYTICSCORE" OrElse token = "SCORE" Then
            Return regression.MixedModelCovarianceGradientMode.AnalyticScore
        End If
        If token = "ANALYTICVALIDATION" OrElse token = "ANALYTICSCOREVALIDATION" OrElse token = "ANALYTICWITHVALIDATION" OrElse
           token = "VALIDATE" OrElse token = "VALIDATION" OrElse token = "ANALYTICSCOREFINITEDIFFERENCEVALIDATION" OrElse
           token = "ANALYTICSCOREWITHFINITEDIFFERENCEVALIDATION" Then
            Return regression.MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation
        End If
        If token = "NUMERICAL" OrElse token = "FINITE" OrElse token = "FINITEDIFFERENCE" OrElse
           token = "FD" OrElse token = "NUMERICALFINITEDIFFERENCE" Then
            Return regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
        End If

        Throw New ArgumentException("Unsupported " & analysisLabel & " covGradientMode. Use Auto, Analytic, AnalyticValidation, Validate, Numerical, or FiniteDifference.")
    End Function

    ''' <summary>
    ''' Applies optimizer aliases that imply a covariance-gradient mode.
    ''' </summary>
    ''' <param name="optimizerArg">Worksheet argument containing the covariance optimizer option.</param>
    ''' <param name="gradientMode">Gradient mode to update when the optimizer alias implies analytic or numerical gradients.</param>
    Friend Sub ApplyMixedModelOptimizerShortcutToGradient(optimizerArg As Object,
                                                         ByRef gradientMode As regression.MixedModelCovarianceGradientMode)
        Dim token As String = NormalizeMixedModelOptionToken(optimizerArg)
        If token = "BFGSANALYTIC" OrElse token = "PROJECTEDBFGSANALYTIC" OrElse
           token = "PROJECTEDBFGSANALYTICGRADIENT" OrElse token = "ANALYTICBFGS" Then
            gradientMode = regression.MixedModelCovarianceGradientMode.AnalyticScore
        ElseIf token = "BFGSNUMERICAL" OrElse token = "PROJECTEDBFGSNUMERICAL" OrElse
               token = "BFGSFINITE" OrElse token = "BFGSFINITEDIFFERENCE" OrElse token = "NUMERICALBFGS" Then
            gradientMode = regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
        End If
    End Sub

    ''' <summary>
    ''' Normalizes a residual covariance structure name for mixed-model worksheet functions.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the residual covariance structure name or alias.</param>
    ''' <param name="defaultValue">Structure name returned when the argument is blank.</param>
    ''' <returns>A canonical residual covariance structure name understood by the mixed-model engine.</returns>
    Friend Function NormalizeMixedModelResidualCovarianceName(arg As Object, defaultValue As String) As String
        Dim token As String = NormalizeMixedModelOptionToken(arg)
        If token = String.Empty Then Return defaultValue
        If token = "ID" OrElse token = "IDENTITY" OrElse token = "INDEPENDENCE" Then Return "Identity"
        If token = "DIAG" OrElse token = "DIAGONAL" OrElse token = "DIAGONALHETEROGENEOUS" OrElse token = "HETEROGENEOUSDIAGONAL" Then Return "Diagonal"
        If token = "CS" OrElse token = "COMPOUNDSYMMETRY" OrElse token = "EXCHANGEABLE" Then Return "CS"
        If token = "HCS" OrElse token = "CSH" OrElse token = "HETEROGENEOUSCS" OrElse token = "HETEROGENEOUSCOMPOUNDSYMMETRY" Then Return "Heterogeneous CS"
        If token = "AR1" OrElse token = "AR" OrElse token = "AUTOREGRESSIVE" Then Return "AR(1)"
        If token = "HAR1" OrElse token = "HAR" OrElse token = "ARH1" OrElse token = "ARH" OrElse token = "HETEROGENEOUSAR1" OrElse token = "HETEROGENEOUSAR" OrElse token = "HETEROGENEOUSAUTOREGRESSIVE" Then Return "Heterogeneous AR(1)"
        If token = "TOEP" OrElse token = "TOEPLITZ" Then Return "Toeplitz (TOEP)"
        If token = "TOEPH" OrElse token = "TOEPHETEROGENEOUS" OrElse token = "HETEROGENEOUSTOEP" OrElse token = "HETEROGENEOUSTOEPLITZ" OrElse token = "TOEPLITZHETEROGENEOUS" Then Return "Heterogeneous Toeplitz (TOEPH)"
        If token = "UN" OrElse token = "UNSTRUCTURED" Then Return "UN"

        Return ExcelArgReaders.AsString(arg)
    End Function

    Friend Function CloneStringArray(values() As String) As String()
        If values Is Nothing Then Return Nothing
        Return DirectCast(values.Clone(), String())
    End Function

End Module
