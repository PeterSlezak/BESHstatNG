"""Incremental physical preservation with explicit authorized source transitions."""
from common40_v3 import *
import collections
lock();physical_drift=[];checkpoint=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files'];assert len(checkpoint)==468
for r in checkpoint:
    assert pin(r['path'])=={k:r[k] for k in ('path','bytes','sha256')}
    stat=(ROOT/r['path']).stat()
    if (stat.st_dev,stat.st_ino)!=(r['device'],r['file_id']):physical_drift.append(dict(path=r['path'],authority='468_CHECKPOINT',before=[r['device'],r['file_id']],after=[stat.st_dev,stat.st_ino],bytes_exact=True))
    copied=ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint'/r['path'];assert copied.stat().st_size==r['bytes'] and pin(copied)['sha256']==r['sha256']
records={};authorities=[]
for path,field in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
    d=load(ROOT/path);authorities.append(pin(path))
    for r in d[field]:
        if r['path'] in records:assert all(records[r['path']][k]==r[k] for k in ('bytes','sha256'))
        records[r['path']]=r
for r in records.values():
    assert pin(r['path'])=={k:r[k] for k in ('path','bytes','sha256')},'PROTECTED_RECORD_CHANGED'
    if 'device' in r:
        st=(ROOT/r['path']).stat()
        if (st.st_dev,st.st_ino)!=(r['device'],r['file_id']):physical_drift.append(dict(path=r['path'],authority='PROTECTED_UNION',before=[r['device'],r['file_id']],after=[st.st_dev,st.st_ino],bytes_exact=True))
before=load(HERE/'entry/source-preimages-v3.json')['files'];assert len(before)==1353
freeze=load(HERE/'matrix/source-freeze-v1.json');assert runner().source_pins()==freeze['source_pins']
delta=[];snap=PRIVATE/'source-preimages-main';snap.mkdir()
for r in before:
    now=pin(r['path'])
    if now!=r:
        data=git('show',MAIN+':'+r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256']
        p=snap/r['path'];p.parent.mkdir(parents=True,exist_ok=True);p.open('xb').write(data)
        delta.append(dict(before=r,after=now,preserved_preimage=pin(p)))
assert len(delta)==16 and not any(r['before']['path'].startswith('BESHStat.Core/') for r in delta)
register=load(ROOT/'evidence/04A39/register/defects-v7.json');reader=module('register40structural',ROOT/'evidence/04A39/verify-register-v5.py')
counts=reader.verify(ROOT,register,check_files=False);assert counts['canonical_entries']==1169 and counts['original_obligations']==1514 and counts['restricted_controls']==802
assert len(register['crosswalk'])==2967 and len(register['current_source_pins'])==33
transitions=[]
for r in register['current_source_pins']:
    now=pin(r['path'])
    if now!=r:
        entry=next(x for x in before if x['path']==r['path']);assert entry==r
        transitions.append(dict(historical=r,current=now,reason='AUTHORIZED40_BEHAVIOR_PRESERVING_SOURCE_DELTA'))
privacy=load(ROOT/'evidence/04A39/correction-v1/privacy/focused-finding-reconciliation.json');assert (privacy['per_parent_findings'],privacy['original_snapshot_retained'],privacy['new_since_snapshot'])==(1941,1599,342)
diag=load(ROOT/'evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json')['changes'];assert len(diag)==len({r['stable_event_id'] for r in diag})==54
assert load(ROOT/'evidence/04A39/map/method-policy-map-v1.json')['focus_members']==192
old_archives=load(ROOT/'evidence/04A39/physical-witness-v1/entry.json')['input_archives']
for r in old_archives:assert pin(r['path'])==r
save(PRIVATE/'physical-file-id-drift-v2.json',dict(records=physical_drift,disposition='BYTES_IDENTICAL;PHYSICAL_CONTINUITY_NOT_VERIFIED;NO_REBASELINE'))
save('preservation/historical-readback-v2.json',dict(original_NOT_READY_files=468,original_bytes_exact=True,physical_identity_unchanged=not physical_drift,physical_identity_drift_count=len(physical_drift),physical_identity_drift=pin(PRIVATE/'physical-file-id-drift-v2.json'),physical_disposition='NOT_VERIFIABLE_FILE_ID_DRIFT;OWNER_PROVENANCE_REVIEW_REQUIRED',protected_unique_paths=len(records),protected_bytes=sum(r['bytes'] for r in records.values()),authorities=authorities,
    source_entry_identities=1353,source_unchanged=1353-len(delta),authorized_source_transitions=delta,register=counts,crosswalk_rows=2967,register_source_pins=33,register_current_source_transitions=transitions,
    canonical_register=pin(ROOT/'evidence/04A39/register/defects-v7.json'),historical_policy_members=192,current_policy_members=200,original54diagnostics='UNCHANGED_FAIL_DIAGNOSTICS_NOT_WAIVED',
    privacy_history=dict(current_each=1941,earlier_each=1599,added_each=342,W3='TWO_ERRORS_UNWAIVED'),archives=old_archives,
    qualified_inherited_W3=dict(files=37484,archives=154,fresh_full_union_audit=False),no_TestResults_recursive_search=True,no_original_execution=True,gates=GATES))
print('Preserved468 checkpoint files;',len(records),'protected exact paths;1353 source entry identities with16 authorized transitions;1169/1514/802 register',flush=True)
