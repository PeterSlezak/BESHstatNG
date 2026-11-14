"""One logical source/evidence review, bounded valid ZIPs and deterministic copies."""
from common import *
import copy,shutil,stat,zipfile,zlib
from publication_privacy_c14_v2 import safe_path,privacy
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=PREFIX+'publication-manifest-v2.json'
LIMIT=180*1024**2;TARGET=155*1024**2
def external(name,value):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)
def selected():
    prior=json.loads((W2/'continuation-v13-review-parts-index-v1.json').read_bytes())
    names={r['path'] for r in prior['files']}
    names.update(path.relative_to(ROOT).as_posix() for path in HERE.rglob('*') if path.is_file() and '__pycache__' not in path.parts)
    names.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
    names.update(row['live']['path'] for row in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files'])
    names.update(row['path'] for row in json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())['authorities'])
    # Every final/failed/superseded public v12 receipt/tool remains additive authority.
    names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v13-*') if p.is_file() and p.suffix in ['.json','.py','.md'])
    names.add((W2/'continuation-v14-retained-private-reader-v2.py').relative_to(ROOT).as_posix())
    names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v14-*') if p.is_file() and p.suffix in ['.json','.py','.md'])
    names={name for name in names if not name.startswith('.test-rationalization-04A38.tmp/') and not name.startswith('.git/')}
    return names
def prepare():
    previous=json.loads((PREVIOUS/'review-exclusions-v1.json').read_bytes())
    save('review-exclusions-v2.json',dict(status='REVIEW_ONLY_EXCLUSIONS_NO_AUTHORITATIVE_DELETION',excluded=previous['excluded']+[json.loads((HERE/'publication-git-index-binding-v1.json').read_bytes())['entry']],raw_review_derivatives=previous['raw_review_derivatives'],
        source=pin(PREVIOUS/'review-exclusions-v1.json'),private_entry_authorities=[r for r in json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())['authorities'] if r['path'].startswith('.test-rationalization-04A38.tmp/')],owner_documents='All16 remain live protected. Required pinned master plan and AGENTS/context are in the complete prior inventory;15 optional owner README documents remain omitted.',
        ignored_history='Never traversed/deleted; inherited11875 explicit private pins, current pins and all physical archives verified separately.',original_queries=0))
def inventory():
    forbidden={row['path'] for row in json.loads((HERE/'review-exclusions-v2.json').read_bytes())['excluded']}
    names=selected()-forbidden;names.update([PREFIX+'file-inventory-v2.json',MANIFEST]);ordered=sorted(names)
    assert len(ordered)==len({name.casefold() for name in ordered})
    save('file-inventory-v2.json',dict(status='SELECTED_COMPLETE_LOGICAL_C14_REVIEW_NOT_FULL_REPOSITORY',paths=ordered,standalone='NOT_CREATED',logical_root='repository root, empty prefix',original_queries=0))
    policy=pin(W2/'continuation-v6/publication_policy.py');rows=[];findings=[];started=time.monotonic()
    for name in ordered:
        safe_path(name)
        if name==MANIFEST:rows.append(dict(path=name,bytes=None,sha256=None));continue
        data=(ROOT/name).read_bytes();observed=privacy(name,data);record=pin(ROOT/name)
        rows.append(dict(record,privacy_decision=dict(status='POLICY_EVALUATED',policy_sha256=policy['sha256'],content_sha256=record['sha256'],findings=observed)))
        findings.extend(observed)
    save('publication-manifest-v2.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',entries=rows,privacy_policy=policy,
        privacy_authority=pin(HERE/'publication-privacy-authority-v1.json'),privacy_execution='One selected-payload policy checkpoint; exact policy/path/length/SHA may use previously verified physical decisions. Export/copy replay hashes decisions and every payload, avoiding repeated expensive policy campaigns.',
        self_entry='One nonrecursive null self-entry; actual SHA/size/CRC in the external part index. Structured manifest-self separately policy-scanned.',standalone='NOT_CREATED',original_queries=0))
    self_findings=privacy(MANIFEST,(ROOT/MANIFEST).read_bytes())
    receipt=external('continuation-v14-publication-privacy-receipt-v2.json',dict(status='SELECTED_PAYLOAD_PRIVACY_POLICY_CHECKPOINT_PASS_NOT_READY',
        manifest=pin(ROOT/MANIFEST),files=len(rows),policy=policy,findings=findings+self_findings,seconds=time.monotonic()-started,
        scope='Review derivatives only; does not repair or waive the two inherited whole-scanner JSON ERRORs. Protected giant remains untouched locally and explicitly excluded only here.',original_queries=0))
    print('One complete logical privacy/hash inventory sealed',len(rows),receipt,flush=True)
def make_zip(path,rows):
    with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as archive:
        for row in rows:
            data=(ROOT/row['path']).read_bytes();assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            info=zipfile.ZipInfo(row['path'],date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
            archive.writestr(info,data,compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)
def verify_zip(path,rows):
    records=[]
    with zipfile.ZipFile(path) as archive:
        names=archive.namelist();assert names==[r['path'] for r in rows] and len(names)==len(set(names))==len({name.casefold() for name in names})
        for row in rows:
            item=archive.getinfo(row['path']);safe_path(item.filename)
            assert not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16)
            data=archive.read(row['path']);assert (len(data),sha(data))==(row['bytes'],row['sha256']) and item.CRC==zlib.crc32(data)&0xffffffff
            records.append(dict(row,stored_CRC32=format(item.CRC,'08x'),compressed_bytes=item.compress_size))
        assert archive.testzip() is None
    return records
def index_valid(doc,expected):
    paths=[r['path'] for r in doc['files']];assert len(paths)==len(set(paths))==len({p.casefold() for p in paths}) and set(paths)==set(expected)
    parts={p['name'] for p in doc['parts']};assert len(parts)==len(doc['parts']) and all(0<p['bytes']<LIMIT for p in doc['parts'])
    for row in doc['files']:
        safe_path(row['path']);assert row['part'] in parts and row['logical_root_prefix']=='' and (row['bytes'],row['sha256'])==(expected[row['path']]['bytes'],expected[row['path']]['sha256'])
    for part in doc['parts']:assert part['files']==sum(row['part']==part['name'] for row in doc['files'])
def export():
    entries=json.loads((ROOT/MANIFEST).read_bytes())['entries'];rows=[pin(ROOT/r['path']) for r in entries];expected={r['path']:r for r in rows}
    folder=PRIVATE/'review-parts-v2';folder.mkdir(exist_ok=False)
    assert shutil.disk_usage(ROOT).free>2*sum(r['bytes'] for r in rows)+1024**3
    prior=json.loads((W2/'continuation-v14-review-parts-index-v1.json').read_bytes())
    compressed={(r['path'],r['bytes'],r['sha256']):r['compressed_bytes'] for r in prior['files']};groups={'ACTIVE':[],'REFERENCE':[]}
    for row in rows:
        key=row['path'],row['bytes'],row['sha256'];size=compressed.get(key)
        if size is None:
            compressor=zlib.compressobj(6,zlib.DEFLATED,-15);data=(ROOT/row['path']).read_bytes();size=len(compressor.compress(data)+compressor.flush())
        assert size<TARGET,'One compressed payload exceeds target'
        group='ACTIVE' if row['path'].startswith(PREFIX) or not row['path'].startswith('evidence/') or row['path'].startswith('evidence/04A38/completion/w2/continuation-v14-') else 'REFERENCE'
        groups[group].append(dict(row,estimated=size))
    parts=[];files=[]
    for group,items in groups.items():
        batches=[];batch=[];size=0
        for row in sorted(items,key=lambda r:r['path']):
            if batch and size+row['estimated']>TARGET:batches.append(batch);batch=[];size=0
            batch.append(row);size+=row['estimated']
        if batch:batches.append(batch)
        for number,batch in enumerate(batches,1):
            path=folder/('BESHstatNG_04A38_W2_v14_v2_'+group+'_'+str(number).zfill(2)+'.zip');payload=[{k:r[k] for k in ['path','bytes','sha256']} for r in batch]
            make_zip(path,payload);records=verify_zip(path,payload);assert path.stat().st_size<LIMIT
            repro=folder/(path.name+'.determinism.zip');make_zip(repro,payload);assert pin(path)['sha256']==pin(repro)['sha256'] and path.stat().st_size==repro.stat().st_size
            parts.append(dict(name=path.name,group=group,**pin(path),files=len(payload),deterministic_reproduction=pin(repro),CRC='ALL_MEMBERS_VERIFIED'))
            files.extend(dict(row,part=path.name,group=group,logical_root_prefix='') for row in records)
            print(path.name,path.stat().st_size,pin(path)['sha256'],flush=True)
    doc=dict(status='COMPLEMENTARY_LOGICAL_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY',manifest=pin(ROOT/MANIFEST),parts=parts,files=sorted(files,key=lambda r:r['path']),
        exclusions=pin(HERE/'review-exclusions-v2.json'),privacy_checkpoint=pin(W2/'continuation-v14-publication-privacy-receipt-v2.json'),standalone='NOT_CREATED',original_queries=0)
    index_valid(doc,expected);controls=[]
    for label,mutate in [('duplicate payload',lambda d:d['files'].append(d['files'][0])),('missing payload',lambda d:d['files'].pop()),('changed SHA',lambda d:d['files'][0].update(sha256='0'*64)),('stale source index',lambda d:next(r for r in d['files'] if r['path']==json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']).update(sha256=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['before']['sha256'])),('wrong root prefix',lambda d:d['files'][0].update(logical_root_prefix='wrong/'))]:
        bad=copy.deepcopy(doc);mutate(bad)
        try:index_valid(bad,expected)
        except (AssertionError,ValueError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Invalid logical union accepted')
    doc['controls']=controls;index=external('continuation-v14-review-parts-index-v2.json',doc)
    receipt=external('continuation-v14-publication-receipt-v2.json',dict(status='SPLIT_REVIEW_LOGICAL_PHYSICAL_PRIVACY_VERIFIED_NOT_READY',manifest=pin(ROOT/MANIFEST),parts_index=index,parts=parts,files=len(files),
        privacy_checkpoint=doc['privacy_checkpoint'],standalone='NOT_CREATED',reviewer_upload='NOT_UPLOADED_BY_AGENT;WINDOWS_LOCAL_SOURCE_BOUND_RECEIPT_ONLY',original_queries=0,scope='All payload hashes/CRCs, exact complementary union and deterministic physical second serializations verified locally on Windows. No standalone physical-byte equivalence claim.'))
    print('Complete review publication',receipt,flush=True)
if __name__=='__main__':{'prepare':prepare,'inventory':inventory,'export':export}[sys.argv[1]]()
