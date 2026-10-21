using BESHStat.WasmSmoke.Infrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public sealed class OfficeExecutionTests
{
    private sealed class Sink : IDiagnosticSink
    {
        public List<DiagnosticRecord> Records { get; } = [];
        public Task EmitAsync(DiagnosticRecord record) { Records.Add(record); return Task.CompletedTask; }
    }
    private static DiagnosticEmitter Emitter(Sink sink) => new(sink, null, null);
    private static TaskCompletionSource<bool> Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static void Ended(Sink sink, OfficeExecutionRun run, ExecutionState state)
    {
        Assert.IsFalse(run.IsBusy);
        Assert.AreEqual(state, run.Execution.Snapshot.State);
        Assert.AreEqual(1, sink.Records.Count(x => x.EventCode == "diagnostic.run.ended"));
        Assert.AreEqual("diagnostic.run.ended", sink.Records.Last().EventCode);
        Assert.IsTrue(sink.Records.All(x => x.RunId == run.Diagnostics!.RunId));
        Assert.AreEqual(32, run.Diagnostics!.RunId.Length);
    }

    [TestMethod]
    public async Task SafeFixtureYieldsAndCancelImmediatelyPreventsLaterWork()
    {
        using var run = new OfficeExecutionRun();
        var sink = new Sink();
        var arrived = Signal(); var release = Signal();
        var yields = 0; var later = 0;
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async session =>
        {
            await session.SyntheticFixtureAsync(async () =>
            {
                yields++; arrived.TrySetResult(true); await release.Task;
            });
            later++;
        });
        await arrived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsTrue(run.CanCancel);
        run.Cancel(); run.Cancel(); release.SetResult(true);
        var ex = await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task);
        Assert.AreEqual(run.Execution.Token, ex.CancellationToken);
        Assert.AreEqual(1, yields); Assert.AreEqual(0, later);
        Ended(sink, run, ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task DefaultFixtureUsesRealTimerAndSupportsResponsiveCancel()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink();
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, s => s.SyntheticFixtureAsync());
        Assert.IsFalse(task.IsCompleted);
        await Task.Delay(20); run.Cancel();
        await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        Ended(sink, run, ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task FixtureCompletesOnlyAfterAllFiftyYields()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var yields = 0;
        await run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, s => s.SyntheticFixtureAsync(async () =>
        {
            Assert.IsTrue(s.Execution.Snapshot.Percent < 100);
            await Task.Yield(); yields++;
        }));
        Assert.AreEqual(50, yields); Assert.AreEqual(100, run.Execution.Snapshot.Percent);
        Ended(sink, run, ExecutionState.Completed);
    }

    [TestMethod]
    public async Task CancelAfterApprovalButBeforeContinuationPreventsWrite()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var writes = 0;
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            await s.ConfirmAsync(() => { s.ChooseOverwrite(true); s.Cancel(); });
            await s.WriteAsync(() => { writes++; return Task.FromResult(1); });
        }));
        Assert.AreEqual(0, writes); Ended(sink, run, ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task CancelDuringAwaitedAcquisitionPreventsComputeAndWrite()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink();
        var source = new TaskCompletionSource<int>();
        var computes = 0; var writes = 0;
        var task = run.RunAsync(Emitter(sink), AnalysisIds.KruskalWallis, true, async s =>
        {
            await s.AcquireAsync(() => source.Task);
            computes++;
            await s.WriteAsync(() => { writes++; return Task.FromResult("Completed"); });
            s.FinishOutput("Completed");
        });
        run.Cancel(); source.SetResult(1);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task);
        Assert.AreEqual(0, computes); Assert.AreEqual(0, writes);
        Ended(sink, run, ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task CancelBeforeWriteNeverCallsWriter()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var writes = 0;
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            s.Cancel();
            await s.WriteAsync(() => { writes++; return Task.FromResult(1); });
        }));
        Assert.AreEqual(0, writes); Ended(sink, run, ExecutionState.Cancelled);
    }

    [DataTestMethod] [DataRow(false)] [DataRow(true)]
    public async Task CancellationOrDisposalReleasesConsentAndCannotContinue(bool dispose)
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink();
        var shown = Signal(); var writes = 0;
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            var overwriteChoice = s.BeginConfirmation(() => shown.SetResult(true));
            var approved = await overwriteChoice.Task;
            if (approved) await s.WriteAsync(() => { writes++; return Task.FromResult(1); });
        });
        await shown.Task;
        if (dispose) run.Dispose(); else { run.Cancel(); run.Cancel(); }
        run.ChooseOverwrite(true);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(0, writes); Assert.IsFalse(run.AwaitingConsent);
        Ended(sink, run, ExecutionState.Cancelled);
    }

    [DataTestMethod] [DataRow(false)] [DataRow(true)]
    public async Task OriginalConsentChoiceReachesWriterUnchanged(bool approved)
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var writes = 0;
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            var choice = await s.ConfirmAsync(() => s.ChooseOverwrite(approved));
            Assert.AreEqual(approved, choice);
            var state = await s.WriteAsync(() => { writes++; return Task.FromResult(choice ? "Completed" : "CancelledByUser"); });
            s.FinishOutput(state);
        });
        if (approved) await task;
        else await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task);
        Assert.AreEqual(1, writes);
        Ended(sink, run, approved ? ExecutionState.Completed : ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task InFlightWriteIsNotInterruptedOrRelabelledOnDispose()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var completion = Signal();
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            await s.WriteAsync(() => completion.Task);
            s.FinishOutput("Completed");
        });
        Assert.IsFalse(run.CanCancel); run.Cancel(); run.Dispose();
        Assert.IsFalse(task.IsCompleted);
        completion.SetResult(true); await task;
        Ended(sink, run, ExecutionState.Completed);
    }

    [TestMethod]
    public async Task DisposeDuringInspectionCannotOpenAnOrphanedConsentDialog()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink(); var inspection = Signal();
        var shown = false;
        var task = run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false, async s =>
        {
            await s.WriteAsync(() => inspection.Task);
            await s.ConfirmAsync(() => shown = true);
        });
        run.Dispose(); inspection.SetResult(true);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.IsFalse(shown); Ended(sink, run, ExecutionState.Cancelled);
    }

    [TestMethod]
    public async Task FailureAndThrowingProgressObserverPreserveOriginalExceptionAndEnd()
    {
        using var run = new OfficeExecutionRun(); var sink = new Sink();
        var original = new InvalidOperationException("calculation");
        run.Execution.Changed += _ => throw new Exception("observer");
        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => run.RunAsync(Emitter(sink), AnalysisIds.OneWayAnova, false,
            _ => throw original));
        Assert.AreSame(original, ex); Ended(sink, run, ExecutionState.Failed);
        run.Cancel();
    }

    [TestMethod]
    public async Task RunsAreIndependentAndRepeatedEntryIsRejected()
    {
        using var first = new OfficeExecutionRun(); using var second = new OfficeExecutionRun();
        var firstSink = new Sink(); var secondSink = new Sink();
        await first.RunAsync(Emitter(firstSink), AnalysisIds.OneWayAnova, false, s => { s.Execution.TryComplete(); return Task.CompletedTask; });
        await second.RunAsync(Emitter(secondSink), AnalysisIds.OneWayAnova, false, s => { s.Execution.TryComplete(); return Task.CompletedTask; });
        Assert.AreNotEqual(first.Diagnostics!.RunId, second.Diagnostics!.RunId);
        first.Cancel(); Assert.IsFalse(second.Execution.Token.IsCancellationRequested);
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => first.RunAsync(Emitter(firstSink), AnalysisIds.OneWayAnova, false, _ => Task.CompletedTask));
        Ended(firstSink, first, ExecutionState.Completed); Ended(secondSink, second, ExecutionState.Completed);
    }
}
