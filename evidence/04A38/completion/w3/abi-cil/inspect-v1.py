"""Fresh passive shipping metadata/export inspection; originals are data only."""
from common import *
from collections import Counter

lock()
config=sys.argv[1]
assert config in ['Debug','Release']
folder=PRIVATE/config/'metadata'
folder.mkdir(exist_ok=False)
lib=PRIVATE/config/'lib'
tool=RUNROOT/'completion-w2-cont15/signatures-v1/MethodMetadata38.exe'
exports_tool=RUNROOT/'completion-w2-cont15/packet-v2/inspection/Inspect38.exe'
results=[]
for name,args in [('metadata',[str(tool),str(lib),str(folder/'current.json'),str(W2/'continuation-v16/native-metadata-owners-v1.txt')]),
                  ('exports',[str(exports_tool),str(lib),str(folder/'exports.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
    r=command(config+'-'+name,args)
    results.append(r)
    assert r['exit_code']==0
data=load(folder/'current.json')
exports=load(folder/'exports.json')['exports']
baseline=load(ROOT/('evidence/04A38/preedit/compiled-'+config+'.json'))['exports']
match=sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(baseline,key=lambda r:r['effective_excel_name'])
report=dict(configuration=config,commands=results,tools=[pin(tool),pin(exports_tool)],
    metadata=pin(folder/'current.json'),exports=pin(folder/'exports.json'),exports_count=len(exports),
    sealed_original_export_baseline=pin(ROOT/('evidence/04A38/preedit/compiled-'+config+'.json')),
    exact_exports=match,methods=len(data['methods']),types=len(data['types']),
    method_schema=list(data['methods'][0]),original_semantic_calls=data['original_semantic_calls'],
    source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'))
if config=='Debug':
    old=load(ACCEPTED/'final-v1/current-Debug.json')
    semantic=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
    report['accepted_Debug_types_exact']=data['types']==old['types']
    report['accepted_Debug_normalized_CIL_exact']=[semantic(r) for r in data['methods']]==[semantic(r) for r in old['methods']]
    report['raw_IL_differences']=sum(x.get('raw_IL_sha256')!=y.get('raw_IL_sha256') for x,y in zip(data['methods'],old['methods']))
save('abi-cil/'+config+'-inspection-v1.json',report)
print(config,'methods',len(data['methods']),'types',len(data['types']),'exports',len(exports),'exact exports',match,'schema',report['method_schema'],flush=True)
assert len(exports)==268 and match,'UNEXPECTED_EXPORT_CHANGE'
if config=='Debug':
    assert report['accepted_Debug_types_exact'] and report['accepted_Debug_normalized_CIL_exact'],'UNEXPECTED_DEBUG_CIL_CHANGE'
lock()
