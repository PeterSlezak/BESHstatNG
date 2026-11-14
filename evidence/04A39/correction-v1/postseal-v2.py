"""Consolidate all final referenced readbacks into the complementary metadata ZIP."""
from common import *
from diagnostic_producer import redactor
from preservation import verify_checkpoint
import zipfile,zlib

prior=load(HERE/'final/delivery-receipt-v2.json')
paths={r['path'] for r in prior['postseal_files']}
paths.update((HERE/p).relative_to(ROOT).as_posix() for p in ['postseal-v2.py','final/delivery-receipt-v1.json','final/delivery-receipt-v2.json','final/git-after-postseal-v1.json','preservation/after-postseal.json','final/added-files-v2.json'])
rows=[];payload={}
for name in sorted(paths):
    path=ROOT/name;data=path.read_bytes()
    assert not redactor.scan(name,data),'POSTSEAL_V2_PRIVACY_FAILURE'
    row=dict(path=name,bytes=len(data),sha256=sha(data),crc32=f'{zlib.crc32(data):08x}')
    assert pin(path)=={k:row[k] for k in ('path','bytes','sha256')}
    rows.append(row);payload[name]=data
index=dict(schema='04A39-correction-postseal-v2',files=rows,main_archive=prior['main_archive'],
    supersedes_for_upload_only=prior['postseal_archive'],previous_archive_preserved=True,
    note='This compact companion includes every final readback linked by delivery-v2. Upload main ZIP plus this v2 ZIP and external delivery-v3. Earlier postseal-v1 remains local historical evidence; not an additional required upload.',gates=GATES)
data=(json.dumps(index,ensure_ascii=False,indent=2)+'\n').encode()
assert not redactor.scan('.postseal-inventory.json',data)
payload['.postseal-inventory.json']=data
archive=PRIVATE/'BESHStatNG_04A39_CORRECTION_POSTSEAL_v2.zip'
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    for name,raw in sorted(payload.items()):
        info=zipfile.ZipInfo(name,date_time=(2026,11,14,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.external_attr=0o600<<16
        z.writestr(info,raw)
with zipfile.ZipFile(archive) as z:
    assert z.testzip() is None and len(z.namelist())==len(payload)==len({n.casefold() for n in z.namelist()})
    for name,raw in payload.items():assert z.read(name)==raw and z.getinfo(name).CRC==zlib.crc32(raw)
unchanged=verify_checkpoint();lock();assert not git('diff','--check') and not git('diff','--cached','--name-only')
receipt=dict(status='BLOCKED_04A39_OWNER_DECISION_REQUIRED',utc=utc(),main=MAIN,branch='codex/04A-39',
    recommended_uploads=[prior['main_archive'],pin(archive)],retained_old_postseal=prior['postseal_archive'],
    previous_delivery=pin(HERE/'final/delivery-receipt-v2.json'),postseal_v2_files=rows,postseal_v2_members=len(payload),postseal_v2_index_sha256=sha(data),
    publication_privacy=dict(main_unresolved=0,main_exact_synthetic_matches=25,postseal_v2_unresolved=0,postseal_v2_matches=0,original_repository_privacy='FAIL_UNWAIVED'),
    crc_sha_size_readbacks='PASS_BOTH_RECOMMENDED_ARCHIVES',source_paths_reverified_after_publication=1353,
    original_checkpoint_files_reverified_after_publication=len(unchanged),original_file_identities_and_hashes_unchanged=True,
    HEAD=git('rev-parse','HEAD').decode().strip(),main_tip=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),
    index=pin('.git/index'),staged_empty=True,diff_check_exit=0,production_or_existing_test_edits=0,commit_merge_push=False,
    owner_review=pin(HERE/'final/owner-review-v2.md'),readiness=pin(HERE/'final/readiness.json'),gates=GATES,
    self_policy='This external delivery-v3 is the only required metadata outside the two recommended ZIPs. It is excluded from their own hash maps to avoid circularity. All referenced prior readback documents are inside postseal-v2.',
    owner_decision='Exact39 publication-domain disposition needed; historical FAILs and errors remain unwaived; no39 acceptance or04A40 authority.')
assert not redactor.scan('final/delivery-receipt-v3.json',(json.dumps(receipt)+'\n').encode())
save('final/delivery-receipt-v3.json',receipt)
print(json.dumps(dict(status=receipt['status'],recommended_uploads=receipt['recommended_uploads'],postseal_members=len(payload),source_lock='UNCHANGED',index='UNCHANGED',staged_empty=True,diff_check_exit=0),sort_keys=True))
