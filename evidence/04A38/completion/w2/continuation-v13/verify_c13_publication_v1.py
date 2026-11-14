"""Hash every review member, authenticate once-per-checkpoint privacy decisions, replay proof."""
import hashlib,json,stat,sys,zipfile,zlib
from pathlib import Path
from publication_privacy_c13_v1 import need,sha,safe_path,privacy
from publication_privacy_c13_v1 import authority as privacy_authority
from verify_c13_checkpoint_v2 import compute
from verify_c13_preservation_v1 import inspect as preservation
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=PREFIX+'publication-manifest-v1.json'
def inspect(root=ROOT,semantic=True):
    root=Path(root);raw=lambda p:(root/p).read_bytes();doc=json.loads(raw(MANIFEST));rows=doc['entries'];paths=[r['path'] for r in rows]
    need((doc['status'],doc['W2'],doc['W3'],doc['standalone'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NOT_CREATED'),'Unsupported publication stage')
    need(paths==sorted(paths) and len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Ambiguous logical paths')
    policy=doc['privacy_policy'];need(policy==privacy_authority['base_policy'] and sha(raw(policy['path']))==policy['sha256'],'Privacy policy changed')
    self_entries=[]
    for row in rows:
        name=row['path'];safe_path(name);need((root/name).resolve().is_relative_to(root.resolve()),'Escaping logical path');data=raw(name)
        if row['sha256'] is None:
            need(row['bytes'] is None,'Malformed manifest self entry');self_entries.append(name);continue
        need((len(data),sha(data))==(row['bytes'],row['sha256']),'Changed review payload '+name)
        decision=row['privacy_decision']
        need(decision['status']=='POLICY_EVALUATED' and decision['policy_sha256']==policy['sha256'] and decision['content_sha256']==row['sha256'],'Missing/altered checkpoint privacy decision '+name)
    need(self_entries==[MANIFEST],'Wrong manifest self identity')
    # Only the small structured self manifest is evaluated here; the expensive
    # complete selected-payload policy campaign already ran once at checkpoint.
    self_findings=privacy(MANIFEST,raw(MANIFEST))
    excluded=json.loads(raw(PREFIX+'review-exclusions-v1.json'))
    need(not {r['path'] for r in excluded['excluded']}&set(paths),'Excluded protected/raw evidence injected')
    giant=next(r for r in excluded['excluded'] if r['bytes']==4213129746)
    need(giant['sha256']=='3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb','Protected giant provenance altered')
    result=dict(status='C13_COMPLETE_LOGICAL_HASH_AND_CHECKPOINT_PRIVACY_AUTHORITY_PASS_NOT_READY',files=len(rows),self_findings=self_findings,
        scope='Every payload rehashed; unchanged policy/content decisions authenticated. This does not repair historical whole-scanner errors or verify omitted live/private files.')
    if semantic:result.update(ownership_source_CIL_validation=compute(root),preservation=preservation(root,False))
    return result

def inspect_part(path,index):
    path=Path(path);doc=index if isinstance(index,dict) else json.loads(Path(index).read_bytes())
    parts=[p for p in doc['parts'] if p['name']==path.name];need(len(parts)==1,'Missing/ambiguous part')
    expected=parts[0];rows=[r for r in doc['files'] if r['part']==path.name];names=[r['path'] for r in rows]
    need(len(names)==len(set(names))==len({n.casefold() for n in names}),'Index collision')
    need(0<path.stat().st_size<180*1024**2,'Physical part exceeds180MiB')
    with zipfile.ZipFile(path) as archive:
        infos=archive.infolist();actual=[i.filename for i in infos]
        need(actual==sorted(actual) and len(actual)==len(set(actual))==len({n.casefold() for n in actual}),'Physical duplicate/casefold/unsorted paths')
        for item in infos:
            safe_path(item.filename)
            need(not item.is_dir() and not item.flag_bits&1 and not stat.S_ISLNK(item.external_attr>>16),'Unsafe physical member')
            need(item.date_time==(1980,1,1,0,0,0) and item.compress_type==zipfile.ZIP_DEFLATED,'Unexpected physical serialization')
        need(set(actual)==set(names),'Missing/extra physical member')
        for row in rows:
            need(row['logical_root_prefix']=='','Wrong logical prefix');data=archive.read(row['path']);item=archive.getinfo(row['path'])
            need((len(data),sha(data))==(row['bytes'],row['sha256']),'Changed member SHA/size')
            need(item.CRC==(zlib.crc32(data)&0xffffffff)==int(row['stored_CRC32'],16),'Changed member CRC')
        need(archive.testzip() is None,'Physical CRC failure')
    with path.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
    need((path.stat().st_size,digest)==(expected['bytes'],expected['sha256']),'Physical SHA/size mismatch')
    return dict(name=path.name,files=len(rows),sha256=digest,bytes=path.stat().st_size,status='PHYSICAL_MEMBERS_SHA_CRC_SAFE_PATH_PASS_NOT_READY')

def inspect_parts(index):
    doc=json.loads(Path(index).read_bytes());rows=doc['files'];paths=[r['path'] for r in rows]
    need(doc['status']=='COMPLEMENTARY_LOGICAL_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY' and doc['standalone']=='NOT_CREATED','Unsupported index stage')
    need(len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Cross-part path collision')
    need(len(doc['parts'])==len({p['name'] for p in doc['parts']}) and {r['part'] for r in rows}=={p['name'] for p in doc['parts']},'Ambiguous part names')
    results=[];manifest=None
    for part in doc['parts']:
        path=ROOT/part['path'];results.append(inspect_part(path,doc))
        if any(r['part']==part['name'] and r['path']==MANIFEST for r in rows):
            with zipfile.ZipFile(path) as archive:manifest=archive.read(MANIFEST)
    need(manifest is not None and (len(manifest),sha(manifest))==(doc['manifest']['bytes'],doc['manifest']['sha256']),'Missing manifest bytes')
    entries=json.loads(manifest)['entries'];expected={r['path']:r for r in entries}
    need(len(entries)==len(expected) and set(expected)==set(paths),'Incomplete logical union')
    for row in rows:
        entry=expected[row['path']]
        if row['path']==MANIFEST:need(entry['bytes'] is entry['sha256'] is None,'Illegal self entry')
        else:need((entry['bytes'],entry['sha256'])==(row['bytes'],row['sha256']),'Logical manifest/index SHA mismatch')
    return dict(status='C13_EXACT_LOGICAL_UNION_PHYSICAL_PARTS_PASS_NOT_READY',files=len(rows),parts=results)
if __name__=='__main__':
    try:print(json.dumps(inspect_parts(sys.argv[2]) if len(sys.argv)>1 and sys.argv[1]=='--parts' else inspect()))
    except Exception as error:print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
