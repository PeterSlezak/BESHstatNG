from common import *
v=(HERE/'publish_parts_v2.py').read_text().replace('from publication_privacy_c14_v2 import safe_path,privacy','from publication_privacy_c14_v2 import safe_path,privacy,stats')
v=v.replace("privacy_execution='One selected-payload policy checkpoint; exact policy/path/length/SHA may use previously verified physical decisions. Export/copy replay hashes decisions and every payload, avoiding repeated expensive policy campaigns.'", "privacy_execution='First full C14 checkpoint policy decisions reused only for exact path/size/SHA/base-policy. New corrective bookkeeping receives fresh delta evaluation. No full scanner/policy campaign repeated.'")
v=v.replace("files=len(rows),policy=policy,findings=findings+self_findings,seconds=", "files=len(rows),policy=policy,findings=findings+self_findings,exact_first_decision_reuse=stats['prior_exact'],fresh_delta_only=stats['fresh'],first_checkpoint=pin(W2/'continuation-v14-publication-privacy-receipt-v1.json'),seconds=")
with (HERE/'publish_parts_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
v="""from common import *
def execute(label,name,*args):
 r=command(label,[sys.executable,'-B',str(HERE/name),*args]);print(label,r['exit_code'],flush=True);assert r['exit_code']==0,'Preserve unsuccessful correction '+label
execute('publication-prepare-v2','publish_parts_v3.py','prepare')
execute('publication-delta-equivalence-v1','prove_publication_delta_v1.py')
save('publication-corrective-attempts-v1.json',dict(failed_copy_command=pin(RUNROOT/'commands/w2c14-copied-default-publication-reader-v1/command.json'),failed_copy_output=pin(RUNROOT/'commands/w2c14-copied-default-publication-reader-v1/stdout.log'),failed_union_command=pin(RUNROOT/'commands/w2c14-copied-union-v1/command.json'),correction=pin(HERE/'publication-safety-correction-v1.json'),scope='No source/reference/test change. First failed copy and every first archive retained. Corrected derivative has no partial .git metadata; identical index evidence is required separately.',original_queries=0))
# Final corrected inventory freezes every new public corrective tool/receipt.
execute('publication-inventory-delta-v2','publish_parts_v3.py','inventory')
execute('publication-export-v2','publish_parts_v3.py','export')
execute('copied-union-v2','copy_review_v3.py')
execute('final-live-v1','finalize_live_v3.py')
execute('capture-final-git-v1','capture_exit_git_v1.py')
print('Corrected publication/copy/live/Git complete. Generate private seal directly after this driver finishes.',flush=True)
"""
with (HERE/'finish_checkpoint_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(v)
