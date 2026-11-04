Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    Public NotInheritable Class SpcLimitRequest
        Public Sub New(source As SpcParameterSource, method As SpcControlLimitMethod, sigma As Double, estimator As SpcWithinSigmaEstimator, natural As SpcNaturalLimitPolicy, movingRangeLength As Integer, bias As Boolean)
            ParameterSource = source : Me.Method = method : SigmaMultiplier = sigma
            WithinSigmaEstimator = estimator : NaturalLimitPolicy = natural
            Me.MovingRangeLength = movingRangeLength : UseBiasCorrection = bias
        End Sub
        Public ReadOnly Property ParameterSource As SpcParameterSource
        Public ReadOnly Property Method As SpcControlLimitMethod
        Public ReadOnly Property SigmaMultiplier As Double
        Public ReadOnly Property WithinSigmaEstimator As SpcWithinSigmaEstimator
        Public ReadOnly Property NaturalLimitPolicy As SpcNaturalLimitPolicy
        Public ReadOnly Property MovingRangeLength As Integer
        Public ReadOnly Property UseBiasCorrection As Boolean
        Public Function ToCore() As SpcControlLimitOptions
            Return New SpcControlLimitOptions With {.ParameterSource = ParameterSource, .Method = Method, .SigmaMultiplier = SigmaMultiplier, .WithinSigmaEstimator = WithinSigmaEstimator, .NaturalLimitPolicy = NaturalLimitPolicy, .MovingRangeLength = MovingRangeLength, .UseBiasCorrection = UseBiasCorrection}
        End Function
    End Class
    Public NotInheritable Class SpcRuleRequest
        Private ReadOnly _custom As SpcRuleDefinition()
        Public Sub New(preset As SpcRulePreset, phaseScope As SpcRulePhaseScope, gap As SpcSequenceGapBehavior, marking As SpcSignalMarkingMode, Optional custom() As SpcRuleDefinition = Nothing)
            Me.Preset = preset : Me.PhaseScope = phaseScope : GapBehavior = gap : MarkingMode = marking
            _custom = If(custom Is Nothing, Array.Empty(Of SpcRuleDefinition)(), CType(custom.Clone(), SpcRuleDefinition()))
        End Sub
        Public ReadOnly Property Preset As SpcRulePreset
        Public ReadOnly Property PhaseScope As SpcRulePhaseScope
        Public ReadOnly Property GapBehavior As SpcSequenceGapBehavior
        Public ReadOnly Property MarkingMode As SpcSignalMarkingMode
        Public Function WithCustomRules(rules() As SpcRuleDefinition) As SpcRuleRequest
            Return New SpcRuleRequest(Preset, PhaseScope, GapBehavior, MarkingMode, rules)
        End Function
        Public Function ToCore(chartType As SpcChartType) As SpcRuleOptions
            Return New SpcRuleOptions With {.Preset = If(SpcRequestPreparation.IsTimeWeightedChart(chartType), SpcRulePreset.None, Preset), .CustomRules = CType(_custom.Clone(), SpcRuleDefinition()), .PhaseScope = PhaseScope, .GapBehavior = GapBehavior, .MarkingMode = MarkingMode}
        End Function
        Public Shared Function FinishCustom(rules As IEnumerable(Of SpcRuleDefinition), allowEmpty As Boolean) As SpcRuleDefinition()
            Dim result = rules.ToArray()
            If result.Length = 0 AndAlso Not allowEmpty Then Throw New ArgumentException("Enable at least one custom signal rule.")
            Return result
        End Function
    End Class
    Public NotInheritable Class SpcRequestPreparation
        Private Sub New()
        End Sub
    Public Shared Function IsAttributeChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.PChart OrElse
               chartType = SpcChartType.NpChart OrElse
               chartType = SpcChartType.CChart OrElse
               chartType = SpcChartType.UChart
    End Function
    Public Shared Function IsSubgroupChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.SubgroupRange OrElse
               chartType = SpcChartType.SubgroupStandardDeviation OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS
    End Function
    Public Shared Function IsTimeWeightedChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Cusum OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function
    Public Shared Function ChartCanUseSpecificationLines(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Individuals OrElse
               chartType = SpcChartType.IndividualsMovingRange OrElse
               chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function
        Public Shared Function Analysis(missing As SpcMissingValuePolicy, limits As SpcLimitRequest, rules As SpcRuleRequest, chartType As SpcChartType, stages() As SpcStageDefinition, exclusions() As SpcExclusionDefinition) As SpcAnalysisOptions
            Return New SpcAnalysisOptions With {.MissingValuePolicy = missing, .ControlLimits = limits.ToCore(), .Rules = rules.ToCore(chartType), .Stages = stages, .Exclusions = exclusions}
        End Function
        Public Shared Function HistoricalRow(stageId As String, mean As Double?, sigma As Double?, proportion As Double?, meanCount As Double?, meanRate As Double?) As SpcHistoricalParameters
            If Not mean.HasValue AndAlso Not sigma.HasValue AndAlso Not proportion.HasValue AndAlso Not meanCount.HasValue AndAlso Not meanRate.HasValue Then
                If stageId.Length = 0 Then Return Nothing
                Throw New ArgumentException("Historical-parameter row for stage '" & stageId & "' contains no parameter relevant to the selected chart.")
            End If
            Return New SpcHistoricalParameters(stageId:=If(stageId.Length = 0, Nothing, stageId), processMean:=mean, processSigma:=sigma, nonconformingProportion:=proportion, meanDefectCount:=meanCount, meanDefectRate:=meanRate)
        End Function
        Public Shared Function Create(chartType As SpcChartType, input As SpcInputData, analysisOptions As SpcAnalysisOptions, historicalParameters() As SpcHistoricalParameters, specificationLimits As SpcSpecificationLimits, chartParameters As SpcChartParameters, requestLabel As String, chartTitle As String, valueAxisTitle As String) As SpcFitRequest
            Return New SpcFitRequest(chartType, input, analysisOptions, historicalParameters, specificationLimits, chartParameters, requestLabel, chartTitle, valueAxisTitle)
        End Function
    End Class
End Namespace
