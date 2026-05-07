# MMRM worksheet functions

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** show how to run **Mixed Models for Repeated Measures (MMRM)** from worksheet formulas instead of the ribbon dialog. The worksheet functions are useful when you need a reproducible workbook, want to refresh the same analysis after updating the data, or need custom LS-mean estimates that are not produced automatically by the graphical output. For the exact generated function signatures, see the [Regression Models UDF reference](../../udf/regression-models.md#beshregrmmrm_fit). For the meaning of LS-means and contrasts, see [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 1. When to use worksheet functions

The ribbon dialog is the easiest way to fit an MMRM interactively. Worksheet functions are better when the analysis should be transparent, reusable, or embedded in a workbook template.

Use the `BESH.REGR.MMRM_*` functions when you want to:

- keep the complete model definition visible in cells;
- refresh the model after replacing or extending the dataset;
- extract only selected tables into a reporting sheet;
- compare several covariance structures or inference methods side by side;
- create a workbook that documents the data range, formula, model options, and output tables;
- define custom LS-mean estimates or contrasts with `BESH.REGR.MMRM_LSMESTIMATE`.

!!! tip "Recommended workflow"
    Use the ribbon first when learning a model or checking data setup. Use worksheet functions once the analysis is stable and should be reproducible.

---

## 2. Example workbook used on this page

The examples on this page use the FEV1 data and UDF workbook included with the documentation:

- [037mmrm_fev_data.csv](../../assets/data/037mmrm/037mmrm_fev_data.csv)
- [037mmrm_fev_data_udfs.xlsx](../../assets/data/037mmrm/037mmrm_fev_data_udfs.xlsx)
- [037mmrm_fev_results.xlsx](../../assets/data/037mmrm/037mmrm_fev_results.xlsx)

The UDF workbook fits the same basic treatment-by-visit MMRM used elsewhere in the documentation and demonstrates a custom contrast with `BESH.REGR.MMRM_LSMESTIMATE`.

![UDF workbook demonstrating an MMRM fit handle, custom LS-mean estimate specification, and returned contrast table.](../../assets/images/037mmrm/037mmrm_udf_lsmestimate.png)

In the workbook, the data are in long format with one row per subject visit. Important columns include:

| Column | Variable | Role in the example |
|---|---|---|
| `B` | `SUBJID` | Subject identifier. |
| `E` | `ARMCDN` | Numeric treatment code. |
| `G` | `RACEN` | Numeric race code. |
| `I` | `SEXN` | Numeric sex code. |
| `K` | `FEV1` | Continuous response. |
| `M` | `VISITN` | Numeric visit/time value. |

The fitted model uses the right-hand-side formula:

```text
factor(g)+factor(i)+factor(e)+factor(m)+e:m
```

Because the formula-addressing mode is `"absolute"`, the letters in the formula refer to worksheet columns: `g` is `RACEN`, `i` is `SEXN`, `e` is `ARMCDN`, and `m` is `VISITN`. The term `e:m` is the treatment-by-visit interaction using the numeric design columns. See [Regression formula syntax](../../udf/regression-formula-syntax.md) for details on formula expansion.

!!! note "Do not type the internal `_xll.` prefix"
    Some workbooks may display Excel-DNA formulas internally as `_xll.BESH.REGR...`. Users should enter the public worksheet function names shown in this documentation, for example `=BESH.REGR.MMRM_FIT(...)`.

---

## 3. Function family overview

The MMRM worksheet workflow has one fitting function, several extractor functions, and two cache-management functions.

| Function | Main use |
|---|---|
| [`BESH.REGR.MMRM_FIT`](../../udf/regression-models.md#beshregrmmrm_fit) | Fit the MMRM and return a reusable session handle. |
| [`BESH.REGR.MMRM_RESULTS`](../../udf/regression-models.md#beshregrmmrm_results) | Return all result tables or one selected table. |
| [`BESH.REGR.MMRM_COEF`](../../udf/regression-models.md#beshregrmmrm_coef) | Return the fixed-effect coefficient table. |
| [`BESH.REGR.MMRM_TYPE3`](../../udf/regression-models.md#beshregrmmrm_type3) | Return term-level fixed-effect F tests. |
| [`BESH.REGR.MMRM_COVPARMS`](../../udf/regression-models.md#beshregrmmrm_covparms) | Return covariance-parameter estimates. |
| [`BESH.REGR.MMRM_R_COV`](../../udf/regression-models.md#beshregrmmrm_r_cov) | Return the fitted within-subject covariance matrix. |
| [`BESH.REGR.MMRM_R_CORR`](../../udf/regression-models.md#beshregrmmrm_r_corr) | Return the fitted within-subject correlation matrix. |
| [`BESH.REGR.MMRM_FITSTATS`](../../udf/regression-models.md#beshregrmmrm_fitstats) | Return likelihood, information-criterion, model-size, and convergence statistics. |
| [`BESH.REGR.MMRM_LSMEANS`](../../udf/regression-models.md#beshregrmmrm_lsmeans) | Return observed-design-grid estimated marginal means. |
| [`BESH.REGR.MMRM_CONTRASTS`](../../udf/regression-models.md#beshregrmmrm_contrasts) | Return observed-design-grid group contrasts by visit. |
| [`BESH.REGR.MMRM_LSMESTIMATE`](../../udf/regression-models.md#beshregrmmrm_lsmestimate) | Return custom SAS-style LS-mean estimates or contrasts. |
| [`BESH.REGR.MMRM_FITTED`](../../udf/regression-models.md#beshregrmmrm_fitted) | Return row-level marginal fitted values. |
| [`BESH.REGR.MMRM_RESID`](../../udf/regression-models.md#beshregrmmrm_resid) | Return row-level marginal fitted values and raw residuals. |
| [`BESH.REGR.MMRM_DROP`](../../udf/regression-models.md#beshregrmmrm_drop) | Remove one fitted handle from the session cache. |
| [`BESH.REGR.MMRM_CLEAR_ALL`](../../udf/regression-models.md#beshregrmmrm_clear_all) | Remove all fitted MMRM handles from the session cache. |

---

## 4. Basic worksheet pattern

The worksheet functions are designed around a **fit once, extract many times** pattern.

### Step 1: fit the model and save the handle

Enter `BESH.REGR.MMRM_FIT` in a cell that will hold the model handle. The handle is an opaque text value such as `MMRM:...`; do not edit it manually.

A generic formula is:

```excel
=BESH.REGR.MMRM_FIT(y, x, subject, visit, varNames, covariance, fitMethod, inference, includeIntercept, formula, formulaAddressing, alpha, maxIter, trace, covOptimizerMode, covGradientMode)
```

A formula matching the supplied FEV1 UDF workbook is:

```excel
=BESH.REGR.MMRM_FIT(K2:K801,E2:M801,B2:B801,M2:M801,E1:M1,,,"kr",,"factor(g)+factor(i)+factor(e)+factor(m)+e:m","absolute")
```

This formula means:

| Argument | Example value | Meaning |
|---|---|---|
| `y` | `K2:K801` | Response column `FEV1`. |
| `x` | `E2:M801` | Raw predictor block available to the formula expander. |
| `subject` | `B2:B801` | Subject identifier `SUBJID`. |
| `visit` | `M2:M801` | Visit/time variable `VISITN`. |
| `varNames` | `E1:M1` | Predictor names. |
| `covariance` | blank | Use the default unstructured covariance. |
| `fitMethod` | blank | Use the default REML fit. |
| `inference` | `"kr"` | Use Kenward-Roger inference. |
| `includeIntercept` | blank | Use the default intercept. |
| `formula` | `"factor(g)+factor(i)+factor(e)+factor(m)+e:m"` | Build fixed effects from selected worksheet columns. |
| `formulaAddressing` | `"absolute"` | Interpret formula letters as worksheet column letters. |

Blank optional arguments use the documented defaults. The default MMRM fit is REML, unstructured covariance, intercept included, and Kenward-Roger inference.

### Step 2: extract standard result tables

Assume the handle is in cell `Q1`. The most common extractor formulas are:

```excel
=BESH.REGR.MMRM_RESULTS(Q1)
=BESH.REGR.MMRM_COEF(Q1)
=BESH.REGR.MMRM_TYPE3(Q1)
=BESH.REGR.MMRM_COVPARMS(Q1)
=BESH.REGR.MMRM_FITSTATS(Q1)
```

### Step 3: extract covariance matrices

```excel
=BESH.REGR.MMRM_R_COV(Q1)
=BESH.REGR.MMRM_R_CORR(Q1)
```

The covariance matrix is on the response scale. The correlation matrix is often easier to read when comparing association between visits.

### Step 4: extract LS-means and contrasts

```excel
=BESH.REGR.MMRM_LSMEANS(Q1)
=BESH.REGR.MMRM_LSMEANS(Q1,"ARMCDN[2]")
=BESH.REGR.MMRM_CONTRASTS(Q1,"ARMCDN[2]","Each group vs control",0)
```

The exact grouping column name must match a fitted design column saved in the model handle. Formula-expanded factors commonly use names such as `ARMCDN[2]`, `SEXN[1]`, or `VISITN[4]`.

### Step 5: extract fitted values and residuals

```excel
=BESH.REGR.MMRM_FITTED(Q1)
=BESH.REGR.MMRM_RESID(Q1)
```

These extractors return rows corresponding to the valid analysis rows retained by the fit. Rows excluded because of missing or invalid response, predictor, subject, or visit values are not returned.

---

## 5. Dynamic-array output

MMRM worksheet functions return **dynamic arrays**. Enter each extractor formula in the top-left cell of an empty output area and allow Excel to spill the full table.

Practical rules:

- Leave enough blank rows and columns below/right of the formula cell.
- Do not type over a spilled result range.
- Place separate extractor formulas in separate blocks or sheets.
- Format the spilled result as needed after the formula has returned.
- If Excel shows a spill error, clear the cells blocking the output area.

For reporting workbooks, keep the fit handle in one cell and refer to that cell from all output formulas. This avoids accidentally fitting slightly different models in different parts of the workbook.

---

## 6. Input requirements for `BESH.REGR.MMRM_FIT`

### Required data shape

The worksheet input must be in **long format**: one row per subject per visit or time point.

| Requirement | Practical check |
|---|---|
| Continuous response | The `y` range must contain the numeric outcome, such as `FEV1`. |
| Subject identifier | The `subject` range must identify which rows belong to the same participant. |
| Visit/time variable | The `visit` range should be numeric when visit order matters or when using visit-based covariance structures and LS-means. |
| Fixed-effect predictors | The `x` range must contain numeric raw variables or already-coded design columns. |
| Same row order | `y`, `x`, `subject`, and `visit` must refer to the same worksheet rows. |
| Header names | `varNames` should match the columns of `x` when formulas or readable output are desired. |

Subject identifiers may be text or numeric. Response and predictor values must be numeric after any formula expansion. Rows with missing or invalid values in required inputs are removed before the model is fitted.

### Using raw predictors with a formula

The `formula` argument lets you supply raw numeric columns and build the fixed-effect design matrix inside the function. This is usually clearer than manually creating every dummy and interaction column.

Examples:

```excel
=BESH.REGR.MMRM_FIT(K2:K801,E2:M801,B2:B801,M2:M801,E1:M1,,,"KR",,"factor(g)+factor(i)+factor(e)+factor(m)+e:m","absolute")
```

```excel
=BESH.REGR.MMRM_FIT(K2:K801,E2:M801,B2:B801,M2:M801,E1:M1,"UN","REML","KR",TRUE,"factor(ARMCDN)+factor(VISITN)+ARMCDN:VISITN","names")
```

Use `"absolute"` when formulas refer to worksheet column letters. Use `"names"` when formulas refer to the names supplied in `varNames`.

### Validating a formula before fitting

The supplied UDF workbook also uses formula validation:

```excel
=BESH.REGR.FORMULA_VALIDATE(Q5,E2:M801,E1:M1,"absolute")
```

This is not required for fitting, but it is helpful when creating a workbook template because it checks that the model formula can be expanded from the supplied predictor block.

---

## 7. Main `MMRM_FIT` options

| Argument | Accepted/common values | Practical guidance |
|---|---|---|
| `covariance` | `ID`, `Diagonal`, `CS`, `HCS`, `AR(1)`, `HAR(1)`, `UN` | Start with `UN` for a standard repeated-visit MMRM when the number of visits and sample size support it. Use simpler structures when the unstructured fit is unstable or when required by the analysis plan. |
| `fitMethod` | `REML`, `ML` | Use `REML` for routine fixed-effect inference and Kenward-Roger analyses. Use `ML` mainly for comparing different fixed-effect mean models. |
| `inference` | `KR`, `Satterthwaite`, `BetweenWithin`, `ResidualDF`, `Wald` | `KR` is the default recommendation for many MMRM analyses. Use other methods for sensitivity analyses, compatibility, speed, or large-sample workflows. |
| `includeIntercept` | `TRUE`, `FALSE` | Usually leave blank or use `TRUE`. Set `FALSE` only if the supplied design matrix already contains the intended intercept/cell-means coding. |
| `formula` | Right-hand-side model formula | Use to generate factors and interactions reproducibly from raw columns. |
| `formulaAddressing` | `relative`, `absolute`, `names` | Choose how formula terms map to the `x` range and `varNames`. |
| `alpha` | For example `0.05` | Controls two-sided confidence intervals returned by extractor functions. |
| `maxIter` | Positive integer | Leave blank unless diagnosing convergence. Increase only after checking the model and data. |
| `trace` | `TRUE`, `FALSE` | Use `TRUE` when you need optimizer trace output for troubleshooting. |
| `covOptimizerMode` | blank/default, `AI`, `AverageInformation`, `FisherScoring`, `SAS`, `BFGS`, `ProjectedBFGS`, `BFGS_ANALYTIC`, `BFGS_NUMERICAL` | Leave blank for routine analyses. Specify a mode only for reproducibility, diagnostics, or software-comparison work. |
| `covGradientMode` | blank/default, `Auto`, `Analytic`, `AnalyticValidation`, `Validate`, `Numerical`, `FiniteDifference` | Leave blank or use `Auto` for routine analyses. Use validation/numerical modes only for diagnostics. |

!!! warning "Kenward-Roger and ML"
    Kenward-Roger inference is REML-based. For worksheet formulas, choose `"REML"` when using `"KR"`. If you need an ML fit, use a non-Kenward-Roger inference method.

---

## 8. Standard extractor functions

### Complete or selected output: `MMRM_RESULTS`

Syntax:

```excel
=BESH.REGR.MMRM_RESULTS(handle, table, includeOptimizerTrace, alpha)
```

Use this when you want the full stacked report or one named table. Leaving `table` blank returns all available result tables. Supplying a table title returns only that table.

Common table names include:

- `Fixed effects`
- `Kenward-Roger term-level F tests`
- `Covariance parameters`
- `Estimated R covariance matrix`
- `Estimated R correlation matrix`
- `Fit statistics`
- `Convergence`

If the model was fitted with `trace=TRUE`, set `includeOptimizerTrace=TRUE` to include optimizer trace information in the stacked result.

### Fixed effects: `MMRM_COEF`

Syntax:

```excel
=BESH.REGR.MMRM_COEF(handle, alpha)
```

Use this to inspect the fitted coefficient estimates, standard errors, degrees of freedom when applicable, test statistics, p-values, and confidence intervals. Coefficients are useful for checking the model parameterization, but treatment effects are usually better reported from LS-means or contrasts.

### Term-level tests: `MMRM_TYPE3`

Syntax:

```excel
=BESH.REGR.MMRM_TYPE3(handle, alpha)
```

Use this to review term-level fixed-effect tests. For Kenward-Roger fits, this table reports Kenward-Roger term-level F tests. The table is useful for checking whether terms such as treatment, visit, or treatment-by-visit interaction contribute to the model, but it does not replace planned LS-mean contrasts.

### Covariance parameters: `MMRM_COVPARMS`

Syntax:

```excel
=BESH.REGR.MMRM_COVPARMS(handle)
```

Use this to review the estimated parameters of the selected within-subject covariance structure. The labels depend on the structure. For example, an unstructured covariance reports visit variances and covariances, while simpler structures have fewer parameters.

### Covariance and correlation matrices: `MMRM_R_COV`, `MMRM_R_CORR`

Syntax:

```excel
=BESH.REGR.MMRM_R_COV(handle)
=BESH.REGR.MMRM_R_CORR(handle)
```

Use the covariance matrix to see the fitted residual variance/covariance pattern on the response scale. Use the correlation matrix to understand the fitted within-subject association between visits.

### Fit statistics: `MMRM_FITSTATS`

Syntax:

```excel
=BESH.REGR.MMRM_FITSTATS(handle)
```

Use this to check likelihood, information criteria, number of observations, number of subjects, number of covariance parameters, and convergence summaries. Compare covariance structures using the same response, fixed-effect model, retained rows, and likelihood method.

---

## 9. LS-means and built-in contrasts

### Visit and visit-by-group LS-means: `MMRM_LSMEANS`

Syntax:

```excel
=BESH.REGR.MMRM_LSMEANS(handle, group, alpha)
```

When `group` is blank, the function returns one observed-design-grid LS-mean for each visit/time value. When `group` names a fitted design column, the function returns LS-means by visit and group.

Examples:

```excel
=BESH.REGR.MMRM_LSMEANS(Q1)
=BESH.REGR.MMRM_LSMEANS(Q1,"ARMCDN[2]")
```

The `group` argument must refer to a fitted design column saved in the handle, not necessarily the raw source column name. When a factor is expanded by the formula service, a two-level variable may appear as an indicator such as `ARMCDN[2]`.

### Built-in group contrasts: `MMRM_CONTRASTS`

Syntax:

```excel
=BESH.REGR.MMRM_CONTRASTS(handle, group, contrastMode, controlLevel, comparisonLevel, direction, alpha)
```

Use this function for common treatment-difference outputs within each visit. It compares levels of one numeric fitted design column.

Common examples:

```excel
=BESH.REGR.MMRM_CONTRASTS(Q1,"ARMCDN[2]")
=BESH.REGR.MMRM_CONTRASTS(Q1,"ARMCDN[2]","Each group vs control",0)
=BESH.REGR.MMRM_CONTRASTS(Q1,"ARMCDN[2]","Selected comparison only",0,1,"Treatment - control")
```

| Argument | Meaning |
|---|---|
| `group` | Fitted design column to compare. |
| `contrastMode` | `Pairwise among group levels`, `Each group vs control`, or `Selected comparison only`. |
| `controlLevel` | Numeric control/reference level. If omitted where needed, the lowest observed group level is used. |
| `comparisonLevel` | Numeric comparison level for selected comparisons. |
| `direction` | Controls the sign of the difference. |
| `alpha` | Optional confidence-interval alpha. |

For nonstandard estimands, do not force them into `MMRM_CONTRASTS`. Use `BESH.REGR.MMRM_LSMESTIMATE` instead.

---

## 10. Custom LS-mean estimates with `MMRM_LSMESTIMATE`

Syntax:

```excel
=BESH.REGR.MMRM_LSMESTIMATE(handle, spec, alpha, at)
```

`BESH.REGR.MMRM_LSMESTIMATE` evaluates user-defined linear estimates from the fitted MMRM handle. It is the worksheet function to use when the desired contrast is not one of the standard visit-by-group differences. Examples include subgroup-specific treatment differences, average treatment effects over selected visits, custom change-from-baseline contrasts, or contrasts involving several fitted design columns at once.

### Specification range

The `spec` range must contain a header row and one or more data rows.

| Column type | Required? | Description |
|---|---|---|
| `label` | Optional | Name of the custom estimate. Rows with the same label are combined into one estimate. Aliases include `contrast`, `estimate`, and `name`. |
| `weight` | Required | Coefficient for the profile row. Aliases include `coef`, `coefficient`, and `contrastweight`. |
| `visit` | Optional | Visit/time value to match. Alias: `time`. |
| Fitted design column names | Optional | Profile restrictions such as `ARMCDN[2]`, `SEXN[1]`, or `VISITN[4]`. |

Rows with the same label are combined as:

\[
L_{custom}=\sum_j w_j L_j,
\qquad
\widehat\eta_{custom}=L_{custom}\hat\beta.
\]

Here, each \(L_j\) is formed from the observed fitted design rows that match the profile values supplied in that row.

### Example from the supplied UDF workbook

The workbook demonstrates a custom comparison of placebo females against treatment males at each visit. The specification range is `Q8:W16`:

| label | weight | `ARMCDN[2]` | `SEXN[1]` | `VISITN[2]` | `VISITN[3]` | `VISITN[4]` |
|---|---:|---:|---:|---:|---:|---:|
| PBO Female vs TRT Males At Vis1 | 1 | 0 | 1 | 0 | 0 | 0 |
| PBO Female vs TRT Males At Vis1 | -1 | 1 | 0 | 0 | 0 | 0 |
| PBO Female vs TRT Males At Vis2 | 1 | 0 | 1 | 1 | 0 | 0 |
| PBO Female vs TRT Males At Vis2 | -1 | 1 | 0 | 1 | 0 | 0 |
| PBO Female vs TRT Males At Vis3 | 1 | 0 | 1 | 0 | 1 | 0 |
| PBO Female vs TRT Males At Vis3 | -1 | 1 | 0 | 0 | 1 | 0 |
| PBO Female vs TRT Males At Vis4 | 1 | 0 | 1 | 0 | 0 | 1 |
| PBO Female vs TRT Males At Vis4 | -1 | 1 | 0 | 0 | 0 | 1 |

The formula is:

```excel
=BESH.REGR.MMRM_LSMESTIMATE(Q1,Q8:W16)
```

The first row for each label has weight `1` and defines the placebo-female profile. The second row has weight `-1` and defines the treatment-male profile. Rows with the same label are added together, giving placebo female minus treatment male for the relevant visit.

The returned table includes the estimate, standard error, denominator degrees of freedom when available, test statistic, p-value, and confidence interval. In the supplied workbook, inference for these custom contrasts uses the Kenward-Roger settings saved in the fit handle.

### Optional `at` range

The optional `at` argument supplies common profile settings that apply to every row in `spec` unless overridden by a value in `spec`.

Two-column form:

```text
name       value
visit      4
SEXN[1]    1
```

Wide one-row form:

```text
visit   SEXN[1]
4       1
```

Use `at` when many rows share the same visit, subgroup, or covariate value. Values in `spec` take precedence over values in `at`.

!!! warning "Observed-design-grid matching"
    `BESH.REGR.MMRM_LSMESTIMATE` uses observed fitted design rows stored in the handle. It does not synthesize profiles that were absent from the retained analysis data. If no retained row matches a requested profile, the function returns a descriptive error.

---

## 11. Fitted values and residuals

### `MMRM_FITTED`

Syntax:

```excel
=BESH.REGR.MMRM_FITTED(handle, includeHeader)
```

Returns row numbers and marginal fitted values for the retained analysis rows.

### `MMRM_RESID`

Syntax:

```excel
=BESH.REGR.MMRM_RESID(handle, includeHeader)
```

Returns row numbers, fitted values, and raw marginal residuals. The residual is:

\[
\text{residual}=y-\widehat{y}.
\]

Use residual output for data review, plotting, and diagnostics. For formal model checking, inspect residuals together with subject, visit, treatment, and observed response values.

---

## 12. Cached handles and recalculation

`BESH.REGR.MMRM_FIT` returns a handle stored in the current Excel session. Extractor functions use this handle so the model does not have to be refitted every time you request another table.

Important points:

- Handles are temporary session objects.
- Closing Excel clears the session cache.
- Recalculating a fit formula can create a new handle.
- Extractor formulas must point to the current handle cell.
- If a handle is no longer present, extractors return an error such as `#N/A` or a descriptive message.

Use:

```excel
=BESH.REGR.MMRM_DROP(Q1)
```

to remove one handle, or:

```excel
=BESH.REGR.MMRM_CLEAR_ALL()
```

to remove all MMRM handles from the current session.

!!! tip "Memory management"
    Most workbooks do not need explicit cache management. Use `MMRM_DROP` or `MMRM_CLEAR_ALL` after large exploratory sessions or after fitting many alternative models.

---

## 13. Recommended workbook layout

A reproducible MMRM workbook is easier to audit when the data, model definition, and outputs are separated.

| Sheet or area | Suggested contents |
|---|---|
| `Data` | Raw long-format analysis data. Keep original columns and avoid manual overwriting. |
| `Model` | `MMRM_FIT` formula, model formula text, fit method, covariance structure, inference method, and any notes. |
| `Output` | Standard extractor formulas: coefficients, type III tests, covariance parameters, fit statistics, LS-means, and contrasts. |
| `Custom contrasts` | `LSMESTIMATE` specification ranges and returned custom contrast tables. |
| `Diagnostics` | Fitted values, residuals, trace output, and any plots created from the extracted arrays. |
| `Readme` | Short description of the dataset, model, software version, and refresh instructions. |

For regulated or highly reviewed work, keep the model settings in visible cells and refer to those cells from the formula where practical. This makes the workbook easier to review than a long formula with hidden assumptions.

---

## 14. Troubleshooting worksheet formulas

| Symptom | Likely cause | What to check |
|---|---|---|
| Formula returns a text error instead of a handle | Invalid input range, nonnumeric predictor, invalid formula, or impossible model. | Confirm that `y`, `x`, `subject`, `visit`, and `varNames` have matching rows/columns and that all required predictor values are numeric. |
| Extractor returns `#N/A` or cannot find the handle | The handle was cleared, Excel was restarted, or the fit cell was recalculated differently. | Recalculate the `MMRM_FIT` formula and ensure extractors point to the current handle cell. |
| Dynamic array does not spill | Output area is blocked. | Clear cells below and to the right of the formula. |
| Group LS-means return an error | The `group` name does not match a fitted design column. | Inspect the coefficient table or formula-expanded names; use names such as `ARMCDN[2]` when the formula expander creates indicator columns. |
| Custom `LSMESTIMATE` returns no matching rows | The profile specified in `spec` or `at` does not occur in the retained observed design grid. | Check the fitted design column names and values; remember that missing rows are excluded before matching. |
| Kenward-Roger fit is slow | KR requires additional covariance and derivative calculations. | Use KR for final inference; use between-within, Satterthwaite, residual DF, or Wald methods for exploratory speed checks when appropriate. |
| Results differ from the ribbon output | Different fit method, inference method, covariance structure, formula coding, missing-row handling, or LS-means definition. | Compare the exact model formula, retained analysis rows, covariance structure, inference method, alpha, and contrast direction. |

---

## 15. Practical checklist

Before relying on a worksheet MMRM result, check that:

1. the data are in long format;
2. the response range, predictor block, subject range, and visit range align row by row;
3. missing data handling leaves the intended analysis rows;
4. the formula expands to the intended fixed-effect design;
5. REML is used for Kenward-Roger inference;
6. the selected covariance structure is appropriate and converged;
7. LS-means and contrasts use the intended group column and direction;
8. custom `LSMESTIMATE` rows define the intended profiles and weights;
9. extractor formulas all refer to the same fit handle;
10. the workbook documents whether LS-means are observed-design-grid estimates.

---

## 16. Generated syntax reference

This page explains the method-oriented workflow. The automatically generated UDF reference contains the exact current signatures and function-wizard descriptions:

- [BESH.REGR.MMRM_FIT](../../udf/regression-models.md#beshregrmmrm_fit)
- [BESH.REGR.MMRM_RESULTS](../../udf/regression-models.md#beshregrmmrm_results)
- [BESH.REGR.MMRM_LSMEANS](../../udf/regression-models.md#beshregrmmrm_lsmeans)
- [BESH.REGR.MMRM_CONTRASTS](../../udf/regression-models.md#beshregrmmrm_contrasts)
- [BESH.REGR.MMRM_LSMESTIMATE](../../udf/regression-models.md#beshregrmmrm_lsmestimate)
