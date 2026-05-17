# LMM concepts and use cases

[← Back to LMM overview](../linear-mixed-models-lmm.md)

**Purpose of this page:** explain what a linear mixed model is, when it is useful, how the data should be arranged, and how to choose between random-intercept, random-slope, and richer mixed-model specifications. For the formal model, likelihood, covariance-structure formulas, and degrees-of-freedom methods, see [Model and mathematics](model-and-mathematics.md).

---

## LMM in one paragraph

A **linear mixed model (LMM)** is a likelihood-based model for a continuous outcome when observations are grouped, clustered, or repeatedly measured. It combines **fixed effects**, which describe the average relationship between the response and predictors in the population, with **random effects**, which describe how subjects, clusters, sites, batches, schools, or other grouping units differ from that average. In BESH Stat NG, LMM is designed for Gaussian continuous outcomes and supports random intercepts, random slopes, multiple random effects, random-effect interactions, G-side random-effects covariance structures, optional R-side residual covariance structures, ML and REML fitting, and small-sample fixed-effect inference options.

!!! tip "Practical interpretation"
    Think of an LMM as ordinary linear regression plus a model for grouped variation. A fixed effect answers questions such as “what is the average treatment or time effect?” A random effect answers questions such as “how much do subjects, sites, or batches differ from each other?”

---

## What problem does LMM solve?

Many datasets contain rows that are related to each other. Standard linear regression treats rows as independent and therefore can give misleading standard errors, tests, and confidence intervals when repeated or clustered observations are present.

LMM addresses this by allowing observations within the same subject or cluster to be related. Typical examples include:

| Situation | Grouping unit | Example question |
|---|---|---|
| Longitudinal study | Subject or patient | What is the average change over time, and do subjects differ in baseline or time trend? |
| Multicenter study | Center, site, or investigator | How large is between-center variation after adjusting for treatment and covariates? |
| Education data | School, class, or teacher | What is the average effect of an intervention while accounting for students within schools? |
| Laboratory or manufacturing data | Batch, plate, lot, instrument, or operator | Is there a fixed effect of process condition after accounting for batch-to-batch variation? |
| Repeated technical measurements | Sample, specimen, device, or unit | How much variation is due to repeated measurements within the same experimental unit? |

The key feature is that the model contains a **subject/cluster identifier** that tells BESH Stat NG which rows belong together.

---

## Fixed effects and random effects

Fixed and random effects have different roles.

| Component | Meaning | Common examples | Typical interpretation |
|---|---|---|---|
| Fixed effect | Population-average effect included directly in the mean model. | Treatment, time, dose, sex, age, visit, site category, baseline value, treatment × time. | Estimated coefficient, adjusted mean difference, or term-level test. |
| Random effect | Cluster-specific deviation from the population-average effect. | Subject intercept, subject time slope, site intercept, batch intercept, subject-specific dose slope. | Estimated variance/covariance and optional subject- or cluster-specific predictions. |

A fixed effect is usually included because it is part of the scientific question or an important adjustment variable. A random effect is usually included because observations share a grouping unit and the analysis should model between-unit variation.

!!! note "Random effects are not just another way to code categorical predictors"
    A categorical fixed effect estimates one parameter or contrast pattern for each included category level. A random effect treats the levels as a sample from a distribution and estimates variation among those levels. Random effects are most useful when the grouping levels are numerous, exchangeable, or not the primary objects of direct comparison.

---

## Typical LMM model

A common way to write an LMM is:

\[
y_i = X_i\beta + Z_i b_i + \varepsilon_i,
\]

where subject or cluster \(i\) contributes a response vector \(y_i\), a fixed-effect design matrix \(X_i\), a random-effect design matrix \(Z_i\), random effects \(b_i\), and residual errors \(\varepsilon_i\).

The random effects have a G-side covariance matrix, and the residual errors have an R-side covariance matrix. The model can therefore represent two sources of within-unit dependence:

1. dependence induced by shared random effects, such as subjects with high intercepts tending to have high observations at many visits;
2. residual dependence that remains after random effects, such as serial residual correlation over visits.

The [Model and mathematics](model-and-mathematics.md) page gives the full notation and covariance details.

---

## Data layout

LMM data should normally be arranged in **long format**: one row per observed measurement. A subject or cluster with multiple observations contributes multiple rows.

### Example long-format layout

| `Subject` | `Visit` | `Reaction` | `Days` | `Treatment` | `Age` | `Difficulty` |
|---|---:|---:|---:|---|---:|---:|
| S001 | 0 | 247.3 | 0 | Control | 41 | 0.18 |
| S001 | 1 | 253.9 | 1 | Control | 41 | 0.21 |
| S001 | 2 | 264.4 | 2 | Control | 41 | 0.28 |
| S002 | 0 | 221.8 | 0 | Active | 36 | 0.10 |
| S002 | 1 | 232.5 | 1 | Active | 36 | 0.14 |
| S002 | 2 | 241.9 | 2 | Active | 36 | 0.19 |

In this example, `Subject` identifies the grouping unit, `Visit` gives a numeric order for repeated measurements, `Reaction` is the continuous response, `Days` is a time predictor, `Treatment` is a between-subject factor, and `Difficulty` is a time-varying covariate.

### Column roles

| Role | Required? | Practical notes |
|---|---:|---|
| Response | Yes | Continuous numeric outcome. Non-numeric response values are not analyzed. |
| Subject / cluster ID | Yes | Identifies rows belonging to the same unit. Text IDs such as `S001`, `PT-104`, or `Batch A` and numeric IDs such as `101` are acceptable. |
| Fixed-effect source variables | Usually | Predictors used to build the population-average mean model. The ribbon model builder uses numeric analysis columns; keep a codebook for numeric-coded categorical factors. |
| Random-effect source variables | Optional | Predictors used to build random slopes or richer random-effect terms. Leave empty when fitting a random-intercept-only model. |
| Visit / time / order variable | Conditional | Required for visit-indexed R-side residual covariance structures such as diagonal heterogeneous, AR(1), heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, and unstructured residual covariance. |

The subject or cluster identifier is allowed to be a text column in the ribbon workflow. This is useful for real study identifiers such as `USUBJID`, `PatientID`, `Site-03`, or `Batch_12`. Predictors and visit/order variables used in the model should still be numeric analysis columns in the current ribbon workflow.

!!! tip "Subject ID versus visit/time"
    The subject ID groups rows. The visit/time/order variable orders rows within subject for selected residual covariance structures. A column can be scientifically important even when it is not used in both roles. For example, `Days` might be used as a fixed and random slope, while `Visit` might be used only to define residual-covariance order.

---

## Random-intercept models

A **random-intercept model** allows each subject or cluster to have its own baseline level while sharing the same fixed-effect slopes.

Use a random intercept when:

- observations are grouped by subject, site, batch, school, or another unit;
- some units tend to have systematically higher or lower outcomes than others;
- the primary within-unit pattern can reasonably share the same slope across units;
- the analysis needs an estimate of between-unit variability.

A simple longitudinal random-intercept model might be described as:

\[
\text{Outcome} = \text{fixed treatment effect} + \text{fixed time effect} + \text{subject-specific intercept} + \text{residual error}.
\]

In practical terms, the model adjusts standard errors for the fact that repeated observations from the same subject are related. The estimated random-intercept variance summarizes how much subjects differ in their overall level after accounting for fixed effects.

### Typical examples

| Example | Random intercept represents |
|---|---|
| Repeated patient measurements | Patient-specific baseline level. |
| Students nested within schools | School-specific average achievement level. |
| Samples measured in batches | Batch-specific shift in measurement level. |
| Multicenter clinical data | Center-specific baseline difference. |

---

## Random-slope models

A **random-slope model** allows the effect of a predictor to vary across subjects or clusters. The most common random slope is a time slope in longitudinal data.

Use a random slope when:

- subjects or clusters plausibly differ in their rate of change;
- spaghetti plots or exploratory summaries show different trajectories by unit;
- the scientific question includes variation in response to time, dose, difficulty, exposure, or another continuous predictor;
- residual diagnostics suggest that a random intercept alone does not adequately explain within-unit dependence.

A simple random-slope model might be described as:

\[
\text{Outcome} = \text{fixed time trend} + \text{subject-specific intercept} + \text{subject-specific time slope} + \text{residual error}.
\]

The fixed time coefficient estimates the average time trend. The random-slope variance estimates how much individual slopes differ around that average.

!!! warning "Random slopes need data support"
    A random slope is difficult to estimate when most subjects have only one or two observations, when the slope predictor has little variation within subject, or when the number of subjects is small. Start with a model that the data can support, then add complexity only when scientifically justified.

---

## Random intercepts and random slopes together

A common LMM includes both a random intercept and a random slope. This allows each subject to have a personal baseline and a personal trajectory.

The random-effects covariance structure determines whether and how these subject-specific deviations are related.

| G-side choice | Interpretation for intercept + slope |
|---|---|
| Random Intercept + Slope | Estimates intercept variance, slope variance, and their covariance. |
| Variance Components (VC/Diag) | Estimates separate intercept and slope variances but assumes zero covariance. |
| Identity | Assumes a common variance for the intercept and slope random effects and zero covariance. Usually too restrictive unless intentionally chosen. |
| Unstructured Random Effects | Estimates all variances and covariances; equivalent to the most flexible option for two random effects. |

The covariance or correlation between the random intercept and random slope can be important. A positive correlation means subjects with higher baselines tend to have larger slopes; a negative correlation means subjects with higher baselines tend to have smaller slopes.

---

## Multiple random effects and random-effect interactions

BESH Stat NG LMM can include more than one random-effect predictor. Examples include:

- random intercept plus random time slope;
- random intercept plus random dose slope;
- random intercept plus random slopes for time and difficulty;
- random intercept plus a random time-by-difficulty interaction;
- random effects for ordered basis terms such as linear and quadratic time components.

As the number of random effects increases, the G-side covariance choice becomes more important.

| G-side structure | Practical role for multiple random effects |
|---|---|
| Variance Components (VC/Diag) | Stable default for multiple random effects; each random effect has its own variance and no covariance with the others. |
| Compound Symmetry (CS) | Uses a common variance and common correlation; compact but restrictive. |
| Heterogeneous Compound Symmetry (CSH) | Allows separate variances with one common correlation. |
| AR(1), ARH(1), Toeplitz, TOEPH | Useful only when the random-effect columns have a meaningful order. |
| Unstructured Random Effects | Most flexible, but parameter count increases quickly and convergence can become difficult. |

For example, a random intercept plus three random slopes creates four random-effect columns. An unstructured G matrix estimates ten covariance parameters for those four columns, while a variance-components structure estimates only four variances. The richer model may fit better when strongly supported by data, but the simpler model is often more robust.

---

## G-side versus R-side covariance

LMM has two covariance concepts that answer different questions.

| Covariance side | What it models | Example |
|---|---|---|
| G-side random-effects covariance | Variation and covariance among random effects. | Do subjects with higher intercepts also tend to have steeper slopes? |
| R-side residual covariance | Remaining residual variance and correlation within a subject after fixed and random effects. | Are residuals from nearby visits still correlated after random intercepts and slopes are included? |

A simple LMM often starts with random effects and identity residual covariance. More complex residual covariance can be added when repeated residuals still have a structured within-subject pattern and the study has enough data support.

!!! note "Do not automatically combine every possible covariance option"
    Random slopes and residual AR(1), Toeplitz, or unstructured covariance can sometimes describe overlapping aspects of within-subject dependence. Use plots, convergence diagnostics, covariance estimates, and the analysis objective to decide whether extra residual covariance is needed.

---

## When to use LMM instead of MMRM

LMM and MMRM are related likelihood-based approaches for continuous repeated or clustered data, but they target different modeling perspectives.

Use **LMM** when:

- subject-, site-, batch-, or cluster-level variation is scientifically important;
- random intercepts or random slopes are part of the planned model;
- the analysis needs subject- or cluster-specific fitted values or random-effect predictions;
- the data are clustered but not necessarily arranged as planned visits;
- there are many grouping levels and treating them as fixed effects would be inefficient or scientifically inappropriate.

Use **MMRM** when:

- the analysis target is primarily marginal visit-specific means, treatment differences, change from baseline, or difference in change;
- repeated measures occur at planned visits and the within-subject covariance is modeled directly;
- user-specified random effects are not part of the analysis plan;
- the workflow needs LS-means and treatment-contrast output centered on repeated-measures estimands.

| Question | Usually better starting point |
|---|---|
| What is the adjusted treatment difference at Visit 4? | MMRM |
| How much do subjects differ in baseline response? | LMM |
| Do subjects differ in their slope over time? | LMM |
| What are adjusted visit-by-treatment means and contrasts? | MMRM |
| What is the random batch variance after adjusting for fixed effects? | LMM |

For details on MMRM, see [Mixed Models for Repeated Measures (MMRM)](../mixed-models-for-repeated-measures-mmrm.md).

---

## When not to use LMM

LMM is not appropriate for every grouped dataset.

Avoid or reconsider LMM when:

- the response is binary, ordinal, count, time-to-event, or otherwise clearly non-Gaussian;
- the scientific question requires a generalized mixed model, nonlinear mixed model, survival model, or specialized time-series model;
- there are too few grouping units to estimate the intended random-effect distribution reliably;
- the random-effect structure is much richer than the data can support;
- key predictors have little or no variation within the grouping units needed for random slopes;
- convergence warnings, boundary variance estimates, or near-singular covariance estimates cannot be resolved by a scientifically reasonable simpler model;
- the analysis target is purely marginal repeated-measures LS-means and contrasts, in which case MMRM may be a better first choice.

For non-continuous outcomes, consider related BESH Stat NG methods such as [Generalized Linear Models (GLM)](../generalized-linear-models-glm.md), [Generalized Estimating Equations (GEE)](../generalized-estimating-equations-gee.md), or survival methods when appropriate.

---

## Practical model-building strategy

A useful LMM workflow is to start with a model that matches the design and then add complexity deliberately.

1. **Define the grouping unit.** Decide whether the repeated or clustered unit is subject, site, batch, school, plate, device, or another identifier.
2. **Choose the fixed-effect mean model.** Include scientifically planned predictors, important covariates, and interactions.
3. **Start with a random intercept when grouping is present.** This estimates between-unit variation in baseline level.
4. **Add random slopes only when justified.** The predictor should vary within grouping unit and have enough observations to estimate slope variation.
5. **Choose a parsimonious G-side structure.** Variance Components (VC/Diag) is often a stable default for multiple random effects; Unstructured is flexible but parameter-intensive.
6. **Add R-side residual covariance only when needed.** Identity residual covariance is a practical starting point when random effects already model within-unit dependence.
7. **Review convergence and covariance estimates.** Boundary variances, extreme correlations, or nonconvergence often indicate that the model is too complex or poorly scaled.
8. **Interpret fixed effects in the context of the random-effect structure.** Changing random effects or covariance assumptions can change standard errors and tests.

!!! tip "Scale continuous predictors"
    Centering or scaling continuous predictors such as time, dose, or age can improve interpretability and numerical behavior, especially when random slopes or interactions are included.

---

## Practical checklist before fitting

Before pressing **Fit**, check the following items.

### Data checks

- The data are in long format, with one row per observed measurement.
- The response is continuous and numeric.
- The subject or cluster ID is present for every analyzed row and consistently identifies the grouping unit.
- Text subject IDs are clean and consistent; for example, `S01` and `S001` should not accidentally refer to the same unit.
- Predictors used in the current ribbon model are numeric analysis columns; categorical factors are numeric-coded where required.
- The visit/time/order variable is numeric when a visit-indexed residual covariance structure is selected.
- Missing values are understood and are not caused by worksheet import or coding mistakes.

### Model checks

- The fixed-effect model reflects the planned scientific question.
- The random-effect design reflects plausible between-unit variation.
- Random slopes have enough within-unit variation and observations.
- The selected G-side covariance structure is compatible with the number and meaning of random effects.
- The selected R-side covariance structure is compatible with the visit/time/order variable and the number of repeated observations.
- The estimation method and inference method match the analysis purpose.

### Output checks

- Fixed effects and Type III tests are requested when inferential summaries are needed.
- G-side covariance/correlation output is requested when interpreting random effects.
- R-side covariance/correlation output is requested when a non-identity residual structure is used.
- Random-effect predictions are requested only when needed, because they can produce large output for many subjects or clusters.
- Fitted values, residuals, diagnostics, and trace output are requested for model review and troubleshooting.

---

## Common interpretation cautions

- A significant fixed effect describes the estimated population-average relationship under the selected mixed-model assumptions.
- A random-effect variance close to zero means the corresponding between-unit variation is estimated to be small after accounting for the rest of the model.
- A very high random-effect correlation may indicate a true relationship, but it may also indicate over-parameterization or limited data support.
- Random-effect predictions are model-based estimates, not directly observed subject effects.
- A more complex covariance structure is not automatically better; it must converge reliably and be interpretable for the design.
- ML and REML serve different purposes. Use care when comparing models with different fixed-effect specifications.

---

## Relationship to the rest of the LMM documentation

After reading this page, continue with:

- [Excel ribbon workflow](user-interface.md) for step-by-step dialog instructions;
- [Random effects and covariance structures](random-effects-and-covariance.md) for choosing random-effect and residual covariance options;
- [Options and output reference](options-and-output.md) for interpreting output tables;
- [Worksheet functions](worksheet-functions.md) for reproducible formula-based workflows;
- [Comparison with other software](software-comparison.md) for validation and migration notes.
