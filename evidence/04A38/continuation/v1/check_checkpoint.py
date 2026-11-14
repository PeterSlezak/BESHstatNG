"""Append-only continuation entry proof; no new reference execution or Git write."""
from datetime import datetime,timezone
import hashlib,json,subprocess
from pathlib import Path

ROOT=Path(__file__).resolve().parents[4]
HERE=Path(__file__).resolve().parent
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
def digest(data):return hashlib.sha256(data).hexdigest()
def pin(path):
 data=(ROOT/path).read_bytes();return dict(path=path,bytes=len(data),sha256=digest(data))
def save(path,value):
 target=HERE/path;target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,indent=2);stream.write('\n')
def git(*args):
 return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True,check=False)

def main():
 assert git('branch','--show-current').stdout.decode().strip()=='codex/04A-38'
 for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]:assert git(*args).stdout.decode().strip()==BASE
 assert not git('diff','--cached','--name-only').stdout
 check=git('diff','--check');assert check.returncode==0
 manifest_path='evidence/04A38/publication-manifest.json';manifest=json.loads((ROOT/manifest_path).read_bytes())
 assert len(manifest['entries'])==612
 external=[row for row in manifest['entries'] if row['path']!=manifest_path]
 assert len(external)==611 and len({row['path'].casefold() for row in external})==611
 for row in external:assert pin(row['path'])==row,('Checkpoint changed',row['path'])
 frozen=[]
 for path,expected in [
  ('evidence/04A38/preedit/frozen-v2/receipt.json','9f9a55337311bf37e010dcc078cb2ff8a8384122c1b105c4593732e90e13d165'),
  ('evidence/04A38/preedit/addendum-frozen-v1/receipt.json','35c790471841ce33fd2857ca881ff84103b2856cf61139fe1312bf002eb30646')]:
  receipt=json.loads((ROOT/path).read_bytes());assert pin(path)['sha256']==expected
  for row in receipt['files']:assert pin(row['path'])==row,('Frozen dependency changed',row['path'])
  frozen.append(dict(receipt=pin(path),dependencies=receipt['files']))
 # Preserve every checkpoint artifact that subsequent implementation/closeout
 # might replace. Historical evidence, readers and references remain in place.
 selected=[row['path'] for row in external if not row['path'].startswith('evidence/')]
 selected+=['evidence/04A38/'+name for name in ['README.md','readiness.json','publication-manifest.json','file-inventory.json']]
 backups=[]
 for relative in selected:
  target=HERE/'checkpoint'/relative;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as stream:stream.write((ROOT/relative).read_bytes())
  backups.append(dict(original=pin(relative),snapshot=pin(target.relative_to(ROOT).as_posix())))
 save('entry.json',dict(status='CHECKPOINT_VERIFIED_BEFORE_CONTINUATION_PRODUCTION_EDITS',captured_utc=datetime.now(timezone.utc).isoformat(),
  branch='codex/04A-38',head=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check=dict(exit_code=0,stdout=check.stdout.decode(),stderr=check.stderr.decode()),
  porcelain=git('status','--porcelain=v1').stdout.decode(),checkpoint_manifest=pin(manifest_path),checkpoint_payloads=external,
  frozen_receipts=frozen,backups=backups,ignored_capture_protection='All611 external bytes verified, including79 normally ignored payloads; no path moved or discarded',
  provenance='Post-checkpoint continuation receipt only; never a PRE_EDIT_FROZEN reference or permission for late expectations',
  production_edits_this_continuation=False,native='NOT_RUN',scientific='NOT_GOLDEN',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED')))
 print('Verified611 checkpoint payloads, both fixed freeze seals and their dependencies; preserved',len(backups),'replaceable checkpoint artifacts')

if __name__=='__main__':main()
