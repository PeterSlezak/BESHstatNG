"""Additive readback: generated closure ordinals are not public method identities."""
from common40_v3 import *
import re,collections,shutil
lock();config=sys.argv[1];folder=PRIVATE/'abi-cil'/config
if not (folder/'current.json').exists():
    # Initial reader/build recipe only; do not repeat any existing extraction.
    import runpy
    try:runpy.run_path(str(HERE/'compiled-final-v1.py'),run_name='__main__')
    except AssertionError:pass
now=load(folder/'current.json');old=load(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json'))
def canonical(value):
    if isinstance(value,dict):return {k:canonical(v) for k,v in value.items() if k!='raw_IL_sha256'}
    if isinstance(value,list):return [canonical(v) for v in value]
    if isinstance(value,str):
        value=re.sub(r'_Closure\$__(\d+)-(\d+)',r'_Closure$__ORD-\2',value)
        value=re.sub(r'_Lambda\$__(\d+)-(\d+)',r'_Lambda$__ORD-\2',value)
        value=re.sub(r'\$I(\d+)-(\d+)',r'$IORD-\2',value)
        return value
    return value
def key(r):return json.dumps(canonical(r),sort_keys=True,ensure_ascii=False)
b=collections.Counter(map(key,old['methods']));a=collections.Counter(map(key,now['methods']))
removed=list((b-a).elements());added=list((a-b).elements())
changes=dict(removed=[json.loads(x) for x in removed],added=[json.loads(x) for x in added])
save(PRIVATE/('abi-'+config+'-canonical-changes-v3.json'),changes)
allowed=['ConfigureUdfGlmFit','ConfigureUdfFit','ConfigureUdfOrdinalFit','ConfigureUdfMultinomialFit','ApplyZipFitSettings','CreateAcquiredModel','ApplyInference',
 'LM_FIT','LM_ANOVA_TYPEI','GLM_FIT','GLMNB_FIT','ZIP_FIT','ORDLOGIT_FIT','MLOGIT_FIT','MNLOGIT_FIT','MULTINOM_FIT','MULTILOGIT_FIT','MULTINOMIAL_FIT','GEE_FIT','COX_FIT','LMM_FIT','CreateCoreModel']
unexpected=[(direction,r['method']) for direction,rows in changes.items() for r in rows if r['member_name'] not in allowed]
save(PRIVATE/('abi-'+config+'-unexpected-v3.json'),unexpected)
assert not unexpected,'UNEXPECTED_COMPILED_CHANGE_SEE_PRIVATE_READBACK'
exports=load(folder/'exports.json')['exports'];prior=load(ROOT/('evidence/04A39/abi-cil/'+config+'/exports-v1.json'))['exports']
assert exports==prior and len(exports)==268
save('abi-cil/'+config+'-proof-v1.json',dict(configuration=config,exports=268,exact_export_order_names_signatures_attributes=True,
 current_metadata=pin(folder/'current.json'),current_exports=pin(folder/'exports.json'),baseline=pin(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json')),
 changed_methods=[dict(direction=k,method=r['method']) for k,rows in changes.items() for r in rows],unexpected=[],
 generated_identity_rule='Compiler ordinals only: closure leading scope number, lambda leading scope number, matching cache field number. Full canonical record multiset retains suffix, bodies, signatures, locals, exceptions, call/delegate/dependency edges; no statistical data or instruction omitted. Raw records preserved.',
 assembly_hashes=[pin(p) for p in sorted((PRIVATE/config/'lib').glob('*.dll'))],owner_metadata_transition='BuildDate owner commit pinned separately.'))
obs=PRIVATE/('observe39-'+config);obs.mkdir()
for p in (PRIVATE/config/'lib').glob('*.dll'):shutil.copy2(p,obs/p.name)
oldexe=ROOT/load(ROOT/'evidence/04A39/fixtures/frozen-input-manifest-v2.json')['driver_executable']['path'];shutil.copy2(oldexe,obs/'Observe39.exe')
receipt=command('final-'+config+'-39fixtures',[str(obs/'Observe39.exe'),str(obs/'observations.json')]);assert receipt['exit_code']==0
before=load(ROOT/'evidence/04A39/fixtures/before-v2.json');after=load(obs/'observations.json');assert before==after and len(after['rows'])==77
save('fixtures/'+config+'-39-retained-inputs-fresh-results-v1.json',dict(cases=77,exact=True,original39=pin(ROOT/'evidence/04A39/fixtures/before-v2.json'),observed=pin(obs/'observations.json'),cases_provenance='Unmodified39 current-behavior inputs, fresh40 results; not historical-original/scientific oracle.'))
print(config,'268 exact exports; compiled closure multiset reconciled;77 exact39 fixtures',flush=True)
