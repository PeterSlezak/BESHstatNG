from common46 import *
_original_command=command
def command(name,args,**kw):return _original_command("final-"+name,args,**kw)
import shutil,collections,copy
lock();config=sys.argv[1];assert config in ['Debug','Release']
freeze=load(HERE/'matrix/source-freeze-v1.json');assert runner().__class__.build.__globals__['source_pins']()==freeze['source_pins']
label='a46-final-v1'
lib=PRIVATE/'compiled'/config/'lib';lib.mkdir(parents=True)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(PRIVATE/'builds'/label/config/'legacy'/name,lib/name)
folder=PRIVATE/'abi-cil'/config;folder.mkdir(parents=True)
tools=ROOT/'.test-rationalization-04A38.tmp/completion-w2-cont15'
for name,args in [('metadata',[str(tools/'signatures-v1/MethodMetadata38.exe'),str(lib),str(folder/'current.json'),str(ROOT/'evidence/04A38/completion/w2/continuation-v16/native-metadata-owners-v1.txt')]),('exports',[str(tools/'packet-v2/inspection/Inspect38.exe'),str(lib),str(folder/'exports.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
    result=command('compiled-'+config+'-'+name,args);assert result['exit_code']==0
oldproof=load(ROOT/('evidence/04A45/abi-cil/'+config+'-proof-v1.json'));exact(oldproof['current_metadata']);exact(oldproof['current_exports'])
normalize=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
old=load(ROOT/oldproof['current_metadata']['path']);now=load(folder/'current.json')
before={r['method']:normalize(r) for r in old['methods']};after={r['method']:normalize(r) for r in now['methods']}
changes=[dict(method=k,before=before.get(k),after=after.get(k)) for k in before.keys()|after.keys() if before.get(k)!=after.get(k)]
owners=['AppInfrastructure.AppGlobals','AppInfrastructure.WinFormsProgressReporter','AppInfrastructure.WinFormsUserMessageService','BESHStatNG.UIprocedures','ExcelCommon.PreferenceSnapshot','ExcelCommon.ExecutionSnapshot','ExcelCommon.UserFaultMapper','ExcelCommon.NumberTextPolicy']
unexpected=[r['method'] for r in changes if not any(owner in r['method'] for owner in owners)]
save(PRIVATE/('compiled-method-differences-'+config+'.json'),dict(changes=changes,unexpected=unexpected))
assert not changes,'UNEXPECTED_PRODUCTION_COMPILED_METHOD_CHANGE'
exports=load(folder/'exports.json')['exports'];previous=load(ROOT/oldproof['current_exports']['path'])['exports'];assert exports==previous and len(exports)==268
save('abi-cil/'+config+'-proof-v1.json',dict(configuration=config,exports=268,exact_export_order_names_signatures_attributes=True,baseline42=pin(ROOT/('evidence/04A45/abi-cil/'+config+'-proof-v1.json')),current_metadata=pin(folder/'current.json'),current_exports=pin(folder/'exports.json'),assembly_hashes=[pin(p) for p in sorted(lib.glob('*.dll'))],changed_methods=sorted(r['method'] for r in changes),unexpected=unexpected,removed=sorted(before.keys()-after.keys()),new=sorted(after.keys()-before.keys()),all_other_compiled_methods_exact=True,Core_scientific_source_unchanged=True,complete_change_records=pin(PRIVATE/('compiled-method-differences-'+config+'.json')),gates=GATES))
def execute_fixture(label,exe,arguments,original):
    target=PRIVATE/('fixture-final-'+config+'-'+label);target.mkdir()
    for p in lib.glob('*.dll'):shutil.copy2(p,target/p.name)
    shutil.copy2(exe,target/exe.name)
    result=command('fixture-final-'+config+'-'+label,[str(target/exe.name),str(target/'observations.json'),*arguments]);assert result['exit_code']==0
    return load(original),load(target/'observations.json'),pin(target/'observations.json')
a,b,current=execute_fixture('41',ROOT/'.test-rationalization-04A41.tmp/fixtures-before-v1/Observe41.exe',[],ROOT/'.test-rationalization-04A41.tmp/fixtures-before-v1/observations.json')
assert a==b and len(a['rows'])==390
save('fixtures/'+config+'-41-v1.json',dict(cases=390,exact=True,kind='MATERIALIZED_CURRENT_MAIN_PRE_EDIT41_COMPATIBILITY;NOT_GOLDEN',baseline=pin(ROOT/'.test-rationalization-04A41.tmp/fixtures-before-v1/observations.json'),current=current,original_late_capture=False))
prior39=load(ROOT/'evidence/04A39/fixtures/frozen-input-manifest-v2.json')
a,b,current=execute_fixture('39',ROOT/prior39['driver_executable']['path'],[],ROOT/'evidence/04A39/fixtures/before-v2.json')
assert a==b and len(a['rows'])==77
save('fixtures/'+config+'-39-v1.json',dict(cases=77,exact=True,baseline=pin(ROOT/'evidence/04A39/fixtures/before-v2.json'),current=current))
a,b,current=execute_fixture('40',ROOT/'.test-rationalization-04A40.tmp/fixtures-before-v2/Observe40.exe',['policy'],ROOT/'.test-rationalization-04A40.tmp/before-v2.json')
timing=[]
for x,y in zip(a['rows'],b['rows']):
    assert x['id']==y['id'];left=copy.deepcopy(x);right=copy.deepcopy(y);removed=False
    for row in [left,right]:
        state=(row.get('output') or {}).get('state',{})
        if 'GEE.CompTime' in state:state.pop('GEE.CompTime');removed=True
    assert left==right,'RETAINED40_CONTRACT_CHANGED'
    if removed:timing.append(x['id'])
assert len(a['rows'])==48
save('fixtures/'+config+'-40-v1.json',dict(cases=48,statistical_input_exception_contracts_exact=True,baseline=pin(ROOT/'.test-rationalization-04A40.tmp/before-v2.json'),current=current,elapsed_only_cases=timing,excluded_field='GEE.CompTime;unchanged Stopwatch source;not a statistical difference',scientific='NOT_GOLDEN'))
print(config,'268 exports/CIL scope PASS;390+77 exact;48 contracts exact with disclosed elapsed fields',flush=True)

sys.path.insert(0,str(ROOT/'evidence/04A42'))
compare42=module('compare42',ROOT/'evidence/04A42/verify-wave-v1.py')
a,b,current=execute_fixture('42',ROOT/'.test-rationalization-04A42.tmp/fixtures-before-v1/Observe42.exe',[],ROOT/'.test-rationalization-04A42.tmp/fixtures-before-v1/observations.json')
assert compare42.canonical(a)==compare42.canonical(b) and len(a['rows'])==1802
save('fixtures/'+config+'-42-v1.json',dict(cases=1802,typed_statistical_shape_error_order_exact=True,baseline=pin(ROOT/'.test-rationalization-04A42.tmp/fixtures-before-v1/observations.json'),current=current,only_excluded_runtime_fields=['Generated session GUID strings','Created UTC timestamp strings'],scientific='NOT_GOLDEN'))

