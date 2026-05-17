# LMM worksheet functions

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** show how to run **Linear Mixed Models (LMM)** from worksheet formulas instead of the ribbon dialog. Worksheet functions are useful when you need a reproducible workbook, want to refresh the same model after changing the data, or need to extract selected model tables into reporting sheets.

For the exact generated function signatures, see the [Regression Models UDF reference](../../udf/regression-models.md#beshregrlmm_fit). For model notation and covariance-structure definitions, see [Model and mathematics](model-and-mathematics.md) and [Random effects and covariance structures](random-effects-and-covariance.md).

---

## 1. When to use worksheet functions

The ribbon dialog is the easiest way to build an LMM interactively. Worksheet functions are better when the analysis should be visible, repeatable, and easy to refresh.

Use the `BESH.REGR.LMM_*` functions when you want to:

- keep the complete model definition in worksheet cells;
- rerun the same model after replacing or extending the data;
- compare several random-effect or residual covariance structures side by side;
- return only selected tables such as fixed effects, covariance parameters, BLUPs, or fit statistics;
- create an auditable workbook template for a recurring analysis;
- reproduce a ribbon analysis without relying on dialog state.

!!! tip "Recommended workflow"
    Use the ribbon first when learning a dataset or deciding which terms to include. Use worksheet functions once the model definition is stable and should be documented in the workbook.

!!! note "Do not type the internal `_xll.` prefix"
    Some workbooks may display Excel-DNA formulas internally as `_xll.BESH.REGR...`. Users should enter the public worksheet function names shown in this documentation, for example `=BESH.REGR.LMM_FIT(...)`.

---

## 2. Function family overview

The LMM worksheet workflow has one fitting function, several extractor functions, and two cache-management functions.

| Function | Main use |
|---|---|
| [`BESH.REGR.LMM_FIT`](../../udf/regression-models.md#beshregrlmm_fit) | Fit the LMM and return a reusable session handle. |
| [`BESH.REGR.LMM_RESULTS`](../../udf/regression-models.md#beshregrlmm_results) | Return all result tables or one selected table. |
| [`BESH.REGR.LMM_COEF`](../../udf/regression-models.md#beshregrlmm_coef) | Return the fixed-effect coefficient table. |
| [`BESH.REGR.LMM_TYPE3`](../../udf/regression-models.md#beshregrlmm_type3) | Return term-level fixed-effect F tests when available. |
| [`BESH.REGR.LMM_COVPARMS`](../../udf/regression-models.md#beshregrlmm_covparms) | Return optimized covariance-parameter estimates. |
| [`BESH.REGR.LMM_G_COV`](../../udf/regression-models.md#beshregrlmm_g_cov) | Return the fitted random-effect covariance matrix. |
| [`BESH.REGR.LMM_G_CORR`](../../udf/regression-models.md#beshregrlmm_g_corr) | Return the fitted random-effect correlation matrix. |
| [`BESH.REGR.LMM_R_COV`](../../udf/regression-models.md#beshregrlmm_r_cov) | Return the fitted residual covariance matrix. |
| [`BESH.REGR.LMM_R_CORR`](../../udf/regression-models.md#beshregrlmm_r_corr) | Return the fitted residual correlation matrix. |
| [`BESH.REGR.LMM_RANEF`](../../udf/regression-models.md#beshregrlmm_ranef) | Return subject- or cluster-specific random-effect predictions. |
| [`BESH.REGR.LMM_FITSTATS`](../../udf/regression-models.md#beshregrlmm_fitstats) | Return likelihood, information-criterion, model-size, and convergence statistics. |
| [`BESH.REGR.LMM_FITTED`](../../udf/regression-models.md#beshregrlmm_fitted) | Return row-level marginal fitted values. |
| [`BESH.REGR.LMM_RESID`](../../udf/regression-models.md#beshregrlmm_resid) | Return row-level marginal fitted values and raw marginal residuals. |
| [`BESH.REGR.LMM_DROP`](../../udf/regression-models.md#beshregrlmm_drop) | Remove one fitted handle from the session cache. |
| [`BESH.REGR.LMM_CLEAR_ALL`](../../udf/regression-models.md#beshregrlmm_clear_all) | Remove all fitted LMM handles from the session cache. |

---

## 3. Basic worksheet pattern

The worksheet functions use a **fit once, extract many times** pattern.

### Step 1: fit the model and save the handle

Enter `BESH.REGR.LMM_FIT` in a cell that will hold the model handle. The handle is an opaque text value such as `LMM:...`; do not edit it manually.

Generic syntax:

```excel
=BESH.REGR.LMM_FIT(y, x, subject, z, visit, xVarNames, zVarNames, residualCovariance, randomCovariance, fitMethod, inference, includeFixedIntercept, includeRandomIntercept, fixedFormula, randomFormula, formulaAddressing, alpha, maxIter, trace, covOptimizerMode, covGradientMode)
```

The first three arguments are required for ordinary use:

| Argument | Meaning |
|---|---|
| `y` | Continuous response column. |
| `x` | Numeric fixed-effect predictor block or raw columns used by `fixedFormula`. |
| `subject` | Subject, cluster, batch, school, site, or other grouping identifier. |

The `z` argument is optional. Leave it blank for a random-intercept-only model. Supply it when the model needs random slopes, multiple random effects, or random-effect interactions.

### Step 2: extract result tables

Assume the handle is in cell `J1`. Common extractor formulas are:

```excel
=BESH.REGR.LMM_RESULTS(J1)
=BESH.REGR.LMM_COEF(J1)
=BESH.REGR.LMM_TYPE3(J1)
=BESH.REGR.LMM_COVPARMS(J1)
=BESH.REGR.LMM_G_COV(J1)
=BESH.REGR.LMM_G_CORR(J1)
=BESH.REGR.LMM_RANEF(J1)
=BESH.REGR.LMM_FITSTATS(J1)
```

Dynamic-array versions of Excel will spill the output table automatically. In older Excel versions, select a sufficiently large output range before entering the formula as an array formula.

### Step 3: remove cached handles when done

Fitted models are cached for the current Excel session. To remove one handle:

```excel
=BESH.REGR.LMM_DROP(J1)
```

To remove all cached LMM handles:

```excel
=BESH.REGR.LMM_CLEAR_ALL()
```

Recalculate the `BESH.REGR.LMM_FIT` formula to recreate a dropped handle.

---

## 4. Data layout and input requirements

The worksheet input should normally be in **long format**: one row per observation, repeated visit, measurement, or clustered unit record.

A typical longitudinal layout is:

| Column | Example variable | Role |
|---|---|---|
| `A` | `Subject` | Subject or cluster identifier. May be text or numeric. |
| `B` | `Visit` | Optional numeric visit/time/order variable. |
| `C` | `Reaction` | Continuous response. |
| `D` | `Days_c` | Fixed-effect predictor and possible random slope. |
| `E` | `Treatment` | Numeric-coded treatment or class variable used with formula expansion. |
| `F` | `Difficulty` | Continuous predictor and possible random slope. |
| `G` | `Age_c` | Subject-level covariate. |

Important requirements:

| Requirement | Practical check |
|---|---|
| Continuous response | The `y` range must contain numeric outcome values. |
| Subject or cluster identifier | The `subject` range identifies rows from the same grouping unit. It may contain text or numeric labels. |
| Fixed-effect predictors | The `x` range must contain numeric raw variables or already-coded design columns. |
| Random-effect predictors | The optional `z` range must contain numeric raw variables or already-coded random-effect design columns. |
| Optional visit/order variable | The `visit` range should be numeric when using visit-indexed residual covariance structures. |
| Same row order | `y`, `x`, `subject`, `z`, and `visit` must refer to the same worksheet rows whenever they are supplied. |
| Header names | `xVarNames` and `zVarNames` should match the supplied predictor columns when formulas or readable output are desired. |

Rows with missing or invalid values in required inputs are removed before fitting. The row-level fitted and residual extractors return rows corresponding to the valid rows retained by the fit.

!!! note "Text subject identifiers are supported"
    The subject or cluster ID may be text or numeric. Text identifiers such as `S001`, `USUBJID-004`, `Site A`, or `Batch_7` are valid grouping labels. Response values and predictor columns used in `x`, `z`, and formulas must be numeric after formula expansion.

---

## 5. Main `LMM_FIT` options

The most important `BESH.REGR.LMM_FIT` options are summarized below. Blank optional arguments use documented defaults.

| Argument | Accepted/common values | Practical guidance |
|---|---|---|
| `residualCovariance` | `ID`, `Diagonal`, `CS`, `HCS`, `AR(1)`, `HAR(1)`, `TOEP`, `TOEPH`, `UN` | Start with `ID` for many LMMs. Add structured residual covariance only when residual dependence remains after the random effects. |
| `randomCovariance` | `RI`, `RI+S`, `ID`, `VC`, `CS`, `CSH`, `AR1`, `ARH1`, `TOEP`, `TOEPH`, `UN` | Use `RI` for a random intercept, `RI+S` for one random slope with random intercept, `VC` for multiple independent random effects, and `UN` only when covariances among random effects are important and estimable. |
| `fitMethod` | `REML`, `ML` | Use `REML` for routine variance-component estimation and fixed-effect inference. Use `ML` mainly for comparing different fixed-effect mean models. |
| `inference` | `KR`, `Satterthwaite`, `BetweenWithin`, `ResidualDF`, `Wald` | `KR` or `Satterthwaite` are usually preferred for small or moderate samples. `Wald` is faster and more asymptotic. |
| `includeFixedIntercept` | `TRUE`, `FALSE` | Usually leave blank or use `TRUE`. Set `FALSE` when the fixed-effect design already contains all intended columns. |
| `includeRandomIntercept` | `TRUE`, `FALSE` | Use `TRUE` for random-intercept and random-intercept-plus-slope models. Use `FALSE` for random-slope-only models. |
| `fixedFormula` | Right-hand-side formula | Expands the raw `x` block into fixed-effect terms such as factors and interactions. |
| `randomFormula` | Right-hand-side formula | Expands the raw `z` block into random-effect slopes, random-effect factors, and random-effect interactions. |
| `formulaAddressing` | `relative`, `absolute`, `names` | Choose how formula terms map to the supplied predictor ranges. |
| `alpha` | For example `0.05` | Controls two-sided confidence intervals returned by extractor functions. |
| `maxIter` | Positive integer | Leave blank unless diagnosing convergence. |
| `trace` | `TRUE`, `FALSE` | Use `TRUE` when optimizer trace output is needed for troubleshooting. |
| `covOptimizerMode` | blank/default, `AI`, `AverageInformation`, `FisherScoring`, `SAS`, `BFGS`, `BFGS_ANALYTIC`, `BFGS_NUMERICAL` | Leave blank for routine analyses. Specify a mode only for diagnostics or reproducibility. |
| `covGradientMode` | blank/default, `Auto`, `Analytic`, `AnalyticValidation`, `Validate`, `Numerical`, `FiniteDifference` | Leave blank or use `Auto` for routine analyses. Use validation/numerical modes only for diagnostics. |

!!! warning "Kenward-Roger and ML"
    Kenward-Roger inference is REML-based. For worksheet formulas, choose `"REML"` when using `"KR"`. If you need an ML fit, choose a non-Kenward-Roger inference method such as `"Satterthwaite"`, `"ResidualDF"`, or `"Wald"`.

---

## 6. Random-intercept example

Suppose the worksheet contains the data layout from Section 4, with rows `2:721` holding data and row `1` holding variable names. A random-intercept model with identity residual covariance can be fitted as:

```excel
=BESH.REGR.LMM_FIT(C2:C721,D2:G721,A2:A721,,,D1:G1,,"ID","RI","REML","KR")
```

This formula means:

| Argument | Example value | Meaning |
|---|---|---|
| `y` | `C2:C721` | Response variable `Reaction`. |
| `x` | `D2:G721` | Fixed-effect predictor block. |
| `subject` | `A2:A721` | Subject or cluster ID. |
| `z` | blank | No supplied random-slope columns. |
| `visit` | blank | No visit-indexed residual covariance. |
| `xVarNames` | `D1:G1` | Names for fixed-effect predictors. |
| `residualCovariance` | `"ID"` | Independent residuals after the random intercept. |
| `randomCovariance` | `"RI"` | Random-intercept G-side model. |
| `fitMethod` | `"REML"` | Restricted maximum likelihood. |
| `inference` | `"KR"` | Kenward-Roger fixed-effect inference. |

The result is a model handle. Use the handle with extractor functions, for example:

```excel
=BESH.REGR.LMM_COEF(J1)
=BESH.REGR.LMM_G_COV(J1)
=BESH.REGR.LMM_RANEF(J1)
```

---

## 7. Random intercept plus random slope example

To add a random slope for `Days_c`, supply that column as `z` and choose a compatible random-effect covariance structure.

```excel
=BESH.REGR.LMM_FIT(C2:C721,D2:G721,A2:A721,D2:D721,,D1:G1,D1,"ID","RI+S","REML","KR")
```

This model has:

- a fixed-effect design from `D2:G721`;
- a random intercept, because `includeRandomIntercept` is blank and therefore defaults to `TRUE`;
- a random slope for `Days_c`, supplied by `D2:D721`;
- a two-by-two random-effects covariance for intercept and slope.

Useful follow-up extractors are:

```excel
=BESH.REGR.LMM_G_COV(J1)
=BESH.REGR.LMM_G_CORR(J1)
=BESH.REGR.LMM_RANEF(J1)
=BESH.REGR.LMM_FITSTATS(J1)
```

The G-side covariance matrix shows the random-intercept variance, random-slope variance, and their covariance. The G-side correlation matrix is often easier to interpret because it expresses the intercept-slope relationship as a correlation.

---

## 8. Random-slope-only example

A random-slope-only model turns off the random intercept. Because the convenience `RI+S` structure assumes a random intercept plus one random slope, use a general structure such as `VC` for a random-slope-only model.

```excel
=BESH.REGR.LMM_FIT(C2:C721,D2:G721,A2:A721,D2:D721,,D1:G1,D1,"ID","VC","REML","KR",TRUE,FALSE)
```

Here `includeFixedIntercept` is `TRUE` and `includeRandomIntercept` is `FALSE`. The model still has a fixed intercept, but the random-effect design contains only the `Days_c` slope.

---

## 9. Multiple random effects and random interactions

For multiple random effects, supply a multi-column `z` range or use `randomFormula`. The variance-components structure is usually the safest starting point because it estimates one variance per random-effect column and fixes random-effect covariances to zero.

For example, suppose `D:F` contains `Days_c`, `Treatment`, and `Difficulty`. A model with fixed effects and random effects generated from formulas can be fitted as:

```excel
=BESH.REGR.LMM_FIT(C2:C721,D2:G721,A2:A721,D2:F721,,D1:G1,D1:F1,"ID","VC","REML","KR",TRUE,TRUE,"'Days_c'+'Treatment'+'Difficulty'+'Days_c':'Treatment'","'Days_c'+'Difficulty'+'Days_c':'Difficulty'","names")
```

This formula uses:

| Option | Role |
|---|---|
| `fixedFormula` | Builds the fixed-effect design from named columns in `x`. |
| `randomFormula` | Builds random slopes and a random interaction from named columns in `z`. |
| `formulaAddressing = "names"` | Tells the formula parser to use names from `xVarNames` and `zVarNames`. |
| `randomCovariance = "VC"` | Uses independent random-effect variances for the richer random-effect design. |

!!! important "Quote variable names in `names` mode"
    When using `formulaAddressing = "names"`, write variable names as single-quoted names, for example `'Days_c'`, `'Difficulty'`, and `'Days_c':'Difficulty'`. This avoids ambiguity with relative or absolute column-letter addressing.

After fitting, inspect:

```excel
=BESH.REGR.LMM_COVPARMS(J1)
=BESH.REGR.LMM_G_COV(J1)
=BESH.REGR.LMM_RANEF(J1)
=BESH.REGR.LMM_FITSTATS(J1)
```

If the model is stable and the random-effect covariances are scientifically important, you can compare `VC` with a richer G-side structure such as `CSH`, `TOEPH`, or `UN`. Richer structures require more information from the data and may be slower or harder to fit.

---

## 10. Optional R-side residual covariance example

Many LMMs use identity residual covariance because the random effects already capture within-subject dependence. Use an R-side residual covariance structure when residuals still show visit-specific variance or serial dependence after the random effects.

For example, a random-intercept-plus-slope model with AR(1) residual correlation can be fitted as:

```excel
=BESH.REGR.LMM_FIT(C2:C721,D2:G721,A2:A721,D2:D721,B2:B721,D1:G1,D1,"AR(1)","RI+S","REML","KR")
```

The `visit` argument is supplied as `B2:B721` because AR(1) residual covariance depends on within-subject ordering. Toeplitz and heterogeneous Toeplitz residual structures also use visit/order information.

!!! warning "Avoid over-modeling dependence"
    A rich G-side structure and a rich R-side structure can compete with each other. Start with a simple residual covariance, then add R-side structure only when there is a design reason or residual diagnostic reason to do so.

---

## 11. Formula syntax notes

The `fixedFormula` and `randomFormula` arguments use the same formula-expansion rules as the regression formula utilities.

Common patterns include:

| Pattern | Meaning |
|---|---|
| `A+B` | Add two main-effect columns in relative or absolute column-letter mode. |
| `A:B` | Add an interaction between two columns. |
| `factor(A)` | Treat a numeric-coded column as a categorical factor. |
| `poly(A,2)` | Add polynomial terms for a numeric column. |
| `'Days_c'+'Treatment'` | Add named variables when `formulaAddressing` is `"names"`. |
| `'Days_c':'Treatment'` | Add a named interaction when `formulaAddressing` is `"names"`. |

For more details, see [Regression formula syntax](../../udf/regression-formula-syntax.md).

Use the same addressing mode for `fixedFormula` and `randomFormula`. If you use `"names"`, make sure `xVarNames` and `zVarNames` are supplied and match the columns in `x` and `z`.

---

## 12. Extractor functions

### Complete or selected output: `LMM_RESULTS`

Syntax:

```excel
=BESH.REGR.LMM_RESULTS(handle, table, includeOptimizerTrace, alpha)
```

Leave `table` blank to return all available result tables stacked vertically. Provide a table title to return only one table. Common table titles include:

- `Fixed effects`
- `Kenward-Roger term-level F tests`
- `Covariance parameters`
- `Estimated G covariance matrix`
- `Estimated G correlation matrix`
- `Estimated R covariance matrix`
- `Estimated R correlation matrix`
- `BLUPs / random effects`
- `Fit statistics`
- `Convergence`

If trace output was requested when the model was fitted, set `includeOptimizerTrace` to `TRUE` to include the optimizer iteration history in the stacked output.

### Fixed-effect estimates: `LMM_COEF`

Syntax:

```excel
=BESH.REGR.LMM_COEF(handle, alpha)
```

Returns estimates, standard errors, test statistics, p-values, and confidence intervals for fixed-effect coefficients. Denominator degrees of freedom are included when the selected inference method provides them.

### Term-level tests: `LMM_TYPE3`

Syntax:

```excel
=BESH.REGR.LMM_TYPE3(handle, alpha)
```

Returns term-level fixed-effect F tests when the fitted model has enough formula information to group columns into terms. This table is most useful when the fixed-effect model was built from `fixedFormula`.

### Covariance parameters: `LMM_COVPARMS`

Syntax:

```excel
=BESH.REGR.LMM_COVPARMS(handle)
```

Returns optimized covariance-parameter estimates. These values are useful for diagnostics and reproducibility. For direct interpretation, also inspect the user-scale G-side and R-side covariance/correlation matrices.

### G-side covariance and correlation: `LMM_G_COV`, `LMM_G_CORR`

Syntax:

```excel
=BESH.REGR.LMM_G_COV(handle)
=BESH.REGR.LMM_G_CORR(handle)
```

Use these to inspect the fitted random-effect covariance and correlation matrices. The rows and columns correspond to the random-effect design columns used in the model, including the random intercept if included.

### R-side covariance and correlation: `LMM_R_COV`, `LMM_R_CORR`

Syntax:

```excel
=BESH.REGR.LMM_R_COV(handle)
=BESH.REGR.LMM_R_CORR(handle)
```

Use these to inspect the fitted residual covariance and correlation matrices. For identity residual covariance, these tables are simple. For visit-indexed structures, the matrix is aligned with the fitted visit/order levels.

### Random-effect predictions: `LMM_RANEF`

Syntax:

```excel
=BESH.REGR.LMM_RANEF(handle)
```

Returns empirical Bayes predictions of the subject- or cluster-specific random effects. These are conditional predictions for the fitted subjects or clusters, not fixed-effect coefficients.

### Fit statistics: `LMM_FITSTATS`

Syntax:

```excel
=BESH.REGR.LMM_FITSTATS(handle)
```

Returns likelihood, information-criterion, model-size, and convergence-related statistics. Use this table when comparing covariance structures or checking whether a richer model has become unstable.

### Fitted values and residuals: `LMM_FITTED`, `LMM_RESID`

Syntax:

```excel
=BESH.REGR.LMM_FITTED(handle, includeHeader)
=BESH.REGR.LMM_RESID(handle, includeHeader)
```

`LMM_FITTED` returns row-level marginal fitted values. `LMM_RESID` returns row number, fitted value, and raw marginal residual. The returned rows correspond to the valid rows retained by `LMM_FIT` after input screening.

---

## 13. Choosing covariance structures in worksheet formulas

The default LMM choices are intentionally conservative: REML fitting, identity residual covariance, and a safe G-side structure based on the random-effect design. You can override these choices explicitly.

### G-side random-effect covariance

| Value | Structure | Common use |
|---|---|---|
| `RI` | Random intercept | One random intercept and no random slopes. |
| `RI+S` | Random intercept plus one random slope | Random intercept and exactly one random slope with covariance estimated. |
| `ID` | Identity | Independent random effects with common variance. |
| `VC` | Variance components / diagonal | Multiple independent random effects with separate variances. |
| `CS` | Compound symmetry | Common variance and common correlation among random effects. |
| `CSH` | Heterogeneous compound symmetry | Separate variances and one common correlation. |
| `AR1` | Autoregressive | Ordered random-effect columns with common variance. |
| `ARH1` | Heterogeneous autoregressive | Ordered random-effect columns with separate variances. |
| `TOEP` | Toeplitz | Ordered random-effect columns with lag-specific correlations. |
| `TOEPH` | Heterogeneous Toeplitz | Ordered random-effect columns with separate variances and lag-specific correlations. |
| `UN` | Unstructured | Estimate every random-effect variance and covariance. |

For general random-effect authoring, `VC` is usually the safest first model. Use `UN` only when the number of subjects or clusters is large enough and the random-effect covariances are important.

### R-side residual covariance

| Value | Structure | Common use |
|---|---|---|
| `ID` | Identity | Default for many LMMs. |
| `Diagonal` | Heterogeneous diagonal | Visit-specific residual variances without residual correlation. |
| `CS` | Compound symmetry | Common residual correlation within subject or cluster. |
| `HCS` | Heterogeneous compound symmetry | Visit-specific residual variances with one common correlation. |
| `AR(1)` | Autoregressive | Serial residual correlation that decays with visit/order lag. |
| `HAR(1)` | Heterogeneous autoregressive | AR(1)-style residual correlation with visit-specific residual variances. |
| `TOEP` | Toeplitz | Lag-specific residual correlations with common residual variance. |
| `TOEPH` | Heterogeneous Toeplitz | Lag-specific residual correlations with visit-specific residual variances. |
| `UN` | Unstructured | Estimate every residual variance and covariance by visit/order level. |

When the R-side structure depends on visit or order, provide the `visit` argument. Keep the visit/order coding numeric and consistent within subjects.

---

## 14. Troubleshooting worksheet formulas

| Symptom or message | Likely cause | What to check |
|---|---|---|
| The fit function returns an error instead of a handle. | Invalid input ranges, unsupported option text, or failed model fit. | Check that all supplied ranges have the same number of rows after optional header trimming. |
| “No valid complete rows remain.” | Required inputs are missing or nonnumeric after screening. | Check response, predictor, random-effect, and visit columns for blanks or text values. |
| Formula parsing error for a named variable. | Variable names were not quoted in `names` mode or do not match `xVarNames`/`zVarNames`. | Use single quotes, for example `'Days_c'`, and verify the header ranges. |
| Random covariance compatibility error. | The selected G-side structure does not match the random-effect design. | Use `RI` only for a random-intercept-only model, `RI+S` only for random intercept plus one slope, and `VC` or `UN` for richer designs. |
| Visit/order error. | A visit-indexed residual covariance was selected without a usable `visit` range. | Supply a numeric visit/order column. |
| Kenward-Roger error with ML. | KR inference requires REML. | Use `"REML"` or choose a different inference method. |
| Output is very large. | `LMM_RESULTS` returns all tables or `LMM_RANEF` returns one row per subject and random-effect column. | Use specific extractor functions or selected table output. |
| Model is slow or unstable. | Covariance structure is too rich for the data. | Start with `ID` residual covariance and `RI`, `RI+S`, or `VC` random covariance, then add complexity gradually. |

---

## 15. Practical checklist

Before relying on an LMM worksheet formula, check that:

- the data are in long format;
- subject or cluster IDs are correctly aligned with the response and predictor rows;
- text subject IDs are in the `subject` argument, not in `x` or `z`;
- fixed-effect and random-effect predictors are numeric or are generated from numeric-coded raw columns;
- `xVarNames` and `zVarNames` match the supplied columns;
- formula variable names are single-quoted in `names` mode;
- random-effect covariance is compatible with the random-effect design;
- visit/order is supplied for visit-indexed R-side covariance structures;
- REML is used with Kenward-Roger inference;
- G-side and R-side covariance matrices are inspected after fitting;
- convergence and fit statistics are reviewed before interpreting p-values.

---

## Related pages

- [LMM overview](../linear-mixed-models-lmm.md)
- [Concepts and use cases](concepts-and-use-cases.md)
- [Model and mathematics](model-and-mathematics.md)
- [Random effects and covariance structures](random-effects-and-covariance.md)
- [Excel ribbon workflow](user-interface.md)
- [Implementation details](implementation-details.md)
- [Regression formula syntax](../../udf/regression-formula-syntax.md)
- [Regression Models UDF reference](../../udf/regression-models.md#beshregrlmm_fit)
