Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum MixedModelMeansMode
        ObservedDesign
        ReferenceGrid
    End Enum
    Public Enum MixedModelContrastSelection
        None
        Pairwise
        EachVsControl
        SelectedComparison
    End Enum
    Public Enum MixedModelContrastDirection
        HigherMinusLower
        TreatmentMinusControl
        ControlMinusTreatment
    End Enum
    Public Enum MixedModelSupplementalSection
        ClassInformation
        MeansByVisit
        ChangeByVisit
        MeansByVisitAndGroup
        GroupDifferences
        ChangeByGroup
        DifferenceInChange
        ReferenceMeans
        ReferenceGroupContrasts
    End Enum

    Public NotInheritable Class MixedModelReportOptions
        Public Property Alpha As Double = 0.05
        Public Property Diagnostics As Boolean = True
        Public Property IterationDetails As Boolean
        Public Property Trace As Boolean
        Public Property Residuals As Boolean
        Public Property RandomCovariance As Boolean = True
        Public Property ResidualCovariance As Boolean = True
        Public Property RandomEffects As Boolean = True
        Public Property ClassInformation As Boolean = True
        Public Property EstimatedMeans As Boolean = True
        Public Property ChangeFromBaseline As Boolean = True
        Public Property DifferenceInChange As Boolean = True
        Public Property MeansMode As MixedModelMeansMode
        Public Property Contrast As MixedModelContrastSelection = MixedModelContrastSelection.Pairwise
        Public Property Direction As MixedModelContrastDirection
        Public Property Weighting As MixedModelReferenceGridWeighting = MixedModelReferenceGridWeighting.EqualCells
        Public Property CovariateMode As MixedModelCovariateReferenceMode = MixedModelCovariateReferenceMode.ObservedMean
        Public Property CovariateValue As Double = 0
        Public Property Multiplicity As MixedModelMultiplicityAdjustment
        Public Property GroupSelection As String
        Public Property BaselineSelection As String
        Public Property ControlSelection As String
        Public Property ComparisonSelection As String
        Public Property BaselineVisit As Nullable(Of Double)
        Public Property ControlLevel As Nullable(Of Double)
        Public Property ComparisonLevel As Nullable(Of Double)
        ' Lossless GUI text for the original Core string seams (including unknown fallbacks).
        Public Property RequestedContrastText As String
        Public Property RequestedDirectionText As String
        Public Function Copy() As MixedModelReportOptions
            Return DirectCast(MemberwiseClone(), MixedModelReportOptions)
        End Function
    End Class

    Public NotInheritable Class MixedModelSupplementalPlan
        Private ReadOnly _options As MixedModelReportOptions
        Friend Sub New(options As MixedModelReportOptions, sections As IEnumerable(Of MixedModelSupplementalSection))
            _options = options.Copy() : Me.Sections = New List(Of MixedModelSupplementalSection)(sections).AsReadOnly()
        End Sub
        Public ReadOnly Property Sections As IReadOnlyList(Of MixedModelSupplementalSection)
        Public ReadOnly Property Options As MixedModelReportOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Function Includes(section As MixedModelSupplementalSection) As Boolean
            Return Sections.Contains(section)
        End Function
    End Class

    Public NotInheritable Class MixedModelWorkflowReport
        Friend Sub New(kind As MixedModelWorkflowKind, result As MixedModelResult, tables As IEnumerable(Of ResultTable), Optional alpha As Double = 0.05,
                       Optional decision As MixedModelFitDecision = Nothing)
            RequestedDecision = decision
            If result IsNot Nothing Then
                Converged = result.Converged : Cancelled = result.Cancelled : Interrupted = result.Interrupted
            End If
            Diagnostics = MixedModelDiagnosticPolicy.Capture(result, alpha)
            Document = New AnalysisResultDocument("mixed." & kind.ToString().ToLowerInvariant(),
                If(tables, Enumerable.Empty(Of ResultTable)()).Select(Function(t) t.ToOutputModel()),
                Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, Diagnostics.Warnings,
                If(result Is Nothing, New AnalysisFault() {New AnalysisFault("mixed.fit.unavailable", "No Core result was supplied.")}, Array.Empty(Of AnalysisFault)()))
            Sections = MixedModelDiagnosticPolicy.DescribeTables(Document)
        End Sub
        Public ReadOnly Property Document As AnalysisResultDocument
        Public ReadOnly Property Diagnostics As MixedModelDiagnosticSnapshot
        Public ReadOnly Property Sections As IReadOnlyList(Of MixedModelReportSection)
        Public ReadOnly Property RequestedDecision As MixedModelFitDecision
        Public ReadOnly Property Converged As Nullable(Of Boolean)
        Public ReadOnly Property Cancelled As Nullable(Of Boolean)
        Public ReadOnly Property Interrupted As Nullable(Of Boolean)
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class

    ''' <summary>Detached reference-grid role/options snapshot. Core owns level merging,
    ''' reference rows, marginalization, L matrices and all subsequent inference.</summary>
    Public NotInheritable Class MixedModelReferenceIntent
        Private ReadOnly _names As String()
        Private ReadOnly _visits, _groups As Double()
        Private ReadOnly _options As MixedModelReportOptions
        Public Sub New(fixedNames() As String, visitKey As String, visits() As Double, groupKey As String, groups() As Double,
                       options As MixedModelReportOptions)
            _names = If(fixedNames Is Nothing, Nothing, DirectCast(fixedNames.Clone(), String()))
            _visits = If(visits Is Nothing, Nothing, DirectCast(visits.Clone(), Double()))
            _groups = If(groups Is Nothing, Nothing, DirectCast(groups.Clone(), Double()))
            Me.VisitKey = visitKey : Me.GroupKey = groupKey : _options = options.Copy()
        End Sub
        Public ReadOnly Property VisitKey As String
        Public ReadOnly Property GroupKey As String
        Public ReadOnly Property FixedNames As String()
            Get
                Return If(_names Is Nothing, Nothing, DirectCast(_names.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property Visits As Double()
            Get
                Return If(_visits Is Nothing, Nothing, DirectCast(_visits.Clone(), Double()))
            End Get
        End Property
        Public ReadOnly Property Groups As Double()
            Get
                Return If(_groups Is Nothing, Nothing, DirectCast(_groups.Clone(), Double()))
            End Get
        End Property
        Public ReadOnly Property Options As MixedModelReportOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Function CreateCoreSpec() As MixedModelReferenceGridSpec
            Dim spec As New MixedModelReferenceGridSpec With {.FixedEffectNames = If(_names Is Nothing, Nothing, DirectCast(_names.Clone(), String())),
                .Weighting = _options.Weighting, .MultiplicityAdjustment = _options.Multiplicity, .Alpha = _options.Alpha}
            Dim levels = MixedModelPostEstimation.UniqueSortedFiniteValues(_visits)
            If levels IsNot Nothing AndAlso levels.Length > 0 Then spec.AddByFactor(MixedModelReferenceGridService.NormalizeProfileName(VisitKey), levels, _visits)
            If Not String.IsNullOrWhiteSpace(GroupKey) AndAlso _groups IsNot Nothing Then
                levels = MixedModelPostEstimation.UniqueSortedFiniteValues(_groups)
                If levels IsNot Nothing AndAlso levels.Length > 1 Then spec.AddByFactor(MixedModelReferenceGridService.NormalizeProfileName(GroupKey), levels, _groups)
            End If
            Return spec
        End Function
    End Class

    Public NotInheritable Class MixedModelReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(kind As MixedModelWorkflowKind, result As MixedModelResult, options As MixedModelReportOptions) As MixedModelWorkflowReport
            Return Project(kind, result, options, Nothing)
        End Function
        Public Shared Function Project(kind As MixedModelWorkflowKind, result As MixedModelResult, options As MixedModelReportOptions,
                                       decision As MixedModelFitDecision) As MixedModelWorkflowReport
            Dim tables As List(Of ResultTable) = Nothing
            If result IsNot Nothing Then tables = result.wrapResults(options.Alpha, options.IterationDetails, kind = MixedModelWorkflowKind.LMM, options.Diagnostics)
            Return New MixedModelWorkflowReport(kind, result, tables, options.Alpha, decision)
        End Function
        Public Shared Function ShouldWriteTrace(trace As Boolean, details As Boolean, text As String) As Boolean
            Return (trace OrElse details) AndAlso Not String.IsNullOrWhiteSpace(text)
        End Function
        Public Shared Function ShouldWriteResidual(selected As Boolean, residual() As Double, observations As Integer) As Boolean
            Return selected AndAlso residual IsNot Nothing AndAlso residual.Length = observations
        End Function
        Public Shared Function Plan(kind As MixedModelWorkflowKind, options As MixedModelReportOptions,
                                    hasGroup As Boolean, Optional usableFitInputs As Boolean = True) As MixedModelSupplementalPlan
            Dim sections As New List(Of MixedModelSupplementalSection)
            If Not usableFitInputs Then Return New MixedModelSupplementalPlan(options, sections)
            If options.ClassInformation Then sections.Add(MixedModelSupplementalSection.ClassInformation)
            If kind = MixedModelWorkflowKind.LMM OrElse Not options.EstimatedMeans Then Return New MixedModelSupplementalPlan(options, sections)
            Dim contrasts = options.Contrast <> MixedModelContrastSelection.None
            If options.MeansMode = MixedModelMeansMode.ReferenceGrid Then
                sections.Add(MixedModelSupplementalSection.ReferenceMeans)
                If hasGroup AndAlso contrasts Then sections.Add(MixedModelSupplementalSection.ReferenceGroupContrasts)
            Else
                sections.Add(MixedModelSupplementalSection.MeansByVisit)
                If options.ChangeFromBaseline Then sections.Add(MixedModelSupplementalSection.ChangeByVisit)
                If hasGroup Then
                    sections.Add(MixedModelSupplementalSection.MeansByVisitAndGroup)
                    If contrasts Then sections.Add(MixedModelSupplementalSection.GroupDifferences)
                    If options.ChangeFromBaseline Then
                        sections.Add(MixedModelSupplementalSection.ChangeByGroup)
                        If contrasts AndAlso options.DifferenceInChange Then sections.Add(MixedModelSupplementalSection.DifferenceInChange)
                    End If
                End If
            End If
            Return New MixedModelSupplementalPlan(options, sections)
        End Function
        Public Shared Function MeansMode(text As String) As MixedModelMeansMode
            Return If(String.Equals(text, "Reference grid", StringComparison.OrdinalIgnoreCase), MixedModelMeansMode.ReferenceGrid, MixedModelMeansMode.ObservedDesign)
        End Function
        Public Shared Function Contrast(text As String) As MixedModelContrastSelection
            If String.Equals(text, "None", StringComparison.OrdinalIgnoreCase) Then Return MixedModelContrastSelection.None
            If String.Equals(text, "Each group vs control", StringComparison.OrdinalIgnoreCase) Then Return MixedModelContrastSelection.EachVsControl
            If String.Equals(text, "Selected comparison only", StringComparison.OrdinalIgnoreCase) Then Return MixedModelContrastSelection.SelectedComparison
            Return MixedModelContrastSelection.Pairwise
        End Function
        Public Shared Function Direction(text As String) As MixedModelContrastDirection
            If String.Equals(text, "Treatment - control", StringComparison.OrdinalIgnoreCase) Then Return MixedModelContrastDirection.TreatmentMinusControl
            If String.Equals(text, "Control - treatment", StringComparison.OrdinalIgnoreCase) Then Return MixedModelContrastDirection.ControlMinusTreatment
            Return MixedModelContrastDirection.HigherMinusLower
        End Function
        Public Shared Function Weighting(text As String) As MixedModelReferenceGridWeighting
            Return If(String.Equals(text, "Observed class-cell weights", StringComparison.OrdinalIgnoreCase), MixedModelReferenceGridWeighting.ObservedCellFrequency, MixedModelReferenceGridWeighting.EqualCells)
        End Function
        Public Shared Function CovariateMode(text As String) As MixedModelCovariateReferenceMode
            Return If(String.Equals(text, "Continuous covariates at 0", StringComparison.OrdinalIgnoreCase), MixedModelCovariateReferenceMode.UserSpecified, MixedModelCovariateReferenceMode.ObservedMean)
        End Function
        Public Shared Function Multiplicity(text As String) As MixedModelMultiplicityAdjustment
            If String.Equals(text, "Bonferroni", StringComparison.OrdinalIgnoreCase) Then Return MixedModelMultiplicityAdjustment.Bonferroni
            If String.Equals(text, "Holm", StringComparison.OrdinalIgnoreCase) Then Return MixedModelMultiplicityAdjustment.Holm
            If String.Equals(text, "Sidak", StringComparison.OrdinalIgnoreCase) Then Return MixedModelMultiplicityAdjustment.Sidak
            Return MixedModelMultiplicityAdjustment.None
        End Function
        Public Shared Function DirectedPair(direction As MixedModelContrastDirection, control As Double, comparison As Double) As Double()
            If direction = MixedModelContrastDirection.TreatmentMinusControl Then Return {comparison, control}
            If direction = MixedModelContrastDirection.ControlMinusTreatment Then Return {control, comparison}
            Return If(comparison >= control, New Double() {comparison, control}, New Double() {control, comparison})
        End Function
        Public Shared Function Validate(options As MixedModelReportOptions, Optional visitLevels() As Double = Nothing,
                                        Optional groupLevels() As Double = Nothing) As RegressionValidation
            Dim issues As New List(Of RegressionDiagnostic)
            If options Is Nothing Then
                MixedModelWorkflowPolicy.Issue(issues, "report.required", "Report options are required.")
            Else
                If Not MixedModelWorkflowPolicy.Finite(options.Alpha) OrElse options.Alpha <= 0 OrElse options.Alpha >= 1 Then MixedModelWorkflowPolicy.Issue(issues, "report.alpha", "Alpha must be finite and between 0 and 1.")
                For Each pair In New KeyValuePair(Of Type, Object)() {
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelMeansMode), options.MeansMode),
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelContrastSelection), options.Contrast),
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelContrastDirection), options.Direction),
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelReferenceGridWeighting), options.Weighting),
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelCovariateReferenceMode), options.CovariateMode),
                    New KeyValuePair(Of Type, Object)(GetType(MixedModelMultiplicityAdjustment), options.Multiplicity)}
                    If Not [Enum].IsDefined(pair.Key, pair.Value) Then MixedModelWorkflowPolicy.Issue(issues, "report.option", "Unknown report option code.")
                Next
                If options.CovariateMode = MixedModelCovariateReferenceMode.UserSpecified AndAlso Not MixedModelWorkflowPolicy.Finite(options.CovariateValue) Then MixedModelWorkflowPolicy.Issue(issues, "report.covariate", "User reference covariate must be finite.")
                If options.BaselineVisit.HasValue AndAlso visitLevels IsNot Nothing AndAlso Not visitLevels.Contains(options.BaselineVisit.Value) Then MixedModelWorkflowPolicy.Issue(issues, "report.baseline", "Selected baseline is absent from visits.")
                If options.ControlLevel.HasValue AndAlso groupLevels IsNot Nothing AndAlso Not groupLevels.Contains(options.ControlLevel.Value) Then MixedModelWorkflowPolicy.Issue(issues, "report.control", "Selected control is absent from group levels.")
                If options.Contrast = MixedModelContrastSelection.SelectedComparison AndAlso (Not options.ComparisonLevel.HasValue OrElse Not options.ControlLevel.HasValue OrElse options.ComparisonLevel = options.ControlLevel) Then MixedModelWorkflowPolicy.Issue(issues, "report.comparison", "Selected comparison requires distinct control and comparison levels.")
            End If
            Return New RegressionValidation(issues, 0, 0, {})
        End Function
    End Class
End Namespace
