Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text

Namespace regression

    ''' <summary>
    ''' One subject/block contribution used by universal Kenward-Roger backend calculations.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class is deliberately independent of whether the model is an MMRM or an LMM.
    ''' It only knows the marginal mixed-model representation:
    ''' </para>
    ''' <para><c>y_i ~ N(X_i beta, V_i(theta))</c></para>
    ''' <para>
    ''' with fixed-effect design <c>X_i</c>, inverse marginal covariance <c>V_i^-1</c>,
    ''' and first/optional second derivatives of <c>V_i</c> with respect to covariance
    ''' parameters.
    ''' </para>
    ''' <para>
    ''' For MMRM, <c>V_i = R_i</c>.  For LMM, <c>V_i = Z_i G Z_i' + R_i</c>.
    ''' The derivative producer can therefore live in the engine/covariance layer,
    ''' while this backend only consumes derivative blocks.
    ''' </para>
    ''' </remarks>
    Public Class MixedModelKrBlock

        ''' <summary>Fixed-effect design matrix for this subject/block.</summary>
        Public Property X As Double(,)

        ''' <summary>Inverse marginal covariance matrix for this subject/block.</summary>
        Public Property VInv As Double(,)

        ''' <summary>
        ''' First derivatives dV_i / d theta_h.
        ''' Dimensions: theta index, row, column.
        ''' </summary>
        Public Property DV As Double(,,)

        ''' <summary>
        ''' Optional second derivatives d2V_i / d theta_h d theta_j.
        ''' Dimensions: theta h, theta j, row, column.
        ''' </summary>
        Public Property D2V As Double(,,,)

        Public Function Validate(expectedP As Integer,
                                 expectedK As Integer,
                                 ByRef message As String) As Boolean
            message = String.Empty

            If X Is Nothing Then
                message = "X is Nothing."
                Return False
            End If

            If VInv Is Nothing Then
                message = "VInv is Nothing."
                Return False
            End If

            If DV Is Nothing Then
                message = "DV is Nothing."
                Return False
            End If

            Dim n As Integer = X.GetLength(0)
            Dim p As Integer = X.GetLength(1)

            If expectedP > 0 AndAlso p <> expectedP Then
                message = "X fixed-effect column count does not match expected P."
                Return False
            End If

            If VInv.GetLength(0) <> n OrElse VInv.GetLength(1) <> n Then
                message = "VInv must be n x n and conformable with X."
                Return False
            End If

            If DV.GetLength(0) <> expectedK OrElse DV.GetLength(1) <> n OrElse DV.GetLength(2) <> n Then
                message = "DV dimensions must be K x n x n."
                Return False
            End If

            If D2V IsNot Nothing Then
                If D2V.GetLength(0) <> expectedK OrElse D2V.GetLength(1) <> expectedK OrElse
                   D2V.GetLength(2) <> n OrElse D2V.GetLength(3) <> n Then
                    message = "D2V dimensions must be K x K x n x n."
                    Return False
                End If
            End If

            Return True
        End Function

    End Class


    ''' <summary>
    ''' Diagnostics for the KR derivative visit-pattern cache used by residual-only MMRM workspaces.
    ''' </summary>
    ''' <remarks>
    ''' The cache is local to a single KR derivative workspace build at one covariance-parameter vector.
    ''' Counts therefore describe reuse across subject blocks that share the same observed visit pattern.
    ''' </remarks>
    Public Class MixedModelKrDerivativePatternCacheDiagnostics
        Public Property Enabled As Boolean = False
        Public Property PatternCount As Integer = 0
        Public Property VInvHits As Integer = 0
        Public Property VInvMisses As Integer = 0
        Public Property FirstDerivativeHits As Integer = 0
        Public Property FirstDerivativeMisses As Integer = 0
        Public Property SecondDerivativeHits As Integer = 0
        Public Property SecondDerivativeMisses As Integer = 0
        Public Property InvalidBuilds As Integer = 0

        Public Function Clone() As MixedModelKrDerivativePatternCacheDiagnostics
            Return New MixedModelKrDerivativePatternCacheDiagnostics With {
                .Enabled = Me.Enabled,
                .PatternCount = Me.PatternCount,
                .VInvHits = Me.VInvHits,
                .VInvMisses = Me.VInvMisses,
                .FirstDerivativeHits = Me.FirstDerivativeHits,
                .FirstDerivativeMisses = Me.FirstDerivativeMisses,
                .SecondDerivativeHits = Me.SecondDerivativeHits,
                .SecondDerivativeMisses = Me.SecondDerivativeMisses,
                .InvalidBuilds = Me.InvalidBuilds
            }
        End Function
    End Class

    ''' <summary>
    ''' Diagnostics for the KR P/Q/R half-pair aggregation optimization.
    ''' </summary>
    ''' <remarks>
    ''' Q_hj is computed for h &lt;= j and Q_jh is filled by transpose. R_hj is computed
    ''' for h &lt;= j and R_jh is filled by covariance-parameter symmetry when second
    ''' derivatives are available. Counts are accumulated over all subject/block
    ''' contributions in the current KR workspace.
    ''' </remarks>
    Public Class MixedModelKrPqrPairDiagnostics
        Public Property Enabled As Boolean = False
        Public Property ParameterCount As Integer = 0
        Public Property FastFactorizationEnabled As Boolean = True
        Public Property AllocationReducedAggregationEnabled As Boolean = True
        Public Property QPairMatricesComputed As Integer = 0
        Public Property QPairMatricesFilledBySymmetry As Integer = 0
        Public Property RPairMatricesComputed As Integer = 0
        Public Property RPairMatricesFilledBySymmetry As Integer = 0

        Public ReadOnly Property PairMatricesComputed As Integer
            Get
                Return QPairMatricesComputed + RPairMatricesComputed
            End Get
        End Property

        Public ReadOnly Property PairMatricesFilledBySymmetry As Integer
            Get
                Return QPairMatricesFilledBySymmetry + RPairMatricesFilledBySymmetry
            End Get
        End Property

        Public Function Clone() As MixedModelKrPqrPairDiagnostics
            Return New MixedModelKrPqrPairDiagnostics With {
                .Enabled = Me.Enabled,
                .ParameterCount = Me.ParameterCount,
                .FastFactorizationEnabled = Me.FastFactorizationEnabled,
                .AllocationReducedAggregationEnabled = Me.AllocationReducedAggregationEnabled,
                .QPairMatricesComputed = Me.QPairMatricesComputed,
                .QPairMatricesFilledBySymmetry = Me.QPairMatricesFilledBySymmetry,
                .RPairMatricesComputed = Me.RPairMatricesComputed,
                .RPairMatricesFilledBySymmetry = Me.RPairMatricesFilledBySymmetry
            }
        End Function
    End Class


    ''' <summary>
    ''' Diagnostics for the KR P/Q/R fixed-design contribution cache.
    ''' </summary>
    ''' <remarks>
    ''' The cache groups KR blocks whose fixed-effect design, inverse covariance, and covariance
    ''' derivative tensors are identical for the P/Q/R calculation. A grouped contribution is
    ''' computed once and then added with its subject/block multiplicity, which is exact for
    ''' repeated MMRM subject profiles and safe for LMM blocks because candidate hits are verified
    ''' by value before reuse.
    ''' </remarks>
    Public Class MixedModelKrPqrDesignPatternCacheDiagnostics
        Public Property Enabled As Boolean = False
        Public Property BlockCount As Integer = 0
        Public Property PatternCount As Integer = 0
        Public Property Hits As Integer = 0
        Public Property Misses As Integer = 0
        Public Property IncompatibleKeyCollisions As Integer = 0
        Public Property InvalidBuilds As Integer = 0

        Public Function Clone() As MixedModelKrPqrDesignPatternCacheDiagnostics
            Return New MixedModelKrPqrDesignPatternCacheDiagnostics With {
                .Enabled = Me.Enabled,
                .BlockCount = Me.BlockCount,
                .PatternCount = Me.PatternCount,
                .Hits = Me.Hits,
                .Misses = Me.Misses,
                .IncompatibleKeyCollisions = Me.IncompatibleKeyCollisions,
                .InvalidBuilds = Me.InvalidBuilds
            }
        End Function
    End Class

    ''' <summary>
    ''' Diagnostics collected while building Kenward-Roger finite-difference derivative blocks.
    ''' </summary>
    Public Class MixedModelKrFiniteDifferenceDiagnostics
        Public Property BlocksStarted As Integer = 0
        Public Property BlocksCompleted As Integer = 0
        Public Property FirstDerivativeCentralCount As Integer = 0
        Public Property FirstDerivativeOneSidedFallbackCount As Integer = 0
        Public Property FirstDerivativeFailedCount As Integer = 0
        Public Property PureSecondDerivativeCentralCount As Integer = 0
        Public Property MixedSecondDerivativeCentralCount As Integer = 0
        Public Property SecondDerivativeFailedCount As Integer = 0
        Public Property MaxStepHalvingUsed As Integer = 0
        Public Property MaxFirstDerivativeRichardsonRelativeChange As Double = 0.0
        Public Property MaxSecondDerivativeRichardsonRelativeChange As Double = 0.0
        Public Property PerturbedViCacheEntries As Integer = 0
        Public Property PerturbedViCacheHits As Integer = 0
        Public Property PerturbedViCacheMisses As Integer = 0
        Public Property PerturbedViCacheInvalidBuilds As Integer = 0

        Public Sub RecordStepHalving(attempt As Integer)
            If attempt > MaxStepHalvingUsed Then MaxStepHalvingUsed = attempt
        End Sub

        Public Sub RecordFirstDerivativeRichardson(relativeChange As Double)
            If Not Double.IsNaN(relativeChange) AndAlso Not Double.IsInfinity(relativeChange) Then
                MaxFirstDerivativeRichardsonRelativeChange = Math.Max(MaxFirstDerivativeRichardsonRelativeChange, relativeChange)
            End If
        End Sub

        Public Sub RecordSecondDerivativeRichardson(relativeChange As Double)
            If Not Double.IsNaN(relativeChange) AndAlso Not Double.IsInfinity(relativeChange) Then
                MaxSecondDerivativeRichardsonRelativeChange = Math.Max(MaxSecondDerivativeRichardsonRelativeChange, relativeChange)
            End If
        End Sub

        Public Function HasFailures() As Boolean
            Return FirstDerivativeFailedCount > 0 OrElse SecondDerivativeFailedCount > 0
        End Function

        Public Function HasFallbacks() As Boolean
            Return FirstDerivativeOneSidedFallbackCount > 0
        End Function

        Public Function HasInvalidPerturbations() As Boolean
            Return PerturbedViCacheInvalidBuilds > 0
        End Function

        Public Function HasLargeRichardsonChange(Optional warningThreshold As Double = 0.25) As Boolean
            If Double.IsNaN(warningThreshold) OrElse Double.IsInfinity(warningThreshold) OrElse warningThreshold <= 0.0 Then
                warningThreshold = 0.25
            End If

            Return MaxFirstDerivativeRichardsonRelativeChange > warningThreshold OrElse
                   MaxSecondDerivativeRichardsonRelativeChange > warningThreshold
        End Function

        Public Function QualityStatus(Optional warningThreshold As Double = 0.25) As String
            If HasFailures() Then Return "Failed"
            If HasFallbacks() OrElse HasInvalidPerturbations() OrElse HasLargeRichardsonChange(warningThreshold) Then Return "Warning"
            Return "OK"
        End Function

        Public Function WarningSummary(Optional warningThreshold As Double = 0.25) As String
            Dim parts As New List(Of String)()

            If FirstDerivativeFailedCount > 0 Then
                parts.Add("first derivative failures=" & FirstDerivativeFailedCount.ToString(System.Globalization.CultureInfo.InvariantCulture))
            End If

            If SecondDerivativeFailedCount > 0 Then
                parts.Add("second derivative failures=" & SecondDerivativeFailedCount.ToString(System.Globalization.CultureInfo.InvariantCulture))
            End If

            If FirstDerivativeOneSidedFallbackCount > 0 Then
                parts.Add("one-sided first-derivative fallbacks=" & FirstDerivativeOneSidedFallbackCount.ToString(System.Globalization.CultureInfo.InvariantCulture))
            End If

            If PerturbedViCacheInvalidBuilds > 0 Then
                parts.Add("invalid perturbed V builds=" & PerturbedViCacheInvalidBuilds.ToString(System.Globalization.CultureInfo.InvariantCulture))
            End If

            If HasLargeRichardsonChange(warningThreshold) Then
                parts.Add("large Richardson change; first=" &
                          MaxFirstDerivativeRichardsonRelativeChange.ToString("G4", System.Globalization.CultureInfo.InvariantCulture) &
                          ", second=" &
                          MaxSecondDerivativeRichardsonRelativeChange.ToString("G4", System.Globalization.CultureInfo.InvariantCulture))
            End If

            If parts.Count = 0 Then Return String.Empty
            Return "KR finite-difference diagnostics: " & String.Join("; ", parts.ToArray()) & "."
        End Function

        Public Function SummaryText(Optional warningThreshold As Double = 0.25) As String
            Return "status=" & QualityStatus(warningThreshold) &
                   "; blocks=" & BlocksCompleted.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "/" & BlocksStarted.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; firstCentral=" & FirstDerivativeCentralCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; firstFallback=" & FirstDerivativeOneSidedFallbackCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; firstFailed=" & FirstDerivativeFailedCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; pureSecond=" & PureSecondDerivativeCentralCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; mixedSecond=" & MixedSecondDerivativeCentralCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; secondFailed=" & SecondDerivativeFailedCount.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; maxStepHalving=" & MaxStepHalvingUsed.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; cacheHits=" & PerturbedViCacheHits.ToString(System.Globalization.CultureInfo.InvariantCulture) &
                   "; cacheMisses=" & PerturbedViCacheMisses.ToString(System.Globalization.CultureInfo.InvariantCulture) & "."
        End Function

        Public Function Clone() As MixedModelKrFiniteDifferenceDiagnostics
            Return New MixedModelKrFiniteDifferenceDiagnostics With {
                .BlocksStarted = Me.BlocksStarted,
                .BlocksCompleted = Me.BlocksCompleted,
                .FirstDerivativeCentralCount = Me.FirstDerivativeCentralCount,
                .FirstDerivativeOneSidedFallbackCount = Me.FirstDerivativeOneSidedFallbackCount,
                .FirstDerivativeFailedCount = Me.FirstDerivativeFailedCount,
                .PureSecondDerivativeCentralCount = Me.PureSecondDerivativeCentralCount,
                .MixedSecondDerivativeCentralCount = Me.MixedSecondDerivativeCentralCount,
                .SecondDerivativeFailedCount = Me.SecondDerivativeFailedCount,
                .MaxStepHalvingUsed = Me.MaxStepHalvingUsed,
                .MaxFirstDerivativeRichardsonRelativeChange = Me.MaxFirstDerivativeRichardsonRelativeChange,
                .MaxSecondDerivativeRichardsonRelativeChange = Me.MaxSecondDerivativeRichardsonRelativeChange,
                .PerturbedViCacheEntries = Me.PerturbedViCacheEntries,
                .PerturbedViCacheHits = Me.PerturbedViCacheHits,
                .PerturbedViCacheMisses = Me.PerturbedViCacheMisses,
                .PerturbedViCacheInvalidBuilds = Me.PerturbedViCacheInvalidBuilds
            }
        End Function
    End Class


    ''' <summary>
    ''' Universal workspace for Kenward-Roger matrix construction.
    ''' </summary>
    Public Class MixedModelKrWorkspace

        Public Property P As Integer
        Public Property K As Integer

        ''' <summary>Unadjusted coefficient covariance Phi = Var(beta).</summary>
        Public Property VarBeta As Double(,)

        ''' <summary>Approximate covariance matrix of theta.</summary>
        Public Property ThetaCovariance As Double(,)

        ''' <summary>
        ''' Covariance-parameter vector on the same scale/order used by KR derivatives
        ''' and <see cref="ThetaCovariance"/>.
        ''' </summary>
        Public Property Theta As Double() = Nothing

        ''' <summary>
        ''' Parameter scale used for KR derivatives and ThetaCovariance.
        ''' </summary>
        Public Property ParameterScale As MixedModelKrParameterScale = MixedModelKrParameterScale.OptimizerInternal

        ''' <summary>
        ''' Optional covariance-parameter names when ParameterScale = Covariance.
        ''' </summary>
        Public Property CovarianceParameterNames As String() = Nothing

        ''' <summary>Subject/block derivative data.</summary>
        Public Property Blocks As List(Of MixedModelKrBlock) = New List(Of MixedModelKrBlock)()

        ''' <summary>KR P_h matrices. Dimensions: h, beta row, beta column.</summary>
        Public Property Pmats As Double(,,)

        ''' <summary>KR Q_hj matrices. Dimensions: h, j, beta row, beta column.</summary>
        Public Property Qmats As Double(,,,)

        ''' <summary>Optional KR R_hj matrices. Dimensions: h, j, beta row, beta column.</summary>
        Public Property Rmats As Double(,,,)

        ''' <summary>KR covariance adjustment requested for this workspace.</summary>
        Public Property AdjustmentKind As MixedModelKenwardRogerAdjustmentKind = MixedModelKenwardRogerAdjustmentKind.Full

        ''' <summary>
        ''' If True, a full KR request may fall back to the linear adjustment when
        ''' second derivative matrices are unavailable. For matching R mmrm full KR
        ''' this should normally remain False.
        ''' </summary>
        Public Property AllowLinearFallback As Boolean = False

        ''' <summary>KR covariance adjustment actually used by the last calculation.</summary>
        Public Property AdjustmentUsed As MixedModelKenwardRogerAdjustmentKind = MixedModelKenwardRogerAdjustmentKind.None

        ''' <summary>Full or linear KR adjusted coefficient covariance matrix.</summary>
        Public Property AdjustedVarBeta As Double(,)

        ''' <summary>Informational diagnostic text produced by the last calculation.</summary>
        Public Property DiagnosticMessage As String = String.Empty

        ''' <summary>Estimated condition number of the last KR adjusted coefficient covariance matrix.</summary>
        Public Property AdjustedVarBetaConditionNumber As Double = Double.NaN

        ''' <summary>Number of subject/block V inverse matrices cached in the KR workspace.</summary>
        Public Property VinvCachedBlockCount As Integer = 0

        ''' <summary>Diagnostics from adaptive finite-difference construction of KR derivative blocks.</summary>
        Public Property FiniteDifferenceDiagnostics As MixedModelKrFiniteDifferenceDiagnostics = Nothing

        ''' <summary>Diagnostics for residual-only MMRM derivative reuse by observed visit pattern.</summary>
        Public Property DerivativePatternCache As MixedModelKrDerivativePatternCacheDiagnostics = New MixedModelKrDerivativePatternCacheDiagnostics()

        ''' <summary>Finite-difference option values used to build the current KR derivative workspace.</summary>
        Public Property FiniteDifferenceOptions As MixedModelKenwardRogerFiniteDifferenceOptions = Nothing

        ''' <summary>Phase-level KR timing diagnostics collected by the backend.</summary>
        Public Property PerformanceDiagnostics As MixedModelPerformanceDiagnostics = New MixedModelPerformanceDiagnostics()

        ''' <summary>Diagnostics for half-pair P/Q/R matrix aggregation in this workspace.</summary>
        Public Property PqrPairDiagnostics As MixedModelKrPqrPairDiagnostics = New MixedModelKrPqrPairDiagnostics()

        ''' <summary>
        ''' Enables exact reuse of complete KR P/Q/R contributions for blocks sharing the same
        ''' fixed design and covariance derivative pattern.
        ''' </summary>
        Public Property UsePqrDesignPatternCache As Boolean = True

        ''' <summary>
        ''' Enables lower-cost algebraic factorization for exact KR P/Q/R contribution building.
        ''' The disabled path is retained as a direct-reference calculation for validation tests.
        ''' </summary>
        Public Property UsePqrFastFactorization As Boolean = True

        ''' <summary>Diagnostics for fixed-design KR P/Q/R contribution reuse.</summary>
        Public Property PqrDesignPatternCache As MixedModelKrPqrDesignPatternCacheDiagnostics = New MixedModelKrPqrDesignPatternCacheDiagnostics()

        ''' <summary>Optional cooperative cancellation callback used by long KR backend loops.</summary>
        Public Property CancellationRequested As Func(Of Boolean) = Nothing

        ''' <summary>Optional progress reporter used by GUI clients during long KR backend phases.</summary>
        Public Property ProgressReporter As Action(Of MixedModelProgressInfo) = Nothing

        Public Function FiniteDifferenceWarningThreshold() As Double
            If FiniteDifferenceOptions Is Nothing Then Return 0.25
            Return FiniteDifferenceOptions.RichardsonWarningRelativeTolerance
        End Function

        ''' <summary>Cached KR denominator-DF/scaling results keyed by a rounded restriction-matrix signature.</summary>
        Public Property DfScalingCache As Dictionary(Of String, MixedModelKenwardRogerDfResult) = New Dictionary(Of String, MixedModelKenwardRogerDfResult)()

        ''' <summary>Numerical warnings accumulated while building or using the KR workspace.</summary>
        Public Property NumericalWarnings As List(Of String) = New List(Of String)()

        Public Sub AddNumericalWarning(message As String)
            MixedModelNumericalDiagnostics.AddUniqueWarning(Me.NumericalWarnings, message)
        End Sub

        Public Function NumericalWarningSummary() As String
            If NumericalWarnings Is Nothing OrElse NumericalWarnings.Count = 0 Then Return String.Empty
            Return String.Join(" ", NumericalWarnings.ToArray())
        End Function

        Public Function HasBasicInputs() As Boolean
            If P <= 0 OrElse K < 0 Then Return False
            If VarBeta Is Nothing Then Return False
            If VarBeta.GetLength(0) <> P OrElse VarBeta.GetLength(1) <> P Then Return False
            If ThetaCovariance Is Nothing Then Return False
            If ThetaCovariance.GetLength(0) <> K OrElse ThetaCovariance.GetLength(1) <> K Then Return False
            If Blocks Is Nothing OrElse Blocks.Count = 0 Then Return False
            Return True
        End Function

    End Class


    ''' <summary>
    ''' Universal Kenward-Roger backend math.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is intentionally model-neutral.  It does not build <c>V_i</c> or its
    ''' derivatives.  Instead, it consumes derivative blocks, so both MMRM and LMM can use
    ''' it once the engine/covariance layer can provide <c>dV/dtheta</c> and optionally
    ''' <c>d2V/dtheta dtheta</c>.
    ''' </para>
    ''' <para>
    ''' The current implementation computes the matrix ingredients and a linear
    ''' Kenward-Roger adjusted covariance matrix.  This is backend infrastructure,
    ''' not a final user-facing KR test.  Full KR still needs validated scaling and
    ''' denominator-df calculations for general multi-df hypotheses.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelKenwardRogerBackend

        Private Sub ThrowIfCancellationRequested(ws As MixedModelKrWorkspace)
            If ws Is Nothing OrElse ws.CancellationRequested Is Nothing Then Exit Sub

            Dim cancel As Boolean = False
            Try
                cancel = ws.CancellationRequested.Invoke()
            Catch
                cancel = False
            End Try

            If cancel Then Throw New OperationCanceledException("Kenward-Roger calculation cancelled by user.")
        End Sub

        Private Sub ReportKrProgress(ws As MixedModelKrWorkspace,
                                     stage As String,
                                     percent As Integer,
                                     Optional iteration As Integer = -1,
                                     Optional maxIterations As Integer = -1,
                                     Optional message As String = "")
            Try
                If ws Is Nothing OrElse ws.ProgressReporter Is Nothing Then Exit Sub

                If percent < 0 Then percent = 0
                If percent > 100 Then percent = 100

                ws.ProgressReporter.Invoke(New MixedModelProgressInfo With {
                    .Stage = If(stage, String.Empty),
                    .Message = If(message, String.Empty),
                    .Percent = percent,
                    .Iteration = iteration,
                    .MaxIterations = maxIterations
                })
            Catch
                ' Progress reporting must never interrupt KR calculation.
            End Try
        End Sub

        Private Function ShouldReportKrProgress(current As Integer, total As Integer) As Boolean
            If total <= 0 Then Return False
            If current <= 1 OrElse current >= total Then Return True
            If total <= 25 Then Return True
            Dim stride As Integer = Math.Max(1, total \ 25)
            Return (current Mod stride) = 0
        End Function

        ''' <summary>
        ''' Builds P_h, Q_hj and optional R_hj matrices from marginal covariance derivatives.
        ''' </summary>
        Public Function TryBuildKrMatrices(ws As MixedModelKrWorkspace,
                                           Optional ByRef diagnostic As String = Nothing) As Boolean
            diagnostic = String.Empty

            Dim timingStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try

                ThrowIfCancellationRequested(ws)

                If ws Is Nothing Then
                    diagnostic = "KR workspace is Nothing."
                    Return False
                End If

                If Not ws.HasBasicInputs() Then
                    diagnostic = "KR workspace is missing basic inputs."
                    ws.DiagnosticMessage = diagnostic
                    Return False
                End If

                Dim p As Integer = ws.P
                Dim k As Integer = ws.K

                If ws.DfScalingCache Is Nothing Then ws.DfScalingCache = New Dictionary(Of String, MixedModelKenwardRogerDfResult)()
                ws.DfScalingCache.Clear()
                If ws.NumericalWarnings Is Nothing Then ws.NumericalWarnings = New List(Of String)()
                ws.VinvCachedBlockCount = If(ws.Blocks Is Nothing, 0, ws.Blocks.Count)

                Dim validatedBlocks As New List(Of MixedModelKrBlock)()
                Dim haveSecond As Boolean = True
                Dim blockTotal As Integer = If(ws.Blocks Is Nothing, 0, ws.Blocks.Count)

                ws.PqrDesignPatternCache = New MixedModelKrPqrDesignPatternCacheDiagnostics With {
                    .Enabled = ws.UsePqrDesignPatternCache,
                    .BlockCount = blockTotal
                }

                For Each block As MixedModelKrBlock In ws.Blocks
                    Dim msg As String = String.Empty
                    If Not block.Validate(p, k, msg) Then
                        diagnostic = "Invalid KR derivative block: " & msg
                        ws.DiagnosticMessage = diagnostic
                        ws.PqrDesignPatternCache.InvalidBuilds += 1
                        Return False
                    End If

                    If block.D2V Is Nothing Then haveSecond = False
                    validatedBlocks.Add(block)
                Next

                Dim groups As List(Of MixedModelKrPqrDesignPatternGroup) = BuildPqrDesignPatternGroups(validatedBlocks,
                                                                                                       haveSecond,
                                                                                                       ws.UsePqrDesignPatternCache,
                                                                                                       ws.PqrDesignPatternCache)

                Dim pMats(k - 1, p - 1, p - 1) As Double
                Dim qMats(k - 1, k - 1, p - 1, p - 1) As Double
                Dim rMats(,,,) As Double = Nothing
                If haveSecond Then ReDim rMats(k - 1, k - 1, p - 1, p - 1)

                Dim pairDiagnostics As New MixedModelKrPqrPairDiagnostics With {
                    .Enabled = True,
                    .ParameterCount = k,
                    .FastFactorizationEnabled = ws.UsePqrFastFactorization,
                    .AllocationReducedAggregationEnabled = ws.UsePqrFastFactorization
                }

                ws.PqrPairDiagnostics = pairDiagnostics

                Dim groupTotal As Integer = groups.Count
                Dim groupIndex As Integer = 0
                ReportKrProgress(ws, "Kenward-Roger P/Q/R design-pattern aggregation", 98, 0, groupTotal)

                For Each group As MixedModelKrPqrDesignPatternGroup In groups
                    ThrowIfCancellationRequested(ws)

                    groupIndex += 1
                    If ShouldReportKrProgress(groupIndex, groupTotal) Then
                        ReportKrProgress(ws,
                                         "Kenward-Roger P/Q/R design-pattern aggregation",
                                         98,
                                         groupIndex,
                                         groupTotal,
                                         "pattern " & groupIndex.ToString(CultureInfo.InvariantCulture) &
                                         " of " & groupTotal.ToString(CultureInfo.InvariantCulture) &
                                         ", blocks=" & group.Count.ToString(CultureInfo.InvariantCulture))
                    End If

                    AddPqrContribution(group.Representative,
                                       p,
                                       k,
                                       haveSecond,
                                       pairDiagnostics,
                                       ws.UsePqrFastFactorization,
                                       pMats,
                                       qMats,
                                       rMats,
                                       CDbl(group.Count))
                Next

                NormalizeKrFirstOrderMatrixSymmetry(pMats, qMats)

                ws.Pmats = pMats
                ws.Qmats = qMats

                If haveSecond Then
                    NormalizeKrSecondOrderMatrixSymmetry(rMats)
                    ws.Rmats = rMats
                Else
                    ws.Rmats = Nothing
                End If

                If ws.PqrDesignPatternCache IsNot Nothing Then ws.PqrDesignPatternCache.PatternCount = groupTotal

                diagnostic = "KR P/Q matrices built successfully" & If(haveSecond, " with second derivatives.", " without second derivatives.")
                If ws.PqrDesignPatternCache IsNot Nothing AndAlso ws.PqrDesignPatternCache.Enabled Then
                    diagnostic &= " P/Q/R design-pattern cache: patterns=" & ws.PqrDesignPatternCache.PatternCount.ToString(CultureInfo.InvariantCulture) &
                                  ", hits=" & ws.PqrDesignPatternCache.Hits.ToString(CultureInfo.InvariantCulture) &
                                  ", misses=" & ws.PqrDesignPatternCache.Misses.ToString(CultureInfo.InvariantCulture) & "."
                End If
                ws.DiagnosticMessage = diagnostic
                ReportKrProgress(ws, "Kenward-Roger P/Q/R aggregation complete", 99, groupTotal, groupTotal)
                Return True
            Finally
                timingStopwatch.Stop()
                RecordKrPerformanceTiming(ws, "KrPqrMatrixTimeMs", timingStopwatch.Elapsed.TotalMilliseconds)
            End Try

        End Function


        Private Class MixedModelKrPqrDesignPatternGroup
            Public Property Key As String = String.Empty
            Public Property Representative As MixedModelKrBlock = Nothing
            Public Property Count As Integer = 0
        End Class

        Private Class MixedModelKrPqrContribution
            Public Property Pmats As Double(,,) = Nothing
            Public Property Qmats As Double(,,,) = Nothing
            Public Property Rmats As Double(,,,) = Nothing
        End Class

        Private Function BuildPqrDesignPatternGroups(blocks As List(Of MixedModelKrBlock),
                                                     includeSecond As Boolean,
                                                     cacheEnabled As Boolean,
                                                     diagnostics As MixedModelKrPqrDesignPatternCacheDiagnostics) As List(Of MixedModelKrPqrDesignPatternGroup)
            Dim groups As New List(Of MixedModelKrPqrDesignPatternGroup)()

            If blocks Is Nothing Then Return groups

            If diagnostics Is Nothing Then diagnostics = New MixedModelKrPqrDesignPatternCacheDiagnostics()
            diagnostics.Enabled = cacheEnabled
            diagnostics.BlockCount = blocks.Count

            If Not cacheEnabled Then
                For Each block As MixedModelKrBlock In blocks
                    groups.Add(New MixedModelKrPqrDesignPatternGroup With {
                        .Key = String.Empty,
                        .Representative = block,
                        .Count = 1
                    })
                Next
                diagnostics.PatternCount = groups.Count
                Return groups
            End If

            Dim byKey As New Dictionary(Of String, List(Of MixedModelKrPqrDesignPatternGroup))(StringComparer.Ordinal)

            For Each block As MixedModelKrBlock In blocks
                Dim key As String = BuildPqrDesignPatternKey(block, includeSecond)
                Dim candidates As List(Of MixedModelKrPqrDesignPatternGroup) = Nothing
                Dim matched As MixedModelKrPqrDesignPatternGroup = Nothing

                If byKey.TryGetValue(key, candidates) Then
                    For Each candidate As MixedModelKrPqrDesignPatternGroup In candidates
                        If BlocksCompatibleForPqrDesignCache(candidate.Representative, block, includeSecond) Then
                            matched = candidate
                            Exit For
                        End If
                    Next

                    If matched Is Nothing AndAlso candidates.Count > 0 Then diagnostics.IncompatibleKeyCollisions += 1
                Else
                    candidates = New List(Of MixedModelKrPqrDesignPatternGroup)()
                    byKey.Add(key, candidates)
                End If

                If matched IsNot Nothing Then
                    matched.Count += 1
                    diagnostics.Hits += 1
                Else
                    Dim created As New MixedModelKrPqrDesignPatternGroup With {
                        .Key = key,
                        .Representative = block,
                        .Count = 1
                    }
                    candidates.Add(created)
                    groups.Add(created)
                    diagnostics.Misses += 1
                End If
            Next

            diagnostics.PatternCount = groups.Count
            Return groups
        End Function

        Private Function BuildPqrDesignPatternKey(block As MixedModelKrBlock,
                                                  includeSecond As Boolean) As String
            Dim sb As New StringBuilder()
            AppendArrayDimensions(sb, "X", block.X)
            AppendArrayDimensions(sb, "V", block.VInv)
            AppendArrayDimensions(sb, "DV", block.DV)
            If includeSecond Then AppendArrayDimensions(sb, "D2V", block.D2V)
            AppendMatrixValues(sb, block.X)
            Return sb.ToString()
        End Function

        Private Sub AppendArrayDimensions(sb As StringBuilder,
                                          label As String,
                                          value As Array)
            sb.Append(label).Append("=")
            If value Is Nothing Then
                sb.Append("Nothing;")
                Exit Sub
            End If

            sb.Append(value.Rank.ToString(CultureInfo.InvariantCulture)).Append(":"c)
            For i As Integer = 0 To value.Rank - 1
                If i > 0 Then sb.Append(","c)
                sb.Append(value.GetLength(i).ToString(CultureInfo.InvariantCulture))
            Next
            sb.Append(";"c)
        End Sub

        Private Sub AppendMatrixValues(sb As StringBuilder,
                                       value(,) As Double)
            If value Is Nothing Then Exit Sub

            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    sb.Append(value(r, c).ToString("R", CultureInfo.InvariantCulture)).Append(";"c)
                Next
            Next
        End Sub

        Private Function BlocksCompatibleForPqrDesignCache(a As MixedModelKrBlock,
                                                           b As MixedModelKrBlock,
                                                           includeSecond As Boolean) As Boolean
            If a Is Nothing OrElse b Is Nothing Then Return False
            If Not MatrixValuesEqual(a.X, b.X) Then Return False
            If Not MatrixValuesEqual(a.VInv, b.VInv) Then Return False
            If Not Tensor3ValuesEqual(a.DV, b.DV) Then Return False
            If includeSecond AndAlso Not Tensor4ValuesEqual(a.D2V, b.D2V) Then Return False
            Return True
        End Function

        Private Function MatrixValuesEqual(a(,) As Double,
                                           b(,) As Double) As Boolean
            If Object.ReferenceEquals(a, b) Then Return True
            If a Is Nothing OrElse b Is Nothing Then Return a Is b
            If a.GetLength(0) <> b.GetLength(0) OrElse a.GetLength(1) <> b.GetLength(1) Then Return False

            For r As Integer = 0 To a.GetLength(0) - 1
                For c As Integer = 0 To a.GetLength(1) - 1
                    If a(r, c) <> b(r, c) Then Return False
                Next
            Next

            Return True
        End Function

        Private Function Tensor3ValuesEqual(a(,,) As Double,
                                            b(,,) As Double) As Boolean
            If Object.ReferenceEquals(a, b) Then Return True
            If a Is Nothing OrElse b Is Nothing Then Return a Is b
            If a.GetLength(0) <> b.GetLength(0) OrElse a.GetLength(1) <> b.GetLength(1) OrElse a.GetLength(2) <> b.GetLength(2) Then Return False

            For h As Integer = 0 To a.GetLength(0) - 1
                For r As Integer = 0 To a.GetLength(1) - 1
                    For c As Integer = 0 To a.GetLength(2) - 1
                        If a(h, r, c) <> b(h, r, c) Then Return False
                    Next
                Next
            Next

            Return True
        End Function

        Private Function Tensor4ValuesEqual(a(,,,) As Double,
                                            b(,,,) As Double) As Boolean
            If Object.ReferenceEquals(a, b) Then Return True
            If a Is Nothing OrElse b Is Nothing Then Return a Is b
            If a.GetLength(0) <> b.GetLength(0) OrElse a.GetLength(1) <> b.GetLength(1) OrElse
               a.GetLength(2) <> b.GetLength(2) OrElse a.GetLength(3) <> b.GetLength(3) Then Return False

            For h As Integer = 0 To a.GetLength(0) - 1
                For j As Integer = 0 To a.GetLength(1) - 1
                    For r As Integer = 0 To a.GetLength(2) - 1
                        For c As Integer = 0 To a.GetLength(3) - 1
                            If a(h, j, r, c) <> b(h, j, r, c) Then Return False
                        Next
                    Next
                Next
            Next

            Return True
        End Function

        Private Sub AddPqrContribution(block As MixedModelKrBlock,
                                      p As Integer,
                                      k As Integer,
                                      includeSecond As Boolean,
                                      pairDiagnostics As MixedModelKrPqrPairDiagnostics,
                                      useFastFactorization As Boolean,
                                      targetP(,,) As Double,
                                      targetQ(,,,) As Double,
                                      targetR(,,,) As Double,
                                      multiplier As Double)
            If useFastFactorization Then
                AddPqrContributionFast(block, p, k, includeSecond, pairDiagnostics, targetP, targetQ, targetR, multiplier)
            Else
                AddPqrContributionDirect(block, p, k, includeSecond, pairDiagnostics, targetP, targetQ, targetR, multiplier)
            End If
        End Sub

        Private Sub AddPqrContributionFast(block As MixedModelKrBlock,
                                           p As Integer,
                                           k As Integer,
                                           includeSecond As Boolean,
                                           pairDiagnostics As MixedModelKrPqrPairDiagnostics,
                                           targetP(,,) As Double,
                                           targetQ(,,,) As Double,
                                           targetR(,,,) As Double,
                                           multiplier As Double)
            Dim leftVInv(,) As Double = XTransposeTimesMatrix(block.X, block.VInv)
            Dim rightVInvX(,) As Double = Matrix.MatrixMult(block.VInv, block.X)
            Dim leftDerivative As New List(Of Double(,))(k)
            Dim rightDerivative As New List(Of Double(,))(k)

            For h As Integer = 0 To k - 1
                Dim leftTimesDvh(,) As Double = MatrixTimesTensor3Slice(leftVInv, block.DV, h)
                AddMatrixProductIntoSlice3D(targetP, h, leftTimesDvh, rightVInvX, multiplier)

                leftDerivative.Add(Matrix.MatrixMult(leftTimesDvh, block.VInv))
                rightDerivative.Add(Tensor3SliceTimesMatrix(block.DV, h, rightVInvX))
            Next

            Dim pairProduct(p - 1, p - 1) As Double

            For h As Integer = 0 To k - 1
                Dim leftH(,) As Double = leftDerivative(h)

                For j As Integer = h To k - 1
                    FillMatrixProduct(leftH, rightDerivative(j), pairProduct)
                    AddScaledMatrixIntoSlice4D(targetQ, h, j, pairProduct, multiplier)
                    pairDiagnostics.QPairMatricesComputed += 1

                    If h <> j Then
                        AddScaledMatrixTransposeIntoSlice4D(targetQ, j, h, pairProduct, multiplier)
                        pairDiagnostics.QPairMatricesFilledBySymmetry += 1
                    End If
                Next
            Next

            If includeSecond Then
                Dim leftTimesSecond(leftVInv.GetLength(0) - 1, block.VInv.GetLength(0) - 1) As Double

                For h As Integer = 0 To k - 1
                    For j As Integer = h To k - 1
                        FillMatrixTimesTensor4Slice(leftVInv, block.D2V, h, j, leftTimesSecond)
                        FillMatrixProduct(leftTimesSecond, rightVInvX, pairProduct)
                        AddScaledMatrixIntoSlice4D(targetR, h, j, pairProduct, multiplier)
                        pairDiagnostics.RPairMatricesComputed += 1

                        If h <> j Then
                            AddScaledMatrixIntoSlice4D(targetR, j, h, pairProduct, multiplier)
                            pairDiagnostics.RPairMatricesFilledBySymmetry += 1
                        End If
                    Next
                Next
            End If
        End Sub

        Private Sub AddPqrContributionDirect(block As MixedModelKrBlock,
                                             p As Integer,
                                             k As Integer,
                                             includeSecond As Boolean,
                                             pairDiagnostics As MixedModelKrPqrPairDiagnostics,
                                             targetP(,,) As Double,
                                             targetQ(,,,) As Double,
                                             targetR(,,,) As Double,
                                             multiplier As Double)
            Dim localP(k - 1, p - 1, p - 1) As Double
            Dim localQ(k - 1, k - 1, p - 1, p - 1) As Double
            Dim localR(,,,) As Double = Nothing
            If includeSecond Then ReDim localR(k - 1, k - 1, p - 1, p - 1)

            For h As Integer = 0 To k - 1
                Dim dVh(,) As Double = Slice3D(block.DV, h)
                Dim vinvDvhVinv(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(block.VInv, dVh), block.VInv)
                Dim ph(,) As Double = XtAX(block.X, vinvDvhVinv)
                AddIntoSlice3D(localP, h, ph)

                For j As Integer = h To k - 1
                    Dim dVj(,) As Double = Slice3D(block.DV, j)
                    Dim qCore(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(vinvDvhVinv, dVj), block.VInv)
                    Dim qhj(,) As Double = XtAX(block.X, qCore)
                    AddIntoSlice4D(localQ, h, j, qhj)
                    pairDiagnostics.QPairMatricesComputed += 1

                    If h <> j Then
                        AddTransposeIntoSlice4D(localQ, j, h, qhj)
                        pairDiagnostics.QPairMatricesFilledBySymmetry += 1
                    End If
                Next
            Next

            If includeSecond Then
                For h As Integer = 0 To k - 1
                    For j As Integer = h To k - 1
                        Dim d2(,) As Double = Slice4D(block.D2V, h, j)
                        Dim core(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(block.VInv, d2), block.VInv)
                        Dim rhj(,) As Double = XtAX(block.X, core)
                        AddIntoSlice4D(localR, h, j, rhj)
                        pairDiagnostics.RPairMatricesComputed += 1

                        If h <> j Then
                            AddIntoSlice4D(localR, j, h, rhj)
                            pairDiagnostics.RPairMatricesFilledBySymmetry += 1
                        End If
                    Next
                Next
            End If

            AddScaledIntoSlice3D(targetP, localP, multiplier)
            AddScaledIntoSlice4D(targetQ, localQ, multiplier)
            If includeSecond Then AddScaledIntoSlice4D(targetR, localR, multiplier)
        End Sub

        ''' <summary>
        ''' Computes the KR adjusted coefficient covariance matrix requested by
        ''' <see cref="MixedModelKrWorkspace.AdjustmentKind"/>.
        ''' </summary>
        ''' <remarks>
        ''' This dispatcher is intentionally strict for Full KR: when Full is requested,
        ''' second derivative matrices R_hj must be available unless
        ''' <see cref="MixedModelKrWorkspace.AllowLinearFallback"/> is True. This avoids
        ''' silently returning the linear KR approximation while labelling it as full KR.
        ''' </remarks>
        Public Function TryComputeAdjustedVarBeta(ws As MixedModelKrWorkspace,
                                                ByRef adjustedVarBeta(,) As Double,
                                                Optional ByRef diagnostic As String = Nothing) As Boolean
            adjustedVarBeta = Nothing
            diagnostic = String.Empty

            Dim timingStopwatch As System.Diagnostics.Stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try

                ThrowIfCancellationRequested(ws)

                If ws Is Nothing Then
                    diagnostic = "KR workspace is Nothing."
                    Return False
                End If

                ReportKrProgress(ws, "Kenward-Roger adjusted Var(beta)", 99, message:="Starting covariance adjustment")

                Select Case ws.AdjustmentKind
                    Case MixedModelKenwardRogerAdjustmentKind.Full
                        If TryComputeFullAdjustedVarBeta(ws, adjustedVarBeta, requireSecondDerivatives:=True, diagnostic:=diagnostic) Then
                            Return True
                        End If

                        If ws.AllowLinearFallback Then
                            Dim fullFailure As String = diagnostic
                            Dim linearDiagnostic As String = String.Empty

                            If TryComputeLinearAdjustedVarBeta(ws, adjustedVarBeta, linearDiagnostic) Then
                                diagnostic = "Full KR adjusted Var(beta) unavailable; used explicit linear fallback. Full failure: " & fullFailure & " Linear diagnostic: " & linearDiagnostic
                                ws.DiagnosticMessage = diagnostic
                                Return True
                            End If

                            diagnostic = "Full KR adjusted Var(beta) unavailable and linear fallback also failed. Full failure: " & fullFailure & " Linear failure: " & linearDiagnostic
                        End If

                        ws.DiagnosticMessage = diagnostic
                        Return False

                    Case MixedModelKenwardRogerAdjustmentKind.Linear
                        Return TryComputeLinearAdjustedVarBeta(ws, adjustedVarBeta, diagnostic)

                    Case MixedModelKenwardRogerAdjustmentKind.None
                        diagnostic = "KR covariance adjustment kind is None."
                        ws.DiagnosticMessage = diagnostic
                        Return False

                    Case Else
                        diagnostic = "Unknown KR covariance adjustment kind: " & ws.AdjustmentKind.ToString() & "."
                        ws.DiagnosticMessage = diagnostic
                        Return False
                End Select
            Finally
                timingStopwatch.Stop()
                RecordKrPerformanceTiming(ws, "KrAdjustedVarBetaTimeMs", timingStopwatch.Elapsed.TotalMilliseconds)
            End Try

        End Function

        ''' <summary>
        ''' Computes the linear KR adjusted coefficient covariance matrix.
        ''' </summary>
        ''' <remarks>
        ''' Linear KR follows the same adjusted covariance formula as full KR but drops
        ''' the second-derivative R_hj contribution. This corresponds to the R mmrm
        ''' "Kenward-Roger-Linear" covariance adjustment.
        ''' </remarks>
        Public Function TryComputeLinearAdjustedVarBeta(ws As MixedModelKrWorkspace,
                                                        ByRef adjustedVarBeta(,) As Double,
                                                        Optional ByRef diagnostic As String = Nothing) As Boolean
            Return TryComputeAdjustedVarBetaCore(ws,
                                                 adjustedVarBeta,
                                                 useSecondOrder:=False,
                                                 requireSecondOrder:=False,
                                                 adjustmentUsed:=MixedModelKenwardRogerAdjustmentKind.Linear,
                                                 diagnostic:=diagnostic)
        End Function

        ''' <summary>
        ''' Computes the full KR adjusted coefficient covariance matrix.
        ''' </summary>
        ''' <remarks>
        ''' Full KR uses the P, Q, and R matrices. For R mmrm matching, callers should
        ''' keep <paramref name="requireSecondDerivatives"/> True so that missing R_hj
        ''' matrices fail loudly rather than degrading to the linear approximation.
        ''' </remarks>
        Public Function TryComputeFullAdjustedVarBeta(ws As MixedModelKrWorkspace,
                                                      ByRef adjustedVarBeta(,) As Double,
                                                      Optional requireSecondDerivatives As Boolean = True,
                                                      Optional ByRef diagnostic As String = Nothing) As Boolean
            Return TryComputeAdjustedVarBetaCore(ws,
                                                 adjustedVarBeta,
                                                 useSecondOrder:=True,
                                                 requireSecondOrder:=requireSecondDerivatives,
                                                 adjustmentUsed:=MixedModelKenwardRogerAdjustmentKind.Full,
                                                 diagnostic:=diagnostic)
        End Function


        Public Function LinearCombinationVariance(l() As Double, varBeta(,) As Double) As Double
            If l Is Nothing OrElse varBeta Is Nothing Then Return Double.NaN
            If varBeta.GetLength(0) <> l.Length OrElse varBeta.GetLength(1) <> l.Length Then Return Double.NaN

            Dim out As Double = 0.0
            For r As Integer = 0 To l.Length - 1
                For c As Integer = 0 To l.Length - 1
                    out += l(r) * varBeta(r, c) * l(c)
                Next
            Next
            Return out
        End Function

        Private Sub RecordKrPerformanceTiming(ws As MixedModelKrWorkspace,
                                             timingFieldName As String,
                                             elapsedMs As Double)
            If ws Is Nothing Then Exit Sub
            If ws.PerformanceDiagnostics Is Nothing Then ws.PerformanceDiagnostics = New MixedModelPerformanceDiagnostics()

            Select Case timingFieldName
                Case "KrPqrMatrixTimeMs"
                    ws.PerformanceDiagnostics.KrPqrMatrixTimeMs = elapsedMs
                Case "KrAdjustedVarBetaTimeMs"
                    ws.PerformanceDiagnostics.KrAdjustedVarBetaTimeMs = elapsedMs
            End Select
        End Sub

        Private Function TryComputeAdjustedVarBetaCore(ws As MixedModelKrWorkspace,
                                                       ByRef adjustedVarBeta(,) As Double,
                                                       useSecondOrder As Boolean,
                                                       requireSecondOrder As Boolean,
                                                       adjustmentUsed As MixedModelKenwardRogerAdjustmentKind,
                                                       Optional ByRef diagnostic As String = Nothing) As Boolean
            adjustedVarBeta = Nothing
            diagnostic = String.Empty

            If ws Is Nothing OrElse Not ws.HasBasicInputs() Then
                diagnostic = "KR workspace is missing basic inputs."
                If ws IsNot Nothing Then ws.DiagnosticMessage = diagnostic
                Return False
            End If

            If ws.Pmats Is Nothing OrElse ws.Qmats Is Nothing Then
                If Not TryBuildKrMatrices(ws, diagnostic) Then Return False
            End If

            Dim p As Integer = ws.P
            Dim k As Integer = ws.K
            Dim phi(,) As Double = ws.VarBeta
            Dim w(,) As Double = ws.ThetaCovariance

            If phi Is Nothing OrElse phi.GetLength(0) <> p OrElse phi.GetLength(1) <> p Then
                diagnostic = "KR VarBeta dimension mismatch."
                ws.DiagnosticMessage = diagnostic
                Return False
            End If

            If w Is Nothing OrElse w.GetLength(0) <> k OrElse w.GetLength(1) <> k Then
                diagnostic = "KR theta covariance dimension mismatch."
                ws.DiagnosticMessage = diagnostic
                Return False
            End If

            If ws.Pmats Is Nothing OrElse ws.Pmats.GetLength(0) <> k OrElse ws.Pmats.GetLength(1) <> p OrElse ws.Pmats.GetLength(2) <> p Then
                diagnostic = "KR P matrices are missing or have invalid dimensions."
                ws.DiagnosticMessage = diagnostic
                Return False
            End If

            If ws.Qmats Is Nothing OrElse ws.Qmats.GetLength(0) <> k OrElse ws.Qmats.GetLength(1) <> k OrElse ws.Qmats.GetLength(2) <> p OrElse ws.Qmats.GetLength(3) <> p Then
                diagnostic = "KR Q matrices are missing or have invalid dimensions."
                ws.DiagnosticMessage = diagnostic
                Return False
            End If

            Dim hasSecondOrder As Boolean = HasConformableSecondDerivativeMatrices(ws)

            If useSecondOrder AndAlso requireSecondOrder AndAlso Not hasSecondOrder Then
                diagnostic = "Full KR adjusted Var(beta) requires conformable R_hj second-derivative matrices. BuildKenwardRogerSecondDerivatives must be True and derivative construction must succeed."
                ws.DiagnosticMessage = diagnostic
                ws.AdjustmentUsed = MixedModelKenwardRogerAdjustmentKind.None
                Return False
            End If

            Dim applySecondOrder As Boolean = useSecondOrder AndAlso hasSecondOrder
            Dim middle(p - 1, p - 1) As Double

            Dim adjustmentPairTotal As Integer = Math.Max(1, k * k)
            Dim adjustmentPairIndex As Integer = 0
            ReportKrProgress(ws, "Kenward-Roger adjusted Var(beta)", 99, 0, adjustmentPairTotal)

            For h As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    adjustmentPairIndex += 1
                    If ShouldReportKrProgress(adjustmentPairIndex, adjustmentPairTotal) Then
                        ReportKrProgress(ws, "Kenward-Roger adjusted Var(beta)", 99, adjustmentPairIndex, adjustmentPairTotal)
                    End If

                    Dim whj As Double = w(h, j)
                    If whj = 0.0 Then Continue For

                    Dim ph(,) As Double = Slice3D(ws.Pmats, h)
                    Dim pj(,) As Double = Slice3D(ws.Pmats, j)
                    Dim qhj(,) As Double = Slice4D(ws.Qmats, h, j)
                    Dim rhj(,) As Double = If(applySecondOrder, Slice4D(ws.Rmats, h, j), Nothing)

                    Dim phPhiPj(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(ph, phi), pj)

                    For r As Integer = 0 To p - 1
                        For c As Integer = 0 To p - 1
                            Dim term As Double = qhj(r, c) - phPhiPj(r, c)

                            If applySecondOrder Then
                                term -= 0.25 * rhj(r, c)
                            End If

                            middle(r, c) += whj * term
                        Next
                    Next
                Next
            Next

            Dim add(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(phi, middle), phi)
            ReDim adjustedVarBeta(p - 1, p - 1)

            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    adjustedVarBeta(r, c) = phi(r, c) + 2.0 * add(r, c)
                Next
            Next

            MixedModelEngine.SymmetrizeInPlace(adjustedVarBeta)

            Dim checkMessage As String = String.Empty
            If Not Matrix.MatrixIsFiniteAndSymmetric(adjustedVarBeta, 0.00000001, checkMessage) Then
                diagnostic = "KR adjusted Var(beta) failed numerical sanity checks: " & checkMessage
                ws.DiagnosticMessage = diagnostic
                ws.AdjustmentUsed = MixedModelKenwardRogerAdjustmentKind.None
                ws.AddNumericalWarning(diagnostic)
                Return False
            End If

            ws.AdjustedVarBetaConditionNumber = MixedModelNumericalDiagnostics.EstimateConditionNumberBySvd(adjustedVarBeta)
            Dim condWarning As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("KR adjusted Var(beta)", ws.AdjustedVarBetaConditionNumber)
            If Not String.IsNullOrWhiteSpace(condWarning) Then ws.AddNumericalWarning(condWarning)

            Dim invCheck(,) As Double = Nothing
            Dim invCheckDiagnostic As String = String.Empty
            If Not MixedModelNumericalDiagnostics.TryInvertSymmetric(adjustedVarBeta, invCheck, invCheckDiagnostic, allowPseudoInverse:=True) Then
                diagnostic = "KR adjusted Var(beta) is symmetric/finite but could not be inverted or pseudo-inverted: " & invCheckDiagnostic
                ws.DiagnosticMessage = diagnostic
                ws.AdjustmentUsed = MixedModelKenwardRogerAdjustmentKind.None
                ws.AddNumericalWarning(diagnostic)
                Return False
            End If
            If invCheckDiagnostic.IndexOf("pseudoinverse", StringComparison.OrdinalIgnoreCase) >= 0 Then
                ws.AddNumericalWarning("KR adjusted Var(beta) required an SVD pseudoinverse numerical check; downstream tests may be unstable.")
            End If

            ws.AdjustedVarBeta = adjustedVarBeta
            ws.AdjustmentUsed = If(applySecondOrder, MixedModelKenwardRogerAdjustmentKind.Full, MixedModelKenwardRogerAdjustmentKind.Linear)

            Dim scaleText As String = ws.ParameterScale.ToString()
            If applySecondOrder Then
                diagnostic = "Full KR adjusted Var(beta) computed on " & scaleText & " parameter scale using P, Q, and R matrices."
            Else
                diagnostic = "Linear KR adjusted Var(beta) computed on " & scaleText & " parameter scale using P and Q matrices only."
            End If

            If Not String.IsNullOrWhiteSpace(ws.NumericalWarningSummary()) Then
                diagnostic &= " Numerical warnings: " & ws.NumericalWarningSummary()
            End If

            ws.DiagnosticMessage = diagnostic
            ReportKrProgress(ws, "Kenward-Roger adjusted Var(beta) complete", 99, adjustmentPairTotal, adjustmentPairTotal)
            Return True
        End Function

        Private Function HasConformableSecondDerivativeMatrices(ws As MixedModelKrWorkspace) As Boolean
            If ws Is Nothing OrElse ws.Rmats Is Nothing Then Return False
            Return ws.Rmats.GetLength(0) = ws.K AndAlso
                   ws.Rmats.GetLength(1) = ws.K AndAlso
                   ws.Rmats.GetLength(2) = ws.P AndAlso
                   ws.Rmats.GetLength(3) = ws.P
        End Function


        ''' <summary>
        ''' Enforces the exact matrix symmetries of the KR first-order P/Q ingredients after aggregation.
        ''' </summary>
        ''' <remarks>
        ''' The P_h and diagonal Q_hh slices are mathematically symmetric, and Q_jh is the transpose of
        ''' Q_hj. Very ill-conditioned covariance fits can amplify ordinary matrix-multiplication roundoff
        ''' enough that validation sees small relative asymmetry in these large matrices. Normalizing the
        ''' slices here preserves the KR identities and keeps downstream tests/inference on the intended
        ''' symmetric algebraic path.
        ''' </remarks>
        Private Sub NormalizeKrFirstOrderMatrixSymmetry(pMats(,,) As Double,
                                                        qMats(,,,) As Double)
            If pMats IsNot Nothing Then
                For h As Integer = 0 To pMats.GetLength(0) - 1
                    SymmetrizeSlice3D(pMats, h)
                Next
            End If

            If qMats Is Nothing Then Exit Sub

            Dim k As Integer = qMats.GetLength(0)
            For h As Integer = 0 To k - 1
                SymmetrizeSlice4D(qMats, h, h)

                For j As Integer = h + 1 To k - 1
                    CopyTransposeSlice4D(qMats, h, j, j, h)
                Next
            Next
        End Sub

        ''' <summary>
        ''' Enforces the exact matrix and parameter-pair symmetries of the KR second-order R ingredients.
        ''' </summary>
        Private Sub NormalizeKrSecondOrderMatrixSymmetry(rMats(,,,) As Double)
            If rMats Is Nothing Then Exit Sub

            Dim k As Integer = rMats.GetLength(0)
            For h As Integer = 0 To k - 1
                For j As Integer = h To k - 1
                    SymmetrizeSlice4D(rMats, h, j)
                    If h <> j Then CopySlice4D(rMats, h, j, j, h)
                Next
            Next
        End Sub

        Private Sub SymmetrizeSlice3D(target(,,) As Double, h As Integer)
            If target Is Nothing Then Exit Sub
            Dim rows As Integer = target.GetLength(1)
            Dim cols As Integer = target.GetLength(2)
            If rows <> cols Then Exit Sub

            For r As Integer = 0 To rows - 1
                For c As Integer = r + 1 To cols - 1
                    Dim v As Double = 0.5 * (target(h, r, c) + target(h, c, r))
                    target(h, r, c) = v
                    target(h, c, r) = v
                Next
            Next
        End Sub

        Private Sub SymmetrizeSlice4D(target(,,,) As Double, h As Integer, j As Integer)
            If target Is Nothing Then Exit Sub
            Dim rows As Integer = target.GetLength(2)
            Dim cols As Integer = target.GetLength(3)
            If rows <> cols Then Exit Sub

            For r As Integer = 0 To rows - 1
                For c As Integer = r + 1 To cols - 1
                    Dim v As Double = 0.5 * (target(h, j, r, c) + target(h, j, c, r))
                    target(h, j, r, c) = v
                    target(h, j, c, r) = v
                Next
            Next
        End Sub

        Private Sub CopyTransposeSlice4D(target(,,,) As Double,
                                         sourceH As Integer,
                                         sourceJ As Integer,
                                         destH As Integer,
                                         destJ As Integer)
            If target Is Nothing Then Exit Sub

            Dim rows As Integer = target.GetLength(2)
            Dim cols As Integer = target.GetLength(3)
            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    target(destH, destJ, r, c) = target(sourceH, sourceJ, c, r)
                Next
            Next
        End Sub

        Private Sub CopySlice4D(target(,,,) As Double,
                                sourceH As Integer,
                                sourceJ As Integer,
                                destH As Integer,
                                destJ As Integer)
            If target Is Nothing Then Exit Sub

            Dim rows As Integer = target.GetLength(2)
            Dim cols As Integer = target.GetLength(3)
            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    target(destH, destJ, r, c) = target(sourceH, sourceJ, r, c)
                Next
            Next
        End Sub

        Private Function MatrixTimesTensor3Slice(left(,) As Double,
                                                   tensor(,,) As Double,
                                                   h As Integer) As Double(,)
            Dim leftRows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)

            If tensor.GetLength(1) <> shrd Then
                Throw New ArgumentException("Tensor slice row count must conform with the left matrix column count.")
            End If

            Dim cols As Integer = tensor.GetLength(2)
            Dim out(leftRows - 1, cols - 1) As Double

            For r As Integer = 0 To leftRows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * tensor(h, t, c)
                    Next
                    out(r, c) = s
                Next
            Next

            Return out
        End Function

        Private Function Tensor3SliceTimesMatrix(tensor(,,) As Double,
                                                 h As Integer,
                                                 right(,) As Double) As Double(,)
            Dim rows As Integer = tensor.GetLength(1)
            Dim shrd As Integer = tensor.GetLength(2)

            If right.GetLength(0) <> shrd Then
                Throw New ArgumentException("Right matrix row count must conform with the tensor slice column count.")
            End If

            Dim cols As Integer = right.GetLength(1)
            Dim out(rows - 1, cols - 1) As Double

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += tensor(h, r, t) * right(t, c)
                    Next
                    out(r, c) = s
                Next
            Next

            Return out
        End Function

        Private Sub FillMatrixTimesTensor4Slice(left(,) As Double,
                                                tensor(,,,) As Double,
                                                h As Integer,
                                                j As Integer,
                                                output(,) As Double)
            Dim rows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)

            If tensor.GetLength(2) <> shrd Then
                Throw New ArgumentException("Tensor slice row count must conform with the left matrix column count.")
            End If

            Dim cols As Integer = tensor.GetLength(3)
            If output.GetLength(0) <> rows OrElse output.GetLength(1) <> cols Then
                Throw New ArgumentException("Output matrix does not have the required dimensions.")
            End If

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * tensor(h, j, t, c)
                    Next
                    output(r, c) = s
                Next
            Next
        End Sub

        Private Sub FillMatrixProduct(left(,) As Double,
                                      right(,) As Double,
                                      output(,) As Double)
            If left Is Nothing OrElse right Is Nothing OrElse output Is Nothing Then Exit Sub

            Dim rows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)
            If right.GetLength(0) <> shrd Then
                Throw New ArgumentException("Matrix dimensions do not conform.")
            End If

            Dim cols As Integer = right.GetLength(1)
            If output.GetLength(0) <> rows OrElse output.GetLength(1) <> cols Then
                Throw New ArgumentException("Output matrix does not have the required dimensions.")
            End If

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * right(t, c)
                    Next
                    output(r, c) = s
                Next
            Next
        End Sub

        Private Sub AddScaledMatrixIntoSlice4D(target(,,,) As Double,
                                               h As Integer,
                                               j As Integer,
                                               value(,) As Double,
                                               multiplier As Double)
            If target Is Nothing OrElse value Is Nothing Then Exit Sub

            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    target(h, j, r, c) += multiplier * value(r, c)
                Next
            Next
        End Sub

        Private Sub AddScaledMatrixTransposeIntoSlice4D(target(,,,) As Double,
                                                        h As Integer,
                                                        j As Integer,
                                                        value(,) As Double,
                                                        multiplier As Double)
            If target Is Nothing OrElse value Is Nothing Then Exit Sub

            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    target(h, j, c, r) += multiplier * value(r, c)
                Next
            Next
        End Sub

        Private Sub AddMatrixProductIntoSlice3D(target(,,) As Double,
                                                h As Integer,
                                                left(,) As Double,
                                                right(,) As Double,
                                                multiplier As Double)
            If target Is Nothing OrElse left Is Nothing OrElse right Is Nothing Then Exit Sub

            Dim rows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)
            If right.GetLength(0) <> shrd Then
                Throw New ArgumentException("Matrix dimensions do not conform.")
            End If
            Dim cols As Integer = right.GetLength(1)

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * right(t, c)
                    Next
                    target(h, r, c) += multiplier * s
                Next
            Next
        End Sub

        Private Sub AddMatrixProductIntoSlice4D(target(,,,) As Double,
                                                h As Integer,
                                                j As Integer,
                                                left(,) As Double,
                                                right(,) As Double,
                                                multiplier As Double)
            If target Is Nothing OrElse left Is Nothing OrElse right Is Nothing Then Exit Sub

            Dim rows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)
            If right.GetLength(0) <> shrd Then
                Throw New ArgumentException("Matrix dimensions do not conform.")
            End If
            Dim cols As Integer = right.GetLength(1)

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * right(t, c)
                    Next
                    target(h, j, r, c) += multiplier * s
                Next
            Next
        End Sub

        Private Sub AddMatrixProductTransposeIntoSlice4D(target(,,,) As Double,
                                                         h As Integer,
                                                         j As Integer,
                                                         left(,) As Double,
                                                         right(,) As Double,
                                                         multiplier As Double)
            If target Is Nothing OrElse left Is Nothing OrElse right Is Nothing Then Exit Sub

            Dim rows As Integer = left.GetLength(0)
            Dim shrd As Integer = left.GetLength(1)
            If right.GetLength(0) <> shrd Then
                Throw New ArgumentException("Matrix dimensions do not conform.")
            End If
            Dim cols As Integer = right.GetLength(1)

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For t As Integer = 0 To shrd - 1
                        s += left(r, t) * right(t, c)
                    Next
                    target(h, j, c, r) += multiplier * s
                Next
            Next
        End Sub

        Private Function XTransposeTimesMatrix(x(,) As Double, a(,) As Double) As Double(,)
            Dim n As Integer = x.GetLength(0)
            Dim p As Integer = x.GetLength(1)

            If a.GetLength(0) <> n Then Throw New ArgumentException("A row count must conform with X row count.")

            Dim cols As Integer = a.GetLength(1)
            Dim out(p - 1, cols - 1) As Double

            For r As Integer = 0 To p - 1
                For c As Integer = 0 To cols - 1
                    Dim s As Double = 0.0
                    For i As Integer = 0 To n - 1
                        s += x(i, r) * a(i, c)
                    Next
                    out(r, c) = s
                Next
            Next

            Return out
        End Function

        Private Function XtAX(x(,) As Double, a(,) As Double) As Double(,)
            Dim n As Integer = x.GetLength(0)
            Dim p As Integer = x.GetLength(1)

            If a.GetLength(0) <> n OrElse a.GetLength(1) <> n Then
                Throw New ArgumentException("A must be n x n and conformable with X.")
            End If

            Dim out(p - 1, p - 1) As Double

            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    Dim s As Double = 0.0
                    For i As Integer = 0 To n - 1
                        For j As Integer = 0 To n - 1
                            s += x(i, r) * a(i, j) * x(j, c)
                        Next
                    Next
                    out(r, c) = s
                Next
            Next

            Return out
        End Function

        Friend Function Slice3D(a(,,) As Double, h As Integer) As Double(,)
            Dim n1 As Integer = a.GetLength(1)
            Dim n2 As Integer = a.GetLength(2)
            Dim out(n1 - 1, n2 - 1) As Double

            For r As Integer = 0 To n1 - 1
                For c As Integer = 0 To n2 - 1
                    out(r, c) = a(h, r, c)
                Next
            Next

            Return out
        End Function

        Private Function Slice4D(a(,,,) As Double, h As Integer, j As Integer) As Double(,)
            Dim n1 As Integer = a.GetLength(2)
            Dim n2 As Integer = a.GetLength(3)
            Dim out(n1 - 1, n2 - 1) As Double

            For r As Integer = 0 To n1 - 1
                For c As Integer = 0 To n2 - 1
                    out(r, c) = a(h, j, r, c)
                Next
            Next

            Return out
        End Function

        Private Sub AddIntoSlice3D(target(,,) As Double, h As Integer, value(,) As Double)
            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    target(h, r, c) += value(r, c)
                Next
            Next
        End Sub

        Private Sub AddIntoSlice4D(target(,,,) As Double, h As Integer, j As Integer, value(,) As Double)
            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    target(h, j, r, c) += value(r, c)
                Next
            Next
        End Sub

        Private Sub AddTransposeIntoSlice4D(target(,,,) As Double,
                                            h As Integer,
                                            j As Integer,
                                            value(,) As Double)
            For r As Integer = 0 To value.GetLength(0) - 1
                For c As Integer = 0 To value.GetLength(1) - 1
                    target(h, j, c, r) += value(r, c)
                Next
            Next
        End Sub

        Private Sub AddScaledIntoSlice3D(target(,,) As Double,
                                         value(,,) As Double,
                                         multiplier As Double)
            If target Is Nothing OrElse value Is Nothing Then Exit Sub

            For h As Integer = 0 To value.GetLength(0) - 1
                For r As Integer = 0 To value.GetLength(1) - 1
                    For c As Integer = 0 To value.GetLength(2) - 1
                        target(h, r, c) += multiplier * value(h, r, c)
                    Next
                Next
            Next
        End Sub

        Private Sub AddScaledIntoSlice4D(target(,,,) As Double,
                                         value(,,,) As Double,
                                         multiplier As Double)
            If target Is Nothing OrElse value Is Nothing Then Exit Sub

            For h As Integer = 0 To value.GetLength(0) - 1
                For j As Integer = 0 To value.GetLength(1) - 1
                    For r As Integer = 0 To value.GetLength(2) - 1
                        For c As Integer = 0 To value.GetLength(3) - 1
                            target(h, j, r, c) += multiplier * value(h, j, r, c)
                        Next
                    Next
                Next
            Next
        End Sub

        Private Function TransposeMatrix(value(,) As Double) As Double(,)
            If value Is Nothing Then Return Nothing

            Dim rows As Integer = value.GetLength(0)
            Dim cols As Integer = value.GetLength(1)
            Dim out(cols - 1, rows - 1) As Double

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    out(c, r) = value(r, c)
                Next
            Next

            Return out
        End Function

    End Module

End Namespace
