Option Explicit On
Option Strict On

Imports System.Threading
Imports System.Threading.Tasks
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class ExecutionCoordinatorTests
    <TestMethod>
    Public Sub SuccessIsSingleTerminalAndStartIsSingleUse()
        Using run As New ExecutionCoordinator()
            Dim initial = run.Snapshot
            Assert.AreEqual(ExecutionState.Created, initial.State)
            Assert.IsFalse(initial.Percent.HasValue)
            Assert.ThrowsException(Of InvalidOperationException)(Sub() run.TryComplete())
            run.Start()
            Assert.ThrowsException(Of InvalidOperationException)(Sub() run.Start())
            Assert.IsTrue(run.TryComplete())
            Assert.AreEqual(100, run.Snapshot.Percent.Value)
            Assert.IsFalse(run.TryComplete())
            Assert.IsFalse(run.TryFail(New Exception()))
            run.RequestCancellation()
            run.Report(4, "private input")
            run.EnterStage(ExecutionStage.Output)
            Assert.AreEqual(ExecutionState.Completed, run.Snapshot.State)
            Assert.IsFalse(run.Token.IsCancellationRequested)
            Assert.AreEqual(ExecutionState.Created, initial.State)
            Assert.ThrowsException(Of InvalidOperationException)(Sub() run.Start())
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub CancellationIsOnlyTerminalAtCheckpoint(started As Boolean)
        Using run As New ExecutionCoordinator()
            If started Then run.Start()
            run.RequestCancellation()
            Assert.AreEqual(ExecutionState.CancellationRequested, run.Snapshot.State)
            Assert.IsTrue(run.Token.IsCancellationRequested)
            Dim failure = Assert.ThrowsException(Of OperationCanceledException)(Sub()
                                                                                  If started Then
                                                                                      run.ThrowIfCancellationRequested()
                                                                                  Else
                                                                                      run.Start()
                                                                                  End If
                                                                              End Sub)
            Assert.AreEqual(run.Token, failure.CancellationToken)
            Assert.AreEqual(ExecutionState.Cancelled, run.Snapshot.State)
            Assert.IsFalse(run.TryComplete())
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub FailureRemainsFailureDespiteRequest(requested As Boolean)
        Using run As New ExecutionCoordinator()
            run.Start()
            If requested Then run.RequestCancellation()
            Assert.IsTrue(run.TryFail(New InvalidOperationException("original")))
            run.RequestCancellation()
            run.Dispose()
            Assert.AreEqual(ExecutionState.Failed, run.Snapshot.State)
            Assert.IsFalse(run.TryComplete())
        End Using
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub OnlyRequestedOwnTokenOceMeansCancellation(kind As Integer)
        Using run As New ExecutionCoordinator(), other As New CancellationTokenSource()
            run.Start()
            run.RequestCancellation()
            Dim token = If(kind = 0, run.Token, If(kind = 1, other.Token, CancellationToken.None))
            Dim failure As New OperationCanceledException(token)
            run.TryFail(failure)
            Assert.AreEqual(If(kind = 0, ExecutionState.Cancelled, ExecutionState.Failed), run.Snapshot.State)
        End Using
    End Sub

    <TestMethod>
    Public Sub LateRequestCannotRelabelCompletedWrite()
        Using run As New ExecutionCoordinator()
            run.Start()
            run.RequestCancellation()
            Assert.IsTrue(run.TryComplete())
            run.ThrowIfCancellationRequested()
            Assert.AreEqual(ExecutionState.Completed, run.Snapshot.State)
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub BoundedMonotonicProgressAndMessageSuppression(suppressed As Boolean)
        Using run As New ExecutionCoordinator(Not suppressed)
            run.Report(80)
            Assert.IsFalse(run.Snapshot.Percent.HasValue)
            run.Start()
            Dim reporter As IProgressReporter = run
            For Each value In New Integer() {-12, 1, 65, 4, 101, Integer.MaxValue}
                Dim prior = If(run.Snapshot.Percent, 0)
                reporter.Report(value, "=SECRET(Payroll!A1)")
                If suppressed Then
                    Assert.IsFalse(run.Snapshot.Percent.HasValue)
                Else
                    Assert.IsTrue(run.Snapshot.Percent >= prior AndAlso run.Snapshot.Percent <= 99)
                End If
            Next
            Dim before = run.Snapshot
            run.EnterStage(ExecutionStage.Output)
            Assert.AreEqual(before.Percent, run.Snapshot.Percent)
            run.TryComplete()
            Assert.AreEqual(Not suppressed, run.Snapshot.Percent.HasValue)
            Assert.AreEqual(ExecutionStage.None, before.Stage)
        End Using
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub DisposalRequestsButDoesNotInventCompletion(started As Boolean)
        Dim run As New ExecutionCoordinator()
        If started Then run.Start()
        Dim token = run.Token
        run.Dispose()
        run.Dispose()
        run.RequestCancellation()
        run.Report(90)
        Assert.AreEqual(ExecutionState.CancellationRequested, run.Snapshot.State)
        Assert.IsTrue(token.IsCancellationRequested)
        Assert.ThrowsException(Of OperationCanceledException)(Sub() run.ThrowIfCancellationRequested())
        Assert.AreEqual(ExecutionState.Cancelled, run.Snapshot.State)
    End Sub

    <TestMethod>
    Public Async Function ParallelCancelAndCrossRunIsolation() As Task
        Using first As New ExecutionCoordinator(), second As New ExecutionCoordinator()
            first.Start() : second.Start()
            Await Task.WhenAll(Enumerable.Range(0, 32).Select(Function(i) Task.Run(Sub() first.RequestCancellation())))
            Assert.IsTrue(first.Token.IsCancellationRequested)
            Assert.IsFalse(second.Token.IsCancellationRequested)
            Assert.AreEqual(ExecutionState.Running, second.Snapshot.State)
            second.TryComplete()
        End Using
    End Function

    <TestMethod>
    Public Async Function ObserversAreOutsideLockReentrantAndFailureIsPreserved() As Task
        Using run As New ExecutionCoordinator()
            Dim observed As Integer
            Dim locked As Boolean
            AddHandler run.Changed, Sub(snapshot)
                                        Dim read = Task.Run(Function() run.Snapshot)
                                        If Not read.Wait(2000) Then locked = True
                                        Interlocked.Increment(observed)
                                    End Sub
            AddHandler run.Changed, Sub(snapshot)
                                        If snapshot.State = ExecutionState.Running Then run.RequestCancellation()
                                        Throw New Exception("broken UI")
                                    End Sub
            Dim original As New InvalidOperationException("calculation failure")
            Dim caught = Await Assert.ThrowsExceptionAsync(Of InvalidOperationException)(Async Function()
                                                                                            Await Task.Yield()
                                                                                            Try
                                                                                                run.Start()
                                                                                            Catch ex As OperationCanceledException
                                                                                                ' This observer's request was observed at Start's checkpoint.
                                                                                            End Try
                                                                                            Throw original
                                                                                        End Function)
            Assert.AreSame(original, caught)
            Assert.IsTrue(observed > 0)
            Assert.IsFalse(locked, "Observer executed under lock")
        End Using
    End Function

    <TestMethod>
    Public Sub ThrowingTokenCallbackCannotReplaceOutcome()
        Using run As New ExecutionCoordinator()
            run.Start()
            Using registration = run.Token.Register(Sub() Throw New Exception("observer"))
                run.RequestCancellation()
                run.TryFail(New ArgumentException("calculation"))
                Assert.AreEqual(ExecutionState.Failed, run.Snapshot.State)
            End Using
        End Using
    End Sub

    <TestMethod>
    Public Async Function BoundedCooperativeAsyncFixtureStopsBetweenStages() As Task
        Using run As New ExecutionCoordinator()
            Dim arrived As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            Dim release As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
            Dim subsequentWork As Integer
            Dim fixture = Async Function() As Task
                              run.Start()
                              For i As Integer = 0 To 49
                                  run.Report(i)
                                  If i = 2 Then
                                      arrived.SetResult(True)
                                      Await release.Task
                                  End If
                                  Await Task.Delay(1, run.Token)
                                  run.ThrowIfCancellationRequested()
                              Next
                              subsequentWork += 1
                          End Function
            Dim pending = fixture()
            Await arrived.Task.WaitAsync(TimeSpan.FromSeconds(5))
            run.RequestCancellation() : release.SetResult(True)
            Dim ex = Await Assert.ThrowsExceptionAsync(Of TaskCanceledException)(Function() pending)
            run.TryFail(ex)
            Assert.AreEqual(0, subsequentWork)
            Assert.AreEqual(ExecutionState.Cancelled, run.Snapshot.State)
        End Using
    End Function

    <TestMethod>
    Public Async Function ConcurrentFinishAndDisposeHasOneTerminalWinner() As Task
        For iteration As Integer = 1 To 30
            Dim run As New ExecutionCoordinator()
            run.Start()
            Await Task.WhenAll(Task.Run(Sub() run.TryComplete()), Task.Run(Sub() run.TryFail(New Exception())), Task.Run(Sub() run.Dispose()))
            Assert.IsTrue(run.Snapshot.IsTerminal)
            Dim final = run.Snapshot
            run.RequestCancellation() : run.Report(50)
            Assert.AreSame(final, run.Snapshot)
        Next
    End Function

    <TestMethod>
    Public Sub ThrowingProgressObserverCannotReplaceCalculationFailure()
        Using run As New ExecutionCoordinator()
            AddHandler run.Changed, Sub(snapshot) Throw New ArgumentException("UI")
            run.Start()
            Dim original As New InvalidOperationException("calculation")
            Dim caught = Assert.ThrowsException(Of InvalidOperationException)(Sub()
                                                                                  Try
                                                                                      run.Report(35, "private")
                                                                                      Throw original
                                                                                  Catch ex As Exception
                                                                                      run.TryFail(ex)
                                                                                      Throw
                                                                                  End Try
                                                                              End Sub)
            Assert.AreSame(original, caught)
            Assert.AreEqual(ExecutionState.Failed, run.Snapshot.State)
        End Using
    End Sub

    <TestMethod>
    Public Async Function DiagnosticCorrelationIsHostOwnedAndIndependent() As Task
        Dim emitter As New DiagnosticEmitter(Nothing)
        Dim diagnostic1 = Await emitter.StartAsync(AnalysisIds.OneWayAnova, False)
        Dim diagnostic2 = Await emitter.StartAsync(AnalysisIds.KruskalWallis, False)
        Using run1 As New ExecutionCoordinator(), run2 As New ExecutionCoordinator()
            run1.Start() : run2.Start()
            run1.RequestCancellation()
            run2.TryComplete()
            Assert.AreNotEqual(diagnostic1.RunId, diagnostic2.RunId)
            Assert.AreEqual(32, diagnostic1.RunId.Length)
            Assert.AreEqual(ExecutionState.CancellationRequested, run1.Snapshot.State)
            Assert.AreEqual(ExecutionState.Completed, run2.Snapshot.State)
        End Using
        Await diagnostic1.EndAsync() : Await diagnostic2.EndAsync()
    End Function
End Class
