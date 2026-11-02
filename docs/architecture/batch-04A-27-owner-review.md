# Official 04A.27 owner review

Implementation record on `codex/04A-27`, from clean local main
`865a4e73b4668fbcbb7a2a24cceda02cc64bc22c`. Cached origin/main agrees; no
network operation. This is a single-parent publication of 04A.05 over
`849025214223b58a500cd7cceed237a666868ce8`; accepted 04A.03/04 and final
04A.05 source and dossiers are committed. The current owner's explicit
acceptance/publication statement supersedes the preserved plan revision2.4's
stale partial status. No historical plan/acceptance record is rewritten here.
Initial tracked worktree and index were empty; no unrelated changes found.
Branch creation alone required sandbox Git metadata permission and succeeded.

## Ownership and compatibility map (before editing production)

| Source member/callers | Actual existing semantics | This batch's boundary |
|---|---|---|
| ExcelDnaDataImporter.Import/ImportInto; paired t/Wilcoxon, wide Friedman/RM ANOVA, nested/PB and other GUI consumers | First area's rows/start reused for each area; True means first area's physical start. False labels rows from1 and rewrites colon ranges from1, but the existing singleton branch retains its original address; its IDs are therefore not uniformly physical. Whole-column last-used row derived from first area; Value (not Value2); ExcelError becomes missing, ErrorWrapper not normalized; names from first imported cell; DataObj CharCols/SkipRow and RemoveMissing retain producer IDs, not proof of original selection geometry | Corrected typed area join separately captures each rectangle. Restricted legacy switch needs new timing disposition; no generalization of C05 exception. Preserve False/CharCols/SkipRow legacy path. No invented original geometry |
| ExcelDnaSharedApplicationAdapter.ToRangeData/ToRangeSetData/SnapshotSelection | Ordered independent rectangles, Value2 metadata and datesystem; preflight all ranges; maximum10,000 cells and explicit smaller chunk limit, no chunked COM read; verify names/code-name/coordinates; reject unverified workbook/revision IDs | Reuse acquisition; common structural transport plus shared Windows value conversion; exact metadata, cancellation and identity tests. Large requests remain rejected before reader; no claim of large native read |
| WindowsPairedInputAdapter.Capture/SelectPairs/SelectBlocks/AlignHotelling | Capture legacy Core raw matrix with legacy NumericMatrix culture/missing semantics; original column coordinates unknown; real CoreTable.RowIds sidecar; complete rows/blocks and physical-row joins in Common | Route frame through canonical structural validation without changing legacy normalization. Preserve accepted policies and Core math. Corrected area frame bridge tested with same selectors, activation remains explicit |
| UdfDataImport.Get2D and representative paired reader | xlCoerce for ExcelReference; zero-based array assumed; numeric reader ignores text/logical/nonfinite; paired complete cases by aligned ordinal row; physical coordinates lost by Get2D; caller error conventions | Shared detached Windows snapshot/normalization bridge and Common frame validation; zero-based defensive copy for arbitrary array lower bounds; typed paired policy after legacy numeric eligibility. Reference geometry retained in new internal snapshot seam, plain arrays explicitly unknown. No exported signature change |
| RefEditEventLease and Excel2007RefEdit selection/workbook/write callbacks | Generation guards, single detach owner, invalidate both leases before teardown, fault requires recreation; borrowed RCWs; exact originating book/sheet object and active context checks | Exercise shipping leases/fail-closed reference identity; no live control construction, COM subscription change or native safety claim. Patch only demonstrated safe in-scope defect, otherwise retain production source |
| Core/GUI result writers | Authoritative algorithms/output schema; side effects after validation | Untouched. No table numerical algorithm or report/layout change |

Portable values never interpret dates solely from number formats. Names/code names
are acquisition context, not HWND/PID or loaded-build proof. Unknown UDF array
coordinates remain0; a CoreDataTable's legacy ordinal IDs must not be relabeled
physical worksheet provenance.

## Shipping API and ownership

New Common APIs (all `Option Strict On`, no new package or target):

```vb
SpreadsheetSelectionTransport.Capture(input As SpreadsheetRangeData,
    Optional rowIds As IReadOnlyList(Of Integer) = Nothing)
    As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)
SpreadsheetSelectionTransport.JoinColumns(input As SpreadsheetRangeSetData,
    alignment As SpreadsheetAreaAlignment, Optional limits As InputSizeLimits = Nothing)
    As SpreadsheetTypedParseResult(Of SpreadsheetTransportFrame)
SpreadsheetTransportFrame.SourceAt(rowIndex, columnIndex) As SpreadsheetTransportCoordinate
SpreadsheetTransportFrame.ToCoreDataTable(names As String(),
    Optional policy As SpreadsheetCellPolicyOptions = Nothing) As CoreDataTable
```

Frame values, areas and row IDs are defensively copied; `Data`/`Areas` return
detached copies. SourceAt carries original area index, selected row/column,
physical row/column, workbook/sheet name, sheet code name and original address.
Area snapshots retain date basis, number formats, errors, address dialect,
selection correlation and any supplied identity/revision constraints. Joined
logical columns deliberately have start-column0 and no invented A1 rectangle.
Core receives its original object/numeric/missing matrix plus actual RowIds;
the frame sidecar retains the richer per-column acquisition metadata which the
existing CoreDataTable/DataSourceInfo cannot carry. No Core schema change.

| Shipping call chain | Actual integration / guarantee |
|---|---|
| UiTwoInputRefedits.getData → existing ImportInto/DataObj → WindowsPairedInputAdapter.SelectPairs/Capture → SpreadsheetSelectionTransport.Capture → accepted PairedBlockInputPolicy | Actual form route, same imported values/rows, no new native reads. Whole-pair filtering and accepted difference direction unchanged |
| Ui0OneRefeditMulticol Friedman/RM ANOVA → SelectBlocks/Capture → same structural transport → accepted ParseBlocks | Actual complete-block route. Skillings–Mack/ICC/contingency/report modes not rewritten |
| Ui9ANOVA2nested/paired Hotelling → existing paired adapter | Shared Capture validation remains compatible; existing named Core policies and source-identity join retained. No nested/Hotelling numerical change |
| ExcelDnaDataImporter.AcquireRangeData/AcquireRangeSetData → shared Value2 adapter → SnapshotValues → canonical Capture | Existing real acquisition API, each rectangle separate; no claim these optional APIs replace legacy form acquisition. SnapshotSelection's callback seam separately owns private expected-reference copies and detached returned frames |
| UdfDataImport.Get2D → WindowsSpreadsheetTransport.ReadUdfMatrix | Real shared UDF acquisition seam. Array lower bounds normalized to a detached zero-based copy; references preflighted before xlCoerce and observed payload shape checked |
| TryReadNumericMatrix → NumericMatrix/NumericFrame/CaptureUdf → shared Capture/CoreTable bridge | Legacy finite-number-only eligibility retained, including NaN output for excluded numeric cells; header/trim offset explicitly carried into available physical reference coordinates |
| TryReadPairedNumericColumns / ExtractPairedNumericColumnsIgnoringNonNumeric → PairedArgumentsAligned → ExtractCompletePairedNumericRows → ExtractNumericPairs → Capture → existing TypedNumericRangeParsers.ParsePaired | Actual shared UDF paired readers. Whole-pair filtering; known references must agree in sheet token and physical rows before coercion. No separate pair algorithm |
| WindowsSpreadsheetTransport.ImportAreas → JoinColumns → ToCoreDataTable → real DataObj/SelectPairs | Corrected internal, non-user-reachable seam, exercised end-to-end by tests. Does not activate the blocked legacy-import switch |

Windows still owns all COM/Excel-DNA reads, worksheet resolution, RefEdit events,
UI state, writers and output. Common owns typed structure/alignment/cell policy;
Core owns every statistic. No form, writer, DataObjContracts, Helpers, Core math,
UDF export declaration, designer, RefEdit source or lease source was edited.

## Selection/value compatibility and limitations

| Case | Corrected transport result; original shipping behavior where relevant |
|---|---|
| B3:B5 + E8:E10 | Original private address method demonstrably rewrites E8:E10 to E3:E5. Independent capture actually reads B3:B5 and E8:E10. Identical-row policy rejects `transport.row-alignment`; explicit intersection rejects `transport.no-common-rows`; no table or write. Legacy Import still has the original behavior pending the switch below |
| B3:B6 + E4:E7 with a missing B5 | Intersection rows4,5,6 with `transport.rows-excluded` warnings and both original areas retained; DataObj/complete pairs retain rows4,6 and values{{2,10},{4,40}}. No ordinal compaction/zip |
| Same start/different length, reversed areas, 2D+column | Explicit identical rows reject unequal extent; intersection preserves common physical rows. Area/column order retained even when reversed. No alphabetic/category sorting |
| Repeated/overlapping cells, different books/sheets/code names/revisions/date bases | `transport.duplicate-cell` or `transport.identity`; no cross-product or same-name-sheet merge. Join requires captured nonempty book/sheet context; code-name/ID/revision constraints must agree |
| Row IDs101,103,107 with incomplete middle block | Capture sidecar preserves all three original IDs; accepted block policy returns101,107, diagnostics at103. Physical row alone is never a subject key |
| Plain UDF array / already coerced input | Workbook/sheet/physical coordinates unavailable, represented as empty/0. Arrays deliberately retain ordinal pairing. Existing numeric-array return types cannot expose a sidecar; internal frame can, without widening public exports |
| ExcelReference | Physical row/column and explicit trim offset retained in internal frame. SheetId is only an Excel-session token, no fabricated workbook ID/name or durable revision. Two known reference arguments with shifted rows or different SheetId now fail before coercion (False / ExcelErrorValue according to reader convention) |
| Raw errors/blank/text/logical/nonfinite | Nothing/DBNull/ExcelEmpty/ExcelMissing → Blank. ExcelError, ErrorWrapper and integer VT_ERROR HRESULTs preserve native codes. Text remains text including numeric-looking text; Boolean remains Boolean; numeric NaN/Inf survives raw capture and is rejected by typed policies or excluded by the existing UDF numeric eligibility. Unsupported objects/DateTime reject rather than invent serial dates |
| GUI legacy numeric versus UDF numeric | GUI legacy NumericMatrix culture/text normalization retained; UDF continues to exclude numeric strings, logical values, errors and nonfinite values. They share transport, not an incorrectly unified conversion rule. Common explicit invariant/sk-SK policies are separately tested |
| Dates/formats | Value2 numbers remain numbers even with date-looking format;1900/1904 metadata retained. Existing Common converter still handles the1900 leap-year quirk only when date interpretation is explicitly requested. Plain UDF arrays have Unknown date basis |
| Oversized/multi-area UDF reference | Metadata preflight before xlCoerce; one rectangular reference, default at most1,048,576 range/total/response cells and128MiB estimated at64 bytes/cell. Explicit smaller MaxChunkCells rejects before callback (`input.udf-response`). Full column admitted by metadata, wrong/truncated payload rejected. Full sheet rejected without callback. Limits are estimates, not a hard heap guarantee or proof of an actual large native read |
| Windows lossless acquisition | Existing max10,000-cell unchunked response cap retained, plus smaller MaxChunkCells, total/range/estimated-byte/time/cancellation checks. No claim of >10,000-cell lossless chunked read; no new COM chunking enabled |
| Full-column legacy import, bStartRow=False, CharCols/SkipRow | Existing behavior unchanged. Proposed True path rejects unbounded/oversized selections rather than borrowing/expanding geometry. False's intentional row1 mode is not globally replaced. Existing CharCols/SkipRow and whole-row missing filtering remain DataObj-owned |
| Merged/address/multi-area shape | Existing Common A1/dialect parser and Windows actual range resolution retained; each raw area must be rectangular, array dimensions must match, quoting is host/Common A1-owned, no new split grammar. Proposed importer patch rejects merged/unverifiable merge context before capture. That native getter is not executed here |

Validation codes include transport.shape/coordinates/row-map/ranges/alignment,
transport.identity/row-alignment/no-common-rows/duplicate-cell/rows-excluded/cell
and existing input.* budget and paired.* parser codes. Cell bridge failures
retain source-area field path and original selected/worksheet coordinates.
No statistics/report/p-value schema changed. Accepted C05 canonical60/55,+.05,
McNemar p≈.4995598332, zero-discordance p1 and paired CI widening remain intact.
The older TryBuildNumericDataObject and specialized agreement/contingency/mixed
readers are not wholesale migrated: their own header/missing/ordinal rules remain;
they benefit from Get2D's bounded, detached acquisition where called. Legacy
CoreDataTable ordinal IDs from opaque arrays are not physical worksheet evidence.

False/default callers were traced separately: Ui11PCA's main/CA imports,
Ui13GEE, Ui4Cox, UiGLM and DM/PSMdataImport model/outcome imports. PCA centers
explicitly uses True. Paired/blocked/nested, UibyID,2×2, MMRM/LMM, KM,
XYZ, Sankey, polar/convex-hull and kite imports use explicit True, including
the multiline calls and CharCols/SkipRow variants. They are not silently migrated
to a common False-mode interpretation. No claim that False producer IDs are
physical selected-row coordinates. The proposed patch's scope is explicitly True.

## Exact gated switch and proposed owner decision

**04A.27 PARTIAL — activating corrected physical-area geometry in legacy
ExcelDnaDataImporter.Import(bStartRow:=True) requires an exact native pre-change
timing disposition.** The shipping importer is byte-identical to the starting
baseline. Its restricted first-area defect is reproduced, not falsely closed.

`04A27/import-physical-geometry-PROPOSED.patch` passes `git apply --check` and is
**not applied or natively compiled/executed**. SHA-256:
`96a6dcfe84b630f8f8451a2da02acbb7108dbe26986c960ae3eebe4d8c8433b1`.
It resolves each selected area itself, uses existing bounded Value2 acquisition,
requests explicit physical intersection and Core-table mapping, leaves False mode
unchanged and rejects invalid/merged/oversized inputs. Proposed labels come from
the first retained common row. These are real behavior differences requiring
review; the separate C05 timing exception cannot authorize them.

The machine crosswalk `04A27/transport-restrictions-v1.json` binds original ledger
SHA `363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10`,
accepted C02 amendment SHA
`8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307`,
original source/decision row hashes,61 actual original member anchors, and44 exact
RefEdit obligations from16 actual importer-calling source files. This is a
conservative dependency set, not a claim that every option/control executes True
mode. Output/Deming controls in that set are retained dependencies, not an
assertion that their native behavior was modified. Source-row hashes are checked
against the actual original CSV, not fabricated from contemporary source.

Immediate paired/blocked/nested input IDs include:

- control:BESHStatNG.UiTwoInputRefedits.RefEdit1
- control:BESHStatNG.UiTwoInputRefedits.RefEdit2
- control:BESHStatNG.Ui0OneRefeditMulticol.RefEdit1
- control:BESHStatNG.Ui9ANOVA2nested.RefEdit1_Group
- control:BESHStatNG.Ui9ANOVA2nested.RefEdit2_nested
- control:BESHStatNG.Ui9ANOVA2nested.RefEdit3_Data
- control:BESHStatNG.UibyID.RefEdit1 and control:BESHStatNG.UibyID.RefEdit2

UibyID maps to ANOVA/normality cases04A02-ANOVA-NATIVE-01 and
04A02-NORMALITY-NATIVE-01; the exact2×2 input/output controls map to
04A02-2X2-NATIVE-01. The other rows retain their original C01 contracts even when
they are not members of a C02 transferred case. No new case IDs invented.

**One actionable owner request:** review the exact patch and44-ID dependency
matrix, then either authorize a case/criterion/ID-bound *timing exception* for
that physical-area True-mode switch with every original obligation OPEN, or
require the missing native baseline after independent risk/N authorization.
This request does not request N1/N2 execution or control release. Safe Common,
UDF and behavior-preserving GUI frame extraction can be reviewed independently;
the batch cannot be called complete while this critical switch is unapplied.

## RefEdit/native handoff and tests

Shipping RefEditEventLease and Excel2007RefEdit are unchanged. New lease tests
execute the real subscription owner with fake producers: generation invalidation
for five named context reasons, deferred stale callbacks, reentrant detach,
poisoned lease refusal, double Dispose/single detach and fresh-instance isolation.
Existing C09 tests cover replacement, failed detach, callback faults, reentrant
subscribe/clear and disposal during bind. Context-reason tests exercise the
generation guard, **not** actual control event detection or native workbook close.
Existing exact workbook/sheet object and active-context guards remain source-backed.
No uninitialized controls, hidden AppGlobals mutation, COM substitute or borrowed
RCW release introduced. No demonstrated new lease defect required a production
lifetime change; native UNKNOWN_HIGH remains unresolved.

04A.45 must address these three prerequisites before any future native attempt:

1. N14-HWND: scripts/04a02/native_n1_c09.cs:149 currently binds hwnd to live
   MainWindowHandle then checks its persistence, without equality to the reviewed
   plan HWND. Require live HWND=reviewed HWND plus PID/start/session/context; add
   negative same-process/different-window and wrong-plan tests in a versioned
   helper. The frozen helper and supervisor are untouched.
2. N14-SOURCE: actual loaded XLL/managed-assembly/source-build attribution and
   TOCTOU/calibration evidence. File hashes at rest or caller-supplied source pins
   are insufficient. Relevant supervisor: native_n1_supervisor_v1.py; original
   compile_native_n1_c09.py and C14 risk packet remain retained.
3. Genuine independent Windows/COM reviewer identity, qualification, date,
   reviewed source/mitigation and exact N-stage verdict, followed by owner release
   and disposable-workbook/provider calibration. No AI signature or self-attested
   independence. C14 N14-OBSERVATION/provider side effects remain unmeasured.

Native attempts0. No Excel, COM, UIA, ROT, NativeOM, native helper, live analysis
form handler, installer, signing, R oracle or signed-in Office session was run.
The unchanged full legacy suite does rerun its10 existing C10 **managed** RefEdit
entrypoint/handler tests: initialized controls on managed STA threads, injected
detach delegates, Nothing connectors/targets, no Excel session. These are retained
host-free baseline tests, distinct from our new fake-lease tests and from native
COM safety. Managed STA execution does not qualify COM connection points.

## Executable evidence and validation

New managed tests:24 Common Transport27Tests cases and28 Windows cases
(Transport27AdapterTests21, Transport27LeaseTests7),52 distinct total. Focused
suite includes accepted numeric/paired/acquisition/lease regression tests:
Common98 PASS and Windows69 PASS. Development compile-01 ran Common74/Windows46
before new tests; focused-01 ran98/65; focused-02 retained its test-only compile
failure (`single` reserved VB identifier), corrected to cellReference;
focused-03 ran98/69. A late identity audit found that SnapshotSelection passed
the same mutable reference to its callback and then used it as expected identity.
The fix separates expected/callback references, checks observed dimensions and
unverifiable revision metadata, and defensively detaches the returned frame.
This is a host-free callback seam change; no additional COM read or event change.
Five negative cases were added. The focused-identity-repair run retained its98
Common PASS /73-of74 Windows result: only a new test's singleton A1 spelling
expectation (`B3` versus existing canonical `B3:B3`) failed and was corrected
before the qualified full Windows build. No failed attempt was erased.
New successor Python12 tests
PASS, including original-row hash verification, zero releases, export count,
unapplied patch, portable boundaries and historical test preservation.

First full Debug and Release runs (47 new cases) were retained, not overwritten;
full suites were repeated solely for the late identity repair, with separate
qualified-Debug/Release TRX and internal writer paths. Only the qualified runs
count toward final distinct totals. Supplemental semantic capture repeats15
existing tests per configuration and is not added to those distinct totals.

The original C05 publication introduced two *preexisting* byte-pin conflicts,
separate from new C27 source changes. Candidate CSV old
`08d63b89160a9e14a61d027bbfcee3d5316fb0cdfb4f242bd5e69fbe5c131e67`
became `bc658b583856ca899aae94fa41717205824a7762ac31a09d1c380e094360b89e`:
23 CRLF rows became LF, and stripping CR proves byte-identical field data against
the retained C05 execution copy. Core contingency old
`225086892c13fb02c5dafba5c65b08c43c37fe70e0a7c7364d2067e732aac508`
became `96765d51276ccb802f5105029b30e002f7cc49664a5ae23bed2cbffd8798d345`:
the original file had mixed endings. Retaining original unchanged-line endings
and applying CRLF to accepted changed lines reconstructs the exact accepted
225086... SHA from current logical source. The successor records this executable
proof; Core's current SHA also equals the initial C27 baseline. Neither Core,
CSV, original candidate metadata, historical script nor C05 owner record was
rewritten. This is provenance reconciliation, not golden promotion.

Final checks also required an explicit smaller UDF response limit to be enforced
before callbacks, rather than using only aggregate limits. Default response cap
is explicitly1,048,576 cells, preserving full-column metadata admission; the
existing test now additionally proves a smaller MaxChunkCells rejects before a
reader call. This last Windows-only repair was followed by full Windows legacy
reruns in **both** configurations under response-Debug/Release, and a final
successor-source rerun. Unchanged Core/Common/Node results from the qualified
full runs are reused; the earlier Windows results are repetitions, not extra
distinct tests. No source/managed/native claim is inferred from the wrapper exit.

## Final validation and retained failures

| Final distinct suite | Debug pass/total | Release pass/total | Fail/skip per configuration |
|---|---:|---:|---|
| Core |1055/1055|1055/1055|0/0|
| Common |1010/1014|1010/1014|4 retained superseded /0|
| Windows legacy (latest response-budget rerun) |918/919|918/919|1 retained superseded /0|
| Office managed |505/505|505/505|0/0|
| WASM execution managed |45/45|45/45|0/0|
| C03 mitigation projection (explicitly test-only) |61/61|61/61|0/0|
| **Managed distinct** |**3594/3599**|**3594/3599**|**5 retained /0**|
| **Node** |**916/916**|**916/916**|**0/0**|
| **Python** |**612/639**|**612/639**|**27 nonpasses (18 FAIL,9 ERROR) /0**|

All52 new managed cases and12 new successor Python tests pass. The full-suite
commands truthfully return1 because of retained failures. Final summary's exit0
means only that actual failures exactly match the disclosed frozen sets and no
Node failure was found; it is not an all-green claim. No newly unresolved
functional/numerical regression identified. Full test identities/outcomes and
TRX hashes: local `TestResults/04A-27-20261102-run01/final-summary.json`.

The five unchanged managed failures are:

- ProportionsPolicyTests.PairedDisjointCountContractAndPureSemanticProjection
  (3 DataRows), expecting paired analysis to remain unavailable.
- ProportionsPolicyTests.ExistingEquivalenceCoreDomainAndPairedOverflowAreNotReinterpreted,
  expecting the accepted C05 overflow defect to remain.
- ContingencyAdapterTests.PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden,
  expecting the wrong pre-authorization p≈6.4931e-8 rather than corrected≈.49956.

All22 previous Python nonpass identities **and FAIL/ERROR outcomes** remain
present, compared by script to the retained C05 completion summary. Their C02
RefEdit/receipt/UibyID/hash and superseded C04/C05/old-plan conditions are not
rewritten or hidden. Five additional nonpasses are classified exactly:

| Test identity (module.class.method) | Disposition |
|---|---|
| test_04a04_alignment.PairedBlockSourceGates.test_udf_exports_and_packaging_unchanged | Newly triggered frozen ban on any UDF-path edit; actual268 export declarations/signatures and packing remain unchanged |
| test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change | Newly triggered frozen ban on UDF-reader edit; successor tests bind the authorized reader transport change |
| test_04a05_contingency.ContingencySourceGates.test_prior_04a04_whole_file_source_check_remains_frozen_and_disclosed | Newly triggered WindowsPairedInputAdapter whole-file pin; current member pre/post anchor and producer tests qualify the narrow Capture change |
| test_04a05_completion_successor.Completion05SuccessorTests.test_independent_candidate_provenance_not_core_or_golden | Preexisting publication CSV newline conflict, independently proven above; original fixture/metadata retained |
| test_04a05_completion_successor.Completion05SuccessorTests.test_owner_bound_successor_and_source_hashes | Preexisting publication Core mixed-ending normalization conflict, exact old SHA reconstructed above; Core unchanged in C27 |

Thus **three newly triggered source-provenance conflicts plus two preexisting
publication-byte conflicts**, not five new numerical defects. Existing native
source mapping errors can also encounter the intentionally changed shared
snapshot file; their original failed identities remain failed and all original
pins remain retained. Nine current member anchors bind pre-change main and new
logical member text independently of original C01 IDs/whole-file pins.

Read-only historical checks were actually run once: original Roslyn
`--check --strict` exit1 (stale UibyID→NormalPlot review dependency), approved
`owner_closeout.py --profile inventory-complete-with-approved-deferrals` exit1
(integrity failure). The historical24-failure audit census remains retained
historical evidence, not a newly recomputed24 count or passed exit gate. No sealed
audit renderer/writer or R candidate execution was rerun.

Reproducible commands, from repository root (all command arrays, durations,
exit codes and script-isolation diffs retained in new run's command JSON/logs):

```powershell
$PythonExe = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
$Run = 'TestResults/04A-27-20261102-run01'
& $PythonExe -B scripts/verify-04a27.py init --evidence $Run
& $PythonExe -B scripts/verify-04a27.py focused --evidence $Run --name compile-01
& $PythonExe -B scripts/verify-04a27.py focused --evidence $Run --name focused-01
& $PythonExe -B scripts/verify-04a27.py focused --evidence $Run --name focused-02
& $PythonExe -B scripts/verify-04a27.py focused --evidence $Run --name focused-03
& $PythonExe -B scripts/verify-04a27.py focused --evidence $Run --name focused-identity-repair
# Original full runs, followed by justified repair runs; each has a fresh destination.
& $PythonExe -B scripts/verify-04a27.py final --evidence $Run --configuration Debug --name final-Debug
& $PythonExe -B scripts/verify-04a27.py final --evidence $Run --configuration Release --name final-Release
& $PythonExe -B scripts/verify-04a27.py final --evidence $Run --configuration Debug --name qualified-Debug
& $PythonExe -B scripts/verify-04a27.py final --evidence $Run --configuration Release --name qualified-Release
& $PythonExe -B scripts/verify-04a27.py legacy-final --evidence $Run --configuration Debug --name response-Debug
& $PythonExe -B scripts/verify-04a27.py legacy-final --evidence $Run --configuration Release --name response-Release
& $PythonExe -B -m unittest discover -s scripts/tests -p test_04a27_transport_successor.py -v
& $PythonExe -B scripts/verify-04a27-contract.py check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply --check docs/architecture/04A27/import-physical-geometry-PROPOSED.patch
dotnet scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll --check --strict
& $PythonExe -B scripts/windows-migration-audit/owner_closeout.py --profile inventory-complete-with-approved-deferrals
& $PythonExe -B scripts/verify-04a27.py summary --evidence $Run
& $PythonExe -B scripts/verify-04a27.py preserve --evidence $Run --name final
```

`init/create` commands are no-clobber, not commands to rerun over this directory.
The wrapper reuses the unchanged C05/C02 assembly-copy isolation and generates
a local execution copy of scripts/verify-beshstatng.ps1 with **-Legacy -Wasm -Js
-Python -VisualStudio2026 -Configuration Debug/Release -PythonExe**. It records
every execution-copy substitution and continues independent suites after known
nonpasses, preserving their exit1. No restore/package/network access. Windows
host Compile/CopyFiles targets omit generator/packing/signing targets. Only one
build runs at a time. Additional Python modules/tools/help tests and all Node
suites are from the actual verification script, not hand-selected substitutes.

Final TRX locations relative to the run:

- qualified-Debug/Debug/results/{Core,OfficeAddin,OfficeExecution}.trx and
  qualified-Release/Release/results/{Core,OfficeAddin,OfficeExecution}.trx.
- executions/qualified-{Debug,Release}-Common/results/Common.trx.
- executions/response-{Debug,Release}-Legacy/results/Legacy.trx (latest Windows).
- projection-executions/qualified-{Debug,Release}/results/ControlMitigation.trx.
- executions/qualified-{Debug,Release}-semantic/results/Common.trx (15 repetitions).

Build warnings are the inherited MSTest discovery warning on nonpublic
MatrixTestHelpers and Git's CRLF checkout advisory; no new compile error remains.

## Protection and publication handoff

Retention R **PASS**:155,841 historical files actually checked against the initial
inventory, with complete path membership, lengths, creation/write nanoseconds and
file identity unchanged; no unexpected historical-root file and no protected
authority/source-hash drift. This includes all retained C01–C05 test evidence and
failed attempts, not just tracked files. It is a metadata/path and selected-hash
proof, not a reread/hash of every historical content byte. Receipt:
`TestResults/04A-27-20261102-run01/preservation-final.json`.
Initial free540,107,378,688 bytes; final538,066,698,240 bytes (about501GiB).
No historical cleanup, overwrite, deletion, archival, compression, movement,
retimestamping, symlink substitution or rebaseline occurred. All repeats use
new paths inside this run; no other output tree was reused.

Final branch/HEAD/local main remain codex/04A-27 /
865a4e73b4668fbcbb7a2a24cceda02cc64bc22c / same. No unrelated baseline edits,
deletions or unexpected change found. Final `git diff --check` and empty-index
checks pass. Normal publication candidates,14 paths (5 tracked modifications,
9 new files), all intentionally unstaged:

- BESHstatNG/BESHStatNG.vbproj — one host Compile include.
- BESHStatNG.Test/BESHStatNG.Tests.vbproj — two new test Compile includes.
- BESHstatNG/src/DM/ExcelDnaSharedApplicationAdapter.vb — canonical detached
  snapshots, shared cell converter and private callback verification authority.
- BESHstatNG/src/DM/WindowsPairedInputAdapter.vb — real Common frame delegation.
- BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb — actual
  bounded acquisition, numeric/paired reader routes and known-reference guard.
- BESHStat.Excel.Common/src/Input/SpreadsheetSelectionTransport.vb — typed
  structure, area/row alignment, immutable sidecar and existing Core bridge.
- BESHStat.Excel.Common.Tests/src/Transport27Tests.vb —24 production policy cases.
- BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb — pure Windows normalization,
  UDF provenance/budget and corrected non-user-reachable import seam.
- BESHStatNG.Test/src/Transport27AdapterTests.vb —21 actual adapter/reader cases.
- BESHStatNG.Test/src/Transport27LeaseTests.vb —7 shipping lease cases.
- scripts/verify-04a27.py — additive isolated execution wrapper and summaries.
- scripts/verify-04a27-contract.py — exact successor/gated-patch crosswalk.
- scripts/tests/test_04a27_transport_successor.py —12 source/restriction checks.
- docs/architecture/batch-04A-27-owner-review.md — this single dossier.

Two additional **ignored** review candidates require an explicit later staging
choice: docs/architecture/04A27/transport-restrictions-v1.json and
docs/architecture/04A27/import-physical-geometry-PROPOSED.patch. They are not
implicitly published. Existing C01–C05 local-only ledgers, owner objects, accepted
receipts/crosswalks, native/risk/candidate records, C03 projection dependencies and
all TestResults remain excluded/preserved. A GitHub clone does not reproduce that
local history; retain independent backups without replacing the originals. No
new VB/test source is masked by .gitignore. No .gitignore, Core source,
framework/project target, UDF export, designer, importer, Helpers/DataObj schema,
RefEdit/lease, version, native helper, writer, installer or signing change.

Publication candidates were checked for private-key markers, SID patterns and
password/API-key names with no matches; this bounded scan is not a universal
secret audit. The requested WinPython toolchain path appears intentionally in
the reproducible commands. Machine/user identities in local TRX/command logs are
not publication artifacts.

## Future native smoke — OWNER / NOT RUN

All three plans require genuine independent Windows/COM review and exact owned
PID/HWND/loaded-build attribution plus case/stage authority first; no N2 write.

1. In a reviewed disposable workbook, select B3:B5 and E8:E10, then overlapping
   B3:B6/E4:E7 with a gap. Verify actual captured rectangles and row sidecar before
   calculation; disjoint inputs must reject under the corrected policy. Confirm
   legacy shipping path is unchanged until the exact patch is authorized; never
   claim this read-only plan has demonstrated worksheet no-write behavior.
2. Compare one bounded UDF reference and an equivalent plain array against the
   GUI fixture. Confirm values/header/missing rules; reference rows/sheet token
   present internally, array physical coordinates unavailable. Reject shifted
   known reference pairs and oversized requests; retain UDF error convention.
3. With owned disposable books/sheets, exercise RefEdit selection, switch/close
   source, cancellation/fault and repeated teardown. Verify stale generations
   never apply to another context, detach ownership once and fault requires
   recreation. Native provider/STA/RCW behavior remains unverified here.

## Owner disposition and next dependency

**04A.27 PARTIAL — corrected legacy True-mode physical-area importer activation
is gated by missing exact native pre-change timing disposition.** Safe shipping
transport/UDF extraction and corrected host-free producer/consumer seam are
implemented and tested. The original first-area geometry defect remains in the
shipping legacy importer; this batch does not claim practical complete wiring.

Proposed owner statement: “Accept the qualified safe 04A.27 transport and
successor evidence as partial; review the exact96a6dc... proposed importer patch
and44 original-ID dependency matrix for a separate bounded timing disposition,
with all obligations OPEN and no native execution/release implied.” This is a
proposal, not an owner approval or a request for routine checkpoints.

After complete04A.27 acceptance,04A.06 can consume captured frames, stable area
coordinates/row maps, detached raw cells and existing univariate/paired policies;
it must not infer original selection geometry from a legacy compacted matrix.
04A.45–48 retain native HWND/source-loaded attribution, independent risk review,
real provider/GUI evidence and exit qualification. No04A.06 or other numbered
batch is started and no Master Plan acceptance count is edited here.

Retained totals: **218 unique transferred blocked controls /239 OPEN transferred
obligations;802 wider restricted live controls /1,514 inherited obligations;
shared UibyID141/160**. These overlap and are not summed. Exact2×2 obligations
remain OPEN: control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData,
control:BESHStatNG.Ui82x2.RefEditOutput and control:BESHStatNG.Ui82x2.btnHelp
(help is not a third blocked control). **Zero controls released, zero obligations
discharged; P=NO, N1=NO GO, N2=NO, Gate C2 UNMET.** No staging, commit, merge,
push, fetch, native execution, scientific golden promotion or04A.06 start.

One owner review is requested for this consolidated partial result and the exact
gated importer action. No general investigation or continuation series is proposed.
