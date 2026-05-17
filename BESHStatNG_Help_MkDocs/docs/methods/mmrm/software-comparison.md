# MMRM comparison with other software

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** help users understand how BESH Stat NG MMRM results should compare with established MMRM software, why small discrepancies can occur, and how to investigate differences. The emphasis is on **result agreement, performance, convergence robustness, and validation logic**, not on translating every syntax detail between programs.

For the model definition, see [Model and mathematics](model-and-mathematics.md). For BESH Stat NG options and result tables, see [Options and output reference](options-and-output.md). For LS-means, contrasts, and custom estimates, see [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md).

---

## 1. Main message

BESH Stat NG MMRM fits the same broad marginal repeated-measures model family used by dedicated MMRM software:

\[
y_i = X_i\beta + \varepsilon_i,
\qquad
\varepsilon_i \sim N\{0, R_i(\theta)\}.
\]

The comparison target is not the software name. The comparison target is the **estimand and fitted model**:

- the same retained rows;
- the same response, subject, and visit variables;
- the same fixed effects and interactions;
- the same factor ordering and reference levels;
- the same residual covariance structure;
- the same REML or ML likelihood;
- the same denominator degrees-of-freedom method;
- the same LS-mean grid, weights, and contrast direction.

When these are aligned, BESH Stat NG should usually agree closely with R `mmrm` and with SAS mixed-model procedures for core likelihood, covariance, fixed-effect, and linear-estimate results. Exact bit-for-bit equality is not expected because implementations can differ in optimization tolerances, covariance-parameter transformations, starting values, rank handling, small-sample approximations, and printed rounding.

!!! important "Validate results, not syntax"
    SAS, R, and BESH Stat NG use different user interfaces and model-specification syntax. A syntax translation that looks similar can still request a different estimand. Compare retained data, covariance matrices, fixed-effect estimates, LS-mean definitions, and convergence status before interpreting p-value differences.

---

## 2. Reference comparison: BESH Stat NG versus R `mmrm` on the FEV1 example

The documentation example uses the FEV1 dataset distributed with the OpenPharma `mmrm` package. The corresponding OpenPharma between-within vignette fits:

- 537 non-missing observations;
- 197 subjects;
- 4 scheduled visits;
- REML estimation;
- an unstructured within-subject covariance matrix with 10 covariance parameters;
- between-within denominator degrees of freedom.

The BESH Stat NG result workbook [037mmrm_fev_results.xlsx](../../assets/data/037mmrm/037mmrm_fev_results.xlsx) fits the same model specification used throughout this documentation.

### 2.1 Fit statistics

| Quantity | BESH Stat NG workbook | R `mmrm` vignette | Interpretation |
|---|---:|---:|---|
| Observations used | 537 | 537 | Matches. |
| Subjects used | 197 | 197 | Matches. |
| Covariance structure | Unstructured | Unstructured | Matches. |
| Covariance parameters | 10 | 10 | Matches. |
| Log-likelihood | -1693.225 | -1693.2 | Matches to printed precision. |
| Objective / deviance | 3386.450 | 3386.4 | Matches to printed precision; this is \(-2\ell\). |
| AIC | 3406.450 | 3406.4 | Matches to printed precision. |
| BIC | 3449.310 | 3439.3 | Different BIC penalty convention; see below. |

The BIC difference is not a model-fit disagreement. The BESH Stat NG workbook uses the number of retained observations for the BIC penalty in this output:

\[
3386.4499 + 10\log(537) = 3449.3099.
\]

The OpenPharma printed BIC is consistent with using the number of subjects as the BIC sample-size term for the same 10-parameter covariance model:

\[
3386.4499 + 10\log(197) = 3439.2797.
\]

Therefore, likelihood and AIC should be compared directly here, while BIC should be compared only after confirming the sample-size convention used by each program.

!!! note "Information criteria are conventions as well as calculations"
    AIC is usually straightforward once the likelihood and parameter count are aligned. BIC can differ because repeated-measures software may choose different effective sample-size conventions, such as number of observations or number of independent subjects.

### 2.2 Fixed-effect estimates

The following table compares BESH Stat NG estimates with the estimates printed in the R `mmrm` between-within vignette. R values are shown at the printed precision available in the vignette, so very small differences should be interpreted as agreement within rounding.

| Term | BESH Stat NG estimate | R `mmrm` estimate | Difference |
|---|---:|---:|---:|
| Intercept | 30.77740 | 30.77748 | -0.00008 |
| Race: Black or African American | 1.53059 | 1.53050 | 0.00009 |
| Race: White | 5.64357 | 5.64357 | -0.00000 |
| Sex: Female | 0.32603 | 0.32606 | -0.00003 |
| Treatment: TRT | 3.77443 | 3.77423 | 0.00020 |
| Visit 2 | 4.83961 | 4.83959 | 0.00002 |
| Visit 3 | 10.34217 | 10.34211 | 0.00006 |
| Visit 4 | 15.05378 | 15.05390 | -0.00012 |
| TRT × Visit 2 | -0.04211 | -0.04193 | -0.00018 |
| TRT × Visit 3 | -0.69382 | -0.69369 | -0.00013 |
| TRT × Visit 4 | 0.62410 | 0.62423 | -0.00013 |

The agreement is the expected pattern for a correctly aligned comparison: the coefficients are the same to the displayed precision, with only tiny differences caused by printing and optimization details.

### 2.3 Denominator degrees of freedom

The R `mmrm` between-within vignette reports between-subject degrees of freedom of 192 for between-subject terms and within-subject degrees of freedom of 334 for visit-related terms. The BESH Stat NG workbook reports the same pattern for this example:

| Effect type | Example terms | BESH Stat NG DF | R `mmrm` DF | Expected result |
|---|---|---:|---:|---|
| Between-subject | race, sex, treatment | 192 | 192 | Matches. |
| Within-subject | visit, treatment × visit | 334 | 334 | Matches. |

This is an important validation point because estimates can match while denominator degrees of freedom and p-values differ. Always compare DF before comparing p-values.

### 2.4 Fitted covariance matrix

The fitted unstructured covariance matrix also agrees to the displayed precision.

| Visit pair | BESH Stat NG covariance | R `mmrm` covariance | Difference |
|---|---:|---:|---:|
| Visit 1, Visit 1 | 40.5545 | 40.5537 | 0.0008 |
| Visit 1, Visit 2 | 14.3961 | 14.3960 | 0.0001 |
| Visit 1, Visit 3 | 4.9761 | 4.9747 | 0.0014 |
| Visit 1, Visit 4 | 13.3768 | 13.3867 | -0.0099 |
| Visit 2, Visit 2 | 26.5715 | 26.5715 | 0.0000 |
| Visit 2, Visit 3 | 2.7836 | 2.7855 | -0.0019 |
| Visit 2, Visit 4 | 7.4772 | 7.4745 | 0.0027 |
| Visit 3, Visit 3 | 14.8979 | 14.8979 | 0.0000 |
| Visit 3, Visit 4 | 0.9033 | 0.9082 | -0.0049 |
| Visit 4, Visit 4 | 95.5561 | 95.5568 | -0.0007 |

These are small absolute differences relative to the covariance scale and are consistent with independent numerical implementations reaching the same likelihood optimum.

---

## 3. Where discrepancies are expected

Even with a correct implementation, some output sections are more likely to differ across software than others.

| Output area | Expected agreement when settings match | Common discrepancy source | Practical action |
|---|---|---|---|
| Retained rows and subjects | Should match exactly. | Different missing-value filtering or visit coding. | Reconcile this before comparing any estimates. |
| Fixed-effect estimates | Should match very closely. | Factor coding, reference level, row order, rank handling, or a genuinely different model. | Compare model matrix columns conceptually, not only labels. |
| Covariance matrix | Should match closely for the same structure. | Different covariance parameterization, visit order, boundary treatment, or optimizer tolerance. | Compare the displayed covariance/correlation matrix, not only parameter names. |
| Log-likelihood / objective | Should match closely. | ML versus REML mismatch; different rows; different covariance structure. | Check REML/ML and retained observations first. |
| AIC | Should match if log-likelihood and parameter count match. | Different parameter-count convention. | Confirm which parameters are counted. |
| BIC | May differ even when the fit is the same. | Different effective sample-size convention. | State whether BIC uses observations, subjects, or another convention. |
| Standard errors | Should usually match closely. | Fixed-effect covariance adjustment, derivative strategy, or numerical Hessian differences. | Check selected inference method and covariance-adjustment method. |
| Denominator DF | Can differ materially. | Different between-within, Satterthwaite, or Kenward-Roger implementation. | Compare DF explicitly before comparing p-values. |
| p-values and confidence intervals | Should match when estimate, SE, DF, and tail convention match. | Any difference in estimate, SE, DF, direction, or multiplicity adjustment. | Decompose the result into estimate, SE, DF, and contrast direction. |
| LS-means | Can differ even when the fitted model matches. | Observed design grid versus prespecified reference grid; weights; covariate settings. | Document the grid and weights used. |
| Custom contrasts | Should match if the same linear function is used. | Sign convention, omitted cells, or different coefficient ordering. | Save and document the contrast weights. |

---

## 4. BESH Stat NG compared with R `mmrm`

R `mmrm` is the strongest open-source technical comparator for BESH Stat NG MMRM because it is also dedicated to marginal repeated-measures modeling. In the OpenPharma benchmarking article, `mmrm` is compared with SAS `PROC GLIMMIX`, `nlme::gls`, `lme4::lmer`, and `glmmTMB` on FEV and BCVA examples with unstructured repeated-measures covariance.

### 4.1 Result agreement

For validation, R `mmrm` is usually expected to agree well with BESH Stat NG on:

- fixed-effect estimates;
- covariance and correlation matrices;
- log-likelihood and AIC when the same likelihood and parameter count are used;
- large-sample, between-within, Satterthwaite, and Kenward-Roger-style linear estimates when the same method and grid are used;
- fitted values and residuals when the same rows and design matrix are retained.

The FEV1 comparison above illustrates this expected behavior: fixed-effect estimates, degrees of freedom, covariance estimates, log-likelihood, and AIC align closely. The notable discrepancy is BIC, which is explained by the sample-size term used in the BIC penalty rather than by a different model fit.

### 4.2 Performance expectations

The OpenPharma benchmark reports very fast `mmrm` convergence relative to other R implementations. For the FEV example, median convergence time was about 56 ms for R `mmrm`, compared with about 100 ms for SAS `PROC GLIMMIX`, 247 ms for `lme4::lmer`, 688 ms for `nlme::gls`, and 716 ms for `glmmTMB`. For the larger BCVA example, `mmrm` again had the fastest reported median convergence time, about 3.36 seconds, versus 18.65 seconds for `glmmTMB`, 36.25 seconds for SAS `PROC GLIMMIX`, and roughly 164–165 seconds for `gls` and `lmer` in that benchmark.

BESH Stat NG should not be benchmarked against those numbers by simply comparing a single Excel workbook run. The supplied BESH Stat NG FEV workbook reports an execution time of 0.096 seconds for this fit, but that value is run-specific and depends on hardware, Excel state, workbook output, add-in loading, and requested result sections. It is useful as a local diagnostic, not as a cross-platform benchmark.

### 4.3 Convergence robustness

The OpenPharma benchmark reports that all tested methods converged on the FEV example with default optimization arguments, and that `mmrm`, `gls`, and SAS `PROC GLIMMIX` were comparatively resilient in simulated missing-data scenarios, with convergence problems mainly appearing in the highest-missingness scenarios. `glmmTMB` had more convergence issues in those simulations.

For BESH Stat NG, the practical expectation is:

- FEV-like models with a modest number of visits and adequate data support should fit quickly and robustly.
- Unstructured covariance with many visits, sparse cells, or heavy dropout is more likely to be difficult.
- Kenward-Roger inference adds computational cost after the covariance model is fitted.
- A simpler covariance structure can be useful as a convergence diagnostic, but it should not replace a protocol-specified unstructured model without statistical justification.

---

## 5. BESH Stat NG compared with SAS mixed-model procedures

SAS remains a common regulatory and validation comparator for MMRM. For BESH Stat NG MMRM, the closest SAS comparison is usually a marginal repeated-measures model fitted with a repeated-measures covariance structure, not a subject-specific random-intercept/random-slope model.

### 5.1 Expected result agreement

When the same retained rows, fixed effects, covariance structure, likelihood, and inference method are aligned, SAS and BESH Stat NG should generally produce similar estimates and covariance matrices. However, SAS has many long-established options, and some defaults or denominator-DF rules differ from R-style MMRM workflows.

Important comparison points include:

- SAS `CLASS` ordering can change coefficient labels and contrast signs.
- The repeated-measures covariance structure must match the BESH Stat NG covariance structure, including homogeneous versus heterogeneous versions.
- REML and ML must be matched before comparing likelihood-based statistics.
- The LS-mean target must be matched: reference grid, covariate settings, weights, and contrast direction.
- Denominator degrees of freedom must be compared explicitly.

### 5.2 Between-within differences

A common source of BESH-versus-SAS discrepancy is between-within degrees of freedom. In the R `mmrm` FEV example, between-subject effects receive 192 DF and within-subject visit-related effects receive 334 DF. The OpenPharma between-within vignette notes that SAS handles some between-within cases differently, including the unstructured covariance case in which SAS assigns all effects to between-subject degrees of freedom.

This means that estimates and standard errors can agree while p-values differ because the t distribution uses different denominator DF. In that situation, the discrepancy is not necessarily a fitting error; it reflects a different small-sample rule.

### 5.3 Kenward-Roger and Satterthwaite differences

Satterthwaite and Kenward-Roger methods approximate the sampling distribution of fixed-effect tests by propagating uncertainty in the covariance-parameter estimates. Different software may use different derivative calculations, finite-sample corrections, or numerical safeguards. Therefore, small differences in DF, adjusted standard errors, and p-values are possible even when the fitted covariance matrix is nearly identical.

For validation reports, decompose any difference into:

1. estimate;
2. standard error;
3. denominator DF;
4. test statistic;
5. p-value;
6. confidence interval.

This usually reveals whether the disagreement is due to model fit, covariance adjustment, or only the denominator-DF approximation.

---

## 6. BESH Stat NG compared with `nlme`, `lme4`, `glmmTMB`, and `emmeans`

### 6.1 `nlme::gls`

`nlme::gls` can be a useful marginal-model comparator because it can represent residual correlation and heterogeneous variances. It is often closer to an MMRM than random-effects-only software. However, MMRM post-estimation details such as denominator DF, LS-means, and small-sample adjustments are not automatically the same as BESH Stat NG or R `mmrm`.

Use `gls` mainly for sensitivity checks unless the covariance structure, likelihood, and post-estimation method have been carefully aligned.

### 6.2 `lme4::lmer`

`lme4::lmer` is excellent for random-effects models, but it is not a direct MMRM substitute. Random intercepts and random slopes induce a covariance pattern through subject-specific effects. MMRM instead specifies the marginal within-subject residual covariance directly. A random-slope model can be scientifically useful, but it generally answers a different modeling question and should not be expected to reproduce unstructured MMRM results.

### 6.3 `glmmTMB`

`glmmTMB` can fit some repeated-measures covariance structures, but the OpenPharma benchmark reported less favorable convergence behavior in some scenarios, especially compared with R `mmrm`, SAS, and `gls` under missingness simulations. It can be valuable for broader model families, but it should be validated carefully when used as an MMRM comparator.

### 6.4 `emmeans`

`emmeans` is a post-estimation tool, not a model-fitting engine. It is useful for LS-means and contrasts after a model has been fitted in R, but it can differ from BESH Stat NG if the reference grid, weights, covariate settings, or contrast direction differ.

For custom contrasts in BESH Stat NG, use the worksheet-function workflow described in [Worksheet functions](worksheet-functions.md), especially `BESH.REGR.MMRM_LSMESTIMATE`.

---

## 7. Performance and memory requirements

### 7.1 What drives runtime

MMRM runtime is driven mainly by:

1. the number of subjects;
2. the number of retained observations;
3. the maximum number of visits per subject;
4. the number of fixed-effect columns;
5. the number of covariance parameters;
6. the selected covariance structure;
7. the selected inference method;
8. the amount of post-estimation output requested.

For an unstructured covariance with \(T\) scheduled visits, the number of covariance parameters is:

\[
q = \frac{T(T+1)}{2}.
\]

This grows quadratically with the number of visits. Four visits require 10 covariance parameters; ten visits require 55. That increase affects optimization, derivative calculations, memory use, and convergence robustness.

### 7.2 Relative cost by output and inference option

| Option or output | Runtime / memory impact | Practical guidance |
|---|---|---|
| Identity, compound symmetry, AR(1) | Lower | Useful for diagnostics and sensitivity analyses; may be too restrictive for the primary analysis. |
| Heterogeneous CS / AR(1) | Moderate | Useful when visit variances differ but correlation can be summarized simply. |
| Toeplitz / heterogeneous Toeplitz | Moderate to high | Useful when same-lag correlations are plausible but AR(1) is too restrictive; cost grows with the number of visits. |
| Unstructured covariance | Highest covariance-model cost | Appropriate for modest visit counts with adequate data support; risky with many visits and sparse data. |
| Large-sample, residual, between-within DF | Lower inference overhead | Useful for fast checks and planned analyses where appropriate. |
| Satterthwaite | Moderate to high | Requires covariance-parameter uncertainty calculations. |
| Kenward-Roger | Highest among current inference options | Adds fixed-effect covariance adjustment and denominator-DF calculations. |
| LS-means and standard contrasts | Usually moderate | Output time grows with the number of requested profiles and contrasts. |
| Custom linear estimates | Depends on number of rows in the contrast table | Keep protocol-specific contrast tables clear and focused. |
| Residual/fitted output | Can be large | Useful for diagnostics; can noticeably increase workbook size for large datasets. |
| Trace/convergence details | Small to moderate | Enable for troubleshooting, not every routine fit. |

### 7.3 Excel-specific considerations

BESH Stat NG is designed for interactive Excel-scale analysis. In typical clinical-trial-style MMRM datasets with a modest number of visits, it should usually be fast enough for exploratory and reporting workflows. Very large simulation studies, bootstrap-like repeated fitting, or high-dimensional visit schedules are better suited to scripted R or SAS pipelines.

Memory pressure increases when:

- the workbook contains many large sheets;
- residuals, fitted values, covariance matrices, and large LS-mean grids are all requested;
- many workbooks are open in Excel;
- the model has many visits and uses unstructured covariance;
- Kenward-Roger inference is requested for a large model.

For larger analyses, 64-bit Excel is strongly preferred. Close unnecessary workbooks, reduce optional output, and validate the model with simpler covariance structures before running the final specification.

---

## 8. Convergence robustness and troubleshooting comparisons

### 8.1 When convergence differences are expected

Different software may converge differently because of:

- different covariance-parameter transformations;
- different optimizer algorithms;
- different starting values;
- different positive-definiteness safeguards;
- different stopping tolerances;
- different treatment of boundary covariance estimates;
- different handling of sparse subject-by-visit patterns.

If BESH Stat NG converges but another package does not, or vice versa, first compare the retained rows, visit ordering, and covariance structure. Then review the fitted covariance matrix for near-zero variances, extremely high correlations, or near-singular behavior.

### 8.2 Practical sequence for difficult models

Use this sequence when a model is slow, unstable, or difficult to reproduce externally:

1. Fit the same fixed-effect model with a simpler covariance structure.
2. Confirm that the retained observations and subject counts match.
3. Confirm that visit order and factor reference levels match.
4. Fit the intended covariance structure without optional large output.
5. Review the estimated covariance and correlation matrices.
6. Add the planned inference method.
7. Add LS-means, contrasts, and custom estimates only after the core fit is stable.

Do not interpret differences in p-values until estimates, covariance, convergence status, and denominator degrees of freedom have been reconciled.

---

## 9. Validation checklist

Use this checklist before concluding that two software outputs disagree.

| Step | Check | Why it matters |
|---|---|---|
| 1 | Same input rows retained? | Missing-value filtering is the most common source of mismatch. |
| 2 | Same subject count? | MMRM likelihood is built from subject-level response blocks. |
| 3 | Same visit order? | Covariance parameters are attached to visit positions. |
| 4 | Same response scale? | Transformations or change scores define a different model. |
| 5 | Same fixed effects and interactions? | The design matrix determines estimates and LS-means. |
| 6 | Same categorical levels and references? | Coefficient labels and contrast signs can change. |
| 7 | Same covariance structure? | Similar labels may still imply different assumptions. |
| 8 | Same REML/ML setting? | Likelihood and covariance estimates can change. |
| 9 | Same convergence status? | A non-converged comparison is not meaningful. |
| 10 | Same denominator-DF method? | p-values can change even when estimates match. |
| 11 | Same LS-mean grid and weights? | Observed-grid and reference-grid means answer different questions. |
| 12 | Same contrast direction? | Treatment minus placebo and placebo minus treatment differ only by sign, but the interpretation changes. |
| 13 | Same multiplicity adjustment? | Adjusted and unadjusted p-values should not be compared directly. |
| 14 | Same information-criterion convention? | BIC can use different effective sample-size terms. |
| 15 | Same rounding? | Printed precision can hide very small numerical agreement. |

---

## 10. Interpreting differences

### 10.1 Differences that are usually acceptable

Small differences are usually acceptable when they are explained by:

- printed precision;
- optimizer tolerance;
- covariance-parameter transformation;
- starting values;
- BIC sample-size convention;
- small-sample DF approximation details;
- LS-mean grid definition;
- factor-label differences.

### 10.2 Differences that require investigation

Investigate more deeply when:

- the retained row count differs;
- the subject count differs;
- the covariance matrix has a different scale or pattern;
- a model converges in one program but not another;
- estimated variances are near zero or correlations are near ±1;
- treatment-difference signs are reversed unexpectedly;
- one model reports an estimability or rank warning;
- estimates differ materially, not just p-values;
- p-values differ because denominator DF differ and the analysis plan requires a specific DF method.

### 10.3 Suggested validation wording

> The BESH Stat NG MMRM fit was compared with external MMRM software using the same retained analysis rows, subject and visit variables, fixed-effect specification, covariance structure, REML/ML setting, and denominator degrees-of-freedom method. Fixed-effect estimates, covariance estimates, likelihood quantities, and planned linear estimates were numerically close. Remaining differences were attributable to documented software conventions such as BIC sample-size definition, denominator-DF approximation, optimizer tolerance, and LS-mean grid specification.

Adjust this wording to the actual validation results. Do not claim agreement for settings that were not tested.

---

## 11. External documentation

Useful external references for result comparisons and benchmarking:

- [OpenPharma `mmrm` between-within vignette](https://openpharma.github.io/mmrm/latest-tag/articles/between_within.html)
- [OpenPharma `mmrm` comparison and benchmarking vignette](https://openpharma.github.io/mmrm/latest-tag/articles/mmrm_review_methods.html#benchmarking)
- [OpenPharma `mmrm` model fitting algorithm](https://openpharma.github.io/mmrm/latest-tag/articles/algorithm.html)
- [OpenPharma `mmrm` package documentation](https://openpharma.github.io/mmrm/)
- [SAS `PROC MIXED` documentation](https://documentation.sas.com/doc/en/statug/latest/statug_mixed_toc.htm)
- [R `emmeans` package documentation](https://cran.r-project.org/package=emmeans)
- [R `nlme` package documentation](https://cran.r-project.org/package=nlme)
- [R `lme4` package documentation](https://cran.r-project.org/package=lme4)

## See also

- [MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)
- [Concepts and use cases](concepts-and-use-cases.md)
- [Model and mathematics](model-and-mathematics.md)
- [Options and output reference](options-and-output.md)
- [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md)
- [Worksheet functions](worksheet-functions.md)
- [Implementation details](implementation-details.md)
- [Examples and interpretation](examples.md)
