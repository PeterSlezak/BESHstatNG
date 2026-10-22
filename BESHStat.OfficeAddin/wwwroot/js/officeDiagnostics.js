// Internal opt-in developer diagnostic. No Office APIs, network requests or raw errors.
(function (scope) {
    "use strict";
    const enabled = new URLSearchParams(scope.location.search).get("beshOfficeDiagnostics") === "1";
    const codes = new Set([
        "bootstrap.office-script-missing-or-failed", "bootstrap.office-script-loaded",
        "bootstrap.office-dependency-failed-or-blocked", "bootstrap.onready-started",
        "bootstrap.onready-resolved", "bootstrap.onready-timeout", "bootstrap.interop-import-failed",
        "bootstrap.interop-create-failed", "bootstrap.interop-invoke-failed",
        "bootstrap.interop-imported", "bootstrap.interop-created", "bootstrap.interop-invoked", "bootstrap.standalone",
        "bootstrap.excel-ready", "bootstrap.script-execution-error", "bootstrap.unhandled-rejection"
    ]);
    const started = scope.performance.now();
    const workbookCodes = new Set([
        "read.metadata-started", "read.full-started", "read.readiness-started", "read.readiness-completed", "read.readiness-rejected",
        "read.metadata-run-started", "read.metadata-run-entered", "read.metadata-sync-started", "read.metadata-sync-completed", "read.metadata-validated",
        "read.tile-run-started", "read.tile-run-entered", "read.preflight-sync-started", "read.preflight-sync-completed", "read.preflight-validated",
        "read.payload-sync-started", "read.payload-sync-completed", "read.reconcile-sync-started", "read.reconcile-sync-completed", "read.reconcile-validated",
        "read.final-run-started", "read.final-run-entered", "read.final-sync-started", "read.final-sync-completed", "read.final-validated",
        "read.completed", "read.js-timeout", "read.rejected", "read.cancelled"
    ]);
    let workbookDevelopment = false;
    const events = [];
    function record(code) {
        if (enabled && (codes.has(code) || (workbookDevelopment && workbookCodes.has(code)))) {
            if (events.length === 80) events.shift();
            events.push({ code, elapsedMs: Math.round(scope.performance.now() - started) });
        }
    }
    function resourceCategory(value) {
        try {
            const url = new URL(value);
            if (!["officeapis.public.onecdn.static.microsoft", "appsforoffice.microsoft.com"].includes(url.hostname)) return "";
            // CSP can redact a blocked URI to its origin. That alone cannot
            // distinguish a primary failure from a failed secondary script.
            if (!/\.js$/i.test(url.pathname)) return "";
            return /\/office\.js$/i.test(url.pathname) ? "office-primary"
                : /office_strings/i.test(url.pathname) ? "office-strings" : "office-dependency";
        } catch { return ""; }
    }
    function loaded(event) {
        if (event.target?.id === "office-js") record("bootstrap.office-script-loaded");
    }
    function error(event) {
        if (event.target?.id === "office-js") record("bootstrap.office-script-missing-or-failed");
        else if (event.target?.tagName === "SCRIPT" && resourceCategory(event.target.src))
            record("bootstrap.office-dependency-failed-or-blocked");
        else if (event.target === scope) record("bootstrap.script-execution-error");
    }
    function csp(event) {
        const category = resourceCategory(event.blockedURI);
        if (category) record(category === "office-primary" ? "bootstrap.office-script-missing-or-failed"
            : "bootstrap.office-dependency-failed-or-blocked");
    }
    const rejection = () => record("bootstrap.unhandled-rejection");
    function stop() {
        scope.removeEventListener("load", loaded, true);
        scope.removeEventListener("error", error, true);
        scope.removeEventListener("securitypolicyviolation", csp);
        scope.removeEventListener("unhandledrejection", rejection);
        scope.removeEventListener("pagehide", stop);
    }
    scope.__beshOfficeDebug = Object.freeze({ record, stop,
        enableWorkbookDevelopment() { if (enabled) workbookDevelopment = true; },
        snapshot() {
        const local = scope.location.origin === "https://localhost:7141";
        const frame = local ? (scope.top === scope ? "localhost-standalone" : "localhost-iframe") : "other";
        return { enabled, frame, events: events.map(event => ({ ...event })),
            resources: enabled ? scope.performance.getEntriesByType("resource").flatMap(entry => {
                const category = resourceCategory(entry.name);
                return category ? [{ category, startMs: Math.round(entry.startTime),
                    durationMs: Math.round(entry.duration),
                    status: Number.isInteger(entry.responseStatus) ? entry.responseStatus : null }] : [];
            }).slice(-40) : [] };
    } });
    if (enabled) {
        scope.addEventListener("load", loaded, true);
        scope.addEventListener("error", error, true);
        scope.addEventListener("securitypolicyviolation", csp);
        scope.addEventListener("unhandledrejection", rejection);
        scope.addEventListener("pagehide", stop, { once: true });
    }
})(globalThis);
