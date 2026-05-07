Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Convenience wrapper for fitting mixed models for repeated measures (MMRM) with the
    ''' shared <see cref="MixedModelEngine"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' MMRM is handled as an R-side-only Gaussian subject-block model:
    ''' <c>y_i ~ N(X_i beta, R_i)</c>.  In the shared engine this is represented by the same
    ''' marginal covariance formula used for LMM,
    ''' <c>V_i = Z_i G Z_i' + R_i</c>, with <c>Z_i</c> absent and no active G-side contribution.
    ''' </para>
    ''' <para>
    ''' This wrapper does not duplicate the likelihood.  It configures and validates the request so that
    ''' the generic <see cref="MixedModelEngine"/> runs the MMRM path.  This is intentional: covariance
    ''' structures such as unstructured, heterogeneous compound symmetry, and heterogeneous AR(1) should
    ''' be tested once in the shared engine and reused by both MMRM and future mixed-model variants.
    ''' </para>
    ''' <para>
    ''' Logging mirrors the lower-level mixed-model classes: every wrapper message is written to
    ''' <see cref="AppGlobals.BSlogg"/> and to an in-memory trace string for future UI/user diagnostics.
    ''' </para>
    ''' </remarks>
    Public Class MMRM

        Private ReadOnly pRequest As MixedModelFitRequest
        Private pEngine As MixedModelEngine = Nothing
        Private pStrTrace As String = String.Empty

        ''' <summary>
        ''' Last fitted result.  The lowercase field mirrors the style used by several existing
        ''' model classes in the project.
        ''' </summary>
        Public results As MixedModelResult = Nothing

        ''' <summary>
        ''' Creates an MMRM wrapper around a prepared <see cref="MixedModelFitRequest"/>.
        ''' </summary>
        Public Sub New(req As MixedModelFitRequest)
            If req Is Nothing Then Throw New ArgumentNullException(NameOf(req))
            pRequest = req
            AppendTrace("MMRM.New initialized. " & req.Describe())
        End Sub

        ''' <summary>
        ''' Fits the MMRM by delegating to <see cref="MixedModelEngine"/>.
        ''' </summary>
        Public Function Fit() As MixedModelResult
            AppendInfo("MMRM.Fit start. " & pRequest.Describe())
            NormalizeAndValidateMMRMIntent()

            pEngine = New MixedModelEngine(pRequest)
            results = pEngine.Fit()
            If results IsNot Nothing Then
                results.strTrace = MergeTraces(results.strTrace, pStrTrace)
            End If

            AppendInfo("MMRM.Fit completed. converged=" & If(results Is Nothing, "False", results.Converged.ToString()) & "; message='" & If(results Is Nothing, "No result", results.Message) & "'.")
            Return results
        End Function

        ''' <summary>
        ''' Returns the engine used for the most recent fit, or <c>Nothing</c> before <see cref="Fit"/> is called.
        ''' </summary>
        Public ReadOnly Property Engine As MixedModelEngine
            Get
                Return pEngine
            End Get
        End Property

        ''' <summary>
        ''' In-memory diagnostic trace accumulated by this wrapper.
        ''' </summary>
        Public ReadOnly Property strTrace As String
            Get
                Return pStrTrace
            End Get
        End Property

        ''' <summary>
        ''' Creates a matrix-level MMRM request from already-blocked data.
        ''' </summary>
        Public Shared Function CreateRequest(data As MixedModelBlockData,
                                             residualStruct As MixedModelRStruct,
                                             Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                             Optional fixedEffectNames() As String = Nothing,
                                             Optional responseVarName As String = "",
                                             Optional subjectVarName As String = "",
                                             Optional visitVarName As String = "",
                                             Optional requestLabel As String = "") As MixedModelFitRequest
            Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateMMRM(data, residualStruct, fitMethod)
            req.FixedEffectNames = fixedEffectNames
            req.ResponseVarName = responseVarName
            req.SubjectVarName = subjectVarName
            req.VisitVarName = visitVarName
            req.RequestLabel = requestLabel
            Return req
        End Function

        ''' <summary>
        ''' Builds and fits an MMRM directly from raw arrays.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This helper is mainly intended for early validation tests.  It deliberately does not accept a
        ''' random-effects design matrix because MMRM is represented by <c>V_i = R_i</c>.  The future formula
        ''' service can construct the fixed-effects matrix from formulas such as treatment-by-visit and
        ''' baseline-by-visit interactions, then call the same request/engine path.
        ''' </para>
        ''' </remarks>
        Public Shared Function FitFromArrays(y() As Double,
                                             x(,) As Double,
                                             subjectId() As Object,
                                             visit() As Double,
                                             Optional residualStructName As String = "Unstructured",
                                             Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                             Optional fixedEffectNames() As String = Nothing,
                                             Optional responseVarName As String = "",
                                             Optional subjectVarName As String = "",
                                             Optional visitVarName As String = "",
                                             Optional requestLabel As String = "") As MixedModelResult

            Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                             x:=x,
                                                                             subjectId:=subjectId,
                                                                             z:=Nothing,
                                                                             visit:=visit,
                                                                             sortWithinSubjectByVisit:=True)
            Dim rStruct As MixedModelRStruct = MixedModelRStructUtils.createMixedModelRStruct(residualStructName)
            Dim req As MixedModelFitRequest = CreateRequest(data,
                                                            rStruct,
                                                            fitMethod,
                                                            fixedEffectNames,
                                                            responseVarName,
                                                            subjectVarName,
                                                            visitVarName,
                                                            requestLabel)
            Dim model As New MMRM(req)
            Return model.Fit()
        End Function

        ''' <summary>
        ''' Releases large fit-state references retained by this convenience wrapper after a GUI caller
        ''' has already consumed the returned <see cref="MixedModelResult"/>.
        ''' </summary>
        ''' <param name="releaseResultWorkspaces">If True, releases large KR/Satterthwaite post-estimation arrays from the cached result.</param>
        ''' <param name="clearRequestRuntimeReferences">If True, clears callbacks and blocked input data held by the request object.</param>
        ''' <remarks>
        ''' This method is intended for modeless GUI workflows where the output workbook has already been
        ''' written and the wrapper is no longer needed.  It should not be called before post-estimation
        ''' calculations that require the detailed KR workspace.
        ''' </remarks>
        Public Sub ReleaseFitState(Optional releaseResultWorkspaces As Boolean = True,
                                   Optional clearRequestRuntimeReferences As Boolean = False)
            Try
                If releaseResultWorkspaces AndAlso results IsNot Nothing Then
                    results.ReleaseLargePostEstimationWorkspaces()
                End If

                If clearRequestRuntimeReferences AndAlso pRequest IsNot Nothing Then
                    pRequest.ProgressReporter = Nothing
                    pRequest.CancellationRequested = Nothing
                    pRequest.InterruptionRequested = Nothing
                    pRequest.Data = Nothing
                End If

                results = Nothing
                pEngine = Nothing
                pStrTrace = String.Empty
            Catch
                ' Release is opportunistic; callers have already consumed the result.
            End Try
        End Sub

        ''' <summary>
        ''' Validates that the request is truly an MMRM/R-side-only request.
        ''' </summary>

        Private Sub NormalizeAndValidateMMRMIntent()
            If pRequest.Data IsNot Nothing AndAlso pRequest.Data.Q > 0 Then
                Throw New ApplicationException("MMRM request must not contain a random-effects design matrix Z. Use LMM for Z/G-side random effects or rebuild the data with z := Nothing.")
            End If

            If pRequest.RandomStruct IsNot Nothing AndAlso Not pRequest.RandomStruct.IsDegenerateZeroG() Then
                AppendWarn("MMRM request supplied a non-degenerate RandomStruct. It will not be used because Data.Q = 0; set RandomStruct = Nothing for a cleaner MMRM request.")
                pRequest.RandomStruct = Nothing
            End If

            pRequest.Validate()

            If pRequest.ResidualStruct IsNot Nothing AndAlso pRequest.ResidualStruct.UsesVisitIndex() AndAlso Not pRequest.Data.HasVisit Then
                AppendWarn("MMRM residual structure uses visit indexing, but the blocked data do not contain visit values. Sequential within-subject order will be used as pseudo-visit order.")
            End If
        End Sub

        Private Sub AppendInfo(message As String)
            AppendLogCore("INFO", message)
            AppGlobals.BSlogg.Info(message)
        End Sub

        Private Sub AppendWarn(message As String)
            AppendLogCore("WARN", message)
            AppGlobals.BSlogg.Warn(message)
        End Sub

        Private Sub AppendTrace(message As String)
            AppendLogCore("TRACE", message)
            AppGlobals.BSlogg.Trace(message)
        End Sub

        Private Sub AppendLogCore(level As String, message As String)
            Dim line As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") & "|" & level & "|MMRM|" & If(message, String.Empty)
            If String.IsNullOrEmpty(pStrTrace) Then
                pStrTrace = line
            Else
                pStrTrace &= vbNewLine & line
            End If
        End Sub

        Private Function MergeTraces(a As String, b As String) As String
            If String.IsNullOrEmpty(a) Then Return If(b, String.Empty)
            If String.IsNullOrEmpty(b) Then Return a
            If a.Contains(b) Then Return a
            Return a & vbNewLine & b
        End Function

    End Class

End Namespace
