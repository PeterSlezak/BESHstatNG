Option Explicit On
Option Strict On
Option Infer On

Imports System

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Identifies the broad statistical family to which a control chart belongs.
    ''' </summary>
    Public Enum SpcChartFamily
        Run = 0
        ShewhartVariables = 1
        ShewhartAttributes = 2
        TimeWeighted = 3
        RareEvent = 4
        Multivariate = 5
        Specialized = 6
    End Enum

    ''' <summary>
    ''' Identifies a control-chart analysis exposed by the SPC engine.
    ''' </summary>
    ''' <remarks>
    ''' Values are grouped in numeric blocks so that additional charts can be added
    ''' without changing existing persisted values. A member appearing here defines
    ''' the public vocabulary; the engine remains responsible for reporting whether
    ''' that chart is implemented in the current release.
    ''' </remarks>
    Public Enum SpcChartType
        RunChart = 0

        Individuals = 100
        MovingRange = 101
        IndividualsMovingRange = 102
        XBar = 110
        SubgroupRange = 111
        SubgroupStandardDeviation = 112
        XBarR = 113
        XBarS = 114

        PChart = 200
        NpChart = 201
        CChart = 202
        UChart = 203
        LaneyPPrime = 210
        LaneyUPrime = 211

        GChart = 220
        TChart = 221

        Cusum = 300
        Ewma = 301
        MovingAverage = 302

        HotellingT2 = 400
        GeneralizedVariance = 401
        PcaT2 = 402
        PcaQ = 403
        Mewma = 410
        Mcusum = 411

        ShortRunZMovingRange = 500
        BetweenWithin = 501
        ResidualChart = 502
        ProfileChart = 503
        RiskAdjustedChart = 504
    End Enum

    ''' <summary>
    ''' Identifies the statistic displayed in one result panel.
    ''' </summary>
    ''' <remarks>
    ''' A composite analysis such as X-bar/R or I-MR produces two panels, each with
    ''' its own statistic, centre line, limits, and signals.
    ''' </remarks>
    Public Enum SpcPanelType
        Run = 0
        IndividualValue = 1
        MovingRange = 2
        SubgroupMean = 3
        SubgroupRange = 4
        SubgroupStandardDeviation = 5
        Proportion = 6
        NumberNonconforming = 7
        DefectCount = 8
        DefectRate = 9
        StandardizedProportion = 10
        StandardizedDefectRate = 11
        EventsBetweenOccurrences = 12
        TimeBetweenOccurrences = 13
        UpperCusum = 14
        LowerCusum = 15
        Ewma = 16
        MovingAverage = 17
        HotellingT2 = 18
        GeneralizedVariance = 19
        PcaT2 = 20
        PcaQ = 21
        Mewma = 22
        Mcusum = 23
        StandardizedValue = 24
        Residual = 25
        ProfileStatistic = 26
        RiskAdjustedStatistic = 27
    End Enum

    ''' <summary>
    ''' Describes how source observations are arranged before host-neutral arrays are built.
    ''' </summary>
    Public Enum SpcDataLayout
        ''' <summary>One subgroup per row and repeated measurements across columns.</summary>
        WideSubgroups = 0

        ''' <summary>One measurement per row with a separate subgroup identifier.</summary>
        StackedObservations = 1

        ''' <summary>One ordered observation per point, as used by I-MR and run charts.</summary>
        IndividualSequence = 2

        ''' <summary>Pre-aggregated counts together with sample size or exposure.</summary>
        AggregatedCounts = 3
    End Enum

    ''' <summary>
    ''' Specifies how missing measurements are handled during chart-point construction.
    ''' </summary>
    Public Enum SpcMissingValuePolicy
        ''' <summary>Reject the analysis when a required value is missing.</summary>
        Reject = 0

        ''' <summary>Omit the complete chart point or subgroup containing a missing value.</summary>
        OmitPoint = 1

        ''' <summary>
        ''' Use the available measurements and retain the effective subgroup size.
        ''' Chart-specific minimum subgroup sizes still apply.
        ''' </summary>
        UseAvailableMeasurements = 2
    End Enum

    ''' <summary>
    ''' Identifies whether observations establish limits or monitor a process against fixed limits.
    ''' </summary>
    Public Enum SpcPhase
        PhaseI = 0
        PhaseII = 1
    End Enum

    ''' <summary>
    ''' Specifies how a stage obtains its centre line and control limits.
    ''' </summary>
    Public Enum SpcStageLimitMode
        ''' <summary>Estimate parameters from eligible observations in this stage.</summary>
        EstimateFromStageData = 0

        ''' <summary>Reuse the frozen parameters and limits associated with another stage.</summary>
        UseReferenceStage = 1

        ''' <summary>Use historical parameters supplied in the fit request.</summary>
        UseHistoricalParameters = 2
    End Enum

    ''' <summary>
    ''' Specifies the default source of process parameters when no stage-specific override is supplied.
    ''' </summary>
    Public Enum SpcParameterSource
        EstimateFromPhaseI = 0
        UseHistoricalParameters = 1
        DefinedByStage = 2
    End Enum

    ''' <summary>
    ''' Specifies the method used to construct control limits.
    ''' </summary>
    Public Enum SpcControlLimitMethod
        ''' <summary>Traditional Shewhart limits based on a sigma multiplier.</summary>
        ShewhartSigma = 0

        ''' <summary>
        ''' Distribution-quantile limits for discrete charts, using coverage implied by
        ''' the configured sigma multiplier.
        ''' </summary>
        ExactProbability = 1
    End Enum

    ''' <summary>
    ''' Specifies how within-process sigma is estimated for variable charts.
    ''' </summary>
    Public Enum SpcWithinSigmaEstimator
        Automatic = 0
        AverageRange = 1
        AverageStandardDeviation = 2
        PooledStandardDeviation = 3
        MovingRange = 4
        MedianMovingRange = 5
        SampleStandardDeviation = 6
        MedianAbsoluteDeviation = 7
    End Enum

    ''' <summary>
    ''' Specifies how mathematically possible but physically impossible limits are handled.
    ''' </summary>
    Public Enum SpcNaturalLimitPolicy
        ''' <summary>
        ''' Clip lower limits to zero and, for proportions, upper limits to one.
        ''' </summary>
        ClipToFeasibleRange = 0

        ''' <summary>Retain the unmodified calculated limits.</summary>
        RetainCalculatedLimits = 1
    End Enum

    ''' <summary>
    ''' Identifies a named collection of special-cause rules.
    ''' </summary>
    Public Enum SpcRulePreset
        None = 0
        RuleOneOnly = 1
        WesternElectric = 2
        Nelson = 3
        PaperMontgomeryEightRules = 4
        Custom = 5
    End Enum

    ''' <summary>
    ''' Identifies the sequence pattern evaluated by a special-cause rule.
    ''' </summary>
    Public Enum SpcRuleKind
        ''' <summary>One or more points beyond a specified sigma distance.</summary>
        BeyondSigma = 0

        ''' <summary>K of M consecutive points beyond a sigma distance on the same side.</summary>
        KOfMConsecutiveBeyondSigma = 1

        ''' <summary>A consecutive run strictly above or below the centre line.</summary>
        RunOnOneSide = 2

        ''' <summary>A strictly increasing or strictly decreasing sequence.</summary>
        MonotonicTrend = 3

        ''' <summary>A sequence whose direction alternates at every step.</summary>
        Alternating = 4

        ''' <summary>All points in the window fall within the specified sigma zone.</summary>
        AllWithinSigma = 5

        ''' <summary>
        ''' All points fall outside the specified central sigma zone and observations
        ''' occur on both sides of the centre line.
        ''' </summary>
        AllBeyondSigmaOnBothSides = 6
    End Enum

    ''' <summary>
    ''' Specifies which side of the centre line a rule evaluates.
    ''' </summary>
    Public Enum SpcRuleSide
        ''' <summary>Evaluate upper and lower patterns independently.</summary>
        EitherSide = 0

        UpperSideOnly = 1
        LowerSideOnly = 2
    End Enum

    ''' <summary>
    ''' Identifies the panel families to which a rule may be applied.
    ''' </summary>
    <Flags>
    Public Enum SpcRuleScope
        None = 0
        LocationPanels = 1
        DispersionPanels = 2
        AttributePanels = 4
        TimeWeightedPanels = 8
        RareEventPanels = 16
        MultivariatePanels = 32

        LocationAndAttributePanels = 5
        AllShewhartPanels = 7
        All = 63
    End Enum

    ''' <summary>
    ''' Specifies the phases in which special-cause rules are evaluated.
    ''' </summary>
    <Flags>
    Public Enum SpcRulePhaseScope
        None = 0
        PhaseI = 1
        PhaseII = 2
        All = 3
    End Enum

    ''' <summary>
    ''' Specifies whether a missing or rule-excluded point interrupts a sequence.
    ''' </summary>
    Public Enum SpcSequenceGapBehavior
        BreakSequence = 0
        SkipPointAndContinue = 1
    End Enum

    ''' <summary>
    ''' Specifies which observations are marked when a sequence rule is triggered.
    ''' </summary>
    Public Enum SpcSignalMarkingMode
        ''' <summary>Mark only the point at which the rule becomes satisfied.</summary>
        TerminalPointOnly = 0

        ''' <summary>Mark every contributing point in the detected pattern.</summary>
        EntirePattern = 1
    End Enum

    ''' <summary>
    ''' Specifies which calculations ignore an explicitly excluded chart point.
    ''' </summary>
    ''' <remarks>
    ''' Exclusion never removes the point from the audit trail or renderer. It only
    ''' controls participation in parameter estimation and rule sequences.
    ''' </remarks>
    <Flags>
    Public Enum SpcExclusionScope
        None = 0
        ParameterEstimation = 1
        RuleEvaluation = 2
        EstimationAndRules = 3
    End Enum

    ''' <summary>
    ''' Defines one contiguous process stage in the ordered chart-point sequence.
    ''' </summary>
    Public NotInheritable Class SpcStageDefinition
        Private ReadOnly _stageId As String
        Private ReadOnly _displayName As String
        Private ReadOnly _firstPointIndex As Integer
        Private ReadOnly _lastPointIndex As Integer
        Private ReadOnly _phase As SpcPhase
        Private ReadOnly _limitMode As SpcStageLimitMode
        Private ReadOnly _referenceStageId As String

        ''' <summary>
        ''' Initializes an immutable stage definition.
        ''' </summary>
        ''' <param name="stageId">Stable, non-empty identifier unique within the request.</param>
        ''' <param name="firstPointIndex">Zero-based index of the first ordered chart point.</param>
        ''' <param name="lastPointIndex">Zero-based, inclusive index of the last ordered chart point.</param>
        ''' <param name="phase">Phase-I or Phase-II role of the stage.</param>
        ''' <param name="limitMode">How the stage obtains its centre line and limits.</param>
        ''' <param name="referenceStageId">
        ''' Required when <paramref name="limitMode"/> is <see cref="SpcStageLimitMode.UseReferenceStage"/>.
        ''' </param>
        ''' <param name="displayName">Optional user-facing label; the stage ID is used when omitted.</param>
        Public Sub New(stageId As String,
                       firstPointIndex As Integer,
                       lastPointIndex As Integer,
                       phase As SpcPhase,
                       limitMode As SpcStageLimitMode,
                       Optional referenceStageId As String = Nothing,
                       Optional displayName As String = Nothing)

            Dim normalizedStageId As String = If(stageId, String.Empty).Trim()
            If normalizedStageId.Length = 0 Then
                Throw New ArgumentException("A stage ID is required.", NameOf(stageId))
            End If
            If firstPointIndex < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(firstPointIndex),
                                                      "The first point index must be zero or greater.")
            End If
            If lastPointIndex < firstPointIndex Then
                Throw New ArgumentOutOfRangeException(NameOf(lastPointIndex),
                                                      "The last point index must not precede the first point index.")
            End If
            If Not [Enum].IsDefined(GetType(SpcPhase), phase) Then
                Throw New ArgumentOutOfRangeException(NameOf(phase))
            End If
            If Not [Enum].IsDefined(GetType(SpcStageLimitMode), limitMode) Then
                Throw New ArgumentOutOfRangeException(NameOf(limitMode))
            End If

            Dim normalizedReferenceId As String = If(referenceStageId, String.Empty).Trim()
            If limitMode = SpcStageLimitMode.UseReferenceStage Then
                If normalizedReferenceId.Length = 0 Then
                    Throw New ArgumentException(
                        "A reference stage ID is required when reference-stage limits are selected.",
                        NameOf(referenceStageId))
                End If
                If String.Equals(normalizedReferenceId,
                                 normalizedStageId,
                                 StringComparison.OrdinalIgnoreCase) Then
                    Throw New ArgumentException("A stage cannot reference itself.", NameOf(referenceStageId))
                End If
            ElseIf normalizedReferenceId.Length > 0 Then
                Throw New ArgumentException(
                    "A reference stage ID is only valid when reference-stage limits are selected.",
                    NameOf(referenceStageId))
            End If

            Dim normalizedDisplayName As String = If(displayName, String.Empty).Trim()
            If normalizedDisplayName.Length = 0 Then
                normalizedDisplayName = normalizedStageId
            End If

            _stageId = normalizedStageId
            _displayName = normalizedDisplayName
            _firstPointIndex = firstPointIndex
            _lastPointIndex = lastPointIndex
            _phase = phase
            _limitMode = limitMode
            _referenceStageId = normalizedReferenceId
        End Sub

        Public ReadOnly Property StageId As String
            Get
                Return _stageId
            End Get
        End Property

        Public ReadOnly Property DisplayName As String
            Get
                Return _displayName
            End Get
        End Property

        Public ReadOnly Property FirstPointIndex As Integer
            Get
                Return _firstPointIndex
            End Get
        End Property

        Public ReadOnly Property LastPointIndex As Integer
            Get
                Return _lastPointIndex
            End Get
        End Property

        Public ReadOnly Property PointCount As Integer
            Get
                Return _lastPointIndex - _firstPointIndex + 1
            End Get
        End Property

        Public ReadOnly Property Phase As SpcPhase
            Get
                Return _phase
            End Get
        End Property

        Public ReadOnly Property LimitMode As SpcStageLimitMode
            Get
                Return _limitMode
            End Get
        End Property

        Public ReadOnly Property ReferenceStageId As String
            Get
                Return _referenceStageId
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Defines an explicit, auditable exclusion for one ordered chart point.
    ''' </summary>
    Public NotInheritable Class SpcExclusionDefinition
        Private ReadOnly _pointIndex As Integer
        Private ReadOnly _scope As SpcExclusionScope
        Private ReadOnly _reason As String

        Public Sub New(pointIndex As Integer,
                       Optional scope As SpcExclusionScope = SpcExclusionScope.EstimationAndRules,
                       Optional reason As String = Nothing)

            If pointIndex < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(pointIndex),
                                                      "The point index must be zero or greater.")
            End If

            Dim numericScope As Integer = CInt(scope)
            If numericScope <= 0 OrElse
               (numericScope And Not CInt(SpcExclusionScope.EstimationAndRules)) <> 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(scope),
                                                      "At least one valid exclusion scope is required.")
            End If

            _pointIndex = pointIndex
            _scope = scope
            _reason = If(reason, String.Empty).Trim()
        End Sub

        Public ReadOnly Property PointIndex As Integer
            Get
                Return _pointIndex
            End Get
        End Property

        Public ReadOnly Property Scope As SpcExclusionScope
            Get
                Return _scope
            End Get
        End Property

        Public ReadOnly Property Reason As String
            Get
                Return _reason
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Defines one standardized special-cause rule.
    ''' </summary>
    ''' <remarks>
    ''' Rules operate on standardized distance from the point-specific centre line.
    ''' Consequently the same definition can be used for fixed-limit charts and for
    ''' p or u charts whose limits vary with subgroup size or exposure.
    ''' </remarks>
    Public NotInheritable Class SpcRuleDefinition
        Private ReadOnly _ruleCode As String
        Private ReadOnly _ruleNumber As Integer
        Private ReadOnly _kind As SpcRuleKind
        Private ReadOnly _windowSize As Integer
        Private ReadOnly _minimumPoints As Integer
        Private ReadOnly _sigmaThreshold As Double
        Private ReadOnly _side As SpcRuleSide
        Private ReadOnly _scope As SpcRuleScope
        Private ReadOnly _displayName As String
        Private ReadOnly _description As String

        ''' <summary>
        ''' Initializes an immutable rule definition.
        ''' </summary>
        ''' <param name="ruleCode">Stable code such as N1 or WE2.</param>
        ''' <param name="ruleNumber">Positive number displayed in signal output.</param>
        ''' <param name="kind">Sequence pattern evaluated by the rule.</param>
        ''' <param name="windowSize">Number of consecutive eligible points examined.</param>
        ''' <param name="minimumPoints">Minimum qualifying points required within the window.</param>
        ''' <param name="sigmaThreshold">Nonnegative standardized zone boundary used by the rule.</param>
        ''' <param name="side">Upper, lower, or either-side evaluation.</param>
        ''' <param name="scope">Panel families to which the rule may be applied.</param>
        ''' <param name="displayName">Optional user-facing name.</param>
        ''' <param name="description">Optional plain-language explanation.</param>
        Public Sub New(ruleCode As String,
                       ruleNumber As Integer,
                       kind As SpcRuleKind,
                       windowSize As Integer,
                       minimumPoints As Integer,
                       sigmaThreshold As Double,
                       Optional side As SpcRuleSide = SpcRuleSide.EitherSide,
                       Optional scope As SpcRuleScope = SpcRuleScope.LocationAndAttributePanels,
                       Optional displayName As String = Nothing,
                       Optional description As String = Nothing)

            Dim normalizedCode As String = If(ruleCode, String.Empty).Trim()
            If normalizedCode.Length = 0 Then
                Throw New ArgumentException("A rule code is required.", NameOf(ruleCode))
            End If
            If ruleNumber <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(ruleNumber),
                                                      "The rule number must be positive.")
            End If
            If Not [Enum].IsDefined(GetType(SpcRuleKind), kind) Then
                Throw New ArgumentOutOfRangeException(NameOf(kind))
            End If
            If windowSize <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(windowSize),
                                                      "The rule window must contain at least one point.")
            End If
            If minimumPoints <= 0 OrElse minimumPoints > windowSize Then
                Throw New ArgumentOutOfRangeException(NameOf(minimumPoints),
                                                      "Minimum points must be between one and the window size.")
            End If
            If Double.IsNaN(sigmaThreshold) OrElse
               Double.IsInfinity(sigmaThreshold) OrElse
               sigmaThreshold < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(sigmaThreshold),
                                                      "The sigma threshold must be finite and nonnegative.")
            End If
            If Not [Enum].IsDefined(GetType(SpcRuleSide), side) Then
                Throw New ArgumentOutOfRangeException(NameOf(side))
            End If

            Dim numericScope As Integer = CInt(scope)
            If numericScope <= 0 OrElse
               (numericScope And Not CInt(SpcRuleScope.All)) <> 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(scope),
                                                      "At least one valid rule scope is required.")
            End If

            Dim normalizedDisplayName As String = If(displayName, String.Empty).Trim()
            If normalizedDisplayName.Length = 0 Then
                normalizedDisplayName = normalizedCode
            End If

            _ruleCode = normalizedCode
            _ruleNumber = ruleNumber
            _kind = kind
            _windowSize = windowSize
            _minimumPoints = minimumPoints
            _sigmaThreshold = sigmaThreshold
            _side = side
            _scope = scope
            _displayName = normalizedDisplayName
            _description = If(description, String.Empty).Trim()
        End Sub

        Public ReadOnly Property RuleCode As String
            Get
                Return _ruleCode
            End Get
        End Property

        Public ReadOnly Property RuleNumber As Integer
            Get
                Return _ruleNumber
            End Get
        End Property

        Public ReadOnly Property Kind As SpcRuleKind
            Get
                Return _kind
            End Get
        End Property

        Public ReadOnly Property WindowSize As Integer
            Get
                Return _windowSize
            End Get
        End Property

        Public ReadOnly Property MinimumPoints As Integer
            Get
                Return _minimumPoints
            End Get
        End Property

        Public ReadOnly Property SigmaThreshold As Double
            Get
                Return _sigmaThreshold
            End Get
        End Property

        Public ReadOnly Property Side As SpcRuleSide
            Get
                Return _side
            End Get
        End Property

        Public ReadOnly Property Scope As SpcRuleScope
            Get
                Return _scope
            End Get
        End Property

        Public ReadOnly Property DisplayName As String
            Get
                Return _displayName
            End Get
        End Property

        Public ReadOnly Property Description As String
            Get
                Return _description
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Configures centre-line and control-limit calculation.
    ''' </summary>
    Public NotInheritable Class SpcControlLimitOptions
        Public Property ParameterSource As SpcParameterSource = SpcParameterSource.EstimateFromPhaseI
        Public Property Method As SpcControlLimitMethod = SpcControlLimitMethod.ShewhartSigma
        Public Property SigmaMultiplier As Double = 3.0
        Public Property WithinSigmaEstimator As SpcWithinSigmaEstimator = SpcWithinSigmaEstimator.Automatic
        Public Property NaturalLimitPolicy As SpcNaturalLimitPolicy = SpcNaturalLimitPolicy.ClipToFeasibleRange
        Public Property MovingRangeLength As Integer = 2
        Public Property UseBiasCorrection As Boolean = True

        ''' <summary>
        ''' Creates an independent snapshot of these options.
        ''' </summary>
        Friend Function Copy() As SpcControlLimitOptions
            Return New SpcControlLimitOptions With {
                .ParameterSource = ParameterSource,
                .Method = Method,
                .SigmaMultiplier = SigmaMultiplier,
                .WithinSigmaEstimator = WithinSigmaEstimator,
                .NaturalLimitPolicy = NaturalLimitPolicy,
                .MovingRangeLength = MovingRangeLength,
                .UseBiasCorrection = UseBiasCorrection
            }
        End Function
    End Class

    ''' <summary>
    ''' Configures special-cause rule selection and sequence handling.
    ''' </summary>
    Public NotInheritable Class SpcRuleOptions
        Public Property Preset As SpcRulePreset = SpcRulePreset.RuleOneOnly
        Public Property CustomRules As SpcRuleDefinition() = Array.Empty(Of SpcRuleDefinition)()
        Public Property PhaseScope As SpcRulePhaseScope = SpcRulePhaseScope.All
        Public Property GapBehavior As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence
        Public Property MarkingMode As SpcSignalMarkingMode = SpcSignalMarkingMode.TerminalPointOnly

        ''' <summary>
        ''' Creates an independent snapshot of these options.
        ''' </summary>
        Friend Function Copy() As SpcRuleOptions
            Dim copiedRules As SpcRuleDefinition()
            If CustomRules Is Nothing Then
                copiedRules = Array.Empty(Of SpcRuleDefinition)()
            Else
                copiedRules = CType(CustomRules.Clone(), SpcRuleDefinition())
            End If

            Return New SpcRuleOptions With {
                .Preset = Preset,
                .CustomRules = copiedRules,
                .PhaseScope = PhaseScope,
                .GapBehavior = GapBehavior,
                .MarkingMode = MarkingMode
            }
        End Function
    End Class

    ''' <summary>
    ''' Collects host-neutral calculation options shared by all SPC chart requests.
    ''' </summary>
    ''' <remarks>
    ''' Data arrays, labels, historical parameters, and chart-specific parameters
    ''' belong to <c>SpcFitRequest</c> in SpcModels.vb. This object contains only
    ''' cross-cutting behaviour that can be shared by the GUI, UDFs, tests, and a
    ''' future Office.js host.
    ''' </remarks>
    Public NotInheritable Class SpcAnalysisOptions
        Public Property MissingValuePolicy As SpcMissingValuePolicy =
            SpcMissingValuePolicy.Reject

        Public Property ControlLimits As SpcControlLimitOptions = New SpcControlLimitOptions()
        Public Property Rules As SpcRuleOptions = New SpcRuleOptions()
        Public Property Stages As SpcStageDefinition() = Array.Empty(Of SpcStageDefinition)()
        Public Property Exclusions As SpcExclusionDefinition() = Array.Empty(Of SpcExclusionDefinition)()

        ''' <summary>
        ''' Creates an independent snapshot of the option graph.
        ''' </summary>
        Friend Function Copy() As SpcAnalysisOptions
            Dim copiedLimits As SpcControlLimitOptions =
                If(ControlLimits Is Nothing, New SpcControlLimitOptions(), ControlLimits.Copy())

            Dim copiedRuleOptions As SpcRuleOptions =
                If(Rules Is Nothing, New SpcRuleOptions(), Rules.Copy())

            Dim copiedStages As SpcStageDefinition()
            If Stages Is Nothing Then
                copiedStages = Array.Empty(Of SpcStageDefinition)()
            Else
                copiedStages = CType(Stages.Clone(), SpcStageDefinition())
            End If

            Dim copiedExclusions As SpcExclusionDefinition()
            If Exclusions Is Nothing Then
                copiedExclusions = Array.Empty(Of SpcExclusionDefinition)()
            Else
                copiedExclusions = CType(Exclusions.Clone(), SpcExclusionDefinition())
            End If

            Return New SpcAnalysisOptions With {
                .MissingValuePolicy = MissingValuePolicy,
                .ControlLimits = copiedLimits,
                .Rules = copiedRuleOptions,
                .Stages = copiedStages,
                .Exclusions = copiedExclusions
            }
        End Function
    End Class

End Namespace
