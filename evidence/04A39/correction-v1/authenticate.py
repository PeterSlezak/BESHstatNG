"""Freeze the NOT_READY checkpoint before changing a reporting implementation."""
from common import *
import shutil
import zipfile

assert git('branch','--show-current').decode().strip()=='codex/04A-39'
assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()=='ba1636f5356ac761509c2cfa16fc66b92430935d'
assert not git('diff','--check') and not git('diff','--cached','--name-only')
delivery=load(PRIOR/'final/delivery-receipt-v1.json')
assert delivery['status']=='NOT_READY_04A39'
PRIVATE.mkdir(exist_ok=False)
authority=[]
for value in delivery.values():
    if isinstance(value,dict) and {'path','sha256','bytes'}<=value.keys():
        assert pin(value['path'])==value
        authority.append(value)
index=pin('.git/index')
assert index==load(ROOT/delivery['Git']['path'])['index']
source=load(PRIOR/'matrix/final-source-state-v1.json')['source_pins']
files=[pin(name) for name in sorted(source)]
assert all(row['sha256']==source[row['path']] for row in files)
for name in ['planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.14.md']:
    if name not in source:files.append(pin(name))
save('entry/source-lock.json',dict(files=files,index=index,main=MAIN,branch='codex/04A-39'))
snapshot=PRIVATE/'checkpoint';snapshot.mkdir()
old=[]
for p in sorted(PRIOR.rglob('*')):
    if not p.is_file() or p.is_relative_to(HERE) or '__pycache__' in p.parts:continue
    row=pin(p);st=p.stat();row.update(device=st.st_dev,file_id=st.st_ino)
    old.append(row)
    target=snapshot/p.relative_to(ROOT);target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write(p.read_bytes())
    assert digest(target)==row['sha256']
save(PRIVATE/'checkpoint-files.json',dict(files=old,count=len(old),authority=authority))
packager=Path('C:/Users/peter/source/repos/zip_BESHStatNGv6.ps1')
other=Path('C:/Users/peter/Downloads/zip_BESHStatNGv6.ps1')
assert digest(packager)==digest(other)=='443528e30208a08f959cc37bbe3f4064944ce3639c4bed8c25090b9dc42c3a06'
shutil.copy2(packager,PRIVATE/packager.name)
external=Path('C:/Users/peter/source/repos/BESHStatNG_04A39_REVIEW_20261114-161951.zip')
with zipfile.ZipFile(external) as z:
    data=z.read('_review_package/REVIEW_INDEX.json')
    with (PRIVATE/'v6-index.json').open('xb') as f:f.write(data)
    v6=json.loads(data)
    names=z.namelist()
save(PRIVATE/'external-package.json',dict(path=str(external),bytes=external.stat().st_size,sha256=digest(external),index_sha256=sha(data),payload_count=v6['payload_count'],members=len(names)))
git_receipts=[]
for args in [('status','--porcelain=v1','--untracked-files=all'),('diff','--name-only'),('diff','--cached','--name-only'),('diff','--check')]:
    b=git(*args);git_receipts.append(dict(args=args,stdout=b.decode(),sha256=sha(b),exit_code=0))
save(PRIVATE/'entry-raw.json',dict(git=git_receipts,authority=authority,index=index,utc=utc()))
save('entry/authentication.json',dict(status='NOT_READY_CHECKPOINT_FROZEN',main=MAIN,parent='ba1636f5356ac761509c2cfa16fc66b92430935d',
    branch='codex/04A-39',index_sha256=index['sha256'],prior_checkpoint_files=len(old),
    protected_snapshot=pin(PRIVATE/'checkpoint-files.json'),source_lock=pin(HERE/'entry/source-lock.json'),
    packager=dict(name=packager.name,bytes=packager.stat().st_size,sha256=digest(packager)),
    external_v6=dict(bytes=external.stat().st_size,sha256=digest(external),payloads=v6['payload_count'],raw_exclusions=v6['excluded']['private_raw_diagnostics']),
    prior_codex=dict(bytes=delivery['archive']['bytes'],sha256=delivery['archive']['sha256'],payloads=1956),gates=GATES,
    source_changes=False,staged_empty=True,diff_check_exit=0,notes=['Initial root-level v6 path absent; identical actual scripts found in owner repos parent and Downloads. Neither executed or modified.']))
print('Frozen',len(old),'prior evidence files;source pins',len(files),'v6 payloads',v6['payload_count'],flush=True)
