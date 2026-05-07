Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Text

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

    ''' <summary>
    ''' Result container for one attempted analytic covariance-gradient evaluation.
    ''' </summary>
    Friend Structure MixedModelAnalyticGradientEvaluation
        Public Success As Boolean
        Public Message As String
        Public Criterion As Double
        Public Gradient() As Double
        Public MaxRelativeFiniteDifferenceDiscrepancy As Double
        Public FailedParameterIndex As Integer
        Public AnalyticDerivativePatternCacheEnabled As Boolean
        Public AnalyticDerivativePatternCount As Integer
        Public AnalyticDerivativePatternCacheHits As Integer
        Public AnalyticDerivativePatternCacheMisses As Integer
        Public AnalyticDerivativeMatricesBuilt As Integer
        Public AnalyticTraceQuadraticContractionTimeMs As Double
    End Structure

    ''' <summary>
    ''' One block-level covariance derivative with respect to one optimizer-scale covariance parameter.
    ''' </summary>
    Friend Structure MixedModelCovarianceDerivativeBlock
        Public ParameterIndex As Integer
        Public DV(,) As Double
    End Structure

    ''' <summary>
    ''' Analytic score evaluator for the profiled mixed-model covariance objective.
    ''' </summary>
    ''' <remarks>
    ''' AG-BFGS-P4 uses this analytic first derivative of the existing profiled ML/REML
    ''' criterion for supported R-side-only MMRM fits when the caller explicitly selects
    ''' an analytic covariance-gradient mode. AG-BFGS-P5 adds a repeated-pattern cache
    ''' inside each analytic gradient evaluation. AG-BFGS-P6 extends the same score path
    ''' to supported LMM G-side random-effects structures. AG-BFGS-P7 adds Auto mode so
    ''' validated structures use analytic gradients by default while unsupported structures
    ''' safely remain on numerical finite differences.
    ''' </remarks>
    Friend Module MixedModelAnalyticGradient

        Friend Const AnalyticProviderName As String = "Analytic score"
        Friend Const NumericalProviderName As String = "Numerical finite difference"
        Friend Const UnsupportedProviderMessage As String = "Analytic covariance gradients are available for supported R-side residual structures and supported G-side random-effects structures."

        Private Const ValidationFiniteDifferenceStep As Double = 0.000001
        Private Const PenaltyObjective As Double = 1.0E+100

        Friend Function TryCreateGradientDelegate(request As MixedModelFitRequest,
                                                  activeG As MixedModelGStruct,
                                                  objectiveEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                                  cancellationRequested As Func(Of Boolean),
                                                  ByRef gradient As Func(Of Double(), Double()),
                                                  ByRef message As String,
                                                  Optional ByRef strTrace As String = Nothing,
                                                  Optional evaluationObserver As Action(Of MixedModelAnalyticGradientEvaluation) = Nothing) As Boolean
            gradient = Nothing
            message = Nothing

            Dim supportMessage As String = Nothing
            If Not IsSupportedAnalyticGradientRequest(request, activeG, supportMessage) Then
                message = supportMessage
                AppendTrace(strTrace, message)
                Return False
            End If

            Dim delegateTrace As String = strTrace
            gradient = Function(theta() As Double) As Double()
                           Dim evalResult As MixedModelAnalyticGradientEvaluation = Nothing
                           If Not TryEvaluateGradientCore(request,
                                                          theta,
                                                          activeG,
                                                          objectiveEvaluator,
                                                          cancellationRequested,
                                                          evalResult,
                                                          delegateTrace,
                                                          bValidateAgainstFiniteDifference:=False) Then
                               If evaluationObserver IsNot Nothing Then evaluationObserver(evalResult)
                               Throw New ApplicationException(If(String.IsNullOrWhiteSpace(evalResult.Message), "Analytic covariance-gradient evaluation failed.", evalResult.Message))
                           End If
                           If evaluationObserver IsNot Nothing Then evaluationObserver(evalResult)
                           Return evalResult.Gradient
                       End Function

            message = "Analytic covariance-gradient provider created for supported mixed-model covariance structures."
            AppendTrace(strTrace, message)
            Return True
        End Function

        Friend Function TryEvaluateGradient(request As MixedModelFitRequest,
                                            theta() As Double,
                                            activeG As MixedModelGStruct,
                                            objectiveEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                            cancellationRequested As Func(Of Boolean),
                                            ByRef result As MixedModelAnalyticGradientEvaluation,
                                            Optional ByRef strTrace As String = Nothing,
                                            Optional validateAgainstFiniteDifference As Boolean = False) As Boolean
            Return TryEvaluateGradientCore(request,
                                           theta,
                                           activeG,
                                           objectiveEvaluator,
                                           cancellationRequested,
                                           result,
                                           strTrace,
                                           validateAgainstFiniteDifference)
        End Function

        Private Function TryEvaluateGradientCore(request As MixedModelFitRequest,
                                                 theta() As Double,
                                                 activeG As MixedModelGStruct,
                                                 objectiveEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                                 cancellationRequested As Func(Of Boolean),
                                                 ByRef result As MixedModelAnalyticGradientEvaluation,
                                                 ByRef strTrace As String,
                                                 bValidateAgainstFiniteDifference As Boolean) As Boolean
            result = New MixedModelAnalyticGradientEvaluation With {
                .Success = False,
                .Message = String.Empty,
                .Criterion = Double.NaN,
                .Gradient = Nothing,
                .MaxRelativeFiniteDifferenceDiscrepancy = Double.NaN,
                .FailedParameterIndex = -1
            }

            Try
                ThrowIfCancellationRequested(cancellationRequested)

                Dim supportMessage As String = Nothing
                If Not IsSupportedAnalyticGradientRequest(request, activeG, supportMessage) Then
                    result.Message = supportMessage
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If

                If theta Is Nothing Then
                    result.Message = "Covariance-parameter vector is missing."
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If
                If objectiveEvaluator Is Nothing Then
                    result.Message = "Profile objective evaluator is missing."
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If

                Dim data As MixedModelBlockData = request.Data
                Dim gCount As Integer = If(activeG Is Nothing, 0, activeG.ParamCount(data.Q))
                Dim rCount As Integer = request.ResidualStruct.ParamCount(data)
                Dim totalCount As Integer = gCount + rCount
                If theta.Length <> totalCount Then
                    result.Message = "Analytic covariance-gradient theta length mismatch. Expected " & totalCount.ToString(System.Globalization.CultureInfo.InvariantCulture) & " parameters (G=" & gCount.ToString(System.Globalization.CultureInfo.InvariantCulture) & ", R=" & rCount.ToString(System.Globalization.CultureInfo.InvariantCulture) & "), received " & theta.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) & "."
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If

                Dim profile As MixedModelProfileEvaluation = objectiveEvaluator(theta)
                If Not profile.Success OrElse Not IsFinite(profile.Criterion) Then
                    result.Message = "Profile objective evaluation failed during analytic-gradient calculation: " & If(profile.Message, String.Empty)
                    result.Criterion = profile.Criterion
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If
                If profile.Beta Is Nothing OrElse profile.Beta.Length <> data.P Then
                    result.Message = "Profile objective evaluation did not return a compatible fixed-effect estimate."
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If
                If request.FitMethod = MixedModelFitMethod.REML Then
                    If profile.VarBeta Is Nothing OrElse profile.VarBeta.GetLength(0) <> data.P OrElse profile.VarBeta.GetLength(1) <> data.P Then
                        result.Message = "Profile objective evaluation did not return a compatible Var(beta) matrix for REML analytic-gradient calculation."
                        AppendTrace(strTrace, result.Message)
                        Return False
                    End If
                End If

                Dim thetaG() As Double = SliceVector(theta, 0, gCount)
                Dim thetaR() As Double = SliceVector(theta, gCount, rCount)
                Dim gradient(theta.Length - 1) As Double
                Dim evalTrace As String = Nothing
                Dim usePatternCache As Boolean = request.Control.UseAnalyticGradientDerivativePatternCache
                Dim patternCache As Dictionary(Of String, AnalyticGradientPatternCacheEntry) = Nothing
                If usePatternCache Then patternCache = New Dictionary(Of String, AnalyticGradientPatternCacheEntry)(StringComparer.Ordinal)
                Dim cacheStats As New AnalyticGradientPatternCacheStatistics()
                cacheStats.Enabled = usePatternCache

                For Each block As MixedModelSubjectBlock In data.Blocks
                    ThrowIfCancellationRequested(cancellationRequested)

                    Dim entry As AnalyticGradientPatternCacheEntry = Nothing
                    Dim patternKey As String = Nothing
                    If usePatternCache Then
                        patternKey = BuildAnalyticDerivativePatternKey(request, block)
                        If patternCache.TryGetValue(patternKey, entry) Then
                            cacheStats.Hits += 1
                        Else
                            cacheStats.Misses += 1
                            If Not TryBuildAnalyticGradientPatternCacheEntry(request,
                                                                             thetaG,
                                                                             thetaR,
                                                                             activeG,
                                                                             block,
                                                                             data,
                                                                             profile,
                                                                             gCount,
                                                                             rCount,
                                                                             entry,
                                                                             cacheStats,
                                                                             evalTrace,
                                                                             result.Message) Then
                                AppendTrace(strTrace, result.Message)
                                Return False
                            End If
                            patternCache(patternKey) = entry
                        End If
                    Else
                        cacheStats.Misses += 1
                        If Not TryBuildAnalyticGradientPatternCacheEntry(request,
                                                                         thetaG,
                                                                         thetaR,
                                                                         activeG,
                                                                         block,
                                                                         data,
                                                                         profile,
                                                                         gCount,
                                                                         rCount,
                                                                         entry,
                                                                         cacheStats,
                                                                         evalTrace,
                                                                         result.Message) Then
                            AppendTrace(strTrace, result.Message)
                            Return False
                        End If
                    End If

                    Dim x(,) As Double = block.X
                    Dim y() As Double = block.Y

                    Dim residual() As Double = BuildResidualVector(y, x, profile.Beta)
                    Dim a() As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(entry.Cholesky, residual)

                    Dim contractionStopwatch As Stopwatch = Stopwatch.StartNew()
                    For h As Integer = 0 To gCount - 1
                        Dim traceTerm As Double = TraceDerivativeProduct(entry.TraceMatrix, entry.GDerivatives, h)
                        Dim quadraticTerm As Double = QuadraticDerivativeProduct(a, entry.GDerivatives, h)
                        gradient(h) += traceTerm - quadraticTerm
                    Next
                    For h As Integer = 0 To rCount - 1
                        Dim traceTerm As Double = TraceDerivativeProduct(entry.TraceMatrix, entry.RDerivatives, h)
                        Dim quadraticTerm As Double = QuadraticDerivativeProduct(a, entry.RDerivatives, h)
                        gradient(gCount + h) += traceTerm - quadraticTerm
                    Next
                    contractionStopwatch.Stop()
                    cacheStats.TraceQuadraticContractionTimeMs += contractionStopwatch.Elapsed.TotalMilliseconds
                Next

                result.AnalyticDerivativePatternCacheEnabled = usePatternCache
                result.AnalyticDerivativePatternCount = If(patternCache Is Nothing, 0, patternCache.Count)
                result.AnalyticDerivativePatternCacheHits = cacheStats.Hits
                result.AnalyticDerivativePatternCacheMisses = cacheStats.Misses
                result.AnalyticDerivativeMatricesBuilt = cacheStats.DerivativeMatricesBuilt
                result.AnalyticTraceQuadraticContractionTimeMs = cacheStats.TraceQuadraticContractionTimeMs


                If Not VectorIsFinite(gradient) Then
                    result.Message = "Analytic covariance-gradient vector contains a non-finite value."
                    AppendTrace(strTrace, result.Message)
                    Return False
                End If

                result.Success = True
                result.Message = "OK"
                result.Criterion = profile.Criterion
                result.Gradient = gradient

                If bValidateAgainstFiniteDifference Then
                    ValidateAgainstFiniteDifference(request, theta, objectiveEvaluator,
                                                    cancellationRequested, result, strTrace)
                End If

                If Not String.IsNullOrEmpty(evalTrace) Then AppendTrace(strTrace, evalTrace.TrimEnd())
                Return True

            Catch ex As OperationCanceledException
                Throw
            Catch ex As Exception
                result.Success = False
                result.Message = ex.Message
                result.Gradient = Nothing
                AppendTrace(strTrace, "Analytic covariance-gradient evaluation failed: " & ex.Message)
                Return False
            End Try
        End Function

        Private Class AnalyticGradientPatternCacheEntry
            Public Cholesky(,) As Double
            Public GDerivatives As Double(,,)
            Public RDerivatives As Double(,,)
            Public TraceMatrix(,) As Double
        End Class

        Private Structure AnalyticGradientPatternCacheStatistics
            Public Enabled As Boolean
            Public Hits As Integer
            Public Misses As Integer
            Public DerivativeMatricesBuilt As Integer
            Public TraceQuadraticContractionTimeMs As Double
        End Structure

        Private Function TryBuildAnalyticGradientPatternCacheEntry(request As MixedModelFitRequest,
                                                                   thetaG() As Double,
                                                                   thetaR() As Double,
                                                                   activeG As MixedModelGStruct,
                                                                   block As MixedModelSubjectBlock,
                                                                   data As MixedModelBlockData,
                                                                   profile As MixedModelProfileEvaluation,
                                                                   gCount As Integer,
                                                                   rCount As Integer,
                                                                   ByRef entry As AnalyticGradientPatternCacheEntry,
                                                                   ByRef cacheStats As AnalyticGradientPatternCacheStatistics,
                                                                   ByRef evalTrace As String,
                                                                   ByRef message As String) As Boolean
            entry = Nothing
            message = Nothing

            Dim vi(,) As Double = MixedModelCovariance.BuildVi(block,
                                                               data,
                                                               activeG,
                                                               request.ResidualStruct,
                                                               thetaG,
                                                               thetaR,
                                                               evalTrace)
            Dim chol(,) As Double = Nothing
            If Not MixedModelCovariance.TryCholesky(vi, chol, evalTrace) Then
                message = "V_i was not positive definite during analytic-gradient calculation for subject '" & block.SubjectKey & "'."
                Return False
            End If

            Dim gDerivatives As Double(,,) = Nothing
            Dim derivativeMessage As String = Nothing
            If gCount > 0 Then
                If Not MixedModelCovarianceDerivatives.TryBuildGDerivatives(activeG,
                                                                            thetaG,
                                                                            block,
                                                                            data,
                                                                            gDerivatives,
                                                                            derivativeMessage) Then
                    message = If(String.IsNullOrWhiteSpace(derivativeMessage), "G-side covariance derivative provider failed.", derivativeMessage)
                    Return False
                End If
            End If

            Dim rDerivatives As Double(,,) = Nothing
            If Not MixedModelCovarianceDerivatives.TryBuildRDerivatives(request.ResidualStruct,
                                                                        thetaR,
                                                                        block,
                                                                        data,
                                                                        rDerivatives,
                                                                        derivativeMessage) Then
                message = If(String.IsNullOrWhiteSpace(derivativeMessage), "R-side covariance derivative provider failed.", derivativeMessage)
                Return False
            End If
            cacheStats.DerivativeMatricesBuilt += gCount + rCount

            Dim x(,) As Double = block.X
            Dim vinv(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
            Dim traceMatrix(,) As Double = vinv
            If request.FitMethod = MixedModelFitMethod.REML Then
                Dim vinvX(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholSolve(chol, x)
                traceMatrix = BuildRemlProjectionTraceMatrix(vinv, vinvX, profile.VarBeta)
            End If

            entry = New AnalyticGradientPatternCacheEntry With {
                .Cholesky = chol,
                .GDerivatives = gDerivatives,
                .RDerivatives = rDerivatives,
                .TraceMatrix = traceMatrix
            }
            Return True
        End Function

        Private Function BuildAnalyticDerivativePatternKey(request As MixedModelFitRequest,
                                                           block As MixedModelSubjectBlock) As String
            Dim sb As New StringBuilder()
            If request IsNot Nothing AndAlso request.ResidualStruct IsNot Nothing Then
                sb.Append(request.ResidualStruct.GetType().FullName)
            End If
            sb.Append("|g=")
            If request IsNot Nothing AndAlso request.RandomStruct IsNot Nothing Then
                sb.Append(request.RandomStruct.GetType().FullName)
            Else
                sb.Append("<none>")
            End If
            sb.Append("|n=").Append(block.Nobs.ToString(System.Globalization.CultureInfo.InvariantCulture))
            sb.Append("|vis=")
            AppendVisitPatternKey(sb, block)
            If block IsNot Nothing AndAlso block.HasRandomEffectsDesign() Then
                sb.Append("|z=")
                AppendMatrixPatternKey(sb, block.Z)
            End If

            ' The REML trace term depends on X_i as well as the visit/R pattern. Include the
            ' fixed-effect design values so cached trace matrices are reused only for truly
            ' repeated design patterns.
            If request IsNot Nothing AndAlso request.FitMethod = MixedModelFitMethod.REML Then
                sb.Append("|x=")
                AppendMatrixPatternKey(sb, block.X)
            End If

            Return sb.ToString()
        End Function

        Private Sub AppendVisitPatternKey(sb As StringBuilder, block As MixedModelSubjectBlock)
            Dim idx() As Integer = block.VisitIndex
            If idx Is Nothing OrElse idx.Length <> block.Nobs Then
                For i As Integer = 0 To block.Nobs - 1
                    If i > 0 Then sb.Append(",")
                    sb.Append(i.ToString(System.Globalization.CultureInfo.InvariantCulture))
                Next
                Exit Sub
            End If

            For i As Integer = 0 To idx.Length - 1
                If i > 0 Then sb.Append(",")
                sb.Append(idx(i).ToString(System.Globalization.CultureInfo.InvariantCulture))
            Next
        End Sub

        Private Sub AppendMatrixPatternKey(sb As StringBuilder, x(,) As Double)
            If x Is Nothing Then
                sb.Append("<null>")
                Exit Sub
            End If
            sb.Append(x.GetLength(0).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append("x").Append(x.GetLength(1).ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(":")
            For i As Integer = 0 To x.GetLength(0) - 1
                If i > 0 Then sb.Append(";")
                For j As Integer = 0 To x.GetLength(1) - 1
                    If j > 0 Then sb.Append(",")
                    sb.Append(x(i, j).ToString("R", System.Globalization.CultureInfo.InvariantCulture))
                Next
            Next
        End Sub

        Private Function IsSupportedAnalyticGradientRequest(request As MixedModelFitRequest,
                                                            activeG As MixedModelGStruct,
                                                            ByRef message As String) As Boolean
            message = Nothing
            If request Is Nothing Then
                message = "Mixed-model fit request is missing."
                Return False
            End If
            If request.Data Is Nothing Then
                message = "Mixed-model block data is missing."
                Return False
            End If
            If request.ResidualStruct Is Nothing Then
                message = "Residual covariance structure is missing."
                Return False
            End If

            Dim data As MixedModelBlockData = request.Data
            Dim rCount As Integer = 0
            Try
                rCount = request.ResidualStruct.ParamCount(data)
            Catch ex As Exception
                message = "Residual covariance structure is not supported for analytic gradients: " & ex.Message
                Return False
            End Try
            If rCount < 0 Then
                message = "Residual covariance structure returned a negative parameter count."
                Return False
            End If

            Dim gCount As Integer = 0
            Dim hasActiveG As Boolean = activeG IsNot Nothing AndAlso Not activeG.IsDegenerateZeroG()
            If hasActiveG Then
                If data.Q <= 0 Then
                    message = "Active G-side random-effects structure requires a random-effects design matrix Z."
                    Return False
                End If

                Try
                    gCount = activeG.ParamCount(data.Q)
                Catch ex As Exception
                    message = "G-side random-effects structure is not supported for analytic gradients: " & ex.Message
                    Return False
                End Try

                If Not (TypeOf activeG Is RandomIntercept OrElse
                        TypeOf activeG Is RandomInterceptSlope OrElse
                        TypeOf activeG Is UnstructuredRandomEffects) Then
                    message = MixedModelCovarianceDerivatives.UnsupportedGDerivativeMessage & " Structure='" & activeG.ToString() & "'."
                    Return False
                End If

                If gCount <= 0 Then
                    message = "Active G-side random-effects structure returned no covariance parameters."
                    Return False
                End If
            ElseIf activeG IsNot Nothing Then
                Try
                    gCount = activeG.ParamCount(data.Q)
                Catch ex As Exception
                    message = "Degenerate G-side structure failed parameter validation: " & ex.Message
                    Return False
                End Try
                If gCount <> 0 Then
                    message = "Degenerate G-side structure returned unexpected covariance parameters."
                    Return False
                End If
            End If

            message = "OK"
            Return True
        End Function

        Private Function SliceVector(values() As Double, startIndex As Integer, length As Integer) As Double()
            If length <= 0 Then Return Array.Empty(Of Double)()
            Dim out(length - 1) As Double
            Array.Copy(values, startIndex, out, 0, length)
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

        Private Function BuildRemlProjectionTraceMatrix(vinv(,) As Double,
                                                        vinvX(,) As Double,
                                                        c(,) As Double) As Double(,)
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

        Private Function TraceDerivativeProduct(traceMatrix(,) As Double,
                                                derivatives As Double(,,),
                                                parameterIndex As Integer) As Double
            Dim n As Integer = traceMatrix.GetLength(0)
            Dim sum As Double = 0.0
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    sum += traceMatrix(i, j) * derivatives(parameterIndex, j, i)
                Next
            Next
            Return sum
        End Function

        Private Function QuadraticDerivativeProduct(a() As Double,
                                                    derivatives As Double(,,),
                                                    parameterIndex As Integer) As Double
            Dim n As Integer = a.Length
            Dim sum As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim ai As Double = a(i)
                If ai = 0.0 Then Continue For
                For j As Integer = 0 To n - 1
                    sum += ai * derivatives(parameterIndex, i, j) * a(j)
                Next
            Next
            Return sum
        End Function

        Private Sub ValidateAgainstFiniteDifference(request As MixedModelFitRequest,
                                                    theta() As Double,
                                                    objectiveEvaluator As Func(Of Double(), MixedModelProfileEvaluation),
                                                    cancellationRequested As Func(Of Boolean),
                                                    ByRef result As MixedModelAnalyticGradientEvaluation,
                                                    ByRef strTrace As String)
            If result.Gradient Is Nothing Then Exit Sub

            Dim objective As Func(Of Double(), Double) =
                Function(candidate() As Double) As Double
                    ThrowIfCancellationRequested(cancellationRequested)
                    Dim ev As MixedModelProfileEvaluation = objectiveEvaluator(candidate)
                    If ev.Success AndAlso IsFinite(ev.Criterion) Then Return ev.Criterion
                    Return PenaltyObjective
                End Function

            Dim validationTrace As String = Nothing
            Dim numeric() As Double = MixedModelOptimizer.NumericalGradient(objective,
                                                                           theta,
                                                                           ValidationFiniteDifferenceStep,
                                                                           Nothing,
                                                                           Nothing,
                                                                           validationTrace,
                                                                           cancellationRequested)

            Dim maxRelative As Double = 0.0
            Dim failedIndex As Integer = -1
            For i As Integer = 0 To result.Gradient.Length - 1
                Dim analyticValue As Double = result.Gradient(i)
                Dim numericValue As Double = numeric(i)
                Dim scale As Double = Math.Max(1.0, Math.Max(Math.Abs(analyticValue), Math.Abs(numericValue)))
                Dim rel As Double = Math.Abs(analyticValue - numericValue) / scale
                If Not IsFinite(rel) Then rel = Double.PositiveInfinity
                If rel > maxRelative Then
                    maxRelative = rel
                    failedIndex = i
                End If
            Next

            result.MaxRelativeFiniteDifferenceDiscrepancy = maxRelative
            result.FailedParameterIndex = failedIndex

            Dim tolerance As Double = request.Control.AnalyticGradientValidationTolerance
            If tolerance <= 0.0 OrElse Not IsFinite(tolerance) Then tolerance = 0.0001
            Dim validationMessage As String = "Analytic-gradient finite-difference validation max relative discrepancy=" & maxRelative.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & ", parameter=" & failedIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) & ", tolerance=" & tolerance.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & "."
            If maxRelative > tolerance Then
                result.Message = validationMessage
            End If
            AppendTrace(strTrace, validationMessage)
            If Not String.IsNullOrWhiteSpace(validationTrace) Then AppendTrace(strTrace, validationTrace.TrimEnd())
        End Sub

        Private Function VectorIsFinite(values() As Double) As Boolean
            If values Is Nothing Then Return False
            For i As Integer = 0 To values.Length - 1
                If Not IsFinite(values(i)) Then Return False
            Next
            Return True
        End Function

        Private Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Private Sub ThrowIfCancellationRequested(cancellationRequested As Func(Of Boolean))
            If cancellationRequested Is Nothing Then Exit Sub
            If cancellationRequested.Invoke() Then Throw New OperationCanceledException("MMRM calculation cancelled by user.")
        End Sub

        Private Sub AppendTrace(ByRef strTrace As String, message As String)
            If String.IsNullOrWhiteSpace(message) Then Exit Sub
            If strTrace Is Nothing Then strTrace = String.Empty
            If strTrace.Length > 0 AndAlso Not strTrace.EndsWith(Environment.NewLine, StringComparison.Ordinal) Then
                strTrace &= Environment.NewLine
            End If
            strTrace &= "MixedModelAnalyticGradient: " & message & Environment.NewLine
        End Sub

    End Module

End Namespace
