"""Bounded append-only pin inventory and actual-byte preservation reader."""
from common import *
import stat
load=lambda p:json.loads(Path(p).read_bytes())
def key(p):return p.replace('\\','/')
def file_identity(path):
 s=Path(path).stat();assert stat.S_ISREG(s.st_mode)
 return dict(device_hex=format(s.st_dev,'x'),file_id_hex=format(s.st_ino,'x'))
def validate_file(row,identity):
 p=ROOT/row['path'];assert not p.is_symlink() and p.resolve().is_relative_to(ROOT)
 assert pin(p)==row,'PRIVATE_BYTE_PRESERVATION_FAILED '+row['path']
 observed=file_identity(p);assert observed==identity,'FILESYSTEM_IDENTITY_CHANGED '+row['path']
 return observed
def prepare(version,parts=None):
 base=W2/'continuation-v16-retained-private-preservation-v2.json';add=W2/'continuation-v16-postseal-private-addendum-v2.json';b=load(base);a=load(add)
 assert b['private_pin_count']==len(b['private_pins'])==23711 and len(b['retained_physical_archives'])==133 and a['count']==len(a['postseal_private_pins'])==102
 pins={};archives={}
 def retain(row):
  assert set(row)=={'path','bytes','sha256'};p=key(row['path']);assert p==row['path']
  assert pins.setdefault(p,row)==row,'Conflicting immutable pin '+p
 for r in b['private_pins']+a['postseal_private_pins']:retain(r)
 inherited=set(pins)
 for r in b['retained_physical_archives']:retain(r);archives[r['path']]=r
 giant=b['retained_interrupted_aggregation'];retain(giant)
 assert giant['bytes']==4213129746 and giant['sha256']=='3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb'
 roots=[PRIVATE]
 for folder in ['builds','executions','commands']:
  for p in (RUNROOT/folder).iterdir():
   if p.is_dir() and p.name.startswith('w2c17-'):
    if folder=='commands' and (p.name.startswith('w2c17-postseal-') or not (p/'command.json').is_file()):continue
    roots.append(p)
 # Only the new continuation's named roots are enumerated. Root/TestResults,
 # previous run roots and historical exclusions are never walked.
 new=[]
 for root in sorted(roots,key=lambda p:p.as_posix()):
  assert root.resolve().is_relative_to(RUNROOT.resolve()) and 'TestResults' not in root.relative_to(RUNROOT).parts
  for path in sorted(root.rglob('*'),key=lambda p:p.as_posix()):
   if not path.is_file():continue
   assert not path.is_symlink() and path.resolve().is_relative_to(root.resolve())
   relative=path.relative_to(ROOT).as_posix()
   if relative in pins:continue
   r=pin(path);pins[relative]=r;new.append(relative)
   if path.name.lower().endswith(('.zip','.zip.001','.zip.002')):archives[relative]=r
 if parts:
  for r in load(parts)['parts']:
   record={k:r[k] for k in ['path','bytes','sha256']};retain(record);archives[record['path']]=record
 rows=[pins[k] for k in sorted(pins)];assert len({r['path'].casefold() for r in rows})==len(rows)
 identities=[dict(path=r['path'],**file_identity(ROOT/r['path'])) for r in rows]
 save('retained-private-preservation-v'+str(version)+'.json',dict(status='EXACT_INHERITED_PINS_PLUS_BOUNDED_CURRENT_RUN_FILES',previous=pin(base),previous_postseal=pin(add),inherited_private_count=23711,inherited_postseal_count=102,inherited_pin_union=len(inherited),private_pins=rows,private_pin_count=len(rows),retained_physical_archives=[archives[k] for k in sorted(archives)],retained_interrupted_aggregation=giant,current_filesystem_identities=identities,bounded_current_roots=[p.relative_to(ROOT).as_posix() for p in roots],historical_TestResults='NOT_ENUMERATED',original_queries=0,scope='No historical pin recaptured or weakened. All inherited expected byte hashes retained verbatim. Current file IDs are recorded separately as a live-filesystem binding; they do not manufacture historical file-ID observations.',excluded_future_raw='Only currently running command roots and explicitly named postseal roots; additive completed raw pins are recorded after final audit.'))
 print('Prepared expected pin inventory',len(rows),'physical archives',len(archives),'without traversing historical roots',flush=True)
def verify(manifest,output):
 d=load(manifest);rows=d['private_pins'];assert rows==sorted(rows,key=lambda r:r['path']) and len(rows)==d['private_pin_count']
 before=load(ROOT/d['previous']['path']);post=load(ROOT/d['previous_postseal']['path']);indexed={r['path']:r for r in rows}
 assert d['inherited_private_count']==23711 and d['inherited_postseal_count']==102 and len(rows)>=23813 and len(d['retained_physical_archives'])>=133
 for r in before['private_pins']+post['postseal_private_pins']:assert indexed[r['path']]==r,'Inherited expected pin omitted or re-pinned'
 ids={r['path']:r for r in d['current_filesystem_identities']};actual=[];seen=set();start=time.monotonic()
 for r in rows:
  expected={k:v for k,v in ids[r['path']].items() if k!='path'};observed=validate_file(r,expected)
  actual.append(dict(path=r['path'],**observed));assert r['path'] not in seen;seen.add(r['path'])
  if len(actual)%3000==0:print('Actual immutable pins',len(actual),'/',len(rows),flush=True)
 for r in d['retained_physical_archives']:assert r['path'] in seen and next(p for p in rows if p['path']==r['path'])==r
 giant=d['retained_interrupted_aggregation'];assert giant['path'] in seen
 result=dict(status='ACTUAL_ALL_PRIVATE_BYTES_FILESYSTEM_IDENTITIES_AND_PHYSICAL_ARCHIVE_SHA_PASS',manifest=pin(manifest),private_pins=len(rows),physical_archives=len(d['retained_physical_archives']),file_identities=actual,seconds=time.monotonic()-start,giant_aggregation='STREAM_HASH_ONLY;NOT_PARSED_OR_SERIALIZED;NOT_IN_DERIVATIVE',historical_TestResults='NOT_ENUMERATED',original_queries=0)
 with Path(output).open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2);f.write('\n')
 print('Actual final preservation PASS',len(rows),len(d['retained_physical_archives']),flush=True)
if __name__=='__main__':
 if sys.argv[1]=='prepare':prepare(int(sys.argv[2]),Path(sys.argv[3]) if len(sys.argv)>3 else None)
 elif sys.argv[1]=='verify':verify(Path(sys.argv[2]),Path(sys.argv[3]))
 else:raise ValueError(sys.argv[1])
