Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Stores optional specification limits and a process target.
    ''' </summary>
    ''' <remarks>
    ''' Specification limits describe product or process requirements. They are
    ''' deliberately separate from statistically calculated control limits.
    ''' </remarks>
    Public NotInheritable Class SpcSpecificationLimits
        Private ReadOnly _lowerSpecificationLimit As Nullable(Of Double)
        Private ReadOnly _target As Nullable(Of Double)
        Private ReadOnly _upperSpecificationLimit As Nullable(Of Double)

        Public Sub New(Optional lowerSpecificationLimit As Nullable(Of Double) = Nothing,
                       Optional target As Nullable(Of Double) = Nothing,
                       Optional upperSpecificationLimit As Nullable(Of Double) = Nothing)

            SpcModelGuards.ValidateOptionalFinite(lowerSpecificationLimit,
                                                  NameOf(lowerSpecificationLimit))
            SpcModelGuards.ValidateOptionalFinite(target, NameOf(target))
            SpcModelGuards.ValidateOptionalFinite(upperSpecificationLimit,
                                                  NameOf(upperSpecificationLimit))

            If lowerSpecificationLimit.HasValue AndAlso
               upperSpecificationLimit.HasValue AndAlso
               lowerSpecificationLimit.Value >= upperSpecificationLimit.Value Then
                Throw New ArgumentException(
                    "The lower specification limit must be less than the upper specification limit.")
            End If

            If target.HasValue Then
                If lowerSpecificationLimit.HasValue AndAlso
                   target.Value < lowerSpecificationLimit.Value Then
                    Throw New ArgumentOutOfRangeException(NameOf(target),
                        "The target must not be below the lower specification limit.")
                End If
                If upperSpecificationLimit.HasValue AndAlso
                   target.Value > upperSpecificationLimit.Value Then
                    Throw New ArgumentOutOfRangeException(NameOf(target),
                        "The target must not exceed the upper specification limit.")
                End If
            End If

            _lowerSpecificationLimit = lowerSpecificationLimit
            _target = target
            _upperSpecificationLimit = upperSpecificationLimit
        End Sub

        Public ReadOnly Property LowerSpecificationLimit As Nullable(Of Double)
            Get
                Return _lowerSpecificationLimit
            End Get
        End Property

        Public ReadOnly Property Target As Nullable(Of Double)
            Get
                Return _target
            End Get
        End Property

        Public ReadOnly Property UpperSpecificationLimit As Nullable(Of Double)
            Get
                Return _upperSpecificationLimit
            End Get
        End Property

        Public ReadOnly Property HasAnyValue As Boolean
            Get
                Return _lowerSpecificationLimit.HasValue OrElse
                       _target.HasValue OrElse
                       _upperSpecificationLimit.HasValue
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Stores historical process parameters supplied instead of estimating them
    ''' from Phase-I observations.
    ''' </summary>
    ''' <remarks>
    ''' An empty <see cref="StageId"/> denotes parameters that apply by default.
    ''' A stage-specific entry overrides the default entry for that stage. The SPC
    ''' engine validates which fields are required for the selected chart.
    ''' </remarks>
    Public NotInheritable Class SpcHistoricalParameters
        Private ReadOnly _stageId As String
        Private ReadOnly _processMean As Nullable(Of Double)
        Private ReadOnly _processSigma As Nullable(Of Double)
        Private ReadOnly _nonconformingProportion As Nullable(Of Double)
        Private ReadOnly _meanDefectCount As Nullable(Of Double)
        Private ReadOnly _meanDefectRate As Nullable(Of Double)
        Private ReadOnly _laneySigmaZ As Nullable(Of Double)

        Public Sub New(Optional stageId As String = Nothing,
                       Optional processMean As Nullable(Of Double) = Nothing,
                       Optional processSigma As Nullable(Of Double) = Nothing,
                       Optional nonconformingProportion As Nullable(Of Double) = Nothing,
                       Optional meanDefectCount As Nullable(Of Double) = Nothing,
                       Optional meanDefectRate As Nullable(Of Double) = Nothing,
                       Optional laneySigmaZ As Nullable(Of Double) = Nothing)

            SpcModelGuards.ValidateOptionalFinite(processMean, NameOf(processMean))
            SpcModelGuards.ValidateOptionalNonnegative(processSigma, NameOf(processSigma))
            SpcModelGuards.ValidateOptionalRange(nonconformingProportion,
                                                 0.0,
                                                 1.0,
                                                 NameOf(nonconformingProportion))
            SpcModelGuards.ValidateOptionalNonnegative(meanDefectCount,
                                                       NameOf(meanDefectCount))
            SpcModelGuards.ValidateOptionalNonnegative(meanDefectRate,
                                                       NameOf(meanDefectRate))
            SpcModelGuards.ValidateOptionalPositive(laneySigmaZ, NameOf(laneySigmaZ))

            If Not processMean.HasValue AndAlso
               Not processSigma.HasValue AndAlso
               Not nonconformingProportion.HasValue AndAlso
               Not meanDefectCount.HasValue AndAlso
               Not meanDefectRate.HasValue AndAlso
               Not laneySigmaZ.HasValue Then
                Throw New ArgumentException(
                    "At least one historical process parameter must be supplied.")
            End If

            _stageId = SpcModelGuards.NormalizeOptionalText(stageId)
            _processMean = processMean
            _processSigma = processSigma
            _nonconformingProportion = nonconformingProportion
            _meanDefectCount = meanDefectCount
            _meanDefectRate = meanDefectRate
            _laneySigmaZ = laneySigmaZ
        End Sub

        ''' <summary>
        ''' Gets the stage identifier, or an empty string when these are the default
        ''' historical parameters.
        ''' </summary>
        Public ReadOnly Property StageId As String
            Get
                Return _stageId
            End Get
        End Property

        Public ReadOnly Property AppliesToAllStages As Boolean
            Get
                Return _stageId.Length = 0
            End Get
        End Property

        Public ReadOnly Property ProcessMean As Nullable(Of Double)
            Get
                Return _processMean
            End Get
        End Property

        Public ReadOnly Property ProcessSigma As Nullable(Of Double)
            Get
                Return _processSigma
            End Get
        End Property

        Public ReadOnly Property NonconformingProportion As Nullable(Of Double)
            Get
                Return _nonconformingProportion
            End Get
        End Property

        Public ReadOnly Property MeanDefectCount As Nullable(Of Double)
            Get
                Return _meanDefectCount
            End Get
        End Property

        Public ReadOnly Property MeanDefectRate As Nullable(Of Double)
            Get
                Return _meanDefectRate
            End Get
        End Property

        Public ReadOnly Property LaneySigmaZ As Nullable(Of Double)
            Get
                Return _laneySigmaZ
            End Get
        End Property

        Public ReadOnly Property ParameterCount As Integer
            Get
                Dim count As Integer = 0
                If _processMean.HasValue Then count += 1
                If _processSigma.HasValue Then count += 1
                If _nonconformingProportion.HasValue Then count += 1
                If _meanDefectCount.HasValue Then count += 1
                If _meanDefectRate.HasValue Then count += 1
                If _laneySigmaZ.HasValue Then count += 1
                Return count
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Stores parameters used by chart families whose calculation requires more
    ''' than the shared control-limit options.
    ''' </summary>
    ''' <remarks>
    ''' The initial Shewhart release normally leaves all nullable fields unset.
    ''' These fields provide a typed request surface for subsequent EWMA, CUSUM,
    ''' and moving-average implementations without introducing host-specific data.
    ''' </remarks>
    Public NotInheritable Class SpcChartParameters
        Private ReadOnly _ewmaLambda As Nullable(Of Double)
        Private ReadOnly _cusumReferenceValue As Nullable(Of Double)
        Private ReadOnly _cusumDecisionInterval As Nullable(Of Double)
        Private ReadOnly _headStart As Nullable(Of Double)
        Private ReadOnly _movingAverageSpan As Nullable(Of Integer)
        Private ReadOnly _useSteadyStateLimits As Boolean

        Public Sub New(Optional ewmaLambda As Nullable(Of Double) = Nothing,
                       Optional cusumReferenceValue As Nullable(Of Double) = Nothing,
                       Optional cusumDecisionInterval As Nullable(Of Double) = Nothing,
                       Optional headStart As Nullable(Of Double) = Nothing,
                       Optional movingAverageSpan As Nullable(Of Integer) = Nothing,
                       Optional useSteadyStateLimits As Boolean = False)

            If ewmaLambda.HasValue Then
                SpcModelGuards.ValidateFinite(ewmaLambda.Value, NameOf(ewmaLambda))
                If ewmaLambda.Value <= 0.0 OrElse ewmaLambda.Value > 1.0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(ewmaLambda),
                        "EWMA lambda must be in the interval (0, 1].")
                End If
            End If

            SpcModelGuards.ValidateOptionalNonnegative(cusumReferenceValue,
                                                       NameOf(cusumReferenceValue))
            SpcModelGuards.ValidateOptionalPositive(cusumDecisionInterval,
                                                    NameOf(cusumDecisionInterval))
            SpcModelGuards.ValidateOptionalNonnegative(headStart, NameOf(headStart))

            If movingAverageSpan.HasValue AndAlso movingAverageSpan.Value < 2 Then
                Throw New ArgumentOutOfRangeException(NameOf(movingAverageSpan),
                    "The moving-average span must be at least two.")
            End If

            _ewmaLambda = ewmaLambda
            _cusumReferenceValue = cusumReferenceValue
            _cusumDecisionInterval = cusumDecisionInterval
            _headStart = headStart
            _movingAverageSpan = movingAverageSpan
            _useSteadyStateLimits = useSteadyStateLimits
        End Sub

        Public ReadOnly Property EwmaLambda As Nullable(Of Double)
            Get
                Return _ewmaLambda
            End Get
        End Property

        Public ReadOnly Property CusumReferenceValue As Nullable(Of Double)
            Get
                Return _cusumReferenceValue
            End Get
        End Property

        Public ReadOnly Property CusumDecisionInterval As Nullable(Of Double)
            Get
                Return _cusumDecisionInterval
            End Get
        End Property

        Public ReadOnly Property HeadStart As Nullable(Of Double)
            Get
                Return _headStart
            End Get
        End Property

        Public ReadOnly Property MovingAverageSpan As Nullable(Of Integer)
            Get
                Return _movingAverageSpan
            End Get
        End Property

        Public ReadOnly Property UseSteadyStateLimits As Boolean
            Get
                Return _useSteadyStateLimits
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Immutable, host-neutral input arrays used to construct ordered SPC chart points.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Missing numeric measurements are represented by <see cref="Double.NaN"/>.
    ''' Infinity is never a valid missing-value marker and is rejected here.
    ''' </para>
    ''' <para>
    ''' Row-aligned metadata refer to input rows. For stacked observations, several
    ''' input rows can later map to one chart point. The resulting point retains all
    ''' corresponding <see cref="SpcPointResult.SourceRowIndices"/> values.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SpcInputData
        Private ReadOnly _layout As SpcDataLayout
        Private ReadOnly _measurements As Double(,)
        Private ReadOnly _counts As Double()
        Private ReadOnly _sampleSizes As Double()
        Private ReadOnly _exposures As Double()
        Private ReadOnly _subgroupIds As String()
        Private ReadOnly _labels As String()
        Private ReadOnly _sequenceValues As Double()
        Private ReadOnly _sourceRowIndices As Integer()
        Private ReadOnly _measurementColumnNames As String()
        Private ReadOnly _rowCount As Integer

        ''' <summary>
        ''' Initializes an input-data snapshot.
        ''' </summary>
        ''' <param name="layout">Arrangement of the source observations.</param>
        ''' <param name="measurements">
        ''' Numeric measurements. Wide-subgroup data use one subgroup per row;
        ''' stacked and individual layouts normally use a single column.
        ''' </param>
        ''' <param name="counts">Optional nonconforming-item or defect counts.</param>
        ''' <param name="sampleSizes">Optional subgroup denominators, principally for p and np charts.</param>
        ''' <param name="exposures">Optional opportunity or exposure values, principally for u charts.</param>
        ''' <param name="subgroupIds">Optional row-aligned subgroup identifiers for stacked data.</param>
        ''' <param name="labels">Optional row-aligned display labels.</param>
        ''' <param name="sequenceValues">
        ''' Optional row-aligned numeric ordering values, including Excel date serials.
        ''' </param>
        ''' <param name="sourceRowIndices">
        ''' Optional nonnegative source-row identifiers. Zero-based input positions are
        ''' generated when this argument is omitted.
        ''' </param>
        ''' <param name="measurementColumnNames">Optional measurement-column labels.</param>
        Public Sub New(layout As SpcDataLayout,
                       Optional measurements As Double(,) = Nothing,
                       Optional counts As Double() = Nothing,
                       Optional sampleSizes As Double() = Nothing,
                       Optional exposures As Double() = Nothing,
                       Optional subgroupIds As String() = Nothing,
                       Optional labels As String() = Nothing,
                       Optional sequenceValues As Double() = Nothing,
                       Optional sourceRowIndices As Integer() = Nothing,
                       Optional measurementColumnNames As String() = Nothing)

            If Not [Enum].IsDefined(GetType(SpcDataLayout), layout) Then
                Throw New ArgumentOutOfRangeException(NameOf(layout))
            End If

            Dim rowCount As Integer = 0
            Dim measurementColumnCount As Integer = 0
            If measurements IsNot Nothing Then
                If measurements.GetLength(0) = 0 OrElse measurements.GetLength(1) = 0 Then
                    Throw New ArgumentException(
                        "The measurement matrix must contain at least one row and one column.",
                        NameOf(measurements))
                End If
                rowCount = measurements.GetLength(0)
                measurementColumnCount = measurements.GetLength(1)
                SpcModelGuards.ValidateNoInfinity(measurements, NameOf(measurements))
            End If

            SpcModelGuards.MergeAlignedLength(rowCount, counts, NameOf(counts))
            SpcModelGuards.MergeAlignedLength(rowCount, sampleSizes, NameOf(sampleSizes))
            SpcModelGuards.MergeAlignedLength(rowCount, exposures, NameOf(exposures))
            SpcModelGuards.MergeAlignedLength(rowCount, subgroupIds, NameOf(subgroupIds))
            SpcModelGuards.MergeAlignedLength(rowCount, labels, NameOf(labels))
            SpcModelGuards.MergeAlignedLength(rowCount, sequenceValues, NameOf(sequenceValues))
            SpcModelGuards.MergeAlignedLength(rowCount, sourceRowIndices, NameOf(sourceRowIndices))

            If measurements Is Nothing AndAlso counts Is Nothing Then
                Throw New ArgumentException(
                    "Measurements or aggregated counts are required.")
            End If
            If rowCount <= 0 Then
                Throw New ArgumentException("At least one input row is required.")
            End If

            SpcModelGuards.ValidateNoInfinity(counts, NameOf(counts))
            SpcModelGuards.ValidateNoInfinity(sampleSizes, NameOf(sampleSizes))
            SpcModelGuards.ValidateNoInfinity(exposures, NameOf(exposures))
            SpcModelGuards.ValidateNoInfinity(sequenceValues, NameOf(sequenceValues))

            If measurementColumnNames IsNot Nothing Then
                If measurements Is Nothing Then
                    Throw New ArgumentException(
                        "Measurement-column names require a measurement matrix.",
                        NameOf(measurementColumnNames))
                End If
                If measurementColumnNames.Length <> measurementColumnCount Then
                    Throw New ArgumentException(
                        "Measurement-column names must match the measurement column count.",
                        NameOf(measurementColumnNames))
                End If
            End If

            Dim copiedSourceRows As Integer()
            If sourceRowIndices Is Nothing Then
                copiedSourceRows = New Integer(rowCount - 1) {}
                For i As Integer = 0 To copiedSourceRows.Length - 1
                    copiedSourceRows(i) = i
                Next
            Else
                copiedSourceRows = CType(sourceRowIndices.Clone(), Integer())
                For i As Integer = 0 To copiedSourceRows.Length - 1
                    If copiedSourceRows(i) < 0 Then
                        Throw New ArgumentOutOfRangeException(NameOf(sourceRowIndices),
                            "Source-row identifiers must be nonnegative.")
                    End If
                Next
            End If

            _layout = layout
            _measurements = SpcModelGuards.CloneMatrix(measurements)
            _counts = SpcModelGuards.CloneVector(counts)
            _sampleSizes = SpcModelGuards.CloneVector(sampleSizes)
            _exposures = SpcModelGuards.CloneVector(exposures)
            _subgroupIds = SpcModelGuards.CopyTextArray(subgroupIds)
            _labels = SpcModelGuards.CopyTextArray(labels)
            _sequenceValues = SpcModelGuards.CloneVector(sequenceValues)
            _sourceRowIndices = copiedSourceRows
            _measurementColumnNames = SpcModelGuards.CopyTextArray(measurementColumnNames)
            _rowCount = rowCount
        End Sub

        ''' <summary>Creates a wide-layout input snapshot.</summary>
        Public Shared Function FromWideSubgroups(measurements As Double(,),
                                                 Optional labels As String() = Nothing,
                                                 Optional sequenceValues As Double() = Nothing,
                                                 Optional sourceRowIndices As Integer() = Nothing,
                                                 Optional measurementColumnNames As String() = Nothing) As SpcInputData
            Return New SpcInputData(
                SpcDataLayout.WideSubgroups,
                measurements:=measurements,
                labels:=labels,
                sequenceValues:=sequenceValues,
                sourceRowIndices:=sourceRowIndices,
                measurementColumnNames:=measurementColumnNames)
        End Function

        ''' <summary>Creates a stacked-observation input snapshot.</summary>
        Public Shared Function FromStackedObservations(values As Double(),
                                                       subgroupIds As String(),
                                                       Optional labels As String() = Nothing,
                                                       Optional sequenceValues As Double() = Nothing,
                                                       Optional sourceRowIndices As Integer() = Nothing,
                                                       Optional valueName As String = Nothing) As SpcInputData
            Dim names As String() = Nothing
            If Not String.IsNullOrWhiteSpace(valueName) Then names = {valueName.Trim()}

            Return New SpcInputData(
                SpcDataLayout.StackedObservations,
                measurements:=SpcModelGuards.ToColumnMatrix(values, NameOf(values)),
                subgroupIds:=subgroupIds,
                labels:=labels,
                sequenceValues:=sequenceValues,
                sourceRowIndices:=sourceRowIndices,
                measurementColumnNames:=names)
        End Function

        ''' <summary>Creates an ordered individual-value input snapshot.</summary>
        Public Shared Function FromIndividualSequence(values As Double(),
                                                      Optional labels As String() = Nothing,
                                                      Optional sequenceValues As Double() = Nothing,
                                                      Optional sourceRowIndices As Integer() = Nothing,
                                                      Optional valueName As String = Nothing) As SpcInputData
            Dim names As String() = Nothing
            If Not String.IsNullOrWhiteSpace(valueName) Then names = {valueName.Trim()}

            Return New SpcInputData(
                SpcDataLayout.IndividualSequence,
                measurements:=SpcModelGuards.ToColumnMatrix(values, NameOf(values)),
                labels:=labels,
                sequenceValues:=sequenceValues,
                sourceRowIndices:=sourceRowIndices,
                measurementColumnNames:=names)
        End Function

        ''' <summary>Creates a pre-aggregated count input snapshot.</summary>
        Public Shared Function FromAggregatedCounts(counts As Double(),
                                                    Optional sampleSizes As Double() = Nothing,
                                                    Optional exposures As Double() = Nothing,
                                                    Optional labels As String() = Nothing,
                                                    Optional sequenceValues As Double() = Nothing,
                                                    Optional sourceRowIndices As Integer() = Nothing) As SpcInputData
            Return New SpcInputData(
                SpcDataLayout.AggregatedCounts,
                counts:=counts,
                sampleSizes:=sampleSizes,
                exposures:=exposures,
                labels:=labels,
                sequenceValues:=sequenceValues,
                sourceRowIndices:=sourceRowIndices)
        End Function

        Public ReadOnly Property Layout As SpcDataLayout
            Get
                Return _layout
            End Get
        End Property

        Public ReadOnly Property RowCount As Integer
            Get
                Return _rowCount
            End Get
        End Property

        Public ReadOnly Property MeasurementColumnCount As Integer
            Get
                If _measurements Is Nothing Then Return 0
                Return _measurements.GetLength(1)
            End Get
        End Property

        Public ReadOnly Property Measurements As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_measurements)
            End Get
        End Property

        Public ReadOnly Property Counts As Double()
            Get
                Return SpcModelGuards.CloneVector(_counts)
            End Get
        End Property

        Public ReadOnly Property SampleSizes As Double()
            Get
                Return SpcModelGuards.CloneVector(_sampleSizes)
            End Get
        End Property

        Public ReadOnly Property Exposures As Double()
            Get
                Return SpcModelGuards.CloneVector(_exposures)
            End Get
        End Property

        Public ReadOnly Property SubgroupIds As String()
            Get
                Return SpcModelGuards.CopyTextArray(_subgroupIds)
            End Get
        End Property

        Public ReadOnly Property Labels As String()
            Get
                Return SpcModelGuards.CopyTextArray(_labels)
            End Get
        End Property

        Public ReadOnly Property SequenceValues As Double()
            Get
                Return SpcModelGuards.CloneVector(_sequenceValues)
            End Get
        End Property

        Public ReadOnly Property SourceRowIndices As Integer()
            Get
                Return CType(_sourceRowIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property MeasurementColumnNames As String()
            Get
                Return SpcModelGuards.CopyTextArray(_measurementColumnNames)
            End Get
        End Property

        Public Function GetMeasurementColumnName(columnIndex As Integer) As String
            If columnIndex < 0 OrElse columnIndex >= MeasurementColumnCount Then
                Throw New ArgumentOutOfRangeException(NameOf(columnIndex))
            End If
            If _measurementColumnNames IsNot Nothing AndAlso
               _measurementColumnNames(columnIndex).Length > 0 Then
                Return _measurementColumnNames(columnIndex)
            End If
            Return "Value" & (columnIndex + 1).ToString(CultureInfo.InvariantCulture)
        End Function
    End Class

    ''' <summary>
    ''' Immutable request passed to <c>SpcEngine.Fit</c>.
    ''' </summary>
    ''' <remarks>
    ''' The constructor snapshots all mutable options and arrays. Consequently a UI
    ''' or UDF caller can safely reuse or edit its source objects after constructing
    ''' the request without changing an in-progress analysis.
    ''' </remarks>
    Public NotInheritable Class SpcFitRequest
        Private ReadOnly _chartType As SpcChartType
        Private ReadOnly _data As SpcInputData
        Private ReadOnly _analysisOptions As SpcAnalysisOptions
        Private ReadOnly _historicalParameters As SpcHistoricalParameters()
        Private ReadOnly _specificationLimits As SpcSpecificationLimits
        Private ReadOnly _chartParameters As SpcChartParameters
        Private ReadOnly _requestLabel As String
        Private ReadOnly _chartTitle As String
        Private ReadOnly _valueAxisTitle As String

        Public Sub New(chartType As SpcChartType,
                       data As SpcInputData,
                       Optional analysisOptions As SpcAnalysisOptions = Nothing,
                       Optional historicalParameters As SpcHistoricalParameters() = Nothing,
                       Optional specificationLimits As SpcSpecificationLimits = Nothing,
                       Optional chartParameters As SpcChartParameters = Nothing,
                       Optional requestLabel As String = Nothing,
                       Optional chartTitle As String = Nothing,
                       Optional valueAxisTitle As String = Nothing)

            If Not [Enum].IsDefined(GetType(SpcChartType), chartType) Then
                Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End If
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))

            Dim copiedHistory As SpcHistoricalParameters()
            If historicalParameters Is Nothing Then
                copiedHistory = Array.Empty(Of SpcHistoricalParameters)()
            Else
                copiedHistory = CType(historicalParameters.Clone(), SpcHistoricalParameters())
                For i As Integer = 0 To copiedHistory.Length - 1
                    If copiedHistory(i) Is Nothing Then
                        Throw New ArgumentException(
                            "Historical parameter entries must not be null.",
                            NameOf(historicalParameters))
                    End If
                Next
            End If

            _chartType = chartType
            _data = data
            _analysisOptions = If(analysisOptions Is Nothing,
                                  New SpcAnalysisOptions(),
                                  analysisOptions.Copy())
            _historicalParameters = copiedHistory
            _specificationLimits = If(specificationLimits,
                                      New SpcSpecificationLimits())
            _chartParameters = If(chartParameters, New SpcChartParameters())
            _requestLabel = SpcModelGuards.NormalizeOptionalText(requestLabel)

            Dim normalizedTitle As String = SpcModelGuards.NormalizeOptionalText(chartTitle)
            If normalizedTitle.Length = 0 Then normalizedTitle = chartType.ToString()
            _chartTitle = normalizedTitle
            _valueAxisTitle = SpcModelGuards.NormalizeOptionalText(valueAxisTitle)
        End Sub

        Public ReadOnly Property ChartType As SpcChartType
            Get
                Return _chartType
            End Get
        End Property

        Public ReadOnly Property DataLayout As SpcDataLayout
            Get
                Return _data.Layout
            End Get
        End Property

        Public ReadOnly Property Data As SpcInputData
            Get
                Return _data
            End Get
        End Property

        ''' <summary>Returns an independent copy of the analysis options.</summary>
        Public ReadOnly Property AnalysisOptions As SpcAnalysisOptions
            Get
                Return _analysisOptions.Copy()
            End Get
        End Property

        Public ReadOnly Property HistoricalParameters As SpcHistoricalParameters()
            Get
                Return CType(_historicalParameters.Clone(), SpcHistoricalParameters())
            End Get
        End Property

        Public ReadOnly Property SpecificationLimits As SpcSpecificationLimits
            Get
                Return _specificationLimits
            End Get
        End Property

        Public ReadOnly Property ChartParameters As SpcChartParameters
            Get
                Return _chartParameters
            End Get
        End Property

        Public ReadOnly Property RequestLabel As String
            Get
                Return _requestLabel
            End Get
        End Property

        Public ReadOnly Property ChartTitle As String
            Get
                Return _chartTitle
            End Get
        End Property

        Public ReadOnly Property ValueAxisTitle As String
            Get
                Return _valueAxisTitle
            End Get
        End Property

        Public ReadOnly Property Stages As SpcStageDefinition()
            Get
                Dim optionsSnapshot As SpcAnalysisOptions = _analysisOptions.Copy()
                Return optionsSnapshot.Stages
            End Get
        End Property

        Public ReadOnly Property Exclusions As SpcExclusionDefinition()
            Get
                Dim optionsSnapshot As SpcAnalysisOptions = _analysisOptions.Copy()
                Return optionsSnapshot.Exclusions
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Describes one process parameter used to construct a panel's centre line or limits.
    ''' </summary>
    Public NotInheritable Class SpcParameterEstimate
        Private ReadOnly _stageId As String
        Private ReadOnly _panelType As SpcPanelType
        Private ReadOnly _parameterName As String
        Private ReadOnly _displayName As String
        Private ReadOnly _value As Double
        Private ReadOnly _standardError As Nullable(Of Double)
        Private ReadOnly _limitMode As SpcStageLimitMode
        Private ReadOnly _sourceStageId As String
        Private ReadOnly _method As String
        Private ReadOnly _sampleCount As Nullable(Of Integer)

        Public Sub New(stageId As String,
                       panelType As SpcPanelType,
                       parameterName As String,
                       value As Double,
                       limitMode As SpcStageLimitMode,
                       Optional standardError As Nullable(Of Double) = Nothing,
                       Optional sourceStageId As String = Nothing,
                       Optional method As String = Nothing,
                       Optional displayName As String = Nothing,
                       Optional sampleCount As Nullable(Of Integer) = Nothing)

            Dim normalizedStageId As String = SpcModelGuards.RequireText(stageId,
                                                                         NameOf(stageId))
            If Not [Enum].IsDefined(GetType(SpcPanelType), panelType) Then
                Throw New ArgumentOutOfRangeException(NameOf(panelType))
            End If
            Dim normalizedName As String = SpcModelGuards.RequireText(parameterName,
                                                                      NameOf(parameterName))
            SpcModelGuards.ValidateFinite(value, NameOf(value))
            SpcModelGuards.ValidateOptionalNonnegative(standardError,
                                                       NameOf(standardError))
            If Not [Enum].IsDefined(GetType(SpcStageLimitMode), limitMode) Then
                Throw New ArgumentOutOfRangeException(NameOf(limitMode))
            End If
            If sampleCount.HasValue AndAlso sampleCount.Value < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(sampleCount),
                    "The parameter-estimation sample count must be nonnegative.")
            End If

            Dim normalizedSourceStage As String =
                SpcModelGuards.NormalizeOptionalText(sourceStageId)
            If limitMode = SpcStageLimitMode.UseReferenceStage AndAlso
               normalizedSourceStage.Length = 0 Then
                Throw New ArgumentException(
                    "A source stage is required for a reference-stage parameter.",
                    NameOf(sourceStageId))
            End If

            Dim normalizedDisplayName As String =
                SpcModelGuards.NormalizeOptionalText(displayName)
            If normalizedDisplayName.Length = 0 Then normalizedDisplayName = normalizedName

            _stageId = normalizedStageId
            _panelType = panelType
            _parameterName = normalizedName
            _displayName = normalizedDisplayName
            _value = value
            _standardError = standardError
            _limitMode = limitMode
            _sourceStageId = normalizedSourceStage
            _method = SpcModelGuards.NormalizeOptionalText(method)
            _sampleCount = sampleCount
        End Sub

        Public ReadOnly Property StageId As String
            Get
                Return _stageId
            End Get
        End Property

        Public ReadOnly Property PanelType As SpcPanelType
            Get
                Return _panelType
            End Get
        End Property

        Public ReadOnly Property ParameterName As String
            Get
                Return _parameterName
            End Get
        End Property

        Public ReadOnly Property DisplayName As String
            Get
                Return _displayName
            End Get
        End Property

        Public ReadOnly Property Value As Double
            Get
                Return _value
            End Get
        End Property

        Public ReadOnly Property StandardError As Nullable(Of Double)
            Get
                Return _standardError
            End Get
        End Property

        Public ReadOnly Property LimitMode As SpcStageLimitMode
            Get
                Return _limitMode
            End Get
        End Property

        Public ReadOnly Property SourceStageId As String
            Get
                Return _sourceStageId
            End Get
        End Property

        Public ReadOnly Property Method As String
            Get
                Return _method
            End Get
        End Property

        Public ReadOnly Property SampleCount As Nullable(Of Integer)
            Get
                Return _sampleCount
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Immutable calculated result for one ordered point in one control-chart panel.
    ''' </summary>
    ''' <remarks>
    ''' Point-specific centre and limit values support unequal p-chart denominators,
    ''' unequal u-chart exposures, multiple stages, and later dynamic EWMA limits.
    ''' </remarks>
    Public NotInheritable Class SpcPointResult
        Private ReadOnly _pointIndex As Integer
        Private ReadOnly _sourceRowIndices As Integer()
        Private ReadOnly _label As String
        Private ReadOnly _sequenceValue As Nullable(Of Double)
        Private ReadOnly _stageId As String
        Private ReadOnly _phase As SpcPhase
        Private ReadOnly _value As Double
        Private ReadOnly _centerLine As Double
        Private ReadOnly _standardError As Double
        Private ReadOnly _standardizedValue As Double
        Private ReadOnly _lowerControlLimit As Double
        Private ReadOnly _upperControlLimit As Double
        Private ReadOnly _lowerOneSigmaLimit As Double
        Private ReadOnly _upperOneSigmaLimit As Double
        Private ReadOnly _lowerTwoSigmaLimit As Double
        Private ReadOnly _upperTwoSigmaLimit As Double
        Private ReadOnly _effectiveSampleSize As Double
        Private ReadOnly _exposure As Double
        Private ReadOnly _includedInParameterEstimation As Boolean
        Private ReadOnly _includedInRuleEvaluation As Boolean
        Private ReadOnly _exclusionScope As SpcExclusionScope
        Private ReadOnly _exclusionReason As String
        Private ReadOnly _signalRuleNumbers As Integer()

        Public Sub New(pointIndex As Integer,
                       value As Double,
                       centerLine As Double,
                       lowerControlLimit As Double,
                       upperControlLimit As Double,
                       Optional label As String = Nothing,
                       Optional stageId As String = "Stage1",
                       Optional phase As SpcPhase = SpcPhase.PhaseI,
                       Optional sequenceValue As Nullable(Of Double) = Nothing,
                       Optional standardError As Double = Double.NaN,
                       Optional standardizedValue As Double = Double.NaN,
                       Optional lowerOneSigmaLimit As Double = Double.NaN,
                       Optional upperOneSigmaLimit As Double = Double.NaN,
                       Optional lowerTwoSigmaLimit As Double = Double.NaN,
                       Optional upperTwoSigmaLimit As Double = Double.NaN,
                       Optional effectiveSampleSize As Double = Double.NaN,
                       Optional exposure As Double = Double.NaN,
                       Optional sourceRowIndices As Integer() = Nothing,
                       Optional includedInParameterEstimation As Boolean = True,
                       Optional includedInRuleEvaluation As Boolean = True,
                       Optional exclusionScope As SpcExclusionScope = SpcExclusionScope.None,
                       Optional exclusionReason As String = Nothing,
                       Optional signalRuleNumbers As Integer() = Nothing)

            If pointIndex < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(pointIndex),
                    "The point index must be nonnegative.")
            End If
            If Not [Enum].IsDefined(GetType(SpcPhase), phase) Then
                Throw New ArgumentOutOfRangeException(NameOf(phase))
            End If

            SpcModelGuards.ValidateFiniteOrNaN(value, NameOf(value))
            SpcModelGuards.ValidateFiniteOrNaN(centerLine, NameOf(centerLine))
            SpcModelGuards.ValidateFiniteOrNaN(lowerControlLimit,
                                               NameOf(lowerControlLimit))
            SpcModelGuards.ValidateFiniteOrNaN(upperControlLimit,
                                               NameOf(upperControlLimit))
            SpcModelGuards.ValidateFiniteOrNaN(standardError, NameOf(standardError))
            SpcModelGuards.ValidateFiniteOrNaN(standardizedValue,
                                               NameOf(standardizedValue))
            SpcModelGuards.ValidateFiniteOrNaN(lowerOneSigmaLimit,
                                               NameOf(lowerOneSigmaLimit))
            SpcModelGuards.ValidateFiniteOrNaN(upperOneSigmaLimit,
                                               NameOf(upperOneSigmaLimit))
            SpcModelGuards.ValidateFiniteOrNaN(lowerTwoSigmaLimit,
                                               NameOf(lowerTwoSigmaLimit))
            SpcModelGuards.ValidateFiniteOrNaN(upperTwoSigmaLimit,
                                               NameOf(upperTwoSigmaLimit))
            SpcModelGuards.ValidateFiniteOrNaN(effectiveSampleSize,
                                               NameOf(effectiveSampleSize))
            SpcModelGuards.ValidateFiniteOrNaN(exposure, NameOf(exposure))
            SpcModelGuards.ValidateOptionalFinite(sequenceValue, NameOf(sequenceValue))

            If SpcModelGuards.IsFinite(standardError) AndAlso standardError < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(standardError),
                    "A point standard error must be nonnegative.")
            End If
            If SpcModelGuards.IsFinite(effectiveSampleSize) AndAlso
               effectiveSampleSize < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(effectiveSampleSize),
                    "An effective sample size must be nonnegative.")
            End If
            If SpcModelGuards.IsFinite(exposure) AndAlso exposure < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(exposure),
                    "Exposure must be nonnegative.")
            End If

            SpcModelGuards.ValidateLimitOrdering(centerLine,
                                                 lowerControlLimit,
                                                 upperControlLimit,
                                                 lowerOneSigmaLimit,
                                                 upperOneSigmaLimit,
                                                 lowerTwoSigmaLimit,
                                                 upperTwoSigmaLimit)
            SpcModelGuards.ValidateExclusionScope(exclusionScope, NameOf(exclusionScope))

            If (exclusionScope And SpcExclusionScope.ParameterEstimation) <>
               SpcExclusionScope.None AndAlso
               includedInParameterEstimation Then
                Throw New ArgumentException(
                    "A point excluded from parameter estimation cannot be marked as included.",
                    NameOf(includedInParameterEstimation))
            End If
            If (exclusionScope And SpcExclusionScope.RuleEvaluation) <>
               SpcExclusionScope.None AndAlso
               includedInRuleEvaluation Then
                Throw New ArgumentException(
                    "A point excluded from rule evaluation cannot be marked as included.",
                    NameOf(includedInRuleEvaluation))
            End If

            Dim copiedRows As Integer()
            If sourceRowIndices Is Nothing OrElse sourceRowIndices.Length = 0 Then
                copiedRows = {pointIndex}
            Else
                copiedRows = SpcModelGuards.CopyUniqueNonnegativeIndices(
                    sourceRowIndices,
                    NameOf(sourceRowIndices))
            End If

            Dim normalizedLabel As String = SpcModelGuards.NormalizeOptionalText(label)
            If normalizedLabel.Length = 0 Then
                normalizedLabel = (pointIndex + 1).ToString(CultureInfo.InvariantCulture)
            End If

            _pointIndex = pointIndex
            _sourceRowIndices = copiedRows
            _label = normalizedLabel
            _sequenceValue = sequenceValue
            _stageId = SpcModelGuards.RequireText(stageId, NameOf(stageId))
            _phase = phase
            _value = value
            _centerLine = centerLine
            _standardError = standardError
            _standardizedValue = standardizedValue
            _lowerControlLimit = lowerControlLimit
            _upperControlLimit = upperControlLimit
            _lowerOneSigmaLimit = lowerOneSigmaLimit
            _upperOneSigmaLimit = upperOneSigmaLimit
            _lowerTwoSigmaLimit = lowerTwoSigmaLimit
            _upperTwoSigmaLimit = upperTwoSigmaLimit
            _effectiveSampleSize = effectiveSampleSize
            _exposure = exposure
            _includedInParameterEstimation = includedInParameterEstimation
            _includedInRuleEvaluation = includedInRuleEvaluation
            _exclusionScope = exclusionScope
            _exclusionReason = SpcModelGuards.NormalizeOptionalText(exclusionReason)
            _signalRuleNumbers = SpcModelGuards.CopyUniquePositiveIntegers(
                signalRuleNumbers,
                NameOf(signalRuleNumbers))
        End Sub

        Public ReadOnly Property PointIndex As Integer
            Get
                Return _pointIndex
            End Get
        End Property

        Public ReadOnly Property SourceRowIndices As Integer()
            Get
                Return CType(_sourceRowIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property Label As String
            Get
                Return _label
            End Get
        End Property

        Public ReadOnly Property SequenceValue As Nullable(Of Double)
            Get
                Return _sequenceValue
            End Get
        End Property

        Public ReadOnly Property StageId As String
            Get
                Return _stageId
            End Get
        End Property

        Public ReadOnly Property Phase As SpcPhase
            Get
                Return _phase
            End Get
        End Property

        Public ReadOnly Property Value As Double
            Get
                Return _value
            End Get
        End Property

        Public ReadOnly Property CenterLine As Double
            Get
                Return _centerLine
            End Get
        End Property

        Public ReadOnly Property StandardError As Double
            Get
                Return _standardError
            End Get
        End Property

        Public ReadOnly Property StandardizedValue As Double
            Get
                Return _standardizedValue
            End Get
        End Property

        Public ReadOnly Property LowerControlLimit As Double
            Get
                Return _lowerControlLimit
            End Get
        End Property

        Public ReadOnly Property UpperControlLimit As Double
            Get
                Return _upperControlLimit
            End Get
        End Property

        Public ReadOnly Property LowerOneSigmaLimit As Double
            Get
                Return _lowerOneSigmaLimit
            End Get
        End Property

        Public ReadOnly Property UpperOneSigmaLimit As Double
            Get
                Return _upperOneSigmaLimit
            End Get
        End Property

        Public ReadOnly Property LowerTwoSigmaLimit As Double
            Get
                Return _lowerTwoSigmaLimit
            End Get
        End Property

        Public ReadOnly Property UpperTwoSigmaLimit As Double
            Get
                Return _upperTwoSigmaLimit
            End Get
        End Property

        Public ReadOnly Property EffectiveSampleSize As Double
            Get
                Return _effectiveSampleSize
            End Get
        End Property

        Public ReadOnly Property Exposure As Double
            Get
                Return _exposure
            End Get
        End Property

        Public ReadOnly Property IncludedInParameterEstimation As Boolean
            Get
                Return _includedInParameterEstimation
            End Get
        End Property

        Public ReadOnly Property IncludedInRuleEvaluation As Boolean
            Get
                Return _includedInRuleEvaluation
            End Get
        End Property

        Public ReadOnly Property ExclusionScope As SpcExclusionScope
            Get
                Return _exclusionScope
            End Get
        End Property

        Public ReadOnly Property ExclusionReason As String
            Get
                Return _exclusionReason
            End Get
        End Property

        Public ReadOnly Property IsExplicitlyExcluded As Boolean
            Get
                Return _exclusionScope <> SpcExclusionScope.None
            End Get
        End Property

        Public ReadOnly Property SignalRuleNumbers As Integer()
            Get
                Return CType(_signalRuleNumbers.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property IsSignalled As Boolean
            Get
                Return _signalRuleNumbers.Length > 0
            End Get
        End Property

        Public ReadOnly Property HasFiniteValue As Boolean
            Get
                Return SpcModelGuards.IsFinite(_value)
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Describes one detected special-cause rule occurrence.
    ''' </summary>
    Public NotInheritable Class SpcSignalResult
        Private ReadOnly _panelType As SpcPanelType
        Private ReadOnly _stageId As String
        Private ReadOnly _rule As SpcRuleDefinition
        Private ReadOnly _triggeredSide As SpcRuleSide
        Private ReadOnly _terminalPointIndex As Integer
        Private ReadOnly _windowStartPointIndex As Integer
        Private ReadOnly _windowEndPointIndex As Integer
        Private ReadOnly _contributingPointIndices As Integer()
        Private ReadOnly _markedPointIndices As Integer()
        Private ReadOnly _message As String

        Public Sub New(panelType As SpcPanelType,
                       stageId As String,
                       rule As SpcRuleDefinition,
                       terminalPointIndex As Integer,
                       windowStartPointIndex As Integer,
                       windowEndPointIndex As Integer,
                       Optional triggeredSide As SpcRuleSide = SpcRuleSide.EitherSide,
                       Optional contributingPointIndices As Integer() = Nothing,
                       Optional markedPointIndices As Integer() = Nothing,
                       Optional message As String = Nothing)

            If Not [Enum].IsDefined(GetType(SpcPanelType), panelType) Then
                Throw New ArgumentOutOfRangeException(NameOf(panelType))
            End If
            Dim normalizedStageId As String = SpcModelGuards.RequireText(stageId,
                                                                         NameOf(stageId))
            If rule Is Nothing Then Throw New ArgumentNullException(NameOf(rule))
            If Not [Enum].IsDefined(GetType(SpcRuleSide), triggeredSide) Then
                Throw New ArgumentOutOfRangeException(NameOf(triggeredSide))
            End If
            If windowStartPointIndex < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(windowStartPointIndex))
            End If
            If windowEndPointIndex < windowStartPointIndex Then
                Throw New ArgumentOutOfRangeException(NameOf(windowEndPointIndex),
                    "The signal window end must not precede its start.")
            End If
            If terminalPointIndex < windowStartPointIndex OrElse
               terminalPointIndex > windowEndPointIndex Then
                Throw New ArgumentOutOfRangeException(NameOf(terminalPointIndex),
                    "The terminal point must fall within the signal window.")
            End If

            Dim contributing As Integer()
            If contributingPointIndices Is Nothing OrElse
               contributingPointIndices.Length = 0 Then
                contributing = {terminalPointIndex}
            Else
                contributing = SpcModelGuards.CopyUniqueNonnegativeIndices(
                    contributingPointIndices,
                    NameOf(contributingPointIndices))
            End If

            Dim marked As Integer()
            If markedPointIndices Is Nothing OrElse markedPointIndices.Length = 0 Then
                marked = {terminalPointIndex}
            Else
                marked = SpcModelGuards.CopyUniqueNonnegativeIndices(
                    markedPointIndices,
                    NameOf(markedPointIndices))
            End If

            SpcModelGuards.ValidateIndicesInWindow(contributing,
                                                   windowStartPointIndex,
                                                   windowEndPointIndex,
                                                   NameOf(contributingPointIndices))
            SpcModelGuards.ValidateIndicesInWindow(marked,
                                                   windowStartPointIndex,
                                                   windowEndPointIndex,
                                                   NameOf(markedPointIndices))
            If Not SpcModelGuards.Contains(contributing, terminalPointIndex) Then
                Throw New ArgumentException(
                    "The contributing points must include the terminal point.",
                    NameOf(contributingPointIndices))
            End If

            Dim normalizedMessage As String = SpcModelGuards.NormalizeOptionalText(message)
            If normalizedMessage.Length = 0 Then
                normalizedMessage = "Rule " &
                    rule.RuleNumber.ToString(CultureInfo.InvariantCulture) &
                    " signalled at point " &
                    (terminalPointIndex + 1).ToString(CultureInfo.InvariantCulture) & "."
            End If

            _panelType = panelType
            _stageId = normalizedStageId
            _rule = rule
            _triggeredSide = triggeredSide
            _terminalPointIndex = terminalPointIndex
            _windowStartPointIndex = windowStartPointIndex
            _windowEndPointIndex = windowEndPointIndex
            _contributingPointIndices = contributing
            _markedPointIndices = marked
            _message = normalizedMessage
        End Sub

        Public ReadOnly Property PanelType As SpcPanelType
            Get
                Return _panelType
            End Get
        End Property

        Public ReadOnly Property StageId As String
            Get
                Return _stageId
            End Get
        End Property

        Public ReadOnly Property Rule As SpcRuleDefinition
            Get
                Return _rule
            End Get
        End Property

        Public ReadOnly Property RuleCode As String
            Get
                Return _rule.RuleCode
            End Get
        End Property

        Public ReadOnly Property RuleNumber As Integer
            Get
                Return _rule.RuleNumber
            End Get
        End Property

        Public ReadOnly Property TriggeredSide As SpcRuleSide
            Get
                Return _triggeredSide
            End Get
        End Property

        Public ReadOnly Property TerminalPointIndex As Integer
            Get
                Return _terminalPointIndex
            End Get
        End Property

        Public ReadOnly Property WindowStartPointIndex As Integer
            Get
                Return _windowStartPointIndex
            End Get
        End Property

        Public ReadOnly Property WindowEndPointIndex As Integer
            Get
                Return _windowEndPointIndex
            End Get
        End Property

        Public ReadOnly Property ContributingPointIndices As Integer()
            Get
                Return CType(_contributingPointIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property MarkedPointIndices As Integer()
            Get
                Return CType(_markedPointIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property Message As String
            Get
                Return _message
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Immutable result for one chart panel, such as X-bar, R, individuals, or MR.
    ''' </summary>
    Public NotInheritable Class SpcPanelResult
        Private ReadOnly _panelType As SpcPanelType
        Private ReadOnly _displayName As String
        Private ReadOnly _valueAxisTitle As String
        Private ReadOnly _points As SpcPointResult()
        Private ReadOnly _parameterEstimates As SpcParameterEstimate()
        Private ReadOnly _signals As SpcSignalResult()
        Private ReadOnly _warnings As String()

        Public Sub New(panelType As SpcPanelType,
                       displayName As String,
                       points As SpcPointResult(),
                       Optional valueAxisTitle As String = Nothing,
                       Optional parameterEstimates As SpcParameterEstimate() = Nothing,
                       Optional signals As SpcSignalResult() = Nothing,
                       Optional warnings As String() = Nothing)

            If Not [Enum].IsDefined(GetType(SpcPanelType), panelType) Then
                Throw New ArgumentOutOfRangeException(NameOf(panelType))
            End If
            Dim normalizedDisplayName As String =
                SpcModelGuards.RequireText(displayName, NameOf(displayName))
            If points Is Nothing OrElse points.Length = 0 Then
                Throw New ArgumentException(
                    "A panel must contain at least one chart point.",
                    NameOf(points))
            End If

            Dim copiedPoints As SpcPointResult() = CType(points.Clone(), SpcPointResult())
            Dim pointMap As New Dictionary(Of Integer, SpcPointResult)()
            Dim previousPointIndex As Integer = -1
            For i As Integer = 0 To copiedPoints.Length - 1
                Dim point As SpcPointResult = copiedPoints(i)
                If point Is Nothing Then
                    Throw New ArgumentException("Panel points must not be null.", NameOf(points))
                End If
                If point.PointIndex <= previousPointIndex Then
                    Throw New ArgumentException(
                        "Panel points must be ordered by unique, increasing point index.",
                        NameOf(points))
                End If
                pointMap.Add(point.PointIndex, point)
                previousPointIndex = point.PointIndex
            Next

            Dim copiedParameters As SpcParameterEstimate() =
                SpcModelGuards.CopyReferenceArray(Of SpcParameterEstimate)(parameterEstimates)
            For i As Integer = 0 To copiedParameters.Length - 1
                If copiedParameters(i) Is Nothing Then
                    Throw New ArgumentException(
                        "Panel parameter estimates must not be null.",
                        NameOf(parameterEstimates))
                End If
                If copiedParameters(i).PanelType <> panelType Then
                    Throw New ArgumentException(
                        "Every parameter estimate must belong to its containing panel.",
                        NameOf(parameterEstimates))
                End If
            Next

            Dim copiedSignals As SpcSignalResult() =
                SpcModelGuards.CopyReferenceArray(Of SpcSignalResult)(signals)
            For i As Integer = 0 To copiedSignals.Length - 1
                Dim signal As SpcSignalResult = copiedSignals(i)
                If signal Is Nothing Then
                    Throw New ArgumentException("Panel signals must not be null.",
                                                NameOf(signals))
                End If
                If signal.PanelType <> panelType Then
                    Throw New ArgumentException(
                        "Every signal must belong to its containing panel.",
                        NameOf(signals))
                End If
                If Not pointMap.ContainsKey(signal.TerminalPointIndex) Then
                    Throw New ArgumentException(
                        "A signal terminal point was not found in the containing panel.",
                        NameOf(signals))
                End If
                If Not String.Equals(
                    pointMap(signal.TerminalPointIndex).StageId,
                    signal.StageId,
                    StringComparison.OrdinalIgnoreCase) Then
                    Throw New ArgumentException(
                        "A signal and its terminal point must belong to the same stage.",
                        NameOf(signals))
                End If

                Dim contributingIndices As Integer() = signal.ContributingPointIndices
                For j As Integer = 0 To contributingIndices.Length - 1
                    Dim contributingIndex As Integer = contributingIndices(j)
                    If Not pointMap.ContainsKey(contributingIndex) Then
                        Throw New ArgumentException(
                            "A contributing signal point was not found in the containing panel.",
                            NameOf(signals))
                    End If
                    If Not String.Equals(
                        pointMap(contributingIndex).StageId,
                        signal.StageId,
                        StringComparison.OrdinalIgnoreCase) Then
                        Throw New ArgumentException(
                            "A rule signal must not cross a stage boundary.",
                            NameOf(signals))
                    End If
                Next

                Dim markedIndices As Integer() = signal.MarkedPointIndices
                For j As Integer = 0 To markedIndices.Length - 1
                    Dim markedIndex As Integer = markedIndices(j)
                    If Not pointMap.ContainsKey(markedIndex) Then
                        Throw New ArgumentException(
                            "A marked signal point was not found in the containing panel.",
                            NameOf(signals))
                    End If
                    If Not String.Equals(
                        pointMap(markedIndex).StageId,
                        signal.StageId,
                        StringComparison.OrdinalIgnoreCase) Then
                        Throw New ArgumentException(
                            "A marked signal point must belong to the signal stage.",
                            NameOf(signals))
                    End If
                    If Not SpcModelGuards.Contains(
                        pointMap(markedIndex).SignalRuleNumbers,
                        signal.RuleNumber) Then
                        Throw New ArgumentException(
                            "A marked point does not retain the corresponding violated rule number.",
                            NameOf(points))
                    End If
                Next
            Next

            For i As Integer = 0 To copiedPoints.Length - 1
                Dim point As SpcPointResult = copiedPoints(i)
                Dim ruleNumbers As Integer() = point.SignalRuleNumbers
                For j As Integer = 0 To ruleNumbers.Length - 1
                    If Not SpcModelGuards.HasSignalForPoint(copiedSignals,
                                                          point.PointIndex,
                                                          ruleNumbers(j)) Then
                        Throw New ArgumentException(
                            "A point retains a rule number with no corresponding panel signal.",
                            NameOf(points))
                    End If
                Next
            Next

            _panelType = panelType
            _displayName = normalizedDisplayName
            _valueAxisTitle = SpcModelGuards.NormalizeOptionalText(valueAxisTitle)
            _points = copiedPoints
            _parameterEstimates = copiedParameters
            _signals = copiedSignals
            _warnings = SpcModelGuards.CopyMessages(warnings)
        End Sub

        Public ReadOnly Property PanelType As SpcPanelType
            Get
                Return _panelType
            End Get
        End Property

        Public ReadOnly Property DisplayName As String
            Get
                Return _displayName
            End Get
        End Property

        Public ReadOnly Property ValueAxisTitle As String
            Get
                Return _valueAxisTitle
            End Get
        End Property

        Public ReadOnly Property Points As SpcPointResult()
            Get
                Return CType(_points.Clone(), SpcPointResult())
            End Get
        End Property

        Public ReadOnly Property ParameterEstimates As SpcParameterEstimate()
            Get
                Return CType(_parameterEstimates.Clone(), SpcParameterEstimate())
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

        Public ReadOnly Property PointCount As Integer
            Get
                Return _points.Length
            End Get
        End Property

        Public ReadOnly Property SignalCount As Integer
            Get
                Return _signals.Length
            End Get
        End Property

        Public ReadOnly Property SignalledPointCount As Integer
            Get
                Dim count As Integer = 0
                For i As Integer = 0 To _points.Length - 1
                    If _points(i).IsSignalled Then count += 1
                Next
                Return count
            End Get
        End Property

        Public Function GetPoint(pointIndex As Integer) As SpcPointResult
            For i As Integer = 0 To _points.Length - 1
                If _points(i).PointIndex = pointIndex Then Return _points(i)
            Next
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Complete immutable snapshot returned by the SPC calculation engine.
    ''' </summary>
    ''' <remarks>
    ''' The result contains no Excel chart, range, workbook, or WinForms object. It
    ''' can therefore be consumed by unit tests, worksheet UDFs, the Excel renderer,
    ''' and a future Office.js service from the same calculated snapshot.
    ''' </remarks>
    Public NotInheritable Class SpcFitResult
        Private ReadOnly _request As SpcFitRequest
        Private ReadOnly _chartFamily As SpcChartFamily
        Private ReadOnly _panels As SpcPanelResult()
        Private ReadOnly _signals As SpcSignalResult()
        Private ReadOnly _parameterEstimates As SpcParameterEstimate()
        Private ReadOnly _warnings As String()
        Private ReadOnly _executionTimeMilliseconds As Double
        Private ReadOnly _executionStartedUtc As Nullable(Of DateTime)
        Private ReadOnly _executionCompletedUtc As Nullable(Of DateTime)

        Public Sub New(request As SpcFitRequest,
                       panels As SpcPanelResult(),
                       Optional warnings As String() = Nothing,
                       Optional executionTimeMilliseconds As Double = Double.NaN,
                       Optional executionStartedUtc As Nullable(Of DateTime) = Nothing,
                       Optional executionCompletedUtc As Nullable(Of DateTime) = Nothing)

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If panels Is Nothing OrElse panels.Length = 0 Then
                Throw New ArgumentException(
                    "An SPC fit result must contain at least one panel.",
                    NameOf(panels))
            End If
            SpcModelGuards.ValidateFiniteOrNaN(executionTimeMilliseconds,
                                               NameOf(executionTimeMilliseconds))
            If SpcModelGuards.IsFinite(executionTimeMilliseconds) AndAlso
               executionTimeMilliseconds < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(executionTimeMilliseconds),
                    "Execution time must be nonnegative.")
            End If
            If executionStartedUtc.HasValue AndAlso executionCompletedUtc.HasValue AndAlso
               executionCompletedUtc.Value < executionStartedUtc.Value Then
                Throw New ArgumentException(
                    "The completion timestamp must not precede the start timestamp.")
            End If

            Dim copiedPanels As SpcPanelResult() = CType(panels.Clone(), SpcPanelResult())
            Dim seenPanelTypes As New HashSet(Of SpcPanelType)()
            For i As Integer = 0 To copiedPanels.Length - 1
                If copiedPanels(i) Is Nothing Then
                    Throw New ArgumentException("Result panels must not be null.",
                                                NameOf(panels))
                End If
                If Not seenPanelTypes.Add(copiedPanels(i).PanelType) Then
                    Throw New ArgumentException(
                        "A fit result must not contain duplicate panel types.",
                        NameOf(panels))
                End If
            Next

            _request = request
            _chartFamily = GetChartFamily(request.ChartType)
            _panels = copiedPanels
            _signals = FlattenSignals(copiedPanels)
            _parameterEstimates = FlattenParameterEstimates(copiedPanels)
            _warnings = SpcModelGuards.CopyMessages(warnings)
            _executionTimeMilliseconds = executionTimeMilliseconds
            _executionStartedUtc = executionStartedUtc
            _executionCompletedUtc = executionCompletedUtc
        End Sub

        Public ReadOnly Property Request As SpcFitRequest
            Get
                Return _request
            End Get
        End Property

        Public ReadOnly Property ChartType As SpcChartType
            Get
                Return _request.ChartType
            End Get
        End Property

        Public ReadOnly Property ChartFamily As SpcChartFamily
            Get
                Return _chartFamily
            End Get
        End Property

        Public ReadOnly Property DataLayout As SpcDataLayout
            Get
                Return _request.DataLayout
            End Get
        End Property

        Public ReadOnly Property ChartTitle As String
            Get
                Return _request.ChartTitle
            End Get
        End Property

        Public ReadOnly Property Panels As SpcPanelResult()
            Get
                Return CType(_panels.Clone(), SpcPanelResult())
            End Get
        End Property

        Public ReadOnly Property Signals As SpcSignalResult()
            Get
                Return CType(_signals.Clone(), SpcSignalResult())
            End Get
        End Property

        Public ReadOnly Property ParameterEstimates As SpcParameterEstimate()
            Get
                Return CType(_parameterEstimates.Clone(), SpcParameterEstimate())
            End Get
        End Property

        Public ReadOnly Property Warnings As String()
            Get
                Return CType(_warnings.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property ExecutionTimeMilliseconds As Double
            Get
                Return _executionTimeMilliseconds
            End Get
        End Property

        Public ReadOnly Property ExecutionStartedUtc As Nullable(Of DateTime)
            Get
                Return _executionStartedUtc
            End Get
        End Property

        Public ReadOnly Property ExecutionCompletedUtc As Nullable(Of DateTime)
            Get
                Return _executionCompletedUtc
            End Get
        End Property

        Public ReadOnly Property PanelCount As Integer
            Get
                Return _panels.Length
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of distinct ordered chart-point indices across all panels.
        ''' </summary>
        Public ReadOnly Property ChartPointCount As Integer
            Get
                Dim indices As New HashSet(Of Integer)()
                For i As Integer = 0 To _panels.Length - 1
                    Dim points As SpcPointResult() = _panels(i).Points
                    For j As Integer = 0 To points.Length - 1
                        indices.Add(points(j).PointIndex)
                    Next
                Next
                Return indices.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets the total number of point rows across all panels.
        ''' </summary>
        Public ReadOnly Property PanelPointCount As Integer
            Get
                Dim count As Integer = 0
                For i As Integer = 0 To _panels.Length - 1
                    count += _panels(i).PointCount
                Next
                Return count
            End Get
        End Property

        Public ReadOnly Property SignalCount As Integer
            Get
                Return _signals.Length
            End Get
        End Property

        Public ReadOnly Property SignalledPanelPointCount As Integer
            Get
                Dim count As Integer = 0
                For i As Integer = 0 To _panels.Length - 1
                    count += _panels(i).SignalledPointCount
                Next
                Return count
            End Get
        End Property

        Public ReadOnly Property IsInControlBySelectedRules As Boolean
            Get
                Return _signals.Length = 0
            End Get
        End Property

        Public Function GetPanel(panelType As SpcPanelType) As SpcPanelResult
            For i As Integer = 0 To _panels.Length - 1
                If _panels(i).PanelType = panelType Then Return _panels(i)
            Next
            Return Nothing
        End Function

        Private Shared Function FlattenSignals(panels As SpcPanelResult()) As SpcSignalResult()
            Dim values As New List(Of SpcSignalResult)()
            For i As Integer = 0 To panels.Length - 1
                values.AddRange(panels(i).Signals)
            Next
            Return values.ToArray()
        End Function

        Private Shared Function FlattenParameterEstimates(
            panels As SpcPanelResult()) As SpcParameterEstimate()

            Dim values As New List(Of SpcParameterEstimate)()
            For i As Integer = 0 To panels.Length - 1
                values.AddRange(panels(i).ParameterEstimates)
            Next
            Return values.ToArray()
        End Function

        Private Shared Function GetChartFamily(chartType As SpcChartType) As SpcChartFamily
            Select Case chartType
                Case SpcChartType.RunChart
                    Return SpcChartFamily.Run

                Case SpcChartType.Individuals,
                     SpcChartType.MovingRange,
                     SpcChartType.IndividualsMovingRange,
                     SpcChartType.XBar,
                     SpcChartType.SubgroupRange,
                     SpcChartType.SubgroupStandardDeviation,
                     SpcChartType.XBarR,
                     SpcChartType.XBarS
                    Return SpcChartFamily.ShewhartVariables

                Case SpcChartType.PChart,
                     SpcChartType.NpChart,
                     SpcChartType.CChart,
                     SpcChartType.UChart,
                     SpcChartType.LaneyPPrime,
                     SpcChartType.LaneyUPrime
                    Return SpcChartFamily.ShewhartAttributes

                Case SpcChartType.GChart, SpcChartType.TChart
                    Return SpcChartFamily.RareEvent

                Case SpcChartType.Cusum,
                     SpcChartType.Ewma,
                     SpcChartType.MovingAverage
                    Return SpcChartFamily.TimeWeighted

                Case SpcChartType.HotellingT2,
                     SpcChartType.GeneralizedVariance,
                     SpcChartType.PcaT2,
                     SpcChartType.PcaQ,
                     SpcChartType.Mewma,
                     SpcChartType.Mcusum
                    Return SpcChartFamily.Multivariate

                Case SpcChartType.ShortRunZMovingRange,
                     SpcChartType.BetweenWithin,
                     SpcChartType.ResidualChart,
                     SpcChartType.ProfileChart,
                     SpcChartType.RiskAdjustedChart
                    Return SpcChartFamily.Specialized

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(chartType))
            End Select
        End Function
    End Class

    ''' <summary>
    ''' Shared validation and defensive-copy helpers for immutable SPC model classes.
    ''' </summary>
    Friend NotInheritable Class SpcModelGuards
        Private Sub New()
        End Sub

        Friend Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Friend Shared Sub ValidateFinite(value As Double, parameterName As String)
            If Not IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be finite.")
            End If
        End Sub

        Friend Shared Sub ValidateFiniteOrNaN(value As Double, parameterName As String)
            If Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "Infinity is not a valid value.")
            End If
        End Sub

        Friend Shared Sub ValidateOptionalFinite(value As Nullable(Of Double),
                                                 parameterName As String)
            If value.HasValue Then ValidateFinite(value.Value, parameterName)
        End Sub

        Friend Shared Sub ValidateOptionalNonnegative(value As Nullable(Of Double),
                                                      parameterName As String)
            If Not value.HasValue Then Return
            ValidateFinite(value.Value, parameterName)
            If value.Value < 0.0 Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be nonnegative.")
            End If
        End Sub

        Friend Shared Sub ValidateOptionalPositive(value As Nullable(Of Double),
                                                   parameterName As String)
            If Not value.HasValue Then Return
            ValidateFinite(value.Value, parameterName)
            If value.Value <= 0.0 Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value must be positive.")
            End If
        End Sub

        Friend Shared Sub ValidateOptionalRange(value As Nullable(Of Double),
                                                lower As Double,
                                                upper As Double,
                                                parameterName As String)
            If Not value.HasValue Then Return
            ValidateFinite(value.Value, parameterName)
            If value.Value < lower OrElse value.Value > upper Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The value lies outside the permitted range.")
            End If
        End Sub

        Friend Shared Function NormalizeOptionalText(value As String) As String
            Return If(value, String.Empty).Trim()
        End Function

        Friend Shared Function RequireText(value As String, parameterName As String) As String
            Dim normalized As String = NormalizeOptionalText(value)
            If normalized.Length = 0 Then
                Throw New ArgumentException("A non-empty value is required.", parameterName)
            End If
            Return normalized
        End Function

        Friend Shared Function CloneVector(values As Double()) As Double()
            If values Is Nothing Then Return Nothing
            Return CType(values.Clone(), Double())
        End Function

        Friend Shared Function CloneMatrix(values As Double(,)) As Double(,)
            If values Is Nothing Then Return Nothing
            Return CType(values.Clone(), Double(,))
        End Function

        Friend Shared Function CopyTextArray(values As String()) As String()
            If values Is Nothing Then Return Nothing
            Dim copied As String() = CType(values.Clone(), String())
            For i As Integer = 0 To copied.Length - 1
                copied(i) = If(copied(i), String.Empty)
            Next
            Return copied
        End Function

        Friend Shared Function CopyReferenceArray(Of T As Class)(values As T()) As T()
            If values Is Nothing Then Return Array.Empty(Of T)()
            Return CType(values.Clone(), T())
        End Function

        Friend Shared Sub MergeAlignedLength(ByRef rowCount As Integer,
                                             values As Array,
                                             parameterName As String)
            If values Is Nothing Then Return
            If values.Rank <> 1 Then
                Throw New ArgumentException("A row-aligned vector must be one-dimensional.",
                                            parameterName)
            End If
            If values.Length = 0 Then
                Throw New ArgumentException("A supplied row-aligned vector must not be empty.",
                                            parameterName)
            End If
            If rowCount = 0 Then
                rowCount = values.Length
            ElseIf values.Length <> rowCount Then
                Throw New ArgumentException(
                    "All row-aligned arrays must have the same number of rows.",
                    parameterName)
            End If
        End Sub

        Friend Shared Sub ValidateNoInfinity(values As Double(), parameterName As String)
            If values Is Nothing Then Return
            For i As Integer = 0 To values.Length - 1
                If Double.IsInfinity(values(i)) Then
                    Throw New ArgumentOutOfRangeException(parameterName,
                        "Infinity is not a valid numeric input.")
                End If
            Next
        End Sub

        Friend Shared Sub ValidateNoInfinity(values As Double(,), parameterName As String)
            If values Is Nothing Then Return
            For i As Integer = 0 To values.GetLength(0) - 1
                For j As Integer = 0 To values.GetLength(1) - 1
                    If Double.IsInfinity(values(i, j)) Then
                        Throw New ArgumentOutOfRangeException(parameterName,
                            "Infinity is not a valid numeric input.")
                    End If
                Next
            Next
        End Sub

        Friend Shared Function ToColumnMatrix(values As Double(),
                                              parameterName As String) As Double(,)
            If values Is Nothing Then Throw New ArgumentNullException(parameterName)
            If values.Length = 0 Then
                Throw New ArgumentException("At least one value is required.", parameterName)
            End If
            Dim result(values.Length - 1, 0) As Double
            For i As Integer = 0 To values.Length - 1
                result(i, 0) = values(i)
            Next
            Return result
        End Function

        Friend Shared Sub ValidateExclusionScope(scope As SpcExclusionScope,
                                                 parameterName As String)
            Dim numericScope As Integer = CInt(scope)
            If numericScope < 0 OrElse
               (numericScope And Not CInt(SpcExclusionScope.EstimationAndRules)) <> 0 Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "The exclusion scope contains an unsupported value.")
            End If
        End Sub

        Friend Shared Function CopyUniquePositiveIntegers(values As Integer(),
                                                          parameterName As String) As Integer()
            If values Is Nothing OrElse values.Length = 0 Then
                Return Array.Empty(Of Integer)()
            End If
            Dim unique As New HashSet(Of Integer)()
            For i As Integer = 0 To values.Length - 1
                If values(i) <= 0 Then
                    Throw New ArgumentOutOfRangeException(parameterName,
                        "Rule numbers must be positive.")
                End If
                unique.Add(values(i))
            Next
            Dim copied As Integer() = New List(Of Integer)(unique).ToArray()
            Array.Sort(copied)
            Return copied
        End Function

        Friend Shared Function CopyUniqueNonnegativeIndices(values As Integer(),
                                                            parameterName As String) As Integer()
            If values Is Nothing OrElse values.Length = 0 Then
                Return Array.Empty(Of Integer)()
            End If
            Dim unique As New HashSet(Of Integer)()
            For i As Integer = 0 To values.Length - 1
                If values(i) < 0 Then
                    Throw New ArgumentOutOfRangeException(parameterName,
                        "Point and source-row indices must be nonnegative.")
                End If
                unique.Add(values(i))
            Next
            Dim copied As Integer() = New List(Of Integer)(unique).ToArray()
            Array.Sort(copied)
            Return copied
        End Function

        Friend Shared Function Contains(values As Integer(), sought As Integer) As Boolean
            If values Is Nothing Then Return False
            For i As Integer = 0 To values.Length - 1
                If values(i) = sought Then Return True
            Next
            Return False
        End Function

        Friend Shared Sub ValidateIndicesInWindow(values As Integer(),
                                                  firstIndex As Integer,
                                                  lastIndex As Integer,
                                                  parameterName As String)
            For i As Integer = 0 To values.Length - 1
                If values(i) < firstIndex OrElse values(i) > lastIndex Then
                    Throw New ArgumentOutOfRangeException(parameterName,
                        "Every point index must fall within the signal window.")
                End If
            Next
        End Sub

        Friend Shared Function CopyMessages(values As String()) As String()
            If values Is Nothing OrElse values.Length = 0 Then
                Return Array.Empty(Of String)()
            End If

            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim copied As New List(Of String)()
            For i As Integer = 0 To values.Length - 1
                Dim message As String = NormalizeOptionalText(values(i))
                If message.Length > 0 AndAlso seen.Add(message) Then copied.Add(message)
            Next
            Return copied.ToArray()
        End Function

        Friend Shared Function HasSignalForPoint(signals As SpcSignalResult(),
                                                 pointIndex As Integer,
                                                 ruleNumber As Integer) As Boolean
            For i As Integer = 0 To signals.Length - 1
                If signals(i).RuleNumber = ruleNumber AndAlso
                   Contains(signals(i).MarkedPointIndices, pointIndex) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Friend Shared Sub ValidateLimitOrdering(centerLine As Double,
                                                lowerControlLimit As Double,
                                                upperControlLimit As Double,
                                                lowerOneSigmaLimit As Double,
                                                upperOneSigmaLimit As Double,
                                                lowerTwoSigmaLimit As Double,
                                                upperTwoSigmaLimit As Double)

            If IsFinite(lowerControlLimit) AndAlso IsFinite(centerLine) AndAlso
               lowerControlLimit > centerLine Then
                Throw New ArgumentException(
                    "The lower control limit must not exceed the centre line.")
            End If
            If IsFinite(upperControlLimit) AndAlso IsFinite(centerLine) AndAlso
               upperControlLimit < centerLine Then
                Throw New ArgumentException(
                    "The upper control limit must not be below the centre line.")
            End If
            If IsFinite(lowerControlLimit) AndAlso IsFinite(upperControlLimit) AndAlso
               lowerControlLimit > upperControlLimit Then
                Throw New ArgumentException(
                    "The lower control limit must not exceed the upper control limit.")
            End If

            If IsFinite(lowerOneSigmaLimit) AndAlso IsFinite(centerLine) AndAlso
               lowerOneSigmaLimit > centerLine Then
                Throw New ArgumentException(
                    "The lower one-sigma line must not exceed the centre line.")
            End If
            If IsFinite(lowerTwoSigmaLimit) AndAlso IsFinite(lowerOneSigmaLimit) AndAlso
               lowerTwoSigmaLimit > lowerOneSigmaLimit Then
                Throw New ArgumentException(
                    "The lower two-sigma line must not exceed the lower one-sigma line.")
            End If
            If IsFinite(lowerControlLimit) AndAlso IsFinite(lowerTwoSigmaLimit) AndAlso
               lowerControlLimit > lowerTwoSigmaLimit Then
                Throw New ArgumentException(
                    "The lower control limit must not exceed the lower two-sigma line.")
            End If

            If IsFinite(upperOneSigmaLimit) AndAlso IsFinite(centerLine) AndAlso
               upperOneSigmaLimit < centerLine Then
                Throw New ArgumentException(
                    "The upper one-sigma line must not be below the centre line.")
            End If
            If IsFinite(upperTwoSigmaLimit) AndAlso IsFinite(upperOneSigmaLimit) AndAlso
               upperTwoSigmaLimit < upperOneSigmaLimit Then
                Throw New ArgumentException(
                    "The upper two-sigma line must not be below the upper one-sigma line.")
            End If
            If IsFinite(upperControlLimit) AndAlso IsFinite(upperTwoSigmaLimit) AndAlso
               upperControlLimit < upperTwoSigmaLimit Then
                Throw New ArgumentException(
                    "The upper control limit must not be below the upper two-sigma line.")
            End If
        End Sub
    End Class

End Namespace
