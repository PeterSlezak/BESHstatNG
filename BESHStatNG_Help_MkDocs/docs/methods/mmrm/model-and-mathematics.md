# MMRM model and mathematics

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** give the formal model definition, likelihood, covariance structures, estimation equations, degrees-of-freedom methods, and hypothesis-test mathematics used by the BESH Stat NG mixed model for repeated measures (MMRM). For a less technical introduction, see [Concepts and use cases](concepts-and-use-cases.md). For practical UI settings and output interpretation, see [Options and output reference](options-and-output.md) and [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 1. Core idea

BESH Stat NG fits MMRM as a **marginal Gaussian repeated-measures model**. The model describes the mean response through fixed effects and describes within-subject dependence through an explicit subject-level covariance matrix. In the user-facing MMRM workflow there are **no user-specified random effects**.

For subject \(i=1,\ldots,m\), let:

- \(n_i\) be the number of observed response values retained for that subject,
- \(y_i \in \mathbb{R}^{n_i}\) be the observed response vector,
- \(X_i \in \mathbb{R}^{n_i \times p}\) be the fixed-effect design matrix,
- \(\beta \in \mathbb{R}^{p}\) be the fixed-effect coefficient vector,
- \(R_i(\theta) \in \mathbb{R}^{n_i \times n_i}\) be the within-subject covariance matrix,
- \(\theta\) be the vector of covariance parameters.

The MMRM model is

\[
y_i = X_i\beta + \varepsilon_i,
\qquad
\varepsilon_i \sim N\{0, R_i(\theta)\},
\qquad
\varepsilon_i \perp \varepsilon_k \quad (i \ne k).
\]

Equivalently,

\[
y_i \sim N\{X_i\beta, R_i(\theta)\}.
\]

In the general linear mixed-model notation, the marginal covariance matrix of the response vector for subject \(i\) is denoted by \(V_i\):

\[
V_i = Z_iGZ_i^\top + R_i.
\]

Here \(V_i = \operatorname{Var}(y_i) = \operatorname{Var}(\varepsilon_i + Z_i b_i)\) is the full within-subject covariance matrix implied by the model after any random effects have been integrated out. In a general LMM, \(Z_iGZ_i^\top\) is the contribution from random effects and \(R_i\) is the residual covariance contribution.

The user-facing MMRM path sets the random-effects design absent, so \(Z_iGZ_i^\top\) is not active and the full marginal covariance is simply the within-subject residual covariance:

\[
V_i = R_i.
\]

!!! important "Marginal interpretation"
    MMRM coefficients, LS-means, and contrasts are interpreted as marginal fixed-effect quantities. The model does not estimate subject-specific random intercepts or random slopes in the user-facing MMRM release.

---

## 2. Example model used throughout the MMRM documentation

The worked examples use the FEV1 dataset distributed with the OpenPharma `mmrm` package and included in this help project as:

- [037mmrm_fev_data.csv](../../assets/data/037mmrm/037mmrm_fev_data.csv)
- [037mmrm_fev_results.xlsx](../../assets/data/037mmrm/037mmrm_fev_results.xlsx)

The example mirrors the OpenPharma between-within example:

```r
FEV1 ~ RACE + SEX + ARMCD * AVISIT + us(AVISIT | USUBJID)
```

In BESH Stat NG terms:

- response: `FEV1`,
- subject ID: `USUBJID`,
- visit/order variable: `VISITN`,
- fixed effects: `RACE`, `SEX`, `ARMCD`, `AVISIT`, and `ARMCD × AVISIT`,
- covariance structure: **Unstructured**,
- estimation method in the supplied workbook: **REML**,
- inference method in the supplied workbook: **Between-within DF**.

The CSV contains 800 scheduled subject-visit rows. After removing rows with missing `FEV1`, the analysis uses 537 observed responses from 197 subjects and 4 scheduled visits.

---

## 3. Stacked matrix notation

It is often useful to stack all observed responses and design matrices:

\[
y =
\begin{bmatrix}
y_1\\
y_2\\
\vdots\\
y_m
\end{bmatrix},
\qquad
X =
\begin{bmatrix}
X_1\\
X_2\\
\vdots\\
X_m
\end{bmatrix},
\qquad
V(\theta)=\operatorname{blockdiag}\{R_1(\theta),\ldots,R_m(\theta)\}.
\]

Then

\[
y \sim N\{X\beta, V(\theta)\}.
\]

Let

\[
N = \sum_{i=1}^{m} n_i
\]

be the number of observed response rows used in the likelihood, and let \(p\) be the rank/number of estimable fixed-effect columns in \(X\).

---

## 4. Fixed-effect estimation for a given covariance

For a fixed value of \(\theta\), BESH Stat NG obtains \(\hat\beta(\theta)\) by generalized least squares:

\[
\hat\beta(\theta)
= \left(\sum_{i=1}^{m} X_i^\top R_i^{-1}X_i\right)^{-1}
  \left(\sum_{i=1}^{m} X_i^\top R_i^{-1}y_i\right).
\]

Define

\[
C(\theta)=\sum_{i=1}^{m} X_i^\top R_i^{-1}X_i,
\qquad
b(\theta)=\sum_{i=1}^{m} X_i^\top R_i^{-1}y_i.
\]

Then

\[
\hat\beta(\theta)=C(\theta)^{-1}b(\theta).
\]

The model-based large-sample covariance of \(\hat\beta\), before optional small-sample adjustment, is

\[
\Phi(\theta)=C(\theta)^{-1}.
\]

BESH Stat NG evaluates these quantities block by block. It uses Cholesky factorization of each \(R_i\), and solves linear systems rather than explicitly forming \(R_i^{-1}\) wherever possible.

---

## 5. Profile ML likelihood

The full Gaussian log-likelihood is

\[
\ell(\beta,\theta)
= -\frac{1}{2}\left[
N\log(2\pi)+\log|V(\theta)|+
\{y-X\beta\}^\top V(\theta)^{-1}\{y-X\beta\}
\right].
\]

Because \(V\) is block diagonal,

\[
\log|V(\theta)| = \sum_{i=1}^{m}\log|R_i(\theta)|.
\]

After profiling out \(\beta\), define the residual quadratic form

\[
Q(\theta)=
\{y-X\hat\beta(\theta)\}^\top V(\theta)^{-1}\{y-X\hat\beta(\theta)\}.
\]

The profiled ML objective minimized by the engine is

\[
-2\ell_{ML}\{\hat\beta(\theta),\theta\}
= \log|V(\theta)|+Q(\theta)+N\log(2\pi).
\]

The reported log-likelihood is

\[
\ell_{ML}=-\frac{1}{2}\{-2\ell_{ML}\}.
\]

---

## 6. Restricted maximum likelihood (REML)

REML estimates covariance parameters after accounting for the fixed effects. In the profiled form used by BESH Stat NG, the REML objective is

\[
-2\ell_R(\theta)
= \log|V(\theta)| + \log|C(\theta)| + Q(\theta) + (N-p)\log(2\pi).
\]

where

\[
C(\theta)=X^\top V(\theta)^{-1}X.
\]

The reported REML log-likelihood is

\[
\ell_R=-\frac{1}{2}\{-2\ell_R\}.
\]

### Why REML is the default

REML is usually preferred for covariance-parameter estimation in Gaussian mixed models and MMRM because it adjusts for the loss of information used to estimate fixed effects. BESH Stat NG therefore uses REML as the practical default. Kenward-Roger inference in BESH Stat NG requires REML; if Kenward-Roger is requested from the UI with ML selected, the UI changes the fit method to REML before fitting.

### When ML is useful

ML can be useful when comparing **different fixed-effect mean models** fitted to the same response data and covariance structure. REML likelihoods depend on the fixed-effect design matrix, so REML information criteria are diagnostic and should not be used to compare models with different fixed-effect specifications.

---

## 7. Information criteria and fit statistics

BESH Stat NG reports the optimized objective, log-likelihood, AIC, BIC, quadratic form, determinant components, and profile scale.

Let \(k_\theta\) be the number of covariance parameters. For ML fits, the parameter count used for AIC/BIC includes fixed effects and covariance parameters:

\[
k = p + k_\theta.
\]

For REML fits, BESH Stat NG reports REML-based AIC/BIC as diagnostic quantities using the covariance-parameter count:

\[
k = k_\theta.
\]

Then

\[
\operatorname{AIC} = -2\ell + 2k,
\qquad
\operatorname{BIC} = -2\ell + \log(N)k.
\]

The profile scale reported in the output is

\[
\widehat\sigma^2_{profile}=\frac{Q}{df},
\qquad
 df =
\begin{cases}
N-p, & \text{REML},\\
N, & \text{ML}.
\end{cases}
\]

For the supplied FEV1 workbook, the REML fit uses \(N=537\), \(m=197\), \(p=11\), and an unstructured 4-visit covariance matrix with 10 covariance parameters. The reported REML objective is 3386.4499, the log-likelihood is −1693.2249, and the profile scale \(Q/(N-p)\) is approximately 1.0000.

---

## 8. Visit indexing and observed covariance submatrices

Let the planned study visits be indexed \(1,\ldots,T\). A subject does not have to be observed at every visit. BESH Stat NG constructs covariance matrices in a global visit space and then uses the submatrix corresponding to the visits observed for each subject.

If the full visit-level covariance matrix is \(R_{full}(\theta)\) and subject \(i\) is observed at visit indices \(s_i=(s_{i1},\ldots,s_{in_i})\), then

\[
R_i(\theta)=R_{full}(\theta)[s_i,s_i].
\]

For example, if a subject is observed only at visits 2 and 4, then \(R_i\) is the 2 × 2 submatrix of \(R_{full}\) using rows and columns 2 and 4.

!!! note "Visit order versus visit label"
    The covariance structure uses the visit/order variable to determine visit ordering and covariance layout. The mean model may also include a categorical visit factor. In the FEV1 example, `VISITN` supplies numeric order while `AVISIT` supplies categorical fixed-effect levels.

---

## 9. Covariance structures

BESH Stat NG supports the following MMRM residual covariance structures. The table uses \(T\) for the number of retained visit levels.

| Structure | Free parameters | Main use |
|---|---:|---|
| Identity / independence | 1 | Baseline or sensitivity structure when no within-subject correlation is modeled. |
| Diagonal heterogeneous | \(T\) | Visit-specific variances with zero covariance. |
| Compound symmetry | 2 | Common variance and common correlation. |
| Heterogeneous compound symmetry | \(T+1\) | Visit-specific variances with common correlation. |
| AR(1) | 2 | Common variance with correlation decaying by visit lag. |
| Heterogeneous AR(1) | \(T+1\) | Visit-specific variances with AR(1) correlation decay. |
| Toeplitz (TOEP) | \(T\) | Common variance with a separate correlation for each visit lag. |
| Heterogeneous Toeplitz (TOEPH) | \(2T-1\) | Visit-specific variances with separate lag correlations. |
| Unstructured | \(T(T+1)/2\) | Fully flexible covariance; default when supported by sample size. |

### 9.1 Identity / independence

\[
R_i = \sigma^2 I_{n_i}.
\]

All retained observations have the same variance and are modeled as conditionally independent within subject. This is usually too restrictive for true repeated-measures data but can be useful as a diagnostic baseline.

### 9.2 Diagonal heterogeneous

\[
R_i = \operatorname{diag}\left(\sigma^2_{s_{i1}},\ldots,\sigma^2_{s_{in_i}}\right).
\]

The variance can differ by visit, but off-diagonal covariances are zero. This structure can be useful when visit-specific spread matters but within-subject residual correlation is weak or deliberately ignored for sensitivity analysis.

### 9.3 Compound symmetry

\[
R_i = \sigma^2\{(1-\rho)I_{n_i}+\rho J_{n_i}\},
\]

where \(J_{n_i}\) is the all-ones matrix. All visits have the same variance and every distinct pair of visits has the same correlation \(\rho\).

### 9.4 Heterogeneous compound symmetry

\[
\operatorname{Var}(Y_{it})=\sigma_t^2,
\qquad
\operatorname{Corr}(Y_{it},Y_{is})=\rho \quad (t\ne s).
\]

Therefore

\[
\operatorname{Cov}(Y_{it},Y_{is})=\rho\sigma_t\sigma_s \quad (t\ne s).
\]

This is more flexible than compound symmetry because each visit can have its own variance.

### 9.5 AR(1)

\[
\operatorname{Cov}(Y_{it},Y_{is})=\sigma^2\rho^{\lvert t-s\rvert}.
\]

This structure assumes correlations decline with visit distance. In BESH Stat NG, the lag \(\lvert t-s \rvert\) is based on the ordinal visit index, not raw continuous time.

### 9.6 Heterogeneous AR(1)

\[
\operatorname{Var}(Y_{it})=\sigma_t^2,
\qquad
+\operatorname{Corr}(Y_{it},Y_{is})=\rho^{\lvert t-s\rvert}.
\]

Therefore

\[
\operatorname{Cov}(Y_{it},Y_{is})=\sigma_t\sigma_s\rho^{\lvert t-s\rvert}.
\]

This is often a practical compromise when the number of visits is too large for an unstructured covariance but visit-specific variances should still be allowed.

### 9.7 Toeplitz (TOEP)

Toeplitz covariance uses one common variance and one correlation parameter for each visit lag:

\[
\operatorname{Cov}(Y_{it},Y_{is})=\sigma^2
\rho_{\lvert t-s\rvert}, \qquad 
\rho_0=1.
\]

For \(T\) retained visits, this gives one variance parameter and \(T-1\) lag-correlation parameters, for a total of \(T\) parameters. Toeplitz is less restrictive than AR(1) because each lag can have its own correlation, but it is much less parameter-heavy than an unstructured covariance.

### 9.8 Heterogeneous Toeplitz (TOEPH)

Heterogeneous Toeplitz allows visit-specific variances while keeping the Toeplitz lag-correlation pattern:

\[
\operatorname{Var}(Y_{it})=\sigma_t^2,
\qquad
\operatorname{Corr}(Y_{it},Y_{is})=
\rho_{\lvert t-s\rvert}, \qquad 
\rho_0=1.
\]

Therefore

\[
\operatorname{Cov}(Y_{it},Y_{is})=\sigma_t\sigma_s
\rho_{\lvert t-s\rvert}.
\]

For \(T\) retained visits, this gives \(T\) variance parameters and \(T-1\) lag-correlation parameters, for a total of \(2T-1\) parameters. This structure is useful when variances differ by visit and correlations mainly depend on lag, but the AR(1) single-decay assumption is too restrictive.

### 9.9 Unstructured

The unstructured covariance estimates every variance and covariance among the \(T\) visits:

\[
R_{full}=
\begin{bmatrix}
\sigma_{11} & \sigma_{12} & \cdots & \sigma_{1T}\\
\sigma_{21} & \sigma_{22} & \cdots & \sigma_{2T}\\
\vdots & \vdots & \ddots & \vdots\\
\sigma_{T1} & \sigma_{T2} & \cdots & \sigma_{TT}
\end{bmatrix}.
\]

The number of covariance parameters is

\[
\frac{T(T+1)}{2}.
\]

For the FEV1 example, \(T=4\), so the unstructured covariance has 10 parameters. The supplied BESH Stat NG workbook reports the following user-scale covariance estimate:

|  | Visit 1 | Visit 2 | Visit 3 | Visit 4 |
|---|---:|---:|---:|---:|
| Visit 1 | 40.5545 | 14.3961 | 4.9761 | 13.3768 |
| Visit 2 | 14.3961 | 26.5715 | 2.7836 | 7.4772 |
| Visit 3 | 4.9761 | 2.7836 | 14.8979 | 0.9033 |
| Visit 4 | 13.3768 | 7.4772 | 0.9033 | 95.5561 |

The corresponding correlations are derived as

\[
\operatorname{Corr}_{ts}=\frac{R_{ts}}{\sqrt{R_{tt}R_{ss}}}.
\]

---

## 10. Internal covariance parameterization

The optimizer works on unconstrained or nearly unconstrained internal parameters. The output covariance and correlation matrices are transformed back to the user scale.

| Parameter type | Internal scale | User scale |
|---|---|---|
| Homogeneous variance | \(\theta=\log(\sigma^2)\) | \(\sigma^2=\exp(\theta)\) |
| Visit-specific variance | \(\theta_t=\log(\sigma_t^2)\) | \(\sigma_t^2=\exp(\theta_t)\) |
| Correlation | \(\eta=\operatorname{atanh}(\rho)\) | \(\rho=\tanh(\eta)\) |
| Toeplitz lag correlations | partial-autocorrelation scale | positive-definite Toeplitz correlation matrix |
| Unstructured covariance | lower-triangular Cholesky factor | \(R=LL^\top\) |

For unstructured covariance, BESH Stat NG parameterizes a lower-triangular matrix \(L\). Diagonal entries are exponentiated to keep them positive, while off-diagonal entries are unconstrained:

\[
R_{full}=LL^\top.
\]

For four visits,

\[
L=
\begin{bmatrix}
\exp(a_1) & 0 & 0 & 0\\
\ell_{21} & \exp(a_2) & 0 & 0\\
\ell_{31} & \ell_{32} & \exp(a_3) & 0\\
\ell_{41} & \ell_{42} & \ell_{43} & \exp(a_4)
\end{bmatrix}.
\]

The covariance table shown to the user is \(LL^\top\), not the internal optimizer vector.

---

## 11. Optimization

BESH Stat NG profiles fixed effects out of the likelihood and optimizes only over covariance parameters. The main loop is:

1. choose starting covariance parameters,
2. build \(R_i(\theta)\) for each subject,
3. compute Cholesky decompositions and determinant terms,
4. accumulate \(X_i^\top R_i^{-1}X_i\), \(X_i^\top R_i^{-1}y_i\), and \(y_i^\top R_i^{-1}y_i\),
5. solve for \(\hat\beta(\theta)\),
6. compute the profiled ML or REML objective,
7. update \(\theta\) until convergence.

### Optimizer choices

The UI exposes covariance optimizer modes corresponding to the implementation paths:

| UI option | Mathematical role |
|---|---|
| AI/Fisher scoring (default) | Average-information / Fisher-scoring style covariance-parameter updates where available. |
| Projected BFGS (auto gradient) | Quasi-Newton optimization with analytic gradients when available and numerical fallback otherwise. |
| Projected BFGS (analytic gradient) | Quasi-Newton optimization using analytic covariance gradients when available. |
| Projected BFGS (finite-difference gradient) | Quasi-Newton optimization using numerical finite-difference gradients. |

The engine treats invalid covariance proposals, non-positive-definite covariance blocks, or rank-deficient fixed-effect cross-products as failed objective evaluations. During optimization, such proposals are assigned a deliberately poor objective-function value, so the optimizer moves away from non-positive-definite or numerically unstable regions rather than accepting them as valid fits.

---

## 12. Fixed-effect coefficient inference

For coefficient \(\beta_j\), define

\[
SE(\hat\beta_j)=\sqrt{\widehat{\operatorname{Var}}(\hat\beta_j)}.
\]

The usual test statistic is

\[
t_j=\frac{\hat\beta_j}{SE(\hat\beta_j)}.
\]

Depending on the selected inference method, BESH Stat NG reports either a large-sample normal test or a \(t\)-test with denominator degrees of freedom.

### 12.1 Large-sample normal

Large-sample normal inference uses the model-based covariance matrix \(\Phi=C^{-1}\) and reports normal-reference p-values:

\[
p_j = 2\{1-\Phi_N(|t_j|)\},
\]

where \(\Phi_N\) is the standard normal distribution function. In the coefficient table this is displayed as a \(z\)-style test.

### 12.2 Residual degrees of freedom

Residual-DF inference uses

\[
df = N-p
\]

for all fixed-effect coefficients and contrasts. This is simple and transparent, but it does not distinguish between subject-level and within-subject effects.

### 12.3 Between-within degrees of freedom

Between-within inference assigns denominator degrees of freedom according to whether a fixed-effect column is subject-constant or varies within subject. For MMRM there is one grouping level: subject.

Let:

- \(N_0=1\) if the model includes an intercept, otherwise \(0\),
- \(N_1=m\), the number of subjects with at least one observed response,
- \(N_2=N\), the number of observed response rows,
- \(p_1\) be the rank contribution of between-subject fixed effects excluding the intercept,
- \(p_2\) be the rank contribution of within-subject fixed effects.

Then

\[
DF_{between}=N_1-(N_0+p_1),
\]

and

\[
DF_{within}=N_2-(N_1+p_2).
\]

BESH Stat NG computes between/within membership from the expanded fixed-effect design columns. Columns that are constant within every subject use between-subject DF; columns that vary within at least one subject use within-subject DF. The intercept is treated specially and uses within-subject DF, following the R `mmrm`-style MMRM convention.

For the FEV1 model:

- \(N_1=197\),
- \(N_2=537\),
- \(N_0=1\),
- between-subject parameters: `RACE` has 2 columns, `SEX` has 1 column, and `ARMCD` has 1 column, so \(p_1=4\),
- within-subject parameters: `AVISIT` has 3 columns and `ARMCD × AVISIT` has 3 columns, so \(p_2=6\).

Therefore

\[
DF_{between}=197-(1+4)=192,
\]

and

\[
DF_{within}=537-(197+6)=334.
\]

These are the denominator degrees of freedom shown in the supplied BESH Stat NG workbook: subject-constant effects such as `RACE`, `SEX`, and `ARMCD` use 192 DF, while visit effects, treatment-by-visit interactions, and the intercept use 334 DF.

### 12.4 Satterthwaite degrees of freedom

Satterthwaite inference accounts for uncertainty in covariance-parameter estimation by approximating the variance estimate of a contrast as a scaled chi-square variable.

For a one-row contrast \(l^\top\beta\), define

\[
\hat\psi = l^\top \widehat{\operatorname{Var}}(\hat\beta)l.
\]

Let \(g\) be the gradient of \(\hat\psi\) with respect to the covariance-parameter vector:

\[
g = \frac{\partial \hat\psi}{\partial\theta}.
\]

Let \(W=\widehat{\operatorname{Var}}(\hat\theta)\). BESH Stat NG approximates \(W\) from the Hessian of the profiled ML/REML criterion. Because the criterion is \(-2\ell\), the approximation is

\[
W \approx 2H^{-1},
\]

where \(H\) is the numerical Hessian of the profiled criterion. Then

\[
\widehat{\operatorname{Var}}(\hat\psi) \approx g^\top Wg,
\]

and the Satterthwaite denominator degrees of freedom are

\[
df_{Sat} = \frac{2\hat\psi^2}{g^\top Wg}.
\]

If the Satterthwaite calculation is numerically unavailable for a fitted MMRM, BESH Stat NG falls back to between-within degrees of freedom and records a warning.

### 12.5 Kenward-Roger inference

Kenward-Roger inference adjusts the fixed-effect covariance matrix and computes moment-matched denominator degrees of freedom for fixed-effect tests. It is designed to improve small-sample inference in Gaussian mixed models and MMRM. Mathematically, the adjustment is based on a Taylor-series approximation to the sampling variation introduced by estimating the covariance parameters rather than treating them as known.

Let

\[
\Phi = (X^\top V^{-1}X)^{-1}
\]

be the model-based covariance matrix of \(\hat\beta\). Let \(W=\widehat{\operatorname{Var}}(\hat\theta)\), and let \(P_h\), \(Q_{hj}\), and \(R_{hj}\) denote Kenward-Roger derivative matrices with respect to covariance parameters \(\theta_h\) and \(\theta_j\). BESH Stat NG builds these matrices by subject-block aggregation.

The full Kenward-Roger adjusted covariance matrix has the form

\[
\Phi_A
= \Phi + 2\Phi
\left\{
\sum_{h=1}^{k}\sum_{j=1}^{k}
W_{hj}\left(Q_{hj}-P_h\Phi P_j-\frac{1}{4}R_{hj}\right)
\right\}
\Phi.
\]

The linear Kenward-Roger approximation omits the second-derivative \(R_{hj}\) term.

For a one-row contrast \(l^\top\beta\), BESH Stat NG uses the KR-adjusted standard error

\[
SE_{KR}(l^\top\hat\beta)=\sqrt{l^\top\Phi_A l}.
\]

For a multi-row hypothesis

\[
H_0: L\beta=0,
\]

the unscaled Wald statistic is

\[
F_0 = \frac{1}{q}(L\hat\beta)^\top(L\Phi_A L^\top)^{-1}(L\hat\beta),
\]

where \(q=\operatorname{rank}(L)\). The reported KR F statistic uses a scaling factor \(\lambda\):

\[
F = \lambda F_0.
\]

BESH Stat NG computes \(\lambda\) and denominator degrees of freedom by R `mmrm`-style moment matching. If the requested \(L\) matrix is rank deficient, the effective hypothesis is rank-reduced before inversion.

!!! note "REML requirement"
    Kenward-Roger inference in BESH Stat NG requires REML. This matches the usual theoretical basis of the KR adjustment and avoids reporting KR results from an ML covariance fit.

---

## 13. Linear estimates, LS-means, and contrasts

All LS-means, changes from baseline, treatment differences, and difference-in-change estimates are linear functions of \(\beta\).

For a row vector \(l\),

\[
\widehat\mu_l=l^\top\hat\beta,
\]

and

\[
\widehat{\operatorname{Var}}(\widehat\mu_l)=l^\top\widehat{\operatorname{Var}}(\hat\beta)l.
\]

For a contrast matrix \(L\),

\[
\widehat\delta=L\hat\beta,
\qquad
\widehat{\operatorname{Var}}(\widehat\delta)=L\widehat{\operatorname{Var}}(\hat\beta)L^\top.
\]

The variance matrix used in these formulas depends on the selected inference method:

- large-sample normal, residual DF, between-within DF, and Satterthwaite use the model-based \(\Phi\) for standard errors;
- Kenward-Roger uses the adjusted \(\Phi_A\) when available.

The practical construction of \(L\) for observed-design grids, reference grids, pairwise differences, control differences, change from baseline, and difference in change is documented in [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 14. Type III / term-level tests

Term-level tests are also linear-hypothesis tests. For a model term represented by one or more expanded coefficient columns, BESH Stat NG builds a restriction matrix \(L\) selecting the columns associated with that term.

The null hypothesis is

\[
H_0: L\beta=0.
\]

The Wald quadratic form is

\[
\chi^2=(L\hat\beta)^\top
\{L\widehat{\operatorname{Var}}(\hat\beta)L^\top\}^{-1}
(L\hat\beta).
\]

For F-style reporting,

\[
F = \lambda\frac{\chi^2}{q},
\]

where \(q=\operatorname{rank}(L)\), and \(\lambda=1\) unless an inference method supplies a scaling factor, such as Kenward-Roger.

Term grouping follows the authored model terms after categorical expansion. For example, in the FEV1 model:

| Authored term | Expanded coefficient rows | Test type |
|---|---|---|
| `RACE` | `RACE` non-reference levels | multi-df between-subject test |
| `SEX` | `SEX` non-reference level | one-df between-subject test |
| `ARMCD` | treatment non-reference level | one-df between-subject test |
| `AVISIT` | visit non-reference levels | multi-df within-subject test |
| `ARMCD × AVISIT` | treatment-by-visit interaction columns | multi-df within-subject test |

Rank-deficient or redundant restrictions are reduced to an effective full-row-rank hypothesis before the F statistic is computed.

---

## 15. Missing data and likelihood contribution

Rows with missing response values do not contribute an observed response to the likelihood. A subject contributes the vector of observed responses and the corresponding design rows and covariance submatrix.

If subject \(i\) has observed visit set \(s_i\), then the likelihood uses

\[
y_i(s_i),
\qquad
X_i(s_i),
\qquad
R_{full}(\theta)[s_i,s_i].
\]

This is why MMRM can use subjects with incomplete visit histories. The method is likelihood based under the fitted model and the usual ignorability/MAR framing. It does not impute missing responses and does not by itself protect against missing-not-at-random mechanisms.

!!! warning "Missing data are a scientific assumption, not a software option"
    MMRM uses the observed data likelihood. The validity of inference depends on whether the mean model, covariance model, and missing-data assumptions are reasonable for the study context.

---

## 16. Assumptions

The main assumptions are:

1. **Continuous Gaussian response conditional on covariates.** The repeated outcome should be reasonably modeled as multivariate normal within subject.
2. **Correct or adequate mean model.** Treatment, visit, treatment-by-visit interaction, baseline, baseline-by-visit interaction, stratification factors, and other covariates should be chosen according to the estimand and design.
3. **Independent subjects.** Correlation is modeled within subject; different subjects are assumed independent.
4. **Adequate covariance structure.** The selected covariance should be flexible enough to represent the repeated-measures dependence without overfitting beyond the data support.
5. **Identifiable fixed effects.** The fixed-effect design matrix must have sufficient rank for the requested model terms and contrasts.
6. **Positive-definite covariance.** Fitted covariance matrices must be finite, symmetric, and positive definite for the retained visit patterns.
7. **Ignorable missingness for likelihood inference.** Incomplete responses are handled through the observed likelihood under the assumed model; MNAR dropout requires sensitivity analysis.

---

## 17. Relation to R `mmrm` and SAS `PROC MIXED`

The BESH Stat NG MMRM implementation intentionally follows the same broad marginal-model formulation used in modern MMRM software: a fixed-effect mean model plus a subject-level covariance model, with no user-facing random-effects specification in the MMRM workflow.

### Similarities with R `mmrm`

- The model is marginal and subject-block based.
- Unstructured covariance is represented through covariance parameters and subject-level covariance submatrices.
- Between-within DF follows the same MMRM-style subject/observation-level logic used in the OpenPharma between-within example.
- Kenward-Roger uses KR-adjusted fixed-effect covariance and R `mmrm`-style moment matching for denominator DF and F scaling.

### Differences to expect

Small numerical differences against R `mmrm`, SAS `PROC MIXED`, or other packages can occur because of:

- optimizer choice and convergence tolerances,
- covariance-parameter scale,
- Cholesky parameterization details,
- finite-difference versus analytic derivatives,
- rank handling and estimability checks,
- how denominator DF are assigned to general contrasts,
- treatment of near-singular covariance or fixed-effect cross-product matrices.

For example, the OpenPharma between-within article notes that SAS and R `mmrm` use different conventions for between-within DF in some cases, especially for unstructured covariance and general contrast statements. Therefore, exact agreement should be judged in the context of a fully matched model, data screening, covariance structure, parameterization, and inference method.

---

## 18. Practical mathematical checklist

Before interpreting an MMRM result, verify the following:

- \(N\), number of subjects, and visit counts match the intended analysis set.
- The fixed-effect model matches the estimand.
- The covariance structure has a reasonable number of parameters for the number of subjects and visits.
- The fit converged without serious numerical warnings.
- The covariance matrix and correlation matrix are plausible.
- The chosen degrees-of-freedom method matches the analysis plan.
- Primary conclusions come from planned LS-means and contrasts, not only from dummy-coded coefficient rows.
- Missingness has been summarized and sensitivity analyses considered where needed.

---

## References and further reading

- Cnaan A, Laird NM, Slasor P. *Using the general linear mixed model to analyse unbalanced repeated measures and longitudinal data.* Statistics in Medicine. 1997.
- Kenward MG, Roger JH. *Small sample inference for fixed effects from restricted maximum likelihood.* Biometrics. 1997.
- Schluchter MD, Elashoff JT. *Small-sample adjustments to tests with unbalanced repeated measures assuming several covariance structures.* Journal of Statistical Computation and Simulation. 1990.
- Mallinckrodt CH, Lane PW, Schnell D. *Recommendations for the primary analysis of continuous endpoints in longitudinal clinical trials.* Drug Information Journal. 2008.
- OpenPharma `mmrm` documentation: [Mixed Models for Repeated Measures](https://openpharma.github.io/mmrm/), [Model Fitting Algorithm](https://openpharma.github.io/mmrm/latest-tag/articles/algorithm.html), [Between-Within](https://openpharma.github.io/mmrm/latest-tag/articles/between_within.html), [Kenward-Roger](https://openpharma.github.io/mmrm/latest-tag/articles/kenward.html), and [Satterthwaite](https://openpharma.github.io/mmrm/latest-tag/articles/satterthwaite.html).
- SAS Institute. *PROC MIXED documentation*, especially repeated-measures covariance structures, LSMEANS/ESTIMATE/CONTRAST statements, and denominator degrees-of-freedom options.
