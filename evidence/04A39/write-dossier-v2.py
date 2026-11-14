"""Consolidated NOT_READY dossier from actual frozen campaigns; no baseline rewrite."""
from common39 import *
import importlib.util

source_lock()
controls=load(HERE/'final/readiness-controls-v2.json')
facts=controls['actual'];assert facts['verdict']=='NOT_READY_04A39'
spec=importlib.util.spec_from_file_location('reader39_final',HERE/'verify-readiness-v2.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
# Recompute identities/contracts. The completed full live input hash readback is
# already bound by the controls receipt; do not rehash22GB for prose generation.
assert reader.verify(ROOT,live=False)==facts
final_sources=load(HERE/'matrix/final-source-state-v1.json')['source_pins']
assert final_sources==runner().__class__.build.__globals__['source_pins']()
register=load(HERE/'register/defects-v7.json')
python=load(HERE/'python-node-scanners/python-identities-v1.json')
delta=load(HERE/'python-node-scanners/python-delta-v2.json')
rows=[]
for config in ['Debug','Release']:
    for row in load(HERE/('build-and-tests/'+config+'-identities-v1.json'))['nonpasses']:
        rows.append(dict(id='MANAGED:'+config+':'+row['identity_sha256']+':'+str(row['occurrence']),identity=row['class_name']+'.'+row['method'],
            status='INHERITED_RESTRICTED;NOT_WAIVED',message_sha256=row['message_sha256'],trace_sha256=row['traceback_sha256'],delta=row['classification']))
for row in python['events']:
    event_id=sha(json.dumps([row['identity'],row['outcome'],row.get('subtest'),row['occurrence']],ensure_ascii=False).encode())
    rows.append(dict(id='PYTHON_EVENT:'+event_id,identity=row['identity'],outcome=row['outcome'],subtest=row.get('subtest'),occurrence=row['occurrence'],
        status='CHANGED_NONPASS_NOT_WAIVED' if row['classification']!='RETAINED_IDENTICAL' else 'INHERITED_NONPASS_NOT_WAIVED',
        delta=row['classification'],message_sha256=row['message_sha256'],trace_sha256=row['traceback_sha256']))
assert len(rows)==len({x['id'] for x in rows})
save('final/unresolved-exceptions-v2.json',dict(current_entries=rows,original_W3_validation_and_events=pin(HERE/'register/validation-record-crosswalk-v1.json'),
    current_python_delta=pin(HERE/'python-node-scanners/python-delta-v2.json'),
    historical_privacy_errors_unwaived=2,old_managed_Release_failures_unwaived=14,old_Release_trace_variations=9,
    original_parent_outcomes=dict(PASS=830,FAIL=113,ERROR=13),original_event_outcomes=dict(FAIL=185,ERROR=13),
    current_parent_outcomes=python['summary']['parent_outcomes'],current_event_outcomes=python['summary']['event_outcomes'],
    blocking=['A39-RAW-REVIEW-PRIVACY-01','PYTHON_54_CHANGED_MESSAGES_AND_TRACES'],no_waivers=True))
save('final/readiness.json',dict(status='NOT_READY_04A39',utc=utc(),main=MAIN,branch='codex/04A-39',tip=MAIN,facts=facts,
    canonical_register=pin(HERE/'register/current-v5.json'),register_counts=register['counts'],
    source_and_test_gate='VERIFIED_BEHAVIOR_PRESERVING_SOFTWARE_SCOPE;NO_PRODUCTION_CHANGE',
    privacy_and_diagnostic_gate='BLOCKING_CHANGED_NONPASSES;NOT_WAIVED',
    exceptions=pin(HERE/'final/unresolved-exceptions-v2.json'),controls=pin(HERE/'final/readiness-controls-v2.json'),
    source_states=pin(HERE/'matrix/final-source-state-v1.json'),production_source_changes=0,activated_defect_fixes=0,
    owner_acceptance='NOT_GRANTED_BY_AGENT',next_batch='04A40_NOT_AUTHORIZED',gates=GATES,
    limitations=['Current833/112/11 is a fresh result, not a replacement of inherited830/113/13.',
        'Two historical UnicodeDecodeError parents now FAIL with1599 findings each. Historical errors remain unwaived.',
        'Three old FAIL parents PASS in the new tracked-diff scope; original failures are retained, not discharged.',
        '54 retained nonpass messages/traces changed and require review.',
        '29,424/config compatibility retained fromW3 after production source/compiled/fixture proof, not freshly executed39 comparator.',
        'Release managed336 targeted only;W3 full Release14 failures/nine trace variations remain inherited.',
        'No native/scientific/R/Excel/COM/UIA/RefEdit/browser/OfficeMac/install/sign/deploy acceptance.',
        'Inherited37484 file pins/154 archives retain qualified W3 physical scope;39 preservation is incremental.']))
text=f'''# 04A.39 owner review

NOT_READY_04A39

The software audit and behavior-preservation campaign completed. Two changed privacy nonpasses and54 changed nonpass diagnostics prevent a READY claim. No production code, original test expectation, archived reference or scientific/native behavior was repaired or rebaselined.

Merged main/branch parent/tip: `{MAIN}`; branch`codex/04A-39`. This commit is the direct child of W3 baseline`ba1636f5356ac761509c2cfa16fc66b92430935d`. All1350 frozen paths exist in the commit:1113 exact Git byte hashes plus237 explicitly owner-authorized newline-only differences; live1311 exact/39 newline-only. All18 package/installer-support files were read back. W2 acceptance13November2026 and qualifiedW3/overall38 acceptance14November2026 are owner assertions in the current instruction history; merge is independently proved. Original hashes and immutable38 dossiers remain untouched.

| Check | Actual result |
|---|---|
| Full managed Debug |21127 identities:21113PASS /14 inheritedFAIL;all21094 W3 identities retained;33 added occurrences PASS;14 messages/traces identical|
| Targeted managed Release |336PASS:303 retained selected+33 added;full Release NOT_RUN_NOT_REQUIRED|
| Fresh shipping Debug/Release |268 exact exports each;every normalized per-method signature/CIL/exception/delegate/callback/dependency record unchanged vs same-configurationW3|
| Retained frozen compatibility |29424 exact/config;28966PRE_EDIT_FROZEN+458LATE_ARCHIVED_ENTRY_SUPPLEMENT;12 corpora;no original or fresh39 current comparator execution|
| Fresh current observation fixtures |77 exact/config;typed inputs,Double bits,cultures,source/selected coordinates and six expected current exceptions preserved|
| Fresh Python parents |956 unique:833PASS /112FAIL /11ERROR|
| Fresh Python nonpass events |184FAIL /11ERROR;139 unchanged,54 changed messages/traces,2 changed privacy FAIL events|
| Inherited W3 Python |830PASS /113FAIL /13ERROR;185FAIL /13ERROR events;all remain preserved separately and unwaived|
| Fresh Node |916PASS;exact W3 identity multiset|
| Fresh strict/profile/MkDocs |1/1/0;strict/profile remain nonpasses|

No production extraction was warranted. The executed101-file Roslyn census contains1861 members, including192 focused import/argument identities (154 functions and38 lambdas). Every focused method has source/body/token hashes, spans, branches/statements, ByRef assignments, callbacks, error paths and caller candidates. Existing portable38 policies already serve the Windows consumers. Excel sentinel/reference/acquisition and session/cache ownership remain Windows-owned; no duplicate parser, statistical algorithm or unnecessary public API was added. Complete compiled dependency/contract equality supplies the semantic edge proof beyond syntax candidates. Debug reflection reordered identical all-static fields in three generated closure type records; all names/types/flags/values and all method instructions remain exact. Release has no such type-record difference.

The single current register is`register/current-v5.json`→`defects-v7.json`: {register['counts']['canonical_entries']} canonical bundles/issues/validation/gate entries,1514 immutable obligations,802 restricted controls,91 historical report IDs/286 occurrences,163 explicitW3 validation/gate records and198 original event mappings.759 original referenced member IDs resolve exactly to original signatures/body/source hashes. Counts overlap; these are not1169 statistical bugs. Exact repeated IDs share one canonical record; C17/C18 no-label CA aliases retain both histories. All distinct original risk contracts/facets remain; no original obligation is discharged and no04A.48 exit disposition is granted. Four39 observations include three input/provenance findings and the blocking raw-review privacy finding. Declaration candidates establish traceability, not scientific/native fault adjudication.

The Framework nested fixture reproduces actual conversion drift: typed1.2345678901234567 bits`3ff3c0ca428c59fb` become Core-input bits`3ff3c0ca428c5a0a` after column stringification caused by numeric text. Near2^53,negative zero,Invariant/sk-SK text,excluded rows and extreme-finite overflow are characterized. No Excel display limit or scientific impact is inferred; no precision-changing path was activated. Historical original precision oracle is REFERENCE_UNAVAILABLE/CANDIDATE_ONLY. Paired fixtures distinguish physical worksheet coordinates,selected positions,complete-case positions,RowIds and explicit subject keys. Raw arrays have no invented source coordinates. Source intersections and positional pairing remain distinct accepted policies. Existing physical sidecar/clone limitations,ByRef state,callback order,shape/lower-bound/error/locale contracts are preserved.

Python executed once with three ordinary workers at peak,then disposable partition,then one serial scanner with zero helpers.10314 exact bounded files were frozen and re-read unchanged. The inherited input manifest included22.18GB and the exact protected4.21GB failed-attempt artifact; it was stream-hashed by its exact path,not parsed or packaged. The historicalTestResults tree was never recursively enumerated or cleaned. Every956 parent started/completed exactly once;195 current events and198 original events remain separately accounted.

Five parent outcomes changed. Three tracked-diff guards now PASS because accepted38 changes were committed and no longer appear in`git diff`; their old failures remain historical/unwaived and their PASS does not prove all untracked/private files. Two privacy parents changedERROR→FAIL and each reports1599 findings in new raw39 artifacts. The raw reports I generated contain machine identifiers/paths; they are preserved locally and require separately named sanitized transport derivatives. This is not a successful repository-wide privacy result. Both historical UnicodeDecodeError events remain retained and unwaived; unordered-SPC and all other inherited errors remain. There are54 changed nonpass messages/traces,fully crosswalked; they are not silently accepted as identical or green. No full campaign was rerun to erase history.

Strict reports the same stale/ambiguous NormalPlot constructor identity:`BESHstatNG/src/Graphics/NormalPlot.vb|BESHStatNG.graphics.NormalPlot|ConstructorBlock|0fdb131aa98f9dfe`. Profile returns the same opaque`INTEGRITY_FAILURE` stdout and exit1; its exact originating assertion is not exposed by that CLI. W3's CRLF warning stderr disappeared after the approved merge/line-ending transition. Both dispositions remainINHERITED_RESTRICTED/EXPLICIT_OWNER_DECISION_REQUIRED. The standard verify-beshstatng.ps1/-Legacy wrappers were NOT_RUN because they implicitly restore and write shared historicalTestResults; the authenticated offline isolated build/test graph ran instead. No wrapper PASS is claimed.

The living Master Plan is Revision2.15 and records the actual38 acceptances/merge and39IN_PROGRESS,not accepted. ExactRevision2.14 is retained separately; original226 roadmap identities/dependencies and39–48 charter remain. No methods.yml,Ribbon,Help availability or Office/Mac support claim changed. MkDocs exit0 is an actual fresh result.

Stopped attempts remain: sandbox branch-creation denial followed by authorized escalation; read-only inspection mistakes; v1 diagnostic helper assembly lookup; new-test SourceAt compile mistake; a new alpha fallback expectation corrected to the actual exception; report filename collision recovered without recompilation; missing rawIL key on abstract metadata; Debug reflection-order report stop; register subset/global802 and Counter-reader mistakes; Python prelaunch replacement-count stop before identities; legacy-source lexical decode stop and slow lookup interrupted before output then replaced by equivalent token membership. Reporting-only versions subsequently complete source signatures and validation/gate coverage. No protected original or failed report was overwritten.

Preservation is incremental. W3's37484 private pins/154 archive declarations retain their original qualified physical scope;39 does not claim another full private-union audit or independent upload of every byte. All18 live-only support files,current production locks,authorized newline/test-project transitions and new task-local outputs are separately pinned. The review ZIP/readback is a derivative: raw privateTRX/worker logs,live index,owner documents,every historical trace/archive and sensitive raw report values are not all transported. Exclusions/derivative hashes are explicit. Byte-level ZIP review does not rerun private owner-machine/native/scientific validation.

Required next decisions/actions:

1. Independently review the changed privacy scope,1599 findings per scanner and54 diagnostic deltas. Do not waive or relabel the failures/errors.
2. Authorize an exact report-only raw/private separation and additive re-freeze plan if the owner wants the repository candidate privacy gate repaired. Preserve every frozen original/failed record and raw SHA. Run the affected privacy gates against the corrected,explicit candidate domain; do not change statistical/test expectations or restore original-reference executions.
3. Keep nested precision/paired/scientific/native candidates dormant under their named future owners and exact authorization gates. Resolve any proposed behavior change separately.
4. Obtain independent ChatGPT review and an explicit owner39 acceptance/merge decision only after the blocking review/repair scope is resolved. No04A.40 start follows automatically.

Suggested later manual checks,only with exact authorization: a disposable Excel scalar/mixed-cell and offset paired-range UDF check; existing WASM prototype paired/provenance inspection. Neither was executed here. Excel/COM/UIA/RefEdit,R/scientific,OfficeMac/browser,installation,XLL packaging,signing and deployment remain NOT_RUN_UNAUTHORIZED.

P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN;04A39_NOT_READY;04A40_NOT_AUTHORIZED
'''
with (HERE/'final/owner-review.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
with (HERE/'final/next-gate-handover.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write('NOT_READY_04A39. Independent ChatGPT review of current-v5/defects-v7, actual Python delta-v2, privacy1599 findings per changed parent,54 diagnostic drifts and safe derivative/exclusions. Owner must authorize an exact reporting/privacy repair/re-freeze scope or explicitly retain restrictions before any39 acceptance decision. No product/scientific/native fix,staging,commit,merge,push or04A40 is authorized here.\n')
with (HERE/'publication/README-v1.md').open('x',encoding='utf-8',newline='\n') as stream:stream.write('NOT_READY39 review derivative. Validate ZIP paths/SHA/size/CRC with verify-package-v1.py before extraction. Read privacy-and-exclusions: separately named sanitized renderings are not original executable source/raw diagnostics; authoritative bytes remain local with exact hashes. Canonical authority is current-v5.json→defects-v7.json. If raw authorities were replaced by transport renderings, live canonical/readiness replay requires owner checkout,not just this derivative. Actual13 per-payload scanner checks the transport;raw repository privacy failures remain unwaived. Complete privateTRX/worker outputs/live index/owner documents/historical trace archives are not all included. No independent host/scientific execution claim. Later binary .001/.002 volumes,if owner chooses them,must be concatenated numerically before extracting an outer0-store ZIP;they are not independent source packages. No binary splitting or upload performed.\n')
print('NOT_READY_04A39 dossier written from actual results',flush=True)
