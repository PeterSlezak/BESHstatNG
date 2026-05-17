# LMM options and output reference

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** provide an option-by-option reference for the BESH Stat NG **Linear Mixed Models (LMM)** dialog and explain the output tables written to the result workbook. For the formal model, likelihood, covariance-structure formulas, and degrees-of-freedom mathematics, see [Model and mathematics](model-and-mathematics.md). For practical workflows using the supplied dataset, screenshots, and result workbooks, see [Examples and interpretation](examples.md).

---

## 1. Where the options are used

The LMM dialog separates the analysis into four practical decisions:

1. **Input and grouping variables** — choose the response, subject or cluster identifier, optional visit/order variable, and candidate source variables.
2. **Fixed and random model terms** — build the population-average fixed effects and the subject- or cluster-specific random effects.
3. **Covariance and estimation options** — choose the G-side random-effects covariance, optional R-side residual covariance, ML/REML fit method, inference method, and numerical controls.
4. **Output options** — choose whether to write covariance matrices, random-effect predictions, fitted values, residuals, trace output, and diagnostic details.

The ribbon workflow writes at least:

- a **Data** sheet with the retained analysis rows, the expanded fixed-effect design, the expanded random-effect design, subject labels, optional visit/order labels, fitted marginal values, and optional residuals;
- an **LMM** sheet with fixed-effect estimates, Type III tests when applicable, class-level information, covariance parameters, G/R covariance and correlation output when requested, random-effect predictions when requested, fit statistics, and convergence information;
- optionally an **LMM Trace** sheet when trace or detailed iteration output is requested.

Not every table appears in every output workbook. For example, R-side covariance/correlation matrices are written only when that output is requested and an R-side matrix is available. In the supplied `200lmm` examples, Models 6 and 7 include R covariance and R correlation tables, while several earlier models focus on G-side output.

!!! tip "Practical default"
    Start with **REML**, **Kenward-Roger**, **Random Intercept** or **Random Intercept + Slope** on the G side, and **Identity** on the R side. Add richer G-side or R-side covariance only when the design, scientific question, and convergence behavior support the additional parameters.

---

## 2. Required inputs

| Input | Required? | Practical meaning | Notes |
|---|---:|---|---|
| **Response** | Yes | Continuous numeric outcome. | Rows with missing or nonnumeric response values are excluded. |
| **Subject ID / cluster ID** | Yes | Defines the independent grouping blocks for random effects. | May be text or numeric in the ribbon workflow. Examples: `Subject`, `School`, `Batch`, `Center`. |
| **Visit / Time / Ordering** | Conditional | Defines the order used by visit-indexed R-side residual covariance structures. | Required for structures such as AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, diagonal heterogeneous, and unstructured residual covariance. |
| **Model source variables** | Usually | Candidate variables used to build fixed and random effects. | Numeric variables can be used directly. Numeric-coded variables can be added as categorical factors when appropriate. |

The analysis uses only complete retained rows for the selected response, fixed-effect design, random-effect design, subject ID, and any required visit/order variable. The output **Data** sheet records the retained rows and expanded design columns so the fitted model can be audited.

!!! note "Text subject IDs are supported"
    The subject or cluster identifier can be a text column, such as `PT001`, `Center A`, or `Batch-17`. Text identifiers are used to define grouping blocks. Numeric predictors, the response, and the visit/order variable must still be valid numeric analysis inputs where required.

---

## 3. Building fixed effects

Fixed effects describe the population-average mean model. They are the effects reported in the **Fixed effects** and **Type III** output tables.

| Control / action | Meaning | Use when |
|---|---|---|
| **Fixed intercept** | Adds an intercept to the fixed-effect design. | Usually selected. Clear it only for intentionally no-intercept models. |
| **Add** | Adds selected variables as continuous fixed effects. | The predictor is numeric and a one-unit slope is meaningful. |
| **Categorical factor** | Expands a selected numeric-coded variable into indicator columns. | The numeric codes represent categories, such as treatment group, site, or visit level. |
| **Two-way interaction** | Adds pairwise interaction terms among selected variables or factors. | The effect of one predictor is expected to differ by another predictor. |
| **Custom interaction** | Adds a specific interaction term. | You want one interaction rather than all two-way combinations. |
| **Polynomial** | Adds powers of a selected continuous variable. | Curvature is scientifically meaningful and the predictor scale is appropriate. |
| **Remove / clear** | Removes selected fixed terms or clears the fixed-effect list. | Correcting the model specification before fitting. |

The fixed-effect design is shown in the **Data** sheet. Categorical factor columns use labels such as `Treatment[1]` or `Site[3]`, and interaction columns combine the component names.

!!! warning "Categorical and continuous time answer different questions"
    A continuous time term estimates one average slope. A categorical visit/time factor estimates separate differences for each nonreference time level. The supplied examples intentionally show both styles: Models 1 to 3 use `Days_c` as a continuous time covariate, Models 4, 5, and 7 use `Days_c` as a categorical fixed effect, and Model 6 uses a continuous `Days_c:Treatment` interaction in the saved workbook.

---

## 4. Building random effects

Random effects describe how subjects or clusters deviate from the population-average model. They are represented by the random-effect design matrix and the G-side covariance structure.

| Control / action | Meaning | Use when |
|---|---|---|
| **Random intercept** | Allows each subject or cluster to have its own baseline shift. | Most LMMs with repeated or clustered observations. |
| **Add random effect** | Adds selected variables as random slopes or random covariates. | Subjects or clusters may differ in the effect of time, dose, difficulty, or another predictor. |
| **Random categorical factor** | Adds random-effect columns based on a categorical variable. | The subject-specific deviation may vary by category level. Use cautiously because this can add several random-effect columns. |
| **Random two-way interaction** | Adds random slopes for an interaction. | Subject-specific interaction patterns are scientifically meaningful and supported by the data. |
| **Random custom interaction** | Adds a specified random interaction. | You need one targeted interaction in the random-effect design. |
| **Random polynomial** | Adds random polynomial terms. | Subject-specific curvature is plausible. Center and scale predictors before fitting. |

When multiple random-effect columns are added, the G-side covariance choice becomes important. A diagonal or variance-components structure is often a safer first choice than a fully unstructured G matrix.

---

## 5. G-side random-effects covariance structure

The **G-side** structure controls the covariance matrix of the random effects. Let `q` be the number of random-effect columns, including the random intercept when selected.

| UI value | Parameters | Meaning | Good starting use |
|---|---:|---|---|
| **Random Intercept** | 1 | One subject/cluster variance for the random intercept. | Repeated or clustered observations with subject-specific baseline shifts. |
| **Random Intercept + Slope** | 3 | Intercept variance, slope variance, and intercept-slope covariance. | Longitudinal data where subjects differ in baseline and linear time trend. |
| **Identity** | 1 | All random effects have the same variance and zero covariance. | Simple or constrained models where a common random-effect variance is intended. |
| **Variance Components (VC/Diag)** | `q` | Each random effect has its own variance; covariances are fixed to zero. | Multiple random effects when an unstructured G matrix is too parameter-heavy. |
| **Compound Symmetry (CS)** | 2 when `q > 1` | Common variance and common covariance/correlation among random effects. | Random effects are exchangeable and similarly scaled. |
| **Heterogeneous Compound Symmetry (CSH)** | `q + 1` when `q > 1` | Separate variances with one common correlation. | Random effects have different scales but a common association. |
| **Autoregressive (AR1)** | 2 when `q > 1` | Common variance with correlations decreasing by random-effect column order. | Ordered random-effect columns, such as ordered basis or time-profile effects. |
| **Heterogeneous Autoregressive (ARH1)** | `q + 1` when `q > 1` | Separate variances with AR(1)-style correlation by random-effect column order. | Ordered random-effect columns with changing random-effect variances. |
| **Toeplitz (TOEP)** | `q` | Common variance with separate lag correlations by random-effect column order. | Ordered random effects where each lag can have its own correlation. |
| **Heterogeneous Toeplitz (TOEPH)** | `2q - 1` | Separate variances with separate lag correlations. | Flexible ordered random-effect covariance, but more parameter-heavy. |
| **Unstructured Random Effects** | `q(q+1)/2` | Every variance and covariance is estimated. | Small/moderate `q` when correlations among random effects are important and estimable. |

!!! important "G-side AR(1) and Toeplitz use random-effect column order"
    G-side AR(1), ARH(1), Toeplitz, and heterogeneous Toeplitz do not use the Visit/Time/Ordering variable directly. They use the order of the random-effect design columns. These structures are meaningful only when the random-effect columns have a natural order.

### Choosing a G-side structure

A practical sequence is:

1. Fit a **Random Intercept** model.
2. Add a key random slope and use **Random Intercept + Slope**.
3. For several random effects, start with **Variance Components (VC/Diag)**.
4. Try **Unstructured Random Effects** only when the number of random-effect columns is modest and the data support estimating covariances.
5. Use **CS**, **CSH**, **AR1**, **ARH1**, **TOEP**, or **TOEPH** when their assumptions match the order or exchangeability of the random-effect columns.

For a detailed discussion of these structures, see [Random effects and covariance structures](random-effects-and-covariance.md).

---

## 6. R-side residual covariance structure

The **R-side** structure controls residual covariance after fixed and random effects have been included. Let `T` be the number of distinct visit/order levels used by the selected residual covariance structure.

| UI value | Parameters | Meaning | Visit/order required? |
|---|---:|---|---:|
| **Identity** | 1 | Constant residual variance and zero residual covariance. | No |
| **Diagonal Heterogeneous** | `T` | Visit-specific residual variances with zero residual covariance. | Yes |
| **Compound Symmetry** | 2 | Common residual variance and common residual covariance/correlation. | Usually no |
| **Heterogeneous CS** | `T + 1` | Visit-specific residual variances with one common residual correlation. | Yes |
| **AR(1)** | 2 | Common residual variance with correlation decreasing by visit lag. | Yes |
| **Heterogeneous AR(1)** | `T + 1` | Visit-specific residual variances with AR(1)-style residual correlation. | Yes |
| **Toeplitz (TOEP)** | `T` | Common residual variance with separate lag correlations. | Yes |
| **Heterogeneous Toeplitz (TOEPH)** | `2T - 1` | Visit-specific residual variances with separate lag correlations. | Yes |
| **Unstructured** | `T(T+1)/2` | Every visit variance and covariance is estimated. | Yes |

R-side covariance is optional in LMM. Many random-intercept and random-slope models start with **Identity** residual covariance because the random effects already explain part of the within-subject association. Add R-side correlation when residual dependence remains after the random effects, or when the scientific model explicitly requires residual correlation.

!!! warning "Avoid double-counting complexity"
    A rich random-effects structure and a rich residual covariance structure can compete to explain the same within-subject dependence. When convergence is unstable or variance/correlation estimates are near boundaries, simplify either the G side or the R side and refit.

---

## 7. Fit method

### REML

**REML** estimates covariance parameters using restricted maximum likelihood. It is the usual default for LMMs when fixed-effect inference and variance-component estimation are the main goals.

Use REML when:

- the fixed-effect design is already chosen;
- Satterthwaite or Kenward-Roger inference is requested;
- the goal is stable covariance estimation rather than fixed-effect model selection.

For REML fits, information criteria are reported as diagnostic quantities. Do not use REML AIC/BIC to compare models with different fixed-effect designs.

### ML

**ML** estimates fixed effects and covariance parameters under the ordinary maximum-likelihood criterion.

Use ML when:

- comparing models with different fixed-effect designs on the same response and retained rows;
- matching another software result that was fitted with ML;
- performing likelihood-ratio-style sensitivity checks where ML is required.

!!! warning "Kenward-Roger should be used with REML"
    Kenward-Roger inference is designed for REML-style mixed-model inference. Use REML for Kenward-Roger fits.

---

## 8. Inference method

The inference method controls fixed-effect standard errors, test statistics, denominator degrees of freedom, p-values, confidence intervals, and Type III tests.

| Inference method | Output style | When to use | Main caution |
|---|---|---|---|
| **Large-sample normal** | Wald z-style tests and normal confidence intervals. | Large samples, quick checks, or comparison with asymptotic software output. | No finite-sample denominator degrees of freedom. |
| **Residual DF** | t tests using a residual degrees-of-freedom approximation. | Simple finite-DF approximation or troubleshooting. | Less tailored to mixed-model covariance uncertainty. |
| **Satterthwaite** | t tests with Satterthwaite denominator DF. | Small-sample fixed-effect inference when KR is not needed or is too expensive. | Approximate and can be sensitive to covariance estimates. |
| **Kenward-Roger** | adjusted fixed-effect covariance, denominator DF, and term-level F tests. | Moderate/small samples or final inference where KR is appropriate. | More computationally expensive, especially with rich covariance structures. |

With finite-DF methods, the fixed-effects table includes a **DF** column and t statistic. With large-sample normal inference, the table reports z-style tests and confidence intervals without denominator DF.

---

## 9. Alpha and confidence intervals

The **alpha** setting controls two-sided confidence intervals in the fixed-effects output. The default is usually `0.050`, giving 95% confidence intervals.

For a coefficient estimate \\(\hat\beta_j\\), BESH Stat NG reports intervals of the form:

\[
\hat\beta_j \pm c_{1-\alpha/2}\,\operatorname{SE}(\hat\beta_j),
\]

where the critical value is chosen from the selected inference method.

---

## 10. Numerical and diagnostic options

| Option | Meaning | Practical advice |
|---|---|---|
| **Convergence criterion** | Tolerance used for gradient, step, and objective-change stopping checks. | The default is usually appropriate. Tighten only for validation work. |
| **Max. iterations** | Maximum covariance-optimizer iterations. | Increase for difficult covariance structures before concluding that the model cannot fit. |
| **Covariance optimizer mode** | Chooses the covariance-parameter optimization strategy. | The default AI/Fisher-scoring path is usually the first choice. Projected BFGS-style optimization can be useful for difficult structures. |
| **Covariance gradient mode** | Chooses analytic, numerical, validation, or automatic derivative behavior. | Use Auto for routine work. Use validation modes when testing or debugging numerical behavior. |
| **Trace** | Writes optimizer trace information. | Useful for diagnosing slow convergence, step-halving, or unstable objectives. |
| **Iteration details** | Writes more detailed iteration information. | Useful for validation and performance review; can make output larger. |
| **Diagnostics** | Adds convergence and derivative diagnostics where available. | Recommended when testing new covariance structures or complex random effects. |
| **Interrupt / cancel** | Allows a long-running fit to be interrupted. | Use when a rich G/R covariance structure is too slow or clearly unstable. |

The **Convergence** output table records whether the fit converged, the message, iteration count, gradient norm, requested tolerances, optimizer mode, gradient mode, and relevant internal diagnostic flags.

---

## 11. Data sheet output

The **Data** sheet is the audit trail for the fitted model. It contains the retained analysis rows and the design columns that were actually fitted.

Common columns include:

| Column type | Example column names | Meaning |
|---|---|---|
| Original row identifier | `Row ID` | Links retained rows back to the source worksheet. |
| Response | `Reaction` | Numeric response used in the fit. |
| Fixed-effect design | `Intercept`, `Days_c`, `Treatment[1]`, `Days_c:Treatment` | Expanded columns of the fixed-effect design matrix. |
| Random-effect design | `Z: Intercept`, `Z: Days_c`, `Z: Difficulty` | Expanded columns of the random-effect design matrix. |
| Subject / cluster ID | `Subject` | Grouping block used for random effects. |
| Visit / order | `Visit` | Visit/order values used by R-side structures when required. |
| Fitted marginal | `Fitted marginal` | Population-average fitted value from the fixed-effect part. |
| Residual | `Residual` | Observed response minus fitted marginal value, when residual output is selected. |

The Data sheet is especially useful for checking whether categorical factors, interactions, and random-effect columns were expanded as intended.

---

## 12. Fixed-effects table

The **Fixed effects** table reports coefficient-level fixed-effect inference.

Typical columns include:

| Column | Meaning |
|---|---|
| Term name | Fixed-effect coefficient label, such as `Intercept`, `Days_c`, or `Treatment[1]`. |
| Estimate | Estimated fixed-effect coefficient. |
| Std. Error | Standard error under the selected inference method. |
| DF | Denominator degrees of freedom for finite-DF methods. Not shown for large-sample normal output. |
| t or z | Test statistic for the coefficient. |
| Pr \( > \lvert t \rvert \) or Pr \( > \lvert z \rvert \) | Two-sided p-value. |
| Lower / Upper CI | Confidence interval limits, when included. |

Interpret fixed-effect coefficients according to the coding of the design matrix. For categorical factors, coefficients are contrasts against the reference level implied by the expanded design. For interactions, lower-order terms describe effects at the reference or zero values of the interacting variables.

!!! tip "Use the Data sheet to understand coefficient coding"
    If a coefficient label is unclear, inspect the fixed-effect design columns in the Data sheet. This is the most direct record of how the model encoded factors and interactions.

---

## 13. Type III fixed-effect tests

When available, the **Type III** or **Kenward-Roger term-level F tests** table reports term-level tests rather than individual coefficient tests.

Typical columns include:

| Column | Meaning |
|---|---|
| Term | Fixed-effect term being tested. |
| Num DF | Numerator degrees of freedom, usually the number of columns associated with the term. |
| Den DF | Denominator degrees of freedom for finite-DF methods. |
| F | Term-level F statistic. |
| Pr\(>F\) | Term-level p-value. |
| Unscaled F | Diagnostic unscaled F value when KR scaling is used. |

Use Type III tests to evaluate multi-column terms such as categorical factors and interactions. Do not treat Type III tests as replacements for scientifically planned contrasts; they answer a broader term-level question.

---

## 14. Class-level information

The **Class level information** table appears when categorical effects are used and the option is selected. It records the levels observed in the cleaned analysis data.

Typical columns include:

| Column | Meaning |
|---|---|
| Model part | Whether the factor belongs to the fixed or random design. |
| Variable | Source variable name. |
| Term kind | Categorical main effect, interaction component, or related term type. |
| Levels | Observed numeric/coded levels. |
| No. levels | Number of observed levels used in the analysis. |

This table is useful for confirming that treatment, site, visit, or other coded factors were interpreted with the intended levels after row screening.

---

## 15. Covariance-parameters table

The **Covariance parameters** table reports optimized covariance parameters. These are primarily diagnostic because some structures are optimized on transformed internal scales.

For example:

- variance parameters may be optimized on a log scale;
- correlations may be optimized on bounded or partial-autocorrelation scales;
- unstructured covariance matrices may be optimized through Cholesky-style parameters.

For interpretation, prefer the user-scale covariance and correlation matrices:

- **Estimated G covariance matrix**;
- **Estimated G correlation matrix**;
- **Estimated R covariance matrix**;
- **Estimated R correlation matrix**.

!!! note "Internal scale versus user scale"
    The covariance-parameters table is useful for validation and troubleshooting. For substantive interpretation, use the G/R covariance and correlation matrices because they are shown on the statistical scale of variances, covariances, and correlations.

---

## 16. G covariance and G correlation matrices

The **Estimated G covariance matrix** shows the fitted covariance matrix for the random effects.

The **Estimated G correlation matrix** rescales the same matrix to correlations:

\[
\operatorname{Corr}(b_j,b_k) = \frac{G_{jk}}{\sqrt{G_{jj}G_{kk}}}.
\]

Use these tables to answer questions such as:

- How large is the random-intercept variance?
- Is there meaningful random-slope variation?
- Are random intercepts and random slopes positively or negatively associated?
- Did a richer G-side structure estimate unstable or near-boundary correlations?

For diagonal structures such as **Variance Components (VC/Diag)**, the off-diagonal covariances are zero by definition. For **Unstructured Random Effects**, every variance and covariance is estimated, so the table can become large when many random-effect columns are included.

---

## 17. R covariance and R correlation matrices

The **Estimated R covariance matrix** shows the residual covariance matrix after the fixed and random effects have been included. The **Estimated R correlation matrix** shows the corresponding residual correlations.

Use these tables to check:

- residual variance by visit or order level;
- residual correlation decay for AR(1)-style structures;
- lag-specific residual correlations for Toeplitz structures;
- whether heterogeneous structures estimate plausible visit-specific residual variances.

The marginal covariance of an observed response vector is not just the R matrix when random effects are present. It is:

\[
V_i = Z_iGZ_i^\top + R_i.
\]

So R-side matrices should be interpreted as residual covariance conditional on the modeled random-effect structure, not as the full within-subject covariance by themselves.

---

## 18. BLUPs / random effects

When selected, the **BLUPs / random effects** table reports subject- or cluster-specific conditional random-effect predictions.

Typical columns include:

| Column | Meaning |
|---|---|
| Subject | Subject or cluster label. |
| Random-effect columns | Predicted random intercept, random slope, or other random-effect columns. |

These predictions are empirical Bayes estimates. They are useful for diagnostics, plotting, and identifying unusual subjects or clusters. They are not additional fixed-effect coefficients and should not be interpreted as independently estimated subject-specific parameters.

!!! caution "Random-effect output can be large"
    The table has one row per subject or cluster and one column per random effect. For large datasets or rich random-effect designs, this output can be much larger than the fixed-effect summary.

---

## 19. Fit statistics

The **Fit statistics** table summarizes the likelihood fit and model size.

Common rows include:

| Row | Meaning |
|---|---|
| Fit method | `ML` or `REML`. |
| N observations | Retained analysis rows. |
| Subjects | Number of retained subject/cluster blocks. |
| Execution time | Time reported by the fit run. Treat as an example run, not a formal benchmark. |
| Fixed-effect parameters | Number of fixed-effect coefficients. |
| Random-effect columns | Number of random-effect design columns. |
| Objective | Optimized objective value. |
| Log-likelihood | Fitted log-likelihood or restricted log-likelihood according to the fit method. |
| AIC / BIC | Information criteria using the reported likelihood and parameter-count convention. |
| REML criterion | Restricted-likelihood criterion for REML fits. |
| Q form | Weighted residual quadratic form. |
| log determinant terms | Components of the likelihood calculation. |
| Profile scale Q/df | Profile scale diagnostic. |

!!! warning "Compare likelihoods carefully"
    REML likelihoods should not be used to compare models with different fixed-effect designs. Use ML for fixed-effect model comparisons. For covariance-structure comparisons, keep the response, retained rows, fixed-effect design, and major model specification aligned.

---

## 20. Convergence table

The **Convergence** table records how the optimizer stopped.

Important rows include:

| Row | Meaning |
|---|---|
| Converged | Whether the optimizer met its stopping criteria. |
| Cancelled / Interrupted | Whether the fit was stopped by the user. |
| Message | Main convergence or stopping message. |
| Iterations | Number of optimizer iterations. |
| Gradient norm | Size of the gradient at the final solution. |
| Requested maximum iterations | Maximum iterations requested in the dialog. |
| Gradient / step / objective-change tolerance | Tolerances used by the optimizer. |
| Covariance optimizer mode | Covariance optimizer path used. |
| Covariance gradient mode | Derivative mode used. |
| Diagnostic flags | Internal derivative, cache, or KR-factorization settings where applicable. |

A converged fit should still be reviewed for boundary estimates, implausible correlations, over-parameterized covariance structures, and sensitivity to simpler models.

---

## 21. Trace output

When trace output is enabled, an **LMM Trace** sheet may be written. Trace output is intended for diagnostics rather than reporting.

Use trace output when:

- the model is slow to fit;
- convergence fails or stops at the maximum iteration count;
- the objective changes erratically;
- a new covariance structure is being validated;
- results need to be compared with another implementation at a numerical level.

For routine analyses, trace output can usually remain off.

---

## 22. Example dataset and result workbooks

The supplied LMM examples use:

- [`Example dataset.csv`](../../assets/data/200lmm/200lmm.csv),

| Model | Build-model screenshot | Options screenshot | Result workbook |
|---:|---|---|---|
| 1 | [`200lmm_buildmodel.png`](../../assets/images/200lmm/200lmm_buildmodel.png) | [`200lmm_options.png`](../../assets/images/200lmm/200lmm_options.png) | [`200lmm_result_model1.xlsx`](../../assets/data/200lmm/200lmm_result_model1.xlsx) |
| 2 | [`200lmm_buildmodel2.png`](../../assets/images/200lmm/200lmm_buildmodel2.png) | [`200lmm_options2.png`](../../assets/images/200lmm/200lmm_options2.png) | [`200lmm_result_model2.xlsx`](../../assets/data/200lmm/200lmm_result_model2.xlsx) |
| 3 | [`200lmm_buildmodel3.png`](../../assets/images/200lmm/200lmm_buildmodel3.png) | [`200lmm_options3.png`](../../assets/images/200lmm/200lmm_options3.png) | [`200lmm_result_model3.xlsx`](../../assets/data/200lmm/200lmm_result_model3.xlsx) |
| 4 | [`200lmm_buildmodel4.png`](../../assets/images/200lmm/200lmm_buildmodel4.png) | [`200lmm_options4.png`](../../assets/images/200lmm/200lmm_options4.png) | [`200lmm_result_model4.xlsx`](../../assets/data/200lmm/200lmm_result_model4.xlsx) |
| 5 | [`200lmm_buildmodel5.png`](../../assets/images/200lmm/200lmm_buildmodel5.png) | [`200lmm_options5.png`](../../assets/images/200lmm/200lmm_options5.png) | [`200lmm_result_model5.xlsx`](../../assets/data/200lmm/200lmm_result_model5.xlsx) |
| 6 | [`200lmm_buildmodel6.png`](../../assets/images/200lmm/200lmm_buildmodel6.png) | [`200lmm_options6.png`](../../assets/images/200lmm/200lmm_options6.png) | [`200lmm_result_model6.xlsx`](../../assets/data/200lmm/200lmm_result_model6.xlsx) |
| 7 | [`200lmm_buildmodel7.png`](../../assets/images/200lmm/200lmm_buildmodel7.png) | [`200lmm_options7.png`](../../assets/images/200lmm/200lmm_options7.png) | [`200lmm_result_model7.xlsx`](../../assets/data/200lmm/200lmm_result_model7.xlsx) |


The [Examples and interpretation](examples.md) page presents the seven saved workflows. It includes the screenshots, result-workbook links, selected output values, and interpretation notes.

The examples are useful for understanding how the options on this page appear in real output:

| Example focus | Where to look |
|---|---|
| Random intercept only | Model 1 |
| Random intercept plus random slope | Models 2 and 3 |
| Multiple independent random effects | Model 4 |
| Unstructured G-side covariance | Model 5 |
| R-side residual correlation | Models 6 and 7 |
| Heterogeneous Toeplitz residual covariance with R matrices | Model 7 |

---

## 23. Troubleshooting guide

| Symptom | Common cause | Suggested action |
|---|---|---|
| Fit does not converge | Too many covariance parameters, poor scaling, or sparse random-effect information. | Simplify G/R covariance, center/scale continuous predictors, increase iterations, or start with VC/Diag. |
| Very large standard errors | Weak information, collinearity, or over-parameterized fixed/random effects. | Check the design matrix and remove unsupported terms. |
| Correlations near -1 or 1 | Boundary-like covariance estimate. | Try VC/Diag, CS/CSH, or a simpler random-effect design. |
| Near-zero variance component | The corresponding random effect may not be supported. | Refit without that random effect and compare diagnostics. |
| R-side structure requires visit/order | The selected residual covariance is visit-indexed. | Select a numeric visit/time/order variable. |
| Output tables are larger than expected | Rich random-effect design or BLUP output selected. | Clear optional random-effect output or simplify the random-effect design. |
| REML AIC appears to favor a model with different fixed effects | REML likelihoods are not comparable across fixed-effect designs. | Refit with ML for fixed-effect model comparison. |

---

## 24. Reporting checklist

For a final analysis report, record:

- response variable;
- subject/cluster ID and whether it is text or numeric;
- retained row count and subject/cluster count;
- fixed-effect specification;
- random-effect specification;
- G-side covariance structure;
- R-side covariance structure and visit/order variable, if used;
- ML or REML;
- inference method and alpha level;
- convergence status and iteration count;
- key fixed-effect estimates or Type III tests;
- relevant G/R covariance or correlation summaries;
- whether random-effect predictions, fitted values, and residuals were exported.

For worked interpretation examples, see [Examples and interpretation](examples.md). For worksheet-function extraction of the same output tables, see [Worksheet functions](worksheet-functions.md).
