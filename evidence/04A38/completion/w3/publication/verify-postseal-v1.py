"""Standalone final W3 supplement byte verifier; no live/private execution claim."""
import hashlib,json,sys,zipfile,zlib,stat,re,unicodedata
from pathlib import Path,PurePosixPath

def safe(name):
    assert name and '\\' not in name and ':' not in name and '\0' not in name and not name.startswith('/')
    assert PurePosixPath(name).as_posix()==name and unicodedata.normalize('NFC',name)==name
    assert all(c not in ['.','..',''] and not c.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',c) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',c) for c in PurePosixPath(name).parts)
    assert not name.startswith(('.git/','.test-rationalization-'))

def verify(receipt,archive):
    d=json.loads(Path(receipt).read_bytes());p=Path(archive)
    with p.open('rb') as f:assert hashlib.file_digest(f,'sha256').hexdigest()==d['archive']['sha256']
    assert p.stat().st_size==d['archive']['bytes'] and p.stat().st_size<180*1024**2
    with zipfile.ZipFile(p) as z:
        meta=z.read('.w3-postseal-manifest.json');assert hashlib.sha256(meta).hexdigest()==d['metadata_sha256']
        doc=json.loads(meta);assert not doc['private_raw_included'] and not doc['binary_volumes']
        rows=doc['files'];names=z.namelist()
        assert len(names)==len(set(names))==len({n.casefold() for n in names})
        assert set(names)=={r['path'] for r in rows}|{'.w3-postseal-manifest.json'}
        for row in rows:
            name=row['path'];safe(name);info=z.getinfo(name)
            assert not info.is_dir() and not info.flag_bits&1 and not stat.S_ISLNK(info.external_attr>>16)
            data=z.read(name)
            assert (len(data),hashlib.sha256(data).hexdigest(),format(zlib.crc32(data)&0xffffffff,'08x'))==(row['bytes'],row['sha256'],row['crc32'])
            assert info.file_size==row['bytes'] and format(info.CRC,'08x')==row['crc32']
    return dict(status='POSTSEAL_PHYSICAL_SHA_AND_ALL_PAYLOAD_SHA_SIZE_CRC_PASS',files=len(rows),
        scope='DERIVATIVE_BYTES_ONLY;NO_INDEPENDENT_LIVE_GIT_PRIVATE_OR_TEST_EXECUTION')

if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2]),indent=2))
