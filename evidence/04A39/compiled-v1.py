"""Fresh per-configuration shipping metadata/CIL/exports and observational fixtures."""
from common39 import *
from collections import Counter
import shutil

source_lock();config=sys.argv[1];assert config in ['Debug','Release']
lib=PRIVATE/'shipping-final'/config
folder=HERE/'abi-cil'/config;folder.mkdir(exist_ok=False)
base=ROOT/'.test-rationalization-04A38.tmp/completion-w2-cont15'
commands=[]
for name,args in [('metadata',[str(base/'signatures-v1/MethodMetadata38.exe'),str(lib),str(folder/'current-v1.json'),
    str(ROOT/'evidence/04A38/completion/w2/continuation-v16/native-metadata-owners-v1.txt')]),
    ('exports',[str(base/'packet-v2/inspection/Inspect38.exe'),str(lib),str(folder/'exports-v1.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
    result=command(config+'-'+name,args);commands.append(result);assert result['exit_code']==0
current=load(folder/'current-v1.json');prior=load(W3/('abi-cil/review-metadata/'+config+'/current.json'))
normalize=lambda row:{k:v for k,v in row.items() if k!='raw_IL_sha256'}
before={x['method']:normalize(x) for x in prior['methods']};after={x['method']:normalize(x) for x in current['methods']}
assert len(before)==len(prior['methods']) and len(after)==len(current['methods'])
changes=[dict(identity=k,before=before.get(k),after=after.get(k)) for k in before.keys()|after.keys() if before.get(k)!=after.get(k)]
exports=load(folder/'exports-v1.json')['exports'];oldexports=load(W3/('abi-cil/review-metadata/'+config+'/exports.json'))['exports']
key=lambda v:v['effective_excel_name']
exact=sorted(exports,key=key)==sorted(oldexports,key=key)
save('abi-cil/'+config+'-comparison-v1.json',dict(configuration=config,commands=commands,
    source_lock=pin(HERE/'entry/source-preimages-v1.json'),accepted_W3_metadata=pin(W3/('abi-cil/review-metadata/'+config+'/current.json')),
    metadata=pin(folder/'current-v1.json'),exports=pin(folder/'exports-v1.json'),exports_count=len(exports),exact_exports=exact,
    method_count=len(after),missing=sorted(before.keys()-after.keys()),added=sorted(after.keys()-before.keys()),
    normalized_method_changes=changes,types_exact=current['types']==prior['types'],
    exception_callback_delegate_dependency_proof='Complete normalized per-method metadata includes signatures, attributes, locals, exceptions and ordered instructions/calls; comparison is config-to-same-config, not Debug IL versus Release IL.',
    actual_consumption='All accepted Windows→Core/Common compiled dependencies retained exactly; no production algorithm/parser/helper/reference was introduced.',
    raw_IL_changed=sum(x['raw_IL_sha256']!=y['raw_IL_sha256'] for x,y in zip(prior['methods'],current['methods']))))
assert not changes and current['types']==prior['types'] and exact and len(exports)==268,'UNEXPECTED_SHIPPING_CONTRACT_CHANGE'
fixture=PRIVATE/('fixtures-after-'+config+'-v1');fixture.mkdir(exist_ok=False)
for path in lib.glob('*.dll'):shutil.copy2(path,fixture/path.name)
exe=ROOT/load(HERE/'fixtures/frozen-input-manifest-v2.json')['driver_executable']['path']
shutil.copy2(exe,fixture/'Observe39.exe')
out=HERE/('fixtures/after-'+config+'-v1.json')
result=command(config+'-fixtures',[str(fixture/'Observe39.exe'),str(out)])
before_fixture=load(HERE/'fixtures/before-v2.json');observed=load(out)
assert len({x['id'] for x in observed['rows']})==len(observed['rows'])==77
save('fixtures/'+config+'-comparison-v1.json',dict(configuration=config,command=result,observed=pin(out),
    before=pin(HERE/'fixtures/before-v2.json'),cases=77,exact=before_fixture==observed,
    case_rows=[dict(id=a['id'],before_sha256=sha(json.dumps(a,sort_keys=True,ensure_ascii=False).encode()),
        after_sha256=sha(json.dumps(b,sort_keys=True,ensure_ascii=False).encode()),exact=a==b) for a,b in zip(before_fixture['rows'],observed['rows'])],
    cases_with_expected_current_exceptions=[x['id'] for x in observed['rows'] if x['outcome']=='EXCEPTION'],
    scientific='NOT_GOLDEN; no scientific-impact estimates executed',native='NOT_RUN_UNAUTHORIZED'))
assert result['exit_code']==0 and observed==before_fixture
source_lock();print(config,'metadata and77 current fixtures exact',flush=True)
