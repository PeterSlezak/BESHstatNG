# GLM/GEE families, links, and working correlation structures

This page documents the **distribution families**, **link functions**, and **working correlation structures** used by BESH Stat NG in **Generalized Linear Models (GLM)** and **Generalized Estimating Equations (GEE)**.

---

## Notation

Let \(y_{ij}\) be the response for cluster/subject \(i=1,\dots,m\) at observation \(j=1,\dots,n_i\). Let \(\mathbf{x}_{ij}\in\mathbb{R}^p\) be the covariate row (including the intercept if present), and let \(\bf \beta\in\mathbb{R}^p\) be the regression parameters.

A **link function** \(g\) connects the mean to a linear predictor:

$$
\eta_{ij} = \mathbf{x}_{ij}^\top\bf \beta + o_{ij},\qquad \mu_{ij}=\mathbb{E}[y_{ij}]=g^{-1}(\eta_{ij}).
$$

- \(o_{ij}\) is an **offset** (typically a known additive term in the linear predictor, e.g., log-exposure).
- A **variance function** \(V(\mu)\) defines the mean–variance relationship.

For a single observation, the marginal variance is

$$
\operatorname{Var}(y_{ij}) = \phi\,V(\mu_{ij}),
$$

where \(\phi\) is a (possibly estimated) **scale/dispersion** parameter.

For GEE, the within-cluster covariance is modeled as

$$
\operatorname{Var}(\mathbf{y}_i)=\mathbf{V}_i = \phi\,\mathbf{A}_i^{1/2}\,\mathbf{R}(\bf \alpha)\,\mathbf{A}_i^{1/2},
\quad \mathbf{A}_i=\operatorname{diag}\{V(\mu_{i1}),\dots,V(\mu_{in_i})\},
$$

where \(\mathbf{R}(\bf \alpha)\) is a **working correlation matrix** parameterized by \(\bf \alpha\).

A convenient standardized residual used throughout the implementation is

$$
r_{ij}=\frac{y_{ij}-\mu_{ij}}{\sqrt{V(\mu_{ij})}}.
$$

---

## Distribution families

BESH Stat NG implements the following families:

- **Gaussian**
- **Binomial** (Bernoulli / proportions in \([0,1]\))
- **Poisson**
- **Negative Binomial (NB2)** with dispersion \(\alpha>0\)
- **Gamma**

Each family provides:

- variance function \(V(\mu)\)
- a **deviance contribution** \(D_i\) (used for deviance residuals)
- a **quasi-likelihood contribution** \(Q_i\) (used for GEE quasi-likelihood and QIC)
- allowed links (enforced at run time)

### Gaussian

- **Domain:** \(y\in\mathbb{R}\)
- **Variance:** \(V(\mu)=1\)
- **Deviance contribution:**

$$
D(y,\mu)=(y-\mu)^2
$$

- **Quasi-likelihood contribution:**

$$
Q(y,\mu)=-\tfrac{1}{2}(y-\mu)^2
$$

- **Allowed links:** Identity, Log, Inverse, Power

### Poisson

- **Domain:** \(y\ge 0\) (counts)
- **Variance:** \(V(\mu)=\mu\)
- **Deviance contribution (with \(0\log 0\equiv 0\) convention):**

$$
D(y,\mu)=2\bigl[y\log(y/\mu)-(y-\mu)\bigr]
$$

- **Quasi-likelihood contribution:**

$$
Q(y,\mu)=y\log(\mu)-\mu
$$

- **Allowed links:** Log, Identity, Sqrt, Power

### Binomial (Bernoulli)

- **Domain:** \(0\le y\le 1\) (typically 0/1)
- **Variance:** \(V(\mu)=\mu(1-\mu)\)
- **Deviance contribution:**

$$
D(y,\mu)=-2\bigl[y\log(\mu)+(1-y)\log(1-\mu)\bigr]
$$

(The implementation clips \(\mu\) slightly away from 0 and 1 to avoid \(\log 0\).)

- **Quasi-likelihood contribution:**

$$
Q(y,\mu)=y\log\Bigl(\frac{\mu}{1-\mu}\Bigr)+\log(1-\mu)
$$

- **Allowed links:** Logit, Probit, Log, Identity

### Gamma

- **Domain:** \(y>0\)
- **Variance:** \(V(\mu)=\mu^2\)
- **Deviance contribution:**

$$
D(y,\mu)=2\Bigl[-\log(y/\mu)+\frac{y-\mu}{\mu}\Bigr]
$$

- **Quasi-likelihood contribution:**

$$
Q(y,\mu)= -\Bigl(\frac{y}{\mu}+\log\mu\Bigr)
$$

- **Allowed links:** Log, Identity, Inverse, Sqrt, Power

### Negative Binomial (NB2)

This is the NB2 parameterization with dispersion \(\alpha>0\).

- **Domain:** \(y\ge 0\) (counts)
- **Variance:**

$$
V(\mu)=\mu+\alpha\mu^2
$$

- **Deviance contribution (NB2):**

$$
D(y,\mu)=2\left[y\log\left(\frac{y}{\mu}\right) - \Bigl(y+\frac{1}{\alpha}\Bigr)\log\left(\frac{1+\alpha y}{1+\alpha\mu}\right)\right]
$$

(with the natural limit \(y\log(y/\mu)\to 0\) when \(y\to 0\)).

- **Quasi-likelihood contribution (NB2; up to an additive constant):**

$$
Q(y,\mu)=\log\Gamma\Bigl(y+\frac{1}{\alpha}\Bigr)-\log\Gamma\Bigl(\frac{1}{\alpha}\Bigr)
+y\log\left(\frac{\alpha\mu}{1+\alpha\mu}\right)+\frac{1}{\alpha}\log\left(\frac{1}{1+\alpha\mu}\right)
$$

- **Allowed links:** Log, Identity, Power

---

## Link functions

A link function \(g\) maps \(\mu\mapsto\eta\). GEE/GLM fitting requires the inverse \(g^{-1}\) and its derivative \(\frac{d\mu}{d\eta}\) (because \(\frac{\partial\mu}{\partial\bf \beta}=\frac{d\mu}{d\eta}\mathbf{x}\)).

### Identity

$$
\eta=\mu,\qquad \mu=\eta,\qquad \frac{d\mu}{d\eta}=1.
$$

### Log

$$
\eta=\log\mu,\qquad \mu=\exp\eta,\qquad \frac{d\mu}{d\eta}=\exp\eta=\mu.
$$

### Logit

$$
\eta=\log\left(\frac{\mu}{1-\mu}\right),\qquad
\mu=\frac{1}{1+e^{-\eta}},\qquad
\frac{d\mu}{d\eta}=\mu(1-\mu).
$$

### Probit

Let \(\Phi\) be the standard normal CDF and \(\varphi\) the standard normal PDF.

$$
\eta=\Phi^{-1}(\mu),\qquad \mu=\Phi(\eta),\qquad \frac{d\mu}{d\eta}=\varphi(\eta).
$$

### Inverse

$$
\eta=\mu^{-1},\qquad \mu=\eta^{-1},\qquad \frac{d\mu}{d\eta}=-\eta^{-2}.
$$

### Square root

$$
\eta=\sqrt\mu,\qquad \mu=\eta^2,\qquad \frac{d\mu}{d\eta}=2\eta.
$$

### Power \(p\)

BESH Stat NG defines the power link as \(g(\mu)=\mu^p\) (not the Box–Cox variant).

$$
\eta=\mu^p,\qquad \mu=\eta^{1/p},\qquad \frac{d\mu}{d\eta}=\frac{1}{p}\,\eta^{(1-p)/p}.
$$

Notes:

- \(p=1\) is the identity link.
- \(p=1/2\) is the square-root link.
- \(p=-1\) is the inverse link.
- \(p=0\) is **not** supported in this implementation.

---

## Working correlation structures (GEE)

GEE models the marginal mean correctly if the mean is specified correctly; the working correlation \(\mathbf{R}(\bf \alpha)\) is used to improve efficiency and to define the model-based covariance.

BESH Stat NG supports:

- **Independence**
- **Exchangeable**
- **Autoregressive (AR(1))**
- **Toeplitz**
- **Unstructured**

### Independence

$$
\mathbf{R}=\mathbf{I}.
$$

No association parameters are estimated.

### Exchangeable

All off-diagonal correlations are equal:

$$
R_{jk}=\begin{cases}
1, & j=k\\
\rho, & j\ne k
\end{cases}
$$

**Method-of-moments estimate (as implemented).** Let \(r_{ij}\) be the standardized residuals.

1. Estimate dispersion \(\phi\) from Pearson residuals (see “\(n-p\) correction” below).
2. Compute the average within-cluster cross-product and divide by \(\phi\):

$$
\hat\rho
=\frac{1}{\phi}\,\frac{\sum_i\sum_{j<k} r_{ij}r_{ik}}{\sum_i \binom{n_i}{2}}.
$$

With the \(n-p\) correction enabled, the denominator is additionally adjusted by \(\frac{n_{\text{pairs}}-p}{n_{\text{pairs}}}\), where \(n_{\text{pairs}}=\sum_i\binom{n_i}{2}\).

**Solving \(\mathbf{V}_i^{-1}\mathbf{z}\).** The implementation uses a closed-form inverse for the exchangeable correlation matrix (a Sherman–Morrison style update), avoiding explicit matrix inversion.

### Autoregressive (AR(1))

Correlation decays geometrically with lag:

$$
R_{jk}=\rho^{\lvert j-k\rvert}.
$$

**Association estimate (as implemented).** Let the within-cluster observations be ordered by the provided ordering variable (equal-spacing assumption). Define standardized residuals for the association update as

$$
r_{ij}=\frac{y_{ij}-\mu_{ij}}{\sqrt{\phi\,V(\mu_{ij})}}.
$$

BESH Stat NG computes a lag-1 moment using *adjacent* pairs in this order (i.e., \((j-1,j)\) within each cluster):

$$
\bar{c}_1=\frac{1}{N_1}\sum_i\sum_{j=2}^{n_i} r_{i,j}\,r_{i,j-1},\qquad
\bar{c}_0=\frac{1}{N_0}\sum_i\sum_{j=1}^{n_i} r_{i,j}^2,
\qquad
\hat\rho=\frac{\bar{c}_1}{\bar{c}_0}.
$$

Here \(N_1=\sum_i(n_i-1)\) counts adjacent pairs and \(N_0=\sum_i n_i\) counts observations.

With the \(n-p\) correction enabled, the effective denominators are adjusted by subtracting \(p\):

$$
\bar{c}_1=\frac{1}{N_1-p}\sum_i\sum_{j=2}^{n_i} r_{i,j}\,r_{i,j-1},\qquad
\bar{c}_0=\frac{1}{N_0-p}\sum_i\sum_{j=1}^{n_i} r_{i,j}^2,
\qquad
\hat\rho=\frac{\bar{c}_1}{\bar{c}_0}.
$$

**Time spacing.** The update uses adjacency in the ordered index (not \(\rho^{\Delta t}\)). Missingness is treated as “right-censored” in the sense that adjacency is defined by the remaining ordered rows.

### Toeplitz

Toeplitz working correlation is stationary by lag but does not require geometric decay. Each position lag has its own correlation parameter:

$$
R_{jk}=\begin{cases}
1, & j=k\\
\rho_{\lvert j-k\rvert}, & j\ne k.
\end{cases}
$$

If there are \(q\) distinct ordered positions, the structure estimates \(q-1\) lag-correlation parameters. This is more flexible than AR(1), but usually less parameter-intensive than an unstructured working correlation.

**Association estimates.** For each time-point pair, BESH Stat NG first accumulates Pearson residual cross-products by the ordered time indices and then averages those pairwise moments by lag. The same lag value is used for all pairs separated by the same ordered-position distance. Conceptually, the fitted lag \(\rho_\ell\) is a stationary average of the unstructured pairwise correlations for pairs with \(\lvert j-k\rvert=\ell\).

For count and binary families such as Poisson, Binomial, and NB2, the Toeplitz association update uses a Pearson-type dispersion estimate for the residual cross-products even when the mean-model scale convention fixes \(\phi=1\). This prevents overdispersed data from artificially driving all Toeplitz lag correlations to the boundary.

**Positive definiteness.** As with the unstructured working correlation, a method-of-moments Toeplitz estimate can be non-positive definite. The implementation applies a nearest positive-definite adjustment when necessary before solving the estimating equations.

### Unstructured

All pairwise correlations are estimated freely.

Let \(q\) be the number of distinct time points in the dataset (from the within-cluster ordering variable). The working correlation matrix is \(q\times q\), with

$$
R_{jj}=1,\qquad R_{jk}=\alpha_{jk}\quad (j\ne k).
$$

**Association estimates.** For each pair \((j,k)\), BESH Stat NG averages standardized residual cross-products across clusters that contain both time points:

$$
\hat\alpha_{jk}=
\frac{1}{\phi}\,\frac{\sum_{i\in\mathcal{I}_{jk}} r_{ij}r_{ik}}{|\mathcal{I}_{jk}|},
$$

where \(\mathcal{I}_{jk}\) is the set of subjects with measurements at both times \(j\) and \(k\). With \(n-p\) correction enabled, the effective denominator uses a conditional adjustment:

$$
d_{jk} =
\begin{cases}
(n_{jk}-p)\phi, & n_{jk} \ge p,\\
n_{jk}\phi, & n_{jk} < p.
\end{cases}
$$

$$
\hat{\alpha}_{jk}=\frac{\sum_{i \in I_{jk}} r_{ij}r_{ik}}{d_{jk}}.
$$

**Positive definiteness.** Because method-of-moments estimates can produce a non–positive definite matrix, the implementation applies a “nearest positive definite” adjustment when needed before using Cholesky factorization.

**Cluster submatrices.** If a subject is missing some time points, its \(\mathbf{R}_i\) is taken as the submatrix corresponding to observed times.

---

## The \(n-p\) correction

The checkbox **Use the n‑p correction for dispersion and correlation estimates** applies small-sample style corrections to the moment estimators.

### Dispersion (scale) estimate

Let \(r_{ij}\) be the Pearson-type standardized residuals defined above, and let \(n=\sum_i n_i\) be the total number of observations and \(p\) the number of regression parameters.

- **Without correction:**

$$
\hat\phi = \frac{\sum_{i,j} r_{ij}^2}{n}
$$

- **With \(n-p\) correction:**

$$
\hat\phi = \frac{\sum_{i,j} r_{ij}^2}{n-p}
$$

### Correlation parameter corrections

For the exchangeable/AR(1)/Toeplitz/unstructured association updates, the implementation also adjusts the effective denominator to account for \(p\), for example multiplying by \(\frac{n_{\text{pairs}}-p}{n_{\text{pairs}}}\) for estimates based on \(n_{\text{pairs}}\) pairs.

## References

- Nelder, J. A., & Wedderburn, R. W. M. (1972). *Generalized Linear Models*. Journal of the Royal Statistical Society: Series A (General), 135(3), 370–384.
- McCullagh, P., & Nelder, J. A. (1989). *Generalized Linear Models* (2nd ed.). Chapman & Hall/CRC.
- Liang, K.-Y., & Zeger, S. L. (1986). Longitudinal data analysis using generalized linear models. *Biometrika*, 73(1), 13–22. https://doi.org/10.1093/biomet/73.1.13
- Zeger, S. L., Liang, K.-Y., & Albert, P. S. (1988). Models for longitudinal data: A generalized estimating equation approach. *Biometrics*, 44(4), 1049–1060.
- Diggle, P. J., Heagerty, P. J., Liang, K.-Y., & Zeger, S. L. (2002). *Analysis of Longitudinal Data* (2nd ed.). Oxford University Press.
- Hardin, J. W., & Hilbe, J. M. (2012/2013). *Generalized Estimating Equations* (2nd ed.). Chapman & Hall/CRC.
- Pan, W. (2001). Akaike’s information criterion in generalized estimating equations. *Biometrics*, 57(1), 120–125. https://doi.org/10.1111/j.0006-341X.2001.00120.x
- Mancl, L. A., & DeRouen, T. A. (2001). A covariance estimator for GEE with improved small-sample properties. *Biometrics*, 57(1), 126–134. https://doi.org/10.1111/j.0006-341X.2001.00126.x

