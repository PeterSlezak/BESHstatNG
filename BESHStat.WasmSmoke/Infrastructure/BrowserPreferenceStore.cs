using BESHStatNG.ExcelCommon;
using Microsoft.JSInterop;

namespace BESHStat.WasmSmoke.Infrastructure;

// Browser APIs remain in JS. Only the allowlisted transport reaches Common.
public sealed class BrowserPreferenceStore(IJSRuntime js) : IPreferenceStore
{
    public async Task<PreferenceResult> LoadAsync()
    {
        BrowserReply reply;
        try { reply = await js.InvokeAsync<BrowserReply>("beshPreferences.load"); }
        catch (JSException) { return Failure(PreferenceSnapshot.Defaults(), "unavailable"); }
        if (reply.Status == "missing") return PreferenceSnapshot.Normalize(null);
        if (reply.Status != "valid") return Failure(PreferenceSnapshot.Defaults(), reply.Status);
        var result = PreferenceSnapshot.Normalize(reply.Data);
        return new PreferenceResult(result.Snapshot, result.Status,
            result.Status == PreferenceStatus.Loaded ? PreferencePersistence.Persisted : PreferencePersistence.SessionOnly, result.Codes);
    }

    public async Task<PreferenceResult> SaveAsync(PreferenceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        BrowserReply reply;
        try { reply = await js.InvokeAsync<BrowserReply>("beshPreferences.save", snapshot.ToData()); }
        catch (JSException) { return Failure(snapshot, "unavailable"); }
        return reply.Status == "saved"
            ? new PreferenceResult(snapshot, PreferenceStatus.Saved, PreferencePersistence.Persisted)
            : Failure(snapshot, reply.Status);
    }

    private static PreferenceResult Failure(PreferenceSnapshot snapshot, string status) => new(snapshot,
        status == "unsupported" ? PreferenceStatus.UnsupportedVersion :
        status == "invalid" ? PreferenceStatus.InvalidData : PreferenceStatus.StorageUnavailable,
        status == "unavailable" ? PreferencePersistence.Unavailable : PreferencePersistence.SessionOnly,
        new[] { "preferences.browser." + (status is "unsupported" or "invalid" ? status : "unavailable") });

    public sealed class BrowserReply
    {
        public string Status { get; set; } = "unavailable";
        public PreferenceData? Data { get; set; }
    }
}
