# Causal Inference UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._

## BESH.PS.BALANCE

Returns covariate balance diagnostics before and after the selected propensity-score adjustment.

**Function wizard:** Returns balance diagnostics before and after matching/weighting.

### Syntax

`=BESH.PS.BALANCE(handle, includeHeader)`

### Parameters

- **handle** — Text handle returned by `BESH.PS.FIT`.
- **includeHeader** — Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.

### Returns

A spill table with covariate-level balance statistics, including treatment and control summaries before and
after adjustment, standardized mean differences, absolute standardized mean differences, variance ratios,
empirical-distribution diagnostics, and threshold flags.

### Notes

Balance diagnostics are central to propensity-score analysis. A small treatment-effect p-value is not meaningful
if the adjustment fails to balance important pre-treatment covariates. Standardized mean differences are often
reviewed against thresholds such as 0.1 or 0.2, while variance ratios and empirical-distribution differences
provide additional checks of covariate comparability.

## BESH.PS.CLEANUP

Removes fitted propensity-score analysis handles from the current Excel session.

**Function wizard:** Removes one PSM handle from the session cache, or all PSM handles when handle is blank or ALL.

### Syntax

`=BESH.PS.CLEANUP(handle)`

### Parameters

- **handle** — Optional handle returned by `BESH.PS.FIT`. Provide a specific handle to remove one stored result. Leave
blank or pass `ALL` to remove all stored propensity-score results from the current session.

### Returns

When clearing all results, returns the number of removed handles. When clearing one result, returns TRUE if
the handle was found and removed, and FALSE otherwise.

### Notes

Handles are stored in memory for the current Excel session so that output functions can reuse a fitted result.
Cleanup is optional but useful in long sessions, large workbooks, or automated examples that fit many analyses.
Closing Excel also clears the stored results.

## BESH.PS.FIT

Fits a propensity-score analysis and returns a reusable handle for subsequent table functions.

**Function wizard:** Fits a propensity-score analysis and returns a reusable handle.

### Syntax

`=BESH.PS.FIT(id, treatment, outcome, covariates, varNames, method, estimand, scoreMethod, existingScore, exactGroups, formula, matchingOptions, diagnosticOptions, alpha, formulaAddressing)`

### Parameters

- **id** — Optional ID column aligned with the treatment, outcome, and covariate rows. If omitted or blank, worksheet
source row numbers are used as row identifiers in output tables. IDs are used only for reporting and matching
audit tables; they do not affect the statistical adjustment.
- **treatment** — Required treatment indicator column. Values must identify two groups, normally coded as `0` for control
and `1` for treated. Rows with invalid, missing, or non-finite treatment values cannot be analyzed.
- **outcome** — Required outcome column aligned with `treatment`. The treatment-effect tables compare
the treated and control outcomes after the selected propensity-score adjustment. The current worksheet
functions are primarily intended for numeric continuous outcomes.
- **covariates** — Required raw covariate matrix with one row per subject and one or more columns of pre-treatment covariates.
These variables are used to estimate propensity scores, to calculate balance diagnostics, and, when requested,
to define matching or coarsening distances. Covariates should be measured before treatment assignment.
- **varNames** — Optional covariate names supplied as a one-row range, one-column range, or comma-separated text. Names are
displayed in output tables and can be referenced by the formula argument when formula addressing is set to
`names`. If omitted, generic covariate names are created from the input columns.
- **method** — Adjustment method. Accepted values include `matching` or `nearest` for nearest-neighbor matching,
`optimal` for optimal pair matching, `weighting` or `iptw` for propensity-score weighting,
`subclassification` or `subclass` for propensity-score strata, and `cem` for coarsened exact matching.
The default is nearest-neighbor matching.
- **estimand** — Target estimand. Accepted values are `ATT` for the average treatment effect among treated subjects,
`ATC` for the average effect among controls, `ATE` for the full-sample average effect, and `ATO`
for the overlap-population effect. Not every method supports every estimand; unsupported combinations return
an error instead of silently changing the requested analysis. The default is `ATT`.
- **scoreMethod** — Propensity-score source. Use `logit`, `logistic`, or `glm` to estimate scores from the treatment
indicator and covariates. Use `supplied` or `existing` when propensity scores are already available
in `existingScore`. The default is estimated logistic propensity scores.
- **existingScore** — Optional supplied propensity-score column. This argument is required when `scoreMethod`
is `supplied` or `existing`. Values must be finite probabilities strictly between 0 and 1. Supplied
scores are used directly for matching, weighting, subclassification, overlap diagnostics, and Love-plot data.
- **exactGroups** — Optional exact-matching or grouping matrix aligned with the analysis rows. When used with matching methods,
treated and control subjects are matched only within the same exact-group combination. With coarsened exact
matching, these columns can be used as additional exact grouping dimensions. Multiple columns are combined
row by row into a joint group label.
- **formula** — Optional right-hand-side formula that selects and expands covariates for the propensity model. If omitted,
all raw covariate columns are used as main effects. The formula can reference supplied variable names,
relative column letters, or absolute worksheet column letters depending on `formulaAddressing`.
- **matchingOptions** — Optional semicolon-separated option string controlling the adjustment method. Common keys include
`ratio`, `replacement`, `caliper`, `caliperScale`, `distance`, `order`,
`seed`, `support`, `trimLower`, `trimUpper`, `subclasses`, `cemBins`,
`normalizeWeights`, `stabilizedWeights`, `ridge`, `maxIter`, and `tol`. For example:
`ratio=2; replacement=false; caliper=0.2; caliperScale=sd_logit; distance=mahalanobis_with_ps_caliper`.
- **diagnosticOptions** — Optional semicolon-separated option string controlling diagnostics and reporting thresholds. Common keys include
`smd` for the standardized-mean-difference threshold, `vrLower` and `vrUpper` for variance-ratio
thresholds, `overlapBins` for propensity-score histogram bins, and `lovePlot` to request Love-plot
data preparation.
- **alpha** — Two-sided significance level used for confidence intervals and hypothesis-test summaries where supported.
The default is `0.05`. This setting affects reporting only; it does not change the matched sets, weights,
propensity scores, or balance diagnostics.
- **formulaAddressing** — Formula-addressing mode. Use `relative` to refer to covariate columns as A, B, C relative to the supplied
covariate range, `absolute` to refer to worksheet column letters, or `names` to refer to values supplied
in `varNames`. The default is `relative`.

### Returns

A text handle identifying the fitted propensity-score analysis in the current Excel session. Pass this handle
to the companion functions to retrieve output tables without refitting. If the function cannot fit the analysis,
it returns an Excel error or an explanatory error message.

### Notes

This function aligns the supplied columns, removes rows that cannot be analyzed, estimates or validates
propensity scores, applies the requested adjustment method, computes balance diagnostics, and stores the
complete result for later retrieval. The returned handle is session-local and is not saved permanently in the
workbook. Recalculate `BESH.PS.FIT` to create a fresh result after changing the input data or options.

Typical workflow: first call `BESH.PS.FIT`; then use `BESH.PS.SUMMARY`, `BESH.PS.SCORES`,
`BESH.PSM.MATCHES`, `BESH.PS.BALANCE`, `BESH.PS.EFFECT`, and related table functions with
the returned handle. Use `BESH.PS.CLEANUP` to remove stored results that are no longer needed.

Example: `=BESH.PS.FIT(A2:A101,B2:B101,C2:C101,D2:H101,D1:H1,"matching","ATT","logit",,,"age + sex + baseline", "ratio=1; replacement=false; caliper=0.2; caliperScale=sd_logit", "smd=0.1; lovePlot=true")`.

## BESH.PS.LOVEPLOT_DATA

Returns chart-ready data for a Love plot of covariate imbalance before and after adjustment.

**Function wizard:** Returns chart-ready Love plot data for a fitted propensity-score handle.

### Syntax

`=BESH.PS.LOVEPLOT_DATA(handle, includeHeader)`

### Parameters

- **handle** — Text handle returned by `BESH.PS.FIT`.
- **includeHeader** — Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.

### Returns

A spill table containing covariate names, absolute standardized mean differences before adjustment, absolute
standardized mean differences after matching or weighting, threshold values, variable grouping information,
and display-order fields suitable for creating an Excel scatter or dot plot.

### Notes

A Love plot visualizes whether covariate imbalance has been reduced by the adjustment. Values farther to the
right indicate larger imbalance. The threshold column can be used to add a reference line, commonly at an
absolute standardized mean difference of 0.1.

## BESH.PS.SUMMARY

Returns a stacked summary report for a fitted propensity-score analysis.

**Function wizard:** Returns stacked summary tables for a fitted propensity-score handle.

### Syntax

`=BESH.PS.SUMMARY(handle)`

### Parameters

- **handle** — Text handle returned by `BESH.PS.FIT`. The handle identifies the fitted analysis to summarize.

### Returns

A spill table containing run settings, imported-data information, sample-size summaries, propensity-score
model information when available, treatment-effect summaries, and warnings. The exact sections depend on
the selected adjustment method and available diagnostics.

### Notes

Use this function as the first review table after fitting. It echoes key options such as adjustment method,
estimand, score source, matching ratio, replacement choice, caliper settings, common-support handling,
diagnostic thresholds, and any rows dropped during data preparation.

The summary is intended for auditability and interpretation. It does not show every row-level score or
matched pair; use the dedicated score, match, balance, and effect functions for those details.

## BESH.PSM.MATCHES

Returns the matched-pair or matched-set table for matching analyses.

**Function wizard:** Returns the matched-pair/set table for a fitted propensity-score handle.

### Syntax

`=BESH.PSM.MATCHES(handle, includeHeader)`

### Parameters

- **handle** — Text handle returned by `BESH.PS.FIT`. The referenced fit should use nearest-neighbor or optimal pair
matching.
- **includeHeader** — Optional TRUE/FALSE value indicating whether to include the header row. The default is TRUE.

### Returns

A spill table describing matched treated-control links, including set identifiers, row identifiers, propensity
scores, matching distance, exact group labels where present, outcome values, and reuse information when
matching with replacement is used. If the selected method does not produce matched pairs, the returned table
indicates that no matched-pair output is available.

### Notes

Use this function to inspect the actual matches behind the effect estimate. For 1:k nearest-neighbor matching,
a treated subject can appear in a matched set with multiple controls for ATT analyses, or a control subject can
appear with multiple treated subjects for ATC analyses. Exact-group and caliper restrictions are reflected in
the returned matched links.
