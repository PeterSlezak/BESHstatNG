"""Read-only exact complementary union/physical ZIP and actual final reader replay.

The authoritative external index must accompany the locally produced archives.
No private/raw excluded file is inferred to have been independently checked in a copy.
"""
import hashlib,json,sys,zipfile,stat,zlib,importlib.util,argparse
from pathlib import Path
from collections import Counter
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE))
from publication_privacy_c16_v1 import safe_path
PREFIX='evidence/04A38/completion/w2/continuation-v16/'
LIMIT=180*1024**2

def need(v,m):
 if not v:raise ValueError(m)

def file_pin(path):
 with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
 return path.stat().st_size,digest

def inspect(root,indexpath,physical=False,check_checkpoint=True,check_preservation=True,parts_dir=None):
 root=Path(root);indexpath=Path(indexpath);index=json.loads(indexpath.read_bytes());manifestpath=root/(PREFIX+'publication-manifest-v1.json')
 need(file_pin(manifestpath)==(index['manifest']['bytes'],index['manifest']['sha256']),'External index/manifest authority differs')
 manifest=json.loads(manifestpath.read_bytes());rows=manifest['entries'];files={r['path']:r for r in index['files']}
 priorpath=root/'evidence/04A38/completion/w2/continuation-v15-review-parts-index-v2.json'
 need(file_pin(priorpath)==(index['previous_index']['bytes'],index['previous_index']['sha256']),'Final v15 index authority changed')
 prior=json.loads(priorpath.read_bytes());current={r['path']:r for r in index['files']}
 transitions={r['path']:r for r in json.loads((root/(PREFIX+'source-transitions-final-v1.json')).read_bytes())['files']}
 for r in prior['files']:
  need(r['path'] in current,'Historical published payload omitted '+r['path'])
  now=current[r['path']]
  if (r['bytes'],r['sha256'])!=(now['bytes'],now['sha256']):
   t=transitions.get(r['path']);need(t is not None and t['before'] is not None,'Unrecorded changed historical payload')
   need((r['bytes'],r['sha256'])==(t['before']['bytes'],t['before']['sha256']) and (now['bytes'],now['sha256'])==(t['after']['bytes'],t['after']['sha256']),'Historical payload transition not exact')

 need(len(files)==len(index['files'])==len(rows) and len(files)==len({p.casefold() for p in files}),'Duplicate/missing logical path')
 need(set(files)=={r['path'] for r in rows}==set(json.loads((root/(PREFIX+'file-inventory-v1.json')).read_bytes())['paths']),'Incomplete logical inventory/union')
 excluded=json.loads((root/(PREFIX+'review-exclusions-v1.json')).read_bytes());need(not set(files)&{r['path'] for r in excluded['excluded']},'Excluded raw payload injected')
 parts={p['name']:p for p in index['parts']};need(len(parts)==len(index['parts']) and all(0<p['bytes']<LIMIT for p in parts.values()),'Invalid physical part cardinality/size')
 policy=manifest['privacy_policy'];need(file_pin(root/policy['path'])==(policy['bytes'],policy['sha256']),'Privacy policy source changed')
 for row in rows:
  path=row['path'];safe_path(path);r=files[path]
  need(r['logical_root_prefix']=='' and r['part'] in parts,'Wrong logical root/part')
  need(file_pin(root/path)==(r['bytes'],r['sha256']),'Logical payload bytes changed '+path)
  if row['sha256'] is None:need(path==PREFIX+'publication-manifest-v1.json','Unexpected null self-entry')
  else:
   need((row['bytes'],row['sha256'])==(r['bytes'],r['sha256']),'Manifest/member pin differs')
   decision=row['privacy_decision'];need(decision['status']=='POLICY_EVALUATED' and decision['policy_sha256']==policy['sha256'] and decision['content_sha256']==r['sha256'],'Missing exact policy/content decision')
 for part in parts.values():need(part['files']==sum(r['part']==part['name'] for r in files.values()),'Wrong part file count')
 actual_physical=[]
 if physical:
  union=[]
  for name,part in parts.items():
   safe_path(name);need(Path(name).name==name,'Physical archive name must be a basename')
   base=Path(parts_dir) if parts_dir is not None else root
   path=base/name if parts_dir is not None else root/part['path'];need(path.resolve().is_relative_to(base.resolve()) and path.name==name,'Physical archive path escaped supplied directory')
   need(file_pin(path)==(part['bytes'],part['sha256']),'Actual physical archive SHA/size differs')
   expected=sorted((r for r in files.values() if r['part']==name),key=lambda r:r['path'])
   with zipfile.ZipFile(path) as archive:
    need(archive.namelist()==[r['path'] for r in expected],'Physical extra/missing/duplicate member')
    for row in expected:
     info=archive.getinfo(row['path']);safe_path(info.filename);need(not info.flag_bits&1 and not info.is_dir() and not stat.S_ISLNK(info.external_attr>>16),'Encrypted/link/directory payload forbidden')
     data=archive.read(info.filename);need((len(data),hashlib.sha256(data).hexdigest())==(row['bytes'],row['sha256']),'Physical uncompressed bytes differ')
     need(info.CRC==zlib.crc32(data)&0xffffffff and format(info.CRC,'08x')==row['stored_CRC32'],'Stored/actual CRC differs');union.append(info.filename)
    need(archive.testzip() is None,'Physical ZIP integrity failed')
   repro=part['deterministic_reproduction'];repro_path=root/repro['path']
   if repro_path.is_file():
    need(file_pin(repro_path)==(repro['bytes'],repro['sha256'])==(part['bytes'],part['sha256']),'Physical deterministic reproduction changed');repro_status='ACTUAL_SECOND_SERIALIZATION_VERIFIED'
   else:repro_status='NOT_SUPPLIED_IN_REVIEW;LOCAL_BOUND_HASH_ONLY'
   actual_physical.append(dict(name=name,bytes=part['bytes'],sha256=part['sha256'],members=len(expected),CRC='ALL_VERIFIED',deterministic_reproduction=repro_status))
  need(Counter(union)==Counter(files.keys()),'Complementary physical union differs')
 seal=json.loads((root/(PREFIX+'review-seal-v2.json')).read_bytes());reader=seal['reader'];need(file_pin(root/reader['path'])==(reader['bytes'],reader['sha256']),'Final checkpoint reader differs')
 if check_checkpoint:
  spec=importlib.util.spec_from_file_location('c15_final_checkpoint',root/reader['path']);module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module);checkpoint=module.compute(root)
 else:checkpoint='NOT_REQUESTED_FOR_PHYSICAL_ONLY_CHECK'
 if check_preservation:
  from verify_c16_preservation_v1 import inspect as preservation
  protected=preservation(root,live=(root/'.git').is_dir())
 else:protected='NOT_REQUESTED_FOR_PHYSICAL_ONLY_CHECK'
 return dict(status='COMPLEMENTARY_REVIEW_READERS_HASH_CRC_PASS_NOT_READY',files=len(files),parts=len(parts),physical_archives=actual_physical,checkpoint=checkpoint,preservation=protected,private='EXPLICIT_LIVE_EXTERNAL_PRIVATE_READER_RECEIPT_REQUIRED;NOT_INFERRED_FROM_COPY',privacy='EXACT_BYTE_BOUND_SELECTED_POLICY_DECISIONS;HISTORICAL_PYTHON_ERRORS_NOT_WAIVED',standalone='NOT_CREATED',original_queries=0,phase04A39='BLOCKED')

if __name__=='__main__':
 parser=argparse.ArgumentParser();parser.add_argument('--index',type=Path,required=True);parser.add_argument('--parts',action='store_true');parser.add_argument('--parts-dir',type=Path);parser.add_argument('--physical-only',action='store_true');args=parser.parse_args()
 try:print(json.dumps(inspect(ROOT,args.index,args.parts,not args.physical_only,not args.physical_only,args.parts_dir)))
 except (AssertionError,ValueError,OSError,KeyError,TypeError,zipfile.BadZipFile) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
