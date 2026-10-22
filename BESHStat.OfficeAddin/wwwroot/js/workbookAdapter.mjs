import { createOfficeHost } from "./officeHost.mjs";

const OK = "workbook.ok";
const defaults = Object.freeze({ maxRangeCells: 10000, maxTotalCells: 10000, maxRanges: 64,
    maxChunkCells: 2000, maxEstimatedBytes: 67108864, estimatedBytesPerCell: 1024, timeBudgetMilliseconds: 30000 });
class Fault extends Error { constructor(code) { super(code); this.code = code; } }
const reject = code => { throw new Fault(code); };
const text = (value, max) => typeof value === "string" && value.length > 0 && value.length <= max && !/[\u0000-\u001f\u007f]/.test(value);
const integer = (value, min, max) => Number.isSafeInteger(value) && value >= min && value <= max;
function safeCode(error) {
    if (error instanceof Fault) return error.code;
    if (["ItemNotFound", "InvalidObjectPath"].includes(error?.code)) return "user.document-changed";
    if (["InvalidSelection", "InvalidReference"].includes(error?.code)) return "input.invalid-selection";
    return "host.operation-failed";
}
function policy(input = defaults) {
    if (!input || typeof input !== "object" || Array.isArray(input)) reject("user.input-invalid");
    if (Object.keys(input).some(key => !Object.hasOwn(defaults, key))) reject("user.input-invalid");
    const result = { ...defaults, ...input };
    for (const key of Object.keys(defaults)) {
        if (!Number.isFinite(result[key]) || result[key] <= 0 || result[key] > defaults[key] ||
            (!key.includes("Bytes") && key !== "timeBudgetMilliseconds" && !Number.isSafeInteger(result[key]))) reject("user.input-invalid");
    }
    if (result.estimatedBytesPerCell < defaults.estimatedBytesPerCell) reject("user.input-invalid");
    return Object.freeze(result);
}
function column(index) {
    let value = "";
    for (; index > 0; index = Math.floor((index - 1) / 26)) value = String.fromCharCode(65 + (index - 1) % 26) + value;
    return value;
}
const address = (row, col, rows, cols) => `${column(col)}${row}:${column(col + cols - 1)}${row + rows - 1}`;
function parseA1(value) {
    if (typeof value !== "string" || value.length > 40) reject("user.input-invalid");
    const parts = /^\$?([A-Z]{1,3})\$?([1-9][0-9]{0,6})(?::\$?([A-Z]{1,3})\$?([1-9][0-9]{0,6}))?$/i.exec(value);
    if (!parts) reject("input.invalid-selection");
    const col = letters => [...letters.toUpperCase()].reduce((n, c) => n * 26 + c.charCodeAt(0) - 64, 0);
    const startRow = Number(parts[2]), startColumn = col(parts[1]);
    const lastRow = Number(parts[4] || parts[2]), lastColumn = col(parts[3] || parts[1]);
    if (lastRow < startRow || lastColumn < startColumn || lastRow > 1048576 || lastColumn > 16384) reject("input.invalid-selection");
    return { startRow, startColumn, rowCount: lastRow - startRow + 1, columnCount: lastColumn - startColumn + 1 };
}
function metadata(range) {
    const sheet = range.worksheet;
    if (!text(sheet.id, 1024) || !text(sheet.name, 256) || !integer(range.rowIndex, 0, 1048575) ||
        !integer(range.columnIndex, 0, 16383) || !integer(range.rowCount, 1, 1048576) || !integer(range.columnCount, 1, 16384) ||
        range.rowIndex + range.rowCount > 1048576 || range.columnIndex + range.columnCount > 16384 ||
        typeof range.address !== "string" || range.address.length > 2048) reject("host.invalid-response");
    const bang = range.address.lastIndexOf("!");
    const local = range.address.slice(bang + 1);
    if (bang >= 0) {
        let qualifier = range.address.slice(0, bang);
        if (qualifier.startsWith("'") && qualifier.endsWith("'")) qualifier = qualifier.slice(1, -1).replace(/''/g, "'");
        if (qualifier !== sheet.name) reject("user.document-changed");
    }
    const parsed = parseA1(local);
    if (parsed.startRow !== range.rowIndex + 1 || parsed.startColumn !== range.columnIndex + 1 ||
        parsed.rowCount !== range.rowCount || parsed.columnCount !== range.columnCount) reject("user.document-changed");
    return { worksheetId: sheet.id, worksheetName: sheet.name, ...parsed,
        address: address(parsed.startRow, parsed.startColumn, parsed.rowCount, parsed.columnCount) };
}
function cell(value, type, format) {
    if (typeof type !== "string" || typeof format !== "string" || format.length > 32767) reject("host.invalid-response");
    const result = { kind: 0, numberValue: 0, textValue: "", booleanValue: false, numberFormat: format, nativeErrorCode: "" };
    switch (type.toLowerCase()) {
        case "empty": if (value !== "" && value !== null) reject("host.invalid-response"); break;
        case "integer": case "double":
            if (typeof value !== "number" || !Number.isFinite(value)) reject("host.invalid-response");
            result.kind = 1; result.numberValue = value; break;
        case "string": case "error":
            if (typeof value !== "string" || value.length > 32767) reject("host.invalid-response");
            result.kind = type.toLowerCase() === "error" ? 4 : 2; result.textValue = value; break;
        case "boolean":
            if (typeof value !== "boolean") reject("host.invalid-response");
            result.kind = 3; result.booleanValue = value; break;
        default: reject("host.invalid-response");
    }
    return result;
}

// Only primitive operation metadata and the actual Office document session object
// survive a call. No Excel proxy, RequestContext, cell matrix or event sink is retained.
// writer is an injected test port; create() never supplies it, so production cannot write.
export function createWorkbookTransport(scope = globalThis, { now = () => performance.now(), writer = null, diagnostics = false } = {}) {
    const readiness = createOfficeHost(scope);
    let active = null, disposed = false;
    if (diagnostics === true) try { scope.__beshOfficeDebug?.enableWorkbookDevelopment?.(); } catch { /* Optional local recorder. */ }
    function mark(op, code) {
        if (diagnostics !== true || disposed || active !== op || op.cancelled) return;
        try { scope.__beshOfficeDebug?.record(code); } catch { /* Diagnostics never affect reads. */ }
    }
    function timeout(op) {
        if (!op.timeoutRecorded) { mark(op, "read.js-timeout"); op.timeoutRecorded = true; }
        return new Fault("input.time-budget");
    }
    const document = () => scope.Office?.context?.document;
    function check(op) {
        if (disposed || op.cancelled) reject("user.cancelled");
        const elapsed = now() - op.started;
        if (!Number.isFinite(elapsed) || elapsed < op.elapsed) reject("host.invalid-response");
        op.elapsed = elapsed;
        if (elapsed > op.limits.timeBudgetMilliseconds) throw timeout(op);
        if (op.document && document() !== op.document) reject("user.document-changed");
    }
    async function bounded(action, op) {
        check(op);
        let timer;
        try {
            return await Promise.race([action(), new Promise((_, no) => {
                timer = setTimeout(() => no(timeout(op)), Math.max(1, op.limits.timeBudgetMilliseconds - (now() - op.started)));
            })]);
        } finally { clearTimeout(timer); }
    }
    async function ready(op) {
        mark(op, "read.readiness-started");
        const status = await bounded(() => readiness.initialize(), op);
        check(op);
        mark(op, status.code === "office.excel-ready" ? "read.readiness-completed" : "read.readiness-rejected");
        if (status.code !== "office.excel-ready") reject(status.code);
        if (typeof scope.Excel?.run !== "function" || !scope.Office.context?.requirements ||
            scope.Office.context.requirements.isSetSupported("ExcelApi", "1.1") !== true) reject("office.unsupported-api");
        const doc = document();
        if (!doc || typeof doc !== "object") reject("user.document-changed");
        return doc;
    }
    function load(range) {
        range.load("address,rowIndex,columnIndex,rowCount,columnCount");
        range.worksheet.load("id,name");
        return range;
    }
    function source(context, op) {
        if (!op.reference) return load(context.workbook.getSelectedRange());
        return load(context.workbook.worksheets.getItem(op.reference.worksheetId).getRange(op.reference.address));
    }
    function same(actual, expected) {
        if (Object.keys(expected).some(key => actual[key] !== expected[key])) reject("user.document-changed");
    }
    async function observe(context, op, stage) {
        check(op);
        const range = source(context, op);
        mark(op, `read.${stage}-sync-started`);
        await bounded(() => context.sync(), op);
        check(op);
        mark(op, `read.${stage}-sync-completed`);
        const meta = metadata(range);
        if (op.meta) same(meta, op.meta);
        if (op.reference) same(meta, op.reference);
        mark(op, `read.${stage}-validated`);
        return { range, meta };
    }
    function run(op, stage, callback) {
        mark(op, `read.${stage}-run-started`);
        return bounded(() => scope.Excel.run(context => {
            check(op); // A native callback can arrive after timeout/disposal.
            mark(op, `read.${stage}-run-entered`);
            return callback(context);
        }), op);
    }
    async function guarded(op, action) {
        try { const result = await action(); check(op); return { code: OK, ...result }; }
        catch (error) {
            mark(op, "read.rejected");
            if (active === op) active = null;
            op.cancelled = true; op.document = null;
            return { code: safeCode(error) };
        }
    }
    async function beginRead(id, limits, reference = null, metadataOnly = false) {
        if (disposed) return { code: "user.cancelled" };
        if (active) return { code: "host.busy" };
        if (!text(id, 64) || typeof metadataOnly !== "boolean") return { code: "user.input-invalid" };
        let checked;
        try { checked = policy(limits); } catch (error) { return { code: safeCode(error) }; }
        const op = { id, limits: checked, started: now(), elapsed: 0, cancelled: false, reference: null,
            meta: null, document: null, row: 0, column: 0, metadataOnly, inFlight: false, bytes: 0 };
        active = op;
        mark(op, metadataOnly ? "read.metadata-started" : "read.full-started");
        return guarded(op, async () => {
            op.document = await ready(op);
            if (reference !== null) {
                if (!reference || reference.addressDialect !== 1 || !text(reference.worksheetId, 1024) || !text(reference.worksheetName, 256) ||
                    reference.workbookId || reference.workbookName || reference.selectionId || reference.selectionRevision) reject("user.input-invalid");
                const parsed = parseA1(reference.address);
                for (const key of Object.keys(parsed)) if (reference[key] !== parsed[key]) reject("user.input-invalid");
                op.reference = { ...parsed, address: address(parsed.startRow, parsed.startColumn, parsed.rowCount, parsed.columnCount),
                    worksheetId: reference.worksheetId, worksheetName: reference.worksheetName };
            }
            const observed = await run(op, "metadata", async context => (await observe(context, op, "metadata")).meta);
            const cells = observed.rowCount * observed.columnCount;
            if (cells > checked.maxRangeCells) reject("input.range-cells");
            if (cells > checked.maxTotalCells) reject("input.total-cells");
            if (cells * checked.estimatedBytesPerCell > checked.maxEstimatedBytes) reject("input.estimated-bytes");
            op.meta = observed;
            return { metadata: observed };
        });
    }
    async function readNext(id) {
        const op = active;
        if (!op || op.id !== id || op.metadataOnly || !op.meta || op.inFlight) return { code: "user.input-invalid" };
        op.inFlight = true;
        try { return await guarded(op, async () => {
            if (op.row >= op.meta.rowCount) reject("user.input-invalid");
            const width = Math.min(op.meta.columnCount, op.limits.maxChunkCells);
            const height = Math.max(1, Math.floor(op.limits.maxChunkCells / width));
            const rowOffset = op.row, columnOffset = op.column;
            const rows = Math.min(height, op.meta.rowCount - rowOffset), cols = Math.min(width, op.meta.columnCount - columnOffset);
            const result = await run(op, "tile", async context => {
                const observed = await observe(context, op, "preflight");
                // getRangeByIndexes requires 1.7. Generated, unqualified A1 uses only 1.1.
                const local = address(op.meta.startRow + rowOffset, op.meta.startColumn + columnOffset, rows, cols);
                const tile = load(observed.range.worksheet.getRange(local));
                tile.load("values,valueTypes,numberFormat");
                mark(op, "read.payload-sync-started");
                await bounded(() => context.sync(), op);
                check(op);
                mark(op, "read.payload-sync-completed");
                const tileMeta = metadata(tile);
                same(tileMeta, { worksheetId: op.meta.worksheetId, worksheetName: op.meta.worksheetName, address: local,
                    startRow: op.meta.startRow + rowOffset, startColumn: op.meta.startColumn + columnOffset, rowCount: rows, columnCount: cols });
                for (const matrix of [tile.values, tile.valueTypes, tile.numberFormat])
                    if (!Array.isArray(matrix) || matrix.length !== rows || matrix.some(row => !Array.isArray(row) || row.length !== cols)) reject("host.invalid-response");
                const cells = tile.values.map((row, r) => row.map((value, c) => {
                    const result = cell(value, tile.valueTypes[r][c], tile.numberFormat[r][c]);
                    op.bytes += op.limits.estimatedBytesPerCell + 2 * (result.textValue.length + result.numberFormat.length);
                    if (op.bytes > op.limits.maxEstimatedBytes) reject("input.estimated-bytes");
                    return result;
                }));
                // Recheck the selection and sheet after the payload sync too.
                await observe(context, op, "reconcile");
                return { metadata: op.meta, rowOffset, columnOffset, rows: cells };
            });
            op.column += width;
            if (op.column >= op.meta.columnCount) { op.column = 0; op.row += height; }
            return { ...result, done: op.row >= op.meta.rowCount };
        }); } finally { op.inFlight = false; }
    }
    async function finishRead(id) {
        const op = active;
        if (!op || op.id !== id || op.inFlight || !op.meta || (!op.metadataOnly && op.row < op.meta.rowCount)) return { code: "user.input-invalid" };
        const result = await guarded(op, async () => ({ metadata: await run(op, "final", async context => (await observe(context, op, "final")).meta) }));
        if (result.code === OK) mark(op, "read.completed");
        if (active === op) active = null;
        op.document = null;
        return result;
    }
    function cancel(id) { if (active?.id === id) { mark(active, "read.cancelled"); active.cancelled = true; active.document = null; active = null; } }
    function writeOutcome(state, code, mutationMayHaveOccurred = false, confirmedWrittenIds = [], required = null, approved = null) {
        return { state, code, mutationMayHaveOccurred, confirmedWrittenIds, confirmationRequired: required, overwriteApproved: approved };
    }
    async function write(plan, intent, consent = null) {
        // No production writer is installed. Higher manifest permission never enables it.
        if (!writer) return writeOutcome(2, intent?.destination === 2 ? "output.destination-unsupported" : "output.rejected");
        if (active || disposed) return writeOutcome(2, "output.rejected");
        const op = { id: "test-write", limits: defaults, started: now(), elapsed: 0, cancelled: false, document: null };
        active = op;
        let mutation = false, required = null, approved = null;
        try {
            op.document = await ready(op);
            if (!plan || plan.schemaVersion !== 1 || !intent || intent.destination !== 0 || !text(intent.worksheetIdentity, 1024) ||
                !text(intent.workbookIdentity, 1024) || !Array.isArray(intent.regions) || !intent.regions.length ||
                plan.target?.destination !== intent.destination || plan.target?.worksheetName !== intent.worksheetName ||
                plan.target?.startRow !== intent.startRow || plan.target?.startColumn !== intent.startColumn) reject("output.rejected");
            const boundPlan = JSON.stringify(plan), boundIntent = JSON.stringify(intent);
            const sameInspection = (a, b) => JSON.stringify(a) === JSON.stringify(b);
            return await bounded(() => scope.Excel.run(async context => {
                const first = await writer.inspect(context, plan, intent);
                check(op);
                if (![0, 1].includes(first?.state) || JSON.stringify(first.intent) !== boundIntent) reject("output.occupancy-unverified");
                required = first.state === 1;
                if (required) {
                    if (!consent) reject("output.confirmation-required");
                    if (!sameInspection(consent.inspection, first) || typeof consent.approved !== "boolean") reject("output.consent-stale");
                    approved = consent.approved;
                    if (!approved) return writeOutcome(1, "output.cancelled", false, [], true, false);
                } else if (consent) reject("output.consent-stale");
                const current = await writer.inspect(context, plan, intent);
                check(op);
                if (!sameInspection(first, current) || JSON.stringify(plan) !== boundPlan || JSON.stringify(intent) !== boundIntent) reject("output.consent-stale");
                mutation = true; // queued writes can mutate even when sync later fails
                await writer.commit(context, plan, intent);
                await context.sync();
                // Observed completion takes precedence over late cancellation.
                return writeOutcome(0, "output.completed", true, intent.regions.filter(r => r.mayMutate).map(r => r.artifactId), required, approved);
            }), op);
        } catch (error) {
            const code = safeCode(error);
            return writeOutcome(mutation ? 4 : 2, mutation ? "output.mutation-uncertain" : code.startsWith("output.") ? code : "output.rejected", mutation, [], required, approved);
        } finally { if (active === op) active = null; op.cancelled = true; op.document = null; }
    }
    function dispose() { disposed = true; if (active) cancel(active.id); readiness.dispose(); }
    return Object.freeze({ beginRead, readNext, finishRead, cancel, write, dispose });
}

// Blazor performs the single IJSObjectReference wrapping.
export function create(developmentDiagnostics = false) { return createWorkbookTransport(globalThis, { diagnostics: developmentDiagnostics === true }); }
