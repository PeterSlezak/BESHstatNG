# Implementation notes

This page gives a high-level overview of *how BESHStatNG is implemented*, so you can better understand performance, numerical stability, and what is (and isn’t) inside the `.xll`.

## Design goals

- **Self-contained and lightweight.** Core statistical and numerical routines are implemented in-house (no heavy external numeric libraries), keeping the add-in small and easy to deploy.
- **Excel-native workflow.** Inputs come directly from worksheets and outputs are written back as tables and charts.
- **Deterministic results.** For the same inputs you should get the same outputs (no hidden randomness unless a method explicitly needs it).

## “No external libraries” — what that means here

BESHStatNG does **not** depend on large third-party numerical toolkits.  
It *does* use Excel-DNA for the add-in hosting, but the statistics, matrix algebra, and application logging are implemented within the project.

## Numerical building blocks used across the add-in

### Matrix algebra and linear algebra

Many methods (regression, PCA, survival models) rely on shared linear-algebra utilities, including:

- **Matrix inversion / solving systems**
  - Uses stable methods such as **Cholesky** when the matrix is symmetric positive definite.
  - Falls back to more general approaches when needed.

- **SVD (singular value decomposition)**
  - Used to handle **rank-deficient** or ill-conditioned problems and to compute **pseudo-inverses** where appropriate.
  - Also supports PCA and several regression routines.

### Iterative model fitting

For models that don’t have closed-form solutions, the add-in uses standard iterative optimizers:

- **IRLS / Newton-style iterations** for GLM-type models (with user-configurable max iterations and tolerance `ε`).
- **EM-style iterations** for Zero-Inflated Poisson (ZIP), where a Poisson count process is combined with a logistic “excess zero” process.
- **Partial likelihood iterations** for Cox proportional hazards regression, with multiple tie-handling strategies.

### Exact and small-sample calculations (when available)

Where it makes sense, BESHStatNG includes exact or small-sample methods, for example:

- Exact p-values for **Fisher’s exact test** (2×2 and also RxC)
- Exact p-values for some **rank-based tests** (when sample sizes allow)

## Output formatting pipeline

Internally, most analyses:

1. Import and validate the selected worksheet ranges.
2. Compute the test/model results.
3. Convert outputs into a set of **ResultTable** objects.
4. Write tables (and optional charts) back to Excel in a consistent layout.

## Performance notes

- **Large ranges:** iterative models (Cox/GLM/GEE) will naturally take longer on large datasets.
- **Stability vs speed:** decomposition-based approaches (like SVD) are used when stability matters, even if they’re slower than a naïve inverse.


## About Excel worksheet functions (and why BESHStatNG avoids them)

BESHStatNG intentionally does **not** rely on Excel’s built-in worksheet functions (e.g. `NORM.*`, `T.*`, `CHISQ.*`, `F.*`, `BETA.*`) for statistical computation. Instead it evaluates distributions using VB.NET + internal numerical routines designed for **tail stability** and reproducibility. Excel’s internal implementations are not publicly documented in detail, so the project explicitly follows **published, peer-reviewed algorithms** that are also widely used in statistical software (including R and SciPy).

> **Note on degrees of freedom in Excel:** several Excel distribution functions document that **non-integer** degrees of freedom are **truncated to an integer** (see Microsoft docs for [T.DIST](https://support.microsoft.com/en-us/office/t-dist-function-4329459f-ae91-48c2-bba8-1ead1c6c21b2), [T.INV.2T](https://support.microsoft.com/en-us/office/t-inv-2t-function-ce72ea19-ec6c-4be7-bed2-b9baf2264f17), [F.DIST](https://support.microsoft.com/en-us/office/f-dist-function-a887efdc-7c8e-46cb-a74a-f884cd29b25d), and [CHISQ.DIST](https://support.microsoft.com/en-us/office/chisq-dist-function-8486b05e-5c05-4942-a9ea-f6b341518732)).  
> BESHStatNG consistently supports **non-integer df** where the underlying mathematics allows it (e.g. Welch/Satterthwaite-style approximations).

### Standard normal quantile (inverse CDF): AS241 + Newton refinement
- **Code:** `src/BaseStat/Distributions.vb` → `NormSInv(p)`
- **Method:** Wichura’s **Algorithm AS 241** rational approximation for the initial `z`, followed by a **Newton refinement step** using `PNorm` + `DNorm`. This “high accuracy + refinement” pattern improves stability in the tails.
- **Reference (paper):**
  - Wichura (1988), *Algorithm AS 241: The Percentage Points of the Normal Distribution*  
    PDF: https://csg.sph.umich.edu/abecasis/gas_power_calculator/algorithm-as-241-the-percentage-points-of-the-normal-distribution.pdf  
    Journal page: https://academic.oup.com/jrsssc/article/37/3/477/6985522
- **Comparable functions:**
  - **R:** `dnorm/pnorm/qnorm` → https://stat.ethz.ch/R-manual/R-devel/library/stats/html/Normal.html
  - **Python (SciPy):** `scipy.stats.norm` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.norm.html

### Studentized range (Tukey Q): AS190 (CDF + inverse)
- **Why it matters:** Excel provides Tukey-style procedures only indirectly; it does **not** provide a native worksheet distribution for the **Studentized range (Q)** (needed for Tukey Q critical values). BESHStatNG includes it explicitly.
- **Code:** `src/BaseStat/StatFunc.vb`
  - `PRTRNG(q, V, r, iFault)` implements **AS190** (CDF / probability)
  - `QTRNG(p, V, r, iFault)` and `QTRNG0(...)` implement **AS190.1 / AS190.2** (quantiles / initial approximation)
- **Reference (paper):**
  - Lund & Lund (1983), *Algorithm AS 190: Probabilities and Upper Quantiles for the Studentized Range*  
    https://rss.onlinelibrary.wiley.com/doi/10.2307/2347300  (alt: https://www.jstor.org/stable/10.2307/2347300)
- **Comparable functions:**
  - **Python (SciPy):** `scipy.stats.studentized_range` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.studentized_range.html  
    Tutorial page: https://docs.scipy.org/doc/scipy/tutorial/stats/continuous_studentized_range.html

### Incomplete beta + inverse incomplete beta: continued fraction + AS109-style start + Newton
These are the numerical “engines” behind **t**, **F**, and **beta-family** distributions.
- **Code:** `src/BaseStat/Distributions.vb`
  - `RegularizedIncompleteBeta(x, a, b)` uses **log-gamma prefactor** + **continued fraction** (Lentz-style stable evaluation)
  - `InverseRegularizedIncompleteBeta(p, a, b)` uses an **AS109-style initial approximation** followed by **Newton refinement**, plus symmetry handling for stable convergence.
- **Reference (AS109 implementation notes):**
  - https://people.sc.fsu.edu/~jburkardt/c_src/asa109/asa109.html
- **Comparable functions:**
  - **R (Beta):** `dbeta/pbeta/qbeta` → https://stat.ethz.ch/R-manual/R-devel/library/stats/html/Beta.html
  - **Python (SciPy):** `scipy.stats.beta` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.beta.html

### Incomplete gamma (regularized) for chi-square CDF: series/continued fraction split (ASA239-style)
- **Code:** `src/BaseStat/StatFunc.vb` → `LowerIncompleteGamma(a, x)`
- **Method:** switches between **series expansion** and **continued fraction** depending on the region (`x < a+1` vs. `x ≥ a+1`) and uses log-space terms to avoid overflow/underflow.
- **Reference (ASA239 implementation notes):**
  - https://people.sc.fsu.edu/~jburkardt/c_src/asa239/asa239.html
- **Comparable functions:**
  - **R (Chi-square):** `dchisq/pchisq/qchisq` → https://stat.ethz.ch/R-manual/R-devel/library/stats/help/pchisq.html
  - **Python (SciPy):** `scipy.stats.chi2` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.chi2.html

### Student-t quantile (inverse CDF): Hill (1970) approximation + Newton
- **Code:** `src/BaseStat/Distributions.vb` → `T_Inv(p, df)`
- **Method:** Hill-style initial approximation (polynomial correction terms) + **Newton refinement** via `T_CDF` and `T_PDF`.
- **Reference (paper):**
  - Hill (1970), *Algorithm 396: Student’s t-Quantiles* (Communications of the ACM)  
    https://dl.acm.org/doi/10.1145/355598.355600
- **Comparable functions:**
  - **R (t):** `dt/pt/qt` → https://stat.ethz.ch/R-manual/R-devel/library/stats/html/TDist.html
  - **Python (SciPy):** `scipy.stats.t` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.t.html

### F distribution (built on incomplete beta)
- **Comparable functions:**
  - **R (F):** `df/pf/qf` → https://stat.ethz.ch/R-manual/R-devel/library/stats/html/Fdist.html
  - **Python (SciPy):** `scipy.stats.f` → https://docs.scipy.org/doc/scipy/reference/generated/scipy.stats.f.html

