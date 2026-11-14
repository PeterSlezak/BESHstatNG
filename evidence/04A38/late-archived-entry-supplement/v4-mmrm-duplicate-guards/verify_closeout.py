"""Independent bounded v4 closeout composition; cannot grant batch READY."""
import subprocess
from pathlib import Path
from verify_inputs import ROOT,PREFIX,BASE,need,load
from verify_supplement import inspect as supplement
from verify_packet import inspect as packet
from verify_mask_gap import inspect as gap

def inspect(root=ROOT):
 root=Path(root)
 publication=packet(root,PREFIX+'publication-manifest-closeout-v1.json',PREFIX+'file-inventory-closeout-v1.json')
 supplement(root);missing=gap(root)
 gate=load(root/(PREFIX+'MMRM_DUPLICATE_GUARD_GATE_PASS.json'))
 need(gate['status']=='MMRM_DUPLICATE_GUARD_GATE_PASS' and gate['batch_status']=='NOT_READY_04A38' and gate['cases_per_configuration']==4 and gate['returned_reports']==0,'False guard/batch readiness')
 need(gate['guard_kinds_per_configuration']==dict(duplicate_visit_time=2,duplicate_fitted_column=2),'Missing guard/culture scope')
 ready=load(root/(PREFIX+'readiness-closeout-v1.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['earliest_blocker_member_id']==missing['member_id'] and ready['new_production_changes']==ready['extra_reference_executions']==0,'False completion or unauthorized extra reference')
 need(ready['full_ready_verifier']=='NOT_COMPLETED_NOT_RUN' and ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),'False final READY or native/scientific promotion')
 validation=load(root/(PREFIX+'validation-closeout-v1.json'))
 need(validation['mask_gap_adverse']['total']==10 and validation['mask_gap_adverse']['exit_code']==0 and validation['new_test_total']==88 and validation['old_test_total']==264,'Missing/forged adverse evidence')
 git=load(root/(PREFIX+'git-final.json'))
 need(git['branch']=='codex/04A-38' and git['head']==git['main']==git['merge_base']==BASE and git['index']=='EMPTY' and git['diff_check_exit']==0,'Git staging/commit/merge/whitespace')
 if root.resolve()==ROOT.resolve():
  def run(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  need(run('branch','--show-current').decode().strip()=='codex/04A-38' and not run('diff','--cached','--name-only'),'Live branch/index changed')
  need(run('rev-parse','HEAD').decode().strip()==run('rev-parse','main').decode().strip()==run('merge-base','HEAD','main').decode().strip()==BASE,'Live Git ancestry changed')
  result=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True);need(result.returncode==0,'Live diff-check failed')
 return dict(status='VALID_NOT_READY_DUPLICATE_GUARD_CLOSEOUT',duplicate_guard_gate='MMRM_DUPLICATE_GUARD_GATE_PASS',exact_each_configuration='4/4',
      returned_reports=0,earliest_blocker_member_id=missing['member_id'],missing='Independent original empty-selection MMRM_LSMEANS row-mask guard witness at line1426',
      external_files=publication['external_files'],new_production_changes=0,extra_references=0,scientific='NOT_GOLDEN',native='NOT_RUN')
if __name__=='__main__':
 import json
 print(json.dumps(inspect()))
