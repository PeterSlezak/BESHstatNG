"""Additive complete logical union reader; replay unchanged sealed C11 semantics."""
import sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
W2=Path(__file__).resolve().parent
HERE=W2/'continuation-v11'
sys.path.insert(0,str(HERE))
from common import sha,pin
import json,zipfile
from publication_privacy_c11_v1 import need,safe_path,privacy
from verify_c11_publication_v2 import inspect as sealed_inspect,inspect_part
MANIFEST=(W2/'continuation-v11-logical-publication-manifest-v2.json').relative_to(ROOT).as_posix()

def inspect(root=ROOT,semantic=True):
    root=Path(root);doc=json.loads((root/MANIFEST).read_bytes());rows=doc['entries']
    need(doc['status']=='NOT_READY_04A38' and doc['original_queries']==0,'Unsupported stage')
    paths=[r['path'] for r in rows]
    need(paths==sorted(paths) and len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Ambiguous logical union')
    nulls=[];findings=[]
    for row in rows:
        name=row['path'];safe_path(name);need((root/name).resolve().is_relative_to(root.resolve()),'Escaping payload')
        data=(root/name).read_bytes()
        if row['sha256'] is None:
            need(row['bytes'] is None,'Malformed self-entry');nulls.append(name)
        else:need((len(data),sha(data))==(row['bytes'],row['sha256']),'Changed logical payload '+name)
        # Existing7563 payload privacy remains the unchanged sealed scanner below.
        if name in doc['supplement_paths']:findings+=privacy(name,data)
    need(nulls==[MANIFEST],'Wrong manifest self-entry')
    old=json.loads((root/doc['sealed_manifest']['path']).read_bytes())
    need(set(paths)=={r['path'] for r in old['entries']}|set(doc['supplement_paths'])|{MANIFEST},'Supplement lost/duplicated baseline payload')
    for row in doc['required_context']:
        need(row in rows,'Required entry context missing')
    result=sealed_inspect(root,semantic=semantic)
    return dict(status='C11_ADDITIVE_COMPLETE_LOGICAL_REVIEW_PASS_NOT_READY',files=len(rows),
        original_sealed_result=result,supplement_privacy_findings=findings,
        scope='Immutable five-part baseline plus additive missing entry context and executable union authority; no historical/protected/source/assertion changed.',original_queries=0)

def inspect_parts(index):
    index=Path(index);doc=json.loads(index.read_bytes());rows=doc['files'];paths=[r['path'] for r in rows]
    need(doc['status']=='COMPLEMENTARY_LOGICAL_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY' and doc['standalone']=='NOT_CREATED','Unsupported index stage')
    need(len(paths)==len(set(paths))==len({p.casefold() for p in paths}),'Cross-part collision')
    need(len(doc['parts'])==len({p['name'] for p in doc['parts']}) and {r['part'] for r in rows}=={p['name'] for p in doc['parts']},'Ambiguous parts')
    results=[];raw_manifest=None
    for part in doc['parts']:
        path=ROOT/part['path'];results.append(inspect_part(path,doc))
        if any(r['part']==part['name'] and r['path']==MANIFEST for r in rows):
            with zipfile.ZipFile(path) as archive:raw_manifest=archive.read(MANIFEST)
    need(raw_manifest is not None and (len(raw_manifest),sha(raw_manifest))==(doc['manifest']['bytes'],doc['manifest']['sha256']),'Missing logical manifest bytes')
    expected=json.loads(raw_manifest)['entries'];lookup={r['path']:r for r in expected}
    need(len(expected)==len(lookup) and set(lookup)==set(paths),'Missing/extra logical payload')
    for row in rows:
        entry=lookup[row['path']];need(row['logical_root_prefix']=='','Unexpected root prefix')
        if row['path']==MANIFEST:need(entry['bytes'] is entry['sha256'] is None,'Invalid self-entry')
        else:need((entry['bytes'],entry['sha256'])==(row['bytes'],row['sha256']),'Logical file SHA mismatch')
    return dict(status='C11_SIX_PART_EXACT_LOGICAL_UNION_PASS_NOT_READY',files=len(rows),parts=results)

if __name__=='__main__':
    try:
        result=inspect_parts(sys.argv[2]) if len(sys.argv)>1 and sys.argv[1]=='--parts' else inspect()
        print(json.dumps(result))
    except Exception as error:
        print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))))
        raise SystemExit(1)
