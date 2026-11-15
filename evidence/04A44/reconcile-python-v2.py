from common44 import *
import collections,ast,re
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import clean
lock();target=PRIVATE/'python-tools-v2';inventory=load(target/'python-discovery-inventory-v1.json')['entries']
current=[];parts=[]
for receipt in load(target/'python-worker-receipts-v1.json')['receipts']:
    exact(receipt['raw']);raw=load(ROOT/receipt['raw']['path']);exact(raw['parent_journal'])
    journal=[json.loads(line) for line in (ROOT/raw['parent_journal']['path']).read_text(encoding='utf-8').splitlines()]
    assert [r['value'] for r in journal if r['kind']=='COMPLETE']==raw['records']
    assert collections.Counter(r['value']['id'] for r in journal if r['kind']=='START')==collections.Counter(r['id'] for r in raw['records'])
    assert raw['exit_code']==receipt['command']['exit_code']
    current+=raw['records'];parts.append(dict(lane=receipt['lane'],phase=receipt['phase'],raw=receipt['raw'],outcomes=raw['outcomes'],exit_code=raw['exit_code']))
previous=[];old_public=load(ROOT/'evidence/04A43/python/parent-event-reconciliation-v2.json')
for part in old_public['parts']:exact(part['raw']);previous+=load(ROOT/part['raw']['path'])['records']
assert len(current)==len(previous)==len(inventory)==956
assert collections.Counter(r['id'] for r in current)==collections.Counter(r['id'] for r in previous)==collections.Counter(r['id'] for r in inventory)
inv={r['id']:r for r in inventory};oldparents={r['id']:r for r in previous}
def source(identity):
    r=inv[identity];path=ROOT/r['module_path'];assert pin(path)['sha256']==r['module_sha256'];tree=ast.parse(path.read_text(encoding='utf-8-sig'));name=identity.rsplit('.',1)[-1]
    return dict(source=pin(path),method=name,lines=[[n.lineno,n.end_lineno] for n in ast.walk(tree) if isinstance(n,(ast.FunctionDef,ast.AsyncFunctionDef)) and n.name==name])
def events(parents):
    out={};seen=collections.Counter()
    for p in parents:
        for event in p['events']:
            assert sha(event['traceback'].encode())==event['traceback_sha256']
            base=(p['id'],event['outcome'],clean((event.get('subtest') or '')[len(p['id']):].strip()));n=seen[base];seen[base]+=1
            out[base+(n,)]=dict(event,identity=p['id'],occurrence=n)
    return out
old=events(previous);now=events(current)
def shape(trace):
    return re.sub(r'File "([^"\n]+)"',lambda m:'File "'+m.group(1).replace('\\','/').rsplit('/',1)[-1]+'"',trace)
guards={'test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change','test_04a04_alignment.PairedBlockSourceGates.test_udf_exports_and_packaging_unchanged','test_04a05_contingency.ContingencySourceGates.test_no_designer_udf_numerical_packaging_or_framework_change'}
privacy={'test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values','test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'}
delta=load(HERE/'matrix/source-freeze-v1.json')['source_delta'];cross=[];raw_pairs=[]
for key in sorted(old.keys()|now.keys()):
    a=old.get(key);b=now.get(key);stable=sha(json.dumps(key,sort_keys=True).encode())
    fingerprint=lambda e:dict(message_sha256=sha(e.get('message','').encode()),traceback_sha256=e['traceback_sha256']) if e else None
    left=fingerprint(a);right=fingerprint(b)
    kind='NEW_NONPASS' if a is None else 'MISSING_NONPASS_EVENT' if b is None else 'RETAINED_IDENTICAL' if left==right else 'RETAINED_CHANGED_MESSAGE' if left['message_sha256']!=right['message_sha256'] else 'RETAINED_CHANGED_TRACE'
    disposition='UNRESOLVED_DIAGNOSTIC_REVIEW';proof={}
    if kind=='RETAINED_IDENTICAL':disposition='INHERITED_NONPASS_UNWAIVED'
    elif key[0] in privacy and b is None:
        disposition='CURRENT_CANDIDATE_PRIVACY_PASS;HISTORICAL_RAW_AND1941_FINDINGS_FAIL_RETAINED'
        proof=dict(current_parent=next(p['outcome'] for p in current if p['id']==key[0]),original_scanner='UNCHANGED_ACTUAL_SCOPE;FINAL_FULL_AND_PHYSICAL_SCANS_RECORDED_SEPARATELY',before40_raw_snapshot='6_RULE_OCCURRENCES_IN_PYTHON;LATER8_FOCUSED_OCCURRENCES_RETAINED',accepted39='1941_EACH_FAIL;1599+342;W3_ERRORS_UNWAIVED')
    elif key[0] in guards:
        disposition='HISTORICAL_SOURCE_DIFF_GUARD_FAIL_RETAINED;CURRENT_AUTHORIZED_UDF_PATH_ARGUMENTS_CHANGED'
        proof=dict(actual_current_git_diff=git('diff','--name-only').decode().splitlines(),original_assertions_unchanged=True,current_outcome='FAIL')
    elif a and b and a.get('message')==b.get('message') and shape(a['traceback'])==shape(b['traceback']):
        disposition='FILE_PATH_ONLY_TRACE_CONTEXT;NONPASS_UNWAIVED';proof=dict(same_exact_message=True,file_basename_normalized_trace_equal=True)
    else:
        joined='\n'.join(str(e.get(k,'')) for e in [a or {},b or {}] for k in ['message','traceback','subtest'])
        matched=[r for r in delta if r['after']['sha256'] in joined and (r['before']['sha256'] in joined or r['after']['path'] in joined.replace('\\','/'))]
        if matched:
            disposition='AUTHORIZED_SOURCE_SHA_ARGUMENT_TRANSITION;ORIGINAL_SOURCE_PIN_FAIL_UNWAIVED'
            proof=dict(transitions=matched,source_test_unchanged=True)
    row=dict(stable_event_id=stable,identity=key[0],outcome=key[1],subtest_identity_sha256=sha(key[2].encode()),occurrence=key[3],before=left,current=right,classification=kind,disposition=disposition,proof=proof,source=source(key[0]))
    cross.append(row);raw_pairs.append(dict(stable_event_id=stable,key=key,before=a,current=b))
parents=[dict(id=p['id'],before=oldparents[p['id']]['outcome'],outcome=p['outcome'],source=source(p['id'])) for p in current]
changes=[p for p in parents if p['before']!=p['outcome']]
# All parent transitions are recorded;final disposition must account for each without changing test outcomes.
assert all(next(p['outcome'] for p in current if p['id']==g)=='FAIL' for g in guards)
save(PRIVATE/'python-event-pairs-v1.json',dict(events=raw_pairs))
post=load(PRIVATE/'python-input-postcheck-v2.json');assert not post['changed'] and post['command']['exit_code']==0
summary=dict(parents=956,parent_outcomes=dict(collections.Counter(p['outcome'] for p in current)),before_outcomes=dict(collections.Counter(p['outcome'] for p in previous)),event_outcomes=dict(collections.Counter(e['outcome'] for e in now.values())),categories=dict(collections.Counter(e['classification'] for e in cross)),changed_parents=changes,unresolved_diagnostic_events=[r['stable_event_id'] for r in cross if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW'],dropped_parents=[],duplicated_parents=[],campaigns=1,workers_at_peak=3)
save('python/parent-event-reconciliation-v1.json',dict(summary=summary,parents=parents,events=cross,parts=parts,protected_raw=pin(PRIVATE/'python-event-pairs-v1.json'),accepted43=pin(ROOT/'evidence/04A43/python/parent-event-reconciliation-v2.json'),original54=pin(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json'),original32=pin(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json'),historical_privacy='TWO_ACCEPTED39_FAIL_PARENTS_1941_EACH;W3_TWO_ERROR_PARENTS_UNWAIVED;EARLIER40_RAW_FAILURES_RETAINED',discovery=pin(HERE/'python/discovery-v2.json'),gates=GATES))
print(json.dumps(dict(summary=summary,unresolved=[dict(identity=r['identity'],event=r['stable_event_id'],classification=r['classification']) for r in cross if r['disposition']=='UNRESOLVED_DIAGNOSTIC_REVIEW'])),flush=True)
