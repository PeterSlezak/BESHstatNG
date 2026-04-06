# Unpaired (two sample) T tests

**Includes:** Pooled-variance (Student) t-test, Welch t-test, F-test for variances, optional descriptive statistics and box plot.  
**Purpose:** Compare the means of two independent groups, either assuming equal variances or allowing unequal variances.

---

## Overview

This analysis compares **two independent samples** (Group 1 vs Group 2) by testing whether the group means differ:

- **Assuming equal variance** (pooled-variance / Student t-test)
- **Assuming unequal variance** (Welch t-test)

BESHStatNG always writes **both** results tables so you can report the most appropriate variant.

---

## Example dataset

In the example, the **first two columns** from the dataset below are used as the two groups:

- Column 1: *Relaxation response and biofeedback*
- Column 2: *Relaxation response alone*

Download:

- [020kruskalwalliscsv.csv](../assets/data/020kruskalwallis/020kruskalwalliscsv.csv)

> Only the first two columns are used for this unpaired t-test example. (The third column is ignored here.)

---

## Screenshots (BESHStatNG)

### Input tab
![Unpaired t-test – Input](../assets/images/014unpairedttest/014unpairedttest_input.png)

### Options tab
![Unpaired t-test – Options](../assets/images/014unpairedttest/014unpairedttest_options.png)

### Results (tables + optional box plot)
![Unpaired t-test – Results](../assets/images/014unpairedttest/014unpairedttest_results.png)

---

## When to use it

Use an unpaired (two-sample) t-test when:

- you have **two independent groups** (different subjects in each group),
- the outcome is **numeric/continuous**,
- you want to test whether the **mean differs** between groups.

Common assumptions / considerations:

- **Independence:** observations are independent within and between groups.
- **Approximate normality:** the t-test is exact under normality; it is often robust when sample sizes are moderate and not extremely skewed.
- **Variance equality (only for pooled test):** the pooled-variance test assumes equal variances; if unsure, Welch’s test is typically preferred.

---

## Inputs in Excel

BESHStatNG uses the shared “**Group by Column / Group by ID**” dialog (also used for Mann–Whitney).

### Option A: Group by Column (two ranges)

- **Group with characteristic present (Group 1):** a single-column range for Group 1 values  
- **Group with characteristic absent (Group 2):** a single-column range for Group 2 values

Example: two adjacent columns containing the two groups.

### Option B: Group by ID (two columns: group + values)

- **Group ID**: a column containing exactly two group labels (e.g., 1/2 or A/B)
- **Values**: a column containing the numeric measurement

BESHStatNG splits the values by the unique group IDs and runs the test.

### Output destination

- Output range (current sheet)
- New worksheet
- New workbook

---

## Options

Depending on your selection, the add-in may append:

- **Full Descriptive Statistics** for each group
- **Box plot** comparing the two groups
- **Alpha** — two-sided significance level used for the mean-difference confidence intervals.  
  Default: **0.05** (95% confidence interval)

---

## Steps in the add-in

1. Ribbon: **BESH Stat NG → Analyse → Parametric → Unpaired (two sample) T tests**
2. On the **Input** tab, select data:
   - either **Group by Column** (two ranges), or
   - **Group by ID** (Group ID + Values)
3. (Optional) On the **Options** tab, select:
   - **Full Descriptive Statistics**
   - **Box plot**
   - **Alpha** for the mean-difference confidence intervals
4. Choose output destination and click **Compute**

---

## What it does (math and implementation details)

Let:

- Group 1 values: \(x_1,\dots,x_{n_1}\)
- Group 2 values: \(y_1,\dots,y_{n_2}\)

Sample means:

\[
\bar{x}=\frac{1}{n_1}\sum_{i=1}^{n_1} x_i,\qquad
\bar{y}=\frac{1}{n_2}\sum_{j=1}^{n_2} y_j
\]

Sample variances (unbiased):

\[
s_x^2=\frac{1}{n_1-1}\sum_{i=1}^{n_1}(x_i-\bar{x})^2,\qquad
s_y^2=\frac{1}{n_2-1}\sum_{j=1}^{n_2}(y_j-\bar{y})^2
\]

BESHStatNG reports the mean difference as:

\[
\Delta = \bar{x}-\bar{y}
\]

### A) Pooled-variance (equal variances) t-test

Pooled variance:

\[
s_p^2=\frac{(n_1-1)s_x^2+(n_2-1)s_y^2}{n_1+n_2-2}
\]

Standard error of the mean difference:

\[
SE_{\text{pooled}}=\sqrt{s_p^2\left(\frac{1}{n_1}+\frac{1}{n_2}\right)}
\]

Test statistic:

\[
t_{\text{pooled}}=\frac{\Delta}{SE_{\text{pooled}}}
\]

Degrees of freedom:

\[
df_{\text{pooled}}=n_1+n_2-2
\]

Two-sided p-value:

\[
p = 2\,P\left(T_{df}\ge |t|\right)
\]

Confidence interval for the mean difference at level \(1-\alpha\):

\[
\Delta \pm t_{1-\alpha/2,\,df_{\text{pooled}}}\,SE_{\text{pooled}}
\]

In the current production UI, **Alpha** is user-selectable. The default is \(\alpha=0.05\), which gives a 95% confidence interval.

### B) Welch (unequal variances) t-test

Standard error:

\[
SE_{\text{Welch}}=\sqrt{\frac{s_x^2}{n_1}+\frac{s_y^2}{n_2}}
\]

Test statistic:

\[
t_{\text{Welch}}=\frac{\Delta}{SE_{\text{Welch}}}
\]

Welch–Satterthwaite degrees of freedom:

\[
df_{\text{Welch}}=
\frac{\left(\frac{s_x^2}{n_1}+\frac{s_y^2}{n_2}\right)^2}{
\frac{\left(\frac{s_x^2}{n_1}\right)^2}{n_1-1}+\frac{\left(\frac{s_y^2}{n_2}\right)^2}{n_2-1}
}
\]

Two-sided p-value and CI use the \(t\) distribution with \(df_{\text{Welch}}\):

\[
p = 2\,P\left(T_{df_{\text{Welch}}}\ge |t_{\text{Welch}}|\right)
\]

\[
\Delta \pm t_{1-\alpha/2,\,df_{\text{Welch}}}\,SE_{\text{Welch}}
\]

### C) F-test for equality of variances

BESHStatNG reports an **F-test p-value** for testing \(H_0:s_x^2=s_y^2\).

Using:

\[
F=\frac{s_x^2}{s_y^2}
\]

with degrees of freedom:

- \(df_1=n_1-1\)
- \(df_2=n_2-1\)

A common two-sided p-value is:

\[
p = 2\min\{P(F_{df_1,df_2}\le F),\;P(F_{df_1,df_2}\ge F)\}
\]

> Note: the F-test is sensitive to non-normality. If your data are strongly non-normal, consider using
> a robust variance test (e.g., Levene) or a nonparametric location test (e.g., Mann–Whitney).

---

## Output

BESHStatNG writes two main result tables:

### 1) Assuming equal variance
- **Combined SE**: \(SE_{\text{pooled}}\)
- **t**: \(t_{\text{pooled}}\)
- **df**: \(df_{\text{pooled}}\)
- **Two sided p-value**
- **mean diff (CI at selected level)**: \(\Delta\) with confidence interval at the selected level

### 2) Assuming unequal variance
- **Combined SE**: \(SE_{\text{Welch}}\)
- **t**: \(t_{\text{Welch}}\)
- **df**: \(df_{\text{Welch}}\) (typically non-integer)
- **Two sided p-value**
- the **mean difference** and its confidence interval at the selected level,
- **F test p-value**: variance equality test

If selected, it appends:

- **Descriptive statistics table** (per group)
- **Box plot** chart comparing the distributions

---

## How to interpret (quick guide)

- Prefer **Welch’s test** when variances may differ or sample sizes are unbalanced (common default in many software).
- The pooled test is appropriate when the **equal-variance assumption is reasonable**.
- Report:
  - the **mean difference** and its confidence interval at the selected level,
  - the chosen test’s **t**, **df**, and **p-value**,
  - optionally a variance check (F-test p-value) as supporting information.

---

## Relationship to R (how to reproduce)

R’s `t.test()` uses **Welch** by default. Use `var.equal = TRUE` for the pooled test. Use `var.test()` for the classical F-test.

### R code (analogous results)

```r
# Example: Unpaired t-tests for the first two columns of 020kruskalwalliscsv.csv

df <- read.csv("020kruskalwalliscsv.csv", check.names = FALSE)

x <- df[[1]]  # Group 1: Relaxation response and biofeedback
y <- df[[2]]  # Group 2: Relaxation response alone

# Drop missing values (BESHStatNG also ignores blanks/non-numeric cells)
x <- x[!is.na(x)]
y <- y[!is.na(y)]

# Welch (unequal variances) - default in R
welch <- t.test(x, y, var.equal = FALSE)

# Pooled-variance (equal variances)
pooled <- t.test(x, y, var.equal = TRUE)

# Classical F-test for equality of variances
ftest <- var.test(x, y)

# Print key outputs in a layout similar to BESHStatNG
mean_diff <- mean(x) - mean(y)

cat("=== Assuming equal variance (pooled) ===\n")
cat("t =", pooled$statistic, "\n")
cat("df =", pooled$parameter, "\n")
cat("p =", pooled$p.value, "\n")
alpha <- 0.05  # current dialog default; use 0.10 for a 90% CI

pooled <- t.test(x, y, var.equal = TRUE, conf.level = 1 - alpha)
welch  <- t.test(x, y, var.equal = FALSE, conf.level = 1 - alpha)

cat("mean diff (CI) =", mean_diff, " (", pooled$conf.int[1], "to", pooled$conf.int[2], ")\n\n")

cat("=== Assuming unequal variance (Welch) ===\n")
cat("t =", welch$statistic, "\n")
cat("df =", welch$parameter, "\n")
cat("p =", welch$p.value, "\n")
cat("mean diff (CI) =", mean_diff, " (", welch$conf.int[1], "to", welch$conf.int[2], ")\n")
cat("F test p-value =", ftest$p.value, "\n")

# Optional: derive the combined SE from t and mean difference
se_pooled <- as.numeric(mean_diff / pooled$statistic)
se_welch  <- as.numeric(mean_diff / welch$statistic)
cat("\nDerived SE pooled =", se_pooled, "\n")
cat("Derived SE Welch  =", se_welch, "\n")
```

---

## Notes

- **Minimum sample size:** the add-in requires **at least 2 values per group**.
- **Missing cells:** blank/non-numeric cells are ignored during import.
- **Direction of difference:** mean difference is computed as **Group 1 − Group 2** (order matters).

---

## See also

- [Paired (single sample) T tests](paired-t-tests.md)
- [Mann–Whitney Test](mann-whitney-test.md)
- [Normality Tests](normality-tests.md)
- [Home](../index.md)
