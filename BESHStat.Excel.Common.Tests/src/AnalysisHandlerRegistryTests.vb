Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Batch 00.03: typed handlers, input/option checks, legacy compatibility and 00.01 golden parity.</summary>
<TestClass>
Public Class AnalysisHandlerRegistryTests

    Private NotInheritable Class UnsupportedInput
        Inherits SpreadsheetAnalysisInput
    End Class

    <TestMethod>
    Public Sub Registry_CoversEveryImplementedDescriptorAndBothInputShapes()
        AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, AnalysisHandlerRegistry.All)
        Assert.AreEqual(4, AnalysisHandlerRegistry.All.Count)
        Assert.AreEqual(2, AnalysisCatalog.All.Count)
        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Dim matches As IAnalysisHandler() = AnalysisHandlerRegistry.All.
                Where(Function(h) String.Equals(h.AnalysisId, descriptor.Id, StringComparison.Ordinal)).ToArray()
            Assert.AreEqual(2, matches.Length, descriptor.Id)
            For Each h As IAnalysisHandler In matches
                Assert.AreEqual(descriptor.OptionsType, h.OptionsType)
                Assert.IsTrue(descriptor.InputTypes.Contains(h.InputType))
            Next
        Next
        Assert.IsInstanceOfType(AnalysisHandlerRegistry.All(0),
                                GetType(TypedAnalysisHandler(Of SpreadsheetRangeData, OneWayAnovaAnalysisOptions, OneWayAnovaApplicationResult)))
        Assert.IsInstanceOfType(AnalysisHandlerRegistry.All(3),
                                GetType(TypedAnalysisHandler(Of GroupedNumericData, KruskalWallisAnalysisOptions, KruskalWallisApplicationResult)))
        Assert.ThrowsException(Of NotSupportedException)(
            Sub() DirectCast(AnalysisHandlerRegistry.All, IList(Of IAnalysisHandler)).Clear())
    End Sub

    <TestMethod>
    Public Sub StableIdAndLegacyEnum_ProduceIdenticalResultsForFiveGoldenFixtures()
        For Each fixtureName As String In New String() {
            "anova-base", "anova-welch-tukey", "anova-all-posthoc", "kw-base", "kw-dunn"
        }
            Dim path As String = System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", fixtureName & ".json")
            Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(path))
                Dim fixture As JsonElement = doc.RootElement
                Dim id As String = fixture.GetProperty("analysisIdProposed").GetString()
                Dim kind As SpreadsheetAnalysisKind = CType(fixture.GetProperty("legacyEnumValue").GetInt32(), SpreadsheetAnalysisKind)
                Dim input As GroupedNumericData = ParseGroups(fixture.GetProperty("groups"))
                Dim options As SpreadsheetAnalysisOptions = ParseOptions(kind, fixture.GetProperty("options"))

                Dim oldResult As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(
                    New SpreadsheetAnalysisRequest(kind, input, options))
                Dim newResult As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(id, input, options)
                CompareResults(oldResult, newResult, fixtureName)
                Assert.AreEqual(kind, newResult.AnalysisKind)
            End Using
        Next
    End Sub

    <TestMethod>
    Public Sub Registry_SupportsBothInputShapesForEachAnalysis()
        Dim range As SpreadsheetRangeData = BuildRange()
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(range)
        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Dim options As SpreadsheetAnalysisOptions = descriptor.CreateDefaultOptions()
            Dim fromRange As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(descriptor.Id, range, options)
            Dim fromGrouped As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(descriptor.Id, grouped, options)
            CompareResults(fromRange, fromGrouped, descriptor.Id)
        Next
    End Sub

    <TestMethod>
    Public Sub Registry_NullOptionsPreserveAnalysisSpecificDefaults()
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(BuildRange())
        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Dim withNull As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(descriptor.Id, grouped)
            Dim withDefaults As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(descriptor.Id, grouped, descriptor.CreateDefaultOptions())
            CompareResults(withNull, withDefaults, descriptor.Id)
        Next
    End Sub

    <TestMethod>
    Public Sub Registry_RejectsUnknownIdsWrongTypesAndNullsBeforeRunningCore()
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(BuildRange())
        Assert.ThrowsException(Of ArgumentNullException)(
            Function() SharedAnalysisApplicationService.Run(CType(Nothing, SpreadsheetAnalysisRequest)))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest()))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run("", grouped))
        Assert.ThrowsException(Of ArgumentNullException)(
            Function() SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, CType(Nothing, SpreadsheetAnalysisInput)))
        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run("ANOVA.ONEWAY", grouped))
        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run("not.registered", grouped))
        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(CType(999, SpreadsheetAnalysisKind), grouped, Nothing)))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, New UnsupportedInput()))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run(AnalysisIds.OneWayAnova, grouped, New KruskalWallisAnalysisOptions()))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run(AnalysisIds.KruskalWallis, grouped, New OneWayAnovaAnalysisOptions()))
        Assert.ThrowsException(Of ArgumentException)(
            Function() SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(SpreadsheetAnalysisKind.OneWayAnova, grouped, New KruskalWallisAnalysisOptions())))
    End Sub

    <TestMethod>
    Public Sub TypedHandlerBoundary_RejectsWrongInputAndOptionTypes()
        Dim handler As IAnalysisHandler = AnalysisHandlerRegistry.All(0)
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(BuildRange())
        Assert.ThrowsException(Of ArgumentNullException)(
            Function() handler.Execute(Nothing, Nothing))
        Assert.ThrowsException(Of ArgumentException)(
            Function() handler.Execute(grouped, New OneWayAnovaAnalysisOptions()))
        Assert.ThrowsException(Of ArgumentException)(
            Function() handler.Execute(BuildRange(), New KruskalWallisAnalysisOptions()))
    End Sub

    <TestMethod>
    Public Sub Validator_RejectsDuplicateIncompleteAndOrphanRegistrations()
        Dim current As New List(Of IAnalysisHandler)(AnalysisHandlerRegistry.All)
        current.Add(AnalysisHandlerRegistry.All(0))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, current))

        Dim incomplete As New List(Of IAnalysisHandler)(AnalysisHandlerRegistry.All)
        incomplete.RemoveAt(0)
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, incomplete))

        Dim orphan As New List(Of IAnalysisHandler)(AnalysisHandlerRegistry.All)
        orphan.Add(New TypedAnalysisHandler(Of GroupedNumericData, OneWayAnovaAnalysisOptions, OneWayAnovaApplicationResult)(
                   "not.registered", Function(input, options) CType(Nothing, OneWayAnovaApplicationResult),
                   Function(detail) CType(Nothing, SpreadsheetAnalysisResult)))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, orphan))

        Dim mismatch As New List(Of IAnalysisHandler)(AnalysisHandlerRegistry.All)
        mismatch(0) = New TypedAnalysisHandler(Of SpreadsheetRangeData, KruskalWallisAnalysisOptions, KruskalWallisApplicationResult)(
            AnalysisIds.OneWayAnova, Function(input, options) CType(Nothing, KruskalWallisApplicationResult),
            Function(detail) CType(Nothing, SpreadsheetAnalysisResult))
        Assert.ThrowsException(Of ArgumentException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, mismatch))
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(Nothing, AnalysisHandlerRegistry.All))
        Assert.ThrowsException(Of ArgumentNullException)(
            Sub() AnalysisHandlerRegistry.ValidateRegistrations(AnalysisCatalog.All, Nothing))
    End Sub

    <TestMethod>
    Public Sub DocumentationOnlyMethodsCannotDispatch()
        Assert.AreEqual(76, GeneratedMethodMetadata.All.Count)
        Assert.AreEqual(2, AnalysisCatalog.All.Count)
        Dim docsOnly As AnalysisMethodMetadata = GeneratedMethodMetadata.All.First(Function(m) m.AnalysisId Is Nothing)
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(BuildRange())
        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run(docsOnly.Key, grouped))
    End Sub

    Private Shared Function BuildRange() As SpreadsheetRangeData
        Dim data As New SpreadsheetRangeData With {
            .WorksheetName = "Source", .Address = "A1:C5", .RowCount = 5, .ColumnCount = 3
        }
        data.Rows.Add(New List(Of SpreadsheetCellValue) From {
            SpreadsheetCellValue.TextCell("A"), SpreadsheetCellValue.TextCell("B"), SpreadsheetCellValue.TextCell("C")})
        For Each item As Double() In New Double()() {
            New Double() {1, 2, 4}, New Double() {2, 3, 5}, New Double() {2, 4, 5}, New Double() {3, 4, 6}
        }
            data.Rows.Add(New List(Of SpreadsheetCellValue) From {
                SpreadsheetCellValue.NumberCell(item(0)), SpreadsheetCellValue.NumberCell(item(1)), SpreadsheetCellValue.NumberCell(item(2))})
        Next
        Return data
    End Function

    Private Shared Function ParseGroups(raw As JsonElement) As GroupedNumericData
        Dim rawNames As JsonElement = raw.GetProperty("names")
        Dim rawValues As JsonElement = raw.GetProperty("values")
        Dim names(rawNames.GetArrayLength() - 1) As String
        Dim groups(rawValues.GetArrayLength() - 1)() As Double
        For i As Integer = 0 To names.Length - 1
            names(i) = rawNames(i).GetString()
            Dim values As New List(Of Double)()
            For Each value As JsonElement In rawValues(i).EnumerateArray()
                values.Add(value.GetDouble())
            Next
            groups(i) = values.ToArray()
        Next
        Return New GroupedNumericData(names, groups)
    End Function

    Private Shared Function ParseOptions(kind As SpreadsheetAnalysisKind, raw As JsonElement) As SpreadsheetAnalysisOptions
        If kind = SpreadsheetAnalysisKind.OneWayAnova Then
            Return New OneWayAnovaAnalysisOptions With {
                .IncludeWelch = Flag(raw, "welch"),
                .IncludeFisherLsd = Flag(raw, "fisherLsd"),
                .IncludeBonferroni = Flag(raw, "bonferroni"),
                .IncludeTukeyKramer = Flag(raw, "tukeyKramer"),
                .IncludeGamesHowell = Flag(raw, "gamesHowell")
            }
        End If
        Return New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = Flag(raw, "dunn")}
    End Function

    Private Shared Function Flag(raw As JsonElement, name As String) As Boolean
        Dim propertyValue As JsonElement
        Return raw.TryGetProperty(name, propertyValue) AndAlso propertyValue.GetBoolean()
    End Function

    Private Shared Sub CompareResults(left As SpreadsheetAnalysisResult, right As SpreadsheetAnalysisResult, description As String)
        Assert.AreEqual(left.AnalysisKind, right.AnalysisKind, description)
        Assert.AreEqual(left.Detail.GetType(), right.Detail.GetType(), description)
        Assert.AreEqual(left.Metrics.Count, right.Metrics.Count, description)
        For Each pair As KeyValuePair(Of String, Double) In left.Metrics
            Assert.IsTrue(right.Metrics.ContainsKey(pair.Key), description & "/" & pair.Key)
            Assert.AreEqual(pair.Value, right.Metrics(pair.Key), 0.000000000001R, description & "/" & pair.Key)
        Next
        Assert.AreEqual(left.Tables.Count, right.Tables.Count, description)
        For t As Integer = 0 To left.Tables.Count - 1
            Dim a As Global.BESHStatNG.ResultTableOutputModel = left.Tables(t)
            Dim b As Global.BESHStatNG.ResultTableOutputModel = right.Tables(t)
            Assert.AreEqual(a.RowCount, b.RowCount, description)
            Assert.AreEqual(a.ColumnCount, b.ColumnCount, description)
            Assert.AreEqual(a.TitleRows, b.TitleRows, description)
            Assert.AreEqual(a.HeaderTopRows, b.HeaderTopRows, description)
            Assert.AreEqual(a.HeaderLeftColumns, b.HeaderLeftColumns, description)
            Assert.AreEqual(a.FooterRows, b.FooterRows, description)
            CollectionAssert.AreEqual(a.PvalueColumns, b.PvalueColumns, description)
            Assert.AreEqual(a.PvalueCells.Count, b.PvalueCells.Count, description)
            For c As Integer = 0 To a.PvalueCells.Count - 1
                Assert.AreEqual(a.PvalueCells(c).BodyRow, b.PvalueCells(c).BodyRow, description)
                Assert.AreEqual(a.PvalueCells(c).BodyColumn, b.PvalueCells(c).BodyColumn, description)
            Next
            For row As Integer = 0 To a.RowCount - 1
                For col As Integer = 0 To a.ColumnCount - 1
                    Dim x As Object = a.Values(row, col)
                    Dim y As Object = b.Values(row, col)
                    Dim tag As String = description & " table=" & t.ToString(CultureInfo.InvariantCulture) &
                        " cell=" & row.ToString(CultureInfo.InvariantCulture) & "," & col.ToString(CultureInfo.InvariantCulture)
                    If TypeOf x Is Double AndAlso TypeOf y Is Double Then
                        Assert.AreEqual(CDbl(x), CDbl(y), 0.000000000001R, tag)
                    Else
                        Assert.AreEqual(Convert.ToString(x, CultureInfo.InvariantCulture), Convert.ToString(y, CultureInfo.InvariantCulture), tag)
                    End If
                Next
            Next
        Next
    End Sub
End Class
