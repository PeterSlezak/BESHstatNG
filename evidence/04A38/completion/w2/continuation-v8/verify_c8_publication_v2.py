"""Complete selected C8 review inventory, hash/CRC/privacy and executable proof."""
import json,hashlib,sys,zipfile,stat,zlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
sys.path.append(str(HERE))
from publication_privacy_v2 import need,sha,safe_path,privacy
from verify_c8_checkpoint_v1 import compute
from verify_c8_preservation_v1 import inspect as preservation

def inspect(root=ROOT,overrides=None,semantic=True):
    root=Path(root);overrides=overrides or {};manifest_path=PREFIX+'publication-manifest-v1.json'
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    load=lambda path:json.loads(raw(path))
    manifest=load(manifest_path);rows=manifest['entries'];paths=[r['path'] for r in rows]
    need(manifest['status']=='NOT_READY_04A38' and manifest['W2']=='IN_PROGRESS' and manifest['W3']=='NOT_STARTED','False publication READY')
    need(paths==sorted(paths) and len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Duplicate/casefold/unsorted publication identity')
    nulls=[];findings=[]
    for row in rows:
        path=row['path'];safe_path(path);need((root/path).resolve().is_relative_to(root.resolve()),'Escaping review payload')
        data=raw(path)
        if row['bytes'] is None or row['sha256'] is None:
            nulls.append(path);need(path==manifest_path and row['bytes'] is row['sha256'] is None,'Illegal recursive/null self-entry')
        else:need((len(data),sha(data))==(row['bytes'],row['sha256']),'Payload hash mismatch '+path)
        findings+=privacy(path,data)
    need(nulls==[manifest_path],'Exactly one nonrecursive null self-entry required')
    need(load(PREFIX+'file-inventory-v1.json')['paths']==paths,'Review inventory mismatch')
    result=dict(status='C8_PARTIAL_MANIFEST_HASH_PRIVACY_PASS_NOT_READY',hashed_payloads=len(rows)-1,null_self_entries=1,privacy_findings=findings,scope='Selected review payloads only. Private raw diagnostics/DLLs, omitted owner documents, live Git and unlisted ignored retained history require the separate live preservation receipt.')
    if semantic:
        result['ownership_source_CIL_validation']=compute(root,overrides)
        result['preservation']=preservation(root,False)
        readiness=load(PREFIX+'readiness-v1.json');proof=result['ownership_source_CIL_validation']
        need(readiness['status']=='NOT_READY_04A38' and readiness['W3']=='NOT_STARTED','False readiness promotion')
        need(readiness['mandatory_blockers']==dict(historical_OPEN=proof['historical']['OPEN'],current_OPEN=proof['current']['OPEN'],distinct_support_OPEN=proof['support']['distinct_OPEN']),'Stale OPEN blockers')
    return result

def inspect_zip(path,manifest_path=PREFIX+'publication-manifest-v1.json'):
    records=[];findings=[]
    with zipfile.ZipFile(path) as archive:
        info=archive.infolist();names=[item.filename for item in info]
        need(len(names)==len(set(names))==len({p.casefold() for p in names}),'Duplicate physical ZIP path')
        need(names==sorted(names),'Unsorted physical ZIP')
        for item in info:
            safe_path(item.filename)
            need(not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16),'Directory/encrypted/symlink payload')
            need(item.date_time==(1980,1,1,0,0,0) and item.compress_type==zipfile.ZIP_DEFLATED,'Non-deterministic ZIP member policy')
        manifest=json.loads(archive.read(manifest_path));rows=manifest['entries'];need(set(names)=={r['path'] for r in rows},'Physical missing/extra member')
        nulls=[]
        for row in rows:
            data=archive.read(row['path']);item=archive.getinfo(row['path'])
            if row['sha256'] is None or row['bytes'] is None:
                nulls.append(row['path']);need(row['path']==manifest_path and row['bytes'] is row['sha256'] is None,'ZIP recursive self-entry')
            else:need((len(data),sha(data))==(row['bytes'],row['sha256']),'ZIP payload SHA mismatch')
            crc=zlib.crc32(data)&0xffffffff;need(crc==item.CRC,'Stored ZIP CRC mismatch')
            findings+=privacy(row['path'],data)
            records.append(dict(path=row['path'],bytes=len(data),sha256=sha(data),stored_CRC32=format(item.CRC,'08x'),compressed_bytes=item.compress_size))
        need(nulls==[manifest_path] and archive.testzip() is None,'ZIP self-entry/CRC failure')
    return dict(status='ALL_PHYSICAL_MEMBERS_SHA_STORED_CRC_PRIVACY_PASS',entries=len(records),privacy_findings=findings,records=records)

if __name__=='__main__':
    try:print(json.dumps(inspect_zip(sys.argv[1]) if len(sys.argv)>1 else inspect()))
    except Exception as error:print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
