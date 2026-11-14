"""Read machine evidence under the owner's limited physical-publication authority."""
from witness_lib import *

VERDICT='READY_FOR_OWNER_04A39_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
def documents():
    return {name:load(HERE/file) for name,file in dict(entry='entry.json',guard='guard-controls.json',inputs='physical-inputs.json',
        physical='scanner-physical-results.json',fixtures='exact-25-fixtures.json',sdk='sdk-HEAD-proof.json',preservation='preservation-reconciliation.json').items()}
def verify(d):
    e=d['entry'];g=e['Git'];a=e['authority']
    assert g['branch']=='codex/04A-39' and g['HEAD']==g['main']==g['merge_base']==MAIN
    assert g['index']['sha256']=='2a0e5c1da0a43550074f695676ba71692af75a8007f2f4f3dc5b4504231fdb45' and not g['staged'] and not g['diff_check']
    assert a['scope']=='04A39 physical review-derivative publication gate only' and not a['acceptance']
    assert a['required_fixture_matches']==25 and a['required_unresolved']==0
    assert d['guard']['guard']['sha256']=='ad17f82adca850afb94fa60d3c0ca5866b446860a7724dc77a5e5b3111c9ce5f'
    assert d['guard']['positive_email_samples']>0 and d['guard']['owner_local_tokens_detected']
    context=d['inputs']['owner_context'];assert context['username_present'] and context['hostname_present'] and context['token_count']>0
    actual=d['physical'];assert actual['owner_local_tokens_applied'] and actual['source_rules_changed'] is False and actual['fresh_executions']==4
    reports=actual['reports'];assert len(reports)==4
    assert {(r['scanner_phase'],r['physical_members']) for r in reports}=={('13',2060),('13',19),('14',2060),('14',19)}
    packages={r['archive']['sha256']:r for r in d['inputs']['archives']}
    assert set(packages)=={'6164b6ff1f605997a4a2c482794682ab3e70bb85a48288721f252dec0bb03cab','871ec6511fbdf8236e5653df8e1153a21c0137ba609b644c00faf860e0abae76'}
    for r in reports:
        p=packages[r['archive']['sha256']]
        assert r['member_views']==len(p['views']) and r['physical_members']==len(p['files'])
        assert r['original_scan_callable'] and len(r['findings'])==(25 if r['physical_members']==2060 else 0)
    f=d['fixtures'];keys=[tuple(r['key']) for r in f['fixtures']]
    assert len(keys)==len(set(keys))==f['count']==25 and f['observed_key_checks']==50
    assert not f['missing'] and not f['changed'] and not f['additional'] and f['new_synthetic_entries']==0
    main_files={r['path']:r for p in packages.values() if len(p['files'])==2060 for r in p['files']}
    for row in f['fixtures']:
        key=row['key'];assert main_files[key[0]]['sha256']==key[1]==row['source']['sha256']
        assert row['original_line']==row['physical_line']==key[3]
        prior=dict(key=key,disposition='REVIEWED_PUBLIC_OR_SYNTHETIC_SOURCE_LITERAL',source=row['source'],proof=row['proof'],evidence=row['original_justification'])
        assert sha(json.dumps(prior,sort_keys=True).encode())==row['original_justification_sha256'],'CHANGED_OR_UNJUSTIFIED_FIXTURE_KEY'
    sdk=d['sdk'];assert sdk['HEAD']==MAIN and sdk['HEAD_snapshot']['sha256']==sdk['HEAD_source']['sha256']
    assert sdk['HEAD_snapshot']['bytes']==sdk['HEAD_source']['bytes']
    assert all(sdk['physical_main_source'][k]==main_files['BESHstatNG/BESHStatNG.vbproj'][k] for k in ('bytes','sha256'))
    assert len(sdk['occurrences'])==2 and {r['line'] for r in sdk['occurrences']}=={603,606}
    assert all(r['new_allowance'] is False for r in sdk['occurrences']) and sdk['new_synthetic_allowances']==0
    assert len(sdk['controls'])==2 and all(r['live_HEAD_findings']==0 and r['missing_HEAD_literal_findings']==2 and r['wildcard_not_used'] for r in sdk['controls'])
    p=d['preservation'];assert (p['original_NOT_READY_files'],p['source_test_plan_pins'])==(468,1353)
    assert (p['register']['canonical_entries'],p['register']['original_obligations'],p['register']['restricted_controls'],p['register_crosswalk_rows'],p['current_register_source_pins'],p['policy_members'])==(1169,1514,802,2967,33,192)
    assert p['register_statuses_aliases_unchanged'] and p['Python_parents']==956 and p['changed_diagnostic_FAIL_events']==54
    assert p['privacy']==dict(W3='TWO_ERROR_PARENTS_UNWAIVED',current='TWO_FAIL_PARENTS_UNWAIVED',each=1941,snapshot_each=1599,added_each=342)
    assert all(r['cases']==29424 and r['preedit']==28966 and r['supplement']==458 for r in p['compatibility']) and len(p['compatibility'])==2
    assert len(p['exports'])==2 and all(r['signatures']==268 for r in p['exports'])
    assert all(d[k].get('gates',GATES)==GATES for k in d),'RESTRICTED_GATE_CHANGED'
    return dict(status=VERDICT,publication='PHYSICAL_DERIVATIVE_PUBLICATION_PASS',publication_unresolved_sensitive_findings=0,
        synthetic_exact_matches=25,public_SDK_original_conditional_matches=2,postseal_matches=0,
        repository_privacy='REPOSITORY_CANDIDATE_PRIVACY_FAIL_UNWAIVED',W3_privacy='TWO_HISTORICAL_ERROR_PARENTS_UNWAIVED',
        changed_diagnostics='54_RETAINED_FAIL_DIFFERENCES_EXPLAINED_NOT_WAIVED',owner39_acceptance=False,accepted_or_merged=False,gates=GATES)
if __name__=='__main__':verify_frozen();print(json.dumps(verify(documents()),sort_keys=True))
