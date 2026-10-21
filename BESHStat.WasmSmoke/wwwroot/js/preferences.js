(function (root) {
    "use strict";
    // Prototype preferences transport; independent of every Office output wire.
    const key = "eu.beshstat.wasmsmoke.preferences.v1";
    const own = (value, names) => value && typeof value === "object" && !Array.isArray(value) &&
        Object.keys(value).every(name => names.includes(name));
    const optional = (value, predicate) => value === undefined || value === null || predicate(value);
    const integer = value => Number.isInteger(value) && value >= -2147483648 && value <= 2147483647;
    function inspect(data) {
        if (!own(data, ["schemaVersion", "defaultAlpha", "defaultRandomSeed", "traceEnabled", "pValues"])) return "invalid";
        if (!Number.isInteger(data.schemaVersion)) return "invalid";
        if (data.schemaVersion !== 1) return "unsupported";
        if (!optional(data.defaultAlpha, value => typeof value === "number" && Number.isFinite(value)) ||
            !optional(data.defaultRandomSeed, integer) || !optional(data.traceEnabled, value => typeof value === "boolean")) return "invalid";
        const display = data.pValues;
        if (display !== null && display !== undefined &&
            (!own(display, ["decimalPlaces", "smallValueDisplay", "useUpperBound"]) ||
                !optional(display.decimalPlaces, integer) || !optional(display.smallValueDisplay, integer) ||
                !optional(display.useUpperBound, value => typeof value === "boolean"))) return "invalid";
        return "valid";
    }
    function decode(raw) {
        if (raw === null) return { status: "missing", data: null };
        try {
            const envelope = JSON.parse(raw);
            if (!own(envelope, ["storageVersion", "preferences"]) || !Number.isInteger(envelope.storageVersion)) return { status: "invalid", data: null };
            if (envelope.storageVersion !== 1) return { status: "unsupported", data: null };
            const status = inspect(envelope.preferences);
            return { status, data: status === "valid" ? envelope.preferences : null };
        } catch (_) { return { status: "invalid", data: null }; }
    }
    root.beshPreferences = {
        load() {
            try { return decode(root.localStorage.getItem(key)); }
            catch (_) { return { status: "unavailable", data: null }; }
        },
        save(data) {
            const status = inspect(data);
            if (status !== "valid") return { status, data: null };
            try {
                const existing = decode(root.localStorage.getItem(key));
                // Never replace a corrupted or incompatible blob, even on explicit Save.
                if (existing.status !== "valid" && existing.status !== "missing") return existing;
                // Exact allowlist above excludes source IDs, consent and workbook data.
                const envelope = JSON.stringify({ storageVersion: 1, preferences: data });
                root.localStorage.setItem(key, envelope);
                // Detect storage implementations that silently discard writes.
                if (root.localStorage.getItem(key) !== envelope) return { status: "unavailable", data: null };
                return { status: "saved", data: null };
            } catch (_) { return { status: "unavailable", data: null }; }
        }
    };
})(window);
