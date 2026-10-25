# BESHStatNG

**BESHStatNG** is a free, open-source statistical add-in for **Microsoft Excel on Windows**, built with **Excel-DNA** and **VB.NET**. It brings a broad collection of statistical analyses, regression and longitudinal models, statistical process control, advanced charts, resampling tools, model-reporting utilities, and worksheet functions directly into Excel.

BESHStatNG is designed for researchers, analysts, teachers, students, biomedical users, and other Excel users who want reproducible statistical workflows without moving their data out of the spreadsheet environment.

- **Website:** https://beshstat.eu/
- **Download:** https://beshstat.eu/download/
- **Documentation:** https://beshstat.eu/beshstatng/help/latest/
- **Tutorials:** https://beshstat.eu/tutorials/
- **Validation:** https://beshstat.eu/validation/
- **GitHub Releases:** https://github.com/PeterSlezak/BESHstatNG/releases

**Product availability:** The **Windows desktop Excel-DNA add-in** is the established, supported implementation. Portable statistical libraries and a separate Office.js/WebAssembly development host are being developed toward future Excel on the web and Mac support. These development components are **not** currently a production Mac or Excel Online release. For installation and supported features, use the [official Windows download](https://beshstat.eu/download/) and [published help](https://beshstat.eu/beshstatng/help/latest/).

![BESHStatNG statistical graphics example](BESHStatNG_Help_MkDocs/docs/assets/images/121cdfplot/121cdfplot_output1.png)

## Why BESHStatNG?

BESHStatNG is intended for users who want more than Excel's built-in Analysis ToolPak while keeping an Excel-first workflow.

Key design goals include:

- **Excel-native analysis** through ribbon dialogs, workbook-based result tables, and Excel charts
- **Broad applied statistical coverage** for research, teaching, biomedical analysis, quality improvement, and routine reporting
- **Modern regression and longitudinal modeling**, including LM, GLM, GEE, MMRM, LMM, survival models, ordinal models, and multinomial models
- **Formula-driven analysis** through a large collection of Excel worksheet functions (UDFs)
- **Reproducible resampling and stochastic workflows** with configurable random seeds
- **Consistent statistical presentation**, including application-wide alpha and p-value display settings
- **Transparent validation** with unit tests, reference datasets, NIST benchmarks, and public validation materials
- **Open development** with source code, documentation, issue templates, and release history available in this repository

## Development direction

BESHStatNG is evolving toward a shared, cross-platform statistical foundation: one portable calculation engine, a common Excel-facing application layer, and host-specific integrations for Windows Excel-DNA and future Office.js-based Excel clients. This is ongoing work; the existing Windows ribbon, dialogs, worksheet functions, and MSI remain the supported product. The dedicated Office add-in includes a tested host foundation, read-only workbook diagnostics, method/options navigation, and a controlled ANOVA/Kruskal–Wallis analysis-and-write pilot. The latest pilot is implemented but not yet owner-accepted; the independent WASM smoke project remains a regression harness. Neither constitutes a released alternative. The remaining work includes reusable cross-platform analysis interfaces, careful extraction of portable policies still embedded in Windows forms, more supported methods and charts, future custom-function evaluation, and release-quality testing across Windows, web and Mac. The statistical engine stays in Core, workbook-independent application rules stay in Common, and platform integration stays in each host. LibreOffice Calc compatibility is a future design consideration, not an active integration.

## Major capabilities

### Regression and longitudinal models

BESHStatNG includes a substantial set of regression workflows:

- Multiple linear regression with continuous predictors, factors, polynomial terms, interactions, ANOVA, VIF, residual diagnostics, Cook's distance, and partial correlations
- Generalized Linear Models (GLM), including Gaussian, Binomial, Poisson, Gamma, and related workflows
- Negative Binomial regression (NB2)
- Zero-Inflated Poisson regression
- Generalized Estimating Equations (GEE), including multiple working-correlation structures and LS-estimate support
- Mixed Models for Repeated Measures (**MMRM**) with ML/REML estimation, flexible within-subject covariance structures, Kenward-Roger, Satterthwaite and other inference options, Type III tests, LS-means, contrasts, and custom estimates
- Linear Mixed Models (**LMM**) with random intercepts/slopes, flexible G-side covariance structures, optional R-side covariance models, ML/REML fitting, Kenward-Roger and Satterthwaite inference, BLUPs, residuals, and diagnostics
- Ordinal logistic regression
- Multinomial logistic regression
- Cox proportional hazards regression

### Survival and classifier reporting

- Kaplan-Meier analysis and survival plots
- Log-rank testing
- Cox proportional hazards models
- ROC analysis
- Confusion matrices and threshold-performance tables
- Sensitivity, specificity, predictive values, precision/recall, and related classifier metrics
- Calibration assessment and calibration plots
- Brier score and prediction-performance summaries

### Propensity-score and causal-inference workflows

Propensity Score Matching tools support workflows such as:

- logistic-regression or supplied propensity scores
- nearest-neighbor and optimal pair matching
- calipers and common-support restrictions
- weighting and subclassification
- coarsened exact matching
- covariate-balance diagnostics
- standardized mean differences
- Love plots
- matched-pair and sensitivity outputs

### Statistical process control

BESHStatNG includes both univariate and multivariate Statistical Process Control tools.

**Control Charts** include:

- Individuals and Moving Range charts
- X-bar, R, and S charts
- p, np, c, and u charts
- CUSUM
- EWMA
- Moving Average charts
- Phase I / Phase II workflows, historical limits, exclusions, and configurable signal rules

**Multivariate Control Charts** include:

- Hotelling's T-squared charts
- Generalized Variance charts
- PCA T-squared and PCA Q monitoring
- MEWMA
- Crosier MCUSUM
- covariance diagnostics, variable contributions, signals, and audit-oriented output

### ANOVA, classical tests, and nonparametric methods

- Paired and unpaired t-tests
- One-way ANOVA
- Repeated-measures ANOVA
- Two-way nested ANOVA
- Multiple-comparison procedures including Fisher's LSD, Bonferroni, Tukey-Kramer, and Games-Howell
- Multiple-comparison confidence-interval plots
- Mann-Whitney, Wilcoxon signed-rank, Kruskal-Wallis, Friedman, Cochran's Q, and Skillings-Mack tests
- Spearman and Kendall rank correlation
- Hotelling's T-squared test
- Theil-Sen simple regression
- normality, variance-homogeneity, symmetry, and outlier diagnostics

### Agreement and method comparison

- Passing-Bablok regression
- Deming regression
- Weighted Deming regression
- Bland-Altman analysis
- Lin's Concordance Correlation Coefficient
- Cohen's and weighted kappa
- Intraclass Correlation Coefficients
- resampling-based confidence intervals for supported agreement methods

### Multivariate analysis

- Principal Component Analysis (PCA), including score labels, grouping, scree plots, biplots, and 3D loading plots
- Exploratory Factor Analysis
- K-means clustering
- Hierarchical clustering
- Correspondence Analysis
- Multiple Correspondence Analysis
- Discriminant Analysis

### Sample-size planning

Sample-size tools cover a growing set of designs, including:

- paired and unpaired means
- single and independent proportions
- log-rank tests
- Cox regression
- Intraclass Correlation Coefficients
- Bland-Altman agreement
- equivalence and non-inferiority settings where supported

### Resampling

A shared resampling framework supports reusable workflows such as:

- jackknife
- bootstrap percentile intervals
- bootstrap BCa intervals
- permutation methods
- exact enumeration where applicable

See the dedicated [Resampling documentation](https://beshstat.eu/beshstatng/help/latest/methods/resampling/).

## Statistical graphics

BESHStatNG provides Excel-native charting tools alongside the statistical procedures. Current chart types include:

- Histogram, including an optional aligned horizontal Tukey box plot
- Box-and-whisker plots with configurable palettes, individual observations, mean markers/lines, and legacy appearance
- ROC and calibration plots
- Kaplan-Meier plots
- Normal probability plots
- 3D XYZ scatter plots with optional animated GIF export
- Scatter-plot matrices
- Polar plots
- Convex-hull plots
- Kite charts
- Grouped, stacked, and comparative categorical histograms
- Violin plots
- Sankey diagrams
- Dumbbell plots
- Ladder / before-after plots
- Bullet charts
- CDF and ECDF plots with grouped comparisons, percentile references, and fitted distributions
- PCA, clustering, correspondence-analysis, control-chart, and other method-specific plots

### Selected chart examples

| Sankey diagram | Dumbbell plot |
|---|---|
| ![Sankey chart example](BESHStatNG_Help_MkDocs/docs/assets/images/117sankey/117sankey_diagram1.png) | ![Dumbbell plot example](BESHStatNG_Help_MkDocs/docs/assets/images/118dumbbellplot/118dumbbellplot_output.png) |

| Bullet chart | CDF / ECDF plot |
|---|---|
| ![Bullet chart example](BESHStatNG_Help_MkDocs/docs/assets/images/120bulletchart/120bulletchart_output1.png) | ![CDF and ECDF plot example](BESHStatNG_Help_MkDocs/docs/assets/images/121cdfplot/121cdfplot_output1.png) |

More examples are available in the [online documentation](https://beshstat.eu/beshstatng/help/latest/) and [tutorials](https://beshstat.eu/tutorials/).

## Excel worksheet functions (UDFs)

In addition to ribbon-driven analyses, BESHStatNG exposes a large and growing collection of worksheet functions for reusable, formula-driven workflows.

UDF coverage includes:

- regression fitting, coefficient tables, predictions, diagnostics, LS-means, and contrasts
- MMRM and LMM workflows
- GEE, GLM, NB2, ZIP, Cox, ordinal, and multinomial models
- survival analysis
- classifier reporting and ROC workflows
- propensity-score analysis and balance diagnostics
- agreement and method-comparison methods
- sample-size calculations
- PCA and other multivariate methods
- contingency tables and proportions
- parametric and nonparametric tests
- distribution functions
- assumption checks
- chart-ready plot data

This makes it possible to create reusable analytical workbooks and teaching templates in which results update when source data change.

See the [UDF Cookbook](https://beshstat.eu/beshstatng/help/latest/udf/udf-cookbook/) for practical examples.

## Global settings and statistical presentation

BESHStatNG includes persistent application-wide settings for commonly reused analysis and reporting options.

Current global settings include:

- default significance level (**alpha**)
- default random seed
- p-value decimal precision
- formatting of very small p-values (`< threshold`, scientific notation, or fixed decimal)
- optional `> threshold` formatting for p-values very close to 1
- diagnostic trace logging

Supported result tables use the underlying numeric p-values for significance decisions while applying the selected display format separately. This preserves full numerical precision in standard Excel result cells while providing consistent presentation across supported analyses and UDF report outputs.

See [Global Settings](https://beshstat.eu/beshstatng/help/latest/global-settings/) for details.

## Documentation, tutorials, and worked examples

The documentation project contains method-specific guides, UDF references, worked examples, downloadable datasets, screenshots, global settings, chart-export guidance, and version history.

Useful starting points:

- [Getting started](https://beshstat.eu/beshstatng/help/latest/getting-started/)
- [What's new](https://beshstat.eu/beshstatng/help/latest/whats-new/)
- [MMRM](https://beshstat.eu/beshstatng/help/latest/methods/mixed-models-for-repeated-measures-mmrm/)
- [LMM](https://beshstat.eu/beshstatng/help/latest/methods/linear-mixed-models-lmm/)
- [Propensity Score Matching](https://beshstat.eu/beshstatng/help/latest/methods/propensity-score-matching/)
- [Control Charts](https://beshstat.eu/beshstatng/help/latest/methods/control-charts/)
- [Multivariate Control Charts](https://beshstat.eu/beshstatng/help/latest/methods/multivariate-control-charts/)
- [Resampling](https://beshstat.eu/beshstatng/help/latest/methods/resampling/)
- [UDF Cookbook](https://beshstat.eu/beshstatng/help/latest/udf/udf-cookbook/)
- [Tutorials](https://beshstat.eu/tutorials/)

The tutorials emphasize practical Excel workflows, interpretation, and reusable examples rather than only software syntax.

## Validation and numerical checking

BESHStatNG includes a separate unit-test project and public validation materials. The validation workflow includes:

- automated unit tests
- reference datasets
- comparison with external/reference statistical outputs where applicable
- NIST linear-regression benchmark datasets
- NIST one-way balanced ANOVA benchmark datasets
- mixed-model reference checks
- regression, resampling, and numerical-validation tests
- public test-result artifacts for release transparency

See the current public summary on the [Validation page](https://beshstat.eu/validation/).

## Installation

The recommended installation method is the signed MSI installer available from the BESHStatNG website or GitHub Releases.

1. Download the current installer from https://beshstat.eu/download/.
2. Close Excel before installation.
3. Run the `.msi` installer.
4. Start Excel and enable the add-in if prompted.

BESHStatNG provides separate builds for 32-bit and 64-bit Excel where applicable. From version 1.0.0 onward, the MSI installer is digitally signed; the signature can be inspected in **Properties → Digital Signatures** before installation.

## Cross-platform development status

The Windows Excel-DNA version remains the product used for published releases. Its statistical and numerical algorithms are increasingly implemented in `BESHStat.Core`; `BESHStat.Excel.Common` now contains typed inputs (including model, survival and other advanced inputs), spreadsheet source identity and validation, output planning, formatting semantics, operation diagnostics, execution state and host capability contracts. The Office.js/WebAssembly host is separate from the original WASM smoke harness.

The Office development host has completed its startup, workbook-reader and navigation foundations in real Excel Online owner checks. A limited ANOVA and Kruskal–Wallis workflow is implemented for explicit selection, preview, analysis, destination inspection and guarded output, but the current read/write pilot still requires owner validation before it is accepted. Other methods may appear in the catalog without having runnable Office editors. Mac Excel acceptance, complete chart parity, Office Custom Functions and production deployment are not yet claimed.

Later development will extend shared handlers and reusable UI, complete the portable/Windows separation (including WinForms calculation/presentation helpers), build cross-platform chart specifications and renderers, and extend automated and manual testing. The code should remain suitable for future non-Excel spreadsheet adapters where practical, but no LibreOffice integration is implemented here.

## Build from source

This repository includes the **Windows add-in**, two portable libraries, a production-oriented Office.js/WebAssembly development host, and an independent WebAssembly regression harness. For ordinary users, the [signed Windows installer](https://beshstat.eu/download/) is recommended instead of a source build.

### Windows add-in

- Windows with Microsoft Excel desktop (match the add-in build to Excel's 32-bit or 64-bit architecture)
- Visual Studio 2026 with .NET Framework 4.8 targeting support, .NET 10 SDK/workloads as applicable, and NuGet package restore
- Open `BESHStatNG.sln`, restore dependencies, and build `BESHStatNG` in the appropriate configuration.
- Run the applicable unit tests and real-Excel smoke tests before packaging. Windows installation packaging is maintained separately under [`BESHStatNG_Installer/`](BESHStatNG_Installer/README.md).

The portable libraries target **.NET Standard 2.0**; the modern test and WebAssembly projects target **.NET 10**. They can be built and tested separately with compatible SDKs. For example:

```powershell
dotnet test BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj -c Release
dotnet test BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj -c Release
dotnet build BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj -c Release
dotnet build BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj -c Release
```

The legacy `BESHStatNG.Test` project targets .NET Framework 4.8 and is usually run from Visual Studio/Test Explorer on Windows. The Office.js project is under active development and separate from the WebAssembly regression harness; building either does **not** install a production Mac/Excel Online product. Consult the project-level README files for details.

## Repository structure

| Project / folder | Purpose | Maturity |
| --- | --- | --- |
| [`BESHstatNG/`](BESHstatNG/README.md) | Excel-DNA add-in, Ribbon/WinForms UI, worksheet functions, Excel COM integration and native charting | **Stable Windows host** |
| [`BESHStat.Core/`](BESHStat.Core/README.md) | Portable numerical, statistical and graphics calculation engine | Shared library under active development |
| [`BESHStat.Excel.Common/`](BESHStat.Excel.Common/README.md) | Host-neutral spreadsheet input, analysis orchestration and result contracts | Shared layer under active development |
| [`BESHStat.OfficeAddin/`](BESHStat.OfficeAddin/README.md) | Blazor WebAssembly/Office.js production-oriented host, safe workbook adapter, method/options navigation and limited analysis/write pilot | **Development-only; not yet a supported release** |
| [`BESHStat.WasmSmoke/`](BESHStat.WasmSmoke/README.md) | Independent Blazor WASM + Office.js compatibility and regression harness | **Experimental test harness** |
| [`BESHStatNG.Test/`](BESHStatNG.Test/README.md) | Windows/legacy regression and validation tests | Tests |
| [`BESHStat.Core.Tests/`](BESHStat.Core.Tests/README.md) | Core numerical and statistical unit tests | Tests |
| [`BESHStat.Excel.Common.Tests/`](BESHStat.Excel.Common.Tests/README.md) | Shared Excel contract, catalog and application-layer tests | Tests |
| [`BESHStatNG_Help_MkDocs/`](BESHStatNG_Help_MkDocs/README.md) | MkDocs help, screenshots, examples and generated UDF documentation | Published Windows help source |
| [`BESHStatNG_Installer/`](BESHStatNG_Installer/README.md) | WiX/MSI packaging for the Windows Excel-DNA release | Windows release packaging |

`Directory.Build.props` centralizes solution version values; the Windows installer and published help are released through their own packaging/publishing steps. Public method/help metadata are authored in `BESHStatNG_Help_MkDocs/docs/_meta/methods.yml`, with Ribbon registration and worksheet function declarations validated against their actual Windows sources. Test and validation assets accompany the relevant projects. Developer verification commands are collected in [scripts/](scripts/README.md); the current architecture and forward-looking design rationale are documented in [planning/](planning/README.md) and [docs/architecture/](docs/architecture/README.md).

## Contributing and issue reporting

Contributions, bug reports, validation notes, documentation improvements, and workflow suggestions are welcome.

Before opening an issue, please check the documentation and current release notes. For reproducible bug reports, include where possible:

- BESHStatNG version
- Excel version and Office bitness
- Windows version
- exact steps to reproduce the problem
- expected and actual behavior
- full error message or relevant log excerpt
- a simplified workbook, screenshot, or sample dataset
- whether the issue occurs through a ribbon dialog, worksheet function, or both

If you submit a fix, validation notes are especially useful: describe how the change was tested and whether related unit tests, reference outputs, documentation, or example workbooks were updated.

Useful links:

- [Open an issue](https://github.com/PeterSlezak/BESHstatNG/issues/new/choose)
- [Issue tracker](https://github.com/PeterSlezak/BESHstatNG/issues)
- [Releases](https://github.com/PeterSlezak/BESHstatNG/releases)

## Project status

The **Windows Excel-DNA add-in is the supported product** and continues to receive improvements to methods, graphics, documentation, and numerical validation. Alongside it, portable statistical components and shared Excel contracts are being developed to support additional Excel hosts in the future. The dedicated Office.js/WebAssembly development host can initialize, read selections and expose a narrow ANOVA/Kruskal–Wallis workflow with guarded output. These are development capabilities; the latest analysis/write pilot is still awaiting owner acceptance. There is **no general-availability Mac or Excel Online edition** represented by this repository today.

See [What's New](https://beshstat.eu/beshstatng/help/latest/whats-new/) for released Windows functionality and the [Releases](https://github.com/PeterSlezak/BESHstatNG/releases) page for distributable builds.

## Support and links

- **Website:** https://beshstat.eu/
- **Download:** https://beshstat.eu/download/
- **Documentation:** https://beshstat.eu/beshstatng/help/latest/
- **Tutorials:** https://beshstat.eu/tutorials/
- **Validation:** https://beshstat.eu/validation/
- **Releases:** https://github.com/PeterSlezak/BESHstatNG/releases
- **Issues:** https://github.com/PeterSlezak/BESHstatNG/issues

## Additional screenshots

![BESHStatNG chart example](BESHStatNG_Help_MkDocs/docs/assets/images/120bulletchart/120bulletchart_output1.png)

### 3D scatter plot

![3D scatter plot example](BESHStatNG_Help_MkDocs/docs/assets/images/0113dscatterplot/0113dscatterplot_result.png)

![Animated 3D scatter plot example](BESHStatNG_Help_MkDocs/docs/assets/images/0113dscatterplot/0113dscatterplot_result_animation.gif)

## License

BESHStatNG is released under the [Apache License 2.0](LICENSE).
