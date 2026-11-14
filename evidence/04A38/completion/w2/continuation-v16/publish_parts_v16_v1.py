"""Complete complementary review ZIPs, exact paths/bytes/SHA/CRC and reproduction.

Review-only raw exclusions retain original local bytes and distinct safe derivatives.
"""
from common import *
import shutil,zipfile,stat,zlib,copy
import publication_privacy_c16_v1 as privacy_reader
safe_path=privacy_reader.safe_path
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=PREFIX+'publication-manifest-v1.json'
LIMIT=180*1024**2;TARGET=155*1024**2

def external(name,value):
 path=W2/name
 with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(path)

def selected():
 prior=json.loads((W2/'continuation-v15-review-parts-index-v2.json').read_bytes());names={r['path'] for r in prior['files']}
 names.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
 names.update(json.loads((HERE/'implementation-plan-v2.json').read_bytes())['source_paths'])
 names.update(r['live']['path'] for r in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files'])
 names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v15-*') if p.is_file() and p.suffix in ['.json','.py','.md'])
 names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v16-*') if p.is_file() and p.suffix in ['.json','.py','.md'])
 names.add('scripts/test_suite_rationalization.py')
 return {n for n in names if not n.startswith(('.git/','.test-rationalization-04A38.tmp/'))}


def checkpoint():
 prior=json.loads((PREVIOUS/'review-exclusions-v2.json').read_bytes());excluded={r['path'] for r in prior['excluded']}
 decisions=[];current_raw=[];derivatives=[]
 for name in sorted(selected()-excluded):
  safe_path(name);data=(ROOT/name).read_bytes();row=pin(ROOT/name)
  try:findings=privacy_reader.privacy(name,data)
  except ValueError as e:
   assert name.startswith(PREFIX),'New out-of-scope privacy exception must stop publication: '+name
   assert 'Unattested privacy finding' in str(e),'Unexpected policy error'
   current_raw.append(row)
   try:obj=json.loads(data);cleaned=json.dumps(clean(obj),indent=2,ensure_ascii=False)+'\n';suffix='.review.json'
   except (ValueError,UnicodeError):cleaned=clean(data.decode('utf-8-sig'));suffix='.review.txt'
   # Distinct review derivative: never rewrite raw authority or reuse its path/hash.
   target=HERE/'review-derivatives'/(name[len(PREFIX):]+suffix)
   try:privacy_reader.privacy(target.relative_to(ROOT).as_posix(),cleaned.encode())
   except ValueError:
    # Omit synthetic adverse values from the review without adding a privacy exception.
    target=HERE/'review-derivatives'/(name[len(PREFIX):]+'.omitted-values.review.json')
    cleaned=json.dumps(dict(raw=row,review_only='Raw synthetic adverse values omitted from review. Exact source remains preserved locally; original assertions and all failed/negative cases remain unchanged.',value_substitution='NONE_IN_AUTHORITATIVE_SOURCE'),indent=2)+'\n'
    privacy_reader.privacy(target.relative_to(ROOT).as_posix(),cleaned.encode())
   target.parent.mkdir(parents=True,exist_ok=True)
   with target.open('x',encoding='utf-8',newline='\n') as f:f.write(cleaned)
   derivative=pin(target);findings=privacy_reader.privacy(derivative['path'],target.read_bytes())
   decisions.append(dict(derivative,findings=findings));derivatives.append(dict(raw=row,review_derivative=derivative,interpretation='Machine paths cleaned only in a separately named review derivative. Raw authority remains untouched and separately preserved.'))
  else:decisions.append(dict(row,findings=findings))
 save('publication-privacy-decisions-v1.json',dict(status='ONE_SELECTED_POLICY_CHECKPOINT_WITH_EXACT_PRIOR_REUSE',base_policy=privacy_reader.base,decisions=decisions,raw_excluded=current_raw,prior_manifest=pin(PREVIOUS/'publication-manifest-v2.json'),stats=privacy_reader.stats,original_queries=0))
 save('review-exclusions-v1.json',dict(status='REVIEW_ONLY_EXCLUSIONS_NO_AUTHORITATIVE_DELETION',excluded=prior['excluded']+current_raw,current_raw_excluded=current_raw,raw_review_derivatives=prior['raw_review_derivatives']+derivatives,
  inherited=pin(PREVIOUS/'review-exclusions-v2.json'),owner_documents='All16 remain live protected. Optional omitted owner documents retain inherited exact pins; Master Plan2.14 remains included and unchanged.',private_evidence='All21208 inherited pins/101 archives plus bounded current evidence remain locally preserved. No excluded file is deleted.',ignored_history='No historical TestResults traversal or deletion.',original_queries=0))
 print('Selected policy checkpoint',len(decisions),'new raw exclusions',len(current_raw),flush=True)

def inventory():
 exclusions=json.loads((HERE/'review-exclusions-v1.json').read_bytes());forbidden={r['path'] for r in exclusions['excluded']}
 names=selected()-forbidden;names.update([PREFIX+'file-inventory-v1.json',MANIFEST]);ordered=sorted(names)
 assert len(ordered)==len({p.casefold() for p in ordered})
 save('file-inventory-v1.json',dict(status='COMPLETE_LOGICAL_C16_REVIEW_NOT_FULL_REPOSITORY',paths=ordered,logical_root='repository root; empty prefix',standalone='NOT_CREATED',original_queries=0))
 rows=[];findings=[]
 for name in ordered:
  safe_path(name)
  if name==MANIFEST:rows.append(dict(path=name,bytes=None,sha256=None));continue
  data=(ROOT/name).read_bytes();observed=privacy_reader.privacy(name,data);r=pin(ROOT/name)
  rows.append(dict(r,privacy_decision=dict(status='POLICY_EVALUATED',policy_sha256=privacy_reader.base['sha256'],content_sha256=r['sha256'],findings=observed)));findings.extend(observed)
 save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',entries=rows,privacy_policy=privacy_reader.base,privacy_checkpoint=pin(HERE/'publication-privacy-decisions-v1.json'),self_entry='One null nonrecursive self-entry; actual bytes/SHA/CRC in external parts index.',privacy_execution='Exact authenticated earlier path/size/SHA/policy decisions reused. Only new final bookkeeping receives fresh unchanged-policy delta evaluation; historical scanner campaign is not repeated.',standalone='NOT_CREATED',original_queries=0))
 findings.extend(privacy_reader.privacy(MANIFEST,(ROOT/MANIFEST).read_bytes()))
 external('continuation-v16-publication-privacy-receipt-v1.json',dict(status='SELECTED_PAYLOAD_PRIVACY_POLICY_PASS_NOT_READY',manifest=pin(ROOT/MANIFEST),files=len(rows),findings=findings,base_policy=privacy_reader.base,stats=privacy_reader.stats,checkpoint=pin(HERE/'publication-privacy-decisions-v1.json'),scope='Review-only exclusions preserve raw bytes. Both historical Python privacy JSON ERRORs remain ERROR and are not waived.',original_queries=0))
 print('Complete logical review inventory',len(rows),flush=True)

def make_zip(path,rows):
 with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as archive:
  for row in rows:
   data=(ROOT/row['path']).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
   info=zipfile.ZipInfo(row['path'],date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
   archive.writestr(info,data,compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)

def verify_zip(path,rows):
 records=[]
 with zipfile.ZipFile(path) as archive:
  names=archive.namelist();assert names==[r['path'] for r in rows] and len(names)==len(set(names))==len({n.casefold() for n in names})
  for row in rows:
   item=archive.getinfo(row['path']);safe_path(item.filename);assert not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16)
   data=archive.read(item.filename);assert (len(data),sha(data))==(row['bytes'],row['sha256']) and item.CRC==zlib.crc32(data)&0xffffffff
   records.append(dict(row,stored_CRC32=format(item.CRC,'08x'),compressed_bytes=item.compress_size))
  assert archive.testzip() is None
 return records

def index_valid(doc,expected):
 paths=[r['path'] for r in doc['files']];assert len(paths)==len(set(paths))==len({p.casefold() for p in paths}) and set(paths)==set(expected)
 parts={p['name'] for p in doc['parts']};assert len(parts)==len(doc['parts']) and all(0<p['bytes']<LIMIT for p in doc['parts'])
 for row in doc['files']:
  safe_path(row['path']);assert row['part'] in parts and row['logical_root_prefix']=='' and (row['bytes'],row['sha256'])==(expected[row['path']]['bytes'],expected[row['path']]['sha256'])
 for part in doc['parts']:assert part['files']==sum(r['part']==part['name'] for r in doc['files'])

def export():
 entries=json.loads((ROOT/MANIFEST).read_bytes())['entries'];rows=[pin(ROOT/r['path']) for r in entries];expected={r['path']:r for r in rows}
 folder=PRIVATE/'review-parts-v1';folder.mkdir(exist_ok=False)
 prior=json.loads((W2/'continuation-v16-review-parts-index-v2.json').read_bytes());compressed={(r['path'],r['bytes'],r['sha256']):r['compressed_bytes'] for r in prior['files']}
 groups={'ACTIVE':[],'REFERENCE':[]}
 for row in rows:
  key=row['path'],row['bytes'],row['sha256'];size=compressed.get(key)
  if size is None:
   compressor=zlib.compressobj(6,zlib.DEFLATED,-15);data=(ROOT/row['path']).read_bytes();size=len(compressor.compress(data)+compressor.flush())
  assert size<TARGET,'Single compressed member exceeds permitted part target'
  group='ACTIVE' if row['path'].startswith(PREFIX) or not row['path'].startswith('evidence/') or row['path'].startswith('evidence/04A38/completion/w2/continuation-v16-') else 'REFERENCE'
  groups[group].append(dict(row,estimated=size))
 parts=[];files=[]
 for group,items in groups.items():
  batches=[];batch=[];size=0
  for row in sorted(items,key=lambda r:r['path']):
   if batch and size+row['estimated']>TARGET:batches.append(batch);batch=[];size=0
   batch.append(row);size+=row['estimated']
  if batch:batches.append(batch)
  for number,batch in enumerate(batches,1):
   path=folder/('BESHstatNG_04A38_W2_v16_'+group+'_'+str(number).zfill(2)+'.zip');payload=[{k:r[k] for k in ['path','bytes','sha256']} for r in batch]
   make_zip(path,payload);records=verify_zip(path,payload);assert path.stat().st_size<LIMIT
   repro=folder/(path.name+'.determinism.zip');make_zip(repro,payload);assert pin(path)['sha256']==pin(repro)['sha256'] and path.stat().st_size==repro.stat().st_size
   parts.append(dict(name=path.name,group=group,**pin(path),files=len(payload),deterministic_reproduction=pin(repro),CRC='ALL_MEMBERS_VERIFIED'))
   files.extend(dict(r,part=path.name,group=group,logical_root_prefix='') for r in records)
   print(path.name,path.stat().st_size,pin(path)['sha256'],flush=True)
 doc=dict(status='COMPLEMENTARY_LOGICAL_PHYSICAL_HASH_CRC_PASS_NOT_READY',manifest=pin(ROOT/MANIFEST),parts=parts,files=sorted(files,key=lambda r:r['path']),exclusions=pin(HERE/'review-exclusions-v1.json'),privacy_checkpoint=pin(W2/'continuation-v16-publication-privacy-receipt-v1.json'),standalone='NOT_CREATED',original_queries=0)
 index_valid(doc,expected);controls=[]
 for label,mutate in [('duplicate payload',lambda d:d['files'].append(d['files'][0])),('missing payload',lambda d:d['files'].pop()),('changed SHA',lambda d:d['files'][0].update(sha256='0'*64)),('wrong root prefix',lambda d:d['files'][0].update(logical_root_prefix='wrong/')),('unsafe parent path',lambda d:d['files'][0].update(path='../wrong'))]:
  bad=copy.deepcopy(doc);mutate(bad)
  try:index_valid(bad,expected)
  except (AssertionError,ValueError):controls.append(dict(name=label,status='REJECTED'))
  else:raise AssertionError('Invalid logical union accepted '+label)
 doc['controls']=controls;index=external('continuation-v16-review-parts-index-v1.json',doc)
 external('continuation-v16-publication-receipt-v1.json',dict(status='REVIEW_LOGICAL_PHYSICAL_PRIVACY_VERIFIED_NOT_READY',manifest=doc['manifest'],parts_index=index,parts=parts,files=len(files),privacy_checkpoint=doc['privacy_checkpoint'],standalone='NOT_CREATED',reviewer_upload='NOT_UPLOADED_BY_AGENT;LOCAL_ACTUAL_BYTES_HASHED',original_queries=0,scope='Independent complementary ZIPs, each below180MiB. Not binary .zip.001 volumes. Exact union/path/size/SHA/CRC and second physical serialization verified locally; actual uploaded bytes are required for independent external review.'))

if __name__=='__main__':{'checkpoint':checkpoint,'inventory':inventory,'export':export}[sys.argv[1]]()
