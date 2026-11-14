from common import *
import shutil
assert git('branch','--show-current').decode().strip()=='codex/04A-38'
assert all(git(*a).decode().strip()==BASE for a in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]);assert not git('diff','--cached','--name-only')
check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0
PRIVATE.mkdir(exist_ok=False)
names=['owner-review.md','owner-review-closeout-v3.md','NEXT_WAVE_HANDOVER.md','validation.json','managed-Debug-identities.json','nonpasses.json','source-transitions-final.json','preservation-closeout.json','ownership-summary-v2.json','historical-ownership-v2.json','current-ownership-v2.json','support-reconciliation.json','remaining-work-queue.json','independent-slice-proof.json','text-final-domain-proof.json','host-cache-v4-proof.json','publication-manifest-v3.json','file-inventory-v3.json','publication-closeout-addendum.md','diagnostic-public-derivative.json']
read=[]
for n in names:
 p=PREVIOUS/n;b=p.read_bytes();x=json.loads(b) if p.suffix=='.json' else b.decode('utf-8-sig');read.append(dict(pin(p),keys=list(x) if isinstance(x,dict) else None))
manifest=PREVIOUS/'publication-manifest-v3.json';entries=json.loads(manifest.read_bytes())['entries'];assert len(entries)==3125
for row in entries:
 if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,'Predecessor bytes drift '+row['path']
for r in json.loads((PREVIOUS/'source-transitions-final.json').read_bytes())['files']:assert pin(ROOT/r['path'])==r['after']
# No ignored TestResults enumeration; only historical tracked names from Git.
tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];assert len(tracked)==4695
assert all((ROOT/p).is_file() for p in tracked),'Live tracked path missing; stop, do not restore from ZIP'
required_review_copy_omissions=['docs/architecture/WindowsMigrationLedger','BESHStatNG_Help_MkDocs/docs/assets','evidence/04A23','evidence/04A24','scripts/windows-migration-audit/control-review-decisions.json']
for p in required_review_copy_omissions:assert (ROOT/p).exists(),'Expected historical live path absent '+p
sources=[]
for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
 b=(ROOT/r['snapshot']['path']).read_bytes();assert pin(ROOT/r['snapshot']['path'])==r['snapshot'];blob=git('cat-file','blob',BASE+':'+r['source']['path']);assert blob.replace(b'\r\n',b'\n')==b.replace(b'\r\n',b'\n')
 p=ROOT/r['source']['path'];p.read_bytes();sources.append(dict(original=r,git_sha256=sha(blob),git_bytes=len(blob),identity='RAW' if blob==b else 'CRLF_LF_ONLY',current=pin(p)))
prior=[]
for p in W1.iterdir():
 if p.suffix=='.json' and any(n in p.name for n in ['matrix','crosswalk','schedule','universe','identity-reallocation']):json.loads(p.read_bytes());prior.append(pin(p))
authorities=[]
for d in (ROOT/'evidence/04A38/late-archived-entry-supplement').iterdir():
 if d.is_dir():
  for p in d.glob('owner-authorization*'):
   p.read_bytes();authorities.append(pin(p))
planned=['BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb','BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportFormula.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
before=[]
for path in planned:
 p=ROOT/path;dst=HERE/'source-before'/(path+'.txt');dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(p,dst);before.append(dict(live=pin(p),preimage=pin(dst)))
docs=[]
for p in [ROOT/'AGENTS.md',ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md',ROOT/'scripts/verify-beshstatng.ps1']:
 p.read_text(encoding='utf-8-sig');docs.append(pin(p))
save('entry.json',dict(status='LIVE_C2_CHECKPOINT_VERIFIED',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check_exit=0,tracked=4695,git_status=clean(git('status','--porcelain=v1','--untracked-files=all').decode()),worktrees=clean(git('worktree','list','--porcelain').decode()),source_before=before,source_authentication=sources,required_read=read,W1_read=prior,authorities=authorities,docs_read=docs,predecessor_manifest=pin(manifest),predecessor_hashed_payloads=3124,review_copy_omissions_live_present=required_review_copy_omissions,owner_review='External review findings supplied in active prompt; ZIP is not live state',original_queries=0,original_builds=0,authority='Existing W2 extraction/current-only validation only',TestResults='NO_IGNORED_TREE_TRAVERSAL',W2='IN_PROGRESS',W3='NOT_STARTED'))
print('Preflight PASS:3124 prior payloads;4695 live tracked paths;41 original snapshots;6 exact source preimages; no original execution')
