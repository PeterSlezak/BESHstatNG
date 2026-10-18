# Batch 01.07 — Formula/design-matrix and model-input compilation

## Boundaries

`BESHStat.Excel.Common` adds `SpreadsheetModelInputCompiler.Compile` and the
immutable (defensively copied) `SpreadsheetModelInput` contract. It reuses
`RectangularCategoricalParsers.ParseDataFrame`, `SpreadsheetCellValuePolicy`,
`SpreadsheetCategoryValue`, `SpreadsheetValidationIssue`, and Core's
`RegressionFormulaParser`, `RegressionFormulaDesignService`, and
`RegressionDesignCore`. There is **no replacement formula parser**, model fitting,
Excel-DNA UI modification, Office.js UI modification, or new Core API.

## Invocation / role model

- Column indices are zero-based *relative to the supplied rectangle*. The
  response column defaults to index 0; all other columns become predictors
  unless `PredictorColumnIndices` selects an explicit subset/order.
- The existing Core **right-hand-side-only** syntax is used. `FormulaText`
  supports relative predictor letters (`A` = first selected predictor),
  quoted header names (`'Dose mg'`), `factor(A)`, `factor(A, ref=2)`,
  polynomial `A^2`, and supported `:` interactions. Blank formula means Core's
  default main effects, **not an intercept-only model**.
- Intercept policy is explicit (`IncludeIntercept`, default `True`).
  Core expands X without adding an intercept; Common prepends it. With an
  intercept, Core omits factor reference columns; without an intercept it
  retains them. This matches Core's factor-dummy expansion policy.
- Categorical predictors are explicitly selected via
  `CategoricalColumnIndices` and must be referenced with `factor(...)`.
  They retain text/number/Boolean identity. Entirely numeric levels retain
  their original numeric values (and entirely Boolean levels use 0/1), matching
  Core's existing `factor(X, ref=N)` reference semantics. Text or mixed-type
  categories receive 1-based codes in complete-case first-appearance order.
  The codebook maps each code back to its original typed value; for text/mixed
  categories `ref=N` refers to the encoded code. A future UI should offer a
  typed reference-level picker.
- `AllowAbsoluteColumnLetters` is optional and off by default. Core's absolute
  letters use the supplied worksheet column coordinates only as an Excel
  compatibility addressing option; the normalized design does not depend on
  Excel/Office objects. Avoid enabling relative and absolute address styles
  together when they create ambiguous references.

## Missing data / diagnostics

Only the response and formula-required predictors are read for complete-case
eligibility. Omitted/unreferenced columns do not drop rows, even if they contain
spreadsheet error cells. Missing rows are rejected by default; `Skip` keeps
original selected/worksheet row IDs, emits per-row warning, and **never** shifts
Y relative to X. Error cells are rejected unless an explicit cell policy marks
them missing; text-to-number and Boolean coercions remain opt-in. Neither
formula text nor source text is interpreted as a worksheet formula.

Core errors are returned as structured `model.formula` or `model.design`
issues. Rank is assessed on the *final intercept-inclusive* design by
reorthogonalized modified Gram-Schmidt at a relative threshold of 1e-10.
Rank-deficiency is a warning by default, or an error when
`RejectRankDeficiency` is `True`. This is an input preflight, **not** the
model fitting engine's pivoted QR/SVD tolerance or a guarantee of inferential
identifiability. Model-family-specific constraints and GLM link/covariance
selection remain later batches.

## Contract guarantees

The response, expanded design, raw numerical predictor encodings,
column names, source row IDs and factor codebook align by row. Public matrix
and response accessors return cloned arrays. Workbook/worksheet identity and
selection revision are preserved for stale-selection checks by later host
adapters. No locale- or date-based conversion is inferred from cell formatting:
only explicitly configured numeric text conversion occurs. The output can be
passed to a future LM/GLM/GEE adapter without re-reading the worksheet.

## Limitations / future work

* Formula request accepts a **single response** and a RHS-only formula; not a
  full `y ~ x` script. Neither a random-effects formula nor an outcome-family
  specification is compiled in 01.07. Existing 01.06 mixed/repeated parsers
  remain the correct sources for GEE/LMM/MMRM subject and visit roles.
* Text category contrasts currently use Core treatment dummies. The supplied
  codebook is required to relabel coefficients meaningfully. General sum/Helmert
  contrasts and typed UI reference-level selection are deferred to model
  analysis batches rather than silently implying support.
* `MaximumExpandedColumns` checks the **result** of Core expansion, not an
  input-size/CPU quota. Bounded large-range acquisition belongs to 01.10.
* This batch does not modify the 00.01 golden fixtures, analysis handlers,
  Windows interop, Office.js bridge, documentation manifest, or Core math.
