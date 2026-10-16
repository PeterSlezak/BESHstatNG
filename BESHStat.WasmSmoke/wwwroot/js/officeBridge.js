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

    function toPortableCell(value, valueType) {
        const typeName = valueType === null || valueType === undefined
            ? ""
            : String(valueType).toLowerCase();

        if (typeName === "empty" || value === null || value === undefined || value === "") {
            return { kind: CELL_KIND.blank, numberValue: 0, textValue: "", booleanValue: false };
        }

        if (typeName === "error") {
            return { kind: CELL_KIND.error, numberValue: 0, textValue: String(value), booleanValue: false };
        }

        if (typeName === "boolean" || typeof value === "boolean") {
            return { kind: CELL_KIND.boolean, numberValue: 0, textValue: "", booleanValue: Boolean(value) };
        }

        if (typeName === "integer" || typeName === "double" || typeof value === "number") {
            if (typeof value !== "number" || !Number.isFinite(value)) {
                return { kind: CELL_KIND.error, numberValue: 0, textValue: String(value), booleanValue: false };
            }
            return { kind: CELL_KIND.number, numberValue: value, textValue: "", booleanValue: false };
        }

        return { kind: CELL_KIND.text, numberValue: 0, textValue: String(value), booleanValue: false };
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
        const actualBook = currentWorkbookName();
        if (reference.workbookName && (!actualBook || reference.workbookName.toLowerCase() !== actualBook.toLowerCase())) {
            throw new Error("The requested workbook is not the current Office workbook.");
        }
    }

    function prepareRange(context, reference) {
        validateReference(reference);
        const worksheet = context.workbook.worksheets.getItem(reference.worksheetName);
        const range = worksheet.getRange(reference.address);
        worksheet.load(["name", "id"]);
        range.load(["address", "values", "valueTypes", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
        return { worksheet, range, reference };
    }

    function toRangeSnapshot(entry) {
        const { range, worksheet, reference } = entry;
        if (reference && reference.worksheetId && reference.worksheetId !== worksheet.id) {
            throw new Error("The source worksheet identity changed; refresh the selection.");
        }
        const rows = [];
        for (let r = 0; r < range.rowCount; r++) {
            const row = [];
            for (let c = 0; c < range.columnCount; c++) {
                row.push(toPortableCell(range.values[r][c], range.valueTypes[r][c]));
            }
            rows.push(row);
        }
        const localAddress = range.address.indexOf("!") >= 0
            ? range.address.slice(range.address.lastIndexOf("!") + 1) : range.address;
        return {
            workbookId: "", workbookName: currentWorkbookName(),
            worksheetId: worksheet.id || "", worksheetName: worksheet.name,
            address: localAddress, startRow: range.rowIndex + 1, startColumn: range.columnIndex + 1,
            rowCount: range.rowCount, columnCount: range.columnCount,
            selectionId: reference && reference.selectionId ? reference.selectionId : "",
            selectionRevision: "", rows
        };
    }

    async function getSelectedRangeData() {
        await ensureExcelHost();
        return Excel.run(async context => {
            const range = context.workbook.getSelectedRange();
            // The selected range owns its worksheet; never substitute the active sheet.
            const worksheet = range.worksheet;
            worksheet.load(["name", "id"]);
            range.load(["address", "values", "valueTypes", "rowCount", "columnCount", "rowIndex", "columnIndex"]);
            await context.sync();
            return toRangeSnapshot({ worksheet, range, reference: null });
        });
    }

    async function getRangeDataByReference(reference) {
        validateReference(reference); // fail before Excel.run
        await ensureExcelHost();
        return Excel.run(async context => {
            const entry = prepareRange(context, reference);
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
        getRangeDataByReference,
        getRangeSetData,
        writeResultTable,
        writeResultTables
    };
})();
