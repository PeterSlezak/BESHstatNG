using BESHStatNG.ExcelCommon;

namespace BESHStat.OfficeAddin.Services;

public sealed record OfficePreferenceResult(PreferenceResult Preferences, bool ForceEnglish = true);
public interface IOfficePreferenceStore : IPreferenceStore
{
    Task<OfficePreferenceResult> LoadOfficeAsync();
    Task<OfficePreferenceResult> SaveOfficeAsync(PreferenceSnapshot snapshot, bool forceEnglish);
}

/// <summary>Office browser adapter to the authoritative Common schema. Never applies implicitly.</summary>
public sealed class OfficePreferenceStore(IOfficePreferenceStorage storage) : IOfficePreferenceStore
{
    public async Task<PreferenceResult> LoadAsync() => (await LoadOfficeAsync()).Preferences;
    public async Task<OfficePreferenceResult> LoadOfficeAsync()
    {
        try {
            var reply = await storage.LoadAsync();
            if (reply.Status == "missing") return new(PreferenceSnapshot.Normalize(null));
            if (reply.Status != "loaded" || reply.Data is null) return new(Failure(reply.Status));
            if (reply.ForceEnglish is null) return new(Failure("invalid"));
            var normalized = PreferenceSnapshot.Normalize(reply.Data);
            return new(new(normalized.Snapshot, normalized.Status,
                normalized.Status == PreferenceStatus.Loaded ? PreferencePersistence.Persisted : PreferencePersistence.SessionOnly, normalized.Codes),
                normalized.CanUseStoredData ? reply.ForceEnglish.Value : true);
        } catch { return new(Failure("unavailable")); }
    }
    public async Task<PreferenceResult> SaveAsync(PreferenceSnapshot snapshot) => (await SaveAsync(snapshot, null)).Preferences;
    public Task<OfficePreferenceResult> SaveOfficeAsync(PreferenceSnapshot snapshot, bool forceEnglish) => SaveAsync(snapshot, forceEnglish);
    private async Task<OfficePreferenceResult> SaveAsync(PreferenceSnapshot snapshot, bool? forceEnglish)
    {
        try {
            var reply = await storage.SaveAsync(snapshot.ToData(), forceEnglish);
            return reply.Status == "saved" ? new(new(snapshot, PreferenceStatus.Saved, PreferencePersistence.Persisted), forceEnglish ?? reply.ForceEnglish ?? true) : new(Failure(reply.Status));
        } catch { return new(Failure("unavailable")); }
    }
    private static PreferenceResult Failure(string status) => new(PreferenceSnapshot.Defaults(), status switch {
        "unsupported" => PreferenceStatus.UnsupportedVersion, "invalid" => PreferenceStatus.InvalidData,
        _ => PreferenceStatus.StorageUnavailable }, PreferencePersistence.Unavailable);
}
