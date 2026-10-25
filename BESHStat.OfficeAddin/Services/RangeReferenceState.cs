using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>Component-owned reference draft; it never acquires cell data.</summary>
public sealed class RangeReferenceState : IAsyncDisposable
{
    private readonly IRangeCaptureHost? host;
    private readonly bool output;
    private readonly Action edited;
    private readonly Func<CancellationToken, Task>? beforeHost;
    private Task captureRetirement = Task.CompletedTask;
    private readonly string owner = Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource lifetime = new();
    private long generation;
    private bool disposed;
    private string committed = "";
    private RangeCaptureReply? resolved, committedReference;
    public string Text { get; private set; } = "";
    public string Code { get; private set; } = "range.not-captured";
    public bool Armed { get; private set; }
    public bool Busy { get; private set; }
    public bool IsValid
    {
        get
        {
            if (disposed || Armed || Busy || Code is not ("workbook.ok" or "range.edited" or "range.not-captured" or "range.capture-cancelled")) return false;
            try { Parse(Text, output); return true; } catch (ArgumentException) { return false; }
        }
    }
    public event Action? Changed;
    public RangeReferenceState(IRangeCaptureHost? host, bool output, Action edited, Func<CancellationToken, Task>? beforeHost = null)
    { this.host = host; this.output = output; this.edited = edited; this.beforeHost = beforeHost; if (host is not null) host.RangeCaptured += Captured; }
    private async Task ReadyAsync(CancellationToken token)
    { await captureRetirement.WaitAsync(token); if (beforeHost is not null) await beforeHost(token); token.ThrowIfCancellationRequested(); }
    private void Captured(RangeCaptureReply reply)
    {
        if (disposed || !Armed || reply.Owner != owner) return;
        Apply(reply, false); Changed?.Invoke();
    }
    private bool Apply(RangeCaptureReply? reply, bool commit)
    {
        Code = reply?.Code ?? "host.invalid-response";
        if (Code != "workbook.ok" || reply?.Reference is not { } reference || string.IsNullOrEmpty(reply.Handle)) return false;
        try
        {
            Validate(reference, output);
            if (reply.OriginalReference is { } original)
            {
                Validate(original, false);
                if (output || !WholeColumns(original) || original.WorksheetName != reference.WorksheetName ||
                    original.WorksheetId.Length > 0 && original.WorksheetId != reference.WorksheetId ||
                    original.StartColumn != reference.StartColumn || original.ColumnCount != reference.ColumnCount ||
                    reference.StartRow < 1 || reference.RowCount < 1 || (long)reference.StartRow + reference.RowCount - 1 > 1048576)
                    throw new ArgumentException();
            }
            Text = Display(reply.OriginalReference ?? reference); resolved = reply;
            if (commit) { committed = Text; committedReference = reply; }
            return true;
        }
        catch (ArgumentException) { Code = "input.invalid-reference"; return false; }
    }
    public static SpreadsheetRangeReference Parse(string text, bool singleCell)
    {
        if (text.Length > 1024 || text.Any(char.IsControl)) throw new ArgumentException();
        // Expand only the Excel entire-column shorthand; Common still owns worksheet quoting and A1 validation.
        var separator = text.LastIndexOf('!');
        if (!singleCell && separator > 0)
        {
            var columns = System.Text.RegularExpressions.Regex.Match(text[(separator + 1)..], @"^\$?([A-Za-z]{1,3}):\$?([A-Za-z]{1,3})$");
            if (columns.Success) text = text[..(separator + 1)] + columns.Groups[1].Value + "1:" + columns.Groups[2].Value + "1048576";
        }
        var reference = SpreadsheetRangeReference.FromUserInput("", text);
        Validate(reference, singleCell); return reference;
    }
    private static void Validate(SpreadsheetRangeReference reference, bool singleCell)
    {
        if (reference.WorkbookName.Length != 0 || reference.WorkbookId.Length != 0 || reference.WorksheetName.Length is 0 or > 256 ||
            reference.Address.Length > 40 || reference.AddressDialect != SpreadsheetAddressDialect.ExcelA1 ||
            (singleCell && (reference.RowCount != 1 || reference.ColumnCount != 1)) ||
            (!singleCell && !WholeColumns(reference) && (long)reference.RowCount * reference.ColumnCount > 10000) ||
            reference.ColumnCount == 16384 && reference.RowCount == 1048576) throw new ArgumentException();
    }
    public static bool WholeColumns(SpreadsheetRangeReference reference) => reference.StartRow == 1 && reference.RowCount == 1048576 && reference.ColumnCount < 16384;
    private static string Display(SpreadsheetRangeReference reference)
    {
        if (!WholeColumns(reference)) return reference.QualifiedAddress;
        var qualified = reference.QualifiedAddress; var parts = reference.Address.Split(':');
        return qualified[..(qualified.LastIndexOf('!') + 1)] + new string(parts[0].Where(char.IsLetter).ToArray()) + ":" + new string(parts[^1].Where(char.IsLetter).ToArray());
    }
    public void Edit(string text)
    {
        if (disposed || Text == text) return;
        Stop(); Text = text; resolved = text == committed ? committedReference : null;
        Code = "range.edited";
        try { Parse(text, output); } catch (ArgumentException) { Code = "input.invalid-reference"; }
        edited(); Changed?.Invoke();
    }
    public void SetDefault(string text)
    { if (!disposed && Text.Length == 0) { Text = committed = text; Changed?.Invoke(); } }
    public void Reset()
    {
        Stop(); Text = committed = ""; resolved = committedReference = null;
        Code = "range.not-captured"; Changed?.Invoke();
    }
    public void FromSnapshot(SpreadsheetRangeData snapshot, string? handle = null, SpreadsheetRangeReference? original = null)
    {
        var reference = SpreadsheetRangeReference.FromAddress(snapshot.Address, snapshot.WorksheetName);
        reference.WorksheetId = snapshot.WorksheetId;
        Text = committed = Display(original ?? reference); Code = "workbook.ok";
        resolved = committedReference = handle is null ? null : new() { Code = "workbook.ok", Handle = handle, Reference = reference, OriginalReference = original };
        Changed?.Invoke();
    }
    private async Task Execute(Func<CancellationToken, Task> action)
    {
        if (disposed || Busy) return;
        Busy = true;
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); bound.CancelAfter(TimeSpan.FromSeconds(6));
        Changed?.Invoke();
        try { await action(bound.Token); }
        catch (Exception)
        {
            Armed = false;
            await EndHost(true);
            if (host is IPilotHost pilot) await pilot.InvalidateAsync();
            if (!disposed) Code = output ? "user.document-changed" : "range.capture-unavailable";
        }
        finally { Busy = false; if (!disposed) Changed?.Invoke(); }
    }
    public Task ArmAsync() => Execute(async token => {
        if (Armed) return;
        Stop(); edited(); var version = generation;
        await ReadyAsync(token); if (disposed || version != generation) return;
        var reply = host is null ? null : await host.BeginCaptureAsync(owner, output, token);
        if (disposed || version != generation) { await EndHost(true); return; }
        Code = reply?.Code ?? "range.capture-unavailable";
        Armed = Code == "workbook.ok";
        if (!Armed) await EndHost(true);
    });
    public Task CaptureAsync() => Execute(async token => {
        if (Armed) await CancelAsync();
        Stop(); edited(); var version = generation;
        await ReadyAsync(token); if (disposed || version != generation) return;
        var reply = host is null ? null : await host.CaptureSelectionAsync(output, token);
        if (!disposed && version == generation) Apply(reply, true);
    });
    public Task FinishAsync() => Execute(async token => {
        if (!Armed || host is null) return;
        var version = generation;
        var reply = await host.EndCaptureAsync(owner, false, token);
        if (disposed || version != generation) return;
        Armed = false; Apply(reply, true);
    });
    public async Task CancelAsync()
    {
        if (disposed) return;
        var alreadyBusy = Busy;
        generation++; Armed = false; Busy = true;
        Text = committed; resolved = committedReference; Code = "range.capture-cancelled"; Changed?.Invoke();
        edited();
        try { await EndHost(true); }
        finally { if (!alreadyBusy) Busy = false; if (!disposed) Changed?.Invoke(); }
    }
    public async Task<RangeCaptureReply?> ResolveAsync(CancellationToken token)
    {
        if (!IsValid) return null;
        var version = generation;
        try
        {
            var reference = Parse(Text, output);
            await ReadyAsync(token); if (disposed || version != generation) return null;
            var reply = host is null ? null : await host.ResolveReferenceAsync(reference, resolved?.Handle, token);
            if (disposed || version != generation || token.IsCancellationRequested) return null;
            return Apply(reply, true) ? reply : null;
        }
        catch (ArgumentException) { Code = "input.invalid-reference"; return null; }
    }
    /// <summary>Explicit preview retry after a transient host rejection. Keep the physical handle:
    /// a renamed, removed or replaced sheet must still be rejected by the existing resolver.</summary>
    public void PrepareResolutionRetry()
    {
        if (disposed || Armed || Busy) return;
        try { Parse(Text, output); Code = "range.edited"; }
        catch (ArgumentException) { Code = "input.invalid-reference"; }
    }
    private async Task EndHost(bool cancel)
    { if (host is not null) try { using var bound = new CancellationTokenSource(TimeSpan.FromSeconds(2)); await host.EndCaptureAsync(owner, cancel, bound.Token); } catch (Exception) { } }
    public void Stop()
    { generation++; if (Armed) { Armed = false; captureRetirement = EndHost(true); } }
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        generation++; Armed = false; disposed = true; lifetime.Cancel(); if (host is not null) host.RangeCaptured -= Captured;
        await EndHost(true); Changed = null; lifetime.Dispose();
    }
}
