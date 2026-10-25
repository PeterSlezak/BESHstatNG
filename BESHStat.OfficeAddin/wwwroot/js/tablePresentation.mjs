// Office host presentation profile. Common v1 roles, formats and significance remain authoritative.
// Matches the existing Windows/WasmSmoke table conventions; never changes values or column widths.
export function tablePresenter(table) {
    const last = new Map(); let footer = table.rowCount;
    for (const cell of table.cells) { last.set(cell.role, Math.max(last.get(cell.role) ?? -1, cell.row)); if (cell.role === "Footer") footer = Math.min(footer, cell.row); }
    const titled = last.has("Title");
    const bottomLine = row => row === footer - 1 || row === last.get("Title") || row === last.get("TopColumnHeader");
    return cell => Object.freeze({ bold: cell.strongEmphasis, color: cell.isSignificant ? "#32FF32" : "#000000",
        name: "Calibri Light", size: cell.role === "Footer" ? 8 : 10, alignment: "Left",
        fill: cell.role === "TopColumnHeader" ? "#DDDDDD" : null,
        // Adjacent cells share an edge in Excel. Express both sides consistently so a later top=None cannot erase a header's bottom line.
        top: cell.row === 0 ? !titled : bottomLine(cell.row - 1), bottom: bottomLine(cell.row) });
}
export const tablePresentation = (table, cell) => tablePresenter(table)(cell);

export function applyPresentation(cell, style) {
    const format = cell.format;
    Object.assign(format.font, { bold: style.bold, color: style.color, name: style.name, size: style.size });
    format.horizontalAlignment = style.alignment;
    format.fill.clear();
    if (style.fill) format.fill.color = style.fill;
    format.fill.pattern = style.fill ? "Solid" : "None";
    const borders = ["EdgeTop", "EdgeBottom", "EdgeLeft", "EdgeRight", "DiagonalDown", "DiagonalUp"].map((edge, index) => {
        const border = format.borders.getItem(edge), line = index === 0 ? style.top : index === 1 && style.bottom;
        border.style = line ? "Continuous" : "None";
        if (line) { border.weight = "Thin"; border.color = "#000000"; }
        return { border, line };
    });
    return { format, borders };
}

export function loadPresentation(item) {
    item.format.font.load("bold,color,name,size"); item.format.load("horizontalAlignment");
    item.format.fill.load("pattern,color"); item.borders.forEach(({border}) => border.load("style,weight,color"));
}

export const sameColor = (a, b) => typeof a === "string" && a.toUpperCase() === b;
export function presentationMatches(item, style, category, webFillPattern = false) {
    const f = item.format;
    switch (category) {
        case "font-name": return typeof f.font.name === "string" && f.font.name.toLowerCase() === style.name.toLowerCase();
        case "font-size": return f.font.size === style.size;
        case "alignment": return f.horizontalAlignment === style.alignment;
        case "fill":
            // Genuine Excel Online returns null even for a single cell's pattern.
            // Accept only that host representation, with exact background/no-fill
            // readback. Undefined, other patterns and all desktop nulls still fail.
            if (webFillPattern && f.fill.pattern === null)
                return style.fill ? sameColor(f.fill.color, style.fill) : f.fill.color === "";
            return f.fill.pattern === (style.fill ? "Solid" : "None") && (!style.fill || sameColor(f.fill.color, style.fill));
        case "borders": return item.borders.every(({border, line}) => border.style === (line ? "Continuous" : "None") &&
            (!line || border.weight === "Thin" && sameColor(border.color, "#000000")));
        default: return false;
    }
}
