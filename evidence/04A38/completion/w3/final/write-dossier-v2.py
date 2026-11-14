"""Actual-result dossier, canonical unwaived exceptions, and owner handover."""
from common import *
import importlib.util,re
from collections import Counter

mode=sys.argv[1];assert mode in ['candidate','final']
spec=importlib.util.spec_from_file_location('w3_readiness',HERE/'final/verify-w3-v2.py')
v=importlib.util.module_from_spec(spec);spec.loader.exec_module(v)
verdict=v.compute(mode=='final')
managed={c:load(HERE/('build-and-tests/'+c+'-identities-v1.json')) for c in ['Debug','Release']}
py=load(HERE/'python-node-scanners/python-identities-v1.json')
register=[]
for c,d in managed.items():
    for r in d['nonpasses']:
        register.append(dict(id='MANAGED:'+c+':'+r['identity_sha256']+':'+str(r['occurrence']),
            identity=r['class_name']+'.'+r['method'],outcome=r['outcome'],classification='VERIFIED_NONPASS_INHERITED',
            message_sha256=r['message_sha256'],trace_sha256=r['traceback_sha256'],delta=r['classification'],
            disposition='INHERITED_RESTRICTED;NOT_WAIVED',action='Owner disposition required for the existing failed contract; no source/test repair authorized in W3.'))
for r in py['parents']:
    if r['outcome']!='PASS':register.append(dict(id='PYTHON_PARENT:'+r['id'],identity=r['id'],outcome=r['outcome'],
        classification='VERIFIED_NONPASS_INHERITED' if r['classification']=='RETAINED_IDENTICAL' else 'NEW_OR_CHANGED_NONPASS',
        disposition='INHERITED_RESTRICTED;NOT_WAIVED',action='Retain parent outcome and each failing/error event; no privacy or scientific waiver inferred.'))
for r in py['events']:
    if r['classification']!='RETAINED_IDENTICAL':
        register.append(dict(id='PYTHON_EVENT_DELTA:'+sha(json.dumps((r['identity'],r.get('subtest'),r['occurrence'])).encode()),
            identity=r['identity'],delta=r['classification'],message_sha256=r['message_sha256'],trace_sha256=r['traceback_sha256'],
            disposition='EXPLICIT_OWNER_DECISION_REQUIRED' if r['classification']=='RETAINED_CHANGED_MESSAGE' else 'INHERITED_RESTRICTED;TRACE_DRIFT_DISCLOSED',
            action='Inspect exact W2/W3 message/trace crosswalk; inherited nonpass remains unwaived.'))
for id,description in [('STRICT_NORMALPLOT','Strict audit exit1; NormalPlot ambiguity retained'),('PROFILE','Profile exit1; inventory-complete-with-approved-deferrals remains nonzero'),
                       ('P','P=NO'),('N1','N1=NO GO'),('N2','N2=NO'),('C2','C2=UNMET'),('C3','C3=UNAPPROVED'),
                       ('SCIENTIFIC','NOT_GOLDEN'),('04A39','04A39_BLOCKED')]:
    source_identity=re.search(r'decision: ([^\r\n]+)',(RUNROOT/'commands/w3v1-serial-strict/stderr.log').read_text(encoding='utf-8')).group(1) if id=='STRICT_NORMALPLOT' else None
    register.append(dict(id=id,description=description,source_identity=source_identity,disposition='EXPLICIT_OWNER_DECISION_REQUIRED',
        classification='REQUIRES_OWNER_DECISION',action='Separate explicit authority and evidence needed; W3 software success grants no release.'))
suffix='v1' if mode=='candidate' else 'v2'
save('final/unresolved-exceptions-'+suffix+'.json',dict(entries=register,
    managed_failure_occurrences=28,unique_managed_failure_identities=14,python_nonpass_parents=sum(r['outcome']!='PASS' for r in py['parents']),
    python_nonpass_events=len(py['events']),scope='Exact identity register; outcomes/messages/traces remain in their machine ledgers. No waiver is issued.'))
paths=['entry/entry-manifest-v2.json','entry/review-source-crosswalk-v2.json','matrix/verification-matrix-v1.json','matrix/python-campaign-amendment-v2.json',
       'abi-cil/complete-closure-v4.json','abi-cil/targeted-regressions-v1.json','compatibility/reference-authority-audit-v2.json',
       'python-node-scanners/python-identities-v1.json','python-node-scanners/python-input-postcheck-v2.json','python-node-scanners/node-v1.json',
       'python-node-scanners/serial-v1.json','final/adverse-controls-v2.json']
if mode=='final':paths+=['publication/parts-index-v1.json','publication/physical-readback-v1.json','preservation/post-publication-lock-v1.json','preservation/w3-new-pins-v1.json','final/git-final-v1.json']
result=dict(verdict,owner_assertion=dict(date='2026-11-13',assertion='W2 source completion accepted after independent v17 post-correction review',source='Current owner instruction history; not a Git commit or pre-existing repo receipt'),
    exact_commands='Versioned matrix, execution receipts and private per-command stdout/stderr/TRX hashes',
    evidence=[pin(HERE/p) for p in paths],exceptions=pin(HERE/('final/unresolved-exceptions-'+suffix+'.json')),
    Release_categories={k:'FRESH_W3_EXECUTED;SEE_SEPARATE_NONPASS_AND_PARITY_RECEIPTS' for k in ['compilation','full-six-suite-unit-testing','ABI-CIL-signatures-exports','current-frozen-compatibility']},
    independent_review_limits='Local source/build/test/private/archive audits were executed by Codex on the owner machine. Upload readers verify derivative bytes and census, not independent live/private/test reproduction. The independent ChatGPT v17 review was not located through the local authority chain; no copy manufactured.',
    manual_checks='NOT_RUN_UNAUTHORIZED: live Excel/native/COM/UIA/RefEdit, R/scientific, Office/Mac/browser/manual. Suggested later authorized smoke: covariance unknown-object conversion/error boxing/no-intercept alias in a controlled Excel session; existing WASM harness ANOVA/KW input/output route.',
    prohibited_actions='No production/test correction, original execution, install/sign/deploy, stage/commit/merge/rebase/reset/push, or04A39',
    terminal='W2_OWNER_ACCEPTED; W3_VERIFICATION_CANDIDATE_OR_BLOCKED; '+v.RESTRICTIONS)
save('final/'+('engineering-candidate-v1.json' if mode=='candidate' else 'readiness.json'),result)
text='# W3 software verification '+('candidate' if mode=='candidate' else 'owner review')+'\n\n'
text+=result['status']+'. W2 source completion is owner-accepted as an assertion dated13November2026. Phase04A.38 remains NOT_READY.\n\n'
text+='| Configuration | Managed PASS | Managed FAIL | Identities | Frozen exact cases | Exports |\n|---|---:|---:|---:|---:|---:|\n'
for c in ['Debug','Release']:
    s=managed[c]['summary'];text+='| '+c+' | '+str(s['outcomes'].get('Passed',0))+' | '+str(s['outcomes'].get('Failed',0))+' | '+str(s['executed'])+' |29424|268|\n'
text+='\nAll four previously deferred Release categories were freshly executed: compilation, complete managed testing, shipping metadata/CIL/signature/export inspection, and current-only frozen compatibility. The original Release references were not executed or regenerated. Each configuration retains28966 PRE_EDIT_FROZEN and458 LATE_ARCHIVED_ENTRY_SUPPLEMENT cases. All52 selected v17 correction regression occurrences passed; no test was repeated for that selection.\n\n'
text+='Python actual unique parents: '+str(py['summary']['parent_outcomes'])+' over'+str(py['summary']['parents'])+'; individual nonpass events: '+str(py['summary']['nonpass_event_outcomes'])+'. Event deltas: '+str(py['summary']['event_categories'])+'. The W2 one-message/three-trace drift remains historical and unwaived. Two privacy UnicodeDecodeError ERRORs and unordered-SPC ERROR remain errors. Node fresh916PASS. Strict/profile/MkDocs actual exits1/1/0.\n\n'
text+='Debug inherited failure messages and traces are identical. Release independently discovered the same21094 identities and14 inherited failures with unchanged messages; nine Windows traces differ from Debug and remain disclosed. These failures are VERIFIED_NONPASS_INHERITED, never green. See the canonical unresolved-exceptions register for exact identities and dispositions.\n\n'
text+='The exact704historical/726logical-current/303distinct-support sets have zero OPEN complements. Source-bound proof replay accounts for20683original fragments,12659current physical fragments and13695predecessor transition fragments, including723physical Windows members plus three Common lambdas. Both configurations have fresh passive metadata. Debug normalized CIL equals accepted Debug; Release differences are explicitly bounded to debugger attributes, generated async state-machine emission, value-type constructor opcode forms and the sealed Release Debug.Print behavior. Earlier report stops remain preserved.\n\n'
text+='Entry authenticated1350/1350 actual source files, including all18package/installer-support files,31910private pins and152physical archive records with byte/file-identity checks. The descriptive master plan remains Revision2.14 at historical W2v12. The uploaded core index alone supplies518source byte matches; the external1332accessible-source assertion is not misrepresented as direct core-index coverage.\n\n'
if mode=='final':
    audit=load(HERE/'preservation/post-publication-lock-v1.json');pins=load(HERE/'preservation/w3-new-pins-v1.json');parts=load(HERE/'publication/parts-index-v1.json')
    text+='After main publication, the actual existing private reader re-audited31896base pins, the14postseal additions and all1350source bytes passed separate readback. W3 added'+str(pins['new_count'])+' pinned files at that audit, for a'+str(pins['union'])+'-file audited union; subsequent final/supplement pins are additive in the delivery receipt. The original massive TestResults tree was not enumerated. The4.21GB protected historical artifact was only stream-hashed by its exact reader.\n\n'
    text+='The new ACTIVE contains '+str(parts['new_active_files'])+' payloads and is complemented by the ten unchanged W2 core ZIPs and separate unchanged W2 POSTSEAL_SUPPLEMENT. All twelve main parts passed physical SHA and full streaming payload SHA/size/CRC readback. New ACTIVE payloads were also extracted locally. A separately named final W3 POSTSEAL supplement binds these post-publication receipts; its final physical proof is in the delivery receipt. No external upload or independent private/live execution is claimed.\n\n'
    text+='Final Git/index receipt confirms codex/04A-38; HEAD/main/merge-base ba1636f5356ac761509c2cfa16fc66b92430935d; index SHA2562036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c; staged empty; diff-check0. New evidence is untracked; accepted W2 working-tree changes remain unstaged and uncommitted.\n\n'
text+='All stopped report attempts, prelaunch errors, raw results and historical failures remain retained. A missing Python private parent caused a0.31-second prelaunch stop before any identity started; the unstarted campaign continued under an additive input freeze with no repeated parent. Production/test source repairs and original-reference execution were not performed. Standard verify-beshstatng wrappers and -Legacy were NOT_RUN because they implicitly restore and write shared TestResults; the authenticated offline isolated equivalents ran instead. No live Excel or WASM browser smoke was run.\n\n'
text+='Independent review must distinguish machine-executed owner-machine evidence from derivative byte/census verification. Raw private files, complete raw TRX/worker traces, all prior published archives and live .git/index are retained locally and are not all inside the derivative. Privacy exclusions are authenticated; no new waiver was issued and repository-wide privacy is not declared clean.\n\n'
text+='Next decision: independent ChatGPT review, then an explicit owner Phase04A.38 acceptance decision. No automatic acceptance, merge, native/scientific release or04A.39 start.\n\n'+result['terminal']+'\n'
path=HERE/'final'/('owner-review-candidate-v1.md' if mode=='candidate' else 'owner-review.md')
for before,after in [('dated13November2026','dated 13 November 2026'),('over956','over 956'),('14inherited','14 inherited'),('268exports','268 exports'),('29424','29,424'),('28966','28,966'),('458 LATE','458 LATE'),('All52','All 52'),('over21','over 21'),('same21094','same 21,094'),('and14','and 14'),('fresh916PASS','fresh 916 PASS'),('actual exits1/1/0','actual exits 1/1/0'),('704historical/726logical-current/303distinct-support','704 historical / 726 logical current / 303 distinct support'),('20683original','20,683 original'),('12659current','12,659 current'),('13695predecessor','13,695 predecessor'),('including723physical','including 723 physical'),('authenticated1350/1350','authenticated 1,350/1,350'),('all18package','all 18 package'),('31910private','31,910 private'),('152physical','152 physical'),('Revision2.14','Revision 2.14'),('W2v12','W2 v12'),('supplies518source','supplies 518 source'),('external1332accessible-source','external 1,332 accessible-source'),('re-audited31896base','re-audited 31,896 base'),('the14postseal','the 14 postseal'),('all1350source','all 1,350 source'),('The4.21GB','The 4.21 GB'),('index SHA256203682','index SHA256 203682'),('diff-check0','diff-check 0'),('a0.31-second','a 0.31-second'),('or04A.39','or 04A.39')]:text=text.replace(before,after)
with path.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
if mode=='final':
    handover='Independent ChatGPT review is the next gate. Review owner-review.md, readiness.json, the unresolved-exceptions-v2.json register, all13complementary ZIPs and the final delivery receipt. Require explicit disposition of inherited managed/Python/strict/profile nonpasses and all restricted native/scientific gates.\n\nOnly a later explicit owner Phase04A.38 acceptance decision and owner-confirmed merge into main can permit preparation of04A.39 under its own charter. This dossier performs neither.\n\nIf binary .001/.002 volumes around a0-store outer ZIP are later used for transmission, concatenate them in numerical order, then extract the outer archive to obtain the original ZIP. They are not independent source packages. No binary splitting was performed here.\n\n'+result['terminal']+'\n'
    with (HERE/'final/next-gate-handover.md').open('x',encoding='utf-8',newline='\n') as f:f.write(handover)
print(mode,result['status'],flush=True)
