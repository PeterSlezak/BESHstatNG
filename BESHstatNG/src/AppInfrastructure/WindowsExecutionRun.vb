Option Explicit On
Option Strict On

Imports System.Threading
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
        Friend Property Lifecycle As WindowsDocumentLifecycle
        Private _sourceAddress As String
        Private _groupAddress As String
        Friend ReadOnly Property SourceAddress As String
            Get
                Return _sourceAddress
            End Get
        End Property
        Friend ReadOnly Property GroupAddress As String
            Get
                Return _groupAddress
            End Get
        End Property
        Friend Sub BindInputAddresses(sourceAddress As String, groupAddress As String)
            If _disposed <> 0 OrElse _sourceAddress IsNot Nothing OrElse String.IsNullOrWhiteSpace(sourceAddress) OrElse sourceAddress.Length > 16384 OrElse
                (groupAddress IsNot Nothing AndAlso groupAddress.Length > 16384) Then Throw New DocumentLifecycleException(DocumentInvalidationReason.Unverifiable)
            _sourceAddress = sourceAddress : _groupAddress = groupAddress
        End Sub
        Friend Sub New(release As Action)
            _release = release
        End Sub
        Friend Sub Checkpoint(stage As ExecutionStage, Optional percent As Integer? = Nothing)
            Execution.ThrowIfCancellationRequested()
            If _disposed <> 0 OrElse Execution.Snapshot.IsTerminal Then Throw New DocumentLifecycleException(DocumentInvalidationReason.ContextDisposed)
            Lifecycle?.EnsureCurrent()
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
        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.Exchange(_disposed, 1) <> 0 Then Return
            Try
                Lifecycle?.Dispose()
            Finally
                Lifecycle = Nothing
                _sourceAddress = Nothing : _groupAddress = Nothing
                Execution.Dispose()
                _release()
            End Try
        End Sub
    End Class
End Namespace
