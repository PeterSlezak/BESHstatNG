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
