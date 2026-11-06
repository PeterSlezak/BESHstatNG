Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class Graphics31SourceIdentityTests
    <TestMethod>
    Public Sub ShuffledPhysicalRowsFollowStableSortedPermutation()
        Dim values = {4.0, 1, 4, 2, 1}
        Dim rows = {47, 12, 99, 31, 18}
        Dim sources = Enumerable.Range(0, values.Length).Select(Function(i) New UnivariateObservationSource(1, i, i + 1, 1, rows(i), 4, "", "", "", "")).ToArray()
        Dim groups As New OrderedPlotGroups(New GroupedNumericData({"duplicate", "duplicate"}, New Double()() {New Double() {7}, values}), observationSources:=sources)
        Dim projection = NormalPlotSeriesData.Compute(values, "Blom", "OLS")
        Dim permutation = projection.SortedToSource, points = groups.PointOrder(1, permutation)
        permutation(0) = 999 : rows(0) = 999 : values(0) = 999 : sources(0) = Nothing
        CollectionAssert.AreEqual({12, 18, 31, 47, 99}, points.Select(Function(p) p.PhysicalRowId.Value).ToArray())
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, points.Select(Function(p) p.ObservationIndex).ToArray())
        CollectionAssert.AreEqual({0, 1, 2, 3, 4}, points.Select(Function(p) p.ChartIndex).ToArray())
        Assert.IsTrue(points.All(Function(p) p.GroupOrdinal = 1))
        Assert.IsTrue(points.All(Function(p) p.ChartIndex <> p.PhysicalRowId.Value))
        For Each point In points
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(groups.Data.Groups(1)(point.ObservationIndex)), BitConverter.DoubleToInt64Bits(projection.X(point.ChartIndex)))
        Next
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(points, IList(Of OrderedPlotPoint)).Clear())
        Assert.IsFalse(groups.PointOrder(0)(0).PhysicalRowId.HasValue)
    End Sub

    <TestMethod>
    Public Sub ExistingParserFilteringKeepsActualRowsAndAcceptedObservationOrdinals()
        Dim input As New SpreadsheetRangeData With {.StartRow = 10, .StartColumn = 4, .RowCount = 5, .ColumnCount = 1}
        input.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell(" A ")})
        input.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(4)})
        input.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.BlankCell()})
        input.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(1)})
        input.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(4)})
        Dim options As New SpreadsheetGroupedParseOptions With {.MinimumGroups = 1, .MinimumObservationsPerGroup = 1}
        Dim parsed = UnivariateSelectionPolicy.Parse(input, options, physicalRowIds:={10, 12, 17, 25, 99})
        Assert.IsTrue(parsed.Success)
        Dim groups As New OrderedPlotGroups(parsed.Data, observationSources:=parsed.Sources)
        Dim projection = NormalPlotSeriesData.Compute(parsed.Data.Groups(0), "Rankit", "R")
        Dim points = groups.PointOrder(0, projection.SortedToSource)
        CollectionAssert.AreEqual({25, 12, 99}, points.Select(Function(p) p.PhysicalRowId.Value).ToArray())
        CollectionAssert.AreEqual({1, 0, 2}, points.Select(Function(p) p.ObservationIndex).ToArray())
        CollectionAssert.AreEqual({0, 1, 2}, points.Select(Function(p) p.ChartIndex).ToArray())
        Assert.AreEqual("A", groups.DisplayLabels(0))
    End Sub

    <TestMethod>
    Public Sub MissingPhysicalRowsAndConstantEmptyGroupsAreNeverSynthesized()
        For Each values In New Double()() {New Double() {3, 3, 3}, New Double() {3}, New Double() {}}
            Dim groups As New OrderedPlotGroups(New GroupedNumericData({"a"}, {values}))
            Dim order As New NormalPlotObservationOrder(values), points = groups.PointOrder(0, order.SortedToSource)
            Assert.AreEqual(values.Length, points.Count)
            CollectionAssert.AreEqual(Enumerable.Range(0, values.Length).ToArray(), points.Select(Function(p) p.ObservationIndex).ToArray())
            Assert.IsTrue(points.All(Function(p) Not p.PhysicalRowId.HasValue AndAlso p.Source Is Nothing))
        Next
        Dim optionalSources = {New UnivariateObservationSource(0, 0, 1, 1, 0, 0, "", "", "", ""), New UnivariateObservationSource(0, 2, 3, 1, 90, 4, "", "", "", "")}
        Dim mixed As New OrderedPlotGroups(New GroupedNumericData({"a"}, New Double()() {New Double() {4, 1, 4}}), observationSources:=optionalSources)
        Dim pointsMixed = mixed.PointOrder(0, New NormalPlotObservationOrder({4, 1, 4}).SortedToSource)
        Assert.IsFalse(pointsMixed(0).PhysicalRowId.HasValue) : Assert.IsFalse(pointsMixed(1).PhysicalRowId.HasValue)
        Assert.AreEqual(90, pointsMixed(2).PhysicalRowId.Value)
    End Sub
End Class
