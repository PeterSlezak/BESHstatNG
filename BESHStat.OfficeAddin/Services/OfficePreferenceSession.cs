using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

/// <summary>One scoped effective owner. Serialized load/save; application is explicit and atomic.</summary>
public sealed class OfficePreferenceSession(IPreferenceStore store) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1);
    private readonly InMemoryPreferenceStore session = new();
    private readonly List<Participant> participants = [];
    private Task? initialization;
    private bool disposed;
    public PreferenceSnapshot Effective { get; private set; } = PreferenceSnapshot.Defaults();
    public bool ForceEnglish { get; private set; } = true;
    public PreferenceResult State { get; private set; } = PreferenceSnapshot.Normalize(null);
    public long Revision { get; private set; }
    public bool Initialized { get; private set; }
    public bool Applying { get; private set; }
    public bool CanApply => !disposed && !Applying && participants.All(p => p.Ready());
    public event Action? Changed;
    public Task InitializeAsync() => initialization ??= LoadAsync();
    private async Task LoadAsync()
    {
        await gate.WaitAsync();
        try {
            var loaded = await SafeLoadAsync();
            if (disposed) return;
            var result = loaded.Preferences;
            if (result.CanUseStoredData) { Effective = result.Snapshot; ForceEnglish = loaded.ForceEnglish; }
            State = new(Effective, result.Status, result.Persistence, result.Codes);
            Initialized = true;
            Notify();
        } finally { gate.Release(); }
    }
    private async Task<OfficePreferenceResult> SafeLoadAsync()
    {
        try { return store is IOfficePreferenceStore office ? await office.LoadOfficeAsync() : new(await store.LoadAsync()); }
        catch { return new(new(Effective, PreferenceStatus.StorageUnavailable, PreferencePersistence.Unavailable)); }
    }
    public IDisposable Participate(Func<bool> ready, Func<Task> retire)
    {
        var participant = new Participant(ready, retire, participants);
        participants.Add(participant); return participant;
    }
    private sealed record Participant(Func<bool> Ready, Func<Task> Retire, List<Participant> Owners) : IDisposable
    { public void Dispose() => Owners.Remove(this); }

    public async Task<bool> SaveAsync(PreferenceSnapshot candidate, bool sessionOnly = false, bool? forceEnglish = null)
    {
        await InitializeAsync();
        // Reject rather than queue a stale click behind a submitted native write.
        if (!CanApply || !await gate.WaitAsync(0)) return false;
        Applying = true; Notify();
        try {
            var candidateLanguage = forceEnglish ?? ForceEnglish;
            // Retire BEFORE touching storage. A failed retirement must not leak a failed Save on reload.
            try { foreach (var owner in participants.ToArray()) await owner.Retire(); }
            catch { State = new(Effective, PreferenceStatus.Failed, PreferencePersistence.Unavailable); return false; }
            if (disposed) return false;
            PreferenceResult saved;
            try {
                if (sessionOnly) saved = await session.SaveAsync(candidate);
                else if (store is IOfficePreferenceStore office) saved = (await office.SaveOfficeAsync(candidate, candidateLanguage)).Preferences;
                else if (candidateLanguage != ForceEnglish) saved = new(Effective, PreferenceStatus.Failed, PreferencePersistence.Unavailable);
                else saved = await store.SaveAsync(candidate);
            }
            catch { saved = new(Effective, PreferenceStatus.StorageUnavailable, PreferencePersistence.Unavailable); }
            if (saved.Status != PreferenceStatus.Saved || (!sessionOnly && saved.Persistence != PreferencePersistence.Persisted)) {
                State = new(Effective, saved.Status, saved.Persistence, saved.Codes); return false;
            }
            Effective = saved.Snapshot;
            ForceEnglish = candidateLanguage;
            State = saved;
            Revision++;
            return true;
        } finally { Applying = false; gate.Release(); Notify(); }
    }
    private void Notify()
    {
        if (disposed || Changed is null) return;
        foreach (Action observer in Changed.GetInvocationList()) try { observer(); } catch { }
    }
    // Once persistence is submitted it completes coherently, even if the pane closes.
    // Closing before submission prevents the write; observers are never called after disposal.
    public void Dispose() { disposed = true; Changed = null; }
}
