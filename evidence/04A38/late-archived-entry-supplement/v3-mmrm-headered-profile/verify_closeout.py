"""Independent bounded closeout composition; never whole-batch acceptance."""
import subprocess
from pathlib import Path
from verify_inputs import ROOT,PREFIX,BASE,need,load
from verify_supplement import inspect as supplement
from verify_packet import inspect as packet
from verify_guard_gap import inspect as gap

def inspect(root=ROOT):
 root=Path(root)
 publication=packet(root,PREFIX+'publication-manifest-closeout-v1.json',PREFIX+'file-inventory-closeout-v1.json')
 proof=supplement(root);missing=gap(root)
 gate=load(root/(PREFIX+'MMRM_HEADERED_PROFILE_GATE_PASS.json'))
 need(gate['status']=='MMRM_HEADERED_PROFILE_GATE_PASS' and gate['batch_status']=='NOT_READY_04A38' and gate['cases_per_configuration']==gate['reports_per_configuration']==2,'False gate/batch promotion')
 ready=load(root/(PREFIX+'readiness-closeout-v1.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['earliest_blocker_member_id']==missing['member_id'] and ready['new_production_changes']==ready['extra_invalid_profile_reference_executions']==0,'False completion/extra reference')
 need(ready['full_ready_verifier']=='NOT_COMPLETED_NOT_RUN' and ready['gates']=={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'},'Unsupported final READY/scientific/native gate')
 validation=load(root/(PREFIX+'validation-closeout-v1.json'))
 need(validation['guard_gap_adverse']['total']==10 and validation['guard_gap_adverse']['exit_code']==0 and validation['new_test_total']==79 and validation['old_test_total']==185,'Missing/forged adverse validation')
 git=load(root/(PREFIX+'git-final.json'))
 need(git['branch']=='codex/04A-38' and git['head']==git['main']==git['merge_base']==BASE and git['index']=='EMPTY' and git['diff_check_exit']==0,'Git staging/commit/merge/whitespace')
 if root.resolve()==ROOT.resolve():
  def run(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  need(run('branch','--show-current').decode().strip()=='codex/04A-38' and not run('diff','--cached','--name-only'),'Live branch/index change')
  need(run('rev-parse','HEAD').decode().strip()==run('rev-parse','main').decode().strip()==run('merge-base','HEAD','main').decode().strip()==BASE,'Live commit/main/merge change')
  result=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True);need(result.returncode==0,'Live diff-check failure')
 return dict(status='VALID_NOT_READY_MMRM_CLOSEOUT',mmrm_headered_gate='MMRM_HEADERED_PROFILE_GATE_PASS',exact_each_configuration='2/2',
      reports_each_configuration=2,blocker_member_id=missing['member_id'],missing='Independent original duplicate visit/time and fitted-design guard references',
      external_files=publication['external_files'],new_production_changes=0,extra_references=0,scientific='NOT_GOLDEN',native='NOT_RUN')
if __name__=='__main__':
 import json
 print(json.dumps(inspect()))
