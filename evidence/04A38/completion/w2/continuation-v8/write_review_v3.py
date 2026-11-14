"""One coherent C8 owner review/handover after actual validation; ASCII punctuation."""
from common import *
from collections import Counter
s=json.loads((HERE/'ownership-summary-v1.json').read_bytes());p=json.loads((HERE/'independent-proof-final-v1.json').read_bytes());v=json.loads((HERE/'validation-v1.json').read_bytes());w=json.loads((HERE/'python-worker-accounting-v1.json').read_bytes());q=json.loads((HERE/'remaining-work-queue-v1.json').read_bytes());trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'];new=json.loads((HERE/'new-closures-v1.json').read_bytes())['rows']
paths='\n'.join('- `'+r['path']+'`'+(' (new, authenticated absent at entry)' if r['before'] is None else '') for r in trans)
ids='\n'.join(str(i)+'. **'+r['source_contract']['current_record']['name']+'** - `'+r['original_id']+'`' for i,r in enumerate(new,1))
next_ids='\n'.join(str(i)+'. **'+r['name']+'** - `'+r['original_id']+'`' for i,r in enumerate(q['first_members'],1))
worker_rows='\n'.join('|'+str(r['lane'])+' '+r['phase']+'|'+str(r['methods'])+'|'+str(r['method_outcomes'])+'|'+str(r['failure_events'])+'/'+str(r['error_events'])+'|'+str(r['exit_code'])+'|'+str(round(r['seconds'],3))+'|' for r in w['executions'])
focus=json.loads((HERE/'focused-v3-packet-v1-Debug.json').read_bytes());focus_counts=[dict(suite=r['suite'],outcomes=r['tests']['outcomes']) for r in focus['results'] if r['kind']=='TEST']
status='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'
review=f'''{status}

C8 completes a coherent sample-size/report/option and prepared range-layout packet. It supports24 additional historical canonical identities:23 function parents and the exact queued Get2D lambda whose current callback was already supported. Current supported bodies increase299 to324:23 counterparts plus2 genuine new private host helpers. No new anonymous callback is introduced. Twelve public Common preparation APIs,15 portable methods including private constructors/failure helper, and2 enum types are independently inventoried outside the historical/current Windows frontier.

| Population | C7 | C8 |
|---|---|---|
| Historical |274/704 supported;430 OPEN|298/704 supported;406 OPEN|
| Current |299/724 supported;425 OPEN|324/726 supported;402 OPEN|
| Distinct support |240/303 supported;63 OPEN|241/303 supported;62 OPEN;325 raw,22 already primary|
| Current fragments |2895/15537|3361/15203;695/3434 branches;214/1004 inline|
| Original fragments |5939/20683|6901/20683;1969/5511 branches;456/1397 inline|
| Public registrations |268|268; Debug/Release exact metadata|

The704 historical identities remain unchanged; there are58 position-bearing current shifts and13 changed existing bodies, not58 new behaviors. Every accepted C7 whole-body status and byte hash is retained under exact signature/parent mapping. The only new container closure is SampleSizeUDFs: every child is supported and all3 declared fields are the authenticated original literal strings. UdfDataImport and UdfOutputTables remain OPEN. Authoritative ledgers are historical-ownership-v2.json and current-ownership-v2.json; the v1 drafts remain with their corrected authority-pointer history.

Exact source/test paths, immediate preimages, absent-new states and copied after-images are in source-transitions-final-v1.json:

{paths}

Eight complete ordered sample-size orchestrations move to Common. Opaque host arguments are read through actual original Windows delegates in the same order; required conversion/collective guards precede optional reads. Core numeric predicates remain their real original targets. Native Text Select becomes an explicit4-state enum bridge at the original lazy point; the original parser and LegacyUdfText caller CIL/source-domain restrictions are unchanged. Margin signs, sidedness/defaults, alpha/beta, existing Core engines and Raw report builders remain exact. Exceptions propagate to original Windows catches/log labels; native Excel errors and final Object(,) materialization remain Windows-owned. No statistical formula, scientific issue, output value/type/shape, help/default/export or framework target is corrected.

Prepared response/predictor/optional/Cox array allocation/copy moves to shared Common helpers. Native range/header/name/optional interpretation, GEE Convert.ToString/Trim/ordinal cluster dictionary, model constructors/import calls, Core GEE sorting with RowIds and final ByRef data assignment remain original. Existing v7 aligned-column copying is reused. Ordinary object-matrix Cox text-strata headers still cause the original row mismatch; no header correction is invented. Arrays/cell references, failure-side allocations, lower-bound interpretation and lazy conversions retain their native boundaries.

whole-body-contracts-v1.json binds49 full authentic original/current source parents and actual transitive CIL. callback-proofs-v2.json binds11 direct/nested source callbacks to their real immediate ldftn parents and complete field/exception contexts. historical-callback-contracts-v1.json independently proves the queued Get2D original/current body, caller, cache fields and ctor/cctor identity without an alias. portable-api-contracts-v1.json and its separate Roslyn census bind all15 new portable declarations/fragment sets and2 enum contexts. new-parent-fragment-crosswalk-v2.json maps960 original fragments individually:885 exact source correspondences, including every original branch/inline under actual argument bindings;75 composite statements/declarations use complete authentic-parent reconstruction. Static fixture reachability is qualified and does not assert every branch executed.

Every retained existing method CIL differs only for the13 intended native parents in each configuration. Core engines and named fields/constructors remain exact. Raw metadata records one unrelated PSM compiler-cache static-field enumeration-order difference; names/types/flags/ctor/method CIL are unchanged and that type is outside all claimed paths. This measurement is explicitly retained, not called complete metadata byte identity or a generic alias. Unused open constructed handles remain scoped raw measurements; all actually claimed methods/fields have concrete owners. Text restrictions and all8 synthetic out-of-domain counterexamples/config remain; no global Unicode equivalence is asserted.

Newly supported exact canonical IDs (full body/branch/inline/CIL evidence in new-closures-v1.json):

{ids}

Validation: one comprehensive six-suite Debug campaign produced20720 PASS/14 inherited FAIL, exactly also its effective result;16 new PASS, no missing/duplicate identity and no inherited outcome/message drift. Common used complementary main/semantic selections, each case once, so only its semantic class received isolated observations. This avoids C7's known4 fixture-environment failures without assertion changes. C7 first20700/18 and effective20704/14 remain historical, not rewritten. Full and selected Release unit tests are NOT_RUN_OWNER_PREFERENCE, with no exception. Offline Release compile/export and fresh29424 current-vs-frozen comparisons/config passed:28966 genuine pre-edit plus separately late A432/B12/C2/D4/E2/F2/G4. Original plain/repeat/trace, typed/branch and failed attempts remain immutable; no original source compilation/execution/query/fit/capture occurred.

Focused first outcomes: {focus_counts}. The new import test first assumed unsorted GEE rows, then incorrectly assumed ordinary object-matrix Cox strata headers were removed. Both assumptions were corrected to authenticated unchanged source, strengthening row/offset/time alignment and header rejection assertions. The failed first TRX and corrective attempts are retained; only that new affected test reran, with no production or historical assertion change. The final focused correction passed. Earlier entry-pin chronology, nested-callback omission, field-enumeration and reader/tool syntax/normalization failures remain versioned attempts.

Python actual parent outcomes: {v['Python']['methods']}; nonpass events: {v['Python']['nonpass_events']}. All956 discovered methods execute once as the effective population; failing subtests remain separate. Historical v6 mixed-unit764/181/11 is labelled separately from the actual C7 parent baseline830/115/11. The C6-created04A27 line60 source-pin FAIL remains retained, not inherited from v5 or waived. The unordered-SPC ERROR keeps its distinct identity/first current message. Every final nonpass has exact identity/subtest/occurrence and raw first traceback/message hashes. Node retains916 identities; strict/profile/MkDocs exits are {v['strict_profile_MkDocs']}.

| Python execution | Methods | Parent outcomes | FAIL/ERROR events | Exit | Seconds |
|---|---|---|---|---|---|
{worker_rows}

Five non-scanner lanes run first; four known source-root scratch writers are pre-routed unchanged to a proved disposable fixture copy in lane5. Scanner lane1 then runs with at most5 read-only helpers, parent+helpers <=6. No full managed/Python overlap occurs. Only lane1 runs11/13/14 scanners. Their unchanged source AST/assertions, C6 content cache/regex positive fallback and all12068 controls are retained. New process-cache equivalence proves the exact actual candidate multiset/order, per-file original function results, reviewed tuples, fixed seed, content/environment invalidation and dropped/abnormal helpers. Raw helper input/output/command/stream hashes and timings are retained; helpers are not counted as extra unittest methods.

The source/proof scanner input set was frozen before the campaign and checked afterward, without descending historical TestResults. Final generated test/publication receipts necessarily follow that boundary and receive separate complete final hash/privacy verification; historical scanners are not claimed to have scanned future bookkeeping files. Unchanged C7 privacy decisions are explicitly authenticated policy/path/content cache reuse; every changed/new payload is evaluated freshly. Four exact new synthetic adverse-control source files retain bounded positive home/URI and empty private-key-marker examples by whole-file/hash/literal/category proof; arbitrary changed bytes/paths/profiles are rejected. Historical Python privacy assertions and their actual nonpasses remain unchanged.

Live entry verified branch codex/04A-38, main/HEAD/merge-base ba1636f5356ac761509c2cfa16fc66b92430935d and empty index; all5914 C7 payload hashes/one null self-entry, the actual standalone436632124-byte C7 ZIP SHA/CRC,4695 tracked paths,278 lineage pins,100 captures,16 owner documents and41 original snapshots. Prior raw command/copy streams are authenticated separately. Canonical Master Plan Rev2.13 and AGENTS.md were read and remain unchanged.

Publication is a selected review derivative, not the authoritative repository. Physical all-member SHA/CRC, privacy findings, second deterministic serialization, copied executable reader and rejection controls are attested only after the external continuation-v8-publication-receipt-v1.json exists. Live .git/ignored private DLLs/TRX/diagnostics and16 excluded owner documents require the separate post-publication continuation-v8-final-git-preservation-v1.json. Omitted material is not deleted. C7 failed long-path copies and all previous incomplete/scanner/tooling attempts remain intact; C8 uses a normal short new copy root.

The canonical portable/-Legacy PowerShell wrappers are NOT_RUN: their implicit restore/shared outputs/packaging conflict with this owner scope. Isolated offline compile/test equivalents and their exact commands/input hashes/exits/durations are supplied in command-receipts-v1.json, build/metadata/compatibility and execution receipts. No stage/commit/merge/rebase/push/reset/clean, owner-document/framework edit, restore/install, original oracle, native Excel/COM/UIA/RefEdit/Mac/manual run, R/scientific work, XLL packaging/signing/deployment, W3 or04A39 occurred.

Future manual checks, NOT_RUN and requiring separate authorization: sample-size superiority/NI/EQ spills and optional-error precedence; GEE/GLM/Cox ranges with offsets/weights/strata/header row alignment. Recommendation remains NOT_READY because406 historical,402 current and62 distinct support identities plus uncovered fragments remain mandatory OPEN obligations.
'''
with (HERE/'owner-review.md').open('x',encoding='utf-8',newline='\n') as f:f.write(review)
handover=f'''{status}

Read owner-review.md, authoritative historical/current ownership-v2 files, review-seal-v1.json, proof-authority-crosswalk-v1.json, source-transitions-final-v1.json, new-parent-fragment-crosswalk-v2.json, and both external live/publication receipts. Reestablish actual Git/private/ZIP state; derivatives cannot prove authoritative ignored material or live Git.

Historical298/704 supported,406 OPEN; current324/726 supported,402 OPEN; distinct support241/303 supported,62 OPEN (325 raw/22 already primary). Current fragments3361/15203; original6901/20683. Exports268 preserved. C8 adds24 historical closures,25 supported current bodies and2 genuine current declarations, with0 new callbacks. SampleSizeUDFs is closed only with every child and3 unchanged literal fields; UdfDataImport/UdfOutputTables remain OPEN.

The original704 IDs are unchanged. All C7 status/body byte identities remain;58 raw position shifts are mapped without inventing behaviors. Continue the following exact filtered C7 machine order (all other OPEN IDs remain in remaining-work-queue-v1.json), with actual dependencies and complete source/CIL/branch/inline/field/closure proof:

{next_ids}

One Debug campaign20720/14 inherited failures;16 new PASS. No Release unit suites; Release compile/export and29424 frozen exact/config passed. Python {v['Python']['methods']} parent methods versus {v['Python']['nonpass_events']} events; all956 identities once, separate disjoint lane5 copy. Node916; strict/profile/docs actual1/1/0. Preserve first failed/new-test/reader/tooling attempts and exact nonpass histories, including C6-created line60 FAIL and distinct unordered-SPC ERROR. Use owner Python, up to6 workers, and the source-pinned equivalent scanner process cache only under its proved seed/environment/file-set constraints. Keep complete source/proof input stable before expensive validation; separately identify generated post-run receipt scope.

No new original source execution/compiler/fit/query/capture or oracle, native/scientific/manual authority, installer/restore/XLL/signing/deployment, commit/stage/merge/rebase/push/reset/clean, W3 or04A39. Stop for separately scoped owner authorization only if a genuinely indispensable original reference gap/protected conflict or unapproved irreversible/native/scientific action appears. No such numerical/original-reference gap was identified in C8.
'''
with (HERE/'NEXT_WAVE_HANDOVER.md').open('x',encoding='utf-8',newline='\n') as f:f.write(handover)
with (HERE/'test-execution-schedule.md').open('x',encoding='utf-8',newline='\n') as f:f.write(status+'\n\n'+review[review.index('Validation:'):review.index('Live entry verified')])
print('C8 owner review, exact next queue and execution accounting written')
