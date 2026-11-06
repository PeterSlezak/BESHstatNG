# Batch 04A.30 owner review

**NOT_READY — targeted evidence disposition; software extraction only.**

The six historical04A.28 source-pin failures now have an executable verified successor:278/272/6 PASS,while the original six subtests remain FAIL. The remaining blocker is local proof for93 original04A.29 EOL rows; no pristine archive/original bytes were available locally. All27 direct matches and all121 current-to-Git identities pass. The owner's ZIP findings are external testimony until originals can be inspected. No product or existing test was changed in this closeout. Prior complete summary is preserved byte-for-byte in `evidence/04A30/closeout-v1-prior/owner-review-pre-closeout.md`.

Canonical closeout: [disposition](../../evidence/04A30/04A30-evidence-disposition-v1.md), [machine receipt](../../evidence/04A30/04A30-evidence-disposition-v1.json), [EOL qualification](../../evidence/04A30/04A29-eol-qualification-v1.json), [source successor](../../evidence/04A30/04A28-source-transition-successor-v1.json), [new tests](../../evidence/04A30/source-transition-tests-v1.json), [historical FAIL rerun](../../evidence/04A30/historical-source-pin-rerun-v1.json). New source-helper tests/AST checks are RUN; prior managed/Node/Python/strict/MkDocs executions below are REUSED_PREVIOUS_04A30_EXECUTION_EVIDENCE,not reruns. Full matrices,native/R and historical payload scans are NOT_RUN in this closeout.

## Entry, authority and publication

Branch `codex/04A-30`; real merged-main entry and unchanged HEAD `45466a88e7567c9f4711ba4fbbe295a7a2e69bb4`. That commit is the accepted 04A.29 audit, whose parent is `02a6d01cd3520116539fcefdd77612bbd9550129`; accepted 04A.28 `ed94086e449f22ee1a0fc3888d6209ecba85a8b5` and test rationalization `033cbe9033cc10bce3e19322e5582e5ea389fe22` are ancestors. Entry had no tracked edits or staged files. The first branch command was denied by the sandbox; the authorized metadata-only retry created the correct branch. No reset/stash/clean/stage/commit/merge/push occurred.

Revision 2.11's stale handoff heading is retained; the owner's acceptance controls status. AGENTS.md, the plan and original 04A.02/03 restrictions were inspected and preserved. Canonical 04A.29 v2 remains frozen: 1,031 unchanged,19 relocated,5 replaced/renamed,5 split,2 uncertain (171/172),zero unmapped. Neither uncertainty was discharged. [Entry state](../../evidence/04A30/entry-state.json), [exact entry pins](../../evidence/04A30/entry-pins.json) and [inherited manifest discrepancy list](../../evidence/04A30/inherited-04A29-manifest-discrepancies.json) qualify the actual checkout. All 93 mismatches match their committed blobs; they are not local drift and were not repaired here.

The complete proposed publication, including product/tests, this single owner dossier and all 04A.30 evidence, is [publication-manifest.json](../../evidence/04A30/publication-manifest.json). It includes itself with null bytes/SHA to avoid recursive hashing. The corrected 04A.29 canonical crosswalk and its 52-file superseded attempt are untouched. Seventy private historical path names use stable SHA-derived aliases in public entry pins; raw diagnostics and temporary implementation scaffolding remain in the fresh ignored run root.

## Operation ownership and actual integration

The [pre-edit map](../../evidence/04A30/migration-operation-map-entry.csv) was recorded before product edits. The [final operation map](../../evidence/04A30/migration-operation-map-v1.csv) gives 15 overlapping member/suboperation records, each linked to an accepted 04A.29 ID, exact entry source/body hash and actual new destination IDs. These are eight scope families, not 506 edits or 15 distinct algorithms. The Box label plan was refined to preserve the original constructor label-array alias; the [refinement receipt](../../evidence/04A30/implementation-map-refinement.json) records why.

| Family | Existing/current owner and new bounded seam | Actual Windows consumer and evidence | Later owner |
|---|---|---|---|
| ROC | New `RocDataCore` moves the original cutoff/count/AUC/Hanley/DeLong/midrank operations. Existing Core distributions/array routines reused. | `ROC.compute` applies `ComputeCurve`, then `DeLongSE` calls `ComputeDeLong`; `MidRanks` forwards. Frozen-source tests and real Framework wrapper tests pass; `wrapResults`/`addROCplot` signatures and bodies retained. |04A.31; disputed numeric policy39–41|
| Normal Q–Q | New `NormalPlotDataCore` sorts a copy and computes original probabilities/Z/reference endpoints with existing Core statistical routines. | `NormalPlot.New/compute/computeRefLIne` delegate; `addChart` retained. Three score × three reference choices, ties/constants/small/nonfinite/fallback/pre-compute cases tested. |04A.31; detrending68–69|
| Box | New `BoxSegmentsCore.Compute` returns five signed Peltier segments only. DescriptiveStat/whisker/outlier/jitter owners retained. | `CalcForPlotting` consumes scalars; frozen whole-method and Framework adapter comparisons pass. No repeated collection reset. |04A.32|
| Histogram | New `HistogramDataCore` protects existing `HistogramBinningCore` with clones and moves midpoint horizontal bounds. No second engine. | `Histogram.compute` calls copy wrappers; `addChart` uses bounds. Existing bin/overlay tests and source-bound/Framework checks pass. |04A.32; raw-bin toggle75|
| Axis | `ChartAxisScalingCore.Compute` was already migrated. No new calculator or bridge edit. | Existing `ChartingFunc.ChartScaling`→mutable `CHARTscale.Min/Max/Scale`; existing and new edge adapter tests pass. |04A.32|
| Scatter matrix | New `ScatterMatrixDataCore.Normalize` owns n×p panel-scaled values and diagonal label X vector. | `transformData` consumes results; frozen transform and Framework regression/sentinel parity pass. Constructor still forces `.1`. |04A.33|
| XYZ | New `XyzDataCore.NormalizeAxis` owns the centered-cube axis transform only. | `getnorm` calls it for aligned X/Y/Z. Source and Framework tests cover direction/manual bounds/zero span/ragged/zero-point behavior. |04A.33|
| Labels/groups | New Common `OrderedPlotGroups/OrderedPlotPoint` composes `GroupedNumericData` and accepted `UnivariateObservationSource`. | `NormalPlot.New` gets the supplied display name; Box `SnapshotGroupNames` supplies category/table labels at consumption. No parser/grouping rewrite or specialty-family mass refactor. |04A.31/32; specialty34/37|

Real entry callers were traced in `UI/UibyID.vb` (ROC/Normal/Histogram/Box), `Ui0OneRefeditMulticol.vb` (Box), `Ui11PCA.vb` (scatter), `Ui3XYZplot.vb` (XYZ), and assumption/ANOVA result adapters. Those acquisition/caller files are unchanged. Existing configurable and univariate parsers remain acquisition authority; a Common test passes their filtered physical-row sources through the new descriptor. No new host GUI or Office/Mac plotting capability is implied.

## Shapes, identity and compatibility

ROC keeps positive group0/negative group1, sorted concatenated copies, midpoint cutoffs including terminal max+1, tie0.5 credit, both CI families/alpha/table order, u cutoffs/specificities and u+1 plotted sensitivity/FPR vectors. Plot X is 1-specificity; Y is sensitivity; initial(1,1),terminal(0,0). The explicit Windows accumulator seed retains repeated `compute` accumulation. Applying the Hanley snapshot before the DeLong call preserves the characterized singleton failure state. Exact invalid-input exception types are tested; arbitrary pre-DeLong invalid-call private-state persistence is not claimed and remains a 04A.31 edge contract. Core `Compute` is first-call/stateless; its partial compatibility entry results are documented precisely in [portable contracts](../../evidence/04A30/portable-plot-contracts-v1.json).

Q–Q keeps literal **Blom / Rankit / Van Der Waerden**, **SPSS / OLS / R**, average ties, existing quartiles/rounding and `Slope/Intercept` argument order, X=data/Y=normal scores and 2×2 reference endpoints. Unknown score labels still yield zero vectors; unknown reference labels still produce zero Y endpoints. The Core snapshots/getters are copied, while the Windows returned score-vector alias remains supported through the reference-line compatibility overload. There is no detrended mode or new R semantics.

Two authorized input-only deltas are explicit: the old Normal constructor sorted the caller array; the new constructor sorts a clone. Histogram FD/overlay used to sort caller arrays indirectly via quartiles; its new wrappers clone before invoking the unchanged engines. Tests characterize the old mutations and assert new input integrity on success/error/repeat paths. Numeric source comparisons pass; no scientific correction is claimed.

Box retains the original quartile/whisker/outlier definitions and five signed segment channels. Histogram retains N×2 typed midpoint/count bins, exact PrettyBreaks default always on, overlay values and native secondary/Y-band/Tukey/event semantics. A new standalone empty `HorizontalBounds` request is rejected explicitly; native `addChart` still exits before calling it for zero bins. Scatter retains n×p orientation, p×p placement, `.1` margin overwrite, correlations/regression order and -100 break interpretation. XYZ retains raw alignment, `[-.5,.5]` normalization/directions, existing bounds/manual limits/nonzero-range policy and downstream rotations/shift/zoom conventions. A final empty-object regression was caught and corrected: count0 with Nothing axis returns an empty vector as the old loop did.

Common preserves supplied group order, raw vs display scalar types, duplicates/case/whitespace and ragged/empty/missing vectors. It does not trim/regroup/deduplicate/format, filter nonfinite observations or coerce native categories. Group ordinal, source observation index, chart index and optional physical WorksheetRow are distinct; explicit permutations preserve shuffled/filtered rows. Existing Windows constructors do not carry physical rows, so none is invented. New numeric results expose sorted values; complete source-to-sorted permutation projection remains a specific later 04A.31 contract. Box keeps the original label-array alias and snapshots its latest values on category/table consumption. Non-zero-bound CLR rank-one arrays are not silently coerced into typed vectors. New Common/Core files contain no native/filesystem/drawing/timer dependencies.

## Changed and new files

Six existing production files changed:

- `BESHstatNG/src/Graphics/ROC.vb`
- `BESHstatNG/src/Graphics/NormalPlot.vb`
- `BESHstatNG/src/Graphics/BoxPlot.vb`
- `BESHstatNG/src/Graphics/Histogram.vb`
- `BESHstatNG/src/Graphics/ScatterPlotMatrix.vb`
- `BESHstatNG/src/Graphics/XYZscatter.vb`

Seven new portable production files:

- `BESHStat.Core/src/Graphics/RocDataCore.vb`
- `BESHStat.Core/src/Graphics/NormalPlotDataCore.vb`
- `BESHStat.Core/src/Graphics/BoxSegmentsCore.vb`
- `BESHStat.Core/src/Graphics/HistogramDataCore.vb`
- `BESHStat.Core/src/Graphics/ScatterMatrixDataCore.vb`
- `BESHStat.Core/src/Graphics/XyzDataCore.vb`
- `BESHStat.Excel.Common/src/Analysis/OrderedPlotGroups.vb`

Five new test sources and the existing Framework test project include changes:

- `BESHStat.Core.Tests/src/Graphics/Graphics30Tests.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacyGraphics30.vb`
- `BESHStat.Core.Tests/src/Graphics/LegacyBoxBounds30.vb`
- `BESHStat.Excel.Common.Tests/src/OrderedPlotGroups30Tests.vb`
- `BESHStatNG.Test/src/Charts/Graphics30AdapterTests.vb`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj`

The test project links the same new Core/Common fixture tests into Framework4.8. No existing test body or expected value was edited. The new legacy fixture files retain nine exact original method statement bodies, with namespace/access/host-field scaffolding only; their arithmetic is not a second copy of the new algorithm. The Windows Matrix wrapper's already-existing forwarding targets are reused in the fixture shim. [AST guard](../../evidence/04A30/source-ownership-guard.json) preserves every original Graphics member declaration/default, accounts for 13 bounded old-member deltas plus one private Box snapshot helper, verifies 38 compiled files and checks the new portable dependencies. Histogram's native chart tail is token-identical; other native bodies are unchanged except Box's category argument consumes the neutral label snapshot. Chart creation/series/style/Shapes/exports/events/cleanup remain Windows-owned.

## Executed validation and genuine nonpasses

| Suite | Entry Debug pass/fail | Final Debug pass/fail | Final Release pass/fail |
|---|---:|---:|---:|
|Core|1058/0|1087/0|1087/0|
|Common|8974/5|8981/5|8981/5|
|Legacy|9208/9|9248/9|9248/9|
|OfficeAddin|508/0|508/0|508/0|
|OfficeExecution|45/0|45/0|45/0|
|ControlMitigation|61/0|61/0|61/0|


Final Debug raw managed **19930 PASS / 14 FAIL**; full Release managed **19930 PASS / 14 FAIL**. Exact before/after identities, including duplicate raw multiplicities, have zero removed cases or changed existing managed outcomes. All new cases pass. [Managed identities](../../evidence/04A30/managed-test-identities-v1.csv) and [TRX/source receipts](../../evidence/04A30/managed-validation-v1.json) preserve the evidence. Entry Release was NOT_RUN; final Release was fully executed. The comparison to fresh original-source Debug is labeled explicitly, not presented as an entry Release run.

Focused final runs: **Core120/120,Common7/7,Legacy42/42** in each Debug and Release. These include40 new unique cases/76 new raw executions per configuration, plus unchanged histogram/violin/categorical/axis suites. [Focused identities](../../evidence/04A30/focused-validation-v1.json). [Casebook](../../evidence/04A30/graphics-compatibility-casebook-v1.json) contains 70 successful numeric case IDs with actual old/new IEEE bits, NaN/infinity flags, shapes/options/source links and runtime/TRX hashes. Error compatibility and copy/identity contracts are identified as assertion evidence; no fabricated exception/output values or scientific golden is supplied.

Node **916/916 PASS**. Python final scope aggregate **840 PASS /106 FAIL /10 ERROR**,956 runner cases. The full verified matrix itself reported838/108/10, including two later-fixed publication-privacy failures; the final745-case scripts rerun reported640/100/5 and is combined with the unchanged tools/help/Common lanes above. Entry Python was844/102/10. The two privacy checks initially saw provisional raw entry-path metadata and later synthetic credential/drive examples in security-test display names. Seventy historical path aliases and stable display hashes fixed those exports; raw identity hashes/class/method/multiplicity remain preserved. Six failures now come from the unchanged04A28 whole-source snapshot test, one per authorized Windows file. The remaining100 failures/10 errors retain their earlier identities. These six remain real FAIL subtests; their dedicated successor is the exact entry-delta/AST/call-through proof, not edited historical expected hashes. [Source/Node identities and deltas](../../evidence/04A30/source-and-node-validation-v1.json). Unittest primary descriptions were reconstructed around interleaved Git warnings; subtest failures were not discarded. Raw Windows stderr encoding is recorded where needed.

Strict audit exit **1**, profile exit **1**, strict MkDocs exit **0**. No strict gate was softened. The stock verifier was inspected but **NOT_RUN unchanged**, because it writes historical `TestResults/Codex` and shared `bin/obj`. Its complete portable/Windows lanes were executed through the accepted isolated runner; no default historical destination was used. Compile-only Windows targets skipped version/help generation, XLL packaging and post-build/signing/deployment. All commands, real durations and raw diagnostic hashes are in [command-outcomes](../../evidence/04A30/command-outcomes.json). No network/package restore or SDK install occurred. The audit helper reused already-existing empty net10 SDK assets; raw scaffolding and all failed intermediate receipts remain local.

Full Debug elapsed 416.06s; full Release managed 173.62s; scoped historical metadata traversal 37.51s. No speedup is claimed. Earlier product/focused runs were retained and superseded after the XYZ count0 fix; final accepted-for-review source hashes are the `verified` receipts.

## Preservation, safety and gates

All3,820 tracked entry files were pinned once. The only changed tracked files are the six approved Windows Graphics sources and the Framework test project; original12 Core Graphics algorithms, other32 Windows Graphics files, Common/Office/result writers/UDF/Ribbon/Designer/AGENTS/plan and all frozen authority remain byte-identical to actual entry. All100 mixed-model captures match their original registry. Historical TestResults has280,953 files/138,180,795,001 bytes and the same membership/size/mtime digest as the accepted receipt. Payload bytes were **NOT_REHASHED**; no stronger claim is made. [Source preservation](../../evidence/04A30/source-preservation.json).

Entry process inspection found no build/test/Excel writer and six Node processes; CIM command-line inspection was denied. Fresh isolated output/obj/log/TRX/sandbox roots and serial inner test hosts avoided shared outputs; no permanent exclusive lease is asserted. Final source protection checks found no unexpected writer change. Private diagnostics stay local; the publication privacy/trackability/index/schema checks are recorded in their receipts. Git index is empty and HEAD remains the entry commit; no publication operation was performed.

**P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.** Gate C3/native releases remain unauthorized. Zero native releases, zero original-control discharges and zero golden promotions. Native Excel/COM/UIA/RefEdit/chart appearance and independent R are **NATIVE_NOT_RUN / NOT_RUN / NOT_GOLDEN**. 04A.29's24 helper passes and04A.28 matrices are historical references only. All C13/C17/C21/C24/C25/C28 candidates,171/172 uncertain contracts and original overlapping obligations remain open.

## Handoff and proposed owner smokes

[Handoff](../../evidence/04A30/graphics-roadmap-handoff-v1.json) maps remaining operations to the official04A.31/32/33 packages and retains specialty04A.34–37 ownership. There is no claim that ROC/Q–Q corner cases, all box/hist/violin aggregates or all3D/scatter geometry are complete. Histogram raw-bin/PrettyBreaks UI belongs04A.75; detrending belongs04A.68–69. Defect corrections/native closure remain separately gated04A.39–48 work. No later batch started.

Three proposed owner checks, **NOT_RUN pending separate native authorization**:

1. ROC with tied two-group observations: compare both CI tables, cutoff order, ROC/diagonal and unchanged source-sheet ordering.
2. Ordinary Q–Q from unsorted duplicates: Blom plus a reference line; verify unchanged source data and X-data/Y-score orientation.
3. Mixed-sign box/histogram or XYZ manual-bound/inversion case: compare labels, overlays/native geometry and cleanup against the reviewed source/build.

**NO_USER_VISIBLE_HELP_CHANGE**; help routes, options, installed identity and Ribbon/UDF signatures are unchanged. No Office/Mac GUI or browser E2E is newly enabled; those manual statuses remain NOT_RUN. After the remaining original-byte qualification, send this Codex report and updated full ZIP for independent04A.30 acceptance review. Acceptance is software-only and does not authorize04A.31, scientific corrections or native observation.
