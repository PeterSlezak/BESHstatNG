Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl
Imports BESHStatNG

' Frozen entry-source characterization, NOT_GOLDEN. No WinForms/Excel references.
Public Class Spc23LegacyPreparation
    Public Class DataGridViewCell
        Public Property Value As Object
    End Class
    Public Class DataGridViewRow
        Public Property IsNewRow As Boolean
        Public ReadOnly Cells As New List(Of DataGridViewCell)
        Public Sub New(ParamArray values As Object())
            For Each value In values : Cells.Add(New DataGridViewCell With {.Value = value}) : Next
        End Sub
    End Class
    Public Class Grid
        Public ReadOnly Rows As New List(Of DataGridViewRow)
    End Class
    Public Class Column
        Public Sub New(index As Integer)
            Me.Index = index
        End Sub
        Public ReadOnly Index As Integer
    End Class
    Public ReadOnly dgvStages As New Grid
    Public ReadOnly dgvExclusions As New Grid
    Public ReadOnly colStageID As New Column(0)
    Public ReadOnly colStageFirstPoint As New Column(1)
    Public ReadOnly colStageLastPoint As New Column(2)
    Public ReadOnly colStagePhase As New Column(3)
    Public ReadOnly colExclusionPoint As New Column(0)
    Public ReadOnly colExclusionScope As New Column(1)
    Public ReadOnly colExclusionReason As New Column(2)
    Public Class InputRowContext
        Public Property FirstRow As Integer
        Public Property LastRow As Integer
        Public Property PointIndexByRow As Integer()
        Public Property PointCount As Integer
        Public Property IsGrouped As Boolean
        Public ReadOnly Property RowCount As Integer
            Get
                Return LastRow - FirstRow + 1
            End Get
        End Property
    End Class
    Public Class PointAssignments
        Public Property StageIds As String()
        Public Property Phases As SpcPhase()
    End Class
    Public Class PointExclusions
        Public Property Scopes As SpcExclusionScope()
        Public Property Reasons As String()
    End Class
    Public Function BuildPointAssignments(pointCount As Integer) As PointAssignments
        Dim stageIds(pointCount - 1) As String
        Dim phases(pointCount - 1) As SpcPhase
        Dim assigned(pointCount - 1) As Boolean
        Dim stageCount As Integer = 0
        Dim usedIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataGridViewRow In dgvStages.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            stageCount += 1
            Dim id As String = CellText(row, colStageID.Index)
            If id.Length = 0 Then Throw New ArgumentException("Every stage requires a nonblank stage ID.")
            If Not usedIds.Add(id) Then Throw New ArgumentException("Stage ID '" & id & "' is duplicated.")
            Dim firstPoint As Integer = RequiredIntegerCell(row, colStageFirstPoint.Index, "First point")
            Dim lastPoint As Integer = RequiredIntegerCell(row, colStageLastPoint.Index, "Last point")
            If firstPoint < 1 OrElse lastPoint < firstPoint OrElse lastPoint > pointCount Then
                Throw New ArgumentException("Stage '" & id & "' must use a valid one-based point range within 1 to " & pointCount.ToString() & ".")
            End If
            Dim phase As SpcPhase = ParsePhaseText(CellText(row, colStagePhase.Index))
            For point As Integer = firstPoint - 1 To lastPoint - 1
                If assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " belongs to more than one stage.")
                assigned(point) = True
                stageIds(point) = id
                phases(point) = phase
            Next
        Next
        If stageCount = 0 Then
            For point As Integer = 0 To pointCount - 1
                stageIds(point) = "Stage1"
                phases(point) = SpcPhase.PhaseI
            Next
        Else
            For point As Integer = 0 To pointCount - 1
                If Not assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " is not covered by the stage grid.")
            Next
        End If
        Return New PointAssignments With {.StageIds = stageIds, .Phases = phases}
    End Function

    Public Function BuildPointExclusions(pointCount As Integer) As PointExclusions
        Dim scopes(pointCount - 1) As SpcExclusionScope
        Dim reasonSets(pointCount - 1) As HashSet(Of String)
        For i As Integer = 0 To pointCount - 1
            reasonSets(i) = New HashSet(Of String)(StringComparer.Ordinal)
        Next
        For Each row As DataGridViewRow In dgvExclusions.Rows
            If row.IsNewRow OrElse GridRowIsBlank(row) Then Continue For
            Dim point As Integer = RequiredIntegerCell(row, colExclusionPoint.Index, "Exclusion point")
            If point < 1 OrElse point > pointCount Then Throw New ArgumentException("Exclusion point must be between 1 and " & pointCount.ToString() & ".")
            scopes(point - 1) = scopes(point - 1) Or ParseExclusionScope(CellText(row, colExclusionScope.Index))
            Dim reason As String = CellText(row, colExclusionReason.Index)
            If reason.Length > 0 Then reasonSets(point - 1).Add(reason)
        Next
        Dim reasons(pointCount - 1) As String
        For i As Integer = 0 To reasons.Length - 1
            reasons(i) = String.Join("; ", reasonSets(i).ToArray())
        Next
        Return New PointExclusions With {.Scopes = scopes, .Reasons = reasons}
    End Function

    Public Shared Sub NormalizeGroupedMetadata(ByRef labels As String(), ByRef sequences As Double(), context As InputRowContext)
        If labels IsNot Nothing Then
            Dim collapsed(context.PointCount - 1) As String
            For point As Integer = 0 To context.PointCount - 1
                collapsed(point) = CollapsedTextValue(labels, context.PointIndexByRow, point, "label")
            Next
            For row As Integer = 0 To labels.Length - 1
                labels(row) = collapsed(context.PointIndexByRow(row))
            Next
        End If
        If sequences IsNot Nothing Then
            Dim collapsed(context.PointCount - 1) As Double
            For point As Integer = 0 To context.PointCount - 1
                Dim found As Nullable(Of Double) = Nothing
                For row As Integer = 0 To sequences.Length - 1
                    If context.PointIndexByRow(row) <> point OrElse Double.IsNaN(sequences(row)) Then Continue For
                    If found.HasValue AndAlso sequences(row) <> found.Value Then
                        Throw New ArgumentException("Rows belonging to subgroup point " & (point + 1).ToString() & " contain inconsistent sequence values.")
                    End If
                    found = sequences(row)
                Next
                collapsed(point) = If(found.HasValue, found.Value, Double.NaN)
            Next
            For row As Integer = 0 To sequences.Length - 1
                sequences(row) = collapsed(context.PointIndexByRow(row))
            Next
        End If
    End Sub

    Public Shared Sub GenerateStageIdsFromPhaseRuns(stageIds As String(), phases As SpcPhase())
        Dim phaseCounts As New Dictionary(Of SpcPhase, Integer)()
        Dim start As Integer = 0
        While start < phases.Length
            Dim phase As SpcPhase = phases(start)
            Dim finish As Integer = start
            While finish + 1 < phases.Length AndAlso phases(finish + 1) = phase
                finish += 1
            End While
            Dim number As Integer = If(phaseCounts.ContainsKey(phase), phaseCounts(phase) + 1, 1)
            phaseCounts(phase) = number
            Dim baseId As String = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
            Dim id As String = If(number = 1, baseId, baseId & "-" & number.ToString(CultureInfo.InvariantCulture))
            For i As Integer = start To finish
                stageIds(i) = id
            Next
            start = finish + 1
        End While
    End Sub

    Public Shared Function GridRowIsBlank(row As DataGridViewRow) As Boolean
        For Each cell As DataGridViewCell In row.Cells
            If cell.Value IsNot Nothing AndAlso Convert.ToString(cell.Value, CultureInfo.CurrentCulture).Trim().Length > 0 Then Return False
        Next
        Return True
    End Function

    Public Shared Function CellText(row As DataGridViewRow, columnIndex As Integer) As String
        Dim value As Object = row.Cells(columnIndex).Value
        Return If(value Is Nothing, String.Empty, Convert.ToString(value, CultureInfo.CurrentCulture).Trim())
    End Function

    Public Shared Function RequiredIntegerCell(row As DataGridViewRow, columnIndex As Integer, label As String) As Integer
        Dim value As Integer
        If Not Integer.TryParse(CellText(row, columnIndex), NumberStyles.Integer, CultureInfo.CurrentCulture, value) Then
            Throw New ArgumentException(label & " must be an integer.")
        End If
        Return value
    End Function

    Public Shared Function RequiredDoubleCell(row As DataGridViewRow, columnIndex As Integer, label As String) As Double
        Dim value As Double
        If Not TryParseDouble(CellText(row, columnIndex), value) Then Throw New ArgumentException(label & " must be numeric.")
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentException(label & " must be finite.")
        Return value
    End Function

    Public Shared Function TryParseDouble(text As String, ByRef value As Double) As Boolean
        If Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, value) Then Return True
        Return Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, value)
    End Function

    Public Shared Function ParsePhaseText(text As String) As SpcPhase
        Dim normalized As String = text.Trim().Replace(" ", String.Empty).ToUpperInvariant()
        If normalized = "PHASEI" OrElse normalized = "I" OrElse normalized = "1" Then Return SpcPhase.PhaseI
        If normalized = "PHASEII" OrElse normalized = "II" OrElse normalized = "2" Then Return SpcPhase.PhaseII
        Throw New ArgumentException("Phase must be 'Phase I' or 'Phase II'.")
    End Function

    Public Shared Function ParseCollapsedPhase(values As String(), pointIndices As Integer(), point As Integer) As SpcPhase
        Dim found As Nullable(Of SpcPhase) = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As SpcPhase = ParsePhaseText(values(i))
            If found.HasValue AndAlso found.Value <> current Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent phase values.")
            End If
            found = current
        Next
        If Not found.HasValue Then Throw New ArgumentException("No phase value is available for chart point " & (point + 1).ToString() & ".")
        Return found.Value
    End Function

    Public Shared Function CollapsedTextValue(values As String(), pointIndices As Integer(), point As Integer, label As String) As String
        Dim found As String = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As String = values(i).Trim()
            If found Is Nothing Then
                found = current
            ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent " & label & " values.")
            End If
        Next
        Return If(found, String.Empty)
    End Function

    Public Shared Function ParseExclusionScope(text As String) As SpcExclusionScope
        Select Case text.Trim().ToLowerInvariant()
            Case "parameter estimation" : Return SpcExclusionScope.ParameterEstimation
            Case "signal evaluation", "rule evaluation" : Return SpcExclusionScope.RuleEvaluation
            Case "parameter estimation and signal evaluation", "parameter estimation and rules" : Return SpcExclusionScope.EstimationAndRules
            Case Else : Throw New ArgumentException("Select a valid exclusion scope.")
        End Select
    End Function

    Public Shared Function IsExclusionIndicator(value As Object) As Boolean
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return DirectCast(value, Boolean)
        Dim numeric As Double
        If TryParseDouble(Convert.ToString(value, CultureInfo.CurrentCulture), numeric) Then Return numeric <> 0R
        Select Case Convert.ToString(value, CultureInfo.CurrentCulture).Trim().ToLowerInvariant()
            Case "true", "yes", "y", "x", "exclude", "excluded" : Return True
            Case "false", "no", "n", "include", "included", "" : Return False
            Case Else : Throw New ArgumentException("Exclusion indicators must be blank/0/No or nonzero/Yes/True/X.")
        End Select
    End Function
    Public Shared Function Map(firstRow As Integer, lastRow As Integer, grouped As Boolean, ids As String()) As InputRowContext
        Dim rowCount = lastRow - firstRow + 1
        Dim pointIndices(rowCount - 1) As Integer
        Dim pointCount As Integer
        If grouped Then
            Dim subgroupIds As String() = ids
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For row As Integer = 0 To subgroupIds.Length - 1
                Dim id As String = subgroupIds(row).Trim()
                If id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & (firstRow + row).ToString() & ".")
                Dim point As Integer
                If Not groups.TryGetValue(id, point) Then
                    point = groups.Count
                    groups.Add(id, point)
                End If
                pointIndices(row) = point
            Next
            pointCount = groups.Count
        Else
            For row As Integer = 0 To rowCount - 1
                pointIndices(row) = row
            Next
            pointCount = rowCount
        End If
        Return New InputRowContext With {.FirstRow=firstRow,.LastRow=lastRow,.IsGrouped=grouped,.PointCount=pointCount,.PointIndexByRow=pointIndices}
    End Function
    Public Sub Imported(context As InputRowContext, rowStages As String(), rowPhases As String())
            Dim pointStages(context.PointCount - 1) As String
            Dim pointPhases(context.PointCount - 1) As SpcPhase
            For point As Integer = 0 To context.PointCount - 1
                pointPhases(point) = If(rowPhases Is Nothing, SpcPhase.PhaseI,
                                        ParseCollapsedPhase(rowPhases, context.PointIndexByRow, point))
                If rowStages IsNot Nothing Then
                    pointStages(point) = CollapsedTextValue(rowStages, context.PointIndexByRow, point, "stage identifier")
                    If pointStages(point).Length = 0 Then Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
            Next
            If rowStages Is Nothing Then GenerateStageIdsFromPhaseRuns(pointStages, pointPhases)

            dgvStages.Rows.Clear()
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart As Integer = 0
            For point As Integer = 1 To context.PointCount
                Dim atEnd As Boolean = point = context.PointCount
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(pointStages(point), pointStages(runStart), StringComparison.Ordinal) OrElse
                     pointPhases(point) <> pointPhases(runStart))
                If Not atEnd AndAlso Not changed Then Continue For
                Dim stageId As String = pointStages(runStart).Trim()
                If Not used.Add(stageId) Then Throw New ArgumentException("Stage '" & stageId & "' occurs in more than one non-contiguous block.")
                AddStageRow(stageId, runStart + 1, point, pointPhases(runStart))
                runStart = point
            Next
    End Sub
    Public Sub AddStageRow(id As String, first As Integer, last As Integer, phase As SpcPhase)
        dgvStages.Rows.Add(New DataGridViewRow(id,first,last,If(phase=SpcPhase.PhaseI,"Phase I","Phase II")))
    End Sub
    Public Sub Quick(context As InputRowContext, allPhaseI As Boolean, allPhaseII As Boolean, lastPhaseIValue As Integer, modelSource As SpcMultivariateModelSource)
            If allPhaseI Then
                AddStageRow("PhaseI", 1, context.PointCount, SpcPhase.PhaseI)
            ElseIf allPhaseII Then
                If modelSource <>
                   SpcMultivariateModelSource.UseHistoricalParameters Then
                    Throw New ArgumentException("All-Phase-II monitoring requires historical parameters.")
                End If
                AddStageRow("PhaseII", 1, context.PointCount, SpcPhase.PhaseII)
            Else
                Dim lastPhaseI As Integer = lastPhaseIValue
                If lastPhaseI < 1 OrElse lastPhaseI >= context.PointCount Then
                    Throw New ArgumentException("The last Phase I point must be between 1 and " &
                                                (context.PointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                AddStageRow("PhaseI", 1, lastPhaseI, SpcPhase.PhaseI)
                AddStageRow("PhaseII", lastPhaseI + 1, context.PointCount, SpcPhase.PhaseII)
            End If
    End Sub
    Public Class RowArrays
        Public Phases As SpcPhase()
        Public Stages As String()
        Public Scopes As SpcExclusionScope()
        Public Reasons As String()
        Public SourceRows As Integer()
    End Class
    Public Shared Function Expand(context As InputRowContext, pointAssignments As PointAssignments, pointExclusions As PointExclusions) As RowArrays
        Dim phases(context.RowCount - 1) As SpcPhase
        Dim stages(context.RowCount - 1) As String
        Dim scopes(context.RowCount - 1) As SpcExclusionScope
        Dim reasons(context.RowCount - 1) As String
        Dim sourceRows(context.RowCount - 1) As Integer
        For row As Integer = 0 To context.RowCount - 1
            Dim point As Integer = context.PointIndexByRow(row)
            phases(row) = pointAssignments.Phases(point)
            stages(row) = pointAssignments.StageIds(point)
            scopes(row) = pointExclusions.Scopes(point)
            reasons(row) = pointExclusions.Reasons(point)
            sourceRows(row) = context.FirstRow + row
        Next

        Return New RowArrays With {.Phases=phases,.Stages=stages,.Scopes=scopes,.Reasons=reasons,.SourceRows=sourceRows}
    End Function
    Public Class RawOptions
        Public MissingValuePolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject
        Public ModelSource As SpcMultivariateModelSource
        Public Alpha As Double = 0.0027R
        Public LowerLimit As Boolean
        Public Ridge As Double
        Public AllowPseudo As Boolean = True
        Public Correlation As Boolean
        Public FixedComponents As Boolean
        Public Components As Integer = 1
        Public CumulativePercent As Double = 90
        Public Lambda As Double = 0.2R
        Public SuppliedMewma As Boolean
        Public MewmaLimit As Double = 12
        Public K As Double = 0.5R
        Public H As Double = 5.5R
        Public SuppliedGv As Boolean
        Public GvSigma As Double = 3
        Public ResetStage As Boolean = True
        Public ResetPhase As Boolean = True
        Public ResetSignal As Boolean
        Public RequestLabel As String = "Multivariate SPC fixture"
        Public Gap As SpcSequenceGapBehavior
    End Class
    Public Shared Function Compose(chart As SpcChartType, measurements As Double(,), variableNames As String(), subgroupIds As String(), labels As String(), phases As SpcPhase(), stages As String(), sequences As Double(), sourceRows As Integer(), scopes As SpcExclusionScope(), reasons As String(), historicalMean As Double(), historicalCovariance As Double(,), options As RawOptions) As SpcMultivariateRequest
        Return New SpcMultivariateRequest(
            chart,
            measurements,
            variableNames:=variableNames,
            subgroupIds:=subgroupIds,
            labels:=labels,
            phases:=phases,
            stageIds:=stages,
            sequenceValues:=sequences,
            sourceRowIndices:=sourceRows,
            exclusionScopes:=scopes,
            exclusionReasons:=reasons,
            missingValuePolicy:=options.MissingValuePolicy,
            modelSource:=options.ModelSource,
            historicalMean:=historicalMean,
            historicalCovariance:=historicalCovariance,
            controlLimitAlpha:=options.Alpha,
            useLowerHotellingLimit:=options.LowerLimit,
            covarianceRegularization:=options.Ridge,
            allowPseudoInverse:=options.AllowPseudo,
            pcaUseCorrelationMatrix:=options.Correlation,
            pcaComponentCount:=If(options.FixedComponents, CType(options.Components,Integer?), Nothing),
            pcaCumulativeVariance:=options.CumulativePercent / 100.0R,
            mewmaLambda:=options.Lambda,
            mewmaControlLimit:=If(options.SuppliedMewma, CType(options.MewmaLimit,Double?), Nothing),
            mcusumReferenceValue:=options.K,
            mcusumDecisionInterval:=options.H,
            generalizedVarianceSigmaMultiplier:=If(options.SuppliedGv, CType(options.GvSigma,Double?), Nothing),
            resetAtStageBoundary:=options.ResetStage,
            resetAtPhaseBoundary:=options.ResetPhase,
            resetAfterSignal:=options.ResetSignal,
            requestLabel:=options.RequestLabel,
            sequenceGapBehavior:=options.Gap)
    End Function
    Public Shared Function RenderingInput(panel As SpcPanelResult) As SpcInputData
        Dim points As SpcPointResult() = panel.Points
        Dim values(points.Length - 1) As Double
        Dim labels(points.Length - 1) As String
        Dim sequences(points.Length - 1) As Double
        Dim sourceRows(points.Length - 1) As Integer
        For i As Integer = 0 To points.Length - 1
            values(i) = points(i).Value
            labels(i) = points(i).Label
            sequences(i) = If(points(i).SequenceValue.HasValue, points(i).SequenceValue.Value, Double.NaN)
            Dim rows As Integer() = points(i).SourceRowIndices
            sourceRows(i) = If(rows.Length > 0, rows(0), points(i).PointIndex)
        Next
        Return SpcInputData.FromIndividualSequence(values, labels, sequences, sourceRows, "Statistic")
    End Function
    Public Shared Function SelectedDiagnostics(result As SpcMultivariateFitResult, signalOnly As Boolean) As List(Of SpcMultivariatePointDiagnostic)
        Dim diagnostics As SpcMultivariatePointDiagnostic() = result.Diagnostics
        Dim panel As SpcPanelResult = result.Panels(0)
        Dim selected As New List(Of SpcMultivariatePointDiagnostic)()
        For Each diagnostic As SpcMultivariatePointDiagnostic In diagnostics
            Dim point As SpcPointResult = panel.GetPoint(diagnostic.PointIndex)
            If Not signalOnly OrElse (point IsNot Nothing AndAlso point.IsSignalled) Then selected.Add(diagnostic)
        Next

        Return selected
    End Function
    Public Property SignalOnly As Boolean
    Public Class AuditControlSnapshot
        Public Text As String = ""
        Public Checked As Boolean
        Public Value As Decimal
    End Class
    Public ReadOnly tbChartTitle As New AuditControlSnapshot
    Public ReadOnly tbValueAxisTitle As New AuditControlSnapshot
    Public ReadOnly tbHorizontalAxisTitle As New AuditControlSnapshot
    Public ReadOnly chkUseSequenceValuesForHorizontalAxis As New AuditControlSnapshot
    Public ReadOnly spinChartWidth As New AuditControlSnapshot With {.Value=760D}
    Public ReadOnly spinChartHeight As New AuditControlSnapshot With {.Value=360D}
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
        Dim diagnostics As SpcMultivariatePointDiagnostic() = result.Diagnostics
        Dim signalOnly As Boolean = SignalOnly
        Dim panel As SpcPanelResult = result.Panels(0)
        Dim selected As New List(Of SpcMultivariatePointDiagnostic)()
        For Each diagnostic As SpcMultivariatePointDiagnostic In diagnostics
            Dim point As SpcPointResult = panel.GetPoint(diagnostic.PointIndex)
            If Not signalOnly OrElse (point IsNot Nothing AndAlso point.IsSignalled) Then selected.Add(diagnostic)
        Next

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

    Public Shared Function ExclusionScopeText(scope As SpcExclusionScope) As String
        Select Case scope
            Case SpcExclusionScope.None : Return "None"
            Case SpcExclusionScope.ParameterEstimation : Return "Parameter estimation"
            Case SpcExclusionScope.RuleEvaluation : Return "Signal evaluation"
            Case SpcExclusionScope.EstimationAndRules : Return "Parameter estimation and signal evaluation"
            Case Else : Return scope.ToString()
        End Select
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
End Class
