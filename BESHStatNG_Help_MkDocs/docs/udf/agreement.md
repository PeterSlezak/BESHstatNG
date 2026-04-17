# Agreement UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Bland Altman](../methods/bland-altman.md)
- [Deming Regression](../methods/deming-regression.md)
- [Intraclass Correlation Coefficients](../methods/intraclass-correlation-coefficients.md)
- [Cohens Kappa](../methods/cohens-kappa.md)
- [Lins Ccc](../methods/lins-ccc.md)
- [Passing Bablok Regression](../methods/passing-bablok-regression.md)

## BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS

Assesses whether the Bland–Altman bias confidence interval is acceptable relative to prespecified allowable limits.

**Function wizard:** Assess Bland–Altman bias against allowable limits on the active analysis scale.

### Syntax

`=BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS(x, y, lowerAllowableBias, upperAllowableBias, subjectIds, alpha, mode, scale, xAxis, ciMethod, bootstrapReplicates, useT, minSubjects, minPairsPerSubject, excludeSingletonSubjects, allowFallbackToSimple, checkProportionalBias, plotMode, randomSeed, varNames)`

### Parameters

- **x** — Reference-method values as a single-column range. Values must be paired row-by-row with `y`.
If the first cell looks like text, it is treated as a header.
- **y** — Test-method values as a single-column range. Values must be paired row-by-row with `x`.
If the first cell looks like text, it is treated as a header.
- **lowerAllowableBias** — Lower acceptable bias on the active Bland–Altman analysis scale.
- **upperAllowableBias** — Upper acceptable bias on the active Bland–Altman analysis scale.
- **subjectIds** — Optional subject identifiers aligned row-by-row with `x` and `y`.
Supply this to enable repeated-measures Bland–Altman assessment.
- **alpha** — Optional two-sided alpha used for the bias confidence interval. Default `0.05`.
- **mode** — Optional Bland–Altman mode: `auto`, `simple`, or `repeated`.
- **scale** — Optional difference scale: `raw`, `meanpct`, `refpct`, `testpct`, or `logratio`.
- **xAxis** — Optional x-axis convention: `mean`, `reference`, or `test`.
- **ciMethod** — Optional confidence-interval method: `analytical`, `jackknife`, `bootstrap`, or `bca`.
- **bootstrapReplicates** — Optional bootstrap replicate count. Default `2000`.
- **useT** — Optional TRUE/FALSE. When TRUE, analytical intervals use the Student-t critical value. Default TRUE.
- **minSubjects** — Optional minimum number of distinct subjects required for repeated mode. Default `2`.
- **minPairsPerSubject** — Optional minimum usable pairs per subject for repeated mode. Default `2`.
- **excludeSingletonSubjects** — Optional TRUE/FALSE. Default TRUE.
- **allowFallbackToSimple** — Optional TRUE/FALSE. Default TRUE.
- **checkProportionalBias** — Optional TRUE/FALSE. Default TRUE.
- **plotMode** — Optional repeated-measures plot mode: `all`, `means`, or `both`.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional method names as comma-separated text or a 1-row/1-column range.

### Returns

A labeled spill table reporting the fitted bias estimate and confidence interval on the selected analysis scale,
together with the allowable bias limits and the interval-based decision.

### Notes

This function is a decision-support companion to the Bland–Altman agreement analysis.
It first fits Bland–Altman on the requested scale and then compares the confidence interval for the mean bias with the
supplied allowable bias region `[L, U]`.

When the analysis scale is transformed, the allowable limits must be expressed on that same transformed scale:
for example percent limits for percent-difference analyses, or log-ratio limits for log-ratio analysis.

The assessment distinguishes whether only the point estimate lies inside the allowable region or whether the full confidence interval does.
The latter is the stricter and usually more defensible criterion in method-comparison studies.

### Example

```

=BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS(A2:A31,B2:B31,-2,2)
=BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS(A2:A31,B2:B31,-10,10,C2:C31,0.05,"repeated","meanpct")
```

## BESH.AGREE.BLANDALTMAN_DECISION

Assesses whether Bland–Altman bias and limits of agreement remain inside prespecified allowable limits.

**Function wizard:** Assess Bland–Altman bias and limits of agreement against allowable decision limits.

### Syntax

`=BESH.AGREE.BLANDALTMAN_DECISION(x, y, lowerAllowableLimit, upperAllowableLimit, subjectIds, alpha, mode, scale, xAxis, ciMethod, bootstrapReplicates, useT, minSubjects, minPairsPerSubject, excludeSingletonSubjects, allowFallbackToSimple, checkProportionalBias, plotMode, randomSeed, varNames)`

### Parameters

- **x** — Reference-method values as a single-column range.
- **y** — Test-method values as a single-column range.
- **lowerAllowableLimit** — Lower acceptable limit on the active Bland–Altman analysis scale.
- **upperAllowableLimit** — Upper acceptable limit on the active Bland–Altman analysis scale.
- **subjectIds** — Optional subject identifiers aligned row-by-row with `x` and `y` for repeated-measures Bland–Altman.
- **alpha** — Optional two-sided alpha. Default 0.05.
- **mode** — Optional Bland–Altman mode: `auto`, `simple`, or `repeated`.
- **scale** — Optional difference scale.
- **xAxis** — Optional x-axis convention.
- **ciMethod** — Optional confidence-interval method.
- **bootstrapReplicates** — Optional bootstrap replicate count.
- **useT** — Optional TRUE/FALSE. Default TRUE.
- **minSubjects** — Optional minimum subject count for repeated mode.
- **minPairsPerSubject** — Optional minimum usable pairs per subject.
- **excludeSingletonSubjects** — Optional TRUE/FALSE. Default TRUE.
- **allowFallbackToSimple** — Optional TRUE/FALSE. Default TRUE.
- **checkProportionalBias** — Optional TRUE/FALSE. Default TRUE.
- **plotMode** — Optional repeated-measures plot mode.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional method names.

### Returns

A labeled spill table reporting the fitted bias, lower limit of agreement, and upper limit of agreement,
together with their confidence intervals and the corresponding allowable-limits decision.

### Notes

This function extends ordinary Bland–Altman reporting into a prespecified decision framework.
It asks whether the observed bias and the agreement limits are acceptably small for the intended use.

Let `[L, U]` be the acceptable region on the active analysis scale. The function reports:

- whether the fitted bias confidence interval lies entirely inside `[L, U]`
- whether the observed lower and upper limits of agreement lie inside `[L, U]`
- whether the confidence interval for the lower limit stays above `L` and the confidence interval for the upper limit stays below `U`

When repeated-measures or transformed-scale Bland–Altman is requested, the allowable limits must be given on that same effective scale.

### Example

```

=BESH.AGREE.BLANDALTMAN_DECISION(A2:A31,B2:B31,-5,5)
=BESH.AGREE.BLANDALTMAN_DECISION(A2:A31,B2:B31,-15,15,C2:C31,0.05,"repeated","meanpct")
```

## BESH.AGREE.BLANDALTMAN_FIT

Returns a spillable labeled result table for Bland–Altman agreement analysis.

**Function wizard:** Bland–Altman analysis for two paired methods. Returns a labeled result table.

### Syntax

`=BESH.AGREE.BLANDALTMAN_FIT(x, y, subjectIds, alpha, mode, scale, xAxis, ciMethod, bootstrapReplicates, useT, minSubjects, minPairsPerSubject, excludeSingletonSubjects, allowFallbackToSimple, checkProportionalBias, plotMode, randomSeed, varNames)`

### Parameters

- **x** — One-column range of reference-method values. Values are paired row-by-row with `y`.
- **y** — One-column range of test-method values. Values are paired row-by-row with `x`.
- **subjectIds** — Optional one-column subject or sample identifiers aligned row-by-row with the paired measurements.
Supply this when repeated paired measurements exist for the same subject and repeated-measures Bland–Altman analysis is desired.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **mode** — Optional analysis mode: `auto`, `simple`, or `repeated`.
- **scale** — Optional difference scale: `raw`, `meanpct`, `refpct`, `testpct`, or `logratio`.
These correspond respectively to
`d_i = y_i - x_i`,
`100*(y_i-x_i)/((x_i+y_i)/2)`,
`100*(y_i-x_i)/x_i`,
`100*(y_i-x_i)/y_i`, and
`ln(y_i/x_i)`.
- **xAxis** — Optional x-axis convention: `mean`, `reference`, or `test`.
- **ciMethod** — Optional confidence-interval method: `analytical`, `jackknife`, `bootstrap`, or `bca`.
- **bootstrapReplicates** — Optional bootstrap replicate count. Default 2000.
- **useT** — Optional TRUE/FALSE. If TRUE, analytical and jackknife intervals use the Student-t critical value where applicable.
- **minSubjects** — Optional minimum subject count required for repeated-measures mode. Default 2.
- **minPairsPerSubject** — Optional minimum number of paired observations required for a subject to contribute to repeated-measures mode. Default 2.
- **excludeSingletonSubjects** — Optional TRUE/FALSE. If TRUE, singleton subjects are excluded from repeated-measures estimation.
- **allowFallbackToSimple** — Optional TRUE/FALSE. If TRUE, repeated-mode requests may fall back to ordinary paired Bland–Altman when repeated-data requirements are not met.
- **checkProportionalBias** — Optional TRUE/FALSE. If TRUE, tests for a linear trend of the differences against the chosen x-axis quantity.
- **plotMode** — Optional repeated-measures plot mode: `all`, `means`, or `both`.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional display names for the two methods as comma-separated text or a small range.

### Returns

A two-dimensional spill range containing a full Bland–Altman worksheet-style report.

### Notes

Bland–Altman analysis is an agreement method, not a correlation method. For ordinary paired data the key summaries are:

- Bias = mean of the paired differences `d_i`
- Standard deviation of differences = sample standard deviation of `d_i`
- Limits of agreement = `bias +/- 1.96 * SD(d)` on the chosen scale

The function can also run a repeated-measures version when `subjectIds` is supplied, allowing within-subject variation
to drive the agreement limits rather than treating every row as fully independent.

Important assumptions depend on the chosen scale:

- `raw`: disagreement is interpreted in the original measurement units.
- `meanpct`, `refpct`, and `testpct`: denominators must be non-zero for retained rows.
- `logratio`: both methods must be strictly positive for retained rows.
- Rows must represent paired measurements on the same item in the same order.

Missing or invalid paired rows are removed pairwise before analysis.

## BESH.AGREE.BLANDALTMAN_PLOTDATA

Returns the x-y pairs required to draw a Bland–Altman plot in the worksheet.

**Function wizard:** Bland–Altman plot data (observation and subject-mean coordinates).

### Syntax

`=BESH.AGREE.BLANDALTMAN_PLOTDATA(x, y, subjectIds, mode, scale, xAxis, plotMode, randomSeed)`

### Parameters

- **x** — One-column range of reference-method values.
- **y** — One-column range of test-method values.
- **subjectIds** — Optional subject or sample IDs for repeated-measures mode.
- **mode** — Optional analysis mode: `auto`, `simple`, or `repeated`.
- **scale** — Optional difference scale.
- **xAxis** — Optional x-axis convention.
- **plotMode** — Optional repeated-measures plot mode: `all`, `means`, or `both`.
- **randomSeed** — Optional integer seed for reproducible bootstrap-enabled workflows.

### Returns

A spill range containing the selected x-axis values, plotted differences, and horizontal reference values needed to draw the bias and limits-of-agreement lines.

### Notes

In a classical Bland–Altman plot the x-axis is usually the mean of the two methods and the y-axis is the paired difference.
This function lets you request alternative x-axis conventions and repeated-measures subject-mean plotting modes while still returning worksheet-friendly plot coordinates.

## BESH.AGREE.BLANDALTMAN_STATS

Returns a compact numerical summary of Bland–Altman statistics.

**Function wizard:** Bland–Altman bias and limits of agreement for two paired methods.

### Syntax

`=BESH.AGREE.BLANDALTMAN_STATS(x, y, subjectIds, alpha, mode, scale, xAxis, ciMethod, bootstrapReplicates, randomSeed)`

### Parameters

- **x** — One-column range of reference-method values.
- **y** — One-column range of test-method values.
- **subjectIds** — Optional one-column subject IDs aligned with `x` and `y` for repeated-measures Bland–Altman.
- **alpha** — Optional two-sided significance level. Default 0.05.
- **mode** — Optional analysis mode: `auto`, `simple`, or `repeated`.
- **scale** — Optional difference scale.
- **xAxis** — Optional x-axis convention.
- **ciMethod** — Optional confidence-interval method.
- **bootstrapReplicates** — Optional bootstrap replicate count.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.

### Returns

A compact spill range containing the core agreement quantities such as bias, lower limit of agreement, upper limit of agreement,
standard deviation of differences, and corresponding confidence intervals.

### Notes

This is the compact numeric companion to `BESH.AGREE.BLANDALTMAN_FIT`. It applies the same agreement model but returns a smaller
result intended for formulas and downstream worksheet calculations rather than a full report layout.

## BESH.AGREE.DEMING_COEF

Returns Deming or weighted Deming regression coefficients together with their confidence intervals.

**Function wizard:** Weighted / generalized Deming regression coefficients and confidence intervals.

### Syntax

`=BESH.AGREE.DEMING_COEF(x, y, alpha, lambda, ciMethod, varianceModel, fitIntercept, sdX, sdY, cvX, cvY, bootstrapReplicates, randomSeed)`

### Parameters

- **x** — One-column range of reference-method values.
- **y** — One-column range of test-method values.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **lambda** — Optional constant error ratio `lambda = sigma_x^2 / sigma_y^2` for the classical Deming model.
- **ciMethod** — Optional confidence-interval method: `analytical`, `jackknife`, `bootstrap`, or `bca`.
- **varianceModel** — Optional variance model: `lambda`, `pointwise`, or `cv`.
- **fitIntercept** — Optional TRUE/FALSE. If FALSE, fits through the origin.
- **sdX** — Optional row-specific standard deviations for `x` when using the pointwise variance model.
- **sdY** — Optional row-specific standard deviations for `y` when using the pointwise variance model.
- **cvX** — Optional coefficient of variation for `x` when using the constant-CV variance model.
- **cvY** — Optional coefficient of variation for `y` when using the constant-CV variance model.
- **bootstrapReplicates** — Optional bootstrap replicate count.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.

### Returns

A compact spill range containing slope and intercept estimates with lower and upper confidence limits.

### Notes

This is the coefficient-focused companion to `BESH.AGREE.DEMING_FIT`. It fits the same model family but returns only
the estimated slope and intercept together with interval estimates.

The slope quantifies proportional difference between methods; the intercept quantifies systematic offset.
When `fitIntercept` is FALSE, the fitted line is constrained to pass through the origin.

## BESH.AGREE.DEMING_FIT

Returns a spillable labeled result table for Deming or weighted Deming regression.

**Function wizard:** Weighted / generalized Deming regression for two paired methods. Returns a labeled result table.

### Syntax

`=BESH.AGREE.DEMING_FIT(x, y, alpha, lambda, ciMethod, varianceModel, fitIntercept, sdX, sdY, cvX, cvY, bootstrapReplicates, randomSeed, varNames)`

### Parameters

- **x** — One-column range containing the reference-method measurements. Values are paired row-by-row with `y`.
- **y** — One-column range containing the test-method measurements. Values are paired row-by-row with `x`.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **lambda** — Optional constant error ratio `lambda = sigma_x^2 / sigma_y^2` used when `varianceModel` is `lambda`.
`lambda = 1` yields orthogonal regression. Larger values place relatively more error on the `x`-axis than on the `y`-axis.
- **ciMethod** — Optional confidence-interval method. Accepted tokens are `analytical`, `jackknife`, `bootstrap`, and `bca`.
- **varianceModel** — Optional variance model. Use:

- `lambda` for classical Deming regression with constant error ratio `lambda`
- `pointwise` when you supply row-specific standard deviations through `sdX` and `sdY`
- `cv` when measurement error is assumed proportional to magnitude and you supply constant coefficients of variation through `cvX` and `cvY`
- **fitIntercept** — Optional TRUE/FALSE. If TRUE, fits `y = a + b x`. If FALSE, forces the line through the origin and fits `y = b x`.
- **sdX** — Optional one-column range of row-specific standard deviations for `x`. Required for `varianceModel` = `pointwise`.
- **sdY** — Optional one-column range of row-specific standard deviations for `y`. Required for `varianceModel` = `pointwise`.
- **cvX** — Optional coefficient of variation for `x`. Required for `varianceModel` = `cv`.
- **cvY** — Optional coefficient of variation for `y`. Required for `varianceModel` = `cv`.
- **bootstrapReplicates** — Optional bootstrap replicate count used by bootstrap-based intervals. Default 2000.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional display names for the two methods as comma-separated text or a small range.

### Returns

A two-dimensional spill range containing the fitted coefficients, confidence intervals, diagnostics, and model settings.

### Notes

Deming regression is an errors-in-variables method-comparison regression. Unlike ordinary least squares, it acknowledges
that both axes may be measured with error. The fitted line has the form `y = a + b x` unless the intercept is fixed at zero.

In the constant-ratio model, the procedure minimizes squared orthogonal deviations weighted according to the ratio
`lambda = sigma_x^2 / sigma_y^2`. In the pointwise-SD model, each row has its own measurement precision. In the constant-CV model,
the standard deviation is taken to be proportional to the absolute measurement magnitude, approximately
`sd_x,i = CV_x * |x_i|` and `sd_y,i = CV_y * |y_i|`.

Typical assumptions are:

- The paired rows represent the same items or subjects in the same order.
- The relation between methods is approximately linear over the observed range.
- The chosen error model (`lambda`, `pointwise`, or `cv`) is a reasonable description of the measurement process.
- Missing or non-numeric paired rows are removed pairwise; if pointwise standard deviations are supplied, they must be available for every retained row.

Use this function when you want the full formatted worksheet report. Use `BESH.AGREE.DEMING_COEF`
when you only want slope and intercept estimates with confidence intervals.

## BESH.AGREE.ICC_FIT

Returns a spillable labeled result table for an intraclass correlation coefficient (ICC) model.

**Function wizard:** Intraclass correlation coefficient (ICC) result table for a selected ICC model.

### Syntax

`=BESH.AGREE.ICC_FIT(data, model, alpha, includeRepeatability)`

### Parameters

- **data** — Numeric matrix containing repeated measurements.
- **model** — ICC model identifier. Supported values are:

- `ICC11` or `ICC(1,1)` — one-way random effects, single measurement.
- `ICC1K` or `ICC(1,k)` — one-way random effects, mean of k measurements.
- `ICC21` or `ICC(2,1)` — two-way random effects, absolute agreement, single measurement.
- `ICC2K` or `ICC(2,k)` — two-way random effects, absolute agreement, mean of k measurements.
- `ICC31` or `ICC(3,1)` — two-way mixed effects, consistency, single measurement.
- `ICC3K` or `ICC(3,k)` — two-way mixed effects, consistency, mean of k measurements.

The default is `ICC21`.
- **alpha** — Optional two-sided significance level used for the confidence interval.
The default is `0.05`, corresponding to a 95% confidence interval.
- **includeRepeatability** — TRUE to append the repeatability coefficient (RC) and SEM to the spill output.
FALSE returns the ICC estimate and its confidence interval only.
The default is FALSE.

### Returns

A two-dimensional spill range containing a labeled result table for the requested ICC model.
Returns `#VALUE!` when the input shape is invalid or contains incompatible non-numeric cells.
Returns `#NUM!` when the requested ICC cannot be estimated from the supplied data.

### Notes

Intraclass correlation coefficients measure the reliability or agreement of repeated measurements made on the same targets.
The meaning of the coefficient depends on the selected model:

- ICC(1,·) assumes a one-way random-effects design where rows are targets and columns are exchangeable repeated measurements. Missing cells are allowed; each row may contain a different number of usable measurements.
- ICC(2,·) assumes a complete balanced two-way random-effects design with rows = targets and columns = raters/replicates. This is the usual choice for absolute agreement when raters are considered random.
- ICC(3,·) assumes a complete balanced two-way mixed-effects design with rows = targets and columns = fixed raters/replicates. This is the usual choice for consistency rather than absolute agreement.

The one-way single-measure coefficient uses the form
`ICC(1,1) = (MSB - MSW) / (MSB + (n0 - 1) MSW)`,
where `MSB` is the between-target mean square, `MSW` is the within-target mean square,
and `n0` is the effective group size for unbalanced data.

The two-way random-effects single-measure coefficient uses
`ICC(2,1) = (MSR - MSE) / (MSR + (k-1)MSE + k(MSC - MSE)/n)`,
where `MSR` is the target mean square, `MSC` is the rater mean square,
`MSE` is the residual mean square, `n` is the number of targets, and `k` is the number of raters.

The two-way mixed-effects consistency single-measure coefficient uses
`ICC(3,1) = (MSR - MSE) / (MSR + (k-1)MSE)`.
The average-measure versions transform these models to the reliability of the mean of k measurements.

Confidence intervals are F-based and therefore generally asymmetric. Lower confidence limits may be negative.

Data layout:

- For ICC(1,1) and ICC(1,k), each row is one target/subject and each column is a repeated measurement. Blank cells are allowed and are treated as missing.
- For ICC(2,1), ICC(2,k), ICC(3,1), and ICC(3,k), the matrix must be complete and balanced: one numeric value in every row × column cell.
- An optional single top header row containing text labels is allowed and is ignored for the calculation.

## BESH.AGREE.ICC_RC

Returns the repeatability coefficient (RC), its confidence interval, and the SEM for a selected ICC design.

**Function wizard:** Repeatability coefficient (RC) and SEM for a selected ICC design.

### Syntax

`=BESH.AGREE.ICC_RC(data, model, alpha)`

### Parameters

- **data** — Numeric matrix of repeated measurements. For one-way ICC models, blank cells are allowed and are treated as missing.
For two-way ICC models, the matrix must be complete and balanced.
- **model** — ICC model identifier. The repeatability calculation is mapped as follows:

- `ICC11` → one-way single-measure RC and SEM.
- `ICC1K` → one-way average-measure RC and SEM.
- `ICC21` → two-way absolute-agreement single-measure RC and SEM.
- `ICC2K` → two-way absolute-agreement average-measure RC and SEM.
- `ICC31` → two-way consistency single-measure RC and SEM.
- `ICC3K` → two-way consistency average-measure RC and SEM.

The default is `ICC21`.
- **alpha** — Optional two-sided significance level used for the confidence interval.
The default is `0.05`.

### Returns

A spillable labeled table containing the repeatability coefficient, its confidence interval, and the SEM.
Returns `#VALUE!` when the input is invalid and `#NUM!` when the repeatability quantity is not estimable from the supplied data.

### Notes

The repeatability coefficient summarizes the expected absolute difference between repeated measurements on the same target.
It is defined as
`RC = z_(1-α/2) × √2 × SEM`,
where `SEM` is the standard error of measurement implied by the selected ICC design.

For one-way ICC models, `SEM` is based on the within-target variance component. For average-measure models the variance of the mean is used.
For two-way models, the repeatability calculation can either include rater variance (absolute agreement) or exclude it (consistency), again with optional averaging across k raters.

This function is useful when you need an agreement quantity in measurement units rather than a unitless reliability coefficient.

## BESH.AGREE.ICC_VALUE

Returns only the numerical value of an intraclass correlation coefficient (ICC) model.

**Function wizard:** Point estimate of a selected intraclass correlation coefficient (ICC) model.

### Syntax

`=BESH.AGREE.ICC_VALUE(data, model, alpha)`

### Parameters

- **data** — Numeric matrix of repeated measurements. For one-way ICC models, blank cells are allowed and are treated as missing.
For two-way ICC models, the matrix must be complete and balanced.
- **model** — ICC model identifier: `ICC11`, `ICC1K`, `ICC21`, `ICC2K`, `ICC31`, or `ICC3K`.
The default is `ICC21`.
- **alpha** — Optional two-sided significance level.
The alpha value affects the confidence interval used internally by the fit routine but this function returns only the point estimate.
The default is 0.05.

### Returns

The point estimate of the requested ICC model as a scalar numeric value.
Returns `#VALUE!` for invalid input and `#NUM!` when the model is not estimable from the supplied data.

### Notes

This function is intended for formulas that need only the ICC estimate itself. The underlying formulas depend on the selected ICC family:

- `ICC(1,1)` — one-way random-effects single-measure reliability.
- `ICC(1,k)` — one-way random-effects average-measure reliability.
- `ICC(2,1)` — two-way random-effects single-measure absolute agreement.
- `ICC(2,k)` — two-way random-effects average-measure absolute agreement.
- `ICC(3,1)` — two-way mixed-effects single-measure consistency.
- `ICC(3,k)` — two-way mixed-effects average-measure consistency.

Values near 1 indicate strong reliability / agreement; values near 0 indicate weak reliability; negative values can occur when within-target variation dominates between-target variation.

## BESH.AGREE.KAPPA_FIT

Returns a spillable labeled result table for Cohen's kappa or weighted kappa for paired ratings.

**Function wizard:** Cohen's / weighted kappa for two paired rating columns. Returns a labeled result table.

### Syntax

`=BESH.AGREE.KAPPA_FIT(rater1, rater2, alpha, weighting, ciMethod, categories, bootstrapReplicates, randomSeed, varNames)`

### Parameters

- **rater1** — One-column range containing the first set of paired ratings. The first cell may be a header.
- **rater2** — One-column range containing the second set of paired ratings. The first cell may be a header.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **weighting** — Optional weighting scheme. Common choices are `unweighted`, `linear`, `quadratic`, `cicchetti`, and `fleiss`.
In theory a `custom` scheme exists, but this UDF does not expose a custom weight matrix argument, so a user-specified custom matrix is not available here.
- **ciMethod** — Optional confidence-interval method: `analytical`, `bootstrap`, or `bca`.
- **categories** — Optional ordered category list supplied as comma-separated text or a small range. Use this when the category order matters for weighted kappa,
for example ordinal scales such as `Low,Medium,High`.
- **bootstrapReplicates** — Optional bootstrap replicate count. Default 2000.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional display names for the two raters or methods.

### Returns

A two-dimensional spill range containing a full kappa report.

### Notes

Unweighted Cohen's kappa is
`kappa = (P_o - P_e) / (1 - P_e)`,
where `P_o` is the observed agreement proportion and `P_e` is the expected agreement under independence of the two raters.

Weighted kappa generalizes this to
`kappa_w = (P_o^w - P_e^w) / (1 - P_e^w)`,
where disagreements closer to the diagonal receive partial credit through a weight matrix.

Use weighted kappa for ordinal categories where near-disagreements are less severe than far-apart disagreements.
Missing or blank category pairs are removed pairwise before fitting.

## BESH.AGREE.KAPPA_VALUE

Returns the numerical value of Cohen's kappa or weighted kappa together with interval information.

**Function wizard:** Cohen's / weighted kappa value for two paired rating columns.

### Syntax

`=BESH.AGREE.KAPPA_VALUE(rater1, rater2, alpha, weighting, ciMethod, categories, bootstrapReplicates, randomSeed)`

### Parameters

- **rater1** — One-column range containing the first set of paired ratings.
- **rater2** — One-column range containing the second set of paired ratings.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **weighting** — Optional weighting scheme.
- **ciMethod** — Optional confidence-interval method.
- **categories** — Optional ordered category list for ordinal weighting.
- **bootstrapReplicates** — Optional bootstrap replicate count.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.

### Returns

A compact spill range containing the kappa estimate, confidence interval, and key scalar summaries.

### Notes

Kappa measures agreement beyond chance for paired categorical ratings on the same items.
Values near 1 indicate strong agreement, values near 0 indicate agreement comparable to chance, and negative values indicate worse-than-chance agreement.

Weighted versions are intended for ordinal ratings where disagreements have different severities.

## BESH.AGREE.LINCCC_FIT

Returns a spillable labeled result table for Lin's concordance correlation coefficient (CCC).

**Function wizard:** Lin's concordance correlation coefficient for two paired methods. Returns a labeled result table.

### Syntax

`=BESH.AGREE.LINCCC_FIT(x, y, alpha, ciMethod, nullConcordance, bootstrapReplicates, randomSeed, varNames)`

### Parameters

- **x** — One-column range of reference-method values paired row-by-row with `y`.
- **y** — One-column range of test-method values paired row-by-row with `x`.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.
- **ciMethod** — Optional confidence-interval method: `analytical`, `jackknife`, `bootstrap`, or `bca`.
- **nullConcordance** — Optional null concordance value used for the hypothesis test. Default 0.
- **bootstrapReplicates** — Optional bootstrap replicate count. Default 2000.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.
- **varNames** — Optional display names for the two methods.

### Returns

A two-dimensional spill range containing a full Lin CCC report.

### Notes

Lin's concordance correlation coefficient combines precision and accuracy into a single agreement statistic:
`rho_c = rho * C_b`, where `rho` is Pearson correlation and `C_b` is a bias-correction factor that shrinks the value when the fitted points deviate from the 45-degree line of equality.

An equivalent moment-based formula is
`rho_c = 2*s_xy / (s_x^2 + s_y^2 + (xbar - ybar)^2)`.

Use this function when you want a full worksheet report containing the concordance estimate, decomposition into precision and accuracy,
confidence interval, and hypothesis test. Missing or non-numeric pairs are removed pairwise before fitting.

The procedure is intended for paired continuous measurements on the same items. It does not replace a full repeated-measures concordance model.

## BESH.AGREE.LINCCC_VALUE

Returns the numerical value of Lin's concordance correlation coefficient together with interval information.

**Function wizard:** Lin's concordance correlation coefficient value for two paired methods.

### Syntax

`=BESH.AGREE.LINCCC_VALUE(x, y, alpha, ciMethod, nullConcordance, bootstrapReplicates, randomSeed)`

### Parameters

- **x** — One-column range of reference-method values.
- **y** — One-column range of test-method values.
- **alpha** — Optional two-sided significance level. Default 0.05.
- **ciMethod** — Optional confidence-interval method.
- **nullConcordance** — Optional null concordance value for the associated hypothesis test.
- **bootstrapReplicates** — Optional bootstrap replicate count.
- **randomSeed** — Optional integer seed for reproducible bootstrap resampling.

### Returns

A compact spill range containing the concordance estimate, confidence interval, and related scalar summaries.

### Notes

This is the compact numeric companion to `BESH.AGREE.LINCCC_FIT`. The main estimand is
`rho_c = rho * C_b`, where values close to 1 indicate strong agreement and values near 0 indicate poor concordance.

## BESH.AGREE.PASSINGBABLOK_COEF

Returns the Passing–Bablok slope and intercept together with their confidence intervals.

**Function wizard:** Passing–Bablok regression coefficients and confidence intervals for two paired methods.

### Syntax

`=BESH.AGREE.PASSINGBABLOK_COEF(x, y, groups, alpha)`

### Parameters

- **x** — One-column range of reference-method values, paired row-by-row with `y`.
- **y** — One-column range of test-method values, paired row-by-row with `x`.
- **groups** — Optional one-column grouping / subject range aligned with `x` and `y` for grouped / block Passing–Bablok analysis.
- **alpha** — Optional two-sided significance level used for confidence intervals. Default 0.05.

### Returns

A compact spill range containing slope and intercept estimates with lower and upper confidence limits.

### Notes

Passing–Bablok regression fits the robust method-comparison line `y = a + b x` by using medians of pairwise slopes
rather than least squares. It is designed for paired measurements where both methods may contain measurement error.

The reported slope describes proportional differences between methods; the reported intercept describes systematic offset.
Confidence intervals are distribution-free in the sense that they do not rely on normal residual assumptions.

Missing or non-numeric rows are removed pairwise. If `groups` is supplied, the group range must align row-by-row
with the paired numeric data.

## BESH.AGREE.PASSINGBABLOK_FIT

Returns a spillable labeled result table for Passing–Bablok method-comparison regression.

**Function wizard:** Passing–Bablok regression for two paired methods. Returns a labeled result table.

### Syntax

`=BESH.AGREE.PASSINGBABLOK_FIT(x, y, groups, alpha, varNames, groupName)`

### Parameters

- **x** — One-column range containing the reference-method measurements. The values must be paired row-by-row with `y`.
If the first cell looks like text, it is treated as a header rather than data.
- **y** — One-column range containing the test-method measurements. The values must be paired row-by-row with `x`.
If the first cell looks like text, it is treated as a header rather than data.
- **groups** — Optional one-column grouping or subject range aligned row-by-row with `x` and `y`.
When supplied, the function performs grouped / block Passing–Bablok regression so that paired observations can be handled within subject or block structure.
- **alpha** — Optional two-sided significance level used for confidence intervals. The default is 0.05, corresponding to 95% confidence intervals.
- **varNames** — Optional display names for the two methods. Supply either a comma-separated string such as `"Reference,Test"`
or a one-row / one-column range with two names.
- **groupName** — Optional display name for the grouping variable shown in the output when `groups` is supplied.

### Returns

A two-dimensional spill range containing a labeled result table with the fitted slope and intercept, interval estimates,
and additional method-comparison diagnostics.

### Notes

Passing–Bablok regression is a robust, non-parametric linear method-comparison procedure for paired measurements.
The fitted line has the form `y = a + b x`.

The slope is estimated from the median of all admissible pairwise slopes
`(y_j - y_i) / (x_j - x_i)` with `x_j <> x_i`, and the intercept is estimated as the median of
`y_i - b x_i`. This makes the method resistant to moderate outliers and removes the need to assume
a normal distribution of residuals.

Typical assumptions are:

- `x` and `y` measure the same items or subjects in the same row order.
- The relation is approximately linear and monotone over the observed range.
- Pairs are independent unless an explicit grouped / block analysis is requested through `groups`.
- Missing or non-numeric rows are removed pairwise; if `groups` is supplied, a retained numeric pair must also have a non-empty group label.

Use this function when you want the full formatted worksheet report. Use `BESH.AGREE.PASSINGBABLOK_COEF`
when you only want the fitted coefficients and their interval estimates.
