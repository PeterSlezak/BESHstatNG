using BESHStatNG.ExcelCommon;

namespace BESHStat.WasmSmoke.Infrastructure;

// Prototype-only orchestration. No Task.Run, worksheet data, ambient state or JS aborts.
public sealed class OfficeExecutionRun : IDisposable
{
    public ExecutionCoordinator Execution { get; } = new();
    public OfficeDocumentLifecycle? Lifecycle { get; set; }
    public DiagnosticRunContext? Diagnostics { get; private set; }
    public bool IsBusy { get; private set; } = true;
    public bool CanCancel => IsBusy && !writing && !Execution.Snapshot.IsTerminal &&
        !Execution.Snapshot.IsCancellationRequested;
    private bool writing;
    private bool entered;
    private bool disposed;
    private TaskCompletionSource<bool>? consent;

    public void Cancel()
    {
        if (!IsBusy || writing) return;
        Execution.RequestCancellation();
        consent?.TrySetResult(false); // Always release the separate dialog await.
    }

    public async Task RunAsync(DiagnosticEmitter emitter, string operation, bool trace,
        Func<OfficeExecutionRun, Task> body)
    {
        if (entered) throw new InvalidOperationException("Execution already entered.");
        entered = true;
        try
        {
            Diagnostics = await emitter.StartAsync(operation, trace);
            Execution.Start();
            await body(this);
            if (!Execution.Snapshot.IsTerminal)
                throw new InvalidOperationException("Execution returned without a terminal outcome.");
        }
        catch (Exception ex)
        {
            Execution.TryFail(ex);
            if (Diagnostics is not null && Execution.Snapshot.State == ExecutionState.Failed)
            {
                var stage = Execution.Snapshot.Stage switch {
                    ExecutionStage.Acquisition => "acquisition", ExecutionStage.Analysis => "analysis",
                    ExecutionStage.Output or ExecutionStage.Confirmation => "output", _ => "validation" };
                var rejected = stage != "output" && ex is SpreadsheetInputValidationException;
                await Diagnostics.ReportAsync(rejected ? "diagnostic.validation.rejected" : "diagnostic.run.failed",
                    stage, new Dictionary<string, string> { ["category"] = rejected ? "input" : "unexpected" });
            }
            throw; // Preserve the original exception and cancellation token.
        }
        finally
        {
            consent?.TrySetResult(false);
            consent = null;
            try { if (Diagnostics is not null) await Diagnostics.EndAsync(); }
            finally
            {
                if (Lifecycle is not null) await Lifecycle.DisposeAsync();
                IsBusy = false;
                Execution.Dispose();
            }
        }
    }

    public async Task<T> AcquireAsync<T>(Func<Task<T>> acquire)
    {
        Checkpoint(ExecutionStage.Acquisition, 10);
        var result = await acquire(); // Do not claim to interrupt the host transaction.
        Execution.ThrowIfCancellationRequested();
        Lifecycle?.EnsureCurrent();
        return result;
    }

    public void Checkpoint(ExecutionStage stage, int? percent = null)
    {
        if (disposed) Execution.RequestCancellation();
        Execution.ThrowIfCancellationRequested();
        if (disposed || Execution.Snapshot.IsTerminal) throw new DocumentLifecycleException(DocumentInvalidationReason.ContextDisposed);
        Lifecycle?.EnsureCurrent();
        Execution.EnterStage(stage, percent);
    }

    public async Task<T> WriteAsync<T>(Func<Task<T>> write)
    {
        Checkpoint(ExecutionStage.Output, 90);
        writing = true; // Cancel is unavailable during an Excel.js transaction.
        try { return await write(); }
        finally { writing = false; }
    }

    public async Task<bool> ConfirmAsync(Action showDialog)
    {
        Checkpoint(ExecutionStage.Confirmation);
        consent = new(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            showDialog();
            var approved = await consent.Task;
            Execution.ThrowIfCancellationRequested();
            return approved;
        }
        finally { consent = null; }
    }
    public sealed record ConsentWait(Task<bool> Task);
    public ConsentWait BeginConfirmation(Action showDialog) => new(ConfirmAsync(showDialog));
    public bool AwaitingConsent => consent is not null;
    public void ChooseOverwrite(bool approved) => consent?.TrySetResult(approved);
    public bool NotifyLifecycleChanged(string handle)
    {
        if (disposed || Execution.Snapshot.IsTerminal || Lifecycle?.NotifyInvalidated(handle) != true) return false;
        consent?.TrySetResult(false);
        return true;
    }

    public void FinishOutput(string state)
    {
        if (state == "Completed") Execution.TryComplete();
        else if (state == "CancelledByUser")
        {
            Execution.RequestCancellation();
            Execution.ThrowIfCancellationRequested();
        }
        else Execution.TryFail(new InvalidOperationException("Output did not complete."));
    }

    // Real asynchronous browser yields, bounded to ~5 seconds; never reads/writes a sheet.
    public async Task SyntheticFixtureAsync(Func<Task>? yield = null)
    {
        for (var step = 0; step < 50; step++)
        {
            Checkpoint(ExecutionStage.SyntheticFixture, step * 2);
            if (yield is null) await Task.Delay(100, Execution.Token);
            else await yield();
            Execution.ThrowIfCancellationRequested();
        }
        Execution.TryComplete();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        // A write already in flight must report its actual outcome; no rollback promise.
        if (!writing) Cancel();
        consent?.TrySetResult(false);
        if (!entered) { IsBusy = false; Execution.Dispose(); }
    }
}
