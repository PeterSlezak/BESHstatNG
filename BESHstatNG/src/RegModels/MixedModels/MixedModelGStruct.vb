Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace regression

    ''' <summary>
    ''' Factory/helper routines for creating G-side random-effects covariance structures
    ''' used by the Gaussian mixed-model engine (LMM/MMRM).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is the G-side companion to <c>MixedModelRStructUtils</c>. It converts
    ''' user-facing structure names into concrete implementations of <see cref="MixedModelGStruct"/>.
    ''' </para>
    ''' <para>
    ''' In a Gaussian mixed model the subject-block covariance is decomposed as:
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' The classes in this file define the covariance matrix <c>G</c> of the subject-specific
    ''' random effects <c>b_i</c>. They therefore control the contribution <c>Z_i G Z_i'</c>
    ''' to the marginal covariance of repeated observations from subject <c>i</c>.
    ''' </para>
    ''' <para>
    ''' For MMRM, the same engine is reused with no G-side contribution; in that case
    ''' <c>Z_i</c> is absent and <c>G</c> is represented by <see cref="NoRandomEffects"/>.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelGStructUtils

        ''' <summary>
        ''' Creates a concrete G-side covariance structure instance from a user-facing name.
        ''' </summary>
        ''' <param name="type">
        ''' Structure name. Matching is case-insensitive and supports short aliases such as
        ''' <c>RI</c>, <c>RI+S</c>, <c>UN</c>, and <c>NONE</c>.
        ''' </param>
        ''' <returns>A new <see cref="MixedModelGStruct"/> implementation.</returns>
        ''' <exception cref="ApplicationException">Thrown when the structure name is unsupported.</exception>
        Public Function createMixedModelGStruct(type As String) As MixedModelGStruct
            Dim f As MixedModelGStruct
            Dim normalized As String = If(type, String.Empty).Trim().ToLowerInvariant()

            If normalized = String.Empty OrElse normalized = "none" OrElse normalized = "no random effects" _
                OrElse normalized = "mmrm" Then
                f = New NoRandomEffects()
            ElseIf normalized = "random intercept" OrElse normalized = "ri" OrElse normalized = "intercept" Then
                f = New RandomIntercept()
            ElseIf normalized = "random intercept + slope" OrElse normalized = "random intercept+slope" _
                OrElse normalized = "random intercept and slope" OrElse normalized = "ri+s" _
                OrElse normalized = "ris" Then
                f = New RandomInterceptSlope()
            ElseIf normalized = "unstructured random effects" OrElse normalized = "unstructured" _
                OrElse normalized = "un" Then
                f = New UnstructuredRandomEffects()
            Else
                AppGlobals.BSlogg.Error($"Unsupported mixed-model random-effects covariance structure. type='{type}'")
                Throw New ApplicationException("Unsupported mixed-model random-effects covariance structure. type = " & type)
            End If

            AppGlobals.BSlogg.Trace($"createMixedModelGStruct created {f.ToString()} for type='{type}'")
            Return f
        End Function

    End Module

    ''' <summary>
    ''' Abstract base class for all G-side random-effects covariance structures.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' If a Gaussian mixed model is written as
    ''' <c>y_i = X_i β + Z_i b_i + ε_i</c>, with <c>b_i ~ N(0, G)</c> and
    ''' <c>ε_i ~ N(0, R_i)</c>, then the marginal covariance of subject <c>i</c> is
    ''' </para>
    ''' <para><c>V_i = Z_i G Z_i' + R_i</c></para>
    ''' <para>
    ''' This class is responsible only for constructing the positive-semidefinite matrix <c>G</c>.
    ''' In the initial implementation, the concrete subclasses focus on the most useful families:
    ''' no random effects, random intercept only, random intercept plus slope, and fully
    ''' unstructured random-effects covariance.
    ''' </para>
    ''' <para>
    ''' Parameterization conventions in this file:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Variance-like quantities are represented internally on the log scale so that <c>σ² = exp(θ)</c> is always positive.</description></item>
    ''' <item><description>Two-dimensional intercept/slope covariance is parameterized by log standard deviations and an unconstrained correlation mapped by <c>ρ = tanh(θ)</c>.</description></item>
    ''' <item><description>General unstructured covariance is parameterized through a lower-triangular Cholesky factor <c>L</c> with log-diagonal entries, so that <c>G = L L'</c> is symmetric positive-definite by construction.</description></item>
    ''' <item><description>Every structure emits trace/debug/warn messages both to <see cref="AppGlobals.BSlogg"/> and to an optional in-memory <c>strTrace</c> accumulator. This mirrors the debugging style used elsewhere in the project and makes it easier to surface details later in the UI or result tables.</description></item>
    ''' </list>
    ''' </remarks>
    Public MustInherit Class MixedModelGStruct

        ''' <summary>
        ''' User-facing list of currently implemented G-side covariance structures.
        ''' </summary>
        Public Shared GStructsList() As String =
            {"None", "Random Intercept", "Random Intercept + Slope", "Unstructured Random Effects"}

        ''' <summary>
        ''' Returns the display name of the structure.
        ''' </summary>
        Public MustOverride Overrides Function ToString() As String

        ''' <summary>
        ''' Returns <c>True</c> when the structure implies the absence of random effects.
        ''' </summary>
        Public Overridable Function IsDegenerateZeroG() As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Returns the number of free optimizer parameters needed for a random-effects design
        ''' with <paramref name="q"/> columns.
        ''' </summary>
        ''' <param name="q">Number of columns in the random-effects design matrix <c>Z</c>.</param>
        Public MustOverride Function ParamCount(q As Integer) As Integer

        ''' <summary>
        ''' Returns human-readable parameter names aligned with <see cref="ParamCount"/>.
        ''' </summary>
        ''' <param name="q">Number of random-effects columns.</param>
        ''' <param name="randomEffectNames">
        ''' Optional names for random-effects columns, typically derived from the random-effects design matrix.
        ''' </param>
        Public MustOverride Function ParamNames(q As Integer,
                                                Optional randomEffectNames() As String = Nothing) As String()

        ''' <summary>
        ''' Returns conservative starting values on the internal optimizer scale.
        ''' </summary>
        ''' <param name="data">Blocked mixed-model data.</param>
        ''' <param name="olsResidualVar">
        ''' Working residual-variance scale, usually taken from a simpler OLS/GLS fit.
        ''' </param>
        Public MustOverride Function StartParams(data As MixedModelBlockData,
                                                 Optional olsResidualVar As Double = 1.0) As Double()

        ''' <summary>
        ''' Builds the random-effects covariance matrix <c>G</c>.
        ''' </summary>
        ''' <param name="theta">Parameter vector on the internal optimizer scale.</param>
        ''' <param name="q">Number of random-effects columns in <c>Z</c>.</param>
        ''' <param name="strTrace">
        ''' Optional in-memory trace accumulator. Messages are also written to the application logger.
        ''' </param>
        ''' <returns>
        ''' A <c>q × q</c> covariance matrix, or <c>Nothing</c> for the degenerate no-random-effects case.
        ''' </returns>
        Public MustOverride Function BuildG(theta() As Double,
                                            q As Integer,
                                            Optional ByRef strTrace As String = Nothing) As Double(,)

        ''' <summary>
        ''' Validates that the supplied random-effects dimension is compatible with the structure.
        ''' </summary>
        Protected Sub ValidateQ(q As Integer,
                                Optional minQ As Integer = 0,
                                Optional maxQ As Integer = Integer.MaxValue)
            If q < minQ OrElse q > maxQ Then
                Throw New ApplicationException($"{ToString()} requires random-effects dimension q in [{minQ}, {maxQ}], but q={q}.")
            End If
        End Sub

        ''' <summary>
        ''' Returns a safe user-facing label for a random-effects column.
        ''' </summary>
        Protected Function GetRandomEffectName(index As Integer,
                                               Optional randomEffectNames() As String = Nothing) As String
            If randomEffectNames Is Nothing OrElse index < 0 OrElse index >= randomEffectNames.Length Then
                Return "b" & (index + 1).ToString()
            End If
            If String.IsNullOrWhiteSpace(randomEffectNames(index)) Then
                Return "b" & (index + 1).ToString()
            End If
            Return randomEffectNames(index).Trim()
        End Function

        ''' <summary>
        ''' Maps an unconstrained internal parameter to a positive variance or standard deviation scale.
        ''' </summary>
        Protected Function ExpMap(x As Double) As Double
            Return Math.Exp(x)
        End Function

        ''' <summary>
        ''' Maps an unconstrained internal parameter to a correlation in (-1, 1).
        ''' </summary>
        Protected Function CorrMap(x As Double) As Double
            Return Math.Tanh(x)
        End Function

        ''' <summary>
        ''' Builds a symmetric matrix <c>G = L L'</c> from packed lower-triangular Cholesky parameters.
        ''' </summary>
        ''' <param name="theta">
        ''' Packed lower-triangular parameters. Diagonal elements are on the log scale; off-diagonal
        ''' elements are used directly.
        ''' </param>
        ''' <param name="q">Target dimension.</param>
        Protected Function BuildSPDFromCholeskyParams(theta() As Double, q As Integer) As Double(,)
            Dim expected As Integer = q * (q + 1) \ 2
            If theta Is Nothing OrElse theta.Length <> expected Then
                Throw New ApplicationException($"Invalid theta length for unstructured random-effects covariance. Expected {expected}, got {If(theta Is Nothing, 0, theta.Length)}.")
            End If

            Dim lMat(q - 1, q - 1) As Double
            Dim k As Integer = 0
            For i As Integer = 0 To q - 1
                For j As Integer = 0 To i
                    If i = j Then
                        lMat(i, j) = Math.Exp(theta(k))
                    Else
                        lMat(i, j) = theta(k)
                    End If
                    k += 1
                Next
            Next

            Dim gMat(q - 1, q - 1) As Double
            For i As Integer = 0 To q - 1
                For j As Integer = 0 To q - 1
                    Dim s As Double = 0.0
                    Dim maxK As Integer = Math.Min(i, j)
                    For t As Integer = 0 To maxK
                        s += lMat(i, t) * lMat(j, t)
                    Next
                    gMat(i, j) = s
                Next
            Next
            Return gMat
        End Function

        ''' <summary>
        ''' Appends a trace message to the optional in-memory buffer and to the application trace logger.
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
        ''' Appends a debug message to the optional in-memory buffer and to the application debug logger.
        ''' </summary>
        Protected Sub LogDebug(message As String,
                               Optional ByRef strTrace As String = Nothing)
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
            AppGlobals.BSlogg.Debug(message)
        End Sub

        ''' <summary>
        ''' Appends a warning message to the optional in-memory buffer and to the application warning logger.
        ''' </summary>
        Protected Sub LogWarn(message As String,
                              Optional ByRef strTrace As String = Nothing)
            If strTrace Is Nothing OrElse strTrace = String.Empty Then
                strTrace = message
            Else
                strTrace &= vbNewLine & message
            End If
            AppGlobals.BSlogg.Warn(message)
        End Sub

    End Class

    ''' <summary>
    ''' Degenerate G-side structure representing the absence of random effects.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This structure is primarily useful for MMRM and for any Gaussian repeated-measures model
    ''' whose full covariance is represented on the R-side.
    ''' </para>
    ''' <para>
    ''' Mathematically, the contribution to the marginal covariance is simply
    ''' <c>Z_i G Z_i' = 0</c>.
    ''' </para>
    ''' </remarks>
    Public Class NoRandomEffects
        Inherits MixedModelGStruct

        Public Overrides Function ToString() As String
            Return "None"
        End Function

        Public Overrides Function IsDegenerateZeroG() As Boolean
            Return True
        End Function

        Public Overrides Function ParamCount(q As Integer) As Integer
            Return 0
        End Function

        Public Overrides Function ParamNames(q As Integer, Optional randomEffectNames() As String = Nothing) As String()
            Return Array.Empty(Of String)()
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData, Optional olsResidualVar As Double = 1.0) As Double()
            Return Array.Empty(Of Double)()
        End Function

        Public Overrides Function BuildG(theta() As Double,
                                         q As Integer,
                                         Optional ByRef strTrace As String = Nothing) As Double(,)
            If theta IsNot Nothing AndAlso theta.Length > 0 Then
                LogWarn($"NoRandomEffects.BuildG received a non-empty theta vector of length {theta.Length}; it will be ignored.", strTrace)
            End If
            LogTrace("NoRandomEffects.BuildG returns Nothing (no G-side contribution).", strTrace)
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Random-intercept covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This is the simplest non-trivial mixed-effects covariance structure. The random effects vector
    ''' contains exactly one element per subject, usually interpreted as a subject-specific intercept:
    ''' </para>
    ''' <para><c>b_i ~ N(0, σ_b²)</c></para>
    ''' <para>
    ''' so that
    ''' </para>
    ''' <para><c>G = [σ_b²]</c></para>
    ''' <para>
    ''' and the induced covariance contribution is <c>σ_b² 1 1'</c> when the first column of
    ''' <c>Z_i</c> is an intercept vector.
    ''' </para>
    ''' <para>
    ''' In this v1 implementation the structure requires <c>q = 1</c>. If the random-effects design
    ''' matrix has more than one column, the caller should instead choose a structure whose dimension
    ''' matches that design.
    ''' </para>
    ''' </remarks>
    Public Class RandomIntercept
        Inherits MixedModelGStruct

        Public Overrides Function ToString() As String
            Return "Random Intercept"
        End Function

        Public Overrides Function ParamCount(q As Integer) As Integer
            ValidateQ(q, 1, 1)
            Return 1
        End Function

        Public Overrides Function ParamNames(q As Integer,
                                             Optional randomEffectNames() As String = Nothing) As String()
            ValidateQ(q, 1, 1)
            Return {"logVar(" & GetRandomEffectName(0, randomEffectNames) & ")"}
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Q <> 1 Then
                Throw New ApplicationException($"RandomIntercept.StartParams expects data.Q = 1, but found {data.Q}.")
            End If
            Dim baseVar As Double = Math.Max(olsResidualVar, 1.0E-6)
            Return {Math.Log(baseVar / 2.0)}
        End Function

        Public Overrides Function BuildG(theta() As Double,
                                         q As Integer,
                                         Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateQ(q, 1, 1)
            If theta Is Nothing OrElse theta.Length <> 1 Then
                Throw New ApplicationException($"RandomIntercept.BuildG expects theta length = 1, but found {If(theta Is Nothing, 0, theta.Length)}.")
            End If

            Dim varB As Double = ExpMap(theta(0))
            Dim gMat(0, 0) As Double
            gMat(0, 0) = varB

            LogTrace($"RandomIntercept.BuildG q=1; varB={varB}", strTrace)
            Return gMat
        End Function
    End Class

    ''' <summary>
    ''' Random-intercept plus random-slope covariance structure.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The random-effects vector contains two subject-specific effects, typically an intercept and one slope:
    ''' </para>
    ''' <para><c>b_i = (b_{0i}, b_{1i})'</c></para>
    ''' <para>
    ''' with covariance matrix
    ''' </para>
    ''' <para>
    ''' <c>
    ''' G = [ σ₀²              ρ σ₀ σ₁ ]
    '''     [ ρ σ₀ σ₁          σ₁²     ]
    ''' </c>
    ''' </para>
    ''' <para>
    ''' Internally the parameters are represented as:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><c>θ₁ = log σ₀</c></description></item>
    ''' <item><description><c>θ₂ = log σ₁</c></description></item>
    ''' <item><description><c>θ₃</c> unconstrained, mapped to <c>ρ = tanh(θ₃)</c></description></item>
    ''' </list>
    ''' <para>
    ''' This representation makes the resulting covariance matrix automatically valid.
    ''' </para>
    ''' <para>
    ''' In the initial implementation the structure requires <c>q = 2</c> exactly. That keeps the engine
    ''' behavior explicit and avoids silently ignoring extra columns in <c>Z</c>.
    ''' </para>
    ''' </remarks>
    Public Class RandomInterceptSlope
        Inherits MixedModelGStruct

        Public Overrides Function ToString() As String
            Return "Random Intercept + Slope"
        End Function

        Public Overrides Function ParamCount(q As Integer) As Integer
            ValidateQ(q, 2, 2)
            Return 3
        End Function

        Public Overrides Function ParamNames(q As Integer,
                                             Optional randomEffectNames() As String = Nothing) As String()
            ValidateQ(q, 2, 2)
            Dim n0 As String = GetRandomEffectName(0, randomEffectNames)
            Dim n1 As String = GetRandomEffectName(1, randomEffectNames)
            Return {
                "logSD(" & n0 & ")",
                "logSD(" & n1 & ")",
                "atanhCorr(" & n0 & "," & n1 & ")"
            }
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Q <> 2 Then
                Throw New ApplicationException($"RandomInterceptSlope.StartParams expects data.Q = 2, but found {data.Q}.")
            End If

            Dim baseSD As Double = Math.Sqrt(Math.Max(olsResidualVar, 1.0E-6))
            Return {
                Math.Log(Math.Max(baseSD / 2.0, 1.0E-6)),
                Math.Log(Math.Max(baseSD / 4.0, 1.0E-6)),
                0.0
            }
        End Function

        Public Overrides Function BuildG(theta() As Double,
                                         q As Integer,
                                         Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateQ(q, 2, 2)
            If theta Is Nothing OrElse theta.Length <> 3 Then
                Throw New ApplicationException($"RandomInterceptSlope.BuildG expects theta length = 3, but found {If(theta Is Nothing, 0, theta.Length)}.")
            End If

            Dim sd0 As Double = ExpMap(theta(0))
            Dim sd1 As Double = ExpMap(theta(1))
            Dim rho As Double = CorrMap(theta(2))
            Dim cov01 As Double = rho * sd0 * sd1

            Dim gMat(1, 1) As Double
            gMat(0, 0) = sd0 * sd0
            gMat(1, 1) = sd1 * sd1
            gMat(0, 1) = cov01
            gMat(1, 0) = cov01

            LogTrace($"RandomInterceptSlope.BuildG q=2; sd0={sd0}; sd1={sd1}; rho={rho}", strTrace)
            Return gMat
        End Function
    End Class

    ''' <summary>
    ''' Fully unstructured random-effects covariance.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This is the most general G-side structure supported in the initial implementation. For a
    ''' <c>q</c>-dimensional random-effects vector, the covariance matrix contains <c>q(q+1)/2</c>
    ''' free parameters and is built as
    ''' </para>
    ''' <para><c>G = L L'</c></para>
    ''' <para>
    ''' where <c>L</c> is a lower-triangular Cholesky factor. The diagonal entries of <c>L</c>
    ''' are stored on the log scale and exponentiated during reconstruction to guarantee positive
    ''' definiteness of <c>G</c>.
    ''' </para>
    ''' <para>
    ''' This is the right general-purpose structure when the random-effects design has more than two
    ''' columns or when the intercept/slope pattern is not the only scientifically plausible option.
    ''' </para>
    ''' </remarks>
    Public Class UnstructuredRandomEffects
        Inherits MixedModelGStruct

        Public Overrides Function ToString() As String
            Return "Unstructured Random Effects"
        End Function

        Public Overrides Function ParamCount(q As Integer) As Integer
            ValidateQ(q, 1)
            Return q * (q + 1) \ 2
        End Function

        Public Overrides Function ParamNames(q As Integer,
                                             Optional randomEffectNames() As String = Nothing) As String()
            ValidateQ(q, 1)
            Dim names(ParamCount(q) - 1) As String
            Dim k As Integer = 0
            For i As Integer = 0 To q - 1
                For j As Integer = 0 To i
                    If i = j Then
                        names(k) = "logCholDiag(" & GetRandomEffectName(i, randomEffectNames) & ")"
                    Else
                        names(k) = "chol(" & GetRandomEffectName(i, randomEffectNames) & "," & GetRandomEffectName(j, randomEffectNames) & ")"
                    End If
                    k += 1
                Next
            Next
            Return names
        End Function

        Public Overrides Function StartParams(data As MixedModelBlockData,
                                              Optional olsResidualVar As Double = 1.0) As Double()
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Q < 1 Then
                Throw New ApplicationException($"UnstructuredRandomEffects.StartParams requires data.Q >= 1, but found {data.Q}.")
            End If

            Dim q As Integer = data.Q
            Dim theta(ParamCount(q) - 1) As Double
            Dim baseSD As Double = Math.Sqrt(Math.Max(olsResidualVar, 1.0E-6))
            Dim k As Integer = 0
            For i As Integer = 0 To q - 1
                For j As Integer = 0 To i
                    If i = j Then
                        theta(k) = Math.Log(Math.Max(baseSD / 2.0, 1.0E-6))
                    Else
                        theta(k) = 0.0
                    End If
                    k += 1
                Next
            Next
            Return theta
        End Function

        Public Overrides Function BuildG(theta() As Double,
                                         q As Integer,
                                         Optional ByRef strTrace As String = Nothing) As Double(,)
            ValidateQ(q, 1)
            Dim gMat(,) As Double = BuildSPDFromCholeskyParams(theta, q)
            LogTrace($"UnstructuredRandomEffects.BuildG q={q}; thetaCount={If(theta Is Nothing, 0, theta.Length)}", strTrace)
            Return gMat
        End Function
    End Class

End Namespace
