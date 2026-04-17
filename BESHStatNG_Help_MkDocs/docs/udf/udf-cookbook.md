# UDF Cookbook

This page is a practical guide to the BESHStatNG worksheet functions.

Use it when you want to:

- find a copy-paste formula pattern quickly
- translate a GUI workflow into worksheet formulas
- understand the common **fit → handle → extract** pattern
- build chart-ready tables directly in Excel

This page complements the function reference in [User Defined Functions (UDFs)](index.md). The reference pages document each function individually. This cookbook shows how the functions are typically combined in real worksheets.

!!! tip "Companion workbook"
    Download the live Excel example workbook used by this page:
    [Download the UDF cookbook workbook](../assets/data/000udfcookbook/000udfcookbook.xlsx)
---

## How to use this page

Most BESHStatNG UDF workflows fall into one of four patterns.

### 1. Scalar functions

These return a single value such as a p-value, test statistic, coefficient, or confidence limit.

Example:

```excel
=BESH.NP.SPEARMAN_RHO(A2:A101,B2:B101)
```

Use scalar functions when you want one result per cell and do not need a labeled result table.

### 2. Labeled result-table functions

These return a spill range with row labels and one or more result columns.

Example:

```excel
=BESH.AGREE.LINCCC_FIT(A2:A101,B2:B101)
```

Use these when you want output that already looks like a report table.

### 3. Handle-based workflows

These are the most important pattern to learn for regression and multivariate analysis.

A handle-based workflow usually looks like this:

1. Fit a model and store the returned handle in one cell.
2. Pass that handle into one or more extractor functions.
3. Drop the handle when you no longer need it.

Example:

```excel
=BESH.MULTI.PCA_FIT(A2:F101)
```

If that formula is in `H2`, you can then use:

```excel
=BESH.MULTI.PCA_SUMMARY(H2)
=BESH.MULTI.PCA_LOADINGS(H2)
=BESH.MULTI.PCA_SCORES(H2)
```

and finally:

```excel
=BESH.MULTI.PCA_DROP(H2)
```

Use this pattern whenever the method has several outputs and you do not want to recompute the whole model repeatedly.

!!! note "Handle cells and recalculation"

    - handles are temporary cached objects
    - keep them in dedicated cells
    - use `_DROP` when you no longer need them
    - if a handle cell is overwritten, dependent extractors will fail

### 4. Plot-data functions

These return chart-ready tables rather than a finished chart.

Example:

```excel
=BESH.PLOT.ROC_POINTS(B2:B101,A2:A101)
```

Use these when you want Excel-native charts that remain fully editable.

---

## Quick recipe index

### Plot-data recipes

- [ROC curve from marker + class labels](#roc-curve-from-marker-class-labels)
- [Histogram bins and normal overlay](#histogram-bins-and-normal-overlay)
- [Kaplan-Meier survival curve table](#kaplan-meier-survival-curve-table)

### Multivariate recipes

- [Principal component analysis (PCA)](#principal-component-analysis-pca)
- [Factor analysis (FA)](#factor-analysis-fa)
- [K-means clustering](#k-means-clustering)
- [Hierarchical clustering](#hierarchical-clustering)
- [Correspondence analysis (CA)](#correspondence-analysis-ca)
- [Multiple correspondence analysis (MCA)](#multiple-correspondence-analysis-mca)
- [Discriminant analysis (DA)](#discriminant-analysis-da)

### Regression and model recipes

- [Linear regression](#linear-regression)
- [Generalized linear models](#generalized-linear-models)
- [Cox regression](#cox-regression)

---

## General tips before you start

### Use vertical ranges whenever possible

Most input arguments are easiest to manage when each variable is stored in one Excel column.

Good:

```excel
A2:A101
B2:B101
C2:F101
```

Less convenient:

```excel
A2:F2
```

### Keep headers outside the formula until you know the pattern

Many UDFs can infer headers, but when you are testing a formula for the first time, it is often easiest to start with data-only ranges and then add headers once the pattern is working.

A safe workflow is:

1. get the formula working on data-only ranges
2. then add header-aware options or variable names if needed

### Put model handles in clearly labeled cells

For handle-based functions, reserve a small block such as:

- `H2` for a PCA handle
- `H20` for an FA handle
- `H40` for a Cox handle

That makes downstream formulas easier to read and audit.

### Separate fit formulas from output formulas

A clean worksheet layout is:

- input data on the left
- model handle cells in a small control area
- extracted result tables to the right or below
- charts on a separate sheet if the spill ranges are large

---

## GUI to UDF mapping

This section shows the most common “I know the dialog, what is the worksheet equivalent?” mappings.

| GUI method | Typical UDF pattern |
|---|---|
| ROC Curve | `BESH.PLOT.ROC_POINTS(...)` and `BESH.PLOT.ROC_STATS(...)` |
| Histogram | `BESH.PLOT.HIST_BINS(...)` and optionally `BESH.PLOT.HIST_NORMAL(...)` |
| Kaplan-Meier Plot | `BESH.PLOT.KM_CURVE(...)` |
| Principal Component Analysis | `BESH.MULTI.PCA_FIT(...)` followed by summary, loadings, scores |
| Factor Analysis | `BESH.MULTI.FA_FIT(...)` followed by summary, loadings, structure, scores |
| K-means Clustering | `BESH.MULTI.KMEANS_FIT(...)` followed by centers and assignments |
| Hierarchical Clustering | `BESH.MULTI.HCLUST_FIT(...)` followed by agglomeration and membership |
| Correspondence Analysis | `BESH.MULTI.CA_FIT(...)` followed by row and column coordinates |
| Multiple Correspondence Analysis | `BESH.MULTI.MCA_FIT(...)` followed by category coordinates |
| Discriminant Analysis | `BESH.MULTI.DA_FIT(...)` followed by confusion, casewise, and prediction outputs |

---

## ROC curve from marker + class labels

### Goal

Create a chart-ready ROC curve table and a separate numerical summary with AUC and interval estimates.

### Typical data layout

| A | B |
|---|---|
| Status | Marker |
| 0 | 3.2 |
| 1 | 5.7 |
| 0 | 2.1 |
| 1 | 6.4 |

### Formula for ROC points

```excel
=BESH.PLOT.ROC_POINTS(B2:B101,A2:A101)
```

This returns a spill table suitable for an XY scatter chart. Typical columns include the threshold sequence and the false-positive and true-positive coordinates.

### Formula for ROC summary statistics

```excel
=BESH.PLOT.ROC_STATS(B2:B101,A2:A101)
```

Use this output when you want the numerical summary that would normally be inspected in the GUI, such as:

- area under the curve (AUC)
- standard error
- confidence interval
- p-value or test against a null AUC

### Notes

- The marker should be numeric.
- The status column can usually be coded as `0/1`, `FALSE/TRUE`, or two group labels depending on the UDF design.
- If the function returns `#VALUE!`, first try excluding the header row explicitly.

### Build the chart in Excel

1. Enter `ROC_POINTS` into a blank cell.
2. Select the spilled result.
3. Insert an XY scatter chart with straight lines.
4. Use false-positive rate on the X axis and true-positive rate on the Y axis.

---

## Histogram bins and normal overlay

### Goal

Create a histogram table and, optionally, an overlay table for a normal curve.

### Formula for histogram bins

```excel
=BESH.PLOT.HIST_BINS(A2:A101)
```

This returns a chart-ready table with bin boundaries or midpoints and counts.

### Formula for normal overlay

```excel
=BESH.PLOT.HIST_NORMAL(A2:A101)
```

Use this when you want to overlay a normal reference curve on top of the histogram.

### Typical workflow

1. Spill `HIST_BINS` into a blank area.
2. Create a column or combo chart from the count output.
3. Spill `HIST_NORMAL` nearby.
4. Add the normal series as a line.

### When to use this pattern

This is useful when you want Excel-native charts that remain editable after calculation.

---

## Kaplan-Meier survival curve table

### Goal

Return a survival-curve table that can be used to build a step plot in Excel.

### Typical data layout

| A | B | C |
|---|---|---|
| Time | Status | Group |
| 5 | 1 | Control |
| 8 | 0 | Control |
| 6 | 1 | Treatment |
| 9 | 1 | Treatment |

### Formula

```excel
=BESH.PLOT.KM_CURVE(A2:A101,B2:B101,C2:C101)
```

If the group argument is omitted, the function returns a single survival curve. If a group column is supplied, the output contains the information needed to plot one curve per group.

### Typical use

- create one step-line series per group
- place time on the X axis
- place survival probability on the Y axis

---

## Principal component analysis (PCA)

### Goal

Fit a PCA model once, then extract the pieces you need.

### Fit the model

```excel
=BESH.MULTI.PCA_FIT(A2:F101)
```

Assume this formula is entered in `H2`.

### Get a summary table

```excel
=BESH.MULTI.PCA_SUMMARY(H2)
```

### Get eigen information

```excel
=BESH.MULTI.PCA_EIGEN(H2)
```

### Get loadings

```excel
=BESH.MULTI.PCA_LOADINGS(H2)
```

### Get scores

```excel
=BESH.MULTI.PCA_SCORES(H2)
```

### Drop the handle

```excel
=BESH.MULTI.PCA_DROP(H2)
```

### When to use this pattern

Use handle-based extraction when you want several PCA outputs without recalculating the model for every table.

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## Factor analysis (FA)

### Goal

Fit the factor model once, then inspect the structure from several angles.

### Fit the model

```excel
=BESH.MULTI.FA_FIT(A2:F101)
```

Assume this formula is in `H20`.

### Common extractor formulas

```excel
=BESH.MULTI.FA_SUMMARY(H20)
=BESH.MULTI.FA_EIGEN(H20)
=BESH.MULTI.FA_LOADINGS(H20)
=BESH.MULTI.FA_STRUCTURE(H20)
=BESH.MULTI.FA_COMMUNALITIES(H20)
=BESH.MULTI.FA_SCORES(H20)
```

### Typical use

- use `SUMMARY` for model-level information
- use `LOADINGS` to interpret factors
- use `STRUCTURE` when the rotation is oblique and you want variable–factor correlations
- use `SCORES` when you want factor scores for downstream work

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## K-means clustering

### Goal

Fit a clustering solution and then extract cluster centers and case assignments.

### Fit the model

```excel
=BESH.MULTI.KMEANS_FIT(A2:D101,3)
```

Assume this formula is in `H40`.

### Common extractor formulas

```excel
=BESH.MULTI.KMEANS_SUMMARY(H40)
=BESH.MULTI.KMEANS_CENTERS(H40)
=BESH.MULTI.KMEANS_ASSIGNMENTS(H40)
```

### Typical use

- use `CENTERS` to inspect cluster profiles
- use `ASSIGNMENTS` to join cluster labels back to your data table

---

## Hierarchical clustering

### Goal

Fit a hierarchical clustering model and inspect the agglomeration structure or cut membership.

### Fit the model

```excel
=BESH.MULTI.HCLUST_FIT(A2:D101)
```

Assume this formula is in `H60`.

### Common extractor formulas

```excel
=BESH.MULTI.HCLUST_SUMMARY(H60)
=BESH.MULTI.HCLUST_AGGLOM(H60)
=BESH.MULTI.HCLUST_LEAFORDER(H60)
=BESH.MULTI.HCLUST_MEMBERSHIP(H60,3)
```

### Typical use

- `AGGLOM` shows the merge sequence
- `LEAFORDER` helps when constructing a dendrogram-like display
- `MEMBERSHIP` gives cluster labels for a requested number of clusters

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## Correspondence analysis (CA)

### Goal

Fit a CA model to a contingency table and extract row and column coordinates.

### Fit the model

```excel
=BESH.MULTI.CA_FIT(A2:D6)
```

Assume this formula is in `H80`.

### Common extractor formulas

```excel
=BESH.MULTI.CA_SUMMARY(H80)
=BESH.MULTI.CA_EIGEN(H80)
=BESH.MULTI.CA_ROW_COORD(H80)
=BESH.MULTI.CA_COL_COORD(H80)
=BESH.MULTI.CA_ROW_CONTRIB(H80)
=BESH.MULTI.CA_COL_CONTRIB(H80)
```

### Typical use

Use the row and column coordinates to build a two-dimensional CA map in Excel.

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## Multiple correspondence analysis (MCA)

### Goal

Fit an MCA model to several categorical variables and extract category coordinates.

### Fit the model

```excel
=BESH.MULTI.MCA_FIT(A2:D101)
```

Assume this formula is in `H100`.

### Common extractor formulas

```excel
=BESH.MULTI.MCA_SUMMARY(H100)
=BESH.MULTI.MCA_EIGEN(H100)
=BESH.MULTI.MCA_COORD(H100)
=BESH.MULTI.MCA_CONTRIB(H100)
=BESH.MULTI.MCA_COS2(H100)
```

### Typical use

MCA output is usually explored with category coordinates, contributions, and squared cosine values.

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## Discriminant analysis (DA)

### Goal

Fit a discriminant model once, then inspect group summaries, classification, and canonical functions.

### Fit the model

```excel
=BESH.MULTI.DA_FIT(A2:D101,E2:E101)
```

Assume this formula is in `H120`.

### Common extractor formulas

```excel
=BESH.MULTI.DA_SUMMARY(H120)
=BESH.MULTI.DA_GROUPSUMMARY(H120)
=BESH.MULTI.DA_MEANS(H120)
=BESH.MULTI.DA_CONFUSION(H120)
=BESH.MULTI.DA_CASEWISE(H120)
=BESH.MULTI.DA_CANONICAL(H120)
=BESH.MULTI.DA_CANONCOEF(H120)
```

### Typical use

- `GROUPSUMMARY` gives per-group counts and priors
- `MEANS` shows group centroids in the original variable space
- `CONFUSION` summarizes classification performance
- `CASEWISE` shows row-level predictions and probabilities
- `CANONICAL` and `CANONCOEF` are useful for LDA interpretation

See also: [Multivariate analysis tutorials](https://beshstat.eu/multivariate-analysis-in-excel/)

---

## Linear regression

### Goal

Fit the model once, then extract coefficients, ANOVA-style information, fitted values, or residuals.

### Pattern

```excel
=BESH.LM.FIT(YRange,XRange)
```

Assume the handle is in `H140`.

Typical extractor formulas depend on the current UDF surface for the regression family, but the handle-based logic is the same:

```excel
=<regression summary extractor>(H140)
=<coefficient extractor>(H140)
=<residual extractor>(H140)
```

See also: [Regression and UDF workflows in Excel tutorial](https://beshstat.eu/regression-and-udf-workflows-in-excel/)

---

## Generalized linear models

### Goal

Use the same handle-based logic as linear regression, but with a family and link appropriate to the outcome type.

### Pattern

```excel
=BESH.GLM.FIT(YRange,XRange,"binomial","logit")
```

Then extract the parts you need from the returned handle.

See also: [Regression and UDF workflows in Excel tutorial](https://beshstat.eu/regression-and-udf-workflows-in-excel/)

---

## Cox regression

### Goal

Fit a survival regression model and then extract coefficient and model tables separately.

### Pattern

```excel
=BESH.COX.FIT(TimeRange,StatusRange,XRange)
```

Store the handle in a dedicated cell and use extractor functions for coefficients, summary output, baseline information, or diagnostics.

See also: [Survival analysis in Excel tutorial](https://beshstat.eu/survival-analysis-in-excel/)

---

## Troubleshooting

### `#VALUE!`

Common causes:

- ranges have different lengths
- a required numeric input contains text that is not treated as a header
- the formula includes headers but the function expects data-only ranges
- a handle cell was overwritten or refers to a dropped object

### `#SPILL!`

The output area is blocked. Clear cells below or to the right of the formula and try again.

### The formula works, but the chart looks wrong

Check that:

- the expected X and Y columns are mapped correctly
- the chart type is appropriate, for example XY scatter for ROC data
- categories and values are not being reversed by Excel chart defaults

### The handle-based outputs do not update as expected

Make sure the extractor formulas refer to the correct handle cell. If you changed the fit formula location, update all downstream references.

---

## Naming conventions used by multivariate UDFs

Many multivariate UDFs now use a consistent naming pattern.

| Suffix | Meaning |
|---|---|
| `_FIT` | Fit the model and return a handle |
| `_SUMMARY` | Return a compact summary table |
| `_EIGEN` | Return eigenvalues and related information |
| `_LOADINGS` | Return component or factor loadings |
| `_SCORES` | Return observation-level component or factor scores |
| `_COORD` | Return coordinates for rows, columns, or categories |
| `_CONTRIB` | Return contributions |
| `_COS2` | Return squared cosine quality measures |
| `_MEMBERSHIP` | Return cluster membership |
| `_PREDICT` | Return predictions for new data or fitted cases |
| `_DROP` | Release the cached object handle |

Once you learn this pattern for one family, it becomes much easier to use the others.

---

## Suggested companion resources

This cookbook works best when used together with:

- the per-function UDF reference pages
- the method pages for the corresponding GUI procedures
- downloadable example workbooks showing live formulas and charts

A good workflow is:

1. find the formula pattern here
2. check the exact argument details in the UDF reference
3. inspect the method page when you need statistical interpretation

---

## Planned expansions

This first cookbook page focuses on the main usage patterns. It can be extended over time with:

- complete GUI → UDF mappings by method family
- larger copy-paste formula libraries
- downloadable workbook links
- troubleshooting pages by family
- “build this chart from these UDFs” walkthroughs

