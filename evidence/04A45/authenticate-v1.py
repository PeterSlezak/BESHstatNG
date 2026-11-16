from common45 import *
import re,stat,zipfile
assert git('branch','--show-current').decode().strip()=='codex/04A-45'
assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()==PRIOR
assert not git('diff','--name-only') and not git('diff','--cached','--name-only') and not git('diff','--check')
PRIVATE.mkdir(exist_ok=True)
rp=ROOT/'.test-rationalization-04A44.tmp/publication/external-delivery-receipt-v1.json'
d=load(rp);exact(d['archive'])
reader=module('accepted44_reader',ROOT/'evidence/04A44/review_reader_v4.py')
assert reader.verify(ROOT/d['archive']['path'],rp)['SHA_size_CRC']=='PASS'
transitions=[];originals={r['path']:r for report in d['full_actual_candidate_scans'] for r in report['members']}
with zipfile.ZipFile(ROOT/d['archive']['path']) as z:
 for p,old in originals.items():
  if pin(p)==old:continue
  blob=git('show','HEAD:'+p);live=(ROOT/p).read_bytes()
  assert live.replace(b'\r\n',b'\n')==blob.replace(b'\r\n',b'\n')
  choices=[z.read(p)] if p in z.namelist() else [blob,blob.replace(b'\n',b'\r\n'),blob.rstrip(b'\n').replace(b'\n',b'\r\n')+b'\n']
  sealed=next(b for b in choices if len(b)==old['bytes'] and sha(b)==old['sha256'])
  target=PRIVATE/'sealed44-originals'/p;target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(sealed)
  target.chmod(stat.S_IREAD)
  transitions.append(dict(sealed=old,current=pin(p),protected_exact=pin(target),kind='AUTHENTICATED_OWNER_MERGE_NEWLINE_ONLY'))
tracked=git('ls-files').decode().splitlines();inventory=[pin(p) for p in tracked]
save(PRIVATE/'tracked-entry-inventory-v1.json',dict(files=inventory))
sources=module('entry_runner45',ROOT/'scripts/test_suite_rationalization.py').source_pins()
for p in sources:
 dest=PRIVATE/'source-preimages'/p;dest.parent.mkdir(parents=True,exist_ok=True)
 with dest.open('xb') as f:f.write((ROOT/p).read_bytes())
 dest.chmod(stat.S_IREAD)
plan=ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'
ids=list(dict.fromkeys(re.findall(r'^\|\s*\*\*(\d{2}A?\.\d{2}[A-Z]?)\*\*',plan.read_text(encoding='utf-8-sig'),re.M)))
assert len(ids)==226
save('entry/source-preimages-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A45',source_pins=sources,tracked_inventory=pin(PRIVATE/'tracked-entry-inventory-v1.json'),plan=pin(plan),plan_ids=ids,detailed_charter=pin('planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md'),scientific='NOT_GOLDEN',native='NOT_RUN'))
save('entry/authentication-v1.json',dict(status='AUTHENTICATED_OWNER_ACCEPTED44_MERGED_MAIN',HEAD=MAIN,main=MAIN,merge_base=MAIN,prior=PRIOR,branch='codex/04A-45',entry_branch='main',index=pin('.git/index'),staged_empty=True,entry_status_porcelain='',diff_check_exit=0,log=git('log','-6','--format=%H %s').decode().splitlines(),accepted44_receipt=pin(rp),accepted44_archive=d['archive'],merged44_newline_transitions=transitions,tracked_files=len(tracked),preexisting_owner_edits=[],living_revision='2.21',living_header_is_historical=True,plan_ids=226,gates=GATES))
print('Authenticated merged44 delivery,',len(transitions),'newline transitions;',len(tracked),'tracked pins; source preimages frozen',flush=True)
