# Regression Models UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Regression Formula Syntax](regression-formula-syntax.md)
- [Generalized Estimating Equations Gee](../methods/generalized-estimating-equations-gee.md)
- [Negative Binomial Regression Nb2](../methods/negative-binomial-regression-nb2.md)
- [Generalized Linear Models Glm](../methods/generalized-linear-models-glm.md)
- [Multiple Linear Regression Lm](../methods/multiple-linear-regression-lm.md)
- [Mixed Models For Repeated Measures Mmrm](../methods/mixed-models-for-repeated-measures-mmrm.md)
- [Multinomial Logistic Regression](../methods/multinomial-logistic-regression.md)
- [Ordinal Logistic Regression](../methods/ordinal-logistic-regression.md)
- [Zero Inflated Poisson Regression](../methods/zero-inflated-poisson-regression.md)

## BESH.CLASS.BRIER

Returns the Brier score for observed binary outcomes and predicted probabilities.

**Function wizard:** Returns the Brier score for observed binary outcomes and predicted probabilities.

### Syntax

`=BESH.CLASS.BRIER(y, probabilities, weights, includeHeader)`

### Parameters

- **y** — Observed binary outcomes coded as `0` and `1`.
Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
- **probabilities** — Predicted event probabilities corresponding row-by-row to `y`.
Supply a single worksheet row or single worksheet column. Values must lie in `[0,1]`.
A leading text header is allowed and is ignored.
- **weights** — Optional nonnegative case weights aligned with `y` and `probabilities`.
Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A small worksheet table containing the Brier score and, for convenience, the sample size and event rate.

### Notes

For observed binary outcomes `y_i` and predicted probabilities `p_i`, the Brier score is the mean squared
probability error. In the unweighted case it is `(1/n) Σ (y_i - p_i)^2`; when observation weights are present,
the corresponding weighted mean is returned.

This summary is threshold-free and complements the threshold-based reports returned by
`BESH.CLASS.CONFUSION` and `BESH.CLASS.THRESH`.

Example:
`=BESH.CLASS.BRIER(A2:A101,B2:B101)`
or
`=BESH.CLASS.BRIER(A2:A101,B2:B101,C2:C101,TRUE)`

## BESH.CLASS.CALIB

Returns calibration-plot data for observed binary outcomes and predicted probabilities.

**Function wizard:** Returns calibration-plot data for observed binary outcomes and predicted probabilities.

### Syntax

`=BESH.CLASS.CALIB(y, probabilities, bins, method, weights, includeHeader)`

### Parameters

- **y** — Observed binary outcomes coded as `0` and `1`.
Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
- **probabilities** — Predicted event probabilities corresponding row-by-row to `y`.
Supply a single worksheet row or single worksheet column. Values must lie in `[0,1]`.
A leading text header is allowed and is ignored.
- **bins** — Optional positive integer giving the number of calibration bins. The default is `10`.
The current implementation requires at least `2` bins.
- **method** — Optional calibration binning method.
Accepted values are `"quantile"` (default) and `"equalwidth"`.
Quantile binning creates groups with approximately equal numbers of observations, while equal-width binning
partitions the probability scale into equal intervals.
- **weights** — Optional nonnegative case weights aligned with `y` and `probabilities`.
Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per calibration bin and the columns:
bin index, number of observations, mean predicted probability, observed event rate,
and lower/upper confidence limits for the observed event rate.
The probability-scale columns are returned on the 0–1 scale so they can be plotted directly in Excel.

### Notes

This function is intended to support calibration plots and simple calibration diagnostics after a set of event probabilities
has been produced. It groups observations by predicted probability and compares the mean predicted risk in each bin with the
empirical event rate observed in that bin.

The returned table can be plotted directly in Excel by using `MeanPredicted` on the x-axis and `ObservedRate`
on the y-axis, optionally with the confidence limits as error bars for the observed rate.

Example:
`=BESH.CLASS.CALIB(A2:A101,B2:B101)`
or
`=BESH.CLASS.CALIB(A2:A101,B2:B101,10,"quantile",,TRUE)`

## BESH.CLASS.CONFUSION

Returns a threshold-based confusion-matrix report for observed binary outcomes and predicted probabilities.

**Function wizard:** Returns a threshold-based confusion-matrix report for observed binary outcomes and predicted probabilities.

### Syntax

`=BESH.CLASS.CONFUSION(y, probabilities, threshold, weights, includeHeader)`

### Parameters

- **y** — Observed binary outcomes coded as `0` and `1`.
Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
- **probabilities** — Predicted event probabilities corresponding row-by-row to `y`.
Supply a single worksheet row or single worksheet column. Values must lie in `[0,1]`.
A leading text header is allowed and is ignored.
- **threshold** — Optional single classification cutoff in `[0,1]`. The default is `0.5`.
The predicted class is defined by `ŷ_i = 1` when `p_i ≥ threshold`.
- **weights** — Optional nonnegative case weights aligned with `y` and `probabilities`.
Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
- **includeHeader** — TRUE to include a descriptive header row in the returned table (default TRUE).

### Returns

A worksheet table containing the 2×2 confusion matrix and selected threshold-based summary measures.
The layout mirrors the classifier-report output used by the fitted-model UDFs so that generic and handle-based
reports can be compared directly.

### Notes

This function is useful when predicted probabilities are already available in the worksheet and only the reporting
layer is needed. Typical use cases include scoring a holdout set, evaluating external validation data, or reusing
probabilities generated earlier by `BESH.REGR.GLM_PRED` or `BESH.REGR.GEE_PRED`.

The report is threshold-dependent. Changing `threshold` changes the predicted classes and therefore
the resulting confusion counts and derived measures. For a threshold sweep over many cutoffs, use
`BESH.CLASS.THRESH`.

Example:
`=BESH.CLASS.CONFUSION(A2:A101,B2:B101)`
or
`=BESH.CLASS.CONFUSION(A2:A101,B2:B101,0.35,C2:C101,TRUE)`

## BESH.CLASS.THRESH

Returns a threshold-performance table for observed binary outcomes and predicted probabilities.

**Function wizard:** Returns a threshold-performance table for observed binary outcomes and predicted probabilities.

### Syntax

`=BESH.CLASS.THRESH(y, probabilities, thresholds, weights, includeHeader)`

### Parameters

- **y** — Observed binary outcomes coded as `0` and `1`.
Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
- **probabilities** — Predicted event probabilities corresponding row-by-row to `y`.
Supply a single worksheet row or single worksheet column. Values must lie in `[0,1]`.
A leading text header is allowed and is ignored.
- **thresholds** — Optional scalar or vector of one or more thresholds in `[0,1]`.
Supply a single worksheet row, a single worksheet column, or a scalar.
If omitted, the default threshold grid is built from the sorted unique predicted probabilities.
- **weights** — Optional nonnegative case weights aligned with `y` and `probabilities`.
Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per threshold and the columns:
threshold, TP, FP, TN, FN, sensitivity, specificity, precision, recall, NPV,
accuracy, balanced accuracy, Youden's J, and F1.
Percentage-based measures are returned on the 0–100 scale.

### Notes

This function evaluates classifier performance over many candidate decision thresholds.
It is useful for selecting an operating point after probabilities have been produced, for comparing cutoffs,
or for complementing ROC-based summaries with concrete confusion counts and predictive values.

When `thresholds` is omitted, the function uses the sorted unique predicted probabilities.
This gives an exact threshold sweep over the observed fitted values rather than only a coarse fixed grid.

Example:
`=BESH.CLASS.THRESH(A2:A101,B2:B101)`
or
`=BESH.CLASS.THRESH(A2:A101,B2:B101,{0.1,0.2,0.3,0.4,0.5},,TRUE)`

## BESH.REGR.FORMULA_VALIDATE

Validates a regression-model formula string against the raw predictor matrix and returns TRUE when validation succeeds.

**Function wizard:** Validates a regression-model formula string and returns TRUE or a descriptive validation message.

### Syntax

`=BESH.REGR.FORMULA_VALIDATE(formula, x, varNames, formulaAddressing)`

### Parameters

- **formula** — The right-hand-side model formula to validate.
Supported syntax currently includes additive terms (`A + B`), polynomial terms (`A^2`),
continuous-continuous interactions (`A:B`, `A:B:C`), categorical main effects such as
`factor(C)` or `factor(C, ref=2)`, categorical-continuous interactions such as
`factor(C):A` or `factor(C, ref=1):A`, and categorical-categorical interactions such as
`factor(C):factor(D)`.
Blank text is considered valid and corresponds to the default design that uses all predictor columns as continuous main effects.
- **x** — The raw predictor matrix that would be supplied to the corresponding regression fit UDF.
The validator uses this matrix to determine the number of raw predictors and, when needed, the absolute worksheet column letters.
- **varNames** — Optional raw predictor names.
This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
These names are used when `formulaAddressing` is set to `names`.
- **formulaAddressing** — Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
Accepted values are `relative` (default), `absolute`, and `names`.
In `relative` mode, bare letters such as `A` and `B` refer to columns 1 and 2 of `x`.
In `absolute` mode, bare letters refer to worksheet columns of the supplied `x` range.
In `names` mode, bare letters are disabled and variables should be referenced using single-quoted names such as `'dose'`.
Single quotes inside names are escaped by doubling them, e.g. `'Children''s dose'`.

### Returns

TRUE when validation succeeds.
If validation fails, returns a descriptive text message that includes the parser or design-build error,
a best-effort indication of the offending fragment, and context about the active addressing mode and available predictor references.

### Notes

This function validates formulas by using the same parser and design-matrix infrastructure that the supported regression fit UDFs use internally.
As a result, a formula that returns TRUE here is expected to satisfy the formula grammar and addressing rules during model fitting as well,
provided that the same `x`, `varNames`, and `formulaAddressing` inputs are used.

The formula grammar supports interactions involving `factor(...)`.  Polynomial subterms inside interactions
and repeated variables inside one interaction term remain unsupported; write polynomial terms separately and then
interact the raw variables only when needed.

When `formulaAddressing="absolute"` is used, the `x` argument must be passed as a direct worksheet range so that the validator
can determine the absolute worksheet column letters that are available to the formula.

### Example

```

=BESH.REGR.FORMULA_VALIDATE("A + A^2 + factor(C, ref=1) + B:D", C2:F101, "prison,dose,stage,treat")
=BESH.REGR.FORMULA_VALIDATE("factor(stage, ref=1) + dose + factor(stage, ref=1):dose + factor(stage):factor(treat)", C2:F101, "prison,dose,stage,treat", "names")
=BESH.REGR.FORMULA_VALIDATE("factor(E, ref=1):C", C2:F101, "prison,dose,stage,treat", "absolute")
```

## BESH.REGR.GEE_BRIER

Returns the Brier score for a fitted binomial generalized estimating equation model.

**Function wizard:** Returns the Brier score for a fitted binomial generalized estimating equation model.

### Syntax

`=BESH.REGR.GEE_BRIER(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT` for a fitted binomial generalized estimating equation model.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A small worksheet table containing the Brier score and, for convenience, the sample size and event rate.

### Notes

For observed binary outcomes `y_i` and fitted marginal probabilities `p_i`, the Brier score is the mean squared probability error.
In the unweighted case it is `(1/n) Σ (y_i - p_i)^2`; when observation weights are present, the corresponding weighted mean is returned.

This summary is threshold-free and complements the threshold-based reports returned by `BESH.REGR.GEE_CLASS` and `BESH.REGR.GEE_THRESH`.

Example:
`=BESH.REGR.GEE_BRIER(A1)`

## BESH.REGR.GEE_CALIB

Returns calibration-plot data for a fitted binomial generalized estimating equation model.

**Function wizard:** Returns calibration-plot data for a fitted binomial generalized estimating equation model.

### Syntax

`=BESH.REGR.GEE_CALIB(handle, bins, method, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT` for a fitted binomial generalized estimating equation model.
- **bins** — Optional positive integer giving the number of calibration bins.
The default is 10. The current implementation requires at least 2 bins.
- **method** — Optional calibration binning method.
Accepted values are `"quantile"` (default) and `"equalwidth"`.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per calibration bin and the columns:
bin index, number of observations, mean predicted probability, observed event rate,
and lower/upper confidence limits for the observed event rate.

### Notes

This function is designed to support calibration plots for fitted binary GEE models.
It groups observations by fitted marginal probability and compares mean predicted probabilities with observed event rates.

The returned table can be plotted directly in Excel by using `MeanPredicted` on the x-axis and `ObservedRate` on the y-axis.

Example:
`=BESH.REGR.GEE_CALIB(A1)`
or
`=BESH.REGR.GEE_CALIB(A1,10,"quantile",TRUE)`

## BESH.REGR.GEE_CLASS

Returns a threshold-based classification report for a fitted binomial generalized estimating equation model.

**Function wizard:** Returns a threshold-based classification report for a fitted binomial generalized estimating equation model.

### Syntax

`=BESH.REGR.GEE_CLASS(handle, threshold, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT` for a previously fitted generalized estimating equation model.
The handle must refer to a binomial GEE because the report is based on fitted event probabilities.
- **threshold** — Optional single classification cutoff in the closed interval `[0,1]`.
The default is `0.5`.
Observations with fitted probability `p_i ≥ threshold` are classified as predicted positives and
observations with `p_i < threshold` are classified as predicted negatives.
- **includeHeader** — TRUE to include a descriptive header row in the returned table (default TRUE).

### Returns

A 4-column worksheet table containing a binary confusion matrix and selected summary measures.
The returned layout mirrors `BESH.REGR.GLM_CLASS` so that GLM and GEE classifier outputs are directly comparable.

### Notes

This function is intended for fitted binary-response GEE models. It uses the model's fitted marginal means
`μ_i` as estimated event probabilities and compares them with the observed binary outcomes `y_i ∈ {0,1}`.

Because GEE is a marginal modeling framework, the returned classification summaries should be interpreted as summaries of the fitted
marginal probabilities, not as subject-specific random-effects predictions.

For a chosen threshold `c`, the predicted class is defined by `ŷ_i = 1` when `p_i ≥ c` and `ŷ_i = 0` otherwise.
The function then derives confusion-matrix counts and the associated threshold-based metrics.

Example:
`=BESH.REGR.GEE_CLASS(A1)`
or
`=BESH.REGR.GEE_CLASS(A1,0.40,TRUE)`

## BESH.REGR.GEE_DROP

Removes a fitted generalized estimating equation handle from the in-memory cache.

**Function wizard:** Removes a fitted generalized estimating equation handle from memory.

### Syntax

`=BESH.REGR.GEE_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.

### Returns

TRUE if the handle was found and removed; otherwise FALSE.

### Notes

Handles persist only for the current Excel session and reference fitted models stored in memory.
This function explicitly releases one cached model so that repeated refits do not keep unnecessary objects alive.
Existing worksheet formulas that still reference a dropped handle will subsequently return a handle-not-found error until the model is refitted.

## BESH.REGR.GEE_FIT

Fits a generalized estimating equation model and returns a reusable handle.

**Function wizard:** Fits a generalized estimating equation model and returns a reusable handle.

### Syntax

`=BESH.REGR.GEE_FIT(y, x, clusterId, time, varNames, family, link, covariance, stdErrType, offset, weights, formula, formulaAddressing, dispersion, power, maxIter, tol, alpha, useP, startParams)`

### Parameters

- **y** — Numeric response vector (single column) with one observation per row.
Typical uses include repeated binary outcomes, repeated counts, and continuous outcomes observed within clusters.
- **x** — Raw predictor matrix with one row per observation.
Rows must align with `y`, `clusterId`, and the optional time, offset, and weight inputs.
- **clusterId** — Cluster or subject identifier (single column).
Observations with the same identifier are treated as belonging to the same marginal-response cluster.
The identifier may be numeric or text.
- **time** — Optional within-cluster ordering variable (single column).
When supplied, observations are ordered within each cluster by this variable before fitting.
When omitted, the current row order within each cluster is used.
- **varNames** — Optional raw predictor names supplied as a comma-separated list or as a one-row/one-column range.
These names are used by the formula parser and by the returned coefficient table.
- **family** — Response family for the marginal variance structure.
Accepted values include `binomial` (default), `poisson`, `negative binomial`/`nb`, `gaussian`, and `gamma`.
Representative variance functions are
`μ(1-μ)` for Binomial,
`μ` for Poisson,
`μ + α μ²` for Negative Binomial,
constant variance for Gaussian, and
`φ μ²` for Gamma-type modeling.
- **link** — Optional link function `g(·)` in `g(μ_ij)=η_ij`.
If omitted, the family's canonical or default link is used.
Accepted values include `logit`, `probit`, `log`, `identity`, `sqrt`, `inverse`, and `power` when compatible with the chosen family.
- **covariance** — Working-correlation structure.
Accepted values include `independence` (default), `exchangeable`, `autoregressive`/`ar1`, and `unstructured`.
The working structure affects efficiency and covariance estimation but not the interpretation of the mean model itself.
- **stdErrType** — Covariance estimator used for coefficient standard errors.
Accepted values are `robust` (default), `naive`, and `bias reduced`.
The robust option returns the sandwich covariance
`B^{-1} C B^{-1}`,
while the naive option returns the model-based covariance
`φ B^{-1}`.
- **offset** — Optional numeric offset vector (single column).
The offset enters additively on the link scale:
`η_ij = β_0 + x_ij'β + o_ij`.
Under a log link this is commonly used for log-exposure or log-person-time adjustment.
- **weights** — Optional nonnegative case weights (single column).
These weights enter the mean-estimating equations and residual calculations in the same row order as the response.
- **formula** — Optional right-hand-side formula used to expand the raw predictor matrix before fitting.
If omitted or blank, all raw predictor columns are included as continuous main effects.
Formula expansion can create transformed terms, continuous-continuous interactions, categorical indicators, categorical-continuous interactions, and categorical-categorical interactions while preserving a consistent design for prediction.
- **formulaAddressing** — Formula-addressing mode: `relative` (default), `absolute`, or `names`.
This controls whether formula tokens refer to columns by relative worksheet letters, absolute worksheet letters, or supplied variable names.
- **dispersion** — Optional fixed NB2 dispersion parameter used only when `family` is Negative Binomial.
In that parameterization the marginal variance is `μ + α μ²`, so this argument supplies the value of `α`.
- **power** — Optional power parameter used only when `link` is `power`.
- **maxIter** — Maximum number of mean/correlation updating iterations (default 20).
- **tol** — Positive convergence tolerance for successive parameter updates (default 1E-8).
- **alpha** — Two-sided significance level used for confidence intervals stored with the fitted result (default 0.05).
This affects inferential reporting only and does not change the fitted coefficients.
- **useP** — Optional logical flag controlling the denominator adjustments used in scale and association-parameter updates.
When TRUE, the fitting routine applies parameter-count adjustments analogous to small-sample corrections used in some GEE software.
- **startParams** — Optional starting values for the mean-model coefficients, supplied as a one-row/one-column range or a comma/space-separated text list.
The intercept starting value must be first, followed by the predictor coefficients in the expanded design-matrix order.

### Returns

A text handle identifying the fitted model within the current Excel session.
The handle can be passed to the associated summary, diagnostics, residual, prediction, and cleanup worksheet functions without refitting.

### Notes

This function fits the population-averaged model defined by
`g(μ_ij)=β_0+x_ij'β+o_ij`
together with the estimating equations
`Σ_i D_i'V_i^{-1}(y_i-μ_i)=0`.
The family determines `A_i`, the working-correlation structure determines `R_i(α)`, and the selected covariance type determines how standard errors are reported.

Estimation alternates between updating the mean coefficients using a Fisher-scoring-style linear solve
and updating the working association parameters from the current standardized residual pattern.
Convergence is judged from the largest absolute or relative coefficient change across iterations.

The returned coefficients are marginal, not cluster-specific.
For example, under a Binomial-logit GEE, exponentiating a slope yields a marginal odds ratio;
under a log link, exponentiating a slope yields a multiplicative effect on the marginal mean.

Rows containing invalid or non-finite values in the response, predictors, time variable, offset, or weights are removed before fitting.
Clusters are then sorted internally, and observations within each cluster are ordered by the supplied time variable when one is provided.

If `formulaAddressing="absolute"` is used, the predictor argument should be a direct worksheet range so absolute worksheet column letters can be resolved.

### Example

```

=BESH.REGR.GEE_FIT(A2:A101,B2:D101,E2:E101)
=BESH.REGR.GEE_FIT(A2:A101,B2:E101,F2:F101,G2:G101,"Age,BMI,Treat,Visit","binomial","logit","exchangeable","robust")
=BESH.REGR.GEE_FIT(A2:A101,B2:D101,E2:E101,,"Dose,Age,Stage","poisson","log","ar1","robust",H2:H101,,"A + B + factor(C) + factor(C):B")
```

## BESH.REGR.GEE_PRED

Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.

**Function wizard:** Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.

### Syntax

`=BESH.REGR.GEE_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **newX** — New raw predictor matrix in the same raw-column order used at fitting time.
When the fitted model contains transformed terms, interactions, or categorical encodings, those derived columns are rebuilt automatically from this raw matrix using the original model specification.
- **newOffset** — Optional offset vector for the new observations.
It is required when the fitted model used an offset and enters additively on the linear-predictor scale.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A two-column table containing the predicted marginal mean response `μ̂_i` and the linear predictor `η̂_i` for each supplied observation.

### Notes

For new observations the worksheet function reconstructs the expanded design columns from the stored predictor specification,
then evaluates
`η̂_i = β̂_0 + x_i'β̂ + o_i`
and
`μ̂_i = g^{-1}(η̂_i)`.
The returned mean is therefore on the natural response scale, while the second column remains on the link scale.

The prediction is marginal with respect to the working-correlation structure.
The cluster identifier and working covariance affect estimation efficiency and inference, but the fitted mean at a new covariate pattern is determined by the estimated regression coefficients and the chosen link.

Intercept-only models can be predicted without supplying `newX`.
In that case, a single prediction row is returned unless a new offset vector is supplied, in which case one prediction is returned for each offset value.

## BESH.REGR.GEE_RESID

Returns residual diagnostics for a fitted generalized estimating equation handle.

**Function wizard:** Returns residual diagnostics for a fitted generalized estimating equation handle.

### Syntax

`=BESH.REGR.GEE_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **residType** — Residual block to return: `all` (default), `raw`, `deviance`, `pearson`, `stdpearson`, `stddeviance`, or `working`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

Either a single residual column or a multi-column diagnostic table, depending on `residType`.

### Notes

The returned residuals are marginal residual diagnostics built from the fitted mean model.
The raw residual is
`r_ij = y_ij - μ̂_ij`.
The Pearson residual rescales by the model-implied marginal standard deviation,
approximately
`r^P_ij = (y_ij - μ̂_ij) / sqrt(V(μ̂_ij))`.

The deviance residual is the signed square root of the observation-wise deviance contribution,
and the working residual is
`(y_ij - μ̂_ij) / (dμ_ij/dη_ij)`.
Scaled Pearson and scaled deviance residuals divide by `sqrt(φ)`, where `φ` is the fitted scale parameter.

These residuals diagnose the mean specification rather than the adequacy of the working-correlation structure itself.

## BESH.REGR.GEE_SUMMARY

Returns the coefficient summary table for a fitted generalized estimating equation handle.

**Function wizard:** Returns the coefficient summary table for a fitted generalized estimating equation handle.

### Syntax

`=BESH.REGR.GEE_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided alpha for the displayed confidence intervals.

### Returns

A rectangular coefficient table with one row per estimated mean-model parameter.

### Notes

The coefficient table is reported on the link scale.
For each parameter the function returns the estimate `β̂`, its selected standard error,
the Wald statistic `Z = β̂ / SE(β̂)`, the associated large-sample two-sided p-value,
and a two-sided confidence interval of the form
`β̂ ± z_{1-α/2} SE(β̂)`.

The standard-error column reflects the covariance estimator chosen at fit time:
model-based, robust sandwich, or bias-reduced sandwich.
This affects inference but not the coefficient estimates themselves.

No exponentiation is applied automatically.
When a marginal odds ratio or marginal rate ratio is desired, users can exponentiate the returned coefficients and confidence limits externally.

## BESH.REGR.GEE_TESTS

Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.

**Function wizard:** Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.

### Syntax

`=BESH.REGR.GEE_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A table of model-level statistics such as family, link, working correlation structure,
numbers of observations and clusters, cluster-size summaries, scale, quasi-information criteria,
iteration counts, convergence indicators, and computational time.

### Notes

Because GEE is based on estimating equations rather than a full likelihood in the general correlated-data setting,
model comparison is commonly summarized by quasi-likelihood information criteria rather than standard likelihood-ratio tests.
The reported QIC and QICu values are based on the fitted quasi-likelihood and the selected covariance structure.

The scale row summarizes the estimated overdispersion or residual scale parameter `φ`.
The cluster-size rows describe the replication pattern that underlies the sandwich covariance and working-correlation updates.

The convergence rows report the last relative coefficient-change criterion and whether the stopping rule was met before the iteration limit.

## BESH.REGR.GEE_THRESH

Returns a threshold table for a fitted binomial generalized estimating equation model.

**Function wizard:** Returns a threshold table for a fitted binomial generalized estimating equation model.

### Syntax

`=BESH.REGR.GEE_THRESH(handle, thresholds, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT` for a fitted binomial generalized estimating equation model.
- **thresholds** — Optional vector of one or more thresholds in `[0,1]` supplied as a row range, column range, or a single scalar.
If omitted, the function builds a default threshold grid from the unique fitted probabilities generated by the model.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per threshold and the columns:
threshold, TP, FP, TN, FN, sensitivity, specificity, precision, recall, NPV,
accuracy, balanced accuracy, Youden's J, and F1.

### Notes

This function evaluates a fitted binomial GEE across a sequence of decision thresholds using the model's fitted marginal probabilities.
It is useful for comparing threshold-dependent operating points after a GEE has been fitted.

When `thresholds` is omitted, the function uses the sorted unique fitted probabilities as the threshold grid.

Example:
`=BESH.REGR.GEE_THRESH(A1)`
or
`=BESH.REGR.GEE_THRESH(A1,{0.25,0.50,0.75},TRUE)`

## BESH.REGR.GEE_VCOV

Returns the covariance matrix of the estimated generalized estimating equation coefficients.

**Function wizard:** Returns the covariance matrix of the estimated generalized estimating equation coefficients.

### Syntax

`=BESH.REGR.GEE_VCOV(handle, covarianceType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **covarianceType** — Covariance estimator to return.
Accepted values are `robust`, `naive`, and `bias reduced`.
If omitted, the covariance type selected at fit time is used.
- **includeHeader** — TRUE to include row and column labels (default TRUE).

### Returns

A square parameter-covariance matrix whose diagonal entries are coefficient variances
and whose off-diagonal entries are coefficient covariances.

### Notes

Let
`B = Σ_i D_i' V_i^{-1} D_i`
and
`u_i = D_i' V_i^{-1} (y_i - μ_i)`.
Then the main covariance estimators reported for generalized estimating equations are:

Naive / model-based:
`Var_naive(β̂) = φ B^{-1}`

Robust / empirical sandwich:
`Var_robust(β̂) = B^{-1} (Σ_i u_i u_i') B^{-1}`

Bias-reduced sandwich:
a leverage-adjusted sandwich estimator intended to improve finite-cluster performance when the ordinary robust covariance is downward biased.

This function returns the full matrix rather than only the standard errors.
Therefore:
the square root of each diagonal entry equals the corresponding coefficient standard error,
and the off-diagonal terms quantify the joint sampling dependence between coefficient estimators.

The matrix is on the linear-predictor coefficient scale.
For example, under a log link it is the covariance of log-rate or log-mean parameters,
and under a logit link it is the covariance of marginal log-odds parameters.

## BESH.REGR.GEE_WCORR

Returns the fitted working correlation matrix for a generalized estimating equation handle.

**Function wizard:** Returns the fitted working correlation matrix for a generalized estimating equation handle.

### Syntax

`=BESH.REGR.GEE_WCORR(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GEE_FIT`.
- **includeHeader** — TRUE to include row and column labels (default TRUE).

### Returns

A square matrix representing the fitted working correlation structure used inside the marginal estimating equations.

### Notes

Generalized estimating equations model the within-cluster covariance through the decomposition
`V_i = φ A_i^{1/2} R_i(α) A_i^{1/2}`,
where `R_i(α)` is the working correlation matrix.
This worksheet function returns that fitted correlation matrix `R_i(α)`.
It is the association structure used by the algorithm, not the empirical sample correlation matrix of the observed responses.

The interpretation depends on the selected working structure:
independence returns an identity matrix,
exchangeable returns a matrix with common off-diagonal correlation,
autoregressive returns a banded-decay structure with entries of the form `ρ^{|t-s|}`,
and unstructured returns a fully estimated symmetric correlation matrix.

When within-cluster time was supplied at fitting, the row and column labels correspond to the ordered time values used internally by the model.
Otherwise a generic sequential labeling is returned.

## BESH.REGR.GLMNB_DROP

Removes a fitted Negative Binomial regression handle from the in-memory cache.

**Function wizard:** Removes a fitted Negative Binomial regression handle from memory.

### Syntax

`=BESH.REGR.GLMNB_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLMNB_FIT`.

### Returns

TRUE when the handle was found and removed; otherwise FALSE.

### Notes

Handles are session-scoped identifiers for cached fitted models.
Removing a handle frees the corresponding in-memory model object for the current Excel session and invalidates subsequent lookups using that handle.

## BESH.REGR.GLMNB_FIT

Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.

**Function wizard:** Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.

### Syntax

`=BESH.REGR.GLMNB_FIT(y, x, varNames, link, offset, weights, includeIntercept, formula, formulaAddressing, power, maxIter, tol, alpha)`

### Parameters

- **y** — Numeric response vector (single column) containing nonnegative count outcomes.
Each row corresponds to one observation.
- **x** — Raw predictor matrix with one row per observation and one column per raw predictor.
The rows must align with `y`, `offset`, and `weights` when supplied.
- **varNames** — Optional raw predictor names supplied either as a comma-separated string or as a one-row/one-column range.
If omitted, fallback names such as X1, X2, … are assigned internally.
- **link** — Optional link function for the mean model.
The default is `log`, which yields `μ_i = exp(η_i)`.
Accepted values follow the underlying Negative Binomial family implementation and include `log`, `identity`, and `power`.
- **offset** — Optional numeric offset vector (single column).
The offset enters additively on the link scale:
`η_i = β_0 + x_i'β + o_i`.
For rate models with exposure `t_i`, a common choice under the log link is `o_i = log(t_i)`.
- **weights** — Optional nonnegative case weights (single column).
These weights are passed into the fitting engine and act multiplicatively in the IRLS working weights and in the dispersion update objective.
- **includeIntercept** — TRUE to include an intercept term (default TRUE).
When FALSE, the fitted linear predictor omits `β_0`.
- **formula** — Optional right-hand-side formula used to expand the raw predictor matrix before fitting. Supported terms include additive effects, polynomial terms, `A:B`, `factor(C)`, `factor(C):B`, and `factor(C):factor(D)`.
If omitted or blank, all raw predictor columns are included as continuous main effects.
- **formulaAddressing** — Formula-addressing mode: `relative` (default), `absolute`, or `names`.
This controls how bare column tokens are interpreted inside `formula`.
- **power** — Optional power parameter used only when `link` is `power`.
If the power link is selected, this parameter is required and must be finite and nonzero.
- **maxIter** — Maximum number of fitting iterations for the outer alternating optimization procedure (default 20).
- **tol** — Positive convergence tolerance for the alternating optimization procedure (default 1E-8).
- **alpha** — Two-sided significance level used for confidence intervals stored in the fitted result object (default 0.05).
This parameter does not control the Negative Binomial dispersion; it controls reporting intervals only.

### Returns

A text handle identifying the fitted Negative Binomial model within the current Excel session.
The handle can be passed to the other `GLMNB_*` worksheet functions to retrieve summaries, tests, residuals, and predictions without refitting.

### Notes

This function fits the NB2 Negative Binomial regression model
`Y_i | x_i ~ NB(μ_i, α)`
with
`E[Y_i | x_i] = μ_i`
and
`Var(Y_i | x_i) = μ_i + α μ_i^2`.
Under the default log link,
`log(μ_i) = β_0 + x_i'β + o_i`,
so exponentiated coefficients represent multiplicative effects on the conditional mean count.

The underlying GLM_NB implementation uses an alternating procedure:

- Fit an initial Poisson GLM to obtain starting mean-model coefficients and fitted means.
- Estimate an initial overdispersion value.
- Repeatedly refit the Negative Binomial mean model for the current dispersion and then update the dispersion from the current fitted means.

Internally the model reports overdispersion in the NB2 parameterization `α`, while some software instead reports
`θ = 1/α`. Both quantities are made available through the test/diagnostic output returned by `BESH.REGR.GLMNB_TESTS`.

Rows with invalid values in the response, predictors, offset, or weights are excluded before fitting.
If too few valid rows remain, or if the resulting design has no estimable parameters, the function returns an Excel error.

If `formulaAddressing="absolute"` is used, the `x` argument should be supplied as a direct worksheet range so that absolute worksheet column letters can be resolved.

### Example

```

=BESH.REGR.GLMNB_FIT(A2:A101,B2:D101,"Age,BMI,Treat")
=BESH.REGR.GLMNB_FIT(A2:A101,B2:E101,"Dose,Age,Stage,Center","log",F2:F101,,TRUE,"A + B + factor(D) + factor(D):B","relative")
=BESH.REGR.GLMNB_FIT(A2:A101,B2:C101,"X1,X2","power",,,,TRUE,,,0.5)
```

## BESH.REGR.GLMNB_PRED

Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.

**Function wizard:** Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.

### Syntax

`=BESH.REGR.GLMNB_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLMNB_FIT`.
- **newX** — New raw predictor matrix in the same raw-column order used at fitting time.
If the fitted model used a formula, the same stored formula design is reapplied to this raw matrix.
- **newOffset** — Optional offset vector for the new observations.
If the fitted model included an offset, this argument is required and is added on the link scale.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A two-column table containing the predicted conditional mean response and the corresponding linear predictor.

### Notes

For each new row, the worksheet function reconstructs the fitted design columns, evaluates the linear predictor
`η_new = β_0 + x_new'β + o_new`, and then returns the mean prediction
`μ_new = g^{-1}(η_new)`.

Under the default log link, the output therefore satisfies
`μ_new = exp(η_new)`,
which is the fitted conditional mean count for the supplied covariate pattern and offset.

## BESH.REGR.GLMNB_RESID

Returns residual diagnostics for a fitted Negative Binomial regression handle.

**Function wizard:** Returns residual diagnostics for a fitted Negative Binomial regression handle.

### Syntax

`=BESH.REGR.GLMNB_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLMNB_FIT`.
- **residType** — Residual block to return: `all` (default), `raw`, `deviance`, `pearson`,
`stdpearson`, `stddeviance`, `leverage`, or `cook`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A residual matrix or vector, depending on `residType`.

### Notes

Residual diagnostics are based on the fitted Negative Binomial mean model and include several commonly used quantities.
The raw residual is `y_i - μ_i`.
The Pearson residual rescales that difference by the model-implied standard deviation,
while the deviance residual is based on the signed square root of the per-observation deviance contribution.

Standardized residuals adjust for leverage, and the leverage/Cook's-distance columns are useful for influence screening.
The `all` option returns the full seven-column block used by the internal GLM diagnostics.

## BESH.REGR.GLMNB_SUMMARY

Returns the coefficient summary table for a fitted Negative Binomial regression handle.

**Function wizard:** Returns the coefficient summary table for a fitted Negative Binomial regression handle.

### Syntax

`=BESH.REGR.GLMNB_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLMNB_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided significance level used to construct the displayed Wald confidence intervals.
This argument controls only interval reporting and does not refit the model.

### Returns

A rectangular coefficient table containing parameter labels, standard errors, Wald z statistics, p-values, and confidence limits.

### Notes

The coefficient table is built from the fitted mean-model parameters `β` and their standard errors.
For coefficient `β_j` with standard error `SE(β_j)`, the worksheet output reports the Wald statistic
`z_j = β_j / SE(β_j)` and the two-sided p-value
`2 Φ(-|z_j|)`, where `Φ` is the standard normal CDF.

A `(1-α)` Wald confidence interval is displayed as
`β_j ± z_{1-α/2} SE(β_j)`.
Under the log link, exponentiating a slope coefficient yields the estimated multiplicative change in the conditional mean count for a one-unit increase in the predictor, holding other predictors fixed.

## BESH.REGR.GLMNB_TESTS

Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.

**Function wizard:** Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.

### Syntax

`=BESH.REGR.GLMNB_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLMNB_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A rectangular table containing family/link information, deviance diagnostics, information criteria, convergence information,
the estimated NB2 dispersion parameter `α`, its reciprocal `θ = 1/α`, and computation metadata.

### Notes

The returned table is based primarily on the model summary table produced by GLM_NB after fitting.
In addition, this worksheet function explicitly reports the NB2 overdispersion estimate `α` and the reciprocal form
`θ = 1/α`, because different software packages report one or the other.

The NB2 variance function is
`V(μ) = μ + α μ^2`.
When `α` is close to 0, the model approaches a Poisson mean-variance relationship.
Larger values of `α` indicate stronger overdispersion relative to Poisson.

Information-criterion rows already account for the estimated dispersion parameter inside the underlying GLM_NB implementation.

## BESH.REGR.GLM_BRIER

Returns the Brier score for a fitted binomial generalized linear model.

**Function wizard:** Returns the Brier score for a fitted binomial generalized linear model.

### Syntax

`=BESH.REGR.GLM_BRIER(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT` for a fitted binomial generalized linear model.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A small worksheet table containing the Brier score and, for convenience, the sample size and event rate.

### Notes

For a binary outcome with observed values `y_i ∈ {0,1}` and fitted probabilities `p_i`, the Brier score is
`(1/n) Σ (y_i - p_i)^2` in the unweighted case, or the corresponding weighted mean when case weights were supplied.
Lower values indicate more accurate probabilistic predictions.

Unlike threshold-based metrics, the Brier score evaluates the full probability forecast and therefore does not depend on a chosen classification cutoff.
It combines aspects of calibration and discrimination into a single proper scoring rule.

Example:
`=BESH.REGR.GLM_BRIER(A1)`

## BESH.REGR.GLM_CALIB

Returns calibration-plot data for a fitted binomial generalized linear model.

**Function wizard:** Returns calibration-plot data for a fitted binomial generalized linear model.

### Syntax

`=BESH.REGR.GLM_CALIB(handle, bins, method, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT` for a fitted binomial generalized linear model.
- **bins** — Optional positive integer giving the number of calibration bins.
The default is 10. The current implementation requires at least 2 bins.
- **method** — Optional calibration binning method.
Accepted values are `"quantile"` (default) and `"equalwidth"`.
Quantile binning creates groups with approximately equal numbers of observations, while equal-width binning
partitions the probability scale into equal intervals.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per calibration bin and the columns:
bin index, number of observations, mean predicted probability, observed event rate,
and lower/upper confidence limits for the observed event rate.

### Notes

This function is designed to support calibration plots for fitted binary GLMs.
A well-calibrated model should produce bins in which the observed event rate is close to the mean predicted probability.

The returned table can be plotted directly in Excel by using `MeanPredicted` on the x-axis and `ObservedRate` on the y-axis,
optionally adding the confidence interval columns as error bars.

Calibration describes how well predicted probabilities agree with empirical event frequencies.
It is different from discrimination measures such as AUC or from threshold-based summaries such as sensitivity and specificity.

Example:
`=BESH.REGR.GLM_CALIB(A1)`
or
`=BESH.REGR.GLM_CALIB(A1,10,"quantile",TRUE)`

## BESH.REGR.GLM_CLASS

Returns a threshold-based classification report for a fitted binomial generalized linear model.

**Function wizard:** Returns a threshold-based classification report for a fitted binomial generalized linear model.

### Syntax

`=BESH.REGR.GLM_CLASS(handle, threshold, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT` for a previously fitted generalized linear model.
The handle must refer to a binomial GLM because the report is based on fitted event probabilities.
- **threshold** — Optional single classification cutoff in the closed interval `[0,1]`.
The default is `0.5`.
Observations with fitted probability `p_i ≥ threshold` are classified as predicted positives and
observations with `p_i < threshold` are classified as predicted negatives.
- **includeHeader** — TRUE to include a descriptive header row in the returned table (default TRUE).

### Returns

A 4-column worksheet table containing a binary confusion matrix and selected summary measures.
The returned layout is:

- Observed vs predicted counts for classes 0 and 1.
- Specificity and sensitivity (reported as percentages).
- Negative predictive value, precision, and overall accuracy (reported as percentages).
- The chosen threshold, balanced accuracy, and Youden's J statistic.

### Notes

This function is intended for fitted binary-response GLMs such as logistic regression.
It uses the model's fitted mean responses `μ_i` as estimated event probabilities and compares them with
the observed binary outcomes `y_i ∈ {0,1}`.

For a chosen threshold `c`, the predicted class is defined by
`ŷ_i = 1` when `p_i ≥ c` and `ŷ_i = 0` otherwise. The function then computes the usual
confusion-matrix counts `TP`, `FP`, `TN`, and `FN`, from which it derives
sensitivity, specificity, precision, negative predictive value, accuracy, balanced accuracy,
and Youden's J statistic.

This is a threshold-dependent report. Changing `threshold` changes the predicted classes and therefore
the reported performance measures. For a threshold sweep across many cutoffs, use `BESH.REGR.GLM_THRESH`.

Example:
`=BESH.REGR.GLM_CLASS(A1)`
or
`=BESH.REGR.GLM_CLASS(A1,0.35,TRUE)`
where `A1` contains a handle returned by `BESH.REGR.GLM_FIT`.

## BESH.REGR.GLM_DROP

Removes a fitted generalized linear model handle from the in-memory cache.

**Function wizard:** Removes a fitted generalized linear model handle from memory.

### Syntax

`=BESH.REGR.GLM_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT`.

### Returns

TRUE if the handle was found and removed; otherwise FALSE.

### Notes

Handles persist only for the current Excel session and reference fitted models stored in memory.
This function explicitly releases one cached model so that long workbooks or repeated refits do not keep unnecessary objects alive.
Existing worksheet formulas that still reference a dropped handle will subsequently return a handle-not-found error until the model is refitted.

## BESH.REGR.GLM_FIT

Fits a generalized linear model and returns a reusable handle.

**Function wizard:** Fits a generalized linear model and returns a reusable handle.

### Syntax

`=BESH.REGR.GLM_FIT(y, x, varNames, family, link, offset, weights, includeIntercept, formula, formulaAddressing, dispersion, power, maxIter, tol, alpha)`

### Parameters

- **y** — Numeric response vector (single column).
Typical uses are continuous responses for Gaussian models, 0/1 outcomes for Binomial models,
nonnegative counts for Poisson or Negative Binomial models, and positive continuous responses for Gamma models.
Each row represents one observation.
- **x** — Raw predictor matrix with one row per observation.
Rows must align with `y`, `offset`, and `weights` whenever those inputs are supplied.
- **varNames** — Optional raw predictor names supplied as a comma-separated list or as a one-row/one-column range.
These names are used by the formula parser and by the returned coefficient table.
- **family** — Response family that determines the variance structure and likelihood contribution.
Accepted values include `gaussian`, `binomial`, `poisson`, `gamma`, and `negative binomial`/`nb`.
The default is `gaussian`.
Representative mean-variance relationships are
`Var(Y_i|x_i)=σ^2` for Gaussian,
`Var(Y_i|x_i)=μ_i(1-μ_i)` for Binomial,
`Var(Y_i|x_i)=μ_i` for Poisson,
`Var(Y_i|x_i)=φ μ_i^2` for Gamma, and
`Var(Y_i|x_i)=μ_i + α μ_i^2` for the fixed-dispersion Negative Binomial form.
- **link** — Optional link function `g(·)` used in `g(μ_i)=η_i`.
If omitted, the family's canonical or default link is used.
Accepted values include `logit`, `probit`, `log`, `identity`, `sqrt`, `inverse`, and `power`.
The link controls the interpretation of coefficients; for example, a log link yields multiplicative effects on the mean,
while a logit link yields additive effects on the log-odds scale.
- **offset** — Optional numeric offset vector (single column).
The offset enters additively on the linear-predictor scale:
`η_i = β_0 + x_i'β + o_i`.
Under a log link, a common choice for rate models is `o_i = log(t_i)`, where `t_i` is exposure or person-time.
- **weights** — Optional nonnegative case weights (single column).
These weights scale the contribution of each observation in the IRLS fitting equations and in the likelihood-based summaries.
- **includeIntercept** — TRUE to include an intercept term (default TRUE).
When FALSE, the fitted predictor is constrained to pass through the origin on the link scale.
- **formula** — Optional right-hand-side formula used to expand the raw predictor matrix before fitting.
If omitted or blank, all raw predictor columns are included as continuous main effects.
Formula expansion can create transformed terms, continuous-continuous interactions, categorical indicators, categorical-continuous interactions, and categorical-categorical interactions while preserving a consistent design for prediction.
- **formulaAddressing** — Formula-addressing mode: `relative` (default), `absolute`, or `names`.
This controls whether formula tokens refer to columns by relative worksheet letters, absolute worksheet letters, or supplied variable names.
- **dispersion** — Optional fixed dispersion parameter for the Negative Binomial family.
It is ignored by the other families.
In the NB2 parameterization the variance is `μ_i + α μ_i^2`, so this argument supplies the fixed value of `α`.
Larger values imply more overdispersion relative to the Poisson model.
- **power** — Optional power parameter used only when `link` is `power`.
For a power link, the transformation is controlled by this exponent, and the value must be finite and nonzero.
- **maxIter** — Maximum number of IRLS iterations (default 20).
Larger values can help difficult models converge but may increase calculation time.
- **tol** — Positive convergence tolerance for IRLS (default 1E-8).
Smaller values request a stricter convergence check on successive updates.
- **alpha** — Two-sided significance level used for confidence intervals stored with the fitted result (default 0.05).
This controls inferential reporting only; it does not change the fitted coefficients.

### Returns

A text handle identifying the fitted model within the current Excel session.
The handle can be passed to the associated summary, diagnostics, residual, prediction, and cleanup worksheet functions without refitting.

### Notes

This function fits the generalized linear model defined by
`g(μ_i)=β_0+x_i'β+o_i`,
where `μ_i=E[Y_i|x_i]` and the conditional variance is determined by the chosen family.
Under canonical links, the score equations take their standard exponential-family form and the IRLS updates correspond to Fisher scoring.

At each iteration, the algorithm forms the working response
`z_i = η_i + (y_i-μ_i)(dη_i/dμ_i)`
and solves a weighted least-squares update using working weights proportional to
`[(dμ_i/dη_i)^2 / Var(Y_i|x_i)]`.
User-supplied case weights multiply these working weights.
This procedure is the standard numerical method used to maximize the GLM log-likelihood or quasi-likelihood criterion.

Coefficients are reported on the link scale.
For example, under a log link, `exp(β_j)` is the multiplicative change in the fitted mean associated with a one-unit increase in predictor `x_j` holding other terms fixed.
Under a logit link, `exp(β_j)` is an odds ratio for a one-unit change in `x_j`.

Rows containing invalid or non-finite values in the response, predictors, offset, or weights are removed before fitting.
If too few valid observations remain or the design matrix becomes non-estimable, the function returns an Excel error instead of a handle.

If `formulaAddressing="absolute"` is used, the predictor argument should be a direct worksheet range so absolute worksheet column letters can be resolved.

### Example

```

=BESH.REGR.GLM_FIT(A2:A101,B2:D101,"Age,BMI,Treat","binomial","logit")
=BESH.REGR.GLM_FIT(A2:A101,B2:E101,"Dose,Age,Stage,Center","poisson","log",F2:F101,,TRUE,"A + B + factor(D) + factor(D):B","relative")
=BESH.REGR.GLM_FIT(A2:A101,B2:C101,"X1,X2","negative binomial","log",, ,TRUE,,,0.75)
```

## BESH.REGR.GLM_PRED

Returns predicted responses and linear predictors for new data under a fitted generalized linear model.

**Function wizard:** Returns predicted responses and linear predictors for new data under a fitted generalized linear model.

### Syntax

`=BESH.REGR.GLM_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT`.
- **newX** — New raw predictor matrix in the same raw-column order used at fitting time.
When the fitted model contains transformed terms, interactions, or categorical encodings, those derived columns are rebuilt automatically from this raw matrix using the original model specification.
- **newOffset** — Optional offset vector for the new observations.
It is required when the fitted model used an offset and enters additively on the linear-predictor scale.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A two-column table containing the predicted mean response `μ̂_i` and the linear predictor `η̂_i` for each supplied observation.

### Notes

Predictions are formed as
`η̂_i = β̂_0 + x_i'β̂ + o_i`
and
`μ̂_i = g^{-1}(η̂_i)`,
where `o_i` is the optional offset for the new observation.
The returned `PredictedResponse` column is therefore on the natural mean scale of the response,
while `LinearPredictor` remains on the link scale.

Under common links, this means the first column is a fitted probability for Binomial-logit models,
a fitted count or rate-scale mean for Poisson or Negative Binomial log-link models,
and a fitted mean outcome for Gaussian identity-link models.

Intercept-only models can be predicted without supplying `newX`.
In that case, a single prediction row is returned unless a new offset vector is supplied, in which case one prediction is returned for each offset value.

## BESH.REGR.GLM_RESID

Returns residual diagnostics for a fitted generalized linear model handle.

**Function wizard:** Returns residual diagnostics for a fitted generalized linear model handle.

### Syntax

`=BESH.REGR.GLM_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT`.
- **residType** — Residual block to return: `all` (default), `raw`, `deviance`, `pearson`, `stdpearson`,
`stddeviance`, `leverage`, or `cook`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

Either a single residual column or a multi-column diagnostic table, depending on `residType`.

### Notes

Residuals summarize different aspects of model misfit.
The raw or response residual is
`r_i = y_i - μ̂_i`.
The Pearson residual rescales this difference by the model-based standard deviation,
approximately
`r_{P,i} = (y_i-μ̂_i) / sqrt(Var(Y_i|x_i))`.

The deviance residual is the signed square root of the observation-wise contribution to model deviance,
`r_{D,i} = sign(y_i-μ̂_i) sqrt(d_i)`,
where `d_i` is the contribution to twice the log-likelihood ratio comparing the fitted model with the saturated model.
Deviance residuals are often more comparable across non-Gaussian families than raw residuals.

Standardized residuals account for leverage by dividing by approximately `sqrt(1-h_i)`,
where `h_i` is the diagonal element of the generalized hat matrix.
High leverage indicates observations with unusual predictor patterns, and Cook's distance combines residual size and leverage to measure potential influence on the fitted coefficients.

## BESH.REGR.GLM_SUMMARY

Returns the coefficient summary table for a fitted generalized linear model handle.

**Function wizard:** Returns the coefficient summary table for a fitted generalized linear model handle.

### Syntax

`=BESH.REGR.GLM_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided significance level used to form the displayed Wald confidence intervals.
If omitted, the confidence level stored with the fitted model is used.

### Returns

A table with one row per estimated parameter.
The columns contain the parameter name, parameter type, estimated coefficient, standard error,
Wald `z` statistic, two-sided p-value, and lower/upper confidence limits.

### Notes

The reported coefficients are on the link scale of the fitted model.
Thus the interpretation depends on the chosen link:
identity-link coefficients act directly on the mean,
log-link coefficients act on `log(μ)`, and
logit-link coefficients act on `log(μ/(1-μ))`.

Standard errors are derived from the estimated covariance matrix of the coefficients, and the table reports the Wald statistic
`z_j = β̂_j / SE(β̂_j)`
together with the usual two-sided large-sample p-value
`2 Φ(-|z_j|)`.
Confidence intervals are shown as
`β̂_j ± z_{1-α/2} SE(β̂_j)`.

No exponentiation is applied automatically.
When an odds-ratio or rate-ratio interpretation is desired, users can exponentiate the returned coefficients and confidence limits externally.

## BESH.REGR.GLM_TESTS

Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.

**Function wizard:** Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.

### Syntax

`=BESH.REGR.GLM_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A table of model-level statistics such as likelihood-based fit summaries, deviance measures,
information criteria, degrees of freedom, p-values where available, computational time, and any warnings.
The exact row set depends on the selected family and the fitted model output.

### Notes

Generalized linear models are commonly assessed with quantities such as residual deviance,
null deviance, likelihood-ratio style tests, Akaike information criterion (AIC), and associated degrees of freedom.
These are returned here in the order stored with the fitted model.
For many families, deviance is the summed contribution of the observation-wise log-likelihood ratio between the fitted model and the saturated model.

For Binomial models, this output also adds the numbers of observations with response greater than zero and equal to zero,
which helps document class balance in binary-response applications.

Any convergence messages or numerical warnings produced during fitting are returned as a final row so they can be surfaced in generated documentation or audit sheets.

## BESH.REGR.GLM_THRESH

Returns a threshold table for a fitted binomial generalized linear model.

**Function wizard:** Returns a threshold table for a fitted binomial generalized linear model.

### Syntax

`=BESH.REGR.GLM_THRESH(handle, thresholds, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.GLM_FIT` for a fitted binomial generalized linear model.
- **thresholds** — Optional vector of one or more thresholds in `[0,1]` supplied as a row range, column range, or a single scalar.
If omitted, the function builds a default threshold grid from the unique fitted probabilities generated by the model.
- **includeHeader** — TRUE to include a header row in the returned table (default TRUE).

### Returns

A worksheet table with one row per threshold and the following columns:
threshold, TP, FP, TN, FN, sensitivity, specificity, precision, recall, NPV,
accuracy, balanced accuracy, Youden's J, and F1.
Percentage-based metrics are returned on the 0-100 scale to match the style of the existing classification UDFs.

### Notes

This function evaluates the fitted binomial GLM across a sequence of classification cutoffs.
It is useful for selecting or comparing decision thresholds after the model has been fitted.

When `thresholds` is omitted, the function uses the sorted unique fitted probabilities as the threshold grid.
This provides an exact threshold sweep over the observed fitted values and is often more informative than using only a small fixed set of cutoffs.

The returned table complements ROC analysis. ROC summarizes discrimination across cutoffs, whereas this function gives the
concrete confusion counts and predictive values at each threshold.

Example:
`=BESH.REGR.GLM_THRESH(A1)`
or
`=BESH.REGR.GLM_THRESH(A1,{0.1,0.2,0.3,0.4,0.5},TRUE)`

## BESH.REGR.LM_ANOVA

Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.

**Function wizard:** Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_ANOVA(handle, scope, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **scope** — Optional ANOVA-table selector.
Accepted values are `overall` (default), `type1`, `typei`, `type3`, and `typeiii`.
- **includeHeader** — TRUE to include the title and header rows (default TRUE).

### Returns

A spilled array containing the requested ANOVA table.

### Example

```

=BESH.REGR.LM_ANOVA(F2)
=BESH.REGR.LM_ANOVA(F2,"type3")
```

## BESH.REGR.LM_DROP

Removes a fitted linear-model handle from the in-memory cache.

**Function wizard:** Removes a fitted linear-model handle from memory.

### Syntax

`=BESH.REGR.LM_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.

### Returns

TRUE if the handle existed and was removed; otherwise FALSE.

### Example

```

=BESH.REGR.LM_DROP(F2)
```

## BESH.REGR.LM_FIT

Fits a Gaussian linear regression model and returns a reusable model handle.

**Function wizard:** Fits a Gaussian linear regression model and returns a reusable handle.

### Syntax

`=BESH.REGR.LM_FIT(y, x, varNames, offset, weights, includeIntercept, formula, formulaAddressing, computeResiduals, alpha)`

### Parameters

- **y** — A single-column numeric range containing the continuous response.
Non-numeric or invalid rows are excluded by the shared regression-data import machinery before fitting.
- **x** — A numeric predictor matrix with one row per observation and one column per raw predictor.
The raw predictor matrix can be used directly or transformed internally by the optional `formula`.
- **varNames** — Optional raw predictor names.
This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
If omitted, default names such as X1, X2, … are assigned automatically.
- **offset** — Optional numeric offset vector with one value per observation.
When supplied, the offset is added to the fitted mean and treated as known rather than estimated.
- **weights** — Optional positive case weights.
When supplied, the fitted coefficients minimize the weighted sum of squared residuals.
Rows with nonpositive or invalid weights are excluded by the shared regression-data import machinery before fitting.
- **includeIntercept** — TRUE to include an intercept term (default TRUE).
Set FALSE to fit a model through the origin after any formula-based predictor expansion.
- **formula** — Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix `x`.
Supported syntax currently includes additive terms (`A + B`), polynomial terms (`A^2`),
continuous-continuous interactions (`A:B`, `A:B:C`), categorical main effects such as
`factor(C)` or `factor(C, ref=2)`, categorical-continuous interactions such as
`factor(C):B`, and categorical-categorical interactions such as `factor(C):factor(D)`.
If omitted or blank, all raw predictor columns are used as continuous main effects.
- **formulaAddressing** — Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
Accepted values are `relative` (default), `absolute`, and `names`.
In `relative` mode, `A`, `B`, `AA`, … refer to columns 1, 2, 27, … of `x`.
In `absolute` mode, bare letters refer to worksheet columns of the supplied `x` range.
In `names` mode, bare letters are disabled and variables should be referenced using single-quoted names such as `'dose'`.
Single quotes inside names are escaped by doubling them, e.g. `'Children''s dose'`.
- **computeResiduals** — TRUE to store observation-level residual diagnostics for later use by `BESH.REGR.LM_RESID` (default TRUE).
Set FALSE to reduce memory use when residual diagnostics will not be requested.
- **alpha** — Optional two-sided significance level used for confidence intervals stored with the fitted model (default 0.05).
This does not affect the estimated coefficients themselves.

### Returns

A text handle identifying the fitted linear model within the current Excel session.
The handle can be passed to the other `LM_*` worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.

### Notes

When an offset is supplied, the fitted mean is `offset + Xβ`. Internally this is implemented by fitting the adjusted response
`Y - offset` against the estimated terms, while predictions returned by `BESH.REGR.LM_PRED` add the offset back.

Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting.
If `formulaAddressing="absolute"` is used, the `x` argument should be passed as a direct worksheet range
so that absolute worksheet column letters can be determined.

Term-wise ANOVA tables are prepared in both sequential (Type I) and partial (Type III) forms so that
`BESH.REGR.LM_ANOVA` can return either table without forcing an additional refit.

### Example

```

=BESH.REGR.LM_FIT(A2:A101,B2:D101,"dose,age,weight")
=BESH.REGR.LM_FIT(A2:A101,B2:E101,"dose,age,stage,treat",,F2:F101,TRUE,"A + B + factor(C, ref=1) + factor(C, ref=1):B","relative",TRUE,0.05)
```

## BESH.REGR.LM_PRED

Returns predicted mean responses for new observations from a fitted linear-model handle.

**Function wizard:** Returns predicted mean responses for new observations from a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **newX** — New raw predictor matrix in the same raw-column order used at fitting time.
- **newOffset** — Optional offset vector for the new observations. Required when the fitted model used an offset.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing one predicted mean response per new observation.
When the fitted model used an offset, the returned predictions include that offset on the original response scale.

### Example

```

=BESH.REGR.LM_PRED(F2,B2:D11)
=BESH.REGR.LM_PRED(F2,B2:D11,E2:E11)
```

## BESH.REGR.LM_RESID

Returns residual diagnostics for a fitted linear-model handle.

**Function wizard:** Returns residual diagnostics for a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **residType** — Optional residual-output selector.
Accepted values are `all` (default), `fitted`, `residual`, `leverage`,
`stdresid`, `cooksd`, and `jackknife`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing the requested residual block.
When `residType` is `all`, the returned table contains fitted values,
raw residuals, leverage, standardized residuals, Cook's distance, and jackknife residuals.

### Example

```

=BESH.REGR.LM_RESID(F2)
=BESH.REGR.LM_RESID(F2,"stdresid")
```

## BESH.REGR.LM_SUMMARY

Returns the coefficient summary table for a fitted linear-model handle.

**Function wizard:** Returns the coefficient summary table for a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided alpha for confidence intervals (default 0.05).

### Returns

A spilled array containing one row per estimated parameter with coefficient estimates,
standard errors, t statistics, p-values, and two-sided confidence limits.

### Notes

When an intercept is included, the intercept appears as its own parameter row.
For factor-coded predictors, each non-reference level contributes its own coefficient row.

### Example

```

=BESH.REGR.LM_SUMMARY(F2)
=BESH.REGR.LM_SUMMARY(F2,TRUE,0.1)
```

## BESH.REGR.LM_TESTS

Returns model-level diagnostics and fit statistics for a fitted linear-model handle.

**Function wizard:** Returns model-level diagnostics and fit statistics for a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing sample size, parameter count, degrees of freedom, R²,
adjusted R², the overall F test, log-likelihood, AIC, and BIC.

### Example

```

=BESH.REGR.LM_TESTS(F2)
```

## BESH.REGR.LM_VIF

Returns the variance-inflation-factor and partial-correlation table for a fitted linear-model handle.

**Function wizard:** Returns the variance-inflation-factor and partial-correlation table for a fitted linear-model handle.

### Syntax

`=BESH.REGR.LM_VIF(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.LM_FIT`.
- **includeHeader** — TRUE to include the title and header rows (default TRUE).

### Returns

A spilled array containing VIF and partial-correlation values per modeled predictor column.
Intercept terms are omitted.

### Example

```

=BESH.REGR.LM_VIF(F2)
```

## BESH.REGR.MMRM_CLEAR_ALL

Removes all fitted MMRM handles from the current worksheet-session cache.

**Function wizard:** Drops all fitted MMRM handles from the session cache.

### Returns

The number of handles removed from the current session cache.

### Notes

Use this function to clear all MMRM handles created during the current Excel session. It is
useful before rerunning a large workbook or after exploratory analyses that created many
temporary model handles. Recalculate any `BESH.REGR.MMRM_FIT` formulas to recreate
handles that are still needed.

## BESH.REGR.MMRM_COEF

Returns the fixed-effect coefficient table for a fitted MMRM handle.

**Function wizard:** Returns the fixed-effect coefficient table for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_COEF(handle, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **alpha** — Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.

### Returns

A dynamic array containing the fixed-effect coefficient table.

### Notes

The table contains the fixed-effect estimates and the inferential columns associated with
the inference method chosen during fitting. For Kenward-Roger fits, the standard errors,
degrees of freedom, test statistics, p-values, and confidence intervals are reported using
the Kenward-Roger adjustment.

## BESH.REGR.MMRM_CONTRASTS

Returns observed-design-grid contrasts between group levels for a fitted MMRM handle.

**Function wizard:** Returns observed-design-grid group contrasts for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_CONTRASTS(handle, group, contrastMode, controlLevel, comparisonLevel, direction, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **group** — Fitted design column used as the grouping factor, for example `treatment_active`.
- **contrastMode** — Contrast mode: `Pairwise among group levels`, `Each group vs control`, or `Selected comparison only`. Default is pairwise.
- **controlLevel** — Optional numeric control/reference level. When omitted, the lowest observed group level is used.
- **comparisonLevel** — Optional numeric comparison level for selected-comparison mode.
- **direction** — Contrast direction: `Higher level - lower level`, `Treatment - control`, or `Control - treatment`. Default is treatment minus control for control-based contrasts and higher minus lower for pairwise contrasts.
- **alpha** — Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.

### Returns

A dynamic array containing group contrasts by visit.

### Notes

This extractor compares levels of one numeric fitted design column within each visit/time
value. It is intended for common treatment-difference workflows where the design matrix
contains a treatment indicator or other coded grouping column.

The default contrast mode returns all pairwise group differences within each visit. To
compare each group against a selected control level, set `contrastMode` to
`"Each group vs control"` and provide `controlLevel`. To request one
selected comparison, set `contrastMode` to `"Selected comparison only"`
and provide both `controlLevel` and `comparisonLevel`.

For Kenward-Roger fits, the returned standard errors, denominator degrees of freedom, test
statistics, p-values, and confidence intervals use the Kenward-Roger adjustment.

## BESH.REGR.MMRM_COVPARMS

Returns estimated within-subject covariance parameters for a fitted MMRM handle.

**Function wizard:** Returns covariance-parameter estimates for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_COVPARMS(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.

### Returns

A dynamic array containing covariance-parameter estimates and related columns.

### Notes

The returned rows describe the fitted covariance parameters for the selected
within-subject covariance structure. The exact parameter labels depend on the covariance
structure used in `BESH.REGR.MMRM_FIT`.

## BESH.REGR.MMRM_DROP

Removes a fitted MMRM handle from the current worksheet-session cache.

**Function wizard:** Drops a fitted MMRM handle from the session cache.

### Syntax

`=BESH.REGR.MMRM_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.

### Returns

TRUE if the handle was found and removed; otherwise FALSE.

### Notes

Use this function when a saved handle is no longer needed. Removing unused handles can
reduce memory use in long Excel sessions. Recalculate the original fit function to create
a new handle.

## BESH.REGR.MMRM_FIT

Fits a Mixed Model for Repeated Measures and returns a reusable worksheet-session handle.

**Function wizard:** Fits an MMRM and returns a reusable handle.

### Syntax

`=BESH.REGR.MMRM_FIT(y, x, subject, visit, varNames, covariance, fitMethod, inference, includeIntercept, formula, formulaAddressing, alpha, maxIter, trace, covOptimizerMode, covGradientMode)`

### Parameters

- **y** — Single-column range containing the continuous response values.
- **x** — Numeric matrix containing raw fixed-effect predictors or already-coded design columns. Each row must correspond to the same row in `y`.
- **subject** — Single-column range identifying the subject for each observation. Repeated rows with the same identifier are treated as belonging to the same subject.
- **visit** — Optional single-column numeric visit/time range. When supplied, observations are sorted within each subject by this value before the covariance structure is evaluated.
- **varNames** — Optional row or column range containing predictor names for the columns of `x`. If omitted, generic names are used.
- **covariance** — Within-subject covariance structure. Accepted values include `ID`, `Diagonal`, `CS`, `HCS`, `AR(1)`, `HAR(1)`, and `UN`. The default is `UN`.
- **fitMethod** — Likelihood method. Use `REML` for restricted maximum likelihood or `ML` for maximum likelihood. The default is `REML`.
- **inference** — Fixed-effect inference method. Accepted values include `KR`, `Satterthwaite`, `BetweenWithin`, `ResidualDF`, and `Wald`. The default is `KR`.
- **includeIntercept** — TRUE to add an intercept column before fitting; FALSE when `x` already contains all desired columns. The default is TRUE.
- **formula** — Optional right-hand-side formula used to expand the raw predictor matrix before fitting. Leave blank to use the columns of `x` as supplied.
- **formulaAddressing** — Formula-addressing mode: `relative` (default), `absolute`, or `names`.
- **alpha** — Two-sided alpha level for confidence intervals returned by extractor functions. The default is 0.05.
- **maxIter** — Optional maximum number of optimizer iterations. Leave blank to use the standard setting.
- **trace** — TRUE to store detailed optimizer trace text in the fitted handle. Stored trace text can
later be included by `BESH.REGR.MMRM_RESULTS` when its
`includeOptimizerTrace` argument is TRUE. The default is FALSE.
- **covOptimizerMode** — Optional covariance-optimizer mode. Blank uses the default SAS PROC MIXED-style
Average Information / Fisher-scoring REML optimizer with safe fallback. Accepted values
include `AI`, `AverageInformation`, `FisherScoring`, or `SAS` for
Average Information / Fisher scoring; `BFGS` or `ProjectedBFGS` for projected
BFGS using the selected gradient mode; `BFGS_ANALYTIC` for projected BFGS with
analytic scores; and `BFGS_NUMERICAL` for projected BFGS with finite-difference
gradients. Average Information is REML-oriented; if it is not applicable or does not
produce a usable covariance solution, the fit records diagnostics and falls back to
projected BFGS.
- **covGradientMode** — Optional covariance-gradient mode used by projected BFGS and by fallback paths. Blank
uses `Auto`. Accepted values include `Auto`, `Analytic`,
`AnalyticValidation`, `Validate`, `Numerical`, and
`FiniteDifference`. Auto uses analytic covariance scores for validated residual
covariance structures and numerical finite differences otherwise. AnalyticValidation
runs the analytic score path and records a finite-difference comparison in diagnostics;
it is intended for validation and troubleshooting rather than routine production use.

### Returns

A text handle that identifies the fitted model in the current Excel session, or an error message if the fit cannot be created.

### Notes

Use this function when observations are grouped by subject and the within-subject
covariance is modeled directly. The response and every predictor column must be numeric.
Subject identifiers may be text or numeric. A visit/time column is optional but recommended
whenever repeated measurements have a meaningful order or when rows are not already sorted
within each subject.

Rows with missing or invalid response, predictor, subject, or supplied visit values are
excluded before fitting. The returned handle stores the fitted analysis for the current
Excel session and can be passed to the extractor functions listed below.

The default fit uses REML, an unstructured within-subject covariance matrix, an intercept
column, and Kenward-Roger fixed-effect inference. Kenward-Roger inference requires REML.
To use maximum likelihood, set `fitMethod` to `"ML"` and choose a
non-Kenward-Roger inference method.

Covariance optimization can be controlled from the worksheet. By default the fit uses a
SAS PROC MIXED-style Average Information / Fisher-scoring REML covariance optimizer when
it is applicable. If that optimizer cannot provide a usable solution, the fit falls back
to the projected BFGS covariance optimizer. The gradient mode controls the derivative
source used by projected BFGS: automatic analytic scores for validated covariance
structures, fully numerical finite differences, analytic scores only, or analytic scores
with finite-difference validation diagnostics.

The optimizer setting is most useful for reproducibility and troubleshooting. Use
`AI` or `AverageInformation` for the default Average Information / Fisher-scoring
optimizer, `BFGS` for projected BFGS with the selected gradient mode,
`BFGS_ANALYTIC` for projected BFGS with analytic scores, or `BFGS_NUMERICAL` for
projected BFGS with finite-difference gradients. Use the numerical option when comparing
against older workbooks or when diagnosing a suspected analytic derivative issue.

Common follow-up functions are `BESH.REGR.MMRM_COEF`,
`BESH.REGR.MMRM_TYPE3`, `BESH.REGR.MMRM_COVPARMS`,
`BESH.REGR.MMRM_FITSTATS`, `BESH.REGR.MMRM_LSMEANS`,
`BESH.REGR.MMRM_CONTRASTS`, `BESH.REGR.MMRM_FITTED`, and
`BESH.REGR.MMRM_RESID`.

## BESH.REGR.MMRM_FITSTATS

Returns likelihood, information-criterion, convergence, and model-size statistics.

**Function wizard:** Returns fit statistics for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_FITSTATS(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.

### Returns

A dynamic array containing model fit statistics.

### Notes

Use this function to review model fit, compare alternative covariance structures fitted
with the same method, and check high-level convergence information.

## BESH.REGR.MMRM_FITTED

Returns row-level marginal fitted values for a fitted MMRM handle.

**Function wizard:** Returns marginal fitted values for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_FITTED(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **includeHeader** — TRUE to include a header row. The default is TRUE.

### Returns

A dynamic array with row number and fitted value columns.

### Notes

The returned rows correspond to the valid rows that remained after input screening in
`BESH.REGR.MMRM_FIT`. This is a convenience extractor for workbooks that need fitted
values without the residual column returned by `BESH.REGR.MMRM_RESID`.

## BESH.REGR.MMRM_LSMEANS

Returns observed-design-grid estimated marginal means for a fitted MMRM handle.

**Function wizard:** Returns observed-design-grid LS-means for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_LSMEANS(handle, group, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **group** — Optional fitted design column to use as a grouping factor, for example `treatment_active`. Leave blank for visit-only means.
- **alpha** — Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.

### Returns

A dynamic array containing estimated marginal means.

### Notes

This extractor computes LS-means from the fitted fixed-effect design rows retained during
the model fit. When `group` is blank, the table contains one estimated
marginal mean for each visit/time value. When `group` names a numeric
design column, the table contains means for each visit-by-group profile.

The estimates use the same inference method saved with the fit. For Kenward-Roger fits,
standard errors, denominator degrees of freedom, test statistics, p-values, and confidence
limits use the Kenward-Roger adjustment.

This function uses the observed design grid. Covariate/reference-grid helper functions can
be added separately if a workbook needs SAS/R-style equal-cell marginalization over class
factors and user-specified covariate values.

## BESH.REGR.MMRM_LSMESTIMATE

Returns custom MMRM LS-mean estimates or contrasts from a fitted MMRM handle, using
a worksheet specification that is intentionally similar to the coefficient rows supplied
to the SAS `PROC MIXED` `LSMESTIMATE` statement.

**Function wizard:** Returns custom SAS-style LS-mean estimates/contrasts for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_LSMESTIMATE(handle, spec, alpha, at)`

### Parameters

- **handle** — A worksheet-session handle returned by `BESH.REGR.MMRM_FIT`.
- **spec** — A worksheet range containing a header row and one or more profile/contrast rows. Required
column: `weight`. Optional columns: `label` and `visit`. Any other nonblank
header must name a fitted fixed-effect design column stored in the MMRM handle.
- **alpha** — Optional two-sided significance level used for confidence intervals. If omitted, the alpha
stored when the MMRM handle was created is used. The value must be finite and between 0 and 1
according to the shared alpha parser used by the MMRM extractor UDFs.
- **at** — Optional SAS `AT`-style worksheet range of common profile settings. The range may be a
two-column name/value table or a wide single-row table. Names may be `visit` or fitted
fixed-effect design column names. Values in `spec` override values in
`at` for the same visit/design column.

### Returns

A dynamic array containing one row per custom estimate/contrast. The columns are produced
by the common mixed-model linear contrast formatter and include the label, estimate, standard
error, confidence limits, test statistic, denominator degrees of freedom when available, and
p-value. On validation failure, the function returns a descriptive text error; if the handle
is not found, it returns `#N/A`.

### Notes

`BESH.REGR.MMRM_LSMESTIMATE` evaluates one or more user-defined linear functions
of the fitted fixed-effect coefficients from a previously fitted MMRM model. The function
is intended for custom estimands that cannot be expressed conveniently by the simpler
pairwise/control LS-mean contrast extractors, for example contrasts involving several
factors at once, weighted averages over selected profiles, custom change-from-baseline
combinations, or an average of several treatment/visit profiles.

The first argument must be a handle returned by `BESH.REGR.MMRM_FIT`. The second
argument, `spec`, is a worksheet range with a header row and one or more
data rows. Each nonblank data row describes one LS-mean profile contribution. Rows with
the same label are accumulated into one final estimate as
`sum(weight * L(profile)) * beta`, where `L(profile)` is the average fixed-effect
design row among observations matching the requested profile. The resulting dynamic array
contains the estimate, standard error, confidence interval, test statistic, degrees of
freedom when available, and p-value using the variance-covariance information stored in
the fitted MMRM result.

The `spec` range must contain a column named `weight`. Accepted
aliases are `coef`, `coefficient`, and `contrastweight`. The weights are
analogous to the numbers written in a SAS `LSMESTIMATE` coefficient row. Use values
such as `1`, `0`, and `-1` for simple differences, or fractional values such
as `0.5` for averages. Rows with a zero weight are ignored after validation.

The optional `label` column groups rows into estimates. Accepted aliases are
`contrast`, `estimate`, and `name`. If no label is supplied for a row,
labels are generated as `Estimate 1`, `Estimate 2`, and so on. To build a
contrast that uses multiple profile rows, give those rows exactly the same label.

The optional `visit` column restricts a profile contribution to a visit/time value
saved in the MMRM handle. Accepted alias: `time`. Any additional nonblank column in
`spec` must match a fitted fixed-effect design column name, using either
the exact name or a punctuation-insensitive/case-insensitive version of the name. For
example, if the model handle stores design columns named `treatment_active`,
`sex_code`, and `treatment_active:sex_code`, those headers may be used as profile
columns. The numeric value in each cell is matched against the corresponding saved design
row value. This makes the function work with model-matrix columns already created by the
BESHStatNG formula/design machinery.

The optional `at` argument implements an Excel range analogue of the SAS
`AT` option. It supplies profile values that are applied to every row in
`spec` unless that row explicitly overrides the same visit/design column.
Use it for settings that are common to the whole custom estimand, such as holding a
covariate at its mean or evaluating all rows at a particular factor level. The
`at` range can be supplied in either of two forms:

- Name/value form: two columns with headers such as `name` and `value`. The first column contains `visit` or a fitted design column name; the second column contains the numeric value to use.
- Wide form: a header row containing `visit` and/or fitted design column names, followed by one nonblank data row containing the numeric values.

Values supplied in `spec` take precedence over values supplied in
`at`. For example, if `at` sets `visit=4` but one
row of `spec` supplies `visit=2`, that row is evaluated at visit 2.
If `at` sets `age=65` and the `spec` range does not
contain an `age` column, every profile contribution is evaluated among observed design
rows with `age=65`.

This worksheet UDF uses the observed fitted design rows stored in the handle. It therefore
evaluates profiles by averaging rows that actually occur in the retained analysis data and
match the requested profile/AT conditions. It does not synthesize new design rows that were
absent from the observed design. If no row matches a requested profile, the function returns
a descriptive error identifying the affected label. When interaction terms are present,
provide any required interaction design columns explicitly in `spec` or
`at` if those columns are needed to identify the intended profile.

Blank cells in profile columns are treated as unspecified. Numeric cells must be finite;
missing, text, Boolean, error, nonnumeric, infinite, or NaN values in required numeric
positions produce an error. The intercept column should normally be left unspecified.

Example `spec` for a treatment difference at visit 2 among male subjects,
with a design column named `sex_code` coded as 1 for male:

```

label              weight   visit   treatment_active   sex_code
Active-Control V2   1        2       1                  1
Active-Control V2  -1        2       0                  1
```

The same example using `at` to avoid repeating the common visit and sex
settings:

```

spec:
label              weight   treatment_active
Active-Control V2   1        1
Active-Control V2  -1        0

at:
name       value
visit      2
sex_code   1
```

## BESH.REGR.MMRM_RESID

Returns row-level fitted values and raw marginal residuals for a fitted MMRM handle.

**Function wizard:** Returns fitted values and raw marginal residuals for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_RESID(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **includeHeader** — TRUE to include a header row. The default is TRUE.

### Returns

A dynamic array with row number, fitted value, and residual columns.

### Notes

The returned rows correspond to the valid rows that remained after input screening in
`BESH.REGR.MMRM_FIT`. The residual is the observed response minus the marginal fitted
value for the fixed-effect part of the model.

## BESH.REGR.MMRM_RESULTS

Returns all result tables, or one selected result table, from a fitted MMRM handle.

**Function wizard:** Returns all MMRM result tables or one named table for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_RESULTS(handle, table, includeOptimizerTrace, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **table** — Optional result-table title to return. Leave blank to return all available tables.
- **includeOptimizerTrace** — TRUE to include stored optimizer trace information when returning all tables. The default is FALSE.
- **alpha** — Optional two-sided alpha level for confidence intervals. Leave blank to use the alpha value saved with the fit.

### Returns

A dynamic array containing the requested result table or tables.

### Notes

Leave `table` blank to return the complete set of available tables stacked
vertically. Provide a table title to return only that table. Common table titles include
`Fixed effects`, `Kenward-Roger term-level F tests`,
`Covariance parameters`, `Estimated R covariance matrix`,
`Estimated R correlation matrix`, `Fit statistics`, and `Convergence`.

If trace output was requested when the model was fitted, set
`includeOptimizerTrace` to TRUE to include the iteration history in the
returned result set.

## BESH.REGR.MMRM_R_CORR

Returns the fitted within-subject correlation matrix for a fitted MMRM handle.

**Function wizard:** Returns the fitted R-side correlation matrix for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_R_CORR(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.

### Returns

A dynamic array containing the estimated within-subject correlation matrix.

### Notes

This table converts the fitted within-subject covariance matrix to correlations, making it
easier to compare the strength of association between visits or time points.

## BESH.REGR.MMRM_R_COV

Returns the fitted within-subject covariance matrix for a fitted MMRM handle.

**Function wizard:** Returns the fitted R-side covariance matrix for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_R_COV(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.

### Returns

A dynamic array containing the estimated within-subject covariance matrix.

### Notes

The matrix is reported on the response scale for the visit levels represented in the
fitted analysis. For unstructured covariance, this table is often the easiest way to review
the estimated variances and covariances by visit.

## BESH.REGR.MMRM_TYPE3

Returns the term-level fixed-effect F-test table for a fitted MMRM handle.

**Function wizard:** Returns the Kenward-Roger term-level F-test table for a fitted MMRM handle.

### Syntax

`=BESH.REGR.MMRM_TYPE3(handle, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MMRM_FIT`.
- **alpha** — Reserved for consistency with other extractors. Leave blank unless a future version documents alpha-dependent columns for this table.

### Returns

A dynamic array containing the term-level fixed-effect F-test table.

### Notes

For Kenward-Roger fits, the table reports term-level F tests using Kenward-Roger
denominator degrees of freedom and F scaling. This is the worksheet equivalent of the
model-wide fixed-effect tests shown in the graphical MMRM output.

## BESH.REGR.MNLOGIT_CLASS

Returns the observed-versus-predicted classification table for a fitted multinomial-logit model handle.

**Function wizard:** Returns the classification confusion matrix for a fitted multinomial-logit model handle.

### Syntax

`=BESH.REGR.MNLOGIT_CLASS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.
- **includeHeader** — TRUE to include header rows and labels (default TRUE).

### Returns

A spilled array containing the weighted or unweighted confusion matrix, per-row recall percentages,
per-column precision percentages, and overall classification accuracy.

### Notes

The classification table is based on assigning each observation to the category with the largest fitted probability.
The row and column labels are shown in the original ascending category order, independent of the reference-category choice used during fitting.

### Example

```

=BESH.REGR.MNLOGIT_CLASS(F2)
```

## BESH.REGR.MNLOGIT_DROP

Removes a fitted multinomial-logit handle from the in-memory cache.

**Function wizard:** Removes a fitted multinomial-logit model handle from memory.

### Syntax

`=BESH.REGR.MNLOGIT_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.

### Returns

TRUE if the handle existed and was removed; otherwise FALSE.

### Example

```

=BESH.REGR.MNLOGIT_DROP(F2)
```

## BESH.REGR.MNLOGIT_FIT

Fits a baseline-category multinomial logistic regression model and returns a reusable model handle.

**Function wizard:** Fits a baseline-category multinomial logistic regression model and returns a reusable handle.

### Syntax

`=BESH.REGR.MNLOGIT_FIT(y, x, varNames, offset, weights, reference, includeIntercept, formula, formulaAddressing, maxIter, tol, alpha)`

### Parameters

- **y** — A single-column numeric range containing the categorical outcome.
Values must be finite integers representing the observed response categories.
The distinct categories are sorted and the requested reference category is moved to the baseline position used internally by the model.
- **x** — A numeric predictor matrix with one row per observation and one column per raw predictor.
The raw predictor matrix can be used directly or transformed internally by the optional `formula`.
- **varNames** — Optional raw predictor names.
This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
If omitted, default names such as X1, X2, … are assigned automatically.
- **offset** — Optional numeric offset vector with one value per observation.
When supplied, the offset is added to each non-baseline linear predictor and treated as known rather than estimated.
- **weights** — Optional nonnegative case weights.
Positive weights act like replicate or importance weights in the log-likelihood. Rows with nonpositive or invalid weights are excluded before fitting.
- **reference** — Optional baseline-category choice for the response scale.
Accepted values are `last` (default) and `first`.
The selected category becomes the baseline category against which all other logits are formed.
- **includeIntercept** — TRUE to include one category-specific intercept for each non-baseline category (default TRUE).
This corresponds to the usual multinomial-logit formulation.
- **formula** — Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix `x`.
Supported syntax currently includes additive terms (`A + B`), polynomial terms (`A^2`),
continuous-continuous interactions (`A:B`, `A:B:C`), categorical main effects such as
`factor(C)` or `factor(C, ref=2)`, categorical-continuous interactions such as
`factor(C):B`, and categorical-categorical interactions such as `factor(C):factor(D)`.
If omitted or blank, all raw predictor columns are used as continuous main effects.
- **formulaAddressing** — Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
Accepted values are `relative` (default), `absolute`, and `names`.
In `relative` mode, `A`, `B`, `AA`, … refer to columns 1, 2, 27, … of `x`.
In `absolute` mode, bare letters refer to worksheet columns of the supplied `x` range.
In `names` mode, bare letters are disabled and variables should be referenced using single-quoted names such as `'dose'`.
Single quotes inside names are escaped by doubling them, e.g. `'Children''s dose'`.
- **maxIter** — Optional maximum number of Newton-style iterations used by the fitting procedure.
Increase this value when convergence is slow for more complex models.
- **tol** — Optional convergence tolerance controlling the stopping criteria for parameter changes and log-likelihood changes.
Smaller values demand tighter convergence but may increase runtime.
- **alpha** — Optional two-sided significance level used internally for confidence intervals stored in the wrapped regression results.
This does not affect the maximum-likelihood estimates themselves.

### Returns

A text handle identifying the fitted multinomial-logit model within the current Excel session.
The handle can be passed to the other `MNLOGIT_*` worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.

### Notes

Unlike ordinal logistic regression, multinomial logistic regression does not assume proportional odds or any intrinsic ordering of the response categories.
A separate slope vector is estimated for each non-baseline category, so predictor effects may differ across category comparisons.

Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting. At least two distinct response categories must remain.

If `formulaAddressing="absolute"` is used, the `x` argument should be passed as a direct worksheet range so that absolute worksheet column letters can be determined.

Residual diagnostics are computed during fitting so that `BESH.REGR.MNLOGIT_RESID` can reuse the fitted object without forcing an additional refit.

### Example

```

=BESH.REGR.MNLOGIT_FIT(A2:A101,B2:D101,"dose,age,prison")
=BESH.REGR.MNLOGIT_FIT(A2:A101,B2:E101,"dose,age,prison,stage",,,"last",TRUE,"A + B + factor(D, ref=1) + factor(D, ref=1):B","relative",100,1E-8,0.05)
```

## BESH.REGR.MNLOGIT_PRED

Returns fitted category probabilities and predicted categories for new data under a fitted multinomial-logit model.

**Function wizard:** Returns fitted probabilities and predicted categories for new data under a fitted multinomial-logit model.

### Syntax

`=BESH.REGR.MNLOGIT_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.
- **newX** — New raw predictor matrix in the same raw-column order used at fitting time.
- **newOffset** — Optional offset vector for the new observations.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing the most likely predicted category, one category-specific linear predictor column for each non-baseline category,
and one fitted probability column per outcome category in the model's internal category order.

### Notes

The probability columns sum to 1 across each row, up to normal floating-point rounding error.
The predicted category is the category whose fitted probability is largest in the returned probability vector.

When the model was fitted with an offset, `newOffset` must be supplied and aligned with the rows of `newX`.

### Example

```

=BESH.REGR.MNLOGIT_PRED(F2,H2:J10)
```

## BESH.REGR.MNLOGIT_RESID

Returns residual diagnostics for a fitted multinomial-logit model handle.

**Function wizard:** Returns residual diagnostics for a fitted multinomial-logit model handle.

### Syntax

`=BESH.REGR.MNLOGIT_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.
- **residType** — Optional residual-output selector.
Accepted values are `all` (default), `observed`, `fittedmean`, `prob`, `response`,
`pearson`, `stdpearson`, `deviance`, `stddeviance`, and `leverage`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing the requested residual block.
When `residType` is `all`, the returned table contains all category-specific blocks plus the scalar residual diagnostics.

### Example

```

=BESH.REGR.MNLOGIT_RESID(F2)
=BESH.REGR.MNLOGIT_RESID(F2,"pearson")
```

## BESH.REGR.MNLOGIT_SUMMARY

Returns the parameter summary table for a fitted multinomial-logit model handle.

**Function wizard:** Returns the parameter summary table for a fitted multinomial-logit model handle.

### Syntax

`=BESH.REGR.MNLOGIT_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided alpha for confidence intervals and odds-ratio confidence limits (default 0.05).

### Returns

A spilled array containing one row per estimated parameter. Slope parameters are accompanied by odds ratios and odds-ratio confidence limits;
category-specific intercept parameters leave the odds-ratio columns blank because exponentiated intercepts are generally not interpreted as predictor-effect odds ratios.

### Notes

Parameter names identify the compared category and the reference category. For example, a parameter name such as
`cat=2 (ref=4): dose` refers to the log-odds contrast between category 2 and the baseline category 4.

### Example

```

=BESH.REGR.MNLOGIT_SUMMARY(F2)
=BESH.REGR.MNLOGIT_SUMMARY(F2,TRUE,0.1)
```

## BESH.REGR.MNLOGIT_TESTS

Returns global model tests and fit statistics for a fitted multinomial-logit model handle.

**Function wizard:** Returns model-level diagnostics and tests for a fitted multinomial-logit model handle.

### Syntax

`=BESH.REGR.MNLOGIT_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.MNLOGIT_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing model-level diagnostics such as log-likelihoods, likelihood-ratio and goodness-of-fit tests,
pseudo-R² measures, information criteria, iteration count, and convergence information.

### Example

```

=BESH.REGR.MNLOGIT_TESTS(F2)
```

## BESH.REGR.ORDLOGIT_CLASS

Returns the observed-versus-predicted classification table for a fitted ordinal-logit model handle.

**Function wizard:** Returns the classification confusion matrix for a fitted ordinal-logit model handle.

### Syntax

`=BESH.REGR.ORDLOGIT_CLASS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.
- **includeHeader** — TRUE to include header rows and labels (default TRUE).

### Returns

A spilled array containing the weighted or unweighted confusion matrix, per-row recall percentages,
per-column precision percentages, and overall classification accuracy.

### Notes

The classification table is based on assigning each observation to the category with the largest fitted probability.
The category columns are shown in the model's internal category order, which depends on the reference-direction choice used during fitting.

### Example

```

=BESH.REGR.ORDLOGIT_CLASS(F2)
```

## BESH.REGR.ORDLOGIT_DROP

Removes a fitted ordinal-logit handle from the in-memory cache.

**Function wizard:** Removes a fitted ordinal-logit model handle from memory.

### Syntax

`=BESH.REGR.ORDLOGIT_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.

### Returns

TRUE if the handle existed and was removed; otherwise FALSE.

### Example

```

=BESH.REGR.ORDLOGIT_DROP(F2)
```

## BESH.REGR.ORDLOGIT_FIT

Fits a proportional-odds ordinal logistic regression model and returns a reusable model handle.

**Function wizard:** Fits a proportional-odds ordinal logistic regression model and returns a reusable handle.

### Syntax

`=BESH.REGR.ORDLOGIT_FIT(y, x, varNames, offset, weights, reference, formula, formulaAddressing, maxIter, tol, alpha)`

### Parameters

- **y** — A single-column numeric range containing the ordinal outcome.
Values must be finite integers representing the ordered response categories.
The observed categories are sorted and used as the ordinal scale of the model.
- **x** — A numeric predictor matrix with one row per observation and one column per raw predictor.
The predictor matrix can be used directly or transformed internally by the optional `formula`.
- **varNames** — Optional raw predictor names.
This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
If omitted, default names such as X1, X2, … are assigned automatically.
- **offset** — Optional numeric offset vector with one value per observation.
When supplied, the offset is added to the linear predictor and treated as known rather than estimated.
- **weights** — Optional nonnegative case weights.
Positive weights act like replicate or importance weights in the log-likelihood. Rows with nonpositive or invalid weights are excluded before fitting.
- **reference** — Optional direction / reference choice for the ordered outcome scale.
Accepted values are `last` (default) and `first`.
The choice changes the internal ordering used by the cumulative logits and therefore changes the interpretation of the thresholds.
- **formula** — Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix `x`.
Supported syntax currently includes additive terms (`A + B`), polynomial terms (`A^2`),
continuous-continuous interactions (`A:B`, `A:B:C`), categorical main effects such as
`factor(C)` or `factor(C, ref=2)`, categorical-continuous interactions such as
`factor(C):B`, and categorical-categorical interactions such as `factor(C):factor(D)`.
If omitted or blank, all raw predictor columns are used as continuous main effects.
- **formulaAddressing** — Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
Accepted values are `relative` (default), `absolute`, and `names`.
In `relative` mode, `A`, `B`, `AA`, … refer to columns 1, 2, 27, … of `x`.
In `absolute` mode, bare letters refer to worksheet columns of the supplied `x` range.
In `names` mode, bare letters are disabled and variables should be referenced using single-quoted names such as `'dose'`.
Single quotes inside names are escaped by doubling them, e.g. `'Children''s dose'`.
- **maxIter** — Optional maximum number of Newton-style iterations used by the fitting procedure.
Increase this value when convergence is slow for more complex models.
- **tol** — Optional convergence tolerance controlling the stopping criteria for parameter changes and log-likelihood changes.
Smaller values demand tighter convergence but may increase runtime.
- **alpha** — Optional two-sided significance level used internally for confidence intervals stored in the wrapped regression results.
This does not affect the maximum-likelihood estimates themselves.

### Returns

A text handle identifying the fitted ordinal-logit model within the current Excel session.
The handle can be passed to the other `ORDLOGIT_*` worksheet functions to obtain summaries, diagnostics, and predictions without refitting the model.

### Notes

The proportional-odds ordinal logistic model uses one common slope vector for all cumulative splits of the ordered response,
while estimating a separate threshold (cutpoint) for each adjacent outcome boundary.
Unlike ordinary binary logistic regression, the thresholds play the role of intercept terms and a separate free intercept column in `x` is not identifiable.

Rows containing invalid values in the response, predictors, offset, or weights are excluded before fitting. At least two distinct ordered outcome categories must remain.

If `formulaAddressing="absolute"` is used, the `x` argument should be passed as a direct worksheet range so that absolute worksheet column letters can be determined.

Residual diagnostics are computed during fitting so that `BESH.REGR.ORDLOGIT_RESID` can reuse the fitted object without forcing an additional refit.

### Example

```

=BESH.REGR.ORDLOGIT_FIT(A2:A101,B2:D101,"dose,age,prison")
=BESH.REGR.ORDLOGIT_FIT(A2:A101,B2:E101,"dose,age,prison,stage",,,"last","A + B + factor(D, ref=1) + factor(D, ref=1):B","relative",100,1E-8,0.05)
```

## BESH.REGR.ORDLOGIT_PRED

Returns fitted probabilities and the most likely category for new predictor values under a fitted ordinal-logit model.

**Function wizard:** Returns fitted probabilities and predicted categories for new data under a fitted ordinal-logit model.

### Syntax

`=BESH.REGR.ORDLOGIT_PRED(handle, newX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.
- **newX** — New raw predictor matrix with the same raw-column structure used at model fitting time.
If the fitted model used a formula, the required expanded design matrix is rebuilt internally from this raw matrix.
- **newOffset** — Optional offset vector for the new observations.
If the fitted model used an offset, a matching new offset must be supplied here.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing the linear predictor, the most likely predicted category, and one fitted probability column per outcome category.
Probability columns are returned in the model's internal category order.

### Notes

The probability columns sum to 1 across each row, up to normal floating-point rounding error. The predicted category is the category whose fitted
probability is largest in the returned probability vector.

When the model was fitted with an offset, `newOffset` must be supplied and aligned with the rows of `newX`.

### Example

```

=BESH.REGR.ORDLOGIT_PRED(F2,H2:J10)
```

## BESH.REGR.ORDLOGIT_RESID

Returns residual diagnostics for a fitted ordinal-logit model handle.

**Function wizard:** Returns residual diagnostics for a fitted ordinal-logit model handle.

### Syntax

`=BESH.REGR.ORDLOGIT_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.
- **residType** — Residual block to return.
Accepted values are `all` (default), `fittedmean`, `prob`, `response`, `pearson`, `stdpearson`,
`deviance`, `stddeviance`, and `leverage`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing the requested residual-related output.
Category-specific outputs return one column per outcome category in the model's internal category order.

### Notes

The `all` view reproduces the main residual blocks computed by the fitted ordinal model:
fitted means, fitted probabilities, raw response residuals, Pearson residuals, standardized Pearson residuals,
deviance residuals, standardized deviance residuals, and leverage.

### Example

```

=BESH.REGR.ORDLOGIT_RESID(F2)
=BESH.REGR.ORDLOGIT_RESID(F2,"pearson")
```

## BESH.REGR.ORDLOGIT_SUMMARY

Returns a coefficient table for a fitted ordinal-logit model handle.

**Function wizard:** Returns the parameter summary table for a fitted ordinal-logit model handle.

### Syntax

`=BESH.REGR.ORDLOGIT_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided alpha for confidence intervals and odds-ratio confidence limits (default 0.05).

### Returns

A spilled array containing one row per estimated parameter. Slope parameters are accompanied by odds ratios and odds-ratio confidence limits;
threshold parameters leave the odds-ratio columns blank because exponentiated thresholds are generally not interpreted as odds ratios for a predictor effect.

### Notes

The first block of parameters corresponds to the common slope vector for the predictors. The remaining parameters are threshold (cutpoint) terms
that separate adjacent levels of the ordered response scale.

For slope parameters, exponentiating the coefficient gives the proportional-odds ratio associated with a one-unit increase in the predictor while the other predictors are held fixed.

### Example

```

=BESH.REGR.ORDLOGIT_SUMMARY(F2)
=BESH.REGR.ORDLOGIT_SUMMARY(F2,TRUE,0.1)
```

## BESH.REGR.ORDLOGIT_TESTS

Returns global model tests and fit statistics for a fitted ordinal-logit model handle.

**Function wizard:** Returns model-level diagnostics and tests for a fitted ordinal-logit model handle.

### Syntax

`=BESH.REGR.ORDLOGIT_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ORDLOGIT_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A spilled array containing model-level diagnostics such as log-likelihoods, likelihood-ratio and goodness-of-fit tests,
pseudo-R² measures, information criteria, iteration count, and convergence information.

### Notes

These diagnostics are taken from the fitted model object without refitting the model. The exact rows mirror the diagnostic quantities stored
by regression.OrdinalLogitModel in its shared regression-results container.

### Example

```

=BESH.REGR.ORDLOGIT_TESTS(F2)
```

## BESH.REGR.ZIP_DROP

Removes a fitted ZIP model handle from the in-memory cache.

**Function wizard:** Removes a fitted Zero-Inflated Poisson model handle from memory.

### Syntax

`=BESH.REGR.ZIP_DROP(handle)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ZIP_FIT`.

### Returns

TRUE when the handle was found and removed; otherwise FALSE.

### Notes

Handles are session-scoped identifiers for cached fitted models.
Removing a handle frees the corresponding in-memory model object for the current Excel session
and invalidates subsequent lookups using that handle.

## BESH.REGR.ZIP_FIT

Fits a Zero-Inflated Poisson regression model and returns a reusable handle.

**Function wizard:** Fits a Zero-Inflated Poisson regression model and returns a reusable handle.

### Syntax

`=BESH.REGR.ZIP_FIT(y, xCount, xZero, countVarNames, zeroVarNames, offset, includeCountIntercept, includeZeroIntercept, countFormula, zeroFormula, formulaAddressing, maxEmIter, maxIrlsIter, tol, alpha)`

### Parameters

- **y** — Integer-valued nonnegative response vector (single column) containing observed counts.
Each row corresponds to one observation.
- **xCount** — Raw predictor matrix for the Poisson count component, with one row per observation.
Rows must align with `y`, `xZero`, and `offset` when supplied.
- **xZero** — Optional raw predictor matrix for the logistic zero-inflation component.
When omitted, the function reuses `xCount` as the raw zero-component predictor matrix.
- **countVarNames** — Optional raw predictor names for the count component, supplied either as a comma-separated string
or as a one-row/one-column range.
- **zeroVarNames** — Optional raw predictor names for the zero component, supplied either as a comma-separated string
or as a one-row/one-column range. When omitted and `xZero` is omitted, the function reuses
the count-component raw predictor names.
- **offset** — Optional numeric offset vector for the Poisson count component only.
The offset enters additively on the log-mean scale:
`log(λ_i) = β_0 + x_i'β + o_i`.
A common rate-model choice is `o_i = log(t_i)` for exposure `t_i`.
- **includeCountIntercept** — TRUE to include an intercept in the Poisson count component (default TRUE).
- **includeZeroIntercept** — TRUE to include an intercept in the logistic zero component (default TRUE).
- **countFormula** — Optional right-hand-side formula used to expand the raw count-component predictor matrix before fitting. Supported terms include additive effects, polynomial terms, `A:B`, `factor(C)`, `factor(C):B`, and `factor(C):factor(D)`.
If omitted or blank, all raw count predictors enter as continuous main effects.
- **zeroFormula** — Optional right-hand-side formula used to expand the raw zero-component predictor matrix before fitting. Supported terms include additive effects, polynomial terms, `A:B`, `factor(C)`, `factor(C):B`, and `factor(C):factor(D)`.
If omitted or blank, all raw zero predictors enter as continuous main effects.
- **formulaAddressing** — Formula-addressing mode shared by both formulas: `relative` (default), `absolute`, or `names`.
- **maxEmIter** — Maximum number of EM iterations (default 200).
- **maxIrlsIter** — Maximum number of IRLS iterations used inside each M-step GLM fit (default 25).
- **tol** — Positive convergence tolerance for the absolute observed-data log-likelihood change (default 1E-9).
- **alpha** — Two-sided significance level used for Wald confidence intervals stored in the fitted result objects (default 0.05).

### Returns

A text handle identifying the fitted Zero-Inflated Poisson model in the current Excel session.
The handle can be passed to the other `ZIP_*` worksheet functions to retrieve summaries, diagnostics,
residuals, and predictions without refitting.

### Notes

This function fits the model
`Y_i ~ ZIP(λ_i, π_i)`
with
`λ_i = exp(β_0 + x_i'β + o_i)`
and
`π_i = logistic(γ_0 + z_i'γ)`.
The unconditional ZIP mean and variance are
`E[Y_i] = (1 - π_i) λ_i`
and
`Var(Y_i) = (1 - π_i) λ_i (1 + π_i λ_i)`.

The implemented EM algorithm alternates between:

- E-step: for zeros, compute the posterior structural-zero probability `τ_i = P(S_i = 1 | Y_i = 0)`.
- M-step count update: fit a Poisson/log GLM to the observed counts with weights `1 - τ_i`.
- M-step zero update: fit a Binomial/logit GLM to the fractional response `τ_i`.

After the plain EM update, the underlying engine attempts an over-relaxed step and falls back monotonically when needed,
so the observed-data log-likelihood does not decrease.

The count and zero components may use different raw predictor matrices and different formulas. Rows containing invalid or
missing values in any required argument are excluded jointly, so both submodels remain aligned on the same retained observations.

If `formulaAddressing="absolute"` is used, the relevant predictor arguments should be direct worksheet ranges so that absolute
worksheet column letters can be resolved for formula parsing.

Unlike the GLM and GLM_NB worksheet functions, this model does not accept case weights because the underlying
ZeroInflatedPoisson implementation exposes a Poisson-part offset but not a user-facing case-weight argument.

### Example

```

=BESH.REGR.ZIP_FIT(A2:A201,B2:D201)
=BESH.REGR.ZIP_FIT(A2:A201,B2:D201,E2:G201,"Age,BMI,Treat","Age,Stage,Smoker",H2:H201,TRUE,TRUE,"factor(C)+A+factor(C):A","factor(B)+C+factor(B):C")
=BESH.REGR.ZIP_FIT(A2:A201,B2:E201,,"Dose,Age,Stage,Center",,TRUE,FALSE,"A + B + factor(C) + factor(C):B","factor(D)")
```

## BESH.REGR.ZIP_PRED

Returns predicted ZIP means and component-level predictions for new data.

**Function wizard:** Returns predicted means and component predictions for new data under a fitted Zero-Inflated Poisson model.

### Syntax

`=BESH.REGR.ZIP_PRED(handle, newCountX, newZeroX, newOffset, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ZIP_FIT`.
- **newCountX** — New raw predictor matrix for the Poisson count component in the same raw-column order used at fitting time.
- **newZeroX** — Optional new raw predictor matrix for the logistic zero component in the same raw-column order used at fitting time.
When omitted, the function reuses `newCountX`.
- **newOffset** — Optional new offset vector for the Poisson count component.
It is required when the fitted model used an offset.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A rectangular table containing the ZIP mean prediction, the Poisson count mean, the structural-zero probability,
and the two component linear predictors.

### Notes

For each new observation, the function reconstructs the expanded design matrices from the stored formula specifications,
then computes
`η_c = β_0 + x'β + o`,
`λ = exp(η_c)`,
`η_z = γ_0 + z'γ`,
and
`π = logistic(η_z)`.
The returned ZIP mean prediction is
`μ = (1 - π) λ`.

This function performs deterministic plug-in prediction from the stored fitted coefficients.
It does not refit the model and does not compute prediction intervals.

## BESH.REGR.ZIP_RESID

Returns residual diagnostics for a fitted ZIP model.

**Function wizard:** Returns residual diagnostics for a fitted Zero-Inflated Poisson model.

### Syntax

`=BESH.REGR.ZIP_RESID(handle, residType, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ZIP_FIT`.
- **residType** — Residual block to return: `all` (default), `raw`, or `pearson`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

Either a two-column table of raw and Pearson residuals, or a single residual vector for the selected type.

### Notes

The raw residual is
`r_i = y_i - μ_i`
where `μ_i = (1 - π_i) λ_i` is the fitted ZIP mean.

The Pearson residual uses the ZIP variance
`Var(Y_i) = (1 - π_i) λ_i (1 + π_i λ_i)`
and is reported as
`r_i^P = (y_i - μ_i) / sqrt(Var(Y_i))`.

## BESH.REGR.ZIP_SUMMARY

Returns coefficient summaries for the count and/or zero component of a fitted ZIP model.

**Function wizard:** Returns coefficient summaries for the count and/or zero component of a fitted Zero-Inflated Poisson model.

### Syntax

`=BESH.REGR.ZIP_SUMMARY(handle, component, includeHeader, alpha)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ZIP_FIT`.
- **component** — Component selector: `all` (default), `count` / `poisson`, or `zero` / `logistic`.
- **includeHeader** — TRUE to include a header row (default TRUE).
- **alpha** — Optional two-sided significance level used to construct the displayed Wald confidence intervals.
This argument affects only interval reporting and does not refit the model.

### Returns

A rectangular coefficient table containing the selected component(s), parameter labels, standard errors,
Wald z statistics, p-values, and confidence limits.

### Notes

For either component, the table reports Wald inference based on
`z_j = \hat θ_j / SE(\hat θ_j)`
with two-sided p-value
`2 Φ(-|z_j|)`.
A `(1-α)` confidence interval is reported as
`\hat θ_j ± z_{1-α/2} SE(\hat θ_j)`.

In the Poisson count component, coefficients live on the log-mean scale, so exponentiating a slope coefficient yields the
multiplicative change in `λ_i` associated with a one-unit change in the predictor, holding other component-specific predictors fixed.

In the logistic zero component, coefficients live on the log-odds scale for structural-zero membership.
Exponentiating a slope coefficient yields the multiplicative change in the structural-zero odds.

## BESH.REGR.ZIP_TESTS

Returns model-level diagnostics and fit statistics for a fitted ZIP model.

**Function wizard:** Returns model-level diagnostics and fit statistics for a fitted Zero-Inflated Poisson model.

### Syntax

`=BESH.REGR.ZIP_TESTS(handle, includeHeader)`

### Parameters

- **handle** — Handle returned by `BESH.REGR.ZIP_FIT`.
- **includeHeader** — TRUE to include a header row (default TRUE).

### Returns

A rectangular table containing model type, component link functions, likelihood-based diagnostics,
sample-size metadata, EM convergence information, information criteria, and selected fitting warnings.

### Notes

The reported information criteria are those of the fitted ZIP model, not of either submodel taken separately.
They are based on the observed-data log-likelihood
`ℓ(β,γ) = Σ_i log P(Y_i = y_i | x_i, z_i)`.

The residual deviance reported by the underlying engine is `-2 ℓ(β,γ)`.
The AIC, AICc, and BIC values summarize overall model tradeoffs using the full ZIP parameter count
from both the Poisson and logistic components.

The convergence rows describe the EM outer loop. The relative log-likelihood-change row is the final absolute
change used by the implementation's stopping rule.
