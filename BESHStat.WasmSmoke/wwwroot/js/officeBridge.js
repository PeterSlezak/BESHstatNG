(function () {
    "use strict";

    const CELL_KIND = Object.freeze({
        blank: 0,
        number: 1,
        text: 2,
        boolean: 3,
        error: 4
    });

    const OUTPUT_DESTINATION = Object.freeze({
        currentWorksheet: 0,
        newWorksheet: 1,
        newWorkbook: 2
    });

    function delay(milliseconds) {
        return new Promise(resolve => setTimeout(resolve, milliseconds));
    }

    async function getHostStatus(timeoutMilliseconds) {
        const timeout = typeof timeoutMilliseconds === "number" && timeoutMilliseconds > 0
            ? timeoutMilliseconds
            : 2500;

        if (typeof Office === "undefined") {
            return {
                officeJsLoaded: false,
                isExcel: false,
                host: "",
                platform: "",
                message: "Office.js is not loaded. Open this page as an Excel task pane for the Office bridge test."
            };
        }

        let readyInfo = null;
        try {
            if (Office.context && Office.context.host) {
                readyInfo = {
                    host: Office.context.host,
                    platform: Office.context.platform || ""
                };
            } else if (typeof Office.onReady === "function") {
                readyInfo = await Promise.race([
                    Office.onReady(),
                    delay(timeout).then(() => null)
                ]);
            }
        } catch (error) {
            return {
                officeJsLoaded: true,
                isExcel: false,
                host: "",
                platform: "",
                message: "Office.js loaded, but Office initialization failed: " +
                    (error && error.message ? error.message : String(error))
            };
        }

        const host = (readyInfo && readyInfo.host) || (Office.context && Office.context.host) || "";
        const platform = (readyInfo && readyInfo.platform) || (Office.context && Office.context.platform) || "";
        const excelHost = Office.HostType ? Office.HostType.Excel : "Excel";
        const isExcel = host === excelHost || host === "Excel";

        return {
            officeJsLoaded: true,
            isExcel: isExcel,
            host: host ? String(host) : "",
            platform: platform ? String(platform) : "",
            message: isExcel
                ? "Connected to Excel through Office.js."
                : "Office.js loaded, but this page is not running inside an Excel task pane."
        };
    }

    // All capability IDs must match HostCapabilityIds in BESHStat.Excel.Common.
    // No new-workbook capability: the prototype Office writer cannot create one.
    const HOST_CAPABILITY = Object.freeze({
        readSelectedRange: "input.read-selected-range",
        writeResultTables: "output.write-result-tables",
        currentWorksheet: "output.current-worksheet",
        newWorksheet: "output.new-worksheet"
    });

    async function getHostCapabilities() {
        const status = await getHostStatus(5000);
        const requirements = typeof Office !== "undefined" && Office.context
            ? Office.context.requirements : null;
        const excelApiSupported = requirements && typeof requirements.isSetSupported === "function"
            ? requirements.isSetSupported("ExcelApi", "1.1") === true
            : false;
        const ready = status.isExcel && typeof Excel !== "undefined" &&
            typeof Excel.run === "function" && excelApiSupported;

        return {
            host: "OfficeJs",
            platform: status.platform || "",
            isExcel: status.isExcel,
            capabilities: ready ? [
                HOST_CAPABILITY.readSelectedRange,
                HOST_CAPABILITY.writeResultTables,
                HOST_CAPABILITY.currentWorksheet,
                HOST_CAPABILITY.newWorksheet
            ] : []
        };
    }

    async function ensureExcelHost() {
        const status = await getHostStatus(5000);
        if (!status.isExcel) {
            throw new Error(status.message);
        }
        if (typeof Excel === "undefined" || typeof Excel.run !== "function") {
            throw new Error("The Excel JavaScript API is unavailable in this host.");
        }
        return status;
    }

    // Preserve Excel's reported kind rather than retyping a formatted value.
    // An empty string returned for a String cell (e.g. ="") is text, not a blank.
    function toPortableCell(value, valueType, numberFormat) {
        if (valueType !== null && valueType !== undefined && typeof valueType !== "string") {
            throw new Error("Unsupported host cell type metadata.");
        }
        const typeName = valueType === null || valueType === undefined
            ? ""
            : String(valueType).toLowerCase();
        const format = typeof numberFormat === "string" ? numberFormat : "";
        const base = { kind: CELL_KIND.blank, numberValue: 0, textValue: "", booleanValue: false,
            numberFormat: format, nativeErrorCode: "" };
        if (typeName === "error") {
            if (typeof value !== "string") throw new Error("Unsupported host error value.");
            return { ...base, kind: CELL_KIND.error, textValue: value };
        }
        if (typeName === "empty" || value === null || value === undefined) return base;
        if (typeName === "boolean" || typeof value === "boolean") {
            if (typeof value !== "boolean") throw new Error("Unsupported host Boolean value.");
            return { ...base, kind: CELL_KIND.boolean, booleanValue: value };
        }
        if (typeName === "integer" || typeName === "double" || (typeName === "" && typeof value === "number")) {
            if (typeof value !== "number") throw new Error("Unsupported host numeric value.");
            if (!Number.isFinite(value)) {
                return { ...base, kind: CELL_KIND.error, textValue: String(value) };
            }
            return { ...base, kind: CELL_KIND.number, numberValue: value };
        }
        if (typeName === "string" || typeof value === "string") {
            if (typeof value !== "string") throw new Error("Unsupported host text value.");
            if (value === "" && typeName === "") return base; // old hosts with no valueTypes
            return { ...base, kind: CELL_KIND.text, textValue: String(value) };
        }
        throw new Error("Unsupported host cell value; refresh with primitive cell values.");
    }

    // A range reference addresses the workbook currently hosting this task pane.
    // Worksheet.id is host-supplied; we do not manufacture workbook/selection IDs.
    function currentWorkbookName() {
        const url = typeof Office !== "undefined" && Office.context && Office.context.document
            ? Office.context.document.url : "";
        if (!url || typeof url !== "string") return "";
        const tail = url.split(/[\\/]/).pop().split(/[?#]/)[0];
        try { return decodeURIComponent(tail); } catch (_) { return tail; }
    }

    function validateReference(reference) {
        if (!reference || typeof reference.worksheetName !== "string" || !reference.worksheetName.trim()) {
            throw new Error("A worksheet name is required for an addressed range.");
        }
        if (/!\$?[a-z]{1,3}\$?[1-9][0-9]{0,6}(?::\$?[a-z]{1,3}\$?[1-9][0-9]{0,6})?$/i.test(reference.worksheetName)) {
            throw new Error("The worksheetName field must contain only the sheet name, not a qualified A1 address.");
        }
        if (typeof reference.address !== "string" ||
            !/^\$?[a-z]{1,3}\$?[1-9][0-9]{0,6}(?::\$?[a-z]{1,3}\$?[1-9][0-9]{0,6})?$/i.test(reference.address)) {
            throw new Error("A rectangular, unqualified A1 range address is required.");
        }
        const endpoints = reference.address.split(":");
        const readCell = value => {
            const match = /^\$?([a-z]{1,3})\$?([1-9][0-9]{0,6})$/i.exec(value);
            let column = 0;
            for (const character of match[1].toUpperCase()) column = column * 26 + character.charCodeAt(0) - 64;
            return { row: Number(match[2]), column };
        };
        const first = readCell(endpoints[0]);
        const last = readCell(endpoints[endpoints.length - 1]);
        if (first.column > 16384 || last.column > 16384 || first.row > 1048576 || last.row > 1048576 ||
            first.column > last.column || first.row > last.row) {
            throw new Error("The requested address exceeds Excel bounds or has reversed endpoints.");
        }
        if (reference.workbookId) {
            // Workbook IDs cannot currently be confirmed through ExcelApi 1.1.
            throw new Error("A workbook ID cannot be verified by this Office.js adapter.");
        }
        if (reference.addressDialect !== undefined && reference.addressDialect !== 1) {
            throw new Error("Office acquisition requires Excel A1 references.");
        }
        if (reference.selectionRevision) {
            throw new Error("A selection revision cannot be verified by this Office.js adapter.");
        }
        const expected = { startRow: first.row, startColumn: first.column,
            rowCount: last.row - first.row + 1, columnCount: last.column - first.column + 1 };
        for (const key of Object.keys(expected)) {
            if (reference[key] !== undefined && reference[key] !== 0 && reference[key] !== expected[key]) {
                throw new Error("Source coordinates conflict with the requested address.");
            }
        }
        const actualBook = currentWorkbookName();
        if (reference.workbookName && (!actualBook || reference.workbookName.toLowerCase() !== actualBook.toLowerCase())) {
            throw new Error("The requested workbook is not the current Office workbook.");
        }
        return expected;
    }

    function prepareRange(context, reference) {
        validateReference(reference);
        const worksheet = context.workbook.worksheets.getItem(reference.worksheetName);
        const range = worksheet.getRange(reference.address);
        worksheet.load(["name", "id"]);
        // Only metadata is requested here. The caller enforces the payload limit
        // before loading values, valueTypes or numberFormat.
        range.load(["address", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
        return { worksheet, range, reference };
    }

    function verifyEntry(entry) {
        const { range, worksheet, reference } = entry;
        if (reference && reference.worksheetId && reference.worksheetId !== worksheet.id) {
            throw new Error("The source worksheet identity changed; refresh the selection.");
        }
        if (reference) {
            const expected = validateReference(reference);
            if (worksheet.name.toLowerCase() !== reference.worksheetName.toLowerCase() ||
                range.rowIndex + 1 !== expected.startRow || range.columnIndex + 1 !== expected.startColumn ||
                range.rowCount !== expected.rowCount || range.columnCount !== expected.columnCount) {
                throw new Error("The source identity or coordinates changed; refresh the selection.");
            }
        }
    }

    function toRangeSnapshot(entry, rows) {
        const { range, worksheet, reference } = entry;
        verifyEntry(entry);
        const localAddress = range.address.indexOf("!") >= 0
            ? range.address.slice(range.address.lastIndexOf("!") + 1) : range.address;
        // Check the host-reported address against its actual rectangle, then use the
        // Common A1 convention (uppercase, no $, explicit endpoints even for one cell).
        validateReference({ worksheetName: worksheet.name, address: localAddress,
            startRow: range.rowIndex + 1, startColumn: range.columnIndex + 1,
            rowCount: range.rowCount, columnCount: range.columnCount });
        const endpoints = localAddress.replace(/\$/g, "").toUpperCase().split(":");
        const canonicalAddress = endpoints[0] + ":" + endpoints[endpoints.length - 1];
        return {
            workbookId: "", workbookName: currentWorkbookName(), dateSystem: 0,
            dateSerialOrigin: null, addressDialect: 1,
            worksheetId: worksheet.id || "", worksheetName: worksheet.name,
            address: canonicalAddress, startRow: range.rowIndex + 1, startColumn: range.columnIndex + 1,
            rowCount: range.rowCount, columnCount: range.columnCount,
            selectionId: reference && reference.selectionId ? reference.selectionId : "",
            selectionRevision: "", rows
        };
    }

    // Mirrors Common InputSizeLimits defaults; Excel-specific ceilings stay here.
    const MAX_SOURCE_CELLS = 10000;
    const INPUT_DEFAULTS = Object.freeze({ maxRangeCells: MAX_SOURCE_CELLS,
        maxTotalCells: MAX_SOURCE_CELLS, maxRanges: 64, maxChunkCells: 2000,
        maxEstimatedBytes: 67108864, estimatedBytesPerCell: 1024, timeBudgetMilliseconds: 30000 });

    function acquisitionOperation(options) {
        if (options !== undefined && (!options || typeof options !== "object" || Array.isArray(options))) {
            throw new Error("Acquisition options must be an object.");
        }
        const limits = { ...INPUT_DEFAULTS, ...options };
        for (const key of Object.keys(INPUT_DEFAULTS)) {
            const value = limits[key];
            if (typeof value !== "number" || !Number.isFinite(value) || value <= 0 ||
                value > Number.MAX_SAFE_INTEGER || (key !== "timeBudgetMilliseconds" &&
                key !== "estimatedBytesPerCell" && key !== "maxEstimatedBytes" && !Number.isSafeInteger(value))) {
                throw new Error(`Invalid acquisition budget ${key}: a finite positive bounded value is required.`);
            }
        }
        // Bounded opt-in, never an unlimited mode or a single giant host response.
        if (limits.maxRangeCells > 1000000 || limits.maxTotalCells > 1000000 ||
            limits.maxRanges > 1024 || limits.maxChunkCells > 10000 ||
            limits.maxEstimatedBytes > 268435456 || limits.estimatedBytesPerCell < 1024 ||
            limits.timeBudgetMilliseconds > 300000) {
            throw new Error("Acquisition options exceed Office safety ceilings (1,000,000 cells, 1024 ranges, " +
                "10,000 cells/chunk, 256 MiB estimate, 300,000 ms; estimate at least 1024 bytes/cell).");
        }
        if (limits.signal !== undefined && (!limits.signal || typeof limits.signal.aborted !== "boolean")) {
            throw new Error("Cancellation signal must expose an aborted Boolean.");
        }
        const now = () => typeof performance !== "undefined" ? performance.now() : Date.now();
        const started = now();
        const operation = { limits, check() {
            if (limits.signal && limits.signal.aborted) {
                const error = new Error("Acquisition cancelled; no snapshot returned.");
                error.code = "input.cancelled";
                throw error;
            }
            const elapsed = now() - started;
            if (elapsed > limits.timeBudgetMilliseconds) quotaError("input.time-budget", "timeBudgetMilliseconds", elapsed, limits.timeBudgetMilliseconds);
        } };
        operation.check();
        return operation;
    }

    function quotaError(code, limit, requested, allowed, message) {
        const error = new Error(message || `Acquisition limit ${limit}: requested ${requested}; allowed ${allowed}. ` +
            "Select less data or supply explicit bounded acquisition options.");
        Object.assign(error, { code, limit, requested, allowed });
        throw error;
    }

    function requireBoundedSource(range, limits) {
        const rows = range.rowCount;
        const columns = range.columnCount;
        if (!Number.isSafeInteger(rows) || !Number.isSafeInteger(columns) || rows < 1 || columns < 1) {
            throw new Error("The Excel source range has invalid dimensions.");
        }
        if (!Number.isSafeInteger(rows * columns) || rows * columns > limits.maxRangeCells) {
            quotaError("input.range-cells", "maxRangeCells", rows * columns, limits.maxRangeCells,
                `The source contains ${rows * columns} cells. The WasmSmoke input preview ` +
                `supports at most ${limits.maxRangeCells} cells; select a smaller populated range or supply explicit bounded options.`);
        }
    }

    function loadRangeCells(range) {
        range.load(["address", "values", "valueTypes", "numberFormat", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
    }

    function requireRangeCount(count, limits) {
        if (count > limits.maxRanges) quotaError("input.range-count", "maxRanges", count, limits.maxRanges);
    }

    function requireBoundedRangeSet(entries, operation) {
        operation.check();
        const limits = operation.limits;
        requireRangeCount(entries.length, limits);
        entries.forEach(entry => requireBoundedSource(entry.range, limits));
        let totalCells = 0;
        for (const entry of entries) {
            const cells = entry.range.rowCount * entry.range.columnCount;
            if (cells > limits.maxTotalCells - totalCells) {
                quotaError("input.total-cells", "maxTotalCells", totalCells + cells, limits.maxTotalCells,
                    `The selected range set contains ${totalCells + cells} cells; select fewer than ${limits.maxTotalCells + 1}.`);
            }
            totalCells += cells;
        }
        const estimate = totalCells * limits.estimatedBytesPerCell;
        if (estimate > limits.maxEstimatedBytes) quotaError("input.estimated-bytes", "maxEstimatedBytes", estimate, limits.maxEstimatedBytes);
    }

    // Lazy rectangular planner: columns first only when a single row exceeds the cap.
    function* tiles(range, cap) {
        const width = Math.min(range.columnCount, cap);
        const height = Math.max(1, Math.floor(cap / width));
        for (let row = 0; row < range.rowCount; row += height) {
            for (let column = 0; column < range.columnCount; column += width) {
                yield { row, column, rows: Math.min(height, range.rowCount - row),
                    columns: Math.min(width, range.columnCount - column) };
            }
        }
    }

    function verifyMetadata(entry) {
        // Snapshot construction validates host address/coordinates too, before payload.
        toRangeSnapshot(entry, []);
        const { range, worksheet } = entry;
        if (typeof worksheet.id !== "string" || !worksheet.id || typeof worksheet.name !== "string" || !worksheet.name ||
            !Number.isSafeInteger(range.rowIndex) || !Number.isSafeInteger(range.columnIndex) ||
            range.rowIndex < 0 || range.columnIndex < 0 ||
            range.rowIndex + range.rowCount > 1048576 || range.columnIndex + range.columnCount > 16384) {
            throw new Error("Unverified worksheet identity or Excel range bounds.");
        }
    }

    function verifyMatrix(matrix, tile, name) {
        if (!Array.isArray(matrix) || matrix.length !== tile.rows ||
            matrix.some(row => !Array.isArray(row) || row.length !== tile.columns)) {
            throw new Error(`Host chunk ${name} dimensions do not match the requested tile.`);
        }
    }

    async function readEntries(context, entries, operation) {
        requireBoundedRangeSet(entries, operation); // preserve aggregate-before-identity precedence
        entries.forEach(verifyMetadata);
        const snapshots = [];
        try {
            for (const entry of entries) {
                const source = entry.range;
                // Freeze the plan: a one-tile payload may refresh the source proxy's
                // own metadata, so comparisons must not use that same mutable proxy.
                const geometry = { rowIndex: source.rowIndex, columnIndex: source.columnIndex,
                    rowCount: source.rowCount, columnCount: source.columnCount };
                const rows = Array.from({ length: geometry.rowCount }, () => []);
                for (const tile of tiles(geometry, operation.limits.maxChunkCells)) {
                    operation.check();
                    const single = tile.rows === geometry.rowCount && tile.columns === geometry.columnCount;
                    const chunk = single ? source : entry.worksheet.getRangeByIndexes(
                        geometry.rowIndex + tile.row, geometry.columnIndex + tile.column, tile.rows, tile.columns);
                    loadRangeCells(chunk);
                    await context.sync(); // one bounded payload in flight, never a full large source
                    operation.check();
                    if (chunk.rowIndex !== geometry.rowIndex + tile.row || chunk.columnIndex !== geometry.columnIndex + tile.column ||
                        chunk.rowCount !== tile.rows || chunk.columnCount !== tile.columns) {
                        throw new Error("Host chunk positions or dimensions changed; refresh the selection.");
                    }
                    verifyMetadata({ ...entry, range: chunk, reference: null });
                    verifyMatrix(chunk.values, tile, "values");
                    verifyMatrix(chunk.valueTypes, tile, "valueTypes");
                    if (chunk.numberFormat !== undefined) verifyMatrix(chunk.numberFormat, tile, "numberFormat");
                    for (let row = 0; row < tile.rows; row++) {
                        for (let column = 0; column < tile.columns; column++) {
                            rows[tile.row + row][tile.column + column] = toPortableCell(chunk.values[row][column],
                                chunk.valueTypes[row][column], chunk.numberFormat ? chunk.numberFormat[row][column] : "");
                        }
                    }
                }
                snapshots.push(toRangeSnapshot(entry, rows));
            }
            operation.check();
            return snapshots;
        } catch (error) {
            snapshots.length = 0; // no partial usable result escapes; host request cannot be forcibly aborted
            throw error;
        }
    }

    async function readSelectedEntries(context, entries, operation) {
        operation.check();
        let narrowed = false;
        for (const entry of entries) {
            const selected = entry.range;
            if (selected.rowCount !== 1048576 && selected.columnCount !== 16384) continue;
            const requirements = typeof Office !== "undefined" && Office.context ? Office.context.requirements : null;
            const canUseNullSafeRange = requirements && typeof requirements.isSetSupported === "function" &&
                requirements.isSetSupported("ExcelApi", "1.4") === true;
            if (canUseNullSafeRange && typeof selected.getUsedRangeOrNullObject === "function") {
                entry.range = selected.getUsedRangeOrNullObject(true);
            } else if (typeof selected.getUsedRange === "function") {
                entry.range = selected.getUsedRange(true);
            } else {
                throw new Error("This Excel host cannot inspect populated cells in whole-row or whole-column selections.");
            }
            entry.range.load(["address", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
            narrowed = true;
        }
        if (narrowed) { await context.sync(); operation.check(); }
        if (entries.some(entry => entry.range.isNullObject)) {
            throw new Error("The selected whole rows or columns contain no populated cells.");
        }
        return readEntries(context, entries, operation);
    }

    function selectionReadError(error, message) {
        if (error && (error.code === "InvalidSelection" ||
            (error.debugInfo && error.debugInfo.code === "InvalidSelection"))) {
            const explanation = new Error(message);
            explanation.code = "InvalidSelection";
            return explanation;
        }
        return error;
    }

    async function getSelectedRangeData(options) {
        const operation = acquisitionOperation(options);
        await ensureExcelHost();
        return readSelectedRange(operation);
    }

    async function readSelectedRange(operation) {
        operation.check();
        try {
            return await Excel.run(async context => {
                const selected = context.workbook.getSelectedRange();
                // The selection owns the worksheet, even when it is not active.
                const worksheet = selected.worksheet;
                worksheet.load(["name", "id"]);
                // Load dimensions only. Loading values here can exceed Office.js response limits.
                selected.load(["address", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
                await context.sync();

                const snapshots = await readSelectedEntries(context, [{ worksheet, range: selected, reference: null }], operation);
                return snapshots[0];
            });
        } catch (error) {
            throw selectionReadError(error, "Select one rectangular range for this operation. " +
                "To preview multiple selected areas, use Read selected areas (01.09).");
        }
    }

    // ExcelApi 1.9 exposes RangeAreas. Preserve its order, never its bounding rectangle.
    // Older hosts retain the single-range API (which rejects disjoint selections).
    async function getSelectedRangeSetData(options) {
        const operation = acquisitionOperation(options);
        await ensureExcelHost();
        operation.check();
        const requirements = Office.context && Office.context.requirements;
        const supportsAreas = requirements && typeof requirements.isSetSupported === "function" &&
            requirements.isSetSupported("ExcelApi", "1.9") === true;
        try {
            if (!supportsAreas) {
                const range = await readSelectedRange(operation);
                operation.check();
                return { selectionId: "", ranges: [range] };
            }
            return await Excel.run(async context => {
                const areas = context.workbook.getSelectedRanges().areas;
                // Loading bare "items" can request default Range properties. Name only
                // dimensions so whole-column areas cannot trigger an early cell payload.
                areas.load(["items/rowCount", "items/columnCount"]);
                await context.sync();
                operation.check();
                if (!areas.items.length) throw new Error("At least one range must be selected.");
                requireRangeCount(areas.items.length, operation.limits);
                const entries = areas.items.map(range => {
                    const worksheet = range.worksheet;
                    worksheet.load(["name", "id"]);
                    range.load(["address", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
                    return { worksheet, range, reference: null };
                });
                await context.sync();
                return { selectionId: "", ranges: await readSelectedEntries(context, entries, operation) };
            });
        } catch (error) {
            throw selectionReadError(error, supportsAreas
                ? "Select worksheet cells and try Read selected areas (01.09) again."
                : "This Excel host needs ExcelApi 1.9 to read multiple selected areas. Select one rectangular range and try again.");
        }
    }

    async function getRangeDataByReference(reference, options) {
        const operation = acquisitionOperation(options);
        validateReference(reference); // fail before Excel.run
        await ensureExcelHost();
        operation.check();
        return Excel.run(async context => {
            const entry = prepareRange(context, reference);
            await context.sync();
            return (await readEntries(context, [entry], operation))[0];
        });
    }

    async function getRangeSetData(selection, options) {
        const operation = acquisitionOperation(options);
        if (!selection || !Array.isArray(selection.ranges) || selection.ranges.length === 0) {
            throw new Error("A nonempty structured range selection is required.");
        }
        requireRangeCount(selection.ranges.length, operation.limits);
        selection.ranges.forEach(validateReference);
        await ensureExcelHost();
        operation.check();
        return Excel.run(async context => {
            const entries = selection.ranges.map(reference => prepareRange(context, reference));
            await context.sync();
            return {
                selectionId: selection.selectionId || "",
                ranges: await readEntries(context, entries, operation)
            };
        });
    }

    function sanitizeWorksheetBaseName(name) {
        let value = typeof name === "string" ? name.trim() : "";
        if (!value) {
            value = "BESHStat_Result";
        }

        value = value.replace(/[\\/\?\*\[\]:]/g, "_");
        if (value.length > 31) {
            value = value.substring(0, 31);
        }
        return value || "BESHStat_Result";
    }

    function nextWorksheetName(existingNames, requestedBaseName) {
        const baseName = sanitizeWorksheetBaseName(requestedBaseName);
        const used = new Set(existingNames.map(name => String(name).toLowerCase()));

        if (!used.has(baseName.toLowerCase())) {
            return baseName;
        }

        for (let i = 2; i < 1000; i++) {
            const suffix = `_${i}`;
            const prefix = baseName.substring(0, Math.max(1, 31 - suffix.length));
            const candidate = prefix + suffix;
            if (!used.has(candidate.toLowerCase())) {
                return candidate;
            }
        }

        throw new Error("Unable to find an unused result worksheet name.");
    }

    function matrixOf(rowCount, columnCount, value) {
        return Array.from({ length: rowCount }, () =>
            Array.from({ length: columnCount }, () => value));
    }

    function applyPValueFormatting(worksheet, outputStartRow, outputStartColumn, payload, rowCount, columnCount) {
        const pvalueColumns = Array.isArray(payload.pvalueColumns) ? payload.pvalueColumns : [];
        const pvalueCells = Array.isArray(payload.pvalueCells) ? payload.pvalueCells : [];
        const numberFormat = payload.pvalueNumberFormat || "";
        const alpha = typeof payload.pvalueHighlightAlpha === "number" && Number.isFinite(payload.pvalueHighlightAlpha)
            ? payload.pvalueHighlightAlpha
            : 0.05;

        if ((!pvalueColumns.length && !pvalueCells.length) || !numberFormat) {
            return;
        }

        const titleRows = Math.max(0, Number(payload.titleRows) || 0);
        const headerTopRows = Math.max(0, Number(payload.headerTopRows) || 0);
        const headerLeftColumns = Math.max(0, Number(payload.headerLeftColumns) || 0);
        const footerRows = Math.max(0, Number(payload.footerRows) || 0);
        const firstBodyRow = titleRows + headerTopRows;
        const bodyRowCount = Math.max(0, rowCount - firstBodyRow - footerRows);

        if (bodyRowCount <= 0) {
            return;
        }

        const formattedCells = new Set();

        for (const bodyColumnRaw of pvalueColumns) {
            const bodyColumn = Number(bodyColumnRaw);
            const relativeColumn = headerLeftColumns + bodyColumn - 1;
            if (!Number.isInteger(relativeColumn) || relativeColumn < 0 || relativeColumn >= columnCount) {
                continue;
            }

            const range = worksheet.getRangeByIndexes(
                outputStartRow + firstBodyRow,
                outputStartColumn + relativeColumn,
                bodyRowCount,
                1);
            range.numberFormat = matrixOf(bodyRowCount, 1, numberFormat);

            for (let rowOffset = 0; rowOffset < bodyRowCount; rowOffset++) {
                const relativeRow = firstBodyRow + rowOffset;
                const raw = payload.values[relativeRow][relativeColumn];
                const key = `${relativeRow}:${relativeColumn}`;
                formattedCells.add(key);

                if (typeof raw === "number" && Number.isFinite(raw) && raw >= 0 && raw <= alpha) {
                    worksheet.getRangeByIndexes(
                        outputStartRow + relativeRow,
                        outputStartColumn + relativeColumn,
                        1,
                        1).format.font.color = "#32FF32";
                }
            }
        }

        for (const address of pvalueCells) {
            if (!address) {
                continue;
            }

            const bodyRow = Number(address.bodyRow);
            const bodyColumn = Number(address.bodyColumn);
            const relativeRow = firstBodyRow + bodyRow - 1;
            const relativeColumn = headerLeftColumns + bodyColumn - 1;

            if (!Number.isInteger(relativeRow) || !Number.isInteger(relativeColumn) ||
                relativeRow < firstBodyRow || relativeRow >= firstBodyRow + bodyRowCount ||
                relativeColumn < 0 || relativeColumn >= columnCount) {
                continue;
            }

            const key = `${relativeRow}:${relativeColumn}`;
            if (!formattedCells.has(key)) {
                const cell = worksheet.getRangeByIndexes(
                    outputStartRow + relativeRow,
                    outputStartColumn + relativeColumn,
                    1,
                    1);
                cell.numberFormat = [[numberFormat]];

                const raw = payload.values[relativeRow][relativeColumn];
                if (typeof raw === "number" && Number.isFinite(raw) && raw >= 0 && raw <= alpha) {
                    cell.format.font.color = "#32FF32";
                }
            }
        }
    }

    function applyResultTableFormatting(worksheet, outputStartRow, outputStartColumn, payload, rowCount, columnCount) {
        if (!payload.isResultTable) {
            return;
        }

        const titleRows = Math.max(0, Number(payload.titleRows) || 0);
        const headerTopRows = Math.max(0, Number(payload.headerTopRows) || 0);
        const headerLeftColumns = Math.max(0, Number(payload.headerLeftColumns) || 0);
        const footerRows = Math.max(0, Number(payload.footerRows) || 0);
        const firstBodyRow = titleRows + headerTopRows;
        const bodyRowCount = Math.max(0, rowCount - firstBodyRow - footerRows);

        if (headerTopRows > 0) {
            const topHeaders = worksheet.getRangeByIndexes(
                outputStartRow + titleRows,
                outputStartColumn,
                Math.min(headerTopRows, rowCount - titleRows),
                columnCount);
            topHeaders.format.font.bold = true;
            topHeaders.format.fill.color = "#DDDDDD";
        }

        if (headerLeftColumns > 0 && bodyRowCount > 0) {
            const leftHeaders = worksheet.getRangeByIndexes(
                outputStartRow + firstBodyRow,
                outputStartColumn,
                bodyRowCount,
                Math.min(headerLeftColumns, columnCount));
            leftHeaders.format.font.bold = true;
        }

        if (footerRows > 0 && footerRows <= rowCount) {
            const footers = worksheet.getRangeByIndexes(
                outputStartRow + rowCount - footerRows,
                outputStartColumn,
                footerRows,
                columnCount);
            footers.format.font.size = 8;
        }

        if (titleRows > 0) {
            const titles = worksheet.getRangeByIndexes(
                outputStartRow,
                outputStartColumn,
                Math.min(titleRows, rowCount),
                columnCount);
            titles.format.font.bold = false;
            titles.format.font.size = 10;
        }

        applyPValueFormatting(
            worksheet,
            outputStartRow,
            outputStartColumn,
            payload,
            rowCount,
            columnCount);
    }

    function validateResultPayload(payload) {
        if (!payload || !Array.isArray(payload.values) || payload.values.length === 0) {
            throw new Error("No result table values were supplied to the Office.js writer.");
        }

        const rowCount = payload.values.length;
        const columnCount = Array.isArray(payload.values[0]) ? payload.values[0].length : 0;

        if (columnCount === 0) {
            throw new Error("The result table has no columns.");
        }

        for (const row of payload.values) {
            if (!Array.isArray(row) || row.length !== columnCount) {
                throw new Error("The result table is not rectangular.");
            }
        }

        return { payload, rowCount, columnCount };
    }

    async function writeResultTables(payloads) {
        await ensureExcelHost();

        if (!Array.isArray(payloads) || payloads.length === 0) {
            throw new Error("No result-table requests were supplied to the Office.js writer.");
        }

        const tables = payloads.map(validateResultPayload);
        const firstTarget = tables[0].payload.outputTarget || {};
        const destination = Number.isInteger(Number(firstTarget.destination))
            ? Number(firstTarget.destination)
            : OUTPUT_DESTINATION.newWorksheet;
        const startRow = Math.max(1, Number(firstTarget.startRow) || 1) - 1;
        const startColumn = Math.max(1, Number(firstTarget.startColumn) || 1) - 1;

        const snapshot = await getHostCapabilities();
        if (!snapshot.isExcel || !snapshot.capabilities.includes(HOST_CAPABILITY.writeResultTables)) {
            throw new Error("Office.js result writing is unavailable in this Excel host.");
        }
        if (destination === OUTPUT_DESTINATION.newWorkbook) {
            throw new Error("New-workbook output is not supported by the Office.js host.");
        }
        if (destination !== OUTPUT_DESTINATION.currentWorksheet &&
            destination !== OUTPUT_DESTINATION.newWorksheet) {
            throw new Error("Unknown result output destination.");
        }
        const destinationCapability = destination === OUTPUT_DESTINATION.currentWorksheet
            ? HOST_CAPABILITY.currentWorksheet : HOST_CAPABILITY.newWorksheet;
        if (!snapshot.capabilities.includes(destinationCapability)) {
            throw new Error("The requested result output destination is unavailable.");
        }

        for (const table of tables) {
            const target = table.payload.outputTarget || {};
            const tableDestination = Number.isInteger(Number(target.destination))
                ? Number(target.destination)
                : destination;
            if (tableDestination !== destination) {
                throw new Error("All result tables in one batch must use the same output destination.");
            }
        }

        return Excel.run(async context => {
            const worksheets = context.workbook.worksheets;
            let worksheet;

            if (destination === OUTPUT_DESTINATION.currentWorksheet) {
                worksheet = worksheets.getActiveWorksheet();
                worksheet.load("name");
            } else {
                worksheets.load("items/name");
                await context.sync();

                const sheetName = nextWorksheetName(
                    worksheets.items.map(item => item.name),
                    firstTarget.worksheetName);
                worksheet = worksheets.add(sheetName);
                worksheet.load("name");
            }

            let cursorRow = startRow;
            let maxColumnCount = 0;
            const writtenRanges = [];

            for (let index = 0; index < tables.length; index++) {
                const table = tables[index];
                const payload = table.payload;
                const outputRange = worksheet.getRangeByIndexes(
                    cursorRow,
                    startColumn,
                    table.rowCount,
                    table.columnCount);

                outputRange.values = payload.values;
                applyResultTableFormatting(
                    worksheet,
                    cursorRow,
                    startColumn,
                    payload,
                    table.rowCount,
                    table.columnCount);

                if (payload.autoFit !== false) {
                    outputRange.format.autofitColumns();
                    outputRange.format.autofitRows();
                }

                outputRange.load("address");
                writtenRanges.push(outputRange);
                maxColumnCount = Math.max(maxColumnCount, table.columnCount);
                cursorRow += table.rowCount;
                if (index < tables.length - 1) {
                    cursorRow += 1;
                }
            }

            const totalRows = Math.max(1, cursorRow - startRow);
            const overallRange = worksheet.getRangeByIndexes(
                startRow,
                startColumn,
                totalRows,
                Math.max(1, maxColumnCount));

            worksheet.activate();
            overallRange.select();
            overallRange.load("address");

            await context.sync();

            return {
                worksheetName: worksheet.name,
                address: overallRange.address,
                tableAddresses: writtenRanges.map(range => range.address),
                pvalueColumns: Array.isArray(tables[0].payload.pvalueColumns)
                    ? tables[0].payload.pvalueColumns
                    : [],
                pvalueNumberFormat: tables[0].payload.pvalueNumberFormat || ""
            };
        });
    }

    async function writeResultTable(payload) {
        const result = await writeResultTables([payload]);
        if (Array.isArray(result.tableAddresses) && result.tableAddresses.length > 0) {
            result.address = result.tableAddresses[0];
        }
        return result;
    }

    window.beshOffice = {
        getHostStatus,
        getHostCapabilities,
        getSelectedRangeData,
        getSelectedRangeSetData,
        getRangeDataByReference,
        getRangeSetData,
        writeResultTable,
        writeResultTables
    };
})();
