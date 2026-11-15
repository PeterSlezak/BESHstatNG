from common44 import *
import shutil
lock()
for config in ['Debug','Release']:
    lib=PRIVATE/'entry-compiled'/config/'lib';lib.mkdir(parents=True)
    for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(PRIVATE/'builds/a44-entry-v1'/config/'legacy'/name,lib/name)
    tools=ROOT/'.test-rationalization-04A38.tmp/completion-w2-cont15';folder=lib.parent
    for name,args in [('metadata',[str(tools/'signatures-v1/MethodMetadata38.exe'),str(lib),str(folder/'current.json'),str(ROOT/'evidence/04A38/completion/w2/continuation-v16/native-metadata-owners-v1.txt')]),('exports',[str(tools/'packet-v2/inspection/Inspect38.exe'),str(lib),str(folder/'exports.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
        r=command('entry-'+config+'-'+name,args);assert r['exit_code']==0
    prior=load(ROOT/('evidence/04A43/abi-cil/'+config+'-proof-v1.json'));exact(prior['current_exports']);exact(prior['current_metadata'])
    norm=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
    assert [norm(r) for r in load(folder/'current.json')['methods']]==[norm(r) for r in load(ROOT/prior['current_metadata']['path'])['methods']]
    assert load(folder/'exports.json')['exports']==load(ROOT/prior['current_exports']['path'])['exports']
    save('entry/'+config+'-compiled-v1.json',dict(metadata=pin(folder/'current.json'),exports=pin(folder/'exports.json'),accepted43=pin(ROOT/('evidence/04A43/abi-cil/'+config+'-proof-v1.json')),all_normalized_methods_and268_exports_exact=True))
print('Debug/Release entry compiled bodies and268 exports match accepted43',flush=True)
