from common42 import *
import shutil
lock();freeze=load(HERE/'matrix/source-freeze-v2.json');preimages=[]
for r in freeze['source_delta']:
    src=PRIVATE/'source-preimages'/r['before']['path'];assert pin(src)['sha256']==r['before']['sha256']
    dest=HERE/'source/preimages-v1'/r['before']['path'];dest.parent.mkdir(parents=True,exist_ok=True)
    with dest.open('xb') as f:f.write(src.read_bytes())
    preimages.append(dict(original=r['before'],protected=pin(src),public_exact_source_copy=pin(dest)))
save('source/preimages-v1.json',dict(kind='EXACT_TRACKED_ACCEPTED_MAIN_SOURCE_PREIMAGES;NOT_EXECUTION_LOGS',files=preimages,base=MAIN,source_only=True))
patch=git('diff','--no-ext-diff','--no-color','--',*[r['after']['path'] for r in freeze['source_delta']],freeze['plan']['path'])
with (HERE/'source-review-v1.patch').open('xb') as f:f.write(patch)
for config in ['Debug','Release']:
    proof=load(HERE/('abi-cil/'+config+'-proof-v3.json'));exact(proof['current_exports']);exports=load(ROOT/proof['current_exports']['path'])['exports'];assert len(exports)==268
    save('abi-cil/'+config+'-exports-review-v1.json',dict(kind='PUBLIC_COMPILED_EXPORT_REVIEW;NOT_RAW_METADATA_LOG',exports=exports,configuration=config,raw=proof['current_exports'],exact_accepted41=True))
s=(HERE/'serial-checks-v1.py').read_text(encoding='utf-8-sig').replace('a42-final-serial-v1','a42-final-serial-v2').replace('serial-checks-raw-v1.json','serial-checks-raw-v2.json').replace('matrix/serial-checks-v1.json','matrix/serial-checks-v2.json')
with (HERE/'serial-checks-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('entry/review-authentication-v1.json',dict(kind='SANITIZED_AUTHENTICATION_FACTS;NOT_RAW_GIT_TRANSCRIPT',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,branch='codex/04A-42',entry_branch='main',index=pin('.git/index'),preexisting_owner_edits=[],accepted41=pin(ROOT/'.test-rationalization-04A41.tmp/publication/external-delivery-receipt-v1.json'),raw40_exclusion_retained=True,plan_ids=226,plan_revision='2.19',source_preimage=pin(HERE/'entry/source-preimages-v1.json'),sealed41_newline_transitions=len(load(HERE/'entry/authentication-v1.json')['merged41_newline_transitions']),physical_identity_continuity='NOT_VERIFIED',gates=GATES))
print('Eight exact accepted-main source preimages,patch and268 export review records prepared',flush=True)
