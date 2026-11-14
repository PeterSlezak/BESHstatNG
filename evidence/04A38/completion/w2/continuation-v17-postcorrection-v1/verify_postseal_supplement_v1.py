"""Standalone postseal supplement verifier; no Git/private inputs or test execution."""
import sys,json,hashlib,zipfile,unicodedata,stat,zlib
from pathlib import Path,PurePosixPath

def safe(name):
 assert isinstance(name,str) and name and not any(c in name for c in ['\\',':','\0']) and not name.startswith('/')
 assert PurePosixPath(name).as_posix()==name and all(c not in ['.','..',''] and not c.endswith((' ','.')) for c in PurePosixPath(name).parts)
 assert unicodedata.normalize('NFC',name)==name and not name.startswith(('.git/','.test-rationalization-04A38.tmp/'))
 return name

def verify(receipt,archive):
 r=json.loads(Path(receipt).read_bytes());p=Path(archive);pin=r['postseal_supplement']
 with p.open('rb') as f:assert hashlib.file_digest(f,'sha256').hexdigest()==pin['sha256']
 assert p.stat().st_size==pin['bytes']
 with zipfile.ZipFile(p) as z:
  names=z.namelist();assert len(names)==len(set(names))==len({s.casefold() for s in names})
  meta=z.read('.postseal-manifest.json');assert hashlib.sha256(meta).hexdigest()==r['supplement_manifest_sha256'];d=json.loads(meta)
  assert d['files']==r['supplement_files'] and not d['private_raw_included'] and not d['binary_volumes']
  assert set(names)=={row['path'] for row in d['files']}|{'.postseal-manifest.json'}
  for row in d['files']:
   name=safe(row['path']);info=z.getinfo(name);assert not info.is_dir() and not info.flag_bits&1 and not stat.S_ISLNK(info.external_attr>>16)
   data=z.read(name);assert (len(data),hashlib.sha256(data).hexdigest(),format(zlib.crc32(data)&0xffffffff,'08x'))==(row['bytes'],row['sha256'],row['crc32'])
 return dict(status='SUPPLEMENT_PHYSICAL_SHA_AND_ALL_PAYLOAD_SHA_SIZE_CRC_PASS',files=len(d['files']),scope='REVIEW_DERIVATIVE_ONLY;NOT_LIVE_GIT_PRIVATE_BYTES_OR_INDEPENDENT_TEST_EXECUTION')
if __name__=='__main__':
 try:print(json.dumps(verify(sys.argv[1],sys.argv[2])),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,zipfile.BadZipFile) as e:print(json.dumps(dict(status='REJECTED',error=str(e))),flush=True);raise SystemExit(1)
