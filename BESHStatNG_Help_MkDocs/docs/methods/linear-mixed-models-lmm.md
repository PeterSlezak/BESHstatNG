# Linear Mixed Models (LMM)

**Includes:** Gaussian linear mixed models, fixed effects, random intercepts, random slopes, multiple random effects, random-effect interactions, G-side random-effects covariance structures, optional R-side residual covariance structures, ML and REML fitting, large-sample / residual-DF / Satterthwaite / Kenward-Roger fixed-effect inference, Type III tests, covariance parameters, fitted values, residuals, subject-level random effects, covariance matrices, convergence diagnostics, and worksheet functions.  
**Purpose:** Fit likelihood-based models for continuous outcomes when observations are grouped, clustered, or repeatedly measured and the analysis needs both population-level effects and subject- or cluster-specific random variation.

---

## Overview

A **linear mixed model (LMM)** extends ordinary linear regression by adding random effects. Fixed effects describe the average relationship between the response and predictors in the population. Random effects describe how subjects, clusters, batches, sites, or other grouping units deviate from that population average.

LMMs are useful when observations are not independent. Common examples include repeated measurements within subjects, students within schools, patients within centers, samples within laboratory batches, and longitudinal outcomes where different subjects may have different baseline levels or slopes over time.

In BESH Stat NG, the LMM workflow is designed for **continuous Gaussian outcomes**. The model can include fixed effects, categorical factors, polynomials, interactions, random intercepts, random slopes, multiple random effects, random-effect interactions, a selected **G-side** covariance structure for random effects, and an optional **R-side** residual covariance structure for within-subject residual correlation.

---

## Typical model

A Gaussian LMM can be written as:

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i,
\]

where:

- \(y_i\) is the response vector for subject, cluster, or unit \(i\);
- \(X_i\beta\) is the fixed-effect part of the model;
- \(Z_i b_i\) is the random-effect part of the model;
- \(b_i\) contains subject- or cluster-specific random effects;
- \(\varepsilon_i\) contains residual errors.

The random effects have covariance matrix \(G\), and residuals have covariance matrix \(R_i\). After random effects are integrated out, the marginal covariance for unit \(i\) is:

\[
V_i = Z_iGZ_i^\top + R_i.
\]

The **G-side** structure controls the covariance among random effects such as random intercepts and random slopes. The **R-side** structure controls residual covariance after the random effects have been included.

---

## What questions can LMM answer?

LMM is most useful when the analysis needs to answer questions such as:

- What is the average effect of time, treatment, dose, or another predictor after accounting for repeated or clustered observations?
- Do subjects or clusters differ mainly in their baseline levels?
- Do subjects or clusters also differ in slopes over time or in response to another continuous predictor?
- How large is the between-subject, between-cluster, between-site, or between-batch variation?
- Does a fixed effect remain important after modeling subject-specific or cluster-specific variability?
- Are fitted values and residuals reasonable after accounting for random effects?

The method is especially useful when the analysis target includes both **population-average fixed effects** and **subject- or cluster-specific random variation**.

---

## Data layout

LMM data should usually be arranged in **long format**: one worksheet row per observation. A subject or cluster with multiple observations contributes multiple rows.

A typical dataset contains:

| Column type | Example | Required? | Notes |
|---|---|---:|---|
| Response | `Reaction`, `FEV1`, `Score` | Yes | Continuous numeric outcome. |
| Subject / cluster ID | `Subject`, `School`, `Batch`, `Center` | Yes | Defines the grouping unit for random effects. |
| Fixed-effect predictors | `Time`, `Treatment`, `Age`, `Site` | Usually | Used to model the population-average mean. |
| Random-effect predictors | `Time`, `Dose`, `Difficulty` | Optional | Used for random slopes or richer subject-specific trajectories. |
| Visit / time / order variable | `Visit`, `Day`, `Week` | Conditional | Needed for visit-indexed R-side structures such as AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, diagonal heterogeneous, and unstructured residual covariance. |

The subject or cluster identifier may be text or numeric in the ribbon workflow. Worksheet functions use numeric predictor/design ranges for fixed and random effects, with optional names supplied separately.

---

## BESH Stat NG workflow

In Excel ribbon:

**BESH Stat NG → Analyse → Regression → Linear Mixed Models (LMM)**

A typical ribbon analysis has this workflow:

1. Arrange the data in long format.
2. Select the continuous response variable.
3. Select the subject, cluster, or grouping identifier.
4. Select a visit/time/order variable if the selected residual covariance structure needs one.
5. Build the fixed-effect model using continuous predictors, categorical factors, polynomials, and interactions.
6. Build the random-effect model using a random intercept, random slopes, multiple random effects, or random-effect interactions.
7. Choose the G-side random-effects covariance structure.
8. Choose the R-side residual covariance structure, usually starting with identity residual covariance.
9. Choose the estimation method, fixed-effect inference method, and convergence options.
10. Select output tables such as covariance matrices, random effects, fitted values, residuals, diagnostics, and trace output.
11. Review convergence, fixed effects, Type III tests, covariance parameters, random effects, residuals, and fit statistics.

The detailed step-by-step dialog guide is available in [Excel ribbon workflow](lmm/user-interface.md).

---

## Recommended starting settings

The best model depends on the design and scientific question, but the following settings are often a practical starting point.

| Setting | Common starting choice | Why |
|---|---|---|
| Estimation method | REML | Common default for estimating variance components and for small-sample fixed-effect inference. |
| Inference method | Kenward-Roger or Satterthwaite | Provides denominator degrees-of-freedom adjustments for fixed-effect tests. |
| G-side random covariance | Random Intercept, Random Intercept + Slope, or Variance Components | Start with a structure that matches the random-effect design without over-parameterizing the covariance matrix. |
| R-side residual covariance | Identity | Random effects often explain much of the within-subject dependence; add residual covariance only when needed. |
| Optimizer | Average-information / Fisher-scoring default | Efficient starting point for standard mixed-model fits. |
| Random-effect output | Off for large analyses unless needed | Subject-level random-effect output can be large when many subjects or clusters are present. |
| Diagnostics | On | Convergence and fit diagnostics should be reviewed before reporting results. |

For model comparison involving different fixed-effect terms, use care when comparing ML and REML fits. ML is commonly used when comparing fixed-effect models with different fixed-effect design matrices. REML is commonly used for final variance-component estimation and for comparing covariance structures under the same fixed-effect model.

---

## Random-effect choices

The random-effect part of the model should match the design and the scientific question.

| Random-effect pattern | Interpretation | Example |
|---|---|---|
| Random intercept | Each subject or cluster has its own baseline level. | `(1 | Subject)` style model. |
| Random slope | Each subject or cluster has its own slope for a predictor. | Subject-specific time slopes. |
| Random intercept + slope | Subjects differ in both baseline level and time trend. | Longitudinal trajectory model. |
| Multiple random effects | More than one subject-specific deviation is modeled. | Random slopes for time and difficulty. |
| Random-effect interaction | Subject-specific deviation for an interaction term. | Random time-by-difficulty interaction. |

A random-effect term consumes data support. Rich random-effect designs may improve realism but can also increase convergence difficulty and produce unstable covariance estimates when the number of subjects is small or the random-effect design is sparse.

---

## G-side random-effects covariance structures

The G-side covariance structure describes how the random effects are allowed to vary and covary.

| Structure | Use it when |
|---|---|
| Random Intercept | The random-effect design contains only a random intercept. |
| Random Intercept + Slope | The random-effect design contains a random intercept and one random slope, with their covariance estimated. |
| Identity | Random effects are independent and share one common variance. |
| Variance Components (VC/Diag) | Random effects are independent but each has its own variance. This is often a stable default for multiple random effects. |
| Compound Symmetry (CS) | Random effects share a common variance and common correlation. |
| Heterogeneous Compound Symmetry (CSH) | Random effects have separate variances but a common correlation. |
| Autoregressive (AR1) | Ordered random effects share one variance and have correlations that decay by random-effect column lag. |
| Heterogeneous Autoregressive (ARH1) | Ordered random effects have separate variances and AR(1)-style correlation. |
| Toeplitz (TOEP) | Ordered random effects share one variance and have a separate correlation for each lag. |
| Heterogeneous Toeplitz (TOEPH) | Ordered random effects have separate variances and separate lag correlations. |
| Unstructured Random Effects | Every random-effect variance and covariance is estimated. |

For G-side AR(1), ARH(1), Toeplitz, and heterogeneous Toeplitz structures, the lag order is the **order of the random-effect design columns**, not the visit/time variable. These structures are most meaningful when the random-effect columns themselves have a natural order.

For more detail, see [Random effects and covariance structures](lmm/random-effects-and-covariance.md).

---

## R-side residual covariance structures

The R-side residual covariance structure describes residual correlation after fixed and random effects have been included. The default is identity residual covariance.

| Structure | Use it when |
|---|---|
| Identity | Residuals have a common variance and are independent after random effects. |
| Diagonal Heterogeneous | Residual variances differ by visit/order, with zero residual covariance. |
| Compound Symmetry | Residuals have common variance and common covariance. |
| Heterogeneous CS | Residual variances differ by visit/order, with common residual correlation. |
| AR(1) | Residual correlation decreases with visit/order lag. |
| Heterogeneous AR(1) | Residual variances differ by visit/order, with AR(1)-style correlation. |
| Toeplitz (TOEP) | Residual correlation depends on visit/order lag but does not have to follow a single AR(1) decay. |
| Heterogeneous Toeplitz (TOEPH) | Residual variances differ by visit/order and residual correlations are lag-specific. |
| Unstructured | Every residual variance and covariance across visits is estimated. |

Visit-indexed residual structures require a meaningful visit/time/order variable. Identity and compound-symmetry residual covariance do not require a visit variable.

---

## Main outputs

The LMM output workbook is designed to support model interpretation, review, and reproducibility.

| Output area | What it tells you |
|---|---|
| Model information | Variables, data counts, estimation method, inference method, G-side structure, R-side structure, and options used. |
| Fixed effects | Coefficient estimates, standard errors, test statistics, confidence intervals, p-values, and denominator degrees of freedom where applicable. |
| Type III tests | Term-level fixed-effect tests for main effects and interactions. |
| Covariance parameters | Estimated G-side and R-side variance/covariance/correlation parameters. |
| G covariance and G correlation matrices | Estimated covariance/correlation among random effects. |
| R covariance and R correlation matrices | Estimated residual covariance/correlation pattern. |
| Random effects | Subject- or cluster-specific random-effect estimates. |
| Fit statistics | Log-likelihood, information criteria, and related model-fit summaries. |
| Fitted values and residuals | Row-level predictions and residuals for diagnostics and export. |
| Diagnostics and trace output | Convergence messages, iteration behavior, and numerical diagnostic information when requested. |

For detailed interpretation of every output block, see [Options and output reference](lmm/options-and-output.md).

---

## Worksheet-function workflow

Users who prefer reproducible worksheet formulas can use the `BESH.REGR.LMM_*` function family. The usual pattern is:

1. fit the model and return a reusable handle with `BESH.REGR.LMM_FIT`;
2. extract coefficient, Type III, covariance, fit-statistic, random-effect, fitted-value, or residual tables from that handle;
3. optionally drop the handle when it is no longer needed.

Common extractor functions include:

| Function | Purpose |
|---|---|
| `BESH.REGR.LMM_RESULTS` | Return all standard result tables. |
| `BESH.REGR.LMM_COEF` | Return the fixed-effects coefficient table. |
| `BESH.REGR.LMM_TYPE3` | Return Type III fixed-effect tests. |
| `BESH.REGR.LMM_COVPARMS` | Return covariance-parameter output. |
| `BESH.REGR.LMM_G_COV` / `BESH.REGR.LMM_G_CORR` | Return G-side covariance or correlation matrices. |
| `BESH.REGR.LMM_R_COV` / `BESH.REGR.LMM_R_CORR` | Return R-side covariance or correlation matrices. |
| `BESH.REGR.LMM_RANEF` | Return subject- or cluster-level random-effect estimates. |
| `BESH.REGR.LMM_FITSTATS` | Return fit statistics. |
| `BESH.REGR.LMM_FITTED` / `BESH.REGR.LMM_RESID` | Return row-level fitted values or residuals. |
| `BESH.REGR.LMM_DROP` / `BESH.REGR.LMM_CLEAR_ALL` | Remove one handle or clear all cached LMM handles. |

The method-oriented worksheet guide is in [Worksheet functions](lmm/worksheet-functions.md). The generated syntax reference starts at [BESH.REGR.LMM_FIT](../udf/regression-models.md#beshregrlmm_fit).

---

## How this documentation is organized

The LMM documentation is split into focused pages so that practical users can start with the dialog workflow while advanced users can review the model definition, covariance structures, and implementation details.

| Page | Audience | Use it for |
|---|---|---|
| [Concepts and use cases](lmm/concepts-and-use-cases.md) | All users | Understanding what LMM is, when it is useful, and how the data should be arranged. |
| [Model and mathematics](lmm/model-and-mathematics.md) | Statistical users | Formal model notation, likelihood, ML/REML, covariance structures, and fixed-effect inference. |
| [Excel ribbon workflow](lmm/user-interface.md) | Practical users | Running LMM from the dialog and understanding the main control groups. |
| [Options and output reference](lmm/options-and-output.md) | All users | Choosing options and interpreting output tables, warnings, and diagnostics. |
| [Random effects and covariance structures](lmm/random-effects-and-covariance.md) | Statistical and applied users | Choosing random-effect terms, G-side covariance, and optional R-side covariance. |
| [Worksheet functions](lmm/worksheet-functions.md) | Excel power users | Building reproducible LMM analyses with worksheet formulas. |
| [Implementation details](lmm/implementation-details.md) | Advanced users | Understanding numerical behavior, convergence safeguards, and reproducibility notes. |
| [Comparison with other software](lmm/software-comparison.md) | Validation users | Aligning BESH Stat NG output with R, SAS-style, and other mixed-model workflows. |
| [Examples and interpretation](lmm/examples.md) | Practical users | Worked examples and interpretation templates. |

Recommended reading paths:

- **New practical users:** start with [Concepts and use cases](lmm/concepts-and-use-cases.md), then [Excel ribbon workflow](lmm/user-interface.md), then [Examples and interpretation](lmm/examples.md).
- **Statisticians and reviewers:** read [Model and mathematics](lmm/model-and-mathematics.md), [Random effects and covariance structures](lmm/random-effects-and-covariance.md), and [Options and output reference](lmm/options-and-output.md).
- **Validation and reproducibility users:** read [Implementation details](lmm/implementation-details.md) and [Comparison with other software](lmm/software-comparison.md).
- **Worksheet users:** read [Worksheet functions](lmm/worksheet-functions.md) and the generated [Regression Models UDF reference](../udf/regression-models.md#beshregrlmm_fit).

---

## Relationship to related methods

| Method | Main target | How it differs from LMM |
|---|---|---|
| Ordinary linear regression | Independent continuous outcomes | Does not include random effects or within-cluster covariance. |
| [Mixed Models for Repeated Measures (MMRM)](mixed-models-for-repeated-measures-mmrm.md) | Marginal repeated-measures means and contrasts | Models the repeated-measures covariance directly and does not require user-specified random effects. |
| [Generalized Estimating Equations (GEE)](generalized-estimating-equations-gee.md) | Marginal models for correlated outcomes | Uses working correlation and robust inference rather than likelihood-based random effects. |
| [Generalized Linear Models (GLM)](generalized-linear-models-glm.md) | Independent normal or non-normal outcomes | Does not model subject- or cluster-specific random effects. |
| Repeated-measures ANOVA | Balanced repeated continuous outcomes | Usually has more restrictive assumptions and less flexible covariance handling. |

LMM and MMRM can both be used for continuous repeated-measures data, but they target different modeling strategies. LMM introduces subject- or cluster-specific random effects. MMRM focuses on marginal repeated-measures covariance and is often used when the primary target is adjusted visit-specific means and planned contrasts.

---

## Before reporting an LMM result

Before using LMM output in a report, check that:

- the data are in long format or otherwise aligned so each row represents one observation;
- the response is continuous and measured on a comparable scale across rows;
- the subject or cluster identifier correctly defines the random-effect grouping unit;
- fixed effects and random effects match the scientific question;
- categorical variables and reference levels are interpreted as intended;
- the random-effect covariance structure is supported by the number of subjects and the random-effect design;
- any visit-indexed residual covariance structure uses a meaningful visit/time/order variable;
- the selected ML/REML and inference methods match the analysis objective;
- convergence diagnostics and warnings have been reviewed;
- covariance matrices are positive definite or otherwise scientifically interpretable;
- fitted values, residuals, and influential observations have been checked;
- random-effect estimates are interpreted as model-based conditional estimates, not as directly observed subject effects.

---

## Further reading

For selected English-language books and articles on random-effects modeling, longitudinal LMMs, mixed-model computation, and Kenward-Roger-style small-sample inference, see [Model and mathematics: References and further reading](lmm/model-and-mathematics.md#17-references-and-further-reading).

## See also

- [Concepts and use cases](lmm/concepts-and-use-cases.md)
- [Model and mathematics](lmm/model-and-mathematics.md)
- [Excel ribbon workflow](lmm/user-interface.md)
- [Options and output reference](lmm/options-and-output.md)
- [Random effects and covariance structures](lmm/random-effects-and-covariance.md)
- [Worksheet functions](lmm/worksheet-functions.md)
- [Mixed Models for Repeated Measures (MMRM)](mixed-models-for-repeated-measures-mmrm.md)
- [Regression formula syntax](../udf/regression-formula-syntax.md)
