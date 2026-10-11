Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class SharedExcelLayerTests

    <TestMethod>
    Public Sub GroupedNumericRangeParser_ParsesHeadersBlanksAndUnequalGroups()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"A", "B", "C"},
                New Object() {1.0, 10.0, 100.0},
                New Object() {2.0, 11.0, 101.0},
                New Object() {3.0, Nothing, 102.0},
                New Object() {Nothing, Nothing, 103.0}
            })

        Dim parsed As GroupedNumericData = GroupedNumericRangeParser.Parse(input)

        CollectionAssert.AreEqual(New String() {"A", "B", "C"}, parsed.GroupNames)
        CollectionAssert.AreEqual(New Integer() {3, 2, 4}, parsed.GroupCounts)
        Assert.AreEqual(11.0, parsed.Groups(1)(1), 0.0)
        Assert.AreEqual(103.0, parsed.Groups(2)(3), 0.0)
    End Sub

    <TestMethod>
    Public Sub GroupedNumericRangeParser_RejectsTextInDataBody()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"A", "B"},
                New Object() {1.0, 10.0},
                New Object() {"bad", 11.0}
            })

        Dim ex As ArgumentException = Assert.ThrowsException(Of ArgumentException)(
            Function() GroupedNumericRangeParser.Parse(input))

        StringAssert.Contains(ex.Message, "Non-numeric value")
        StringAssert.Contains(ex.Message, "A")
    End Sub

    <TestMethod>
    Public Sub OneWayAnovaApplicationService_MatchesReferenceAndReturnsPvalueMetadata()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"Group A", "Group B", "Group C"},
                New Object() {1.0, 2.0, 5.0},
                New Object() {2.0, 3.0, 6.0},
                New Object() {3.0, 4.0, 7.0}
            })

        Dim result As OneWayAnovaApplicationResult = OneWayAnovaApplicationService.Run(input)

        Assert.AreEqual(13.0, result.FStatistic, 0.000000000001)
        Assert.AreEqual(0.006591796875, result.PValue, 0.000000000001)
        CollectionAssert.AreEqual(New Integer() {3, 3, 3}, result.GroupCounts)
        Assert.AreEqual(4, result.Table.RowCount)
        Assert.AreEqual(6, result.Table.ColumnCount)
        Assert.AreEqual(1, result.Table.PvalueColumns.Count)
        Assert.AreEqual(5, result.Table.PvalueColumns(0))
    End Sub

    <TestMethod>
    Public Sub OneWayAnovaApplicationService_AcceptsPreparsedGroupedData()
        Dim grouped As New GroupedNumericData(
            New String() {"Group A", "Group B", "Group C"},
            New Double()() {
                New Double() {1.0, 2.0, 3.0},
                New Double() {2.0, 3.0, 4.0},
                New Double() {5.0, 6.0, 7.0}
            })

        Dim result As OneWayAnovaApplicationResult = OneWayAnovaApplicationService.Run(grouped)

        Assert.AreEqual(13.0, result.FStatistic, 0.000000000001)
        Assert.AreEqual(0.006591796875, result.PValue, 0.000000000001)
        CollectionAssert.AreEqual(New Integer() {3, 3, 3}, result.GroupCounts)
        CollectionAssert.AreEqual(New String() {"Group A", "Group B", "Group C"}, result.GroupNames)
    End Sub

    <TestMethod>
    Public Sub OneWayAnovaApplicationService_WelchAndPostHoc_AreProducedInOneSharedRun()
        Dim grouped As New GroupedNumericData(
            New String() {"G1", "G2", "G3"},
            New Double()() {
                New Double() {1.0, 2.0, 3.0},
                New Double() {2.0, 3.0, 4.0},
                New Double() {5.0, 6.0, 7.0}
            })
        Dim options As New OneWayAnovaAnalysisOptions With {
            .IncludeWelch = True,
            .IncludeTukeyKramer = True,
            .Alpha = 0.05
        }

        Dim result As OneWayAnovaApplicationResult = OneWayAnovaApplicationService.Run(grouped, options)

        Assert.IsTrue(result.HasWelch)
        Assert.AreEqual(11.142857142857142, result.WelchFStatistic, 0.000000001)
        Assert.AreEqual(0.023156899810964086, result.WelchPValue, 0.00000005)
        Assert.AreEqual(4.0, result.WelchDenominatorDf, 0.000000000001)
        Assert.AreEqual(9, result.Table.ColumnCount)
        Assert.AreEqual(2, result.Table.PvalueColumns.Count)
        Assert.AreEqual(5, result.Table.PvalueColumns(0))
        Assert.AreEqual(8, result.Table.PvalueColumns(1))

        Assert.AreEqual(2, result.Tables.Count, "Primary ANOVA plus Tukey-Kramer table expected.")
        Assert.IsNotNull(result.TukeyKramerPlotData)
        Assert.IsTrue(result.TukeyKramerPlotData.Labels.Length > 0)
    End Sub

    <TestMethod>
    Public Sub SharedAnalysisApplicationService_DispatchesRangeInputAndReturnsGenericMetrics()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"Group A", "Group B", "Group C"},
                New Object() {1.0, 2.0, 5.0},
                New Object() {2.0, 3.0, 6.0},
                New Object() {3.0, 4.0, 7.0}
            })
        Dim request As New SpreadsheetAnalysisRequest(
            SpreadsheetAnalysisKind.OneWayAnova,
            input,
            New OneWayAnovaAnalysisOptions With {.IncludeWelch = True})

        Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(request)

        Assert.AreEqual(SpreadsheetAnalysisKind.OneWayAnova, result.AnalysisKind)
        Assert.IsNotNull(result.PrimaryTable)
        Assert.AreEqual(9, result.PrimaryTable.ColumnCount)
        Assert.AreEqual(13.0, result.Metrics(AnalysisMetricNames.FStatistic), 0.000000000001)
        Assert.AreEqual(0.006591796875, result.Metrics(AnalysisMetricNames.PValue), 0.000000000001)
        Assert.AreEqual(11.142857142857142, result.Metrics(AnalysisMetricNames.WelchFStatistic), 0.000000001)
        Assert.AreEqual(0.023156899810964086, result.Metrics(AnalysisMetricNames.WelchPValue), 0.00000005)
        Assert.IsInstanceOfType(result.Detail, GetType(OneWayAnovaApplicationResult))
    End Sub

    <TestMethod>
    Public Sub SharedAnalysisApplicationService_DispatchesPreparsedGroupedInput()
        Dim grouped As New GroupedNumericData(
            New String() {"A", "B", "C"},
            New Double()() {
                New Double() {1.0, 2.0, 3.0},
                New Double() {2.0, 3.0, 4.0},
                New Double() {5.0, 6.0, 7.0}
            })
        Dim request As New SpreadsheetAnalysisRequest(
            SpreadsheetAnalysisKind.OneWayAnova,
            grouped,
            New OneWayAnovaAnalysisOptions())

        Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(request)

        Assert.AreEqual(13.0, result.Metrics(AnalysisMetricNames.FStatistic), 0.000000000001)
        Assert.AreEqual(1, result.Tables.Count)
    End Sub

    <TestMethod>
    Public Sub SharedAnalysisApplicationService_RejectsUnsupportedAnalysisKind()
        Dim grouped As New GroupedNumericData(
            New String() {"A", "B"},
            New Double()() {
                New Double() {1.0, 2.0},
                New Double() {3.0, 4.0}
            })
        Dim request As New SpreadsheetAnalysisRequest With {
            .AnalysisKind = CType(999, SpreadsheetAnalysisKind),
            .Input = grouped
        }

        Assert.ThrowsException(Of NotSupportedException)(
            Function() SharedAnalysisApplicationService.Run(request))
    End Sub

    <TestMethod>
    Public Sub KruskalWallisApplicationService_WithTies_MatchesReferenceAndReturnsDunnTable()
        Dim grouped As New GroupedNumericData(
            New String() {"A", "B", "C"},
            New Double()() {
                New Double() {1.0, 2.0, 2.0, 3.0},
                New Double() {2.0, 3.0, 4.0, 4.0},
                New Double() {4.0, 5.0, 5.0, 6.0}
            })
        Dim options As New KruskalWallisAnalysisOptions With {
            .IncludeDunnComparisons = True,
            .Alpha = 0.05
        }

        Dim result As KruskalWallisApplicationResult = KruskalWallisApplicationService.Run(grouped, options)

        Assert.AreEqual(7.875, result.HStatistic, 0.000000000001)
        Assert.AreEqual(8.1603260869565215, result.TieCorrectedHStatistic, 0.000000000001)
        Assert.AreEqual(0.019496896108597995, result.PValue, 0.000000000001)
        Assert.AreEqual(0.016904709225411534, result.TieCorrectedPValue, 0.000000000001)
        CollectionAssert.AreEqual(New Integer() {4, 4, 4}, result.GroupCounts)

        Assert.AreEqual(2, result.Tables.Count, "Kruskal-Wallis plus Dunn comparison table expected.")
        Assert.AreEqual(2, result.Table.PvalueCells.Count)
        Assert.AreEqual(1, result.Tables(1).PvalueColumns.Count)
        Assert.AreEqual(4, result.Tables(1).PvalueColumns(0))
    End Sub

    <TestMethod>
    Public Sub KruskalWallisApplicationService_CanOmitDunnComparisons()
        Dim grouped As New GroupedNumericData(
            New String() {"A", "B", "C"},
            New Double()() {
                New Double() {1.0, 2.0, 2.0, 3.0},
                New Double() {2.0, 3.0, 4.0, 4.0},
                New Double() {4.0, 5.0, 5.0, 6.0}
            })

        Dim result As KruskalWallisApplicationResult = KruskalWallisApplicationService.Run(
            grouped,
            New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = False})

        Assert.AreEqual(1, result.Tables.Count)
    End Sub

    <TestMethod>
    Public Sub SharedAnalysisApplicationService_DispatchesKruskalWallisAndReturnsGenericMetrics()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"A", "B", "C"},
                New Object() {1.0, 2.0, 4.0},
                New Object() {2.0, 3.0, 5.0},
                New Object() {2.0, 4.0, 5.0},
                New Object() {3.0, 4.0, 6.0}
            })
        Dim request As New SpreadsheetAnalysisRequest(
            SpreadsheetAnalysisKind.KruskalWallis,
            input,
            New KruskalWallisAnalysisOptions With {.IncludeDunnComparisons = True})

        Dim result As SpreadsheetAnalysisResult = SharedAnalysisApplicationService.Run(request)

        Assert.AreEqual(SpreadsheetAnalysisKind.KruskalWallis, result.AnalysisKind)
        Assert.AreEqual(2, result.Tables.Count)
        Assert.AreEqual(7.875, result.Metrics(AnalysisMetricNames.HStatistic), 0.000000000001)
        Assert.AreEqual(8.1603260869565215, result.Metrics(AnalysisMetricNames.TieCorrectedHStatistic), 0.000000000001)
        Assert.AreEqual(0.019496896108597995, result.Metrics(AnalysisMetricNames.PValue), 0.000000000001)
        Assert.AreEqual(0.016904709225411534, result.Metrics(AnalysisMetricNames.TieCorrectedPValue), 0.000000000001)
        Assert.IsInstanceOfType(result.Detail, GetType(KruskalWallisApplicationResult))
    End Sub

    <TestMethod>
    Public Sub ResultTableWriteRequestFactory_CarriesFormattingAndOutputTarget()
        Dim input As SpreadsheetRangeData = BuildRange(
            New Object()() {
                New Object() {"Group A", "Group B", "Group C"},
                New Object() {1.0, 2.0, 5.0},
                New Object() {2.0, 3.0, 6.0},
                New Object() {3.0, 4.0, 7.0}
            })
        Dim result As OneWayAnovaApplicationResult = OneWayAnovaApplicationService.Run(input)

        Dim settings As New PValuePresentationSettings With {
            .DecimalPlaces = 5,
            .SmallValueDisplay = PValueSmallValueDisplay.LessThanThreshold,
            .UseUpperBound = True
        }
        Dim target As New SpreadsheetOutputTarget With {
            .Destination = SpreadsheetOutputDestination.CurrentWorksheet,
            .WorksheetName = "Ignored for current sheet",
            .StartRow = 4,
            .StartColumn = 3
        }

        Dim request As ResultTableWriteRequest = ResultTableWriteRequestFactory.Create(
            result.Table,
            settings,
            0.01,
            target)

        Assert.AreEqual(4, request.Values.Count)
        Assert.AreEqual(6, request.Values(0).Count)
        Assert.AreEqual(1, request.PvalueColumns.Count)
        Assert.AreEqual(5, request.PvalueColumns(0))
        Assert.AreEqual(0.01, request.PvalueHighlightAlpha, 0.0)
        StringAssert.Contains(request.PvalueNumberFormat, "<0.00001")
        StringAssert.Contains(request.PvalueNumberFormat, ">0.99999")
        Assert.AreEqual(SpreadsheetOutputDestination.CurrentWorksheet, request.OutputTarget.Destination)
        Assert.AreEqual(4, request.OutputTarget.StartRow)
        Assert.AreEqual(3, request.OutputTarget.StartColumn)

        Dim requests As List(Of ResultTableWriteRequest) = ResultTableWriteRequestFactory.CreateMany(
            New List(Of BESHStatNG.ResultTableOutputModel) From {result.Table, result.Table},
            settings,
            0.01,
            target)
        Assert.AreEqual(2, requests.Count)
        Assert.AreEqual(request.PvalueNumberFormat, requests(1).PvalueNumberFormat)
    End Sub

    Private Shared Function BuildRange(rows As Object()()) As SpreadsheetRangeData
        Dim output As New SpreadsheetRangeData With {
            .WorksheetName = "Input",
            .Address = "Input!A1:C5",
            .RowCount = rows.Length,
            .ColumnCount = rows(0).Length
        }

        For Each sourceRow As Object() In rows
            Dim targetRow As New List(Of SpreadsheetCellValue)()
            For Each value As Object In sourceRow
                targetRow.Add(ToCell(value))
            Next
            output.Rows.Add(targetRow)
        Next

        Return output
    End Function

    Private Shared Function ToCell(value As Object) As SpreadsheetCellValue
        If value Is Nothing Then Return SpreadsheetCellValue.BlankCell()
        If TypeOf value Is String Then Return SpreadsheetCellValue.TextCell(DirectCast(value, String))
        If TypeOf value Is Boolean Then Return SpreadsheetCellValue.BooleanCell(DirectCast(value, Boolean))
        Return SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, Globalization.CultureInfo.InvariantCulture))
    End Function

End Class
