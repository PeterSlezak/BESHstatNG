"""Local review archive only; exact manifest bytes, CRC and independent copy verification."""
import shutil,zipfile
from common import *
from verify_publication import inspect
from verify_preservation import inspect as preservation
def hashfile(p):
 with p.open('rb') as f:return __import__('hashlib').file_digest(f,'sha256').hexdigest()
manifest=(HERE/'publication-manifest-v3.json').relative_to(ROOT).as_posix();proof=inspect(manifest_path=manifest);records=[]
for label,script,args in [('final-public-local-v3','verify_publication.py',[manifest]),('final-public-copy-v3','check_public_copy.py',[manifest]),('final-diagnostic-derivative','verify_diagnostic_derivative.py',[])]:
 r=command(label,[sys.executable,'-B',str(HERE/script),*args]);records.append(dict(label=label,command=clean(r)));assert r['exit_code']==0
rows=json.loads((ROOT/manifest).read_bytes())['entries'];folder=RUNROOT/'exports';folder.mkdir(exist_ok=True);name='BESHstatNG_04A38_W2_continuation_v2_20261110_review.zip';path=folder/name;assert not path.exists()
with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
 for r in rows:z.write(ROOT/r['path'],arcname=r['path'])
with zipfile.ZipFile(path) as z:
 crc=z.testzip();assert crc is None;names=z.namelist();assert names==[r['path'] for r in rows] and len(names)==len(set(names))
copy=folder/name.replace('.zip','.hash-copy.zip');assert not copy.exists();shutil.copyfile(path,copy);digest=hashfile(path);assert hashfile(copy)==digest
live=preservation();assert not git('diff','--cached','--name-only');diff=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert diff.returncode==0
result=dict(status='VERIFIED_LOCAL_DERIVATIVE_REVIEW_ARCHIVE_NOT_READY',zip=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest,entries=len(rows),CRC='PASS_ALL_ENTRIES',independent_copy_hash='PASS_IDENTICAL_BYTES',manifest=pin(ROOT/manifest),readers=records,manifest_check=proof,preservation=live,branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,status_paths=clean(git('status','--porcelain=v1','--untracked-files=all').decode()),limitations='Derivative includes every intended v3 payload. Explicit exclusions in file-inventory-v3.json and diagnostic-public-derivative.json; live Git/private loaded DLLs/omitted owner files/ignored history cannot be reverified from ZIP. Raw full-managed receipt remains local; only two display labels differ in separately identified public derivative.',new_original_queries=0,owner_acceptance='NOT_GRANTED',Git_publication='NONE',W2='IN_PROGRESS',W3='NOT_STARTED',Phase04A39='NOT_STARTED',gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'))
receipt=folder/'BESHstatNG_04A38_W2_continuation_v2_20261110_export.json'
with receipt.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
print(json.dumps({k:result[k] for k in ['status','zip','bytes','sha256','entries','CRC','independent_copy_hash','branch','HEAD','main','merge_base','index','diff_check_exit']}))
