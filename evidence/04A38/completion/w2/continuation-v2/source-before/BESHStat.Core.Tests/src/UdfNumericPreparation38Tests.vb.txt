Option Explicit On
Option Strict On

Imports System
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.WorksheetCompatibility

<TestClass>
Public Class UdfNumericPreparation38Tests
    <TestMethod>
    Public Sub PercentGuardsRetainUdfNegativeAndNonfiniteSemantics()
        CollectionAssert.AreEqual(New Double() {-25.0, -75.0}, UdfNumericPreparation38.PercentOfTotal({1.0, 3.0}, -4.0))
        For Each total As Double In {0.0, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}
            CollectionAssert.AreEqual(New Double() {0.0, 0.0}, UdfNumericPreparation38.PercentOfTotal({1.0, 3.0}, total))
        Next
        Assert.IsNull(UdfNumericPreparation38.PercentOfTotal(Nothing, 1.0))
    End Sub

    <TestMethod>
    Public Sub DistinctEventRateRulesAreNotInterchanged()
        Dim y As Double() = {0.25, 0.75}
        Assert.AreEqual(0.5, UdfNumericPreparation38.ComputeWeightedEventRate(y, Nothing))
        Assert.AreEqual(0.5, UdfNumericPreparation38.ComputeBinaryEventRate(y, Nothing))
        Assert.AreEqual(0.75, UdfNumericPreparation38.ComputeWeightedEventRate(y, {1.0, 3.0}))
        Assert.AreEqual(0.625, UdfNumericPreparation38.ComputeBinaryEventRate(y, {1.0, 3.0}))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfNumericPreparation38.ComputeWeightedEventRate(y, {1.0}))
        Assert.AreEqual(0.5, UdfNumericPreparation38.ComputeBinaryEventRate(y, {1.0}))
    End Sub

    <TestMethod>
    Public Sub PredictorRetainsOffsetAndCoefficientOrdering()
        Dim x As Double(,) = {{2.0, 3.0}, {-4.0, 5.0}}
        Assert.AreEqual(12.0, UdfNumericPreparation38.ComputeLinearPredictor(x, 0, {1.0, 2.0, 1.0}, True, {4.0, 8.0}))
        Assert.AreEqual(5.0, UdfNumericPreparation38.ComputeLinearPredictor(x, 1, {2.0, 1.0}, False, {4.0, 8.0}))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfNumericPreparation38.ComputeLinearPredictor(x, -1, {1.0, 2.0}, False, Nothing))
    End Sub

    <TestMethod>
    Public Sub PatternStructureProductIsNotSquaredPattern()
        CollectionAssert.AreEqual(New Double() {26.0, 44.0}, UdfNumericPreparation38.ColumnSumsOfSquares(New Double(,) {{1, 2}, {3, 4}}, New Double(,) {{5, 6}, {7, 8}}))
        Assert.IsNull(UdfNumericPreparation38.ColumnSumsOfSquares(Nothing, New Double(,) {{1.0}}))
    End Sub

    <TestMethod>
    Public Sub IntegerTotalsKeepCheckedOverflow()
        Assert.ThrowsException(Of OverflowException)(Sub() UdfNumericPreparation38.TableTotal(New Integer(,) {{Integer.MaxValue, 1}}))
        Assert.AreEqual(0, UdfNumericPreparation38.TableTotal(New Integer(0, -1) {}))
    End Sub

    <TestMethod>
    Public Sub StepLookupKeepsLegacyNanAndEmptyBehavior()
        Dim values As Double(,) = {{1.0, 0.8}, {3.0, 0.4}}
        Assert.AreEqual(1.0, UdfNumericPreparation38.LookupStepValue(values, 0.0, 1))
        Assert.AreEqual(0.8, UdfNumericPreparation38.LookupStepValue(values, 2.0, 1))
        Assert.AreEqual(0.4, UdfNumericPreparation38.LookupStepValue(values, Double.NaN, 1))
        Assert.AreEqual(1.0, UdfNumericPreparation38.LookupStepValue(New Double(-1, 1) {}, 1.0, 1))
    End Sub

    <TestMethod>
    Public Sub ReturnedArraysDoNotShareInputsOrRepeatedResults()
        Dim input As Double(,) = {{1.0, 2.0}, {3.0, 4.0}}
        Dim first As Double(,) = UdfNumericPreparation38.AddInterceptIfRequested(input, False)
        first(0, 0) = 999.0
        Assert.AreEqual(1.0, input(0, 0))
        Assert.AreEqual(1.0, UdfNumericPreparation38.AddInterceptIfRequested(input, False)(0, 0))
        Dim source As Double() = {BitConverter.Int64BitsToDouble(Long.MinValue), 2.0}
        Dim copied As Double() = UdfNumericPreparation38.CloneDoubleArray(source)
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(copied(0)))
        copied(0) = 7.0
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(source(0)))
    End Sub

    <TestMethod>
    Public Sub ConcurrentPreparationHasNoSharedResultState()
        Dim inputs As Double() = {1.0, 2.0, 3.0}
        Parallel.For(0, 40, Sub(i)
                                Dim result As Double() = UdfNumericPreparation38.Cumulative(inputs)
                                Assert.AreEqual(6.0, result(2))
                                result(2) = CDbl(i)
                            End Sub)
        CollectionAssert.AreEqual(New Double() {1.0, 2.0, 3.0}, inputs)
        CollectionAssert.AreEqual(New Double() {1.0, 3.0, 6.0}, UdfNumericPreparation38.Cumulative(inputs))
    End Sub

    <TestMethod>
    Public Sub ProfileAveragingRetainsObservationOrderAndReturnsDetachedVector()
        Dim design As Double(,) = {{1.0, 2.0}, {1.0, 4.0}, {1.0, 8.0}}
        Dim count As Integer
        Dim result As Double() = UdfProfilePreparation38.AverageDesign(design, {0.0, 0.0, 1.0}, True, 0.0,
            New Integer() {}, Function(column) column, Function(column) 0.0,
            Function(a, b) a = b, count)
        Assert.AreEqual(2, count)
        CollectionAssert.AreEqual(New Double() {1.0, 3.0}, result)
        result(1) = 99.0
        Assert.AreEqual(2.0, design(0, 1))
    End Sub

    <TestMethod>
    Public Sub ProfileAverageSkipsUnavailableTimeRowsBeforeProfileEnumeration()
        Dim count As Integer = 7
        Dim result As Double() = UdfProfilePreparation38.AverageDesign(Of Integer)(New Double(,) {{1.0}}, Nothing,
            True, 0.0, Nothing, Function(column) column, Function(column) 0.0, Function(a, b) a = b, count)
        Assert.IsNull(result)
        Assert.AreEqual(0, count)
    End Sub
End Class
