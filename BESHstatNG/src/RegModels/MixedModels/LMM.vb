Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Convenience wrapper for fitting ordinary Gaussian linear mixed models (LMM) with the
    ''' shared <see cref="MixedModelEngine"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The numerical work is performed by <see cref="MixedModelEngine"/>.  This wrapper exists to
    ''' make the intent of an ordinary linear mixed model explicit, to centralize LMM-specific
    ''' validation/warnings, and to provide a small matrix-based entry point that can be used by unit
    ''' tests before the UI/formula/UDF layers are added.
    ''' </para>
    ''' <para>
    ''' Mathematically, an LMM is represented by subject blocks
    ''' <c>y_i ~ N(X_i beta, V_i)</c> with
    ''' <c>V_i = Z_i G Z_i' + R_i</c>.  This wrapper therefore expects a random-effects design
    ''' matrix <c>Z</c> and a non-degenerate <see cref="MixedModelGStruct"/> in the usual case.
    ''' It does not implement a separate algorithm; it simply configures the shared engine so that
    ''' the same code path can also support MMRM through <see cref="MMRM"/>.
    ''' </para>
    ''' <para>
    ''' Logging follows the same mixed-model pattern used in the lower-level classes: messages are
    ''' written to <see cref="AppGlobals.BSlogg"/> and also accumulated in <see cref="strTrace"/> so
    ''' that the future UI or UDF layer can expose diagnostics to the user.
    ''' </para>
    ''' </remarks>
    Public Class LMM

        Private ReadOnly pRequest As MixedModelFitRequest
        Private pEngine As MixedModelEngine = Nothing
        Private pStrTrace As String = String.Empty

        ''' <summary>
        ''' Last fitted result.  The lowercase field mirrors the style used by several existing
        ''' model classes in the project.
        ''' </summary>
        Public results As MixedModelResult = Nothing

        ''' <summary>
        ''' Creates an LMM wrapper around a prepared <see cref="MixedModelFitRequest"/>.
        ''' </summary>
        ''' <param name="req">Prepared request containing blocked data, R-side structure, G-side structure, and controls.</param>
        Public Sub New(req As MixedModelFitRequest)
            If req Is Nothing Then Throw New ArgumentNullException(NameOf(req))
            pRequest = req
            AppendTrace("LMM.New initialized. " & req.Describe())
        End Sub

        ''' <summary>
        ''' Fits the LMM by delegating to <see cref="MixedModelEngine"/>.
        ''' </summary>
        ''' <returns>A <see cref="MixedModelResult"/> containing fixed effects, covariance parameters, fit statistics, fitted values, residuals, and BLUPs when available.</returns>
        Public Function Fit() As MixedModelResult
            AppendInfo("LMM.Fit start. " & pRequest.Describe())
            ValidateLMMIntent()

            pEngine = New MixedModelEngine(pRequest)
            results = pEngine.Fit()
            If results IsNot Nothing Then
                results.strTrace = MergeTraces(results.strTrace, pStrTrace)
            End If

            AppendInfo("LMM.Fit completed. converged=" & If(results Is Nothing, "False", results.Converged.ToString()) & "; message='" & If(results Is Nothing, "No result", results.Message) & "'.")
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
        ''' Builds a blocked LMM request directly from raw arrays and fits it.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This helper is intended primarily for early unit tests and validation examples.  The future
        ''' UI/formula layer should build <see cref="MixedModelFitRequest"/> through a formula service
        ''' instead of relying on this raw matrix entry point.
        ''' </para>
        ''' </remarks>
        Public Shared Function FitFromArrays(y() As Double,
                                             x(,) As Double,
                                             subjectId() As Object,
                                             z(,) As Double,
                                             Optional visit() As Double = Nothing,
                                             Optional residualStructName As String = "Identity",
                                             Optional randomStructName As String = "Random Intercept",
                                             Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML,
                                             Optional fixedEffectNames() As String = Nothing,
                                             Optional randomEffectNames() As String = Nothing,
                                             Optional responseVarName As String = "",
                                             Optional subjectVarName As String = "",
                                             Optional visitVarName As String = "",
                                             Optional requestLabel As String = "") As MixedModelResult

            Dim data As MixedModelBlockData = MixedModelBlockData.FromArrays(y:=y,
                                                                             x:=x,
                                                                             subjectId:=subjectId,
                                                                             z:=z,
                                                                             visit:=visit,
                                                                             sortWithinSubjectByVisit:=True)
            Dim rStruct As MixedModelRStruct = MixedModelRStructUtils.createMixedModelRStruct(residualStructName)
            Dim gStruct As MixedModelGStruct = MixedModelGStructUtils.createMixedModelGStruct(randomStructName)
            Dim req As MixedModelFitRequest = MixedModelFitRequest.CreateLMM(data, rStruct, gStruct, fitMethod)
            req.FixedEffectNames = fixedEffectNames
            req.RandomEffectNames = randomEffectNames
            req.ResponseVarName = responseVarName
            req.SubjectVarName = subjectVarName
            req.VisitVarName = visitVarName
            req.RequestLabel = requestLabel

            Dim model As New LMM(req)
            Return model.Fit()
        End Function

        ''' <summary>
        ''' Performs LMM-specific structural checks and warnings before the generic engine starts.
        ''' </summary>
        Private Sub ValidateLMMIntent()
            pRequest.Validate()

            If pRequest.Data Is Nothing Then Return

            If pRequest.Data.Q <= 0 Then
                AppendWarn("LMM request has Data.Q = 0. The generic engine can fit the model, but this is effectively an R-side-only marginal/MMRM-style model rather than an ordinary random-effects LMM.")
                Return
            End If

            If pRequest.RandomStruct Is Nothing Then
                Throw New ApplicationException("LMM request contains a random-effects design Z, but RandomStruct is Nothing.")
            End If

            If pRequest.RandomStruct.IsDegenerateZeroG() Then
                AppendWarn("LMM request uses a degenerate zero G-side structure even though Z is present. The fit will ignore the random-effects design and behave like an R-side-only model.")
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
            Dim line As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") & "|" & level & "|LMM|" & If(message, String.Empty)
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
