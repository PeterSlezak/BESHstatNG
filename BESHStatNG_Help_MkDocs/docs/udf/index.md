# User Defined Functions (UDFs)

_This page is auto-generated from XML doc comments in the VB files under the add-in `udfs/` folder._

The pages below document the worksheet functions exposed by the add-in. Each group corresponds to the
Excel Function Wizard category (e.g. `BESHStatNG - Nonparametric`).

## Groups

| Group | What it covers | Functions | Related dialog documentation |
|---|---|---:|---|
| [Agreement](agreement.md) | Worksheet functions in this category. | 16 | [Bland Altman](../methods/bland-altman.md), [Deming Regression](../methods/deming-regression.md), [Intraclass Correlation Coefficients](../methods/intraclass-correlation-coefficients.md), [Cohens Kappa](../methods/cohens-kappa.md), [Lins Ccc](../methods/lins-ccc.md), [Passing Bablok Regression](../methods/passing-bablok-regression.md) |
| [Assumptions](assumptions.md) | Worksheet functions in this category. | 12 | [Univariate Outliers](../methods/univariate-outliers.md), [Homogeneity Of Variance](../methods/homogeneity-of-variance.md), [Normality Tests](../methods/normality-tests.md), [Symmetry](../methods/symmetry.md) |
| [Causal Inference](causal-inference.md) | Worksheet functions in this category. | 6 | — |
| [Contingency Tables](contingency-tables.md) | Worksheet functions in this category. | 15 | [2X2 Table](../methods/2x2-table.md), [Mantel Haenszel Test](../methods/mantel-haenszel-test.md), [Proportions](../methods/proportions.md), [Rxc Table](../methods/rxc-table.md) |
| [Distributions](distributions.md) | Probability distribution helper functions (PDF/CDF/quantiles and related utilities). | 3 | — |
| [Multivariate Analysis](multivariate-analysis.md) | Worksheet functions in this category. | 70 | [Correspondence Analysis](../methods/correspondence-analysis.md), [Discriminant Analysis](../methods/discriminant-analysis.md), [Factor Analysis](../methods/factor-analysis.md), [Hierarchical Clustering](../methods/hierarchical-clustering.md), [K Means Clustering](../methods/k-means-clustering.md), [Multiple Correspondence Analysis](../methods/multiple-correspondence-analysis.md), [Principal Component Analysis](../methods/principal-component-analysis.md) |
| [Nonparametric](nonparametric.md) | Rank-based and other nonparametric hypothesis tests and related statistics. | 14 | [Friedman Test](../methods/friedman-test.md), [Kendalls Rank Correlation](../methods/kendalls-rank-correlation.md), [Kruskal Wallis Test](../methods/kruskal-wallis-test.md), [Mann Whitney Test](../methods/mann-whitney-test.md), [Spearman Rank Correlation](../methods/spearman-rank-correlation.md), [Wilcoxon Signed Rank Test](../methods/wilcoxon-signed-rank-test.md) |
| [Parametric](parametric.md) | Worksheet functions in this category. | 10 | [One Way Anova](../methods/one-way-anova.md), [Two Way Nested Anova](../methods/two-way-nested-anova.md), [One Way Repeated Measures Anova](../methods/one-way-repeated-measures-anova.md), [Paired T Tests](../methods/paired-t-tests.md), [Unpaired Two Sample T Tests](../methods/unpaired-two-sample-t-tests.md) |
| [Plot Data](plot-data.md) | Worksheet functions in this category. | 6 | [Histogram](../methods/histogram.md), [Roc Curve](../methods/roc-curve.md) |
| [Regression Models](regression-models.md) | Worksheet functions in this category. | 92 | [Regression Formula Syntax](regression-formula-syntax.md), [Generalized Estimating Equations Gee](../methods/generalized-estimating-equations-gee.md), [Negative Binomial Regression Nb2](../methods/negative-binomial-regression-nb2.md), [Generalized Linear Models Glm](../methods/generalized-linear-models-glm.md), [Linear Mixed Models Lmm](../methods/linear-mixed-models-lmm.md), [Multiple Linear Regression Lm](../methods/multiple-linear-regression-lm.md), [Mixed Models For Repeated Measures Mmrm](../methods/mixed-models-for-repeated-measures-mmrm.md), [Multinomial Logistic Regression](../methods/multinomial-logistic-regression.md), [Ordinal Logistic Regression](../methods/ordinal-logistic-regression.md), [Zero Inflated Poisson Regression](../methods/zero-inflated-poisson-regression.md) |
| [Sample Size](sample-size.md) | Worksheet functions in this category. | 9 | [Sample Size Bland Altman](../methods/sample-size-bland-altman.md), [Sample Size Cox Regression](../methods/sample-size-cox-regression.md), [Sample Size Icc](../methods/sample-size-icc.md), [Sample Size Log Rank](../methods/sample-size-log-rank.md), [Sample Size Independent Proportions](../methods/sample-size-independent-proportions.md), [Sample Size Single Proportion](../methods/sample-size-single-proportion.md), [Sample Size Paired T Test](../methods/sample-size-paired-t-test.md), [Sample Size Unpaired T Test](../methods/sample-size-unpaired-t-test.md) |
| [Survival](survival.md) | Worksheet functions in this category. | 11 | [Cox Regression](../methods/cox-regression.md), [Regression Formula Syntax](regression-formula-syntax.md), [Logrank Test](../methods/logrank-test.md), [Kaplan Meier Plot](../methods/kaplan-meier-plot.md) |

## Function index

### Agreement

- `BESH.AGREE.BLANDALTMAN_ALLOWABLE_BIAS` — Assess Bland–Altman bias against allowable limits on the active analysis scale.
- `BESH.AGREE.BLANDALTMAN_DECISION` — Assess Bland–Altman bias and limits of agreement against allowable decision limits.
- `BESH.AGREE.BLANDALTMAN_FIT` — Bland–Altman analysis for two paired methods. Returns a labeled result table.
- `BESH.AGREE.BLANDALTMAN_PLOTDATA` — Bland–Altman plot data (observation and subject-mean coordinates).
- `BESH.AGREE.BLANDALTMAN_STATS` — Bland–Altman bias and limits of agreement for two paired methods.
- `BESH.AGREE.DEMING_COEF` — Weighted / generalized Deming regression coefficients and confidence intervals.
- `BESH.AGREE.DEMING_FIT` — Weighted / generalized Deming regression for two paired methods. Returns a labeled result table.
- `BESH.AGREE.ICC_FIT` — Intraclass correlation coefficient (ICC) result table for a selected ICC model.
- `BESH.AGREE.ICC_RC` — Repeatability coefficient (RC) and SEM for a selected ICC design.
- `BESH.AGREE.ICC_VALUE` — Point estimate of a selected intraclass correlation coefficient (ICC) model.
- `BESH.AGREE.KAPPA_FIT` — Cohen's / weighted kappa for two paired rating columns. Returns a labeled result table.
- `BESH.AGREE.KAPPA_VALUE` — Cohen's / weighted kappa value for two paired rating columns.
- `BESH.AGREE.LINCCC_FIT` — Lin's concordance correlation coefficient for two paired methods. Returns a labeled result table.
- `BESH.AGREE.LINCCC_VALUE` — Lin's concordance correlation coefficient value for two paired methods.
- `BESH.AGREE.PASSINGBABLOK_COEF` — Passing–Bablok regression coefficients and confidence intervals for two paired methods.
- `BESH.AGREE.PASSINGBABLOK_FIT` — Passing–Bablok regression for two paired methods. Returns a labeled result table.

See: [Agreement UDFs](agreement.md)

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

### Causal Inference

- `BESH.PS.BALANCE` — Returns balance diagnostics before and after matching/weighting.
- `BESH.PS.CLEANUP` — Removes one PSM handle from the session cache, or all PSM handles when handle is blank or ALL.
- `BESH.PS.FIT` — Fits a propensity-score analysis and returns a reusable handle.
- `BESH.PS.LOVEPLOT_DATA` — Returns chart-ready Love plot data for a fitted propensity-score handle.
- `BESH.PS.SUMMARY` — Returns stacked summary tables for a fitted propensity-score handle.
- `BESH.PSM.MATCHES` — Returns the matched-pair/set table for a fitted propensity-score handle.

See: [Causal Inference UDFs](causal-inference.md)

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
- `BESH.CT.TWO_INDEPENDENT_PROPORTIONS_EQUIV` — TOST-style equivalence comparison for two independent proportions with interval-based decision reporting.
- `BESH.CT.TWO_INDEPENDENT_PROPORTIONS_NI` — Non-inferiority comparison for two independent proportions with CI-based decision reporting.

See: [Contingency Tables UDFs](contingency-tables.md)

### Distributions

- `BESH.DIST.F_PDF` — F distribution PDF (equivalent to F.DIST(x, df1, df2, FALSE)).
- `BESH.DIST.PRTRNG` — Studentized range CDF: returns P(0 ≤ Q ≤ q) for df=v and r groups (AS190).
- `BESH.DIST.PRTRNG.TAIL` — Studentized range upper-tail: returns P(Q > q) = 1 - BESH.DIST.PRTRNG(q,v,r).

See: [Distributions UDFs](distributions.md)

### Multivariate Analysis

- `BESH.MULTI.CA_COLUMNS` — Returns column-category overview statistics for a fitted correspondence-analysis model.
- `BESH.MULTI.CA_COL_CONTRIB` — Returns column contributions for each axis of a fitted correspondence-analysis model.
- `BESH.MULTI.CA_COL_COORD` — Returns the column principal-coordinate matrix for a fitted correspondence-analysis model.
- `BESH.MULTI.CA_COL_COS2` — Returns column cos² values for each axis of a fitted correspondence-analysis model.
- `BESH.MULTI.CA_DROP` — Removes a correspondence-analysis handle from memory.
- `BESH.MULTI.CA_EIGEN` — Returns inertia and explained-percentage summaries for a fitted correspondence-analysis model.
- `BESH.MULTI.CA_FIT` — Fits a simple correspondence-analysis model to a contingency table and returns a reusable handle.
- `BESH.MULTI.CA_ROWS` — Returns row-category overview statistics for a fitted correspondence-analysis model.
- `BESH.MULTI.CA_ROW_CONTRIB` — Returns row contributions for each axis of a fitted correspondence-analysis model.
- `BESH.MULTI.CA_ROW_COORD` — Returns the row principal-coordinate matrix for a fitted correspondence-analysis model.
- `BESH.MULTI.CA_ROW_COS2` — Returns row cos² values for each axis of a fitted correspondence-analysis model.
- `BESH.MULTI.CA_SUMMARY` — Returns a compact settings summary for a fitted correspondence-analysis model.
- `BESH.MULTI.DA_CANONCOEF` — Returns the canonical coefficient matrix for a fitted linear discriminant-analysis model.
- `BESH.MULTI.DA_CANONICAL` — Returns the canonical discriminant-functions summary for a fitted linear discriminant-analysis model.
- `BESH.MULTI.DA_CASEWISE` — Returns the casewise classification table for the training or validation pass.
- `BESH.MULTI.DA_CENTROIDS` — Returns the group centroids in canonical discriminant space for a fitted linear discriminant-analysis model.
- `BESH.MULTI.DA_CONFUSION` — Returns an observed-versus-predicted classification table for the training or validation pass.
- `BESH.MULTI.DA_COVARIANCE` — Returns the covariance matrix used by a fitted discriminant-analysis model.
- `BESH.MULTI.DA_DROP` — Removes a discriminant-analysis handle from memory.
- `BESH.MULTI.DA_FIT` — Fits a discriminant-analysis model and returns a reusable handle.
- `BESH.MULTI.DA_FUNCTIONS` — Returns the linear classification-function table for a fitted linear discriminant-analysis model.
- `BESH.MULTI.DA_GROUPSUMMARY` — Returns group counts, prior probabilities, and covariance diagnostics for a fitted discriminant-analysis model.
- `BESH.MULTI.DA_MEANS` — Returns the group mean table for a fitted discriminant-analysis model.
- `BESH.MULTI.DA_PREDICT` — Applies a fitted discriminant-analysis model to a new predictor matrix and returns casewise predictions.
- `BESH.MULTI.DA_PREPROCESS` — Returns the preprocessing constants used by a fitted discriminant-analysis model.
- `BESH.MULTI.DA_REMOVED` — Returns the rows removed by the missing-value policy before discriminant analysis was fitted.
- `BESH.MULTI.DA_SUMMARY` — Returns a compact settings and performance summary for a fitted discriminant-analysis model.
- `BESH.MULTI.FA_COMMUNALITIES` — Returns communalities and uniquenesses for a fitted factor-analysis model.
- `BESH.MULTI.FA_DROP` — Removes a factor-analysis handle from memory.
- `BESH.MULTI.FA_EIGEN` — Returns the initial and retained variance table for a fitted factor-analysis model.
- `BESH.MULTI.FA_FACTORABILITY` — Returns factorability diagnostics for a fitted factor-analysis model.
- `BESH.MULTI.FA_FACTORCORR` — Returns the factor-correlation matrix for a fitted factor-analysis model.
- `BESH.MULTI.FA_FIT` — Fits an exploratory factor-analysis model and returns a reusable handle.
- `BESH.MULTI.FA_LOADINGS` — Returns the rotated loading matrix for a fitted factor-analysis model.
- `BESH.MULTI.FA_MATRIX` — Returns the working covariance or correlation matrix for a fitted factor-analysis model.
- `BESH.MULTI.FA_SCORES` — Returns factor scores for the analyzed observations.
- `BESH.MULTI.FA_STRUCTURE` — Returns the strctre matrix for a fitted factor-analysis model.
- `BESH.MULTI.FA_SUMMARY` — Returns a compact settings and convergence summary for a fitted factor-analysis model.
- `BESH.MULTI.HCLUST_AGGLOM` — Returns the agglomeration schedule for a fitted hierarchical clustering model.
- `BESH.MULTI.HCLUST_DROP` — Removes a hierarchical clustering handle from memory.
- `BESH.MULTI.HCLUST_FIT` — Fits an agglomerative hierarchical clustering model and returns a reusable handle.
- `BESH.MULTI.HCLUST_LEAFORDER` — Returns the leaf order used to display the fitted hierarchical tree.
- `BESH.MULTI.HCLUST_MEMBERSHIP` — Returns a cluster-membership table obtained by cutting the fitted hierarchical tree.
- `BESH.MULTI.HCLUST_PREPROCESS` — Returns the preprocessing constants used by a fitted hierarchical clustering model.
- `BESH.MULTI.HCLUST_REMOVED` — Returns the rows removed by the missing-value policy before hierarchical clustering was fitted.
- `BESH.MULTI.HCLUST_SUMMARY` — Returns a compact settings and fit summary for a fitted hierarchical clustering model.
- `BESH.MULTI.KMEANS_ASSIGNMENTS` — Returns the active-observation assignment table for a fitted k-means model.
- `BESH.MULTI.KMEANS_CENTERS` — Returns the fitted k-means cluster centers.
- `BESH.MULTI.KMEANS_DROP` — Removes a k-means handle from memory.
- `BESH.MULTI.KMEANS_FIT` — Fits a k-means clustering model and returns a reusable handle.
- `BESH.MULTI.KMEANS_PREPROCESS` — Returns the preprocessing constants used by a fitted k-means model.
- `BESH.MULTI.KMEANS_REMOVED` — Returns the rows removed by the missing-value policy before k-means fitting.
- `BESH.MULTI.KMEANS_SUMMARY` — Returns a compact settings and fit summary for a fitted k-means model.
- `BESH.MULTI.MCA_BURT` — Returns the Burt table for a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_CATEGORIES` — Returns category-level overview statistics for a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_CONTRIB` — Returns category contributions for each axis of a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_COORD` — Returns category principal coordinates for a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_COS2` — Returns category cos² values for each axis of a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_DROP` — Removes a multiple correspondence-analysis handle from memory.
- `BESH.MULTI.MCA_EIGEN` — Returns inertia and explained-percentage summaries for a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_FIT` — Fits a multiple correspondence-analysis model to a categorical data matrix and returns a reusable handle.
- `BESH.MULTI.MCA_INDICATOR` — Returns the indicator (design) matrix used internally by a fitted multiple correspondence-analysis model.
- `BESH.MULTI.MCA_SUMMARY` — Returns a compact settings summary for a fitted multiple correspondence-analysis model.
- `BESH.MULTI.PCA_DROP` — Removes a principal component analysis handle from memory.
- `BESH.MULTI.PCA_EIGEN` — Returns eigenvalues and explained-variance summaries for a fitted principal component model.
- `BESH.MULTI.PCA_FIT` — Fits a principal component analysis model and returns a reusable handle.
- `BESH.MULTI.PCA_LOADINGS` — Returns the retained loading matrix for a fitted principal component model.
- `BESH.MULTI.PCA_MATRIX` — Returns the analyzed covariance or correlation matrix for a fitted principal component model.
- `BESH.MULTI.PCA_SCORES` — Returns principal-component scores for the analyzed observations.
- `BESH.MULTI.PCA_SUMMARY` — Returns a compact settings summary for a fitted principal component analysis model.

See: [Multivariate Analysis UDFs](multivariate-analysis.md)

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
- `BESH.PAR.TTEST_UNPAIRED_EQUIV` — TOST-style equivalence comparison for two independent means with interval-based decision reporting.
- `BESH.PAR.TTEST_UNPAIRED_NI` — Non-inferiority comparison for two independent means with CI-based decision reporting.

See: [Parametric UDFs](parametric.md)

### Plot Data

- `BESH.PLOT.CALIB_POINTS` — Calibration-bin points for plotting observed event rate vs. mean predicted probability.
- `BESH.PLOT.HIST_BINS` — Histogram bin midpoints and frequencies for one or more numeric series.
- `BESH.PLOT.HIST_NORMAL` — Normal-overlay coordinates matched to the GUI histogram frequency scale.
- `BESH.PLOT.KM_CURVE` — Step-ready Kaplan-Meier survival-curve coordinates with optional confidence limits.
- `BESH.PLOT.ROC_POINTS` — ROC thresholds and curve coordinates (false-positive rate vs. true-positive rate).
- `BESH.PLOT.ROC_STATS` — Numerical ROC summary: AUC, standard errors, confidence intervals, and p-value.

See: [Plot Data UDFs](plot-data.md)

### Regression Models

- `BESH.CLASS.BRIER` — Returns the Brier score for observed binary outcomes and predicted probabilities.
- `BESH.CLASS.CALIB` — Returns calibration-plot data for observed binary outcomes and predicted probabilities.
- `BESH.CLASS.CONFUSION` — Returns a threshold-based confusion-matrix report for observed binary outcomes and predicted probabilities.
- `BESH.CLASS.THRESH` — Returns a threshold-performance table for observed binary outcomes and predicted probabilities.
- `BESH.REGR.FORMULA_VALIDATE` — Validates a regression-model formula string and returns TRUE or a descriptive validation message.
- `BESH.REGR.GEE_BRIER` — Returns the Brier score for a fitted binomial generalized estimating equation model.
- `BESH.REGR.GEE_CALIB` — Returns calibration-plot data for a fitted binomial generalized estimating equation model.
- `BESH.REGR.GEE_CLASS` — Returns a threshold-based classification report for a fitted binomial generalized estimating equation model.
- `BESH.REGR.GEE_DROP` — Removes a fitted generalized estimating equation handle from memory.
- `BESH.REGR.GEE_FIT` — Fits a generalized estimating equation model and returns a reusable handle.
- `BESH.REGR.GEE_LSMESTIMATE` — Returns custom LS-mean estimates/contrasts for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_PRED` — Returns predicted marginal means and linear predictors for new data under a fitted generalized estimating equation model.
- `BESH.REGR.GEE_RESID` — Returns residual diagnostics for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_SUMMARY` — Returns the coefficient summary table for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_TESTS` — Returns model-level diagnostics and fit statistics for a fitted generalized estimating equation handle.
- `BESH.REGR.GEE_THRESH` — Returns a threshold table for a fitted binomial generalized estimating equation model.
- `BESH.REGR.GEE_VCOV` — Returns the covariance matrix of the estimated generalized estimating equation coefficients.
- `BESH.REGR.GEE_WCORR` — Returns the fitted working correlation matrix for a generalized estimating equation handle.
- `BESH.REGR.GLMNB_DROP` — Removes a fitted Negative Binomial regression handle from memory.
- `BESH.REGR.GLMNB_FIT` — Fits a Negative Binomial regression model with estimated overdispersion and returns a reusable handle.
- `BESH.REGR.GLMNB_PRED` — Returns predicted means and linear predictors for new data under a fitted Negative Binomial regression model.
- `BESH.REGR.GLMNB_RESID` — Returns residual diagnostics for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLMNB_SUMMARY` — Returns the coefficient summary table for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLMNB_TESTS` — Returns model-level diagnostics and fit statistics for a fitted Negative Binomial regression handle.
- `BESH.REGR.GLM_BRIER` — Returns the Brier score for a fitted binomial generalized linear model.
- `BESH.REGR.GLM_CALIB` — Returns calibration-plot data for a fitted binomial generalized linear model.
- `BESH.REGR.GLM_CLASS` — Returns a threshold-based classification report for a fitted binomial generalized linear model.
- `BESH.REGR.GLM_DROP` — Removes a fitted generalized linear model handle from memory.
- `BESH.REGR.GLM_FIT` — Fits a generalized linear model and returns a reusable handle.
- `BESH.REGR.GLM_PRED` — Returns predicted responses and linear predictors for new data under a fitted generalized linear model.
- `BESH.REGR.GLM_RESID` — Returns residual diagnostics for a fitted generalized linear model handle.
- `BESH.REGR.GLM_SUMMARY` — Returns the coefficient summary table for a fitted generalized linear model handle.
- `BESH.REGR.GLM_TESTS` — Returns model-level diagnostics and fit statistics for a fitted generalized linear model handle.
- `BESH.REGR.GLM_THRESH` — Returns a threshold table for a fitted binomial generalized linear model.
- `BESH.REGR.LMM_CLEAR_ALL` — Drops all fitted LMM handles from the session cache.
- `BESH.REGR.LMM_COEF` — Returns the fixed-effect coefficient table for a fitted LMM handle.
- `BESH.REGR.LMM_COVPARMS` — Returns covariance-parameter estimates for a fitted LMM handle.
- `BESH.REGR.LMM_DROP` — Drops a fitted LMM handle from the session cache.
- `BESH.REGR.LMM_FIT` — Fits a linear mixed model and returns a reusable handle.
- `BESH.REGR.LMM_FITSTATS` — Returns fit statistics for a fitted LMM handle.
- `BESH.REGR.LMM_FITTED` — Returns marginal fitted values for a fitted LMM handle.
- `BESH.REGR.LMM_G_CORR` — Returns the fitted G-side random-effect correlation matrix for a fitted LMM handle.
- `BESH.REGR.LMM_G_COV` — Returns the fitted G-side random-effect covariance matrix for a fitted LMM handle.
- `BESH.REGR.LMM_RANEF` — Returns subject-specific random-effect predictions for a fitted LMM handle.
- `BESH.REGR.LMM_RESID` — Returns fitted values and raw marginal residuals for a fitted LMM handle.
- `BESH.REGR.LMM_RESULTS` — Returns all LMM result tables or one named table for a fitted LMM handle.
- `BESH.REGR.LMM_R_CORR` — Returns the fitted R-side residual correlation matrix for a fitted LMM handle.
- `BESH.REGR.LMM_R_COV` — Returns the fitted R-side residual covariance matrix for a fitted LMM handle.
- `BESH.REGR.LMM_TYPE3` — Returns term-level fixed-effect tests for a fitted LMM handle when available.
- `BESH.REGR.LM_ANOVA` — Returns an overall, Type I, or Type III ANOVA table for a fitted linear-model handle.
- `BESH.REGR.LM_DROP` — Removes a fitted linear-model handle from memory.
- `BESH.REGR.LM_FIT` — Fits a Gaussian linear regression model and returns a reusable handle.
- `BESH.REGR.LM_PRED` — Returns predicted mean responses for new observations from a fitted linear-model handle.
- `BESH.REGR.LM_RESID` — Returns residual diagnostics for a fitted linear-model handle.
- `BESH.REGR.LM_SUMMARY` — Returns the coefficient summary table for a fitted linear-model handle.
- `BESH.REGR.LM_TESTS` — Returns model-level diagnostics and fit statistics for a fitted linear-model handle.
- `BESH.REGR.LM_VIF` — Returns the variance-inflation-factor and partial-correlation table for a fitted linear-model handle.
- `BESH.REGR.MMRM_CLEAR_ALL` — Drops all fitted MMRM handles from the session cache.
- `BESH.REGR.MMRM_COEF` — Returns the fixed-effect coefficient table for a fitted MMRM handle.
- `BESH.REGR.MMRM_CONTRASTS` — Returns observed-design-grid group contrasts for a fitted MMRM handle.
- `BESH.REGR.MMRM_COVPARMS` — Returns covariance-parameter estimates for a fitted MMRM handle.
- `BESH.REGR.MMRM_DROP` — Drops a fitted MMRM handle from the session cache.
- `BESH.REGR.MMRM_FIT` — Fits an MMRM and returns a reusable handle.
- `BESH.REGR.MMRM_FITSTATS` — Returns fit statistics for a fitted MMRM handle.
- `BESH.REGR.MMRM_FITTED` — Returns marginal fitted values for a fitted MMRM handle.
- `BESH.REGR.MMRM_LSMEANS` — Returns observed-design-grid LS-means for a fitted MMRM handle.
- `BESH.REGR.MMRM_LSMESTIMATE` — Returns custom SAS-style LS-mean estimates/contrasts for a fitted MMRM handle.
- `BESH.REGR.MMRM_RESID` — Returns fitted values and raw marginal residuals for a fitted MMRM handle.
- `BESH.REGR.MMRM_RESULTS` — Returns all MMRM result tables or one named table for a fitted MMRM handle.
- `BESH.REGR.MMRM_R_CORR` — Returns the fitted R-side correlation matrix for a fitted MMRM handle.
- `BESH.REGR.MMRM_R_COV` — Returns the fitted R-side covariance matrix for a fitted MMRM handle.
- `BESH.REGR.MMRM_TYPE3` — Returns the Kenward-Roger term-level F-test table for a fitted MMRM handle.
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

- `BESH.SSIZE.BLANDALTMAN` — Required pairs for a Bland-Altman agreement study with a target limits-of-agreement precision.
- `BESH.SSIZE.COX_BINARY` — Required events for a Cox design with a binary covariate, with optional total-sample estimate.
- `BESH.SSIZE.COX_CONTINUOUS` — Required events for a Cox design with a continuous covariate, with optional total-sample estimate.
- `BESH.SSIZE.ICC` — Required subjects for a reliability study based on an intraclass-correlation target.
- `BESH.SSIZE.LOGRANK` — Required events and subjects for a two-group log-rank design.
- `BESH.SSIZE.PROP_INDEP` — Required group sizes for superiority, non-inferiority, or equivalence comparisons of two independent proportions.
- `BESH.SSIZE.PROP_SINGLE` — Required sample size for a one-sample two-sided proportion test.
- `BESH.SSIZE.TTEST_PAIRED` — Required number of pairs for a paired two-sided t-test.
- `BESH.SSIZE.TTEST_UNPAIRED` — Required group sizes for unpaired superiority, non-inferiority, or equivalence t-test planning.

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
