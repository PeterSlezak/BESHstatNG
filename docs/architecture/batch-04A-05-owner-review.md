# Official 04A.05 owner review

Pre-implementation ownership/compatibility map, 2026-11-02. Branch codex/04A-05
starts at clean local main 849025214223b58a500cd7cceed237a666868ce8 (also cached
origin/main). Its tree contains committed 04A.04 policies, adapter, real GUI
integration and dossier; the owner states acceptance. No merge was synthesized,
network accessed or publication performed. Historical receipt text is retained.

| Source member / behavior | Numerical authority and order | Intended ownership and compatibility |
|---|---|---|
| Ui82x2.getDataMultipleGroups/btCompute_Click | DataObj complete-row filtering then Matrix.Array2intArray/CInt rounding; dialog {{A,B},{C,D}} | New strict typed count policy reuses ParseContingencyCounts. Restricted worksheet/dialog compute wiring held if strict rejection differs from frozen native path; no bypass flag or silent rounding in the new policy |
| Ui82x2 report sequence | Analyzed; Fisher exact/mid-p (column2 p-values); Chi2 (2,2) + nominal whenever Chi2 OR Association; Liddell (1,2); OR Woolf/Cornfield; RR | Common calls unchanged Core and projects exact rows/CI strings/markers; >1000 Fisher skip warning retained; no numeric CI reparsing |
| Ui0OneRefeditMulticol.getDataMultipleGroups/RunRxC | Current numeric producer/filtering; Matrix.Array2intArray; Chi2 mandatory, nominal optional, FFH try/catch, ordinal tau-b/tau-c/Gamma/Somers D Item4 columns dependent, trend transpose only 2 rows | Windows acquires raw producer snapshot, Common validates exact integer counts and maps semantic reports. Only RxCTable selection branch changes; SelectBlocks and all unrelated tags retained |
| RxC markers / labels | Chi2 (2,2), FFH (1,2), ordinal (4,2),(8,2),(12,2),(16,2), trend (2,2),(4,2); legacy second trend label repeats linear-trend wording | Common preserves row sequence, CurrentCulture CI percentage labels, Core ordinal strings, FFH failure text and trend dimension behavior; no Core formula copied |
| Ui8Proportions single | SingleProportion; 2*BinomDist(min(r,N-r),N,.5,True) capped1; total/responders/proportion/CI/p rows; p(5,2) | Typed Common count/options policy and Core-backed report; positive N and r<=N |
| Independent superiority | TwoIndependentProportions(control,test) is control-minus-test; Fisher {{control responders,test responders},{control nonresponders,test nonresponders}}; p(9,2),(10,2) | Preserve existing sign, layout, two-sided CI and exact/mid-p. Do not silently relabel as experimental-minus-control |
| Independent NI/equivalence | Existing Core functions explicitly experimental-minus-control; positive margin; NI -margin / equivalence [-m,+m]; one-sided alpha; CI 1-2alpha; NI p(13,2), TOST p(12,2),(14,2),(15,2) | Common validation/calculation/projection. Existing ValidateIndependentMargin is currently unwired; wire validation/focus without changing mode event behavior |
| Paired proportions | Core PairedProportions treats B,C as exclusive and D as both. Old form Liddell table {{B-D,D},{N-B-C+D,C-D}} instead treats B,C as marginals, while off-diagonal test uses D and N-B-C+D | Scientific alignment candidate, not a new permanent wrong portable algorithm. Implement strict disjoint counts and semantic projector; keep existing affected numerical invocation guarded/unchanged pending an exact correction decision if parity cannot be maintained |
| Windows-only operations | Constructors/designers/RefEdit, worksheet/workbook resolution, MsgBox/focus, output destination/overwrite, Excel writes | Remain Windows-owned; no native execution or RefEdit lease changes |

Pre-change source SHA-256 (actual checkout bytes): Ui82x2.vb
8df9f73fd01e5677f671b8f30ff423d6d7f038a17361bc2c1c4c9f9c9ff41d85;
Ui0OneRefeditMulticol.vb f4bb3adcad29c2af20922276d7ddee2912d95e5b27580a97c0653755350e0013;
Ui8Proportions.vb 72ebd241734d3c4ecfce2039a3836b56fa7bcd03cac3655d93edd14c2daba2c7.
New source/semantic checks will be versioned independently of sealed expectations.

Planned files: Common contingency/proportions typed policy and semantic reports;
one Windows adapter; RxC and proportions form code-behind plus explicit host/test
Compile entries; Common/Windows/source tests; an isolated verification wrapper;
this dossier and one compact 04A05 source/restriction/compatibility ledger.
Ui82x2 wiring is a specific native pre-change decision, not an implicit release.

Exact 04A02-2X2-NATIVE-01 owner is 04A.05. Original blocked controls are
BESHStatNG.Ui82x2.RefEdit1_WorksheetData and BESHStatNG.Ui82x2.RefEditOutput;
deferred obligations are their exact control: IDs plus control:BESHStatNG.Ui82x2.btnHelp.
Amendment hash 8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307
and receipt remain immutable. No fresh case/criterion/ID-bound deferral is granted.
All 218/239 and 802/1514 restrictions remain; P=NO, N1=NO GO, N2=NO, Gate C2 UNMET.

## Shipping implementation and contract status

Main publication is a single-parent commit: 8490252 has parent e1377cc. Both
accepted predecessor extraction trees are present; no separate new signed
04A.04 receipt was found. The owner's current instruction supplies the acceptance
statement, independently supported by committed source/integration and retained
review/evidence anchors. This batch does not rewrite historical receipt wording.

| 04A.05 contract | Production API / real consumer | Executable qualification | Status |
|---|---|---|---|
| Exact counts and ordered categorical axes | ContingencyInputPolicy.Parse/FromDialog → existing ParseContingencyCounts | InvalidCountHasCoordinatesAndNoCoreInput; ExplicitLabels_OrderTypesCaseAndHeadersRemainDistinct | Implemented; strict portable/host-free |
| Checked Core Int32 boundary | ContingencyCountSelection.CoreCounts, cell and aggregate checks | SourceRowMismatchAndInt32EdgesAreExplicit; invalid count data rows | Implemented; no rounding/truncation |
| 2×2 construction/statistics/semantic report | ContingencyReportPolicy.TwoByTwo; adapter AnalyzeDialog/ParseWorksheet | TwoByTwo_AllSelected_ExactLegacySemanticSnapshot; IndividualOptions; FisherGuard; zero-cell cases | Implemented and tested; **Ui82x2 production connection withheld** |
| RxC nominal/exact/ordinal/trend | Ui0OneRefeditMulticol.btCompute_Click → RunRxC → WindowsContingencyInputAdapter.AnalyzeRxC → Common | RxC_LegacyOrdinalNominalAndTrendSnapshots; exact success/failure; producer/culture adapter tests | Wired; valid-input managed parity |
| Single/independent superiority | Ui8Proportions.btCompute_Click → adapter → ProportionsInputPolicy.Validate → ProportionsReportPolicy.Analyze | Single_ExactLegacySnapshotAndCap; independent snapshots; real adapter cases | Wired |
| NI/TOST margins, alpha and report | Same route; ValidateIndependentMargin now calls Common ValidateMargin | Independent_AllHypothesesLegacyTablesSignsCIAndMarkers; invalid mode/count/margin rows | Wired; existing Core definitions unchanged |
| Paired count accounting and semantic projection | Common validated disjoint request → Windows LegacyPairedEvidence → Common ProjectPaired | PairedDisjointCountContractAndPureSemanticProjection; host legacy evidence test; discrepancy fixture | Wired with **explicit unresolved scientific compatibility exception** |
| No statistics/CI parsing in forms | RunRxC and Ui8Proportions compute contain control reads, guards, presentation and writer flow only | Shipping source/member checks; complete semantic snapshots | Met for those two paths; **unmet in unchanged Ui82x2** |
| Native Windows manual contract | Exact-case pre-change disposition / independent reviewer / owned-process plan | Not executed, no native claim | Restricted / NOT RUN |

New Common signatures (namespace BESHStatNG.ExcelCommon):

```vb
ContingencyInputPolicy.Parse(input As SpreadsheetRangeData,
    options As SpreadsheetContingencyCountParseOptions,
    Optional requireTwoByTwo As Boolean = False,
    Optional sourceRows As IReadOnlyList(Of Integer) = Nothing)
    As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
ContingencyInputPolicy.FromDialog(a As Double, b As Double, c As Double, d As Double)
    As SpreadsheetTypedParseResult(Of ContingencyCountSelection)
ProportionsInputPolicy.Validate(mode As ProportionsMode,
    hypothesis As IndependentProportionsHypothesis, n1 As Double, r1 As Double,
    n2 As Double, r2 As Double, both As Double, alpha As Double, margin As Double)
    As SpreadsheetTypedParseResult(Of ProportionsRequest)
ProportionsInputPolicy.ValidateMargin(hypothesis As IndependentProportionsHypothesis,
    margin As Double) As SpreadsheetValidationReport
ProportionsInputPolicy.ModeFromChoices(singleSample As Boolean,
    independent As Boolean, paired As Boolean) As ProportionsMode
ContingencyReportPolicy.TwoByTwo(input As ContingencyCountSelection,
    options As TwoByTwoReportOptions) As ContingencyReportResult
ContingencyReportPolicy.RxC(input As ContingencyCountSelection,
    options As RxCReportOptions,
    Optional exactEvaluator As Func(Of Integer(,), Double) = Nothing)
    As ContingencyReportResult
ProportionsReportPolicy.Analyze(request As ProportionsRequest) As ContingencyReportResult
ProportionsReportPolicy.ProjectPaired(request As ProportionsRequest,
    interval As ConfidenceIntervalResult, test As TestResult) As ContingencyReportResult
```

The optional exact evaluator is a host-free engine failure seam; production uses
Core FisherExactEngine. Its exception path retains the exact old FFH fallback
text, not a fabricated successful p-value. No separate statistical algorithm is
provided. CI numerical endpoints are copied from Core into existing immutable
AnalysisMetricArtifact values; formatted Core CI strings are projected directly.

ContingencyCountSelection retains the defensive Long-count table, a checked and
copied Integer matrix, immutable SpreadsheetFrameCell source mapping, copied
workbook/worksheet IDs/name and opaque acquisition address. Unknown coordinates
and identities stay zero/empty. ProportionsRequest is immutable and exposes the
explicit independent Fisher table and paired **disjoint** outcome table. It
does not expose a portable builder for the incorrect legacy paired test table.
ContingencySemanticTable copies body/header/formatting positions and builds fresh
Core ResultTables or existing AnalysisTableArtifact/ResultTableOutputModel
objects. Error results expose zero tables/metrics, including after a Core failure.
Report input guards enforce at least two rows/columns before table statistics.

## Input, orientation and behavior deltas

The count policy composes the existing parser and cell policy; no second generic
contingency parser, CI parser, distribution or ratio formula was introduced.
It preserves categorical axis order, distinguishes text "0" from numeric 0,
supports explicit headers/row labels and configured case sensitivity, and rejects
duplicate category keys. It never sorts levels or turns counts into raw pairs.
The generic parser retains Long counts and its 2^53−1 precision limit; the new
statistical boundary additionally rejects individual cells or totals above
Int32.MaxValue. Zero cells are allowed; zero table totals are rejected. No new
pseudocount or endpoint repair is applied. Core may return undefined/infinite
intervals, which retain their original display strings and numerical values.

| Route / choice | Before | After / qualification boundary |
|---|---|---|
| Ui82x2 worksheet | DataObj drops unusable rows, then CInt-based count conversion can round 1.5 to 2 | **Unchanged shipping source**. New strict adapter rejects raw fractional/missing counts; proposed patch requires exact native case disposition |
| Ui82x2 dialog | {{A,B},{C,D}}, existing options and output | Unchanged shipping handler. Same layout and report are qualified through the production adapter in host-free tests; no claim the form calls it |
| RxC raw acquisition | Same RefEdit/prepareRef2D/ImportInto(True); final filtered matrix then CInt | Existing acquisition retained. Adapter reads detached raw CoreTable with its actual RowIds, so invalid/missing cells now diagnose rather than silently delete a row or round counts |
| RxC headers/labels | No label selector in this path; text header rows could disappear during filtering | GUI explicitly requests no headers/row labels; nonnumeric header rows now reject. Portable API supports explicitly supplied labels/headers. No auto-guess, new UI or silent header inference |
| Modern count errors | Fractional/nonfinite/negative/missing/error/precision hazard must not become counts | Stable parser diagnostics, source/selected coordinates and checked Core conversion. All-zero, malformed/too-small table and unknown options fail before analysis |
| Legacy Excel cell normalization | Importer maps native Excel errors to Nothing and normalizes numeric text by current culture | Adapter does not invent lost error codes or change the importer. Missing diagnostics retain actual raw row; strict lossless portable cells retain their existing error metadata |
| Proportion count bounds | Partial form checks; paired disjoint sum could exceed N; N=0 could enter computation | Positive totals, responders<=N, nonnegative exact counts, complete paired accounting, finite alpha and positive active NI/TOST margins validate before Core |
| Inactive fields | Hidden mode controls may retain old values | Single ignores unused counts/margin; superiority ignores margin; mode selection must be exclusive; invalid enums fail |
| Superiority versus NI/TOST | Core superiority is sample1−sample2; NI/TOST explicitly experimental−control | Preserved exactly. NI null limit is −margin; TOST margins [−m,+m], one-sided alpha and equivalent CI 1−2alpha |
| Paired inference | CI uses exclusive counts; legacy Liddell uses a contradictory marginal-count layout | Existing invocation retained only in named private Windows compatibility seam; correct disjoint policy and portable report projector are separate; correction requires scientific decision |

Windows numeric-text conversion uses the actual producer's NumericMatrix/current
culture semantics; Common sees typed numbers, or explicitly configured numeric
text via the existing cell policy. Boolean/DateTime count values reject. Error
cell diagnostics in the lossless parser and missing diagnostics after legacy
normalization are distinguished. Physical columns remain unknown/zero, since
DataObj does not provide trustworthy selected-column provenance. The opaque
SourceAddress is the producer's descriptor, not proof of original selection
geometry; the 04A.04 combined-range limitation remains assigned to 04A.27.

Important new codes include contingency.source-rows, .two-by-two, .int32-count,
.int32-total, .shape, .input, .options, .options.none, .no-result, .core,
.fisher.too-large, .trend.shape; proportions.mode, .alpha, .margin, .int32,
.int32-total, .total, .responders, .paired-accounting, .input, .core and
.paired-evidence-required/.paired-evidence. Existing category/cell/contingency
parser codes remain authoritative. Warnings are distinguished from errors:
Fisher >1000 skips exact work; other selected tables may continue. Invalid trend
dimensions warn and omit the trend table, preserving the other RxC results.

## Semantic parity and scientific findings

ContingencyLegacyCharacterization.vb is a **test-only** frozen projection of
the three 8490252 form computation/report bodies. Control reads become explicit
arguments; invalid MsgBox branches throw; no shipping constructor, event handler,
COM connector or writer executes. A missing Windows Matrix reference was replaced
by a test-only transpose loop. Provenance and original Git source preimages are
retained under the isolated run. These fixtures are characterizations, not newly
approved numerical goldens and not native before/after evidence.

The actual Common policy/adapter outputs compare every semantic table cell,
table sequence, header counts, p-value columns and body-cell markers against
those old bodies. Fixtures cover {{12,5},{7,19}}, asymmetric row/column swaps,
3×4, 2×3 and 3×2 tables, zeros, all-selected/individual options, two cultures and
alpha .05/.1 (also .075 for decimal percentage labels). Fisher column2 exact/mid-p
ordering and association-implies-Chi2 are preserved. RxC Somers D uses Core Item4
with columns dependent; ordinal p-value rows remain 4/8/12/16. Only the 2-row
trend branch transposes. Both legacy trend p-value labels retain their original
wording. Exact engine success uses Core; failure is tested with an injected
exception and retains ".error. not possible to compute" plus its p-value marker.

Single N100/responders45, midpoint cap1, all-success/all-failure, independent
N100/r45 versus N80/r52, NI and TOST compare complete old semantic snapshots.
Paired count accounting and pure projection also execute; the Windows compatibility
test explicitly compares the old Core invocation without calling it correct.
Concurrent fresh single requests and mutation attempts verify detached/copy
boundaries; this does not certify COM concurrency or every Core engine's threading.

Three explicit findings remain; none changes Core:

1. **SC-PROPORTIONS-PAIRED-01.** N100, first-only20, second-only15, both40 gives
   the disjoint outcome table {{40,20},{15,25}}. The old form instead passes
   {{−20,40},{105,−25}} to Liddell. Its p-value tests both40 against105, not the
   discordant20 versus15; executable tests demonstrate different p-values.
   Legacy report rows also label a count as a total and a fraction as a count.
   Common Analyze deliberately refuses to invent a paired test mapping; the GUI
   temporarily supplies its unchanged Core evidence through LegacyPairedEvidence.
   The owner/scientific reviewer must decide the exclusive-category test/labels
   and zero-discordance behavior before correction. This remains an **04A.05
   unresolved scientific exception**, not work silently shifted to 04A.27.
2. **SC-CORE-PAIRED-OVERFLOW-02.** PairedProportions(1000,100,80,300) has valid
   disjoint counts but overflows an existing Integer intermediate. The test
   reproduces it; the adapter returns a structured failure with no partial tables.
   A Core contingency maintainer/scientific review owns any numerical repair.
3. **CORE-EQUIVALENCE-DOMAIN.** Existing Core rejects an observed difference .2
   outside symmetric margin .15. The success fixture uses .3; a separate negative
   case retains the existing rejection. No new TOST definition was substituted.

UDF inventory: CT_PAIRED_PROPORTIONS / BESH.CT.PAIRED_PROPORTIONS already names
respondersOnly1/respondersOnly2 and calls the same Core interval. Its current
validation also lacks the full disjoint-sum check, an alignment candidate for
04A.27 after an owner decision. The paired-table UDF calls Core Liddell directly
on its supplied table; the GUI-only wrong construction is the finding above.
The nominal/ordinal/trend and independent interval UDFs use the same unchanged
Core methods. No export signature, array layout or public UDF behavior changed.

## Exact withheld native delta and requested disposition

Shipping Ui82x2.vb remains byte-identical at SHA-256
8df9f73fd01e5677f671b8f30ff423d6d7f038a17361bc2c1c4c9f9c9ff41d85.
The owner-review patch `04A05/ui82x2-wiring-PROPOSED.patch` replaces only its
compute body and private input-return conversion with typed adapter/report calls;
acquisition, options/events and writer/overwrite flow stay Windows-owned.
**NOT APPLIED, NOT BUILT, NOT NATIVE-EXECUTED.** Shared API calls and semantic
outputs are tested, but the patch itself is a withheld proposal.
Its SHA-256 is 95328f1417731c433ef39fe1e6a6777e637d2ecf822a9541ffed4779e59e52f4;
read-only `git apply --check` passes against the unchanged form.

The semantic delta that prevents unilateral application is explicit rejection
of raw fractional/missing/error count cells versus current row filtering and
rounding, together with the affected 2×2 semantic output connection. There is no
missing general investigation: the smallest requested authorization is a fresh
**04A02-2X2-NATIVE-01 / strict count validation and Common report-wiring criterion /
control:BESHStatNG.Ui82x2.RefEdit1_WorksheetData, control:BESHStatNG.Ui82x2.RefEditOutput,
control:BESHStatNG.Ui82x2.btnHelp** disposition bound to this patch/source and
its host-free fixtures. It may defer native capture; it must not imply N1/N2 or
release a control. The existing amendment alone is not that fresh disposition.

The source/restriction ledger retains the three exact original row hashes,
source decision hashes, risk tiers (two RefEdit P0, help P2), owners, gates and
safe-capture preconditions. All three obligations remain OPEN; help is not a
third blocked control. No independent risk-review signature, exact native
baseline, owned PID/HWND equality or loaded-XLL/source attribution is supplied.
Those remain future prerequisites to any owner-authorized native pilot. Wider
218/239 and 802/1514 populations remain unchanged, with zero releases/discharges.
P=NO, N1=NO GO, N2=NO, **Gate C2 UNMET**.

## Validation, exceptions and preserved evidence

| Final suite | Debug | Release |
|---|---:|---:|
| Core MSTest | 1,024 pass | 1,024 pass |
| Common MSTest | 961 pass | 961 pass |
| Windows legacy MSTest | 866 pass | 866 pass |
| Office managed contracts | 505 pass | 505 pass |
| WASM execution managed contracts | 45 pass | 45 pass |
| Prior mitigation projection | 61 pass | 61 pass |
| **Distinct managed cases per configuration** | **3,462 pass** | **3,462 pass** |
| Node (468+71+377) | **916 pass** | **916 pass** |
| Python (27+16+403+168) | **598/614 pass** | **598/614 pass** |

No managed/Node failures or skips. There are **73 new managed cases** (60 Common,
13 Windows) and **16 new Python source/gate cases**. The last focused Common run
has 85 passes (60 new plus 25 retained numeric-parser cases), Windows13, source16.
Existing focused Core contingency/equivalence references: 17 passes; an earlier
five-case equivalence-only filter is a repetition, not five more distinct tests.
Source-derived mapping fixtures do not independently validate a changed Core
algorithm; unchanged Core reference tests and source separation provide that
boundary. No new R, P-golden or mathematical reference was generated.

The full Debug sequence ran once, then a final API review added the pre-Core
minimum 2×2 report shape guard and structured Core InvalidOperation handling.
The existing negative test now checks a deliberately one-row parsed selection.
The affected **complete Debug Common961 and legacy866 suites** were rerun in
fresh sandboxes, along with source16 and focused85/13. These repaired receipts
are the final Debug Common/legacy entries in the table; the first receipts remain
retained as repetitions. Core/Office/WASM/Node APIs used in that run are unchanged
by the new report-only guard. The full Release sequence ran once after the repair.
There were no further production/test changes.

The machine summary counts 586 successful focused executions and retains the
two development test failures from an equivalence fixture whose observed
difference lay outside Core's existing margin precondition. The corrected success
fixture uses .3, with the .15 rejection now explicitly tested. Additional shape
repair focus85/13, full Debug reruns961/866 and semantic capture15 per configuration
are repetitions, not new cases. Early pre-test Windows focus discovered zero
matching cases; it is not counted as qualification. Two compile-scaffold repairs
(test-only Matrix dependency, ambiguous TestResult type) and the source-byte
characterization assumption are recorded; no frozen historical test was changed.

Both full commands correctly return **exit1**, not an all-green claim. Exact
identity/outcome comparison against the retained preceding summary confirms the
**same original 15 Python nonpasses**: three old RefEdit source checks, eight
receipt/namespace provenance issues and the four owner-dispositioned source
provenance contracts, including their repeated loader assertions. In addition,
the unchanged frozen test
`test_04a04_alignment.PairedBlockSourceGates.test_exact_crosswalk_and_authorities`
fails against its old whole-file byte pins. Its current raw-source mismatches are
UiTwoInputRefedits, Ui9ANOVA2nested and Ui0OneRefeditMulticol. The first two are
**untouched in this batch and have no Git diff** following publication/checkout;
the third has the explicitly scoped 04A.05 change. This is consistent with the
previously warned line-ending normalization plus new source evolution; this batch
does not claim to have independently recovered every old raw after-source byte.
It does not rewrite or waive that assertion. New versioned successor checks bind
current hashes, changed methods, unchanged non-target methods and authority rows.
The additional nonpass is disclosed separately from the original15; there are
no other new failure identities or managed numerical failures.

Historical strict/profile exit codes remain 1/1, with 24 old audit failures.
Sealed historical audit writers were not rerun, cleaned, relabeled green or
overwritten. The four accepted 04A.03 source conflicts, old RefEdit checks and
receipt attempts remain visible. UDF declarations are **268 before/after**;
Core math, targets, three designers, importer/geometry, RefEdit, packing/signing,
version attributes, Office UI and `.gitignore` are unchanged.

Reproduction commands (PowerShell, repository root):

```powershell
$py = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
$run = 'TestResults/04A-05-20261102-run01'
& $py -B scripts/verify-04a05.py init --evidence $run
& $py -B scripts/verify-04a05.py focused --evidence $run --name focused-final --windows
& $py -B scripts/verify-04a05.py focused-core --evidence $run --name focused-Core-reference
& $py -B -m unittest discover -s scripts/tests -p test_04a05_contingency.py -v
& $py -B scripts/verify-04a05.py final --evidence $run --configuration Debug --name final-Debug
& $py -B scripts/verify-04a05.py repair-validation --evidence $run --configuration Debug
& $py -B scripts/verify-04a05.py final --evidence $run --configuration Release --name final-Release
& $py -B scripts/verify-04a05.py result-summary --evidence $run
& $py -B scripts/verify-04a05.py preserve --evidence $run --name final
& $py -B scripts/verify-04a05-contract.py check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG apply --check docs/architecture/04A05/ui82x2-wiring-PROPOSED.patch
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
```

Use new run/execution names for reproduction: evidence writers use no-clobber
creation. Full execution uses a hash-bound, diff-recorded copy of the unchanged
`verify-beshstatng.ps1` with -Legacy -Wasm -Js -Python -VisualStudio2026,
the current WinPython and no restore. Real Windows Compile/Copy targets avoid
BeforeBuild generators and AfterBuild Excel-DNA packing/signing. All expanded
commands/exits are retained in `*-command.json` and execution records.

TRX paths below the run root:

- `{Debug,Release}/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-Debug-{Common,Legacy}-repair/results/{Common,Legacy}.trx`
- `executions/final-Release-{Common,Legacy}/results/{Common,Legacy}.trx`
- `projection-executions/final-{Debug,Release}/results/ControlMitigation.trx`
- `executions/final-{Debug,Release}-semantic/results/Common.trx` (15 repetitions)
- `focused-core/focused-Core-reference/Core.trx` (17 existing Core cases)

The 61-case prior mitigation run remains a hash-verified frozen source projection,
not a shipping constructor or native validation. All Common/legacy binaries and
fixtures are copied to deep per-execution sandboxes; their internal writers are
isolated, not merely TRX. Incompatible optional old-batch observation writers are
disabled without changing test assertions; separate semantic capture retains the
candidate comparison inputs. Full/source/repair logs and final-summary.json retain
the sixteen exact Python failure identities and all suite counters/receipt hashes.
No original test was skipped, xfailed, rewritten or promoted.

## Manual contract and remaining work — NOT RUN

No Excel, COM, UIA, XLL, shipping handler, R, installer or signed-in Office native
test ran. Managed/Node WASM/Office tests are contract tests, not native smoke.
Later native work first needs the exact owner authority, independent Windows/COM
risk review, owned PID/HWND equality and loaded-source/XLL attribution.

1. **2×2, after disposition of the withheld patch:** disposable owned workbook
   and dialog each containing {{12,5},{7,19}}; compare all selected table rows,
   exact/mid-p markers, OR/RR/CI at .05/.1, association-without-Chi2, no options
   and Fisher1001. Fractional/missing count input should diagnose before output
   under the newly approved strict criterion. This is currently a proposed change.
2. **RxC:** explicit no-header 3×4 and 2×3 count grids; compare nominal/ordinal
   rows, Somers columns-dependent sign, FFH/fallback and trend transposition.
   Blank/error/fractional cells now stop before output, not silently drop a row.
3. **Proportions:** N100/r45; independent100/45 versus80/52; NI/TOST margin.3 and
   one-sided alpha.05; check signs, CI90%, conclusions and p-value positions.
   Paired100/20/15/40 retains the flagged legacy p-value until scientific approval;
   this is not a correct-discordance acceptance test. Invalid totals/margins stop.

04A.27 retains combined-selection geometry and shared importer/RefEdit/UDF reader
alignment; it is not started. 04A.28 owns later Excel layout/formatting, not the
unresolved 04A.05 scientific decision above. Native04A.45–48 and 04A.06 are not
started. No numerical golden promotion or native-control release is implied.

## Publication preparation and proposed owner disposition

Four tracked files modified: BESHstatNG/BESHStatNG.vbproj,
BESHStatNG.Test/BESHStatNG.Tests.vbproj (one Compile entry each),
BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb and Ui8Proportions.vb. The first form
changes only btCompute_Click/RunRxC; the second only btCompute_Click/
ValidateIndependentMargin. Unchanged non-target methods, constructors, events,
GetResultWriter and overwrite/write tails are checked against actual main.

New normal publication candidates:

- BESHStat.Excel.Common/src/Input/ContingencyInputPolicy.vb
- BESHStat.Excel.Common/src/Results/ContingencyReportPolicy.vb
- BESHStat.Excel.Common/src/Results/ProportionsReportPolicy.vb
- BESHstatNG/src/DM/WindowsContingencyInputAdapter.vb
- BESHStat.Excel.Common.Tests/src/ContingencyLegacyCharacterization.vb
- BESHStat.Excel.Common.Tests/src/ContingencyPolicyTests.vb
- BESHStat.Excel.Common.Tests/src/ProportionsPolicyTests.vb
- BESHStatNG.Test/src/ContingencyAdapterTests.vb
- scripts/verify-04a05.py, scripts/verify-04a05-contract.py
- scripts/tests/test_04a05_contingency.py and this dossier

Two required review artifacts are ignored by `.gitignore:48` and need a later
explicit publication/staging choice: `04A05/source-compatibility-restrictions.json`
and `04A05/ui82x2-wiring-PROPOSED.patch`. Their omission from normal git status is
not omission from this handoff. The three owner-approved local-only 04A.03 JSONs,
04A.04 crosswalk, larger historical ledgers/projection and all old/new TestResults
remain local-only dependencies; a fresh GitHub clone cannot reproduce those
source/evidence assertions by itself. No ignored old evidence was restaged.

**Proposed disposition: PARTIALLY QUALIFIED PENDING EXACT OWNER DISPOSITION.**
Portable count/options and Core-backed report services, real RxC wiring and real
proportions validation/projection are qualified by host-free tests. Ui82x2 is
explicitly not connected and retains its old form statistics; paired numerical
dispatch retains the named Windows compatibility exception. Full 04A.05 acceptance
is not claimed. One owner review should decide the exact case-bound withheld
2×2 delta and the paired scientific convention. Neither decision is a blanket
native release. No stage/commit/merge/push, N/P release or next-batch work occurs.

Final preservation: **PASS**, `TestResults/04A-05-20261102-run01/preservation-final.json`.
All **150,391** historical files retain their full path membership, length,
creation/write nanoseconds and file identity. No metadata/membership drift or
unexpected historical-root members; frozen authority/version/source pins match.
This is a complete metadata/membership proof plus selected source/content hashes,
not a claim that every historical content byte was reread. Initial free space
541,718,896,640 bytes; final 541,156,622,336 bytes (541.2 decimal GB). No retention
cleanup, relocation, compression, overwrite, timestamp rewrite or rebaseline.

Final Git checks: whitespace **exit0**, cached diff **exit0 / index empty**;
branch codex/04A-05; HEAD and main both
849025214223b58a500cd7cceed237a666868ce8. Status is exactly four tracked
modifications and twelve new normal files listed above, plus the two explicitly
disclosed ignored review artifacts. No deleted or unrelated path. No commit,
merge, push, fetch or staging. Shared UibyID141/160 and all broader native
populations remain intact, including the prior mitigation's zero releases.

**04A.05 PARTIALLY QUALIFIED PENDING EXACT OWNER DISPOSITION.** Request one owner
review of this implemented/validated source scope, the hash-bound unapplied
Ui82x2 patch and SC-PROPORTIONS-PAIRED-01. The owner may accept the qualified
portion and explicitly decide those two remaining criteria; this document does
not self-accept them or imply permission to publish or execute native Excel.
P=NO, N1=NO GO, N2=NO, **Gate C2 UNMET**; 04A.06/27/28/45–48 not started.
