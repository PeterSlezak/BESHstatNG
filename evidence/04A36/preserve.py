"""Bounded tracked-file and protected-capture hashes; no historical payload sweep."""
from work import *
from verify_packet import SOURCES
from summarize import load,clean
if __name__=='__main__':
    old=load(HERE/'entry-tracked-pins.json')['files'];aliases={public(p):p for p in git('ls-files').splitlines()}
    external=load(HERE/'external-changes.json');outside={r['path']:r for r in external['files']};changes=[];owner=[]
    for r in old:
        p=aliases[r['path']];now=pin(p)
        if (now['bytes'],now['sha256'])!=(r['bytes'],r['sha256']):
            row=dict(path=p,entry_sha256=r['sha256'],final_sha256=now['sha256'],entry_bytes=r['bytes'],final_bytes=now['bytes'])
            if p in outside:owner.append(dict(row,disposition='EXTERNAL_DOCUMENTATION_PRESERVED_NOT_36'))
            else:
                assert p in SOURCES,'Unexpected tracked modification '+p
                changes.append(dict(row,reason='Authorized36 coordinate/presentation extraction, consumed adapter or essential project links; original tests not modified.'))
    before={r['path']:r for r in old};lineage=[]
    for r in load(ROOT/'evidence/04A35/lineage-check.json')['pins']:
        p=r['path'];lineage.append(dict(path=p,original_04A28_sha256=r['original_04A28_sha256'],accepted35_sha256=r['final35_sha256'],merged_main_sha256=before[p]['sha256'],final36_sha256=pin(p)['sha256'],transition='AUTHORIZED_36_SOURCE_SUCCESSOR' if p in {x['path'] for x in changes} else 'PRESERVED_ENTRY_BYTES'))
    assert len(lineage)==278
    save('lineage-check.json',dict(status='VERIFIED_SUCCESSOR_MAP',pins=lineage,qualification='Original278 source assertions/manifests remain unchanged; current authorized successors do not retroactively pass old whole-file pins.'))
    captures=[]
    for p,r in load(ROOT/'evidence/04A14/baseline-registry.json')['files'].items():
        now=pin(p);assert now['sha256']==r['sha256'],public(p);captures.append(dict(path=public(p),bytes=now['bytes'],sha256=now['sha256'],matches=True))
    assert len(captures)==100
    save('mixed-model-captures-check.json',dict(status='PASS',count=100,files=captures,scientific='NOT_GOLDEN'))
    save('preservation.json',dict(status='HASHES_VERIFIED',tracked_entry=len(old),unchanged=len(old)-len(changes)-len(owner),authorized_changes=changes,external_documentation_changes=owner,unexpected_changes=[],all_original_test_sources_unchanged=True,all_historical_dossiers_evidence_manifests_unchanged=True,previous_accepted_numerical_engines_unchanged=True,historical_payload='Ignored ~138GB TestResults bytes NOT_REHASHED; bounded metadata census matches accepted35. No cleanup, delete, archive, compression or rebaseline.',master_plan='External owner-managed Revision2.13 preserved byte-for-byte as observed; confirmation pending; not a36 source/publication artifact.'))
    transitions=[]
    for p in SOURCES:
        assert (ROOT/p).is_file(),p
        transitions.append(dict(pin(p),entry_sha256=before.get(p,{}).get('sha256'),owner='Core numerical geometry / Common presentation / Windows rendering / test / review dossier',reason='Official36 only',affected_legacy_test_ids='source-and-node-validation.json and nonpasses.json'))
    save('source-transitions.json',dict(files=transitions,scope='Every proposed production/test/project/dossier path. Public evidence paths independently covered by publication manifest; external README/plan changes excluded.'))
    assert git('branch','--show-current')=='codex/04A-36' and git('rev-parse','HEAD')==git('rev-parse','main')==BASE and git('merge-base','HEAD','main')==BASE
    assert not git('diff','--cached','--name-only')
    args=['git','-c','safe.directory='+ROOT.as_posix()]
    full=subprocess.run(args+['diff','--check'],cwd=ROOT,capture_output=True,text=True)
    batch=subprocess.run(args+['diff','--check','--',*SOURCES],cwd=ROOT,capture_output=True,text=True)
    assert batch.returncode==0
    save('git-final.json',dict(branch='codex/04A-36',head=BASE,main=BASE,merge_base=BASE,index='EMPTY',status_short=git('status','--short').splitlines(),diff_stat=git('diff','--stat'),batch_diff_check_exit=batch.returncode,full_diff_check_exit=full.returncode,full_diff_check_output=clean(full.stdout+full.stderr),publication='NO_STAGE_NO_COMMIT_NO_MERGE_NO_PUSH_NO_UPLOAD_NO_DEPLOY',next_batch='NOT_STARTED'))
    print('Verified',len(old),'entry files;',len(changes),'authorized transitions;',len(owner),'external documentation changes;278pins/100captures; batch diff0/full',full.returncode)
