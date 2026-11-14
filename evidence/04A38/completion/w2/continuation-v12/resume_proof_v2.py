"""Resume after preserved accounting rejection; actual campaigns are never rerun."""
from common import *
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]];runpy.run_path(sys.argv[0],run_name="__main__")'
result=command('install-postscan-bookkeeping-v1',[sys.executable,'-B','-c',bootstrap,str(HERE),str(PRIVATE/'bookkeeping-code-v1/install_postscan_tools_v1.py')]);assert result['exit_code']==0
from finish_checkpoint_v1 import run
for label,name,args in [
    ('prepare-postscan-accounting-successor-v1','prepare_postprocess_successor_v1.py',[]),
    ('postscan-identity-reconciliation-v2','postprocess_v2.py',[]),
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
