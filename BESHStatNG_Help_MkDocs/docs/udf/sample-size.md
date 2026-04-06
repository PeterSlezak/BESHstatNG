# Sample Size UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Sample Size Independent Proportions](../methods/sample-size-independent-proportions.md)
- [Sample Size Single Proportion](../methods/sample-size-single-proportion.md)
- [Sample Size Paired T Test](../methods/sample-size-paired-t-test.md)
- [Sample Size Unpaired T Test](../methods/sample-size-unpaired-t-test.md)

## BESH.SSIZE.PROP_INDEP

Estimates the required sample sizes for comparing two independent proportions.

**Function wizard:** Required group sizes for comparing two independent proportions.

### Syntax

`=BESH.SSIZE.PROP_INDEP(controlProportion, experimentalProportion, controlToExperimentalRatio, alpha, beta)`

### Parameters

- **controlProportion** — The anticipated proportion in the control group.
The value must satisfy `0 ≤ controlProportion ≤ 1`.
- **experimentalProportion** — The anticipated proportion in the experimental group.
The value must satisfy `0 ≤ experimentalProportion ≤ 1` and must differ from `controlProportion`.
- **controlToExperimentalRatio** — The planned allocation ratio defined as
`number of control subjects / number of experimental subjects`.
The value must be strictly positive.
- **alpha** — Two-sided significance level.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.

### Returns

A three-column spill range with headers.
The first result row contains the required control and experimental group sizes for the uncorrected chi-square approach.
The second result row contains the required control and experimental group sizes for the corrected chi-square or Fisher exact approach.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function to plan a study that compares two independent proportions,
for example response rates, event rates, or prevalences in two groups.

Two sets of recommendations are returned because the required sample size depends on the intended test framework.
The corrected/Fisher-style recommendation is usually at least as large as the uncorrected chi-square recommendation.

### Example

```

=BESH.SSIZE.PROP_INDEP(0.3, 0.5, 1, 0.05, 0.2)
```

## BESH.SSIZE.PROP_SINGLE

Estimates the required sample size for a one-sample test of a proportion.

**Function wizard:** Required sample size for a one-sample two-sided proportion test.

### Syntax

`=BESH.SSIZE.PROP_SINGLE(anticipatedProportion, nullProportion, alpha, beta)`

### Parameters

- **anticipatedProportion** — The proportion expected under the alternative hypothesis.
This is typically the proportion that the study is designed to detect.
The value must satisfy `0 ≤ anticipatedProportion ≤ 1`.
- **nullProportion** — The reference proportion under the null hypothesis.
The value must satisfy `0 ≤ nullProportion ≤ 1` and must differ from `anticipatedProportion`.
- **alpha** — Two-sided significance level.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.

### Returns

A two-column spill range with headers that reports the required number of subjects.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function when the primary analysis compares a single population proportion
against a prespecified reference value.

The calculation uses a normal approximation and rounds the result up to the next whole subject.
When the anticipated proportion is very close to the null proportion, the required sample size can become very large.

### Example

```

=BESH.SSIZE.PROP_SINGLE(0.6, 0.5, 0.05, 0.2)
```

## BESH.SSIZE.TTEST_PAIRED

Estimates the number of paired observations required for a paired two-sided t-test.

**Function wizard:** Required number of pairs for a paired two-sided t-test.

### Syntax

`=BESH.SSIZE.TTEST_PAIRED(meanDifference, sdDifference, alpha, beta)`

### Parameters

- **meanDifference** — The expected mean of the paired differences.
This is the effect size on the original measurement scale after subtracting one paired measurement from the other.
The value must be non-zero.
- **sdDifference** — The expected standard deviation of the paired differences.
This is not the standard deviation of the raw measurements; it is the standard deviation of the within-pair differences.
The value must be strictly positive.
- **alpha** — Two-sided significance level.
Common choices are 0.05 or 0.01.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
For example, `beta = 0.20` corresponds to 80% power.
The value must satisfy `0 < beta < 1`.

### Returns

A two-column spill range with headers that reports the required number of pairs.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function for matched or repeated-measures designs where each subject contributes a pair of observations,
such as before/after measurements or measurements from matched units.

The calculation assumes a two-sided hypothesis test and refines the required size using the t distribution.
The result is the number of complete pairs, not the number of individual observations.

### Example

```

=BESH.SSIZE.TTEST_PAIRED(2, 5, 0.05, 0.2)
```

## BESH.SSIZE.TTEST_UNPAIRED

Estimates the required group sizes for an unpaired two-sided t-test.

**Function wizard:** Required control and experimental group sizes for an unpaired two-sided t-test.

### Syntax

`=BESH.SSIZE.TTEST_UNPAIRED(meanDifference, commonSd, controlToExperimentalRatio, alpha, beta)`

### Parameters

- **meanDifference** — The expected difference in means between the two groups on the original measurement scale.
The value must be non-zero.
- **commonSd** — The expected common standard deviation for the outcome variable.
The value must be strictly positive.
- **controlToExperimentalRatio** — The planned allocation ratio defined as
`number of control subjects / number of experimental subjects`.
A value of 1 means equal group sizes, 2 means twice as many controls as experimental subjects,
and 0.5 means half as many controls as experimental subjects.
The value must be strictly positive.
- **alpha** — Two-sided significance level.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.

### Returns

A two-column spill range with headers that reports the required number of control and experimental subjects.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function for two independent groups when the primary endpoint is approximately continuous
and the design is planned with a two-sided t-test.

The reported counts are rounded up to whole subjects. When the allocation ratio is not 1,
the function preserves the requested control-to-experimental ratio as closely as possible after rounding.

### Example

```

=BESH.SSIZE.TTEST_UNPAIRED(2, 5, 1, 0.05, 0.2)
```
