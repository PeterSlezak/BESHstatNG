from common42 import *
import stat,re,zipfile
lock_branch=git('branch','--show-current').decode().strip();assert lock_branch=='codex/04A-42'
assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()==PRIOR
assert not git('diff','--name-only') and not git('diff','--cached','--name-only') and not git('diff','--check')
PRIVATE.mkdir(exist_ok=True)
receipt=ROOT/'.test-rationalization-04A41.tmp/publication/external-delivery-receipt-v1.json'
assert pin(receipt)['sha256']=='f920b13e8b00ccdc1dfb6d4455fc9d374969937e1b779230cf500e061ccb9eab'
d=load(receipt);exact(d['archive']);reader=module('reader41',ROOT/'evidence/04A41/review_reader.py');assert reader.verify(ROOT/d['archive']['path'])['SHA_size_CRC']=='PASS'
transitions=[]
originals={r['path']:r for report in d['full_actual_candidate_scans'] for r in report['members']}
for p,old in originals.items():
    if pin(p)==old:continue
    blob=git('show','HEAD:'+p);live=(ROOT/p).read_bytes()
    assert live.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n'),'MERGED_SOURCE_MISMATCH'
    with zipfile.ZipFile(ROOT/d['archive']['path']) as z:
        if p in z.namelist():original=z.read(p)
        else:
            opts=[blob,blob.replace(b'\n',b'\r\n'),blob.rstrip(b'\n').replace(b'\n',b'\r\n')+b'\n']
            original=next(b for b in opts if len(b)==old['bytes'] and sha(b)==old['sha256'])
    assert len(original)==old['bytes'] and sha(original)==old['sha256']
    target=PRIVATE/'sealed41-originals'/p;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(original);target.chmod(stat.S_IREAD)
    transitions.append(dict(sealed=old,current=pin(p),protected_exact=pin(target),kind='CLEAN_OWNER_CHECKOUT_NEWLINE_ONLY'))
cross=load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')
for row in cross['artifacts']:
    assert not (ROOT/row['original']['path']).exists();exact(row['protected_copy']);exact(row['representation'])
    assert not (ROOT/row['protected_copy']['path']).stat().st_mode&stat.S_IWRITE
for r in load(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json')['files']:exact(r)
for p in ['evidence/04A39/register/defects-v7.json','evidence/04A40/register/issue-delta-v4.json','evidence/04A41/register/issue-delta-v2.json']:
    assert p in git('ls-files').decode().splitlines()
plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';ids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',plan.read_text(encoding='utf-8-sig'),re.M)));assert len(ids)==226
tracked=git('ls-files').decode().splitlines();inventory=[]
for i,p in enumerate(tracked):
    inventory.append(pin(p))
    if (i+1)%4000==0:print('Entry tracked hashes',i+1,'/',len(tracked),flush=True)
save(PRIVATE/'tracked-entry-inventory-v1.json',dict(files=inventory))
sources=module('runner42',ROOT/'scripts/test_suite_rationalization.py').source_pins()
for p,h in sources.items():
    assert pin(p)['sha256']==h
    dst=PRIVATE/'source-preimages'/p;dst.parent.mkdir(parents=True,exist_ok=True);dst.write_bytes((ROOT/p).read_bytes());dst.chmod(stat.S_IREAD)
save('entry/source-preimages-v1.json',dict(source_pins=sources,files=[pin(p) for p in sources],tracked_inventory=pin(PRIVATE/'tracked-entry-inventory-v1.json'),plan=pin(plan),plan_ids=ids))
save('entry/authentication-v1.json',dict(status='AUTHENTICATED_ACCEPTED41_MERGED_MAIN',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,branch='codex/04A-42',entry_branch='main',index=pin('.git/index'),preexisting_owner_edits=[],accepted41_receipt=pin(receipt),accepted41_archive=d['archive'],merged41_newline_transitions=transitions,plan_revision='2.18',plan_header_stale=True,plan_ids=226,raw40_boundary=True,raw41_inventory=pin(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json'),gates=GATES))
print('Accepted41 merged main/source/raw/receipt authenticated;226 plan IDs;entry source frozen',flush=True)
