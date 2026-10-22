Option Explicit On
Option Strict On
Imports System.Threading
Imports System.IO
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

    <DataTestMethod>
    <DataRow(OutputWriteState.Completed, "")>
    <DataRow(OutputWriteState.CancelledByUser, "output.cancelled")>
    <DataRow(OutputWriteState.RejectedBeforeWrite, "output.consent-stale")>
    <DataRow(OutputWriteState.FailedBeforeWrite, "output.host-failed")>
    <DataRow(OutputWriteState.PartiallyWritten, "output.mutation-uncertain")>
    Public Sub PilotMessageUsesActualWriterOutcome(state As OutputWriteState, expectedCode As String)
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            Dim intent = New WriteIntent("plan", "target", New SpreadsheetOutputTarget(), New WriteRegion() {})
            Dim result = New WriteResult(state, WriteCodes.Changed, "PRIVATE C:\secret.xlsx", intent,
                state = OutputWriteState.PartiallyWritten,
                confirmationRequired:=If(state = OutputWriteState.CancelledByUser, CType(True, Boolean?), Nothing),
                overwriteApproved:=If(state = OutputWriteState.CancelledByUser, CType(False, Boolean?), Nothing))
            Try
                run.Write(Function() state)
            Catch ex As OperationCanceledException
                run.Execution.TryFail(ex)
            End Try
            Dim runId = Guid.NewGuid().ToString("N")
            Dim display = run.FailureDisplay(New Exception("PRIVATE"), result, runId)
            If state = OutputWriteState.Completed Then
                Assert.AreEqual(String.Empty, display)
            Else
                StringAssert.Contains(display, expectedCode)
                StringAssert.Contains(display, runId)
                Assert.IsFalse(display.Contains("PRIVATE"))
                Assert.IsFalse(display.Contains("secret.xlsx"))
            End If
            Assert.AreEqual(state, result.State)
        End Using
    End Sub

    <TestMethod>
    Public Sub PilotFailureDoesNotTreatAnUnrelatedCancellationAsCancelled()
        Using run = New WindowsExecutionDispatcher().TryBegin()
            run.Execution.Start()
            run.Execution.RequestCancellation()
            Dim ex As New OperationCanceledException("PRIVATE")
            run.Execution.TryFail(ex)
            StringAssert.Contains(run.FailureDisplay(ex, Nothing, Nothing), UserFaultCodes.Unexpected)
            Assert.AreEqual(ExecutionState.Failed, run.Execution.Snapshot.State)
        End Using
    End Sub

    <TestMethod>
    Public Sub PilotFormHasNoDemoControlsOrArtificialNormalDelayAndKeepsUiOwnership()
        Dim directory = New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        While directory IsNot Nothing AndAlso Not File.Exists(Path.Combine(directory.FullName, "AGENTS.md"))
            directory = directory.Parent
        End While
        Assert.IsNotNull(directory, "Repository source is required for the form source regression check.")
        Dim source = File.ReadAllText(Path.Combine(directory.FullName, "BESHstatNG", "src", "UI", "UibyID.vb"))
        Assert.IsFalse(source.Contains("executionDemo"))
        Assert.IsFalse(source.Contains("Safe yielding demo before run"))
        Assert.IsFalse(source.Contains("Task.Delay("))
        Assert.IsFalse(source.Contains("Task.Run("))
        StringAssert.Contains(source, "WindowsFormsSynchronizationContext")
        StringAssert.Contains(source, "Task.Yield()")
        StringAssert.Contains(source, "ExecutionFormClosing")
        StringAssert.Contains(source, "ShowPilotFault(UserFaultMapper.FromWriteResult(LastSharedWriteOutcome)")
        Assert.IsFalse(source.Contains("BESHSTAT_DEV_EXECUTION_FIXTURE"))
        Assert.IsFalse(source.Contains("SyntheticFixtureAsync"))
        StringAssert.Contains(source, "BindPilotLifecycle(run)")
        StringAssert.Contains(source, "executionRun.SourceAddress")
        StringAssert.Contains(source, "executionRun.GroupAddress")
        StringAssert.Contains(source, "executionCancel")
        StringAssert.Contains(source, "executionLabel")
        Assert.IsFalse(source.Contains("CoreServices.Logger.Error(ex,"), "Native exception text must not enter the default pilot log.")
        StringAssert.Contains(source, "UserFaultMapper.FromException(ex).Code")
        Dim adapter = File.ReadAllText(Path.Combine(directory.FullName, "BESHstatNG", "src", "AppInfrastructure", "WindowsExecutionRun.vb"))
        Assert.IsFalse(adapter.Contains("DeveloperFixtureEnabled"))
        Assert.IsFalse(adapter.Contains("SyntheticFixtureAsync"))
        Assert.IsFalse(adapter.Contains("Task.Delay"))
    End Sub
End Class
