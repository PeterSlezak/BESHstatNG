# Nonparametric UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Friedman Test](../methods/friedman-test.md)
- [Kendalls Rank Correlation](../methods/kendalls-rank-correlation.md)
- [Kruskal Wallis Test](../methods/kruskal-wallis-test.md)
- [Mann Whitney Test](../methods/mann-whitney-test.md)
- [Spearman Rank Correlation](../methods/spearman-rank-correlation.md)
- [Wilcoxon Signed Rank Test](../methods/wilcoxon-signed-rank-test.md)

## BESH.NP.FRIEDMAN_MCP

Post-hoc multiple comparisons following a Friedman test.

**Function wizard:** Friedman post-hoc multiple comparisons: Dunn (default) or Conover.

### Syntax

`=BESH.NP.FRIEDMAN_MCP(data, conditionNames, method, alpha)`

### Parameters

- **data** — Numeric matrix where rows are blocks/subjects and columns are treatments/conditions.
Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
- **conditionNames** — Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
- **method** — Post-hoc method to return:

- `"dunn"` / `"spss"` (default)
- `"conover"`
- `"all"` — stack both MCP tables
- **alpha** — Reserved for API consistency with other MCP UDFs.
The current Friedman MCP implementation reports adjusted p-values only and does not compute confidence intervals.

### Returns

A labeled multiple-comparison table as a dynamic array.
Returns `#VALUE!` for invalid input or unknown method.
Returns `#NUM!` for invalid alpha or too few complete rows/conditions.

## BESH.NP.FRIEDMAN_P

Returns the p-value for the Friedman test for repeated-measures / blocked designs (k related samples).

**Function wizard:** Friedman test p-value for repeated-measures/blocked designs (chi-square or F-approximation).

### Syntax

`=BESH.NP.FRIEDMAN_P(data, pType)`

### Parameters

- **data** — A multi-column range where each column is a treatment/condition and each row is a block/subject.

The p-value is computed on complete blocks only: rows with any non-numeric or missing value in any column
are ignored so that the remaining rows contain paired observations across all treatments.
- **pType** — Selects which p-value approximation to return:

- `"T1"` (or `"CHI"`): p-value from the chi-square approximation (df = k − 1).
- `"T2"` (or `"F"`): p-value from the Iman–Davenport F-approximation.

If omitted or empty, `"T1"` is used.

### Returns

The requested p-value in the range [0, 1].
Returns `#VALUE!` if `data` is not a 2+ column range.
Returns `#NUM!` if there are fewer than 2 complete blocks after filtering.

### Notes

The null hypothesis is that all `k` conditions have the same distribution (no systematic rank differences).
A small p-value indicates evidence that at least one condition tends to be higher/lower than another.

### Example

```

=BESH.NP.FRIEDMAN_P(A2:C21,"T1")
=BESH.NP.FRIEDMAN_P(A2:C21,"F")
```

## BESH.NP.FRIEDMAN_STAT

Returns the Friedman test statistic for repeated-measures / blocked designs (k related samples).

**Function wizard:** Friedman test statistic for repeated-measures/blocked designs (T1 chi-square or T2 F-approximation).

### Syntax

`=BESH.NP.FRIEDMAN_STAT(data, statType)`

### Parameters

- **data** — A multi-column range where each column is a treatment/condition and each row is a block/subject.

The test is computed on complete blocks only: rows with any non-numeric or missing value in any column
are ignored so that the remaining rows contain paired observations across all treatments.
- **statType** — Selects which Friedman statistic to return:

- `"T1"` (or `"CHI"`): chi-square approximation statistic (classic Friedman χ²).
- `"T2"` (or `"F"`): Iman–Davenport F-approximation statistic (often better for small samples).

If omitted or empty, `"T1"` is used.

### Returns

The requested Friedman statistic (T1 or T2).
Returns `#VALUE!` if `data` is not a 2+ column range.
Returns `#NUM!` if there are fewer than 2 complete blocks after filtering.

### Notes

Use this test when the same subjects (or blocks) are measured under `k` different conditions
and you want a distribution-free alternative to repeated-measures ANOVA.

Interpretation: larger statistics indicate stronger evidence that at least one condition tends to produce
systematically different values (in terms of ranks) compared with the others.

### Example

```

' Data layout: columns = conditions A..C, rows = subjects
=BESH.NP.FRIEDMAN_STAT(A2:C21,"T1")
=BESH.NP.FRIEDMAN_STAT(A2:C21,"F")
```

## BESH.NP.KENDALL_P

Returns the p-value for Kendall’s rank correlation test (τb) for two paired samples.

**Function wizard:** P-value for Kendall rank correlation test (τb) for two paired samples.

### Syntax

`=BESH.NP.KENDALL_P(xRange, yRange, alpha)`

### Parameters

- **xRange** — One-column range containing the first variable (X). Values are paired by row with `yRange`.
Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
- **yRange** — One-column range containing the second variable (Y). Values are paired by row with `xRange`.
Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
- **alpha** — Optional two-sided significance level passed through to the underlying Kendall procedure for API consistency.
The returned p-value itself does not depend on `alpha`.
The default is `0.05`.

### Returns

Two-sided p-value for testing the null hypothesis of no association (τ = 0).
Returns an Excel error code if inputs are invalid.

### Notes

The test is based on Kendall’s τb and uses an exact permutation distribution for very small samples
when feasible; otherwise it uses an accurate approximation that accounts for ties.

### Example

```

=BESH.NP.KENDALL_P(A2:A51, B2:B51)
=BESH.NP.KENDALL_P(A2:A51, B2:B51, 0.1)
```

## BESH.NP.KENDALL_TAU

Returns Kendall’s rank correlation coefficient (τb) for two paired samples.

**Function wizard:** Kendall rank correlation coefficient (τb) for two paired samples.

### Syntax

`=BESH.NP.KENDALL_TAU(xRange, yRange, alpha)`

### Parameters

- **xRange** — One-column range containing the first variable (X). Values are paired by row with `yRange`.
Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
- **yRange** — One-column range containing the second variable (Y). Values are paired by row with `xRange`.
Rows where either X or Y is non-numeric (empty, text, logical, error) are ignored.
- **alpha** — Optional two-sided significance level used for the internal confidence-interval metadata
computed by the underlying Kendall procedure.
The returned coefficient itself does not depend on `alpha`.
The default is `0.05`.

### Returns

Kendall’s τb in the range [-1, 1], or an Excel error code if inputs are invalid.

### Notes

Kendall’s τb is a nonparametric measure of monotonic association based on the number of
concordant and discordant pairs among the observations. The τb variant adjusts for ties
in X and/or Y, so it remains well-defined when there are repeated values.

Input requirements:

- Each input must be a single-column range.
- The two ranges must have the same number of rows (paired by row).
- At least 4 valid numeric pairs are required for the associated significance test.
- `alpha` must satisfy 0 < alpha < 1.

### Example

```

=BESH.NP.KENDALL_TAU(A2:A51, B2:B51)
=BESH.NP.KENDALL_TAU(A2:A51, B2:B51, 0.1)
```

## BESH.NP.KW_MCP

Dunn's post-hoc multiple comparisons following a Kruskal-Wallis test.

**Function wizard:** Kruskal-Wallis post-hoc multiple comparisons (Dunn test).

### Syntax

`=BESH.NP.KW_MCP(groups, groupNames, alpha)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one independent group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and used as group names.
- **groupNames** — Optional group names supplied as a comma-separated string or as a one-row/one-column range.
When omitted, names are taken from the first row of `groups` when it looks like a header;
otherwise default names such as Group 1, Group 2, … are used.
- **alpha** — Reserved for API consistency with other MCP UDFs.
The current Dunn implementation reports adjusted p-values only and does not compute confidence intervals.

### Returns

A labeled Dunn multiple-comparison table as a dynamic array.
Returns `#VALUE!` for invalid input.
Returns `#NUM!` for invalid alpha or too few groups.

## BESH.NP.KW_P

Kruskal-Wallis p-value for comparing 2 or more independent groups.

**Function wizard:** P-value for Kruskal-Wallis test (based on H or tie-corrected Hcor) for 2+ independent groups.

### Syntax

`=BESH.NP.KW_P(groups, pType)`

### Parameters

- **groups** — Input data arranged as one column per group (a multi-column Excel range).
Each column represents an independent group of observations.
Non-numeric cells (empty, text, logical, error) are ignored within each column.
- **pType** — Select which p-value to return:

- `"H"` - p-value based on the uncorrected H statistic
- `"Hcor"` - p-value based on the tie-corrected Hcor statistic

The comparison is case-insensitive. If omitted, `"Hcor"` is used.

### Returns

A p-value in the range [0, 1]. Returns an Excel error code if inputs are invalid.

### Notes

The p-value is obtained from the chi-square distribution with `k-1` degrees of freedom,
where `k` is the number of non-empty groups (columns).

When there are tied values, the tie-corrected p-value (based on Hcor) is recommended.

### Example

```

=BESH.NP.KW_P(A2:C20, "Hcor")
```

## BESH.NP.KW_STAT

Kruskal-Wallis test statistic (H) for comparing 2 or more independent groups.

**Function wizard:** Kruskal-Wallis test statistic H (or tie-corrected Hcor) for 2+ independent groups.

### Syntax

`=BESH.NP.KW_STAT(groups, statType)`

### Parameters

- **groups** — Input data arranged as one column per group (a multi-column Excel range).
Each column represents an independent group of observations.
Non-numeric cells (empty, text, logical, error) are ignored within each column.
- **statType** — Select which statistic to return:

- `"H"` - the uncorrected Kruskal-Wallis H statistic
- `"Hcor"` - tie-corrected H (recommended when there are ties)

The comparison is case-insensitive. If omitted, `"Hcor"` is used.

### Returns

The Kruskal-Wallis test statistic (H or Hcor) as a number.
Returns an Excel error code if inputs are invalid.

### Notes

The Kruskal-Wallis test is a nonparametric alternative to one-way ANOVA.
It tests whether multiple independent groups come from the same distribution,
by ranking all observations together and comparing the sums of ranks between groups.

When there are tied values, a tie correction can be applied to the H statistic.
The tie-corrected version (Hcor) is usually preferred in real data.

### Example

```

=BESH.NP.KW_STAT(A2:C20, "Hcor")         ' 3 groups stored in columns A..C
```

## BESH.NP.MW_P_EXACT

Mann–Whitney (Wilcoxon rank-sum) test — exact p-value (available only for total n ≤ 50).

**Function wizard:** Mann–Whitney test: exact p-value (n ≤ 50). side: two/lower/upper.

### Syntax

`=BESH.NP.MW_P_EXACT(group1, group2, side)`

### Parameters

- **group1** — Group 1 data (a single-column Excel range).
Non-numeric cells (empty, text, logical, error) are ignored.
- **group2** — Group 2 data (a single-column Excel range).
Non-numeric cells (empty, text, logical, error) are ignored.
- **side** — Specifies which exact p-value to return:

- `"two"` / `"two-sided"` / `"2"` — two-sided exact p-value
- `"lower"` / `"less"` — lower-tail exact p-value
- `"upper"` / `"greater"` — upper-tail exact p-value

The comparison direction matches the internal implementation: lower-tail corresponds to smaller U
values (group 1 tends to have smaller values than group 2).

### Returns

Exact p-value requested by `side` when the exact distribution is available.
Returns `#VALUE!` if either input range is not a single column, or if `side`
is not recognized.
Returns `#NUM!` if there are insufficient numeric observations, or if the exact p-value is not
available (total sample size > 50).

### Notes

This worksheet function computes an exact p‑value for the Mann–Whitney U statistic by enumerating
its sampling distribution using a dynamic‑programming approach. Exact p‑values are most useful for small
sample sizes and discrete data.

Exact computation is performed only when `n = n1 + n2 ≤ 50`
For larger samples use MW_P_NORM.

### Example

```

=BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "two")
=BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "lower")
=BESH.NP.MW_P_EXACT(A2:A10, B2:B12, "upper")
```

## BESH.NP.MW_P_NORM

Mann–Whitney (Wilcoxon rank-sum) test — two-sided p-value using the normal approximation.

**Function wizard:** Mann–Whitney test: two-sided p-value (normal approximation with ties & continuity correction).

### Syntax

`=BESH.NP.MW_P_NORM(group1, group2)`

### Parameters

- **group1** — Group 1 data (a single-column Excel range).
Non-numeric cells (empty, text, logical, error) are ignored.
- **group2** — Group 2 data (a single-column Excel range).
Non-numeric cells (empty, text, logical, error) are ignored.

### Returns

Two-sided p-value computed via the continuity-corrected, tie-corrected normal approximation.
Returns `#VALUE!` if either input range is not a single column.
Returns `#NUM!` if there are insufficient numeric observations.

### Notes

The Mann–Whitney U test (also called the Wilcoxon rank‑sum test) compares two independent groups.
It tests whether one group tends to have larger values than the other without assuming normality.

```

p = 2 * P( Z ≤ -|z| )
```

where `z` is a continuity-corrected normal statistic with tie correction.

In Excel terminology, this is an asymptotic p-value (normal approximation),
suitable for moderate-to-large sample sizes.

### Example

```

=BESH.NP.MW_P_NORM(A2:A21, B2:B16)
```

## BESH.NP.SPEARMAN_P

Returns the p-value for Spearman's rank correlation test for two paired samples.

**Function wizard:** Two-sided p-value for Spearman rank correlation test (paired samples).

### Syntax

`=BESH.NP.SPEARMAN_P(xRange, yRange, alpha)`

### Parameters

- **xRange** — One-column range containing the first variable (X). Values are paired by row with `yRange`.
Non-numeric cells are ignored together with the corresponding row in `yRange`.
- **yRange** — One-column range containing the second variable (Y). Values are paired by row with `xRange`.
Non-numeric cells are ignored together with the corresponding row in `xRange`.
- **alpha** — Optional two-sided significance level passed through to the underlying Spearman procedure for API consistency.
The returned p-value itself does not depend on `alpha`.
The default is `0.05`.

### Returns

Two-sided p-value for testing the null hypothesis of no monotonic association (ρ = 0),
or an Excel error code if inputs are invalid.

### Notes

The test computes Spearman's ρ from ranked data (average ranks for ties), then produces a p-value using:

- Exact permutation p-values for small samples when feasible.
- An accurate approximation for moderate sample sizes without ties.
- A large-sample approximation based on a t-statistic for general cases.

Input requirements match SPEARMAN_RHO. At least 3 valid numeric pairs are required.

### Example

```

=BESH.NP.SPEARMAN_P(A2:A51, B2:B51)
=BESH.NP.SPEARMAN_P(A2:A51, B2:B51, 0.1)
```

## BESH.NP.SPEARMAN_RHO

Returns Spearman's rank correlation coefficient (ρ) for two paired samples.

**Function wizard:** Spearman rank correlation coefficient (ρ) for two paired samples.

### Syntax

`=BESH.NP.SPEARMAN_RHO(xRange, yRange, alpha)`

### Parameters

- **xRange** — One-column range containing the first variable (X). Values are paired by row with `yRange`.
Non-numeric cells are ignored together with the corresponding row in `yRange`.
- **yRange** — One-column range containing the second variable (Y). Values are paired by row with `xRange`.
Non-numeric cells are ignored together with the corresponding row in `xRange`.
- **alpha** — Optional two-sided significance level used for the internal confidence-interval metadata
computed by the underlying Spearman procedure.
The returned coefficient itself does not depend on `alpha`.
The default is `0.05`.

### Returns

Spearman's ρ in the range [-1, 1], or an Excel error code if inputs are invalid.

### Notes

Spearman's ρ is the Pearson correlation computed on the ranks of the data (average ranks are used for ties).
The result measures the strength of a monotonic association between X and Y.

Input requirements:

- Each input must be a single-column range.
- The two ranges must have the same number of rows (paired by row).
- At least 3 valid numeric pairs are required.
- `alpha` must satisfy 0 < alpha < 1.

### Example

```

=BESH.NP.SPEARMAN_RHO(A2:A51, B2:B51)
=BESH.NP.SPEARMAN_RHO(A2:A51, B2:B51, 0.1)
```

## BESH.NP.WILCOX_P_EXACT

Wilcoxon signed-rank test — exact p-value (paired samples; available only for up to 60 non-zero differences).

**Function wizard:** Wilcoxon signed-rank test: exact p-value (paired samples; up to 60 non-zero diffs). side: two/lower/upper.

### Syntax

`=BESH.NP.WILCOX_P_EXACT(x, y, side)`

### Parameters

- **x** — First set of paired observations (a single-column Excel range).
Values are paired by row with `y`.
Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
- **y** — Second set of paired observations (a single-column Excel range).
Values are paired by row with `x`.
Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
- **side** — Specifies which exact p-value to return:

- `"two"` / `"two-sided"` / `"2"` — two-sided exact p-value
- `"lower"` / `"less"` — lower-tail exact p-value
- `"upper"` / `"greater"` — upper-tail exact p-value

Lower-tail corresponds to unusually small `W` (more negative differences), upper-tail to unusually large `W`.

### Returns

Exact p-value requested by `side` when exact computation is available.
Returns `#VALUE!` if either input is not a single column, if the input ranges have different row counts,
or if `side` is not recognized.
Returns `#NUM!` if there are insufficient usable pairs or if the exact p-value is not available.

### Notes

This worksheet function computes an exact p-value for the Wilcoxon signed-rank statistic by constructing
the exact sampling distribution via dynamic programming. Exact p-values are most useful for small samples.

Exact computation is performed only when the number of non-zero paired differences is at most 60.
If there are more non-zero differences, the function returns `#NUM!`. In that case use WILCOX_P_NORM.

### Example

```

=BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "two")
=BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "lower")
=BESH.NP.WILCOX_P_EXACT(A2:A10, B2:B10, "upper")
```

## BESH.NP.WILCOX_P_NORM

Wilcoxon signed-rank test — two-sided p-value using the normal approximation (paired samples).

**Function wizard:** Wilcoxon signed-rank test: two-sided p-value (normal approximation; paired samples).

### Syntax

`=BESH.NP.WILCOX_P_NORM(x, y)`

### Parameters

- **x** — First set of paired observations (a single-column Excel range).
Values are paired by row with `y`.
Rows where either cell is non-numeric (empty, text, logical, error) are ignored.
- **y** — Second set of paired observations (a single-column Excel range).
Values are paired by row with `x`.
Rows where either cell is non-numeric (empty, text, logical, error) are ignored.

### Returns

Two-sided p-value based on the continuity-corrected, tie-corrected normal approximation.
Returns `#VALUE!` if either input is not a single column or if the input ranges have different row counts.
Returns `#NUM!` if there are insufficient usable pairs.

### Notes

The Wilcoxon signed-rank test compares two paired samples (e.g., before/after measurements on the same subjects).
It tests whether the median of the paired differences (`x - y`) is zero without assuming normality.

The test forms paired differences, discards zero differences, ranks the absolute differences (averaging tied ranks),
and computes `W`, the sum of ranks for positive differences. This function returns a two-sided p-value
using a normal approximation with tie correction and a continuity correction.

For small samples, you may prefer the exact p-value returned by WILCOX_P_EXACT.

### Example

```

=BESH.NP.WILCOX_P_NORM(A2:A21, B2:B21)
```
