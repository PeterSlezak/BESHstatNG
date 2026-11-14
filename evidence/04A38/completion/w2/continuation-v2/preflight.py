import shutil,importlib.util
from common import *
def main():
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 assert all(git(*a).decode().strip()==BASE for a in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')])
 assert not git('diff','--cached','--name-only')
 diff=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert diff.returncode==0
 PRIVATE.mkdir(exist_ok=False)
 names=['owner-review.md','owner-review-closeout-v2.md','executive-summary.md','NEXT_WAVE_HANDOVER.md','readiness-closeout-v2.json','ownership-summary.json','historical-ownership.json','current-ownership.json','remaining-boundary-review.json','remaining-work-queue.json','source-transitions-p0-v2.json','source-transitions-p1-v5.json','inspection-p1-v5.json','compatibility-p1-v4.json','compatibility-p1-v5.json','validation.json','validation-qualification.json','nonpasses.json','options-policy-diagnostic.json','managed-Debug-identities.json','tracked-path-preservation.json','preservation-closeout.json','publication-manifest-v4.json','publication-closeout-addendum-v4.md']
 read=[]
 for name in names:
  p=PREVIOUS/name;b=p.read_bytes();v=json.loads(b) if p.suffix=='.json' else b.decode('utf-8-sig');read.append(dict(pin(p),top_keys=list(v) if isinstance(v,dict) else None))
 prior=[]
 for p in W1.iterdir():
  if p.suffix=='.json' and any(s in p.name for s in ['matrix','crosswalk','universe','implementation-schedule','compiled-current','identity-reallocation','witness','export','signature']):
   v=json.loads(p.read_bytes());prior.append(dict(pin(p),top_keys=list(v) if isinstance(v,dict) else None))
 for row in json.loads((PREVIOUS/'source-transitions-p1-v5.json').read_bytes())['files']:assert pin(ROOT/row['after']['path'])==row['after']
 manifest=PREVIOUS/'publication-manifest-v4.json';assert sha(manifest.read_bytes())=='726f684cae59a2ace90bfeccd109c2443d1f43a610a51330f34cb891497301d8'
 for row in json.loads(manifest.read_bytes())['entries']:
  if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,row['path']
 planned=['BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb','BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb','BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb','BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb','BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb']
 before=[]
 for path in planned:
  src=ROOT/path;dest=HERE/'source-before'/(path+'.txt');dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dest);before.append(dict(live=pin(src),preimage=pin(dest)))
 # Record old and new boundaries without invoking any archived UDF.
 originals=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'];auth=[]
 for row in originals:
  snapshot=(ROOT/row['snapshot']['path']).read_bytes();blob=git('cat-file','blob',BASE+':'+row['source']['path']);assert sha(snapshot)==row['snapshot']['sha256'];assert blob.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
  auth.append(dict(path=row['source']['path'],git_blob_bytes=len(blob),git_sha256=sha(blob),snapshot=row['snapshot'],identity='RAW_EXACT' if blob==snapshot else 'CRLF_LF_ONLY'))
 docs=[]
 for p in [ROOT/'AGENTS.md',ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md',ROOT/'docs/architecture/batch-04A-38-owner-review.md',ROOT/'scripts/verify-beshstatng.ps1']:
  p.read_text(encoding='utf-8-sig');docs.append(pin(p))
 save('entry.json',dict(status='VERIFIED_EXISTING_CONTINUATION_V1_W2',date='2026-11-10',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check_exit=0,git_status=clean(git('status','--porcelain=v1','--untracked-files=all').decode()),worktrees=clean(git('worktree','list','--porcelain').decode()),required_checkpoint_read=read,W1_read=prior,docs_read=docs,source_before=before,source_authentication=auth,predecessor_payloads=2644,predecessor_manifest=pin(manifest),authority='Existing W2/W3 only; no original model/query/capture and no original-source assembly compilation',original_queries=0,owner_documents='READ_ONLY',TestResults='NO_IGNORED_TREE_TRAVERSAL',review_context='Latest user prompt supplied the independent review findings. Named external review Markdown/owner ZIP not found among live named repo files; neither substituted for Git/source truth.',W2='IN_PROGRESS',W3='NOT_STARTED'))
 print('Entry/source/protected predecessor2644 payloads authenticated;9 narrow preimages; original snapshots41 Git-authenticated; no original query')
if __name__=='__main__':main()
