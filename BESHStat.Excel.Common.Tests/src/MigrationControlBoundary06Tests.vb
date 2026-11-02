Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Real Common policy combinations. No Windows form, Excel adapter or COM activation.
<TestClass>
Public Class MigrationControlBoundary06Tests
    <DataTestMethod>
    <DataRow(False, DocumentInvalidationReason.WorksheetRemoved)>
    <DataRow(True, DocumentInvalidationReason.WorksheetRemoved)>
    <DataRow(False, DocumentInvalidationReason.WorkbookClosed)>
    <DataRow(True, DocumentInvalidationReason.WorkbookClosed)>
    Public Sub CopiedInputCannotMakeInvalidOutputTargetSafe(copied As Boolean, reason As DocumentInvalidationReason)
        Dim binding As New DocumentLifecycleSnapshot("disposable", "original-sheet", "Results", New OutputRectangle(1, 1, 4, 6), "r1")
        Using source As New DocumentLifecycleLease(binding, selectionBound:=True), target As New DocumentLifecycleLease(binding)
            If copied Then source.MarkInputCopied()
            target.Invalidate(reason)
            Dim writes As Integer
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub()
                target.EnsureCurrent(binding)
                writes += 1
            End Sub)
            Assert.AreEqual(0, writes, "Mock write delegate must remain uncalled")
            Assert.AreNotEqual(DocumentLifecycleStatus.VerifiedEqual, target.Check(binding))
            ' A fresh identical name does not release sticky invalidation.
            Assert.ThrowsException(Of DocumentLifecycleException)(Sub() target.EnsureCurrent(
                New DocumentLifecycleSnapshot("disposable", "replacement-sheet", "Results")))
        End Using
    End Sub

    <DataTestMethod>
    <DataRow(ExecutionStage.Acquisition)> <DataRow(ExecutionStage.Analysis)> <DataRow(ExecutionStage.Output)>
    Public Sub CancelledBoundaryPreventsMockSideEffect(stage As ExecutionStage)
        Using run As New ExecutionCoordinator()
            run.Start()
            run.EnterStage(stage)
            run.RequestCancellation()
            Dim writes As Integer
            Assert.ThrowsException(Of OperationCanceledException)(Sub()
                run.ThrowIfCancellationRequested()
                writes += 1
            End Sub)
            Assert.AreEqual(0, writes)
            Assert.AreEqual(ExecutionState.Cancelled, run.Snapshot.State)
            Assert.IsFalse(run.TryComplete())
        End Using
    End Sub

    <TestMethod>
    Public Sub CompletionBeforeStartIsRejectedWithoutOutput()
        Using run As New ExecutionCoordinator()
            Dim writes As Integer
            Assert.ThrowsException(Of InvalidOperationException)(Sub()
                run.TryComplete()
                writes += 1
            End Sub)
            Assert.AreEqual(0, writes)
            run.Start()
            Assert.IsTrue(run.TryComplete())
        End Using
    End Sub
End Class
