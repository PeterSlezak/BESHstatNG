"""Standalone combined W3 ACTIVE + unchanged W2 reference ZIP reader.

Run: python -I -B verify-review-v1.py transport-manifest-v1.json parts-index-v1.json
The parts index contains local relative paths; optional third arg is a flat upload folder.
No Git, private execution archive, original DLL, or network access is required.
"""
import hashlib,json,sys,zipfile,zlib,stat,re,unicodedata
from pathlib import Path,PurePosixPath
from collections import Counter


def safe(p):
    assert isinstance(p,str) and p and '\\' not in p and ':' not in p and not p.startswith('/') and '\0' not in p
    assert PurePosixPath(p).as_posix()==p and unicodedata.normalize('NFC',p)==p
    assert all(c not in ['.','..',''] and not c.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',c) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',c) for c in PurePosixPath(p).parts)
    assert not p.startswith(('.git/','.test-rationalization-'))
    return p


def verify(manifest_path,index_path,flat=None,repo=None,extract=None):
    manifest_bytes=Path(manifest_path).read_bytes();d=json.loads(manifest_bytes);index=json.loads(Path(index_path).read_bytes())
    assert hashlib.sha256(manifest_bytes).hexdigest()==index['manifest']['sha256'] and len(manifest_bytes)==index['manifest']['bytes']
    rows={r['path']:r for r in d['files']};assert len(rows)==len(d['files'])==len({safe(p).casefold() for p in rows})
    assert d['private_raw_included'] is False and d['original_queries']==0 and d['binary_volumes'] is False
    parts=[*index['reference_parts'],dict(index['new_active'],name=Path(index['new_active']['path']).name,metadata='.w3-review-manifest.json',metadata_sha256=hashlib.sha256(manifest_bytes).hexdigest())]
    assert len(parts)==index['required_parts']==12
    seen=set();documents={};root=Path(repo or '.').resolve();destination=Path(extract).resolve() if extract else None
    if destination:destination.mkdir(parents=True,exist_ok=False)
    for part in parts:
        p=Path(flat)/part['name'] if flat else root/part['path']
        with p.open('rb') as f:assert hashlib.file_digest(f,'sha256').hexdigest()==part['sha256']
        assert p.stat().st_size==part['bytes'] and p.stat().st_size<180*1024**2
        expected={n:r for n,r in rows.items() if r['part']==part['name']}
        with zipfile.ZipFile(p) as z:
            names=z.namelist();assert len(names)==len(set(names))==len({n.casefold() for n in names})
            assert set(names)==set(expected)|{part['metadata']}
            assert hashlib.sha256(z.read(part['metadata'])).hexdigest()==part['metadata_sha256']
            for name,row in expected.items():
                safe(name);assert name not in seen;seen.add(name)
                info=z.getinfo(name);assert not info.is_dir() and not info.flag_bits&1 and not stat.S_ISLNK(info.external_attr>>16)
                assert info.file_size==row['bytes'] and format(info.CRC,'08x')==row['crc32']
                digest=hashlib.sha256();crc=0;count=0
                target=None
                if destination and part['metadata']=='.w3-review-manifest.json':
                    out=destination/name;assert out.resolve().is_relative_to(destination);out.parent.mkdir(parents=True,exist_ok=True);target=out.open('xb')
                try:
                    with z.open(info) as stream:
                        for chunk in iter(lambda:stream.read(1024*1024),b''):
                            digest.update(chunk);crc=zlib.crc32(chunk,crc);count+=len(chunk)
                            if target:target.write(chunk)
                finally:
                    if target:target.close()
                assert count==row['bytes'] and digest.hexdigest()==row['sha256'] and format(crc&0xffffffff,'08x')==row['crc32']
                if name.endswith('-identities-v1.json') and '/completion/w3/' in name:documents[name]=json.loads(z.read(name))
    assert seen==set(rows)
    # Independent parent/occurrence census; raw private test execution is not
    # reproduced or implied by transport hash verification.
    counts={}
    for name,obj in documents.items():
        if '/build-and-tests/' in name:
            t=obj['tests'];assert len(t)==len({(r['suite'],r['identity_sha256'],r['occurrence']) for r in t})==21094
            assert Counter(r['outcome'] for r in t)==dict(Passed=21080,Failed=14)
            counts[Path(name).name]=dict(Counter(r['outcome'] for r in t))
        elif '/python-node-scanners/python-identities-' in name:
            assert len(obj['parents'])==len({r['id'] for r in obj['parents']})==956
            assert Counter(r['outcome'] for r in obj['parents'])==dict(PASS=830,FAIL=113,ERROR=13)
            assert Counter(r['outcome'] for r in obj['events'])==dict(FAIL=185,ERROR=13)
            counts['Python']=dict(Counter(r['outcome'] for r in obj['parents']))
    assert len([n for n in counts if n.startswith(('Debug-','Release-'))])==2 and 'Python' in counts
    return dict(status='ALL_LOGICAL_PAYLOAD_SHA_SIZE_CRC_AND_PHYSICAL_PART_SHA_PASS',files=len(seen),parts=len(parts),
        census=counts,extraction='NEW_ACTIVE_PAYLOADS_EXTRACTED;UNCHANGED_REFERENCE_PARTS_FULL_STREAM_READBACK' if destination else 'NO_FILESYSTEM_EXTRACTION',
        scope='DERIVATIVE_BYTES_AND_PUBLISHED_CENSUS_ONLY;NO_LIVE_GIT_PRIVATE_OR_TEST_EXECUTION_ATTESTATION',external_upload='NOT_PERFORMED')


if __name__=='__main__':
    print(json.dumps(verify(sys.argv[1],sys.argv[2],sys.argv[3] if len(sys.argv)>3 else None),indent=2))
