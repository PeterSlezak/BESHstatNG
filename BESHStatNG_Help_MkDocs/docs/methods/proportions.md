# Proportions

**Includes:** Single proportion (estimate + confidence interval), Two independent proportions (difference + confidence interval, Fisher exact p-values), Two paired proportions (difference + confidence interval, Liddell/McNemar-type test).  
**Purpose:** Work with binomial outcomes: estimate proportions and compare proportions between groups.

---

## Overview

Use **Proportions** when your outcome is binary (e.g., responder vs non‑responder, event vs no event).

- **Single proportion**: estimate the event rate in one sample and test against a reference proportion.
- **Two independent**: compare event rates between **two unrelated** groups (two samples).
- **Two paired**: compare event rates between **two matched/paired** measurements on the same subjects (before/after, left/right, etc.).

### Assumptions and notes

- Observations are counts of successes out of totals.
- For the CI methods used here (Wilson/Newcombe), performance is generally better than Wald CIs, especially for small samples or proportions near 0/1.
- Exact p‑values are used where indicated.

---

## Inputs
This dialog accepts **counts** (not raw individual‑level records).

> Tip: Even though counts are typed into the form, you can keep the raw data in a worksheet and simply copy the totals into the numeric fields.

### Analysis types

#### Single

- **Total number of observations**: \(n\)
- **Number of responders**: \(x\)

#### Two Independent

- Sample 1: total \(n_1\), responders \(x_1\)
- Sample 2: total \(n_2\), responders \(x_2\)

#### Paired

- **Total number of observations**: \(n\)
- **Number of responders in 1st category only**: \(b\)
- **Number of responders in 2nd category only**: \(c\)
- **Number of responders in both categories**: \(a\)

The implied 2×2 matched‑pairs table is:

$$
\begin{array}{c|cc}
 & \text{Category 2 = Yes} & \text{Category 2 = No}\\\hline
\text{Category 1 = Yes} & a & b\\
\text{Category 1 = No}  & c & d
\end{array}
\qquad d = n - a - b - c.
$$

---

## Using the add‑in

1. In Excel ribbon: **BESH Stat NG → Analyse → Contingency Table Analysis → Proportions**.
2. Choose **Single**, **Two Independent**, or **Paired**.
3. Enter the required counts.
4. Set **Alpha** for the confidence interval level.  
   Default: **0.05** (95% confidence interval).
5. Choose output destination (**New Worksheet**, **New Workbook**, or **Output Range**).
6. Click **Compute**.

---

## Screenshots

**Two independent:**

![](../assets/images/029proportions/029proportions_input_independent.png)

**Paired:**

![](../assets/images/029proportions/029proportions_input_paired.png)

**Single:**

![](../assets/images/029proportions/029proportions_input_single.png)

**Example output (two independent):**

![](../assets/images/029proportions/029proportions_results_independent.png)

**Example output (paired):**

![](../assets/images/029proportions/029proportions_results_paired.png)

**Example output (single):**

![](../assets/images/029proportions/029proportions_results_single.png)

---

## Output and interpretation

### Single proportion

- **Proportion**: \(\hat p = x/n\).
- **Confidence interval**: Wilson score interval at the selected level \(1-\alpha\).
- **two-sided P-value**: exact binomial test of \(H_0: p = 0.5\) (two-sided).

### Two independent proportions

- **Proportion in sample 1/2**: \(\hat p_1=x_1/n_1\), \(\hat p_2=x_2/n_2\).
- **Proportions Difference**: \(\hat\Delta = \hat p_1 - \hat p_2\).
- **Confidence interval**: Newcombe (Wilson-based) interval for the difference at the selected level \(1-\alpha\).
- **Exact two-sided P-value** and **Exact mid two-sided P-value**: Fisher’s exact test on the corresponding 2×2 table.

### Two paired proportions

- **Proportion in the 1st/2nd category**: \(\hat p_1=(a+b)/n\), \(\hat p_2=(a+c)/n\).
- **Proportions Difference**: \(\hat\Delta = \hat p_1-\hat p_2 = (b-c)/n\).
- **Confidence interval**: Wilson-based interval with paired-correlation adjustment at the selected level \(1-\alpha\).
- **Two-sided P-value**: Liddell’s exact McNemar test (exact binomial on discordant pairs).

---
## Statistical methods

### 1) Single proportion

#### Estimate

$$
\hat p = \frac{x}{n}.
$$

#### Confidence interval at level \(1-\alpha\) (Wilson score)

BESHStatNG uses the **Wilson score interval** with \(z = z_{1-\alpha/2}\). In the dialog, \(\alpha\) is user-selectable; the default is \(\alpha=0.05\), which gives a 95% confidence interval:

$$
\tilde p = \frac{x + \tfrac{z^2}{2}}{n+z^2},
\qquad
h = \frac{z}{n+z^2}\sqrt{\frac{x(n-x)}{n} + \frac{z^2}{4}},
$$

$$
\text{CI}_{1-\alpha} = \big[\,\tilde p - h,\; \tilde p + h\,\big].
$$

This interval generally performs better than the Wald interval, especially for small \(n\) or \(\hat p\) near 0 or 1.

#### Exact two-sided p-value (binomial test)

BESHStatNG reports an **exact** two‑sided p‑value for testing:

$$
H_0: p = 0.5.
$$

It computes:

$$
k = \min(x,\,n-x),
\qquad
p_{2s} = 2\,\Pr\{X \le k\},\; X\sim\mathrm{Bin}(n,0.5),
$$

and caps at 1 (i.e., \(p_{2s}=\min(p_{2s},1)\)). This matches the usual two‑sided binomial test reported by common statistical software.

---

### 2) Two independent proportions

Let \(x_1\) successes out of \(n_1\) in sample 1, and \(x_2\) out of \(n_2\) in sample 2.

#### Estimate

$$
\hat p_1 = \frac{x_1}{n_1},\qquad \hat p_2 = \frac{x_2}{n_2},\qquad \hat\Delta = \hat p_1-\hat p_2.
$$

#### Confidence interval for \(\Delta\) at level \(1-\alpha\) (Newcombe / Wilson)

BESHStatNG uses the **Newcombe (1998) score-based** CI for the difference in proportions. Compute Wilson CIs for each proportion:

$$
[L_1,U_1] \text{ for } p_1,\qquad [L_2,U_2] \text{ for } p_2.
$$

Then form the Newcombe CI:

$$
\Delta_L = \hat\Delta - \sqrt{(\hat p_1 - L_1)^2 + (U_2 - \hat p_2)^2},
$$

$$
\Delta_U = \hat\Delta + \sqrt{(U_1 - \hat p_1)^2 + (\hat p_2 - L_2)^2}.
$$

#### Exact p-values (Fisher exact)

BESHStatNG also reports Fisher’s exact test p‑values on the corresponding 2×2 table:

$$
\begin{array}{c|cc}
 & \text{Sample 1} & \text{Sample 2}\\\hline
\text{Success} & x_1 & x_2\\
\text{Failure} & n_1-x_1 & n_2-x_2
\end{array}
$$

- **Exact two-sided P-value:** two-sided Fisher p-value using probability ordering.
- **Exact mid two-sided P-value:** mid-p adjustment (subtract half the observed-table probability in each tail).

> This is the same Fisher calculation used in the **2×2 Table** tool. If you enter the same counts in both tools, the Fisher p‑values match.

---

### 3) Two paired proportions

For matched pairs, define:

- \(a\): “Yes/Yes” (responders in both categories)
- \(b\): “Yes/No” (1st category only)
- \(c\): “No/Yes” (2nd category only)
- \(d\): “No/No”, where \(d=n-a-b-c\)

#### Estimate

The marginal proportions are:

$$
\hat p_1 = \frac{a+b}{n},\qquad \hat p_2 = \frac{a+c}{n},
$$

and the difference is:

$$
\hat\Delta = \hat p_1-\hat p_2 = \frac{b-c}{n}.
$$

#### Confidence interval at level \(1-\alpha\) (Wilson + correlation adjustment)

BESHStatNG builds Wilson limits for each marginal proportion and then adjusts for within‑pair correlation.

1) Compute Wilson limits \([L_1,U_1]\) for \(\hat p_1\) using \(x=a+b\) and \(n\), and \([L_2,U_2]\) for \(\hat p_2\) using \(x=a+c\) and \(n\).

2) Compute an association term (\(\phi\)) from the paired 2×2 table (with a small (paired correlation) adjustment):

$$
A = (a+b)(n-a-b)(a+c)(n-a-c),
$$

$$
B = a\,d - b\,c.
$$

Define

$$
C =
\begin{cases}
B - \tfrac{n}{2}, & B > \tfrac{n}{2}\\
0, & 0 \le B \le \tfrac{n}{2}\\
B, & B < 0
\end{cases}
\qquad
\phi = \frac{C}{\sqrt{A}}.
$$

3) The CI is:

$$
\Delta_L = \hat\Delta - \sqrt{(\hat p_1-L_1)^2 - 2\phi(\hat p_1-L_1)(U_2-\hat p_2) + (U_2-\hat p_2)^2},
$$

$$
\Delta_U = \hat\Delta + \sqrt{(\hat p_2-L_2)^2 - 2\phi(\hat p_2-L_2)(U_1-\hat p_1) + (U_1-\hat p_1)^2}.
$$

#### Exact p‑value (Liddell / McNemar)

To test equality of paired proportions (marginal homogeneity), BESHStatNG uses **Liddell’s exact McNemar test**, which depends only on discordant pairs \(b\) and \(c\):

$$
H_0: \Pr(\text{Yes/No}) = \Pr(\text{No/Yes}).
$$

Under \(H_0\), conditional on \(b+c\), we have \(X\sim\mathrm{Bin}(b+c,0.5)\), and the two‑sided p‑value is:

$$
p = 2\,\Pr\{X \le \min(b,c)\}.
$$

This is the same test used by the **2×2 Table** tool when **Paired Data** is selected.

---

## Example (same counts as the 2×2 Table help)

The screenshots use the same 2×2 counts as the **2×2 Table** example:

- Sample 1: \(n_1=159\), \(x_1=99\)
- Sample 2: \(n_2=74\), \(x_2=32\)

For the paired example:

- \(n=233\), \(a=99\), \(b=60\), \(c=32\), \(d=42\)

---

## R code (reference)

Below are R examples to reproduce the same results. Small differences can occur if you use different CI methods or different definitions of “two‑sided Fisher p‑value.”

### Single proportion (Wilson CI + exact binomial test)

```r
x <- 60
n <- 233

# Estimate
p_hat <- x/n

# Wilson score CI
if (!requireNamespace("binom", quietly = TRUE)) install.packages("binom")
ci_wilson <- binom::binom.confint(x, n, methods = "wilson")
ci_wilson[, c("lower", "upper")]

# Exact two-sided binomial p-value for H0: p = 0.5
binom.test(x, n, p = 0.5, alternative = "two.sided")$p.value
```

### Two independent proportions (Newcombe CI + Fisher exact + mid-p)

```r
x1 <- 99; n1 <- 159
x2 <- 32; n2 <- 74

# Difference in proportions
p1 <- x1/n1
p2 <- x2/n2
diff <- p1 - p2

# Newcombe (1998) CI for difference
if (!requireNamespace("DescTools", quietly = TRUE)) install.packages("DescTools")
DescTools::BinomDiffCI(x1, n1, x2, n2, method = "Newcombe")

# Fisher exact test (two-sided)
T <- matrix(c(x1, n1-x1, x2, n2-x2), nrow = 2, byrow = TRUE)
fisher.test(T)$p.value

# Mid-p Fisher (matches the add-in's 'Exact Mid two-sided P-value')
if (!requireNamespace("exact2x2", quietly = TRUE)) install.packages("exact2x2")
exact2x2::fisher.exact(T, tsmethod = "minlike", midp = TRUE)$p.value
```

> Notes:
> - `fisher.test()` reports a standard two-sided p-value; the exact two-sided definition can vary slightly between implementations. BESHStatNG uses probability ordering and reports a mid‑p variant as well.

### Two paired proportions (paired CI as implemented + exact McNemar/Liddell)

```r
n <- 233
b <- 60  # category 1 only
c <- 32  # category 2 only
 a <- 99 # both
 d <- n - a - b - c

p1 <- (a + b)/n
p2 <- (a + c)/n
Delta <- p1 - p2

# --- CI for Delta (as implemented in BESHStatNG) ---
alpha <- 0.05   # match the dialog default; change to 0.10 for a 90% CI
wilson_limits <- function(x, n, alpha = 0.05) {
  z <- qnorm(1 - alpha/2)
  p <- x/n
  a <- 2*x + z^2
  b <- z * sqrt(z^2 + 4*x*(1 - p))
  c <- 2*(n + z^2)
  L <- (a - b)/c
  U <- (a + b)/c
  c(L = L, U = U)
}

L1U1 <- wilson_limits(a + b, n)
L2U2 <- wilson_limits(a + c, n)
L1 <- L1U1["L"]; U1 <- L1U1["U"]
L2 <- L2U2["L"]; U2 <- L2U2["U"]

A <- (a+b)*(n-a-b)*(a+c)*(n-a-c)
B <- a*d - b*c
Ccorr <- if (B > n/2) B - n/2 else if (B >= 0) 0 else B
phi <- if (A == 0) 0 else Ccorr / sqrt(A)

Delta_L <- Delta - sqrt((p1 - L1)^2 - 2*phi*(p1 - L1)*(U2 - p2) + (U2 - p2)^2)
Delta_U <- Delta + sqrt((p2 - L2)^2 - 2*phi*(p2 - L2)*(U1 - p1) + (U1 - p1)^2)

c(Delta = Delta, lower = Delta_L, upper = Delta_U)

# --- Exact McNemar / Liddell p-value (two-sided) ---
# Conditional on discordants (b+c), test b ~ Bin(b+c, 0.5)
binom.test(min(b, c), b + c, p = 0.5)$p.value * 2  # equivalent two-sided form

# Or directly using binom.test with the observed discordant count:
binom.test(b, b + c, p = 0.5, alternative = "two.sided")$p.value
```

---

## Notes

- **Same data as the 2×2 Table tool:**
  - The **Two independent** proportions analysis corresponds to a 2×2 table with rows = success/failure and columns = sample.
  - The **Paired** proportions p‑value corresponds to the **paired (McNemar/Liddell)** option in the 2×2 tool.
- **Why CI methods may differ across software:** Wilson/Newcombe intervals are method‑dependent; different packages may default to Wald, Wilson, Agresti‑Coull, or exact methods.

## References

- Wilson E.B. (1927). Probable inference, the law of succession, and statistical inference. Journal of the American Statistical Association, 22, 209-212.
- Newcombe R.G. (1998). Improved confidence intervals for the difference between binomial proportions based on paired data. Statistics in Medicine 17:2635-2650.
- Newcombe R.G., Altman D.G. (2000). Proportions and their differences. In Statistics with Confidence 2nd ed. London, BMJ Publishing Group, 45-56.
- Newcombe R.G. (1998). Interval estimation for the difference between independent proportions. Statistics in Medicine 17:873-890.

## See also

- [2×2 Table](2x2-table.md)
- [RxC Table](rxc-table.md)
- [Home](../index.md)

