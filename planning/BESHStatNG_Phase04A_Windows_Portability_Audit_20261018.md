# BESHStatNG — Windows portable-code migration review and Phase 04A source ledger

**Baseline:** `BESHstatNG(20261018-140903).zip` · Read-only static inspection performed 18 October 2026 · Companion to master plan Revision 1.4.

## Executive findings

The source is already partially portable: many numerical procedures delegate to `BESHStat.Core`, `BESHStat.Excel.Common` contains typed input/parsing and two exemplar handlers, and Windows Excel-DNA remains a valid Framework 4.8 host. The remaining separation problem is **not solely backend classes**: 32 non-designer UI modules combine host acquisition, typed statistical choices, Core calls, table assembly and chart creation. The Windows `src/Graphics` directory contains both mixed numerical/rendering classes and Excel renderer classes; the `src/ExcelUDFs` directory contains a large UDF registration/marshaling surface. The migration must avoid moving COM, WinForms or Excel-specific rendering into portable projects, but move reusable computation, input policies and semantic output to Core/Common.

**Audit scope and limits:** All actual source-file paths in UI, Graphics and UDF trees were enumerated; a targeted static search and representative function inspection identified concrete extraction candidates. This is a **planning audit**, not proof that all UI functions or all UDFs were manually verified for numerical parity. The first 48 Phase 04A batches explicitly require *method-by-method closure* with an exception/ownership ledger. Non-designer code LOC and function counts are automated estimates. Generated `.Designer.vb` modules are counted for event/option wiring; they should not be migrated to portable code.

| Source layer | Files | Primary action |
|---|---:|---|
| Non-designer Windows UI code-behind/controllers | 32 | Split typed options/validation/statistic/plot payloads from event handlers and COM |
| WinForms designer modules | 29 | Wire-test controls, options and events; keep UI platform-specific |
| Windows Graphics VB modules | 38 | Reuse existing Core geometry; extract remaining statistical/geometry math; retain Excel rendering |
| Excel UDF VB modules | 41 | Review 268 `<ExcelFunction>` exports + helpers; separate calculations/validation from registration/caching |
| Windows DM | 9 | Preserve COM ingestion/writer; consolidate typed data and output rules |
| AppInfrastructure | 2 | Move app defaults/help/errors semantic rules to Common; keep Excel host runtime |
| RegModels | 3 | Review cached Core façade, model report builders and matrix wrappers |
| Help/Update | 4 | Keep generated Windows help/UI/installer actions as host adapters |

## Source evidence and priority-specific findings

1. `UI/UibyID.vb` uses a multibranch `HelpTopic` dispatcher for normality, outlier, ANOVA/KW, Mann–Whitney, violin and ordinary normal plots. Its `RunNormality` constructs and p-value-marks multiple `ResultTable`s directly; forms should ultimately ask Common for a result document.
2. `UI/Ui82x2.vb` already calls Core `contingencytable.OddsRatio` and `RiskRatio`, then formats the numeric estimates and intervals as Excel tables and writes through COM. New RR/OR plots should reuse these results and move CI semantics to Common; avoid reading back formatted CI strings.
3. `UI/Ui0OneRefeditMulticol.vb` already uses `contingencytable.cTableORDINALassoc` to output **Kendall tau-b, tau-c, Goodman–Kruskal Gamma and column-dependent Somers D**, including standard errors, interval strings and p-values. The requested extension primarily needs portable typed interval payloads and plots.
4. Core `StatTests/Nonparametric.vb` Friedman has **internal rank sums** and public `MeanRanks`. `MCP` implements Conover and Dunn; no code for Nemenyi CD was located. New Sum Rank reporting should expose existing sums without rederiving ranks in the form.
5. Core `StatTests/Assumptions.vb` includes Grubbs already. A chart can consume the existing detection statistic and source row. `Kolmogorov–Smirnov` appears in explanatory prose only, not an implemented named procedure in the inspected source.
6. Core `StatTests/ContingencyTable.vb` includes Cochran–Armitage; Windows includes Cochran's Q. Neither is the requested **Cochran variance outlier C test**; it needs a separate distribution/critical-value implementation.
7. Windows `Graphics/NormalPlot.vb` contains normal-score plotting positions and reference-line computation inside an Excel-dependent class; `New` calls `Array.Sort(arrData)` on the caller-supplied array, introducing a potential input mutation. A Core extraction should copy before sorting and provide both ordinary and detrended coordinates.
8. Windows `UI/RegressionEffectsController.vb` has reusable term-writing logic but retains `System.Windows.Forms.ListBox`. Split pure `TermSpec` operations into a Common model-authoring service and retain only ListBox projection in the existing controller.
9. `RegModels/LinearAlgebra/Matrix.vb` is **partially a compatibility façade** (e.g. `M_OUTERPRODUCT`, `MatrixMult`, `MatrixTrace` delegate to `MatrixArithmeticCore`). A migration must audit method-by-method rather than move the entire file again; delete wrappers only when callers and binary API allow.
10. `BESHStat.Excel.Common/src/Input/InputSizeLimits.vb` and Windows/Office bounded acquisitions are present for 01.10. Windows still uses single bounded `Value2` calls, while Office chunks; do not claim identical chunk acquisition. The repository records a complete validation run. The reviewer reran the extracted 74 JavaScript tests successfully; no local `dotnet` CLI or Excel GUI was available.

## UI code-behind and controller file ledger (ALL non-designer VB files)

| File | LOC | Methods* | Portability action |
|---|---:|---:|---|
| `ExcelWindowWrapper.vb` | 18 | 1 | Wrap Excel HWND for WinForms ownership; Windows-only |
| `Refedit/Excel2007RefEdit.vb` | 819 | 15 | Custom range picker with COM workbook/worksheet lifecycle, sheet and selection events, A1 formatting and focus state; keep control in Windows but extract neutral range-reference normalization and event/revision semantics |
| `RegressionEffectsController.vb` | 392 | 10 | WinForms ListBox-bound model-effect editing; separate pure term authoring from WinForms controller |
| `Ui01BulletChart.vb` | 946 | 48 | Bullet bands/targets data and chart appearance; shared plot schema and Excel rendering |
| `Ui01ConvexHullPlot.vb` | 638 | 27 | Convex-hull inputs and graphic appearance; Core geometry + Common chart spec |
| `Ui01DumbbellPlot.vb` | 835 | 35 | Dumbbell pairs/labels and chart settings; portable spec and Excel renderer |
| `Ui01LadderPlot.vb` | 911 | 44 | Ladder categories/series/labels, appearance and chart actions; portable spec and Excel renderer |
| `Ui01PolarPlot.vb` | 505 | 13 | Polar input roles, radius/angular formatting and chart settings; Common chart spec |
| `Ui01SankeyPlot.vb` | 1054 | 45 | Sankey node/band display rules, data roles and Office COM; move portable configuration and layout to Common/Core |
| `Ui0OneRefeditMulticol.vb` | 967 | 24 | Friedman/Cochran Q, RxC ordinal association, blocked/repeated methods, result tables and plot orchestration; portable report builders / Windows entry only |
| `Ui10ShowLog.vb` | 52 | 5 | Windows log viewer; backend diagnostic event contract only |
| `Ui11AboutAddin.vb` | 89 | 5 | Windows About dialog; generic version strings from shared metadata, form host-only |
| `Ui11PCA.vb` | 1546 | 77 | Multivariate PCA/CA/MCA/cluster/discriminant/factor option and matrix assembly, chart data; Common adapters |
| `Ui12GlobalSettings.vb` | 68 | 2 | Host settings form; settings DTO/default validation shared, persistence remains Windows |
| `Ui12SampleSizeTtestSingleProp.vb` | 850 | 24 | Range-free sample-size option interpretation, rounding, output values; Common scalar options and results |
| `Ui13GEE.vb` | 919 | 47 | GEE family/correlation/offset and report orchestration; Common options/report |
| `Ui18MMRM.vb` | 2349 | 96 | MMRM contrasts, covariance, LS-means/reference grid and fitting controls; Common option schemas and report builder |
| `Ui19LMM.vb` | 1333 | 70 | LMM random-effect and post-estimation GUI/logic; typed Common request/report |
| `Ui20PropensityScoreMatching.vb` | 901 | 52 | PSM model/matching/trim/balance diagnostics, reports and plots; Common orchestration |
| `Ui21ControlCharts.vb` | 2361 | 142 | Univariate SPC phases, exclusions, rules, chart control state; isolate Core rules + Common payload |
| `Ui22MultivariateControlCharts.vb` | 2476 | 148 | Multivariate SPC setup, covariance/model controls, phased alerts and reporting; isolate Core/Common |
| `Ui3XYZplot.vb` | 841 | 46 | XYZ 3D shape/data options, animation/export settings; math/geometry shared where viable |
| `Ui4Cox.vb` | 719 | 33 | Cox formula, input, diagnostics and output; Common model build/diagnostic schema |
| `Ui4KMandLogRank.vb` | 276 | 10 | Survival/censor and time/strata validation, KM display; typed Common workflow |
| `Ui82x2.vb` | 202 | 9 | 2x2 Fisher/chi-square/OR/RR, worksheet vs scalar inputs, multiple output destination handling; move report semantics to Common |
| `Ui8Proportions.vb` | 382 | 12 | Proportions/equivalence/noninferiority policy and inputs; Common strongly-typed options |
| `Ui99ExportChart.vb` | 637 | 30 | Chart export dialog; COM, file and clipboard operations host-only, export format descriptors potentially shared |
| `Ui9ANOVA2nested.vb` | 541 | 17 | Two-way nested ANOVA input/output; Core computations stay Core, Common request/result policies |
| `UiGLM.vb` | 1583 | 59 | GLM/categorical model setup, formula, weights/offset and response assembly; Common model service |
| `UiTwoInputRefedits.vb` | 849 | 28 | Paired t, Wilcoxon, Spearman, Kendall, Theil–Sen, ICC/agreement, Deming and subgroup-missing handling; extract option/input/result policies |
| `UiUpdateAvailable.vb` | 129 | 2 | Windows updater prompt; UI and installer path host-only |
| `UibyID.vb` | 2108 | 56 | Multimethod form: grouped/paired/import logic; normality, outliers, ANOVA/KW, Mann–Whitney, histogram, ROC, normal plot, violin and result writing; split into Common roles/report builders and Windows view adapter |

*Methods: count of leading `Sub`/`Function` declarations by static matching; overloaded, interface, nested or conditional VB members can change counts. Generated `.Designer.vb` code is intentionally excluded from this count but is covered in Phase 04A.01 and 04A.25 for event wiring.*

## Graphics file ledger (ALL 38 Windows `.vb` modules)

| File | LOC | Proposed Core/Common/Windows treatment |
|---|---:|---|
| `3Dobjects.vb` | 259 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `AgreementPlotExcel.vb` | 174 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `BoxPlot.vb` | 812 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `BulletChart.vb` | 52 | Core calculation already exists (`BulletChartCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `BulletChartExcel.vb` | 1241 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `CalibrationPlot.vb` | 164 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `CategoricalHistogram.vb` | 416 | Core calculation already exists (`CategoricalHistogramCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `ChartExport.vb` | 301 | Likely host-specific chart export/animation; retain IO/COM but extract neutral export/options model if beneficial. |
| `ChartingFunc.vb` | 224 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `ClusterAnalysisPlotExcel.vb` | 342 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `ConvexHullPlot.vb` | 505 | Core calculation already exists (`ConvexHullPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `CorrespondenceAnalysisPlotExcel.vb` | 211 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `CoxSurvivalPlotExcel.vb` | 96 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `CumulativeDistributionPlot.vb` | 37 | Core calculation already exists (`CumulativeDistributionPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `CumulativeDistributionPlotExcel.vb` | 1181 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `DumbbellPlot.vb` | 39 | Core calculation already exists (`DumbbellPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `DumbbellPlotExcel.vb` | 1049 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `FactorAnalysisPlotExcel.vb` | 269 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `GifAnimator.vb` | 169 | Likely host-specific chart export/animation; retain IO/COM but extract neutral export/options model if beneficial. |
| `Histogram.vb` | 601 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `KaplanMeierPlotExcel.vb` | 248 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `KiteChart.vb` | 589 | Core calculation already exists (`KiteChartCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `LadderPlot.vb` | 50 | Core calculation already exists (`LadderPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `LadderPlotExcel.vb` | 1148 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `MultipleComparisonPlotExcel.vb` | 195 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `NormalPlot.vb` | 221 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `PcaPlotExcel.vb` | 567 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `PolarPlot.vb` | 777 | Core calculation already exists (`PolarPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `PsmLovePlotExcel.vb` | 243 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `ROC.vb` | 564 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `SankeyPlot.vb` | 36 | Core calculation already exists (`SankeyPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `SankeyPlotExcel.vb` | 1283 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `ScatterPlotMatrix.vb` | 417 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `SpcControlChartExcel.vb` | 1323 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `SymetryPlot.vb` | 201 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |
| `TheilSenPlotExcel.vb` | 45 | Excel COM renderer; extract any data-to-geometry calculation and semantic legend/axis/interval rules, retain API calls. |
| `ViolinPlot.vb` | 826 | Core calculation already exists (`ViolinPlotCore.vb`); audit remaining Windows decoration/coordinate transforms and host adapter. |
| `XYZscatter.vb` | 1793 | Priority mixed calculations/Excel rendering: isolate reusable computation, copy-safe inputs and neutral chart geometry. |

The portable calculation files already under `BESHStat.Core/src/Graphics` include `BulletChartCore`, `CategoricalHistogramCore`, `ChartAxisScalingCore`, `ConvexHullPlotCore`, `CumulativeDistributionPlotCore`, `DumbbellPlotCore`, `HistogramBinningCore`, `KiteChartCore`, `LadderPlotCore`, `PolarPlotCore`, `SankeyPlotCore` and `ViolinPlotCore`. Do not implement these again simply because a Windows rendering wrapper exists.

## UDF source ledger (ALL 41 `.vb` modules; 268 registration attributes)

| Relative source | LOC | ExcelFunction declarations | Portability action |
|---|---:|---:|---|
| `AgreementMethodUDFs.vb` | 1411 | 16 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `AssumptionsUDFs.vb` | 810 | 12 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `ClassificationReportUDFs.vb` | 389 | 4 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `ContingencyTableUDFs.vb` | 1395 | 15 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `DistributionUDFs.vb` | 212 | 3 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `ExcelArgModules.vb` | 278 | 0 | Output/argument formatting; separate spreadsheet-neutral semantics from Excel registration and spill machinery. |
| `MultivariateAnalysisUDFs.vb` | 4056 | 70 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `NonparametricUDFs.vb` | 1120 | 14 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `ParametricUDFs.vb` | 1027 | 10 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `PlotDataUDFs.vb` | 714 | 6 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `PropensityScoreUDFs.vb` | 896 | 10 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/CoxUDFs.vb` | 1017 | 7 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/GeeUDFs.vb` | 2075 | 13 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/GlmNbUDFs.vb` | 748 | 6 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/GlmUDFs.vb` | 1216 | 10 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/LinearModelUDFs.vb` | 882 | 8 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/LmmUDFs.vb` | 1104 | 15 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/MixedModelUDFs.vb` | 1575 | 15 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/MultinomialLogitUDFs.vb` | 976 | 7 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/OrdinalLogitUDFs.vb` | 906 | 7 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/RegressionFormulaUDFs.vb` | 389 | 1 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `Regression/UdfRegressionOutput.vb` | 139 | 0 | Output/argument formatting; separate spreadsheet-neutral semantics from Excel registration and spill machinery. |
| `Regression/ZipUDFs.vb` | 972 | 6 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `SampleSizeUDFs.vb` | 1196 | 9 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `SurvivalUDFs.vb` | 328 | 4 | Excel-DNA signature/registration host-only; validate calculation delegation, typed Core args/error and scalar/array output. |
| `UDFdataImport/UdfDataImport.vb` | 768 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportAgreement.vb` | 497 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportContingency.vb` | 36 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportFormula.vb` | 194 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportMixedModel.vb` | 791 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportMultivariate.vb` | 272 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportOptions.vb` | 165 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportPlot.vb` | 200 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportPsm.vb` | 188 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportRangeReaders.vb` | 1150 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportRegressionOptions.vb` | 104 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFdataImport/UdfDataImportSurvival.vb` | 268 | 0 | Excel UDF range marshaling and model-data readers; use Common typed parsing with Excel Range adapter. |
| `UDFhelpers.vb` | 61 | 0 | Helper/wrapper; search for algorithms or logic still to extract, retain Excel-only API as adapter. |
| `UdfLinearAlgebra.vb` | 45 | 0 | Helper/wrapper; search for algorithms or logic still to extract, retain Excel-only API as adapter. |
| `UdfOutputTables.vb` | 433 | 0 | Output/argument formatting; separate spreadsheet-neutral semantics from Excel registration and spill machinery. |
| `UdfScalarUtilities.vb` | 194 | 0 | Output/argument formatting; separate spreadsheet-neutral semantics from Excel registration and spill machinery. |

The 268 UDF declaration count represents occurrences of the `<ExcelFunction>` attribute in source, not a verified inventory of callable names or successful runtime registration. The canonical UDF docs generator already has extraction regression tests from earlier batches. Phase 04A.23 will track each exported function through argument validation, Core calculation, output shape, thread safety and model cache lifetime. Phase 11 remains responsible for **Office Custom Functions registration/runtime**, not repeated statistical algorithm migration.

## Remaining Windows-support files

| Files | Decision |
|---|---|
| `DM/ExcelDnaDataImporter.vb`, `DM/ExcelDnaSharedApplicationAdapter.vb`, `DM/ExcelDnaHostCapabilities.vb`, `DM/DataObjContracts.vb`, `DM/Helpers.vb`, `DM/PSMdataImport.vb`, `DM/SheetManagement.vb`, `DM/UIprocedures.vb`, `DM/ResultWriter/ExcelDnaResultWriter.vb` | 9-file host integration/formatting review in 04A.18–04A.19; Common receives only genuinely spreadsheet-neutral semantics. |
| `AppInfrastructure/AppGlobals.vb`, `AppInfrastructure/ExcelDnaHostInfrastructure.vb` | Settings, alpha, help diagnostics and host capability reconciliation in 04A.24. |
| `RegModels/BinaryClassificationReporting.vb`, `RegModels/LinearAlgebra/Matrix.vb`, `RegModels/MixedModels/MixedModelFrontEndHelpers.vb` | Numerical façade vs reusable table/model preparation audit in 04A.09, 04A.12 and 04A.26. |
| `Help/HelpContext.vb`, `Help/HelpLinks.vb`, `Help/HelpWiring.vb`, `Update/AutoUpdate.vb` | Help mappings have generated metadata and remain Windows host-specific; neutral identifiers/help URLs in Common. Windows updater stays platform-specific. |
| Windows `Resources/RibbonXML.xml`, form `.Designer.vb`, MSI/WiX project, Excel-DNA loader | Preserve as host/deployment surface; validate declared callbacks and reliable event paths rather than porting to Core/Common. |

## Scientific scope and overlap with existing master-plan phases

**Feature completion rules:** each selected PDF request gets (a) independent reference fixtures for any new inferential statistic; (b) Core computation or reuse of an existing Core method; (c) typed Common options/results/chart payloads; (d) Excel-DNA view, output and chart; (e) numerical/chart snapshots + a Windows smoke test; (f) generated method/help catalog validation. A chart cannot claim significance from visual overlap alone. Method choices such as Hsu MCB, Dunnett, fitted-normal KS and Nemenyi require explicit methodological documentation.

**Phase 05–09:** expose the newly extracted Common workflows to Office.js and validate host parity, without moving algorithms again. **Phase 10:** render portable chart specifications in Office, without recalculating statistics in JS. **Phase 11:** expose existing portable UDF calculations through Office Custom Functions, including cache/runtime validation. LibreOffice remains an architectural concern only.

**Current checkpoint limitation:** this ledger records *where* portability work is justified in the actual source, not that every individual UI helper has been migrated; 04A.01–04A.26 explicitly complete that work and close the remaining list of exceptions.
