"""Actual final C12 seal/private/physical/live replay before any implementation."""
from common import *
from concurrent.futures import ThreadPoolExecutor

def check(row):assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']},'ENTRY_PIN_MISMATCH_STOP '+row['path']
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
    result=command('entry-git-'+name+'-v1',['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args])
    folder=RUNROOT/('commands/w2c13-entry-git-'+name+'-v1');out=(folder/'stdout.log').read_bytes()
    assert result['exit_code']==0
    if name=='branch':assert out.decode().strip()=='codex/04A-38'
    if name in ['HEAD','main','merge-base']:assert out.decode().strip()==BASE,'BASE_MISMATCH_STOP'
    if name=='index':assert not out,'NONEMPTY_INDEX_STOP'
    checks.append(dict(check=name,execution=clean(result),raw_command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')))
index=pin(ROOT/'.git/index');assert index['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
save('git-entry-v1.json',dict(checks=checks,index_file=index,base=BASE,production_edits=0))
final_path=W2/'continuation-v12-final-git-preservation-v1.json'
assert pin(final_path)['sha256']=='8acd2c578c247744b252bccc9b3fc88166a1a662d1ae34498b789a52c58109d3','FINAL_V12_AUTHORITY_MISMATCH_STOP'
final=json.loads(final_path.read_bytes());authorities=[pin(final_path)]
for key in ['private_preservation','private_verification','live_preservation','publication','privacy_checkpoint','parts_index','copied_reader','post_publication_artifacts','postseal_addendum','postseal_tool','superseded_binding','previous_provisional_artifacts','previous_provisional_addendum']:
    check(final[key]);authorities.append(final[key])
assert final['postseal_addendum']['path'].endswith('continuation-v12-postseal-handover-addendum-v2.md')
assert final['post_publication_artifacts']['path'].endswith('continuation-v12-post-publication-artifacts-v2.json')
for record in final['Git']:
    assert record['exit_code']==0
    for key in ['actual_current_raw_command','actual_current_raw_stdout','actual_current_raw_stderr']:check(record[key]);authorities.append(record[key])
    assert '/w2c12-exit-git-' in record['actual_current_raw_command']['path'],'STALE_GIT_BINDING_STOP'
artifacts=json.loads((ROOT/final['post_publication_artifacts']['path']).read_bytes())
for key in ['checkpoint_reader','publication_reader','checkpoint_seal','sealed_review','sealed_handover','authoritative_private_reader','authoritative_private_preservation','private_reader_verification','current_synthetic_phase','unsupported_closure_control','accounting_correction','prior_finalizer_source','previous_provisional_artifacts','exit_git_correction','finalization_tool']:
    check(artifacts[key]);authorities.append(artifacts[key])
for row in artifacts['source_after_images']+artifacts['current_compiled_artifacts']:check(row)
private=json.loads((ROOT/final['private_preservation']['path']).read_bytes())
assert private['private_pin_count']==len(private['private_pins'])==8801 and private['inherited_private_count']==5661 and private['new_current_private_count']==3140
assert len(private['retained_prior_physical_archives'])==45
giant=private['retained_interrupted_aggregation'];assert (giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
modified=git('diff','--name-only','-z').decode().split('\0')
untracked=git('ls-files','--others','--exclude-standard','-z','--','BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG','BESHStatNG.Test').decode().split('\0')
rows=[]
for name in sorted(set(modified+untracked)-{''}):
    source=ROOT/name;target=HERE/'source-before'/(name+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write(source.read_bytes())
    rows.append(dict(live=pin(source),preimage=pin(target)))
save('all-edited-source-preimages-v1.json',dict(files=rows,count=len(rows),scope='Every tracked diff and every untracked source/test project path. No historical ignored TestResults traversal.'))
for path in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md']:
    (ROOT/path).read_bytes().decode('utf-8-sig');authorities.append(pin(ROOT/path))
names=['source-transitions-final-v1.json','canonical-identity-status-v1.json','new-closures-final-v1.json','remaining-work-queue-v1.json','historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json','current-semantic-position-crosswalk-v1.json','original-fragment-parent-crosswalk-v2.json','new-parent-fragment-crosswalk-v1.json','support-fragment-crosswalk-v1.json','whole-body-contracts-v1.json','callback-proofs-v1.json','portable-api-contracts-v1.json','host-cache-contexts-v1.json','constructed-owner-proofs-v1.json','compiled-method-deltas-v1.json','managed-Debug-identities-v1.json','python-first-failures-v2.json','python-nonpass-investigation-v1.json','node-identities-v1.json','validation-v1.json','compatibility-packet-v1.json','frozen-provenance-v1.json','build-packet-v1-Debug.json','build-packet-v1-Release.json']
for name in names:
    path=PREVIOUS/name;doc=json.loads(path.read_bytes());authorities.append(pin(path))
save('entry-authority-pins-v1.json',dict(authorities=authorities,source_after_images=artifacts['source_after_images'],current_compiled_artifacts=artifacts['current_compiled_artifacts'],original_queries=0))
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
tasks=[('prior-sealed-reader-v2',PREVIOUS,PREVIOUS/'verify_c12_checkpoint_v2.py',[]),('prior-private-reader-v1',PREVIOUS,W2/'continuation-v12-retained-private-reader-v1.py',['--verify']),('prior-live-preservation-v1',PREVIOUS,PREVIOUS/'verify_c12_preservation_v1.py',[]),('prior-physical-union-v1',PREVIOUS,PREVIOUS/'verify_c12_publication_v1.py',['--parts',str(W2/'continuation-v12-review-parts-index-v1.json')])]
def execute(task):
    label,directory,script,args=task
    return label,command(label,[sys.executable,'-I','-B','-c',bootstrap,str(directory),str(script),*args])
with ThreadPoolExecutor(max_workers=3) as pool:results=list(pool.map(execute,tasks))
outcomes={name:result['exit_code'] for name,result in results}
save('entry-executable-authentication-v1.json',dict(status='AUTHENTICATED' if all(v==0 for v in outcomes.values()) else 'ENTRY_AUTHENTICATION_FAILED_STOP',executions=[dict(label=name,result=clean(result)) for name,result in results],production_edits=0,original_queries=0,gates='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED'))
assert all(v==0 for v in outcomes.values()),'ENTRY_AUTHENTICATION_FAILED_STOP '+str(outcomes)
print(json.dumps(dict(status='AUTHENTICATED_PRE_IMPLEMENTATION',sources=len(rows),outcomes=outcomes,original_queries=0)))
