Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class CoreDataTableTests

    <TestMethod>
    Public Sub FromObjectMatrix_BuildsExpectedShapeRowIdsAndMetadata()

        Dim values(,) As Object = {
            {1.0R, "A"},
            {2.0R, "B"},
            {3.0R, "C"}
        }

        Dim source As New DataSourceInfo With {
            .SourceKind = "WorksheetRange",
            .Address = "$B$5:$C$7",
            .SheetName = "Data",
            .FirstSourceColumn = 2
        }

        Dim table As CoreDataTable =
            CoreDataTable.FromObjectMatrix(
                values,
                New String() {"Value", "Group"},
                firstSourceRow:=5,
                sourceInfo:=source)

        Assert.AreEqual(3, table.RowCount)
        Assert.AreEqual(2, table.ColumnCount)

        CollectionAssert.AreEqual(
            New Integer() {5, 6, 7},
            table.RowIds)

        CollectionAssert.AreEqual(
            New String() {"Value", "Group"},
            table.ColumnNames)

        Assert.AreEqual(5, table.FirstSourceRow)
        Assert.AreEqual(2, table.FirstSourceColumn)

        Assert.IsNotNull(table.SourceInfo)
        Assert.AreEqual("WorksheetRange", table.SourceInfo.SourceKind)
        Assert.AreEqual("$B$5:$C$7", table.SourceInfo.Address)
        Assert.AreEqual("Data", table.SourceInfo.SheetName)
        Assert.AreEqual(5, table.SourceInfo.FirstSourceRow)
        Assert.AreEqual(2, table.SourceInfo.FirstSourceColumn)

        CollectionAssert.AreEqual(
            New String() {"Value", "Group"},
            table.SourceInfo.ColumnNames)

    End Sub


    <TestMethod>
    Public Sub FromObjectMatrix_DefaultOwnership_DefensivelyCopiesValuesAndColumnNames()

        Dim values(,) As Object = {
            {1.0R, "A"},
            {2.0R, "B"}
        }

        Dim names() As String = {"Value", "Group"}

        Dim table As CoreDataTable =
            CoreDataTable.FromObjectMatrix(values, names)

        Assert.IsFalse(Object.ReferenceEquals(values, table.ObjectMatrix))
        Assert.IsFalse(Object.ReferenceEquals(names, table.ColumnNames))

        values(0, 0) = 999.0R
        values(0, 1) = "Changed"
        names(0) = "Changed name"

        Assert.AreEqual(1.0R, CDbl(table.ObjectMatrix(0, 0)), 0.0R)
        Assert.AreEqual("A", CStr(table.ObjectMatrix(0, 1)))
        Assert.AreEqual("Value", table.ColumnNames(0))

    End Sub


    <TestMethod>
    Public Sub FromObjectMatrix_CopyValuesFalse_TransfersObjectMatrixOwnership()

        Dim values(,) As Object = {
            {1.0R, 2.0R},
            {3.0R, 4.0R}
        }

        Dim table As CoreDataTable =
            CoreDataTable.FromObjectMatrix(
                values,
                New String() {"A", "B"},
                copyValues:=False)

        Assert.IsTrue(Object.ReferenceEquals(values, table.ObjectMatrix))

        values(0, 0) = 123.0R
        Assert.AreEqual(123.0R, CDbl(table.ObjectMatrix(0, 0)), 0.0R)

    End Sub


    <TestMethod>
    Public Sub FromObjectMatrix_ClampsSourceCoordinatesToOne()

        Dim values(,) As Object = {{1.0R}}

        Dim source As New DataSourceInfo With {
            .FirstSourceRow = -20,
            .FirstSourceColumn = 0
        }

        Dim table As CoreDataTable =
            CoreDataTable.FromObjectMatrix(
                values,
                New String() {"X"},
                firstSourceRow:=0,
                sourceInfo:=source)

        Assert.AreEqual(1, table.FirstSourceRow)
        Assert.AreEqual(1, table.FirstSourceColumn)
        CollectionAssert.AreEqual(New Integer() {1}, table.RowIds)

        Assert.AreEqual(1, table.SourceInfo.FirstSourceRow)
        Assert.AreEqual(1, table.SourceInfo.FirstSourceColumn)

    End Sub


    <TestMethod>
    Public Sub FromObjectMatrix_RejectsNullInputsAndColumnCountMismatch()

        Dim names() As String = {"A"}
        Dim values(,) As Object = {{1.0R, 2.0R}}

        Dim nullValues As ArgumentNullException =
            Assert.ThrowsException(Of ArgumentNullException)(
                Sub()
                    CoreDataTable.FromObjectMatrix(Nothing, names)
                End Sub)

        Assert.AreEqual("values", nullValues.ParamName)

        Dim nullNames As ArgumentNullException =
            Assert.ThrowsException(Of ArgumentNullException)(
                Sub()
                    CoreDataTable.FromObjectMatrix(values, Nothing)
                End Sub)

        Assert.AreEqual("columnNames", nullNames.ParamName)

        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                CoreDataTable.FromObjectMatrix(values, names)
            End Sub)

    End Sub


    <TestMethod>
    Public Sub IsMissingValue_RecognizesOnlyConfiguredMissingRepresentations()

        Assert.IsTrue(CoreDataTable.IsMissingValue(Nothing))
        Assert.IsTrue(CoreDataTable.IsMissingValue(DBNull.Value))
        Assert.IsTrue(CoreDataTable.IsMissingValue(String.Empty))
        Assert.IsTrue(CoreDataTable.IsMissingValue("   "))
        Assert.IsTrue(CoreDataTable.IsMissingValue(vbTab))

        Assert.IsFalse(CoreDataTable.IsMissingValue("0"))
        Assert.IsFalse(CoreDataTable.IsMissingValue(0.0R))
        Assert.IsFalse(CoreDataTable.IsMissingValue(False))
        Assert.IsFalse(CoreDataTable.IsMissingValue("NA"))

    End Sub


    <TestMethod>
    Public Sub FromObjectMatrix_BuildsMissingMaskAndNumericMatrixConsistently()

        Dim values(,) As Object = {
            {1, 2.5R, "3.75", Nothing},
            {CDec(4.25R), CSng(5.5R), "text", DBNull.Value},
            {CByte(6), CLng(7), "   ", True}
        }

        Dim table As CoreDataTable =
            CoreDataTable.FromObjectMatrix(
                values,
                New String() {"A", "B", "C", "D"})

        Assert.IsFalse(table.MissingMask(0, 0))
        Assert.IsFalse(table.MissingMask(0, 1))
        Assert.IsFalse(table.MissingMask(0, 2))
        Assert.IsTrue(table.MissingMask(0, 3))

        Assert.IsFalse(table.MissingMask(1, 0))
        Assert.IsFalse(table.MissingMask(1, 1))
        Assert.IsFalse(table.MissingMask(1, 2))
        Assert.IsTrue(table.MissingMask(1, 3))

        Assert.IsFalse(table.MissingMask(2, 0))
        Assert.IsFalse(table.MissingMask(2, 1))
        Assert.IsTrue(table.MissingMask(2, 2))
        Assert.IsFalse(table.MissingMask(2, 3))

        Assert.AreEqual(1.0R, table.NumericMatrix(0, 0), 0.0R)
        Assert.AreEqual(2.5R, table.NumericMatrix(0, 1), 0.0R)
        Assert.AreEqual(3.75R, table.NumericMatrix(0, 2), 0.0R)
        Assert.IsTrue(Double.IsNaN(table.NumericMatrix(0, 3)))

        Assert.AreEqual(4.25R, table.NumericMatrix(1, 0), 0.0R)
        Assert.AreEqual(5.5R, table.NumericMatrix(1, 1), 0.0R)
        Assert.IsTrue(Double.IsNaN(table.NumericMatrix(1, 2)))
        Assert.IsTrue(Double.IsNaN(table.NumericMatrix(1, 3)))

        Assert.AreEqual(6.0R, table.NumericMatrix(2, 0), 0.0R)
        Assert.AreEqual(7.0R, table.NumericMatrix(2, 1), 0.0R)
        Assert.IsTrue(Double.IsNaN(table.NumericMatrix(2, 2)))
        Assert.IsTrue(Double.IsNaN(table.NumericMatrix(2, 3)))

    End Sub


    <TestMethod>
    <DoNotParallelize>
    Public Sub BuildNumericMatrix_StringConversion_UsesCurrentCulture()

        Dim originalCulture As CultureInfo = CultureInfo.CurrentCulture

        Try
            CultureInfo.CurrentCulture = New CultureInfo("fr-FR")

            Dim values(,) As Object = {
                {"1,5", "not-a-number"}
            }

            Dim numeric(,) As Double =
                CoreDataTable.BuildNumericMatrix(values)

            Assert.AreEqual(1.5R, numeric(0, 0), 0.0R)
            Assert.IsTrue(Double.IsNaN(numeric(0, 1)))

        Finally
            CultureInfo.CurrentCulture = originalCulture
        End Try

    End Sub


    <TestMethod>
    Public Sub BuildNumericMatrix_HonorsProvidedMissingMask()

        Dim values(,) As Object = {
            {10.0R, 20.0R}
        }

        Dim mask(,) As Boolean = {
            {False, True}
        }

        Dim numeric(,) As Double =
            CoreDataTable.BuildNumericMatrix(values, mask)

        Assert.AreEqual(10.0R, numeric(0, 0), 0.0R)
        Assert.IsTrue(Double.IsNaN(numeric(0, 1)))

    End Sub


    <TestMethod>
    Public Sub Clone_CreatesIndependentTableArraysAndSourceMetadata()

        Dim values(,) As Object = {
            {1.0R, Nothing},
            {2.0R, "3"}
        }

        Dim source As New DataSourceInfo With {
            .SourceKind = "Test",
            .Address = "A1:B2",
            .SheetName = "Sheet1",
            .FirstSourceColumn = 4
        }

        Dim original As CoreDataTable =
            CoreDataTable.FromObjectMatrix(
                values,
                New String() {"X", "Y"},
                firstSourceRow:=10,
                sourceInfo:=source)

        Dim clone As CoreDataTable = original.Clone()

        Assert.IsFalse(Object.ReferenceEquals(original, clone))
        Assert.IsFalse(Object.ReferenceEquals(original.ColumnNames, clone.ColumnNames))
        Assert.IsFalse(Object.ReferenceEquals(original.RowIds, clone.RowIds))
        Assert.IsFalse(Object.ReferenceEquals(original.ObjectMatrix, clone.ObjectMatrix))
        Assert.IsFalse(Object.ReferenceEquals(original.NumericMatrix, clone.NumericMatrix))
        Assert.IsFalse(Object.ReferenceEquals(original.MissingMask, clone.MissingMask))
        Assert.IsFalse(Object.ReferenceEquals(original.SourceInfo, clone.SourceInfo))
        Assert.IsFalse(Object.ReferenceEquals(original.SourceInfo.ColumnNames, clone.SourceInfo.ColumnNames))

        clone.ColumnNames(0) = "Changed"
        clone.RowIds(0) = 999
        clone.ObjectMatrix(0, 0) = 999.0R
        clone.NumericMatrix(0, 0) = 999.0R
        clone.MissingMask(0, 1) = False
        clone.SourceInfo.SheetName = "Changed"
        clone.SourceInfo.ColumnNames(0) = "Changed"

        Assert.AreEqual("X", original.ColumnNames(0))
        Assert.AreEqual(10, original.RowIds(0))
        Assert.AreEqual(1.0R, CDbl(original.ObjectMatrix(0, 0)), 0.0R)
        Assert.AreEqual(1.0R, original.NumericMatrix(0, 0), 0.0R)
        Assert.IsTrue(original.MissingMask(0, 1))
        Assert.AreEqual("Sheet1", original.SourceInfo.SheetName)
        Assert.AreEqual("X", original.SourceInfo.ColumnNames(0))

    End Sub


    <TestMethod>
    Public Sub CopyColumnNames_NormalizesNothingEntriesAndReturnsIndependentArray()

        Dim names() As String = {"A", Nothing, "C"}

        Dim copy() As String = CoreDataTable.CopyColumnNames(names)

        Assert.IsFalse(Object.ReferenceEquals(names, copy))
        CollectionAssert.AreEqual(
            New String() {"A", String.Empty, "C"},
            copy)

        names(0) = "Changed"
        Assert.AreEqual("A", copy(0))

    End Sub


    <TestMethod>
    Public Sub CopyObjectMatrix_ReturnsIndependentMatrixWithSameValues()

        Dim values(,) As Object = {
            {1.0R, "A"},
            {2.0R, "B"}
        }

        Dim copy(,) As Object = CoreDataTable.CopyObjectMatrix(values)

        Assert.IsFalse(Object.ReferenceEquals(values, copy))
        Assert.AreEqual(values.GetLength(0), copy.GetLength(0))
        Assert.AreEqual(values.GetLength(1), copy.GetLength(1))
        Assert.AreEqual(1.0R, CDbl(copy(0, 0)), 0.0R)
        Assert.AreEqual("B", CStr(copy(1, 1)))

        values(0, 0) = 999.0R
        Assert.AreEqual(1.0R, CDbl(copy(0, 0)), 0.0R)

    End Sub


    <TestMethod>
    Public Sub BuildSequentialRowIds_HandlesOffsetsAndNonPositiveCounts()

        CollectionAssert.AreEqual(
            New Integer() {7, 8, 9},
            CoreDataTable.BuildSequentialRowIds(3, 7))

        CollectionAssert.AreEqual(
            New Integer() {1, 2},
            CoreDataTable.BuildSequentialRowIds(2, -5))

        Assert.AreEqual(
            0,
            CoreDataTable.BuildSequentialRowIds(0, 10).Length)

        Assert.AreEqual(
            0,
            CoreDataTable.BuildSequentialRowIds(-1, 10).Length)

    End Sub

End Class
