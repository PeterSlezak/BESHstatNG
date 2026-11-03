Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports BESHStatNG.regression

' Source-derived unchanged merged helper. Only declaration visibility/static ownership adapted.
Public NotInheritable Class LegacyMixed14Options
        Public Shared Function ParseFitMethod(arg As Object,
                                       Optional defaultValue As MixedModelFitMethod = MixedModelFitMethod.REML) As MixedModelFitMethod
            Dim token As String = NormalizeToken(AsString(arg))
            If token = "ML" OrElse token = "MAXIMUMLIKELIHOOD" Then Return MixedModelFitMethod.ML
            If token = "REML" OrElse token = "RESTRICTEDML" OrElse token = "RESTRICTEDMAXIMUMLIKELIHOOD" Then Return MixedModelFitMethod.REML
            Return defaultValue
        End Function

        Public Shared Function ParseInferenceMethod(arg As Object,
                                             Optional defaultValue As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger) As MixedModelFixedInferenceMethod
            Dim token As String = NormalizeToken(AsString(arg))
            If token = "KR" OrElse token = "KENWARDROGER" Then Return MixedModelFixedInferenceMethod.KenwardRoger
            If token = "SATTERTHWAITE" OrElse token = "SATTERTHWAITEDF" Then Return MixedModelFixedInferenceMethod.Satterthwaite
            If token = "BETWEENWITHIN" OrElse token = "BW" Then Return MixedModelFixedInferenceMethod.BetweenWithin
            If token = "RESIDUALDF" OrElse token = "RESIDUAL" Then Return MixedModelFixedInferenceMethod.ResidualDF
            If token = "WALD" OrElse token = "WALDNORMAL" OrElse token = "LARGESAMPLENORMAL" OrElse token = "NORMAL" Then Return MixedModelFixedInferenceMethod.WaldNormal
            Return defaultValue
        End Function

        Public Shared Function ParseCovarianceOptimizerMode(arg As Object,
                                                      Optional defaultValue As MixedModelCovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml) As MixedModelCovarianceOptimizerMode
            Dim token As String = NormalizeToken(AsString(arg))
            If token = String.Empty Then Return defaultValue

            If token = "AI" OrElse token = "AVERAGEINFORMATION" OrElse token = "FISHERSCORING" OrElse token = "SAS" OrElse
               token = "AIFISHERSCORINGDEFAULT" OrElse token = "AVERAGEINFORMATIONREML" Then
                Return MixedModelCovarianceOptimizerMode.AverageInformationReml
            End If

            If token = "BFGSANALYTIC" OrElse token = "PROJECTEDBFGSANALYTIC" OrElse
               token = "PROJECTEDBFGSANALYTICGRADIENT" OrElse token = "ANALYTICBFGS" Then
                Return MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            End If

            If token = "BFGS" OrElse token = "PROJECTEDBFGS" OrElse token = "BFGSAUTOGRADIENT" OrElse
               token = "PROJECTEDBFGSAUTOGRADIENT" OrElse token = "BFGSNUMERICAL" OrElse
               token = "PROJECTEDBFGSNUMERICAL" OrElse token = "BFGSFINITE" OrElse
               token = "BFGSFINITEDIFFERENCE" OrElse token = "NUMERICALBFGS" Then
                Return MixedModelCovarianceOptimizerMode.ProjectedBfgs
            End If

            Return defaultValue
        End Function

        Public Shared Function ParseCovarianceGradientMode(arg As Object,
                                                     Optional defaultValue As MixedModelCovarianceGradientMode = MixedModelCovarianceGradientMode.Auto) As MixedModelCovarianceGradientMode
            Dim token As String = NormalizeToken(AsString(arg))
            If token = String.Empty OrElse token = "AUTO" OrElse token = "AUTOANALYTICWHEREAVAILABLE" Then Return defaultValue

            If token = "ANALYTIC" OrElse token = "ANALYTICSCORE" Then
                Return MixedModelCovarianceGradientMode.AnalyticScore
            End If

            If token = "ANALYTICVALIDATION" OrElse token = "VALIDATE" OrElse
               token = "ANALYTICSCOREFINITEDIFFERENCEVALIDATION" OrElse
               token = "ANALYTICSCOREWITHFINITEDIFFERENCEVALIDATION" Then
                Return MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation
            End If

            If token = "NUMERICAL" OrElse token = "FINITE" OrElse token = "FINITEDIFFERENCE" OrElse
               token = "NUMERICALFINITEDIFFERENCE" Then
                Return MixedModelCovarianceGradientMode.NumericalFiniteDifference
            End If

            Return defaultValue
        End Function

        Public Shared Function NormalizeToken(s As String) As String
            If String.IsNullOrWhiteSpace(s) Then Return String.Empty
            Dim chars As IEnumerable(Of Char) = s.Where(Function(ch) Char.IsLetterOrDigit(ch))
            Return New String(chars.ToArray()).ToUpperInvariant()
        End Function

        Public Shared Function AsString(arg As Object) As String
            If arg Is Nothing Then Return String.Empty
            Return Convert.ToString(arg, CultureInfo.InvariantCulture)
        End Function

End Class
