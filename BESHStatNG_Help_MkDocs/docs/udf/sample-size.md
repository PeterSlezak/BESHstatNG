# Sample Size UDFs

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._
## Related dialog documentation

- [Sample Size Bland Altman](../methods/sample-size-bland-altman.md)
- [Sample Size Cox Regression](../methods/sample-size-cox-regression.md)
- [Sample Size Icc](../methods/sample-size-icc.md)
- [Sample Size Log Rank](../methods/sample-size-log-rank.md)
- [Sample Size Independent Proportions](../methods/sample-size-independent-proportions.md)
- [Sample Size Single Proportion](../methods/sample-size-single-proportion.md)
- [Sample Size Paired T Test](../methods/sample-size-paired-t-test.md)
- [Sample Size Unpaired T Test](../methods/sample-size-unpaired-t-test.md)

## BESH.SSIZE.BLANDALTMAN

Estimates the number of paired measurements required so that the confidence interval around either limit of agreement
has a desired half-width in a Bland-Altman agreement study.

**Function wizard:** Required pairs for a Bland-Altman agreement study with a target limits-of-agreement precision.

### Syntax

`=BESH.SSIZE.BLANDALTMAN(sdDifference, desiredHalfWidth, alpha, loaMultiplier)`

### Parameters

- **sdDifference** — Expected standard deviation of the paired differences.
This is the standard deviation of the measurement differences, not the standard deviation of the raw measurements.
The value must be strictly positive.
- **desiredHalfWidth** — Desired half-width of the confidence interval around a limit of agreement, expressed on the original measurement scale.
Smaller values require larger samples.
The value must be strictly positive.
- **alpha** — Two-sided alpha used for the confidence interval around each limit of agreement.
The value must satisfy `0 < alpha < 1`.
- **loaMultiplier** — Optional multiplier used to define the limits of agreement.
The conventional value is `1.96` for 95% limits of agreement.
The value must be strictly positive.
If omitted, `1.96` is used.

### Returns

A two-column spill range reporting the required number of pairs, the expected standard deviation of the paired differences,
the requested half-width, the achieved half-width at the final rounded sample size,
the alpha level, and the limits-of-agreement multiplier.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function when planning an agreement study in which the main precision goal is the width of the confidence interval
around the Bland-Altman limits of agreement.

The result is the number of complete pairs of measurements required.

### Example

```

=BESH.SSIZE.BLANDALTMAN(5, 2, 0.05, 1.96)
```

## BESH.SSIZE.COX_BINARY

Estimates the required number of events for a Cox proportional hazards design with a binary covariate,
and optionally converts that event count to a total sample size.

**Function wizard:** Required events for a Cox design with a binary covariate, with optional total-sample estimate.

### Syntax

`=BESH.SSIZE.COX_BINARY(hazardRatio, controlToExperimentalRatio, alpha, beta, rSquaredWithOtherCovariates, overallEventProportion, twoSided)`

### Parameters

- **hazardRatio** — Anticipated hazard ratio associated with the binary covariate of interest.
In a two-arm treatment study this is commonly the hazard ratio for experimental versus control.
The value must be strictly positive and must differ from 1.
- **controlToExperimentalRatio** — Allocation ratio expressed as control subjects divided by experimental subjects.
Use `1` for equal allocation.
The value must be strictly positive.
- **alpha** — Type I error rate for the planned covariate test.
When `twoSided` is TRUE this is interpreted as a two-sided alpha;
when `twoSided` is FALSE it is interpreted as a one-sided alpha.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.
- **rSquaredWithOtherCovariates** — Optional proportion of variance in the binary covariate explained by the remaining covariates in the model.
Use `0` when planning an unadjusted effect or when no meaningful inflation is needed.
The value must satisfy `0 <= R^2 < 1`.
If omitted, `0` is used.
- **overallEventProportion** — Optional overall event proportion expected in the full study cohort during follow-up.
When supplied, the function also reports an estimated total number of subjects.
The value must satisfy `0 < p < 1`.
If omitted, only the required event count is reported.
- **twoSided** — Optional logical flag indicating whether the design is two-sided.
TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
If omitted, a two-sided design is used.

### Returns

A two-column spill range reporting the required event count, an optional estimated total sample size,
the log hazard ratio, the effective covariate variance determined by the allocation ratio,
and the assumed `R^2` with the other covariates.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function for planning Cox regression when the covariate of primary interest is binary,
such as treatment assignment, exposure status, or membership in one of two groups.

The function can be used either for pure event-count planning or, when an overall event proportion is available,
for an approximate total-sample-size calculation.

### Example

```

=BESH.SSIZE.COX_BINARY(0.7, 1, 0.05, 0.20, 0, 0.26, TRUE)
```

## BESH.SSIZE.COX_CONTINUOUS

Estimates the required number of events for a Cox proportional hazards design with a continuous covariate,
and optionally converts that event count to a total sample size.

**Function wizard:** Required events for a Cox design with a continuous covariate, with optional total-sample estimate.

### Syntax

`=BESH.SSIZE.COX_CONTINUOUS(hazardRatioPerUnit, covariateSd, alpha, beta, rSquaredWithOtherCovariates, overallEventProportion, twoSided)`

### Parameters

- **hazardRatioPerUnit** — Anticipated hazard ratio for a one-unit increase in the covariate.
Values above 1 indicate an increased hazard per unit increase, and values below 1 indicate a decreased hazard per unit increase.
The value must be strictly positive and must differ from 1.
- **covariateSd** — Expected standard deviation of the covariate in the target population.
The value must be strictly positive.
- **alpha** — Type I error rate for the planned covariate test.
When `twoSided` is TRUE this is interpreted as a two-sided alpha;
when `twoSided` is FALSE it is interpreted as a one-sided alpha.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.
- **rSquaredWithOtherCovariates** — Optional proportion of variance in the covariate explained by the remaining covariates in the model.
The value must satisfy `0 <= R^2 < 1`.
If omitted, `0` is used.
- **overallEventProportion** — Optional overall event proportion expected in the full study cohort during follow-up.
When supplied, the function also reports an estimated total number of subjects.
The value must satisfy `0 < p < 1`.
If omitted, only the required event count is reported.
- **twoSided** — Optional logical flag indicating whether the design is two-sided.
TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
If omitted, a two-sided design is used.

### Returns

A two-column spill range reporting the required event count, an optional estimated total sample size,
the log hazard ratio for a one-unit covariate increase, the effective variance after accounting for the covariate spread,
and the assumed `R^2` with the other covariates.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function when the primary Cox-regression predictor is continuous,
such as age, biomarker level, dose, or another quantitative measurement.

The hazard ratio is interpreted per one-unit increase, so the units of the covariate must be chosen carefully.

### Example

```

=BESH.SSIZE.COX_CONTINUOUS(1.25, 2.5, 0.05, 0.20, 0.10, 0.30, TRUE)
```

## BESH.SSIZE.ICC

Estimates the number of subjects required to test whether an intraclass correlation exceeds a minimum acceptable value.

**Function wizard:** Required subjects for a reliability study based on an intraclass-correlation target.

### Syntax

`=BESH.SSIZE.ICC(nullIcc, alternativeIcc, observationsPerSubject, alpha, beta)`

### Parameters

- **nullIcc** — Minimum acceptable intraclass correlation under the null hypothesis.
The value must satisfy `0 <= ICC < 1`.
- **alternativeIcc** — Target intraclass correlation under the alternative hypothesis.
The value must satisfy `0 <= ICC < 1` and must be greater than `nullIcc`.
- **observationsPerSubject** — Number of repeated measurements or raters per subject.
The value must be an integer greater than or equal to 2.
- **alpha** — One-sided type I error rate for the reliability test.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.

### Returns

A two-column spill range reporting the required number of subjects,
the number of observations per subject, the null and alternative ICC values,
and the achieved power at the final rounded sample size.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function when planning a reliability study in which each subject is assessed repeatedly,
or by multiple raters, and the goal is to demonstrate that the intraclass correlation exceeds a pre-specified minimum.

The design is based on a one-way random-effects testing framework and requires at least two observations per subject.

### Example

```

=BESH.SSIZE.ICC(0.5, 0.75, 3, 0.05, 0.20)
```

## BESH.SSIZE.LOGRANK

Estimates the required number of events and subjects for a two-group log-rank comparison.

**Function wizard:** Required events and subjects for a two-group log-rank design.

### Syntax

`=BESH.SSIZE.LOGRANK(hazardRatio, controlEventProportion, experimentalEventProportion, controlToExperimentalRatio, alpha, beta, twoSided)`

### Parameters

- **hazardRatio** — Anticipated hazard ratio for the experimental group relative to the control group.
Values below 1 indicate a lower event rate in the experimental group, and values above 1 indicate a higher event rate.
The value must be strictly positive and must differ from 1.
- **controlEventProportion** — Expected event proportion in the control group during the full study window.
This should be the cumulative proportion of subjects expected to experience the event by the end of follow-up.
The value must satisfy `0 < p < 1`.
- **experimentalEventProportion** — Expected event proportion in the experimental group during the full study window.
This should be on the same study horizon as the control-group event proportion.
The value must satisfy `0 < p < 1`.
- **controlToExperimentalRatio** — Allocation ratio expressed as control subjects divided by experimental subjects.
Use `1` for equal allocation, `2` for twice as many control subjects as experimental subjects, and so on.
The value must be strictly positive.
- **alpha** — Type I error rate for the planned comparison.
When `twoSided` is TRUE this is interpreted as a two-sided alpha;
when `twoSided` is FALSE it is interpreted as a one-sided alpha.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
For example, `beta = 0.20` corresponds to 80% power.
The value must satisfy `0 < beta < 1`.
- **twoSided** — Optional logical flag indicating whether the design is two-sided.
TRUE, Yes, or 1 request a two-sided design. FALSE, No, or 0 request a one-sided design.
If omitted, a two-sided design is used.

### Returns

A two-column spill range reporting the required number of events, the planned control and experimental group sizes,
the total number of subjects, the allocation proportions implied by the ratio, and the weighted average event proportion.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Use this function when planning a two-group time-to-event study analyzed with a log-rank test.
The function first estimates how many events are needed to achieve the requested alpha and power,
then inflates that event count to a total sample size using the expected event proportions in the two study arms.

The event proportions should reflect the anticipated follow-up duration, accrual pattern, and censoring context for the study.

### Example

```

=BESH.SSIZE.LOGRANK(0.7, 0.30, 0.22, 1, 0.05, 0.20)
```

## BESH.SSIZE.PROP_INDEP

Estimates the required group sizes for superiority, non-inferiority, or equivalence comparisons of two independent proportions.

**Function wizard:** Required group sizes for superiority, non-inferiority, or equivalence comparisons of two independent proportions.

### Syntax

`=BESH.SSIZE.PROP_INDEP(controlProportion, experimentalProportion, controlToExperimentalRatio, alpha, beta, hypothesisType, margin)`

### Parameters

- **controlProportion** — The anticipated proportion in the control group.
The value must satisfy `0 <= controlProportion <= 1`.
- **experimentalProportion** — The anticipated proportion in the experimental group.
The value must satisfy `0 <= experimentalProportion <= 1`.
- **controlToExperimentalRatio** — The planned allocation ratio defined as
`number of control subjects / number of experimental subjects`.
The value must be strictly positive.
- **alpha** — Significance level used for planning.
For `hypothesisType="superiority"`, this is the usual two-sided alpha.
For `hypothesisType="noninferiority"` and `"equivalence"`, this is the one-sided alpha.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
- **hypothesisType** — Optional hypothesis selector: `"superiority"` (default), `"noninferiority"`, or `"equivalence"`.
Common short aliases such as `"ni"` and `"eq"` are also accepted.
- **margin** — Optional positive margin magnitude used only when `hypothesisType` is `"noninferiority"` or `"equivalence"`.
For non-inferiority, the function interprets this as the absolute size of the lower margin on the
`experimental - control` scale, so a value of `0.1` means the experimental proportion may be up to 0.1 lower than control.
For equivalence, the function uses symmetric margins `-margin` and `+margin`.

### Returns

For superiority and non-inferiority, returns a spill range showing both uncorrected chi-square and corrected chi-square / Fisher exact recommendations.
For equivalence, returns a larger spill range showing lower-bound and upper-bound TOST components and the final driving recommendations.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

Existing formulas that omit `hypothesisType` and `margin` continue to use the original superiority calculation.

The function returns both uncorrected and corrected/Fisher-style recommendations because the required sample size depends on the intended test framework.

### Example

```

=BESH.SSIZE.PROP_INDEP(0.3, 0.5, 1, 0.05, 0.2)
=BESH.SSIZE.PROP_INDEP(0.5, 0.5, 1, 0.025, 0.2, "noninferiority", 0.1)
=BESH.SSIZE.PROP_INDEP(0.5, 0.5, 1, 0.025, 0.2, "equivalence", 0.1)
```

## BESH.SSIZE.PROP_SINGLE

Estimates the required sample size for a one-sample test of a proportion.

**Function wizard:** Required sample size for a one-sample two-sided proportion test.

### Syntax

`=BESH.SSIZE.PROP_SINGLE(anticipatedProportion, nullProportion, alpha, beta)`

### Parameters

- **anticipatedProportion** — The proportion expected under the alternative hypothesis.
This is typically the proportion that the study is designed to detect.
The value must satisfy `0 <= anticipatedProportion <= 1`.
- **nullProportion** — The reference proportion under the null hypothesis.
The value must satisfy `0 <= nullProportion <= 1` and must differ from `anticipatedProportion`.
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

Estimates the required group sizes for an unpaired two-sample t-test, non-inferiority test, or equivalence test.

**Function wizard:** Required group sizes for unpaired superiority, non-inferiority, or equivalence t-test planning.

### Syntax

`=BESH.SSIZE.TTEST_UNPAIRED(meanDifference, commonSd, controlToExperimentalRatio, alpha, beta, hypothesisType, margin)`

### Parameters

- **meanDifference** — The expected mean difference on the scale `experimental - control`.
For superiority this is the target difference to detect.
For non-inferiority and equivalence this expected difference must lie on the favorable side of the supplied margin(s).
- **commonSd** — The expected common standard deviation for the outcome variable.
The value must be strictly positive.
- **controlToExperimentalRatio** — The planned allocation ratio defined as
`number of control subjects / number of experimental subjects`.
A value of 1 means equal group sizes, 2 means twice as many controls as experimental subjects,
and 0.5 means half as many controls as experimental subjects.
The value must be strictly positive.
- **alpha** — Significance level used for planning.
For `hypothesisType="superiority"`, this is the usual two-sided alpha.
For `hypothesisType="noninferiority"` and `"equivalence"`, this is the one-sided alpha.
The value must satisfy `0 < alpha < 1`.
- **beta** — Type II error rate used for planning.
Statistical power equals `1 - beta`.
The value must satisfy `0 < beta < 1`.
- **hypothesisType** — Optional hypothesis selector: `"superiority"` (default), `"noninferiority"`, or `"equivalence"`.
Common short aliases such as `"ni"` and `"eq"` are also accepted.
- **margin** — Optional positive margin magnitude used only when `hypothesisType` is `"noninferiority"` or `"equivalence"`.
For non-inferiority, the function interprets this as the absolute size of the lower margin on the
`experimental - control` scale, so a value of `0.5` means the experimental mean may be up to 0.5 units lower than control.
For equivalence, the function uses symmetric margins `-margin` and `+margin`.

### Returns

For superiority and non-inferiority, returns a three-row spill range with the required control and experimental group sizes.
For equivalence, returns a larger spill range showing the lower-bound and upper-bound TOST components and the final driving result.
Returns `#VALUE!` when an argument is missing or non-numeric.
Returns `#NUM!` when the supplied values are outside the valid statistical domain.

### Notes

This function extends the original two-sided superiority planning workflow by optionally supporting
non-inferiority and symmetric equivalence planning through the same worksheet function.
Existing formulas that omit `hypothesisType` and `margin` continue to work as before.

For equivalence, the returned table includes separate results for the lower and upper TOST components,
plus the final group sizes determined by the driving bound.

### Example

```

=BESH.SSIZE.TTEST_UNPAIRED(2, 5, 1, 0.05, 0.2)
=BESH.SSIZE.TTEST_UNPAIRED(0, 5, 1, 0.025, 0.2, "noninferiority", 1)
=BESH.SSIZE.TTEST_UNPAIRED(0, 5, 1, 0.025, 0.2, "equivalence", 1)
```
