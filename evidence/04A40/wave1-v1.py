from common40_v3 import *
lock();run=runner();result=run.build('Debug','a40-wave1-v1')
save(PRIVATE/'wave1-Debug-build-v1.json',result)
assert not any(result['exit_codes']) and result['source_unchanged']
test=run.test('Debug','a40-wave1-v1','Common','a40-wave1-Common','FullyQualifiedName~Regression40FitPolicyTests')
save(PRIVATE/'wave1-Common-tests-v1.json',test)
save('waves/wave1-v1.json',dict(build_exit_codes=result['exit_codes'],source_unchanged_during_build=result['source_unchanged'],
    raw_build=pin(PRIVATE/'wave1-Debug-build-v1.json'),raw_tests=pin(PRIVATE/'wave1-Common-tests-v1.json'),scope='Focused new Common fit-options tests only; full Debug scheduled once after final source freeze.'))
assert test['exit_code']==0
lock()
