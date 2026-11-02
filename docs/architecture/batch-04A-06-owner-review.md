# 04A.06 — assumptions and supplementary reports: owner review

**READY_FOR_OWNER_04A06_ACCEPTANCE_REVIEW** — the authorized behavior-preserving
extraction is implemented in shipping source and tested. Owner acceptance remains
pending. The retained failures below are not green gates; native Excel execution,
numerical promotion and Git publication remain unauthorized.

## Baseline and ownership

Branch `codex/04A-06` was created from clean local `main`, commit
`4a3d2e16b4d76a559427d596a424d382bcb0b915`. Local `origin/main` also named that
commit; no fetch was performed. The commit contains accepted 04A.27 over
04A.05 (`865a4e73…`), 04A.04 and 04A.03. The owner's current instruction supplies
the subsequent acceptance/merge authority; proposal-era status strings in
immutable older dossiers and Master Plan revision 2.4 were not rewritten.

The shipping importer remains byte-identical to its accepted physical-area
implementation, SHA-256
`32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9`.
No work on 04A.07 or another batch has begun. HEAD remains the starting commit.
The initial repository status was clean; the baseline-capture record includes
only this batch's newly created verifier as an untracked file. No unrelated owner
edit was carried into the branch. Final index is empty and all work is uncommitted.

Before production edits, `scripts/capture-04a06-baseline.py` captured the seven
current report/helper methods and generated a frozen, test-only characterization
fixture. [baseline-v1.json](04A06/baseline-v1.json) records source SHA-256
`123d181ba1e91b10509d994ac916d3ef0bb298a66f568e4188d4a201dc0671c0`, complete
method bodies, ownership and fixture hash. This is source-derived managed
characterization, **not a native capture, independent scientific reference or
promoted golden**. The fixture remains unchanged after capture.

| Existing member | Production extraction | Windows operations retained |
|---|---|---|
| `RunNormality` | Common fixed nine-row table, eligibility, Core calls, NA/p-value mapping and supplement plan | Checkbox snapshot; unchanged writer/overwrite/chart tail |
| `RunOutliers` | Grubbs/Rosner orchestration, alpha cells, fixed padding and labels | Radio/alpha snapshot; unchanged output and box renderer |
| `RunHomogeneityVar` | Selected methods and supplement ordering | View flags; unchanged output/chart tail |
| `RunDescStat` | Core descriptive calculation and selected-statistic report | Checkbox-to-key snapshot; unchanged output/chart tail |
| `RunSymmetry` | Both symmetry methods, headers, statistic/p-value table | Plot construction/rendering and native write tail |
| `ComputeVarianceHomogeneity` | Compatibility facade to Common | Captures the existing four flags; ANOVA/KW primary engines unchanged |
| `ComputeDescriptiveStats` | Compatibility facade to Common | Existing ANOVA/KW/ROC/t-test callers and their report ordering unchanged |

`verify-04a06-contract.py` proves that only these seven pre-existing methods
changed, one view-state helper was added, every other `UibyID` method is identical,
and each of the five native output/chart tails is identical. Designer, constructor,
event/RefEdit lifecycle, acquisition, dispatch, help and output destinations were
not changed. Core, UDF sources/exports, metadata, packaging, versions, targets and
the Master Plan were not modified.

## Shipping API and actual call chain

Common production source:
`BESHStat.Excel.Common/src/Results/AssumptionReportPolicy.vb`.

```vb
New AssumptionReportInput(groups As Double()(), names As String(),
    Optional sources As IEnumerable(Of UnivariateObservationSource) = Nothing)
New AssumptionReportOptions(kind As AssumptionReportKind,
    Optional outlierMethod As AssumptionOutlierMethod = None,
    Optional alpha As Decimal = 0.05D,
    Optional homogeneity As HomogeneityMethods = None,
    Optional symmetry As AssumptionSymmetryMethod = MiaoGelGastwirth,
    Optional descriptive As Boolean = False, Optional boxSummary As Boolean = False,
    Optional statistics As IEnumerable(Of String) = Nothing)
AssumptionReportPolicy.Build(input, options,
    Optional cancellation As CancellationToken = Nothing) As AssumptionReportResult
AssumptionReportPolicy.BoxSummary(names, q1, medians, q3, small, big)
    As AssumptionTableSpecification
```

Inputs/options defensively copy supplied arrays and selections. Results expose
immutable table specifications, ordered stages and `SpreadsheetValidationReport`.
`ToArtifact(index)` uses the existing `AnalysisTableArtifact` and
`ResultTableOutputModel`; `ToLegacyTable()` creates fresh Core table components
with original formatting hints. Neither carries native references. Numeric cells
remain numeric; no CI or localized numeric text is reparsed.

Actual routes are `UibyID.Run… → WindowsAssumptionReportAdapter.Prepare(data,
options) → AssumptionReportPolicy.Build → existing Core assumptions/DescriptiveStat
→ semantic specification → fresh ResultTable → unchanged Windows writer`.
The compatibility helper routes call adapter `Descriptive`/`Homogeneity`.
`Prepare` acquires no Excel object, constructs no controls and performs no writes.
Common stays `netstandard2.0`; Windows stays .NET Framework 4.8.
Current form entry lines: outliers 777, symmetry 806, homogeneity 841, normality
1241, descriptive 1267; compatibility helpers 1418/1422. Final form SHA-256:
`e5e725a6da7f1886a1122bf7859a115adf8612d5eceb50822f7bd939645036e3`.

Optional output stages are primary, optional descriptive, optional box summary
and chart, with one blank separator row in the unchanged writer. Existing Windows
`graphics.BoxPlot.Calculate/CalcForPlotting` still computes box statistics; Common
maps the existing six-column summary. Excel chart creation and plot geometry
remain Windows-owned. This is an explicit 04A.30 portability boundary, not a claim
to have migrated graphics mathematics.

## Report compatibility and diagnostics

The normality body retains exactly these nine rows, followed by each selected
variable in original order:

| Row | Label | One-based p-value body cell |
|---|---|---|
| 1 | Shapiro-Wilk test | |
| 2 | Test statistics | |
| 3 | Two-sided P-value | `(3, group + 2)` |
| 4 | D'Agostino-Pearson K2 test | |
| 5 | Test statistics | |
| 6 | Two-sided P-value | `(6, group + 2)` |
| 7 | Anderson Darling test | |
| 8 | Test statistics | |
| 9 | Two-sided P-value | `(9, group + 2)` |

The header is `Normality Tests` plus exact names. Shipping Shapiro eligibility
remains `n > 3 And n < 5000`; D'Agostino remains `n >= 9`; Anderson–Darling remains
`n > 1`. Noneligible three-cell blocks preserve `NA n<4 or n>5000`, `NA n<9`,
or `NA`, respectively, including the existing misleading Shapiro wording at
n=5000. At n=4999/5000/5001 the existing subsequent D'Agostino overflow aborts the
whole report; these NA eligibility cells are not claimed to have been rendered
or written. Empty method-heading cells remain empty strings. Zero and p=1 remain
numeric. All n=2,3,4,8,9,4999,5000,5001 cases have executable coverage.

Grubbs preserves Decimal alpha, **critical** statistic first, observed statistic
second, and the exact Core result string. Rosner preserves its 13-row body,
`List of Outliers:` text, zero display for `Nothing`, up to ten **outlier values**
(not asserted to be row indices), and unused blank padding. Additional diagnostic
codes distinguish `ROSNER_NO_RESULT`, `ROSNER_NOT_APPLICABLE` and
`ROSNER_SMALL_SAMPLE`; they do not silently reinterpret the legacy zero cell.

Homogeneity order remains Fligner–Killeen, Levene with `bW50=True`, squared ranks,
Bartlett. Every selected method has three rows, with the p-value cell in column 2.
All 16 option combinations are checked against the frozen helper. Descriptive
default order, all 17 selectable keys, `wrapSelf(True/False)`, titles, names and
Shapiro p-value hints are preserved. Both symmetry methods retain the existing
`Asymetry test` spelling, two top headers and `(2, group+1)` p-value markers.

`ASSUMPTION_SHAPE`, `ASSUMPTION_EMPTY_GROUP`, `ASSUMPTION_NONFINITE`,
`ASSUMPTION_SOURCE_MAP`, `ASSUMPTION_OPTIONS` and `ASSUMPTION_ALPHA` reject invalid
portable requests before numerical work. Cancellation returns
`ASSUMPTION_CANCELLED`. Core null/exception outcomes yield method/report diagnostics
and **no accepted partial table**. The legacy throwing facade deliberately retains
the existing form's outer error path before it can acquire an output writer.
The actual `getDataMultipleGroups` True-mode acquisition already rejects nonfinite
spreadsheet numbers through the accepted Common cell policy; the new portable
guard does not loosen or replace that path. Undefined numerical cells retain their values with
`ASSUMPTION_UNDEFINED_RESULT`; they are not converted to valid zeros or NA.

Invariant and `sk-SK` tests compare raw cell values/types and all writer metadata.
The host's existing p-value formatting configuration is untouched. Warnings remain
distinct from numerical results; per-group error state is fresh.

### Source rows and computational mutation

Core routines can sort passed vectors in place. Common uses private cloned scratch
arrays and never mutates the original input or its observation-source mapping.
`LegacyWorkingGroups` is explicitly **not provenance-bearing**. The Windows
compatibility adapter copies that scratch ordering back into the existing vectors
so native plots and supplementary callers observe the same sorting effects as
before. Tests compare these effects to the frozen source fixture.

Optional source mappings retain selected/physical coordinates, identities and
noncontiguous rows on the **original** observations. The existing Windows
`MultiGroupsUnpairedData` has no RowIds; this report adapter invents none. The
accepted 04A.03/04A.27 input/geometry flow remains unchanged, including its upstream
provenance sidecar. This batch does not claim that a legacy grouped DTO suddenly
carries physical row identity or that Rosner's returned values identify unique
source rows when values repeat.

### Scientific/compatibility candidates retained, not corrected

* **SC-06-SHAPIRO-BOUNDS:** Core/UDF permits n=3 and n=5000; GUI eligibility does
  not. No scientific or native policy correction is inferred from extraction.
* **SC-06-DAGOSTINO-OVERFLOW:** the unchanged Core denominator
  `(n-2)*(n+5)*(n+7)*(n+9)` uses Int32 intermediate arithmetic. Direct Core and
  frozen legacy routes throw `OverflowException` at 4999/5000/5001. Common records
  failure; Windows retains the exception/no-write boundary. This is separate
  arithmetic evidence, not a newly approved numerical fix or golden.
* **SC-06-CONSTANT-SHAPIRO:** zero-range Core returns `Nothing`; the old UI
  dereferences it. The new portable result is a structured failure; its Windows
  compatibility facade retains the same null-reference error boundary. No new
  user-visible warning/recovery behavior was applied.
* **SC-06-EMPTY-HOMOGENEITY:** all methods and supplements off yields no report
  tables; the unchanged native tail still requests a writer. Common represents
  this as `ASSUMPTION_NO_OUTPUT`. A native no-write guard would be a separate
  behavior-changing correction and requires exact-case owner disposition. It is
  not silently bundled into this behavior-preserving extraction.

The current numerical fixtures compare to authoritative existing Core results
and the frozen source-derived layout. No R/Python statistical oracle was run and
no new scientific references were promoted. Public assumption UDFs continue using
Core with their existing error returns; export signatures and mathematics remain
unchanged. Candidate alignment work requires a separately bounded future decision.

## Native restrictions and retained authority

[source-restriction-crosswalk-v1.json](04A06/source-restriction-crosswalk-v1.json)
contains **32 exact affected control/obligation IDs**, original CSV row hashes,
original source/designer hashes, risk tiers, overlapping ANOVA/normality case IDs,
the full mandatory pre-change gate and missing native prerequisites. All 160
shared obligation IDs are retained. The original CSV row hashes were recalculated
and matched against the immutable original ledger and accepted amendment.

This is a source-anchored behavior-preserving report extraction authorized by the
current instruction. No changed native policy, chart geometry, input selection,
default, result ordering or writer tail is activated. Managed/source parity does
**not** satisfy a native pre-change capture or release a control. Any subsequent
behavior-changing correction remains subject to the original exact-case gate.
Neither the 04A.05 timing exception nor the 44-ID importer timing exception is
generalized to normality.

The accepted amendment SHA-256 remains
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`;
the original ledger remains
`363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10`.
Shared **141 blocked controls / 160 obligations**, transferred **218 / 239**,
and wider **802 / 1,514** populations are overlapping sets, not additive counts.
All 44 importer timing obligations and these three 2×2 obligations remain OPEN:
`control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData`,
`control:BESHStatNG.Ui82x2.RefEditOutput`,
`control:BESHStatNG.Ui82x2.btnHelp` (help is not a third blocked control).
Released controls **0**, discharged obligations **0**.

**P NO; N1 NO GO; N2 NO; Gate C2 UNMET.** The original normality native case is
still NOT_RUN/BLOCKED. Later native evidence requires matching the independently
reviewed planned HWND and PID to the live owned instance, proving the *loaded*
XLL/managed assemblies correspond to reviewed source, genuine independent
Windows/COM risk review and exact owner N authority. The pending C14 2×2 review
does not authorize this normality case; its shared HWND/source-attribution defects
remain relevant to 04A.45–48.

## Executed validation and preservation

Final Debug and Release runs each executed the portable/legacy suites exactly
once after the last production/test-assembly change. No full run was repeated.
Both retained exit code **1**, because independent suites continue after the known
failures for complete accounting. No failure was converted into a successful exit.

| Suite | Debug passed / total | Release passed / total | Failed / skipped each |
|---|---:|---:|---:|
| Core | 1,055 / 1,055 | 1,055 / 1,055 | 0 / 0 |
| Common | 1,081 / 1,085 | 1,081 / 1,085 | 4 / 0 |
| Windows legacy | 986 / 987 | 986 / 987 | 1 / 0 |
| Office add-in managed | 505 / 505 | 505 / 505 | 0 / 0 |
| WASM execution managed | 45 / 45 | 45 / 45 | 0 / 0 |
| Successor C03 managed control projection | 61 / 61 | 61 / 61 | 0 / 0 |
| **Distinct managed total** | **3,733 / 3,738** | **3,733 / 3,738** | **5 / 0** |
| Node | **916 / 916** | **916 / 916** | **0 / 0** |
| Python methods | **621 / 657** | **621 / 657** | **36 / 0** |

Python has **37 failure events across 36 nonpassing methods** (26 failed methods,
10 errored methods): the frozen
`test_all_native_lifetime_cancel_write_and_reporting_methods_unchanged` has two
failing subtests (`RunNormality`, `RunDescStat`). The method is counted once in
the distinct total. No failures are being hidden by that distinction.

**106 new managed cases** pass in each configuration: 63 Common and 43 Windows
adapter cases. All eight new Python successor tests pass. The final focused run
passed 117 Common and 97 Windows cases (214, including earlier regression cases),
plus the separate 61-case projection. These and the repeated 15-case C02 semantic
subset are **repetitions**, not additional distinct tests. Early focused attempts
remain retained: they exposed the pre-existing large-n Core overflow, then routine
new-test compile/reflection scaffolding issues; final focused runs passed.

The C03 projection is explicitly a **test-only source projection** with initialized
managed controls, never shown. It omits original Excel connectors/preferences/focus
and does not execute the full shipping constructor. Its successor copies the
frozen parent and substitutes only the actual homogeneity delegation plus its new
view-state helper. Parent sources/receipts stay untouched; the actual adapter is
separately tested. Passing these tests does not establish native GUI/COM/STA safety.

Representative new executable tests are
`AssumptionReport06Tests.ShippingEligibilityAndNineRows`,
`EveryDescriptiveSelectionUsesCore`, `HomogeneityCoreMethodsAndOrdering`,
`OutlierValuesAndFixedPadding`, `SourceCoordinatesStayWithOriginalValuesAndFailureDoesNotWrite`,
`CoreNullFailureCancellationAndNoOutputAreDistinct` and
`NoncontiguousSourcesStayOriginalAfterScratchSortAndNamesNeverReorder`;
Windows tests include `ActualAdapterMatchesPreExtractionNormality`,
`EveryHomogeneityCombinationMatchesFrozenHelper`,
`FullAndSelectedDescriptiveSnapshotsForAllExistingCallers`,
`RealBoxCalculationAndSupplementOrderAreUnchanged`,
`LegacyCoreFailureAndCancellationAbortBeforeCallerCanWrite` and
`ReversedColumnsAndSelectedInputProducerReachActualReportAdapter`.
All new case identities and outcomes are also in the TRX-linked validation record.

`capture-04a06-observations.ps1` additionally compared **22 before/after table
observations per configuration**, across invariant and Slovak cultures. The
observations include empty descriptive selection (title/header only), Rosner
zero/one/two returned outliers and a ten-extreme candidate capacity fixture.
The latter's legacy no-array/zero display is recorded as existing Core behavior,
not an independently justified scientific detection result. The original
20-observation exploratory file is retained; v2 corrects the fixture naming and
adds the positive two-outlier scenario. These observations are not added to MSTest
counts. [report-fixtures-v1.json](04A06/report-fixtures-v1.json) contains typed
canonical cells and metadata; local observations retain both before and after.
Two earlier capture-script attempts failed before creating output because
PowerShell reflection wrapped the data/list arguments in `PSObject`; the harness
now explicitly passes their base objects. This was a test-capture repair, with no
production source change. No old observation file was overwritten.

The five managed nonpasses are exactly the retained C05 superseded expectations:

* `ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection`
  — rows `(20,30,50)`, `(20,15,40)`, `(0,0,0)`;
* `ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted`;
* `ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden`.

The preceding 30 historical Python nonpass identities remain listed in
[validation-v1.json](04A06/validation-v1.json). **27 recur**; three checks now pass
because they examine the current Git-diff scope after owner publication of 04A.27:
`test_04a04_alignment.…test_udf_exports_and_packaging_unchanged`,
`test_04a05_completion_successor.…test_no_designer_importer_targets_exports_or_signing_change`,
and `test_04a05_contingency.…test_no_designer_udf_numerical_packaging_or_framework_change`.
Their historical failures have not been erased or claimed repaired. No old test
or expected value was modified.

**Nine newly triggered frozen-source method conflicts** remain nonpasses. Their
full identities/outcomes and specific source causes are in the machine-readable
validation record. In brief: three C03 control-mitigation whole-source/consumer
checks, two C03 original extraction-boundary checks, three C27 source/project pins,
and the older normality n=5000 source-text search. The successor proves the bounds
in the actual Common eligibility API, unchanged native tails, all original rows
and original acquisition/UDF/RefEdit code. These are documented source-provenance
conflicts, not a blanket waiver of old tests. **No newly introduced functional or
numerical regression was identified.** All old RefEdit/receipt/source expectations
remain intact and nonpasses are still reported as failures/errors.

Historical strict/profile checkers were also executed read-only: **exit 1 / 1**.
The strict checker reports the stale `UibyID` review dependency for
`graphics.NormalPlot`; profile reports `INTEGRITY_FAILURE`. The previously recorded
24 audit failures were not regenerated, rewritten or declared green. R/native
Excel/COM/UIA, loaded-binary attribution, live browser/WASM/Office UI, installers
and signing were **NOT RUN**.

Exact commands (the local Python path needs review before public publication):

```powershell
$py = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
& $py -B scripts/verify-04a06.py init --evidence TestResults/04A-06-20261102-run01
& $py -B scripts/capture-04a06-baseline.py
& $py -B scripts/verify-04a06.py focused --evidence TestResults/04A-06-20261102-run01 --name focused-06
& $py -B -m unittest discover -s scripts/tests -p test_04a06_assumptions_successor.py -v
& $py -B scripts/verify-04a06.py projection --evidence TestResults/04A-06-20261102-run01 --name focused-projection-01
& $py -B scripts/verify-04a06.py final --evidence TestResults/04A-06-20261102-run01 --configuration Debug --name final-Debug
& $py -B scripts/verify-04a06.py final --evidence TestResults/04A-06-20261102-run01 --configuration Release --name final-Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a06-observations.ps1 -AssemblyDirectory TestResults/04A-06-20261102-run01/executions/focused-06-windows/sandbox/Legacy/bin/Debug/net48 -EvidenceDirectory TestResults/04A-06-20261102-run01 -Configuration Debug
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/capture-04a06-observations.ps1 -AssemblyDirectory TestResults/04A-06-20261102-run01/executions/final-Release-Legacy/sandbox/Legacy/bin/Release/net48 -EvidenceDirectory TestResults/04A-06-20261102-run01 -Configuration Release
& $py -B scripts/verify-04a06-contract.py check
& $py -B scripts/summarize-04a06.py
& $py -B scripts/verify-04a06.py preserve --evidence TestResults/04A-06-20261102-run01 --name final
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
& $py -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals
git diff --check
git diff --cached --exit-code
git status --short --untracked-files=all
```

The final wrapper executes isolated copies of the required
`powershell -NoProfile -ExecutionPolicy Bypass -File scripts/verify-beshstatng.ps1`
with `-Legacy -Wasm -Js -Python -VisualStudio 2026`, the stated Python executable
and configuration. Per-run `.provenance.json` records the exact transformation:
no restore/network, isolated Common/legacy assemblies **and internal writer data**,
fresh TRX paths, managed compilation without packaging/signing/version generators,
and independent continuation after failing commands. The original verifier is
unchanged. Development focused-01 through focused-06 command/build logs and all
failed attempts are retained separately.

Evidence root: `TestResults/04A-06-20261102-run01/`. Each configuration's TRX files
are under `final-<configuration>/<configuration>/results/` (Core/Office),
`executions/final-<configuration>-Common/results/`,
`executions/final-<configuration>-Legacy/results/`, and
`projection-executions/final-<configuration>/results/`.
`final-<configuration>-full.log` and `-common-python.log` retain Python/Node output;
`final-summary.json` lists all identities, counters and TRX hashes.

Retention R **PASS**: all **161,648** original historical TestResults paths retain
membership, size, write/creation timestamps and file/device identity; no additions
outside this new run were found. Initial protected tracked-source/authority hash
pins also match. `preservation-final.json` reports zero discrepancies. This proves
the specified metadata/source-pin scope, **not** a reread/content hash of every
historical file. Initial free disk **500.32 GiB**, final **499.89 GiB**. No cleanup,
relocation, recompression, rebaseline, timestamp rewrite or historical writer run.

## Later native smoke — NOT_RUN

1. **Normality plus descriptive/box:** after exact-case N authorization and the
   identity/risk prerequisites above, use an owner-owned disposable workbook with
   two named nonconstant columns. Check the nine-row table, p-value formatting,
   optional descriptive then box summary, separator rows, chart series/geometry,
   and original selection order. Also observe n=3 and n=5000 NA/error behavior
   without approving a scientific change. Abort on identity drift, unexpected
   selection, warning/fault or attempted unapproved output; N2 writing remains
   separately unauthorized until specifically released.
2. **Homogeneity/Grubbs:** in a separately approved native case, check all four
   homogeneity methods in the recorded order and median Levene, then Grubbs with
   the chosen alpha and optional descriptive/box output. Observe cancellation,
   invalid-input no-write and teardown. Do not attach to a user's existing Excel
   instance. Retain original evidence and partial/failure receipts.

These are future plans, not executed GUI, COM, STA, Office or browser verification.

## Owner disposition and publication preparation

Suggested owner decision: **APPROVE_SCOPED_WITH_EXACT_OPEN_ITEMS** (alternatives:
`APPROVE` or `REQUEST_CHANGES`). Proposed sentence:

> I approve 04A.06's behavior-preserving Common report extraction and actual
> Windows adapter wiring on the documented source/evidence hashes, with the
> disclosed five managed expectations, 36 Python nonpasses and scientific
> candidates retained. All native obligations remain OPEN; P NO, N1 NO GO,
> N2 NO and Gate C2 UNMET. This does not authorize Git publication or 04A.07.

There is no unapplied required wiring patch or requested normality timing
exception: the authorized extraction is active. The separately described
behavior-changing remediations have not been applied or claimed accepted.
Only the owner can approve the batch. The software acceptance recommendation is
bounded by managed/source evidence; native output capture remains mandatory before
any separately gated native behavior correction or control release.

Proposed publication scope: **15 normal modified/new paths**, plus **five ignored
JSON evidence paths** requiring a later explicit staging decision:

| Path | Purpose |
|---|---|
| `BESHStat.Excel.Common/src/Results/AssumptionReportPolicy.vb` | Typed options/input, immutable report specification, Core orchestration/diagnostics |
| `BESHstatNG/src/DM/WindowsAssumptionReportAdapter.vb` | Real host-free bridge, sorting compatibility, box summary conversion |
| `BESHstatNG/src/UI/UibyID.vb` | Five report delegates, two compatibility facades, view-state flag capture |
| `BESHstatNG/BESHStatNG.vbproj` | One explicit adapter Compile include |
| `BESHStatNG.Test/BESHStatNG.Tests.vbproj` | Two explicit test/fixture Compile includes |
| `BESHStat.Excel.Common.Tests/src/AssumptionReport06Tests.vb` | 63 new portable cases |
| `BESHStatNG.Test/src/AssumptionReport06AdapterTests.vb` | 43 real-adapter/source-fixture comparisons |
| `BESHStatNG.Test/src/LegacyAssumption06Characterization.vb` | Frozen pre-extraction test-only source projection |
| `scripts/capture-04a06-baseline.py` | One-time pre-edit characterization capture, fails if source already changed |
| `scripts/capture-04a06-observations.ps1` | No-clobber host-free before/after typed table observations |
| `scripts/verify-04a06.py` | Isolated focused/full validation and successor control projection |
| `scripts/verify-04a06-contract.py` | Exact-ID, source/row/authority and native-tail proof |
| `scripts/summarize-04a06.py` | Distinct test/failure accounting and candidate table serialization |
| `scripts/tests/test_04a06_assumptions_successor.py` | Eight additive successor source/restriction checks |
| `docs/architecture/batch-04A-06-owner-review.md` | This owner dossier |
| `docs/architecture/04A06/baseline-v1.json` **ignored** | Before-change methods, ownership and source-derived fixture pins |
| `docs/architecture/04A06/source-restriction-crosswalk-v1.json` **ignored** | Exact 32 touched obligations and all 160 shared IDs; original lineage |
| `docs/architecture/04A06/validation-v1.json` **ignored** | Per-suite results, every old/new nonpass, TRX and observation hashes |
| `docs/architecture/04A06/report-fixtures-v1.json` **ignored** | Typed semantic table cells and metadata, NOT_GOLDEN |
| `docs/architecture/04A06/closeout-v1.json` **ignored** | Final hashes, preservation receipt and unchanged-method hash inventory |

No deletions, designer/Core/importer/UDF/packing/version/framework/Help/Master Plan
edits. Both `git diff --check` and the empty-index check succeeded. Existing CRLF
source bytes were retained; Git warns of its configured future CRLF-to-LF
normalization, which has not been performed here. No Git publication operation ran.

All source/tests/scripts and this dossier are proposed publication candidates;
ignored `docs/architecture/04A06/*.json` evidence needs an explicit later staging
decision. `TestResults/04A-06-20261102-run01/` and all historical/local-only records
remain local preservation evidence; a GitHub clone is not self-contained for
them. The pre-extraction fixture, seven original method bodies, current source,
typed snapshots, exact-ID crosswalk and validation summary are included in this
proposed ZIP/publication scope. Source checks additionally require the named Git
baseline and local preservation/projection evidence; missing local artifacts are
a reproducibility limitation, not a native-validation claim.
The mandated executable path above is local validation metadata and should be
reviewed before public publication. No SIDs, credentials or real workbook contents
are intentionally included in new publication candidates. No staging, commit, merge, push,
remote change, native Excel execution, P promotion, N release or C2 completion.
