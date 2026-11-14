"""One logical review inventory; bounded complementary ZIPs, never a standalone claim."""
from common import *
import copy
import shutil
import stat
import zipfile
import zlib
from publication_privacy_c11_v1 import privacy,safe_path

PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=PREFIX+'publication-manifest-v1.json'
LIMIT=180*1024**2
TARGET=155*1024**2

def selected():
    paths={r['path'] for r in json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())['entries']}
    paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
    paths.update(r['live']['path'] for r in json.loads((HERE/'all-edited-source-preimages-v1.json').read_bytes())['files'])
    for name in ['current-universe-packet-v2.json','portable-universe-v1.json','dependency-full-common-v2.json','dependency-full-core-v2.json']:
        known={p.casefold():p for p in paths}
        for row in json.loads((HERE/name).read_bytes())['sources']:
            if row['path'].casefold() not in known:paths.add(row['path']);known[row['path'].casefold()]=row['path']
    paths.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v10-*') if p.is_file())
    paths.update(p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
    need_giant='evidence/04A38/completion/w2/continuation-v9/python-first-failures-v1.json'
    assert need_giant not in paths
    assert len(paths)==len({p.casefold() for p in paths}),'Logical casefold collision'
    return paths

def prepare():
    """Retain raw entry logs; create explicitly labelled machine-path review derivatives."""
    excluded=[];derivatives=[]
    for path in sorted(selected()):
        data=(ROOT/path).read_bytes()
        try:privacy(path,data)
        except ValueError:
            assert path.startswith(PREFIX) and Path(path).suffix in ['.json','.log'],'No new source/privacy exception is authorized'
            relative=Path(path).relative_to(Path(PREFIX));target=HERE/'review-derivatives'/relative
            target.parent.mkdir(parents=True,exist_ok=True)
            if path.endswith('.json'):
                normalized=(json.dumps(clean(json.loads(data)),ensure_ascii=False,indent=2,allow_nan=False)+'\n').encode()
            else:normalized=clean(data.decode('utf-8-sig')).encode()
            assert normalized!=data
            with target.open('xb') as stream:stream.write(normalized)
            privacy(target.relative_to(ROOT).as_posix(),normalized)
            excluded.append(dict(**pin(ROOT/path),reason='Raw machine-path execution/entry receipt retained unchanged in authoritative repository; review derivative only.'))
            derivatives.append(dict(raw=pin(ROOT/path),derivative=pin(target),transform='Exact existing baseline_report.clean on parsed JSON strings or UTF8 log text; no source/model/reference bytes transformed.'))
    giant=json.loads((W2/'continuation-v10-retained-private-preservation-v2.json').read_bytes())['retained_interrupted_aggregation']
    save('review-exclusions-v1.json',dict(status='REVIEW_DERIVATIVES_ONLY_NO_AUTHORITATIVE_DELETION',
        excluded=[dict(giant,reason='Immutable malformed4213129746-byte v9 interrupted aggregation; inherited authorized review-only exclusion, never altered/omitted from authoritative scanner or preservation.')]+excluded,
        raw_review_derivatives=derivatives,retained_owner_documents='All16 remain protected by live preservation; same prior review exclusions retained.',
        ignored_history='Not traversed or deleted; explicit private pins and prior physical archives checked separately.',original_queries=0))
    print('Explicit raw review exclusions',len(excluded),'new derivatives',len(derivatives),flush=True)

def inventory():
    exclusions=json.loads((HERE/'review-exclusions-v1.json').read_bytes())
    forbidden={r['path'] for r in exclusions['excluded']}
    paths=selected()-forbidden
    paths.update([PREFIX+'file-inventory-v1.json',MANIFEST])
    ordered=sorted(paths)
    assert len(ordered)==len({p.casefold() for p in ordered})
    save('file-inventory-v1.json',dict(status='SELECTED_LOGICAL_C11_REVIEW_INVENTORY_NOT_AUTHORITATIVE_REPO',
        paths=ordered,exclusion_manifest='review-exclusions-v1.json',logical_root='repository root, no prefix',
        standalone='NOT_CREATED; no split-versus-standalone physical-byte equivalence claim',original_queries=0))
    rows=[]
    for path in ordered:
        safe_path(path)
        if path==MANIFEST:rows.append(dict(path=path,bytes=None,sha256=None));continue
        data=(ROOT/path).read_bytes();privacy(path,data);rows.append(pin(ROOT/path))
    save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',entries=rows,
        inventory='file-inventory-v1.json',self_entry='Single nonrecursive null manifest self-entry; actual byte SHA/length appears in the external cross-part index, as for every payload.',
        standalone='NOT_CREATED',original_queries=0))
    print('C11 logical manifest',len(rows),'files sealed',flush=True)

def make_zip(path,rows):
    with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as archive:
        for row in rows:
            data=(ROOT/row['path']).read_bytes()
            assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            info=zipfile.ZipInfo(row['path'],date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED
            info.create_system=3;info.external_attr=0o100644<<16
            archive.writestr(info,data,compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)

def verify_zip(path,rows):
    result=[]
    with zipfile.ZipFile(path) as archive:
        names=archive.namelist();assert names==[r['path'] for r in rows] and len(names)==len(set(names))==len({p.casefold() for p in names})
        for row in rows:
            info=archive.getinfo(row['path']);safe_path(info.filename)
            assert not info.is_dir() and not info.flag_bits&1 and not stat.S_ISLNK(info.external_attr>>16)
            assert info.date_time==(1980,1,1,0,0,0) and info.compress_type==zipfile.ZIP_DEFLATED
            data=archive.read(row['path']);assert (len(data),sha(data))==(row['bytes'],row['sha256'])
            assert zlib.crc32(data)&0xffffffff==info.CRC
            result.append(dict(row,stored_CRC32=format(info.CRC,'08x'),compressed_bytes=info.compress_size))
        assert archive.testzip() is None
    return result

def index_valid(doc,expected):
    keys=[r['path'] for r in doc['files']]
    assert len(keys)==len(set(keys))==len({p.casefold() for p in keys}) and set(keys)==set(expected)
    parts={p['name'] for p in doc['parts']}
    assert len(parts)==len(doc['parts']) and all(0<p['bytes']<LIMIT for p in doc['parts'])
    for r in doc['files']:
        safe_path(r['path']);assert r['part'] in parts and r['logical_root_prefix']==''
        assert (r['bytes'],r['sha256'])==(expected[r['path']]['bytes'],expected[r['path']]['sha256'])
    for p in doc['parts']:
        assert p['files']==sum(r['part']==p['name'] for r in doc['files'])
    return True

def export():
    from verify_c11_publication_v1 import inspect
    live=inspect(semantic=True);print('Complete logical source/CIL/validation/privacy verification PASS_NOT_READY',flush=True)
    entries=json.loads((ROOT/MANIFEST).read_bytes())['entries'];rows=[pin(ROOT/r['path']) for r in entries]
    expected={r['path']:r for r in rows}
    prior=json.loads((W2/'continuation-v10-publication-receipt-v1.json').read_bytes())['physical_zip']['records']
    compression={(r['path'],r['bytes'],r['sha256']):r['compressed_bytes'] for r in prior}
    groups={'ACTIVE':[],'REFERENCE':[]}
    for row in rows:
        key=row['path'],row['bytes'],row['sha256']
        if key in compression:size=compression[key]
        else:
            compressor=zlib.compressobj(6,zlib.DEFLATED,-15)
            data=(ROOT/row['path']).read_bytes();size=len(compressor.compress(data)+compressor.flush())
        assert size<TARGET,'Single compressed payload exceeds bounded part size'
        group='ACTIVE' if row['path'].startswith(PREFIX) or not row['path'].startswith('evidence/') else 'REFERENCE'
        groups[group].append(dict(row,estimated_compressed=size))
    folder=PRIVATE/'review-parts-v1';folder.mkdir(exist_ok=False)
    assert shutil.disk_usage(ROOT).free>2*sum(r['bytes'] for r in rows)+1024**3,'Insufficient space; retain all evidence'
    parts=[];files=[]
    for group,items in groups.items():
        batches=[];batch=[];size=0
        for row in sorted(items,key=lambda r:r['path']):
            if batch and size+row['estimated_compressed']>TARGET:batches.append(batch);batch=[];size=0
            batch.append(row);size+=row['estimated_compressed']
        if batch:batches.append(batch)
        for number,batch in enumerate(batches,1):
            name='BESHstatNG_04A38_W2_v11_'+group+'_'+str(number).zfill(2)+'.zip';path=folder/name
            payload=[{k:r[k] for k in ['path','bytes','sha256']} for r in batch]
            make_zip(path,payload);records=verify_zip(path,payload)
            assert path.stat().st_size<LIMIT
            reproduction=folder/(name+'.determinism.zip');make_zip(reproduction,payload)
            assert pin(path)['sha256']==pin(reproduction)['sha256'] and path.stat().st_size==reproduction.stat().st_size
            parts.append(dict(name=name,group=group,**pin(path),files=len(payload),
                deterministic_reproduction=pin(reproduction),CRC='ALL_MEMBERS_VERIFIED'))
            files.extend(dict(r,part=name,group=group,logical_root_prefix='') for r in records)
            print(name,path.stat().st_size,pin(path)['sha256'],flush=True)
    doc=dict(status='COMPLEMENTARY_LOGICAL_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY',manifest=pin(ROOT/MANIFEST),
        parts=parts,files=sorted(files,key=lambda r:r['path']),logical_root='repository root, empty prefix',
        standalone='NOT_CREATED',exclusions=pin(HERE/'review-exclusions-v1.json'),original_queries=0)
    index_valid(doc,expected);controls=[]
    for label,mutate in [('duplicate path',lambda d:d['files'].append(d['files'][0])),
                         ('missing payload',lambda d:d['files'].pop()),
                         ('changed SHA',lambda d:d['files'][0].update(sha256='0'*64)),
                         ('wrong logical prefix',lambda d:d['files'][0].update(logical_root_prefix='wrong/'))]:
        altered=copy.deepcopy(doc);mutate(altered)
        try:index_valid(altered,expected)
        except (AssertionError,ValueError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Invalid logical inventory accepted '+label)
    doc['controls']=controls;target=W2/'continuation-v11-review-parts-index-v1.json'
    with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    receipt=dict(status='PARTIAL_W2_NOT_READY_SPLIT_PUBLICATION_VERIFIED',manifest=pin(ROOT/MANIFEST),
        parts_index=pin(target),parts=parts,files=len(files),live=live,standalone='NOT_CREATED',
        logical_union='Exactly one copy per payload, no missing/duplicate/casefold-colliding/unsafe paths; all file SHA/length/CRC explicit in shared index, including manifest self bytes.',
        physical='Every part and its complete independent deterministic second serialization match exact physical SHA/length. No standalone physical-byte reproduction claim.',original_queries=0)
    target=W2/'continuation-v11-publication-receipt-v1.json'
    with target.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
    print('C11 split logical publication receipt',pin(target),flush=True)

if __name__=='__main__':
    {'prepare':prepare,'inventory':inventory,'export':export}[sys.argv[1]]()
