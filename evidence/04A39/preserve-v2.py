"""Qualified incremental preservation; never describe this as a second whole W3 private audit."""
from common39 import *
source_lock()
prior=load(W3/'final/delivery-receipt-v1.json')
assert prior['private_file_union']==37484 and prior['physical_archive_union']==154
checked=[]
for name in ['final/delivery-receipt-v1.json','preservation/final-additive-pins-v1.json','final/postseal-source-lock-v1.json']:
    checked.append(pin(W3/name))
live_only=load(W3/'entry/authority-crosswalk-v3.json')['actual_package_installer_support_files']
for row in live_only:assert pin(row['path'])==row
source=load(HERE/'entry/source-preimages-v1.json')['files']
transitions=[]
for row in source:
    now=pin(row['path'])
    if now!=row:
        assert row['path'] in ['BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md']
        before=PRIVATE/'source-preimages-v1'/row['path'];assert pin(before)['sha256']==row['sha256']
        transitions.append(dict(before=row,after=now,archived_preimage=pin(before),authorization='04A39 two new test inclusions' if row['path'].endswith('.vbproj') else 'Owner-authorized living Master Plan2.15 status update; exact2.14 preserved.'))
reconciliation=load(HERE/'entry/line-ending-reconciliation-v1.json')
save('preservation/inherited-transition-v1.json',dict(inherited_W3_file_pins=37484,inherited_archive_records=154,
    inherited_scope='Qualified W3 full owner-machine audit and additive final pins inherited, not freshly rehashed whole union or all uploaded.',
    original_anchors=checked,live_only18=live_only,new_authorized_source_transitions=transitions,
    owner_authorized_EOL_transition=pin(HERE/'entry/line-ending-reconciliation-v1.json'),
    historical_original_and_negative_evidence='No overwrite, cleanup, deletion, regeneration or new original execution. HistoricTestResults never traversed by this incremental audit.',
    retained_restrictions=['278 source-lineage pins','100 mixed-model captures','16 owner documents','41 original UDF snapshots','W1/W2/W3 stopped attempts and nonpasses'],
    physical_limit='No assertion that every inherited private/archive byte was newly verified or uploaded under39. Exact18 files and all current production locks read back; task-local new outputs pinned separately.'))
rows=[]
for base in [HERE,PRIVATE]:
    for path in sorted(base.rglob('*')):
        if not path.is_file() or '__pycache__' in path.parts:continue
        # Exclude this packet's currently open command logs; its completed outer receipt is pinned by final delivery.
        if any(part.startswith('a39v1-packet-preservation-v2') for part in path.parts):continue
        stat=path.stat();row=pin(path);row.update(device=stat.st_dev,file_id=stat.st_ino)
        rows.append(row)
assert len({x['path'] for x in rows})==len(rows)
save('preservation/new-pins-v1.json',dict(files=rows,count=len(rows),
    inherited_baseline_file_pins=37484,inherited_archive_records=154,
    scope='New39 namespace and new isolated private execution root only. Additive count is not a new full physical audit of inherited37484.',
    self_and_later_bookkeeping='Excluded from recursive self-hash; final delivery pins completed manifest and post-publication receipts.'))
source_lock();print('New39 pins',len(rows),flush=True)
