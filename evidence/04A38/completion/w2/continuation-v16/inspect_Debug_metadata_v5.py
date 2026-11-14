"""Add the actual adjacent native helper owner to the passive Debug census."""
from common import *
oldprivate=RUNROOT/'completion-w2-cont15';tool=oldprivate/'signatures-v1/MethodMetadata38.exe'
priorowners=PREVIOUS/'native-metadata-owners-v3.txt';owners=HERE/'native-metadata-owners-v1.txt'
original=priorowners.read_bytes();addition=b'BESHStatNG.regression.MixedModelFrontEndHelpers\n'
assert addition.strip() not in original
with owners.open('xb') as f:f.write(original+b'\n'+addition)
folder=HERE/'final-v5';folder.mkdir(exist_ok=False)
lib=PRIVATE/'combined-v3/Debug/lib'
r=command('final-Debug-metadata-v5',[str(tool),str(lib),str(folder/'current-Debug.json'),str(owners)]);assert r['exit_code']==0
old=json.loads((HERE/'final-v4/current-Debug.json').read_bytes());new=json.loads((folder/'current-Debug.json').read_bytes())
before={r['method']:r for r in old['methods']};after={r['method']:r for r in new['methods']}
assert all(after.get(k)==v for k,v in before.items()),'Previously captured Debug method bytes changed'
extra=[v for k,v in after.items() if k not in before]
assert any(v['declaring_type']=='BESHStatNG.regression.MixedModelFrontEndHelpers' and v['member_name']=='DefaultNames' for v in extra)
save('final-Debug-metadata-domain-v5.json',dict(status='EXACT_PRIOR_DEBUG_CENSUS_PLUS_REQUIRED_ADJACENT_OWNER',configuration='Debug',command=clean(r),tool=pin(tool),previous_metadata=pin(HERE/'final-v4/current-Debug.json'),current_metadata=pin(folder/'current-Debug.json'),previous_owner_list=pin(priorowners),current_owner_list=pin(owners),added_existing_records=extra,previous_records_unchanged=len(before),reason='DefaultNames remains physically shipped and used outside the selected UDF graph; its disappearance from a transitive-only capture is not a source/API removal. Enumerate the required whole native helper owner explicitly.',source_freeze=pin(HERE/'production-test-freeze-v3.json'),Release_ABI_CIL_signatures_exports=DEFERRED,original_queries=0))
print('Debug census domain extension',len(before),'unchanged records +',len(extra),'existing helper records',flush=True)
