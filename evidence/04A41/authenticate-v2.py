from common41 import *
import re,stat,zipfile
assert git('branch','--show-current').decode().strip()=='main'
assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()==PRIOR
assert not git('diff','--name-only') and not git('diff','--cached','--name-only') and not git('diff','--check')
PRIVATE.mkdir(exist_ok=True)
receipt_path=ROOT/'.test-rationalization-04A40.tmp/owner-authorized-candidate-v3/external-delivery-receipt-v3.json'
assert pin(receipt_path)['sha256']=='3f6319067baea846b8f3a29a8d7dd376c81bb4fd44a245c1df7461f2d3c1000c'
receipt=load(receipt_path);exact(receipt['archive'])
assert receipt['status']=='READY_FOR_OWNER_04A40_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
reader=module('accepted40_archive_reader',ROOT/'evidence/04A40/owner-authorized-candidate-v3/readback.py')
assert reader.verify(ROOT/receipt['archive']['path'])['SHA_size_CRC']=='PASS'
newline=[];originals={r['path']:r for report in receipt['candidate_privacy']['final_full_actual_candidate_reports'] for r in report['members']}
for p,old in originals.items():
    current=pin(p)
    if current==old:continue
    blob=git('show','HEAD:'+p);live=(ROOT/p).read_bytes()
    assert live.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n'),'NON_NEWLINE_ACCEPTED40_CHANGE'
    candidates=[blob,blob.replace(b'\n',b'\r\n'),blob.rstrip(b'\n').replace(b'\n',b'\r\n')+b'\n']
    matches=[b for b in candidates if len(b)==old['bytes'] and sha(b)==old['sha256']]
    assert matches,'SEALED_ORIGINAL_NOT_RECOVERED'
    dst=PRIVATE/'sealed40-original-bytes-v2'/p;dst.parent.mkdir(parents=True,exist_ok=True)
    with dst.open('xb') as f:f.write(matches[0])
    dst.chmod(stat.S_IREAD)
    newline.append(dict(original_sealed=old,current_owner_checkout=current,exact_original_copy=pin(dst),scope='Existing clean owner commit/checkout representation, before41; no historical file rewritten by41.',recovery='Verified canonical Git blob plus deterministic newline rendering; exact original SHA/size required.'))
assert len(newline)==31
raw_cross=load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')
for row in raw_cross['artifacts']:
    assert not (ROOT/row['original']['path']).exists()
    exact(row['protected_copy']);exact(row['representation'])
    assert not ((ROOT/row['protected_copy']['path']).stat().st_mode & stat.S_IWRITE)
register_pointer=load(ROOT/'evidence/04A39/register/current-v5.json');exact(register_pointer['canonical'])
register=load(ROOT/register_pointer['canonical']['path'])
counts=module('accepted39_register',ROOT/'evidence/04A39/verify-register-v5.py').verify(ROOT,register,check_files=False)
assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'],len(register['crosswalk']))==(1169,1514,802,2967)
plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';text=plan.read_text(encoding='utf-8-sig')
ids=re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',text,re.M)
assert len(ids)==len(set(ids))==226 and '04A.41' in ids
tracked=git('ls-files').decode().splitlines();inventory=[]
for i,p in enumerate(tracked):
    inventory.append(pin(p))
    if (i+1)%4000==0:print('Tracked baseline hashes',i+1,'/',len(tracked),flush=True)
save(PRIVATE/'tracked-entry-inventory-v1.json',dict(files=inventory,git_index_entries=git('ls-files','--stage').decode().splitlines()))
sources=module('source_inventory41',ROOT/'scripts/test_suite_rationalization.py').source_pins()
for p,h in sources.items():
    assert pin(p)['sha256']==h
    dst=PRIVATE/'source-preimages'/p;dst.parent.mkdir(parents=True,exist_ok=True)
    with dst.open('xb') as f:f.write((ROOT/p).read_bytes())
for p in ['planning/BESHStatNG_CrossPlatform_MasterPlan.md','.git/index']:
    dst=PRIVATE/'entry-pins'/p;dst.parent.mkdir(parents=True,exist_ok=True)
    with dst.open('xb') as f:f.write((ROOT/p).read_bytes())
    dst.chmod(stat.S_IREAD)
save('entry/source-preimages-v1.json',dict(source_pins=sources,files=[pin(p) for p in sources],tracked_inventory=pin(PRIVATE/'tracked-entry-inventory-v1.json'),source_copy_root='.test-rationalization-04A41.tmp/source-preimages',plan=pin(plan),plan_ids=ids))
save('entry/authentication-v1.json',dict(status='AUTHENTICATED_OWNER_ACCEPTED40_MERGED_MAIN',utc=utc(),entry_branch='main',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,ancestry=git('log','-6','--format=%H %P').decode().splitlines(),index=pin('.git/index'),preexisting_tracked_edits=[],preexisting_staged_edits=[],preexisting_untracked_owner_edits=[],own_initial_evidence=['evidence/04A41/common41.py','evidence/04A41/authenticate-v1.py'],diff_check_exit=0,accepted40_receipt=pin(receipt_path),accepted40_archive=receipt['archive'],accepted40_owner_review=pin(ROOT/'evidence/04A40/owner-authorized-candidate-v3/owner-review-v3.md'),newline_only_owner_checkout_transitions=newline,raw_boundary_verified=True,canonical_register=register_pointer['canonical'],register_counts=counts,mappings=2967,plan_revision='2.17',plan_status_stale=True,owner_authority='Current task explicitly accepts40 with restrictions, reports merged main, authorizes41 only; this overrides the stale plan header, not the deferred-defect charter.',plan_ids=226,gates=GATES))
print('Authenticated merged main,31 sealed-original newline recoveries,226 plan IDs,canonical register,protected raw boundary;source freeze complete',flush=True)
