"""Versioned real-byte private preservation audit; bounded current roots only."""
import sys,json,hashlib,stat
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREVIOUS=__PREVIOUS__
ADDENDUM=__ADDENDUM__
def digest(path):
 with path.open('rb') as f:return hashlib.file_digest(f,'sha256').hexdigest()
def need(value,message):
 if not value:raise ValueError(message)
def pin(path):return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest(path))
def check_pin(row):need(pin(ROOT/row['path'])==row,'Protected real bytes changed '+row['path'])
def inspect(manifest,overrides=None):
 overrides=overrides or {};path=Path(manifest);doc=json.loads(path.read_bytes())
 check_pin(PREVIOUS);check_pin(ADDENDUM)
 old=json.loads((ROOT/PREVIOUS['path']).read_bytes());post=json.loads((ROOT/ADDENDUM['path']).read_bytes())
 need(len(old['private_pins'])==21208 and len(old['retained_prior_physical_archives'])==101 and post['count']==31,'Inherited private denominator changed')
 need(doc['previous']==PREVIOUS and doc['previous_postseal']==ADDENDUM,'Wrong predecessor authority')
 check_pin(doc['reader']);need(doc['reader']['path']==Path(__file__).resolve().relative_to(ROOT).as_posix(),'Wrong actual private reader')
 rows={r['path']:r for r in doc['private_pins']};need(len(rows)==len(doc['private_pins']),'Duplicate private file')
 for r in old['private_pins']+post['postseal_private_pins']:need(rows.get(r['path'])==r,'Omitted or rewritten historical private pin '+r['path'])
 archives={r['path']:r for r in doc['retained_physical_archives']};need(len(archives)==len(doc['retained_physical_archives']),'Duplicate physical archive')
 for r in old['retained_prior_physical_archives']:need(archives.get(r['path'])==r,'Omitted physical historical archive')
 giant=doc['retained_interrupted_aggregation'];need(giant==old['retained_interrupted_aggregation'] and (giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb'),'Malformed v9 authority changed')
 checked={};identities=[]
 for r in list(rows.values())+list(archives.values())+[giant]:
  name=r['path'];p=(ROOT/name).resolve();need(p.is_relative_to(ROOT.resolve()),'Private path escaped workspace')
  if name not in checked:
   if name in overrides:
    value=overrides[name];need(value is not None,'Missing private file '+name);checked[name]=(len(value),hashlib.sha256(value).hexdigest())
   else:
    s=p.stat();need(stat.S_ISREG(s.st_mode) and not p.is_symlink(),'Private file identity is not a real ordinary file '+name)
    checked[name]=(s.st_size,digest(p));identities.append(dict(path=name,device=s.st_dev,file_id=s.st_ino,bytes=s.st_size))
  need(checked[name]==(r['bytes'],r['sha256']),'Private/source/archive byte mismatch '+name)
 for root in doc['bounded_current_roots']:
  name=root['path'];need(name.startswith(('.test-rationalization-04A38.tmp/completion-w2-cont16','.test-rationalization-04A38.tmp/commands/w2c16-','.test-rationalization-04A38.tmp/builds/w2c16-','.test-rationalization-04A38.tmp/executions/w2c16-')),'Unbounded private traversal requested')
  actual=sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/name).rglob('*') if p.is_file())
  need(actual==root['files'] and all(p in rows for p in actual),'Current bounded private file omitted or added '+name)
 return dict(status='ACTUAL_PRIVATE_FILE_IDENTITIES_BYTES_AND_PHYSICAL_ARCHIVE_HASHES_PASS',private_pin_count=len(rows),inherited_private_count=21208,inherited_postseal_count=31,physical_archives=len(archives),inherited_physical_archives=101,real_file_identities=identities,unique_files_hashed=len(checked),giant=giant,ignored_TestResults='NOT_ENUMERATED',original_queries=0)
if __name__=='__main__':
 try:
  need(len(sys.argv)==3 and sys.argv[1]=='--verify','Use --verify <versioned manifest>')
  print(json.dumps(inspect(Path(sys.argv[2]))),flush=True)
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:print(json.dumps(dict(status='PRIVATE_PRESERVATION_FAILED',error=str(e))),flush=True);raise SystemExit(1)
