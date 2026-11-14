"""Append-only evidence publication preparation, no Git or external publication."""
import json,re,sys,subprocess
from datetime import datetime,timezone
from inspect_entry import ROOT,HERE,BASE,CLASS,pin,save,git
from verify_gee import inspect,verify_publication
from verify_preservation import inspect as preservation
sys.path.insert(0,str(ROOT/'evidence/04A38'))
from baseline_report import clean,python_receipt
from run import PRIVATE

def main():
 report=inspect();save('compatibility-comparison.json',report)
 save('preservation.json',preservation())
 save('focused-validation.json',dict(gee_adverse=python_receipt('gee38-v2-adverse-corrected'),
      protected_adverse=python_receipt('gee38-v2-protected-adverse'),failed_first_attempt='attempt-adverse-v1.json',
      inherited_results_not_rerun=dict(managed_each='20556 PASS /14 inherited FAIL',node='916 PASS',python='767 PASS /178 FAIL /11 ERROR',strict_profile_mkdocs=[1,1,0]),
      full_lanes='NOT_RUN: evidence-only source/capture/readers; no production/test-project changes',
      wrappers='NOT_RUN: implicit restore/shared outputs/packaging; isolated offline original builds and focused captures executed'))
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True)
 assert check.returncode==0 and not git('diff','--cached','--name-only')
 save('git-final.json',dict(branch=git('branch','--show-current').decode().strip(),head=git('rev-parse','HEAD').decode().strip(),
      main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',
      diff_check_exit=check.returncode,status=git('status','--short').decode(),git_writes=False,recorded_utc=datetime.now(timezone.utc).isoformat()))
 save('readiness.json',dict(status='NOT_READY_04A38',gee_evidence='Exact source/input/output/branch proof complete; publication adverse gate pending',
      remaining='Existing import/profile/report extraction;704 historical/719 current/317 support/inline reconciliation;full final independent READY audit',
      production_changes=0,gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),native='NOT_RUN',scientific='NOT_GOLDEN',next_batch='04A39_NOT_STARTED'))
 previous=ROOT/'evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json'
 paths={r['path'] for r in json.loads(previous.read_bytes())['entries']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and p.name!='entry-inspection.json')
 self_path=(HERE/'publication-manifest.json').relative_to(ROOT).as_posix()
 inventory_path=(HERE/'file-inventory.json').relative_to(ROOT).as_posix()
 paths.update([self_path,inventory_path])
 save('file-inventory.json',dict(paths=sorted(paths),excluded_private_raw_environment_log='entry-inspection.json; exact digest and public view disclosed in private-environment-control.json',
      raw_private_build_logs='Outside publication, sanitized command receipts/digests included; no package or binary export'))
 # Privacy scan covers every new intended text artifact, including captures.
 findings=[]
 for path in sorted(paths):
  if path in {self_path} or not path.startswith(HERE.relative_to(ROOT).as_posix()+'/'):continue
  data=(ROOT/path).read_bytes()
  if re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',data):findings.append(dict(path=path,kind='UNREDACTED_OWNER_HOME_PATH'))
  if re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',data):findings.append(dict(path=path,kind='PRIVATE_KEY'))
 assert not findings,findings
 save('privacy.json',dict(status='PASS',new_public_text_files_checked=len([p for p in paths if p.startswith(HERE.relative_to(ROOT).as_posix()+'/')])-1,
      findings=findings,raw_environment_diagnostic='Retained locally, explicitly excluded;public view preserves every nonenvironment field',
      prior_privacy_nonpasses='Unchanged historical synthetic-fixture findings; not relabeled PASS',workbook_data='Synthetic sealed fixtures only'))
 paths.add((HERE/'privacy.json').relative_to(ROOT).as_posix())
 # Inventory created above is not sealed yet; update only this new unsealed draft.
 inventory=HERE/'file-inventory.json'
 obj=json.loads(inventory.read_bytes());obj['paths']=sorted(paths)
 inventory.write_text(json.dumps(obj,indent=2)+'\n',encoding='utf-8',newline='\n')
 rows=[dict(path=p,bytes=None,sha256=None) if p==self_path else pin(ROOT/p) for p in sorted(paths)]
 save('publication-manifest.json',dict(status='NOT_READY_04A38',reference_class=CLASS,scope='GEE_SUPPLEMENT_GATE_PACKET_ONLY; no whole-batch acceptance',predecessor=pin(previous),entries=rows))
 print(json.dumps(verify_publication(ROOT)))
if __name__=='__main__':main()
