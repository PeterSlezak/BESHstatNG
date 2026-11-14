"""Complete explicit inventory and physical ZIP verifier for partial C5. Never READY."""
import json
import stat
import zipfile
from pathlib import Path
from publication_policy import ROOT, HERE, PREFIX, need, sha, safe_path, privacy, forbidden
from verify_ownership_v2 import inspect as ownership
from verify_preservation_v2 import inspect as preservation


def inspect(root=ROOT, manifest_path=None, overrides=None, semantic=True):
    root=Path(root);overrides=overrides or {};manifest_path=manifest_path or PREFIX+'publication-manifest-v1.json'
    def raw(path):return overrides[path] if path in overrides else (root/path).read_bytes()
    def pairs(items):
        result={}
        for key,value in items:
            need(key not in result,'Duplicate JSON key');result[key]=value
        return result
    load=lambda path:json.loads(raw(path),object_pairs_hook=pairs)
    manifest=load(manifest_path);need(manifest['status']=='NOT_READY_04A38','False READY')
    rows=manifest['entries'];paths=[r['path'] for r in rows]
    need(paths==sorted(paths) and len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Duplicate/casefold/unsorted paths')
    need(paths.count(manifest_path)==1,'Self missing')
    nulls=0;findings=[]
    for row in rows:
        path=row['path'];safe_path(path);need((root/path).resolve().is_relative_to(root.resolve()),'Escaping payload')
        if row['bytes'] is None or row['sha256'] is None:
            nulls+=1;need(path==manifest_path and row['bytes'] is None and row['sha256'] is None,'Illegal recursive/null self')
        else:
            data=raw(path);need((len(data),sha(data))==(row['bytes'],row['sha256']),'Payload hash mismatch '+path)
        findings+=privacy(path,raw(path))
    need(nulls==1,'Exactly one nonrecursive null self')
    need(load(PREFIX+manifest['inventory'])['paths']==paths,'Inventory mismatch')
    ready=load(PREFIX+'readiness-v2.json')
    need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED','Unsupported W3/READY')
    need(ready['new_original_queries']==0 and ready['owner_decision_needed'] is False,'Original authority exceeded')
    need(ready['validation']==load(PREFIX+'validation-v2.json'),'Stale validation')
    result=dict(status='C5_PARTIAL_NOT_READY_MANIFEST_HASH_PRIVACY_PASS',hashed_payloads=len(rows)-1,null_self_entries=1,
                privacy_findings=findings,omitted='Private raw diagnostics/caches/DLLs, omitted owner documents and ignored historical TestResults cannot be independently attested by this copy')
    if semantic:
        result['ownership']=ownership(root,overrides)
        result['preservation']=preservation(root,False)
        for key,value in [('historical_OPEN',result['ownership']['historical']['OPEN']),('current_OPEN',result['ownership']['current']['OPEN']),('distinct_support_OPEN',result['ownership']['support']['distinct_OPEN'])]:
            need(ready['mandatory_blockers'][key]==value and value>0,'Invented closure')
    return result


def inspect_zip(path,manifest_path):
    with zipfile.ZipFile(path) as archive:
        info=archive.infolist();names=[i.filename for i in info]
        need(len(names)==len(set(names))==len({p.casefold() for p in names}),'Physical duplicate/casefold path')
        for item in info:
            safe_path(item.filename)
            need(not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16),'Directory/encrypted/symlink payload')
        need(manifest_path in names,'Physical manifest missing')
        manifest=json.loads(archive.read(manifest_path));need(manifest['status']=='NOT_READY_04A38','Physical false READY')
        rows=manifest['entries'];paths=[r['path'] for r in rows]
        need(len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Duplicate manifest paths')
        need(set(names)==set(paths),'Physical extra/missing payload')
        nulls=0;findings=[]
        for row in rows:
            data=archive.read(row['path'])
            if row['bytes'] is None or row['sha256'] is None:
                nulls+=1;need(row['path']==manifest_path and row['bytes'] is None and row['sha256'] is None,'ZIP recursive/null self')
            else:need((len(data),sha(data))==(row['bytes'],row['sha256']),'Physical payload hash mismatch')
            findings+=privacy(row['path'],data)
        need(nulls==1,'ZIP requires one null self')
        need(archive.testzip() is None,'Archive CRC mismatch')
    return dict(status='ALL_PHYSICAL_MEMBERS_HASH_CRC_PRIVACY_PASS',entries=len(names),privacy_findings=findings)


if __name__=='__main__':
    import sys
    try:
        result=inspect_zip(sys.argv[2],sys.argv[1]) if len(sys.argv)>2 else inspect(manifest_path=sys.argv[1] if len(sys.argv)>1 else None)
        print(json.dumps(result))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))))
        raise SystemExit(1)
