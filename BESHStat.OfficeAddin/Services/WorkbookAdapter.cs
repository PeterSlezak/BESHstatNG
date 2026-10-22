using BESHStat.OfficeAddin.Infrastructure;
using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

// Pane-scoped service, operation-owned budgets/leases. Readiness is borrowed,
// never disposed here. No cancellation returns a partially copied snapshot.
public sealed class WorkbookAdapter : IWorkbookAdapter
{
    private readonly CancellationTokenSource lifetime = new();
    private readonly CancellationToken lifetimeToken;
    private readonly IOfficeHostReadiness readiness;
    private readonly IWorkbookTransport transport;
    private readonly WorkbookDiagnostics? diagnostics;
    private readonly SemaphoreSlim operation = new(1, 1);
    private bool disposed;
    public WorkbookAdapter(IOfficeHostReadiness readiness, IWorkbookTransport transport, WorkbookDiagnostics? diagnostics = null)
    {
        this.readiness = readiness;
        this.transport = transport;
        this.diagnostics = diagnostics;
        lifetimeToken = lifetime.Token;
    }
    private static readonly HashSet<string> SafeCodes = ["office.unavailable", "office.unsupported-api", "office.unsupported-host",
        "office.timeout", "office.initialization-failed", "user.cancelled", "user.input-invalid", "user.document-changed", "host.busy",
        "input.range-cells", "input.total-cells", "input.estimated-bytes", "input.time-budget", "input.dimensions", "input.invalid-selection",
        "host.operation-failed", "host.invalid-response", "host.interop-import-failed", "host.interop-create-failed", "host.interop-invoke-failed"];
    private static string Safe(string? code) => code is not null && SafeCodes.Contains(code) ? code : "host.invalid-response";

    public async Task<WorkbookResult<HostCapabilitySnapshot>> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        if (disposed || cancellationToken.IsCancellationRequested) return new("user.cancelled");
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken, cancellationToken);
        bound.CancelAfter(TimeSpan.FromSeconds(12));
        try
        {
            var status = await readiness.InitializeAsync(bound.Token).WaitAsync(bound.Token);
            if (disposed || cancellationToken.IsCancellationRequested) return new("user.cancelled");
            if (!status.IsExcel || status.Code != "office.excel-ready") return new(Safe(status.Code));
            return new("workbook.ok", new HostCapabilitySnapshot { Host = "OfficeJs", Platform = status.Platform,
                IsExcel = true, IsSpreadsheetHost = true, Capabilities = [HostCapabilityIds.ReadSelectedRange] });
        }
        catch (OperationCanceledException) { return new(cancellationToken.IsCancellationRequested || lifetimeToken.IsCancellationRequested ? "user.cancelled" : "office.timeout"); }
        catch (Exception) { return new("host.operation-failed"); }
    }

    public async Task<WorkbookResult<SelectedRangeMetadata>> InspectSelectionAsync(CancellationToken cancellationToken = default)
    {
        var result = await AcquireAsync(null, new InputSizeLimits(), true, cancellationToken);
        return result.Succeeded ? new("workbook.ok", new(result.Value!.RowCount, result.Value.ColumnCount, result.Value.ToReference())) : new(result.Code);
    }
    public Task<WorkbookResult<SpreadsheetRangeData>> ReadSelectionAsync(InputSizeLimits? limits = null, CancellationToken cancellationToken = default)
        => AcquireAsync(null, limits ?? new InputSizeLimits(), false, cancellationToken);
    public Task<WorkbookResult<SpreadsheetRangeData>> ReadRangeAsync(SpreadsheetRangeReference reference, InputSizeLimits? limits = null, CancellationToken cancellationToken = default)
        => reference is null ? Task.FromResult(new WorkbookResult<SpreadsheetRangeData>("user.input-invalid")) : AcquireAsync(reference, limits ?? new InputSizeLimits(), false, cancellationToken);

    private static void ValidatePolicy(InputSizeLimits limits)
    {
        var cap = new InputSizeLimits();
        if (limits.MaxRangeCells > cap.MaxRangeCells || limits.MaxTotalCells > cap.MaxTotalCells || limits.MaxRanges > cap.MaxRanges ||
            limits.MaxChunkCells > cap.MaxChunkCells || limits.MaxEstimatedBytes > cap.MaxEstimatedBytes || limits.EstimatedBytesPerCell != cap.EstimatedBytesPerCell ||
            limits.TimeBudgetMilliseconds > cap.TimeBudgetMilliseconds) throw new ArgumentException();
    }
    private static SpreadsheetRangeReference CopyReference(SpreadsheetRangeReference reference)
    {
        if (reference.AddressDialect != SpreadsheetAddressDialect.ExcelA1 || !Bounded(reference.WorksheetId, 1024) ||
            !Bounded(reference.WorksheetName, 256) || reference.Address is null || reference.Address.Length > 40 ||
            !string.IsNullOrEmpty(reference.WorkbookId) || !string.IsNullOrEmpty(reference.WorkbookName) ||
            !string.IsNullOrEmpty(reference.SelectionId) || !string.IsNullOrEmpty(reference.SelectionRevision)) throw new ArgumentException();
        var copy = SpreadsheetRangeReference.FromAddress(reference.Address, reference.WorksheetName);
        if (copy.StartRow != reference.StartRow || copy.StartColumn != reference.StartColumn || copy.RowCount != reference.RowCount || copy.ColumnCount != reference.ColumnCount)
            throw new ArgumentException();
        copy.WorksheetId = reference.WorksheetId;
        return copy;
    }
    private static void ValidateMetadata(RangeMetadata? metadata)
    {
        if (metadata is null || !Bounded(metadata.WorksheetId, 1024) || !Bounded(metadata.WorksheetName, 256) || metadata.Address is null || metadata.Address.Length > 40)
            throw new InvalidDataException();
        var parsed = SpreadsheetRangeAddress.Parse(metadata.Address, metadata.WorksheetName);
        if (parsed.WorkbookName.Length != 0 || parsed.Address != metadata.Address || parsed.StartRow != metadata.StartRow ||
            parsed.StartColumn != metadata.StartColumn || parsed.RowCount != metadata.RowCount || parsed.ColumnCount != metadata.ColumnCount) throw new InvalidDataException();
    }
    private static bool Bounded(string? text, int maximum) => !string.IsNullOrWhiteSpace(text) && text.Length <= maximum && !text.Any(char.IsControl);
    private static DocumentLifecycleSnapshot Snapshot(RangeMetadata metadata, string id) => new("", metadata.WorksheetId!, metadata.WorksheetName!,
        new OutputRectangle(metadata.StartRow, metadata.StartColumn, metadata.StartRow + metadata.RowCount - 1, metadata.StartColumn + metadata.ColumnCount - 1), contextIdentity: id);
    private static void CheckReply(ReadReply? reply)
    {
        if (reply?.Code != "workbook.ok") throw new WorkbookTransportException(Safe(reply?.Code));
        try { ValidateMetadata(reply.Metadata); } catch (Exception) { throw new InvalidDataException(); }
    }
    private static SpreadsheetCellValue CopyCell(CellReply? cell)
    {
        if (cell is null || !Enum.IsDefined(cell.Kind) || !double.IsFinite(cell.NumberValue) || cell.TextValue is null ||
            cell.TextValue.Length > 32767 || cell.NumberFormat is null || cell.NumberFormat.Length > 32767 ||
            cell.NativeErrorCode is null || cell.NativeErrorCode.Length != 0 ||
            (cell.Kind != SpreadsheetCellKind.Number && cell.NumberValue != 0) ||
            (cell.Kind is not (SpreadsheetCellKind.Text or SpreadsheetCellKind.CellError) && cell.TextValue.Length != 0) ||
            (cell.Kind != SpreadsheetCellKind.Bool && cell.BooleanValue)) throw new InvalidDataException();
        return new SpreadsheetCellValue { Kind = cell.Kind, NumberValue = cell.NumberValue, TextValue = cell.TextValue,
            BooleanValue = cell.BooleanValue, NumberFormat = cell.NumberFormat, NativeErrorCode = cell.NativeErrorCode };
    }

    private async Task<WorkbookResult<SpreadsheetRangeData>> AcquireAsync(SpreadsheetRangeReference? reference, InputSizeLimits limits, bool metadataOnly, CancellationToken cancellationToken)
    {
        if (disposed || cancellationToken.IsCancellationRequested) return new("user.cancelled");
        if (!await operation.WaitAsync(0)) return new("host.busy");
        if (disposed) { operation.Release(); return new("user.cancelled"); }
        var id = Guid.NewGuid().ToString("N"); // operation handle, never a workbook/selection identity
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken, cancellationToken);
        bound.CancelAfter(TimeSpan.FromMilliseconds(limits.TimeBudgetMilliseconds));
        diagnostics?.Record(metadataOnly ? "cs.metadata-started" : "cs.read-started");
        try
        {
            ValidatePolicy(limits);
            reference = reference is null ? null : CopyReference(reference);
            var budget = new InputAcquisitionBudget(limits, bound.Token);
            diagnostics?.Record("cs.capabilities-started");
            var host = await GetCapabilitiesAsync(bound.Token);
            diagnostics?.Record("cs.capabilities-completed");
            budget.Checkpoint();
            if (!host.Succeeded) return new(host.Code);
            diagnostics?.Record("cs.begin-started");
            var begin = await transport.BeginReadAsync(id, limits, reference, metadataOnly, bound.Token).WaitAsync(bound.Token);
            diagnostics?.Record("cs.begin-completed");
            CheckReply(begin);
            var received = begin!.Metadata!;
            var meta = new RangeMetadata { WorksheetId = received.WorksheetId, WorksheetName = received.WorksheetName, Address = received.Address,
                StartRow = received.StartRow, StartColumn = received.StartColumn, RowCount = received.RowCount, ColumnCount = received.ColumnCount };
            budget.AddRange(meta.RowCount, meta.ColumnCount);
            if (reference is not null && (reference.WorksheetId != meta.WorksheetId || reference.WorksheetName != meta.WorksheetName || reference.Address != meta.Address))
                return new("user.document-changed");
            using var lease = new DocumentLifecycleLease(Snapshot(meta, id));
            var rows = new List<List<SpreadsheetCellValue>>();
            double actualEstimate = 0;
            if (!metadataOnly)
            {
                for (int r = 0; r < meta.RowCount; r++) rows.Add(Enumerable.Repeat<SpreadsheetCellValue>(null!, meta.ColumnCount).ToList());
                int row = 0, col = 0;
                while (row < meta.RowCount)
                {
                    budget.Checkpoint();
                    diagnostics?.Record("cs.tile-started");
                    var chunk = await transport.ReadNextAsync(id, bound.Token).WaitAsync(bound.Token);
                    diagnostics?.Record("cs.tile-completed");
                    CheckReply(chunk);
                    lease.EnsureCurrent(Snapshot(chunk!.Metadata!, id));
                    int width = Math.Min(meta.ColumnCount, limits.MaxChunkCells);
                    int height = Math.Max(1, limits.MaxChunkCells / width);
                    int tileRows = Math.Min(height, meta.RowCount - row), tileCols = Math.Min(width, meta.ColumnCount - col);
                    if (chunk.RowOffset != row || chunk.ColumnOffset != col || chunk.Rows?.Count != tileRows || chunk.Rows.Any(cells => cells is null || cells.Count != tileCols))
                        throw new InvalidDataException();
                    for (int r = 0; r < tileRows; r++)
                        for (int c = 0; c < tileCols; c++)
                        {
                            var cell = CopyCell(chunk.Rows[r][c]);
                            actualEstimate += limits.EstimatedBytesPerCell + 2d * (cell.TextValue.Length + cell.NumberFormat.Length);
                            if (actualEstimate > limits.MaxEstimatedBytes) return new("input.estimated-bytes");
                            rows[row + r][col + c] = cell;
                        }
                    col += width;
                    if (col >= meta.ColumnCount) { col = 0; row += height; }
                    if (chunk.Done != (row >= meta.RowCount)) throw new InvalidDataException();
                }
            }
            diagnostics?.Record("cs.finish-started");
            var finish = await transport.FinishReadAsync(id, bound.Token).WaitAsync(bound.Token);
            diagnostics?.Record("cs.finish-completed");
            CheckReply(finish);
            lease.EnsureCurrent(Snapshot(finish!.Metadata!, id));
            budget.Checkpoint();
            lease.MarkInputCopied();
            diagnostics?.Record("cs.read-completed");
            return new("workbook.ok", new SpreadsheetRangeData { WorksheetId = meta.WorksheetId, WorksheetName = meta.WorksheetName,
                Address = meta.Address, StartRow = meta.StartRow, StartColumn = meta.StartColumn, RowCount = meta.RowCount, ColumnCount = meta.ColumnCount,
                Rows = rows, SelectionId = "", SelectionRevision = "" });
        }
        catch (OperationCanceledException)
        {
            var cancelled = cancellationToken.IsCancellationRequested || lifetimeToken.IsCancellationRequested;
            diagnostics?.Record(cancelled ? "cs.cancelled" : "cs.timeout");
            return new(cancelled ? "user.cancelled" : "input.time-budget");
        }
        catch (DocumentLifecycleException) { return new("user.document-changed"); }
        catch (SpreadsheetInputValidationException ex)
        {
            var code = Safe(ex.Report.Issues[0].Code);
            diagnostics?.Record(code == "input.time-budget" ? "cs.timeout" : "cs.read-rejected");
            return new(code);
        }
        catch (WorkbookTransportException ex)
        {
            diagnostics?.Record(ex.Code == "input.time-budget" ? "cs.js-timeout-received" : "cs.read-rejected");
            return new(Safe(ex.Code));
        }
        catch (ArgumentException) { return new("user.input-invalid"); }
        catch (Exception) { return new("host.invalid-response"); }
        finally
        {
            diagnostics?.Record("cs.cleanup-started");
            try { await transport.CancelAsync(id).WaitAsync(TimeSpan.FromSeconds(2)); } catch (Exception) { }
            diagnostics?.Record("cs.cleanup-completed");
            operation.Release();
        }
    }

    public async Task<WriteResult> WriteAsync(OfficeOutputPlan plan, WriteIntent intent, WriteConsent? consent = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        // The production transport is fail-closed. This typed seam also projects
        // fake transport outcomes without losing completion/mutation precedence.
        if (disposed || cancellationToken.IsCancellationRequested) return Rejected(intent, UserFaultCodes.Cancelled);
        if (!await operation.WaitAsync(0)) return Rejected(intent, WriteCodes.Rejected);
        if (disposed) { operation.Release(); return Rejected(intent, UserFaultCodes.Cancelled); }
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken, cancellationToken);
        bound.CancelAfter(TimeSpan.FromSeconds(30));
        var invoked = false;
        try
        {
            if (plan is null || plan.Target.Destination != (int)intent.Destination || plan.Target.WorksheetName != intent.WorksheetName ||
                plan.Target.StartRow != intent.StartRow || plan.Target.StartColumn != intent.StartColumn) return Rejected(intent, WriteCodes.Rejected);
            if (plan.Tables.Count != intent.Regions.Count || plan.Tables.Where((table, index) => {
                var region = intent.Regions[index];
                return region.ArtifactId != table.ArtifactId || !region.MayMutate || region.Bounds is null ||
                    region.Bounds.StartRow != table.Bounds.StartRow || region.Bounds.StartColumn != table.Bounds.StartColumn ||
                    region.Bounds.EndRow != table.Bounds.EndRow || region.Bounds.EndColumn != table.Bounds.EndColumn;
            }).Any()) return Rejected(intent, WriteCodes.Rejected);
            var host = await GetCapabilitiesAsync(bound.Token);
            if (!host.Succeeded) return Rejected(intent, UserFaultCodes.HostUnavailable);
            if (intent.Destination == SpreadsheetOutputDestination.NewWorkbook) return Rejected(intent, CapabilityPreflightCodes.UnsupportedDestination);
            invoked = true;
            var reply = await transport.WriteAsync(plan, intent, consent, bound.Token).WaitAsync(bound.Token);
            if (reply is null || reply.Code is not ("output.completed" or "output.cancelled" or "output.rejected" or "output.destination-unsupported" or
                "output.occupancy-unverified" or "output.confirmation-required" or "output.consent-stale" or "output.mutation-uncertain" or "output.host-failed"))
                return Uncertain(intent);
            if ((reply.State == OutputWriteState.Completed && reply.Code != WriteCodes.Completed) ||
                (reply.State == OutputWriteState.CancelledByUser && reply.Code != WriteCodes.Cancelled) ||
                (reply.State == OutputWriteState.PartiallyWritten && reply.Code != WriteCodes.MutationUncertain) ||
                (reply.State == OutputWriteState.FailedBeforeWrite && reply.Code != WriteCodes.Failed) ||
                (reply.State == OutputWriteState.RejectedBeforeWrite && reply.Code is "output.completed" or "output.cancelled" or "output.mutation-uncertain"))
                return Uncertain(intent);
            return new WriteResult(reply.State, reply.Code, "Output transport result. No rollback is available.", intent,
                reply.MutationMayHaveOccurred, reply.ConfirmedWrittenIds, reply.ConfirmationRequired, reply.OverwriteApproved);
        }
        catch (WorkbookTransportException ex) when (ex.Code is "host.interop-import-failed" or "host.interop-create-failed")
        { return new(OutputWriteState.FailedBeforeWrite, WriteCodes.Failed, "Output transport was unavailable before writing.", intent, false); }
        catch (Exception) { return invoked ? Uncertain(intent) : Rejected(intent, WriteCodes.Rejected); }
        finally { operation.Release(); }
    }
    private static WriteResult Rejected(WriteIntent intent, string code) => new(OutputWriteState.RejectedBeforeWrite, code, "Output is unavailable or rejected before writing.", intent, false);
    private static WriteResult Uncertain(WriteIntent intent) => new(OutputWriteState.PartiallyWritten, WriteCodes.MutationUncertain, "Output completion could not be verified. No rollback was attempted.", intent, true);

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        lifetime.Cancel();
        await operation.WaitAsync();
        try { await transport.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2)); }
        catch (Exception) { }
        finally { operation.Release(); lifetime.Dispose(); }
    }
}
