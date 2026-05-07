# MMRM concepts and use cases

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** explain what a mixed model for repeated measures (MMRM) is, when it is useful, how the data should be arranged, and how to think about a practical MMRM analysis before choosing detailed options. For the formal model, likelihood, covariance-structure formulas, and degrees-of-freedom methods, see [Model and mathematics](model-and-mathematics.md).

---

## MMRM in one paragraph

A **mixed model for repeated measures (MMRM)** is a likelihood-based model for a continuous outcome measured repeatedly on the same subject. It estimates the mean response using fixed effects, such as treatment, visit, baseline value, sex, race, region, and treatment-by-visit interaction, while modeling the correlation among repeated observations from the same subject through a subject-level covariance matrix. The main estimands are usually **marginal** quantities: visit-specific adjusted means, treatment differences, change from baseline, or difference in change from baseline. In BESH Stat NG, MMRM is exposed as a user-facing repeated-measures method without user-specified random effects; the within-subject covariance is modeled directly.

!!! tip "Practical interpretation"
    Think of MMRM as a way to compare treatment groups over planned visits while respecting the fact that repeated measurements from the same subject are related. It is usually used when the outcome is continuous and the analysis target is an adjusted mean or treatment difference at one or more visits.

---

## What problem does MMRM solve?

Repeated-measures studies create three practical problems:

1. **Repeated rows from the same subject are not independent.**  
   A subject with high values at one visit often tends to have high values at nearby visits. Standard linear regression treats rows as independent and therefore usually gives inappropriate standard errors for longitudinal data.

2. **Subjects may have different observed visits.**  
   In real longitudinal data, some visits are missed and some subjects discontinue early. Standard repeated-measures ANOVA workflows are often awkward or overly restrictive for incomplete repeated measurements.

3. **The scientific question is usually marginal.**  
   In many clinical-trial and biomedical applications, the question is not “what is this subject's random intercept?” but “what is the adjusted mean response in each treatment group at each visit?” or “what is the treatment difference at the primary visit?”

MMRM addresses these problems by fitting a Gaussian marginal model of the form:

\[
y_i = X_i\beta + \varepsilon_i, \qquad \varepsilon_i \sim N(0, R_i(\theta)),
\]

where subject \(i\) contributes the vector of observed repeated outcomes \(y_i\), the fixed-effect design matrix \(X_i\), and a subject-specific covariance matrix \(R_i(\theta)\) derived from the selected residual covariance structure. Subjects with fewer observed visits contribute smaller observed response vectors and the corresponding covariance submatrix.

---

## Why MMRM is important in practice

MMRM is a flagship method because it sits at the intersection of rigorous longitudinal modeling and practical endpoint reporting. It is especially valuable when a study has scheduled visits, incomplete follow-up, and protocol-defined treatment comparisons.

Key strengths are:

- **Uses all retained observed responses.** Subjects do not have to be complete at every scheduled visit to contribute information.
- **Models the repeated-measures covariance directly.** The user can select structures such as unstructured, autoregressive, compound symmetry, or diagonal covariance depending on the design and data support.
- **Supports adjusted marginal estimands.** LS-means and contrasts provide direct estimates of adjusted treatment means, treatment differences, changes from baseline, and differences in change.
- **Allows realistic fixed-effect models.** Treatment, visit, treatment-by-visit interaction, baseline response, baseline-by-visit interaction, stratification factors, and other prognostic covariates can be included as fixed effects.
- **Provides small-sample inference options.** Kenward-Roger, Satterthwaite, between-within, residual-DF, and large-sample Wald-style inference are available in the broader MMRM workflow.
- **Matches common regulatory and clinical-trial workflows.** Continuous longitudinal endpoints are often summarized through visit-specific adjusted means and model-based treatment contrasts.

!!! note "MMRM is not simply repeated-measures ANOVA with a new name"
    MMRM is a likelihood-based longitudinal model with an explicit subject-level covariance structure. It is usually more flexible than classic repeated-measures ANOVA, particularly when missing visits are present or when the covariance pattern is not well represented by sphericity-type assumptions.

---

## Example data used in this documentation

The worked MMRM pages use the FEV1 example dataset distributed with the OpenPharma `mmrm` package and included in this help project as:

- [037mmrm_fev_data.csv](../../assets/data/037mmrm/037mmrm_fev_data.csv)

The dataset is artificial and contains FEV1 measurements, where FEV1 is forced expired volume in one second. The OpenPharma documentation describes low FEV1 as a possible indicator of chronic obstructive pulmonary disease (COPD). The corresponding OpenPharma between-within example fits the model:

```r
FEV1 ~ RACE + SEX + ARMCD * AVISIT + us(AVISIT | USUBJID)
```

In BESH Stat NG, the same practical model is represented as:

- dependent variable: `FEV1`,
- subject ID: `USUBJID`,
- visit/order variable: `VISITN`,
- categorical visit fixed effect: `AVISIT`,
- treatment factor: `ARMCD`,
- additional factors: `RACE`, `SEX`,
- interaction: `ARMCD × AVISIT`,
- residual covariance: **Unstructured**.

### Dataset summary

| Quantity | Value in `037mmrm_fev_data.csv` |
|---|---:|
| Source rows | 800 |
| Subjects in source data | 200 |
| Scheduled visits per subject | 4 |
| Non-missing `FEV1` observations | 537 |
| Subjects with at least one non-missing `FEV1` | 197 |
| Treatment groups | `PBO`, `TRT` |
| Visit labels | `VIS1`, `VIS2`, `VIS3`, `VIS4` |

!!! note "Why there are more source rows than analyzed responses"
    The CSV contains one row for every scheduled subject-visit combination. Some rows have a blank `FEV1`. These rows describe the scheduled data layout and missingness pattern, but they do not contribute a response value to the likelihood. The observed rows for each subject are retained.

---

## Long-format data requirement

MMRM requires **long format** data: one row per subject per visit/time point. Baseline and other subject-level covariates are usually repeated on every row for the same subject.

### Required data shape

The following rows are taken from the FEV1 example dataset. They show the expected shape, including subject ID, numeric visit order, categorical visit label, treatment, covariates, baseline value, and response.

| `USUBJID` | `VISITN` | `AVISIT` | `ARMCD` | `RACE` | `SEX` | `FEV1_BL` | `FEV1` |
|---|---:|---|---|---|---|---:|---:|
| PT1 | 1 | VIS1 | TRT | Black or African American | Female | 25.2714 |  |
| PT1 | 2 | VIS2 | TRT | Black or African American | Female | 25.2714 | 39.9710 |
| PT1 | 3 | VIS3 | TRT | Black or African American | Female | 25.2714 |  |
| PT1 | 4 | VIS4 | TRT | Black or African American | Female | 25.2714 | 20.4838 |
| PT2 | 1 | VIS1 | PBO | Asian | Male | 45.0248 |  |
| PT2 | 2 | VIS2 | PBO | Asian | Male | 45.0248 | 31.4552 |
| PT2 | 3 | VIS3 | PBO | Asian | Male | 45.0248 | 36.8789 |
| PT2 | 4 | VIS4 | PBO | Asian | Male | 45.0248 | 48.8081 |

In this example, `USUBJID` identifies the subject, `VISITN` gives the numeric visit order, `AVISIT` gives the categorical visit label used in the fixed-effects model, `ARMCD` is the treatment group, `RACE` and `SEX` are subject-level factors, `FEV1_BL` is the subject's baseline value, and `FEV1` is the repeated continuous outcome.

### Column roles

| Role | Example column | Practical notes |
|---|---|---|
| Subject ID | `USUBJID` | Identifies which rows belong to the same subject. Text IDs such as `PT1` are acceptable. |
| Visit/order variable | `VISITN` | Numeric order used to identify the repeated-measures order and covariance layout. |
| Visit label/factor | `AVISIT` | Categorical visit effect used in the mean model, for example `VIS1` to `VIS4`. |
| Treatment/group factor | `ARMCD` | Usually the main between-subject factor, for example placebo vs active treatment. |
| Baseline covariate | `FEV1_BL` | Usually repeated on all rows for the same subject; commonly included to improve precision. |
| Response | `FEV1` | Continuous repeated outcome. Rows with missing response are not analyzed as observed responses. |
| Other covariates | `RACE`, `SEX` | Subject-level or visit-level covariates included when scientifically or protocol justified. |

### Observed-response counts in the FEV1 example

| Visit | PBO observed | TRT observed | Total observed |
|---|---:|---:|---:|
| VIS1 | 68 | 66 | 134 |
| VIS2 | 69 | 71 | 140 |
| VIS3 | 71 | 58 | 129 |
| VIS4 | 67 | 67 | 134 |
| **Total** | **275** | **262** | **537** |

These counts are a useful reminder that MMRM does not require every subject to have all scheduled visits. It uses each subject's observed response vector and the matching part of the covariance structure.

---

## Practical data checks before fitting

Before running MMRM, review the dataset carefully. Many fitting and interpretation problems are caused by data-shape problems rather than by the MMRM algorithm itself.

### Minimum checks

- Confirm that there is **one row per subject per scheduled visit** or one row per observed subject-visit combination.
- Check for duplicate non-missing response records for the same subject and visit.
- Confirm that the subject ID is stable and unique across subjects.
- Confirm that the visit/order variable has the intended ordering.
- Confirm that treatment and categorical covariates have the intended reference levels.
- Check how many response values are missing by treatment and visit.
- Check for subjects with no non-missing responses after data screening.
- Inspect response distributions and outliers by treatment and visit.
- Confirm that baseline values are constant within subject when baseline is a subject-level covariate.

### Missingness pattern checks

A simple table of observed counts by treatment and visit should be created before fitting. Large imbalances, monotone dropout, or missingness concentrated in one treatment group do not automatically invalidate MMRM, but they should be understood and reported. MMRM relies on assumptions about the missing-data process; it does not prove those assumptions from the data.

---

## When to use MMRM

MMRM is usually a good candidate when all or most of the following are true:

- The outcome is continuous and approximately Gaussian, or can be made suitable through a prespecified transformation or sensitivity analysis.
- The same subject is measured repeatedly at planned visits or time windows.
- The analysis target is a population-average adjusted mean, treatment difference, change from baseline, or difference in change.
- Treatment or group comparisons should be adjusted for baseline value and other prespecified covariates.
- Some visits may be missing, but the analysis plan considers a likelihood-based MAR/ignorable-missingness framework appropriate.
- The covariance among repeated measurements is important and should be modeled rather than ignored.
- The analysis needs LS-means and visit-specific contrasts for reporting.

### Typical clinical-trial estimands

| Scientific question | Common MMRM output |
|---|---|
| What is the adjusted mean response in each treatment group at each visit? | LS-means by treatment and visit. |
| What is the treatment difference at the primary endpoint visit? | Visit-specific group contrast, for example `TRT - PBO` at `VIS4`. |
| Does treatment effect vary over time? | Treatment-by-visit interaction and visit-specific contrasts. |
| How much did each group change from baseline? | LS-mean change from baseline by treatment and visit. |
| Did treatment improve change from baseline compared with control? | Difference in change from baseline. |
| Are results sensitive to covariance assumptions? | Compare unstructured, heterogeneous AR(1), compound symmetry, and simpler structures where appropriate. |

### Non-clinical examples

MMRM is not limited to clinical trials. The same logic applies when:

- a laboratory value is measured repeatedly after a treatment or intervention,
- an agricultural outcome is measured at planned growth stages,
- a manufacturing quality characteristic is measured repeatedly on the same unit,
- a behavioral or educational score is measured over several scheduled assessments.

The key requirements are a continuous repeated outcome, a meaningful subject/unit identifier, and a scientific question about marginal means or contrasts over time.

---

## When not to use MMRM

MMRM should not be the default choice for every repeated or clustered dataset.

| Situation | Better direction |
|---|---|
| Binary outcome, such as response/non-response | Consider [GLM](../generalized-linear-models-glm.md) for independent data or [GEE](../generalized-estimating-equations-gee.md) for correlated marginal binary data. |
| Count outcome, such as number of events | Consider Poisson/negative-binomial GLM or GEE depending on the correlation structure and estimand. |
| Ordinal or nominal outcome | Use an ordinal or multinomial model where available and appropriate. |
| Time-to-event endpoint | Consider [Cox regression](../cox-regression.md) or survival-specific methods. |
| Primary interest is subject-specific random-effect variability | A random-effects LMM may be more appropriate, but that is not the user-facing MMRM workflow in this release. |
| Very few subjects relative to covariance parameters | Use a simpler covariance structure or reconsider the model. |
| Severe non-normality or influential outliers dominate the fit | Consider transformation, robust sensitivity checks, or a different outcome model. |
| Missingness is likely strongly MNAR | Use sensitivity analyses or explicit missing-data methods; MMRM alone is not sufficient. |

---

## MMRM versus repeated-measures ANOVA

Classic repeated-measures ANOVA is useful for simple balanced designs, but it is usually too restrictive for modern longitudinal endpoint analysis.

| Feature | Repeated-measures ANOVA | MMRM |
|---|---|---|
| Data shape | Often wide or balanced repeated format | Long format, one row per subject-visit |
| Complete repeated measurements | Often required or effectively required | Incomplete subjects can contribute observed responses |
| Covariance assumption | Often sphericity-oriented or restrictive | User-selectable subject-level covariance |
| Baseline adjustment | Possible but less central in simple workflows | Natural fixed-effect covariate |
| Visit-specific treatment effects | Possible but can be awkward | Natural through treatment-by-visit interaction and LS-means |
| Missing visits | Problematic in standard workflows | Handled by the likelihood under model assumptions |
| Main outputs | Omnibus tests | Fixed effects, covariance parameters, LS-means, contrasts, fit statistics, residuals |

Use repeated-measures ANOVA for small, balanced, simple teaching-style designs. Use MMRM when the study has planned longitudinal visits, covariate adjustment, incomplete follow-up, or visit-specific treatment estimands.

---

## MMRM versus GEE

Both MMRM and GEE are marginal approaches, but they are built on different estimating principles and are useful in different situations.

| Feature | MMRM | GEE |
|---|---|---|
| Main outcome type in BESH Stat NG | Continuous Gaussian repeated outcome | GLM-family outcomes, including Gaussian, binary, count, gamma, and others |
| Correlation handling | Direct likelihood covariance model | Working correlation in estimating equations |
| Estimation principle | ML or REML likelihood | Generalized estimating equations |
| Main estimands | Adjusted means, visit-specific differences, change from baseline, difference in change | Marginal regression effects for correlated/clustered data |
| Inference options | Model-based with DF adjustments such as Kenward-Roger, Satterthwaite, and between-within | Naive/robust/bias-reduced covariance options, depending on selected GEE settings |
| Missing data | Likelihood-based under ignorable/MAR assumptions conditional on the model | Depends on estimating-equation assumptions and any weighting or sensitivity strategy |
| Best use | Continuous longitudinal endpoints with scheduled visits | Correlated non-normal outcomes or marginal GLM-style effects |

A simple rule is: use MMRM for continuous scheduled repeated-measures endpoints where adjusted visit-specific means and contrasts are the primary outputs; use GEE when the response distribution or estimand calls for a marginal GLM approach.

---

## How missing data are handled conceptually

MMRM uses the observed response rows for each subject. If a subject has responses at `VIS2` and `VIS4` but not at `VIS1` or `VIS3`, the subject still contributes the observed two-dimensional response vector. The covariance matrix used for that subject is the submatrix corresponding to the observed visits.

### What MMRM does

- Uses all retained non-missing response observations.
- Allows different subjects to have different observed visit patterns.
- Estimates fixed effects and covariance parameters jointly under ML or REML.
- Provides likelihood-based inference under the model assumptions.

### What MMRM does not do

- It does **not** impute missing responses.
- It does **not** prove that missingness is missing at random.
- It does **not** automatically correct for informative dropout or MNAR mechanisms.
- It does **not** rescue a poorly specified mean model.
- It does **not** remove the need for missing-data sensitivity analyses in confirmatory work.

!!! warning "Missing-data interpretation"
    A valid MMRM analysis depends on the scientific plausibility of the missing-data assumptions, the adequacy of the mean model, and the selected covariance model. Missingness should be summarized and discussed, not hidden.

---

## From scientific question to MMRM specification

A good MMRM analysis starts with the estimand, not with the covariance menu.

### Step 1: Define the estimand

Examples:

- treatment difference at the final visit,
- treatment difference averaged over selected visits,
- change from baseline within each treatment group,
- difference in change from baseline,
- visit-specific treatment differences over time.

### Step 2: Choose the mean model

For the FEV1 example, a practical mean model is:

\[
\text{FEV1} \sim \text{RACE} + \text{SEX} + \text{ARMCD} \times \text{AVISIT}.
\]

For many clinical-trial analyses, baseline response is also included:

\[
\text{Response} \sim \text{Baseline} + \text{Treatment} \times \text{Visit} + \text{Prespecified covariates}.
\]

Sometimes baseline-by-visit interaction is included when the baseline effect is expected to vary over time or the analysis plan requires it.

### Step 3: Choose the covariance structure

Start from the design and sample size:

- **Unstructured** is often preferred when the number of visits is modest and the data support estimating all variances and covariances.
- **Heterogeneous AR(1)** can be useful when correlations decline with visit distance and variances may differ by visit.
- **Compound symmetry** can be useful as a simple sensitivity model when correlations are roughly constant.
- **Diagonal** may be useful as a diagnostic or sensitivity structure when within-subject covariance is weak, but it ignores residual correlation.

Detailed covariance definitions are documented in [Model and mathematics](model-and-mathematics.md) and [Options and output reference](options-and-output.md).

### Step 4: Choose estimation and inference settings

For routine fixed-effect inference in a clinical-trial-style MMRM, a common starting point is:

- estimation: **REML**,
- inference: **Kenward-Roger** or **Satterthwaite**,
- covariance: **Unstructured** when supported by the data,
- estimand reporting: LS-means and planned contrasts.

Between-within inference is also available and is useful for comparison with specific analysis plans or software workflows.

### Step 5: Interpret estimands, not only coefficient rows

The coefficient table is important for model diagnostics and reproducibility, but the primary interpretation should usually come from LS-means and contrasts. For example, in the FEV1 workflow, the treatment difference at `VIS4` is more directly interpreted from the `TRT - PBO` contrast at `VIS4` than from individual dummy-coded coefficient rows.

---

## Common analysis patterns

### Treatment difference at final visit

Use when the primary endpoint is a scheduled final visit.

- Mean model: baseline, treatment, visit, treatment-by-visit, prespecified covariates.
- Output: LS-means by treatment and visit; contrast at the final visit.
- Interpretation: adjusted treatment difference at the primary endpoint visit.

### Treatment difference over visits

Use when the trajectory over time matters.

- Mean model: include treatment-by-visit interaction.
- Output: visit-specific treatment contrasts.
- Interpretation: how the treatment effect changes across visits.

### Change from baseline

Use when the response scale is best interpreted relative to a baseline value.

- Mean model: include baseline response as a covariate, and optionally baseline-by-visit interaction when prespecified.
- Output: change from baseline by group and visit.
- Interpretation: adjusted change within each group.

### Difference in change from baseline

Use when the primary estimand is improvement or deterioration relative to baseline compared between groups.

- Mean model: same as above.
- Output: treatment difference in change from baseline.
- Interpretation: whether the active treatment improved the change trajectory compared with control.

### Covariance-structure sensitivity

Use when the primary result should be checked against reasonable covariance alternatives.

- Primary structure: often unstructured for modest visit counts.
- Sensitivity structures: heterogeneous AR(1), compound symmetry, heterogeneous compound symmetry, diagonal.
- Interpretation: whether the treatment estimate and standard error are stable across plausible covariance models.

---

## Practical checklist

Before considering an MMRM result final, check the following:

- The data are in long format and have the intended subject and visit variables.
- Missing response counts are summarized by treatment and visit.
- The mean model matches the scientific estimand and analysis plan.
- Categorical reference levels are correct.
- The covariance structure is appropriate for the number of visits and available data.
- The model converged without serious warnings.
- Covariance estimates are plausible and positive definite.
- LS-means and contrasts are aligned with the intended direction, for example `TRT - PBO`.
- Sensitivity checks are performed when required by the analysis plan.
- The report states the estimation method, inference method, covariance structure, and missing-data assumptions.

---

## See also

- [Model and mathematics](model-and-mathematics.md)
- [Excel ribbon workflow](user-interface.md)
- [Options and output reference](options-and-output.md)
- [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md)
- [Comparison with other software](software-comparison.md)
- [Examples and interpretation](examples.md)
- [Generalized Estimating Equations (GEE)](../generalized-estimating-equations-gee.md)
- [Generalized Linear Models (GLM)](../generalized-linear-models-glm.md)

---

## Source material for the example

- OpenPharma `mmrm` FEV1 dataset reference: <https://openpharma.github.io/mmrm/latest-tag/reference/fev_data.html>
- OpenPharma `mmrm` between-within vignette: <https://openpharma.github.io/mmrm/latest-tag/articles/between_within.html>
- OpenPharma `mmrm` methodological introduction: <https://openpharma.github.io/mmrm/latest-tag/articles/methodological_introduction.html>
