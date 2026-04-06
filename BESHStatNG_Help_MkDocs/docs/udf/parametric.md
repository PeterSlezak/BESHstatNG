# Parametric UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [One Way Anova](../methods/one-way-anova.md)
- [Two Way Nested Anova](../methods/two-way-nested-anova.md)
- [One Way Repeated Measures Anova](../methods/one-way-repeated-measures-anova.md)
- [Paired T Tests](../methods/paired-t-tests.md)
- [Unpaired Two Sample T Tests](../methods/unpaired-two-sample-t-tests.md)

## BESH.PAR.ANOVA1

Classical one-way ANOVA for comparing means across two or more independent groups.

**Function wizard:** One-way ANOVA table. Input: one column per group.

### Syntax

`=BESH.PAR.ANOVA1(groups, groupNames)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and used as group names.
- **groupNames** — Optional group names supplied as a comma-separated string or as a one-row/one-column range.
When omitted, names are taken from the first row of `groups` when it looks like a header;
otherwise default names such as Group 1, Group 2, … are used.

### Returns

A complete ANOVA table with row and column headers, including between-group, within-group, and total sums of squares,
degrees of freedom, mean squares, F statistic, and p-value.
Returns `#VALUE!` if the input is not a valid grouped range.
Returns `#NUM!` if fewer than two non-empty groups remain after filtering.

### Notes

One-way ANOVA tests whether the population means of several independent groups are equal.
The total variability is partitioned into variability explained by differences between group means and residual variability within groups.
The test statistic is
`F = MS_between / MS_within`,
where `MS` denotes a mean square.

A small p-value indicates evidence that at least one group mean differs from the others.
The test assumes independent observations, approximate normality within groups, and equal variances across groups.

### Example

```

=BESH.PAR.ANOVA1(A1:C20)
```

## BESH.PAR.ANOVA1_MCP

One-way ANOVA post-hoc multiple comparisons for grouped data.

**Function wizard:** One-way ANOVA multiple comparisons: Tukey-Kramer, Games-Howell, Fisher LSD, or Bonferroni.

### Syntax

`=BESH.PAR.ANOVA1_MCP(groups, groupNames, method, alpha)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and used as group names.
- **groupNames** — Optional group names supplied as a comma-separated string or as a one-row/one-column range.
When omitted, names are taken from the first row of `groups` when it looks like a header;
otherwise default names such as Group 1, Group 2, … are used.
- **method** — Post-hoc method to return:

- `"tukey"` / `"tukey-kramer"` (default)
- `"games-howell"`
- `"lsd"` / `"fisher"`
- `"bonferroni"`
- `"all"` — stack all four MCP tables
- **alpha** — Significance level used for confidence intervals in the returned MCP table(s).
The default is `0.05`, corresponding to 95% confidence intervals.

### Returns

A labeled multiple-comparison table as a dynamic array.
Returns `#VALUE!` for invalid input or unknown method.
Returns `#NUM!` for invalid alpha or too few groups.

## BESH.PAR.ANOVA1_WELCH

Welch heteroscedastic one-way ANOVA for comparing means when group variances may differ.

**Function wizard:** Welch one-way ANOVA summary. Input: one column per group.

### Syntax

`=BESH.PAR.ANOVA1_WELCH(groups, groupNames)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and used as group names.
- **groupNames** — Optional group names supplied as a comma-separated string or as a one-row/one-column range.

### Returns

A Welch ANOVA summary table showing numerator and denominator degrees of freedom, F statistic, and p-value.
Returns `#VALUE!` if the input is not a valid grouped range.
Returns `#NUM!` if fewer than two non-empty groups remain after filtering.

### Notes

Welch's ANOVA is a heteroscedastic alternative to classical one-way ANOVA.
It tests equality of group means without requiring equal variances and adjusts the denominator degrees of freedom
using a Satterthwaite-type approximation.

This procedure is often preferred when group sizes and variances are notably unequal.
The null hypothesis remains that all group means are equal.

### Example

```

=BESH.PAR.ANOVA1_WELCH(A1:C20)
```

## BESH.PAR.ANOVA2_NESTED

Two-way nested ANOVA for designs where one factor is nested within another.

**Function wizard:** Two-way nested ANOVA. Input: 3 columns = group, subgroup, response.

### Syntax

`=BESH.PAR.ANOVA2_NESTED(data, varNames, outputType)`

### Parameters

- **data** — Three-column Excel range containing:

- Column 1: higher-level group factor
- Column 2: subgroup factor nested within the group factor
- Column 3: numeric response variable

The first row may contain headers. Rows with blank factor labels or non-numeric responses are ignored.
- **varNames** — Optional variable names as a comma-separated string or a one-row/one-column range.
Three names are expected: group factor, subgroup factor, and response variable.
- **outputType** — Optional output selection:

- `"both"` — return the main ANOVA table followed by the Satterthwaite-adjusted table (default)
- `"main"` — return the main ANOVA table only
- `"satterthwaite"` or `"sw"` — return only the Satterthwaite-adjusted table

### Returns

A labeled ANOVA table, or two stacked tables when `outputType` is `"both"`.
The main table includes variance-component percentages. The Satterthwaite table is returned when applicable.
Returns `#VALUE!` for invalid input shape and `#NUM!` when too few valid observations remain.

### Notes

In a nested ANOVA, the levels of one factor occur only within a single level of another factor.
This differs from a crossed two-way design, where all combinations of factor levels may occur.

The procedure partitions variation into between-group, between-subgroup-within-group, and residual components.
When the design is unbalanced, Satterthwaite-style approximations may be used for selected F tests.

### Example

```

=BESH.PAR.ANOVA2_NESTED(A1:C100,,"both")
```

## BESH.PAR.RMANOVA1

One-way repeated-measures ANOVA for comparing several conditions measured on the same subjects or blocks.

**Function wizard:** One-way repeated-measures ANOVA table. Input: rows=subjects, cols=conditions.

### Syntax

`=BESH.PAR.RMANOVA1(data, conditionNames, correction)`

### Parameters

- **data** — Numeric matrix where rows are subjects/blocks and columns are repeated-measure conditions.
Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
- **conditionNames** — Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
- **correction** — Optional sphericity-correction setting:

- `"none"` — classical RM-ANOVA table only (default)
- `"gg"` — append Greenhouse–Geisser epsilon and corrected p-value
- `"hf"` — append Huynh–Feldt epsilon and corrected p-value
- `"both"` — append both corrections

### Returns

A complete repeated-measures ANOVA table with row and column headers.
Depending on `correction`, the table may include Greenhouse–Geisser and/or Huynh–Feldt corrections.
Returns `#VALUE!` if the input is not a valid repeated-measures matrix.
Returns `#NUM!` if too few complete rows remain.

### Notes

One-way repeated-measures ANOVA partitions variation into treatment (between conditions), subject, and residual components.
It tests whether the mean response differs across repeated conditions while accounting for subject-level dependence.

The usual F test assumes sphericity, meaning that the variances of all pairwise differences between conditions are equal.
Greenhouse–Geisser and Huynh–Feldt corrections relax this assumption by shrinking the effective degrees of freedom,
which typically increases the p-value when sphericity is violated.

### Example

```

=BESH.PAR.RMANOVA1(A1:D25,,"both")
```

## BESH.PAR.RMANOVA1_MCP

One-way repeated-measures ANOVA post-hoc multiple comparisons.

**Function wizard:** Repeated-measures ANOVA multiple comparisons: TukeyKramerRM2 (default) or Tukey assuming sphericity.

### Syntax

`=BESH.PAR.RMANOVA1_MCP(data, conditionNames, method, alpha)`

### Parameters

- **data** — Numeric matrix where rows are subjects/blocks and columns are repeated-measure conditions.
Rows containing any missing or non-numeric value are excluded so that the remaining matrix is complete.
If the first row contains non-numeric labels, it is treated as a header row and used as condition names.
- **conditionNames** — Optional condition names supplied as a comma-separated string or as a one-row/one-column range.
- **method** — Post-hoc method to return:

- `"rm2"` / `"tukeyrm2"` (default; does not assume sphericity)
- `"tukey"` / `"sphericity"` (assumes sphericity)
- `"all"` — stack both MCP tables
- **alpha** — Significance level used for confidence intervals in the returned MCP table(s).
The default is `0.05`, corresponding to 95% confidence intervals.

### Returns

A labeled multiple-comparison table as a dynamic array.
Returns `#VALUE!` for invalid input or unknown method.
Returns `#NUM!` for invalid alpha or too few complete rows/conditions.

## BESH.PAR.TTEST_PAIRED

Paired t-test for comparing the mean of within-row differences between two matched measurements.

**Function wizard:** Paired t-test for two matched samples. Returns a labeled result table.

### Syntax

`=BESH.PAR.TTEST_PAIRED(x, y, varNames)`

### Parameters

- **x** — First measurement as a single-column Excel range.
Values are paired by row with the second input. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and may be used as the default name of the first measurement.
- **y** — Second measurement as a single-column Excel range.
Values are paired by row with the first input. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and may be used as the default name of the second measurement.
- **varNames** — Optional names supplied as a comma-separated string or as a one-row/one-column range.
Two names are expected. When omitted, names are taken from header cells when available;
otherwise default names such as Sample 1 and Sample 2 are used.

### Returns

A labeled result table showing the number of usable pairs, the mean of differences,
the standard deviation and standard error of the differences, the degrees of freedom,
the t statistic, and the two-sided p-value.
Returns `#VALUE!` if either input is not a single column or if the two inputs have different row counts.
Returns `#NUM!` if fewer than two usable numeric pairs remain after filtering.

### Notes

This worksheet function compares two paired measurements, such as before/after values,
left/right measurements, or matched observations from the same subjects.
Rows are matched strictly by position.

The test is carried out on the within-row differences `x - y`.
It tests whether the mean difference equals zero while accounting for the pairing structure.
Rows where either entry is non-numeric are discarded before the calculation.

### Example

```

=BESH.PAR.TTEST_PAIRED(A2:A21, B2:B21)
=BESH.PAR.TTEST_PAIRED(A1:A21, B1:B21, "Before,After")
```

## BESH.PAR.TTEST_UNPAIRED

Two-sample unpaired t-test for comparing the means of two independent groups.

**Function wizard:** Two-sample unpaired t-test. Returns pooled, Welch, or both result tables.

### Syntax

`=BESH.PAR.TTEST_UNPAIRED(x, y, groupNames, outputType, alpha)`

### Parameters

- **x** — First group as a single-column Excel range.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and may be used as the default name of the first group.
- **y** — Second group as a single-column Excel range.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and may be used as the default name of the second group.
- **groupNames** — Optional group names supplied as a comma-separated string or as a one-row/one-column range.
Two names are expected. When omitted, names are taken from header cells when available;
otherwise default names such as Group 1 and Group 2 are used.
- **outputType** — Optional output selection:

- `"both"` — return the pooled-variance table followed by the Welch table (default)
- `"equal"`, `"pooled"`, or `"student"` — return only the equal-variance table
- `"unequal"` or `"welch"` — return only the unequal-variance table
- **alpha** — Optional two-sided significance level used for the mean-difference confidence interval.
The default is `0.05`, corresponding to a 95% confidence interval.

### Returns

A labeled result table, or two stacked labeled tables when `outputType` is `"both"`.
The equal-variance output reports the pooled standard error, t statistic, degrees of freedom, two-sided p-value,
and confidence interval for the mean difference. The unequal-variance output reports the Welch standard error,
Welch degrees of freedom, two-sided p-value, confidence interval, and the p-value of the variance-comparison F test.
Returns `#VALUE!` if either input is not a single column or if `outputType` is not recognized.
Returns `#NUM!` if either group has fewer than two usable numeric observations or if `alpha` is invalid.

### Notes

This worksheet function compares two independent samples.
By default it returns both the classical pooled-variance t-test and Welch’s unequal-variance alternative,
which is often preferred when sample sizes or variances differ noticeably.

The test statistic is based on the difference between the sample means.
The pooled version assumes equal population variances, while the Welch version does not and uses an adjusted
degrees-of-freedom approximation.

### Example

```

=BESH.PAR.TTEST_UNPAIRED(A2:A21, B2:B19)
=BESH.PAR.TTEST_UNPAIRED(A1:A21, B1:B19, "Control,Treatment", "welch", 0.01)
```
