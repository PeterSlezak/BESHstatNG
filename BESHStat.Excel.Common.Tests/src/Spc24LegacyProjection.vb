Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG.ExcelCommon
Public NotInheritable Class Spc24LegacyProjection
    Public Class AuditCell
        Public Text As String = ""
        Public Checked As Boolean
        Public Value As Decimal
    End Class
    Public SignalOnly As Boolean
    Public DefaultCaption As String = "Default chart"
    Public ReadOnly tbChartTitle As New AuditCell
    Public ReadOnly tbValueAxisTitle As New AuditCell
    Public ReadOnly tbHorizontalAxisTitle As New AuditCell
    Public ReadOnly chkUseSequenceValuesForHorizontalAxis As New AuditCell
    Public ReadOnly spinChartWidth As New AuditCell With {.Value=760D}
    Public ReadOnly spinChartHeight As New AuditCell With {.Value=360D}
    Public Function BuildChartRenderingResult(result As SpcMultivariateFitResult) As SpcFitResult
        Dim context = SpcMultivariateResultAxisPolicy.Project(result)
        Dim sourcePanels As SpcPanelResult() = context.Panels.ToArray()
        Dim panels(sourcePanels.Length - 1) As SpcPanelResult
        For i As Integer = 0 To sourcePanels.Length - 1
            Dim source As SpcPanelResult = sourcePanels(i)
            panels(i) = New SpcPanelResult(
                source.PanelType,
                source.DisplayName,
                source.Points,
                valueAxisTitle:=If(String.IsNullOrWhiteSpace(tbValueAxisTitle.Text), source.ValueAxisTitle, tbValueAxisTitle.Text.Trim()),
                parameterEstimates:=source.ParameterEstimates,
                signals:=source.Signals,
                warnings:=source.Warnings)
        Next
        Dim input As SpcInputData = SpcMultivariateResultAxisPolicy.RenderingInput(panels(0))
        Dim title As String = If(String.IsNullOrWhiteSpace(tbChartTitle.Text), DefaultCaption, tbChartTitle.Text.Trim())
        Dim request As New SpcFitRequest(result.Request.ChartType, input,
                                         requestLabel:=result.Request.RequestLabel,
                                         chartTitle:=title,
                                         valueAxisTitle:=tbValueAxisTitle.Text.Trim())
        Return New SpcFitResult(request, panels, result.Warnings, result.ExecutionTimeMilliseconds)
    End Function

    Public Function BuildSummaryTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim panel As SpcPanelResult = result.Panels(0)
        Dim model As SpcMultivariateModelResult = result.Model
        Dim signalCount As Integer = panel.Signals.Length
        Dim rows As New List(Of Object()) From {
            Row("Analysis", result.Request.RequestLabel),
            Row("Chart type", ChartTypeText(result.Request.ChartType)),
            Row("Observation structure", If(result.Request.HasSubgroups, "Rational subgroups", "Individual observations")),
            Row("Input rows", result.Request.RowCount),
            Row("Variables", result.Request.VariableCount),
            Row("Chart points", panel.PointCount),
            Row("Model source", ModelSourceText(result.Request.ModelSource)),
            Row("Baseline observations", model.BaselineObservationCount),
            Row("Baseline subgroups", model.BaselineSubgroupCount),
            Row("Covariance degrees of freedom", model.CovarianceDegreesOfFreedom),
            Row("Effective dimension", model.EffectiveDimension),
            Row("Retained PCA components", model.RetainedComponentCount),
            Row("Pseudoinverse used", YesNo(model.UsedPseudoInverse)),
            Row("Signal occurrences", signalCount),
            Row("Signalled points", panel.SignalledPointCount),
            Row("Process status", If(signalCount = 0, "No intrinsic-limit signals detected", "Intrinsic-limit signal(s) detected")),
            Row("Warnings", CollectWarnings(result).Count),
            Row("Execution time (ms)", result.ExecutionTimeMilliseconds)
        }
        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate SPC analysis - summary", {"Item", "Value"}, rows,
                        footnote:="Signal status reflects the selected multivariate chart and its intrinsic control or decision limit.")
        }

        Dim parameterRows As New List(Of Object())()
        For Each estimate As SpcParameterEstimate In panel.ParameterEstimates
            parameterRows.Add(Row(estimate.StageId, estimate.DisplayName, estimate.ParameterName,
                                  estimate.Value, NullableValue(estimate.StandardError), estimate.Method,
                                  NullableValue(estimate.SampleCount)))
        Next
        tables.Add(CreateTable("Multivariate SPC analysis - retained chart parameters",
                               {"Stage", "Parameter", "Identifier", "Value", "Standard Error", "Method", "Sample Count"},
                               parameterRows, emptyMessage:="No retained chart parameters were reported."))
        tables.Add(BuildWarningsTable(result))
        Return tables
    End Function

    Public Function BuildModelTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim model As SpcMultivariateModelResult = result.Model
        Dim names As String() = result.Request.VariableNames
        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate in-control model - properties", {"Item", "Value"},
                New List(Of Object()) From {
                    Row("Source", ModelSourceText(model.Source)),
                    Row("Baseline observations", model.BaselineObservationCount),
                    Row("Baseline subgroups", model.BaselineSubgroupCount),
                    Row("Covariance degrees of freedom", model.CovarianceDegreesOfFreedom),
                    Row("Effective dimension", model.EffectiveDimension),
                    Row("Retained components", model.RetainedComponentCount),
                    Row("Pseudoinverse used", YesNo(model.UsedPseudoInverse)),
                    Row("Diagonal ridge factor", model.Regularization)
                })
        }
        If model.ProcessMean IsNot Nothing Then tables.Add(CreateVectorTable("Process mean vector", names, model.ProcessMean, "Mean"))
        tables.Add(CreateVectorTable("Analysis scale", names, model.AnalysisScale, "Scale"))
        tables.Add(CreateMatrixTable("Process covariance matrix", names, names, model.ProcessCovariance))
        tables.Add(CreateMatrixTable("Analysis covariance matrix", names, names, model.AnalysisCovariance))
        tables.Add(CreateMatrixTable("Analysis covariance inverse", names, names, model.AnalysisCovarianceInverse))
        tables.Add(CreateIndexedVectorTable("Eigenvalues", model.Eigenvalues, "Component", "Eigenvalue"))
        tables.Add(CreateMatrixTable("Eigenvectors", names, ComponentNames(model.Eigenvectors.GetLength(1)), model.Eigenvectors))
        Return tables
    End Function

    Public Function BuildChartDataTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        Dim panel As SpcPanelResult = result.Panels(0)
        For Each point As SpcPointResult In panel.Points
            rows.Add(Row(point.PointIndex + 1,
                         JoinIntegers(point.SourceRowIndices, addOne:=False),
                         point.Label,
                         point.StageId,
                         PhaseText(point.Phase),
                         point.Value,
                         point.CenterLine,
                         point.LowerControlLimit,
                         point.UpperControlLimit,
                         YesNo(point.IncludedInParameterEstimation),
                         YesNo(point.IncludedInRuleEvaluation),
                         ExclusionScopeText(point.ExclusionScope),
                         point.ExclusionReason,
                         YesNo(point.IsSignalled),
                         JoinIntegers(point.SignalRuleNumbers, addOne:=False)))
        Next
        Return CreateTable("Multivariate SPC chart data",
            {"Point", "Source Rows", "Label", "Stage", "Phase", "Statistic", "Center Line", "LCL", "UCL",
             "Used to Estimate", "Signal Eligible", "Exclusion Scope", "Exclusion Reason", "Signal", "Signal Numbers"},
            rows)
    End Function

    Public Function BuildSignalsTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        For Each signal As SpcSignalResult In result.Panels(0).Signals
            rows.Add(Row(signal.RuleNumber,
                         signal.RuleCode,
                         signal.Rule.DisplayName,
                         signal.StageId,
                         signal.TerminalPointIndex + 1,
                         signal.WindowStartPointIndex + 1,
                         signal.WindowEndPointIndex + 1,
                         signal.TriggeredSide.ToString(),
                         JoinIntegers(signal.ContributingPointIndices, addOne:=True),
                         JoinIntegers(signal.MarkedPointIndices, addOne:=True),
                         signal.Message))
        Next
        Return CreateTable("Multivariate SPC signals",
            {"Signal Number", "Code", "Signal", "Stage", "Terminal Point", "Window Start", "Window End",
             "Side", "Contributing Points", "Marked Points", "Message"},
            rows, emptyMessage:="No intrinsic-limit signals were detected.")
    End Function

    Public Function BuildDiagnosticTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim context = SpcMultivariateResultAxisPolicy.Project(result)
        Dim signalOnly As Boolean = Me.SignalOnly
        Dim selected = context.SelectDiagnostics(signalOnly)

        Dim names As String() = result.Request.VariableNames
        Dim hasState As Boolean = selected.Any(Function(item) item.StateVector IsNot Nothing)
        Dim hasScores As Boolean = selected.Any(Function(item) item.ComponentScores IsNot Nothing)
        Dim hasResiduals As Boolean = selected.Any(Function(item) item.ResidualVector IsNot Nothing)
        Dim hasContributions As Boolean = selected.Any(Function(item) item.Contributions IsNot Nothing)
        Dim scoreCount As Integer = 0
        For Each item As SpcMultivariatePointDiagnostic In selected
            If item.ComponentScores IsNot Nothing Then scoreCount = Math.Max(scoreCount, item.ComponentScores.Length)
        Next

        Dim headers As New List(Of String) From {
            "Point", "Source Rows", "Label", "Stage", "Phase", "Statistic", "Effective Sample Size"
        }
        For Each name As String In names
            headers.Add("Observed: " & name)
        Next
        If hasState Then
            For Each name As String In names
                headers.Add("State: " & name)
            Next
        End If
        If hasScores Then
            For i As Integer = 1 To scoreCount
                headers.Add("PC Score " & i.ToString())
            Next
        End If
        If hasResiduals Then
            For Each name As String In names
                headers.Add("Residual: " & name)
            Next
        End If
        If hasContributions Then
            For Each name As String In names
                headers.Add("Contribution: " & name)
            Next
            headers.Add("Contribution Total")
            headers.Add("Contribution Basis")
        End If

        Dim rows As New List(Of Object())()
        For Each item As SpcMultivariatePointDiagnostic In selected
            Dim values As New List(Of Object) From {
                item.PointIndex + 1,
                JoinIntegers(item.SourceRowIndices, addOne:=False),
                item.Label,
                item.StageId,
                PhaseText(item.Phase),
                item.Statistic,
                item.EffectiveSampleSize
            }
            AddVectorValues(values, item.ObservationVector, names.Length)
            If hasState Then AddVectorValues(values, item.StateVector, names.Length)
            If hasScores Then AddVectorValues(values, item.ComponentScores, scoreCount)
            If hasResiduals Then AddVectorValues(values, item.ResidualVector, names.Length)
            If hasContributions Then
                AddVectorValues(values, item.Contributions, names.Length)
                values.Add(If(Double.IsNaN(item.ContributionTotal), CType(String.Empty, Object), item.ContributionTotal))
                values.Add(item.ContributionBasis)
            End If
            rows.Add(values.ToArray())
        Next

        Dim tables As New List(Of ResultTable) From {
            CreateTable("Multivariate point diagnostics and contributions", headers.ToArray(), rows,
                        emptyMessage:=If(signalOnly, "No signalled points have diagnostics.", "No point diagnostics were returned."))
        }
        For Each item As SpcMultivariatePointDiagnostic In selected
            If item.SubgroupCovariance Is Nothing Then Continue For
            tables.Add(CreateMatrixTable("Subgroup covariance - point " & (item.PointIndex + 1).ToString(),
                                          names, names, item.SubgroupCovariance))
        Next
        Return tables
    End Function

    Public Function BuildAuditTables(result As SpcMultivariateFitResult) As List(Of ResultTable)
        Dim request As SpcMultivariateRequest = result.Request
        Dim settings As New List(Of Object()) From {
            Row("Request label", request.RequestLabel),
            Row("Chart type", ChartTypeText(request.ChartType)),
            Row("Observation structure", If(request.HasSubgroups, "Rational subgroups", "Individual observations")),
            Row("Missing-value policy", MissingPolicyText(request.MissingValuePolicy)),
            Row("Model source", ModelSourceText(request.ModelSource)),
            Row("Control-limit alpha", request.ControlLimitAlpha),
            Row("Use lower Hotelling limit", YesNo(request.UseLowerHotellingLimit)),
            Row("Diagonal ridge factor", request.CovarianceRegularization),
            Row("Allow pseudoinverse", YesNo(request.AllowPseudoInverse)),
            Row("PCA matrix", If(request.PcaUseCorrelationMatrix, "Correlation", "Covariance")),
            Row("PCA component count", NullableValue(request.PcaComponentCount)),
            Row("PCA cumulative variance", request.PcaCumulativeVariance),
            Row("MEWMA lambda", request.MewmaLambda),
            Row("MEWMA supplied UCL", NullableValue(request.MewmaControlLimit)),
            Row("MCUSUM k", request.McusumReferenceValue),
            Row("MCUSUM h", request.McusumDecisionInterval),
            Row("Generalized-variance sigma multiplier", NullableValue(request.GeneralizedVarianceSigmaMultiplier)),
            Row("Reset at stage boundary", YesNo(request.ResetAtStageBoundary)),
            Row("Reset at phase boundary", YesNo(request.ResetAtPhaseBoundary)),
            Row("Reset after signal", YesNo(request.ResetAfterSignal)),
            Row("Sequence-gap behaviour", SequenceGapBehaviorText(request.SequenceGapBehavior)),
            Row("Chart title", tbChartTitle.Text.Trim()),
            Row("Value-axis title", tbValueAxisTitle.Text.Trim()),
            Row("Horizontal-axis title", tbHorizontalAxisTitle.Text.Trim()),
            Row("Use sequence on horizontal axis", YesNo(chkUseSequenceValuesForHorizontalAxis.Checked)),
            Row("Chart width", spinChartWidth.Value),
            Row("Chart height", spinChartHeight.Value),
            Row("Execution time (ms)", result.ExecutionTimeMilliseconds)
        }

        Dim stageRows As New List(Of Object())()
        Dim panelPoints As SpcPointResult() = result.Panels(0).Points
        If panelPoints.Length > 0 Then
            Dim start As Integer = 0
            For i As Integer = 1 To panelPoints.Length
                Dim atEnd As Boolean = i = panelPoints.Length
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(panelPoints(i).StageId, panelPoints(start).StageId, StringComparison.Ordinal) OrElse
                     panelPoints(i).Phase <> panelPoints(start).Phase)
                If Not atEnd AndAlso Not changed Then Continue For
                stageRows.Add(Row(panelPoints(start).StageId,
                                  panelPoints(start).PointIndex + 1,
                                  panelPoints(i - 1).PointIndex + 1,
                                  PhaseText(panelPoints(start).Phase)))
                start = i
            Next
        End If

        Dim exclusionRows As New List(Of Object())()
        Dim scopes As SpcExclusionScope() = request.ExclusionScopes
        Dim reasons As String() = request.ExclusionReasons
        Dim sourceRows As Integer() = request.SourceRowIndices
        For i As Integer = 0 To scopes.Length - 1
            If scopes(i) = SpcExclusionScope.None Then Continue For
            exclusionRows.Add(Row(sourceRows(i), ExclusionScopeText(scopes(i)), reasons(i)))
        Next

        Return New List(Of ResultTable) From {
            CreateTable("Multivariate SPC settings and audit", {"Setting", "Value"}, settings),
            CreateTable("Effective stages", {"Stage", "First Point", "Last Point", "Phase"}, stageRows,
                        emptyMessage:="No chart points were returned."),
            CreateTable("Source-row exclusions", {"Source Row", "Scope", "Reason"}, exclusionRows,
                        emptyMessage:="No explicit exclusions were supplied."),
            BuildWarningsTable(result)
        }
    End Function

    Public Function BuildWarningsTable(result As SpcMultivariateFitResult) As ResultTable
        Dim rows As New List(Of Object())()
        Dim warnings As List(Of String) = CollectWarnings(result)
        For i As Integer = 0 To warnings.Count - 1
            rows.Add(Row(i + 1, warnings(i)))
        Next
        Return CreateTable("Multivariate SPC warnings", {"Number", "Warning"}, rows,
                           emptyMessage:="No calculation warnings were reported.")
    End Function

    Public Shared Function CollectWarnings(result As SpcMultivariateFitResult) As List(Of String)
        Dim values As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each warning As String In result.Warnings
            If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
        Next
        For Each panel As SpcPanelResult In result.Panels
            For Each warning As String In panel.Warnings
                If warning.Length > 0 AndAlso seen.Add(warning) Then values.Add(warning)
            Next
        Next
        Return values
    End Function

    Public Shared Function CreateTable(title As String, headers As String(), rows As IList(Of Object()),
                                        Optional emptyMessage As String = Nothing,
                                        Optional footnote As String = Nothing) As ResultTable
        Dim table As New ResultTable()
        If title.Length > 0 Then table.AddTitle(title)
        table.AddHeaderTopRow(CType(headers.Clone(), String()))
        If rows.Count > 0 Then
            Dim body(rows.Count - 1, headers.Length - 1) As Object
            For r As Integer = 0 To rows.Count - 1
                If rows(r).Length <> headers.Length Then Throw New InvalidOperationException("A result row does not match its table header.")
                For c As Integer = 0 To headers.Length - 1
                    body(r, c) = rows(r)(c)
                Next
            Next
            table.SetBody(body)
        ElseIf Not String.IsNullOrWhiteSpace(emptyMessage) Then
            table.AddFootnote(emptyMessage)
        End If
        If Not String.IsNullOrWhiteSpace(footnote) Then table.AddFootnote(footnote)
        Return table
    End Function

    Public Shared Function CreateVectorTable(title As String, names As String(), values As Double(), valueHeader As String) As ResultTable
        Dim rows As New List(Of Object())()
        If values IsNot Nothing Then
            For i As Integer = 0 To values.Length - 1
                rows.Add(Row(names(i), values(i)))
            Next
        End If
        Return CreateTable(title, {"Variable", valueHeader}, rows, emptyMessage:="Not applicable to this chart.")
    End Function

    Public Shared Function CreateIndexedVectorTable(title As String, values As Double(), indexHeader As String, valueHeader As String) As ResultTable
        Dim rows As New List(Of Object())()
        For i As Integer = 0 To values.Length - 1
            rows.Add(Row(i + 1, values(i)))
        Next
        Return CreateTable(title, {indexHeader, valueHeader}, rows)
    End Function

    Public Shared Function CreateMatrixTable(title As String, rowNames As String(), columnNames As String(), values As Double(,)) As ResultTable
        Dim headers As New List(Of String) From {"Variable"}
        headers.AddRange(columnNames)
        Dim rows As New List(Of Object())()
        For r As Integer = 0 To values.GetLength(0) - 1
            Dim row(values.GetLength(1)) As Object
            row(0) = rowNames(r)
            For c As Integer = 0 To values.GetLength(1) - 1
                row(c + 1) = values(r, c)
            Next
            rows.Add(row)
        Next
        Return CreateTable(title, headers.ToArray(), rows)
    End Function

    Public Shared Function ComponentNames(count As Integer) As String()
        Dim names(count - 1) As String
        For i As Integer = 0 To count - 1
            names(i) = "PC" & (i + 1).ToString(CultureInfo.InvariantCulture)
        Next
        Return names
    End Function

    Public Shared Sub AddVectorValues(target As List(Of Object), values As Double(), count As Integer)
        For i As Integer = 0 To count - 1
            If values IsNot Nothing AndAlso i < values.Length Then
                target.Add(values(i))
            Else
                target.Add(String.Empty)
            End If
        Next
    End Sub

    Public Shared Function Row(ParamArray values As Object()) As Object()
        Return values
    End Function

    Public Shared Function PhaseText(value As SpcPhase) As String
        Return If(value = SpcPhase.PhaseI, "Phase I", "Phase II")
    End Function

    Public Shared Function MissingPolicyText(value As SpcMissingValuePolicy) As String
        Return If(value = SpcMissingValuePolicy.Reject, "Reject", "Omit incomplete observation")
    End Function

    Public Shared Function ModelSourceText(value As SpcMultivariateModelSource) As String
        Return If(value = SpcMultivariateModelSource.EstimateFromPhaseI, "Estimate from Phase I", "Use historical parameters")
    End Function

    Public Shared Function SequenceGapBehaviorText(value As SpcSequenceGapBehavior) As String
        Select Case value
            Case SpcSequenceGapBehavior.BreakSequence : Return "Break sequence"
            Case SpcSequenceGapBehavior.SkipPointAndContinue : Return "Skip point and continue"
            Case Else : Return value.ToString()
        End Select
    End Function

    Public Shared Function ChartTypeText(value As SpcChartType) As String
        Select Case value
            Case SpcChartType.HotellingT2 : Return "Hotelling T-squared"
            Case SpcChartType.GeneralizedVariance : Return "Generalized variance"
            Case SpcChartType.PcaT2 : Return "PCA T-squared"
            Case SpcChartType.PcaQ : Return "PCA Q"
            Case SpcChartType.Mewma : Return "MEWMA"
            Case SpcChartType.Mcusum : Return "MCUSUM (Crosier)"
            Case Else : Return value.ToString()
        End Select
    End Function

    Public Shared Function YesNo(value As Boolean) As String
        Return If(value, "Yes", "No")
    End Function

    Public Shared Function NullableValue(value As Nullable(Of Double)) As Object
        Return If(value.HasValue, CType(value.Value, Object), String.Empty)
    End Function

    Public Shared Function NullableValue(value As Nullable(Of Integer)) As Object
        Return If(value.HasValue, CType(value.Value, Object), String.Empty)
    End Function

    Public Shared Function JoinIntegers(values As Integer(), addOne As Boolean) As String
        If values Is Nothing OrElse values.Length = 0 Then Return String.Empty
        Dim text(values.Length - 1) As String
        For i As Integer = 0 To values.Length - 1
            Dim value As Integer = values(i)
            If addOne Then value += 1
            text(i) = value.ToString(CultureInfo.InvariantCulture)
        Next
        Return String.Join(", ", text)
    End Function

    Public Shared Function ExclusionScopeText(scope As SpcExclusionScope) As String
        Select Case scope
            Case SpcExclusionScope.None : Return "None"
            Case SpcExclusionScope.ParameterEstimation : Return "Parameter estimation"
            Case SpcExclusionScope.RuleEvaluation : Return "Signal evaluation"
            Case SpcExclusionScope.EstimationAndRules : Return "Parameter estimation and signal evaluation"
            Case Else : Return scope.ToString()
        End Select
    End Function
End Class
