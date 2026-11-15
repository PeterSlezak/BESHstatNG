"""Exact source/hash/Git argument explanations, preserving every FAIL and raw pair."""
from common40_v3 import *
import collections,re,unittest
lock();v1=load(HERE/'python/diagnostic-dispositions-v1.json');raw={r['stable_event_id']:r for r in load(PRIVATE/'python-event-crosswalk-raw-v2.json')['events']}
delta=load(HERE/'matrix/source-freeze-v2.json')['source_delta'];old_source={r['path']:r['sha256'] for r in load(ROOT/'evidence/04A39/correction-v1/entry/source-lock.json')['files']}
current_hash={}
def current(path):
    if path not in current_hash:current_hash[path]=pin(path)['sha256']
    return current_hash[path]
def message(left,right,label=None):
    try:unittest.TestCase().assertEqual(left,right,label)
    except AssertionError as e:return str(e)
    return None
scope={}
for phase,extra in [('13',False),('14',True)]:
    c=module('diagnostic_scope_'+phase,ROOT/('scripts/capture-04a'+phase+'-contract.py'));pins=load(c.RUN/'baseline.json')['source_pins'];excluded=c.CHANGED|({c.OWNER_PLAN} if extra else set())
    before=[p for p,h in pins.items() if p not in excluded and old_source.get(p,current(p))!=h]
    now=[p for p,h in pins.items() if p not in excluded and current(p)!=h]
    scope[phase]=dict(before=before,current=now,before_message=message([],before),current_message=message([],now),authority=pin(c.RUN/'baseline.json'),reader=pin(c.__file__))
for row in v1['rows']:
    if row['disposition']!='SAME_SOURCE_TEST_ASSERTION_SITE;ARGUMENT_DRIFT_REQUIRES_REVIEW':continue
    r=raw[row['stable_event_id']];a=r['before']['message'];b=r['current']['message'];proof=row['proof'];kind=None
    if a.replace('codex/04A-39','codex/04A-40')==b:
        kind='AUTHENTICATED_FEATURE_BRANCH_CONTEXT_DRIFT;FAIL_UNWAIVED';proof['before_branch']='codex/04A-39';proof['current_branch']='codex/04A-40'
    elif a.replace(PREVIOUS,MAIN)==b:
        kind='AUTHENTICATED_MAIN_ADVANCE;FAIL_UNWAIVED';proof.update(before_main=PREVIOUS,current_main=MAIN)
    for item in delta:
        before=item['before']['sha256'];after=item['after']['sha256'];path=item['before']['path']
        if a.replace(before,after)==b:
            kind='AUTHORIZED_SOURCE_PIN_ARGUMENT_DRIFT;FAIL_UNWAIVED';proof['transition']=item;break
        if b in [message(after,before,path),message(before,after,path)]:
            kind='FIRST_FAILING_SOURCE_PIN_SHIFT;CURRENT_AUTHORIZED_TRANSITION;FAIL_UNWAIVED';proof['transition']=item;proof['current_assertion_message_reproduced_exact']=True;break
    for phase,name in [('13','test_all_prechange_protected_bytes_except_exact_approved_scope'),('14','test_exact_scope_and_external_owner_plan_are_separate')]:
        if row['identity'].endswith(name):
            s=scope[phase]
            if s['before_message']==a and s['current_message']==b:
                kind='EXACT_RECONSTRUCTED_HISTORICAL_SOURCE_SCOPE_ARGUMENTS;FAIL_UNWAIVED';proof.update(baseline=s['authority'],reader=s['reader'],before_paths=s['before'],current_paths=s['current'])
    allowed=git('diff','--name-only').decode().splitlines()
    if b==str(allowed) and a==str(['planning/BESHStatNG_CrossPlatform_MasterPlan.md']):
        kind='LEGACY_PUBLICATION_SCOPE_REJECTS_AUTHORIZED_CURRENT_DIFF;FAIL_UNWAIVED';proof.update(before_paths=['planning/BESHStatNG_CrossPlatform_MasterPlan.md'],current_paths=allowed)
    if kind:row['disposition']=kind
v1['counts']=dict(collections.Counter(r['disposition'] for r in v1['rows']));v1['unresolved']=[r['stable_event_id'] for r in v1['rows'] if 'REQUIRES_REVIEW' in r['disposition'] or r['disposition']=='UNRESOLVED']
v1['reproduction']='Read-only source/assertion-argument reconstruction, not another parent-test campaign. Unchanged assertions/expected operands; no FAIL waived.'
save('python/diagnostic-dispositions-v2.json',v1)
print('Exact diagnostic contexts',v1['counts'],'unresolved',len(v1['unresolved']),flush=True)
