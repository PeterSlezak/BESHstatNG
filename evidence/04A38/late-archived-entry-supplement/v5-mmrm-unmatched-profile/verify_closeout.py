"""Independent bounded closeout: explicitly rejects whole-batch READY."""
import subprocess
from pathlib import Path
from verify_inputs import ROOT,PREFIX,BASE,need,load
from verify_supplement import inspect as supplement,time
from verify_packet import inspect as packet
from verify_group_gap import inspect as gap
def inspect(root=ROOT,version='v1'):
 root=Path(root)
 publication=packet(root,PREFIX+'publication-manifest-closeout-'+version+'.json',PREFIX+'file-inventory-closeout-'+version+'.json')
 comparison=supplement(root);missing=gap(root)
 gate=load(root/(PREFIX+'MMRM_UNMATCHED_PROFILE_GATE_PASS.json'))
 need(gate['status']=='MMRM_UNMATCHED_PROFILE_GATE_PASS' and gate['batch_status']=='NOT_READY_04A38' and gate['cases_per_configuration']==2 and gate['returned_reports']==0,'False guard/batch readiness')
 need(gate['mask_false_entries_per_case']==54 and gate['matched_count_per_case']==0 and gate['original_true_branch_line']==1426,'Lost true mask witness')
 ready=load(root/(PREFIX+'readiness-closeout-v1.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['earliest_blocker_member_id']==missing['member_id'] and ready['new_production_changes']==ready['extra_reference_executions']==0,'False completion or extra reference')
 need(ready['ownership_closed'] is False and ready['full_ready_verifier']=='NOT_COMPLETED_NOT_RUN' and ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),'False final READY/scientific promotion')
 validation=load(root/(PREFIX+'validation-closeout-v1.json'))
 need(validation['group_gap_adverse']['total']==12 and validation['group_gap_adverse']['exit_code']==0 and validation['new_test_total']==93 and validation['old_test_total']==352,'Missing/forged adverse evidence')
 need(validation['inherited_receipts_only']==dict(managed_Debug='20556 PASS /14 inherited FAIL',managed_Release='20556 PASS /14 inherited FAIL',Python='767 PASS /178 FAIL /11 ERROR',Node='916 PASS',strict=1,profile=1,MkDocs=0),'Changed historical nonpass accounting')
 need(validation['full_suites'].startswith('NOT_RUN') and validation['full_ready_verifier']=='NOT_COMPLETED_NOT_RUN','Unrun full lane/READY verifier presented as new pass')
 sealed=time(load(root/(PREFIX+'original-output-seal.json'))['sealed_utc'])
 for item in load(root/(PREFIX+'gate-validation.json'))['current_compiled_artifact_chronology']:need(time(item['modified_utc'])>sealed and time(item['birth_utc'])>sealed,'Current compilation before original seal')
 transition=load(root/(PREFIX+'source-transitions.json'));need(transition['new_production_transitions']==transition['new_existing_test_project_transitions']==transition['new_portable_consumer_edges']==[],'Unaccounted production/consumer change')
 git=load(root/(PREFIX+'git-final.json'))
 need(git['branch']=='codex/04A-38' and git['head']==git['main']==git['merge_base']==BASE and git['index']=='EMPTY' and git['diff_check_exit']==0,'Git staging/commit/merge/whitespace')
 if root.resolve()==ROOT.resolve():
  def run(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  need(run('branch','--show-current').decode().strip()=='codex/04A-38' and not run('diff','--cached','--name-only'),'Live branch/index changed')
  need(run('rev-parse','HEAD').decode().strip()==run('rev-parse','main').decode().strip()==run('merge-base','HEAD','main').decode().strip()==BASE,'Live Git ancestry changed')
  result=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True);need(result.returncode==0,'Live diff-check failed')
 return dict(status='VALID_NOT_READY_UNMATCHED_PROFILE_CLOSEOUT',unmatched_profile_gate='MMRM_UNMATCHED_PROFILE_GATE_PASS',exact_each_configuration='2/2',original_line1426_true_witnesses=4,
  returned_reports=0,earliest_blocker_member_id=missing['member_id'],missing='Independent original scalar intercept-group guard witness at lines1477-1479',external_files=publication['external_files'],new_production_changes=0,extra_references=0,scientific='NOT_GOLDEN',native='NOT_RUN',
  review_boundary='Live Git checked' if root.resolve()==ROOT.resolve() else 'Published bytes verified; copied packet cannot prove live Git/4695 tracked-path preservation/ignored history')
if __name__=='__main__':
 import json,sys
 print(json.dumps(inspect(version=sys.argv[1] if len(sys.argv)>1 else 'v1')))
