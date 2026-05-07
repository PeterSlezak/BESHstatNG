# Survival UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Cox Regression](../methods/cox-regression.md)
- [Regression Formula Syntax](regression-formula-syntax.md)
- [Logrank Test](../methods/logrank-test.md)
- [Kaplan Meier Plot](../methods/kaplan-meier-plot.md)

## BESH.SURV.COX_BASELINE

Returns baseline quantities derived from a fitted Cox proportional hazards model.

**Function wizard:** Returns baseline survival or cumulative hazard from a fitted Cox model.

### Syntax

`=BESH.SURV.COX_BASELINE(handle, baselineType)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.
- **baselineType** — Optional output type.
Common choices include `table`, `survival`, `cumhaz`, or a plot-ready representation, depending on the supported outputs in the implementation.

### Returns

A spilled array containing baseline output evaluated over event times.
Depending on the requested type, the result may include event times together with the baseline survival function,
cumulative baseline hazard, or a fuller tabular representation.

### Notes

In the Cox model, the regression coefficients describe relative hazards, while the baseline hazard or baseline survival function captures the underlying time pattern
for a reference covariate pattern with linear predictor equal to 0.

The baseline cumulative hazard is often estimated using a Breslow-type estimator, and the baseline survival function is then obtained as
`S_0(t) = exp(-H_0(t))`.
These quantities form the basis for individual survival predictions once a subject's linear predictor has been calculated.

In stratified models, separate baseline functions are estimated for each stratum.
As a result, baseline output should always be interpreted together with the stratum structure used during fitting.

Baseline quantities are model-based estimates and therefore inherit the assumptions of the fitted Cox model, including the proportional hazards structure.

### Example

```

=BESH.SURV.COX_BASELINE(F2,"survival")
```

## BESH.SURV.COX_DROP

Removes a fitted Cox model handle from memory.

**Function wizard:** Removes a fitted Cox model handle from memory.

### Syntax

`=BESH.SURV.COX_DROP(handle)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.

### Returns

TRUE if the handle existed and was removed successfully; otherwise FALSE.

### Notes

Cox model handles are stored only for the current Excel session so that related worksheet functions can reuse the fitted model without repeating the estimation step.
This function removes the cached model when it is no longer needed.

Removing unused handles can help reduce memory use in large workbooks or after repeated model fitting during exploratory analysis.
Once a handle has been removed, it can no longer be used by other Cox-related worksheet functions.

### Example

```

=BESH.SURV.COX_DROP(F2)
```

## BESH.SURV.COX_FIT

Fits a Cox proportional hazards regression model and returns a reusable model handle.

**Function wizard:** Fits a Cox proportional hazards model and returns a handle for use with other COX_* functions.

### Syntax

`=BESH.SURV.COX_FIT(time, status, x, varNames, strata, ties, robust, formula, formulaAddressing, maxIter, tol)`

### Parameters

- **time** — A single-column range containing observed follow-up times.
Values must be numeric and greater than or equal to 0.
Each row corresponds to one subject or observational unit.
- **status** — A single-column range containing event indicators.
Use 1 for an observed event and 0 for a right-censored observation.
The number of rows must match `time` and the predictor matrix.
- **x** — A numeric predictor matrix with one row per subject and one column per covariate.
All rows must align with `time` and `status`.
Each coefficient in the fitted model represents the effect of a one-unit increase in the corresponding covariate on the log hazard.
- **varNames** — Optional predictor names.
This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per predictor.
If omitted, default names such as X1, X2, … are assigned automatically.
- **strata** — Optional stratification variable supplied as a single-column range.
When used, the model allows each stratum to have its own baseline hazard function while estimating a common set of regression coefficients across strata.
Stratification is useful when baseline risk differs between groups but proportional effects of the covariates are still assumed within strata.
- **ties** — Optional method used to handle tied event times.
Accepted values are typically `breslow`, `efron`, and `exact`.
The Breslow approximation is simple and fast, Efron is usually more accurate when ties are present, and the exact method is most computationally intensive.
- **robust** — Optional logical flag indicating whether robust (sandwich) standard errors should be computed.
Robust standard errors can be useful when model assumptions are mildly violated or when greater protection against variance misspecification is desired.
- **formula** — Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix `x`.
Supported syntax currently includes additive terms (`A + B`), polynomial terms (`A^2`),
continuous-continuous interactions (`A:B`, `A:B:C`), categorical main effects such as
`factor(C)` or `factor(C, ref=2)`, categorical-continuous interactions such as
`factor(C):B`, and categorical-categorical interactions such as `factor(C):factor(D)`.
If omitted or blank, all predictor columns are used as continuous main effects.
- **formulaAddressing** — Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
Accepted values are `relative` (default), `absolute`, and `names`.
In `relative` mode, `A`, `B`, `AA`, … refer to columns 1, 2, 27, … of `x`.
In `absolute` mode, bare letters refer to worksheet columns of the supplied `x` range.
In `names` mode, bare letters are disabled and variables should be referenced using single-quoted names such as `'Age'`.
Quoted variable names are also allowed in the other two modes.
- **maxIter** — Optional maximum number of iterations allowed in the numerical optimization.
Increase this value if convergence is slow for more complex models.
- **tol** — Optional convergence tolerance controlling when the iterative fitting procedure stops.
Smaller values require a tighter convergence criterion but may increase computation time.

### Returns

A text handle identifying the fitted Cox model within the current Excel session.
This handle can be passed to other Cox-related worksheet functions to retrieve summaries, tests, and diagnostics without refitting the model.

### Notes

The Cox model estimates regression effects by maximizing the partial likelihood rather than specifying a full parametric distribution for survival times.
As a result, the method is flexible and widely used for time-to-event data with right censoring.

The sign of a coefficient indicates the direction of association with the hazard:
positive coefficients increase the hazard and therefore correspond to shorter expected survival, whereas negative coefficients decrease the hazard.
Exponentiating a coefficient gives the hazard ratio associated with a one-unit increase in the covariate.

When a formula is supplied, the model matrix is built internally from the raw predictor columns.
If `formulaAddressing="absolute"` is used, the `x` argument should be passed as a direct worksheet range so its absolute worksheet column letters can be determined.

Rows with invalid values, such as non-numeric times or predictors, are excluded before fitting.
If too few valid rows remain after filtering, the function returns an Excel error.

The returned handle is valid only for the current Excel session and should be treated as a temporary identifier rather than a permanent stored result.

### Example

```

=BESH.SURV.COX_FIT(A2:A101,B2:B101,C2:D101,"Age,Treatment")
=BESH.SURV.COX_FIT(A2:A101,B2:B101,C2:F101,"Age,BMI,Stage,Treat",,,"efron",FALSE,100,1E-8,"A + A^2 + factor(C, ref=1) + factor(C, ref=1):B","relative")
```

## BESH.SURV.COX_PRED

Returns predictions from a fitted Cox proportional hazards model for new covariate values.

**Function wizard:** Computes predictions from a fitted Cox model (linear predictor, risk, survival, or cumulative hazard).

### Syntax

`=BESH.SURV.COX_PRED(handle, newX, predType, timeGrid)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.
- **newX** — A numeric matrix containing one row per subject and one column per predictor.
The number and ordering of columns must match the predictor matrix used when the model was fitted.
- **predType** — Optional prediction type.
Common choices include `lp` for the linear predictor, `risk` for relative risk, `survival` for predicted survival probabilities,
and `cumhaz` for predicted cumulative hazard values.
- **timeGrid** — Optional time grid used for time-dependent predictions such as survival probabilities or cumulative hazards.
This should usually be supplied as a single-column numeric range.

### Returns

A spilled array containing the requested predictions.
For scalar predictions such as the linear predictor or relative risk, the output typically contains one row per subject.
For time-dependent predictions, the output may contain one row per time point or one block of results per subject, depending on the selected type.

### Notes

The linear predictor is the quantity `x'β`. It summarizes the combined effect of the subject's covariates on the log hazard scale.
Exponentiating the linear predictor gives the relative risk `exp(x'β)`, which compares a subject's hazard with the baseline hazard.

Predicted survival for subject `i` at time `t` is typically obtained as
`S_i(t) = S_0(t) ^ exp(x_i'β)`, where `S_0(t)` is the estimated baseline survival function.
Likewise, the predicted cumulative hazard is obtained by scaling the baseline cumulative hazard by `exp(x_i'β)`.

Predictions are conditional on the fitted model and therefore depend on the chosen ties method, any stratification, and the observed event structure in the estimation sample.
In stratified models, time-dependent predictions require the appropriate stratum-specific baseline function.

The `newX` argument should contain the raw predictor columns in the same order as the original model input.
When the fitted model uses internally expanded terms such as factors, polynomials, or interactions, the prediction path rebuilds the required design matrix automatically from those raw inputs.

Prediction functions are most meaningful when the new covariate values are within the practical range of the data used to fit the model.
Strong extrapolation beyond the observed predictor region should be interpreted cautiously.

### Example

```

=BESH.SURV.COX_PRED(F2,H2:I6,"risk")
```

## BESH.SURV.COX_RESID

Returns residual-based diagnostics for a fitted Cox proportional hazards model.

**Function wizard:** Returns residual diagnostics for a fitted Cox model.

### Syntax

`=BESH.SURV.COX_RESID(handle, residType)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.
- **residType** — The residual type to return.
Supported values commonly include `martingale`, `deviance`, `schoenfeld`, and `dfbeta`.

### Returns

A spilled array containing the requested residual output.
For observation-level residuals such as martingale or deviance residuals, the result is usually one row per subject.
For coefficient-specific diagnostics such as Schoenfeld residuals or DFBETA values, the result may contain one column per coefficient.

### Notes

Residuals provide diagnostic information about model fit, influential observations, and possible departures from assumptions.
Different residuals answer different questions and should be interpreted accordingly.

Martingale residuals are useful for assessing functional form and identifying outlying event patterns, but they are often highly skewed.
Deviance residuals are a transformation of martingale residuals that tends to be more symmetric and easier to inspect graphically.

Schoenfeld residuals are tied to event times and are especially useful for assessing the proportional hazards assumption.
DFBETA diagnostics quantify how strongly each observation influences each coefficient estimate.

Residuals should generally be interpreted together with the fitted model, subject-matter knowledge, and graphical inspection.
A single unusual residual value does not automatically imply model failure, but systematic patterns can indicate misspecification or influential data points.

### Example

```

=BESH.SURV.COX_RESID(F2,"martingale")
```

## BESH.SURV.COX_SUMMARY

Returns a coefficient table for a fitted Cox proportional hazards model.

**Function wizard:** Returns coefficient table (beta, SE, z, p, HR, CI) for a fitted Cox model handle.

### Syntax

`=BESH.SURV.COX_SUMMARY(handle, includeHeader, alpha)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.
- **includeHeader** — Optional logical flag indicating whether a header row should be included in the spilled output.
If omitted, a header row is included.
- **alpha** — Optional two-sided significance level used for the hazard-ratio confidence interval.
The default is `0.05`, corresponding to a 95% confidence interval.

### Returns

A spilled array with one row per predictor.
The output includes the variable name, regression coefficient, standard error, Wald z statistic, two-sided p-value,
hazard ratio, and a two-sided hazard-ratio confidence interval at level `1 - alpha`.

### Notes

The regression coefficient is the estimated change in log hazard associated with a one-unit increase in the predictor while holding the other predictors fixed.
The hazard ratio is obtained by exponentiating the coefficient and is often easier to interpret in applied work.

A hazard ratio greater than 1 indicates increased hazard, a hazard ratio less than 1 indicates reduced hazard, and a hazard ratio equal to 1 indicates no change.
For example, a hazard ratio of 1.25 corresponds to an estimated 25% increase in hazard per one-unit increase in the predictor.

The z statistic is formed by dividing the estimated coefficient by its standard error.
The reported p-value is the usual two-sided Wald p-value for testing whether the coefficient equals 0.

If robust standard errors were requested during model fitting, the summary uses those robust standard errors in place of the model-based standard errors.

### Example

```

=BESH.SURV.COX_SUMMARY(F2)
=BESH.SURV.COX_SUMMARY(F2, TRUE, 0.1)
```

## BESH.SURV.COX_TESTS

Returns global significance tests and fit statistics for a fitted Cox proportional hazards model.

**Function wizard:** Returns global tests (LR, Wald) and fit statistics for a fitted Cox model handle.

### Syntax

`=BESH.SURV.COX_TESTS(handle, includeHeader)`

### Parameters

- **handle** — A model handle previously returned by `BESH.SURV.COX_FIT`.
- **includeHeader** — Optional logical flag indicating whether a header row should be included in the spilled output.
If omitted, a header row is included.

### Returns

A spilled array containing global likelihood-ratio and Wald chi-square tests, their degrees of freedom and p-values,
together with additional fit information such as the null and fitted log-likelihoods, number of iterations, and convergence status.

### Notes

The likelihood-ratio test compares the fitted model with a model containing no predictors by comparing their log partial likelihood values.
It assesses whether the predictors, taken together, improve model fit.

The Wald test assesses the joint null hypothesis that all regression coefficients are equal to 0.
It is based on the estimated coefficient vector and its covariance matrix.

In many applications the likelihood-ratio, Wald, and score tests give similar conclusions, but they are not identical and may differ somewhat in small samples
or when the model is close to numerical instability.

Log-likelihood values are useful for model comparison. Larger fitted log-likelihood values indicate better agreement with the observed event ordering,
although comparisons are most meaningful between models fit to the same data.

The convergence indicator reports whether the iterative estimation procedure satisfied the stopping criterion before reaching the iteration limit.
Lack of convergence may indicate separation, collinearity, sparse information, or an overly complex model relative to the data.

### Example

```

=BESH.SURV.COX_TESTS(F2)
```

## BESH.SURV.KM_TABLE

Returns a tabular Kaplan–Meier survival curve.

**Function wizard:** Kaplan-Meier tabular survival curve: group, time, at-risk, S(t), SE, lower/upper CI.

### Syntax

`=BESH.SURV.KM_TABLE(time, status, group, alpha)`

### Parameters

- **time** — Single-column range of follow-up times (>= 0).
- **status** — Single-column range of event indicators (1=event, 0=censored).
- **group** — Optional single-column range of group IDs (text or numbers). When omitted, all observations are treated as one group.
- **alpha** — Optional two-sided significance level used for the Kaplan–Meier confidence interval.
The default is `0.05`, corresponding to a 95% confidence interval.

### Returns

A 2D array with one row per time point per group and 7 columns:
group, time, at risk, survival, SE, lower confidence limit, upper confidence limit.

### Notes

This function computes the Kaplan–Meier estimate of the survival function `S(t)`
for the full sample (when `group` is omitted) or separately for each group
(when `group` is provided).

The output includes, for each event/censoring time, the number at risk, the estimated survival
probability, Greenwood standard error, and a two-sided confidence interval at level `1 - alpha`.
Confidence limits are computed on a transformed scale to keep limits within `[0,1]`.

Input rules

- `time` must be a single-column range of non-negative times.
- `status` must be a single-column range coded as 1=event, 0=censored.
- `group` is optional; if provided, it must be a single-column range with the same number of rows as `time`.
- Rows with missing/non-numeric time or invalid status are ignored. If `group` is provided, blank group IDs are ignored.

Returned table (no header row):

- Col 1: Group ID
- Col 2: Time
- Col 3: At risk
- Col 4: S(t)
- Col 5: SE(S(t))
- Col 6: Lower confidence limit
- Col 7: Upper confidence limit

### Example

```

=BESH.SURV.KM_TABLE(A2:A200, B2:B200)
=BESH.SURV.KM_TABLE(A2:A200, B2:B200, C2:C200)
=BESH.SURV.KM_TABLE(A2:A200, B2:B200, C2:C200, 0.1)
```

## BESH.SURV.LOGRANK_P

Computes the p-value for a (possibly stratified) log-rank family test comparing survival curves across groups.

**Function wizard:** Log-rank family test p-value for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).

### Syntax

`=BESH.SURV.LOGRANK_P(timeRange, statusRange, groupRange, strataRange, weight)`

### Parameters

- **timeRange** — A single-column range containing follow-up times (time-to-event or time-to-censoring). Values must be ≥ 0.
- **statusRange** — A single-column range containing event indicators: 1 = event occurred, 0 = censored. Other values are invalid.
- **groupRange** — A single-column range containing group identifiers (text or numbers). Each distinct value defines a group.
- **strataRange** — Optional single-column range containing stratum identifiers (text or numbers) for stratified analysis.
If omitted, all rows are treated as belonging to a single stratum.
- **weight** — Optional weighting scheme for the log-rank family test:

- `"logrank"` — standard log-rank test (equal weights across time).
- `"gehan-breslow"` — Gehan–Breslow (generalized Wilcoxon) weights emphasize early events.
- `"tarone-ware"` — Tarone–Ware weights (intermediate emphasis on early events).
- `"peto"` — Peto–Peto weights based on the pooled Kaplan–Meier estimate just prior to each event time.
- `"modified peto"` — modified Peto weight with a small-sample adjustment.

The comparison is performed by accumulating weighted observed-minus-expected event counts over event times
(and across strata when provided) and evaluating the resulting chi-square statistic.

### Returns

The (upper-tail) p-value from a chi-square distribution with degrees of freedom equal to (number of groups − 1).
Returns `#VALUE!` for invalid range shapes or unknown weight names, and `#NUM!` for invalid data.

### Example

```

=BESH.SURV.LOGRANK_P(A2:A101, B2:B101, C2:C101)
=BESH.SURV.LOGRANK_P(A2:A101, B2:B101, C2:C101, D2:D101, "tarone-ware")
```

## BESH.SURV.LOGRANK_STAT

Computes the test statistic for a (possibly stratified) log-rank family test comparing survival curves across groups.

**Function wizard:** Log-rank family test chi-square statistic for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).

### Syntax

`=BESH.SURV.LOGRANK_STAT(timeRange, statusRange, groupRange, strataRange, weight)`

### Parameters

- **timeRange** — Single-column range of follow-up times (≥ 0).
- **statusRange** — Single-column range of event indicators: 1 = event, 0 = censored.
- **groupRange** — Single-column range of group identifiers (text or numbers).
- **strataRange** — Optional single-column range of stratum identifiers for stratified analysis.
If omitted, all rows are treated as belonging to a single stratum.
- **weight** — Optional weighting scheme: logrank, gehan-breslow, tarone-ware, peto, modified peto.

### Returns

The chi-square test statistic with degrees of freedom (number of groups − 1).
Returns `#VALUE!` for invalid range shapes or unknown weight names, and `#NUM!` for invalid data.

### Example

```

=BESH.SURV.LOGRANK_STAT(A2:A101, B2:B101, C2:C101, , "logrank")
```

## BESH.SURV.MEDIAN_CI

Computes the Kaplan–Meier median survival time and its Brookmeyer–Crowley confidence interval at level `1 - alpha`.

**Function wizard:** Kaplan–Meier median survival time with Brookmeyer–Crowley CI (overall or by group). Returns a 2D table.

### Syntax

`=BESH.SURV.MEDIAN_CI(time, status, group, alpha)`

### Parameters

- **time** — A single-column range containing follow-up times (time-to-event or time-to-censoring). Values must be ≥ 0.
- **status** — A single-column range containing event indicators: 1 = event occurred, 0 = censored. Other values are invalid.
- **group** — Optional single-column range of group identifiers (text or numbers).
If omitted, the median and confidence interval are computed for the whole sample and a single-row result is returned.
If provided, the result includes one row per group (based on distinct identifiers present in the input), allowing
quick comparison of group-specific medians.
- **alpha** — Optional two-sided significance level used for the Brookmeyer–Crowley confidence interval.
The default is `0.05`, corresponding to a 95% confidence interval.

### Returns

A 2D array with one row per group and the following columns:

- Column 0: Group identifier (or `ALL` when no group range is provided)
- Column 1: Median survival time (Kaplan–Meier estimate)
- Column 2: confidence interval lower bound
- Column 3: confidence interval upper bound

If the median (or CI bound) is not defined for a group (e.g., heavy censoring and the estimated survival curve never drops to 0.5),
the corresponding cell is returned as `#N/A`.

Returns `#VALUE!` for invalid range shapes (inputs must be single-column and have the same number of rows),
and `#NUM!` when there is insufficient valid data.

### Example

```

=BESH.SURV.MEDIAN_CI(A2:A101, B2:B101)
=BESH.SURV.MEDIAN_CI(A2:A101, B2:B101, C2:C101)
=BESH.SURV.MEDIAN_CI(A2:A101, B2:B101, C2:C101, 0.1)
```
