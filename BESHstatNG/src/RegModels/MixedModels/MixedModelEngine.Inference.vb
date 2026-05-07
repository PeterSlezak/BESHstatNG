Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ' <summary>
    ' Post-estimation fixed-effect inference, Satterthwaite, and Kenward-Roger helpers
    ' for <see cref="MixedModelEngine"/>.
    ' </summary>
    ' <remarks>
    ' This partial class file intentionally contains the inference-heavy code that was
    ' formerly embedded in MixedModelEngine.vb, leaving the original file focused on
    ' fitting, likelihood evaluation, optimization, result assembly, diagnostics, and logging.
    ' </remarks>
    Partial Public Class MixedModelEngine

        Private Function ResolveFixedInferenceMethod() As MixedModelFixedInferenceMethod
            If pRequest Is Nothing Then Return MixedModelFixedInferenceMethod.WaldNormal

            Dim requested As MixedModelFixedInferenceMethod = pRequest.FixedInferenceMethod

            If pRequest.UseSatterthwaite Then
                requested = MixedModelFixedInferenceMethod.Satterthwaite
            End If

            If requested = MixedModelFixedInferenceMethod.KenwardRoger Then
                ' This is currently an internal validation option.  It is allowed for both
                ' MMRM and LMM only when the KR workspace is requested or explicitly implied
                ' by BuildResult.  PopulateFixedEffectDiagnostics falls back if the workspace
                ' cannot be built.
                Return MixedModelFixedInferenceMethod.KenwardRoger
            End If

            ' BetweenWithin is meaningful only for MMRM/no-random-effects fits.  For ordinary LMM,
            ' keep this implementation honest and fall back to large-sample normal until LMM-specific
            ' df methods are deliberately validated.
            If requested = MixedModelFixedInferenceMethod.BetweenWithin AndAlso Not IsCurrentMMRMFit() Then
                Return MixedModelFixedInferenceMethod.WaldNormal
            End If

            ' This first Satterthwaite implementation is designed for the MMRM path.
            ' It may also run for LMM, but LMM validation is intentionally deferred.
            If requested = MixedModelFixedInferenceMethod.Satterthwaite AndAlso Not IsCurrentMMRMFit() Then
                Return MixedModelFixedInferenceMethod.WaldNormal
            End If

            Return requested
        End Function

        Private Function ComputeFixedEffectDenominatorDFs(res As MixedModelResult) As Double()
            Dim p As Integer = res.P
            Dim method As MixedModelFixedInferenceMethod = ResolveFixedInferenceMethod()

            If method = MixedModelFixedInferenceMethod.WaldNormal Then
                Return MakeNaNVector(p)
            End If

            If method = MixedModelFixedInferenceMethod.ResidualDF OrElse Not IsCurrentMMRMFit() Then
                Return ComputeResidualDenominatorDFs(p)
            End If

            If method = MixedModelFixedInferenceMethod.Satterthwaite Then
                Dim sat() As Double = ComputeSatterthwaiteDenominatorDFs(res)

                If HasUsableDFVector(sat, p) Then
                    Return sat
                End If

                AppendWarn("Satterthwaite denominator df could not be computed reliably; falling back to Between-within df.")
                Return ComputeBetweenWithinDenominatorDFs(p)
            End If

            Return ComputeBetweenWithinDenominatorDFs(p)
        End Function

        Private Function IsCurrentMMRMFit() As Boolean
            Return pRequest IsNot Nothing AndAlso pRequest.Data IsNot Nothing AndAlso pRequest.Data.Q = 0 AndAlso ActiveGStruct() Is Nothing
        End Function

        Private Function IsUnstructuredMMRMResidual() As Boolean
            Return IsCurrentMMRMFit() AndAlso pRequest.ResidualStruct IsNot Nothing AndAlso TypeOf pRequest.ResidualStruct Is UnstructuredR
        End Function

        Private Function GetSubjectConstantFixedEffectColumns() As Boolean()
            Dim p As Integer = pRequest.Data.P
            Dim out(p - 1) As Boolean
            For j As Integer = 0 To p - 1
                out(j) = Not ColumnVariesWithinAnySubject(j, 0.0000000001R)
            Next
            Return out
        End Function

        Private Function ComputeSubjectLevelDesignRank(subjectConstantCols() As Boolean) As Integer
            Dim selected As New List(Of Integer)
            For j As Integer = 0 To subjectConstantCols.Length - 1
                If subjectConstantCols(j) Then selected.Add(j)
            Next

            If selected.Count = 0 Then Return 0

            Dim m As Integer = pRequest.Data.NoSubjects
            Dim k As Integer = selected.Count
            Dim a(m - 1, k - 1) As Double

            For i As Integer = 0 To m - 1
                Dim block As MixedModelSubjectBlock = pRequest.Data.Blocks(i)
                For c As Integer = 0 To k - 1
                    a(i, c) = block.X(0, selected(c))
                Next
            Next

            Return NumericRankByModifiedGramSchmidt(a, 0.00000001R)
        End Function

        Private Shared Function NumericRankByModifiedGramSchmidt(a(,) As Double, tol As Double) As Integer
            If a Is Nothing Then Return 0
            Dim nRows As Integer = a.GetLength(0)
            Dim nCols As Integer = a.GetLength(1)
            If nRows = 0 OrElse nCols = 0 Then Return 0

            Dim basis As New List(Of Double())
            Dim scale As Double = 0.0

            For c As Integer = 0 To nCols - 1
                Dim v(nRows - 1) As Double
                For r As Integer = 0 To nRows - 1
                    v(r) = a(r, c)
                    scale = Math.Max(scale, Math.Abs(v(r)))
                Next

                For Each q() As Double In basis
                    Dim proj As Double = 0.0
                    For r As Integer = 0 To nRows - 1
                        proj += v(r) * q(r)
                    Next
                    For r As Integer = 0 To nRows - 1
                        v(r) -= proj * q(r)
                    Next
                Next

                Dim norm2 As Double = 0.0
                For r As Integer = 0 To nRows - 1
                    norm2 += v(r) * v(r)
                Next
                Dim norm As Double = Math.Sqrt(norm2)

                If norm > tol * Math.Max(1.0, scale) Then
                    For r As Integer = 0 To nRows - 1
                        v(r) /= norm
                    Next
                    basis.Add(v)
                End If
            Next

            Return basis.Count
        End Function

        Private Shared Function MakeNaNVector(n As Integer) As Double()
            If n <= 0 Then Return Array.Empty(Of Double)()
            Dim out(n - 1) As Double
            For i As Integer = 0 To n - 1
                out(i) = Double.NaN
            Next
            Return out
        End Function

        Private Function ColumnVariesWithinAnySubject(colIndex As Integer, tol As Double) As Boolean
            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                If block Is Nothing OrElse block.X Is Nothing OrElse block.Nobs <= 1 Then Continue For

                Dim first As Double = block.X(0, colIndex)
                For i As Integer = 1 To block.Nobs - 1
                    Dim current As Double = block.X(i, colIndex)
                    If IsFinite(first) AndAlso IsFinite(current) Then
                        If Math.Abs(current - first) > tol Then Return True
                    ElseIf Double.IsNaN(first) <> Double.IsNaN(current) OrElse Double.IsInfinity(first) <> Double.IsInfinity(current) Then
                        Return True
                    End If
                Next
            Next
            Return False
        End Function

        Private Function ComputeResidualDenominatorDFs(p As Integer) As Double()
            Dim out(p - 1) As Double
            Dim residualDF As Double = Math.Max(1.0, CDbl(pRequest.Data.Nobs - pRequest.Data.P))

            For j As Integer = 0 To p - 1
                out(j) = residualDF
            Next

            Return out
        End Function


        Private Function ComputeBetweenWithinDenominatorDFs(p As Integer) As Double()
            Dim out(p - 1) As Double

            Dim betweenCols As Boolean() = GetSubjectConstantFixedEffectColumns()
            Dim interceptCols As Boolean() = GetInterceptFixedEffectColumns()

            ' R mmrm-style between-within df for MMRM fixed effects:
            '   DF_between = N_subjects - (N0 + p_between)
            '   DF_within  = N_obs - (N_subjects + p_within)
            '
            ' Compute the parameter counts by rank, not by raw dummy-column count, so aliased or
            ' non-estimable coding artifacts do not inflate the denominator-df subtraction.  The
            ' subject-level rank includes the intercept when present, matching N0 + p_between.
            Dim betweenRank As Integer = ComputeSubjectLevelDesignRank(betweenCols)
            Dim withinRank As Integer = Math.Max(0, pRequest.Data.P - betweenRank)

            Dim betweenDF As Double = Math.Max(1.0, CDbl(pRequest.Data.NoSubjects - betweenRank))
            Dim withinDF As Double = Math.Max(1.0, CDbl(pRequest.Data.Nobs - pRequest.Data.NoSubjects - withinRank))

            For j As Integer = 0 To p - 1
                ' mmrm treats the intercept specially: although it is subject-constant,
                ' the intercept coefficient itself uses the within-subject denominator df.
                ' Interactions that contain a within-subject factor naturally vary within
                ' subjects and therefore also receive withinDF.
                If interceptCols(j) Then
                    out(j) = withinDF
                Else
                    out(j) = If(betweenCols(j), betweenDF, withinDF)
                End If
            Next

            Return out
        End Function

        Private Function GetInterceptFixedEffectColumns() As Boolean()
            Dim p As Integer = pRequest.Data.P
            Dim out(p - 1) As Boolean
            Dim names() As String = GetFixedEffectNames()

            For j As Integer = 0 To p - 1
                Dim nameLooksLikeIntercept As Boolean = False
                If names IsNot Nothing AndAlso j < names.Length Then
                    Dim nm As String = If(names(j), String.Empty).Trim()
                    nameLooksLikeIntercept = String.Equals(nm, "Intercept", StringComparison.OrdinalIgnoreCase) OrElse
                                             String.Equals(nm, "(Intercept)", StringComparison.OrdinalIgnoreCase)
                End If

                out(j) = nameLooksLikeIntercept OrElse ColumnIsConstantValue(j, 1.0, 0.0000000001R)
            Next

            Return out
        End Function

        Private Function ColumnIsConstantValue(colIndex As Integer, expected As Double, tol As Double) As Boolean
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.Data.Blocks Is Nothing Then Return False

            Dim sawValue As Boolean = False
            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                If block Is Nothing OrElse block.X Is Nothing Then Continue For

                For i As Integer = 0 To block.Nobs - 1
                    Dim current As Double = block.X(i, colIndex)
                    If Not IsFinite(current) OrElse Math.Abs(current - expected) > tol Then Return False
                    sawValue = True
                Next
            Next

            Return sawValue
        End Function


        Private Function ComputeSatterthwaiteDenominatorDFs(res As MixedModelResult) As Double()
            Dim p As Integer = res.P
            Dim out(p - 1) As Double

            For j As Integer = 0 To p - 1
                out(j) = Double.NaN
            Next

            If res Is Nothing OrElse res.Theta Is Nothing OrElse res.Theta.Length = 0 Then
                Return ComputeResidualDenominatorDFs(p)
            End If

            If res.VarBeta Is Nothing OrElse res.VarBeta.GetLength(0) <> p OrElse res.VarBeta.GetLength(1) <> p Then
                Return out
            End If

            Dim theta() As Double = CType(res.Theta.Clone(), Double())
            Dim thetaCov(,) As Double = ComputeApproxCovThetaFromProfileHessian(theta)

            If thetaCov Is Nothing Then
                Return out
            End If

            Dim gradMats(,,) As Double = ComputeVarBetaThetaGradientMatricesForSatterthwaite(theta, p)
            If gradMats Is Nothing Then
                Return out
            End If

            ' Unified Satterthwaite storage.  The legacy result properties are aliases to this workspace.
            res.InferenceWorkspace = New regression.MixedModelInferenceWorkspace With {
                .P = p,
                .K = theta.Length,
                .VarBeta = res.VarBeta,
                .ThetaCovariance = thetaCov,
                .VarBetaGradient = gradMats
            }

            For j As Integer = 0 To p - 1
                Dim l(p - 1) As Double
                l(j) = 1.0

                Dim df As Double = Double.NaN
                If regression.MixedModelInferenceMath.TrySatterthwaiteDF(l, res.InferenceWorkspace, df) Then
                    out(j) = df
                End If
            Next

            Return out
        End Function


        ''' <summary>
        ''' Approximates Cov(theta) from the Hessian of the profiled criterion.
        ''' Since the criterion is -2 log L, Cov(theta) is approximated as 2 * H^{-1}.
        ''' </summary>
        Private Function ComputeApproxCovThetaFromProfileHessian(theta() As Double) As Double(,)
            Try
                If theta Is Nothing OrElse theta.Length = 0 Then Return Nothing

                Dim hess(,) As Double = ComputeProfileCriterionHessian(theta)
                If hess Is Nothing Then Return Nothing

                SymmetrizeInPlace(hess)

                Dim invH(,) As Double = Global.BESHStatNG.Matrix.Matrix.pseudoInverse(hess)
                If invH Is Nothing Then Return Nothing

                Dim m As Integer = invH.GetLength(0)
                Dim out(m - 1, m - 1) As Double

                For r As Integer = 0 To m - 1
                    For c As Integer = 0 To m - 1
                        out(r, c) = 2.0 * invH(r, c)
                    Next
                Next

                SymmetrizeInPlace(out)
                Return out

            Catch ex As Exception
                AppendWarn("Satterthwaite Cov(theta) approximation failed: " & ex.Message)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Computes a central-difference Hessian of the profiled ML/REML criterion
        ''' with respect to the internal covariance parameter vector.
        ''' </summary>
        Private Function ComputeProfileCriterionHessian(theta() As Double) As Double(,)
            If theta Is Nothing OrElse theta.Length = 0 Then Return Nothing

            Dim m As Integer = theta.Length
            Dim h(m - 1) As Double
            For j As Integer = 0 To m - 1
                h(j) = GetSatterthwaiteFiniteDifferenceStep(theta(j))
            Next

            Dim baseEval As MixedModelProfileEvaluation = EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False)
            If Not baseEval.Success OrElse Not IsFinite(baseEval.Criterion) Then Return Nothing

            Dim f0 As Double = baseEval.Criterion
            Dim out(m - 1, m - 1) As Double
            Dim hessianWorkTotal As Integer = Math.Max(1, (m * (m + 1)) \ 2)
            Dim hessianWorkDone As Integer = 0
            ReportProgress("Kenward-Roger theta covariance/Hessian", 96, iteration:=0, maxIterations:=hessianWorkTotal)

            For i As Integer = 0 To m - 1
                Dim tPlus() As Double = CType(theta.Clone(), Double())
                Dim tMinus() As Double = CType(theta.Clone(), Double())
                tPlus(i) += h(i)
                tMinus(i) -= h(i)

                Dim fPlus As Double = SafeCriterionValue(tPlus)
                Dim fMinus As Double = SafeCriterionValue(tMinus)

                If IsFinite(fPlus) AndAlso IsFinite(fMinus) Then
                    out(i, i) = (fPlus - 2.0 * f0 + fMinus) / (h(i) * h(i))
                Else
                    out(i, i) = 0.0
                End If

                hessianWorkDone += 1
                ReportProgress("Kenward-Roger theta covariance/Hessian", 96, iteration:=hessianWorkDone, maxIterations:=hessianWorkTotal)

                For j As Integer = i + 1 To m - 1
                    Dim tPP() As Double = CType(theta.Clone(), Double())
                    Dim tPM() As Double = CType(theta.Clone(), Double())
                    Dim tMP() As Double = CType(theta.Clone(), Double())
                    Dim tMM() As Double = CType(theta.Clone(), Double())

                    tPP(i) += h(i) : tPP(j) += h(j)
                    tPM(i) += h(i) : tPM(j) -= h(j)
                    tMP(i) -= h(i) : tMP(j) += h(j)
                    tMM(i) -= h(i) : tMM(j) -= h(j)

                    Dim fPP As Double = SafeCriterionValue(tPP)
                    Dim fPM As Double = SafeCriterionValue(tPM)
                    Dim fMP As Double = SafeCriterionValue(tMP)
                    Dim fMM As Double = SafeCriterionValue(tMM)

                    If IsFinite(fPP) AndAlso IsFinite(fPM) AndAlso IsFinite(fMP) AndAlso IsFinite(fMM) Then
                        Dim hij As Double = (fPP - fPM - fMP + fMM) / (4.0 * h(i) * h(j))
                        out(i, j) = hij
                        out(j, i) = hij
                    Else
                        out(i, j) = 0.0
                        out(j, i) = 0.0
                    End If

                    hessianWorkDone += 1
                    ReportProgress("Kenward-Roger theta covariance/Hessian", 96, iteration:=hessianWorkDone, maxIterations:=hessianWorkTotal)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function

        Private Function SafeVarBetaDiagonal(theta() As Double, betaIndex As Integer) As Double
            Try
                Dim ev As MixedModelProfileEvaluation = EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False)
                If Not ev.Success OrElse ev.VarBeta Is Nothing Then Return Double.NaN
                If betaIndex < 0 OrElse betaIndex >= ev.VarBeta.GetLength(0) Then Return Double.NaN
                Return ev.VarBeta(betaIndex, betaIndex)
            Catch
                Return Double.NaN
            End Try
        End Function

        Private Function SafeCriterionValue(theta() As Double) As Double
            Try
                Dim ev As MixedModelProfileEvaluation = EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False)
                If ev.Success AndAlso IsFinite(ev.Criterion) Then Return ev.Criterion
            Catch
            End Try

            Return Double.NaN
        End Function


        Private Function GetSatterthwaiteFiniteDifferenceStep(x As Double) As Double
            Dim scale As Double = Math.Max(1.0, Math.Abs(x))

            ' Slightly larger than optimizer gradient step to reduce cancellation in second derivatives.
            Dim h As Double = 0.0001 * scale

            If Not IsFinite(h) OrElse h <= 0.0 Then h = 0.0001
            Return h
        End Function


        Private Shared Function HasUsableDFVector(df() As Double, expectedLength As Integer) As Boolean
            If df Is Nothing OrElse df.Length <> expectedLength Then Return False

            For i As Integer = 0 To df.Length - 1
                If Double.IsNaN(df(i)) OrElse Double.IsInfinity(df(i)) OrElse df(i) <= 0.0 Then
                    Return False
                End If
            Next

            Return True
        End Function


        Friend Shared Function QuadraticFormLMM(v() As Double, a(,) As Double) As Double
            If v Is Nothing OrElse a Is Nothing Then Return Double.NaN
            If a.GetLength(0) <> v.Length OrElse a.GetLength(1) <> v.Length Then Return Double.NaN

            Dim tmp(v.Length - 1) As Double
            For i As Integer = 0 To v.Length - 1
                Dim s As Double = 0.0
                For j As Integer = 0 To v.Length - 1
                    s += a(i, j) * v(j)
                Next
                tmp(i) = s
            Next

            Dim out As Double = 0.0
            For i As Integer = 0 To v.Length - 1
                out += v(i) * tmp(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Builds the internal universal Kenward-Roger derivative workspace for the fitted model.
        ''' </summary>
        ''' <remarks>
        ''' This is deliberately not exposed as final Kenward-Roger inference.  It prepares
        ''' and stores the reusable KR backend ingredients so they can be validated against
        ''' SAS/R for both MMRM and LMM.
        ''' </remarks>
        Private Sub PopulateKenwardRogerWorkspace(res As MixedModelResult)
            Dim workspaceStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try
                If res Is Nothing Then Exit Sub
                If res.Theta Is Nothing OrElse res.Theta.Length = 0 Then
                    res.KenwardRogerStatusMessage = "KR workspace not built: model has no covariance parameters."
                    Exit Sub
                End If

                If res.VarBeta Is Nothing OrElse res.VarBeta.GetLength(0) <> res.P OrElse res.VarBeta.GetLength(1) <> res.P Then
                    res.KenwardRogerStatusMessage = "KR workspace not built: Var(beta) is unavailable or has invalid dimensions."
                    Exit Sub
                End If

                Dim thetaOptimizer() As Double = CType(res.Theta.Clone(), Double())

                Dim thetaCovOpt(,) As Double = Nothing
                If res.InferenceWorkspace IsNot Nothing Then thetaCovOpt = res.InferenceWorkspace.ThetaCovariance
                If thetaCovOpt Is Nothing Then
                    ReportProgress("Kenward-Roger theta covariance/Hessian", 96, message:="Approximating covariance-parameter covariance")
                    thetaCovOpt = ComputeApproxCovThetaFromProfileHessian(thetaOptimizer)
                End If

                If thetaCovOpt Is Nothing Then
                    res.KenwardRogerStatusMessage = "KR workspace not built: covariance of optimizer covariance parameters could not be approximated."
                    Exit Sub
                End If

                Dim krMap As MixedModelKrParameterMap = Nothing
                Dim scaleDiagnostic As String = String.Empty

                If pRequest.KenwardRogerOptions Is Nothing Then
                    pRequest.KenwardRogerOptions = MixedModelKenwardRogerOptions.CreateDefault()
                End If

                If Not MixedModelCovarianceParameterScale.TryCreateParameterMap(pRequest,
                                                                              thetaOptimizer,
                                                                              thetaCovOpt,
                                                                              pRequest.KenwardRogerOptions,
                                                                              krMap,
                                                                              scaleDiagnostic) Then

                    If pRequest.KenwardRogerOptions IsNot Nothing AndAlso pRequest.KenwardRogerOptions.StrictValidation Then
                        res.KenwardRogerStatusMessage = "KR workspace not built: strict KR parameter-scale contract could not be satisfied. " &
                                                         scaleDiagnostic
                        AppendWarn(res.KenwardRogerStatusMessage)
                        Exit Sub
                    End If

                    AppendWarn("KR parameter map unavailable; falling back to optimizer-internal scale. " & scaleDiagnostic)
                    krMap = New MixedModelKrParameterMap With {
                            .OptimizerTheta = thetaOptimizer,
                            .OptimizerThetaCovariance = thetaCovOpt,
                            .KrTheta = thetaOptimizer,
                            .KrThetaCovariance = thetaCovOpt,
                            .ParameterNames = Nothing,
                            .ParameterScale = MixedModelKrParameterScale.OptimizerInternal,
                            .DiagnosticMessage = "KR parameter map fallback: optimizer-internal scale."
                        }
                End If

                If Not String.IsNullOrWhiteSpace(scaleDiagnostic) Then
                    AppendInfo(scaleDiagnostic)
                    If scaleDiagnostic.IndexOf("Warning:", StringComparison.OrdinalIgnoreCase) >= 0 Then
                        AppendWarn("KR covariance-parameter map diagnostic: " & scaleDiagnostic)
                    End If
                End If

                Dim thetaForKr() As Double = krMap.KrTheta
                Dim thetaCovForKr(,) As Double = krMap.KrThetaCovariance
                Dim krScale As MixedModelKrParameterScale = krMap.ParameterScale
                Dim krCovNames() As String = krMap.ParameterNames

                If thetaForKr IsNot Nothing AndAlso res.NoSubjects > 0 AndAlso thetaForKr.Length >= res.NoSubjects Then
                    res.AddUserWarning("KR warning: number of covariance parameters is high relative to subject count; denominator DF and covariance adjustment may be unstable.")
                End If

                Dim blocks As List(Of MixedModelKrBlock) = Nothing
                _krFiniteDifferenceDiagnostics = New MixedModelKrFiniteDifferenceDiagnostics()
                _krDerivativePatternCacheDiagnostics = New MixedModelKrDerivativePatternCacheDiagnostics()
                Dim derivativeBlockStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()
                Dim krDerivativeBlockTotal As Integer = If(pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.Data.Blocks Is Nothing, 0, pRequest.Data.Blocks.Count)
                ReportProgress("Kenward-Roger derivative blocks", 97, iteration:=0, maxIterations:=krDerivativeBlockTotal)

                Try
                    If krScale = MixedModelKrParameterScale.Covariance Then
                        Dim analyticDiagnostic As String = String.Empty
                        If TryBuildKenwardRogerDerivativeBlocksLmmCovarianceScale(thetaForKr,
                                                                              pRequest.BuildKenwardRogerSecondDerivatives,
                                                                              blocks,
                                                                              analyticDiagnostic) Then
                            AppendInfo(analyticDiagnostic)
                        Else
                            If Not String.IsNullOrWhiteSpace(analyticDiagnostic) Then
                                AppendDebug("KR analytic LMM covariance-scale derivative path not used: " & analyticDiagnostic)
                            End If

                            blocks = BuildKenwardRogerDerivativeBlocksCovarianceScale(thetaForKr,
                                                                  includeSecondDerivatives:=pRequest.BuildKenwardRogerSecondDerivatives)
                        End If
                    Else
                        Dim useMmrmThetaBackTransform As Boolean = (krScale = MixedModelKrParameterScale.MmrmTheta AndAlso
                                                               krMap IsNot Nothing AndAlso
                                                               krMap.RequiresMmrmThetaBackTransform)
                        blocks = BuildKenwardRogerDerivativeBlocks(thetaForKr,
                                               includeSecondDerivatives:=pRequest.BuildKenwardRogerSecondDerivatives,
                                               useMmrmThetaBackTransform:=useMmrmThetaBackTransform)
                    End If
                Finally
                    derivativeBlockStopwatch.Stop()
                    EnsurePerformanceDiagnostics(res).KrDerivativeBlockTimeMs = derivativeBlockStopwatch.Elapsed.TotalMilliseconds
                End Try

                If blocks Is Nothing OrElse blocks.Count = 0 Then
                    res.KenwardRogerStatusMessage = "KR workspace not built: no valid derivative blocks were created."
                    Exit Sub
                End If

                Dim ws As New MixedModelKrWorkspace With {
                        .P = res.P,
                        .K = thetaForKr.Length,
                        .VarBeta = res.VarBeta,
                        .Theta = If(thetaForKr Is Nothing, Nothing, CType(thetaForKr.Clone(), Double())),
                        .ThetaCovariance = thetaCovForKr,
                        .Blocks = blocks,
                        .ParameterScale = krScale,
                        .CovarianceParameterNames = krCovNames,
                        .AdjustmentKind = pRequest.KenwardRogerOptions.Adjustment,
                        .AllowLinearFallback = pRequest.KenwardRogerOptions.AllowLinearFallback,
                        .FiniteDifferenceDiagnostics = If(_krFiniteDifferenceDiagnostics Is Nothing, Nothing, _krFiniteDifferenceDiagnostics.Clone()),
                        .DerivativePatternCache = If(_krDerivativePatternCacheDiagnostics Is Nothing, New MixedModelKrDerivativePatternCacheDiagnostics(), _krDerivativePatternCacheDiagnostics.Clone()),
                        .FiniteDifferenceOptions = CurrentKrFiniteDifferenceOptions().Clone(),
                        .PerformanceDiagnostics = EnsurePerformanceDiagnostics(res),
                        .UsePqrDesignPatternCache = If(pRequest Is Nothing, True, pRequest.Control.UseKrPqrDesignPatternCache),
                        .UsePqrFastFactorization = If(pRequest Is Nothing, True, pRequest.Control.UseKrPqrFastFactorization),
                        .CancellationRequested = AddressOf IsCancellationRequested,
                        .ProgressReporter = If(pRequest Is Nothing, Nothing, pRequest.ProgressReporter)
                    }

                AddKrFiniteDifferenceDiagnosticWarnings(ws)

                Dim diagnostic As String = String.Empty

                ReportProgress("Kenward-Roger P/Q/R aggregation", 98, iteration:=0, maxIterations:=If(blocks Is Nothing, 0, blocks.Count))
                If Not MixedModelKenwardRogerBackend.TryBuildKrMatrices(ws, diagnostic) Then
                    res.KenwardRogerWorkspace = ws
                    res.KenwardRogerStatusMessage = diagnostic
                    SyncKenwardRogerWorkspaceToInferenceWorkspace(res, ws)
                    AppendWarn("KR backend matrix construction failed: " & diagnostic)
                    Exit Sub
                End If

                Dim adjusted(,) As Double = Nothing
                ReportProgress("Kenward-Roger adjusted Var(beta)", 99, message:="Computing adjusted fixed-effect covariance")
                If MixedModelKenwardRogerBackend.TryComputeAdjustedVarBeta(ws, adjusted, diagnostic) Then
                    res.KenwardRogerAdjustedVarBeta = adjusted
                Else
                    res.AddUserWarning("KR covariance adjustment failed: " & diagnostic)
                End If

                If Not String.IsNullOrWhiteSpace(ws.NumericalWarningSummary()) Then
                    res.AddUserWarning(ws.NumericalWarningSummary())
                End If

                res.KenwardRogerWorkspace = ws
                res.KenwardRogerStatusMessage = diagnostic
                SyncKenwardRogerWorkspaceToInferenceWorkspace(res, ws)
                ReportProgress("Kenward-Roger complete", 99, message:=diagnostic)

                AppendInfo("KR derivative workspace built. blocks=" & blocks.Count.ToString() &
                           "; thetaCount=" & thetaForKr.Length.ToString() &
                           "; parameterScale='" & krScale.ToString() & "'" &
                           "; adjustmentRequested='" & ws.AdjustmentKind.ToString() & "'" &
                           "; adjustmentUsed='" & ws.AdjustmentUsed.ToString() & "'" &
                           "; status='" & diagnostic & "'.")

            Catch ex As Exception
                If res IsNot Nothing Then res.KenwardRogerStatusMessage = "KR workspace failed: " & ex.Message
                AppendWarn("PopulateKenwardRogerWorkspace failed: " & ex.ToString())
            Finally
                workspaceStopwatch.Stop()
                If res IsNot Nothing Then
                    EnsurePerformanceDiagnostics(res).KrWorkspaceBuildTimeMs = workspaceStopwatch.Elapsed.TotalMilliseconds
                End If
            End Try
        End Sub


        Private Shared Function EnsurePerformanceDiagnostics(res As MixedModelResult) As MixedModelPerformanceDiagnostics
            If res Is Nothing Then Return New MixedModelPerformanceDiagnostics()
            If res.PerformanceDiagnostics Is Nothing Then res.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()
            Return res.PerformanceDiagnostics
        End Function


        Private Sub AddKrFiniteDifferenceDiagnosticWarnings(ws As MixedModelKrWorkspace)
            If ws Is Nothing OrElse ws.FiniteDifferenceDiagnostics Is Nothing Then Exit Sub

            Dim threshold As Double = ws.FiniteDifferenceWarningThreshold()
            Dim warning As String = ws.FiniteDifferenceDiagnostics.WarningSummary(threshold)
            If Not String.IsNullOrWhiteSpace(warning) Then
                ws.AddNumericalWarning(warning)
            End If

            AppendDebug("KR finite-difference summary: " &
                        ws.FiniteDifferenceDiagnostics.SummaryText(threshold))
        End Sub

        ''' <summary>
        ''' Builds exact LMM KR derivative blocks on the direct covariance-parameter scale when
        ''' the model has supported G/R structures. For direct covariance parameters,
        ''' V_i = Z_i G Z_i' + R_i is linear in each covariance element, so all second
        ''' derivative matrices are exactly zero.
        ''' </summary>
        Private Function TryBuildKenwardRogerDerivativeBlocksLmmCovarianceScale(covarianceTheta() As Double,
                                                                                includeSecondDerivatives As Boolean,
                                                                                ByRef blocks As List(Of MixedModelKrBlock),
                                                                                ByRef diagnostic As String) As Boolean
            blocks = Nothing
            diagnostic = String.Empty

            If covarianceTheta Is Nothing OrElse covarianceTheta.Length = 0 Then
                diagnostic = "covariance theta is empty."
                Return False
            End If

            If pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse Not pRequest.HasRandomEffects() Then
                diagnostic = "request is not an active LMM random-effects fit."
                Return False
            End If

            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG Is Nothing OrElse activeG.IsDegenerateZeroG() Then
                diagnostic = "active G structure is missing or degenerate."
                Return False
            End If

            Dim gCount As Integer = AnalyticLmmCovarianceGParameterCount(activeG, pRequest.Data.Q)
            If gCount <= 0 Then
                diagnostic = "unsupported G structure for analytic LMM covariance-scale KR derivatives: " & activeG.ToString()
                Return False
            End If

            Dim rCount As Integer = AnalyticLmmCovarianceRParameterCount(pRequest.ResidualStruct, pRequest.Data)
            If rCount < 0 Then
                diagnostic = "unsupported R structure for analytic LMM covariance-scale KR derivatives: " &
                             If(pRequest.ResidualStruct Is Nothing, "<null>", pRequest.ResidualStruct.ToString())
                Return False
            End If

            If covarianceTheta.Length <> gCount + rCount Then
                diagnostic = "covariance theta length mismatch for analytic LMM KR derivatives. Expected " &
                             (gCount + rCount).ToString() & ", got " & covarianceTheta.Length.ToString() & "."
                Return False
            End If

            Dim out As New List(Of MixedModelKrBlock)()
            Dim k As Integer = covarianceTheta.Length
            Dim totalBlocks As Integer = If(pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.Data.Blocks Is Nothing, 0, pRequest.Data.Blocks.Count)
            Dim blockIndex As Integer = 0

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                blockIndex += 1
                ReportProgress("Kenward-Roger analytic derivative blocks", 97, iteration:=blockIndex, maxIterations:=totalBlocks, message:=If(block Is Nothing, String.Empty, If(block.SubjectKey, String.Empty)))
                If block Is Nothing OrElse block.X Is Nothing OrElse block.Nobs <= 0 Then Continue For
                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksStarted += 1

                Dim viBase(,) As Double = SafeBuildViForCovarianceTheta(covarianceTheta, block)
                If viBase Is Nothing Then
                    diagnostic = "analytic LMM KR derivative path could not build base V_i for subject '" & block.SubjectKey & "'."
                    Return False
                End If

                Dim chol(,) As Double = Nothing
                Dim cholTrace As String = Nothing
                If Not MixedModelCovariance.TryCholesky(viBase, chol, cholTrace) Then
                    diagnostic = "analytic LMM KR derivative path could not factor base V_i for subject '" & block.SubjectKey & "'. " & If(cholTrace, String.Empty)
                    Return False
                End If

                Dim vinv(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
                If vinv Is Nothing OrElse Not MatrixLooksUsable(vinv, block.Nobs) Then
                    diagnostic = "analytic LMM KR derivative path produced an unusable V_i inverse for subject '" & block.SubjectKey & "'."
                    Return False
                End If

                Dim n As Integer = block.Nobs
                Dim dv(k - 1, n - 1, n - 1) As Double

                For paramIndex As Integer = 0 To k - 1
                    Dim deriv(,) As Double = BuildAnalyticLmmCovarianceDerivativeMatrix(block, paramIndex, gCount, rCount)
                    If deriv Is Nothing Then
                        diagnostic = "analytic LMM KR derivative path could not build derivative for subject '" &
                                     block.SubjectKey & "', parameter " & (paramIndex + 1).ToString() & "."
                        Return False
                    End If

                    CopyMatrixToTensorSlice(dv, paramIndex, deriv)
                    SymmetrizeTensorMatrixSlice(dv, paramIndex)
                Next

                Dim d2v(,,,) As Double = Nothing
                If includeSecondDerivatives Then
                    ' Direct covariance parameters make V_i linear in every supported G/R element.
                    ' The full-KR second-derivative tensor is therefore conformable but exactly zero.
                    ReDim d2v(k - 1, k - 1, n - 1, n - 1)
                End If

                out.Add(New MixedModelKrBlock With {
                    .X = Matrix.CloneMatrix(block.X),
                    .VInv = vinv,
                    .DV = dv,
                    .D2V = d2v
                })

                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksCompleted += 1
            Next

            If out.Count = 0 Then
                diagnostic = "analytic LMM KR derivative path did not create any subject blocks."
                Return False
            End If

            blocks = out
            diagnostic = "KR analytic LMM covariance-scale derivative workspace built. blocks=" & out.Count.ToString() &
                         "; thetaCount=" & k.ToString() & "; secondDerivatives=" & includeSecondDerivatives.ToString() & "."
            Return True
        End Function


        Private Function AnalyticLmmCovarianceGParameterCount(gStruct As MixedModelGStruct, q As Integer) As Integer
            If gStruct Is Nothing Then Return 0
            If TypeOf gStruct Is RandomIntercept Then Return 1
            If TypeOf gStruct Is RandomInterceptSlope Then
                If q < 2 Then Return -1
                Return 3
            End If
            If TypeOf gStruct Is UnstructuredRandomEffects Then
                If q <= 0 Then Return -1
                Return q * (q + 1) \ 2
            End If
            Return -1
        End Function


        Private Function AnalyticLmmCovarianceRParameterCount(rStruct As MixedModelRStruct,
                                                              data As MixedModelBlockData) As Integer
            If rStruct Is Nothing OrElse data Is Nothing Then Return -1
            If TypeOf rStruct Is IdentityR Then Return 1
            If TypeOf rStruct Is DiagonalHeterogeneousR Then Return rStruct.ParamCount(data)
            Return -1
        End Function


        Private Function BuildAnalyticLmmCovarianceDerivativeMatrix(block As MixedModelSubjectBlock,
                                                                    paramIndex As Integer,
                                                                    gCount As Integer,
                                                                    rCount As Integer) As Double(,)
            If block Is Nothing OrElse block.Nobs <= 0 Then Return Nothing

            If paramIndex < gCount Then
                Return BuildAnalyticLmmGDerivativeMatrix(block, paramIndex)
            End If

            Dim rIndex As Integer = paramIndex - gCount
            If rIndex < 0 OrElse rIndex >= rCount Then Return Nothing
            Return BuildAnalyticLmmRDerivativeMatrix(block, rIndex)
        End Function


        Private Function BuildAnalyticLmmGDerivativeMatrix(block As MixedModelSubjectBlock, gParamIndex As Integer) As Double(,)
            Dim z(,) As Double = block.Z
            If z Is Nothing Then Return Nothing

            Dim q As Integer = block.Q
            Dim activeG As MixedModelGStruct = ActiveGStruct()
            If activeG Is Nothing Then Return Nothing

            If TypeOf activeG Is RandomIntercept Then
                If q < 1 OrElse gParamIndex <> 0 Then Return Nothing
                Return OuterProductColumns(z, 0, 0)
            End If

            If TypeOf activeG Is RandomInterceptSlope Then
                If q < 2 Then Return Nothing
                Select Case gParamIndex
                    Case 0
                        Return OuterProductColumns(z, 0, 0)
                    Case 1
                        Return SymmetricCrossOuterProductColumns(z, 0, 1)
                    Case 2
                        Return OuterProductColumns(z, 1, 1)
                    Case Else
                        Return Nothing
                End Select
            End If

            If TypeOf activeG Is UnstructuredRandomEffects Then
                If q <= 0 Then Return Nothing

                Dim k As Integer = 0
                For i As Integer = 0 To q - 1
                    For j As Integer = 0 To i
                        If k = gParamIndex Then
                            If i = j Then Return OuterProductColumns(z, i, i)
                            Return SymmetricCrossOuterProductColumns(z, i, j)
                        End If
                        k += 1
                    Next
                Next
            End If

            Return Nothing
        End Function


        Private Function BuildAnalyticLmmRDerivativeMatrix(block As MixedModelSubjectBlock, rParamIndex As Integer) As Double(,)
            Dim n As Integer = block.Nobs
            Dim out(n - 1, n - 1) As Double

            If TypeOf pRequest.ResidualStruct Is IdentityR Then
                If rParamIndex <> 0 Then Return Nothing
                For i As Integer = 0 To n - 1
                    out(i, i) = 1.0
                Next
                Return out
            End If

            If TypeOf pRequest.ResidualStruct Is DiagonalHeterogeneousR Then
                Dim visitIndex() As Integer = block.VisitIndex
                If visitIndex Is Nothing OrElse visitIndex.Length <> n Then Return Nothing

                For i As Integer = 0 To n - 1
                    If visitIndex(i) = rParamIndex Then out(i, i) = 1.0
                Next
                Return out
            End If

            Return Nothing
        End Function


        Private Function OuterProductColumns(z(,) As Double, colA As Integer, colB As Integer) As Double(,)
            If z Is Nothing Then Return Nothing
            Dim n As Integer = z.GetLength(0)
            If colA < 0 OrElse colB < 0 OrElse colA >= z.GetLength(1) OrElse colB >= z.GetLength(1) Then Return Nothing

            Dim out(n - 1, n - 1) As Double
            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    out(r, c) = z(r, colA) * z(c, colB)
                Next
            Next
            Return out
        End Function


        Private Function SymmetricCrossOuterProductColumns(z(,) As Double, colA As Integer, colB As Integer) As Double(,)
            If z Is Nothing Then Return Nothing
            Dim n As Integer = z.GetLength(0)
            If colA < 0 OrElse colB < 0 OrElse colA >= z.GetLength(1) OrElse colB >= z.GetLength(1) Then Return Nothing

            Dim out(n - 1, n - 1) As Double
            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    out(r, c) = z(r, colA) * z(c, colB) + z(r, colB) * z(c, colA)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Creates subject/block derivative data by finite-differencing V_i(theta).
        ''' </summary>
        Private Function BuildKenwardRogerDerivativeBlocks(theta() As Double,
                                                   Optional includeSecondDerivatives As Boolean = False,
                                                   Optional useMmrmThetaBackTransform As Boolean = False) As List(Of MixedModelKrBlock)
            If theta Is Nothing OrElse theta.Length = 0 Then Return New List(Of MixedModelKrBlock)()

            Dim out As New List(Of MixedModelKrBlock)
            Dim k As Integer = theta.Length
            Dim usePatternCache As Boolean = ShouldUseKrDerivativePatternCache()
            Dim patternCache As Dictionary(Of String, KrPatternDerivativeBundle) = Nothing

            If usePatternCache Then
                patternCache = New Dictionary(Of String, KrPatternDerivativeBundle)(StringComparer.Ordinal)
                If _krDerivativePatternCacheDiagnostics Is Nothing Then _krDerivativePatternCacheDiagnostics = New MixedModelKrDerivativePatternCacheDiagnostics()
                _krDerivativePatternCacheDiagnostics.Enabled = True
            End If

            Dim totalBlocks As Integer = If(pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.Data.Blocks Is Nothing, 0, pRequest.Data.Blocks.Count)
            Dim blockIndex As Integer = 0

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                blockIndex += 1
                ReportProgress("Kenward-Roger derivative blocks", 97, iteration:=blockIndex, maxIterations:=totalBlocks, message:=If(block Is Nothing, String.Empty, If(block.SubjectKey, String.Empty)))
                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksStarted += 1

                Dim patternKey As String = Nothing
                Dim cachedBundle As KrPatternDerivativeBundle = Nothing
                Dim hasCachedBundle As Boolean = False

                If usePatternCache Then
                    patternKey = BuildKrDerivativePatternKey(block)
                    If Not String.IsNullOrWhiteSpace(patternKey) AndAlso patternCache.TryGetValue(patternKey, cachedBundle) Then
                        hasCachedBundle = True
                    End If
                End If

                If hasCachedBundle AndAlso cachedBundle IsNot Nothing Then
                    If _krDerivativePatternCacheDiagnostics IsNot Nothing Then
                        _krDerivativePatternCacheDiagnostics.VInvHits += 1
                        _krDerivativePatternCacheDiagnostics.FirstDerivativeHits += k
                        If includeSecondDerivatives AndAlso cachedBundle.D2V IsNot Nothing Then
                            _krDerivativePatternCacheDiagnostics.SecondDerivativeHits += k * k
                        End If
                    End If

                    out.Add(New MixedModelKrBlock With {
                        .X = Matrix.CloneMatrix(block.X),
                        .VInv = Matrix.CloneMatrix(cachedBundle.VInv),
                        .DV = CloneTensor3D(cachedBundle.DV),
                        .D2V = CloneTensor4D(cachedBundle.D2V)
                    })

                    If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksCompleted += 1
                    Continue For
                End If

                Dim viBase(,) As Double = If(useMmrmThetaBackTransform,
                                              SafeBuildViForMmrmTheta(theta, block),
                                              SafeBuildViForTheta(theta, block))
                If viBase Is Nothing Then
                    If _krDerivativePatternCacheDiagnostics IsNot Nothing AndAlso usePatternCache Then _krDerivativePatternCacheDiagnostics.InvalidBuilds += 1
                    AppendWarn("KR derivative block skipped for subject '" & block.SubjectKey & "': base V_i could not be built.")
                    Continue For
                End If

                Dim n As Integer = viBase.GetLength(0)
                Dim chol(,) As Double = Nothing
                Dim krTrace As String = Nothing

                If Not MixedModelCovariance.TryCholesky(viBase, chol, krTrace) Then
                    If _krDerivativePatternCacheDiagnostics IsNot Nothing AndAlso usePatternCache Then _krDerivativePatternCacheDiagnostics.InvalidBuilds += 1
                    AppendWarn("KR derivative block skipped for subject '" & block.SubjectKey & "': base V_i was not SPD.")
                    Continue For
                End If

                Dim vinv(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
                If Not MatrixLooksUsable(vinv, n) Then
                    If _krDerivativePatternCacheDiagnostics IsNot Nothing AndAlso usePatternCache Then _krDerivativePatternCacheDiagnostics.InvalidBuilds += 1
                    AppendWarn("KR derivative block skipped for subject '" & block.SubjectKey & "': V_i inverse is not usable.")
                    Continue For
                End If

                BeginKrDerivativeViCache(block.SubjectKey)

                Dim dv(k - 1, n - 1, n - 1) As Double

                For paramIndex As Integer = 0 To k - 1
                    Dim deriv(,) As Double = Nothing
                    If TryBuildKrFirstDerivativeMatrix(theta, block, viBase, paramIndex,
                                                       covarianceScale:=False,
                                                       derivative:=deriv,
                                                       useMmrmThetaBackTransform:=useMmrmThetaBackTransform) Then
                        CopyMatrixToTensorSlice(dv, paramIndex, deriv)
                    Else
                        AppendWarn("KR derivative warning: first derivative for subject '" & block.SubjectKey &
                                   "', parameter " & (paramIndex + 1).ToString() &
                                   " could not be computed; a zero derivative slice was used.")
                    End If

                    SymmetrizeTensorMatrixSlice(dv, paramIndex)
                Next

                Dim d2v(,,,) As Double = Nothing
                If includeSecondDerivatives Then
                    d2v = BuildKenwardRogerSecondDerivativeTensor(theta,
                                                                        block,
                                                                        viBase,
                                                                        useMmrmThetaBackTransform:=useMmrmThetaBackTransform)
                End If

                out.Add(New MixedModelKrBlock With {
                    .X = Matrix.CloneMatrix(block.X),
                    .VInv = vinv,
                    .DV = dv,
                    .D2V = d2v
                })
                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksCompleted += 1
                EndKrDerivativeViCache(If(useMmrmThetaBackTransform, "r-mmrm-theta-scale", "theta-scale"))

                If usePatternCache AndAlso Not String.IsNullOrWhiteSpace(patternKey) Then
                    patternCache(patternKey) = New KrPatternDerivativeBundle With {
                        .VInv = Matrix.CloneMatrix(vinv),
                        .DV = CloneTensor3D(dv),
                        .D2V = CloneTensor4D(d2v)
                    }

                    If _krDerivativePatternCacheDiagnostics IsNot Nothing Then
                        _krDerivativePatternCacheDiagnostics.VInvMisses += 1
                        _krDerivativePatternCacheDiagnostics.FirstDerivativeMisses += k
                        If includeSecondDerivatives AndAlso d2v IsNot Nothing Then
                            _krDerivativePatternCacheDiagnostics.SecondDerivativeMisses += k * k
                        End If
                        _krDerivativePatternCacheDiagnostics.PatternCount = Math.Max(_krDerivativePatternCacheDiagnostics.PatternCount, patternCache.Count)
                    End If
                End If
            Next

            Return out
        End Function


        Private Function ShouldUseKrDerivativePatternCache() As Boolean
            If pRequest Is Nothing OrElse pRequest.Data Is Nothing Then Return False
            If pRequest.HasRandomEffects() Then Return False
            If pRequest.ResidualStruct Is Nothing Then Return False
            If Not pRequest.Data.HasVisit Then Return False
            Return True
        End Function


        Private Function BuildKrDerivativePatternKey(block As MixedModelSubjectBlock) As String
            If block Is Nothing OrElse block.Nobs <= 0 Then Return String.Empty

            Dim sb As New System.Text.StringBuilder()
            sb.Append("n=").Append(block.Nobs.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(";")

            Dim visitIndex() As Integer = block.VisitIndex
            If visitIndex IsNot Nothing AndAlso visitIndex.Length = block.Nobs Then
                sb.Append("vi=")
                For i As Integer = 0 To visitIndex.Length - 1
                    If i > 0 Then sb.Append("|"c)
                    sb.Append(visitIndex(i).ToString(System.Globalization.CultureInfo.InvariantCulture))
                Next
                Return sb.ToString()
            End If

            Dim visit() As Double = block.Visit
            If visit IsNot Nothing AndAlso visit.Length = block.Nobs Then
                sb.Append("v=")
                For i As Integer = 0 To visit.Length - 1
                    If i > 0 Then sb.Append("|"c)
                    sb.Append(visit(i).ToString("G17", System.Globalization.CultureInfo.InvariantCulture))
                Next
                Return sb.ToString()
            End If

            Return String.Empty
        End Function


        Private Shared Function CloneTensor3D(source(,,) As Double) As Double(,,)
            If source Is Nothing Then Return Nothing

            Dim n0 As Integer = source.GetLength(0)
            Dim n1 As Integer = source.GetLength(1)
            Dim n2 As Integer = source.GetLength(2)
            Dim cloned(n0 - 1, n1 - 1, n2 - 1) As Double

            For i As Integer = 0 To n0 - 1
                For j As Integer = 0 To n1 - 1
                    For k As Integer = 0 To n2 - 1
                        cloned(i, j, k) = source(i, j, k)
                    Next
                Next
            Next

            Return cloned
        End Function


        Private Shared Function CloneTensor4D(source(,,,) As Double) As Double(,,,)
            If source Is Nothing Then Return Nothing

            Dim n0 As Integer = source.GetLength(0)
            Dim n1 As Integer = source.GetLength(1)
            Dim n2 As Integer = source.GetLength(2)
            Dim n3 As Integer = source.GetLength(3)
            Dim cloned(n0 - 1, n1 - 1, n2 - 1, n3 - 1) As Double

            For i As Integer = 0 To n0 - 1
                For j As Integer = 0 To n1 - 1
                    For r As Integer = 0 To n2 - 1
                        For c As Integer = 0 To n3 - 1
                            cloned(i, j, r, c) = source(i, j, r, c)
                        Next
                    Next
                Next
            Next

            Return cloned
        End Function


        ''' <summary>
        ''' Creates subject/block derivative data by finite-differencing V_i(kappa) on
        ''' the covariance-parameter scale.
        ''' </summary>
        Private Function BuildKenwardRogerDerivativeBlocksCovarianceScale(covarianceTheta() As Double,
                                                                  Optional includeSecondDerivatives As Boolean = False) As List(Of MixedModelKrBlock)
            If covarianceTheta Is Nothing OrElse covarianceTheta.Length = 0 Then Return New List(Of MixedModelKrBlock)()

            Dim out As New List(Of MixedModelKrBlock)
            Dim k As Integer = covarianceTheta.Length
            Dim totalBlocks As Integer = If(pRequest Is Nothing OrElse pRequest.Data Is Nothing OrElse pRequest.Data.Blocks Is Nothing, 0, pRequest.Data.Blocks.Count)
            Dim blockIndex As Integer = 0

            For Each block As MixedModelSubjectBlock In pRequest.Data.Blocks
                ThrowIfCancellationRequested()
                blockIndex += 1
                ReportProgress("Kenward-Roger covariance-scale derivative blocks", 97, iteration:=blockIndex, maxIterations:=totalBlocks, message:=If(block Is Nothing, String.Empty, If(block.SubjectKey, String.Empty)))
                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksStarted += 1
                Dim viBase(,) As Double = SafeBuildViForCovarianceTheta(covarianceTheta, block)
                If viBase Is Nothing Then
                    AppendWarn("KR covariance-scale derivative block skipped for subject '" & block.SubjectKey & "': base V_i could not be built.")
                    Continue For
                End If

                Dim n As Integer = viBase.GetLength(0)
                Dim chol(,) As Double = Nothing
                Dim krTrace As String = Nothing

                If Not MixedModelCovariance.TryCholesky(viBase, chol, krTrace) Then
                    AppendWarn("KR covariance-scale derivative block skipped for subject '" & block.SubjectKey & "': base V_i was not SPD.")
                    Continue For
                End If

                Dim vinv(,) As Double = Global.BESHStatNG.Matrix.Matrix.CholInv(chol)
                If Not MatrixLooksUsable(vinv, n) Then
                    AppendWarn("KR covariance-scale derivative block skipped for subject '" & block.SubjectKey & "': V_i inverse is not usable.")
                    Continue For
                End If

                BeginKrDerivativeViCache(block.SubjectKey)

                Dim dv(k - 1, n - 1, n - 1) As Double

                For paramIndex As Integer = 0 To k - 1
                    Dim deriv(,) As Double = Nothing
                    If TryBuildKrFirstDerivativeMatrix(covarianceTheta, block, viBase, paramIndex,
                                                       covarianceScale:=True, derivative:=deriv) Then
                        CopyMatrixToTensorSlice(dv, paramIndex, deriv)
                    Else
                        AppendWarn("KR covariance-scale derivative warning: first derivative for subject '" & block.SubjectKey &
                                   "', parameter " & (paramIndex + 1).ToString() &
                                   " could not be computed; a zero derivative slice was used.")
                    End If

                    SymmetrizeTensorMatrixSlice(dv, paramIndex)
                Next

                Dim d2v(,,,) As Double = Nothing
                If includeSecondDerivatives Then
                    d2v = BuildKenwardRogerSecondDerivativeTensorCovarianceScale(covarianceTheta, block, viBase)
                End If

                out.Add(New MixedModelKrBlock With {
                        .X = Matrix.CloneMatrix(block.X),
                        .VInv = vinv,
                        .DV = dv,
                        .D2V = d2v
                    })

                If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.BlocksCompleted += 1
                EndKrDerivativeViCache("covariance-scale")
            Next

            Return out
        End Function

        ''' <summary>
        ''' Builds V_i for one subject from covariance-scale kappa.
        ''' </summary>
        Private Function SafeBuildViForCovarianceTheta(covarianceTheta() As Double, block As MixedModelSubjectBlock) As Double(,)
            Try
                Dim optimizerTheta() As Double = Nothing
                Dim msg As String = Nothing

                If Not MixedModelCovarianceParameterScale.TryCovarianceToOptimizerTheta(pRequest, covarianceTheta, optimizerTheta, msg) Then
                    Return Nothing
                End If

                Return SafeBuildViForTheta(optimizerTheta, block)

            Catch
                Return Nothing
            End Try
        End Function


        ''' <summary>
        ''' Builds d2 V_i / d kappa_h d kappa_j on the covariance-parameter scale.
        ''' </summary>
        Private Function BuildKenwardRogerSecondDerivativeTensorCovarianceScale(covarianceTheta() As Double,
                                                                        block As MixedModelSubjectBlock,
                                                                        viBase(,) As Double) As Double(,,,)
            If covarianceTheta Is Nothing OrElse covarianceTheta.Length = 0 Then Return Nothing
            If block Is Nothing OrElse viBase Is Nothing Then Return Nothing

            Dim k As Integer = covarianceTheta.Length
            Dim n As Integer = viBase.GetLength(0)
            Dim out(k - 1, k - 1, n - 1, n - 1) As Double

            For hIndex As Integer = 0 To k - 1
                Dim pure(,) As Double = Nothing
                If TryBuildKrPureSecondDerivativeMatrix(covarianceTheta, block, viBase, hIndex,
                                                        covarianceScale:=True, derivative:=pure) Then
                    CopyMatrixToTensorSlice4(out, hIndex, hIndex, pure)
                    SymmetrizeTensorMatrixSlice4(out, hIndex, hIndex)
                Else
                    AppendWarn("KR covariance-scale derivative warning: pure second derivative for subject '" & block.SubjectKey &
                               "', parameter " & (hIndex + 1).ToString() &
                               " could not be computed; a zero second-derivative slice was used.")
                End If

                For jIndex As Integer = hIndex + 1 To k - 1
                    Dim mixed(,) As Double = Nothing
                    If TryBuildKrMixedSecondDerivativeMatrix(covarianceTheta, block, hIndex, jIndex,
                                                             covarianceScale:=True, derivative:=mixed) Then
                        CopyMatrixToTensorSlice4(out, hIndex, jIndex, mixed)
                        CopyMatrixToTensorSlice4(out, jIndex, hIndex, mixed)
                    Else
                        AppendWarn("KR covariance-scale derivative warning: mixed second derivative for subject '" & block.SubjectKey &
                                   "', parameters " & (hIndex + 1).ToString() & "," & (jIndex + 1).ToString() &
                                   " could not be computed; a zero second-derivative slice was used.")
                        SymmetrizeTensorMatrixSlice4(out, hIndex, jIndex)
                        SymmetrizeTensorMatrixSlice4(out, jIndex, hIndex)
                    End If
                Next
            Next

            Return out
        End Function

        Friend Shared Sub SymmetrizeInPlace(a(,) As Double)
            If a Is Nothing Then Exit Sub
            If a.GetLength(0) <> a.GetLength(1) Then Exit Sub

            Dim n As Integer = a.GetLength(0)
            For i As Integer = 0 To n - 1
                For j As Integer = i + 1 To n - 1
                    Dim v As Double = 0.5 * (a(i, j) + a(j, i))
                    a(i, j) = v
                    a(j, i) = v
                Next
            Next
        End Sub

        ''' <summary>
        ''' Builds V_i(theta) for one subject block and returns Nothing on numerical failure.
        ''' </summary>
        Private Function SafeBuildViForTheta(theta() As Double,
                                     block As MixedModelSubjectBlock) As Double(,)
            Try
                Dim thetaG() As Double = Nothing
                Dim thetaR() As Double = Nothing
                UnpackTheta(theta, thetaG, thetaR)

                Dim trace As String = Nothing
                Dim vi(,) As Double = MixedModelCovariance.BuildVi(block,
                                                           pRequest.Data,
                                                           ActiveGStruct(),
                                                           pRequest.ResidualStruct,
                                                           thetaG,
                                                           thetaR,
                                                           trace)

                If vi Is Nothing Then Return Nothing
                If vi.GetLength(0) <> block.Nobs OrElse vi.GetLength(1) <> block.Nobs Then Return Nothing
                If Not MatrixLooksUsable(vi, block.Nobs) Then Return Nothing

                Return vi

            Catch
                Return Nothing
            End Try
        End Function


        ''' <summary>
        ''' Builds V_i from an R mmrm-style theta vector by converting it back to the
        ''' optimizer theta expected by the residual-covariance builders.
        ''' </summary>
        Private Function SafeBuildViForMmrmTheta(theta() As Double,
                                                 block As MixedModelSubjectBlock) As Double(,)
            Try
                Dim optimizerTheta() As Double = Nothing
                Dim msg As String = Nothing

                If Not MixedModelCovarianceParameterScale.TryMmrmThetaToOptimizerTheta(pRequest,
                                                                                       theta,
                                                                                       optimizerTheta,
                                                                                       msg) Then
                    Return Nothing
                End If

                Return SafeBuildViForTheta(optimizerTheta, block)

            Catch
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Computes finite-difference derivative matrices d Var(beta) / d theta_k.
        ''' </summary>
        Private Function ComputeVarBetaThetaGradientMatricesForSatterthwaite(theta() As Double, p As Integer) As Double(,,)
            Try
                If theta Is Nothing OrElse theta.Length = 0 Then Return Nothing
                If p <= 0 Then Return Nothing

                Dim m As Integer = theta.Length
                Dim out(m - 1, p - 1, p - 1) As Double

                For k As Integer = 0 To m - 1
                    Dim h As Double = GetSatterthwaiteFiniteDifferenceStep(theta(k))

                    Dim tPlus() As Double = CType(theta.Clone(), Double())
                    Dim tMinus() As Double = CType(theta.Clone(), Double())

                    tPlus(k) += h
                    tMinus(k) -= h

                    Dim vbPlus(,) As Double = SafeVarBetaMatrix(tPlus)
                    Dim vbMinus(,) As Double = SafeVarBetaMatrix(tMinus)
                    Dim vbBase(,) As Double = Nothing

                    Dim useCentral As Boolean = MatrixLooksUsable(vbPlus, p) AndAlso MatrixLooksUsable(vbMinus, p)
                    Dim useForward As Boolean = False
                    Dim useBackward As Boolean = False

                    If Not useCentral Then
                        vbBase = SafeVarBetaMatrix(theta)
                        useForward = MatrixLooksUsable(vbPlus, p) AndAlso MatrixLooksUsable(vbBase, p)
                        useBackward = MatrixLooksUsable(vbMinus, p) AndAlso MatrixLooksUsable(vbBase, p)
                    End If

                    For r As Integer = 0 To p - 1
                        For c As Integer = 0 To p - 1
                            Dim deriv As Double = 0.0

                            If useCentral Then
                                deriv = (vbPlus(r, c) - vbMinus(r, c)) / (2.0 * h)
                            ElseIf useForward Then
                                deriv = (vbPlus(r, c) - vbBase(r, c)) / h
                            ElseIf useBackward Then
                                deriv = (vbBase(r, c) - vbMinus(r, c)) / h
                            End If

                            If Not IsFinite(deriv) Then deriv = 0.0
                            out(k, r, c) = deriv
                        Next
                    Next

                    ' Force symmetry for each derivative matrix.
                    For r As Integer = 0 To p - 1
                        For c As Integer = r + 1 To p - 1
                            Dim v As Double = 0.5 * (out(k, r, c) + out(k, c, r))
                            out(k, r, c) = v
                            out(k, c, r) = v
                        Next
                    Next
                Next

                Return out

            Catch ex As Exception
                AppendWarn("ComputeVarBetaThetaGradientMatricesForSatterthwaite failed: " & ex.Message)
                Return Nothing
            End Try
        End Function

        ' KR finite-difference behavior is configured through pRequest.KenwardRogerOptions.FiniteDifferenceOptions.
        ' The helper accessors below preserve the previous defaults when the caller leaves  the option object unset.
        Private Class KrPatternDerivativeBundle
            Public Property VInv As Double(,)
            Public Property DV As Double(,,)
            Public Property D2V As Double(,,,)
        End Class

        Private _krDerivativeViCache As Dictionary(Of String, Double(,)) = Nothing
        Private _krDerivativeViCacheHits As Integer = 0
        Private _krDerivativeViCacheMisses As Integer = 0
        Private _krDerivativeViCacheInvalid As Integer = 0
        Private _krDerivativeViCacheSubjectKey As String = String.Empty
        Private _krFiniteDifferenceDiagnostics As MixedModelKrFiniteDifferenceDiagnostics = Nothing
        Private _krDerivativePatternCacheDiagnostics As MixedModelKrDerivativePatternCacheDiagnostics = Nothing

        Private Function TryBuildKrFirstDerivativeMatrix(parameters() As Double,
                                                         block As MixedModelSubjectBlock,
                                                         viBase(,) As Double,
                                                         paramIndex As Integer,
                                                         covarianceScale As Boolean,
                                                         ByRef derivative(,) As Double,
                                                         Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            derivative = Nothing
            If parameters Is Nothing OrElse block Is Nothing OrElse viBase Is Nothing Then Return False

            Dim n As Integer = viBase.GetLength(0)
            Dim baseStep As Double = GetKrFiniteDifferenceStep(parameters(paramIndex), secondDerivative:=False)
            Dim fallback(,) As Double = Nothing
            Dim fallbackKind As String = String.Empty
            Dim fallbackStep As Double = Double.NaN

            For attempt As Integer = 0 To KrFdMaxStepHalvings()
                Dim stepSize As Double = ScaleStepByHalving(baseStep, attempt)

                Dim viPlus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, stepSize, covarianceScale, useMmrmThetaBackTransform)
                Dim viMinus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, -stepSize, covarianceScale, useMmrmThetaBackTransform)

                Dim havePlus As Boolean = MatrixLooksUsable(viPlus, n)
                Dim haveMinus As Boolean = MatrixLooksUsable(viMinus, n)

                If havePlus AndAlso haveMinus Then
                    Dim coarse(,) As Double = CentralFirstDerivative(viPlus, viMinus, stepSize)
                    Dim refined(,) As Double = Nothing

                    If KrFdUseRichardsonRefinement() AndAlso TryBuildRichardsonFirstDerivative(parameters,
                                                         block,
                                                         paramIndex,
                                                         stepSize,
                                                         covarianceScale,
                                                         coarse,
                                                         refined,
                                                         useMmrmThetaBackTransform) Then
                        derivative = refined
                    Else
                        derivative = coarse
                    End If

                    SymmetrizeInPlace(derivative)
                    If MatrixLooksUsable(derivative, n) Then
                        If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                            _krFiniteDifferenceDiagnostics.FirstDerivativeCentralCount += 1
                            _krFiniteDifferenceDiagnostics.RecordStepHalving(attempt)
                        End If
                        Return True
                    End If
                    Return False
                End If

                If havePlus AndAlso MatrixLooksUsable(viBase, n) Then
                    fallback = ForwardFirstDerivative(viPlus, viBase, stepSize)
                    fallbackKind = "forward"
                    fallbackStep = stepSize
                ElseIf haveMinus AndAlso MatrixLooksUsable(viBase, n) Then
                    fallback = BackwardFirstDerivative(viBase, viMinus, stepSize)
                    fallbackKind = "backward"
                    fallbackStep = stepSize
                End If
            Next

            If KrFdAllowOneSidedFirstDerivativeFallback() AndAlso MatrixLooksUsable(fallback, n) Then
                derivative = fallback
                SymmetrizeInPlace(derivative)
                If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                    _krFiniteDifferenceDiagnostics.FirstDerivativeOneSidedFallbackCount += 1
                End If
                AppendWarn("KR derivative warning: first derivative for subject '" & block.SubjectKey &
                           "', parameter " & (paramIndex + 1).ToString() &
                           " used a " & fallbackKind & " one-sided finite difference after step halving (step=" &
                           fallbackStep.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) & ").")
                Return True
            End If

            If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.FirstDerivativeFailedCount += 1
            Return False
        End Function


        Private Function TryBuildKrPureSecondDerivativeMatrix(parameters() As Double,
                                                             block As MixedModelSubjectBlock,
                                                             viBase(,) As Double,
                                                             paramIndex As Integer,
                                                             covarianceScale As Boolean,
                                                             ByRef derivative(,) As Double,
                                                             Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            derivative = Nothing
            If parameters Is Nothing OrElse block Is Nothing OrElse viBase Is Nothing Then Return False

            Dim n As Integer = viBase.GetLength(0)
            Dim baseStep As Double = GetKrFiniteDifferenceStep(parameters(paramIndex), secondDerivative:=True)

            For attempt As Integer = 0 To KrFdMaxStepHalvings()
                Dim stepSize As Double = ScaleStepByHalving(baseStep, attempt)

                Dim viPlus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, stepSize, covarianceScale, useMmrmThetaBackTransform)
                Dim viMinus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, -stepSize, covarianceScale, useMmrmThetaBackTransform)

                If MatrixLooksUsable(viPlus, n) AndAlso MatrixLooksUsable(viMinus, n) Then
                    Dim coarse(,) As Double = CentralPureSecondDerivative(viPlus, viBase, viMinus, stepSize)
                    Dim refined(,) As Double = Nothing

                    If KrFdUseRichardsonRefinement() AndAlso
                        TryBuildRichardsonPureSecondDerivative(parameters, block, viBase, paramIndex, stepSize,
                                                             covarianceScale, coarse, refined, useMmrmThetaBackTransform) Then
                        derivative = refined
                    Else
                        derivative = coarse
                    End If

                    SymmetrizeInPlace(derivative)
                    If MatrixLooksUsable(derivative, n) Then
                        If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                            _krFiniteDifferenceDiagnostics.PureSecondDerivativeCentralCount += 1
                            _krFiniteDifferenceDiagnostics.RecordStepHalving(attempt)
                        End If
                        Return True
                    End If
                    Return False
                End If
            Next

            If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.SecondDerivativeFailedCount += 1
            Return False
        End Function


        Private Function TryBuildKrMixedSecondDerivativeMatrix(parameters() As Double,
                                                              block As MixedModelSubjectBlock,
                                                              hIndex As Integer,
                                                              jIndex As Integer,
                                                              covarianceScale As Boolean,
                                                              ByRef derivative(,) As Double,
                                                              Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            derivative = Nothing
            If parameters Is Nothing OrElse block Is Nothing Then Return False

            Dim n As Integer = block.Nobs
            Dim hBase As Double = GetKrFiniteDifferenceStep(parameters(hIndex), secondDerivative:=True)
            Dim jBase As Double = GetKrFiniteDifferenceStep(parameters(jIndex), secondDerivative:=True)

            For attempt As Integer = 0 To KrFdMaxStepHalvings()
                Dim hStep As Double = ScaleStepByHalving(hBase, attempt)
                Dim jStep As Double = ScaleStepByHalving(jBase, attempt)

                Dim vPP(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, hStep, jIndex, jStep, covarianceScale, useMmrmThetaBackTransform)
                Dim vPM(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, hStep, jIndex, -jStep, covarianceScale, useMmrmThetaBackTransform)
                Dim vMP(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, -hStep, jIndex, jStep, covarianceScale, useMmrmThetaBackTransform)
                Dim vMM(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, -hStep, jIndex, -jStep, covarianceScale, useMmrmThetaBackTransform)

                If MatrixLooksUsable(vPP, n) AndAlso MatrixLooksUsable(vPM, n) AndAlso
                   MatrixLooksUsable(vMP, n) AndAlso MatrixLooksUsable(vMM, n) Then

                    Dim coarse(,) As Double = CentralMixedSecondDerivative(vPP, vPM, vMP, vMM, hStep, jStep)
                    Dim refined(,) As Double = Nothing

                    If KrFdUseRichardsonRefinement() AndAlso
                        TryBuildRichardsonMixedSecondDerivative(parameters, block, hIndex, jIndex, hStep,
                                                              jStep, covarianceScale, coarse, refined, useMmrmThetaBackTransform) Then
                        derivative = refined
                    Else
                        derivative = coarse
                    End If

                    SymmetrizeInPlace(derivative)
                    If MatrixLooksUsable(derivative, n) Then
                        If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                            _krFiniteDifferenceDiagnostics.MixedSecondDerivativeCentralCount += 1
                            _krFiniteDifferenceDiagnostics.RecordStepHalving(attempt)
                        End If
                        Return True
                    End If
                    Return False
                End If
            Next

            If _krFiniteDifferenceDiagnostics IsNot Nothing Then _krFiniteDifferenceDiagnostics.SecondDerivativeFailedCount += 1
            Return False
        End Function


        Private Function TryBuildRichardsonFirstDerivative(parameters() As Double,
                                                           block As MixedModelSubjectBlock,
                                                           paramIndex As Integer,
                                                           coarseStep As Double,
                                                           covarianceScale As Boolean,
                                                           coarse(,) As Double,
                                                           ByRef refined(,) As Double,
                                                           Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            refined = Nothing
            If coarse Is Nothing Then Return False

            Dim n As Integer = coarse.GetLength(0)
            Dim halfStep As Double = 0.5 * coarseStep

            Dim viPlus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, halfStep, covarianceScale, useMmrmThetaBackTransform)
            Dim viMinus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, -halfStep, covarianceScale, useMmrmThetaBackTransform)
            If Not (MatrixLooksUsable(viPlus, n) AndAlso MatrixLooksUsable(viMinus, n)) Then Return False

            Dim fine(,) As Double = CentralFirstDerivative(viPlus, viMinus, halfStep)
            refined = RichardsonRefineCentral(coarse, fine)
            WarnIfRichardsonUnstable("first derivative", block, paramIndex, -1, coarse, fine)
            Return MatrixLooksUsable(refined, n)
        End Function


        Private Function TryBuildRichardsonPureSecondDerivative(parameters() As Double,
                                                               block As MixedModelSubjectBlock,
                                                               viBase(,) As Double,
                                                               paramIndex As Integer,
                                                               coarseStep As Double,
                                                               covarianceScale As Boolean,
                                                               coarse(,) As Double,
                                                               ByRef refined(,) As Double,
                                                               Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            refined = Nothing
            If coarse Is Nothing OrElse viBase Is Nothing Then Return False

            Dim n As Integer = coarse.GetLength(0)
            Dim halfStep As Double = 0.5 * coarseStep

            Dim viPlus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, halfStep, covarianceScale, useMmrmThetaBackTransform)
            Dim viMinus(,) As Double = BuildPerturbedKrVi(parameters, block, paramIndex, -halfStep, covarianceScale, useMmrmThetaBackTransform)
            If Not (MatrixLooksUsable(viPlus, n) AndAlso MatrixLooksUsable(viMinus, n)) Then Return False

            Dim fine(,) As Double = CentralPureSecondDerivative(viPlus, viBase, viMinus, halfStep)
            refined = RichardsonRefineCentral(coarse, fine)
            WarnIfRichardsonUnstable("pure second derivative", block, paramIndex, paramIndex, coarse, fine)
            Return MatrixLooksUsable(refined, n)
        End Function


        Private Function TryBuildRichardsonMixedSecondDerivative(parameters() As Double,
                                                                block As MixedModelSubjectBlock,
                                                                hIndex As Integer,
                                                                jIndex As Integer,
                                                                hStep As Double,
                                                                jStep As Double,
                                                                covarianceScale As Boolean,
                                                                coarse(,) As Double,
                                                                ByRef refined(,) As Double,
                                                                Optional useMmrmThetaBackTransform As Boolean = False) As Boolean
            refined = Nothing
            If coarse Is Nothing Then Return False

            Dim n As Integer = coarse.GetLength(0)
            Dim hh As Double = 0.5 * hStep
            Dim hj As Double = 0.5 * jStep

            Dim vPP(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, hh, jIndex, hj, covarianceScale, useMmrmThetaBackTransform)
            Dim vPM(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, hh, jIndex, -hj, covarianceScale, useMmrmThetaBackTransform)
            Dim vMP(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, -hh, jIndex, hj, covarianceScale, useMmrmThetaBackTransform)
            Dim vMM(,) As Double = BuildDoublePerturbedKrVi(parameters, block, hIndex, -hh, jIndex, -hj, covarianceScale, useMmrmThetaBackTransform)

            If Not (MatrixLooksUsable(vPP, n) AndAlso MatrixLooksUsable(vPM, n) AndAlso
                    MatrixLooksUsable(vMP, n) AndAlso MatrixLooksUsable(vMM, n)) Then Return False

            Dim fine(,) As Double = CentralMixedSecondDerivative(vPP, vPM, vMP, vMM, hh, hj)
            refined = RichardsonRefineCentral(coarse, fine)
            WarnIfRichardsonUnstable("mixed second derivative", block, hIndex, jIndex, coarse, fine)
            Return MatrixLooksUsable(refined, n)
        End Function


        Private Function BuildPerturbedKrVi(parameters() As Double,
                                            block As MixedModelSubjectBlock,
                                            paramIndex As Integer,
                                            delta As Double,
                                            covarianceScale As Boolean,
                                            Optional useMmrmThetaBackTransform As Boolean = False) As Double(,)
            Dim perturbed() As Double = CType(parameters.Clone(), Double())
            perturbed(paramIndex) += delta
            Return BuildKrViFromParameterVector(perturbed, block, covarianceScale, useMmrmThetaBackTransform)
        End Function


        Private Function BuildDoublePerturbedKrVi(parameters() As Double,
                                                  block As MixedModelSubjectBlock,
                                                  hIndex As Integer,
                                                  hDelta As Double,
                                                  jIndex As Integer,
                                                  jDelta As Double,
                                                  covarianceScale As Boolean,
                                                  Optional useMmrmThetaBackTransform As Boolean = False) As Double(,)
            Dim perturbed() As Double = CType(parameters.Clone(), Double())
            perturbed(hIndex) += hDelta
            perturbed(jIndex) += jDelta
            Return BuildKrViFromParameterVector(perturbed, block, covarianceScale, useMmrmThetaBackTransform)
        End Function


        Private Function BuildKrViFromParameterVector(parameters() As Double,
                                                           block As MixedModelSubjectBlock,
                                                           covarianceScale As Boolean,
                                                           Optional useMmrmThetaBackTransform As Boolean = False) As Double(,)
            Dim directBuild As Func(Of Double(,)) = Function()
                                                        If covarianceScale Then
                                                            Return SafeBuildViForCovarianceTheta(parameters, block)
                                                        End If

                                                        If useMmrmThetaBackTransform Then
                                                            Return SafeBuildViForMmrmTheta(parameters, block)
                                                        End If

                                                        Return SafeBuildViForTheta(parameters, block)
                                                    End Function

            If _krDerivativeViCache Is Nothing Then Return directBuild.Invoke()

            Dim key As String = BuildKrDerivativeViCacheKey(parameters, covarianceScale, useMmrmThetaBackTransform)
            Dim cached(,) As Double = Nothing

            If _krDerivativeViCache.TryGetValue(key, cached) Then
                _krDerivativeViCacheHits += 1
                Return cached
            End If

            cached = directBuild.Invoke()
            If cached Is Nothing Then
                _krDerivativeViCacheInvalid += 1
                Return Nothing
            End If

            _krDerivativeViCacheMisses += 1
            _krDerivativeViCache(key) = cached
            Return cached
        End Function

        Private Sub BeginKrDerivativeViCache(subjectKey As String)
            _krDerivativeViCache = New Dictionary(Of String, Double(,))(StringComparer.Ordinal)
            _krDerivativeViCacheHits = 0
            _krDerivativeViCacheMisses = 0
            _krDerivativeViCacheInvalid = 0
            _krDerivativeViCacheSubjectKey = If(subjectKey, String.Empty)
        End Sub


        Private Sub EndKrDerivativeViCache(scaleLabel As String)
            If _krDerivativeViCache Is Nothing Then Exit Sub

            If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                _krFiniteDifferenceDiagnostics.PerturbedViCacheEntries += _krDerivativeViCache.Count
                _krFiniteDifferenceDiagnostics.PerturbedViCacheHits += _krDerivativeViCacheHits
                _krFiniteDifferenceDiagnostics.PerturbedViCacheMisses += _krDerivativeViCacheMisses
                _krFiniteDifferenceDiagnostics.PerturbedViCacheInvalidBuilds += _krDerivativeViCacheInvalid
            End If

            If KrFdEmitPerturbedViCacheDiagnostics() AndAlso (_krDerivativeViCacheHits > 0 OrElse _krDerivativeViCacheInvalid > 0) Then
                AppendDebug("KR derivative V_i cache (" & If(scaleLabel, String.Empty) & ") for subject '" &
                            _krDerivativeViCacheSubjectKey & "': entries=" &
                            _krDerivativeViCache.Count.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                            "; hits=" & _krDerivativeViCacheHits.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                            "; misses=" & _krDerivativeViCacheMisses.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                            "; invalidBuilds=" & _krDerivativeViCacheInvalid.ToString(System.Globalization.CultureInfo.InvariantCulture) & ".")
            End If

            _krDerivativeViCache.Clear()
            _krDerivativeViCache = Nothing
            _krDerivativeViCacheHits = 0
            _krDerivativeViCacheMisses = 0
            _krDerivativeViCacheInvalid = 0
            _krDerivativeViCacheSubjectKey = String.Empty
        End Sub

        Private Function BuildKrDerivativeViCacheKey(parameters() As Double,
                                                          covarianceScale As Boolean,
                                                          Optional useMmrmThetaBackTransform As Boolean = False) As String
            Dim prefix As String = If(covarianceScale, "C", If(useMmrmThetaBackTransform, "M", "T"))
            If parameters Is Nothing Then Return prefix & ":<null>"

            Dim sb As New System.Text.StringBuilder()
            sb.Append(prefix).Append(":"c)

            For i As Integer = 0 To parameters.Length - 1
                If i > 0 Then sb.Append("|"c)
                sb.Append(parameters(i).ToString("G17", System.Globalization.CultureInfo.InvariantCulture))
            Next

            Return sb.ToString()
        End Function

        Private Function CurrentKrFiniteDifferenceOptions() As MixedModelKenwardRogerFiniteDifferenceOptions
            Dim opts As MixedModelKenwardRogerFiniteDifferenceOptions = Nothing

            If pRequest IsNot Nothing AndAlso pRequest.KenwardRogerOptions IsNot Nothing Then
                opts = pRequest.KenwardRogerOptions.FiniteDifferenceOptions
            End If

            If opts Is Nothing Then opts = MixedModelKenwardRogerFiniteDifferenceOptions.CreateDefault()
            opts.Validate()
            Return opts
        End Function


        Private Function KrFdFirstStepScale() As Double
            Return CurrentKrFiniteDifferenceOptions().FirstDerivativeStepScale
        End Function


        Private Function KrFdSecondStepScale() As Double
            Return CurrentKrFiniteDifferenceOptions().SecondDerivativeStepScale
        End Function


        Private Function KrFdMinimumStep() As Double
            Return CurrentKrFiniteDifferenceOptions().MinimumStep
        End Function


        Private Function KrFdMaximumStep() As Double
            Return CurrentKrFiniteDifferenceOptions().MaximumStep
        End Function


        Private Function KrFdMaxStepHalvings() As Integer
            Return CurrentKrFiniteDifferenceOptions().MaxStepHalvings
        End Function


        Private Function KrFdUseRichardsonRefinement() As Boolean
            Return CurrentKrFiniteDifferenceOptions().UseRichardsonRefinement
        End Function


        Private Function KrFdAllowOneSidedFirstDerivativeFallback() As Boolean
            Return CurrentKrFiniteDifferenceOptions().AllowOneSidedFirstDerivativeFallback
        End Function


        Private Function KrFdRichardsonWarnRel() As Double
            Return CurrentKrFiniteDifferenceOptions().RichardsonWarningRelativeTolerance
        End Function


        Private Function KrFdEmitPerturbedViCacheDiagnostics() As Boolean
            Return CurrentKrFiniteDifferenceOptions().EmitPerturbedViCacheDiagnostics
        End Function

        Private Function GetKrFiniteDifferenceStep(value As Double, secondDerivative As Boolean) As Double
            Dim scale As Double = If(secondDerivative, KrFdSecondStepScale(), KrFdFirstStepScale())
            Dim reference As Double = If(IsFinite(value), Math.Max(Math.Abs(value), 1.0), 1.0)
            Dim stepSize As Double = reference * scale
            stepSize = Math.Max(stepSize, KrFdMinimumStep())
            stepSize = Math.Min(stepSize, KrFdMaximumStep())

            If Not IsFinite(stepSize) OrElse stepSize <= 0.0 Then
                stepSize = If(secondDerivative, KrFdSecondStepScale(), KrFdFirstStepScale())
            End If

            Return stepSize
        End Function


        Private Function ScaleStepByHalving(baseStep As Double, attempt As Integer) As Double
            Dim stepSize As Double = baseStep
            For i As Integer = 1 To attempt
                stepSize *= 0.5
            Next
            If stepSize < KrFdMinimumStep() Then stepSize = KrFdMinimumStep()
            Return stepSize
        End Function


        Private Function CentralFirstDerivative(plus(,) As Double, minus(,) As Double, stepSize As Double) As Double(,)
            Dim n As Integer = plus.GetLength(0)
            Dim out(n - 1, n - 1) As Double
            Dim denom As Double = 2.0 * stepSize

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = (plus(r, c) - minus(r, c)) / denom
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Function ForwardFirstDerivative(plus(,) As Double, baseMatrix(,) As Double, stepSize As Double) As Double(,)
            Dim n As Integer = plus.GetLength(0)
            Dim out(n - 1, n - 1) As Double

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = (plus(r, c) - baseMatrix(r, c)) / stepSize
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Function BackwardFirstDerivative(baseMatrix(,) As Double, minus(,) As Double, stepSize As Double) As Double(,)
            Dim n As Integer = baseMatrix.GetLength(0)
            Dim out(n - 1, n - 1) As Double

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = (baseMatrix(r, c) - minus(r, c)) / stepSize
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Function CentralPureSecondDerivative(plus(,) As Double,
                                                     baseMatrix(,) As Double,
                                                     minus(,) As Double,
                                                     stepSize As Double) As Double(,)
            Dim n As Integer = baseMatrix.GetLength(0)
            Dim out(n - 1, n - 1) As Double
            Dim denom As Double = stepSize * stepSize

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = (plus(r, c) - 2.0 * baseMatrix(r, c) + minus(r, c)) / denom
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Function CentralMixedSecondDerivative(vPP(,) As Double,
                                                      vPM(,) As Double,
                                                      vMP(,) As Double,
                                                      vMM(,) As Double,
                                                      hStep As Double,
                                                      jStep As Double) As Double(,)
            Dim n As Integer = vPP.GetLength(0)
            Dim out(n - 1, n - 1) As Double
            Dim denom As Double = 4.0 * hStep * jStep

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = (vPP(r, c) - vPM(r, c) - vMP(r, c) + vMM(r, c)) / denom
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Function RichardsonRefineCentral(coarse(,) As Double, fine(,) As Double) As Double(,)
            Dim n As Integer = coarse.GetLength(0)
            Dim out(n - 1, n - 1) As Double

            For r As Integer = 0 To n - 1
                For c As Integer = 0 To n - 1
                    Dim v As Double = fine(r, c) + (fine(r, c) - coarse(r, c)) / 3.0
                    out(r, c) = If(IsFinite(v), v, 0.0)
                Next
            Next

            SymmetrizeInPlace(out)
            Return out
        End Function


        Private Sub WarnIfRichardsonUnstable(kind As String,
                                             block As MixedModelSubjectBlock,
                                             hIndex As Integer,
                                             jIndex As Integer,
                                             coarse(,) As Double,
                                             fine(,) As Double)
            Dim rel As Double = MatrixRelativeDifference(coarse, fine)
            If _krFiniteDifferenceDiagnostics IsNot Nothing Then
                If kind IsNot Nothing AndAlso kind.IndexOf("first", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    _krFiniteDifferenceDiagnostics.RecordFirstDerivativeRichardson(rel)
                Else
                    _krFiniteDifferenceDiagnostics.RecordSecondDerivativeRichardson(rel)
                End If
            End If
            If rel <= KrFdRichardsonWarnRel() Then Exit Sub

            Dim paramText As String = If(jIndex < 0,
                                         (hIndex + 1).ToString(),
                                         (hIndex + 1).ToString() & "," & (jIndex + 1).ToString())

            AppendWarn("KR derivative warning: Richardson refinement changed " & kind &
                       " for subject '" & If(block Is Nothing, "", block.SubjectKey) &
                       "', parameter(s) " & paramText &
                       " by relative amount " & rel.ToString("G4", System.Globalization.CultureInfo.InvariantCulture) & ".")
        End Sub


        Private Function MatrixRelativeDifference(a(,) As Double, b(,) As Double) As Double
            If a Is Nothing OrElse b Is Nothing Then Return Double.PositiveInfinity
            If a.GetLength(0) <> b.GetLength(0) OrElse a.GetLength(1) <> b.GetLength(1) Then Return Double.PositiveInfinity

            Dim diffSq As Double = 0.0
            Dim baseSq As Double = 0.0

            For r As Integer = 0 To a.GetLength(0) - 1
                For c As Integer = 0 To a.GetLength(1) - 1
                    Dim d As Double = b(r, c) - a(r, c)
                    If IsFinite(d) Then diffSq += d * d
                    If IsFinite(b(r, c)) Then baseSq += b(r, c) * b(r, c)
                Next
            Next

            Return Math.Sqrt(diffSq) / Math.Max(1.0, Math.Sqrt(baseSq))
        End Function


        Private Sub CopyMatrixToTensorSlice(tensor(,,) As Double, h As Integer, matrix(,) As Double)
            If tensor Is Nothing OrElse matrix Is Nothing Then Exit Sub

            For r As Integer = 0 To matrix.GetLength(0) - 1
                For c As Integer = 0 To matrix.GetLength(1) - 1
                    tensor(h, r, c) = matrix(r, c)
                Next
            Next
        End Sub


        Private Sub CopyMatrixToTensorSlice4(tensor(,,,) As Double, h As Integer, j As Integer, matrix(,) As Double)
            If tensor Is Nothing OrElse matrix Is Nothing Then Exit Sub

            For r As Integer = 0 To matrix.GetLength(0) - 1
                For c As Integer = 0 To matrix.GetLength(1) - 1
                    tensor(h, j, r, c) = matrix(r, c)
                Next
            Next
        End Sub

        Private Function SafeVarBetaMatrix(theta() As Double) As Double(,)
            Try
                Dim ev As MixedModelProfileEvaluation = EvaluateProfileCriterion(theta, throwOnFailure:=False, collectTrace:=False)
                If Not ev.Success OrElse ev.VarBeta Is Nothing Then Return Nothing
                Return ev.VarBeta
            Catch
                Return Nothing
            End Try
        End Function


        Private Shared Function MatrixLooksUsable(a(,) As Double, expectedDim As Integer) As Boolean
            If a Is Nothing Then Return False
            If a.GetLength(0) <> expectedDim OrElse a.GetLength(1) <> expectedDim Then Return False

            For r As Integer = 0 To expectedDim - 1
                For c As Integer = 0 To expectedDim - 1
                    If Double.IsNaN(a(r, c)) OrElse Double.IsInfinity(a(r, c)) Then Return False
                Next
            Next

            Return True
        End Function

        ''' <summary>
        ''' Mirrors the internal KR derivative workspace into the generic inference workspace
        ''' so future linear-combination inference has one common source for Satterthwaite/KR
        ''' ingredients.
        ''' </summary>
        Private Sub SyncKenwardRogerWorkspaceToInferenceWorkspace(res As MixedModelResult, kr As MixedModelKrWorkspace)
            If res Is Nothing OrElse kr Is Nothing Then Exit Sub

            If res.InferenceWorkspace Is Nothing Then
                res.InferenceWorkspace = New regression.MixedModelInferenceWorkspace()
            End If

            res.InferenceWorkspace.P = If(res.P > 0, res.P, kr.P)
            res.InferenceWorkspace.K = kr.K

            If res.InferenceWorkspace.VarBeta Is Nothing Then
                res.InferenceWorkspace.VarBeta = If(res.VarBeta Is Nothing, kr.VarBeta, res.VarBeta)
            End If

            If res.InferenceWorkspace.ThetaCovariance Is Nothing Then
                res.InferenceWorkspace.ThetaCovariance = kr.ThetaCovariance
            End If

            res.InferenceWorkspace.KR_P = kr.Pmats
            res.InferenceWorkspace.KR_Q = kr.Qmats
            res.InferenceWorkspace.KR_R = kr.Rmats

            If kr.AdjustedVarBeta IsNot Nothing Then
                res.InferenceWorkspace.AdjustedVarBeta = kr.AdjustedVarBeta
            End If

            res.KenwardRogerParameterScale = kr.ParameterScale
            res.KenwardRogerCovarianceParameterNames = kr.CovarianceParameterNames
        End Sub

        ''' <summary>
        ''' Builds the second-derivative tensor d2V_i/dtheta_h dtheta_j for one subject block.
        ''' </summary>
        Private Function BuildKenwardRogerSecondDerivativeTensor(theta() As Double,
                                                                  block As MixedModelSubjectBlock,
                                                                  viBase(,) As Double,
                                                                  Optional useMmrmThetaBackTransform As Boolean = False) As Double(,,,)
            If theta Is Nothing OrElse theta.Length = 0 Then Return Nothing
            If block Is Nothing OrElse viBase Is Nothing Then Return Nothing

            Dim k As Integer = theta.Length
            Dim n As Integer = viBase.GetLength(0)
            Dim out(k - 1, k - 1, n - 1, n - 1) As Double

            For hIndex As Integer = 0 To k - 1
                Dim pure(,) As Double = Nothing
                If TryBuildKrPureSecondDerivativeMatrix(theta, block, viBase, hIndex,
                                                        covarianceScale:=False,
                                                        derivative:=pure,
                                                        useMmrmThetaBackTransform:=useMmrmThetaBackTransform) Then
                    CopyMatrixToTensorSlice4(out, hIndex, hIndex, pure)
                    SymmetrizeTensorMatrixSlice4(out, hIndex, hIndex)
                Else
                    AppendWarn("KR derivative warning: pure second derivative for subject '" & block.SubjectKey &
                               "', parameter " & (hIndex + 1).ToString() &
                               " could not be computed; a zero second-derivative slice was used.")
                End If

                ' Mixed second derivatives.
                For jIndex As Integer = hIndex + 1 To k - 1
                    Dim mixed(,) As Double = Nothing
                    If TryBuildKrMixedSecondDerivativeMatrix(theta, block, hIndex, jIndex,
                                                             covarianceScale:=False,
                                                             derivative:=mixed,
                                                             useMmrmThetaBackTransform:=useMmrmThetaBackTransform) Then
                        CopyMatrixToTensorSlice4(out, hIndex, jIndex, mixed)
                        CopyMatrixToTensorSlice4(out, jIndex, hIndex, mixed)

                        SymmetrizeTensorMatrixSlice4(out, hIndex, jIndex)
                        SymmetrizeTensorMatrixSlice4(out, jIndex, hIndex)
                    Else
                        AppendWarn("KR derivative warning: mixed second derivative for subject '" & block.SubjectKey &
                                   "', parameters " & (hIndex + 1).ToString() & "," & (jIndex + 1).ToString() &
                                   " could not be computed; a zero second-derivative slice was used.")
                    End If
                Next
            Next

            Return out
        End Function

        ''' <summary>
        ''' Symmetrizes one matrix slice of a 3D tensor with dimensions k,n,n.
        ''' </summary>
        Private Shared Sub SymmetrizeTensorMatrixSlice(a(,,) As Double, h As Integer)
            If a Is Nothing Then Exit Sub

            Dim n As Integer = a.GetLength(1)
            If a.GetLength(2) <> n Then Exit Sub

            For r As Integer = 0 To n - 1
                For c As Integer = r + 1 To n - 1
                    Dim v As Double = 0.5 * (a(h, r, c) + a(h, c, r))
                    a(h, r, c) = v
                    a(h, c, r) = v
                Next
            Next
        End Sub


        ''' <summary>
        ''' Symmetrizes one matrix slice of a 4D tensor with dimensions k,k,n,n.
        ''' </summary>
        Private Shared Sub SymmetrizeTensorMatrixSlice4(a(,,,) As Double, h As Integer, j As Integer)
            If a Is Nothing Then Exit Sub

            Dim n As Integer = a.GetLength(2)
            If a.GetLength(3) <> n Then Exit Sub

            For r As Integer = 0 To n - 1
                For c As Integer = r + 1 To n - 1
                    Dim v As Double = 0.5 * (a(h, j, r, c) + a(h, j, c, r))
                    a(h, j, r, c) = v
                    a(h, j, c, r) = v
                Next
            Next
        End Sub

    End Class

End Namespace