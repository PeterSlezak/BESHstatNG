# LMM implementation details

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** explain implementation choices that affect reproducibility, convergence behavior, diagnostics, validation, and performance for Linear Mixed Models (LMM). This page is intended for advanced users, validation reviewers, and method maintainers. It is not a programming reference and does not rely on source-code class names.

For the statistical model and formulas, see [Model and mathematics](model-and-mathematics.md). For the random-effect and covariance options, see [Random effects and covariance structures](random-effects-and-covariance.md). For dialog settings and result tables, see [Options and output reference](options-and-output.md). For worksheet functions, see [Worksheet functions](worksheet-functions.md).

---

## 1. Implementation philosophy

The LMM implementation follows five design principles.

| Design principle | Practical consequence |
|---|---|
| **Keep the model specification explicit** | Fixed effects, random effects, G-side covariance, and R-side residual covariance are selected separately so users can see which part of the model explains each source of variation. |
| **Reuse the same fitted-model logic across workflows** | The Excel ribbon and worksheet-function paths use the same conceptual model fit. Differences between the two paths should be due to output formatting, not to different estimation algorithms. |
| **Support useful simple models first** | Random-intercept and random-intercept-plus-slope models are easy to specify, while richer multiple-random-effect models remain available through general G-side structures. |
| **Avoid silently over-parameterized defaults** | When several random-effect columns are present, the practical default is a diagonal variance-components structure rather than a fully unstructured G matrix. |
| **Make validation and troubleshooting inspectable** | Model information, covariance output, fit statistics, convergence diagnostics, optional trace output, and worksheet-function extraction tables are designed to support review and reproducibility. |

A Gaussian LMM can be written as:

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i,
\qquad
b_i \sim N(0,G),
\qquad
\varepsilon_i \sim N(0,R_i),
\]

where each subject or cluster contributes its own response vector, fixed-effect design rows, random-effect design rows, and residual covariance block. The marginal covariance used in likelihood evaluation is:

\[
V_i = Z_iGZ_i^\top + R_i.
\]

This is different from the MMRM workflow, where the within-subject covariance is modeled directly through the residual/marginal covariance matrix and no user-specified random effects are active. For the marginal repeated-measures workflow, see [Mixed Models for Repeated Measures (MMRM)](../mixed-models-for-repeated-measures-mmrm.md).

---

## 2. Public surface

The public LMM surface consists of:

- the Excel ribbon command **Linear Mixed Models (LMM)**;
- the LMM dialog described in [Excel ribbon workflow](user-interface.md);
- the worksheet functions named `BESH.REGR.LMM_*`, described in [Worksheet functions](worksheet-functions.md);
- the output tables described in [Options and output reference](options-and-output.md).

The LMM workflow is intended for continuous outcomes with grouped, clustered, or repeated observations. It supports fixed effects, random intercepts, random slopes, multiple random effects, random-effect interactions, G-side covariance structures, optional R-side residual covariance structures, ML and REML fitting, and several fixed-effect inference methods.

!!! note "LMM versus MMRM"
    Use LMM when subject- or cluster-specific random effects are part of the analysis question or are needed to represent clustering. Use MMRM when the goal is a marginal repeated-measures model with visit-specific means and a residual-side repeated-measures covariance structure.

---

## 3. End-to-end workflow

The ribbon and worksheet-function paths follow the same conceptual sequence.

1. **Collect model inputs.**  
   The user specifies the response, subject or cluster identifier, fixed-effect terms, random-effect terms, intercept choices, optional visit/time/order variable, covariance structures, estimation method, inference method, and diagnostic/output options.

2. **Screen and align rows.**  
   Rows are retained only when all required values for the selected model are valid. Screening is performed before the model is fitted so all model matrices and grouping variables refer to the same retained rows.

3. **Build the fixed-effect design matrix.**  
   Continuous terms, categorical factors, polynomials, interactions, and the fixed intercept option are expanded into the fixed-effect design matrix \(X\).

4. **Build the random-effect design matrix.**  
   The random intercept option and selected random-effect terms are expanded into the random-effect design matrix \(Z\). The random-effect column names are retained for G-side covariance output and random-effect estimates.

5. **Create subject or cluster blocks.**  
   Rows are grouped by the subject or cluster identifier. For each block, the implementation stores the retained response values, fixed-effect rows, random-effect rows, optional visit/order indices, and worksheet row positions used for fitted values and residuals.

6. **Construct covariance blocks.**  
   For each covariance-parameter proposal, the selected G-side structure builds \(G\), and the selected R-side structure builds each subject-specific \(R_i\). The marginal block is then \(V_i=Z_iGZ_i^\top+R_i\).

7. **Profile fixed effects while optimizing covariance parameters.**  
   For each covariance proposal, fixed effects are estimated by generalized least squares. The optimizer searches over covariance parameters using the selected ML or REML criterion.

8. **Compute inference and diagnostics.**  
   After convergence, coefficient inference, Type III tests, covariance output, fit statistics, optional random effects, optional residual output, and warnings are assembled.

9. **Write or return results.**  
   The ribbon path writes formatted Excel sheets. The worksheet-function path stores the fitted model behind a handle and extraction functions return specific result tables.

---

## 4. Data preparation and retained analysis rows

LMM data are expected in long format. Each retained row represents one observation for one subject or cluster. The same subject or cluster can appear on multiple rows.

A row may be excluded before fitting when a required value is missing or invalid, for example:

- missing or non-numeric response;
- missing subject or cluster identifier;
- missing or invalid fixed-effect value required by the selected model;
- missing or invalid random-effect value required by the selected model;
- missing or invalid visit/time/order value when the selected R-side covariance structure requires one;
- a row that is outside the selected input ranges or not aligned across selected ranges.

Subject or cluster identifiers may be numeric or text. Text identifiers such as `PT001`, `USUBJID-004`, `Clinic A`, or `Site-12` can be used as grouping values. The identifier is used to group rows; it is not treated as a numeric predictor.

!!! important "Row screening is model-dependent"
    Adding a random slope, an interaction, or a visit-indexed residual covariance structure can change which rows are complete for the selected model. Always review the retained analysis data before comparing models.

The ribbon output writes retained analysis rows to the output workbook. This makes the fitted analysis set visible and helps users explain differences from other software when row screening, factor coding, or missingness handling differ.

---

## 5. Subject blocks and visit/order indexing

The implementation uses a block representation by subject or cluster. For block \(i\), the fitted model uses:

- \(y_i\), the retained response vector;
- \(X_i\), retained fixed-effect design rows;
- \(Z_i\), retained random-effect design rows;
- optional visit/time/order indices for R-side residual covariance;
- worksheet row positions used for fitted values and residuals.

The subject identifier defines the grouping. The visit/time/order variable has a different role: it is used only when the selected R-side residual covariance structure needs an ordered residual pattern, such as diagonal heterogeneous, heterogeneous compound symmetry, AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, or unstructured residual covariance.

For R-side residual structures that use visit/order levels, raw visit values are mapped to a global ordered index. A subject with missing visits contributes only the observed rows and the corresponding residual-covariance submatrix.

!!! warning "Do not confuse grouping and ordering"
    The subject or cluster ID groups observations. The visit/time/order variable orders residual covariance levels. A random slope can use a time variable even when the R-side residual covariance is identity, but visit-indexed R-side structures still require a valid ordering variable.

---

## 6. Fixed-effect design construction

The fixed-effect design matrix \(X\) represents the population-average part of the model. In the ribbon workflow, \(X\) is built from terms selected in the fixed-effect list. In the worksheet-function workflow, users can supply a fixed-effect range directly or request supported formula expansion.

The design builder supports:

- continuous main effects;
- categorical-factor expansion through the supported model-building controls or formula syntax;
- two-way and custom interactions;
- polynomial terms;
- optional fixed intercept;
- retained column labels for coefficient, Type III, and fitted-value output.

The model is fitted to the expanded design columns. Output labels attempt to retain meaningful term and column names so users can map results back to the worksheet selections.

!!! tip "Keep fixed-effect coding reproducible"
    When comparing results to another program, match row screening, factor coding, reference levels, interaction construction, centering, scaling, and the fixed intercept option. Differences in any of these can change coefficient labels and estimates even when the model looks similar on paper.

---

## 7. Random-effect design construction

The random-effect design matrix \(Z\) represents subject- or cluster-specific deviations from the population mean. The LMM workflow supports:

- random intercepts;
- random slopes;
- multiple random-effect columns;
- random-effect interactions;
- optional random intercept removal for random-slope-only designs;
- random-effect column labels used in G covariance/correlation and random-effect output.

The random intercept option adds a constant random-effect column. Additional random-effect terms add columns based on selected variables or formula expansion. If several random-effect columns are present, the G-side covariance structure determines whether their covariances are estimated, constrained, or fixed to zero.

The convenience G-side options **Random Intercept** and **Random Intercept + Slope** are intentionally limited:

| Convenience option | Expected random-effect design |
|---|---|
| Random Intercept | Random intercept only, with no additional random-effect columns. |
| Random Intercept + Slope | Random intercept plus exactly one random slope/effect. |

For multiple random slopes, random-effect interactions, or richer random designs, use a general G-side structure such as **Variance Components (VC/Diag)**, **Heterogeneous Compound Symmetry**, **Toeplitz**, or **Unstructured Random Effects**.

---

## 8. G-side and R-side covariance construction

The LMM covariance model has two parts.

| Component | Matrix | Meaning |
|---|---|---|
| G side | \(G\) | Covariance among random effects, such as random intercepts and random slopes. |
| R side | \(R_i\) | Residual covariance within subject or cluster after fixed and random effects are included. |

The marginal covariance of the retained response vector for subject or cluster \(i\) is:

\[
V_i = Z_iGZ_i^\top + R_i.
\]

### G-side covariance

The G-side structure is defined over the random-effect columns in \(Z\). Available structures include random-intercept, random-intercept-plus-slope, identity, variance-components/diagonal, compound symmetry, heterogeneous compound symmetry, AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, and unstructured random-effects covariance.

G-side AR(1), heterogeneous AR(1), Toeplitz, and heterogeneous Toeplitz use the order of the random-effect columns. They should be used only when that column order has scientific meaning.

### R-side covariance

The R-side structure is defined over residuals within subject or cluster. Available structures include identity, diagonal heterogeneous, compound symmetry, heterogeneous compound symmetry, AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, and unstructured residual covariance.

R-side AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, diagonal heterogeneous, heterogeneous compound symmetry, and unstructured structures require a valid visit/time/order variable because their parameters are tied to ordered residual levels.

!!! warning "Rich G and rich R can compete"
    A random intercept and a compound-symmetry residual structure can both represent broad within-subject similarity. A random time slope and a serial residual structure can both explain longitudinal dependence. If both sides are rich, review convergence, covariance estimates, and sensitivity models carefully.

---

## 9. Covariance parameterization and constraints

Covariance parameters are optimized on internal scales designed to make valid covariance matrices easier to obtain.

| Quantity | Implementation idea | Practical effect |
|---|---|---|
| Variances and standard deviations | Positive quantities are represented on unconstrained scales and transformed back to positive values. | Variance estimates cannot become negative after transformation. |
| Correlations | Correlation-like quantities are transformed from unconstrained optimizer values into valid correlation ranges. | Trial correlations are kept inside admissible bounds where possible. |
| Compound-symmetry correlations | The common correlation is constrained to the positive-definite range for the matrix dimension. | Prevents invalid equicorrelation matrices for more than two random effects or visits. |
| Toeplitz correlations | Lag correlations use a positive-definite correlation parameterization. | Reduces invalid Toeplitz proposals during optimization. |
| Unstructured covariance | A factor-based representation is used. | The resulting covariance matrix is positive definite for finite, valid factor values. |

The output is transformed back to user-facing covariance, variance, standard-deviation, and correlation quantities where possible.

### Handling invalid trial values

Even with constrained parameterizations, a trial optimizer step can produce a numerically unusable state: a matrix may be nearly singular, non-finite, or impossible to invert reliably. Such trial values are not accepted as fitted models. The objective function returns a poor value for the trial step so the optimizer moves away from that region.

This behavior is a numerical safeguard. It is not a statistical penalty such as ridge regression or LASSO.

---

## 10. Likelihood evaluation and profiling

For a fixed covariance-parameter vector \(\theta\), the implementation evaluates the model block by block. The fixed-effect estimate for that covariance proposal is the generalized least-squares estimate:

\[
\hat\beta(\theta)=
\left(\sum_i X_i^\top V_i^{-1}X_i\right)^{-1}
\left(\sum_i X_i^\top V_i^{-1}y_i\right).
\]

The optimizer works with a profiled objective: fixed effects are updated from the GLS equations for each covariance proposal, and the numerical search is over covariance parameters. This reduces the dimension of the nonlinear optimization problem and is standard for Gaussian mixed models.

Both ML and REML are supported.

| Method | Practical role |
|---|---|
| ML | Useful when comparing different fixed-effect mean models fitted to the same retained observations. |
| REML | Common default for variance-component estimation and required for Kenward-Roger inference in BESH Stat NG. |

REML criteria depend on the fixed-effect design, so REML likelihoods should not be used as the primary basis for comparing different fixed-effect mean models.

---

## 11. Optimization strategy

LMM covariance optimization can be easy for simple random-intercept models and difficult for models with many random effects, rich G-side covariance, rich R-side covariance, sparse subjects, or poorly scaled predictors.

The implementation uses a conservative optimization strategy:

1. **Start from stable covariance values.**  
   Starting values are based on simple variance estimates and neutral correlation starts where possible.

2. **Profile fixed effects.**  
   Fixed effects are solved by GLS for each covariance proposal, while the optimizer updates covariance parameters.

3. **Use derivative information where available.**  
   Analytic derivative information is used for supported covariance structures. Numerical derivatives remain available as fallback or diagnostic paths.

4. **Check matrix validity.**  
   Trial covariance matrices are checked for finite values and numerical invertibility before they can contribute to an accepted fit.

5. **Apply step and convergence safeguards.**  
   Objective checks, step controls, maximum-iteration limits, and convergence tolerances help prevent unstable trial steps from being reported as final fits.

6. **Expose diagnostics.**  
   The output reports convergence status, iteration counts, fit statistics, and optional trace details.

The optimizer mode and gradient mode options are primarily intended for validation, troubleshooting, and performance tuning. Most users should start with the default settings.

---

## 12. Derivatives and finite-difference fallbacks

Derivative information is used in optimization and in small-sample inference adjustments. The implementation distinguishes between structures where closed-form derivative information is straightforward and structures where transformed correlation parameters make direct derivatives more complex.

When an analytic derivative path is available and appropriate, it is used for speed and numerical stability. When a covariance structure uses a parameterization that is easier to validate numerically than to express in compact closed form, finite-difference derivatives can be used.

This fallback behavior is especially relevant for richer correlation structures such as heterogeneous autoregressive and Toeplitz-style structures. The goal is to preserve user-facing validity and reproducibility rather than forcing every covariance structure through the same derivative formula.

!!! note "Small numerical differences are expected"
    Models that use finite-difference derivatives, complex covariance parameterizations, or small-sample degrees-of-freedom adjustments can differ slightly across software. Compare retained rows, coding, covariance structures, estimation method, optimizer tolerance, and inference method before interpreting differences.

---

## 13. Fixed-effect inference

After the covariance parameters and fixed effects are estimated, BESH Stat NG computes fixed-effect inference using the selected method.

The LMM workflow supports:

- large-sample Wald-style inference;
- residual degrees of freedom;
- Satterthwaite degrees of freedom;
- Kenward-Roger adjusted inference.

The same fitted model can produce coefficient tables and Type III tests. Type III tests are constructed from term-level hypothesis matrices rather than simply testing each expanded design column in isolation. This matters for categorical factors and interactions, where a model term may correspond to several columns.

### Kenward-Roger logic

Kenward-Roger inference adjusts the covariance matrix of fixed-effect estimates and uses a finite-sample approximation for tests. In BESH Stat NG, Kenward-Roger is tied to REML because the adjustment is derived for REML-based covariance estimation.

If an advanced adjustment cannot be computed reliably for a particular model, the result should include warnings or fall back to a simpler path rather than silently reporting unstable adjusted inference.

---

## 14. Random-effect estimates

The LMM workflow can output subject- or cluster-level random-effect estimates. These are empirical predictions from the fitted model, often called BLUPs or conditional modes. At a high level, for subject or cluster \(i\), the estimate is based on:

\[
\hat b_i = GZ_i^\top V_i^{-1}(y_i-X_i\hat\beta).
\]

Random-effect estimates are useful for diagnostics and for understanding how subjects or clusters deviate from the population-average model. They should not be treated as independently observed data. Their amount of shrinkage depends on the fitted covariance parameters, the number of observations in the subject or cluster, residual variance, and the random-effect design.

!!! tip "Random-effect output can be large"
    A model with many subjects and several random-effect columns can produce a large random-effect table. Turn this output on when it is needed for interpretation or diagnostics.

---

## 15. Output architecture

The ribbon workflow writes results to Excel sheets rather than hiding them in a modal dialog.

| Output area | Implementation purpose |
|---|---|
| **Data sheet** | Preserves retained analysis rows, selected variables, fitted values, and optional residuals. |
| **LMM sheet** | Contains model information, fixed effects, Type III tests, covariance parameters, optional covariance matrices, optional random-effect estimates, fit statistics, convergence details, and warnings. |
| **LMM Trace sheet** | Optional iteration and diagnostic output for validation and troubleshooting. |
| **Worksheet-function handle** | Stores a fitted model so extraction functions can return specific tables without refitting. |

The handle-based worksheet-function design is important for performance. An LMM can be expensive to fit, especially with rich covariance structures or Kenward-Roger inference. Returning a handle lets users fit once and then extract coefficients, Type III tests, covariance parameters, G/R covariance matrices, random effects, fit statistics, fitted values, and residuals separately.

Users should drop handles that are no longer needed, especially in large workbooks or validation workbooks that fit many models.

---

## 16. Performance considerations

The runtime of an LMM depends on more than the number of rows.

| Driver | Why it matters |
|---|---|
| Number of subjects or clusters | More blocks increase likelihood and derivative work. |
| Rows per subject or cluster | Larger blocks require larger matrix operations. |
| Number of fixed-effect columns | Affects GLS solving and fixed-effect covariance calculations. |
| Number of random-effect columns | Increases the dimension of \(G\) and the cost of forming \(Z_iGZ_i^\top\). |
| G-side covariance structure | Unstructured covariance grows as \(q(q+1)/2\); VC/Diag grows only as \(q\). |
| R-side covariance structure | Unstructured residual covariance grows as \(T(T+1)/2\); TOEPH grows as \(2T-1\). |
| Inference method | Kenward-Roger and Satterthwaite can require derivative and matrix-adjustment work beyond the fit itself. |
| Scaling and collinearity | Poorly scaled or highly collinear predictors can slow convergence or create unstable estimates. |

Practical performance guidance:

- start with the simplest random-effect pattern that matches the scientific design;
- use **Variance Components (VC/Diag)** before trying unstructured G covariance for many random effects;
- use identity residual covariance first, then add R-side structure only when needed;
- center and scale random-slope predictors when their units are large or poorly conditioned;
- use ML for fixed-effect model comparisons and REML for final variance/inference workflows when appropriate;
- use trace output for troubleshooting, not for every routine analysis.

---

## 17. Validation strategy

The LMM implementation is validated through several complementary test categories.

| Test category | What it protects against |
|---|---|
| Core LMM fits | Regression failures in likelihood, covariance construction, convergence reporting, and fixed-effect output. |
| Random-effect design tests | Incorrect handling of random intercepts, random slopes, multiple random effects, and random-effect interactions. |
| G-side covariance tests | Incorrect parameter counts, aliases, transformations, derivative behavior, and covariance/correlation output. |
| R-side covariance tests | Incorrect residual covariance construction, visit/order indexing, parameter counts, and submatrix extraction. |
| Inference tests | Incorrect standard errors, denominator degrees of freedom, test statistics, p-values, or confidence intervals. |
| Worksheet-function tests | Broken handles, extraction functions, table shapes, row labels, and user-facing error messages. |
| Cross-software checks | Differences from R or SAS-style fits that need to be explained by coding, screening, covariance, optimizer, or inference settings. |

Validation uses both structural expectations and numerical tolerances. Tolerances are necessary because mixed-model fits can differ across software due to floating-point linear algebra, optimizer tolerances, parameterization, boundary handling, and small-sample inference approximations.

---

## 18. Practical limitations

The current LMM implementation is intentionally focused. Users should be aware of these practical limits:

- the response is continuous and modeled with a Gaussian likelihood;
- subject or cluster IDs may be text or numeric, but they are grouping labels, not numeric predictors;
- missing responses are not imputed;
- fixed-effect and random-effect designs must have enough information after row screening;
- covariance structures require enough data support for the requested number of parameters;
- random-effect interactions and rich G-side structures can be unstable in small datasets;
- rich G-side and R-side structures can compete with each other;
- small-sample inference methods are approximations and can differ across software;
- G-side ordered structures use random-effect column order, while R-side ordered structures use the visit/time/order variable;
- worksheet-function handles store fitted results and should be cleared when no longer needed.

When a model has convergence warnings, boundary variance estimates, correlations near \(-1\) or \(1\), very small denominator degrees of freedom, or unstable fixed-effect estimates, simplify the random-effect structure, simplify the covariance structure, review retained rows, check scaling, and compare sensitivity analyses before reporting final results.

---

## 19. Why the implementation was made this way

### Why use subject or cluster blocks?

Blocks match the statistical model. Each subject or cluster has its own response vector, design rows, random-effect contribution, and residual covariance block. This supports unbalanced data and avoids forcing the data into a wide balanced matrix.

### Why separate G-side and R-side covariance?

Random-effect covariance and residual covariance represent different sources of variation. Keeping them separate helps users build interpretable models and avoid accidentally treating residual correlation as random-effect variability or vice versa.

### Why default to identity residual covariance?

In many LMMs, random effects explain the main within-subject or within-cluster dependence. Identity residual covariance is therefore a stable starting point. More complex R-side covariance can be added when design knowledge or diagnostics suggest remaining residual dependence.

### Why use VC/Diag for multiple random effects by default?

A fully unstructured G matrix can become parameter-heavy quickly. VC/Diag estimates one variance per random-effect column and fixes covariances to zero, which is often a more stable first model for multiple random slopes or random interactions.

### Why transform covariance parameters?

Optimizers work on unconstrained numeric values, but covariance matrices must be positive definite and variances must be positive. Internal transformations let the optimizer search efficiently while reducing invalid covariance proposals.

### Why keep both UI and UDF paths?

The ribbon workflow is useful for guided analyses and formatted output. Worksheet functions are useful for reproducible workbooks, custom validation sheets, and chained calculations. Both paths use the same fitted-model logic so results remain consistent.
