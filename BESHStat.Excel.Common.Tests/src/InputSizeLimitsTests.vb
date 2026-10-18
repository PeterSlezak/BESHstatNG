Option Explicit On
Option Strict On

Imports System
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class InputSizeLimitsTests
    Private Shared Function Rejection(action As Action, code As String) As SpreadsheetValidationIssue
        Dim ex = Assert.ThrowsException(Of SpreadsheetInputValidationException)(action)
        Assert.AreEqual(code, ex.Report.Issues(0).Code)
        StringAssert.Contains(ex.Message, "requested")
        StringAssert.Contains(ex.Message, "allowed")
        Return ex.Report.Issues(0)
    End Function

    <TestMethod>
    Public Sub Defaults_AreBoundedAndMatchOfficePolicy()
        Dim limits As New InputSizeLimits()
        Assert.AreEqual(10000L, limits.MaxRangeCells)
        Assert.AreEqual(10000L, limits.MaxTotalCells)
        Assert.AreEqual(64, limits.MaxRanges)
        Assert.AreEqual(2000, limits.MaxChunkCells)
        Assert.AreEqual(67108864R, limits.MaxEstimatedBytes)
        Assert.AreEqual(1024R, limits.EstimatedBytesPerCell)
        Assert.AreEqual(30000R, limits.TimeBudgetMilliseconds)
    End Sub

    <TestMethod>
    Public Sub ExactCellLimit_IsAccepted()
        Dim budget As New InputAcquisitionBudget()
        budget.AddRange(100, 100)
        Assert.AreEqual(10000L, budget.TotalCells)
        Assert.AreEqual(1, budget.RangeCount)
    End Sub

    <TestMethod>
    Public Sub LimitPlusOne_IsRejectedWithoutChangingCounters()
        Dim budget As New InputAcquisitionBudget()
        Rejection(Sub() budget.AddRange(1, 10001), "input.range-cells")
        Assert.AreEqual(0L, budget.TotalCells)
    End Sub

    <TestMethod>
    Public Sub ExplicitLargeQuota_IsHostNeutralAndIndependentOfChunkSize()
        Dim budget As New InputAcquisitionBudget(New InputSizeLimits(maxRangeCells:=2000000, maxTotalCells:=4000000,
            maxChunkCells:=17, maxEstimatedBytes:=4096000000))
        budget.AddRange(1, 2000000) ' exceeds Excel column count; Common has no Excel bounds
        budget.AddRange(2000000, 1) ' exceeds Excel row count
        Assert.AreEqual(4000000L, budget.TotalCells)
    End Sub

    <TestMethod>
    Public Sub MultipleRanges_ExactAggregateThenOneMore()
        Dim budget As New InputAcquisitionBudget()
        budget.AddRange(100, 60)
        budget.AddRange(100, 40)
        Dim issue = Rejection(Sub() budget.AddRange(1, 1), "input.total-cells")
        Assert.AreEqual("maxTotalCells", issue.FieldPath)
        StringAssert.Contains(issue.Message, "10001")
        Assert.AreEqual(10000L, budget.TotalCells)
        Assert.AreEqual(2, budget.RangeCount)
    End Sub

    <TestMethod>
    Public Sub RangeCount_IsBoundedIncludingDuplicateRectangles()
        Dim budget As New InputAcquisitionBudget(New InputSizeLimits(maxRanges:=2))
        budget.AddRange(1, 1)
        budget.AddRange(1, 1)
        Rejection(Sub() budget.AddRange(1, 1), "input.range-count")
    End Sub

    <TestMethod>
    Public Sub InvalidDimensions_AreRejected()
        For Each value As Long In {0L, -1L, Long.MinValue}
            Dim budget As New InputAcquisitionBudget()
            Rejection(Sub() budget.AddRange(value, 1), "input.dimensions")
            Rejection(Sub() budget.AddRange(1, value), "input.dimensions")
        Next
    End Sub

    <TestMethod>
    Public Sub MultiplicationOverflow_IsRejectedBeforeAllocation()
        Dim budget As New InputAcquisitionBudget()
        Rejection(Sub() budget.AddRange(Long.MaxValue, Long.MaxValue), "input.cell-overflow")
        Rejection(Sub() budget.AddRange(Long.MaxValue, 2), "input.cell-overflow")
        Rejection(Sub() budget.AddRange(Integer.MaxValue, Integer.MaxValue), "input.range-cells")
    End Sub

    <TestMethod>
    Public Sub AggregateCannotWrapLongMaxValue()
        Dim budget As New InputAcquisitionBudget(New InputSizeLimits(maxRangeCells:=Long.MaxValue, maxTotalCells:=Long.MaxValue,
            maxEstimatedBytes:=9007199254740991R, estimatedBytesPerCell:=0.0001))
        budget.AddRange(Long.MaxValue, 1)
        Rejection(Sub() budget.AddRange(1, 1), "input.total-cells")
        Assert.AreEqual(Long.MaxValue, budget.TotalCells)
    End Sub

    <TestMethod>
    Public Sub EstimateExactLimitAcceptedThenRejected()
        Dim budget As New InputAcquisitionBudget(New InputSizeLimits(maxEstimatedBytes:=2048))
        budget.AddRange(1, 2)
        Rejection(Sub() budget.AddRange(1, 1), "input.estimated-bytes")
        Assert.AreEqual(2L, budget.TotalCells)
    End Sub

    <TestMethod>
    Public Sub InvalidIntegerOptions_AreRejected()
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(maxRangeCells:=0))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(maxTotalCells:=-1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(maxRanges:=0))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(maxChunkCells:=0))
    End Sub

    <TestMethod>
    Public Sub NonfiniteNegativeAndOversizedBudgets_AreRejected()
        For Each value As Double In {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, -1R, 0R, Double.MaxValue}
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(maxEstimatedBytes:=value))
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(estimatedBytesPerCell:=value))
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New InputSizeLimits(timeBudgetMilliseconds:=value))
        Next
    End Sub

    <TestMethod>
    Public Sub CancellationBeforePreflightAndBetweenPayloads_IsObserved()
        Using source As New CancellationTokenSource()
            Dim budget As New InputAcquisitionBudget(cancellation:=source.Token)
            budget.AddRange(2, 2)
            source.Cancel()
            Assert.ThrowsException(Of OperationCanceledException)(Sub() budget.Checkpoint())
            Assert.ThrowsException(Of OperationCanceledException)(Sub() budget.AddRange(1, 1))
            Assert.ThrowsException(Of OperationCanceledException)(Function() New InputAcquisitionBudget(cancellation:=source.Token))
        End Using
    End Sub

    <TestMethod>
    Public Sub Deadline_IsMeasuredFromOperationStartIncludingMetadata()
        Dim elapsed As Double = 0
        Dim budget As New InputAcquisitionBudget(New InputSizeLimits(timeBudgetMilliseconds:=10), elapsedMilliseconds:=Function() elapsed)
        elapsed = 10
        budget.AddRange(2, 2)
        elapsed = 11
        Rejection(Sub() budget.Checkpoint(), "input.time-budget")
        Rejection(Sub() budget.AddRange(1, 1), "input.time-budget")
    End Sub

    <TestMethod>
    Public Sub HostClockMustSupplyFiniteNonnegativeElapsedTime()
        For Each value As Double In {Double.NaN, Double.PositiveInfinity, -1R}
            Assert.ThrowsException(Of ArgumentException)(Function() New InputAcquisitionBudget(elapsedMilliseconds:=Function() value))
        Next
        Dim elapsed As Double = 2
        Dim budget As New InputAcquisitionBudget(elapsedMilliseconds:=Function() elapsed)
        elapsed = 1
        Assert.ThrowsException(Of ArgumentException)(Sub() budget.Checkpoint())
    End Sub
End Class
