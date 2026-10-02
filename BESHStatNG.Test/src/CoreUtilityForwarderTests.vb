Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.DataManagement
Imports BESHStatNG.Matrix

<TestClass()>
Public Class CoreUtilityForwarderTests

    <TestMethod()>
    Public Sub Helpers_SubsetArrayByIds_ForwardsToCoreImplementation()
        Dim data(,) As Integer = {
            {10, 11},
            {20, 21},
            {30, 31}
        }
        Dim ids As New Dictionary(Of Integer, Integer) From {
            {2, 0},
            {0, 0}
        }

        Dim legacy = Helpers.SubsetArrayByIds(data, ids)
        Dim core = RowArrayUtilities.SubsetRowsByIds(data, ids.Keys)

        AssertMatrixEqual(core, legacy)
    End Sub

    <TestMethod()>
    Public Sub Helpers_Subset1DArrayByIds_ForwardsToCoreImplementation()
        Dim data() As String = {"A", "B", "C", "D"}
        Dim ids As New Dictionary(Of Integer, Integer) From {
            {3, 0},
            {1, 0}
        }

        CollectionAssert.AreEqual(
            RowArrayUtilities.SubsetItemsByIds(data, ids.Keys),
            Helpers.Subset1DArrayByIds(data, ids))
    End Sub

    <TestMethod()>
    Public Sub Helpers_CommonItems_ForwardsToCoreImplementation()
        Dim first() As String = {"A", "A", "B", "C"}
        Dim second() As String = {"A", "C"}

        Dim legacy = Helpers.CommonItems(first, second)
        Dim core = RowArrayUtilities.FindCommonItems(first, second)

        Assert.AreEqual(core.Count, legacy.Count)
        For Each pair In core
            Assert.IsTrue(legacy.ContainsKey(pair.Key))
            Assert.AreEqual(pair.Value, legacy(pair.Key))
        Next
    End Sub

    <TestMethod()>
    Public Sub Helpers_QuickSort2D_ForwardsToCoreImplementation()
        Dim legacy(,) As Integer = {
            {2, 1, 201},
            {1, 2, 102},
            {2, 3, 203},
            {1, 1, 101}
        }
        Dim core = DirectCast(legacy.Clone(), Integer(,))

        Helpers.QuickSort2D(legacy, "0,A,1,D", 0, legacy.GetUpperBound(0))
        RowArrayUtilities.SortRows(core, "0,A,1,D", 0, core.GetUpperBound(0))

        AssertMatrixEqual(core, legacy)
    End Sub

    <TestMethod()>
    Public Sub Matrix_SubsetArray_ForwardsToCoreImplementation()
        Dim data() As Integer = {10, 20, 30, 40, 50}

        CollectionAssert.AreEqual(
            ArrayUtilities.Slice(data, 1, 3),
            SubsetArray(data, 1, 3))
    End Sub

    <TestMethod()>
    Public Sub Matrix_GetColumnFrom2Darray_ForwardsToCoreImplementation()
        Dim data(,) As Integer = {
            {1, 2},
            {3, 4},
            {5, 6}
        }

        CollectionAssert.AreEqual(
            ArrayUtilities.GetColumn(data, 1),
            GetColumnFrom2Darray(data, 1))
    End Sub

    Private Shared Sub AssertMatrixEqual(Of T)(expected(,) As T, actual(,) As T)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))

        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(i, j), actual(i, j), $"Mismatch at ({i},{j}).")
            Next
        Next
    End Sub

End Class
