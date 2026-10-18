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
        const typeName = valueType === null || valueType === undefined
            ? ""
            : String(valueType).toLowerCase();
        const format = typeof numberFormat === "string" ? numberFormat : "";
        const base = { kind: CELL_KIND.blank, numberValue: 0, textValue: "", booleanValue: false,
            numberFormat: format, nativeErrorCode: "" };
        if (typeName === "error") {
            return { ...base, kind: CELL_KIND.error, textValue: String(value ?? "") };
        }
        if (typeName === "empty" || value === null || value === undefined) return base;
        if (typeName === "boolean" || typeof value === "boolean") {
            return { ...base, kind: CELL_KIND.boolean, booleanValue: Boolean(value) };
        }
        if (typeName === "integer" || typeName === "double" || (typeName === "" && typeof value === "number")) {
            if (typeof value !== "number" || !Number.isFinite(value)) {
                return { ...base, kind: CELL_KIND.error, textValue: String(value) };
            }
            return { ...base, kind: CELL_KIND.number, numberValue: value };
        }
        if (typeName === "string" || typeName === "richvalue" || typeof value === "string") {
            if (value === "" && typeName === "") return base; // old hosts with no valueTypes
            return { ...base, kind: CELL_KIND.text, textValue: String(value) };
        }
        return { ...base, kind: CELL_KIND.text, textValue: String(value) };
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
        range.load(["rowCount", "columnCount", "rowIndex", "columnIndex"]);
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

    function toRangeSnapshot(entry) {
        const { range, worksheet, reference } = entry;
        verifyEntry(entry);
        const rows = [];
        for (let r = 0; r < range.rowCount; r++) {
            const row = [];
            for (let c = 0; c < range.columnCount; c++) {
                row.push(toPortableCell(range.values[r][c], range.valueTypes[r][c],
                    range.numberFormat && range.numberFormat[r] ? range.numberFormat[r][c] : ""));
            }
            rows.push(row);
        }
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

    // A full-column selection in Excel contains 1,048,576 rows. Never load its
    // cell matrices before asking Excel for the occupied portion of that range.
    // A conservative temporary ceiling protects the smoke harness; large-range
    // chunking and quota policy are reserved for the later input-acquisition batch.
    const MAX_SOURCE_CELLS = 10000;

    function requireBoundedSource(range) {
        const rows = Number(range.rowCount);
        const columns = Number(range.columnCount);
        if (!Number.isSafeInteger(rows) || !Number.isSafeInteger(columns) || rows < 1 || columns < 1) {
            throw new Error("The Excel source range has invalid dimensions.");
        }
        if (rows * columns > MAX_SOURCE_CELLS) {
            throw new Error(`The source contains ${rows * columns} cells. The WasmSmoke input preview ` +
                `currently supports at most ${MAX_SOURCE_CELLS} cells; select a smaller populated range.`);
        }
    }

    function loadRangeCells(range) {
        range.load(["address", "values", "valueTypes", "numberFormat", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
    }

    function requireBoundedRangeSet(entries) {
        entries.forEach(entry => requireBoundedSource(entry.range));
        const totalCells = entries.reduce((sum, entry) => sum + entry.range.rowCount * entry.range.columnCount, 0);
        if (totalCells > MAX_SOURCE_CELLS) {
            throw new Error(`The selected range set contains ${totalCells} cells; select fewer than ${MAX_SOURCE_CELLS + 1}.`);
        }
    }

    async function readSelectedEntries(context, entries) {
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
            entry.range.load(["rowCount", "columnCount"]);
            narrowed = true;
        }
        if (narrowed) await context.sync();
        if (entries.some(entry => entry.range.isNullObject)) {
            throw new Error("The selected whole rows or columns contain no populated cells.");
        }
        requireBoundedRangeSet(entries);
        entries.forEach(entry => loadRangeCells(entry.range));
        await context.sync();
        return entries.map(toRangeSnapshot);
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

    async function getSelectedRangeData() {
        await ensureExcelHost();
        try {
            return await Excel.run(async context => {
                const selected = context.workbook.getSelectedRange();
                // The selection owns the worksheet, even when it is not active.
                const worksheet = selected.worksheet;
                worksheet.load(["name", "id"]);
                // Load dimensions only. Loading values here can exceed Office.js response limits.
                selected.load(["rowCount", "columnCount"]);
                await context.sync();

                const snapshots = await readSelectedEntries(context, [{ worksheet, range: selected, reference: null }]);
                return snapshots[0];
            });
        } catch (error) {
            throw selectionReadError(error, "Select one rectangular range for this operation. " +
                "To preview multiple selected areas, use Read selected areas (01.09).");
        }
    }

    // ExcelApi 1.9 exposes RangeAreas. Preserve its order, never its bounding rectangle.
    // Older hosts retain the single-range API (which rejects disjoint selections).
    async function getSelectedRangeSetData() {
        await ensureExcelHost();
        const requirements = Office.context && Office.context.requirements;
        const supportsAreas = requirements && typeof requirements.isSetSupported === "function" &&
            requirements.isSetSupported("ExcelApi", "1.9") === true;
        try {
            if (!supportsAreas) return { selectionId: "", ranges: [await getSelectedRangeData()] };
            return await Excel.run(async context => {
                const areas = context.workbook.getSelectedRanges().areas;
                // Loading bare "items" can request default Range properties. Name only
                // dimensions so whole-column areas cannot trigger an early cell payload.
                areas.load(["items/rowCount", "items/columnCount"]);
                await context.sync();
                if (!areas.items.length) throw new Error("At least one range must be selected.");
                const entries = areas.items.map(range => {
                    const worksheet = range.worksheet;
                    worksheet.load(["name", "id"]);
                    range.load(["rowCount", "columnCount"]);
                    return { worksheet, range, reference: null };
                });
                await context.sync();
                return { selectionId: "", ranges: await readSelectedEntries(context, entries) };
            });
        } catch (error) {
            throw selectionReadError(error, supportsAreas
                ? "Select worksheet cells and try Read selected areas (01.09) again."
                : "This Excel host needs ExcelApi 1.9 to read multiple selected areas. Select one rectangular range and try again.");
        }
    }

    async function getRangeDataByReference(reference) {
        validateReference(reference); // fail before Excel.run
        await ensureExcelHost();
        return Excel.run(async context => {
            const entry = prepareRange(context, reference);
            await context.sync();
            verifyEntry(entry);
            requireBoundedSource(entry.range);
            loadRangeCells(entry.range);
            await context.sync();
            return toRangeSnapshot(entry);
        });
    }

    async function getRangeSetData(selection) {
        if (!selection || !Array.isArray(selection.ranges) || selection.ranges.length === 0) {
            throw new Error("A nonempty structured range selection is required.");
        }
        selection.ranges.forEach(validateReference);
        await ensureExcelHost();
        return Excel.run(async context => {
            const entries = selection.ranges.map(reference => prepareRange(context, reference));
            await context.sync();
            requireBoundedRangeSet(entries);
            entries.forEach(verifyEntry);
            entries.forEach(entry => loadRangeCells(entry.range));
            await context.sync();
            return {
                selectionId: selection.selectionId || "",
                ranges: entries.map(toRangeSnapshot)
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
