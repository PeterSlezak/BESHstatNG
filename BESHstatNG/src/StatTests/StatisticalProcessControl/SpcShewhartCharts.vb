Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Calculates the host-neutral Shewhart charts in the first SPC milestone.
    ''' </summary>
    ''' <remarks>
    ''' This class contains no Excel, Excel-DNA, chart-rendering, or WinForms code.
    ''' It is discovered automatically by <see cref="SpcEngine"/> through the
    ''' <see cref="ISpcChartCalculator"/> contract.
    ''' </remarks>
    Friend NotInheritable Class SpcShewhartChartCalculator
        Implements ISpcChartCalculator

        Private Shared ReadOnly SupportedChartTypes As SpcChartType() = {
            SpcChartType.Individuals,
            SpcChartType.MovingRange,
            SpcChartType.IndividualsMovingRange,
            SpcChartType.XBar,
            SpcChartType.SubgroupRange,
            SpcChartType.SubgroupStandardDeviation,
            SpcChartType.XBarR,
            SpcChartType.XBarS,
            SpcChartType.PChart,
            SpcChartType.NpChart,
            SpcChartType.CChart,
            SpcChartType.UChart
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
                    "The Shewhart calculator does not support " & request.ChartType.ToString() & ".")
            End If

            CheckCancellation(cancellationRequested)

            Dim options As SpcAnalysisOptions = request.AnalysisOptions
            Dim warnings As New List(Of String)()
            ValidateCalculatorOptions(request, options)

            Dim rawPoints As RawChartPoint()
            If IsAttributeChart(request.ChartType) Then
                rawPoints = BuildAttributePoints(request, options, warnings, cancellationRequested)
            Else
                rawPoints = BuildVariablePoints(request, options, warnings, cancellationRequested)
            End If

            Dim stages As StageContext() = BuildStages(request, rawPoints.Length)
            ApplyStageAssignments(rawPoints, stages)
            ApplyExclusions(rawPoints, request.Exclusions)

            If CountRetainedPoints(rawPoints) = 0 Then
                Throw New ArgumentException("No chart points remain after applying the missing-value policy.")
            End If

            Dim panels As New List(Of SpcPanelResult)()
            If IsAttributeChart(request.ChartType) Then
                Dim attributeCache As New Dictionary(Of String, StageParameters)(
                    StringComparer.OrdinalIgnoreCase)
                Dim visiting As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                panels.Add(BuildAttributePanel(request,
                                               options,
                                               rawPoints,
                                               stages,
                                               attributeCache,
                                               visiting,
                                               cancellationRequested))
            Else
                Dim variableCache As New Dictionary(Of String, StageParameters)(
                    StringComparer.OrdinalIgnoreCase)
                Dim visiting As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                Select Case request.ChartType
                    Case SpcChartType.Individuals
                        panels.Add(BuildIndividualsPanel(request,
                                                         options,
                                                         rawPoints,
                                                         stages,
                                                         variableCache,
                                                         visiting,
                                                         cancellationRequested))

                    Case SpcChartType.MovingRange
                        panels.Add(BuildMovingRangePanel(request,
                                                         options,
                                                         rawPoints,
                                                         stages,
                                                         variableCache,
                                                         visiting,
                                                         cancellationRequested))

                    Case SpcChartType.IndividualsMovingRange
                        panels.Add(BuildIndividualsPanel(request,
                                                         options,
                                                         rawPoints,
                                                         stages,
                                                         variableCache,
                                                         visiting,
                                                         cancellationRequested))
                        panels.Add(BuildMovingRangePanel(request,
                                                         options,
                                                         rawPoints,
                                                         stages,
                                                         variableCache,
                                                         visiting,
                                                         cancellationRequested))

                    Case SpcChartType.XBar
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupMean,
                                                      cancellationRequested))

                    Case SpcChartType.SubgroupRange
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupRange,
                                                      cancellationRequested))

                    Case SpcChartType.SubgroupStandardDeviation
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupStandardDeviation,
                                                      cancellationRequested))

                    Case SpcChartType.XBarR
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupMean,
                                                      cancellationRequested))
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupRange,
                                                      cancellationRequested))

                    Case SpcChartType.XBarS
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupMean,
                                                      cancellationRequested))
                        panels.Add(BuildSubgroupPanel(request,
                                                      options,
                                                      rawPoints,
                                                      stages,
                                                      variableCache,
                                                      visiting,
                                                      SpcPanelType.SubgroupStandardDeviation,
                                                      cancellationRequested))
                End Select
            End If

            CheckCancellation(cancellationRequested)
            Return New SpcCalculationResult(panels.ToArray(), warnings.ToArray())
        End Function

        Private Shared Sub ValidateCalculatorOptions(request As SpcFitRequest,
                                                     options As SpcAnalysisOptions)
            If options.ControlLimits.Method = SpcControlLimitMethod.ExactProbability AndAlso
               Not IsAttributeChart(request.ChartType) Then
                Throw New NotSupportedException(
                    "Exact-probability limits apply only to p, np, c, and u charts. " &
                    "Use Shewhart sigma limits for variable charts.")
            End If

            If (request.ChartType = SpcChartType.Individuals OrElse
                request.ChartType = SpcChartType.MovingRange OrElse
                request.ChartType = SpcChartType.IndividualsMovingRange) AndAlso
               options.ControlLimits.MovingRangeLength > 25 Then
                Throw New ArgumentOutOfRangeException(
                    "MovingRangeLength",
                    "Moving-range constants are supported for lengths from 2 through 25.")
            End If
        End Sub

        Private Shared Function IsAttributeChart(chartType As SpcChartType) As Boolean
            Return chartType = SpcChartType.PChart OrElse
                   chartType = SpcChartType.NpChart OrElse
                   chartType = SpcChartType.CChart OrElse
                   chartType = SpcChartType.UChart
        End Function

        Private Shared Function IsIndividualChart(chartType As SpcChartType) As Boolean
            Return chartType = SpcChartType.Individuals OrElse
                   chartType = SpcChartType.MovingRange OrElse
                   chartType = SpcChartType.IndividualsMovingRange
        End Function

        Private Shared Function RequiresSubgroupSizeTwo(chartType As SpcChartType) As Boolean
            Return chartType = SpcChartType.SubgroupRange OrElse
                   chartType = SpcChartType.SubgroupStandardDeviation OrElse
                   chartType = SpcChartType.XBarR OrElse
                   chartType = SpcChartType.XBarS
        End Function

#Region "Input construction"

        Private Shared Function BuildVariablePoints(request As SpcFitRequest,
                                                    options As SpcAnalysisOptions,
                                                    warnings As List(Of String),
                                                    cancellationRequested As Func(Of Boolean)) As RawChartPoint()
            If IsIndividualChart(request.ChartType) Then
                Return BuildIndividualPoints(request, options, warnings, cancellationRequested)
            End If

            Select Case request.DataLayout
                Case SpcDataLayout.WideSubgroups
                    Return BuildWideSubgroupPoints(request,
                                                   options,
                                                   warnings,
                                                   cancellationRequested)
                Case SpcDataLayout.StackedObservations
                    Return BuildStackedSubgroupPoints(request,
                                                      options,
                                                      warnings,
                                                      cancellationRequested)
                Case Else
                    Throw New ArgumentException(
                        "Variable subgroup charts require wide or stacked subgroup data.")
            End Select
        End Function

        Private Shared Function BuildIndividualPoints(request As SpcFitRequest,
                                                      options As SpcAnalysisOptions,
                                                      warnings As List(Of String),
                                                      cancellationRequested As Func(Of Boolean)) As RawChartPoint()
            Dim data As SpcInputData = request.Data
            Dim measurements As Double(,) = data.Measurements
            If measurements Is Nothing OrElse measurements.GetLength(1) <> 1 Then
                Throw New ArgumentException(
                    "Individual charts require exactly one measurement column.")
            End If

            Dim labels As String() = data.Labels
            Dim sequenceValues As Double() = data.SequenceValues
            Dim sourceRows As Integer() = data.SourceRowIndices
            Dim result(data.RowCount - 1) As RawChartPoint
            Dim omittedCount As Integer = 0

            For row As Integer = 0 To data.RowCount - 1
                CheckCancellationPeriodically(row, cancellationRequested)
                Dim value As Double = measurements(row, 0)
                Dim omitted As Boolean = Double.IsNaN(value)
                If omitted AndAlso options.MissingValuePolicy = SpcMissingValuePolicy.Reject Then
                    Throw New ArgumentException(
                        "A missing individual measurement was found at point " &
                        (row + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If
                If omitted Then omittedCount += 1

                result(row) = New RawChartPoint With {
                    .LogicalIndex = row,
                    .Value = value,
                    .Label = GetRowLabel(labels, row),
                    .SequenceValue = GetSequenceValue(sequenceValues, row),
                    .SourceRowIndices = {sourceRows(row)},
                    .IsOmitted = omitted
                }
            Next

            AddOmissionWarning(warnings, omittedCount)
            Return result
        End Function

        Private Shared Function BuildWideSubgroupPoints(request As SpcFitRequest,
                                                        options As SpcAnalysisOptions,
                                                        warnings As List(Of String),
                                                        cancellationRequested As Func(Of Boolean)) As RawChartPoint()
            Dim data As SpcInputData = request.Data
            Dim measurements As Double(,) = data.Measurements
            If measurements Is Nothing Then
                Throw New ArgumentException("Subgroup measurements are required.")
            End If

            Dim labels As String() = data.Labels
            Dim sequenceValues As Double() = data.SequenceValues
            Dim sourceRows As Integer() = data.SourceRowIndices
            Dim columnCount As Integer = measurements.GetLength(1)
            Dim result(data.RowCount - 1) As RawChartPoint
            Dim omittedCount As Integer = 0

            For row As Integer = 0 To data.RowCount - 1
                CheckCancellationPeriodically(row, cancellationRequested)
                Dim values As New List(Of Double)(columnCount)
                Dim hasMissing As Boolean = False
                For column As Integer = 0 To columnCount - 1
                    Dim value As Double = measurements(row, column)
                    If Double.IsNaN(value) Then
                        hasMissing = True
                    Else
                        values.Add(value)
                    End If
                Next

                Dim omitted As Boolean = False
                If hasMissing Then
                    Select Case options.MissingValuePolicy
                        Case SpcMissingValuePolicy.Reject
                            Throw New ArgumentException(
                                "A missing subgroup measurement was found at point " &
                                (row + 1).ToString(CultureInfo.InvariantCulture) & ".")
                        Case SpcMissingValuePolicy.OmitPoint
                            omitted = True
                        Case SpcMissingValuePolicy.UseAvailableMeasurements
                            omitted = values.Count = 0
                    End Select
                End If

                If Not omitted AndAlso
                   RequiresSubgroupSizeTwo(request.ChartType) AndAlso
                   values.Count < 2 Then
                    Throw New ArgumentException(
                        "Point " & (row + 1).ToString(CultureInfo.InvariantCulture) &
                        " has fewer than two usable measurements; this chart requires at least two per subgroup.")
                End If

                Dim subgroup As SpcSubgroupStatistics = Nothing
                If Not omitted Then subgroup = SpcStatistics.CalculateSubgroup(values.ToArray())
                If omitted Then omittedCount += 1

                result(row) = New RawChartPoint With {
                    .LogicalIndex = row,
                    .Subgroup = subgroup,
                    .Label = GetRowLabel(labels, row),
                    .SequenceValue = GetSequenceValue(sequenceValues, row),
                    .SourceRowIndices = {sourceRows(row)},
                    .IsOmitted = omitted
                }
            Next

            AddOmissionWarning(warnings, omittedCount)
            Return result
        End Function

        Private Shared Function BuildStackedSubgroupPoints(request As SpcFitRequest,
                                                           options As SpcAnalysisOptions,
                                                           warnings As List(Of String),
                                                           cancellationRequested As Func(Of Boolean)) As RawChartPoint()
            Dim data As SpcInputData = request.Data
            Dim measurements As Double(,) = data.Measurements
            If measurements Is Nothing OrElse measurements.GetLength(1) <> 1 Then
                Throw New ArgumentException(
                    "Stacked subgroup data require exactly one measurement column.")
            End If

            Dim subgroupIds As String() = data.SubgroupIds
            If subgroupIds Is Nothing Then
                Throw New ArgumentException("Stacked subgroup identifiers are required.")
            End If

            Dim labels As String() = data.Labels
            Dim sequenceValues As Double() = data.SequenceValues
            Dim sourceRows As Integer() = data.SourceRowIndices
            Dim groups As New List(Of StackedGroup)()
            Dim groupMap As New Dictionary(Of String, StackedGroup)(StringComparer.Ordinal)

            For row As Integer = 0 To data.RowCount - 1
                CheckCancellationPeriodically(row, cancellationRequested)
                Dim subgroupId As String = If(subgroupIds(row), String.Empty).Trim()
                If subgroupId.Length = 0 Then
                    Throw New ArgumentException(
                        "A stacked subgroup identifier is missing at input row " &
                        (row + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If

                Dim group As StackedGroup = Nothing
                If Not groupMap.TryGetValue(subgroupId, group) Then
                    group = New StackedGroup(subgroupId,
                                             GetRowLabel(labels, row),
                                             GetSequenceValue(sequenceValues, row))
                    groupMap.Add(subgroupId, group)
                    groups.Add(group)
                End If

                group.SourceRows.Add(sourceRows(row))
                Dim value As Double = measurements(row, 0)
                If Double.IsNaN(value) Then
                    group.HasMissing = True
                Else
                    group.Values.Add(value)
                End If
            Next

            Dim result(groups.Count - 1) As RawChartPoint
            Dim omittedCount As Integer = 0
            For pointIndex As Integer = 0 To groups.Count - 1
                CheckCancellationPeriodically(pointIndex, cancellationRequested)
                Dim group As StackedGroup = groups(pointIndex)
                Dim omitted As Boolean = False
                If group.HasMissing Then
                    Select Case options.MissingValuePolicy
                        Case SpcMissingValuePolicy.Reject
                            Throw New ArgumentException(
                                "Subgroup '" & group.SubgroupId & "' contains a missing measurement.")
                        Case SpcMissingValuePolicy.OmitPoint
                            omitted = True
                        Case SpcMissingValuePolicy.UseAvailableMeasurements
                            omitted = group.Values.Count = 0
                    End Select
                End If

                If Not omitted AndAlso
                   RequiresSubgroupSizeTwo(request.ChartType) AndAlso
                   group.Values.Count < 2 Then
                    Throw New ArgumentException(
                        "Subgroup '" & group.SubgroupId &
                        "' has fewer than two usable measurements; this chart requires at least two.")
                End If

                Dim subgroup As SpcSubgroupStatistics = Nothing
                If Not omitted Then
                    subgroup = SpcStatistics.CalculateSubgroup(group.Values.ToArray())
                Else
                    omittedCount += 1
                End If

                Dim pointLabel As String = group.Label
                If pointLabel.Length = 0 Then pointLabel = group.SubgroupId
                result(pointIndex) = New RawChartPoint With {
                    .LogicalIndex = pointIndex,
                    .Subgroup = subgroup,
                    .Label = pointLabel,
                    .SequenceValue = group.SequenceValue,
                    .SourceRowIndices = group.SourceRows.ToArray(),
                    .IsOmitted = omitted
                }
            Next

            AddOmissionWarning(warnings, omittedCount)
            Return result
        End Function

        Private Shared Function BuildAttributePoints(request As SpcFitRequest,
                                                     options As SpcAnalysisOptions,
                                                     warnings As List(Of String),
                                                     cancellationRequested As Func(Of Boolean)) As RawChartPoint()
            Dim data As SpcInputData = request.Data
            Dim counts As Double() = data.Counts
            If counts Is Nothing Then Throw New ArgumentException("Attribute counts are required.")

            Dim sampleSizes As Double() = data.SampleSizes
            Dim exposures As Double() = data.Exposures
            Dim labels As String() = data.Labels
            Dim sequenceValues As Double() = data.SequenceValues
            Dim sourceRows As Integer() = data.SourceRowIndices
            Dim result(data.RowCount - 1) As RawChartPoint
            Dim omittedCount As Integer = 0

            For row As Integer = 0 To data.RowCount - 1
                CheckCancellationPeriodically(row, cancellationRequested)
                Dim count As Double = counts(row)
                Dim sampleSize As Double = If(sampleSizes Is Nothing, Double.NaN, sampleSizes(row))
                Dim exposure As Double = If(exposures Is Nothing, Double.NaN, exposures(row))
                Dim hasMissing As Boolean = Double.IsNaN(count)

                If request.ChartType = SpcChartType.PChart OrElse
                   request.ChartType = SpcChartType.NpChart Then
                    hasMissing = hasMissing OrElse Double.IsNaN(sampleSize)
                ElseIf request.ChartType = SpcChartType.UChart Then
                    hasMissing = hasMissing OrElse Double.IsNaN(exposure)
                End If

                If hasMissing AndAlso options.MissingValuePolicy = SpcMissingValuePolicy.Reject Then
                    Throw New ArgumentException(
                        "A required attribute-chart value is missing at point " &
                        (row + 1).ToString(CultureInfo.InvariantCulture) & ".")
                End If

                Dim omitted As Boolean = hasMissing
                If omitted Then
                    omittedCount += 1
                Else
                    ValidateCountValue(count, "Count", row)
                    If request.ChartType = SpcChartType.PChart OrElse
                       request.ChartType = SpcChartType.NpChart Then
                        ValidateCountValue(sampleSize, "Sample size", row)
                    End If
                End If

                result(row) = New RawChartPoint With {
                    .LogicalIndex = row,
                    .CountValue = count,
                    .SampleSize = sampleSize,
                    .Exposure = exposure,
                    .Label = GetRowLabel(labels, row),
                    .SequenceValue = GetSequenceValue(sequenceValues, row),
                    .SourceRowIndices = {sourceRows(row)},
                    .IsOmitted = omitted
                }
            Next

            AddOmissionWarning(warnings, omittedCount)
            Return result
        End Function

        Private Shared Sub ValidateCountValue(value As Double,
                                              valueName As String,
                                              row As Integer)
            If value <> Math.Truncate(value) Then
                Throw New ArgumentException(
                    valueName & " at point " &
                    (row + 1).ToString(CultureInfo.InvariantCulture) &
                    " must be an integer.")
            End If
        End Sub

        Private Shared Function GetRowLabel(labels As String(), row As Integer) As String
            If labels Is Nothing Then Return String.Empty
            Return If(labels(row), String.Empty).Trim()
        End Function

        Private Shared Function GetSequenceValue(values As Double(),
                                                 row As Integer) As Nullable(Of Double)
            If values Is Nothing OrElse Double.IsNaN(values(row)) Then Return Nothing
            Return values(row)
        End Function

        Private Shared Sub AddOmissionWarning(warnings As List(Of String), omittedCount As Integer)
            If omittedCount <= 0 Then Return
            warnings.Add(
                omittedCount.ToString(CultureInfo.InvariantCulture) &
                If(omittedCount = 1, " chart point was", " chart points were") &
                " omitted because required values were missing.")
        End Sub

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
                        "Stages",
                        "A stage extends beyond the number of constructed chart points. " &
                        "For stacked data, stage indices refer to grouped chart points, not source rows.")
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

        Private Shared Sub ApplyStageAssignments(points As RawChartPoint(),
                                                 stages As StageContext())
            Dim stageIndex As Integer = 0
            For pointIndex As Integer = 0 To points.Length - 1
                While pointIndex > stages(stageIndex).Definition.LastPointIndex
                    stageIndex += 1
                End While
                points(pointIndex).Stage = stages(stageIndex)
            Next
        End Sub

        Private Shared Sub ApplyExclusions(points As RawChartPoint(),
                                           exclusions As SpcExclusionDefinition())
            For i As Integer = 0 To exclusions.Length - 1
                Dim exclusion As SpcExclusionDefinition = exclusions(i)
                If exclusion.PointIndex >= points.Length Then
                    Throw New ArgumentOutOfRangeException(
                        "Exclusions",
                        "An exclusion extends beyond the number of constructed chart points. " &
                        "For stacked data, exclusions refer to grouped chart points.")
                End If
                Dim point As RawChartPoint = points(exclusion.PointIndex)
                point.ExclusionScope = point.ExclusionScope Or exclusion.Scope
                point.ExclusionReason = CombineReason(point.ExclusionReason, exclusion.Reason)
            Next
        End Sub

        Private Shared Function ResolveVariableParameters(
            stage As StageContext,
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint(),
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
                    result = EstimateVariableParameters(stage,
                                                        request,
                                                        options,
                                                        rawPoints)

                Case SpcStageLimitMode.UseHistoricalParameters
                    Dim history As SpcHistoricalParameters = FindHistoricalParameters(request,
                                                                                       stage.Definition.StageId)
                    If history Is Nothing Then
                        Throw New ArgumentException(
                            "No historical parameters were supplied for stage '" &
                            stage.Definition.StageId & "'.")
                    End If
                    If Not history.ProcessSigma.HasValue Then
                        Throw New ArgumentException(
                            "Historical process sigma is required for stage '" &
                            stage.Definition.StageId & "'.")
                    End If
                    If VariableChartRequiresMean(request.ChartType) AndAlso
                       Not history.ProcessMean.HasValue Then
                        Throw New ArgumentException(
                            "Historical process mean is required for stage '" &
                            stage.Definition.StageId & "'.")
                    End If
                    result = New StageParameters With {
                        .Center = If(history.ProcessMean.HasValue,
                                     history.ProcessMean.Value,
                                     Double.NaN),
                        .Sigma = history.ProcessSigma.Value,
                        .CenterStandardError = Double.NaN,
                        .EstimationPointCount = -1,
                        .SigmaEstimationPointCount = -1,
                        .LimitMode = SpcStageLimitMode.UseHistoricalParameters,
                        .SourceStageId = String.Empty,
                        .Method = "Historical parameters"
                    }

                Case SpcStageLimitMode.UseReferenceStage
                    Dim referenced As StageContext = FindStage(stages,
                                                               stage.Definition.ReferenceStageId)
                    Dim source As StageParameters = ResolveVariableParameters(referenced,
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

        Private Shared Function EstimateVariableParameters(
            stage As StageContext,
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint()) As StageParameters

            If IsIndividualChart(request.ChartType) Then
                Dim values(stage.Definition.PointCount - 1) As Double
                For i As Integer = 0 To values.Length - 1
                    values(i) = Double.NaN
                Next

                Dim mean As Double = 0.0
                Dim meanCount As Integer = 0
                For logicalIndex As Integer = stage.Definition.FirstPointIndex To _
                                                 stage.Definition.LastPointIndex
                    Dim point As RawChartPoint = rawPoints(logicalIndex)
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
                Dim estimate As SpcSigmaEstimate =
                    SpcStatistics.EstimateSigmaFromIndividuals(values,
                                                               limits.WithinSigmaEstimator,
                                                               limits.MovingRangeLength,
                                                               limits.UseBiasCorrection)
                Return New StageParameters With {
                    .Center = mean,
                    .Sigma = estimate.Value,
                    .CenterStandardError = estimate.Value / Math.Sqrt(CDbl(meanCount)),
                    .EstimationPointCount = meanCount,
                    .SigmaEstimationPointCount = estimate.ContributingPointCount,
                    .LimitMode = SpcStageLimitMode.EstimateFromStageData,
                    .SourceStageId = String.Empty,
                    .Method = estimate.Method
                }
            End If

            Dim subgroups As New List(Of SpcSubgroupStatistics)()
            Dim pooledMean As Double = 0.0
            Dim totalMeasurements As Integer = 0
            For logicalIndex As Integer = stage.Definition.FirstPointIndex To _
                                             stage.Definition.LastPointIndex
                Dim point As RawChartPoint = rawPoints(logicalIndex)
                If IsEstimationEligible(point) Then
                    subgroups.Add(point.Subgroup)
                    Dim newTotal As Integer = totalMeasurements + point.Subgroup.Count
                    pooledMean += (point.Subgroup.Mean - pooledMean) *
                                  CDbl(point.Subgroup.Count) / CDbl(newTotal)
                    totalMeasurements = newTotal
                End If
            Next
            If subgroups.Count = 0 Then
                Throw New ArgumentException(
                    "Stage '" & stage.Definition.StageId &
                    "' has no subgroups eligible for parameter estimation.")
            End If

            Dim selectedEstimator As SpcWithinSigmaEstimator =
                ResolveSubgroupEstimator(request.ChartType,
                                         options.ControlLimits.WithinSigmaEstimator)
            Dim sigmaEstimate As SpcSigmaEstimate =
                SpcStatistics.EstimateSigmaFromSubgroups(
                    subgroups.ToArray(),
                    selectedEstimator,
                    options.ControlLimits.UseBiasCorrection)

            Return New StageParameters With {
                .Center = pooledMean,
                .Sigma = sigmaEstimate.Value,
                .CenterStandardError = sigmaEstimate.Value /
                                       Math.Sqrt(CDbl(totalMeasurements)),
                .EstimationPointCount = subgroups.Count,
                .SigmaEstimationPointCount = sigmaEstimate.ContributingPointCount,
                .LimitMode = SpcStageLimitMode.EstimateFromStageData,
                .SourceStageId = String.Empty,
                .Method = sigmaEstimate.Method
            }
        End Function

        Private Shared Function ResolveSubgroupEstimator(
            chartType As SpcChartType,
            requested As SpcWithinSigmaEstimator) As SpcWithinSigmaEstimator

            If requested <> SpcWithinSigmaEstimator.Automatic Then Return requested
            If chartType = SpcChartType.SubgroupStandardDeviation OrElse
               chartType = SpcChartType.XBarS Then
                Return SpcWithinSigmaEstimator.AverageStandardDeviation
            End If
            Return SpcWithinSigmaEstimator.AverageRange
        End Function

        Private Shared Function ResolveAttributeParameters(
            stage As StageContext,
            request As SpcFitRequest,
            rawPoints As RawChartPoint(),
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
                    result = EstimateAttributeParameters(stage, request, rawPoints)

                Case SpcStageLimitMode.UseHistoricalParameters
                    Dim history As SpcHistoricalParameters = FindHistoricalParameters(request,
                                                                                       stage.Definition.StageId)
                    If history Is Nothing Then
                        Throw New ArgumentException(
                            "No historical parameters were supplied for stage '" &
                            stage.Definition.StageId & "'.")
                    End If

                    Dim center As Nullable(Of Double) = Nothing
                    Select Case request.ChartType
                        Case SpcChartType.PChart, SpcChartType.NpChart
                            center = history.NonconformingProportion
                        Case SpcChartType.CChart
                            center = history.MeanDefectCount
                        Case SpcChartType.UChart
                            center = history.MeanDefectRate
                    End Select
                    If Not center.HasValue Then
                        Throw New ArgumentException(
                            "The required historical attribute parameter is missing for stage '" &
                            stage.Definition.StageId & "'.")
                    End If

                    result = New StageParameters With {
                        .Center = center.Value,
                        .Sigma = Double.NaN,
                        .CenterStandardError = Double.NaN,
                        .EstimationPointCount = -1,
                        .SigmaEstimationPointCount = -1,
                        .LimitMode = SpcStageLimitMode.UseHistoricalParameters,
                        .SourceStageId = String.Empty,
                        .Method = "Historical parameters"
                    }

                Case SpcStageLimitMode.UseReferenceStage
                    Dim referenced As StageContext = FindStage(stages,
                                                               stage.Definition.ReferenceStageId)
                    Dim source As StageParameters = ResolveAttributeParameters(referenced,
                                                                               request,
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

        Private Shared Function EstimateAttributeParameters(
            stage As StageContext,
            request As SpcFitRequest,
            rawPoints As RawChartPoint()) As StageParameters

            Dim countTotal As Double = 0.0
            Dim denominatorTotal As Double = 0.0
            Dim pointCount As Integer = 0

            For logicalIndex As Integer = stage.Definition.FirstPointIndex To _
                                             stage.Definition.LastPointIndex
                Dim point As RawChartPoint = rawPoints(logicalIndex)
                If Not IsEstimationEligible(point) Then Continue For
                pointCount += 1
                countTotal += point.CountValue
                Select Case request.ChartType
                    Case SpcChartType.PChart, SpcChartType.NpChart
                        denominatorTotal += point.SampleSize
                    Case SpcChartType.CChart
                        denominatorTotal += 1.0
                    Case SpcChartType.UChart
                        denominatorTotal += point.Exposure
                End Select
            Next

            If pointCount = 0 OrElse denominatorTotal <= 0.0 Then
                Throw New ArgumentException(
                    "Stage '" & stage.Definition.StageId &
                    "' has no points eligible for parameter estimation.")
            End If

            Dim center As Double = countTotal / denominatorTotal
            Dim centerSe As Double
            Select Case request.ChartType
                Case SpcChartType.PChart, SpcChartType.NpChart
                    centerSe = Math.Sqrt(Math.Max(0.0,
                                                  center * (1.0 - center) /
                                                  denominatorTotal))
                Case SpcChartType.CChart
                    centerSe = Math.Sqrt(center / denominatorTotal)
                Case SpcChartType.UChart
                    centerSe = Math.Sqrt(center / denominatorTotal)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(request.ChartType))
            End Select

            Return New StageParameters With {
                .Center = center,
                .Sigma = Double.NaN,
                .CenterStandardError = centerSe,
                .EstimationPointCount = pointCount,
                .SigmaEstimationPointCount = -1,
                .LimitMode = SpcStageLimitMode.EstimateFromStageData,
                .SourceStageId = String.Empty,
                .Method = AttributeEstimationMethod(request.ChartType)
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

        Private Shared Function VariableChartRequiresMean(chartType As SpcChartType) As Boolean
            Return chartType = SpcChartType.Individuals OrElse
                   chartType = SpcChartType.IndividualsMovingRange OrElse
                   chartType = SpcChartType.XBar OrElse
                   chartType = SpcChartType.XBarR OrElse
                   chartType = SpcChartType.XBarS
        End Function

        Private Shared Function IsEstimationEligible(point As RawChartPoint) As Boolean
            Return Not point.IsOmitted AndAlso
                   (point.ExclusionScope And SpcExclusionScope.ParameterEstimation) =
                   SpcExclusionScope.None
        End Function

#End Region

#Region "Variable chart panels"

        Private Shared Function BuildIndividualsPanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawChartPoint = rawPoints(i)
                If raw.IsOmitted Then Continue For

                Dim parameters As StageParameters = ResolveVariableParameters(
                    raw.Stage, request, options, rawPoints, stages, cache, visiting)
                Dim limits As LimitValues = BuildSigmaLimits(
                    parameters.Center,
                    parameters.Sigma,
                    options.ControlLimits.SigmaMultiplier,
                    options.ControlLimits.NaturalLimitPolicy,
                    Nothing,
                    Nothing)

                points.Add(CreatePoint(raw,
                                       raw.Value,
                                       parameters.Center,
                                       parameters.Sigma,
                                       limits,
                                       1.0,
                                       Double.NaN))
            Next

            Return New SpcPanelResult(
                SpcPanelType.IndividualValue,
                "Individuals",
                points.ToArray(),
                ResolveAxisTitle(request, "Individual value"),
                BuildVariableParameterEstimates(stages,
                                                rawPoints,
                                                cache,
                                                SpcPanelType.IndividualValue,
                                                includeMean:=True))
        End Function

        Private Shared Function BuildMovingRangePanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim movingRangeLength As Integer = options.ControlLimits.MovingRangeLength
            Dim constants As SpcControlChartConstants =
                SpcStatistics.GetControlChartConstants(movingRangeLength)
            Dim points As New List(Of SpcPointResult)()
            Dim finiteMovingRanges As Integer = 0

            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawChartPoint = rawPoints(i)
                If raw.IsOmitted Then Continue For

                Dim window As MovingRangeWindow = BuildMovingRangeWindow(rawPoints,
                                                                          i,
                                                                          movingRangeLength)
                If window.HasValue Then finiteMovingRanges += 1

                Dim parameters As StageParameters = ResolveVariableParameters(
                    raw.Stage, request, options, rawPoints, stages, cache, visiting)
                Dim center As Double = constants.D2 * parameters.Sigma
                Dim standardError As Double = constants.D3 * parameters.Sigma
                Dim limits As LimitValues = BuildSigmaLimits(
                    center,
                    standardError,
                    options.ControlLimits.SigmaMultiplier,
                    options.ControlLimits.NaturalLimitPolicy,
                    0.0,
                    Nothing)

                If Not window.HasValue Then
                    window.ExclusionScope = raw.ExclusionScope
                    window.ExclusionReason = raw.ExclusionReason
                End If

                Dim includedInEstimation As Boolean = window.HasValue AndAlso
                    raw.Stage.Definition.LimitMode = SpcStageLimitMode.EstimateFromStageData AndAlso
                    (window.ExclusionScope And SpcExclusionScope.ParameterEstimation) =
                    SpcExclusionScope.None
                Dim includedInRules As Boolean = window.HasValue AndAlso
                    (window.ExclusionScope And SpcExclusionScope.RuleEvaluation) =
                    SpcExclusionScope.None

                points.Add(New SpcPointResult(
                    raw.LogicalIndex,
                    If(window.HasValue, window.Value, Double.NaN),
                    center,
                    limits.LowerControlLimit,
                    limits.UpperControlLimit,
                    label:=raw.Label,
                    stageId:=raw.Stage.Definition.StageId,
                    phase:=raw.Stage.Definition.Phase,
                    sequenceValue:=raw.SequenceValue,
                    standardError:=standardError,
                    standardizedValue:=Standardize(If(window.HasValue,
                                                       window.Value,
                                                       Double.NaN),
                                                   center,
                                                   standardError),
                    lowerOneSigmaLimit:=limits.LowerOneSigmaLimit,
                    upperOneSigmaLimit:=limits.UpperOneSigmaLimit,
                    lowerTwoSigmaLimit:=limits.LowerTwoSigmaLimit,
                    upperTwoSigmaLimit:=limits.UpperTwoSigmaLimit,
                    effectiveSampleSize:=If(window.HasValue,
                                             CDbl(movingRangeLength),
                                             Double.NaN),
                    sourceRowIndices:=window.SourceRowIndices,
                    includedInParameterEstimation:=includedInEstimation,
                    includedInRuleEvaluation:=includedInRules,
                    exclusionScope:=window.ExclusionScope,
                    exclusionReason:=window.ExclusionReason))
            Next

            If finiteMovingRanges = 0 Then
                Throw New ArgumentException(
                    "At least one complete moving range is required for a moving-range panel.")
            End If

            Return New SpcPanelResult(
                SpcPanelType.MovingRange,
                "Moving Range",
                points.ToArray(),
                ResolveAxisTitle(request, "Moving range"),
                BuildVariableParameterEstimates(stages,
                                                rawPoints,
                                                cache,
                                                SpcPanelType.MovingRange,
                                                includeMean:=False))
        End Function

        Private Shared Function BuildSubgroupPanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            panelType As SpcPanelType,
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawChartPoint = rawPoints(i)
                If raw.IsOmitted Then Continue For

                Dim parameters As StageParameters = ResolveVariableParameters(
                    raw.Stage, request, options, rawPoints, stages, cache, visiting)
                Dim value As Double
                Dim center As Double
                Dim standardError As Double
                Dim lowerBound As Nullable(Of Double) = Nothing

                Select Case panelType
                    Case SpcPanelType.SubgroupMean
                        value = raw.Subgroup.Mean
                        center = parameters.Center
                        standardError = parameters.Sigma /
                                        Math.Sqrt(CDbl(raw.Subgroup.Count))

                    Case SpcPanelType.SubgroupRange
                        Dim constants As SpcControlChartConstants =
                            SpcStatistics.GetControlChartConstants(raw.Subgroup.Count)
                        value = raw.Subgroup.Range
                        center = constants.D2 * parameters.Sigma
                        standardError = constants.D3 * parameters.Sigma
                        lowerBound = 0.0

                    Case SpcPanelType.SubgroupStandardDeviation
                        Dim c4 As Double = SpcStatistics.C4(raw.Subgroup.Count)
                        value = raw.Subgroup.SampleStandardDeviation
                        center = c4 * parameters.Sigma
                        standardError = parameters.Sigma *
                                        Math.Sqrt(Math.Max(0.0, 1.0 - c4 * c4))
                        lowerBound = 0.0

                    Case Else
                        Throw New ArgumentOutOfRangeException(NameOf(panelType))
                End Select

                Dim limits As LimitValues = BuildSigmaLimits(
                    center,
                    standardError,
                    options.ControlLimits.SigmaMultiplier,
                    options.ControlLimits.NaturalLimitPolicy,
                    lowerBound,
                    Nothing)

                points.Add(CreatePoint(raw,
                                       value,
                                       center,
                                       standardError,
                                       limits,
                                       CDbl(raw.Subgroup.Count),
                                       Double.NaN))
            Next

            Dim includeMean As Boolean = panelType = SpcPanelType.SubgroupMean
            Return New SpcPanelResult(
                panelType,
                PanelDisplayName(panelType),
                points.ToArray(),
                ResolveAxisTitle(request, DefaultAxisTitle(panelType)),
                BuildVariableParameterEstimates(stages,
                                                rawPoints,
                                                cache,
                                                panelType,
                                                includeMean))
        End Function

        Private Shared Function BuildVariableParameterEstimates(
            stages As StageContext(),
            rawPoints As RawChartPoint(),
            cache As Dictionary(Of String, StageParameters),
            panelType As SpcPanelType,
            includeMean As Boolean) As SpcParameterEstimate()

            Dim values As New List(Of SpcParameterEstimate)()
            For i As Integer = 0 To stages.Length - 1
                Dim stage As StageContext = stages(i)
                If Not StageHasRetainedPoint(stage, rawPoints) Then Continue For
                Dim parameters As StageParameters = Nothing
                If Not cache.TryGetValue(stage.Definition.StageId, parameters) Then
                    Throw New InvalidOperationException(
                        "Calculated stage parameters were not retained in the stage cache.")
                End If

                If includeMean Then
                    values.Add(New SpcParameterEstimate(
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
                End If

                values.Add(New SpcParameterEstimate(
                    stage.Definition.StageId,
                    panelType,
                    "ProcessSigma",
                    parameters.Sigma,
                    parameters.LimitMode,
                    sourceStageId:=parameters.SourceStageId,
                    method:=parameters.Method,
                    displayName:="Within-process sigma",
                    sampleCount:=ToNullableCount(parameters.SigmaEstimationPointCount)))
            Next
            Return values.ToArray()
        End Function

#End Region

#Region "Attribute chart panel"

        Private Shared Function BuildAttributePanel(
            request As SpcFitRequest,
            options As SpcAnalysisOptions,
            rawPoints As RawChartPoint(),
            stages As StageContext(),
            cache As Dictionary(Of String, StageParameters),
            visiting As HashSet(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim panelType As SpcPanelType = AttributePanelType(request.ChartType)
            Dim points As New List(Of SpcPointResult)()

            For i As Integer = 0 To rawPoints.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim raw As RawChartPoint = rawPoints(i)
                If raw.IsOmitted Then Continue For

                Dim parameters As StageParameters = ResolveAttributeParameters(
                    raw.Stage, request, rawPoints, stages, cache, visiting)

                Dim value As Double
                Dim center As Double
                Dim standardError As Double
                Dim effectiveSampleSize As Double
                Dim exposure As Double = Double.NaN
                Select Case request.ChartType
                    Case SpcChartType.PChart
                        value = raw.CountValue / raw.SampleSize
                        center = parameters.Center
                        standardError = Math.Sqrt(Math.Max(
                            0.0,
                            parameters.Center * (1.0 - parameters.Center) /
                            raw.SampleSize))
                        effectiveSampleSize = raw.SampleSize

                    Case SpcChartType.NpChart
                        value = raw.CountValue
                        center = raw.SampleSize * parameters.Center
                        standardError = Math.Sqrt(Math.Max(
                            0.0,
                            raw.SampleSize * parameters.Center *
                            (1.0 - parameters.Center)))
                        effectiveSampleSize = raw.SampleSize

                    Case SpcChartType.CChart
                        value = raw.CountValue
                        center = parameters.Center
                        standardError = Math.Sqrt(parameters.Center)
                        effectiveSampleSize = 1.0

                    Case SpcChartType.UChart
                        value = raw.CountValue / raw.Exposure
                        center = parameters.Center
                        standardError = Math.Sqrt(parameters.Center / raw.Exposure)
                        effectiveSampleSize = raw.Exposure
                        exposure = raw.Exposure

                    Case Else
                        Throw New ArgumentOutOfRangeException(NameOf(request.ChartType))
                End Select

                Dim limits As LimitValues
                If options.ControlLimits.Method = SpcControlLimitMethod.ExactProbability Then
                    limits = BuildExactAttributeLimits(request.ChartType,
                                                       parameters.Center,
                                                       raw,
                                                       options.ControlLimits.SigmaMultiplier)
                Else
                    Dim upperBound As Nullable(Of Double) = Nothing
                    If request.ChartType = SpcChartType.PChart Then upperBound = 1.0
                    If request.ChartType = SpcChartType.NpChart Then upperBound = raw.SampleSize
                    limits = BuildSigmaLimits(center,
                                              standardError,
                                              options.ControlLimits.SigmaMultiplier,
                                              options.ControlLimits.NaturalLimitPolicy,
                                              0.0,
                                              upperBound)
                End If

                points.Add(CreatePoint(raw,
                                       value,
                                       center,
                                       standardError,
                                       limits,
                                       effectiveSampleSize,
                                       exposure))
            Next

            Return New SpcPanelResult(
                panelType,
                PanelDisplayName(panelType),
                points.ToArray(),
                ResolveAxisTitle(request, DefaultAxisTitle(panelType)),
                BuildAttributeParameterEstimates(stages,
                                                 rawPoints,
                                                 cache,
                                                 panelType,
                                                 request.ChartType))
        End Function

        Private Shared Function BuildAttributeParameterEstimates(
            stages As StageContext(),
            rawPoints As RawChartPoint(),
            cache As Dictionary(Of String, StageParameters),
            panelType As SpcPanelType,
            chartType As SpcChartType) As SpcParameterEstimate()

            Dim parameterName As String
            Dim displayName As String
            Select Case chartType
                Case SpcChartType.PChart, SpcChartType.NpChart
                    parameterName = "NonconformingProportion"
                    displayName = "Nonconforming proportion"
                Case SpcChartType.CChart
                    parameterName = "MeanDefectCount"
                    displayName = "Mean defect count"
                Case SpcChartType.UChart
                    parameterName = "MeanDefectRate"
                    displayName = "Mean defect rate"
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End Select

            Dim values As New List(Of SpcParameterEstimate)()
            For i As Integer = 0 To stages.Length - 1
                Dim stage As StageContext = stages(i)
                If Not StageHasRetainedPoint(stage, rawPoints) Then Continue For
                Dim parameters As StageParameters = Nothing
                If Not cache.TryGetValue(stage.Definition.StageId, parameters) Then
                    Throw New InvalidOperationException(
                        "Calculated stage parameters were not retained in the stage cache.")
                End If

                values.Add(New SpcParameterEstimate(
                    stage.Definition.StageId,
                    panelType,
                    parameterName,
                    parameters.Center,
                    parameters.LimitMode,
                    standardError:=ToNullableFinite(parameters.CenterStandardError),
                    sourceStageId:=parameters.SourceStageId,
                    method:=parameters.Method,
                    displayName:=displayName,
                    sampleCount:=ToNullableCount(parameters.EstimationPointCount)))
            Next
            Return values.ToArray()
        End Function

        Private Shared Function BuildExactAttributeLimits(
            chartType As SpcChartType,
            parameterCenter As Double,
            raw As RawChartPoint,
            sigmaMultiplier As Double) As LimitValues

            Dim oneTailProbability As Double =
                1.0 - Global.BESHStatNG.distributions.Distributions.PNorm(sigmaMultiplier)
            oneTailProbability = Math.Max(0.0, Math.Min(0.5, oneTailProbability))

            Dim lower As Double
            Dim upper As Double
            Dim displayedCenter As Double
            Select Case chartType
                Case SpcChartType.PChart, SpcChartType.NpChart
                    Dim sampleSize As Integer = ToExactInteger(raw.SampleSize, "sample size")
                    Dim lowerCount As Integer = BinomialQuantile(oneTailProbability,
                                                                 sampleSize,
                                                                 parameterCenter)
                    Dim upperCount As Integer = BinomialQuantile(1.0 - oneTailProbability,
                                                                 sampleSize,
                                                                 parameterCenter)
                    If chartType = SpcChartType.PChart Then
                        lower = CDbl(lowerCount) / raw.SampleSize
                        upper = CDbl(upperCount) / raw.SampleSize
                        displayedCenter = parameterCenter
                    Else
                        lower = CDbl(lowerCount)
                        upper = CDbl(upperCount)
                        displayedCenter = raw.SampleSize * parameterCenter
                    End If

                Case SpcChartType.CChart
                    lower = CDbl(PoissonQuantile(oneTailProbability, parameterCenter))
                    upper = CDbl(PoissonQuantile(1.0 - oneTailProbability,
                                                  parameterCenter))
                    displayedCenter = parameterCenter

                Case SpcChartType.UChart
                    Dim lambda As Double = parameterCenter * raw.Exposure
                    lower = CDbl(PoissonQuantile(oneTailProbability, lambda)) /
                            raw.Exposure
                    upper = CDbl(PoissonQuantile(1.0 - oneTailProbability, lambda)) /
                            raw.Exposure
                    displayedCenter = parameterCenter

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End Select

            ' Very small sigma multipliers can place a discrete quantile on the
            ' same side of the distribution mean. A control limit still must
            ' bracket the chart centre line.
            lower = Math.Min(lower, displayedCenter)
            upper = Math.Max(upper, displayedCenter)

            ' Exact discrete limits do not generally coincide with one- and
            ' two-sigma zone boundaries. Returning NaN prevents those lines from
            ' being mislabelled and preserves the result-model ordering invariant.
            Return New LimitValues With {
                .LowerControlLimit = lower,
                .UpperControlLimit = upper,
                .LowerOneSigmaLimit = Double.NaN,
                .UpperOneSigmaLimit = Double.NaN,
                .LowerTwoSigmaLimit = Double.NaN,
                .UpperTwoSigmaLimit = Double.NaN
            }
        End Function

        Private Shared Function BinomialQuantile(probability As Double,
                                                 sampleSize As Integer,
                                                 proportion As Double) As Integer
            If probability <= 0.0 OrElse proportion <= 0.0 Then Return 0
            If probability >= 1.0 OrElse proportion >= 1.0 Then Return sampleSize

            Dim low As Integer = 0
            Dim high As Integer = sampleSize
            While low < high
                Dim middle As Integer = low + (high - low) \ 2
                Dim cdf As Double = BinomialCdf(middle, sampleSize, proportion)
                If cdf >= probability Then
                    high = middle
                Else
                    low = middle + 1
                End If
            End While
            Return low
        End Function

        Private Shared Function BinomialCdf(count As Integer,
                                            sampleSize As Integer,
                                            proportion As Double) As Double
            If count < 0 Then Return 0.0
            If count >= sampleSize Then Return 1.0
            Return Global.BESHStatNG.distributions.Distributions.RegularizedIncompleteBeta(
                1.0 - proportion,
                CDbl(sampleSize - count),
                CDbl(count + 1))
        End Function

        Private Shared Function PoissonQuantile(probability As Double,
                                                mean As Double) As Integer
            If mean > CDbl(Integer.MaxValue - 100000) Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(mean),
                    "The Poisson mean is too large for exact integer limits; use sigma limits.")
            End If
            Dim value As Integer =
                Global.BESHStatNG.distributions.Distributions.PoissonInv(probability, mean)
            If value = Integer.MinValue OrElse value = Integer.MaxValue Then
                Throw New InvalidOperationException("An exact Poisson limit could not be calculated.")
            End If
            Return value
        End Function

        Private Shared Function ToExactInteger(value As Double,
                                               valueName As String) As Integer
            If value > CDbl(Integer.MaxValue) Then
                Throw New ArgumentOutOfRangeException(
                    valueName,
                    "The value is too large for exact discrete limits; use sigma limits.")
            End If
            Return CInt(value)
        End Function

#End Region

#Region "Point and limit helpers"

        Private Shared Function CreatePoint(raw As RawChartPoint,
                                            value As Double,
                                            center As Double,
                                            standardError As Double,
                                            limits As LimitValues,
                                            effectiveSampleSize As Double,
                                            exposure As Double) As SpcPointResult
            Dim includedInEstimation As Boolean =
                raw.Stage.Definition.LimitMode = SpcStageLimitMode.EstimateFromStageData AndAlso
                IsEstimationEligible(raw)
            Dim includedInRules As Boolean =
                (raw.ExclusionScope And SpcExclusionScope.RuleEvaluation) =
                SpcExclusionScope.None AndAlso Not Double.IsNaN(value)

            Return New SpcPointResult(
                raw.LogicalIndex,
                value,
                center,
                limits.LowerControlLimit,
                limits.UpperControlLimit,
                label:=raw.Label,
                stageId:=raw.Stage.Definition.StageId,
                phase:=raw.Stage.Definition.Phase,
                sequenceValue:=raw.SequenceValue,
                standardError:=standardError,
                standardizedValue:=Standardize(value, center, standardError),
                lowerOneSigmaLimit:=limits.LowerOneSigmaLimit,
                upperOneSigmaLimit:=limits.UpperOneSigmaLimit,
                lowerTwoSigmaLimit:=limits.LowerTwoSigmaLimit,
                upperTwoSigmaLimit:=limits.UpperTwoSigmaLimit,
                effectiveSampleSize:=effectiveSampleSize,
                exposure:=exposure,
                sourceRowIndices:=raw.SourceRowIndices,
                includedInParameterEstimation:=includedInEstimation,
                includedInRuleEvaluation:=includedInRules,
                exclusionScope:=raw.ExclusionScope,
                exclusionReason:=raw.ExclusionReason)
        End Function

        Private Shared Function BuildSigmaLimits(
            center As Double,
            standardError As Double,
            sigmaMultiplier As Double,
            naturalLimitPolicy As SpcNaturalLimitPolicy,
            lowerFeasibleBound As Nullable(Of Double),
            upperFeasibleBound As Nullable(Of Double)) As LimitValues

            Dim result As New LimitValues With {
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

            If naturalLimitPolicy = SpcNaturalLimitPolicy.ClipToFeasibleRange Then
                If lowerFeasibleBound.HasValue Then
                    result.LowerControlLimit = Math.Max(lowerFeasibleBound.Value,
                                                        result.LowerControlLimit)
                    If Not Double.IsNaN(result.LowerOneSigmaLimit) Then
                        result.LowerOneSigmaLimit = Math.Max(lowerFeasibleBound.Value,
                                                             result.LowerOneSigmaLimit)
                    End If
                    If Not Double.IsNaN(result.LowerTwoSigmaLimit) Then
                        result.LowerTwoSigmaLimit = Math.Max(lowerFeasibleBound.Value,
                                                             result.LowerTwoSigmaLimit)
                    End If
                End If
                If upperFeasibleBound.HasValue Then
                    result.UpperControlLimit = Math.Min(upperFeasibleBound.Value,
                                                        result.UpperControlLimit)
                    If Not Double.IsNaN(result.UpperOneSigmaLimit) Then
                        result.UpperOneSigmaLimit = Math.Min(upperFeasibleBound.Value,
                                                             result.UpperOneSigmaLimit)
                    End If
                    If Not Double.IsNaN(result.UpperTwoSigmaLimit) Then
                        result.UpperTwoSigmaLimit = Math.Min(upperFeasibleBound.Value,
                                                             result.UpperTwoSigmaLimit)
                    End If
                End If
            End If
            Return result
        End Function

        Private Shared Function Standardize(value As Double,
                                            center As Double,
                                            standardError As Double) As Double
            If Double.IsNaN(value) Then Return Double.NaN
            If standardError > 0.0 Then Return (value - center) / standardError
            If value = center Then Return 0.0
            Return Double.NaN
        End Function

        Private Shared Function BuildMovingRangeWindow(
            points As RawChartPoint(),
            lastIndex As Integer,
            movingRangeLength As Integer) As MovingRangeWindow

            Dim firstIndex As Integer = lastIndex - movingRangeLength + 1
            If firstIndex < 0 Then
                Return MovingRangeWindow.Missing(points(lastIndex).SourceRowIndices)
            End If

            Dim stageId As String = points(lastIndex).Stage.Definition.StageId
            Dim minimum As Double = Double.PositiveInfinity
            Dim maximum As Double = Double.NegativeInfinity
            Dim sourceRows As New List(Of Integer)()
            Dim scope As SpcExclusionScope = SpcExclusionScope.None
            Dim reason As String = String.Empty

            For i As Integer = firstIndex To lastIndex
                Dim point As RawChartPoint = points(i)
                If point.IsOmitted OrElse
                   Not String.Equals(point.Stage.Definition.StageId,
                                     stageId,
                                     StringComparison.OrdinalIgnoreCase) Then
                    Return MovingRangeWindow.Missing(points(lastIndex).SourceRowIndices)
                End If
                minimum = Math.Min(minimum, point.Value)
                maximum = Math.Max(maximum, point.Value)
                sourceRows.AddRange(point.SourceRowIndices)
                scope = scope Or point.ExclusionScope
                reason = CombineReason(reason, point.ExclusionReason)
            Next

            Return New MovingRangeWindow With {
                .HasValue = True,
                .Value = maximum - minimum,
                .SourceRowIndices = UniqueSortedIndices(sourceRows),
                .ExclusionScope = scope,
                .ExclusionReason = reason
            }
        End Function

        Private Shared Function UniqueSortedIndices(values As List(Of Integer)) As Integer()
            Dim unique As New HashSet(Of Integer)()
            For i As Integer = 0 To values.Count - 1
                unique.Add(values(i))
            Next
            Dim result As Integer() = New List(Of Integer)(unique).ToArray()
            Array.Sort(result)
            Return result
        End Function

        Private Shared Function CombineReason(existingReason As String,
                                              additionalReason As String) As String
            Dim normalized As String = If(additionalReason, String.Empty).Trim()
            If normalized.Length = 0 Then Return If(existingReason, String.Empty)
            If String.IsNullOrWhiteSpace(existingReason) Then Return normalized
            If existingReason.IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return existingReason
            End If
            Return existingReason & "; " & normalized
        End Function

        Private Shared Function StageHasRetainedPoint(stage As StageContext,
                                                     points As RawChartPoint()) As Boolean
            For i As Integer = stage.Definition.FirstPointIndex To _
                               stage.Definition.LastPointIndex
                If Not points(i).IsOmitted Then Return True
            Next
            Return False
        End Function

        Private Shared Function CountRetainedPoints(points As RawChartPoint()) As Integer
            Dim count As Integer = 0
            For i As Integer = 0 To points.Length - 1
                If Not points(i).IsOmitted Then count += 1
            Next
            Return count
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

        Private Shared Function PanelDisplayName(panelType As SpcPanelType) As String
            Select Case panelType
                Case SpcPanelType.SubgroupMean
                    Return "X-bar"
                Case SpcPanelType.SubgroupRange
                    Return "Range"
                Case SpcPanelType.SubgroupStandardDeviation
                    Return "Standard Deviation"
                Case SpcPanelType.Proportion
                    Return "p Chart"
                Case SpcPanelType.NumberNonconforming
                    Return "np Chart"
                Case SpcPanelType.DefectCount
                    Return "c Chart"
                Case SpcPanelType.DefectRate
                    Return "u Chart"
                Case Else
                    Return panelType.ToString()
            End Select
        End Function

        Private Shared Function DefaultAxisTitle(panelType As SpcPanelType) As String
            Select Case panelType
                Case SpcPanelType.SubgroupMean
                    Return "Subgroup mean"
                Case SpcPanelType.SubgroupRange
                    Return "Subgroup range"
                Case SpcPanelType.SubgroupStandardDeviation
                    Return "Subgroup standard deviation"
                Case SpcPanelType.Proportion
                    Return "Proportion nonconforming"
                Case SpcPanelType.NumberNonconforming
                    Return "Number nonconforming"
                Case SpcPanelType.DefectCount
                    Return "Defect count"
                Case SpcPanelType.DefectRate
                    Return "Defects per unit of exposure"
                Case Else
                    Return panelType.ToString()
            End Select
        End Function

        Private Shared Function AttributePanelType(chartType As SpcChartType) As SpcPanelType
            Select Case chartType
                Case SpcChartType.PChart
                    Return SpcPanelType.Proportion
                Case SpcChartType.NpChart
                    Return SpcPanelType.NumberNonconforming
                Case SpcChartType.CChart
                    Return SpcPanelType.DefectCount
                Case SpcChartType.UChart
                    Return SpcPanelType.DefectRate
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End Select
        End Function

        Private Shared Function AttributeEstimationMethod(chartType As SpcChartType) As String
            Select Case chartType
                Case SpcChartType.PChart, SpcChartType.NpChart
                    Return "Pooled nonconforming proportion"
                Case SpcChartType.CChart
                    Return "Average defect count"
                Case SpcChartType.UChart
                    Return "Pooled defect rate"
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End Select
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

        Private NotInheritable Class RawChartPoint
            Public Property LogicalIndex As Integer
            Public Property Value As Double = Double.NaN
            Public Property CountValue As Double = Double.NaN
            Public Property SampleSize As Double = Double.NaN
            Public Property Exposure As Double = Double.NaN
            Public Property Subgroup As SpcSubgroupStatistics
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

        Private NotInheritable Class StackedGroup
            Public Sub New(subgroupId As String,
                           label As String,
                           sequenceValue As Nullable(Of Double))
                Me.SubgroupId = subgroupId
                Me.Label = label
                Me.SequenceValue = sequenceValue
            End Sub

            Public ReadOnly Property SubgroupId As String
            Public ReadOnly Property Label As String
            Public ReadOnly Property SequenceValue As Nullable(Of Double)
            Public ReadOnly Property Values As New List(Of Double)()
            Public ReadOnly Property SourceRows As New List(Of Integer)()
            Public Property HasMissing As Boolean
        End Class

        Private Structure LimitValues
            Public LowerControlLimit As Double
            Public UpperControlLimit As Double
            Public LowerOneSigmaLimit As Double
            Public UpperOneSigmaLimit As Double
            Public LowerTwoSigmaLimit As Double
            Public UpperTwoSigmaLimit As Double
        End Structure

        Private Structure MovingRangeWindow
            Public HasValue As Boolean
            Public Value As Double
            Public SourceRowIndices As Integer()
            Public ExclusionScope As SpcExclusionScope
            Public ExclusionReason As String

            Public Shared Function Missing(sourceRows As Integer()) As MovingRangeWindow
                Return New MovingRangeWindow With {
                    .HasValue = False,
                    .Value = Double.NaN,
                    .SourceRowIndices = CType(sourceRows.Clone(), Integer()),
                    .ExclusionScope = SpcExclusionScope.None,
                    .ExclusionReason = String.Empty
                }
            End Function
        End Structure

#End Region

    End Class

End Namespace
