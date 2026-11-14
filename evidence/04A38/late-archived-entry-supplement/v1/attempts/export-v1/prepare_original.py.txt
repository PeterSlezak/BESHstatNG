"""Authenticated read-only Git export and bounded late-supplement authority.

No current production source/binary is used as an original implementation.
No permissions, Git index, branches, packages or historical files are changed.
"""
from datetime import datetime,timezone
from pathlib import Path
import hashlib,io,json,subprocess,tarfile

ROOT=Path(__file__).resolve().parents[4]
HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A38.tmp'/'late38-v1'
TREE=PRIVATE/'original-tree'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
 data=path.read_bytes();return dict(path=path.relative_to(ROOT).as_posix(),bytes=len(data),sha256=sha(data))
def save(name,value):
 p=HERE/name;p.parent.mkdir(parents=True,exist_ok=True)
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2);f.write('\n')
def git(*args):
 return subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True,check=True).stdout
def public_path(path):
 protected=json.loads((ROOT/'evidence/04A38/entry-source-pins.json').read_bytes())['files']
 hidden={r['path'] for r in protected if r['path'].startswith('<PRIVATE_PROTECTED_PATH:')}
 token='<PRIVATE_PROTECTED_PATH:'+sha(path.encode())[:24]+'>'
 return token if token in hidden else path

def main():
 assert git('branch','--show-current').decode().strip()=='codex/04A-38'
 assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==git('merge-base','HEAD','main').decode().strip()==BASE
 assert not git('diff','--cached','--name-only')
 check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0
 save('owner-authorization.json',dict(owner_quote='Yes, I approve',owner_decision_date='2026-11-08',
  provenance='Exact owner response/date reported in the current user execution prompt; no invented external approval or transferred04A37 permission',
  scope='04A38 only: proposed432 calibration/grouped/nested cases/configuration; independent pinned original source before current supplemental comparison; no matrix expansion',
  source_commit=BASE,chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',
  not_preedit_frozen=True,earlier_missing_coverage='evidence/04A38/continuation/v1/frozen-branch-coverage-v2.json',
  proposal=pin(ROOT/'evidence/04A38/continuation/v1/proposed-missing-case-matrix.json'),
  recorded_utc=datetime.now(timezone.utc).isoformat(),gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED'),
  statistical_changes=False,native='NOT_RUN',r='NOT_RUN',scientific='NOT_GOLDEN',git_publication=False,next_batch='04A39_NOT_AUTHORIZED'))
 TREE.mkdir(parents=True,exist_ok=False)
 archive=git('archive','--format=tar',BASE)
 blobs={}
 for item in git('ls-tree','-r','-z',BASE).split(b'\0'):
  if not item:continue
  meta,name=item.split(b'\t',1);mode,kind,oid=meta.decode().split();assert kind=='blob'
  blobs[name.decode()]=(mode,oid)
 records=[]
 with tarfile.open(fileobj=io.BytesIO(archive),mode='r:') as tar:
  for member in tar:
   target=TREE/member.name
   assert target.resolve().is_relative_to(TREE.resolve())
   if member.isdir():target.mkdir(parents=True,exist_ok=True);continue
   assert member.isfile(),('Nonregular Git archive member',public_path(member.name))
   data=tar.extractfile(member).read();assert member.name in blobs
   mode,oid=blobs[member.name];assert hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==oid
   target.parent.mkdir(parents=True,exist_ok=True)
   with target.open('xb') as f:f.write(data)
   records.append(dict(path=public_path(member.name),git_blob=oid,git_mode=mode,bytes=len(data),sha256=sha(data)))
 assert len(records)==len(blobs)
 # Git's canonical bytes and the original worktree snapshot may differ only by
 # explicitly proved line-ending conversion. Never silently normalize output.
 originals=json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files'];proof=[]
 for row in originals:
  data=(TREE/row['source']['path']).read_bytes();snapshot=(ROOT/row['snapshot']['path']).read_bytes()
  assert data==snapshot or data.replace(b'\r\n',b'\n')==snapshot.replace(b'\r\n',b'\n')
  proof.append(dict(path=row['source']['path'],git_sha256=sha(data),entry_snapshot=row['snapshot'],relation='EXACT' if data==snapshot else 'PROVED_LF_CRLF_ONLY'))
 save('original-source-authenticity.json',dict(status='PRISTINE_PINNED_GIT_TREE_VERIFIED',source_commit=BASE,git_tar_sha256=sha(archive),git_tar_bytes=len(archive),
  archive_stored=False,source_tree='<ISOLATED_ORIGINAL_TREE>',files=records,original_udf_snapshot_crosscheck=proof,
  build_not_yet_executed=True,qualification='Complete Git source export; ignored offline SDK/third-party dependencies must be separately pinned. No current Core/Common binary is an original dependency.',
  chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',native='NOT_RUN',scientific='NOT_GOLDEN'))
 print('Authenticated pristine Git tree:',len(records),'files;41 original UDF snapshots crosschecked; exact owner approval recorded')

if __name__=='__main__':main()
