# LMM model and mathematics

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** give the formal model definition, likelihood, covariance notation, estimation equations, degrees-of-freedom methods, and key assumptions used by BESH Stat NG linear mixed models (LMM). For a less technical introduction, see [Concepts and use cases](concepts-and-use-cases.md). For practical option guidance, see [Options and output reference](options-and-output.md) and [Random effects and covariance structures](random-effects-and-covariance.md).

---

## 1. Core idea

BESH Stat NG fits Gaussian **linear mixed models** for continuous responses. An LMM combines:

- a **fixed-effect mean model**, which estimates population-level effects;
- a **random-effect model**, which allows subjects, clusters, sites, batches, or other grouping units to deviate from the population average;
- an optional **R-side residual covariance model**, which describes residual covariance that remains after the fixed and random effects are included.

For subject or cluster \(i=1,\ldots,m\), the model is

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i.
\]

The components are:

| Symbol | Meaning |
|---|---|
| \(y_i\) | \(n_i \times 1\) response vector for subject/cluster \(i\). |
| \(X_i\) | \(n_i \times p\) fixed-effect design matrix. |
| \(\beta\) | \(p \times 1\) fixed-effect coefficient vector. |
| \(Z_i\) | \(n_i \times q\) random-effect design matrix. |
| \(b_i\) | \(q \times 1\) subject/cluster-specific random-effect vector. |
| \(\varepsilon_i\) | \(n_i \times 1\) residual-error vector. |

The distributional assumptions are

\[
b_i \sim N(0,G),
\qquad
\varepsilon_i \sim N(0,R_i),
\qquad
b_i \perp \varepsilon_i,
\]

with independent subjects/clusters:

\[
(b_i,\varepsilon_i) \perp (b_k,\varepsilon_k) \quad (i \ne k).
\]

The random-effects covariance matrix \(G\) is called the **G-side covariance**. The residual covariance matrix \(R_i\) is called the **R-side covariance**.

!!! important "Conditional and marginal views"
    Conditional on the random effects, the expected response is \(X_i\beta + Z_i b_i\). After the random effects are integrated out, the marginal expected response is \(X_i\beta\). Fixed-effect estimates describe the population-average mean model; random-effect estimates describe model-based subject/cluster deviations from that mean.

---

## 2. Data blocks and variable roles

The model is built by grouping retained worksheet rows by the selected subject or cluster identifier. The identifier may be text or numeric in the ribbon workflow. Its role is to define blocks; it is not treated as a numeric covariate unless the user also supplies a numeric-coded version as a model predictor.

For each subject/cluster block:

- \(n_i\) is the number of retained observations for that block;
- \(X_i\) is built from the selected fixed-effect terms;
- \(Z_i\) is built from the selected random-effect terms;
- \(R_i\) is built from the selected residual covariance structure;
- \(G\) is common across subjects/clusters for a given model.

The optional visit/time/order variable has a different role from the subject ID. It supplies the within-subject ordering used by visit-indexed residual covariance structures. It is required for R-side structures such as diagonal heterogeneous, heterogeneous compound symmetry, AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, and unstructured residual covariance. It is not required for identity residual covariance or ordinary compound-symmetry residual covariance.

!!! tip "Subject ID, time, and random slopes are separate concepts"
    A subject ID groups rows. A visit/time/order variable orders residuals within each subject when the residual covariance structure needs an order. A predictor such as time or dose can also be used as a fixed effect and as a random slope. These roles may use the same worksheet column in some analyses, but they are conceptually different.

---

## 3. Marginal covariance

After integrating over the random effects, the marginal distribution of \(y_i\) is

\[
y_i \sim N\{X_i\beta, V_i\},
\]

where

\[
V_i = Z_iGZ_i^\top + R_i.
\]

The term \(Z_iGZ_i^\top\) is the covariance contribution induced by random effects. For example, a random intercept creates positive covariance among observations from the same subject because all observations share the same subject-specific intercept deviation.

The term \(R_i\) is the residual covariance after fixed and random effects. In a simple random-intercept or random-slope model, \(R_i\) is often identity residual covariance: independent residuals with a common residual variance. More complex R-side structures can be used when residuals remain correlated or have visit-specific variability after the random effects are included.

### Example: random intercept only

For a random-intercept model with

\[
Z_i = \mathbf{1}_{n_i},
\qquad
G = \sigma_b^2,
\qquad
R_i = \sigma^2 I_{n_i},
\]

we have

\[
V_i = \sigma_b^2\mathbf{1}_{n_i}\mathbf{1}_{n_i}^\top + \sigma^2 I_{n_i}.
\]

The covariance between two different observations from the same subject is \(\sigma_b^2\), and the marginal variance of each observation is \(\sigma_b^2 + \sigma^2\).

### Example: random intercept and random slope

For a time variable \(t\), a random-intercept and random-slope model has a two-column random-effect design:

\[
Z_i =
\begin{bmatrix}
1 & t_{i1}\\
1 & t_{i2}\\
\vdots & \vdots\\
1 & t_{in_i}
\end{bmatrix}.
\]

With an unstructured two-by-two random-effects covariance matrix,

\[
G=
\begin{bmatrix}
\sigma_0^2 & \sigma_{01}\\
\sigma_{01} & \sigma_1^2
\end{bmatrix},
\]

where \(\sigma_0^2\) is the random-intercept variance, \(\sigma_1^2\) is the random-slope variance, and \(\sigma_{01}\) is their covariance.

The implied marginal covariance between two observations at times \(t_{ij}\) and \(t_{ik}\), before adding residual covariance, is

\[
\operatorname{Cov}(Z_{ij}b_i, Z_{ik}b_i)
= \sigma_0^2 + t_{ij}t_{ik}\sigma_1^2 + (t_{ij}+t_{ik})\sigma_{01}.
\]

This shows why random slopes create covariance patterns that depend on the predictor values, not only on visit lag.

---

## 4. Stacked notation

Stacking all retained observations gives

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
V(\theta) = \operatorname{blockdiag}\{V_1(\theta),\ldots,V_m(\theta)\}.
\]

The stacked marginal model is

\[
y \sim N\{X\beta, V(\theta)\},
\]

where \(\theta\) denotes the covariance parameters used to build \(G\) and \(R_i\). Let

\[
N = \sum_{i=1}^{m} n_i
\]

be the number of retained observations, and let \(p\) be the number of fixed-effect columns used in the model after expansion.

---

## 5. Fixed-effect estimation for a given covariance

For a fixed value of the covariance parameters \(\theta\), BESH Stat NG estimates \(\beta\) by generalized least squares:

\[
\hat\beta(\theta)
= \left(\sum_{i=1}^{m} X_i^\top V_i^{-1}X_i\right)^{-1}
  \left(\sum_{i=1}^{m} X_i^\top V_i^{-1}y_i\right).
\]

Define

\[
C(\theta)=\sum_{i=1}^{m} X_i^\top V_i^{-1}X_i,
\qquad
b(\theta)=\sum_{i=1}^{m} X_i^\top V_i^{-1}y_i.
\]

Then

\[
\hat\beta(\theta)=C(\theta)^{-1}b(\theta).
\]

The model-based covariance matrix of the fixed-effect estimates, before optional small-sample adjustment, is

\[
\Phi(\theta)=C(\theta)^{-1}.
\]

BESH Stat NG evaluates these quantities subject by subject and uses matrix factorizations and linear solves rather than explicitly forming large dense inverse matrices wherever possible.

---

## 6. Maximum likelihood

The full Gaussian log-likelihood is

\[
\ell(\beta,\theta)
= -\frac{1}{2}
\left[
N\log(2\pi) + \log|V(\theta)|
+ \{y-X\beta\}^\top V(\theta)^{-1}\{y-X\beta\}
\right].
\]

Because \(V\) is block diagonal,

\[
\log|V(\theta)| = \sum_{i=1}^{m}\log|V_i(\theta)|.
\]

After profiling out \(\beta\), define the residual quadratic form

\[
Q(\theta)=
\{y-X\hat\beta(\theta)\}^\top V(\theta)^{-1}\{y-X\hat\beta(\theta)\}.
\]

The profiled ML objective minimized by the engine is

\[
-2\ell_{ML}\{\hat\beta(\theta),\theta\}
= \log|V(\theta)| + Q(\theta) + N\log(2\pi).
\]

The reported ML log-likelihood is

\[
\ell_{ML}=-\frac{1}{2}\{-2\ell_{ML}\}.
\]

---

## 7. Restricted maximum likelihood

Restricted maximum likelihood (REML) estimates covariance parameters after accounting for the fixed effects. In the profiled form used by BESH Stat NG,

\[
-2\ell_R(\theta)
= \log|V(\theta)| + \log|C(\theta)| + Q(\theta) + (N-p)\log(2\pi),
\]

where

\[
C(\theta)=X^\top V(\theta)^{-1}X.
\]

The reported REML log-likelihood is

\[
\ell_R=-\frac{1}{2}\{-2\ell_R\}.
\]

### Why REML is usually the starting choice

REML is commonly preferred for estimating variance components in Gaussian mixed models because it accounts for the fixed-effect degrees of freedom used by the mean model. It is often the practical default for final LMM fits and for small-sample fixed-effect inference.

Kenward-Roger inference in BESH Stat NG is designed for REML fits. If Kenward-Roger is requested from the ribbon workflow with ML selected, the workflow should use REML for the fitted model.

### When ML is useful

ML is useful when comparing models that differ in their fixed-effect mean structure but are fitted to the same response data under comparable covariance assumptions. REML likelihoods depend on the fixed-effect design, so REML information criteria should not be used as the primary basis for comparing models with different fixed-effect terms.

---

## 8. Information criteria and fit statistics

BESH Stat NG reports likelihood-based fit statistics such as log-likelihood, AIC, BIC, objective value, determinant components, and residual quadratic form.

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

!!! note "Comparing models"
    Information criteria are most interpretable when the models are fitted to the same retained observations and are comparable for the analysis question. Use ML rather than REML when comparing different fixed-effect mean models. REML-based fit statistics are useful diagnostics for covariance structures under the same fixed-effect model.

---

## 9. G-side random-effects covariance structures

Let \(q\) be the number of random-effect columns in \(Z_i\), including the random intercept if present. The G-side covariance matrix \(G\) is a \(q \times q\) matrix describing the variances and covariances of the random effects.

The following structures are available in the LMM workflow.

| G-side structure | Parameters | Mathematical description |
|---|---:|---|
| Random Intercept | 1 | A single random-intercept variance. |
| Random Intercept + Slope | 3 | Two random effects with an unstructured \(2\times2\) covariance matrix. |
| Identity | 1 | \(G=\sigma_b^2 I_q\). Common variance, zero covariance. |
| Variance Components (VC/Diag) | \(q\) | \(G=\operatorname{diag}(\sigma_1^2,\ldots,\sigma_q^2)\). Separate variances, zero covariance. |
| Compound Symmetry (CS) | 2 for \(q>1\) | Common variance and common correlation/covariance among all random effects. |
| Heterogeneous Compound Symmetry (CSH) | \(q+1\) for \(q>1\) | Separate variances and one common correlation. |
| Autoregressive (AR1) | 2 for \(q>1\) | Common variance with correlation \(\rho^{\lvert j-k\rvert}\) by random-effect column lag. |
| Heterogeneous Autoregressive (ARH1) | \(q+1\) for \(q>1\) | Separate variances with AR(1)-style correlation. |
| Toeplitz (TOEP) | \(q\) | Common variance with a separate correlation for each random-effect column lag. |
| Heterogeneous Toeplitz (TOEPH) | \(2q-1\) | Separate variances and separate lag correlations. |
| Unstructured Random Effects | \(q(q+1)/2\) | Every variance and covariance is estimated. |

For one random-effect column, all valid G-side structures reduce to a single variance parameter. The special random-intercept and random-intercept-plus-slope entries are convenience structures for common LMMs. More general multiple-random-effect models usually use variance components, a structured covariance, or an unstructured covariance.

### 9.1 Identity G-side covariance

Identity G-side covariance assumes all random-effect columns are independent and have the same variance:

\[
G = \sigma_b^2 I_q.
\]

This is compact but restrictive. It is most useful when the random-effect columns are on comparable scales or when the model is intentionally constrained.

### 9.2 Variance-components G-side covariance

Variance-components covariance, also called diagonal or VC, estimates a separate variance for each random effect while fixing all covariances to zero:

\[
G =
\begin{bmatrix}
\sigma_1^2 & 0 & \cdots & 0\\
0 & \sigma_2^2 & \cdots & 0\\
\vdots & \vdots & \ddots & \vdots\\
0 & 0 & \cdots & \sigma_q^2
\end{bmatrix}.
\]

This is often a stable default for multiple random effects because the number of covariance parameters grows linearly with \(q\).

### 9.3 Compound-symmetry G-side covariance

Compound symmetry assumes a common random-effect variance and a common correlation:

\[
G_{jk} =
\begin{cases}
\sigma_b^2, & j=k,\\
\sigma_b^2\rho, & j\ne k.
\end{cases}
\]

This is compact but assumes that all random effects are on a comparable scale and have the same pairwise relationship.

### 9.4 Heterogeneous compound-symmetry G-side covariance

Heterogeneous compound symmetry allows each random effect to have its own variance while retaining one common correlation:

\[
G_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
\sigma_j\sigma_k\rho, & j\ne k.
\end{cases}
\]

This is less restrictive than CS and less parameter-heavy than unstructured covariance.

### 9.5 AR(1) and heterogeneous AR(1) G-side covariance

AR(1) G-side covariance is meaningful when the random-effect columns have a natural order, such as ordered basis terms. The homogeneous form is

\[
G_{jk}=\sigma_b^2\rho^{|j-k|}.
\]

The heterogeneous form is

\[
G_{jk}=\sigma_j\sigma_k\rho^{|j-k|}.
\]

Here the lag is based on **random-effect column order**, not the visit/time/order variable.

### 9.6 Toeplitz and heterogeneous Toeplitz G-side covariance

Toeplitz G-side covariance allows each random-effect column lag to have its own correlation. The homogeneous form is

\[
G_{jk}=\sigma_b^2\rho_{|j-k|},
\qquad
\rho_0=1.
\]

The heterogeneous form is

\[
G_{jk}=\sigma_j\sigma_k\rho_{|j-k|},
\qquad
\rho_0=1.
\]

Toeplitz is more flexible than AR(1) because the lag-2 correlation, for example, does not have to equal the square of the lag-1 correlation. It is still more structured than a fully unstructured covariance matrix.

### 9.7 Unstructured G-side covariance

Unstructured covariance estimates every random-effect variance and covariance:

\[
G =
\begin{bmatrix}
\sigma_{11} & \sigma_{12} & \cdots & \sigma_{1q}\\
\sigma_{12} & \sigma_{22} & \cdots & \sigma_{2q}\\
\vdots & \vdots & \ddots & \vdots\\
\sigma_{1q} & \sigma_{2q} & \cdots & \sigma_{qq}
\end{bmatrix}.
\]

The parameter count grows as \(q(q+1)/2\). For example, four random-effect columns require ten G-side covariance parameters. This flexibility can be useful, but it requires enough subjects and enough information in the random-effect design.

!!! warning "Column order for G-side AR and Toeplitz structures"
    G-side AR(1), ARH(1), Toeplitz, and heterogeneous Toeplitz structures use the order of the random-effect design columns. They should be used only when that column order has a meaningful interpretation. R-side AR and Toeplitz structures use the visit/time/order variable instead.

---

## 10. R-side residual covariance structures

Let \(T\) denote the number of retained visit/time/order levels used to define a representative residual covariance matrix. A particular subject may have fewer than \(T\) observed rows; the subject-specific \(R_i\) uses the retained levels available for that subject.

The following R-side structures are available.

| R-side structure | Parameters | Mathematical description |
|---|---:|---|
| Identity | 1 | Common residual variance with independent residuals. |
| Diagonal Heterogeneous | \(T\) | Visit/order-specific variances with zero residual covariance. |
| Compound Symmetry | 2 for \(T>1\) | Common variance and common residual correlation/covariance. |
| Heterogeneous CS | \(T+1\) for \(T>1\) | Visit/order-specific variances and one common residual correlation. |
| AR(1) | 2 for \(T>1\) | Common variance with correlation \(\rho^{\lvertt_j-t_k\rvert}\) or visit-order lag correlation. |
| Heterogeneous AR(1) | \(T+1\) for \(T>1\) | Visit/order-specific variances with AR(1)-style correlation. |
| Toeplitz (TOEP) | \(T\) | Common variance with a separate correlation for each visit/order lag. |
| Heterogeneous Toeplitz (TOEPH) | \(2T-1\) | Visit/order-specific variances and separate lag correlations. |
| Unstructured | \(T(T+1)/2\) | Every residual variance and covariance is estimated. |

For \(T=1\), all valid residual structures reduce to one residual variance.

### 10.1 Identity residual covariance

Identity residual covariance is

\[
R_i = \sigma^2 I_{n_i}.
\]

This is the usual starting point for LMMs because random effects already account for some within-subject or within-cluster dependence.

### 10.2 Diagonal heterogeneous residual covariance

Diagonal heterogeneous covariance allows different residual variances by visit or order level while keeping residual covariances at zero:

\[
R_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
0, & j\ne k.
\end{cases}
\]

This is useful when residual variability changes over visits but residual correlations are not modeled after random effects.

### 10.3 Compound-symmetry residual covariance

Compound symmetry assumes a common residual variance and common residual correlation:

\[
R_{jk} =
\begin{cases}
\sigma^2, & j=k,\\
\sigma^2\rho, & j\ne k.
\end{cases}
\]

This can be useful for clustered data where residuals within the same cluster have a shared pairwise correlation.

### 10.4 Heterogeneous compound-symmetry residual covariance

Heterogeneous compound symmetry allows visit/order-specific residual variances with one common residual correlation:

\[
R_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
\sigma_j\sigma_k\rho, & j\ne k.
\end{cases}
\]

### 10.5 AR(1) and heterogeneous AR(1) residual covariance

AR(1) residual covariance assumes residual correlation declines as visit/order lag increases:

\[
R_{jk}=\sigma^2\rho^{d_{jk}},
\]

where \(d_{jk}\) is the visit/order lag between observations \(j\) and \(k\). The heterogeneous form is

\[
R_{jk}=\sigma_j\sigma_k\rho^{d_{jk}}.
\]

These structures require a meaningful visit/time/order variable.

### 10.6 Toeplitz and heterogeneous Toeplitz residual covariance

Toeplitz residual covariance uses a separate lag correlation for each visit/order lag:

\[
R_{jk}=\sigma^2\rho_{d_{jk}},
\qquad
\rho_0=1.
\]

The heterogeneous form is

\[
R_{jk}=\sigma_j\sigma_k\rho_{d_{jk}},
\qquad
\rho_0=1.
\]

Toeplitz can be useful when correlations are mainly a function of lag but do not follow the strict geometric decay imposed by AR(1).

### 10.7 Unstructured residual covariance

Unstructured residual covariance estimates each visit/order variance and covariance separately:

\[
R =
\begin{bmatrix}
\sigma_{11} & \sigma_{12} & \cdots & \sigma_{1T}\\
\sigma_{12} & \sigma_{22} & \cdots & \sigma_{2T}\\
\vdots & \vdots & \ddots & \vdots\\
\sigma_{1T} & \sigma_{2T} & \cdots & \sigma_{TT}
\end{bmatrix}.
\]

This is the most flexible residual covariance structure, but the parameter count increases quickly with the number of visits. It is usually more common in MMRM than in random-effects LMMs.

---

## 11. Covariance parameterization and validity

The tables above describe covariance structures on the **user scale**: variances, covariances, and correlations. For numerical fitting, covariance parameters are optimized on transformed scales so that fitted variances remain positive and fitted correlation matrices remain valid where possible.

Typical transformations include:

| Quantity | Practical constraint | Fitting-scale idea |
|---|---|---|
| Variance | Must be positive | Log-variance scale. |
| Correlation | Must stay inside the valid range | Unconstrained transformed correlation. |
| Compound-symmetry correlation | Must satisfy the positive-definite lower bound | Bounded transformation using the matrix dimension. |
| Toeplitz lag correlations | Must imply a valid correlation matrix | Partial-autocorrelation-style representation. |
| Unstructured covariance | Must remain positive definite | Cholesky-style representation. |

The exact fitting scale is not usually needed for interpretation. Output covariance and correlation matrices are reported on the user scale.

---

## 12. Fixed-effect inference

Fixed-effect inference is based on linear functions of \(\beta\). A single coefficient or contrast can be written as

\[
L\beta,
\]

where \(L\) is a row vector. The estimate is

\[
L\hat\beta,
\]

with estimated variance

\[
\widehat{\operatorname{Var}}(L\hat\beta)=L\widehat{\Phi}L^\top.
\]

A single-degree-of-freedom Wald statistic is

\[
t = \frac{L\hat\beta-h}{\sqrt{L\widehat{\Phi}L^\top}},
\]

where \(h\) is the null value, usually 0.

A multi-degree-of-freedom fixed-effect test uses a matrix \(L\) with \(r\) estimable rows:

\[
F = \frac{1}{r}
(L\hat\beta-h)^\top
\{L\widehat{\Phi}L^\top\}^{-1}
(L\hat\beta-h).
\]

This is the basis for Type III fixed-effect tests.

### Denominator degrees of freedom

BESH Stat NG supports several fixed-effect inference modes:

| Inference mode | Interpretation |
|---|---|
| Large-sample / asymptotic | Uses normal or chi-square-style large-sample reference behavior. |
| Residual degrees of freedom | Uses a residual denominator degrees of freedom approximation. |
| Satterthwaite | Approximates denominator degrees of freedom using uncertainty in the covariance estimates. |
| Kenward-Roger | Adjusts the fixed-effect covariance matrix and denominator degrees of freedom for small-sample mixed-model inference. |

Satterthwaite and Kenward-Roger methods are approximate. They can differ from other software because of covariance parameterization, derivative calculations, convergence tolerances, rank decisions, and details of the denominator-DF approximation.

!!! note "Type III tests and coding choices"
    Type III tests are tests of fixed-effect terms after accounting for the other terms in the model. Their interpretation depends on the design matrix, categorical coding, included interactions, and estimability. Keep the coding and reference-level definitions with the analysis output.

---

## 13. Random-effect estimates and fitted values

After the fixed effects and covariance parameters are estimated, subject/cluster-specific random effects can be estimated by empirical best linear unbiased prediction, often called BLUPs. For subject \(i\), the random-effect estimate is

\[
\hat b_i = \hat G Z_i^\top \hat V_i^{-1}(y_i - X_i\hat\beta).
\]

These estimates are **conditional model-based predictions**, not directly observed subject effects. They are shrunk toward zero, with stronger shrinkage when the subject has little information or when the fitted random-effect variance is small.

BESH Stat NG reports marginal fitted values and raw marginal residuals as

\[
\hat y_{ij}^{\,marginal} = x_{ij}^\top\hat\beta,
\qquad
\hat e_{ij}^{\,marginal}=y_{ij}-x_{ij}^\top\hat\beta.
\]

Random-effect output, when requested, reports the subject/cluster-specific random-effect estimates. These are useful for diagnostics and interpretation of cluster deviations, but they should not be treated as independent observed data.

---

## 14. Identifiability, rank, and estimability

An LMM can be mathematically valid but poorly supported by a particular dataset. Practical issues include:

- too few subjects or clusters for a rich random-effects covariance structure;
- random slopes with little or no within-subject variation;
- random-effect columns that are nearly collinear;
- fixed-effect columns that are aliased by design or sparse categorical levels;
- covariance structures with more parameters than the data can estimate reliably;
- boundary estimates, such as random-effect variances close to zero;
- non-positive-definite or nearly singular fitted covariance matrices.

A more complex covariance structure does not automatically produce a better scientific model. It may instead produce unstable estimates, convergence warnings, or results that are sensitive to small data changes.

!!! tip "Model-building strategy"
    Start with the random effects required by the design and question. Use a simple G-side structure such as random intercept, random intercept plus slope, or variance components. Add covariance complexity only when the data support it and the additional parameters have a clear interpretation.

---

## 15. Assumptions

The main LMM assumptions are:

1. **Continuous Gaussian response model.** The response is modeled as conditionally normal given the random effects and covariates.
2. **Correct mean structure.** Important fixed effects and interactions are included as needed for the analysis objective.
3. **Appropriate random-effect structure.** The random effects represent scientifically meaningful subject/cluster deviations.
4. **Appropriate covariance structure.** The chosen G-side and R-side covariance structures are adequate but not unnecessarily over-parameterized.
5. **Independent grouping units.** Different subjects, clusters, or grouping units are assumed independent.
6. **Missingness handled by the likelihood.** Rows with missing required analysis values are excluded. Likelihood-based inference is generally interpreted under assumptions such as missing at random conditional on the modeled data.
7. **No extreme undue influence.** A small number of subjects or rows should not dominate the fitted fixed effects or covariance estimates.

Diagnostic output, residual plots, random-effect summaries, and convergence messages should be reviewed before reporting the model.

---

## 16. Relationship to MMRM mathematics

The MMRM model documented in [MMRM model and mathematics](../mmrm/model-and-mathematics.md) is a marginal repeated-measures model with no user-specified random effects in the public MMRM workflow. In the general mixed-model covariance expression,

\[
V_i = Z_iGZ_i^\top + R_i,
\]

MMRM uses

\[
V_i = R_i.
\]

LMM uses random effects, so \(Z_iGZ_i^\top\) is active. Optional R-side residual covariance may also be used. This gives LMM a subject- or cluster-specific interpretation through random effects, while MMRM focuses on the marginal repeated-measures covariance and planned marginal estimates.

## 17. References and further reading

---

The following books and articles are useful background references for the LMM methods described in this documentation. They are not required for routine use of BESH Stat NG, but they provide the statistical context for random-effect modeling, longitudinal mixed models, covariance structures, and small-sample inference.

- Laird NM, Ware JH. *Random-effects models for longitudinal data.* Biometrics. 1982;38(4):963–974.  
  Foundational article introducing random-effect models for longitudinal continuous responses, including subject-specific random intercepts and slopes.
- Verbeke G, Molenberghs G. *Linear Mixed Models for Longitudinal Data.* Springer; 2000.  
  Comprehensive longitudinal-data reference with detailed discussion of mean structures, covariance modeling, subject profiles, missing data, and diagnostics.
- Pinheiro JC, Bates DM. *Mixed-Effects Models in S and S-PLUS.* Springer; 2000.  
  Practical and computational reference for linear and nonlinear mixed-effects models, including model-building strategy and grouped-data examples.
- Fitzmaurice GM, Laird NM, Ware JH. *Applied Longitudinal Analysis.* 2nd ed. Wiley; 2011.  
  Applied longitudinal-analysis text covering clustered and repeated-measures data, linear mixed models, marginal models, and generalized extensions.
- Kenward MG, Roger JH. *Small sample inference for fixed effects from restricted maximum likelihood.* Biometrics. 1997;53(3):983–997.  
  Key article behind Kenward-Roger-style small-sample fixed-effect inference for mixed models fitted by restricted maximum likelihood.

---

## See also

- [LMM overview](../linear-mixed-models-lmm.md)
- [Concepts and use cases](concepts-and-use-cases.md)
- [Random effects and covariance structures](random-effects-and-covariance.md)
- [Options and output reference](options-and-output.md)
- [Worksheet functions](worksheet-functions.md)
- [Implementation details](implementation-details.md)
- [MMRM model and mathematics](../mmrm/model-and-mathematics.md)
