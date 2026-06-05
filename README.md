# BESHStatNG

**BESHStatNG** is a free, open-source, Excel-DNA based statistical add-in for **Microsoft Excel on Windows**. It brings statistical methods, charting tools, model reporting, resampling utilities, and worksheet functions directly into Excel for analysts, researchers, teachers, students, and applied biomedical users who want reproducible statistical workflows without leaving the spreadsheet environment.

- Website: https://beshstat.eu/
- Download: https://beshstat.eu/download/
- Documentation: https://beshstat.eu/beshstatng/help/latest/
- Tutorials: https://beshstat.eu/tutorials/
- Validation: https://beshstat.eu/validation/
- Releases: https://github.com/PeterSlezak/BESHstatNG/releases

![BESHStatNG ribbon](.github/assets/beshstatng_ribbon2.png)

## Why BESHStatNG?

BESHStatNG is designed for users who want more than Excel's built-in Analysis ToolPak, but who still prefer an Excel-first workflow.

It focuses on:

- **Excel-native analysis** with ribbon dialogs, workbook-based output, and chart-ready tables
- **Broad applied statistical coverage** for teaching, reporting, biomedical analysis, and reproducible workflows
- **Regression and longitudinal modeling**, including linear models, GLM, GEE, MMRM, and LMM
- **Survival, agreement, multivariate, sample-size, and causal-inference workflows** inside Excel
- **Formula-driven analysis** through a large set of worksheet functions (UDFs)
- **Transparent validation** with automated tests, reference datasets, NIST benchmark checks, and public test-result artifacts
- **Open development** with source code, documentation, issue templates, and releases available in this repository

## Current scope

Based on the current source and documentation snapshot, BESHStatNG includes:

- **66 documented method/help pages**
- **173 Excel worksheet functions (UDFs)** exposed through Excel-DNA
- **Dialog-driven workflows** plus formula-based analysis for many tasks
- **Versioned MkDocs documentation**
- **A dedicated unit-test project** with reference datasets and public `.trx` test-result artifacts

### Main method areas

- Descriptive statistics, normality checks, outlier diagnostics, and distribution diagnostics
- Parametric tests, nonparametric tests, ANOVA, nested ANOVA, and related post-hoc workflows
- Contingency tables, proportions, symmetry, agreement, and association measures
- Linear, generalized linear, negative binomial, ordinal, multinomial, and zero-inflated regression
- **Mixed Models for Repeated Measures (MMRM)**
- **Linear Mixed Models (LMM)**
- **Generalized Estimating Equations (GEE)**
- Survival analysis, including Kaplan-Meier, log-rank, and Cox regression
- ROC analysis, classifier reporting, calibration, threshold tables, Brier score, and prediction-performance summaries
- Propensity Score Matching (PSM), weighting, subclassification, and balance diagnostics
- Multivariate methods, including PCA, factor analysis, clustering, correspondence analysis, discriminant analysis, and Hotelling's T²
- Agreement and method comparison, including Bland-Altman, Lin's CCC, Cohen's / weighted kappa, Passing-Bablok, Deming, weighted Deming, and ICC
- Sample-size planning for means, proportions, survival, Cox regression, ICC, Bland-Altman agreement, equivalence, and non-inferiority settings
- Resampling support, including jackknife, bootstrap percentile, bootstrap BCa, permutation, and exact-enumeration workflows where applicable
- Statistical graphics, chart-data UDFs, and exportable charts

## What the project offers

### 1. Ribbon-based analyses inside Excel

BESHStatNG adds task-focused dialogs to Excel so users can run statistical analyses without writing code. Output remains in the workbook, which makes results easy to review, teach, reproduce, and share.

### 2. Worksheet functions for reusable models and templates

The project includes a large and growing set of UDFs for:

- regression model fitting, summaries, predictions, LS-means, contrasts, and diagnostics
- MMRM and LMM workflows
- survival analysis
- agreement and method-comparison workflows
- classifier performance, calibration, ROC, and threshold reporting
- propensity-score analysis and balance diagnostics
- sample-size calculations
- multivariate analysis
- contingency tables and proportions
- distribution functions
- parametric and nonparametric utilities
- assumption checks
- chart-ready plot data

This makes it possible to build reusable templates, teaching workbooks, and lightweight analytical pipelines directly in Excel.

### 3. Documentation, tutorials, and worked examples

The documentation covers method-specific help, implementation notes, UDF reference pages, formula syntax, chart export, global settings, and version history.

The tutorial pages provide practical workbook-based workflows for:

- method comparison and agreement
- survival analysis
- regression and UDF workflows
- multivariate analysis
- sample-size planning
- ROC analysis
- binary classifier reporting
- calibration and threshold selection
- repeated-measures analysis
- formula-driven workbook templates
- biomedical and teaching examples

### 4. Validation and numerical checking

BESHStatNG includes a separate test project and public validation materials. The validation workflow includes:

- automated unit tests
- reference datasets
- R/reference-output comparison scripts where applicable
- NIST linear-regression benchmark datasets
- NIST one-way balanced ANOVA benchmark datasets
- mixed-model reference tests
- public `.trx` test-result artifacts for release transparency

See the validation page for the current public summary:

https://beshstat.eu/validation/

## Installation

The recommended way to install BESHStatNG is from the project website download page or from GitHub Releases.

1. Download the latest `.msi` installer.
2. Close Excel before installing.
3. Run the installer.
4. Start Excel and enable the add-in if prompted.

Current download page:

https://beshstat.eu/download/

## Documentation

Online help is published at:

https://beshstat.eu/beshstatng/help/latest/

The documentation source in this repository is built with **MkDocs**.

Useful documentation entry points:

- What's new: https://beshstat.eu/beshstatng/help/latest/whats-new/
- UDF Cookbook: https://beshstat.eu/beshstatng/help/latest/udf/udf-cookbook/
- Resampling: https://beshstat.eu/beshstatng/help/latest/methods/resampling/
- MMRM: https://beshstat.eu/beshstatng/help/latest/methods/mixed-models-for-repeated-measures-mmrm/
- LMM: https://beshstat.eu/beshstatng/help/latest/methods/linear-mixed-models-lmm/

## Contributing and issue reporting

Contributions, bug reports, validation notes, documentation improvements, and workflow suggestions are welcome.

Before opening an issue, please check the documentation and the current release notes. If you have found a bug, use the GitHub bug-report template and include:

- the BESHStatNG version
- the Excel version and Office bitness
- the Windows version
- exact steps to reproduce the problem
- the expected behavior and actual behavior
- the full error message or stack trace, if available
- a simplified workbook, screenshot, or sample data if it helps reproduce the issue
- whether the problem occurs through the ribbon dialog, a UDF formula, or both

If you plan to submit a fix, please also include validation notes describing how the change was tested in Excel and whether any unit tests, reference outputs, documentation, or tutorial workbooks were updated.

Useful links:

- Contributing guide: [.github/CONTRIBUTING.md](.github/CONTRIBUTING.md)
- Bug reports: https://github.com/PeterSlezak/BESHstatNG/issues/new/choose
- Issue tracker: https://github.com/PeterSlezak/BESHstatNG/issues
- Releases: https://github.com/PeterSlezak/BESHstatNG/releases

## Build from source

### Prerequisites

- Windows
- Microsoft Excel desktop
- Visual Studio with **.NET Framework 4.8** support
- NuGet package restore enabled

### Main project stack

- Visual Basic .NET
- Excel-DNA
- .NET Framework 4.8
- MkDocs for documentation
- WiX / installer project for packaging

### Build the add-in

1. Clone the repository.
2. Restore NuGet packages.
3. Open `BESHStatNG.sln` in Visual Studio.
4. Build the solution in `Release` mode.
5. Run the relevant unit tests from `BESHStatNG.Test`.
6. Package the installer using the installer project or your local packaging workflow.

## Repository structure

```text
BESHStatNG.sln
/BESHStatNG/                         Excel-DNA based add-in project source code
    src/                             application code
        AppInfrastructure/           global app settings and shared classes
        BaseStat/                    core statistical procedures, distributions, and resampling infrastructure
        ExcelUDFs/                   Excel worksheet functions
        Graphics/                    chart generation and chart exporting code
        Help/                        help-link mapping and help integration
        RegModels/                   regression, mixed models, survival, PSM, and multivariate model code
        StatTests/                   statistical test implementations
        UI/                          Windows Forms UI and ribbon handlers
        Update/                      version checker and updater
    tools/                           helper scripts
/BESHStatNG.Test/                    unit-test project, reference datasets, and public test-result artifacts
/BESHStatNG_Help_MkDocs/             MkDocs documentation source files
/BESHStatNG_Installer/               WiX installer project
/.github/                            issue templates, contribution guide, pull-request template, and assets
/tools/                              repository-level helper scripts
README.md
```

## Intended users

BESHStatNG is especially useful for:

- researchers working in Excel
- teachers preparing statistical demonstrations
- students learning applied statistics
- analysts who prefer spreadsheet-based workflows
- users who need reproducible tables and charts directly in workbooks
- biomedical, clinical, laboratory, and method-comparison users
- users who want transparent statistical workflows with source code and validation materials available

## Project status

BESHStatNG is under active development.

Current priorities include:

- strengthening LMM, MMRM, and PSM tutorials and workbook examples
- extending validation summaries and public benchmark coverage
- adding more real-world biomedical, teaching, and formula-driven workbook templates

## Support and links

- Website: https://beshstat.eu/
- Download: https://beshstat.eu/download/
- Documentation: https://beshstat.eu/beshstatng/help/latest/
- Tutorials: https://beshstat.eu/tutorials/
- Validation: https://beshstat.eu/validation/
- Releases: https://github.com/PeterSlezak/BESHstatNG/releases
- Issues: https://github.com/PeterSlezak/BESHstatNG/issues

## Screenshots

![BESHStatNG screenshot](.github/assets/beshstatng_ribbon.png)
![3D scatter plot example](BESHStatNG_Help_MkDocs/docs/assets/images/0113dscatterplot/0113dscatterplot_result.png)
![animated 3D scatter plot example](BESHStatNG_Help_MkDocs/docs/assets/images/0113dscatterplot/0113dscatterplot_result_animation.gif)
