"""Rehash existing receipts and protected populations; no managed rerun."""
import sys,stat,zipfile
from pathlib import Path
ROOT=Path.cwd()
sys.path.insert(0,str(ROOT/'evidence/04A47/recovery-v2'))
from recovery import *
def inherited():
    source_lock();payload();receipt=load(RECEIPT1)
    with zipfile.ZipFile(ZIP1) as z:
        for n in z.namelist():
            if n=='.review-index.json':continue
            if n=='planning/BESHStatNG_CrossPlatform_MasterPlan.md':p=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.24.md'
            else:p=ROOT/n
            assert p.read_bytes()==z.read(n),'SEALED_V1_PAYLOAD_CHANGED:'+n
    # Every entry tracked byte, with only the earlier47 and owner-approved ignore transitions.
    rows=load(ROOT/'.test-rationalization-04A47.tmp/tracked-entry-inventory-v1.json')['files']
    for r in rows:
        n=r['path']
        if n=='BESHStatNG.Test/BESHStatNG.Tests.vbproj':p=ROOT/'.test-rationalization-04A47.tmp/source-preimages'/n;assert all(pin(p)[k]==r[k] for k in ['bytes','sha256'])
        elif n=='planning/BESHStatNG_CrossPlatform_MasterPlan.md':p=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.23.md';assert all(pin(p)[k]==r[k] for k in ['bytes','sha256'])
        elif n=='.gitignore':b=git('show','HEAD:.gitignore');assert len(b)==r['bytes'] and sha(b)==r['sha256'];assert pin(n)['sha256']=='95b233c640d30104d3ea71bad0fdb09ba7a7d4a8e7dcef34e8cc370a0e29346d'
        else:exact(r)
    history=load(ROOT/'evidence/04A40/preservation/historical-readback-v4.json');endings={r['original']['path']:r for r in history['owner_line_ending_transitions']}
    cp=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files'];assert len(cp)==468
    for r in cp:
        exact(dict(r,path='.test-rationalization-04A39-correction-v1.tmp/checkpoint/'+r['path']))
        if r['path'] in endings:exact(endings[r['path']]['committed']);exact(endings[r['path']]['exact_original_copy'])
        else:exact(r)
    records={}
    for p,k in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
        for r in load(ROOT/p)[k]:
            if r['path'] in records:assert all(records[r['path']][x]==r[x] for x in ['bytes','sha256'])
            records[r['path']]=r
    assert len(records)==8984
    for r in records.values():exact(endings[r['path']]['committed'] if r['path'] in endings else r)
    for r in history['archives']:exact(r)
    raw40=load(ROOT/'evidence/04A40/preservation/restricted-new-raw-inventory-v1.json');exact(raw40['private_inventory']);raw=load(ROOT/raw40['private_inventory']['path'])['files'];assert len(raw)==1906
    replacements={r['original']['path']:r for r in load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')['artifacts']}
    for r in raw:
        if r['path'] in replacements:
            p=replacements[r['path']];assert not (ROOT/r['path']).exists();exact(p['protected_copy']);exact(p['representation']);assert not (ROOT/p['protected_copy']['path']).stat().st_mode&stat.S_IWRITE
        else:exact(r)
    populations=[];preserve=load(OLD/'preservation/readback-v1.json')
    for row in preserve['raw_populations']+preserve['accepted45_46_populations']:
        exact(row['current_inventory']);files=load(ROOT/row['current_inventory']['path'])['files']
        for r in files:exact(r)
        populations.append(dict(batch=row['batch'],rehashed=len(files),inventory=row['current_inventory'],exact=True))
    # Snapshot every original47 private file, including raw failures, journals and v1 delivery.
    oldfiles=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A47.tmp').rglob('*')) if p.is_file()]
    save(PRIVATE/'original47-whole-store-v2.json',dict(files=oldfiles))
    # Original reader authenticates individual managed identities, all failure fingerprints,
    # 268 ABI item hashes and every frozen compatibility original/current SHA/length pair.
    oldreader=original.module('inherited_reader47',OLD/'review_reader.py');result=oldreader.verify(ZIP1,RECEIPT1)
    software=load(OLD/'final/software-verification-v2.json')
    reports=[pin(OLD/p) for p in ['final/software-verification-v2.json','build-and-tests/Debug-identities-v1.json','build-and-tests/Release-identities-v1.json','build-and-tests/Release-trace-disposition-v2.json','abi-cil/UI-and-attribution-v1.json','abi-cil/accepted46-exact-v1.json','compatibility/Debug-v1.json','compatibility/Release-v1.json','final/source-review-v1.json','register/interim-delta-v2.json']]
    save('inherited-software-and-preservation-v1.json',dict(classification='INHERITED_VERIFIED_BY_RECEIPT_AND_CURRENT_LISTED_BYTE_READBACK;NOT_REEXECUTED',Debug=software['Debug'],Release=software['Release'],full_Release='NOT_RUN',exports_per_configuration=268,compatibility_per_configuration=dict(total=29424,PRE_EDIT_FROZEN=28966,LATE_ARCHIVED_ENTRY_SUPPLEMENT=458),methods=101,new_managed_tests=141,observer_cases_per_configuration=147,Node='916_PASS_INHERITED_NOT_RERUN',strict_profile_MkDocs=[1,1,0],strict_profile='NONPASS',source_freeze=pin(OLD/'matrix/source-freeze-v1.json'),original_six_sources_and_project_exact=True,compiled_evidence_and_CIL='SEALED_RECEIPTS_AUTHENTICATED;NO_NEW_BUILD',reports=reports,reader=result,preservation=dict(entry_tracked=len(rows),checkpoint_copies=len(cp),historical_paths=len(records),raw40=len(raw),populations=populations,original47_store_files=len(oldfiles),original47_store_inventory=pin(PRIVATE/'original47-whole-store-v2.json'),historical_drifts_retained=788,continuous_physical_identity='NOT_VERIFIED',W3_union='INHERITED_QUALIFIED_NOT_FRESH_FULL_UNION'),canonical_counts=dict(issues=1169,obligations=1514,restricted_controls=802,mappings=2967),discharges=0,v1_archive=pin(ZIP1),v1_receipt=pin(RECEIPT1),native='NOT_RUN',gates=original.GATES))
    print('Inherited managed/ABI/compatibility receipts and all listed protected populations authenticated',flush=True)
if __name__=='__main__':inherited()
