"""Actual physical-stage privacy gate precedes archive creation, without exceptions."""
from common import *
from publication_gate_v2 import *
import zipfile,zlib

lock()
selection=load(PRIVATE/'publication-selection-v2-with-exclusions.json');stage=ROOT/selection['stage']
files=selection['files'];names=physical_inventory(stage,files,selection['excluded'])
start=utc()
findings,reviewed,details,encodings=scan(stage,names,load(HERE/'entry/source-lock.json')['files'])
save(PRIVATE/'final-stage-scan-raw.json',dict(start_utc=start,end_utc=utc(),findings=findings,details=details,reviewed=reviewed,encodings=encodings,selection=pin(PRIVATE/'publication-selection-v2-with-exclusions.json')))
verdict=decide(findings,details,files,selection['source_dispositions'])
physical_inventory(stage,files,selection['excluded'])
assert len(findings)==25,'UNEXPECTED_STAGED_FINDING_POPULATION'
payload_fingerprint=sha(json.dumps([(r['path'],r['bytes'],r['sha256'],r['crc32']) for r in files],separators=(',',':')).encode())
gate=dict(**verdict,command_exit=0,sanitization_status='DETERMINISTIC_SANITIZED_DERIVATIVES;RAW_PRESERVED_LOCAL',
    scope='Exact physical staged derivative, including original synthetic test literals with25 exact occurrence dispositions. Not a repository-wide privacy result.',
    payload_files=len(files),payload_fingerprint=payload_fingerprint,protected_paths=len(selection['excluded']),
    source_coverage=load(HERE/'publication/source-coverage.json')['source_paths'],legacy_source_encodings=encodings,
    scanner=pin(ROOT/'scripts/verify-04a13-publication.py'),scanner14=pin(ROOT/'scripts/verify-04a14-publication.py'),
    scanner_rules_changed=False,source_or_test_changed=False,gates=GATES,raw_receipt=pin(PRIVATE/'final-stage-scan-raw.json'))
def metadata(name,obj):
    data=(json.dumps(obj,ensure_ascii=False,indent=2)+'\n').encode();path=stage/name
    with path.open('xb') as f:f.write(data)
    return dict(path=name,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}')
gate_row=metadata('.review-gate.json',gate)
main_files=files+[gate_row]
index=dict(schema='04A39-correction-review-v1',purpose='DERIVATIVE_REVIEW_ONLY;OWNER_DECISION_PENDING',
    files=main_files,protected_exclusions=selection['excluded'],source_dispositions=selection['source_dispositions'],
    self_policy='This index excluded only from its own hash map. External delivery receipt pins the index and every physical member, including metadata.',
    source_coverage=load(HERE/'publication/source-coverage.json'),gates=GATES)
index_row=metadata('.review-inventory.json',index)
# Newly generated metadata also goes through the actual scanner, before ZIP construction.
meta_findings,_,meta_details,_=scan(stage,[gate_row['path'],index_row['path']],[])
assert not meta_findings,'GENERATED_PUBLICATION_METADATA_NOT_PRIVATE_SAFE'
all_files=main_files+[index_row]
physical_inventory(stage,all_files,selection['excluded'])
save('publication/privacy-gate.json',gate)
save('publication/payload-index.json',dict(files=all_files,index=index_row,gate=gate_row,self_policy='Publication payload-index is an external receipt, not an unscanned extra archive member.'))
print('PASS staged privacy gate:',len(files),'payloads;',len(findings),'documented synthetic occurrences; unresolved0. Creating archive now.',flush=True)
archive=PRIVATE/'BESHStatNG_04A39_CORRECTION_REVIEW_v1.zip'
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
    for row in sorted(all_files,key=lambda r:r['path']):
        path=stage/row['path'];assert digest(path)==row['sha256']
        info=zipfile.ZipInfo(row['path'],date_time=(2026,11,14,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.external_attr=0o600<<16
        z.writestr(info,path.read_bytes())
with zipfile.ZipFile(archive) as z:
    assert z.testzip() is None
    assert len(z.namelist())==len(all_files)==len(set(z.namelist()))==len({p.casefold() for p in z.namelist()})
    assert set(z.namelist())=={r['path'] for r in all_files}
    for row in all_files:
        info=z.getinfo(row['path']);data=z.read(info)
        assert len(data)==row['bytes'] and sha(data)==row['sha256'] and f'{info.CRC:08x}'==row['crc32']
assert archive.stat().st_size<180*1024*1024,'UPLOAD_SIZE_EXCEEDED_KEEP_ARTIFACT_NO_DELETE'
physical_inventory(stage,all_files,selection['excluded']);lock()
save('publication/archive-readback.json',dict(status='PASS_SHA_SIZE_CRC_EXACT_PHYSICAL_MEMBERS',archive=pin(archive),physical_files=len(all_files),payload_files=len(files),
    index=index_row,gate=gate_row,whole_archive_crc_test='PASS',per_file_crc_sha_size='PASS',missing=[],extra=[],duplicates=[],case_collisions=[],
    privacy='PASS_STAGED_DERIVATIVE_ONLY;25_EXACT_SYNTHETIC_MATCHES;ROOT_NONPASS_UNWAIVED',upload_performed=False,
    source_tree_modified=False,gates=GATES))
print('Archive verified:',archive.name,archive.stat().st_size,'bytes;',len(all_files),'physical members',flush=True)
