# Sample Size – Unpaired T-test

**Includes:** Sample size for unpaired t-test (with group ratio \(\kappa\)).  
**Purpose:** Estimate required sample sizes per group for a two-sample t-test given effect size, SD, \(\alpha\) and power.

---

## Overview

This tool estimates sample size for comparing **two independent group means** using the (pooled-variance) two-sample t-test.

You enter the planning values directly into the dialog:

- **Mean difference** (clinically meaningful difference to detect)
- **Standard deviation** (assumed common SD)
- **Ratio of control to experimental subjects** \(\kappa\)
- **Alpha** \(\alpha\) (two-sided)
- **Beta** \(\beta\) (Type II error; power \(=1-\beta\))

The add-in reports the estimated number of **controls** and **experimental subjects**.

---

## Screenshots (BESHStatNG)

### Input
![Sample size – Unpaired t-test – Input](../assets/images/046samplesizeunpairedt/046samplesizeunpairedt_input.png)

### Results (and Save to sheet)
![Sample size – Unpaired t-test – Results](../assets/images/046samplesizeunpairedt/046samplesizeunpairedt_results.png)

---

## When to use it

Use this tool when you are planning a study with:

- **two independent groups** (different subjects in each group),
- a **continuous outcome**,
- a target **mean difference** to detect, and
- an assumed **common SD**.

Key assumptions / considerations:

- The classical two-sample t-test assumes the outcome is approximately normal within groups.
- The calculation assumes a **common SD** across groups and targets the **pooled-variance** two-sample t-test.
- \(\alpha\) is treated as **two-sided** (the tool uses \(\alpha/2\) internally).
- If you expect unequal variances or strongly non-normal data, consider planning with more conservative assumptions.

---

## Inputs

All inputs are typed directly into the dialog (no worksheet range selection).

- **Mean Difference** \(\delta\): the difference in group means you want to be able to detect.
- **Standard Deviation** \(\sigma\): the assumed (common) SD of the outcome in each group.
- **Ratio of control to experimental subjects** \\(\kappa = n_C / n_E\\) (controls : experimental)
  where \(n_C\) is the control sample size and \(n_E\) is the experimental sample size.
- **Alpha** \(\alpha\): significance level (two-sided).
- **Beta** \(\beta\): Type II error; power is \(1-\beta\).

---

## Steps in the add-in

1. In Excel ribbon: **BESH Stat NG → Analyse → Sample Size → Unpaired T-test**
2. Enter the planning inputs in the dialog.
3. Click **Compute**.
4. (Optional) Click **Save** to write the results to a new worksheet.

> Tip: Each click on **Compute** appends a new block of output to the results box.

---

## What it does (math and implementation details)

Let:

- \(\delta\) = target mean difference
- \(\sigma\) = assumed common SD
- \(\alpha\) = two-sided significance level
- \(\beta\) = Type II error (power \(=1-\beta\))
- \(\kappa = n_C / n_E\) = allocation ratio (controls : experimental)

For a pooled-variance two-sample t-test, the standard error of the mean difference is:

\[
SE(\bar{x}_C-\bar{x}_E) = \sigma\sqrt{\frac{1}{n_C}+\frac{1}{n_E}}
= \sigma\sqrt{\frac{1+1/\kappa}{n_E}}
\]

### 1) Initial estimate (normal approximation)

The add-in first computes a normal-based starting point (rounded up to the next integer):

\[
\hat{n}_{E,0} = \left\lceil (1+1/\kappa)\left(\frac{\sigma\,(z_{1-\alpha/2}+z_{1-\beta})}{\delta}\right)^2 \right\rceil
\]

and sets \(n_E=\hat{n}_{E,0}\).

### 2) Iterative refinement using t critical values

The tool then iteratively increases \(n_E\) until \(n_E\) exceeds a t-based criterion:

- degrees of freedom (pooled): \\(df = n_C + n_E - 2\\)
- t critical values: \(t_{1-\alpha/2,df}\) and \(t_{1-\beta,df}\)

At each step it computes:

\[
\text{Crit}(n_E)= (1+1/\kappa)\left(\frac{\sigma\,(t_{1-\alpha/2,df}+t_{1-\beta,df})}{\delta}\right)^2
\]

and increments \(n_E\) until:

\[
 n_E > \text{Crit}(n_E)
\]

### Reported sample sizes

The dialog reports:

- **Experimental subjects**: \(n_E\)
- **Controls**: \(n_C = \lfloor \kappa\,n_E \rfloor\)

> Implementation note: the VB.NET code prints controls as `Int(nt * Kappa)` (truncation toward \(-\infty\) for positive values). For integer \(\kappa\) this is exactly \(\kappa n_E\).

---

## Output

The results box prints a short summary per run, for example (from the screenshot):

- \(\delta=5\), \(\sigma=10\), \(\kappa=1\), \(\alpha=0.05\), \(\beta=0.2\)  → **64 controls** and **64 experimental**
- \(\delta=2\), \(\sigma=10\), \(\kappa=1\), \(\alpha=0.01\), \(\beta=0.1\)  → **746 controls** and **746 experimental**
- \(\delta=2\), \(\sigma=10\), \(\kappa=2\), \(\alpha=0.01\), \(\beta=0.1\)  → **1118 controls** and **559 experimental**

Click **Save** to write the output lines to a new worksheet (one line per row).

---

## Reproducing results in R

### A) Equal allocation (\(\kappa=1\)) using `power.t.test`

For equal group sizes, base R can solve for \(n\) directly:

```r
# Example: mean difference = 2, SD = 10, alpha = 0.01, beta = 0.1
alpha <- 0.01
beta  <- 0.10
power <- 1 - beta

delta <- 2
sd    <- 10

res <- power.t.test(delta = delta, sd = sd,
                    sig.level = alpha, power = power,
                    type = "two.sample", alternative = "two.sided")
res

n_per_group <- ceiling(res$n)
n_per_group
```

Notes:

- `power.t.test(type="two.sample")` uses the **noncentral t distribution** for the two-sample t-test and solves for \(n\).
- The add-in uses a **t-quantile iteration** (a different numerical approach), so results can differ by a small amount (often 0–1 subject per group).

### B) Unequal allocation (\(\kappa\neq 1\)) using an exact noncentral-t power function

Base R does not provide a direct `power.t.test()` solver for unequal group sizes, but you can compute power using the noncentral t distribution and then search for the smallest \(n_E\).

```r
# Power for a two-sample pooled t-test with unequal n
power_two_sample <- function(nE, kappa, delta, sd, alpha = 0.05) {
  nC <- floor(kappa * nE)
  df <- nC + nE - 2
  se <- sd * sqrt(1/nC + 1/nE)
  ncp <- delta / se

  tcrit <- qt(1 - alpha/2, df)
  # Two-sided power under alternative with positive delta
  1 - (pt(tcrit, df, ncp = ncp) - pt(-tcrit, df, ncp = ncp))
}

# Find minimal nE to reach target power
n_two_sample_ratio <- function(delta, sd, kappa, alpha, beta, nmax = 1e6) {
  target <- 1 - beta
  nE <- 2
  while (nE <= nmax) {
    if (power_two_sample(nE, kappa, delta, sd, alpha) >= target) break
    nE <- nE + 1
  }
  nC <- floor(kappa * nE)
  list(nE = nE, nC = nC, power = power_two_sample(nE, kappa, delta, sd, alpha))
}

# Example matching the screenshot block with kappa = 2
n_two_sample_ratio(delta = 2, sd = 10, kappa = 2, alpha = 0.01, beta = 0.10)
```

Expected differences:

- The **noncentral-t** approach in section B targets the classical two-sample t-test power more directly.

---

## References

- Chow, S.-C., Shao, J., & Wang, H. (2008). *Sample Size Calculations in Clinical Research* (2nd ed.). Chapman & Hall/CRC.
- Julious, S. A. (2009). *Sample Sizes for Clinical Trials*. Chapman & Hall/CRC.
- Cohen, J. (1988). *Statistical Power Analysis for the Behavioral Sciences* (2nd ed.). Lawrence Erlbaum.
- Fleiss, J. L., Levin, B., & Paik, M. C. (2003). *Statistical Methods for Rates and Proportions* (3rd ed.). Wiley.
- Hedges, L. V., & Olkin, I. (1985). *Statistical Methods for Meta-Analysis*. Academic Press. (Effect size conventions)

## See also

- [Sample Size – Paired T-test](sample-size-paired-t-test.md)
- [Unpaired (two sample) T tests](unpaired-two-sample-t-tests.md)
- [Paired T tests](paired-t-tests.md)
- [Home](../index.md)