Option Explicit On
Option Strict On

Imports System.Threading
Imports System.Threading.Tasks
Imports BESHStatNG.ExcelCommon

Namespace AppInfrastructure
    ''' <summary>Form-owned repeat/reentrancy guard, independent of all other forms/runs.</summary>
    Friend NotInheritable Class WindowsExecutionDispatcher
        Private _active As Integer
        Friend Function TryBegin() As WindowsExecutionRun
            If Interlocked.CompareExchange(_active, 1, 0) <> 0 Then Return Nothing
            Return New WindowsExecutionRun(Sub() Interlocked.Exchange(_active, 0))
        End Function
    End Class

    ''' <summary>Runs delegates inline on the calling Excel/UI thread. Never pumps or
    ''' backgrounds COM. Cancellation is observed only at the next supported boundary.</summary>
    Friend NotInheritable Class WindowsExecutionRun
        Implements IDisposable
        Friend ReadOnly Property Execution As New ExecutionCoordinator()
        Private ReadOnly _release As Action
        Private _disposed As Integer
        Friend Sub New(release As Action)
            _release = release
        End Sub
        Friend Sub Checkpoint(stage As ExecutionStage, Optional percent As Integer? = Nothing)
            Execution.ThrowIfCancellationRequested()
            Execution.EnterStage(stage, percent)
        End Sub
        Friend Function InvokeStage(Of T)(stage As ExecutionStage, work As Func(Of T)) As T
            Checkpoint(stage)
            Dim result = work()
            Execution.ThrowIfCancellationRequested()
            Return result
        End Function
        Friend Function Write(work As Func(Of OutputWriteState)) As OutputWriteState
            Checkpoint(ExecutionStage.Output, 90)
            Dim result = work() ' Existing writer owns consent/partial outcomes.
            If result = OutputWriteState.Completed Then
                Execution.TryComplete() ' No post-write cancellation checkpoint.
            ElseIf result = OutputWriteState.CancelledByUser Then
                Execution.RequestCancellation()
                Execution.ThrowIfCancellationRequested()
            Else
                Execution.TryFail(New InvalidOperationException("Output did not complete."))
            End If
            Return result
        End Function
        Friend Function FailureDisplay(failureException As Exception, result As WriteResult, runId As String) As String
            Return UserFaultMapper.FormatDisplay(UserFaultMapper.FromOutcome(Execution.Snapshot, result, failureException), runId)
        End Function
        ' Temporary 03.06 close/cancel smoke seam. No visible demo controls in the normal form.
        ' Mandatory Phase 03 exit: remove this opt-in and SyntheticFixtureAsync after host acceptance.
        Friend Shared Function DeveloperFixtureEnabled(value As String) As Boolean
            Return String.Equals(value, "1", StringComparison.Ordinal)
        End Function
        Friend Async Function SyntheticFixtureAsync() As Task
            For stepIndex As Integer = 0 To 49
                Checkpoint(ExecutionStage.SyntheticFixture, stepIndex \ 10)
                Await Task.Delay(100, Execution.Token)
            Next
            Execution.ThrowIfCancellationRequested()
        End Function
        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.Exchange(_disposed, 1) <> 0 Then Return
            Execution.Dispose()
            _release()
        End Sub
    End Class
End Namespace
