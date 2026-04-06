# How to select data

BESHStatNG methods read data directly from the active workbook. From a user perspective there are two main input patterns:

1. **Select a cell range** using the custom **RefEdit** control (most tests and plots)
2. **Choose variables by column** from listboxes (regression-style workflows)

This page explains both, plus the most common pitfalls (headers, missing values, text in numeric columns).

---

## Pattern 1 — RefEdit range selection (most methods)

Many dialogs contain one or more *RefEdit* controls. They behave similarly to Excel’s built-in range pickers:

- You can **type** an address (e.g. `Sheet1!A1:B100`)
- Or click the small **collapse/select** button, pick a range in Excel, then return to the dialog

### Tips for RefEdit selection

- **Select a rectangular block** that contains the data you want to analyse.
- If your data includes a **header row**, many tools expect it in the *first row* of the selected range (names are used in outputs). If you don’t have headers, that’s ok.
- Keep your selection **consistent with the method**:
  - *One variable*: a single column
  - *Two groups*: two columns (or one value column + one group-id column, depending on the dialog)
  - *Paired data*: two columns with matched rows
  - *Multi-column repeated measures*: select the whole block of repeated-measure columns

!!! tip "You can include the sheet name"
    The RefEdit control can include the sheet name in the reference (e.g. `'MySheet'!A:B`). This makes the input stable even if you switch sheets.

---

## Pattern 2 — Choose variables by column (regression-style methods)

Methods such as **Cox regression**, **GLM**, **GEE**, and other multi-variable models let you select variables from listboxes instead of selecting a big range.

### How the column list is built

- BESHStatNG scans the **first row (Row 1)** for headers.
- A column is included if it contains at least some **non-missing numeric data** further down the sheet (unless text data are expected e.g. [Multiple Correspondence Analysis](methods/multiple-correspondence-analysis.md)).
- Display names are:
  - `HeaderName | VarA` when the column has a header in Row 1
  - `VarA`, `VarB`, … when the header is missing/empty

You can also pick a different worksheet from a drop-down and click **Reload** to re-scan columns.

### What happens when you click Run

Internally, the dialog builds an Excel reference from your selected columns (e.g. `Sheet!A:A, Sheet!C:C, ...`) and imports the data for analysis.

---

## Headers, missing values, and text in numeric columns

### Missing values

When importing data, BESHStatNG treats these as **missing**:

- empty cells
- Excel errors (e.g. `#N/A`, `#VALUE!`)
- (in numeric columns) text values that cannot be converted to numbers

Most analyses **remove whole rows** that contain missing values in any required column.

!!! note "Some tools may allow missing values"
    Certain workflows can allow missing values (treated as `NaN`) for specific analyses e.g. [Skilling's Mack test](methods/skillings-mack-test.md). In such case, a *Allow missing values* flag is used internally, and BESHStatNG will keep rows and mark missing cells as missing rather than dropping the row.

### Text in numeric columns

If a column is intended to be numeric but contains text, BESHStatNG will typically treat those cells as missing (or reject the row), depending on the dialog.

**Best practice:** keep numeric input columns clean (numbers only) and use a separate column for group IDs or labels.

---

## Common “gotchas” (quick fixes)

- **Nothing happens / zero valid data**: check that your selected range actually contains numbers (not empty cells or text).
- **Wrong variables in the list**: make sure Row 1 contains your headers and your data starts below it.
- **Unexpected row deletions**: look for blank cells or `#N/A` in any selected column.
- **You changed sheet data**: click **Reload** in regression dialogs to refresh the variable list.

If you still get stuck, see **Logs and error messages** in [Getting started](getting-started.md).
