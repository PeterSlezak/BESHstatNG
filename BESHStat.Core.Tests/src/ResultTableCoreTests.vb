Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class ResultTableCoreTests

    <TestMethod>
    Public Sub ResultTable_EmptyTable_ReturnsMinimumOneByOneMatrix()

        Dim table As New ResultTable()

        Assert.AreEqual(1, table.TotalRows)
        Assert.AreEqual(1, table.TotalCols)

        Dim output(,) As Object = table.returnSelf()

        Assert.AreEqual(1, output.GetLength(0))
        Assert.AreEqual(1, output.GetLength(1))
        Assert.IsNull(output(0, 0))

    End Sub


    <TestMethod>
    Public Sub ResultTable_ObjectBody_PreservesDimensionsAndValues()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {"A", 1.5R},
            {"B", 2.5R}
        }

        table.SetBody(body)

        Assert.AreEqual(2, table.TotalRows)
        Assert.AreEqual(2, table.TotalCols)

        Dim output(,) As Object = table.returnSelf()

        Assert.AreEqual(2, output.GetLength(0))
        Assert.AreEqual(2, output.GetLength(1))

        Assert.AreEqual("A", CStr(output(0, 0)))
        Assert.AreEqual(1.5R, CDbl(output(0, 1)), 0.0R)
        Assert.AreEqual("B", CStr(output(1, 0)))
        Assert.AreEqual(2.5R, CDbl(output(1, 1)), 0.0R)

    End Sub


    <TestMethod>
    Public Sub ResultTable_OneDimensionalBody_IsConvertedToSingleColumn()

        Dim table As New ResultTable()

        Dim body() As Object = {"A", "B", "C"}
        table.SetBody(body)

        Dim output(,) As Object = table.returnSelf()

        Assert.AreEqual(3, output.GetLength(0))
        Assert.AreEqual(1, output.GetLength(1))

        Assert.AreEqual("A", CStr(output(0, 0)))
        Assert.AreEqual("B", CStr(output(1, 0)))
        Assert.AreEqual("C", CStr(output(2, 0)))

    End Sub


    <TestMethod>
    Public Sub ResultTable_TitleHeadersBodyAndFootnote_AreAssembledInExpectedPositions()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {1.0R, 0.05R},
            {2.0R, 0.1R}
        }

        table.SetBody(body)
        table.AddHeaderTopRow(New String() {"Statistic", "P"})
        table.AddHeaderLeftRow(New String() {"A", "B"})
        table.AddTitle("Example title")
        table.AddFootnote("Example footnote")

        Assert.AreEqual(5, table.TotalRows)
        Assert.AreEqual(3, table.TotalCols)
        Assert.AreEqual(1, table.HeadersTopCount)
        Assert.AreEqual(1, table.HeadersLeftCount)
        Assert.AreEqual(1, table.TitlesCount)
        Assert.AreEqual(1, table.FootersCount)

        Dim output(,) As Object = table.returnSelf()

        Assert.AreEqual(5, output.GetLength(0))
        Assert.AreEqual(3, output.GetLength(1))

        Assert.AreEqual("Example title", CStr(output(0, 0)))
        Assert.IsNull(output(0, 1))
        Assert.IsNull(output(0, 2))

        Assert.IsNull(output(1, 0))
        Assert.AreEqual("Statistic", CStr(output(1, 1)))
        Assert.AreEqual("P", CStr(output(1, 2)))

        Assert.AreEqual("A", CStr(output(2, 0)))
        Assert.AreEqual(1.0R, CDbl(output(2, 1)), 0.0R)
        Assert.AreEqual(0.05R, CDbl(output(2, 2)), 0.0R)

        Assert.AreEqual("B", CStr(output(3, 0)))
        Assert.AreEqual(2.0R, CDbl(output(3, 1)), 0.0R)
        Assert.AreEqual(0.1R, CDbl(output(3, 2)), 0.0R)

        Assert.AreEqual("Example footnote", CStr(output(4, 0)))
        Assert.IsNull(output(4, 1))
        Assert.IsNull(output(4, 2))

    End Sub


    <TestMethod>
    Public Sub ResultTable_PvalueColumnMetadata_RejectsInvalidAndDuplicateColumns()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {1.0R, 2.0R, 0.01R},
            {3.0R, 4.0R, 0.02R}
        }

        table.SetBody(body)

        table.AddPvalueToFormat(3)
        table.AddPvalueToFormat(3)
        table.AddPvalueToFormat(0)
        table.AddPvalueToFormat(4)

        Assert.AreEqual(1, table.PvalColumns.Count)
        Assert.AreEqual(3, table.PvalColumns(0))

    End Sub


    <TestMethod>
    Public Sub ResultTable_PvalueCellMetadata_RejectsInvalidAndDuplicateCells()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {1.0R, 0.01R},
            {2.0R, 0.02R}
        }

        table.SetBody(body)

        table.AddPvalueCellToFormat(2, 2)
        table.AddPvalueCellToFormat(2, 2)
        table.AddPvalueCellToFormat(0, 1)
        table.AddPvalueCellToFormat(3, 1)
        table.AddPvalueCellToFormat(1, 3)

        Assert.AreEqual(1, table.PvalCells.Count)
        Assert.AreEqual(2, table.PvalCells(0).BodyRow)
        Assert.AreEqual(2, table.PvalCells(0).BodyColumn)

    End Sub


    <TestMethod>
    Public Sub ResultTable_ToOutputModel_PreservesValuesAndMetadata()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {"Estimate", 1.23R, 0.04R},
            {"Other", 4.56R, 0.5R}
        }

        table.SetBody(body)
        table.AddHeaderTopRow(New String() {"Term", "Value", "P"})
        table.AddHeaderLeftRow(New String() {"Row 1", "Row 2"})
        table.AddTitle("Model results")
        table.AddFootnote("End")
        table.AddPvalueToFormat(3)
        table.AddPvalueCellToFormat(1, 3)

        Dim model As ResultTableOutputModel = table.ToOutputModel()

        Assert.IsNotNull(model)
        Assert.IsTrue(model.IsResultTable)

        Assert.AreEqual(table.TotalRows, model.RowCount)
        Assert.AreEqual(table.TotalCols, model.ColumnCount)

        Assert.AreEqual(1, model.HeaderTopRows)
        Assert.AreEqual(1, model.HeaderLeftColumns)
        Assert.AreEqual(1, model.TitleRows)
        Assert.AreEqual(1, model.FooterRows)

        Assert.AreEqual(1, model.PvalueColumns.Count)
        Assert.AreEqual(3, model.PvalueColumns(0))

        Assert.AreEqual(1, model.PvalueCells.Count)
        Assert.AreEqual(1, model.PvalueCells(0).BodyRow)
        Assert.AreEqual(3, model.PvalueCells(0).BodyColumn)

        Assert.AreEqual("Model results", CStr(model.Values(0, 0)))

    End Sub


    <TestMethod>
    Public Sub ResultTableOutputModel_NothingValues_HasZeroDimensions()

        Dim model As New ResultTableOutputModel(Nothing)

        Assert.AreEqual(0, model.RowCount)
        Assert.AreEqual(0, model.ColumnCount)
        Assert.IsTrue(model.IsResultTable)

    End Sub


    <TestMethod>
    Public Sub ResultTableOutputModel_CopiesInputMetadataCollections()

        Dim sourceColumns As New List(Of Integer) From {2, 4}
        Dim sourceCells As New List(Of ResultTableCellAddress) From {
            New ResultTableCellAddress(1, 2)
        }

        Dim values(0, 0) As Object
        values(0, 0) = 0.05R

        Dim model As New ResultTableOutputModel(
            values,
            pvalueColumns:=sourceColumns,
            pvalueCells:=sourceCells)

        sourceColumns.Add(6)
        sourceCells.Add(New ResultTableCellAddress(2, 3))

        Assert.AreEqual(2, model.PvalueColumns.Count)
        Assert.AreEqual(2, model.PvalueColumns(0))
        Assert.AreEqual(4, model.PvalueColumns(1))

        Assert.AreEqual(1, model.PvalueCells.Count)
        Assert.AreEqual(1, model.PvalueCells(0).BodyRow)
        Assert.AreEqual(2, model.PvalueCells(0).BodyColumn)

    End Sub


    <TestMethod>
    Public Sub ResultTableOutputBlock_ComputesInclusiveEndCoordinates()

        Dim values(1, 2) As Object

        Dim model As New ResultTableOutputModel(values)
        Dim block As New ResultTableOutputBlock(5, 7, model)

        Assert.AreEqual(5, block.StartRow)
        Assert.AreEqual(7, block.StartColumn)
        Assert.AreEqual(6, block.EndRow)
        Assert.AreEqual(9, block.EndColumn)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_WritesScalarAndAdvancesRowPointer()

        Dim writer As New CapturingWriter(row:=4, col:=6)

        writer.write("hello")

        Assert.IsNotNull(writer.LastBlock)
        Assert.AreEqual(4, writer.LastBlock.StartRow)
        Assert.AreEqual(6, writer.LastBlock.StartColumn)

        Assert.AreEqual(1, writer.LastBlock.Model.RowCount)
        Assert.AreEqual(1, writer.LastBlock.Model.ColumnCount)
        Assert.AreEqual("hello", CStr(writer.LastBlock.Model.Values(0, 0)))
        Assert.IsFalse(writer.LastBlock.Model.IsResultTable)

        Assert.AreEqual(5, writer.RowID)
        Assert.AreEqual(6, writer.ColID)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_OneDimensionalArray_DefaultsToHorizontal()

        Dim writer As New CapturingWriter()

        Dim values() As Integer = {10, 20, 30}

        writer.write(values)

        Dim model As ResultTableOutputModel = writer.LastBlock.Model

        Assert.AreEqual(1, model.RowCount)
        Assert.AreEqual(3, model.ColumnCount)

        Assert.AreEqual(10, CInt(model.Values(0, 0)))
        Assert.AreEqual(20, CInt(model.Values(0, 1)))
        Assert.AreEqual(30, CInt(model.Values(0, 2)))

        Assert.AreEqual(2, writer.RowID)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_OneDimensionalArray_CanBeWrittenTall()

        Dim writer As New CapturingWriter()

        Dim values() As Integer = {10, 20, 30}

        writer.write(values, bTall:=True)

        Dim model As ResultTableOutputModel = writer.LastBlock.Model

        Assert.AreEqual(3, model.RowCount)
        Assert.AreEqual(1, model.ColumnCount)

        Assert.AreEqual(10, CInt(model.Values(0, 0)))
        Assert.AreEqual(20, CInt(model.Values(1, 0)))
        Assert.AreEqual(30, CInt(model.Values(2, 0)))

        Assert.AreEqual(4, writer.RowID)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_NormalizesNonFiniteFloatingPointValues()

        Dim writer As New CapturingWriter()

        Dim values(,) As Object = {
            {Double.NaN, Double.PositiveInfinity},
            {Double.NegativeInfinity, 1.25R}
        }

        writer.write(values)

        Dim output(,) As Object = writer.LastBlock.Model.Values

        Assert.AreEqual("#N/A", CStr(output(0, 0)))
        Assert.AreEqual("#Pinf", CStr(output(0, 1)))
        Assert.AreEqual("#Ninf", CStr(output(1, 0)))
        Assert.AreEqual(1.25R, CDbl(output(1, 1)), 0.0R)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_ResultTable_PreservesResultMetadata()

        Dim table As New ResultTable()

        Dim body(,) As Object = {
            {"A", 0.01R},
            {"B", 0.2R}
        }

        table.SetBody(body)
        table.AddHeaderTopRow(New String() {"Term", "P"})
        table.AddPvalueToFormat(2)
        table.AddPvalueCellToFormat(1, 2)

        Dim writer As New CapturingWriter(row:=3, col:=2)

        writer.write(table)

        Dim block As ResultTableOutputBlock = writer.LastBlock
        Dim model As ResultTableOutputModel = block.Model

        Assert.AreEqual(3, block.StartRow)
        Assert.AreEqual(2, block.StartColumn)

        Assert.IsTrue(model.IsResultTable)
        Assert.AreEqual(1, model.HeaderTopRows)

        Assert.AreEqual(1, model.PvalueColumns.Count)
        Assert.AreEqual(2, model.PvalueColumns(0))

        Assert.AreEqual(1, model.PvalueCells.Count)
        Assert.AreEqual(1, model.PvalueCells(0).BodyRow)
        Assert.AreEqual(2, model.PvalueCells(0).BodyColumn)

        Assert.AreEqual(3 + model.RowCount, writer.RowID)

    End Sub


    <TestMethod>
    Public Sub ResultTableWriterBase_RejectsArraysWithMoreThanTwoDimensions()

        Dim writer As New CapturingWriter()
        Dim values(1, 1, 1) As Double

        Assert.ThrowsException(Of NotSupportedException)(
            Sub()
                writer.write(values)
            End Sub)

    End Sub


    Private NotInheritable Class CapturingWriter
        Inherits ResultTableWriterBase

        Public Sub New(Optional row As Integer = 1,
                       Optional col As Integer = 1)
            MyBase.New(row, col)
        End Sub

        Public Property LastBlock As ResultTableOutputBlock

        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Me.LastBlock = block
        End Sub
    End Class

End Class
