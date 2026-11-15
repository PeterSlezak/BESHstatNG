from common40 import *
import xml.etree.ElementTree as ET

assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==git('merge-base','HEAD','main').decode().strip()==MAIN
assert git('rev-parse','HEAD^').decode().strip()==PREVIOUS
assert not git('diff','--cached','--name-only') and not git('diff','--check')
assert git('diff','--name-only').decode().splitlines()==['BESHstatNG/Generated/VersionAttributes.vb']
PRIVATE.mkdir(exist_ok=False)
raw=dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),head=MAIN,main=MAIN,parent=PREVIOUS,
    status=git('status','--short').decode(),status_all=git('status','--porcelain=v1','-uall').decode(),index=pin('.git/index'),
    owner_diff=git('diff','--','BESHstatNG/Generated/VersionAttributes.vb').decode(),worktrees=git('worktree','list','--porcelain').decode())
save(PRIVATE/'entry-git-raw.json',raw)
freeze=load(ROOT/'evidence/04A39/correction-v1/entry/source-lock.json')['files']
current=[]
for row in freeze:
    now=pin(row['path']);assert now==row,'ACCEPTED39_LIVE_SOURCE_CHANGED '+row['path'];current.append(now)
paths=[r['path'] for r in freeze]
p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'cat-file','--batch'],cwd=ROOT,input=('\n'.join(MAIN+':'+p for p in paths)+'\n').encode(),capture_output=True)
assert p.returncode==0
offset=0;committed=[];excluded=[]
for row in freeze:
    end=p.stdout.index(b'\n',offset);header=p.stdout[offset:end];offset=end+1
    if header.endswith(b' missing'):
        excluded.append(dict(**row,reason='PINNED_GENERATED_OR_IGNORED_SUPPORT_NOT_IN_GIT_TREE'));continue
    object_id,kind,size=header.split();size=int(size);assert kind==b'blob';data=p.stdout[offset:offset+size];offset+=size+1
    actual=(ROOT/row['path']).read_bytes()
    classification='EXACT' if sha(data)==row['sha256'] else 'AUTHORIZED_NEWLINE_ONLY' if data.replace(b'\r\n',b'\n')==actual.replace(b'\r\n',b'\n') else 'UNEXPECTED'
    assert classification!='UNEXPECTED','MERGED39_TREE_SOURCE_MISMATCH '+row['path']
    committed.append(dict(path=row['path'],git_blob=object_id.decode(),bytes=size,sha256=sha(data),classification=classification))
important=['evidence/04A39/physical-witness-v1/owner-review-v4.md','evidence/04A39/physical-witness-v1/readiness-v4.json','evidence/04A39/physical-witness-v1/delivery-receipt-v4.json',
 'evidence/04A39/register/current-v5.json','evidence/04A39/register/defects-v7.json','evidence/04A39/map/method-policy-map-v1.json','evidence/04A39/final/next-gate-handover.md']
authorities=[]
for path in important:
    blob=git('show',MAIN+':'+path);live=(ROOT/path).read_bytes()
    assert blob.replace(b'\r\n',b'\n')==live.replace(b'\r\n',b'\n')
    authorities.append(dict(live=pin(path),committed_sha256=sha(blob)))
assert load(ROOT/'evidence/04A39/physical-witness-v1/readiness-v4.json')['status']=='READY_FOR_OWNER_04A39_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
reg=load(ROOT/'evidence/04A39/register/defects-v7.json');assert len(reg['issues'])==1169 and len(reg['crosswalk'])==2967
save('entry/source-preimages-v1.json',dict(kind='04A40_CURRENT_MERGED_MAIN_PRE_CHANGE',files=current,committed=committed,excluded_from_git_tree=excluded))
save('entry/authentication-v1.json',dict(utc=utc(),status='04A39_OWNER_ACCEPTED_AND_MERGED_VERIFIED',owner_acceptance='Owner assertion in current40 instruction; no invented prior receipt.',
    entry_branch=raw['branch'],HEAD=MAIN,main=MAIN,parent=PREVIOUS,merge_base=MAIN,index=raw['index'],staged_empty=True,diff_check_exit=0,
    preexisting_owner_edit=pin('BESHstatNG/Generated/VersionAttributes.vb'),owner_edit_scope='BuildDate2026-11-02 to2026-11-14 only; preserve bytes and do not run version-generator target.',
    source_paths=1353,committed_source_paths=len(committed),ignored_support_paths=len(excluded),authority_chain=authorities,
    plan=dict(path='planning/BESHStatNG_CrossPlatform_MasterPlan.md',revision='2.15',discrepancy='Still records39IN_PROGRESS; owner accepted/merged39 at actualf2eb. Reconcile additively without claiming existing plan was current.'),
    raw_git=pin(PRIVATE/'entry-git-raw.json'),gates=GATES))
print('Verified39 merge',MAIN,'direct child',PREVIOUS,'1353 exact live source pins;',len(committed),'committed source identities;',len(excluded),'explicit exclusions. Owner BuildDate preserved.',flush=True)
