# MMRM implementation details

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** explain how the BESH Stat NG mixed model for repeated measures (MMRM) is implemented and why the implementation was designed this way. This page is intended for advanced users, validation reviewers, and method maintainers. It is not a programming reference, and it intentionally avoids documenting internal classes unless a name is needed to identify the public user-facing surface.

For the statistical model and formulas, see [Model and mathematics](model-and-mathematics.md). For dialog settings and result tables, see [Options and output reference](options-and-output.md). For estimands and post-estimation output, see [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 1. Implementation philosophy

The MMRM implementation follows four design principles.

| Design principle | Practical consequence |
|---|---|
| **Keep MMRM and LMM workflows distinct** | The MMRM interface is focused on marginal repeated-measures models with a residual-side within-subject covariance matrix. Random-effects LMM analyses, including random intercepts, random slopes, G-side covariance, BLUPs, and subject-specific predictions, are documented separately in [Linear Mixed Models (LMM)](../linear-mixed-models-lmm.md). |
| **Fit the observed-data likelihood** | Subjects with incomplete visits can still contribute all observed response values. Missing visits are not filled in, imputed, or forced into a balanced matrix. |
| **Use subject-level covariance blocks** | Each subject contributes a response vector, design rows, and a covariance submatrix. This is natural for MMRM and keeps missing-visit handling explicit. |
| **Prefer transparent, reproducible output** | The ribbon workflow writes retained analysis data, fitted values, optional residuals, model tables, post-estimation tables, fit statistics, and optional iteration trace to Excel sheets that can be reviewed and archived. |

The implementation reuses shared Gaussian mixed-model infrastructure internally, but the supported MMRM workflow remains the marginal repeated-measures workflow. Users should think of the method as:
 
\[
y_i = X_i\beta + \varepsilon_i, \qquad \varepsilon_i \sim N\{0, R_i(\theta)\},
\]

where the within-subject covariance matrix \(R_i(\theta)\) is the whole marginal covariance matrix for subject \(i\). No user-specified random-effect contribution is active in the MMRM path.

---

## 2. Public surface

The public user-facing surface consists of:

- the Excel ribbon command **Mixed Models for Repeated Measures (MMRM)**,
- the MMRM dialog described in [User interface](user-interface.md),
- the worksheet functions named `BESH.REGR.MMRM_*`, described in [Worksheet functions](worksheet-functions.md),
- the output workbook tables described in [Options and output reference](options-and-output.md).

The implementation contains shared mixed-model infrastructure because MMRM and ordinary Gaussian linear mixed models share likelihood, covariance, derivative, and inference machinery. The MMRM workflow remains the marginal repeated-measures workflow with no user-specified random effects. Random-effects LMM analyses are documented separately in [Linear Mixed Models (LMM)](../linear-mixed-models-lmm.md).
 
!!! note "MMRM versus LMM"
    MMRM models the repeated-measures covariance directly through the residual/marginal covariance matrix and is often used for adjusted visit-specific means and planned repeated-measures contrasts. LMM introduces user-specified random effects and G-side covariance structures, which support subject- or cluster-specific intercepts, slopes, and random-effect predictions.

---

## 3. End-to-end workflow

The ribbon and worksheet-function paths use the same conceptual workflow.

1. **Collect user inputs.**  
   The user selects the response, subject identifier, visit/time variable, fixed effects, categorical predictors, covariance structure, estimation method, inference method, and post-estimation options.

2. **Screen and align rows.**  
   The implementation retains rows that have valid values for all required fields in the selected model. Invalid rows are excluded before the model is fitted.

3. **Build the fixed-effect design matrix.**  
   Selected main effects, interactions, categorical encodings, and the intercept option are expanded into the matrix \(X\). Column names are retained so that coefficient, Type III, and post-estimation output can be labeled.

4. **Create subject blocks.**  
   Rows are grouped by subject. For each subject, the implementation stores the retained response values, fixed-effect design rows, original row positions, and observed visit indices.

5. **Construct within-subject covariance blocks.**  
   For a proposed covariance-parameter vector \(\theta\), the selected covariance structure builds each subject-specific covariance matrix \(R_i(\theta)\). Subjects with missing visits use the appropriate observed-visit submatrix.

6. **Profile fixed effects while optimizing covariance parameters.**  
   For each candidate covariance parameter vector, fixed effects are estimated by generalized least squares. The optimizer then updates covariance parameters by minimizing the profiled ML or REML objective.

7. **Compute fixed-effect inference.**  
   After convergence, the selected inference method is applied to coefficients, term-level hypotheses, LS-means, and contrasts.

8. **Create post-estimation tables.**  
   Optional LS-means, group contrasts, change-from-baseline estimates, difference-in-change estimates, fitted values, residuals, covariance matrices, and fit statistics are produced.

9. **Write or return results.**  
   The ribbon path writes Excel output sheets. The worksheet-function path stores a fitted model behind a handle and lets extraction functions return specific tables.

---

## 4. Data preparation and retained analysis rows

MMRM requires long-format repeated-measures data. Each retained row represents one observed measurement occasion for one subject.

A row can be excluded before fitting when a required value is missing or invalid, for example:

- missing or non-numeric response,
- missing subject identifier,
- invalid visit/time value,
- missing or non-numeric fixed-effect predictor required by the selected model,
- a categorical value that cannot be represented consistently in the model design,
- a row outside the selected input range or not aligned across selected ranges.

This is row-level screening, not subject-level complete-case deletion. A subject does not need to have every scheduled visit. If at least one valid response row remains for a subject, that subject can contribute to the likelihood.

!!! important "Missing visits are handled through the likelihood"
    Missing visits are not imputed and are not treated as zero. The model is fitted to the observed response vector for each subject. Under the usual likelihood assumptions for MMRM, this is the appropriate behavior for incomplete repeated-measures profiles when missingness is ignorable given the modeled information.

The ribbon output writes the retained analysis rows to the **Data** sheet. This is intentional: it lets users verify exactly which rows were analyzed before interpreting model output.

---

## 5. Subject blocks and visit indexing

The implementation uses a subject-block representation. For subject \(i\), the fitted model works with:

- \(y_i\), the retained response vector,
- \(X_i\), the retained fixed-effect design rows,
- the observed visit values,
- the row indices that connect fitted values and residuals back to the worksheet.

This design is important because MMRM covariance structures are usually defined over a common visit scale. For example, an unstructured four-visit covariance matrix has parameters for visits 1, 2, 3, and 4. If a subject is missing visit 3, the subject still contributes the submatrix for the visits that are observed.

The implementation therefore maps raw visit values to a global ordered visit index. The covariance structure is defined in that global visit space, and each subject receives the observed-visit submatrix required for that subject.

This design was chosen because it:

- preserves incomplete subject profiles,
- keeps covariance parameters aligned to the same visit across subjects,
- supports monotone and intermittent missingness,
- avoids silently treating row order as time when an explicit visit variable is available,
- matches how MMRM is commonly described in clinical repeated-measures analyses.

---

## 6. Mean-model construction

The mean model is represented by the fixed-effect design matrix \(X\). In the ribbon workflow, the design matrix is built from the variables and terms selected in the **Build Model** tab. In the worksheet-function workflow, it can be supplied directly or constructed from a supported formula expression.

The implementation keeps track of the design-column names because they are needed later for:

- coefficient estimates,
- Type III tests,
- estimability checks,
- LS-means and contrasts,
- custom linear functions created by `BESH.REGR.MMRM_LSMESTIMATE`.

Categorical predictors and interactions are expanded before fitting. This means that the numerical model operates on design columns, while the output attempts to report results in terms of the original model terms selected by the user.

!!! tip "Interpret LS-means and contrasts rather than raw coding coefficients"
    Raw fixed-effect coefficients depend on coding and reference levels. For most applied MMRM reporting, estimated marginal means, treatment contrasts, change-from-baseline estimates, and difference-in-change estimates are more directly interpretable.

---

## 7. Covariance construction

The MMRM covariance model is residual-side only. For each subject, the implementation builds a matrix \(R_i(\theta)\) from the selected residual covariance structure.

The currently documented user-facing covariance structures are:

| Structure | Implementation idea | Main use |
|---|---|---|
| **Identity** | One common variance and no within-subject covariance. | Simplest working model or diagnostic comparison. |
| **Diagonal heterogeneous** | Different variance by visit, no covariance between visits. | When visit-specific variability matters but serial covariance is not modeled. |
| **Compound symmetry** | Common variance and common covariance/correlation. | Simple exchangeable repeated-measures structure. |
| **Heterogeneous CS** | Visit-specific variances with common correlation. | Useful when spread changes by visit but correlations are approximately exchangeable. |
| **AR(1)** | Common variance with correlation decreasing by visit lag. | Ordered visits with serial decay and approximately constant variance. |
| **Heterogeneous AR(1)** | Visit-specific variances with AR(1) correlation. | Practical compromise for ordered visits with changing variability. |
| **Toeplitz (TOEP)** | Common variance with a separate correlation for each visit lag. | Ordered visits where correlation depends on lag but not necessarily by AR(1) decay. |
| **Heterogeneous Toeplitz (TOEPH)** | Visit-specific variances with a separate correlation for each visit lag. | Ordered visits with changing variability and lag-specific correlation. |
| **Unstructured** | A flexible positive-definite visit-level covariance matrix. | Standard MMRM default when the data support the number of parameters. |

### Parameter constraints

Covariance parameters are optimized on internal scales that make invalid covariance matrices less likely:

- variance-like quantities are represented on a log scale so they remain positive after transformation,
- correlation-like quantities are represented on an unconstrained scale and transformed back into the interval \((-1, 1)\),
- Toeplitz-style lag correlations are represented through partial-autocorrelation parameters so the implied Toeplitz correlation matrix remains positive definite,
- the unstructured covariance matrix is represented through a Cholesky-style factor so the full visit-level covariance matrix is positive definite for finite parameter values.

The output is transformed back to interpretable covariance, variance, standard-deviation, and correlation quantities where appropriate.

### Handling invalid proposals

Even with careful parameterization, a trial optimizer step can occasionally produce an unusable numerical state: a matrix may be singular, nearly singular, non-finite, or impossible to invert reliably. Such trial values are not accepted as fitted models. Instead, the objective function returns a deliberately poor value, which guides the optimizer away from invalid regions of the parameter space.

This is a numerical safeguard, not a statistical penalty such as ridge regression, LASSO, AIC, or BIC.

---

## 8. Likelihood evaluation

For a fixed covariance-parameter vector \(\theta\), the implementation evaluates the model subject by subject.

At a high level, the calculation uses:

\[
V_i = R_i(\theta),
\]

and combines across subjects:

\[
X^\top V^{-1}X = \sum_i X_i^\top V_i^{-1}X_i,
\qquad
X^\top V^{-1}y = \sum_i X_i^\top V_i^{-1}y_i.
\]

The fixed-effect estimate for that covariance proposal is the generalized least-squares estimate:

\[
\hat\beta(\theta) =
\left(X^\top V^{-1}X\right)^{-1}X^\top V^{-1}y.
\]

The optimizer works with a profiled objective: fixed effects are updated from the GLS equations for each covariance proposal, and the optimizer searches over \(\theta\). Both ML and REML criteria are supported. REML is the practical default because it is usually preferred for covariance estimation and is required for Kenward-Roger inference in BESH Stat NG.

This profiled design was chosen because it avoids optimizing fixed effects and covariance parameters as one large parameter vector. That improves numerical stability and matches the standard way many mixed-model implementations fit Gaussian MMRM models.

---

## 9. Optimization strategy

The covariance optimizer is deliberately conservative. MMRM users often choose an unstructured covariance matrix, and such models can be numerically demanding when the sample size is modest, visits are sparse, or some treatment-by-visit cells have limited support.

The default strategy is:

1. **Start from conservative covariance values.**  
   Starting values are based on simple variance estimates and neutral correlation starts where possible.

2. **Use profiled covariance optimization.**  
   Fixed effects are updated by GLS for each covariance proposal; the optimizer updates only the covariance parameters.

3. **Use average-information / Fisher-scoring updates for REML when available.**  
   This path is designed to behave similarly to established mixed-model software for common REML MMRM fits.

4. **Use analytic covariance gradients where validated.**  
   Analytic derivatives are faster and more stable for supported structures. Numerical derivatives remain available as a fallback or diagnostic path.

5. **Apply safeguards and fallback behavior.**  
   The optimizer uses step-size controls, objective checks, positive-definiteness checks, and fallback logic so that a failed trial step does not become a reported fit.

6. **Report convergence state.**  
   The output includes convergence status, iteration counts, and fit statistics. Optional trace output gives more detail when troubleshooting is needed.

!!! note "Why optimization details matter"
    Cross-software differences in MMRM are often caused less by the model formula and more by parameterization, optimizer tolerances, starting values, covariance constraints, and degrees-of-freedom calculations. The implementation therefore keeps diagnostics available for validation, while keeping the normal user workflow simple.

---

## 10. Fixed-effect inference

After the covariance parameters and fixed effects are estimated, BESH Stat NG computes fixed-effect inference using the selected method.

The inference layer supports:

- large-sample normal/Wald inference,
- residual degrees of freedom,
- between-within degrees of freedom,
- Satterthwaite degrees of freedom,
- Kenward-Roger adjusted inference.

The same inference machinery is used for:

- coefficient tables,
- Type III tests,
- LS-means,
- treatment contrasts,
- change-from-baseline estimates,
- difference-in-change estimates,
- custom linear estimates.

### Kenward-Roger implementation logic

Kenward-Roger inference is more than a different denominator degrees-of-freedom formula. It adjusts the covariance matrix of fixed-effect estimates and then computes finite-sample inference from the adjusted covariance. In BESH Stat NG, Kenward-Roger is tied to REML because the adjustment is derived for REML-based covariance estimation.

The implementation constructs derivative information for the covariance model, uses a Taylor-series approximation to account for covariance-parameter uncertainty, and applies the resulting adjustment to fixed-effect linear estimates. Multi-row hypotheses use the corresponding adjusted F-test logic when available.

This approach was chosen because Kenward-Roger is a common expectation for clinical-trial-style MMRM analyses and because it improves finite-sample behavior compared with purely large-sample Wald inference.

### Fallback behavior

If an advanced inference adjustment cannot be computed reliably for a specific model, the result includes warnings and falls back to a simpler inference path where needed. This is preferable to silently reporting an unstable small-sample adjustment.

---

## 11. Type III tests and estimability

Type III tests are built from hypothesis matrices that correspond to model terms. This is more robust than simply testing each design column separately, because categorical predictors and interactions usually expand into several columns.

The implementation checks whether requested linear functions are estimable from the fitted design. Non-estimable or rank-deficient requests should not be interpreted as ordinary tests of the intended term or contrast.

This matters in MMRM because empty cells, sparse visit-by-treatment combinations, or redundant predictors can make some design columns aliased or weakly supported.

---

## 12. LS-means, contrasts, and custom estimates

Post-estimation is implemented as linear functions of the fitted fixed effects:

\[
\hat\mu_L = L\hat\beta.
\]

The practical difference between LS-means, contrasts, and custom estimates is how the row vector or matrix \(L\) is constructed.

### Observed-design grid

The observed-design-grid approach averages fitted design rows from the retained analysis data for the requested visit, group, or profile. This was chosen as the default-style workflow because it is transparent and avoids asking users to define a separate reference population before they can obtain useful LS-means.

### Reference grid

The reference-grid workflow constructs target profiles from factor levels and continuous-covariate rules. It is useful when users need LS-means corresponding to a controlled reference population rather than the observed distribution in the retained analysis data.

### Custom estimates

When a custom contrast is needed beyond the built-in pairwise, change-from-baseline, or difference-in-change tables, use the worksheet function `BESH.REGR.MMRM_LSMESTIMATE`. The function creates a user-defined linear estimate from the fitted model handle and the supplied profile or weighting specification.

This separation keeps the ribbon workflow approachable while still giving expert users a flexible path for analysis-plan-specific estimands.

---

## 13. Output architecture

The ribbon workflow writes results to Excel sheets rather than hiding output in modal dialogs.

| Output area | Implementation purpose |
|---|---|
| **Data sheet** | Preserves retained analysis rows, subject/visit identifiers, design information, fitted values, and optional residuals. |
| **MMRM sheet** | Contains model information, estimates, Type III tests, LS-means, contrasts, covariance output, fit statistics, and convergence details. |
| **MMRM Trace sheet** | Optional detailed iteration/diagnostic output for validation and troubleshooting. |
| **Worksheet-function handle** | Stores a fitted model so extraction functions can return specific tables without refitting each time. |

The handle-based worksheet-function design is important for performance. A fitted MMRM can be expensive, especially with unstructured covariance and Kenward-Roger inference. Returning a handle lets users fit once and then extract coefficients, Type III tests, covariance parameters, LS-means, contrasts, fitted values, and residuals separately.

---

## 14. Validation strategy

The implementation is validated through several complementary test categories.

| Test category | What it protects against |
|---|---|
| Core MMRM fits | Regression failures in likelihood, covariance construction, fitting, and convergence reporting. |
| Missing-visit cases | Incorrect deletion of incomplete subjects or wrong covariance submatrix extraction. |
| Covariance-structure tests | Errors in parameter counts, starting values, transformations, and matrix construction. |
| Inference tests | Incorrect denominator degrees of freedom, adjusted covariance matrices, test statistics, p-values, or confidence intervals. |
| Kenward-Roger derivative tests | Errors in derivative scale, finite-difference behavior, and Taylor-series adjustment components. |
| LS-means and contrast tests | Incorrect construction of \(L\) matrices, sign conventions, baseline handling, and comparison levels. |
| Worksheet-function tests | Broken fit handles, extraction functions, table shapes, and user-facing UDF behavior. |
| Cross-software reference checks | Differences from established R and SAS-style workflows that need to be explained or corrected. |

Validation uses both exact structural expectations and numerical tolerances. Tolerances are necessary because MMRM results can differ slightly across software due to optimizer tolerances, covariance parameterization, floating-point linear algebra, convergence rules, and small-sample degrees-of-freedom algorithms.

---

## 15. Why the implementation was made this way

### Why subject blocks instead of a wide matrix?

A wide matrix is convenient for balanced data, but MMRM is frequently used precisely because subjects can have missing visits. Subject blocks allow each subject to contribute only the observed response vector and the corresponding covariance submatrix.

### Why visit-indexed covariance structures?

Visit-indexed structures ensure that covariance parameters have the same meaning across subjects. For example, the variance for visit 4 is the same model parameter whether a subject has visits 1-4 or only visits 2 and 4.

### Why transform covariance parameters?

Optimizers work best on unconstrained numeric vectors. Variances must remain positive and correlations must remain inside valid bounds. Internal transformations let the optimizer search freely while the model receives valid covariance quantities.

### Why Cholesky parameterization for unstructured covariance?

Directly optimizing every variance and covariance can easily propose matrices that are not positive definite. A Cholesky-style parameterization is more stable because it constructs the covariance matrix from a factor whose diagonal elements are constrained positive.

### Why profile fixed effects?

For a proposed covariance matrix, the fixed effects have a closed-form GLS solution. Profiling fixed effects out of the optimizer reduces the dimension of the numerical search and improves stability.

### Why keep both UI and UDF paths?

The ribbon workflow is useful for guided analyses and formatted output. The worksheet functions are useful for reproducible workbooks, custom estimates, chained calculations, and validation checks. Both paths use the same fitted-model logic so results are consistent.

---

## 16. Practical limitations

The current MMRM implementation is intentionally focused. Users should be aware of these practical limits:

- the response is continuous and modeled with a Gaussian likelihood,
- random-effect LMM workflows are handled by the separate [Linear Mixed Models (LMM)](../linear-mixed-models-lmm.md) method rather than by the MMRM dialog,
- missing responses are not imputed,
- covariance structures require enough data support for the requested number of parameters,
- unstructured covariance can be difficult with many visits, small samples, or sparse visit patterns,
- small-sample inference methods are approximations and can differ across software,
- reference-grid definitions and weighting rules must match the intended estimand,
- residual diagnostics are useful but not a complete substitute for substantive model checking,
- cross-software agreement depends on matching data screening, factor coding, covariance structure, estimation method, inference method, and contrast definitions.

When a model has convergence warnings, unstable covariance estimates, very small denominator degrees of freedom, or non-estimable contrasts, users should simplify the covariance structure, review the design support, check retained rows, and compare sensitivity analyses before reporting final results.

