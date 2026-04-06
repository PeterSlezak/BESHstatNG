# Deming Regression

**Includes:** Deming (errors-in-variables) regression for method comparison, user-specified error ratio, confidence intervals via **Analytical (Linnet)**, **Analytical (closed form)** or **Jackknife**, regression plot with unity line.  
**Purpose:** Use when both X and Y have measurement error and you want a symmetric method-comparison regression.

---

## Overview

Deming regression (also called **orthogonal regression** when the error ratio is 1) is a linear errors-in-variables model used for **method comparison** when both axes are measured with error.

Let the unobserved (“true”) values satisfy a linear relationship:

$$
Y^* = \alpha + \beta X^*
$$

Observed values include measurement errors:

$$
x = X^* + \varepsilon_x, \qquad y = Y^* + \varepsilon_y
$$

Deming regression assumes:

- Errors have mean 0 and are independent of the true values.
- The ratio of measurement error variances is **known (or chosen)**:

$$
\lambda = \frac{\sigma_x^2}{\sigma_y^2}
$$

> **Important:** Some software uses the inverse definition \(\delta = \sigma_y^2/\sigma_x^2\).  
> In this add-in, the **Error Ratio** is \(\lambda = \sigma_x^2/\sigma_y^2\) (reference/test). If you have \(\delta\), set \(\lambda = 1/\delta\).

---

## Input

### Reference method (X)
Select the range containing the reference method measurements.

### Test method (Y)
Select the range containing the test method measurements.

The add-in requires paired observations: both ranges must have the same length and matching rows correspond to paired measurements.

## Options

### Confidence Interval Construction

The add-in provides three CI construction options.

#### 1) Jackknife

This option uses the “classic” delete-1 jackknife standard error based on the leave-one-out estimates \(\hat\theta_{(i)}\):

$$
SE(\hat\theta) = \sqrt{\frac{n-1}{n}\sum_{i=1}^n\left(\hat\theta_{(i)} - \bar\theta\right)^2}
$$

where \(\bar\theta\) is the mean of the leave-one-out estimates.

Confidence intervals are then:

$$
\hat\theta \pm t_{1-\alpha/2,\,df}\;SE(\hat\theta)
$$

with \(df=n-2\) used by default.

The results table footnote will show:

- **SE type = Jackknife**

#### 2) Analytical (Linnet)

This option follows the approach commonly used in method-comparison software (e.g., **mcr**) where:

- Point estimates are Deming regression estimates \(\hat\alpha, \hat\beta\).
- Standard errors are computed using **Linnet’s jackknife pseudo-values**.
- Confidence intervals are reported as Wald/t intervals with \(df = n - 2\):

$$
\hat\theta \pm t_{1-\alpha/2,\,n-2}\;SE(\hat\theta)
$$

**Linnet pseudo-values (for \(\theta\in\{\alpha,\beta\}\))**

Let \(\hat\theta\) be the estimate from the full dataset, and \(\hat\theta_{(i)}\) the estimate leaving out observation \(i\).
Pseudo-values are:

$$
p_i = n\,\hat\theta - (n-1)\,\hat\theta_{(i)}
$$

The standard error is:

$$
SE(\hat\theta) = \frac{sd(p_1,\dots,p_n)}{\sqrt{n}}
$$

The results table footnote will show:

- **SE type = Analytical - Linnet (jackknife pseudo-values)**

#### 3) Analytical (closed form / linearization)

This option computes Deming point estimates \((\hat\alpha,\hat\beta)\) in closed form and then estimates standard errors using a **linearization (Gauss–Newton / observed information) approximation**.

Define residuals \(r_i = y_i - \hat\alpha - \hat\beta x_i\) and \(D=\delta+\hat\beta^2\) where \(\delta=\sigma_y^2/\sigma_x^2 = 1/\lambda\). Let:

$$
u_i = \frac{r_i}{\sqrt{D}}
$$

Estimate \(s^2 = \sum u_i^2/(n-2)\). With Jacobian rows \(J_i=[\partial u_i/\partial \alpha,\ \partial u_i/\partial \beta]\):

$$
\frac{\partial u_i}{\partial \alpha} = -\frac{1}{\sqrt{D}}
$$

$$
\frac{\partial u_i}{\partial \beta} = -\frac{x_i}{\sqrt{D}} - \frac{\beta\,r_i}{D^{3/2}}
$$

Then:

$$
\mathrm{Cov}(\hat\theta) \approx s^2 (J^\top J)^{-1}
$$

and Wald/t confidence intervals are reported as:

$$
\hat\theta \pm t_{1-\alpha/2,\,n-2}\;SE(\hat\theta)
$$

The results table footnote will show:

- **SE type = Analytical (closed form / linearization)**

### Which confidence interval method should I prefer?

- **Jackknife**  
  Prefer for small-to-moderate sample sizes or when you want a generally reliable SE estimate with minimal extra assumptions. It is slower (requires \(n\) refits) but tends to be stable.

- **Analytical (Linnet)**  
  Prefer when you want results that align with common method-comparison software conventions (e.g., “analytical” workflows based on Linnet pseudo-values). It still requires \(n\) refits, but often matches what method-comparison users expect.

- **Analytical (closed form / linearization)**  
  Prefer for larger datasets when speed matters (no refitting) and the Deming model assumptions are reasonably met. It can be less reliable with small \(n\), weak association, or influential outliers because it relies on a linearization approximation.

---

### Alpha

The two-sided significance level \(\alpha\) used for confidence intervals. The default is `0.05`, corresponding to a 95% confidence interval.

---

### Error Ratio

The **Error Ratio** controls how errors in X and Y are weighted.

In the add-in:

$$
\lambda = \frac{\sigma_x^2}{\sigma_y^2}
$$

Interpretation:

- \(\lambda = 1\): equal measurement error variance in X and Y → **orthogonal regression**
- \(\lambda > 1\): X is noisier than Y → slope is adjusted accordingly
- \(\lambda < 1\): Y is noisier than X → slope is adjusted accordingly

## Steps in the add-in

1. Ribbon: **BESH Stat NG → Analyse → Agreement → Deming Regression**
2. In **Input**:
   - Select **Reference method (X)** and **Test method (Y)**
   - Choose output destination
3. In **Options**:
   - Select confidence interval method
   - Set **Alpha**
   - Set **Error Ratio**
4. Click **Compute**

---

## Screenshots

### Input screen

![](../assets/images/102demingregression/102demingregression_input.png)

### Options

![](../assets/images/102demingregression/102demingregression_options.png)

### Results

![](../assets/images/102demingregression/102demingregression_results1.png)

![](../assets/images/102demingregression/102demingregression_results2.png)

---

## Method and Mathematics

### Point estimates (slope and intercept)

Let \(\bar x\), \(\bar y\) be sample means and define sample (co)variances:

$$
S_{xx}=\frac{1}{n-1}\sum_{i=1}^{n}(x_i-\bar x)^2,\quad
S_{yy}=\frac{1}{n-1}\sum_{i=1}^{n}(y_i-\bar y)^2,\quad
S_{xy}=\frac{1}{n-1}\sum_{i=1}^{n}(x_i-\bar x)(y_i-\bar y)
$$

Define \(\delta = \sigma_y^2/\sigma_x^2 = 1/\lambda\).  
The Deming slope is:

$$
\hat\beta =
\frac{S_{yy}-\delta S_{xx} + \operatorname{sign}(S_{xy})\sqrt{(S_{yy}-\delta S_{xx})^2 + 4\delta S_{xy}^2}}{2S_{xy}}
$$

The intercept is:

$$
\hat\alpha = \bar y - \hat\beta\,\bar x
$$

### Objective minimized

Deming regression can be viewed as minimizing a weighted squared-distance criterion. One equivalent form used in the implementation is:

$$
Q(\alpha,\beta)=\sum_{i=1}^{n}\frac{(y_i-\alpha-\beta x_i)^2}{\delta+\beta^2}
\qquad\left(\delta=\frac{\sigma_y^2}{\sigma_x^2}\right)
$$

---

## Output and Interpretation

The results worksheet contains:

- **Test method** and **Reference method**
- **Sample size**
- **Error Ratio**
- **Slope** with CI (interpreted as proportional differences)
- **Intercept** with CI (interpreted as systematic differences)
- **SE type** (Jackknife, Analytical - Linnet (jackknife pseudo-values), or Analytical (closed form / linearization))

### How to interpret slope and intercept

| Quantity | Interpretation |
|---|---|
| \(\hat\beta \neq 1\) | Proportional difference (scaling bias) |
| \(\hat\alpha \neq 0\) | Systematic difference (constant bias) |
| CI for \(\beta\) includes 1 | No evidence of proportional bias |
| CI for \(\alpha\) includes 0 | No evidence of constant bias |

The plot shows:

- Scatter of data
- Deming regression line
- Unity line \(y=x\) for visual comparison

---

## Example dataset

The Deming regression example uses the same dataset as the Passing–Bablok help page:

- Ignore the first column (ID)
- Use:
  - Reference method (X): `refMethod`
  - Test method (Y): `testMethod`

Download the example dataset used here: [101passingbablok.csv](../assets/data/101passingbablok/101passingbablok.csv)

---

## R Code for Reference

### Using the **mcr** package (recommended for method comparison)

```r
library(mcr)

d <- read.csv("101passingbablok.csv")

# Deming regression with error ratio λ = σx^2/σy^2
# (In this example λ = 1, orthogonal regression)
fit_jack <- mcreg(
  x = d$refMethod,
  y = d$testMethod,
  method.reg = "Deming",
  method.ci  = "jackknife",
  error.ratio = 1,
  alpha = 0.05
)

fit_ana <- mcreg(
  x = d$refMethod,
  y = d$testMethod,
  method.reg = "Deming",
  method.ci  = "analytical",
  error.ratio = 1,
  alpha = 0.05
)

summary(fit_jack)
summary(fit_ana)
```

### Expected differences vs the add-in

- Different software sometimes defines the error ratio as \(\delta=\sigma_y^2/\sigma_x^2\) instead of \(\lambda=\sigma_x^2/\sigma_y^2\).  
  If results disagree when \(\lambda\neq 1\), confirm the definition and convert using \(\lambda=1/\delta\).
- “Analytical” confidence intervals can refer either to Linnet pseudo-values (refitting-based) or to closed-form/linearization SEs; different tools may use different analytical SE approximations, so CIs may not match exactly even with the same \(\lambda\).

---

## Relation to Passing–Bablok Regression

Both Deming and Passing–Bablok regression are used for method comparison (errors in both variables), but they differ in assumptions and robustness.

### Deming vs Passing–Bablok (practical guidance)

Use **Deming regression** when:

- You have (or can justify) a measurement error variance ratio \(\lambda\)
- Errors are approximately normal and outliers are limited
- You want a parametric, efficient estimator under the model

Use **Passing–Bablok regression** when:

- You prefer a robust, non-parametric method
- Outliers are possible or error distributions are non-normal
- You do not want to assume a specific error variance ratio

A common workflow is to report both:

- Passing–Bablok for robustness / sensitivity to outliers
- Deming for model-based estimation when an error ratio is available

---

## Notes and limitations

- The Deming model is sensitive to the chosen **Error Ratio**. If \(\lambda\) is misspecified, the slope can be biased.
- If the association is degenerate (e.g., \(S_{xy}=0\)), the slope is undefined.
- Confidence intervals here are Wald/t-style intervals and rely on large-sample approximations.

---

## References

- Deming, W. E. (1943). *Statistical Adjustment of Data*. Wiley.
- Fuller, W. A. (1987). *Measurement Error Models*. Wiley.
- Linnet, K. (1990). Estimation of the linear relationship between two methods of measurement. *(Introduces jackknife-based uncertainty estimation in method comparison contexts.)*
- Linnet, K. (1993). Evaluation of regression procedures for methods comparison studies. *Clinical Chemistry*, 39(3), 424–432.
- Passing, H., & Bablok, W. (1983). A new biometrical procedure for testing the equality of measurements from two different analytical methods.

## See also
- [Passing–Bablok Regression](passing-bablok-regression.md)
- [Theil-Sen Simple Regression](theil-sen-simple-regression.md)
- [Home](../index.md)