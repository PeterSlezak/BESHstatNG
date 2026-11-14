"""Independent composition of bounded GEE, publication and honest stop readers.

This reader deliberately cannot return whole-batch READY or close ownership.
No producer or statistical implementation is imported.
"""
import json,subprocess
from pathlib import Path
from verify_gee import ROOT,PREFIX,BASE,need,load,check_pin,inspect as gee,verify_publication
from verify_remaining_mmrm import inspect as mmrm
from verify_preservation import inspect as preservation

def inspect(root=ROOT):
 root=Path(root)
 published=verify_publication(root,PREFIX+'publication-manifest-closeout-v1.json',PREFIX+'file-inventory-closeout-v1.json')
 gee(root,publication=True);gap=mmrm(root)
 gate=load(root/(PREFIX+'GEE_SUPPLEMENT_GATE_PASS.json'))
 need(gate['status']=='GEE_SUPPLEMENT_GATE_PASS' and gate['whole_batch_status']=='NOT_READY_04A38','Wrong GEE/batch qualification')
 need((gate['cases_per_configuration'],gate['positive_profile_selections_per_configuration'],gate['intended_guards_per_configuration'],gate['returned_reports'])==(12,6,6,0),'Wrong GEE case/outcome counts')
 ready=load(root/(PREFIX+'readiness-closeout-v1.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['blocker_member_id']==gap['member_id'] and ready['new_production_changes']==0,'False completion/ownership')
 need(ready['full_ready_verifier']=='NOT_COMPLETED_NOT_RUN' and ready['new_mmrm_references']=='NOT_AUTHORIZED_NOT_EXECUTED','Unapproved acceptance/reference')
 need(ready['gates']=={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'},'Scientific/native promotion')
 validation=load(root/(PREFIX+'validation-closeout-v1.json'))
 need(validation['mmrm_gap_adverse']['total']==9 and validation['mmrm_gap_adverse']['exit_code']==0,'Missing independent gap adverse tests')
 git=load(root/(PREFIX+'git-final-closeout-v1.json'))
 need(git['index']=='EMPTY' and git['head']==git['main']==git['merge_base']==BASE and git['branch']=='codex/04A-38' and git['diff_check_exit']==0,'Git stage/commit/branch/whitespace')
 if root.resolve()==ROOT.resolve():
  preservation(root)
  def run(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  need(not run('diff','--cached','--name-only'),'Live staging')
  need(run('rev-parse','HEAD').decode().strip()==run('rev-parse','main').decode().strip()==run('merge-base','HEAD','main').decode().strip()==BASE,'Live commit/merge')
  need(run('branch','--show-current').decode().strip()=='codex/04A-38','Live branch change')
  check=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True)
  need(check.returncode==0,'Live whitespace failure')
 return dict(status='VALID_NOT_READY_GEE_CLOSEOUT',gee_supplement='GEE_SUPPLEMENT_GATE_PASS',
      stop='MMRM headered Name/Value original reference missing',member_id=gap['member_id'],
      external_files=published['external_files'],new_production_changes=0,new_mmrm_references=0,scientific='NOT_GOLDEN',native='NOT_RUN')
if __name__=='__main__':print(json.dumps(inspect()))
