"""Write the concrete partial-W2 review, canonical next queue and final live receipts."""
from common import *
from verify_checkpoint_v5 import compute
from verify_preservation_v1 import inspect as preservation
from verify_derivative_v1 import inspect as derivative

proof=compute();save('independent-proof-final-v1.json',proof)
save('preservation-final-v1.json',preservation())
save('diagnostic-preservation-final-v1.json',derivative())
import importlib.util
spec=importlib.util.spec_from_file_location('v5_raw_derivative_closeout',PREVIOUS/'verify_derivative_v3.py')
old_derivative=importlib.util.module_from_spec(spec);spec.loader.exec_module(old_derivative)
save('v5-diagnostic-preservation-final-v1.json',old_derivative.inspect())
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-only']),('status',['status','--short']),('diff-check',['diff','--check'])]:
 r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
 assert r.returncode==0
 checks.append(dict(check=name,exit_code=r.returncode,stdout=clean(r.stdout.decode()),stderr=clean(r.stderr.decode())))
assert checks[0]['stdout'].strip()=='codex/04A-38' and all(r['stdout'].strip()==BASE for r in checks[1:4]) and not checks[4]['stdout']
save('git-closeout-v1.json',dict(records=checks,index='EMPTY',baseline=BASE,source_scope='9 approved C6 source/test paths; all preexisting dirty tracked/untracked paths retained'))
commands=[dict(name=p.parent.name,**clean(json.loads(p.read_bytes()))) for p in sorted((RUNROOT/'commands').glob('w2c6-*/command.json'))]
save('command-receipts-v1.json',dict(commands=commands,scope='Executed pre-seal C6 commands; raw private logs retained; publication-copy commands appear separately in external post-seal receipt'))
attempts=[pin(p) for p in sorted((HERE/'attempts').rglob('*')) if p.is_file()]
save('attempt-accounting-final-v1.json',dict(records=attempts,additional=[
 dict(attempt='schema_inspect.py',cause='Assumed metadata list was a dictionary; bounded schema_inspect_v2.py corrected inspection only'),
 dict(attempt='read-only shell searches',cause='Windows literal wildcard filenames and nonexistent speculative inspection paths returned errors; corrected explicit file reads. No source/evidence mutation'),
 dict(attempt='Node display-name accounting',cause='Historical line-only regex omitted one embedded-newline WASM identity. Complete multiline parser independently recounts all377 WASM/916 overall identities and exact C5/C6 occurrence equality; old receipts unchanged. An inspection console print hit cp1252 UnicodeEncodeError; JSON ASCII-safe inspection corrected output only'),
 dict(attempt='final review prose preparation',cause='Default locale decode rejected a UTF8 arrow in the newly authored helper; explicit UTF8 read corrected the preparation. The rejected read wrote no file. All original sources/records remain intact'),
 dict(attempt='retained-preedit-controls-v1',cause='Fresh private parent directory absent: setup ERROR,0 tests. Raw receipt retained; v2 creates parent then unchanged28 tests PASS'),
 dict(attempt='unmodified-full-Python-scripts',cause='Own verified process stopped after2751sec during historical privacy scans; incomplete receipt not counted as full suite. Full assertions rerun with separately proved regex necessary-condition optimization'),
 dict(attempt='regex-guarded-only-Python-scripts',cause='Own exact process identity checked and stopped after523sec in repeated scan; incomplete raw receipt retained separately. Complete lane adds equivalent byte-keyed per-file reuse for identical scanner ASTs, preserving every original assertion'),
 dict(attempt='new focused fixture assumptions',cause='New fullwidth-WELCH culture and out-of-margin equivalence fixtures corrected to actual existing contracts; failed attempts preserved. No old tests weakened')],
 no_hidden_correction=True,no_new_original_queries=True))
v=json.loads((HERE/'validation-v1.json').read_bytes());s=json.loads((HERE/'ownership-summary-v1.json').read_bytes());q=json.loads((HERE/'remaining-work-queue-v1.json').read_bytes());trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
new=[r for r in contracts if r['new_support'] and r['original_id'] is not None]
def write(name,text):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
paths='\n'.join('- `'+r['path']+'`'+(' (new)' if r['before'] is None else '') for r in trans)
names=', '.join(r['name'] for r in new)
nextrows='\n'.join(str(i)+'. **'+r['name']+'** â€” `'+r['original_id']+'`' for i,r in enumerate(q['first_members'],1))
python=v['Python']
write('owner-review.md',f'''NOT_READY_04A38 â€” W2 advanced; W3 NOT_STARTED

Continuation-v6 closes49 additional historical bodies across complete import, parametric and rank UDF dependency families. All22 ordered v5 priority identities are supported. This is a partial W2 checkpoint;456 historical bodies,458 current bodies and64 distinct support declarations remain OPEN. No W3 readiness claim is made.

| Obligation | Independently recomputed result |
|---|---|
| Historical behavioral bodies |248 supported /456 OPEN /704|
| Current behavioral bodies |266 supported /458 OPEN /724|
| Distinct support declarations |239 supported /64 OPEN /303 (325 raw)|
| Current fragments |2346/15564; branches434/3580; inline123/1080|
| Original census fragments |5331/20683; branches1538/5511; inline289/1397|
| Public Excel-DNA registrations |268/268, complete attributes/signatures/defaults exact in Debug/Release|

Current membership grew717â†’724 through seven introduced native callbacks. No historical identifier changed. Fragment populations are independently recomputed from the full immutable W1 original census and final live current census. Original full-census counts include all statement/branch/inline nodes; they must not be compared as though identical to earlier narrower branch/inline subsets. Every fragment is crosswalked individually to its original/current parent and covered status. Current fragment count fell15730â†’15564 as native loops moved to portable consumers, without claiming these fragments disappeared from their behavior contracts.

Entry authentication re-executed v5 ownership and preservation readers, checked all4492 prior publication payloads plus one null self-entry, the prior ZIP hash, accepted Git main/HEAD/merge-base, empty index and whitespace. ANOVA1/Welch/MCP parent closures were reassessed through their full import, unchanged engines, composition, formatting, Err/catch and output dependencies; no unsupported closure was carried forward. See entry.json, v5-anova-parent-reassessment.json and python-nonpass-investigation.json.

Source changes (exact before/after/preimage bytes in source-transitions-final-v1.json):

{paths}

Independent/grouped/single/paired/matrix/complete-case/probability import preparation consumes one Common/Core implementation. Native acquisition, Excel cell interpretation, header detection, per-cell conversion and logging stay in Windows. Indexed callbacks preserve original row order and avoid an extra forwarded Object copy. Native reset occurs before acquisition, then no-reset prepared Common phases preserve reentrant ByRef effects. Direct Common callers retain their public reset wrappers. Single import retains default List capacity while permissive rank import retains row-count capacity. Probability deduplication compares consecutive sorted inputs at the original strict tolerance.

UdfInferencePreparation38 moves complete NI/EQ table initialization, CI preparation, correction selection, repeated-measures/rank MCP orchestration and shared rank input/report preparation. Native alpha/default acquisition, culture-sensitive selector conversions at their original engine-fit boundaries, p-value formatting, error marshaling, ResultTable/spill projection and Err/ProjectData contexts remain native. Existing Core engines, including Welch, are unchanged. Already delegated MW, Wilcoxon and paired-test bodies were reviewed without needless production edits. No duplicate numerical engine or global normalization policy was added.

Newly supported historical bodies: {names}.

whole-body-contracts-v1.json binds91 complete original/current source bodies,49 new historical closures, one newly closed current report helper, dependency chains, branch/statement/inline IDs and fixture crosswalks. callback-proofs-v1.json binds19 complete callbacks to real ldftn targets and captured fields. Five complete stateless module supports close only after all constituent bodies and field/static state are supported. Closed status is never inferred from a portable call alone. proof-authority-crosswalk-v2.json resolves final authority while retaining old ledger references.

Compiled proof retains1915 v5 complete methods/config, explicitly accounts for26 CIL transitions, verifies24 real new Windowsâ†’portable consumers/config, and authenticates533 original engine/adapter methods plus55 types. Existing Common4238 Debug/4236 Release methods plus674 type/field contexts match passive original metadata. A single exact compiler closure-type index alias is documented and all fields/ctors/instructions under it are checked. No new original source execution, fits, queries, captures or compilation occurred. Original archived source byte views are authenticated before UTF16 Roslyn-span slicing; CRLF/LF handling never substitutes a mismatching body.

Source composition proves9 full reconstructions and4 complete prepared-phase CIL comparisons. Text policy is unchanged:153 EqualText sites on62 lines,67 CaseToken sites with788 choices,3969 domain values and1746360 observations/config give0 source-domain differences. Eight synthetic out-of-domain differences/config remain adverse evidence. Arbitrary throwing custom CultureInfo subclasses are outside the original Excel caller domain; no broad custom-provider equivalence is asserted. Deferred historical inconsistencies are listed in deferred-compatibility-constraints-v1.json for the04A.39â€“48 charter, not fixed here.

| Validation | Actual final result |
|---|---|
| Managed Debug, six full suites |20699 PASS /14 inherited FAIL; old identity, occurrence, outcome and failure-message hashes exact;11 new PASS|
| Selected managed Release |192 distinct PASS (97 Common,76 Windows,19 Core)|
| Full managed Release |NOT_RUN_OWNER_PREFERENCE|
| Node |916 PASS; exact prior identity multiset|
| Python |{python['passed']} PASS /{python['failed']} FAIL /{python['errors']} ERROR; exact prior test identity/occurrence population|
| Strict /profile /MkDocs exits |1 /1 /0|
| New independent mutation controls |41 PASS|
| Retained pre-edit reader controls |28 PASS|
| Frozen pre-edit comparison |28966 exact/configuration|
| Separately approved Aâ€“G late comparison |458 exact/configuration|

All nonpasses, raw traceback hashes, first locations and occurrence deltas are in nonpasses-v2.json and python-first-failures-v2.json. One new historical body-pin FAIL is retained at test_04a27_transport_successor.py:60: TryReadPairedNumericColumns and ExtractPairedNumericColumnsIgnoringNonNumeric now delegate copying to existing Core RowsToMatrix. Their exact C6 preimages reproduce the historical anchors; current source/CIL and all frozen results reconcile. See python-source-pin-delta-v1.json. This changes765/180/11 to764/181/11; no statistical or runtime regression is inferred from a source-hash assertion. Current SPC ERROR encounters ValidateFiniteNonnegative first at line68. The complete Node identity parser also restores one embedded-newline WASM identity omitted by historical line-only accounting; all916 full identities and occurrences match v5. The v4â†’v5 FAIL181/ERROR10â†’FAIL180/ERROR11 change is the same unordered-SPC identity: line68 can encounter PanelSupportsSpecificationLimits assertion failure or IncludeNullableInBounds KeyError first. No test outcome was relabeled. Historical authorized source-pin failures remain failures. An initial unmodified Python scripts run was stopped after2751sec in inherited privacy scans and retained as incomplete. The complete lane executes unchanged test assertions with a separately proved necessary-condition email-regex guard; it returns original Match objects for every possible positive case.12068 equivalence controls and scanner source hashes are retained. Other failed tooling/fixture attempts remain versioned in attempt-accounting-final-v1.json.

Exact commands, exits, timings and raw-output hashes are in command-receipts-v1.json. Builds use offline isolated /t:Build for Core/Common and compile-only Windows targets; no restore or XLL packaging. Focused tests ran under Invariant and sk-SK, Debug and selected Release. Full managed Debug used run_validation.py managed; Node/tools/help/scripts used source plus the documented guarded rerun; Common Python used its isolated semantic candidate; strict/profile/MkDocs used serial. The requested canonical PowerShell portable/-Legacy wrappers were inspected but not executed because they invoke implicit restore/shared outputs or XLL packaging; safe equivalent suites were executed instead. Reader commands: owner Python -B verify_checkpoint_v5.py, verify_preservation_v1.py and verify_publication_v1.py from this directory. Publication-copy command and results are in the external post-seal receipt.

Live preservation independently verifies4695 tracked paths,278 lineage pins,100 protected captures,16 owner documents and41 original snapshots against immutable hashes plus explicit source transitions. Omission from the compact derivative never means live deletion. All raw full-managed diagnostics, caches, private command logs/binaries, omitted owner documents and unlisted ignored history are physically excluded from the ZIP. Raw originals remain local; the only public raw-diagnostic derivative changes two synthetic display labels and proves every other JSON leaf exact. Retained historical TestResults was neither removed nor broadly traversed. The deterministic publication manifest has exactly one nonrecursive null self-entry. The external continuation-v6-publication-receipt-v1.json records live/copied executable verification, every ZIP payload/hash/CRC/privacy check and meaningful mutations. No completed publication claim precedes that receipt.

Git remains codex/04A-38 at accepted {BASE}, with identical main/merge-base, empty index and diff-check0. Prior modifications and every earlier evidence version remain. No staging, commit, merge, rebase, push, reset, clean, owner-document edit, framework change or04A.39 work occurred. See git-closeout-v1.json and the external post-seal Git/preservation receipt.

P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN. Native Excel/COM/UIA/RefEdit, independent scientific/R validation, Office/Mac/manual controls, restore/install, packaging/signing and deployment are NOT_RUN. Suggested future manual checks, not executed: import a headed/noncontiguous numeric range with blanks/errors and verify paired/group labels; run Welch/Kruskal-Wallis with omitted and explicit alpha and compare spill shape, p-value formatting and Excel errors.

The next authorized action is ordinary W2 continuation from the exact canonical queue in NEXT_WAVE_HANDOVER.md and remaining-work-queue-v1.json. No new essential original-reference gap was identified. W3 remains barred by mandatory OPEN populations, uncovered fragments and unresolved support dependencies.
''')
write('NEXT_WAVE_HANDOVER.md',f'''NOT_READY_04A38 â€” W2 advanced; W3 NOT_STARTED

Stay on codex/04A-38. Accepted main/HEAD/merge-base: {BASE}. No stage/commit/merge/rebase/push/reset/clean. Preserve all previous evidence, preimages, adverse controls, owner documents and private raw diagnostics. Read the owner review and final publication/Git sibling receipts before source edits.

The authoritative partial reader is verify_checkpoint_v5.py, with review-seal-v5.json and proof-authority-crosswalk-v2.json. Run it from the owner Python with -B; preserve all earlier readers. Current final build/capture stage is families-v2, source-transitions-final-v1.json. Historical248/704 closed (456 OPEN), current266/724 closed (458 OPEN), support239/303 closed (64 OPEN). Fragment totals and covered parent crosswalks are in fragment-coverage-v1.json and original-fragment-parent-crosswalk-v1.json. Public exports268 preserved;28966 genuinely pre-edit plus458 separately approved late cases/config exact. No new original queries authorized.

All22 v5 priority entries are complete. Continue in this exact ordered canonical queue (IDs retained from the original ledger, not reconstructed from line guesses):

{nextrows}

remaining-work-queue-v1.json includes every456 historical OPEN identity and458 current OPEN identity, not merely this priority list. current-ownership-v1.json includes all303 distinct/325raw support declarations and every15564 current fragment. Original full census20683 fragments and historical704 identities are separately mapped. Review full parent importâ†’engineâ†’reportâ†’formatâ†’exception/output paths; unchanged or delegated methods can close only with complete source/CIL/caller proof. Do not close a parent from a single compiled edge or a generic HOST_ONLY label.

Reuse UdfNumericImportPreparation38 prepared phases and UdfInferencePreparation38. Keep original reset/acquisition/ByRef timing, per-cell native conversion, original array bounds/order and exception context. Do not introduce an extra boxed-Object forwarding layer. Preserve lazy header/name behavior and adjacent sorted probability deduplication. Preserve original Core statistical engines and all public contracts. Matrix/complete-case public Common reset wrappers remain valid for direct Common callers; native callers intentionally consume no-reset prepared phases after resetting before acquisition.

Do not alter EqualText/CaseToken policy: new fixed source-domain frontier153/62 EqualText and67/788 CaseToken is explicitly reviewed;1746360 observations/config exact;8 synthetic out-of-domain differences retained. Fresh frontier changes require a new reviewed tool version, never relaxing dynamic/ASCII/domain guards. Source slicing must authenticate raw or Git-normalized bytes to the exact Roslyn source hash before UTF16 spans; source_views.py contains the final reader. Metadata method keys require complete generic arity and array ranks.

Python receipts: current{python['passed']}/{python['failed']}/{python['errors']} PASS/FAIL/ERROR; v5 765/180/11; v4 765/181/10. See exact identity/subtest/occurrence comparison and first failures in nonpasses-v2.json, python-first-failures-v2.json and python-nonpass-investigation.json. SPC set ordering explains the inherited classification movement. C6 additionally retains one new historical source-pin FAIL for the two authorized paired-import RowsToMatrix body transitions, detailed in python-source-pin-delta-v1.json. Do not rewrite the historical04A27 anchors or its test. No relabeling or old assertion edits. Preserve the incomplete2751sec scan attempt and the complete guarded lane. The guard changes no assertions/positive regex result and authenticates unchanged scanner sources.

Managed Debug20699 PASS/14 inherited FAIL with exact prior identities/messages; selected Release192 PASS; full Release NOT_RUN_OWNER_PREFERENCE; Node916 PASS; strict/profile/MkDocs1/1/0.41 new mutation controls and28 retained pre-edit reader controls PASS. All focused/culture/build/comparison receipts and earlier failed attempts remain. Use the owner-specified WPy64-31470 executable; offline isolated outputs and no unsafe wrappers.

No independently complete W2 readiness verifier exists because mandatory OPEN entries remain. Do not start W3,04A.39, native/manual/scientific/restore/install/package/signing/deploy work. Gate states remain P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN. Stop only for an actual mandatory discrepancy or indispensable new original-reference gap requiring a specifically scoped owner decision; ordinary W2 continuation is already authorized.
''')
print('Concrete C6 review, exact canonical handover and final live receipts written')
