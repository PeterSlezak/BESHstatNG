Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AssumptionReport06AdapterTests
    Private Shared Function Data(Optional n As Integer = 30) As MultiGroupsUnpairedData
        Return New MultiGroupsUnpairedData With {.varNames = {"B'eta", "0"}, .X = {
            Enumerable.Range(1, n).Select(Function(i) Math.Sin(i * 1.7) + i * 0.11 + (i Mod 5) * 0.2).Reverse().ToArray(),
            Enumerable.Range(1, n + 2).Select(Function(i) Math.Cos(i * 1.9) + i * 0.08 + (i Mod 3) * 0.3).Reverse().ToArray()}}
    End Function
    Private Shared Function Copy(data As MultiGroupsUnpairedData) As MultiGroupsUnpairedData
        Return New MultiGroupsUnpairedData With {.X = data.X.Select(Function(x) DirectCast(x.Clone(), Double())).ToArray(), .varNames = DirectCast(data.varNames.Clone(), String())}
    End Function
    Public Shared Sub Same(expected As ResultTable, actual As ResultTable)
        Dim a = expected.ToOutputModel(), b = actual.ToOutputModel()
        Assert.AreEqual(a.RowCount, b.RowCount)
        Assert.AreEqual(a.ColumnCount, b.ColumnCount)
        Assert.AreEqual(a.HeaderTopRows, b.HeaderTopRows)
        Assert.AreEqual(a.HeaderLeftColumns, b.HeaderLeftColumns)
        Assert.AreEqual(a.TitleRows, b.TitleRows)
        For row = 0 To a.RowCount - 1
            For col = 0 To a.ColumnCount - 1
                Assert.AreEqual(a.Values(row, col), b.Values(row, col), "cell " & row.ToString() & "," & col.ToString())
                If a.Values(row, col) IsNot Nothing Then Assert.AreEqual(a.Values(row, col).GetType(), b.Values(row, col).GetType())
            Next
        Next
        CollectionAssert.AreEqual(a.PvalueCells.Select(Function(c) c.BodyRow).ToArray(), b.PvalueCells.Select(Function(c) c.BodyRow).ToArray())
        CollectionAssert.AreEqual(a.PvalueCells.Select(Function(c) c.BodyColumn).ToArray(), b.PvalueCells.Select(Function(c) c.BodyColumn).ToArray())
        Assert.AreEqual(expected.bLeftHeaderAdjustUp, actual.bLeftHeaderAdjustUp)
    End Sub
    Private Shared Sub SameMutation(before As MultiGroupsUnpairedData, after As MultiGroupsUnpairedData)
        For g = 0 To before.X.Length - 1
            CollectionAssert.AreEqual(before.X(g), after.X(g))
        Next
    End Sub

    <DataTestMethod>
    <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(8)> <DataRow(9)>
    <DataRow(4999)> <DataRow(5000)> <DataRow(5001)>
    Public Sub ActualAdapterMatchesPreExtractionNormality(n As Integer)
        Dim before = Data(n), after = Copy(before)
        If n >= 4999 Then
            Assert.ThrowsException(Of OverflowException)(Sub() LegacyAssumption06Characterization.RunNormality(before))
            Assert.ThrowsException(Of OverflowException)(Sub() WindowsAssumptionReportAdapter.Prepare(after, New AssumptionReportOptions(AssumptionReportKind.Normality)))
            SameMutation(before, after)
            Return
        End If
        Dim expected = LegacyAssumption06Characterization.RunNormality(before)
        Dim result = WindowsAssumptionReportAdapter.Prepare(after, New AssumptionReportOptions(AssumptionReportKind.Normality))
        Same(expected, result.Tables(0))
        SameMutation(before, after)
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)>
    <DataRow(8)> <DataRow(9)> <DataRow(10)> <DataRow(11)> <DataRow(12)> <DataRow(13)> <DataRow(14)> <DataRow(15)>
    Public Sub EveryHomogeneityCombinationMatchesFrozenHelper(bits As Integer)
        Dim before = Data(), after = Copy(before)
        Dim expected = LegacyAssumption06Characterization.ComputeVarianceHomogeneity(before, (bits And 1) <> 0, (bits And 2) <> 0, (bits And 4) <> 0, (bits And 8) <> 0)
        Same(expected, WindowsAssumptionReportAdapter.Homogeneity(after, CType(bits, HomogeneityMethods)))
        SameMutation(before, after)
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub FullAndSelectedDescriptiveSnapshotsForAllExistingCallers(selected As Boolean)
        Dim before = Data(), after = Copy(before)
        Dim keys = If(selected, New List(Of String) From {"n", "mean", "median", "cv", "variance", "sd", "sem", "skewness", "kurtosis", "q1", "q3", "iqr", "minimum", "maximum", "range", "swstat", "swpvalue"}, Nothing)
        Same(LegacyAssumption06Characterization.ComputeDescriptiveStats(before, keys), WindowsAssumptionReportAdapter.Descriptive(after, keys))
        SameMutation(before, after)
        'This exact compatibility facade is also called by unchanged ANOVA/KW/ROC/t-test paths.
    End Sub

    <DataTestMethod> <DataRow(True)> <DataRow(False)>
    Public Sub SymmetryBothMethodsMatchFrozenProjection(mgg As Boolean)
        Dim before = Data(), after = Copy(before)
        Same(LegacyAssumption06Characterization.RunSymmetry(before, mgg),
            WindowsAssumptionReportAdapter.Prepare(after, New AssumptionReportOptions(AssumptionReportKind.Symmetry,
                symmetry:=If(mgg, AssumptionSymmetryMethod.MiaoGelGastwirth, AssumptionSymmetryMethod.CabilioMasaro))).Tables(0))
        SameMutation(before, after)
    End Sub

    <DataTestMethod> <DataRow(AssumptionOutlierMethod.Grubbs, 30)> <DataRow(AssumptionOutlierMethod.Rosner, 14)>
    <DataRow(AssumptionOutlierMethod.Rosner, 20)> <DataRow(AssumptionOutlierMethod.Rosner, 30)>
    <DataRow(AssumptionOutlierMethod.None, 30)>
    Public Sub OutlierStatisticsPaddingAndValuesAreSourceCompatible(method As AssumptionOutlierMethod, n As Integer)
        Dim before = Data(n)
        before.X(0)(0) = 1000
        before.X(1)(0) = 2000
        before.X(1)(1) = -2000
        Dim after = Copy(before)
        Same(LegacyAssumption06Characterization.RunOutliers(before, method = AssumptionOutlierMethod.Grubbs, method = AssumptionOutlierMethod.Rosner, 0.1D),
            WindowsAssumptionReportAdapter.Prepare(after, New AssumptionReportOptions(AssumptionReportKind.Outliers, outlierMethod:=method, alpha:=0.1D)).Tables(0))
        SameMutation(before, after)
    End Sub

    <DataTestMethod> <DataRow(False, False, "")> <DataRow(True, False, "")> <DataRow(False, True, "")> <DataRow(True, True, "")>
    <DataRow(False, False, "sk-SK")> <DataRow(True, False, "sk-SK")> <DataRow(False, True, "sk-SK")> <DataRow(True, True, "sk-SK")>
    Public Sub RealBoxCalculationAndSupplementOrderAreUnchanged(desc As Boolean, box As Boolean, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim before = Data(), after = Copy(before)
            Dim expected As New List(Of ResultTable) From {LegacyAssumption06Characterization.RunNormality(before)}
            If desc Then expected.Add(LegacyAssumption06Characterization.ComputeDescriptiveStats(before))
            If box Then
                'Invoke the real constructor with no worksheet. Reflection avoids adding an
                'Excel Interop compile dependency to this host-free test assembly.
                Dim legacyBox = DirectCast(Activator.CreateInstance(GetType(graphics.BoxPlot), New Object() {before.X, before.varNames, Nothing, ""}), graphics.BoxPlot)
                legacyBox.Calculate()
                legacyBox.CalcForPlotting()
                expected.Add(legacyBox.wrapResults())
            End If
            Dim actual = WindowsAssumptionReportAdapter.Prepare(after, New AssumptionReportOptions(AssumptionReportKind.Normality, descriptive:=desc, boxSummary:=box))
            Assert.AreEqual(expected.Count, actual.Tables.Count)
            For i = 0 To expected.Count - 1
                Same(expected(i), actual.Tables(i))
            Next
            Assert.AreEqual(box, actual.Box IsNot Nothing)
            SameMutation(before, after)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub LegacyCoreFailureAndCancellationAbortBeforeCallerCanWrite()
        Dim before = New MultiGroupsUnpairedData With {.X = {New Double() {2, 2, 2, 2}}, .varNames = {"constant"}}
        Assert.ThrowsException(Of NullReferenceException)(Sub() LegacyAssumption06Characterization.RunNormality(Copy(before)))
        Assert.ThrowsException(Of NullReferenceException)(Sub() WindowsAssumptionReportAdapter.Prepare(Copy(before), New AssumptionReportOptions(AssumptionReportKind.Normality)))
        Dim writes = 0
        Try
            Dim prepared = WindowsAssumptionReportAdapter.Prepare(Data(), New AssumptionReportOptions(AssumptionReportKind.Normality), New CancellationToken(True))
            writes += prepared.Tables.Count
        Catch ex As OperationCanceledException
        End Try
        Assert.AreEqual(0, writes)
    End Sub

    <TestMethod>
    Public Sub ReversedColumnsAndSelectedInputProducerReachActualReportAdapter()
        Dim first As New DataObj(), second As New DataObj()
        first.DataImportRawMatrix(New Object(,) {{"second"}, {12.0}, {Nothing}, {5.0}, {3.0}, {9.0}}, {"second"}, firstSourceRow:=101)
        second.DataImportRawMatrix(New Object(,) {{"first"}, {3.0}, {4.0}, {5.0}, {6.0}, {9.0}}, {"first"}, firstSourceRow:=101)
        Dim errorText As String = Nothing
        Dim groups = WindowsUnivariateInputAdapter.SelectColumns({first, second}, errorText)
        Assert.IsNotNull(groups)
        Dim before = Copy(groups)
        Same(LegacyAssumption06Characterization.RunNormality(before), WindowsAssumptionReportAdapter.Prepare(groups, New AssumptionReportOptions(AssumptionReportKind.Normality)).Tables(0))
        'The legacy grouped object has no row map. No invented provenance is asserted here.
    End Sub
End Class
