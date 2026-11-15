from common44 import *
lock();freeze=load(HERE/'matrix/source-freeze-v1.json');copies=[]
for r in freeze['source_delta']:
 p=PRIVATE/'source-preimages'/r['before']['path'];assert pin(p)['sha256']==r['before']['sha256'];dest=HERE/'source/preimages-v1'/r['before']['path'];dest.parent.mkdir(parents=True,exist_ok=True)
 with dest.open('xb') as f:f.write(p.read_bytes())
 copies.append(dict(original=r['before'],protected=pin(p),public_exact_source_copy=pin(dest)))
save('source/preimages-v1.json',dict(kind='EXACT_CURRENT_MAIN_SOURCE_PREIMAGES;NOT_RAW_EXECUTION_LOGS',base=MAIN,files=copies,added_sources=freeze['new_production'],added_tests=freeze['new_tests'],gates=GATES))
with (HERE/'source-review-v1.patch').open('xb') as f:f.write(git('diff','--no-ext-diff','--no-color','--',*[r['after']['path'] for r in freeze['source_delta']],freeze['plan']['path']))
sets={c:load(ROOT/load(HERE/('abi-cil/'+c+'-proof-v1.json'))['current_exports']['path'])['exports'] for c in ['Debug','Release']};assert sets['Debug']==sets['Release'] and len(sets['Debug'])==268
save('abi-cil/exports-review-v1.json',dict(kind='ONE_SHARED_DEBUG_RELEASE_268_EXPORT_METADATA',exports=sets['Debug'],exact_accepted43=True,configurations=['Debug','Release'],gates=GATES))
save('entry/review-authentication-v1.json',dict(kind='SANITIZED_AUTHENTICATION_FACTS;NOT_GIT_TRANSCRIPT',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,branch='codex/04A-44',entry_branch='main',index=pin('.git/index'),preexisting_owner_edits=[],accepted43=pin(ROOT/'.test-rationalization-04A43.tmp/publication/external-delivery-receipt-v1.json'),accepted43_postdelivery=pin(ROOT/'.test-rationalization-04A43.tmp/publication/postdelivery-git-preservation-v1.json'),raw40_exclusion_retained=True,plan_ids=226,plan_revision='2.21',sealed43_newline_transitions=len(load(HERE/'entry/authentication-v1.json')['merged43_newline_transitions']),physical_identity_continuity='NOT_VERIFIED',gates=GATES))
print('Seven existing source preimages,shared268 metadata and safe current-main authentication prepared',flush=True)
