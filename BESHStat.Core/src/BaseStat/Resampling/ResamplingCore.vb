Option Explicit On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace Resampling

    ''' <summary>
    ''' Enumerates the confidence-interval construction method used by the shared resampling infrastructure.
    ''' </summary>
    Public Enum ResamplingCiMethod
        Analytical = 0
        Jackknife = 1
        BootstrapPercentile = 2
        BootstrapBCa = 3
    End Enum

    ''' <summary>
    ''' Enumerates the strategy used to generate the permutation reference distribution.
    ''' </summary>
    Public Enum PermutationMode
        ExactEnumeration = 0
        MonteCarlo = 1
    End Enum

    ''' <summary>
    ''' Enumerates the tail direction used for permutation- or bootstrap-based hypothesis testing.
    ''' </summary>
    Public Enum AlternativeHypothesis
        TwoSided = 0
        Less = 1
        Greater = 2
    End Enum

    ''' <summary>
    ''' Common bootstrap options shared across statistical methods.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This type uses <see cref="Double.NaN"/> as the sentinel value meaning “no explicit alpha was supplied”.
    ''' In that case the engine falls back to the application-wide default alpha from <see cref="CoreServices.AnalysisDefaults"/>.
    ''' </para>
    ''' <para>
    ''' The random-seed handling mirrors the current project-wide pattern: <see cref="Integer.MinValue"/> means
    ''' “no explicit seed was supplied”, so the engine falls back to <see cref="CoreServices.AnalysisDefaults"/>.
    ''' </para>
    ''' </remarks>
    Public Class BootstrapOptions

        ''' <summary>
        ''' Gets or sets the two-sided significance level used for confidence intervals.
        ''' </summary>
        ''' <remarks>
        ''' Use <see cref="Double.NaN"/> to indicate that no explicit alpha was supplied and the application default alpha should be used.
        ''' </remarks>
        Public Property Alpha As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the requested number of bootstrap replicates.
        ''' </summary>
        Public Property Replicates As Integer = 2000

        ''' <summary>
        ''' Gets or sets the explicit random seed.
        ''' </summary>
        ''' <remarks>
        ''' Use <see cref="Integer.MinValue"/> to indicate that no explicit seed was supplied.
        ''' </remarks>
        Public Property RandomSeed As Integer = Integer.MinValue

        ''' <summary>
        ''' Gets or sets the maximum number of failed bootstrap replicates tolerated before the run is aborted.
        ''' </summary>
        Public Property MaxFailures As Integer = 1000

    End Class

    ''' <summary>
    ''' Common jackknife options shared across statistical methods.
    ''' </summary>
    Public Class JackknifeOptions

        ''' <summary>
        ''' Gets or sets the two-sided significance level used for confidence intervals.
        ''' </summary>
        ''' <remarks>
        ''' Use <see cref="Double.NaN"/> to indicate that no explicit alpha was supplied and the application default alpha should be used.
        ''' </remarks>
        Public Property Alpha As Double = Double.NaN

    End Class

    ''' <summary>
    ''' Common permutation-test options shared across statistical methods.
    ''' </summary>
    Public Class PermutationOptions

        ''' <summary>
        ''' Gets or sets the significance level associated with the permutation analysis.
        ''' </summary>
        ''' <remarks>
        ''' Use <see cref="Double.NaN"/> to indicate that no explicit alpha was supplied and the application default alpha should be used.
        ''' </remarks>
        Public Property Alpha As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the alternative hypothesis used to evaluate the empirical p-value.
        ''' </summary>
        Public Property Alternative As AlternativeHypothesis = AlternativeHypothesis.TwoSided

        ''' <summary>
        ''' Gets or sets whether the permutation analysis uses exact enumeration or Monte Carlo sampling.
        ''' </summary>
        Public Property Mode As PermutationMode = PermutationMode.MonteCarlo

        ''' <summary>
        ''' Gets or sets the requested number of Monte Carlo permutations.
        ''' </summary>
        Public Property MonteCarloReplicates As Integer = 10000

        ''' <summary>
        ''' Gets or sets the explicit random seed.
        ''' </summary>
        ''' <remarks>
        ''' Use <see cref="Integer.MinValue"/> to indicate that no explicit seed was supplied.
        ''' </remarks>
        Public Property RandomSeed As Integer = Integer.MinValue

        ''' <summary>
        ''' Gets or sets the upper bound on the number of exact permutations the engine is allowed to enumerate.
        ''' </summary>
        Public Property MaxExactEnumerations As Long = 2000000

        ''' <summary>
        ''' Gets or sets whether the empirical p-value should use the common “add-one” correction.
        ''' </summary>
        Public Property UseAddOneCorrection As Boolean = True

    End Class

    ''' <summary>
    ''' Stores metadata about a completed bootstrap, jackknife, or permutation run.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This type is designed to be attached to method-specific result objects so output sheets can report
    ''' reproducibility information such as the seed used, the requested and successful replicate counts,
    ''' and any fallback notes.
    ''' </para>
    ''' </remarks>
    Public Class ResamplingRunInfo

        ''' <summary>
        ''' Gets or sets the resolved seed used by the RNG.
        ''' </summary>
        Public Property SeedUsed As Integer = Integer.MinValue

        ''' <summary>
        ''' Gets or sets the effective alpha level used by the run after fallback to global defaults.
        ''' </summary>
        Public Property AlphaUsed As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the number of resamples requested by the caller.
        ''' </summary>
        Public Property ReplicatesRequested As Integer = 0

        ''' <summary>
        ''' Gets or sets the number of successful resamples actually used.
        ''' </summary>
        Public Property ReplicatesUsed As Integer = 0

        ''' <summary>
        ''' Gets or sets the number of failed or discarded resamples.
        ''' </summary>
        Public Property FailedReplicates As Integer = 0

        ''' <summary>
        ''' Gets or sets a short descriptive label for the method that produced the resampling run.
        ''' </summary>
        Public Property MethodLabel As String = String.Empty

        ''' <summary>
        ''' Gets or sets free-form notes about the resampling run.
        ''' </summary>
        Public Property Notes As New List(Of String)

    End Class

    ''' <summary>
    ''' Core helpers shared by bootstrap, jackknife, and permutation engines.
    ''' </summary>
    Public Module ResamplingCore

        ''' <summary>
        ''' Resolves the concrete alpha level that should be used for a resampling run.
        ''' </summary>
        ''' <param name="requestedAlpha">
        ''' Explicit alpha requested by the caller. Use <see cref="Double.NaN"/> to indicate that no explicit alpha was supplied.
        ''' </param>
        ''' <returns>
        ''' The explicit alpha if provided and valid; otherwise the application-wide default alpha from <see cref="CoreServices.AnalysisDefaults"/>.
        ''' </returns>
        ''' <remarks>
        ''' This function gives the resampling infrastructure the same global-settings behavior for alpha that it already has for random seeds.
        ''' </remarks>
        Public Function ResolveAlpha(Optional requestedAlpha As Double = Double.NaN) As Double
            If Not Double.IsNaN(requestedAlpha) Then
                ValidateAlpha(requestedAlpha)
                Return requestedAlpha
            End If

            Return CoreServices.AnalysisDefaults.ResolveAlpha()
        End Function

        ''' <summary>
        ''' Resolves the concrete pseudo-random seed that should be used for a resampling run.
        ''' </summary>
        ''' <param name="requestedSeed">
        ''' Explicit seed requested by the caller. Use <see cref="Integer.MinValue"/> to indicate that no explicit seed was supplied.
        ''' </param>
        ''' <returns>
        ''' The explicit seed if provided; otherwise the application-wide default seed from <see cref="CoreServices.AnalysisDefaults"/>;
        ''' otherwise a time-based seed derived from <see cref="Environment.TickCount"/>.
        ''' </returns>
        Public Function ResolveSeed(Optional requestedSeed As Integer = Integer.MinValue) As Integer
            Return CoreServices.AnalysisDefaults.ResolveRandomSeed(requestedSeed, generateWhenMissing:=True)
        End Function

        ''' <summary>
        ''' Creates a <see cref="Random"/> instance together with the concrete seed used to initialize it.
        ''' </summary>
        ''' <param name="requestedSeed">
        ''' Optional explicit seed. Use <see cref="Integer.MinValue"/> to request fallback to the application default seed.
        ''' </param>
        ''' <returns>
        ''' A tuple containing the initialized random-number generator and the resolved seed used to create it.
        ''' </returns>
        Public Function CreateRandomWithResolvedSeed(Optional requestedSeed As Integer = Integer.MinValue) As (Rng As Random, SeedUsed As Integer)
            Return CoreServices.AnalysisDefaults.CreateRandomWithResolvedSeed(requestedSeed)
        End Function

        ''' <summary>
        ''' Creates a new <see cref="ResamplingRunInfo"/> instance initialized for a specific method and replicate request.
        ''' </summary>
        ''' <param name="methodLabel">Short human-readable method label, such as "Lin CCC bootstrap".</param>
        ''' <param name="replicatesRequested">Number of resamples requested by the caller.</param>
        ''' <param name="seedUsed">Concrete RNG seed used for the run.</param>
        ''' <param name="alphaUsed">Effective alpha used by the run after fallback to global defaults.</param>
        ''' <returns>A new initialized <see cref="ResamplingRunInfo"/> object.</returns>
        Public Function CreateRunInfo(methodLabel As String,
                                      replicatesRequested As Integer,
                                      seedUsed As Integer,
                                      alphaUsed As Double) As ResamplingRunInfo
            Return New ResamplingRunInfo With {
                .MethodLabel = If(methodLabel, String.Empty),
                .ReplicatesRequested = replicatesRequested,
                .SeedUsed = seedUsed,
                .AlphaUsed = alphaUsed,
                .ReplicatesUsed = 0,
                .FailedReplicates = 0
            }
        End Function

        ''' <summary>
        ''' Normalizes a possibly missing bootstrap-options object and validates its contents.
        ''' </summary>
        ''' <param name="opts">Bootstrap options supplied by the caller, or <c>Nothing</c>.</param>
        ''' <returns>A non-null validated <see cref="BootstrapOptions"/> instance.</returns>
        Public Function NormalizeBootstrapOptions(opts As BootstrapOptions) As BootstrapOptions
            Dim out As BootstrapOptions = If(opts, New BootstrapOptions())
            out.Alpha = ResolveAlpha(out.Alpha)
            ValidateBootstrapOptions(out)
            Return out
        End Function

        ''' <summary>
        ''' Normalizes a possibly missing jackknife-options object and validates its contents.
        ''' </summary>
        ''' <param name="opts">Jackknife options supplied by the caller, or <c>Nothing</c>.</param>
        ''' <returns>A non-null validated <see cref="JackknifeOptions"/> instance.</returns>
        Public Function NormalizeJackknifeOptions(opts As JackknifeOptions) As JackknifeOptions
            Dim out As JackknifeOptions = If(opts, New JackknifeOptions())
            out.Alpha = ResolveAlpha(out.Alpha)
            ValidateJackknifeOptions(out)
            Return out
        End Function

        ''' <summary>
        ''' Normalizes a possibly missing permutation-options object and validates its contents.
        ''' </summary>
        ''' <param name="opts">Permutation options supplied by the caller, or <c>Nothing</c>.</param>
        ''' <returns>A non-null validated <see cref="PermutationOptions"/> instance.</returns>
        Public Function NormalizePermutationOptions(opts As PermutationOptions) As PermutationOptions
            Dim out As PermutationOptions = If(opts, New PermutationOptions())
            out.Alpha = ResolveAlpha(out.Alpha)
            ValidatePermutationOptions(out)
            Return out
        End Function

        ''' <summary>
        ''' Validates a bootstrap-options object.
        ''' </summary>
        ''' <param name="opts">Options to validate.</param>
        Public Sub ValidateBootstrapOptions(opts As BootstrapOptions)
            If opts Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            ValidateAlpha(opts.Alpha)
            ValidatePositiveReplicates(opts.Replicates, NameOf(opts.Replicates), 1)
            ValidatePositiveReplicates(opts.MaxFailures, NameOf(opts.MaxFailures), 0)
        End Sub

        ''' <summary>
        ''' Validates a jackknife-options object.
        ''' </summary>
        ''' <param name="opts">Options to validate.</param>
        Public Sub ValidateJackknifeOptions(opts As JackknifeOptions)
            If opts Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            ValidateAlpha(opts.Alpha)
        End Sub

        ''' <summary>
        ''' Validates a permutation-options object.
        ''' </summary>
        ''' <param name="opts">Options to validate.</param>
        Public Sub ValidatePermutationOptions(opts As PermutationOptions)
            If opts Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            ValidateAlpha(opts.Alpha)
            ValidatePositiveReplicates(opts.MonteCarloReplicates, NameOf(opts.MonteCarloReplicates), 1)
            ValidatePositiveLong(opts.MaxExactEnumerations, NameOf(opts.MaxExactEnumerations), 1)
        End Sub

        ''' <summary>
        ''' Appends a non-empty note to a resampling run-info object.
        ''' </summary>
        ''' <param name="info">Run metadata object that will receive the note.</param>
        ''' <param name="note">Note text to append.</param>
        Public Sub AppendNote(info As ResamplingRunInfo, note As String)
            If info Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))
            If String.IsNullOrWhiteSpace(note) Then Exit Sub
            info.Notes.Add(note.Trim())
        End Sub

        ''' <summary>
        ''' Completes a run-info object with final successful and failed replicate counts.
        ''' </summary>
        ''' <param name="info">Run metadata object to update.</param>
        ''' <param name="replicatesUsed">Number of successful resamples used.</param>
        ''' <param name="failedReplicates">Number of failed or discarded resamples.</param>
        Public Sub CompleteRunInfo(info As ResamplingRunInfo, replicatesUsed As Integer, failedReplicates As Integer)
            If info Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(info)))
            If replicatesUsed < 0 Then CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(replicatesUsed)))
            If failedReplicates < 0 Then CoreServices.Errors.LogAndThrow(New ArgumentOutOfRangeException(NameOf(failedReplicates)))

            info.ReplicatesUsed = replicatesUsed
            info.FailedReplicates = failedReplicates
        End Sub

    End Module

End Namespace
