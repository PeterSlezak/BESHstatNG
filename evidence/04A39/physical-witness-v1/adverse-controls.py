from witness_lib import *
from gate_reader import documents,verify,VERDICT
from physical import safe_name
import copy

source=documents();results=[]
assert verify(source)['status']==VERDICT
mutations=[
 ('MISSING_PHYSICAL_SCANNER_REPORT',lambda d:d['physical']['reports'].pop()),
 ('CHANGED_ARCHIVE_HASH',lambda d:d['physical']['reports'][0]['archive'].__setitem__('sha256','0'*64)),
 ('MISSING_MEMBER_VIEW',lambda d:d['physical']['reports'][0].__setitem__('member_views',1)),
 ('MISSING_FIXTURE',lambda d:d['fixtures']['fixtures'].pop()),
 ('ADDED_FIXTURE_ALLOWANCE',lambda d:d['fixtures']['fixtures'].append(copy.deepcopy(d['fixtures']['fixtures'][0]))),
 ('CHANGED_FIXTURE_MATCH_SHA',lambda d:d['fixtures']['fixtures'][0]['key'].__setitem__(6,'0'*64)),
 ('UNRESOLVED_PHYSICAL_MATCH',lambda d:d['physical']['reports'][0]['findings'].append(dict(category='unresolved'))),
 ('SDK_LOCATION_CHANGED',lambda d:d['sdk']['occurrences'][0].__setitem__('line',999)),
 ('SDK_HEAD_CHANGED',lambda d:d['sdk'].__setitem__('HEAD','0'*40)),
 ('SDK_WILDCARD_BYPASS',lambda d:d['sdk']['controls'][0].__setitem__('missing_HEAD_literal_findings',0)),
 ('HOSTNAME_CONTEXT_ABSENT',lambda d:d['inputs']['owner_context'].__setitem__('hostname_present',False)),
 ('GUARD_REPLACED',lambda d:d['guard']['guard'].__setitem__('sha256','0'*64)),
 ('ORIGINAL_PRIVACY_WAIVED',lambda d:d['preservation']['privacy'].__setitem__('current','PASS')),
 ('PROTECTED_SOURCE_LOSS',lambda d:d['preservation'].__setitem__('source_test_plan_pins',1352)),
 ('GATE_RELEASE',lambda d:d['entry']['gates'].__setitem__('P','YES'))]
for name,mutation in mutations:
    data=copy.deepcopy(source);mutation(data)
    try:verify(data)
    except (AssertionError,KeyError):results.append(dict(id=name,outcome='PASS_REJECTED'))
    else:raise AssertionError('NEGATIVE_CONTROL_NOT_REJECTED '+name)
for n,name in enumerate(['../traversal','/absolute','x/../path','x/NUL.txt','x/trailing.',chr(67)+':'+chr(92)+'drive']):
    try:safe_name(name)
    except AssertionError:results.append(dict(id='UNSAFE_PATH_'+str(n+1),outcome='PASS_REJECTED'))
    else:raise AssertionError('UNSAFE_PATH_NOT_REJECTED')
save('adverse-controls.json',dict(status='PASS',tests=results,executed=len(results),failed=0,positive_gate=VERDICT,source_rules_or_tests_changed=False))
print('Readiness/physical-scope adverse controls PASS:',len(results),'rejections',flush=True)
