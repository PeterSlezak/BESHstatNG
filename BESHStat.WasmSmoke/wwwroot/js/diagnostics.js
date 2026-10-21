"use strict";
(function () {
    const events = Object.freeze({
        "diagnostic.run.started": "information",
        "diagnostic.validation.rejected": "warning",
        "diagnostic.output.attempted": "information",
        "diagnostic.output.completed": "information",
        "diagnostic.output.incomplete": "warning",
        "diagnostic.run.failed": "error",
        "diagnostic.run.ended": "information",
        "diagnostic.stage.entered": "debug",
        "diagnostic.event.redacted": "warning"
    });
    const allowed = (value, list) => typeof value === "string" && value.length <= 64 && list.includes(value) ? value : "redacted";
    // Defense in depth at the host boundary; never console.log the incoming object.
    function emit(source, traceEnabled = true) {
        try {
            if (!source || typeof source.runId !== "string" || source.runId.length !== 32 || !/^[a-f0-9]{32}$/.test(source.runId)) return false;
            const code = typeof source.eventCode === "string" && Object.hasOwn(events, source.eventCode)
                ? source.eventCode : "diagnostic.event.redacted";
            const severity = events[code];
            if (severity === "debug" && traceEnabled !== true) return true;
            const metadata = {};
            const fields = {
                category: ["input", "host", "analysis", "output", "unexpected"],
                outcome: ["completed", "rejected", "failed", "cancelled", "incomplete", "ended"],
                host: ["windows", "office"]
            };
            for (const key of Object.keys(fields)) {
                if (source.metadata && Object.hasOwn(source.metadata, key))
                    metadata[key] = allowed(source.metadata[key], fields[key]);
            }
            const safe = Object.freeze({ eventCode: code, severity, runId: source.runId,
                operation: allowed(source.operation, ["anova.oneway", "nonparametric.kruskal-wallis"]),
                stage: allowed(source.stage, ["invocation", "validation", "acquisition", "analysis", "output"]),
                metadata: Object.freeze(metadata) });
            const method = severity === "error" ? "error" : severity === "warning" ? "warn" : severity === "debug" ? "debug" : "info";
            const target = window.console;
            if (!target || typeof target[method] !== "function") return false;
            target[method](safe);
            return true;
        } catch { return false; }
    }
    window.beshDiagnostics = Object.freeze({ emit });
}());
