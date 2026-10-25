using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed record InputIssue(string Code, string Field, int SelectedRow, int SelectedColumn, int WorksheetRow, int WorksheetColumn);
public sealed record InputPreview(int Rows, int Columns, int Numbers, int Blanks, int Text, int Booleans, int Errors,
    IReadOnlyList<int> GroupCounts, IReadOnlyList<InputIssue> Issues, bool Valid);
public sealed record TablePreview(string Id, int Rows, int Columns);

/// <summary>One component/operation owner. Common legacy data/results are private, never shared drafts.</summary>
public sealed class AnalysisPilotState : IAsyncDisposable
{
    private readonly IAnalysisDescriptor descriptor;
    private readonly AnalysisOptionDraft draft;
    private readonly IWorkbookAdapter workbook;
    private readonly IPilotHost host;
    private readonly GroupedInputConstraints constraints;
    private readonly PreferenceSnapshot preferences;
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? pending;
    private Task retirement = Task.CompletedTask;
    private long generation;
    private bool disposed;
    private bool writing;
    private SpreadsheetRangeData? source;
    private GroupedNumericData? grouped;
    private string? sourceToken, session;
    private long sourceRevision;
    private AnalysisResultDocument? result;
    private OutputDocument? document;
    private OfficeOutputPlan? plan;
    private WriteIntent? intent;
    public InputPreview? Preview { get; private set; }
    public bool ShowPreview { get; private set; }
    public IReadOnlyList<InputNotice> InputNotices { get; private set; } = [];
    private bool validationRequested;
    public string? InputRole { get; private set; }
    public WriteInspection? Inspection { get; private set; }
    public WriteResult? WriteOutcome { get; private set; }
    public WorksheetNameNotice? SheetNameNotice { get; private set; }
    public string? ResolvedWorksheetName { get; private set; }
    public string? CreatedWorksheetName { get; private set; }
    private bool requiresRecovery;
    public bool RequiresRecovery => requiresRecovery || (host as IPilotSafetyHost)?.RequiresRecovery == true;
    public const string RecoveryGuidance = "Inspect the workbook manually and wait for the previous native operation to finish. Do not retry this write. Close and reopen the task pane before another operation. No rollback is available.";
    public IReadOnlyList<TablePreview> Tables { get; private set; } = [];
    public IReadOnlyList<AnalysisMetricArtifact> Metrics => result?.Metrics ?? Array.Empty<AnalysisMetricArtifact>();
    public IReadOnlyList<string> WarningCodes => result?.Warnings.Select(w => w.Code).ToArray() ?? [];
    public IReadOnlyList<string> FaultCodes => result?.Faults.Select(w => w.Code).ToArray() ?? [];
    public int NonCellArtifacts => document?.Artifacts.Count(a => a is not OutputTableArtifact) ?? 0;
    public string Code { get; private set; } = "input.not-acquired";
    public string Guidance => RequiresRecovery ? RecoveryGuidance : Code switch {
        "office.unsupported-api" => "The pilot requires ExcelApi 1.17. The existing read-only diagnostic remains independent.",
        "input.preview-stale" or "user.document-changed" or "output.consent-stale" => "Click Run analysis to read and validate the current input references again. If a sheet was renamed or replaced, correct its reference first. Write results checks sources and destination afresh.",
        "input.validation-failed" => "Correct the input using the issue codes and physical row/column locations below, then Run again.",
        "input.long-shape" => $"{InputRole}: choose one column only. Category and values must be distinct columns on the same worksheet with overlapping rows.",
        "input.invalid-reference" => $"{InputRole ?? "Input range"}: enter one worksheet-qualified A1 range. External workbooks and multiple areas are unavailable.",
        "input.invalid-selection" => $"{InputRole ?? "Input range"}: select one contiguous range. In long format select one column for each role; multiple areas cannot be used.",
        "input.long-alignment" => $"{InputRole ?? "Group/category and numeric values ranges"}: use distinct single columns on the same worksheet with overlapping worksheet rows.",
        "input.long-header-alignment" => "Category and values must start on the same worksheet row for Required/Auto headers. Align the first rows or explicitly choose None for data without headers.",
        "input.empty-columns" => "The selected columns contain no used values. Choose columns with data or type a bounded input range.",
        "input.range-count" => "Whole-column verification found too many separate formula areas. Type a smaller bounded input range.",
        "output.inspecting" => "Checking sources and destination. No cells have been changed.",
        "output.writing" => "Writing results. Submitted Excel writes cannot be interrupted safely.",
        "input.total-cells" => "The combined input ranges exceed 10,000 cells. Select fewer rows in both ranges.",
        "input.estimated-bytes" => "The combined input exceeds the memory budget. Select fewer rows or smaller cell contents.",
        "input.time-budget" => "Input verification exceeded its time budget. Select less data and Run again.",
        "input.range-cells" => "Select a single rectangle of at most 10,000 cells.",
        "output.target-unwritable" => "Choose an unprotected writable workbook and worksheet.",
        "output.target-merged" => "Choose output cells that do not intersect merged areas.",
        "output.sheet-name-collision" => "A worksheet appeared after name allocation. No write was submitted. Click Write results again for a fresh available name.",
        "output.sheet-name-unavailable" => "No available numbered worksheet name could be verified within the bounded search. Choose another base name. No write was submitted.",
        "output.target-invalid" or "output.rejected" => "Check the start cell and sheet name. Output must avoid the input, tables, pivot tables and spill ranges.",
        "output.mutation-uncertain" => RecoveryGuidance,
        "analysis.completed" => "Results are ready. Choose an output destination and click Write results. Occupied cells require confirmation.",
        "host.busy" => "Wait for the current operation or request cancellation.",
        _ => "Run reads and validates the input before calculation. Validate input is optional. Save results requires a separate explicit write."
    };
    public bool Busy { get; private set; }
    public bool Current => source is not null && sourceToken is not null && sourceRevision == draft.Revision && draft.IsValid;
    private bool Supported
    {
        get
        {
            IAnalysisDescriptor registered = null!;
            return AnalysisCatalog.TryGetById(descriptor.Id, ref registered) && ReferenceEquals(registered, descriptor)
                && draft.IsValid && descriptor.OptionsType == draft.Snapshot().GetType() && descriptor.InputTypes.Contains(typeof(GroupedNumericData));
        }
    }
    public bool CanRun => !disposed && !RequiresRecovery && !Picking && !Busy && Supported;
    public bool CanInspect => !disposed && !RequiresRecovery && !Picking && !Busy && Current && OutputDraftValid && document is not null && result?.Faults.Count == 0 && Supported;
    private bool OutputDraftValid => Destination == SpreadsheetOutputDestination.NewWorksheet ||
        (Destination == SpreadsheetOutputDestination.CurrentWorksheet && OutputReference.IsValid);
    public bool CanWrite => CanInspect && plan is not null && intent is not null && Inspection is not null;
    public SpreadsheetOutputDestination Destination { get; private set; }
    public string StartCell { get; private set; } = "J1";
    public string NewSheetName { get; private set; } = "BESHStat results";
    public string? Geometry { get; private set; }
    public RangeReferenceState InputReference { get; }
    public RangeReferenceState CategoryReference { get; }
    public RangeReferenceState ValueReference { get; }
    public RangeReferenceState OutputReference { get; }
    public SpreadsheetGroupedOrientation Orientation { get; private set; } = SpreadsheetGroupedOrientation.GroupsInColumns;
    public SpreadsheetHeaderMode Header { get; private set; } = SpreadsheetHeaderMode.Required;
    public bool Picking => References.Any(r => r.Armed || r.Busy);
    private RangeReferenceState[] References => [InputReference, CategoryReference, ValueReference, OutputReference];
    public string Interpretation => $"{Orientation}; headers: {Header}";
    public event Action? Changed;

    public AnalysisPilotState(IAnalysisDescriptor descriptor, AnalysisOptionDraft draft, IWorkbookAdapter workbook, IPilotHost host,
        GroupedInputConstraints? constraints = null, PreferenceSnapshot? preferences = null)
    {
        this.descriptor = descriptor; this.draft = draft; this.workbook = workbook; this.host = host;
        this.constraints = constraints ?? new(); this.preferences = preferences ?? PreferenceSnapshot.Defaults();
        WriteOutcome = (host as IPilotSafetyHost)?.LastWriteOutcome;
        InputReference = new(host as IRangeCaptureHost, false, Invalidate, WaitForRetirementsAsync);
        CategoryReference = new(host as IRangeCaptureHost, false, Invalidate, WaitForRetirementsAsync);
        ValueReference = new(host as IRangeCaptureHost, false, Invalidate, WaitForRetirementsAsync);
        OutputReference = new(host as IRangeCaptureHost, true, DestinationEdited, WaitForRetirementsAsync);
        InputReference.Changed += ReferenceChanged; OutputReference.Changed += OutputReferenceChanged;
        CategoryReference.Changed += ReferenceChanged; ValueReference.Changed += ReferenceChanged;
        draft.Changed += OptionsChanged; host.Invalidated += Invalidate;
    }
    private void ReferenceChanged() => Changed?.Invoke();
    private void OutputReferenceChanged()
    {
        if (OutputReference.Code == "user.document-changed" && source is not null) { Invalidate(); return; }
        if (Destination == SpreadsheetOutputDestination.CurrentWorksheet && StartCell != OutputReference.Text) { StartCell = OutputReference.Text; DestinationEdited(); }
        Changed?.Invoke();
    }
    private void DestinationEdited() { generation++; pending?.Cancel(); ClearOutput(); QueueRetirement(true); Changed?.Invoke(); }
    private void QueueRetirement(bool outputOnly = false) => retirement = RetireAfterAsync(retirement, outputOnly);
    private async Task RetireAfterAsync(Task previous, bool outputOnly)
    {
        await previous;
        if (outputOnly) await host.InvalidateOutputAsync(); else await host.InvalidateAsync();
    }
    private async Task WaitForRetirementsAsync(CancellationToken token)
    {
        Task observed;
        do { observed = retirement; await observed.WaitAsync(token); } while (!ReferenceEquals(observed, retirement));
    }
    public void SetInterpretation(SpreadsheetGroupedOrientation orientation, SpreadsheetHeaderMode header)
    {
        if (disposed || (Orientation == orientation && Header == header)) return;
        Orientation = orientation; Header = header; Invalidate();
    }
    public Task AcquireSelectionAsync()
    {
        // Fallback is independent of event support. The existing reader acquires the selection.
        InputReference.Edit(""); return AcquireAsync();
    }
    private void OptionsChanged() => Invalidate();
    public void Invalidate()
    {
        if (disposed) return;
        foreach (var reference in References) reference.Stop();
        generation++; pending?.Cancel(); source = null; grouped = null; sourceToken = session = null;
        result = null; document = null; Tables = []; ClearOutput(); Code = "input.preview-stale";
        Preview = null; ShowPreview = false; validationRequested = false; InputRole = null; InputNotices = [];
        QueueRetirement(); Changed?.Invoke();
    }
    private void ClearOutput() { plan = null; intent = null; Inspection = null; Geometry = null; SheetNameNotice = null; ResolvedWorksheetName = null; }
    public void Reset()
    {
        if (Busy || disposed) return;
        Invalidate(); foreach (var reference in References) reference.Reset();
        Orientation = SpreadsheetGroupedOrientation.GroupsInColumns; Header = SpreadsheetHeaderMode.Required;
        Destination = SpreadsheetOutputDestination.CurrentWorksheet; StartCell = "J1"; NewSheetName = "BESHStat results";
        draft.Reset(); Code = RequiresRecovery ? "output.mutation-uncertain" : "input.not-acquired";
        Changed?.Invoke();
    }
    public void SetDestination(SpreadsheetOutputDestination destination, string cell, string name)
    {
        if (disposed || (Destination == destination && StartCell == cell && NewSheetName == name)) return;
        generation++; pending?.Cancel(); Destination = destination; StartCell = cell; NewSheetName = name;
        if (destination == SpreadsheetOutputDestination.CurrentWorksheet) OutputReference.Edit(cell); ClearOutput(); QueueRetirement(true); Changed?.Invoke();
    }
    public void Cancel()
    {
        if (writing) { pending?.Cancel(); QueueRetirement(); Code = "output.cancellation-requested"; Changed?.Invoke(); }
        else { Invalidate(); Code = "user.cancelled"; }
    }
    private async Task Operation(Func<CancellationToken, long, Task> action)
    {
        if (disposed || Busy) { if (!disposed) Code = "host.busy"; return; }
        Busy = true;
        Changed?.Invoke();
        var version = generation;
        using var bound = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        bound.CancelAfter(TimeSpan.FromSeconds(30)); pending = bound;
        try { await WaitForRetirementsAsync(bound.Token); if (Accept(version, bound.Token)) await action(bound.Token, version); }
        catch (OperationCanceledException) { if (!disposed && version == generation) { Invalidate(); Code = lifetime.IsCancellationRequested ? "user.cancelled" : "input.time-budget"; ShowPreview = !lifetime.IsCancellationRequested; } }
        catch (SpreadsheetInputValidationException ex) { if (!disposed && version == generation) { ClearOutput(); Code = Safe(ex.Report.Issues[0].Code); ShowPreview = true; } }
        catch (GroupedInputException ex) { if (!disposed && version == generation) { ClearOutput(); Code = ex.Code; ShowPreview = true; } }
        catch (Exception) { if (!disposed && version == generation) { ClearOutput(); Code = "host.operation-failed"; } }
        finally { if (pending == bound) pending = null; Busy = false; if (!disposed) Changed?.Invoke(); }
    }
    private bool Accept(long version, CancellationToken token)
    {
        if (disposed || version != generation) return false;
        // A deadline is a visible failed operation even if an adapter returned a cancellation reply.
        token.ThrowIfCancellationRequested();
        return true;
    }
    private static string Safe(string? code) => code is "workbook.ok" or "host.busy" or "user.cancelled" or "user.document-changed" or
        "office.unavailable" or "office.unsupported-api" or "input.range-cells" or "input.range-count" or "input.empty-columns" or "input.invalid-selection" or "input.time-budget" or
        "input.estimated-bytes" or "input.total-cells" or "input.invalid-reference" or "range.capture-unavailable" or "output.rejected" or "output.mutation-uncertain" or "output.target-unwritable" or
        "output.target-merged" or "output.target-invalid" or "output.sheet-name-collision" or "output.sheet-name-adjusted" or "output.sheet-name-unavailable" or "output.consent-stale" ? code : "host.operation-failed";

    public Task AcquireAsync() => Operation(async (token, version) => {
        ShowPreview = validationRequested = true;
        await AcquireWithinOperationAsync(token, version, allowSelection: true);
    });
    public Task ValidateAsync() => Operation(async (token, version) => {
        ShowPreview = validationRequested = true;
        await AcquireWithinOperationAsync(token, version);
    });

    private async Task AcquireWithinOperationAsync(CancellationToken token, long version, bool allowSelection = false)
    {
        if (RequiresRecovery) { Code = "output.mutation-uncertain"; return; }
        source = null; grouped = null; sourceToken = session = null; result = null; document = null; Tables = []; ClearOutput();
        Preview = null; InputRole = null; InputNotices = [];
        if (Picking || !Supported || !Enum.IsDefined(Orientation) || !Enum.IsDefined(Header)) { Code = "analysis.invalid-request"; return; }
        var longFormat = Orientation == SpreadsheetGroupedOrientation.LongFormat;
        var inputs = longFormat ? new[] { CategoryReference, ValueReference } : new[] { InputReference };
        var refs = new List<SpreadsheetRangeReference>();
        var limits = new InputSizeLimits();
        var budget = new InputAcquisitionBudget(limits, token);
        var effectiveBudget = new InputAcquisitionBudget(limits, token);
        foreach (var input in inputs)
        {
            InputRole = longFormat ? (input == CategoryReference ? "Group/category (ID) range" : "Numeric values range") : "Input range";
            if (allowSelection && !longFormat && input.Text.Length == 0) break;
            SpreadsheetRangeReference reference;
            try { reference = RangeReferenceState.Parse(input.Text, false); }
            catch (ArgumentException) { Code = "input.invalid-reference"; return; }
            if (longFormat && reference.ColumnCount != 1) { Code = "input.long-shape"; return; }
            if (!RangeReferenceState.WholeColumns(reference)) budget.AddRange(reference.RowCount, reference.ColumnCount);
            refs.Add(reference);
        }
        if (longFormat && (refs[0].WorksheetName != refs[1].WorksheetName || refs[0].StartColumn == refs[1].StartColumn ||
            Math.Max(refs[0].StartRow, refs[1].StartRow) > Math.Min(refs[0].StartRow + refs[0].RowCount - 1, refs[1].StartRow + refs[1].RowCount - 1)))
        { Code = "input.long-alignment"; return; }
        if (longFormat && !refs.Any(RangeReferenceState.WholeColumns) && Header != SpreadsheetHeaderMode.None && refs[0].StartRow != refs[1].StartRow)
        { Code = "input.long-header-alignment"; return; }
        await host.InvalidateAsync();
        var physical = new List<PhysicalSource>();
        var resolvedInputs = new List<RangeCaptureReply?>();
        double bytes = 0;
        foreach (var input in inputs)
        {
            InputRole = longFormat ? (input == CategoryReference ? "Group/category (ID) range" : "Numeric values range") : "Input range";
            var reference = input.Text.Length > 0 ? await input.ResolveAsync(token) : null;
            if (!Accept(version, token)) return;
            if (input.Text.Length > 0 && reference?.Reference is null) { Code = input.Code; return; }
            if (reference?.Reference is { } effective)
            {
                effectiveBudget.AddRange(effective.RowCount, effective.ColumnCount);
                if (reference.OriginalReference is { } original && RangeReferenceState.WholeColumns(original))
                    InputNotices = InputNotices.Append(new("input.columns-narrowed", InputRole!, effective.RowCount, 0)).ToArray();
            }
            resolvedInputs.Add(reference);
        }
        foreach (var reference in resolvedInputs)
        {
            InputRole = longFormat ? (physical.Count == 0 ? "Group/category (ID) range" : "Numeric values range") : "Input range";
            var acquired = reference is null ? await workbook.ReadSelectionAsync(cancellationToken: token)
                : await workbook.ReadRangeAsync(reference.Reference!, cancellationToken: token);
            if (!Accept(version, token)) return;
            budget.Checkpoint(); Code = Safe(acquired.Code);
            if (!acquired.Succeeded) return;
            var copy = CopySource(acquired.Value!);
            bytes += copy.Rows.Where(r => r is not null).SelectMany(r => r).Where(c => c is not null)
                .Sum(c => limits.EstimatedBytesPerCell + 2d * ((c.TextValue?.Length ?? 0) + (c.NumberFormat?.Length ?? 0)));
            if (bytes > limits.MaxEstimatedBytes) { Code = "input.estimated-bytes"; return; }
            physical.Add(new(copy, reference?.Handle ?? ""));
        }
        if (longFormat && !PhysicalInputView.Aligned(physical[0].Snapshot, physical[1].Snapshot)) { Code = "input.long-alignment"; return; }
        var parsed = GroupedInput.Parse(physical.Select(p => p.Snapshot).ToArray(), new(Orientation, Header, constraints));
        var snapshot = parsed.ParserInput;
        InputNotices = InputNotices.Concat(parsed.Notices).ToArray();
        var primary = physical[0].Snapshot;
        InputRole = null;
        if (OutputReference.Text.Length == 0)
        {
            var defaultOutput = SpreadsheetRangeReference.FromAddress("J1", primary.WorksheetName).QualifiedAddress;
            if (Destination == SpreadsheetOutputDestination.CurrentWorksheet) StartCell = defaultOutput;
            OutputReference.SetDefault(defaultOutput);
        }
        var valid = parsed.Valid;
        var cells = snapshot.Rows.Where(r => r is not null).SelectMany(r => r).ToArray();
        int Count(SpreadsheetCellKind kind) => cells.Count(c => c?.Kind == kind);
        Preview = new(snapshot.RowCount, snapshot.ColumnCount, Count(SpreadsheetCellKind.Number), Count(SpreadsheetCellKind.Blank),
            Count(SpreadsheetCellKind.Text), Count(SpreadsheetCellKind.Bool), Count(SpreadsheetCellKind.CellError),
            Array.AsReadOnly(parsed.Data?.GroupCounts ?? []), parsed.Issues, valid);
        if (!valid) { Code = "input.validation-failed"; return; }
        PilotReply? pin;
        if (longFormat)
        {
            if (host is not ICompositeSourceHost composite || physical.Any(p => string.IsNullOrWhiteSpace(p.Handle)))
            { Code = "office.unsupported-api"; return; }
            pin = await composite.PinSourcesAsync(physical, token);
        }
        else pin = physical[0].Handle.Length > 0 && host is IRangeCaptureHost rangeHost
            ? await rangeHost.PinReferenceSourceAsync(primary, physical[0].Handle, token) : await host.PinSourceAsync(primary, token);
        if (!Accept(version, token)) return;
        Code = Safe(pin?.Code);
        if (pin?.Code != "workbook.ok" || string.IsNullOrWhiteSpace(pin.SourceToken) || string.IsNullOrWhiteSpace(pin.SessionIdentity)) return;
        source = primary; grouped = parsed.Data; sourceToken = pin.SourceToken; session = pin.SessionIdentity; sourceRevision = draft.Revision;
        if (!longFormat) InputReference.FromSnapshot(primary, pin.SourceToken, resolvedInputs[0]?.OriginalReference);
    }

    public Task RunAsync() => Operation(async (token, version) => {
        ShowPreview = validationRequested;
        if (Picking || !Supported) { Code = "analysis.invalid-request"; return; }
        await AcquireWithinOperationAsync(token, version);
        if (!Accept(version, token)) return;
        if (!Current || Preview?.Valid != true || grouped is null) { ShowPreview = true; return; }
        ClearOutput(); result = null; document = null; Tables = [];
        var check = await host.CheckSourceAsync(sourceToken!, token);
        if (!Accept(version, token)) return;
        if (check?.Code != "workbook.ok") { Invalidate(); Code = Safe(check?.Code); return; }
        var options = draft.Snapshot();
        await Task.Yield(); // Render busy before synchronous Common computation, without claiming interruptibility.
        if (!Accept(version, token)) return;
        var computed = SharedAnalysisApplicationService.RunDocument(descriptor.Id, grouped, options);
        if (!Accept(version, token)) return;
        result = computed; document = OutputDocument.FromAnalysis(computed);
        Tables = Array.AsReadOnly(document.Artifacts.OfType<OutputTableArtifact>().Select(t => new TablePreview(t.Id, t.RowCount, t.ColumnCount)).ToArray());
        Code = computed.Faults.Count == 0 ? "analysis.completed" : computed.Faults[0].Code;
    });

    private static SpreadsheetRangeData CopySource(SpreadsheetRangeData s) => new()
    {
        WorksheetId = s.WorksheetId, WorksheetName = s.WorksheetName, Address = s.Address, AddressDialect = s.AddressDialect,
        WorkbookId = s.WorkbookId, WorkbookName = s.WorkbookName, SelectionId = s.SelectionId, SelectionRevision = s.SelectionRevision,
        DateSystem = s.DateSystem, DateSerialOrigin = s.DateSerialOrigin, StartRow = s.StartRow, StartColumn = s.StartColumn,
        RowCount = s.RowCount, ColumnCount = s.ColumnCount, Rows = s.Rows.Select(r => r is null ? null! : r.Select(c => c is null ? null! : new SpreadsheetCellValue
        { Kind = c.Kind, NumberValue = c.NumberValue, TextValue = c.TextValue, BooleanValue = c.BooleanValue, NumberFormat = c.NumberFormat, NativeErrorCode = c.NativeErrorCode }).ToList()).ToList()
    };

    public Task InspectAsync() => Operation(InspectWithinOperationAsync);
    private async Task InspectWithinOperationAsync(CancellationToken token, long version)
    {
        if (Picking || !Current || document is null || result?.Faults.Count != 0 || source is null) { Code = "input.preview-stale"; return; }
        if (!OutputDraftValid) { Code = "output.rejected"; return; }
        ClearOutput();
        var check = await host.CheckSourceAsync(sourceToken!, token);
        if (!Accept(version, token)) return;
        if (check?.Code != "workbook.ok") { Invalidate(); Code = Safe(check?.Code); return; }
        try
        {
            var cell = SpreadsheetRangeAddress.Parse(StartCell);
            if (cell.RowCount != 1 || cell.ColumnCount != 1 || cell.WorkbookName.Length != 0 || Destination == SpreadsheetOutputDestination.NewWorkbook) throw new ArgumentException();
            var targetName = source.WorksheetName;
            var targetId = source.WorksheetId;
            if (Destination == SpreadsheetOutputDestination.CurrentWorksheet && cell.WorksheetName.Length > 0)
            {
                if (host is IRangeCaptureHost)
                {
                    var destination = await OutputReference.ResolveAsync(token);
                    if (!Accept(version, token)) return;
                    if (destination?.Reference is not { } resolved) { Code = OutputReference.Code; return; }
                    targetName = resolved.WorksheetName; targetId = resolved.WorksheetId;
                }
                else if (cell.WorksheetName != source.WorksheetName) throw new ArgumentException();
            }
            else if (Destination == SpreadsheetOutputDestination.NewWorksheet && cell.WorksheetName.Length > 0 && cell.WorksheetName != NewSheetName)
                throw new ArgumentException();
            if (Destination == SpreadsheetOutputDestination.NewWorksheet)
            {
                if (host is not IWorksheetNameHost nameHost) { Code = "office.unsupported-api"; return; }
                var allocation = await nameHost.ResolveNewWorksheetNameAsync(NewSheetName, sourceToken!, token);
                if (!Accept(version, token)) return;
                if (allocation?.Code != "workbook.ok") { Code = Safe(allocation?.Code); return; }
                if (allocation.RequestedName != NewSheetName || string.IsNullOrWhiteSpace(allocation.ResolvedName) ||
                    allocation.ResolvedName.Length > 31 ||
                    (allocation.ResolvedName == NewSheetName ? allocation.WarningCode is not null : allocation.WarningCode != "output.sheet-name-adjusted"))
                { Code = "host.invalid-response"; return; }
                targetName = allocation.ResolvedName;
                ResolvedWorksheetName = targetName;
                if (allocation.WarningCode is { } warning) SheetNameNotice = new(NewSheetName, targetName, warning);
                Changed?.Invoke();
            }
            var target = new SpreadsheetOutputTarget { Destination = Destination, StartRow = cell.StartRow, StartColumn = cell.StartColumn,
                WorksheetName = targetName };
            var capabilities = new HostCapabilitySnapshot { Host = "OfficeJsPilot", IsExcel = true, IsSpreadsheetHost = true,
                Capabilities = [HostCapabilityIds.WriteResultTables, HostCapabilityIds.CurrentWorksheet, HostCapabilityIds.NewWorksheet] };
            var decision = TargetPlanner.Create(target, capabilities);
            decision.EnsureSupported();
            var wire = OutputPlanPresentation.Prepare(document, decision.ToTarget(), draft.ValidatedAlpha, preferences);
            var writeIntent = new WriteIntent(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), decision.ToTarget(),
                wire.Tables.Select(t => new WriteRegion(t.ArtifactId, t.Bounds)), workbookIdentity: session,
                worksheetIdentity: Destination == SpreadsheetOutputDestination.CurrentWorksheet ? targetId : null);
            var observation = await host.PrepareOutputAsync(wire, writeIntent, sourceToken!, token);
            if (!Accept(version, token)) return;
            Code = Safe(observation?.Code);
            if (observation?.Code != "workbook.ok" || observation.State is not (OutputOccupancy.Empty or OutputOccupancy.Occupied)) return;
            if (observation.Facts?.Verified(Destination) != true) { Code = "output.occupancy-unverified"; return; }
            TargetPlanner.Create(target, capabilities, observation.Facts.ToCommon()).EnsureSupported();
            var inspection = new WriteInspection(writeIntent, observation.State, observation.ObservationIdentity!, observation.OccupiedRegionIds,
                observation.HasValues, observation.HasFormulas);
            plan = wire; intent = writeIntent; Inspection = inspection;
            Geometry = wire.UsedBounds is { } b ? $"{wire.Tables.Count} tables; rows {b.StartRow}–{b.EndRow}, columns {b.StartColumn}–{b.EndColumn}" : "No cell output";
            Code = inspection.Code;
        }
        catch (ArgumentException) { Code = "output.rejected"; }
    }

    public Task WriteResultsAsync() => Operation(async (token, version) => {
        if (RequiresRecovery) { Code = "output.mutation-uncertain"; return; }
        Code = "output.inspecting"; Changed?.Invoke();
        await InspectWithinOperationAsync(token, version);
        if (Accept(version, token) && Inspection?.State == OutputOccupancy.Empty)
            await WriteWithinOperationAsync(token, version, null);
    });

    public Task WriteAsync(bool? approve = null) => Operation((token, version) => WriteWithinOperationAsync(token, version, approve));
    private async Task WriteWithinOperationAsync(CancellationToken token, long version, bool? approve)
    {
        if (Picking || !Current || !OutputDraftValid || plan is null || intent is null || Inspection is null ||
            (Inspection.State == OutputOccupancy.Occupied && !approve.HasValue) || (Inspection.State == OutputOccupancy.Empty && approve.HasValue))
        { Code = "output.rejected"; return; }
        var boundPlan = plan; var boundIntent = intent; var inspection = Inspection;
        plan = null; intent = null; Inspection = null;
        var check = await host.CheckSourceAsync(sourceToken!, token);
        if (!Accept(version, token)) return;
        if (check?.Code != "workbook.ok") { Invalidate(); Code = "output.consent-stale"; return; }
        var consent = approve.HasValue ? new WriteConsent(inspection, approve.Value) : null;
        writing = true;
        Code = "output.writing"; Changed?.Invoke();
        WriteResult written;
        try { written = await workbook.WriteAsync(boundPlan, boundIntent, consent, token); }
        finally { writing = false; }
        // Keep the pane's latch even if this method's component was retired during native work.
        (host as IPilotSafetyHost)?.RecordWriteOutcome(written);
        // An observed write outcome remains truthful after late Cancel or host invalidation.
        if (disposed) return;
        WriteOutcome = written; Code = written.Code;
        CreatedWorksheetName = written.State == OutputWriteState.Completed && boundIntent.Destination == SpreadsheetOutputDestination.NewWorksheet
            ? boundIntent.WorksheetName : null;
        requiresRecovery |= written.State == OutputWriteState.PartiallyWritten && written.MutationMayHaveOccurred;
        // A declined/rejected non-mutation may retain results only after a fresh physical-source check.
        // The consumed plan/consent is always retired; another mutation still needs an explicit new click.
        var retainSource = false;
        if (!written.MutationMayHaveOccurred && written.State is OutputWriteState.CancelledByUser or OutputWriteState.RejectedBeforeWrite)
            try {
                var current = await host.CheckSourceAsync(sourceToken!, token);
                retainSource = !disposed && generation == version && !token.IsCancellationRequested && current?.Code == "workbook.ok";
            } catch (Exception) { /* The observed write outcome is retained; source continuity is unavailable. */ }
        if (!retainSource) { source = null; grouped = null; sourceToken = session = null; }
        var notice = SheetNameNotice is { } n ? n with { Created = written.State == OutputWriteState.Completed } : null;
        var resolvedName = ResolvedWorksheetName;
        ClearOutput(); SheetNameNotice = notice; ResolvedWorksheetName = resolvedName;
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        draft.Changed -= OptionsChanged; host.Invalidated -= Invalidate; generation++; disposed = true;
        pending?.Cancel(); lifetime.Cancel(); Changed = null;
        QueueRetirement(); // Retire immediately, before asynchronous picker disposal can yield to a new owner.
        foreach (var reference in References) await reference.DisposeAsync();
        await retirement; source = null; grouped = null; result = null; document = null; ClearOutput(); lifetime.Dispose();
    }
}
