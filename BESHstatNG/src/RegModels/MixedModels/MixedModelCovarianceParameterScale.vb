Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Namespace regression

    ''' <summary>
    ''' Supported parameter scale used by the Kenward-Roger derivative backend.
    ''' </summary>
    Public Enum MixedModelKrParameterScale
        ''' <summary>
        ''' Use the optimizer's unconstrained internal parameters directly. This is the
        ''' legacy path and is kept as a fallback.
        ''' </summary>
        OptimizerInternal = 0

        ''' <summary>
        ''' Use statistically interpretable covariance parameters: variances,
        ''' covariances, and selected correlations where the fitted structure is not
        ''' represented by a full covariance matrix.
        ''' </summary>
        Covariance = 1

        ''' <summary>
        ''' Use the R mmrm-compatible theta scale for MMRM KR calculations. For
        ''' variance/correlation structures this uses log standard deviations plus
        ''' R mmrm's correlation transform; for UnstructuredR this means log Cholesky
        ''' diagonals first, followed by row-normalized Cholesky off-diagonals Lij / Lii.
        ''' Unsupported structures fall back to the legacy optimizer theta.
        ''' </summary>
        MmrmTheta = 2
    End Enum


    ''' <summary>
    ''' KR covariance adjustment requested by the caller.
    ''' </summary>
    Public Enum MixedModelKenwardRogerAdjustmentKind
        None = 0
        Linear = 1
        Full = 2
    End Enum


    ''' <summary>
    ''' Caller preference for the covariance-parameter scale used by the KR derivative
    ''' workspace.
    ''' </summary>
    Public Enum MixedModelKenwardRogerParameterScalePreference
        Automatic = 0
        OptimizerInternal = 1
        MmrmTheta = 2
        Covariance = 3
    End Enum

    ''' <summary>
    ''' Numerical finite-difference settings used when the Kenward-Roger derivative
    ''' workspace builds dV/dtheta and d2V/dtheta_h dtheta_j by perturbing covariance
    ''' parameters.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The defaults intentionally match the adaptive Richardson finite-difference
    ''' settings used by the engine before this option object was introduced.
    ''' Exposing them on the KR contract makes R-parity tuning reproducible without
    ''' editing engine constants.
    ''' </para>
    ''' <para>
    ''' Step sizes are relative to <c>max(abs(theta_h), 1)</c> and then clamped between
    ''' <see cref="MinimumStep"/> and <see cref="MaximumStep"/>.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelKenwardRogerFiniteDifferenceOptions
        Public Property FirstDerivativeStepScale As Double = 0.0001
        Public Property SecondDerivativeStepScale As Double = 0.00025
        Public Property MinimumStep As Double = 0.0000001
        Public Property MaximumStep As Double = 0.01
        Public Property MaxStepHalvings As Integer = 8
        Public Property UseRichardsonRefinement As Boolean = True
        Public Property AllowOneSidedFirstDerivativeFallback As Boolean = True
        Public Property RichardsonWarningRelativeTolerance As Double = 0.25
        Public Property EmitPerturbedViCacheDiagnostics As Boolean = True

        Public Shared Function CreateDefault() As MixedModelKenwardRogerFiniteDifferenceOptions
            Return New MixedModelKenwardRogerFiniteDifferenceOptions()
        End Function

        Public Function Clone() As MixedModelKenwardRogerFiniteDifferenceOptions
            Return New MixedModelKenwardRogerFiniteDifferenceOptions With {
                .FirstDerivativeStepScale = Me.FirstDerivativeStepScale,
                .SecondDerivativeStepScale = Me.SecondDerivativeStepScale,
                .MinimumStep = Me.MinimumStep,
                .MaximumStep = Me.MaximumStep,
                .MaxStepHalvings = Me.MaxStepHalvings,
                .UseRichardsonRefinement = Me.UseRichardsonRefinement,
                .AllowOneSidedFirstDerivativeFallback = Me.AllowOneSidedFirstDerivativeFallback,
                .RichardsonWarningRelativeTolerance = Me.RichardsonWarningRelativeTolerance,
                .EmitPerturbedViCacheDiagnostics = Me.EmitPerturbedViCacheDiagnostics
            }
        End Function

        Public Sub Validate()
            If Not IsFinitePositive(FirstDerivativeStepScale) Then FirstDerivativeStepScale = 0.0001
            If Not IsFinitePositive(SecondDerivativeStepScale) Then SecondDerivativeStepScale = 0.00025
            If Not IsFinitePositive(MinimumStep) Then MinimumStep = 0.0000001
            If Not IsFinitePositive(MaximumStep) Then MaximumStep = 0.01
            If MaximumStep < MinimumStep Then MaximumStep = MinimumStep
            If MaxStepHalvings < 0 Then MaxStepHalvings = 0
            If MaxStepHalvings > 20 Then MaxStepHalvings = 20
            If Not IsFinitePositive(RichardsonWarningRelativeTolerance) Then RichardsonWarningRelativeTolerance = 0.25
        End Sub

        Private Shared Function IsFinitePositive(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0
        End Function
    End Class

    ''' <summary>
    ''' Central Kenward-Roger configuration: whether KR is enabled, whether linear or
    ''' full covariance adjustment is requested, and which covariance-parameter scale
    ''' the derivative backend should use.
    ''' </summary>
    Public Class MixedModelKenwardRogerOptions
        Public Property Enabled As Boolean = False
        Public Property Adjustment As MixedModelKenwardRogerAdjustmentKind = MixedModelKenwardRogerAdjustmentKind.Full
        Public Property ParameterScalePreference As MixedModelKenwardRogerParameterScalePreference = MixedModelKenwardRogerParameterScalePreference.Automatic
        Public Property RequireReml As Boolean = True
        Public Property AllowLinearFallback As Boolean = False
        Public Property StrictValidation As Boolean = False
        Public Property FiniteDifferenceOptions As MixedModelKenwardRogerFiniteDifferenceOptions = MixedModelKenwardRogerFiniteDifferenceOptions.CreateDefault()
        Public Shared Function CreateDefault() As MixedModelKenwardRogerOptions
            Return New MixedModelKenwardRogerOptions()
        End Function

        Public Shared Function CreateFullMmrm() As MixedModelKenwardRogerOptions
            Return New MixedModelKenwardRogerOptions With {
                .Enabled = True,
                .Adjustment = MixedModelKenwardRogerAdjustmentKind.Full,
                .ParameterScalePreference = MixedModelKenwardRogerParameterScalePreference.MmrmTheta,
                .RequireReml = True,
                .AllowLinearFallback = False,
                .StrictValidation = False
            }
        End Function

        ''' <summary>
        ''' Creates the full KR contract intended for LMM inference. LMM uses the
        ''' direct covariance-parameter scale, unlike MMRM where the R mmrm-compatible
        ''' theta scale is required.
        ''' </summary>
        Public Shared Function CreateFullLmm() As MixedModelKenwardRogerOptions
            Return New MixedModelKenwardRogerOptions With {
                .Enabled = True,
                .Adjustment = MixedModelKenwardRogerAdjustmentKind.Full,
                .ParameterScalePreference = MixedModelKenwardRogerParameterScalePreference.Covariance,
                .RequireReml = True,
                .AllowLinearFallback = False,
                .StrictValidation = True
             }
        End Function

        Public Function Clone() As MixedModelKenwardRogerOptions
            Return New MixedModelKenwardRogerOptions With {
                .Enabled = Me.Enabled,
                .Adjustment = Me.Adjustment,
                .ParameterScalePreference = Me.ParameterScalePreference,
                .RequireReml = Me.RequireReml,
                .AllowLinearFallback = Me.AllowLinearFallback,
                .StrictValidation = Me.StrictValidation,
                 .FiniteDifferenceOptions = If(Me.FiniteDifferenceOptions Is Nothing, MixedModelKenwardRogerFiniteDifferenceOptions.CreateDefault(), Me.FiniteDifferenceOptions.Clone())
            }
        End Function
    End Class


    ''' <summary>
    ''' Resolved KR parameter mapping used by the engine after fitting. It separates
    ''' optimizer theta from the theta actually used by KR derivatives.
    ''' </summary>
    Public Class MixedModelKrParameterMap
        Public Property OptimizerTheta As Double() = Nothing
        Public Property OptimizerThetaCovariance As Double(,) = Nothing
        Public Property KrTheta As Double() = Nothing
        Public Property KrThetaCovariance As Double(,) = Nothing
        Public Property OptimizerToKrJacobian As Double(,) = Nothing
        Public Property ParameterNames As String() = Nothing
        Public Property ParameterScale As MixedModelKrParameterScale = MixedModelKrParameterScale.OptimizerInternal

        ''' <summary>
        ''' True when <see cref="KrTheta"/> is an R mmrm-style theta vector that must
        ''' be converted back to the optimizer theta before calling the engine's
        ''' residual-covariance builders.
        ''' </summary>
        Public Property RequiresMmrmThetaBackTransform As Boolean = False

        Public Property DiagnosticMessage As String = String.Empty
    End Class

    ''' <summary>
    ''' Mapping between the optimizer parameter vector and the covariance-parameter
    ''' vector used by the Kenward-Roger derivative backend.
    ''' </summary>
    Public Class MixedModelCovarianceScaleWorkspace

        Public Property OptimizerTheta As Double() = Nothing
        Public Property CovarianceTheta As Double() = Nothing
        Public Property CovarianceThetaNames As String() = Nothing

        ''' <summary>
        ''' Jacobian J = d covarianceTheta / d optimizerTheta.
        ''' </summary>
        Public Property OptimizerToCovarianceJacobian As Double(,) = Nothing

        Public Property ParameterScale As MixedModelKrParameterScale = MixedModelKrParameterScale.Covariance
        Public Property DiagnosticMessage As String = String.Empty

    End Class


    ''' <summary>
    ''' Utilities for separating the mixed-model optimizer parameterization from the
    ''' covariance-parameter scale used by KR derivatives.
    ''' </summary>
    Public Module MixedModelCovarianceParameterScale

        Private Const MIN_POSITIVE As Double = 1.0E-10
        Private Const MAX_ABS_RHO As Double = 0.999999

        ''' <summary>
        ''' Resolves the KR derivative parameter scale for a fitted request. For MMRM,
        ''' the automatic/default path is the R mmrm-compatible theta scale rather than
        ''' direct covariance elements. Full KR derivatives are then finite-differenced
        ''' on that R mmrm-compatible theta scale, including the second-derivative term.
        ''' </summary>
        Public Function TryCreateParameterMap(request As MixedModelFitRequest,
                                             optimizerTheta() As Double,
                                             optimizerThetaCovariance(,) As Double,
                                             options As MixedModelKenwardRogerOptions,
                                             ByRef map As MixedModelKrParameterMap,
                                             Optional ByRef diagnostic As String = Nothing) As Boolean
            map = Nothing
            diagnostic = String.Empty

            If request Is Nothing Then
                diagnostic = "Cannot create KR parameter map: request is Nothing."
                Return False
            End If

            If optimizerTheta Is Nothing OrElse optimizerTheta.Length = 0 Then
                diagnostic = "Cannot create KR parameter map: optimizer theta is empty."
                Return False
            End If

            If optimizerThetaCovariance Is Nothing OrElse
               optimizerThetaCovariance.GetLength(0) <> optimizerTheta.Length OrElse
               optimizerThetaCovariance.GetLength(1) <> optimizerTheta.Length Then
                diagnostic = "Cannot create KR parameter map: optimizer theta covariance is missing or has incompatible dimensions."
                Return False
            End If

            Dim opt As MixedModelKenwardRogerOptions = If(options, MixedModelKenwardRogerOptions.CreateDefault())
            Dim preference As MixedModelKenwardRogerParameterScalePreference = ResolveParameterScalePreference(request, opt)

            If preference = MixedModelKenwardRogerParameterScalePreference.MmrmTheta Then
                Dim mmrmMap As MixedModelKrParameterMap = Nothing
                Dim mmrmDiagnostic As String = String.Empty

                If TryCreateMmrmThetaParameterMap(request,
                                                  optimizerTheta,
                                                  optimizerThetaCovariance,
                                                  mmrmMap,
                                                  mmrmDiagnostic) Then
                    map = mmrmMap
                    diagnostic = map.DiagnosticMessage
                    Return True
                End If

                map = New MixedModelKrParameterMap With {
                    .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                    .OptimizerThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                    .KrTheta = CType(optimizerTheta.Clone(), Double()),
                    .KrThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                    .OptimizerToKrJacobian = Matrix.IdentityMat(optimizerTheta.Length - 1),
                    .ParameterNames = GetOptimizerParameterNames(request),
                    .ParameterScale = MixedModelKrParameterScale.MmrmTheta,
                    .RequiresMmrmThetaBackTransform = False,
                    .DiagnosticMessage = "KR parameter map uses legacy optimizer theta as MMRM theta. " & mmrmDiagnostic
                }
                diagnostic = map.DiagnosticMessage
                Return True
            End If

            If preference = MixedModelKenwardRogerParameterScalePreference.OptimizerInternal Then
                map = New MixedModelKrParameterMap With {
                    .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                    .OptimizerThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                    .KrTheta = CType(optimizerTheta.Clone(), Double()),
                    .KrThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                    .OptimizerToKrJacobian = Matrix.IdentityMat(optimizerTheta.Length - 1),
                    .ParameterNames = GetOptimizerParameterNames(request),
                    .ParameterScale = MixedModelKrParameterScale.OptimizerInternal,
                    .RequiresMmrmThetaBackTransform = False,
                    .DiagnosticMessage = "KR parameter map uses optimizer-internal theta scale."
                }
                diagnostic = map.DiagnosticMessage
                Return True
            End If

            Dim covScaleWs As MixedModelCovarianceScaleWorkspace = Nothing
            Dim covDiag As String = String.Empty
            If Not TryCreate(request, optimizerTheta, optimizerThetaCovariance, covScaleWs, covDiag) Then
                diagnostic = "Cannot create covariance-scale KR parameter map: " & covDiag
                Return False
            End If

            Dim transformedCov(,) As Double = TransformCovariance(covScaleWs.OptimizerToCovarianceJacobian,
                                                                  optimizerThetaCovariance)
            If transformedCov Is Nothing Then
                diagnostic = "Cannot create covariance-scale KR parameter map: covariance transform J*C*J' failed."
                Return False
            End If

            Dim validationDiagnostic As String = String.Empty
            If Not ValidateCovarianceScaleParameterMap(request, covScaleWs, transformedCov, opt, validationDiagnostic) Then
                diagnostic = "Cannot create covariance-scale KR parameter map: " & validationDiagnostic
                Return False
            End If

            map = New MixedModelKrParameterMap With {
                .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                .OptimizerThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                .KrTheta = CType(covScaleWs.CovarianceTheta.Clone(), Double()),
                .KrThetaCovariance = transformedCov,
                .OptimizerToKrJacobian = covScaleWs.OptimizerToCovarianceJacobian,
                .ParameterNames = covScaleWs.CovarianceThetaNames,
                .ParameterScale = MixedModelKrParameterScale.Covariance,
                .DiagnosticMessage = "KR parameter map uses direct covariance-parameter scale. " & validationDiagnostic
            }

            diagnostic = map.DiagnosticMessage
            Return True
        End Function

        ''' <summary>
        ''' Validates the covariance-scale KR parameter map before the engine uses it to
        ''' build LMM covariance-scale derivative blocks.
        ''' </summary>
        ''' <remarks>
        ''' The validation is deliberately model-neutral: it checks dimensions, finite
        ''' values, symmetry, basic covariance-diagonal behavior, transformation rank,
        ''' and condition-number diagnostics.  Strict LMM KR contracts use this as the
        ''' no-silent-fallback gate before KR derivative construction begins.
        ''' </remarks>
        Private Function ValidateCovarianceScaleParameterMap(request As MixedModelFitRequest,
                                                            workspace As MixedModelCovarianceScaleWorkspace,
                                                            transformedCovariance(,) As Double,
                                                            options As MixedModelKenwardRogerOptions,
                                                            ByRef diagnostic As String) As Boolean
            diagnostic = String.Empty

            If request Is Nothing Then
                diagnostic = "request is Nothing."
                Return False
            End If

            If workspace Is Nothing Then
                diagnostic = "covariance-scale workspace is Nothing."
                Return False
            End If

            If workspace.OptimizerTheta Is Nothing OrElse workspace.OptimizerTheta.Length = 0 Then
                diagnostic = "optimizer theta is empty."
                Return False
            End If

            If workspace.CovarianceTheta Is Nothing OrElse workspace.CovarianceTheta.Length = 0 Then
                diagnostic = "covariance theta is empty."
                Return False
            End If

            Dim kCov As Integer = workspace.CovarianceTheta.Length
            Dim kOpt As Integer = workspace.OptimizerTheta.Length

            If Not Matrix.VectorIsFinite(workspace.CovarianceTheta) Then
                diagnostic = "covariance theta contains non-finite values."
                Return False
            End If

            If workspace.CovarianceThetaNames IsNot Nothing AndAlso workspace.CovarianceThetaNames.Length <> kCov Then
                diagnostic = "covariance theta name count does not match covariance theta length."
                Return False
            End If

            If workspace.OptimizerToCovarianceJacobian Is Nothing OrElse
               workspace.OptimizerToCovarianceJacobian.GetLength(0) <> kCov OrElse
               workspace.OptimizerToCovarianceJacobian.GetLength(1) <> kOpt Then
                diagnostic = "optimizer-to-covariance Jacobian has incompatible dimensions."
                Return False
            End If

            If Not Matrix.MatrixIsFinite(workspace.OptimizerToCovarianceJacobian) Then
                diagnostic = "optimizer-to-covariance Jacobian contains non-finite values."
                Return False
            End If

            If transformedCovariance Is Nothing OrElse
               transformedCovariance.GetLength(0) <> kCov OrElse
               transformedCovariance.GetLength(1) <> kCov Then
                diagnostic = "transformed covariance-theta covariance has incompatible dimensions."
                Return False
            End If

            If Not Matrix.MatrixIsFinite(transformedCovariance) Then
                diagnostic = "transformed covariance-theta covariance contains non-finite values."
                Return False
            End If

            Dim symMessage As String = String.Empty
            If Not Matrix.MatrixIsFiniteAndSymmetric(transformedCovariance, 0.00000001, symMessage) Then
                diagnostic = "transformed covariance-theta covariance failed finite/symmetry checks: " & symMessage
                Return False
            End If

            If HasClearlyNegativeCovarianceDiagonal(transformedCovariance) Then
                diagnostic = "transformed covariance-theta covariance has a clearly negative diagonal element."
                Return False
            End If

            regression.MixedModelEngine.SymmetrizeInPlace(transformedCovariance)

            Dim jacobianRank As Integer = MixedModelNumericalDiagnostics.NumericRankBySvd(workspace.OptimizerToCovarianceJacobian)
            Dim thetaCovRank As Integer = MixedModelNumericalDiagnostics.NumericRankBySvd(transformedCovariance)
            Dim thetaCovCondition As Double = MixedModelNumericalDiagnostics.EstimateConditionNumberBySvd(transformedCovariance)

            If jacobianRank <= 0 Then
                diagnostic = "optimizer-to-covariance Jacobian is numerically rank zero."
                Return False
            End If

            If thetaCovRank <= 0 Then
                diagnostic = "transformed covariance-theta covariance is numerically rank zero."
                Return False
            End If

            Dim sb As New StringBuilder()
            sb.Append("Covariance-scale map validated: covarianceThetaCount=").Append(kCov.ToString())
            sb.Append("; optimizerThetaCount=").Append(kOpt.ToString())
            sb.Append("; jacobianRank=").Append(jacobianRank.ToString()).Append("/").Append(Math.Min(kCov, kOpt).ToString())
            sb.Append("; thetaCovRank=").Append(thetaCovRank.ToString()).Append("/").Append(kCov.ToString())
            sb.Append("; thetaCovCondition=").Append(FormatDiagnosticDouble(thetaCovCondition)).Append(".")

            If jacobianRank < Math.Min(kCov, kOpt) Then
                sb.Append(" Warning: optimizer-to-covariance Jacobian is rank deficient; some covariance-scale directions may be weakly identified.")
            End If

            If thetaCovRank < kCov Then
                sb.Append(" Warning: transformed covariance-theta covariance is rank deficient; KR denominator DF may be unstable.")
            End If

            Dim conditionWarning As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("Covariance-scale theta covariance", thetaCovCondition)
            If Not String.IsNullOrWhiteSpace(conditionWarning) Then
                sb.Append(" Warning: ").Append(conditionWarning)
            End If

            Dim varianceWarning As String = PotentialNegativeNamedVarianceWarning(workspace.CovarianceThetaNames,
                                                                                 workspace.CovarianceTheta)
            If Not String.IsNullOrWhiteSpace(varianceWarning) Then
                sb.Append(" Warning: ").Append(varianceWarning)
            End If

            diagnostic = sb.ToString()
            Return True
        End Function

        Private Function HasClearlyNegativeCovarianceDiagonal(a(,) As Double) As Boolean
            If a Is Nothing OrElse a.GetLength(0) <> a.GetLength(1) Then Return True

            For i As Integer = 0 To a.GetLength(0) - 1
                Dim value As Double = a(i, i)
                Dim tol As Double = Math.Max(0.0000000001, Math.Abs(value) * 0.00000001)
                If value < -tol Then Return True
            Next

            Return False
        End Function


        Private Function FormatDiagnosticDouble(value As Double) As String
            If Double.IsPositiveInfinity(value) Then Return "Infinity"
            If Double.IsNegativeInfinity(value) Then Return "-Infinity"
            If Double.IsNaN(value) Then Return "NaN"
            Return value.ToString("G6", CultureInfo.InvariantCulture)
        End Function


        Private Function PotentialNegativeNamedVarianceWarning(names() As String,
                                                               values() As Double) As String
            If names Is Nothing OrElse values Is Nothing Then Return String.Empty
            If names.Length <> values.Length Then Return String.Empty

            For i As Integer = 0 To names.Length - 1
                Dim name As String = If(names(i), String.Empty).ToLowerInvariant()
                If (name.Contains("var") OrElse name.Contains("variance") OrElse name.Contains("sigma2")) AndAlso
                   values(i) < -0.00000001 Then
                    Return "covariance parameter '" & names(i) & "' has a negative variance-like value."
                End If
            Next

            Return String.Empty
        End Function

        Private Function TryCreateMmrmThetaParameterMap(request As MixedModelFitRequest,
                                                       optimizerTheta() As Double,
                                                       optimizerThetaCovariance(,) As Double,
                                                       ByRef map As MixedModelKrParameterMap,
                                                       ByRef diagnostic As String) As Boolean
            map = Nothing
            diagnostic = String.Empty

            If request Is Nothing OrElse Not request.IsMMRM() Then
                diagnostic = "R mmrm theta mapping is only used for MMRM requests."
                Return False
            End If

            If TypeOf request.ResidualStruct Is UnstructuredR Then
                Return TryCreateUnstructuredMmrmThetaParameterMap(request,
                                                                 optimizerTheta,
                                                                 optimizerThetaCovariance,
                                                                 map,
                                                                 diagnostic)
            End If

            If TypeOf request.ResidualStruct Is CompoundSymmetryR OrElse
               TypeOf request.ResidualStruct Is HeterogeneousCSR OrElse
               TypeOf request.ResidualStruct Is AR1R OrElse
               TypeOf request.ResidualStruct Is HeterogeneousAR1R Then
                Return TryCreateVarianceCorrelationMmrmThetaParameterMap(request,
                                                                         optimizerTheta,
                                                                         optimizerThetaCovariance,
                                                                         map,
                                                                         diagnostic)
            End If

            diagnostic = "No structure-specific R mmrm theta map is implemented for " &
                         If(request.ResidualStruct Is Nothing, "<none>", request.ResidualStruct.ToString()) & "; using legacy optimizer theta."
            Return False
        End Function

        Private Function TryCreateUnstructuredMmrmThetaParameterMap(request As MixedModelFitRequest,
                                                                   optimizerTheta() As Double,
                                                                   optimizerThetaCovariance(,) As Double,
                                                                   ByRef map As MixedModelKrParameterMap,
                                                                   ByRef diagnostic As String) As Boolean
            map = Nothing
            diagnostic = String.Empty

            Dim mmrmTheta() As Double = Nothing
            Dim names() As String = Nothing
            Dim jacobian(,) As Double = Nothing

            If Not TryOptimizerToMmrmThetaUnstructured(request,
                                                       optimizerTheta,
                                                       mmrmTheta,
                                                       names,
                                                       jacobian,
                                                       diagnostic) Then
                Return False
            End If

            Dim transformedCovariance(,) As Double = TransformCovariance(jacobian, optimizerThetaCovariance)
            If transformedCovariance Is Nothing Then
                diagnostic = "R mmrm UN theta covariance transform J*C*J' failed."
                Return False
            End If

            map = New MixedModelKrParameterMap With {
                .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                .OptimizerThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                .KrTheta = mmrmTheta,
                .KrThetaCovariance = transformedCovariance,
                .OptimizerToKrJacobian = jacobian,
                .ParameterNames = names,
                .ParameterScale = MixedModelKrParameterScale.MmrmTheta,
                .RequiresMmrmThetaBackTransform = True,
                .DiagnosticMessage = "KR parameter map uses R mmrm-style UN theta: log Cholesky diagonals followed by row-normalized Cholesky off-diagonals."
            }

            diagnostic = map.DiagnosticMessage
            Return True
        End Function

        Public Function TryMmrmThetaToOptimizerTheta(request As MixedModelFitRequest,
                                                    mmrmTheta() As Double,
                                                    ByRef optimizerTheta() As Double,
                                                    Optional ByRef diagnostic As String = Nothing) As Boolean
            optimizerTheta = Nothing
            diagnostic = String.Empty

            If request Is Nothing OrElse mmrmTheta Is Nothing Then
                diagnostic = "Cannot convert R mmrm theta to optimizer theta: request or theta is missing."
                Return False
            End If

            If request.IsMMRM() AndAlso TypeOf request.ResidualStruct Is UnstructuredR Then
                Return TryMmrmThetaUnstructuredToOptimizer(request, mmrmTheta, optimizerTheta, diagnostic)
            End If

            If request.IsMMRM() AndAlso
               (TypeOf request.ResidualStruct Is CompoundSymmetryR OrElse
                TypeOf request.ResidualStruct Is HeterogeneousCSR OrElse
                TypeOf request.ResidualStruct Is AR1R OrElse
                TypeOf request.ResidualStruct Is HeterogeneousAR1R) Then
                Return TryMmrmThetaVarianceCorrelationToOptimizer(request, mmrmTheta, optimizerTheta, diagnostic)
            End If

            optimizerTheta = CType(mmrmTheta.Clone(), Double())
            diagnostic = "R mmrm theta conversion used identity fallback."
            Return True
        End Function

        Private Function TryCreateVarianceCorrelationMmrmThetaParameterMap(request As MixedModelFitRequest,
                                                                          optimizerTheta() As Double,
                                                                          optimizerThetaCovariance(,) As Double,
                                                                          ByRef map As MixedModelKrParameterMap,
                                                                          ByRef diagnostic As String) As Boolean
            map = Nothing
            diagnostic = String.Empty

            Dim mmrmTheta() As Double = Nothing
            Dim names() As String = Nothing
            Dim jacobian(,) As Double = Nothing

            If Not TryOptimizerToMmrmThetaVarianceCorrelation(request,
                                                              optimizerTheta,
                                                              mmrmTheta,
                                                              names,
                                                              jacobian,
                                                              diagnostic) Then
                Return False
            End If

            Dim transformedCovariance(,) As Double = TransformCovariance(jacobian, optimizerThetaCovariance)
            If transformedCovariance Is Nothing Then
                diagnostic = "R mmrm variance/correlation theta covariance transform J*C*J' failed."
                Return False
            End If

            map = New MixedModelKrParameterMap With {
                 .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                 .OptimizerThetaCovariance = CType(optimizerThetaCovariance.Clone(), Double(,)),
                 .KrTheta = mmrmTheta,
                 .KrThetaCovariance = transformedCovariance,
                 .OptimizerToKrJacobian = jacobian,
                 .ParameterNames = names,
                 .ParameterScale = MixedModelKrParameterScale.MmrmTheta,
                 .RequiresMmrmThetaBackTransform = True,
                 .DiagnosticMessage = "KR parameter map uses R mmrm-style theta for " & request.ResidualStruct.ToString() & "."
            }

            diagnostic = map.DiagnosticMessage
            Return True
        End Function

        Private Function TryOptimizerToMmrmThetaVarianceCorrelation(request As MixedModelFitRequest,
                                                                    optimizerTheta() As Double,
                                                                    ByRef mmrmTheta() As Double,
                                                                    ByRef names() As String,
                                                                    ByRef jacobian(,) As Double,
                                                                    ByRef diagnostic As String) As Boolean
            mmrmTheta = Nothing
            names = Nothing
            jacobian = Nothing
            diagnostic = String.Empty

            Dim thetaG() As Double = Nothing
            Dim thetaR() As Double = Nothing
            If Not TryUnpackTheta(request, optimizerTheta, thetaG, thetaR, diagnostic) Then Return False

            Dim m As Integer = VisitDimension(request.Data)
            Dim isHeterogeneous As Boolean = (TypeOf request.ResidualStruct Is HeterogeneousCSR OrElse
                                              TypeOf request.ResidualStruct Is HeterogeneousAR1R)
            Dim isCompoundSymmetry As Boolean = (TypeOf request.ResidualStruct Is CompoundSymmetryR OrElse
                                                 TypeOf request.ResidualStruct Is HeterogeneousCSR)
            Dim rCount As Integer = If(isHeterogeneous, m + 1, 2)

            If m <= 1 Then
                diagnostic = "Cannot map R mmrm theta: visit dimension must be at least two."
                Return False
            End If

            If thetaR Is Nothing OrElse thetaR.Length <> rCount Then
                diagnostic = "Cannot map R mmrm theta: unexpected residual theta length."
                Return False
            End If

            Dim gCount As Integer = If(thetaG Is Nothing, 0, thetaG.Length)
            ReDim mmrmTheta(gCount + rCount - 1)
            ReDim names(gCount + rCount - 1)
            ReDim jacobian(gCount + rCount - 1, optimizerTheta.Length - 1)

            For g As Integer = 0 To gCount - 1
                mmrmTheta(g) = thetaG(g)
                names(g) = "G:" & g.ToString(System.Globalization.CultureInfo.InvariantCulture)
                jacobian(g, g) = 1.0
            Next

            Dim varianceCount As Integer = If(isHeterogeneous, m, 1)
            For v As Integer = 0 To varianceCount - 1
                Dim outIndex As Integer = gCount + v
                Dim optIndex As Integer = gCount + v
                mmrmTheta(outIndex) = 0.5 * thetaR(v)
                If isHeterogeneous Then
                    names(outIndex) = "R:mmrm_log_sd_visit" & (v + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)
                Else
                    names(outIndex) = "R:mmrm_log_sd"
                End If
                jacobian(outIndex, optIndex) = 0.5
            Next

            Dim corrOptLocalIndex As Integer = varianceCount
            Dim corrOutIndex As Integer = gCount + varianceCount
            Dim corrOptIndex As Integer = gCount + corrOptLocalIndex
            Dim optimizerCorrTheta As Double = thetaR(corrOptLocalIndex)
            Dim rho As Double = Math.Tanh(optimizerCorrTheta)

            If isCompoundSymmetry Then
                Dim a As Double = 1.0 / CDbl(m - 1)
                If rho <= -a + 0.000000000001 OrElse rho >= 1.0 - 0.000000000001 Then
                    diagnostic = "Cannot map CS/HCS correlation to R mmrm theta: rho is outside the admissible open interval."
                    Return False
                End If

                Dim x As Double = (rho + a) / (1.0 + a)
                mmrmTheta(corrOutIndex) = LogitClamped(x)
                names(corrOutIndex) = "R:mmrm_cs_rho"
                jacobian(corrOutIndex, corrOptIndex) = ((1.0 + a) * (1.0 - rho * rho)) / ((rho + a) * (1.0 - rho))
            Else
                mmrmTheta(corrOutIndex) = Math.Sinh(optimizerCorrTheta)
                names(corrOutIndex) = "R:mmrm_ar1_rho"
                jacobian(corrOutIndex, corrOptIndex) = Math.Cosh(optimizerCorrTheta)
            End If

            Return True
        End Function

        Private Function TryMmrmThetaVarianceCorrelationToOptimizer(request As MixedModelFitRequest,
                                                                    mmrmTheta() As Double,
                                                                    ByRef optimizerTheta() As Double,
                                                                    ByRef diagnostic As String) As Boolean
            optimizerTheta = Nothing
            diagnostic = String.Empty

            Dim activeG As MixedModelGStruct = ActiveGStruct(request)
            Dim gCount As Integer = 0
            If activeG IsNot Nothing Then gCount = activeG.ParamCount(request.Data.Q)

            Dim m As Integer = VisitDimension(request.Data)
            Dim isHeterogeneous As Boolean = (TypeOf request.ResidualStruct Is HeterogeneousCSR OrElse
                                              TypeOf request.ResidualStruct Is HeterogeneousAR1R)
            Dim isCompoundSymmetry As Boolean = (TypeOf request.ResidualStruct Is CompoundSymmetryR OrElse
                                                 TypeOf request.ResidualStruct Is HeterogeneousCSR)
            Dim rCount As Integer = If(isHeterogeneous, m + 1, 2)
            If m <= 1 OrElse mmrmTheta Is Nothing OrElse mmrmTheta.Length <> gCount + rCount Then
                diagnostic = "R mmrm variance/correlation theta length mismatch."
                Return False
            End If

            Dim thetaG() As Double = SubVector(mmrmTheta, 0, gCount)
            Dim thetaR(rCount - 1) As Double
            Dim varianceCount As Integer = If(isHeterogeneous, m, 1)

            For v As Integer = 0 To varianceCount - 1
                thetaR(v) = 2.0 * mmrmTheta(gCount + v)
            Next

            Dim corrMmrmTheta As Double = mmrmTheta(gCount + varianceCount)
            Dim rho As Double
            If isCompoundSymmetry Then
                Dim a As Double = 1.0 / CDbl(m - 1)
                rho = Logit.LogisticStable(corrMmrmTheta) * (1.0 + a) - a
            Else
                Dim t As Double = corrMmrmTheta
                rho = t / Math.Sqrt(1.0 + t * t)
            End If

            If rho <= -MAX_ABS_RHO Then rho = -MAX_ABS_RHO
            If rho >= MAX_ABS_RHO Then rho = MAX_ABS_RHO
            thetaR(varianceCount) = Atanh(rho, False, MAX_ABS_RHO)

            optimizerTheta = Pack(thetaG, thetaR)
            Return True
        End Function

        Private Function LogitClamped(x As Double) As Double
            Dim z As Double = x
            If z < 0.000000000001 Then z = 0.000000000001
            If z > 1.0 - 0.000000000001 Then z = 1.0 - 0.000000000001
            Return Math.Log(z / (1.0 - z))
        End Function

        Private Function TryOptimizerToMmrmThetaUnstructured(request As MixedModelFitRequest,
                                                            optimizerTheta() As Double,
                                                            ByRef mmrmTheta() As Double,
                                                            ByRef names() As String,
                                                            ByRef jacobian(,) As Double,
                                                            ByRef diagnostic As String) As Boolean
            mmrmTheta = Nothing
            names = Nothing
            jacobian = Nothing
            diagnostic = String.Empty

            Dim thetaG() As Double = Nothing
            Dim thetaR() As Double = Nothing
            If Not TryUnpackTheta(request, optimizerTheta, thetaG, thetaR, diagnostic) Then Return False

            Dim m As Integer = VisitDimension(request.Data)
            Dim rCount As Integer = m * (m + 1) \ 2
            If m <= 0 OrElse thetaR Is Nothing OrElse thetaR.Length <> rCount Then
                diagnostic = "Cannot map UN theta to R mmrm scale: unexpected residual theta length."
                Return False
            End If

            Dim gCount As Integer = If(thetaG Is Nothing, 0, thetaG.Length)
            Dim totalCount As Integer = gCount + rCount
            ReDim mmrmTheta(totalCount - 1)
            ReDim names(totalCount - 1)
            ReDim jacobian(totalCount - 1, optimizerTheta.Length - 1)

            For g As Integer = 0 To gCount - 1
                mmrmTheta(g) = thetaG(g)
                names(g) = "G:" & g.ToString(System.Globalization.CultureInfo.InvariantCulture)
                jacobian(g, g) = 1.0
            Next

            Dim diagOptIndex(m - 1) As Integer
            Dim offRows As New List(Of Integer)()
            Dim offCols As New List(Of Integer)()
            Dim offOptIndices As New List(Of Integer)()

            Dim optIndex As Integer = 0
            For row As Integer = 0 To m - 1
                For col As Integer = 0 To row
                    If row = col Then
                        diagOptIndex(row) = optIndex
                    Else
                        offRows.Add(row)
                        offCols.Add(col)
                        offOptIndices.Add(optIndex)
                    End If
                    optIndex += 1
                Next
            Next

            For row As Integer = 0 To m - 1
                Dim outIndex As Integer = gCount + row
                Dim optDiag As Integer = gCount + diagOptIndex(row)
                mmrmTheta(outIndex) = thetaR(diagOptIndex(row))
                names(outIndex) = "R:mmrm_log_chol_diag_" & (row + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)
                jacobian(outIndex, optDiag) = 1.0
            Next

            For off As Integer = 0 To offOptIndices.Count - 1
                Dim row As Integer = offRows(off)
                Dim col As Integer = offCols(off)
                Dim outIndex As Integer = gCount + m + off
                Dim optOff As Integer = gCount + offOptIndices(off)
                Dim optDiag As Integer = gCount + diagOptIndex(row)
                Dim diagScale As Double = Math.Exp(thetaR(diagOptIndex(row)))
                If diagScale <= 0.0 OrElse Double.IsInfinity(diagScale) OrElse Double.IsNaN(diagScale) Then
                    diagnostic = "Cannot map UN theta to R mmrm scale: non-finite Cholesky diagonal."
                    Return False
                End If

                Dim ratio As Double = thetaR(offOptIndices(off)) / diagScale
                mmrmTheta(outIndex) = ratio
                names(outIndex) = "R:mmrm_chol_ratio_" & (row + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) & "_" & (col + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)
                jacobian(outIndex, optOff) = 1.0 / diagScale
                jacobian(outIndex, optDiag) = -ratio
            Next

            Return True
        End Function

        Private Function TryMmrmThetaUnstructuredToOptimizer(request As MixedModelFitRequest,
                                                            mmrmTheta() As Double,
                                                            ByRef optimizerTheta() As Double,
                                                            ByRef diagnostic As String) As Boolean
            optimizerTheta = Nothing
            diagnostic = String.Empty

            Dim activeG As MixedModelGStruct = ActiveGStruct(request)
            Dim gCount As Integer = 0
            If activeG IsNot Nothing Then gCount = activeG.ParamCount(request.Data.Q)

            Dim m As Integer = VisitDimension(request.Data)
            Dim rCount As Integer = m * (m + 1) \ 2
            If m <= 0 OrElse mmrmTheta.Length <> gCount + rCount Then
                diagnostic = "R mmrm UN theta length mismatch."
                Return False
            End If

            Dim thetaG() As Double = SubVector(mmrmTheta, 0, gCount)
            Dim thetaR(rCount - 1) As Double
            Dim diagOptIndex(m - 1) As Integer
            Dim offRows As New List(Of Integer)()
            Dim offCols As New List(Of Integer)()
            Dim offOptIndices As New List(Of Integer)()

            Dim optIndex As Integer = 0
            For row As Integer = 0 To m - 1
                For col As Integer = 0 To row
                    If row = col Then
                        diagOptIndex(row) = optIndex
                    Else
                        offRows.Add(row)
                        offCols.Add(col)
                        offOptIndices.Add(optIndex)
                    End If
                    optIndex += 1
                Next
            Next

            For row As Integer = 0 To m - 1
                thetaR(diagOptIndex(row)) = mmrmTheta(gCount + row)
            Next

            For off As Integer = 0 To offOptIndices.Count - 1
                Dim row As Integer = offRows(off)
                Dim diagScale As Double = Math.Exp(mmrmTheta(gCount + row))
                thetaR(offOptIndices(off)) = mmrmTheta(gCount + m + off) * diagScale
            Next

            optimizerTheta = Pack(thetaG, thetaR)
            Return True
        End Function

        ''' <summary>
        ''' Builds a covariance-scale workspace for a fitted mixed-model parameter vector.
        ''' </summary>
        Public Function TryCreate(request As MixedModelFitRequest,
                                  optimizerTheta() As Double,
                                  optimizerThetaCovariance(,) As Double,
                                  ByRef workspace As MixedModelCovarianceScaleWorkspace,
                                  Optional ByRef diagnostic As String = Nothing) As Boolean
            workspace = Nothing
            diagnostic = String.Empty

            If request Is Nothing Then
                diagnostic = "Cannot create covariance-scale workspace: request is Nothing."
                Return False
            End If

            If optimizerTheta Is Nothing OrElse optimizerTheta.Length = 0 Then
                diagnostic = "Cannot create covariance-scale workspace: optimizer theta is empty."
                Return False
            End If
            Dim automaticScaleDiagnostic As String = String.Empty
            If Not ShouldUseCovarianceScaleForKenwardRoger(request, automaticScaleDiagnostic) Then
                diagnostic = automaticScaleDiagnostic
                Return False
            End If

            Dim covarianceTheta() As Double = Nothing
            Dim covarianceNames() As String = Nothing

            If Not TryOptimizerToCovarianceTheta(request, optimizerTheta, covarianceTheta, covarianceNames, diagnostic) Then
                Return False
            End If

            Dim jac(,) As Double = NumericalJacobianOptimizerToCovariance(request, optimizerTheta, covarianceTheta)
            If jac Is Nothing Then
                diagnostic = "Cannot create covariance-scale workspace: optimizer-to-covariance Jacobian failed."
                Return False
            End If

            workspace = New MixedModelCovarianceScaleWorkspace With {
                .OptimizerTheta = CType(optimizerTheta.Clone(), Double()),
                .CovarianceTheta = covarianceTheta,
                .CovarianceThetaNames = covarianceNames,
                .OptimizerToCovarianceJacobian = jac,
                .ParameterScale = MixedModelKrParameterScale.Covariance,
                .DiagnosticMessage = "KR covariance-parameter scale workspace created. k=" & covarianceTheta.Length.ToString()
            }

            diagnostic = workspace.DiagnosticMessage
            Return True
        End Function


        ''' <summary>
        ''' Converts optimizer-scale theta to covariance-scale theta.
        ''' </summary>
        Public Function TryOptimizerToCovarianceTheta(request As MixedModelFitRequest,
                                                     optimizerTheta() As Double,
                                                     ByRef covarianceTheta() As Double,
                                                     ByRef covarianceThetaNames() As String,
                                                     Optional ByRef diagnostic As String = Nothing) As Boolean
            covarianceTheta = Nothing
            covarianceThetaNames = Nothing
            diagnostic = String.Empty

            If request Is Nothing OrElse request.Data Is Nothing OrElse request.ResidualStruct Is Nothing Then
                diagnostic = "Missing request, data, or residual structure."
                Return False
            End If

            Dim thetaG() As Double = Nothing
            Dim thetaR() As Double = Nothing

            If Not TryUnpackTheta(request, optimizerTheta, thetaG, thetaR, diagnostic) Then Return False

            Dim gVals As New List(Of Double)()
            Dim gNames As New List(Of String)()

            Dim activeG As MixedModelGStruct = ActiveGStruct(request)
            If activeG IsNot Nothing Then
                If Not TryOptimizerGToCovariance(activeG,
                                                 request.Data.Q,
                                                 thetaG,
                                                 request.RandomEffectNames,
                                                 gVals,
                                                 gNames,
                                                 diagnostic) Then
                    diagnostic = "G-side covariance-scale conversion failed: " & diagnostic
                    Return False
                End If
            End If

            Dim rVals As New List(Of Double)()
            Dim rNames As New List(Of String)()

            If Not TryOptimizerRToCovariance(request.ResidualStruct,
                                             request.Data,
                                             thetaR,
                                             rVals,
                                             rNames,
                                             diagnostic) Then
                diagnostic = "R-side covariance-scale conversion failed: " & diagnostic
                Return False
            End If

            Dim allVals As New List(Of Double)()
            allVals.AddRange(gVals)
            allVals.AddRange(rVals)

            Dim allNames As New List(Of String)()
            allNames.AddRange(gNames)
            allNames.AddRange(rNames)

            covarianceTheta = allVals.ToArray()
            covarianceThetaNames = allNames.ToArray()

            Return True
        End Function


        ''' <summary>
        ''' Converts covariance-scale theta back to optimizer-scale theta.
        ''' </summary>
        Public Function TryCovarianceToOptimizerTheta(request As MixedModelFitRequest,
                                                     covarianceTheta() As Double,
                                                     ByRef optimizerTheta() As Double,
                                                     Optional ByRef diagnostic As String = Nothing) As Boolean
            optimizerTheta = Nothing
            diagnostic = String.Empty

            If request Is Nothing OrElse request.Data Is Nothing OrElse request.ResidualStruct Is Nothing Then
                diagnostic = "Missing request, data, or residual structure."
                Return False
            End If

            If covarianceTheta Is Nothing Then
                diagnostic = "Covariance theta is Nothing."
                Return False
            End If

            Dim activeG As MixedModelGStruct = ActiveGStruct(request)
            Dim gCountCov As Integer = CovarianceParameterCountG(activeG, request.Data.Q)
            Dim rCountCov As Integer = CovarianceParameterCountR(request.ResidualStruct, request.Data)

            If covarianceTheta.Length <> gCountCov + rCountCov Then
                diagnostic = "Covariance theta length mismatch. Expected " &
                             (gCountCov + rCountCov).ToString() & ", got " &
                             covarianceTheta.Length.ToString() & "."
                Return False
            End If

            Dim k As Integer = 0
            Dim covG() As Double = SubVector(covarianceTheta, k, gCountCov)
            k += gCountCov
            Dim covR() As Double = SubVector(covarianceTheta, k, rCountCov)

            Dim optG() As Double = Nothing
            If activeG IsNot Nothing Then
                If Not TryCovarianceGToOptimizer(activeG, request.Data.Q, covG, optG, diagnostic) Then
                    diagnostic = "G-side covariance-to-optimizer conversion failed: " & diagnostic
                    Return False
                End If
            Else
                optG = Array.Empty(Of Double)()
            End If

            Dim optR() As Double = Nothing
            If Not TryCovarianceRToOptimizer(request.ResidualStruct, request.Data, covR, optR, diagnostic) Then
                diagnostic = "R-side covariance-to-optimizer conversion failed: " & diagnostic
                Return False
            End If

            optimizerTheta = Pack(optG, optR)
            Return True
        End Function


        ''' <summary>
        ''' Transforms covariance of optimizer parameters to covariance of covariance
        ''' parameters using J * C * J'.
        ''' </summary>
        Public Function TransformCovariance(jacobian(,) As Double, optimizerCovariance(,) As Double) As Double(,)
            If jacobian Is Nothing OrElse optimizerCovariance Is Nothing Then Return Nothing

            Dim kCov As Integer = jacobian.GetLength(0)
            Dim kOpt As Integer = jacobian.GetLength(1)

            If optimizerCovariance.GetLength(0) <> kOpt OrElse optimizerCovariance.GetLength(1) <> kOpt Then
                Return Nothing
            End If

            Dim temp(kCov - 1, kOpt - 1) As Double

            For r As Integer = 0 To kCov - 1
                For c As Integer = 0 To kOpt - 1
                    Dim s As Double = 0.0
                    For h As Integer = 0 To kOpt - 1
                        s += jacobian(r, h) * optimizerCovariance(h, c)
                    Next
                    temp(r, c) = s
                Next
            Next

            Dim out(kCov - 1, kCov - 1) As Double

            For r As Integer = 0 To kCov - 1
                For c As Integer = 0 To kCov - 1
                    Dim s As Double = 0.0
                    For h As Integer = 0 To kOpt - 1
                        s += temp(r, h) * jacobian(c, h)
                    Next
                    out(r, c) = s
                Next
            Next

            regression.MixedModelEngine.SymmetrizeInPlace(out)
            Return out
        End Function


        ''' <summary>
        ''' Conservative finite-difference step on covariance scale.
        ''' </summary>
        Public Function FiniteDifferenceStep(covarianceThetaValue As Double) As Double
            Dim scale As Double = Math.Max(1.0, Math.Abs(covarianceThetaValue))
            Return Math.Max(1.0E-7, 1.0E-5 * scale)
        End Function

        ''' <summary>
        ''' Decides whether the automatic KR derivative path should use the covariance
        ''' parameter scale for this request.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Direct covariance lower-triangle parameters are useful for LMM variance-component
        ''' models because they avoid taking KR second derivatives on the optimizer's
        ''' transformed log-variance scale.
        ''' </para>
        ''' <para>
        ''' For MMRM with unstructured residual covariance, however, the direct covariance
        ''' lower-triangle scale makes <c>V(kappa)</c> linear and therefore removes the
        ''' <c>R_hj</c> second-derivative contribution. That produces the linear-KR value
        ''' and no longer matches the current R <c>mmrm</c> reference test. Until the
        ''' mmrm-compatible UN KR parameter scale is explicitly validated, UN MMRM should
        ''' remain on the optimizer-internal scale.
        ''' </para>
        ''' </remarks>
        Private Function ShouldUseCovarianceScaleForKenwardRoger(request As MixedModelFitRequest, ByRef diagnostic As String) As Boolean
            diagnostic = String.Empty

            If request Is Nothing OrElse request.ResidualStruct Is Nothing OrElse request.Data Is Nothing Then
                diagnostic = "KR covariance-scale path unavailable: request, data, or residual structure is missing."
                Return False
            End If

            Dim hasActiveG As Boolean = request.RandomStruct IsNot Nothing AndAlso request.Data.Q > 0 AndAlso
                                        Not request.RandomStruct.IsDegenerateZeroG()

            If TypeOf request.ResidualStruct Is UnstructuredR AndAlso Not hasActiveG Then
                diagnostic = "KR covariance-scale path intentionally disabled for MMRM with UnstructuredR. " &
                     "Direct covariance lower-triangle parameters make d2V zero and reproduce the linear-KR approximation; " &
                     "falling back to optimizer-internal scale for R mmrm compatibility."
                Return False
            End If

            Return True
        End Function

        Private Function NumericalJacobianOptimizerToCovariance(request As MixedModelFitRequest,
                                                                optimizerTheta() As Double,
                                                                baseCovarianceTheta() As Double) As Double(,)
            If request Is Nothing OrElse optimizerTheta Is Nothing OrElse baseCovarianceTheta Is Nothing Then Return Nothing

            Dim kCov As Integer = baseCovarianceTheta.Length
            Dim kOpt As Integer = optimizerTheta.Length
            Dim out(kCov - 1, kOpt - 1) As Double

            For j As Integer = 0 To kOpt - 1
                Dim h As Double = Math.Max(1.0E-6, 1.0E-5 * Math.Max(1.0, Math.Abs(optimizerTheta(j))))

                Dim tPlus() As Double = CType(optimizerTheta.Clone(), Double())
                Dim tMinus() As Double = CType(optimizerTheta.Clone(), Double())
                tPlus(j) += h
                tMinus(j) -= h

                Dim cPlus() As Double = Nothing
                Dim nPlus() As String = Nothing
                Dim cMinus() As Double = Nothing
                Dim nMinus() As String = Nothing
                Dim msg As String = Nothing

                Dim okPlus As Boolean = TryOptimizerToCovarianceTheta(request, tPlus, cPlus, nPlus, msg)
                Dim okMinus As Boolean = TryOptimizerToCovarianceTheta(request, tMinus, cMinus, nMinus, msg)

                For r As Integer = 0 To kCov - 1
                    Dim deriv As Double = 0.0

                    If okPlus AndAlso okMinus AndAlso cPlus.Length = kCov AndAlso cMinus.Length = kCov Then
                        deriv = (cPlus(r) - cMinus(r)) / (2.0 * h)
                    ElseIf okPlus AndAlso cPlus.Length = kCov Then
                        deriv = (cPlus(r) - baseCovarianceTheta(r)) / h
                    ElseIf okMinus AndAlso cMinus.Length = kCov Then
                        deriv = (baseCovarianceTheta(r) - cMinus(r)) / h
                    End If

                    If Not AppInfrastructure.IsFinite(deriv) Then deriv = 0.0
                    out(r, j) = deriv
                Next
            Next

            Return out
        End Function

        Private Function ResolveParameterScalePreference(request As MixedModelFitRequest,
                                                        options As MixedModelKenwardRogerOptions) As MixedModelKenwardRogerParameterScalePreference
            If options IsNot Nothing AndAlso options.ParameterScalePreference <> MixedModelKenwardRogerParameterScalePreference.Automatic Then
                Return options.ParameterScalePreference
            End If

            If request IsNot Nothing AndAlso request.IsMMRM() Then
                Return MixedModelKenwardRogerParameterScalePreference.MmrmTheta
            End If

            Return MixedModelKenwardRogerParameterScalePreference.Covariance
        End Function

        Private Function GetOptimizerParameterNames(request As MixedModelFitRequest) As String()
            If request Is Nothing OrElse request.Data Is Nothing Then Return Nothing

            Dim names As New List(Of String)()
            Dim activeG As MixedModelGStruct = ActiveGStruct(request)

            If activeG IsNot Nothing Then
                Dim gNames() As String = activeG.ParamNames(request.Data.Q, request.RandomEffectNames)
                If gNames IsNot Nothing Then
                    For Each nm As String In gNames
                        names.Add("G:" & If(nm, String.Empty))
                    Next
                End If
            End If

            If request.ResidualStruct IsNot Nothing Then
                Dim rNames() As String = request.ResidualStruct.ParamNames(request.Data)
                If rNames IsNot Nothing Then
                    For Each nm As String In rNames
                        names.Add("R:" & If(nm, String.Empty))
                    Next
                End If
            End If

            If names.Count = 0 Then Return Nothing
            Return names.ToArray()
        End Function

        Private Function TryOptimizerGToCovariance(gStruct As MixedModelGStruct,
                                                   q As Integer,
                                                   thetaG() As Double,
                                                   randomEffectNames() As String,
                                                   outVals As List(Of Double),
                                                   outNames As List(Of String),
                                                   ByRef diagnostic As String) As Boolean
            If gStruct Is Nothing Then Return True

            If TypeOf gStruct Is RandomIntercept Then
                If thetaG Is Nothing OrElse thetaG.Length <> 1 Then
                    diagnostic = "RandomIntercept expects one optimizer parameter."
                    Return False
                End If

                outVals.Add(Math.Exp(thetaG(0)))
                outNames.Add("G_var(" & RandomEffectName(0, randomEffectNames) & ")")
                Return True
            End If

            If TypeOf gStruct Is RandomInterceptSlope Then
                If thetaG Is Nothing OrElse thetaG.Length <> 3 Then
                    diagnostic = "RandomInterceptSlope expects three optimizer parameters."
                    Return False
                End If

                Dim sd0 As Double = Math.Exp(thetaG(0))
                Dim sd1 As Double = Math.Exp(thetaG(1))
                Dim rho As Double = Math.Tanh(thetaG(2))
                Dim cov01 As Double = rho * sd0 * sd1

                outVals.Add(sd0 * sd0)
                outVals.Add(cov01)
                outVals.Add(sd1 * sd1)

                outNames.Add("G_var(" & RandomEffectName(0, randomEffectNames) & ")")
                outNames.Add("G_cov(" & RandomEffectName(1, randomEffectNames) & "," & RandomEffectName(0, randomEffectNames) & ")")
                outNames.Add("G_var(" & RandomEffectName(1, randomEffectNames) & ")")
                Return True
            End If

            If TypeOf gStruct Is VarianceComponentsRandomEffects Then
                If q <= 0 Then
                    diagnostic = "VarianceComponentsRandomEffects requires q > 0."
                    Return False
                End If
                If thetaG Is Nothing OrElse thetaG.Length <> q Then
                    diagnostic = "VarianceComponentsRandomEffects expects one optimizer parameter per random-effect column."
                    Return False
                End If

                For i As Integer = 0 To q - 1
                    outVals.Add(Math.Exp(thetaG(i)))
                    outNames.Add("G_var(" & RandomEffectName(i, randomEffectNames) & ")")
                Next

                Return True
            End If

            If TypeOf gStruct Is IdentityRandomEffects Then
                If thetaG Is Nothing OrElse thetaG.Length <> 1 Then
                    diagnostic = "IdentityRandomEffects expects one optimizer parameter."
                    Return False
                End If
                outVals.Add(Math.Exp(thetaG(0)))
                outNames.Add("G_var(ID)")
                Return True
            End If

            If TypeOf gStruct Is CompoundSymmetryRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If thetaG Is Nothing OrElse thetaG.Length <> expected Then
                    diagnostic = "CompoundSymmetryRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                Dim varB As Double = Math.Exp(thetaG(0))
                outVals.Add(varB)
                outNames.Add("G_var(CS)")
                If q > 1 Then
                    outVals.Add(BoundedCompoundSymmetryCorrelation(thetaG(1), q))
                    outNames.Add("G_corr(CS)")
                End If
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousCompoundSymmetryRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If thetaG Is Nothing OrElse thetaG.Length <> expected Then
                    diagnostic = "HeterogeneousCompoundSymmetryRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                For i As Integer = 0 To q - 1
                    outVals.Add(Math.Exp(thetaG(i)))
                    outNames.Add("G_var(" & RandomEffectName(i, randomEffectNames) & ")")
                Next
                If q > 1 Then
                    outVals.Add(BoundedCompoundSymmetryCorrelation(thetaG(q), q))
                    outNames.Add("G_corr(CSH)")
                End If
                Return True
            End If

            If TypeOf gStruct Is AutoregressiveRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If thetaG Is Nothing OrElse thetaG.Length <> expected Then
                    diagnostic = "AutoregressiveRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                outVals.Add(Math.Exp(thetaG(0)))
                outNames.Add("G_var(AR1)")
                If q > 1 Then
                    outVals.Add(Math.Tanh(thetaG(1)))
                    outNames.Add("G_corrLag1(AR1)")
                End If
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousAutoregressiveRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If thetaG Is Nothing OrElse thetaG.Length <> expected Then
                    diagnostic = "HeterogeneousAutoregressiveRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                For i As Integer = 0 To q - 1
                    outVals.Add(Math.Exp(thetaG(i)))
                    outNames.Add("G_var(" & RandomEffectName(i, randomEffectNames) & ")")
                Next
                If q > 1 Then
                    outVals.Add(Math.Tanh(thetaG(q)))
                    outNames.Add("G_corrLag1(ARH1)")
                End If
                Return True
            End If

            If TypeOf gStruct Is ToeplitzRandomEffects Then
                If thetaG Is Nothing OrElse thetaG.Length <> q Then
                    diagnostic = "ToeplitzRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                Dim gMat(,) As Double = gStruct.BuildG(thetaG, q)
                If gMat Is Nothing Then
                    diagnostic = "ToeplitzRandomEffects BuildG returned Nothing."
                    Return False
                End If
                Dim varB As Double = gMat(0, 0)
                outVals.Add(varB)
                outNames.Add("G_var(TOEP)")
                For lag As Integer = 1 To q - 1
                    outVals.Add(gMat(lag, 0) / varB)
                    outNames.Add("G_corrLag" & lag.ToString() & "(TOEP)")
                Next
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousToeplitzRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If thetaG Is Nothing OrElse thetaG.Length <> expected Then
                    diagnostic = "HeterogeneousToeplitzRandomEffects optimizer parameter length mismatch."
                    Return False
                End If
                Dim gMat(,) As Double = gStruct.BuildG(thetaG, q)
                If gMat Is Nothing Then
                    diagnostic = "HeterogeneousToeplitzRandomEffects BuildG returned Nothing."
                    Return False
                End If
                For i As Integer = 0 To q - 1
                    outVals.Add(gMat(i, i))
                    outNames.Add("G_var(" & RandomEffectName(i, randomEffectNames) & ")")
                Next
                For lag As Integer = 1 To q - 1
                    Dim denom As Double = Math.Sqrt(gMat(lag, lag) * gMat(0, 0))
                    If denom <= MIN_POSITIVE Then
                        diagnostic = "HeterogeneousToeplitzRandomEffects produced a nonpositive variance."
                        Return False
                    End If
                    outVals.Add(gMat(lag, 0) / denom)
                    outNames.Add("G_corrLag" & lag.ToString() & "(TOEPH)")
                Next
                Return True
            End If

            If TypeOf gStruct Is UnstructuredRandomEffects Then
                If q <= 0 Then
                    diagnostic = "UnstructuredRandomEffects requires q > 0."
                    Return False
                End If

                Dim gMat(,) As Double = gStruct.BuildG(thetaG, q)
                If gMat Is Nothing Then
                    diagnostic = "UnstructuredRandomEffects BuildG returned Nothing."
                    Return False
                End If

                AppendLowerTriangle(gMat, outVals)

                For i As Integer = 0 To q - 1
                    For j As Integer = 0 To i
                        If i = j Then
                            outNames.Add("G_var(" & RandomEffectName(i, randomEffectNames) & ")")
                        Else
                            outNames.Add("G_cov(" & RandomEffectName(i, randomEffectNames) & "," & RandomEffectName(j, randomEffectNames) & ")")
                        End If
                    Next
                Next

                Return True
            End If

            diagnostic = "Unsupported G structure for covariance-scale KR: " & gStruct.ToString()
            Return False
        End Function


        Private Function TryCovarianceGToOptimizer(gStruct As MixedModelGStruct,
                                                   q As Integer,
                                                   covG() As Double,
                                                   ByRef thetaG() As Double,
                                                   ByRef diagnostic As String) As Boolean
            thetaG = Nothing

            If gStruct Is Nothing Then
                thetaG = Array.Empty(Of Double)()
                Return True
            End If

            If TypeOf gStruct Is RandomIntercept Then
                If covG Is Nothing OrElse covG.Length <> 1 Then
                    diagnostic = "RandomIntercept covariance theta length must be one."
                    Return False
                End If

                If covG(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(0)) Then
                    diagnostic = "Random-intercept variance must be positive."
                    Return False
                End If

                thetaG = New Double() {Math.Log(covG(0))}
                Return True
            End If

            If TypeOf gStruct Is RandomInterceptSlope Then
                If covG Is Nothing OrElse covG.Length <> 3 Then
                    diagnostic = "RandomInterceptSlope covariance theta length must be three."
                    Return False
                End If

                Dim var0 As Double = covG(0)
                Dim cov01 As Double = covG(1)
                Dim var1 As Double = covG(2)

                If var0 <= MIN_POSITIVE OrElse var1 <= MIN_POSITIVE Then
                    diagnostic = "Random intercept/slope variances must be positive."
                    Return False
                End If

                Dim sd0 As Double = Math.Sqrt(var0)
                Dim sd1 As Double = Math.Sqrt(var1)
                Dim rho As Double = cov01 / (sd0 * sd1)

                If Math.Abs(rho) >= MAX_ABS_RHO Then
                    diagnostic = "Random intercept/slope covariance implies |rho| >= 1."
                    Return False
                End If

                thetaG = New Double() {Math.Log(sd0), Math.Log(sd1), Atanh(rho, False, MAX_ABS_RHO)}
                Return True
            End If

            If TypeOf gStruct Is VarianceComponentsRandomEffects Then
                If q <= 0 Then
                    diagnostic = "VarianceComponentsRandomEffects requires q > 0."
                    Return False
                End If
                If covG Is Nothing OrElse covG.Length <> q Then
                    diagnostic = "VarianceComponentsRandomEffects covariance theta length mismatch."
                    Return False
                End If

                thetaG = New Double(q - 1) {}
                For i As Integer = 0 To q - 1
                    If covG(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(i)) Then
                        diagnostic = "Variance-components random-effect variances must be positive."
                        Return False
                    End If
                    thetaG(i) = Math.Log(covG(i))
                Next

                Return True
            End If

            If TypeOf gStruct Is IdentityRandomEffects Then
                If covG Is Nothing OrElse covG.Length <> 1 Then
                    diagnostic = "IdentityRandomEffects covariance theta length must be one."
                    Return False
                End If
                If covG(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(0)) Then
                    diagnostic = "Identity random-effects variance must be positive."
                    Return False
                End If
                thetaG = New Double() {Math.Log(covG(0))}
                Return True
            End If

            If TypeOf gStruct Is CompoundSymmetryRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "CompoundSymmetryRandomEffects covariance theta length mismatch."
                    Return False
                End If
                If covG(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(0)) Then
                    diagnostic = "Compound-symmetry random-effects variance must be positive."
                    Return False
                End If
                thetaG = New Double(expected - 1) {}
                thetaG(0) = Math.Log(covG(0))
                If q > 1 Then
                    Dim lower As Double = CompoundSymmetryCorrelationLowerBound(q)
                    If covG(1) <= lower OrElse covG(1) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(covG(1)) Then
                        diagnostic = "Compound-symmetry random-effects correlation is outside its positive-definite bounds."
                        Return False
                    End If
                    thetaG(1) = LogitBounded(covG(1), lower, MAX_ABS_RHO)
                End If
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousCompoundSymmetryRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "HeterogeneousCompoundSymmetryRandomEffects covariance theta length mismatch."
                    Return False
                End If
                thetaG = New Double(expected - 1) {}
                For i As Integer = 0 To q - 1
                    If covG(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(i)) Then
                        diagnostic = "Heterogeneous compound-symmetry random-effect variances must be positive."
                        Return False
                    End If
                    thetaG(i) = Math.Log(covG(i))
                Next
                If q > 1 Then
                    Dim rho As Double = covG(q)
                    Dim lower As Double = CompoundSymmetryCorrelationLowerBound(q)
                    If rho <= lower OrElse rho >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(rho) Then
                        diagnostic = "Heterogeneous compound-symmetry random-effects correlation is outside its positive-definite bounds."
                        Return False
                    End If
                    thetaG(q) = LogitBounded(rho, lower, MAX_ABS_RHO)
                End If
                Return True
            End If

            If TypeOf gStruct Is AutoregressiveRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "AutoregressiveRandomEffects covariance theta length mismatch."
                    Return False
                End If
                If covG(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(0)) Then
                    diagnostic = "AR1 random-effects variance must be positive."
                    Return False
                End If
                thetaG = New Double(expected - 1) {}
                thetaG(0) = Math.Log(covG(0))
                If q > 1 Then
                    If Math.Abs(covG(1)) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(covG(1)) Then
                        diagnostic = "AR1 random-effects correlation outside (-1,1)."
                        Return False
                    End If
                    thetaG(1) = Atanh(covG(1), False, MAX_ABS_RHO)
                End If
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousAutoregressiveRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "HeterogeneousAutoregressiveRandomEffects covariance theta length mismatch."
                    Return False
                End If
                thetaG = New Double(expected - 1) {}
                For i As Integer = 0 To q - 1
                    If covG(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(i)) Then
                        diagnostic = "ARH1 random-effect variances must be positive."
                        Return False
                    End If
                    thetaG(i) = Math.Log(covG(i))
                Next
                If q > 1 Then
                    Dim rho As Double = covG(q)
                    If Math.Abs(rho) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(rho) Then
                        diagnostic = "ARH1 random-effects correlation outside (-1,1)."
                        Return False
                    End If
                    thetaG(q) = Atanh(rho, False, MAX_ABS_RHO)
                End If
                Return True
            End If

            If TypeOf gStruct Is ToeplitzRandomEffects Then
                If covG Is Nothing OrElse covG.Length <> q Then
                    diagnostic = "ToeplitzRandomEffects covariance theta length mismatch."
                    Return False
                End If
                If covG(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(0)) Then
                    diagnostic = "Toeplitz random-effects variance must be positive."
                    Return False
                End If
                thetaG = New Double(q - 1) {}
                thetaG(0) = Math.Log(covG(0))
                If q > 1 Then
                    Dim corr(q - 1) As Double
                    corr(0) = 1.0
                    For lag As Integer = 1 To q - 1
                        If Math.Abs(covG(lag)) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(covG(lag)) Then
                            diagnostic = "Toeplitz random-effects lag correlation outside (-1,1)."
                            Return False
                        End If
                        corr(lag) = covG(lag)
                    Next
                    Dim pacfValues() As Double = Nothing
                    If Not TryAutocorrelationsToPartialCorrelations(corr, pacfValues, diagnostic) Then Return False
                    For lag As Integer = 1 To q - 1
                        thetaG(lag) = Atanh(pacfValues(lag), False, MAX_ABS_RHO)
                    Next
                End If
                Return True
            End If

            If TypeOf gStruct Is HeterogeneousToeplitzRandomEffects Then
                Dim expected As Integer = gStruct.ParamCount(q)
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "HeterogeneousToeplitzRandomEffects covariance theta length mismatch."
                    Return False
                End If
                thetaG = New Double(expected - 1) {}
                For i As Integer = 0 To q - 1
                    If covG(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covG(i)) Then
                        diagnostic = "TOEPH random-effect variances must be positive."
                        Return False
                    End If
                    thetaG(i) = Math.Log(covG(i))
                Next
                If q > 1 Then
                    Dim corr(q - 1) As Double
                    corr(0) = 1.0
                    For lag As Integer = 1 To q - 1
                        Dim rho As Double = covG(q + lag - 1)
                        If Math.Abs(rho) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(rho) Then
                            diagnostic = "TOEPH random-effects lag correlation outside (-1,1)."
                            Return False
                        End If
                        corr(lag) = rho
                    Next
                    Dim pacfValues() As Double = Nothing
                    If Not TryAutocorrelationsToPartialCorrelations(corr, pacfValues, diagnostic) Then Return False
                    For lag As Integer = 1 To q - 1
                        thetaG(q + lag - 1) = Atanh(pacfValues(lag), False, MAX_ABS_RHO)
                    Next
                End If
                Return True
            End If

            If TypeOf gStruct Is UnstructuredRandomEffects Then
                Dim expected As Integer = q * (q + 1) \ 2
                If covG Is Nothing OrElse covG.Length <> expected Then
                    diagnostic = "UnstructuredRandomEffects covariance theta length mismatch."
                    Return False
                End If

                Dim mat(,) As Double = MatrixFromLowerTriangle(covG, q)
                Dim chol(,) As Double = Nothing

                If Not TryCholeskyLower(mat, chol) Then
                    diagnostic = "Unstructured random-effects covariance matrix is not SPD."
                    Return False
                End If

                thetaG = PackOptimizerCholesky(chol)
                Return True
            End If

            diagnostic = "Unsupported G structure for covariance-scale KR: " & gStruct.ToString()
            Return False
        End Function


        Private Function TryOptimizerRToCovariance(rStruct As MixedModelRStruct,
                                                   data As MixedModelBlockData,
                                                   thetaR() As Double,
                                                   outVals As List(Of Double),
                                                   outNames As List(Of String),
                                                   ByRef diagnostic As String) As Boolean
            If rStruct Is Nothing OrElse data Is Nothing Then
                diagnostic = "Missing R structure or data."
                Return False
            End If

            If TypeOf rStruct Is IdentityR Then
                If thetaR Is Nothing OrElse thetaR.Length <> 1 Then diagnostic = "IdentityR expects one parameter." : Return False
                outVals.Add(Math.Exp(thetaR(0)))
                outNames.Add("R_var")
                Return True
            End If

            If TypeOf rStruct Is DiagonalHeterogeneousR Then
                For i As Integer = 0 To thetaR.Length - 1
                    outVals.Add(Math.Exp(thetaR(i)))
                    outNames.Add("R_var_visit" & (i + 1).ToString())
                Next
                Return True
            End If

            If TypeOf rStruct Is CompoundSymmetryR Then
                If thetaR Is Nothing OrElse thetaR.Length <> 2 Then diagnostic = "CompoundSymmetryR expects two parameters." : Return False
                Dim varR As Double = Math.Exp(thetaR(0))
                Dim rho As Double = Math.Tanh(thetaR(1))
                outVals.Add(varR)
                outVals.Add(varR * rho)
                outNames.Add("R_var")
                outNames.Add("R_cov")
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousCSR Then
                For i As Integer = 0 To thetaR.Length - 2
                    outVals.Add(Math.Exp(thetaR(i)))
                    outNames.Add("R_var_visit" & (i + 1).ToString())
                Next
                outVals.Add(Math.Tanh(thetaR(thetaR.Length - 1)))
                outNames.Add("R_common_corr")
                Return True
            End If

            If TypeOf rStruct Is AR1R Then
                If thetaR Is Nothing OrElse thetaR.Length <> 2 Then diagnostic = "AR1R expects two parameters." : Return False
                outVals.Add(Math.Exp(thetaR(0)))
                outVals.Add(Math.Tanh(thetaR(1)))
                outNames.Add("R_var")
                outNames.Add("R_ar1_corr")
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousAR1R Then
                For i As Integer = 0 To thetaR.Length - 2
                    outVals.Add(Math.Exp(thetaR(i)))
                    outNames.Add("R_var_visit" & (i + 1).ToString())
                Next
                outVals.Add(Math.Tanh(thetaR(thetaR.Length - 1)))
                outNames.Add("R_ar1_corr")
                Return True
            End If

            If TypeOf rStruct Is ToeplitzR Then
                Dim m As Integer = VisitDimension(data)
                If thetaR Is Nothing OrElse thetaR.Length <> m Then diagnostic = "ToeplitzR expects visit-dimension parameters." : Return False
                outVals.Add(Math.Exp(thetaR(0)))
                outNames.Add("R_var")
                Dim rho() As Double = BuildToeplitzAutocorrelationsFromPartialTheta(thetaR, m, 1)
                For lag As Integer = 1 To m - 1
                    outVals.Add(rho(lag))
                    outNames.Add("R_toep_corr_lag" & lag.ToString())
                Next
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousToeplitzR Then
                Dim m As Integer = VisitDimension(data)
                If thetaR Is Nothing OrElse thetaR.Length <> 2 * m - 1 Then diagnostic = "HeterogeneousToeplitzR expects 2*m-1 parameters." : Return False
                For i As Integer = 0 To m - 1
                    outVals.Add(Math.Exp(thetaR(i)))
                    outNames.Add("R_var_visit" & (i + 1).ToString())
                Next
                Dim rho() As Double = BuildToeplitzAutocorrelationsFromPartialTheta(thetaR, m, m)
                For lag As Integer = 1 To m - 1
                    outVals.Add(rho(lag))
                    outNames.Add("R_toeph_corr_lag" & lag.ToString())
                Next
                Return True
            End If

            If TypeOf rStruct Is UnstructuredR Then
                Dim m As Integer = VisitDimension(data)
                Dim dummyBlock As MixedModelSubjectBlock = BuildFullVisitDummyBlock(data)
                Dim rFull(,) As Double = rStruct.BuildRi(thetaR, dummyBlock, data)
                If rFull Is Nothing OrElse rFull.GetLength(0) <> m Then
                    diagnostic = "UnstructuredR BuildRi failed for full-visit dummy block."
                    Return False
                End If

                AppendLowerTriangle(rFull, outVals)

                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To i
                        If i = j Then
                            outNames.Add("R_var_visit" & (i + 1).ToString())
                        Else
                            outNames.Add("R_cov_visit" & (i + 1).ToString() & "_" & (j + 1).ToString())
                        End If
                    Next
                Next

                Return True
            End If

            diagnostic = "Unsupported R structure for covariance-scale KR: " & rStruct.ToString()
            Return False
        End Function


        Private Function TryCovarianceRToOptimizer(rStruct As MixedModelRStruct,
                                                   data As MixedModelBlockData,
                                                   covR() As Double,
                                                   ByRef thetaR() As Double,
                                                   ByRef diagnostic As String) As Boolean
            thetaR = Nothing

            If TypeOf rStruct Is IdentityR Then
                If covR Is Nothing OrElse covR.Length <> 1 Then diagnostic = "IdentityR covariance theta length must be one." : Return False
                If covR(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(0)) Then diagnostic = "Residual variance must be positive." : Return False
                thetaR = New Double() {Math.Log(covR(0))}
                Return True
            End If

            If TypeOf rStruct Is DiagonalHeterogeneousR Then
                thetaR = LogPositiveVector(covR, diagnostic)
                Return thetaR IsNot Nothing
            End If

            If TypeOf rStruct Is CompoundSymmetryR Then
                If covR Is Nothing OrElse covR.Length <> 2 Then diagnostic = "CompoundSymmetryR covariance theta length must be two." : Return False
                Dim varR As Double = covR(0)
                Dim covRho As Double = covR(1)
                If varR <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(varR) Then diagnostic = "CS variance must be positive." : Return False
                Dim rho As Double = covRho / varR
                If Math.Abs(rho) >= MAX_ABS_RHO Then diagnostic = "CS covariance implies |rho| >= 1." : Return False
                thetaR = New Double() {Math.Log(varR), Atanh(rho, False, MAX_ABS_RHO)}
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousCSR Then
                If covR Is Nothing OrElse covR.Length < 2 Then diagnostic = "Heterogeneous CS covariance theta is too short." : Return False
                ReDim thetaR(covR.Length - 1)
                For i As Integer = 0 To covR.Length - 2
                    If covR(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(i)) Then diagnostic = "HCS variance must be positive." : Return False
                    thetaR(i) = Math.Log(covR(i))
                Next
                Dim rho As Double = covR(covR.Length - 1)
                If Math.Abs(rho) >= MAX_ABS_RHO Then diagnostic = "HCS correlation outside (-1,1)." : Return False
                thetaR(thetaR.Length - 1) = Atanh(rho, False, MAX_ABS_RHO)
                Return True
            End If

            If TypeOf rStruct Is AR1R Then
                If covR Is Nothing OrElse covR.Length <> 2 Then diagnostic = "AR1R covariance theta length must be two." : Return False
                If covR(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(0)) Then diagnostic = "AR1 variance must be positive." : Return False
                If Math.Abs(covR(1)) >= MAX_ABS_RHO Then diagnostic = "AR1 correlation outside (-1,1)." : Return False
                thetaR = New Double() {Math.Log(covR(0)), Atanh(covR(1), False, MAX_ABS_RHO)}
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousAR1R Then
                If covR Is Nothing OrElse covR.Length < 2 Then diagnostic = "Heterogeneous AR1 covariance theta is too short." : Return False
                ReDim thetaR(covR.Length - 1)
                For i As Integer = 0 To covR.Length - 2
                    If covR(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(i)) Then diagnostic = "HAR1 variance must be positive." : Return False
                    thetaR(i) = Math.Log(covR(i))
                Next
                Dim rho As Double = covR(covR.Length - 1)
                If Math.Abs(rho) >= MAX_ABS_RHO Then diagnostic = "HAR1 correlation outside (-1,1)." : Return False
                thetaR(thetaR.Length - 1) = Atanh(rho, False, MAX_ABS_RHO)
                Return True
            End If

            If TypeOf rStruct Is ToeplitzR Then
                Dim m As Integer = VisitDimension(data)
                If covR Is Nothing OrElse covR.Length <> m Then diagnostic = "ToeplitzR covariance theta length mismatch." : Return False
                If covR(0) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(0)) Then diagnostic = "Toeplitz residual variance must be positive." : Return False
                thetaR = New Double(m - 1) {}
                thetaR(0) = Math.Log(covR(0))
                If m > 1 Then
                    Dim corr(m - 1) As Double
                    corr(0) = 1.0
                    For lag As Integer = 1 To m - 1
                        Dim rhoLag As Double = covR(lag)
                        If Math.Abs(rhoLag) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(rhoLag) Then
                            diagnostic = "Toeplitz residual lag correlation outside (-1,1)."
                            Return False
                        End If
                        corr(lag) = rhoLag
                    Next
                    Dim pacfValues() As Double = Nothing
                    If Not TryAutocorrelationsToPartialCorrelations(corr, pacfValues, diagnostic) Then Return False
                    For lag As Integer = 1 To m - 1
                        thetaR(lag) = Atanh(pacfValues(lag), False, MAX_ABS_RHO)
                    Next
                End If
                Return True
            End If

            If TypeOf rStruct Is HeterogeneousToeplitzR Then
                Dim m As Integer = VisitDimension(data)
                Dim expected As Integer = 2 * m - 1
                If covR Is Nothing OrElse covR.Length <> expected Then diagnostic = "HeterogeneousToeplitzR covariance theta length mismatch." : Return False
                thetaR = New Double(expected - 1) {}
                For i As Integer = 0 To m - 1
                    If covR(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(covR(i)) Then diagnostic = "TOEPH residual variances must be positive." : Return False
                    thetaR(i) = Math.Log(covR(i))
                Next
                If m > 1 Then
                    Dim corr(m - 1) As Double
                    corr(0) = 1.0
                    For lag As Integer = 1 To m - 1
                        Dim rhoLag As Double = covR(m + lag - 1)
                        If Math.Abs(rhoLag) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(rhoLag) Then
                            diagnostic = "TOEPH residual lag correlation outside (-1,1)."
                            Return False
                        End If
                        corr(lag) = rhoLag
                    Next
                    Dim pacfValues() As Double = Nothing
                    If Not TryAutocorrelationsToPartialCorrelations(corr, pacfValues, diagnostic) Then Return False
                    For lag As Integer = 1 To m - 1
                        thetaR(m + lag - 1) = Atanh(pacfValues(lag), False, MAX_ABS_RHO)
                    Next
                End If
                Return True
            End If

            If TypeOf rStruct Is UnstructuredR Then
                Dim m As Integer = VisitDimension(data)
                Dim expected As Integer = m * (m + 1) \ 2
                If covR Is Nothing OrElse covR.Length <> expected Then
                    diagnostic = "UnstructuredR covariance theta length mismatch."
                    Return False
                End If

                Dim mat(,) As Double = MatrixFromLowerTriangle(covR, m)
                Dim chol(,) As Double = Nothing

                If Not TryCholeskyLower(mat, chol) Then
                    diagnostic = "Unstructured residual covariance matrix is not SPD."
                    Return False
                End If

                thetaR = PackOptimizerCholesky(chol)
                Return True
            End If

            diagnostic = "Unsupported R structure for covariance-scale KR: " & rStruct.ToString()
            Return False
        End Function


        Private Function TryUnpackTheta(request As MixedModelFitRequest,
                                        theta() As Double,
                                        ByRef thetaG() As Double,
                                        ByRef thetaR() As Double,
                                        ByRef diagnostic As String) As Boolean
            If theta Is Nothing Then theta = Array.Empty(Of Double)()

            Dim gCount As Integer = 0
            Dim activeG As MixedModelGStruct = ActiveGStruct(request)
            If activeG IsNot Nothing Then gCount = activeG.ParamCount(request.Data.Q)

            Dim rCount As Integer = request.ResidualStruct.ParamCount(request.Data)

            If theta.Length <> gCount + rCount Then
                diagnostic = "Theta length mismatch. Expected " & (gCount + rCount).ToString() &
                             ", got " & theta.Length.ToString() & "."
                Return False
            End If

            thetaG = SubVector(theta, 0, gCount)
            thetaR = SubVector(theta, gCount, rCount)
            Return True
        End Function


        Private Function ActiveGStruct(request As MixedModelFitRequest) As MixedModelGStruct
            If request Is Nothing OrElse request.RandomStruct Is Nothing Then Return Nothing
            If request.Data Is Nothing OrElse request.Data.Q <= 0 Then Return Nothing
            If request.RandomStruct.IsDegenerateZeroG() Then Return Nothing
            Return request.RandomStruct
        End Function


        Private Function CovarianceParameterCountG(gStruct As MixedModelGStruct, q As Integer) As Integer
            If gStruct Is Nothing Then Return 0
            If TypeOf gStruct Is RandomIntercept Then Return 1
            If TypeOf gStruct Is RandomInterceptSlope Then Return 3
            If TypeOf gStruct Is VarianceComponentsRandomEffects Then Return q
            If TypeOf gStruct Is IdentityRandomEffects Then Return 1
            If TypeOf gStruct Is CompoundSymmetryRandomEffects Then Return gStruct.ParamCount(q)
            If TypeOf gStruct Is HeterogeneousCompoundSymmetryRandomEffects Then Return gStruct.ParamCount(q)
            If TypeOf gStruct Is AutoregressiveRandomEffects Then Return gStruct.ParamCount(q)
            If TypeOf gStruct Is HeterogeneousAutoregressiveRandomEffects Then Return gStruct.ParamCount(q)
            If TypeOf gStruct Is ToeplitzRandomEffects Then Return q
            If TypeOf gStruct Is HeterogeneousToeplitzRandomEffects Then Return gStruct.ParamCount(q)
            If TypeOf gStruct Is UnstructuredRandomEffects Then Return q * (q + 1) \ 2
            Return gStruct.ParamCount(q)
        End Function


        Private Function CovarianceParameterCountR(rStruct As MixedModelRStruct, data As MixedModelBlockData) As Integer
            If rStruct Is Nothing Then Return 0
            If TypeOf rStruct Is UnstructuredR Then
                Dim m As Integer = VisitDimension(data)
                Return m * (m + 1) \ 2
            End If
            Return rStruct.ParamCount(data)
        End Function

        Private Function BuildFullVisitDummyBlock(data As MixedModelBlockData) As MixedModelSubjectBlock
            Dim m As Integer = VisitDimension(data)
            If m <= 0 Then Throw New ArgumentException("Cannot build full-visit dummy block because visit dimension is zero.")

            Dim y(m - 1) As Double
            Dim rowIndices(m - 1) As Integer
            Dim visit(m - 1) As Double
            Dim visitIndex(m - 1) As Integer

            Dim p As Integer = 1
            If data IsNot Nothing AndAlso data.P > 0 Then p = data.P

            Dim x(m - 1, p - 1) As Double

            For i As Integer = 0 To m - 1
                y(i) = 0.0
                rowIndices(i) = i
                visitIndex(i) = i

                If data IsNot Nothing AndAlso data.UniqueVisitValues IsNot Nothing AndAlso data.UniqueVisitValues.Length = m Then
                    visit(i) = data.UniqueVisitValues(i)
                Else
                    visit(i) = CDbl(i + 1)
                End If

                x(i, 0) = 1.0
            Next

            Return New MixedModelSubjectBlock(subjectKey:="__KR_FULL_VISIT__",
                                      rowIndices:=rowIndices,
                                      y:=y,
                                      x:=x,
                                      z:=Nothing,
                                      visit:=visit,
                                      visitIndex:=visitIndex)
        End Function


        Private Function VisitDimension(data As MixedModelBlockData) As Integer
            If data Is Nothing Then Return 0
            Dim uniqueVisits() As Double = data.UniqueVisitValues
            If uniqueVisits IsNot Nothing AndAlso uniqueVisits.Length > 0 Then Return uniqueVisits.Length
            If data.Blocks Is Nothing Then Return 0
            Return data.MaxClusterSize()
        End Function


        Private Function Pack(thetaG() As Double, thetaR() As Double) As Double()
            Dim gLen As Integer = If(thetaG Is Nothing, 0, thetaG.Length)
            Dim rLen As Integer = If(thetaR Is Nothing, 0, thetaR.Length)
            If gLen + rLen = 0 Then Return Array.Empty(Of Double)()

            Dim out(gLen + rLen - 1) As Double
            Dim k As Integer = 0

            If thetaG IsNot Nothing Then
                For i As Integer = 0 To thetaG.Length - 1
                    out(k) = thetaG(i)
                    k += 1
                Next
            End If

            If thetaR IsNot Nothing Then
                For i As Integer = 0 To thetaR.Length - 1
                    out(k) = thetaR(i)
                    k += 1
                Next
            End If

            Return out
        End Function


        Private Function SubVector(x() As Double, startIndex As Integer, length As Integer) As Double()
            If length <= 0 Then Return Array.Empty(Of Double)()

            Dim out(length - 1) As Double
            Array.Copy(x, startIndex, out, 0, length)
            Return out
        End Function


        Private Sub AppendLowerTriangle(a(,) As Double, vals As List(Of Double))
            Dim n As Integer = a.GetLength(0)
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    vals.Add(a(i, j))
                Next
            Next
        End Sub

        Private Function MatrixFromLowerTriangle(vals() As Double, n As Integer) As Double(,)
            Dim out(n - 1, n - 1) As Double
            Dim k As Integer = 0

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    out(i, j) = vals(k)
                    out(j, i) = vals(k)
                    k += 1
                Next
            Next

            Return out
        End Function

        Private Function CompoundSymmetryCorrelationLowerBound(q As Integer) As Double
            If q <= 1 Then Return -MAX_ABS_RHO
            Return -1.0 / CDbl(q - 1)
        End Function


        Private Function BoundedCompoundSymmetryCorrelation(raw As Double, q As Integer) As Double
            Dim lower As Double = CompoundSymmetryCorrelationLowerBound(q)
            If raw >= 0.0 Then Return MAX_ABS_RHO * Math.Tanh(raw)
            Return lower * Math.Tanh(-raw)
        End Function


        Private Function LogitBounded(value As Double, lower As Double, upper As Double) As Double
            If value <= lower OrElse value >= upper Then Throw New ArgumentOutOfRangeException(NameOf(value))
            If value >= 0.0 Then
                Return Atanh(value / upper, False, MAX_ABS_RHO)
            End If
            Return -Atanh(value / lower, False, MAX_ABS_RHO)
        End Function

        Private Function BuildToeplitzAutocorrelationsFromPartialTheta(theta() As Double, q As Integer, startIndex As Integer) As Double()
            If q < 1 Then Throw New ArgumentOutOfRangeException(NameOf(q))
            Dim rho(q - 1) As Double
            rho(0) = 1.0
            If q = 1 Then Return rho

            Dim phi(q - 1, q - 1) As Double
            For k As Integer = 1 To q - 1
                Dim pacf As Double = Math.Tanh(theta(startIndex + k - 1))
                If pacf > MAX_ABS_RHO Then pacf = MAX_ABS_RHO
                If pacf < -MAX_ABS_RHO Then pacf = -MAX_ABS_RHO

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

        Private Function TryAutocorrelationsToPartialCorrelations(rho() As Double,
                                                                  ByRef pacfValues() As Double,
                                                                  ByRef diagnostic As String) As Boolean
            pacfValues = Nothing
            If rho Is Nothing OrElse rho.Length = 0 Then
                diagnostic = "Toeplitz autocorrelation vector is empty."
                Return False
            End If

            Dim q As Integer = rho.Length
            pacfValues = New Double(q - 1) {}
            pacfValues(0) = 1.0
            If q = 1 Then Return True

            Dim phi(q - 1, q - 1) As Double
            Dim predictionVar As Double = 1.0

            For k As Integer = 1 To q - 1
                Dim numerator As Double = rho(k)
                For j As Integer = 1 To k - 1
                    numerator -= phi(k - 1, j) * rho(k - j)
                Next

                If predictionVar <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(predictionVar) Then
                    diagnostic = "Toeplitz autocorrelation sequence is not positive definite."
                    Return False
                End If

                Dim pacf As Double = numerator / predictionVar
                If Math.Abs(pacf) >= MAX_ABS_RHO OrElse Not AppInfrastructure.IsFinite(pacf) Then
                    diagnostic = "Toeplitz autocorrelation sequence implies an invalid partial autocorrelation."
                    Return False
                End If

                pacfValues(k) = pacf
                phi(k, k) = pacf
                If k > 1 Then
                    For j As Integer = 1 To k - 1
                        phi(k, j) = phi(k - 1, j) - pacf * phi(k - 1, k - j)
                    Next
                End If

                predictionVar *= (1.0 - pacf * pacf)
            Next

            Return True
        End Function

        Private Function TryCholeskyLower(a(,) As Double, ByRef l(,) As Double) As Boolean
            l = Nothing
            If a Is Nothing OrElse a.GetLength(0) <> a.GetLength(1) Then Return False

            Dim n As Integer = a.GetLength(0)
            ReDim l(n - 1, n - 1)

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    Dim sum As Double = a(i, j)

                    For k As Integer = 0 To j - 1
                        sum -= l(i, k) * l(j, k)
                    Next

                    If i = j Then
                        If sum <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(sum) Then Return False
                        l(i, j) = Math.Sqrt(sum)
                    Else
                        If l(j, j) <= 0.0 Then Return False
                        l(i, j) = sum / l(j, j)
                    End If
                Next
            Next

            Return True
        End Function


        Private Function PackOptimizerCholesky(l(,) As Double) As Double()
            Dim n As Integer = l.GetLength(0)
            Dim out(n * (n + 1) \ 2 - 1) As Double
            Dim k As Integer = 0

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    If i = j Then
                        If l(i, j) <= MIN_POSITIVE Then Return Nothing
                        out(k) = Math.Log(l(i, j))
                    Else
                        out(k) = l(i, j)
                    End If
                    k += 1
                Next
            Next

            Return out
        End Function


        Private Function LogPositiveVector(values() As Double, ByRef diagnostic As String) As Double()
            If values Is Nothing Then Return Nothing

            Dim out(values.Length - 1) As Double

            For i As Integer = 0 To values.Length - 1
                If values(i) <= MIN_POSITIVE OrElse Not AppInfrastructure.IsFinite(values(i)) Then
                    diagnostic = "Variance/covariance parameter " & i.ToString() & " must be positive."
                    Return Nothing
                End If

                out(i) = Math.Log(values(i))
            Next

            Return out
        End Function


        Private Function RandomEffectName(index As Integer, names() As String) As String
            If names Is Nothing OrElse index < 0 OrElse index >= names.Length OrElse String.IsNullOrWhiteSpace(names(index)) Then
                Return "b" & (index + 1).ToString()
            End If

            Return names(index).Trim()
        End Function

    End Module

End Namespace
