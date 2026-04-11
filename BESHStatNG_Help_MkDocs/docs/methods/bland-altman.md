# Bland–Altman Analysis

**Includes:** Simple paired Bland–Altman analysis, repeated-measures Bland–Altman by subject, bias and limits of agreement (LoA), analytical / jackknife / bootstrap confidence intervals, optional proportional-bias check, raw-difference / percentage / log-ratio scales, several x-axis conventions, and Bland–Altman plots with optional subject means.  
**Purpose:** Use when you want to assess **agreement between two paired measurement methods** and quantify the expected size of their differences, rather than only their association.

---

## Overview

Bland–Altman analysis is a method-comparison technique designed to answer a practical question:

> **How far apart are two methods likely to be when they are used on the same subject / sample?**

For paired measurements \((x_i, y_i)\), the method studies:

- the **difference** between methods
- the **average level** of the methods
- whether the difference changes with the magnitude of measurement

For the classical raw-difference version:

$$
d_i = y_i - x_i
$$

$$
m_i = \frac{x_i + y_i}{2}
$$

where:

- \(x_i\) = reference method
- \(y_i\) = test method
- \(d_i\) = signed difference
- \(m_i\) = mean of the two methods

The main outputs are:

- **Bias** (mean difference)
- **Standard deviation of the differences**
- **Limits of agreement (LoA)**
- **Confidence intervals** for the bias and LoA
- optional **proportional-bias test**

Unlike Pearson correlation or linear regression, Bland–Altman is focused on **agreement in measurement units**, which is often the key question in clinical chemistry, device comparison, assay validation, and biomedical method comparison.

---

## Input

### Subject ID (optional)

Optional subject/sample identifier used for **repeated-measures Bland–Altman**.

Use this when:

- the same subject/sample appears in multiple rows
- repeated paired measurements exist for the same subject
- you want the repeated-measures Bland–Altman option to estimate a pooled within-subject SD

If this field is left empty, the analysis is treated as an ordinary paired Bland–Altman analysis.

### Reference method (X)

Select the range containing the reference method measurements.

### Test method (Y)

Select the range containing the test method measurements.

Requirements:

- both ranges must contain the same number of rows
- matching rows must represent paired observations from the same subject / sample / item
- any pair containing a non-finite value is removed before fitting, and the number of dropped pairs is reported in the output

---

## Options

The **Options** tab controls the analysis mode, scale, plotting style, and confidence-interval method.

## General

### Mode

The current GUI supports the following core modes.

#### Paired / simple Bland–Altman

This is the classical Bland–Altman setting for **one paired observation per subject/item**.

Use when:

- each row is a unique paired observation
- there are no repeated measures per subject
- or repeated structure is not relevant for the current analysis

#### Repeated by subject

This mode is designed for **multiple paired observations per subject**.

Instead of using the ordinary SD of all differences directly, the method estimates a **pooled within-subject SD of the differences**, and uses that pooled SD to compute the repeated-measures limits of agreement.

Use when:

- the same subject/sample has more than one paired measurement
- repeated observations should not be treated as fully independent
- you want the LoA to reflect **within-subject repeatability of differences**

This is the mode shown in the attached screenshots.

---

### Scale

The add-in supports several difference scales.

#### 1) Raw difference

$$
d_i = y_i - x_i
$$

Use when:

- absolute disagreement in the original measurement unit matters
- interpretation in the original scale is most natural
- measurement variability is roughly additive across the range

This is the default and most common choice.

#### 2) Percent of mean

$$
d_i^{(\%)} = 100 \cdot \frac{y_i - x_i}{(x_i + y_i)/2}
$$

Use when:

- relative disagreement is more meaningful than absolute disagreement
- neither method is clearly privileged as denominator

#### 3) Percent of reference

$$
d_i^{(\%ref)} = 100 \cdot \frac{y_i - x_i}{x_i}
$$

Use when:

- X is the natural baseline or comparator
- the clinical interpretation is “percent difference relative to the reference method”

#### 4) Percent of test

$$
d_i^{(\%test)} = 100 \cdot \frac{y_i - x_i}{y_i}
$$

Use when:

- Y is the natural denominator for practical reasons
- you need a test-relative percent difference

#### 5) Log ratio

$$
d_i^{(\log)} = \ln\left(\frac{y_i}{x_i}\right)
$$

Use when:

- differences are multiplicative rather than additive
- method variability increases with magnitude
- ratios are more natural than raw differences
- all paired values are strictly positive

On the log-ratio scale, exponentiation converts the results back into ratio interpretation.

---

### X-axis

The add-in supports three x-axis conventions.

#### Mean of methods

$$
m_i = \frac{x_i + y_i}{2}
$$

This is the classical Bland–Altman choice and the default in most applications.

Use when:

- neither method should dominate the x-axis definition
- you want the conventional Bland–Altman plot

#### Reference method

$$
m_i = x_i
$$

Use when:

- X is a natural standard or comparator
- you want to inspect how disagreement varies with the reference method itself

#### Test method

$$
m_i = y_i
$$

Use when:

- the test method is the practical operational scale of interest

---

### Plot

For repeated-measures mode, the plot can optionally show:

- all observations only
- subject means only
- all observations + subject means

The screenshot example uses:

- **All observations + subject means**

This is often the most informative choice because it shows both:

- the raw paired-difference cloud
- the subject-level central tendency

---

### Use t distribution for analytical CI

When enabled, analytical confidence intervals are based on the Student t distribution rather than the standard normal approximation.

Use when:

- the sample is not very large
- you want a slightly more conservative analytical CI

---

### Check proportional bias

If selected, the add-in fits a regression of differences on the selected x-axis quantity.

For the classical mean-of-methods x-axis:

$$
d_i = \alpha + \beta m_i + \varepsilon_i
$$

Interpretation:

- \(\beta = 0\): no evidence of proportional bias
- \(\beta > 0\): difference tends to increase with measurement magnitude
- \(\beta < 0\): difference tends to decrease with measurement magnitude

This is reported in the **Proportional Bias Check** table.

---

### Allow fallback to simple analysis

Relevant mainly for repeated-measures mode.

Use this when you want the add-in to continue with a simple paired Bland–Altman analysis if the repeated structure is insufficient after filtering.

Example situations:

- too few subjects remain with repeated observations
- too many subjects are removed by the minimum-pairs rule

If disabled, the analysis stops instead of silently falling back.

---

### Exclude singleton subjects

Relevant for repeated-measures mode.

If enabled, subjects with only one valid paired observation are excluded from the repeated-measures pooled within-subject SD calculation.

This is usually the right choice when the goal is a genuine repeated-measures estimate of within-subject variability.

In the attached example:

- 22 subjects contributed to the repeated model
- 31 singleton subjects were excluded

---

### Min subjects

Minimum number of subjects required for the repeated-measures model.

Use this to avoid unstable repeated-measures estimates based on too few subjects.

---

### Min pairs / subject

Minimum number of valid paired observations a subject must contribute to be included in the repeated-measures SD estimation.

Use this to require that each contributing subject provides enough within-subject information.

In the attached example, the setting is:

- **Min pairs / subject = 2**

---

## Confidence Intervals

### Analytical

This option uses formula-based confidence intervals.

For the ordinary paired Bland–Altman model:

- bias CI is based on the estimated standard error of the mean difference
- LoA CIs use the standard large-sample approximation for the standard error of each LoA

For the classical bias CI:

$$
SE(\bar d) = \frac{s_d}{\sqrt{n}}
$$

and the analytical CI is:

$$
\bar d \pm c \cdot SE(\bar d)
$$

where \(c\) is either a t critical value or a z critical value depending on the selected option.

For the LoA:

$$
\text{LoA} = \bar d \pm 1.96 s_d
$$

with approximate standard error:

$$
SE(\text{LoA}) \approx s_d \sqrt{\frac{1}{n} + \frac{1.96^2}{2(n-1)}}
$$

Use Analytical when:

- sample size is moderate to large
- a fast formula-based interval is preferred
- the difference distribution is not severely non-normal

#### Jackknife

The current implementation now supports **true jackknife confidence intervals**:

- **simple Bland–Altman:** leave-one-pair-out jackknife
- **repeated-measures Bland–Altman:** leave-one-subject-out jackknife

These jackknife replicates are used to estimate the standard error and construct confidence intervals around the full-sample estimates.

This is useful when:

- you want a method less tied to a specific parametric standard-error formula
- you want a stability check relative to the analytical interval

### Bootstrap Percentile

Bootstrap resamples paired rows with replacement and forms percentile confidence intervals from the empirical bootstrap distribution.

Use this when:

- sample size is modest
- distributional assumptions are uncertain
- you want a more empirically driven interval

### Bootstrap BCa

The GUI exposes BCa, but where BCa is not yet implemented separately, the results notes should clarify whether percentile-bootstrap limits were used as fallback.

### alpha

The two-sided significance level used for confidence intervals.

Examples:

- `0.05` → 95% CI
- `0.01` → 99% CI

### Bootstrap seed and reproducibility

In the Excel GUI, **Bland–Altman** does not expose a dedicated seed input for bootstrap confidence intervals.

Therefore the bootstrap seed is resolved as follows:

1. use the **Global Settings → Default Random Seed**, if it has been set;
2. otherwise use a **time-based seed**.

When a bootstrap method is used, the output notes report the concrete seed that was actually used, for example:

- `Bootstrap seed = 123456789.`

This makes bootstrap confidence intervals reproducible when the same dataset, options, and seed are used.

!!! tip
    Set **Default Random Seed** in **Global Settings** if you want the Bland–Altman bootstrap interval to be reproducible across runs and sessions.

---

## Steps in the add-in

1. Ribbon: **BESH Stat NG → Analyse → Agreement → Bland–Altman**
2. In **Input**:
   - optionally select **Subject ID**
   - select **Reference method (X)** and **Test method (Y)**
   - choose output destination
3. In **Options**:
   - choose **Mode**
   - choose **Scale**
   - choose **X-axis**
   - choose **Plot** mode
   - set confidence-interval method and **alpha**
   - enable or disable proportional-bias and repeated-measures options as needed
4. Click **Compute**

---

## Screenshots

### Input screen

![](../assets/images/105blandaltman/105blandaltman_input.png)

### Options

![](../assets/images/105blandaltman/105blandaltman_options.png)

### Results

![](../assets/images/105blandaltman/105blandaltman_results1.png)

![](../assets/images/105blandaltman/105blandaltman_results2.png)

![](../assets/images/105blandaltman/105blandaltman_results3.png)

---

## Method and Mathematics

## Classical paired Bland–Altman

For paired differences \(d_i\):

$$
\bar d = \frac{1}{n}\sum_{i=1}^{n} d_i
$$

$$
s_d = \sqrt{\frac{1}{n-1}\sum_{i=1}^{n}(d_i - \bar d)^2}
$$

The classical limits of agreement are:

$$
\text{Lower LoA} = \bar d - 1.96 s_d
$$

$$
\text{Upper LoA} = \bar d + 1.96 s_d
$$

The **repeatability coefficient** reported by the add-in is:

$$
RC = 1.96 s_d
$$

which is the half-width of the classical LoA band.

---

## Repeated-measures Bland–Altman

When multiple paired observations exist for the same subject, the add-in can estimate a **pooled within-subject SD of the differences**.

Let subject \(j\) contribute \(n_j\) valid paired differences \(d_{ij}\). The subject mean difference is:

$$
\bar d_j = \frac{1}{n_j}\sum_{i=1}^{n_j} d_{ij}
$$

The pooled within-subject variance of the differences is:

$$
s_w^2 = \frac{\sum_j \sum_i (d_{ij} - \bar d_j)^2}{\sum_j (n_j - 1)}
$$

and the repeated-measures Bland–Altman limits use:

$$
\text{LoA} = \bar d \pm 1.96 s_w
$$

where \(\bar d\) is the overall mean difference across all valid pairs.

The add-in also reports:

- **Within-subject SD(diff)**
- **Between-subject SD(mean diff)**
- subject-level means for plotting and diagnostics

This is the repeated-measures model shown in the attached example.

---

## Proportional-bias regression

If enabled, the add-in reports an OLS regression of differences on the selected x-axis quantity.

For mean-of-methods x-axis:

$$
d_i = \alpha + \beta m_i + \varepsilon_i
$$

Reported output includes:

- slope
- t statistic
- degrees of freedom
- two-sided p-value

This is useful as a screening tool for trend in disagreement across the measurement range.

---

## Output and Interpretation

The results worksheet contains:

- method-comparison summary
- selected mode and actual mode used
- scale and x-axis definition
- CI method
- Bland–Altman agreement table
- optional repeated-measurements summary
- optional subject-level summary
- optional proportional-bias check
- Bland–Altman plot
- bootstrap seed in the notes when a bootstrap CI method is used

### Main quantities

#### Bias

The average signed difference between methods.

- positive bias: test method tends to read higher than reference
- negative bias: test method tends to read lower than reference

#### Lower and Upper LoA

The interval in which most paired differences are expected to fall.

The narrower the LoA, the better the agreement in practical units.

#### SD(diff)

In simple mode this is the SD of all paired differences.  
In repeated mode this is the pooled within-subject SD used for LoA.

#### Repeatability coefficient

Half-width of the LoA band:

$$
1.96 \times SD(diff)
$$

#### Repeated-measurements summary

In repeated mode, this provides additional structure-specific diagnostics:

- subjects used for repeated model
- subjects excluded
- within-subject SD(diff)
- between-subject SD(mean diff)
- subject means available for plot

#### Proportional Bias Check

If the slope is near zero and the p-value is not small, there is little evidence that disagreement changes with measurement magnitude.

---

## Worked example using the attached dataset

Using:

- **Subject ID:** `PacientID`
- **Reference method:** `refMethod`
- **Test method:** `testMethod`
- **Mode:** Repeated by subject
- **Scale:** Raw difference
- **X-axis:** Mean of methods
- **Plot:** All observations + subject means
- **Use t distribution for analytical CI:** checked
- **Check proportional bias:** checked
- **Allow fallback to simple analysis:** checked
- **Exclude singleton subjects:** checked
- **Min subjects:** 2
- **Min pairs / subject:** 2
- **CI method:** Analytical
- **alpha:** 0.05

The add-in reports:

- complete finite pairs: **87**
- dropped non-finite pairs: **0**
- requested mode: **RepeatedBySubject**
- mode used: **Repeated Bland–Altman**
- subjects used for repeated model: **22**
- subjects excluded: **31**

Main agreement results:

- **Bias:** 0.139416092
- **Lower LoA:** -0.122081013
- **Upper LoA:** 0.400913197
- **Within-subject SD(diff):** 0.133416890
- **Between-subject SD(mean diff):** 0.113553820
- **Repeatability coefficient:** 0.261497105

Proportional-bias output:

- slope: **-0.1611444**
- t statistic: **-1.33528274**
- df: **85**
- two-sided p-value: **0.18534881**

Interpretation of this example:

- the test method is on average about **0.139 units higher** than the reference method
- the repeated-measures LoA suggest that the difference is typically expected to lie between about **-0.122 and 0.401**
- the proportional-bias check is **not statistically significant** at the 5% level, so there is no strong evidence that the disagreement changes systematically with measurement magnitude in this dataset

---

## R code to replicate the main example

The following code reproduces the main repeated-measures Bland–Altman quantities used in the example above.

```r
# Example data
# file: 101passingbablok.csv

d <- read.csv("101passingbablok.csv")

subject <- d$PacientID
x <- d$refMethod
y <- d$testMethod

diff <- y - x
mean_xy <- (x + y) / 2

# Overall bias across all valid pairs
bias <- mean(diff)

# Repeated-measures settings used in the example
min_pairs_per_subject <- 2
exclude_singletons <- TRUE

# Keep only subjects with >= 2 pairs for repeated-measures SD estimation
subject_counts <- table(subject)
used_subjects <- names(subject_counts[subject_counts >= min_pairs_per_subject])

idx_used <- subject %in% used_subjects
subject_used <- subject[idx_used]
diff_used <- diff[idx_used]
mean_used <- mean_xy[idx_used]

# Pooled within-subject SD of differences
subject_mean_diff <- tapply(diff_used, subject_used, mean)

num <- 0
Den <- 0
for (sid in names(subject_mean_diff)) {
  di <- diff_used[subject_used == sid]
  num <- num + sum((di - mean(di))^2)
  Den <- Den + (length(di) - 1)
}

within_sd <- sqrt(num / Den)

# Repeated-measures LoA using pooled within-subject SD
loa_lower <- bias - 1.96 * within_sd
loa_upper <- bias + 1.96 * within_sd
repeatability_coefficient <- 1.96 * within_sd

# Between-subject SD of subject mean differences
between_subject_sd_mean_diff <- sd(subject_mean_diff)

# Subject-level x-axis means (for plotting)
subject_mean_x <- tapply(mean_used, subject_used, mean)

# Proportional bias check: difference ~ mean of methods
prop_bias_model <- summary(lm(diff ~ mean_xy))
```

For ordinary (non-repeated) Bland–Altman, the classical computations are:

```r
bias_simple <- mean(diff)
sd_diff_simple <- sd(diff)
loa_lower_simple <- bias_simple - 1.96 * sd_diff_simple
loa_upper_simple <- bias_simple + 1.96 * sd_diff_simple
```

---

## What options should I use, and why?

### Use **Repeated by subject** when:

- the same subject/sample appears on multiple rows
- you want LoA based on within-subject variability
- repeated observations should not be treated as fully independent

### Use **simple paired Bland–Altman** when:

- there is only one pair per subject
- or repeated structure is not relevant for the current question

### Use **Raw difference** when:

- the measurement unit itself is important clinically or scientifically
- absolute disagreement is the main concern

### Use **Percent** or **Log ratio** scales when:

- relative disagreement matters more than absolute disagreement
- method variability increases with magnitude
- ratios are more interpretable than raw differences

### Use **Analytical CI** when:

- you want fast output
- sample size is moderate or large
- distributional assumptions are acceptable

### Use **Bootstrap Percentile** when:

- you want fewer parametric assumptions
- sample size is modest
- you are comfortable with longer computation time

### Enable **Check proportional bias** when:

- you want to assess whether disagreement changes with magnitude
- method bias may depend on analyte level or signal strength

### Enable **Exclude singleton subjects** when:

- you want a genuine repeated-measures estimate of within-subject variability
- single-observation subjects should not influence pooled within-subject SD

### Enable **Allow fallback to simple analysis** when:

- repeated mode is preferred but you still want a result if the repeated structure proves insufficient after filtering

---

## Limitations

- Bland–Altman assumes that paired differences are reasonably well behaved; severe skewness or heavy tails can affect LoA interpretation.
- The classical LoA formula is approximate and is most natural when the difference distribution is not strongly non-normal.
- Repeated-measures Bland–Altman requires enough repeated information per subject to estimate pooled within-subject variability reliably.
- The proportional-bias check is a useful screen, but it is not a substitute for a full method-comparison regression model when a structured relationship is the main target.
- Log-ratio Bland–Altman requires strictly positive paired values.
- Percentage-based scales become unstable when denominators are very close to zero.

---

## Expected differences compared with other software

Small differences from other software are normal and may arise because of:

- different definitions of analytical LoA confidence intervals
- choice of t vs z critical values
- different treatment of repeated measures
- different subject-inclusion rules in repeated-measures mode
- different default x-axis convention
- different treatment of singleton subjects
- bootstrap confidence intervals may differ because of different random seeds, bootstrap interpolation conventions, or different repeated-measures implementations

In particular:

- some packages use only the classical simple-pair Bland–Altman formulas
- some repeated-measures implementations use mixed-model-based variance components rather than the pooled within-subject SD approach
- some software reports ratio-based LoA by back-transforming log-scale limits; others report them directly on the log scale

So modest discrepancies are expected even when the high-level method is the same.

---

## References

- Bland, J. M., & Altman, D. G. (1986). *Statistical methods for assessing agreement between two methods of clinical measurement.* The Lancet, 1(8476), 307–310.
- Bland, J. M., & Altman, D. G. (1999). *Measuring agreement in method comparison studies.* Statistical Methods in Medical Research, 8(2), 135–160.
- Carstensen, B. (2010). *Comparing Clinical Measurement Methods: A Practical Guide.* Wiley.
- Giavarina, D. (2015). *Understanding Bland Altman analysis.* Biochemia Medica, 25(2), 141–151.
- Zou, G. Y. (2013). *Confidence interval estimation for the Bland–Altman limits of agreement with multiple observations per individual.* Statistical Methods in Medical Research, 22(6), 630–642.

---

## See also

- [Deming Regression](deming-regression.md) — when both methods contain measurement error and a symmetric regression line is the main target.
- [Passing–Bablok Regression](passing-bablok-regression.md) — robust nonparametric method-comparison regression.
- [Lin's Concordance Correlation Coefficient](lins-ccc.md) — compact agreement coefficient decomposed into precision and accuracy.
- [Intraclass Correlation Coefficients](intraclass-correlation-coefficients.md) — reliability/agreement measures for repeated ratings or raters.
- [Cohen's / Weighted Kappa](cohens-kappa.md) — agreement for categorical or ordinal paired ratings.
