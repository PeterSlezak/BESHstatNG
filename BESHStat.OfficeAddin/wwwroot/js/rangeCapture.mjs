// Metadata only: no values/formulas, A1 parsing, range selection or workbook mutations.
const validText = (s, n) => typeof s === "string" && s.length > 0 && s.length <= n && !/[\u0000-\u001f\u007f]/.test(s);
const column = n => { let s = ""; for (; n; n = Math.floor((n - 1) / 26)) s = String.fromCharCode(65 + (n - 1) % 26) + s; return s; };
const local = m => `${column(m.startColumn)}${m.startRow}:${column(m.startColumn + m.columnCount - 1)}${m.startRow + m.rowCount - 1}`;
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const wholeColumns = m => m.startRow === 1 && m.rowCount === 1048576 && m.columnCount < 16384;
const reject = code => { throw Object.assign(new Error(code), { safeCode: code }); };

export function createRangeCapture(scope, hooks) {
    let disposed = false, capture = null, nativeBusy = false;
    const references = new Map();
    const document = () => scope.Office?.context?.document;
    function metadata(range, single) {
        const m = { worksheetId: range.worksheet.id, worksheetName: range.worksheet.name, startRow: range.rowIndex + 1,
            startColumn: range.columnIndex + 1, rowCount: range.rowCount, columnCount: range.columnCount };
        if (!validText(m.worksheetId, 1024) || !validText(m.worksheetName, 256) ||
            !Object.values(m).slice(2).every(n => Number.isSafeInteger(n) && n > 0) ||
            m.startRow + m.rowCount - 1 > 1048576 || m.startColumn + m.columnCount - 1 > 16384 ||
            (single ? m.rowCount !== 1 || m.columnCount !== 1 : !wholeColumns(m) && m.rowCount * m.columnCount > 10000)) reject("input.invalid-reference");
        return m;
    }
    async function native(action) {
        if (disposed || nativeBusy) reject("host.busy");
        if (!document() || typeof scope.Excel?.run !== "function" || !scope.crypto?.randomUUID ||
            scope.Office?.context?.requirements?.isSetSupported("ExcelApi", "1.1") !== true) reject("office.unavailable");
        nativeBusy = true; const doc = document(); let timer, expired = false, settled = false;
        const check = () => { if (disposed || expired || document() !== doc) reject("user.document-changed"); };
        const pending = Promise.resolve().then(() => scope.Excel.run(async context => { check(); const r = await action(context, check); check(); return r; }));
        pending.then(() => { settled = true; if (expired) nativeBusy = false; }, () => { settled = true; if (expired) nativeBusy = false; });
        try { return await Promise.race([pending,
            new Promise((_, no) => { timer = setTimeout(() => { expired = true; no(Object.assign(new Error(), { safeCode: "input.time-budget" })); }, 5000); })]); }
        finally { clearTimeout(timer); if (settled) nativeBusy = false; }
    }
    function load(range) { range.load("rowIndex,columnIndex,rowCount,columnCount"); range.worksheet.load("id,name"); return range; }
    async function normalize(context, range, original, check) {
        if (!wholeColumns(original)) return original;
        if (typeof range.getUsedRangeOrNullObject !== "function" || typeof range.getSpecialCellsOrNullObject !== "function" ||
            scope.Office.context.requirements.isSetSupported("ExcelApi", "1.9") !== true) reject("office.unsupported-api");
        const used = range.getUsedRangeOrNullObject(true);
        used.load("rowIndex,columnIndex,rowCount,columnCount");
        const formulas = range.getSpecialCellsOrNullObject("Formulas");
        formulas.load("areaCount");
        await context.sync(); check();
        const spans = [];
        if (typeof used.isNullObject !== "boolean" || typeof formulas.isNullObject !== "boolean") reject("host.invalid-response");
        if (!used.isNullObject) spans.push({rowIndex:used.rowIndex,rowCount:used.rowCount});
        // Formula metadata covers empty-string results even on hosts that omit them from values-only used bounds.
        // Load at most 64 area geometries, never their cell matrices or original full-column payload.
        if (!formulas.isNullObject) {
            if (!Number.isSafeInteger(formulas.areaCount) || formulas.areaCount < 1) reject("host.invalid-response");
            if (formulas.areaCount > 64) reject("input.range-count");
            formulas.areas.load("items/rowIndex,items/rowCount"); await context.sync(); check();
            if (!Array.isArray(formulas.areas.items) || formulas.areas.items.length !== formulas.areaCount) reject("user.document-changed");
            spans.push(...formulas.areas.items.map(r => ({rowIndex:r.rowIndex,rowCount:r.rowCount})));
        }
        if (!spans.length) reject("input.empty-columns");
        if (spans.some(s => !Number.isSafeInteger(s.rowIndex) || !Number.isSafeInteger(s.rowCount) || s.rowIndex < 0 || s.rowCount < 1 ||
            s.rowIndex + s.rowCount > 1048576)) reject("host.invalid-response");
        const first = Math.min(...spans.map(s => s.rowIndex)), last = Math.max(...spans.map(s => s.rowIndex + s.rowCount));
        const effective = { ...original, startRow: first + 1, rowCount: last - first };
        if (effective.rowCount * effective.columnCount > 10000) reject("input.range-cells");
        return effective;
    }
    const reference = m => ({ ...m, address: local(m), addressDialect: 1, workbookId: "", workbookName: "" });
    function remember(meta, owner = null, original = meta) {
        const handle = scope.crypto.randomUUID();
        references.set(handle, { document: document(), meta, original });
        while (references.size > 8) references.delete([...references].find(([, r]) => !r.pinned)[0]);
        return { code: "workbook.ok", owner, handle, reference: reference(meta), originalReference: wholeColumns(original) ? reference(original) : null };
    }
    async function selection(single, owner = null) {
        return native(async (context, check) => {
            const range = load(context.workbook.getSelectedRange()); await context.sync(); check();
            const original = metadata(range, single);
            return remember(single ? original : await normalize(context, range, original, check), owner, original);
        });
    }
    async function resolve(reference, handle = null) {
        if (!reference || reference.workbookId || reference.workbookName || reference.addressDialect !== 1 || !validText(reference.worksheetName, 256)) reject("input.invalid-reference");
        const expected = references.get(handle);
        if (handle && (!expected || expected.document !== document())) reject("user.document-changed");
        return native(async (context, check) => {
            // Integer coordinates came from Common. Never evaluate the original user string in JS.
            const shape = { worksheetId: "pending", worksheetName: reference.worksheetName, startRow: reference.startRow,
                startColumn: reference.startColumn, rowCount: reference.rowCount, columnCount: reference.columnCount };
            if (![shape.startRow, shape.startColumn, shape.rowCount, shape.columnCount].every(n => Number.isSafeInteger(n) && n > 0) ||
                shape.startRow + shape.rowCount - 1 > 1048576 || shape.startColumn + shape.columnCount - 1 > 16384 ||
                (!wholeColumns(shape) && shape.rowCount * shape.columnCount > 10000) || reference.address !== local(shape)) reject("input.invalid-reference");
            const sheet = context.workbook.worksheets.getItem(expected?.meta.worksheetId || reference.worksheetName);
            const range = load(sheet.getRange(local(shape))); await context.sync(); check();
            const original = metadata(range, false);
            const m = await normalize(context, range, original, check);
            // Explicit reacquisition can refresh a whole-column used footprint, while retaining
            // the original physical sheet/selection identity. Existing pinned leases still fail stale.
            if (m.worksheetName !== reference.worksheetName || (expected && !wholeColumns(expected.original) && !same(m, expected.meta))) reject("user.document-changed");
            if (expected && !same(original, expected.original ?? expected.meta)) reject("user.document-changed");
            return remember(m, null, original);
        });
    }
    function verify(handle, snapshot) {
        const record = references.get(handle);
        const m = { worksheetId: snapshot.worksheetId, worksheetName: snapshot.worksheetName, startRow: snapshot.startRow,
            startColumn: snapshot.startColumn, rowCount: snapshot.rowCount, columnCount: snapshot.columnCount };
        if (!record || record.document !== document() || !same(record.meta, m)) reject("user.document-changed");
        record.pinned = true;
        return record;
    }
    async function checkNormalization(record) {
        if (!record || !wholeColumns(record.original)) return;
        if (record.document !== document()) reject("user.document-changed");
        await native(async (context, check) => {
            const range = load(context.workbook.worksheets.getItem(record.original.worksheetId).getRange(local(record.original)));
            await context.sync(); check();
            const original = metadata(range, false);
            if (!same(original, record.original) || !same(await normalize(context, range, original, check), record.meta)) reject("user.document-changed");
        });
    }
    async function remove(item) {
        if (!item) return;
        item.cancelled = true;
        if (capture === item) capture = null;
        if (item.subscription) try { await scope.Excel.run(item.subscription.context, async context => { item.subscription.remove(); await context.sync(); }); } catch { /* Inert callback after close. */ }
    }
    async function pump(item) {
        if (item.processing) { item.again = true; return; }
        item.processing = true;
        try {
            do {
                item.again = false;
                let reply;
                try { reply = await selection(item.output, item.owner); }
                catch (e) { reply = { code: e.safeCode || "range.capture-unavailable", owner: item.owner }; }
                if (!disposed && capture === item && !item.cancelled)
                    try { await item.callback.invokeMethodAsync("RangeCapturedCallback", reply); } catch { /* Disposed owner. */ }
            } while (item.again && capture === item && !item.cancelled);
        } finally { item.processing = false; }
    }
    async function begin(owner, output, callback) {
        if (!validText(owner, 256) || typeof output !== "boolean" || !callback?.invokeMethodAsync || capture) reject("host.busy");
        if (scope.Office?.context?.requirements?.isSetSupported("ExcelApi", "1.9") !== true) reject("range.capture-unavailable");
        await hooks.begin(output);
        const item = { owner, output, callback, cancelled: false, subscription: null };
        capture = item;
        try {
            await native(async (context, check) => {
                if (capture !== item || item.cancelled) reject("user.cancelled");
                const event = context.workbook.worksheets.onSelectionChanged;
                if (typeof event?.add !== "function") reject("range.capture-unavailable");
                item.subscription = event.add(args => {
                    // Args are hints only: resolve the current selection + full native sheet name,
                    // never pass an event address into a second A1 grammar.
                    if (capture === item && !disposed && !item.cancelled) void pump(item);
                });
                await context.sync(); check();
            });
            if (capture !== item || item.cancelled) reject("user.cancelled");
            return { code: "workbook.ok", owner };
        } catch (e) { await remove(item); await hooks.end(output); throw e; }
    }
    async function end(owner, cancel) {
        const item = capture;
        if (!item || item.owner !== owner) return { code: "range.capture-cancelled", owner };
        await remove(item);
        // Drain the one bounded metadata request before the final explicit capture.
        while (item.processing) await new Promise(resolve => setTimeout(resolve, 5));
        try {
            const reply = cancel ? { code: "range.capture-cancelled", owner } : await selection(item.output, owner);
            await hooks.end(item.output, reply.reference); return reply;
        } catch (e) { await hooks.abort(); throw e; }
    }
    async function stop() {
        const item = capture; await remove(item);
        if (item) await hooks.abort();
    }
    async function dispose() { disposed = true; await stop(); references.clear(); }
    function bind(handle, meta, record = null) {
        for (const record of references.values()) record.pinned = false;
        references.set(handle, { document: document(), meta, original: record?.original ?? meta, pinned: true });
        while (references.size > 8) references.delete([...references].find(([, r]) => !r.pinned)[0]);
    }
    // Keep bounded read-only identity hints for Refresh, without an active source lease.
    function retire() { for (const record of references.values()) record.pinned = false; }
    return { selection, resolve, verify, checkNormalization, bind, retire, begin, end, stop, dispose, get armed() { return capture !== null; } };
}
