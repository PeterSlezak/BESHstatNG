Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics

Namespace regression

    ''' <summary>
    ''' Diagnostics returned by one Average Information / Fisher-scoring covariance-optimizer run.
    ''' </summary>
    Friend Structure MixedModelAverageInformationDiagnostics
        Public IterationCount As Integer
        Public StepHalvingCount As Integer
        Public RidgeAdjustmentCount As Integer
        Public InformationMatrixEvaluationCount As Integer
        Public InformationMatrixTimeMs As Double
        Public Message As String
    End Structure

    ''' <summary>
    ''' Experimental Average Information / Fisher-scoring optimizer for profiled REML covariance parameters.
    ''' </summary>
    ''' <remarks>
    ''' AG-BFGS-P8 keeps the existing projected BFGS path as the default and adds this optimizer only behind
    ''' an explicit control option. The score vector is the same analytic score used by analytic-gradient BFGS.
    ''' The AI matrix is accumulated from block-level covariance derivatives and the profiled projection matrix.
    ''' The optimizer uses diagonal ridge stabilization plus step halving, and falls back to the existing BFGS
    ''' path when the engine decides this experimental path is unavailable or unsuccessful.
    ''' </remarks>
    Friend Module MixedModelAverageInformationOptimizer

        Friend Const OptimizerName As String = "Average Information REML"
        Private Const PenaltyObjective As Double = 1.0E+100
        Private Const DefaultInitialRidge As Double = 0.000001
        Private Const DefaultMaxRidgeAttempts As Integer = 8
        Private Const DefaultMaxStepHalving As Integer = 12
        Private Const DefaultMinStepSize As Double = 0.000000000001

        Friend Function TryOptimize(request As MixedModelFitRequest,
                                    activeG As MixedModelGStruct,
                                    startTheta() As Double,
                                    profileEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                    cancellationRequested As Func(Of Boolean),
                                    interruptionRequested As Func(Of Boolean),
                                    ByRef diagnostics As MixedModelAverageInformationDiagnostics,
                                    Optional ByRef strTrace As String = Nothing) As MixedModelOptimizationState
            diagnostics = New MixedModelAverageInformationDiagnostics With {
                .IterationCount = 0,
                .StepHalvingCount = 0,
                .RidgeAdjustmentCount = 0,
                .InformationMatrixEvaluationCount = 0,
                .InformationMatrixTimeMs = Double.NaN,
                .Message = String.Empty
            }

            Dim state As MixedModelOptimizationState = CreateInitialState(startTheta)

            Try
                If request Is Nothing Then Return FailState(state, MixedModelOptimizationStatus.InvalidInput, "Mixed-model request is missing.", strTrace)
                If startTheta Is Nothing Then Return FailState(state, MixedModelOptimizationStatus.InvalidInput, "Starting covariance-parameter vector is missing.", strTrace)
                If profileEvaluator Is Nothing Then Return FailState(state, MixedModelOptimizationStatus.InvalidInput, "Profile objective evaluator is missing.", strTrace)
                If request.FitMethod <> MixedModelFitMethod.REML Then Return FailState(state, MixedModelOptimizationStatus.InvalidInput, "Average Information optimizer is currently available only for REML fits.", strTrace)

                Dim supportMessage As String = Nothing
                If Not IsSupportedRequest(request, activeG, supportMessage) Then
                    Return FailState(state, MixedModelOptimizationStatus.InvalidInput, supportMessage, strTrace)
                End If

                ThrowIfCancellationRequested(cancellationRequested)

                Dim theta() As Double = CloneVector(startTheta)
                Dim current As MixedModelProfileEvaluation = profileEvaluator(theta)
                state.ObjectiveEvaluationCount += 1
                If Not current.Success OrElse Not IsFiniteUsableObjective(current.Criterion) Then
                    Return FailState(state, MixedModelOptimizationStatus.NonFiniteObjective, "Starting objective is non-finite or penalized for Average Information optimization.", strTrace)
                End If

                state.Theta = CloneVector(theta)
                state.Objective = current.Criterion
                state.Status = MixedModelOptimizationStatus.NotStarted
                state.Message = "Average Information optimization initialized."
                state.GradientProviderName = OptimizerName

                If theta.Length = 0 Then
                    state.Converged = True
                    state.Status = MixedModelOptimizationStatus.ConvergedGradient
                    state.Message = "No covariance parameters to optimize; profiled criterion evaluated once."
                    Return state
                End If

                Dim maxIter As Integer = Math.Max(1, request.Control.MaxIter)
                Dim gradTol As Double = If(request.Control.Epsilon > 0.0, request.Control.Epsilon, 0.00000001)
                Dim stepTol As Double = If(request.Control.StepTolerance > 0.0, request.Control.StepTolerance, 0.0000001)
                Dim funcTol As Double = If(request.Control.FunctionTolerance > 0.0, request.Control.FunctionTolerance, 0.000000001)
                Dim traceRows As New List(Of Double())()
                Dim infoTimeTotal As Double = 0.0

                For iter As Integer = 1 To maxIter
                    ThrowIfCancellationRequested(cancellationRequested)
                    If IsRequested(interruptionRequested) Then
                        MarkInterruptedState(state, theta, current.Criterion, iter - 1, strTrace)
                        Return state
                    End If

                    Dim aiResult As MixedModelAverageInformationEvaluation = Nothing
                    Dim sw As Stopwatch = Stopwatch.StartNew()
                    If Not TryEvaluateAverageInformation(request,
                                                          theta,
                                                          activeG,
                                                          profileEvaluator,
                                                          cancellationRequested,
                                                          aiResult,
                                                          strTrace) Then
                        Return FailState(state, MixedModelOptimizationStatus.InvalidInput, aiResult.Message, strTrace)
                    End If
                    sw.Stop()
                    infoTimeTotal += sw.Elapsed.TotalMilliseconds
                    diagnostics.InformationMatrixEvaluationCount += 1
                    state.GradientEvaluationCount += 1
                    state.ObjectiveEvaluationCount += 2 ' TryEvaluateAverageInformation evaluates the profile for the analytic score and AI matrix.

                    Dim grad() As Double = aiResult.Gradient
                    Dim gradNorm As Double = Matrix.VectorNorm(grad)
                    state.GradNorm = gradNorm
                    state.Iterations = iter - 1
                    state.Objective = aiResult.Criterion
                    current.Criterion = aiResult.Criterion

                    If gradNorm <= gradTol Then
                        state.Converged = True
                        state.Status = MixedModelOptimizationStatus.ConvergedGradient
                        state.Message = "Converged: Average Information gradient norm " & gradNorm.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & " <= tolerance " & gradTol.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & "."
                        Exit For
                    End If

                    Dim stepDirection() As Double = Nothing
                    If Not TrySolveInformationStep(aiResult.InformationMatrix,
                                                   grad,
                                                   stepDirection,
                                                   diagnostics,
                                                   strTrace) Then
                        Return FailState(state, MixedModelOptimizationStatus.LineSearchFailed, "Average Information matrix could not be stabilized with diagonal ridge.", strTrace)
                    End If

                    Dim accepted As Boolean = False
                    Dim bestTheta() As Double = CloneVector(theta)
                    Dim bestEval As MixedModelProfileEvaluation = current
                    Dim stepScale As Double = 1.0
                    Dim stepNorm As Double = 0.0

                    For halve As Integer = 0 To DefaultMaxStepHalving
                        ThrowIfCancellationRequested(cancellationRequested)
                        If IsRequested(interruptionRequested) Then
                            MarkInterruptedState(state, theta, current.Criterion, iter - 1, strTrace)
                            Return state
                        End If

                        Dim trial() As Double = AddScaled(theta, stepDirection, stepScale)
                        stepNorm = Distance(theta, trial)
                        If stepNorm <= stepTol Then Exit For

                        Dim trialEval As MixedModelProfileEvaluation = profileEvaluator(trial)
                        state.ObjectiveEvaluationCount += 1
                        state.LineSearchEvaluationCount += 1

                        If trialEval.Success AndAlso IsFiniteUsableObjective(trialEval.Criterion) AndAlso trialEval.Criterion < current.Criterion Then
                            accepted = True
                            bestTheta = trial
                            bestEval = trialEval
                            Exit For
                        End If

                        stepScale *= 0.5
                        diagnostics.StepHalvingCount += 1
                        If stepScale < DefaultMinStepSize Then Exit For
                    Next

                    If Not accepted Then
                        state.Converged = False
                        state.Status = MixedModelOptimizationStatus.LineSearchFailed
                        state.Message = "Average Information step-halving failed to find a finite improving objective."
                        state.StepSize = stepScale
                        state.StepNorm = stepNorm
                        Exit For
                    End If

                    Dim fOld As Double = current.Criterion
                    theta = bestTheta
                    current = bestEval
                    Dim fChange As Double = Math.Abs(fOld - current.Criterion)

                    state.Theta = CloneVector(theta)
                    state.Objective = current.Criterion
                    state.Iterations = iter
                    state.StepNorm = stepNorm
                    state.StepSize = stepScale
                    state.FunctionChange = fChange
                    diagnostics.IterationCount = iter
                    traceRows.Add(New Double() {CDbl(iter), fOld, gradNorm, stepNorm, stepScale, current.Criterion})

                    If stepNorm <= stepTol Then
                        state.Converged = True
                        state.Status = MixedModelOptimizationStatus.ConvergedStep
                        state.Message = "Converged: Average Information accepted step norm " & stepNorm.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & " <= tolerance " & stepTol.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & "."
                        Exit For
                    End If

                    If fChange <= funcTol Then
                        state.Converged = True
                        state.Status = MixedModelOptimizationStatus.ConvergedFunction
                        state.Message = "Converged: Average Information objective change " & fChange.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & " <= tolerance " & funcTol.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & "."
                        Exit For
                    End If

                    If iter = maxIter Then
                        state.Converged = False
                        state.Status = MixedModelOptimizationStatus.IterationLimit
                        state.Message = "Average Information maximum iterations reached before convergence."
                    End If
                Next

                diagnostics.InformationMatrixTimeMs = infoTimeTotal
                diagnostics.Message = state.Message
                state.TraceTable = BuildTraceMatrix(traceRows)
                state.strTrace = strTrace
                Return state

            Catch ex As OperationCanceledException
                state.Status = MixedModelOptimizationStatus.Cancelled
                state.Message = If(String.IsNullOrWhiteSpace(ex.Message), "Optimization cancelled by user.", ex.Message)
                state.Converged = False
                state.strTrace = strTrace
                Return state
            Catch ex As Exception
                state.Status = MixedModelOptimizationStatus.InvalidInput
                state.Message = "Average Information optimizer failed: " & ex.Message
                state.Converged = False
                state.strTrace = strTrace
                diagnostics.Message = state.Message
                Return state
            Finally
                If Not AppInfrastructure.NumericGuards.IsFinite(diagnostics.InformationMatrixTimeMs) Then diagnostics.InformationMatrixTimeMs = 0.0
            End Try
        End Function

        Private Structure MixedModelAverageInformationEvaluation
            Public Success As Boolean
            Public Message As String
            Public Criterion As Double
            Public Gradient() As Double
            Public InformationMatrix(,) As Double
        End Structure

        Private Function TryEvaluateAverageInformation(request As MixedModelFitRequest,
                                                       theta() As Double,
                                                       activeG As MixedModelGStruct,
                                                       profileEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                                       cancellationRequested As Func(Of Boolean),
                                                       ByRef result As MixedModelAverageInformationEvaluation,
                                                       ByRef strTrace As String) As Boolean
            result = New MixedModelAverageInformationEvaluation With {
                .Success = False,
                .Message = String.Empty,
                .Criterion = Double.NaN,
                .Gradient = Nothing,
                .InformationMatrix = Nothing
            }

            Dim gradientEval As MixedModelAnalyticGradientEvaluation = Nothing
            If Not MixedModelAnalyticGradient.TryEvaluateGradient(request,
                                                                  theta,
                                                                  activeG,
                                                                  profileEvaluator,
                                                                  cancellationRequested,
                                                                  gradientEval,
                                                                  strTrace,
                                                                  validateAgainstFiniteDifference:=False) Then
                result.Message = If(String.IsNullOrWhiteSpace(gradientEval.Message), "Analytic score evaluation failed for Average Information optimization.", gradientEval.Message)
                Return False
            End If

            Dim data As MixedModelBlockData = request.Data
            Dim gCount As Integer = If(activeG Is Nothing, 0, activeG.ParamCount(data.Q))
            Dim rCount As Integer = request.ResidualStruct.ParamCount(data)
            Dim paramCount As Integer = gCount + rCount
            Dim thetaG() As Double = SliceVector(theta, 0, gCount)
            Dim thetaR() As Double = SliceVector(theta, gCount, rCount)
            Dim info(paramCount - 1, paramCount - 1) As Double

            Dim profile As MixedModelProfileEvaluation = profileEvaluator(theta)
            If Not profile.Success OrElse Not IsFiniteUsableObjective(profile.Criterion) Then
                result.Message = "Profile objective evaluation failed while building the Average Information matrix: " & If(profile.Message, String.Empty)
                Return False
            End If

            For Each block As MixedModelSubjectBlock In data.Blocks
                ThrowIfCancellationRequested(cancellationRequested)

                Dim evalTrace As String = Nothing
                Dim vi(,) As Double = MixedModelCovariance.BuildVi(block,
                                                                   data,
                                                                   activeG,
                                                                   request.ResidualStruct,
                                                                   thetaG,
                                                                   thetaR,
                                                                   evalTrace)
                Dim chol(,) As Double = Nothing
                If Not MixedModelCovariance.TryCholesky(vi, chol, evalTrace) Then
                    result.Message = "V_i was not positive definite while building Average Information matrix for subject '" & block.SubjectKey & "'."
                    Return False
                End If

                Dim x(,) As Double = block.X
                Dim y() As Double = block.Y
                Dim vinv(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
                Dim pMatrix(,) As Double = vinv
                If request.FitMethod = MixedModelFitMethod.REML Then
                    Dim vinvX(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, x)
                    pMatrix = BuildRemlProjectionTraceMatrix(vinv, vinvX, profile.VarBeta)
                End If

                Dim residual() As Double = BuildResidualVector(y, x, profile.Beta)
                Dim a() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, residual)

                Dim derivatives As New List(Of Double(,))()
                Dim derivativeMessage As String = Nothing
                If gCount > 0 Then
                    Dim gDerivs As Double(,,) = Nothing
                    If Not MixedModelCovarianceDerivatives.TryBuildGDerivatives(activeG, thetaG, block, data, gDerivs, derivativeMessage) Then
                        result.Message = If(String.IsNullOrWhiteSpace(derivativeMessage), "G-side covariance derivative provider failed while building Average Information matrix.", derivativeMessage)
                        Return False
                    End If
                    For h As Integer = 0 To gCount - 1
                        derivatives.Add(SliceDerivative(gDerivs, h))
                    Next
                End If

                Dim rDerivs As Double(,,) = Nothing
                If Not MixedModelCovarianceDerivatives.TryBuildRDerivatives(request.ResidualStruct, thetaR, block, data, rDerivs, derivativeMessage) Then
                    result.Message = If(String.IsNullOrWhiteSpace(derivativeMessage), "R-side covariance derivative provider failed while building Average Information matrix.", derivativeMessage)
                    Return False
                End If
                For h As Integer = 0 To rCount - 1
                    derivatives.Add(SliceDerivative(rDerivs, h))
                Next

                Dim qVectors As New List(Of Double())()
                For h As Integer = 0 To paramCount - 1
                    Dim dVa() As Double = MatrixVectorProduct(derivatives(h), a)
                    qVectors.Add(MatrixVectorProduct(pMatrix, dVa))
                Next

                For h As Integer = 0 To paramCount - 1
                    Dim dVhA() As Double = MatrixVectorProduct(derivatives(h), a)
                    For j As Integer = 0 To h
                        Dim value As Double = Matrix.DotProduct(dVhA, qVectors(j))
                        If request.FitMethod = MixedModelFitMethod.REML Then
                            ' The criterion minimized by the engine is -2 log L, so the AI matrix is
                            ' criterion-scaled.  This is the 2*AI(logLik) equivalent of the standard
                            ' 0.5 y'P dV_h P dV_j P y expression.
                            info(h, j) += value
                        Else
                            info(h, j) += value
                        End If
                        If h <> j Then info(j, h) = info(h, j)
                    Next
                Next
            Next

            result.Success = True
            result.Message = "OK"
            result.Criterion = profile.Criterion
            result.Gradient = gradientEval.Gradient
            result.InformationMatrix = info
            Return True
        End Function

        Private Function TrySolveInformationStep(info(,) As Double,
                                                 gradient() As Double,
                                                 ByRef stepDirection() As Double,
                                                 ByRef diagnostics As MixedModelAverageInformationDiagnostics,
                                                 ByRef strTrace As String) As Boolean
            stepDirection = Nothing
            If info Is Nothing OrElse gradient Is Nothing Then Return False
            Dim n As Integer = gradient.Length
            If n = 0 Then
                stepDirection = Array.Empty(Of Double)()
                Return True
            End If

            Dim rhs(n - 1) As Double
            For i As Integer = 0 To n - 1
                rhs(i) = -gradient(i)
            Next

            Dim baseScale As Double = 0.0
            For i As Integer = 0 To n - 1
                baseScale = Math.Max(baseScale, Math.Abs(info(i, i)))
            Next
            baseScale = Math.Max(1.0, baseScale)

            For attempt As Integer = 0 To DefaultMaxRidgeAttempts
                Dim ridge As Double = If(attempt = 0, 0.0, DefaultInitialRidge * baseScale * Math.Pow(10.0, attempt - 1))
                Dim stabilized(,) As Double = CType(info.Clone(), Double(,))
                If ridge > 0.0 Then
                    diagnostics.RidgeAdjustmentCount += 1
                    For i As Integer = 0 To n - 1
                        stabilized(i, i) += ridge
                    Next
                End If

                Dim chol(,) As Double = Nothing
                Dim tmpTrace As String = Nothing
                If MixedModelCovariance.TryCholesky(stabilized, chol, tmpTrace) Then
                    Dim solved() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, rhs)
                    If Matrix.VectorIsFinite(solved) Then
                        stepDirection = solved
                        Return True
                    End If
                End If
            Next

            Return False
        End Function

        Private Function IsSupportedRequest(request As MixedModelFitRequest, activeG As MixedModelGStruct, ByRef message As String) As Boolean
            message = Nothing
            If request Is Nothing Then
                message = "Mixed-model request is missing."
                Return False
            End If
            If request.Data Is Nothing Then
                message = "Mixed-model data is missing."
                Return False
            End If
            If request.ResidualStruct Is Nothing Then
                message = "Residual covariance structure is missing."
                Return False
            End If
            If request.FitMethod <> MixedModelFitMethod.REML Then
                message = "Average Information optimizer is currently available only for REML fits."
                Return False
            End If

            Dim supportMessage As String = Nothing
            Dim dummy As MixedModelAnalyticGradientEvaluation = Nothing
            Dim objective As Func(Of Double(), MixedModelProfileEvaluation) = Function(theta() As Double) New MixedModelProfileEvaluation With {.Success = False, .Message = "support check"}
            Dim gradient As Func(Of Double(), Double()) = Nothing
            If Not MixedModelAnalyticGradient.TryCreateGradientDelegate(request, activeG, objective,
                                                                        Function() False,
                                                                        gradient, supportMessage) Then
                message = supportMessage
                Return False
            End If

            message = "OK"
            Return True
        End Function

        Private Function BuildRemlProjectionTraceMatrix(vinv(,) As Double, vinvX(,) As Double, c(,) As Double) As Double(,)
            Dim n As Integer = vinv.GetLength(0)
            Dim p As Integer = c.GetLength(0)
            Dim out(,) As Double = CType(vinv.Clone(), Double(,))

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim adjustment As Double = 0.0
                    For a As Integer = 0 To p - 1
                        Dim left As Double = vinvX(i, a)
                        If left = 0.0 Then Continue For
                        For b As Integer = 0 To p - 1
                            adjustment += left * c(a, b) * vinvX(j, b)
                        Next
                    Next
                    out(i, j) -= adjustment
                Next
            Next

            Return out
        End Function

        Private Function BuildResidualVector(y() As Double, x(,) As Double, beta() As Double) As Double()
            Dim n As Integer = y.Length
            Dim p As Integer = beta.Length
            Dim out(n - 1) As Double
            For i As Integer = 0 To n - 1
                Dim fitted As Double = 0.0
                For j As Integer = 0 To p - 1
                    fitted += x(i, j) * beta(j)
                Next
                out(i) = y(i) - fitted
            Next
            Return out
        End Function

        Private Function SliceDerivative(derivatives As Double(,,), parameterIndex As Integer) As Double(,)
            Dim n As Integer = derivatives.GetLength(1)
            Dim out(n - 1, n - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    out(i, j) = derivatives(parameterIndex, i, j)
                Next
            Next
            Return out
        End Function

        Private Function SliceVector(values() As Double, startIndex As Integer, length As Integer) As Double()
            If length <= 0 Then Return Array.Empty(Of Double)()
            Dim out(length - 1) As Double
            Array.Copy(values, startIndex, out, 0, length)
            Return out
        End Function

        Private Sub ThrowIfCancellationRequested(cancellationRequested As Func(Of Boolean))
            If cancellationRequested Is Nothing Then Exit Sub
            If cancellationRequested.Invoke() Then Throw New OperationCanceledException("Mixed-model Average Information optimization cancelled by user.")
        End Sub

        Private Function IsRequested(flag As Func(Of Boolean)) As Boolean
            If flag Is Nothing Then Return False
            Return flag.Invoke()
        End Function

        Private Function CreateInitialState(startTheta() As Double) As MixedModelOptimizationState
            Return New MixedModelOptimizationState With {
                .Theta = If(startTheta Is Nothing, Nothing, CloneVector(startTheta)),
                .Objective = Double.NaN,
                .Iterations = 0,
                .Converged = False,
                .GradNorm = Double.NaN,
                .StepNorm = Double.NaN,
                .FunctionChange = Double.NaN,
                .StepSize = Double.NaN,
                .Status = MixedModelOptimizationStatus.NotStarted,
                .Message = String.Empty,
                .GradientProviderName = OptimizerName
            }
        End Function

        Private Function FailState(state As MixedModelOptimizationState,
                                   status As MixedModelOptimizationStatus,
                                   message As String,
                                   ByRef strTrace As String) As MixedModelOptimizationState
            state.Status = status
            state.Message = If(message, String.Empty)
            state.Converged = False
            state.strTrace = strTrace
            Return state
        End Function

        Private Sub MarkInterruptedState(ByRef state As MixedModelOptimizationState,
                                         theta() As Double,
                                         objective As Double,
                                         iterations As Integer,
                                         ByRef strTrace As String)
            state.Theta = CloneVector(theta)
            state.Objective = objective
            state.Iterations = Math.Max(0, iterations)
            state.Converged = False
            state.Status = MixedModelOptimizationStatus.Interrupted
            state.Message = "Average Information optimization interrupted by user request; returning the last accepted covariance-parameter iterate."
            state.strTrace = strTrace
        End Sub

        Private Function BuildTraceMatrix(rows As List(Of Double())) As Double(,)
            If rows Is Nothing OrElse rows.Count = 0 Then Return Nothing
            Dim out(rows.Count - 1, 5) As Double
            For i As Integer = 0 To rows.Count - 1
                For j As Integer = 0 To 5
                    out(i, j) = rows(i)(j)
                Next
            Next
            Return out
        End Function

    End Module

End Namespace