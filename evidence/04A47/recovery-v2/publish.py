"""Full original privacy scopes and non-circular physical delivery sealing."""
import sys,zipfile,zlib,unicodedata
from pathlib import Path
ROOT=Path.cwd()
sys.path.insert(0,str(ROOT/'evidence/04A47/recovery-v2'))
from recovery import *
STATUS='READY_FOR_OWNER_04A47_POSTRECOVERY_SAFE_SOFTWARE_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
def publish():
    source_lock();payload();py=load(HERE/'python-reconciliation-v2.json');assert not py['unresolved_diagnostic_events']
    reader=original.module('recovery_standalone_reader',HERE/'review_reader_v2.py')
    pub=original.module('unchanged47privacy_helpers',OLD/'publication47.py')
    pub.PRIVATE=PRIVATE;pub.save=save
    # Same original paths()/scan() and authenticated guard; no filtering of payload.
    candidates=pub.candidate_scan('postrecovery-v2-candidates')
    for c in candidates:assert TARGET in {r['path'] for r in c['members']}
    folder=PRIVATE/'publication-v2';folder.mkdir()
    archive=folder/'BESHStatNG_04A47_POSTRECOVERY_SAFE_SOFTWARE_REVIEW_v2.zip'
    with zipfile.ZipFile(ZIP1) as z:contents={n:z.read(n) for n in z.namelist() if n!='.review-index.json'};legacy_index=z.read('.review-index.json')
    contents['evidence/04A47/recovery-v2/sealed-v1-index.json']=legacy_index
    contents['evidence/04A47/recovery-v2/sealed-v1-external-receipt.json']=RECEIPT1.read_bytes()
    for p in sorted(HERE.rglob('*')):
        if p.is_file():contents[p.relative_to(ROOT).as_posix()]=p.read_bytes()
    for n in ['.gitignore','docs/architecture/batch-04A-47-recovered-SPC23-owner-review-v2.md','evidence/04A24/frozen-capture-hashes.json','BESHStat.Excel.Common.Tests/src/Spc24ExpectedRejections.vb','planning/BESHStatNG_CrossPlatform_MasterPlan2.24.md']:
        contents[n]=(ROOT/n).read_bytes()
    assert TARGET not in contents
    # Root plan under original v1 member name remains sealed v1; current successor is explicit.
    contents['evidence/04A47/recovery-v2/current-plan-successor-v1.md']=(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md').read_bytes()
    contents['.review-index.json']=(json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(contents.items())]),indent=2)+'\n').encode()
    with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
        for n,b in sorted(contents.items()):z.writestr(n,b)
    verified=reader.verify(archive);adverse=reader.adverse(archive)
    neutral=original.module('neutral_recovery47',OLD/'neutral_privacy_replay.py').replay(archive)
    members,physical=pub.physical_scan(archive,'postrecovery-v2')
    # All preservation proofs are re-read after delivery, not just their filenames.
    inherited=load(HERE/'inherited-software-and-preservation-v1.json');preserved=0
    for pop in inherited['preservation']['populations']:
        exact(pop['inventory'])
        for p in load(ROOT/pop['inventory']['path'])['files']:exact(p);preserved+=1
    for p in load(ROOT/inherited['preservation']['original47_store_inventory']['path'])['files']:exact(p);preserved+=1
    for c in candidates:
        m=pub.physical.scanner(c['scanner']);assert m.paths()==[p['path'] for p in c['members']]
        for p in c['members']:exact(p)
    source_lock();payload();assert not git('diff','--cached','--name-only')
    primary=[pin(HERE/n) for n in ['payload-authentication-v1.json','focused-parent-comparison-v1.json','python-reconciliation-v2.json','inherited-software-and-preservation-v1.json','owner-authority-successor-v1.md','readiness-v2.json','source-and-command-continuity-v1.json']]
    primary.append(pin('docs/architecture/batch-04A-47-recovered-SPC23-owner-review-v2.md'))
    d=dict(status=STATUS,utc=utc(),archive=pin(archive),physical_members=members,primary_reports=primary,candidate_scans=[dict(c,scope='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS') for c in candidates],physical_scans=physical,adverse_controls=adverse,neutral_reviewer_replay=neutral,independent_reader=verified,postdelivery_git=dict(branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked=git('ls-files','--others','--exclude-standard').decode().splitlines()),postdelivery_candidate_bytes_and_membership_exact=True,postdelivery_preservation=dict(listed_paths_rehashed=preserved,all_exact=True,earlier_full_listed_readback=pin(HERE/'inherited-software-and-preservation-v1.json'),continuous_physical_identity='NOT_VERIFIED',W3_union='INHERITED_QUALIFIED'),v1_archive=pin(ZIP1),v1_receipt=pin(RECEIPT1),payload_postread=payload(),payload_excluded_from_zip=True,payload_in_original_candidate_scopes=True,historical_evidence_writes=0,native='NOT_RUN',gates=original.GATES)
    receipt=folder/'external-delivery-receipt-v2.json';save(receipt,d)
    for phase in ['13','14']:
        m=pub.physical.scanner(phase);m.ROOT=pub.ProbeRoot(receipt.read_text(encoding='utf-8'));f,_=m.scan(['external-delivery-receipt-v2.json']);assert not f
    assert reader.verify(archive,receipt)['status']=='PASS_EXTERNAL_AND_SEMANTIC'
    save(folder/'receipt-independent-readback-v1.json',dict(archive=pin(archive),receipt=pin(receipt),candidate_membership_and_bytes_exact=True,index=pin('.git/index'),adverse_controls=len(adverse),owner_positive_controls=sum(len(r['controls']['positive']) for r in candidates+physical),owner_clean_negatives=sum(r['controls']['negative_clean'] for r in candidates+physical),v1_archive=pin(ZIP1),v1_receipt=pin(RECEIPT1)))
    print(json.dumps(dict(archive=pin(archive),receipt=pin(receipt),adverse_controls=len(adverse),candidate_scopes=[c['candidates'] for c in candidates],physical_members=len(members),status=STATUS)),flush=True)
if __name__=='__main__':
    try:publish()
    except Exception:
        import traceback
        save(PRIVATE/('publication-failure-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S')+'.json'),dict(traceback=traceback.format_exc()))
        print('Publication STOP: protected failed attempt retained',flush=True);raise SystemExit(2)
