Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>Identifies the source of the in-control multivariate model.</summary>
    Public Enum SpcMultivariateModelSource
        EstimateFromPhaseI = 0
        UseHistoricalParameters = 1
    End Enum

    ''' <summary>
    ''' Immutable input for Hotelling T-squared, generalized-variance, PCA,
    ''' MEWMA, and MCUSUM control charts.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Rows are ordered observations and columns are variables. For a subgrouped
    ''' Hotelling or generalized-variance chart, subgroupIds
    ''' identifies the rows belonging to each rational subgroup.
    ''' </para>
    ''' <para>
    ''' A Phase-I row is eligible for model estimation unless its exclusion scope
    ''' contains <see cref="SpcExclusionScope.ParameterEstimation"/>. Phase-II rows
    ''' are monitored against the frozen Phase-I or historical model.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SpcMultivariateRequest
        Private ReadOnly _chartType As SpcChartType
        Private ReadOnly _measurements As Double(,)
        Private ReadOnly _variableNames As String()
        Private ReadOnly _subgroupIds As String()
        Private ReadOnly _labels As String()
        Private ReadOnly _phases As SpcPhase()
        Private ReadOnly _stageIds As String()
        Private ReadOnly _sequenceValues As Double()
        Private ReadOnly _sourceRowIndices As Integer()
        Private ReadOnly _exclusionScopes As SpcExclusionScope()
        Private ReadOnly _exclusionReasons As String()
        Private ReadOnly _missingValuePolicy As SpcMissingValuePolicy
        Private ReadOnly _modelSource As SpcMultivariateModelSource
        Private ReadOnly _historicalMean As Double()
        Private ReadOnly _historicalCovariance As Double(,)
        Private ReadOnly _controlLimitAlpha As Double
        Private ReadOnly _useLowerHotellingLimit As Boolean
        Private ReadOnly _covarianceRegularization As Double
        Private ReadOnly _allowPseudoInverse As Boolean
        Private ReadOnly _pcaUseCorrelationMatrix As Boolean
        Private ReadOnly _pcaComponentCount As Nullable(Of Integer)
        Private ReadOnly _pcaCumulativeVariance As Double
        Private ReadOnly _mewmaLambda As Double
        Private ReadOnly _mewmaControlLimit As Nullable(Of Double)
        Private ReadOnly _mcusumReferenceValue As Double
        Private ReadOnly _mcusumDecisionInterval As Double
        Private ReadOnly _generalizedVarianceSigmaMultiplier As Nullable(Of Double)
        Private ReadOnly _resetAtStageBoundary As Boolean
        Private ReadOnly _resetAtPhaseBoundary As Boolean
        Private ReadOnly _resetAfterSignal As Boolean
        Private ReadOnly _sequenceGapBehavior As SpcSequenceGapBehavior
        Private ReadOnly _requestLabel As String

        Public Sub New(chartType As SpcChartType,
                       measurements As Double(,),
                       Optional variableNames As String() = Nothing,
                       Optional subgroupIds As String() = Nothing,
                       Optional labels As String() = Nothing,
                       Optional phases As SpcPhase() = Nothing,
                       Optional stageIds As String() = Nothing,
                       Optional sequenceValues As Double() = Nothing,
                       Optional sourceRowIndices As Integer() = Nothing,
                       Optional exclusionScopes As SpcExclusionScope() = Nothing,
                       Optional exclusionReasons As String() = Nothing,
                       Optional missingValuePolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject,
                       Optional modelSource As SpcMultivariateModelSource = SpcMultivariateModelSource.EstimateFromPhaseI,
                       Optional historicalMean As Double() = Nothing,
                       Optional historicalCovariance As Double(,) = Nothing,
                       Optional controlLimitAlpha As Double = 0.0027,
                       Optional useLowerHotellingLimit As Boolean = False,
                       Optional covarianceRegularization As Double = 0.0,
                       Optional allowPseudoInverse As Boolean = True,
                       Optional pcaUseCorrelationMatrix As Boolean = False,
                       Optional pcaComponentCount As Nullable(Of Integer) = Nothing,
                       Optional pcaCumulativeVariance As Double = 0.9,
                       Optional mewmaLambda As Double = 0.2,
                       Optional mewmaControlLimit As Nullable(Of Double) = Nothing,
                       Optional mcusumReferenceValue As Double = 0.5,
                       Optional mcusumDecisionInterval As Double = 5.5,
                       Optional generalizedVarianceSigmaMultiplier As Nullable(Of Double) = Nothing,
                       Optional resetAtStageBoundary As Boolean = True,
                       Optional resetAtPhaseBoundary As Boolean = True,
                       Optional resetAfterSignal As Boolean = False,
                       Optional requestLabel As String = Nothing,
                       Optional sequenceGapBehavior As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence)

            ValidateChartType(chartType)
            If measurements Is Nothing Then Throw New ArgumentNullException(NameOf(measurements))
            If measurements.GetLength(0) = 0 OrElse measurements.GetLength(1) < 2 Then
                Throw New ArgumentException(
                    "Multivariate data require at least one row and two variables.",
                    NameOf(measurements))
            End If
            If Not [Enum].IsDefined(GetType(SpcMissingValuePolicy), missingValuePolicy) Then
                Throw New ArgumentOutOfRangeException(NameOf(missingValuePolicy))
            End If
            If missingValuePolicy = SpcMissingValuePolicy.UseAvailableMeasurements Then
                Throw New ArgumentException(
                    "Multivariate charts cannot use a partially observed row without an explicit imputation model. " &
                    "Choose Reject or OmitPoint.",
                    NameOf(missingValuePolicy))
            End If
            If Not [Enum].IsDefined(GetType(SpcMultivariateModelSource), modelSource) Then
                Throw New ArgumentOutOfRangeException(NameOf(modelSource))
            End If
            If Not [Enum].IsDefined(GetType(SpcSequenceGapBehavior), sequenceGapBehavior) Then
                Throw New ArgumentOutOfRangeException(NameOf(sequenceGapBehavior))
            End If
            ValidateProbability(controlLimitAlpha, NameOf(controlLimitAlpha))
            ValidateFiniteNonnegative(covarianceRegularization,
                                      NameOf(covarianceRegularization))
            If pcaComponentCount.HasValue AndAlso pcaComponentCount.Value <= 0 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(pcaComponentCount), "The PCA component count must be positive.")
            End If
            If pcaCumulativeVariance <= 0.0 OrElse pcaCumulativeVariance > 1.0 OrElse
               Not SpcModelGuards.IsFinite(pcaCumulativeVariance) Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(pcaCumulativeVariance),
                    "PCA cumulative variance must be in the interval (0, 1].")
            End If
            If mewmaLambda <= 0.0 OrElse mewmaLambda > 1.0 OrElse
               Not SpcModelGuards.IsFinite(mewmaLambda) Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(mewmaLambda), "MEWMA lambda must be in the interval (0, 1].")
            End If
            ValidateOptionalPositive(mewmaControlLimit, NameOf(mewmaControlLimit))
            ValidateFiniteNonnegative(mcusumReferenceValue,
                                      NameOf(mcusumReferenceValue))
            ValidateFinitePositive(mcusumDecisionInterval,
                                   NameOf(mcusumDecisionInterval))
            ValidateOptionalPositive(generalizedVarianceSigmaMultiplier,
                                     NameOf(generalizedVarianceSigmaMultiplier))

            Dim rowCount As Integer = measurements.GetLength(0)
            Dim variableCount As Integer = measurements.GetLength(1)
            ValidateAlignedLength(variableNames, variableCount, NameOf(variableNames))
            ValidateAlignedLength(subgroupIds, rowCount, NameOf(subgroupIds))
            ValidateAlignedLength(labels, rowCount, NameOf(labels))
            ValidateAlignedLength(phases, rowCount, NameOf(phases))
            ValidateAlignedLength(stageIds, rowCount, NameOf(stageIds))
            ValidateAlignedLength(sequenceValues, rowCount, NameOf(sequenceValues))
            ValidateAlignedLength(sourceRowIndices, rowCount, NameOf(sourceRowIndices))
            ValidateAlignedLength(exclusionScopes, rowCount, NameOf(exclusionScopes))
            ValidateAlignedLength(exclusionReasons, rowCount, NameOf(exclusionReasons))

            If subgroupIds IsNot Nothing AndAlso
               chartType <> SpcChartType.HotellingT2 AndAlso
               chartType <> SpcChartType.GeneralizedVariance Then
                Throw New ArgumentException(
                    "Subgroup identifiers are supported only by Hotelling T-squared and generalized-variance charts.",
                    NameOf(subgroupIds))
            End If
            If chartType = SpcChartType.GeneralizedVariance AndAlso subgroupIds Is Nothing Then
                Throw New ArgumentException(
                    "A generalized-variance chart requires subgroup identifiers.",
                    NameOf(subgroupIds))
            End If

            Dim copiedMeasurements As Double(,) =
                CType(measurements.Clone(), Double(,))
            Dim completeRowCount As Integer = 0
            For i As Integer = 0 To rowCount - 1
                Dim complete As Boolean = True
                For j As Integer = 0 To variableCount - 1
                    Dim value As Double = copiedMeasurements(i, j)
                    If Double.IsInfinity(value) Then
                        Throw New ArgumentException(
                            "Measurements must not contain infinity.", NameOf(measurements))
                    End If
                    If Double.IsNaN(value) Then complete = False
                Next
                If complete Then
                    completeRowCount += 1
                ElseIf missingValuePolicy = SpcMissingValuePolicy.Reject Then
                    Throw New ArgumentException(
                        "Measurements contain a missing value and the selected policy is Reject.",
                        NameOf(measurements))
                End If
            Next
            If completeRowCount = 0 Then
                Throw New ArgumentException(
                    "At least one complete multivariate observation is required.",
                    NameOf(measurements))
            End If

            Dim copiedVariableNames(variableCount - 1) As String
            For j As Integer = 0 To variableCount - 1
                Dim value As String = If(variableNames Is Nothing,
                                         String.Empty,
                                         SpcModelGuards.NormalizeOptionalText(variableNames(j)))
                If value.Length = 0 Then
                    value = "Variable" & (j + 1).ToString(CultureInfo.InvariantCulture)
                End If
                copiedVariableNames(j) = value
            Next

            Dim copiedSubgroups As String() = Nothing
            If subgroupIds IsNot Nothing Then
                copiedSubgroups = New String(rowCount - 1) {}
            End If
            Dim copiedLabels(rowCount - 1) As String
            Dim copiedPhases(rowCount - 1) As SpcPhase
            Dim copiedStages(rowCount - 1) As String
            Dim copiedSequences(rowCount - 1) As Double
            Dim copiedSourceRows(rowCount - 1) As Integer
            Dim copiedScopes(rowCount - 1) As SpcExclusionScope
            Dim copiedReasons(rowCount - 1) As String

            For i As Integer = 0 To rowCount - 1
                copiedLabels(i) = If(labels Is Nothing,
                                     (i + 1).ToString(CultureInfo.InvariantCulture),
                                     SpcModelGuards.NormalizeOptionalText(labels(i)))
                If copiedLabels(i).Length = 0 Then
                    copiedLabels(i) = (i + 1).ToString(CultureInfo.InvariantCulture)
                End If

                copiedPhases(i) = If(phases Is Nothing, SpcPhase.PhaseI, phases(i))
                If Not [Enum].IsDefined(GetType(SpcPhase), copiedPhases(i)) Then
                    Throw New ArgumentOutOfRangeException(NameOf(phases))
                End If

                copiedStages(i) = If(stageIds Is Nothing,
                                     "Stage1",
                                     SpcModelGuards.NormalizeOptionalText(stageIds(i)))
                If copiedStages(i).Length = 0 Then
                    Throw New ArgumentException(
                        "Every row must have a nonblank stage identifier.",
                        NameOf(stageIds))
                End If

                copiedSequences(i) = If(sequenceValues Is Nothing,
                                        Double.NaN,
                                        sequenceValues(i))
                If Double.IsInfinity(copiedSequences(i)) Then
                    Throw New ArgumentException(
                        "Sequence values must not contain infinity.",
                        NameOf(sequenceValues))
                End If

                copiedSourceRows(i) = If(sourceRowIndices Is Nothing,
                                         i,
                                         sourceRowIndices(i))
                If copiedSourceRows(i) < 0 Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(sourceRowIndices),
                        "Source-row indices must be nonnegative.")
                End If

                copiedScopes(i) = If(exclusionScopes Is Nothing,
                                     SpcExclusionScope.None,
                                     exclusionScopes(i))
                SpcModelGuards.ValidateExclusionScope(copiedScopes(i),
                                                      NameOf(exclusionScopes))
                copiedReasons(i) = If(exclusionReasons Is Nothing,
                                      String.Empty,
                                      SpcModelGuards.NormalizeOptionalText(exclusionReasons(i)))

                If copiedSubgroups IsNot Nothing Then
                    copiedSubgroups(i) = SpcModelGuards.NormalizeOptionalText(subgroupIds(i))
                    If copiedSubgroups(i).Length = 0 AndAlso IsCompleteRow(copiedMeasurements, i) Then
                        Throw New ArgumentException(
                            "Every complete row in subgrouped data must have a nonblank subgroup identifier.",
                            NameOf(subgroupIds))
                    End If
                End If
            Next

            Dim copiedHistoricalMean As Double() = Nothing
            Dim copiedHistoricalCovariance As Double(,) = Nothing
            If historicalMean IsNot Nothing Then
                If historicalMean.Length <> variableCount Then
                    Throw New ArgumentException(
                        "The historical mean vector must match the number of variables.",
                        NameOf(historicalMean))
                End If
                copiedHistoricalMean = CType(historicalMean.Clone(), Double())
                For j As Integer = 0 To copiedHistoricalMean.Length - 1
                    SpcModelGuards.ValidateFinite(copiedHistoricalMean(j),
                                                  NameOf(historicalMean))
                Next
            End If
            If historicalCovariance IsNot Nothing Then
                ValidateCovarianceDimensionsAndValues(historicalCovariance,
                                                      variableCount,
                                                      NameOf(historicalCovariance))
                copiedHistoricalCovariance = SymmetrizedCopy(historicalCovariance)
                ValidatePositiveDefiniteCovariance(copiedHistoricalCovariance,
                                                   NameOf(historicalCovariance))
            End If

            If modelSource = SpcMultivariateModelSource.UseHistoricalParameters Then
                If copiedHistoricalCovariance Is Nothing Then
                    Throw New ArgumentException(
                        "Historical covariance is required by the selected model source.",
                        NameOf(historicalCovariance))
                End If
                If chartType <> SpcChartType.GeneralizedVariance AndAlso
                   copiedHistoricalMean Is Nothing Then
                    Throw New ArgumentException(
                        "A historical mean vector is required for a location-monitoring chart.",
                        NameOf(historicalMean))
                End If
            ElseIf copiedHistoricalMean IsNot Nothing OrElse
                   copiedHistoricalCovariance IsNot Nothing Then
                Throw New ArgumentException(
                    "Historical parameters were supplied while the model source is EstimateFromPhaseI.")
            End If

            _chartType = chartType
            _measurements = copiedMeasurements
            _variableNames = copiedVariableNames
            _subgroupIds = copiedSubgroups
            _labels = copiedLabels
            _phases = copiedPhases
            _stageIds = copiedStages
            _sequenceValues = copiedSequences
            _sourceRowIndices = copiedSourceRows
            _exclusionScopes = copiedScopes
            _exclusionReasons = copiedReasons
            _missingValuePolicy = missingValuePolicy
            _modelSource = modelSource
            _historicalMean = copiedHistoricalMean
            _historicalCovariance = copiedHistoricalCovariance
            _controlLimitAlpha = controlLimitAlpha
            _useLowerHotellingLimit = useLowerHotellingLimit
            _covarianceRegularization = covarianceRegularization
            _allowPseudoInverse = allowPseudoInverse
            _pcaUseCorrelationMatrix = pcaUseCorrelationMatrix
            _pcaComponentCount = pcaComponentCount
            _pcaCumulativeVariance = pcaCumulativeVariance
            _mewmaLambda = mewmaLambda
            _mewmaControlLimit = mewmaControlLimit
            _mcusumReferenceValue = mcusumReferenceValue
            _mcusumDecisionInterval = mcusumDecisionInterval
            _generalizedVarianceSigmaMultiplier = generalizedVarianceSigmaMultiplier
            _resetAtStageBoundary = resetAtStageBoundary
            _resetAtPhaseBoundary = resetAtPhaseBoundary
            _resetAfterSignal = resetAfterSignal
            _sequenceGapBehavior = sequenceGapBehavior
            _requestLabel = SpcModelGuards.NormalizeOptionalText(requestLabel)
        End Sub

        Public ReadOnly Property ChartType As SpcChartType
            Get
                Return _chartType
            End Get
        End Property

        Public ReadOnly Property Measurements As Double(,)
            Get
                Return CType(_measurements.Clone(), Double(,))
            End Get
        End Property

        Public ReadOnly Property RowCount As Integer
            Get
                Return _measurements.GetLength(0)
            End Get
        End Property

        Public ReadOnly Property VariableCount As Integer
            Get
                Return _measurements.GetLength(1)
            End Get
        End Property

        Public ReadOnly Property VariableNames As String()
            Get
                Return CType(_variableNames.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property SubgroupIds As String()
            Get
                If _subgroupIds Is Nothing Then Return Nothing
                Return CType(_subgroupIds.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property HasSubgroups As Boolean
            Get
                Return _subgroupIds IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property Labels As String()
            Get
                Return CType(_labels.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property Phases As SpcPhase()
            Get
                Return CType(_phases.Clone(), SpcPhase())
            End Get
        End Property

        Public ReadOnly Property StageIds As String()
            Get
                Return CType(_stageIds.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property SequenceValues As Double()
            Get
                Return CType(_sequenceValues.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property SourceRowIndices As Integer()
            Get
                Return CType(_sourceRowIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property ExclusionScopes As SpcExclusionScope()
            Get
                Return CType(_exclusionScopes.Clone(), SpcExclusionScope())
            End Get
        End Property

        Public ReadOnly Property ExclusionReasons As String()
            Get
                Return CType(_exclusionReasons.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property MissingValuePolicy As SpcMissingValuePolicy
            Get
                Return _missingValuePolicy
            End Get
        End Property

        Public ReadOnly Property ModelSource As SpcMultivariateModelSource
            Get
                Return _modelSource
            End Get
        End Property

        Public ReadOnly Property HistoricalMean As Double()
            Get
                If _historicalMean Is Nothing Then Return Nothing
                Return CType(_historicalMean.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property HistoricalCovariance As Double(,)
            Get
                If _historicalCovariance Is Nothing Then Return Nothing
                Return CType(_historicalCovariance.Clone(), Double(,))
            End Get
        End Property

        Public ReadOnly Property ControlLimitAlpha As Double
            Get
                Return _controlLimitAlpha
            End Get
        End Property

        Public ReadOnly Property UseLowerHotellingLimit As Boolean
            Get
                Return _useLowerHotellingLimit
            End Get
        End Property

        Public ReadOnly Property CovarianceRegularization As Double
            Get
                Return _covarianceRegularization
            End Get
        End Property

        Public ReadOnly Property AllowPseudoInverse As Boolean
            Get
                Return _allowPseudoInverse
            End Get
        End Property

        Public ReadOnly Property PcaUseCorrelationMatrix As Boolean
            Get
                Return _pcaUseCorrelationMatrix
            End Get
        End Property

        Public ReadOnly Property PcaComponentCount As Nullable(Of Integer)
            Get
                Return _pcaComponentCount
            End Get
        End Property

        Public ReadOnly Property PcaCumulativeVariance As Double
            Get
                Return _pcaCumulativeVariance
            End Get
        End Property

        Public ReadOnly Property MewmaLambda As Double
            Get
                Return _mewmaLambda
            End Get
        End Property

        Public ReadOnly Property MewmaControlLimit As Nullable(Of Double)
            Get
                Return _mewmaControlLimit
            End Get
        End Property

        Public ReadOnly Property McusumReferenceValue As Double
            Get
                Return _mcusumReferenceValue
            End Get
        End Property

        Public ReadOnly Property McusumDecisionInterval As Double
            Get
                Return _mcusumDecisionInterval
            End Get
        End Property

        Public ReadOnly Property GeneralizedVarianceSigmaMultiplier As Nullable(Of Double)
            Get
                Return _generalizedVarianceSigmaMultiplier
            End Get
        End Property

        Public ReadOnly Property ResetAtStageBoundary As Boolean
            Get
                Return _resetAtStageBoundary
            End Get
        End Property

        Public ReadOnly Property ResetAtPhaseBoundary As Boolean
            Get
                Return _resetAtPhaseBoundary
            End Get
        End Property

        Public ReadOnly Property ResetAfterSignal As Boolean
            Get
                Return _resetAfterSignal
            End Get
        End Property

        Public ReadOnly Property SequenceGapBehavior As SpcSequenceGapBehavior
            Get
                Return _sequenceGapBehavior
            End Get
        End Property

        Public ReadOnly Property RequestLabel As String
            Get
                Return _requestLabel
            End Get
        End Property

        Private Shared Sub ValidateChartType(chartType As SpcChartType)
            Select Case chartType
                Case SpcChartType.HotellingT2,
                     SpcChartType.GeneralizedVariance,
                     SpcChartType.PcaT2,
                     SpcChartType.PcaQ,
                     SpcChartType.Mewma,
                     SpcChartType.Mcusum
                    Return
                Case Else
                    Throw New ArgumentOutOfRangeException(
                        NameOf(chartType),
                        "The selected chart is not implemented by the multivariate SPC engine.")
            End Select
        End Sub

        Private Shared Sub ValidateProbability(value As Double, parameterName As String)
            If value <= 0.0 OrElse value >= 1.0 OrElse
               Not SpcModelGuards.IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(
                    parameterName, "The probability must be in the open interval (0, 1).")
            End If
        End Sub

        Private Shared Sub ValidateFinitePositive(value As Double, parameterName As String)
            If value <= 0.0 OrElse Not SpcModelGuards.IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(parameterName,
                                                      "The value must be finite and positive.")
            End If
        End Sub

        Private Shared Sub ValidateFiniteNonnegative(value As Double, parameterName As String)
            If value < 0.0 OrElse Not SpcModelGuards.IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(parameterName,
                                                      "The value must be finite and nonnegative.")
            End If
        End Sub

        Private Shared Sub ValidateOptionalPositive(value As Nullable(Of Double),
                                                    parameterName As String)
            If value.HasValue Then ValidateFinitePositive(value.Value, parameterName)
        End Sub

        Private Shared Sub ValidateAlignedLength(values As Array,
                                                 expectedLength As Integer,
                                                 parameterName As String)
            If values IsNot Nothing AndAlso values.Length <> expectedLength Then
                Throw New ArgumentException(
                    "The supplied array does not have the required aligned length.",
                    parameterName)
            End If
        End Sub

        Private Shared Function IsCompleteRow(values As Double(,), rowIndex As Integer) As Boolean
            For j As Integer = 0 To values.GetLength(1) - 1
                If Double.IsNaN(values(rowIndex, j)) Then Return False
            Next
            Return True
        End Function

        Private Shared Sub ValidateCovarianceDimensionsAndValues(values As Double(,),
                                                                variableCount As Integer,
                                                                parameterName As String)
            If values.GetLength(0) <> variableCount OrElse
               values.GetLength(1) <> variableCount Then
                Throw New ArgumentException(
                    "The historical covariance matrix must be square and match the number of variables.",
                    parameterName)
            End If
            For i As Integer = 0 To variableCount - 1
                If Not SpcModelGuards.IsFinite(values(i, i)) OrElse values(i, i) <= 0.0 Then
                    Throw New ArgumentException(
                        "Historical covariance diagonal values must be finite and positive.",
                        parameterName)
                End If
                For j As Integer = 0 To variableCount - 1
                    If Not SpcModelGuards.IsFinite(values(i, j)) Then
                        Throw New ArgumentException(
                            "Historical covariance values must be finite.", parameterName)
                    End If
                    Dim tolerance As Double = 0.00000001 *
                        Math.Max(1.0, Math.Max(Math.Abs(values(i, j)), Math.Abs(values(j, i))))
                    If Math.Abs(values(i, j) - values(j, i)) > tolerance Then
                        Throw New ArgumentException(
                            "The historical covariance matrix must be symmetric.",
                            parameterName)
                    End If
                Next
            Next
        End Sub

        Private Shared Function SymmetrizedCopy(values As Double(,)) As Double(,)
            Dim n As Integer = values.GetLength(0)
            Dim result(n - 1, n - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    result(i, j) = 0.5 * (values(i, j) + values(j, i))
                Next
            Next
            Return result
        End Function

        Private Shared Sub ValidatePositiveDefiniteCovariance(
            values As Double(,),
            parameterName As String)

            Dim errorCode As Integer = 0
            Try
                Dim ignoredInverse As Double(,) =
                    Matrix.MatInv(values, "CHOL", errorCode, False)
            Catch ex As Exception
                Throw New ArgumentException(
                    "The historical covariance matrix must be positive definite.",
                    parameterName,
                    ex)
            End Try
            If errorCode <> 0 Then
                Throw New ArgumentException(
                    "The historical covariance matrix must be positive definite.",
                    parameterName)
            End If
        End Sub
    End Class

    ''' <summary>Immutable fitted in-control model used by a multivariate chart.</summary>
    Public NotInheritable Class SpcMultivariateModelResult
        Private ReadOnly _source As SpcMultivariateModelSource
        Private ReadOnly _processMean As Double()
        Private ReadOnly _processCovariance As Double(,)
        Private ReadOnly _analysisScale As Double()
        Private ReadOnly _analysisCovariance As Double(,)
        Private ReadOnly _analysisCovarianceInverse As Double(,)
        Private ReadOnly _eigenvalues As Double()
        Private ReadOnly _eigenvectors As Double(,)
        Private ReadOnly _baselineObservationCount As Integer
        Private ReadOnly _baselineSubgroupCount As Integer
        Private ReadOnly _covarianceDegreesOfFreedom As Integer
        Private ReadOnly _effectiveDimension As Integer
        Private ReadOnly _retainedComponentCount As Integer
        Private ReadOnly _usedPseudoInverse As Boolean
        Private ReadOnly _regularization As Double

        Friend Sub New(source As SpcMultivariateModelSource,
                       processMean As Double(),
                       processCovariance As Double(,),
                       analysisScale As Double(),
                       analysisCovariance As Double(,),
                       analysisCovarianceInverse As Double(,),
                       eigenvalues As Double(),
                       eigenvectors As Double(,),
                       baselineObservationCount As Integer,
                       baselineSubgroupCount As Integer,
                       covarianceDegreesOfFreedom As Integer,
                       effectiveDimension As Integer,
                       retainedComponentCount As Integer,
                       usedPseudoInverse As Boolean,
                       regularization As Double)

            _source = source
            _processMean = SpcModelGuards.CloneVector(processMean)
            _processCovariance = SpcModelGuards.CloneMatrix(processCovariance)
            _analysisScale = SpcModelGuards.CloneVector(analysisScale)
            _analysisCovariance = SpcModelGuards.CloneMatrix(analysisCovariance)
            _analysisCovarianceInverse = SpcModelGuards.CloneMatrix(analysisCovarianceInverse)
            _eigenvalues = SpcModelGuards.CloneVector(eigenvalues)
            _eigenvectors = SpcModelGuards.CloneMatrix(eigenvectors)
            _baselineObservationCount = baselineObservationCount
            _baselineSubgroupCount = baselineSubgroupCount
            _covarianceDegreesOfFreedom = covarianceDegreesOfFreedom
            _effectiveDimension = effectiveDimension
            _retainedComponentCount = retainedComponentCount
            _usedPseudoInverse = usedPseudoInverse
            _regularization = regularization
        End Sub

        Public ReadOnly Property Source As SpcMultivariateModelSource
            Get
                Return _source
            End Get
        End Property

        Public ReadOnly Property ProcessMean As Double()
            Get
                Return SpcModelGuards.CloneVector(_processMean)
            End Get
        End Property

        Public ReadOnly Property ProcessCovariance As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_processCovariance)
            End Get
        End Property

        ''' <summary>
        ''' Gets variable scales used by correlation-PCA. All values are one for
        ''' covariance-scale analyses.
        ''' </summary>
        Public ReadOnly Property AnalysisScale As Double()
            Get
                Return SpcModelGuards.CloneVector(_analysisScale)
            End Get
        End Property

        Public ReadOnly Property AnalysisCovariance As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_analysisCovariance)
            End Get
        End Property

        Public ReadOnly Property AnalysisCovarianceInverse As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_analysisCovarianceInverse)
            End Get
        End Property

        Public ReadOnly Property Eigenvalues As Double()
            Get
                Return SpcModelGuards.CloneVector(_eigenvalues)
            End Get
        End Property

        Public ReadOnly Property Eigenvectors As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_eigenvectors)
            End Get
        End Property

        Public ReadOnly Property BaselineObservationCount As Integer
            Get
                Return _baselineObservationCount
            End Get
        End Property

        Public ReadOnly Property BaselineSubgroupCount As Integer
            Get
                Return _baselineSubgroupCount
            End Get
        End Property

        Public ReadOnly Property CovarianceDegreesOfFreedom As Integer
            Get
                Return _covarianceDegreesOfFreedom
            End Get
        End Property

        Public ReadOnly Property EffectiveDimension As Integer
            Get
                Return _effectiveDimension
            End Get
        End Property

        Public ReadOnly Property RetainedComponentCount As Integer
            Get
                Return _retainedComponentCount
            End Get
        End Property

        Public ReadOnly Property UsedPseudoInverse As Boolean
            Get
                Return _usedPseudoInverse
            End Get
        End Property

        Public ReadOnly Property Regularization As Double
            Get
                Return _regularization
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Per-point multivariate diagnostics, including component scores and
    ''' variable-level contributions.
    ''' </summary>
    Public NotInheritable Class SpcMultivariatePointDiagnostic
        Private ReadOnly _pointIndex As Integer
        Private ReadOnly _sourceRowIndices As Integer()
        Private ReadOnly _label As String
        Private ReadOnly _stageId As String
        Private ReadOnly _phase As SpcPhase
        Private ReadOnly _observationVector As Double()
        Private ReadOnly _stateVector As Double()
        Private ReadOnly _componentScores As Double()
        Private ReadOnly _residualVector As Double()
        Private ReadOnly _contributions As Double()
        Private ReadOnly _subgroupCovariance As Double(,)
        Private ReadOnly _statistic As Double
        Private ReadOnly _contributionTotal As Double
        Private ReadOnly _contributionBasis As String
        Private ReadOnly _effectiveSampleSize As Double

        Friend Sub New(pointIndex As Integer,
                       sourceRowIndices As Integer(),
                       label As String,
                       stageId As String,
                       phase As SpcPhase,
                       observationVector As Double(),
                       statistic As Double,
                       Optional stateVector As Double() = Nothing,
                       Optional componentScores As Double() = Nothing,
                       Optional residualVector As Double() = Nothing,
                       Optional contributions As Double() = Nothing,
                       Optional subgroupCovariance As Double(,) = Nothing,
                       Optional contributionBasis As String = Nothing,
                       Optional effectiveSampleSize As Double = 1.0)

            _pointIndex = pointIndex
            _sourceRowIndices = CType(sourceRowIndices.Clone(), Integer())
            _label = label
            _stageId = stageId
            _phase = phase
            _observationVector = SpcModelGuards.CloneVector(observationVector)
            _stateVector = SpcModelGuards.CloneVector(stateVector)
            _componentScores = SpcModelGuards.CloneVector(componentScores)
            _residualVector = SpcModelGuards.CloneVector(residualVector)
            _contributions = SpcModelGuards.CloneVector(contributions)
            _subgroupCovariance = SpcModelGuards.CloneMatrix(subgroupCovariance)
            _statistic = statistic
            _contributionTotal = SumFinite(contributions)
            _contributionBasis = SpcModelGuards.NormalizeOptionalText(contributionBasis)
            _effectiveSampleSize = effectiveSampleSize
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

        Public ReadOnly Property ObservationVector As Double()
            Get
                Return SpcModelGuards.CloneVector(_observationVector)
            End Get
        End Property

        Public ReadOnly Property StateVector As Double()
            Get
                Return SpcModelGuards.CloneVector(_stateVector)
            End Get
        End Property

        Public ReadOnly Property ComponentScores As Double()
            Get
                Return SpcModelGuards.CloneVector(_componentScores)
            End Get
        End Property

        Public ReadOnly Property ResidualVector As Double()
            Get
                Return SpcModelGuards.CloneVector(_residualVector)
            End Get
        End Property

        Public ReadOnly Property Contributions As Double()
            Get
                Return SpcModelGuards.CloneVector(_contributions)
            End Get
        End Property

        Public ReadOnly Property SubgroupCovariance As Double(,)
            Get
                Return SpcModelGuards.CloneMatrix(_subgroupCovariance)
            End Get
        End Property

        Public ReadOnly Property Statistic As Double
            Get
                Return _statistic
            End Get
        End Property

        Public ReadOnly Property ContributionTotal As Double
            Get
                Return _contributionTotal
            End Get
        End Property

        Public ReadOnly Property ContributionBasis As String
            Get
                Return _contributionBasis
            End Get
        End Property

        Public ReadOnly Property EffectiveSampleSize As Double
            Get
                Return _effectiveSampleSize
            End Get
        End Property

        Private Shared Function SumFinite(values As Double()) As Double
            If values Is Nothing Then Return Double.NaN
            Dim total As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                If SpcModelGuards.IsFinite(values(i)) Then total += values(i)
            Next
            Return total
        End Function
    End Class

    ''' <summary>Complete immutable result of a multivariate SPC fit.</summary>
    Public NotInheritable Class SpcMultivariateFitResult
        Private ReadOnly _request As SpcMultivariateRequest
        Private ReadOnly _model As SpcMultivariateModelResult
        Private ReadOnly _panels As SpcPanelResult()
        Private ReadOnly _diagnostics As SpcMultivariatePointDiagnostic()
        Private ReadOnly _warnings As String()
        Private ReadOnly _executionTimeMilliseconds As Double

        Friend Sub New(request As SpcMultivariateRequest,
                       model As SpcMultivariateModelResult,
                       panels As SpcPanelResult(),
                       diagnostics As SpcMultivariatePointDiagnostic(),
                       warnings As String(),
                       executionTimeMilliseconds As Double)

            _request = request
            _model = model
            _panels = CType(panels.Clone(), SpcPanelResult())
            _diagnostics = CType(diagnostics.Clone(), SpcMultivariatePointDiagnostic())
            _warnings = SpcModelGuards.CopyMessages(warnings)
            _executionTimeMilliseconds = executionTimeMilliseconds
        End Sub

        Public ReadOnly Property Request As SpcMultivariateRequest
            Get
                Return _request
            End Get
        End Property

        Public ReadOnly Property Model As SpcMultivariateModelResult
            Get
                Return _model
            End Get
        End Property

        Public ReadOnly Property Panels As SpcPanelResult()
            Get
                Return CType(_panels.Clone(), SpcPanelResult())
            End Get
        End Property

        Public ReadOnly Property Diagnostics As SpcMultivariatePointDiagnostic()
            Get
                Return CType(_diagnostics.Clone(), SpcMultivariatePointDiagnostic())
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

        Public Function GetPanel(panelType As SpcPanelType) As SpcPanelResult
            For i As Integer = 0 To _panels.Length - 1
                If _panels(i).PanelType = panelType Then Return _panels(i)
            Next
            Return Nothing
        End Function

        Public Function GetDiagnostic(pointIndex As Integer) As SpcMultivariatePointDiagnostic
            For i As Integer = 0 To _diagnostics.Length - 1
                If _diagnostics(i).PointIndex = pointIndex Then Return _diagnostics(i)
            Next
            Return Nothing
        End Function
    End Class

    ''' <summary>
    ''' Fits the common multivariate SPC charts while reusing BESHStatNG matrix,
    ''' PCA-eigen, and probability-distribution functions.
    ''' </summary>
    Public NotInheritable Class SpcMultivariate
        Private Const HotellingSignalNumber As Integer = 9018
        Private Const GeneralizedVarianceSignalNumber As Integer = 9019
        Private Const PcaT2SignalNumber As Integer = 9020
        Private Const PcaQSignalNumber As Integer = 9021
        Private Const MewmaSignalNumber As Integer = 9022
        Private Const McusumSignalNumber As Integer = 9023
        Private Const RankToleranceFactor As Double = 0.000000000001

        Private Sub New()
        End Sub

        Public Shared Function Fit(
            request As SpcMultivariateRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcMultivariateFitResult

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            Dim cancel As Func(Of Boolean) = If(cancellationRequested,
                                                Function() False)
            CheckCancellation(cancel)
            Dim timer As Stopwatch = Stopwatch.StartNew()
            Dim warnings As New List(Of String)()
            Dim diagnostics As New List(Of SpcMultivariatePointDiagnostic)()
            Dim panels As SpcPanelResult()
            Dim model As SpcMultivariateModelResult

            Select Case request.ChartType
                Case SpcChartType.HotellingT2
                    Dim observations As List(Of WorkingObservation) =
                        PrepareLocationObservations(request, cancel)
                    If request.HasSubgroups Then
                        Dim groups As List(Of WorkingGroup) = BuildGroups(observations,
                                                                         requireCovariance:=True,
                                                                         cancellationRequested:=cancel)
                        model = FitGroupedLocationModel(request, groups, warnings, cancel)
                        panels = {BuildGroupedHotellingPanel(request,
                                                            groups,
                                                            model,
                                                            diagnostics,
                                                            warnings,
                                                            cancel)}
                    Else
                        model = FitIndividualLocationModel(request,
                                                           observations,
                                                           needPca:=False,
                                                           warnings:=warnings,
                                                           cancellationRequested:=cancel)
                        panels = {BuildIndividualHotellingPanel(request,
                                                               observations,
                                                               model,
                                                               diagnostics,
                                                               cancel)}
                    End If

                Case SpcChartType.GeneralizedVariance
                    Dim observations As List(Of WorkingObservation) =
                        PrepareLocationObservations(request, cancel)
                    Dim groups As List(Of WorkingGroup) = BuildGroups(observations,
                                                                     requireCovariance:=True,
                                                                     cancellationRequested:=cancel)
                    model = FitGeneralizedVarianceModel(request,
                                                        groups,
                                                        warnings,
                                                        cancel)
                    panels = {BuildGeneralizedVariancePanel(request,
                                                            groups,
                                                            model,
                                                            diagnostics,
                                                            warnings,
                                                            cancel)}

                Case SpcChartType.PcaT2, SpcChartType.PcaQ
                    Dim observations As List(Of WorkingObservation) =
                        PrepareLocationObservations(request, cancel)
                    model = FitIndividualLocationModel(request,
                                                       observations,
                                                       needPca:=True,
                                                       warnings:=warnings,
                                                       cancellationRequested:=cancel)
                    panels = {BuildPcaPanel(request,
                                            observations,
                                            model,
                                            diagnostics,
                                            warnings,
                                            cancel)}

                Case SpcChartType.Mewma
                    Dim observations As List(Of WorkingObservation) =
                        PrepareLocationObservations(request, cancel)
                    model = FitIndividualLocationModel(request,
                                                       observations,
                                                       needPca:=False,
                                                       warnings:=warnings,
                                                       cancellationRequested:=cancel)
                    panels = {BuildMewmaPanel(request,
                                             observations,
                                             model,
                                             diagnostics,
                                             warnings,
                                             cancel)}

                Case SpcChartType.Mcusum
                    Dim observations As List(Of WorkingObservation) =
                        PrepareLocationObservations(request, cancel)
                    model = FitIndividualLocationModel(request,
                                                       observations,
                                                       needPca:=False,
                                                       warnings:=warnings,
                                                       cancellationRequested:=cancel)
                    panels = {BuildMcusumPanel(request,
                                              observations,
                                              model,
                                              diagnostics,
                                              warnings,
                                              cancel)}

                Case Else
                    Throw New NotSupportedException(
                        "The selected multivariate chart is not implemented.")
            End Select

            CheckCancellation(cancel)
            timer.Stop()
            Return New SpcMultivariateFitResult(request,
                                                model,
                                                panels,
                                                diagnostics.ToArray(),
                                                warnings.ToArray(),
                                                timer.Elapsed.TotalMilliseconds)
        End Function

#Region "Preparation and model fitting"

        Private Shared Function PrepareLocationObservations(
            request As SpcMultivariateRequest,
            cancellationRequested As Func(Of Boolean)) As List(Of WorkingObservation)

            Dim values As Double(,) = request.Measurements
            Dim labels As String() = request.Labels
            Dim phases As SpcPhase() = request.Phases
            Dim stages As String() = request.StageIds
            Dim sequences As Double() = request.SequenceValues
            Dim sourceRows As Integer() = request.SourceRowIndices
            Dim scopes As SpcExclusionScope() = request.ExclusionScopes
            Dim reasons As String() = request.ExclusionReasons
            Dim subgroupIds As String() = request.SubgroupIds
            Dim result As New List(Of WorkingObservation)()

            For i As Integer = 0 To values.GetLength(0) - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                If Not IsCompleteRow(values, i) Then Continue For
                Dim row(values.GetLength(1) - 1) As Double
                For j As Integer = 0 To row.Length - 1
                    row(j) = values(i, j)
                Next
                result.Add(New WorkingObservation With {
                    .PointIndex = i,
                    .Values = row,
                    .Label = labels(i),
                    .Phase = phases(i),
                    .StageId = stages(i),
                    .SequenceValue = ToNullableFinite(sequences(i)),
                    .SourceRowIndices = {sourceRows(i)},
                    .ExclusionScope = scopes(i),
                    .ExclusionReason = reasons(i),
                    .SubgroupId = If(subgroupIds Is Nothing, String.Empty, subgroupIds(i))
                })
            Next
            If result.Count = 0 Then
                Throw New ArgumentException("No complete observations remain after missing-value handling.")
            End If
            Return result
        End Function

        Private Shared Function BuildGroups(
            observations As List(Of WorkingObservation),
            requireCovariance As Boolean,
            cancellationRequested As Func(Of Boolean)) As List(Of WorkingGroup)

            Dim groups As New List(Of WorkingGroup)()
            Dim map As New Dictionary(Of String, WorkingGroup)(StringComparer.Ordinal)
            For i As Integer = 0 To observations.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim observation As WorkingObservation = observations(i)
                Dim group As WorkingGroup = Nothing
                Dim groupKey As String = observation.StageId & ChrW(31) &
                    observation.SubgroupId
                If Not map.TryGetValue(groupKey, group) Then
                    group = New WorkingGroup With {
                        .PointIndex = groups.Count,
                        .GroupId = observation.SubgroupId,
                        .Label = observation.SubgroupId,
                        .Phase = observation.Phase,
                        .StageId = observation.StageId,
                        .SequenceValue = observation.SequenceValue,
                        .ExclusionScope = SpcExclusionScope.None
                    }
                    map.Add(groupKey, group)
                    groups.Add(group)
                Else
                    If group.Phase <> observation.Phase Then
                        Throw New ArgumentException(
                            "All rows in a subgroup must have the same SPC phase.")
                    End If
                    If Not String.Equals(group.StageId,
                                         observation.StageId,
                                         StringComparison.OrdinalIgnoreCase) Then
                        Throw New ArgumentException(
                            "All rows in a subgroup must have the same stage identifier.")
                    End If
                End If
                group.Observations.Add(observation)
                group.ExclusionScope = group.ExclusionScope Or observation.ExclusionScope
                group.ExclusionReason = CombineReason(group.ExclusionReason,
                                                      observation.ExclusionReason)
            Next

            For i As Integer = 0 To groups.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim group As WorkingGroup = groups(i)
                If requireCovariance AndAlso group.Observations.Count < 2 Then
                    Throw New ArgumentException(
                        "Every subgroup must contain at least two complete observations.")
                End If
                Dim matrix As Double(,) = ObservationsToMatrix(group.Observations)
                group.MeanVector = ColumnMeans(matrix)
                group.Covariance = If(group.Observations.Count > 1,
                                      beshstatng.Matrix.MatCovar(matrix),
                                      Nothing)
                group.SourceRowIndices = CollectSourceRows(group.Observations)
                Dim firstLabel As String = group.Observations(0).Label
                If firstLabel.Length > 0 AndAlso
                   Not String.Equals(firstLabel,
                                     group.GroupId,
                                     StringComparison.Ordinal) Then
                    group.Label = firstLabel
                End If
            Next
            Return groups
        End Function

        Private Shared Function FitIndividualLocationModel(
            request As SpcMultivariateRequest,
            observations As List(Of WorkingObservation),
            needPca As Boolean,
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcMultivariateModelResult

            Dim baseline As New List(Of WorkingObservation)()
            For i As Integer = 0 To observations.Count - 1
                If IsEstimationEligible(observations(i)) Then baseline.Add(observations(i))
            Next

            Dim mean As Double()
            Dim covariance As Double(,)
            Dim covarianceDf As Integer
            If request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters Then
                mean = request.HistoricalMean
                covariance = request.HistoricalCovariance
                covarianceDf = 0
            Else
                If baseline.Count < 3 Then
                    Throw New ArgumentException(
                        "At least three eligible Phase-I observations are required to estimate a multivariate model.")
                End If
                Dim baselineMatrix As Double(,) = ObservationsToMatrix(baseline)
                mean = ColumnMeans(baselineMatrix)
                covariance = Matrix.MatCovar(baselineMatrix)
                covarianceDf = baseline.Count - 1
            End If

            CheckCancellation(cancellationRequested)
            Dim scale(mean.Length - 1) As Double
            Dim analysisCovariance As Double(,)
            If needPca AndAlso request.PcaUseCorrelationMatrix Then
                For j As Integer = 0 To scale.Length - 1
                    scale(j) = Math.Sqrt(Math.Max(0.0, covariance(j, j)))
                    If scale(j) <= 0.0 OrElse Not SpcModelGuards.IsFinite(scale(j)) Then
                        Throw New InvalidOperationException(
                            "Correlation-PCA requires every baseline variable to have positive variance.")
                    End If
                Next
                analysisCovariance = CovarianceToCorrelation(covariance, scale)
            Else
                For j As Integer = 0 To scale.Length - 1
                    scale(j) = 1.0
                Next
                analysisCovariance = CType(covariance.Clone(), Double(,))
            End If

            Dim regularized As Double(,) = ApplyRegularization(
                analysisCovariance, request.CovarianceRegularization)
            If request.CovarianceRegularization > 0.0 Then
                warnings.Add(
                    "A diagonal ridge equal to " &
                    request.CovarianceRegularization.ToString("0.####", CultureInfo.InvariantCulture) &
                    " times the average analysis variance was added to the covariance matrix.")
            End If

            Dim eigen As EigenInformation = ComputeEigenInformation(regularized)
            Dim inverseResult As InverseInformation = BuildInverse(regularized, request.AllowPseudoInverse, eigen.Tolerance, eigen.Rank)
            If inverseResult.UsedPseudoInverse Then
                warnings.Add(
                    "The analysis covariance matrix was singular or numerically rank deficient; " &
                    "a Moore-Penrose pseudoinverse was used and control-limit dimension was reduced to the effective rank.")
            End If

            Dim retained As Integer = 0
            If needPca Then
                retained = SelectPcaComponentCount(request, eigen.Values, eigen.Rank)
                If request.ChartType = SpcChartType.PcaQ AndAlso retained >= eigen.Rank Then
                    If eigen.Rank <= 1 Then
                        Throw New InvalidOperationException(
                            "A PCA Q chart requires at least two positive-eigenvalue dimensions.")
                    End If
                    retained = eigen.Rank - 1
                    warnings.Add(
                        "The requested PCA selection retained every nonzero component; one component was left in the residual subspace so that Q can be monitored.")
                End If
            End If

            Return New SpcMultivariateModelResult(
                request.ModelSource,
                mean,
                covariance,
                scale,
                regularized,
                inverseResult.Inverse,
                eigen.Values,
                eigen.Vectors,
                baseline.Count,
                0,
                covarianceDf,
                eigen.Rank,
                retained,
                inverseResult.UsedPseudoInverse,
                request.CovarianceRegularization)
        End Function

        Private Shared Function FitGroupedLocationModel(
            request As SpcMultivariateRequest,
            groups As List(Of WorkingGroup),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcMultivariateModelResult

            If groups.Count = 0 Then Throw New ArgumentException("No subgroups are available.")
            Dim subgroupSize As Integer = groups(0).Observations.Count
            For i As Integer = 1 To groups.Count - 1
                If groups(i).Observations.Count <> subgroupSize Then
                    Throw New ArgumentException(
                        "The implemented exact subgroup Hotelling limits require equal subgroup sizes.")
                End If
            Next

            Dim baseline As New List(Of WorkingGroup)()
            For i As Integer = 0 To groups.Count - 1
                If IsEstimationEligible(groups(i)) Then baseline.Add(groups(i))
            Next

            Dim mean As Double()
            Dim covariance As Double(,)
            Dim covarianceDf As Integer
            If request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters Then
                mean = request.HistoricalMean
                covariance = request.HistoricalCovariance
                covarianceDf = 0
            Else
                If baseline.Count < 2 Then
                    Throw New ArgumentException(
                        "At least two eligible Phase-I subgroups are required.")
                End If
                mean = MeanOfGroupMeans(baseline)
                covarianceDf = baseline.Count * (subgroupSize - 1)
                covariance = PooledWithinCovariance(baseline, covarianceDf)
            End If

            Dim regularized As Double(,) = ApplyRegularization(
                covariance, request.CovarianceRegularization)
            Dim eigen As EigenInformation = ComputeEigenInformation(regularized)
            Dim inverseResult As InverseInformation = BuildInverse(regularized, request.AllowPseudoInverse, eigen.Tolerance, eigen.Rank)
            If inverseResult.UsedPseudoInverse Then
                warnings.Add(
                    "The pooled covariance matrix required a pseudoinverse; exact Hotelling limits use its effective rank.")
            End If
            If request.CovarianceRegularization > 0.0 Then
                warnings.Add("A diagonal ridge was added to the pooled covariance matrix.")
            End If
            Dim scale(mean.Length - 1) As Double
            For j As Integer = 0 To scale.Length - 1
                scale(j) = 1.0
            Next

            CheckCancellation(cancellationRequested)
            Return New SpcMultivariateModelResult(
                request.ModelSource,
                mean,
                covariance,
                scale,
                regularized,
                inverseResult.Inverse,
                eigen.Values,
                eigen.Vectors,
                baseline.Count * subgroupSize,
                baseline.Count,
                covarianceDf,
                eigen.Rank,
                0,
                inverseResult.UsedPseudoInverse,
                request.CovarianceRegularization)
        End Function

        Private Shared Function FitGeneralizedVarianceModel(
            request As SpcMultivariateRequest,
            groups As List(Of WorkingGroup),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcMultivariateModelResult

            Dim baseline As New List(Of WorkingGroup)()
            Dim baselineObservationCount As Integer = 0
            For i As Integer = 0 To groups.Count - 1
                If IsEstimationEligible(groups(i)) Then
                    baseline.Add(groups(i))
                    baselineObservationCount += groups(i).Observations.Count
                End If
            Next

            Dim covariance As Double(,)
            Dim covarianceDf As Integer
            If request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters Then
                covariance = request.HistoricalCovariance
                covarianceDf = 0
            Else
                If baseline.Count < 2 Then
                    Throw New ArgumentException(
                        "At least two eligible Phase-I subgroups are required for a generalized-variance chart.")
                End If
                covarianceDf = 0
                For i As Integer = 0 To baseline.Count - 1
                    covarianceDf += baseline(i).Observations.Count - 1
                Next
                covariance = PooledWithinCovariance(baseline, covarianceDf)
            End If

            Dim regularized As Double(,) = ApplyRegularization(
                covariance, request.CovarianceRegularization)
            Dim eigen As EigenInformation = ComputeEigenInformation(regularized)
            If eigen.Rank < request.VariableCount Then
                Throw New InvalidOperationException(
                    "A generalized-variance chart requires a full-rank covariance matrix. " &
                    "Supply more baseline data or a positive covariance regularization value.")
            End If
            Dim inverseResult As InverseInformation = BuildInverse(regularized, allowPseudoInverse:=False, tolerance:=eigen.Tolerance, numericalRank:=eigen.Rank)
            Dim scale(request.VariableCount - 1) As Double
            For j As Integer = 0 To scale.Length - 1
                scale(j) = 1.0
            Next
            If request.CovarianceRegularization > 0.0 Then
                warnings.Add("A diagonal ridge was added before calculating generalized variance.")
            End If
            warnings.Add(
                "Generalized-variance limits use the conventional normal moment approximation for |S|; " &
                "small subgroups can have a false-signal rate different from the nominal alpha.")

            CheckCancellation(cancellationRequested)
            Return New SpcMultivariateModelResult(
                request.ModelSource,
                Nothing,
                covariance,
                scale,
                regularized,
                inverseResult.Inverse,
                eigen.Values,
                eigen.Vectors,
                baselineObservationCount,
                baseline.Count,
                covarianceDf,
                eigen.Rank,
                0,
                False,
                request.CovarianceRegularization)
        End Function

#End Region

#Region "Chart calculations"

        Private Shared Function BuildIndividualHotellingPanel(
            request As SpcMultivariateRequest,
            observations As List(Of WorkingObservation),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                SpcPanelType.HotellingT2,
                HotellingSignalNumber,
                "Hotelling T-squared control-limit signal")
            Dim mean As Double() = model.ProcessMean
            Dim inverse As Double(,) = model.AnalysisCovarianceInverse

            For i As Integer = 0 To observations.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim observation As WorkingObservation = observations(i)
                Dim difference As Double() = Subtract(observation.Values, mean)
                Dim statistic As Double = QuadraticForm(difference, inverse)
                Dim limits As LimitPair = IndividualHotellingLimits(
                    observation.Phase,
                    request.ControlLimitAlpha,
                    model.BaselineObservationCount,
                    model.EffectiveDimension,
                    request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters,
                    request.UseLowerHotellingLimit)
                Dim center As Double = HotellingCenter(
                    observation.Phase,
                    model.BaselineObservationCount,
                    model.EffectiveDimension,
                    request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters,
                    limits)
                Dim contribution As Double() = QuadraticContributions(difference, inverse)
                Dim signalled As Boolean = IsRuleEligible(observation) AndAlso
                    (statistic > limits.Upper OrElse statistic < limits.Lower)
                AddPointAndSignal(points,
                                  signals,
                                  SpcPanelType.HotellingT2,
                                  observation,
                                  statistic,
                                  center,
                                  limits,
                                  1.0,
                                  signalled,
                                  rule,
                                  HotellingSignalNumber,
                                  "Hotelling T-squared is outside its control limits.")
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    observation.PointIndex,
                    observation.SourceRowIndices,
                    observation.Label,
                    observation.StageId,
                    observation.Phase,
                    observation.Values,
                    statistic,
                    stateVector:=difference,
                    contributions:=contribution,
                    contributionBasis:="Signed variable contributions summing to T-squared"))
            Next

            Return New SpcPanelResult(
                SpcPanelType.HotellingT2,
                "Hotelling T-squared",
                points.ToArray(),
                valueAxisTitle:="T-squared",
                parameterEstimates:=BuildPanelParameters(request,
                                                         model,
                                                         SpcPanelType.HotellingT2),
                signals:=signals.ToArray())
        End Function

        Private Shared Function BuildGroupedHotellingPanel(
            request As SpcMultivariateRequest,
            groups As List(Of WorkingGroup),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                SpcPanelType.HotellingT2,
                HotellingSignalNumber,
                "Subgroup Hotelling T-squared control-limit signal")
            Dim mean As Double() = model.ProcessMean
            Dim inverse As Double(,) = model.AnalysisCovarianceInverse
            Dim subgroupSize As Integer = groups(0).Observations.Count

            For i As Integer = 0 To groups.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim group As WorkingGroup = groups(i)
                Dim difference As Double() = Subtract(group.MeanVector, mean)
                Dim statistic As Double = subgroupSize * QuadraticForm(difference, inverse)
                Dim limits As LimitPair = GroupedHotellingLimits(
                    group.Phase,
                    request.ControlLimitAlpha,
                    model.BaselineSubgroupCount,
                    subgroupSize,
                    model.EffectiveDimension,
                    request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters)
                Dim center As Double = Math.Min(CDbl(model.EffectiveDimension), limits.Upper)
                Dim contribution As Double() = QuadraticContributions(difference, inverse)
                For j As Integer = 0 To contribution.Length - 1
                    contribution(j) *= subgroupSize
                Next
                Dim signalled As Boolean = IsRuleEligible(group) AndAlso statistic > limits.Upper
                AddGroupPointAndSignal(points,
                                       signals,
                                       SpcPanelType.HotellingT2,
                                       group,
                                       statistic,
                                       center,
                                       limits,
                                       signalled,
                                       rule,
                                       HotellingSignalNumber,
                                       "Subgroup Hotelling T-squared exceeds its upper control limit.")
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    group.PointIndex,
                    group.SourceRowIndices,
                    group.Label,
                    group.StageId,
                    group.Phase,
                    group.MeanVector,
                    statistic,
                    stateVector:=difference,
                    contributions:=contribution,
                    subgroupCovariance:=group.Covariance,
                    contributionBasis:="Signed variable contributions summing to subgroup T-squared",
                    effectiveSampleSize:=subgroupSize))
            Next

            If request.UseLowerHotellingLimit Then
                warnings.Add(
                    "The lower Hotelling limit option does not apply to subgroup-mean charts and was ignored.")
            End If
            Return New SpcPanelResult(
                SpcPanelType.HotellingT2,
                "Hotelling T-squared (subgroups)",
                points.ToArray(),
                valueAxisTitle:="T-squared",
                parameterEstimates:=BuildPanelParameters(request,
                                                         model,
                                                         SpcPanelType.HotellingT2),
                signals:=signals.ToArray())
        End Function

        Private Shared Function BuildGeneralizedVariancePanel(
            request As SpcMultivariateRequest,
            groups As List(Of WorkingGroup),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                SpcPanelType.GeneralizedVariance,
                GeneralizedVarianceSignalNumber,
                "Generalized-variance control-limit signal")
            Dim processDeterminant As Double = PositiveDeterminant(model.AnalysisCovariance)
            Dim determinantRidge As Double = request.CovarianceRegularization *
                AverageDiagonal(model.ProcessCovariance)
            Dim sigmaMultiplier As Double = If(
                request.GeneralizedVarianceSigmaMultiplier.HasValue,
                request.GeneralizedVarianceSigmaMultiplier.Value,
                distributions.NormSInv(1.0 - request.ControlLimitAlpha / 2.0))

            For i As Integer = 0 To groups.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim group As WorkingGroup = groups(i)
                Dim subgroupSize As Integer = group.Observations.Count
                If subgroupSize <= request.VariableCount Then
                    Throw New ArgumentException(
                        "Each generalized-variance subgroup must contain more observations than variables.")
                End If
                Dim subgroupCovarianceForStatistic As Double(,) =
                    AddDiagonal(group.Covariance, determinantRidge)
                Dim statistic As Double = Math.Max(0.0, Matrix.MDeterm(
                    subgroupCovarianceForStatistic))
                Dim moments As DeterminantMoments = GeneralizedVarianceMoments(
                    subgroupSize, request.VariableCount)
                Dim center As Double = moments.B1 * processDeterminant
                Dim standardDeviation As Double = Math.Sqrt(moments.B2) * processDeterminant
                Dim limits As New LimitPair With {
                    .Lower = Math.Max(0.0, center - sigmaMultiplier * standardDeviation),
                    .Upper = center + sigmaMultiplier * standardDeviation
                }
                Dim signalled As Boolean = IsRuleEligible(group) AndAlso
                    (statistic < limits.Lower OrElse statistic > limits.Upper)
                AddGroupPointAndSignal(points,
                                       signals,
                                       SpcPanelType.GeneralizedVariance,
                                       group,
                                       statistic,
                                       center,
                                       limits,
                                       signalled,
                                       rule,
                                       GeneralizedVarianceSignalNumber,
                                       "The subgroup generalized variance is outside its control limits.")
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    group.PointIndex,
                    group.SourceRowIndices,
                    group.Label,
                    group.StageId,
                    group.Phase,
                    group.MeanVector,
                    statistic,
                    subgroupCovariance:=group.Covariance,
                    contributionBasis:="No order-invariant variable decomposition is reported for |S|",
                    effectiveSampleSize:=subgroupSize))
            Next

            warnings.Add(
                "Generalized variance depends on measurement units; rescaling any variable changes |S|.")
            Return New SpcPanelResult(
                SpcPanelType.GeneralizedVariance,
                "Generalized variance",
                points.ToArray(),
                valueAxisTitle:="|S|",
                parameterEstimates:=BuildPanelParameters(request,
                                                         model,
                                                         SpcPanelType.GeneralizedVariance,
                                                         sigmaMultiplier),
                signals:=signals.ToArray(),
                warnings:=warnings.ToArray())
        End Function

        Private Shared Function BuildPcaPanel(
            request As SpcMultivariateRequest,
            observations As List(Of WorkingObservation),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim panelType As SpcPanelType = If(request.ChartType = SpcChartType.PcaT2,
                                               SpcPanelType.PcaT2,
                                               SpcPanelType.PcaQ)
            Dim signalNumber As Integer = If(panelType = SpcPanelType.PcaT2,
                                             PcaT2SignalNumber,
                                             PcaQSignalNumber)
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                panelType,
                signalNumber,
                If(panelType = SpcPanelType.PcaT2,
                   "PCA T-squared control-limit signal",
                   "PCA Q control-limit signal"))
            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim mean As Double() = model.ProcessMean
            Dim scale As Double() = model.AnalysisScale
            Dim eigenvalues As Double() = model.Eigenvalues
            Dim eigenvectors As Double(,) = model.Eigenvectors
            Dim retained As Integer = model.RetainedComponentCount
            Dim qLimit As Double = Double.NaN
            Dim qCenter As Double = Double.NaN
            If panelType = SpcPanelType.PcaQ Then
                Dim qInfo As QLimitInformation = CalculateJacksonMudholkarLimit(
                    eigenvalues,
                    retained,
                    request.ControlLimitAlpha)
                qLimit = qInfo.UpperLimit
                qCenter = qInfo.ExpectedValue
                If qInfo.UsedFallback Then
                    warnings.Add(
                        "Jackson-Mudholkar Q-limit moments were numerically degenerate; an empirical Phase-I quantile was used.")
                End If
            End If

            Dim baselineQ As New List(Of Double)()
            If panelType = SpcPanelType.PcaQ AndAlso Not SpcModelGuards.IsFinite(qLimit) Then
                For i As Integer = 0 To observations.Count - 1
                    If IsEstimationEligible(observations(i)) Then
                        Dim temporary As PcaProjection = ProjectPca(
                            observations(i).Values, mean, scale, eigenvectors, retained)
                        baselineQ.Add(SumSquares(temporary.Residual))
                    End If
                Next
                qLimit = EmpiricalQuantile(baselineQ.ToArray(),
                                           1.0 - request.ControlLimitAlpha)
                qCenter = Average(baselineQ.ToArray())
            End If

            For i As Integer = 0 To observations.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim observation As WorkingObservation = observations(i)
                Dim projection As PcaProjection = ProjectPca(
                    observation.Values, mean, scale, eigenvectors, retained)
                Dim statistic As Double
                Dim center As Double
                Dim limits As LimitPair
                Dim contributions As Double()
                Dim contributionBasis As String

                If panelType = SpcPanelType.PcaT2 Then
                    statistic = 0.0
                    For k As Integer = 0 To retained - 1
                        statistic += projection.Scores(k) * projection.Scores(k) /
                                     eigenvalues(k)
                    Next
                    limits = IndividualHotellingLimits(
                        observation.Phase,
                        request.ControlLimitAlpha,
                        model.BaselineObservationCount,
                        retained,
                        request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters,
                        request.UseLowerHotellingLimit)
                    center = HotellingCenter(
                        observation.Phase,
                        model.BaselineObservationCount,
                        retained,
                        request.ModelSource = SpcMultivariateModelSource.UseHistoricalParameters,
                        limits)
                    contributions = PcaT2Contributions(
                        projection.WorkingDifference,
                        eigenvalues,
                        eigenvectors,
                        retained)
                    contributionBasis = "Signed variable contributions summing to PCA T-squared"
                Else
                    statistic = SumSquares(projection.Residual)
                    center = Math.Min(qCenter, qLimit)
                    limits = New LimitPair With {.Lower = 0.0, .Upper = qLimit}
                    contributions = SquaredElements(projection.Residual)
                    contributionBasis = "Squared residual-variable contributions summing to Q"
                End If

                Dim signalled As Boolean = IsRuleEligible(observation) AndAlso
                    (statistic < limits.Lower OrElse statistic > limits.Upper)
                AddPointAndSignal(points,
                                  signals,
                                  panelType,
                                  observation,
                                  statistic,
                                  center,
                                  limits,
                                  1.0,
                                  signalled,
                                  rule,
                                  signalNumber,
                                  If(panelType = SpcPanelType.PcaT2,
                                     "PCA T-squared is outside its control limits.",
                                     "PCA Q residual statistic exceeds its upper control limit."))
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    observation.PointIndex,
                    observation.SourceRowIndices,
                    observation.Label,
                    observation.StageId,
                    observation.Phase,
                    observation.Values,
                    statistic,
                    stateVector:=projection.WorkingDifference,
                    componentScores:=projection.Scores,
                    residualVector:=projection.Residual,
                    contributions:=contributions,
                    contributionBasis:=contributionBasis))
            Next

            Return New SpcPanelResult(
                panelType,
                If(panelType = SpcPanelType.PcaT2, "PCA T-squared", "PCA Q residual"),
                points.ToArray(),
                valueAxisTitle:=If(panelType = SpcPanelType.PcaT2, "T-squared", "Q"),
                parameterEstimates:=BuildPanelParameters(request, model, panelType),
                signals:=signals.ToArray())
        End Function

        Private Shared Function BuildMewmaPanel(
            request As SpcMultivariateRequest,
            observations As List(Of WorkingObservation),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                SpcPanelType.Mewma,
                MewmaSignalNumber,
                "MEWMA control-limit signal")
            Dim mean As Double() = model.ProcessMean
            Dim inverse As Double(,) = model.AnalysisCovarianceInverse
            Dim state(mean.Length - 1) As Double
            Dim runLength As Integer = 0
            Dim previousStage As String = Nothing
            Dim previousPhase As Nullable(Of SpcPhase) = Nothing
            Dim previousPointIndex As Integer = -1
            Dim upper As Double = If(request.MewmaControlLimit.HasValue,
                                     request.MewmaControlLimit.Value,
                                     distributions.ChiSquareInv(1.0 - request.ControlLimitAlpha,
                                                                model.EffectiveDimension))
            If Not request.MewmaControlLimit.HasValue Then
                warnings.Add(
                    "The MEWMA UCL uses a chi-square pointwise approximation. " &
                    "For production monitoring, supply an ARL-calibrated MEWMA control limit.")
            End If

            For i As Integer = 0 To observations.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim observation As WorkingObservation = observations(i)
                Dim hasOmittedPointGap As Boolean = previousPointIndex >= 0 AndAlso observation.PointIndex <> previousPointIndex + 1
                If ShouldResetState(request,
                                    previousStage,
                                    previousPhase,
                                    observation.StageId,
                                    observation.Phase) OrElse
                                    (hasOmittedPointGap AndAlso
                                    request.SequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence) Then
                    Array.Clear(state, 0, state.Length)
                    runLength = 0
                End If
                previousStage = observation.StageId
                previousPhase = observation.Phase
                previousPointIndex = observation.PointIndex
                Dim priorState As Double() = CType(state.Clone(), Double())
                Dim priorRunLength As Integer = runLength
                runLength += 1

                Dim difference As Double() = Subtract(observation.Values, mean)
                For j As Integer = 0 To state.Length - 1
                    state(j) = request.MewmaLambda * difference(j) +
                               (1.0 - request.MewmaLambda) * state(j)
                Next
                Dim varianceFactor As Double = request.MewmaLambda /
                    (2.0 - request.MewmaLambda) *
                    (1.0 - Math.Pow(1.0 - request.MewmaLambda, 2.0 * runLength))
                Dim statistic As Double = QuadraticForm(state, inverse) / varianceFactor
                Dim contributions As Double() = QuadraticContributions(state, inverse)
                For j As Integer = 0 To contributions.Length - 1
                    contributions(j) /= varianceFactor
                Next
                Dim limits As New LimitPair With {.Lower = 0.0, .Upper = upper}
                Dim signalled As Boolean = IsRuleEligible(observation) AndAlso statistic > upper
                AddPointAndSignal(points,
                                  signals,
                                  SpcPanelType.Mewma,
                                  observation,
                                  statistic,
                                  Math.Min(CDbl(model.EffectiveDimension), upper),
                                  limits,
                                  1.0,
                                  signalled,
                                  rule,
                                  MewmaSignalNumber,
                                  "The MEWMA statistic exceeds its upper control limit.")
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    observation.PointIndex,
                    observation.SourceRowIndices,
                    observation.Label,
                    observation.StageId,
                    observation.Phase,
                    observation.Values,
                    statistic,
                    stateVector:=CType(state.Clone(), Double()),
                    contributions:=contributions,
                    contributionBasis:="Signed variable contributions summing to the MEWMA T-squared statistic"))
                If IsRuleEligible(observation) Then
                    If signalled AndAlso request.ResetAfterSignal Then
                        Array.Clear(state, 0, state.Length)
                        runLength = 0
                    End If
                ElseIf request.SequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    Array.Clear(state, 0, state.Length)
                    runLength = 0
                Else
                    'Display the excluded point's provisional statistic, but do
                    'not allow it to alter the recursion carried forward.
                    state = priorState
                    runLength = priorRunLength
                End If
            Next

            Return New SpcPanelResult(
                SpcPanelType.Mewma,
                "Multivariate EWMA",
                points.ToArray(),
                valueAxisTitle:="MEWMA T-squared",
                parameterEstimates:=BuildPanelParameters(request,
                                                         model,
                                                         SpcPanelType.Mewma,
                                                         upper),
                signals:=signals.ToArray())
        End Function

        Private Shared Function BuildMcusumPanel(
            request As SpcMultivariateRequest,
            observations As List(Of WorkingObservation),
            model As SpcMultivariateModelResult,
            diagnostics As List(Of SpcMultivariatePointDiagnostic),
            warnings As List(Of String),
            cancellationRequested As Func(Of Boolean)) As SpcPanelResult

            Dim points As New List(Of SpcPointResult)()
            Dim signals As New List(Of SpcSignalResult)()
            Dim rule As SpcRuleDefinition = CreateIntrinsicRule(
                SpcPanelType.Mcusum,
                McusumSignalNumber,
                "MCUSUM decision-interval signal")
            Dim mean As Double() = model.ProcessMean
            Dim inverse As Double(,) = model.AnalysisCovarianceInverse
            Dim state(mean.Length - 1) As Double
            Dim previousStage As String = Nothing
            Dim previousPhase As Nullable(Of SpcPhase) = Nothing
            Dim previousPointIndex As Integer = -1

            warnings.Add(
                "MCUSUM k and h are design parameters whose in-control ARL depends on dimension and covariance estimation; validate the selected design for the intended process.")

            For i As Integer = 0 To observations.Count - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim observation As WorkingObservation = observations(i)
                Dim hasOmittedPointGap As Boolean = previousPointIndex >= 0 AndAlso observation.PointIndex <> previousPointIndex + 1
                If ShouldResetState(request,
                                    previousStage,
                                    previousPhase,
                                    observation.StageId,
                                    observation.Phase) OrElse
                                    (hasOmittedPointGap AndAlso request.SequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence) Then
                    Array.Clear(state, 0, state.Length)
                End If
                previousStage = observation.StageId
                previousPhase = observation.Phase
                previousPointIndex = observation.PointIndex
                Dim priorState As Double() = CType(state.Clone(), Double())

                Dim difference As Double() = Subtract(observation.Values, mean)
                Dim candidate(state.Length - 1) As Double
                For j As Integer = 0 To candidate.Length - 1
                    candidate(j) = state(j) + difference(j)
                Next
                Dim candidateNorm As Double = Math.Sqrt(
                    Math.Max(0.0, QuadraticForm(candidate, inverse)))
                If candidateNorm <= request.McusumReferenceValue OrElse candidateNorm = 0.0 Then
                    Array.Clear(state, 0, state.Length)
                Else
                    Dim shrink As Double = 1.0 -
                        request.McusumReferenceValue / candidateNorm
                    For j As Integer = 0 To state.Length - 1
                        state(j) = shrink * candidate(j)
                    Next
                End If

                Dim squaredStatistic As Double = Math.Max(0.0,
                    QuadraticForm(state, inverse))
                Dim statistic As Double = Math.Sqrt(squaredStatistic)
                Dim contributions As Double() = QuadraticContributions(state, inverse)
                Dim limits As New LimitPair With {
                    .Lower = 0.0,
                    .Upper = request.McusumDecisionInterval
                }
                Dim signalled As Boolean = IsRuleEligible(observation) AndAlso
                    statistic > request.McusumDecisionInterval
                AddPointAndSignal(points,
                                  signals,
                                  SpcPanelType.Mcusum,
                                  observation,
                                  statistic,
                                  0.0,
                                  limits,
                                  1.0,
                                  signalled,
                                  rule,
                                  McusumSignalNumber,
                                  "The MCUSUM norm exceeds decision interval h.")
                diagnostics.Add(New SpcMultivariatePointDiagnostic(
                    observation.PointIndex,
                    observation.SourceRowIndices,
                    observation.Label,
                    observation.StageId,
                    observation.Phase,
                    observation.Values,
                    statistic,
                    stateVector:=CType(state.Clone(), Double()),
                    contributions:=contributions,
                    contributionBasis:="Signed variable contributions summing to the squared MCUSUM norm"))
                If IsRuleEligible(observation) Then
                    If signalled AndAlso request.ResetAfterSignal Then
                        Array.Clear(state, 0, state.Length)
                    End If
                ElseIf request.SequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence Then
                    Array.Clear(state, 0, state.Length)
                Else
                    'Display the excluded point's provisional statistic, but do
                    'not allow it to alter the state carried forward.
                    state = priorState
                End If
            Next

            Return New SpcPanelResult(
                SpcPanelType.Mcusum,
                "Multivariate CUSUM (Crosier)",
                points.ToArray(),
                valueAxisTitle:="MCUSUM norm",
                parameterEstimates:=BuildPanelParameters(request,
                                                         model,
                                                         SpcPanelType.Mcusum,
                                                         request.McusumDecisionInterval),
                signals:=signals.ToArray())
        End Function

#End Region

#Region "Limits and numerical helpers"

        Private Shared Function IndividualHotellingLimits(
            phase As SpcPhase,
            alpha As Double,
            baselineCount As Integer,
            dimension As Integer,
            historical As Boolean,
            useLowerLimit As Boolean) As LimitPair

            ' The request alpha is the two-sided false-signal probability used by
            ' the Tracy-Young-Mason/NIST limits. Hiding the lower line does not
            ' relax the upper limit; it only suppresses lower-tail signalling.
            Dim tail As Double = alpha / 2.0
            If historical Then
                Return New LimitPair With {
                    .Lower = If(useLowerLimit,
                                distributions.ChiSquareInv(tail, dimension),
                                0.0),
                    .Upper = distributions.ChiSquareInv(1.0 - tail, dimension)
                }
            End If
            If baselineCount <= dimension + 1 Then
                Throw New ArgumentException(
                    "Exact Phase-I Hotelling limits require the baseline count to exceed effective dimension plus one.")
            End If

            If phase = SpcPhase.PhaseI Then
                Dim shapeA As Double = dimension / 2.0
                Dim shapeB As Double = (baselineCount - dimension - 1) / 2.0
                Dim multiplier As Double = CDbl((baselineCount - 1) * (baselineCount - 1)) /
                    baselineCount
                Return New LimitPair With {
                    .Lower = If(useLowerLimit,
                                multiplier * distributions.InverseRegularizedIncompleteBeta(
                                    tail, shapeA, shapeB),
                                0.0),
                    .Upper = multiplier * distributions.InverseRegularizedIncompleteBeta(
                        1.0 - tail, shapeA, shapeB)
                }
            End If

            Dim factor As Double = CDbl(dimension * (baselineCount + 1) * (baselineCount - 1)) /
                (baselineCount * (baselineCount - dimension))
            Return New LimitPair With {
                .Lower = If(useLowerLimit,
                            factor * distributions.F_Inv(tail,
                                                         dimension,
                                                         baselineCount - dimension),
                            0.0),
                .Upper = factor * distributions.F_Inv_RT(tail,
                                                         dimension,
                                                         baselineCount - dimension)
            }
        End Function

        Private Shared Function GroupedHotellingLimits(
            phase As SpcPhase,
            alpha As Double,
            baselineGroupCount As Integer,
            subgroupSize As Integer,
            dimension As Integer,
            historical As Boolean) As LimitPair

            If historical Then
                Return New LimitPair With {
                    .Lower = 0.0,
                    .Upper = distributions.ChiSquareInv(1.0 - alpha, dimension)
                }
            End If
            If baselineGroupCount < 2 Then
                Throw New ArgumentException(
                    "At least two Phase-I subgroups are required for estimated Hotelling limits.")
            End If

            Dim denominatorDf As Integer = baselineGroupCount * subgroupSize -
                baselineGroupCount - dimension + 1
            If denominatorDf <= 0 Then
                Throw New ArgumentException(
                    "There are insufficient within-subgroup degrees of freedom for Hotelling limits.")
            End If

            Dim factor As Double
            If phase = SpcPhase.PhaseI Then
                Dim numerator As Double =
                    baselineGroupCount * subgroupSize * dimension -
                    baselineGroupCount * subgroupSize -
                    subgroupSize * dimension + dimension
                factor = numerator / denominatorDf
            Else
                factor = CDbl(dimension * (baselineGroupCount + 1) * (subgroupSize - 1)) /
                         denominatorDf
            End If
            Return New LimitPair With {
                .Lower = 0.0,
                .Upper = factor * distributions.F_Inv_RT(
                    alpha, dimension, denominatorDf)
            }
        End Function

        Private Shared Function HotellingCenter(
            phase As SpcPhase,
            baselineCount As Integer,
            dimension As Integer,
            historical As Boolean,
            limits As LimitPair) As Double

            Dim center As Double = dimension
            If Not historical Then
                If phase = SpcPhase.PhaseI Then
                    center = CDbl(dimension * (baselineCount - 1)) / baselineCount
                ElseIf baselineCount > dimension + 2 Then
                    center = CDbl(dimension * (baselineCount + 1) * (baselineCount - 1)) /
                             (baselineCount * (baselineCount - dimension - 2))
                End If
            End If
            Return Math.Max(limits.Lower, Math.Min(center, limits.Upper))
        End Function

        Private Shared Function GeneralizedVarianceMoments(
            subgroupSize As Integer,
            dimension As Integer) As DeterminantMoments

            Dim degrees As Integer = subgroupSize - 1
            Dim firstProduct As Double = 1.0
            Dim secondProduct As Double = 1.0
            For i As Integer = 1 To dimension
                Dim first As Double = subgroupSize - i
                If first <= 0.0 Then
                    Throw New ArgumentException(
                        "The subgroup size must exceed the number of variables.")
                End If
                firstProduct *= first
                secondProduct *= subgroupSize - i + 2.0
            Next
            Dim denominator As Double = Math.Pow(degrees, dimension)
            Dim b1 As Double = firstProduct / denominator
            Dim b2 As Double = firstProduct * (secondProduct - firstProduct) /
                (denominator * denominator)
            Return New DeterminantMoments With {
                .B1 = b1,
                .B2 = Math.Max(0.0, b2)
            }
        End Function

        Private Shared Function CalculateJacksonMudholkarLimit(
            eigenvalues As Double(),
            retained As Integer,
            alpha As Double) As QLimitInformation

            Dim theta1 As Double = 0.0
            Dim theta2 As Double = 0.0
            Dim theta3 As Double = 0.0
            For j As Integer = retained To eigenvalues.Length - 1
                Dim value As Double = Math.Max(0.0, eigenvalues(j))
                theta1 += value
                theta2 += value * value
                theta3 += value * value * value
            Next
            If theta1 <= 0.0 OrElse theta2 <= 0.0 Then
                Return New QLimitInformation With {
                    .ExpectedValue = theta1,
                    .UpperLimit = Double.NaN,
                    .UsedFallback = True
                }
            End If
            Dim h0 As Double = 1.0 - 2.0 * theta1 * theta3 /
                (3.0 * theta2 * theta2)
            If h0 <= 0.0 Then
                Return New QLimitInformation With {
                    .ExpectedValue = theta1,
                    .UpperLimit = Double.NaN,
                    .UsedFallback = True
                }
            End If
            Dim z As Double = distributions.NormSInv(1.0 - alpha)
            Dim bracket As Double = 1.0 +
                z * Math.Sqrt(2.0 * theta2 * h0 * h0) / theta1 +
                theta2 * h0 * (h0 - 1.0) / (theta1 * theta1)
            If bracket <= 0.0 OrElse Not SpcModelGuards.IsFinite(bracket) Then
                Return New QLimitInformation With {
                    .ExpectedValue = theta1,
                    .UpperLimit = Double.NaN,
                    .UsedFallback = True
                }
            End If
            Return New QLimitInformation With {
                .ExpectedValue = theta1,
                .UpperLimit = theta1 * Math.Pow(bracket, 1.0 / h0),
                .UsedFallback = False
            }
        End Function

        Private Shared Function ComputeEigenInformation(covariance As Double(,)) As EigenInformation
            Dim raw = Matrix.EIGEN_JK(covariance, 100, 0.000000000001)
            Dim sorted = Global.BESHStatNG.Multivariate.MultivariateShared.SortEigenpairsDescending(
                raw.Item1, raw.Item2)
            Dim values As Double() = sorted.Item1
            Dim vectors As Double(,) = sorted.Item2
            Dim maximum As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                If values(i) > maximum Then maximum = values(i)
            Next
            Dim tolerance As Double = Math.Max(1.0, maximum) *
                Math.Max(covariance.GetLength(0), covariance.GetLength(1)) *
                RankToleranceFactor
            Dim rank As Integer = 0
            For i As Integer = 0 To values.Length - 1
                If values(i) > tolerance Then
                    rank += 1
                ElseIf values(i) < -tolerance Then
                    Throw New InvalidOperationException(
                        "The fitted covariance matrix is not positive semidefinite.")
                End If
            Next
            If rank = 0 Then
                Throw New InvalidOperationException(
                    "The fitted covariance matrix has zero numerical rank.")
            End If
            Return New EigenInformation With {
                .Values = values,
                .Vectors = vectors,
                .Rank = rank,
                .Tolerance = tolerance
            }
        End Function

        Private Shared Function BuildInverse(covariance As Double(,),
                                             allowPseudoInverse As Boolean,
                                             tolerance As Double,
                                             numericalRank As Integer) As InverseInformation

            If numericalRank < covariance.GetLength(0) Then
                If Not allowPseudoInverse Then
                    Throw New InvalidOperationException(
                        "The covariance matrix is singular or numerically rank deficient.")
                End If
                Return New InverseInformation With {
                    .Inverse = Matrix.pseudoInverse(covariance, tolerance),
                    .UsedPseudoInverse = True
                }
            End If

            Dim errorCode As Integer = 0
            Try
                Dim inverse As Double(,) = Matrix.MatInv(
                    covariance,
                    "CHOL",
                    errorCode,
                    False)
                If errorCode = 0 Then
                    Return New InverseInformation With {
                        .Inverse = inverse,
                        .UsedPseudoInverse = False
                    }
                End If
            Catch
                If Not allowPseudoInverse Then Throw
            End Try
            If Not allowPseudoInverse Then
                Throw New InvalidOperationException(
                    "The covariance matrix is singular or not positive definite.")
            End If
            Return New InverseInformation With {
                .Inverse = Matrix.pseudoInverse(covariance, tolerance),
                .UsedPseudoInverse = True
            }
        End Function

        Private Shared Function ApplyRegularization(
            covariance As Double(,),
            regularization As Double) As Double(,)

            Dim result As Double(,) = CType(covariance.Clone(), Double(,))
            If regularization = 0.0 Then Return result
            Dim averageVariance As Double = 0.0
            For i As Integer = 0 To result.GetLength(0) - 1
                averageVariance += result(i, i)
            Next
            averageVariance /= result.GetLength(0)
            If averageVariance <= 0.0 Then
                Throw New InvalidOperationException(
                    "Covariance regularization requires positive average variance.")
            End If
            Dim ridge As Double = regularization * averageVariance
            For i As Integer = 0 To result.GetLength(0) - 1
                result(i, i) += ridge
            Next
            Return result
        End Function

        Private Shared Function AddDiagonal(values As Double(,), amount As Double) As Double(,)
            Dim result As Double(,) = CType(values.Clone(), Double(,))
            If amount <> 0.0 Then
                For i As Integer = 0 To result.GetLength(0) - 1
                    result(i, i) += amount
                Next
            End If
            Return result
        End Function

        Private Shared Function AverageDiagonal(values As Double(,)) As Double
            Dim result As Double = 0.0
            For i As Integer = 0 To values.GetLength(0) - 1
                result += values(i, i)
            Next
            Return result / values.GetLength(0)
        End Function

        Private Shared Function SelectPcaComponentCount(
            request As SpcMultivariateRequest,
            eigenvalues As Double(),
            rank As Integer) As Integer

            If request.PcaComponentCount.HasValue Then
                If request.PcaComponentCount.Value > rank Then
                    Throw New ArgumentOutOfRangeException(
                        "PcaComponentCount",
                        "The PCA component count must not exceed the covariance rank.")
                End If
                Return request.PcaComponentCount.Value
            End If
            Dim total As Double = 0.0
            For i As Integer = 0 To rank - 1
                total += Math.Max(0.0, eigenvalues(i))
            Next
            Dim cumulative As Double = 0.0
            For i As Integer = 0 To rank - 1
                cumulative += Math.Max(0.0, eigenvalues(i))
                If cumulative / total >= request.PcaCumulativeVariance Then Return i + 1
            Next
            Return rank
        End Function

        Private Shared Function CovarianceToCorrelation(
            covariance As Double(,),
            scale As Double()) As Double(,)

            Dim result(covariance.GetLength(0) - 1,
                       covariance.GetLength(1) - 1) As Double
            For i As Integer = 0 To result.GetLength(0) - 1
                For j As Integer = 0 To result.GetLength(1) - 1
                    result(i, j) = covariance(i, j) / (scale(i) * scale(j))
                Next
            Next
            Return result
        End Function

        Private Shared Function ProjectPca(
            observation As Double(),
            mean As Double(),
            scale As Double(),
            eigenvectors As Double(,),
            retained As Integer) As PcaProjection

            Dim difference(observation.Length - 1) As Double
            For j As Integer = 0 To difference.Length - 1
                difference(j) = (observation(j) - mean(j)) / scale(j)
            Next
            Dim scores(retained - 1) As Double
            For k As Integer = 0 To retained - 1
                For j As Integer = 0 To difference.Length - 1
                    scores(k) += difference(j) * eigenvectors(j, k)
                Next
            Next
            Dim residual As Double() = CType(difference.Clone(), Double())
            For j As Integer = 0 To residual.Length - 1
                For k As Integer = 0 To retained - 1
                    residual(j) -= scores(k) * eigenvectors(j, k)
                Next
            Next
            Return New PcaProjection With {
                .WorkingDifference = difference,
                .Scores = scores,
                .Residual = residual
            }
        End Function

        Private Shared Function PcaT2Contributions(
            difference As Double(),
            eigenvalues As Double(),
            eigenvectors As Double(,),
            retained As Integer) As Double()

            Dim weighted(difference.Length - 1) As Double
            For k As Integer = 0 To retained - 1
                Dim score As Double = 0.0
                For j As Integer = 0 To difference.Length - 1
                    score += difference(j) * eigenvectors(j, k)
                Next
                For j As Integer = 0 To difference.Length - 1
                    weighted(j) += eigenvectors(j, k) * score / eigenvalues(k)
                Next
            Next
            Dim result(difference.Length - 1) As Double
            For j As Integer = 0 To result.Length - 1
                result(j) = difference(j) * weighted(j)
            Next
            Return result
        End Function

#End Region

#Region "Result construction helpers"

        Private Shared Sub AddPointAndSignal(
            points As List(Of SpcPointResult),
            signals As List(Of SpcSignalResult),
            panelType As SpcPanelType,
            observation As WorkingObservation,
            statistic As Double,
            center As Double,
            limits As LimitPair,
            effectiveSampleSize As Double,
            signalled As Boolean,
            rule As SpcRuleDefinition,
            signalNumber As Integer,
            message As String)

            Dim signalNumbers As Integer() = If(signalled,
                                                New Integer() {signalNumber},
                                                Nothing)
            points.Add(New SpcPointResult(
                observation.PointIndex,
                statistic,
                center,
                limits.Lower,
                limits.Upper,
                label:=observation.Label,
                stageId:=observation.StageId,
                phase:=observation.Phase,
                sequenceValue:=observation.SequenceValue,
                effectiveSampleSize:=effectiveSampleSize,
                sourceRowIndices:=observation.SourceRowIndices,
                includedInParameterEstimation:=IsEstimationEligible(observation),
                includedInRuleEvaluation:=IsRuleEligible(observation),
                exclusionScope:=observation.ExclusionScope,
                exclusionReason:=observation.ExclusionReason,
                signalRuleNumbers:=signalNumbers))
            If signalled Then
                signals.Add(CreateIntrinsicSignal(panelType,
                                                  observation.StageId,
                                                  observation.PointIndex,
                                                  rule,
                                                  statistic > limits.Upper,
                                                  message))
            End If
        End Sub

        Private Shared Sub AddGroupPointAndSignal(
            points As List(Of SpcPointResult),
            signals As List(Of SpcSignalResult),
            panelType As SpcPanelType,
            group As WorkingGroup,
            statistic As Double,
            center As Double,
            limits As LimitPair,
            signalled As Boolean,
            rule As SpcRuleDefinition,
            signalNumber As Integer,
            message As String)

            Dim signalNumbers As Integer() = If(signalled,
                                                New Integer() {signalNumber},
                                                Nothing)
            points.Add(New SpcPointResult(
                group.PointIndex,
                statistic,
                center,
                limits.Lower,
                limits.Upper,
                label:=group.Label,
                stageId:=group.StageId,
                phase:=group.Phase,
                sequenceValue:=group.SequenceValue,
                effectiveSampleSize:=group.Observations.Count,
                sourceRowIndices:=group.SourceRowIndices,
                includedInParameterEstimation:=IsEstimationEligible(group),
                includedInRuleEvaluation:=IsRuleEligible(group),
                exclusionScope:=group.ExclusionScope,
                exclusionReason:=group.ExclusionReason,
                signalRuleNumbers:=signalNumbers))
            If signalled Then
                signals.Add(CreateIntrinsicSignal(panelType,
                                                  group.StageId,
                                                  group.PointIndex,
                                                  rule,
                                                  statistic > limits.Upper,
                                                  message))
            End If
        End Sub

        Private Shared Function CreateIntrinsicRule(
            panelType As SpcPanelType,
            ruleNumber As Integer,
            displayName As String) As SpcRuleDefinition

            Return New SpcRuleDefinition(
                "MV" & CInt(panelType).ToString(CultureInfo.InvariantCulture),
                ruleNumber,
                SpcRuleKind.BeyondSigma,
                1,
                1,
                1.0,
                side:=SpcRuleSide.EitherSide,
                scope:=SpcRuleScope.MultivariatePanels,
                displayName:=displayName,
                description:="The multivariate chart statistic crossed an intrinsic control or decision limit.")
        End Function

        Private Shared Function CreateIntrinsicSignal(
            panelType As SpcPanelType,
            stageId As String,
            pointIndex As Integer,
            rule As SpcRuleDefinition,
            upper As Boolean,
            message As String) As SpcSignalResult

            Return New SpcSignalResult(
                panelType,
                stageId,
                rule,
                pointIndex,
                pointIndex,
                pointIndex,
                triggeredSide:=If(upper,
                                  SpcRuleSide.UpperSideOnly,
                                  SpcRuleSide.LowerSideOnly),
                contributingPointIndices:={pointIndex},
                markedPointIndices:={pointIndex},
                message:=message)
        End Function

        Private Shared Function BuildPanelParameters(
            request As SpcMultivariateRequest,
            model As SpcMultivariateModelResult,
            panelType As SpcPanelType,
            Optional designValue As Double = Double.NaN) As SpcParameterEstimate()

            Dim result As New List(Of SpcParameterEstimate)()
            Dim mode As SpcStageLimitMode = If(
                model.Source = SpcMultivariateModelSource.UseHistoricalParameters,
                SpcStageLimitMode.UseHistoricalParameters,
                SpcStageLimitMode.EstimateFromStageData)
            Dim sampleCount As Nullable(Of Integer) = Nothing
            If model.BaselineObservationCount > 0 Then
                sampleCount = New Nullable(Of Integer)(model.BaselineObservationCount)
            End If
            Dim stages As String() = UniqueStages(request.StageIds)
            For i As Integer = 0 To stages.Length - 1
                result.Add(New SpcParameterEstimate(
                    stages(i),
                    panelType,
                    "EffectiveDimension",
                    model.EffectiveDimension,
                    mode,
                    method:="Numerical covariance rank",
                    displayName:="Effective dimension",
                    sampleCount:=sampleCount))
                result.Add(New SpcParameterEstimate(
                    stages(i),
                    panelType,
                    "ControlLimitAlpha",
                    request.ControlLimitAlpha,
                    mode,
                    method:="Multivariate control-limit design",
                    displayName:="Control-limit alpha",
                    sampleCount:=sampleCount))
                If model.RetainedComponentCount > 0 Then
                    result.Add(New SpcParameterEstimate(
                        stages(i),
                        panelType,
                        "RetainedComponents",
                        model.RetainedComponentCount,
                        mode,
                        method:="PCA selection",
                        displayName:="Retained principal components",
                        sampleCount:=sampleCount))
                End If
                If SpcModelGuards.IsFinite(designValue) Then
                    result.Add(New SpcParameterEstimate(
                        stages(i),
                        panelType,
                        "DesignValue",
                        designValue,
                        mode,
                        method:="Chart design parameter",
                        displayName:="Design limit/value",
                        sampleCount:=sampleCount))
                End If
            Next
            Return result.ToArray()
        End Function

#End Region

#Region "General helpers"

        Private Shared Function IsCompleteRow(values As Double(,), rowIndex As Integer) As Boolean
            For j As Integer = 0 To values.GetLength(1) - 1
                If Double.IsNaN(values(rowIndex, j)) Then Return False
            Next
            Return True
        End Function

        Private Shared Function IsEstimationEligible(observation As WorkingObservation) As Boolean
            Return observation.Phase = SpcPhase.PhaseI AndAlso
                (observation.ExclusionScope And SpcExclusionScope.ParameterEstimation) =
                    SpcExclusionScope.None
        End Function

        Private Shared Function IsEstimationEligible(group As WorkingGroup) As Boolean
            Return group.Phase = SpcPhase.PhaseI AndAlso
                (group.ExclusionScope And SpcExclusionScope.ParameterEstimation) =
                    SpcExclusionScope.None
        End Function

        Private Shared Function IsRuleEligible(observation As WorkingObservation) As Boolean
            Return (observation.ExclusionScope And SpcExclusionScope.RuleEvaluation) =
                SpcExclusionScope.None
        End Function

        Private Shared Function IsRuleEligible(group As WorkingGroup) As Boolean
            Return (group.ExclusionScope And SpcExclusionScope.RuleEvaluation) =
                SpcExclusionScope.None
        End Function

        Private Shared Function ObservationsToMatrix(
            observations As IList(Of WorkingObservation)) As Double(,)

            Dim result(observations.Count - 1,
                       observations(0).Values.Length - 1) As Double
            For i As Integer = 0 To observations.Count - 1
                For j As Integer = 0 To observations(i).Values.Length - 1
                    result(i, j) = observations(i).Values(j)
                Next
            Next
            Return result
        End Function

        Private Shared Function ColumnMeans(values As Double(,)) As Double()
            Dim result(values.GetLength(1) - 1) As Double
            For j As Integer = 0 To result.Length - 1
                Dim sum As Double = 0.0
                Dim compensation As Double = 0.0
                For i As Integer = 0 To values.GetLength(0) - 1
                    Dim adjusted As Double = values(i, j) - compensation
                    Dim nextSum As Double = sum + adjusted
                    compensation = (nextSum - sum) - adjusted
                    sum = nextSum
                Next
                result(j) = sum / values.GetLength(0)
            Next
            Return result
        End Function

        Private Shared Function MeanOfGroupMeans(groups As IList(Of WorkingGroup)) As Double()
            Dim result(groups(0).MeanVector.Length - 1) As Double
            For i As Integer = 0 To groups.Count - 1
                For j As Integer = 0 To result.Length - 1
                    result(j) += groups(i).MeanVector(j)
                Next
            Next
            For j As Integer = 0 To result.Length - 1
                result(j) /= groups.Count
            Next
            Return result
        End Function

        Private Shared Function PooledWithinCovariance(
            groups As IList(Of WorkingGroup),
            totalDegreesOfFreedom As Integer) As Double(,)

            If totalDegreesOfFreedom <= 0 Then
                Throw New ArgumentException("Positive pooled covariance degrees of freedom are required.")
            End If
            Dim p As Integer = groups(0).Covariance.GetLength(0)
            Dim result(p - 1, p - 1) As Double
            For g As Integer = 0 To groups.Count - 1
                Dim weight As Integer = groups(g).Observations.Count - 1
                For i As Integer = 0 To p - 1
                    For j As Integer = 0 To p - 1
                        result(i, j) += weight * groups(g).Covariance(i, j)
                    Next
                Next
            Next
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To p - 1
                    result(i, j) /= totalDegreesOfFreedom
                Next
            Next
            Return result
        End Function

        Private Shared Function CollectSourceRows(
            observations As IList(Of WorkingObservation)) As Integer()

            Dim result(observations.Count - 1) As Integer
            For i As Integer = 0 To observations.Count - 1
                result(i) = observations(i).SourceRowIndices(0)
            Next
            Array.Sort(result)
            Return result
        End Function

        Private Shared Function Subtract(left As Double(), right As Double()) As Double()
            Dim result(left.Length - 1) As Double
            For i As Integer = 0 To result.Length - 1
                result(i) = left(i) - right(i)
            Next
            Return result
        End Function

        Private Shared Function QuadraticForm(vector As Double(), inverse As Double(,)) As Double
            Dim transformed(vector.Length - 1) As Double
            For i As Integer = 0 To vector.Length - 1
                For j As Integer = 0 To vector.Length - 1
                    transformed(i) += inverse(i, j) * vector(j)
                Next
            Next
            Dim result As Double = 0.0
            For i As Integer = 0 To vector.Length - 1
                result += vector(i) * transformed(i)
            Next
            If result < 0.0 AndAlso Math.Abs(result) < 0.000000001 Then Return 0.0
            Return result
        End Function

        Private Shared Function QuadraticContributions(
            vector As Double(),
            inverse As Double(,)) As Double()

            Dim result(vector.Length - 1) As Double
            For i As Integer = 0 To vector.Length - 1
                Dim transformed As Double = 0.0
                For j As Integer = 0 To vector.Length - 1
                    transformed += inverse(i, j) * vector(j)
                Next
                result(i) = vector(i) * transformed
            Next
            Return result
        End Function

        Private Shared Function SquaredElements(values As Double()) As Double()
            Dim result(values.Length - 1) As Double
            For i As Integer = 0 To values.Length - 1
                result(i) = values(i) * values(i)
            Next
            Return result
        End Function

        Private Shared Function SumSquares(values As Double()) As Double
            Dim result As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                result += values(i) * values(i)
            Next
            Return result
        End Function

        Private Shared Function Average(values As Double()) As Double
            If values.Length = 0 Then Return Double.NaN
            Dim result As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                result += values(i)
            Next
            Return result / values.Length
        End Function

        Private Shared Function EmpiricalQuantile(values As Double(), probability As Double) As Double
            If values.Length = 0 Then Return Double.NaN
            Dim sorted As Double() = CType(values.Clone(), Double())
            Array.Sort(sorted)
            If sorted.Length = 1 Then Return sorted(0)
            Dim position As Double = probability * (sorted.Length - 1)
            Dim lower As Integer = CInt(Math.Floor(position))
            Dim upper As Integer = CInt(Math.Ceiling(position))
            If lower = upper Then Return sorted(lower)
            Dim fraction As Double = position - lower
            Return sorted(lower) + fraction * (sorted(upper) - sorted(lower))
        End Function

        Private Shared Function PositiveDeterminant(values As Double(,)) As Double
            Dim determinant As Double = Matrix.MDeterm(CType(values.Clone(), Double(,)))
            If determinant <= 0.0 OrElse Not SpcModelGuards.IsFinite(determinant) Then
                Throw New InvalidOperationException(
                    "The fitted covariance matrix must have a positive determinant.")
            End If
            Return determinant
        End Function

        Private Shared Function UniqueStages(stageIds As String()) As String()
            Dim result As New List(Of String)()
            Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For i As Integer = 0 To stageIds.Length - 1
                If seen.Add(stageIds(i)) Then result.Add(stageIds(i))
            Next
            Return result.ToArray()
        End Function

        Private Shared Function ShouldResetState(
            request As SpcMultivariateRequest,
            previousStage As String,
            previousPhase As Nullable(Of SpcPhase),
            currentStage As String,
            currentPhase As SpcPhase) As Boolean

            If previousStage Is Nothing Then Return True
            If request.ResetAtStageBoundary AndAlso
               Not String.Equals(previousStage,
                                 currentStage,
                                 StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
            If request.ResetAtPhaseBoundary AndAlso
               previousPhase.HasValue AndAlso previousPhase.Value <> currentPhase Then
                Return True
            End If
            Return False
        End Function

        Private Shared Function CombineReason(existingReason As String,
                                              additionalReason As String) As String
            Dim normalized As String = SpcModelGuards.NormalizeOptionalText(additionalReason)
            If normalized.Length = 0 Then Return SpcModelGuards.NormalizeOptionalText(existingReason)
            If String.IsNullOrWhiteSpace(existingReason) Then Return normalized
            If existingReason.IndexOf(normalized,
                                      StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return existingReason
            End If
            Return existingReason & "; " & normalized
        End Function

        Private Shared Function ToNullableFinite(value As Double) As Nullable(Of Double)
            If SpcModelGuards.IsFinite(value) Then Return value
            Return Nothing
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

        Private NotInheritable Class WorkingObservation
            Public Property PointIndex As Integer
            Public Property Values As Double()
            Public Property Label As String = String.Empty
            Public Property Phase As SpcPhase
            Public Property StageId As String = "Stage1"
            Public Property SequenceValue As Nullable(Of Double)
            Public Property SourceRowIndices As Integer() = Array.Empty(Of Integer)()
            Public Property ExclusionScope As SpcExclusionScope
            Public Property ExclusionReason As String = String.Empty
            Public Property SubgroupId As String = String.Empty
        End Class

        Private NotInheritable Class WorkingGroup
            Public Sub New()
                Observations = New List(Of WorkingObservation)()
            End Sub

            Public Property PointIndex As Integer
            Public Property GroupId As String = String.Empty
            Public Property Label As String = String.Empty
            Public Property Phase As SpcPhase
            Public Property StageId As String = "Stage1"
            Public Property SequenceValue As Nullable(Of Double)
            Public Property Observations As List(Of WorkingObservation)
            Public Property MeanVector As Double()
            Public Property Covariance As Double(,)
            Public Property SourceRowIndices As Integer() = Array.Empty(Of Integer)()
            Public Property ExclusionScope As SpcExclusionScope
            Public Property ExclusionReason As String = String.Empty
        End Class

        Private Structure LimitPair
            Public Lower As Double
            Public Upper As Double
        End Structure

        Private Structure DeterminantMoments
            Public B1 As Double
            Public B2 As Double
        End Structure

        Private Structure EigenInformation
            Public Values As Double()
            Public Vectors As Double(,)
            Public Rank As Integer
            Public Tolerance As Double
        End Structure

        Private Structure InverseInformation
            Public Inverse As Double(,)
            Public UsedPseudoInverse As Boolean
        End Structure

        Private Structure PcaProjection
            Public WorkingDifference As Double()
            Public Scores As Double()
            Public Residual As Double()
        End Structure

        Private Structure QLimitInformation
            Public ExpectedValue As Double
            Public UpperLimit As Double
            Public UsedFallback As Boolean
        End Structure

#End Region

    End Class

End Namespace
