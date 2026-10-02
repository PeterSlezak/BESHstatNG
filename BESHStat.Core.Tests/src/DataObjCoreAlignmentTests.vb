Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG

<TestClass>
Public Class DataObjCoreAlignmentTests

    <TestMethod>
    Public Sub DataObj_SkipRows_PreservesSourceRowIds()
        Dim raw(,) As Object = {
            {10.0R, 100.0R},
            {20.0R, 200.0R},
            {30.0R, 300.0R},
            {40.0R, 400.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X"},
                                 firstSourceRow:=10,
                                 SkipRow:=2)

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {12, 13}, data.RowIds)
        Assert.AreEqual(30.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(40.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_MissingDisallowed_DropsRowsAndKeepsOriginalRowIds()
        Dim raw(,) As Object = {
            {1.0R, 10.0R},
            {2.0R, Nothing},
            {3.0R, 30.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X"},
                                 firstSourceRow:=5)

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {5, 7}, data.RowIds)
        Assert.AreEqual(1.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(3.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_AllowMissing_KeepsPartialMissingAndDataDblUsesNaN()
        Dim raw(,) As Object = {
            {1.0R, Nothing},
            {2.0R, 20.0R}
        }

        Dim options As New DataImportOptions With {
            .AllowMissing = True,
            .FirstSourceRow = 20
        }

        Dim data As New DataObj()
        data.DataImportRawMatrixWithOptions(raw, New String() {"Y", "X"}, options)

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {20, 21}, data.RowIds)
        Assert.IsNull(data.FinalData(0, 1))

        Dim numeric(,) As Double = data.DataDbl
        Assert.AreEqual(1.0R, numeric(0, 0), 0.0R)
        Assert.IsTrue(Double.IsNaN(numeric(0, 1)))
        Assert.AreEqual(20.0R, numeric(1, 1), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_AllowMissing_DropsCompletelyMissingRows()
        Dim raw(,) As Object = {
            {Nothing, Nothing},
            {2.0R, 20.0R},
            {3.0R, Nothing}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrixWithOptions(
            raw,
            New String() {"Y", "X"},
            New DataImportOptions With {.AllowMissing = True, .FirstSourceRow = 30})

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {31, 32}, data.RowIds)
        Assert.AreEqual(2.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(3.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_AllowMissing_DropsAllMissingColumnsAndKeepsAlignment()
        Dim raw(,) As Object = {
            {1.0R, Nothing, 10.0R},
            {2.0R, Nothing, Nothing},
            {3.0R, Nothing, 30.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrixWithOptions(
            raw,
            New String() {"Y", "DropMe", "X"},
            New DataImportOptions With {.AllowMissing = True, .FirstSourceRow = 40})

        Assert.IsFalse(data.bZeroValid)
        Assert.AreEqual(3, data.nRows)
        Assert.AreEqual(2, data.nCols)
        CollectionAssert.AreEqual(New String() {"Y", "X"}, data.varNames)
        CollectionAssert.AreEqual(New Integer() {40, 41, 42}, data.RowIds)
        Assert.AreEqual(10.0R, Convert.ToDouble(data.FinalData(0, 1)), 0.0R)
        Assert.IsNull(data.FinalData(1, 1))
        Assert.AreEqual(30.0R, Convert.ToDouble(data.FinalData(2, 1)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_AllRowsMissing_SetsZeroValidState()
        Dim raw(,) As Object = {
            {Nothing, Nothing},
            {DBNull.Value, "   "}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrixWithOptions(
            raw,
            New String() {"A", "B"},
            New DataImportOptions With {.AllowMissing = True})

        Assert.IsTrue(data.bZeroValid)
        Assert.AreEqual(0, data.nRows)
        Assert.IsNull(data.FinalData)
        CollectionAssert.AreEqual(Array.Empty(Of Integer)(), data.RowIds)
    End Sub

    <TestMethod>
    Public Sub DataObj_NumericTextColumn_PreservesTextAndConvertsToNumericView()
        Dim raw(,) As Object = {
            {"1", 10.0R},
            {"2", 20.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw, New String() {"TextNumber", "X"})

        Assert.AreEqual("1", DirectCast(data.FinalData(0, 0), String))
        Assert.AreEqual("2", DirectCast(data.FinalData(1, 0), String))
        Assert.AreEqual(1.0R, data.DataDbl(0, 0), 0.0R)
        Assert.AreEqual(2.0R, data.DataDbl(1, 0), 0.0R)
    End Sub

    <TestMethod>
    Public Sub DataObj_Subset_PreservesRequestedDataAndSourceRowIds()
        Dim raw(,) As Object = {
            {10.0R, 100.0R},
            {20.0R, 200.0R},
            {30.0R, 300.0R},
            {40.0R, 400.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw, New String() {"Y", "X"}, firstSourceRow:=100)

        Dim selected As New Dictionary(Of Integer, Integer) From {
            {3, 103},
            {1, 101}
        }
        data.SubsetByRowIdValues(selected)

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {103, 101}, data.RowIds)
        Assert.AreEqual(40.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(20.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
        Assert.AreEqual(400.0R, Convert.ToDouble(data.FinalData(0, 1)), 0.0R)
        Assert.AreEqual(200.0R, Convert.ToDouble(data.FinalData(1, 1)), 0.0R)
    End Sub


    <TestMethod>
    Public Sub DataObj_DataById_PreservesFirstSeenGroupOrderAndMembership()
        Dim raw(,) As Object = {
            {"B", 2.0R},
            {"A", 1.0R},
            {"B", 4.0R},
            {"A", 3.0R}
        }

        Dim data As New DataObj()
        data.DataImportRawMatrix(raw,
                                 New String() {"Group", "Value"},
                                 CharCols:=0)

        Dim grouped()() As Double = data.DataByID2ByColumn

        Assert.AreEqual(2, grouped.Length)
        CollectionAssert.AreEqual(New Double() {2.0R, 4.0R}, grouped(0))
        CollectionAssert.AreEqual(New Double() {1.0R, 3.0R}, grouped(1))
    End Sub

    <TestMethod>
    Public Sub GlmData_MissingRow_DropsWeightAndOffsetForSameObservation()
        Dim raw(,) As Object = {
            {1.0R, 10.0R, 0.1R, 1.5R},
            {2.0R, Nothing, 0.2R, 2.5R},
            {3.0R, 30.0R, 0.3R, 3.5R}
        }

        Dim data As New glmData With {
            .bOffset = True,
            .bWeights = True
        }
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X", "Offset", "Weight"},
                                 firstSourceRow:=50)

        Assert.AreEqual(2, data.nRows)
        Assert.AreEqual(2, data.nCols)
        CollectionAssert.AreEqual(New Integer() {50, 52}, data.RowIds)
        CollectionAssert.AreEqual(New Double() {0.1R, 0.3R}, data.OffsetData)
        CollectionAssert.AreEqual(New Double() {1.5R, 3.5R}, data.WeightData)
        Assert.AreEqual(1.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(3.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub GlmData_Subset_KeepsWeightsOffsetsAndRowsAligned()
        Dim raw(,) As Object = {
            {1.0R, 10.0R, 0.1R, 1.0R},
            {2.0R, 20.0R, 0.2R, 2.0R},
            {3.0R, 30.0R, 0.3R, 3.0R}
        }

        Dim data As New glmData With {.bOffset = True, .bWeights = True}
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X", "Offset", "Weight"},
                                 firstSourceRow:=60)

        data.SubsetByRowIdValues(New Dictionary(Of Integer, Integer) From {
            {2, 62},
            {0, 60}
        })

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {62, 60}, data.RowIds)
        CollectionAssert.AreEqual(New Double() {0.3R, 0.1R}, data.OffsetData)
        CollectionAssert.AreEqual(New Double() {3.0R, 1.0R}, data.WeightData)
        Assert.AreEqual(3.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(1.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub GeeData_AllAuxiliaryColumns_SortsAndKeepsEveryVectorAligned()
        Dim raw(,) As Object = {
            {100.0R, 10.0R, 2.0R, 2.0R, 0.2R, 2.0R},
            {200.0R, 20.0R, 1.0R, 3.0R, 0.3R, 3.0R},
            {300.0R, 30.0R, 1.0R, 1.0R, 0.1R, 1.0R},
            {400.0R, 40.0R, 2.0R, 1.0R, 0.4R, 4.0R}
        }

        Dim data As New geeData With {
            .bTime = True,
            .bOffset = True,
            .bWeights = True
        }
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X", "Cluster", "Time", "Offset", "Weight"},
                                 firstSourceRow:=70)

        Assert.AreEqual(2, data.nCols)
        CollectionAssert.AreEqual(New Integer() {72, 71, 73, 70}, data.RowIds)
        CollectionAssert.AreEqual(New Object() {1.0R, 1.0R, 2.0R, 2.0R}, data.ClusterIdData)
        CollectionAssert.AreEqual(New Double() {1.0R, 3.0R, 1.0R, 2.0R}, data.TimeData)
        CollectionAssert.AreEqual(New Double() {0.1R, 0.3R, 0.4R, 0.2R}, data.OffsetData)
        CollectionAssert.AreEqual(New Double() {1.0R, 3.0R, 4.0R, 2.0R}, data.WeightData)
        Assert.AreEqual(300.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(200.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
        Assert.AreEqual(400.0R, Convert.ToDouble(data.FinalData(2, 0)), 0.0R)
        Assert.AreEqual(100.0R, Convert.ToDouble(data.FinalData(3, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub GeeData_NoTime_SortsByClusterThenOriginalSourceOrder()
        Dim raw(,) As Object = {
            {10.0R, 2.0R},
            {20.0R, 1.0R},
            {30.0R, 2.0R},
            {40.0R, 1.0R}
        }

        Dim data As New geeData()
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "Cluster"},
                                 firstSourceRow:=80)

        CollectionAssert.AreEqual(New Integer() {81, 83, 80, 82}, data.RowIds)
        CollectionAssert.AreEqual(New Object() {1.0R, 1.0R, 2.0R, 2.0R}, data.ClusterIdData)
        Assert.AreEqual(20.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(40.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
        Assert.AreEqual(10.0R, Convert.ToDouble(data.FinalData(2, 0)), 0.0R)
        Assert.AreEqual(30.0R, Convert.ToDouble(data.FinalData(3, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub GeeData_MissingObservation_IsRemovedBeforeClusterTimeSort()
        Dim raw(,) As Object = {
            {10.0R, 2.0R, 2.0R},
            {20.0R, 1.0R, 3.0R},
            {Nothing, 1.0R, 1.0R},
            {40.0R, 2.0R, 1.0R}
        }

        Dim data As New geeData With {.bTime = True}
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "Cluster", "Time"},
                                 firstSourceRow:=90)

        Assert.AreEqual(3, data.nRows)
        CollectionAssert.AreEqual(New Integer() {91, 93, 90}, data.RowIds)
        CollectionAssert.AreEqual(New Object() {1.0R, 2.0R, 2.0R}, data.ClusterIdData)
        CollectionAssert.AreEqual(New Double() {3.0R, 1.0R, 2.0R}, data.TimeData)
        Assert.AreEqual(20.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(40.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
        Assert.AreEqual(10.0R, Convert.ToDouble(data.FinalData(2, 0)), 0.0R)
    End Sub

    <TestMethod>
    Public Sub GeeData_Subset_KeepsClusterTimeWeightsOffsetAndRowsAligned()
        Dim raw(,) As Object = {
            {100.0R, 10.0R, 2.0R, 2.0R, 0.2R, 2.0R},
            {200.0R, 20.0R, 1.0R, 3.0R, 0.3R, 3.0R},
            {300.0R, 30.0R, 1.0R, 1.0R, 0.1R, 1.0R},
            {400.0R, 40.0R, 2.0R, 1.0R, 0.4R, 4.0R}
        }

        Dim data As New geeData With {
            .bTime = True,
            .bOffset = True,
            .bWeights = True
        }
        data.DataImportRawMatrix(raw,
                                 New String() {"Y", "X", "Cluster", "Time", "Offset", "Weight"},
                                 firstSourceRow:=100)

        ' Sorted positions are source rows 102, 101, 103, 100. Select positions 3 then 0.
        data.SubsetByRowIdValues(New Dictionary(Of Integer, Integer) From {
            {3, 100},
            {0, 102}
        })

        Assert.AreEqual(2, data.nRows)
        CollectionAssert.AreEqual(New Integer() {100, 102}, data.RowIds)
        CollectionAssert.AreEqual(New Object() {2.0R, 1.0R}, data.ClusterIdData)
        CollectionAssert.AreEqual(New Double() {2.0R, 1.0R}, data.TimeData)
        CollectionAssert.AreEqual(New Double() {0.2R, 0.1R}, data.OffsetData)
        CollectionAssert.AreEqual(New Double() {2.0R, 1.0R}, data.WeightData)
        Assert.AreEqual(100.0R, Convert.ToDouble(data.FinalData(0, 0)), 0.0R)
        Assert.AreEqual(300.0R, Convert.ToDouble(data.FinalData(1, 0)), 0.0R)
    End Sub

End Class
