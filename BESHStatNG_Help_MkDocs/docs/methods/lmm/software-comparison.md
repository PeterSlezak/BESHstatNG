# LMM comparison with other software

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** help users compare BESH Stat NG Linear Mixed Models (LMM) with common mixed-model workflows in R, SAS, and other software. The emphasis is on **model equivalence, expected result agreement, covariance-structure mapping, convergence behavior, and practical validation**, not on reproducing every syntax detail from each external package.

For model notation, see [Model and mathematics](model-and-mathematics.md). For random-effect and covariance guidance, see [Random effects and covariance structures](random-effects-and-covariance.md). For worksheet formulas, see [Worksheet functions](worksheet-functions.md).

---

## 1. Main message

BESH Stat NG LMM fits Gaussian mixed models of the form

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i,
\qquad
b_i \sim N(0,G),
\qquad
\varepsilon_i \sim N(0,R_i).
\]

The comparison target is not the software name. The comparison target is the **same fitted model**:

- the same retained analysis rows;
- the same response scale;
- the same subject or cluster grouping variable;
- the same fixed-effect design matrix;
- the same random-effect design matrix;
- the same G-side random-effect covariance structure;
- the same R-side residual covariance structure, if used;
- the same REML or ML likelihood;
- the same denominator degrees-of-freedom method;
- the same convergence tolerance and optimizer assumptions where possible.

When these items are aligned, BESH Stat NG should usually agree closely with established LMM software for fixed-effect estimates, fitted covariance matrices, log-likelihood quantities, fitted values, residuals, and subject/cluster random-effect predictions. Exact bit-for-bit equality is not expected because software can differ in contrast coding, covariance parameterization, optimizer scaling, boundary handling, rank handling, small-sample degrees-of-freedom approximations, and printed rounding.

!!! important "Compare the model, not only the formula text"
    A formula that looks similar in two programs can still request a different model. Before comparing p-values, first compare retained rows, fixed-effect columns, random-effect columns, G and R covariance assumptions, likelihood method, and convergence status.

---

## 2. BESH Stat NG compared with R `lme4` and `lmerTest`

R `lme4::lmer` is a natural comparator for random-intercept and random-slope LMMs. It is especially useful for checking fixed-effect estimates, random-effect covariance estimates, log-likelihood values, and conditional random-effect predictions.

The closest R formulas are usually:

```r
# Random intercept
Reaction ~ Days_c + TreatmentF + SiteF + Age_c + Difficulty +
  (1 | Subject)

# Random intercept plus random slope with estimated intercept-slope covariance
Reaction ~ Days_c * TreatmentF + SiteF + Age_c + Difficulty +
  (Days_c | Subject)

# Multiple independent random effects, similar to a variance-components/diagonal G matrix
Reaction ~ Days_c * TreatmentF + SiteF + Age_c + Difficulty + DaysDifficulty +
  (1 | Subject) +
  (0 + Days_c | Subject) +
  (0 + Difficulty | Subject) +
  (0 + DaysDifficulty | Subject)
```

The `lmerTest` package extends `lmer` output with denominator degrees-of-freedom and fixed-effect tests, including Satterthwaite and Kenward-Roger-style workflows. It is often a better comparator than base `lme4` when the validation target includes p-values or Type III-style fixed-effect tests.

### 2.1 Expected agreement

For directly comparable random-effects models with identity residual covariance, expect close agreement for:

- fixed-effect estimates;
- fixed-effect standard errors under model-based covariance assumptions;
- G-side variance/covariance estimates;
- log-likelihood, REML criterion, AIC, and related fit quantities when parameter counting conventions match;
- marginal fitted values and marginal residuals;
- conditional random-effect predictions, allowing for naming and ordering differences.

### 2.2 Expected differences

Common differences include:

| Output area | Why differences can occur | Practical action |
|---|---|---|
| Coefficient labels | Different factor coding and reference levels. | Compare the design matrix conceptually, not only labels. |
| Type III tests | Different contrast coding, marginality rules, and rank handling. | Align factor coding and test definitions before comparing F tests. |
| Denominator DF | `lme4` itself does not provide ordinary small-sample p-values; `lmerTest` adds approximations. | Compare estimate, SE, DF, and test statistic separately. |
| Information criteria | ML/REML setting and parameter-count conventions can differ. | Compare log-likelihood first, then AIC/BIC conventions. |
| Random effects | BLUP labels and ordering may differ. | Match by subject and random-effect term name. |
| Residual covariance | `lmer` is primarily a random-effects LMM engine and does not provide the same R-side residual covariance menu as BESH Stat NG. | Use `nlme` or SAS for R-side covariance comparisons. |

!!! note "Independent random effects"
    In R, the double-bar syntax such as `Days_c || Subject` is commonly used for independent random effects with numeric predictors. For categorical or more complex independent random effects, separate random-effect terms are often clearer for validation.

---

## 3. BESH Stat NG compared with R `nlme`

R `nlme::lme` is useful when the comparison includes residual correlation or residual heterogeneity. Unlike `lme4`, `nlme` can combine random effects with residual correlation structures such as AR(1) and compound symmetry, and with variance functions for heterogeneous residual variance.

Example comparison model:

```r
library(nlme)

m_nlme_ar1 <- lme(
  Reaction ~ Days_c * TreatmentF + SiteF + Age_c + Difficulty,
  random = ~ Days_c | Subject,
  correlation = corAR1(form = ~ Visit | Subject),
  data = d,
  method = "REML"
)
```

This is useful for comparing a BESH Stat NG model with:

- random intercept plus random slope for `Days_c`;
- R-side `AR(1)` residual covariance;
- `Visit` used as the residual ordering variable.

### 3.1 What `nlme` is good for

Use `nlme` comparisons when the validation target includes:

- residual AR(1) correlation;
- residual compound symmetry;
- heterogeneous residual variances;
- random intercepts and random slopes;
- REML or ML likelihood comparison for moderate-size datasets.

### 3.2 Where `nlme` may not match exactly

`nlme` and BESH Stat NG may use different covariance parameterizations, optimizer scaling, and defaults for factor contrasts and denominator degrees of freedom. Some BESH Stat NG covariance structures, such as heterogeneous Toeplitz, may not have a direct one-line `nlme` equivalent. In those cases, compare fitted covariance matrices and likelihood quantities only after confirming that the structures are genuinely equivalent.

---

## 4. BESH Stat NG compared with SAS `PROC MIXED`

SAS `PROC MIXED` is a common regulatory and production comparator for Gaussian mixed models. Conceptually, BESH Stat NG LMM options map to the same broad PROC MIXED ideas:

- `MODEL` statement: fixed-effect mean model;
- `RANDOM` statement: G-side random effects and random-effect covariance structure;
- `REPEATED` statement: R-side residual covariance structure;
- `METHOD=REML` or `METHOD=ML`: likelihood choice;
- `DDFM=` options: denominator degrees-of-freedom method.

Illustrative SAS-style specifications:

```sas
/* Random intercept */
proc mixed data=lmmdata method=reml;
  class Subject Treatment Site;
  model Reaction = Days_c Treatment Site Age_c Difficulty / solution ddfm=kr;
  random intercept / subject=Subject type=vc;
run;

/* Random intercept plus random slope with unstructured 2x2 G */
proc mixed data=lmmdata method=reml;
  class Subject Treatment Site;
  model Reaction = Days_c Treatment Site Age_c Difficulty / solution ddfm=kr;
  random intercept Days_c / subject=Subject type=un;
run;

/* Random slope plus residual AR(1) by visit */
proc mixed data=lmmdata method=reml;
  class Subject Treatment Site Visit;
  model Reaction = Days_c Treatment Site Age_c Difficulty / solution ddfm=kr;
  random intercept Days_c / subject=Subject type=un;
  repeated Visit / subject=Subject type=ar(1);
run;
```

### 4.1 Expected agreement

When the same fixed effects, random effects, covariance structures, likelihood method, and denominator-DF method are selected, SAS and BESH Stat NG should usually agree closely on:

- fixed-effect estimates;
- G and R covariance estimates;
- covariance and correlation matrices;
- log-likelihood and AIC, allowing for parameter-count convention differences;
- Type III-style tests when factor coding and estimability rules are aligned;
- random-effect predictions when the same conditional prediction convention is used.

### 4.2 Common SAS-versus-BESH differences

| Area | Possible difference | What to check |
|---|---|---|
| `CLASS` ordering | Reference levels and coefficient labels can differ. | Explicitly record level order and reference category. |
| Covariance aliases | Similar names may be applied on G side, R side, or both. | Confirm whether the structure is attached to random effects or residuals. |
| Denominator DF | Kenward-Roger and Satterthwaite details can differ. | Compare DF before comparing p-values. |
| Boundary estimates | Near-zero variances or correlations near ±1 can be handled differently. | Review covariance matrices and convergence diagnostics. |
| Information criteria | BIC sample-size and parameter-count conventions may differ. | Compare log-likelihood first. |

---

## 5. Covariance-structure mapping

BESH Stat NG exposes both **G-side** random-effect covariance structures and **R-side** residual covariance structures. Other software sometimes uses the same short names for both sides, so always confirm where the structure is applied.

### 5.1 G-side random-effect covariance

| BESH Stat NG G-side option | Meaning | Common comparison approach |
|---|---|---|
| Random Intercept | One random intercept variance. | R `lmer`: <code>(1 &#124; Subject)</code>; SAS `RANDOM intercept / subject=...`. |
| Random Intercept + Slope | Random intercept and one random slope with covariance. | R `lmer`: <code>(Days_c &#124; Subject)</code>; SAS `RANDOM intercept Days_c / TYPE=UN`. |
| Identity (ID) | Same variance for each random-effect column, no covariance. | Specialized structure; compare fitted G matrix rather than relying on formula syntax. |
| Variance Components (VC/Diag) | Separate variances, no covariances. | R: separate random-effect terms or double-bar syntax for numeric predictors; SAS `TYPE=VC`. |
| Compound Symmetry (CS) | Common variance and common covariance/correlation among random-effect columns. | SAS `TYPE=CS`; R usually requires a more specialized parameterization. |
| Heterogeneous CS (CSH) | Different variances with common correlation. | SAS `TYPE=CSH`; compare fitted G matrix. |
| Autoregressive (AR1) | Common variance with correlation decaying by random-effect column order. | SAS `TYPE=AR(1)` on the random-effect block when appropriate. |
| Heterogeneous AR(1) (ARH1) | Different variances with AR(1) correlation by random-effect column order. | SAS `TYPE=ARH(1)` where available; compare fitted G matrix. |
| Toeplitz (TOEP) | Common variance with separate correlations by random-effect-column lag. | SAS `TYPE=TOEP`; compare fitted G matrix. |
| Heterogeneous Toeplitz (TOEPH) | Different variances with separate lag correlations. | SAS `TYPE=TOEPH`; compare fitted G matrix. |
| Unstructured (UN) | Every random-effect variance and covariance is freely estimated. | R `lmer`: correlated random-effects term; SAS `TYPE=UN`. |

!!! warning "Column order matters for G-side AR/Toeplitz"
    G-side AR(1), ARH(1), Toeplitz, and heterogeneous Toeplitz use the order of the random-effect design columns. They are meaningful only when the random-effect columns have a scientifically ordered interpretation.

### 5.2 R-side residual covariance

| BESH Stat NG R-side option | Meaning | Common comparison approach |
|---|---|---|
| Identity (ID) | Independent residuals with common variance. | Default residual assumption in many LMMs. |
| Diagonal Heterogeneous | Independent residuals with visit-specific variances. | SAS diagonal/repeated structure; `nlme` variance functions can help. |
| Compound Symmetry (CS) | Common residual variance and common residual correlation. | SAS `TYPE=CS`; `nlme::corCompSymm`. |
| Heterogeneous CS (HCS) | Visit-specific variances with common residual correlation. | SAS `TYPE=CSH`; `nlme` correlation plus variance function. |
| AR(1) | Correlation decays by visit/order lag with common variance. | SAS `TYPE=AR(1)`; `nlme::corAR1`. |
| Heterogeneous AR(1) (HAR1) | Visit-specific variances with AR(1) correlation. | SAS `TYPE=ARH(1)`; `nlme` AR(1) plus variance function. |
| Toeplitz (TOEP) | Common variance with separate residual correlations for each visit lag. | SAS `TYPE=TOEP`; compare fitted R matrix. |
| Heterogeneous Toeplitz (TOEPH) | Visit-specific variances with separate residual lag correlations. | SAS `TYPE=TOEPH`; compare fitted R matrix. |
| Unstructured (UN) | Every visit variance and visit-pair covariance is freely estimated. | SAS `TYPE=UN`; `nlme::corSymm` plus variance functions can be used for some comparisons. |

!!! note "R-side structures require the visit/order variable"
    In BESH Stat NG, residual structures that depend on visit-specific variance or visit lag require the Visit / Time / Ordering variable. Identity and ordinary compound symmetry do not require visit ordering.

---

## 6. Recommended comparison workflow

Use this sequence before deciding that two software outputs disagree.

| Step | Check | Why it matters |
|---|---|---|
| 1 | Same rows retained? | Missing or invalid values are the most common source of disagreement. |
| 2 | Same subject IDs and grouping? | Random effects and residual blocks are subject/cluster based. |
| 3 | Same fixed-effect design? | Factor coding, interactions, and centering determine estimates. |
| 4 | Same random-effect design? | A random intercept, random slope, and random interaction are different models. |
| 5 | Same G-side covariance? | `VC`, `UN`, `CS`, and `AR1` can give different estimates even with the same random-effect columns. |
| 6 | Same R-side covariance? | Adding residual AR(1), Toeplitz, or UN changes the marginal covariance. |
| 7 | Same REML/ML setting? | Likelihood quantities and variance estimates can change. |
| 8 | Same convergence status? | A non-converged fit should not be used as a validation target. |
| 9 | Same denominator-DF method? | P-values can differ even when estimates and SEs match. |
| 10 | Same output scale and rounding? | Printed summaries often hide small numerical agreement. |

For validation, compare output in this order:

1. retained row count and subject count;
2. model formula and covariance choices;
3. convergence status and iteration diagnostics;
4. log-likelihood and fit statistics;
5. fixed-effect estimates;
6. G and R covariance matrices;
7. fixed-effect standard errors;
8. denominator degrees of freedom;
9. p-values and confidence intervals;
10. random-effect predictions and residual diagnostics.

---

## 7. Performance and convergence expectations

LMM runtime is driven by several quantities:

- number of observations;
- number of subjects or clusters;
- number of fixed-effect columns;
- number of random-effect columns;
- number of G-side covariance parameters;
- number of R-side covariance parameters;
- selected inference method;
- amount of output requested.

### 7.1 Relative cost of G-side structures

| G-side option | Relative cost | Practical guidance |
|---|---|---|
| Random Intercept | Low | Best first check for clustered data. |
| Random Intercept + Slope | Low to moderate | Good first slope model when one subject-specific slope is expected. |
| Identity | Low | Useful when common random-effect variance is scientifically plausible. |
| VC/Diag | Low to moderate | Good default for multiple random effects when covariances are not central. |
| CS / CSH | Moderate | Useful when random effects are exchangeable or share a common correlation pattern. |
| AR1 / ARH1 | Moderate | Useful only when random-effect columns have a natural order. |
| TOEP / TOEPH | Moderate to high | Flexible ordered-column structures; cost grows with the number of random-effect columns. |
| UN | Highest G-side cost | Most flexible, but easiest to over-parameterize. |

### 7.2 Relative cost of R-side structures

| R-side option | Relative cost | Practical guidance |
|---|---|---|
| Identity | Low | Start here for many LMMs. |
| CS / AR(1) | Low to moderate | Useful residual structures when random effects do not fully explain within-subject dependence. |
| HCS / HAR(1) | Moderate | Allows visit-specific residual variances. |
| TOEP / TOEPH | Moderate to high | Useful when residual correlations depend on lag but not by a simple AR(1) decay. |
| UN | Highest R-side cost | Use only when there are enough subjects and visits to support it. |

### 7.3 Inference cost

Kenward-Roger and Satterthwaite inference require covariance-parameter uncertainty calculations and can take longer than large-sample or residual-DF tests. For difficult models, a practical workflow is:

1. fit the covariance model with a faster inference option;
2. confirm convergence and covariance estimates;
3. refit with Kenward-Roger or Satterthwaite for the final inference;
4. request large output tables only after the model is stable.

!!! tip "Separate optimization time from output time"
    Workbook output, BLUP tables, residual tables, and trace output can noticeably increase elapsed time. When benchmarking software, record both the fitted model specification and the requested output sections.
