# Sample Size – Independent Proportions

**Includes:** Sample size for two independent proportions (uncorrected and corrected chi-square / Fisher).  
**Purpose:** Estimate sample sizes for comparing two independent proportions (with optional group-size ratio κ).

---

## Overview

This tool estimates the required sample size for comparing **two independent proportions** (control vs experimental) using a **two-sided** test.

It reports two estimates:

1. **Uncorrected chi-square test** (large-sample normal approximation)
2. **Corrected chi-square or Fisher’s exact test** (a conservative inflation applied to the uncorrected estimate)

> **Data entry:** All inputs are typed directly into the dialog controls (no worksheet ranges).

---

## Dialog

### Input screen

![](../assets/images/048samplesizeindependentproportions/048samplesizeindependentproportions_input.png)

### Results (written to the sheet)

![](../assets/images/048samplesizeindependentproportions/048samplesizeindependentproportions_results.png)

---

## When to use

Use this tool to plan sample size for comparing two **independent** event proportions, for example response rates, adverse event rates, or conversion rates across two arms.

---

## Hypotheses

Let:

- \(p_C\) = control-group proportion
- \(p_E\) = experimental-group proportion

Two-sided test:

$$
H_0: p_C = p_E
\qquad\text{vs}\qquad
H_1: p_C \ne p_E
$$

---

## Inputs

- **Control Group Proportion**: \(p_C\)
- **Experimental Group Proportion**: \(p_E\)
- **Ratio of control to experimental subjects**: \(\kappa = n_C / n_E\)
- **Alpha**: \(\alpha\) (two-sided; the tool uses \(\alpha/2\))
- **Beta**: \(\beta\) (power is \(1-\beta\))

---

## Method used by the add-in

### Common quantities

Define the allocation ratio \(\kappa = n_C/n_E\), and the weighted planning proportion:

$$
\bar p = \frac{\kappa p_C + p_E}{\kappa + 1}
$$

Let:

$$
z_{1-\alpha/2} = \Phi^{-1}(1-\alpha/2),
\qquad
z_{1-\beta} = \Phi^{-1}(1-\beta)
$$

where \(\Phi^{-1}\) is the standard normal quantile function.

### 1) Uncorrected chi-square estimate

The add-in computes the required experimental-group size:

$$
n_E^{(unc)}=
\left\lceil
\frac{
\left[
 z_{1-\alpha/2}\sqrt{(1+\kappa)\bar p(1-\bar p)}
 +
 z_{1-\beta}\sqrt{p_C(1-p_C)+\kappa p_E(1-p_E)}
\right]^2
}{
\kappa (p_E-p_C)^2
}
\right\rceil
$$

Then controls are:

$$
n_C^{(unc)} = \left\lfloor \kappa\, n_E^{(unc)} \right\rfloor
$$

**Rounding note (matches the VB.NET code):** \(n_E\) is rounded **up** (ceiling). \(n_C\) is computed as \(\kappa n_E\) and then **truncated** to an integer (for positive \(\kappa\), this is equivalent to floor).

### 2) Corrected chi-square / Fisher’s exact estimate

A conservative correction is applied by inflating the uncorrected experimental size:

$$
n_E^{(cor)}=
\left\lceil
\frac{n_E^{(unc)}}{4}
\left(
 1+\sqrt{1+\frac{2(\kappa+1)}{n_E^{(unc)}\,\kappa\,\lvert p_C-p_E\rvert}}
\right)^2
\right\rceil
$$

Controls:

$$
n_C^{(cor)} = \left\lfloor \kappa\, n_E^{(cor)} \right\rfloor
$$

This value is intended to be more conservative when a continuity correction (e.g., Yates) or an exact test would be preferred.

---

## Output

For each run, the tool prints:

- the inputs used (\(p_C\), \(p_E\), \(\kappa\), \(\alpha\), \(\beta\))
- **Uncorrected chi-square test**: estimated \(n_C\) and \(n_E\)
- **Corrected chi-square or Fisher’s exact test**: estimated \(n_C\) and \(n_E\)

---

## R code (to reproduce the add-in)

The function below reproduces the add-in’s results exactly (same formulas and rounding):

```r
ss_indep_prop_addin <- function(pC, pE, kappa, alpha, beta) {
  pbar <- (kappa * pC + pE) / (kappa + 1)
  z_a  <- qnorm(1 - alpha/2)
  z_b  <- qnorm(1 - beta)

  nE_unc <- ( z_a * sqrt((1 + kappa) * pbar * (1 - pbar)) +
             z_b * sqrt(pC * (1 - pC) + kappa * pE * (1 - pE)) )^2 /
            (kappa * (pE - pC)^2)

  nE_unc <- ceiling(nE_unc)
  nC_unc <- floor(kappa * nE_unc)

  nE_cor <- (nE_unc / 4) * (1 + sqrt(1 + (2 * (kappa + 1)) /
                                     (nE_unc * kappa * abs(pC - pE))))^2
  nE_cor <- ceiling(nE_cor)
  nC_cor <- floor(kappa * nE_cor)

  list(
    uncorrected = c(nC = nC_unc, nE = nE_unc),
    corrected   = c(nC = nC_cor, nE = nE_cor)
  )
}

# Examples matching the screenshots
ss_indep_prop_addin(0.4, 0.5, kappa = 1, alpha = 0.05, beta = 0.2)
ss_indep_prop_addin(0.4, 0.5, kappa = 2, alpha = 0.02, beta = 0.2)
```

---

## Comparison with common R functions (expected differences)

- **`power.prop.test()` (base R)** uses a normal approximation for two proportions. It is typically used for **equal group sizes**; if you need an allocation ratio, you must adapt the calculation. It also uses a different variance structure than the add-in’s “uncorrected” formula, so results may differ by a few subjects.
- Methods targeting **Yates-corrected chi-square** or **Fisher’s exact** power often produce larger \(n\) than uncorrected normal-approximation results. The add-in’s “corrected” line is a conservative inflation intended to approximate that behavior.

---

## Notes and limitations

- These calculations assume independent Bernoulli outcomes and a fixed target difference \(p_E - p_C\).
- Large-sample approximations are most reliable when expected cell counts are not small.
- Plan for attrition/non-response by inflating the final \(n\) as needed.

## References

- Fleiss, J. L., Levin, B., & Paik, M. C. (2013). Statistical Methods for Rates and Proportions. Wiley.
- Chow, S.-C., Shao, J., & Wang, H. (2008). Sample Size Calculations in Clinical Research. Chapman & Hall/CRC.
- Agresti, A. (2019). An Introduction to Categorical Data Analysis. Wiley.

## See also
- [Proportions](proportions.md)
- [Sample Size - Single Proportion](sample-size-single-proportion.md)
- [Sample Size – Paired T-test](sample-size-paired-t-test.md)
- [Home](../index.md)
