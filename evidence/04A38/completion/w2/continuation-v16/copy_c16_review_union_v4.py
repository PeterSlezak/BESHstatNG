"""Extract only verified complementary payloads into a fresh .git-free review copy."""
from common import *
import zipfile,zlib,stat
from publication_privacy_c16_v1 import safe_path
indexpath=W2/'continuation-v16-review-parts-index-v3.json';index=json.loads(indexpath.read_bytes())
copyroot=RUNROOT/'r4';assert copyroot.resolve().is_relative_to(ROOT.resolve());copyroot.mkdir(exist_ok=False)
expected={r['path']:r for r in index['files']};assert len(expected)==len(index['files'])==len({p.casefold() for p in expected})
assert not any(p.startswith('.git/') for p in expected)
observed=[]
for part in index['parts']:
 path=ROOT/part['path'];assert pin(path)=={k:part[k] for k in ['path','bytes','sha256']}
 with zipfile.ZipFile(path) as archive:
  names=archive.namelist();assert len(names)==len(set(names))
  for info in archive.infolist():
   name=info.filename;safe_path(name);assert name in expected and expected[name]['part']==part['name']
   assert not info.flag_bits&1 and not info.is_dir() and not stat.S_ISLNK(info.external_attr>>16)
   data=archive.read(name);row=expected[name]
   assert (len(data),sha(data))==(row['bytes'],row['sha256']) and info.CRC==zlib.crc32(data)&0xffffffff and format(info.CRC,'08x')==row['stored_CRC32']
   target=copyroot/name;assert target.resolve().is_relative_to(copyroot.resolve());target.parent.mkdir(parents=True,exist_ok=True)
   with target.open('xb') as f:f.write(data)
   observed.append(name)
assert len(observed)==len(set(observed))==len(expected) and set(observed)==set(expected) and not (copyroot/'.git').exists()
folder=copyroot/'evidence/04A38/completion/w2/continuation-v16'
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
result=command('postseal-copied-union-reader-v2',[sys.executable,'-I','-B','-c',bootstrap,str(folder),str(folder/'verify_c16_publication_v4.py'),'--index',str(indexpath)],cwd=copyroot)
receipt=W2/'continuation-v16-copied-review-reader-v2.json'
with receipt.open('x',encoding='utf-8') as f:json.dump(dict(status='PASS' if result['exit_code']==0 else 'STOP_COPY_READER_FAILED',command=clean(result),copied_files=len(observed),index=pin(indexpath),git_metadata='ABSENT',standalone='NOT_CREATED',scope='Actual reader in the freshly extracted logical union, isolated imports, current Windows Python. No production installation or cross-platform standalone equivalence claim.',original_queries=0),f,indent=2)
assert result['exit_code']==0,'Copied reader failed; preserve derivative and STOP'
print('Actual complementary copied-union reader PASS',len(observed))
