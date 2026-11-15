from common43 import *
lock();assert load(HERE/'fixtures/pre-edit-current-main-v1.json')['kind']=='CURRENT_MAIN_PRE_EDIT_04A43'
assert load(HERE/'fixtures/pre-edit-races-v1.json')['rows']==864
result=runner().build('Debug','a43-wave-a-v1');save(PRIVATE/'wave-a-build-v1.json',result);assert not any(result['exit_codes']) and result['source_unchanged']
r=runner().test('Debug','a43-wave-a-v1','Common','a43-wave-a-Common-v1','FullyQualifiedName~UdfHandlePolicyTests');save(PRIVATE/'wave-a-tests-v1.json',r);assert not r['outcomes'].get('Failed',0)
save('waves/wave-a-v1.json',dict(build_exits=result['exit_codes'],tests=r['outcomes'],software='Host-free policy only;no live invalidation/caches/events activated',raw=pin(PRIVATE/'wave-a-tests-v1.json'),gates=GATES))
print('Wave A host-free descriptor/key/lifecycle policy tests PASS',r['outcomes'],flush=True)
