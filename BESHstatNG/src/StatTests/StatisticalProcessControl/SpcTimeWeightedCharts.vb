Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Calculates host-neutral tabular CUSUM, EWMA, and moving-average charts.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' CUSUM reference values, decision intervals, and head starts are expressed
    ''' in units of the in-control process standard deviation. The upper CUSUM is
    ''' plotted above zero and the lower CUSUM is plotted below zero.
    ''' </para>
    ''' <para>
    ''' EWMA and moving-average statistics remain in the original measurement
    ''' units. Dynamic startup limits are used unless steady-state limits are
    ''' requested. For a moving-average chart, requesting steady-state limits also
    ''' suppresses incomplete startup windows rather than applying full-window
    ''' limits to partial averages.
    ''' </para>
    ''' <para>
    ''' The calculator contains no Excel, Excel-DNA, chart-rendering, or WinForms
    ''' code. It is discovered automatically by <see cref="SpcEngine"/> through
    ''' the <see cref="ISpcChartCalculator"/> contract.
    ''' </para>
    ''' </remarks>
    Friend NotInheritable Class SpcTimeWeightedChartCalculator
        Implements ISpcChartCalculator

        Private Const DefaultEwmaLambda As Double = 0.2
        Private Const DefaultCusumReferenceValue As Double = 0.5
        Private Const DefaultCusumDecisionInterval As Double = 5.0
        Private Const DefaultHeadStart As Double = 0.0
        Private Const DefaultMovingAverageSpan As Integer = 3

        Private Const UpperCusumSignalNumber As Integer = 101
        Private Const LowerCusumSignalNumber As Integer = 102
        Private Const EwmaSignalNumber As Integer = 103
        Private Const MovingAverageSignalNumber As Integer = 104

        Private Shared ReadOnly SupportedChartTypes As SpcChartType() = {
            SpcChartType.Cusum,
            SpcChartType.Ewma,
            SpcChartType.MovingAverage
        }

        Public Sub New()
        End Sub

        Public Function CanCalculate(chartType As SpcChartType) As Boolean _
            Implements ISpcChartCalculator.CanCalculate

            For i As Integer = 0 To SupportedChartTypes.Length - 1
                If SupportedChartTypes(i) = chartType Then Return True
            Next
            Return False
        End Function

        Public Function Calculate(request As SpcFitRequest,
                                  cancellationRequested As Func(Of Boolean)) As SpcCalculationResult _
            Implements ISpcChartCalculator.Calculate

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If Not CanCalculate(request.ChartType) Then
                Throw New NotSupportedException(
                    "The time-weighted calculator does not support " &
                    request.ChartType.ToString() & ".")
            End If

            CheckCancellation(cancellationRequested)

            Dim options As SpcAnalysisOptions = request.AnalysisOptions
            ValidateCalculatorOptions(request, options)

            Dim warnings As New List(Of String)()
            Dim rawPoints As RawTimePoint() = BuildRawPoints(
                request, options, warnings, cancellationRequested)
            Dim stages As StageContext() = BuildStages(request, rawPoints.Length)
            ApplyStageAssignments(rawPoints, stages)
            ApplyExclusions(rawPoints, request.Exclusions)

            If CountRetainedPoints(rawPoints) = 0 Then
                Throw New ArgumentException(
                    "No chart points remain after applying the missing-value policy.")
            End If

            Dim cache As New Dictionary(Of String, StageParameters)(
                StringComparer.OrdinalIgnoreCase)
            Dim visiting As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim panels As SpcPanelResult()

            Select Case request.ChartType
                Case SpcChartType.Cusum
                    panels = BuildCusumPanels(request,
                                              options,
                                              rawPoints,
                                              stages,
                                              cache,
                                              visiting,
                                              warnings,
                                              cancellationRequested)

                Case SpcChartType.Ewma
                    panels = {
                        BuildEwmaPanel(request,
                                       options,
                                       rawPoints,
                                       stages,
                                       cache,
                                       visiting,
                                       cancellationRequested)
                    }

                Case SpcChartType.MovingAverage
                    panels = {
                        BuildMovingAveragePanel(request,
                                                options,
                                                rawPoints,
                                                stages,
                                                cache,
                                                visiting,
                                                warnings,
                                                cancellationRequested)
                    }

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(request.ChartType))
            End Select

            CheckCancellation(cancellationRequested)
            Return New SpcCalculationResult(panels, warnings.ToArray())
        End Function

#Region "Validation and input construction"

        Private Shared Sub ValidateCalculatorOptions(request As SpcFitRequest,
                                                     options As SpcAnalysisOptions)
            If request.DataLayout <> SpcDataLayout.IndividualSequence Then
                Throw New ArgumentException(
                    "CUSUM, EWMA, and moving-average charts require individual-sequence data.")
            End If

            Dim measurements As Double(,) = request.Data.Measurements
            If measurements Is Nothing OrElse measurements.GetLength(1) <> 1 Then
                Throw New ArgumentException(
                    "Time-weighted charts require exactly one measurement column.")
            End If

            If options.ControlLimits.Method <> SpcControlLimitMethod.ShewhartSigma Then
                Throw New NotSupportedException(
                    "Time-weighted charts require sigma-based limits; exact-probability limits are not applicable.")
            End If

            If options.ControlLimits.MovingRangeLength > 25 Then
                Throw New ArgumentOutOfRangeException(
                    "MovingRangeLength",
                    "Moving-range constants are supported for lengths from 2 through 25.")
            End If

            Dim chartParameters As SpcChartParameters = request.ChartParameters
            Select Case request.ChartType
                Case SpcChartType.Cusum
                    Dim decisionInterval As Double = GetCusumDecisionInterval(chartParameters)
                    Dim headStart As Double = GetHeadStart(chartParameters)
                    If headStart >= decisionInterval Then
                        Throw New ArgumentOutOfRangeException(
                            "HeadStart",
                            "The CUSUM head start must be smaller than the decision interval.")
                    End If

                Case SpcChartType.Ewma
                    Dim lambda As Double = GetEwmaLambda(chartParameters)
                    If lambda <= 0.0 OrElse lambda > 1.0 Then
                        Throw New ArgumentOutOfRangeException(
                            "EwmaLambda", "EWMA lambda must be in the interval (0, 1].")
                    End If

                Case SpcChartType.MovingAverage
                    Dim span As Integer = GetMovingAverageSpan(chartParameters)
                    If span < 2 Then
                        Throw New ArgumentOutOfRangeException(
                            "MovingAverageSpan",
                            "The moving-average span must be at least two.")
                    End If
            End Select
        End Sub

        Private Shared Function BuildRawPoints(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As RawTimePoint()

            Dim data As SpcInputData = request.Data
            Dim measurements As Double(,) = data.Measurements
            Dim labels As String() = data.Labels
            Dim sequenceValues As Double() = data.SequenceValues
            Dim sourceRows As Integer() = data.SourceRowIndices
            Dim result(data.RowCount - 1) As RawTimePoint
            Dim omittedCount As Integer = 0

            For row As Integer = 0 To data.RowCount - 1
                CheckCancellationPeriodically(row, cancellationRequested)

                Dim value As Double = measurements(row, 0)
                Dim omitted As Boolean = Double.IsNaN(value)
                If omitted AndAlso
                   options.MissingValuePolicy = SpcMissingValuePolicy.Reject Then
                    Throw New ArgumentException(
                        "A missing measurement was found at point " &
                        (row + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If
                If omitted Then omittedCount += 1

                result(row) = New RawTimePoint With {
                    .LogicalIndex = row,
                    .Value = value,
                    .Label = GetRowLabel(labels, row),
                    .SequenceValue = GetSequenceValue(sequenceValues, row),
                    .SourceRowIndices = {sourceRows(row)},
                    .IsOmitted = omitted
                }
            Next

            If omittedCount > 0 Then
                Dim gapMessage As String
                If options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    gapMessage =
                        "Each missing point resets the time-weighted recursion or moving window."
                Else
                    gapMessage =
                        "Missing points are skipped without updating the time-weighted recursion or moving window."
                End If

                warnings.Add(
                    omittedCount.ToString(CultureInfo.InvariantCulture) &
                    If(omittedCount = 1, " chart point was", " chart points were") &
                    " omitted because required measurements were missing. " &
                    gapMessage)
            End If

            Return result
        End Function

        Private Shared Function GetRowLabel(labels As String(), row As Integer) As String
            If labels Is Nothing Then Return String.Empty
            Return If(labels(row), String.Empty).Trim()
        End Function

        Private Shared Function GetSequenceValue(values As Double(),
                                                 row As Integer) As Nullable(Of Double)
            If values Is Nothing OrElse Double.IsNaN(values(row)) Then Return Nothing
            Return values(row)
        End Function

#End Region

#Region "Stages, exclusions, and parameter estimation"

        Private Shared Function BuildStages(request As SpcFitRequest,
                                            logicalPointCount As Integer) As StageContext()
            If logicalPointCount <= 0 Then
                Throw New ArgumentException("At least one logical chart point is required.")
            End If

            Dim definitions As SpcStageDefinition() = request.Stages
            If definitions.Length = 0 Then
                Dim mode As SpcStageLimitMode
                Select Case request.AnalysisOptions.ControlLimits.ParameterSource
                    Case SpcParameterSource.EstimateFromPhaseI
                        mode = SpcStageLimitMode.EstimateFromStageData
                    Case SpcParameterSource.UseHistoricalParameters
                        mode = SpcStageLimitMode.UseHistoricalParameters
                    Case Else
                        Throw New ArgumentException(
                            "Defined-by-stage parameter sourcing requires at least one stage definition.")
                End Select

                definitions = {
                    New SpcStageDefinition("Stage1",
                                           0,
                                           logicalPointCount - 1,
                                           SpcPhase.PhaseI,
                                           mode)
                }
            Else
                definitions = CType(definitions.Clone(), SpcStageDefinition())
                Array.Sort(definitions,
                           Function(left As SpcStageDefinition,
                                    right As SpcStageDefinition) As Integer
                               Return left.FirstPointIndex.CompareTo(right.FirstPointIndex)
                           End Function)
            End If

            Dim contexts(definitions.Length - 1) As StageContext
            Dim expectedFirst As Integer = 0
            For i As Integer = 0 To definitions.Length - 1
                Dim definition As SpcStageDefinition = definitions(i)
                If definition.FirstPointIndex <> expectedFirst Then
                    Throw New ArgumentException(
                        "Stage ranges must cover the ordered chart points without gaps.")
                End If
                If definition.LastPointIndex >= logicalPointCount Then
                    Throw New ArgumentOutOfRangeException(
                        "Stages", "A stage extends beyond the number of chart points.")
                End If
                contexts(i) = New StageContext(definition)
                expectedFirst = definition.LastPointIndex + 1
            Next

            If expectedFirst <> logicalPointCount Then
                Throw New ArgumentException(
                    "Stage ranges must cover every ordered chart point.")
            End If
            Return contexts
        End Function

        Private Shared Sub ApplyStageAssignments(points As RawTimePoint(),
                                                 stages As StageContext())
            Dim stageIndex As Integer = 0
            For pointIndex As Integer = 0 To points.Length - 1
                While pointIndex > stages(stageIndex).Definition.LastPointIndex
                    stageIndex += 1
                End While
                points(pointIndex).Stage = stages(stageIndex)
            Next
        End Sub

        Private Shared Sub ApplyExclusions(points As RawTimePoint(),
                                           exclusions As SpcExclusionDefinition())
            For i As Integer = 0 To exclusions.Length - 1
                Dim exclusion As SpcExclusionDefinition = exclusions(i)
                If exclusion.PointIndex >= points.Length Then
                    Throw New ArgumentOutOfRangeException(
                        "Exclusions",
                        "An exclusion extends beyond the number of chart points.")
                End If

                Dim point As RawTimePoint = points(exclusion.PointIndex)
                point.ExclusionScope = point.ExclusionScope Or exclusion.Scope
                point.ExclusionReason = CombineReason(point.ExclusionReason,
                                                      exclusion.Reason)
            Next
        End Sub

        Private Shared Function ResolveParameters(
            stage As StageContext,
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawTimePoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String)) As StageParameters

            Dim cached As StageParameters = Nothing
            If cache.TryGetValue(stage.Definition.StageId, cached) Then Return cached
            If Not visiting.Add(stage.Definition.StageId) Then
                Throw New ArgumentException("Stage limit references contain a cycle.")
            End If

            Dim result As StageParameters
            Select Case stage.Definition.LimitMode
                Case SpcStageLimitMode.EstimateFromStageData
                    result = EstimateParameters(stage, options, rawPoints)

                Case SpcStageLimitMode.UseHistoricalParameters
                    Dim history As SpcHistoricalParameters = FindHistoricalParameters(
                        request, stage.Definition.StageId)
                    If history Is Nothing Then
                        Throw New ArgumentException(
                            "No historical parameters were supplied for stage '" &
                            stage.Definition.StageId & "'.")
                    End If
                    If Not history.ProcessMean.HasValue Then
                        Throw New ArgumentException(
                            "Historical process mean is required for stage '" &
                            stage.Definition.StageId & "'.")
                    End If
                    If Not history.ProcessSigma.HasValue OrElse
                       history.ProcessSigma.Value <= 0.0 Then
                        Throw New ArgumentException(
                            "A positive historical process sigma is required for stage '" &
                            stage.Definition.StageId & "'.")
                    End If

                    result = New StageParameters With {
                        .Center = history.ProcessMean.Value,
                        .Sigma = history.ProcessSigma.Value,
                        .CenterStandardError = Double.NaN,
                        .EstimationPointCount = -1,
                        .SigmaEstimationPointCount = -1,
                        .LimitMode = SpcStageLimitMode.UseHistoricalParameters,
                        .SourceStageId = String.Empty,
                        .Method = "Historical parameters"
                    }

                Case SpcStageLimitMode.UseReferenceStage
                    Dim referenced As StageContext = FindStage(
                        stages, stage.Definition.ReferenceStageId)
                    Dim source As StageParameters = ResolveParameters(
                        referenced,
                        request,
                        options,
                        rawPoints,
                        stages,
                        cache,
                        visiting)
                    result = source.CopyForReference(stage.Definition.ReferenceStageId)

                Case Else
                    Throw New ArgumentOutOfRangeException("LimitMode")
            End Select

            visiting.Remove(stage.Definition.StageId)
            cache.Add(stage.Definition.StageId, result)
            Return result
        End Function

        Private Shared Function EstimateParameters(
            stage As StageContext,
            options As SpcAnalysisOptions,
            rawPoints As RawTimePoint()) As StageParameters

            Dim values(stage.Definition.PointCount - 1) As Double
            For i As Integer = 0 To values.Length - 1
                values(i) = Double.NaN
            Next

            Dim mean As Double = 0.0
            Dim meanCount As Integer = 0
            For logicalIndex As Integer = stage.Definition.FirstPointIndex To stage.Definition.LastPointIndex
                Dim point As RawTimePoint = rawPoints(logicalIndex)
                If IsEstimationEligible(point) Then
                    values(logicalIndex - stage.Definition.FirstPointIndex) = point.Value
                    meanCount += 1
                    mean += (point.Value - mean) / CDbl(meanCount)
                End If
            Next

            If meanCount = 0 Then
                Throw New ArgumentException(
                    "Stage '" & stage.Definition.StageId &
                    "' has no observations eligible for parameter estimation.")
            End If

            Dim limits As SpcControlLimitOptions = options.ControlLimits
            Dim sigmaEstimate As SpcSigmaEstimate =
                SpcStatistics.EstimateSigmaFromIndividuals(
                    values,
                    limits.WithinSigmaEstimator,
                    limits.MovingRangeLength,
                    limits.UseBiasCorrection)

            If sigmaEstimate.Value <= 0.0 Then
                Throw New ArgumentException(
                    "Stage '" & stage.Definition.StageId &
                    "' has zero estimated within-process variation; " &
                    "a time-weighted chart requires positive sigma.")
            End If

            Return New StageParameters With {
                .Center = mean,
                .Sigma = sigmaEstimate.Value,
                .CenterStandardError = sigmaEstimate.Value / Math.Sqrt(CDbl(meanCount)),
                .EstimationPointCount = meanCount,
                .SigmaEstimationPointCount = sigmaEstimate.ContributingPointCount,
                .LimitMode = SpcStageLimitMode.EstimateFromStageData,
                .SourceStageId = String.Empty,
                .Method = sigmaEstimate.Method
            }
        End Function

        Private Shared Function FindHistoricalParameters(
            request As SpcFitRequest,
            stageId As String) As SpcHistoricalParameters

            Dim defaultValue As SpcHistoricalParameters = Nothing
            Dim values As SpcHistoricalParameters() = request.HistoricalParameters
            For i As Integer = 0 To values.Length - 1
                If values(i).AppliesToAllStages Then
                    defaultValue = values(i)
                ElseIf String.Equals(values(i).StageId,
                                     stageId,
                                     StringComparison.OrdinalIgnoreCase) Then
                    Return values(i)
                End If
            Next
            Return defaultValue
        End Function

        Private Shared Function FindStage(stages As StageContext(),
                                          stageId As String) As StageContext
            For i As Integer = 0 To stages.Length - 1
                If String.Equals(stages(i).Definition.StageId,
                                 stageId,
                                 StringComparison.OrdinalIgnoreCase) Then
                    Return stages(i)
                End If
            Next
            Throw New ArgumentException("Reference stage '" & stageId & "' was not found.")
        End Function

        Private Shared Function IsEstimationEligible(point As RawTimePoint) As Boolean
            Return Not point.IsOmitted AndAlso
                   (point.ExclusionScope And SpcExclusionScope.ParameterEstimation) =
                   SpcExclusionScope.None
        End Function

        Private Shared Function IsRuleEvaluationExcluded(point As RawTimePoint) As Boolean
            Return (point.ExclusionScope And SpcExclusionScope.RuleEvaluation) <>
                   SpcExclusionScope.None
        End Function

#End Region

#Region "CUSUM"

        Private Shared Function BuildCusumPanels(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawTimePoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult()

            Dim chartParameters As SpcChartParameters = request.ChartParameters
            Dim referenceValue As Double = GetCusumReferenceValue(chartParameters)
            Dim decisionInterval As Double = GetCusumDecisionInterval(chartParameters)
            Dim headStart As Double = GetHeadStart(chartParameters)

            If referenceValue >= decisionInterval Then
                warnings.Add(
                    "The CUSUM reference value is not smaller than the decision interval; " &
                    "this design may be insensitive to practically relevant shifts.")
            End If

            Dim upperRule As SpcRuleDefinition = CreateCusumDecisionRule(
                upper:=True, decisionInterval:=decisionInterval)
            Dim lowerRule As SpcRuleDefinition = CreateCusumDecisionRule(
                upper:=False, decisionInterval:=decisionInterval)

            Dim upperPoints As New List(Of SpcPointResult)()
            Dim lowerPoints As New List(Of SpcPointResult)()
            Dim upperSignals As New List(Of SpcSignalResult)()
            Dim lowerSignals As New List(Of SpcSignalResult)()

            Dim currentStageId As String = Nothing
            Dim upperState As Double = headStart
            Dim lowerMagnitude As Double = headStart
            Dim hasState As Boolean = False

            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawTimePoint = rawPoints(i)
                Dim stageId As String = raw.Stage.Definition.StageId
                Dim stageChanged As Boolean = currentStageId Is Nothing OrElse
                    Not String.Equals(currentStageId,
                                      stageId,
                                      StringComparison.OrdinalIgnoreCase)
                If stageChanged Then
                    currentStageId = stageId
                    upperState = headStart
                    lowerMagnitude = headStart
                    hasState = True
                End If

                If raw.IsOmitted Then
                    If options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                        upperState = headStart
                        lowerMagnitude = headStart
                        hasState = False
                    End If
                    Continue For
                End If

                Dim parameters As StageParameters = ResolveParameters(
                    raw.Stage, request, options, rawPoints, stages, cache, visiting)
                If Not hasState Then
                    upperState = headStart
                    lowerMagnitude = headStart
                    hasState = True
                End If

                Dim priorUpperState As Double = upperState
                Dim priorLowerMagnitude As Double = lowerMagnitude
                Dim standardizedObservation As Double = (raw.Value - parameters.Center) / parameters.Sigma
                Dim upperValue As Double = Math.Max(0.0, priorUpperState + standardizedObservation - referenceValue)
                Dim lowerValueMagnitude As Double = Math.Max(0.0, priorLowerMagnitude - standardizedObservation - referenceValue)
                Dim lowerValue As Double = -lowerValueMagnitude

                Dim includedInEstimation As Boolean = raw.Stage.Definition.LimitMode =
                        SpcStageLimitMode.EstimateFromStageData AndAlso IsEstimationEligible(raw)
                Dim includedInRules As Boolean = Not IsRuleEvaluationExcluded(raw)

                Dim upperSignalled As Boolean = includedInRules AndAlso upperValue > decisionInterval
                Dim lowerSignalled As Boolean = includedInRules AndAlso lowerValue < -decisionInterval
                Dim upperSignalNumbers As Integer() = Nothing
                Dim lowerSignalNumbers As Integer() = Nothing
                If upperSignalled Then upperSignalNumbers = {UpperCusumSignalNumber}
                If lowerSignalled Then lowerSignalNumbers = {LowerCusumSignalNumber}

                upperPoints.Add(New SpcPointResult(
                    raw.LogicalIndex,
                    upperValue,
                    0.0,
                    0.0,
                    decisionInterval,
                    label:=raw.Label,
                    stageId:=stageId,
                    phase:=raw.Stage.Definition.Phase,
                    sequenceValue:=raw.SequenceValue,
                    standardError:=1.0,
                    standardizedValue:=upperValue,
                    effectiveSampleSize:=1.0,
                    sourceRowIndices:=raw.SourceRowIndices,
                    includedInParameterEstimation:=includedInEstimation,
                    includedInRuleEvaluation:=includedInRules,
                    exclusionScope:=raw.ExclusionScope,
                    exclusionReason:=raw.ExclusionReason,
                    signalRuleNumbers:=upperSignalNumbers))

                lowerPoints.Add(New SpcPointResult(
                    raw.LogicalIndex,
                    lowerValue,
                    0.0,
                    -decisionInterval,
                    0.0,
                    label:=raw.Label,
                    stageId:=stageId,
                    phase:=raw.Stage.Definition.Phase,
                    sequenceValue:=raw.SequenceValue,
                    standardError:=1.0,
                    standardizedValue:=lowerValue,
                    effectiveSampleSize:=1.0,
                    sourceRowIndices:=raw.SourceRowIndices,
                    includedInParameterEstimation:=includedInEstimation,
                    includedInRuleEvaluation:=includedInRules,
                    exclusionScope:=raw.ExclusionScope,
                    exclusionReason:=raw.ExclusionReason,
                    signalRuleNumbers:=lowerSignalNumbers))

                If upperSignalled Then
                    upperSignals.Add(CreateIntrinsicSignal(
                        SpcPanelType.UpperCusum,
                        raw,
                        upperRule,
                        SpcRuleSide.UpperSideOnly,
                        "The upper CUSUM exceeded decision interval h=" &
                        FormatNumber(decisionInterval) & "."))
                End If
                If lowerSignalled Then
                    lowerSignals.Add(CreateIntrinsicSignal(
                        SpcPanelType.LowerCusum,
                        raw,
                        lowerRule,
                        SpcRuleSide.LowerSideOnly,
                        "The lower CUSUM exceeded decision interval h=" &
                        FormatNumber(decisionInterval) & " in the negative direction."))
                End If

                If includedInRules Then
                    upperState = upperValue
                    lowerMagnitude = lowerValueMagnitude
                ElseIf options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    upperState = headStart
                    lowerMagnitude = headStart
                    hasState = False
                Else
                    ' Skip the excluded observation without allowing it to alter
                    ' the state carried into the next eligible point.
                    upperState = priorUpperState
                    lowerMagnitude = priorLowerMagnitude
                End If
            Next

            Dim upperParameters As SpcParameterEstimate() = BuildParameterEstimates(
                request,
                stages,
                rawPoints,
                cache,
                SpcPanelType.UpperCusum,
                referenceValue,
                decisionInterval,
                headStart)
            Dim lowerParameters As SpcParameterEstimate() = BuildParameterEstimates(
                request,
                stages,
                rawPoints,
                cache,
                SpcPanelType.LowerCusum,
                referenceValue,
                decisionInterval,
                headStart)

            Return {
                New SpcPanelResult(
                    SpcPanelType.UpperCusum,
                    "Upper CUSUM",
                    upperPoints.ToArray(),
                    ResolveAxisTitle(request, "Standardized upper CUSUM"),
                    upperParameters,
                    upperSignals.ToArray()),
                New SpcPanelResult(
                    SpcPanelType.LowerCusum,
                    "Lower CUSUM",
                    lowerPoints.ToArray(),
                    ResolveAxisTitle(request, "Standardized lower CUSUM"),
                    lowerParameters,
                    lowerSignals.ToArray())
            }
        End Function

        Private Shared Function CreateCusumDecisionRule(
            upper As Boolean,
            decisionInterval As Double) As SpcRuleDefinition

            If upper Then
                Return New SpcRuleDefinition(
                    "CUSUM-U",
                    UpperCusumSignalNumber,
                    SpcRuleKind.BeyondSigma,
                    1,
                    1,
                    decisionInterval,
                    side:=SpcRuleSide.UpperSideOnly,
                    scope:=SpcRuleScope.TimeWeightedPanels,
                    displayName:="Upper CUSUM decision interval",
                    description:="The standardized upper CUSUM exceeded its decision interval.")
            End If

            Return New SpcRuleDefinition(
                "CUSUM-L",
                LowerCusumSignalNumber,
                SpcRuleKind.BeyondSigma,
                1,
                1,
                decisionInterval,
                side:=SpcRuleSide.LowerSideOnly,
                scope:=SpcRuleScope.TimeWeightedPanels,
                displayName:="Lower CUSUM decision interval",
                description:="The standardized lower CUSUM exceeded its decision interval.")
        End Function

#End Region

#Region "EWMA"

        Private Shared Function BuildEwmaPanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawTimePoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim lambda As Double = GetEwmaLambda(request.ChartParameters)
            Dim limitMultiplier As Double = options.ControlLimits.SigmaMultiplier
            Dim steadyState As Boolean = request.ChartParameters.UseSteadyStateLimits
            Dim signalRule As SpcRuleDefinition = CreateLimitSignalRule(
                "EWMA-CL",
                EwmaSignalNumber,
                limitMultiplier,
                "EWMA control limit")

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim currentStageId As String = Nothing
            Dim previousEwma As Double = Double.NaN
            Dim recursionAge As Integer = 0
            Dim hasState As Boolean = False

            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawTimePoint = rawPoints(i)
                Dim stageId As String = raw.Stage.Definition.StageId
                Dim stageChanged As Boolean = currentStageId Is Nothing OrElse
                    Not String.Equals(currentStageId,
                                      stageId,
                                      StringComparison.OrdinalIgnoreCase)
                If stageChanged Then
                    currentStageId = stageId
                    recursionAge = 0
                    hasState = False
                End If

                If raw.IsOmitted Then
                    If options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                        recursionAge = 0
                        hasState = False
                    End If
                    Continue For
                End If

                Dim parameters As StageParameters = ResolveParameters(raw.Stage, request, options, rawPoints, stages, cache, visiting)
                If Not hasState Then
                    previousEwma = parameters.Center
                    recursionAge = 0
                    hasState = True
                End If

                Dim priorEwma As Double = previousEwma
                Dim priorRecursionAge As Integer = recursionAge
                Dim ewma As Double = lambda * raw.Value + (1.0 - lambda) * priorEwma
                Dim pointRecursionAge As Integer = priorRecursionAge + 1
                recursionAge += 1

                Dim varianceFactor As Double = lambda / (2.0 - lambda)
                If Not steadyState Then
                    varianceFactor *= 1.0 - Math.Pow(1.0 - lambda, 2.0 * CDbl(pointRecursionAge))
                End If
                Dim standardError As Double = parameters.Sigma * Math.Sqrt(Math.Max(0.0, varianceFactor))
                Dim limits As LimitValues = BuildSymmetricLimits(parameters.Center, standardError, limitMultiplier)
                Dim includedInEstimation As Boolean = raw.Stage.Definition.LimitMode =
                        SpcStageLimitMode.EstimateFromStageData AndAlso IsEstimationEligible(raw)
                Dim includedInRules As Boolean = Not IsRuleEvaluationExcluded(raw)
                Dim signalled As Boolean = includedInRules AndAlso
                    (ewma < limits.LowerControlLimit OrElse ewma > limits.UpperControlLimit)
                Dim signalNumbers As Integer() = Nothing
                If signalled Then signalNumbers = {EwmaSignalNumber}

                points.Add(New SpcPointResult(
                    raw.LogicalIndex,
                    ewma,
                    parameters.Center,
                    limits.LowerControlLimit,
                    limits.UpperControlLimit,
                    label:=raw.Label,
                    stageId:=stageId,
                    phase:=raw.Stage.Definition.Phase,
                    sequenceValue:=raw.SequenceValue,
                    standardError:=standardError,
                    standardizedValue:=Standardize(ewma, parameters.Center, standardError),
                    lowerOneSigmaLimit:=limits.LowerOneSigmaLimit,
                    upperOneSigmaLimit:=limits.UpperOneSigmaLimit,
                    lowerTwoSigmaLimit:=limits.LowerTwoSigmaLimit,
                    upperTwoSigmaLimit:=limits.UpperTwoSigmaLimit,
                    effectiveSampleSize:=1.0 / varianceFactor,
                    sourceRowIndices:=raw.SourceRowIndices,
                    includedInParameterEstimation:=includedInEstimation,
                    includedInRuleEvaluation:=includedInRules,
                    exclusionScope:=raw.ExclusionScope,
                    exclusionReason:=raw.ExclusionReason,
                    signalRuleNumbers:=signalNumbers))

                If signalled Then
                    Dim side As SpcRuleSide = If(
                        ewma > limits.UpperControlLimit,
                        SpcRuleSide.UpperSideOnly,
                        SpcRuleSide.LowerSideOnly)
                    signals.Add(CreateIntrinsicSignal(
                        SpcPanelType.Ewma,
                        raw,
                        signalRule,
                        side,
                        "The EWMA statistic was outside its " &
                        FormatNumber(limitMultiplier) & "-sigma control limits."))
                End If

                If includedInRules Then
                    previousEwma = ewma
                    recursionAge = pointRecursionAge
                ElseIf options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    recursionAge = 0
                    hasState = False
                Else
                    ' Keep the pre-exclusion state and startup age.
                    previousEwma = priorEwma
                    recursionAge = priorRecursionAge
                End If
            Next

            Return New SpcPanelResult(
                SpcPanelType.Ewma,
                "EWMA",
                points.ToArray(),
                ResolveAxisTitle(request, "Exponentially weighted moving average"),
                BuildParameterEstimates(
                    request,
                    stages,
                    rawPoints,
                    cache,
                    SpcPanelType.Ewma,
                    lambda,
                    limitMultiplier,
                    If(steadyState, 1.0, 0.0)),
                signals.ToArray())
        End Function

#End Region

#Region "Moving average"

        Private Shared Function BuildMovingAveragePanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawTimePoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim span As Integer = GetMovingAverageSpan(request.ChartParameters)
            Dim limitMultiplier As Double = options.ControlLimits.SigmaMultiplier
            Dim steadyState As Boolean = request.ChartParameters.UseSteadyStateLimits
            Dim signalRule As SpcRuleDefinition = CreateLimitSignalRule(
                "MA-CL",
                MovingAverageSignalNumber,
                limitMultiplier,
                "Moving-average control limit")

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim window As New List(Of RawTimePoint)(span)
            Dim currentStageId As String = Nothing
            Dim finiteMovingAverages As Integer = 0

            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawTimePoint = rawPoints(i)
                Dim stageId As String = raw.Stage.Definition.StageId
                Dim stageChanged As Boolean = currentStageId Is Nothing OrElse
                    Not String.Equals(currentStageId,
                                      stageId,
                                      StringComparison.OrdinalIgnoreCase)
                If stageChanged Then
                    currentStageId = stageId
                    window.Clear()
                End If

                If raw.IsOmitted Then
                    If options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                        window.Clear()
                    End If
                    Continue For
                End If

                Dim ruleExcluded As Boolean = IsRuleEvaluationExcluded(raw)
                Dim calculationWindow As List(Of RawTimePoint)
                If ruleExcluded Then
                    ' Retain an excluded point for chart/audit display, but use a
                    ' temporary window so it cannot contaminate subsequent moving
                    ' averages.
                    calculationWindow = New List(Of RawTimePoint)(window)
                Else
                    calculationWindow = window
                End If
                calculationWindow.Add(raw)
                If calculationWindow.Count > span Then calculationWindow.RemoveAt(0)

                Dim parameters As StageParameters = ResolveParameters(raw.Stage, request, options, rawPoints, stages, cache, visiting)
                Dim hasValue As Boolean = Not steadyState OrElse calculationWindow.Count = span
                Dim value As Double = Double.NaN
                Dim standardError As Double = Double.NaN
                Dim effectiveSampleSize As Double = Double.NaN
                Dim limits As LimitValues = BuildMissingLimits(parameters.Center)

                If hasValue Then
                    value = AverageWindow(calculationWindow)
                    effectiveSampleSize = CDbl(calculationWindow.Count)
                    standardError = parameters.Sigma / Math.Sqrt(effectiveSampleSize)
                    limits = BuildSymmetricLimits(parameters.Center, standardError, limitMultiplier)
                    finiteMovingAverages += 1
                End If

                Dim windowScope As SpcExclusionScope = GetWindowExclusionScope(calculationWindow)
                Dim windowReason As String = GetWindowExclusionReason(calculationWindow)
                Dim includedInEstimation As Boolean = hasValue AndAlso raw.Stage.Definition.LimitMode =
                        SpcStageLimitMode.EstimateFromStageData AndAlso
                    (windowScope And SpcExclusionScope.ParameterEstimation) = SpcExclusionScope.None
                Dim includedInRules As Boolean = hasValue AndAlso (windowScope And SpcExclusionScope.RuleEvaluation) =
                        SpcExclusionScope.None
                Dim signalled As Boolean = includedInRules AndAlso
                    (value < limits.LowerControlLimit OrElse value > limits.UpperControlLimit)
                Dim signalNumbers As Integer() = Nothing
                If signalled Then signalNumbers = {MovingAverageSignalNumber}

                points.Add(New SpcPointResult(
                    raw.LogicalIndex,
                    value,
                    parameters.Center,
                    limits.LowerControlLimit,
                    limits.UpperControlLimit,
                    label:=raw.Label,
                    stageId:=stageId,
                    phase:=raw.Stage.Definition.Phase,
                    sequenceValue:=raw.SequenceValue,
                    standardError:=standardError,
                    standardizedValue:=Standardize(value, parameters.Center, standardError),
                    lowerOneSigmaLimit:=limits.LowerOneSigmaLimit,
                    upperOneSigmaLimit:=limits.UpperOneSigmaLimit,
                    lowerTwoSigmaLimit:=limits.LowerTwoSigmaLimit,
                    upperTwoSigmaLimit:=limits.UpperTwoSigmaLimit,
                    effectiveSampleSize:=effectiveSampleSize,
                    sourceRowIndices:=GetWindowSourceRows(calculationWindow),
                    includedInParameterEstimation:=includedInEstimation,
                    includedInRuleEvaluation:=includedInRules,
                    exclusionScope:=windowScope,
                    exclusionReason:=windowReason,
                    signalRuleNumbers:=signalNumbers))

                If signalled Then
                    Dim side As SpcRuleSide = If(
                        value > limits.UpperControlLimit,
                        SpcRuleSide.UpperSideOnly,
                        SpcRuleSide.LowerSideOnly)
                    signals.Add(CreateIntrinsicSignal(
                        SpcPanelType.MovingAverage,
                        raw,
                        signalRule,
                        side,
                        "The moving average was outside its " &
                        FormatNumber(limitMultiplier) & "-sigma control limits."))
                End If

                If ruleExcluded AndAlso options.Rules.GapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    window.Clear()
                End If
            Next

            If finiteMovingAverages = 0 Then
                Throw New ArgumentException(
                    "No complete moving-average window is available. " &
                    "Reduce the span, provide more observations per stage, " &
                    "or use dynamic startup limits.")
            End If

            If steadyState Then
                warnings.Add(
                    "Moving-average startup points with fewer than " &
                    span.ToString(CultureInfo.InvariantCulture) &
                    " observations are retained as undefined points because steady-state limits were requested.")
            End If

            Return New SpcPanelResult(
                SpcPanelType.MovingAverage,
                "Moving Average",
                points.ToArray(),
                ResolveAxisTitle(request, "Moving average"),
                BuildParameterEstimates(
                    request,
                    stages,
                    rawPoints,
                    cache,
                    SpcPanelType.MovingAverage,
                    CDbl(span),
                    limitMultiplier,
                    If(steadyState, 1.0, 0.0)),
                signals.ToArray())
        End Function

        Private Shared Function AverageWindow(window As List(Of RawTimePoint)) As Double
            Dim mean As Double = 0.0
            For i As Integer = 0 To window.Count - 1
                mean += (window(i).Value - mean) / CDbl(i + 1)
            Next
            Return mean
        End Function

        Private Shared Function GetWindowExclusionScope(
            window As List(Of RawTimePoint)) As SpcExclusionScope

            Dim scope As SpcExclusionScope = SpcExclusionScope.None
            For i As Integer = 0 To window.Count - 1
                scope = scope Or window(i).ExclusionScope
            Next
            Return scope
        End Function

        Private Shared Function GetWindowExclusionReason(
            window As List(Of RawTimePoint)) As String

            Dim reason As String = String.Empty
            For i As Integer = 0 To window.Count - 1
                reason = CombineReason(reason, window(i).ExclusionReason)
            Next
            Return reason
        End Function

        Private Shared Function GetWindowSourceRows(
            window As List(Of RawTimePoint)) As Integer()

            Dim values As New HashSet(Of Integer)()
            For i As Integer = 0 To window.Count - 1
                Dim sourceRows As Integer() = window(i).SourceRowIndices
                For j As Integer = 0 To sourceRows.Length - 1
                    values.Add(sourceRows(j))
                Next
            Next
            Dim result As Integer() = New List(Of Integer)(values).ToArray()
            Array.Sort(result)
            Return result
        End Function

#End Region

#Region "Results and shared calculations"

        Private Shared Function BuildParameterEstimates(
            request As SpcFitRequest,
            stages As StageContext(),
            rawPoints As RawTimePoint(),
            cache As Dictionary(Of String, StageParameters),
            panelType As SpcPanelType,
            designValueOne As Double,
            designValueTwo As Double,
            designValueThree As Double) As SpcParameterEstimate()

            Dim result As New List(Of SpcParameterEstimate)()
            For i As Integer = 0 To stages.Length - 1
                Dim stage As StageContext = stages(i)
                If Not StageHasRetainedPoint(stage, rawPoints) Then Continue For

                Dim parameters As StageParameters = Nothing
                If Not cache.TryGetValue(stage.Definition.StageId, parameters) Then
                    Throw New InvalidOperationException(
                        "Calculated stage parameters were not retained in the stage cache.")
                End If

                result.Add(New SpcParameterEstimate(
                    stage.Definition.StageId,
                    panelType,
                    "ProcessMean",
                    parameters.Center,
                    parameters.LimitMode,
                    standardError:=ToNullableFinite(parameters.CenterStandardError),
                    sourceStageId:=parameters.SourceStageId,
                    method:=parameters.Method,
                    displayName:="Process mean",
                    sampleCount:=ToNullableCount(parameters.EstimationPointCount)))
                result.Add(New SpcParameterEstimate(
                    stage.Definition.StageId,
                    panelType,
                    "ProcessSigma",
                    parameters.Sigma,
                    parameters.LimitMode,
                    sourceStageId:=parameters.SourceStageId,
                    method:=parameters.Method,
                    displayName:="Within-process sigma",
                    sampleCount:=ToNullableCount(parameters.SigmaEstimationPointCount)))

                Select Case request.ChartType
                    Case SpcChartType.Cusum
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "CusumReferenceValueK",
                                           "CUSUM reference value k (sigma units)",
                                           designValueOne)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "CusumDecisionIntervalH",
                                           "CUSUM decision interval h (sigma units)",
                                           designValueTwo)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "HeadStart",
                                           "CUSUM head start (sigma units)",
                                           designValueThree)

                    Case SpcChartType.Ewma
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "EwmaLambda",
                                           "EWMA lambda",
                                           designValueOne)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "LimitSigmaMultiplier",
                                           "Control-limit sigma multiplier",
                                           designValueTwo)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "SteadyStateLimits",
                                           "Steady-state limits (1=yes)",
                                           designValueThree)

                    Case SpcChartType.MovingAverage
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "MovingAverageSpan",
                                           "Moving-average span",
                                           designValueOne)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "LimitSigmaMultiplier",
                                           "Control-limit sigma multiplier",
                                           designValueTwo)
                        AddDesignParameter(result,
                                           stage,
                                           panelType,
                                           parameters,
                                           "SteadyStateLimits",
                                           "Steady-state limits (1=yes)",
                                           designValueThree)
                End Select
            Next
            Return result.ToArray()
        End Function

        Private Shared Sub AddDesignParameter(
            values As List(Of SpcParameterEstimate),
            stage As StageContext,
            panelType As SpcPanelType,
            parameters As StageParameters,
            parameterName As String,
            displayName As String,
            value As Double)

            values.Add(New SpcParameterEstimate(
                stage.Definition.StageId,
                panelType,
                parameterName,
                value,
                parameters.LimitMode,
                sourceStageId:=parameters.SourceStageId,
                method:="Chart design parameter",
                displayName:=displayName))
        End Sub

        Private Shared Function CreateLimitSignalRule(
            ruleCode As String,
            ruleNumber As Integer,
            sigmaMultiplier As Double,
            displayName As String) As SpcRuleDefinition

            Return New SpcRuleDefinition(
                ruleCode,
                ruleNumber,
                SpcRuleKind.BeyondSigma,
                1,
                1,
                sigmaMultiplier,
                side:=SpcRuleSide.EitherSide,
                scope:=SpcRuleScope.TimeWeightedPanels,
                displayName:=displayName,
                description:="The time-weighted statistic was outside its control limits.")
        End Function

        Private Shared Function CreateIntrinsicSignal(
            panelType As SpcPanelType,
            raw As RawTimePoint,
            rule As SpcRuleDefinition,
            side As SpcRuleSide,
            message As String) As SpcSignalResult

            Return New SpcSignalResult(
                panelType,
                raw.Stage.Definition.StageId,
                rule,
                raw.LogicalIndex,
                raw.LogicalIndex,
                raw.LogicalIndex,
                triggeredSide:=side,
                contributingPointIndices:={raw.LogicalIndex},
                markedPointIndices:={raw.LogicalIndex},
                message:=message)
        End Function

        Private Shared Function BuildSymmetricLimits(
            center As Double,
            standardError As Double,
            sigmaMultiplier As Double) As LimitValues

            Return New LimitValues With {
                .LowerControlLimit = center - sigmaMultiplier * standardError,
                .UpperControlLimit = center + sigmaMultiplier * standardError,
                .LowerOneSigmaLimit = If(sigmaMultiplier >= 1.0,
                                         center - standardError,
                                         Double.NaN),
                .UpperOneSigmaLimit = If(sigmaMultiplier >= 1.0,
                                         center + standardError,
                                         Double.NaN),
                .LowerTwoSigmaLimit = If(sigmaMultiplier >= 2.0,
                                         center - 2.0 * standardError,
                                         Double.NaN),
                .UpperTwoSigmaLimit = If(sigmaMultiplier >= 2.0,
                                         center + 2.0 * standardError,
                                         Double.NaN)
            }
        End Function

        Private Shared Function BuildMissingLimits(center As Double) As LimitValues
            Return New LimitValues With {
                .LowerControlLimit = center,
                .UpperControlLimit = center,
                .LowerOneSigmaLimit = Double.NaN,
                .UpperOneSigmaLimit = Double.NaN,
                .LowerTwoSigmaLimit = Double.NaN,
                .UpperTwoSigmaLimit = Double.NaN
            }
        End Function

        Private Shared Function Standardize(value As Double,
                                            center As Double,
                                            standardError As Double) As Double
            If Double.IsNaN(value) OrElse Double.IsNaN(standardError) Then
                Return Double.NaN
            End If
            If standardError > 0.0 Then Return (value - center) / standardError
            If value = center Then Return 0.0
            Return Double.NaN
        End Function

        Private Shared Function GetEwmaLambda(parameters As SpcChartParameters) As Double
            Return If(parameters.EwmaLambda.HasValue,
                      parameters.EwmaLambda.Value,
                      DefaultEwmaLambda)
        End Function

        Private Shared Function GetCusumReferenceValue(
            parameters As SpcChartParameters) As Double

            Return If(parameters.CusumReferenceValue.HasValue,
                      parameters.CusumReferenceValue.Value,
                      DefaultCusumReferenceValue)
        End Function

        Private Shared Function GetCusumDecisionInterval(
            parameters As SpcChartParameters) As Double

            Return If(parameters.CusumDecisionInterval.HasValue,
                      parameters.CusumDecisionInterval.Value,
                      DefaultCusumDecisionInterval)
        End Function

        Private Shared Function GetHeadStart(parameters As SpcChartParameters) As Double
            Return If(parameters.HeadStart.HasValue,
                      parameters.HeadStart.Value,
                      DefaultHeadStart)
        End Function

        Private Shared Function GetMovingAverageSpan(
            parameters As SpcChartParameters) As Integer

            Return If(parameters.MovingAverageSpan.HasValue,
                      parameters.MovingAverageSpan.Value,
                      DefaultMovingAverageSpan)
        End Function

        Private Shared Function StageHasRetainedPoint(
            stage As StageContext,
            points As RawTimePoint()) As Boolean

            For i As Integer = stage.Definition.FirstPointIndex To stage.Definition.LastPointIndex
                If Not points(i).IsOmitted Then Return True
            Next
            Return False
        End Function

        Private Shared Function CountRetainedPoints(points As RawTimePoint()) As Integer
            Dim count As Integer = 0
            For i As Integer = 0 To points.Length - 1
                If Not points(i).IsOmitted Then count += 1
            Next
            Return count
        End Function

        Private Shared Function CombineReason(existingReason As String,
                                              additionalReason As String) As String
            Dim normalized As String = If(additionalReason, String.Empty).Trim()
            If normalized.Length = 0 Then Return If(existingReason, String.Empty)
            If String.IsNullOrWhiteSpace(existingReason) Then Return normalized
            If existingReason.IndexOf(normalized,
                                      StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return existingReason
            End If
            Return existingReason & "; " & normalized
        End Function

        Private Shared Function ToNullableFinite(value As Double) As Nullable(Of Double)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return Nothing
            Return value
        End Function

        Private Shared Function ToNullableCount(value As Integer) As Nullable(Of Integer)
            If value < 0 Then Return Nothing
            Return value
        End Function

        Private Shared Function ResolveAxisTitle(request As SpcFitRequest,
                                                fallback As String) As String
            If request.ValueAxisTitle.Length > 0 Then Return request.ValueAxisTitle
            Return fallback
        End Function

        Private Shared Function FormatNumber(value As Double) As String
            Return value.ToString("0.###", CultureInfo.InvariantCulture)
        End Function

        Private Shared Sub CheckCancellation(cancellationRequested As Func(Of Boolean))
            SpcEngine.ThrowIfCancellationRequested(cancellationRequested)
        End Sub

        Private Shared Sub CheckCancellationPeriodically(
            index As Integer,
            cancellationRequested As Func(Of Boolean))
            If (index And 127) = 0 Then CheckCancellation(cancellationRequested)
        End Sub

#End Region

#Region "Private data holders"

        Private NotInheritable Class RawTimePoint
            Public Property LogicalIndex As Integer
            Public Property Value As Double = Double.NaN
            Public Property Label As String = String.Empty
            Public Property SequenceValue As Nullable(Of Double)
            Public Property SourceRowIndices As Integer() = Array.Empty(Of Integer)()
            Public Property IsOmitted As Boolean
            Public Property Stage As StageContext
            Public Property ExclusionScope As SpcExclusionScope = SpcExclusionScope.None
            Public Property ExclusionReason As String = String.Empty
        End Class

        Private NotInheritable Class StageContext
            Public Sub New(definition As SpcStageDefinition)
                Me.Definition = definition
            End Sub

            Public ReadOnly Property Definition As SpcStageDefinition
        End Class

        Private NotInheritable Class StageParameters
            Public Property Center As Double
            Public Property Sigma As Double
            Public Property CenterStandardError As Double
            Public Property EstimationPointCount As Integer
            Public Property SigmaEstimationPointCount As Integer
            Public Property LimitMode As SpcStageLimitMode
            Public Property SourceStageId As String = String.Empty
            Public Property Method As String = String.Empty

            Public Function CopyForReference(referenceStageId As String) As StageParameters
                Return New StageParameters With {
                    .Center = Center,
                    .Sigma = Sigma,
                    .CenterStandardError = CenterStandardError,
                    .EstimationPointCount = EstimationPointCount,
                    .SigmaEstimationPointCount = SigmaEstimationPointCount,
                    .LimitMode = SpcStageLimitMode.UseReferenceStage,
                    .SourceStageId = referenceStageId,
                    .Method = "Reference stage " & referenceStageId & ": " & Method
                }
            End Function
        End Class

        Private Structure LimitValues
            Public LowerControlLimit As Double
            Public UpperControlLimit As Double
            Public LowerOneSigmaLimit As Double
            Public UpperOneSigmaLimit As Double
            Public LowerTwoSigmaLimit As Double
            Public UpperTwoSigmaLimit As Double
        End Structure

#End Region

    End Class

End Namespace
