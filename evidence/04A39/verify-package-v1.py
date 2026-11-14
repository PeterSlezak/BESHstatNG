"""Standalone transport integrity reader. Stdlib only, no extraction or live owner access."""
import hashlib
import json
from pathlib import PurePosixPath, Path
import stat
import sys
import zipfile
import zlib

def safe(path):
    assert isinstance(path,str) and path and '\\' not in path and ':' not in path and not any(ord(c)<32 or c in '<>"|?*' for c in path), 'UNSAFE_PATH'
    p=PurePosixPath(path)
    assert not p.is_absolute() and all(x not in ['.','..',''] for x in path.split('/')), 'PATH_TRAVERSAL'
    assert str(p)==path and not any(x.endswith((' ','.')) for x in p.parts), 'AMBIGUOUS_PATH'
    assert not any(x.split('.')[0].upper() in {'CON','PRN','AUX','NUL',*('COM'+str(i) for i in range(1,10)),*('LPT'+str(i) for i in range(1,10))} for x in p.parts), 'WINDOWS_RESERVED_NAME'
    return path

def verify(path):
    with zipfile.ZipFile(path) as archive:
        infos=archive.infolist();names=[safe(x.filename) for x in infos]
        assert len(names)==len(set(names))==len({x.casefold() for x in names}), 'DUPLICATE_OR_CASE_COLLISION'
        assert all(not stat.S_ISLNK(x.external_attr>>16) for x in infos), 'SYMLINK'
        manifest=json.loads(archive.read('.review-manifest.json'))
        rows=manifest['files'];expected=[safe(x['path']) for x in rows]
        assert len(expected)==len(set(expected))==len({x.casefold() for x in expected}), 'MANIFEST_DUPLICATE'
        assert set(names)==set(expected)|{'.review-manifest.json'}, 'MISSING_OR_EXTRA_PAYLOAD'
        for row in rows:
            b=archive.read(row['path']);info=archive.getinfo(row['path'])
            assert len(b)==row['bytes']==info.file_size, 'SIZE_MISMATCH'
            assert hashlib.sha256(b).hexdigest()==row['sha256'], 'SHA_MISMATCH'
            assert format(zlib.crc32(b)&0xffffffff,'08x')==row['crc32']==format(info.CRC,'08x'), 'CRC_MISMATCH'
    return dict(files=len(rows),bytes=Path(path).stat().st_size,sha256=hashlib.file_digest(open(path,'rb'),'sha256').hexdigest(),
        scope='Derivative bytes/path/CRC proof only; no native/scientific or private owner-machine execution claim.')

if __name__=='__main__':print(json.dumps(verify(sys.argv[1]),sort_keys=True))
