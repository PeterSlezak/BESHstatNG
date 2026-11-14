"""Last additive bindings; no source/test/old evidence modification or campaign."""
import sys,json
from pathlib import Path
sys.path.insert(0,str(Path(__file__).parent/'continuation-v16'))
from common import *
load=lambda p:json.loads(Path(p).read_bytes())
prior=W2/'continuation-v16-postseal-handover-addendum-v1.md'
text=prior.read_text(encoding='utf-8')
text+='''
## Last additive preservation and raw-stream binding

This version2 handover is the final authority; version1 remains unchanged. The canonical private manifest is `continuation-v16-retained-private-preservation-v2.json`, verified by the **unchanged** `continuation-v16/verify_c16_private_v1.py`. The first full actual-byte attempt passed protected hashing but failed its bounded-list order comparison: WindowsPath sorting versus case-sensitive normalized string sorting. The exact actual/expected multisets were independently equal, with no missing, extra or unlisted path. Manifest2 changes only list order and records manifest1; every expected file/archive hash and every reader assertion is unchanged. Both raw attempts remain retained; the corrected actual full reader passed. This is a construction-order correction, not a waived private-file mismatch, recaptured historical reference or repeated Python privacy campaign.

Final source/live protection passed for all16 owner documents,278 lineage pins,100 mixed-model captures and41 original source snapshots, with source-transition predecessors retained exactly. Final private audit:23,711 file pins /133 physical archives. The version2 postseal-private addendum additionally binds every completed late command stream, including the finalizer and final Git checks. No earlier seal, review, manifest, ZIP, failure or original/late capture was rewritten or removed.

Canonical final review remains `continuation-v16/final/owner-review.md`; checkpoint reader3/seal3; historical/fragment/canonical/queue ledgers2; publication manifest3/index3/receipt3, publication reader4 and copy reader4. Actual corrected copied-reader receipt2 and physical-reader receipt2 both pass. The earlier605-identity review and index2 ZIPs remain nonfinal historical artifacts. Authoritative final counts are607/704 historical,635/726 current and271/303 support; OPEN97/91/32; new70/69/9; selected57/58. Existing conversion dependency and `LMM_FIT` remain OPEN.

Review ZIPs are local, not uploaded. Supplied physical bytes must match index3 for independent external review. No fresh Release build, unit, ABI/CIL/export or frozen execution occurred; each is `NOT_RUN_OWNER_DEFERRED_TO_04A38_FINALIZATION`. No source/test correction or additional managed/Python/Node/strict/profile/MkDocs campaign followed the combined source freeze.
'''
handover=W2/'continuation-v16-postseal-handover-addendum-v2.md'
with handover.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
addpath=W2/'continuation-v16-postseal-private-addendum-v2.json';gitpath=W2/'continuation-v16-final-git-preservation-v2.json';artifactpath=W2/'continuation-v16-post-publication-artifacts-v2.json'
with addpath.open('x',encoding='utf-8') as addout,gitpath.open('x',encoding='utf-8') as gout,artifactpath.open('x',encoding='utf-8') as aout:
 receipts=[]
 for label,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('staged',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status-short',['status','--short'])]:
  r=command('postseal-final-git-v2-'+label,['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args]);assert r['exit_code']==0
  folder=RUNROOT/'commands'/('w2c16-postseal-final-git-v2-'+label);value=(folder/'stdout.log').read_bytes()
  if label in ['HEAD','main','merge-base']:assert value.decode().strip()==BASE
  if label=='branch':assert value.decode().strip()=='codex/04A-38'
  if label=='staged':assert not value
  receipts.append(dict(label=label,command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')))
 idx=pin(ROOT/'.git/index');assert(idx['bytes'],idx['sha256'])==(620185,'2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c')
 for row in load(HERE/'production-test-freeze-v3.json')['sources']:assert pin(ROOT/row['path'])==row
 json.dump(dict(status='FINAL_LIVE_ANCHORS_INDEX_BYTES_EMPTY_STAGED_DIFF_ZERO_AND_FINAL_SOURCE_FREEZE_PASS',previous=pin(W2/'continuation-v16-final-git-preservation-v1.json'),branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index=idx,staged='EMPTY',diff_check_exit=0,raw=receipts,source_freeze=pin(HERE/'production-test-freeze-v3.json'),original_queries=0),gout,indent=2);gout.flush()
 late=[]
 for folder in sorted((RUNROOT/'commands').glob('w2c16-postseal-*')):
  assert (folder/'command.json').is_file(),'Unfinished late command '+str(folder)
  late.extend(pin(p) for p in sorted(folder.rglob('*')) if p.is_file())
 json.dump(dict(status='ALL_COMPLETED_POSTSEAL_RAW_FILES_PINNED',base=pin(W2/'continuation-v16-retained-private-preservation-v2.json'),previous=pin(W2/'continuation-v16-postseal-private-addendum-v1.json'),postseal_private_pins=late,count=len(late),bound_real_finalizer_command=pin(RUNROOT/'commands/w2c16-postseal-finalize-v1/command.json'),original_queries=0),addout,indent=2);addout.flush()
 files=[pin(p) for p in sorted(W2.glob('continuation-v16-*')) if p.is_file() and p!=artifactpath]
 json.dump(dict(status='FINAL_ADDITIVE_AUTHORITY_AND_ACTUAL_RAW_BINDINGS',previous=pin(W2/'continuation-v16-post-publication-artifacts-v1.json'),final_authority=pin(handover),final_Git=pin(gitpath),final_private_addendum=pin(addpath),canonical_review=pin(HERE/'final/owner-review.md'),canonical_seal=pin(HERE/'review-seal-v3.json'),canonical_index=pin(W2/'continuation-v16-review-parts-index-v3.json'),files=files,self_entry='Nonrecursive external manifest; no self SHA claim.',original_queries=0),aout,indent=2)
print(json.dumps(dict(status='FINAL_V16_PARTIAL_REVIEW_COMPLETE',historical='607/704;97 OPEN',current='635/726;91 OPEN',support='271/303;32 OPEN',private_pins=23711,physical_archives=133,ZIP_parts=8,logical_files=10187,handover=pin(handover),Git=pin(gitpath),artifacts=pin(artifactpath))),flush=True)
