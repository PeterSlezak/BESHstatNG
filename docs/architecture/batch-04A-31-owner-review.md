# Official batch 04A.31 owner review

**READY_FOR_OWNER_04A31_ACCEPTANCE_REVIEW** — software portability/compatibility only. No batch acceptance blocker identified; inherited full-suite nonpasses remain real failures.

Branch `codex/04A-31`;actual verified post-04A.30 merged-main base/unchanged HEAD `f840b1695fd95be25faba6fd5408c9498a31d804`;parent `45466a88e7567c9f4711ba4fbbe295a7a2e69bb4`. Entry tracked tree/index were empty;no owner untracked file was overwritten. Branch creation required sandbox Git-metadata approval. Other Codex/Node/GitHubDesktop processes were visible;three MSBuild processes were idle nodes. No global-writer exclusivity is claimed. All new outputs are isolated. [Entry authority](../../evidence/04A31/entry-state.json) records the stale Revision2.11 plan handoff and exact matching official04A.30–33 rows;the plan/AGENTS remain unchanged.

The actual merged compiled source is authoritative. Accepted04A.30 Core engines and all18 pre-existing Core Graphics files remain byte-identical to entry. Its119 externally hashed publication files were authenticated against the existing local pre-merge publication archive:14 direct,105 exact CRLF-to-LF transformations to committed main blobs,zero unexplained. This qualifies new post-merge normalization,including mixed endings;the archive was never branch authority. Accepted04A.29 original-byte receipt remains frozen:120 authenticated,27 direct/93 transformations. No old archive was requested or historical proof rewritten.

## Delivered files and ownership

- `BESHstatNG/src/Graphics/ROC.vb`
- `BESHstatNG/src/Graphics/NormalPlot.vb`
- `BESHStat.Core/src/Graphics/RocPlotSeriesData.vb`
- `BESHStat.Core/src/Graphics/NormalPlotSeriesData.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics31RocTests.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics31NormalPlotTests.vb`
- `BESHStat.Core.Tests/src/Graphics/Graphics31Evidence.vb`
- `BESHStat.Excel.Common.Tests/src/Graphics31SourceIdentityTests.vb`
- `BESHStatNG.Test/src/Charts/Graphics31AdapterTests.vb`
- `BESHStatNG.Test/BESHStatNG.Tests.vbproj`

These are two existing production adapters,two new portable production files,five new test/helper sources,and additive Framework test-project Compile entries. No Common production edit was necessary:existing `OrderedPlotGroups.PointOrder` already supplies the correct provenance seam. Other36 Windows Graphics modules,all original12 Core Graphics implementations plus six04A.30 engines,UI callers,Ribbon/UDFs,host capabilities,targets,packaging and installers are protected. No04A.32 work began.

[Pre-edit operation map](../../evidence/04A31/roc-qq-operation-map-entry.csv) and [final map](../../evidence/04A31/roc-qq-operation-map-final.csv) bind source paths,entry/final hashes/ranges,04A.29 member/operation IDs and04A.30 map links. `RocDataCore.ComputeCurve/ComputeDeLong/MidRanks`,`NormalPlotDataCore.SortedCopy/Scores/ReferenceLine`,existing ranks/distributions/quartiles/regression and Common grouping/source contracts are reused. No second numerical engine was introduced. The initial read traced `UI/UibyID.RunROC/RunNormalPlot`,binary-classification reporting and two ROC UDF routes;all callers remain unchanged.

## Coordinate and provenance contracts

`RocPlotSeriesData` snapshots X=FPR,Y=sensitivity in exact accepted order,initial(1,1),terminal(0,0). There are u cutoffs/specificities,u+1 X/Y points. Cutoff i maps curve slot i+1;slot0 has no cutoff. `CutoffPointIndexes` exposes this relationship;there is no fabricated extra threshold. Diagonal X/Y each `[0,1]`;axis0..1,major0.2,crossing0. Every getter copies. Existing CI-copy behavior is retained. [Exact ROC contract](../../evidence/04A31/roc-plot-coordinate-contract.json).

`NormalPlotSeriesData` exposes n observed-X/normal-Y/probabilities,exact method strings,n forward/inverse identity slots and a2×2 reference matrix:rows=endpoints,columns=X/Y. All three score methods × all three line methods reuse accepted calculations. `NormalPlotObservationOrder` calls unchanged `SortedCopy`;each sorted IEEE value takes the earliest unused original ordinal with identical bits. Identical-bit ties are stable. Numeric-equal signed zeros/distinct NaN payloads keep the legacy sorted coordinate slots;no historical Array.Sort stability is claimed. Both permutations and every coordinate/line getter copy independently. [Exact Q–Q contract](../../evidence/04A31/normal-qq-coordinate-and-permutation-contract.json).

Original input ordinal,sorted plot ordinal,Common group ordinal and neutral ChartIndex are zero-based and distinct. Optional physical row is one-based and comes only from actual `UnivariateObservationSource`;missing physical rows remain nullable NotAvailable. The proposed native point ordinal is sorted slot+1,not a worksheet row,and is NATIVE_NOT_RUN. Common tests prove shuffled supplied physical rows and accepted-parser filtered-row alignment. The real Windows constructor receives values/name only;`PlotPointOrder` exposes group0/input/sorted identity without invented physical rows.

Windows `ROC.compute` consumes the projection;`addROCplot` uses X/Y and Core diagonal. `NormalPlot.New/compute/computeRefLIne` consume order/series projections;`addChart` uses observedX/expectedY/reference columns. Public legacy signatures/default alpha/string spelling remain intact. Mutable Windows return-array aliases and ROC seed/repeated-call/partial DeLong-failure state remain Windows-owned. Fresh immutable snapshots capture current alias edits without changing earlier snapshots. `wrapResults` is token-identical. [Roslyn source guard](../../evidence/04A31/source-ownership.json) proves renderer differences are bounded coordinate substitutions;creation,size,labels,series order,colors,markers,axes,cleanup and object lifetime remain native-owned. This is source evidence,not executed Excel calls or chart equivalence.

## Executed validation

|Managed suite|Debug PASS / FAIL|Release PASS / FAIL|
|---|---:|---:|
|Core|1113 / 0|1113 / 0|
|Common|8984 / 5|8984 / 5|
|Legacy|9288 / 9|9288 / 9|
|OfficeAddin|508 / 0|508 / 0|
|OfficeExecution|45 / 0|45 / 0|
|ControlMitigation|61 / 0|61 / 0|

Full Debug: **19999 PASS / 14 FAIL**. Full Release: **19999 PASS / 14 FAIL**. Every inherited raw identity,multiplicity and outcome matches the accepted04A.30 reference;no existing case removed. New raw cases per configuration:69,all PASS. This is a fresh full Release execution,not the04A.28 trial. Prior04A.30 counts are historical references only. [Managed identities](../../evidence/04A31/managed-test-identities.csv) and [validation/source-build/TRX pins](../../evidence/04A31/graphics31-validation.json).

Focused final: Debug Core 55/55; Debug Common 10/10; Debug Legacy 80/80; Release Core 55/55; Release Common 10/10; Release Legacy 80/80. These include unchanged04A.30 regressions. ROC matrix covers both separation directions,overlap,ties,constant/unbalanced/boundary/nonfinite/negative-zero/floating ties,alpha,permutations,copies,CI copies and explicit seeds/repeated Windows calls. Q–Q covers all nine combinations,sorted/reversed/shuffled/tied/constant/singleton/two-point/empty/missing/nonfinite/low/high magnitude data,exact string fallbacks,pre-score calls,mutable adapter aliases and source identities. [Casebook](../../evidence/04A31/graphics31-compatibility-casebook.json) records source-backed expected versus actual IEEE output,fixtures,options,culture,shapes and exceptions. Finite/infinite/signed-zero values compare by bits;numeric NaNs compare by classification;copy/provenance checks use exact bits,including NaN payloads. No floating tolerance is used. Expectations come from frozen source or explicit metadata fixtures,never from the new implementation presented as an independent oracle.

Node: **916 PASS / 0 FAIL**,including all `.cjs` suites. Python full aggregate:**840 PASS / 106 FAIL / 10 ERROR** out of 956 reported cases;all explicit subtest failures preserved. Python identities/nonpass deltas against the accepted04A.30 reference are in [source/Node receipts](../../evidence/04A31/source-and-node-validation.json). Additional new publication/lineage audit tests:**12 PASS**,including adversarial duplicate/missing/hash/size/self/path-escape cases and post-seal validation. The compiled Core API guard also executes two independent inverse-permutation copy checks with hand-specified metadata expectations. Strict/profile exits **1/1**;strict MkDocs exit **0**. None of these nonzero statuses is reported green. [Nonpasses](../../evidence/04A31/nonpasses.json) separates inherited issues,post-merge EOL receipt effects and any blockers.

Core/Common netstandard2.0 and Windows Framework4.8 compile in both configurations with isolated outputs. Windows compile-only targets omit packaging/post-build/signing/version/help generators. No restore/install/network request occurred. The unchanged verifier wrappers themselves are NOT_RUN because they hard-code shared historical output paths and Legacy packaging hooks;their complete portable/Legacy managed equivalents were executed through the accepted isolated runner. Exact commands,real exits and failed intermediate receipts are retained in [command outcomes](../../evidence/04A31/command-outcomes.json). New Python evidence/manifest/privacy tests are separately recorded there;the full existing Python matrix remains unmodified.

## Preservation, restrictions and handoff

[Preservation](../../evidence/04A31/source-preservation.json) checks every3933 entry tracked file:only the two authorized adapters and additive test project changed. All100 mixed-model captures authenticate against the frozen registry;04A.01 decisions,04A.29 canonical-v2 lineage/uncertainties,04A.30 source/evidence and all historical tests/expected values remain unchanged. [Source successor](../../evidence/04A31/source-transition-successor.json) retains the278-pin lineage:272 original unchanged,four retained04A.30 transitions,two further authorized04A.31 transitions. The historical six failing source-pin subtests remain unchanged failures. [Historical stat receipt](../../evidence/04A31/historical-results-preservation.json) compares membership,size and mtime only;the138GB historical payload was NOT_REHASHED. No historical result was deleted,archived or rewritten.

[Deferred register](../../evidence/04A31/deferred-issues.json) retains ROC cutoff-table versus curve-slot indexing,nonfinite/terminal/alpha methodology,legacy repeated state,Q–Q degenerate/quartile/R fitting semantics and unavailable physical row acquisition. Scientific/report investigation belongs04A.39–41,native confirmation04A.45–48;detrending04A.68–69. No statistical correction was activated. Box/Histogram/Violin/axis remain04A.32;other families remain04A.33–37.

P=NO;N1=NO GO;N2=NO;Gate C2=UNMET;Gate C3 not approved. Original native controls/deferred obligations remain OPEN,zero releases/discharges. All comparisons **NOT_GOLDEN**. Actual Excel/COM/UIA/RefEdit/chart rendering/callbacks/native smoke and independent R/scientific oracle execution are **NOT_RUN / NATIVE_NOT_RUN**. Software compatibility is not independent scientific accuracy,chart appearance equivalence or Office/Mac availability. [Proposed synthetic native smoke](../../evidence/04A31/native-smoke-proposal.md) is reproducible but requires separate authorization. Browser/WASM manual smoke is NOT_RUN;no new production renderer is introduced.

Final intended publication is exactly [publication-manifest.json](../../evidence/04A31/publication-manifest.json):relative paths,byte sizes,SHA256 for every file,and a null self-entry to avoid recursion. [Privacy](../../evidence/04A31/privacy.json) reports sanitized published paths/identity displays;private raw logs remain ignored. Integrity,manifest,traceability,Git diff and empty-index checks must pass at sealing. Nothing is staged,committed,merged,pushed,deployed,signed or installed. Owner publication and independent acceptance review remain separate;04A.32 is not started.
