Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Exit/status code returned by the generic mixed-model covariance-parameter optimizer.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The first LMM/MMRM engine optimizes covariance parameters while profiling out the fixed effects.
    ''' The optimizer therefore needs to report more than a simple True/False flag: during mixed-model
    ''' development it is important to distinguish ordinary convergence from line-search failure, non-finite
    ''' objectives, and iteration limits.
    ''' </para>
    ''' </remarks>
    Public Enum MixedModelOptimizationStatus
        ''' <summary>No optimization has been attempted.</summary>
        NotStarted = 0

        ''' <summary>The projected gradient norm is below the requested tolerance.</summary>
        ConvergedGradient = 1

        ''' <summary>The absolute change in the objective is below the requested tolerance.</summary>
        ConvergedFunction = 2

        ''' <summary>The accepted parameter step is below the requested tolerance.</summary>
        ConvergedStep = 3

        ''' <summary>The maximum number of iterations was reached before convergence.</summary>
        IterationLimit = 4

        ''' <summary>The starting point or a trial point produced a non-finite objective value.</summary>
        NonFiniteObjective = 5

        ''' <summary>The backtracking line search could not find an improving finite objective.</summary>
        LineSearchFailed = 6

        ''' <summary>The caller supplied inconsistent inputs.</summary>
        InvalidInput = 7

        ''' <summary>The caller requested cooperative cancellation.</summary>
        Cancelled = 8

        ''' <summary>The caller requested interruption and wants the latest accepted iterate returned.</summary>
        Interrupted = 9
    End Enum

    ''' <summary>
    ''' Result object returned by <see cref="MixedModelOptimizer.OptimizeProjected"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The object stores the final internal-scale covariance parameter vector, the profiled objective value,
    ''' convergence diagnostics, and an optional iteration trace table.  The trace table is intentionally simple
    ''' and numeric so that it can later be wrapped into a <c>ResultTable</c>, returned from UDFs, or printed into
    ''' a diagnostics sheet.
    ''' </para>
    ''' <para>
    ''' Trace-table columns are:
    ''' </para>
    ''' <list type="number">
    ''' <item><description>iteration index,</description></item>
    ''' <item><description>objective value at the start of the iteration,</description></item>
    ''' <item><description>projected gradient norm,</description></item>
    ''' <item><description>accepted step norm,</description></item>
    ''' <item><description>accepted line-search step length,</description></item>
    ''' <item><description>objective value after the accepted step.</description></item>
    ''' </list>
    ''' </remarks>
    Public Structure MixedModelOptimizationState
        Public Theta() As Double
        Public Objective As Double
        Public Iterations As Integer
        Public Converged As Boolean
        Public GradNorm As Double
        Public StepNorm As Double
        Public FunctionChange As Double
        Public StepSize As Double
        Public Status As MixedModelOptimizationStatus
        Public Message As String
        Public TraceTable(,) As Double
        Public ObjectiveEvaluationCount As Integer
        Public GradientEvaluationCount As Integer
        Public NumericalGradientObjectiveEvaluationCount As Integer
        Public LineSearchEvaluationCount As Integer
        Public BfgsResetCount As Integer
        Public GradientProviderName As String
        Public strTrace As String
    End Structure

    ''' <summary>
    ''' Generic numerical optimizer used by the Gaussian mixed-model engine.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The first LMM/MMRM implementation should keep covariance optimization deliberately conservative and
    ''' inspectable.  This module therefore provides a small projected-gradient optimizer with central-difference
    ''' numerical gradients, backtracking line search, optional box constraints, and explicit handling of
    ''' non-finite likelihood values.
    ''' </para>
    ''' <para>
    ''' In the mixed-model engine the objective supplied to this optimizer will usually be the profiled ML/REML
    ''' criterion as a function of the internal covariance parameter vector <c>theta</c>.  Fixed effects are
    ''' profiled out inside the objective evaluator; this optimizer only sees the covariance parameters.
    ''' </para>
    ''' <para>
    ''' Implementation decisions:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Central differences are used for the default gradient because they are slower but more stable during early engine validation.</description></item>
    ''' <item><description>Line search requires a finite objective and a strict improvement before accepting a trial point.</description></item>
    ''' <item><description>Optional bounds are supported for future parameters that remain explicitly constrained.  Most covariance structures should instead use internal transformations such as log standard deviations and Fisher/tanh correlations.</description></item>
    ''' <item><description>All diagnostics are written both to <see cref="CoreServices.logger"/> and to an optional in-memory trace accumulator so the future UI can expose the same messages to users.</description></item>
    ''' </list>
    ''' </remarks>
    Public Module MixedModelOptimizer

        Private Const DefaultGradientRelativeStep As Double = 0.000001
        Private Const DefaultOptimizerGradientTolerance As Double = 0.00000001
        Private Const DefaultMaxLineSearchSteps As Integer = 30
        Private Const DefaultLineSearchShrink As Double = 0.5
        Private Const DefaultInitialStepSize As Double = 1.0
        Private Const DefaultMinStepSize As Double = 0.000000000001
        Private Const DefaultArmijoC As Double = 0.0001
        Private Const DefaultPenaltyObjective As Double = 1.0E+100

        ''' <summary>
        ''' Computes a central-difference numerical gradient for a scalar objective.
        ''' </summary>
        ''' <param name="objective">Objective function to evaluate.  It is assumed to be minimized.</param>
        ''' <param name="point">Point at which the gradient is evaluated.</param>
        ''' <param name="epsilon">Relative finite-difference step.  If non-positive, a conservative default is used.</param>
        ''' <param name="lower">Optional lower bounds.  If supplied, one value per parameter is required.</param>
        ''' <param name="upper">Optional upper bounds.  If supplied, one value per parameter is required.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>Central-difference gradient vector.</returns>
        ''' <remarks>
        ''' <para>
        ''' For parameter <c>theta_j</c>, the method evaluates
        ''' </para>
        ''' <para><c>[f(theta + h e_j) - f(theta - h e_j)] / (2 h)</c></para>
        ''' <para>
        ''' with <c>h = epsilon * max(1, abs(theta_j))</c>.  If a requested perturbation would cross a box bound,
        ''' the method falls back to a one-sided difference in the feasible direction.
        ''' </para>
        ''' <para>
        ''' Non-finite objective values are converted to a large penalty.  This is useful when the mixed-model
        ''' covariance evaluator rejects non-SPD proposals during optimization.
        ''' </para>
        ''' </remarks>
        Public Function NumericalGradient(objective As Func(Of Double(), Double),
                                          point() As Double,
                                          Optional epsilon As Double = DefaultGradientRelativeStep,
                                          Optional lower() As Double = Nothing,
                                          Optional upper() As Double = Nothing,
                                          Optional ByRef strTrace As String = Nothing,
                                          Optional cancellationRequested As Func(Of Boolean) = Nothing) As Double()
            ValidateObjective(objective)
            ValidateTheta(point, "point")
            ValidateBounds(point.Length, lower, upper)

            If epsilon <= 0.0 OrElse Double.IsNaN(epsilon) OrElse Double.IsInfinity(epsilon) Then
                LogWarn("MixedModelOptimizer.NumericalGradient received non-positive/invalid epsilon; default will be used.", strTrace)
                epsilon = DefaultGradientRelativeStep
            End If

            If point.Length = 0 Then
                LogTrace("MixedModelOptimizer.NumericalGradient completed for zero-length parameter vector.", strTrace)
                Return Array.Empty(Of Double)()
            End If

            Dim g(point.Length - 1) As Double

            For j As Integer = 0 To point.Length - 1
                ThrowIfCancellationRequested(cancellationRequested)

                Dim h As Double = epsilon * Math.Max(1.0, Math.Abs(point(j)))
                If h <= 0.0 Then h = epsilon

                Dim xPlus() As Double = CloneVector(point)
                Dim xMinus() As Double = CloneVector(point)
                Dim canPlus As Boolean = True
                Dim canMinus As Boolean = True

                xPlus(j) += h
                xMinus(j) -= h

                If lower IsNot Nothing AndAlso xMinus(j) < lower(j) Then canMinus = False
                If upper IsNot Nothing AndAlso xPlus(j) > upper(j) Then canPlus = False

                If canPlus AndAlso canMinus Then
                    Dim fPlus As Double = SafeObjective(objective, xPlus, strTrace)
                    Dim fMinus As Double = SafeObjective(objective, xMinus, strTrace)
                    g(j) = (fPlus - fMinus) / (2.0 * h)
                ElseIf canPlus Then
                    Dim f0 As Double = SafeObjective(objective, point, strTrace)
                    Dim fPlus As Double = SafeObjective(objective, xPlus, strTrace)
                    g(j) = (fPlus - f0) / h
                ElseIf canMinus Then
                    Dim f0 As Double = SafeObjective(objective, point, strTrace)
                    Dim fMinus As Double = SafeObjective(objective, xMinus, strTrace)
                    g(j) = (f0 - fMinus) / h
                Else
                    g(j) = 0.0
                    LogWarn($"MixedModelOptimizer.NumericalGradient parameter {j} cannot be perturbed within bounds; gradient set to zero.", strTrace)
                End If

                If Not IsFinite(g(j)) Then
                    LogWarn($"MixedModelOptimizer.NumericalGradient non-finite gradient at parameter {j}; gradient set to zero.", strTrace)
                    g(j) = 0.0
                End If
            Next

            LogTrace($"MixedModelOptimizer.NumericalGradient completed. dim={point.Length}; gradNorm={Matrix.VectorNorm(g)}", strTrace)
            Return g
        End Function

        ''' <summary>
        ''' Minimizes a scalar objective with a simple projected-gradient/backtracking method.
        ''' </summary>
        ''' <param name="startTheta">Starting parameter vector on the internal optimizer scale.</param>
        ''' <param name="objective">Objective function to minimize.</param>
        ''' <param name="gradient">Optional analytic/numeric gradient function. If omitted, <see cref="NumericalGradient"/> is used.</param>
        ''' <param name="maxIterations">Maximum number of outer iterations.</param>
        ''' <param name="epsilon">Gradient-norm convergence tolerance. Numerical gradients use a separate conservative relative step.</param>
        ''' <param name="stepTolerance">Convergence tolerance for accepted parameter-step norm.</param>
        ''' <param name="functionTolerance">Convergence tolerance for objective change.</param>
        ''' <param name="lower">Optional lower bounds.</param>
        ''' <param name="upper">Optional upper bounds.</param>
        ''' <param name="initialStepSize">Initial line-search step length for each iteration.</param>
        ''' <param name="maxLineSearchSteps">Maximum number of backtracking attempts per iteration.</param>
        ''' <param name="storeTraceTable">If True, stores a numeric iteration trace table in the returned state.</param>
        ''' <param name="strTrace">Optional in-memory trace accumulator.</param>
        ''' <returns>Final optimization state.</returns>
        ''' <remarks>
        ''' <para>
        ''' When an explicit gradient delegate is not supplied, the numerical-gradient finite-difference step is
        ''' intentionally decoupled from <paramref name="epsilon"/>.  This lets callers keep a tight gradient-norm
        ''' convergence tolerance without forcing cancellation-prone objective differences at the same tiny scale.
        ''' </para>
        ''' <para>
        ''' The routine searches along the negative gradient direction and projects each trial point into the
        ''' supplied box constraints.  A trial point is accepted if it produces a finite objective and improves the
        ''' current objective.  An Armijo-like sufficient decrease is attempted, but a strict finite improvement is
        ''' accepted as a fallback because early mixed-model likelihood surfaces can be noisy when the objective is
        ''' evaluated through Cholesky factorizations and finite-difference derivatives.
        ''' </para>
        ''' <para>
        ''' This is not intended to be the final high-performance optimizer for all mixed models.  It is intended
        ''' as a transparent first optimizer that makes failures easy to debug while the LMM/MMRM likelihood code,
        ''' parameterizations, and validation tests are being developed.
        ''' </para>
        ''' </remarks>
        Public Function OptimizeProjected(startTheta() As Double,
                                          objective As Func(Of Double(), Double),
                                          Optional gradient As Func(Of Double(), Double()) = Nothing,
                                          Optional maxIterations As Integer = 100,
                                          Optional epsilon As Double = DefaultOptimizerGradientTolerance,
                                          Optional stepTolerance As Double = 0.0000001,
                                          Optional functionTolerance As Double = 0.000000001,
                                          Optional lower() As Double = Nothing,
                                          Optional upper() As Double = Nothing,
                                          Optional initialStepSize As Double = DefaultInitialStepSize,
                                          Optional maxLineSearchSteps As Integer = DefaultMaxLineSearchSteps,
                                          Optional storeTraceTable As Boolean = True,
                                          Optional ByRef strTrace As String = Nothing,
                                          Optional iterationCallback As Action(Of MixedModelOptimizationState) = Nothing,
                                          Optional cancellationRequested As Func(Of Boolean) = Nothing,
                                          Optional interruptionRequested As Func(Of Boolean) = Nothing,
                                          Optional useBfgsDirection As Boolean = True) As MixedModelOptimizationState

            Dim state As MixedModelOptimizationState = CreateEmptyState()

            Try
                ThrowIfCancellationRequested(cancellationRequested)
                ValidateObjective(objective)
                ValidateTheta(startTheta, "startTheta")
                ValidateBounds(startTheta.Length, lower, upper)

                If maxIterations < 1 Then maxIterations = 1
                If epsilon <= 0.0 OrElse Not IsFinite(epsilon) Then epsilon = DefaultOptimizerGradientTolerance
                Dim gradientStep As Double = OptimizerGradientFiniteDifferenceStep(epsilon)
                If stepTolerance <= 0.0 OrElse Not IsFinite(stepTolerance) Then stepTolerance = 0.0000001
                If functionTolerance <= 0.0 OrElse Not IsFinite(functionTolerance) Then functionTolerance = 0.000000001
                If initialStepSize <= 0.0 OrElse Not IsFinite(initialStepSize) Then initialStepSize = DefaultInitialStepSize
                If maxLineSearchSteps < 1 Then maxLineSearchSteps = DefaultMaxLineSearchSteps

                Dim theta() As Double = ProjectToBounds(CloneVector(startTheta), lower, upper)
                state.GradientProviderName = If(gradient Is Nothing, "Numerical finite difference", "Caller-supplied gradient")

                Dim objectiveCounter As New MixedModelObjectiveEvaluationCounter(objective, strTrace)
                Dim countedObjective As Func(Of Double(), Double) = AddressOf objectiveCounter.Evaluate

                Dim f As Double = countedObjective(theta)
                CopyObjectiveCounterToState(objectiveCounter, state)
                strTrace = objectiveCounter.Trace

                state.Theta = CloneVector(theta)
                state.Objective = f
                state.Status = MixedModelOptimizationStatus.NotStarted
                state.Message = "Optimization initialized."

                If IsInterruptionRequested(interruptionRequested) Then
                    MarkInterruptedState(state, theta, f, 0, strTrace)
                    Return state
                End If

                If Not IsFiniteUsableObjective(f) Then
                    state.Status = MixedModelOptimizationStatus.NonFiniteObjective
                    state.Message = "Starting objective is non-finite or penalized. Check starting covariance parameters."
                    state.Converged = False
                    state.strTrace = strTrace
                    LogWarn("MixedModelOptimizer.OptimizeProjected failed at start: non-finite objective.", strTrace)
                    Return state
                End If

                If theta.Length = 0 Then
                    state.Theta = Array.Empty(Of Double)()
                    state.Objective = f
                    state.Iterations = 0
                    state.Converged = True
                    state.GradNorm = 0.0
                    state.StepNorm = 0.0
                    state.FunctionChange = 0.0
                    state.StepSize = 0.0
                    state.Status = MixedModelOptimizationStatus.ConvergedGradient
                    state.Message = "No covariance parameters to optimize; objective evaluated once."
                    state.strTrace = strTrace
                    LogTrace("MixedModelOptimizer.OptimizeProjected completed for zero-length parameter vector.", strTrace)
                    If iterationCallback IsNot Nothing Then iterationCallback(state)
                    Return state
                End If

                Dim traceRows As New List(Of Double())()
                Dim inverseHessianApprox(,) As Double = Matrix.IdentityMat(theta.Length - 1)
                Dim previousThetaForBfgs() As Double = Nothing
                Dim previousGradientForBfgs() As Double = Nothing
                Dim bfgsEnabled As Boolean = useBfgsDirection AndAlso theta.Length > 0
                LogDebug($"MixedModelOptimizer.OptimizeProjected start. dim={theta.Length}; f0={f}; maxIter={maxIterations}; gradTol={epsilon}; gradStep={gradientStep}; bfgs={bfgsEnabled}", strTrace)

                For iter As Integer = 1 To maxIterations
                    ThrowIfCancellationRequested(cancellationRequested)
                    If IsInterruptionRequested(interruptionRequested) Then
                        MarkInterruptedState(state, theta, f, iter - 1, strTrace)
                        Return state
                    End If

                    Dim g() As Double
                    state.GradientEvaluationCount += 1
                    If gradient Is Nothing Then
                        objectiveCounter.Context = "NumericalGradient"
                        Dim gradientTrace As String = objectiveCounter.Trace
                        Try
                            g = NumericalGradient(countedObjective, theta, gradientStep, lower, upper, gradientTrace, cancellationRequested)
                        Finally
                            objectiveCounter.Context = String.Empty
                            objectiveCounter.Trace = gradientTrace
                            CopyObjectiveCounterToState(objectiveCounter, state)
                            strTrace = objectiveCounter.Trace
                        End Try
                    Else
                        g = gradient(theta)
                        ValidateGradient(g, theta.Length)
                    End If

                    Dim pg() As Double = ProjectedGradient(theta, g, lower, upper)

                    If bfgsEnabled AndAlso previousThetaForBfgs IsNot Nothing AndAlso previousGradientForBfgs IsNot Nothing Then
                        Dim sStep() As Double = Matrix.M_SUB(theta, previousThetaForBfgs)
                        Dim yStep() As Double = Matrix.M_SUB(g, previousGradientForBfgs)
                        If Not TryUpdateInverseBfgs(inverseHessianApprox, sStep, yStep, strTrace) Then
                            inverseHessianApprox = Matrix.IdentityMat(theta.Length - 1)
                            state.BfgsResetCount += 1
                        End If
                    End If

                    Dim gradNorm As Double = Matrix.VectorNorm(pg)
                    state.GradNorm = gradNorm
                    state.Iterations = iter - 1

                    If gradNorm <= epsilon Then
                        state.Status = MixedModelOptimizationStatus.ConvergedGradient
                        state.Message = $"Converged: projected gradient norm {gradNorm} <= tolerance {epsilon}."
                        state.Converged = True
                        Exit For
                    End If

                    Dim steepestDirection() As Double = Matrix.NegativeVector(pg)
                    Dim directionCandidates As New List(Of Double())()
                    Dim usedBfgsCandidate As Boolean = False

                    If bfgsEnabled Then
                        Dim bfgsDirection() As Double = BfgsSearchDirection(inverseHessianApprox, pg)
                        If bfgsDirection IsNot Nothing AndAlso bfgsDirection.Length = theta.Length Then
                            directionCandidates.Add(bfgsDirection)
                            usedBfgsCandidate = True
                        End If
                    End If

                    directionCandidates.Add(steepestDirection)

                    Dim accepted As Boolean = False
                    Dim alpha As Double = initialStepSize
                    Dim bestTheta() As Double = CloneVector(theta)
                    Dim bestF As Double = f
                    Dim stepNorm As Double = 0.0

                    For directionIndex As Integer = 0 To directionCandidates.Count - 1
                        Dim direction() As Double = directionCandidates(directionIndex)
                        Dim slope As Double = Matrix.DotProduct(g, direction)

                        If slope >= 0.0 OrElse Not IsFinite(slope) Then
                            If directionIndex = 0 AndAlso usedBfgsCandidate Then
                                LogTrace($"MixedModelOptimizer BFGS direction was not descending at iteration {iter}; falling back to steepest descent.", strTrace)
                            End If
                            Continue For
                        End If

                        alpha = initialStepSize
                        bestTheta = CloneVector(theta)
                        bestF = f
                        stepNorm = 0.0

                        For ls As Integer = 1 To maxLineSearchSteps
                            ThrowIfCancellationRequested(cancellationRequested)
                            If IsInterruptionRequested(interruptionRequested) Then
                                MarkInterruptedState(state, theta, f, iter - 1, strTrace)
                                Return state
                            End If

                            Dim trial() As Double = AddScaled(theta, direction, alpha)
                            trial = ProjectToBounds(trial, lower, upper)
                            stepNorm = Distance(theta, trial)

                            If stepNorm <= stepTolerance Then
                                LogTrace($"MixedModelOptimizer line search iter={iter}, ls={ls}: projected step below tolerance ({stepNorm}).", strTrace)
                                Exit For
                            End If

                            Dim fTrial As Double
                            objectiveCounter.Context = "LineSearch"
                            Try
                                fTrial = countedObjective(trial)
                            Finally
                                objectiveCounter.Context = String.Empty
                                CopyObjectiveCounterToState(objectiveCounter, state)
                                strTrace = objectiveCounter.Trace
                            End Try

                            If IsFiniteUsableObjective(fTrial) Then
                                Dim armijoTarget As Double = f + DefaultArmijoC * alpha * slope
                                If fTrial <= armijoTarget OrElse fTrial < f Then
                                    accepted = True
                                    bestTheta = trial
                                    bestF = fTrial
                                    Exit For
                                End If
                            End If

                            alpha *= DefaultLineSearchShrink
                            If alpha < DefaultMinStepSize Then Exit For
                        Next

                        If accepted Then Exit For
                        If directionIndex = 0 AndAlso usedBfgsCandidate Then
                            inverseHessianApprox = Matrix.IdentityMat(theta.Length - 1)
                            state.BfgsResetCount += 1
                            LogTrace($"MixedModelOptimizer BFGS line search failed at iteration {iter}; retrying steepest descent fallback and resetting BFGS memory.", strTrace)
                        End If

                    Next

                    If Not accepted Then
                        state.Status = MixedModelOptimizationStatus.LineSearchFailed
                        state.Message = "Line search failed to find a finite improving objective."
                        state.Converged = False
                        state.StepSize = alpha
                        state.StepNorm = stepNorm
                        state.Iterations = iter - 1
                        If iterationCallback IsNot Nothing Then iterationCallback(state)
                        LogWarn($"MixedModelOptimizer.OptimizeProjected line search failed at iteration {iter}. f={f}; gradNorm={gradNorm}", strTrace)
                        Exit For
                    End If

                    Dim fOld As Double = f
                    Dim thetaOld() As Double = theta
                    Dim gradientOld() As Double = CloneVector(g)
                    theta = bestTheta
                    f = bestF

                    stepNorm = Distance(thetaOld, theta)
                    Dim fChange As Double = Math.Abs(fOld - f)

                    state.Theta = CloneVector(theta)
                    state.Objective = f
                    state.StepNorm = stepNorm
                    state.FunctionChange = fChange
                    state.StepSize = alpha
                    state.Iterations = iter
                    If iterationCallback IsNot Nothing Then iterationCallback(state)

                    If storeTraceTable Then
                        traceRows.Add(New Double() {CDbl(iter), fOld, gradNorm, stepNorm, alpha, f})
                    End If

                    If iter = 1 OrElse iter Mod 5 = 0 Then
                        LogTrace($"MixedModelOptimizer iter={iter}; f={f}; gradNorm={gradNorm}; stepNorm={stepNorm}; alpha={alpha}", strTrace)
                    End If

                    previousThetaForBfgs = CloneVector(thetaOld)
                    previousGradientForBfgs = gradientOld

                    If stepNorm <= stepTolerance Then
                        state.Status = MixedModelOptimizationStatus.ConvergedStep
                        state.Message = $"Converged: accepted step norm {stepNorm} <= tolerance {stepTolerance}."
                        state.Converged = True
                        Exit For
                    End If

                    If fChange <= functionTolerance Then
                        state.Status = MixedModelOptimizationStatus.ConvergedFunction
                        state.Message = $"Converged: objective change {fChange} <= tolerance {functionTolerance}."
                        state.Converged = True
                        Exit For
                    End If

                    If iter = maxIterations Then
                        state.Status = MixedModelOptimizationStatus.IterationLimit
                        state.Message = "Maximum iterations reached before convergence."
                        state.Converged = False
                    End If
                Next

                If storeTraceTable Then state.TraceTable = BuildTraceMatrix(traceRows)
                state.strTrace = strTrace
                LogDebug($"MixedModelOptimizer.OptimizeProjected completed. status={state.Status}; converged={state.Converged}; iter={state.Iterations}; f={state.Objective}; gradNorm={state.GradNorm}; objectiveEvals={state.ObjectiveEvaluationCount}; gradientEvals={state.GradientEvaluationCount}; lineSearchEvals={state.LineSearchEvaluationCount}; gradientProvider={state.GradientProviderName}", strTrace)
                Return state

            Catch ex As OperationCanceledException
                state.Status = MixedModelOptimizationStatus.Cancelled
                state.Message = If(String.IsNullOrWhiteSpace(ex.Message), "Optimization cancelled by user.", ex.Message)
                state.Converged = False
                state.strTrace = strTrace
                LogWarn("MixedModelOptimizer.OptimizeProjected cancelled by caller.", strTrace)
                Return state

            Catch ex As Exception
                state.Status = MixedModelOptimizationStatus.InvalidInput
                state.Message = ex.Message
                state.Converged = False
                state.strTrace = strTrace
                CoreServices.Logger.Error(ex, "MixedModelOptimizer.OptimizeProjected failed.")
                Return state
            End Try
        End Function

        ''' <summary>
        ''' Convenience wrapper that reads basic optimizer tolerances from <see cref="MixedModelControl"/>.
        ''' </summary>
        Public Function OptimizeProjected(startTheta() As Double,
                                  objective As Func(Of Double(), Double),
                                  control As MixedModelControl,
                                  Optional gradient As Func(Of Double(), Double()) = Nothing,
                                  Optional lower() As Double = Nothing,
                                  Optional upper() As Double = Nothing,
                                  Optional ByRef strTrace As String = Nothing,
                                  Optional iterationCallback As Action(Of MixedModelOptimizationState) = Nothing,
                                  Optional cancellationRequested As Func(Of Boolean) = Nothing,
                                  Optional interruptionRequested As Func(Of Boolean) = Nothing) As MixedModelOptimizationState
            Return OptimizeProjected(startTheta,
                             objective,
                             gradient,
                             control.MaxIter,
                             control.Epsilon,
                             control.StepTolerance,
                             control.FunctionTolerance,
                             lower,
                             upper,
                             DefaultInitialStepSize,
                             DefaultMaxLineSearchSteps,
                             control.Trace,
                             strTrace,
                             iterationCallback,
                             cancellationRequested,
                             interruptionRequested,
                             control.UseBfgsCovarianceOptimization)
        End Function

        ''' <summary>
        ''' Returns the relative finite-difference step used for optimizer gradients.
        ''' </summary>
        ''' <remarks>
        ''' Central-difference gradients of the profiled mixed-model criterion are objective-difference calculations.
        ''' A step as small as the convergence tolerance, for example 1E-8, can amplify cancellation noise and stop the
        ''' projected-gradient optimizer slightly away from the likelihood minimum.  Using at least the conservative
        ''' default step preserves the existing number of objective evaluations while improving the stability of the
        ''' search direction.
        ''' </remarks>
        Private Function OptimizerGradientFiniteDifferenceStep(gradientTolerance As Double) As Double
            If gradientTolerance <= 0.0 OrElse Not IsFinite(gradientTolerance) Then Return DefaultGradientRelativeStep
            Return Math.Max(DefaultGradientRelativeStep, gradientTolerance)
        End Function

        Friend Function MatrixVectorProduct(a(,) As Double, x() As Double) As Double()
            If a Is Nothing OrElse x Is Nothing Then Return Nothing
            Dim n As Integer = x.Length
            If a.GetLength(0) <> n OrElse a.GetLength(1) <> n Then Return Nothing
            Dim out(n - 1) As Double
            For r As Integer = 0 To n - 1
                Dim sum As Double = 0.0
                For c As Integer = 0 To n - 1
                    sum += a(r, c) * x(c)
                Next
                out(r) = sum
            Next
            Return out
        End Function

        Private Function BfgsSearchDirection(inverseHessianApprox(,) As Double, gradient() As Double) As Double()
            Dim hg() As Double = MatrixVectorProduct(inverseHessianApprox, gradient)
            If hg Is Nothing Then Return Nothing
            Return Matrix.NegativeVector(hg)
        End Function

        Private Function TryUpdateInverseBfgs(ByRef inverseHessianApprox(,) As Double,
                                              sStep() As Double,
                                              yStep() As Double,
                                              Optional ByRef strTrace As String = Nothing) As Boolean
            If inverseHessianApprox Is Nothing OrElse sStep Is Nothing OrElse yStep Is Nothing Then Return False
            Dim n As Integer = sStep.Length
            If n = 0 OrElse yStep.Length <> n Then Return False
            If inverseHessianApprox.GetLength(0) <> n OrElse inverseHessianApprox.GetLength(1) <> n Then Return False

            Dim ys As Double = Matrix.DotProduct(yStep, sStep)
            Dim scale As Double = Math.Max(1.0, Matrix.VectorNorm(yStep) * Matrix.VectorNorm(sStep))
            If Not IsFinite(ys) OrElse ys <= 0.000000000001 * scale Then
                LogTrace("MixedModelOptimizer skipped BFGS update because curvature was non-positive or too small.", strTrace)
                Return False
            End If

            Dim hy() As Double = MatrixVectorProduct(inverseHessianApprox, yStep)
            If hy Is Nothing Then Return False
            Dim yhy As Double = Matrix.DotProduct(yStep, hy)
            If Not IsFinite(yhy) Then Return False

            Dim rho As Double = 1.0 / ys
            Dim coeff As Double = (1.0 + yhy * rho) * rho
            Dim hNew(n - 1, n - 1) As Double

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    hNew(r, c) = inverseHessianApprox(r, c) +
                                 coeff * sStep(r) * sStep(c) -
                                 rho * (sStep(r) * hy(c) + hy(r) * sStep(c))
                    If Not IsFinite(hNew(r, c)) Then Return False
                Next
            Next

            inverseHessianApprox = hNew
            Return True
        End Function

        ''' <summary>
        ''' Projects a vector into optional lower and upper bounds.
        ''' </summary>
        Public Function ProjectToBounds(theta() As Double,
                                        Optional lower() As Double = Nothing,
                                        Optional upper() As Double = Nothing) As Double()
            ValidateTheta(theta, "theta")
            ValidateBounds(theta.Length, lower, upper)

            Dim out() As Double = CloneVector(theta)
            For j As Integer = 0 To out.Length - 1
                If lower IsNot Nothing AndAlso out(j) < lower(j) Then out(j) = lower(j)
                If upper IsNot Nothing AndAlso out(j) > upper(j) Then out(j) = upper(j)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns the maximum absolute element of a vector.
        ''' </summary>
        Public Function MaxAbs(x() As Double) As Double
            If x Is Nothing OrElse x.Length = 0 Then Return 0.0
            Dim out As Double = 0.0
            For i As Integer = 0 To x.Length - 1
                out = Math.Max(out, Math.Abs(x(i)))
            Next
            Return out
        End Function

        Private Class MixedModelObjectiveEvaluationCounter
            Private ReadOnly pObjective As Func(Of Double(), Double)

            Public Sub New(objective As Func(Of Double(), Double), initialTrace As String)
                pObjective = objective
                Trace = If(initialTrace, String.Empty)
            End Sub

            Public Property Trace As String
            Public Property Context As String = String.Empty
            Public Property ObjectiveEvaluationCount As Integer = 0
            Public Property NumericalGradientObjectiveEvaluationCount As Integer = 0
            Public Property LineSearchEvaluationCount As Integer = 0

            Public Function Evaluate(theta() As Double) As Double
                ObjectiveEvaluationCount += 1
                If String.Equals(Context, "NumericalGradient", StringComparison.Ordinal) Then
                    NumericalGradientObjectiveEvaluationCount += 1
                ElseIf String.Equals(Context, "LineSearch", StringComparison.Ordinal) Then
                    LineSearchEvaluationCount += 1
                End If

                Dim currentTrace As String = Trace
                Dim value As Double = SafeObjective(pObjective, theta, currentTrace)
                Trace = currentTrace
                Return value
            End Function
        End Class

        Private Sub CopyObjectiveCounterToState(counter As MixedModelObjectiveEvaluationCounter,
                                                ByRef state As MixedModelOptimizationState)
            If counter Is Nothing Then Exit Sub

            state.ObjectiveEvaluationCount = counter.ObjectiveEvaluationCount
            state.NumericalGradientObjectiveEvaluationCount = counter.NumericalGradientObjectiveEvaluationCount
            state.LineSearchEvaluationCount = counter.LineSearchEvaluationCount
        End Sub

        Private Function CreateEmptyState() As MixedModelOptimizationState
            Dim s As New MixedModelOptimizationState
            s.Theta = Array.Empty(Of Double)()
            s.Objective = Double.NaN
            s.Iterations = 0
            s.Converged = False
            s.GradNorm = Double.NaN
            s.StepNorm = Double.NaN
            s.FunctionChange = Double.NaN
            s.StepSize = Double.NaN
            s.Status = MixedModelOptimizationStatus.NotStarted
            s.Message = String.Empty
            s.TraceTable = Nothing
            s.ObjectiveEvaluationCount = 0
            s.GradientEvaluationCount = 0
            s.NumericalGradientObjectiveEvaluationCount = 0
            s.LineSearchEvaluationCount = 0
            s.BfgsResetCount = 0
            s.GradientProviderName = String.Empty
            s.strTrace = String.Empty
            Return s
        End Function

        Private Function SafeObjective(objective As Func(Of Double(), Double),
                                       theta() As Double,
                                       Optional ByRef strTrace As String = Nothing) As Double
            Try
                Dim f As Double = objective(CloneVector(theta))
                If IsFinite(f) Then Return f
                LogTrace("MixedModelOptimizer.SafeObjective received non-finite objective; penalty returned.", strTrace)
                Return DefaultPenaltyObjective
            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                LogTrace("MixedModelOptimizer.SafeObjective caught objective exception; penalty returned. " & ex.Message, strTrace)
                Return DefaultPenaltyObjective
            End Try
        End Function

        Private Function IsInterruptionRequested(interruptionRequested As Func(Of Boolean)) As Boolean
            If interruptionRequested Is Nothing Then Return False

            Try
                Return interruptionRequested.Invoke()
            Catch
                Return False
            End Try
        End Function

        Private Sub MarkInterruptedState(ByRef state As MixedModelOptimizationState,
                                         theta() As Double,
                                         objective As Double,
                                         iterations As Integer,
                                         Optional ByRef strTrace As String = Nothing)
            state.Theta = If(theta Is Nothing, Array.Empty(Of Double)(), CloneVector(theta))
            state.Objective = objective
            state.Iterations = Math.Max(0, iterations)
            state.Converged = False
            state.Status = MixedModelOptimizationStatus.Interrupted
            state.Message = "Optimization interrupted by user; returning latest accepted parameter vector."
            state.strTrace = strTrace
            LogWarn("MixedModelOptimizer.OptimizeProjected interrupted by caller; latest accepted iterate will be returned.", strTrace)
        End Sub

        Private Sub ThrowIfCancellationRequested(cancellationRequested As Func(Of Boolean))
            If cancellationRequested Is Nothing Then Exit Sub

            Dim cancel As Boolean = False
            Try
                cancel = cancellationRequested.Invoke()
            Catch
                cancel = False
            End Try

            If cancel Then Throw New OperationCanceledException("Optimization cancelled by user.")
        End Sub

        Friend Function IsFiniteUsableObjective(x As Double) As Boolean
            Return IsFinite(x) AndAlso x < DefaultPenaltyObjective * 0.1
        End Function

        Private Sub ValidateObjective(objective As Func(Of Double(), Double))
            If objective Is Nothing Then Throw New ArgumentNullException(NameOf(objective))
        End Sub

        Private Sub ValidateTheta(theta() As Double, label As String)
            If theta Is Nothing Then Throw New ArgumentNullException(label)
            For i As Integer = 0 To theta.Length - 1
                If Double.IsNaN(theta(i)) OrElse Double.IsInfinity(theta(i)) Then
                    Throw New ArgumentException($"{label} contains a non-finite value at index {i}.")
                End If
            Next
        End Sub

        Private Sub ValidateGradient(g() As Double, expectedLength As Integer)
            If g Is Nothing Then Throw New ArgumentNullException(NameOf(g))
            If g.Length <> expectedLength Then Throw New ArgumentException($"Gradient length mismatch. Expected {expectedLength}, found {g.Length}.")
            For i As Integer = 0 To g.Length - 1
                If Not IsFinite(g(i)) Then Throw New ArgumentException($"Gradient contains a non-finite value at index {i}.")
            Next
        End Sub

        Private Sub ValidateBounds(dimSize As Integer, lower() As Double, upper() As Double)
            If lower IsNot Nothing AndAlso lower.Length <> dimSize Then Throw New ArgumentException("lower bounds length must match theta length.")
            If upper IsNot Nothing AndAlso upper.Length <> dimSize Then Throw New ArgumentException("upper bounds length must match theta length.")

            If lower IsNot Nothing Then
                For i As Integer = 0 To lower.Length - 1
                    If Not IsFinite(lower(i)) Then Throw New ArgumentException($"lower bound at index {i} is non-finite.")
                Next
            End If

            If upper IsNot Nothing Then
                For i As Integer = 0 To upper.Length - 1
                    If Not IsFinite(upper(i)) Then Throw New ArgumentException($"upper bound at index {i} is non-finite.")
                Next
            End If

            If lower IsNot Nothing AndAlso upper IsNot Nothing Then
                For i As Integer = 0 To dimSize - 1
                    If lower(i) > upper(i) Then Throw New ArgumentException($"lower bound exceeds upper bound at index {i}.")
                Next
            End If
        End Sub

        Private Function ProjectedGradient(theta() As Double,
                                           gradient() As Double,
                                           lower() As Double,
                                           upper() As Double) As Double()
            Dim pg() As Double = CloneVector(gradient)
            For j As Integer = 0 To pg.Length - 1
                If lower IsNot Nothing AndAlso Math.Abs(theta(j) - lower(j)) <= 0.000000000001 AndAlso pg(j) > 0.0 Then
                    pg(j) = 0.0
                End If
                If upper IsNot Nothing AndAlso Math.Abs(theta(j) - upper(j)) <= 0.000000000001 AndAlso pg(j) < 0.0 Then
                    pg(j) = 0.0
                End If
            Next
            Return pg
        End Function

        Friend Function AddScaled(x() As Double, direction() As Double, alpha As Double) As Double()
            Dim out(x.Length - 1) As Double
            For i As Integer = 0 To x.Length - 1
                out(i) = x(i) + alpha * direction(i)
            Next
            Return out
        End Function

        Friend Function Distance(a() As Double, b() As Double) As Double
            If a.Length <> b.Length Then Throw New ArgumentException("Distance vector length mismatch.")
            Dim ss As Double = 0.0
            For i As Integer = 0 To a.Length - 1
                Dim d As Double = a(i) - b(i)
                ss += d * d
            Next
            Return Math.Sqrt(ss)
        End Function

        Friend Function CloneVector(x() As Double) As Double()
            If x Is Nothing Then Return Nothing
            Return CType(x.Clone(), Double())
        End Function

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

        Private Sub AppendTraceLine(message As String, Optional ByRef strTrace As String = Nothing)
            If String.IsNullOrEmpty(message) Then Return
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
        End Sub

        Private Sub LogTrace(message As String, Optional ByRef strTrace As String = Nothing)
            If strTrace Is Nothing Then Return
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Trace(message)
        End Sub

        Private Sub LogDebug(message As String, Optional ByRef strTrace As String = Nothing)
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Debug(message)
        End Sub

        Private Sub LogWarn(message As String, Optional ByRef strTrace As String = Nothing)
            AppendTraceLine(message, strTrace)
            CoreServices.Logger.Warn(message)
        End Sub

    End Module

End Namespace
