Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Multivariate17AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub AcquiredIdentityAndActualWriterConsumeDetachedReport(kindValue As Integer)
        Dim data As New DataObj, values(29, 2) As Object, x = Multivariate17Fixture.Data()
        For i = 0 To 29
            For j = 0 To 2
                values(i, j) = x(i, j)
            Next
        Next
        data.DataImportRawMatrix(values, Multivariate17Fixture.Names, firstSourceRow:=101)
        data.SubsetByRowIdValues(New Dictionary(Of Integer, Integer) From {{0, data.RowIds(0)}, {3, data.RowIds(3)}, {9, data.RowIds(9)}})
        Dim kind = CType(kindValue, MultivariateAnalysisKind), labels = {"dup", "dup", "other"}, groups = {"a", "A", "a"}
        Dim input = WindowsMultivariateWorkflowAdapter.Capture(kind, data, labels, groups)
        CollectionAssert.AreEqual(data.RowIds, input.SourceRowIds)
        Assert.IsTrue(input.SourceRowIds(1) > input.SourceRowIds(0) + 1)
        Dim original = Multivariate17Fixture.Fit(kind, 0, False)
        Dim report = MultivariateReportPolicy.Project(input, original)
        Dim writer As New MemoryWriter, legacyWriter As New MemoryWriter
        Dim currentProcess As New ProcessListofResultTables(report.LegacyTables()), originalProcess As New ProcessListofResultTables(original)
        currentProcess.writeToSheet(writer, True)
        originalProcess.writeToSheet(legacyWriter, True)
        Assert.AreEqual(legacyWriter.Blocks.Count, writer.Blocks.Count)
        For i = 0 To writer.Blocks.Count - 1
            Assert.AreEqual(legacyWriter.Blocks(i).StartRow, writer.Blocks(i).StartRow)
            Assert.AreEqual(legacyWriter.Blocks(i).StartColumn, writer.Blocks(i).StartColumn)
            Multivariate17Fixture.Cells(legacyWriter.Blocks(i).Model.Values, writer.Blocks(i).Model.Values)
        Next
        labels(0) = "changed" : groups(0) = "changed"
        Assert.AreEqual("dup", report.Input.RowLabels(0)) : Assert.AreEqual("a", report.Input.GroupLabels(0))
        Dim second = WindowsMultivariateWorkflowAdapter.Capture(kind, data, labels, groups)
        Assert.AreEqual("changed", second.RowLabels(0)) : Assert.AreEqual("dup", input.RowLabels(0))
    End Sub
    <TestMethod>
    Public Sub McaFirstRowSubsetPreservesAcquiredPhysicalIdentity()
        Dim data As New DataObj
        data.DataImportRawMatrix(New Object(,) {{"header", "category"}, {"a", "low"}, {"A", "high"}, {"a", "high"}}, {"x", "y"}, CharCols:=20000, firstSourceRow:=101)
        Dim before = DirectCast(data.RowIds.Clone(), Integer()), ids As New Dictionary(Of Integer, Integer)
        For i = 1 To data.nRows - 1
            ids.Add(i, data.RowIds(i))
        Next
        data.SubsetByRowIdValues(ids)
        Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.MCA, data)
        CollectionAssert.AreEqual(before.Skip(1).ToArray(), input.SourceRowIds)
        Assert.AreEqual(MultivariateCellSemantics.CategoricalAnalysis, input.CellSemantics)
        Assert.AreEqual("a", data.FinalData(0, 0)) : Assert.AreEqual("A", data.FinalData(1, 0))
    End Sub
    <TestMethod>
    Public Sub OriginalStandaloneCaNoLabelIndexDefectIsNotHidden()
        ' Exact nonempty no-label source loop, isolated from native acquisition/writer.
        ' C17-CA-NOLABEL-INDEX-01: compatibility-only defect, not a successful CA fit.
        Dim cells As Object(,) = {{2.0, 3.0}, {4.0, 5.0}}
        Dim table(cells.GetLength(0) - 1, cells.GetLength(1) - 1) As Integer
        Assert.ThrowsException(Of IndexOutOfRangeException)(
            Sub()
                For i = 0 To UBound(cells, 1)
                    For j = 0 To UBound(cells, 2)
                        table(i, j - 1) = CInt(cells(i, j))
                    Next
                Next
            End Sub)
    End Sub
End Class
