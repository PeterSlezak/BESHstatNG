from common42 import *
lock();r=runner().test('Debug','a42-wave-b3-v1','Legacy','a42-corrected-integration-adapters-v2','FullyQualifiedName~UdfPreparation38AdapterTests');save(PRIVATE/'corrected-adapters-v2.json',r);assert not r['outcomes'].get('Failed',0)
save('waves/corrected-adapters-v2.json',dict(outcomes=r['outcomes'],raw=pin(PRIVATE/'corrected-adapters-v2.json'),existing_tests_modified=False,failed_first_freeze_retained=pin(HERE/'matrix/failed-freeze-v1-disposition.json')))
print('Existing adapter integration tests PASS',r['outcomes'],flush=True)
