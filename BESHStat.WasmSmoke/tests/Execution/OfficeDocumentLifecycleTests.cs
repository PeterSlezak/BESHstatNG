using BESHStat.WasmSmoke.Infrastructure;
using BESHStatNG.ExcelCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BESHStat.WasmSmoke.Tests;

[TestClass]
public class OfficeDocumentLifecycleTests
{
    private sealed class Sink : IDiagnosticSink
    {
        public Task EmitAsync(DiagnosticRecord record) => Task.CompletedTask;
    }
    private static OfficeDocumentLifecycle Scope(string handle, Func<string, Task>? release = null) =>
        new(new OfficeLifecycleBinding { Handle = handle, ContextIdentity = "verified-host-context",
            WorksheetIdentity = "native-sheet", WorksheetName = "Data" }, release ?? (_ => Task.CompletedTask));

    [TestMethod]
    public async Task DisposalIsIdempotentIgnoresLateCallbacksAndCannotInvalidateAnotherScope()
    {
        var releases = 0;
        var first = Scope("first", _ => { releases++; return Task.CompletedTask; });
        await using var second = Scope("second");
        Assert.IsFalse(first.NotifyInvalidated("second"));
        second.EnsureCurrent();
        await first.DisposeAsync(); await first.DisposeAsync();
        Assert.AreEqual(1, releases);
        Assert.IsFalse(first.NotifyInvalidated("first"));
        Assert.ThrowsException<DocumentLifecycleException>(first.EnsureCurrent);
        second.EnsureCurrent();
    }

    [TestMethod]
    public async Task SelectionChangeWhileAcquiringRejectsBeforeOutputAndCleansUp()
    {
        using var run = new OfficeExecutionRun();
        var release = new TaskCompletionSource<bool>(); var acquired = new TaskCompletionSource<bool>();
        var cleanups = 0; var writes = 0;
        run.Lifecycle = Scope("acquiring", _ => { cleanups++; return Task.CompletedTask; });
        var task = run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.OneWayAnova, false, async session =>
        {
            await session.AcquireAsync(async () => { acquired.SetResult(true); await release.Task; return 1; });
            await session.WriteAsync(() => { writes++; return Task.FromResult("Completed"); });
        });
        await acquired.Task;
        Assert.IsTrue(run.Lifecycle.NotifyInvalidated("acquiring"));
        Assert.IsFalse(run.Lifecycle.NotifyInvalidated("acquiring"));
        release.SetResult(true);
        await Assert.ThrowsExceptionAsync<DocumentLifecycleException>(() => task);
        Assert.AreEqual(0, writes); Assert.AreEqual(1, cleanups);
        Assert.AreEqual(ExecutionState.Failed, run.Execution.Snapshot.State);
    }

    [TestMethod]
    public async Task InvalidatedConsentIsReleasedAndNeverStartsAWrite()
    {
        using var run = new OfficeExecutionRun();
        run.Lifecycle = Scope("consent"); var shown = new TaskCompletionSource<bool>(); var writes = 0;
        var task = run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.KruskalWallis, false, async session =>
        {
            var approved = await session.ConfirmAsync(() => shown.SetResult(true));
            await session.WriteAsync(() => { writes++; return Task.FromResult("Completed"); });
            session.FinishOutput("Completed");
        });
        await shown.Task;
        Assert.IsTrue(run.NotifyLifecycleChanged("consent"));
        await Assert.ThrowsExceptionAsync<DocumentLifecycleException>(() => task);
        Assert.AreEqual(0, writes);
    }

    [DataTestMethod] [DataRow("Completed")] [DataRow("PartiallyWritten")]
    public async Task DisposeAndLateSelectionDuringActiveWriteRetainActualWriterOutcome(string state)
    {
        using var run = new OfficeExecutionRun();
        var writing = new TaskCompletionSource<bool>(); var release = new TaskCompletionSource<bool>();
        var cleanups = 0;
        run.Lifecycle = Scope("writing", _ => { cleanups++; return Task.CompletedTask; });
        var task = run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.OneWayAnova, false, async session =>
        {
            var actual = await session.WriteAsync(async () => { writing.SetResult(true); await release.Task; return state; });
            session.FinishOutput(actual);
        });
        await writing.Task;
        run.Dispose(); run.Cancel();
        await run.Lifecycle.DisposeAsync();
        Assert.IsFalse(run.Lifecycle.NotifyInvalidated("writing"));
        release.SetResult(true); await task;
        Assert.AreEqual(state == "Completed" ? ExecutionState.Completed : ExecutionState.Failed, run.Execution.Snapshot.State);
        Assert.AreEqual(1, cleanups);
    }

    [TestMethod]
    public async Task CancelWhileAwaitingConsentCleansLeaseAndNewRunGetsFreshBinding()
    {
        using var run = new OfficeExecutionRun();
        run.Lifecycle = Scope("old"); var shown = new TaskCompletionSource<bool>();
        var task = run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.OneWayAnova, false,
            async session => { await session.ConfirmAsync(() => shown.SetResult(true)); });
        await shown.Task; run.Dispose();
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => task);
        await using var fresh = Scope("fresh");
        fresh.EnsureCurrent(); Assert.IsFalse(fresh.NotifyInvalidated("old"));
    }

    [TestMethod]
    public async Task CleanupFailureDoesNotEscapeAndUnverifiedIdentityCannotBind()
    {
        var scope = Scope("safe", _ => throw new InvalidOperationException("disconnected"));
        await scope.DisposeAsync();
        Assert.ThrowsException<DocumentLifecycleException>(scope.EnsureCurrent);
        Assert.ThrowsException<DocumentLifecycleException>(() => new OfficeDocumentLifecycle(
            new OfficeLifecycleBinding { Handle = "unverified", WorksheetIdentity = "id", WorksheetName = "Data" }, _ => Task.CompletedTask));
    }

    [TestMethod]
    public async Task CompletedWriterIgnoresLaterLifecycleNotification()
    {
        using var run = new OfficeExecutionRun();
        run.Lifecycle = Scope("completed");
        await run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.OneWayAnova, false, session =>
        {
            session.FinishOutput("Completed");
            Assert.IsFalse(session.NotifyLifecycleChanged("completed"));
            Assert.AreEqual(ExecutionState.Completed, session.Execution.Snapshot.State);
            return Task.CompletedTask;
        });
    }

    [TestMethod]
    public async Task CompletedDisposedRunCannotStartAnotherHostDelegate()
    {
        using var run = new OfficeExecutionRun(); var writes = 0;
        await run.RunAsync(new DiagnosticEmitter(new Sink()), AnalysisIds.OneWayAnova, false, session =>
        {
            session.FinishOutput("Completed"); return Task.CompletedTask;
        });
        run.Dispose();
        await Assert.ThrowsExceptionAsync<DocumentLifecycleException>(() => run.WriteAsync(() =>
        {
            writes++; return Task.FromResult("Completed");
        }));
        Assert.AreEqual(0, writes);
        Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State);
    }
}
