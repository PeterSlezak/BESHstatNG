"""One bounded publication-policy checkpoint, then ten complementary review ZIPs."""
from common import *
import zipfile,zlib,copy,stat
from verify_transport_v1 import safe_path,validate_manifest,META
sys.path.append(str(W2/'continuation-v6'));import publication_policy as policy
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=HERE/'transport-manifest-v1.json';INDEX=W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json'
def external(name,d):
 p=W2/name
 with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(d,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(p)
def selected():
 prior=load(W2/'continuation-v17-review-parts-index-v1.json');paths={r['path'] for r in prior['files']}
 paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
 paths.update(r['path'] for r in load(PREVIOUS/'production-test-freeze-v8.json')['sources'])
 paths.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v17-*') if p.is_file())
 return paths
load=lambda p:json.loads(Path(p).read_bytes())
def checkpoint():
 base=pin(W2/'continuation-v6/publication_policy.py');old=load(W2/'continuation-v17-review-parts-index-v1.json')
 cache={}
 for r in old['files']:
  if r['sha256'] is None:continue
  d=r['privacy_decision'];assert d['status']=='POLICY_EVALUATED' and d['policy_sha256']==base['sha256'] and d['content_sha256']==r['sha256'];cache[r['path'],r['bytes'],r['sha256']]=d['findings']
 prior_ex=load(PREVIOUS/'review-exclusions-v1.json');excluded={r['path'] for r in prior_ex['excluded']};rows=[];derivatives=[];stats=dict(prior_exact=0,fresh=0)
 assert 'evidence/04A38/completion/w2/continuation-v9/python-first-failures-v1.json' in excluded
 for name in sorted(selected()-excluded):
  safe_path(name);data=(ROOT/name).read_bytes();r=pin(ROOT/name);identity=(name,len(data),sha(data))
  if identity in cache:findings=cache[identity];stats['prior_exact']+=1
  else:
   stats['fresh']+=1
   try:findings=policy.privacy(name,data)
   except ValueError as e:
    assert (name.startswith(PREFIX) or name.startswith('evidence/04A38/completion/w2/continuation-v17-')) and 'Unattested privacy finding' in str(e),'Unexpected out-of-scope publication finding '+name
    try:obj=json.loads(data);cleaned=(json.dumps(clean(obj),indent=2,ensure_ascii=False)+'\n').encode();suffix='.review.json'
    except (ValueError,UnicodeError):cleaned=clean(data.decode('utf-8-sig')).encode();suffix='.review.txt'
    path=HERE/'review-derivatives'/((name[len(PREFIX):] if name.startswith(PREFIX) else 'inherited-postseal/'+Path(name).name)+suffix)
    try:policy.privacy(path.relative_to(ROOT).as_posix(),cleaned)
    except ValueError:
     path=HERE/'review-derivatives'/((name[len(PREFIX):] if name.startswith(PREFIX) else 'inherited-postseal/'+Path(name).name)+'.omitted-values.review.json');cleaned=(json.dumps(dict(raw=r,review_only='Synthetic adverse/machine values omitted only in this separately named derivative; raw authority and negative evidence remain locally preserved.'),indent=2)+'\n').encode()
    findings=policy.privacy(path.relative_to(ROOT).as_posix(),cleaned);path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('xb') as f:f.write(cleaned)
    derivative=pin(path);derivatives.append(dict(raw=r,review_derivative=derivative));excluded.add(name);rows.append(dict(derivative,findings=findings));continue
  rows.append(dict(r,findings=findings))
 save('publication-privacy-decisions-v1.json',dict(status='ONE_BOUNDED_REVIEW_POLICY_CHECKPOINT_WITH_EXACT_PRIOR_DECISION_REUSE',decisions=rows,privacy_policy=base,prior_manifest=pin(W2/'continuation-v17-review-parts-index-v1.json'),stats=stats,repository_wide_historical_scanner_repeats=0,scope='Only changed/new review bytes receive unchanged policy evaluation. Ordinary Python privacy errors remain ERROR; this review transport scope does not waive them.',original_queries=0))
 save('review-exclusions-v1.json',dict(excluded=prior_ex['excluded']+[r['raw'] for r in derivatives],raw_review_derivatives=prior_ex['raw_review_derivatives']+derivatives,private_raw='NOT_INCLUDED',giant_aggregation='PIN_ONLY;NOT_PARSED_OR_SERIALIZED',prior_physical_eight_ZIPs='NOT_UPLOADED;ACTUAL_LOCAL_SHA_PRESERVED',original_queries=0))
 print('Single bounded review policy checkpoint',stats,'separate raw exclusions',len(derivatives),flush=True)
def inventory():
 decisions=load(HERE/'publication-privacy-decisions-v1.json');cache={(r['path'],r['bytes'],r['sha256']):r['findings'] for r in decisions['decisions']};excluded={r['path'] for r in load(HERE/'review-exclusions-v1.json')['excluded']}
 paths=selected()-excluded;paths.discard(MANIFEST.relative_to(ROOT).as_posix());rows=[];base=decisions['privacy_policy'];oldfiles={(r['path'],r['bytes'],r['sha256']):r for r in load(W2/'continuation-v17-review-parts-index-v1.json')['files']}
 for name in sorted(paths):
  safe_path(name);data=(ROOT/name).read_bytes();r=pin(ROOT/name);identity=(name,r['bytes'],r['sha256'])
  findings=cache[identity] if identity in cache else policy.privacy(name,data)
  prior=oldfiles.get(identity);size=prior['compressed_bytes'] if prior else None
  if size is None:
   compressor=zlib.compressobj(6,zlib.DEFLATED,-15);size=len(compressor.compress(data)+compressor.flush())
  crc=format(zlib.crc32(data)&0xffffffff,'08x');role='ACTIVE' if name.startswith(PREFIX) or not name.startswith('evidence/') else 'REFERENCE'
  rows.append(dict(r,crc32=crc,estimated_compressed_bytes=size,role=role,privacy_decision=dict(status='POLICY_EVALUATED',policy_sha256=base['sha256'],content_sha256=r['sha256'],findings=findings)))
 assert len(rows)==len({r['path'].casefold() for r in rows})
 batches=[[] for _ in range(10)];totals=[0]*10
 for r in sorted(rows,key=lambda r:(-r['estimated_compressed_bytes'],r['path'])):
  choices=[0,1] if r['role']=='ACTIVE' else list(range(2,10));i=min(choices,key=lambda i:totals[i]);batches[i].append(r);totals[i]+=r['estimated_compressed_bytes'];r['part']='BESHstatNG_04A38_W2_v17_postcorrection_v1_'+('ACTIVE_'+str(i+1).zfill(2) if i<2 else 'REFERENCE_'+str(i-1).zfill(2))+'.zip'
 for i,total in enumerate(totals):assert total<179*1024**2,('Complement compressed budget',i,total)
 doc=dict(scope='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT',files=sorted(rows,key=lambda r:r['path']),original_bytes=sum(r['bytes'] for r in rows),private_raw_included=False,prior_physical_eight_ZIPs='NOT_UPLOADED',manifest_self='Identical metadata member in each part; excluded from unique logical payload union to avoid recursive hashing.',original_queries=0,privacy_policy=base,source_seal=pin(HERE/'review-seal-v1.json'),exclusions=pin(HERE/'review-exclusions-v1.json'))
 validate_manifest(doc);save(MANIFEST.name,doc);print('Transport inventory',len(rows),'payloads; estimated MiB',[round(v/2**20,1) for v in totals],flush=True)
def export():
 doc=load(MANIFEST);validate_manifest(doc);metadata=MANIFEST.read_bytes();folder=PRIVATE/'review-parts-v1';folder.mkdir(exist_ok=False);parts=[]
 for name in sorted({r['part'] for r in doc['files']}):
  rows=[r for r in doc['files'] if r['part']==name];path=folder/name
  with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as archive:
   for logical,data in [(META,metadata)]+[(r['path'],None) for r in rows]:
    info=zipfile.ZipInfo(logical,(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
    if data is None:
     row=next(r for r in rows if r['path']==logical);data=(ROOT/logical).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
    archive.writestr(info,data,compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)
  assert path.stat().st_size<180*1024**2;parts.append(dict(name=name,**pin(path),files=len(rows)));print(name,path.stat().st_size,flush=True)
 index=dict(status='REVIEW_ONLY_LOCAL_PHYSICAL_PARTS_CREATED;VERIFICATION_RECEIPT_SEPARATE',parts=parts,files=doc['files'],embedded_manifest_sha256=sha(metadata),manifest=pin(MANIFEST),private_raw_included=False,prior_physical_eight_ZIPs='NOT_UPLOADED',uploaded_by_agent=False,binary_volumes=False,original_queries=0)
 external('continuation-v17-postcorrection-v1-review-parts-index-v1.json',index)
 external('continuation-v17-postcorrection-v1-publication-receipt-v1.json',dict(parts_index=pin(INDEX),parts=parts,files=len(doc['files']),original_bytes=doc['original_bytes'],manifest=pin(MANIFEST),authorized_exclusions=pin(HERE/'review-exclusions-v1.json'),actual_external_upload='NOT_PERFORMED;PHYSICAL_EXTERNAL_VERIFICATION_REQUIRES_UPLOADED_BYTES',scope='Ten independent complementary ZIPs; two ACTIVE and eight REFERENCE. Common embedded manifest is identical; payload union is unique. No .001/.002 binary volumes.',original_queries=0))
if __name__=='__main__':{'checkpoint':checkpoint,'inventory':inventory,'export':export}[sys.argv[1]]()
