"""Final source/Git/original checkpoint readback after physical publication gate."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import zipfile,collections
lock();publication=load(HERE/'publication/physical-readback-v1.json');assert publication['status']=='PHYSICAL_DERIVATIVE_PUBLICATION_PASS'
archive=publication['archive'];assert pin(archive['path'])==archive
with zipfile.ZipFile(ROOT/archive['path']) as z:
    assert z.testzip() is None
    index=__import__('json').loads(z.read('.review-index.json'));assert len(index['files'])+1==len(z.namelist())
freeze=load(HERE/'matrix/source-freeze-v2.json');delta={r['before']['path']:r['after'] for r in freeze['source_delta']};entry=load(HERE/'entry/source-preimages-v3.json')['files']
for r in entry:assert pin(r['path'])==delta.get(r['path'],r)
for r in freeze['new_tests']:assert pin(r['path'])==r
for p,h in freeze['source_pins'].items():assert pin(p)['sha256']==h
for c in ['Debug','Release']:
    proof=load(HERE/('abi-cil/'+c+'-proof-v2.json'))
    for r in proof['assembly_hashes']+[proof['current_metadata'],proof['current_exports']]:assert pin(r['path'])==r
pres=load(HERE/'preservation/historical-readback-v4.json');converted={r['original']['path']:r for r in pres['owner_line_ending_transitions']};checkpoint=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files']
for r in checkpoint:
    expected=converted[r['path']]['committed'] if r['path'] in converted else {k:r[k] for k in ['path','bytes','sha256']}
    assert pin(r['path'])==expected
    copy=ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint'/r['path'];assert pin(copy)['sha256']==r['sha256'] and copy.stat().st_size==r['bytes']
for r in pres['archives']:assert pin(r['path'])==r
assert pin('evidence/04A39/register/defects-v7.json')==pres['canonical_register']
assert not git('diff','--check') and not git('diff','--cached','--name-only')
prior=load(HERE/'final/git-final-v1.json');current_index=pin('.git/index');assert current_index==prior['index']
gitfinal=dict(prior,utc=utc(),untracked=git('ls-files','--others','--exclude-standard').decode().splitlines(),modified=git('diff','--name-only').decode().splitlines(),
    HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),
    branch=git('branch','--show-current').decode().strip(),diff_check_exit=0,staged_empty=True,index=current_index,publication_archive=archive)
save('final/git-postpublication-v2.json',gitfinal)
save('preservation/postpublication-lock-v1.json',dict(utc=utc(),source_entry_checked=1353,authorized_transitions=16,new_tests_checked=2,compiler_input_files=len(freeze['source_pins']),
    original_checkpoint=468,original_bytes_or_authorized_LF_and_exact_copies=True,historical_archives=pres['archives'],original_register_unchanged=pres['canonical_register'],
    full_8984_protected_byte_union='Earlier exact bounded audit retained; this postseal reruns468 originals/copies,1353 source locks,shipping/CIL pins,archive and register locks. No false fresh full37484/154 claim.',
    prior_preservation=pin(HERE/'preservation/historical-readback-v4.json'),physical_publication=pin(HERE/'publication/physical-readback-v1.json'),gates=GATES))
save('publication/postseal-delivery-v1.json',dict(archive=archive,physical_members=publication['physical_members'],indexed_members=publication['indexed'],
    physical_privacy='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',unresolved_physical_findings=0,
    overall='NOT_READY_04A40',owner_review=pin(HERE/'final/owner-review.md'),readiness=pin(HERE/'final/readiness-v2.json'),
    scanner_receipt=pin(HERE/'publication/physical-readback-v1.json'),Git=pin(HERE/'final/git-postpublication-v2.json'),preservation=pin(HERE/'preservation/postpublication-lock-v1.json'),
    self_hash='Excluded; external receipt binds immutable ZIP and independently hashed receipts without circular self-reference.',accepted=False,merged=False,gates=GATES))
print('Postpublication source1353/checkpoint468/Git/index/shipping/archive locks verified',flush=True)
