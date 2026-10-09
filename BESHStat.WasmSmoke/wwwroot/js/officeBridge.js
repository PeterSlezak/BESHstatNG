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

    async function getSelectedRangeData() {
        await ensureExcelHost();

        return Excel.run(async context => {
            const range = context.workbook.getSelectedRange();
            const worksheet = context.workbook.worksheets.getActiveWorksheet();

            range.load(["address", "values", "valueTypes", "rowCount", "columnCount"]);
            worksheet.load("name");
            await context.sync();

            const rows = [];
            for (let rowIndex = 0; rowIndex < range.rowCount; rowIndex++) {
                const row = [];
                for (let columnIndex = 0; columnIndex < range.columnCount; columnIndex++) {
                    row.push(toPortableCell(
                        range.values[rowIndex][columnIndex],
                        range.valueTypes[rowIndex][columnIndex]));
                }
                rows.push(row);
            }

            return {
                worksheetName: worksheet.name,
                address: range.address,
                rowCount: range.rowCount,
                columnCount: range.columnCount,
                rows: rows
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

    async function writeResultTable(payload) {
        await ensureExcelHost();

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

        const target = payload.outputTarget || {};
        const destination = Number.isInteger(Number(target.destination))
            ? Number(target.destination)
            : OUTPUT_DESTINATION.newWorksheet;
        const startRow = Math.max(1, Number(target.startRow) || 1) - 1;
        const startColumn = Math.max(1, Number(target.startColumn) || 1) - 1;

        if (destination === OUTPUT_DESTINATION.newWorkbook) {
            throw new Error("New-workbook output is reserved for the next Stage 3 host-capability batch.");
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
                    target.worksheetName);
                worksheet = worksheets.add(sheetName);
                worksheet.load("name");
            }

            const outputRange = worksheet.getRangeByIndexes(
                startRow,
                startColumn,
                rowCount,
                columnCount);

            outputRange.values = payload.values;
            applyResultTableFormatting(
                worksheet,
                startRow,
                startColumn,
                payload,
                rowCount,
                columnCount);

            if (payload.autoFit !== false) {
                outputRange.format.autofitColumns();
                outputRange.format.autofitRows();
            }

            worksheet.activate();
            outputRange.select();
            outputRange.load("address");

            await context.sync();

            return {
                worksheetName: worksheet.name,
                address: outputRange.address,
                pvalueColumns: Array.isArray(payload.pvalueColumns) ? payload.pvalueColumns : [],
                pvalueNumberFormat: payload.pvalueNumberFormat || ""
            };
        });
    }

    window.beshOffice = {
        getHostStatus,
        getSelectedRangeData,
        writeResultTable
    };
})();
