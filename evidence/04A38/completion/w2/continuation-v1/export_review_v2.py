"""Non-destructive derivative review ZIP; not installer/release packaging."""
import shutil,zipfile
from common import *
from verify_publication_v2 import inspect
from verify_preservation_v4 import inspect as preserve
def hashfile(path):
 with path.open('rb') as f:return __import__('hashlib').file_digest(f,'sha256').hexdigest()
def main():
 manifest=(HERE/'publication-manifest-v4.json').relative_to(ROOT).as_posix()
 local=inspect(manifest_path=manifest);assert local['status']=='VALID_PARTIAL_W2_CONTINUATION_NOT_READY'
 records=[]
 for label,script,args in [('publication-local-v4','verify_publication_v2.py',[manifest]),('publication-copy-v4','check_public_copy_v2.py',[manifest])]:
  r=command(label,[sys.executable,'-B',str(HERE/script),*args]);records.append(dict(label=label,command=clean(r)));assert r['exit_code']==0
 entries=json.loads((ROOT/manifest).read_bytes())['entries'];export=RUNROOT/'exports';export.mkdir(exist_ok=True)
 name='BESHstatNG_04A38_W2_continuation_v1_20261110_review.zip';path=export/name;assert not path.exists(),'Select a new derivative name; never overwrite'
 with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
  for row in entries:z.write(ROOT/row['path'],arcname=row['path'])
 with zipfile.ZipFile(path) as z:
  crc=z.testzip();names=z.namelist();assert crc is None and names==[r['path'] for r in entries] and len(names)==len(set(names))
 copy=export/name.replace('.zip','.hash-copy.zip');assert not copy.exists();shutil.copyfile(path,copy);digest=hashfile(path);assert hashfile(copy)==digest
 protected=preserve();assert not git('diff','--cached','--name-only')
 result=dict(status='VERIFIED_DERIVATIVE_REVIEW_COPY_NOT_READY',zip=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest,entries=len(entries),CRC='PASS_ALL_ENTRIES',hash_copy='PASS_IDENTICAL_BYTES',manifest=pin(ROOT/manifest),independent_local_and_copy_readers=records,preservation=protected,branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,git_status=clean(git('status','--porcelain=v1','--untracked-files=all').decode()),scope='All intended manifest payloads; no live Git/private loaded binaries/omitted files or ignored-history proof from ZIP. Read file-inventory-v2.json for inclusions/exclusions.',publication='LOCAL_REVIEW_ARCHIVE_ONLY;NO_UPLOAD_COMMIT_MERGE_OR_OWNER_ACCEPTANCE',W2='IN_PROGRESS',W3='NOT_STARTED')
 receipt=export/'BESHstatNG_04A38_W2_continuation_v1_20261110_export.json'
 with receipt.open('x',encoding='utf-8') as f:json.dump(result,f,indent=2,ensure_ascii=False);f.write('\n')
 print(json.dumps({k:result[k] for k in ['status','zip','bytes','sha256','entries','CRC','hash_copy']}))
if __name__=='__main__':main()
