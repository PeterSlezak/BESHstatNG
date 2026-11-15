from common43 import *
import re,stat,zipfile
assert git('branch','--show-current').decode().strip()=='codex/04A-43'
assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()==PRIOR
assert not git('diff','--name-only') and not git('diff','--cached','--name-only') and not git('diff','--check')
PRIVATE.mkdir(exist_ok=True)
rp=ROOT/'.test-rationalization-04A42.tmp/publication/external-delivery-receipt-v1.json';assert pin(rp)['sha256']=='a60e331d9b9181b62f60b9a35f6df93a0f552d1636d4529d4418a92ffb460299'
d=load(rp);exact(d['archive']);reader=module('accepted42_reader',ROOT/'evidence/04A42/review_reader.py');assert reader.verify(ROOT/d['archive']['path'],rp)['SHA_size_CRC']=='PASS'
transitions=[];originals={r['path']:r for report in d['full_actual_candidate_scans'] for r in report['members']}
with zipfile.ZipFile(ROOT/d['archive']['path']) as z:
    for p,old in originals.items():
        if pin(p)==old:continue
        blob=git('show','HEAD:'+p);live=(ROOT/p).read_bytes();assert live.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n')
        choices=[z.read(p)] if p in z.namelist() else [blob,blob.replace(b'\n',b'\r\n'),blob.rstrip(b'\n').replace(b'\n',b'\r\n')+b'\n']
        exact_bytes=next(b for b in choices if len(b)==old['bytes'] and sha(b)==old['sha256'])
        target=PRIVATE/'sealed42-originals'/p;target.parent.mkdir(parents=True,exist_ok=True)
        with target.open('xb') as f:f.write(exact_bytes)
        target.chmod(stat.S_IREAD);transitions.append(dict(sealed=old,current=pin(p),protected_exact=pin(target),kind='AUTHENTICATED_OWNER_MERGE_NEWLINE_ONLY'))
for row in load(ROOT/'evidence/04A40/owner-authorized-candidate-v3/raw-presentation-crosswalk-v3.json')['artifacts']:
    assert not (ROOT/row['original']['path']).exists();exact(row['protected_copy']);exact(row['representation'])
    assert not (ROOT/row['protected_copy']['path']).stat().st_mode&stat.S_IWRITE
for batch in ['41','42']:
    raw=ROOT/('.test-rationalization-04A'+batch+'.tmp/new-raw-inventory-v1.json');files=load(raw)['files']
    for r in files:exact(r)
    print('Authenticated raw'+batch+' pins',len(files),flush=True)
plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md';ids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',plan.read_text(encoding='utf-8-sig'),re.M)));assert len(ids)==226
tracked=git('ls-files').decode().splitlines();inventory=[]
for i,p in enumerate(tracked):
    inventory.append(pin(p))
    if (i+1)%4000==0:print('Tracked entry hashes',i+1,'/',len(tracked),flush=True)
save(PRIVATE/'tracked-entry-inventory-v1.json',dict(files=inventory))
sources=module('isolated43',ROOT/'scripts/test_suite_rationalization.py').source_pins()
for p,h in sources.items():
    assert pin(p)['sha256']==h;dest=PRIVATE/'source-preimages'/p;dest.parent.mkdir(parents=True,exist_ok=True)
    with dest.open('xb') as f:f.write((ROOT/p).read_bytes())
    dest.chmod(stat.S_IREAD)
for p in [plan,ROOT/'.git/index']:
    dest=PRIVATE/'entry-pins'/p.relative_to(ROOT);dest.parent.mkdir(parents=True,exist_ok=True);dest.write_bytes(p.read_bytes());dest.chmod(stat.S_IREAD)
save('entry/source-preimages-v1.json',dict(source_pins=sources,files=[pin(p) for p in sources],tracked_inventory=pin(PRIVATE/'tracked-entry-inventory-v1.json'),plan=pin(plan),plan_ids=ids,detailed_plan=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md')))
save('entry/authentication-v1.json',dict(status='AUTHENTICATED_OWNER_ACCEPTED42_MERGED_MAIN',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,branch='codex/04A-43',entry_branch='main',index=pin('.git/index'),preexisting_owner_edits=[],accepted42_receipt=pin(rp),accepted42_archive=d['archive'],merged42_newline_transitions=transitions,plan_revision='2.19',plan_header_stale=True,plan_ids=226,raw40_exclusion=True,raw41_prior_inventory=pin(ROOT/'.test-rationalization-04A41.tmp/new-raw-inventory-v1.json'),raw42_prior_inventory=pin(ROOT/'.test-rationalization-04A42.tmp/new-raw-inventory-v1.json'),gates=GATES))
print('Merged42 source/raw/delivery and226 plan IDs authenticated;exact current source preimages frozen',flush=True)
