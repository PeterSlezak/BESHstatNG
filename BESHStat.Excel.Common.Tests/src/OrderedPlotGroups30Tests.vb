Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class OrderedPlotGroups30Tests
    <TestMethod>
    Public Sub OrderedLabelsRetainDuplicatesWhitespaceCaseAndScalarType()
        Dim names = {" A ", "A", "a", "1", "1", Nothing, ""}
        Dim groups = names.Select(Function(label, i) New Double() {i}).ToArray()
        Dim raw As Object() = {" A ", "A", "a", 1, "1", Nothing, ""}
        Dim p As New OrderedPlotGroups(New GroupedNumericData(names, groups), raw)
        CollectionAssert.AreEqual(names, p.DisplayLabels)
        Assert.IsInstanceOfType(p.RawLabels(3), GetType(Integer))
        Assert.IsInstanceOfType(p.RawLabels(4), GetType(String))
        Assert.AreEqual(7, p.Data.Groups.Length)
        For i As Integer = 0 To names.Length - 1
            Assert.AreEqual(i, p.PointOrder(i)(0).GroupOrdinal)
            Assert.IsFalse(p.PointOrder(i)(0).PhysicalRowId.HasValue)
        Next
    End Sub

    <TestMethod>
    Public Sub PhysicalRowsAreDistinctFromSourceAndPlottedOrdinals()
        Dim groups = New Double()() {New Double() {90, 10, 40}, New Double() {7}}
        Dim sources = {
            New UnivariateObservationSource(0, 0, 1, 1, 12, 3, "", "", "", ""),
            New UnivariateObservationSource(0, 1, 2, 1, 17, 3, "", "", "", ""),
            New UnivariateObservationSource(0, 2, 3, 1, 25, 3, "", "", "", "")}
        Dim p As New OrderedPlotGroups(New GroupedNumericData({"same", "same"}, groups), observationSources:=sources)
        Dim permutation = {1, 2, 0}, points = p.PointOrder(0, permutation)
        permutation(0) = 0
        CollectionAssert.AreEqual({17, 25, 12}, points.Select(Function(x) x.PhysicalRowId.Value).ToArray())
        CollectionAssert.AreEqual({1, 2, 0}, points.Select(Function(x) x.ObservationIndex).ToArray())
        CollectionAssert.AreEqual({0, 1, 2}, points.Select(Function(x) x.ChartIndex).ToArray())
        Assert.IsFalse(p.PointOrder(1)(0).PhysicalRowId.HasValue)
        Assert.ThrowsException(Of ArgumentException)(Sub() p.PointOrder(0, {0, 0, 1}))
    End Sub

    <TestMethod>
    Public Sub DefensiveCopiesAndRaggedMissingGroups()
        Dim names = {"a", "b", "c"}, raw As Object() = {1, "1", " "}
        Dim groups As Double()() = {New Double() {3, Double.NaN}, Nothing, New Double() {}}
        Dim p As New OrderedPlotGroups(New GroupedNumericData(names, groups), raw)
        names(0) = "changed" : raw(0) = 99 : groups(0)(0) = 99
        Assert.AreEqual("a", p.DisplayLabels(0)) : Assert.AreEqual(1, p.RawLabels(0))
        Assert.AreEqual(3.0, p.Data.Groups(0)(0))
        p.Data.Groups(0)(0) = 123
        p.DisplayLabels(0) = "other" : p.RawLabels(0) = "other"
        Assert.AreEqual("a", p.DisplayLabels(0)) : Assert.AreEqual(1, p.RawLabels(0))
        Assert.AreEqual(3.0, p.Data.Groups(0)(0))
        Assert.AreEqual(0, p.PointOrder(1).Count) : Assert.AreEqual(0, p.PointOrder(2).Count)
        Assert.IsTrue(Double.IsNaN(p.Data.Groups(0)(1)))
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub NoImplicitFormattingOrCultureGrouping(cultureName As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            Dim p As New OrderedPlotGroups(New GroupedNumericData({"1.5", "1,5"}, New Double()() {New Double() {1}, New Double() {2}}), {1.5, "1.5"})
            CollectionAssert.AreEqual({"1.5", "1,5"}, p.DisplayLabels)
            Assert.IsInstanceOfType(p.RawLabels(0), GetType(Double))
            Assert.AreEqual(1.5, CDbl(p.RawLabels(0)))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub ExplicitContractErrorsDoNotMutateInputs()
        Dim groups = New Double()() {New Double() {3, 1}}
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New OrderedPlotGroups(New GroupedNumericData({}, groups))))
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New OrderedPlotGroups(New GroupedNumericData({"a"}, groups), {New Object()})))
        Dim source As New UnivariateObservationSource(0, 2, 0, 0, 0, 0, "", "", "", "")
        Assert.ThrowsException(Of ArgumentException)(Sub() GC.KeepAlive(New OrderedPlotGroups(New GroupedNumericData({"a"}, groups), observationSources:={source})))
        CollectionAssert.AreEqual({3.0, 1.0}, groups(0))
        'CLR non-vector rank-one arrays cannot be silently coerced to typed vectors.
        Dim nonzero = Array.CreateInstance(GetType(Double), {2}, {1})
        nonzero.SetValue(9.0, 1) : nonzero.SetValue(3.0, 2)
        Assert.ThrowsException(Of InvalidCastException)(Sub() AcceptVector(DirectCast(nonzero, Double())))
        Assert.AreEqual(9.0, CDbl(nonzero.GetValue(1)))
    End Sub
    Private Shared Sub AcceptVector(values As Double())
        Assert.IsNotNull(values)
    End Sub

    <TestMethod>
    Public Sub ReusesAcceptedParserSourcesAfterBlankAndFilteredRows()
        Dim input As New SpreadsheetRangeData With {.StartRow = 10, .StartColumn = 4, .RowCount = 4, .ColumnCount = 1}
        input.Rows.Add(New Collections.Generic.List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell(" A ")})
        input.Rows.Add(New Collections.Generic.List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(9)})
        input.Rows.Add(New Collections.Generic.List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.BlankCell()})
        input.Rows.Add(New Collections.Generic.List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(2)})
        Dim options As New SpreadsheetGroupedParseOptions With {.MinimumGroups = 1, .MinimumObservationsPerGroup = 1}
        Dim parsed = UnivariateSelectionPolicy.Parse(input, options, physicalRowIds:={10, 12, 17, 25})
        Assert.IsTrue(parsed.Success)
        Dim plot As New OrderedPlotGroups(parsed.Data, rawLabels:={" A "}, observationSources:=parsed.Sources)
        Assert.AreEqual("A", plot.DisplayLabels(0)) 'Accepted parser header policy, not a new normalization.
        Assert.AreEqual(" A ", plot.RawLabels(0))
        CollectionAssert.AreEqual({25, 12}, plot.PointOrder(0, {1, 0}).Select(Function(p) p.PhysicalRowId.Value).ToArray())
    End Sub
End Class
