Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.Agreement

Namespace ExcelCommon
    Public Enum DemingExecutionIntent
        AnalyticalClosedForm
        AnalyticalMcr
        Jackknife
        Bootstrap
        Fit
    End Enum
    ''' <summary>Original radio precedence; the numerical methods and progress reporter stay with their owners.</summary>
    Public NotInheritable Class DemingRequestSelection
        Private ReadOnly options As DemingOptions
        Public Sub New(parsed As DemingOptions, percentile As Boolean, bca As Boolean, closedForm As Boolean, linnet As Boolean)
            options = Copy(parsed)
            If percentile Then
                options.CiMethod = AgreementCiMethod.BootstrapPercentile
            ElseIf bca Then
                options.CiMethod = AgreementCiMethod.BootstrapBCa
            ElseIf closedForm OrElse linnet Then
                options.CiMethod = AgreementCiMethod.Analytical
            Else
                options.CiMethod = AgreementCiMethod.Jackknife
            End If
            If options.CiMethod = AgreementCiMethod.Analytical Then
                Execution = If(linnet, DemingExecutionIntent.AnalyticalMcr, DemingExecutionIntent.AnalyticalClosedForm)
            ElseIf options.CiMethod = AgreementCiMethod.Jackknife Then
                Execution = DemingExecutionIntent.Jackknife
            ElseIf options.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse options.CiMethod = AgreementCiMethod.BootstrapBCa Then
                Execution = DemingExecutionIntent.Bootstrap
            Else
                Execution = DemingExecutionIntent.Fit
            End If
        End Sub
        Public ReadOnly Property Execution As DemingExecutionIntent
        Public Function CreateOptions() As DemingOptions
            Return Copy(options)
        End Function
        Private Shared Function Copy(source As DemingOptions) As DemingOptions
            Return New DemingOptions With {.Alpha = source.Alpha, .CiMethod = source.CiMethod, .VarianceModel = source.VarianceModel,
                .Lambda = source.Lambda, .SDx = If(source.SDx Is Nothing, Nothing, CType(source.SDx.Clone(), Double())),
                .SDy = If(source.SDy Is Nothing, Nothing, CType(source.SDy.Clone(), Double())), .CVx = source.CVx, .CVy = source.CVy,
                .BootstrapReplicates = source.BootstrapReplicates, .FitIntercept = source.FitIntercept}
        End Function
        Public Shared Function VarianceForIndex(index As Integer) As DemingVarianceModel
            Return If(index = 1, DemingVarianceModel.ConstantCV, If(index = 2, DemingVarianceModel.KnownPointwiseSD, DemingVarianceModel.ConstantLambda))
        End Function
    End Class
    Public NotInheritable Class LinRequestSelection
        Private ReadOnly options As LinConcordanceOptions
        Public Sub New(parsed As LinConcordanceOptions, analytical As Boolean, percentile As Boolean, bca As Boolean)
            options = Copy(parsed)
            options.CiMethod = If(analytical, AgreementCiMethod.Analytical, If(percentile, AgreementCiMethod.BootstrapPercentile, If(bca, AgreementCiMethod.BootstrapBCa, AgreementCiMethod.Analytical)))
        End Sub
        Public ReadOnly Property UsesBootstrap As Boolean
            Get
                Return options.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse options.CiMethod = AgreementCiMethod.BootstrapBCa
            End Get
        End Property
        Public Function CreateOptions() As LinConcordanceOptions
            Return Copy(options)
        End Function
        Private Shared Function Copy(source As LinConcordanceOptions) As LinConcordanceOptions
            Return New LinConcordanceOptions With {.Alpha = source.Alpha, .CiMethod = source.CiMethod, .BootstrapReplicates = source.BootstrapReplicates,
                .NullConcordance = source.NullConcordance, .SubjectIds = If(source.SubjectIds Is Nothing, Nothing, CType(source.SubjectIds.Clone(), Object()))}
        End Function
    End Class
    Public NotInheritable Class KappaRequestSelection
        Private ReadOnly options As KappaOptions
        Public Sub New(parsed As KappaOptions, weightIndex As Integer, analytical As Boolean, jackknife As Boolean, percentile As Boolean, bca As Boolean)
            options = Copy(parsed)
            options.Weighting = If(weightIndex >= 0 AndAlso weightIndex <= 4, CType(weightIndex, KappaWeightingScheme), KappaWeightingScheme.Unweighted)
            options.CiMethod = If(analytical, AgreementCiMethod.Analytical, If(jackknife, AgreementCiMethod.Jackknife, If(percentile, AgreementCiMethod.BootstrapPercentile, If(bca, AgreementCiMethod.BootstrapBCa, AgreementCiMethod.Analytical))))
        End Sub
        Public ReadOnly Property UsesResampling As Boolean
            Get
                Return options.CiMethod = AgreementCiMethod.Jackknife OrElse options.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse options.CiMethod = AgreementCiMethod.BootstrapBCa
            End Get
        End Property
        Public Function CreateOptions() As KappaOptions
            Return Copy(options)
        End Function
        Private Shared Function Copy(source As KappaOptions) As KappaOptions
            Return New KappaOptions With {.Alpha = source.Alpha, .Weighting = source.Weighting, .CiMethod = source.CiMethod,
                .BootstrapReplicates = source.BootstrapReplicates, .CustomWeights = If(source.CustomWeights Is Nothing, Nothing, CType(source.CustomWeights.Clone(), Double(,))),
                .Categories = If(source.Categories Is Nothing, Nothing, CType(source.Categories.Clone(), Object()))}
        End Function
    End Class
    ''' <summary>Composes 04A.04 ProjectBland; native parsing/ValidateBland call stay at the same boundary.</summary>
    Public NotInheritable Class BlandRequestSelection
        Private ReadOnly options As BlandAltmanOptions
        Public Sub New(parsed As BlandAltmanOptions, xAxisIndex As Integer, plotIndex As Integer)
            options = Copy(parsed)
            options.XAxisMode = If(xAxisIndex = 1, BlandAltmanXAxisMode.ReferenceMethod, If(xAxisIndex = 2, BlandAltmanXAxisMode.TestMethod, BlandAltmanXAxisMode.MeanOfMethods))
            options.PlotMode = If(plotIndex = 1, RepeatedBlandAltmanPlotMode.SubjectMeansOnly, If(plotIndex = 2, RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans, RepeatedBlandAltmanPlotMode.AllObservations))
            If options.BootstrapReplicates <= 0 Then options.BootstrapReplicates = 2000
        End Sub
        Public Function CreateOptions() As BlandAltmanOptions
            Return Copy(options)
        End Function
        Private Shared Function Copy(source As BlandAltmanOptions) As BlandAltmanOptions
            Return New BlandAltmanOptions With {.Alpha = source.Alpha, .Mode = source.Mode, .Scale = source.Scale, .CiMethod = source.CiMethod,
                .XAxisMode = source.XAxisMode, .PlotMode = source.PlotMode, .UseTDistribution = source.UseTDistribution,
                .BootstrapReplicates = source.BootstrapReplicates, .SubjectIds = If(source.SubjectIds Is Nothing, Nothing, CType(source.SubjectIds.Clone(), Object())),
                .ExcludeSingletonSubjects = source.ExcludeSingletonSubjects, .MinSubjects = source.MinSubjects, .MinPairsPerSubject = source.MinPairsPerSubject,
                .CheckProportionalBias = source.CheckProportionalBias, .AllowFallbackToSimple = source.AllowFallbackToSimple}
        End Function
    End Class
End Namespace
