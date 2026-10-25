let allowed = new Set(), routes = new Set(), resolver = '', published = false, configured = false;
export function configureHelp(urls, resolverUrl, resolverPublished, routeKeys) {
    if (configured) return;
    allowed = new Set(urls.filter(u => typeof u === 'string' && u.startsWith('https://')));
    routes = new Set(routeKeys);
    resolver = resolverUrl; published = resolverPublished === true; configured = true;
}
export function displayLocale(environment = globalThis) {
    try {
        return environment.Office?.context?.displayLanguage || environment.navigator?.language || 'en';
    } catch { return 'en'; }
}
export function safeHelp(url) {
    if (allowed.has(url)) return true;
    if (!published || typeof url !== 'string' || !url.startsWith(resolver + '?version=')) return false;
    // Common validated this exact route/version. Bridge also rejects arbitrary origins/query injection.
    const match = url.slice(resolver.length).match(/^\?version=(\d{1,5}\.){3}\d{1,5}&route=([A-Za-z][A-Za-z0-9]*)$/);
    if (!match || !routes.has(match[2])) return false;
    const parts = url.split('version=')[1].split('&')[0].split('.');
    return parts.some(v => Number(v) > 0) && parts.every(v => Number(v) <= 65535 && String(Number(v)) === v);
}
export function openHelp(url, environment = globalThis) {
    if (!safeHelp(url)) return 'rejected';
    try {
        const ui = environment.Office?.context?.ui;
        if (typeof ui?.openBrowserWindow === 'function') {
            ui.openBrowserWindow(url);
            // Office's void API supplies no page-load confirmation.
            return 'requested';
        }
        // Standalone fallback only. In Office expose a deliberate HTTPS link in the UI.
        if (environment.Office?.context) return 'unavailable';
        const opened = environment.open?.(url, '_blank');
        if (!opened) return 'blocked';
        opened.opener = null;
        return 'requested';
    } catch { return 'blocked'; }
}
export async function copyLog(text, environment = globalThis) {
    if (typeof text !== 'string' || text.length > 131072) return false;
    try { await environment.navigator.clipboard.writeText(text); return true; } catch { return false; }
}
