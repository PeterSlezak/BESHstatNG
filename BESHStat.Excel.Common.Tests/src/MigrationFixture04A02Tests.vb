Option Explicit On
Option Strict On

Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MigrationFixture04A02Tests
    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA")>
    <DataRow("C30-SYN-KW")>
    Public Sub Fixture_RoundTripAndSourceProvenance(id As String)
        Dim f = MigrationFixture04A02.Load(id)
        MigrationFixture04A02.Validate(f)
        Dim json = MigrationFixture04A02.Canonical(f)
        Assert.AreEqual(json, MigrationFixture04A02.Canonical(JsonNode.Parse(json)))
        Assert.AreEqual("12e25924aae93f0b36ab33937176c86c19641b50", f("source_commit").GetValue(Of String)())
        Dim grouped = MigrationFixture04A02.Grouped(f)
        CollectionAssert.AreEqual(New String() {"A", "B", "C"}, grouped.GroupNames)
        CollectionAssert.AreEqual(New Integer() {3, 3, 3}, grouped.GroupCounts)
        Dim expected = If(id = "C30-SYN-ANOVA", New Double() {1, 2, 3}, New Double() {1, 2, 2})
        CollectionAssert.AreEqual(expected, grouped.Groups(0))
        CollectionAssert.AreEqual(New Double() {2, 4, 6}, grouped.Groups(1))
        CollectionAssert.AreEqual(New Double() {3, 5, 7}, grouped.Groups(2))
        Dim rows = f("dataset")("row_ids").AsArray()
        Dim expectedRows As Integer()() = {New Integer() {101, 103, 107}, New Integer() {109, 113, 127}, New Integer() {131, 137, 139}}
        For i As Integer = 0 To 2
            CollectionAssert.AreEqual(expectedRows(i), rows(i).AsArray().Select(Function(n) n.GetValue(Of Integer)()).ToArray())
        Next
        Assert.IsNull(grouped.GetType().GetProperty("RowIds"), "Provenance must not imply production transport")
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA", "")>
    <DataRow("C30-SYN-KW", "")>
    <DataRow("C30-SYN-ANOVA", "sk-SK")>
    <DataRow("C30-SYN-KW", "sk-SK")>
    Public Sub TypedService_CandidateObservationOnly(id As String, culture As String)
        Dim savedCulture = CultureInfo.CurrentCulture
        Dim savedUiCulture = CultureInfo.CurrentUICulture
        Dim f = MigrationFixture04A02.Load(id)
        Dim before = MigrationFixture04A02.Canonical(f)
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture
            Dim invariant = MigrationFixture04A02.Canonical(MigrationFixture04A02.Observe(f))
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture)
            Dim observation = MigrationFixture04A02.Observe(f)
            Dim actual = MigrationFixture04A02.Canonical(observation)
            Assert.AreEqual(invariant, actual, "Candidate capture must be culture independent")
            Assert.AreEqual(id, observation("case_id").GetValue(Of String)())
            Assert.AreEqual("UNVERIFIED_AGAINST_INDEPENDENT_ORACLE", observation("status").GetValue(Of String)())
            CollectionAssert.AreEqual(New String() {"A", "B", "C"}, observation("group_names").AsArray().Select(Function(n) n.GetValue(Of String)()).ToArray())
            CollectionAssert.AreEqual(New Integer() {3, 3, 3}, observation("group_counts").AsArray().Select(Function(n) n.GetValue(Of Integer)()).ToArray())
            Assert.AreEqual(If(id = "C30-SYN-ANOVA", 2, 4), observation("metrics").AsObject().Count)
            For Each metric In observation("metrics").AsObject()
                Assert.IsTrue(Double.IsFinite(metric.Value.GetValue(Of Double)()))
            Next
            Assert.AreEqual(before, MigrationFixture04A02.Canonical(f))
            ' Only an explicitly configured new run sandbox may receive observations.
            Dim output = Environment.GetEnvironmentVariable("BESHSTAT_04A02_OBSERVATIONS")
            If Not String.IsNullOrEmpty(output) Then
                Dim full = Path.GetFullPath(output)
                Dim evidence = Path.GetFullPath(Path.Combine(MigrationFixture04A02.SourceRoot(), "TestResults")) & Path.DirectorySeparatorChar
                Assert.IsTrue(full.StartsWith(evidence, StringComparison.OrdinalIgnoreCase))
                Dim runName = Path.GetRelativePath(evidence, full).Split(Path.DirectorySeparatorChar)(0)
                Assert.IsTrue(runName.StartsWith("04A-02-initial-fixtures-", StringComparison.Ordinal))
                Directory.CreateDirectory(full)
                Dim name = id & "-" & If(culture = "", "invariant", culture) & ".candidate.json"
                Using stream As New FileStream(Path.Combine(full, name), FileMode.CreateNew, FileAccess.Write)
                    Using writer As New StreamWriter(stream, New System.Text.UTF8Encoding(False))
                        writer.Write(actual & vbLf)
                    End Using
                End Using
            End If
        Finally
            CultureInfo.CurrentCulture = savedCulture
            CultureInfo.CurrentUICulture = savedUiCulture
        End Try
    End Sub

    <TestMethod>
    Public Sub FixtureSet_RejectsDuplicateCaseId()
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.ValidateSet({MigrationFixture04A02.Load("C30-SYN-ANOVA"), MigrationFixture04A02.Load("C30-SYN-ANOVA")}))
        MigrationFixture04A02.ValidateSet(MigrationFixture04A02.CaseIds.Select(Function(id) MigrationFixture04A02.Load(id)))
    End Sub

    <DataTestMethod>
    <DataRow("row-length")> <DataRow("nonfinite")> <DataRow("missing-label")>
    <DataRow("alpha-zero")> <DataRow("alpha-one")> <DataRow("alpha-policy")>
    <DataRow("reference-metadata")> <DataRow("output-status")> <DataRow("reference-value")>
    <DataRow("native")> <DataRow("source-hash")> <DataRow("source-path")>
    <DataRow("dataset-hash")> <DataRow("options")> <DataRow("schema")>
    <DataRow("absent-reference-metadata")> <DataRow("missing-table-schema")> <DataRow("method")>
    Public Sub Fixture_RejectsInvalidAdmission(defect As String)
        Dim f = MigrationFixture04A02.Load("C30-SYN-ANOVA")
        Select Case defect
            Case "row-length" : f("dataset")("row_ids")(0).AsArray().RemoveAt(0)
            Case "nonfinite" : f("dataset")("values")(0)(0) = JsonNode.Parse("1e400")
            Case "missing-label" : f("dataset")("group_labels")(0) = JsonValue.Create("")
            Case "alpha-zero" : f("alpha") = JsonValue.Create(0.0)
            Case "alpha-one" : f("alpha") = JsonValue.Create(1.0)
            Case "alpha-policy" : f("alpha") = JsonValue.Create(0.01)
            Case "reference-metadata" : f("reference_provenance").AsObject().Remove("version")
            Case "output-status" : f("output_status") = JsonValue.Create("APPROVED_GOLDEN")
            Case "reference-value" : f("expected_numerical_reference") = JsonValue.Create(1.0)
            Case "native" : f("live_control_capture") = JsonValue.Create(True)
            Case "source-hash" : f("source_pins")(0)("sha256") = JsonValue.Create(New String("0"c, 64))
            Case "source-path" : f("source_pins")(0)("path") = JsonValue.Create("../outside")
            Case "dataset-hash" : f("dataset_sha256") = JsonValue.Create(New String("0"c, 64))
            Case "options" : f("options")("IncludeWelch") = JsonValue.Create(True)
            Case "schema" : f("schema") = JsonValue.Create("unrecognized")
            Case "absent-reference-metadata" : f.Remove("reference_provenance")
            Case "missing-table-schema" : f.Remove("expected_table_schema")
            Case "method" : f("method") = JsonValue.Create("Welch")
        End Select
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Validate(f), defect)
    End Sub

    <TestMethod>
    Public Sub KruskalWallis_RejectsImplicitDunnDefault()
        Dim f = MigrationFixture04A02.Load("C30-SYN-KW")
        f("options")("IncludeDunnComparisons") = JsonValue.Create(True)
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Validate(f))
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA")> <DataRow("C30-SYN-KW")>
    Public Sub Fixture_RejectsRehashedDatasetDrift(id As String)
        Dim f = MigrationFixture04A02.Load(id)
        f("dataset")("values")(0)(0) = JsonValue.Create(99.0)
        f("dataset_sha256") = JsonValue.Create(MigrationFixture04A02.Hash(MigrationFixture04A02.Canonical(f("dataset"))))
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Validate(f))
    End Sub

    <TestMethod>
    Public Sub CanonicalSerialization_SortsKeysAndRejectsNonfinite()
        Assert.AreEqual("{""a"":1,""b"":[2,3]}", MigrationFixture04A02.Canonical(JsonNode.Parse("{""b"":[2,3],""a"":1}")))
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Canonical(JsonNode.Parse("[1e400]")))
        Assert.ThrowsException(Of ArgumentException)(Sub() JsonSerializer.Serialize(New Double() {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}))
    End Sub

    <DataTestMethod>
    <DataRow("2147483648")> <DataRow("-2147483649")>
    <DataRow("9007199254740992")> <DataRow("9007199254740993")>
    <DataRow("101.0")> <DataRow("1.01e2")> <DataRow("null")>
    Public Sub ProvenanceRowIds_RejectOutOfRangeAndFloatingAliases(token As String)
        Dim f = MigrationFixture04A02.Load("C30-SYN-ANOVA")
        f("dataset")("row_ids")(0)(0) = JsonNode.Parse(token)
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Validate(f))
    End Sub

    <TestMethod>
    Public Sub CanonicalIntegers_AreExactBeyondBinary64AndBoundedAtInt64()
        Assert.AreEqual("9007199254740992", MigrationFixture04A02.Canonical(JsonNode.Parse("9007199254740992")))
        Assert.AreEqual("9007199254740993", MigrationFixture04A02.Canonical(JsonNode.Parse("9007199254740993")))
        Assert.AreEqual("9223372036854775807", MigrationFixture04A02.Canonical(JsonNode.Parse("9223372036854775807")))
        Assert.AreEqual("-9223372036854775808", MigrationFixture04A02.Canonical(JsonNode.Parse("-9223372036854775808")))
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Canonical(JsonNode.Parse("9223372036854775808")))
        Assert.ThrowsException(Of InvalidDataException)(Sub() MigrationFixture04A02.Canonical(JsonNode.Parse("-9223372036854775809")))
        ' Decimal/exponent tokens deliberately retain the existing binary64 policy.
        Assert.AreEqual("1", MigrationFixture04A02.Canonical(JsonNode.Parse("1.0")))
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA", "98dce529f444e6ede2f5f8e34ef072cd7407efd2a3a25597d2d1504c6b4f6cba")>
    <DataRow("C30-SYN-KW", "7e156caca9ded21a8e0967d5755ec40143a11542b2094cc03ebe7474400e120e")>
    Public Sub ExactIntegerSafety_PreservesFrozenDatasetHashes(id As String, expected As String)
        Dim f = MigrationFixture04A02.Load(id)
        Assert.AreEqual(expected, MigrationFixture04A02.Hash(MigrationFixture04A02.Canonical(f("dataset"))))
        Assert.AreEqual(expected, f("dataset_sha256").GetValue(Of String)())
    End Sub

    <DataTestMethod>
    <DataRow("C30-SYN-ANOVA")> <DataRow("C30-SYN-KW")>
    Public Sub Observe_PreservesEveryServiceCellAndPValueMetadata(id As String)
        Dim f = MigrationFixture04A02.Load(id)
        Dim observation = MigrationFixture04A02.Observe(f)
        Dim tables As List(Of Global.BESHStatNG.ResultTableOutputModel)
        If id = "C30-SYN-ANOVA" Then
            Dim result = OneWayAnovaApplicationService.Run(MigrationFixture04A02.Grouped(f), New OneWayAnovaAnalysisOptions With {
                .Alpha = 0.05, .IncludeWelch = False, .IncludeFisherLsd = False, .IncludeBonferroni = False,
                .IncludeTukeyKramer = False, .IncludeGamesHowell = False})
            tables = result.Tables
            Assert.AreEqual(result.FStatistic, observation("metrics")("FStatistic").GetValue(Of Double)())
            Assert.AreEqual(result.PValue, observation("metrics")("PValue").GetValue(Of Double)())
        Else
            Dim result = KruskalWallisApplicationService.Run(MigrationFixture04A02.Grouped(f), New KruskalWallisAnalysisOptions With {
                .Alpha = 0.05, .IncludeDunnComparisons = False})
            tables = result.Tables
            Assert.AreEqual(result.HStatistic, observation("metrics")("HStatistic").GetValue(Of Double)())
            Assert.AreEqual(result.TieCorrectedHStatistic, observation("metrics")("TieCorrectedHStatistic").GetValue(Of Double)())
            Assert.AreEqual(result.PValue, observation("metrics")("PValue").GetValue(Of Double)())
            Assert.AreEqual(result.TieCorrectedPValue, observation("metrics")("TieCorrectedPValue").GetValue(Of Double)())
        End If
        Assert.AreEqual(tables.Count, observation("tables").AsArray().Count)
        For index As Integer = 0 To tables.Count - 1
            Dim model = tables(index)
            Dim captured = observation("tables")(index)
            Assert.AreEqual(model.RowCount, captured("rows").GetValue(Of Integer)())
            Assert.AreEqual(model.ColumnCount, captured("columns").GetValue(Of Integer)())
            Assert.AreEqual(model.HeaderTopRows, captured("header_top_rows").GetValue(Of Integer)())
            Assert.AreEqual(model.HeaderLeftColumns, captured("header_left_columns").GetValue(Of Integer)())
            Assert.AreEqual(model.FooterRows, captured("footer_rows").GetValue(Of Integer)())
            Assert.AreEqual(model.TitleRows, captured("title_rows").GetValue(Of Integer)())
            Assert.AreEqual(model.IsResultTable, captured("is_result_table").GetValue(Of Boolean)())
            Assert.AreEqual(MigrationFixture04A02.Canonical(JsonSerializer.SerializeToNode(model.PvalueColumns)), MigrationFixture04A02.Canonical(captured("pvalue_columns")))
            Assert.AreEqual(MigrationFixture04A02.Canonical(JsonSerializer.SerializeToNode(model.PvalueCells)), MigrationFixture04A02.Canonical(captured("pvalue_cells")))
            For row As Integer = 0 To model.RowCount - 1
                For column As Integer = 0 To model.ColumnCount - 1
                    Dim value = model.Values(row, column)
                    Dim cell = captured("cells")(row)(column)
                    Assert.AreEqual(If(value Is Nothing, "null", value.GetType().FullName), cell("type").GetValue(Of String)())
                    Assert.AreEqual(MigrationFixture04A02.Canonical(JsonSerializer.SerializeToNode(value)), MigrationFixture04A02.Canonical(cell("value")))
                Next
            Next
        Next
        Assert.AreEqual("UNVERIFIED_AGAINST_INDEPENDENT_ORACLE", observation("status").GetValue(Of String)())
        Assert.IsNull(f("expected_numerical_reference"))
        Assert.AreEqual("NOT_APPROVED", f("reference_status").GetValue(Of String)())
    End Sub
End Class
