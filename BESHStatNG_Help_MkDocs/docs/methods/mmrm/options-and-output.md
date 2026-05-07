# MMRM options and output reference

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** provide a practical, option-by-option reference for the BESH Stat NG **Mixed Models for Repeated Measures (MMRM)** dialog and explain the result tables written to the output workbook. For the formal model, likelihood, covariance-structure formulas, and degrees-of-freedom mathematics, see [Model and mathematics](model-and-mathematics.md). For the ribbon workflow, see [User interface](user-interface.md). For a focused discussion of estimands, see [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 1. Where the options are used

The MMRM dialog separates the analysis into three decisions:

1. **Model specification** — how the repeated-measures model is fitted: fit method, residual covariance structure, inference method, alpha level, residual output, and optimizer settings.
2. **Post-estimation output** — which estimated marginal means, group contrasts, change-from-baseline tables, and difference-in-change tables are appended to the model output.
3. **Numerical diagnostics** — how much optimizer and diagnostic information is written for review or troubleshooting.

The result workbook created by the ribbon workflow contains at least:

- a **Data** sheet with the retained analysis rows, fixed-effect design matrix, subject/visit identifiers, fitted marginal values, and optionally residuals,
- an **MMRM** sheet with model estimates, post-estimation tables, covariance output, fit statistics, and convergence information,
- optionally an **MMRM Trace** sheet when trace or detailed iteration output is requested.

!!! tip "Practical default"
    For a standard clinical-trial-style continuous repeated-measures analysis, start with **REML**, **Kenward-Roger**, **Unstructured** residual covariance, **Observed design grid** LS-means, and treatment-by-visit group contrasts. Then simplify or modify the settings only when the design, sample size, convergence behavior, or analysis plan requires it.

---

## 2. Default settings

| Option area | Option | Default / common initial value | Practical meaning |
|---|---|---|---|
| Model fit | **Fit Method** | `REML` | Restricted maximum likelihood; usually preferred for covariance estimation and fixed-effect inference. |
| Model fit | **Inference Method** | `Kenward-Roger` | Small-sample fixed-effect inference with adjusted covariance and denominator degrees of freedom. |
| Model fit | **Residual Covariance Structure** | `Unstructured` | Most flexible visit-level covariance pattern when the data support it. |
| Model fit | **Compute Residuals** | Off unless selected | Adds raw marginal residuals to the Data sheet. |
| Model fit | **alpha** | Global default, usually `0.050` | Controls two-sided confidence intervals; 0.05 gives 95% CIs. |
| Post-estimation | **Show class-level information** | On | Reports subject ID levels and, when available, grouping-factor levels. |
| Post-estimation | **Show LS-means / estimated marginal means** | On | Adds estimated marginal mean tables. |
| Post-estimation | **Compare groups by** | `(Auto)` | Attempts to choose a suitable grouping factor, usually treatment. |
| Post-estimation | **Comparison type** | `Pairwise among group levels` | Compares all pairs of group levels within each visit. |
| Post-estimation | **Difference direction** | `Higher level - lower level` | Controls the sign of reported group contrasts. |
| Post-estimation | **Baseline visit for change** | `(Smallest)` | Uses the smallest observed numeric visit as baseline. |
| Post-estimation | **Show change from baseline** | On | Adds visit-minus-baseline tables. |
| Post-estimation | **Show group difference in change** | On | Adds difference-in-change tables when a grouping factor is available. |
| LS-means | **LS-means mode** | `Observed design grid` | Averages fitted design rows from the retained analysis data. |
| Reference grid | **Reference-grid weighting** | `Equal class-cell weights` | Used only in reference-grid mode. |
| Reference grid | **Continuous covariates** | `Continuous covariates at observed means` | Used only in reference-grid mode. |
| Multiplicity | **Multiplicity** | `None` | No adjusted p-value unless another method is selected and the output path supports adjustment. |
| Optimizer | **Covariance optimizer mode** | `AI/Fisher scoring (default)` | Average Information / Fisher-scoring path for covariance parameters. |
| Optimizer | **Covariance gradient mode** | `Auto (analytic where available)` | Uses analytic derivatives where available, otherwise numerical fallback. |
| Optimizer | **Convergence Criterion** | `0.000001` | Applied to gradient, step, and objective-change stopping tolerances. |
| Optimizer | **Max. Iterations** | `500` | Maximum optimizer iterations before non-convergence. |

---

## 3. Example settings used in the supplied FEV1 workbook

The examples in this MMRM documentation use:

- [037mmrm_fev_data.csv](../../assets/data/037mmrm/037mmrm_fev_data.csv),
- [037mmrm_fev_results.xlsx](../../assets/data/037mmrm/037mmrm_fev_results.xlsx).

The supplied result workbook fits the FEV1 example with:

| Setting | Value in the supplied workbook |
|---|---|
| Response | `FEV1` |
| Subject ID | `SUBJID` |
| Visit/time | `VISITN` |
| Fixed effects | `RACEN + SEXN + ARMCDN + VISITN + ARMCDN:VISITN` |
| Categorical fixed effects | `RACEN`, `SEXN`, `ARMCDN`, `VISITN` |
| Fit method | `REML` |
| Inference method | `Between-within DF` |
| Residual covariance structure | `Unstructured` |
| LS-means mode | `Observed design grid` |
| Grouping factor | `ARMCDN` |
| Baseline visit | visit `1` via `(Smallest)` |
| Group contrast mode | `Pairwise among group levels` |
| Difference direction | `Higher level - lower level`, therefore `ARMCDN 2 - 1` |
| Alpha | `0.050`, giving 95% confidence intervals |
| Analysis rows | 537 non-missing FEV1 observations from 197 subjects |

!!! note "Why this page uses between-within output"
    The dialog default may be **Kenward-Roger**, but the supplied FEV1 workbook intentionally uses **Between-within DF** to reproduce the between-within example used throughout the MMRM documentation. The option explanations below apply to all inference methods.

---

## 4. Fit Method

### REML

**REML** estimates covariance parameters using restricted maximum likelihood. It adjusts the covariance-parameter likelihood for the fixed effects in the model and is the usual default for MMRM.

Use REML when:

- the main goal is fixed-effect inference, LS-means, or treatment contrasts,
- Kenward-Roger inference is requested,
- covariance parameters are not being compared across different fixed-effect mean models.

For REML fits, BESH Stat NG reports REML-based information criteria as **diagnostic quantities**. Do not use REML AIC/BIC to compare models with different fixed-effect specifications; use ML for that purpose.

### ML

**ML** estimates fixed effects and covariance parameters under the ordinary maximum-likelihood criterion.

Use ML mainly when:

- comparing different fixed-effect mean models fitted to the same response data and covariance structure,
- matching another software result that was explicitly fitted by ML,
- performing a sensitivity analysis where ML is required by the analysis plan.

!!! warning "Kenward-Roger requires REML"
    If **Kenward-Roger** is selected with **ML** in the ribbon dialog, BESH Stat NG changes the fit method to **REML** and displays an informational message. If using worksheet functions, choose REML for Kenward-Roger fits.

---

## 5. Inference Method

The inference method controls the standard error adjustment, test statistic, denominator degrees of freedom, p-values, and confidence intervals used for fixed effects and linear estimates. The mathematical details are described in [Model and mathematics](model-and-mathematics.md#12-fixed-effect-coefficient-inference).

| Inference method | Output style | When to use | Main caution |
|---|---|---|---|
| **Large-sample normal** | Wald normal / z-style inference | Large samples, quick diagnostics, or comparison with asymptotic software output. | Does not attach finite-sample denominator degrees of freedom. |
| **Residual DF** | t inference with residual denominator DF | Simple finite-DF approximation or troubleshooting. | Usually less tailored to repeated-measures covariance than the other DF methods. |
| **Between-within DF** | t inference using R `mmrm`-style between-within DF | Compatibility/sensitivity analyses and examples that require between-within denominator DF. | Approximate; does not apply Satterthwaite or Kenward-Roger covariance adjustment. |
| **Satterthwaite** | t inference with Satterthwaite denominator DF | Small-sample linear estimates when KR is not needed or is too expensive. | Uses a first-order approximation; may be sensitive to covariance-parameter uncertainty and numerical derivatives. |
| **Kenward-Roger** | adjusted covariance, denominator DF, and term-level F tests where available | Default recommendation for many MMRM analyses, especially moderate/small samples. | Requires REML and is computationally more expensive. |

### Interpreting fixed-effect p-values

The fixed-effects table uses the selected inference method. With finite-DF methods, the table includes a **DF** column and a t statistic. With large-sample normal inference, the table omits denominator DF and uses normal/Wald inference.

For practical reporting, most users should report LS-means or contrasts rather than raw fixed-effect coefficients, because coefficients depend on the coding and reference levels of the design matrix.

---

## 6. Residual Covariance Structure

The residual covariance structure defines the within-subject covariance matrix across visits. BESH Stat NG MMRM is an R-side-only marginal repeated-measures model, so this matrix is the fitted marginal within-subject covariance matrix.

| UI value | Parameters for \(T\) visits | Use when | Caution |
|---|---:|---|---|
| **Identity** | 1 | Residual variance is common and repeated observations are treated as uncorrelated. Useful as a diagnostic baseline. | Usually too restrictive for repeated measures. |
| **Diagonal Heterogeneous** | \(T\) | Variance can differ by visit but within-subject correlations are ignored. | Ignores repeated-measures correlation. |
| **Compound Symmetry** | 2 | Common variance and one common correlation are plausible. | Often too restrictive when correlations decline with visit distance. |
| **Heterogeneous CS** | \(T+1\) | Variance differs by visit but one common correlation is plausible. | Still assumes a single correlation across all visit pairs. |
| **AR(1)** | 2 | Equally spaced ordered visits with decaying correlation and common variance. | Assumes one decay parameter and common variance. |
| **Heterogeneous AR(1)** | \(T+1\) | Ordered visits with decaying correlation and visit-specific variances. | Less flexible than unstructured for non-monotone correlation patterns. |
| **Unstructured** | \(T(T+1)/2\) | Each visit variance and covariance is estimated separately. Often preferred for confirmatory MMRM when there are enough subjects. | Uses many parameters; may be unstable with sparse visits or small samples. |

### Choosing a covariance structure

Start with **Unstructured** when the number of visits is modest and the study has enough data at each visit and visit pair. Consider a simpler structure when:

- the unstructured fit does not converge,
- the estimated covariance matrix is near singular,
- some visit pairs have very little support,
- the number of visits is large relative to the number of subjects,
- the analysis plan prespecifies a simpler covariance pattern.

The **Estimated R covariance matrix** and **Estimated R correlation matrix** output tables are the easiest way to inspect whether the chosen structure is behaving reasonably.

---

## 7. Alpha and confidence intervals

The **alpha** value controls two-sided confidence intervals. With `alpha = 0.050`, the output reports 95% confidence intervals.

For a linear estimate \(L\hat\beta\), the reported confidence interval is generally:

\[
L\hat\beta \pm c_{1-\alpha/2}\,\operatorname{SE}(L\hat\beta),
\]

where the critical value \(c\) is taken from the selected inference method: a normal critical value for large-sample normal inference or a t critical value for finite-DF methods.

---

## 8. Compute Residuals

When **Compute Residuals** is selected, the Data sheet includes raw marginal residuals:

\[
e_i = y_i - \hat y_i.
\]

In the ribbon output, \(\hat y_i\) is shown in the **Fitted marginal** column. The residual is the observed response minus the marginal fitted value for the retained analysis row.

Use residuals for:

- identifying unusually large deviations,
- checking whether residual spread differs by visit or group,
- detecting obvious data issues or miscoded visits/groups,
- exporting row-level diagnostics for further plotting.

Do not interpret these residuals as subject-specific conditional residuals from a random-effects model. The user-facing MMRM release does not fit subject-specific random intercepts or random slopes.

---

## 9. Class-level information and grouping factor

### Show class-level information

When **Show class-level information** is selected, the output includes a **Class Level Information** table. In the FEV1 workbook it reports:

| Class | Levels | Meaning |
|---|---:|---|
| `SUBJID` | 197 | Number of subjects retained in the analysis. |
| `ARMCDN` | 2 | Grouping factor used for LS-means and contrasts. |

This table is useful for confirming that the expected subject count and group levels were used after missing-value screening.

### Compare groups by

The **Compare groups by** option chooses the grouping factor for group-specific LS-means, group contrasts, change-from-baseline by group, and difference-in-change tables.

| UI value | Meaning | Use when |
|---|---|---|
| `(Auto)` | BESH Stat NG attempts to choose a suitable categorical/factor-like source variable. | Quick exploratory use or simple treatment/visit designs. |
| `(None)` | No group-specific LS-means or group contrasts are generated. | You only need overall visit means or the model has no group comparison. |
| Specific variable | Uses that selected source variable as the grouping factor. | Recommended for final analyses; explicitly choose treatment, dose, or subgroup. |

!!! tip "Prefer explicit grouping in final work"
    `(Auto)` is convenient, but final documentation, validation, or regulated analysis should explicitly select the grouping factor so the contrast labels and output are unambiguous.

---

## 10. LS-means mode and reference-grid options

### Observed design grid

**Observed design grid** is the default LS-means mode. It uses the fixed-effect design rows actually retained in the analysis data.

For a visit \(t\), the estimated mean is based on:

\[
L_t = \frac{1}{|\mathcal{I}_t|}\sum_{r\in\mathcal{I}_t} x_r^\top,
\qquad
\widehat\mu_t = L_t\hat\beta.
\]

For a visit-by-group mean, the average is restricted to rows matching that visit and group.

Observed-design-grid LS-means are useful because they:

- match the retained analysis data,
- avoid creating profiles not present in the dataset,
- are easy to explain to practical users,
- work well for the first ribbon release where users need direct visit and treatment summaries.

The main caution is that the estimand can depend on the observed covariate/factor distribution in the retained rows.

### Reference grid

**Reference grid** creates target profiles first and then evaluates estimated means or contrasts on that grid. It is closer to SAS `LSMEANS` / R `emmeans`-style marginal means when the estimand should average over a specified reference population.

Reference-grid mode activates two additional options:

| Option | Values | Meaning |
|---|---|---|
| **Reference-grid weighting** | `Equal class-cell weights`, `Observed class-cell weights` | Controls whether class cells are averaged equally or according to observed frequencies. |
| **Continuous covariates** | `Continuous covariates at observed means`, `Continuous covariates at 0` | Controls how continuous covariates are fixed in reference profiles. |

Use reference-grid mode when the estimand should average equally over categorical covariate levels, when matching an external SAS/R reference-grid result, or when the analysis plan defines a specific reference population.

---

## 11. Estimated marginal mean output

When **Show LS-means / estimated marginal means** is selected, the output can include:

| Output table | Appears when | Question answered |
|---|---|---|
| **Estimated marginal means by visit** | LS-means are enabled. | What is the adjusted mean response at each visit? |
| **Estimated marginal means by visit and group** | LS-means are enabled and a grouping factor is available. | What is the adjusted mean response in each group at each visit? |
| **MMRM change from baseline by visit** | Change from baseline is enabled. | How much does the overall adjusted mean differ from baseline at each visit? |
| **MMRM change from baseline by visit and group** | Change from baseline is enabled and a grouping factor is available. | How much does each group change from baseline at each visit? |
| **MMRM group differences by visit** | A grouping factor is available and comparison type is not `None`. | How do groups differ at each visit? |
| **MMRM difference in change from baseline** | Difference in change is enabled and a grouping factor is available. | How do groups differ in change from baseline? |

The common columns are:

| Column | Meaning |
|---|---|
| `N` | Number of retained analysis rows contributing to that observed-design profile, where shown. |
| `Estimate` | Estimated marginal mean or contrast. |
| `Std. Error` | Model-based standard error, adjusted according to the selected inference method when applicable. |
| `DF` | Denominator degrees of freedom, blank for large-sample normal inference. |
| `t` or `z` | Test statistic. |
| `Pr(>\|t\|)` or `Pr(>\|z\|)` | Two-sided p-value. |
| `Lower 95% CI`, `Upper 95% CI` | Confidence interval based on the selected alpha level. |

---

## 12. Contrast options

### Comparison type

| UI value | Meaning | Example with `ARMCDN` levels 1 and 2 |
|---|---|---|
| `None` | Do not output group contrasts. | No treatment-difference table. |
| `Pairwise among group levels` | Compare every pair of observed group levels within each visit. | `ARMCDN 2 - 1` at each visit. |
| `Each group vs control` | Compare each non-control level against the selected control/reference level. | Level 2 versus selected control level 1. |
| `Selected comparison only` | Compare one selected group level against the selected control/reference level. | Only level 2 versus level 1. |

### Reference/control level and comparison level

The **Reference/control level** is used by control-based comparisons. `(First)` means the first sorted numeric level. The **Comparison level** is used only for **Selected comparison only**.

### Difference direction

| Direction | Meaning | Sign interpretation |
|---|---|---|
| `Higher level - lower level` | Higher sorted numeric group level minus lower sorted numeric group level. | With `ARMCDN` levels 1 and 2, the output is `2 - 1`. |
| `Treatment - control` | Selected treatment/comparison level minus selected control level. | Positive values mean the treatment/comparison level is higher. |
| `Control - treatment` | Selected control level minus selected treatment/comparison level. | Positive values mean the control level is higher. |

!!! warning "Always report the contrast direction"
    The same numerical estimate changes sign when the direction is reversed. In reports, write labels such as `ARMCDN 2 - 1`, `Treatment - Control`, or `Control - Treatment` rather than only saying “treatment difference.”

### Example contrast table from the FEV1 workbook

The supplied result workbook uses **Pairwise among group levels** with direction **Higher level - lower level**, so the group contrast is `ARMCDN 2 - 1` at each visit:

| Contrast | Estimate | Std. Error | DF | t | Pr(>\|t\|) | Lower 95% CI | Upper 95% CI |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Visit 1: ARMCDN 2 - 1 | 4.40899 | 1.06933 | 192 | 4.12314 | 5.56e-05 | 2.29985 | 6.51813 |
| Visit 2: ARMCDN 2 - 1 | 4.12926 | 0.853436 | 192 | 4.83839 | 2.68e-06 | 2.44594 | 5.81257 |
| Visit 3: ARMCDN 2 - 1 | 3.68099 | 0.678965 | 192 | 5.42147 | 1.76e-07 | 2.3418 | 5.02018 |
| Visit 4: ARMCDN 2 - 1 | 5.00644 | 1.67844 | 192 | 2.98279 | 0.00322688 | 1.69589 | 8.317 |

In this example, the visit-4 estimate is approximately 5.01, meaning that the adjusted mean FEV1 for `ARMCDN=2` is about 5.01 units higher than for `ARMCDN=1` at visit 4, under the observed-design-grid LS-mean definition and the selected between-within DF inference.

### When a custom contrast is needed

The ribbon contrast controls cover the most common visit-by-group comparisons. For custom contrasts, use the worksheet function:

```excel
=BESH.REGR.MMRM_LSMESTIMATE(handle, spec, alpha, at)
```

Use `BESH.REGR.MMRM_LSMESTIMATE` when you need:

- a weighted average over selected visits,
- a custom subgroup or covariate profile,
- a nonstandard difference-in-change definition,
- an average treatment effect over multiple post-baseline visits,
- a contrast involving more than one factor beyond the standard group-by-visit options.

The function is described in [Worksheet functions](worksheet-functions.md#10-custom-ls-mean-estimates-with-mmrm_lsmestimate) and in the [Regression Models UDF reference](../../udf/regression-models.md#beshregrmmrm_lsmestimate).

---

## 13. Change from baseline and difference in change

### Baseline visit for change

| UI value | Meaning |
|---|---|
| `(Smallest)` | Uses the smallest observed numeric visit value as baseline. |
| Specific visit value | Uses the selected observed visit value as baseline. |

The baseline value is selected from the retained analysis visit values. In the FEV1 workbook, `(Smallest)` resolves to visit `1`.

### Show change from baseline

For visit \(t\) and baseline visit \(0\), the overall change is:

\[
\widehat\Delta_t = \widehat\mu_t - \widehat\mu_0.
\]

With a grouping factor \(g\), group-specific change is:

\[
\widehat\Delta_{g,t} = \widehat\mu_{g,t} - \widehat\mu_{g,0}.
\]

### Show group difference in change

For two groups, such as active treatment \(A\) and control \(C\), the difference in change is:

\[
(\widehat\mu_{A,t} - \widehat\mu_{A,0})
-
(\widehat\mu_{C,t} - \widehat\mu_{C,0}).
\]

The FEV1 workbook reports the following difference-in-change contrasts:

| Contrast | Estimate | Std. Error | DF | t | Pr(>\|t\|) | Lower 95% CI | Upper 95% CI |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Visit 2 vs baseline 1: Δ(ARMCDN=2) - Δ(ARMCDN=1) | -0.279733 | 1.12924 | 192 | -0.247717 | 0.804618 | -2.50705 | 1.94758 |
| Visit 3 vs baseline 1: Δ(ARMCDN=2) - Δ(ARMCDN=1) | -0.728 | 1.18679 | 192 | -0.613419 | 0.540325 | -3.06882 | 1.61282 |
| Visit 4 vs baseline 1: Δ(ARMCDN=2) - Δ(ARMCDN=1) | 0.597453 | 1.8509 | 192 | 0.32279 | 0.747205 | -3.05326 | 4.24816 |

In this example, the difference-in-change estimates are small relative to their standard errors, and the p-values do not indicate clear evidence that the change from baseline differs between `ARMCDN=2` and `ARMCDN=1` at visits 2–4.

---

## 14. Multiplicity adjustment

The **Multiplicity** option provides:

| UI value | Meaning |
|---|---|
| `None` | Report unadjusted p-values. |
| `Bonferroni` | Multiply p-values by the number of comparisons, capped at 1. |
| `Holm` | Step-down family-wise error-rate adjustment; usually less conservative than Bonferroni. |
| `Sidak` | Sidak family-wise adjustment. |

In the current implementation, reference-grid contrast tables include both the raw p-value and an **Adjusted p** column when multiplicity adjustment is used. Confidence limits remain unadjusted. The observed-design-grid contrast tables shown in the supplied workbook report unadjusted p-values and confidence intervals.

!!! tip "Analysis-plan decision"
    Multiplicity adjustment should follow the analysis plan. Do not turn it on simply because multiple rows appear in the output; some rows may be descriptive or part of a prespecified hierarchical testing strategy handled outside the model output.

---

## 15. Optimizer and convergence options

### Covariance optimizer mode

| UI value | Intended use |
|---|---|
| `AI/Fisher scoring (default)` | Default production path for MMRM covariance estimation. |
| `Projected BFGS (auto gradient)` | Robust fallback/alternative that lets the engine choose analytic or numerical gradients. |
| `Projected BFGS (analytic gradient)` | Diagnostic or performance-oriented BFGS path using analytic scores. |
| `Projected BFGS (finite-difference gradient)` | Troubleshooting path using numerical finite differences. Useful for validation and difficult fits. |

### Covariance gradient mode

| UI value | Intended use |
|---|---|
| `Auto (analytic where available)` | Default; uses analytic derivatives where supported and falls back when necessary. |
| `Analytic score` | Forces analytic covariance scores where supported. |
| `Analytic score + finite-difference validation` | Compares analytic gradients against finite differences for diagnostics. |
| `Numerical finite difference` | Fully numerical gradient path. Slower but useful for validation. |

### Convergence Criterion

The **Convergence Criterion** is applied consistently to the covariance-optimizer stopping tolerances:

- gradient tolerance,
- step tolerance,
- objective-change tolerance.

A smaller value is stricter. If a model nearly converges but stops early, increasing **Max. Iterations** is usually safer than immediately loosening the convergence criterion. If a model is unstable, first check the data and covariance structure.

### Max. Iterations

The default is `500`. Increase this when:

- the objective is still improving,
- the gradient is decreasing,
- the model is close to convergence,
- the covariance structure is complex but scientifically justified.

Do not use very high iteration limits to force a poorly supported unstructured covariance model. If the model repeatedly fails, consider a simpler covariance structure.

### Iterations Details, Trace Execution, and Diagnostic

| Checkbox | Output effect | Use when |
|---|---|---|
| **Iterations Details** | Adds optimizer trace details to the MMRM output and may create an MMRM Trace sheet when trace text is available. | You need to review convergence behavior. |
| **Trace Execution** | Stores detailed optimizer trace text for the output trace sheet. | You are debugging or validating numerical behavior. |
| **Diagnostic** | Adds available performance, support, restart, or KR finite-difference diagnostics. | You are troubleshooting convergence, sparse visits, or KR calculations. |

Diagnostic tables may include performance timings, optimizer mode actually used, gradient-provider diagnostics, visit-pair support diagnostics, restart diagnostics, and KR finite-difference diagnostics when those components were active for the fit.

---

## 16. Output workbook: Data sheet

The **Data** sheet contains the analysis rows retained after screening and the design matrix used by the fitted model. In the FEV1 workbook, the columns are:

| Column group | Example columns | Meaning |
|---|---|---|
| Row identifier | `Row ID` | Original worksheet row ID retained in the analysis. |
| Response | `FEV1` | Observed repeated outcome used in the likelihood. |
| Fixed-effect design | `Intercept`, `RACEN[2]`, `ARMCDN[2]`, `VISITN[2]`, `ARMCDN[2]:VISITN[4]`, etc. | Expanded model-matrix columns generated from the Build Model tab. |
| Subject and visit | `SUBJID`, `VISITN` | Block identifier and visit/order variable. |
| Fitted value | `Fitted marginal` | Marginal fitted response \(x_i^\top\hat\beta\). |
| Residual | `Residual` | Raw marginal residual when **Compute Residuals** is selected. |

Use the Data sheet to confirm that the model matrix, retained rows, subject IDs, and visit values match the analysis you intended to run.

---

## 17. Output workbook: MMRM sheet

The **MMRM** sheet contains model estimates and post-estimation output. The exact tables depend on the selected options and inference method.

### Fixed effects

The **Fixed effects** table contains the fitted coefficient vector. In the FEV1 example, it includes terms such as:

- `Intercept`,
- `RACEN[2]`, `RACEN[3]`,
- `SEXN[1]`,
- `ARMCDN[2]`,
- `VISITN[2]`, `VISITN[3]`, `VISITN[4]`,
- `ARMCDN[2]:VISITN[2]`, `ARMCDN[2]:VISITN[3]`, `ARMCDN[2]:VISITN[4]`.

These are model coefficients, not necessarily the final estimands of interest. For treatment reporting, use the LS-mean and contrast tables.

### Optional Kenward-Roger term-level F tests

When **Kenward-Roger** inference is selected and the required KR workspace is available, the output can include a **Kenward-Roger term-level F tests** table. This table summarizes multi-degree-of-freedom fixed-effect terms, such as a treatment-by-visit interaction, using KR-adjusted fixed-effect covariance information.

### Covariance parameters

The **Covariance parameters** table reports optimized R-side parameters on the **internal optimizer scale**. For an unstructured residual covariance, parameters include Cholesky-scale quantities such as `log_chol_diag_1` and `chol_2_1`.

!!! important "Use the R matrix for interpretation"
    The internal covariance-parameter table is primarily for reproducibility and diagnostics. For practical interpretation, use the **Estimated R covariance matrix** and **Estimated R correlation matrix** tables.

### Estimated R covariance matrix

For MMRM/no-random-effects fits, the R matrix is the fitted marginal within-subject covariance matrix. In the supplied FEV1 workbook:

|  | Visit 1 | Visit 2 | Visit 3 | Visit 4 |
| --- | --- | --- | --- | --- |
| Visit 1 | 40.5545 | 14.3961 | 4.97608 | 13.3768 |
| Visit 2 | 14.3961 | 26.5715 | 2.78361 | 7.47718 |
| Visit 3 | 4.97608 | 2.78361 | 14.8979 | 0.903262 |
| Visit 4 | 13.3768 | 7.47718 | 0.903262 | 95.5561 |

The diagonal entries are visit-specific variances. The off-diagonal entries are fitted visit-pair covariances.

### Estimated R correlation matrix

The R correlation matrix rescales the covariance matrix to correlations:

\[
\operatorname{Corr}(j,k)=\frac{R_{jk}}{\sqrt{R_{jj}R_{kk}}}.
\]

In the supplied FEV1 workbook:

|  | Visit 1 | Visit 2 | Visit 3 | Visit 4 |
| --- | --- | --- | --- | --- |
| Visit 1 | 1 | 0.438548 | 0.202444 | 0.214884 |
| Visit 2 | 0.438548 | 1 | 0.139907 | 0.148389 |
| Visit 3 | 0.202444 | 0.139907 | 1 | 0.0239398 |
| Visit 4 | 0.214884 | 0.148389 | 0.0239398 | 1 |

For example, the fitted correlation between visits 1 and 2 is about 0.439, while the fitted correlation between visits 3 and 4 is about 0.024.

### Fit statistics

The supplied FEV1 workbook reports:

| Statistic | Value |
| --- | --- |
| Fit method | REML |
| N observations | 537 |
| Subjects | 197 |
| Execution time | 0.096 s |
| Fixed-effect parameters | 11 |
| Random-effect columns | 0 |
| Objective | 3386.45 |
| Log-likelihood | -1693.22 |
| AIC | 3406.45 |
| BIC | 3449.31 |
| REML criterion | 3386.45 |
| Q form | 526 |
| log\|V\| | 1884.76 |
| log\|X'V^-1X\| | 8.96366 |
| Profile scale Q/df | 1 |

For REML fits, information criteria are diagnostic and should not be used to compare models with different fixed-effect specifications. The `N observations` and `Subjects` rows are especially useful for confirming the analysis set after missing-value screening.

### Convergence

The convergence table reports whether the optimizer converged and records the main numerical settings used. The supplied FEV1 workbook includes:

| Convergence item | Value |
| --- | --- |
| Converged | 1 |
| Cancelled | 0 |
| Interrupted | 0 |
| Message | Converged: Average Information objective change 7.233588803501334E-07 <= tolerance 9.9999999999999995E-07. |
| Iterations | 7 |
| Gradient norm | 0.00396807 |
| Requested maximum iterations | 500 |
| Gradient tolerance | 1e-06 |
| Step tolerance | 1e-06 |
| Objective-change tolerance | 1e-06 |
| BFGS covariance optimization | 1 |
| Covariance optimizer mode | AverageInformationReml |

A successful fit should have `Converged = 1`, `Cancelled = 0`, and `Interrupted = 0`. If the model does not converge, inspect the message, gradient norm, covariance structure, visit support, and any diagnostic or trace output.

---

## 18. Optional MMRM Trace sheet

The **MMRM Trace** sheet appears when trace text is available and either **Trace Execution** or **Iterations Details** is selected. It can include iteration-level optimizer messages, objective changes, step adjustments, gradient diagnostics, and fallback information.

Use the trace sheet when:

- a model fails to converge,
- a complex covariance structure is slow or unstable,
- analytic-gradient validation is being checked,
- results need to be compared against another software implementation.

For routine reporting, the trace sheet is usually not needed.

---

## 19. Common warnings and what to do

| Message or symptom | What it means | What to do |
|---|---|---|
| Kenward-Roger requires REML | KR was selected with ML. | Use REML or choose another inference method. |
| No valid observations | Missing/non-numeric screening removed all rows. | Check response, subject, visit, and fixed-effect columns. |
| Singular or rank-deficient fixed-effect design | Some fixed-effect columns are linearly dependent or unsupported by the data. | Remove redundant terms, check factor coding, and ensure every level has support. |
| Covariance solution unusable or non-positive definite | The proposed covariance parameters do not yield a valid covariance matrix. | Simplify the covariance structure, check sparse visits, or try a different optimizer mode. |
| LS-means skipped | Required visit/group/profile rows cannot be constructed. | Check the grouping factor, visit coding, and whether selected profiles exist in the analysis data. |
| Very large standard errors | The model or covariance structure is weakly supported. | Inspect visit-pair support, consider a simpler covariance structure, and check for sparse factor combinations. |
| Converged = 0 | The optimizer stopped without satisfying convergence criteria. | Review the convergence message, increase max iterations only if progress is continuing, and consider simplifying the model. |

---

## 20. Reporting checklist

For a reproducible MMRM report, include:

- response variable,
- subject ID and visit/time variable,
- fixed-effect model, including categorical factors and interactions,
- whether visit is treated as categorical or continuous,
- residual covariance structure,
- ML or REML fit method,
- inference method,
- alpha/confidence level,
- LS-means mode: observed design grid or reference grid,
- grouping factor and contrast direction,
- baseline visit for change-from-baseline estimands,
- multiplicity adjustment, if used,
- number of observations and subjects used,
- convergence status and notable warnings,
- software/version information when results are compared across tools.

!!! tip "Best reporting practice"
    Report the exact contrast label from the output table, such as `Visit 4: ARMCDN 2 - 1`, together with the estimate, standard error, confidence interval, denominator DF, p-value, and selected inference method.
