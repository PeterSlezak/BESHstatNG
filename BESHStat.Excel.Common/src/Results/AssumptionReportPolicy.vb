Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq
Imports System.Threading

Namespace ExcelCommon
    Public Enum AssumptionReportKind
        Normality
        Outliers
        Homogeneity
        Descriptive
        Symmetry
    End Enum

    Public Enum AssumptionOutlierMethod
        None
        Grubbs
        Rosner
    End Enum

    Public Enum AssumptionSymmetryMethod
        MiaoGelGastwirth
        CabilioMasaro
    End Enum

    <Flags>
    Public Enum HomogeneityMethods
        None = 0
        FlignerKilleen = 1
        LeveneMedian = 2
        SquaredRanks = 4
        Bartlett = 8
        All = 15
    End Enum

    Public Enum AssumptionOutputStage
        PrimaryTable
        DescriptiveTable
        WindowsBoxSummaryAndChart
    End Enum

    ''' <summary>Shipping GUI eligibility, intentionally distinct from Core's inclusive
    ''' Shapiro documentation. Does not claim numerical success for an eligible method.</summary>
    Public NotInheritable Class NormalityReportEligibility
        Public Sub New(observationCount As Integer)
            Shapiro = observationCount > 3 AndAlso observationCount < 5000
            DAgostino = observationCount >= 9
            AndersonDarling = observationCount > 1
        End Sub
        Public ReadOnly Property Shapiro As Boolean
        Public ReadOnly Property DAgostino As Boolean
        Public ReadOnly Property AndersonDarling As Boolean
    End Class

    ''' <summary>Original, immutable observations. Sources refer only to these observations,
    ''' never to the reordered Core scratch buffers used by legacy graphics.</summary>
    Public NotInheritable Class AssumptionReportInput
        Private ReadOnly _groups As Double()()
        Private ReadOnly _names As String()
        Private ReadOnly _sources As IReadOnlyList(Of UnivariateObservationSource)
        Public Sub New(groups As Double()(), names As String(), Optional sources As IEnumerable(Of UnivariateObservationSource) = Nothing)
            _groups = CopyGroups(groups)
            _names = If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
            _sources = New ReadOnlyCollection(Of UnivariateObservationSource)(If(sources, Array.Empty(Of UnivariateObservationSource)()).ToList())
        End Sub
        Public ReadOnly Property Groups As Double()()
            Get
                Return CopyGroups(_groups)
            End Get
        End Property
        Public ReadOnly Property Names As String()
            Get
                Return If(_names Is Nothing, Nothing, DirectCast(_names.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property Sources As IReadOnlyList(Of UnivariateObservationSource)
            Get
                Return _sources
            End Get
        End Property
        Friend Shared Function CopyGroups(groups As Double()()) As Double()()
            If groups Is Nothing Then Return Nothing
            Return groups.Select(Function(g) If(g Is Nothing, Nothing, DirectCast(g.Clone(), Double()))).ToArray()
        End Function
    End Class

    ''' <summary>View-independent option snapshot. Decimal alpha preserves the existing
    ''' outlier table cell type; no localized number parsing occurs here.</summary>
    Public NotInheritable Class AssumptionReportOptions
        Private ReadOnly _statistics As String()
        Public Sub New(kind As AssumptionReportKind,
                       Optional outlierMethod As AssumptionOutlierMethod = AssumptionOutlierMethod.None,
                       Optional alpha As Decimal = 0.05D,
                       Optional homogeneity As HomogeneityMethods = HomogeneityMethods.None,
                       Optional symmetry As AssumptionSymmetryMethod = AssumptionSymmetryMethod.MiaoGelGastwirth,
                       Optional descriptive As Boolean = False, Optional boxSummary As Boolean = False,
                       Optional statistics As IEnumerable(Of String) = Nothing)
            Me.Kind = kind
            Me.OutlierMethod = outlierMethod
            Me.Alpha = alpha
            Me.Homogeneity = homogeneity
            Me.Symmetry = symmetry
            Me.IncludeDescriptive = descriptive
            Me.IncludeBoxSummary = boxSummary
            _statistics = If(statistics Is Nothing, Nothing, statistics.ToArray())
        End Sub
        Public ReadOnly Property Kind As AssumptionReportKind
        Public ReadOnly Property OutlierMethod As AssumptionOutlierMethod
        Public ReadOnly Property Alpha As Decimal
        Public ReadOnly Property Homogeneity As HomogeneityMethods
        Public ReadOnly Property Symmetry As AssumptionSymmetryMethod
        Public ReadOnly Property IncludeDescriptive As Boolean
        Public ReadOnly Property IncludeBoxSummary As Boolean
        Public ReadOnly Property Statistics As IReadOnlyList(Of String)
            Get
                Return If(_statistics Is Nothing, Nothing, Array.AsReadOnly(DirectCast(_statistics.Clone(), String())))
            End Get
        End Property
    End Class

    ''' <summary>Immutable report components and writer hints. Produces a fresh legacy table
    ''' or existing portable output artifact; neither contains native host references.</summary>
    Public NotInheritable Class AssumptionTableSpecification
        Private ReadOnly _body As Object(,)
        Private ReadOnly _headers As String()()
        Private ReadOnly _left As String()
        Private ReadOnly _title As String
        Private ReadOnly _pvalues As ResultTableCellAddress()
        Private ReadOnly _adjust As Boolean
        Friend Sub New(body As Object(,), headers As String()(), Optional left As String() = Nothing,
                       Optional title As String = Nothing, Optional pvalues As IEnumerable(Of ResultTableCellAddress) = Nothing,
                       Optional adjust As Boolean = False)
            _body = If(body Is Nothing, Nothing, DirectCast(body.Clone(), Object(,)))
            _headers = headers.Select(Function(h) DirectCast(h.Clone(), String())).ToArray()
            _left = If(left Is Nothing, Nothing, DirectCast(left.Clone(), String()))
            _title = title
            _pvalues = If(pvalues, Array.Empty(Of ResultTableCellAddress)()).ToArray()
            _adjust = adjust
        End Sub
        Public Function ToLegacyTable() As ResultTable
            Dim table As New ResultTable
            table.SetBody(If(_body Is Nothing, Nothing, DirectCast(_body.Clone(), Object(,))))
            For Each header In _headers
                table.AddHeaderTopRow(DirectCast(header.Clone(), String()))
            Next
            If _left IsNot Nothing Then table.AddHeaderLeftRow(DirectCast(_left.Clone(), String()))
            If _title IsNot Nothing Then table.AddTitle(_title)
            For Each cell In _pvalues
                table.AddPvalueCellToFormat(cell.BodyRow, cell.BodyColumn)
            Next
            table.bLeftHeaderAdjustUp = _adjust
            Return table
        End Function
        Public Function ToArtifact(index As Integer) As AnalysisTableArtifact
            Return New AnalysisTableArtifact(index, ToLegacyTable().ToOutputModel())
        End Function
    End Class

    Public NotInheritable Class AssumptionReportResult
        Private ReadOnly _working As Double()()
        Private ReadOnly _failure As Exception
        Friend Sub New(tables As IEnumerable(Of AssumptionTableSpecification), issues As IEnumerable(Of SpreadsheetValidationIssue),
                       stages As IEnumerable(Of AssumptionOutputStage), working As Double()(), failure As Exception)
            Me.Tables = tables.ToList().AsReadOnly()
            Me.Validation = New SpreadsheetValidationReport(issues)
            Me.Stages = stages.ToList().AsReadOnly()
            _working = AssumptionReportInput.CopyGroups(working)
            _failure = failure
        End Sub
        Public ReadOnly Property Tables As IReadOnlyList(Of AssumptionTableSpecification)
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Stages As IReadOnlyList(Of AssumptionOutputStage)
        Public ReadOnly Property Success As Boolean
            Get
                Return Not Validation.HasErrors
            End Get
        End Property
        Public ReadOnly Property BlankRowsBetweenTables As Integer = 1
        ''' <summary>Legacy-only computational scratch. NO physical-row identity is asserted.
        ''' Kept separate from original input/provenance because Core sorts in place.</summary>
        Public ReadOnly Property LegacyWorkingGroups As Double()()
            Get
                Return AssumptionReportInput.CopyGroups(_working)
            End Get
        End Property
        ''' <summary>Compatibility facade for the existing host's outer exception handler.
        ''' Portable consumers inspect Validation and never need this facade.</summary>
        Public Sub ThrowIfFailed()
            If Success Then Return
            If _failure IsNot Nothing Then Throw _failure
            Throw New SpreadsheetInputValidationException(Validation)
        End Sub
    End Class

    Public NotInheritable Class AssumptionReportPolicy
        Private Sub New()
        End Sub

        Public Shared Function Build(input As AssumptionReportInput, options As AssumptionReportOptions,
                                     Optional cancellation As CancellationToken = Nothing) As AssumptionReportResult
            Dim tables As New List(Of AssumptionTableSpecification)
            Dim issues As New List(Of SpreadsheetValidationIssue)
            Dim stages As New List(Of AssumptionOutputStage)
            Dim groups = If(input Is Nothing, Nothing, input.Groups)
            Dim names = If(input Is Nothing, Nothing, input.Names)
            Dim failure As Exception = Nothing
            Validate(input, options, groups, names, issues)
            If issues.Any(Function(i) i.Severity = SpreadsheetValidationSeverity.Error) Then Return New AssumptionReportResult(tables, issues, stages, groups, Nothing)
            Try
                cancellation.ThrowIfCancellationRequested()
                Select Case options.Kind
                    Case AssumptionReportKind.Normality
                        tables.Add(Normality(groups, names, issues, cancellation))
                    Case AssumptionReportKind.Outliers
                        tables.Add(Outliers(groups, names, options, issues, cancellation))
                    Case AssumptionReportKind.Homogeneity
                        If options.Homogeneity <> HomogeneityMethods.None Then tables.Add(Homogeneity(groups, options.Homogeneity))
                    Case AssumptionReportKind.Descriptive
                        tables.Add(Descriptive(groups, names, options.Statistics, cancellation))
                    Case AssumptionReportKind.Symmetry
                        tables.Add(Symmetry(groups, names, options.Symmetry, cancellation))
                End Select
                If tables.Count > 0 Then stages.Add(AssumptionOutputStage.PrimaryTable)
                If options.IncludeDescriptive AndAlso options.Kind <> AssumptionReportKind.Descriptive Then
                    tables.Add(Descriptive(groups, names, Nothing, cancellation))
                    stages.Add(AssumptionOutputStage.DescriptiveTable)
                End If
                If options.IncludeBoxSummary Then stages.Add(AssumptionOutputStage.WindowsBoxSummaryAndChart)
                For t = 0 To tables.Count - 1
                    Dim model = tables(t).ToArtifact(t).Model
                    For r = 0 To model.RowCount - 1
                        For c = 0 To model.ColumnCount - 1
                            If TypeOf model.Values(r, c) Is Double Then
                                Dim value = CDbl(model.Values(r, c))
                                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                                    Issue(issues, "ASSUMPTION_UNDEFINED_RESULT", SpreadsheetValidationSeverity.Warning,
                                          "Core returned an undefined or nonfinite value; the legacy cell is retained.",
                                          String.Format(Globalization.CultureInfo.InvariantCulture, "tables[{0}].cells[{1},{2}]", t, r, c))
                                End If
                            End If
                        Next
                    Next
                Next
                cancellation.ThrowIfCancellationRequested()
                If stages.Count = 0 Then Issue(issues, "ASSUMPTION_NO_OUTPUT", SpreadsheetValidationSeverity.Information, "No report selected.", "options")
            Catch ex As OperationCanceledException
                failure = ex
                Issue(issues, "ASSUMPTION_CANCELLED", SpreadsheetValidationSeverity.Error, "Report cancelled.", "report")
            Catch ex As Exception
                failure = ex
                Issue(issues, "ASSUMPTION_CORE_FAILURE", SpreadsheetValidationSeverity.Error, ex.Message, "report")
            End Try
            If failure IsNot Nothing Then
                tables.Clear()
                stages.Clear()
            End If
            Return New AssumptionReportResult(tables, issues, stages, groups, failure)
        End Function

        Private Shared Sub Validate(input As AssumptionReportInput, options As AssumptionReportOptions, groups As Double()(), names As String(), issues As List(Of SpreadsheetValidationIssue))
            If options Is Nothing OrElse Not [Enum].IsDefined(GetType(AssumptionReportKind), options.Kind) Then
                Issue(issues, "ASSUMPTION_OPTIONS", SpreadsheetValidationSeverity.Error, "A supported report kind is required.", "options.kind")
                Return
            End If
            If Not [Enum].IsDefined(GetType(AssumptionOutlierMethod), options.OutlierMethod) OrElse
               Not [Enum].IsDefined(GetType(AssumptionSymmetryMethod), options.Symmetry) OrElse
               (CInt(options.Homogeneity) And Not CInt(HomogeneityMethods.All)) <> 0 Then
                Issue(issues, "ASSUMPTION_OPTIONS", SpreadsheetValidationSeverity.Error, "Unsupported method option.", "options.method")
            End If
            If options.Alpha <= 0D OrElse options.Alpha >= 1D Then Issue(issues, "ASSUMPTION_ALPHA", SpreadsheetValidationSeverity.Error, "Alpha must be between zero and one.", "options.alpha")
            If groups Is Nothing OrElse names Is Nothing OrElse groups.Length = 0 OrElse groups.Length <> names.Length Then
                Issue(issues, "ASSUMPTION_SHAPE", SpreadsheetValidationSeverity.Error, "Named numeric groups are required.", "input.groups")
                Return
            End If
            For g = 0 To groups.Length - 1
                If groups(g) Is Nothing OrElse groups(g).Length = 0 Then
                    Issue(issues, "ASSUMPTION_EMPTY_GROUP", SpreadsheetValidationSeverity.Error, "An observation is required.", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "]")
                    Continue For
                End If
                For r = 0 To groups(g).Length - 1
                    If Double.IsNaN(groups(g)(r)) OrElse Double.IsInfinity(groups(g)(r)) Then
                        Dim groupIndex = g, observationIndex = r
                        Dim src = input.Sources.FirstOrDefault(Function(s) s IsNot Nothing AndAlso s.GroupIndex = groupIndex AndAlso s.ObservationIndex = observationIndex)
                        issues.Add(New SpreadsheetValidationIssue("ASSUMPTION_NONFINITE", SpreadsheetValidationSeverity.Error, "A finite numeric observation is required.",
                            "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "][" & r.ToString(Globalization.CultureInfo.InvariantCulture) & "]",
                            If(src Is Nothing, 0, Math.Max(0, src.SelectedRow)), If(src Is Nothing, 0, Math.Max(0, src.SelectedColumn)), If(src Is Nothing, 0, Math.Max(0, src.WorksheetRow)), If(src Is Nothing, 0, Math.Max(0, src.WorksheetColumn))))
                    End If
                Next
            Next
            For Each src In input.Sources
                If src Is Nothing OrElse src.GroupIndex < 0 OrElse src.GroupIndex >= groups.Length OrElse groups(src.GroupIndex) Is Nothing OrElse
                   src.ObservationIndex < 0 OrElse src.ObservationIndex >= groups(src.GroupIndex).Length OrElse
                   src.SelectedRow < 0 OrElse src.SelectedColumn < 0 OrElse src.WorksheetRow < 0 OrElse src.WorksheetColumn < 0 Then
                    Issue(issues, "ASSUMPTION_SOURCE_MAP", SpreadsheetValidationSeverity.Error, "Source mapping is outside the original observations.", "input.sources")
                End If
            Next
            If input.Sources.Where(Function(s) s IsNot Nothing).GroupBy(Function(s) Tuple.Create(s.GroupIndex, s.ObservationIndex)).Any(Function(s) s.Count() > 1) Then
                Issue(issues, "ASSUMPTION_SOURCE_MAP", SpreadsheetValidationSeverity.Error, "An original observation has duplicate source mappings.", "input.sources")
            End If
        End Sub

        Private Shared Sub Issue(issues As List(Of SpreadsheetValidationIssue), code As String, severity As SpreadsheetValidationSeverity, message As String, field As String)
            issues.Add(New SpreadsheetValidationIssue(code, severity, message, field))
        End Sub

        Private Shared Function Normality(groups As Double()(), names As String(), issues As List(Of SpreadsheetValidationIssue), token As CancellationToken) As AssumptionTableSpecification
            Dim labels = {"Shapiro-Wilk test", "Test statistics", "Two-sided P-value", "D'Agostino-Pearson K2 test", "Test statistics", "Two-sided P-value", "Anderson Darling test", "Test statistics", "Two-sided P-value"}
            Dim body(8, groups.Length) As Object
            Dim pvalues As New List(Of ResultTableCellAddress)
            For row = 0 To 8
                body(row, 0) = labels(row)
            Next
            For g = 0 To groups.Length - 1
                token.ThrowIfCancellationRequested()
                Dim eligible As New NormalityReportEligibility(groups(g).Length)
                Dim errorText As String = Nothing 'fresh per group; never shared diagnostic state
                If eligible.Shapiro Then
                    Dim current = groups(g)
                    PutTest(body, 0, g + 1, CoreTest(Function() assumptions.ShapiroWilk(current, errorText), issues, g, "shapiro"))
                Else
                    PutNA(body, 0, g + 1, "NA n<4 or n>5000")
                    Issue(issues, "NORMALITY_SW_NOT_APPLICABLE", SpreadsheetValidationSeverity.Information, "NA n<4 or n>5000", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].shapiro")
                End If
                If eligible.DAgostino Then
                    Dim current = groups(g)
                    PutTest(body, 3, g + 1, CoreTest(Function() assumptions.DAgostino(current, errorText), issues, g, "dagostino"))
                Else
                    PutNA(body, 3, g + 1, "NA n<9")
                    Issue(issues, "NORMALITY_DA_NOT_APPLICABLE", SpreadsheetValidationSeverity.Information, "NA n<9", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].dagostino")
                End If
                If eligible.AndersonDarling Then
                    Dim current = groups(g)
                    PutTest(body, 6, g + 1, CoreTest(Function() assumptions.AndersonDarlingTEST(current), issues, g, "anderson"))
                Else
                    PutNA(body, 6, g + 1, "NA")
                    Issue(issues, "NORMALITY_AD_NOT_APPLICABLE", SpreadsheetValidationSeverity.Information, "NA", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].anderson")
                End If
                For Each row In {3, 6, 9}
                    pvalues.Add(New ResultTableCellAddress(row, g + 2))
                Next
            Next
            Return New AssumptionTableSpecification(body, {New String() {"Normality Tests"}.Concat(names).ToArray()}, pvalues:=pvalues)
        End Function

        Private Shared Function CoreTest(compute As Func(Of TestResult), issues As List(Of SpreadsheetValidationIssue), group As Integer, method As String) As TestResult
            Try
                Dim result = compute()
                If result Is Nothing Then Throw New NullReferenceException()
                Return result
            Catch ex As Exception
                Issue(issues, "ASSUMPTION_METHOD_FAILURE", SpreadsheetValidationSeverity.Error, ex.Message,
                      "groups[" & group.ToString(Globalization.CultureInfo.InvariantCulture) & "]." & method)
                Throw
            End Try
        End Function

        Private Shared Sub PutNA(body As Object(,), row As Integer, col As Integer, value As String)
            For r = row To row + 2
                body(r, col) = value
            Next
        End Sub
        Private Shared Sub PutTest(body As Object(,), row As Integer, col As Integer, result As TestResult)
            'Deliberately retain the old failure boundary when Core returns Nothing.
            'Build catches it and returns no accepted table; the legacy facade rethrows.
            body(row, col) = ""
            body(row + 1, col) = result.TestStatistics1
            body(row + 2, col) = result.Pvalue
        End Sub

        Private Shared Function Descriptive(groups As Double()(), names As String(), statistics As IReadOnlyList(Of String), token As CancellationToken) As AssumptionTableSpecification
            Dim body As Object(,) = Nothing
            Dim selected = If(statistics Is Nothing, Nothing, statistics.ToList())
            For g = 0 To groups.Length - 1
                token.ThrowIfCancellationRequested()
                Dim ds As New DescriptiveStat(groups(g))
                ds.compute()
                Dim column = ds.wrapSelf(g = 0, selected)
                body = If(g = 0, column, DataManagement.ArrayUtilities.VerticalStackArrays(body, column))
            Next
            Dim pvalues As New List(Of ResultTableCellAddress)
            For row = 0 To body.GetLength(0) - 1
                If String.Equals(TryCast(body(row, 0), String), "Two-sided p-value", StringComparison.OrdinalIgnoreCase) Then
                    For col = 1 To body.GetLength(1) - 1
                        pvalues.Add(New ResultTableCellAddress(row + 1, col + 1))
                    Next
                End If
            Next
            Return New AssumptionTableSpecification(body, {New String() {""}.Concat(names).ToArray()}, title:="Descriptive Statistics", pvalues:=pvalues)
        End Function

        Private Shared Function Homogeneity(groups As Double()(), selected As HomogeneityMethods) As AssumptionTableSpecification
            Dim body As Object(,) = Nothing
            For Each method In {HomogeneityMethods.FlignerKilleen, HomogeneityMethods.LeveneMedian, HomogeneityMethods.SquaredRanks, HomogeneityMethods.Bartlett}
                If (selected And method) = 0 Then Continue For
                Dim result As TestResult, label As String
                Select Case method
                    Case HomogeneityMethods.FlignerKilleen
                        result = assumptions.FlignerKilleenTEST(groups) : label = "Fligner-Killeen"
                    Case HomogeneityMethods.LeveneMedian
                        result = assumptions.LeveneTEST(groups, True) : label = "Levene"
                    Case HomogeneityMethods.SquaredRanks
                        result = assumptions.SquaredRanksTestVARIANCE(groups) : label = "Squared Ranks"
                    Case Else
                        result = assumptions.BartlettTEST(groups) : label = "Bartlett"
                End Select
                Dim block As Object(,) = {{label, ""}, {"Test statistics", result.TestStatistics1}, {"P-value", result.Pvalue}}
                body = If(body Is Nothing, block, DataManagement.ArrayUtilities.HorizontalStackArrays(body, block))
            Next
            Dim pvalues As New List(Of ResultTableCellAddress)
            For row = 3 To body.GetLength(0) Step 3
                pvalues.Add(New ResultTableCellAddress(row, 2))
            Next
            Return New AssumptionTableSpecification(body, {New String() {"Homogeneity of Variances", ""}}, pvalues:=pvalues)
        End Function

        Private Shared Function Outliers(groups As Double()(), names As String(), options As AssumptionReportOptions, issues As List(Of SpreadsheetValidationIssue), token As CancellationToken) As AssumptionTableSpecification
            Dim body As Object(,) = Nothing, left As String() = Nothing
            If options.OutlierMethod = AssumptionOutlierMethod.Grubbs Then
                ReDim body(3, groups.Length - 1)
                left = {"Sample", "Alpha", "Critical Test Statistic", "Sample Test Statistic", "Result"}
            ElseIf options.OutlierMethod = AssumptionOutlierMethod.Rosner Then
                ReDim body(12, groups.Length - 1)
                left = {"Sample", "Alpha", "Number of Outliers"}
            End If
            For g = 0 To groups.Length - 1
                token.ThrowIfCancellationRequested()
                If options.OutlierMethod = AssumptionOutlierMethod.Grubbs Then
                    Dim result = assumptions.Grubbs(groups(g), CDbl(options.Alpha))
                    body(0, g) = options.Alpha
                    body(1, g) = result.TestStatistics1
                    body(2, g) = result.TestStatistics2
                    body(3, g) = result.strSpecialInformation
                ElseIf options.OutlierMethod = AssumptionOutlierMethod.Rosner Then
                    Dim values = assumptions.Rosner(groups(g), CDbl(options.Alpha))
                    body(0, g) = options.Alpha
                    body(1, g) = If(values Is Nothing, 0, values.Length)
                    body(2, g) = "List of Outliers:"
                    If values IsNot Nothing Then
                        For r = 0 To values.Length - 1
                            body(r + 3, g) = values(r)
                        Next
                    Else
                        Issue(issues, "ROSNER_NO_RESULT", SpreadsheetValidationSeverity.Information, "Core returned no outlier array; the legacy table displays zero.", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].rosner")
                    End If
                    If groups(g).Length < 15 Then Issue(issues, "ROSNER_NOT_APPLICABLE", SpreadsheetValidationSeverity.Warning, "Core returns no result below 15 observations; legacy table displays zero.", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].rosner")
                    If groups(g).Length >= 15 AndAlso groups(g).Length < 25 Then Issue(issues, "ROSNER_SMALL_SAMPLE", SpreadsheetValidationSeverity.Warning, "Core warns for samples below 25 observations.", "groups[" & g.ToString(Globalization.CultureInfo.InvariantCulture) & "].rosner")
                End If
            Next
            Return New AssumptionTableSpecification(body, {names}, left:=left, adjust:=True)
        End Function

        Private Shared Function Symmetry(groups As Double()(), names As String(), method As AssumptionSymmetryMethod, token As CancellationToken) As AssumptionTableSpecification
            Dim label = If(method = AssumptionSymmetryMethod.MiaoGelGastwirth, "Miao-Gel-Gastwirth", "Cabilio-Masaro")
            Dim body(1, groups.Length - 1) As Object
            Dim pvalues As New List(Of ResultTableCellAddress)
            For g = 0 To groups.Length - 1
                token.ThrowIfCancellationRequested()
                Dim result = assumptions.SymmetryTest(groups(g), label)
                body(0, g) = result.TestStatistics1
                body(1, g) = result.Pvalue
                pvalues.Add(New ResultTableCellAddress(2, g + 1))
            Next
            Dim first(names.Length - 1) As String
            first(0) = label
            Return New AssumptionTableSpecification(body, {first, names}, left:={"Asymetry test", "Test Statistic", "Two-sided P-value"}, pvalues:=pvalues)
        End Function

        ''' <summary>Maps already computed host box statistics; no Tukey/plot calculations or rendering.</summary>
        Public Shared Function BoxSummary(names As String(), q1 As Double(), medians As Double(), q3 As Double(), small As Long(), big As Long()) As AssumptionTableSpecification
            If names Is Nothing OrElse q1 Is Nothing OrElse medians Is Nothing OrElse q3 Is Nothing OrElse small Is Nothing OrElse big Is Nothing OrElse
               {q1.Length, medians.Length, q3.Length, small.Length, big.Length}.Any(Function(n) n <> names.Length) Then Throw New ArgumentException("Box summary dimensions must match.")
            Dim body(names.Length - 1, 5) As Object
            For g = 0 To names.Length - 1
                body(g, 0) = names(g) : body(g, 1) = q1(g) : body(g, 2) = medians(g) : body(g, 3) = q3(g) : body(g, 4) = small(g) : body(g, 5) = big(g)
            Next
            Return New AssumptionTableSpecification(body, {New String() {"Groups", "Q1", "Median", "Q3", "Outliers small", "Outliers big"}})
        End Function
    End Class
End Namespace
