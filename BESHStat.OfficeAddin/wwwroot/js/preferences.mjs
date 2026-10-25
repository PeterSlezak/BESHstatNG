// Office browser origin-local preferences only. No workbook data or OfficeRuntime dependency.
export const preferenceKey = 'beshstatng.office.preferences.v1';
export const maximumPreferenceBytes = 4096;
const own = (v, k) => Object.prototype.hasOwnProperty.call(v, k);
const object = v => v !== null && typeof v === 'object' && !Array.isArray(v);
const only = (v, keys) => Object.keys(v).every(k => keys.includes(k));
export function inspectPreferenceJson(raw) {
    if (typeof raw !== 'string' || new TextEncoder().encode(raw).length > maximumPreferenceBytes) return { status: 'invalid' };
    let data;
    try { data = JSON.parse(raw); } catch { return { status: 'invalid' }; }
    if (!object(data) || !own(data, 'schemaVersion')) return { status: 'invalid' };
    if (data.schemaVersion !== 1) return { status: 'unsupported' };
    if (!only(data, ['schemaVersion', 'defaultAlpha', 'defaultRandomSeed', 'traceEnabled', 'pValues', 'forceEnglish'])) return { status: 'invalid' };
    if (own(data, 'forceEnglish') && typeof data.forceEnglish !== 'boolean') return { status: 'invalid' };
    const optional = (v, key, check) => !own(v, key) || v[key] === null || check(v[key]);
    if (!optional(data, 'defaultAlpha', v => typeof v === 'number' && Number.isFinite(v)) ||
        !optional(data, 'defaultRandomSeed', v => Number.isInteger(v) && v >= -2147483648 && v <= 2147483647) ||
        !optional(data, 'traceEnabled', v => typeof v === 'boolean')) return { status: 'invalid' };
    const p = data.pValues;
    if (p != null && (!object(p) || !only(p, ['decimalPlaces', 'smallValueDisplay', 'useUpperBound']) ||
        !optional(p, 'decimalPlaces', Number.isInteger) || !optional(p, 'smallValueDisplay', Number.isInteger) ||
        !optional(p, 'useUpperBound', v => typeof v === 'boolean'))) return { status: 'invalid' };
    const { forceEnglish = true, ...commonData } = data;
    return { status: 'loaded', data: commonData, forceEnglish };
}
export function loadPreferences(environment = globalThis) {
    try {
        const raw = environment.localStorage.getItem(preferenceKey);
        return raw === null ? { status: 'missing' } : inspectPreferenceJson(raw);
    } catch { return { status: 'unavailable' }; }
}
export function savePreferences(data, forceEnglish = undefined, environment = globalThis) {
    try {
        const storage = environment.localStorage;
        const existing = storage.getItem(preferenceKey);
        const previous = existing === null ? { forceEnglish: true } : inspectPreferenceJson(existing);
        if (existing !== null && previous.status !== 'loaded') return previous;
        const flag = forceEnglish == null ? previous.forceEnglish : forceEnglish;
        const raw = JSON.stringify({ ...data, forceEnglish: flag });
        const check = inspectPreferenceJson(raw);
        if (check.status !== 'loaded') return check;
        storage.setItem(preferenceKey, raw);
        try {
            if (storage.getItem(preferenceKey) === raw) return { status: 'saved', forceEnglish: flag };
        } catch {
            // Best-effort restore of the one key if verification itself becomes unavailable.
            try { if (existing === null) storage.removeItem(preferenceKey); else storage.setItem(preferenceKey, existing); } catch { }
        }
        return { status: 'unavailable' };
    } catch { return { status: 'unavailable' }; }
}
