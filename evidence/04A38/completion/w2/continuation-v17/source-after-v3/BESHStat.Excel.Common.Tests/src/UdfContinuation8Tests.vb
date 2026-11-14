Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class UdfContinuation8Tests
    Private Shared Function Number(value As Object) As Double?
        If value Is Nothing Then Return Nothing
        Return CDbl(value)
    End Function

    Private Shared Function OpenUnit(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0 AndAlso value < 1
    End Function

    Private Shared Function ClosedUnit(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value >= 0 AndAlso value <= 1
    End Function

    <TestMethod>
    Public Sub RequiredReadsFinishBeforeCollectiveFailureAndOptionsStayLazy()
        Dim seen As New List(Of Object)()
        Dim read As Func(Of Object, Double?) = Function(value)
                                                  seen.Add(value)
                                                  Return Number(value)
                                              End Function
        Dim options As Func(Of Object, UdfSampleSizeHypothesis38) = Function(value)
                                                                     Assert.Fail("Option read before required guards")
                                                                     Return UdfSampleSizeHypothesis38.Invalid
                                                                 End Function
        Dim errorKind As UdfSampleSizeError38 = UdfSampleSizeError38.Numeric
        Assert.IsNull(UdfSampleSizePreparation38.PrepareUnpaired(Nothing, 2.0R, 1.0R, 0.05R, 0.2R, Nothing, Nothing, read, options, AddressOf OpenUnit, errorKind))
        CollectionAssert.AreEqual(New Object() {Nothing, 2.0R, 1.0R, 0.05R, 0.2R}, seen)
        Assert.AreEqual(UdfSampleSizeError38.Value, errorKind)
        seen.Clear()
        Assert.IsNull(UdfSampleSizePreparation38.PrepareUnpaired(1.0R, 0.0R, 1.0R, 0.05R, 0.2R, Nothing, Nothing, read, options, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(5, seen.Count)
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind)
    End Sub

    <TestMethod>
    Public Sub HypothesisAndMarginFailuresKeepOriginalPrecedenceAndExceptions()
        Dim calls As Integer = 0
        Dim read As Func(Of Object, Double?) = Function(value)
                                                  calls += 1
                                                  Return Number(value)
                                              End Function
        Dim errorKind As UdfSampleSizeError38
        Assert.IsNull(UdfSampleSizePreparation38.PrepareUnpaired(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, Nothing, 0.0R, read, Function(value) UdfSampleSizeHypothesis38.Invalid, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(5, calls)
        Assert.AreEqual(UdfSampleSizeError38.Value, errorKind)
        calls = 0
        Assert.IsNull(UdfSampleSizePreparation38.PrepareUnpaired(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, Nothing, 0.0R, read, Function(value) UdfSampleSizeHypothesis38.NonInferiority, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(6, calls)
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind)
        Dim failure As New InvalidOperationException("host text conversion")
        Dim observed = Assert.ThrowsException(Of InvalidOperationException)(Sub()
                                                                              UdfSampleSizePreparation38.PrepareUnpaired(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, Nothing, Nothing, read, Function(value)
                                                                                                                                                                                         Throw failure
                                                                                                                                                                                         Return UdfSampleSizeHypothesis38.Invalid
                                                                                                                                                                                     End Function, AddressOf OpenUnit, errorKind)
                                                                          End Sub)
        Assert.AreSame(failure, observed)
    End Sub

    <DataTestMethod>
    <DataRow(1, 3, 2)>
    <DataRow(2, 3, 2)>
    <DataRow(3, 4, 4)>
    Public Sub UnpairedPlanningUsesExistingTypedReports(kind As Integer, rows As Integer, columns As Integer)
        Dim errorKind As UdfSampleSizeError38
        Dim result = UdfSampleSizePreparation38.PrepareUnpaired(0.2R, 1.0R, 1.0R, 0.05R, 0.2R, Nothing, 1.0R, AddressOf Number, Function(value) CType(kind, UdfSampleSizeHypothesis38), AddressOf OpenUnit, errorKind)
        Dim table = result.ToOutputModel().Values
        Assert.AreEqual(rows, table.GetLength(0))
        Assert.AreEqual(columns, table.GetLength(1))
        Assert.IsInstanceOfType(table(1, 1), GetType(Integer))
        Assert.AreEqual(UdfReportPreparation38.Raw, result.Preparation)
    End Sub

    <DataTestMethod>
    <DataRow(1, 3, 3)>
    <DataRow(2, 3, 3)>
    <DataRow(3, 7, 5)>
    Public Sub IndependentProportionComponentsKeepTheirRowsAndTypes(kind As Integer, rows As Integer, columns As Integer)
        Dim errorKind As UdfSampleSizeError38
        Dim result = UdfSampleSizePreparation38.PrepareIndependentProportions(0.4R, 0.45R, 1.0R, 0.05R, 0.2R, Nothing, 0.2R, AddressOf Number, Function(value) CType(kind, UdfSampleSizeHypothesis38), AddressOf OpenUnit, AddressOf ClosedUnit, errorKind)
        Dim table = result.ToOutputModel().Values
        Assert.AreEqual(rows, table.GetLength(0))
        Assert.AreEqual(columns, table.GetLength(1))
        Assert.IsInstanceOfType(table(1, 1 + If(kind = 3, 1, 0)), GetType(Integer))
    End Sub

    <TestMethod>
    Public Sub CoxOptionReadsStopAtFirstInvalidOptionalAndMissingDefaultsStayLazy()
        Dim reads As Integer = 0, missingReads As Integer = 0, boolReads As Integer = 0
        Dim read As Func(Of Object, Double?) = Function(value)
                                                  reads += 1
                                                  Return Number(value)
                                              End Function
        Dim missing As Func(Of Object, Boolean) = Function(value)
                                                     missingReads += 1
                                                     Return value Is Nothing
                                                 End Function
        Dim logical As Func(Of Object, Boolean, Boolean) = Function(value, defaultValue)
                                                              boolReads += 1
                                                              Assert.IsTrue(defaultValue)
                                                              Return False
                                                          End Function
        Dim errorKind As UdfSampleSizeError38
        Assert.IsNull(UdfSampleSizePreparation38.PrepareCoxBinary(0.7R, 1.0R, 0.05R, 0.2R, 1.0R, New Object(), New Object(), read, missing, logical, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(5, reads) : Assert.AreEqual(1, missingReads) : Assert.AreEqual(0, boolReads)
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind)
        Dim result = UdfSampleSizePreparation38.PrepareCoxContinuous(0.8R, 1.0R, 0.05R, 0.2R, Nothing, Nothing, Nothing, read, missing, logical, AddressOf OpenUnit, errorKind)
        Dim table = result.ToOutputModel().Values
        Assert.AreEqual(8, table.GetLength(0))
        Assert.AreEqual("Continuous covariate", table(1, 1))
        Assert.AreEqual("Not supplied", table(4, 1))
        Assert.AreEqual(1, boolReads)
    End Sub

    <TestMethod>
    Public Sub OtherPlanningFamiliesKeepOriginalReportShapesAndScalarKinds()
        Dim errorKind As UdfSampleSizeError38
        Dim singleReport = UdfSampleSizePreparation38.PrepareSingleProportion(0.6R, 0.5R, 0.05R, 0.2R, AddressOf Number, AddressOf OpenUnit, AddressOf ClosedUnit, errorKind)
        Assert.AreEqual(2, singleReport.ToOutputModel().RowCount)
        Assert.IsInstanceOfType(singleReport.ToOutputModel().Values(1, 1), GetType(Integer))
        Dim logRank = UdfSampleSizePreparation38.PrepareLogRank(0.7R, 0.3R, 0.2R, 1.0R, 0.05R, 0.2R, Nothing, AddressOf Number, Function(value, fallback) fallback, AddressOf OpenUnit, errorKind)
        Assert.AreEqual(8, logRank.ToOutputModel().RowCount)
        Dim icc = UdfSampleSizePreparation38.PrepareIcc(0.5R, 0.75R, 3.0R, 0.05R, 0.2R, AddressOf Number, AddressOf OpenUnit, errorKind)
        Assert.AreEqual(6, icc.ToOutputModel().RowCount)
        Dim bland = UdfSampleSizePreparation38.PrepareBlandAltman(2.0R, 1.0R, 0.05R, Nothing, AddressOf Number, Function(value) value Is Nothing, AddressOf OpenUnit, errorKind)
        Assert.AreEqual(7, bland.ToOutputModel().RowCount)
        Assert.AreEqual(1.96R, bland.ToOutputModel().Values(6, 1))
        Assert.IsNull(UdfSampleSizePreparation38.PrepareIcc(0.5R, 0.75R, 2.5R, 0.05R, 0.2R, AddressOf Number, AddressOf OpenUnit, errorKind))
        Assert.AreEqual(UdfSampleSizeError38.Numeric, errorKind)
    End Sub

    <TestMethod>
    Public Sub PreparedLayoutsPreserveOpaqueCellsNamesAndPartialWrites()
        Dim marker As New Object()
        Dim raw(,) As Object = Nothing, names() As String = Nothing
        UdfModelImportPreparation38.InitializeResponsePredictorsPrepared(2, 3, 1, " ", {"duplicate"}, New Object(,) {{marker}, {2.0R}}, New Object(,) {{3.0R}, {4.0R}}, raw, names)
        Assert.AreEqual("Y", names(0)) : Assert.AreEqual("duplicate", names(1))
        Assert.AreSame(marker, raw(0, 0)) : Assert.AreEqual(4.0R, raw(1, 1))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfModelImportPreparation38.CopyNamedColumnPrepared(raw, names, 2, 2, " ", "Offset", New Object(,) {{marker}}))
        Assert.AreEqual("Offset", names(2)) : Assert.AreSame(marker, raw(0, 2)) : Assert.IsNull(raw(1, 2))
        Dim bounded = DirectCast(Array.CreateInstance(GetType(Object), {2, 1}, {4, 7}), Object(,))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfModelImportPreparation38.InitializePredictorsPrepared(2, 1, 1, {"X"}, bounded, raw, names))
        Assert.AreEqual(2, raw.GetLength(0)) : Assert.AreEqual("X", names(0))
        UdfModelImportPreparation38.InitializePredictorsPrepared(0, 1, 1, {"X"}, New Object(-1, 0) {}, raw, names)
        Assert.AreEqual(0, raw.GetLength(0)) : Assert.AreEqual("X", names(0))
    End Sub

    <TestMethod>
    Public Sub CoxPreparedLayoutKeepsTimeStatusStrataPredictorOrdering()
        Dim raw(,) As Object = Nothing, names() As String = Nothing
        UdfModelImportPreparation38.InitializeCoxPrepared(2, 4, 1, Nothing, " ", Nothing, {"duplicate"}, New Object(,) {{1.0R}, {2.0R}}, New Object(,) {{False}, {True}}, New Object(,) {{"A"}, {"B"}}, New Object(,) {{"opaque"}, {4.0R}}, True, raw, names)
        CollectionAssert.AreEqual(New String() {"Time", "Status", "Strata", "duplicate"}, names)
        Assert.AreEqual(1.0R, raw(0, 0)) : Assert.AreEqual(False, raw(0, 1))
        Assert.AreEqual("B", raw(1, 2)) : Assert.AreEqual("opaque", raw(0, 3))
    End Sub
End Class
