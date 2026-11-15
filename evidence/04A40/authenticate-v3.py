"""Explicit owner-commit transition, not normalization of the accepted39 source oracle."""
from common40_v3 import *

assert git('rev-parse','HEAD').decode().strip()==git('rev-parse','main').decode().strip()==git('merge-base','HEAD','main').decode().strip()==MAIN
assert git('rev-parse',MAIN+'^').decode().strip()==MERGE39 and git('rev-parse',MERGE39+'^').decode().strip()==PREVIOUS
ownerpath='BESHstatNG/Generated/VersionAttributes.vb'
assert git('diff','--name-only',MERGE39,MAIN).decode().splitlines()==[ownerpath]
assert not git('diff','--name-only') and not git('diff','--cached','--name-only') and not git('diff','--check')
raw=dict(utc=utc(),branch=git('branch','--show-current').decode().strip(),HEAD=MAIN,main=MAIN,merge39=MERGE39,pre39=PREVIOUS,index=pin('.git/index'),
    status=git('status','--porcelain=v1','-uall').decode(),worktrees=git('worktree','list','--porcelain').decode())
save(PRIVATE/'entry-git-v3-raw.json',raw)
freeze=load(ROOT/'evidence/04A39/correction-v1/entry/source-lock.json')['files'];current=[];transitions=[]
for row in freeze:
    now=pin(row['path'])
    if now!=row:
        assert row['path']==ownerpath,'UNEXPECTED39_SOURCE_CHANGE'
        old=git('show',MERGE39+':'+ownerpath);new=git('show',MAIN+':'+ownerpath);actual=(ROOT/ownerpath).read_bytes()
        assert new.replace(b'\r\n',b'\n')==actual.replace(b'\r\n',b'\n')
        assert old.replace(b'2026-11-02',b'2026-11-14')==new
        transitions.append(dict(path=ownerpath,historical_pin=row,current_pin=now,from_commit=MERGE39,to_commit=MAIN,
            classification='PREEXISTING_OWNER_COMMITTED_BUILDDATE_CHANGE;NOT_04A40_PRODUCT_EDIT'))
    current.append(now)
assert len(transitions)==1
paths=[r['path'] for r in current]
p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'cat-file','--batch'],cwd=ROOT,input=('\n'.join(MAIN+':'+p for p in paths)+'\n').encode(),capture_output=True)
assert p.returncode==0
offset=0;committed=[];excluded=[]
for row in current:
    end=p.stdout.index(b'\n',offset);header=p.stdout[offset:end];offset=end+1
    if header.endswith(b' missing'):excluded.append(dict(row,reason='PINNED_IGNORED_SUPPORT_NOT_IN_COMMIT'));continue
    oid,kind,size=header.split();size=int(size);data=p.stdout[offset:offset+size];offset+=size+1
    assert kind==b'blob'
    actual=(ROOT/row['path']).read_bytes()
    classification='EXACT' if sha(data)==row['sha256'] else 'NEWLINE_REPRESENTATION' if data.replace(b'\r\n',b'\n')==actual.replace(b'\r\n',b'\n') else 'UNEXPECTED'
    assert classification!='UNEXPECTED','MERGED_SOURCE_TREE_MISMATCH'
    committed.append(dict(path=row['path'],git_blob=oid.decode(),bytes=size,sha256=sha(data),classification=classification))
important=['evidence/04A39/physical-witness-v1/owner-review-v4.md','evidence/04A39/physical-witness-v1/readiness-v4.json','evidence/04A39/physical-witness-v1/delivery-receipt-v4.json',
 'evidence/04A39/register/current-v5.json','evidence/04A39/register/defects-v7.json','evidence/04A39/map/method-policy-map-v1.json','evidence/04A39/final/next-gate-handover.md']
authorities=[]
for path in important:
    blob=git('show',MERGE39+':'+path);live=(ROOT/path).read_bytes();assert blob.replace(b'\r\n',b'\n')==live.replace(b'\r\n',b'\n')
    authorities.append(dict(live=pin(path),committed_sha256=sha(blob)))
assert load(ROOT/'evidence/04A39/physical-witness-v1/readiness-v4.json')['status']=='READY_FOR_OWNER_04A39_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
reg=load(ROOT/'evidence/04A39/register/defects-v7.json');assert len(reg['issues'])==1169 and len(reg['crosswalk'])==2967
save('entry/source-preimages-v3.json',dict(kind='04A40_CURRENT_MERGED_MAIN_PRE_CHANGE',files=current,committed=committed,excluded_from_git_tree=excluded,historical39_source_transitions=transitions))
save('entry/authentication-v3.json',dict(status='04A39_OWNER_ACCEPTED_AND_MERGED_VERIFIED',utc=utc(),entry_branch=raw['branch'],HEAD=MAIN,main=MAIN,merge_base=MAIN,merge39=MERGE39,pre39=PREVIOUS,
    index=raw['index'],staged_empty=True,diff_check_exit=0,preexisting_owner_edit=pin(ownerpath),owner_edit_scope='BuildDate-only owner commit observed during entry; preserve newmain bytes. Initial stale-anchor and historical-pin stops retained; no product implementation started.',
    original39_pins=1353,historical39_exact_current=1352,owner_transitions=transitions,committed_source_paths=len(committed),ignored_support_paths=len(excluded),authority_chain=authorities,
    owner_acceptance='Current owner40 instruction asserts39 accepted/merged; actual committed39 implementation authenticated independently.',
    plan_discrepancy='Live/main plan remainsRevision2.15 with39IN_PROGRESS; a historical description, not current owner authority.',raw_git=pin(PRIVATE/'entry-git-v3-raw.json'),gates=GATES))
print('Verified updated main',MAIN,'accepted39 commit',MERGE39,'1352 exact historical pins +1 explicit owner BuildDate transition;',len(committed),'committed paths;',len(excluded),'ignored support files.',flush=True)
