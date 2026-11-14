"""Recover completed per-payload assertions; correct only unsafe synthetic receipt label, then package/read back."""
from common39 import *
import runpy
import zipfile
import zlib

source_lock()
privacy=runpy.run_path(str(HERE/'transport-privacy-v2.py'))
reader=runpy.run_path(str(HERE/'verify-package-v1.py'))
old_path=HERE/'publication/privacy-and-exclusions-v1.json';old=load(old_path)
failed=load(PRIVATE/'commands/a39v1-packet-publication-v3/command.json')
assert failed['exit_code']==1
error=(PRIVATE/'commands/a39v1-packet-publication-v3/stderr.log').read_text(encoding='utf-8')
assert "assert not privacy2['scan'](p.relative_to(ROOT).as_posix(),data)" in error
assert not (HERE/'publication/payload-manifest-v1.json').exists()
source=(HERE/'publish-v3.py').read_text(encoding='utf-8')
assert source.index("assert not privacy2['scan'](name,data)") < source.index("rows.append(dict(pin(path)") < source.index("decisions.append(dict(path=name")
assert source.index("save('publication/privacy-and-exclusions-v1.json'") < source.index("assert not privacy2['scan'](p.relative_to(ROOT).as_posix(),data)")
new=dict(old)
new['controls']=[dict(row,id='ADVERSE_CONTROL_LABEL_SHA256:'+sha(row['id'].encode())) for row in old['controls']]
new['previous_failed_receipt']=pin(old_path)
new['recovery']=dict(driver=pin(HERE/'publish-v3.py'),failed_command=pin(PRIVATE/'commands/a39v1-packet-publication-v3/command.json'),
    source_kernel=pin('scripts/verify-04a13-publication.py'),memory_binding=pin(HERE/'transport-privacy-v2.py'),
    method='Each payload assertion completed before its successful SHA was appended to decisions. All exact bytes/SHA are independently re-read now; assertions are retained, not skipped or reclassified. Only final receipt/ZIP/readback was unstarted.',
    original_failure='Synthetic C:/drive adverse label tripped actual drive-path scanner; old receipt retained unchanged.',
    scanner_environment='Same owner ROOT and inherited process environment; scanner/kernel sources unchanged. No new privacy waiver; no test campaign replay.')
save('publication/privacy-and-exclusions-v2.json',new)
path=HERE/'publication/privacy-and-exclusions-v2.json'
assert not privacy['scan'](path.relative_to(ROOT).as_posix(),path.read_bytes())
rows=[]
for decision in old['decisions']:
    path=ROOT/decision['path'];data=path.read_bytes()
    assert sha(data)==decision['sha256'],'CHANGED_SUCCESSFUL_PAYLOAD'
    reader['safe'](decision['path'])
    rows.append(dict(pin(path),crc32=format(zlib.crc32(data)&0xffffffff,'08x')))
for path in [HERE/'publication/privacy-and-exclusions-v2.json',HERE/'publish-recovery-v4.py']:
    data=path.read_bytes();assert not privacy['scan'](path.relative_to(ROOT).as_posix(),data)
    rows.append(dict(pin(path),crc32=format(zlib.crc32(data)&0xffffffff,'08x')))
assert len(rows)==len({x['path'] for x in rows})==len({x['path'].casefold() for x in rows})
manifest=dict(schema='04A39_REVIEW_TRANSPORT_V1',main=MAIN,files=sorted(rows,key=lambda x:x['path']),
    privacy=pin(HERE/'publication/privacy-and-exclusions-v2.json'),scope='NOT_READY39 derivative; retained completed per-payload actual13 assertions plus fresh exact byte/CRC/SHA readback; raw repository nonpasses unwaived',
    failed_receipt=pin(old_path),failed_attempt=pin(PRIVATE/'commands/a39v1-packet-publication-v3/command.json'),
    external_upload='NOT_PERFORMED',source_renderings='Separately named renderings/omissions are not authoritative raw source; exact raw SHA and preserved originals remain bound.')
save('publication/payload-manifest-v1.json',manifest)
folder=PRIVATE/'review-v1';folder.mkdir(exist_ok=False)
archive=folder/'BESHstatNG_04A39_REVIEW_v1.zip'
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
    z.writestr('.review-manifest.json',(HERE/'publication/payload-manifest-v1.json').read_bytes())
    for row in rows:
        assert pin(row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
        z.write(ROOT/row['path'],row['path'])
assert archive.stat().st_size<180*1024**2
result=reader['verify'](archive)
extract=PRIVATE/'review-readback-v1';extract.mkdir(exist_ok=False)
with zipfile.ZipFile(archive) as z:z.extractall(extract)
for row in rows:
    b=(extract/row['path']).read_bytes();assert len(b)==row['bytes'] and sha(b)==row['sha256']
save('publication/physical-readback-v1.json',dict(archive=pin(archive),result=result,extracted_payloads=len(rows),
    actual13_payload_privacy_findings=0,raw_repository_privacy='FAIL_NOT_WAIVED',privacy=pin(HERE/'publication/privacy-and-exclusions-v2.json'),
    retained_success_assertions=len(old['decisions']),fresh_receipt_and_recovery_scans=2,
    logical_manifest=pin(HERE/'publication/payload-manifest-v1.json'),external_upload='NOT_PERFORMED'))
source_lock();print('Review ZIP',archive.stat().st_size,'bytes;',len(rows),'payloads',flush=True)
