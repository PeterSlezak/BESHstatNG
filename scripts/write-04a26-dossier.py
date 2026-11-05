"""Single owner dossier from reconciled 04A.26 evidence; never publishes Git."""
from collections import Counter
import importlib.util
import json
from pathlib import Path
import platform
import subprocess
import sys

ROOT=Path(__file__).resolve().parents[1]
s=importlib.util.spec_from_file_location('capture26_dossier',ROOT/'scripts/capture-04a26-contract.py');c=importlib.util.module_from_spec(s);s.loader.exec_module(c)
def doc(name):return json.loads((c.OUT/name).read_bytes())
def ratio(d):return f"{d['passed']:,}/{d['total']:,}"
def main():
    validation=doc('validation.json')['configurations'];entry=doc('entry-validation.json')['configurations']
    inventory=doc('new-test-inventory.json');corpus=doc('sample-agreement-same-core-v1.json');parallel=doc('parallel-validation-audit.json')
    qualification=doc('final-source-qualification.json');retention=doc('retention.json');gates=doc('gate-validation.json');timing=doc('end-to-end-timing.json')
    blockers=[]
    for cfg,d in validation.items():
        if d['new_logical_passed']!=d['new_logical_total']:blockers.append(cfg+' new managed test nonpass')
        if d['lanes']['errors']:blockers.append(cfg+' incomplete lane')
        if not d['prior_managed_inventory_exact']:blockers.append(cfg+' prior inventory drift')
        if d['python']['reused_not_run'] or d['node_passed']!=d['node_total']:blockers.append(cfg+' incomplete Python/Node execution')
    if any(e['outcome']!='PASS' for r in inventory['python_cases'].values() for e in r['executions']):blockers.append('new Python nonpass')
    if gates['serial_commands']['mkdocs']['exit_code']:blockers.append('strict MkDocs failed')
    if not retention['membership_exact'] or not qualification['index_empty']:blockers.append('retention/index audit failed')
    decision='NOT_READY' if blockers else 'READY_FOR_OWNER_04A26_ACCEPTANCE_REVIEW'
    rows=[];lanes=[]
    for cfg,d in validation.items():
        py=d['python'];rows.append(f"| {cfg} | {ratio(entry[cfg]['logical_cases'])} (reused) | {ratio(d['logical_cases'])} | {ratio(d['runtime_executions'])} | {d['node_passed']}/{d['node_total']} | {py['passed']}/{py['total']} ({py['failure_methods']} FAIL / {py['error_methods']} ERROR) |")
        lane={r['lane']:r['seconds'] for r in parallel['configurations'][cfg]['lanes']}
        lanes.append(f"| {cfg} | {lane['A']:.2f} | {lane['B']:.2f} | {lane['C']:.2f} | {lane['D']:.2f} | {parallel['configurations'][cfg]['seconds']:.2f} |")
    changed='\n'.join('| `'+p+'` | '+', '.join(names)+' |' for p,names in qualification['method_deltas'].items())
    managed=[]
    for suite in validation['Debug']['managed_suites']:
        for r in suite['nonpasses']:
            message=r.get('message','').replace('\r',' ').replace('\n',' ')[:210].replace('|','/')
            managed.append('| `'+r['class_name'].split('.')[-1]+'.'+r['test_name'].replace('|','/')+'` | '+message+' |')
    pyrows='\n'.join('| `'+name.replace('|','/')+'` | '+outcome+' |' for name,outcome in sorted(validation['Debug']['python']['nonpasses'].items()))
    triggers='\n'.join('- `'+cfg+': '+str(row)+'`' for cfg,d in validation.items() for row in d['new_managed_nonpasses']) or 'None.'
    newpy='\n'.join('- `'+cfg+': '+name+'` — '+outcome for cfg,d in validation.items() for name,outcome in d['new_python_nonpasses'].items()) or 'None.'
    protected=doc('original-control-crosswalk.json');populations='\n'.join('| `'+name+'` | '+str(len(r['control_ids']))+' | '+str(len(r['obligation_ids']))+' |' for name,r in protected['populations'].items())
    historical=doc('historical-hash-reconciliation.json');classifications=Counter(r['classification'] for r in historical['records'])
    profiling=doc('python-module-profile.json')
    profile_rows='\n'.join('- `'+name+'`: '+str(row['executed'])+' repeated methods, '+f"{row['seconds']:.2f}"+' s; '+str(row['fail'])+' FAIL / '+str(row['error'])+' ERROR.' for name,row in profiling['modules'].items())
    fixture_sources='\n'.join('- `'+p+'`: `'+h+'`' for p,h in corpus['fixtures'].items())
    candidate_rows='\n'.join('- `'+r['id']+'`: '+r['actual']+' Proposed later owner: '+r['owner']+'. Activation: '+r['activation_gate'] for r in doc('candidate-issues.json')['issues'])
    c.emit('toolchain.json',dict(python=dict(executable='<PYTHON_EXE>',version=platform.python_version(),sha256=c.sha(Path(sys.executable))),dotnet_sdk=subprocess.check_output(['dotnet','--version'],cwd=ROOT,text=True).strip(),node=subprocess.check_output(['node','--version'],cwd=ROOT,text=True).strip(),mkdocs=subprocess.check_output([sys.executable,'-B','-m','mkdocs','--version'],cwd=ROOT,text=True).strip(),visual_studio='Installed VS2026 MSBuild 18 / VSTest; exact private commands in command-outcomes.json with public tool aliases.',targets=dict(Core='netstandard2.0',Common='netstandard2.0',Windows='net48',managed_portable_tests='net10.0'),network_packages_installers_signing='NOT_RUN / UNAUTHORIZED'))
    c.emit('owner-decision.json',dict(decision=decision,blockers=blockers,proposed_scope='Accept 04A.26 software extraction only; retain every original native/scientific restriction, inherited/frozen failures and dormant candidate. No Git publication or later batch authorization.',entry_managed='NOT_RUN_REUSED_EVIDENCE / all 801 inputs qualified, 799 exact and two LF/CRLF only; raw receipts independently SHA-verified.',entry_release_python='NOT_RUN; final full Python executed per configuration.',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET')))
    text=f'''# Official 04A.26 owner review

## 1. Decision

`{decision}`

{'; '.join(blockers) if blockers else 'Software extraction is ready for scoped owner review. This decision does not approve statistical/native gates, Git publication or another batch.'}

All eight sizing families and five target agreement handlers consume shipping Common policies. Core formulas, rounding, CI algorithms and registered UDF contracts are unchanged. Native Excel/COM/RefEdit/UIA, Office/Mac/browser and independent R were **NOT_RUN / UNAUTHORIZED**. No independent numerical golden or chart appearance is claimed.

## 2. Actual Git authority and owner edits

Verified merged `main`, feature HEAD and entry/base: `{c.BASE}`. Branch: `codex/04A-26`. Local main reflog records `97d5461 merge codex/04A-25: Fast-forward`; the accepted 04A.25 branch is an ancestor at that commit. The owner reports formal acceptance; Git corroborates merge/ancestry and the accepted dossier, rather than independently proving a human decision. The pre-merge archive and former `bd46904...` entry are not substituted for current main.

Entry worktree/index were empty. During this run the owner revised the Master Plan from 2.9 to 2.10. That existing concurrent edit is preserved, hashed separately and **excluded from publication candidates**; no agent plan write occurred. Revision 2.10 retains the exact 04A.26 row and documents 04A.26 as in progress. Initial/final status, ancestry, all source pins and the excluded edit hashes are in `evidence/04A26/entry-state.json`, `entry-source-qualification.json` and `final-source-qualification.json`.

The staged index remains empty. Nothing was staged, committed, merged, pushed, reset, rebased, stashed or cleaned. The only Git mutation was creation of the authorized feature branch, after the sandbox denied writing its ref and explicit tool approval allowed that operation. No historical file was repaired to a former archive hash.

## 3. Shipping ownership and changed symbols

| Windows source | Changed existing members (normalized member-body audit) |
|---|---|
{changed}

The Ui12 constructor also consumes portable paired-t/single-proportion numeric defaults; native layout helpers and their toggling chronology remain unchanged. The test project adds six linked Common files and one adapter file; original ProjectReference XML is unchanged. Entry BOM/LF/terminal-newline conventions were restored on modified files, with qualified Windows rebuild/retest and strict MkDocs on final Help bytes; `encoding-audit.json` records exact counts. No historical tree normalization occurred. Eight new portable files under `BESHStat.Excel.Common/src/Analysis/` are `SampleSizePlanningRequest`, `SampleSizePlanningPolicy`, `SampleSizeTextProjection`, `SampleSizePlanningDescriptions`, `SampleSizePlanningDefaults`, `AgreementInputAxis`, `AgreementOptionSelections` and `AgreementReportPlan`. All use `Option Strict On` and contain no Excel/WinForms/COM/filesystem/Microsoft.VisualBasic dependency.

Actual call graph:

```text
Ui12.btCompute_Click (original title dispatch and CheckData)
  -> RunPTtest / RunUPTtest / RunSingleProp / RunIndependentProp
     / RunLogRank / RunCox / RunICC / RunBlandAltman
  -> Windows parsing and selected-branch SampleSizePlanningRequest
  -> Common SampleSizePlanningPolicy.Calculate (one selected Core call)
  -> original Core SampleSizeCalculator.Calculate... API
  -> Common SampleSizeTextProjection -> immutable report.Text
  -> original tbOutput.AppendText
  -> original btnSaveToSheet_Click: current Lines, new sheet, title first

UiTwo.getData/getPairedCategoricalData (original import/conversion)
  -> detached source-row/category axis
UiTwo.RunDeming / RunLinsCCC / RunCohensKappa
  -> Common option snapshots and original CI/variance execution intent
  -> existing Core constructor, original selected Fit/analytical/jackknife call
  -> original wrapResults -> Common AgreementReportPlan -> fresh table list
  -> original GetResultWriter / ProcessListofResultTables / writeToSheet
  -> original native plot only for Deming/Lin

Ui9.RunBlandAltman
  -> original ProjectBland / Common BlandRequestSelection / original ValidateBland
  -> converted observation/subject/source axis -> original Core Fit once
  -> original optional bias/LoA assessments and native table builders
  -> Common decision-table order / report span / plot flag
  -> original Excel writer and Bland plot

Ui0.RunICC
  -> original import branch / accepted paired block evidence
  -> Common IccRequestIntent + AgreementInputAxis
  -> original six ICC... and conditional repeatability methods, same order
  -> original wrapResults label -> Common report span -> original writer
```

Source-row metadata is stored in form-owned weak tables against imported DTOs; no Core result or importer schema changes. Unknown physical rows remain nullable, rather than manufactured ordinals. Subject keys, categorical values, physical rows and observation positions remain separate. `SpreadsheetRowAlignment` and accepted paired blocks are reused; numeric/categorical conversion, missing filtering and first-row interpretation stay native. ICC one-way jagged measurement arrays and two-way matrices retain original shapes and column ordering.

Deliberately host-owned/untransferred portions are control visibility/title/help dispatch, culture/Excel conversion, messages/focus and rejection timing, RefEdit/SD acquisition, decision-limit table formatting, overwrite/area/destination checks, native progress/errors, actual text append/save and plot geometry. These are unchanged boundary responsibilities, not a claim of portable GUI/Excel execution. Agreement engines stay in Core; Common does not reproduce estimators or tables.

## 4. Before/after behavior matrix

| Route/intent | Original behavior retained by Common/native composition |
|---|---|
| Paired t | Entered difference/SD/alpha/beta; original number of pairs and text order; no new rounding. |
| Unpaired t | Superiority, NI and EQ stay separate Core APIs; absolute margin converted to original signed bounds; control/experimental allocations, bound outputs and driving bound unchanged. |
| Single proportion | Entered alternative/null proportions, corrected/uncorrected sizing and original report labels remain. |
| Independent proportions | Three hypotheses, group order, kappa, signed margins, correction/Fisher/chi-square text preserved. Constructor versus toggled default orientation retained and recorded as a dormant candidate. |
| Log-rank | HR, group event proportions, allocation, alpha/beta and two-sided=True unchanged; C13 scientific issue remains unapplied. |
| Cox | Binary versus continuous Core calls; optional overall event proportion NaN versus explicit zero, R-squared absent=0; original “not provided”/“not estimated”, log HR/events/effective variance and subject estimates retained. |
| ICC planning | Null/alternative, one-sided alpha, original integer guard then CInt, subject count/observations/achieved power preserved. Nonfinite count issue characterized, not fixed. |
| Bland–Altman planning | SD, desired CI half-width, LoA multiplier and alpha; hidden beta remains NaN; original achieved precision/count preserved. |
| All sizing reports | Same current-culture interpolation, punctuation, field order and explicit CRLF; byte-exact original text derivative checked in invariant/sk-SK. Append and one-column new-sheet save methods remain unchanged. |
| Deming | GUI index1=constant CV, index2=pointwise SD, default=constant lambda (Core enum ordering differs). Original SD imports/messages/length validation and fit-intercept retained. Percentile > BCa > analytical > jackknife; MCR wins within analytical when Linnet checked. Existing analytical/MCR/jackknife/Fit invocation retained once. |
| Lin CCC | Analytical > percentile > BCa > analytical fallback; null concordance/alpha/replicates and conditional native reporter preserved; plot requested. |
| Kappa | Five GUI weights and original analytical > jackknife > percentile > BCa > analytical fallback; text/Unicode/blank categories and category order retained; resampling reporter retained; no plot. |
| ICC estimation | All ICC(1,1), ICC(1,k), ICC(2,1), ICC(2,k), ICC(3,1), ICC(3,k) branches; first-checked precedence, six original methods, repeatability order and exact k label interpolation from measurement columns retained. No plot. |
| Bland–Altman agreement | Original two/three-column conversion, subject identity, Auto/repeated/simple, raw/percent/log scale, X-axis/plot choice, proportional bias, original validation/fallback and <=0 replicates→2000 preserved. Bias table precedes LoA table only when enabled. |
| Output layout | Deming/Lin/Kappa/ICC `tableRows + sections - 1`; Bland `tableRows + sections + sections - 1`. Fresh lists preserve original Core table/footnote order. All writer destinations, occupied-area prompts and Excel lifetime remain native. |

`SampleSizePlanningDefaults` describes fresh numeric defaults without changing disabled/retained control text. Dynamic layouts remain native to preserve event side effects. Alpha/beta/hypothesis roles are detached; beta is not reinterpreted as power. No new validation is activated: Common diagnostics are candidate-only. Core exceptions and original GUI guards remain authoritative at their former boundaries.

Live assurance search found **no separate assurance solver or GUI**. ICC achieved power and BA achieved LoA half-width are result metadata under entered assumptions; no assurance algorithm, guarantee or public solver was invented.

## 5. Same-Core compatibility and reference limits

Corpus: **{corpus['record_count']} executed records / {corpus['payload_count']} distinct content-addressed payloads**. Classifications: `{corpus['classifications']}`; family execution counts: `{corpus['families']}`. Every record is `NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY`. Profile/test hashes:

{fixture_sources}

The 208 sizing comparisons per runtime cover thirteen branches × eight parameter/rejection profiles × two cultures. Eighty agreement comparisons per runtime cover five methods × eight profiles × two cultures. Original option/dispatch/text derivatives are compared with the new projection using the same unchanged Core APIs, exact typed result hashes, exact paragraphs and result-table content/order. Agreement payloads include full typed input/subject/source axes, effective options, computed result values and existing ToOutputModel cells, including nonfinite IEEE representations. ICC uses its actual five-subject, three-measurement fixture axis. Invalid Core cases compare exception class/message and remain rejections. There is no fabricated effect or zero-result success.

Existing seed parameters use 12345 in tests only. Native Core default `Integer.MinValue` and bootstrap behavior are unchanged; no unsupported cross-run CI equivalence is inferred. Fixture comparisons are synthetic and bounded, not independent R/native goldens. Full historical Core and plot-payload tests supplement new cases. Output appearance, placement, real worksheet coercion and clinical/scientific validity still require the original owner-native/reference gates.

Payloads and all per-case configuration/runtime/culture/options/source-row/subject/result metadata are indexed in `sample-agreement-same-core-v1.json` and its SHA buckets. Source-only GUI guard/append/save/SD-range characterizations are not mislabeled as native executions. Historic PSM **{historical['retained_psm_records']} records / {historical['retained_psm_payloads']} payloads** and **{historical['mixed_model_original_captures']} original mixed-model captures** remain hash-verified and unchanged.

## 6. Entry versus final validation and nonpasses

| Configuration | Entry logical reference | Final logical managed | Final raw managed | Node | Executed Python |
|---|---:|---:|---:|---:|---:|
{chr(10).join(rows)}

Entry managed/Node identities are **NOT_RUN_REUSED_EVIDENCE**, qualified against all 801 actual-main compiled/test input members, 799 exact SHA and two demonstrated LF/CRLF-only PSM source differences, with accepted raw TRX and Node receipt hashes verified. Fresh main focused builds/tests also ran. Full Debug entry Python was freshly measured **807/910 (93 FAIL / 10 ERROR)**; a separate Release entry Python execution is **NOT_RUN**, not an asserted second pass. This explicit entry-reference qualification minimizes unnecessary baseline whole-suite reruns and does not omit either final configuration.

New unique logical managed: **{inventory['logical_count']}**, included in final totals (linked Common source counted once logically). New Python: **{inventory['python_count']}**, all pass in both final configurations. Every final Python method was executed for each configuration; **zero final verdicts reused**. Raw framework executions, logical shared identities and separate semantic-only probe repetitions are not added together. Complete identities, traces and private receipt SHA references are in `entry-final-test-identities.json`, all final logical/raw identity files, Python trace files and `nonpasses.json`.

New managed nonpasses against entry:

{triggers}

New Python nonpasses against freshly measured entry:

{newpy}

Frozen prior tests are unchanged. Literal-source tests may now fail because an authorized wrapper moved, or because the owner plan/hash/publication authority advanced; these remain **real failed tests**. New IL adapter tests prove actual shipping Common request/option/axis/report calls; new Python successors prove preserved validation/save/import/native boundary member hashes. No failed source pin is rewritten green. Earlier nonpasses remain visible; this is not an all-green suite.

The measured Debug additions are three protected owner-plan hash assertions in 04A.23, 04A.24 and 04A.25 tests, triggered by the owner's concurrent Revision 2.10 update. They are retained as failures, with the owner edit excluded from publication. No new managed failure was introduced by the extraction; no extraction source-pin failure is invented to explain a historical nonpass.

Debug managed raw nonpass identities and assertion excerpts (Release exact identities are independently retained in JSON):

| Identity | Actual assertion excerpt |
|---|---|
{chr(10).join(managed)}

Debug Python method identities (individual FAIL/ERROR traces and corresponding Release identities in the machine-readable artifacts):

| Identity | Outcome |
|---|---|
{pyrows}

## 7. Four-lane execution and timing

| Configuration | A Core/Common seconds | B Windows seconds | C Office/Node seconds | D Python/source seconds | Matrix wall seconds |
|---|---:|---:|---:|---:|---:|
{chr(10).join(lanes)}

Final matrices total **{parallel['matrix_seconds']:.2f} s**; final builds **{parallel['final_build_seconds']:.2f} s**; serial strict/profile/MkDocs **{parallel['serial_gate_seconds']:.2f} s**; requalification **{parallel['requalification_seconds']:.2f} s**. Entry-freeze through terminal audit measured **{timing['seconds']:.2f} s**, including development and setup/debug attempts; initial interactive authority/Git preflight and later dossier/manifest sealing are explicitly outside that interval. Final sealing timing is recorded separately. No measured sequential baseline exists and no speedup is claimed.

Lane D starts first. Each root uses copied prebuilt binaries, isolated TRX/logs/exports and project-relative TestData. No concurrent builds across configurations; no testhost Excel activation. MaxCpuCount=1, MSTest Workers=1, Node concurrency=1, at most four runner command roots; OS root PID/CPU/working-set samples are retained, not misrepresented as complete child-process memory measurements. All lanes are awaited despite inherited failures. No collision or historical write was found.

Python dependency/side-effect matrix records source hashes and conservative module classifications. Source/inventory validators remain unmodified and fully executed twice. Immutable completed TRX parsing/hash lookup is cached only in accounting with a terminal SHA reread. No Python method verdict, historical source scan or test inventory is skipped by caching. No fifth worker, unsafe discovery split or alternating-batch policy was activated.

Bounded, serial post-matrix profiling measures three representative historical source/corpus modules and two fresh immutable-registry hash scans; these profiling repetitions do not inflate final totals. Other modules have root-command timings only. Method identities, outcomes and scan byte counts/durations are in `python-module-profile.json`:

{profile_rows}

The original Release matrix additionally recorded two privacy-test failures from a slash-separated prose phrase in the new dossier helper, matching the frozen home-path pattern. The prose was corrected without relaxing the scanner or frozen tests. Corpus review then strengthened two new fixture/reference files: valid 200-replicate positive profiles supplement genuine Core rejections, ICC provenance matches the five-subject matrix, and full typed numerical snapshots are emitted. Affected full Common/Windows assemblies and scripts discovery were requalified in both configurations. Entry encodings were restored and Windows Debug was separately rebuilt/retested. Initial totals, source hashes and receipts remain in `initial-full-matrix-validation.json`, `initial-profile-coverage.json` and `python-requalification.json`; every intermediate attempt remains private. Repetitions never inflate distinct final totals. Product semantics, Core and frozen tests did not change during this requalification.

Setup receipts are preserved: zero-test filter, import-before-Option, VB request shadowing, ProjectReference XML insertion, Unicode reference decoding and comparison-type hash mistakes were corrected before final build. Final source hashes equal both compiled build manifests. Evidence-only post-matrix updates require fresh audits/sealing, rather than rerunning unchanged managed binaries.

## 8. Original restrictions, retention and dormant candidates

| Original host | Control records | Linked OPEN obligations (overlapping) |
|---|---:|---:|
{populations}

Actual combined host population is **{protected['counts']['host_controls']} controls / {protected['counts']['linked_open_obligations']} linked OPEN obligations**. These host-wide records include unrelated modes and overlap with native/source obligations. They are not added to overall original **3,866 source members / 2,056 controls / 1,514 obligations**, nor to 04A.27's **44 OPEN importer obligations**. Exact IDs/original hashes and 04A.02 blocked-control overlap are in `original-control-crosswalk.json` and `restriction-retention.json`.

Historical results: **{retention['historical_result_files']} files**, exact membership and file identity/size/creation-write metadata retained. This is explicitly a metadata preservation check, not a claim of rereading every large TestResults byte. **{retention['frozen_authority_files']} authority/corpus hashes** were separately verified, with **{retention['byte_exact_count']} entry-exact files** and only the excluded owner Master Plan revision differing. Previous 04A.25 registry reconciliation: `{dict(classifications)}`; differences are recorded without altering history. The original 100 mixed-model hashes and prior PSM bucket hashes are independently checked.

Dormant candidates, all **OPEN / CANDIDATE_ONLY / UNAPPLIED**:

{candidate_rows}

All prior C09/C11/C12 patches, C13 log-rank, C17–C25 issues, C21 stacked row validation and C24 historical oracle scope remain unchanged/unapplied. **Zero native controls released; zero obligations discharged. P=NO; N1=NO GO; N2=NO; Gate C2=UNMET.** Other E/GC/C3 restrictions remain under their original authority.

Privacy screening covers every publication candidate including decoded JSON/base64 and private owner, host, home, drive, SID and credential patterns. Full unredacted logs/TRX stay private under `TestResults/04A-26/run01`. Public paths use `<REPO_ROOT>`, `<PYTHON_EXE>` and tool aliases. `publication-manifest.json` seals every candidate SHA-256/size/mode except its explicitly omitted self-hash; the owner plan is separately accounted for. Trackability, empty index, diff-check and preservation are independently verified by the new publication helper. Exact terminal candidate/hash count is in that manifest and console receipt; no ignored history is forced into publication.

## 9. Help, gates and excluded execution

All eight sizing pages, Bland–Altman, Deming, ICC, Kappa and Lin-concordance pages plus authored `_meta/methods.yml` were audited. Only ICC/BA sizing pages gain one clarification each: achieved power/half-width is existing result metadata and there is no separate assurance solver. Authored metadata, links, screenshots and workbook assets remain unchanged; original assets are hash-listed in `documentation-audit.json`.

Actual serial exits: strict **{gates['serial_commands']['strict']['exit_code']}**, profile **{gates['serial_commands']['profile']['exit_code']}**, strict MkDocs **{gates['serial_commands']['mkdocs']['exit_code']}**. Strict/profile nonzero results remain UNMET; raw stdout/stderr hashes and command identities are retained. MkDocs uses final authored Help bytes and isolated `--site-dir`. Exact installed versions and target frameworks are in `toolchain.json`.

No working Office.js/WASM sample-size/agreement panel is implemented or claimed. No native Excel/COM/RefEdit/UIA smoke, authenticated Office/Mac/browser run, workbook/chart capture, R oracle, installer/signing or release occurred. Representative future owner-native smoke remains paired sample-size (repeat append/save) and Bland–Altman (subject layout, optional decision tables and spacing/plot). These are deferred checks, not newly authorized runs. Core mathematics, chart geometry (later 04A.35), UDF contracts (later 04A.39–42), unrelated UI, accepted 04A.27 transport and frozen references are excluded.

## 10. Proposed scoped owner decision

Accept **04A.26 software extraction only**, subject to independent review of this dossier and manifest. Retain every inherited/protected nonpass, dormant correction and native/scientific gate. This proposed decision does **not** authorize Git staging/commit/merge/push, numerical-golden promotion, native-control release, obligation discharge or another numbered batch. Owner acceptance/publication and any later batch remain separate actions.
'''
    path=ROOT/'docs/architecture/batch-04A-26-owner-review.md';path.write_text(c.clean(text),encoding='utf-8',newline='\n')
    print(decision,'new logical',inventory['logical_count'],'new Python',inventory['python_count'])
if __name__=='__main__':main()
