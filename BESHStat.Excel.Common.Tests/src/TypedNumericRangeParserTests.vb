Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class TypedNumericRangeParserTests

    Private Shared Function Source(data As Object()(), Optional startRow As Integer = 0,
                                   Optional startColumn As Integer = 0) As SpreadsheetRangeData
        Dim rows As New List(Of List(Of SpreadsheetCellValue))()
        For Each values As Object() In data
            Dim cells As New List(Of SpreadsheetCellValue)()
            For Each value As Object In values
                If value Is Nothing Then
                    cells.Add(SpreadsheetCellValue.BlankCell())
                ElseIf TypeOf value Is SpreadsheetCellValue Then
                    cells.Add(DirectCast(value, SpreadsheetCellValue))
                ElseIf TypeOf value Is Boolean Then
                    cells.Add(SpreadsheetCellValue.BooleanCell(CBool(value)))
                ElseIf TypeOf value Is Double OrElse TypeOf value Is Integer Then
                    cells.Add(SpreadsheetCellValue.NumberCell(Convert.ToDouble(value, CultureInfo.InvariantCulture)))
                Else
                    cells.Add(SpreadsheetCellValue.TextCell(Convert.ToString(value, CultureInfo.InvariantCulture)))
                End If
            Next
            rows.Add(cells)
        Next
        Return New SpreadsheetRangeData With {
            .WorksheetName = "Study", .Rows = rows, .RowCount = rows.Count,
            .ColumnCount = If(rows.Count = 0, 0, rows(0).Count),
            .StartRow = startRow, .StartColumn = startColumn}
    End Function

    Private Shared Function Issue(Of T As SpreadsheetAnalysisInput)(result As SpreadsheetTypedParseResult(Of T),
                                                                  code As String) As SpreadsheetValidationIssue
        Return result.Validation.Issues.FirstOrDefault(Function(item) item.Code = code)
    End Function

    <TestMethod>
    Public Sub Scalar_UsesRawNumericAndAbsoluteSourceCoordinates()
        Dim input As SpreadsheetRangeData = Source(New Object()() {New Object() {12.5R}}, 5, 7)
        Dim result = TypedNumericRangeParsers.ParseScalar(input)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(12.5R, result.Data.Value)
        Assert.AreEqual(1, result.Data.Observation.SelectedRow)
        Assert.AreEqual(5, result.Data.Observation.WorksheetRow)
        Assert.AreEqual(7, result.Data.Observation.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Scalar_RejectsMulticellAndBlankWithStableCodes()
        Assert.IsNotNull(Issue(TypedNumericRangeParsers.ParseScalar(Source(New Object()() {New Object() {1, 2}})), "scalar.shape"))
        Dim missing = TypedNumericRangeParsers.ParseScalar(Source(New Object()() {New Object() {Nothing}}))
        Assert.IsFalse(missing.Success)
        Assert.IsNull(missing.Data)
        Assert.IsNotNull(Issue(missing, "scalar.missing"))
    End Sub

    <TestMethod>
    Public Sub Scalar_DistinguishesExcelErrorFromLiteralText()
        Dim errorCell = TypedNumericRangeParsers.ParseScalar(Source(New Object()() {New Object() {SpreadsheetCellValue.ErrorCell("#N/A")}}))
        Dim textCell = TypedNumericRangeParsers.ParseScalar(Source(New Object()() {New Object() {"#N/A"}}))
        Assert.IsNotNull(Issue(errorCell, "cell.excel-error"))
        Assert.IsNotNull(Issue(textCell, "cell.not-numeric"))
    End Sub

    <TestMethod>
    Public Sub Scalar_OptInInvariantNumericTextAndBoolean()
        Dim opts As New SpreadsheetCellPolicyOptions With {
            .NumericText = SpreadsheetNumericTextPolicy.Invariant,
            .AllowBooleanNumbers = True}
        Assert.AreEqual(1.25R, TypedNumericRangeParsers.ParseScalar(
            Source(New Object()() {New Object() {"1.25"}}), opts).Data.Value)
        Assert.AreEqual(1.0R, TypedNumericRangeParsers.ParseScalar(
            Source(New Object()() {New Object() {True}}), opts).Data.Value)
        Assert.IsFalse(TypedNumericRangeParsers.ParseScalar(
            Source(New Object()() {New Object() {"1.25"}})).Success)
    End Sub

    <TestMethod>
    Public Sub Vector_AutoColumnPreservesOrderAndSourceRows()
        Dim result = TypedNumericRangeParsers.ParseVector(Source(New Object()() {
            New Object() {1}, New Object() {Nothing}, New Object() {3}}, 20, 2))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {1, 3}, result.Data.Values)
        Assert.AreEqual(22, result.Data.Observations(1).WorksheetRow)
        Assert.AreEqual(2, result.Data.Observations(1).WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Vector_HeaderRequiredAndAutoCorrectlySkipOnlyHeader()
        Dim input = Source(New Object()() {New Object() {"Score"}, New Object() {2}, New Object() {3}})
        Dim opts As New SpreadsheetVectorParseOptions With {.Header = SpreadsheetHeaderMode.Required}
        Dim required = TypedNumericRangeParsers.ParseVector(input, opts)
        Assert.IsTrue(required.Success)
        Assert.AreEqual("Score", required.Data.Name)
        CollectionAssert.AreEqual(New Double() {2, 3}, required.Data.Values)
        opts.Header = SpreadsheetHeaderMode.Auto
        Dim automatic = TypedNumericRangeParsers.ParseVector(input, opts)
        Assert.IsTrue(automatic.Success)
        Assert.IsNotNull(Issue(automatic, "header.inferred"))
    End Sub

    <TestMethod>
    Public Sub Vector_NoHeaderRowSelectionAndDefensiveValuesCopy()
        Dim result = TypedNumericRangeParsers.ParseVector(Source(New Object()() {New Object() {2, 3, 4}}))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {2, 3, 4}, result.Data.Values)
        Dim numbers As Double() = result.Data.Values
        numbers(0) = 12345
        Assert.AreEqual(2.0R, result.Data.Values(0))
        Assert.AreEqual(3, result.Data.Observations(2).SelectedColumn)
    End Sub

    <TestMethod>
    Public Sub Vector_ExplicitColumnFromRectangleAndMissingReject()
        Dim input = Source(New Object()() {New Object() {1, 4}, New Object() {2, Nothing}, New Object() {3, 6}})
        Dim opts As New SpreadsheetVectorParseOptions With {
            .Orientation = SpreadsheetVectorOrientation.Column, .ColumnIndex = 1,
            .Missing = SpreadsheetIncompleteObservationPolicy.Reject}
        Dim result = TypedNumericRangeParsers.ParseVector(input, opts)
        Assert.IsFalse(result.Success)
        Assert.IsNotNull(Issue(result, "vector.missing"))
        Assert.AreEqual(2, Issue(result, "vector.missing").SelectedRow)
        Assert.AreEqual(2, Issue(result, "vector.missing").SelectedColumn)
    End Sub

    <TestMethod>
    Public Sub Vector_RejectsAmbiguousTwoDimensionalAutoAndInvalidIndex()
        Dim input = Source(New Object()() {New Object() {1, 2}, New Object() {3, 4}})
        Assert.IsNotNull(Issue(TypedNumericRangeParsers.ParseVector(input), "vector.orientation-ambiguous"))
        Dim opts As New SpreadsheetVectorParseOptions With {
            .Orientation = SpreadsheetVectorOrientation.Column, .ColumnIndex = 9}
        Assert.IsNotNull(Issue(TypedNumericRangeParsers.ParseVector(input, opts), "vector.index"))
    End Sub

    <TestMethod>
    Public Sub Vector_AutoBlankHeaderIsAmbiguous()
        Dim opts As New SpreadsheetVectorParseOptions With {.Header = SpreadsheetHeaderMode.Auto}
        Dim result = TypedNumericRangeParsers.ParseVector(Source(New Object()() {
            New Object() {Nothing}, New Object() {1}}), opts)
        Assert.IsNotNull(Issue(result, "header.ambiguous"))
    End Sub

    <TestMethod>
    Public Sub Paired_ColumnHeadersRemainAlignedWithCoordinates()
        Dim input = Source(New Object()() {
            New Object() {"Before", "After"}, New Object() {1, 10},
            New Object() {2, 20}, New Object() {3, 30}}, 8, 3)
        Dim result = TypedNumericRangeParsers.ParsePaired(input,
            New SpreadsheetPairedParseOptions With {.Header = SpreadsheetHeaderMode.Required})
        Assert.IsTrue(result.Success)
        Assert.AreEqual("Before", result.Data.FirstName)
        Assert.AreEqual("After", result.Data.SecondName)
        CollectionAssert.AreEqual(New Double() {1, 2, 3}, result.Data.FirstValues)
        CollectionAssert.AreEqual(New Double() {10, 20, 30}, result.Data.SecondValues)
        Assert.AreEqual(9, result.Data.Pairs(0).Second.WorksheetRow)
        Assert.AreEqual(4, result.Data.Pairs(0).Second.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub Paired_IncompletePairsAreSkippedAsWholePairs_NotRepacked()
        Dim input = Source(New Object()() {
            New Object() {1, 10}, New Object() {Nothing, 20},
            New Object() {3, Nothing}, New Object() {4, 40}})
        Dim result = TypedNumericRangeParsers.ParsePaired(input,
            New SpreadsheetPairedParseOptions With {.Missing = SpreadsheetIncompleteObservationPolicy.Skip})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {1, 4}, result.Data.FirstValues)
        CollectionAssert.AreEqual(New Double() {10, 40}, result.Data.SecondValues)
        Assert.AreEqual(2, result.Validation.Issues.Where(Function(issue) issue.Code = "paired.incomplete-skipped").Count())
        Assert.AreEqual(4, result.Data.Pairs(1).First.SelectedRow)
    End Sub

    <TestMethod>
    Public Sub Paired_StrictDefaultRejectsIncompletePair()
        Dim result = TypedNumericRangeParsers.ParsePaired(Source(New Object()() {
            New Object() {1, 2}, New Object() {Nothing, 3}}))
        Assert.IsFalse(result.Success)
        Assert.IsNotNull(Issue(result, "paired.incomplete"))
    End Sub

    <TestMethod>
    Public Sub Paired_TwoRowsAndAutoHeader()
        Dim input = Source(New Object()() {
            New Object() {"X", 2, 3}, New Object() {"Y", 4, 5}})
        Dim result = TypedNumericRangeParsers.ParsePaired(input,
            New SpreadsheetPairedParseOptions With {
                .Orientation = SpreadsheetPairOrientation.TwoRows,
                .Header = SpreadsheetHeaderMode.Auto})
        Assert.IsTrue(result.Success)
        Assert.AreEqual("X", result.Data.FirstName)
        CollectionAssert.AreEqual(New Double() {2, 3}, result.Data.FirstValues)
        CollectionAssert.AreEqual(New Double() {4, 5}, result.Data.SecondValues)
        Assert.IsNotNull(Issue(result, "header.inferred"))
    End Sub

    <TestMethod>
    Public Sub Paired_AutoMixedHeaderIsAmbiguous()
        Dim input = Source(New Object()() {
            New Object() {"Before", 1}, New Object() {2, 3}})
        Dim result = TypedNumericRangeParsers.ParsePaired(input,
            New SpreadsheetPairedParseOptions With {.Header = SpreadsheetHeaderMode.Auto})
        Assert.IsNotNull(Issue(result, "header.ambiguous"))
    End Sub

    <TestMethod>
    Public Sub Paired_InvalidIndicesDoNotThrowAndCarryDiagnostics()
        Dim input = Source(New Object()() {New Object() {1, 2}, New Object() {3, 4}})
        Dim result = TypedNumericRangeParsers.ParsePaired(input,
            New SpreadsheetPairedParseOptions With {.FirstIndex = 1, .SecondIndex = 1})
        Assert.IsNotNull(Issue(result, "paired.indices"))
    End Sub

    <TestMethod>
    Public Sub Paired_ActualExcelErrorGetsSourceCoordinates()
        Dim input = Source(New Object()() {
            New Object() {1, 3}, New Object() {SpreadsheetCellValue.ErrorCell("#VALUE!"), 5}}, 5, 8)
        Dim result = TypedNumericRangeParsers.ParsePaired(input)
        Dim diagnostic = Issue(result, "cell.excel-error")
        Assert.IsNotNull(diagnostic)
        Assert.AreEqual(2, diagnostic.SelectedRow)
        Assert.AreEqual(6, diagnostic.WorksheetRow)
        Assert.AreEqual(8, diagnostic.WorksheetColumn)
    End Sub

    <TestMethod>
    Public Sub GroupedTypedAdapterPreservesLegacyNumericReference()
        Dim input = Source(New Object()() {
            New Object() {"A", "B", "C"},
            New Object() {1, 2, 5}, New Object() {2, 3, 6}, New Object() {3, 4, 7}})
        Dim original = GroupedNumericRangeParser.ParseWithDiagnostics(input)
        Dim typed = TypedNumericRangeParsers.ParseGrouped(input)
        Assert.IsTrue(typed.Success)
        CollectionAssert.AreEqual(original.Data.GroupNames, typed.Data.GroupNames)
        For index As Integer = 0 To typed.Data.Groups.Length - 1
            CollectionAssert.AreEqual(original.Data.Groups(index), typed.Data.Groups(index))
        Next
        Assert.AreEqual(13.0R, OneWayAnovaApplicationService.Run(typed.Data).FStatistic, 0.00000000001R)
    End Sub

    <TestMethod>
    Public Sub GroupedTypedAdapterPreservesConfigurableLongFormat()
        Dim input = Source(New Object()() {
            New Object() {"Category", "Value"},
            New Object() {"B", 4}, New Object() {"A", 1},
            New Object() {"A", 2}, New Object() {"B", 5}})
        Dim typed = TypedNumericRangeParsers.ParseGrouped(input,
            New SpreadsheetGroupedParseOptions With {.Orientation = SpreadsheetGroupedOrientation.LongFormat})
        Assert.IsTrue(typed.Success)
        CollectionAssert.AreEqual(New String() {"B", "A"}, typed.Data.GroupNames)
        CollectionAssert.AreEqual(New Double() {4, 5}, typed.Data.Groups(0))
    End Sub

    <TestMethod>
    Public Sub GroupedTypedAdapterRetainsDiagnosticsForMalformedInput()
        Dim input = Source(New Object()() {
            New Object() {"A", "B"}, New Object() {1, 2},
            New Object() {SpreadsheetCellValue.ErrorCell("#VALUE!"), 3}})
        Dim result = TypedNumericRangeParsers.ParseGrouped(input)
        Assert.IsFalse(result.Success)
        Assert.IsNull(result.Data)
        Assert.IsNotNull(Issue(result, "cell.excel-error"))
    End Sub

    <TestMethod>
    Public Sub TypedParserDoesNotMutateSourceCellKinds()
        Dim errorValue = SpreadsheetCellValue.ErrorCell("#N/A")
        Dim input = Source(New Object()() {New Object() {errorValue}})
        Dim result = TypedNumericRangeParsers.ParseScalar(input)
        Assert.IsFalse(result.Success)
        Assert.AreSame(errorValue, input.Rows(0)(0))
        Assert.AreEqual(SpreadsheetCellKind.CellError, input.Rows(0)(0).Kind)
    End Sub

    <TestMethod>
    Public Sub Vector_RowIndexWithinRectangleUsesOnlySelectedRow()
        Dim input = Source(New Object()() {
            New Object() {11, 12, 13}, New Object() {21, 22, 23}})
        Dim result = TypedNumericRangeParsers.ParseVector(input,
            New SpreadsheetVectorParseOptions With {
                .Orientation = SpreadsheetVectorOrientation.Row, .RowIndex = 1})
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {21, 22, 23}, result.Data.Values)
        Assert.AreEqual(2, result.Data.Observations(0).SelectedRow)
    End Sub

    <TestMethod>
    Public Sub Vector_InvalidInputShapeYieldsDiagnosticNotException()
        Dim ragged = Source(New Object()() {New Object() {1, 2}, New Object() {3}})
        Dim result = TypedNumericRangeParsers.ParseVector(ragged)
        Assert.IsFalse(result.Success)
        Assert.IsNotNull(Issue(result, "range.not-rectangular"))
    End Sub

    <TestMethod>
    Public Sub Paired_ExplicitErrorAsMissingStillSkipsWholePair()
        Dim input = Source(New Object()() {
            New Object() {1, 11}, New Object() {SpreadsheetCellValue.ErrorCell("#N/A"), 22},
            New Object() {3, 33}})
        Dim opts As New SpreadsheetPairedParseOptions With {
            .Missing = SpreadsheetIncompleteObservationPolicy.Skip,
            .CellPolicy = New SpreadsheetCellPolicyOptions With {.Errors = SpreadsheetErrorPolicy.TreatAsMissing}}
        Dim result = TypedNumericRangeParsers.ParsePaired(input, opts)
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(New Double() {1, 3}, result.Data.FirstValues)
        CollectionAssert.AreEqual(New Double() {11, 33}, result.Data.SecondValues)
        Assert.IsNotNull(Issue(result, "paired.incomplete-skipped"))
    End Sub

    <TestMethod>
    Public Sub Scalar_DateFormattedNumberStaysNumberWithoutDateConversion()
        Dim cell As SpreadsheetCellValue = SpreadsheetCellValue.NumberCell(46000.0R)
        cell.NumberFormat = "m/d/yyyy"
        Dim input = Source(New Object()() {New Object() {cell}})
        Dim result = TypedNumericRangeParsers.ParseScalar(input)
        Assert.IsTrue(result.Success)
        Assert.AreEqual(46000.0R, result.Data.Value)
        Assert.AreEqual("m/d/yyyy", input.Rows(0)(0).NumberFormat)
    End Sub

End Class
