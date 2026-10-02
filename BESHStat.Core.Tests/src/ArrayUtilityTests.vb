Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.DataManagement

<TestClass>
Public Class ArrayUtilityTests

    <TestMethod>
    Public Sub SubsetRowsByIds_PreservesRequestedEnumerationOrder()
        Dim data(,) As Integer = {
            {10, 11},
            {20, 21},
            {30, 31}
        }

        Dim actual = RowArrayUtilities.SubsetRowsByIds(data, New Integer() {2, 0})

        Assert.AreEqual(2, actual.GetLength(0))
        Assert.AreEqual(2, actual.GetLength(1))
        Assert.AreEqual(30, actual(0, 0))
        Assert.AreEqual(31, actual(0, 1))
        Assert.AreEqual(10, actual(1, 0))
        Assert.AreEqual(11, actual(1, 1))
    End Sub

    <TestMethod>
    Public Sub SubsetItemsByIds_PreservesRequestedEnumerationOrder()
        Dim data() As String = {"A", "B", "C", "D"}

        Dim actual = RowArrayUtilities.SubsetItemsByIds(data, New Integer() {3, 1})

        CollectionAssert.AreEqual(New String() {"D", "B"}, actual)
    End Sub

    <TestMethod>
    Public Sub SubsetRowsByIds_DuplicateRequestedIds_AreRetainedInOrder()
        Dim data(,) As Integer = {
            {10, 11},
            {20, 21},
            {30, 31}
        }

        Dim actual = RowArrayUtilities.SubsetRowsByIds(data, New Integer() {1, 1, 2})

        Assert.AreEqual(3, actual.GetLength(0))
        Assert.AreEqual(20, actual(0, 0))
        Assert.AreEqual(20, actual(1, 0))
        Assert.AreEqual(30, actual(2, 0))
    End Sub

    <TestMethod>
    Public Sub FindCommonItems_RetainsDuplicateMatchingValuesFromFirstArray()
        Dim first() As String = {"A", "A", "B", "C"}
        Dim second() As String = {"A", "C", "A"}

        Dim actual = RowArrayUtilities.FindCommonItems(first, second)

        Assert.AreEqual(3, actual.Count)
        Assert.AreEqual("A", actual(0))
        Assert.AreEqual("A", actual(1))
        Assert.AreEqual("C", actual(3))
    End Sub

    <TestMethod>
    Public Sub FindCommonItems_UsesSuppliedComparer()
        Dim first() As String = {"alpha", "BETA", "gamma"}
        Dim second() As String = {"ALPHA", "beta"}

        Dim actual = RowArrayUtilities.FindCommonItems(first, second, StringComparer.OrdinalIgnoreCase)

        Assert.AreEqual(2, actual.Count)
        Assert.AreEqual("alpha", actual(0))
        Assert.AreEqual("BETA", actual(1))
    End Sub

    <TestMethod>
    Public Sub EmptySelections_ReturnZeroLengthOutputsWithoutLosingColumnCount()
        Dim matrix(,) As Integer = {
            {1, 2, 3},
            {4, 5, 6}
        }
        Dim vector() As Integer = {1, 2, 3}

        Dim rows = RowArrayUtilities.SubsetRowsByIds(matrix, Array.Empty(Of Integer)())
        Dim items = RowArrayUtilities.SubsetItemsByIds(vector, Array.Empty(Of Integer)())

        Assert.AreEqual(0, rows.GetLength(0))
        Assert.AreEqual(3, rows.GetLength(1))
        Assert.AreEqual(0, items.Length)
    End Sub

    <TestMethod>
    Public Sub SubsetRowsByIds_InvalidNegativeIndex_ThrowsBeforeCopying()
        Dim data(,) As Integer = {
            {1, 2},
            {3, 4}
        }

        Dim ex = Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                RowArrayUtilities.SubsetRowsByIds(data, New Integer() {0, -1})
            End Sub)

        Assert.AreEqual("rIds", ex.ParamName)
        StringAssert.Contains(ex.Message, "Row index -1 is outside the valid range 0 to 1.")
    End Sub

    <TestMethod>
    Public Sub SubsetItemsByIds_IndexAboveUpperBound_Throws()
        Dim data() As Integer = {10, 20, 30}

        Dim ex = Assert.ThrowsException(Of ArgumentOutOfRangeException)(
            Sub()
                RowArrayUtilities.SubsetItemsByIds(data, New Integer() {3})
            End Sub)

        Assert.AreEqual("rIds", ex.ParamName)
        StringAssert.Contains(ex.Message, "Row index 3 is outside the valid range 0 to 2.")
    End Sub

    <TestMethod>
    Public Sub SortRows_MultipleKeys_UsesAscendingThenDescendingCriteria()
        Dim data(,) As Integer = {
            {2, 1, 201},
            {1, 2, 102},
            {2, 3, 203},
            {1, 1, 101},
            {2, 2, 202},
            {1, 3, 103}
        }

        RowArrayUtilities.SortRows(data, "0,A,1,D", 0, data.GetUpperBound(0))

        Dim expectedIds() As Integer = {103, 102, 101, 203, 202, 201}
        For i As Integer = 0 To expectedIds.Length - 1
            Assert.AreEqual(expectedIds(i), data(i, 2), $"Unexpected row at sorted position {i}.")
        Next
    End Sub

    <TestMethod>
    Public Sub Slice_ReturnsInclusiveRangeAndPreservesLegacyStartGreaterThanEndBehavior()
        Dim data() As Integer = {10, 20, 30, 40, 50}

        CollectionAssert.AreEqual(New Integer() {20, 30, 40}, ArrayUtilities.Slice(data, 1, 3))
        CollectionAssert.AreEqual(New Integer() {10, 20, 30, 40, 50}, ArrayUtilities.Slice(data))
        CollectionAssert.AreEqual(New Integer() {50}, ArrayUtilities.Slice(data, 99, -1))
    End Sub

    <TestMethod>
    Public Sub GetColumn_ReturnsRequestedColumn()
        Dim data(,) As Integer = {
            {1, 2, 3},
            {4, 5, 6},
            {7, 8, 9}
        }

        CollectionAssert.AreEqual(New Integer() {2, 5, 8}, ArrayUtilities.GetColumn(data, 1))
    End Sub

    <TestMethod>
    Public Sub GetColumn_IndexAboveUpperBound_ThrowsLegacyArgumentException()
        Dim data(,) As Integer = {
            {1, 2},
            {3, 4}
        }

        Dim ex = Assert.ThrowsException(Of ArgumentException)(
            Sub()
                ArrayUtilities.GetColumn(data, 2)
            End Sub)

        StringAssert.Contains(ex.Message, "Provided column number is larger than array 2nd dimension.")
    End Sub

End Class
