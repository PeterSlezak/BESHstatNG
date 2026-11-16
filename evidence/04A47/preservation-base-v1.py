from common47 import *
import stat,collections
lock();entry=load(PRIVATE/'tracked-entry-inventory-v1.json')['files'];allowed={'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'}
for i,r in enumerate(entry):
 if r['path'] in allowed:
  old=PRIVATE/'source-preimages'/r['path'] if r['path'].endswith('.vbproj') else ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.23.md'
  assert pin(old)['sha256']==r['sha256'] and pin(old)['bytes']==r['bytes']
 else:exact(r)
print('All',len(entry),'entry tracked bytes reconciled',flush=True)
history=load(ROOT/'evidence/04A40/preservation/historical-readback-v4.json');endings={r['original']['path']:r for r in history['owner_line_ending_transitions']}
checkpoint=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files'];assert len(checkpoint)==468
for r in checkpoint:
 exact(dict(r,path='.test-rationalization-04A39-correction-v1.tmp/checkpoint/'+r['path']))
 if r['path'] in endings:
  exact(endings[r['path']]['committed']);exact(endings[r['path']]['exact_original_copy'])
 else:exact(r)
records={}
for p,field in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
 for r in load(ROOT/p)[field]:
  if r['path'] in records:assert all(records[r['path']][k]==r[k] for k in ['bytes','sha256'])
  records[r['path']]=r
assert len(records)==8984
for r in records.values():exact(endings[r['path']]['committed'] if r['path'] in endings else r)
for r in history['archives']:exact(r)
raw40=load(ROOT/'evidence/04A40/preservation/restricted-new-raw-inventory-v1.json');exact(raw40['private_inventory']);rows=load(ROOT/raw40['private_inventory']['path'])['files'];assert len(rows)==1906
replaced={r['original']['path']:r for r in load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')['artifacts']}
for r in rows:
 if r['path'] in replaced:
  p=replaced[r['path']];assert not (ROOT/r['path']).exists();exact(p['protected_copy']);exact(p['representation'])
  assert not (ROOT/p['protected_copy']['path']).stat().st_mode&stat.S_IWRITE
 else:exact(r)
populations=[]
for b in ['41','42','43','44']:
 p=ROOT/('.test-rationalization-04A'+b+'.tmp/new-raw-inventory-v1.json');rows=load(p)['files']
 for r in rows:exact(r)
 current=[pin(f) for f in sorted(p.parent.rglob('*')) if f.is_file()]
 inventory=PRIVATE/('prior'+b+'-whole-store-readback-v1.json');save(inventory,dict(files=current))
 populations.append(dict(batch=b,prior_manifest=pin(p),prior_exact=len(rows),current_whole_store_files=len(current),current_inventory=pin(inventory)))
 print('Raw'+b,len(rows),'prior pins exact;',len(current),'whole-store files read',flush=True)
for p in [ROOT/'.test-rationalization-04A44.tmp/publication/postdelivery-full-preservation-v1.json',ROOT/'.test-rationalization-04A44.tmp/publication/postdelivery-git-preservation-v1.json']:exact(pin(p))
canonical=load(ROOT/'evidence/04A39/register/defects-v7.json');counts=module('register47_verify',ROOT/'evidence/04A39/verify-register-v5.py').verify(ROOT,canonical,check_files=False)
assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'],len(canonical['crosswalk']))==(1169,1514,802,2967)
# Accepted44 source lineage is independently pinned in live tracked evidence and protected
# entry copies; every current production file equals authenticated main byte-for-byte.
freeze44=load(ROOT/'evidence/04A46/matrix/source-freeze-v1.json')
for p,h in freeze44['source_pins'].items():
 if p=='BESHStatNG.Test/BESHStatNG.Tests.vbproj':continue
 assert pin(p)['sha256']==h,'ACCEPTED44_SOURCE_CHANGED'
for r in load(ROOT/'evidence/04A44/entry/source-preimages-v1.json')['files']:
 copy=ROOT/'.test-rationalization-04A44.tmp/source-preimages'/r['path'];assert pin(copy)['sha256']==r['sha256']
save('preservation/readback-base-v1.json',dict(status='FRESH_LISTED_BYTE_READBACK_PASS',entry_tracked=len(entry),allowed_changes=sorted(allowed),all_other_entry_bytes_exact=True,checkpoint_copies=468,checkpoint_live_exact=464,checkpoint_authorized_newline=4,historical_paths=8984,raw40_paths=1906,raw_populations=populations,accepted44_source_freeze=pin('evidence/04A44/matrix/source-freeze-v1.json'),accepted44_source_lineage_exact=True,original_source_history=1353,source_history_proof='Accepted44 full source lineage dossier remains byte exact via tracked-entry readback;its current production endpoints and protected preimages freshly rehashed. Not a claim every historical transition was re-executed.',historical_file_identity_drifts=788,continuous_physical_identity='NOT_VERIFIED',W3_union=dict(paths=37484,archives=154,fresh_full_union_readback=False,inherited_qualified=True),canonical_counts=counts,mappings=2967,original_privacy_findings_per_parent=1941,W3_privacy_errors='RETAINED_UNWAIVED',old_Debug_FAIL=14,old_04A08_scope_FAIL='RETAINED_UNWAIVED',accepted41_Release_FAIL='RETAINED_OUTSIDE_SELECTED_SET',accepted42_failed_first_freeze='RETAINED',accepted44_raw_publication_failure='RETAINED',gates=GATES))
print('Preservation468/8984/raw40-44/current main PASS;physical continuity/W3 full union NOT_VERIFIED',flush=True)
