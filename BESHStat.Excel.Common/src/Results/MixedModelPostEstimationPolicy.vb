Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public NotInheritable Class MixedModelPostEstimationIntent
        Private ReadOnly _rows As Integer()
        Public Sub New(kind As MixedModelWorkflowKind, names() As String, visitKey As String, visits() As Double,
                       hasGroup As Boolean, groupKey As String, groups() As Double, options As MixedModelReportOptions,
                       Optional sourceRowIds() As Integer = Nothing, Optional decision As MixedModelFitDecision = Nothing)
            Me.Kind = kind : Me.HasGroup = hasGroup : Me.Decision = decision
            Reference = New MixedModelReferenceIntent(names, visitKey, visits, If(hasGroup, groupKey, Nothing), If(hasGroup, groups, Nothing), If(options, New MixedModelReportOptions))
            _rows = If(sourceRowIds Is Nothing, Nothing, DirectCast(sourceRowIds.Clone(), Integer()))
            Dim candidate = MixedModelReportPolicy.Validate(Reference.Options,
                MixedModelPostEstimation.UniqueSortedFiniteValues(Reference.Visits), MixedModelPostEstimation.UniqueSortedFiniteValues(Reference.Groups))
            Dim issues As New List(Of RegressionDiagnostic)(candidate.Diagnostics)
            If _rows IsNot Nothing AndAlso visits IsNot Nothing AndAlso _rows.Length <> visits.Length Then MixedModelWorkflowPolicy.Issue(issues, "post.rows", "Source row sidecar length differs from the visit axis.")
            If Reference.Options.CovariateMode = MixedModelCovariateReferenceMode.UserSpecified AndAlso Reference.Options.CovariateValue <> 0 Then MixedModelWorkflowPolicy.Issue(issues, "post.covariate.zero-only", "The existing GUI projection supports continuous covariates at zero only.", False)
            Validation = New RegressionValidation(issues, 0, 0, {})
        End Sub
        Public ReadOnly Property Kind As MixedModelWorkflowKind
        Public ReadOnly Property HasGroup As Boolean
        Public ReadOnly Property Reference As MixedModelReferenceIntent
        Public ReadOnly Property Decision As MixedModelFitDecision
        ' Candidate diagnostics only; acquired Windows projection never rejects on these.
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Options As MixedModelReportOptions
            Get
                Return Reference.Options
            End Get
        End Property
        Public ReadOnly Property SourceRowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
    End Class

    Public Enum MixedModelReferenceColumnRole
        MarginalFactor
        ContinuousCovariate
    End Enum
    Public Enum MixedModelGroupingChoice
        Automatic
        None
        Explicit
    End Enum
    Public NotInheritable Class MixedModelReferenceColumn
        Private ReadOnly _values As Double()
        Public Sub New(key As String, role As MixedModelReferenceColumnRole, values() As Double)
            Me.Key = key : Me.Role = role
            _values = If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
        End Sub
        Public ReadOnly Property Key As String
        Public ReadOnly Property Role As MixedModelReferenceColumnRole
        Public ReadOnly Property Values As Double()
            Get
                Return If(_values Is Nothing, Nothing, DirectCast(_values.Clone(), Double()))
            End Get
        End Property
    End Class

    ''' <summary>Host-acquired class display values; original host value interpretation is
    ''' intentionally separate from portable table composition. No worksheet coordinates.</summary>
    Public NotInheritable Class MixedModelClassLevel
        Public Sub New(name As String, count As Integer, levels As String, Optional modelPart As String = Nothing)
            Me.Name = name : Me.Count = count : Me.Levels = levels : Me.ModelPart = modelPart
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property Count As Integer
        Public ReadOnly Property Levels As String
        Public ReadOnly Property ModelPart As String
    End Class

    Public NotInheritable Class MixedModelPostEstimationNotice
        Public Sub New(message As String, warning As Boolean)
            Me.Message = message : Me.IsWarning = warning
        End Sub
        Public ReadOnly Property Message As String
        Public ReadOnly Property IsWarning As Boolean
    End Class

    ''' <summary>Materialized, detached tables only; never holds a fit, X/Z, reference rows,
    ''' factories, exception objects or inference workspace. Core additional tables are not
    ''' mutated by projection. Windows owns attachment at its existing post-fit seam.</summary>
    Public NotInheritable Class MixedModelSupplementalReport
        Friend Sub New(sections As IEnumerable(Of MixedModelSupplementalSection), tables As IEnumerable(Of ResultTable), warnings As IEnumerable(Of AnalysisWarning))
            Me.Sections = New List(Of MixedModelSupplementalSection)(sections).AsReadOnly()
            Document = New AnalysisResultDocument("mixed.supplemental", tables.Select(Function(t) t.ToOutputModel()),
                Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, warnings)
        End Sub
        Public ReadOnly Property Sections As IReadOnlyList(Of MixedModelSupplementalSection)
        Public ReadOnly Property Document As AnalysisResultDocument
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class

    Public NotInheritable Class MixedModelPostEstimationPolicy
        Private Sub New()
        End Sub
        Public Const ComparisonUnavailable As String = "Selected comparison could not be resolved. Choose a comparison/treatment level different from the control/reference level."
        Private Shared Sub Log(sink As Action(Of MixedModelPostEstimationNotice), message As String, Optional warning As Boolean = False)
            If sink IsNot Nothing Then sink(New MixedModelPostEstimationNotice(message, warning))
        End Sub
        Public Shared Function EmptyReport(Optional warning As AnalysisWarning = Nothing) As MixedModelSupplementalReport
            Return New MixedModelSupplementalReport({}, {}, If(warning Is Nothing, Array.Empty(Of AnalysisWarning)(), {warning}))
        End Function
        Private Shared Sub OptionalTable(section As MixedModelSupplementalSection, label As String, factory As Func(Of ResultTable),
                                         sections As List(Of MixedModelSupplementalSection), tables As List(Of ResultTable), warnings As List(Of AnalysisWarning),
                                         sink As Action(Of MixedModelPostEstimationNotice))
            If factory Is Nothing Then Return
            Try
                Dim table = factory()
                If table Is Nothing Then
                    Log(sink, "MMRM optional table not created: " & label)
                    If warnings.Count < MixedModelWorkflowPolicy.MaximumDiagnostics Then warnings.Add(New AnalysisWarning("mixed.post.table.unavailable", "Optional table was not created: " & label, section.ToString()))
                    Return
                End If
                sections.Add(section) : tables.Add(table)
                Log(sink, "MMRM optional table added: " & label)
            Catch ex As Exception
                Log(sink, "MMRM optional table failed: " & label & ": " & ex.ToString(), True)
                If warnings.Count < MixedModelWorkflowPolicy.MaximumDiagnostics Then warnings.Add(New AnalysisWarning("mixed.post.table.failed", "Optional table could not be created: " & label, section.ToString()))
            End Try
        End Sub

        Public Shared Function ProjectMMRM(result As MixedModelResult, x(,) As Double, intent As MixedModelPostEstimationIntent,
                                            Optional classFactory As Func(Of ResultTable) = Nothing,
                                            Optional referenceFactory As Func(Of MixedModelSupplementalReport) = Nothing,
                                            Optional sink As Action(Of MixedModelPostEstimationNotice) = Nothing) As MixedModelSupplementalReport
            If intent Is Nothing OrElse result Is Nothing OrElse x Is Nothing Then Return EmptyReport(New AnalysisWarning("mixed.post.inputs.unavailable", "Post-estimation inputs are unavailable."))
            If intent.Kind <> MixedModelWorkflowKind.MMRM Then Return EmptyReport(New AnalysisWarning("mixed.post.kind.unsupported", "The observed MMRM orchestration is not an LMM GUI feature."))
            Dim visit = intent.Reference.Visits, groups = intent.Reference.Groups, o = intent.Options
            If visit Is Nothing OrElse result.Beta Is Nothing OrElse result.Beta.Length = 0 OrElse result.VarBeta Is Nothing OrElse x.GetLength(0) <> visit.Length Then
                Return EmptyReport(New AnalysisWarning("mixed.post.inputs.unavailable", "Post-estimation inputs are unavailable."))
            End If
            Dim sections As New List(Of MixedModelSupplementalSection), tables As New List(Of ResultTable), warnings As New List(Of AnalysisWarning)
            Dim plan = MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, o, intent.HasGroup)
            If plan.Includes(MixedModelSupplementalSection.ClassInformation) Then OptionalTable(MixedModelSupplementalSection.ClassInformation, "Class Level Information", classFactory, sections, tables, warnings, sink)
            If Not o.EstimatedMeans Then
                Log(sink, "AppendMMRMEstimatedMeanTables finished after class-level output because estimated means are disabled.")
                Return New MixedModelSupplementalReport(sections, tables, warnings)
            End If
            Dim baseline = If(o.BaselineVisit.HasValue, o.BaselineVisit.Value, ResolveLevel(visit, o.BaselineSelection, "(Smallest)"))
            Dim control = If(intent.HasGroup, If(o.ControlLevel.HasValue, o.ControlLevel.Value, ResolveLevel(groups, o.ControlSelection, "(First)")), Double.NaN)
            Dim mode = ContrastText(o), direction = DirectionText(o)
            If o.MeansMode = MixedModelMeansMode.ReferenceGrid Then
                Try
                    Dim report = If(referenceFactory IsNot Nothing, referenceFactory(), ProjectReference(result, intent.Reference.CreateCoreSpec(), intent, sink))
                    If report IsNot Nothing Then
                        sections.AddRange(report.Sections) : tables.AddRange(report.LegacyTables()) : warnings.AddRange(report.Document.Warnings)
                    End If
                    Log(sink, "AppendMMRMEstimatedMeanTables used reference-grid mode; observed-design-grid tables skipped.")
                Catch ex As Exception
                    Log(sink, "AppendMMRMEstimatedMeanTables failed: " & ex.ToString(), True)
                    warnings.Add(New AnalysisWarning("mixed.post.reference.failed", "Reference-grid specification or row construction failed."))
                End Try
                Return New MixedModelSupplementalReport(sections, tables, warnings)
            End If
            Log(sink, "AppendMMRMEstimatedMeanTables options. baseline=" & baseline.ToString(CultureInfo.InvariantCulture) &
                "; contrastMode='" & mode & "'; direction='" & direction & "'; controlLevel=" & control.ToString(CultureInfo.InvariantCulture))
            OptionalTable(MixedModelSupplementalSection.MeansByVisit, "Estimated marginal means by visit", Function() MMRMPostEstimation.BuildEstimatedMeansByVisitTable(result, x, visit, o.Alpha), sections, tables, warnings, sink)
            If plan.Includes(MixedModelSupplementalSection.ChangeByVisit) Then OptionalTable(MixedModelSupplementalSection.ChangeByVisit, "MMRM change from baseline by visit", Function() MMRMPostEstimation.BuildChangeFromBaselineTableControlled(result, x, visit, baseline, o.Alpha), sections, tables, warnings, sink)
            If intent.HasGroup Then
                OptionalTable(MixedModelSupplementalSection.MeansByVisitAndGroup, "Estimated marginal means by visit and group", Function() MMRMPostEstimation.BuildEstimatedMeansByVisitAndGroupTable(result, x, visit, groups, intent.Reference.GroupKey, o.Alpha), sections, tables, warnings, sink)
                If plan.Includes(MixedModelSupplementalSection.GroupDifferences) Then OptionalTable(MixedModelSupplementalSection.GroupDifferences, "MMRM group differences by visit", Function() BuildObservedContrast(result, x, visit, groups, intent.Reference.GroupKey, baseline, control, o, False), sections, tables, warnings, sink)
                If plan.Includes(MixedModelSupplementalSection.ChangeByGroup) Then
                    OptionalTable(MixedModelSupplementalSection.ChangeByGroup, "MMRM change from baseline by visit and group", Function() MMRMPostEstimation.BuildChangeFromBaselineByGroupTableControlled(result, x, visit, groups, intent.Reference.GroupKey, baseline, o.Alpha), sections, tables, warnings, sink)
                    If plan.Includes(MixedModelSupplementalSection.DifferenceInChange) Then OptionalTable(MixedModelSupplementalSection.DifferenceInChange, "MMRM difference in change from baseline", Function() BuildObservedContrast(result, x, visit, groups, intent.Reference.GroupKey, baseline, control, o, True), sections, tables, warnings, sink)
                End If
            Else
                Log(sink, "AppendMMRMEstimatedMeanTables: no grouping factor resolved; group-specific LS-means/contrast tables skipped.")
            End If
            Return New MixedModelSupplementalReport(sections, tables, warnings)
        End Function

        Public Shared Function ProjectReference(result As MixedModelResult, spec As MixedModelReferenceGridSpec, intent As MixedModelPostEstimationIntent,
                                                Optional sink As Action(Of MixedModelPostEstimationNotice) = Nothing) As MixedModelSupplementalReport
            If result Is Nothing OrElse intent Is Nothing Then Return EmptyReport(New AnalysisWarning("mixed.post.inputs.unavailable", "Post-estimation inputs are unavailable."))
            If spec Is Nothing Then
                Log(sink, "Reference-grid LS-means skipped: specification could not be built.", True)
                Return EmptyReport(New AnalysisWarning("mixed.post.reference.unavailable", "Reference-grid specification could not be built."))
            End If
            Dim rows = MixedModelReferenceGridService.BuildReferenceGridRows(spec)
            If rows Is Nothing OrElse rows.Count = 0 Then
                Log(sink, "Reference-grid LS-means skipped: no reference-grid rows were created.", True)
                Return EmptyReport(New AnalysisWarning("mixed.post.reference.empty", "No reference-grid rows were created."))
            End If
            Dim sections As New List(Of MixedModelSupplementalSection), tables As New List(Of ResultTable), warnings As New List(Of AnalysisWarning)
            OptionalTable(MixedModelSupplementalSection.ReferenceMeans, "Reference-grid estimated marginal means", Function() MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid estimated marginal means", rows, result, spec), sections, tables, warnings, sink)
            Dim o = intent.Options
            If Not o.ControlLevel.HasValue Then o.ControlLevel = ResolveLevel(intent.Reference.Groups, o.ControlSelection, "(First)")
            If intent.HasGroup AndAlso Not String.IsNullOrWhiteSpace(intent.Reference.GroupKey) AndAlso o.Contrast <> MixedModelContrastSelection.None Then
                OptionalTable(MixedModelSupplementalSection.ReferenceGroupContrasts, "Reference-grid group contrasts", Function() BuildReferenceContrasts(result, rows, spec, intent.Reference.GroupKey, o), sections, tables, warnings, sink)
            End If
            Return New MixedModelSupplementalReport(sections, tables, warnings)
        End Function

        Public Shared Function ContrastText(o As MixedModelReportOptions) As String
            If o.RequestedContrastText IsNot Nothing Then Return o.RequestedContrastText
            Select Case o.Contrast
                Case MixedModelContrastSelection.None : Return "None"
                Case MixedModelContrastSelection.EachVsControl : Return "Each group vs control"
                Case MixedModelContrastSelection.SelectedComparison : Return "Selected comparison only"
                Case Else : Return "Pairwise among group levels"
            End Select
        End Function
        Public Shared Function DirectionText(o As MixedModelReportOptions) As String
            If o.RequestedDirectionText IsNot Nothing Then Return o.RequestedDirectionText
            Select Case o.Direction
                Case MixedModelContrastDirection.TreatmentMinusControl : Return "Treatment - control"
                Case MixedModelContrastDirection.ControlMinusTreatment : Return "Control - treatment"
                Case Else : Return "Higher level - lower level"
            End Select
        End Function
        Public Shared Function ResolveLevel(values() As Double, selection As String, placeholder As String) As Double
            Dim levels = MixedModelPostEstimation.UniqueSortedFiniteValues(values)
            If levels Is Nothing OrElse levels.Length = 0 Then Return Double.NaN
            Dim text = If(String.IsNullOrWhiteSpace(selection), placeholder, selection)
            If String.Equals(text, placeholder, StringComparison.OrdinalIgnoreCase) Then Return levels(0)
            For Each value In levels
                If String.Equals(MixedModelPostEstimation.FormatProfileValue(value), text, StringComparison.OrdinalIgnoreCase) Then Return value
            Next
            Dim parsed As Double
            If Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then Return parsed
            If Double.TryParse(text, parsed) Then Return parsed
            Return levels(0)
        End Function
        Public Shared Function ResolveComparison(levels() As Double, control As Double, selection As String, ByRef comparison As Double) As Boolean
            comparison = Double.NaN
            If levels Is Nothing OrElse levels.Length = 0 Then Return False
            Dim text = If(selection, "(Auto)").Trim()
            If text.Length = 0 OrElse text.Equals("(Auto)", StringComparison.OrdinalIgnoreCase) Then
                For Each value In levels
                    If Not MixedModelPostEstimation.NearlyEqual(value, control) Then comparison = value : Return True
                Next
                Return False
            End If
            Dim parsed As Double
            If Double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, parsed) OrElse Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then
                If MixedModelPostEstimation.NearlyEqual(parsed, control) Then Return False
                comparison = parsed : Return True
            End If
            For Each value In levels
                If String.Equals(MixedModelPostEstimation.FormatProfileValue(value), text, StringComparison.OrdinalIgnoreCase) Then
                    If MixedModelPostEstimation.NearlyEqual(value, control) Then Return False
                    comparison = value : Return True
                End If
            Next
            Return False
        End Function
        Private Shared Function ResolveContrastComparison(levels() As Double, control As Double, o As MixedModelReportOptions) As Double
            Dim value = If(o.ComparisonLevel.HasValue, o.ComparisonLevel.Value, Double.NaN)
            If String.Equals(ContrastText(o), "Selected comparison only", StringComparison.OrdinalIgnoreCase) AndAlso
                Not ResolveComparison(levels, control, If(o.ComparisonLevel.HasValue, value.ToString("R", CultureInfo.InvariantCulture), o.ComparisonSelection), value) Then Throw New ApplicationException(ComparisonUnavailable)
            Return value
        End Function
        Public Shared Function BuildObservedContrast(result As MixedModelResult, x(,) As Double, visit() As Double, groups() As Double, groupKey As String,
                                                     baseline As Double, control As Double, o As MixedModelReportOptions, differenceInChange As Boolean) As ResultTable
            Dim comparison = ResolveContrastComparison(MixedModelPostEstimation.UniqueSortedFiniteValues(groups), control, o)
            If differenceInChange Then Return MMRMPostEstimation.BuildDifferenceInChangeFromBaselineTableControlled(result, x, visit, groups, groupKey, baseline, o.Alpha, ContrastText(o), control, comparison, DirectionText(o))
            Return MMRMPostEstimation.BuildVisitGroupDifferencesTableControlled(result, x, visit, groups, groupKey, o.Alpha, ContrastText(o), control, comparison, DirectionText(o))
        End Function
        Public Shared Function BuildReferenceContrasts(result As MixedModelResult, rows As List(Of MixedModelReferenceGridRow), spec As MixedModelReferenceGridSpec, groupKey As String, o As MixedModelReportOptions) As ResultTable
            If rows Is Nothing OrElse rows.Count = 0 Then Return Nothing
            Dim name = MixedModelReferenceGridService.NormalizeProfileName(groupKey), mode = ContrastText(o)
            If String.Equals(mode, "Pairwise among group levels", StringComparison.OrdinalIgnoreCase) Then Return MixedModelReferenceGridService.BuildPairwiseContrastsByFactor(rows, result, spec, name, "Reference-grid group contrasts")
            Dim control = If(o.ControlLevel.HasValue, o.ControlLevel.Value, Double.NaN)
            Dim comparison = ResolveContrastComparison(MixedModelReferenceGridService.GetLevelsFromRows(rows, name), control, o)
            Return MixedModelReferenceGridService.BuildContrastsAgainstControlByFactor(rows, result, spec, name, control, comparison,
                String.Equals(mode, "Selected comparison only", StringComparison.OrdinalIgnoreCase), DirectionText(o), "Treatment - control", "Control - treatment", "Reference-grid group contrasts")
        End Function

        Public Shared Function ReferenceColumnIncluded(key As String, visitProfile As String, groupProfile As String) As Boolean
            Dim name = MixedModelReferenceGridService.NormalizeProfileName(key)
            Return Not String.Equals(name, visitProfile, StringComparison.OrdinalIgnoreCase) AndAlso
                (String.IsNullOrWhiteSpace(groupProfile) OrElse Not String.Equals(name, groupProfile, StringComparison.OrdinalIgnoreCase))
        End Function
        Public Shared Sub AddReferenceColumn(spec As MixedModelReferenceGridSpec, column As MixedModelReferenceColumn, visitProfile As String, groupProfile As String, o As MixedModelReportOptions)
            If Not ReferenceColumnIncluded(column.Key, visitProfile, groupProfile) Then Return
            Dim name = MixedModelReferenceGridService.NormalizeProfileName(column.Key), values = column.Values
            If column.Role = MixedModelReferenceColumnRole.MarginalFactor Then
                Dim levels = MixedModelPostEstimation.UniqueSortedFiniteValues(values)
                If levels IsNot Nothing AndAlso levels.Length > 1 Then spec.AddMarginalFactor(name, levels, values)
            ElseIf o.CovariateMode = MixedModelCovariateReferenceMode.ObservedMean Then
                spec.AddCovariateMean(name, values)
            Else
                spec.AddCovariateValue(name, values, 0.0) ' Shipping GUI supports mean or zero only.
            End If
        End Sub
        Public Shared Function ClassInformation(kind As MixedModelWorkflowKind, rows As IEnumerable(Of MixedModelClassLevel)) As ResultTable
            If rows Is Nothing Then Return Nothing
            Dim materialized = rows.Where(Function(row) row IsNot Nothing).ToArray()
            If materialized.Length = 0 Then Return Nothing
            Dim mmrm = kind = MixedModelWorkflowKind.MMRM, body(materialized.Length - 1, If(mmrm, 2, 4)) As Object
            For i = 0 To materialized.Length - 1
                Dim row = materialized(i)
                If mmrm Then
                    body(i, 0) = row.Name : body(i, 1) = row.Count : body(i, 2) = row.Levels
                Else
                    body(i, 0) = row.ModelPart : body(i, 1) = row.Name : body(i, 2) = "Categorical main effect" : body(i, 3) = row.Levels : body(i, 4) = row.Count
                End If
            Next
            Dim table As New ResultTable
            table.AddTitle(If(mmrm, "Class Level Information", "Class level information")) : table.SetBody(body)
            table.AddHeaderTopRow(If(mmrm, New String() {"Class", "Levels", "Values"}, New String() {"Model part", "Variable", "Term kind", "Levels", "No. levels"}))
            table.AddFootnote(If(mmrm, "Class levels are shown for the subject ID and, when available, the grouping factor used by MMRM LS-means/contrast output.", "Levels are observed numeric/coded values in the cleaned analysis data for variables used as categorical effects."))
            Return table
        End Function
        Public Shared Function LmmClassReport(rows As IEnumerable(Of MixedModelClassLevel)) As MixedModelSupplementalReport
            Dim table = ClassInformation(MixedModelWorkflowKind.LMM, rows)
            If table Is Nothing Then Return EmptyReport()
            Return New MixedModelSupplementalReport({MixedModelSupplementalSection.ClassInformation}, {table}, {})
        End Function
        Public Shared Function BetterGroupingCandidate(levelCount As Integer, bestCount As Integer) As Boolean
            Return HasGroupingLevels(levelCount) AndAlso levelCount < bestCount
        End Function
        Public Shared Function HasGroupingLevels(levelCount As Integer) As Boolean
            Return levelCount >= 2
        End Function
        Public Shared Function GroupingChoice(text As String) As MixedModelGroupingChoice
            If String.Equals(text, "(None)", StringComparison.OrdinalIgnoreCase) Then Return MixedModelGroupingChoice.None
            If String.Equals(text, "(Auto)", StringComparison.OrdinalIgnoreCase) Then Return MixedModelGroupingChoice.Automatic
            Return MixedModelGroupingChoice.Explicit
        End Function
        Public Shared Function MainEffectKeys(selected As IEnumerable(Of String), specs As IDictionary(Of String, TermSpec), scale As PredictorScale,
                                               Optional excludeBase As String = Nothing, Optional singleBaseOnly As Boolean = True) As List(Of String)
            Dim keys As New List(Of String)
            If selected Is Nothing OrElse specs Is Nothing Then Return keys
            For Each key In selected
                If (singleBaseOnly AndAlso String.IsNullOrWhiteSpace(key)) OrElse Not specs.ContainsKey(key) Then Continue For
                Dim spec = specs(key)
                If spec Is Nothing OrElse spec.BaseVarKeys Is Nothing OrElse Not String.Equals(spec.Kind, "MainEffect", StringComparison.OrdinalIgnoreCase) OrElse spec.Scale <> scale Then Continue For
                If singleBaseOnly AndAlso spec.BaseVarKeys.Count <> 1 Then Continue For
                For Each baseKey In spec.BaseVarKeys
                    If Not String.IsNullOrWhiteSpace(excludeBase) AndAlso String.Equals(RegressionDesignCore.GetCoefBaseName(baseKey), excludeBase, StringComparison.OrdinalIgnoreCase) Then Continue For
                    If Not keys.Contains(baseKey) Then keys.Add(baseKey)
                Next
            Next
            Return keys
        End Function
    End Class
End Namespace
