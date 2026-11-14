"""Reproducible ACTIVE derivative from the authenticated earlier selection plus correction evidence."""
from common import *
from diagnostic_producer import redactor
from publication_gate_v2 import physical_inventory, safe_name
import zlib

lock()
prior=load(PRIVATE/'publication-selection-v1.json');source_stage=ROOT/prior['stage']
physical_inventory(source_stage,prior['files'],prior['excluded'])
dispositions=load(PRIVATE/'source-literal-dispositions.json')
protected=set(dispositions['protected_source_paths'])
stage=PRIVATE/'publication-stage-v2';stage.mkdir(exist_ok=False)
records={};exclusions=list(prior['excluded']);coverage=[]
def add(path,data,authority):
    safe_name(path)
    assert not any(p!=path and p.casefold()==path.casefold() for p in records)
    out=stage/path;out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(data)
    records[path]=dict(path=path,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}',authority=authority)
for row in prior['files']:
    if row['path'] not in protected:add(row['path'],(source_stage/row['path']).read_bytes(),row['authority'])
for path in sorted(protected):
    original=pin(path);raw=(ROOT/path).read_bytes()
    dest=PRIVATE/'source-preimages'/path;dest.parent.mkdir(parents=True,exist_ok=True)
    with dest.open('xb') as f:f.write(raw)
    rendered=redactor.redact_string(raw.decode('utf-8-sig')).encode()
    name='review-renderings/'+path+'.review.txt'
    assert not redactor.scan(name,rendered),'SOURCE_RENDERING_STILL_SENSITIVE'
    add(name,rendered,dict(kind='SANITIZED_SPECIFIC_SOURCE_PREIMAGE_NOT_EXECUTABLE_AUTHORITY',original=original))
    exclusions.append(dict(path=path,original=original,reason='EXACT_SOURCE_FINDING_DISPOSITION;HISTORICAL_RAW_MACHINE_REFERENCE_OR_PERSONAL_IDENTIFIER',derivative=name))
for path in sorted(HERE.rglob('*')):
    if not path.is_file() or '__pycache__' in path.parts:continue
    rel=path.relative_to(ROOT).as_posix();data=path.read_bytes()
    if redactor.scan(rel,data):
        if path.suffix=='.json':data=(json.dumps(redactor.redact(json.loads(data)),ensure_ascii=False,indent=2)+'\n').encode()
        else:data=redactor.redact_string(data.decode('utf-8-sig')).encode()
        name='review-renderings/'+rel+'.review.txt'
        assert not redactor.scan(name,data),'CORRECTION_RENDERING_STILL_SENSITIVE'
        add(name,data,dict(kind='SANITIZED_CORRECTION_PREIMAGE',original=pin(path)))
        if rel in records:
            # Raw paths are never selected in v1 for these artifacts, but fail closed if they were.
            raise AssertionError('RAW_CORRECTION_SELECTED')
        exclusions.append(dict(path=rel,original=pin(path),reason='PRIVATE_DIAGNOSTIC_PREIMAGE;SEPARATE_RENDERING',derivative=name))
    else:add(rel,data,dict(kind='EXACT_CORRECTION_EVIDENCE',original=pin(path)))
for row in load(HERE/'entry/source-lock.json')['files']:
    name=('review-renderings/'+row['path']+'.review.txt') if row['path'] in protected else row['path']
    assert name in records
    coverage.append(dict(original=row,payload=dict((k,records[name][k]) for k in ('path','bytes','sha256')),classification='SANITIZED_REVIEW_RENDERING' if row['path'] in protected else 'EXACT_SOURCE'))
save('publication/source-coverage.json',dict(source_paths=len(coverage),exact=sum(r['classification']=='EXACT_SOURCE' for r in coverage),rendered=len(protected),files=coverage,
    limits='Review transport contains31 specifically identified source renderings with raw source hashes; it is not a substitute source checkout and must not be used to execute historical operators or build altered UI source. No local product/test source was edited.'))
path=HERE/'publication/source-coverage.json';add(path.relative_to(ROOT).as_posix(),path.read_bytes(),dict(kind='EXACT_CORRECTION_EVIDENCE',original=pin(path)))
# One explicit protected-path rule per raw preimage, de-duplicated without losing its original SHA.
by_path={}
for row in exclusions:
    if row['path'] in by_path:assert by_path[row['path']]['original']==row['original']
    by_path[row['path']]=row
exclusions=sorted(by_path.values(),key=lambda r:r['path'])
save(PRIVATE/'publication-selection-v2.json',dict(stage=stage.relative_to(ROOT).as_posix(),files=sorted(records.values(),key=lambda r:r['path']),excluded=exclusions,source_count=len(coverage),source_dispositions=dispositions['allowed']))
save('publication/protected-exclusions.json',dict(files=exclusions,scope='Derivative-only exclusions; originals preserved locally; known sensitive values omitted; no changes to source Git ignore or historical scanner rules.',protected_private_roots=[PRIVATE.relative_to(ROOT).as_posix(),'.test-rationalization-04A39.tmp'],raw_hashes_protected=True))
path=HERE/'publication/protected-exclusions.json'
# This metadata has no protected raw values; it will also undergo the actual staged scan.
add(path.relative_to(ROOT).as_posix(),path.read_bytes(),dict(kind='EXACT_CORRECTION_EVIDENCE',original=pin(path)))
save(PRIVATE/'publication-selection-v2-with-exclusions.json',dict(stage=stage.relative_to(ROOT).as_posix(),files=sorted(records.values(),key=lambda r:r['path']),excluded=exclusions,source_count=len(coverage),source_dispositions=dispositions['allowed']))
print('Final stage',len(records),'payloads;',len(coverage)-len(protected),'exact sources plus',len(protected),'explicit source renderings; NO ZIP',flush=True)
