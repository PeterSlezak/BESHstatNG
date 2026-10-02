Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class DataObjPreparationTests

    <TestMethod>
    Public Sub DataImportRawMatrix_PreservesHostNeutralSourceMetadata()
        Dim raw(,) As Object = {
            {1.0, 2.0},
            {3.0, 4.0}
        }
        Dim names() As String = {"Y", "X"}
        Dim source As New DataSourceInfo With {
            .SourceKind = "UnitTest",
            .Address = "A10:B11",
            .SheetName = "Input",
            .FirstSourceColumn = 3
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 names,
                                 firstSourceRow:=10,
                                 sourceInfo:=source)

        Assert.IsFalse(data.bZeroValid)
        Assert.AreEqual(2, data.nRows)
        Assert.AreEqual(2, data.nCols)
        CollectionAssert.AreEqual(New Integer() {10, 11}, data.RowIds)
        Assert.IsNotNull(data.SourceInfo)
        Assert.AreEqual("UnitTest", data.SourceInfo.SourceKind)
        Assert.AreEqual("A10:B11", data.SourceInfo.Address)
        Assert.AreEqual("Input", data.SourceInfo.SheetName)
        Assert.AreEqual(10, data.SourceInfo.FirstSourceRow)
        Assert.AreEqual(3, data.SourceInfo.FirstSourceColumn)
        CollectionAssert.AreEqual(names, data.SourceInfo.ColumnNames)

        ' DataObj clones neutral metadata before CoreDataTable normalizes it.
        Assert.IsNull(source.ColumnNames)
        Assert.AreEqual(1, source.FirstSourceRow)
    End Sub

    <TestMethod>
    Public Sub DataImportRawMatrixWithOptions_UsesNeutralSourceInfo()
        Dim raw(,) As Object = {
            {5.0},
            {6.0}
        }
        Dim source As New DataSourceInfo With {
            .SourceKind = "OriginalKind",
            .SheetName = "SheetMeta"
        }
        Dim options As New DataImportOptions With {
            .FirstSourceRow = 20,
            .SourceInfo = source,
            .SourceKind = "OptionsKind",
            .SourceAddress = "C20:C21"
        }

        Dim data As New DataObj()
        data.DataImportRawMatrixWithOptions(raw, New String() {"Value"}, options)

        Assert.AreEqual("OptionsKind", data.SourceInfo.SourceKind)
        Assert.AreEqual("C20:C21", data.SourceInfo.Address)
        Assert.AreEqual("SheetMeta", data.SourceInfo.SheetName)
        Assert.AreEqual(20, data.SourceInfo.FirstSourceRow)
        CollectionAssert.AreEqual(New Integer() {20, 21}, data.RowIds)
    End Sub

    <TestMethod>
    Public Sub DataByID2ByColumn_PreservesGroupingBehavior()
        Dim raw(,) As Object = {
            {"A", 1.0},
            {"B", 2.0},
            {"A", 3.0}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 New String() {"Group", "Value"},
                                 CharCols:=0)

        Dim grouped()() As Double = data.DataByID2ByColumn

        Assert.AreEqual(2, grouped.Length)
        CollectionAssert.AreEqual(New Double() {1.0, 3.0}, grouped(0))
        CollectionAssert.AreEqual(New Double() {2.0}, grouped(1))
    End Sub

    <TestMethod>
    Public Sub DataObj_SubsetByRowIdValues_PreservesSelectionAndRowIds()
        Dim raw(,) As Object = {
            {10.0, 100.0},
            {20.0, 200.0},
            {30.0, 300.0}
        }
        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X"},
                                 firstSourceRow:=5)

        Dim selected As New Dictionary(Of Integer, Integer) From {
            {2, 7},
            {0, 5}
        }
        data.SubsetByRowIdValues(selected)

        Assert.AreEqual(30.0, Convert.ToDouble(data.FinalData(0, 0)), 0.0)
        Assert.AreEqual(10.0, Convert.ToDouble(data.FinalData(1, 0)), 0.0)
        CollectionAssert.AreEqual(New Integer() {7, 5}, data.RowIds)
    End Sub

    <TestMethod>
    Public Sub GlmData_RawImport_SplitsWeightsAndOffset()
        Dim raw(,) As Object = {
            {1.0, 10.0, 0.1, 1.5},
            {2.0, 20.0, 0.2, 2.5}
        }
        Dim data As New glmData With {
            .bOffset = True,
            .bWeights = True
        }

        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X", "Offset", "Weight"})

        Assert.AreEqual(2, data.nCols)
        Assert.AreEqual("Offset", data.OffsetVarName)
        Assert.AreEqual("Weight", data.WeightVarName)
        CollectionAssert.AreEqual(New Double() {0.1, 0.2}, data.OffsetData)
        CollectionAssert.AreEqual(New Double() {1.5, 2.5}, data.WeightData)
        Assert.AreEqual(1.0, Convert.ToDouble(data.FinalData(0, 0)), 0.0)
        Assert.AreEqual(20.0, Convert.ToDouble(data.FinalData(1, 1)), 0.0)
    End Sub

    <TestMethod>
    Public Sub GeeData_RawImport_SortsByClusterThenTime()
        Dim raw(,) As Object = {
            {10.0, 2.0, 2.0},
            {20.0, 1.0, 3.0},
            {30.0, 1.0, 1.0},
            {40.0, 2.0, 1.0}
        }
        Dim data As New geeData With {
            .bTime = True
        }

        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "Cluster", "Time"})

        Assert.AreEqual(1, data.nCols)
        CollectionAssert.AreEqual(New Integer() {3, 2, 4, 1}, data.RowIds)
        CollectionAssert.AreEqual(New Object() {1.0, 1.0, 2.0, 2.0}, data.ClusterIdData)
        CollectionAssert.AreEqual(New Double() {1.0, 3.0, 1.0, 2.0}, data.TimeData)
        Assert.AreEqual(30.0, Convert.ToDouble(data.FinalData(0, 0)), 0.0)
        Assert.AreEqual(20.0, Convert.ToDouble(data.FinalData(1, 0)), 0.0)
        Assert.AreEqual(40.0, Convert.ToDouble(data.FinalData(2, 0)), 0.0)
        Assert.AreEqual(10.0, Convert.ToDouble(data.FinalData(3, 0)), 0.0)
    End Sub

End Class
