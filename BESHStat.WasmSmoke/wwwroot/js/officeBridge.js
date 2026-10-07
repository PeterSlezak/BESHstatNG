(function () {
    "use strict";

    function delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async function getHostStatus(timeoutMs) {
        const timeout = Number.isFinite(timeoutMs) ? timeoutMs : 2000;

        if (typeof Office === "undefined") {
            return {
                officeJsLoaded: false,
                isExcel: false,
                host: "",
                platform: "",
                message: "Office.js is not loaded. Open this page from the Excel task pane."
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
                message: "Office.js loaded, but Office initialization failed: " + (error && error.message ? error.message : String(error))
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

    async function getSelectedAnovaData() {
        await ensureExcelHost();

        return Excel.run(async context => {
            const range = context.workbook.getSelectedRange();
            const worksheet = context.workbook.worksheets.getActiveWorksheet();

            range.load(["address", "values", "rowCount", "columnCount"]);
            worksheet.load("name");
            await context.sync();

            const values = range.values;

            if (range.columnCount < 2) {
                throw new Error("Select at least two group columns.");
            }
            if (range.rowCount < 3) {
                throw new Error("The selection must contain a header row plus at least two data rows.");
            }

            const headers = [];
            const columns = [];

            for (let col = 0; col < range.columnCount; col++) {
                const rawHeader = values[0][col];
                const header = rawHeader === null || rawHeader === undefined || String(rawHeader).trim() === ""
                    ? `Group ${col + 1}`
                    : String(rawHeader).trim();

                const group = [];
                for (let row = 1; row < range.rowCount; row++) {
                    const value = values[row][col];

                    if (value === null || value === undefined || value === "") {
                        continue;
                    }

                    if (typeof value !== "number" || !Number.isFinite(value)) {
                        throw new Error(
                            `Non-numeric value found in group '${header}' at selected row ${row + 1}. ` +
                            "For this Stage 2 test, data cells must be numeric or blank."
                        );
                    }

                    group.push(value);
                }

                if (group.length < 2) {
                    throw new Error(`Group '${header}' contains fewer than two numeric observations.`);
                }

                headers.push(header);
                columns.push(group);
            }

            return {
                worksheetName: worksheet.name,
                address: range.address,
                rowCount: range.rowCount,
                columnCount: range.columnCount,
                headers: headers,
                columns: columns
            };
        });
    }

    function nextWorksheetName(existingNames) {
        const baseName = "BESHStat_WASM_Result";
        const used = new Set(existingNames.map(name => name.toLowerCase()));

        if (!used.has(baseName.toLowerCase())) {
            return baseName;
        }

        for (let i = 2; i < 1000; i++) {
            const candidate = `${baseName}_${i}`;
            if (!used.has(candidate.toLowerCase())) {
                return candidate;
            }
        }

        throw new Error("Unable to find an unused result worksheet name.");
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

        return Excel.run(async context => {
            const worksheets = context.workbook.worksheets;
            worksheets.load("items/name");
            await context.sync();

            const sheetName = nextWorksheetName(worksheets.items.map(item => item.name));
            const worksheet = worksheets.add(sheetName);
            const outputRange = worksheet.getRangeByIndexes(0, 0, rowCount, columnCount);

            outputRange.values = payload.values;

            const titleRows = Math.max(0, payload.titleRows || 0);
            const headerTopRows = Math.max(0, payload.headerTopRows || 0);
            const headerLeftColumns = Math.max(0, payload.headerLeftColumns || 0);
            const footerRows = Math.max(0, payload.footerRows || 0);
            const topHeaderRows = Math.min(rowCount, titleRows + headerTopRows);

            if (topHeaderRows > 0) {
                const headerRange = worksheet.getRangeByIndexes(0, 0, topHeaderRows, columnCount);
                headerRange.format.font.bold = true;
            }

            const bodyStartRow = topHeaderRows;
            const bodyRowCount = Math.max(0, rowCount - bodyStartRow - footerRows);
            if (headerLeftColumns > 0 && bodyRowCount > 0) {
                const leftHeaderRange = worksheet.getRangeByIndexes(
                    bodyStartRow,
                    0,
                    bodyRowCount,
                    Math.min(headerLeftColumns, columnCount));
                leftHeaderRange.format.font.bold = true;
            }

            outputRange.format.autofitColumns();
            outputRange.format.autofitRows();
            worksheet.activate();
            outputRange.select();
            outputRange.load("address");

            await context.sync();

            return {
                worksheetName: sheetName,
                address: outputRange.address,
                pvalueColumns: Array.isArray(payload.pvalueColumns) ? payload.pvalueColumns : []
            };
        });
    }

    window.beshOffice = {
        getHostStatus,
        getSelectedAnovaData,
        writeResultTable
    };
})();
