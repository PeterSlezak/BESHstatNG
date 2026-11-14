"""Independent whole W1/F publication verifier; explicitly cannot grant READY."""
import json,re,subprocess,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];W='evidence/04A38/completion/w1/'
V='evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group/'
sys.path.insert(0,str(ROOT/V))
from verify_packet import inspect as packet
from verify_inputs import need,load,BASE
from verify_w1_v2 import inspect as census
def inspect(root=ROOT,version='v1'):
 root=Path(root);publication=packet(root,W+'publication-manifest-'+version+'.json',W+'file-inventory-'+version+'.json');proof=census(root)
 manifest=load(root/(W+'publication-manifest-'+version+'.json'));paths={r['path'] for r in manifest['entries']}
 for row in manifest['entries']:
  if row['path'].startswith(W) and row['sha256'] is not None:
   data=(root/row['path']).read_bytes();need(not re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',data),'Private owner-machine path in W1 payload')
   need(not re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',data),'Private key in W1')
 ready=load(root/(W+'readiness.json'));need(ready['status']=='NOT_READY_04A38' and ready['W2']=='NOT_STARTED_AUTHORITY_BLOCKED' and ready['W3']=='NOT_STARTED','False implementation/readiness')
 need(ready['ownership']['historical_whole_bodies_closed']==0 and ready['ownership']['historical_whole_bodies_open']==704 and ready['new_production_changes']==0 and ready['new_original_queries_beyond_F']==0,'Unjustified closure or unauthorized work')
 need(ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),'Scientific/native gate changed')
 release=load(root/(W+'compiled-current-Release.json'));baseline=load(root/'evidence/04A38/preedit/compiled-Release.json')
 need({r['effective_excel_name']:r for r in release['exports']}=={r['effective_excel_name']:r for r in baseline['exports']} and len(release['exports'])==268,'Release exports/defaults/attributes changed')
 validation=load(root/(W+'validation.json'));need(validation['new_F_gate_tests']==70 and validation['new_W1_tests']['total']==18 and validation['new_W1_tests']['exit_code']==0 and validation['unchanged_prior_tests']==457,'Missing/forged executed tests')
 need(validation['full_suites']=='NOT_RUN_EVIDENCE_ONLY_NO_PRODUCTION_EDIT_AND_MANDATORY_REFERENCE_STOP','Full suites misrepresented')
 git=load(root/(W+'git-final.json'));need(git['branch']=='codex/04A-38' and git['head']==git['main']==git['merge_base']==BASE and git['index']=='EMPTY' and git['diff_check_exit']==0,'Git staging/branch/commit/whitespace changed')
 if root.resolve()==ROOT.resolve():
  def run(*a):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*a],cwd=root)
  need(run('branch','--show-current').decode().strip()=='codex/04A-38' and not run('diff','--cached','--name-only'),'Live branch/index changed')
  need(run('rev-parse','HEAD').decode().strip()==run('rev-parse','main').decode().strip()==run('merge-base','HEAD','main').decode().strip()==BASE,'Live ancestry changed')
  need(subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True).returncode==0,'Live diff-check failed')
  need({p.relative_to(root).as_posix() for p in (root/W).rglob('*') if p.is_file()}<=paths,'Unlisted W1 payload')
  need({p.relative_to(root).as_posix() for p in (root/V).rglob('*') if p.is_file()}<=paths,'Unlisted F payload')
 return dict(status='VALID_NOT_READY_W1_CONSOLIDATED_PUBLICATION',F='MMRM_INTERCEPT_GROUP_GATE_PASS',counts=proof,external_files=publication['external_files'],native='NOT_RUN',scientific='NOT_GOLDEN',scope='Local live Git plus published bytes' if root.resolve()==ROOT.resolve() else 'Public-copy bytes only; cannot establish live Git/4695 paths/ignored history')
if __name__=='__main__':print(json.dumps(inspect(version=sys.argv[1] if len(sys.argv)>1 else 'v1')))
