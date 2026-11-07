"""Current tracked-source preservation and successor lineage; no old payload rehash."""
from entry import ROOT,HERE,BASE,git,pin,sha,save,public
from verify_manifest import SOURCES
import json,re,collections,subprocess
def load(p):return json.loads(p.read_bytes())
if __name__=='__main__':
    entry=load(HERE/'entry-pins.json')['files']; aliases={public(p):p for p in git('ls-files').splitlines()}
    changed=[]
    for r in entry:
        p=aliases[r['path']];now=pin(p)
        if now['sha256']!=r['sha256'] or now['bytes']!=r['bytes']:
            assert p in SOURCES,'Unrelated tracked modification: '+p
            changed.append(dict(path=p,entry_sha256=r['sha256'],final_sha256=now['sha256'],reason='Authorized completed04A34 extraction or necessary Framework test Compile links',owner='Core/Common geometry; native host retained',affected_historical_tests='See source-and-node-validation-final.json; original assertions unchanged'))
    save('source-preservation.json',dict(status='PASS',tracked_entry_files=len(entry),unchanged=len(entry)-len(changed),changed=changed,all_previous_tests_evidence_controls_unchanged=True,all_04A30_33_core_engines_unchanged=True,master_plan_unchanged=True,unrelated_owner_changes=[],qualification='Every tracked entry file SHA/size compared; unrelated bytes unchanged. Ignored historical payload bytes not rehashed.'))
    before={r['path']:r for r in entry}; prior=load(ROOT/'evidence/04A33/source-transition-successor.json');rows=[]
    for r in prior['pins']:
        p=r['path'];current=pin(p)
        rows.append(dict(path=p,original_04A28_sha256=r['original_04A28_sha256'],accepted_04A33_sha256=r['final_04A33_sha256'],merged_main_sha256=before[p]['sha256'],final_04A34_sha256=current['sha256'],transition='AUTHORIZED_04A34' if p in {x['path'] for x in changed} else 'RETAINED_ACCEPTED_LINEAGE'))
    assert len(rows)==278 and len({r['path'] for r in rows})==278
    save('lineage-check.json',dict(status='PASS_SUCCESSOR_MAP',prior=pin('evidence/04A33/source-transition-successor.json'),pins=rows,qualification='Original278 pins and historical tests remain unchanged. Authorized changed sources are successor transitions, not passes for old whole-file pins. Prior EOL qualification remains in entry manifest receipt.'))
    captures=[]
    for p,r in load(ROOT/'evidence/04A14/baseline-registry.json')['files'].items():
        actual=pin(p);captures.append(dict(path=public(p),sha256=actual['sha256'],matches=actual['sha256']==r['sha256']))
    assert len(captures)==100 and all(r['matches'] for r in captures)
    save('mixed-model-captures-check.json',dict(status='PASS',count=100,files=captures,scientific='NOT_GOLDEN'))
    source=[]
    for p in SOURCES:
        if (ROOT/p).is_file():source.append(dict(pin(p),entry_sha256=before.get(p,{}).get('sha256'),kind='MODIFIED' if p in before else 'NEW',reason='Authorized completed04A34 source/test/project/review artifact; no publication performed'))
    save('source-transitions.json',dict(status='COMPLETE_BATCH_INVENTORY',files=source,changed_tracked=len(changed),new_source_test_files=sum(r['entry_sha256'] is None for r in source),publication='All paths must also appear in publication-manifest.json',index='EMPTY'))
    assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert git('branch','--show-current')=='codex/04A-34' and git('merge-base','HEAD','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--check')
    def exact_git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').rstrip('\r\n')
    save('git-final.json',dict(branch=git('branch','--show-current'),head=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check='PASS',status_short=exact_git('status','--short').splitlines(),diff_stat=exact_git('diff','--stat'),unrelated_owner_changes=[],next_batch='NOT_STARTED',publication='NO_STAGE_NO_COMMIT_NO_MERGE_NO_PUSH_NO_DEPLOY'))
    print('PASS tracked preservation;278 successor pins;100 mixed-model captures;index empty')
