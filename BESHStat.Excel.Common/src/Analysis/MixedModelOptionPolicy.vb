Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports BESHStatNG.regression

' Portable authority for the original front-end option parsers; exact established aliases.
Namespace ExcelCommon
    Public NotInheritable Class MixedModelOptionPolicy
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

        ' Validation-only projection of the unchanged Core factory alias branches.
        ' Stateless Core structures supply UsesVisitIndex/type metadata; no covariance
        ' calculations, logger calls or new aliases. Actual request creation uses Core.
        Friend Shared Function ResidualForValidation(type As String) As MixedModelRStruct
            Dim f As MixedModelRStruct
            Dim normalized As String = If(type, String.Empty).Trim().ToLowerInvariant()

            If normalized = "identity" OrElse normalized = "id" OrElse normalized = "independence" Then
                f = New IdentityR()
            ElseIf normalized = "diagonal heterogeneous" OrElse normalized = "diag heterogeneous" _
                OrElse normalized = "diagonal" OrElse normalized = "diag" Then
                f = New DiagonalHeterogeneousR()
            ElseIf normalized = "compound symmetry" OrElse normalized = "compound-symmetry" _
                OrElse normalized = "cs" OrElse normalized = "exchangeable" Then
                f = New CompoundSymmetryR()
            ElseIf normalized = "heterogeneous cs" OrElse normalized = "heterogeneous compound symmetry" _
                OrElse normalized = "csh" Then
                f = New HeterogeneousCSR()
            ElseIf normalized = "ar(1)" OrElse normalized = "ar1" OrElse normalized = "autoregressive" Then
                f = New AR1R()
            ElseIf normalized = "heterogeneous ar(1)" OrElse normalized = "heterogeneous ar1" _
                OrElse normalized = "heterogeneous autoregressive" OrElse normalized = "arh(1)" _
                OrElse normalized = "arh1" OrElse normalized = "arh" OrElse normalized = "har1" _
                OrElse normalized = "har(1)" OrElse normalized = "har" Then
                f = New HeterogeneousAR1R()
            ElseIf normalized = "toeplitz" OrElse normalized = "toep" OrElse normalized = "toeplitz (toep)" Then
                f = New ToeplitzR()
            ElseIf normalized = "heterogeneous toeplitz" OrElse normalized = "toeplitz heterogeneous" _
                OrElse normalized = "toeph" OrElse normalized = "toep h" OrElse normalized = "toeplitz h" _
                OrElse normalized = "toeplitz (heterogeneous)" _
                OrElse normalized = "heterogeneous toeplitz (toeph)" Then
                f = New HeterogeneousToeplitzR()
            ElseIf normalized = "unstructured" OrElse normalized = "un" Then
                f = New UnstructuredR()
            Else
                Return Nothing
            End If

            Return f
        End Function

        ' Validation-only projection of the unchanged Core factory alias branches.
        ' Stateless Core structures supply UsesVisitIndex/type metadata; no covariance
        ' calculations, logger calls or new aliases. Actual request creation uses Core.
        Friend Shared Function RandomForValidation(type As String) As MixedModelGStruct
            Dim f As MixedModelGStruct
            Dim normalized As String = If(type, String.Empty).Trim().ToLowerInvariant()

            If normalized = String.Empty OrElse normalized = "none" OrElse normalized = "no random effects" _
                OrElse normalized = "mmrm" Then
                f = New NoRandomEffects()
            ElseIf normalized = "random intercept" OrElse normalized = "ri" OrElse normalized = "intercept" Then
                f = New RandomIntercept()
            ElseIf normalized = "random intercept + slope" OrElse normalized = "random intercept+slope" _
                OrElse normalized = "random intercept and slope" OrElse normalized = "ri+s" _
                OrElse normalized = "ris" Then
                f = New RandomInterceptSlope()
            ElseIf normalized = "variance components (vc/diag)" OrElse normalized = "variance components" _
                OrElse normalized = "variance component" OrElse normalized = "vc" _
                OrElse normalized = "diag" OrElse normalized = "diagonal" _
                OrElse normalized = "diagonal random effects" OrElse normalized = "independent random effects" Then
                f = New VarianceComponentsRandomEffects()
            ElseIf normalized = "identity" OrElse normalized = "id" OrElse normalized = "identity random effects" _
                OrElse normalized = "equal variance independent" Then
                f = New IdentityRandomEffects()
            ElseIf normalized = "compound symmetry" OrElse normalized = "compound symmetry (cs)" _
                OrElse normalized = "cs" OrElse normalized = "exchangeable" OrElse normalized = "exchangeable random effects" Then
                f = New CompoundSymmetryRandomEffects()
            ElseIf normalized = "heterogeneous compound symmetry" OrElse normalized = "heterogeneous compound symmetry (csh)" _
                OrElse normalized = "csh" OrElse normalized = "cshp" OrElse normalized = "heterogeneous cs" Then
                f = New HeterogeneousCompoundSymmetryRandomEffects()
            ElseIf normalized = "autoregressive" OrElse normalized = "autoregressive (ar1)" _
                OrElse normalized = "ar1" OrElse normalized = "ar(1)" OrElse normalized = "ar 1" Then
                f = New AutoregressiveRandomEffects()
            ElseIf normalized = "heterogeneous autoregressive" OrElse normalized = "heterogeneous autoregressive (arh1)" _
                OrElse normalized = "heterogeneous ar1" OrElse normalized = "heterogeneous ar(1)" _
                OrElse normalized = "arh1" OrElse normalized = "arh(1)" OrElse normalized = "arh 1" _
                OrElse normalized = "arh" OrElse normalized = "arh1 random effects" Then
                f = New HeterogeneousAutoregressiveRandomEffects()
            ElseIf normalized = "toeplitz" OrElse normalized = "toeplitz (toep)" _
                OrElse normalized = "toep" OrElse normalized = "toeplitz random effects" Then
                f = New ToeplitzRandomEffects()
            ElseIf normalized = "heterogeneous toeplitz" OrElse normalized = "heterogeneous toeplitz (toeph)" _
                OrElse normalized = "toeph" OrElse normalized = "toeph random effects" _
                OrElse normalized = "toep h" OrElse normalized = "toeplitz heterogeneous" Then
                f = New HeterogeneousToeplitzRandomEffects()
            ElseIf normalized = "unstructured random effects" OrElse normalized = "unstructured" _
                OrElse normalized = "un" Then
                f = New UnstructuredRandomEffects()
            Else
                Return Nothing
            End If

            Return f
        End Function
    End Class
End Namespace
