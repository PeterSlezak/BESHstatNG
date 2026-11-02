# 04A.07 — ANOVA/Kruskal–Wallis workflows: owner review

**READY_FOR_OWNER_04A07_ACCEPTANCE_REVIEW.** Implementation, Debug/Release
validation and preservation checks are complete. Owner acceptance is pending.
This is software/source/managed evidence,
not native Excel verification or a numerical golden promotion.

## Git baseline and authority

Branch `codex/04A-07` was created from clean local `main`, full commit
`b984e20bd5a8112a4f22ade94df8851cca3a61e8`. HEAD, main and locally cached
`origin/main` initially named this commit. No fetch or other network operation
occurred. History contains accepted 04A.06 over 04A.27 (`4a3d2e16…`), 04A.05,
04A.04 and 04A.03. The owner's current explicit acceptance/merge confirmation is
the authority; earlier immutable dossiers' pending-review wording and Master Plan
revision 2.4's stale status text were not rewritten. No 04A.08 work was started.

The sandbox initially refused Git's index lock; only the already-authorized branch
creation was retried with escalation. No staging, commit, reset, stash, merge,
rebase, cherry-pick, cleanup, push or remote mutation occurred. No unrelated owner
change was present or overwritten. HEAD remains the starting commit.

Before production edits, [baseline-v1.json](04A07/baseline-v1.json) captured full
report member bodies, source hashes, options/order/ownership and a frozen test-only
RM/nested oracle. Initial tracked and ignored authority hashes and source preimages
are retained locally under `TestResults/04A-07-20261102-run01/`. The initial status
record contains only this batch's newly created verifier as an untracked file.

## Production ownership and actual call chains

| Shipping route | Common ownership now | Native remainder |
|---|---|---|
| `UibyID.RunOneWayANOVAThroughSharedLayer` | `RunGrouped` calls existing dispatcher once; immutable options/plan; homogeneity → descriptive → box-summary stages; LSD → Bonferroni → Tukey–Kramer → Games–Howell plot payload order | Checkbox/global-alpha snapshot, original analysis checkpoint and detail guard; unchanged shared output/diagnostic/lifecycle/chart closure |
| `UibyID.RunKruskallWalis` | Existing KW/Dunn handler once; legacy Dunn=True; descriptive → homogeneity → box-summary stages | Original checkpoint, detail guard, shared writer and box-render closure |
| `Ui0OneRefeditMulticol.Run1WayRmANOVA` | Core compute → optional GG → HF → TukeyKramerRM2 → Tukey → wrap → optional Mauchly → descriptive; portable table capture and box-summary decision | Existing checkbox snapshot, destination/overwrite/write sequence and Excel box renderer |
| `Ui9ANOVA2nested.Run2WayNested` | Core nested model, compute/wrap, balanced flag, two-line banner and Satterthwaite separator metadata | Existing writer construction/placement/overwrite logic; same banner/separator values supplied by Common |
| Existing grouped/blocked/nested producers | Existing C03/C04 policies still parse; weak sidecars retain their immutable original input/provenance | Same acquisition, missing/header/culture behavior, legacy return shapes and workbook checks |

New portable API is in
`BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb`:

```vb
AnovaWorkflowPolicy.RunGrouped(input As AssumptionReportInput,
    options As AnovaGroupedWorkflowOptions, Optional cancellation As CancellationToken = Nothing)
    As AnovaGroupedWorkflowResult
AnovaWorkflowPolicy.BuildSupplements(input As AssumptionReportInput,
    options As AnovaGroupedWorkflowOptions, Optional cancellation As CancellationToken = Nothing)
    As AssumptionReportResult
AnovaWorkflowPolicy.RunRepeated(input As SpreadsheetNumericBlocks,
    options As RepeatedAnovaWorkflowOptions, Optional cancellation As CancellationToken = Nothing)
    As AnovaModelReportResult
AnovaWorkflowPolicy.RunRepeated(matrix As Double(,), names As String(), options,
    Optional sources As SpreadsheetNumericBlocks = Nothing, Optional cancellation = Nothing)
AnovaWorkflowPolicy.RunNested(input As SpreadsheetNestedInput, Optional cancellation = Nothing)
AnovaWorkflowPolicy.RunNested(matrix As Object(,), names As String(),
    Optional sources As SpreadsheetNestedInput = Nothing, Optional cancellation = Nothing,
    Optional originalSource As SpreadsheetNestedInput = Nothing) As AnovaModelReportResult
```

`AnovaGroupedWorkflowOptions`, `RepeatedAnovaWorkflowOptions` and
`AnovaWorkflowPlan` are immutable snapshots. `AnovaReportTable` uses existing
`AnalysisTableArtifact`/`ResultTableOutputModel` and recreates fresh legacy table
components, retaining headers, titles, footnotes, p-value columns/cells and cell
types. It does not parse numerical CI text. Existing Core-supplied display strings
are retained pending later formatting work. `ComparisonPlots` defensively copies
numeric payloads. The grouped result explicitly retains the **existing mutable
legacy analysis object** for the established writer; new consumers can use its
immutable `Document` and the immutable workflow plan.

`WindowsAnovaWorkflowAdapter` exposes `AnalyzeGrouped`, `PrepareSupplements`,
`Repeated` and `Nested`. These are actual shipping seams, not demonstration calls.
They acquire no Excel object and perform no writes. Box `Calculate/CalcForPlotting`
stays in the existing Windows graphics class; its six-column result mapping reuses
04A.06 `AssumptionReportPolicy.BoxSummary`. Rendering, coordinates and chart
creation remain native and were not migrated. No plotting redesign was attempted.

The existing shared ANOVA/KW services, handlers, catalog, registry and option
constructors were not changed. `SpreadsheetAnalysisKind.OneWayAnova=1` and
`KruskalWallis=2`, stable analysis IDs and option defaults remain intact. RM/nested
facades are standalone reusable Common APIs; they do not register new Office
editors or assert availability. Core/Common remain `netstandard2.0`, Windows
remains .NET Framework 4.8; no package/framework/export/schema change.

## Output and option compatibility

| Workflow | Preserved numerical/option policy | Output order/format evidence |
|---|---|---|
| One-way | Existing classical engine, optional Welch and all four post-hoc procedures; unequal n supported by existing engines | Primary/Welch table then selected LSD, Bonferroni, Tukey–Kramer, Games–Howell tables; original Core headings/CI footnotes/p-value markers; corresponding plot payloads in the same order |
| KW | Raw H/p and tie-corrected H/p stay separate; API Dunn optional, actual GUI Dunn always True | Existing primary table then Dunn/Bonferroni table; descriptive precedes homogeneity, unlike ANOVA |
| RM | GG/HF independently selectable; one Tukey flag still invokes **both** existing variants in order; Mauchly independent | Original expanded primary table, no-sphericity Tukey table, pooled-sphericity Tukey table, Mauchly, optional descriptive, optional box summary |
| Nested | Existing balancing/df/variance-component/Satterthwaite engines unchanged | Existing two tables; prelude `Two-Way Nested ANOVA`, `Balanced design = True/False`; original two-row separator headed `Satterthwaite approximation` |

Primary one-way/KW tables still flow through the original `RunSharedOutput` and
`GetSharedResultWriter`. The analysis checkpoint remains **after primary analysis,
before supplements**. Cancel/output target/lifecycle revalidation, disabled-control
handling and diagnostic writer stages remain unchanged. Native chart closures are
identical, including box-before-comparison charts, chart offsets and height spacing.
RM's complete native write/chart tail is identical. Nested's tail differs only in
receiving its banner/separator values from the portable result; its existing
`totrows = rr.TotRows + res.Count + res.Count - 1` formula and write order are kept.
All other members/imports/constructor/designer state in the three forms are identical.

Supplementary calculations use the accepted `AssumptionReportPolicy`; there is no
second descriptive or homogeneity implementation for these workflows. The old
`Ui0OneRefeditMulticol.ComputeDescriptiveStatistics` helper remains **unchanged for
unrelated analyses**, including Friedman. RM no longer calls it. RxC, Cochran,
ICC, Skillings–Mack, other chart paths, Bland–Altman and paired option policies
were not migrated or changed.

Alpha is still captured from `AppGlobals.DefaultAlpha`. Its existing Core global
setter normalizes invalid values to .05. Standalone grouped requests preserve the
existing shared invalid-alpha default **and warning**. The new repeated typed API
rejects invalid alpha; the actual GUI global getter already guarantees valid alpha.
No GUI alpha, multiple-testing adjustment, CI definition or p-value threshold changed.

### Original rows, scratch and producer types

The grouped primary adapter historically cloned vectors before dispatch. This
behavior is retained: primary scratch is **not** replayed into GUI data. Supplement
sorting effects are replayed as in 04A.06. Nested Core sorts its matrix in place;
the compatibility adapter replays that sort for legacy callers. Original provenance
is immutable and stays separate from sorted scratch; it is never relabeled.

`WindowsUnivariateInputAdapter.GetOriginalSelection`,
`WindowsPairedInputAdapter.GetBlockEvidence` and `GetNestedEvidence` expose weak,
operation-keyed immutable captures of the already-existing parser results. No
native object, workbook or event ownership is added. Existing return DTO shapes
stay unchanged. The grouped bridge attaches source mappings only while original
names/values still match; changed legacy vectors do not inherit stale row IDs.
RM checks original source dimensions, values, per-block row consistency and
duplicate known physical subject rows before Core. Reversed/noncontiguous original
row order is retained. Missingness still uses the accepted whole-block policy.

Some legacy contracts know physical row numbers but not workbook/worksheet IDs or
physical columns. Unknown fields remain unknown; no identity is inferred from
sheet names or array positions, and no cross-workbook matching is added.
Manually supplied arrays report `ANOVA_SOURCE_UNAVAILABLE`. Native workbook
verification remains at the unchanged acquisition boundary. The accepted physical
importer SHA remains
`32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9`.

The original `DataObj.CharCols` path converts categorical factor columns to
current-culture strings before nested ANOVA, including Boolean, date/numeric
labels. The new bridge consumes those original surviving values; it does not
redefine acquisition types. Standalone typed nested input explicitly carries
string labels; the raw compatibility overload preserves the supplied legacy
text/numeric runtime types. Extra response columns remain present, while existing
Core uses only the first response and the new result records that limitation.

A real development regression was reproduced in .NET Framework: if a response
column contains numeric text, legacy `DataObj` stringifies that column. A numeric
value `1.2345678901234567` can survive as `"1.23456789012346"`. The first bridge
version compared that Core-input number to raw typed capture and rejected an
otherwise valid legacy analysis. The repaired producer stores **both** snapshots:
`GetNestedEvidence` / `OriginalNestedInput` retain raw numeric capture, while
`GetNestedCoreInput` / `Nested` carry actual surviving Core-input numbers with
exact original coordinates. The matrix passed to Core is unchanged. Two new net48
producer-to-policy-to-Core-report tests, invariant and `sk-SK`, assert the real precision
difference and complete frozen-legacy table/metadata parity. No rounding algorithm
was introduced into Common and no legacy number was silently corrected.

This exposes **SC07-LEGACY-NUMERIC-TEXT-COERCION**, an existing input-fidelity
candidate for a separately reviewed consumer/ingestion correction. Changing the
actual legacy numerical input would need scientific/host-contract evidence and any
applicable native pre-change disposition; that correction is not applied here.
The earlier v1 source/validation/snapshot records and both initial full runs remain
retained. Current source pins and results are the v2 successors.

Parser exclusion warnings remain at the accepted input-policy/acquisition boundary;
these sidecars retain accepted observations and coordinates, without inventing a
reason for an arbitrary row gap. Typed callers can combine the original parser
`Validation` with workflow `Validation`; no new native warning dialog is added.


## Diagnostics and scientific evidence

Reports expose structured `SpreadsheetValidationReport` issues. Failures and
cancellation produce **no accepted partial tables**; the legacy throwing facades
retain original Core exceptions for the unchanged outer form error handling before
writer acquisition. Codes include `ANOVA_GROUPED_FAILURE`, `ANOVA_REPEATED_FAILURE`,
`ANOVA_NESTED_FAILURE`, `ANOVA_SUPPLEMENT_FAILURE`, `ANOVA_CANCELLED`,
`ANOVA_SOURCE_MAP`, `ANOVA_DUPLICATE_BLOCK` and `ANOVA_SOURCE_UNAVAILABLE`.
Undefined Core numeric cells remain their original values, with
`ANOVA_UNDEFINED_RESULT`; they are not silently changed to zero, finite endpoints
or NA. Warnings are separate from statistics. No new dialog/warning UI was applied.

The initial source characterization raised global nested-label aggregation as an
ambiguity. Full inspection confirmed that Core subsequently **rejects** a child
label appearing under multiple parents. The existing typed hierarchy parser can
represent that input, but this numerical engine requires globally unique child
labels. The workflow records `ANOVA_NESTED_GLOBAL_LABEL` and retains the Core
rejection; it does not silently recode factors, pool a new model or present a valid
estimate. Balanced/unbalanced supported cases retain their existing behavior.
The initial characterization remains retained rather than rewritten.

[reference-evidence-v1.json](04A07/reference-evidence-v1.json) records independent
read/hash verification of the two **retained** owner-local publication manifests:

* C30-SYN-ANOVA: `27a2594c2b52ebf5b1fad3abbf0ce5cab0afc5b1dbc6af08c7f5aa02f2221c89`;
* C30-SYN-KW: `ec88aebe4f2ea8182f65a0c43fe9b854804973e7879eded89f9473c52dbdb4fa`.

All eight retained C14 current-Core/reference comparison receipt hashes also match.
Those are historical receipts, not new R executions. Current workflow tests check
ANOVA F=2.3333333333333335, p=.177978515625, SS=14/18/32, and KW raw/corrected
H=4.622222222222227/4.781609195402297 and
p=.09915102252982504/.09155598849604304 with explicit 1e-12 tolerances.
Welch/RM/GG/HF/Mauchly constants are reused from existing Core tests, with their
documented tolerances; no new independent provenance is asserted for those tests.
A small new nested arithmetic candidate has grand mean 5 and SS(group,nested,
error,total)=8,52,8,68. It is **NOT_GOLDEN**. Option/source-oracle comparisons prove
compatibility with existing engines, not independent statistical correctness.

No Core formula or scientific policy was corrected. Inherited 04A.06 Shapiro
bounds/constant-data/D'Agostino overflow candidates remain unchanged. No R process
was run, two qualified independent scientific reviews are still missing, and no
reviewer identity/signature or owner P decision was fabricated. **P remains NO.**

## Exact restrictions and source proof

[source-restriction-crosswalk-v2.json](04A07/source-restriction-crosswalk-v2.json)
maps **29 source-associated controls**, with original declaration/CSV row hashes
and actual deferred/transfer membership. **21** have original OPEN obligations:
15 are in the shared UibyID ANOVA/normality set and six are the two multicol/four
nested RefEdit obligations. Eight other controls have original source-control
rows but no deferred obligation entry; their obligation ID is explicitly null,
not a fabricated `control:` ID. No native verification or release is inferred.

The shared source-associated controls include `btCompute`, `ckWelch`, `ckLSD`,
`ckBonferroni`, `ckTukey`, `ckGamesHowell`, `ckDescriptiveStatistics`, `ckBoxPlot`,
the four variance-test options and the three existing RefEdits. Exact literal IDs,
original source/designer pins and pre-change gates are in the machine crosswalk.
The six nonshared RefEdit IDs remain associated with their original ledger rows,
not invented members of the four-case transfer set.

The original amendment remains SHA-256
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`;
the original obligation ledger remains
`363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10`.
Every one of the **239** transfers is still OPEN, including the shared **141
blocked controls / 160 obligations**. These overlap the **218** transferred
controls, **802** wider live-control restrictions and **1,514** original obligations;
counts are not additive. All **44 importer timing obligations** remain OPEN, as do
the three 2×2 obligations (two RefEdits plus help, not three blocked controls).
Released controls **0**, discharged obligations **0**.

Only behavior-preserving extraction is active. **No new ANOVA/KW native timing
exception is claimed or requested for this applied software change.** The 2×2 and
importer timing exceptions do not authorize ANOVA/KW behavior changes. Missing
pre-change native captures remain mandatory before any later behavior-changing
restricted patch, with exact owner disposition required if evidence is unavailable.
No required wiring patch is left inactive. Native evidence remains missing and
must not be inferred from managed tests or source equality.

**N1 NO GO; N2 NO; Gate C2 UNMET.** Full native qualification still requires an
independently reviewed planned/live HWND and PID comparison, proof of the
actually loaded XLL/managed assembly against reviewed source/build, independent
Windows/COM risk review and exact stage authority. These remain 04A.45–48 work.
Borrowed RCW lifetime, RefEdit callbacks, renderers, designers and native helper
sources remain unchanged. No Excel-native helper/pilot was launched; source and
fixture checks do not qualify N1.

## Validation and protection

Final results are recorded in [validation-v2.json](04A07/validation-v2.json) and
the post-repair TRX files. The initial `final-Debug`/`final-Release` full runs and v1 summaries
remain **superseded, not erased**: the numeric-text precision edge was found after
those runs. Both configurations were repeated as `final-fixed-Debug` and
`final-fixed-Release` because production metadata adaptation and two tests changed.
This is the sole full-run repetition; it is justified by that genuine repair.
No old failing expectation was changed, skipped, xfailed or rebaselined.

| Suite | Final Debug passed / total | Final Release passed / total | Nonpasses / skips each |
|---|---:|---:|---:|
| Core | 1,055 / 1,055 | 1,055 / 1,055 | 0 / 0 |
| Common | 1,217 / 1,221 | 1,217 / 1,221 | 4 / 0 |
| Windows legacy | 1,087 / 1,088 | 1,087 / 1,088 | 1 / 0 |
| Office managed | 505 / 505 | 505 / 505 | 0 / 0 |
| WASM execution managed | 45 / 45 | 45 / 45 | 0 / 0 |
| Retained C06 control projection | 61 / 61 | 61 / 61 | 0 / 0 |
| **Distinct managed total** | **3,970 / 3,975** | **3,970 / 3,975** | **5 / 0** |
| Node | **916 / 916** | **916 / 916** | **0 / 0** |
| Python methods | **623 / 667** | **623 / 667** | **44 / 0** |

**237 new managed cases** pass per configuration: 136 Common and 101 Windows.
All ten additive Python successor tests pass. The final focused run passed
263 Common and 171 Windows cases (434 including earlier regression cases).
Existing C03/C06 control projection adds 61 executed cases; it remains a
**test-only projection with initialized managed controls**, never shown, omitting
Excel connectors/preferences/focus and not executing the full shipping constructor.
Its relevant constructor/option/homogeneity methods are unchanged in 04A.07.
No native GUI/COM/STA equivalence is inferred from it.

The repeated 15-case C02 semantic subset and all focused/initial full executions
are repetitions, not additional distinct tests. The final full commands retain
**exit 1** for the historical/source nonpasses; independent suites continue for
accounting. The suite is not presented as green.

The five managed nonpasses remain exactly:

* `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection`
  rows `(20,30,50)`, `(20,15,40)`, `(0,0,0)`;
* `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`;
* `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`.

Python has **44 nonpassing methods** (35 failed, nine errored), with **48 failure
events** because four additional subtest events belong to already counted methods.
All 36 preceding nonpass identities recur. One old C03 per-mode source check changes
from ERROR to FAIL: it now fails an earlier frozen in-form option-text assertion
before its old missing-substring error. This is still a nonpass, not a repair or
native qualification. The eight new nonpassing method identities are:

* `test_04a05_completion_successor.Completion05SuccessorTests.test_nonpaired_code_and_unrelated_forms_are_unchanged_from_partial`;
* `test_04a05_contingency.ContingencySourceGates.test_other_multicol_members_including_04a04_selection_unchanged`;
* `test_04a06_assumptions_successor.Assumptions06SuccessorTests.test_actual_form_routes_and_unchanged_native_tails`;
* the same C06 class's `test_native_population_is_union_not_sum`,
  `test_new_scientific_candidates_are_explicitly_unmodified`,
  `test_no_duplicate_statistics_in_form_or_windows_adapter`, and
  `test_production_sources_and_exact_open_ledger_lineage`;
* `test_investigation06.Investigation06Tests.test_anova_defaults_mapping_remain_explicit_source_facts`.

These freeze earlier whole-form/member scopes or look for old property initializers
inside the form. C06's whole-source verifier rejects the two newly authorized
grouped workflow members before its downstream population/science assertions run.
The successor checks unchanged old assumption/Core code, original rows/statuses,
actual option arguments and unchanged native tails explicitly. **These remain
failing source-provenance checks; no historical assertion is weakened.** Full
current/previous identities, outcomes, subtest contexts and causes are in the v2
validation record. No genuine regression remains after the bridge repair.

`capture-04a07-observations.ps1` compares **16 managed before/after report plans per
configuration**, invariant and `sk-SK`: ANOVA base/all options, KW API no-Dunn/GUI
Dunn, RM base/all, nested balanced/unbalanced. Typed cells, headers, footnotes,
p-value metadata, stages, comparison payloads and nested banner/separator metadata
are retained. These are additional observations, not extra MSTest identities.
The first exploratory capture encountered a PowerShell fractional-literal issue;
an exploratory dataset variant and the corrected v2 captures remain retained.
Post-fix captures use fresh `observations-<configuration>-post-fix.json` files.

Representative new tests include `EveryOneWayOptionCombinationUsesExistingDispatcherOnceAndStablePlots`,
`C30CandidatesRetainDistinctMetricsAndOriginalSyntheticRows`,
`EveryRepeatedFlagPreservesCoreCallAndTableOrder`,
`NestedBalancedMetadataAndIndependentSumOfSquares`,
`DuplicateOrMisalignedBlockEvidenceIsRejectedBeforeCore`,
`CompleteBlockPolicyReusedForMissingRowsAndDuplicateTreatmentLabels`,
`RealRepeatedBridgeMatchesFrozenSourceAllOptions`,
`ActualBlockProducerKeepsGapsAndFailureAtomicity`,
`ActualNestedProducerRetainsOriginalRowMapWhileCoreSortsSeparateScratch`, and
`NumericTextColumnKeepsRawEvidenceAndSurvivingFrameworkCoercion`.
All identities are available in the TRX-linked machine evidence.

Exact validation commands (local executable path is reviewable local metadata):

```powershell
$py = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
& $py -B scripts/verify-04a07.py init --evidence TestResults/04A-07-20261102-run01
& $py -B scripts/capture-04a07-baseline.py
& $py -B scripts/verify-04a07.py focused --evidence TestResults/04A-07-20261102-run01 --name focused-06
& $py -B -m unittest discover -s scripts/tests -p test_04a07_workflow_successor.py -v
& $py -B scripts/verify-04a07.py projection --evidence TestResults/04A-07-20261102-run01 --configuration Debug --name focused-projection-01
& $py -B scripts/verify-04a07.py final --evidence TestResults/04A-07-20261102-run01 --configuration Debug --name final-fixed-Debug
& $py -B scripts/verify-04a07.py final --evidence TestResults/04A-07-20261102-run01 --configuration Release --name final-fixed-Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a07-observations.ps1 -AssemblyDirectory TestResults/04A-07-20261102-run01/executions/focused-06-windows/sandbox/Legacy/bin/Debug/net48 -EvidenceDirectory TestResults/04A-07-20261102-run01 -Configuration Debug -Name post-fix
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a07-observations.ps1 -AssemblyDirectory TestResults/04A-07-20261102-run01/executions/final-fixed-Release-Legacy/sandbox/Legacy/bin/Release/net48 -EvidenceDirectory TestResults/04A-07-20261102-run01 -Configuration Release -Name post-fix
& $py -B scripts/verify-04a07-contract.py check
& $py -B scripts/summarize-04a07.py
& $py -B scripts/verify-04a07.py preserve --evidence TestResults/04A-07-20261102-run01 --name final-after-fix
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
& $py -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals
git diff --check
git diff --cached --exit-code
git status --short --untracked-files=all
git diff --stat
```

Initial full commands used names `final-Debug` / `final-Release` and are retained.
The wrapper runs isolated copies of the required `verify-beshstatng.ps1` with
`-Legacy -Wasm -Js -Python -VisualStudio 2026`, the stated Python executable and
configuration. Per-run `.provenance.json` records the exact transformation: no
restore/network; copied Common/legacy assemblies **and internal writer data**;
unique TRX paths; managed compilation without packaging/signing/version generators;
and continued independent suites after failing commands. The original verifier
is unchanged. Every failed development/focused attempt is retained.

Final evidence root: `TestResults/04A-07-20261102-run01/`. TRXs are under
`final-fixed-<configuration>/<configuration>/results/` (Core/Office),
`executions/final-fixed-<configuration>-Common/results/`,
`executions/final-fixed-<configuration>-Legacy/results/`, and
`projection-executions/final-fixed-<configuration>/results/`.
`final-fixed-<configuration>-full.log` and `-common-python.log` retain complete
Python/Node output; `final-summary-v2.json` lists counters, identities and hashes.

Read-only strict/profile checks were executed before the metadata-only repair:
**exit 1 / 1**, stale `UibyID` review dependency for `graphics.NormalPlot` and
`INTEGRITY_FAILURE`. That triggering source did not change during the repair.
The 24 historical audit failures remain recorded failures; no expensive sealed
audit writers were rerun or rebaselined. No R, Excel/COM/UIA, browser/Office UI,
installer or signing run was performed.

The initial inventory protected **164,310 historical files**; free disk was
**499.79 GiB**. The final `preservation-final-after-fix.json` receipt is **PASS**,
with no errors: complete initial membership, size, write/creation timestamps and
file/device identity plus original source/authority hashes. Final free disk was
**498.96 GiB** (535,753,027,584 bytes). This is not a content reread/hash of every historical file.
New output is confined to this run; no historical cleanup, relocation, compression,
deletion, retimestamping, rebaseline or overwrite is permitted.

## Future native smoke — NOT RUN

1. After exact-case pre-change/identity/risk/N authorization, exercise three
   unequal-n groups with Welch and selected/all post-hoc options, then KW with
   ties. Observe table/CI/p-value formatting and exact supplement order, original
   group labels, plot payload/series correspondence, cancel and invalid-target
   boundaries. Abort on workbook/lease/PID/HWND/source drift or unapproved output.
2. Under separately approved RM/nested native cases, exercise GG/HF, both Tukey
   variants and Mauchly, then balanced/unbalanced nested data. Verify existing
   destinations, banner/separator placement, overwrite behavior and box output.
   Do not attach to a user's existing Excel instance. N2 writing still needs its
   own explicit authorization and retained failure/capture receipts.

These are plans only. No live Excel/COM/UIA/ROT, native Office/browser smoke,
installer, signing or deployment was performed.

## Owner decision and publication preparation

Proposed owner decision: **APPROVE software-scope 04A.07 with the documented
historical/source failures and scientific limitations retained; all original native
obligations remain OPEN, P=NO, N1=NO GO, N2=NO and Gate C2=UNMET.** Alternatively,
REQUEST_CHANGES with the exact affected software behavior. No additional native
timing exception is requested for this behavior-preserving extraction.
Only the owner can accept 04A.07.
No approval of Git publication or 04A.08 follows from this dossier.

The seven modified tracked paths are:

* `BESHStatNG.Test/BESHStatNG.Tests.vbproj` (two explicit test/oracle includes);
* `BESHstatNG/BESHStatNG.vbproj` (one adapter include);
* `BESHstatNG/src/DM/WindowsPairedInputAdapter.vb` (immutable original/surviving evidence);
* `BESHstatNG/src/DM/WindowsUnivariateInputAdapter.vb` (original grouped evidence);
* `BESHstatNG/src/UI/UibyID.vb` (two grouped report members);
* `BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb` (RM report member);
* `BESHstatNG/src/UI/Ui9ANOVA2nested.vb` (nested report member).

The twelve new normally visible publication candidates are:

* `BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb`;
* `BESHStat.Excel.Common.Tests/src/AnovaWorkflow07Tests.vb`;
* `BESHstatNG/src/DM/WindowsAnovaWorkflowAdapter.vb`;
* `BESHStatNG.Test/src/AnovaWorkflow07AdapterTests.vb`;
* `BESHStatNG.Test/src/LegacyAnovaWorkflow07.vb`;
* this owner-review dossier;
* `scripts/capture-04a07-baseline.py`;
* `scripts/capture-04a07-observations.ps1`;
* `scripts/verify-04a07.py`;
* `scripts/verify-04a07-contract.py`;
* `scripts/summarize-04a07.py`;
* `scripts/tests/test_04a07_workflow_successor.py`.

Nine compact JSON files under `docs/architecture/04A07/` are ignored by existing
rules and need an explicit later publication decision: `baseline-v1.json`,
`reference-evidence-v1.json`, `source-restriction-crosswalk-v1.json` and `-v2.json`,
`validation-v1.json` and `-v2.json`, `report-fixtures-v1.json` and `-v2.json`, and
`closeout-v1.json`. Retained v1 source/validation/fixture files are superseded;
the v2 files are current. The closeout records exact path/byte hashes and
before/after member hashes, including evidence-producer changes. Nothing is staged.

Historical TestResults, owner-local reference bytes and failed attempts remain
local and in place; a fresh GitHub clone is not self-contained for those artifacts.
No raw test tree or external owner-reference directory is proposed for publication.
The new source-derived oracle and compact fixture/validation records are proposed
ZIP/publication content, not numerical goldens. The local Python executable path
above is an intentional validation-metadata exception for publication review.
No .gitignore or historical Master Plan/receipt change is included. Assembly/file
version **1.2.6.0**, BuildDate **2026-11-02**, framework targets, UDF declarations
and XLL packaging remain unchanged. Final diff checks pass and the index is empty.

No staging, commit, merge, push, native Excel/COM/UIA/ROT, scientific promotion or
04A.08 work occurred. This dossier requests exactly one owner acceptance review.
