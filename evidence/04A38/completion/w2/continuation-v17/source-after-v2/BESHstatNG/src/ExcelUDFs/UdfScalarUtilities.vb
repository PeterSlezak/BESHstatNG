Option Explicit On
Option Strict On

Imports BESHStatNG.WorksheetFunctions

Module UdfScalarUtilities

    ''' <summary>
    ''' Normalizes a string for token or key matching by trimming whitespace, optionally changing case,
    ''' and removing selected separator characters.
    ''' </summary>
    Friend Function NormalizeMatchToken(value As String, Optional toUpper As Boolean = True, Optional removeUnderscore As Boolean = False) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeMatchToken(value, toUpper, removeUnderscore)
    End Function

    ''' <summary>
    ''' Normalizes an optional text argument for case-insensitive method matching.
    ''' </summary>
    Friend Function NormalizeText(v As Object) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeText(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(v))
    End Function

    ''' <summary>
    ''' Normalizes an optional token-style worksheet argument for case-insensitive option parsing.
    ''' </summary>
    Friend Function NormalizeToken(arg As Object) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeToken(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg))
    End Function

    ''' <summary>
    ''' Normalizes a key for case-insensitive dictionary or option matching.
    ''' </summary>
    Friend Function NormalizeKey(value As String) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeKey(value)
    End Function


    ''' <summary>
    ''' Normalizes a mixed-model worksheet option for strict UDF parsing.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing an option name or alias.</param>
    ''' <returns>Uppercase letters and digits only. Blanks and missing values return an empty string.</returns>
    Friend Function NormalizeMixedModelOptionToken(arg As Object) As String
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.NormalizeMixedModelOptionToken(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg))
    End Function

    ''' <summary>
    ''' Strictly parses a mixed-model likelihood method from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing REML or ML.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected likelihood method. Blank input returns REML.</returns>
    Friend Function ParseMixedModelFitMethodStrict(arg As Object, analysisLabel As String) As regression.MixedModelFitMethod
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.ParseMixedModelFitMethodStrict(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), analysisLabel)
    End Function

    ''' <summary>
    ''' Strictly parses a mixed-model fixed-effect inference method from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the inference-method name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected inference method. Blank input returns Kenward-Roger.</returns>
    Friend Function ParseMixedModelInferenceMethodStrict(arg As Object, analysisLabel As String) As regression.MixedModelFixedInferenceMethod
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.ParseMixedModelInferenceMethodStrict(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), analysisLabel)
    End Function

    ''' <summary>
    ''' Strictly parses the mixed-model covariance optimizer option from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the covariance optimizer name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected covariance optimizer. Blank input returns Average Information / Fisher scoring.</returns>
    Friend Function ParseMixedModelCovarianceOptimizerModeStrict(arg As Object, analysisLabel As String) As regression.MixedModelCovarianceOptimizerMode
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.ParseMixedModelCovarianceOptimizerModeStrict(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), analysisLabel)
    End Function

    ''' <summary>
    ''' Strictly parses the mixed-model covariance-gradient option from a worksheet argument.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the gradient mode name or alias.</param>
    ''' <param name="analysisLabel">User-facing analysis label used in error messages.</param>
    ''' <returns>The selected covariance-gradient mode. Blank input returns automatic selection.</returns>
    Friend Function ParseMixedModelCovarianceGradientModeStrict(arg As Object, analysisLabel As String) As regression.MixedModelCovarianceGradientMode
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.ParseMixedModelCovarianceGradientModeStrict(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), analysisLabel)
    End Function

    ''' <summary>
    ''' Applies optimizer aliases that imply a covariance-gradient mode.
    ''' </summary>
    ''' <param name="optimizerArg">Worksheet argument containing the covariance optimizer option.</param>
    ''' <param name="gradientMode">Gradient mode to update when the optimizer alias implies analytic or numerical gradients.</param>
    Friend Sub ApplyMixedModelOptimizerShortcutToGradient(optimizerArg As Object,
                                                         ByRef gradientMode As regression.MixedModelCovarianceGradientMode)
        Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.ApplyMixedModelOptimizerShortcutToGradient(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(optimizerArg), gradientMode)
    End Sub

    ''' <summary>
    ''' Normalizes a residual covariance structure name for mixed-model worksheet functions.
    ''' </summary>
    ''' <param name="arg">Worksheet argument containing the residual covariance structure name or alias.</param>
    ''' <param name="defaultValue">Structure name returned when the argument is blank.</param>
    ''' <returns>A canonical residual covariance structure name understood by the mixed-model engine.</returns>
    Friend Function NormalizeMixedModelResidualCovarianceName(arg As Object, defaultValue As String) As String
        Dim canonical As String = Nothing
        If Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38.TryClassifyMixedModelResidualCovarianceName(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), defaultValue, canonical) Then Return canonical
        Return Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
    End Function

    Friend Function CloneStringArray(values() As String) As String()
        Return Global.BESHStatNG.ExcelCommon.UdfScalarUtilitiesMetadata38.CloneStringArray(values)
    End Function

End Module
