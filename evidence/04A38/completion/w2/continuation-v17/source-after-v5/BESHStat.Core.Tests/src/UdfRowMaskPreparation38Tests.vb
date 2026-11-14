Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.WorksheetCompatibility

<TestClass>
Public Class UdfRowMaskPreparation38Tests
    <TestMethod>
    Public Sub MaskRetainsSavedRowPositionsAndCount()
        Dim mask(3) As Boolean
        Dim count As Integer = 0
        UdfProfilePreparation38.FillRowMask(New Double(,) {{1, 4}, {1, 5}, {1, 4}, {1, 6}}, {1.0R, 2.0R, 1.0R, 3.0R}, True, 1.0R,
            New Integer() {1}, Function(x) x, Function(x) 4.0R, Function(a, b) a = b, mask, count)
        CollectionAssert.AreEqual(New Boolean() {True, False, True, False}, mask)
        Assert.AreEqual(2, count)
    End Sub

    <TestMethod>
    Public Sub UnmatchedVisitWritesEveryMaskCellFalse()
        Dim mask As Boolean() = {True, True, True}
        Dim count As Integer = 0
        UdfProfilePreparation38.FillRowMask(New Double(,) {{1}, {1}, {1}}, {1.0R, 2.0R, 3.0R}, True, 99.0R,
            New Integer() {}, Function(x) x, Function(x) 0.0R, Function(a, b) a = b, mask, count)
        CollectionAssert.AreEqual(New Boolean() {False, False, False}, mask)
        Assert.AreEqual(0, count)
    End Sub

    <TestMethod>
    Public Sub ExcludedTimeRowsNeverReadAProfile()
        Dim mask(0) As Boolean
        Dim count As Integer = 0
        UdfProfilePreparation38.FillRowMask(New Double(,) {{1}}, {1.0R}, True, 99.0R,
            New Integer() {0}, Function(x) ThrowColumn(), Function(x) 0.0R, Function(a, b) a = b, mask, count)
        Assert.IsFalse(mask(0))
    End Sub

    Private Shared Function ThrowColumn() As Integer
        Throw New InvalidOperationException("column must remain lazy")
    End Function

    <DataTestMethod, DataRow(-1), DataRow(1)>
    Public Sub InvalidColumnRejectsWithoutReadingValue(columnIndex As Integer)
        Dim mask(0) As Boolean
        Dim count As Integer = 0
        UdfProfilePreparation38.FillRowMask(New Double(,) {{1}}, {1.0R}, False, Double.NaN,
            New Integer() {columnIndex}, Function(x) x, Function(x) CDbl(ThrowColumn()), Function(a, b) a = b, mask, count)
        Assert.IsFalse(mask(0))
        Assert.AreEqual(0, count)
    End Sub

    <TestMethod>
    Public Sub ExceptionalComparisonRetainsEarlierMaskWritesAndCount()
        Dim mask As Boolean() = {False, True, True}
        Dim count As Integer = 0
        Dim calls As Integer = 0
        Dim equal As Func(Of Double, Double, Boolean) =
            Function(a, b)
                calls += 1
                If calls = 2 Then Throw New InvalidOperationException("comparison stopped")
                Return True
            End Function
        Dim ex = Assert.ThrowsException(Of InvalidOperationException)(
            Sub() UdfProfilePreparation38.FillRowMask(New Double(,) {{1}, {1}, {1}}, {1.0R, 2.0R, 3.0R}, True, 1.0R,
                New Integer() {}, Function(x) x, Function(x) 0.0R, equal, mask, count))
        Assert.AreEqual("comparison stopped", ex.Message)
        CollectionAssert.AreEqual(New Boolean() {True, True, True}, mask)
        Assert.AreEqual(1, count)
    End Sub
End Class
