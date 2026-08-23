Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Provides immutable definitions for the named special-cause rule presets.
    ''' </summary>
    Public NotInheritable Class SpcRuleCatalog

        Private Shared ReadOnly RuleOneRules As SpcRuleDefinition() =
            CreateRuleOnePreset()

        Private Shared ReadOnly WesternElectricRules As SpcRuleDefinition() =
            CreateWesternElectricPreset()

        Private Shared ReadOnly NelsonRules As SpcRuleDefinition() =
            CreateNelsonPreset()

        Private Shared ReadOnly PaperMontgomeryRules As SpcRuleDefinition() =
            CreatePaperMontgomeryPreset()

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Returns the immutable definitions belonging to a named preset.
        ''' </summary>
        ''' <remarks>
        ''' Custom rules are supplied through <see cref="SpcRuleOptions.CustomRules"/>
        ''' and therefore cannot be resolved by this overload.
        ''' </remarks>
        Public Shared Function GetRules(preset As SpcRulePreset) As SpcRuleDefinition()
            If Not [Enum].IsDefined(GetType(SpcRulePreset), preset) Then
                Throw New ArgumentOutOfRangeException(NameOf(preset))
            End If

            Select Case preset
                Case SpcRulePreset.None
                    Return Array.Empty(Of SpcRuleDefinition)()

                Case SpcRulePreset.RuleOneOnly
                    Return CopyDefinitions(RuleOneRules)

                Case SpcRulePreset.WesternElectric
                    Return CopyDefinitions(WesternElectricRules)

                Case SpcRulePreset.Nelson
                    Return CopyDefinitions(NelsonRules)

                Case SpcRulePreset.PaperMontgomeryEightRules
                    Return CopyDefinitions(PaperMontgomeryRules)

                Case SpcRulePreset.Custom
                    Throw New ArgumentException(
                        "Custom rules must be obtained from SpcRuleOptions.CustomRules.",
                        NameOf(preset))

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(preset))
            End Select
        End Function

        ''' <summary>
        ''' Resolves and validates the effective definitions selected by rule options.
        ''' </summary>
        Public Shared Function ResolveRules(options As SpcRuleOptions) As SpcRuleDefinition()
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))

            ValidateRuleOptions(options)

            Dim rules As SpcRuleDefinition()
            If options.Preset = SpcRulePreset.Custom Then
                rules = CType(options.CustomRules.Clone(), SpcRuleDefinition())
            Else
                rules = GetRules(options.Preset)
            End If

            ValidateDefinitions(rules)
            Return rules
        End Function

        ''' <summary>Returns a short user-facing description of a named preset.</summary>
        Public Shared Function GetPresetDescription(preset As SpcRulePreset) As String
            If Not [Enum].IsDefined(GetType(SpcRulePreset), preset) Then
                Throw New ArgumentOutOfRangeException(NameOf(preset))
            End If

            Select Case preset
                Case SpcRulePreset.None
                    Return "Do not evaluate special-cause sequence rules."
                Case SpcRulePreset.RuleOneOnly
                    Return "Signal a point more than three standard errors from the centre line."
                Case SpcRulePreset.WesternElectric
                    Return "Evaluate the four Western Electric zone and run rules."
                Case SpcRulePreset.Nelson
                    Return "Evaluate the eight Nelson special-cause rules."
                Case SpcRulePreset.PaperMontgomeryEightRules
                    Return "Evaluate the eight rules used by the paper and Montgomery reference."
                Case SpcRulePreset.Custom
                    Return "Evaluate the supplied custom rule definitions."
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(preset))
            End Select
        End Function

        Friend Shared Sub ValidateRuleOptions(options As SpcRuleOptions)
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            If Not [Enum].IsDefined(GetType(SpcRulePreset), options.Preset) Then
                Throw New ArgumentOutOfRangeException("RulePreset")
            End If
            ValidatePhaseScope(options.PhaseScope)
            If Not [Enum].IsDefined(GetType(SpcSequenceGapBehavior), options.GapBehavior) Then
                Throw New ArgumentOutOfRangeException("SequenceGapBehavior")
            End If
            If Not [Enum].IsDefined(GetType(SpcSignalMarkingMode), options.MarkingMode) Then
                Throw New ArgumentOutOfRangeException("SignalMarkingMode")
            End If

            If options.Preset = SpcRulePreset.Custom AndAlso
               (options.CustomRules Is Nothing OrElse options.CustomRules.Length = 0) Then
                Throw New ArgumentException(
                    "At least one custom rule is required when the custom preset is selected.",
                    NameOf(options))
            End If
        End Sub

        Private Shared Sub ValidatePhaseScope(scope As SpcRulePhaseScope)
            Dim numericScope As Integer = CInt(scope)
            If numericScope < 0 OrElse
               (numericScope And Not CInt(SpcRulePhaseScope.All)) <> 0 Then
                Throw New ArgumentOutOfRangeException("RulePhaseScope")
            End If
        End Sub

        Private Shared Sub ValidateDefinitions(rules As SpcRuleDefinition())
            If rules Is Nothing Then Throw New ArgumentNullException(NameOf(rules))

            Dim codes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim numbers As New HashSet(Of Integer)()

            For i As Integer = 0 To rules.Length - 1
                Dim rule As SpcRuleDefinition = rules(i)
                If rule Is Nothing Then
                    Throw New ArgumentException(
                        "Rule definitions must not contain null entries.", NameOf(rules))
                End If
                If Not codes.Add(rule.RuleCode) Then
                    Throw New ArgumentException(
                        "Rule codes must be unique, ignoring case.", NameOf(rules))
                End If
                If Not numbers.Add(rule.RuleNumber) Then
                    Throw New ArgumentException(
                        "Rule numbers must be unique within the selected rule set.", NameOf(rules))
                End If

                ValidateDefinitionShape(rule)
            Next
        End Sub

        Private Shared Sub ValidateDefinitionShape(rule As SpcRuleDefinition)
            Select Case rule.Kind
                Case SpcRuleKind.BeyondSigma,
                     SpcRuleKind.KOfMConsecutiveBeyondSigma
                    ' The constructor already validates the K-of-M dimensions.

                Case SpcRuleKind.RunOnOneSide,
                     SpcRuleKind.AllWithinSigma,
                     SpcRuleKind.AllBeyondSigmaOnBothSides
                    RequireWholeWindow(rule)

                Case SpcRuleKind.MonotonicTrend
                    RequireWholeWindow(rule)
                    If rule.WindowSize < 2 Then
                        Throw New ArgumentException(
                            "A monotonic-trend rule requires at least two points.")
                    End If

                Case SpcRuleKind.Alternating
                    RequireWholeWindow(rule)
                    If rule.WindowSize < 3 Then
                        Throw New ArgumentException(
                            "An alternating rule requires at least three points.")
                    End If

                Case Else
                    Throw New ArgumentOutOfRangeException("RuleKind")
            End Select

            If (rule.Kind = SpcRuleKind.Alternating OrElse
                rule.Kind = SpcRuleKind.AllWithinSigma OrElse
                rule.Kind = SpcRuleKind.AllBeyondSigmaOnBothSides) AndAlso
               rule.Side <> SpcRuleSide.EitherSide Then
                Throw New ArgumentException(
                    "Alternating, central-zone, and both-side mixture rules require EitherSide.")
            End If
        End Sub

        Private Shared Sub RequireWholeWindow(rule As SpcRuleDefinition)
            If rule.MinimumPoints <> rule.WindowSize Then
                Throw New ArgumentException(
                    rule.RuleCode & " requires MinimumPoints to equal WindowSize.")
            End If
        End Sub

        Private Shared Function CreateRuleOnePreset() As SpcRuleDefinition()
            Return {
                CreateRuleOne("R1")
            }
        End Function

        Private Shared Function CreateWesternElectricPreset() As SpcRuleDefinition()
            Return {
                CreateRuleOne("WE1"),
                New SpcRuleDefinition(
                    "WE2", 2, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    3, 2, 2.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Two of three beyond 2 sigma",
                    description:="Two of three consecutive points are beyond two sigma on the same side."),
                New SpcRuleDefinition(
                    "WE3", 3, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    5, 4, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Four of five beyond 1 sigma",
                    description:="Four of five consecutive points are beyond one sigma on the same side."),
                New SpcRuleDefinition(
                    "WE4", 4, SpcRuleKind.RunOnOneSide,
                    8, 8, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Eight on one side",
                    description:="Eight consecutive points are on the same side of the centre line.")
            }
        End Function

        Private Shared Function CreateNelsonPreset() As SpcRuleDefinition()
            Return {
                CreateRuleOne("N1"),
                New SpcRuleDefinition(
                    "N2", 2, SpcRuleKind.RunOnOneSide,
                    9, 9, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Nine on one side",
                    description:="Nine consecutive points are on the same side of the centre line."),
                New SpcRuleDefinition(
                    "N3", 3, SpcRuleKind.MonotonicTrend,
                    6, 6, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Six-point trend",
                    description:="Six consecutive points are steadily increasing or decreasing."),
                New SpcRuleDefinition(
                    "N4", 4, SpcRuleKind.Alternating,
                    14, 14, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Fourteen alternating",
                    description:="Fourteen consecutive points alternate in direction."),
                New SpcRuleDefinition(
                    "N5", 5, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    3, 2, 2.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Two of three beyond 2 sigma",
                    description:="Two of three consecutive points are beyond two sigma on the same side."),
                New SpcRuleDefinition(
                    "N6", 6, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    5, 4, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Four of five beyond 1 sigma",
                    description:="Four of five consecutive points are beyond one sigma on the same side."),
                New SpcRuleDefinition(
                    "N7", 7, SpcRuleKind.AllWithinSigma,
                    15, 15, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Fifteen within 1 sigma",
                    description:="Fifteen consecutive points are within one sigma of the centre line."),
                New SpcRuleDefinition(
                    "N8", 8, SpcRuleKind.AllBeyondSigmaOnBothSides,
                    8, 8, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Eight outside 1 sigma",
                    description:="Eight consecutive points are outside one sigma with points on both sides.")
            }
        End Function

        Private Shared Function CreatePaperMontgomeryPreset() As SpcRuleDefinition()
            Return {
                CreateRuleOne("M1"),
                New SpcRuleDefinition(
                    "M2", 2, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    3, 2, 2.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Two of three beyond 2 sigma",
                    description:="Two of three consecutive points are beyond two sigma in the same direction."),
                New SpcRuleDefinition(
                    "M3", 3, SpcRuleKind.KOfMConsecutiveBeyondSigma,
                    5, 4, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Four of five beyond 1 sigma",
                    description:="Four of five consecutive points are beyond one sigma in the same direction."),
                New SpcRuleDefinition(
                    "M4", 4, SpcRuleKind.RunOnOneSide,
                    8, 8, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Eight on one side",
                    description:="Eight consecutive points are on the same side of the centre line."),
                New SpcRuleDefinition(
                    "M5", 5, SpcRuleKind.MonotonicTrend,
                    6, 6, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Six-point trend",
                    description:="Six consecutive points steadily increase or decrease."),
                New SpcRuleDefinition(
                    "M6", 6, SpcRuleKind.AllWithinSigma,
                    15, 15, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Fifteen within 1 sigma",
                    description:="Fifteen consecutive points remain within one sigma of the centre line."),
                New SpcRuleDefinition(
                    "M7", 7, SpcRuleKind.Alternating,
                    14, 14, 0.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Fourteen alternating",
                    description:="Fourteen consecutive points alternate up and down."),
                New SpcRuleDefinition(
                    "M8", 8, SpcRuleKind.AllBeyondSigmaOnBothSides,
                    8, 8, 1.0,
                    scope:=SpcRuleScope.LocationAndAttributePanels,
                    displayName:="Eight outside 1 sigma",
                    description:="Eight consecutive points are outside one sigma with points on both sides.")
            }
        End Function

        Private Shared Function CreateRuleOne(ruleCode As String) As SpcRuleDefinition
            Return New SpcRuleDefinition(
                ruleCode, 1, SpcRuleKind.BeyondSigma,
                1, 1, 3.0,
                scope:=SpcRuleScope.AllShewhartPanels,
                displayName:="One point beyond 3 sigma",
                description:="A point is more than three standard errors from the centre line.")
        End Function

        Private Shared Function CopyDefinitions(values As SpcRuleDefinition()) As SpcRuleDefinition()
            Return CType(values.Clone(), SpcRuleDefinition())
        End Function
    End Class

    ''' <summary>
    ''' Immutable output from applying a special-cause rule set to chart panels.
    ''' </summary>
    Public NotInheritable Class SpcRuleEvaluationResult
        Private ReadOnly _panels As SpcPanelResult()
        Private ReadOnly _rules As SpcRuleDefinition()
        Private ReadOnly _signals As SpcSignalResult()
        Private ReadOnly _warnings As String()

        Friend Sub New(panels As SpcPanelResult(),
                       rules As SpcRuleDefinition(),
                       warnings As String())
            If panels Is Nothing OrElse panels.Length = 0 Then
                Throw New ArgumentException(
                    "At least one panel is required.", NameOf(panels))
            End If

            _panels = CType(panels.Clone(), SpcPanelResult())
            For i As Integer = 0 To _panels.Length - 1
                If _panels(i) Is Nothing Then
                    Throw New ArgumentException(
                        "Evaluated panels must not contain null entries.", NameOf(panels))
                End If
            Next

            If rules Is Nothing Then
                _rules = Array.Empty(Of SpcRuleDefinition)()
            Else
                _rules = CType(rules.Clone(), SpcRuleDefinition())
            End If
            _warnings = CopyMessages(warnings)
            _signals = FlattenSignals(_panels)
        End Sub

        Public ReadOnly Property Panels As SpcPanelResult()
            Get
                Return CType(_panels.Clone(), SpcPanelResult())
            End Get
        End Property

        Public ReadOnly Property Rules As SpcRuleDefinition()
            Get
                Return CType(_rules.Clone(), SpcRuleDefinition())
            End Get
        End Property

        Public ReadOnly Property Signals As SpcSignalResult()
            Get
                Return CType(_signals.Clone(), SpcSignalResult())
            End Get
        End Property

        Public ReadOnly Property Warnings As String()
            Get
                Return CType(_warnings.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property SignalCount As Integer
            Get
                Return _signals.Length
            End Get
        End Property

        Private Shared Function FlattenSignals(panels As SpcPanelResult()) As SpcSignalResult()
            Dim values As New List(Of SpcSignalResult)()
            For i As Integer = 0 To panels.Length - 1
                values.AddRange(panels(i).Signals)
            Next
            values.Sort(AddressOf SpcRuleEvaluator.CompareSignals)
            Return values.ToArray()
        End Function

        Private Shared Function CopyMessages(messages As String()) As String()
            If messages Is Nothing OrElse messages.Length = 0 Then
                Return Array.Empty(Of String)()
            End If

            Dim result As New List(Of String)()
            For i As Integer = 0 To messages.Length - 1
                Dim message As String = If(messages(i), String.Empty).Trim()
                If message.Length > 0 AndAlso Not result.Contains(message) Then
                    result.Add(message)
                End If
            Next
            Return result.ToArray()
        End Function
    End Class

    ''' <summary>
    ''' Evaluates standardized special-cause rules and rebuilds immutable panel results.
    ''' </summary>
    ''' <remarks>
    ''' Sequences always reset at stage boundaries. Missing or explicitly rule-excluded
    ''' points either break a sequence or are skipped according to
    ''' <see cref="SpcRuleOptions.GapBehavior"/>. Existing signals are preserved, making
    ''' the evaluator suitable for later chart calculators that generate native signals.
    ''' </remarks>
    Public NotInheritable Class SpcRuleEvaluator

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Applies the selected rules to all applicable panels.
        ''' </summary>
        Public Shared Function Evaluate(
            panels As SpcPanelResult(),
            options As SpcRuleOptions,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcRuleEvaluationResult

            If panels Is Nothing OrElse panels.Length = 0 Then
                Throw New ArgumentException(
                    "At least one panel is required for rule evaluation.", NameOf(panels))
            End If
            SpcRuleCatalog.ValidateRuleOptions(options)

            Dim rules As SpcRuleDefinition() = SpcRuleCatalog.ResolveRules(options)
            Dim evaluatedPanels(panels.Length - 1) As SpcPanelResult
            Dim warnings As New List(Of String)()
            Dim anyApplicableRule As Boolean = False

            If rules.Length > 0 AndAlso options.PhaseScope = SpcRulePhaseScope.None Then
                warnings.Add(
                    "Special-cause rule evaluation was disabled because no phase was selected.")
            End If

            For panelIndex As Integer = 0 To panels.Length - 1
                CheckCancellationPeriodically(panelIndex, cancellationRequested)

                Dim panel As SpcPanelResult = panels(panelIndex)
                If panel Is Nothing Then
                    Throw New ArgumentException(
                        "Panels must not contain null entries.", NameOf(panels))
                End If

                Dim applicableRules As SpcRuleDefinition() =
                    SelectApplicableRules(rules, panel.PanelType)
                If applicableRules.Length > 0 Then anyApplicableRule = True

                If applicableRules.Length = 0 OrElse
                   options.PhaseScope = SpcRulePhaseScope.None Then
                    evaluatedPanels(panelIndex) = panel
                Else
                    evaluatedPanels(panelIndex) = EvaluatePanel(
                        panel,
                        applicableRules,
                        options,
                        cancellationRequested)
                End If
            Next

            If rules.Length > 0 AndAlso Not anyApplicableRule Then
                warnings.Add(
                    "No selected special-cause rule applies to the calculated panel types.")
            End If

            ThrowIfCancellationRequested(cancellationRequested)
            Return New SpcRuleEvaluationResult(
                evaluatedPanels,
                rules,
                warnings.ToArray())
        End Function

        ''' <summary>
        ''' Applies rules to a calculator payload and preserves its existing warnings.
        ''' </summary>
        ''' <remarks>
        ''' This helper is intended for the orchestration layer after a chart calculator
        ''' has constructed point values and standardized distances.
        ''' </remarks>
        Public Shared Function EvaluateCalculation(
            calculation As SpcCalculationResult,
            options As SpcRuleOptions,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcCalculationResult

            If calculation Is Nothing Then Throw New ArgumentNullException(NameOf(calculation))

            Dim evaluated As SpcRuleEvaluationResult =
                Evaluate(calculation.Panels, options, cancellationRequested)
            Dim warnings As New List(Of String)(calculation.Warnings)
            AddUniqueMessages(warnings, evaluated.Warnings)

            Return New SpcCalculationResult(evaluated.Panels, warnings.ToArray())
        End Function

        ''' <summary>Maps a result panel to its rule-applicability family.</summary>
        Public Shared Function GetPanelRuleScope(panelType As SpcPanelType) As SpcRuleScope
            If Not [Enum].IsDefined(GetType(SpcPanelType), panelType) Then
                Throw New ArgumentOutOfRangeException(NameOf(panelType))
            End If

            Select Case panelType
                Case SpcPanelType.Run,
                     SpcPanelType.IndividualValue,
                     SpcPanelType.SubgroupMean,
                     SpcPanelType.StandardizedValue,
                     SpcPanelType.Residual,
                     SpcPanelType.ProfileStatistic,
                     SpcPanelType.RiskAdjustedStatistic
                    Return SpcRuleScope.LocationPanels

                Case SpcPanelType.MovingRange,
                     SpcPanelType.SubgroupRange,
                     SpcPanelType.SubgroupStandardDeviation
                    Return SpcRuleScope.DispersionPanels

                Case SpcPanelType.Proportion,
                     SpcPanelType.NumberNonconforming,
                     SpcPanelType.DefectCount,
                     SpcPanelType.DefectRate,
                     SpcPanelType.StandardizedProportion,
                     SpcPanelType.StandardizedDefectRate
                    Return SpcRuleScope.AttributePanels

                Case SpcPanelType.UpperCusum,
                     SpcPanelType.LowerCusum,
                     SpcPanelType.Ewma,
                     SpcPanelType.MovingAverage
                    Return SpcRuleScope.TimeWeightedPanels

                Case SpcPanelType.EventsBetweenOccurrences,
                     SpcPanelType.TimeBetweenOccurrences
                    Return SpcRuleScope.RareEventPanels

                Case SpcPanelType.HotellingT2,
                     SpcPanelType.GeneralizedVariance,
                     SpcPanelType.PcaT2,
                     SpcPanelType.PcaQ,
                     SpcPanelType.Mewma,
                     SpcPanelType.Mcusum
                    Return SpcRuleScope.MultivariatePanels

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(panelType))
            End Select
        End Function

        Friend Shared Function CompareSignals(left As SpcSignalResult,
                                              right As SpcSignalResult) As Integer
            If left Is Nothing Then Return If(right Is Nothing, 0, -1)
            If right Is Nothing Then Return 1

            Dim comparison As Integer = CInt(left.PanelType).CompareTo(CInt(right.PanelType))
            If comparison <> 0 Then Return comparison

            comparison = StringComparer.OrdinalIgnoreCase.Compare(left.StageId, right.StageId)
            If comparison <> 0 Then Return comparison

            comparison = left.TerminalPointIndex.CompareTo(
                right.TerminalPointIndex)
            If comparison <> 0 Then Return comparison

            comparison = left.RuleNumber.CompareTo(right.RuleNumber)
            If comparison <> 0 Then Return comparison

            comparison = CInt(left.TriggeredSide).CompareTo(CInt(right.TriggeredSide))
            If comparison <> 0 Then Return comparison

            comparison = left.WindowStartPointIndex.CompareTo(
                right.WindowStartPointIndex)
            If comparison <> 0 Then Return comparison

            Return StringComparer.OrdinalIgnoreCase.Compare(left.RuleCode, right.RuleCode)
        End Function

        Private Shared Function EvaluatePanel(
            panel As SpcPanelResult,
            rules As SpcRuleDefinition(),
            options As SpcRuleOptions,
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As SpcPointResult() = panel.Points
            Dim existingSignals As SpcSignalResult() = panel.Signals
            Dim newSignals As New List(Of SpcSignalResult)()

            For ruleIndex As Integer = 0 To rules.Length - 1
                CheckCancellationPeriodically(ruleIndex, cancellationRequested)
                EvaluateRuleAcrossPanel(
                    panel.PanelType,
                    points,
                    rules(ruleIndex),
                    options,
                    existingSignals,
                    newSignals,
                    cancellationRequested)
            Next

            If newSignals.Count = 0 Then Return panel

            Dim allSignals As New List(Of SpcSignalResult)(existingSignals)
            allSignals.AddRange(newSignals)
            allSignals.Sort(AddressOf CompareSignals)

            Dim ruleNumbersByPoint As New Dictionary(Of Integer, HashSet(Of Integer))()
            For i As Integer = 0 To points.Length - 1
                Dim numbers As New HashSet(Of Integer)(points(i).SignalRuleNumbers)
                ruleNumbersByPoint.Add(points(i).PointIndex, numbers)
            Next

            For i As Integer = 0 To newSignals.Count - 1
                Dim markedIndices As Integer() = newSignals(i).MarkedPointIndices
                For j As Integer = 0 To markedIndices.Length - 1
                    ruleNumbersByPoint(markedIndices(j)).Add(newSignals(i).RuleNumber)
                Next
            Next

            Dim rebuiltPoints(points.Length - 1) As SpcPointResult
            For i As Integer = 0 To points.Length - 1
                Dim numbers As Integer() =
                    New List(Of Integer)(ruleNumbersByPoint(points(i).PointIndex)).ToArray()
                Array.Sort(numbers)
                rebuiltPoints(i) = CopyPointWithSignals(points(i), numbers)
            Next

            Return New SpcPanelResult(
                panel.PanelType,
                panel.DisplayName,
                rebuiltPoints,
                panel.ValueAxisTitle,
                panel.ParameterEstimates,
                allSignals.ToArray(),
                panel.Warnings)
        End Function

        Private Shared Sub EvaluateRuleAcrossPanel(
            panelType As SpcPanelType,
            points As SpcPointResult(),
            rule As SpcRuleDefinition,
            options As SpcRuleOptions,
            existingSignals As SpcSignalResult(),
            newSignals As List(Of SpcSignalResult),
            cancellationRequested As Func(Of Boolean))

            Dim window As New List(Of SpcPointResult)(rule.WindowSize)
            Dim currentStageId As String = Nothing
            Dim previousPanelPointIndex As Integer = -1

            For pointPosition As Integer = 0 To points.Length - 1
                CheckCancellationPeriodically(pointPosition, cancellationRequested)

                Dim point As SpcPointResult = points(pointPosition)
                Dim stageChanged As Boolean =
                    currentStageId Is Nothing OrElse
                    Not String.Equals(currentStageId,
                                      point.StageId,
                                      StringComparison.OrdinalIgnoreCase)

                If stageChanged Then
                    window.Clear()
                    currentStageId = point.StageId
                    previousPanelPointIndex = -1
                ElseIf options.GapBehavior = SpcSequenceGapBehavior.BreakSequence AndAlso
                       previousPanelPointIndex >= 0 AndAlso
                       point.PointIndex <> previousPanelPointIndex + 1 Then
                    window.Clear()
                End If
                previousPanelPointIndex = point.PointIndex

                If Not PhaseIsSelected(point.Phase, options.PhaseScope) Then
                    window.Clear()
                    Continue For
                End If

                If Not IsRuleEligible(point) Then
                    If options.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                        window.Clear()
                    End If
                    Continue For
                End If

                window.Add(point)
                If window.Count > rule.WindowSize Then window.RemoveAt(0)
                If window.Count < rule.WindowSize Then Continue For

                Dim matches As RuleMatch() = EvaluateWindow(window, rule, panelType)
                For matchIndex As Integer = 0 To matches.Length - 1
                    Dim match As RuleMatch = matches(matchIndex)
                    Dim terminalPoint As SpcPointResult = window(window.Count - 1)
                    Dim contributing As Integer() = match.ContributingPointIndices
                    Dim marked As Integer()
                    If options.MarkingMode = SpcSignalMarkingMode.EntirePattern Then
                        marked = CType(contributing.Clone(), Integer())
                    Else
                        marked = {terminalPoint.PointIndex}
                    End If

                    Dim signal As New SpcSignalResult(
                        panelType,
                        terminalPoint.StageId,
                        rule,
                        terminalPoint.PointIndex,
                        window(0).PointIndex,
                        terminalPoint.PointIndex,
                        triggeredSide:=match.TriggeredSide,
                        contributingPointIndices:=contributing,
                        markedPointIndices:=marked,
                        message:=BuildSignalMessage(rule, match.TriggeredSide, terminalPoint, panelType))

                    If Not ContainsEquivalentSignal(existingSignals, signal) AndAlso
                       Not ContainsEquivalentSignal(newSignals, signal) Then
                        newSignals.Add(signal)
                    End If
                Next
            Next
        End Sub

        Private Shared Function EvaluateWindow(window As List(Of SpcPointResult),
                                               rule As SpcRuleDefinition,
                                               panelType As SpcPanelType) As RuleMatch()
            Select Case rule.Kind
                Case SpcRuleKind.BeyondSigma,
                     SpcRuleKind.KOfMConsecutiveBeyondSigma
                    Return EvaluateBeyondSigma(window, rule, panelType)

                Case SpcRuleKind.RunOnOneSide
                    Return EvaluateRun(window, rule)

                Case SpcRuleKind.MonotonicTrend
                    Return EvaluateTrend(window, rule)

                Case SpcRuleKind.Alternating
                    If IsAlternating(window) Then
                        Return {New RuleMatch(
                            SpcRuleSide.EitherSide,
                            GetAllPointIndices(window))}
                    End If

                Case SpcRuleKind.AllWithinSigma
                    If AllWithinSigma(window, rule.SigmaThreshold) Then
                        Return {New RuleMatch(
                            SpcRuleSide.EitherSide,
                            GetAllPointIndices(window))}
                    End If

                Case SpcRuleKind.AllBeyondSigmaOnBothSides
                    If AllBeyondSigmaOnBothSides(window, rule.SigmaThreshold) Then
                        Return {New RuleMatch(
                            SpcRuleSide.EitherSide,
                            GetAllPointIndices(window))}
                    End If

                Case Else
                    Throw New ArgumentOutOfRangeException("RuleKind")
            End Select

            Return Array.Empty(Of RuleMatch)()
        End Function

        Private Shared Function EvaluateBeyondSigma(window As List(Of SpcPointResult),
                                                    rule As SpcRuleDefinition,
                                                    panelType As SpcPanelType) As RuleMatch()

            Dim matches As New List(Of RuleMatch)()

            If rule.Side <> SpcRuleSide.LowerSideOnly Then
                Dim upperIndices As Integer() = GetBeyondIndices(window, rule, panelType, upperSide:=True)
                If upperIndices.Length >= rule.MinimumPoints Then
                    matches.Add(New RuleMatch(SpcRuleSide.UpperSideOnly,
                        IncludeTerminalPoint(upperIndices, window(window.Count - 1).PointIndex)))
                End If
            End If

            If rule.Side <> SpcRuleSide.UpperSideOnly Then
                Dim lowerIndices As Integer() = GetBeyondIndices(window, rule, panelType, upperSide:=False)
                If lowerIndices.Length >= rule.MinimumPoints Then
                    matches.Add(New RuleMatch(SpcRuleSide.LowerSideOnly,
                        IncludeTerminalPoint(lowerIndices, window(window.Count - 1).PointIndex)))
                End If
            End If

            Return matches.ToArray()
        End Function

        Private Shared Function EvaluateRun(window As List(Of SpcPointResult),
                                            rule As SpcRuleDefinition) As RuleMatch()
            Dim allUpper As Boolean = True
            Dim allLower As Boolean = True
            For i As Integer = 0 To window.Count - 1
                Dim z As Double = window(i).StandardizedValue
                If z <= 0.0 Then allUpper = False
                If z >= 0.0 Then allLower = False
            Next

            Dim indices As Integer() = GetAllPointIndices(window)
            If allUpper AndAlso rule.Side <> SpcRuleSide.LowerSideOnly Then
                Return {New RuleMatch(SpcRuleSide.UpperSideOnly, indices)}
            End If
            If allLower AndAlso rule.Side <> SpcRuleSide.UpperSideOnly Then
                Return {New RuleMatch(SpcRuleSide.LowerSideOnly, indices)}
            End If
            Return Array.Empty(Of RuleMatch)()
        End Function

        Private Shared Function EvaluateTrend(window As List(Of SpcPointResult),
                                              rule As SpcRuleDefinition) As RuleMatch()
            Dim increasing As Boolean = True
            Dim decreasing As Boolean = True
            For i As Integer = 1 To window.Count - 1
                Dim previousValue As Double = window(i - 1).StandardizedValue
                Dim currentValue As Double = window(i).StandardizedValue
                If currentValue <= previousValue Then increasing = False
                If currentValue >= previousValue Then decreasing = False
            Next

            Dim indices As Integer() = GetAllPointIndices(window)
            If increasing AndAlso rule.Side <> SpcRuleSide.LowerSideOnly Then
                Return {New RuleMatch(SpcRuleSide.UpperSideOnly, indices)}
            End If
            If decreasing AndAlso rule.Side <> SpcRuleSide.UpperSideOnly Then
                Return {New RuleMatch(SpcRuleSide.LowerSideOnly, indices)}
            End If
            Return Array.Empty(Of RuleMatch)()
        End Function

        Private Shared Function IsAlternating(window As List(Of SpcPointResult)) As Boolean
            Dim previousDirection As Integer = 0
            For i As Integer = 1 To window.Count - 1
                Dim difference As Double = window(i).StandardizedValue -
                                           window(i - 1).StandardizedValue
                If difference = 0.0 Then Return False

                Dim direction As Integer = If(difference > 0.0, 1, -1)
                If previousDirection <> 0 AndAlso direction = previousDirection Then
                    Return False
                End If
                previousDirection = direction
            Next
            Return True
        End Function

        Private Shared Function AllWithinSigma(window As List(Of SpcPointResult), threshold As Double) As Boolean
            For i As Integer = 0 To window.Count - 1
                If Math.Abs(window(i).StandardizedValue) >= threshold Then Return False
            Next
            Return True
        End Function

        Private Shared Function AllBeyondSigmaOnBothSides(window As List(Of SpcPointResult),
                                                            threshold As Double) As Boolean

            Dim hasUpper As Boolean = False
            Dim hasLower As Boolean = False
            For i As Integer = 0 To window.Count - 1
                Dim z As Double = window(i).StandardizedValue
                If Math.Abs(z) <= threshold Then Return False
                If z > 0.0 Then hasUpper = True
                If z < 0.0 Then hasLower = True
            Next
            Return hasUpper AndAlso hasLower
        End Function

        Private Shared Function GetBeyondIndices(window As List(Of SpcPointResult),
                                                 rule As SpcRuleDefinition,
                                                 panelType As SpcPanelType,
                                                 upperSide As Boolean) As Integer()

            Dim indices As New List(Of Integer)()
            For i As Integer = 0 To window.Count - 1
                Dim point As SpcPointResult = window(i)
                Dim beyond As Boolean

                If IsSinglePointExactLimitRule(point, rule, panelType) Then
                    ' Exact-probability attribute charts deliberately have no
                    ' one- or two-sigma zone boundaries.  For their single-point
                    ' Rule 1, test the same point-specific exact LCL/UCL that is
                    ' displayed on the chart instead of the normal-approximation z.
                    beyond = If(upperSide,
                                point.Value > point.UpperControlLimit,
                                point.Value < point.LowerControlLimit)
                Else
                    Dim z As Double = point.StandardizedValue
                    beyond = If(upperSide,
                                z > rule.SigmaThreshold,
                                z < -rule.SigmaThreshold)
                End If

                If beyond Then
                    indices.Add(point.PointIndex)
                End If
            Next
            Return indices.ToArray()
        End Function

        Private Shared Function IsSinglePointExactLimitRule(point As SpcPointResult,
                                                            rule As SpcRuleDefinition,
                                                            panelType As SpcPanelType) As Boolean

            If rule.Kind <> SpcRuleKind.BeyondSigma OrElse
               rule.WindowSize <> 1 OrElse
               rule.MinimumPoints <> 1 Then
                Return False
            End If

            Select Case panelType
                Case SpcPanelType.Proportion,
                     SpcPanelType.NumberNonconforming,
                     SpcPanelType.DefectCount,
                     SpcPanelType.DefectRate
                    ' Supported exact-probability attribute panels.
                Case Else
                    Return False
            End Select

            ' Exact discrete limits are the only current SPC point results that
            ' supply finite control limits while leaving every sigma-zone limit
            ' undefined. This keeps the rule evaluator host-neutral and avoids
            ' adding calculation-method state to immutable result panels.
            Return IsFinite(point.Value) AndAlso
                   IsFinite(point.LowerControlLimit) AndAlso
                   IsFinite(point.UpperControlLimit) AndAlso
                   Double.IsNaN(point.LowerOneSigmaLimit) AndAlso
                   Double.IsNaN(point.UpperOneSigmaLimit) AndAlso
                   Double.IsNaN(point.LowerTwoSigmaLimit) AndAlso
                   Double.IsNaN(point.UpperTwoSigmaLimit)
        End Function

        Private Shared Function IncludeTerminalPoint(indices As Integer(), terminalPointIndex As Integer) As Integer()
            Dim values As New List(Of Integer)(indices)
            If Not values.Contains(terminalPointIndex) Then values.Add(terminalPointIndex)
            values.Sort()
            Return values.ToArray()
        End Function

        Private Shared Function GetAllPointIndices(window As List(Of SpcPointResult)) As Integer()

            Dim values(window.Count - 1) As Integer
            For i As Integer = 0 To window.Count - 1
                values(i) = window(i).PointIndex
            Next
            Return values
        End Function

        Private Shared Function SelectApplicableRules(rules As SpcRuleDefinition(),
                                                      panelType As SpcPanelType) As SpcRuleDefinition()

            Dim panelScope As SpcRuleScope = GetPanelRuleScope(panelType)
            Dim values As New List(Of SpcRuleDefinition)()
            For i As Integer = 0 To rules.Length - 1
                If (rules(i).Scope And panelScope) <> SpcRuleScope.None Then
                    values.Add(rules(i))
                End If
            Next
            Return values.ToArray()
        End Function

        Private Shared Function PhaseIsSelected(phase As SpcPhase, scope As SpcRulePhaseScope) As Boolean
            Dim phaseFlag As SpcRulePhaseScope
            Select Case phase
                Case SpcPhase.PhaseI
                    phaseFlag = SpcRulePhaseScope.PhaseI
                Case SpcPhase.PhaseII
                    phaseFlag = SpcRulePhaseScope.PhaseII
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(phase))
            End Select
            Return (scope And phaseFlag) <> SpcRulePhaseScope.None
        End Function

        Private Shared Function IsRuleEligible(point As SpcPointResult) As Boolean
            Return point.IncludedInRuleEvaluation AndAlso
                   point.HasFiniteValue AndAlso
                   IsFinite(point.StandardizedValue)
        End Function

        Private Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Private Shared Function CopyPointWithSignals(point As SpcPointResult,
                                                     signalRuleNumbers As Integer()) As SpcPointResult

            Return New SpcPointResult(
                point.PointIndex,
                point.Value,
                point.CenterLine,
                point.LowerControlLimit,
                point.UpperControlLimit,
                label:=point.Label,
                stageId:=point.StageId,
                phase:=point.Phase,
                sequenceValue:=point.SequenceValue,
                standardError:=point.StandardError,
                standardizedValue:=point.StandardizedValue,
                lowerOneSigmaLimit:=point.LowerOneSigmaLimit,
                upperOneSigmaLimit:=point.UpperOneSigmaLimit,
                lowerTwoSigmaLimit:=point.LowerTwoSigmaLimit,
                upperTwoSigmaLimit:=point.UpperTwoSigmaLimit,
                effectiveSampleSize:=point.EffectiveSampleSize,
                exposure:=point.Exposure,
                sourceRowIndices:=point.SourceRowIndices,
                includedInParameterEstimation:=point.IncludedInParameterEstimation,
                includedInRuleEvaluation:=point.IncludedInRuleEvaluation,
                exclusionScope:=point.ExclusionScope,
                exclusionReason:=point.ExclusionReason,
                signalRuleNumbers:=signalRuleNumbers)
        End Function

        Private Shared Function ContainsEquivalentSignal(signals As SpcSignalResult(),
                                                         candidate As SpcSignalResult) As Boolean

            For i As Integer = 0 To signals.Length - 1
                If SignalsAreEquivalent(signals(i), candidate) Then Return True
            Next
            Return False
        End Function

        Private Shared Function ContainsEquivalentSignal(signals As List(Of SpcSignalResult),
                                                         candidate As SpcSignalResult) As Boolean

            For i As Integer = 0 To signals.Count - 1
                If SignalsAreEquivalent(signals(i), candidate) Then Return True
            Next
            Return False
        End Function

        Private Shared Function SignalsAreEquivalent(left As SpcSignalResult, right As SpcSignalResult) As Boolean
            Return left.PanelType = right.PanelType AndAlso
                   String.Equals(left.StageId,
                                 right.StageId,
                                 StringComparison.OrdinalIgnoreCase) AndAlso
                   String.Equals(left.RuleCode,
                                 right.RuleCode,
                                 StringComparison.OrdinalIgnoreCase) AndAlso
                   left.TriggeredSide = right.TriggeredSide AndAlso
                   left.TerminalPointIndex = right.TerminalPointIndex AndAlso
                   left.WindowStartPointIndex = right.WindowStartPointIndex AndAlso
                   left.WindowEndPointIndex = right.WindowEndPointIndex
        End Function

        Private Shared Function BuildSignalMessage(rule As SpcRuleDefinition,
                                                   triggeredSide As SpcRuleSide,
                                                   terminalPoint As SpcPointResult,
                                                   panelType As SpcPanelType) As String

            Dim prefix As String = "Rule " &
                rule.RuleNumber.ToString(CultureInfo.InvariantCulture) &
                " (" & rule.RuleCode & ") signalled at point " &
                (terminalPoint.PointIndex + 1).ToString(CultureInfo.InvariantCulture) &
                " [" & terminalPoint.Label & "] in stage '" &
                terminalPoint.StageId & "': "

            Dim sideText As String = GetSideText(triggeredSide)
            Dim thresholdText As String = rule.SigmaThreshold.ToString(
                "0.###", CultureInfo.InvariantCulture)

            Select Case rule.Kind
                Case SpcRuleKind.BeyondSigma
                    If IsSinglePointExactLimitRule(terminalPoint, rule, panelType) Then
                        Return prefix & "a point was beyond the exact " &
                            If(triggeredSide = SpcRuleSide.UpperSideOnly,
                               "upper", "lower") & " control limit."
                    End If
                    Return prefix & "a point was beyond " & thresholdText &
                        " sigma " & sideText & " the centre line."

                Case SpcRuleKind.KOfMConsecutiveBeyondSigma
                    Return prefix & rule.MinimumPoints.ToString(CultureInfo.InvariantCulture) &
                        " of " & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " points were beyond " & thresholdText & " sigma " &
                        sideText & " the centre line."

                Case SpcRuleKind.RunOnOneSide
                    Return prefix & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " consecutive points were " & sideText & " the centre line."

                Case SpcRuleKind.MonotonicTrend
                    Dim direction As String =
                        If(triggeredSide = SpcRuleSide.UpperSideOnly,
                           "increasing", "decreasing")
                    Return prefix & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " consecutive points were strictly " & direction & "."

                Case SpcRuleKind.Alternating
                    Return prefix & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " consecutive points alternated in direction."

                Case SpcRuleKind.AllWithinSigma
                    Return prefix & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " consecutive points were within " & thresholdText &
                        " sigma of the centre line."

                Case SpcRuleKind.AllBeyondSigmaOnBothSides
                    Return prefix & rule.WindowSize.ToString(CultureInfo.InvariantCulture) &
                        " consecutive points were outside " & thresholdText &
                        " sigma with observations on both sides."

                Case Else
                    Return prefix & rule.DisplayName & "."
            End Select
        End Function

        Private Shared Function GetSideText(side As SpcRuleSide) As String
            Select Case side
                Case SpcRuleSide.UpperSideOnly
                    Return "above"
                Case SpcRuleSide.LowerSideOnly
                    Return "below"
                Case Else
                    Return "on either side of"
            End Select
        End Function

        Private Shared Sub AddUniqueMessages(target As List(Of String),
                                             messages As String())
            If messages Is Nothing Then Return
            For i As Integer = 0 To messages.Length - 1
                Dim message As String = If(messages(i), String.Empty).Trim()
                If message.Length > 0 AndAlso Not target.Contains(message) Then
                    target.Add(message)
                End If
            Next
        End Sub

        Private Shared Sub CheckCancellationPeriodically(
            index As Integer,
            cancellationRequested As Func(Of Boolean))

            If (index And 127) = 0 Then ThrowIfCancellationRequested(cancellationRequested)
        End Sub

        Private Shared Sub ThrowIfCancellationRequested(
            cancellationRequested As Func(Of Boolean))

            SpcEngine.ThrowIfCancellationRequested(cancellationRequested)
        End Sub

        Private NotInheritable Class RuleMatch
            Public Sub New(triggeredSide As SpcRuleSide,
                           contributingPointIndices As Integer())
                Me.TriggeredSide = triggeredSide
                Me.ContributingPointIndices = contributingPointIndices
            End Sub

            Public ReadOnly Property TriggeredSide As SpcRuleSide
            Public ReadOnly Property ContributingPointIndices As Integer()
        End Class
    End Class

End Namespace
