from common40_v3 import *
test=load(PRIVATE/'wave1-Common-tests-v2.json')
assert test['command']['exit_code']==0 and test['outcomes']=={'Passed':6} and len(test['records'])==6
save('waves/wave1-readback-v3.json',dict(status='SIX_FOCUSED_TESTS_PASS',raw=pin(PRIVATE/'wave1-Common-tests-v2.json'),
    command_exit=0,executed=6,outcomes=test['outcomes'],prior_reporting_error='Original reader addressed exit_code at wrong nesting; build and6 tests already passed. No execution repeated.'))
print('Wave1 actual6 PASS; report-only receipt nesting corrected without rerun',flush=True)
