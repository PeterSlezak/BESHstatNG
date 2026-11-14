"""Exact first-run delta investigation; no historical assertion or classification waiver."""
from common import *
from collections import Counter
old=json.loads((PREVIOUS/'python-first-failures-v2.json').read_bytes())['records'];now=json.loads((HERE/'python-first-failures-v2.json').read_bytes())['records'];effective=json.loads((HERE/'python-effective-identities-v1.json').read_bytes());transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
def key(r):
    sub=r.get('subtest');return r['identity'],r['outcome'],sub[len(r['identity']):].strip() if sub else '',r['occurrence']
before={key(r):r for r in old};actual={key(r):r for r in now};assert len(before)==len(old) and len(actual)==len(now)
lost=[list(k) for k in before if k not in actual];extra=[];changes=[];reclassified=[];same=0
scanner_ids={'test_04a11_publication.Publication11Tests.test_complete_candidate_privacy_scan','test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values','test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'}
giant=json.loads((HERE/'v9-private-entry-v1.json').read_bytes())['partial'];frozen=json.loads((HERE/'python-scan-input-freeze-v1.json').read_bytes());assert giant in frozen['files']
modules=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes())['modules']
for k,r in actual.items():
    if k not in before:
        prior=next((p for p in old if p['identity']==r['identity'] and key(p)[2:]==k[2:] and p['outcome']=='FAIL'),None)
        if r['outcome']=='ERROR' and prior is not None and r['identity'] in scanner_ids:
            assert 'JSONDecodeError' in r['traceback'] and 'json' in r['traceback'] and 'scan' in r['traceback']
            reclassified.append(dict(identity=r['identity'],old_event_key=list(key(prior)),new_event_key=list(k),old=prior,new=r,classification='ORIGINAL_UNCHANGED_SCANNER_JSON_ERROR_ON_PROTECTED_POST_V9_FAILED_ARTIFACT_NOT_WAIVED',input=giant,source_modules=modules,proof=pin(HERE/'parallel-scanner-equivalence-v1.json')))
            continue
        assert r['identity'].startswith('test_04a28_output.') and r['outcome']=='FAIL','Uninvestigated new nonpass '+r['identity']
        t=next(t for t in transitions if t['after']['sha256'] in r['message']);assert t['path'] in (r.get('subtest') or '')
        extra.append(dict(r,classification='AUTHORIZED_C10_SOURCE_PIN_FAIL_NOT_WAIVED',source_transition=t));continue
    p=before[k]
    if r['message']==p['message']:same+=1;continue
    row=dict(identity=r['identity'],subtest=r.get('subtest'),occurrence=r['occurrence'],outcome=r['outcome'],old_message=p['message'],new_message=r['message'],old_message_sha256=sha(p['message'].encode()),new_message_sha256=sha(r['message'].encode()),old_traceback_sha256=p['traceback_sha256'],new_traceback_sha256=r['traceback_sha256'],first_traceback=r['first_traceback'])
    if r['identity'].startswith('test_04a28_output.'):
        t=next(t for t in transitions if t['after']['sha256'] in r['message']);assert t['before']['sha256'] in p['message'];row.update(classification='RETAINED_SOURCE_PIN_FAIL_AUTHORIZED_ACTUAL_SOURCE_HASH_DELTA_NOT_WAIVED',transition=t)
    elif r['identity'] in scanner_ids:
        row.update(classification='RETAINED_PRIVACY_ASSERTION_CHANGED_ACTUAL_FROZEN_FILESET_NOT_WAIVED')
    else:raise AssertionError('Changed message requires specific source proof '+r['identity'])
    changes.append(row)
assert Counter(tuple(v) for v in lost)==Counter(tuple(r['old_event_key']) for r in reclassified),'Historical event disappeared without exact reclassification'
findings=[];helpers=[]
for path in sorted((PRIVATE/'python-v1/1/parallel-scan-1').glob('*/raw-result.json')):
    helpers.append(pin(path));doc=json.loads(path.read_bytes())
    for r in doc['rows']:findings.extend(r['findings'])
save('python-nonpass-investigation-v1.json',dict(status='COMPLETE_SOURCE_BACKED_NONPASS_INVESTIGATION_NOT_WAIVED',actual_event_keys=[list(key(r)) for r in now],baseline_event_keys=[list(key(r)) for r in old],actual_event_outcomes=dict(Counter(r['outcome'] for r in now)),actual_parent_outcomes=effective['method_outcomes'],extra_events=extra,lost_events=lost,reclassified_events=reclassified,old_events_retained_or_explicitly_reclassified=len(old),historical_assertions_changed=0,source_module_pins=modules,original_queries=0))
save('python-message-delta-investigation-v1.json',dict(status='EXACT_CURRENT_AND_PRIOR_MESSAGES_RETAINED_NOT_WAIVED',same_clean_messages=same,changed_messages=changes,new_events=extra,reclassified=reclassified,all_actual_helper_privacy_findings=findings,helper_findings=len(findings),raw_helpers=helpers,scope='Pure helper findings are actual precomputed normal-file findings, not a claim that original whole scanners reached their final privacy assertion past the protected giant JSON failure. The original scanner parent retains original failures/errors and first tracebacks. No file filtering or new privacy exception.'))
print(json.dumps(dict(old_events=len(old),actual_events=len(now),extra=len(extra),reclassified=len(reclassified),changed_messages=len(changes),parents=effective['method_outcomes'])))
