# Official 04A.27 geometry completion — final owner review

Continue `codex/04A-27`, HEAD and local main
`865a4e73b4668fbcbb7a2a24cceda02cc64bc22c`. No branch replacement, reset,
stash, rebase, merge or remote operation. The prior partially qualified work was
uncommitted:5 tracked modifications,9 normal untracked files,2 ignored review
artifacts. It is retained. Initial index empty; no unrelated edit discovered.
The preserved Master Plan revision2.4 status text is historical; the current
owner authorizations govern this bounded completion. The plan is not edited.

This successor does not rewrite `batch-04A-27-owner-review.md`, its proposed
patch, original crosswalk, tests or previous evidence. Fresh local evidence:
`TestResults/04A-27-20261102-completion-01/`, created with no-clobber checks.

## Binding decision and exact application

`04A27/geometry-owner-disposition-2026-11-02.json` quotes the current owner's
decision verbatim, including:

> Exact patch SHA-256: `96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1`

> Exact native-restriction crosswalk: `docs/architecture/04A27/transport-restrictions-v1.json`, containing **44 distinct original RefEdit obligations**. They **all remain OPEN**.

> This is a **narrow timing exception for missing native pre-change capture** on this importer switch. It is NOT a finding that native Excel behavior is validated, not a release or discharge of an obligation, and not a general exception for other source changes. The prior 04A.05 2×2 timing exception does not broaden this one.

This is an owner-instruction record, not a cryptographic signature, independent
Windows/COM review or final batch acceptance. All exact44 IDs/source/designer/row
pins are copied from the frozen crosswalk and independently matched to the
original1514-row ledger and canonical CSV-row SHA convention. All remain OPEN,
owner_released=false, native_executed=false. The44 are a subset/dependency set,
not44 newly released controls or an extra population to add to other registers.

| Pin | Verified SHA-256 |
|---|---|
| Importer before application |5aa0578f771675c072e40206f165d458a693f8087a97b405abf6da19531fa9ae|
| Reviewed original patch, unchanged |96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1|
| Exact applied importer, before follow-up edits |36b70018df3dab98569e2c4ba4580c240fca4bdb52fe5e24db6bf51b32efb326|
| Final shipping importer |32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9|
| Original C27 crosswalk, unchanged |b86197c2d466da3caa94c530d665b04be785e5d85986b2be479a38527d3d4a86|
| Original deferred ledger, unchanged |363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10|
| Accepted C02 amendment, unchanged |8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307|

`git apply --check` passed, then normal `git apply` actually applied the pinned
patch. `git apply --reverse --check` passed on that exact applied state. Its full
bytes and application receipt are preserved as importer-exact-reviewed-patch.vb
and reviewed-application.json in the new run. The original patch still contains
its historical PROPOSED ONLY comment and is byte-identical.

## Shipping call chain and narrowly documented follow-up edits

The shipping chain is now:

```text
ExcelDnaDataImporter.Import(ref, bStartRow:=True)
  → ImportPhysicalAreas(ws, original, addresses)
  → ImportPhysicalSelection(original, detached addresses, operation-local acquire)
      acquire → each ws.Range(original area address)
              → ValidatePhysicalMergeState(area.MergeCells)
              → AcquireRangeSetData(areas)
              → existing bounded/identity-checked Windows Value2 snapshots
      → SpreadsheetSelectionTransport.JoinColumns(PhysicalIntersection)
      → first retained common row supplies labels; explicit existing cell policy
      → SpreadsheetTransportFrame.ToCoreDataTable
  → ImportInto's unchanged LoadCoreDataTable(CharCols, SkipRow, cloneTable:=False)
  → existing paired/blocked/univariate/contingency policy → existing Core engine
```

Source-boundary tests pin the actual public Import dispatch **before** every
legacy first-area geometry operation, and pin the actual private resolver calls.
Executable tests call the same production ImportPhysicalSelection assembly and
conversion logic with deterministic snapshots from the existing Windows
SnapshotSelection/SnapshotValues adapters. They do not merely test an independent
Common join. The public COM resolver itself is not invoked without Excel; its
native behavior remains unobserved, explicitly distinct from compiled source and
host-free execution evidence.

Additional edits beyond the exact patch are explicit and bounded:

1. Replace misleading importer comments, including PROPOSED ONLY, with the
   current timing authorization; the original reviewed patch is preserved.
2. Extract the actual join/label/cell-policy conversion into Friend
   ImportPhysicalSelection, accepting an operation-local acquisition delegate
   and a defensive read-only address copy. No global injected Excel object,
   mutable resolver switch, feature flag or alternative parsing algorithm.
3. Extract the exact original Boolean merge-state predicate into
   ValidatePhysicalMergeState so false/true/Nothing/DBNull/unsupported states can
   be executed without a COM range. Predicate and rejection semantics unchanged.
4. Attach the typed frame/validation report to the returned CoreDataTable using
   a Windows-owned ConditionalWeakTable. GetPhysicalImportEvidence retrieves
   original areas, values, coordinates, workbook/sheet/code-name context, date
   metadata and exclusion warnings. Values contain no COM objects. ImportInto
   already transfers that same Core table without cloning; no Core schema change.
   Arbitrary Core clones retain their numeric/row data but do not acquire a new
   Windows sidecar; callers needing it must retain the original evidence.
5. A failing all-blank-header test exposed a safety defect in the reviewed
   fallback: ColNumber2Letter reads AppGlobals.app.ActiveSheet and returned only
   "Var" in the host-free run. The new True path now uses Common's additive
   SpreadsheetRangeAddress.ColumnLetters(Integer) wrapper around its **existing**
   private ToColumn formatter. It validates1..16384 and preserves intended
   VarB/VarE/VarAA labels without querying active Excel. No duplicate A1 grammar,
   statistical API change or other caller migration. Every original Common
   range-reference source line remains identical after removing this one wrapper.
   The legacy False-path ColNumber2Letter call is untouched.

The reviewed join/header/missing/culture projection remains exactly equal after
normalizing only the named fallback-call substitution. The successor records
both member hashes and that sole substitution. Common formatter source before:
700ff7f83f9d2c1fa3ab43e4a80f11c976666509f2894eca4569e2e596b445ce;
after:86e59a8d1ec073c681a81d39c87e663c2b55f6f4c6fdcfde9be826425207ec81.
This is an additive pure API, no removal/change to an existing signature.

## Product-boundary behavior matrix

| Case | Before / corrected result | Decision and provenance |
|---|---|---|
| B3:B5 and E8:E10 | Previously later area could be read as E3:E5. Now each original area is resolved independently; no common physical rows | Intentional fix; transport.no-common-rows, no returned table/statistics/write. Host-free read log is exactly B3:B5,E8:E10 |
| B3:B6 and E4:E7; B5 missing | Previously first area's geometry could shift/extend the second. Now raw joined rows4,5,6; downstream complete pairs retain4,6, values{{2,10},{4,40}} | Intentional fix; original source areas/row maps and rows-excluded warnings retained; no ordinal zip/renumber |
| Reverse selected areas | Columns stay E then B, final pairs{{10,2},{40,4}} at rows4,6 | Selected column/order and existing first-minus-second interpretation preserved |
| Same start/different lengths; shifted same length; rectangle+singleton | Only physical intersection retained. B3:C4 plus G4 gives row4, columns B,C,G and values3,4,9 | Explicit intersection, no fabricated row/column rectangle; typed sidecar carries real per-column origins |
| Headers | Label from first retained common row, including numeric-looking text and current-culture numeric labels | Reviewed semantics; original leading label outside intersection is not silently carried into a different physical row |
| Nothing/DBNull/ExcelEmpty/ExcelMissing/ExcelError | Explicit existing snapshot normalization and error-to-missing policy, whole-row downstream removal | Raw typed cells/native errors remain in evidence, no independent per-column compaction. All-blank input yields existing bZeroValid/too-few-pairs rejection before calculation |
| Nonfinite/unsupported cells | Unsupported host object rejects at snapshot; NaN/Inf rejects at typed Core bridge | No accepted partial table. transport.cell retains row/column where representable |
| Date-looking format / numeric string | Value2 serial61 stays61, format is a hint; explicit invariant/current sk-SK text conversion | No inferred date/calendar conversion or changed numerical formula |
| Book/sheet/code-name/revision mismatch; repeated/overlapping cells | Identity or duplicate-cell rejection | No cross-sheet/book match by ordinal row/name alone; existing stale-snapshot verification remains fail-closed |
| Merged/unverifiable merge state; disconnected reader | Existing reviewed predicate rejects true/non-Boolean state; acquisition faults propagate | No accepted table. Native getter behavior not observed; fake provider only |
| Whole-column/oversized selection | Existing aggregate/per-area/unchunked Windows preflight rejects before Value2 payload callback | No native entire-sheet read or new chunking. Full-column metadata rejection is intentional under the reviewed bounded True path |
| CharCols / SkipRow | ImportInto body unchanged; inclusive character-column boundary and explicit row skip passed unchanged to DataObj | By-ID fixture groups B then A with values{1,3}/{2}; SkipRow1 retains physical rows5,6. Complete-block fixture stays in treatment order |
| bStartRow=False | Entire original Import body remains equal after removing only the True dispatch; ImportInto/NormalizeExcelValue/ResolveStartRow/BuildConcreteRangeAddress unchanged | Observed legacy semantics retained, including row1 origin and singleton-address behavior. No new claim those legacy ordinal IDs are selected physical rows |
| UDF versus GUI | Equivalent finite cells give the same numeric shape/values; known GUI rows3,4 versus opaque UDF array coordinates0,0 | UDF acquisition/export implementation unchanged by completion; no invented array book/sheet coordinates |

Representative True consumers include UiTwoInputRefedits paired numeric and
Hotelling imports; Ui0OneRefeditMulticol paired/blocked/RxC; UibyID; Ui82x2;
Ui9ANOVA2nested; MMRM/LMM and bounded plot imports. Direct Import in Sankey uses
True. Representative False/default consumers remain PCA main/CA, GEE, GLM, Cox
and DM/PSMdataImport; PCA centers explicitly uses True. No wholesale False-mode,
form, categorical, margin, output, event or UDF migration was performed here.
New tests cover the actual conversion through DataObj and existing paired,
blocked and by-ID Windows policies, not live controls or Excel resolution.

Warnings are retained, not newly displayed via a redesigned dialog. Existing
Windows exception/error presentation and validation-before-writer routing remain.
Precise per-column metadata remains in the Windows sidecar because the legacy
Core table has one column-origin field; no misleading contiguous origin is added.

## Focused development and independent checks

New executable coverage:25 PhysicalImporter27Tests Windows cases,8 pure Common
column-formatter cases,10 successor Python tests. Final focused run:213 Common
and141 Windows PASS, plus10 Python PASS. These include the earlier accepted
04A.03–05 policies, C05 paired/2×2 candidate tests and C27 acquisition/RefEdit
seams. Core calculations, candidate fixtures and numerical tolerances are
unchanged; no new R execution, numerical reference or P promotion.

Failed attempts retained in this run: focused-01 compiled Common205 PASS but a
new test used Count(predicate) on an IReadOnlyList property; changed to
Where(...).Count(). focused-02 ran Common205 PASS, Windows140/141, exposing the
actual active-worksheet header fallback above. focused-03, after the pure
formatter correction, passed213/141. No source test was weakened and no original
test/fixture/receipt was rewritten. New test expectations pin VarB/VarE, source
row values and Core consumer results independently of implementation branches.

The ten source checks independently verify original44 rows, exact patch/applied
copy, shipping dispatch, False-mode member equality, reviewed-projection delta,
weak sidecar scope, inherited source/test preservation,268 unchanged UDF exports,
native helper/RefEdit/target preservation and an empty index.

## Final regression and nonpass reconciliation

Final full verification ran **once per configuration after the last production
correction**, sequentially, in new destinations. No final full rerun was needed.

| Distinct suite | Debug pass/total | Release pass/total | Remaining fail/skip per configuration |
|---|---:|---:|---|
| Core |1055/1055|1055/1055|0/0|
| Common |1018/1022|1018/1022|4 superseded expectations /0|
| Windows legacy |943/944|943/944|1 superseded expectation /0|
| Office managed |505/505|505/505|0/0|
| WASM execution managed |45/45|45/45|0/0|
| Retained C03 test-only mitigation projection |61/61|61/61|0/0|
| **Managed** |**3627/3632**|**3627/3632**|**5 known /0**|
| **Node** |**916/916**|**916/916**|**0/0**|
| **Python** |**619/649**|**619/649**|**30 (21 FAIL,9 ERROR) /0**|

All33 new managed cases and10 new source cases PASS. The previously qualified
C27 and C03–05 cases are included, not counted again as newly authored tests.
The extra15 semantic capture cases per configuration are repetitions and excluded
from distinct totals. Focused runs are also repetitions, not additions to the
final unique counts. Full commands truthfully return1 due to retained nonpasses;
summary exit0 means exact failure-set reconciliation, not an all-green suite.
No unexplained new functional or numerical failure remains.

The five managed failures are unchanged:

- ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection
  (3 DataRows), expecting paired analysis to remain unavailable.
- ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted,
  expecting the C05-authorized overflow defect to remain.
- ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden,
  expecting wrong legacy p≈6.4931e-8 rather than the corrected≈.49956.

All27 preceding Python nonpass identities and FAIL/ERROR outcomes are preserved,
compared automatically against the previous C27 final-summary.json. They include
the C02 receipt/RefEdit/source issues and disclosed C03–05/source/plan/publication
byte conflicts; no historical script, receipt, candidate or expectation changed.
Three newly triggered frozen-source failures are explicit successors, not hidden
or relabeled as newly passing tests:

| Test | Actual reason / successor disposition |
|---|---|
| test_04a04_alignment.PairedBlockSourceGates.test_legacy_geometry_ambiguity_is_not_hidden | Frozen equality to the original whole importer; owner now authorizes fixing that ambiguity. New False-mode member proof and product-route cases replace that *current* invariant without editing the old test |
| test_04a05_contingency.ContingencySourceGates.test_historical_ledgers_and_importer_unchanged | Old C05 source pin requires unchanged importer. Original ledgers stay unchanged; only the authorized importer/label safety correction differs |
| test_04a27_transport_successor.Transport27SuccessorTests.test_exact_successor_and_unapplied_patch | Old partial record asserts the switch is unapplied. It remains intact as history; dated44-ID successor binds the applied state, patch bytes and final source |

Detailed identities, test names, counters and TRX hashes:
`TestResults/04A-27-20261102-completion-01/final-summary.json`.
The classification does not erase any old failure or assert native safety.

Read-only original strict/profile checks were executed: Roslyn `--check --strict`
exit1 for the existing stale UibyID→NormalPlot dependency; approved inventory
profile exit1 for integrity failure. Their previous24-failure audit census stays
historical, not a new count or a green gate. No sealed audit writers/renderers,
R statistical references, package restore or numerical rebaseline ran.
Existing legacy C10 managed entrypoint/handler tests still execute initialized
RefEdit controls on managed STA threads with injected detach delegates and no
Excel connectors. These are retained host-free baseline tests, not native
connection-point/Excel evidence. New completion tests use no WinForms controls.

Exact principal commands (repository root):

```powershell
$PythonExe = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
$Run = 'TestResults/04A-27-20261102-completion-01'
& $PythonExe -B scripts/verify-04a27-completion.py init --evidence $Run
# Before applying: exact hashes verified, previous C27 contract checked.
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply --check docs/architecture/04A27/import-physical-geometry-PROPOSED.patch
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply docs/architecture/04A27/import-physical-geometry-PROPOSED.patch
# This reverse-check was on the exact applied copy, before the recorded follow-up edits.
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply --reverse --check docs/architecture/04A27/import-physical-geometry-PROPOSED.patch
& $PythonExe -B scripts/verify-04a27-completion.py focused --evidence $Run --name focused-01
& $PythonExe -B scripts/verify-04a27-completion.py focused --evidence $Run --name focused-02
& $PythonExe -B scripts/verify-04a27-completion.py focused --evidence $Run --name focused-03
& $PythonExe -B -m unittest discover -s scripts/tests -p test_04a27_geometry_completion.py -v
& $PythonExe -B scripts/verify-04a27-completion.py final --evidence $Run --configuration Debug --name final-Debug
& $PythonExe -B scripts/verify-04a27-completion.py final --evidence $Run --configuration Release --name final-Release
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
& $PythonExe -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals
& $PythonExe -B scripts/verify-04a27-geometry-contract.py check
& $PythonExe -B scripts/verify-04a27-completion.py summary --evidence $Run
& $PythonExe -B scripts/verify-04a27-completion.py preserve --evidence $Run --name final
```

Init/create/application are historical commands here, not instructions to clobber
this run or reapply the patch. The new wrapper reuses the unchanged C05/C02
assembly-copy isolation and transforms a **local execution copy** of
scripts/verify-beshstatng.ps1: `-Legacy -Wasm -Js -Python -VisualStudio 2026
-Configuration Debug/Release -PythonExe` with the exact WinPython path above.
Every argument, duration, exit and execution-copy diff is retained in command
JSON/provenance logs. Windows host Compile/CopyFiles targets avoid generators,
XLL packing/signing/installers. Builds do not compete over bin/obj. Optional
internal observation writers are isolated/disabled by the accepted strategy;
TRX relocation alone is not the protection mechanism.

Final TRX paths relative to the completion run:

- final-{Debug,Release}/{Debug,Release}/results/{Core,OfficeAddin,OfficeExecution}.trx.
- executions/final-{Debug,Release}-Common/results/Common.trx.
- executions/final-{Debug,Release}-Legacy/results/Legacy.trx.
- projection-executions/final-{Debug,Release}/results/ControlMitigation.trx.
- executions/final-{Debug,Release}-semantic/results/Common.trx (15 repetitions).

## Protection, Git and publication scope

Retention R **PASS** for **159,700 actual historical files**, including the prior
C27 run and all older failed attempts. Complete initial path membership, size,
creation/write nanoseconds and file identity remain equal; no unexpected file
outside the fresh run and no protected source/authority hash drift. This is a
metadata/path and selected-source-hash proof, not a full reread/hash of every
historical byte. Receipt: preservation-final.json in the completion run.
Initial free537,645,174,784 bytes; final537,331,470,336 bytes (about500GiB).
No cleanup, deletion, archive/compression, move, retimestamp, symlink substitution,
old output reuse or rebaseline. The exact reviewed/applied copies and all failed
new attempts also remain retained. Initial inventory/preimages and new evidence
are local-only, not assumed available in a GitHub clone.

Completion-specific normal source/review paths:

- BESHstatNG/src/DM/ExcelDnaDataImporter.vb — actual active geometry path,
  operation-local product seam, merge predicate and weak evidence sidecar.
- BESHStat.Excel.Common/src/Contracts/SpreadsheetRangeReferences.vb —6-line
  pure wrapper around existing column formatter; other original code unchanged.
- BESHStatNG.Test/BESHStatNG.Tests.vbproj —one additive test Compile entry,
  retaining the inherited uncommitted entries.
- BESHStatNG.Test/src/PhysicalImporter27Tests.vb —25 host-free production cases.
- BESHStat.Excel.Common.Tests/src/PhysicalImporter27ColumnNameTests.vb —8 cases.
- scripts/verify-04a27-completion.py —fresh-run execution/preservation/accounting.
- scripts/verify-04a27-geometry-contract.py —dated44-ID/current-source proof.
- scripts/tests/test_04a27_geometry_completion.py —10 successor checks.
- docs/architecture/batch-04A-27-final-owner-review.md —this single final dossier.

One new **ignored** review artifact requires an explicit later publication choice:
docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json. The original
ignored transport-restrictions-v1.json and PROPOSED.patch remain unchanged.
All existing prior partial files are retained; the full original+completion diff
has22 normal paths (7 tracked modifications,15 normal new files), plus these3
ignored review artifacts. Completion changes only the three existing source/
project paths listed above and adds the six normal files plus dated JSON. No
deletion or unrelated change. No source/test VB is ignored. The full path/hash
publication manifest is retained locally, with historical data/evidence excluded.

Core algorithms, UDF declarations/signatures/defaults (268 exports), writer
contracts, form/designer/RefEdit/lease source, the external Master Plan, versions,
packing, .gitignore, framework targets and package definitions are unchanged from
the initial completion baseline. .NET Framework4.8 and netstandard2.0 boundaries
remain. The existing CRLF Git advisory and inherited RefEdit-test/discovery
warnings are disclosed; no new compile error remains. Final diff-check and
empty-index checks pass. No stage/commit/merge/push/fetch occurred; ending HEAD
and main remain865a4e73b4668fbcbb7a2a24cceda02cc64bc22c.

Publication candidates were scanned for private-key/SID/password/API-key markers
with no matches; this bounded scan is not a universal secret audit. The requested
absolute WinPython toolchain path is intentionally included. Local TRX/command
logs with machine identities are excluded from proposed public source artifacts.

## Remaining risks, native plan and recommendation

| Remaining issue | Severity / software acceptance effect |
|---|---|
| Actual Excel range resolution, Value2/merged getters, workbook transitions, runtime performance and provider side effects not observed | High native qualification risk; timing explicitly dispositioned for this44-ID switch. Not proof of native parity and not an unexplained software test failure |
| Live HWND=reviewed planned HWND/PID/context, actually loaded XLL/assembly/source attribution, genuine independent Windows/COM risk approval | High; still owned by04A.45 and later GUI release work. No helper patched/run or independent signature fabricated |
| Sidecar intentionally tied to returned table identity, not arbitrary Core clones | Bounded API limitation, tested/documented; actual ImportInto cloneTable=False preserves it. Physical row IDs survive ordinary Core cloning independently |
| Existing superseded managed/frozen source failures and old strict/profile integrity failures | Explicit retained review exceptions with current successor proofs, not secretly green or erased |
| False mode retains its historical origin/singleton semantics | Deliberately stable under this authorization; not relabeled physical, not a hidden alternative route for True |

Future manual smoke **OWNER / NOT RUN**, contingent on exact native stage release,
owned PID/HWND/loaded-build linkage and independent review: (1) bounded disjoint
B3:B5/E8:E10, then overlapping B3:B6/E4:E7 with missing B5; observe actual areas
and row4/6 analysis mapping; (2) blank/numeric/text headers and a preseeded UDF
reference/array fixture, preserving culture/type/unknown-array provenance;
(3) RefEdit workbook/sheet switch, fault/cancel and teardown. No native workbook
write/output action is allowed while N2 remains NO. These plans are not executed
evidence, and host-free tests do not qualify native STA/connection points.

**04A.27 COMPLETE FOR OWNER REVIEW.** The software first-area geometry defect is
fixed in the shipping True-mode source and exercised through the real shared
importer assembly/conversion route. False-mode behavior is preserved. The
reviewed patch is active; no critical switch remains unapplied. Follow-up edits
are bounded, justified by required testing/safety and separately pinned. No
remaining unexplained software defect prevents acceptance under the approved
timing exception. Native control verification/release remains outstanding.

Proposed owner sentence: “Accept official04A.27 software completion and its dated
successor evidence under the recorded44-ID pre-change-capture timing exception,
with disclosed historical expectations retained and all native/P/N/C2 restrictions
unchanged.” This document does not issue that acceptance. Git publication needs
separate authorization;04A.06 is **not started**.

All44 original RefEdit obligations remain OPEN, owner_released=false and
native_executed=false. All **218 distinct transferred blocked controls /239 OPEN
transferred obligations**, **802 wider restricted live controls /1,514 original
obligations**, and the overlapping **141/160 UibyID subset** remain retained.
The2×2 input/output/help obligations remain OPEN; help is not a third blocked
control. **Released controls0, discharged obligations0; P=NO, N1=NO GO, N2=NO,
Gate C2 UNMET.** No live Excel/COM/UIA/ROT/NativeOM, native binary attribution,
native pilot, installer/signing, numerical golden promotion or04A.06 work.
