Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.regression

<TestClass>
Public Class MixedModelAnalyticGradientAverageInformationCoreTests

    Private Const TwoPi As Double = 6.2831853071795862R
    Private Const PenaltyObjective As Double = 1.0E+100

    <TestMethod>
    Public Sub AnalyticGradient_IdentityMmrm_MatchesFiniteDifferenceForReml()
        Dim data As MixedModelBlockData = CreateRepeatedInterceptOnlyData()
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                              New IdentityR(),
                                                                              MixedModelFitMethod.REML)
        Dim theta() As Double = {0.0R}
        Dim evaluator As Func(Of Double(), MixedModelProfileEvaluation) =
            Function(candidate() As Double) EvaluateProfile(request, candidate)

        Dim result As MixedModelAnalyticGradientEvaluation = Nothing
        Dim trace As String = String.Empty
        Dim ok As Boolean = MixedModelAnalyticGradient.TryEvaluateGradient(request,
                                                                            theta,
                                                                            Nothing,
                                                                            evaluator,
                                                                            Function() False,
                                                                            result,
                                                                            trace,
                                                                            validateAgainstFiniteDifference:=True)

        Assert.IsTrue(ok, result.Message & Environment.NewLine & trace)
        Assert.IsTrue(result.Success)
        Assert.IsNotNull(result.Gradient)
        Assert.AreEqual(1, result.Gradient.Length)
        Assert.AreEqual(3.0R, result.Gradient(0), 0.000001R,
                        "For sigma^2=1, REML score on the -2 log L scale should be (n-p)-SSE = 11-8 = 3.")
        Assert.IsTrue(result.MaxRelativeFiniteDifferenceDiscrepancy <= 0.00001R,
                      "Analytic score should agree with the optimizer's finite-difference check. maxRel=" &
                      result.MaxRelativeFiniteDifferenceDiscrepancy.ToString("G17", Globalization.CultureInfo.InvariantCulture))
    End Sub

    <TestMethod>
    Public Sub AnalyticGradient_DerivativePatternCache_ReusesRepeatedMmrmPattern()
        Dim data As MixedModelBlockData = CreateRepeatedInterceptOnlyData()
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                              New DiagonalHeterogeneousR(),
                                                                              MixedModelFitMethod.REML)
        Dim control As MixedModelControl = request.Control
        control.UseAnalyticGradientDerivativePatternCache = True
        request.Control = control

        Dim theta() As Double = {0.0R, 0.0R, 0.0R}
        Dim evaluator As Func(Of Double(), MixedModelProfileEvaluation) =
            Function(candidate() As Double) EvaluateProfile(request, candidate)

        Dim result As MixedModelAnalyticGradientEvaluation = Nothing
        Dim trace As String = String.Empty
        Dim ok As Boolean = MixedModelAnalyticGradient.TryEvaluateGradient(request,
                                                                            theta,
                                                                            Nothing,
                                                                            evaluator,
                                                                            Function() False,
                                                                            result,
                                                                            trace,
                                                                            validateAgainstFiniteDifference:=False)

        Assert.IsTrue(ok, result.Message & Environment.NewLine & trace)
        Assert.IsTrue(result.AnalyticDerivativePatternCacheEnabled)
        Assert.AreEqual(1, result.AnalyticDerivativePatternCount,
                        "All four subjects have the same visit and fixed-design pattern.")
        Assert.AreEqual(1, result.AnalyticDerivativePatternCacheMisses)
        Assert.AreEqual(3, result.AnalyticDerivativePatternCacheHits)
        Assert.AreEqual(3, result.AnalyticDerivativeMatricesBuilt,
                        "Only one three-parameter R-side derivative pattern should be built.")
    End Sub

    <TestMethod>
    Public Sub AverageInformationOptimizer_IdentityMmrm_ConvergesToClosedFormRemlVariance()
        Dim data As MixedModelBlockData = CreateRepeatedInterceptOnlyData()
        Dim request As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data,
                                                                              New IdentityR(),
                                                                              MixedModelFitMethod.REML)
        Dim control As MixedModelControl = request.Control
        control.MaxIter = 30
        control.Epsilon = 0.0000001R
        control.StepTolerance = 0.0000001R
        control.FunctionTolerance = 0.000000001R
        request.Control = control

        Dim evaluator As Func(Of Double(), MixedModelProfileEvaluation) =
            Function(candidate() As Double) EvaluateProfile(request, candidate)

        Dim diagnostics As MixedModelAverageInformationDiagnostics = Nothing
        Dim trace As String = String.Empty
        Dim state As MixedModelOptimizationState = MixedModelAverageInformationOptimizer.TryOptimize(request,
                                                                                                       Nothing,
                                                                                                       New Double() {0.0R},
                                                                                                       evaluator,
                                                                                                       Function() False,
                                                                                                       Function() False,
                                                                                                       diagnostics,
                                                                                                       trace)

        Assert.IsTrue(state.Converged, state.Message & Environment.NewLine & trace)
        Assert.IsNotNull(state.Theta)
        Assert.AreEqual(1, state.Theta.Length)

        Dim expectedVariance As Double = 8.0R / 11.0R
        Dim estimatedVariance As Double = Math.Exp(state.Theta(0))
        Assert.AreEqual(expectedVariance, estimatedVariance, 0.00001R,
                        "Intercept-only REML estimate should equal SSE/(n-p).")
        Assert.AreEqual(MixedModelAverageInformationOptimizer.OptimizerName, state.GradientProviderName)
        Assert.IsTrue(diagnostics.InformationMatrixEvaluationCount > 0)
        Assert.IsTrue(diagnostics.InformationMatrixTimeMs >= 0.0R)
        Assert.IsTrue(state.ObjectiveEvaluationCount > 0)
        Assert.IsTrue(state.GradientEvaluationCount > 0)
    End Sub

    Private Shared Function CreateRepeatedInterceptOnlyData() As MixedModelBlockData
        Dim subjectCount As Integer = 4
        Dim visitCount As Integer = 3
        Dim n As Integer = subjectCount * visitCount
        Dim y(n - 1) As Double
        Dim x(n - 1, 0) As Double
        Dim subjectId(n - 1) As Object
        Dim visit(n - 1) As Double
        Dim pattern() As Double = {-1.0R, 0.0R, 1.0R}

        Dim row As Integer = 0
        For s As Integer = 0 To subjectCount - 1
            For v As Integer = 0 To visitCount - 1
                y(row) = pattern(v)
                x(row, 0) = 1.0R
                subjectId(row) = "S" & (s + 1).ToString(Globalization.CultureInfo.InvariantCulture)
                visit(row) = Convert.ToDouble(v)
                row += 1
            Next
        Next

        Return MixedModelBlockData.FromArrays(y:=y,
                                              x:=x,
                                              subjectId:=subjectId,
                                              z:=Nothing,
                                              visit:=visit,
                                              sortWithinSubjectByVisit:=True)
    End Function

    Private Shared Function EvaluateProfile(request As MixedModelFitRequest,
                                            theta() As Double) As MixedModelProfileEvaluation
        Dim result As New MixedModelProfileEvaluation With {
            .Success = False,
            .Message = String.Empty,
            .Criterion = PenaltyObjective,
            .LogLik = Double.NaN,
            .QForm = Double.NaN,
            .LogDetV = Double.NaN,
            .LogDetXtVinvX = Double.NaN,
            .Sigma2Profile = Double.NaN
        }

        Try
            Dim data As MixedModelBlockData = request.Data
            Dim expectedCount As Integer = request.ResidualStruct.ParamCount(data)
            If theta Is Nothing OrElse theta.Length <> expectedCount Then
                result.Message = "Unexpected covariance-parameter length."
                Return result
            End If

            Dim p As Integer = data.P
            Dim n As Integer = data.Nobs
            Dim xtVinvX(p - 1, p - 1) As Double
            Dim xtVinvY(p - 1) As Double
            Dim yVinvY As Double = 0.0R
            Dim logDetV As Double = 0.0R
            Dim noG() As Double = Array.Empty(Of Double)()

            For Each block As MixedModelSubjectBlock In data.Blocks
                Dim trace As String = Nothing
                Dim vi(,) As Double = MixedModelCovariance.BuildVi(block,
                                                                   data,
                                                                   Nothing,
                                                                   request.ResidualStruct,
                                                                   noG,
                                                                   theta,
                                                                   trace)
                Dim chol(,) As Double = Nothing
                If Not MixedModelCovariance.TryCholesky(vi, chol, trace) Then
                    result.Message = "Subject covariance was not positive definite."
                    Return result
                End If

                logDetV += MixedModelCovariance.LogDetFromCholesky(chol)
                MixedModelCovariance.AccumulateProfileCrossProducts(block,
                                                                     chol,
                                                                     xtVinvX,
                                                                     xtVinvY,
                                                                     yVinvY,
                                                                     trace)
            Next

            Dim cholX(,) As Double = Nothing
            Dim xTrace As String = Nothing
            If Not MixedModelCovariance.TryCholesky(xtVinvX, cholX, xTrace) Then
                result.Message = "X'V^-1X was not positive definite."
                Return result
            End If

            Dim beta() As Double = MixedModelCovariance.SolveSPDVector(xtVinvX, xtVinvY)
            Dim betaDot As Double = DotProduct(beta, xtVinvY)
            Dim qForm As Double = Math.Max(0.0R, yVinvY - betaDot)
            Dim logDetX As Double = MixedModelCovariance.LogDetFromCholesky(cholX)
            Dim varBeta(,) As Double = MixedModelCovariance.InverseSPD(xtVinvX)

            Dim df As Integer = If(request.FitMethod = MixedModelFitMethod.REML, n - p, n)
            If df <= 0 Then
                result.Message = "Non-positive residual degrees of freedom."
                Return result
            End If

            Dim criterion As Double
            If request.FitMethod = MixedModelFitMethod.REML Then
                criterion = logDetV + logDetX + qForm + Convert.ToDouble(n - p) * Math.Log(TwoPi)
            Else
                criterion = logDetV + qForm + Convert.ToDouble(n) * Math.Log(TwoPi)
            End If

            If Double.IsNaN(criterion) OrElse Double.IsInfinity(criterion) Then
                result.Message = "Non-finite profile criterion."
                Return result
            End If

            result.Success = True
            result.Message = "OK"
            result.Criterion = criterion
            result.LogLik = -0.5R * criterion
            result.Beta = beta
            result.VarBeta = varBeta
            result.XtVinvX = xtVinvX
            result.XtVinvY = xtVinvY
            result.QForm = qForm
            result.LogDetV = logDetV
            result.LogDetXtVinvX = logDetX
            result.Sigma2Profile = qForm / Convert.ToDouble(df)
            Return result
        Catch ex As Exception
            result.Success = False
            result.Message = ex.Message
            result.Criterion = PenaltyObjective
            Return result
        End Try
    End Function

    Private Shared Function DotProduct(a() As Double, b() As Double) As Double
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then
            Throw New ArgumentException("Vector dimensions must agree.")
        End If
        Dim value As Double = 0.0R
        For i As Integer = 0 To a.Length - 1
            value += a(i) * b(i)
        Next
        Return value
    End Function

End Class
