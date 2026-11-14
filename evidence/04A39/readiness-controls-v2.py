"""Actual fail-closed reader adverse cases, no campaigns or expected-result changes."""
from common39 import *
import copy
import importlib.util

spec=importlib.util.spec_from_file_location('readiness_reader39',HERE/'verify-readiness-v2.py')
reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
base=reader.verify(ROOT)
tests=[]
def reject(name,path,change,compiled=False):
    key='evidence/04A39/'+path
    value=copy.deepcopy(load(ROOT/key));change(value)
    try:reader.verify(ROOT,{key:value},live=False,compiled=compiled)
    except (AssertionError,KeyError,ValueError) as error:tests.append(dict(id=name,outcome='PASS_REJECTED',reason=str(error)[:250]))
    else:raise AssertionError('ADVERSE_NOT_REJECTED '+name)
reject('SOURCE_UNIVERSE_LOST','entry/source-preimages-v1.json',lambda x:x['files'].pop())
reject('LOST_FOCUS_MEMBER','map/method-policy-map-v1.json',lambda x:x['methods'].pop())
reject('LOST_DEPENDENCY_EDGE','map/method-policy-map-v1.json',lambda x:next(m for m in x['methods'] if m['all_calls'])['all_calls'].pop())
reject('MISSING_MANAGED_IDENTITY','build-and-tests/Debug-identities-v1.json',lambda x:x['tests'].pop(0))
reject('DUPLICATE_MANAGED_IDENTITY','build-and-tests/Debug-identities-v1.json',lambda x:x['tests'].append(copy.deepcopy(x['tests'][0])))
reject('CHANGED_MANAGED_MESSAGE','build-and-tests/Debug-identities-v1.json',lambda x:x['nonpasses'][0].update(message_sha256='0'*64))
reject('INHERITED_FAIL_MARKED_PASS','build-and-tests/Debug-identities-v1.json',lambda x:next(r for r in x['tests'] if r['outcome']=='Failed').update(outcome='Passed'))
reject('RELEASE_BUILD_NONPASS','build-and-tests/Release-build-v4.json',lambda x:x['exit_codes'].__setitem__(0,1))
reject('CHANGED_EXPORT_COUNT','abi-cil/Release-comparison-v2.json',lambda x:x.update(exports_count=267))
reject('COMPILED_INSTRUCTION_CHANGED','abi-cil/Release/current-v1.json',lambda x:next(r for r in x['methods'] if r.get('instructions'))['instructions'].pop(),True)
reject('FROZEN_CASE_LOST','compatibility/Release-retained-v1.json',lambda x:x['case_identities'].pop())
reject('FROZEN_CASE_DUPLICATED','compatibility/Debug-retained-v1.json',lambda x:x['case_identities'].__setitem__(0,copy.deepcopy(x['case_identities'][1])))
reject('FROZEN_ORDER_CHANGED','compatibility/Debug-retained-v1.json',lambda x:x['case_identities'].reverse())
reject('ORIGINAL_REFERENCE_EXECUTION_CONTAMINATION','compatibility/Release-retained-v1.json',lambda x:x.update(original_queries=1))
reject('SUPPLEMENT_RELABELLED','compatibility/Release-retained-v1.json',lambda x:x.update(PRE_EDIT_FROZEN=29424,LATE_ARCHIVED_ENTRY_SUPPLEMENT=0))
reject('PRECISION_BEHAVIOR_CHANGED','fixtures/after-Release-v1.json',lambda x:x['rows'].pop())
reject('PYTHON_PARENT_MISSING','python-node-scanners/python-identities-v1.json',lambda x:x['parents'].pop())
reject('PYTHON_ERROR_ERASED','python-node-scanners/python-identities-v1.json',lambda x:next(e for e in x['events'] if e['outcome']=='ERROR').update(outcome='PASS'))
reject('PRIVACY_ERROR_WAIVED','python-node-scanners/python-delta-v2.json',lambda x:x.update(historical_privacy_errors_waived=True))
reject('NODE_IDENTITY_MISSING','python-node-scanners/node-v1.json',lambda x:x['suites'][0]['identities'].pop())
reject('STRICT_NONPASS_GREEN','python-node-scanners/serial-v1.json',lambda x:x['results'][0]['result'].update(exit_code=0))
reject('RESTRICTED_GATE_RELEASE','entry/merge-preflight-v1.json',lambda x:x['gates'].update(P='YES'))
assert base['verdict']=='NOT_READY_04A39' and base['historical_privacy_errors_unwaived']==2
reject('CHANGED_PYTHON_OUTCOMES_HIDDEN','python-node-scanners/python-delta-v2.json',lambda x:x.update(changes=[]))
save('final/readiness-controls-v2.json',dict(actual=base,reader=pin(HERE/'verify-readiness-v2.py'),controls=tests,
    register_controls=pin(HERE/'register/validation-v5.json'),campaign_repeats=0))
print('Readiness adverse controls',len(tests),'passed',flush=True)
