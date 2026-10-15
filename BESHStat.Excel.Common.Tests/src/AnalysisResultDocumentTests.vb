Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Batch 00.04: document artifacts, diagnostics, serialization and legacy parity.</summary>
<TestClass>
Public Class AnalysisResultDocumentTests

    Private NotInheritable Class UnsupportedInput
        Inherits SpreadsheetAnalysisInput
    End Class

    <TestMethod>
    Public Sub AllFiveGoldenResults_PreserveTableOrderMetricsAndTypedDetail()
        For Each fixtureName As String In New String() {
            "anova-base", "anova-welch-tukey", "anova-all-posthoc", "kw-base", "kw-dunn"
        }
            Using json As JsonDocument = JsonDocument.Parse(File.ReadAllText(
                Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", fixtureName & ".json")))
                Dim fixture As JsonElement = json.RootElement
                Dim id As String = fixture.GetProperty("analysisIdProposed").GetString()
                Dim input As GroupedNumericData = ParseGroups(fixture.GetProperty("groups"))
                Dim options As SpreadsheetAnalysisOptions = ParseOptions(id, fixture.GetProperty("options"))
                Dim legacy As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(id, input, options)
                Dim doc As AnalysisResultDocument = legacy.Document

                Assert.AreEqual(id, doc.AnalysisId, fixtureName)
                Assert.AreEqual(AnalysisDocumentStatus.Succeeded, doc.Status, fixtureName)
                Assert.AreEqual(0, doc.Faults.Count, fixtureName)
                Assert.AreEqual(0, doc.Warnings.Count, fixtureName)
                Assert.AreEqual(legacy.Tables.Count, doc.Tables.Count, fixtureName)
                Assert.AreEqual(legacy.Metrics.Count, doc.Metrics.Count, fixtureName)
                Assert.AreEqual(legacy.Tables.Count + legacy.Metrics.Count + 1, doc.Artifacts.Count, fixtureName)

                For t As Integer = 0 To legacy.Tables.Count - 1
                    Dim actual As Global.BESHStatNG.ResultTableOutputModel = doc.Tables(t).Model
                    Dim expected As Global.BESHStatNG.ResultTableOutputModel = legacy.Tables(t)
                    Assert.AreEqual(t, doc.Tables(t).Index, fixtureName)
                    Assert.AreEqual(AnalysisArtifactKind.Table, doc.Artifacts(t).Kind, fixtureName)
                    Assert.AreEqual(expected.RowCount, actual.RowCount, fixtureName)
                    Assert.AreEqual(expected.ColumnCount, actual.ColumnCount, fixtureName)
                    Assert.AreEqual(expected.TitleRows, actual.TitleRows, fixtureName)
                    Assert.AreEqual(expected.HeaderTopRows, actual.HeaderTopRows, fixtureName)
                    Assert.AreEqual(expected.FooterRows, actual.FooterRows, fixtureName)
                    CollectionAssert.AreEqual(expected.PvalueColumns, actual.PvalueColumns, fixtureName)
                    Assert.AreEqual(expected.PvalueCells.Count, actual.PvalueCells.Count, fixtureName)
                    For cellIndex As Integer = 0 To expected.PvalueCells.Count - 1
                        Assert.AreEqual(expected.PvalueCells(cellIndex).BodyRow, actual.PvalueCells(cellIndex).BodyRow, fixtureName)
                        Assert.AreEqual(expected.PvalueCells(cellIndex).BodyColumn, actual.PvalueCells(cellIndex).BodyColumn, fixtureName)
                    Next
                    For r As Integer = 0 To expected.RowCount - 1
                        For c As Integer = 0 To expected.ColumnCount - 1
                            Assert.AreEqual(expected.Values(r, c), actual.Values(r, c), fixtureName & "/table " & t.ToString())
                        Next
                    Next
                Next
                For Each metric As AnalysisMetricArtifact In doc.Metrics
                    Assert.AreEqual(legacy.Metrics(metric.Name), metric.Value, 0.000000000001R, fixtureName & "/" & metric.Name)
                    Assert.AreEqual(If(IsPValue(metric.Name), AnalysisMetricRole.PValue, AnalysisMetricRole.Statistic), metric.Role)
                Next
                If id = AnalysisIds.OneWayAnova Then
                    Dim detail As OneWayAnovaApplicationResult = Nothing
                    Assert.IsTrue(doc.TryGetDetail(detail), fixtureName)
                    Assert.AreSame(legacy.Detail, detail)
                    Dim wrong As KruskalWallisApplicationResult = Nothing
                    Assert.IsFalse(doc.TryGetDetail(wrong))
                    Assert.IsNull(wrong)
                Else
                    Dim detail As KruskalWallisApplicationResult = Nothing
                    Assert.IsTrue(doc.TryGetDetail(detail), fixtureName)
                    Assert.AreSame(legacy.Detail, detail)
                End If
                Dim direct As AnalysisResultDocument = SharedAnalysisApplicationService.RunDocument(id, input, options)
                Assert.AreEqual(doc.Tables.Count, direct.Tables.Count, fixtureName)
                Assert.AreEqual(doc.Metrics.Count, direct.Metrics.Count, fixtureName)
            End Using
        Next
    End Sub

    <TestMethod>
    Public Sub Document_TableAndCollectionAreDefensivelyCaptured()
        Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, BuildGroups())
        Dim doc As AnalysisResultDocument = result.Document
        Dim original As Object = doc.Tables(0).Model.Values(0, 0)
        result.Tables(0).Values(0, 0) = "changed by legacy host"
        Assert.AreEqual(original, doc.Tables(0).Model.Values(0, 0))
        Dim copy As Global.BESHStatNG.ResultTableOutputModel = doc.Tables(0).Model
        copy.Values(0, 0) = "changed by document consumer"
        copy.PvalueColumns.Clear()
        Assert.AreEqual(original, doc.Tables(0).Model.Values(0, 0))
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(doc.Artifacts, IList(Of AnalysisResultArtifact)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(doc.Tables, IList(Of AnalysisTableArtifact)).Clear())
    End Sub

    <TestMethod>
    Public Sub LegacyConstructor_FourParameterClrSignatureIsPreserved()
        Dim originalSignature As Type() = {
            GetType(SpreadsheetAnalysisKind),
            GetType(List(Of Global.BESHStatNG.ResultTableOutputModel)),
            GetType(Dictionary(Of String, Double)),
            GetType(Object)
        }
        Assert.IsNotNull(GetType(SpreadsheetAnalysisResult).GetConstructor(originalSignature))
    End Sub

    <TestMethod>
    Public Sub InvalidAlpha_ProducesWarningWithoutChangingCalculation()
        Dim grouped As GroupedNumericData = BuildGroups()
        Dim standard As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, grouped)
        Dim invalid As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(
            AnalysisIds.OneWayAnova, grouped, New OneWayAnovaAnalysisOptions With {.Alpha = Double.NaN})
        Assert.AreEqual(1, invalid.Document.Warnings.Count)
        Assert.AreEqual(AnalysisWarningCodes.AlphaDefaulted, invalid.Document.Warnings(0).Code)
        Assert.AreEqual("options.alpha", invalid.Document.Warnings(0).FieldPath)
        Assert.AreEqual(standard.Metrics(AnalysisMetricNames.FStatistic), invalid.Metrics(AnalysisMetricNames.FStatistic), 0.000000000001R)
        Assert.AreEqual(standard.Tables.Count, invalid.Tables.Count)

        Dim kw As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(
            AnalysisIds.KruskalWallis, grouped, New KruskalWallisAnalysisOptions With {.Alpha = 2.0})
        Assert.AreEqual(AnalysisWarningCodes.AlphaDefaulted, kw.Document.Warnings(0).Code)
        Assert.AreEqual(AnalysisDocumentStatus.Succeeded, kw.Document.Status)
    End Sub

    <TestMethod>
    Public Sub RunDocument_MapsExpectedFaultsButRunRemainsThrowing()
        Dim unsupported As AnalysisResultDocument = SharedAnalysisApplicationService.RunDocument("not.registered", BuildGroups())
        Assert.AreEqual(AnalysisDocumentStatus.Failed, unsupported.Status)
        Assert.AreEqual(AnalysisFaultCodes.UnsupportedAnalysis, unsupported.Faults(0).Code)
        Assert.AreEqual(0, unsupported.Tables.Count)
        Assert.AreEqual(0, unsupported.Metrics.Count)
        Dim invalid As AnalysisResultDocument = SharedAnalysisApplicationService.RunDocument(
            AnalysisIds.OneWayAnova, New UnsupportedInput())
        Assert.AreEqual(AnalysisFaultCodes.InvalidRequest, invalid.Faults(0).Code)
        Dim empty As AnalysisResultDocument = SharedAnalysisApplicationService.RunDocument("", BuildGroups())
        Assert.AreEqual("analysis.unknown", empty.AnalysisId)
        Assert.AreEqual(AnalysisFaultCodes.InvalidRequest, empty.Faults(0).Code)
        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run("not.registered", BuildGroups()))
    End Sub

    <TestMethod>
    Public Sub ContractSnapshot_RoundTripsWithoutSerializingRawDetailOrCells()
        Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(
            AnalysisIds.KruskalWallis, BuildGroups(), New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = True})
        Dim snapshot As AnalysisResultDocumentSnapshot = result.Document.ToContractSnapshot()
        Dim serialized As String = JsonSerializer.Serialize(snapshot)
        Dim restored As AnalysisResultDocumentSnapshot = JsonSerializer.Deserialize(Of AnalysisResultDocumentSnapshot)(serialized)
        Assert.IsNotNull(restored)
        Assert.AreEqual(1, restored.SchemaVersion)
        Assert.AreEqual(AnalysisIds.KruskalWallis, restored.AnalysisId)
        Assert.AreEqual(result.Tables.Count, restored.Tables.Count)
        Assert.AreEqual(result.Metrics.Count, restored.Metrics.Count)
        For i As Integer = 0 To restored.Tables.Count - 1
            Dim expected As Global.BESHStatNG.ResultTableOutputModel = result.Tables(i)
            Assert.AreEqual(expected.PvalueColumns.Count, restored.Tables(i).PvalueColumns.Count)
            Assert.AreEqual(expected.PvalueCells.Count, restored.Tables(i).PvalueCells.Count)
            For j As Integer = 0 To expected.PvalueCells.Count - 1
                Assert.AreEqual(expected.PvalueCells(j).BodyRow, restored.Tables(i).PvalueCells(j).BodyRow)
                Assert.AreEqual(expected.PvalueCells(j).BodyColumn, restored.Tables(i).PvalueCells(j).BodyColumn)
            Next
        Next
        Assert.AreEqual(GetType(KruskalWallisApplicationResult).FullName, restored.DetailTypeName)
        Assert.IsFalse(serialized.Contains("GroupNames", StringComparison.Ordinal))
        Assert.IsFalse(serialized.Contains("Values", StringComparison.Ordinal))
        Assert.IsFalse(serialized.Contains("SpreadsheetAnalysisResult", StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub Snapshot_PreservesNonFiniteScalarClassificationAsJsonSafeValues()
        Dim metric As New Dictionary(Of String, Double) From {
            {"not-a-number", Double.NaN}, {"positive", Double.PositiveInfinity},
            {"negative", Double.NegativeInfinity}, {AnalysisMetricNames.PValue, 0.025}
        }
        Dim doc As New AnalysisResultDocument(AnalysisIds.OneWayAnova,
            Array.Empty(Of Global.BESHStatNG.ResultTableOutputModel)(), metric, Nothing)
        Dim snapshot As AnalysisResultDocumentSnapshot = doc.ToContractSnapshot()
        Dim serialized As String = JsonSerializer.Serialize(snapshot)
        Dim restored As AnalysisResultDocumentSnapshot = JsonSerializer.Deserialize(Of AnalysisResultDocumentSnapshot)(serialized)
        Assert.AreEqual("NaN", restored.Metrics.First(Function(m) m.Name = "not-a-number").NumericKind)
        Assert.IsFalse(restored.Metrics.First(Function(m) m.Name = "not-a-number").Value.HasValue)
        Assert.AreEqual("PositiveInfinity", restored.Metrics.First(Function(m) m.Name = "positive").NumericKind)
        Assert.AreEqual("NegativeInfinity", restored.Metrics.First(Function(m) m.Name = "negative").NumericKind)
        Assert.AreEqual(AnalysisMetricRole.PValue, restored.Metrics.First(Function(m) m.Name = AnalysisMetricNames.PValue).Role)
    End Sub

    <TestMethod>
    Public Sub FaultAndWarningSnapshot_RoundTripsCodesAndFieldPaths()
        Dim warnings As AnalysisWarning() = {New AnalysisWarning("data.omitted", "Rows were excluded.", "input.rows")}
        Dim faults As AnalysisFault() = {New AnalysisFault("fit.failed", "Computation did not converge.", "model")}
        Dim doc As New AnalysisResultDocument(AnalysisIds.OneWayAnova,
            Array.Empty(Of Global.BESHStatNG.ResultTableOutputModel)(),
            Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, warnings, faults)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, doc.Status)
        Dim json As String = JsonSerializer.Serialize(doc.ToContractSnapshot())
        Dim restored As AnalysisResultDocumentSnapshot = JsonSerializer.Deserialize(Of AnalysisResultDocumentSnapshot)(json)
        Assert.AreEqual("data.omitted", restored.Warnings(0).Code)
        Assert.AreEqual("input.rows", restored.Warnings(0).FieldPath)
        Assert.AreEqual("fit.failed", restored.Faults(0).Code)
    End Sub

    <TestMethod>
    Public Sub Document_RejectsNullsAndInvalidIssueContracts()
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim unused As New AnalysisWarning("", "message")
            End Sub)
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim unused As New AnalysisFault("code", "")
            End Sub)
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim unused As New AnalysisResultDocument("",
                    Array.Empty(Of Global.BESHStatNG.ResultTableOutputModel)(),
                    Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing)
            End Sub)
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub()
                Dim unused As New AnalysisResultDocument(AnalysisIds.OneWayAnova, Nothing,
                    Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing)
            End Sub)
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() AnalysisResultDocument.Failure(AnalysisIds.OneWayAnova, Nothing))
    End Sub

    Private Shared Function IsPValue(name As String) As Boolean
        Return name = AnalysisMetricNames.PValue OrElse name = AnalysisMetricNames.WelchPValue OrElse
               name = AnalysisMetricNames.TieCorrectedPValue
    End Function

    Private Shared Function BuildGroups() As GroupedNumericData
        Return New GroupedNumericData(New String() {"A", "B", "C"}, New Double()() {
            New Double() {1.0, 2.0, 2.0, 3.0},
            New Double() {2.0, 3.0, 4.0, 4.0},
            New Double() {4.0, 5.0, 5.0, 6.0}})
    End Function

    Private Shared Function ParseGroups(raw As JsonElement) As GroupedNumericData
        Dim rawNames As JsonElement = raw.GetProperty("names")
        Dim rawValues As JsonElement = raw.GetProperty("values")
        Dim names(rawNames.GetArrayLength() - 1) As String
        Dim groups(rawValues.GetArrayLength() - 1)() As Double
        For i As Integer = 0 To names.Length - 1
            names(i) = rawNames(i).GetString()
            Dim values As New List(Of Double)()
            For Each val As JsonElement In rawValues(i).EnumerateArray()
                values.Add(val.GetDouble())
            Next
            groups(i) = values.ToArray()
        Next
        Return New GroupedNumericData(names, groups)
    End Function

    Private Shared Function ParseOptions(id As String, raw As JsonElement) As SpreadsheetAnalysisOptions
        If id = AnalysisIds.OneWayAnova Then
            Return New OneWayAnovaAnalysisOptions With {
                .IncludeWelch = GetFlag(raw, "welch"),
                .IncludeFisherLsd = GetFlag(raw, "fisherLsd"),
                .IncludeBonferroni = GetFlag(raw, "bonferroni"),
                .IncludeTukeyKramer = GetFlag(raw, "tukeyKramer"),
                .IncludeGamesHowell = GetFlag(raw, "gamesHowell")}
        End If
        Return New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = GetFlag(raw, "dunn")}
    End Function

    Private Shared Function GetFlag(raw As JsonElement, name As String) As Boolean
        Dim propertyValue As JsonElement
        Return raw.TryGetProperty(name, propertyValue) AndAlso propertyValue.GetBoolean()
    End Function
End Class
