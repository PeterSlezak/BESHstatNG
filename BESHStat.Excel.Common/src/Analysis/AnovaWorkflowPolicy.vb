Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading

Namespace ExcelCommon
    Public Enum AnovaWorkflowStage
        PrimaryTables
        Homogeneity
        Descriptive
        WindowsBoxSummary
    End Enum

    ''' <summary>Immutable view-independent snapshot. Existing analysis options/IDs remain unchanged.</summary>
    Public NotInheritable Class AnovaGroupedWorkflowOptions
        Public Sub New(kind As SpreadsheetAnalysisKind, Optional alpha As Double = 0.05,
                       Optional welch As Boolean = False, Optional lsd As Boolean = False,
                       Optional bonferroni As Boolean = False, Optional tukey As Boolean = False,
                       Optional gamesHowell As Boolean = False, Optional dunn As Boolean = True,
                       Optional homogeneity As HomogeneityMethods = HomogeneityMethods.None,
                       Optional descriptive As Boolean = False, Optional boxSummary As Boolean = False)
            Me.Kind = kind : Me.Alpha = alpha : Me.Welch = welch : Me.Lsd = lsd
            Me.Bonferroni = bonferroni : Me.Tukey = tukey : Me.GamesHowell = gamesHowell
            Me.Dunn = dunn : Me.Homogeneity = homogeneity : Me.Descriptive = descriptive : Me.BoxSummary = boxSummary
        End Sub
        Public ReadOnly Property Kind As SpreadsheetAnalysisKind
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property Welch As Boolean
        Public ReadOnly Property Lsd As Boolean
        Public ReadOnly Property Bonferroni As Boolean
        Public ReadOnly Property Tukey As Boolean
        Public ReadOnly Property GamesHowell As Boolean
        Public ReadOnly Property Dunn As Boolean
        Public ReadOnly Property Homogeneity As HomogeneityMethods
        Public ReadOnly Property Descriptive As Boolean
        Public ReadOnly Property BoxSummary As Boolean
        Public Function ToAnalysisOptions() As SpreadsheetAnalysisOptions
            If Kind = SpreadsheetAnalysisKind.OneWayAnova Then
                Return New OneWayAnovaAnalysisOptions With {.Alpha = Alpha, .IncludeWelch = Welch, .IncludeFisherLsd = Lsd,
                    .IncludeBonferroni = Bonferroni, .IncludeTukeyKramer = Tukey, .IncludeGamesHowell = GamesHowell}
            End If
            If Kind = SpreadsheetAnalysisKind.KruskalWallis Then Return New KruskalWallisAnalysisOptions With {.Alpha = Alpha, .IncludeDunnComparisons = Dunn}
            Throw New ArgumentOutOfRangeException(NameOf(Kind))
        End Function
    End Class

    Public NotInheritable Class AnovaWorkflowPlan
        Friend Sub New(options As AnovaGroupedWorkflowOptions)
            If options Is Nothing OrElse (options.Kind <> SpreadsheetAnalysisKind.OneWayAnova AndAlso options.Kind <> SpreadsheetAnalysisKind.KruskalWallis) Then Throw New ArgumentException("Unsupported grouped workflow kind.")
            Dim steps As New List(Of AnovaWorkflowStage) From {AnovaWorkflowStage.PrimaryTables}
            If options.Kind = SpreadsheetAnalysisKind.OneWayAnova Then
                If options.Homogeneity <> HomogeneityMethods.None Then steps.Add(AnovaWorkflowStage.Homogeneity)
                If options.Descriptive Then steps.Add(AnovaWorkflowStage.Descriptive)
            Else
                If options.Descriptive Then steps.Add(AnovaWorkflowStage.Descriptive)
                If options.Homogeneity <> HomogeneityMethods.None Then steps.Add(AnovaWorkflowStage.Homogeneity)
            End If
            If options.BoxSummary Then steps.Add(AnovaWorkflowStage.WindowsBoxSummary)
            Stages = steps.AsReadOnly()
        End Sub
        Public ReadOnly Property Stages As IReadOnlyList(Of AnovaWorkflowStage)
        Public ReadOnly Property BlankRowsBetweenTables As Integer = 1
        Public ReadOnly Property BoxChartBeforeComparisonCharts As Boolean = True
    End Class

    Public NotInheritable Class AnovaGroupedWorkflowResult
        Private ReadOnly _plots As parametric.MultipleComparisonPlotData()
        Private ReadOnly _failure As Exception
        Friend Sub New(input As AssumptionReportInput, options As AnovaGroupedWorkflowOptions, analysis As SpreadsheetAnalysisResult,
                       issues As IEnumerable(Of SpreadsheetValidationIssue), plots As IEnumerable(Of parametric.MultipleComparisonPlotData), failure As Exception)
            Me.Options = options
            Me.Input = input
            If options IsNot Nothing AndAlso (options.Kind = SpreadsheetAnalysisKind.OneWayAnova OrElse options.Kind = SpreadsheetAnalysisKind.KruskalWallis) Then Me.Plan = New AnovaWorkflowPlan(options)
            Me.LegacyAnalysis = analysis
            Validation = New SpreadsheetValidationReport(issues)
            _plots = plots.Select(AddressOf CopyPlot).ToArray()
            _failure = failure
        End Sub
        Public ReadOnly Property Options As AnovaGroupedWorkflowOptions
        Public ReadOnly Property Input As AssumptionReportInput
        Public ReadOnly Property Plan As AnovaWorkflowPlan
        ''' <summary>Existing mutable legacy result, for the established shared Windows writer only.
        ''' New consumers use its immutable Document and this immutable Plan.</summary>
        Public ReadOnly Property LegacyAnalysis As SpreadsheetAnalysisResult
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Success As Boolean
            Get
                Return Not Validation.HasErrors
            End Get
        End Property
        Public ReadOnly Property ComparisonPlots As IReadOnlyList(Of parametric.MultipleComparisonPlotData)
            Get
                Return Array.AsReadOnly(_plots.Select(AddressOf CopyPlot).ToArray())
            End Get
        End Property
        Private Shared Function CopyPlot(source As parametric.MultipleComparisonPlotData) As parametric.MultipleComparisonPlotData
            Return New parametric.MultipleComparisonPlotData With {.ProcedureName = source.ProcedureName, .Alpha = source.Alpha,
                .Labels = DirectCast(source.Labels.Clone(), String()), .Estimates = DirectCast(source.Estimates.Clone(), Double()),
                .LowerLimits = DirectCast(source.LowerLimits.Clone(), Double()), .UpperLimits = DirectCast(source.UpperLimits.Clone(), Double())}
        End Function
        Public Sub ThrowIfFailed()
            If Success Then Return
            If _failure IsNot Nothing Then Throw _failure
            Throw New SpreadsheetInputValidationException(Validation)
        End Sub
    End Class

    ''' <summary>Immutable capture of existing Core table output, including p-value columns,
    ''' cells, headers and CI footnotes. No reformatting/reparsing of numerical CI strings.</summary>
    Public NotInheritable Class AnovaReportTable
        Private ReadOnly _artifact As AnalysisTableArtifact
        Friend Sub New(index As Integer, table As ResultTable)
            If table.bLeftHeaderAdjustUp Then Throw New ArgumentException("Unexpected ANOVA table header alignment.")
            _artifact = New AnalysisTableArtifact(index, table.ToOutputModel())
        End Sub
        Public ReadOnly Property Artifact As AnalysisTableArtifact
            Get
                Return _artifact
            End Get
        End Property
        Public Function ToLegacyTable() As ResultTable
            Dim model = _artifact.Model, values = model.Values
            Dim result As New ResultTable
            Dim bodyRows = model.RowCount - model.TitleRows - model.HeaderTopRows - model.FooterRows
            Dim bodyColumns = model.ColumnCount - model.HeaderLeftColumns
            Dim body(bodyRows - 1, bodyColumns - 1) As Object
            For r = 0 To bodyRows - 1
                For c = 0 To bodyColumns - 1
                    body(r, c) = values(model.TitleRows + model.HeaderTopRows + r, model.HeaderLeftColumns + c)
                Next
            Next
            result.SetBody(body)
            For r = 0 To model.TitleRows - 1
                result.AddTitle(TryCast(values(r, 0), String))
            Next
            For r = 0 To model.HeaderTopRows - 1
                Dim header(model.ColumnCount - 1) As String
                For c = 0 To model.ColumnCount - 1
                    header(c) = TryCast(values(model.TitleRows + r, c), String)
                Next
                result.AddHeaderTopRow(header)
            Next
            For c = 0 To model.HeaderLeftColumns - 1
                Dim left(bodyRows - 1) As String
                For r = 0 To bodyRows - 1
                    left(r) = TryCast(values(model.TitleRows + model.HeaderTopRows + r, c), String)
                Next
                result.AddHeaderLeftRow(left)
            Next
            For r = model.RowCount - model.FooterRows To model.RowCount - 1
                result.AddFootnote(TryCast(values(r, 0), String))
            Next
            For Each col In model.PvalueColumns
                result.AddPvalueToFormat(col)
            Next
            For Each cell In model.PvalueCells
                result.AddPvalueCellToFormat(cell.BodyRow, cell.BodyColumn)
            Next
            Return result
        End Function
    End Class

    Public NotInheritable Class RepeatedAnovaWorkflowOptions
        Public Sub New(Optional alpha As Double = 0.05, Optional greenhouseGeisser As Boolean = False,
                       Optional huynhFeldt As Boolean = False, Optional tukey As Boolean = False,
                       Optional mauchly As Boolean = False, Optional descriptive As Boolean = False, Optional boxSummary As Boolean = False)
            Me.Alpha = alpha : Me.GreenhouseGeisser = greenhouseGeisser : Me.HuynhFeldt = huynhFeldt
            Me.Tukey = tukey : Me.Mauchly = mauchly : Me.Descriptive = descriptive : Me.BoxSummary = boxSummary
        End Sub
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property GreenhouseGeisser As Boolean
        Public ReadOnly Property HuynhFeldt As Boolean
        Public ReadOnly Property Tukey As Boolean
        Public ReadOnly Property Mauchly As Boolean
        Public ReadOnly Property Descriptive As Boolean
        Public ReadOnly Property BoxSummary As Boolean
    End Class

    Public NotInheritable Class AnovaModelReportResult
        Private ReadOnly _failure As Exception
        Private ReadOnly _matrix As Double(,)
        Private ReadOnly _objects As Object(,)
        Private ReadOnly _banner As Object(,)
        Friend Sub New(tables As IEnumerable(Of ResultTable), issues As IEnumerable(Of SpreadsheetValidationIssue), calls As IEnumerable(Of String),
                       matrix As Double(,), objects As Object(,), blocks As SpreadsheetNumericBlocks, nested As SpreadsheetNestedInput,
                       balanced As Boolean?, failure As Exception, boxSummary As Boolean,
                       Optional originalNested As SpreadsheetNestedInput = Nothing)
            Me.Tables = tables.Select(Function(t, index) New AnovaReportTable(index, t)).ToList().AsReadOnly()
            Validation = New SpreadsheetValidationReport(issues)
            Me.Calls = calls.ToList().AsReadOnly()
            _matrix = If(matrix Is Nothing, Nothing, DirectCast(matrix.Clone(), Double(,)))
            _objects = If(objects Is Nothing, Nothing, DirectCast(objects.Clone(), Object(,)))
            Me.Blocks = blocks : Me.Nested = nested : Me.BalancedDesign = balanced
            Me.OriginalNestedInput = If(originalNested, nested)
            Me.BoxSummary = boxSummary : _failure = failure
            If balanced.HasValue Then _banner = New Object(,) {{"Two-Way Nested ANOVA"}, {"Balanced design = " & balanced.Value.ToString()}}
        End Sub
        Public ReadOnly Property Tables As IReadOnlyList(Of AnovaReportTable)
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Calls As IReadOnlyList(Of String)
        Public ReadOnly Property Blocks As SpreadsheetNumericBlocks
        Public ReadOnly Property Nested As SpreadsheetNestedInput
        ''' <summary>Original captured values before legacy host coercion, when supplied.
        ''' Nested carries actual Core-input values; both retain the same real coordinates.</summary>
        Public ReadOnly Property OriginalNestedInput As SpreadsheetNestedInput
        Public ReadOnly Property BalancedDesign As Boolean?
        Public ReadOnly Property BoxSummary As Boolean
        Public ReadOnly Property LegacyWorkingMatrix As Double(,)
            Get
                Return If(_matrix Is Nothing, Nothing, DirectCast(_matrix.Clone(), Double(,)))
            End Get
        End Property
        ''' <summary>Core sorts this scratch. Never assign original source rows to it.</summary>
        Public ReadOnly Property LegacyWorkingObjects As Object(,)
            Get
                Return If(_objects Is Nothing, Nothing, DirectCast(_objects.Clone(), Object(,)))
            End Get
        End Property
        Public ReadOnly Property Banner As Object(,)
            Get
                Return If(_banner Is Nothing, Nothing, DirectCast(_banner.Clone(), Object(,)))
            End Get
        End Property
        Public ReadOnly Property Separators As List(Of Object(,))
            Get
                Return New List(Of Object(,)) From {New Object(,) {{"", ""}, {"Satterthwaite approximation", ""}}}
            End Get
        End Property
        Public ReadOnly Property Success As Boolean
            Get
                Return Not Validation.HasErrors
            End Get
        End Property
        Public Function ToLegacyTables() As List(Of ResultTable)
            ThrowIfFailed()
            Return Tables.Select(Function(t) t.ToLegacyTable()).ToList()
        End Function
        Public Sub ThrowIfFailed()
            If Success Then Return
            If _failure IsNot Nothing Then Throw _failure
            Throw New SpreadsheetInputValidationException(Validation)
        End Sub
    End Class

    ''' <summary>Workflow/composition only. Existing shared handlers and Core engines remain
    ''' the single numerical implementations. No host state, acquisition, charts or writes.</summary>
    Public NotInheritable Class AnovaWorkflowPolicy
        Private Sub New()
        End Sub
        Private Shared Sub Issue(issues As List(Of SpreadsheetValidationIssue), code As String, message As String, field As String,
                                 Optional severity As SpreadsheetValidationSeverity = SpreadsheetValidationSeverity.Error)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, field))
        End Sub
        Public Shared Function RunGrouped(input As AssumptionReportInput, options As AnovaGroupedWorkflowOptions,
                                          Optional cancellation As CancellationToken = Nothing) As AnovaGroupedWorkflowResult
            Dim issues As New List(Of SpreadsheetValidationIssue), plots As New List(Of parametric.MultipleComparisonPlotData)
            Dim result As SpreadsheetAnalysisResult = Nothing, failure As Exception = Nothing
            Try
                cancellation.ThrowIfCancellationRequested()
                If input Is Nothing OrElse options Is Nothing Then Throw New ArgumentException("Named groups and explicit workflow options are required.")
                Dim groups = input.Groups, names = input.Names
                If groups Is Nothing OrElse names Is Nothing OrElse groups.Length < 2 OrElse groups.Length <> names.Length OrElse
                   groups.Any(Function(g) g Is Nothing OrElse g.Length = 0 OrElse g.Any(Function(v) Double.IsNaN(v) OrElse Double.IsInfinity(v))) Then Throw New ArgumentException("At least two nonempty finite groups and matching names are required.")
                If (CInt(options.Homogeneity) And Not CInt(HomogeneityMethods.All)) <> 0 Then Throw New ArgumentException("Unsupported homogeneity options.")
                If input.Sources.Count = 0 Then Issue(issues, "ANOVA_SOURCE_UNAVAILABLE", "No physical observation mapping was supplied; none is invented.", "sources", SpreadsheetValidationSeverity.Information)
                Dim seenSources As New HashSet(Of Tuple(Of Integer, Integer))
                For Each source In input.Sources
                    If source Is Nothing OrElse source.GroupIndex < 0 OrElse source.GroupIndex >= groups.Length OrElse
                       source.ObservationIndex < 0 OrElse source.ObservationIndex >= groups(source.GroupIndex).Length OrElse
                       source.SelectedRow < 0 OrElse source.SelectedColumn < 0 OrElse source.WorksheetRow < 0 OrElse source.WorksheetColumn < 0 Then Throw New ArgumentException("Original observation source mapping is invalid.")
                    If Not seenSources.Add(Tuple.Create(source.GroupIndex, source.ObservationIndex)) Then Throw New ArgumentException("An original observation has ambiguous source mappings.")
                Next
                'Preserve existing invalid-alpha normalization AND warning policy in the unchanged dispatcher.
                result = SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(options.Kind, New GroupedNumericData(names, groups), options.ToAnalysisOptions()))
                Dim detail = TryCast(result.Detail, OneWayAnovaApplicationResult)
                If detail IsNot Nothing Then
                    If options.Lsd AndAlso detail.FisherLsdPlotData IsNot Nothing Then plots.Add(detail.FisherLsdPlotData)
                    If options.Bonferroni AndAlso detail.BonferroniPlotData IsNot Nothing Then plots.Add(detail.BonferroniPlotData)
                    If options.Tukey AndAlso detail.TukeyKramerPlotData IsNot Nothing Then plots.Add(detail.TukeyKramerPlotData)
                    If options.GamesHowell AndAlso detail.GamesHowellPlotData IsNot Nothing Then plots.Add(detail.GamesHowellPlotData)
                End If
                For Each warning In result.Document.Warnings
                    Issue(issues, warning.Code, warning.Message, warning.FieldPath, SpreadsheetValidationSeverity.Warning)
                Next
                ObserveUndefined(result.Tables, issues)
                cancellation.ThrowIfCancellationRequested()
            Catch ex As Exception
                failure = ex : result = Nothing : plots.Clear()
                Issue(issues, If(TypeOf ex Is OperationCanceledException, "ANOVA_CANCELLED", "ANOVA_GROUPED_FAILURE"), ex.Message, "workflow")
            End Try
            Return New AnovaGroupedWorkflowResult(input, options, result, issues, plots, failure)
        End Function

        Public Shared Function BuildSupplements(input As AssumptionReportInput, options As AnovaGroupedWorkflowOptions,
                                                Optional cancellation As CancellationToken = Nothing) As AssumptionReportResult
            Dim tables As New List(Of AssumptionTableSpecification), issues As New List(Of SpreadsheetValidationIssue)
            Dim working = If(input Is Nothing, Nothing, input.Groups), failure As Exception = Nothing
            Dim stages As New List(Of AssumptionOutputStage)
            Try
                If input Is Nothing OrElse options Is Nothing Then Throw New ArgumentException("Input and workflow options are required.")
                For Each stage In New AnovaWorkflowPlan(options).Stages
                    cancellation.ThrowIfCancellationRequested()
                    If stage = AnovaWorkflowStage.PrimaryTables Then Continue For
                    If stage = AnovaWorkflowStage.WindowsBoxSummary Then
                        stages.Add(AssumptionOutputStage.WindowsBoxSummaryAndChart)
                        Continue For
                    End If
                    Dim kind = If(stage = AnovaWorkflowStage.Descriptive, AssumptionReportKind.Descriptive, AssumptionReportKind.Homogeneity)
                    'These are computational scratch vectors, possibly sorted by an earlier
                    'supplement. Original provenance remains on input, never on this scratch.
                    Dim report = AssumptionReportPolicy.Build(New AssumptionReportInput(working, input.Names),
                        New AssumptionReportOptions(kind, homogeneity:=options.Homogeneity), cancellation)
                    working = report.LegacyWorkingGroups
                    issues.AddRange(report.Validation.Issues)
                    report.ThrowIfFailed()
                    tables.AddRange(report.Tables)
                    stages.Add(If(stage = AnovaWorkflowStage.Descriptive, AssumptionOutputStage.DescriptiveTable, AssumptionOutputStage.PrimaryTable))
                Next
            Catch ex As Exception
                failure = ex : tables.Clear() : stages.Clear()
                Issue(issues, If(TypeOf ex Is OperationCanceledException, "ANOVA_CANCELLED", "ANOVA_SUPPLEMENT_FAILURE"), ex.Message, "supplements")
            End Try
            Return New AssumptionReportResult(tables, issues, stages, working, failure)
        End Function

        Public Shared Function RunRepeated(matrix As Double(,), names As String(), options As RepeatedAnovaWorkflowOptions,
                                           Optional sources As SpreadsheetNumericBlocks = Nothing,
                                           Optional cancellation As CancellationToken = Nothing) As AnovaModelReportResult
            Dim working = If(matrix Is Nothing, Nothing, DirectCast(matrix.Clone(), Double(,)))
            Dim labels = If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
            Dim issues As New List(Of SpreadsheetValidationIssue), calls As New List(Of String), tables As New List(Of ResultTable)
            Dim failure As Exception = Nothing
            Try
                cancellation.ThrowIfCancellationRequested()
                If working Is Nothing OrElse labels Is Nothing OrElse working.GetLength(0) < 1 OrElse working.GetLength(1) < 2 OrElse labels.Length <> working.GetLength(1) OrElse options Is Nothing Then Throw New ArgumentException("Complete subject rows and at least two named treatments are required.")
                If Double.IsNaN(options.Alpha) OrElse Double.IsInfinity(options.Alpha) OrElse options.Alpha <= 0 OrElse options.Alpha >= 1 Then Throw New ArgumentException("Alpha must be finite and between zero and one.")
                For Each value In working
                    If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentException("Repeated ANOVA requires finite complete blocks.")
                Next
                ValidateSources(working, sources, issues)
                If issues.Any(Function(i) i.Severity = SpreadsheetValidationSeverity.Error) Then Throw New SpreadsheetInputValidationException(New SpreadsheetValidationReport(issues))
                Dim model As New parametric.OneWayRmANOVA(working, labels)
                calls.Add("compute") : model.compute()
                If options.GreenhouseGeisser Then
                    calls.Add("GreenhouseGeisser") : model.GreenhouseGeisser()
                End If
                If options.HuynhFeldt Then
                    calls.Add("HuyhnFeldt") : model.HuyhnFeldt()
                End If
                If options.Tukey Then
                    calls.Add("TukeyKramerRM2") : model.TukeyKramerRM2(options.Alpha)
                    calls.Add("Tukey") : model.Tukey(options.Alpha)
                End If
                calls.Add("wrapResults") : tables.AddRange(model.wrapResults())
                If options.Mauchly Then
                    calls.Add("MauchlyTest")
                    Dim test = assumptions.MauchlyTest(working), table As New ResultTable
                    table.SetBody(New Object(,) {{"Chi2", test.TestStatistics1}, {"P-value", test.Pvalue}})
                    table.AddHeaderTopRow({"Mauchly 's Test of Sphericity", ""})
                    table.AddPvalueCellToFormat(2, 2) : tables.Add(table)
                End If
                If options.Descriptive Then
                    calls.Add("Descriptive")
                    Dim groups = Enumerable.Range(0, labels.Length).Select(Function(c) DataManagement.ArrayUtilities.GetColumn(working, c)).ToArray()
                    Dim report = AssumptionReportPolicy.Build(New AssumptionReportInput(groups, labels), New AssumptionReportOptions(AssumptionReportKind.Descriptive), cancellation)
                    issues.AddRange(report.Validation.Issues) : report.ThrowIfFailed()
                    tables.AddRange(report.Tables.Select(Function(t) t.ToLegacyTable()))
                End If
                cancellation.ThrowIfCancellationRequested()
                ObserveUndefined(tables.Select(Function(t) t.ToOutputModel()), issues)
            Catch ex As Exception
                failure = ex : tables.Clear()
                Issue(issues, If(TypeOf ex Is OperationCanceledException, "ANOVA_CANCELLED", "ANOVA_REPEATED_FAILURE"), ex.Message, "repeated")
            End Try
            Return New AnovaModelReportResult(tables, issues, calls, working, Nothing, sources, Nothing, Nothing, failure, options IsNot Nothing AndAlso options.BoxSummary)
        End Function

        Public Shared Function RunRepeated(input As SpreadsheetNumericBlocks, options As RepeatedAnovaWorkflowOptions,
                                           Optional cancellation As CancellationToken = Nothing) As AnovaModelReportResult
            Return RunRepeated(If(input Is Nothing, Nothing, input.Matrix), If(input Is Nothing, Nothing, input.Names.ToArray()), options, input, cancellation)
        End Function

        Private Shared Sub ValidateSources(matrix As Double(,), sources As SpreadsheetNumericBlocks, issues As List(Of SpreadsheetValidationIssue))
            If sources Is Nothing Then
                Issue(issues, "ANOVA_SOURCE_UNAVAILABLE", "No physical row provenance was supplied; none is invented.", "sources", SpreadsheetValidationSeverity.Information)
                Return
            End If
            If sources.Blocks.Count <> matrix.GetLength(0) OrElse sources.Names.Count <> matrix.GetLength(1) Then
                Issue(issues, "ANOVA_SOURCE_MAP", "Source dimensions do not match the original matrix.", "sources")
                Return
            End If
            Dim seen As New HashSet(Of Integer)
            For r = 0 To sources.Blocks.Count - 1
                Dim row = sources.Blocks(r)(0).WorksheetRow
                If row > 0 AndAlso Not seen.Add(row) Then Issue(issues, "ANOVA_DUPLICATE_BLOCK", "Duplicate physical subject row.", "sources")
                For c = 0 To matrix.GetLength(1) - 1
                    Dim source = sources.Blocks(r)(c)
                    If source.Value <> matrix(r, c) OrElse source.WorksheetRow <> row Then Issue(issues, "ANOVA_SOURCE_MAP", "Original source values/rows disagree with the block matrix.", "sources")
                Next
            Next
        End Sub

        Public Shared Function RunNested(matrix As Object(,), names As String(), Optional sources As SpreadsheetNestedInput = Nothing,
                                         Optional cancellation As CancellationToken = Nothing,
                                         Optional originalSource As SpreadsheetNestedInput = Nothing) As AnovaModelReportResult
            Dim working = If(matrix Is Nothing, Nothing, DirectCast(matrix.Clone(), Object(,)))
            Dim labels = If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
            Dim issues As New List(Of SpreadsheetValidationIssue), calls As New List(Of String), tables As New List(Of ResultTable)
            Dim failure As Exception = Nothing, balanced As Boolean? = Nothing
            Try
                cancellation.ThrowIfCancellationRequested()
                If working Is Nothing OrElse labels Is Nothing OrElse working.GetLength(0) = 0 OrElse working.GetLength(1) < 3 OrElse labels.Length <> working.GetLength(1) Then Throw New ArgumentException("Named group, nested factor and response columns are required.")
                Dim parents As New Dictionary(Of String, HashSet(Of String))(StringComparer.Ordinal)
                For r = 0 To working.GetLength(0) - 1
                    For c = 0 To working.GetLength(1) - 1
                        Dim value = working(r, c)
                        If value Is Nothing OrElse (Not TypeOf value Is String AndAlso Not IsNumericType(value)) Then Throw New ArgumentException("Only detached text/numeric cells are supported.")
                        If c < 2 AndAlso String.IsNullOrWhiteSpace(Convert.ToString(value, Globalization.CultureInfo.CurrentCulture)) Then Throw New ArgumentException("Nested labels must be nonempty.")
                        If c >= 2 Then
                            Dim numeric = Convert.ToDouble(value, Globalization.CultureInfo.CurrentCulture)
                            If Double.IsNaN(numeric) OrElse Double.IsInfinity(numeric) Then Throw New ArgumentException("Responses must be finite.")
                        End If
                    Next
                    Dim parent = Convert.ToString(working(r, 0), Globalization.CultureInfo.CurrentCulture)
                    Dim child = Convert.ToString(working(r, 1), Globalization.CultureInfo.CurrentCulture)
                    If Not parents.ContainsKey(child) Then parents.Add(child, New HashSet(Of String)(StringComparer.Ordinal))
                    parents(child).Add(parent)
                Next
                If parents.Values.Any(Function(p) p.Count > 1) Then Issue(issues, "ANOVA_NESTED_GLOBAL_LABEL", "Existing Core requires globally unique subgroup labels and rejects a repeated label under different parents. No automatic recoding was applied.", "nested.labels", SpreadsheetValidationSeverity.Warning)
                If working.GetLength(1) > 3 Then Issue(issues, "ANOVA_NESTED_EXTRA_RESPONSES", "Existing Core uses the first response column only; extra original columns are retained.", "nested.responses", SpreadsheetValidationSeverity.Warning)
                If sources Is Nothing Then
                    Issue(issues, "ANOVA_SOURCE_UNAVAILABLE", "Physical source provenance is unavailable; none is invented.", "sources", SpreadsheetValidationSeverity.Information)
                Else
                    Dim response(working.GetLength(0) - 1, working.GetLength(1) - 3) As Double
                    If sources.GroupLabels.Count <> working.GetLength(0) OrElse sources.NestedLabels.Count <> working.GetLength(0) Then Throw New ArgumentException("Nested source labels have invalid dimensions.")
                    For r = 0 To working.GetLength(0) - 1
                        If sources.GroupLabels(r) <> Convert.ToString(working(r, 0), Globalization.CultureInfo.CurrentCulture) OrElse
                           sources.NestedLabels(r) <> Convert.ToString(working(r, 1), Globalization.CultureInfo.CurrentCulture) Then Throw New ArgumentException("Nested source labels disagree with the original matrix.")
                        For c = 2 To working.GetLength(1) - 1
                            response(r, c - 2) = Convert.ToDouble(working(r, c), Globalization.CultureInfo.CurrentCulture)
                        Next
                    Next
                    ValidateSources(response, sources.Responses, issues)
                    If issues.Any(Function(i) i.Severity = SpreadsheetValidationSeverity.Error) Then Throw New SpreadsheetInputValidationException(New SpreadsheetValidationReport(issues))
                End If
                calls.Add("TwoWayNestedANOVA")
                Dim model As New parametric.TwoWayNestedANOVA(working, labels)
                calls.Add("compute") : model.compute()
                calls.Add("wrapResults") : tables.AddRange(model.wrapResults())
                balanced = model.balancedDesign
                cancellation.ThrowIfCancellationRequested()
                ObserveUndefined(tables.Select(Function(t) t.ToOutputModel()), issues)
            Catch ex As Exception
                failure = ex : tables.Clear() : balanced = Nothing
                Issue(issues, If(TypeOf ex Is OperationCanceledException, "ANOVA_CANCELLED", "ANOVA_NESTED_FAILURE"), ex.Message, "nested")
            End Try
            Return New AnovaModelReportResult(tables, issues, calls, Nothing, working, Nothing, sources, balanced, failure, False, originalSource)
        End Function
        Public Shared Function RunNested(input As SpreadsheetNestedInput, Optional cancellation As CancellationToken = Nothing) As AnovaModelReportResult
            If input Is Nothing Then Return RunNested(Nothing, Nothing, Nothing, cancellation)
            If input.Responses Is Nothing OrElse input.GroupLabels.Count <> input.Responses.Blocks.Count OrElse input.NestedLabels.Count <> input.Responses.Blocks.Count Then Return RunNested(Nothing, Nothing, input, cancellation)
            Dim matrix(input.Responses.Blocks.Count - 1, input.Responses.Names.Count + 1) As Object
            For r = 0 To input.Responses.Blocks.Count - 1
                matrix(r, 0) = input.GroupLabels(r) : matrix(r, 1) = input.NestedLabels(r)
                For c = 0 To input.Responses.Names.Count - 1
                    matrix(r, c + 2) = input.Responses.Blocks(r)(c).Value
                Next
            Next
            Return RunNested(matrix, New String() {"Group", "Nested"}.Concat(input.Responses.Names).ToArray(), input, cancellation)
        End Function
        Private Shared Function IsNumericType(value As Object) As Boolean
            Select Case Type.GetTypeCode(value.GetType())
                Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32, TypeCode.UInt32,
                     TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                    Return True
            End Select
            Return False
        End Function
        Private Shared Sub ObserveUndefined(tables As IEnumerable(Of ResultTableOutputModel), issues As List(Of SpreadsheetValidationIssue))
            Dim index = 0
            For Each table In tables
                For r = 0 To table.RowCount - 1
                    For c = 0 To table.ColumnCount - 1
                        If TypeOf table.Values(r, c) Is Double Then
                            Dim value = CDbl(table.Values(r, c))
                            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                                Issue(issues, "ANOVA_UNDEFINED_RESULT", "Existing Core returned an undefined/nonfinite cell; no finite value or NA label was substituted.",
                                    String.Format(Globalization.CultureInfo.InvariantCulture, "tables[{0}].cells[{1},{2}]", index, r, c), SpreadsheetValidationSeverity.Warning)
                            End If
                        End If
                    Next
                Next
                index += 1
            Next
        End Sub
    End Class
End Namespace
