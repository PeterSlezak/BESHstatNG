# LMM random effects and covariance structures

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** explain how random effects are specified in BESH Stat NG linear mixed models (LMM), how the G-side random-effects covariance structures differ from the R-side residual covariance structures, and how to choose a structure that is useful without over-parameterizing the model.

---

## 1. Why random effects and covariance structures matter

A linear mixed model separates the mean model from the covariance model. The fixed effects describe the population-average mean. The random effects and residual covariance describe how observations from the same subject, cluster, site, batch, school, or other grouping unit are related.

For subject or cluster \(i\), the model can be written as:

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i,
\]

where \(X_i\beta\) is the fixed-effect part, \(Z_i b_i\) is the random-effect part, and \(\varepsilon_i\) is the residual error. The two covariance components are:

\[
b_i \sim N(0,G), \qquad \varepsilon_i \sim N(0,R_i),
\]

and the marginal covariance of the response vector is:

\[
V_i = Z_iGZ_i^\top + R_i.
\]

The **G-side covariance** \(G\) describes the variances and covariances of the random effects, such as random intercepts and random slopes. The **R-side covariance** \(R_i\) describes residual variation that remains after the fixed effects and random effects have been included.

!!! tip "A practical way to think about the two sides"
    Use the G side to model subject- or cluster-level deviations such as different baseline levels or different slopes. Use the R side only when residuals within the same subject or cluster still need additional structure, such as visit-specific residual variances or serial residual correlation.

---

## 2. Fixed effects versus random effects

Fixed and random effects answer different questions.

| Component | What it represents | Examples | Main output |
|---|---|---|---|
| Fixed effects | Population-average effects. | Treatment, time, age, site category, treatment × time. | Coefficients, standard errors, confidence intervals, p-values, Type III tests. |
| Random effects | Subject- or cluster-specific deviations from the population-average effects. | Subject intercept, subject time slope, batch intercept, school intercept. | Variance/covariance estimates and optional subject- or cluster-specific random-effect estimates. |

A random effect is usually appropriate when the levels of the grouping variable are numerous, exchangeable, or not the main levels being compared. For example, subject-specific intercepts are usually modeled as random effects because individual subjects are sampled units. A treatment group is usually modeled as a fixed effect because its estimated difference is usually part of the analysis question.

!!! note "Subject IDs can be text or numeric"
    In the ribbon workflow, the subject or cluster ID may be a text or numeric column. Text identifiers such as `S001`, `USUBJID-014`, `Site A`, or `Batch_7` are valid grouping labels. The response and numeric model predictors must still be usable as numeric analysis columns.

---

## 3. Random-effect design patterns

The random-effect design matrix \(Z_i\) is built from the random-effect terms selected in the dialog or supplied to worksheet functions. Its columns define which subject- or cluster-specific deviations are estimated.

### 3.1 Random intercept

A **random intercept** gives each subject or cluster its own baseline level:

\[
y_{ij} = x_{ij}^\top\beta + b_{0i} + \varepsilon_{ij}.
\]

Use a random intercept when observations from the same unit tend to be systematically higher or lower than observations from other units.

Common examples:

- repeated measurements within a subject;
- samples measured within a laboratory batch;
- students nested within schools;
- patients nested within centers.

A random-intercept model is often the first mixed model to try because it captures grouped dependence with only one G-side variance parameter.

### 3.2 Random slope

A **random slope** allows the effect of a predictor to vary by subject or cluster:

\[
y_{ij} = x_{ij}^\top\beta + z_{ij} b_{1i} + \varepsilon_{ij}.
\]

Use a random slope when subjects or clusters plausibly have different trajectories or different responses to a continuous predictor. In a longitudinal study, a random time slope allows some subjects to improve or decline faster than others.

A random-slope-only model is possible, but it is less common than a random-intercept-plus-slope model. If the random intercept is turned off, use a general G-side structure such as **Variance Components (VC/Diag)** or **Unstructured Random Effects**, not the convenience **Random Intercept + Slope** structure.

### 3.3 Random intercept plus random slope

A random-intercept-plus-slope model allows each subject or cluster to differ in baseline and in slope:

\[
y_{ij} = x_{ij}^\top\beta + b_{0i} + z_{ij} b_{1i} + \varepsilon_{ij}.
\]

The G-side covariance can estimate whether high-baseline subjects also tend to have higher or lower slopes. This model is common for longitudinal trajectories, dose-response curves, calibration data, and repeated measurements over time.

### 3.4 Multiple random effects

An LMM may contain more than one random slope. For example, a subject may have a random intercept, a random time slope, and a random difficulty slope.

When the random-effect design has \(q\) columns, the G-side covariance matrix is \(q \times q\). The number of covariance parameters can grow quickly, especially with an unstructured covariance matrix. For example, four random-effect columns require:

| G-side structure | Parameters for 4 random-effect columns |
|---|---:|
| Identity | 1 |
| Variance Components (VC/Diag) | 4 |
| Compound Symmetry (CS) | 2 |
| Heterogeneous Compound Symmetry (CSH) | 5 |
| Autoregressive (AR1) | 2 |
| Heterogeneous Autoregressive (ARH1) | 5 |
| Toeplitz (TOEP) | 4 |
| Heterogeneous Toeplitz (TOEPH) | 7 |
| Unstructured Random Effects | 10 |

This is why **Variance Components (VC/Diag)** is often a practical default for multiple random effects: each random effect gets its own variance, but covariances are fixed to zero.

### 3.5 Random-effect interactions

A random-effect interaction allows the subject- or cluster-specific deviation for an interaction term to vary. For example, a subject-specific `Time × Difficulty` random effect allows subjects to differ in how difficulty modifies their time trend.

Random-effect interactions can be useful, but they are demanding. They add columns to \(Z_i\), increase the number of G-side covariance parameters, and can be sensitive to scaling. Before using a random interaction, check that:

- the interaction is scientifically meaningful;
- the underlying predictors vary enough within subjects or clusters;
- there are enough subjects or clusters to estimate the extra variance component;
- the model still converges reliably;
- the fitted variance is not essentially zero or unstable.

!!! tip "Start with the simplest defensible random-effect pattern"
    Add random slopes and random interactions because the design and question require them, not only because they are available. When in doubt, compare a simpler random-effect structure with the richer one and review convergence, covariance estimates, and diagnostics.

---

## 4. G-side versus R-side covariance

The same dataset can show dependence for two different reasons.

| Side | Matrix | What it models | Typical LMM starting point |
|---|---|---|---|
| G side | \(G\) | Covariance among random effects, such as random intercepts and random slopes. | Random Intercept, Random Intercept + Slope, or VC/Diag. |
| R side | \(R_i\) | Residual covariance after fixed and random effects. | Identity. |

In many LMM analyses, a random intercept or random slope explains most within-subject dependence, so identity residual covariance is adequate. In other analyses, residuals may still be correlated or have visit-specific variances. Then an R-side structure such as diagonal heterogeneous, AR(1), Toeplitz, or unstructured residual covariance may be useful.

!!! warning "Avoid modeling the same dependence twice"
    Rich G-side and rich R-side structures can compete with each other. For example, a random intercept plus a compound-symmetry residual structure can both represent broad within-subject similarity. Use scientific design, diagnostics, convergence behavior, and parsimony to decide whether both are needed.

---

## 5. G-side covariance structures

The G-side structure controls the covariance matrix of the random effects. Let \(q\) be the number of random-effect columns, including the random intercept if selected.

### 5.1 G-side structure summary

| G-side structure | Parameters | Use when | Main caution |
|---|---:|---|---|
| **Random Intercept** | 1 | The only random effect is a subject or cluster intercept. | Not valid for random slopes or multiple random-effect columns. |
| **Random Intercept + Slope** | 3 | The random design contains a random intercept and exactly one random slope, and their covariance should be estimated. | Use a general structure if there are multiple slopes or no random intercept. |
| **Identity** | 1 | Random effects are independent and share one common variance. | Restrictive unless random-effect columns are on comparable scales. |
| **Variance Components (VC/Diag)** | \(q\) | Random effects are independent but each has its own variance. | Does not estimate covariance among random effects. |
| **Compound Symmetry (CS)** | 2 when \(q>1\) | Random effects have common variance and common pairwise correlation. | Assumes random-effect columns are comparable. |
| **Heterogeneous Compound Symmetry (CSH)** | \(q+1\) when \(q>1\) | Random effects have separate variances but one common correlation. | One correlation may be too simple for unrelated random effects. |
| **Autoregressive (AR1)** | 2 when \(q>1\) | Random-effect columns are ordered and correlations should decay by column lag. | Uses random-effect column order, not the visit variable. |
| **Heterogeneous Autoregressive (ARH1)** | \(q+1\) when \(q>1\) | Ordered random effects have separate variances and AR(1)-style correlation. | Requires meaningful random-effect column order. |
| **Toeplitz (TOEP)** | \(q\) | Ordered random effects have lag-specific correlations with common variance. | More flexible than AR(1), but still assumes same-lag equality. |
| **Heterogeneous Toeplitz (TOEPH)** | \(2q-1\) | Ordered random effects have separate variances and lag-specific correlations. | Can be parameter-heavy for large \(q\). |
| **Unstructured Random Effects** | \(q(q+1)/2\) | Every random-effect variance and covariance should be estimated. | Most flexible but often hardest to fit. |

For one random-effect column, all valid G-side structures reduce to a single variance parameter. The convenience **Random Intercept** and **Random Intercept + Slope** entries are intended for the most common simple models. Use **VC/Diag**, another structured covariance, or **Unstructured Random Effects** for richer random-effect designs.

### 5.2 Random Intercept

**Random Intercept** estimates a single between-subject or between-cluster variance. It should be used only when the random-effect model contains the random intercept and no other random-effect columns.

This is the most stable G-side option and is often the first model to fit. It is appropriate when subjects or clusters have different baselines but the slopes are assumed to be common after fixed effects have been included.

### 5.3 Random Intercept + Slope

**Random Intercept + Slope** estimates a two-by-two unstructured covariance matrix for a random intercept and one random slope. It estimates:

- random-intercept variance;
- random-slope variance;
- covariance or correlation between the random intercept and slope.

Use it when the model contains exactly one random slope in addition to the random intercept. If there are two or more random slopes, choose **VC/Diag**, a structured G-side covariance, or **Unstructured Random Effects**.

### 5.4 Identity

**Identity** assumes all random-effect columns are independent and have the same variance:

\[
G = \sigma_b^2 I_q.
\]

This is compact but restrictive. It can be useful as a diagnostic or when random-effect columns are intentionally scaled to be comparable. It is less appropriate when one random effect is an intercept and another is a slope measured on a different scale.

### 5.5 Variance Components (VC/Diag)

**Variance Components (VC/Diag)** estimates a separate variance for each random-effect column and fixes all covariances to zero:

\[
G = \operatorname{diag}(\sigma_1^2,\ldots,\sigma_q^2).
\]

This is often the best automatic choice for multiple random effects. It supports random intercepts, random slopes, categorical random effects, polynomial random effects, and random-effect interactions without requiring the model to estimate every covariance among them.

Use VC/Diag when:

- the random-effect design has several columns;
- the sample size is not large enough for an unstructured G matrix;
- random-effect variances are needed, but random-effect covariances are not central to the analysis;
- an unstructured G fit is unstable or near singular.

### 5.6 Compound Symmetry (CS)

**Compound Symmetry (CS)** assumes common random-effect variance and common pairwise correlation:

\[
G_{jk} =
\begin{cases}
\sigma_b^2, & j=k,\\
\sigma_b^2\rho, & j\ne k.
\end{cases}
\]

This is parsimonious for a set of comparable random-effect columns. It is usually not a good default for an intercept plus unscaled slopes, because those columns often have different units and variances.

### 5.7 Heterogeneous Compound Symmetry (CSH)

**Heterogeneous Compound Symmetry (CSH)** allows each random effect to have its own variance while using one common correlation:

\[
G_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
\sigma_j\sigma_k\rho, & j\ne k.
\end{cases}
\]

CSH is less restrictive than CS and less parameter-heavy than an unstructured covariance matrix. It can be useful when the random-effect columns have different scales but a single shared correlation is plausible.

### 5.8 Autoregressive (AR1) and heterogeneous AR(1)

**AR1** and **ARH1** are intended for ordered random-effect columns. The homogeneous form assumes common variance:

\[
G_{jk}=\sigma_b^2\rho^{\lvert j-k\rvert}.
\]

The heterogeneous form allows separate variances:

\[
G_{jk}=\sigma_j\sigma_k\rho^{\lvert j-k\rvert}.
\]

The lag \(\lvert j-k\rvert\) is based on the order of the random-effect columns in the random-effect design. It is not based on the visit/time/order variable.

Use G-side AR structures only when the random-effect columns have a meaningful sequence. Examples might include ordered basis functions or ordered random visit effects. Do not use AR1 simply because the original data are longitudinal if the random-effect columns themselves are `Intercept`, `Time`, `Treatment`, and `Time × Treatment`; those columns do not usually form an autoregressive sequence.

### 5.9 Toeplitz (TOEP) and heterogeneous Toeplitz (TOEPH)

**Toeplitz** structures also require ordered random-effect columns. They allow a separate correlation for each random-effect column lag. The homogeneous form is:

\[
G_{jk}=\sigma_b^2\rho_{\lvert j-k\rvert}, \qquad \rho_0=1.
\]

The heterogeneous form is:

\[
G_{jk}=\sigma_j\sigma_k\rho_{\lvert j-k\rvert}, \qquad \rho_0=1.
\]

Toeplitz is more flexible than AR(1) because the lag-two correlation does not have to equal the square of the lag-one correlation. It remains more structured than an unstructured covariance matrix.

Use G-side Toeplitz structures only when same-lag random-effect relationships are scientifically meaningful.

### 5.10 Unstructured Random Effects

**Unstructured Random Effects** estimates every variance and covariance among the random effects:

\[
G =
\begin{bmatrix}
\sigma_{11} & \sigma_{12} & \cdots & \sigma_{1q}\\
\sigma_{12} & \sigma_{22} & \cdots & \sigma_{2q}\\
\vdots & \vdots & \ddots & \vdots\\
\sigma_{1q} & \sigma_{2q} & \cdots & \sigma_{qq}
\end{bmatrix}.
\]

This is the most flexible G-side structure, but it requires \(q(q+1)/2\) covariance parameters. It is most appropriate when:

- the number of random-effect columns is small;
- the number of subjects or clusters is large enough;
- random-effect covariances are scientifically important;
- simpler structures fit poorly or are not plausible.

!!! warning "Unstructured does not mean automatically better"
    An unstructured G matrix can be unstable when the data provide little information about some random-effect covariances. Boundary estimates, high correlations near \(-1\) or \(1\), non-convergence, or large standard errors are signs that a simpler G-side structure may be preferable.

---

## 6. R-side residual covariance structures

The R-side residual covariance structure describes within-subject residual variation after fixed effects and random effects have been included. Let \(T\) be the number of retained visit/time/order levels used to define the residual covariance pattern.

### 6.1 R-side structure summary

| R-side structure | Parameters | Use when | Visit/time/order required? |
|---|---:|---|---:|
| **Identity** | 1 | Residuals are independent with common variance after random effects. | No |
| **Diagonal Heterogeneous** | \(T\) | Residual variance differs by visit/order, but residual correlations are not modeled. | Yes |
| **Compound Symmetry** | 2 when \(T>1\) | Residuals have common variance and common pairwise correlation. | No |
| **Heterogeneous CS** | \(T+1\) when \(T>1\) | Residual variance differs by visit/order with one common residual correlation. | Yes |
| **AR(1)** | 2 when \(T>1\) | Ordered residual correlations decay by visit/order lag, with common variance. | Yes |
| **Heterogeneous AR(1)** | \(T+1\) when \(T>1\) | Ordered residual correlations decay by lag, with visit/order-specific variance. | Yes |
| **Toeplitz (TOEP)** | \(T\) | Residual correlations depend on lag but do not have to follow AR(1) decay. | Yes |
| **Heterogeneous Toeplitz (TOEPH)** | \(2T-1\) | Residual variances differ by visit/order and correlations are lag-specific. | Yes |
| **Unstructured** | \(T(T+1)/2\) | Every residual variance and covariance across visits should be estimated. | Yes |

For \(T=1\), all valid R-side structures reduce to one residual variance. For incomplete longitudinal data, each subject contributes the rows that are observed; the selected R-side structure defines the covariance pattern for the retained visit/order levels.

### 6.2 Identity residual covariance

**Identity** residual covariance is the usual starting point for LMMs:

\[
R_i = \sigma^2 I_{n_i}.
\]

It assumes that, after fixed effects and random effects are included, residuals have common variance and no remaining within-subject residual correlation.

### 6.3 Diagonal heterogeneous residual covariance

**Diagonal Heterogeneous** residual covariance allows residual variance to differ by visit or order level, while residual covariances remain zero:

\[
R_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
0, & j\ne k.
\end{cases}
\]

This is useful when residual spread changes over time but residual correlation is already handled adequately by random effects.

### 6.4 Compound symmetry and heterogeneous compound symmetry

**Compound Symmetry** assumes common residual variance and common residual correlation:

\[
R_{jk} =
\begin{cases}
\sigma^2, & j=k,\\
\sigma^2\rho, & j\ne k.
\end{cases}
\]

**Heterogeneous CS** allows visit/order-specific residual variances with one common residual correlation:

\[
R_{jk} =
\begin{cases}
\sigma_j^2, & j=k,\\
\sigma_j\sigma_k\rho, & j\ne k.
\end{cases}
\]

Compound-symmetry residual covariance can overlap conceptually with a random intercept. Consider whether both are needed in the same model.

### 6.5 AR(1) and heterogeneous AR(1) residual covariance

**AR(1)** residual covariance assumes residual correlation decreases as the visit/order lag increases:

\[
R_{jk}=\sigma^2\rho^{d_{jk}},
\]

where \(d_{jk}\) is the lag between observations \(j\) and \(k\) based on the selected visit/time/order variable. The heterogeneous form is:

\[
R_{jk}=\sigma_j\sigma_k\rho^{d_{jk}}.
\]

AR(1) structures are most natural when visits are ordered and approximately equally spaced, or when the selected ordering is intended to behave like equally spaced occasions.

### 6.6 Toeplitz and heterogeneous Toeplitz residual covariance

**Toeplitz** residual covariance uses a separate residual correlation for each visit/order lag:

\[
R_{jk}=\sigma^2\rho_{d_{jk}}, \qquad \rho_0=1.
\]

The heterogeneous form is:

\[
R_{jk}=\sigma_j\sigma_k\rho_{d_{jk}}, \qquad \rho_0=1.
\]

Toeplitz is useful when residual correlation is mostly determined by lag, but the AR(1) geometric decay pattern is too restrictive. Heterogeneous Toeplitz is more flexible because it also allows visit-specific residual variances.

### 6.7 Unstructured residual covariance

**Unstructured** residual covariance estimates every residual variance and covariance across visit/order levels. It is the most flexible R-side option and is common in MMRM-style repeated-measures models.

In LMMs, unstructured residual covariance should be used cautiously when random effects are also present. If the number of visits is large, the number of R-side covariance parameters can become large quickly.

---

## 7. Choosing a practical covariance structure

A good covariance structure is scientifically plausible, identifiable from the data, and stable enough to support the fixed-effect inference. The most complex structure is not always the best choice.

### 7.1 Suggested starting points

| Analysis goal | Suggested starting model |
|---|---|
| Basic clustered or repeated-measures adjustment | Random intercept, G-side Random Intercept, R-side Identity. |
| Longitudinal trajectories with subject-specific slopes | Random intercept + one random time slope, G-side Random Intercept + Slope, R-side Identity. |
| Multiple random slopes or random-effect interactions | G-side VC/Diag first; consider CSH or Unstructured only if supported. |
| Remaining residual heterogeneity by visit | Add R-side Diagonal Heterogeneous. |
| Remaining serial residual correlation | Add R-side AR(1), heterogeneous AR(1), Toeplitz, or heterogeneous Toeplitz. |
| MMRM-like residual covariance without random effects | Use the MMRM workflow when the goal is a marginal repeated-measures model. |

### 7.2 Escalation strategy

A practical strategy is:

1. Fit the fixed-effect model with the simplest random-effect pattern required by the design.
2. Add random slopes only when subject- or cluster-specific slopes are scientifically meaningful and supported by the data.
3. Use **VC/Diag** for multiple random effects before trying **Unstructured Random Effects**.
4. Add R-side residual covariance only when diagnostics or design considerations suggest residual dependence remains after random effects.
5. Compare fits only when models are fitted to the same retained observations and are scientifically comparable.
6. Prefer simpler structures when richer structures produce boundary estimates, unstable correlations, or convergence warnings.

### 7.3 REML and ML when comparing structures

REML is commonly used for final variance-component estimation and for fixed-effect inference with Kenward-Roger or Satterthwaite methods. When comparing covariance structures under the same fixed-effect model, REML-based fit statistics can be useful diagnostics.

When comparing different fixed-effect mean models, use ML rather than REML, because REML likelihoods depend on the fixed-effect design.

---

## 8. Ordered random effects versus ordered residuals

The meaning of order is different on the G side and R side.

| Structure family | What defines the lag? | Example |
|---|---|---|
| G-side AR1, ARH1, TOEP, TOEPH | Order of random-effect columns in \(Z_i\). | Ordered random visit-effect columns. |
| R-side AR(1), heterogeneous AR(1), TOEP, TOEPH | Selected visit/time/order variable. | Residuals at visits 1, 2, 3, and 4. |

This distinction is important. A longitudinal dataset does not automatically make a G-side AR(1) structure appropriate. The random-effect columns themselves must have a natural order.

!!! example "Column-order example"
    A random-effect design with columns `Intercept`, `Days`, and `Difficulty` does not usually have an autoregressive order. In contrast, a random-effect design built from ordered visit indicators or ordered basis terms may have a meaningful column order.

---

## 9. Scaling, centering, and interactions

Random slopes and random interactions are easier to estimate when predictors are on stable scales.

Recommended practices:

- center time or dose predictors when the intercept should represent a meaningful baseline;
- avoid very large or very small numeric scales in random slopes;
- check whether polynomial or interaction columns are highly correlated;
- use VC/Diag or another structured G-side covariance before trying an unstructured covariance for many random-effect columns;
- keep a clear codebook when categorical factors are represented by numeric codes.

Centering affects interpretation. For example, if `Days_c` is centered at baseline or at the mean visit, the random intercept represents subject-specific deviation at that centered value. This can make the random-intercept variance and intercept-slope covariance easier to interpret.

---

## 10. Interpreting covariance output

The LMM output can include several covariance-related tables.

| Output | What it shows | How to use it |
|---|---|---|
| Covariance parameters | Estimated variances, covariances, correlations, and residual parameters. | Check parameter estimates, boundary values, and whether the selected structure behaves as expected. |
| G covariance matrix | Estimated covariance among random effects. | Inspect scale and covariance among random intercepts, slopes, and other random effects. |
| G correlation matrix | Correlation form of the G covariance matrix. | Easier to review than covariance when random effects use different scales. |
| R covariance matrix | Estimated residual covariance by visit/order pattern. | Check whether residual variances and correlations are plausible. |
| R correlation matrix | Correlation form of the R covariance matrix. | Inspect residual-correlation shape across visits or order levels. |
| Random effects | Subject- or cluster-specific predicted random effects. | Useful for diagnostics and understanding cluster deviations; not independent observed data. |

Boundary estimates, variances near zero, or correlations near \(-1\) or \(1\) often indicate that the random-effect design or covariance structure is too rich for the available data.

---

## 11. Common warning signs

Review the model carefully when you see any of the following:

- non-convergence or maximum iteration warnings;
- covariance matrices reported as near singular;
- random-effect variance estimates essentially equal to zero;
- correlations very close to \(-1\) or \(1\);
- very large standard errors for fixed effects or covariance parameters;
- substantial changes in fixed-effect conclusions when the covariance structure is changed;
- rich G-side and R-side structures in a small dataset;
- many random-effect columns relative to the number of subjects or clusters.

These warnings do not always invalidate the analysis, but they should prompt a simpler comparison model and a review of the design.

---

## 12. Relationship to MMRM

MMRM and LMM can both analyze repeated or clustered continuous outcomes, but they emphasize different covariance modeling strategies.

| Feature | LMM | MMRM |
|---|---|---|
| Random effects | Yes. Random intercepts, random slopes, multiple random effects, and random-effect interactions. | No user-facing random effects in the MMRM workflow. |
| G-side covariance | Yes. Describes random-effect variation. | Not applicable. |
| R-side covariance | Optional residual covariance after random effects. | Main covariance model for repeated measures. |
| Typical default | Random intercept or random intercept + slope with identity residual covariance. | Structured or unstructured residual covariance across visits. |
| Common use | Subject-specific or cluster-specific variation is important. | Marginal repeated-measures treatment effects across visits are the main focus. |

Use the LMM workflow when random intercepts, random slopes, or cluster-specific predictions are part of the analysis. Use the MMRM workflow when the goal is a marginal repeated-measures model with a flexible residual covariance structure and MMRM-style LS-means and repeated-measures contrasts.

---

## 13. Pre-fit checklist

Before fitting a rich random-effect or covariance model, check the following.

- Is the response continuous and approximately suitable for a Gaussian LMM?
- Is the subject or cluster ID correctly coded, with one label per grouping unit?
- Does each random slope vary within enough subjects or clusters to support estimation?
- Are continuous random-slope predictors centered or scaled appropriately?
- Are categorical random effects and interactions scientifically justified?
- Is the selected G-side structure compatible with the random-effect design?
- Is the selected R-side structure compatible with the available visit/time/order information?
- Are there enough subjects, clusters, visits, and observations for the number of covariance parameters?
- Are simpler models available as sensitivity checks?

---

## 14. Next steps

- For formal covariance formulas, likelihood definitions, and degrees-of-freedom methods, see [Model and mathematics](model-and-mathematics.md).
- For ribbon controls and model-building steps, see [Excel ribbon workflow](user-interface.md).
- For output-table interpretation, see [Options and output reference](options-and-output.md).
- For worksheet formulas, see [Worksheet functions](worksheet-functions.md).
- For worked examples, see [Examples and interpretation](examples.md).
