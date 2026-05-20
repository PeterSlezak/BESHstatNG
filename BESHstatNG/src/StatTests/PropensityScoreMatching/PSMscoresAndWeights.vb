Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace CausalInference

    ''' <summary>
    ''' Propensity score estimation and balancing-weight helpers.
    ''' This implementation is self-contained so it can be unit-tested without Excel-DNA or the GUI layer.
    ''' </summary>
    Public NotInheritable Class PsmPropensityEstimator
        Private Sub New()
        End Sub

        Public Shared Function Estimate(input As PsmInputData, options As PsmOptions) As PsmScoreModelResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If options Is Nothing Then Throw New ArgumentNullException("options")
            input.Validate(options)

            If options.ScoreMethod = PsmScoreMethod.Supplied Then
                Return FromSupplied(input)
            End If

            Return FitLogisticRegression(input, options)
        End Function

        Private Shared Function FromSupplied(input As PsmInputData) As PsmScoreModelResult
            Dim n As Integer = input.RowCount
            Dim scores(n - 1) As Double
            Dim eta(n - 1) As Double
            For i As Integer = 0 To n - 1
                scores(i) = PsmMath.Clamp(input.SuppliedPropensityScores(i), 0.000000000001, 0.999999999999)
                eta(i) = PsmMath.SafeLogit(scores(i))
            Next

            Return New PsmScoreModelResult With {
                .Method = PsmScoreMethod.Supplied,
                .Scores = scores,
                .LinearPredictor = eta,
                .Converged = True,
                .Iterations = 0,
                .LogLikelihood = Double.NaN
            }
        End Function

        Public Shared Function FitLogisticRegression(input As PsmInputData, options As PsmOptions) As PsmScoreModelResult
            Dim n As Integer = input.RowCount
            Dim pCov As Integer = input.CovariateCount
            Dim p As Integer = pCov + If(options.IncludeIntercept, 1, 0)
            If n <= p Then Throw New ArgumentException("There are not enough observations to estimate the propensity score model.")

            Dim fitData(n - 1, pCov) As Double
            Dim variableNames(p - 1) As String
            Dim glmVarNames(pCov) As String
            glmVarNames(0) = "Treatment"

            If options.IncludeIntercept Then variableNames(0) = "Intercept"
            Dim variableOffset As Integer = If(options.IncludeIntercept, 1, 0)

            For i As Integer = 0 To n - 1
                fitData(i, 0) = If(input.Treatment(i) >= 0.5, 1.0, 0.0)
            Next

            For j As Integer = 0 To pCov - 1
                Dim covariateName As String = input.GetCovariateName(j)
                glmVarNames(j + 1) = covariateName
                variableNames(j + variableOffset) = covariateName

                Dim col(n - 1) As Double
                For i As Integer = 0 To n - 1
                    col(i) = input.Covariates(i, j)
                Next
                Dim mean As Double = If(options.StandardizeCovariates, col.Average(), 0.0)
                Dim sd As Double = If(options.StandardizeCovariates, StatFunc.stDev(col), 1.0)
                If Not AppInfrastructure.IsFinite(sd) OrElse sd <= 0 Then sd = 1.0
                For i As Integer = 0 To n - 1
                    fitData(i, j + 1) = If(options.StandardizeCovariates, (input.Covariates(i, j) - mean) / sd, input.Covariates(i, j))
                Next
            Next

            Dim warningList As New List(Of String)()
            Dim glm As New GLM(New regression.Binomial(), New regression.Logit())
            glm.bHosmerLemeshow = False
            glm.bComputeResiduals = False
            glm.bReturnCov = False
            glm.bIterationDetails = False
            glm.settingInputs(0.05, options.LogisticMaxIterations, options.LogisticTolerance)
            ApplyGlmRidgeOptions(glm, options, warningList)
            glm.data(fitData)
            glm.setVarNames(glmVarNames)
            glm.Fit(If(options.IncludeIntercept, 1, 0), False)

            If glm.results Is Nothing OrElse glm.results.Coeffs_est Is Nothing Then
                Dim msg As String = "Propensity score logistic regression did not return coefficient estimates."
                If Not String.IsNullOrWhiteSpace(glm.strError) Then msg &= " " & glm.strError.Trim()
                Throw New InvalidOperationException(msg)
            End If

            Dim scores As Double() = PsmMath.CopyVector(glm.PredictedResponses)
            Dim finalEta As Double() = PsmMath.CopyVector(glm.LinPred)
            If scores Is Nothing OrElse scores.Length <> n Then Throw New InvalidOperationException("GLM did not return a fitted propensity score for every row.")
            If finalEta Is Nothing OrElse finalEta.Length <> n Then Throw New InvalidOperationException("GLM did not return a linear predictor for every row.")


            For i As Integer = 0 To n - 1
                scores(i) = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                If Not AppInfrastructure.IsFinite(finalEta(i)) Then finalEta(i) = PsmMath.SafeLogit(scores(i))
            Next

            If Not glm.Converged Then warningList.Add("Propensity-score GLM did not converge within the configured iteration limit.")
            If glm.bSeparation Then warningList.Add("Complete separation was detected in the propensity-score GLM; scores and matches may be unstable.")
            If glm.bQuasiSeparation Then warningList.Add("Quasi-separation was detected in the propensity-score GLM; scores and matches may be unstable.")
            If Not String.IsNullOrWhiteSpace(glm.strError) Then warningList.Add(glm.strError.Trim())
            If scores.Min() < 0.01 OrElse scores.Max() > 0.99 Then warningList.Add("Some estimated propensity scores are close to 0 or 1; overlap and weight stability should be checked.")

            Return New PsmScoreModelResult With {
                .Method = PsmScoreMethod.LogisticRegression,
                .Scores = scores,
                .LinearPredictor = finalEta,
                .Coefficients = PsmMath.CopyVector(glm.results.Coeffs_est),
                .StandardErrors = PsmMath.CopyVector(glm.results.Coeffs_SEs),
                .VariableNames = variableNames,
                .Converged = glm.Converged,
                .Iterations = glm.pIRLSiterations,
                .LogLikelihood = glm.LogLikelihoodUnscaled,
                .Warnings = warningList
            }
        End Function

        Private Shared Sub ApplyGlmRidgeOptions(glm As GLM, options As PsmOptions, warnings As List(Of String))
            If glm Is Nothing OrElse options Is Nothing Then Return
            If options.LogisticRidgePenalty <= 0 Then Return

            Try
                glm.WlsRidgePenalty = options.LogisticRidgePenalty
                glm.WlsRidgeExcludeIntercept = True
            Catch ex As Exception
                warnings.Add("Could not apply GLM ridge penalty for PSM logistic fitting: " & ex.Message)
            End Try
        End Sub
    End Class

    Public NotInheritable Class PsmWeightEngine
        Private Sub New()
        End Sub

        Public Shared Function ComputeBalancingWeights(input As PsmInputData, scores As Double(), options As PsmOptions) As Double()
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If scores Is Nothing OrElse scores.Length <> input.RowCount Then Throw New ArgumentException("Scores must match input row count.")
            If options Is Nothing Then Throw New ArgumentNullException("options")

            Dim n As Integer = input.RowCount
            Dim weights(n - 1) As Double
            For i As Integer = 0 To n - 1
                Dim p As Double = PsmMath.Clamp(scores(i), 0.00000001, 0.99999999)
                Dim treated As Boolean = input.Treatment(i) >= 0.5
                If p < options.TrimPropensityLower OrElse p > options.TrimPropensityUpper Then
                    weights(i) = 0.0
                Else
                    Select Case options.Estimand
                        Case PsmEstimand.ATE
                            weights(i) = If(treated, 1.0 / p, 1.0 / (1.0 - p))
                        Case PsmEstimand.ATT
                            weights(i) = If(treated, 1.0, p / (1.0 - p))
                        Case PsmEstimand.ATC
                            weights(i) = If(treated, (1.0 - p) / p, 1.0)
                        Case PsmEstimand.ATO
                            weights(i) = If(treated, 1.0 - p, p)
                        Case Else
                            weights(i) = 1.0
                    End Select
                End If
            Next

            If options.NormalizeWeightsToSampleSize Then NormalizeByGroup(input, weights)
            Return weights
        End Function

        Private Shared Sub NormalizeByGroup(input As PsmInputData, weights As Double())
            Dim sumT As Double = 0.0
            Dim sumC As Double = 0.0
            Dim nT As Integer = 0
            Dim nC As Integer = 0
            For i As Integer = 0 To input.RowCount - 1
                If input.Treatment(i) >= 0.5 Then
                    nT += 1
                    sumT += weights(i)
                Else
                    nC += 1
                    sumC += weights(i)
                End If
            Next
            Dim scaleT As Double = If(sumT > 0, CDbl(nT) / sumT, 1.0)
            Dim scaleC As Double = If(sumC > 0, CDbl(nC) / sumC, 1.0)
            For i As Integer = 0 To input.RowCount - 1
                If input.Treatment(i) >= 0.5 Then
                    weights(i) *= scaleT
                Else
                    weights(i) *= scaleC
                End If
            Next
        End Sub
    End Class

End Namespace
