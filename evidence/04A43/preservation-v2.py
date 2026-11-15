from common43 import *
import stat
lock();freeze=load(HERE/'matrix/source-freeze-v1.json');entry=load(HERE/'entry/source-preimages-v1.json')
for p,h in freeze['source_pins'].items():assert pin(p)['sha256']==h
exact(freeze['plan']);exact(freeze['plan_snapshot'])
allowed={r['after']['path']:r['after'] for r in freeze['source_delta']};allowed[freeze['plan']['path']]=freeze['plan']
tracked=load(PRIVATE/'tracked-entry-inventory-v1.json')['files'];assert len(tracked)==14634
for i,r in enumerate(tracked):
    if r['path'] in allowed:
        exact(allowed[r['path']])
        copy=PRIVATE/'source-preimages'/r['path'] if r['path']!=freeze['plan']['path'] else ROOT/freeze['plan_snapshot']['path']
        assert pin(copy)['sha256']==r['sha256'] and pin(copy)['bytes']==r['bytes']
    else:exact(r)
    if (i+1)%4000==0:print('Tracked entry preservation',i+1,'/',len(tracked),flush=True)
historical=load(ROOT/'evidence/04A40/preservation/historical-readback-v4.json')
lineendings={r['original']['path']:r for r in historical['owner_line_ending_transitions']};assert len(lineendings)==4
checkpoint=load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files'];assert len(checkpoint)==468
drift=0
for r in checkpoint:
    exact(dict(r,path='.test-rationalization-04A39-correction-v1.tmp/checkpoint/'+r['path']))
    if r['path'] in lineendings:
        t=lineendings[r['path']];exact(t['committed']);exact(t['exact_original_copy'])
        assert (ROOT/t['exact_original_copy']['path']).read_bytes().replace(b'\r\n',b'\n')==(ROOT/r['path']).read_bytes().replace(b'\r\n',b'\n')
    else:exact(r)
    st=(ROOT/r['path']).stat();drift+=((st.st_dev,st.st_ino)!=(r['device'],r['file_id']))
records={};authorities=[]
for p,field in [('evidence/04A39/preservation/new-pins-v1.json','files'),('evidence/04A39/preservation/post-publication-additions-v1.json','added'),('.test-rationalization-04A39-correction-v1.tmp/private-pins-after-postseal.json','files')]:
    authorities.append(pin(p))
    for r in load(ROOT/p)[field]:
        if r['path'] in records:assert all(records[r['path']][k]==r[k] for k in ['bytes','sha256'])
        records[r['path']]=r
assert len(records)==8984
for i,r in enumerate(records.values()):
    if r['path'] in lineendings:exact(lineendings[r['path']]['committed'])
    else:exact(r)
    if 'device' in r:
        st=(ROOT/r['path']).stat();drift+=((st.st_dev,st.st_ino)!=(r['device'],r['file_id']))
    if (i+1)%3000==0:print('Historical protected pins',i+1,'/',len(records),flush=True)
raw_boundary=load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')
resolved={r['original']['path']:r for r in raw_boundary['artifacts']}
for r in resolved.values():
    assert not (ROOT/r['original']['path']).exists();exact(r['protected_copy']);exact(r['representation'])
    assert not ((ROOT/r['protected_copy']['path']).stat().st_mode & stat.S_IWRITE)
raw_authority=load(ROOT/'evidence/04A40/preservation/restricted-new-raw-inventory-v1.json');exact(raw_authority['private_inventory'])
newraw=load(ROOT/raw_authority['private_inventory']['path'])['files'];assert len(newraw)==1906
for r in newraw:
    if r['path'] in resolved:
        cp=resolved[r['path']]['protected_copy'];assert (cp['bytes'],cp['sha256'])==(r['bytes'],r['sha256']);exact(cp)
    else:exact(r)
old_history=load(ROOT/'evidence/04A40/entry/source-preimages-v3.json')['files'];assert len(old_history)==1353
transitions={r['before']['path']:r for r in historical['authorized_source_transitions']};assert len(transitions)==16
initial={r['path']:r for r in tracked}
freeze41=load(ROOT/'evidence/04A41/matrix/source-freeze-v1.json');freeze42=load(ROOT/'evidence/04A42/matrix/source-freeze-v2.json')
after41={r['before']['path']:r['after'] for r in freeze41['source_delta']};after41[freeze41['plan']['path']]=freeze41['plan']
after42={r['before']['path']:r['after'] for r in freeze42['source_delta']};after42[freeze42['plan']['path']]=freeze42['plan']
owner41={r['sealed']['path']:r for r in load(ROOT/'evidence/04A42/entry/authentication-v1.json')['merged41_newline_transitions']}
owner42={r['sealed']['path']:r for r in load(HERE/'entry/authentication-v1.json')['merged42_newline_transitions']}
for r in old_history:
    p=r['path'];expected=transitions[p]['after'] if p in transitions else r
    if p in after41:expected=after41[p]
    if p in owner41:
        t=owner41[p];assert expected==t['sealed'];exact(t['protected_exact']);expected=t['current']
    if p in after42:expected=after42[p]
    if p in owner42:
        t=owner42[p];assert expected==t['sealed'];exact(t['protected_exact']);expected=t['current']
    assert initial[p]==expected,'UNAUTHENTICATED_SOURCE_HISTORY_ENTRY'
    if p in transitions:exact(transitions[p]['preserved_preimage'])
for r in load(ROOT/'evidence/04A41/entry/authentication-v1.json')['newline_only_owner_checkout_transitions']:
    exact(r['current_owner_checkout']);exact(r['exact_original_copy'])
for r in historical['archives']:exact(r)
for r in load(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json')['files']:exact(r)
old41_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A41.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited41-current-raw-inventory-v1.json',dict(files=old41_files,count=len(old41_files),prior_inventory_exact=6810,scope='Fresh41 store recount;prior6810 and later accepted receipt separately pinned;no physical continuity claim.'))
for table in [owner41,owner42]:
    for r in table.values():assert initial[r['current']['path']]==r['current'];exact(r['protected_exact'])
raw42=load(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json')
for r in raw42['files']:exact(r)
old42_files=[pin(p) for p in sorted((ROOT/'.test-rationalization-04A42.tmp').rglob('*')) if p.is_file()]
save(PRIVATE/'inherited42-current-raw-inventory-v1.json',dict(files=old42_files,count=len(old42_files),prior_pins_exact=len(raw42['files']),scope='Fresh42 store recount,not a claim later additions had original pre43 ledger. Prior11457 and accepted42 final/postdelivery receipts separately authenticated.'))
register=load(ROOT/'evidence/04A39/register/defects-v7.json')
counts=module('preserved_register41',ROOT/'evidence/04A39/verify-register-v5.py').verify(ROOT,register,check_files=False)
assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'],len(register['crosswalk']))==(1169,1514,802,2967)
for name,count,field in [('evidence/04A39/correction-v1/diagnostics/changed-54-crosswalk-v2.json',54,'changes'),('evidence/04A40/python/diagnostic-dispositions-v3.json',32,'rows')]:assert len(load(ROOT/name)[field])==count
js=[r for r in tracked if r['path'].endswith(('.js','.mjs','.cjs'))]
for r in js:exact(r)
save('preservation/readback-v1.json',dict(raw42_prior_inventory=pin(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json'),raw42_prior_pins_exact=len(raw42['files']),current42_raw_recount=pin(PRIVATE/'inherited42-current-raw-inventory-v1.json'),current42_raw_count=len(old42_files),current41_raw_count=len(old41_files),sealed42_newline_recoveries=len(owner42),raw41_inventory=pin(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json'),raw41_prior_pins_exact=6810,current41_raw_recount=pin(PRIVATE/'inherited41-current-raw-inventory-v1.json'),accepted42_delivery=pin(ROOT/'.test-rationalization-04A42.tmp/publication/external-delivery-receipt-v1.json'),accepted41_sealed_newline_recoveries=len(owner41),status='EXACT_LISTED_BYTE_PRESERVATION_AND_SOURCE_HISTORY_PASS',entry_tracked_files=len(tracked),authorized_changed_tracked_paths=sorted(allowed),all_other_tracked_files_exact=True,entry_source_pins=len(entry['source_pins']),current_source_pins=len(freeze['source_pins']),historical_source_entry_identities=1353,accepted40_source_transitions=16,accepted41_runtime_source_transitions=10,accepted42_runtime_source_transitions=8,new43_existing_runtime_transitions=3,new43_TestProject_Compile_registration=1,new43_Common_source_files=1,new43_tests=2,new43_plan_transition=1,checkpoint=dict(files=468,live_exact=464,previous_authorized_newline_renderings=4,exact_original_copies=468),historical_protected=dict(paths=8984,bytes=sum(r['bytes'] for r in records.values()),authorities=authorities),newer40_raw=dict(paths=1906,bytes=sum(r['bytes'] for r in newraw),authority=raw_authority['private_inventory']),owner_checkout_40_newline_differences=31,exact_sealed40_original_copies_verified=True,raw40_boundary='TWO_RAW_ORIGINALS_PROTECTED_OUTSIDE_GIT_CANDIDATE;SANITIZED_SUCCESSORS_EXACT',file_id_drift_records=drift,previous_file_id_drifts=788,continuous_physical_identity='NOT_VERIFIED;NOT_REBASELINED',W3_union=dict(paths=37484,archives=154,inherited_qualified=True,fresh_full_union_audit=False),register=counts,mappings=2967,old54_and32_diagnostic_records_retained=True,JavaScript_files_exact=len(js),Node=dict(outcome='INHERITED_916_PASS;NOT_RERUN',baseline=pin(ROOT/'evidence/04A40/python/Node-fresh-identities-v3.json'),reason='No JS/Office contract source or existing DTO/host-facing compiled method changed;fresh managed Office contracts passed. No new Office/WASM capability.'),no_recursive_TestResults_walk=True,gates=GATES))
print('Preservation PASS:14634 tracked entry,468 checkpoint,8984 protected,1906 raw40,6810 raw41,11457 raw42 and complete41/42 recount;ID continuity NOT_VERIFIED',flush=True)
