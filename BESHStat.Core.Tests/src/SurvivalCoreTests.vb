Option Explicit On
Option Strict On

Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.survival

<TestClass>
Public Class SurvivalCoreTests

    <TestMethod>
    Public Sub CreateSurvivalData_MapsGroupsAndStrataByFirstSeenOrder()
        Dim err As String = String.Empty
        Dim records = Survival.CreatSurvivalData(
            New Double() {5.0R, 2.0R, 7.0R, 4.0R},
            New Integer() {1, 0, 1, 1},
            New String() {"B", "A", "B", "C"},
            New String() {"S2", "S1", "S2", "S3"},
            err)

        Assert.IsNotNull(records)
        Assert.AreEqual(String.Empty, err)
        Assert.AreEqual(4, records.Count)
        CollectionAssert.AreEqual(New Integer() {0, 1, 0, 2}, records.ConvertAll(Function(r) r.Group).ToArray())
        CollectionAssert.AreEqual(New String() {"0", "1", "0", "2"}, records.ConvertAll(Function(r) r.Stratum).ToArray())
        Assert.AreEqual("B", records(0).strGroup)
        Assert.AreEqual("S2", records(0).strStratum)
    End Sub

    <TestMethod>
    Public Sub CreateSurvivalData_DimensionMismatch_ReturnsNothingAndError()
        Dim err As String = String.Empty
        Dim records = Survival.CreatSurvivalData(
            New Double() {1.0R, 2.0R},
            New Integer() {1},
            New String() {"A", "A"},
            New String() {"0", "0"},
            err)

        Assert.IsNull(records)
        Assert.AreEqual("Invalid input dimensions", err)
    End Sub

    <TestMethod>
    Public Sub CreateSurvivalData_NegativeTime_ReturnsNothingAndError()
        Dim err As String = String.Empty
        Dim records = Survival.CreatSurvivalData(
            New Double() {1.0R, -0.1R},
            New Integer() {1, 0},
            New String() {"A", "A"},
            New String() {"0", "0"},
            err)

        Assert.IsNull(records)
        StringAssert.Contains(err, "Unexpected time value")
    End Sub

    <TestMethod>
    Public Sub CreateSurvivalData_InvalidCensoring_ReturnsNothingAndError()
        Dim err As String = String.Empty
        Dim records = Survival.CreatSurvivalData(
            New Double() {1.0R, 2.0R},
            New Integer() {1, 2},
            New String() {"A", "A"},
            New String() {"0", "0"},
            err)

        Assert.IsNull(records)
        StringAssert.Contains(err, "Unexpected censoring indictor")
    End Sub

    <TestMethod>
    Public Sub CoxPHData_WithoutStrata_AlignsRowsCovariatesAndSurvivalRecords()
        Dim raw(,) As Object = {
            {5.0R, 1.0R, 10.0R, 100.0R},
            {6.0R, 0.0R, Nothing, 200.0R},
            {7.0R, 0.0R, 30.0R, 300.0R}
        }

        Dim data As New CoxPHData()
        data.DataImportRawMatrix(raw,
                                 New String() {"time", "status", "x1", "x2"},
                                 firstSourceRow:=10)

        Assert.AreEqual(2, data.nRows)
        Assert.AreEqual(2, data.nCols)
        CollectionAssert.AreEqual(New Integer() {10, 12}, data.RowIds)
        CollectionAssert.AreEqual(New String() {"x1", "x2"}, data.varNames)
        CollectionAssert.AreEqual(New Double() {5.0R, 7.0R}, data.TimeData)
        CollectionAssert.AreEqual(New Integer() {1, 0}, data.CensorData)
        Assert.AreEqual(2, data.SurvRecordsList.Count)
        Assert.AreEqual(10, data.SurvRecordsList(0).Index)
        Assert.AreEqual("0", data.SurvRecordsList(0).Stratum)
        CollectionAssert.AreEqual(New Double() {10.0R, 100.0R}, data.SurvRecordsList(0).Covariates)
        Assert.AreEqual(12, data.SurvRecordsList(1).Index)
        CollectionAssert.AreEqual(New Double() {30.0R, 300.0R}, data.SurvRecordsList(1).Covariates)
    End Sub

    <TestMethod>
    Public Sub CoxPHData_WithStrata_SeparatesStratumAndPreservesSourceRows()
        Dim raw(,) As Object = {
            {5.0R, 1.0R, "A", 10.0R},
            {6.0R, 0.0R, "B", Nothing},
            {7.0R, 0.0R, "B", 30.0R}
        }

        Dim data As New CoxPHData With {.bStrata = True}
        data.DataImportRawMatrix(raw,
                                 New String() {"time", "status", "strata", "x"},
                                 firstSourceRow:=20)

        Assert.AreEqual(2, data.nRows)
        Assert.AreEqual(1, data.nCols)
        CollectionAssert.AreEqual(New Integer() {20, 22}, data.RowIds)
        CollectionAssert.AreEqual(New String() {"x"}, data.varNames)
        CollectionAssert.AreEqual(New String() {"A", "B"}, data.StrataData)
        Assert.AreEqual("strata", data.StrataVarName)
        Assert.AreEqual("A", data.SurvRecordsList(0).Stratum)
        Assert.AreEqual("B", data.SurvRecordsList(1).Stratum)
        Assert.AreEqual(20, data.SurvRecordsList(0).Index)
        Assert.AreEqual(22, data.SurvRecordsList(1).Index)
        CollectionAssert.AreEqual(New Double() {10.0R}, data.SurvRecordsList(0).Covariates)
        CollectionAssert.AreEqual(New Double() {30.0R}, data.SurvRecordsList(1).Covariates)
    End Sub

    <TestMethod>
    Public Sub CoxPHData_InvalidCensoring_Throws()
        Dim raw(,) As Object = {
            {5.0R, 1.0R, 10.0R},
            {6.0R, 2.0R, 20.0R}
        }

        Dim data As New CoxPHData()
        Dim ex = Assert.ThrowsException(Of ArgumentException)(
            Sub() data.DataImportRawMatrix(raw, New String() {"time", "status", "x"}))

        StringAssert.Contains(ex.Message, "not 1/0")
    End Sub

    <TestMethod>
    Public Sub CoxPHData_AllRowsInvalid_LeavesZeroValidStateAndNoRecords()
        Dim raw(,) As Object = {
            {Nothing, 1.0R, 10.0R},
            {6.0R, Nothing, 20.0R}
        }

        Dim data As New CoxPHData()
        data.DataImportRawMatrix(raw, New String() {"time", "status", "x"}, firstSourceRow:=30)

        Assert.IsTrue(data.bZeroValid)
        Assert.AreEqual(0, data.nRows)
        Assert.AreEqual(0, data.RowIds.Length)
        Assert.AreEqual(0, data.SurvRecordsList.Count)
        Assert.IsNull(data.TimeData)
        Assert.IsNull(data.CensorData)
    End Sub

End Class
