"""Fresh shipping export/CIL evidence, preserving authorized source deltas explicitly."""
from common40_v3 import *
import shutil
lock();config=sys.argv[1]
label='a40-wave2-v2' if config=='Debug' else 'a40-final-v1'
lib=PRIVATE/config/'lib';lib.mkdir(parents=True)
for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
    shutil.copy2(PRIVATE/'builds'/label/config/'legacy'/name,lib/name)
folder=PRIVATE/'abi-cil'/config;folder.mkdir(parents=True)
oldtools=ROOT/'.test-rationalization-04A38.tmp/completion-w2-cont15'
for name,args in [('metadata',[str(oldtools/'signatures-v1/MethodMetadata38.exe'),str(lib),str(folder/'current.json'),str(ROOT/'evidence/04A38/completion/w2/continuation-v16/native-metadata-owners-v1.txt')]),
 ('exports',[str(oldtools/'packet-v2/inspection/Inspect38.exe'),str(lib),str(folder/'exports.json'),str(ROOT/'evidence/04A38/entry-members.json')])]:
    receipt=command('final-'+config+'-'+name,args);save(folder/(name+'-command.json'),receipt);assert receipt['exit_code']==0
now=load(folder/'current.json');old=load(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json'))
normalize=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
before={r['method']:normalize(r) for r in old['methods']};after={r['method']:normalize(r) for r in now['methods']}
changes=[dict(method=k,before=before.get(k),after=after.get(k)) for k in before.keys()|after.keys() if before.get(k)!=after.get(k)]
exports=load(folder/'exports.json')['exports'];prior=load(ROOT/('evidence/04A39/abi-cil/'+config+'/exports-v1.json'))['exports']
assert exports==prior and len(exports)==268
assert not before.keys()-after.keys()
allowed=['.ConfigureUdfGlmFit(','.ConfigureUdfFit(','.ConfigureUdfOrdinalFit(','.ConfigureUdfMultinomialFit(','.ApplyZipFitSettings(','.CreateAcquiredModel(','.ApplyInference(',
 '.LM_FIT(','.GLM_FIT(','.GLMNB_FIT(','.ZIP_FIT(','.ORDLOGIT_FIT(','.MLOGIT_FIT(','.MULTINOM_FIT(','.MULTILOGIT_FIT(','.GEE_FIT(','.COX_FIT(','.LMM_FIT(','.CreateCoreModel(']
unexpected=[r['method'] for r in changes if not any(token in r['method'] for token in allowed)]
save(PRIVATE/('abi-'+config+'-changes.json'),dict(changes=changes,unexpected=unexpected))
assert not unexpected,'UNEXPECTED_COMPILED_METHOD_CHANGE'
save('abi-cil/'+config+'-proof-v1.json',dict(configuration=config,exports=268,exact_export_order_names_signatures_attributes=True,
    current_metadata=pin(folder/'current.json'),current_exports=pin(folder/'exports.json'),baseline=pin(ROOT/('evidence/04A39/abi-cil/'+config+'/current-v1.json')),
    changed_methods=[r['method'] for r in changes],added=sorted(after.keys()-before.keys()),removed=[],unexpected=[],
    instructions_exception_callback_delegate_dependency_edges='Complete per-method records preserved; changed methods limited to typed option/request policy and nine UDF parents. Core statistical methods unchanged.',
    assembly_hashes=[pin(p) for p in sorted(lib.glob('*.dll'))],owner_metadata_transition='Windows BuildDate owner commit pinned separately; not numerical/ABI rebaseline.'))
obs=PRIVATE/('observe39-'+config);obs.mkdir()
for p in lib.glob('*.dll'):shutil.copy2(p,obs/p.name)
oldexe=ROOT/load(ROOT/'evidence/04A39/fixtures/frozen-input-manifest-v2.json')['driver_executable']['path'];shutil.copy2(oldexe,obs/'Observe39.exe')
receipt=command('final-'+config+'-39fixtures',[str(obs/'Observe39.exe'),str(obs/'observations.json')]);assert receipt['exit_code']==0
before=load(ROOT/'evidence/04A39/fixtures/before-v2.json');after=load(obs/'observations.json');assert before==after and len(after['rows'])==77
save('fixtures/'+config+'-39-retained-inputs-fresh-results-v1.json',dict(cases=77,exact=True,original39=pin(ROOT/'evidence/04A39/fixtures/before-v2.json'),observed=pin(obs/'observations.json'),
    cases_provenance='Unmodified39 current-behavior fixture inputs; fresh40 configuration results; not historical-original/scientific oracle.'))
print(config,'268 exact exports; expected compiled option-policy changes;77 fresh current39 fixtures exact',flush=True)
