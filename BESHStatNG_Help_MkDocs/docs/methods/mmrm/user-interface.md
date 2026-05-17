# MMRM Excel ribbon workflow

[← Back to MMRM overview](../mixed-models-for-repeated-measures-mmrm.md)

**Purpose of this page:** show how to fit an MMRM from the Excel ribbon, using the FEV1 example data that accompanies this documentation. This page is written for practical users who want to know which columns to select, how to build the fixed-effect model, what the main options mean in the dialog, and where the main results appear. For the formal model definition, likelihood, covariance structures, and degrees-of-freedom methods, see [Model and mathematics](model-and-mathematics.md). For a full option-by-option reference, see [Options and output reference](options-and-output.md).

---

## Example used on this page

The screenshots use the FEV1 repeated-measures dataset included with this help project:

- [037mmrm_fev_data.csv](../../assets/data/037mmrm/037mmrm_fev_data.csv)
- [037mmrm_fev_results.xlsx](../../assets/data/037mmrm/037mmrm_fev_results.xlsx)

The source data contain scheduled FEV1 measurements at four visits. The example model is the same practical model used throughout the MMRM documentation: FEV1 is modeled using race, sex, treatment, visit, and treatment-by-visit interaction, with an unstructured within-subject covariance matrix.

In the screenshots, the numeric-coded columns are used in the dialog:

| Conceptual variable | Column used in screenshots | Role |
|---|---|---|
| Response | `FEV1` | continuous repeated outcome |
| Subject identifier | `SUBJID` | groups repeated rows from the same subject |
| Visit order | `VISITN` | numeric visit/time variable used to order within-subject observations |
| Treatment | `ARMCDN` | categorical fixed effect and grouping factor for contrasts |
| Race | `RACEN` | categorical fixed effect |
| Sex | `SEXN` | categorical fixed effect |
| Visit fixed effect | `VISITN` | categorical fixed effect in the mean model |

!!! note "Numeric-coded factors in the ribbon workflow"
    The MMRM ribbon model builder expands categorical factors from numeric-coded columns. Keep a codebook for columns such as `ARMCDN`, `RACEN`, `SEXN`, and `VISITN`, because coefficient labels and contrast labels use the numeric levels. In the FEV1 example, level `1` is the reference level for binary factors because the current reference coding uses the smallest observed numeric level by default.

---

## Opening the method

Open the active worksheet that contains the analysis dataset, then choose:

**BESH Stat NG → Analyse → Regression → Mixed Models for Repeated Measures (MMRM)**

The dialog opens with three tabs:

1. **Select Variables** — choose the response, subject ID, visit/time column, and source variables for the fixed-effect model.
2. **Build Model** — construct the fixed-effect formula from selected source variables.
3. **Options** — choose the fit method, covariance structure, inference method, LS-means, contrasts, and optimizer options.

The **Fit** button runs the model and writes a new output workbook. The original data worksheet is not overwritten.

---

## Step 1: Select Variables tab

![MMRM Select Variables tab with FEV1 example columns](../../assets/images/037mmrm/037mmrm_input.png)

Use this tab to tell BESH Stat NG which worksheet columns play each analysis role. Highlight one or more columns in **Worksheet Columns** and move them into the appropriate box with `>>`. Use `<<` to remove a column from a role.

### Required selections

| Dialog field | Example selection | Required? | Meaning |
|---|---|---:|---|
| **Dependent Variable (Outcome)** | `FEV1` | Yes | The continuous repeated response. Exactly one response variable must be selected. |
| **Subject ID** | `SUBJID` | Yes | Identifies rows that belong to the same subject. Exactly one subject ID must be selected. |
| **Visit / Time** | `VISITN` | Yes | Numeric visit/time ordering used to sort observations within subject and build the residual covariance matrix. |
| **Fixed-Effect Source Variable(s)** | `ARMCDN`, `RACEN`, `SEXN`, `VISITN` | Usually yes | Raw columns that will be used to create fixed-effect main effects, factors, polynomials, and interactions on the **Build Model** tab. |

!!! tip "When visit is part of the mean model"
    A visit column can be used twice: once as **Visit / Time** and once as a **Fixed-Effect Source Variable**. In the FEV1 example, `VISITN` orders the repeated measurements and is also added as a categorical fixed effect on the Build Model tab.

### Practical selection rules

- Use **long-format data**: one row per subject and visit/time point.
- The response and visit/time columns must be numeric.
- The subject ID may be text or numeric. Text identifiers such as `PT1`, `USUBJID-004`, or `Site A` are allowed as the grouping variable.
- Categorical predictors in the ribbon workflow should be numeric-coded and then added as categorical factors on the Build Model tab.
- Select every raw variable needed for the fixed-effect model before building effects. For example, to fit treatment-by-visit interaction, both the treatment column and the visit column must be available as fixed-effect source variables.
- Avoid filtering the worksheet to complete cases unless the analysis plan explicitly requires complete cases. Rows with missing analysis values are excluded from the fitted model, but subjects do not need to have every scheduled visit.
- Use **Reload Sheet Data** after changing the active sheet or editing column headers so the dialog refreshes the available worksheet columns.

### What the FEV1 screenshot selects

The screenshot sets up the following model-building inputs:

| Role | Selected column |
|---|---|
| Response | `FEV1` |
| Subject ID | `SUBJID` |
| Visit/time | `VISITN` |
| Fixed-effect sources | `ARMCDN`, `RACEN`, `SEXN`, `VISITN` |

At this stage, the fixed-effect model has not been created yet. The source variables have only been made available for the next tab.

---

## Step 2: Build Model tab

![MMRM Build Model tab showing race, sex, treatment, visit, and treatment-by-visit interaction](../../assets/images/037mmrm/037mmrm_build_model.png)

Use the **Build Model** tab to convert the source variables into fixed effects. The left panel lists the variables selected on the previous tab. The right panel lists the fixed effects that will be included in the fitted mean model.

### Model-building buttons

| Control | Use it for | Practical guidance |
|---|---|---|
| **Add >>** | Continuous main effects | Use for numeric covariates that should enter linearly, such as baseline value, age, weight, or a continuous time trend. |
| **Add as Categorical Factor >>** | Categorical main effects | Use for treatment, visit, sex, race, site, region, or any numeric-coded grouping variable. The design matrix uses reference coding. |
| **Poly >>** | Polynomial terms | Use only when a continuous predictor should enter with powers such as squared or cubic terms. Do not use this as a substitute for a categorical visit effect unless that is the planned model. |
| **2-way Interactions >>** | Pairwise interactions among selected variables | Use for common terms such as treatment × visit. If several variables are highlighted, all requested two-way interactions are created. |
| **Custom Interaction >>** | One custom multi-variable interaction | Use when a specific interaction should be created from the selected variables. |
| **Intercept** | Model intercept | Keep enabled for ordinary MMRM analyses. Disable only when the analysis plan explicitly requires a no-intercept parameterization. |
| **Remove** | Delete selected effects | Removes highlighted effects from the fixed-effect list. |
| **Clear All** | Clear the fixed-effect list | Useful when rebuilding the model from scratch. |

### FEV1 example fixed-effect model

The screenshot builds the model:

```text
FEV1 ~ RACEN + SEXN + ARMCDN + VISITN + ARMCDN:VISITN
```

where `RACEN`, `SEXN`, `ARMCDN`, and `VISITN` are all treated as categorical factors. In the selected-effects list this appears as:

```text
[Cat] RACEN | VarG
[Cat] SEXN | VarI
[Cat] ARMCDN | VarE
[Cat] VISITN | VarM
"ARMCDN | VarE":"VISITN | VarM"
```

Because the intercept is enabled, the smallest observed numeric level of each categorical factor is omitted as the reference level. The fixed-effect table in the output therefore shows coefficients such as `ARMCDN[2]`, `VISITN[2]`, `VISITN[3]`, and `VISITN[4]`, each interpreted relative to its reference level and conditional on the other terms in the model.

!!! tip "Treatment-by-visit interaction is usually central"
    In a clinical-trial-style MMRM, treatment-by-visit interaction is often the term that allows treatment differences to vary by visit. Without that interaction, the model imposes a more restrictive common treatment effect across visits.

### Common fixed-effect patterns

| Goal | Typical fixed effects |
|---|---|
| Visit-specific treatment difference | treatment + visit + treatment × visit |
| Adjusted treatment difference | baseline + treatment + visit + treatment × visit + stratification factors |
| Baseline effect allowed to vary by visit | baseline + baseline × visit + treatment + visit + treatment × visit |
| Descriptive repeated-measures profile with no treatment comparison | visit + important covariates |
| Sensitivity model with continuous time trend | treatment + time + treatment × time, where time is intentionally continuous |

The model should follow the study protocol or analysis plan. Do not add interaction terms simply because they are available; add them because they represent the planned estimand or a scientifically meaningful adjustment.

---

## Step 3: Options tab

![MMRM Options tab with REML, unstructured covariance, between-within inference, LS-means, and contrast options](../../assets/images/037mmrm/037mmrm_options.png)

The **Options** tab controls estimation, inference, post-estimation output, and optimizer behavior. The screenshot shows the settings used for the example output workbook: **REML**, **Unstructured** residual covariance, and **Between-within DF** inference. The default inference method in the dialog may be **Kenward-Roger**; choose **Between-within DF** when reproducing the specific between-within example shown in these screenshots.

### Model specification

| Option | Common choice | Meaning |
|---|---|---|
| **Fit Method** | `REML` | Estimates covariance parameters using restricted maximum likelihood. This is the usual default for fixed-effect inference in MMRM. |
| **Residual Covariance Structure** | `Unstructured` | Allows each visit variance and each pairwise visit covariance to be estimated separately. Use when there are enough subjects and planned visits are not too numerous. Toeplitz and heterogeneous Toeplitz are useful alternatives when correlations mainly depend on visit lag. |
| **Inference Method** | `Kenward-Roger`, `Satterthwaite`, or `Between-within DF` | Determines how denominator degrees of freedom, standard errors, and p-values are calculated for fixed effects and contrasts. |
| **Compute Residuals** | checked when diagnostics are needed | Adds residuals to the output **Data** sheet. |
| **alpha** | `0.050` | Significance level used for two-sided confidence intervals. |

!!! note "Kenward-Roger and REML"
    If **Kenward-Roger** inference is selected together with **ML**, the dialog changes the fit method to **REML** and displays a message explaining that Kenward-Roger inference requires REML. Use ML mainly when comparing models with different fixed-effect specifications.

### Post-estimation output

| Option | Meaning |
|---|---|
| **Show class-level information** | Adds a table showing the subject ID and, when available, the grouping factor used for LS-means and contrasts. |
| **Show LS-means / estimated marginal means** | Adds estimated marginal mean tables. These are usually the most important practical summaries from an MMRM. |
| **Compare groups by** | Selects the grouping factor used for group-specific LS-means and contrasts. Use an explicit variable such as `ARMCDN` to avoid ambiguity. |
| **Comparison type** | Chooses whether to show no group contrasts, all pairwise group contrasts, each group versus a control level, or one selected comparison. |
| **Reference/control level** | Defines the control group for control-based comparisons. `(First)` uses the first sorted numeric group level. |
| **Comparison level** | Used when **Selected comparison only** is chosen. |
| **Difference direction** | Controls the sign of the reported difference, for example higher level minus lower level or treatment minus control. |
| **Multiplicity** | Applies no adjustment, Bonferroni, Holm, or Sidak adjustment to the selected family of comparisons. |
| **Baseline visit for change** | Defines the baseline visit used for change-from-baseline tables. `(Smallest)` uses the smallest numeric visit value. |
| **Show change from baseline** | Adds estimated change-from-baseline tables. |
| **Show group difference in change** | Adds treatment/group differences in change from baseline when a grouping factor and contrasts are available. |

See [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md) for a deeper explanation of which post-estimation table corresponds to each common estimand.

### How LS-means are averaged

| Option | Meaning |
|---|---|
| **Observed design grid** | Estimates means using the observed design rows. This is often the most direct workflow for the ribbon example. |
| **Reference grid** | Builds a model-based grid of class levels and covariate reference values, similar in spirit to estimated marginal means workflows in other software. |
| **Class-cell weighting** | Controls how class cells are weighted when using the reference grid. |
| **Continuous covariates** | Controls whether continuous covariates are set at observed means or zero when using the reference grid. |

The class-cell weighting and continuous-covariate options are enabled when **Reference grid** is selected.

### Convergence and optimizer options

| Option | Default / common value | Meaning |
|---|---|---|
| **Convergence Criterion** | `1E-06` | Stopping tolerance applied to optimizer convergence checks. |
| **Max. Iterations** | `500` | Maximum number of optimizer iterations before the fit stops. |
| **Iterations Details** | unchecked | Adds more iteration detail to the main output. |
| **Trace Execution** | unchecked | Writes optimizer trace information to an **MMRM Trace** sheet when trace text is available. |
| **Diagnostic** | unchecked | Adds diagnostic output tables to the main **MMRM** sheet. |
| **Optimizer mode** | `AI/Fisher scoring (default)` | Main covariance-parameter optimizer. Projected BFGS alternatives are available for sensitivity or difficult fits. |
| **Gradient mode** | `Auto (analytic where available)` | Chooses how gradients are obtained for the covariance optimizer. Analytic and finite-difference modes are available for diagnostics. |

For routine analyses, keep the default optimizer and gradient settings. Change them mainly when investigating convergence, validating derivatives, or running sensitivity checks.

---

## Step 4: Fit the model

Click **Fit** after the variables, fixed effects, and options are set.

During fitting, the dialog validates the selections, imports the retained analysis rows, expands the fixed-effect design matrix, sorts observations within subject by the visit/time variable, fits the MMRM, creates optional LS-mean and contrast tables, and writes a new output workbook.

The progress area reports elapsed time and, for longer fits, optimization status such as objective-function value, objective change, and gradient norm. The **Interrupt** button requests a graceful interruption. When interruption succeeds, the output is based on the latest accepted estimates rather than a fully converged final fit; treat such output as diagnostic unless there is a specific reason to use it.

### Validation messages users may see

| Message or situation | What to check |
|---|---|
| “Please select exactly one continuous response variable.” | Move exactly one response column into **Dependent Variable (Outcome)**. |
| “Please select a Subject ID variable.” | Move exactly one subject identifier into **Subject ID**. |
| “Please select a Visit / Time variable for MMRM.” | Move a numeric visit/order column into **Visit / Time**. |
| “No fixed effects were specified and the intercept is disabled.” | Add at least one fixed effect or enable the intercept. |
| “Please select a residual covariance structure.” | Choose a covariance structure on the **Options** tab. |
| “Convergence epsilon must be positive.” | Enter a positive convergence criterion. |
| “Maximum iterations must be positive.” | Enter a positive integer for **Max. Iterations**. |
| “No valid observations.” | Check missing response, missing selected predictors, nonnumeric columns, and any worksheet filters or blank rows. |

---

## Output workbook

The ribbon workflow creates a new workbook with output sheets. The exact set of tables depends on the options selected.

| Sheet | Created when | Contents |
|---|---|---|
| **Data** | always | Retained analysis rows, response, expanded fixed-effect design columns, subject ID, visit/time, fitted marginal values, and optional residuals. |
| **MMRM** | always | Model specification, convergence information, fixed effects, covariance parameters, covariance/correlation matrices, fit statistics, optional diagnostics, LS-means, and contrasts. |
| **MMRM Trace** | when trace output is requested and available | Iteration or optimizer trace information for diagnostic review. |

### Fixed-effect table

![MMRM output fixed-effect table](../../assets/images/037mmrm/037mmrm_results1.png)

The fixed-effect table gives the coefficient estimate, standard error, denominator degrees of freedom, test statistic, p-value, and confidence interval for each fixed-effect coefficient. In the screenshot, categorical variables are shown using numeric-coded levels, for example `ARMCDN[2]` and `VISITN[4]`.

Use this table to check that the intended model was fitted, but do not rely only on individual coefficient rows for the main study interpretation. For treatment comparisons at each visit, the LS-means and contrast tables are usually more directly interpretable.

!!! tip "Why coefficient labels may not look like treatment labels"
    The ribbon workflow uses numeric-coded categorical factors. If `ARMCDN = 1` means placebo and `ARMCDN = 2` means treatment, the coefficient `ARMCDN[2]` refers to level 2 relative to the omitted level 1, subject to the rest of the model parameterization. Keep the codebook beside the output.

### Fit statistics

![MMRM output fit statistics table](../../assets/images/037mmrm/037mmrm_results2.png)

The fit statistics table summarizes the fitted model. In the FEV1 example it shows **537** retained observations and **197** subjects, matching the non-missing FEV1 analysis rows. It also reports the fit method, execution time, number of fixed-effect parameters, objective value, log-likelihood, information criteria, and REML diagnostics.

For REML fits, information criteria are diagnostic for a given fixed-effect specification. If you need to compare models with different fixed-effect specifications, fit those candidate models using ML rather than REML.

### Estimated covariance and correlation matrices

![MMRM output estimated R covariance and correlation matrices](../../assets/images/037mmrm/037mmrm_results3.png)

The covariance and correlation matrices show the fitted within-subject residual covariance structure. For an MMRM without random effects, this matrix is the fitted marginal within-subject covariance matrix. In the screenshot, the selected structure is **Unstructured**, so every visit variance and every visit-pair covariance is estimated directly from the selected model.

Use these tables to check whether the selected covariance structure is plausible. Very small variances, very large correlations, or near-singular patterns may indicate that the covariance structure is too ambitious for the data.

---

## Practical beginner workflow

For a common treatment-by-visit MMRM:

1. Arrange the data in long format.
2. Open **Mixed Models for Repeated Measures (MMRM)** from the ribbon.
3. Select the continuous response, subject ID, and numeric visit/time variable.
4. Add treatment, visit, and planned covariates as fixed-effect source variables.
5. On **Build Model**, add treatment and visit as categorical factors.
6. Add the treatment × visit interaction.
7. Add baseline and other planned covariates if they are part of the analysis plan.
8. Keep the intercept enabled.
9. On **Options**, use REML and start with an unstructured covariance when the number of visits and sample size support it.
10. Choose the inference method required by the analysis plan. For small-sample adjusted inference, use Kenward-Roger or Satterthwaite where appropriate.
11. Enable LS-means and select the treatment variable under **Compare groups by**.
12. Run the model and interpret visit-specific LS-means and treatment contrasts.

---

## Troubleshooting

### Some columns do not appear or cannot be used as factors

The ribbon model builder is designed around numeric analysis columns. For categorical predictors, use numeric-coded columns and keep the text labels in a codebook or adjacent worksheet columns. For example, use `ARMCDN` in the model and retain `ARMCD` to document that level `1` is placebo and level `2` is treatment.

### The number of analyzed observations is smaller than the source data

Rows with missing values in selected analysis columns are not retained as observed analysis rows. In the FEV1 example, the source dataset has scheduled rows for all subject-visit combinations, but the fitted model uses the rows with non-missing FEV1 and valid selected model inputs.

### The unstructured covariance model does not converge

An unstructured covariance matrix is flexible but parameter-heavy. Check the number of subjects per visit, missingness patterns, sparse treatment-by-visit cells, outliers, and duplicate subject-visit records. If the structure is not supported by the data, try a simpler structure such as heterogeneous AR(1), Toeplitz, heterogeneous Toeplitz, compound symmetry, heterogeneous compound symmetry, diagonal heterogeneous, or identity as a sensitivity analysis.

### The covariance matrix appears near singular

A near-singular covariance estimate often means that the selected covariance structure is too flexible for the observed data pattern, or that some visit pairs have weak support. Review observed counts by visit and group, simplify the covariance structure, and check whether the visit variable has been coded and ordered correctly.

### LS-means or group contrasts are missing

Check that **Show LS-means / estimated marginal means** is enabled. For group contrasts, also check that **Compare groups by** is set to the intended grouping factor, that the grouping factor has at least two observed levels, and that **Comparison type** is not set to **None**. Use an explicit grouping factor such as `ARMCDN` rather than relying on `(Auto)` when several categorical factors are in the model.

### Kenward-Roger was requested with ML

Kenward-Roger inference is REML-based in this implementation. If **ML** is selected and **Kenward-Roger** is requested, the dialog changes the fit method to **REML** for that analysis and displays an information message.

### Results differ from another software package

First check whether the same analysis rows, fixed-effect coding, reference levels, covariance structure, fit method, inference method, contrast direction, and LS-means averaging method were used. Differences between REML and ML, between-within and Kenward-Roger degrees of freedom, or observed-grid and reference-grid LS-means can change standard errors, degrees of freedom, p-values, and contrast estimates.

---

## See also

- [MMRM concepts and use cases](concepts-and-use-cases.md)
- [Model and mathematics](model-and-mathematics.md)
- [Options and output reference](options-and-output.md)
- [LS-means, contrasts, and estimands](lsmeans-and-contrasts.md)
- [Worksheet functions](worksheet-functions.md)
- [Examples and interpretation](examples.md)
