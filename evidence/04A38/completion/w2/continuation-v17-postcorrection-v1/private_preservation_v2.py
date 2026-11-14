"""Versioned private preservation: retain exact old pins, add only this named run, verify actual bytes and file IDs."""
from common import *
import stat
from concurrent.futures import ThreadPoolExecutor
load=lambda p:json.loads(Path(p).read_bytes())
def identity(p):
 s=Path(p).stat();assert stat.S_ISREG(s.st_mode)
 return dict(device_hex=format(s.st_dev,'x'),file_id_hex=format(s.st_ino,'x'))
def prepare(version):
 base=PREVIOUS/'retained-private-preservation-v2.json';add=W2/'continuation-v17-postseal-private-addendum-v1.json';final=W2/'continuation-v17-finalizer-raw-binding-v1.json'
 b=load(base);a=load(add);f=load(final)
 assert b['private_pin_count']==len(b['private_pins'])==29489 and len(b['retained_physical_archives'])==141
 pins={r['path']:r for r in b['private_pins']};ids={r['path']:{k:v for k,v in r.items() if k!='path'} for r in b['current_filesystem_identities']};archives={r['path']:r for r in b['retained_physical_archives']}
 for r in a['postseal_private_pins']+f['raw']:
  assert pins.setdefault(r['path'],r)==r
 inherited=set(pins)
 roots=[PRIVATE,RUNROOT/'c17pc1-py']
 for group in ['builds','executions','commands']:
  for path in (RUNROOT/group).iterdir():
   if path.is_dir() and path.name.startswith('w2c17pc1-') and (group!='commands' or (path/'command.json').exists()):roots.append(path)
 for root in roots:
  assert root.resolve().is_relative_to(RUNROOT.resolve()) and root!=RUNROOT
  for path in root.rglob('*'):
   if not path.is_file():continue
   assert not path.is_symlink() and path.resolve().is_relative_to(root.resolve())
   name=path.relative_to(ROOT).as_posix()
   if name not in pins:pins[name]=pin(path)
   if path.name.endswith(('.zip','.zip.001','.zip.002')):archives[name]=pins[name]
 rows=[pins[k] for k in sorted(pins)];assert len(rows)==len({r['path'].casefold() for r in rows})
 for r in rows:
  if r['path'] not in ids:ids[r['path']]=identity(ROOT/r['path'])
 save('retained-private-preservation-v'+str(version)+'.json',dict(private_pin_count=len(rows),private_pins=rows,current_filesystem_identities=[dict(path=r['path'],**ids[r['path']]) for r in rows],retained_physical_archives=[archives[k] for k in sorted(archives)],retained_interrupted_aggregation=b['retained_interrupted_aggregation'],previous=pin(base),previous_postseal=pin(add),previous_finalizer_raw=pin(final),inherited_pin_union=len(inherited),historical_TestResults='NOT_ENUMERATED',roots=[r.relative_to(ROOT).as_posix() for r in roots],scope='All old byte and existing file-identity pins retained verbatim. Only this run enumerated; ongoing final-audit command and later additive evidence separately bound.'))
 print('Prepared preservation',len(rows),len(archives),flush=True)
def verify(manifest,output):
 d=load(manifest);pins={r['path']:r for r in d['private_pins']};ids={r['path']:{k:v for k,v in r.items() if k!='path'} for r in d['current_filesystem_identities']}
 for r in load(PREVIOUS/'retained-private-preservation-v2.json')['private_pins']+load(W2/'continuation-v17-postseal-private-addendum-v1.json')['postseal_private_pins']+load(W2/'continuation-v17-finalizer-raw-binding-v1.json')['raw']:assert pins[r['path']]==r
 oldids=load(PREVIOUS/'retained-private-preservation-v2.json')['current_filesystem_identities']
 for r in oldids:assert ids[r['path']]=={k:v for k,v in r.items() if k!='path'}
 assert list(pins)==sorted(pins) and len(pins)==d['private_pin_count'] and len({n.casefold() for n in pins})==len(pins)
 def check(r):
  path=ROOT/r['path'];assert not path.is_symlink() and path.resolve().is_relative_to(ROOT)
  assert pin(path)==r,'PRIVATE_BYTES_CHANGED '+r['path'];assert identity(path)==ids[r['path']],'FILE_ID_CHANGED '+r['path']
  return r['path']
 with ThreadPoolExecutor(max_workers=4) as pool:checked=list(pool.map(check,d['private_pins']))
 assert set(checked)==set(pins)
 for r in d['retained_physical_archives']:assert pins[r['path']]==r
 g=d['retained_interrupted_aggregation'];assert pins[g['path']]==g and g['bytes']==4213129746 and g['sha256']=='3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb'
 result=dict(status='ACTUAL_ALL_PINNED_BYTES_FILESYSTEM_IDS_AND_PHYSICAL_ZIP_SHA_PASS',manifest=pin(manifest),private_pins=len(pins),physical_archives=len(d['retained_physical_archives']),historical_TestResults='NOT_ENUMERATED',giant='STREAM_HASH_ONLY_NOT_PARSED_OR_PACKAGED',workers=4,original_queries=0)
 with Path(output).open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2);f.write('\n')
 print(json.dumps(result),flush=True)
if __name__=='__main__':
 if sys.argv[1]=='prepare':prepare(int(sys.argv[2]))
 elif sys.argv[1]=='verify':verify(Path(sys.argv[2]),Path(sys.argv[3]))
 else:raise ValueError(sys.argv[1])
