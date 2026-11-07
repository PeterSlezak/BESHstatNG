# Official batch 04A.34 owner acceptance review

**READY_FOR_OWNER_04A34_ACCEPTANCE_REVIEW** — bounded software portability and Windows delegation only. All seventeen basic family responsibilities are accounted for, with **zero current OPEN** dispositions. This is an owner-review candidate, not acceptance, scientific-golden validation, native Excel approval or authorization to ship. No 04A.35 or later work began.

Branch `codex/04A-34`; HEAD, main and merge-base `b162a6ac801d7784fc5e27974fbf91aa57a2aee1`. Parent `43d20f86b80934c49b2f3dec90824aa62af5aaee` is the historical04A.33 pre-merge reference. Accepted04A.33 production, tests, dossier and manifest were verified on merged main. The owner's Master Plan Revision2.12, official34–37/39–48 rows, deferred-defect charter,04A.01 ledger, accepted29 v2 lineage and29–33 dossiers/current sources remain authoritative and byte-preserved. Owner acceptance supersedes the stale “33 next” wording; the plan was not rewritten.

Continuation entry preserved the existing partial implementation and verified its 96 external manifest hashes plus nonrecursive self-entry. It did not restart from a ZIP. [Continuation entry](../../evidence/04A34/completion/entry.json) preserves actual source/evidence hashes and the partial metadata snapshot. [Original entry](../../evidence/04A34/entry-authority.json) and [prior publication check](../../evidence/04A34/prior-publication-check.json) retain merged-main ancestry and 58 accepted33 external entries with explicit Git/EOL qualification. No unrelated owner modification was observed. Final [Git status/stat/index](../../evidence/04A34/git-final.json) records an empty index and unstaged/uncommitted changes; no reset, stash, cherry-pick, stage, commit, merge, push, upload or deployment occurred.

## Implemented contracts and Windows integration

Fourteen Windows source files delegate to 26 new portable Core/Common source contracts. Three test projects add only required source links. All previous Core/Common engines and04A.30–33 source bytes remain unchanged; no estimator, matching/flow solver, binning, CI or regression replacement was introduced. Core/Common remain netstandard2.0 with Option Strict On and no native dependency; Windows/Test remain net48. Ribbon/UI/Designer/RefEdit/UDF/help/Office/WASM/installer/signing targets are unchanged.

| Portable source | Actual Windows consuming member(s), including helper chains |
|---|---|
|AnimationFrameTimingCore.vb|GifAnimator:MsToCentiseconds|
|BulletMeasuredGeometryCore.vb|BulletChartExcel:BuildDrawingLayout, BulletChartExcel:BuildRowLayout, BulletChartExcel:Clip01, BulletChartExcel:DrawActualBar, BulletChartExcel:DrawActualValueLabel, BulletChartExcel:DrawLeftLabels, BulletChartExcel:DrawQualitativeBands, BulletChartExcel:DrawScale, BulletChartExcel:DrawTargetMarker, BulletChartExcel:DrawTargetValueLabel, BulletChartExcel:DrawTitle, BulletChartExcel:NormalizedToChartX, BulletChartExcel:ResolveBandColor|
|CalibrationSeriesData.vb|CalibrationPlot:addCalibrationPlot|
|ChartReferenceSeries.vb|AgreementPlotExcelCompatibility:AddHorizontalReferenceLine, AgreementPlotExcelCompatibility:AddPlot, MultipleComparisonPlotExcel:AddPlot|
|ComparisonCoordinatesCore.vb|MultipleComparisonPlotExcel:AddPlot|
|CumulativeStepSeriesData.vb|CumulativeDistributionPlotExcel:ExtendStepToAxisBounds|
|LadderLabelGeometryCore.vb|LadderPlotExcel:SpreadLabelPositions|
|PolarMeasuredLayoutCore.vb|PolarPlotExcel:MakePlotAreaSquare|
|SankeyMeasuredGeometryCore.vb|SankeyPlotExcel:BuildDrawingLayout, SankeyPlotExcel:BuildStageCentres, SankeyPlotExcel:DrawLegend, SankeyPlotExcel:DrawNode, SankeyPlotExcel:DrawNodeLabel, SankeyPlotExcel:DrawRibbon, SankeyPlotExcel:DrawStageLabels, SankeyPlotExcel:DrawTitle, SankeyPlotExcel:NormalizedXToChartX, SankeyPlotExcel:NormalizedYToChartY|
|SpcAxisBoundsCore.vb|SpcControlChartExcel:AddCharts, SpcControlChartExcel:AddConstantReferenceLine, SpcControlChartExcel:ComputeYAxisBounds, SpcControlChartExcel:IncludeNullableInBounds|
|SymmetrySeriesData.vb|SymetryPlot:AsymmetryPlot|
|AgreementSeriesPreparation34.vb|AgreementPlotExcelCompatibility:AddPlot|
|BulletChartPreparation34.vb|BulletChartExcel:BuildAutomaticNumberFormat, BulletChartExcel:FormatNumeric, BulletChartExcel:RequiredDecimalPlaces, BulletChartExcel:ResolveNumberFormat|
|CdfChartPreparation34.vb|CumulativeDistributionPlotExcel:AutomaticNumberFormat, CumulativeDistributionPlotExcel:DistributionShortName, CumulativeDistributionPlotExcel:FormatPercentileLabel, CumulativeDistributionPlotExcel:ResolveAxisContext, CumulativeDistributionPlotExcel:ResolveChartTitle, CumulativeDistributionPlotExcel:ResolveEmpiricalSeriesName, CumulativeDistributionPlotExcel:ResolveFittedSeriesName, CumulativeDistributionPlotExcel:ResolveGroupLegendName, CumulativeDistributionPlotExcel:SnapshotCdfSettings34, DumbbellPlotExcel:ResolveAxisContext, LadderPlotExcel:ResolveAxisContext|
|CdfSeriesPreparation34.vb|CumulativeDistributionPlotExcel:AddPercentileReferenceSeries|
|ComparisonSeriesPreparation34.vb|MultipleComparisonPlotExcel:SuggestedHeight|
|DumbbellChartPreparation34.vb|CumulativeDistributionPlotExcel:ResolveAxisContext, DumbbellPlotExcel:CeilingToStep, DumbbellPlotExcel:FloorToStep, DumbbellPlotExcel:NiceStep125, DumbbellPlotExcel:ResolveAxisContext, DumbbellPlotExcel:ResolveSeriesName, DumbbellPlotExcel:ResolveXAxisNumberFormat, DumbbellPlotExcel:SnapshotDumbbellSettings34, DumbbellPlotExcel:SuggestedHeight, LadderPlotExcel:ResolveAxisContext|
|DumbbellSeriesPreparation34.vb|DumbbellPlotExcel:AddAuxiliarySeries, DumbbellPlotExcel:AddCategoryLabelSeries, DumbbellPlotExcel:AddConnectorSeries, DumbbellPlotExcel:AddEndpointSeries, DumbbellPlotExcel:FormatEndpointValue|
|HullChartPreparation34.vb|ConvexHullPlotExcel:ResolveSeriesName, ConvexHullPlotExcel:SnapshotHullSettings34|
|LadderChartPreparation34.vb|CumulativeDistributionPlotExcel:ResolveAxisContext, DumbbellPlotExcel:ResolveAxisContext, LadderPlotExcel:BuildEndpointLabel, LadderPlotExcel:CeilingToStep, LadderPlotExcel:FloorToStep, LadderPlotExcel:FormatEndpointValue, LadderPlotExcel:NiceStep125, LadderPlotExcel:ResolveAxisContext, LadderPlotExcel:ResolveObservationSeriesName, LadderPlotExcel:ResolveYAxisNumberFormat, LadderPlotExcel:SnapshotLadderSettings34, LadderPlotExcel:SuggestedHeight, LadderPlotExcel:SuggestedWidth|
|LadderSeriesPreparation34.vb|LadderPlotExcel:AddEndpointLabelSeries, LadderPlotExcel:AddObservationSeries, LadderPlotExcel:AddSideLabelSeries, LadderPlotExcel:ResolveMinimumLabelSeparation|
|PolarChartPreparation34.vb|PolarPlotExcel:ResolveSeriesName, PolarPlotExcel:SnapshotPolarSettings34|
|PsmLoveSeriesPolicy.vb|PsmLovePlotExcel:AddThresholdSeries, PsmLovePlotExcel:HasAnyFinite|
|SankeyChartPreparation34.vb|SankeyPlotExcel:FormatNodeLabel, SankeyPlotExcel:SnapshotSankeySettings34|
|SpcChartPreparation34.vb|SpcControlChartAppearanceOptions:NormalizeNumberFormat, SpcControlChartAppearanceOptions:NormalizeText, SpcControlChartExcel:PanelSupportsSpecificationLimits, SpcControlChartExcel:ResolveChartTitle, SpcControlChartExcel:ResolveValueAxisTitle, SpcControlChartExcel:SnapshotSpcSettings34|
|SpecialtyLegendPolicy34.vb|CumulativeDistributionPlotExcel:RegisterLegendSeries, LadderPlotExcel:ShouldKeepLegendEntry, SankeyPlotExcel:ResolveCategoryStyles|

Bullet now consumes explicit-size band/bar/target/scale/tick rectangles and label positions, preserving clipping, admission, palette rank, current-culture numeric format policy, input order and actual/target boundaries. Native font/text acquisition, RGB, enums and shape creation remain Windows. Existing ComputeLayout/ComputeRow/MapX/Clip01 semantics are unchanged.

Sankey consumes ordered node/anchor/ribbon cubic controls, stage and legend geometry and labels with the existing percentage basis/culture. The accepted staged flow solver, category identity and curvature are reused. Native freeform path order, measured text, mutable RGB overrides, resize registry and cleanup remain Windows. No Bullet clipping convention is substituted.

CDF retains accepted empirical/fitted/percentile vectors, step endpoints, group ordering and Percent defaults. It now delegates axes, numeric formats, legend representative policy, names/titles and percentile references. The private invalid/no-extension wrapper keeps original array aliases. Dumbbell delegates axes, connector/endpoint/category/auxiliary vectors, numeric labels and source ordinals; native date interpretation and pixel label offset remain. Ladder delegates axes, endpoint/side/label vectors, text, explicit acquired-height separation and legend flags, reusing stable spreading/clamping. Native HashSet legend lifetime and measurement acquisition remain at their original call points.

Agreement's four public overloads retain their signatures and accepted GetPlotData engines. Basic observed/reference/fit endpoints and the Bland–Altman base span are consumed without refitting. Multiple comparison retains title/validation/order/SuggestedHeight semantics with basic coordinates/zero-reference and height projection. Detailed agreement/CI/error/report/Love work remains04A.35. PSM retains accepted absolute-SMD/order/mask/table/maxX/threshold policy and Y reversal; its basic finite/threshold intent is consumed. SPC reuses ten-channel projection and adds only basic bounds/optional extent/constant references/panel placement/text; detailed channel/stage/zone annotations remain04A.36. No native SPC controls are released.

Calibration preserves bin order, historical NaN-only admission (including infinity), identity and fixed[0,1] axes. Symmetry preserves upper-tail X/lower-tail Y, original host sorting/alias and exceptions; its deferred caption is unchanged. Accepted33 wire sphere/ellipsoid projection/sampling/current-state drawing are untouched. Theil–Sen remains accepted Core plot data. Hull/Kite/Polar reuse accepted points/polygons/grid/source order; Hull/Polar label policies and explicit measured Polar square are consumed. GIF retains only the tested delay conversion; encoding/GDI+/raster/file/event/lifetime stays native.

All new scalar/settings/geometry results are readonly to external consumers. Vector payloads copy constructor inputs and getters, including label/source vectors. Accepted Core source ordinals are zero-based input identities; grouped/series/stage/link ordinals remain distinct and native Points are one-based. No worksheet row is inferred from values-only arrays. Settings are read per preparation/draw call, after existing validation, rather than frozen at constructor time; repeated calls and live appearance mutations are tested. Detailed API/header/shape/missing/culture ownership is in the [contract](../../evidence/04A34/coordinate-and-chart-spec-contract.json).

## All 17 families and exact member identities

The [historical crosswalk](../../evidence/04A34/operation-map-final.csv) retains 768 unique historical IDs with one disposition each. The [current map](../../evidence/04A34/family-ownership-map.csv) retains exactly 764 pre-existing continuation candidate identities, with separate final successor IDs/source spans; new portable members are a separate census. Zero historical or current OPEN entries remain. HOST_ONLY reasons are specific to native state, validation/marshalling, concrete styles, measurement or lifetime; mixed numerical methods delegate their pure preparation. Later reservations cite official35/36 scope. This does not certify completion of those batches.

| Family | Pre-existing current candidates | Historical IDs | Source | Current dispositions |
|---|---:|---:|---|---|
|3D wire objects|34|34|Reused / native|HOST_ONLY:30, ALREADY_PORTABLE_REUSED:4|
|AgreementPlot|4|4|Changed|PORTED_AND_CONSUMED:4|
|BulletChart|103|103|Changed|HOST_ONLY:85, PORTED_AND_CONSUMED:18|
|CalibrationPlot|3|3|Changed|HOST_ONLY:2, PORTED_AND_CONSUMED:1|
|ConvexHullPlot|62|62|Changed|HOST_ONLY:57, ALREADY_PORTABLE_REUSED:4, PORTED_AND_CONSUMED:1|
|CumulativeDistributionPlot|78|78|Changed|HOST_ONLY:65, PORTED_AND_CONSUMED:13|
|DumbbellPlot|74|74|Changed|HOST_ONLY:61, PORTED_AND_CONSUMED:13|
|GifAnimator|1|1|Changed|PORTED_AND_CONSUMED:1|
|KiteChart|48|48|Reused / native|HOST_ONLY:47, ALREADY_PORTABLE_REUSED:1|
|LadderPlot|97|99|Changed|HOST_ONLY:80, PORTED_AND_CONSUMED:17|
|MultipleComparisonPlot|5|5|Changed|PORTED_AND_CONSUMED:2, LATER_BATCH_RESERVED:3|
|PolarPlot|36|36|Changed|HOST_ONLY:31, ALREADY_PORTABLE_REUSED:3, PORTED_AND_CONSUMED:2|
|PsmLovePlot|4|5|Changed|ALREADY_PORTABLE_REUSED:3, PORTED_AND_CONSUMED:1|
|SankeyPlot|110|110|Changed|HOST_ONLY:96, PORTED_AND_CONSUMED:14|
|SpcControlChart|101|102|Changed|HOST_ONLY:86, PORTED_AND_CONSUMED:9, LATER_BATCH_RESERVED:6|
|Symmetry|3|3|Changed|HOST_ONLY:2, PORTED_AND_CONSUMED:1|
|TheilSenPlot|1|1|Reused / native|ALREADY_PORTABLE_REUSED:1|

The four previously unmatched headers remain truthful relocations, without fabricated same-file matches:

- Ladder `c2396206bed850c5@30089:21` → portable SpreadLabelPositions OrderBy value selector; `@30172:13` → its ThenBy original-ordinal tie breaker.
- Love `53143a2ee7cb7aa2@8095:41` → PsmLoveSeriesPolicy.HasAnyFinite's Any predicate, with the same NaN/infinity admission.
- SPC `ca14220ee65e6934@51030:1071` → SpcAxisBoundsCore.HasNaturalZero, called by FromExtents through the actual Windows ComputeYAxisBounds.

Three additional explicit mappings record removal of the unused duplicate private Ladder.Clamp in favor of its already-consumed portable clamp, and Sankey's two stable sort selectors into Common order policies. The final source has six unmatched historical same-file headers (the original four plus Clamp and one Sankey header); the seventh relocation is a same-header selector whose operator ownership moved. Exact IDs, token hashes, operator context, source spans and actual caller edges are in [member consumption](../../evidence/04A34/completion/member-consumption.json). Canonical29 v2 historical uncertainties171/172 remain unresolved; these relocations do not discharge them.

## Compatibility authority and renderer preservation

The original 544 adapter rows, 512 layout cases and 1752 reference/bounds rows and their source signatures/hashes remain byte-identical to continuation entry. Before each additional seam changed, independent test-only original-source bodies/prefixes were frozen and compiled/executed. Source/namespace/owner/accepted-engine binding adaptations are explicit in the supplemental specifications. New Core is never used as both oracle and actual. Accepted Core factories supply typed fixture inputs and source identity, labelled ACCEPTED_CORE/SAME_CORE_WIRING rather than independent scientific evidence.

Final actual pure Windows adapters and portable contracts reproduce **19236 frozen rows per configuration**, Debug and Release, with exact CSV/IEEE values, signed zero and raw NaN bits. Capture groups: `{'geometry': 912, 'extra-geometry': 860, 'preparation': 11840, 'series': 4160, 'policies': 800, 'spc-basis': 120, 'compatibility': 544}`. Post payloads are content-identical and remain ignored to avoid duplicate published megabytes; [comparison receipts](../../evidence/04A34/completion/capture-comparison.json), original bytes and executable capture readers reproduce them. Additional managed checks compare NaN by classification and preserve observable exception types/parameter names, null/empty/mismatched/constant/tied/nonfinite paths, sentinel/order/cardinality and deliberate aliases.

Independent analytical tests cover identity/zero references, measured bar mapping, Bézier endpoints, square centering, SPC padding/bounds and symmetry orientation, alongside frozen compatibility. Input/getter mutation, repeated reads, settings snapshots and native-boundary date/legend state are tested. [Entry portable IL](../../evidence/04A34/completion/entry-portable-il.csv) compares all prior partial Core methods in their original types using resolved metadata-token normalization; existing engines are separately protected by exact source hashes. All software data remains **NOT_GOLDEN**; no R/scientific oracle was executed.

[Renderer preservation](../../evidence/04A34/completion/renderer-preservation.json) compares native call sequence/count/arguments, style assignments, exception boundaries and native measurement order against frozen continuation sources. Only six exact coordinate/copied-label substitutions are qualified: Bullet title width, subtitle top, tick bottom, actual-label top; Dumbbell copied category text; Sankey stage-label left. All other recorded arguments/styles are exact. Pure numeric prefixes move; native chart/shape/freeform/series loops, RGB/fonts/enums, error/lifetime tails stay Windows. CDF legend branch intent and Ladder's real host-free HashSet state are tested separately. Merged-main public Windows headers are preserved by Roslyn; private Clamp/HasNaturalZero removals are explicit. This is SOURCE_ONLY boundary evidence, not fake-chart or real Excel visual equivalence.

## Actual validation and retained nonpasses

Commands use the accepted isolated offline runner, preserving historical output folders. [Final command receipts](../../evidence/04A34/command-outcomes-final.json) retain actual arguments, scope, exit codes and raw-log hashes, including earlier harness/compile failures and successful corrections. Public commands use `<PYTHON>`/`<REPO_ROOT>`/`<ISOLATED_RUN>` placeholders. Final Debug/Release compile-only host and all portable/test builds exit0; no packaging/signing hooks ran. Full suites with inherited failures exit1 and remain failed suites.

| Managed suite | Fresh-main Debug PASS / FAIL | Final Debug PASS / FAIL | Final Release PASS / FAIL |
|---|---:|---:|---:|
|Core|1177 / 0|1232 / 0|1232 / 0|
|Common|8984 / 5|9001 / 5|9001 / 5|
|Legacy|9370 / 9|9479 / 9|9479 / 9|
|OfficeAddin|508 / 0|508 / 0|508 / 0|
|OfficeExecution|45 / 0|45 / 0|45 / 0|
|ControlMitigation|61 / 0|61 / 0|61 / 0|

Totals: **20,326 PASS /14 FAIL /0 ERROR /0 SKIP** per configuration. **181 new raw** 04A.34 passes per configuration, including the original 105 partial passes plus 76 continuation passes. No accepted33 identity/occurrence was removed or changed; no new managed failure was introduced. Full final identity CSV and inherited nonpass records preserve multiplicities in [identities](../../evidence/04A34/managed-test-identities-final.csv) and [nonpasses](../../evidence/04A34/nonpasses-final.json). The partial identity/nonpass receipts remain historical and unmodified. Focused04A.30–34 Core/Common/Framework runs pass in both configurations; exact counts are in validation.json.

The14 inherited raw failures per configuration are:

- `Common: BESHStat.Excel.Common.Tests.ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted` (1 raw occurrence).
- `Common: BESHStat.Excel.Common.Tests.ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection` (3 raw occurrences).
- `Common: BESHStat.Excel.Common.Tests.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings` (1 raw occurrence).
- `Legacy: BESHStatNG.Test.ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden` (1 raw occurrence).
- `Legacy: BESHStatNG.Test.Regression10PolicyTests.ConstantAndSeparatedBinomialPreserveActualFitWarnings` (1 raw occurrence).
- `Legacy: BESHStatNG.Test.Spc21AdapterTests.NativeHostCoercionStateFitOutputAndChartTailStayEntryBytes` (1 raw occurrence).
- `Legacy: BESHStatNG.Test.Spc23AdapterTests.ShippingCompatibilityWrapperUsesActualResultAndSharedRenderer` (6 raw occurrences).

Node: **916 PASS /0 FAIL**, identical baseline identities. Python: **814 PASS /131 FAIL /11 ERROR**, with exact nonpass identities and source transitions retained. Accepted33 was832/114/10; fresh34 entry830/116/10 included two entry privacy-artifact failures, subsequently corrected. Partial final818/127/11 was a different execution identity set, not an interchangeable baseline. Final packet audits: **33 PASS**. Strict/profile exits **1/1** remain genuine nonzero gates; strict MkDocs exits **0**.

New Python nonpasses relative to fresh-main entry are qualified individually below; original assertions/hashes were not altered:

- `test_all_retained_native_methods_and_appearance_remain_exact (test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact)`  — AUTHORIZED_NUMERIC_MEMBER_EXTRACTION_RETAINED_NONPASS. scripts/tests/test_04a22_spc.py:68; ComputeYAxisBounds/private HasNaturalZero and basic optional/constant/axis preparation moved to Core/Common; detailed native annotations unchanged. Original unordered source-member loop may first report FAIL for changed ComputeYAxisBounds or ERROR KeyError HasNaturalZero. Both executed outcomes retained; this is historical source-dictionary inspection, not a statistical/runtime exception from the product. Frozen-source bounds comparisons pass.
- `test_common_is_strict_host_free_and_not_an_estimator (test_04a25_psm.Psm25SourceContracts.test_common_is_strict_host_free_and_not_an_estimator)`  — AUTHORIZED_ADDITIVE_POLICY_CENSUS_RETAINED_FAIL. scripts/tests/test_04a25_psm.py:126 expects exactly five Psm*.vb; new consumed series policy makes six; old five policies unchanged
- `test_new_portable_code_is_strict_and_host_free (test_04a26_planning_agreement.PlanningAgreement26Contracts.test_new_portable_code_is_strict_and_host_free)`  — AUTHORIZED_ADDITIVE_POLICY_CENSUS_RETAINED_FAIL. scripts/tests/test_04a26_planning_agreement.py:129 expects exactly eight Agreement/SampleSize sources; consumed basic AgreementSeriesPreparation34 makes nine. Existing eight are unchanged; dependency/Strict checks independently pass.
- `test_old_paired_and_import_transport_authority_unchanged (test_04a26_planning_agreement.PlanningAgreement26Contracts.test_old_paired_and_import_transport_authority_unchanged)`  — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a26_planning_agreement.py:37; exact AgreementPlotExcel old/new source hash pair
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/AgreementPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/BulletChartExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/CalibrationPlot.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/ConvexHullPlot.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/DumbbellPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/GifAnimator.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/LadderPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/PolarPlot.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/PsmLovePlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/SankeyPlotExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/SpcControlChartExcel.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified
- `test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved (test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved)` (path='BESHstatNG/src/Graphics/SymetryPlot.vb') — AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL. scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified

The old SPC source-dictionary test is unordered: a changed native-member hash may FAIL before lookup of relocated HasNaturalZero, or lookup may ERROR KeyError first. Both historical run outcomes remain recorded; this is a source-inspection failure, not a product runtime/statistical error. The additive PSM and Agreement source-count assertions remain genuine FAILs. No unexplained functional/statistical failure remains. [Source/Node validation](../../evidence/04A34/source-and-node-validation-final.json) compares exact identities and source hashes, and [validation](../../evidence/04A34/validation.json) binds counts to receipts.

Fresh entry full Release was **NOT_RUN**, using fresh-main Debug plus the independently accepted 04A.33 Release reference; both final configurations executed locally. Full wrapper scripts `verify-beshstatng.ps1` and `-Legacy` remain **NOT_RUN** under the continuation's explicit isolation restriction: they touch shared historical outputs and Legacy packaging. The equivalent isolated Core/Common/Framework/Office, Node/Python, strict/profile/docs lanes ran. No wrapper PASS is inferred.

## Preservation, privacy and local publication inventory

All **4074** tracked entry files were compared by actual SHA256/size: **4057 unchanged**, **17 authorized tracked source/project transitions**, zero unrelated mismatches. Owner plan, all previous tests/evidence/controls and accepted Core engines remain unchanged. Original278 lineage pins and100 protected mixed-model captures were independently checked; original pins, hashes, historical test statuses/manifests and canonical29 v2 corrections were not rewritten. New authorized source hashes are successor transitions, never retroactive old-pin passes. See [preservation](../../evidence/04A34/source-preservation.json), [source transitions](../../evidence/04A34/source-transitions.json), [lineage](../../evidence/04A34/lineage-check.json) and [captures](../../evidence/04A34/mixed-model-captures-check.json).

Historical payload census: **280953 files /138180795001 bytes**, matching accepted metadata membership/size/mtime hash. The roughly 138 GB payload population was **NOT_REHASHED**. Only tracked source/evidence and100 protected captures were byte-checked as stated; historical ignored payload bytes are excluded. The owner's prior ZIP independently attested3673 unchanged packaged files and12 transitions, with389 historical tracked files absent from that ZIP; that is review evidence only. The actual4074-entry repository ledger remains the main authority. No historical cleanup/deletion was performed or authorized.

The [local proposed manifest](../../evidence/04A34/publication-manifest.json) contains every source/test/project/dossier/evidence artifact with exact byte size/SHA256 and exactly one null/nonrecursive self-entry. The independent reader imports no producer and rejects missing/foreign/duplicate/case/traversal/absolute/size/hash/self tampering. New negative audits also reject missing candidates, disconnected ports and false relocations/OPEN-to-READY claims. Privacy scans cover proposed external contents/paths/decoded JSON; raw private logs/builds and duplicate post captures stay ignored. Privacy and manifest receipts are refreshed after final artifacts; no upload or publication occurred.

## Restricted gates, deferred issues and manual work

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET; Gate C3=UNAPPROVED.** Existing C13/survival and SPC/native restrictions remain. G29-SYMMETRY-AXIS-CAPTION, C25-LOVE-THRESHOLD-MISSING-01, C25-LOVE-AXIS-NOFINITE-01, regression/report/axis candidates and canonical29 uncertainties171/172 are unchanged. Recorded infinity-admission and short-symmetry-vector candidates remain deferred; no convenient numerical correction is included. Detailed35/36/37 work and39–48 defect/native adjudication are not implemented.

**NOT_RUN:** live Excel/COM/UIA/RefEdit, native controls/visual parity, independent R, Mac/Online/Office/LibreOffice UI, installer/package/signing/deployment. Proposed independently approved native smokes remain PSM Love (finite/missing/category/reversal), SPC (channels/stages/exclusions/specs), calibration/agreement, Bullet/Sankey/CDF/Dumbbell/Ladder/symmetry specialty options, and survival only as the plan's gated manual example. Existing WASM UI smoke is not native approval. No gate is discharged by a portable payload.

## Owner review and Git publication checklist

1. Review this dossier, the complete member/contract/source-transition maps, genuine nonpasses and independently verified manifest against a fresh owner-prepared project ZIP/current repository.
2. Confirm the approved basic34 scope, all seventeen dispositions/relocations, unchanged native/scientific gates and exact preservation evidence. Keep accepted historical payloads; post-phase cleanup needs a separate explicit owner task.
3. If independently accepted, the owner may stage/commit/merge in a separate authorized action. This agent leaves everything unstaged and uncommitted and does not start04A.35.

## Proposed source/test/project/dossier paths

- `BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj`
- `BESHStat.Core.Tests/src/Graphics/Completion34Cases.vb`
- `BESHStat.Core.Tests/src/Graphics/CompletionPolicyCases34.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics34CompletionBoundsTests.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics34CompletionGeometryTests.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics34Tests.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacyCompletionExtras34.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacyCompletionPolicies34.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacyReference34.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacySpcBasis34.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacySpecialty34.vb`
- `BESHStat.Core/src/Graphics/AnimationFrameTimingCore.vb`
- `BESHStat.Core/src/Graphics/BulletMeasuredGeometryCore.vb`
- `BESHStat.Core/src/Graphics/CalibrationSeriesData.vb`
- `BESHStat.Core/src/Graphics/ChartReferenceSeries.vb`
- `BESHStat.Core/src/Graphics/ComparisonCoordinatesCore.vb`
- `BESHStat.Core/src/Graphics/CumulativeStepSeriesData.vb`
- `BESHStat.Core/src/Graphics/LadderLabelGeometryCore.vb`
- `BESHStat.Core/src/Graphics/PolarMeasuredLayoutCore.vb`
- `BESHStat.Core/src/Graphics/SankeyMeasuredGeometryCore.vb`
- `BESHStat.Core/src/Graphics/SpcAxisBoundsCore.vb`
- `BESHStat.Core/src/Graphics/SymmetrySeriesData.vb`
- `BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj`
- `BESHStat.Excel.Common.Tests/src/Graphics34CompletionLegendTests.vb`
- `BESHStat.Excel.Common.Tests/src/Graphics34CompletionPolicyTests.vb`
- `BESHStat.Excel.Common.Tests/src/Graphics34CompletionSeriesTests.vb`
- `BESHStat.Excel.Common.Tests/src/Graphics34LoveTests.vb`
- `BESHStat.Excel.Common/src/Analysis/AgreementSeriesPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/BulletChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/CdfChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/CdfSeriesPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/ComparisonSeriesPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/DumbbellChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/DumbbellSeriesPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/HullChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/LadderChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/LadderSeriesPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/PolarChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/PsmLoveSeriesPolicy.vb`
- `BESHStat.Excel.Common/src/Analysis/SankeyChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/SpcChartPreparation34.vb`
- `BESHStat.Excel.Common/src/Analysis/SpecialtyLegendPolicy34.vb`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj`
- `BESHStatNG.Test/src/Charts/CompletionPreparationCases34.vb`
- `BESHStatNG.Test/src/Charts/CompletionSeriesCases34.vb`
- `BESHStatNG.Test/src/Charts/Graphics34AdapterTests.vb`
- `BESHStatNG.Test/src/Charts/Graphics34CompletionNativeStateTests.vb`
- `BESHStatNG.Test/src/Charts/Graphics34CompletionPreparationTests.vb`
- `BESHStatNG.Test/src/Charts/LegacyCompletionNativeState34.vb`
- `BESHStatNG.Test/src/Charts/LegacyCompletionPreparation34.vb`
- `BESHStatNG.Test/src/Charts/LegacyCompletionSeries34.vb`
- `BESHStatNG.Test/src/Charts/LegacyCompletionShapes34.vb`
- `BESHStatNG.Test/src/Charts/LegacyLayouts34.vb`
- `BESHstatNG/src/Graphics/AgreementPlotExcel.vb`
- `BESHstatNG/src/Graphics/BulletChartExcel.vb`
- `BESHstatNG/src/Graphics/CalibrationPlot.vb`
- `BESHstatNG/src/Graphics/ConvexHullPlot.vb`
- `BESHstatNG/src/Graphics/CumulativeDistributionPlotExcel.vb`
- `BESHstatNG/src/Graphics/DumbbellPlotExcel.vb`
- `BESHstatNG/src/Graphics/GifAnimator.vb`
- `BESHstatNG/src/Graphics/LadderPlotExcel.vb`
- `BESHstatNG/src/Graphics/MultipleComparisonPlotExcel.vb`
- `BESHstatNG/src/Graphics/PolarPlot.vb`
- `BESHstatNG/src/Graphics/PsmLovePlotExcel.vb`
- `BESHstatNG/src/Graphics/SankeyPlotExcel.vb`
- `BESHstatNG/src/Graphics/SpcControlChartExcel.vb`
- `BESHstatNG/src/Graphics/SymetryPlot.vb`
- `docs/architecture/batch-04A-34-owner-review.md`
