Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>Requested scalars only. Core validates applicability and numerical ranges.</summary>
    Public NotInheritable Class SpcMultivariateOptions
        Public Sub New(Optional missingValuePolicy As SpcMissingValuePolicy = SpcMissingValuePolicy.Reject,
                       Optional controlLimitAlpha As Double = 0.0027R,
                       Optional useLowerHotellingLimit As Boolean = False,
                       Optional covarianceRegularization As Double = 0.0R,
                       Optional allowPseudoInverse As Boolean = True,
                       Optional pcaUseCorrelationMatrix As Boolean = False,
                       Optional pcaComponentCount As Integer? = Nothing,
                       Optional pcaCumulativeVariance As Double = 0.9R,
                       Optional mewmaLambda As Double = 0.2R,
                       Optional mewmaControlLimit As Double? = Nothing,
                       Optional mcusumReferenceValue As Double = 0.5R,
                       Optional mcusumDecisionInterval As Double = 5.5R,
                       Optional generalizedVarianceSigmaMultiplier As Double? = Nothing,
                       Optional resetAtStageBoundary As Boolean = True,
                       Optional resetAtPhaseBoundary As Boolean = True,
                       Optional resetAfterSignal As Boolean = False,
                       Optional requestLabel As String = Nothing,
                       Optional sequenceGapBehavior As SpcSequenceGapBehavior = SpcSequenceGapBehavior.BreakSequence)
            Me.MissingValuePolicy = missingValuePolicy
            Me.ControlLimitAlpha = controlLimitAlpha
            Me.UseLowerHotellingLimit = useLowerHotellingLimit
            Me.CovarianceRegularization = covarianceRegularization
            Me.AllowPseudoInverse = allowPseudoInverse
            Me.PcaUseCorrelationMatrix = pcaUseCorrelationMatrix
            Me.PcaComponentCount = pcaComponentCount
            Me.PcaCumulativeVariance = pcaCumulativeVariance
            Me.MewmaLambda = mewmaLambda
            Me.MewmaControlLimit = mewmaControlLimit
            Me.McusumReferenceValue = mcusumReferenceValue
            Me.McusumDecisionInterval = mcusumDecisionInterval
            Me.GeneralizedVarianceSigmaMultiplier = generalizedVarianceSigmaMultiplier
            Me.ResetAtStageBoundary = resetAtStageBoundary
            Me.ResetAtPhaseBoundary = resetAtPhaseBoundary
            Me.ResetAfterSignal = resetAfterSignal
            Me.RequestLabel = requestLabel
            Me.SequenceGapBehavior = sequenceGapBehavior
        End Sub
        Public ReadOnly Property MissingValuePolicy As SpcMissingValuePolicy
        Public ReadOnly Property ControlLimitAlpha As Double
        Public ReadOnly Property UseLowerHotellingLimit As Boolean
        Public ReadOnly Property CovarianceRegularization As Double
        Public ReadOnly Property AllowPseudoInverse As Boolean
        Public ReadOnly Property PcaUseCorrelationMatrix As Boolean
        Public ReadOnly Property PcaComponentCount As Integer?
        Public ReadOnly Property PcaCumulativeVariance As Double
        Public ReadOnly Property MewmaLambda As Double
        Public ReadOnly Property MewmaControlLimit As Double?
        Public ReadOnly Property McusumReferenceValue As Double
        Public ReadOnly Property McusumDecisionInterval As Double
        Public ReadOnly Property GeneralizedVarianceSigmaMultiplier As Double?
        Public ReadOnly Property ResetAtStageBoundary As Boolean
        Public ReadOnly Property ResetAtPhaseBoundary As Boolean
        Public ReadOnly Property ResetAfterSignal As Boolean
        Public ReadOnly Property RequestLabel As String
        Public ReadOnly Property SequenceGapBehavior As SpcSequenceGapBehavior
    End Class

    ''' <summary>Ordered variable axis and optional historical shapes; no covariance mathematics.</summary>
    Public NotInheritable Class SpcMultivariateModelIntent
        Private ReadOnly _names As String()
        Private ReadOnly _mean As Double()
        Private ReadOnly _covariance As Double(,)
        Public Sub New(variableNames As String(), source As SpcMultivariateModelSource, Optional mean As Double() = Nothing, Optional covariance As Double(,) = Nothing)
            _names = If(variableNames Is Nothing, Nothing, CType(variableNames.Clone(), String()))
            _mean = If(mean Is Nothing, Nothing, CType(mean.Clone(), Double()))
            _covariance = If(covariance Is Nothing, Nothing, CType(covariance.Clone(), Double(,)))
            Me.Source = source
        End Sub
        Public ReadOnly Property Source As SpcMultivariateModelSource
        Public ReadOnly Property VariableNames As String()
            Get
                Return If(_names Is Nothing, Nothing, CType(_names.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property HistoricalMean As Double()
            Get
                Return If(_mean Is Nothing, Nothing, CType(_mean.Clone(), Double()))
            End Get
        End Property
        Public ReadOnly Property HistoricalCovariance As Double(,)
            Get
                Return If(_covariance Is Nothing, Nothing, CType(_covariance.Clone(), Double(,)))
            End Get
        End Property
        Public ReadOnly Property HistoricalMeanLength As Integer?
            Get
                Return If(_mean Is Nothing, CType(Nothing, Integer?), _mean.Length)
            End Get
        End Property
        Public ReadOnly Property CovarianceRows As Integer?
            Get
                Return If(_covariance Is Nothing, CType(Nothing, Integer?), _covariance.GetLength(0))
            End Get
        End Property
        Public ReadOnly Property CovarianceColumns As Integer?
            Get
                Return If(_covariance Is Nothing, CType(Nothing, Integer?), _covariance.GetLength(1))
            End Get
        End Property
        Friend Function Compose(chart As SpcChartType, measurements As Double(,), options As SpcMultivariateOptions, subgroupIds As String(), labels As String(), phases As SpcPhase(), stages As String(), sequences As Double(), sourceRows As Integer(), scopes As SpcExclusionScope(), reasons As String()) As SpcMultivariateRequest
            'Borrow measurements synchronously; Core performs the sole N x P durable copy.
            Return New SpcMultivariateRequest(chart, measurements, variableNames:=_names,
                subgroupIds:=subgroupIds, labels:=labels, phases:=phases, stageIds:=stages,
                sequenceValues:=sequences, sourceRowIndices:=sourceRows, exclusionScopes:=scopes, exclusionReasons:=reasons,
                missingValuePolicy:=options.MissingValuePolicy, modelSource:=Source,
                historicalMean:=_mean, historicalCovariance:=_covariance,
                controlLimitAlpha:=options.ControlLimitAlpha, useLowerHotellingLimit:=options.UseLowerHotellingLimit,
                covarianceRegularization:=options.CovarianceRegularization, allowPseudoInverse:=options.AllowPseudoInverse,
                pcaUseCorrelationMatrix:=options.PcaUseCorrelationMatrix, pcaComponentCount:=options.PcaComponentCount,
                pcaCumulativeVariance:=options.PcaCumulativeVariance, mewmaLambda:=options.MewmaLambda, mewmaControlLimit:=options.MewmaControlLimit,
                mcusumReferenceValue:=options.McusumReferenceValue, mcusumDecisionInterval:=options.McusumDecisionInterval,
                generalizedVarianceSigmaMultiplier:=options.GeneralizedVarianceSigmaMultiplier,
                resetAtStageBoundary:=options.ResetAtStageBoundary, resetAtPhaseBoundary:=options.ResetAtPhaseBoundary,
                resetAfterSignal:=options.ResetAfterSignal, requestLabel:=options.RequestLabel, sequenceGapBehavior:=options.SequenceGapBehavior)
        End Function
    End Class

    ''' <summary>Existing six-choice presentation applicability, without activating client methods.</summary>
    Public NotInheritable Class SpcMultivariateChartCompatibility
        Friend Sub New(chart As SpcChartType)
            ChartType = chart
        End Sub
        Public ReadOnly Property ChartType As SpcChartType
        Public ReadOnly Property AllowsSubgroups As Boolean
            Get
                Return ChartType = SpcChartType.HotellingT2 OrElse ChartType = SpcChartType.GeneralizedVariance
            End Get
        End Property
        Public ReadOnly Property RequiresSubgroups As Boolean
            Get
                Return ChartType = SpcChartType.GeneralizedVariance
            End Get
        End Property
        Public ReadOnly Property IsPca As Boolean
            Get
                Return ChartType = SpcChartType.PcaT2 OrElse ChartType = SpcChartType.PcaQ
            End Get
        End Property
        Public ReadOnly Property IsSequential As Boolean
            Get
                Return ChartType = SpcChartType.Mewma OrElse ChartType = SpcChartType.Mcusum
            End Get
        End Property
        Public Function UseLowerHotellingLimitAvailable(grouped As Boolean) As Boolean
            Return ChartType = SpcChartType.HotellingT2 AndAlso Not grouped
        End Function
        Public Function AlphaEnabled(suppliedMewmaLimit As Boolean, suppliedGvMultiplier As Boolean) As Boolean
            Return ChartType <> SpcChartType.Mcusum AndAlso Not (ChartType = SpcChartType.Mewma AndAlso suppliedMewmaLimit) AndAlso Not (ChartType = SpcChartType.GeneralizedVariance AndAlso suppliedGvMultiplier)
        End Function
    End Class

    Public NotInheritable Class SpcMultivariateRequestPolicy
        Private Sub New()
        End Sub
        Public Shared Function Compatibility(chart As SpcChartType) As SpcMultivariateChartCompatibility
            Return New SpcMultivariateChartCompatibility(chart)
        End Function
        Public Shared Function Create(chart As SpcChartType, measurements As Double(,), model As SpcMultivariateModelIntent, options As SpcMultivariateOptions,
            Optional subgroupIds As String() = Nothing, Optional labels As String() = Nothing, Optional phases As SpcPhase() = Nothing, Optional stageIds As String() = Nothing,
            Optional sequenceValues As Double() = Nothing, Optional sourceRowIndices As Integer() = Nothing, Optional exclusionScopes As SpcExclusionScope() = Nothing, Optional exclusionReasons As String() = Nothing) As SpcMultivariateRequest
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            Return model.Compose(chart, measurements, options, subgroupIds, labels, phases, stageIds, sequenceValues, sourceRowIndices, exclusionScopes, exclusionReasons)
        End Function
    End Class
End Namespace
