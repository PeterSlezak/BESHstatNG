"""Fresh recovery campaign versus accepted46 and immutable47 v1, event by event."""
import sys,re,ast
from pathlib import Path
ROOT=Path.cwd()
sys.path.insert(0,str(ROOT/'evidence/04A47/recovery-v2'))
from recovery import *
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import clean
def collect(parts):
    out=[]
    for part in parts:
        exact(part['raw']);raw=load(ROOT/part['raw']['path']);exact(raw['parent_journal'])
        journal=[json.loads(s) for s in (ROOT/raw['parent_journal']['path']).read_text(encoding='utf-8').splitlines()]
        assert [r['value'] for r in journal if r['kind']=='COMPLETE']==raw['records']
        assert collections.Counter(r['value']['id'] for r in journal if r['kind']=='START')==collections.Counter(r['id'] for r in raw['records'])
        out+=raw['records']
    assert len(out)==len({p['id'] for p in out})==956
    return {p['id']:p for p in out}
def events(parents):
    out={};seen=collections.Counter()
    for p in parents.values():
        for e in p['events']:
            assert sha(e['traceback'].encode())==e['traceback_sha256']
            k=(p['id'],e['outcome'],clean((e.get('subtest') or '')[len(p['id']):].strip()));n=seen[k];seen[k]+=1
            out[k+(n,)]=e
    return out
def fp(e):return dict(message_sha256=sha(e.get('message','').encode()),traceback_sha256=e['traceback_sha256']) if e else None
def shape(t):return re.sub(r'File "([^"\n]+)"',lambda m:'File "'+m.group(1).replace('\\','/').rsplit('/',1)[-1]+'"',t)
def reconcile():
    source_lock();payload();tools=PRIVATE/'python-tools-v1';post=load(PRIVATE/'python-input-postcheck-v2.json');assert not post['changed'] and post['command']['exit_code']==0
    receipts=load(tools/'python-worker-receipts-v1.json')['receipts'];parts=[dict(lane=r['lane'],phase=r['phase'],raw=r['raw']) for r in receipts]
    current=collect(parts);old47=load(OLD/'python/parent-event-reconciliation-v2.json');old46=load(ROOT/'evidence/04A46/python/parent-event-reconciliation-v2.json')
    v1=collect(old47['parts']);accepted=collect(old46['parts']);inventory=load(tools/'python-discovery-inventory-v1.json');inv={r['id']:r for r in inventory['entries']}
    assert set(current)==set(v1)==set(accepted)==set(inv)
    parents=[]
    for k,p in current.items():
        s=pin(inv[k]['module_path']);assert s['sha256']==inv[k]['module_sha256']
        parents.append(dict(id=k,outcome=p['outcome'],v1=v1[k]['outcome'],accepted46=accepted[k]['outcome'],source=s,runtime_seconds=p['end_seconds']-p['start_seconds']))
    guard='test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change'
    changes=[p for p in parents if p['outcome']!=p['v1']];assert {p['id'] for p in changes}==set(IDS)|{guard} and len(changes)==3,'UNEXPLAINED_PARENT_OUTCOME_TRANSITION'
    assert all(p['outcome']==p['accepted46'] for p in parents if p['id']!=guard),'UNEXPLAINED_ACCEPTED46_PARENT_OUTCOME_DRIFT'
    assert current[guard]['outcome']=='FAIL' and v1[guard]['outcome']=='PASS' and accepted[guard]['outcome']=='PASS'
    assert current[guard]['events'][0]['message']=="b'.gitignore' is not false"
    assert collections.Counter(p['outcome'] for p in parents)==dict(PASS=833,FAIL=112,ERROR=11)
    a=events(accepted);b=events(v1);c=events(current);cross=[];raw=[]
    olddispositions={r['stable_event_id']:r for r in load(OLD/'python/dispositions-v1.json')['rows']}
    plan_before=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.24.md')['sha256'];plan_after=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md')['sha256']
    for k in sorted(a.keys()|b.keys()|c.keys()):
        ea,eb,ec=a.get(k),b.get(k),c.get(k);stable=sha(json.dumps(k,sort_keys=True).encode())
        why='UNRESOLVED';proof={}
        if k[0]==guard:
            why='OWNER_APPROVED_IGNORE_POLICY_DIFF_REACTIVATES_UNCHANGED_SOURCE_GUARD;ACTUAL_FAIL_UNWAIVED'
            proof=dict(assertion_source=pin('scripts/tests/test_04a05_completion_successor.py'),line=136,actual_git_diff=['.gitignore'],owner_approval='.gitignore was modified on purpose and is approved. Please continue.',test_changed=False,waived=False)
        elif k[0] in IDS:
            why='RECOVERED_ORIGINAL_INPUT_RESTORES_ACCEPTED46_OUTCOME;V1_ERRORS_RETAINED'
            if ea and ec:assert ea['message']==ec['message'];proof['accepted46_message_exact']=True
        elif fp(eb)==fp(ec):
            if fp(ea)==fp(ec):why='ALL_THREE_EVENT_FINGERPRINTS_IDENTICAL_UNWAIVED'
            else:
                prior=olddispositions.get(stable);assert prior and prior['disposition']!='UNRESOLVED_DIAGNOSTIC_REVIEW'
                why='EXACT_V1_EVENT;ACCEPTED46_TRANSITION_ALREADY_DISPOSITIONED';proof=dict(prior_disposition=prior['disposition'])
        elif eb and ec and eb['message']==ec['message'] and shape(eb['traceback'])==shape(ec['traceback']):why='RUNNER_PATH_ONLY_TRACE_CONTEXT_UNWAIVED'
        elif eb and ec:
            before=eb['message'];after=ec['message']
            if plan_before in before and plan_after in after and before.replace(plan_before,plan_after)==after:
                why='AUTHORIZED_APPEND_ONLY_PLAN_SHA_ARGUMENT_CONTEXT;FAIL_UNWAIVED';proof=dict(before_plan_sha256=plan_before,current_plan_sha256=plan_after)
            elif '6fdd2d9d0d05d73c12694ba149c8051b97a12da0e71f19f71eed1935b7adb913' in before and before.replace('6fdd2d9d0d05d73c12694ba149c8051b97a12da0e71f19f71eed1935b7adb913','95b233c640d30104d3ea71bad0fdb09ba7a7d4a8e7dcef34e8cc370a0e29346d')==after:
                why='OWNER_APPROVED_IGNORE_SHA_ARGUMENT_TRANSITION;HISTORICAL_FAIL_UNWAIVED';proof=dict(unchanged_expected_sha256='9a3cbe846efb3c06a8ee68a619f85c46b5f0e7c10171440c121b380a7ad3b112',before_actual_sha256='6fdd2d9d0d05d73c12694ba149c8051b97a12da0e71f19f71eed1935b7adb913',current_actual_sha256='95b233c640d30104d3ea71bad0fdb09ba7a7d4a8e7dcef34e8cc370a0e29346d',assertions_changed=False)
            elif before.startswith('{') and after.startswith('{'):
                try:oldset=ast.literal_eval(before);newset=ast.literal_eval(after)
                except (ValueError,SyntaxError):pass
                else:
                    if oldset=={'planning/BESHStatNG_CrossPlatform_MasterPlan.md'} and newset==oldset|{'.gitignore'}:
                        why='OWNER_APPROVED_IGNORE_PATH_ADDED_TO_EXISTING_SCOPE_ASSERTION;FAIL_UNWAIVED';proof=dict(before_actual_paths=sorted(oldset),current_actual_paths=sorted(newset),assertions_changed=False)
        row=dict(stable_event_id=stable,id=k[0],outcome=k[1],subtest_sha256=sha(k[2].encode()),occurrence=k[3],accepted46=fp(ea),v1=fp(eb),v2=fp(ec),disposition=why,proof=proof)
        cross.append(row);raw.append(dict(key=k,accepted46=ea,v1=eb,v2=ec))
    save(PRIVATE/'three-way-event-pairs-v3.json',dict(events=raw))
    unresolved=[r for r in cross if r['disposition']=='UNRESOLVED']
    report=dict(classification='FRESH_COMPLETE_CAMPAIGN_NOT_COMPOSITE',campaigns=1,parents=sorted(parents,key=lambda p:p['id']),outcomes=dict(collections.Counter(p['outcome'] for p in parents)),accepted46_outcomes=dict(collections.Counter(p['outcome'] for p in accepted.values())),v1_outcomes=dict(collections.Counter(p['outcome'] for p in v1.values())),changed_parents=changes,events=cross,event_counts=dict(accepted46=len(a),v1=len(b),v2=len(c)),event_dispositions=dict(collections.Counter(r['disposition'] for r in cross)),unresolved_diagnostic_events=[r['stable_event_id'] for r in unresolved],all_source_modules_exact=True,all_input_pins_unchanged=True,all_historical_nonpasses_unwaived=True,old_v1_errors_preserved=True,workers_at_peak=3,parts=parts,interpreter=load(HERE/'payload-authentication-v1.json')['interpreter'],input_freeze=pin(PRIVATE/'python-input-freeze-v2.json'),input_postcheck=pin(PRIVATE/'python-input-postcheck-v2.json'),protected_events=pin(PRIVATE/'three-way-event-pairs-v3.json'),accepted46=pin(ROOT/'evidence/04A46/python/parent-event-reconciliation-v2.json'),v1=pin(OLD/'python/parent-event-reconciliation-v2.json'),first_diagnostic_review=pin(HERE/'python-reconciliation-v1.json'),historical_privacy='1941_FINDINGS_EACH_PARENT_AND_W3_ERRORS_RETAINED_UNWAIVED',historical_scope_FAIL='FAIL_UNWAIVED',gates=original.GATES)
    save('python-reconciliation-v2.json',report)
    print(json.dumps(dict(outcomes=report['outcomes'],event_counts=report['event_counts'],event_dispositions=report['event_dispositions'],unresolved=[dict(id=r['id'],stable_event_id=r['stable_event_id']) for r in unresolved])),flush=True)
if __name__=='__main__':reconcile()
