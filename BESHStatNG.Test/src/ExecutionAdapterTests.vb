Option Explicit On
Option Strict On
Imports System.Threading
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ExecutionAdapterTests
    <TestMethod>
    Public Sub RepeatComputeGuardReleasesOnDisposeAndFormsAreIndependent()
        Dim first As New WindowsExecutionDispatcher(), second As New WindowsExecutionDispatcher()
        Using run = first.TryBegin(), other = second.TryBegin()
            Assert.IsNotNull(run)
            Assert.IsNotNull(other)
            Assert.IsNull(first.TryBegin())
            run.Execution.Start() : other.Execution.Start()
            run.Execution.RequestCancellation()
            Assert.IsFalse(other.Execution.Token.IsCancellationRequested)
        End Using
        Using fresh = first.TryBegin()
            Assert.IsNotNull(fresh)
            Assert.IsFalse(fresh.Execution.Token.IsCancellationRequested)
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub CancelBeforeComputeOrWritePreventsDelegate(beforeCompute As Boolean)
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            Dim calls As Integer
            run.Execution.RequestCancellation()
            Assert.ThrowsException(Of OperationCanceledException)(Sub()
                                                                      If beforeCompute Then
                                                                          run.InvokeStage(ExecutionStage.Analysis, Function()
                                                                                                                      calls += 1
                                                                                                                      Return 1
                                                                                                                  End Function)
                                                                      Else
                                                                          run.Write(Function()
                                                                                        calls += 1
                                                                                        Return OutputWriteState.Completed
                                                                                    End Function)
                                                                      End If
                                                                  End Sub)
            Assert.AreEqual(0, calls)
            Assert.AreEqual(ExecutionState.Cancelled, run.Execution.Snapshot.State)
        End Using
    End Sub

    <TestMethod>
    Public Sub DispatcherStaysOnCallingThreadAndChecksAfterComputation()
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            Dim thread = Threading.Thread.CurrentThread.ManagedThreadId
            Assert.ThrowsException(Of OperationCanceledException)(Sub() run.InvokeStage(ExecutionStage.Analysis,
                Function()
                    Assert.AreEqual(thread, Threading.Thread.CurrentThread.ManagedThreadId)
                    run.Execution.RequestCancellation()
                    Return 12
                End Function))
        End Using
    End Sub

    <DataTestMethod> <DataRow(OutputWriteState.Completed)> <DataRow(OutputWriteState.CancelledByUser)>
    <DataRow(OutputWriteState.RejectedBeforeWrite)> <DataRow(OutputWriteState.FailedBeforeWrite)> <DataRow(OutputWriteState.PartiallyWritten)>
    Public Sub ExistingOutputOutcomesArePreserved(state As OutputWriteState)
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            If state = OutputWriteState.CancelledByUser Then
                Assert.ThrowsException(Of OperationCanceledException)(Sub() run.Write(Function() state))
                Assert.AreEqual(ExecutionState.Cancelled, run.Execution.Snapshot.State)
            Else
                Assert.AreEqual(state, run.Write(Function() state))
                Assert.AreEqual(If(state = OutputWriteState.Completed, ExecutionState.Completed, ExecutionState.Failed), run.Execution.Snapshot.State)
            End If
        End Using
    End Sub

    <TestMethod>
    Public Sub RequestDuringCompletedWriteCannotRetroactivelyCancelIt()
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            run.Write(Function()
                          run.Execution.RequestCancellation()
                          Return OutputWriteState.Completed
                      End Function)
            Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State)
        End Using
    End Sub
End Class
