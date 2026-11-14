"""Sequential postscan bookkeeping; fail immediately and retain every raw attempt."""
from common import *

def run(label,filename,*args,private=False):
    path=PRIVATE/'bookkeeping-code-v1'/filename if private else HERE/filename
    result=command(label,[sys.executable,'-B',str(path),*args])
    print(label,result['exit_code'],flush=True)
    assert result['exit_code']==0,'Preserve unsuccessful attempt and investigate: '+label

def proof():
    assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
    run('postscan-identity-reconciliation-v1','postprocess_v1.py')
    bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]];runpy.run_path(sys.argv[0],run_name="__main__")'
    result=command('install-postscan-bookkeeping-v1',[sys.executable,'-B','-c',bootstrap,str(HERE),str(PRIVATE/'bookkeeping-code-v1/install_postscan_tools_v1.py')])
    print('install-postscan-bookkeeping-v1',result['exit_code'],flush=True);assert result['exit_code']==0
    for label,name,args in [
        ('prepare-preservation-reader-v1','prepare_preservation_reader_v1.py',[]),
        ('complete-compatibility-operation-crosswalk-v1','fixture_crosswalk_v1.py',[]),
        ('independent-validation-replay-v1','prepare_final_validation_v1.py',[]),
        ('final-source-validation-binding-v1','bind_final_validation_v1.py',[]),
        ('prepare-publication-privacy-v1','prepare_publication_tools_v1.py',[]),
        ('prepare-archive-controls-v1','prepare_archive_controls_v1.py',[]),
        ('publication-privacy-adverse-controls-v1','prove_publication_privacy_v1.py',[]),
        ('write-owner-review-v1','write_review_v1.py',[]),
        ('prepare-review-exclusions-v1','publish_parts_v1.py',['prepare']),
        ('seal-checkpoint-and-mutations-v1','seal_controls_v1.py',[]),
        ('physical-archive-adverse-controls-v1','publication_adverse_v1.py',[])]:run(label,name,*args)
    print('POSTSCAN_PROOF_COMPLETE_NOT_READY',flush=True)

def publish():
    for label,name,args in [
        ('publication-inventory-once-v1','publish_parts_v1.py',['inventory']),
        ('publication-split-export-v1','publish_parts_v1.py',['export']),
        ('independent-copy-reader-v1','copy_review_v1.py',[]),
        ('live-preservation-v1','finalize_live_v1.py',['preserve']),
        ('exit-git-preservation-v1','capture_exit_git_v1.py',[])]:run(label,name,*args)
    print('PUBLICATION_LIVE_GIT_COMPLETE_NOT_READY; finish private generation and final receipts outside Runner',flush=True)

if __name__=='__main__':{'proof':proof,'publish':publish}[sys.argv[1]]()
