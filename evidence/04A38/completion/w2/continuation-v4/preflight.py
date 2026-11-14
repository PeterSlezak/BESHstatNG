from common import *
import shutil
assert git('branch','--show-current').decode().strip()=='codex/04A-38'
assert all(git(*a).decode().strip()==BASE for a in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]);assert not git('diff','--cached','--name-only')
r=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert r.returncode==0
PRIVATE.mkdir(exist_ok=False)
names=['owner-review.md','NEXT_WAVE_HANDOVER.md','readiness.json','ownership-summary.json','historical-ownership.json','current-ownership.json','per-family-ownership.json','remaining-work-queue-v2.json','original-fragment-parent-crosswalk.json','source-transitions-final.json','independent-names-proof.json','independent-ownership-proof.json','validation.json','nonpasses.json','compatibility-names-v3.json','publication-manifest-v2.json']
read=[]
for n in names:
 p=PREVIOUS/n;b=p.read_bytes();x=json.loads(b) if p.suffix=='.json' else b.decode('utf-8-sig');read.append(dict(pin(p),keys=list(x) if isinstance(x,dict) else None))
manifest=PREVIOUS/'publication-manifest-v2.json';assert sha(manifest.read_bytes())=='6c1be0d2bc3e6fbe00968ed1385ad599cab28ad49306756de6085358f98a45dc'
rows=json.loads(manifest.read_bytes())['entries'];assert len(rows)==3425 and sum(r['sha256'] is None for r in rows)==1
for row in rows:
 if row['sha256'] is not None:assert pin(ROOT/row['path'])==row,'C3 bytes drift '+row['path']
for row in json.loads((PREVIOUS/'source-transitions-final.json').read_bytes())['files']:assert pin(ROOT/row['path'])==row['after']
tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];assert len(tracked)==4695 and all((ROOT/p).is_file() for p in tracked)
omissions={s:[p for p in tracked if p.casefold().startswith(s.casefold()+'/')] for s in ['BESHStat.WasmSmoke','installer']}
# Installer location is derived from tracked paths, not a guessed upload tree.
installer=[p for p in tracked if 'installer' in p.casefold()];assert omissions['BESHStat.WasmSmoke'] and installer
sources=[]
for row in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']:
 snap=ROOT/row['snapshot']['path'];assert pin(snap)==row['snapshot'];blob=git('cat-file','blob',BASE+':'+row['source']['path']);assert blob.replace(b'\r\n',b'\n')==snap.read_bytes().replace(b'\r\n',b'\n');sources.append(dict(original=row,Git_sha256=sha(blob),Git_bytes=len(blob),identity='RAW' if blob==snap.read_bytes() else 'CRLF_LF_ONLY',current=pin(ROOT/row['source']['path'])))
u=json.loads((PREVIOUS/'current-universe-names-v3.json').read_bytes());assert len(u['sources'])==41 and len(u['members'])==717 and len(u['exports'])==268
for row in u['sources']:assert sha((ROOT/row['path']).read_bytes())==row['sha256']
originals=[]
for d in (ROOT/'evidence/04A38/late-archived-entry-supplement').iterdir():
 if d.is_dir():
  for p in [*d.glob('owner-authorization*'),*d.glob('owner-review*.md'),*d.glob('input-seal*.json'),*d.glob('original-output-seal*.json')]:p.read_bytes();originals.append(pin(p))
prior=[]
for p in W1.iterdir():
 if p.suffix=='.json' and any(n in p.name for n in ['matrix','crosswalk','schedule','universe','identity-reallocation']):json.loads(p.read_bytes());prior.append(pin(p))
planned=['BESHstatNG/src/ExcelUDFs/UdfLinearAlgebra.vb','BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb','BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb']
before=[]
for path in planned:
 src=ROOT/path;dest=HERE/'source-before'/(path+'.txt');dest.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dest);before.append(dict(live=pin(src),preimage=pin(dest)))
docs=[]
for p in [ROOT/'AGENTS.md',ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md',ROOT/'scripts/verify-beshstatng.ps1']:p.read_text(encoding='utf-8-sig');docs.append(pin(p))
save('entry.json',dict(status='LIVE_C3_CHECKPOINT_AUTHENTICATED',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check_exit=0,git_status=clean(git('status','--porcelain=v1','-uall').decode()),tracked=4695,read=read,predecessor_manifest=pin(manifest),predecessor_hashes=3424,source_before=before,source_authentication=sources,W1_read=prior,A_to_G_read=originals,docs_read=docs,review_copy_omissions_live=dict(WasmSmoke_tracked=len(omissions['BESHStat.WasmSmoke']),installer_tracked=len(installer),all_present=True),original_semantic_queries=0,original_compilations=0,W2='IN_PROGRESS',W3='NOT_STARTED',TestResults='NO_IGNORED_TREE_TRAVERSAL'))
print('C3 verified:3424 hashes;4695 live paths;41 original snapshots;717 current members/268 exports source pins; source preimages frozen')
