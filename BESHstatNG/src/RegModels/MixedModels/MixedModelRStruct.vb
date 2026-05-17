Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Factory/helper routines for creating residual covariance structures used by the
    ''' Gaussian mixed-model engine (LMM/MMRM).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module intentionally mirrors the role that <c>GEEcovStructUtils</c> plays
    ''' for GEE working-correlation structures: it provides one centralized conversion
    ''' from user-facing text labels to a concrete covariance-structure implementation.
    ''' </para>
    ''' <para>
    ''' The structures created here are used on the <c>R</c>-side of the mixed-model
    ''' covariance decomposition:
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' For ordinary Gaussian mixed models, <c>R_i</c> represents within-subject residual
    ''' covariance after accounting for random effects. For MMRM, the same machinery is
    ''' re-used with <c>Z_i = Nothing</c> and no <c>G</c>-side contribution, so that the
    ''' entire block covariance is represented by <c>R_i</c>.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelRStructUtils

        ''' <summary>
        ''' Creates a concrete residual covariance structure instance from a user-facing name.
        ''' </summary>
        ''' <param name="type">
        ''' Name of the covariance structure. Matching is case-insensitive and supports a few
        ''' common abbreviations (for example <c>CS</c>, <c>AR1</c>, <c>UN</c>).
        ''' </param>
        ''' <returns>A newly constructed <see cref="MixedModelRStruct"/> implementation.</returns>
        ''' <exception cref="ApplicationException">Thrown when the structure name is not supported.</exception>
        Public Function createMixedModelRStruct(type As String) As MixedModelRStruct
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
                AppGlobals.BSlogg.Error($"Unsupported mixed-model residual covariance structure. type='{type}'")
                Throw New ApplicationException("Unsupported mixed-model residual covariance structure. type = " & type)
            End If

            AppGlobals.BSlogg.Trace($"createMixedModelRStruct created {f.ToString()} for type='{type}'")
            Return f
        End Function

    End Module

    ''' <summary>
    ''' Abstract base class for all mixed-model residual covariance structures.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' In a Gaussian mixed model, each subject/cluster block <c>i</c> contributes a covariance
    ''' matrix of the form:
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' This class is responsible only for the <c>R_i</c> term — the within-subject residual
    ''' covariance. That makes it directly reusable for both ordinary LMM and MMRM.
    ''' </para>
    ''' <para>
    ''' Design intent:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Provide one place where the mathematical definition of each R-side structure lives.</description></item>
    ''' <item><description>Keep parameter counting, naming, initialization, and matrix construction together.</description></item>
    ''' <item><description>Guarantee positive semi-definite / positive definite construction where practical via parameter transforms.</description></item>
    ''' <item><description>Start building a structured logging/trace surface so that optimizer/debug traces can later be shown to users, similar in spirit to GLM/GEE.</description></item>
    ''' </list>
    ''' <para>
    ''' Parameterization decisions in this file:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Variance-like parameters are represented on the log scale, i.e. <c>σ² = exp(θ)</c>.</description></item>
    ''' <item><description>Correlation parameters are stored on an unconstrained scale and mapped with <c>ρ = tanh(θ)</c>.</description></item>
    ''' <item><description>Unstructured covariance uses a Cholesky parameterization <c>R = L L'</c>, with diagonal entries of <c>L</c> represented on the log scale.</description></item>
    ''' <item><description>Visit-based structures use the block's visit-index mapping when available. If no visit information exists, row order within subject is used as a sequential fallback; this is logged because it affects interpretation.</description></item>
    ''' </list>
    ''' </remarks>
    Public MustInherit Class MixedModelRStruct

        ''' <summary>
        ''' User-facing list of currently implemented residual covariance structures.
        ''' </summary>
        Public Shared RStructsList() As String =
            {"Identity", "Diagonal Heterogeneous", "Compound Symmetry", "Heterogeneous CS",
             "AR(1)", "Heterogeneous AR(1)", "Toeplitz (TOEP)", "Heterogeneous Toeplitz (TOEPH)", "Unstructured"}

        ''' <summary>
        ''' Returns the display name of the structure.
        ''' </summary>
        Public MustOverride Overrides Function ToString() As String

        ''' <summary>
        ''' Returns <c>True</c> when the structure uses visit ordering / visit indexing,
        ''' rather than treating observations as exchangeable only.
        ''' </summary>
        Public Overridable Function UsesVisitIndex() As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Returns the number of free covariance parameters required by the structure
        ''' for the supplied dataset.
        ''' </summary>
        Public MustOverride Function ParamCount(data As MixedModelBlockData) As Integer

        ''' <summary>
        ''' Returns human-readable parameter names aligned with <see cref="ParamCount"/>.
        ''' </summary>
        Public MustOverride Function ParamNames(data As MixedModelBlockData) As String()

        ''' <summary>
        ''' Returns a conservative set of starting values on the internal optimizer scale.
        ''' </summary>
        ''' <param name="data">Blocked mixed-model data.</param>
        ''' <param name="olsResidualVar">
        ''' Residual-variance scale coming from a simpler working fit (typically OLS / GLS independence).
        ''' This value is converted to log-variance starts where needed.
        ''' </param>
        Public MustOverride Function StartParams(data As MixedModelBlockData,
                                                 Optional olsResidualVar As Double = 1.0) As Double()

        ''' <summary>
        ''' Builds the residual covariance matrix <c>R_i</c> for one subject block.
        ''' </summary>
        ''' <param name="theta">Parameter vector on the optimizer/internal scale.</param>
        ''' <param name="block">Subject block for which the covariance matrix is being built.</param>
        ''' <param name="data">Global blocked dataset metadata.</param>
        ''' <param name="strTrace">
        ''' Optional trace text accumulator. This lets the caller keep an in-memory iteration/debug trace,
        ''' while the same information is also written through the application's logger.
        ''' </param>
        ''' <returns>A square covariance matrix with dimension <c>block.Nobs × block.Nobs</c>.</returns>
        Public MustOverride Function BuildRi(theta() As Double,
                                             block As MixedModelSubjectBlock,
                                             data As MixedModelBlockData,
                                             Optional ByRef strTrace As String = Nothing) As Double(,)

        ''' <summary>
        ''' Returns the visit dimension used for global visit-based covariance parameterization.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' When explicit visit information is available, the global visit dimension is the number of
        ''' distinct visit values in the full dataset. This is the preferred interpretation for MMRM-style
        ''' structures because it lets a single covariance parameter correspond to the same visit across
        ''' subjects with monotone or intermittent missingness.
        ''' </para>
        ''' <para>
        ''' If visit information is absent, the code falls back to the maximum cluster size and interprets
        ''' within-subject row order as pseudo-visit order. This is convenient for a first implementation,
        ''' but it is a weaker assumption and should be visible in the trace/logs.
        ''' </para>
        ''' </remarks>
        Protected Function VisitDimension(data As MixedModelBlockData) As Integer
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.HasVisit AndAlso data.UniqueVisitValues IsNot Nothing AndAlso data.UniqueVisitValues.Length > 0 Then
                Return data.UniqueVisitValues.Length
            End If
            Return data.MaxClusterSize()
        End Function

        ''' <summary>
        ''' Converts the block's visit information into zero-based global visit indices.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' For structures such as UN, AR(1), and heterogeneous visit-based models, the covariance parameters
        ''' are defined in a global visit space. Observed rows for a subject are then represented by a submatrix
        ''' selected from that global covariance template using the visit indices returned here.
        ''' </para>
        ''' <para>
        ''' If explicit visit indices are not available, the function falls back to sequential order
        ''' <c>0,1,...,n_i-1</c> and writes a debug/trace entry.
        ''' </para>
        ''' </remarks>
        Protected Function GetBlockVisitIndices(block As MixedModelSubjectBlock,
                                                data As MixedModelBlockData,
                                                Optional ByRef strTrace As String = Nothing) As Integer()
            If block Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

            Dim n As Integer = block.Nobs
            Dim out(n - 1) As Integer

            If block.VisitIndex IsNot Nothing AndAlso block.VisitIndex.Length = n Then
                Array.Copy(block.VisitIndex, out, n)
                Return out
            End If

            For i = 0 To n - 1
                out(i) = i
            Next

            LogDebug($"{ToString()}.GetBlockVisitIndices using sequential pseudo-visit order for subject='{block.SubjectKey}' because no visit index is available.", strTrace)
            Return out
        End Function

        ''' <summary>
        ''' Ensures that the supplied parameter vector has the expected length.
        ''' </summary>
        Protected Sub ValidateThetaLength(theta() As Double, expected As Integer)
            Dim actual As Integer = If(theta Is Nothing, 0, theta.Length)
            If actual <> expected Then
                Throw New ArgumentException($"Unexpected parameter length for {ToString()}. Expected {expected}, received {actual}.")
            End If
        End Sub

        ''' <summary>
        ''' Maps an unconstrained optimizer parameter to a strictly positive variance/scale quantity.
        ''' </summary>
        Protected Function PositiveScale(theta As Double) As Double
            Return Math.Exp(theta)
        End Function

        ''' <summary>
        ''' Maps an unconstrained optimizer parameter to a correlation parameter using
        ''' the hyperbolic tangent transform.
        ''' </summary>
        ''' <remarks>
        ''' A small absolute cap is applied so that near-singular correlation matrices are less likely to
        ''' cause repeated Cholesky failures during likelihood optimization.
        ''' </remarks>
        Protected Function CorrelationFromTheta(theta As Double,
                                                Optional absLimit As Double = 0.995) As Double
            Dim rho As Double = Math.Tanh(theta)
            If rho > absLimit Then rho = absLimit
            If rho < -absLimit Then rho = -absLimit
            Return rho
        End Function

        ''' <summary>
        ''' Converts a residual-variance guess into a stable log-variance start.
        ''' </summary>
        Protected Function SafeLogVarianceStart(olsResidualVar As Double) As Double
            Dim v As Double = olsResidualVar
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v <= 0 Then v = 1.0
            If v < 0.000000000001 Then v = 0.000000000001
            Return Math.Log(v)
        End Function

        ''' <summary>
        ''' Returns a stable positive square root scale from a variance.
        ''' </summary>
        Protected Function SafeStdDev(varValue As Double) As Double
            Dim v As Double = varValue
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v <= 0 Then v = 0.000000000001
            Return Math.Sqrt(v)
        End Function

        ''' <summary>
        ''' Builds an identity matrix optionally scaled by a constant.
        ''' </summary>
        Protected Function IdentityMatrix(n As Integer,
                                          Optional scale As Double = 1.0) As Double(,)
            Dim out(n - 1, n - 1) As Double
            For i = 0 To n - 1
                out(i, i) = scale
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds a compound-symmetry correlation matrix with unit diagonal and constant off-diagonal correlation.
        ''' </summary>
        Protected Function BuildCorrelationCS(n As Integer, rho As Double) As Double(,)
            Dim out(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    out(i, j) = If(i = j, 1.0, rho)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds an AR(1)-style correlation matrix based on visit-index lag distance.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The implementation uses ordinal visit index distance, not raw numeric time difference.
        ''' This is intentional for the first mixed-model/MMRM engine because visit-indexed covariance
        ''' structures are the common case in clinical repeated-measures workflows.
        ''' </para>
        ''' <para>
        ''' If later you want continuous-time AR(1), this helper can remain unchanged and a separate
        ''' continuous-time structure can be added instead.
        ''' </para>
        ''' </remarks>
        Protected Function BuildCorrelationAR1(indices() As Integer, rho As Double) As Double(,)
            Dim n As Integer = indices.Length
            Dim out(n - 1, n - 1) As Double

            For i = 0 To n - 1
                For j = 0 To n - 1
                    Dim lag As Integer = Math.Abs(indices(i) - indices(j))
                    out(i, j) = rho ^ lag
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds a positive-definite Toeplitz autocorrelation sequence from unconstrained
        ''' partial-autocorrelation parameters. <c>rho(0)</c> is always 1.
        ''' </summary>
        Protected Function BuildToeplitzAutocorrelations(theta() As Double,
                                                         q As Integer,
                                                         Optional startIndex As Integer = 0) As Double()
            If q < 1 Then Throw New ArgumentOutOfRangeException(NameOf(q))

            Dim rho(q - 1) As Double
            rho(0) = 1.0
            If q = 1 Then Return rho

            Dim phi(q - 1, q - 1) As Double
            For k As Integer = 1 To q - 1
                Dim pacf As Double = CorrelationFromTheta(theta(startIndex + k - 1))
                phi(k, k) = pacf
                If k > 1 Then
                    For j As Integer = 1 To k - 1
                        phi(k, j) = phi(k - 1, j) - pacf * phi(k - 1, k - j)
                    Next
                End If
                For j As Integer = 1 To k
                    rho(j) = phi(k, j)
                Next
            Next

            Return rho
        End Function

        ''' <summary>
        ''' Builds a Toeplitz correlation matrix using visit-index lag distances.
        ''' </summary>
        Protected Function BuildCorrelationToeplitz(indices() As Integer, rho() As Double) As Double(,)
            Dim n As Integer = indices.Length
            Dim out(n - 1, n - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    Dim lag As Integer = Math.Abs(indices(i) - indices(j))
                    If lag >= rho.Length Then
                        Throw New ArgumentException("Toeplitz correlation sequence is shorter than the observed visit lag.")
                    End If
                    out(i, j) = rho(lag)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds <c>D * Corr * D</c> where <c>D = diag(sd)</c>.
        ''' </summary>
        Protected Function MultiplyDiagCorrDiag(stddev() As Double,
                                                corr(,) As Double) As Double(,)
            Dim n As Integer = stddev.Length
            Dim out(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    out(i, j) = stddev(i) * corr(i, j) * stddev(j)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Extracts a subject-specific submatrix from a global visit-indexed covariance matrix.
        ''' </summary>
        Protected Function SubsetGlobalMatrix(globalMat(,) As Double,
                                              indices() As Integer) As Double(,)
            Dim n As Integer = indices.Length
            Dim out(n - 1, n - 1) As Double
            For i = 0 To n - 1
                For j = 0 To n - 1
                    out(i, j) = globalMat(indices(i), indices(j))
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Appends a trace line and writes the same message through the global logger at trace level.
        ''' </summary>
        Protected Sub LogTrace(message As String,
                       Optional ByRef strTrace As String = Nothing)
            If strTrace Is Nothing Then Exit Sub
            If message Is Nothing Then Exit Sub
            If strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
            AppGlobals.BSlogg.Trace(message)
        End Sub

        ''' <summary>
        ''' Appends a trace line and writes the same message through the global logger at debug level.
        ''' </summary>
        Protected Sub LogDebug(message As String,
                               Optional ByRef strTrace As String = Nothing)
            If message Is Nothing Then Exit Sub
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
            AppGlobals.BSlogg.Debug(message)
        End Sub

        ''' <summary>
        ''' Appends a trace line and writes the same message through the global logger at warning level.
        ''' </summary>
        Protected Sub LogWarn(message As String,
                              Optional ByRef strTrace As String = Nothing)
            If message Is Nothing Then Exit Sub
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
            AppGlobals.BSlogg.Warn(message)
        End Sub

    End Class

    ''' <summary>
    ''' Identity residual covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i = σ² I_{n_i}</c></para>
    ''' <para>
    ''' This is the simplest Gaussian residual structure and corresponds to independent
    ''' homoscedastic residuals within subject. In a mixed model with random effects,
    ''' this is often the first baseline structure; in MMRM, it is usually too restrictive,
    ''' but still useful for debugging and as a starting/benchmark fit.
    ''' </para>
    ''' <para>
    ''' Internal parameterization: one free parameter <c>θ = log(σ²)</c>.
    ''' </para>
    ''' </remarks>
    Public Class IdentityR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Identity"
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return 1
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Return {"log_sigma2"}
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Return {SafeLogVarianceStart(olsResidualVar)}
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateThetaLength(theta, 1)
            Dim sigma2 As Double = PositiveScale(theta(0))
            LogTrace($"IdentityR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; sigma2={sigma2}", strTrace)
            Return IdentityMatrix(block.Nobs, sigma2)
        End Function
    End Class

    ''' <summary>
    ''' Visit-specific heterogeneous diagonal residual structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i = diag(σ²_{v_{i1}}, σ²_{v_{i2}}, ..., σ²_{v_{in_i}})</c></para>
    ''' <para>
    ''' Residuals are independent within subject, but each visit can have its own variance.
    ''' This is useful when repeated measurements clearly differ in spread over time, but no
    ''' serial correlation is assumed.
    ''' </para>
    ''' <para>
    ''' Internal parameterization: one log-variance parameter per global visit index.
    ''' For subjects with missing visits, <c>R_i</c> is simply the diagonal submatrix for the
    ''' observed visits.
    ''' </para>
    ''' </remarks>
    Public Class DiagonalHeterogeneousR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Diagonal Heterogeneous"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return VisitDimension(data)
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(m - 1) As String
            For i = 0 To m - 1
                out(i) = $"log_sigma2_visit{i + 1}"
            Next
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(m - 1) As Double
            Dim s As Double = SafeLogVarianceStart(olsResidualVar)
            For i = 0 To m - 1
                out(i) = s
            Next
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m)

            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim n As Integer = block.Nobs
            Dim out(n - 1, n - 1) As Double

            For i = 0 To n - 1
                out(i, i) = PositiveScale(theta(idx(i)))
            Next

            LogTrace($"DiagonalHeterogeneousR.BuildRi subject='{block.SubjectKey}' n={n}; visitDim={m}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Compound-symmetry residual covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i = σ²[(1 - ρ)I + ρJ]</c></para>
    ''' <para>
    ''' where <c>J</c> is the all-ones matrix. All marginal residual variances are equal and all
    ''' within-subject covariances are equal. This is the residual-side analogue of an exchangeable
    ''' correlation structure.
    ''' </para>
    ''' <para>
    ''' Internal parameterization:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><c>θ₁ = log(σ²)</c></description></item>
    ''' <item><description><c>θ₂</c> unconstrained, mapped to <c>ρ = tanh(θ₂)</c></description></item>
    ''' </list>
    ''' </remarks>
    Public Class CompoundSymmetryR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Compound Symmetry"
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return 2
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Return {"log_sigma2", "atanh_rho"}
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Return {SafeLogVarianceStart(olsResidualVar), 0.0}
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateThetaLength(theta, 2)

            Dim sigma2 As Double = PositiveScale(theta(0))
            Dim rho As Double = CorrelationFromTheta(theta(1))
            Dim corr(,) As Double = BuildCorrelationCS(block.Nobs, rho)
            Dim out As Double(,) = MultiplyDiagCorrDiag(BuildRepeatedStdDevVector(block.Nobs, sigma2), corr)

            LogTrace($"CompoundSymmetryR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; sigma2={sigma2}; rho={rho}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Heterogeneous compound-symmetry residual covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para>
    ''' <c>R_i(j,k) = sqrt(σ²_{v_j}) sqrt(σ²_{v_k}) ρ</c> for <c>j ≠ k</c>,
    ''' and <c>R_i(j,j) = σ²_{v_j}</c>.
    ''' </para>
    ''' <para>
    ''' This generalizes compound symmetry by letting each visit have its own variance while keeping
    ''' a common residual correlation among all distinct visit pairs.
    ''' </para>
    ''' </remarks>
    Public Class HeterogeneousCSR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Heterogeneous CS"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return VisitDimension(data) + 1
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(m) As String
            For i = 0 To m - 1
                out(i) = $"log_sigma2_visit{i + 1}"
            Next
            out(m) = "atanh_rho"
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(m) As Double
            Dim s As Double = SafeLogVarianceStart(olsResidualVar)
            For i = 0 To m - 1
                out(i) = s
            Next
            out(m) = 0.0
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m + 1)

            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim rho As Double = CorrelationFromTheta(theta(m))
            Dim corr(,) As Double = BuildCorrelationCS(block.Nobs, rho)
            Dim stddev(block.Nobs - 1) As Double
            For i = 0 To block.Nobs - 1
                stddev(i) = SafeStdDev(PositiveScale(theta(idx(i))))
            Next

            Dim out As Double(,) = MultiplyDiagCorrDiag(stddev, corr)
            LogTrace($"HeterogeneousCSR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; visitDim={m}; rho={rho}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Homogeneous autoregressive residual covariance structure of order 1.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form (visit-indexed):
    ''' </para>
    ''' <para><c>R_i(j,k) = σ² ρ^{|v_j - v_k|}</c></para>
    ''' <para>
    ''' with visit indices <c>v_j</c>, <c>v_k</c>. This is the standard discrete-time AR(1)
    ''' residual structure used in many longitudinal models.
    ''' </para>
    ''' <para>
    ''' Implementation decision: lag is computed from mapped ordinal visit indices, not raw numeric
    ''' time. This is deliberate for v1 because it matches visit-based MMRM use cases and handles
    ''' missing visits via submatrix extraction from a global visit space.
    ''' </para>
    ''' </remarks>
    Public Class AR1R
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "AR(1)"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return 2
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Return {"log_sigma2", "atanh_rho"}
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Return {SafeLogVarianceStart(olsResidualVar), 0.0}
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateThetaLength(theta, 2)

            Dim sigma2 As Double = PositiveScale(theta(0))
            Dim rho As Double = CorrelationFromTheta(theta(1))
            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim corr(,) As Double = BuildCorrelationAR1(idx, rho)
            Dim out As Double(,) = MultiplyDiagCorrDiag(BuildRepeatedStdDevVector(block.Nobs, sigma2), corr)

            LogTrace($"AR1R.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; sigma2={sigma2}; rho={rho}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Heterogeneous autoregressive residual covariance structure of order 1.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i(j,k) = sqrt(σ²_{v_j}) sqrt(σ²_{v_k}) ρ^{|v_j - v_k|}</c></para>
    ''' <para>
    ''' This structure is often a very practical compromise for MMRM-style models because it allows
    ''' the marginal variance to vary by visit while preserving a one-parameter serial-correlation model.
    ''' </para>
    ''' </remarks>
    Public Class HeterogeneousAR1R
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Heterogeneous AR(1)"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return VisitDimension(data) + 1
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(m) As String
            For i = 0 To m - 1
                out(i) = $"log_sigma2_visit{i + 1}"
            Next
            out(m) = "atanh_rho"
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(m) As Double
            Dim s As Double = SafeLogVarianceStart(olsResidualVar)
            For i = 0 To m - 1
                out(i) = s
            Next
            out(m) = 0.0
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m + 1)

            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim rho As Double = CorrelationFromTheta(theta(m))
            Dim corr(,) As Double = BuildCorrelationAR1(idx, rho)
            Dim stddev(block.Nobs - 1) As Double
            For i = 0 To block.Nobs - 1
                stddev(i) = SafeStdDev(PositiveScale(theta(idx(i))))
            Next

            Dim out As Double(,) = MultiplyDiagCorrDiag(stddev, corr)
            LogTrace($"HeterogeneousAR1R.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; visitDim={m}; rho={rho}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Homogeneous Toeplitz residual covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i(j,k) = sigma2 * rho_lag</c>, with <c>rho_0 = 1</c>.</para>
    ''' <para>
    ''' The lag correlations are internally represented by unconstrained partial-autocorrelation
    ''' parameters. This keeps the full Toeplitz correlation matrix positive definite for finite
    ''' optimizer values.
    ''' </para>
    ''' </remarks>
    Public Class ToeplitzR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Toeplitz (TOEP)"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Return VisitDimension(data)
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(m - 1) As String
            out(0) = "log_sigma2"
            For lag As Integer = 1 To m - 1
                out(lag) = $"atanhPartialCorr_lag{lag}"
            Next
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(m - 1) As Double
            out(0) = SafeLogVarianceStart(olsResidualVar)
            For lag As Integer = 1 To m - 1
                out(lag) = 0.0
            Next
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, m)

            Dim sigma2 As Double = PositiveScale(theta(0))
            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim rho() As Double = BuildToeplitzAutocorrelations(theta, m, 1)
            Dim corr(,) As Double = BuildCorrelationToeplitz(idx, rho)
            Dim out As Double(,) = MultiplyDiagCorrDiag(BuildRepeatedStdDevVector(block.Nobs, sigma2), corr)

            LogTrace($"ToeplitzR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; visitDim={m}; sigma2={sigma2}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Heterogeneous Toeplitz residual covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Mathematical form:
    ''' </para>
    ''' <para><c>R_i(j,k) = sd_visit_j * sd_visit_k * rho_lag</c>, with <c>rho_0 = 1</c>.</para>
    ''' <para>
    ''' This is the heterogeneous-variance analogue of Toeplitz and corresponds to commonly used
    ''' repeated-measures structures such as SAS <c>TYPE=TOEPH</c>.
    ''' </para>
    ''' </remarks>
    Public Class HeterogeneousToeplitzR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Heterogeneous Toeplitz (TOEPH)"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Dim m As Integer = VisitDimension(data)
            Return 2 * m - 1
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(2 * m - 2) As String
            For i As Integer = 0 To m - 1
                out(i) = $"log_sigma2_visit{i + 1}"
            Next
            For lag As Integer = 1 To m - 1
                out(m + lag - 1) = $"atanhPartialCorr_lag{lag}"
            Next
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(2 * m - 2) As Double
            Dim s As Double = SafeLogVarianceStart(olsResidualVar)
            For i As Integer = 0 To m - 1
                out(i) = s
            Next
            For lag As Integer = 1 To m - 1
                out(m + lag - 1) = 0.0
            Next
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, 2 * m - 1)

            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim rho() As Double = BuildToeplitzAutocorrelations(theta, m, m)
            Dim corr(,) As Double = BuildCorrelationToeplitz(idx, rho)
            Dim stddev(block.Nobs - 1) As Double
            For i As Integer = 0 To block.Nobs - 1
                stddev(i) = SafeStdDev(PositiveScale(theta(idx(i))))
            Next

            Dim out As Double(,) = MultiplyDiagCorrDiag(stddev, corr)
            LogTrace($"HeterogeneousToeplitzR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; visitDim={m}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Fully unstructured residual covariance parameterized through a lower-triangular Cholesky factor.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' A global visit covariance matrix <c>R_full</c> is parameterized as:
    ''' </para>
    ''' <para><c>R_full = L L'</c></para>
    ''' <para>
    ''' where <c>L</c> is lower-triangular. Its diagonal entries are constrained positive via
    ''' log-scale parameterization, and off-diagonal entries are left unconstrained. This guarantees
    ''' that <c>R_full</c> is symmetric positive definite for any finite optimizer vector.
    ''' </para>
    ''' <para>
    ''' For each subject, the observed block covariance <c>R_i</c> is obtained by taking the submatrix
    ''' corresponding to that subject's observed visit indices. This makes the structure naturally suited
    ''' to monotone or intermittent missingness patterns seen in MMRM.
    ''' </para>
    ''' </remarks>
    Public Class UnstructuredR
        Inherits MixedModelRStruct

        Public Overrides Function ToString() As String
            Return "Unstructured"
        End Function

        Public Overrides Function UsesVisitIndex() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(data As MixedModelBlockData) As Integer
            Dim m As Integer = VisitDimension(data)
            Return m * (m + 1) \ 2
        End Function

        Public Overrides Function ParamNames(data As MixedModelBlockData) As String()
            Dim m As Integer = VisitDimension(data)
            Dim out(Me.ParamCount(data) - 1) As String
            Dim k As Integer = 0
            For i = 0 To m - 1
                For j = 0 To i
                    If i = j Then
                        out(k) = $"log_chol_diag_{i + 1}"
                    Else
                        out(k) = $"chol_{i + 1}_{j + 1}"
                    End If
                    k += 1
                Next
            Next
            Return out
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            Dim m As Integer = VisitDimension(data)
            Dim out(Me.ParamCount(data) - 1) As Double
            Dim k As Integer = 0
            Dim diagStart As Double = 0.5 * SafeLogVarianceStart(olsResidualVar)

            For i = 0 To m - 1
                For j = 0 To i
                    If i = j Then
                        out(k) = diagStart
                    Else
                        out(k) = 0.0
                    End If
                    k += 1
                Next
            Next
            Return out
        End Function

        Public Overrides Function BuildRi(theta() As Double,
                                          block As MixedModelSubjectBlock,
                                          data As MixedModelBlockData,
                                          Optional ByRef strTrace As String = Nothing) As Double(,)
            Dim m As Integer = VisitDimension(data)
            ValidateThetaLength(theta, ParamCount(data))

            Dim L(m - 1, m - 1) As Double
            Dim k As Integer = 0
            For i = 0 To m - 1
                For j = 0 To i
                    If i = j Then
                        L(i, j) = Math.Exp(theta(k))
                    Else
                        L(i, j) = theta(k)
                    End If
                    k += 1
                Next
            Next

            Dim fullR(m - 1, m - 1) As Double
            For i = 0 To m - 1
                For j = 0 To m - 1
                    Dim s As Double = 0.0
                    Dim upper As Integer = Math.Min(i, j)
                    For h = 0 To upper
                        s += L(i, h) * L(j, h)
                    Next
                    fullR(i, j) = s
                Next
            Next

            Dim idx() As Integer = GetBlockVisitIndices(block, data, strTrace)
            Dim out As Double(,) = SubsetGlobalMatrix(fullR, idx)
            LogTrace($"UnstructuredR.BuildRi subject='{block.SubjectKey}' n={block.Nobs}; visitDim={m}; thetaCount={theta.Length}", strTrace)
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Small helper for creating repeated standard-deviation vectors used by homogeneous structures.
    ''' </summary>
    Friend Module MixedModelRStructHelpers

        Friend Function BuildRepeatedStdDevVector(n As Integer, sigma2 As Double) As Double()
            Dim out(n - 1) As Double
            Dim sd As Double = Math.Sqrt(sigma2)
            For i = 0 To n - 1
                out(i) = sd
            Next
            Return out
        End Function

    End Module

End Namespace
