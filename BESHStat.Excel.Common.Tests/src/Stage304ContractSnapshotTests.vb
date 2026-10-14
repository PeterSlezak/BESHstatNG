Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>
''' Batch 00.01: frozen, host-neutral Stage 3.04 results.
''' Expected values and semantic table metadata live in TestData/Stage304/*.json.
''' No Excel host or runtime reflection is required to run these tests.
''' </summary>
<TestClass>
Public Class Stage304ContractSnapshotTests

    Private Shared ReadOnly FixtureNames As String() = {
        "anova-base", "anova-welch-tukey", "anova-all-posthoc", "kw-base", "kw-dunn"
    }

    <TestMethod>
    Public Sub LegacyAnalysisKindValuesAndDefaults_RemainBackwardCompatible()
        Assert.AreEqual(1, CInt(SpreadsheetAnalysisKind.OneWayAnova))
        Assert.AreEqual(2, CInt(SpreadsheetAnalysisKind.KruskalWallis))

        Dim anova As New OneWayAnovaAnalysisOptions()
        Assert.AreEqual(0.05, anova.Alpha, 0.0)
        Assert.IsFalse(anova.IncludeWelch)
        Assert.IsFalse(anova.IncludeFisherLsd)
        Assert.IsFalse(anova.IncludeBonferroni)
        Assert.IsFalse(anova.IncludeTukeyKramer)
        Assert.IsFalse(anova.IncludeGamesHowell)

        Dim kw As New KruskalWallisAnalysisOptions()
        Assert.AreEqual(0.05, kw.Alpha, 0.0)
        Assert.IsTrue(kw.IncludeDunnComparisons)

        Dim output As New SpreadsheetOutputTarget()
        Assert.AreEqual(SpreadsheetOutputDestination.NewWorksheet, output.Destination)
        Assert.AreEqual("BESHStat_Result", output.WorksheetName)
        Assert.AreEqual(1, output.StartRow)
        Assert.AreEqual(1, output.StartColumn)
    End Sub

    <TestMethod>
    Public Sub Stage304GoldenCases_MatchMetricsTableOrderAndPvalueMetadata()
        For Each fileName As String In FixtureNames
            Using doc As JsonDocument = LoadFixture(fileName)
                VerifyScenario(doc.RootElement)
            End Using
        Next
    End Sub

    <TestMethod>
    Public Sub Stage304GoldenCases_WriteRequestsPreserveFormattingAndOrder()
        Dim settings As New PValuePresentationSettings With {
            .DecimalPlaces = 5,
            .SmallValueDisplay = PValueSmallValueDisplay.LessThanThreshold,
            .UseUpperBound = True
        }
        Dim target As New SpreadsheetOutputTarget With {
            .Destination = SpreadsheetOutputDestination.NewWorksheet,
            .WorksheetName = "Parity_Result",
            .StartRow = 6,
            .StartColumn = 3
        }

        For Each fileName As String In FixtureNames
            Using doc As JsonDocument = LoadFixture(fileName)
                Dim result As SpreadsheetAnalysisResult = ExecuteFixture(doc.RootElement)
                Dim requests As List(Of ResultTableWriteRequest) = ResultTableWriteRequestFactory.CreateMany(
                    result.Tables, settings, 0.01, target)

                Assert.AreEqual(result.Tables.Count, requests.Count, fileName)
                For tableIndex As Integer = 0 To requests.Count - 1
                    Dim model As Global.BESHStatNG.ResultTableOutputModel = result.Tables(tableIndex)
                    Dim request As ResultTableWriteRequest = requests(tableIndex)
                    Assert.AreEqual(model.RowCount, request.Values.Count, fileName)
                    Assert.AreEqual(model.ColumnCount, request.Values(0).Count, fileName)
                    Assert.AreEqual(model.HeaderTopRows, request.HeaderTopRows, fileName)
                    Assert.AreEqual(model.FooterRows, request.FooterRows, fileName)
                    Assert.AreEqual(0.01, request.PvalueHighlightAlpha, 0.0, fileName)
                    Assert.AreEqual(Global.BESHStatNG.PValuePresentation.BuildExcelNumberFormat(settings),
                                    request.PvalueNumberFormat, fileName)
                    Assert.AreEqual(SpreadsheetOutputDestination.NewWorksheet, request.OutputTarget.Destination, fileName)
                    Assert.AreEqual("Parity_Result", request.OutputTarget.WorksheetName, fileName)
                    Assert.AreEqual(6, request.OutputTarget.StartRow, fileName)
                    Assert.AreEqual(3, request.OutputTarget.StartColumn, fileName)
                    Assert.IsTrue(request.AutoFit, fileName)
                    CollectionAssert.AreEqual(model.PvalueColumns, request.PvalueColumns, fileName)
                    Assert.AreEqual(model.PvalueCells.Count, request.PvalueCells.Count, fileName)
                    For cellIndex As Integer = 0 To model.PvalueCells.Count - 1
                        Assert.AreEqual(model.PvalueCells(cellIndex).BodyRow,
                                        request.PvalueCells(cellIndex).BodyRow, fileName)
                        Assert.AreEqual(model.PvalueCells(cellIndex).BodyColumn,
                                        request.PvalueCells(cellIndex).BodyColumn, fileName)
                    Next
                Next
            End Using
        Next
    End Sub

    <TestMethod>
    Public Sub SpreadsheetCellPayload_SerializesUsingStage304KindsAndPrimitiveValues()
        Dim cells As New List(Of SpreadsheetCellValue) From {
            SpreadsheetCellValue.BlankCell(),
            SpreadsheetCellValue.NumberCell(2.5),
            SpreadsheetCellValue.TextCell("=1+1"),
            SpreadsheetCellValue.BooleanCell(True),
            SpreadsheetCellValue.ErrorCell("#N/A")
        }
        Dim json As String = JsonSerializer.Serialize(cells)
        Using doc As JsonDocument = JsonDocument.Parse(json)
            Dim entries As JsonElement = doc.RootElement
            Assert.AreEqual(5, entries.GetArrayLength())
            For i As Integer = 0 To 4
                Assert.AreEqual(i, entries(i).GetProperty("Kind").GetInt32())
            Next
            Assert.AreEqual(2.5, entries(1).GetProperty("NumberValue").GetDouble(), 0.0)
            Assert.AreEqual("=1+1", entries(2).GetProperty("TextValue").GetString())
            Assert.IsTrue(entries(3).GetProperty("BooleanValue").GetBoolean())
            Assert.AreEqual("#N/A", entries(4).GetProperty("TextValue").GetString())
        End Using
    End Sub

    <TestMethod>
    Public Sub ResultWritePayload_SerializesExistingNonfiniteAndCellTypePolicy()
        ' This captures the CURRENT string placeholder policy, not a future typed error-cell design.
        Dim values As Object(,) = {
            {"literal =SUM(A1:A2)", 2.5R, True, Nothing},
            {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, "#N/A"}
        }
        Dim model As New Global.BESHStatNG.ResultTableOutputModel(values, isResultTable:=False)
        Dim request As ResultTableWriteRequest = ResultTableWriteRequestFactory.Create(
            model, New PValuePresentationSettings(), 0.05, New SpreadsheetOutputTarget())

        Using doc As JsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(request))
            Dim rows As JsonElement = doc.RootElement.GetProperty("Values")
            Assert.AreEqual("literal =SUM(A1:A2)", rows(0)(0).GetString())
            Assert.AreEqual(2.5, rows(0)(1).GetDouble(), 0.0)
            Assert.IsTrue(rows(0)(2).GetBoolean())
            Assert.AreEqual(JsonValueKind.Null, rows(0)(3).ValueKind)
            Assert.AreEqual("#N/A", rows(1)(0).GetString())
            Assert.AreEqual("#Pinf", rows(1)(1).GetString())
            Assert.AreEqual("#Ninf", rows(1)(2).GetString())
            Assert.AreEqual("#N/A", rows(1)(3).GetString())
            Assert.AreEqual(0.05, doc.RootElement.GetProperty("PvalueHighlightAlpha").GetDouble(), 0.0)
        End Using
    End Sub

    <TestMethod>
    Public Sub GroupedParser_LegacyHeaderAndBlankContract_RemainsUnchanged()
        Dim range As New SpreadsheetRangeData With {
            .WorksheetName = "Input",
            .Address = "A1:B4",
            .RowCount = 4,
            .ColumnCount = 2,
            .Rows = New List(Of List(Of SpreadsheetCellValue)) From {
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.BlankCell(), SpreadsheetCellValue.TextCell(" B ")},
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.NumberCell(1.0), SpreadsheetCellValue.NumberCell(2.0)},
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.BlankCell(), SpreadsheetCellValue.NumberCell(3.0)},
                New List(Of SpreadsheetCellValue) From {
                    SpreadsheetCellValue.NumberCell(4.0), SpreadsheetCellValue.NumberCell(4.0)}
            }
        }
        Dim grouped As GroupedNumericData = GroupedNumericRangeParser.Parse(range)
        CollectionAssert.AreEqual(New String() {"Group 1", "B"}, grouped.GroupNames)
        CollectionAssert.AreEqual(New Integer() {2, 3}, grouped.GroupCounts)

        range.Rows(2)(0) = SpreadsheetCellValue.ErrorCell("#DIV/0!")
        Dim ex As ArgumentException = Assert.ThrowsException(Of ArgumentException)(
            Function() GroupedNumericRangeParser.Parse(range))
        StringAssert.Contains(ex.Message, "Non-numeric value")

        range.Rows(2)(0) = SpreadsheetCellValue.NumberCell(Double.PositiveInfinity)
        ex = Assert.ThrowsException(Of ArgumentException)(Function() GroupedNumericRangeParser.Parse(range))
        StringAssert.Contains(ex.Message, "non-finite")

        range.Rows(2)(0) = SpreadsheetCellValue.BlankCell()
        range.RowCount = 5
        ex = Assert.ThrowsException(Of ArgumentException)(Function() GroupedNumericRangeParser.Parse(range))
        StringAssert.Contains(ex.Message, "row count")
    End Sub

    Private Shared Function LoadFixture(name As String) As JsonDocument
        Dim fixturePath As String = System.IO.Path.Combine(AppContext.BaseDirectory, "TestData", "Stage304", name & ".json")
        Assert.IsTrue(File.Exists(fixturePath), "Missing golden fixture: " & fixturePath)
        Return JsonDocument.Parse(File.ReadAllText(fixturePath))
    End Function

    Private Shared Function ExecuteFixture(fixture As JsonElement) As SpreadsheetAnalysisResult
        Assert.AreEqual(1, fixture.GetProperty("fixtureVersion").GetInt32())
        Dim groupPayload As JsonElement = fixture.GetProperty("groups")
        Dim rawNames As JsonElement = groupPayload.GetProperty("names")
        Dim rawValues As JsonElement = groupPayload.GetProperty("values")
        Dim names(rawNames.GetArrayLength() - 1) As String
        Dim groups(rawValues.GetArrayLength() - 1)() As Double

        For i As Integer = 0 To names.Length - 1
            names(i) = rawNames(i).GetString()
            Dim rawGroup As JsonElement = rawValues(i)
            Dim values As New List(Of Double)()
            For Each element As JsonElement In rawGroup.EnumerateArray()
                values.Add(element.GetDouble())
            Next
            groups(i) = values.ToArray()
        Next
        Dim input As New GroupedNumericData(names, groups)
        Dim kind As SpreadsheetAnalysisKind = CType(fixture.GetProperty("legacyEnumValue").GetInt32(), SpreadsheetAnalysisKind)
        Dim options As SpreadsheetAnalysisOptions
        Dim opt As JsonElement = fixture.GetProperty("options")

        Select Case kind
            Case SpreadsheetAnalysisKind.OneWayAnova
                Dim o As New OneWayAnovaAnalysisOptions With {
                    .IncludeWelch = OptionFlag(opt, "welch"),
                    .IncludeFisherLsd = OptionFlag(opt, "fisherLsd"),
                    .IncludeBonferroni = OptionFlag(opt, "bonferroni"),
                    .IncludeTukeyKramer = OptionFlag(opt, "tukeyKramer"),
                    .IncludeGamesHowell = OptionFlag(opt, "gamesHowell")
                }
                options = o
            Case SpreadsheetAnalysisKind.KruskalWallis
                Dim o As New KruskalWallisAnalysisOptions With {
                    .IncludeDunnComparisons = OptionFlag(opt, "dunn")
                }
                options = o
            Case Else
                Assert.Fail("Unsupported Stage 3.04 fixture enum: " & CInt(kind).ToString(CultureInfo.InvariantCulture))
                Throw New InvalidOperationException("Unreachable")
        End Select

        Return SharedAnalysisApplicationService.Run(New SpreadsheetAnalysisRequest(kind, input, options))
    End Function

    Private Shared Function OptionFlag(options As JsonElement, propertyName As String) As Boolean
        Dim flag As JsonElement
        Return options.TryGetProperty(propertyName, flag) AndAlso flag.GetBoolean()
    End Function

    Private Shared Sub VerifyScenario(fixture As JsonElement)
        Dim fixtureId As String = fixture.GetProperty("fixtureId").GetString()
        Dim result As SpreadsheetAnalysisResult = ExecuteFixture(fixture)
        Assert.AreEqual(fixture.GetProperty("legacyEnumValue").GetInt32(), CInt(result.AnalysisKind), fixtureId)
        Assert.IsNotNull(result.PrimaryTable, fixtureId)

        Dim expectedGroups As JsonElement = fixture.GetProperty("groups")
        Dim expectedValues As JsonElement = expectedGroups.GetProperty("values")
        Dim expectedCounts(expectedValues.GetArrayLength() - 1) As Integer
        For i As Integer = 0 To expectedCounts.Length - 1
            expectedCounts(i) = expectedValues(i).GetArrayLength()
        Next
        If result.AnalysisKind = SpreadsheetAnalysisKind.OneWayAnova Then
            Assert.IsInstanceOfType(result.Detail, GetType(OneWayAnovaApplicationResult), fixtureId)
            CollectionAssert.AreEqual(expectedCounts,
                                      DirectCast(result.Detail, OneWayAnovaApplicationResult).GroupCounts, fixtureId)
        Else
            Assert.IsInstanceOfType(result.Detail, GetType(KruskalWallisApplicationResult), fixtureId)
            CollectionAssert.AreEqual(expectedCounts,
                                      DirectCast(result.Detail, KruskalWallisApplicationResult).GroupCounts, fixtureId)
        End If

        Dim expectedMetrics As JsonElement = fixture.GetProperty("metrics")
        Dim metricCount As Integer = 0
        For Each propertyItem As JsonProperty In expectedMetrics.EnumerateObject()
            metricCount += 1
            Dim actual As Double
            Assert.IsTrue(result.TryGetMetric(propertyItem.Name, actual), fixtureId & ": " & propertyItem.Name)
            Assert.AreEqual(propertyItem.Value.GetDouble(), actual, 0.00000005R, fixtureId & ": " & propertyItem.Name)
        Next
        Assert.AreEqual(metricCount, result.Metrics.Count, fixtureId & ": unexpected metric key")

        Dim expectedTables As JsonElement = fixture.GetProperty("tables")
        Assert.AreEqual(expectedTables.GetArrayLength(), result.Tables.Count, fixtureId & ": table count/order")
        For tableIndex As Integer = 0 To result.Tables.Count - 1
            Dim expected As JsonElement = expectedTables(tableIndex)
            Dim model As Global.BESHStatNG.ResultTableOutputModel = result.Tables(tableIndex)
            Assert.AreEqual(expected.GetProperty("topLeft").GetString(),
                            Convert.ToString(model.Values(0, 0), CultureInfo.InvariantCulture),
                            fixtureId & ": table " & tableIndex.ToString(CultureInfo.InvariantCulture))
            Assert.AreEqual(expected.GetProperty("rows").GetInt32(), model.RowCount, fixtureId)
            Assert.AreEqual(expected.GetProperty("columns").GetInt32(), model.ColumnCount, fixtureId)
            Assert.AreEqual(expected.GetProperty("headerTopRows").GetInt32(), model.HeaderTopRows, fixtureId)
            Assert.AreEqual(expected.GetProperty("footerRows").GetInt32(), model.FooterRows, fixtureId)

            Dim columns As JsonElement = expected.GetProperty("pvalueColumns")
            Assert.AreEqual(columns.GetArrayLength(), model.PvalueColumns.Count, fixtureId)
            For i As Integer = 0 To model.PvalueColumns.Count - 1
                Assert.AreEqual(columns(i).GetInt32(), model.PvalueColumns(i), fixtureId)
            Next

            Dim cells As JsonElement = expected.GetProperty("pvalueCells")
            Assert.AreEqual(cells.GetArrayLength(), model.PvalueCells.Count, fixtureId)
            For i As Integer = 0 To model.PvalueCells.Count - 1
                Assert.AreEqual(cells(i)(0).GetInt32(), model.PvalueCells(i).BodyRow, fixtureId)
                Assert.AreEqual(cells(i)(1).GetInt32(), model.PvalueCells(i).BodyColumn, fixtureId)
            Next
        Next

        For Each cell As JsonElement In fixture.GetProperty("cells").EnumerateArray()
            Dim model As Global.BESHStatNG.ResultTableOutputModel = result.Tables(cell.GetProperty("table").GetInt32())
            Dim actual As Double = Convert.ToDouble(model.Values(cell.GetProperty("row").GetInt32(),
                                                                cell.GetProperty("column").GetInt32()),
                                                       CultureInfo.InvariantCulture)
            Assert.AreEqual(cell.GetProperty("expectedNumber").GetDouble(), actual,
                            cell.GetProperty("tolerance").GetDouble(), fixtureId & ": selected numeric cell")
        Next
    End Sub

End Class
