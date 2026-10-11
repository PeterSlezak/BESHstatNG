Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.DataManagement
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class MatrixArrayUtilitiesCoreTests

    <TestMethod>
    Public Sub ConcatArrays_PreservesOrderAndDoesNotAliasInputs()
        Dim first() As String = {"a", Nothing}
        Dim second() As String = {"b", "c"}
        Dim actual = ArrayUtilities.ConcatArrays(first, second)

        CollectionAssert.AreEqual(New String() {"a", Nothing, "b", "c"}, actual)
        Assert.AreNotSame(first, actual)
        Assert.AreNotSame(second, actual)
        actual(0) = "changed"
        Assert.AreEqual("a", first(0))
    End Sub

    <TestMethod>
    Public Sub ConcatArrays_EmptyInputsRemainValid()
        CollectionAssert.AreEqual(New Integer() {3}, ArrayUtilities.ConcatArrays(Array.Empty(Of Integer)(), {3}))
        CollectionAssert.AreEqual(New Integer() {3}, ArrayUtilities.ConcatArrays({3}, Array.Empty(Of Integer)()))
        Assert.AreEqual(0, ArrayUtilities.ConcatArrays(Array.Empty(Of Integer)(), Array.Empty(Of Integer)()).Length)
    End Sub

    <TestMethod>
    Public Sub VerticalStackArrays_AppendsColumns_NotRows()
        Dim first(,) As Integer = {{1, 2}, {3, 4}}
        Dim second(,) As Integer = {{5}, {6}}
        Dim result = ArrayUtilities.VerticalStackArrays(first, second)

        Assert.AreEqual(2, result.GetLength(0))
        Assert.AreEqual(3, result.GetLength(1))
        CollectionAssert.AreEqual(New Integer() {1, 2, 5, 3, 4, 6}, Flatten(result))
        Assert.AreEqual(2, first.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub VerticalStackArrays_MismatchedRows_RequirePadding()
        Dim first(,) As String = {{"a"}, {"b"}}
        Dim second(,) As String = {{"x", "y"}}
        Dim ex = Assert.ThrowsException(Of ArgumentException)(Sub() ArrayUtilities.VerticalStackArrays(first, second))
        StringAssert.Contains(ex.Message, "Invalid input array dimensions")

        Dim padded = ArrayUtilities.VerticalStackArrays(first, second, True)
        Assert.AreEqual(2, padded.GetLength(0))
        Assert.AreEqual(3, padded.GetLength(1))
        Assert.AreEqual("a", padded(0, 0))
        Assert.AreEqual("b", padded(1, 0))
        Assert.AreEqual("x", padded(0, 1))
        Assert.AreEqual("y", padded(0, 2))
        Assert.IsNull(padded(1, 1))
        Assert.IsNull(padded(1, 2))
    End Sub

    <TestMethod>
    Public Sub HorizontalStackArrays_AppendsRows_NotColumns()
        Dim first(,) As Integer = {{1, 2}, {3, 4}}
        Dim second(,) As Integer = {{5, 6}}
        Dim result = ArrayUtilities.HorizontalStackArrays(first, second)

        Assert.AreEqual(3, result.GetLength(0))
        Assert.AreEqual(2, result.GetLength(1))
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6}, Flatten(result))
    End Sub

    <TestMethod>
    Public Sub HorizontalStackArrays_MismatchedColumns_RequirePadding()
        Dim first(,) As Integer = {{1}}
        Dim second(,) As Integer = {{2, 3}}
        Dim ex = Assert.ThrowsException(Of ArgumentException)(Sub() ArrayUtilities.HorizontalStackArrays(first, second))
        StringAssert.Contains(ex.Message, "Invalid input array dimensions")

        Dim padded = ArrayUtilities.HorizontalStackArrays(first, second, True)
        Assert.AreEqual(2, padded.GetLength(0))
        Assert.AreEqual(2, padded.GetLength(1))
        CollectionAssert.AreEqual(New Integer() {1, 0, 2, 3}, Flatten(padded))
    End Sub

    <TestMethod>
    Public Sub Stacking_EmptyRankTwoDimensions_ArePreserved()
        Dim noRows As Integer(,) = DirectCast(Array.CreateInstance(GetType(Integer), 0, 2), Integer(,))
        Dim oneRow(,) As Integer = {{7, 8}}
        Dim stackedRows = ArrayUtilities.HorizontalStackArrays(noRows, oneRow)
        Assert.AreEqual(1, stackedRows.GetLength(0))
        Assert.AreEqual(2, stackedRows.GetLength(1))
        Assert.AreEqual(7, stackedRows(0, 0))

        Dim noColumns As Integer(,) = DirectCast(Array.CreateInstance(GetType(Integer), 2, 0), Integer(,))
        Dim stackedCols = ArrayUtilities.VerticalStackArrays(noColumns, DirectCast(Array.CreateInstance(GetType(Integer), 2, 1), Integer(,)))
        Assert.AreEqual(2, stackedCols.GetLength(0))
        Assert.AreEqual(1, stackedCols.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub Array2strArray_OneAndTwoDimensions_PreserveNothingAndShape()
        Dim items() As Object = {12, Nothing, "hello", DBNull.Value}
        CollectionAssert.AreEqual(New String() {"12", "", "hello", ""}, ArrayUtilities.Array2strArray(items))

        Dim cells(,) As Object = {{Nothing, "alpha"}, {10, DBNull.Value}}
        Dim strings = ArrayUtilities.Array2strArray(cells)
        Assert.AreEqual(2, strings.GetLength(0))
        Assert.AreEqual(2, strings.GetLength(1))
        Assert.AreEqual("", strings(0, 0))
        Assert.AreEqual("alpha", strings(0, 1))
        Assert.AreEqual("10", strings(1, 0))
        Assert.AreEqual("", strings(1, 1))
    End Sub

    <TestMethod>
    Public Sub Array2dblArray_UsesBclConversionAndPreservesSpecialValues()
        Dim actual = ArrayUtilities.Array2dblArray(New Object() {Nothing, "2", -3, 1.25R, Double.NaN})
        Assert.AreEqual(5, actual.Length)
        Assert.AreEqual(0.0R, actual(0))
        Assert.AreEqual(2.0R, actual(1))
        Assert.AreEqual(-3.0R, actual(2))
        Assert.AreEqual(1.25R, actual(3))
        Assert.IsTrue(Double.IsNaN(actual(4)))

        Assert.ThrowsException(Of FormatException)(Sub() ArrayUtilities.Array2dblArray(New String() {"n/a"}))
    End Sub

    <TestMethod>
    Public Sub Array2intArray_BothDimensions_UseBankersRoundingAndNothingToZero()
        Dim actual = ArrayUtilities.Array2intArray(New Object() {Nothing, 1.5R, 2.5R, -3.5R, "6"})
        CollectionAssert.AreEqual(New Integer() {0, 2, 2, -4, 6}, actual)

        Dim values(,) As Object = {{Nothing, 1.5R, 2.5R}, {-3.5R, "6", -1.5R}}
        Dim ints = ArrayUtilities.Array2intArray(values)
        Assert.AreEqual(2, ints.GetLength(0))
        Assert.AreEqual(3, ints.GetLength(1))
        CollectionAssert.AreEqual(New Integer() {0, 2, 2, -4, 6, -2}, Flatten(ints))
        Assert.ThrowsException(Of OverflowException)(Sub() ArrayUtilities.Array2intArray(New Double() {Double.MaxValue}))
    End Sub

    <TestMethod>
    Public Sub Array2objArray_BoxesElementsAndKeepsOriginalShape()
        Dim values(,) As Double = {{1.25R, 2.5R}, {-3.0R, 4.0R}}
        Dim boxed = ArrayUtilities.Array2objArray(values)

        Assert.AreEqual(2, boxed.GetLength(0))
        Assert.AreEqual(2, boxed.GetLength(1))
        Assert.AreEqual(1.25R, DirectCast(boxed(0, 0), Double))
        Assert.AreEqual(-3.0R, DirectCast(boxed(1, 0), Double))
        boxed(0, 0) = 99.0R
        Assert.AreEqual(1.25R, values(0, 0))
    End Sub

    <TestMethod>
    Public Sub Conversions_EmptyVectorsAndMatrices_KeepTheirDimensions()
        Assert.AreEqual(0, ArrayUtilities.Array2strArray(Array.Empty(Of Object)()).Length)
        Assert.AreEqual(0, ArrayUtilities.Array2dblArray(Array.Empty(Of Object)()).Length)
        Assert.AreEqual(0, ArrayUtilities.Array2intArray(Array.Empty(Of Object)()).Length)

        Dim empty As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), 0, 3), Object(,))
        Dim strs = ArrayUtilities.Array2strArray(empty)
        Dim ints = ArrayUtilities.Array2intArray(empty)
        Dim objects = ArrayUtilities.Array2objArray(empty)
        Assert.AreEqual(0, strs.GetLength(0))
        Assert.AreEqual(3, strs.GetLength(1))
        Assert.AreEqual(0, ints.GetLength(0))
        Assert.AreEqual(3, ints.GetLength(1))
        Assert.AreEqual(0, objects.GetLength(0))
        Assert.AreEqual(3, objects.GetLength(1))
    End Sub

    Private Shared Function Flatten(Of T)(values(,) As T) As T()
        Dim result(values.Length - 1) As T
        Dim offset As Integer = 0
        For i As Integer = 0 To values.GetLength(0) - 1
            For j As Integer = 0 To values.GetLength(1) - 1
                result(offset) = values(i, j)
                offset += 1
            Next
        Next
        Return result
    End Function
End Class
