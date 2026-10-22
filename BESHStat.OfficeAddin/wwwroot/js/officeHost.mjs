// This module never opens Excel.run or accesses Office.context.document.
const reply = (state, code, platform = "", officeReady = false, excelApiSupported = false) =>
    Object.freeze({ state, code, platform, officeReady, excelApiSupported });
const unavailable = () => reply("standalone", "office.unavailable");
const failed = () => reply("failed", "office.initialization-failed");
const cancelled = () => reply("disposed", "office.cancelled");

function mark(scope, code) {
    try { scope.__beshOfficeDebug?.record(code); } catch { /* Diagnostics cannot affect readiness. */ }
}

// One Office promise observer, with removable attempt subscribers. A never-settled
// Office promise retains only this record, not a disposed adapter or retry callbacks.
function observe(promise) {
    const record = { outcome: null, subscribers: new Set() };
    const complete = outcome => {
        record.outcome = outcome;
        for (const subscriber of record.subscribers) subscriber(outcome);
        record.subscribers.clear();
    };
    Promise.resolve(promise).then(info => complete({ info }), () => complete({ failed: true }));
    return record;
}

function isStandaloneWindow(scope) {
    // This is only negative evidence at the timeout boundary, never Excel proof.
    try { return scope.window === scope && scope.top === scope; } catch { return false; }
}

export function createOfficeHost(scope = globalThis, timeoutMilliseconds = 5000) {
    if (!scope || typeof scope !== "object") scope = {};
    // Tests may shorten the bound; production always uses 5 seconds.
    const timeout = Number.isFinite(timeoutMilliseconds) && timeoutMilliseconds > 0
        ? Math.min(timeoutMilliseconds, 5000) : 5000;
    let pending = null;
    let settled = false;
    let disposed = false;
    let cancelPending = null;
    let timer = null;
    let detach = () => {};
    let wait = null;
    let waitOffice = null;
    let waitMethod = null;

    function initialize(retry = false) {
        if (disposed) return Promise.resolve(cancelled());
        if (typeof retry !== "boolean") return Promise.resolve(failed());
        if (pending && !(retry && settled)) return pending;
        mark(scope, "bootstrap.interop-invoked");
        settled = false;
        pending = new Promise(resolve => {
            let finished = false;
            let officeSeen = false;
            let unsubscribe = () => {};
            function finish(result) {
                if (finished) return;
                finished = true;
                settled = true;
                if (timer !== null) clearTimeout(timer);
                timer = null;
                detach();
                detach = () => {};
                unsubscribe();
                cancelPending = null;
                resolve(result);
            }
            cancelPending = () => finish(cancelled());
            timer = setTimeout(() => {
                mark(scope, officeSeen ? "bootstrap.onready-timeout" : "bootstrap.office-script-missing-or-failed");
                if (!officeSeen || isStandaloneWindow(scope)) {
                    mark(scope, "bootstrap.standalone");
                    finish(unavailable());
                } else finish(reply("failed", "office.timeout"));
            }, timeout);

            function ready() {
                try {
                    const office = scope.Office;
                    officeSeen = !!office;
                    if (!office || typeof office.onReady !== "function") return finish(failed());
                    mark(scope, "bootstrap.office-script-loaded");
                    if (!wait || waitOffice !== office || waitMethod !== office.onReady || wait.outcome?.failed) {
                        mark(scope, "bootstrap.onready-started");
                        const promise = office.onReady();
                        if (!promise || typeof promise.then !== "function") return finish(failed());
                        wait = observe(promise);
                        waitOffice = office;
                        waitMethod = office.onReady;
                    }
                    const current = wait;
                    const project = outcome => {
                        if (finished || disposed) return;
                        try {
                            if (outcome.failed) return finish(failed());
                            mark(scope, "bootstrap.onready-resolved");
                            const info = outcome.info;
                            if (!info || (info.host == null && info.platform == null)) {
                                mark(scope, "bootstrap.standalone");
                                return finish(unavailable());
                            }
                            const platform = ["PC", "Mac", "OfficeOnline"].includes(info.platform) ? info.platform : "";
                            if (typeof info.host !== "string" || info.host !== "Excel")
                                return finish(reply("unsupported", "office.unsupported-host", platform, true));
                            // Only genuine onReady completion permits requirement metadata access.
                            const requirements = office.context?.requirements;
                            const supported = typeof requirements?.isSetSupported === "function" &&
                                requirements.isSetSupported("ExcelApi", "1.1") === true;
                            if (supported) mark(scope, "bootstrap.excel-ready");
                            finish(supported ? reply("excel", "office.excel-ready", platform, true, true)
                                : reply("unsupported", "office.unsupported-api", platform, true));
                        } catch { finish(failed()); }
                    };
                    unsubscribe = () => current.subscribers.delete(project);
                    if (current.outcome) project(current.outcome);
                    else current.subscribers.add(project);
                } catch { finish(failed()); }
            }

            try {
                if (scope.Office) { void ready(); return; }
                const script = scope.document?.getElementById("office-js");
                if (!script) { mark(scope, "bootstrap.office-script-missing-or-failed"); finish(unavailable()); return; }
                const loaded = () => { detach(); void ready(); };
                const error = () => { mark(scope, "bootstrap.office-script-missing-or-failed"); finish(unavailable()); };
                let attached = true;
                detach = () => {
                    if (!attached) return;
                    attached = false;
                    script.removeEventListener("load", loaded);
                    script.removeEventListener("error", error);
                };
                script.addEventListener("load", loaded, { once: true });
                script.addEventListener("error", error, { once: true });
            } catch { finish(failed()); }
        });
        return pending;
    }

    function dispose() {
        if (disposed) return;
        disposed = true;
        cancelPending?.();
        detach();
        wait = waitOffice = waitMethod = null;
    }
    return Object.freeze({ initialize, dispose });
}

// The object reference has pane-scoped ownership in .NET; no module singleton retains Office state.
export function create() {
    // InvokeAsync<IJSObjectReference> already wraps the returned object. Returning
    // a createJSObjectReference marker here wraps the marker a second time.
    mark(globalThis, "bootstrap.interop-imported");
    const adapter = createOfficeHost();
    mark(globalThis, "bootstrap.interop-created");
    return adapter;
}
