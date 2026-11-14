"""Inventory both real previous packages, then stage an explicit derivative. No ZIP creation."""
from common import *
from scanner_adapter import guard
import collections, importlib.util, shutil, zipfile, zlib

lock()
spec=importlib.util.spec_from_file_location('old_transport_redactor',PRIOR/'transport-privacy-v2.py')
transport=importlib.util.module_from_spec(spec);spec.loader.exec_module(transport)
transport.actual.re=guard.RegexProxy()
ext=load(PRIVATE/'external-package.json')
external=Path(ext['path'])
old=ROOT/load(PRIOR/'final/delivery-receipt-v1.json')['archive']['path']
old_sha=digest(old)
def inventory(archive):
    with zipfile.ZipFile(archive) as z:
        assert len(z.namelist())==len(set(z.namelist()))==len({n.casefold() for n in z.namelist()})
        assert z.testzip() is None
        rows=[]
        for item in z.infolist():
            if item.is_dir():continue
            assert not item.filename.startswith(('/', '\\')) and '..' not in Path(item.filename).parts
            data=z.read(item)
            rows.append(dict(path=item.filename,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}'))
        return rows
v6=inventory(external);codex=inventory(old)
index=load(PRIVATE/'v6-index.json')
v6payload={r['path']:r for r in v6 if not r['path'].startswith('_review_package/')}
codexpayload={r['path']:r for r in codex if r['path']!='.review-manifest.json'}
assert len(v6payload)==4479 and len(codexpayload)==1956
save(PRIVATE/'previous-package-inventories.json',dict(v6=v6,codex=codex))
common=set(v6payload)&set(codexpayload)
save('publication/previous-package-comparison.json',transport.redact(dict(v6=dict(archive_sha256=digest(external),payloads=4479,raw_exclusions=index['excluded']['private_raw_diagnostics'],crc_test='PASS',physical_files=len(v6)),
    codex=dict(archive_sha256=digest(old),payloads=1956,crc_test='PASS',physical_files=len(codex)),
    common_paths=len(common),common_identical=sum(v6payload[p]['sha256']==codexpayload[p]['sha256'] for p in common),
    common_different=[dict(path=p,v6_sha256=v6payload[p]['sha256'],codex_sha256=codexpayload[p]['sha256']) for p in sorted(common) if v6payload[p]['sha256']!=codexpayload[p]['sha256']],
    v6_only=sorted(set(v6payload)-set(codexpayload)),codex_only=sorted(set(codexpayload)-set(v6payload)),
    explanation='v6 is a broad robocopy tree with named folder exclusions and only a narrow raw-managed filename exclusion; Codex is an explicit smaller payload with separately named sanitized review derivatives. These are not equivalent archives. Both CRC readbacks are integrity checks, not privacy acceptance.',
    old_packager_not_executed_or_modified=True,original_archives_unchanged=True)))

stage=PRIVATE/'publication-stage-v1';stage.mkdir(exist_ok=False)
records={};excluded=[]
def add(path,data,authority):
    assert not path.startswith(('/', '\\')) and ':' not in path and '..' not in Path(path).parts
    assert path.casefold() not in {p.casefold() for p in records if p!=path}
    target=stage/path;target.parent.mkdir(parents=True,exist_ok=True)
    # The stage is disposable; authoritative originals are never changed.
    target.write_bytes(data)
    records[path]=dict(path=path,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}',authority=authority)
with zipfile.ZipFile(old) as z:
    for path in codexpayload:
        add(path,z.read(path),dict(kind='RETAINED_SANITIZED_DERIVATIVE_OR_SOURCE',archive_sha256=old_sha,original_payload=codexpayload[path]))
# Include every frozen source/test path exactly; no blanket source exclusion or redaction.
for row in load(HERE/'entry/source-lock.json')['files']:
    add(row['path'],(ROOT/row['path']).read_bytes(),dict(kind='EXACT_CURRENT_SOURCE',original=row))
# New correction scripts and review evidence use separate renderings when needed.
protected=set(r['path'] for r in load(PRIVATE/'privacy-13-replay-v2.json')['findings'])
protected.update(r['path'] for row in load(PRIVATE/'focused-cache.json')['rows'] for r in row['findings'])
for path in sorted(HERE.rglob('*')):
    if not path.is_file() or '__pycache__' in path.parts:continue
    rel=path.relative_to(ROOT).as_posix();data=path.read_bytes()
    findings=transport.scan(rel,data)
    if findings:
        protected.add(rel)
        if path.suffix=='.json':
            data=(json.dumps(transport.redact(json.loads(data)),ensure_ascii=False,indent=2)+'\n').encode()
        else:data=transport.redact_string(data.decode('utf-8-sig')).encode()
        derivative='review-renderings/'+rel+'.review'+path.suffix
        assert not transport.scan(derivative,data), 'UNSANITIZED_NEW_DERIVATIVE'
        add(derivative,data,dict(kind='SANITIZED_RENDERING_NOT_EXECUTABLE_AUTHORITY',original=pin(path)))
        excluded.append(dict(path=rel,original=pin(path),reason='PROTECTED_RAW_DIAGNOSTIC_OR_WORKFLOW_PREIMAGE',derivative=derivative))
    else:add(rel,data,dict(kind='EXACT_CORRECTION_EVIDENCE',original=pin(path)))
for rel in sorted(protected):
    if not any(r['path']==rel for r in excluded):excluded.append(dict(path=rel,original=pin(rel),reason='IMMUTABLE_RAW_DIAGNOSTIC_LOCAL_ONLY;SANITIZED_COUNTERPART_IN_RETAINED_REVIEW_PAYLOAD'))
save(PRIVATE/'publication-selection-v1.json',dict(stage=stage.relative_to(ROOT).as_posix(),files=sorted(records.values(),key=lambda r:r['path']),excluded=excluded,source_count=len(load(HERE/'entry/source-lock.json')['files'])))
print('Previous ZIPs independently CRC/readback verified. Physical stage prepared; NO ZIP created.',len(records),'files',flush=True)
