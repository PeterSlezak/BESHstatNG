# User Defined Functions (UDFs)

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._

The pages below document the worksheet functions exposed by the add-in. Each group corresponds to the
Excel Function Wizard category (e.g. `BESHStatNG - Nonparametric`).

## Groups

| Group | What it covers | Functions | Related dialog documentation |
|---|---|---:|---|
| [Assumptions](assumptions.md) | Worksheet functions in this category. | 12 | [Univariate Outliers](../methods/univariate-outliers.md), [Homogeneity Of Variance](../methods/homogeneity-of-variance.md), [Normality Tests](../methods/normality-tests.md), [Symmetry](../methods/symmetry.md) |
| [Contingency Tables](contingency-tables.md) | Worksheet functions in this category. | 13 | [2X2 Table](../methods/2x2-table.md), [Mantel Haenszel Test](../methods/mantel-haenszel-test.md), [Proportions](../methods/proportions.md), [Rxc Table](../methods/rxc-table.md) |
| [Distributions](distributions.md) | Probability distribution helper functions (PDF/CDF/quantiles and related utilities). | 3 | — |
| [Nonparametric](nonparametric.md) | Rank-based and other nonparametric hypothesis tests and related statistics. | 14 | [Friedman Test](../methods/friedman-test.md), [Kendalls Rank Correlation](../methods/kendalls-rank-correlation.md), [Kruskal Wallis Test](../methods/kruskal-wallis-test.md), [Mann Whitney Test](../methods/mann-whitney-test.md), [Spearman Rank Correlation](../methods/spearman-rank-correlation.md), [Wilcoxon Signed Rank Test](../methods/wilcoxon-signed-rank-test.md) |
| [Parametric](parametric.md) | Worksheet functions in this category. | 8 | [One Way Anova](../methods/one-way-anova.md), [Two Way Nested Anova](../methods/two-way-nested-anova.md), [One Way Repeated Measures Anova](../methods/one-way-repeated-measures-anova.md), [Paired T Tests](../methods/paired-t-tests.md), [Unpaired Two Sample T Tests](../methods/unpaired-two-sample-t-tests.md) |
| [Regression Models](regression-models.md) | Worksheet functions in this category. | 49 | [Regression Formula Syntax](regression-formula-syntax.md), [Generalized Estimating Equations Gee](../methods/generalized-estimating-equations-gee.md), [Negative Binomial Regression Nb2](../methods/negative-binomial-regression-nb2.md), [Generalized Linear Models Glm](../methods/generalized-linear-models-glm.md), [Multiple Linear Regression Lm](../methods/multiple-linear-regression-lm.md), [Multinomial Logistic Regression](../methods/multinomial-logistic-regression.md), [Ordinal Logistic Regression](../methods/ordinal-logistic-regression.md), [Zero Inflated Poisson Regression](../methods/zero-inflated-poisson-regression.md) |
| [Sample Size](sample-size.md) | Worksheet functions in this category. | 4 | [Sample Size Independent Proportions](../methods/sample-size-independent-proportions.md), [Sample Size Single Proportion](../methods/sample-size-single-proportion.md), [Sample Size Paired T Test](../methods/sample-size-paired-t-test.md), [Sample Size Unpaired T Test](../methods/sample-size-unpaired-t-test.md) |
| [Survival](survival.md) | Worksheet functions in this category. | 11 | [Cox Regression](../methods/cox-regression.md), [Regression Formula Syntax](regression-formula-syntax.md), [Logrank Test](../methods/logrank-test.md), [Kaplan Meier Plot](../methods/kaplan-meier-plot.md) |

## Function index

### Assumptions

- `BESH.ASM.ANDERSON_DARLING` — Anderson-Darling normality test for a single sample.
- `BESH.ASM.BARTLETT` — Bartlett test for homogeneity of variances across groups.
- `BESH.ASM.BOX_M` — Box's M test for equality of covariance matrices across groups.
- `BESH.ASM.DAGOSTINO_PEARSON` — D'Agostino-Pearson K² normality test for a single sample.
- `BESH.ASM.FLIGNER_KILLEEN` — Fligner-Killeen test for homogeneity of variances across groups.
- `BESH.ASM.GRUBBS` — Grubbs test for a single outlier in a univariate sample.
- `BESH.ASM.LEVENE` — Levene or Brown-Forsythe test for homogeneity of variances across groups.
- `BESH.ASM.MAUCHLY` — Mauchly's test of sphericity for repeated-measures data.
- `BESH.ASM.ROSNER` — Rosner generalized ESD test for multiple outliers in a univariate sample.
- `BESH.ASM.SHAPIRO_WILK` — Shapiro-Wilk normality test for a single sample.
- `BESH.ASM.SQUARED_RANKS` — Squared-ranks test for homogeneity of variances across groups.
- `BESH.ASM.SYMMETRY` — Symmetry test about an unknown median: MGG (default) or Cabilio-Masaro.

See: [Assumptions UDFs](assumptions.md)

### Contingency Tables

- `BESH.CT.CHI2` — Pearson chi-square test of independence for an r×c contingency table.
- `BESH.CT.FFH_EXACT` — Fisher-Freeman-Halton exact test for a general r×c contingency table.
- `BESH.CT.FISHER_2X2` — Fisher's exact test for a 2×2 contingency table, including mid-p values.
- `BESH.CT.MANTEL_HAENSZEL` — Mantel-Haenszel pooled test and common odds ratio across stacked 2×2 strata.
- `BESH.CT.MCNEMAR_EXACT` — Exact paired 2×2 analysis: McNemar/Liddell p-value and matched-pairs odds-ratio interval.
- `BESH.CT.NOMINAL_ASSOC` — Cramér's V, Pearson's contingency coefficient, and Phi for an r×c table.
- `BESH.CT.ODDS_RATIO` — Odds ratio for a 2×2 table with Woolf and Cornfield confidence intervals.
- `BESH.CT.ORDINAL_ASSOC` — Ordinal association measures: Kendall tau-b, tau-c, gamma, and Somers' D.
- `BESH.CT.PAIRED_PROPORTIONS` — Estimate the difference between two paired proportions and return a confidence interval.
- `BESH.CT.RISK_RATIO` — Risk ratio (relative risk) for a 2×2 contingency table.
- `BESH.CT.SINGLE_PROPORTION` — Estimate a single proportion and return a Wilson score confidence interval.
- `BESH.CT.TREND` — Cochran-Armitage test for linear trend in proportions across ordered groups.
- `BESH.CT.TWO_INDEPENDENT_PROPORTIONS` — Estimate the difference between two independent proportions and return a confidence interval.

See: [Contingency Tables UDFs](contingency-tables.md)

### Distributions

- `BESH.DIST.F_PDF` — F distribution PDF (equivalent to F.DIST(x, df1, df2, FALSE)).
- `BESH.DIST.PRTRNG` — Studentized range CDF: returns P(0 ≤ Q ≤ q) for df=v and r groups (AS190).
- `BESH.DIST.PRTRNG.TAIL` — Studentized range upper-tail: returns P(Q > q) = 1 - BESH.DIST.PRTRNG(q,v,r).

See: [Distributions UDFs](distributions.md)

### Nonparametric

- `BESH.NP.FRIEDMAN_MCP` — Friedman post-hoc multiple comparisons: Dunn (default) or Conover.
- `BESH.NP.FRIEDMAN_P` — Friedman test p-value for repeated-measures/blocked designs (chi-square or F-approximation).
- `BESH.NP.FRIEDMAN_STAT` — Friedman test statistic for repeated-measures/blocked designs (T1 chi-square or T2 F-approximation).
- `BESH.NP.KENDALL_P` — P-value for Kendall rank correlation test (τb) for two paired samples.
- `BESH.NP.KENDALL_TAU` — Kendall rank correlation coefficient (τb) for two paired samples.
- `BESH.NP.KW_MCP` — Kruskal-Wallis post-hoc multiple comparisons (Dunn test).
- `BESH.NP.KW_P` — P-value for Kruskal-Wallis test (based on H or tie-corrected Hcor) for 2+ independent groups.
- `BESH.NP.KW_STAT` — Kruskal-Wallis test statistic H (or tie-corrected Hcor) for 2+ independent groups.
- `BESH.NP.MW_P_EXACT` — Mann–Whitney test: exact p-value (n ≤ 50). side: two/lower/upper.
- `BESH.NP.MW_P_NORM` — Mann–Whitney test: two-sided p-value (normal approximation with ties & continuity correction).
- `BESH.NP.SPEARMAN_P` — Two-sided p-value for Spearman rank correlation test (paired samples).
- `BESH.NP.SPEARMAN_RHO` — Spearman rank correlation coefficient (ρ) for two paired samples.
- `BESH.NP.WILCOX_P_EXACT` — Wilcoxon signed-rank test: exact p-value (paired samples; up to 60 non-zero diffs). side: two/lower/upper.
- `BESH.NP.WILCOX_P_NORM` — Wilcoxon signed-rank test: two-sided p-value (normal approximation; paired samples).

See: [Nonparametric UDFs](nonparametric.md)

### Parametric

- `BESH.PAR.ANOVA1` — One-way ANOVA table. Input: one column per group.
- `BESH.PAR.ANOVA1_MCP` — One-way ANOVA multiple comparisons: Tukey-Kramer, Games-Howell, Fisher LSD, or Bonferroni.
- `BESH.PAR.ANOVA1_WELCH` — Welch one-way ANOVA summary. Input: one column per group.
- `BESH.PAR.ANOVA2_NESTED` — Two-way nested ANOVA. Input: 3 columns = group, subgroup, response.
- `BESH.PAR.RMANOVA1` — One-way repeated-measures ANOVA table. Input: rows=subjects, cols=conditions.
- `BESH.PAR.RMANOVA1_MCP` — Repeated-measures ANOVA multiple comparisons: TukeyKramerRM2 (default) or Tukey assuming sphericity.
- `BESH.PAR.TTEST_PAIRED` — Paired t-test for two matched samples. Returns a labeled result table.
- `BESH.PAR.TTEST_UNPAIRED` — Two-sample unpaired t-test. Returns pooled, Welch, or both result tables.

See: [Parametric UDFs](parametric.md)

### Regression Models

- `BESH.REGR.FORMULA_VALIDATE` — Validates a regression-model formula string and returns TRUE or a descriptive validation message.
- `BESH.REGR.GEE_DROP` — Removes a fitted generalized estimating equation handle from memory.
- `BESH.REGR.GEE_FIT` — Fits a generalized estimating equation model and returns a reusable handle.
- `BESH.REGR.GEE_PRED` — Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.
- `BESH.REGR.GEE_RESID` — Returns residual diagnostics for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_SUMMARY` — Returns the coefficient summary table for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_TESTS` — Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_VCOV` — Returns the covariance matrix of the estimated generalized estimating equation coefficients.
- `BESH.REGR.GEE_WCORR` — Returns the fitted working correlation matrix for a generalized estimating equation handle.
- `BESH.REGR.GLMNB_DROP` — Removes a fitted Negative Binomial regression handle from memory.
- `BESH.REGR.GLMNB_FIT` — Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.
- `BESH.REGR.GLMNB_PRED` — Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.
- `BESH.REGR.GLMNB_RESID` — Returns residual diagnostics for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLMNB_SUMMARY` — Returns the coefficient summary table for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLMNB_TESTS` — Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLM_DROP` — Removes a fitted generalized linear model handle from memory.
- `BESH.REGR.GLM_FIT` — Fits a generalized linear model and returns a reusable handle.
- `BESH.REGR.GLM_PRED` — Returns predicted responses and linear predictors for new data under a fitted generalized linear model.
- `BESH.REGR.GLM_RESID` — Returns residual diagnostics for a fitted generalized linear model handle.
- `BESH.REGR.GLM_SUMMARY` — Returns the coefficient summary table for a fitted generalized linear model handle.
- `BESH.REGR.GLM_TESTS` — Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.
- `BESH.REGR.LM_ANOVA` — Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.
- `BESH.REGR.LM_DROP` — Removes a fitted linear-model handle from memory.
- `BESH.REGR.LM_FIT` — Fits a Gaussian linear regression model and returns a reusable handle.
- `BESH.REGR.LM_PRED` — Returns predicted mean responses for new observations from a fitted linear-model handle.
- `BESH.REGR.LM_RESID` — Returns residual diagnostics for a fitted linear-model handle.
- `BESH.REGR.LM_SUMMARY` — Returns the coefficient summary table for a fitted linear-model handle.
- `BESH.REGR.LM_TESTS` — Returns model-level diagnostics and fit statistics for a fitted linear-model handle.
- `BESH.REGR.LM_VIF` — Returns the variance-inflation-factor table for a fitted linear-model handle.
- `BESH.REGR.MNLOGIT_CLASS` — Returns the classification confusion matrix for a fitted multinomial-logit model handle.
- `BESH.REGR.MNLOGIT_DROP` — Removes a fitted multinomial-logit model handle from memory.
- `BESH.REGR.MNLOGIT_FIT` — Fits a baseline-category multinomial logistic regression model and returns a reusable handle.
- `BESH.REGR.MNLOGIT_PRED` — Returns fitted probabilities and predicted categories for new data under a fitted multinomial-logit model.
- `BESH.REGR.MNLOGIT_RESID` — Returns residual diagnostics for a fitted multinomial-logit model handle.
- `BESH.REGR.MNLOGIT_SUMMARY` — Returns the parameter summary table for a fitted multinomial-logit model handle.
- `BESH.REGR.MNLOGIT_TESTS` — Returns model-level diagnostics and tests for a fitted multinomial-logit model handle.
- `BESH.REGR.ORDLOGIT_CLASS` — Returns the classification confusion matrix for a fitted ordinal-logit model handle.
- `BESH.REGR.ORDLOGIT_DROP` — Removes a fitted ordinal-logit model handle from memory.
- `BESH.REGR.ORDLOGIT_FIT` — Fits a proportional-odds ordinal logistic regression model and returns a reusable handle.
- `BESH.REGR.ORDLOGIT_PRED` — Returns fitted probabilities and predicted categories for new data under a fitted ordinal-logit model.
- `BESH.REGR.ORDLOGIT_RESID` — Returns residual diagnostics for a fitted ordinal-logit model handle.
- `BESH.REGR.ORDLOGIT_SUMMARY` — Returns the parameter summary table for a fitted ordinal-logit model handle.
- `BESH.REGR.ORDLOGIT_TESTS` — Returns model-level diagnostics and tests for a fitted ordinal-logit model handle.
- `BESH.REGR.ZIP_DROP` — Removes a fitted Zero-Inflated Poisson model handle from memory.
- `BESH.REGR.ZIP_FIT` — Fits a Zero-Inflated Poisson regression model and returns a reusable handle.
- `BESH.REGR.ZIP_PRED` — Returns predicted means and component predictions for new data under a fitted Zero-Inflated Poisson model.
- `BESH.REGR.ZIP_RESID` — Returns residual diagnostics for a fitted Zero-Inflated Poisson model.
- `BESH.REGR.ZIP_SUMMARY` — Returns coefficient summaries for the count and/or zero component of a fitted Zero-Inflated Poisson model.
- `BESH.REGR.ZIP_TESTS` — Returns model-level diagnostics and fit statistics for a fitted Zero-Inflated Poisson model.

See: [Regression Models UDFs](regression-models.md)

### Sample Size

- `BESH.SSIZE.PROP_INDEP` — Required group sizes for comparing two independent proportions.
- `BESH.SSIZE.PROP_SINGLE` — Required sample size for a one-sample two-sided proportion test.
- `BESH.SSIZE.TTEST_PAIRED` — Required number of pairs for a paired two-sided t-test.
- `BESH.SSIZE.TTEST_UNPAIRED` — Required control and experimental group sizes for an unpaired two-sided t-test.

See: [Sample Size UDFs](sample-size.md)

### Survival

- `BESH.SURV.COX_BASELINE` — Returns baseline survival or cumulative hazard from a fitted Cox model.
- `BESH.SURV.COX_DROP` — Removes a fitted Cox model handle from memory.
- `BESH.SURV.COX_FIT` — Fits a Cox proportional hazards model and returns a handle for use with other COX_* functions.
- `BESH.SURV.COX_PRED` — Computes predictions from a fitted Cox model (linear predictor, risk, survival, or cumulative hazard).
- `BESH.SURV.COX_RESID` — Returns residual diagnostics for a fitted Cox model.
- `BESH.SURV.COX_SUMMARY` — Returns coefficient table (beta, SE, z, p, HR, CI) for a fitted Cox model handle.
- `BESH.SURV.COX_TESTS` — Returns global tests (LR, Wald) and fit statistics for a fitted Cox model handle.
- `BESH.SURV.KM_TABLE` — Kaplan-Meier tabular survival curve: group, time, at-risk, S(t), SE, lower/upper CI.
- `BESH.SURV.LOGRANK_P` — Log-rank family test p-value for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).
- `BESH.SURV.LOGRANK_STAT` — Log-rank family test chi-square statistic for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).
- `BESH.SURV.MEDIAN_CI` — Kaplan–Meier median survival time with Brookmeyer–Crowley CI (overall or by group). Returns a 2D table.

See: [Survival UDFs](survival.md)
