using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public enum SpecificationStatus { DraftChanged, Busy, Ready, Invalid, SourceStale, DocumentChanged, Disposed }

/// <summary>One worksheet-based preview owner. Discovery/read/pin operations are serialized.</summary>
public sealed class SpecificationBuilderState : IAsyncDisposable
{
    private readonly IWorkbookAdapter workbook;
    private readonly IPilotHost host;
    private readonly IWorksheetDiscoveryHost? discovery;
    private readonly string owner=Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource lifetime=new();
    private CancellationTokenSource? pending;
    private Task operation=Task.CompletedTask;
    private Task retirement=Task.CompletedTask;
    private long generation, scheduledVersion;
    private bool disposed, resetting;
    private WorksheetColumnCatalog? source;
    public WorksheetCatalog? Worksheets { get; private set; }
    public string SelectedWorksheetId { get; private set; } = "";
    public SpecificationDraft Draft { get; } = new();
    public SpecificationPreviewData? Preview { get; private set; }
    public IReadOnlyList<SpreadsheetModelCategoricalColumn> FactorLevels { get; private set; } = [];
    public IReadOnlyList<SpecificationColumn> Columns => source?.ForHeader(Draft.Header) ?? [];
    public SpecificationStatus Status { get; private set; } = SpecificationStatus.DraftChanged;
    public string Code { get; private set; } = "spec.not-discovered";
    public string? Notice { get; private set; }
    public long VariableRevision { get; private set; }
    public bool Busy { get; private set; }
    public bool RequiresRecovery => (host as IPilotSafetyHost)?.RequiresRecovery == true;
    public bool CanPreview => !disposed && !Busy && !RequiresRecovery && Columns.Count>0;
    public string Guidance => Code switch {
        "office.unsupported-api" => "Worksheet discovery requires ExcelApi 1.17 and the worksheet metadata/event APIs. This host cannot verify them.",
        "input.range-cells" => "The row-1-to-used-data rectangle exceeds 10,000 cells. Remove distant unused data or use a smaller worksheet data footprint.",
        "spec.column-limit" => "The used-data span exceeds 128 physical columns. No columns were truncated; reduce the worksheet footprint.",
        "spec.sheet-limit" => "The workbook has more than 256 worksheets. Discovery cannot safely enumerate this workbook.",
        "input.range-count" => "More than 64 formula areas were found. Use a smaller or more contiguous worksheet data footprint.",
        "user.document-changed" => "The worksheet or workbook changed. Choose a current worksheet or refresh variables, then choose roles again.",
        "input.time-budget" or "host.busy" => "The host is busy or metadata verification timed out. Wait for native work to settle and refresh variables.",
        "host.operation-failed" or "host.invalid-response" => "Worksheet metadata could not be verified. Refresh variables when the Excel host is ready.",
        _ => "Preview reads fresh worksheet data and uses Common/Core validation. Local role and term edits do not read or write Excel."
    };
    public event Action? Changed;
    public SpecificationBuilderState(IWorkbookAdapter workbook, IPilotHost host)
    {
        this.workbook=workbook;this.host=host;discovery=host as IWorksheetDiscoveryHost;
        Draft.Changed+=DraftChanged;host.Invalidated+=HostChanged;
        if(discovery is not null)discovery.WorksheetsInvalidated+=CatalogChanged;
    }
    private void Notify(){if(!disposed)Changed?.Invoke();}
    private void ClearPreview(){Preview=null;FactorLevels=[];}
    private void DraftChanged()
    {
        if(resetting || disposed)return;
        if(Busy){generation++;pending?.Cancel();}
        ClearPreview();Retire();Status=SpecificationStatus.DraftChanged;Code="spec.draft-changed";Notify();
        // Keep only the read-only discovery watch; no compiled spec/output permission survives.
    }
    private void ResetRoles(string notice)
    {
        resetting=true;try{Draft.ResetRoles();}finally{resetting=false;}
        VariableRevision++;Notice=notice;ClearPreview();
    }
    private void HostChanged()=>CatalogChanged(owner,"spec.source-stale");
    private void CatalogChanged(string callbackOwner,string code)
    {
        if(disposed || callbackOwner!=owner)return;
        ClearPreview();Code=code;Status=code=="user.document-changed"?SpecificationStatus.DocumentChanged:SpecificationStatus.SourceStale;
        // Schedule stores this task; all later operations and disposal drain it.
        _=RefreshAsync();
    }
    public Task InitializeAsync()=>Schedule(false);
    public Task RefreshAsync()=>Schedule(false);
    public Task SelectWorksheetAsync(string id,string? expectedDocumentIdentity=null)
    {
        if(expectedDocumentIdentity is not null && expectedDocumentIdentity!=Worksheets?.DocumentIdentity){Reject("user.document-changed");Notify();return Task.CompletedTask;}
        if(disposed || id==SelectedWorksheetId)return Task.CompletedTask;
        if(Worksheets?.Worksheets.All(s=>s.Id!=id)!=false){Reject("user.document-changed");Notify();return Task.CompletedTask;}
        SelectedWorksheetId=id;source=null;ResetRoles("Worksheet changed. Choose roles and formula terms for this worksheet.");
        return Schedule(false);
    }
    public Task PreviewAsync()=>CanPreview?Schedule(true):Task.CompletedTask;
    public void DocumentChanged()
    {
        if(disposed)return;
        generation++;pending?.Cancel();Retire();Worksheets=null;source=null;SelectedWorksheetId="";ClearPreview();
        ResetRoles("Document readiness changed. Variables and roles must be checked again.");
        Code="user.document-changed";Status=SpecificationStatus.DocumentChanged;Notify();
    }
    public void Cancel()
    {
        if(disposed)return;
        generation++;pending?.Cancel();ClearPreview();Retire();Code="user.cancelled";Status=SpecificationStatus.DraftChanged;Notify();
    }
    private Task Schedule(bool compile)
    {
        if(disposed || RequiresRecovery)return Task.CompletedTask;
        var version=++generation;scheduledVersion=version;pending?.Cancel();ClearPreview();Busy=true;Status=SpecificationStatus.Busy;Code="spec.discovering";
        operation=RunAfter(operation,version,compile);Notify();return operation;
    }
    private async Task RetireAfter(Task prior)
    {try{await prior;}catch(Exception){}await host.InvalidateAsync();}
    private Task Retire(){retirement=RetireAfter(retirement);return retirement;}
    private async Task RunAfter(Task prior,long version,bool compile)
    {
        try{await prior;}catch(Exception){ /* Retry cleanup; never authorize an old preview. */ }
        if(disposed || version!=generation)return;
        using var bound=CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);bound.CancelAfter(TimeSpan.FromSeconds(30));pending=bound;
        bool Accept()=>!disposed && version==generation && !bound.IsCancellationRequested;
        try
        {
            await Retire().WaitAsync(bound.Token);
            if(!Accept())return;
            if(discovery is null){Reject("office.unsupported-api");return;}
            var reply=await discovery.GetWorksheetsAsync(owner,bound.Token);
            if(!Accept())return;
            if(reply?.Code!="workbook.ok"){Reject(reply?.Code);return;}
            var catalog=WorksheetDiscoveryValidation.Catalog(reply!);
            var documentChanged=Worksheets is not null && Worksheets.DocumentIdentity!=catalog.DocumentIdentity;
            if(documentChanged){source=null;SelectedWorksheetId="";ResetRoles("Workbook changed. Choose roles for the active worksheet.");}
            Worksheets=catalog;
            if(SelectedWorksheetId.Length==0)SelectedWorksheetId=catalog.ActiveWorksheetId;
            var sheet=catalog.Worksheets.FirstOrDefault(s=>s.Id==SelectedWorksheetId);
            if(sheet is null){source=null;ResetRoles("The source worksheet was removed. Choose a worksheet.");Reject("user.document-changed");return;}
            var bounds=await discovery.GetWorksheetBoundsAsync(owner,catalog.DocumentIdentity,sheet.Id,bound.Token);
            if(!Accept())return;
            if(bounds?.Code!="workbook.ok"){Reject(bounds?.Code);return;}
            var reference=WorksheetDiscoveryValidation.Bounds(bounds!,catalog,sheet);
            if(reference is null){source=null;ResetRoles("This worksheet has no variables. Add data, or choose another worksheet.");Code="spec.empty-sheet";Status=SpecificationStatus.DraftChanged;return;}
            var read=await workbook.ReadRangeAsync(reference,cancellationToken:bound.Token);
            if(!Accept())return;
            if(!read.Succeeded){Reject(read.Code);return;}
            var snapshot=read.Value!;
            if(snapshot.WorksheetId!=sheet.Id || snapshot.WorksheetName!=sheet.Name || snapshot.StartRow!=reference.StartRow || snapshot.StartColumn!=reference.StartColumn || snapshot.RowCount!=reference.RowCount || snapshot.ColumnCount!=reference.ColumnCount)
            {Reject("user.document-changed");return;}
            var fresh=new WorksheetColumnCatalog(catalog.DocumentIdentity,snapshot);
            var changed=source is not null && !source.SameSource(fresh);
            if(changed)ResetRoles("Source data or columns changed. Roles, factor references and terms were reset; choose them again.");
            else if(source is null)ResetRoles("Variables discovered. Choose roles for this worksheet.");
            source=fresh;
            if(source.Columns.Count==0){Code="spec.empty-sheet";Status=SpecificationStatus.DraftChanged;return;}
            var pin=await host.PinSourceAsync(source.PhysicalCopy(),bound.Token);
            if(!Accept())return;
            if(pin?.Code!="workbook.ok" || string.IsNullOrEmpty(pin.SourceToken) || string.IsNullOrEmpty(pin.SessionIdentity)){Reject(pin?.Code=="workbook.ok"?"host.invalid-response":pin?.Code);return;}
            // Re-observe bounds/document after pin to catch growth outside the original rectangle.
            var after=await discovery.GetWorksheetBoundsAsync(owner,catalog.DocumentIdentity,sheet.Id,bound.Token);
            if(!Accept())return;
            if(after?.Code!="workbook.ok"){Reject(after?.Code);return;}
            var finalReference=WorksheetDiscoveryValidation.Bounds(after!,catalog,sheet);
            if(finalReference is null || finalReference.Address!=reference.Address || finalReference.WorksheetName!=reference.WorksheetName){Reject("user.document-changed");return;}
            SpecificationPreviewData? compiled=null;
            if(compile && !changed && !documentChanged)
            {
                await Task.Yield();if(!Accept())return;
                compiled=SpecificationCompilation.Compile(source,Draft);
            }
            var check=await host.CheckSourceAsync(pin.SourceToken,bound.Token);
            if(!Accept())return;
            if(check?.Code!="workbook.ok"){Reject(check?.Code);return;}
            if(compiled is not null)
            {
                Preview=compiled with {HostSessionIdentity=pin.SessionIdentity};
                FactorLevels=(Preview.Specification as SpreadsheetModelInput)?.CategoricalColumns??[];
                Status=Preview.Valid?SpecificationStatus.Ready:SpecificationStatus.Invalid;Code=Preview.Valid?"spec.ready":"spec.invalid";
            }
            else{Status=SpecificationStatus.DraftChanged;Code=changed||documentChanged?"spec.variables-reset":"spec.variables-ready";}
        }
        catch(OperationCanceledException){if(!disposed && version==generation)Reject("input.time-budget");}
        catch(WorkbookTransportException ex){if(!disposed && version==generation)Reject(ex.Code);}
        catch(Exception){if(!disposed && version==generation)Reject("host.invalid-response");}
        finally{if(pending==bound)pending=null;if(!disposed && scheduledVersion==version){Busy=false;Notify();}}
    }
    private void Reject(string? code){ClearPreview();Retire();Code=WorksheetDiscoveryValidation.Safe(code);Status=Code=="user.document-changed"?SpecificationStatus.DocumentChanged:SpecificationStatus.Invalid;}
    public async ValueTask DisposeAsync()
    {
        if(disposed)return;
        disposed=true;generation++;pending?.Cancel();lifetime.Cancel();Status=SpecificationStatus.Disposed;
        Draft.Changed-=DraftChanged;host.Invalidated-=HostChanged;if(discovery is not null)discovery.WorksheetsInvalidated-=CatalogChanged;
        await operation;
        await Retire();if(discovery is not null)await discovery.StopWorksheetDiscoveryAsync(owner);
        source=null;Worksheets=null;Changed=null;Busy=false;lifetime.Dispose();
    }
}
