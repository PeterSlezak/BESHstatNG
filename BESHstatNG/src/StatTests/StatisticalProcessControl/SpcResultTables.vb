Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Builds host-neutral <see cref="Global.BESHStatNG.ResultTable"/> output from an
    ''' immutable <see cref="SpcFitResult"/> snapshot.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class contains no Excel-DNA, Excel Interop, WinForms, worksheet, or chart
    ''' references. The same tables can therefore be written by the existing
    ''' Excel-DNA writer, returned by worksheet functions, inspected by tests, or
    ''' consumed by a future Office.js writer.
    ''' </para>
    ''' <para>
    ''' Internal point indices are zero-based. Human-facing point numbers in the
    ''' generated tables are one-based; the audit columns retain both representations.
    ''' Source-row identifiers are emitted exactly as supplied to <see cref="SpcInputData"/>.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SpcResultTables

        Private Sub New()
        End Sub

#Region "Grouped output"

        ''' <summary>
        ''' Returns the compact result tables intended for an SPC summary sheet.
        ''' </summary>
        Public Shared Function BuildSummaryTables(
            result As SpcFitResult) As List(Of Global.BESHStatNG.ResultTable)

            RequireResult(result)

            Return New List(Of Global.BESHStatNG.ResultTable) From {
                BuildRunSummaryTable(result),
                BuildPanelSummaryTable(result),
                BuildParameterEstimatesTable(result),
                BuildWarningsTable(result)
            }
        End Function

        ''' <summary>
        ''' Returns chart-point tables for a chart-data sheet.
        ''' </summary>
        ''' <param name="separatePanels">
        ''' When false, one combined table is returned. When true, one table is
        ''' returned for each panel in result order.
        ''' </param>
        Public Shared Function BuildChartDataTables(
            result As SpcFitResult,
            Optional separatePanels As Boolean = False) As List(Of Global.BESHStatNG.ResultTable)

            RequireResult(result)

            Dim tables As New List(Of Global.BESHStatNG.ResultTable)()
            If Not separatePanels Then
                tables.Add(BuildPointDataTable(result))
                Return tables
            End If

            Dim panels As SpcPanelResult() = result.Panels
            For i As Integer = 0 To panels.Length - 1
                tables.Add(BuildPointDataTable(panels(i)))
            Next
            Return tables
        End Function

        ''' <summary>
        ''' Returns signal occurrences and the effective rule definitions.
        ''' </summary>
        Public Shared Function BuildSignalTables(
            result As SpcFitResult) As List(Of Global.BESHStatNG.ResultTable)

            RequireResult(result)

            Return New List(Of Global.BESHStatNG.ResultTable) From {
                BuildSignalsTable(result),
                BuildRuleDefinitionsTable(result)
            }
        End Function

        ''' <summary>
        ''' Returns request settings, phase/stage, exclusion, historical-parameter,
        ''' specification-limit, and execution-audit tables.
        ''' </summary>
        Public Shared Function BuildAuditTables(
            result As SpcFitResult) As List(Of Global.BESHStatNG.ResultTable)

            RequireResult(result)

            Return New List(Of Global.BESHStatNG.ResultTable) From {
                BuildSettingsTable(result),
                BuildStagesTable(result),
                BuildExclusionsTable(result),
                BuildHistoricalParametersTable(result),
                BuildSpecificationLimitsTable(result),
                BuildExecutionAuditTable(result)
            }
        End Function

#End Region

#Region "Summary tables"

        ''' <summary>Builds a compact analysis-level run summary.</summary>
        Public Shared Function BuildRunSummaryTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim request As SpcFitRequest = result.Request
            Dim data As SpcInputData = request.Data
            Dim ruleOptions As SpcRuleOptions = request.AnalysisOptions.Rules
            Dim warnings As List(Of Object()) = CollectWarningRows(result)

            Dim rows As New List(Of Object()) From {
                Row("Request label", request.RequestLabel),
                Row("Chart title", request.ChartTitle),
                Row("Chart type", ChartTypeText(result.ChartType)),
                Row("Chart family", ChartFamilyText(result.ChartFamily)),
                Row("Data layout", DataLayoutText(result.DataLayout)),
                Row("Input rows", data.RowCount),
                Row("Measurement columns", data.MeasurementColumnCount),
                Row("Panels", result.PanelCount),
                Row("Distinct chart points", result.ChartPointCount),
                Row("Panel-point rows", result.PanelPointCount),
                Row("Parameter estimates", result.ParameterEstimates.Length),
                Row("Signal occurrences", result.SignalCount),
                Row("Signalled panel points", result.SignalledPanelPointCount),
                Row("Selected rule preset", RulePresetText(ruleOptions.Preset)),
                Row("Process status", If(result.IsInControlBySelectedRules,
                                          "No selected-rule signals detected",
                                          "Special-cause signal(s) detected")),
                Row("Warnings", warnings.Count),
                Row("Execution time (ms)", result.ExecutionTimeMilliseconds),
                Row("Execution started (UTC)", NullableUtcText(result.ExecutionStartedUtc)),
                Row("Execution completed (UTC)", NullableUtcText(result.ExecutionCompletedUtc))
            }

            Return CreateTable(
                "SPC analysis - run summary",
                {"Item", "Value"},
                rows,
                footnote:="Process status reflects only the signal rules selected for this analysis; absence of a signal does not prove that the process is stable.")
        End Function

        ''' <summary>Builds one summary row for every result panel.</summary>
        Public Shared Function BuildPanelSummaryTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim rows As New List(Of Object())()
            Dim panels As SpcPanelResult() = result.Panels

            For i As Integer = 0 To panels.Length - 1
                Dim panel As SpcPanelResult = panels(i)
                Dim points As SpcPointResult() = panel.Points
                Dim finiteCount As Integer = 0
                Dim estimationCount As Integer = 0
                Dim ruleCount As Integer = 0
                Dim exclusionCount As Integer = 0

                For j As Integer = 0 To points.Length - 1
                    If points(j).HasFiniteValue Then finiteCount += 1
                    If points(j).IncludedInParameterEstimation Then estimationCount += 1
                    If points(j).IncludedInRuleEvaluation Then ruleCount += 1
                    If points(j).IsExplicitlyExcluded Then exclusionCount += 1
                Next

                rows.Add(Row(panel.DisplayName,
                             PanelTypeText(panel.PanelType),
                             panel.ValueAxisTitle,
                             panel.PointCount,
                             finiteCount,
                             panel.PointCount - finiteCount,
                             estimationCount,
                             ruleCount,
                             exclusionCount,
                             panel.SignalCount,
                             panel.SignalledPointCount,
                             panel.ParameterEstimates.Length,
                             panel.Warnings.Length))
            Next

            Return CreateTable(
                "SPC analysis - panel summary",
                {"Panel", "Panel Type", "Value Axis", "Points", "Finite Points",
                 "Missing Points", "Used to Estimate", "Rule-Eligible Points", "Excluded Points",
                 "Signal Occurrences", "Signalled Points", "Parameters", "Warnings"},
                rows)
        End Function

        ''' <summary>Builds the complete retained parameter-estimate table.</summary>
        Public Shared Function BuildParameterEstimatesTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim rows As New List(Of Object())()
            Dim estimates As SpcParameterEstimate() = result.ParameterEstimates

            For i As Integer = 0 To estimates.Length - 1
                Dim estimate As SpcParameterEstimate = estimates(i)
                rows.Add(Row(estimate.StageId,
                             PanelTypeText(estimate.PanelType),
                             estimate.DisplayName,
                             estimate.ParameterName,
                             estimate.Value,
                             NullableDoubleValue(estimate.StandardError),
                             StageLimitModeText(estimate.LimitMode),
                             estimate.SourceStageId,
                             estimate.Method,
                             NullableIntegerValue(estimate.SampleCount)))
            Next

            Return CreateTable(
                "SPC analysis - parameter estimates",
                {"Stage", "Panel", "Parameter", "Parameter Name", "Estimate", "Std. Error",
                 "Limit Mode", "Source Stage", "Method", "Estimation Points"},
                rows,
                emptyMessage:="No parameter estimates were retained for this chart.",
                footnote:="Reference-stage and historical parameters are repeated for each stage and panel to preserve the exact limit-construction audit trail.")
        End Function

        ''' <summary>Builds fit-level and panel-level warnings.</summary>
        Public Shared Function BuildWarningsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Return CreateTable(
                "SPC warnings",
                {"Scope", "Panel", "Message"},
                CollectWarningRows(result),
                emptyMessage:="No warnings were generated.")
        End Function

#End Region

#Region "Point and signal tables"

        ''' <summary>
        ''' Builds one combined chart-data table containing every panel-point row.
        ''' </summary>
        Public Shared Function BuildPointDataTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim rows As New List(Of Object())()
            Dim panels As SpcPanelResult() = result.Panels
            For i As Integer = 0 To panels.Length - 1
                AddPointRows(rows, panels(i), includePanelColumns:=True)
            Next

            Return CreateTable(
                "SPC chart data",
                PointHeaders(includePanelColumns:=True),
                rows,
                emptyMessage:="No chart points are available.",
                footnote:="Control limits are statistical process limits. Specification limits, when supplied, are reported separately in the audit output.")
        End Function

        ''' <summary>Builds chart data for one panel.</summary>
        Public Shared Function BuildPointDataTable(
            panel As SpcPanelResult) As Global.BESHStatNG.ResultTable

            If panel Is Nothing Then Throw New ArgumentNullException(NameOf(panel))

            Dim rows As New List(Of Object())()
            AddPointRows(rows, panel, includePanelColumns:=False)

            Return CreateTable(
                "SPC chart data - " & panel.DisplayName,
                PointHeaders(includePanelColumns:=False),
                rows,
                emptyMessage:="No chart points are available.",
                footnote:="Point is one-based for display; Point Index is the zero-based identifier retained by the calculation engine.")
        End Function

        ''' <summary>Builds one row for every detected rule occurrence.</summary>
        Public Shared Function BuildSignalsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim rows As New List(Of Object())()
            Dim signals As SpcSignalResult() = result.Signals

            For i As Integer = 0 To signals.Length - 1
                Dim signal As SpcSignalResult = signals(i)
                Dim panel As SpcPanelResult = result.GetPanel(signal.PanelType)
                Dim terminal As SpcPointResult = Nothing
                If panel IsNot Nothing Then terminal = panel.GetPoint(signal.TerminalPointIndex)

                rows.Add(Row(PanelTypeText(signal.PanelType),
                             signal.StageId,
                             signal.RuleNumber,
                             signal.RuleCode,
                             signal.Rule.DisplayName,
                             RuleKindText(signal.Rule.Kind),
                             RuleSideText(signal.TriggeredSide),
                             signal.TerminalPointIndex + 1,
                             If(terminal Is Nothing, String.Empty, terminal.Label),
                             If(terminal Is Nothing,
                                String.Empty,
                                IntegerListText(terminal.SourceRowIndices, addOne:=False)),
                             If(terminal Is Nothing, CType(Double.NaN, Object), terminal.Value),
                             If(terminal Is Nothing,
                                CType(Double.NaN, Object),
                                terminal.StandardizedValue),
                             signal.WindowStartPointIndex + 1,
                             signal.WindowEndPointIndex + 1,
                             IntegerListText(signal.ContributingPointIndices, addOne:=True),
                             IntegerListText(signal.MarkedPointIndices, addOne:=True),
                             signal.Message))
            Next

            Return CreateTable(
                "SPC special-cause signals",
                {"Panel", "Stage", "Rule", "Rule Code", "Rule Name", "Pattern", "Side",
                 "Terminal Point", "Terminal Label", "Terminal Source Row(s)", "Terminal Value",
                 "Terminal Standardized Value", "Window Start", "Window End",
                 "Contributing Point(s)", "Marked Point(s)", "Message"},
                rows,
                emptyMessage:="No special-cause signals were detected by the selected rules.",
                footnote:="Each row is one rule occurrence. A point can participate in overlapping occurrences and can violate more than one rule.")
        End Function

        ''' <summary>Builds the effective selected special-cause rule definitions.</summary>
        Public Shared Function BuildRuleDefinitionsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim options As SpcRuleOptions = result.Request.AnalysisOptions.Rules
            Dim definitions As SpcRuleDefinition() = SpcRuleCatalog.ResolveRules(options)
            Dim rows As New List(Of Object())()

            For i As Integer = 0 To definitions.Length - 1
                Dim rule As SpcRuleDefinition = definitions(i)
                rows.Add(Row(rule.RuleNumber,
                             rule.RuleCode,
                             rule.DisplayName,
                             RuleKindText(rule.Kind),
                             rule.WindowSize,
                             rule.MinimumPoints,
                             rule.SigmaThreshold,
                             RuleSideText(rule.Side),
                             RuleScopeText(rule.Scope),
                             rule.Description))
            Next

            Dim presetDescription As String = SpcRuleCatalog.GetPresetDescription(options.Preset)
            Dim sequenceDescription As String =
                "Phase scope: " & RulePhaseScopeText(options.PhaseScope) &
                "; gap behavior: " & SequenceGapText(options.GapBehavior) &
                "; marking: " & SignalMarkingText(options.MarkingMode) & "."

            Return CreateTable(
                "SPC selected signal rules - " & RulePresetText(options.Preset),
                {"Rule", "Code", "Name", "Pattern", "Window", "Minimum Points",
                 "Sigma Threshold", "Side", "Applicable Panels", "Description"},
                rows,
                emptyMessage:="Special-cause rule evaluation was disabled.",
                footnote:=presetDescription & " " & sequenceDescription)
        End Function

#End Region

#Region "Settings and audit tables"

        ''' <summary>Builds a key/value snapshot of all shared and chart-specific settings.</summary>
        Public Shared Function BuildSettingsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim request As SpcFitRequest = result.Request
            Dim options As SpcAnalysisOptions = request.AnalysisOptions
            Dim limits As SpcControlLimitOptions = options.ControlLimits
            Dim rules As SpcRuleOptions = options.Rules
            Dim chartParameters As SpcChartParameters = request.ChartParameters
            Dim data As SpcInputData = request.Data

            Dim rows As New List(Of Object()) From {
                Row("Request label", request.RequestLabel),
                Row("Chart title", request.ChartTitle),
                Row("Value-axis title", request.ValueAxisTitle),
                Row("Chart type", ChartTypeText(request.ChartType)),
                Row("Chart family", ChartFamilyText(result.ChartFamily)),
                Row("Data layout", DataLayoutText(request.DataLayout)),
                Row("Missing-value policy", MissingValuePolicyText(options.MissingValuePolicy)),
                Row("Parameter source", ParameterSourceText(limits.ParameterSource)),
                Row("Control-limit method", ControlLimitMethodText(limits.Method)),
                Row("Sigma multiplier", limits.SigmaMultiplier),
                Row("Within-sigma estimator", SigmaEstimatorText(limits.WithinSigmaEstimator)),
                Row("Natural-limit policy", NaturalLimitPolicyText(limits.NaturalLimitPolicy)),
                Row("Moving-range length", limits.MovingRangeLength),
                Row("Bias correction", YesNo(limits.UseBiasCorrection)),
                Row("Rule preset", RulePresetText(rules.Preset)),
                Row("Rule phase scope", RulePhaseScopeText(rules.PhaseScope)),
                Row("Sequence-gap behavior", SequenceGapText(rules.GapBehavior)),
                Row("Signal marking", SignalMarkingText(rules.MarkingMode)),
                Row("Input rows", data.RowCount),
                Row("Measurement columns", data.MeasurementColumnCount),
                Row("Count data supplied", YesNo(data.Counts IsNot Nothing)),
                Row("Sample sizes supplied", YesNo(data.SampleSizes IsNot Nothing)),
                Row("Exposures supplied", YesNo(data.Exposures IsNot Nothing)),
                Row("Subgroup IDs supplied", YesNo(data.SubgroupIds IsNot Nothing)),
                Row("Labels supplied", YesNo(data.Labels IsNot Nothing)),
                Row("Sequence values supplied", YesNo(data.SequenceValues IsNot Nothing)),
                Row("Configured stages", request.Stages.Length),
                Row("Configured exclusions", request.Exclusions.Length),
                Row("Historical-parameter sets", request.HistoricalParameters.Length),
                Row("Specification values supplied", YesNo(request.SpecificationLimits.HasAnyValue)),
                Row("EWMA lambda", NullableDoubleValue(chartParameters.EwmaLambda)),
                Row("CUSUM reference value", NullableDoubleValue(chartParameters.CusumReferenceValue)),
                Row("CUSUM decision interval", NullableDoubleValue(chartParameters.CusumDecisionInterval)),
                Row("Head start", NullableDoubleValue(chartParameters.HeadStart)),
                Row("Moving-average span", NullableIntegerValue(chartParameters.MovingAverageSpan)),
                Row("Steady-state limits", YesNo(chartParameters.UseSteadyStateLimits))
            }

            Return CreateTable(
                "SPC analysis settings",
                {"Setting", "Value"},
                rows)
        End Function

        ''' <summary>Builds effective phase/stage definitions and retained-point counts.</summary>
        Public Shared Function BuildStagesTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim stages As List(Of EffectiveStage) = GetEffectiveStages(result)
            Dim rows As New List(Of Object())()

            For i As Integer = 0 To stages.Count - 1
                Dim stage As EffectiveStage = stages(i)
                rows.Add(Row(stage.StageId,
                             stage.DisplayName,
                             stage.FirstPointIndex + 1,
                             stage.LastPointIndex + 1,
                             stage.LastPointIndex - stage.FirstPointIndex + 1,
                             PhaseText(stage.Phase),
                             StageLimitModeText(stage.LimitMode),
                             stage.ReferenceStageId,
                             YesNo(stage.DefinedInRequest),
                             CountDistinctStagePoints(result, stage.StageId, PointCountMode.All),
                             CountDistinctStagePoints(result, stage.StageId, PointCountMode.Estimation),
                             CountDistinctStagePoints(result, stage.StageId, PointCountMode.Rules),
                             CountStageSignals(result, stage.StageId)))
            Next

            Return CreateTable(
                "SPC stages and phases",
                {"Stage", "Display Name", "First Point", "Last Point", "Defined Point Count",
                 "Phase", "Limit Mode", "Reference Stage", "Defined in Request",
                 "Retained Chart Points", "Estimation-Eligible Points",
                 "Rule-Eligible Points", "Signal Occurrences"},
                rows,
                emptyMessage:="No stage information is available.",
                footnote:="Rule sequences reset at stage boundaries. Phase-II and reference-stage limits remain frozen according to the recorded limit mode.")
        End Function

        ''' <summary>Builds configured exclusions and their retained result mapping.</summary>
        Public Shared Function BuildExclusionsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim exclusions As SpcExclusionDefinition() = result.Request.Exclusions
            Dim rows As New List(Of Object())()

            For i As Integer = 0 To exclusions.Length - 1
                Dim exclusion As SpcExclusionDefinition = exclusions(i)
                Dim matching As List(Of SpcPointResult) =
                    GetPointsAtIndex(result, exclusion.PointIndex)

                rows.Add(Row(exclusion.PointIndex + 1,
                             exclusion.PointIndex,
                             JoinDistinctPointText(matching, PointTextField.Label),
                             JoinDistinctPointText(matching, PointTextField.Stage),
                             JoinDistinctSourceRows(matching),
                             ExclusionScopeText(exclusion.Scope),
                             exclusion.Reason,
                             YesNo(matching.Count > 0),
                             JoinDistinctPointText(matching, PointTextField.EstimationIncluded),
                             JoinDistinctPointText(matching, PointTextField.RuleIncluded)))
            Next

            Return CreateTable(
                "SPC exclusions",
                {"Point", "Point Index (0-based)", "Label", "Stage", "Source Row(s)",
                 "Exclusion Scope", "Reason", "Retained in Result", "Used to Estimate",
                 "Rule-Evaluation Eligible"},
                rows,
                emptyMessage:="No explicit exclusions were configured.",
                footnote:="Exclusions remain visible in the audit trail. They control participation in parameter estimation and/or rule sequences; they do not delete the plotted point.")
        End Function

        ''' <summary>Builds supplied historical process parameters.</summary>
        Public Shared Function BuildHistoricalParametersTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim history As SpcHistoricalParameters() = result.Request.HistoricalParameters
            Dim rows As New List(Of Object())()

            For i As Integer = 0 To history.Length - 1
                Dim item As SpcHistoricalParameters = history(i)
                rows.Add(Row(If(item.AppliesToAllStages, "All stages", item.StageId),
                             YesNo(item.AppliesToAllStages),
                             NullableDoubleValue(item.ProcessMean),
                             NullableDoubleValue(item.ProcessSigma),
                             NullableDoubleValue(item.NonconformingProportion),
                             NullableDoubleValue(item.MeanDefectCount),
                             NullableDoubleValue(item.MeanDefectRate),
                             NullableDoubleValue(item.LaneySigmaZ)))
            Next

            Return CreateTable(
                "SPC historical parameters",
                {"Stage", "Default for All Stages", "Process Mean", "Process Sigma",
                 "Nonconforming Proportion", "Mean Defect Count", "Mean Defect Rate",
                 "Laney Sigma Z"},
                rows,
                emptyMessage:="No historical process parameters were supplied.")
        End Function

        ''' <summary>Builds specification limits separately from control limits.</summary>
        Public Shared Function BuildSpecificationLimitsTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim specifications As SpcSpecificationLimits = result.Request.SpecificationLimits
            Dim rows As New List(Of Object())()

            If specifications.LowerSpecificationLimit.HasValue Then
                rows.Add(Row("Lower specification limit", specifications.LowerSpecificationLimit.Value))
            End If
            If specifications.Target.HasValue Then
                rows.Add(Row("Target", specifications.Target.Value))
            End If
            If specifications.UpperSpecificationLimit.HasValue Then
                rows.Add(Row("Upper specification limit", specifications.UpperSpecificationLimit.Value))
            End If

            Return CreateTable(
                "SPC specification limits",
                {"Specification", "Value"},
                rows,
                emptyMessage:="No specification limits or target were supplied.",
                footnote:="Specification limits describe requirements and must not be interpreted as statistically estimated control limits.")
        End Function

        ''' <summary>Builds calculation timing and result-size audit metadata.</summary>
        Public Shared Function BuildExecutionAuditTable(
            result As SpcFitResult) As Global.BESHStatNG.ResultTable

            RequireResult(result)

            Dim rows As New List(Of Object()) From {
                Row("Request label", result.Request.RequestLabel),
                Row("Execution started (UTC)", NullableUtcText(result.ExecutionStartedUtc)),
                Row("Execution completed (UTC)", NullableUtcText(result.ExecutionCompletedUtc)),
                Row("Execution time (ms)", result.ExecutionTimeMilliseconds),
                Row("Input rows", result.Request.Data.RowCount),
                Row("Distinct chart points", result.ChartPointCount),
                Row("Panel-point rows", result.PanelPointCount),
                Row("Panels", result.PanelCount),
                Row("Parameter estimates", result.ParameterEstimates.Length),
                Row("Signal occurrences", result.SignalCount),
                Row("Warnings", CollectWarningRows(result).Count)
            }

            Return CreateTable(
                "SPC execution audit",
                {"Audit Item", "Value"},
                rows,
                footnote:="This table describes one internally consistent result snapshot; recalculation produces a new snapshot rather than partially updating worksheet formulas and flags.")
        End Function

#End Region

#Region "Table construction helpers"

        Private Shared Function CreateTable(
            title As String,
            headers As String(),
            rows As IList(Of Object()),
            Optional emptyMessage As String = Nothing,
            Optional footnote As String = Nothing) As Global.BESHStatNG.ResultTable

            If headers Is Nothing OrElse headers.Length = 0 Then
                Throw New ArgumentException("At least one table header is required.", NameOf(headers))
            End If
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))

            Dim table As New Global.BESHStatNG.ResultTable()
            If Not String.IsNullOrWhiteSpace(title) Then table.AddTitle(title.Trim())
            table.AddHeaderTopRow(CType(headers.Clone(), String()))

            If rows.Count > 0 Then
                Dim body(rows.Count - 1, headers.Length - 1) As Object
                For r As Integer = 0 To rows.Count - 1
                    Dim source As Object() = rows(r)
                    If source Is Nothing OrElse source.Length <> headers.Length Then
                        Throw New InvalidOperationException(
                            "Every result-table row must match the header column count.")
                    End If
                    For c As Integer = 0 To headers.Length - 1
                        body(r, c) = source(c)
                    Next
                Next
                table.SetBody(body)
            ElseIf Not String.IsNullOrWhiteSpace(emptyMessage) Then
                table.AddFootnote(emptyMessage.Trim())
            End If

            If Not String.IsNullOrWhiteSpace(footnote) Then table.AddFootnote(footnote.Trim())
            Return table
        End Function

        Private Shared Function Row(ParamArray values As Object()) As Object()
            Return values
        End Function

        Private Shared Sub AddPointRows(rows As List(Of Object()),
                                        panel As SpcPanelResult,
                                        includePanelColumns As Boolean)
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
            If panel Is Nothing Then Throw New ArgumentNullException(NameOf(panel))

            Dim points As SpcPointResult() = panel.Points
            For i As Integer = 0 To points.Length - 1
                Dim point As SpcPointResult = points(i)
                Dim values As New List(Of Object)()

                If includePanelColumns Then
                    values.Add(panel.DisplayName)
                    values.Add(PanelTypeText(panel.PanelType))
                End If

                values.Add(point.PointIndex + 1)
                values.Add(point.PointIndex)
                values.Add(IntegerListText(point.SourceRowIndices, addOne:=False))
                values.Add(point.Label)
                values.Add(NullableDoubleValue(point.SequenceValue))
                values.Add(point.StageId)
                values.Add(PhaseText(point.Phase))
                values.Add(point.Value)
                values.Add(point.CenterLine)
                values.Add(point.StandardError)
                values.Add(point.StandardizedValue)
                values.Add(point.LowerControlLimit)
                values.Add(point.LowerTwoSigmaLimit)
                values.Add(point.LowerOneSigmaLimit)
                values.Add(point.UpperOneSigmaLimit)
                values.Add(point.UpperTwoSigmaLimit)
                values.Add(point.UpperControlLimit)
                values.Add(point.EffectiveSampleSize)
                values.Add(point.Exposure)
                values.Add(YesNo(point.IncludedInParameterEstimation))
                values.Add(YesNo(point.IncludedInRuleEvaluation))
                values.Add(ExclusionScopeText(point.ExclusionScope))
                values.Add(point.ExclusionReason)
                values.Add(IntegerListText(point.SignalRuleNumbers, addOne:=False))
                values.Add(YesNo(point.IsSignalled))

                rows.Add(values.ToArray())
            Next
        End Sub

        Private Shared Function PointHeaders(includePanelColumns As Boolean) As String()
            Dim headers As New List(Of String)()
            If includePanelColumns Then
                headers.Add("Panel")
                headers.Add("Panel Type")
            End If

            headers.AddRange({"Point", "Point Index (0-based)", "Source Row(s)", "Label",
                              "Sequence", "Stage", "Phase", "Value", "Center Line",
                              "Std. Error", "Standardized Value", "LCL", "-2 Sigma",
                              "-1 Sigma", "+1 Sigma", "+2 Sigma", "UCL", "Effective N",
                              "Exposure", "Used to Estimate", "Rule-Evaluation Eligible",
                              "Exclusion Scope", "Exclusion Reason", "Rule Numbers",
                              "Signalled"})
            Return headers.ToArray()
        End Function

        Private Shared Function CollectWarningRows(result As SpcFitResult) As List(Of Object())
            Dim rows As New List(Of Object())()
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)

            Dim fitWarnings As String() = result.Warnings
            For i As Integer = 0 To fitWarnings.Length - 1
                AddWarningRow(rows, seen, "Fit", String.Empty, fitWarnings(i))
            Next

            Dim panels As SpcPanelResult() = result.Panels
            For i As Integer = 0 To panels.Length - 1
                Dim panelWarnings As String() = panels(i).Warnings
                For j As Integer = 0 To panelWarnings.Length - 1
                    AddWarningRow(rows,
                                  seen,
                                  "Panel",
                                  panels(i).DisplayName,
                                  panelWarnings(j))
                Next
            Next
            Return rows
        End Function

        Private Shared Sub AddWarningRow(rows As List(Of Object()),
                                         seen As HashSet(Of String),
                                         scope As String,
                                         panel As String,
                                         message As String)
            Dim normalized As String = If(message, String.Empty).Trim()
            If normalized.Length = 0 Then Return

            Dim key As String = scope & ChrW(30) & panel & ChrW(30) & normalized
            If seen.Add(key) Then rows.Add(Row(scope, panel, normalized))
        End Sub

#End Region

#Region "Stage and exclusion audit helpers"

        Private Enum PointCountMode
            All = 0
            Estimation = 1
            Rules = 2
        End Enum

        Private Enum PointTextField
            Label = 0
            Stage = 1
            EstimationIncluded = 2
            RuleIncluded = 3
        End Enum

        Private NotInheritable Class EffectiveStage
            Public Sub New(stageId As String,
                           displayName As String,
                           firstPointIndex As Integer,
                           lastPointIndex As Integer,
                           phase As SpcPhase,
                           limitMode As SpcStageLimitMode,
                           referenceStageId As String,
                           definedInRequest As Boolean)
                Me.StageId = stageId
                Me.DisplayName = displayName
                Me.FirstPointIndex = firstPointIndex
                Me.LastPointIndex = lastPointIndex
                Me.Phase = phase
                Me.LimitMode = limitMode
                Me.ReferenceStageId = referenceStageId
                Me.DefinedInRequest = definedInRequest
            End Sub

            Public ReadOnly Property StageId As String
            Public ReadOnly Property DisplayName As String
            Public Property FirstPointIndex As Integer
            Public Property LastPointIndex As Integer
            Public ReadOnly Property Phase As SpcPhase
            Public ReadOnly Property LimitMode As SpcStageLimitMode
            Public ReadOnly Property ReferenceStageId As String
            Public ReadOnly Property DefinedInRequest As Boolean
        End Class

        Private Shared Function GetEffectiveStages(result As SpcFitResult) As List(Of EffectiveStage)
            Dim configured As SpcStageDefinition() = result.Request.Stages
            Dim values As New List(Of EffectiveStage)()

            If configured.Length > 0 Then
                For i As Integer = 0 To configured.Length - 1
                    Dim stage As SpcStageDefinition = configured(i)
                    values.Add(New EffectiveStage(stage.StageId,
                                                  stage.DisplayName,
                                                  stage.FirstPointIndex,
                                                  stage.LastPointIndex,
                                                  stage.Phase,
                                                  stage.LimitMode,
                                                  stage.ReferenceStageId,
                                                  definedInRequest:=True))
                Next
                Return values
            End If

            Dim positions As New Dictionary(Of String, EffectiveStage)(
                StringComparer.OrdinalIgnoreCase)
            Dim panels As SpcPanelResult() = result.Panels

            For i As Integer = 0 To panels.Length - 1
                Dim points As SpcPointResult() = panels(i).Points
                For j As Integer = 0 To points.Length - 1
                    Dim point As SpcPointResult = points(j)
                    Dim effective As EffectiveStage = Nothing
                    If positions.TryGetValue(point.StageId, effective) Then
                        effective.FirstPointIndex = Math.Min(effective.FirstPointIndex,
                                                            point.PointIndex)
                        effective.LastPointIndex = Math.Max(effective.LastPointIndex,
                                                           point.PointIndex)
                    Else
                        Dim limitMode As SpcStageLimitMode =
                            DefaultStageLimitMode(result.Request.AnalysisOptions.ControlLimits.ParameterSource)
                        Dim sourceStage As String = String.Empty
                        Dim estimate As SpcParameterEstimate =
                            FindParameterEstimate(result, point.StageId)
                        If estimate IsNot Nothing Then
                            limitMode = estimate.LimitMode
                            sourceStage = estimate.SourceStageId
                        End If

                        effective = New EffectiveStage(point.StageId,
                                                       point.StageId,
                                                       point.PointIndex,
                                                       point.PointIndex,
                                                       point.Phase,
                                                       limitMode,
                                                       sourceStage,
                                                       definedInRequest:=False)
                        positions.Add(point.StageId, effective)
                    End If
                Next
            Next

            values.AddRange(positions.Values)
            values.Sort(Function(left As EffectiveStage, right As EffectiveStage) As Integer
                            Dim comparison As Integer =
                                left.FirstPointIndex.CompareTo(right.FirstPointIndex)
                            If comparison <> 0 Then Return comparison
                            Return StringComparer.OrdinalIgnoreCase.Compare(left.StageId,
                                                                           right.StageId)
                        End Function)
            Return values
        End Function

        Private Shared Function DefaultStageLimitMode(
            source As SpcParameterSource) As SpcStageLimitMode

            Select Case source
                Case SpcParameterSource.UseHistoricalParameters
                    Return SpcStageLimitMode.UseHistoricalParameters
                Case Else
                    Return SpcStageLimitMode.EstimateFromStageData
            End Select
        End Function

        Private Shared Function FindParameterEstimate(result As SpcFitResult,
                                                       stageId As String) As SpcParameterEstimate
            Dim estimates As SpcParameterEstimate() = result.ParameterEstimates
            For i As Integer = 0 To estimates.Length - 1
                If String.Equals(estimates(i).StageId,
                                 stageId,
                                 StringComparison.OrdinalIgnoreCase) Then
                    Return estimates(i)
                End If
            Next
            Return Nothing
        End Function

        Private Shared Function CountDistinctStagePoints(result As SpcFitResult,
                                                         stageId As String,
                                                         mode As PointCountMode) As Integer
            Dim indices As New HashSet(Of Integer)()
            Dim panels As SpcPanelResult() = result.Panels

            For i As Integer = 0 To panels.Length - 1
                Dim points As SpcPointResult() = panels(i).Points
                For j As Integer = 0 To points.Length - 1
                    Dim point As SpcPointResult = points(j)
                    If Not String.Equals(point.StageId,
                                         stageId,
                                         StringComparison.OrdinalIgnoreCase) Then
                        Continue For
                    End If

                    Select Case mode
                        Case PointCountMode.All
                            indices.Add(point.PointIndex)
                        Case PointCountMode.Estimation
                            If point.IncludedInParameterEstimation Then
                                indices.Add(point.PointIndex)
                            End If
                        Case PointCountMode.Rules
                            If point.IncludedInRuleEvaluation Then
                                indices.Add(point.PointIndex)
                            End If
                    End Select
                Next
            Next
            Return indices.Count
        End Function

        Private Shared Function CountStageSignals(result As SpcFitResult,
                                                  stageId As String) As Integer
            Dim count As Integer = 0
            Dim signals As SpcSignalResult() = result.Signals
            For i As Integer = 0 To signals.Length - 1
                If String.Equals(signals(i).StageId,
                                 stageId,
                                 StringComparison.OrdinalIgnoreCase) Then
                    count += 1
                End If
            Next
            Return count
        End Function

        Private Shared Function GetPointsAtIndex(result As SpcFitResult,
                                                 pointIndex As Integer) As List(Of SpcPointResult)
            Dim values As New List(Of SpcPointResult)()
            Dim panels As SpcPanelResult() = result.Panels
            For i As Integer = 0 To panels.Length - 1
                Dim point As SpcPointResult = panels(i).GetPoint(pointIndex)
                If point IsNot Nothing Then values.Add(point)
            Next
            Return values
        End Function

        Private Shared Function JoinDistinctSourceRows(
            points As IEnumerable(Of SpcPointResult)) As String

            Dim values As New List(Of Integer)()
            Dim seen As New HashSet(Of Integer)()
            For Each point As SpcPointResult In points
                Dim sourceRows As Integer() = point.SourceRowIndices
                For i As Integer = 0 To sourceRows.Length - 1
                    If seen.Add(sourceRows(i)) Then values.Add(sourceRows(i))
                Next
            Next
            values.Sort()
            Return IntegerListText(values.ToArray(), addOne:=False)
        End Function

        Private Shared Function JoinDistinctPointText(
            points As IEnumerable(Of SpcPointResult),
            field As PointTextField) As String

            Dim values As New List(Of String)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each point As SpcPointResult In points
                Dim value As String
                Select Case field
                    Case PointTextField.Label
                        value = point.Label
                    Case PointTextField.Stage
                        value = point.StageId
                    Case PointTextField.EstimationIncluded
                        value = YesNo(point.IncludedInParameterEstimation)
                    Case PointTextField.RuleIncluded
                        value = YesNo(point.IncludedInRuleEvaluation)
                    Case Else
                        Throw New ArgumentOutOfRangeException(NameOf(field))
                End Select

                If seen.Add(value) Then values.Add(value)
            Next
            Return String.Join(", ", values.ToArray())
        End Function

#End Region

#Region "Display helpers"

        Private Shared Sub RequireResult(result As SpcFitResult)
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        End Sub

        Private Shared Function YesNo(value As Boolean) As String
            Return If(value, "Yes", "No")
        End Function

        Private Shared Function NullableDoubleValue(value As Nullable(Of Double)) As Object
            If value.HasValue Then Return value.Value
            Return String.Empty
        End Function

        Private Shared Function NullableIntegerValue(value As Nullable(Of Integer)) As Object
            If value.HasValue Then Return value.Value
            Return String.Empty
        End Function

        Private Shared Function NullableUtcText(value As Nullable(Of DateTime)) As Object
            If Not value.HasValue Then Return String.Empty
            Return value.Value.ToUniversalTime().ToString(
                "yyyy-MM-dd HH:mm:ss.fff 'UTC'",
                CultureInfo.InvariantCulture)
        End Function

        Private Shared Function IntegerListText(values As Integer(), addOne As Boolean) As String
            If values Is Nothing OrElse values.Length = 0 Then Return String.Empty

            Dim text(values.Length - 1) As String
            For i As Integer = 0 To values.Length - 1
                Dim value As Integer = values(i)
                If addOne Then value += 1
                text(i) = value.ToString(CultureInfo.InvariantCulture)
            Next
            Return String.Join(", ", text)
        End Function

        Private Shared Function ChartTypeText(value As SpcChartType) As String
            Select Case value
                Case SpcChartType.RunChart : Return "Run chart"
                Case SpcChartType.Individuals : Return "Individuals chart"
                Case SpcChartType.MovingRange : Return "Moving-range chart"
                Case SpcChartType.IndividualsMovingRange : Return "I-MR chart"
                Case SpcChartType.XBar : Return "X-bar chart"
                Case SpcChartType.SubgroupRange : Return "R chart"
                Case SpcChartType.SubgroupStandardDeviation : Return "S chart"
                Case SpcChartType.XBarR : Return "X-bar / R chart"
                Case SpcChartType.XBarS : Return "X-bar / S chart"
                Case SpcChartType.PChart : Return "p chart"
                Case SpcChartType.NpChart : Return "np chart"
                Case SpcChartType.CChart : Return "c chart"
                Case SpcChartType.UChart : Return "u chart"
                Case SpcChartType.LaneyPPrime : Return "Laney p-prime chart"
                Case SpcChartType.LaneyUPrime : Return "Laney u-prime chart"
                Case SpcChartType.GChart : Return "g chart"
                Case SpcChartType.TChart : Return "t chart"
                Case SpcChartType.Cusum : Return "CUSUM chart"
                Case SpcChartType.Ewma : Return "EWMA chart"
                Case SpcChartType.MovingAverage : Return "Moving-average chart"
                Case SpcChartType.HotellingT2 : Return "Hotelling T-squared chart"
                Case SpcChartType.GeneralizedVariance : Return "Generalized-variance chart"
                Case SpcChartType.PcaT2 : Return "PCA T-squared chart"
                Case SpcChartType.PcaQ : Return "PCA Q chart"
                Case SpcChartType.Mewma : Return "MEWMA chart"
                Case SpcChartType.Mcusum : Return "MCUSUM chart"
                Case SpcChartType.ShortRunZMovingRange : Return "Short-run Z-MR chart"
                Case SpcChartType.BetweenWithin : Return "Between/within chart"
                Case SpcChartType.ResidualChart : Return "Residual chart"
                Case SpcChartType.ProfileChart : Return "Profile chart"
                Case SpcChartType.RiskAdjustedChart : Return "Risk-adjusted chart"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function ChartFamilyText(value As SpcChartFamily) As String
            Select Case value
                Case SpcChartFamily.Run : Return "Run"
                Case SpcChartFamily.ShewhartVariables : Return "Shewhart variables"
                Case SpcChartFamily.ShewhartAttributes : Return "Shewhart attributes"
                Case SpcChartFamily.TimeWeighted : Return "Time-weighted"
                Case SpcChartFamily.RareEvent : Return "Rare event"
                Case SpcChartFamily.Multivariate : Return "Multivariate"
                Case SpcChartFamily.Specialized : Return "Specialized"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function PanelTypeText(value As SpcPanelType) As String
            Select Case value
                Case SpcPanelType.Run : Return "Run"
                Case SpcPanelType.IndividualValue : Return "Individual value"
                Case SpcPanelType.MovingRange : Return "Moving range"
                Case SpcPanelType.SubgroupMean : Return "Subgroup mean"
                Case SpcPanelType.SubgroupRange : Return "Subgroup range"
                Case SpcPanelType.SubgroupStandardDeviation : Return "Subgroup standard deviation"
                Case SpcPanelType.Proportion : Return "Proportion"
                Case SpcPanelType.NumberNonconforming : Return "Number nonconforming"
                Case SpcPanelType.DefectCount : Return "Defect count"
                Case SpcPanelType.DefectRate : Return "Defect rate"
                Case SpcPanelType.StandardizedProportion : Return "Standardized proportion"
                Case SpcPanelType.StandardizedDefectRate : Return "Standardized defect rate"
                Case SpcPanelType.EventsBetweenOccurrences : Return "Events between occurrences"
                Case SpcPanelType.TimeBetweenOccurrences : Return "Time between occurrences"
                Case SpcPanelType.UpperCusum : Return "Upper CUSUM"
                Case SpcPanelType.LowerCusum : Return "Lower CUSUM"
                Case SpcPanelType.Ewma : Return "EWMA"
                Case SpcPanelType.MovingAverage : Return "Moving average"
                Case SpcPanelType.HotellingT2 : Return "Hotelling T-squared"
                Case SpcPanelType.GeneralizedVariance : Return "Generalized variance"
                Case SpcPanelType.PcaT2 : Return "PCA T-squared"
                Case SpcPanelType.PcaQ : Return "PCA Q"
                Case SpcPanelType.Mewma : Return "MEWMA"
                Case SpcPanelType.Mcusum : Return "MCUSUM"
                Case SpcPanelType.StandardizedValue : Return "Standardized value"
                Case SpcPanelType.Residual : Return "Residual"
                Case SpcPanelType.ProfileStatistic : Return "Profile statistic"
                Case SpcPanelType.RiskAdjustedStatistic : Return "Risk-adjusted statistic"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function DataLayoutText(value As SpcDataLayout) As String
            Select Case value
                Case SpcDataLayout.WideSubgroups : Return "Wide subgroups"
                Case SpcDataLayout.StackedObservations : Return "Stacked observations"
                Case SpcDataLayout.IndividualSequence : Return "Individual sequence"
                Case SpcDataLayout.AggregatedCounts : Return "Aggregated counts"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function MissingValuePolicyText(value As SpcMissingValuePolicy) As String
            Select Case value
                Case SpcMissingValuePolicy.Reject : Return "Reject"
                Case SpcMissingValuePolicy.OmitPoint : Return "Omit chart point"
                Case SpcMissingValuePolicy.UseAvailableMeasurements : Return "Use available measurements"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function PhaseText(value As SpcPhase) As String
            Select Case value
                Case SpcPhase.PhaseI : Return "Phase I"
                Case SpcPhase.PhaseII : Return "Phase II"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function StageLimitModeText(value As SpcStageLimitMode) As String
            Select Case value
                Case SpcStageLimitMode.EstimateFromStageData : Return "Estimate from stage data"
                Case SpcStageLimitMode.UseReferenceStage : Return "Use reference stage"
                Case SpcStageLimitMode.UseHistoricalParameters : Return "Use historical parameters"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function ParameterSourceText(value As SpcParameterSource) As String
            Select Case value
                Case SpcParameterSource.EstimateFromPhaseI : Return "Estimate from Phase I"
                Case SpcParameterSource.UseHistoricalParameters : Return "Use historical parameters"
                Case SpcParameterSource.DefinedByStage : Return "Defined by stage"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function ControlLimitMethodText(value As SpcControlLimitMethod) As String
            Select Case value
                Case SpcControlLimitMethod.ShewhartSigma : Return "Shewhart sigma limits"
                Case SpcControlLimitMethod.ExactProbability : Return "Exact probability limits"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function SigmaEstimatorText(value As SpcWithinSigmaEstimator) As String
            Select Case value
                Case SpcWithinSigmaEstimator.Automatic : Return "Automatic"
                Case SpcWithinSigmaEstimator.AverageRange : Return "Average range"
                Case SpcWithinSigmaEstimator.AverageStandardDeviation : Return "Average standard deviation"
                Case SpcWithinSigmaEstimator.PooledStandardDeviation : Return "Pooled standard deviation"
                Case SpcWithinSigmaEstimator.MovingRange : Return "Moving range"
                Case SpcWithinSigmaEstimator.MedianMovingRange : Return "Median moving range"
                Case SpcWithinSigmaEstimator.SampleStandardDeviation : Return "Sample standard deviation"
                Case SpcWithinSigmaEstimator.MedianAbsoluteDeviation : Return "Median absolute deviation"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function NaturalLimitPolicyText(value As SpcNaturalLimitPolicy) As String
            Select Case value
                Case SpcNaturalLimitPolicy.ClipToFeasibleRange : Return "Clip to feasible range"
                Case SpcNaturalLimitPolicy.RetainCalculatedLimits : Return "Retain calculated limits"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function RulePresetText(value As SpcRulePreset) As String
            Select Case value
                Case SpcRulePreset.None : Return "None"
                Case SpcRulePreset.RuleOneOnly : Return "Rule 1 only"
                Case SpcRulePreset.WesternElectric : Return "Western Electric"
                Case SpcRulePreset.Nelson : Return "Nelson"
                Case SpcRulePreset.PaperMontgomeryEightRules : Return "Paper / Montgomery eight rules"
                Case SpcRulePreset.Custom : Return "Custom"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function RuleKindText(value As SpcRuleKind) As String
            Select Case value
                Case SpcRuleKind.BeyondSigma : Return "Beyond sigma"
                Case SpcRuleKind.KOfMConsecutiveBeyondSigma : Return "K of M beyond sigma"
                Case SpcRuleKind.RunOnOneSide : Return "Run on one side"
                Case SpcRuleKind.MonotonicTrend : Return "Monotonic trend"
                Case SpcRuleKind.Alternating : Return "Alternating"
                Case SpcRuleKind.AllWithinSigma : Return "All within sigma"
                Case SpcRuleKind.AllBeyondSigmaOnBothSides : Return "Outside sigma on both sides"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function RuleSideText(value As SpcRuleSide) As String
            Select Case value
                Case SpcRuleSide.EitherSide : Return "Either side"
                Case SpcRuleSide.UpperSideOnly : Return "Upper"
                Case SpcRuleSide.LowerSideOnly : Return "Lower"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function RuleScopeText(value As SpcRuleScope) As String
            If value = SpcRuleScope.All Then Return "All panels"
            If value = SpcRuleScope.AllShewhartPanels Then Return "All Shewhart panels"
            If value = SpcRuleScope.LocationAndAttributePanels Then Return "Location and attribute panels"
            If value = SpcRuleScope.None Then Return "None"

            Dim names As New List(Of String)()
            If (value And SpcRuleScope.LocationPanels) <> SpcRuleScope.None Then names.Add("Location")
            If (value And SpcRuleScope.DispersionPanels) <> SpcRuleScope.None Then names.Add("Dispersion")
            If (value And SpcRuleScope.AttributePanels) <> SpcRuleScope.None Then names.Add("Attribute")
            If (value And SpcRuleScope.TimeWeightedPanels) <> SpcRuleScope.None Then names.Add("Time-weighted")
            If (value And SpcRuleScope.RareEventPanels) <> SpcRuleScope.None Then names.Add("Rare-event")
            If (value And SpcRuleScope.MultivariatePanels) <> SpcRuleScope.None Then names.Add("Multivariate")
            Return String.Join(", ", names.ToArray())
        End Function

        Private Shared Function RulePhaseScopeText(value As SpcRulePhaseScope) As String
            Select Case value
                Case SpcRulePhaseScope.None : Return "None"
                Case SpcRulePhaseScope.PhaseI : Return "Phase I"
                Case SpcRulePhaseScope.PhaseII : Return "Phase II"
                Case SpcRulePhaseScope.All : Return "Phase I and Phase II"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function SequenceGapText(value As SpcSequenceGapBehavior) As String
            Select Case value
                Case SpcSequenceGapBehavior.BreakSequence : Return "Break sequence"
                Case SpcSequenceGapBehavior.SkipPointAndContinue : Return "Skip point and continue"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function SignalMarkingText(value As SpcSignalMarkingMode) As String
            Select Case value
                Case SpcSignalMarkingMode.TerminalPointOnly : Return "Terminal point only"
                Case SpcSignalMarkingMode.EntirePattern : Return "Entire contributing pattern"
                Case Else : Return value.ToString()
            End Select
        End Function

        Private Shared Function ExclusionScopeText(value As SpcExclusionScope) As String
            Select Case value
                Case SpcExclusionScope.None : Return "None"
                Case SpcExclusionScope.ParameterEstimation : Return "Parameter estimation"
                Case SpcExclusionScope.RuleEvaluation : Return "Rule evaluation"
                Case SpcExclusionScope.EstimationAndRules : Return "Estimation and rules"
                Case Else : Return value.ToString()
            End Select
        End Function

#End Region

    End Class

End Namespace
