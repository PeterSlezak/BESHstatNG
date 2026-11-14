"""Exact first-run event/message delta investigation; historical tests stay untouched."""
from common_v2 import *
from collections import Counter
old=json.loads((PREVIOUS/'python-first-failures-v1.json').read_bytes())['records'];now=json.loads((HERE/'python-first-failures-v1.json').read_bytes())['records'];effective=json.loads((HERE/'python-effective-identities-v1.json').read_bytes());transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
def key(r):
    sub=r.get('subtest');return r['identity'],r['outcome'],sub[len(r['identity']):].strip() if sub else '',r['occurrence']
before={key(r):r for r in old};actual={key(r):r for r in now};need=lambda b,msg:None if b else (_ for _ in ()).throw(AssertionError(msg))
need(len(before)==len(old) and len(actual)==len(now),'Duplicate event occurrence');lost=[list(k) for k in before if k not in actual];need(not lost,'Historical nonpass event disappeared')
extra=[];changes=[];same=0
for k,r in actual.items():
    if k not in before:
        need(r['identity']=='test_04a28_output.Output28SourceContracts.test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved' and r['outcome']=='FAIL','Uninvestigated new non-source-pin failure')
        t=next(t for t in transitions if t['after']['sha256'] in r['message']);need(t['path'] in (r.get('subtest') or ''),'New pin failure does not match actual changed source')
        extra.append(dict(r,classification='AUTHORIZED_C9_SOURCE_PIN_FAIL_NOT_WAIVED',source_transition=t,first_assertion='Immutable test_04a28_output.py line29; new actual source SHA differs from its historical expected pin. The test remains FAIL.'))
        continue
    prior=before[k]
    if r['message']==prior['message']:same+=1;continue
    need(r['first_traceback']==prior['first_traceback'],'Changed first assertion requires separate investigation')
    row=dict(identity=r['identity'],subtest=r.get('subtest'),occurrence=r['occurrence'],outcome=r['outcome'],old_message=prior['message'],new_message=r['message'],old_clean_message_sha256=sha(prior['message'].encode()),new_clean_message_sha256=sha(r['message'].encode()),old_traceback_sha256=prior['traceback_sha256'],new_traceback_sha256=r['traceback_sha256'],first_traceback=r['first_traceback'])
    if r['identity'].startswith('test_04a28_output.'):
        t=next(t for t in transitions if t['after']['sha256'] in r['message']);need(t['before']['sha256'] in prior['message'],'Changed source-pin before hash differs');row.update(classification='RETAINED_ASSERTION_AUTHORIZED_ACTUAL_SOURCE_HASH_DELTA_NOT_WAIVED',transition=t)
    else:
        need(r['identity'] in ['test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values','test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'],'Unexpected changed nonpass message')
        row.update(classification='RETAINED_PRIVACY_ASSERTION_ACTUAL_FROZEN_FILESET_FINDING_COUNT_CHANGED_NOT_WAIVED')
    changes.append(row)
helpers=[];findings=[]
for f in sorted((PRIVATE/'python-v1/1/parallel-scan-1').glob('*/raw-result.json')):
    helpers.append(pin(f))
    for row in json.loads(f.read_bytes())['rows']:findings.extend(row['findings'])
need(len(helpers)==5,'Missing actual scanner helper reports')
classified=[]
for r in findings:
    if '/continuation-v9/' in r['path']:
        category='FIXED_PUBLIC_REFERENCE_PATH_SOURCE' if r['path'].endswith(('inspect_stage_v1.py','compatibility_v1.py')) else 'SOURCE_CODE_SYNTHETIC_CONTROL_OR_SERIALIZED_TRACE_SYNTAX'
        classified.append(dict(r,classification=category,source=pin(ROOT/r['path'])))
save('python-nonpass-investigation-v1.json',dict(status='COMPLETE_SOURCE_BACKED_NONPASS_INVESTIGATION_NOT_WAIVED',actual_event_keys=[list(key(r)) for r in now],baseline_event_keys=[list(key(r)) for r in old],actual_event_outcomes=dict(Counter(r['outcome'] for r in now)),actual_parent_outcomes=effective['method_outcomes'],extra_events=extra,lost_events=lost,old_events_retained=len(old),historical_assertions_changed=0,source_module_pins=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes())['modules'],original_queries=0))
save('python-message-delta-investigation-v1.json',dict(status='EXACT_CURRENT_AND_PRIOR_MESSAGES_RETAINED_NOT_WAIVED',same_clean_messages=same,changed_messages=changes,new_events=extra,all_actual_privacy_findings=findings,current_findings=len(findings),C8_historical_findings=206,C8_history='v8 had172 to206:24 synthetic controls,8 traceback separator matches,2 fixed reference source paths. All remain historical actual failures; no old tuple list is fabricated.',C9_findings_with_source=classified,raw_helpers=helpers,scope='Original privacy assertions remain FAIL. Current tuples are exact raw helper outputs; classifiers describe code/fixture syntax without suppressing events. Future generated receipt files are outside the frozen historical scanner input set and receive separate final publication policy checks.'))
print(json.dumps(dict(old_events=len(old),actual_events=len(now),extra=len(extra),changed_messages=len(changes),privacy_findings=len(findings),parents=effective['method_outcomes'])))
