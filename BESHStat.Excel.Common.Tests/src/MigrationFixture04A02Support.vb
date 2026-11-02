Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports BESHStatNG.ExcelCommon

' Test-only admission and serialization. This does not approve scientific references.
Friend NotInheritable Class MigrationFixture04A02
    Public Shared ReadOnly CaseIds As String() = {"C30-SYN-ANOVA", "C30-SYN-KW"}

    Public Shared Function Load(id As String) As JsonObject
        Return JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "04A02", id & ".json"))).AsObject()
    End Function

    Public Shared Function SourceRoot() As String
        Dim configured = Environment.GetEnvironmentVariable("BESHSTAT_04A02_SOURCE_ROOT")
        If Not String.IsNullOrEmpty(configured) Then Return configured
        Dim directory As New DirectoryInfo(AppContext.BaseDirectory)
        While directory IsNot Nothing
            If File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) Then Return directory.FullName
            directory = directory.Parent
        End While
        Throw New InvalidDataException("Source root unavailable; set BESHSTAT_04A02_SOURCE_ROOT for isolated assemblies.")
    End Function

    Public Shared Sub ValidateSet(cases As IEnumerable(Of JsonObject))
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
        For Each fixture In cases
            Validate(fixture)
            Require(seen.Add(fixture("case_id").GetValue(Of String)()), "Duplicate case ID")
        Next
        Require(seen.SetEquals(CaseIds), "Exactly the two authorized cases are required")
    End Sub

    Public Shared Sub Validate(f As JsonObject)
        Try
            ValidateCore(f)
        Catch ex As InvalidDataException
            Throw
        Catch ex As Exception When TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is NullReferenceException OrElse
                                   TypeOf ex Is FormatException OrElse TypeOf ex Is JsonException OrElse TypeOf ex Is KeyNotFoundException
            Throw New InvalidDataException("Invalid or absent fixture field", ex)
        End Try
    End Sub

    Private Shared Sub ValidateCore(f As JsonObject)
        Require(f("schema").GetValue(Of String)() = "04A02-typed-preparation-v1", "Schema")
        Dim id = f("case_id").GetValue(Of String)()
        Require(CaseIds.Contains(id, StringComparer.Ordinal), "Case ID")
        Require(f("kind").GetValue(Of String)() = "typed-synthetic", "Kind")
        Require(f("approved_scope").GetValue(Of String)() = "typed-only preparation", "Scope")
        Require(Not f("live_control_capture").GetValue(Of Boolean)(), "Native capture is unauthorized")
        Require(f("scientific_admission").GetValue(Of String)() = "PENDING_ORACLE_APPROVAL", "Admission")
        Require(f("reference_status").GetValue(Of String)() = "NOT_APPROVED", "Reference status")
        Require(f.ContainsKey("expected_numerical_reference") AndAlso f("expected_numerical_reference") Is Nothing, "No approved reference")
        Require(f("observed_state").GetValue(Of String)() = "NOT_RUN_IN_FIXTURE", "Observation belongs in run evidence")
        Require(f("output_status").GetValue(Of String)() = "CANDIDATE_ONLY", "Output status")
        Require(f("culture").GetValue(Of String)() = "InvariantCulture", "Culture")
        Require(f("planned_culture_check").GetValue(Of String)() = "sk-SK", "Culture check")
        Require(f("seed").GetValue(Of String)() = "N/A", "Seed")
        Require(f("row_id_transport").GetValue(Of String)() = "provenance-only; not transported by GroupedNumericData", "Row identity boundary")
        Dim alpha = f("alpha").GetValue(Of Double)()
        Require(Double.IsFinite(alpha) AndAlso alpha > 0 AndAlso alpha < 1 AndAlso alpha = 0.05, "Alpha")
        Dim labels = f("dataset")("group_labels").AsArray()
        Dim values = f("dataset")("values").AsArray()
        Dim rows = f("dataset")("row_ids").AsArray()
        Require(labels.Count = 3 AndAlso values.Count = 3 AndAlso rows.Count = 3, "Group dimensions")
        For i As Integer = 0 To 2
            Require(Not String.IsNullOrWhiteSpace(labels(i).GetValue(Of String)()), "Missing label")
            Require(values(i).AsArray().Count = rows(i).AsArray().Count AndAlso values(i).AsArray().Count = 3, "RowId alignment")
            For Each v In values(i).AsArray()
                Require(Double.IsFinite(v.GetValue(Of Double)()), "Nonfinite input")
            Next
            ' v1 provenance IDs are exact Int32 JSON integer tokens, not floating aliases.
            Dim rowIds As New List(Of Integer)
            For Each rowId In rows(i).AsArray()
                Dim token = JsonSerializer.SerializeToElement(rowId)
                Dim exact As Integer
                Require(token.ValueKind = JsonValueKind.Number AndAlso token.TryGetInt32(exact), "RowId must be an exact Int32 JSON integer")
                rowIds.Add(exact)
            Next
            Require(rowIds.Distinct().Count() = 3, "RowId uniqueness")
        Next
        Require(f("dataset_sha256").GetValue(Of String)() = Hash(Canonical(f("dataset"))), "Dataset hash")
        Dim options = f("options").AsObject()
        Dim flags = If(id = CaseIds(0),
            New String() {"IncludeWelch", "IncludeFisherLsd", "IncludeBonferroni", "IncludeTukeyKramer", "IncludeGamesHowell"},
            New String() {"IncludeDunnComparisons"})
        Require(options.Count = flags.Length, "Option keys")
        For Each flag In flags
            Require(Not options(flag).GetValue(Of Boolean)(), "Only primary analysis is authorized")
        Next
        Dim reference = f("reference_provenance").AsObject()
        Require(reference.Count = 6, "Reference metadata required")
        For Each key In {"script", "version", "method", "citation", "generated_at", "approval"}
            Require(reference.ContainsKey(key) AndAlso reference(key) Is Nothing, "Reference slot must remain pending")
        Next
        Require(f("tolerances").AsArray().Count = 0, "Unapproved tolerances")
        Dim commit = f("source_commit").GetValue(Of String)()
        Require(commit.Length = 40 AndAlso commit.All(Function(c) "0123456789abcdef".Contains(c)), "Source commit")
        Require(f("expected_table_schema")("classification").GetValue(Of String)() = "semantic candidate only", "Table classification")
        Require(f("control_ids").AsArray().Count = 0, "No control dependency claimed by typed fixture")
        Dim proposalPath = "docs/architecture/WindowsMigrationLedger/continuation30-safe-fixture-candidates.json"
        Require(f("proposal_path").GetValue(Of String)() = proposalPath, "Frozen proposal path")
        Dim requiredPins As New HashSet(Of String)(New String() {proposalPath,
            "BESHStat.Excel.Common/src/Input/GroupedNumericRangeParser.vb",
            "BESHStat.Excel.Common/src/Analysis/SpreadsheetAnalysisContracts.vb",
            "BESHStat.Excel.Common/src/Analysis/" & If(id = CaseIds(0), "OneWayAnovaApplicationService.vb", "KruskalWallisApplicationService.vb")}, StringComparer.Ordinal)
        Require(f("source_pins").AsArray().Count = 4, "Four source pins required")
        For Each pin In f("source_pins").AsArray()
            Dim relative = pin("path").GetValue(Of String)()
            Require(requiredPins.Remove(relative), "Unexpected or duplicate source path")
            Require(FileHash(Path.Combine(SourceRoot(), relative)) = pin("sha256").GetValue(Of String)(), "Source hash: " & relative)
        Next
        Dim original = JsonNode.Parse(File.ReadAllText(Path.Combine(SourceRoot(), f("proposal_path").GetValue(Of String)())))
        Dim proposal = original("cases").AsArray().Single(Function(n) n("case_id").GetValue(Of String)() = id)
        Require(Canonical(labels) = Canonical(proposal("group_labels")), "Frozen labels")
        Require(Canonical(values) = Canonical(proposal("values")), "Frozen values")
        Require(Canonical(rows) = Canonical(proposal("source_row_ids")), "Frozen RowIds")
        Require(f("method").GetValue(Of String)() = proposal("settings")("method").GetValue(Of String)(), "Frozen method")
    End Sub

    Public Shared Function Grouped(f As JsonObject) As GroupedNumericData
        Return New GroupedNumericData(f("dataset")("group_labels").AsArray().Select(Function(n) n.GetValue(Of String)()).ToArray(),
            f("dataset")("values").AsArray().Select(Function(g) g.AsArray().Select(Function(n) n.GetValue(Of Double)()).ToArray()).ToArray())
    End Function

    Public Shared Function Observe(f As JsonObject) As JsonObject
        Validate(f)
        Dim input = Grouped(f)
        Dim before = JsonSerializer.Serialize(input)
        Dim tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
        Dim metrics As New Dictionary(Of String, Double)
        Dim names As String(), counts As Integer()
        If f("case_id").GetValue(Of String)() = CaseIds(0) Then
            Dim o = f("options")
            Dim result = OneWayAnovaApplicationService.Run(input, New OneWayAnovaAnalysisOptions With {
                .Alpha = f("alpha").GetValue(Of Double)(), .IncludeWelch = o("IncludeWelch").GetValue(Of Boolean)(),
                .IncludeFisherLsd = o("IncludeFisherLsd").GetValue(Of Boolean)(), .IncludeBonferroni = o("IncludeBonferroni").GetValue(Of Boolean)(),
                .IncludeTukeyKramer = o("IncludeTukeyKramer").GetValue(Of Boolean)(), .IncludeGamesHowell = o("IncludeGamesHowell").GetValue(Of Boolean)()})
            metrics.Add("FStatistic", result.FStatistic) : metrics.Add("PValue", result.PValue)
            Require(Not result.HasWelch, "Unexpected Welch")
            tables = result.Tables : names = result.GroupNames : counts = result.GroupCounts
        Else
            Dim result = KruskalWallisApplicationService.Run(input, New KruskalWallisAnalysisOptions With {
                .Alpha = f("alpha").GetValue(Of Double)(), .IncludeDunnComparisons = f("options")("IncludeDunnComparisons").GetValue(Of Boolean)()})
            metrics.Add("HStatistic", result.HStatistic) : metrics.Add("TieCorrectedHStatistic", result.TieCorrectedHStatistic)
            metrics.Add("PValue", result.PValue) : metrics.Add("TieCorrectedPValue", result.TieCorrectedPValue)
            tables = result.Tables : names = result.GroupNames : counts = result.GroupCounts
        End If
        Require(before = JsonSerializer.Serialize(input), "Service mutated input")
        Require(tables.Count > 0 AndAlso tables.All(Function(t) t.RowCount > 0 AndAlso t.ColumnCount > 0), "Empty results")
        Dim serializedTables As New List(Of Object)
        For Each table In tables
            Dim cells As New List(Of Object)
            For row As Integer = 0 To table.RowCount - 1
                Dim line As New List(Of Object)
                For column As Integer = 0 To table.ColumnCount - 1
                    Dim value = table.Values(row, column)
                    line.Add(New With {.type = If(value Is Nothing, "null", value.GetType().FullName), .value = value})
                Next
                cells.Add(line)
            Next
            serializedTables.Add(New With {.cells = cells, .rows = table.RowCount, .columns = table.ColumnCount,
                .header_top_rows = table.HeaderTopRows, .header_left_columns = table.HeaderLeftColumns,
                .footer_rows = table.FooterRows, .title_rows = table.TitleRows, .is_result_table = table.IsResultTable,
                .pvalue_columns = table.PvalueColumns, .pvalue_cells = table.PvalueCells})
        Next
        Return JsonSerializer.SerializeToNode(New With {.schema = "04A02-candidate-observation-v1",
            .case_id = f("case_id").GetValue(Of String)(), .status = "UNVERIFIED_AGAINST_INDEPENDENT_ORACLE",
            .classification = "semantic candidate; no rendered workbook", .live_control_capture = False,
            .source_commit = f("source_commit").GetValue(Of String)(), .fixture_sha256 = Hash(Canonical(f)),
            .serialization_culture = "InvariantCulture", .alpha = f("alpha").GetValue(Of Double)(), .seed = "N/A",
            .dataset_sha256 = f("dataset_sha256").GetValue(Of String)(), .metrics = metrics,
            .group_names = names, .group_counts = counts, .tables = serializedTables}).AsObject()
    End Function

    Public Shared Function Canonical(node As JsonNode) As String
        Using stream As New MemoryStream()
            Using writer As New Utf8JsonWriter(stream)
                WriteCanonical(writer, JsonSerializer.SerializeToElement(node))
            End Using
            Return Encoding.UTF8.GetString(stream.ToArray())
        End Using
    End Function

    Private Shared Sub WriteCanonical(writer As Utf8JsonWriter, element As JsonElement)
        Select Case element.ValueKind
            Case JsonValueKind.Object
                writer.WriteStartObject()
                For Each item In element.EnumerateObject().OrderBy(Function(p) p.Name, StringComparer.Ordinal)
                    writer.WritePropertyName(item.Name) : WriteCanonical(writer, item.Value)
                Next
                writer.WriteEndObject()
            Case JsonValueKind.Array
                writer.WriteStartArray()
                For Each item In element.EnumerateArray()
                    WriteCanonical(writer, item)
                Next
                writer.WriteEndArray()
            Case JsonValueKind.Number
                ' Preserve lexical integer values exactly. Binary64 is only the policy
                ' for decimal/exponent tokens; this is not an exact-decimal serializer.
                Dim raw = element.GetRawText()
                If raw.IndexOfAny(New Char() {"."c, "e"c, "E"c}) < 0 Then
                    Dim exact As Long
                    Require(element.TryGetInt64(exact), "Canonical integer exceeds Int64; a new schema is required")
                    writer.WriteNumberValue(exact)
                    Return
                End If
                Dim value = element.GetDouble()
                Require(Double.IsFinite(value), "JSON nonfinite number")
                writer.WriteNumberValue(value)
            Case Else
                element.WriteTo(writer)
        End Select
    End Sub

    Public Shared Function Hash(value As String) As String
        Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()
    End Function

    Public Shared Function FileHash(path As String) As String
        Return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()
    End Function

    Private Shared Sub Require(condition As Boolean, message As String)
        If Not condition Then Throw New InvalidDataException(message)
    End Sub
End Class
