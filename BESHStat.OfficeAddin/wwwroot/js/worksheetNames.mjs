// Private Office host policy. No mutations, workbook values, telemetry or Common schema changes.
const fail = code => { throw Object.assign(new Error(code), { safeCode: code }); };
export const worksheetNameKey = name => name.toUpperCase();
const fold = worksheetNameKey;
export function validWorksheetName(name) {
    return typeof name === "string" && name.length > 0 && name.length <= 31 && name.trim().length > 0 &&
        !/[\u0000-\u001f\u007f\\/?:*\[\]]/.test(name) && !name.startsWith("'") && !name.endsWith("'") &&
        fold(name) !== "HISTORY" && !/[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]/u.test(name);
}
export function allocateWorksheetName(requestedName, existingNames, maxSuffix = 10000) {
    if (!validWorksheetName(requestedName)) fail("output.target-invalid");
    if (!Array.isArray(existingNames) || existingNames.length > 10000 || !existingNames.every(validWorksheetName) ||
        !Number.isSafeInteger(maxSuffix) || maxSuffix < 1 || maxSuffix > 10000) fail("output.sheet-name-unavailable");
    const occupied = new Set(existingNames.map(fold));
    if (occupied.size !== existingNames.length) fail("output.sheet-name-unavailable");
    if (!occupied.has(fold(requestedName))) return { requestedName, resolvedName: requestedName, warningCode: null };
    for (let suffix = 1; suffix <= maxSuffix; suffix++) {
        const digits = String(suffix);
        let prefix = requestedName.slice(0, 31 - digits.length);
        // Do not cut a surrogate pair in half when reserving suffix space.
        if (/[\uD800-\uDBFF]$/.test(prefix)) prefix = prefix.slice(0, -1);
        const candidate = prefix + digits;
        if (validWorksheetName(candidate) && !occupied.has(fold(candidate)))
            return { requestedName, resolvedName: candidate, warningCode: "output.sheet-name-adjusted" };
    }
    fail("output.sheet-name-unavailable");
}
