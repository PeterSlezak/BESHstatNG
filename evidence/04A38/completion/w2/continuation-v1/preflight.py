"""Read and authenticate the entire required checkpoint, without recapturing originals."""
import shutil
from common import *

def main():
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 assert all(git(*args).decode().strip()==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')])
 assert not git('diff','--cached','--name-only')
 checked=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert checked.returncode==0
 PRIVATE.mkdir(exist_ok=False)
 names=['owner-review.md','NEXT_WAVE_HANDOVER.md','readiness.json','ownership-summary.json','historical-ownership-checkpoint.json','current-member-support-fragment-checkpoint.json','independent-profile-proof.json','delegated-body-proof-v2.json','independent-delegated-proof-v2.json','profile-edges-Debug.json','profile-edges-Release.json','source-transitions.json','validation.json','nonpasses.json','preedit-compatibility.json','A-compatibility-Debug.json','A-compatibility-Release.json','profile-compatibility-v3-Debug.json','profile-compatibility-v3-Release.json','publication-manifest-closeout-v2.json','publication-manifest-v1.json','file-inventory-closeout-v2.json']
 read=[]
 for name in names:
  p=W2/name;b=p.read_bytes();v=json.loads(b) if p.suffix=='.json' else b.decode('utf-8-sig');read.append(dict(pin(p),keys=list(v) if isinstance(v,dict) else None))
 prior=[]
 for p in sorted(W1.iterdir()):
  if p.suffix=='.json' and any(x in p.name for x in ['matrix','crosswalk','universe','implementation-schedule','identity-reallocation','compiled-current','witness','export','signature']):
   b=p.read_bytes();v=json.loads(b);prior.append(dict(pin(p),keys=list(v) if isinstance(v,dict) else None))
 for row in json.loads((W2/'source-transitions.json').read_bytes())['files']:assert pin(ROOT/row['after']['path'])==row['after']
 # No giant whole-repository copies: preserve only this cohesive slice, as neutral
 # data files, keeping original bytes and avoiding the prior encoding problem.
 planned=['BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb','BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb','BESHStat.Excel.Common.Tests/src/UdfModelProfilePreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
 before=[]
 for path in planned:
  src=ROOT/path;dst=HERE/'source-before'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
  assert src.read_bytes()==dst.read_bytes();before.append(dict(live=pin(src),preimage=pin(dst)))
 # Original-source text/metadata inspection only, never an original fit/query.
 original=ROOT/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt'
 raw=original.read_bytes();assert len(raw)==92704 and sha(raw)=='596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d'
 module=raw.decode('utf-8-sig');start=module.index('        Private Function FormatMmrmAtProfileDescription(');end=module.index('        End Function',start)+len('        End Function')
 save('entry.json',dict(status='VERIFIED_EXISTING_W2_CHECKPOINT_CONTINUATION',date='2026-11-10',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check_exit=0,
  git_status=git('status','--porcelain=v1','--untracked-files=all').decode(),worktrees=clean(git('worktree','list','--porcelain').decode()),required_checkpoint_read=read,W1_crosswalks_read=prior,source_before=before,
  predecessor_manifest=pin(W2/'publication-manifest-closeout-v2.json'),P0_original_member=dict(canonical_id='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb|BESHStatNG.WorksheetFunctions.MixedModelUDFs|FunctionBlock|9465ac0daaaabaa5@86828:701',snapshot=pin(original),body=module[start:end]),
  authority='Already authorized04A38 W2/W3 execution-context continuation; G already passed and exhausted; no new original model/query authorized or executed',W3='NOT_STARTED',TestResults='NOT_TRAVERSED',owner_documents='READ_ONLY'))
 print('Checkpoint source hashes/Git authenticated; required ledgers parsed in full; four exact source preimages preserved; no original query')
if __name__=='__main__':main()
