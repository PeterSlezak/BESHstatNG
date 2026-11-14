"""File-wise complementary ACTIVE/REFERENCE packages with a common external index."""
from common import *
import zipfile,zlib,copy

def validate_index(doc,expected):
    rows=doc['files'];keys=[r['path'] for r in rows]
    assert len(keys)==len(set(keys))==len({k.casefold() for k in keys})
    assert set(keys)==set(expected) and all(r['sha256']==expected[r['path']]['sha256'] and r['bytes']==expected[r['path']]['bytes'] for r in rows)
    assert all(r['part'] in {p['name'] for p in doc['parts']} and r['logical_root_prefix']=='' for r in rows)
    assert all(p['bytes']<=200*1024**2 for p in doc['parts'])
    return True

def main():
    publication=W2/'continuation-v10-publication-receipt-v1.json';receipt=json.loads(publication.read_bytes());archive=ROOT/receipt['archive']['path'];assert pin(archive)=={k:receipt['archive'][k] for k in ['path','bytes','sha256']}
    records=receipt['physical_zip']['records'];expected={r['path']:r for r in records};folder=PRIVATE/'review-parts-v1';folder.mkdir(exist_ok=False)
    changed=set(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths']);prefix=HERE.relative_to(ROOT).as_posix()+'/'
    groups={'ACTIVE':[],'REFERENCE':[]}
    for r in records:groups['ACTIVE' if r['path'].startswith(prefix) or not r['path'].startswith('evidence/') or r['path'] in changed else 'REFERENCE'].append(r)
    parts=[];files=[]
    with zipfile.ZipFile(archive) as original:
        for group,rows in groups.items():
            batches=[];batch=[];size=0
            for r in sorted(rows,key=lambda r:r['path']):
                assert r['compressed_bytes']<180*1024**2,'A single compressed file exceeds the practical file-wise limit'
                if batch and size+r['compressed_bytes']>180*1024**2:batches.append(batch);batch=[];size=0
                batch.append(r);size+=r['compressed_bytes']
            if batch:batches.append(batch)
            for number,batch in enumerate(batches,1):
                name='BESHstatNG_04A38_W2_v10_'+group+'_'+str(number).zfill(2)+'.zip';path=folder/name
                def build(target):
                    with zipfile.ZipFile(target,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as out:
                        for r in batch:
                            data=original.read(r['path']);assert sha(data)==r['sha256'] and len(data)==r['bytes']
                            info=zipfile.ZipInfo(r['path'],date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
                            out.writestr(info,data,compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)
                build(path);repeat=folder/(name+'.determinism.zip');build(repeat);assert pin(path)['sha256']==pin(repeat)['sha256'] and path.stat().st_size==repeat.stat().st_size
                with zipfile.ZipFile(path) as part:
                    assert part.namelist()==[r['path'] for r in batch] and part.testzip() is None
                    for r in batch:
                        data=part.read(r['path']);stored=part.getinfo(r['path']);assert sha(data)==r['sha256'] and stored.CRC==(zlib.crc32(data)&0xffffffff)==int(r['stored_CRC32'],16)
                        files.append(dict(path=r['path'],bytes=len(data),sha256=sha(data),stored_CRC32=r['stored_CRC32'],part=name,group=group,logical_root_prefix=''))
                parts.append(dict(name=name,group=group,**pin(path),deterministic_reproduction=pin(repeat),files=len(batch),CRC='ALL_MEMBERS_VERIFIED'))
    doc=dict(status='COMPLEMENTARY_FILEWISE_ACTIVE_REFERENCE_HASH_CRC_PASS_NOT_READY',standalone_receipt=pin(publication),standalone_manifest=receipt['manifest'],parts=parts,files=sorted(files,key=lambda r:r['path']),logical_root='repository root, no normalization prefix',exclusions=json.loads((HERE/'review-exclusions-v1.json').read_bytes()),scope='Selected standalone payload partition only; no overlap/missing paths. The protected giant failed attempt is independently audited in the authoritative checkout and explicitly excluded only from review derivatives. Live Git/private/owner/ignored preservation requires separate receipts.',original_queries=0)
    validate_index(doc,expected);controls=[]
    for label,edit in [('duplicate path',lambda d:d['files'].append(d['files'][0])),('missing path',lambda d:d['files'].pop()),('changed digest',lambda d:d['files'][0].update(sha256='0'*64)),('wrong logical prefix',lambda d:d['files'][0].update(logical_root_prefix='wrong/'))]:
        altered=copy.deepcopy(doc);edit(altered)
        try:validate_index(altered,expected)
        except AssertionError:controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Review part index mutation accepted '+label)
    for row in doc['exclusions']['excluded']:assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
    doc['controls']=controls;target=W2/'continuation-v10-review-parts-index-v1.json'
    with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(index=pin(target),parts=[{k:r[k] for k in ['name','bytes','sha256']} for r in parts],files=len(files),missing=0,overlap=0)))
if __name__=='__main__':main()
