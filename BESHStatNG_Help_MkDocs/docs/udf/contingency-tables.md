# Contingency Tables UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [2X2 Table](../methods/2x2-table.md)
- [Mantel Haenszel Test](../methods/mantel-haenszel-test.md)
- [Proportions](../methods/proportions.md)
- [Rxc Table](../methods/rxc-table.md)

## BESH.CT.CHI2

Pearson chi-square test of independence for an `r×c` contingency table.

**Function wizard:** Pearson chi-square test of independence for an r×c contingency table.

### Syntax

`=BESH.CT.CHI2(table)`

### Parameters

- **table** — A numeric matrix of non-negative cell counts.
Rows represent categories of one variable and columns represent categories of the second variable.
An optional single header row containing non-numeric labels is allowed and will be ignored.

### Returns

A labeled result table containing the Pearson chi-square statistic,
the associated degrees of freedom,
and the upper-tail p-value from the chi-square distribution.
Returns `#VALUE!` when the input is not a valid non-negative integer matrix.
Returns `#NUM!` when the table has fewer than two rows or fewer than two columns,
when the total count is zero,
or when the statistic cannot be evaluated numerically.

### Notes

This function tests the null hypothesis that row membership and column membership are statistically independent.
Let `O_ij` denote the observed count in cell `(i,j)`,
let `R_i` and `C_j` denote the row and column totals,
and let `N` denote the grand total.
Under independence the expected count is
`E_ij = R_i C_j / N`.
The test statistic is
`X² = Σ_ij (O_ij - E_ij)² / E_ij`.

The reported p-value is based on the asymptotic chi-square reference distribution with
`(r-1)(c-1)` degrees of freedom after excluding structurally empty all-zero rows or columns.
The approximation is most reliable when expected counts are not too small.
For sparse tables or very small samples, consider the exact Fisher-Freeman-Halton procedure instead.

### Example

```

=BESH.CT.CHI2(A1:C4)
```

## BESH.CT.FFH_EXACT

Fisher-Freeman-Halton exact test for a general `r×c` contingency table.

**Function wizard:** Fisher-Freeman-Halton exact test for a general r×c contingency table.

### Syntax

`=BESH.CT.FFH_EXACT(table)`

### Parameters

- **table** — A numeric matrix of non-negative cell counts with at least two rows and two columns.
An optional single top header row containing non-numeric labels is allowed and will be ignored.

### Returns

A labeled result table containing the observed-table probability under the conditional null distribution
and the exact two-sided p-value.
Returns `#VALUE!` when the input is not a valid count matrix.
Returns `#NUM!` when the exact network calculation fails because the table is too large or too sparse for the available workspace.

### Notes

This procedure generalizes Fisher's exact test from `2×2` tables to arbitrary fixed-margin `r×c` tables.
Conditional on the observed row and column margins,
the null distribution assigns probability proportional to
`1 / Π_ij O_ij!`
up to a margin-dependent normalizing constant.
The exact p-value sums the probabilities of all feasible tables whose conditional probability is no greater than that of the observed table.

Because the number of feasible tables can be very large,
the calculation uses a network-style enumeration algorithm rather than naive brute-force generation.
Exact inference is particularly useful when asymptotic chi-square approximations are doubtful because expected counts are small.

### Example

```

=BESH.CT.FFH_EXACT(A1:D5)
```

## BESH.CT.FISHER_2X2

Fisher's exact test for a `2×2` contingency table.

**Function wizard:** Fisher's exact test for a 2×2 contingency table, including mid-p values.

### Syntax

`=BESH.CT.FISHER_2X2(table)`

### Parameters

- **table** — A `2×2` matrix of non-negative counts.
The cells are interpreted as
`a = table(1,1)`, `b = table(1,2)`, `c = table(2,1)`, and `d = table(2,2)`.
An optional single top header row containing non-numeric labels is allowed and will be ignored.

### Returns

A labeled result table containing one-sided and two-sided exact p-values,
together with the corresponding mid-p versions.
Returns `#VALUE!` when the supplied range is not a valid `2×2` count table.
Returns `#NUM!` when the exact probabilities cannot be evaluated numerically.

### Notes

Conditional on the fixed row and column totals,
the upper-left cell of a `2×2` table follows a hypergeometric distribution under the null hypothesis of independence.
Fisher's exact procedure sums the probabilities of all tables at least as extreme as the observed table,
thereby avoiding the large-sample chi-square approximation.

The mid-p values subtract one half of the observed-table probability before doubling or tail summation.
Mid-p procedures are often less conservative than fully exact p-values,
but they are no longer guaranteed to control the type-I error rate in the strict conditional-exact sense.

This function is appropriate when sample sizes are small,
expected counts are sparse,
or exact conditional inference is preferred for a `2×2` design.

### Example

```

=BESH.CT.FISHER_2X2(A1:B3)
```

## BESH.CT.MANTEL_HAENSZEL

Mantel-Haenszel pooled analysis across multiple stratified `2×2` tables.

**Function wizard:** Mantel-Haenszel pooled test and common odds ratio across stacked 2×2 strata.

### Syntax

`=BESH.CT.MANTEL_HAENSZEL(stackedTables, alpha)`

### Parameters

- **stackedTables** — A numeric matrix with exactly two columns and an even number of rows.
Every consecutive pair of rows represents one stratum-specific `2×2` table in the form
`[a,b]` on the first row and `[c,d]` on the second row.
An optional single top header row containing non-numeric labels is allowed and will be ignored.
- **alpha** — Two-sided significance level for the pooled common-odds-ratio confidence interval.
The default is `0.05`.

### Returns

A labeled result table containing the Mantel-Haenszel chi-square statistic,
its p-value,
the pooled common odds ratio,
and a confidence interval for that pooled effect.
Returns `#VALUE!` when the input does not have the required stacked two-column layout.
Returns `#NUM!` when `alpha` is invalid or when the quantities cannot be evaluated numerically.

### Notes

Suppose there are strata `k = 1,…,K`, each contributing a `2×2` table with cells `a_k,b_k,c_k,d_k` and total `n_k`.
The Mantel-Haenszel approach combines the stratum-specific information while conditioning on the stratum margins.
The common odds-ratio estimator is
`OR_MH = [Σ_k a_k d_k / n_k] / [Σ_k b_k c_k / n_k]`.

The accompanying chi-square statistic is a one-degree-of-freedom test of the null hypothesis that the common odds ratio equals 1 across strata.
It is commonly used for stratified case-control data or when combining several matched `2×2` tables while adjusting for a categorical confounder.

If any stratum contains a zero cell,
a small continuity adjustment is applied internally before effect-size estimation to stabilize the pooled odds-ratio calculations.

### Example

```

=BESH.CT.MANTEL_HAENSZEL(A1:B7)
```

## BESH.CT.MCNEMAR_EXACT

Exact paired `2×2` analysis using the McNemar/Liddell framework.

**Function wizard:** Exact paired 2×2 analysis: McNemar/Liddell p-value and matched-pairs odds-ratio interval.

### Syntax

`=BESH.CT.MCNEMAR_EXACT(table, alpha)`

### Parameters

- **table** — A `2×2` matched-pairs table of non-negative counts.
The off-diagonal cells are the discordant pairs and drive both the exact p-value and the matched-pairs odds ratio.
An optional single top header row containing non-numeric labels is allowed and will be ignored.
- **alpha** — Two-sided significance level for the confidence interval.
The default is `0.05`, corresponding to a 95% interval.

### Returns

A labeled result table containing the exact two-sided p-value,
the matched-pairs odds-ratio estimate,
and its confidence interval.
Returns `#VALUE!` when the supplied range is not a valid `2×2` count table.
Returns `#NUM!` when `alpha` is not strictly between 0 and 1,
or when the quantities cannot be evaluated numerically.

### Notes

For paired binary outcomes, only the discordant counts contribute information about marginal change.
If the table is written as
`[[a,b],[c,d]]`,
then the exact paired null hypothesis is assessed through the discordant counts `b` and `c`.
The exact p-value is a McNemar-type conditional test,
while the reported effect estimate is the matched-pairs odds ratio `b/c`.

The confidence interval is derived from exact finite-sample arguments based on `F`-distribution quantiles.
When one discordant cell is zero,
the interval may become one-sided with an infinite upper or lower bound,
reflecting the fact that the matched-pairs odds ratio is not bounded on both sides by the data.

### Example

```

=BESH.CT.MCNEMAR_EXACT(A1:B3)
=BESH.CT.MCNEMAR_EXACT(A1:B3, 0.1)
```

## BESH.CT.NOMINAL_ASSOC

Measures of nominal association for an `r×c` contingency table.

**Function wizard:** Cramér's V, Pearson's contingency coefficient, and Phi for an r×c table.

### Syntax

`=BESH.CT.NOMINAL_ASSOC(table)`

### Parameters

- **table** — A numeric matrix of non-negative cell counts.
Rows and columns are treated as nominal categories, so only the pattern of cell frequencies matters and no ordering is assumed.
An optional single top header row containing non-numeric labels is allowed and will be ignored.

### Returns

A labeled result table containing Cramér's `V`, Pearson's contingency coefficient,
and the Phi coefficient.
Returns `#VALUE!` when the input is not a valid non-negative integer matrix.
Returns `#NUM!` when the table is too small or has zero total count.

### Notes

These quantities are effect-size summaries derived from the Pearson chi-square statistic.
If `X²` is the chi-square statistic and `N` is the grand total, then
`Phi = √(X²/N)`,
`V = √(X² / [N·min(r-1,c-1)])`,
and
Pearson's contingency coefficient is `C = √(X² / (X² + N))`.

Cramér's `V` is usually preferred for general rectangular tables because it rescales the chi-square statistic to the unit interval.
The Phi coefficient equals the absolute value of the correlation coefficient only for `2×2` tables.
These measures summarize association strength but do not indicate direction for nominal variables.

### Example

```

=BESH.CT.NOMINAL_ASSOC(A1:C4)
```

## BESH.CT.ODDS_RATIO

Odds ratio for an independent `2×2` contingency table,
with both large-sample and exact-style confidence intervals.

**Function wizard:** Odds ratio for a 2×2 table with Woolf and Cornfield confidence intervals.

### Syntax

`=BESH.CT.ODDS_RATIO(table, alpha)`

### Parameters

- **table** — A `2×2` matrix of non-negative counts in the layout
`[[a,b],[c,d]]`.
An optional single top header row containing non-numeric labels is allowed and will be ignored.
- **alpha** — Two-sided significance level for the confidence intervals.
The default is `0.05`.

### Returns

A labeled result table containing the odds-ratio estimate,
a Woolf log-normal confidence interval,
and a Cornfield confidence interval.
Returns `#VALUE!` when the supplied range is not a valid `2×2` count table.
Returns `#NUM!` when `alpha` is invalid or when the estimates are not numerically defined.

### Notes

For a `2×2` table written as
`[[a,b],[c,d]]`,
the odds ratio is
`OR = ad/(bc)`.
It compares the odds of the row-1 outcome across the two column groups,
or equivalently the odds of the column-1 outcome across the two row groups.
Values greater than 1 indicate positive association and values smaller than 1 indicate negative association.

The Woolf interval uses the normal approximation on the log scale:
`log(OR) ± z_{1-α/2} · √(1/a + 1/b + 1/c + 1/d)`.
The Cornfield interval is more exact in spirit and is obtained by inverting the conditional test using iterative calculations.

The odds ratio is the natural effect measure for case-control studies and for logistic-type modeling,
whereas the risk ratio is usually more interpretable in prospective cohort settings.

### Example

```

=BESH.CT.ODDS_RATIO(A1:B3)
```

## BESH.CT.ORDINAL_ASSOC

Ordinal association measures for an ordered contingency table.

**Function wizard:** Ordinal association measures: Kendall tau-b, tau-c, gamma, and Somers' D.

### Syntax

`=BESH.CT.ORDINAL_ASSOC(table, alpha)`

### Parameters

- **table** — A numeric matrix of non-negative cell counts whose row and column categories are both intrinsically ordered.
An optional single top header row containing non-numeric labels is allowed and will be ignored.
- **alpha** — Two-sided significance level used for the reported confidence intervals.
The default is `0.05`.

### Returns

A labeled result table containing Kendall's tau-b,
Kendall's tau-c,
Goodman-Kruskal's gamma,
and Somers' `D` (columns treated as the dependent ordering),
together with their standard errors,
confidence intervals,
and two-sided p-values.
Returns `#VALUE!` when the input is not a valid count matrix.
Returns `#NUM!` when `alpha` is invalid or when the statistics cannot be evaluated numerically.

### Notes

These measures compare concordant and discordant pairs of observations extracted from the ordered table.
A pair is concordant when the observation that is higher on the row ordering is also higher on the column ordering;
it is discordant when the two orderings disagree.
Tied pairs may be handled differently depending on the measure.

Kendall's tau-b adjusts for ties in both margins,
tau-c rescales association for rectangular tables,
Goodman-Kruskal's gamma ignores ties entirely,
and Somers' `D` is asymmetric because it conditions on one variable being treated as the response ordering.

The reported confidence intervals use normal approximations of the form
`estimate ± z_{1-α/2}·SE`.
These summaries are meaningful only when the category order is substantively important;
for purely nominal tables use the nominal-association UDF instead.

### Example

```

=BESH.CT.ORDINAL_ASSOC(A1:D4)
=BESH.CT.ORDINAL_ASSOC(A1:D4, 0.1)
```

## BESH.CT.PAIRED_PROPORTIONS

Estimates the difference between two paired proportions and returns a confidence interval.

**Function wizard:** Estimate the difference between two paired proportions and return a confidence interval.

### Syntax

`=BESH.CT.PAIRED_PROPORTIONS(totalN, respondersOnly1, respondersOnly2, respondersBoth, alpha)`

### Parameters

- **totalN** — Total number of paired observations.
Each observational unit contributes one paired binary outcome,
such as before/after response for the same subject or two ratings on the same subject.
The value must be a positive integer.
- **respondersOnly1** — Number of pairs that are positive only in the first condition and negative in the second condition.
In a paired `2×2` table this is one of the discordant cells.
- **respondersOnly2** — Number of pairs that are positive only in the second condition and negative in the first condition.
This is the other discordant cell.
- **respondersBoth** — Number of pairs that are positive in both conditions simultaneously.
This is one of the concordant cells.
- **alpha** — Two-sided significance level used to construct the confidence interval.
The returned confidence level is `1 - alpha`.
The default is `0.05`.

### Returns

A labeled result table containing the marginal paired proportions,
the estimated difference,
and the lower and upper confidence limits.
Returns `#VALUE!` when one or more inputs are missing or non-numeric.
Returns `#NUM!` when the arguments are outside the valid statistical domain.

### Notes

This function is for matched or repeated-measures binary data.
If the paired `2×2` table is written as

`[ negative in both, respondersOnly1 ;
respondersOnly2, respondersBoth ]`

then the two marginal proportions are
`p̂₁ = (respondersOnly1 + respondersBoth) / totalN`
and
`p̂₂ = (respondersOnly2 + respondersBoth) / totalN`.
The reported effect estimate is
`Δ̂ = p̂₁ - p̂₂`.

The confidence interval is based on Wilson-score limits for the two marginal proportions
together with a dependence adjustment derived from the paired binary association.
This is important because the two marginal proportions are computed from the same observational units
and are therefore not statistically independent.

A positive estimate means the first condition has the higher observed marginal proportion.
A negative estimate means the second condition has the higher observed marginal proportion.
This function focuses on estimation of the paired proportion difference rather than on hypothesis testing.

### Example

```

=BESH.CT.PAIRED_PROPORTIONS(80,12,7,20)
=BESH.CT.PAIRED_PROPORTIONS(80,12,7,20,0.1)
```

## BESH.CT.RISK_RATIO

Risk ratio (relative risk) for an independent `2×2` contingency table.

**Function wizard:** Risk ratio (relative risk) for a 2×2 contingency table.

### Syntax

`=BESH.CT.RISK_RATIO(table, alpha)`

### Parameters

- **table** — A `2×2` matrix of non-negative counts in the layout
`[[a,b],[c,d]]`,
where the first column corresponds to event counts and the second column to non-event counts.
An optional single top header row containing non-numeric labels is allowed and will be ignored.
- **alpha** — Two-sided significance level for the confidence interval.
The default is `0.05`.

### Returns

A labeled result table containing the risk-ratio estimate and its confidence interval.
Returns `#VALUE!` when the supplied range is not a valid `2×2` count table.
Returns `#NUM!` when `alpha` is invalid or when the estimate is not numerically defined.

### Notes

For a table written as
`[[a,b],[c,d]]`,
the reported quantity is
`RR = [a/(a+c)] / [b/(b+d)]`
according to the orientation used by the current add-in implementation.
Therefore the interpretation depends on how event and comparison groups are laid out in the worksheet.

Confidence limits are computed on the log scale using the large-sample approximation
`log(RR) ± z_{1-α/2}·SE`,
with
`SE = √[ c/(a(a+c)) + d/(b(b+d)) ]`
under the implemented cell ordering.

The risk ratio is often easier to communicate than the odds ratio in cohort or prospective settings,
but unlike the odds ratio it is not invariant to transposing the event/non-event orientation of the table.

### Example

```

=BESH.CT.RISK_RATIO(A1:B3)
```

## BESH.CT.SINGLE_PROPORTION

Estimates a single proportion and returns a score-based confidence interval.

**Function wizard:** Estimate a single proportion and return a Wilson score confidence interval.

### Syntax

`=BESH.CT.SINGLE_PROPORTION(responders, totalN, alpha)`

### Parameters

- **responders** — Number of observations classified as responders, successes, or events.
This is the numerator of the observed proportion and must satisfy
`0 ≤ responders ≤ totalN`.
- **totalN** — Total number of observations or Bernoulli trials.
The observed proportion is
`p̂ = responders / totalN`.
The value must be a positive integer.
- **alpha** — Two-sided significance level used to construct the confidence interval.
The returned confidence level is `1 - alpha`.
The default is `0.05`, corresponding to a 95% confidence interval.

### Returns

A labeled result table containing the sample size,
the number of responders,
the observed proportion,
and the lower and upper confidence limits.
Returns `#VALUE!` when one or more inputs are missing or non-numeric.
Returns `#NUM!` when the arguments are outside the valid statistical domain.

### Notes

This function estimates a binomial proportion from a sample of size `n`
with `x` responders, so the point estimate is
`p̂ = x / n`.

The confidence interval is based on the Wilson score method rather than the simple
Wald interval `p̂ ± z √(p̂(1-p̂)/n)`.
The Wilson interval is generally preferred because it has better coverage properties,
especially when the sample size is small or when the proportion is close to 0 or 1.

If `z = Φ⁻¹(1 - alpha/2)`, one form of the Wilson limits is

`L,U = (2x + z² ± z √(z² + 4x(1 - x/n))) / (2(n + z²)).`

The interval is bounded inside the unit interval and should be interpreted on the probability scale.
For example, a returned estimate of 0.32 means that about 32% of the observed subjects were responders.

### Example

```

=BESH.CT.SINGLE_PROPORTION(18,50)
=BESH.CT.SINGLE_PROPORTION(18,50,0.1)
```

## BESH.CT.TREND

Cochran-Armitage test for linear trend in proportions across ordered groups.

**Function wizard:** Cochran-Armitage test for linear trend in proportions across ordered groups.

### Syntax

`=BESH.CT.TREND(table)`

### Parameters

- **table** — A count table with one dimension of length 2.
The function accepts either an `r×2` table or a `2×c` table;
if two rows are supplied the table is transposed automatically so that ordered groups run down the rows.
An optional single top header row containing non-numeric labels is allowed and will be ignored.

### Returns

A labeled result table containing the chi-square statistic for linear trend,
its p-value,
the residual chi-square for departure from linearity,
and the corresponding p-value.
Returns `#VALUE!` when the input is not a valid count matrix.
Returns `#NUM!` when neither dimension equals 2,
when the table is only `2×2`,
or when the statistic cannot be evaluated numerically.

### Notes

This test is designed for binary outcomes observed across ordered exposure groups,
such as increasing dose levels or ordered severity categories.
It tests whether the event probability changes linearly with the ordered group score.

If the rows are indexed by scores `w_i` (here taken as `0,1,2,…`),
the trend component is a one-degree-of-freedom quadratic form based on the covariance between the group scores and the observed successes.
The remaining lack-of-fit against a purely linear trend is reported as a second chi-square statistic with
`r-2` degrees of freedom.

The procedure is more powerful than the full Pearson chi-square test when the scientific alternative is specifically monotone or approximately linear across ordered groups.

### Example

```

=BESH.CT.TREND(A1:B5)
=BESH.CT.TREND(A1:E2)
```

## BESH.CT.TWO_INDEPENDENT_PROPORTIONS

Estimates the difference between two independent proportions and returns a confidence interval.

**Function wizard:** Estimate the difference between two independent proportions and return a confidence interval.

### Syntax

`=BESH.CT.TWO_INDEPENDENT_PROPORTIONS(responders1, totalN1, responders2, totalN2, alpha)`

### Parameters

- **responders1** — Number of responders, successes, or events in the first sample.
Must satisfy `0 ≤ responders1 ≤ totalN1`.
- **totalN1** — Total number of observations in the first sample.
The first sample proportion is `p̂₁ = responders1 / totalN1`.
- **responders2** — Number of responders, successes, or events in the second sample.
Must satisfy `0 ≤ responders2 ≤ totalN2`.
- **totalN2** — Total number of observations in the second sample.
The second sample proportion is `p̂₂ = responders2 / totalN2`.
- **alpha** — Two-sided significance level used to construct the confidence interval.
The returned confidence level is `1 - alpha`.
The default is `0.05`.

### Returns

A labeled result table containing both sample proportions,
the estimated difference `p̂₁ - p̂₂`,
and the corresponding lower and upper confidence limits.
Returns `#VALUE!` when one or more inputs are missing or non-numeric.
Returns `#NUM!` when the arguments are outside the valid statistical domain.

### Notes

This function compares two independent binomial proportions,
such as a response rate in a treatment group versus a control group.
The point estimate is
`Δ̂ = p̂₁ - p̂₂`.
A positive value means the first sample has the higher observed proportion,
and a negative value means the second sample has the higher observed proportion.

The confidence limits are constructed from Wilson-score limits for the two marginal proportions
and then combined into a score-type interval for the difference.
This approach is more stable than the elementary normal approximation,
particularly when one or both sample proportions are near the boundaries 0 or 1.

The returned interval is for the absolute risk difference on the probability scale.
For example, an estimate of `0.12` means that the first sample proportion exceeds
the second sample proportion by 12 percentage points.

The procedure assumes that the two samples are statistically independent.
If the same subjects contribute to both measurements, use the paired-proportions function instead.

### Example

```

=BESH.CT.TWO_INDEPENDENT_PROPORTIONS(18,50,10,45)
=BESH.CT.TWO_INDEPENDENT_PROPORTIONS(18,50,10,45,0.1)
```

## BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV

TOST-style equivalence comparison for two independent proportions.

**Function wizard:** TOST-style equivalence comparison for two independent proportions with interval-based decision reporting.

### Syntax

`=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV(controlResponders, controlTotal, experimentalResponders, experimentalTotal, lowerMargin, upperMargin, alpha)`

### Parameters

- **controlResponders** — Number of responders in the control or reference group.
- **controlTotal** — Total number of observations in the control or reference group.
- **experimentalResponders** — Number of responders in the experimental or test group.
- **experimentalTotal** — Total number of observations in the experimental or test group.
- **lowerMargin** — Lower equivalence margin on the risk-difference scale `p(experimental) - p(control)`.
If `upperMargin` is omitted, this argument is interpreted as a positive symmetric margin magnitude `M`
and the function uses margins `-M` and `+M`.
- **upperMargin** — Optional upper equivalence margin. When omitted, ±lowerMargin is used.
- **alpha** — Optional one-sided alpha for each TOST component. Default `0.025`.

### Returns

A labeled spill table containing the two one-sided proportion tests, the combined TOST p-value,
the matched two-sided confidence interval, and the interval-based decision summary.

### Notes

Equivalence is evaluated on the absolute risk-difference scale using the Two One-Sided Tests principle.
Let `Δ = p(experimental) - p(control)`. The function evaluates
`H0,lower: Δ ≤ L` versus `H1,lower: Δ > L`
and
`H0,upper: Δ ≥ U` versus `H1,upper: Δ < U`.
Both components must be significant at the supplied one-sided α level for equivalence to be supported.

The reported confidence interval is a Wilson/Newcombe-style interval for the difference in two independent proportions.
Equivalence is supported when that interval lies completely inside the stated equivalence margins.

### Example

```

=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV(18,50,16,48,0.1)
=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV(18,50,16,48,-0.08,0.08,0.025)
```

## BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI

One-sided non-inferiority comparison for two independent proportions.

**Function wizard:** Non-inferiority comparison for two independent proportions with CI-based decision reporting.

### Syntax

`=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI(controlResponders, controlTotal, experimentalResponders, experimentalTotal, margin, alpha)`

### Parameters

- **controlResponders** — Number of responders in the control or reference group.
- **controlTotal** — Total number of observations in the control or reference group.
- **experimentalResponders** — Number of responders in the experimental or test group.
- **experimentalTotal** — Total number of observations in the experimental or test group.
- **margin** — Positive non-inferiority margin magnitude `M` on the absolute risk-difference scale.
The comparison is performed on `Δ = p(experimental) - p(control)`, so the null boundary is `-M`.
- **alpha** — Optional one-sided significance level. Default `0.025`.

### Returns

A labeled spill table containing observed proportions, the difference `p(experimental) - p(control)`,
the one-sided z test, the matching two-sided confidence interval, and the interval-based decision summary.

### Notes

The non-inferiority hypotheses are
`H0: Δ ≤ -M` versus `H1: Δ > -M`,
where `M > 0` is the largest acceptable loss in the experimental response probability.

The function reports the usual risk-difference estimate together with a Wilson/Newcombe-style two-sided confidence interval.
Non-inferiority is supported when the lower confidence bound exceeds `-M`, which corresponds to a one-sided p-value at most `α`.

### Example

```

=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI(18,50,16,48,0.1)
=BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI(18,50,16,48,0.08,0.025)
```
