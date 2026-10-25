import { createRangeCapture } from "./rangeCapture.mjs";
import { tablePresenter, applyPresentation, loadPresentation, presentationMatches } from "./tablePresentation.mjs";
import { allocateWorksheetName, validWorksheetName, worksheetNameKey } from "./worksheetNames.mjs";

// Production session protocol v1. Common table wire v1 is consumed unchanged.
// Tokens prove continuity of this Office document object, never a durable workbook ID.
const MAX = 10000, CHUNK = 2000;
const fail = code => { throw Object.assign(new Error(code), { safeCode: code }); };
const boundedText = (s, max = 256) => typeof s === "string" && s.length > 0 && s.length <= max && !/[\u0000-\u001f\u007f]/.test(s);
const integer = (n, max) => Number.isSafeInteger(n) && n > 0 && n <= max;
const equal = (a, b) => JSON.stringify(a) === JSON.stringify(b);
function column(n) { let s = ""; for (; n; n = Math.floor((n - 1) / 26)) s = String.fromCharCode(65 + (n - 1) % 26) + s; return s; }
const area = b => `${column(b.startColumn)}${b.startRow}:${column(b.endColumn)}${b.endRow}`;
const box = b => b && integer(b.startRow, 1048576) && integer(b.endRow, 1048576) && integer(b.startColumn, 16384) && integer(b.endColumn, 16384) && b.endRow >= b.startRow && b.endColumn >= b.startColumn;
const intersects = (a, b) => a.startRow <= b.endRow && b.startRow <= a.endRow && a.startColumn <= b.endColumn && b.startColumn <= a.endColumn;
const code = e => e?.safeCode || (e?.code === "InvalidSelection" ? "input.invalid-selection"
    : ["ItemNotFound", "InvalidObjectPath"].includes(e?.code) ? "user.document-changed" : "host.operation-failed");
const outcome = (state, code, mutationMayHaveOccurred = false, confirmedWrittenIds = [], confirmationRequired = null, overwriteApproved = null) =>
    ({ state, code, mutationMayHaveOccurred, confirmedWrittenIds, confirmationRequired, overwriteApproved });

// Explicit, memory-only diagnostics. Never retain native arguments, exceptions or workbook data.
let traceEnabled = false, traceEntries = [], traceSequence = 0;
export function enableWriterTrace(enabled = true) { traceEnabled = enabled === true; traceEntries = []; traceSequence = 0; }
export function getWriterTrace() { return traceEntries.map(entry => ({ ...entry })); }
function trace(op, stage, detail = "") {
    if (!traceEnabled) return;
    traceEntries.push({ sequence: ++traceSequence, ms: Math.min(60000, Math.max(0, Math.round(performance.now() - (op?.started ?? performance.now())))),
        stage, detail, mutation: op?.mutation === true, invalidated: op?.invalidated === true });
    if (traceEntries.length > 256) traceEntries.shift();
}
function traceResult(op, result) {
    trace(op, "writer.result", result.code);
    if (traceEnabled) Object.assign(traceEntries.at(-1), { state: result.state, confirmedWritten: result.confirmedWrittenIds.length,
        mutationMayHaveOccurred: result.mutationMayHaveOccurred });
}
const representation = value => value === null ? "null" : typeof value === "string"
    ? /^#[0-9a-f]{6}$/i.test(value) ? "hex6" : /^[a-z]+$/i.test(value) ? "letters" : "other-string" : typeof value;
const displayed = v => v.kind === "blank" ? "" : v.kind === "number" ? v.numberValue : v.kind === "boolean" ? v.booleanValue : v.textValue;

export function matchesNativeOutputValue(actual, value, web = false) {
    const expected = displayed(value);
    if (value.kind !== "number") return actual === expected;
    if (typeof actual !== "number" || !Number.isFinite(actual) || !Number.isFinite(expected)) return false;
    if (actual === expected) return true;
    // Genuine Excel Online stores noninteger results at 15 significant digits.
    // Match only that exact round-trip, never an epsilon band or a coercion.
    // Integer loss and underflow remain failures. The wire/Core value is untouched.
    if (!web || Number.isInteger(expected)) return false;
    const native = Number(expected.toPrecision(15));
    return native !== 0 && Number.isFinite(native) && actual === native;
}

export function validateProductionPlan(plan, intent) {
    if (plan?.schemaVersion !== 1 || !intent || ![0, 1].includes(intent.destination) || plan.target?.destination !== intent.destination ||
        plan.target?.worksheetName !== intent.worksheetName || plan.target?.startRow !== intent.startRow || plan.target?.startColumn !== intent.startColumn ||
        !boundedText(intent.planIdentity) || !boundedText(intent.targetIdentity, 1024) || !boundedText(intent.workbookIdentity, 1024) ||
        !boundedText(intent.worksheetName, 31) || !Array.isArray(intent.regions) || !Array.isArray(plan.tables) || !plan.tables.length ||
        plan.tables.length > 64 || plan.tableCount !== plan.tables.length || plan.tables.length !== intent.regions.length ||
        intent.guards?.length || intent.reservations?.length) fail("output.rejected");
    let count = 0, bytes = 0;
    const ids = new Set();
    for (let i = 0; i < plan.tables.length; i++) {
        const t = plan.tables[i], r = intent.regions[i], b = t.bounds;
        if (!box(b) || !boundedText(t.artifactId) || ids.has(t.artifactId) || t.autoFit !== false || !Array.isArray(t.cells) ||
            t.rowCount !== b.endRow - b.startRow + 1 || t.columnCount !== b.endColumn - b.startColumn + 1 ||
            t.cells.length !== t.rowCount * t.columnCount || r.artifactId !== t.artifactId || r.mayMutate !== true || !equal(r.bounds, b) ||
            plan.tables.slice(0, i).some(p => intersects(p.bounds, b))) fail("output.rejected");
        ids.add(t.artifactId); count += t.cells.length;
        if (count > MAX) fail("output.rejected");
        for (let j = 0; j < t.cells.length; j++) {
            const c = t.cells[j], v = c.value;
            if (c.row !== Math.floor(j / t.columnCount) || c.column !== j % t.columnCount || !v ||
                !["blank", "number", "text", "boolean"].includes(v.kind) || typeof c.numberFormat !== "string" || c.numberFormat.length > 256 ||
                !["Body", "Title", "TopColumnHeader", "LeftRowHeader", "Footer"].includes(c.role) ||
                typeof c.strongEmphasis !== "boolean" || typeof c.isPValue !== "boolean" || typeof c.isSignificant !== "boolean" ||
                (v.kind === "number" && (typeof v.numberValue !== "number" || !Number.isFinite(v.numberValue))) ||
                (v.kind === "text" && (typeof v.textValue !== "string" || v.textValue.length > 32767)) ||
                (v.kind === "boolean" && typeof v.booleanValue !== "boolean")) fail("output.rejected");
            if (!equal(Object.keys(v).sort(), ["booleanValue", "kind", "numberValue", "textValue"]) ||
                (v.kind !== "number" && v.numberValue !== null) || (v.kind !== "text" && v.textValue !== null) ||
                (v.kind !== "boolean" && v.booleanValue !== null)) fail("output.rejected");
            bytes += 1024 + 2 * ((v.textValue?.length || 0) + c.numberFormat.length);
            if (bytes > 67108864) fail("output.rejected");
        }
    }
}

export function createProductionWriter(scope = globalThis, { timeoutMs = 30000 } = {}) {
    let disposed = false, busy = false, uncertain = false, epoch = 0, source = null, prepared = null, operation = null;
    let listeners = [], pickingOutput = false;
    const ranges = createRangeCapture(scope, {
        begin: async output => { prepared = null; if (output && source) { await observe({ cancelled: false }, source); pickingOutput = true; } },
        end: async (output, expected) => {
            if (output && source) {
                const pinned = source, op = { cancelled: false }; checkpoint(op, pinned);
                await scope.Excel.run(async context => {
                    const selected = loadSelection(context); await context.sync(); checkpoint(op, pinned);
                    const selection = meta(selected);
                    if (expected && Object.keys(selection).some(k => selection[k] !== expected[k])) fail("user.document-changed");
                    pinned.selection = selection;
                });
                await observe(op, pinned);
            }
            pickingOutput = false;
        },
        abort: async () => { pickingOutput = false; invalidate(); }
    });
    const document = () => scope.Office?.context?.document;
    const token = () => scope.crypto.randomUUID();
    function invalidate() { pickingOutput = false; epoch++; source = null; prepared = null; if (operation) { operation.cancelled = true; operation.invalidated = true; } }
    function checkpoint(op, pinned = source) {
        if (disposed || op.cancelled || op.timedOut) fail("user.cancelled");
        if (!pinned || pinned.epoch !== epoch || document() !== pinned.document) fail("user.document-changed");
    }
    async function guarded(action, writing = false, capture = false) {
        if (capture) trace(operation, "capture.guard", uncertain ? "uncertain" : busy ? "busy" : disposed ? "disposed" : "available");
        if (disposed || busy) return writing ? outcome(2, "output.rejected") : { code: disposed ? "user.cancelled" : "host.busy" };
        if (uncertain) return writing ? outcome(2, "output.rejected") : { code: "output.mutation-uncertain" };
        busy = true;
        const op = { cancelled: false, mutation: false, required: null, approved: null, started: performance.now() };
        operation = op;
        let timer, settled = false;
        const release = () => { if (operation === op) { operation = null; busy = false; } };
        // A timer cannot settle an Office.js Promise. Keep the lease until native work ends.
        const pending = Promise.resolve().then(() => action(op));
        pending.then(() => { settled = true; if (op.timedOut) release(); }, () => { settled = true; if (op.timedOut) release(); });
        try {
            const result = await Promise.race([pending, new Promise((resolve, reject) => {
                timer = setTimeout(() => { op.cancelled = true; op.timedOut = true; trace(op, "operation.timeout");
                    if (op.completed) resolve(op.completed);
                    else reject(Object.assign(new Error(), { safeCode: "input.time-budget" })); }, timeoutMs);
            })]);
            if (writing) traceResult(op, result);
            return result;
        } catch (e) {
            trace(op, "operation.failure", !e?.safeCode ? op.nativeException || typeof e?.code === "string" || e?.name === "RichApi.Error" ? "native-exception" : "adapter-exception"
                : op.invalidated && ["user.document-changed", "user.cancelled"].includes(e.safeCode) ? "lifecycle-invalidation" : "safety-check");
            if (writing && op.completed) { traceResult(op, op.completed); return op.completed; }
            if (writing) { uncertain ||= op.mutation; const why = code(e); const result = outcome(op.mutation ? 4 : 2, op.mutation ? "output.mutation-uncertain" :
                why.startsWith("output.") ? why : why === "user.document-changed" ? "output.consent-stale" : "output.rejected", op.mutation, [], op.required, op.approved);
                if (op.mutation) { source = null; prepared = null; epoch++; ranges.retire(); }
                traceResult(op, result); return result; }
            return { code: code(e) };
        } finally { clearTimeout(timer); if (settled) release(); }
    }
    async function detach() {
        const old = listeners; listeners = [];
        for (const owner of new Set(old.map(result => result.context))) try {
            await scope.Excel.run(owner, async context => { for (const result of old) if (result.context === owner) result.remove(); await context.sync(); });
        } catch { /* Closed host; callbacks remain inert after disposal/source retirement. */ }
    }
    async function watch(op, pinned) {
        await detach(); checkpoint(op, pinned);
        await scope.Excel.run(async context => {
            checkpoint(op, pinned);
            const sheets = context.workbook.worksheets;
            const changed = () => { if (!disposed && source === pinned) {
                epoch++; prepared = null;
                if (operation) operation.invalidated = true;
                trace(operation, "lifecycle.event", operation?.mutation ? "after-mutation-queued" : "before-mutation");
                // Own mutations cannot turn an observed completion into a cancellation.
                if (!operation?.mutation) try { void pinned.callback?.invokeMethodAsync("SourceInvalidated", pinned.token).catch(() => {}); } catch { /* Disposed callback. */ }
            } };
            const selectionChanged = () => { if (pickingOutput) { prepared = null; return; } changed(); };
            const events = [[sheets.onChanged, changed], [sheets.onSelectionChanged, selectionChanged], [sheets.onNameChanged, changed], [sheets.onDeleted, changed],
                [sheets.onAdded, changed], [sheets.onActivated, selectionChanged], [sheets.onProtectionChanged, changed], [sheets.onFormatChanged, changed], [context.workbook.onActivated, changed]];
            for (const [event, handler] of events) {
                if (typeof event?.add !== "function") fail("office.unsupported-api");
                listeners.push(event.add(handler));
            }
            await context.sync(); checkpoint(op, pinned);
        });
    }
    function meta(range) {
        return { worksheetId: range.worksheet.id, worksheetName: range.worksheet.name, startRow: range.rowIndex + 1,
            startColumn: range.columnIndex + 1, rowCount: range.rowCount, columnCount: range.columnCount };
    }
    function loadSelection(context) {
        const range = context.workbook.getSelectedRange();
        range.load("rowIndex,columnIndex,rowCount,columnCount"); range.worksheet.load("id,name"); return range;
    }
    function scalar(value, type, format) {
        const kind = { Empty: 0, Integer: 1, Double: 1, String: 2, Boolean: 3, Error: 4 }[type];
        if (kind === undefined || typeof format !== "string" || (kind === 1 && !Number.isFinite(value))) fail("host.invalid-response");
        return [kind, kind === 1 ? value : 0, [2, 4].includes(kind) ? value : "", kind === 3 ? value : false, format];
    }
    async function observe(op, pinned, expected = null) {
        const parts = pinned.parts || [pinned];
        let bytes = 0;
        for (let i = 0; i < parts.length; i++) {
            const part = parts[i];
            const fingerprint = await observeRange(op, pinned, part, expected && (pinned.parts ? expected[i] : expected), size => {
                bytes += size; if (bytes > 67108864) fail("input.estimated-bytes");
            });
            part.fingerprint = fingerprint;
        }
        return pinned.fingerprint;
    }
    async function observeRange(op, pinned, part, expected, addBytes) {
        checkpoint(op, pinned);
        await ranges.checkNormalization(part.reference);
        checkpoint(op, pinned);
        const actual = [], formulas = [];
        const m = part.meta;
        for (let r = 0; r < m.rowCount;) {
            const height = Math.min(m.rowCount - r, Math.max(1, Math.floor(CHUNK / m.columnCount)));
            for (let c = 0; c < m.columnCount; c += CHUNK) {
                const width = Math.min(CHUNK, m.columnCount - c);
                await scope.Excel.run(async context => {
                    checkpoint(op, pinned);
                    const selection = loadSelection(context);
                    const b = { startRow: m.startRow + r, endRow: m.startRow + r + height - 1, startColumn: m.startColumn + c, endColumn: m.startColumn + c + width - 1 };
                    const tile = context.workbook.worksheets.getItem(m.worksheetId).getRange(area(b));
                    tile.load("values,valueTypes,numberFormat,formulas"); tile.worksheet.load("id,name");
                    await context.sync(); checkpoint(op, pinned);
                    pinned.selection ??= meta(selection);
                    if (!equal(meta(selection), pinned.selection) || tile.worksheet.id !== m.worksheetId || tile.worksheet.name !== m.worksheetName) fail("user.document-changed");
                    for (const matrix of [tile.values, tile.valueTypes, tile.numberFormat, tile.formulas])
                        if (!Array.isArray(matrix) || matrix.length !== height || matrix.some(row => !Array.isArray(row) || row.length !== width)) fail("host.invalid-response");
                    for (let y = 0; y < height; y++) for (let x = 0; x < width; x++) {
                        const index = (r + y) * m.columnCount + c + x;
                        actual[index] = scalar(tile.values[y][x], tile.valueTypes[y][x], tile.numberFormat[y][x]);
                        formulas[index] = tile.formulas[y][x];
                        addBytes(1024 + 2 * ((typeof tile.values[y][x] === "string" ? tile.values[y][x].length : 0) + tile.numberFormat[y][x].length +
                            (typeof tile.formulas[y][x] === "string" ? tile.formulas[y][x].length : 0)));
                    }
                });
            }
            r += height;
        }
        if (expected && !equal(actual, expected)) fail("user.document-changed");
        const fingerprint = JSON.stringify([actual, formulas]);
        if (part.fingerprint && fingerprint !== part.fingerprint) fail("user.document-changed");
        return fingerprint;
    }
    async function pinSource(snapshot, callback = null, referenceHandle = null) {
        return guarded(async op => {
            await ranges.stop(); invalidate(); op.cancelled = false; op.invalidated = false;
            ranges.retire();
            const reference = referenceHandle ? ranges.verify(referenceHandle, snapshot) : null;
            if (scope.Office?.context?.requirements?.isSetSupported("ExcelApi", "1.17") !== true || !scope.crypto?.randomUUID) fail("office.unsupported-api");
            if (!snapshot || !integer(snapshot.rowCount, MAX) || !integer(snapshot.columnCount, MAX) ||
                snapshot.rowCount * snapshot.columnCount > MAX || !boundedText(snapshot.worksheetId, 1024) || !boundedText(snapshot.worksheetName, 256)) fail("user.input-invalid");
            const pinned = { epoch, document: document(), token: token(), callback, reference, meta: {
                worksheetId: snapshot.worksheetId, worksheetName: snapshot.worksheetName, startRow: snapshot.startRow,
                startColumn: snapshot.startColumn, rowCount: snapshot.rowCount, columnCount: snapshot.columnCount } };
            if (!pinned.document) fail("user.document-changed");
            source = pinned;
            await watch(op, pinned);
            const expected = snapshot.rows.flatMap(row => row.map(c => [c.kind, c.numberValue, c.textValue, c.booleanValue, c.numberFormat]));
            pinned.fingerprint = await observe(op, pinned, expected);
            ranges.bind(pinned.token, pinned.meta, reference);
            return { code: "workbook.ok", sourceToken: pinned.token, sessionIdentity: pinned.token };
        });
    }
    async function pinSources(sources, callback = null) {
        return guarded(async op => {
            await ranges.stop(); invalidate(); op.cancelled = false; op.invalidated = false;
            ranges.retire();
            if (scope.Office?.context?.requirements?.isSetSupported("ExcelApi", "1.17") !== true || !scope.crypto?.randomUUID) fail("office.unsupported-api");
            if (!Array.isArray(sources) || sources.length !== 2) fail("input.invalid-reference");
            const parts = sources.map(({ snapshot: s, handle }) => {
                if (!s || s.columnCount !== 1 || !integer(s.rowCount, MAX) || !integer(s.startRow, 1048576) ||
                    !integer(s.startColumn, 16384) || s.startRow + s.rowCount - 1 > 1048576 ||
                    !boundedText(s.worksheetId, 1024) || !boundedText(s.worksheetName, 256) ||
                    !Array.isArray(s.rows) || s.rows.length !== s.rowCount || s.rows.some(r => !Array.isArray(r) || r.length !== 1)) fail("input.invalid-reference");
                const reference = ranges.verify(handle, s); // Every original selection stays bound to this Office document.
                return { reference, meta: { worksheetId: s.worksheetId, worksheetName: s.worksheetName, startRow: s.startRow,
                    startColumn: s.startColumn, rowCount: s.rowCount, columnCount: 1 } };
            });
            const [a, b] = parts.map(p => p.meta);
            if (a.worksheetId !== b.worksheetId || a.worksheetName !== b.worksheetName || a.startColumn === b.startColumn ||
                Math.max(a.startRow, b.startRow) > Math.min(a.startRow + a.rowCount - 1, b.startRow + b.rowCount - 1)) fail("input.long-alignment");
            if (a.rowCount + b.rowCount > MAX) fail("input.total-cells");
            const pinned = { epoch, document: document(), token: token(), callback, parts, meta: a };
            if (!pinned.document) fail("user.document-changed");
            source = pinned; await watch(op, pinned);
            const expected = sources.map(({ snapshot: s }) => s.rows.map(([c]) => [c.kind, c.numberValue, c.textValue, c.booleanValue, c.numberFormat]));
            await observe(op, pinned, expected);
            checkpoint(op, pinned);
            return { code: "workbook.ok", sourceToken: pinned.token, sessionIdentity: pinned.token };
        });
    }
    async function checkSource(id) {
        return guarded(async op => {
            const pinned = source;
            if (ranges.armed || pinned?.token !== id) fail("user.document-changed");
            await observe(op, pinned);
            return { code: "workbook.ok" };
        });
    }
    async function inspect(context, op, pinned, plan, intent) {
        checkpoint(op, pinned);
        if (intent.workbookIdentity !== pinned.token) fail("user.document-changed");
        const readWrite = scope.Office?.DocumentMode?.ReadWrite;
        if (typeof readWrite !== "string" || document()?.mode !== readWrite) fail("output.target-unwritable");
        const book = context.workbook, sheets = book.worksheets;
        const selected = loadSelection(context);
        book.protection.load("protected"); sheets.load("items/id,items/name");
        await context.sync(); checkpoint(op, pinned);
        if (!equal(meta(selected), pinned.selection)) fail("user.document-changed");
        if (book.protection.protected !== false) fail("output.target-unwritable");
        const matching = sheets.items.filter(s => worksheetNameKey(s.name) === worksheetNameKey(intent.worksheetName));
        if (intent.destination === 1) {
            if (matching.length) fail("output.sheet-name-collision");
            if (!validWorksheetName(intent.worksheetName) || intent.worksheetIdentity !== null) fail("output.target-invalid");
            return { state: 0, occupiedRegionIds: [], hasValues: false, hasFormulas: false, fingerprint: "new-sheet" };
        }
        const match = matching.find(s => s.id === intent.worksheetIdentity);
        if (!match || match.name !== intent.worksheetName) fail("user.document-changed");
        const sheet = sheets.getItem(match.id); sheet.protection.load("protected");
        for (const part of pinned.parts || [pinned]) {
            const m = part.reference?.original ?? part.meta;
            const sourceBounds = { startRow: m.startRow, startColumn: m.startColumn,
                endRow: m.startRow + m.rowCount - 1, endColumn: m.startColumn + m.columnCount - 1 };
            if (match.id === m.worksheetId && plan.tables.some(t => intersects(t.bounds, sourceBounds))) fail("output.target-invalid");
        }
        const ranges = plan.tables.map(t => {
            const range = sheet.getRange(area(t.bounds)); range.load("values,formulas,valueTypes,numberFormat,hasSpill");
            const merged = range.getMergedAreasOrNullObject(); // OrNullObject resolves isNullObject at sync.
            const tables = range.getTables(false); tables.load("items/id");
            const pivots = range.getPivotTables(false); pivots.load("items/id");
            const formulaAreas = range.getSpecialCellsOrNullObject("Formulas");
            return { range, merged, tables, pivots, formulaAreas };
        });
        await context.sync(); checkpoint(op, pinned);
        if (sheet.protection.protected !== false) fail("output.target-unwritable");
        if (ranges.some(r => r.merged.isNullObject !== true)) fail("output.target-merged");
        if (ranges.some(r => r.range.hasSpill !== false || r.tables.items.length || r.pivots.items.length)) fail("output.target-invalid");
        const occupiedRegionIds = []; let hasValues = false, hasFormulas = false;
        for (let i = 0; i < ranges.length; i++) {
            const r = ranges[i].range;
            const values = r.valueTypes.some(row => row.some(t => t !== "Empty"));
            if (typeof ranges[i].formulaAreas.isNullObject !== "boolean") fail("output.occupancy-unverified");
            const formulas = !ranges[i].formulaAreas.isNullObject;
            hasValues ||= values; hasFormulas ||= formulas;
            if (values || formulas) occupiedRegionIds.push(plan.tables[i].artifactId);
        }
        return { state: occupiedRegionIds.length ? 1 : 0, occupiedRegionIds, hasValues, hasFormulas,
            fingerprint: JSON.stringify(ranges.map(r => [r.range.values, r.range.formulas, r.range.valueTypes, r.range.numberFormat])) };
    }
    async function resolveNewWorksheetName(requestedName, id) {
        return guarded(async op => {
            prepared = null;
            const pinned = source;
            if (ranges.armed || pinned?.token !== id) fail("user.document-changed");
            if (!validWorksheetName(requestedName)) fail("output.target-invalid");
            await observe(op, pinned);
            return scope.Excel.run(async context => {
                const sheets = context.workbook.worksheets;
                sheets.load("items/id,items/name"); context.workbook.protection.load("protected");
                await context.sync(); checkpoint(op, pinned);
                if (context.workbook.protection.protected !== false || document()?.mode !== scope.Office?.DocumentMode?.ReadWrite)
                    fail("output.target-unwritable");
                if (!Array.isArray(sheets.items) || sheets.items.some(s => !boundedText(s.id, 1024)) ||
                    new Set(sheets.items.map(s => s.id)).size !== sheets.items.length) fail("output.sheet-name-unavailable");
                const allocation = allocateWorksheetName(requestedName, sheets.items.map(s => s.name));
                if (allocation.warningCode) trace(op, "output.sheet-name-adjusted");
                return { code: "workbook.ok", ...allocation };
            });
        });
    }
    async function prepareOutput(plan, intent, id) {
        return guarded(async op => {
            prepared = null; validateProductionPlan(plan, intent);
            const pinned = source;
            if (ranges.armed || pinned?.token !== id) fail("user.document-changed");
            await observe(op, pinned);
            const facts = await scope.Excel.run(context => inspect(context, op, pinned, plan, intent));
            const observationIdentity = token();
            const inspection = { intent, state: facts.state, observationIdentity, occupiedRegionIds: facts.occupiedRegionIds,
                hasValues: facts.hasValues, hasFormulas: facts.hasFormulas };
            prepared = { plan: JSON.stringify(plan), intent: JSON.stringify(intent), facts, inspection, pinned };
            return { code: "workbook.ok", ...inspection, facts: { workbookAvailable: true, sheetExists: intent.destination === 0,
                identityUnambiguous: true, writable: true, protectedSheet: false, mergedConflict: false,
                canCreateWorksheet: true, worksheetNameValid: true, newWorksheetNameCollision: false } };
        });
    }
    const nativeValue = v => v.kind === "blank" ? "" : v.kind === "number" ? v.numberValue : v.kind === "boolean" ? v.booleanValue : "'" + v.textValue;
    async function mutationSync(context, op, stage) {
        trace(op, `${stage}.sync.start`);
        try { await context.sync(); }
        catch (e) { op.nativeException = true; throw e; }
        trace(op, `${stage}.sync.end`);
    }
    async function execute(plan, intent, consent) {
        return guarded(async op => {
            const bound = prepared; prepared = null; // Single-use, including refusals and failed attempts.
            validateProductionPlan(plan, intent);
            if (!bound || bound.plan !== JSON.stringify(plan) || bound.intent !== JSON.stringify(intent)) fail("output.consent-stale");
            checkpoint(op, bound.pinned);
            op.required = bound.facts.state === 1;
            if (op.required) {
                const c = consent?.inspection, expected = bound.inspection;
                if (!c || c.observationIdentity !== expected.observationIdentity || !equal(c.intent, expected.intent) || c.state !== 1 ||
                    !equal(c.occupiedRegionIds, expected.occupiedRegionIds) || c.hasValues !== expected.hasValues || c.hasFormulas !== expected.hasFormulas || typeof consent.approved !== "boolean") fail("output.consent-stale");
                op.approved = consent.approved;
                if (!consent.approved) return outcome(1, "output.cancelled", false, [], true, false);
            } else if (consent) fail("output.consent-stale");
            await observe(op, bound.pinned);
            const result = await scope.Excel.run(async context => {
                const fresh = await inspect(context, op, bound.pinned, plan, intent);
                if (!equal(fresh, bound.facts)) fail("output.consent-stale");
                checkpoint(op, bound.pinned);
                let sheet;
                // No request to create a worksheet is ever queued during inspection.
                if (intent.destination === 1) { trace(op, "worksheet.add.before"); op.mutation = true; sheet = context.workbook.worksheets.add(intent.worksheetName); }
                else sheet = context.workbook.worksheets.getItem(intent.worksheetIdentity);
                const written = [];
                for (const table of plan.tables) {
                    const range = sheet.getRange(area(table.bounds));
                    const values = [], formats = [];
                    for (let r = 0; r < table.rowCount; r++) {
                        const cells = table.cells.slice(r * table.columnCount, (r + 1) * table.columnCount);
                        values.push(cells.map(c => nativeValue(c.value))); formats.push(cells.map(c => c.numberFormat));
                    }
                    op.mutation = true;
                    range.values = values; range.numberFormat = formats;
                    const styles = table.cells.map(tablePresenter(table));
                    const presentation = table.cells.map((c, i) => applyPresentation(range.getCell(c.row, c.column), styles[i]));
                    written.push({ range, presentation, styles, table });
                }
                await mutationSync(context, op, "mutation"); // Cancellation cannot abort native mutation.
                for (const w of written) { w.range.load("values,formulas,numberFormat"); w.presentation.forEach(loadPresentation);
                    w.formulaAreas = w.range.getSpecialCellsOrNullObject("Formulas"); }
                sheet.load("id,name");
                await mutationSync(context, op, "readback");
                const verify = (category, test, detail = () => "") => {
                    trace(op, `verify.${category}.start`);
                    const matches = test(), reason = matches ? "" : detail();
                    trace(op, `verify.${category}`, matches ? "match" : `mismatch${reason ? ":" + reason : ""}`);
                    if (!matches) fail("output.mutation-uncertain");
                };
                verify("worksheet", () => document() === bound.pinned.document && boundedText(sheet.id, 1024) && sheet.name === intent.worksheetName &&
                    (intent.destination === 1 || sheet.id === intent.worksheetIdentity));
                const cells = written.flatMap(w => w.table.cells.map(c => ({ w, c, p: w.presentation[c.row * w.table.columnCount + c.column],
                    f: w.presentation[c.row * w.table.columnCount + c.column].format.font, s: w.styles[c.row * w.table.columnCount + c.column] })));
                const matrixMatches = (w, matrix) => Array.isArray(matrix) && matrix.length === w.table.rowCount && matrix.every(row => Array.isArray(row) && row.length === w.table.columnCount);
                verify("values", () => written.every(w => matrixMatches(w, w.range.values)) && cells.every(({ w, c }) =>
                    matchesNativeOutputValue(w.range.values[c.row][c.column], c.value, scope.Office?.context?.platform === "OfficeOnline")));
                verify("formulas", () => written.every(w => matrixMatches(w, w.range.formulas) && w.formulaAreas.isNullObject === true));
                verify("number-format", () => written.every(w => matrixMatches(w, w.range.numberFormat)) && cells.every(({ w, c }) => w.range.numberFormat[c.row][c.column] === c.numberFormat));
                verify("font-bold", () => cells.every(({ f, s }) => f.bold === s.bold));
                let colorMismatch;
                verify("font-color", () => { colorMismatch = cells.find(({ f, s }) => typeof f.color !== "string" || f.color.toUpperCase() !== s.color); return !colorMismatch; },
                    () => representation(colorMismatch.f.color));
                for (const category of ["font-name", "font-size", "alignment", "fill", "borders"])
                    verify(category, () => cells.every(({p, s}) => presentationMatches(p, s, category, scope.Office?.context?.platform === "OfficeOnline")));
                // A timed-out caller has already received uncertainty, even if later readback matches.
                if (op.timedOut) fail("output.mutation-uncertain");
                source = null; epoch++; ranges.retire();
                op.completed = outcome(0, "output.completed", true, plan.tables.map(t => t.artifactId), op.required, op.approved);
                return op.completed;
            });
            // Remove subscriptions after the native write batch settles, using their owning context.
            // Listener cleanup cannot downgrade a verified write; stale closures are already inert.
            await detach();
            return result;
        }, true);
    }
    async function dispose() { disposed = true; invalidate(); await ranges.dispose(); await detach(); }
    return Object.freeze({ pinSource, pinSources, checkSource, resolveNewWorksheetName, prepareOutput, execute,
        resolveReference: (reference, handle) => guarded(() => ranges.resolve(reference, handle), false, true),
        captureSelection: single => guarded(() => ranges.selection(single), false, true),
        beginCapture: (owner, output, callback) => guarded(() => ranges.begin(owner, output, callback), false, true),
        endCapture: (owner, cancel) => guarded(() => ranges.end(owner, cancel)),
        invalidate: () => { invalidate(); void ranges.stop(); }, clearOutput: () => { prepared = null; }, dispose });
}
