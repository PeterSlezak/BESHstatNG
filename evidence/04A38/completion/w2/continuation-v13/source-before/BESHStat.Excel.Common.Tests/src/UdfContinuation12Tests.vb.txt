Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation12Tests
    Private Shared Function Mca() As Multivariate.CA
        Dim model As New Multivariate.CA()
        model.DataMultiple(New String(,) {{"a", "x"}, {"a", "y"}, {"b", "x"}, {"b", "y"}}, New String() {"First", "Second"})
        model.Calculate()
        Return model
    End Function

    <DataTestMethod, DataRow(True, ""), DataRow(False, ""), DataRow(True, "sk-SK"), DataRow(False, "sk-SK")>
    Public Sub McaSummaryKeepsTypedCountsHeadersAndAxes(header As Boolean, culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim model = Mca(), eigen = model.Eigenvalues
            Dim result = UdfMultivariateReportPreparation38.BuildMcaSummary(model, 4, New String() {"First", "Second"}, eigen, header)
            Dim start = If(header, 1, 0)
            Assert.AreEqual(6 + start, result.GetLength(0))
            Assert.AreEqual(2, result.GetLength(1))
            Assert.AreEqual("Multiple correspondence analysis", result(start, 1))
            Assert.AreEqual(4, result(start + 1, 1))
            Assert.AreEqual(2, result(start + 2, 1))
            Assert.AreEqual(4, result(start + 3, 1))
            Assert.AreEqual(eigen.Length, result(start + 4, 1))
            If header Then Assert.AreEqual("Setting", result(0, 0))
            Assert.AreSame(eigen, model.Eigenvalues)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(True), DataRow(False)>
    Public Sub McaSummaryRetainsMissingEigenvalueCells(header As Boolean)
        Dim result = UdfMultivariateReportPreparation38.BuildMcaSummary(Mca(), 4, New String() {"A", "B"}, Nothing, header)
        Dim start = If(header, 1, 0)
        Assert.AreEqual(0, result(start + 4, 1))
        Assert.IsNull(result(start + 5, 1))
    End Sub

    <TestMethod>
    Public Sub McaSummaryDoesNotReplaceNullVariableNamesWithFallbacks()
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildMcaSummary(Mca(), 4, Nothing, Nothing, False))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub DiscriminantSummaryLeavesLateValidationDetailForHost(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim result = UdfMultivariateReportPreparation38.BuildDiscriminantSummary(New Multivariate.DiscriminantAnalysis(),
                "Linear", "Group", New String() {"X", "X"}, "None", "Error", "Equal", "unparsed host mode", 0.125R)
            Assert.AreEqual(15, result.GetLength(0))
            Assert.AreEqual(2, result.GetLength(1))
            Assert.AreEqual(2, result(3, 1))
            Assert.AreEqual(0, result(4, 1))
            Assert.AreEqual(0, result(5, 1))
            Assert.AreEqual(0, result(6, 1))
            Assert.IsNull(result(11, 1))
            Assert.IsNull(result(12, 1))
            Assert.AreEqual(0.125R, result(13, 1))
            Assert.AreEqual("Validation detail", result(14, 0))
            Assert.IsNull(result(14, 1))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub DiscriminantSummaryRetainsMissingModelFailure()
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildDiscriminantSummary(
            Nothing, "Linear", "Group", New String() {"X"}, "None", "Error", "Equal", "bad", 0.0R))
    End Sub

    <TestMethod>
    Public Sub CategoricalCallbacksKeepHeaderResolveAndRowMajorOrder()
        Dim calls As New List(Of String), namesArg As Object = "explicit"
        Dim data(,) As String = Nothing, names() As String = Nothing
        Dim source As Object(,) = {{" Head ", Nothing}, {" a ", 7}, {" b ", 8}}
        Assert.IsTrue(UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(source, namesArg, 2, True, 1, 2,
            Function(value)
                calls.Add("blank:" & Convert.ToString(value, CultureInfo.InvariantCulture))
                Return value Is Nothing
            End Function,
            Function(value)
                calls.Add("text:" & Convert.ToString(value, CultureInfo.InvariantCulture))
                Return Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
            End Function,
            Function(value, count, prefix, inferred)
                calls.Add("resolve")
                Assert.AreSame(namesArg, value)
                CollectionAssert.AreEqual(New String() {"Head", "Variable 2"}, inferred)
                Return New String() {"duplicate", "duplicate"}
            End Function, data, names))
        CollectionAssert.AreEqual(New String() {"blank: Head ", "text: Head ", "blank:", "resolve", "text: a ", "text:7", "text: b ", "text:8"}, calls)
        Assert.AreEqual("a", data(0, 0))
        Assert.AreEqual("8", data(1, 1))
        CollectionAssert.AreEqual(New String() {"duplicate", "duplicate"}, names)
    End Sub

    <TestMethod>
    Public Sub CategoricalNoHeaderDoesNotInvokeBlankPolicy()
        Dim arg As Object = Nothing, data(,) As String = Nothing, names() As String = Nothing
        Assert.IsTrue(UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(New Object(,) {{"x"}}, arg, 1, False, 0, 1,
            Nothing, Function(value) CStr(value), Function(value, count, prefix, inferred) inferred, data, names))
        Assert.AreEqual("Variable 1", names(0))
        Assert.AreEqual("x", data(0, 0))
    End Sub

    <TestMethod>
    Public Sub CategoricalResolveFailurePrecedesOutputAllocation()
        Dim arg As Object = Nothing, data As String(,) = {{"old"}}, names() As String = {"old"}
        Dim oldData = data, oldNames = names, marker As New InvalidOperationException("resolve boundary")
        Dim caught = Assert.ThrowsException(Of InvalidOperationException)(
            Sub() UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(New Object(,) {{"x"}}, arg, 1, False, 0, 1,
                Nothing, Nothing, Function(value, count, prefix, inferred) ThrowResolve(marker), data, names))
        Assert.AreSame(marker, caught)
        Assert.AreSame(oldData, data)
        Assert.AreSame(oldNames, names)
    End Sub

    Private Shared Function ThrowResolve(marker As Exception) As String()
        Throw marker
    End Function

    <TestMethod>
    Public Sub CategoricalCellFailureRetainsResolvedNamesAndPartialArray()
        Dim arg As Object = Nothing, data(,) As String = Nothing, names() As String = Nothing
        Dim marker As New InvalidOperationException("cell boundary"), reads As Integer = 0
        Dim caught = Assert.ThrowsException(Of InvalidOperationException)(
            Sub() UdfModelImportPreparation38.CompleteCategoricalMatrixPrepared(New Object(,) {{"a", "b"}}, arg, 2, False, 0, 1,
                Nothing, Function(value)
                             reads += 1
                             If reads = 2 Then Throw marker
                             Return CStr(value)
                         End Function,
                Function(value, count, prefix, inferred) New String() {"A", "B"}, data, names))
        Assert.AreSame(marker, caught)
        Assert.AreEqual("a", data(0, 0))
        Assert.IsNull(data(0, 1))
        CollectionAssert.AreEqual(New String() {"A", "B"}, names)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub StringVectorUsesSuppliedCulturePolicyInCellOrder(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim output() As String = Nothing, seen As New List(Of Object)
            Assert.IsTrue(UdfModelImportPreparation38.TryGetStringVectorPrepared(New Object(,) {{1.5R, Nothing, " x "}},
                Function(value)
                    seen.Add(value)
                    Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
                End Function, output))
            CollectionAssert.AreEqual(New String() {1.5R.ToString(CultureInfo.CurrentCulture), "", "x"}, output)
            Assert.AreEqual(3, seen.Count)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub VectorRectangleRejectsWithoutCallingPolicyOrReplacingOutput()
        Dim strings() As String = {"old"}, numbers() As Double = {17}
        Dim oldStrings = strings, oldNumbers = numbers
        Assert.IsFalse(UdfModelImportPreparation38.TryGetStringVectorPrepared(New Object(1, 1) {}, Nothing, strings))
        Assert.IsFalse(UdfModelImportPreparation38.TryGetDoubleVectorPrepared(New Object(1, 1) {}, Nothing, numbers))
        Assert.AreSame(oldStrings, strings)
        Assert.AreSame(oldNumbers, numbers)
    End Sub

    <TestMethod>
    Public Sub NumericVectorMissingPolicyResultStopsBeforeCommitAndLaterCells()
        Dim output() As Double = {17}, previous = output, seen As New List(Of Integer)
        Assert.IsFalse(UdfModelImportPreparation38.TryGetDoubleVectorPrepared(New Object(,) {{1}, {2}, {3}},
            Function(value)
                seen.Add(CInt(value))
                Return If(CInt(value) = 2, CType(Nothing, Double?), CType(CDbl(value), Double?))
            End Function, output))
        CollectionAssert.AreEqual(New Integer() {1, 2}, seen)
        Assert.AreSame(previous, output)
    End Sub

    <TestMethod>
    Public Sub NumericVectorPreservesValuesChosenByHostPolicy()
        Dim output() As Double = Nothing
        Assert.IsTrue(UdfModelImportPreparation38.TryGetDoubleVectorPrepared(New Object(,) {{1, 2}}, Function(value) CType(CDbl(value), Double?), output))
        CollectionAssert.AreEqual(New Double() {1, 2}, output)
        Assert.IsTrue(UdfModelImportPreparation38.TryGetDoubleVectorPrepared(New Object(,) {{"host decision"}}, Function(value) CType(Double.NaN, Double?), output))
        Assert.IsTrue(Double.IsNaN(output(0)))
    End Sub

    <TestMethod>
    Public Sub EmptySingleRowCommitsEmptyOutputAndReturnsFalse()
        Dim strings() As String = Nothing, numbers() As Double = Nothing
        Assert.IsFalse(UdfModelImportPreparation38.TryGetStringVectorPrepared(New Object(0, -1) {}, Nothing, strings))
        Assert.IsFalse(UdfModelImportPreparation38.TryGetDoubleVectorPrepared(New Object(0, -1) {}, Nothing, numbers))
        Assert.AreEqual(0, strings.Length)
        Assert.AreEqual(0, numbers.Length)
    End Sub

    <TestMethod>
    Public Sub PreparedVectorRetainsZeroBasedIndexFailure()
        Dim source = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {1, 2}, New Integer() {4, 6}), Object(,))
        Dim output() As String = {"old"}, previous = output
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfModelImportPreparation38.TryGetStringVectorPrepared(source, Function(value) "x", output))
        Assert.AreSame(previous, output)
    End Sub
End Class
