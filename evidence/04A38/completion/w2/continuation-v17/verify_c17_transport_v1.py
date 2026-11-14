"""Git-free review verification. It never attests live Git/private/test execution."""
import sys,json,hashlib,zipfile,stat,unicodedata,zlib,shutil
from pathlib import Path,PurePosixPath
META='.review-manifest.json'
def safe_path(name):
 assert isinstance(name,str) and name and '\\' not in name and ':' not in name and '\x00' not in name and not name.startswith('/')
 p=PurePosixPath(name);assert p.as_posix()==name and all(c not in ['.','..',''] and not c.endswith((' ','.')) for c in p.parts)
 assert unicodedata.normalize('NFC',name)==name and not name.startswith(('.git/','.test-rationalization-04A38.tmp/'))
 return name
def digest(path):
 with Path(path).open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def validate_manifest(d):
 paths=[safe_path(r['path']) for r in d['files']]
 assert len(paths)==len(set(paths))==len({p.casefold() for p in paths})
 assert META not in paths and all(r['bytes']>=0 and len(r['sha256'])==64 and len(r['crc32'])==8 for r in d['files'])
 assert d['scope']=='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT' and d['private_raw_included'] is False
 assert all('continuation-v9/python-first-failures-v1.json' not in p for p in paths)
def verify(index_path,parts_dir,extract=None):
 index=json.loads(Path(index_path).read_bytes());parts_dir=Path(parts_dir);allrows={r['path']:r for r in index['files']};seen=set();common=None;root=Path(extract).resolve() if extract else None
 if root:root.mkdir(parents=True,exist_ok=False)
 for part in index['parts']:
  path=parts_dir/part['name'];assert path.name==part['name'] and path.stat().st_size==part['bytes'] and digest(path)==part['sha256']
  with zipfile.ZipFile(path) as archive:
   names=archive.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names}) and META in names
   data=archive.read(META);assert hashlib.sha256(data).hexdigest()==index['embedded_manifest_sha256']
   if common is None:common=data;manifest=json.loads(data);validate_manifest(manifest);assert manifest['files']==index['files']
   else:assert data==common
   expected=[r['path'] for r in index['files'] if r['part']==part['name']]
   assert set(names)==set(expected)|{META} and part['files']==len(expected)
   for name in names:
    if name!=META:safe_path(name)
    info=archive.getinfo(name);assert not info.is_dir() and not info.flag_bits&1 and not stat.S_ISLNK(info.external_attr>>16)
    if name==META:
     if root and not (root/META).exists():(root/META).write_bytes(data)
     continue
    assert name not in seen;seen.add(name);row=allrows[name]
    assert info.file_size==row['bytes'] and format(info.CRC,'08x')==row['crc32']
    h=hashlib.sha256();crc=0;count=0;destination=root/name if root else None
    if destination:
     assert destination.resolve().is_relative_to(root);destination.parent.mkdir(parents=True,exist_ok=True)
    target=destination.open('xb') if destination else None
    try:
     with archive.open(info) as stream:
      for chunk in iter(lambda:stream.read(1024*1024),b''):
       h.update(chunk);crc=zlib.crc32(chunk,crc);count+=len(chunk)
       if target:target.write(chunk)
    finally:
     if target:target.close()
    assert count==row['bytes'] and h.hexdigest()==row['sha256'] and format(crc&0xffffffff,'08x')==row['crc32']
 assert seen==set(allrows)
 if root:assert not (root/'.git').exists()
 return dict(status='ALL_LOGICAL_PAYLOAD_SHA_SIZE_CRC_AND_PHYSICAL_PART_SHA_PASS',files=len(seen),parts=len(index['parts']),embedded_manifest_identical=True,logical_duplicate_payloads=0,scope='COPIED_GIT_FREE_REVIEW_DERIVATIVE;LIVE_GIT_PRIVATE_BYTES_AND_EXECUTION_NOT_ATTESTED',actual_external_upload='NOT_PERFORMED_BY_AGENT',original_queries=0)
if __name__=='__main__':
 try:print(json.dumps(verify(sys.argv[1],sys.argv[2],sys.argv[3] if len(sys.argv)>3 else None)),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,zipfile.BadZipFile) as e:print(json.dumps(dict(status='REJECTED',error=str(e))),flush=True);raise SystemExit(1)
