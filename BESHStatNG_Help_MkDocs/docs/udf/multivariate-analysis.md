# Multivariate Analysis UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Correspondence Analysis](../methods/correspondence-analysis.md)
- [Discriminant Analysis](../methods/discriminant-analysis.md)
- [Factor Analysis](../methods/factor-analysis.md)
- [Hierarchical Clustering](../methods/hierarchical-clustering.md)
- [K Means Clustering](../methods/k-means-clustering.md)
- [Multiple Correspondence Analysis](../methods/multiple-correspondence-analysis.md)
- [Principal Component Analysis](../methods/principal-component-analysis.md)

## BESH.MULTI.CA_COLUMNS

Returns column-category overview statistics for a fitted correspondence-analysis model.

**Function wizard:** Returns column-category overview statistics for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_COLUMNS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per column category showing quality of representation, mass, chi-square distance,
and inertia. This output is the column-side counterpart of `BESH.MULTI.CA_ROWS`.

## BESH.MULTI.CA_COL_CONTRIB

Returns column contributions for each axis of a fitted correspondence-analysis model.

**Function wizard:** Returns column contributions for each axis of a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_COL_CONTRIB(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of column contributions by available axis.
Contributions identify which column categories define each axis.

## BESH.MULTI.CA_COL_COORD

Returns the column principal-coordinate matrix for a fitted correspondence-analysis model.

**Function wizard:** Returns the column principal-coordinate matrix for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_COL_COORD(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix with one row per column category and one column per available axis.
These are the column points on the correspondence-analysis map.

## BESH.MULTI.CA_COL_COS2

Returns column cos² values for each axis of a fitted correspondence-analysis model.

**Function wizard:** Returns column cos² values for each axis of a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_COL_COS2(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of column cos² values (squared cosines) by available axis.

## BESH.MULTI.CA_DROP

Removes a correspondence-analysis handle from memory.

**Function wizard:** Removes a correspondence-analysis handle from memory.

### Syntax

`=BESH.MULTI.CA_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.CA_EIGEN

Returns the inertia (eigenvalue) table for a fitted correspondence-analysis model.

**Function wizard:** Returns inertia and explained-percentage summaries for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_EIGEN(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per axis showing the principal inertia, percentage inertia,
and cumulative percentage inertia. Large early axes indicate that most of the association structure
can be summarized in a low-dimensional map.

## BESH.MULTI.CA_FIT

Fits a simple correspondence-analysis model to a contingency table and returns a reusable handle.

**Function wizard:** Fits a simple correspondence-analysis model to a contingency table and returns a reusable handle.

### Syntax

`=BESH.MULTI.CA_FIT(table, rowNames, colNames)`

### Parameters

- **table** — Numeric contingency table of non-negative counts with row categories in rows and column categories in columns.
A single top header row containing column labels is detected automatically and skipped when present.
Embedded row-label columns are not supported in the supplied range; pass `rowNames` separately when you want row labels.
- **rowNames** — Optional row-category names as a comma-separated list or as a one-row or one-column range.
When omitted, default labels Row 1, Row 2, … are generated.
- **colNames** — Optional column-category names as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from a detected header row when available; otherwise default labels Col 1, Col 2, … are generated.

### Returns

A text handle for the fitted correspondence-analysis solution. Pass the handle to the other `CA_*` worksheet functions
to retrieve inertia summaries, row and column overview tables, coordinates, cos² tables, and contribution tables.

### Notes

Correspondence analysis decomposes the departure from independence in a contingency table into orthogonal latent axes.
It is especially useful when the Pearson chi-square test shows association but you also want to understand which row and column
categories drive that association and how categories are arranged in a low-dimensional map.

If `N` is the contingency table, the analysis is based on the matrix of standardized residuals
`S = D_r^(-1/2) (P - r cᵀ) D_c^(-1/2)`, where `P = N / n`, `r` and `c` are row and column masses,
and `D_r` and `D_c` are diagonal mass matrices. The singular values of `S` produce the principal inertias (eigenvalues),
while the left and right singular vectors produce row and column principal coordinates.

Axis signs are arbitrary. If the same table is compared with another software package, the coordinates can differ only by a sign reversal,
while distances, inertias, cos² values, and contributions remain unchanged.

### Example

```

=BESH.MULTI.CA_FIT(A1:D5)
=BESH.MULTI.CA_FIT(A1:D5,{"Low";"Medium";"High"},{"Control","Treatment A","Treatment B"})
```

## BESH.MULTI.CA_ROWS

Returns row-category overview statistics for a fitted correspondence-analysis model.

**Function wizard:** Returns row-category overview statistics for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_ROWS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per row category showing quality of representation, mass, chi-square distance,
and inertia. Mass is the row marginal proportion. Distance measures how far the row profile is from the average profile.
Inertia combines mass and distance, so rare but very unusual categories and common moderately unusual categories can both be influential.

## BESH.MULTI.CA_ROW_CONTRIB

Returns row contributions for each axis of a fitted correspondence-analysis model.

**Function wizard:** Returns row contributions for each axis of a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_ROW_CONTRIB(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of row contributions by available axis.
Contributions identify which row categories define each axis. High-contribution rows help anchor the interpretation of that dimension.

## BESH.MULTI.CA_ROW_COORD

Returns the row principal-coordinate matrix for a fitted correspondence-analysis model.

**Function wizard:** Returns the row principal-coordinate matrix for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_ROW_COORD(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix with one row per row category and one column per available axis.
These principal coordinates are the row points typically shown on a correspondence-analysis map.
Categories with similar coordinates have similar conditional profiles across the table columns.

## BESH.MULTI.CA_ROW_COS2

Returns row cos² values for each axis of a fitted correspondence-analysis model.

**Function wizard:** Returns row cos² values for each axis of a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_ROW_COS2(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of row cos² values (squared cosines) by available axis.
Cos² values measure how well an axis represents a row category. Large values indicate that the row lies mainly along that axis.

## BESH.MULTI.CA_SUMMARY

Returns a compact settings summary for a fitted correspondence-analysis model.

**Function wizard:** Returns a compact settings summary for a fitted correspondence-analysis model.

### Syntax

`=BESH.MULTI.CA_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.CA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table listing the analyzed table dimensions, the number of row and column categories,
the number of available axes, and the total inertia of the fitted correspondence-analysis solution.

## BESH.MULTI.DA_CANONCOEF

Returns the canonical coefficient matrix for a fitted linear discriminant-analysis model.

**Function wizard:** Returns the canonical coefficient matrix for a fitted linear discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_CANONCOEF(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled coefficient matrix showing how each predictor contributes to each canonical discriminant function on the working analysis scale.
Large absolute coefficients indicate variables that contribute strongly to the corresponding discriminant dimension.

## BESH.MULTI.DA_CANONICAL

Returns the canonical discriminant-functions summary for a fitted linear discriminant-analysis model.

**Function wizard:** Returns the canonical discriminant-functions summary for a fitted linear discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_CANONICAL(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table containing the eigenvalues, canonical correlations, explained proportions, and Wilks' lambda values for the canonical functions.
This output is available only for the linear method and only when at least one canonical function exists.

## BESH.MULTI.DA_CASEWISE

Returns the casewise classification table for the training or validation pass.

**Function wizard:** Returns the casewise classification table for the training or validation pass.

### Syntax

`=BESH.MULTI.DA_CASEWISE(handle, source, maxRows, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **source** — Optional source: training (default) or validation.
- **maxRows** — Optional maximum number of rows to return. Leave blank to spill the full table.
This is useful when you want to inspect the first portion of a large classification result without filling a large worksheet area.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled casewise table containing observed and predicted groups, assigned posterior probability, per-group posterior probabilities,
and squared distances for each case.

## BESH.MULTI.DA_CENTROIDS

Returns the group centroids in canonical discriminant space for a fitted linear discriminant-analysis model.

**Function wizard:** Returns the group centroids in canonical discriminant space for a fitted linear discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_CENTROIDS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table of group centroids on the canonical axes.
Centroids that are far apart indicate strong between-group separation on the corresponding discriminant functions.

## BESH.MULTI.DA_CONFUSION

Returns an observed-versus-predicted classification table for the training or validation pass.

**Function wizard:** Returns an observed-versus-predicted classification table for the training or validation pass.

### Syntax

`=BESH.MULTI.DA_CONFUSION(handle, source, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **source** — Optional result source: `"training"` (default) or `"validation"`.
The training table is the apparent or resubstitution table based on the fitted model.
The validation table is available only when validation was requested during fitting.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled confusion matrix including row totals, column totals, per-group recall, per-group precision, and overall classification accuracy.

## BESH.MULTI.DA_COVARIANCE

Returns the covariance matrix used by a fitted discriminant-analysis model.

**Function wizard:** Returns the covariance matrix used by a fitted discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_COVARIANCE(handle, groupLabel, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **groupLabel** — Optional group label. Leave blank to request the pooled covariance matrix used by linear discriminant analysis.
Supply a specific group label when you want that group's within-group covariance matrix.
For quadratic discriminant analysis, a group label is normally required because the model uses one covariance matrix per group.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A labeled covariance matrix on the working analysis scale.

## BESH.MULTI.DA_DROP

Removes a discriminant-analysis handle from memory.

**Function wizard:** Removes a discriminant-analysis handle from memory.

### Syntax

`=BESH.MULTI.DA_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.DA_FIT

Fits a discriminant-analysis classification model and returns a reusable handle.

**Function wizard:** Fits a discriminant-analysis model and returns a reusable handle.

### Syntax

`=BESH.MULTI.DA_FIT(x, groups, varNames, rowLabels, method, standardization, missingValuePolicy, priorMode, priorLabels, priorProbabilities, covarianceRegularization, validationMode, numberOfFolds, holdoutFraction, stratified, randomSeed)`

### Parameters

- **x** — Numeric predictor matrix with observations in rows and analysis variables in columns.
A single header row is detected automatically when the first row is nonnumeric and the rows below are numeric.
Use one row per case and one numeric predictor per column.
- **groups** — One-column grouping variable aligned with `x`. The grouping variable defines the known classes used to fit the classifier.
Text or numeric labels are accepted. A single top header cell is detected automatically when the supplied range is a whole-column style reference.
- **varNames** — Optional predictor names supplied either as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from the detected header row when available; otherwise default names X1, X2, … are generated.
- **rowLabels** — Optional one-column range of case labels aligned with `x`.
These labels are carried into the casewise classification tables and removed-row report. When omitted, generic labels are generated.
- **method** — Optional discriminant method: `"linear"` (default) or `"quadratic"`.
Linear discriminant analysis assumes all groups share one pooled within-group covariance matrix.
Quadratic discriminant analysis allows each group to have its own covariance matrix and can model curved decision boundaries.
- **standardization** — Optional preprocessing mode: `"none"` (default), `"zscores"`, or `"range01"`.
Standardization is useful when predictors are measured on very different scales and you do not want variables with larger units to dominate the covariance structure.
- **missingValuePolicy** — Optional missing-data policy: `"error"` (default) or `"listwise"`.
The error policy stops the fit when any case contains a missing or non-finite predictor value.
The listwise policy removes incomplete rows before the model is estimated.
- **priorMode** — Optional prior-probability mode: `"proportional"` (default), `"equal"`, or `"user"`.
Priors affect posterior probabilities and therefore the final classification rule, especially when groups are imbalanced or when misclassification costs are conceptually asymmetric.
- **priorLabels** — Optional group labels used only when `priorMode` is `"user"`.
Supply a comma-separated list or a one-row or one-column range with one label per training group.
- **priorProbabilities** — Optional prior probabilities used only when `priorMode` is `"user"`.
Supply a one-row or one-column numeric range, or a comma-separated numeric list, aligned with `priorLabels`.
The values are internally normalized to sum to 1.
- **covarianceRegularization** — Optional non-negative ridge constant added to covariance diagonals when inversion is numerically difficult.
Default: 0.00000001. Increase this slightly when near-singular covariance matrices are expected, for example with highly collinear predictors or very small groups.
- **validationMode** — Optional validation strategy: `"none"` (default), `"leaveoneout"`, `"kfold"`, or `"holdout"`.
Validation does not change the fitted final training model; it adds an extra out-of-sample style assessment.
- **numberOfFolds** — Optional number of folds for k-fold validation. Default 5.
- **holdoutFraction** — Optional test-set fraction for holdout validation. Default 0.3.
- **stratified** — Optional TRUE/FALSE flag controlling whether k-fold and holdout validation preserve the observed group proportions as closely as possible.
Default TRUE.
- **randomSeed** — Optional deterministic random seed for k-fold or holdout validation.
Leave blank to use a time-based seed. Supplying a seed improves reproducibility across recalculations.

### Returns

A text handle for the fitted discriminant-analysis model. Pass the handle to the other `DA_*` worksheet functions
to retrieve settings, group summaries, mean tables, covariance matrices, classification tables, canonical summaries, or prediction output.

### Notes

Discriminant analysis is a supervised classification method. It starts from observations whose group membership is already known,
estimates one score function per group, and then classifies each case to the group with the largest posterior support.
It is often used both for practical prediction and for understanding which groups are well separated in multivariate space.

Use the linear method when the groups can reasonably share one within-group covariance matrix and you want a stable, interpretable model.
Use the quadratic method when group covariance patterns are meaningfully different and you have enough observations in each group to estimate them reliably.
The quadratic method is more flexible but usually needs more data.

When validation is requested, the model also stores a second classification table based on leave-one-out, k-fold, or holdout validation.
The apparent training table is still available separately because it answers a different question: how well the fitted rule classifies the same data used to estimate it.

### Example

```

=BESH.MULTI.DA_FIT(B1:F101,A1:A101)
=BESH.MULTI.DA_FIT(B1:F101,A1:A101,,G1:G101,"linear","zscores","listwise","equal")
=BESH.MULTI.DA_FIT(B1:F101,A1:A101,,,"quadratic","none","listwise","user",{"Control";"Case"},{0.4;0.6},1E-06,"kfold",10,,TRUE,12345)
```

## BESH.MULTI.DA_FUNCTIONS

Returns the linear classification-function table for a fitted linear discriminant-analysis model.

**Function wizard:** Returns the linear classification-function table for a fitted linear discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_FUNCTIONS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table containing the linear classification constants and coefficients on the original input scale.
These coefficients are available only for the linear method. For each group, calculate the displayed linear score and assign the case to the group with the largest score.

## BESH.MULTI.DA_GROUPSUMMARY

Returns group counts, prior probabilities, and covariance diagnostics for a fitted discriminant-analysis model.

**Function wizard:** Returns group counts, prior probabilities, and covariance diagnostics for a fitted discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_GROUPSUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per group showing the number of training cases retained for that group,
the prior probability used by the classifier, and the group-specific covariance diagnostics on the working analysis scale.

## BESH.MULTI.DA_MEANS

Returns the group mean table for a fitted discriminant-analysis model.

**Function wizard:** Returns the group mean table for a fitted discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_MEANS(handle, scale, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **scale** — Optional output scale: `"original"` (default) or `"working"`.
The original scale reports means in the original measurement units.
The working scale reports means after any requested standardization and is therefore the scale used by the fitted covariance matrices and classification functions.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A labeled matrix of group means with groups in rows and predictors in columns.

## BESH.MULTI.DA_PREDICT

Applies a fitted discriminant-analysis model to a new predictor matrix and returns casewise predictions.

**Function wizard:** Applies a fitted discriminant-analysis model to a new predictor matrix and returns casewise predictions.

### Syntax

`=BESH.MULTI.DA_PREDICT(handle, newX, rowLabels, actualGroups, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **newX** — New numeric predictor matrix with the same columns and column order used when the model was fitted.
- **rowLabels** — Optional one-column range of case labels aligned with `newX`.
- **actualGroups** — Optional one-column range of known groups for the new cases, used only to populate the Actual column in the output.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled casewise prediction table containing predicted groups, assigned posterior probabilities, per-group posterior probabilities,
and squared distances for each new case.

## BESH.MULTI.DA_PREPROCESS

Returns the preprocessing constants used by a fitted discriminant-analysis model.

**Function wizard:** Returns the preprocessing constants used by a fitted discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_PREPROCESS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table of variable-wise location and scale constants used during preprocessing.
When no preprocessing was applied, the function returns a compact note table rather than an error.

## BESH.MULTI.DA_REMOVED

Returns the rows removed by the missing-value policy before discriminant analysis was fitted.

**Function wizard:** Returns the rows removed by the missing-value policy before discriminant analysis was fitted.

### Syntax

`=BESH.MULTI.DA_REMOVED(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table listing the original case indices and optional case labels removed before fitting because at least one predictor
was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.

## BESH.MULTI.DA_SUMMARY

Returns a compact settings and performance summary for a fitted discriminant-analysis model.

**Function wizard:** Returns a compact settings and performance summary for a fitted discriminant-analysis model.

### Syntax

`=BESH.MULTI.DA_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.DA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column summary describing the analysis method, preprocessing, priors, validation settings,
sample size after missing-data handling, number of groups, and apparent and validation classification accuracy when available.

## BESH.MULTI.FA_COMMUNALITIES

Returns the communalities table for a fitted factor-analysis model.

**Function wizard:** Returns communalities and uniquenesses for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_COMMUNALITIES(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table giving the initial communality, each factor’s contribution, the final extracted communality, and the uniqueness for every variable.
Large communalities indicate that the retained factors explain most of a variable’s variance. Large uniqueness values indicate that much of the variance remains specific or residual.

## BESH.MULTI.FA_DROP

Removes a factor-analysis handle from memory.

**Function wizard:** Removes a factor-analysis handle from memory.

### Syntax

`=BESH.MULTI.FA_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.FA_EIGEN

Returns the initial and retained variance table for a fitted factor-analysis model.

**Function wizard:** Returns the initial and retained variance table for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_EIGEN(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table showing initial eigenvalues and percentages for all variables, plus the extraction and rotation sums of squares for the retained factors.
This table is useful for deciding how many factors to keep and for reporting how much common variance is represented by the final solution.

## BESH.MULTI.FA_FACTORABILITY

Returns factorability diagnostics for a fitted factor-analysis model.

**Function wizard:** Returns factorability diagnostics for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_FACTORABILITY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table containing the determinant of the correlation matrix, overall KMO, Bartlett’s test,
its degrees of freedom and p-value, and RMSR. These diagnostics help judge whether factor analysis is appropriate
and how well the retained-factor solution reproduces the observed association matrix.

## BESH.MULTI.FA_FACTORCORR

Returns the factor-correlation matrix for a fitted factor-analysis model.

**Function wizard:** Returns the factor-correlation matrix for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_FACTORCORR(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A labeled square matrix of factor correlations. Under orthogonal rotation this is the identity matrix.
Under oblique rotation the off-diagonal values show how strongly the retained factors correlate with one another.

## BESH.MULTI.FA_FIT

Fits an exploratory factor-analysis model and returns a reusable handle.

**Function wizard:** Fits an exploratory factor-analysis model and returns a reusable handle.

### Syntax

`=BESH.MULTI.FA_FIT(x, varNames, matrixType, extractionMethod, retentionMethod, retentionValue, rotationMethod, scoreMethod, communalityInitialization, missingValuePolicy, useKaiserNormalization, promaxPower, maxIterations, epsilon)`

### Parameters

- **x** — Numeric data matrix with observations in rows and variables in columns.
If the first row contains labels, that row is detected automatically and used as variable names.
Missing or invalid cells are passed through to the factor-analysis engine so that the requested missing-value policy can be applied.
- **varNames** — Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
- **matrixType** — Optional working matrix: `"correlation"` (default) or `"covariance"`.
Correlation analysis standardizes variables first and is the usual choice when variables are measured on different scales.
Covariance analysis preserves the original measurement scale.
- **extractionMethod** — Optional extraction method. Accepted values include `"principalaxis"`, `"principalcomponents"`, `"ml"`,
`"gls"`, `"image"`, and `"alpha"`. Default: `"principalaxis"`.
- **retentionMethod** — Optional retention rule: `"fixed"`, `"eigenvalue"`, or `"variance"`. Default: `"eigenvalue"`.
- **retentionValue** — Optional parameter paired with the retention rule.
Use an integer count for the fixed rule, an eigenvalue cutoff such as 1.0 for the eigenvalue rule,
or a target cumulative percentage such as 70 for the variance rule. Default: 1.0.
- **rotationMethod** — Optional post-extraction rotation. Accepted values include `"none"`, `"varimax"`, `"quartimax"`, `"equamax"`, and `"promax"`.
Rotation is used to obtain a loading pattern that is often easier to interpret than the raw extraction output.
Default: `"none"`.
- **scoreMethod** — Optional factor-score estimator. Accepted values include `"none"`, `"regression"`, and `"bartlett"`.
Default: `"regression"`.
- **communalityInitialization** — Optional starting communality rule used by principal-axis factoring.
Accepted values include `"smc"` for squared multiple correlations and `"one"` for unit communalities.
Default: `"smc"`.
- **missingValuePolicy** — Optional missing-data policy. Accepted values include `"error"` and `"listwise"`.
Use `"error"` when any missing value should stop the analysis. Use `"listwise"` to delete incomplete rows before fitting.
Default: `"error"`.
- **useKaiserNormalization** — Optional TRUE/FALSE flag controlling Kaiser normalization before orthomax-family rotation.
This is commonly left TRUE for varimax, quartimax, equamax, and promax. Default TRUE.
- **promaxPower** — Optional power used when promax rotation is requested. Larger values typically encourage a simpler, more polarized loading pattern.
Standard software often uses 4. Default 4.
- **maxIterations** — Optional maximum number of iterations used by extraction and rotation routines. Default 250.
- **epsilon** — Optional convergence tolerance for iterative fitting routines. Default 0.000001.

### Returns

A text handle for the fitted factor-analysis model. Pass the handle to the other `FA_*` worksheet functions
to retrieve the specific output needed for reporting, interpretation, or downstream analysis.

### Notes

Unlike PCA, exploratory factor analysis focuses on shared variance and separates common variance from uniqueness.
Rotation choices influence interpretability, and oblique rotation additionally allows the retained factors to correlate.

The returned handle is especially useful when you want to inspect several tables from the same fitted solution,
such as communalities, the rotated pattern matrix, factor correlations, and factor scores.

### Example

```

=BESH.MULTI.FA_FIT(A1:H31)
=BESH.MULTI.FA_FIT(A1:H31,,"correlation","ml","eigenvalue",1,"varimax")
=BESH.MULTI.FA_FIT(A1:H31,,"correlation","principalaxis","fixed",3,"promax","regression","smc","listwise",TRUE,4)
```

## BESH.MULTI.FA_LOADINGS

Returns the rotated loading matrix for a fitted factor-analysis model.

**Function wizard:** Returns the rotated loading matrix for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_LOADINGS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per variable and one column per retained factor.
For orthogonal rotations this is the usual rotated loading matrix. For oblique rotation it is the pattern matrix.

## BESH.MULTI.FA_MATRIX

Returns the working covariance or correlation matrix for a fitted factor-analysis model.

**Function wizard:** Returns the working covariance or correlation matrix for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_MATRIX(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row with variable names. Default TRUE.

### Returns

A labeled square matrix containing the working analysis matrix.

## BESH.MULTI.FA_SCORES

Returns factor scores for the analyzed observations.

**Function wizard:** Returns factor scores for the analyzed observations.

### Syntax

`=BESH.MULTI.FA_SCORES(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per analyzed observation and one column per retained factor.
The first column contains row identifiers after any listwise deletion requested by the missing-value policy.
If factor scores were not requested when the model was fitted, this function returns `#N/A`.

## BESH.MULTI.FA_STRUCTURE

Returns the strctre matrix for a fitted factor-analysis model.

**Function wizard:** Returns the strctre matrix for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_STRUCTURE(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per variable and one column per retained factor.
Under orthogonal rotation this matrix equals the loading matrix. Under oblique rotation it contains variable–factor correlations.

## BESH.MULTI.FA_SUMMARY

Returns a compact settings and convergence summary for a fitted factor-analysis model.

**Function wizard:** Returns a compact settings and convergence summary for a fitted factor-analysis model.

### Syntax

`=BESH.MULTI.FA_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.FA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table describing the working matrix, extraction and rotation choices,
sample size after missing-data handling, retained factors, convergence flags, and RMSR.

## BESH.MULTI.HCLUST_AGGLOM

Returns the agglomeration schedule for a fitted hierarchical clustering model.

**Function wizard:** Returns the agglomeration schedule for a fitted hierarchical clustering model.

### Syntax

`=BESH.MULTI.HCLUST_AGGLOM(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per merge showing the left and right cluster ids merged at each step, the merge height,
and the size of the newly formed cluster.

## BESH.MULTI.HCLUST_DROP

Removes a hierarchical clustering handle from memory.

**Function wizard:** Removes a hierarchical clustering handle from memory.

### Syntax

`=BESH.MULTI.HCLUST_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.HCLUST_FIT

Fits an agglomerative hierarchical clustering model and returns a reusable handle.

**Function wizard:** Fits an agglomerative hierarchical clustering model and returns a reusable handle.

### Syntax

`=BESH.MULTI.HCLUST_FIT(x, varNames, rowLabels, linkage, distanceMetric, minkowskiPower, standardization, missingValuePolicy)`

### Parameters

- **x** — Numeric data matrix with observations in rows and variables in columns.
A single header row is detected automatically when present. Missing values are passed through to the clustering engine
so the requested missing-value policy can either stop the analysis or remove incomplete rows listwise.
- **varNames** — Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
- **rowLabels** — Optional one-column range of observation labels aligned with `x`.
These labels are carried into the leaf-order, membership, and removed-row tables. When omitted, generic labels are used.
- **linkage** — Optional agglomeration rule: `"ward"` (default), `"single"`, `"complete"`, `"average"`,
`"weightedaverage"`, `"centroid"`, or `"median"`.
- **distanceMetric** — Optional base observation-level distance: `"squaredeuclidean"` (default), `"euclidean"`, `"manhattan"`,
`"chebyshev"`, `"minkowski"`, `"cosine"`, or `"correlation"`.
Some linkages impose restrictions: centroid, median, and Ward linkage require Euclidean or squared Euclidean distance.
- **minkowskiPower** — Optional Minkowski power parameter used only when `distanceMetric` is `"minkowski"`. Default 2.
- **standardization** — Optional preprocessing mode: none (default), zscores, or range01.
- **missingValuePolicy** — Optional missing-data policy: error (default) or listwise.

### Returns

A text handle for the fitted hierarchical clustering solution. Pass the handle to the other `HCLUST_*` worksheet functions
to retrieve the fit summary, agglomeration schedule, leaf order, membership tables, preprocessing constants, or removed-row report.

### Notes

Hierarchical clustering builds a full merge tree rather than a single partition. You can later cut that tree either to a requested number
of clusters or at a chosen merge height without refitting the model.

The handle-based design is therefore especially convenient: fit the tree once, then inspect several alternative cut levels using
repeated calls to `HCLUST_MEMBERSHIP`.

### Example

```

=BESH.MULTI.HCLUST_FIT(A1:F101)
=BESH.MULTI.HCLUST_FIT(A1:F101,,G1:G101,"ward","squaredeuclidean",2,"zscores","listwise")
```

## BESH.MULTI.HCLUST_LEAFORDER

Returns the leaf order used to display the fitted hierarchical tree.

**Function wizard:** Returns the leaf order used to display the fitted hierarchical tree.

### Syntax

`=BESH.MULTI.HCLUST_LEAFORDER(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table giving the display order from left to right in the dendrogram together with the original row numbers and row labels.

## BESH.MULTI.HCLUST_MEMBERSHIP

Returns a cluster-membership table obtained by cutting the fitted hierarchical tree.

**Function wizard:** Returns a cluster-membership table obtained by cutting the fitted hierarchical tree.

### Syntax

`=BESH.MULTI.HCLUST_MEMBERSHIP(handle, mode, value, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **mode** — Optional cut mode: `"clusters"` or `"count"` to cut the tree to a requested number of clusters, or `"height"`
to cut the tree at a merge-height threshold. Default: `"clusters"`.
- **value** — Optional parameter paired with `mode`. Supply an integer cluster count when the mode is by clusters,
or a numeric merge-height threshold when the mode is by height. Default: 3 when mode is by clusters, otherwise 0.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled membership table for the active observations. Because the tree is already fitted, you can call this function repeatedly
with different cut values to compare alternative cluster solutions without refitting the hierarchy.

## BESH.MULTI.HCLUST_PREPROCESS

Returns the preprocessing constants used by a fitted hierarchical clustering model.

**Function wizard:** Returns the preprocessing constants used by a fitted hierarchical clustering model.

### Syntax

`=BESH.MULTI.HCLUST_PREPROCESS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table of variable-wise location and scale constants used during preprocessing.
When no preprocessing was applied, the function returns a compact note table rather than an error.

## BESH.MULTI.HCLUST_REMOVED

Returns the rows removed by the missing-value policy before hierarchical clustering was fitted.

**Function wizard:** Returns the rows removed by the missing-value policy before hierarchical clustering was fitted.

### Syntax

`=BESH.MULTI.HCLUST_REMOVED(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table listing the original row numbers and optional row labels removed before fitting because at least one analysis variable
was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.

## BESH.MULTI.HCLUST_SUMMARY

Returns a compact settings and fit summary for a fitted hierarchical clustering model.

**Function wizard:** Returns a compact settings and fit summary for a fitted hierarchical clustering model.

### Syntax

`=BESH.MULTI.HCLUST_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.HCLUST_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table listing the linkage rule, distance metric, preprocessing choices, active and removed observation counts,
total merge steps, and the final merge height of the fitted tree.

## BESH.MULTI.KMEANS_ASSIGNMENTS

Returns the active-observation assignment table for a fitted k-means model.

**Function wizard:** Returns the active-observation assignment table for a fitted k-means model.

### Syntax

`=BESH.MULTI.KMEANS_ASSIGNMENTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table containing the original row number, optional row label, assigned cluster, and point-to-center distance
for every active observation retained in the fitted k-means analysis.

## BESH.MULTI.KMEANS_CENTERS

Returns the fitted k-means cluster centers.

**Function wizard:** Returns the fitted k-means cluster centers.

### Syntax

`=BESH.MULTI.KMEANS_CENTERS(handle, scale, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.
- **scale** — Optional center scale: `"original"` (default) or `"working"`.
Use original to report centers back on the original measurement scale, or working to inspect the centers after preprocessing.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per cluster containing the cluster size, the within-cluster sum of squares for that cluster,
and the fitted center coordinates.

## BESH.MULTI.KMEANS_DROP

Removes a k-means handle from memory.

**Function wizard:** Removes a k-means handle from memory.

### Syntax

`=BESH.MULTI.KMEANS_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.KMEANS_FIT

Fits a k-means clustering model and returns a reusable handle.

**Function wizard:** Fits a k-means clustering model and returns a reusable handle.

### Syntax

`=BESH.MULTI.KMEANS_FIT(x, varNames, rowLabels, numberOfClusters, initializationMethod, distanceMetric, nStarts, maxIterations, tolerance, standardization, missingValuePolicy, emptyClusterHandling, randomSeed, startingCenters)`

### Parameters

- **x** — Numeric data matrix with observations in rows and variables in columns.
A single header row is detected automatically when present. Missing values are passed through to the clustering engine
so the requested missing-value policy can either stop the analysis or remove incomplete rows listwise.
- **varNames** — Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
- **rowLabels** — Optional one-column range of observation labels aligned with `x`.
These labels are carried into the assignment and removed-row tables. When omitted, generic observation labels are used.
- **numberOfClusters** — Requested number of clusters `k`. Default 3.
- **initializationMethod** — Optional initialization strategy: `"kmeans++"` (default), `"forgy"`, `"randompartition"`, or `"userspecified"`.
When `startingCenters` is supplied, user-specified centers are used automatically.
- **distanceMetric** — Optional reporting distance: `"squaredeuclidean"` (default) or `"euclidean"`.
Classical k-means still minimizes the sum of squared Euclidean distances either way; this option mainly affects the displayed distances.
- **nStarts** — Optional number of random starts. Default 10.
- **maxIterations** — Optional maximum number of update iterations per start. Default 100.
- **tolerance** — Optional convergence tolerance for center movement on the working analysis scale. Default 0.000001.
- **standardization** — Optional preprocessing mode: `"none"` (default), `"zscores"`, or `"range01"`.
Standardization is helpful when variables are measured on very different scales.
- **missingValuePolicy** — Optional missing-data policy: `"error"` (default) or `"listwise"`.
- **emptyClusterHandling** — Optional strategy for a temporarily empty cluster: `"farthestobservation"` (default), `"randomobservation"`, or `"keeppreviouscenter"`.
- **randomSeed** — Optional deterministic random seed. Leave blank to use a time-based seed. Supplying a seed improves reproducibility across recalculations.
- **startingCenters** — Optional matrix of user-specified starting centers, with one row per cluster and one column per variable.
When supplied, the matrix must have exactly `numberOfClusters` rows and the same number of columns as `x`.

### Returns

A text handle for the fitted k-means solution. Pass the handle to the other `KMEANS_*` worksheet functions
to retrieve the fit summary, centers, assignments, preprocessing constants, or removed-row report.

### Notes

K-means partitions observations into `k` compact centroid-based clusters. Because the objective is non-convex,
the final partition can depend on the starting centers. Multiple random starts and k-means++ seeding often improve the solution.

The handle-based design is useful when you want to inspect several outputs from the same fitted partition without repeating the fit.

### Example

```

=BESH.MULTI.KMEANS_FIT(A1:F101)
=BESH.MULTI.KMEANS_FIT(A1:F101,,G1:G101,4,"kmeans++","squaredeuclidean",25,100,1E-06,"zscores","listwise","farthestobservation",12345)
```

## BESH.MULTI.KMEANS_PREPROCESS

Returns the preprocessing constants used by a fitted k-means model.

**Function wizard:** Returns the preprocessing constants used by a fitted k-means model.

### Syntax

`=BESH.MULTI.KMEANS_PREPROCESS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table of variable-wise location and scale constants used during preprocessing.
For z-score standardization the location is the mean and the scale is the sample standard deviation.
For range standardization the location is the minimum and the scale is the observed range. When no preprocessing was applied,
the function returns a compact note table rather than an error.

## BESH.MULTI.KMEANS_REMOVED

Returns the rows removed by the missing-value policy before k-means fitting.

**Function wizard:** Returns the rows removed by the missing-value policy before k-means fitting.

### Syntax

`=BESH.MULTI.KMEANS_REMOVED(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table listing the original row numbers and optional row labels removed before fitting because at least one analysis variable
was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.

## BESH.MULTI.KMEANS_SUMMARY

Returns a compact settings and fit summary for a fitted k-means model.

**Function wizard:** Returns a compact settings and fit summary for a fitted k-means model.

### Syntax

`=BESH.MULTI.KMEANS_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.KMEANS_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table listing the requested and realized clustering settings together with key fit diagnostics,
including the number of active observations, removed observations, convergence, and sums of squares.

## BESH.MULTI.MCA_BURT

Returns the Burt table for a fitted multiple correspondence-analysis model.

**Function wizard:** Returns the Burt table for a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_BURT(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A labeled square matrix containing all pairwise cross-tabulations between category levels.
Diagonal blocks contain one-variable category counts. Off-diagonal blocks contain contingency tables for pairs of variables.

## BESH.MULTI.MCA_CATEGORIES

Returns category-level overview statistics for a fitted multiple correspondence-analysis model.

**Function wizard:** Returns category-level overview statistics for a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_CATEGORIES(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per category level showing the originating variable, the category label,
quality of representation, mass, chi-square distance, and inertia.

## BESH.MULTI.MCA_CONTRIB

Returns category contributions for each axis of a fitted multiple correspondence-analysis model.

**Function wizard:** Returns category contributions for each axis of a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_CONTRIB(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of category contributions by available axis.
High-contribution categories are the ones that primarily define the orientation of each MCA dimension.

## BESH.MULTI.MCA_COORD

Returns category principal coordinates for a fitted multiple correspondence-analysis model.

**Function wizard:** Returns category principal coordinates for a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_COORD(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix with one row per category and one column per available axis.
Categories with similar coordinates tend to co-occur across observations.

## BESH.MULTI.MCA_COS2

Returns category cos² values for each axis of a fitted multiple correspondence-analysis model.

**Function wizard:** Returns category cos² values for each axis of a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_COS2(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled matrix of category cos² values (squared cosines) by available axis.
Large values indicate that the category is well represented by that axis.

## BESH.MULTI.MCA_DROP

Removes a multiple correspondence-analysis handle from memory.

**Function wizard:** Removes a multiple correspondence-analysis handle from memory.

### Syntax

`=BESH.MULTI.MCA_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.MCA_EIGEN

Returns the inertia (eigenvalue) table for a fitted multiple correspondence-analysis model.

**Function wizard:** Returns inertia and explained-percentage summaries for a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_EIGEN(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per axis showing principal inertia, percentage inertia, and cumulative percentage inertia.

## BESH.MULTI.MCA_FIT

Fits a multiple correspondence-analysis model to a matrix of categorical variables and returns a reusable handle.

**Function wizard:** Fits a multiple correspondence-analysis model to a categorical data matrix and returns a reusable handle.

### Syntax

`=BESH.MULTI.MCA_FIT(x, varNames, hasHeader)`

### Parameters

- **x** — Categorical data matrix with one observation per row and one categorical variable per column.
Cells are converted to trimmed text. Numbers are allowed and are treated as category labels.
Blank cells are treated as an empty-string category unless you recode them beforehand.
- **varNames** — Optional variable names as a comma-separated list or a one-row or one-column range.
When omitted, names are taken from the first row when `hasHeader` is TRUE; otherwise default names Variable 1, Variable 2, … are generated.
- **hasHeader** — Optional flag indicating whether the first row of `x` contains variable names rather than observations.
Default: TRUE when `varNames` is omitted, otherwise FALSE.

### Returns

A text handle for the fitted MCA solution. Pass the handle to the other `MCA_*` worksheet functions
to retrieve eigen summaries, Burt and indicator matrices, category overview tables, coordinates, cos² values, and contributions.

### Notes

Multiple correspondence analysis extends simple correspondence analysis from a two-way table to several categorical variables.
Internally, the method constructs an indicator matrix with one binary column per category level across all variables, and it also forms
the Burt table containing all pairwise cross-tabulations between category levels.

MCA is useful when you want to explore association structure in survey-like data, questionnaire items, or collections of coded categorical variables.
Categories that tend to occur together appear near each other in the coordinate map, while categories with very different association profiles
are separated along the dominant axes.

Because the implementation treats raw cell text as categories, it is usually best to clean spelling and capitalization first.
For example, `"yes"`, `"Yes"`, and `"YES"` are different categories unless standardized before fitting.

### Example

```

=BESH.MULTI.MCA_FIT(A1:F101)
=BESH.MULTI.MCA_FIT(A2:F101,{"Sex","Smoke","Diet","Exercise","Region","Outcome"},FALSE)
```

## BESH.MULTI.MCA_INDICATOR

Returns the indicator (design) matrix used internally by a fitted multiple correspondence-analysis model.

**Function wizard:** Returns the indicator (design) matrix used internally by a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_INDICATOR(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled binary matrix with one row per observation and one column per category level.
Each row contains one active category for each original variable.

## BESH.MULTI.MCA_SUMMARY

Returns a compact settings summary for a fitted multiple correspondence-analysis model.

**Function wizard:** Returns a compact settings summary for a fitted multiple correspondence-analysis model.

### Syntax

`=BESH.MULTI.MCA_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.MCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table listing the number of observations, variables, total category levels,
available axes, and total inertia of the fitted MCA solution.

## BESH.MULTI.PCA_DROP

Removes a principal component analysis handle from memory.

**Function wizard:** Removes a principal component analysis handle from memory.

### Syntax

`=BESH.MULTI.PCA_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.

### Returns

TRUE when the handle was removed; FALSE when the handle was not found.

## BESH.MULTI.PCA_EIGEN

Returns the eigenvalue table for a fitted principal component model.

**Function wizard:** Returns eigenvalues and explained-variance summaries for a fitted principal component model.

### Syntax

`=BESH.MULTI.PCA_EIGEN(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per component showing the eigenvalue, percentage of variance explained,
cumulative percentage, and whether the component was retained by the requested rule.

## BESH.MULTI.PCA_FIT

Fits a principal component analysis model and returns a reusable handle.

**Function wizard:** Fits a principal component analysis model and returns a reusable handle.

### Syntax

`=BESH.MULTI.PCA_FIT(x, varNames, matrixType, retentionMethod, retentionValue, maxIterations, epsilon)`

### Parameters

- **x** — Numeric data matrix with observations in rows and variables in columns.
A single header row is detected automatically when the first row is nonnumeric and the rows below are numeric.
Rows containing invalid or missing numeric values are removed before fitting, so the returned PCA handle always
represents a complete numeric matrix.
- **varNames** — Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
- **matrixType** — Optional matrix type. Accepted values include `"correlation"` and `"covariance"`.
Choose correlation when variables are on different scales and you want each variable to contribute on a standardized basis.
Choose covariance when the original measurement units are directly meaningful and comparable.
Default: `"correlation"`.
- **retentionMethod** — Optional component-retention rule. Accepted values include `"eigenvalue"`, `"fixed"`, and `"variance"`.
The eigenvalue rule retains components whose eigenvalue meets the cutoff. The fixed rule keeps an exact number of components.
The variance rule keeps the smallest number of components that reaches the requested cumulative percentage.
Default: `"eigenvalue"`.
- **retentionValue** — Optional value paired with `retentionMethod`.
Use 1.0 for the common Kaiser rule under the eigenvalue method, an integer count for the fixed method,
or a cumulative percentage such as 80 for the variance method.
Default: 1.0.
- **maxIterations** — Optional maximum number of iterations for the eigenvalue solver. Larger values may help for numerically difficult matrices.
Default: 250.
- **epsilon** — Optional numerical convergence tolerance for the eigenvalue solver. Smaller values request stricter convergence.
Default: 0.000001.

### Returns

A text handle for the fitted principal component model. Pass the handle to the other `PCA_*` worksheet functions
to retrieve only the result table you need, such as the working matrix, eigenvalue table, loading matrix, or scores.

### Notes

PCA decomposes either the covariance matrix or the correlation matrix into orthogonal linear combinations of the original
variables. The retained components are ordered from largest to smallest explained variance.

The handle-based design is especially helpful when you want to use the same fitted PCA solution in several worksheet
locations without recomputing the decomposition each time.

### Example

```

=BESH.MULTI.PCA_FIT(A1:H31)
=BESH.MULTI.PCA_FIT(A1:H31,,"correlation","variance",80)
=BESH.MULTI.PCA_FIT(A1:H31,,"covariance","fixed",3,500,1E-08)
```

## BESH.MULTI.PCA_LOADINGS

Returns the retained principal-component loading matrix.

**Function wizard:** Returns the retained loading matrix for a fitted principal component model.

### Syntax

`=BESH.MULTI.PCA_LOADINGS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per variable and one column per retained component.
The values are the retained loading directions used to compute component scores.

## BESH.MULTI.PCA_MATRIX

Returns the analyzed covariance or correlation matrix for a fitted principal component model.

**Function wizard:** Returns the analyzed covariance or correlation matrix for a fitted principal component model.

### Syntax

`=BESH.MULTI.PCA_MATRIX(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.
- **includeHeader** — TRUE to include a header row with variable names. Default TRUE.

### Returns

A labeled square matrix. The first column contains variable names. When `includeHeader` is TRUE,
the first row contains the same variable names as column headers.

## BESH.MULTI.PCA_SCORES

Returns principal-component scores for the analyzed observations.

**Function wizard:** Returns principal-component scores for the analyzed observations.

### Syntax

`=BESH.MULTI.PCA_SCORES(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled table with one row per analyzed observation and one column per retained component.
The first column contains row identifiers relative to the supplied input range after rows with invalid values were removed.

## BESH.MULTI.PCA_SUMMARY

Returns a compact settings summary for a fitted principal component analysis model.

**Function wizard:** Returns a compact settings summary for a fitted principal component analysis model.

### Syntax

`=BESH.MULTI.PCA_SUMMARY(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.MULTI.PCA_FIT`.
- **includeHeader** — TRUE to include a header row. Default TRUE.

### Returns

A spilled two-column table listing the matrix analyzed, dimensions, retained components, and retention rule.
