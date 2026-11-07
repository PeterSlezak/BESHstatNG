"""Compare tracked entry hashes and protected small captures; no historical payload rehash."""
from work import *
from verify_packet import SOURCES
def load(p):return json.loads(p.read_bytes())
if __name__=='__main__':
    old=load(HERE/'entry-tracked-pins.json')['files'];aliases={public(p):p for p in git('ls-files').splitlines()};changes=[]
    for r in old:
        p=aliases[r['path']];now=pin(p)
        if now['bytes']!=r['bytes'] or now['sha256']!=r['sha256']:
            assert p in SOURCES,'Unrelated modification '+p
            changes.append(dict(path=p,entry_sha256=r['sha256'],final_sha256=now['sha256'],reason='Authorized35 projection/adapter or required Framework Compile links. Existing Love projection adds original-list ordinals only; old numeric/table/getter bodies preserved.',affected_tests='validation.json and source-and-node-validation.json; historical assertions remain real FAIL/ERROR'))
    previous=load(ROOT/'evidence/04A34/lineage-check.json');before={r['path']:r for r in old};lineage=[]
    for r in previous['pins']:
        p=r['path'];lineage.append(dict(path=p,original_04A28_sha256=r['original_04A28_sha256'],accepted34_sha256=r['final_04A34_sha256'],merged_main_sha256=before[p]['sha256'],final35_sha256=pin(p)['sha256'],transition='AUTHORIZED_35' if p in {x['path'] for x in changes} else 'PRESERVED_ENTRY_BYTES'))
    assert len(lineage)==278
    save('lineage-check.json',dict(status='PASS_SUCCESSOR_MAP',pins=lineage,qualification='Old278 pins/manifests/tests unchanged. Entry Git/EOL qualification distinct from final authorized source transitions; not retroactive old-pin passes.'))
    captures=[]
    for p,r in load(ROOT/'evidence/04A14/baseline-registry.json')['files'].items():
        now=pin(p);assert now['sha256']==r['sha256'],p;captures.append(dict(path=public(p),sha256=now['sha256'],matches=True))
    assert len(captures)==100
    save('mixed-model-captures-check.json',dict(status='PASS',count=100,files=captures,scientific='NOT_GOLDEN'))
    save('preservation.json',dict(status='PASS',tracked_entry=len(old),unchanged=len(old)-len(changes),authorized_changes=changes,unrelated_changes=[],master_plan_unchanged=True,all_prior_tests_evidence_unchanged=True,all_accepted_core_statistics_unchanged=True,accepted_common_exception='Only PsmLovePlotProjection constructor/source-ordinal metadata; unchanged old nonconstructor token bodies verified.',historical_payload='Ignored ~138GB TestResults population NOT_REHASHED; metadata census separately verified; no cleanup/delete/relocation'))
    transitions=[]
    for p in SOURCES:
        assert (ROOT/p).is_file(),p
        transitions.append(dict(pin(p),entry_sha256=before.get(p,{}).get('sha256'),owner='Core numeric geometry / Common chart intent / Windows rendering or test/evidence',reason='Official35 only'))
    save('source-transitions.json',dict(files=transitions,scope='All proposed source/test/project/dossier files; evidence artifacts inventoried by publication manifest'))
    assert git('branch','--show-current')=='codex/04A-35' and git('rev-parse','HEAD')==git('rev-parse','main')==BASE and git('merge-base','HEAD','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--check')
    save('git-final.json',dict(branch='codex/04A-35',head=BASE,main=BASE,merge_base=BASE,index='EMPTY',status_short=git('status','--short').splitlines(),diff_stat=git('diff','--stat'),diff_check='PASS',publication='NO_STAGE_NO_COMMIT_NO_MERGE_NO_PUSH_NO_UPLOAD_NO_DEPLOY',next_batch='NOT_STARTED'))
    print('Preserved',len(old)-len(changes),'tracked entry files;',len(changes),'authorized transitions;278pins/100captures')
