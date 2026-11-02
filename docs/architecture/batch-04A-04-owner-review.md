# Batch 04A.04 owner review

Implemented on `codex/04A-04`, from verified clean local main
`e1377cceabbe8890395f94cf41578b621aef3d4c` (also cached origin/main).
Both accepted 04A.03 source implementations and review dossiers are committed
there. The owner explicitly approved retaining the three 04A.03 JSON authorities
as local-only and continuing without committing them. This supersedes only that
administrative publication prerequisite; no numerical/native release follows.
No stage, commit, merge, push, fetch, 04A.05 or 04A.27 is authorized/performed.

## Ownership and compatibility map established before implementation

| Production seam | Existing producer/identity/missing rule | Extraction and compatibility boundary |
|---|---|---|
| `UiTwoInputRefedits.getData`, paired t/Wilcoxon | One combined import, whole-row DataObj filtering; first range determines every area's actual imported start/length; RowIds populated by CoreTable/RemoveMissing; first-column minus second-column in both engines | Windows captures the producer's raw numeric/missing normalization and actual imported row map. Common reuses ParsePaired and selects complete pairs. Original selection geometry is not inferable from the combined matrix; do not claim it is |
| `getDataHotteling`, paired branch | Independently cleaned matrices, same-column check and intersection of physical RowIds; source-order dictionaries subset both matrices. Single/reference and independent branches differ | Typed Common physical-row join with explicit source identity; Windows retains COM and DataObj conversion/subsetting. No ordinal zip, no invented RowIds. Nonpaired/single fitting untouched |
| `Ui0OneRefeditMulticol.getDataMultipleGroups`, Friedman/repeated ANOVA | One wide frame, complete-row filtering; treatment column order; names from first selected cell; no per-treatment filtering | Common complete-block selection over captured typed cells. Other callers retain their old path: unpaired ICC, Skillings–Mack partial blocks, Cochran, RxC/CA/Kite/Mantel–Haenszel |
| `Ui9ANOVA2nested.getData` | Combined group/nested/data import, CharCols=1; whole-row filtering; Core sorts/handles balancing and may mutate its input | Common validates roles and preserves group+nested membership/provenance, including repeated subgroup labels under distinct parents. No relabeling, balancing or change to Core nesting/statistics |
| `getDataPB` | Optional subject/group column plus X,Y; CharCols=1 when present; shared Passing–Bablok/Bland route | Acquisition and Passing–Bablok untouched; Bland option/decision policy validated separately, no forced paired interpretation of categorical subjects |
| Bland decisions and mode/scale choices | Five exact scales, three exact modes; ParseUiDouble uses Excel-number culture, CurrentCulture then invariant, Float+AllowThousands. Disabled decision fields ignored; margins already on active scale | Windows keeps text/culture/Excel separator capture; Common validates numeric limits and semantic projection without conversion or new CI method |
| Alpha/alternatives | Paired t has no exposed alternative/alpha option (hidden global alpha); Wilcoxon shift uses global spin alpha and optional sign test; no tail selector | Explicit typed two-sided/sign convention policy. Do not invent a UI alternative, margin test or reinterpret disabled controls |

Planned concrete files: new Common paired/blocked/nested input policy and option
policy; new pure Windows conversion adapter; the three form code-behind files
and Windows compile entry; focused Common/legacy/source tests plus compile entry;
an isolated 04A.04 verification wrapper and a compact source/native crosswalk.
All statistical engines, UDF signatures, designer files, RefEdit lease code,
output builders/writers and targets remain authoritative and outside refactoring.

Known pre-change ambiguity: ExcelDnaDataImporter.BuildConcreteRangeAddress applies
the first area's start row and row count to every joined area. Separately selected
ranges with different starts cannot be truthfully called aligned to their original
selection after this import. The batch will preserve actual producer coordinates,
test explicit independent-range alignment separately and disclose any blocked
correction rather than silently repairing this legacy acquisition behavior.

Native gates remain P=NO, N1=NO GO, N2=NO, Gate C2 UNMET. All inherited 141/160,
218/239 and 802/1,514 restrictions remain OPEN. No Excel/COM/UIA pilot is run.

## Implementation and production wiring

The preflight found a fast-forward/single-parent publication, not a merge commit:
`e1377cceabbe8890395f94cf41578b621aef3d4c` has parent
`9e2326d4b562cc94660745f1453b231df938bff0`. Local main, cached origin/main,
codex/04A-03 and this branch's starting HEAD all point to e1377cc. Its committed
tree contains both 04A.03 dossiers, Common policy, Windows adapter, tests and
mitigation test project. No network was contacted. Ending commit remains the
starting commit; all 04A.04 changes are working-tree changes.

New portable APIs (namespace `BESHStatNG.ExcelCommon`):

```vb
PairedBlockInputPolicy.ParsePairs(input As SpreadsheetRangeData,
    options As SpreadsheetPairedParseOptions,
    Optional rowIds As IReadOnlyList(Of Integer) = Nothing)
    As SpreadsheetTypedParseResult(Of SpreadsheetPairedNumericInput)
PairedBlockInputPolicy.ParseBlocks(input As SpreadsheetRangeData,
    options As SpreadsheetBlockParseOptions,
    Optional rowIds As IReadOnlyList(Of Integer) = Nothing,
    Optional names As IReadOnlyList(Of String) = Nothing)
    As SpreadsheetTypedParseResult(Of SpreadsheetNumericBlocks)
PairedBlockInputPolicy.AlignRows(first As SpreadsheetRangeData,
    firstRows As IReadOnlyList(Of Integer), second As SpreadsheetRangeData,
    secondRows As IReadOnlyList(Of Integer),
    Optional firstKeys As IReadOnlyList(Of String) = Nothing,
    Optional secondKeys As IReadOnlyList(Of String) = Nothing)
    As SpreadsheetTypedParseResult(Of SpreadsheetRowAlignment)
PairedBlockInputPolicy.ParseNested(input As SpreadsheetRangeData,
    rowIds As IReadOnlyList(Of Integer), Optional missing As
    SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip)
    As SpreadsheetTypedParseResult(Of SpreadsheetNestedInput)
PairedOptionPolicy.Validate(alpha As Double, alternative As PairedAlternative,
    direction As PairedDifferenceDirection) As SpreadsheetValidationReport
PairedOptionPolicy.ValidateLimits(enabled As Boolean, lower As Double,
    upper As Double) As SpreadsheetValidationReport
PairedOptionPolicy.ProjectBland(modeIndex As Integer, scaleIndex As Integer,
    bootstrap As Boolean, bca As Boolean, jackknife As Boolean)
    As Agreement.BlandAltmanOptions
PairedOptionPolicy.ValidateBland(options As Agreement.BlandAltmanOptions,
    x As IReadOnlyList(Of Double), y As IReadOnlyList(Of Double))
    As SpreadsheetValidationReport
```

`ParsePairs` composes the existing `TypedNumericRangeParsers.ParsePaired`; it
does not add another pair parser. `ParseBlocks` evaluates cells with the existing
cell policy and selects entire rows. `ParseNested` composes that block policy
over response roles, retaining hierarchical parent/child labels and repetitions.
It does not balance data or calculate ANOVA. `AlignRows` validates dimensions,
identity and unique physical rows or explicitly supplied subject keys; it returns
one-based selection indices in first-selection order, with unmatched-row warnings.
It cannot perform a cross-product or zip separately compacted arrays.

`SpreadsheetNumericBlocks` copies labels and observations into read-only lists;
its Matrix property returns a fresh array. Nested labels and alignment index lists
are also copied/read-only. Numeric observations carry selected row/column and
physical worksheet row/column. Workbook/worksheet identity belongs to the supplied
snapshot; physical joins require verified, nonempty equal IDs. Identity is not
invented or encoded in a numerical result. The existing paired DTO constructor
and Core statistical contracts are unchanged. Explicit row sidecars override
sequential coordinate assumptions, including gaps and reordered independent input.

The pure Windows `WindowsPairedInputAdapter` exposes `Capture`, `SelectPairs`,
`SelectBlocks`, `SelectNested`, `AlignHotelling` and `Accept`. Capture reads the
actual imported CoreTable normalization, raw labels and real RowIds. Nonnumeric
legacy cells become missing; numeric text follows the producer's CurrentCulture
conversion. This is deliberately distinct from the strict lossless-cell portable
policy. Captured cells are detached. Physical columns remain **zero/unknown**,
because the joined legacy importer does not supply trustworthy per-column source
metadata; its default FirstSourceColumn=1 is not promoted into evidence.

Actual production call chains:

| GUI method | New call and result | Scope boundary |
|---|---|---|
| UiTwoInputRefedits.getData | SelectPairs → ParsePairs → existing TwoGroupsPairedData | Only PairedSingleSampleTTests/WilcoxonSignedRankTest; acquisition and reports unchanged |
| getDataHotteling, optPaired | AlignHotelling → AlignRows → original DataObj subsetting | Verified workbook FullName + qualified sheet, real independently retained RowIds; single/unpaired model selection unchanged |
| btCompute_Click | Validate alpha, two-sided, FirstMinusSecond before acquisition/dispatch | Wilcoxon/Hotelling exposed alpha only; no new t-test alpha/tail selector |
| Ui0OneRefeditMulticol.getDataMultipleGroups | SelectBlocks → ParseBlocks → MultiGroupsPairedData | Friedman/OneWayRepeatedMeasuresANOVA only |
| Ui9ANOVA2nested.getData | SelectNested → ParseNested → original categorical runtime types/matrix | Additional response columns/order preserved; Core retains its existing first-response/statistical interpretation |
| RunBlandAltman | ProjectBland and ValidateBland before Fit/write | Existing mode/scale/CI precedence, alpha, subject options and Core Fit remain authoritative |
| TryGetBlandDecisionLimitInputs | Existing ParseUiDouble → ValidateLimits | No unit conversion; disabled fields still ignored; existing order-error text retained |

Kappa/categorical headers, Deming/pointwise SD, Passing–Bablok, Spearman/Kendall/
Theil–Sen/Lin ingestion, unpaired ICC missing rows, Skillings–Mack partial blocks,
Cochran, CA/RxC/Kite/Mantel–Haenszel, chart construction and all output writers
retain their existing routes. RefEdit connector/lease/event code, constructors,
designers, cancellation/progress, help, output selection and overwrite dialogs
are unchanged. No new feature flag or parallel statistical engine is present.

## Policy and compatibility decisions

| Input/choice | Portable explicit policy | Actual Windows compatibility |
|---|---|---|
| Pair orientation/header | Existing two columns/two rows; None/Required/Auto; source coordinate remapping | Target GUI two-column combined frame; None because legacy import determines names/filtering |
| Blocks | Wide rows=blocks; None/Required text headers; Auto unsupported diagnostic | Complete-row Skip; minimum two columns and one retained block preserve old input selection. Portable default minimum three treatments; numerical method sample/df checks remain Core-owned |
| Incomplete observations | Reject or Skip entire pair/block; warnings carry original row | Blank, errors normalized to missing, nonnumeric, Boolean and DateTime are skipped as legacy importer did; no treatment-wise shifting |
| Numeric-looking text/header | Portable numeric text opt-in with named culture; explicit text header "0" retained | Numeric text already normalized by CoreTable using current culture; a numeric first cell may be both a variable name and data, as before |
| Labels/order | Ordinal case-sensitive keys, first-source order; duplicate block labels retained unless uniqueness requested | No sorting/relabeling/case folding of treatments or factors |
| Independent matrices | Same workbook/sheet physical join; unique row IDs; explicit subject-key join is a different choice | Paired Hotelling uses physical intersection after legacy complete-row filtering; ordinal position is never the join key |
| Nested | Two label roles plus numeric responses; whole-row missing policy; parent+child membership | No relabeling of identical child labels under different parents; repetitions, unbalanced data and all original object values go to unchanged Core |
| Alpha/alternative/direction | Finite 0<alpha<1, valid enums, explicit difference direction | Paired t/Wilcoxon first-minus-second; GUI remains two-sided, optional sign-test and Wilcoxon shift paths unchanged |
| Bland limits | Enabled implies finite lower<=upper; inactive values ignored | Limits already use active raw/percent/log units; ParseUiDouble remains Windows-owned (Excel separators, CurrentCulture, invariant fallback) |
| Bland scales/mode/CI | Five exact scales; Auto/simple/repeated; legacy fallback indices and percentile→BCa→jackknife→analytic precedence | Existing flag values, bootstrap default 2000, X axis/plot options, subject filtering and Core fitting unchanged; no CI compatibility restriction invented |

Stable diagnostics include `alignment.shape`, `.row-ids`, `.identity`, `.columns`,
`.keys`, `.empty`, `.unmatched-first/second`; existing `paired.*` parser codes;
`blocks.options`, `.header`, `.numeric`, `.treatments`, `.duplicate-name`,
`.incomplete`, `.incomplete-skipped`, `.too-few`; `nested.roles`,
`nested.missing-label`; `paired.alpha/alternative/direction`;
`bland.limits.finite/order`, `.shape`, `.scale`, `.mode`, `.ci`, `.subjects`,
`.subjects.required`, `.scale.domain`, `.subject.filtered`, `.pair.filtered`.
Codes do not depend on localization. Validation returns no data on errors.
Windows presents the first error and existing event guards abort before output.
No worksheet write was executed to test this boundary: pure adapter tests and
source-order checks establish it without claiming a native no-write capture.

Two narrowly bounded diagnostic repairs are explicit: Hotelling now checks its
existing bZeroValid guard before dereferencing DataDbl; nonfinite active limits
and malformed typed options receive structured errors before calculation/write.
Valid numerical inputs/options are unchanged. No scientific inference convention
or reference numerical value has been corrected or approved by this work.

## Before/after evidence and remaining acquisition limitations

The versioned working crosswalk is `04A04/compatibility-crosswalk.json`; it pins
the original Git form preimages, current source bytes, authority hashes, real
control IDs and representative fixtures. Preimages also reside under the isolated
run directory. The initial copied verifier wrote the prior historical HEAD literal
into start.json; `preflight-head-correction.json` explicitly corrects that one
field to e1377cc, without rewriting the historical inventory or pretending that
9e2326d is the current baseline.

The Windows adapter tests use real DataObj.DataImportRawMatrix producers and
frozen old selection/engine calls as compatibility oracles. These are managed
characterizations, not native goldens. The paired fixture retains physical rows
21,22,24,25,26 (header row20 and incomplete row23 excluded), values
(1,1),(2,1),(3,2),(4,3),(5,5); existing t=2.4494897427831779,
p=0.070483996910219962. Repeated-ANOVA input retains four complete blocks in
A,B,C order; existing F=15.545454545454524, p=0.0042330297170771669.
Wilcoxon ties/zero differences and Friedman outputs match the unchanged old
engine inputs. Hotelling's independently retained rows [20,22,23] and [21,22]
join to [22], before and after. Nested ANOVA compute-table cells compare exactly;
all five Bland scales compare bias/LoA estimates and all wrapped table cells.
Those comparisons approve no newly generated numerical reference.

The independently selected-range geometry issue remains explicit: combined
import applies the first area's start/length to each area. Captured physical rows
describe the cells **actually read by that producer**, not proof that the second
original RefEdit selection was honored. Correcting acquisition would change
existing results for shifted selections; it is not silently included. The Common
independent-row join is tested for shifted, disjoint, overlapping, reordered,
duplicate/missing physical rows and separate workbook/sheet identities. A later
exact owner decision and 04A.27 reconciliation must address original-selection
equivalence. Common does not reconstruct identity from equal array lengths.

Bland's existing Core filters finite complete pairs and required subject IDs
before scale transformation; the new validator preserves that order. Raw permits
negative/zero measurements; percentage denominators must be nonzero; log-ratio
requires both measurements positive. No transform or statistical formula was
copied. AllowFallbackToSimple is projected unchanged; validation is not a promise
that a tiny/empty repeated dataset will fit—Core's sample-size and model checks
still apply. Existing nested balancing/global-subgroup numerical behavior is also
left to Core; a hierarchical input contract does not certify a numerical redesign.

## Exact native obligations and future integration

The new crosswalk enumerates 21 actual control references. Six have original
deferred control rows, all P0 and still OPEN:

- `control:BESHStatNG.UiTwoInputRefedits.RefEdit1`
- `control:BESHStatNG.UiTwoInputRefedits.RefEdit2`
- `control:BESHStatNG.Ui0OneRefeditMulticol.RefEdit1`
- `control:BESHStatNG.Ui9ANOVA2nested.RefEdit1_Group`
- `control:BESHStatNG.Ui9ANOVA2nested.RefEdit2_nested`
- `control:BESHStatNG.Ui9ANOVA2nested.RefEdit3_Data`

For each, the crosswalk retains original source/decision row hashes, declaration
and codebehind evidence, risk tiers, owner roles, blocked gate, safe-capture and
acceptance prerequisites. The other 15 references have no original control
obligation row; this is recorded as absence, never as release. None of these IDs
belongs to the four accepted transfer cases, so no ANOVA/normality/2x2/PCA case
assignment is fabricated. The six original obligations retain Gate C2/04A.48 and
the requirement for mitigation, executed regression and explicit owner sign-off
before affected live work. Exact transfer pre-change gates are not replaced with
a general exemption: source extraction uses typed managed evidence and does not
rely on any missing native baseline. Acquisition, leases and native controls have
not been exercised or native-qualified.

Recalculated populations remain shared UibyID 141 blocked controls/160 obligations,
four-case 218/239, broader 802 restricted live controls/1,514 original obligations.
No control is released and no original obligation is discharged. N1 remains
NO GO: exact-case owner authority, independent Windows/COM risk review, owned
PID/HWND equality and loaded XLL/assembly/source attribution are still missing.
N2 writes and numerical promotion P remain unauthorized. Gate C2 remains UNMET.

04A.05 can reuse typed coordinate/validation and alignment infrastructure while
retaining categorical level/error/header semantics; numeric block filtering is
not an RxC/2x2 parser. Full 04A.27 follows 04A.04 and 04A.05. Named alignment
cases for its review are `IndependentAlignment_StableFirstOrder_AndExplicitSubjectKeys`,
`Pairs_WholePairsAndNoncontiguousRows`, and the shifted combined-selection
geometry limitation. Existing UDF `TryReadPairedNumericColumns`/
`ExtractCompletePairedNumericRows` in UdfDataImportRangeReaders require equal
array heights, detect headers separately and pair by argument offset, without
physical row identities; GUI acquisition currently reconstructs geometry.
`FitBlandAltmanFromUdfArgs` and UDF mode/scale parsing are further consumers to
reconcile. These are alignment candidates for later review, not a wholesale UDF
migration. Public exports, packaging and numerical engines are unchanged.

## Owner manual smoke — NOT RUN

All require the appropriate later exact native authority and independent safety
review first. No Excel instance was launched, attached or driven in this batch.

1. Paired t-test: disposable owned workbook, the paired fixture above plus one
   incomplete row; existing labels/count5, t and p above; Wilcoxon keeps the same
   pairs and difference sign. Invalid/mismatched input must return a diagnostic
   without creating output. Capture original selected and actually read rows.
2. Friedman and repeated ANOVA: four complete A/B/C blocks plus a partial block;
   expect whole-block removal, unchanged column order/count4, and repeated-ANOVA
   F/p above. Confirm Skillings–Mack remains a separate partial-block workflow.
3. Optional nested/Bland: retain nested parent/child memberships and report order;
   exercise all five scales, enabled/equal/reversed limits and simple/repeated
   choices. Reversed/nonfinite active limits must stop before output. Compare
   existing reports; no new margin conversion or CI method is expected.

Office/WASM validation below is managed/Node contract testing, not signed-in
Office or a native add-in smoke. No stage N attempt, R reference run, golden
promotion, installer, signing or deployment occurred.

## Validation and evidence

The full Debug and Release sequences each ran once after the final production
and test change, sequentially. `scripts/verify-04a04.py` makes a hash-bound,
diff-recorded execution copy of `verify-beshstatng.ps1`, using VS2026 and local
packages with no restore. It compiles the real Windows project through
PrepareForBuild/PrepareResources/Compile/CopyFilesToOutputDirectory and builds
legacy tests with BuildProjectReferences=false, avoiding generation, Excel-DNA
packing and signing targets. The original verifier is unchanged.

Common and legacy assemblies, fixture/data directories and test-internal writers
run in deep per-execution sandboxes. Frozen C02/C03 optional observation writers
whose guards prohibit a new batch directory are disabled without changing their
assertions. The 15 semantic cases run separately to supply the existing Common
Python candidate comparisons; these are repetitions, not extra distinct cases.
The accepted 61-control mitigation tests rerun against their hash-verified frozen
projection in a fresh output/obj directory, explicitly still a source projection,
not the shipping constructor or native controls. Discovery and executed/pass
counts are checked, so a zero-test exit cannot qualify it.

| Suite | Debug | Release |
|---|---:|---:|
| Core MSTest | 1,024 pass | 1,024 pass |
| Common MSTest | 901 pass | 901 pass |
| Windows legacy MSTest | 853 pass | 853 pass |
| Office managed contracts | 505 pass | 505 pass |
| WASM execution managed contracts | 45 pass | 45 pass |
| Existing mitigation projection | 61 pass | 61 pass |
| **Distinct managed tests per configuration** | **3,389 pass** | **3,389 pass** |
| Node (468 + 71 + 377) | **916 pass** | **916 pass** |
| Python (27 + 16 + 387 + 168) | **583/598 pass** | **583/598 pass** |

No managed/Node failures or skips. New tests: 49 Common + 15 Windows = 64 managed
cases, and 14 Python successor gates. Final focused execution was 74 Common
(49 new plus 25 existing numeric parser cases), 15 Windows and 14 Python passes.
Earlier successful focused managed executions total 399 repetitions, separate
from the 6,778 final Debug/Release managed executions and 30 semantic repetitions.
Focused compilation attempts retained in evidence exposed and fixed three test
scaffolding mistakes (missing Agreement import, WilcoxonTest type name and
BiasCI.Estimate member). A new source test initially assumed Git/worktree byte
line endings were identical; it now compares unchanged designer lines. No old
assertion or fixture was weakened. The final source/production tests pass.

Both full verification commands deliberately retain exit **1** because of the
same 15 historical Python nonpasses, compared by exact identity and outcome to
`TestResults/04A-03-controls-20261102-run01/final-summary.json`. They comprise the
three old RefEdit source checks, eight receipt/namespace provenance issues and
four owner-dispositioned source-provenance contracts (some loaders surface the
same conflict through multiple assertions). No new failing identity appeared.
The three newly modified form files also have new source bytes: their Git
preimages and versioned successor source checks document the change without
rewriting a sealed old whole-file expectation. This does not retrospectively
make the old audit green. Historical strict/profile exit codes remain 1/1 with 24 old audit
failures; those sealed historical writers were not rerun or overwritten.

Reproducible commands (PowerShell, repository root):

```powershell
$py = 'C:\Users\peter\Desktop\WPy64-31470\python\python.exe'
$run = 'TestResults/04A-04-20261102-run01'
& $py -B scripts/verify-04a04.py init --evidence $run
& $py -B scripts/verify-04a04.py focused --evidence $run --name focused-06 --windows
& $py -B -m unittest discover -s scripts/tests -p test_04a04_alignment.py -v
& $py -B scripts/verify-04a04.py final --evidence $run --configuration Debug --name final-Debug
& $py -B scripts/verify-04a04.py final --evidence $run --configuration Release --name final-Release
& $py -B scripts/verify-04a04.py result-summary --evidence $run
& $py -B scripts/verify-04a04.py preserve --evidence $run --name final
& $py -B scripts/verify-04a04-contract.py check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --check
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG diff --cached --exit-code
git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG status --short --untracked-files=all
```

Evidence commands use exclusive new destinations; use a fresh name/run for a
later reproduction. Earlier focused names 01–05 and their build/test logs remain.
`final-{Debug,Release}-verify-isolated.provenance.json` records the exact original
and execution verifier hashes/diffs. All expanded commands and exit codes are in
the associated `*-command.json` files. Primary TRX paths under the run root:

- `{Debug,Release}/results/{Core,OfficeAddin,OfficeExecution}.trx`
- `executions/final-{Debug,Release}-Common/results/Common.trx`
- `executions/final-{Debug,Release}-Legacy/results/Legacy.trx`
- `projection-executions/final-{Debug,Release}/results/ControlMitigation.trx`
- `executions/final-{Debug,Release}-semantic/results/Common.trx` (15 repetitions)

`final-summary.json` contains individual suite counters/hashes and all 15 exact
Python nonpasses; full and Common-Python logs retain original tracebacks. The
existing benign non-public MatrixTestHelpers discovery warning and legacy
unassigned-variable warnings remain visible. Contract checks confirm no forbidden
Core/Common dependency, no changed UDF/packing source, and 268 UDF declarations
both before and after. All three designers are unchanged after line-ending-aware
Git comparison. Accepted 04A.03 source/authority pins: 18/20 unchanged; the only
two expected differences are the host/test project files' new Compile entries.

## Publication inventory and owner decision

Proposed source/document publication list, **not staged**:

| Paths | Purpose |
|---|---|
| BESHStat.Excel.Common/src/Input/PairedBlockInputPolicy.vb | New typed pairs/blocks/independent alignment/nested policy and immutable results |
| BESHStat.Excel.Common/src/Input/PairedOptionPolicy.vb | New alpha/direction/limits and Bland option validation/projection |
| BESHstatNG/src/DM/WindowsPairedInputAdapter.vb | Pure real-producer conversion bridge |
| BESHstatNG/src/UI/UiTwoInputRefedits.vb | Paired numeric/Hotelling/alpha delegation and existing empty-input guard ordering |
| BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb | Friedman/repeated ANOVA block delegation only |
| BESHstatNG/src/UI/Ui9ANOVA2nested.vb | Nested and Bland option/limit delegation |
| BESHstatNG/BESHStatNG.vbproj | Adapter Compile entry |
| BESHStatNG.Test/BESHStatNG.Tests.vbproj | New adapter test Compile entry |
| BESHStat.Excel.Common.Tests/src/PairedBlockPolicyTests.vb | 49 new typed-policy/culture/provenance/negative cases |
| BESHStatNG.Test/src/PairedBlockAdapterTests.vb | 15 new actual producer/adapter/Core compatibility cases |
| scripts/tests/test_04a04_alignment.py | 14 executable successor source/identity/native-gate checks |
| scripts/verify-04a04.py | Isolated focus/full execution, preservation and result summary |
| scripts/verify-04a04-contract.py | Versioned additive exact-ID/source/fixture crosswalk and checks |
| docs/architecture/batch-04A-04-owner-review.md | This consolidated dossier |
| docs/architecture/04A04/compatibility-crosswalk.json | Compact machine-readable ownership, provenance and compatibility fixture matrix |

The last JSON is ignored by `.gitignore:48` and requires an explicit later
publication/staging decision if it is to travel with GitHub source. No production
VB or test source is ignored. All TestResults evidence is retained local-only;
the crosswalk/checks and rerun of prior mitigation cases also depend on preserved
local historical authorities/projection. A fresh clone alone cannot reproduce
those assertions. The owner's three-JSON 04A.03 local-only decision remains in
force for pre-change-crosswalk.json, successor-candidates.json and
native-readonly-plan.json; their hashes are unchanged and pinned in the successor
crosswalk. No historical CSV/index or ignored evidence was restaged.

Final protection result: **PASS**, `TestResults/04A-04-20261102-run01/preservation-final.json`.
All **147,980** historical files retained complete path membership, byte length,
creation/write nanoseconds and file identity, with zero drift or unexpected
historical-root members. Frozen source pins also match. This is a complete
metadata/membership check plus selected authority hashes, not a claim of
rereading every historical byte. Initial free space 542,279,340,032 bytes;
final 541,890,297,856 bytes (541.9 decimal GB), comfortably above warning/stop
thresholds. No cleanup, relocation, compression, timestamp rewrite or rebaseline.

Final Git checks: `diff --check` **exit0**, `diff --cached --exit-code` **exit0**,
branch `codex/04A-04`, HEAD=main=e1377cceabbe8890395f94cf41578b621aef3d4c.
Status is exactly five modified tracked files (the two project files and three
form code-behind files) plus nine untracked source/test/script/dossier files
listed above; the new crosswalk JSON is the separately disclosed ignored file.
Git's CRLF-to-LF advisory on two form files is not a whitespace error. No unrelated
source, sealed authority, baseline, designer, UDF, framework target or historical
evidence changed. No staging, commit, merge, push or fetch occurred.

**04A.04 COMPLETE FOR OWNER REVIEW.** The required portable policies have actual
Windows production call chains and passing managed/contract regressions. Native
parity is not asserted. Owner acceptance is one review of this extraction, its
explicit diagnostic repairs and retained acquisition ambiguity; it does not
authorize Git publication, numerical promotion or native release. All native
obligations remain OPEN, P=NO, N1=NO GO, N2=NO, **Gate C2 UNMET**.
04A.05 and 04A.27 have not started. Requested next action: one owner acceptance
decision on this dossier and proposed source scope, retaining the named
original-selection geometry issue for a separately authorized later correction.
