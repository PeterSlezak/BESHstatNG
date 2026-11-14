"""Complete selected logical publication and independent physical part reader; never READY."""
import hashlib
import json
from pathlib import Path
import stat
import sys
import zipfile
import zlib
from publication_privacy_c11_v1 import need,sha,privacy,safe_path
from verify_c11_checkpoint_v2 import compute
from verify_c11_preservation_v1 import inspect as preservation

ROOT=Path(__file__).resolve().parents[5]
HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=PREFIX+'publication-manifest-v1.json'

def inspect(root=ROOT,overrides=None,semantic=True):
    root=Path(root);overrides=overrides or {}
    raw=lambda p:overrides[p] if p in overrides else (root/p).read_bytes()
    load=lambda p:json.loads(raw(p))
    doc=load(MANIFEST);rows=doc['entries'];paths=[r['path'] for r in rows]
    need((doc['status'],doc['W2'],doc['W3'],doc['standalone'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NOT_CREATED'),'Unsupported publication stage/readiness')
    need(paths==sorted(paths) and len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Ambiguous/duplicate/casefold publication identity')
    findings=[];nulls=[]
    for row in rows:
        path=row['path'];safe_path(path);need((root/path).resolve().is_relative_to(root.resolve()),'Escaping payload path')
        data=raw(path)
        if row['bytes'] is None or row['sha256'] is None:
            nulls.append(path);need(path==MANIFEST and row['bytes'] is row['sha256'] is None,'Illegal null/recursive self-entry')
        else:need((len(data),sha(data))==(row['bytes'],row['sha256']),'Selected payload SHA mismatch '+path)
        findings+=privacy(path,data)
    need(nulls==[MANIFEST] and load(PREFIX+'file-inventory-v1.json')['paths']==paths,'Wrong logical manifest/inventory union')
    excluded=load(PREFIX+'review-exclusions-v1.json')
    need(not {r['path'] for r in excluded['excluded']}&set(paths),'Explicit private review exclusion injected')
    giant=next(r for r in excluded['excluded'] if r['path'].endswith('continuation-v9/python-first-failures-v1.json'))
    need((giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb'),'Protected giant pin altered')
    result=dict(status='C11_LOGICAL_MANIFEST_HASH_PRIVACY_PASS_NOT_READY',files=len(rows),hashed_payloads=len(rows)-1,
        manifest_self='Actual byte SHA/length in shared external parts index',privacy_findings=findings,
        scope='Selected review derivatives only. Live Git, all16 owner docs and ignored/private raw evidence require the separate authoritative preservation receipts.')
    if semantic:
        result['ownership_source_CIL_validation']=compute(root,overrides)
        result['preservation']=preservation(root,False)
    return result

def inspect_part(path,index):
    path=Path(path);doc=json.loads(Path(index).read_bytes()) if not isinstance(index,dict) else index
    parts=[p for p in doc['parts'] if p['name']==path.name]
    need(len(parts)==1,'Missing/ambiguous physical part identity')
    expected=parts[0];rows=[r for r in doc['files'] if r['part']==path.name]
    need(len(rows)==len({r['path'] for r in rows})==len({r['path'].casefold() for r in rows}),'Index path collision')
    need(0<path.stat().st_size<180*1024**2,'Part exceeds180MiB bound')
    with zipfile.ZipFile(path) as archive:
        info=archive.infolist();names=[i.filename for i in info]
        need(names==sorted(names) and len(names)==len(set(names))==len({n.casefold() for n in names}),'Physical duplicate/casefold/unsorted ZIP identity')
        for item in info:
            safe_path(item.filename)
            need(not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16),'Directory/encrypted/symlink member')
            need(item.date_time==(1980,1,1,0,0,0) and item.compress_type==zipfile.ZIP_DEFLATED,'Unexpected physical serialization')
        need(set(names)=={r['path'] for r in rows},'Physical missing/extra payload')
        for row in rows:
            need(row['logical_root_prefix']=='','Ambiguous logical root')
            data=archive.read(row['path']);item=archive.getinfo(row['path'])
            need((len(data),sha(data))==(row['bytes'],row['sha256']),'ZIP payload SHA mismatch')
            need(item.CRC==(zlib.crc32(data)&0xffffffff)==int(row['stored_CRC32'],16),'ZIP stored CRC mismatch')
        need(archive.testzip() is None,'ZIP CRC failure')
    with path.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
    need((path.stat().st_size,digest)==(expected['bytes'],expected['sha256']),'Physical part SHA/length mismatch')
    return dict(name=path.name,files=len(rows),sha256=digest,bytes=path.stat().st_size,status='ALL_PART_MEMBERS_SHA_CRC_SAFE_PATH_PASS_NOT_READY')

def inspect_parts(index,folder=None):
    index=Path(index);doc=json.loads(index.read_bytes());rows=doc['files'];paths=[r['path'] for r in rows]
    need(doc['status']=='COMPLEMENTARY_LOGICAL_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY' and doc['standalone']=='NOT_CREATED','Unsupported review-index readiness/stage')
    need(len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Logical cross-part collision')
    need({r['part'] for r in rows}=={p['name'] for p in doc['parts']},'Part identity missing/extra')
    folder=Path(folder) if folder else ROOT
    results=[];manifest_bytes=None
    for part in doc['parts']:
        path=folder/part['name'] if folder else ROOT/part['path']
        if not path.is_file():path=ROOT/part['path']
        results.append(inspect_part(path,doc))
        if any(r['part']==part['name'] and r['path']==MANIFEST for r in rows):
            with zipfile.ZipFile(path) as archive:manifest_bytes=archive.read(MANIFEST)
    need(manifest_bytes is not None and (len(manifest_bytes),sha(manifest_bytes))==(doc['manifest']['bytes'],doc['manifest']['sha256']),'Shared logical manifest byte authority missing')
    manifest=json.loads(manifest_bytes);expected={r['path']:r for r in manifest['entries']}
    need(set(expected)==set(paths) and len(expected)==len(manifest['entries']),'Cross-part union differs from complete logical manifest')
    for row in rows:
        entry=expected[row['path']]
        if row['path']==MANIFEST:need(entry['bytes'] is entry['sha256'] is None,'Illegal self-entry')
        else:need((row['bytes'],row['sha256'])==(entry['bytes'],entry['sha256']),'Cross-part file hash differs from logical manifest')
    return dict(status='EXACT_LOGICAL_UNION_PHYSICAL_PARTS_PASS_NOT_READY',files=len(rows),parts=results,standalone='NOT_CREATED')

if __name__=='__main__':
    try:
        if len(sys.argv)>1 and sys.argv[1]=='--parts':result=inspect_parts(sys.argv[2],sys.argv[3] if len(sys.argv)>3 else None)
        elif len(sys.argv)>2:result=inspect_part(sys.argv[1],sys.argv[2])
        else:result=inspect()
        print(json.dumps(result))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))))
        raise SystemExit(1)
