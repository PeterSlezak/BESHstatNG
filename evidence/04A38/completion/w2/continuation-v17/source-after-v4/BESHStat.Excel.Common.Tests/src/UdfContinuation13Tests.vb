Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation13Tests
    Private Shared Function Finite(cell As Object, ByRef value As Double) As Boolean
        If cell Is Nothing OrElse Not (TypeOf cell Is Double OrElse TypeOf cell Is Integer) Then Return False
        value = Convert.ToDouble(cell, CultureInfo.InvariantCulture)
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function Status(cell As Object, ByRef value As Integer) As Boolean
        If Not TypeOf cell Is Integer Then Return False
        value = DirectCast(cell, Integer)
        Return value = 0 OrElse value = 1
    End Function

    Private Shared Function Text(cell As Object) As String
        Return Convert.ToString(cell, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function Normalize(value As String) As String
        Return value.Trim().ToUpperInvariant()
    End Function

    <DataTestMethod, DataRow("", False), DataRow("sk-SK", False), DataRow("", True), DataRow("sk-SK", True)>
    Public Sub RocPreparedKeepsPartitionOrderDirectionAndTextShortCircuit(culture As String, lower As Boolean)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim values(,) As Object = {{2}, {5}, {3}, {1}}, labels(,) As Object = {{" yes "}, {"no"}, {"YES"}, {"no"}}
            Dim statusReads As Integer, output()() As Double = Nothing
            Dim statusPolicy As UdfPlotStatusReader38 = Function(cell As Object, ByRef number As Integer)
                                                          statusReads += 1
                                                          Throw New InvalidOperationException("text labels must short circuit numeric status")
                                                      End Function
            Assert.IsTrue(UdfPlotDataPreparation38.CompleteRocInputPrepared(values, labels, "YES", lower, AddressOf Finite, statusPolicy, AddressOf Text, AddressOf Normalize, output))
            Dim sign = If(lower, -1.0R, 1.0R)
            CollectionAssert.AreEqual(New Double() {2 * sign, 3 * sign}, output(0))
            CollectionAssert.AreEqual(New Double() {5 * sign, 1 * sign}, output(1))
            Assert.AreEqual(0, statusReads)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub RocNumericStatusFallbackAndSkippedRowsRemainDistinct()
        Dim output()() As Double = Nothing
        Assert.IsTrue(UdfPlotDataPreparation38.CompleteRocInputPrepared(
            New Object(,) {{1}, {2}, {3}, {4}, {Double.NaN}}, New Object(,) {{0}, {1}, {"other"}, {Nothing}, {1}},
            "1", False, AddressOf Finite, AddressOf Status, AddressOf Text, AddressOf Normalize, output))
        CollectionAssert.AreEqual(New Double() {2}, output(0))
        CollectionAssert.AreEqual(New Double() {1, 3}, output(1))
    End Sub

    <TestMethod>
    Public Sub RocPreparedFailureDoesNotInventAnOutputOrResetHostOwnedState()
        Dim sentinel()() As Double = {New Double() {17}}, output = sentinel
        Assert.IsFalse(UdfPlotDataPreparation38.CompleteRocInputPrepared(New Object(,) {{1}}, New Object(,) {{1}},
            "1", False, AddressOf Finite, AddressOf Status, AddressOf Text, AddressOf Normalize, output))
        Assert.AreSame(sentinel, output)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub SurvivalPreparedKeepsSourceOrderCaseSensitiveGroupIdsAndSkippedRows(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim records As List(Of survival.SurvivalRecord) = Nothing
            Assert.IsTrue(UdfPlotDataPreparation38.CompleteSurvivalRecordsPrepared(
                New Object(,) {{5}, {Double.NaN}, {7}, {9}}, New Object(,) {{1}, {1}, {0}, {1}},
                New Object(,) {{"B"}, {"ignored"}, {"b"}, {""}}, True, AddressOf Finite, AddressOf Status, AddressOf Text, records))
            CollectionAssert.AreEqual(New Double() {5, 7}, records.Select(Function(r) r.Time).ToArray())
            CollectionAssert.AreEqual(New String() {"B", "b"}, records.Select(Function(r) r.strGroup).ToArray())
            CollectionAssert.AreEqual(New Integer() {0, 1}, records.Select(Function(r) r.Group).ToArray())
            CollectionAssert.AreEqual(New Integer() {1, 0}, records.Select(Function(r) r.Censorship).ToArray())
            Assert.IsTrue(records.All(Function(r) r.strStratum = "ALL"))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub NegativeSurvivalTimeFailsBeforeStatusAndLabelPolicies()
        Dim calls As Integer, records As List(Of survival.SurvivalRecord) = Nothing
        Dim statusPolicy As UdfPlotStatusReader38 = Function(cell As Object, ByRef number As Integer)
                                                      calls += 1
                                                      Throw New InvalidOperationException("status should not run")
                                                  End Function
        Assert.IsFalse(UdfPlotDataPreparation38.CompleteSurvivalRecordsPrepared(New Object(,) {{-1}}, New Object(,) {{"bad"}}, Nothing,
            True, AddressOf Finite, statusPolicy, Function(cell) ThrowText(), records))
        Assert.AreEqual(0, calls)
        Assert.IsNull(records)
    End Sub

    Private Shared Function ThrowText() As String
        Throw New InvalidOperationException("label should not run")
    End Function

    <TestMethod>
    Public Sub SurvivalWithoutGroupDoesNotReadAnAbsentGroupColumn()
        Dim records As List(Of survival.SurvivalRecord) = Nothing
        Assert.IsTrue(UdfPlotDataPreparation38.CompleteSurvivalRecordsPrepared(New Object(,) {{0}, {2}}, New Object(,) {{1}, {0}}, Nothing,
            False, AddressOf Finite, AddressOf Status, Function(cell) ThrowText(), records))
        Assert.IsTrue(records.All(Function(r) r.strGroup = "ALL" AndAlso r.Group = 0))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub HistogramPartsKeepSeriesOrderLabelsTypesAndEngineInputs(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim values As Double() = {3}, seen As Integer
            Dim bins As Func(Of Double(), String, Object(,)) = Function(data, rule)
                                                                seen += 1
                                                                Assert.AreSame(values, data)
                                                                Assert.AreEqual("rule passed through", rule)
                                                                Return New Object(,) {{2.0R, 9}, {4.0R, 7}}
                                                            End Function
            Dim parts = PlotDataUDFsReports38.BuildHistogramPartsPrepared(New Double()() {Nothing, New Double() {}, values},
                New String() {"skip", "skip", " label "}, "rule passed through", bins)
            Assert.AreEqual(1, seen)
            Assert.AreEqual(1, parts.Count)
            Assert.AreEqual(3, parts(0).GetLength(0))
            CollectionAssert.AreEqual(New Object() {"Group", "BinMidpoint", "Frequency"}, Enumerable.Range(0, 3).Select(Function(j) parts(0)(0, j)).ToArray())
            Assert.AreEqual(" label ", parts(0)(1, 0))
            Assert.AreEqual(GetType(Double), parts(0)(1, 1).GetType())
            Assert.AreEqual(GetType(Integer), parts(0)(1, 2).GetType())
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub HistogramNormalKeepsInvariantMidpointsAndEnginePrecedence(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim order As New List(Of String)
            Dim parts = PlotDataUDFsReports38.BuildHistogramNormalPartsPrepared(New Double()() {New Double() {1, 2}}, New String() {"G"}, "same",
                Function(values, rule)
                    order.Add("bins")
                    Return New Object(,) {{"1.5", 7}, {2.5R, 8}}
                End Function,
                Function(values, mids)
                    order.Add("overlay")
                    CollectionAssert.AreEqual(New Double() {1.5R, 2.5R}, mids)
                    Return New Double(,) {{10, 11}, {12, 13}}
                End Function)
            CollectionAssert.AreEqual(New String() {"bins", "overlay"}, order)
            Assert.AreEqual(10.0R, parts(0)(1, 1))
            Assert.AreEqual(13.0R, parts(0)(2, 2))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub MissingOverlaySkipsLabelsAndEmptyBinsSkipOverlay()
        Dim overlayCalls As Integer
        Dim parts = PlotDataUDFsReports38.BuildHistogramNormalPartsPrepared(New Double()() {New Double() {1, 2}}, Nothing, "x",
            Function(values, rule) New Object(1, 1) {}, Function(values, mids)
                                                         overlayCalls += 1
                                                         Return Nothing
                                                     End Function)
        Assert.AreEqual(0, parts.Count)
        Assert.AreEqual(1, overlayCalls)
        parts = PlotDataUDFsReports38.BuildHistogramNormalPartsPrepared(New Double()() {New Double() {1, 2}}, Nothing, "x",
            Function(values, rule) Nothing, Function(values, mids)
                                               Assert.Fail("overlay after absent bins")
                                               Return Nothing
                                           End Function)
        Assert.AreEqual(0, parts.Count)
    End Sub

    <TestMethod>
    Public Sub HistogramRunsEngineBeforeAnInvalidLabelIndex()
        Dim calls As Integer
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() PlotDataUDFsReports38.BuildHistogramPartsPrepared(New Double()() {New Double() {1}}, New String() {}, "x",
            Function(values, rule)
                calls += 1
                Return New Object(,) {{1, 2}}
            End Function))
        Assert.AreEqual(1, calls)
    End Sub

    <TestMethod>
    Public Sub KmCountsAndFillsWithSeparateCallbacksAndExactStepRows()
        Dim calls As New List(Of String)
        Dim records As Object() = {New survival.SurvivalTableRecord With {.strGroup = "G", .Time = 4, .AtRisk = 3, .Prob = 0.5R, .ProbCILL = 0.3R, .ProbCIUL = 0.8R},
            New survival.SurvivalTableRecord With {.strGroup = "G", .Time = 7, .AtRisk = 2, .Prob = 0.25R, .ProbCILL = 0.1R, .ProbCIUL = 0.6R}}
        Dim result = PlotDataUDFsReports38.BuildKaplanMeierStepTablePrepared(New Object() {Nothing, 7, records},
            Function(value)
                calls.Add("count")
                Return CType(value, survival.SurvivalTableRecord)
            End Function,
            Function(value)
                calls.Add("fill")
                Return CType(value, survival.SurvivalTableRecord)
            End Function, Function(label, value) New Object(,) {{label, value}})
        CollectionAssert.AreEqual(New String() {"count", "count", "fill", "fill"}, calls)
        Assert.AreEqual(6, result.GetLength(0))
        Assert.AreEqual(7, result.GetLength(1))
        Assert.AreEqual("AtRisk", result(0, 6))
        Assert.AreEqual(0.0R, result(1, 2))
        Assert.AreEqual(1.0R, result(2, 3))
        Assert.AreEqual(0.5R, result(3, 3))
        Assert.AreEqual(0.5R, result(4, 3))
        Assert.AreEqual(0.25R, result(5, 3))
        Assert.AreEqual("", result(2, 6))
        Assert.AreEqual(2, result(5, 6))
    End Sub

    <TestMethod>
    Public Sub KmEmptyGroupsUseOnlyTheNotePolicy()
        Dim notes As Integer
        Dim result = PlotDataUDFsReports38.BuildKaplanMeierStepTablePrepared(New Object() {Nothing, 7, New Object() {}},
            Function(value) CType(value, survival.SurvivalTableRecord), Function(value) CType(value, survival.SurvivalTableRecord),
            Function(label, value)
                notes += 1
                Assert.AreEqual("Kaplan-Meier", label)
                Return New Object(,) {{label, value}}
            End Function)
        Assert.AreEqual(1, notes)
        Assert.AreEqual("No survival-curve rows were returned.", result(0, 1))
    End Sub

    <TestMethod>
    Public Sub KmCastFailureOccursInTheCountCallbackBeforeFill()
        Dim fills As Integer
        Assert.ThrowsException(Of InvalidCastException)(Sub() PlotDataUDFsReports38.BuildKaplanMeierStepTablePrepared(New Object() {New Object() {New Object()}},
            Function(value) CType(value, survival.SurvivalTableRecord), Function(value)
                                                                         fills += 1
                                                                         Return CType(value, survival.SurvivalTableRecord)
                                                                     End Function, Function(label, value) Nothing))
        Assert.AreEqual(0, fills)
    End Sub

    <TestMethod>
    Public Sub StackingKeepsOneHeaderRawCellReferencesAndEmptyBodyParts()
        Dim marker As New Object(), first(,) As Object = {{"A", "B"}, {marker, 7}}, empty(,) As Object = {{"ignored", "ignored"}}
        Dim result = PlotDataUDFsReports38.StackHomogeneousTablesPrepared(New List(Of Object(,)) From {first, empty, New Object(,) {{"ignored", "ignored"}, {"last", Nothing}}})
        Assert.AreEqual(3, result.GetLength(0))
        Assert.AreSame(marker, result(1, 0))
        Assert.AreEqual("A", result(0, 0))
        Assert.AreEqual("last", result(2, 0))
        Assert.IsNull(result(2, 1))
    End Sub

    <TestMethod>
    Public Sub StackingDoesNotNormalizeNonzeroArrayLowerBounds()
        Dim shifted = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {2, 2}, New Integer() {1, 1}), Object(,))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() PlotDataUDFsReports38.StackHomogeneousTablesPrepared(New List(Of Object(,)) From {shifted}))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub DiscriminantGroupSelectionKeepsFirstOrdinalMatchAndReferenceIdentity(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim first As New Multivariate.DiscriminantGroupStatistics With {.GroupLabel = "A"}
            Assert.AreSame(first, MultivariateAnalysisUDFsReports38.FindDiscriminantGroupPrepared(New Multivariate.DiscriminantGroupStatistics() {first, Nothing}, "a"))
            Assert.IsNull(MultivariateAnalysisUDFsReports38.FindDiscriminantGroupPrepared(New Multivariate.DiscriminantGroupStatistics() {first}, "missing"))
            Assert.ThrowsException(Of NullReferenceException)(Sub() MultivariateAnalysisUDFsReports38.FindDiscriminantGroupPrepared(New Multivariate.DiscriminantGroupStatistics() {Nothing}, "missing"))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub DiscriminantWrappedSelectionReturnsBeforeADeferredNullTable()
        Dim table As New ResultTable, marker As New Object()
        table.SetBody(New Object(,) {{"Wanted suffix", marker}})
        Dim result = MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(New ResultTable() {table, Nothing}, "wanted", True, AddressOf Text)
        Assert.AreSame(marker, result(0, 1))
        Assert.IsNull(MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(New ResultTable() {table}, "wanted", False, AddressOf Text))
        Assert.ThrowsException(Of NullReferenceException)(Sub() MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(New ResultTable() {Nothing, table}, "wanted", True, AddressOf Text))
    End Sub
End Class
