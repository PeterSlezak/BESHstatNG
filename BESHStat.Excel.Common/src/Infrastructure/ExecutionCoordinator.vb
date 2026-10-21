Option Explicit On
Option Strict On

Imports System.Threading
Imports BESHStatNG.AppInfrastructure

Namespace ExcelCommon
    Public Enum ExecutionState
        Created
        Running
        CancellationRequested
        Completed
        Cancelled
        Failed
    End Enum

    ' Fixed public labels only: never retain a Core reporter's free-form message.
    Public Enum ExecutionStage
        None
        Validation
        Acquisition
        Analysis
        Output
        Confirmation
        SyntheticFixture
    End Enum

    ''' <summary>Immutable, atomic projection. Nothing means unknown/suppressed percentage.
    ''' Version increases on every change; concurrent/reentrant notifications may arrive out of order.
    ''' Consumers should read Snapshot or ignore older versions. Contains no input or exception data.</summary>
    Public NotInheritable Class ExecutionSnapshot
        Public ReadOnly Property State As ExecutionState
        Public ReadOnly Property Stage As ExecutionStage
        Public ReadOnly Property Percent As Integer?
        Public ReadOnly Property IsCancellationRequested As Boolean
        Public ReadOnly Property Version As Long
        Public ReadOnly Property IsTerminal As Boolean
            Get
                Return State = ExecutionState.Completed OrElse State = ExecutionState.Cancelled OrElse State = ExecutionState.Failed
            End Get
        End Property
        Friend Sub New(state As ExecutionState, stage As ExecutionStage, percent As Integer?, requested As Boolean, version As Long)
            Me.State = state
            Me.Stage = stage
            Me.Percent = percent
            Me.IsCancellationRequested = requested
            Me.Version = version
        End Sub
    End Class

    ''' <summary>
    ''' Per-operation owner, never ambient. Created -> Running -> Completed/Failed;
    ''' Created/Running -> CancellationRequested -> Cancelled at a checkpoint.
    ''' A request can instead end in Completed (e.g. write already finished) or Failed.
    ''' Exactly one terminal wins. Start is single-use. Finish methods are idempotent.
    ''' Dispose requests cancellation but does not invent a terminal outcome for active work;
    ''' the owner must still unwind/finalize it. Cached Token remains usable after disposal.
    ''' No scheduling, UI dispatch, waits, or diagnostics. Observers run outside locks and
    ''' their exceptions are isolated; observers must be short and must not block their caller.
    ''' </summary>
    Public NotInheritable Class ExecutionCoordinator
        Implements IDisposable, IProgressReporter
        Private ReadOnly _gate As New Object()
        Private ReadOnly _source As New CancellationTokenSource()
        Private ReadOnly _token As CancellationToken
        Private ReadOnly _showProgress As Boolean
        Private _snapshot As New ExecutionSnapshot(ExecutionState.Created, ExecutionStage.None, Nothing, False, 0)
        Private _disposed As Boolean
        Private _cancelling As Integer
        Private _sourceDisposed As Boolean
        Private _started As Boolean
        Public Event Changed As Action(Of ExecutionSnapshot)

        Public Sub New(Optional reportProgress As Boolean = True)
            _token = _source.Token
            _showProgress = reportProgress
        End Sub
        Public ReadOnly Property Token As CancellationToken
            Get
                Return _token
            End Get
        End Property
        Public ReadOnly Property Snapshot As ExecutionSnapshot
            Get
                SyncLock _gate
                    Return _snapshot
                End SyncLock
            End Get
        End Property
        Public Sub Start()
            ThrowIfCancellationRequested()
            SyncLock _gate
                If _disposed Then Throw New ObjectDisposedException(NameOf(ExecutionCoordinator))
                If _snapshot.State <> ExecutionState.CancellationRequested Then
                    If _started OrElse _snapshot.State <> ExecutionState.Created Then Throw New InvalidOperationException("Execution has already started or finished.")
                    _started = True
                    Update(ExecutionState.Running, _snapshot.Stage, Nothing)
                End If
            End SyncLock
            Notify()
            ThrowIfCancellationRequested()
        End Sub
        Public Sub RequestCancellation()
            SyncLock _gate
                If _disposed OrElse _snapshot.IsTerminal OrElse _snapshot.IsCancellationRequested Then Return
                Update(ExecutionState.CancellationRequested, _snapshot.Stage, _snapshot.Percent, True)
                _cancelling += 1
            End SyncLock
            ' Token callbacks are user code too: never invoke them under our lock.
            Try
                Try
                    _source.Cancel()
                Catch ex As AggregateException
                    ' A token observer cannot replace the underlying operation's outcome.
                End Try
            Finally
                SyncLock _gate
                    _cancelling -= 1
                    DisposeSourceIfReady()
                End SyncLock
            End Try
            Notify()
        End Sub
        Public Sub ThrowIfCancellationRequested()
            Dim cancelled As Boolean
            SyncLock _gate
                If _snapshot.IsTerminal AndAlso _snapshot.State <> ExecutionState.Cancelled Then Return
                cancelled = _snapshot.IsCancellationRequested
                If cancelled AndAlso Not _snapshot.IsTerminal Then Update(ExecutionState.Cancelled, _snapshot.Stage, _snapshot.Percent)
            End SyncLock
            If cancelled Then
                Notify()
                Throw New OperationCanceledException(_token)
            End If
        End Sub
        Public Sub EnterStage(stage As ExecutionStage, Optional percent As Integer? = Nothing)
            If Not [Enum].IsDefined(GetType(ExecutionStage), stage) Then Throw New ArgumentOutOfRangeException(NameOf(stage))
            SyncLock _gate
                If _disposed OrElse _snapshot.IsTerminal OrElse Not _started Then Return
                Update(_snapshot.State, stage, NextPercent(percent))
            End SyncLock
            Notify()
        End Sub
        ''' <summary>Clamps to 0..99; never regresses. Unknown preserves prior determinate
        ''' progress. Suppressed runs always have Nothing, even on completion. Only successful
        ''' completion sets 100. Reports before start or after terminal/disposal are ignored.</summary>
        Public Sub Report(percent As Integer, Optional message As String = Nothing) Implements IProgressReporter.Report
            SyncLock _gate
                If _disposed OrElse _snapshot.IsTerminal OrElse Not _started Then Return
                Update(_snapshot.State, _snapshot.Stage, NextPercent(percent))
            End SyncLock
            Notify()
        End Sub
        Private Function NextPercent(value As Integer?) As Integer?
            If Not _showProgress Then Return Nothing
            If Not value.HasValue Then Return _snapshot.Percent
            Return Math.Max(If(_snapshot.Percent, 0), Math.Max(0, Math.Min(99, value.Value)))
        End Function
        Public Function TryComplete() As Boolean
            Return Finish(ExecutionState.Completed)
        End Function
        ''' <summary>Only an OCE carrying this requested token is cancellation. Unrelated OCEs
        ''' and ordinary exceptions remain failures, even when a request raced with failure.
        ''' Does not wrap, log, store or throw the supplied exception; hosts rethrow with Throw.</summary>
        Public Function TryFail(failure As Exception) As Boolean
            If failure Is Nothing Then Throw New ArgumentNullException(NameOf(failure))
            Dim oce = TryCast(failure, OperationCanceledException)
            Return Finish(If(oce IsNot Nothing AndAlso oce.CancellationToken = _token AndAlso Snapshot.IsCancellationRequested,
                             ExecutionState.Cancelled, ExecutionState.Failed))
        End Function
        Private Function Finish(state As ExecutionState) As Boolean
            SyncLock _gate
                If _snapshot.IsTerminal Then Return False
                If Not _started AndAlso state = ExecutionState.Completed Then Throw New InvalidOperationException("Execution has not started.")
                Update(state, _snapshot.Stage, If(state = ExecutionState.Completed AndAlso _showProgress, CType(100, Integer?), _snapshot.Percent))
            End SyncLock
            Notify()
            Return True
        End Function
        Private Sub Update(state As ExecutionState, stage As ExecutionStage, percent As Integer?, Optional requested As Boolean = False)
            _snapshot = New ExecutionSnapshot(state, stage, percent, requested OrElse _snapshot.IsCancellationRequested, _snapshot.Version + 1)
        End Sub
        Private Sub Notify()
            Dim handlers = ChangedEvent
            If handlers Is Nothing Then Return
            Dim current = Snapshot
            For Each callback As Action(Of ExecutionSnapshot) In handlers.GetInvocationList()
                Try
                    callback(current)
                Catch
                    ' UI observer failures never replace a calculation failure.
                End Try
            Next
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            RequestCancellation()
            SyncLock _gate
                _disposed = True
                DisposeSourceIfReady()
            End SyncLock
        End Sub
        Private Sub DisposeSourceIfReady()
            If _disposed AndAlso _cancelling = 0 AndAlso Not _sourceDisposed Then
                _sourceDisposed = True
                _source.Dispose()
            End If
        End Sub
    End Class
End Namespace
