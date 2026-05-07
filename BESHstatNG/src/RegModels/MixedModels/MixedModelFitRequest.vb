Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Progress update raised by the mixed-model engine.
    ''' </summary>
    Public Class MixedModelProgressInfo
        Public Property Stage As String = String.Empty
        Public Property Message As String = String.Empty
        Public Property Percent As Integer = 0
        Public Property Iteration As Integer = -1
        Public Property MaxIterations As Integer = -1
        Public Property Objective As Double = Double.NaN
        Public Property FunctionChange As Double = Double.NaN
        Public Property GradNorm As Double = Double.NaN
        Public Property StepNorm As Double = Double.NaN
        Public Property ElapsedTimeMs As Double = Double.NaN
    End Class

    ''' <summary>
    ''' Request / configuration object passed into the Gaussian mixed-model engine.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The mixed-model fitting pipeline is intentionally split into two layers:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><see cref="MixedModelBlockData"/> stores the blocked subject-level data representation.</description></item>
    ''' <item><description><see cref="MixedModelFitRequest"/> stores model intent, covariance choices, starting values, and runtime controls.</description></item>
    ''' </list>
    ''' <para>
    ''' This separation is deliberate. It keeps the numerical engine independent from the UI and from the
    ''' formula parser, while still allowing one validated engine to power ordinary Gaussian mixed models (LMM)
    ''' and mixed models for repeated measures (MMRM).
    ''' </para>
    ''' <para>
    ''' Mathematically, the engine later consumes this request in order to evaluate the profiled Gaussian
    ''' likelihood for subject blocks i = 1, ..., n:
    ''' </para>
    ''' <para><c>y_i ~ N(X_i beta, V_i)</c></para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' where:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><c>X_i</c> is the fixed-effects design matrix for subject i.</description></item>
    ''' <item><description><c>Z_i</c> is the random-effects design matrix for subject i.</description></item>
    ''' <item><description><c>G</c> is the random-effects covariance structure (G-side).</description></item>
    ''' <item><description><c>R_i</c> is the residual / within-subject covariance structure (R-side).</description></item>
    ''' </list>
    ''' <para>
    ''' For MMRM the same request object is reused with <c>Z_i = Nothing</c> and a <c>NoRandomEffects</c>
    ''' G-side structure so that the full block covariance is represented on the residual side:
    ''' <c>V_i = R_i</c>.
    ''' </para>
    ''' <para>
    ''' Validation and logging are implemented directly on the request object because the request is the first
    ''' place where inconsistent settings can be detected. Catching such problems early makes debugging easier
    ''' and produces clearer user-facing errors later.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelFitRequest

        ''' <summary>
        ''' Optional progress reporter used by GUI clients. UDF callers should normally
        ''' leave this unset.
        ''' </summary>
        Public Property ProgressReporter As Action(Of MixedModelProgressInfo) = Nothing

        ''' <summary>
        ''' Optional cooperative cancellation callback. GUI callers can set this to
        ''' request that long MMRM/LMM fitting exits at the next safe numerical checkpoint.
        ''' </summary>
        Public Property CancellationRequested As Func(Of Boolean) = Nothing

        ''' <summary>
        ''' Optional cooperative interruption callback. Unlike cancellation, interruption asks the
        ''' optimizer to stop after the latest accepted covariance-parameter iterate and return
        ''' the corresponding partial/current model estimates when possible.
        ''' </summary>
        Public Property InterruptionRequested As Func(Of Boolean) = Nothing

        ''' <summary>
        ''' Blocked subject-level data used by the engine.
        ''' </summary>
        ''' <remarks>
        ''' This object is usually produced by <see cref="MixedModelBlockData.FromArrays"/> or by a future
        ''' formula/data service that constructs y, X, Z, subject IDs, and visit indices.
        ''' </remarks>
        Public Property Data As MixedModelBlockData = Nothing

        ''' <summary>
        ''' Optional user-facing name of the response variable.
        ''' </summary>
        Public Property ResponseVarName As String = String.Empty

        ''' <summary>
        ''' Optional user-facing name of the subject identifier variable.
        ''' </summary>
        Public Property SubjectVarName As String = String.Empty

        ''' <summary>
        ''' Optional user-facing name of the visit/time variable.
        ''' </summary>
        Public Property VisitVarName As String = String.Empty

        ''' <summary>
        ''' Optional normalized fixed-effects formula text used to construct X.
        ''' </summary>
        Public Property FixedFormulaText As String = String.Empty

        ''' <summary>
        ''' Optional normalized random-effects formula text used to construct Z.
        ''' </summary>
        Public Property RandomFormulaText As String = String.Empty

        ''' <summary>
        ''' Column names corresponding to the fixed-effects design matrix X.
        ''' </summary>
        ''' <remarks>
        ''' If supplied, the array length should equal <c>Data.P</c>. These names are later used in fixed-effects
        ''' result tables, parameter labels, traces, and UDF extractors.
        ''' </remarks>
        Public Property FixedEffectNames As String() = Nothing

        ''' <summary>
        ''' Column names corresponding to the random-effects design matrix Z.
        ''' </summary>
        ''' <remarks>
        ''' If supplied, the array length should equal <c>Data.Q</c>. These names are later used for variance-component
        ''' parameter labels, BLUP tables, and covariance-parameter summaries.
        ''' </remarks>
        Public Property RandomEffectNames As String() = Nothing

        ''' <summary>
        ''' Likelihood criterion used by the engine.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' For Gaussian mixed models this is usually either full maximum likelihood (ML) or restricted maximum
        ''' likelihood (REML). The profiled objective handled by the engine is:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description><c>-2 log L_ML = Sum_i log|V_i| + Q(theta) + n log(2 pi)</c></description></item>
        ''' <item><description><c>-2 log L_REML = Sum_i log|V_i| + log|X' V^-1 X| + Q(theta) + (n-p) log(2 pi)</c></description></item>
        ''' </list>
        ''' <para>
        ''' where <c>Q(theta)</c> denotes the residual quadratic form after profiling out the fixed effects.
        ''' </para>
        ''' </remarks>
        Public Property FitMethod As MixedModelFitMethod = MixedModelFitMethod.REML

        ''' <summary>
        ''' If True, the engine builds the Kenward-Roger derivative workspace after
        ''' fitting. Request validation enables this automatically when KR inference is
        ''' selected through <see cref="UseKenwardRoger"/>, <see cref="FixedInferenceMethod"/>,
        ''' or <see cref="KenwardRogerOptions"/>.
        ''' </summary>
        Public Property BuildKenwardRogerWorkspace As Boolean = False

        ''' <summary>
        ''' If True and <see cref="BuildKenwardRogerWorkspace"/> is also True, the engine
        ''' finite-differences second derivatives d2V_i/dtheta_h dtheta_j for full KR
        ''' R_hj matrices. Request validation enables this automatically for
        ''' <see cref="MixedModelKenwardRogerAdjustmentKind.Full"/>.
        ''' </summary>
        Public Property BuildKenwardRogerSecondDerivatives As Boolean = False

        ''' <summary>
        ''' Central Kenward-Roger options.  This supersedes the older loose KR flags
        ''' while keeping them source-compatible.  For full R mmrm-style MMRM KR set
        ''' this to MixedModelKenwardRogerOptions.CreateFullMmrm().
        ''' </summary>
        Public Property KenwardRogerOptions As MixedModelKenwardRogerOptions = MixedModelKenwardRogerOptions.CreateDefault()

        ''' <summary>
        ''' Residual / within-subject covariance structure (R-side).
        ''' </summary>
        ''' <remarks>
        ''' This is required for both LMM and MMRM. In MMRM it is the primary covariance object because
        ''' <c>V_i = R_i</c> when no random effects are present.
        ''' </remarks>
        Public Property ResidualStruct As MixedModelRStruct = Nothing

        ''' <summary>
        ''' Random-effects covariance structure (G-side).
        ''' </summary>
        ''' <remarks>
        ''' This property is optional for an MMRM-style fit and required for ordinary LMM whenever the data
        ''' contain a random-effects design matrix Z. The forthcoming <c>MixedModelGStruct.vb</c> file defines
        ''' the concrete implementations used here.
        ''' </remarks>
        Public Property RandomStruct As MixedModelGStruct = Nothing

        ''' <summary>
        ''' Runtime and optimizer control settings.
        ''' </summary>
        Public Property Control As MixedModelControl = MixedModelControl.CreateDefault()

        ''' <summary>
        ''' Optional user-supplied starting values for the covariance parameters on the G-side.
        ''' </summary>
        ''' <remarks>
        ''' Length should match <c>RandomStruct.ParamCount(Data.Q)</c> once a concrete G-side implementation exists.
        ''' The parameterization is intentionally engine-internal (for example, log-variances and unconstrained
        ''' correlation / Cholesky parameters) rather than a user-facing statistical parameterization.
        ''' </remarks>
        Public Property StartThetaG As Double() = Nothing

        ''' <summary>
        ''' Optional user-supplied starting values for the covariance parameters on the R-side.
        ''' </summary>
        ''' <remarks>
        ''' Length should match <c>ResidualStruct.ParamCount(Data)</c>. The exact interpretation depends on the
        ''' chosen residual structure and is intentionally on the engine/internal scale.
        ''' </remarks>
        Public Property StartThetaR As Double() = Nothing

        ''' <summary>
        ''' Optional user-supplied starting values for the fixed effects.
        ''' </summary>
        ''' <remarks>
        ''' If supplied, length should equal <c>Data.P</c>. In the initial implementation, this is mainly a
        ''' diagnostic / advanced-user feature because good default starts can often be obtained from an ordinary
        ''' Gaussian regression that ignores within-subject correlation.
        ''' </remarks>
        Public Property StartBeta As Double() = Nothing

        ''' <summary>
        ''' If True, the request asks the engine to compute Satterthwaite denominator degrees of freedom.
        ''' </summary>
        ''' <remarks>
        ''' This is intentionally just a request flag for now. The early mixed-model engine can ignore the flag
        ''' until denominator degrees-of-freedom approximations are implemented.
        ''' </remarks>
        Public Property UseSatterthwaite As Boolean = False

        ''' <summary>
        ''' If True, the request asks the engine to compute Kenward-Roger adjusted inference.
        ''' </summary>
        ''' <remarks>
        ''' Like <see cref="UseSatterthwaite"/>, this is stored on the request now so the UI and UDF surface do not
        ''' need to change later when the approximation is added.
        ''' </remarks>
        Public Property UseKenwardRoger As Boolean = False

        ''' <summary>
        ''' Optional free-form request label for trace/debug output.
        ''' </summary>
        ''' <remarks>
        ''' Typical uses include a worksheet/UDF label, a dialog operation name, or an internal test-case name.
        ''' It is included in trace messages when present.
        ''' </remarks>
        Public Property RequestLabel As String = String.Empty

        ''' <summary>
        ''' In-memory trace text accumulated while validating and preparing the request.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This mirrors the project's broader pattern where numerical helpers often accept a ByRef strTrace
        ''' accumulator while also writing to <see cref="AppGlobals.BSlogg"/>. Keeping both channels is useful:
        ''' </para>
        ''' <list type="bullet">
        ''' <item><description>the global logger provides durable diagnostic output,</description></item>
        ''' <item><description>the request trace can later be surfaced in UI panels, UDF messages, or result tables.</description></item>
        ''' </list>
        ''' </remarks>
        Public Property strTrace As String = String.Empty

        ''' <summary>
        ''' Fixed-effect inference method used when computing coefficient test statistics, p-values,
        ''' and confidence intervals in <see cref="MixedModelResult.wrapResults"/>.
        ''' </summary>
        ''' <remarks>
        ''' The numerical likelihood fit itself is unchanged by this option.  It affects only the
        ''' denominator degrees of freedom and therefore the reported statistic labels, p-values,
        ''' and confidence interval critical values for fixed effects.
        ''' </remarks>
        Public Property FixedInferenceMethod As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal

        ''' <summary>
        ''' Creates a request with conservative defaults.
        ''' </summary>
        Public Sub New()
            Control = MixedModelControl.CreateDefault()
            strTrace = String.Empty
        End Sub

        ''' <summary>
        ''' Enables the full Kenward-Roger request contract used for R mmrm-compatible
        ''' MMRM inference. The numerical fit itself is still performed by the normal
        ''' engine path; this method asks the engine to build the full KR workspace
        ''' and use KR fixed-effect inference.
        ''' </summary>
        Public Sub EnableFullKenwardRogerForMmrm()
            UseKenwardRoger = True
            UseSatterthwaite = False
            FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger
            BuildKenwardRogerWorkspace = True
            BuildKenwardRogerSecondDerivatives = True
            KenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullMmrm()
        End Sub

        ''' <summary>
        ''' Enables the full Kenward-Roger request contract intended for LMM inference.
        ''' The numerical fit itself is unchanged; this method asks the engine to build
        ''' a full covariance-scale KR workspace and use KR fixed-effect inference.
        ''' </summary>
        Public Sub EnableFullKenwardRogerForLmm()
            UseKenwardRoger = True
            UseSatterthwaite = False
            FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger
            BuildKenwardRogerWorkspace = True
            BuildKenwardRogerSecondDerivatives = True
            KenwardRogerOptions = MixedModelKenwardRogerOptions.CreateFullLmm()
        End Sub

        ''' <summary>
        ''' Creates a request for an ordinary Gaussian mixed model (LMM).
        ''' </summary>
        ''' <param name="data">Blocked subject data.</param>
        ''' <param name="residualStruct">R-side residual structure.</param>
        ''' <param name="randomStruct">G-side random-effects structure.</param>
        ''' <param name="fitMethod">Likelihood criterion.</param>
        Public Shared Function CreateLMM(data As MixedModelBlockData,
                                         residualStruct As MixedModelRStruct,
                                         randomStruct As MixedModelGStruct,
                                         Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML) As MixedModelFitRequest
            Dim req As New MixedModelFitRequest With {
                .Data = data,
                .ResidualStruct = residualStruct,
                .RandomStruct = randomStruct,
                .FitMethod = fitMethod,
                .Control = MixedModelControl.CreateDefault()
            }
            req.AppendTrace("CreateLMM initialized. fitMethod=" & fitMethod.ToString() & "; residual=" & req.ResidualStructName() & "; random=" & req.RandomStructName())
            Return req
        End Function

        ''' <summary>
        ''' Creates a request for an MMRM-style fit using only an R-side covariance structure.
        ''' </summary>
        ''' <param name="data">Blocked subject data.</param>
        ''' <param name="residualStruct">R-side covariance structure.</param>
        ''' <param name="fitMethod">Likelihood criterion (typically REML in early use).</param>
        ''' <remarks>
        ''' The request intentionally leaves <see cref="RandomStruct"/> as Nothing. The engine can later
        ''' interpret that as "no G-side contribution" or it can replace it internally with a dedicated
        ''' <c>NoRandomEffects</c> structure once the G-side abstraction is added.
        ''' </remarks>
        Public Shared Function CreateMMRM(data As MixedModelBlockData,
                                          residualStruct As MixedModelRStruct,
                                          Optional fitMethod As MixedModelFitMethod = MixedModelFitMethod.REML) As MixedModelFitRequest
            Dim req As New MixedModelFitRequest With {
                .Data = data,
                .ResidualStruct = residualStruct,
                .RandomStruct = Nothing,
                .FitMethod = fitMethod,
                .FixedInferenceMethod = MixedModelFixedInferenceMethod.BetweenWithin,
                .Control = MixedModelControl.CreateDefault()
            }
            req.AppendTrace("CreateMMRM initialized. fitMethod=" & fitMethod.ToString() & "; residual=" & req.ResidualStructName() & "; random=None")
            Return req
        End Function

        ''' <summary>
        ''' Returns True when the request represents an MMRM-style fit with no random-effects structure.
        ''' </summary>
        Public Function IsMMRM() As Boolean
            Return RandomStruct Is Nothing AndAlso (Data Is Nothing OrElse Data.Q = 0)
        End Function

        ''' <summary>
        ''' Returns True when the request carries a random-effects design and a G-side covariance structure.
        ''' </summary>
        Public Function HasRandomEffects() As Boolean
            Return Data IsNot Nothing AndAlso Data.Q > 0 AndAlso RandomStruct IsNot Nothing
        End Function

        ''' <summary>
        ''' Returns a short human-readable description useful in logs, debug windows, and test output.
        ''' </summary>
        Public Function Describe() As String
            Dim nSubj As Integer = If(Data Is Nothing, 0, Data.NoSubjects)
            Dim nObs As Integer = If(Data Is Nothing, 0, Data.Nobs)
            Dim p As Integer = If(Data Is Nothing, 0, Data.P)
            Dim q As Integer = If(Data Is Nothing, 0, Data.Q)
            Return "label='" & If(RequestLabel, String.Empty) & "'; fitMethod=" & FitMethod.ToString() & "; nSubjects=" & nSubj.ToString() & "; nObs=" & nObs.ToString() & "; p=" & p.ToString() & "; q=" & q.ToString() & "; residual=" & ResidualStructName() & "; random=" & RandomStructName()
        End Function

        ''' <summary>
        ''' Clears the in-memory trace text.
        ''' </summary>
        Public Sub ClearTrace()
            strTrace = String.Empty
        End Sub

        ''' <summary>
        ''' Appends an informational message to the in-memory trace and to the global logger.
        ''' </summary>
        Public Sub AppendInfo(message As String)
            AppendLogCore("INFO", message)
            AppGlobals.BSlogg.Info(FormatLogMessage(message))
        End Sub

        ''' <summary>
        ''' Appends a warning message to the in-memory trace and to the global logger.
        ''' </summary>
        Public Sub AppendWarn(message As String)
            AppendLogCore("WARN", message)
            AppGlobals.BSlogg.Warn(FormatLogMessage(message))
        End Sub

        ''' <summary>
        ''' Appends a debug message to the in-memory trace and to the global logger.
        ''' </summary>
        Public Sub AppendDebug(message As String)
            AppendLogCore("DEBUG", message)
            AppGlobals.BSlogg.Debug(FormatLogMessage(message))
        End Sub

        ''' <summary>
        ''' Appends a trace message to the in-memory trace and to the global logger.
        ''' </summary>
        Public Sub AppendTrace(message As String)
            AppendLogCore("TRACE", message)
            AppGlobals.BSlogg.Trace(FormatLogMessage(message))
        End Sub

        ''' <summary>
        ''' Validates that the request is internally consistent before the numerical engine starts.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The validation performed here is intentionally structural rather than algorithmic. It answers questions
        ''' such as: "are the data present?", "do the parameter vectors have compatible lengths?", and
        ''' "is this request logically an LMM or an MMRM?".
        ''' </para>
        ''' <para>
        ''' Numerical validation - for example, whether a particular starting covariance matrix is positive definite
        ''' or whether the optimizer can step safely from the start - belongs to later engine layers.
        ''' </para>
        ''' </remarks>
        ''' <exception cref="ApplicationException">
        ''' Thrown when a required component is missing or when obviously inconsistent settings are detected.
        ''' </exception>
        Public Sub Validate()
            AppendTrace("MixedModelFitRequest.Validate start. " & Describe())

            If Data Is Nothing Then
                LogAndThrow("Mixed-model request validation failed: Data is Nothing.")
            End If

            If Data.NoSubjects <= 0 OrElse Data.Nobs <= 0 Then
                LogAndThrow("Mixed-model request validation failed: blocked data contain no subjects or no observations.")
            End If

            If Data.P <= 0 Then
                LogAndThrow("Mixed-model request validation failed: fixed-effects design matrix has zero columns.")
            End If

            If ResidualStruct Is Nothing Then
                LogAndThrow("Mixed-model request validation failed: ResidualStruct is Nothing.")
            End If

            If Control.MaxIter <= 0 Then
                LogAndThrow("Mixed-model request validation failed: Control.MaxIter must be > 0.")
            End If

            If Control.Epsilon <= 0 OrElse Control.StepTolerance <= 0 OrElse Control.FunctionTolerance <= 0 Then
                LogAndThrow("Mixed-model request validation failed: all control tolerances must be > 0.")
            End If

            If FixedEffectNames IsNot Nothing AndAlso FixedEffectNames.Length <> Data.P Then
                LogAndThrow("Mixed-model request validation failed: FixedEffectNames length (" & FixedEffectNames.Length.ToString() & ") does not match Data.P (" & Data.P.ToString() & ").")
            End If

            If Data.Q > 0 Then
                If RandomStruct Is Nothing Then
                    LogAndThrow("Mixed-model request validation failed: random-effects design Z is present but RandomStruct is Nothing.")
                End If
                If RandomEffectNames IsNot Nothing AndAlso RandomEffectNames.Length <> Data.Q Then
                    LogAndThrow("Mixed-model request validation failed: RandomEffectNames length (" & RandomEffectNames.Length.ToString() & ") does not match Data.Q (" & Data.Q.ToString() & ").")
                End If
            Else
                If RandomEffectNames IsNot Nothing AndAlso RandomEffectNames.Length > 0 Then
                    AppendWarn("RandomEffectNames were supplied but Data.Q = 0. The names will be ignored unless a random-effects design is later added.")
                End If
                If RandomStruct IsNot Nothing Then
                    AppendWarn("RandomStruct was supplied but Data.Q = 0. The request will behave like an MMRM/no-random-effects fit unless Z is later provided.")
                End If
            End If

            If StartBeta IsNot Nothing AndAlso StartBeta.Length <> Data.P Then
                LogAndThrow("Mixed-model request validation failed: StartBeta length (" & StartBeta.Length.ToString() & ") does not match Data.P (" & Data.P.ToString() & ").")
            End If

            Dim expectedR As Integer = ResidualStruct.ParamCount(Data)
            If StartThetaR IsNot Nothing AndAlso StartThetaR.Length <> expectedR Then
                LogAndThrow("Mixed-model request validation failed: StartThetaR length (" & StartThetaR.Length.ToString() & ") does not match ResidualStruct.ParamCount(Data) (" & expectedR.ToString() & ").")
            End If

            If RandomStruct IsNot Nothing AndAlso Data.Q > 0 Then
                Dim expectedG As Integer = RandomStruct.ParamCount(Data.Q)
                If StartThetaG IsNot Nothing AndAlso StartThetaG.Length <> expectedG Then
                    LogAndThrow("Mixed-model request validation failed: StartThetaG length (" & StartThetaG.Length.ToString() & ") does not match RandomStruct.ParamCount(Data.Q) (" & expectedG.ToString() & ").")
                End If
            ElseIf StartThetaG IsNot Nothing AndAlso StartThetaG.Length > 0 Then
                AppendWarn("StartThetaG was supplied even though no active G-side structure is present. The values will be ignored unless a random-effects design is later added.")
            End If

            If UseKenwardRoger AndAlso UseSatterthwaite Then
                LogAndThrow("Mixed-model request validation failed: UseKenwardRoger and UseSatterthwaite cannot both be True in the same request.")
            End If

            If ResidualStruct.UsesVisitIndex() Then
                If Not Data.HasVisit Then
                    AppendWarn("Residual structure '" & ResidualStruct.ToString() & "' is visit-index based, but Data.HasVisit = False. Sequential within-subject row order will be used as pseudo-visit order.")
                ElseIf String.IsNullOrWhiteSpace(VisitVarName) Then
                    AppendDebug("Residual structure '" & ResidualStruct.ToString() & "' uses visit indexing. Visit values exist in blocked data, but VisitVarName was not supplied.")
                End If
            End If

            Dim krRequested As Boolean = UseKenwardRoger OrElse
                                            FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger OrElse
                                            BuildKenwardRogerWorkspace OrElse
                                            (KenwardRogerOptions IsNot Nothing AndAlso KenwardRogerOptions.Enabled)

            If krRequested Then
                If KenwardRogerOptions Is Nothing Then KenwardRogerOptions = MixedModelKenwardRogerOptions.CreateDefault()
                KenwardRogerOptions.Enabled = True
                BuildKenwardRogerWorkspace = True

                If KenwardRogerOptions.Adjustment = MixedModelKenwardRogerAdjustmentKind.Full Then
                    BuildKenwardRogerSecondDerivatives = True
                End If

                If FitMethod = MixedModelFitMethod.ML AndAlso KenwardRogerOptions.RequireReml Then
                    LogAndThrow("Mixed-model request validation failed: Kenward-Roger inference requires REML. Please refit with FitMethod = REML before requesting KR.")
                ElseIf FitMethod = MixedModelFitMethod.ML Then
                    AppendWarn("Kenward-Roger was requested with ML. This is allowed only because KenwardRogerOptions.RequireReml = False; validation should prefer REML.")
                End If
            End If

            If String.IsNullOrWhiteSpace(ResponseVarName) Then
                AppendDebug("ResponseVarName is blank. This is allowed, but output tables will be less descriptive.")
            End If

            AppendTrace("MixedModelFitRequest.Validate completed successfully. " & Describe())
        End Sub

        ''' <summary>
        ''' Returns the residual structure display name or None.
        ''' </summary>
        Public Function ResidualStructName() As String
            If ResidualStruct Is Nothing Then Return "None"
            Return ResidualStruct.ToString()
        End Function

        ''' <summary>
        ''' Returns the random-effects structure display name or None.
        ''' </summary>
        Public Function RandomStructName() As String
            If RandomStruct Is Nothing Then Return "None"
            Return RandomStruct.ToString()
        End Function

        ''' <summary>
        ''' Throws an <see cref="ApplicationException"/> after logging the message.
        ''' </summary>
        Private Sub LogAndThrow(message As String)
            AppendWarn(message)
            Throw New ApplicationException(message)
        End Sub

        ''' <summary>
        ''' Appends one trace/debug line to the in-memory trace accumulator.
        ''' </summary>
        Private Sub AppendLogCore(level As String, message As String)
            Dim line As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") & "|" & level & "|" & FormatLogMessage(message)
            If String.IsNullOrEmpty(strTrace) Then
                strTrace = line
            Else
                strTrace &= vbNewLine & line
            End If
        End Sub

        ''' <summary>
        ''' Formats a request-scoped log message.
        ''' </summary>
        Private Function FormatLogMessage(message As String) As String
            Dim prefix As String = "MixedModelFitRequest"
            If Not String.IsNullOrWhiteSpace(RequestLabel) Then prefix &= "[" & RequestLabel & "]"
            Return prefix & ": " & If(message, String.Empty)
        End Function

    End Class

End Namespace
