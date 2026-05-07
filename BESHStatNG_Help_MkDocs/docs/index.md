# BESHStatNG Help

**BESHStatNG** (New Generation) is a VB.NET reimplementation of the original BESHStat VBA add-in. It adds the **BESH Stat NG** ribbon tab with dialogs for statistical tests, plots, regression models, and sample size tools.

## Why BESHStatNG exists (and why it replaces the old VBA add-in)

BESHStatNG (“New Generation”) is a complete reimplementation of the original BESHStat add-in that was written in VBA (`.xlam`).  
The goal was not to change what users *can do*, but to make the add-in **more reliable, easier to maintain, and easier to extend**.

### What changes for you as a user

**1) Better compatibility with modern Excel (especially 64-bit)**  
VBA solutions often require extra care across Excel versions and between 32-bit/64-bit environments.  
A compiled `.xll` add-in (built with Excel-DNA) is designed for modern Excel installations and is easier to keep consistent.

**2) Fewer “macro security” obstacles**  
`.xlam` add-ins are treated like macro-enabled workbooks, so users can run into macro policy blocks, warnings, or restricted environments.  
BESHStatNG is a compiled add-in and does not depend on workbook macros. (The installer may still show a Windows warning if it is not code-signed.)

**3) More stable UI and input handling**  
BESHStatNG uses dedicated Windows Forms dialogs and consistent data import rules.  
This reduces “worksheet-state” issues that can happen with macro-driven tools and makes workflows more predictable.

**4) Performance and scalability**  
Compiled code can handle larger datasets and more complex workflows more efficiently than macro code, especially for methods that require repeated computation.

**5) Easier updates and faster delivery of new methods**  
The NG architecture makes it simpler to add new statistical methods, improve existing ones, and release updates without changing workbook logic.

**6) Better diagnostics when something goes wrong**  
NG keeps internal logs that help track down issues (useful when reporting a bug), rather than failing silently or with generic VBA errors.

!!! tip
    Need to report a bug or suggest an improvement? Please use the [GitHub issue tracker](https://github.com/PeterSlezak/BESHstatNG/issues) so the problem can be tracked, reproduced, and linked to fixes.

### In short
BESHStatNG keeps the same “Excel-first” workflow, but moves the engine from an old macro-based add-in (`.xlam`) to a modern compiled add-in (`.xll`) so it can grow and remain dependable.

## Quick start

1. Install the add-in using the `.msi` installer → see [Getting started](getting-started.md).
2. Open Excel and go to **BESH Stat NG → Analyse**.
3. Choose a method, select the required data (range picker or variable list), and click **Run**.

!!! tip "New here? Read these two pages first"
    - [Getting started](getting-started.md) — install, SmartScreen/Trust Center issues, logs
    - [How to select data](data-selection.md) — range selection vs variable-by-column selection

## How results are written

Most methods write results into the active workbook:

- Many analyses create a **new worksheet** for results.
- Some workflows write results into a chosen area and will refuse to overwrite non-empty cells.

If you unexpectedly get an extra sheet or the tool refuses to write, check that you have a clear area or allow it to create a new sheet.

---

## Implementation notes

BESHStatNG is a lightweight, self-contained `.xll` add-in. If you're curious how results are computed (matrix algebra, SVD, exact p-values, etc.), see **[Implementation notes](implementation-notes.md)**.

## Methods

Choose a method below. Each page explains what the method does, what data it expects, and how to interpret the output.

### Assumptions
- [Normality Tests](methods/normality-tests.md) — *Shapiro–Wilk • D’Agostino–Pearson K² • Anderson–Darling.* Check whether a sample distribution is consistent with normality—useful before parametric tests.
- [Univariate Outliers](methods/univariate-outliers.md) — *Grubbs’ test (single outlier) • Rosner / generalized ESD (multiple outliers).* Identify statistically unusual values in a single variable, with options suited to one or multiple outliers.
- [Homogeneity of Variance](methods/homogeneity-of-variance.md) — *Fligner–Killeen • Levene (Brown–Forsythe/median) • Squared Ranks • Bartlett.* Test whether multiple groups have similar variances, which is an important assumption for many parametric comparisons.
- [Symmetry](methods/symmetry.md) — *Miao–Gel–Gastwirth test • Cabilio–Masaro test • Asymmetry plot (optional).* Evaluate whether a distribution is symmetric around its center and visualize asymmetry when needed.
- [Descriptive Statistics](methods/descriptive-statistics.md) — *n • mean • median • SD / variance • SEM • CV • skewness / kurtosis • Q1 / Q3, IQR • min / max / range • Shapiro–Wilk (optional).* Compute a compact set of summary statistics for one or multiple variables, optionally adding a normality check.

### Graphics
- [Histogram](methods/histogram.md) — *Automatic bin rules (Sturges, Doane, Scott, Freedman–Diaconis) • Overlay multiple groups (optional).* Create publication-friendly histograms with sensible bin-width heuristics and optional overlays.
- [Box and Whiskers](methods/box-and-whiskers.md) — *Tukey boxplot (median, quartiles, whiskers) • Outliers via 1.5×IQR rule.* Visualize distribution shape and outliers across one or more groups.
- [ROC Curve](methods/roc-curve.md) — *ROC curve points, AUC (Wilcoxon), DeLong SE / CI, Hanley–McNeil SE / CI, p-value, Cutoff table.* Assess binary classifier performance and explore sensitivity/specificity tradeoffs across thresholds.
- [Kaplan-Meier Plot](methods/kaplan-meier-plot.md) — *Kaplan–Meier survival estimate • Optional confidence bands • Tabular survival output (optional).* Plot survival over time and summarize survival probabilities (with censoring) for one or multiple groups.
- [Normal Plot](methods/normal-plot.md) — *Rank methods: Blom, Rankit, Van der Waerden • Line fits: SPSS, OLS, R-style • Optional descriptive stats.* Create a normal probability plot to visually assess normality and identify deviations such as skewness or heavy tails.
- [XYZ 3D Scatterplot](methods/xyz-3d-scatterplot.md) — *3D scatter plot (X,Y,Z) • Optional group coloring • Optional point labels • Rotation/zoom + plane projections (optional).* Visualize three-dimensional relationships with interactive rotation controls.
- [Scatter Plot Matrix](methods/scatter-plot-matrix.md) — *Scatter plot matrix • Optional correlation coefficients • Optional regression lines.* Quickly explore pairwise relationships among many variables.

### Parametric
- [Paired (single sample) T tests](methods/paired-t-tests.md) — *Paired t-test (matched pairs).* Compare two paired measurements on the same subjects by testing the mean of differences.
- [Unpaired (two sample) T tests](methods/unpaired-two-sample-t-tests.md) — *Pooled-variance t-test • Welch t-test • F-test for variances (optional).* Compare the means of two independent groups, either assuming equal variances or allowing unequal variances.
- [One-Way ANOVA](methods/one-way-anova.md) — *Classic one-way ANOVA • Welch ANOVA • Post-hoc: LSD, Bonferroni, Tukey–Kramer, Games–Howell.* Compare means across multiple independent groups and run common post-hoc procedures when differences are detected.
- [One-Way Repeated-Measures ANOVA](methods/one-way-repeated-measures-anova.md) — *RM ANOVA • Sphericity: Mauchly test (optional) • Corrections: Greenhouse–Geisser, Huynh–Feldt (optional) • Post-hoc: Tukey (optional) • Descriptive stats / box plot (optional).* Compare means across repeated conditions for the same subjects, with optional sphericity diagnostics and corrections.
- [Two-Way Nested ANOVA](methods/two-way-nested-anova.md) — *Two-way nested ANOVA • Balanced design check • Satterthwaite approximation.* Analyze hierarchical (nested) designs such as subjects nested within centers, with appropriate variance decomposition.

### Nonparametric
- [Mann-Whitney Test](methods/mann-whitney-test.md) — *Exact p-values (when available) • Normal approximation (ties + continuity correction) • Hodges–Lehmann shift estimate (optional).* Nonparametric alternative to the two-sample t-test for comparing two independent groups.
- [Wilcoxon Signed Rank Test](methods/wilcoxon-signed-rank-test.md) — *Exact p-values (when available) • Normal approximation (ties + continuity correction) • Hodges–Lehmann shift estimate • Sign test (optional).* Nonparametric paired comparison test for matched samples or before/after measurements.
- [Kruskal-Wallis Test](methods/kruskal-wallis-test.md) — *H statistic (uncorrected) • H statistic (tie-corrected) • Dunn’s post-hoc comparisons (optional).* Nonparametric one-way ANOVA alternative for comparing more than two independent groups.
- [Friedman Test](methods/friedman-test.md) — *Friedman test • Post-hoc multiple comparisons (MCP) • Descriptive stats (optional) • Box plot (optional).* Nonparametric repeated-measures alternative to one-way RM ANOVA for ranked data across conditions.
- [Cochran's Q Test](methods/cochrans-q-test.md) — *Cochran’s Q • Percent of 1’s per condition.* Test differences in matched binary outcomes across three or more conditions (extension of McNemar).
- [Skillings-Mack Test](methods/skillings-mack-test.md) — *Skillings–Mack test (handles missing values).* Nonparametric test for block designs like Friedman, but robust to missing observations.
- [Spearman Rank Correlation](methods/spearman-rank-correlation.md) — *Spearman’s ρ (rank correlation) • p-value / CI where applicable.* Measure monotonic association between two variables using ranks (robust to non-normality and outliers).
- [Kendall's Rank Correlation](methods/kendalls-rank-correlation.md) — *Kendall’s τ • p-value / CI where applicable.* Measure ordinal association between two variables, with good behavior under ties and small samples.
- [Theil-Sen Simple Regression](methods/theil-sen-simple-regression.md) — *Median slope (Theil–Sen) • CI at the selected level (Sen/large-sample approx.) • Robust intercept.* Robust simple linear regression resistant to outliers, based on median pairwise slopes.

### Contingency Table Analysis
- [2x2 Table](methods/2x2-table.md) — *Pearson chi-square • Fisher’s exact (two-sided, one-sided, mid-p) • Odds ratio + CI • Relative risk + CI • McNemar / Liddell test for paired 2×2.* Analyze 2×2 contingency tables with exact tests and common effect-size measures.
- [RxC Table](methods/rxc-table.md) — *Pearson chi-square test of independence • Nominal association (Cramer's V, Phi, contingency coefficient) • Fisher–Freeman–Halton exact test (optional) • Ordinal association (tau-b/tau-c, gamma, Somers’ D) • Cochran–Armitage trend test (when applicable).* Analyze general contingency tables and report association measures and exact/trend tests where applicable.
- [Mantel-Haenszel Test](methods/mantel-haenszel-test.md) — *Mantel–Haenszel chi-square • Pooled odds ratio + confidence interval at the selected level.* Combine stratified 2×2 tables to estimate a pooled association while controlling for a stratification factor.
- [Proportions](methods/proportions.md) — *Single proportion (estimate + CI) • Two independent proportions (difference + CI, Fisher exact p-values) • Two paired proportions (difference + CI, Liddell/McNemar-type test).* Work with binomial outcomes: estimate proportions and compare proportions between groups.
- [Correspondence Analysis](methods/correspondence-analysis.md) — *Correspondence analysis (CA) • Row/column contribution plots • Biplot.* Explore structure in contingency tables via low-dimensional map representations of rows and columns.

### Survival Analysis
- [Kaplan-Meier Plot](methods/kaplan-meier-plot.md) — *Kaplan–Meier survival estimate • Optional confidence bands • Tabular survival output (optional).* Plot survival over time and summarize survival probabilities (with censoring) for one or multiple groups.
- [Logrank Test](methods/logrank-test.md) — *Logrank • Tarone–Ware • Gehan–Breslow • Peto • Modified Peto (Andersen).* Compare survival curves between groups using logrank-type tests (with several common weightings).
- [Cox Regression](methods/cox-regression.md) — *Cox proportional hazards model • Tie handling: Breslow, Efron, Exact • Robust variance (optional) • Residuals + PH score test (optional) • Baseline + adjusted survival curves.* Fit a proportional hazards model to time-to-event data and report hazard ratios with diagnostics.

### Regression
- [Multiple Linear Regression (LM)](methods/multiple-linear-regression-lm.md) — *OLS multiple regression • Optional weights • Type I or Type III term SS (ANOVA table) • Optional covariance matrix, residuals, VIF, and partial r.* Fit a standard linear regression model, report coefficients and ANOVA-style term tests.
- [Generalized Linear Models (GLM)](methods/generalized-linear-models-glm.md) — *GLM families: Gaussian, Binomial, Poisson, Negative Binomial, Gamma • Links per family (selectable) • Optional weights and offset • IRLS with user-set iterations/ε • Optional covariance matrix and residuals.* Fit generalized linear models to a wide range of outcome types with configurable link functions.
- [Negative Binomial Regression (NB2)](methods/negative-binomial-regression-nb2.md) — *Negative Binomial (NB2) regression • Overdispersion parameter (estimated) • Optional offset/weights, covariance matrix, residuals.* Model overdispersed count outcomes using an NB2 variance function.
- [Generalized Estimating Equations (GEE)](methods/generalized-estimating-equations-gee.md) — *Families: Gaussian, Binomial, Poisson, Negative Binomial, Gamma • Covariance structures: Independence, Exchangeable, Autoregressive, Unstructured • SE types: Robust, Naive, Bias-reduced.* Fit marginal models for correlated/clustered data using GEE with selectable working correlation.
- [Cox Regression](methods/cox-regression.md) — *Cox proportional hazards model • Tie handling: Breslow, Efron, Exact • Robust variance (optional) • Residuals + PH score test (optional) • Baseline + adjusted survival curves.* Fit a proportional hazards model to time-to-event data and report hazard ratios with diagnostics.
- [Zero-Inflated Poisson Regression](methods/zero-inflated-poisson-regression.md) — *ZIP model (Poisson count + logistic inflation part) • EM-style fitting (with iterations/ε) • Optional starting values.* Model count data with excess zeros by combining a Poisson model with a separate zero-inflation process.
- [Multinomial Logistic Regression](methods/multinomial-logistic-regression.md) — *Multinomial logistic regression • Reference category selection (first/last) • Optional offset/weights, covariance matrix, residuals.* Model nominal outcomes with more than two categories.
- [Ordinal Logistic Regression](methods/ordinal-logistic-regression.md) — *Ordinal logistic regression • Reference category selection (first/last) • Optional offset/weights, covariance matrix, residuals.* Model ordered categorical outcomes using an ordinal logistic framework.

### Multivariate Analysis
- [Hotelling's T-Squared Test](methods/hotellings-t-squared-test.md) — *One-sample Hotelling’s T² • Two-sample (independent) Hotelling’s T² • Paired Hotelling’s T² • Simultaneous confidence intervals.* Multivariate extension of the t-test for comparing mean vectors (one-sample, two-sample, or paired).
- [Principal Component Analysis](methods/principal-component-analysis.md) — *PCA on correlation or covariance matrix • Component extraction: eigenvalue, fixed k, variance threshold • Outputs: scores, loadings, reduced dataset • Plots: scree, score/loading plots, biplots (2D/3D).* Reduce dimensionality of multivariate numeric data and visualize dominant patterns.
- [Correspondence Analysis](methods/correspondence-analysis.md) — *Correspondence analysis (CA) • Row/column contribution plots • Biplot.* Explore structure in contingency tables via low-dimensional map representations of rows and columns.
- [Multiple Correspondence Analysis](methods/multiple-correspondence-analysis.md) — *Multiple correspondence analysis (MCA) via indicator/Burt matrix • Contribution plots • Biplot.* Extend correspondence analysis to multiple categorical variables (survey-style data).
- [K-Means Clustering](methods/k-means-clustering.md) — *K-means clustering for numeric data.* Partition observations into a requested number of compact clusters by minimizing the total within-cluster sum of squared Euclidean distances.
- [Hierarchical Clustering](methods/hierarchical-clustering.md) — *Agglomerative hierarchical clustering • Dendogram.* Build a full bottom-up clustering hierarchy and inspect it through merge tables, membership cuts, and a dendrogram.
- [Factor Analysis](methods/factor-analysis.md) — *Exploratory factor analysis on correlation or covariance matrix, extraction methods, retention by fixed count / eigenvalue cutoff / cumulative variance, rotations, regression or Bartlett scores, factorability diagnostics.* Discover lower-dimensional latent structure behind correlated numeric variables and summarize the common variance they share.
- [Discriminant Analysis](methods/discriminant-analysis.md) — *Linear and quadratic discriminant analysis, group priors, optional preprocessing, posterior probabilities, classification tables, canonical discriminant functions, and leave-one-out / k-fold / holdout validation.*.

### Sample Size
- [Sample Size – Paired T-test](methods/sample-size-paired-t-test.md) — *Sample size for paired t-test (iterative t critical values).* Estimate required number of pairs for a paired t-test given effect size, SD, α and power.
- [Sample Size – Unpaired T-test](methods/sample-size-unpaired-t-test.md) — *Sample size planning for an unpaired two-sample t-test in Superiority, Noninferiority, and Equivalence modes.* Estimate required sample sizes per group for a two-sample t-test given effect size, SD, α and power.
- [Sample Size – Single Proportion](methods/sample-size-single-proportion.md) — *Sample size for a single proportion test.* Estimate required sample size for testing a single proportion against a null value.
- [Sample Size – Independent Proportions](methods/sample-size-independent-proportions.md) — *Sample size for two independent proportions with Superiority, Noninferiority, and Equivalence modes.* Estimate sample sizes for comparing two independent proportions (with optional group-size ratio κ).
- [Sample Size – Log-rank test](methods/sample-size-log-rank.md) — *Sample size planning for a two-group log-rank test* Estimate the required number of events, plus the corresponding numbers of controls, experimental subjects, and total subjects for a study with a time-to-event endpoint.
- [Sample Size – Cox Regression](methods/sample-size-cox-regression.md) — *Event-count and subject-count planning for a Cox proportional hazards model with either a binary covariate or a continuous covariate.* Estimate the required number of events, and when an overall event proportion is supplied, the corresponding number of subjects.
- [Sample Size – Intraclass Correlation (ICC)](methods/sample-size-icc.md) — *Sample size planning for a one-sided hypothesis test on an intraclass correlation coefficient (ICC)* Estimate the required number of subjects for a study in which each subject is measured repeatedly or rated multiple times.
- [Sample Size – Agreement (Bland-Altman)](methods/sample-size-bland-altman.md) — *Sample size planning for a Bland-Altman agreement study when the goal is to control the confidence-interval half-width around a limit of agreement (LoA)* Estimate the required number of paired measurements so that the confidence interval around either LoA is sufficiently precise on the original measurement scale.

### Agreement
- [Passing–Bablok Regression](methods/passing-bablok-regression.md) — *Passing–Bablok nonparametric linear regression for method comparison, slope/intercept estimates, confidence intervals, and robust handling of outliers.* Use when comparing two measurement methods without assuming normal errors or homoscedasticity.
- [Deming Regression](methods/deming-regression.md) — *Deming (errors-in-variables) regression with configurable error ratio (λ), weighted/generalized variance models, point estimates and confidence intervals.* Use when both X and Y have measurement error and you want a symmetric method-comparison regression.
- [Bland–Altman Analysis](methods/bland-altman.md) — *Simple paired and repeated-measures Bland–Altman analysis, bias and limits of agreement (LoA), analytical / jackknife / bootstrap confidence intervals, proportional-bias check, raw-difference / percentage / log-ratio scales, several x-axis conventions, and Bland–Altman plots.* Use when you want to assess agreement between two paired measurement methods and quantify the expected size of their differences, rather than only their association.
- [Lin's Concordance Correlation Coefficient](methods/lins-ccc.md) — *Lin CCC, Pearson r, bias-correction factor Cb, analytical or bootstrap confidence intervals, null-concordance test, and identity-line plot.* Use when you want a compact agreement measure for two paired numeric methods and a decomposition into precision and accuracy.
- [Cohen's / Weighted Kappa](methods/cohens-kappa.md) — *Cohen's kappa (unweighted), linear / quadratic weighted kappa, Cicchetti–Allison and Fleiss–Cohen weighting schemes, analytical, jackknife, and bootstrap confidence intervals, confusion matrix, and weight-matrix output.* Use when two paired categorical ratings are assigned to the same items and you want a chance-corrected measure of agreement.
- [Intraclass Correlation Coefficients](methods/intraclass-correlation-coefficients.md) — *ICC(1,1), ICC(1,k), ICC(2,1), ICC(2,k), ICC(3,1), ICC(3,k) with confidence intervals, Repeatability Coefficient.* Quantify reliability/agreement of measurements across raters/replicates using standard ICC families.

## BESH Stat NG - User Defined Functions (UDF)
- [Distributions](udf/distributions.md) — *Studentized range* Various statistical distribution functions
- [Nonparametric](udf/nonparametric.md) — *Mann-Whitney test* Various nonparametric tests
- [Parametric](udf/parametric.md) — *T-test, ANOVA* Various parametric tests
- [Contingency Tables](udf/contingency-tables.md) — *Fisher exact test, Chi2, ORDINAL association, OR, RR, Mantel-Haenszel* Various tests for the contingency table analysis.
- [Survival](udf/survival.md) — *Log-rank test, Kaplan-Meier estimate, Cox regression* Various survival analysis functions
- [Assumptions](udf/assumptions.md) — *Shapiro-Wilk test* Various tests for checking statistical assumptions such as normality, homogeneity of variances, and outliers.
- [Sample Size Calculation](udf/sample-size.md) — *Sample size for (un)paired t-test, proportions, Log-rank test, Cox regression, ICC, Bland-Altman agreement* Estimate required sample sizes for various designs.
- [Regression Models](udf/regression-models.md) - *Linear Models, GLM, GEE*
- [Plot Data](udf/plot-data.md) - *ROC, Histogram binning, Kaplan-Meier plot*
- [Formula Syntax](udf/regression-formula-syntax.md) — Formula syntax for regression Fit functions.
