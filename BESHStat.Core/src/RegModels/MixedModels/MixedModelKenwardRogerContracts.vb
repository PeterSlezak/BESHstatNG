Option Explicit On
Option Strict On

Imports System

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

End Namespace
