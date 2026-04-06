# Assumptions UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Univariate Outliers](../methods/univariate-outliers.md)
- [Homogeneity Of Variance](../methods/homogeneity-of-variance.md)
- [Normality Tests](../methods/normality-tests.md)
- [Symmetry](../methods/symmetry.md)

## BESH.ASM.ANDERSON_DARLING

Anderson–Darling normality test for a single sample.

**Function wizard:** Anderson-Darling normality test for a single sample.

### Syntax

`=BESH.ASM.ANDERSON_DARLING(data)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.

### Returns

A labeled result table containing the adjusted Anderson–Darling statistic and its approximate p-value.
Returns `#VALUE!` if the input is not a single-column range.
Returns `#NUM!` if fewer than 2 usable observations remain or if the statistic cannot be computed.

### Notes

The Anderson–Darling test compares the empirical distribution of the sample with the fitted normal distribution.
Relative to some other normality tests, it is especially sensitive to discrepancies in the tails.

The reported p-value is an approximation based on the adjusted statistic.

### Example

```

=BESH.ASM.ANDERSON_DARLING(A2:A100)
```

## BESH.ASM.BARTLETT

Bartlett's test for equality of variances across grouped samples.

**Function wizard:** Bartlett test for homogeneity of variances across groups.

### Syntax

`=BESH.ASM.BARTLETT(groups)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and excluded.

### Returns

A labeled result table containing the chi-square statistic and p-value.
Returns `#VALUE!` if the input is not a valid grouped range.
Returns `#NUM!` if fewer than two usable groups remain or if any retained group has fewer than two observations.

### Notes

Bartlett's test is powerful under normality but can be sensitive to non-normal data.
When normality is doubtful, more robust alternatives such as Fligner–Killeen or the Brown–Forsythe variant of Levene's test may be preferable.

### Example

```

=BESH.ASM.BARTLETT(A1:C25)
```

## BESH.ASM.BOX_M

Box's M test for equality of covariance matrices across two or more groups.

**Function wizard:** Box's M test for equality of covariance matrices across groups.

### Syntax

`=BESH.ASM.BOX_M(data, groups)`

### Parameters

- **data** — Numeric data matrix with one row per observation and one column per variable.
The first row may contain variable headers. Rows with any non-numeric value are excluded.
- **groups** — A single-column range of group labels aligned row-for-row with `data`.
Labels may be text or numbers. An optional header cell may be included above the labels
and is excluded automatically when present.
Blank labels are ignored.

### Returns

A labeled result table containing Box's M statistic and its p-value.
Returns `#VALUE!` if the inputs are not a valid matrix-plus-group specification.
Returns `#NUM!` if fewer than two groups remain or if any retained group has fewer than two complete observations.

### Notes

Box's M test evaluates whether the within-group covariance matrices are equal across groups.
It is commonly used as an assumption check before multivariate procedures such as Hotelling's two-sample test or MANOVA.

Rows are filtered jointly: a row is used only when it has a nonblank group label and complete numeric data across all variables.

Each group must contain enough complete observations to produce a non-singular covariance matrix. In practice, the number 
of complete observations in every group should exceed the number of variables. If any within-group covariance matrix is 
singular, the function returns `#NUM!`.

### Example

```

=BESH.ASM.BOX_M(A1:C101, D1:D101)
```

## BESH.ASM.DAGOSTINO_PEARSON

D'Agostino–Pearson omnibus normality test based on skewness and kurtosis.

**Function wizard:** D'Agostino-Pearson K² normality test for a single sample.

### Syntax

`=BESH.ASM.DAGOSTINO_PEARSON(data)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.

### Returns

A labeled result table containing the K² statistic and its two-sided p-value.
Returns `#VALUE!` if the input is not a single-column range.
Returns `#NUM!` if fewer than 9 usable observations remain.

### Notes

This omnibus normality test combines evidence from sample skewness and sample kurtosis.
It is useful when departures from normality may arise through asymmetry, heavy tails, or both.

The reported statistic follows an approximate chi-square distribution with 2 degrees of freedom under the null hypothesis of normality.

### Example

```

=BESH.ASM.DAGOSTINO_PEARSON(A2:A100)
```

## BESH.ASM.FLIGNER_KILLEEN

Fligner–Killeen test for equality of variances across grouped samples.

**Function wizard:** Fligner-Killeen test for homogeneity of variances across groups.

### Syntax

`=BESH.ASM.FLIGNER_KILLEEN(groups)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and excluded.

### Returns

A labeled result table containing the chi-square statistic and p-value.
Returns `#VALUE!` if the input is not a valid grouped range.
Returns `#NUM!` if fewer than two non-empty groups remain.

### Notes

The Fligner–Killeen test is a robust, rank-based procedure for comparing group variances.
It is often preferred when the normality assumption is doubtful.

### Example

```

=BESH.ASM.FLIGNER_KILLEEN(A1:C25)
```

## BESH.ASM.GRUBBS

Grubbs' test for detecting a single outlying observation in a univariate sample.

**Function wizard:** Grubbs test for a single outlier in a univariate sample.

### Syntax

`=BESH.ASM.GRUBBS(data, alpha)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.
- **alpha** — Optional significance level in the open interval `(0,1)`.
The default is `0.05`.

### Returns

A labeled result table containing alpha, the critical statistic, the observed statistic, and a textual conclusion.
Returns `#VALUE!` if the input is not a single-column range.
Returns `#NUM!` if too few usable observations remain or if `alpha` is invalid.

### Notes

Grubbs' test evaluates whether the most extreme observation is inconsistent with the remainder of the sample under approximate normality.
It is intended for detecting at most one outlier at a time.

### Example

```

=BESH.ASM.GRUBBS(A2:A30)
=BESH.ASM.GRUBBS(A2:A30, 0.01)
```

## BESH.ASM.LEVENE

Levene's test or Brown–Forsythe modification for equality of variances across grouped samples.

**Function wizard:** Levene or Brown-Forsythe test for homogeneity of variances across groups.

### Syntax

`=BESH.ASM.LEVENE(groups, center)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and excluded.
- **center** — Optional centering rule:

- `"mean"` or `"levene"` — classical Levene test (default)
- `"median"`, `"brown-forsythe"`, or `"bf"` — Brown–Forsythe modification

### Returns

A labeled result table containing the F statistic and p-value.
Returns `#VALUE!` if the input is not a valid grouped range or if `center` is not recognized.
Returns `#NUM!` if fewer than two non-empty groups remain.

### Notes

Classical Levene's test centers observations around the group mean.
The Brown–Forsythe variant centers them around the group median and is more robust when group distributions are skewed or heavy-tailed.

### Example

```

=BESH.ASM.LEVENE(A1:C25)
=BESH.ASM.LEVENE(A1:C25, "brown-forsythe")
```

## BESH.ASM.MAUCHLY

Mauchly's test of sphericity for repeated-measures data.

**Function wizard:** Mauchly's test of sphericity for repeated-measures data.

### Syntax

`=BESH.ASM.MAUCHLY(data)`

### Parameters

- **data** — Numeric matrix where rows are subjects and columns are repeated-measure conditions.
Rows containing any missing or non-numeric value are excluded so that the retained matrix is complete.
If the first row contains non-numeric labels, it is treated as a header row and excluded.

### Returns

A labeled result table containing the chi-square statistic and p-value for the sphericity test.
Returns `#VALUE!` if the input is not a valid numeric matrix.
Returns `#NUM!` if too few complete rows remain or if fewer than three conditions are supplied.

### Notes

Mauchly's test assesses the sphericity assumption used by classical repeated-measures ANOVA.
A small p-value indicates evidence that the covariance structure departs from sphericity,
in which case corrections such as Greenhouse–Geisser or Huynh–Feldt are commonly considered.

### Example

```

=BESH.ASM.MAUCHLY(A1:D25)
```

## BESH.ASM.ROSNER

Rosner generalized ESD test for detecting multiple outliers in a univariate sample.

**Function wizard:** Rosner generalized ESD test for multiple outliers in a univariate sample.

### Syntax

`=BESH.ASM.ROSNER(data, alpha)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.
- **alpha** — Optional significance level in the open interval `(0,1)`.
The default is `0.05`.

### Returns

A labeled result table containing alpha, the number of detected outliers, and the detected outlying values.
Returns `#VALUE!` if the input is not a single-column range.
Returns `#NUM!` if fewer than 15 usable observations remain or if `alpha` is invalid.

### Notes

Rosner's generalized ESD procedure iteratively checks for up to ten potential outliers and determines how many of the most extreme values should be flagged.
It is intended for larger samples than Grubbs' test.

For sample sizes below 25, the result can still be computed but should be interpreted with caution.

### Example

```

=BESH.ASM.ROSNER(A2:A50)
=BESH.ASM.ROSNER(A2:A50, 0.1)
```

## BESH.ASM.SHAPIRO_WILK

Shapiro–Wilk test for assessing univariate normality.

**Function wizard:** Shapiro-Wilk normality test for a single sample.

### Syntax

`=BESH.ASM.SHAPIRO_WILK(data)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.

### Returns

A labeled result table containing the Shapiro–Wilk W statistic and the corresponding two-sided p-value.
Returns `#VALUE!` if the input is not a single-column range.
Returns `#NUM!` if fewer than 3 usable observations remain, more than 5000 usable observations are supplied,
or the test cannot be evaluated because the data have zero range.

### Notes

The Shapiro–Wilk test is one of the most widely used tests of normality for small to moderate sample sizes.
It compares the ordered sample values with the corresponding expected order statistics under a normal distribution.

Small p-values indicate evidence against the assumption of normality.
The implementation is intended for sample sizes from 3 to 5000 usable observations.

### Example

```

=BESH.ASM.SHAPIRO_WILK(A2:A51)
=BESH.ASM.SHAPIRO_WILK(A1:A51)
```

## BESH.ASM.SQUARED_RANKS

Squared-ranks test for equality of variances across grouped samples.

**Function wizard:** Squared-ranks test for homogeneity of variances across groups.

### Syntax

`=BESH.ASM.SQUARED_RANKS(groups)`

### Parameters

- **groups** — Multi-column Excel range where each column represents one group.
Non-numeric cells inside a group column are ignored. Columns with no numeric observations are ignored.
If the first row contains non-numeric labels, it is treated as a header row and excluded.

### Returns

A labeled result table containing the chi-square statistic and p-value.
Returns `#VALUE!` if the input is not a valid grouped range.
Returns `#NUM!` if fewer than two non-empty groups remain.

### Notes

The squared-ranks test is a nonparametric procedure for comparing variability across groups.
It is based on ranks of absolute deviations and provides a robust alternative when normality is questionable.

### Example

```

=BESH.ASM.SQUARED_RANKS(A1:C25)
```

## BESH.ASM.SYMMETRY

Symmetry test about an unknown median for a single sample.

**Function wizard:** Symmetry test about an unknown median: MGG (default) or Cabilio-Masaro.

### Syntax

`=BESH.ASM.SYMMETRY(data, method)`

### Parameters

- **data** — A single-column Excel range containing the sample values.
Non-numeric cells are ignored. If the first cell is a non-numeric label and numeric values follow,
it is treated as a header and excluded from the calculation.
- **method** — Optional symmetry test to use:

- `"mgg"`, `"miao-gel-gastwirth"` — Miao–Gel–Gastwirth test (default)
- `"cm"` or `"cabilio-masaro"` — Cabilio–Masaro test

### Returns

A labeled result table containing the test statistic and two-sided p-value.
Returns `#VALUE!` if the input is not a single-column range or if `method` is not recognized.
Returns `#NUM!` if fewer than two usable observations remain.

### Notes

These tests assess whether the sample distribution is symmetric around an unknown median.
The Miao–Gel–Gastwirth option uses a robust scale estimate, while the Cabilio–Masaro option is based on the difference between the mean and the median.

### Example

```

=BESH.ASM.SYMMETRY(A2:A51)
=BESH.ASM.SYMMETRY(A2:A51, "cm")
```
