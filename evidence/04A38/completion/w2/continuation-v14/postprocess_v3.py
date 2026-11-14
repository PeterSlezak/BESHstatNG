"""Preserve each old event and source-backed delta; parent/event units remain distinct."""
from common import *
from collections import Counter
import ast,csv,re
old=json.loads((PREVIOUS/'python-first-failures-v2.json').read_bytes())['records'];now=json.loads((HERE/'python-first-failures-v2.json').read_bytes())['records']
key=lambda r:(r['identity'],r['outcome'],(r.get('subtest') or '')[len(r['identity']):].strip(),r['occurrence'])
before={key(r):r for r in old};actual={key(r):r for r in now};assert len(before)==len(old) and len(actual)==len(now)
lost=[list(k) for k in before if k not in actual];assert not lost,'Prior nonpass events disappeared; investigate without waiver'
trans=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'];changed_paths={t['path'] for t in trans}|{json.loads((HERE/'owner-plan-authorized-transition-v1.json').read_bytes())['after']['path']};extra=[];changes=[];same=0
scanids={'test_04a11_publication.Publication11Tests.test_complete_candidate_privacy_scan',
    'test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values',
    'test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'}
scopebytes='test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change'
scopesets={'test_04a22_spc.Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy','test_04a24_multivariate_payload.MultivariatePayload24.test_publication_exact_membership_hashes_and_empty_index'}
scopelists={
 'test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope':'TestResults/04A-13-20261103-run01/baseline.json',
 'test_04a14_mixed_workflow.Mixed14SourceContracts.test_exact_scope_and_external_owner_plan_are_separate':'TestResults/04A-14-20261103-run01/baseline.json'}
spc='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact'
for k,row in actual.items():
    if k not in before:
        assert row['identity'].startswith('test_04a28_output.') and row['outcome']=='FAIL','Uninvestigated new Python event '+row['identity']
        t=next(t for t in trans if t['after']['sha256'] in row['message']);assert t['path'] in (row.get('subtest') or '')
        extra.append(dict(row,classification='NEW_C14_SOURCE_PIN_FAIL_NOT_WAIVED',transition=t));continue
    previous=before[k]
    if row['message']==previous['message']:same+=1;continue
    delta=dict(identity=row['identity'],outcome=row['outcome'],subtest=row.get('subtest'),occurrence=row['occurrence'],
        old_message=previous['message'],new_message=row['message'],old_message_sha256=sha(previous['message'].encode()),new_message_sha256=sha(row['message'].encode()),
        old_traceback_sha256=previous['traceback_sha256'],new_traceback_sha256=row['traceback_sha256'],first_traceback=row['first_traceback'])
    if row['identity'].startswith('test_04a28_output.'):
        t=next(t for t in trans if t['after']['sha256'] in row['message']);assert t['before'] and t['before']['sha256'] in previous['message']
        delta.update(classification='RETAINED_SOURCE_PIN_FAIL_C14_AUTHORIZED_HASH_DELTA_NOT_WAIVED',transition=t)
    elif row['identity']==scopebytes or row['identity'] in scopesets:
        if row['identity']==scopebytes:
            a=set(ast.literal_eval(previous['message'].removesuffix(' is not false')).decode().splitlines());b=set(ast.literal_eval(row['message'].removesuffix(' is not false')).decode().splitlines())
        else:a=ast.literal_eval(previous['message']);b=ast.literal_eval(row['message'])
        assert isinstance(a,set) and isinstance(b,set) and not a-b and b-a and b-a<=changed_paths
        delta.update(classification='RETAINED_HISTORICAL_SCOPE_FAIL_CURRENT_AUTHORIZED_PATH_DELTA_NOT_WAIVED',before_paths=sorted(a),after_paths=sorted(b),added=sorted(b-a),removed=[])
    elif row['identity'] in scopelists:
        path=ROOT/scopelists[row['identity']];pins=json.loads(path.read_bytes())['source_pins'];events=[]
        for t in trans:
            if t['path'] in pins:
                assert t['before'] is not None
                was=t['before']['sha256']!=pins[t['path']];isnow=t['after']['sha256']!=pins[t['path']]
                if was!=isnow:events.append(dict(path=t['path'],before_error=was,after_error=isnow,baseline_sha256=pins[t['path']]))
        count=lambda s:int(re.search(r'Second list contains (\d+) additional elements',s)[1])
        assert events and all(not e['before_error'] and e['after_error'] for e in events) and count(row['message'])-count(previous['message'])==len(events)
        delta.update(classification='RETAINED_BASELINE_SCOPE_FAIL_SOURCE_BACKED_ADDED_PATHS_NOT_WAIVED',baseline=pin(path),changes=events,before_count=count(previous['message']),after_count=count(row['message']))
    elif row['identity'] in {'test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates','test_04a23_multivariate_spc.Spc23Contracts.test_actual_merged_base_and_protected_owner_plan'}:
        transition=json.loads((HERE/'owner-plan-authorized-transition-v1.json').read_bytes())
        assert transition['before']['sha256'] in previous['message'] and transition['after']['sha256'] in row['message'] and pin(ROOT/transition['after']['path'])==transition['after']
        delta.update(classification='RETAINED_OWNER_DOCUMENT_PIN_FAIL_OWNER_AUTHORIZED_PLAN_HASH_DELTA_NOT_WAIVED',transition=pin(HERE/'owner-plan-authorized-transition-v1.json'))
    elif row['identity'] in scanids:delta.update(classification='RETAINED_ORIGINAL_SCANNER_NONPASS_ACTUAL_FROZEN_FILESET_NOT_WAIVED')
    elif row['identity']==spc:delta.update(classification='RETAINED_DISTINCT_UNORDERED_SPC_ERROR_FIRST_KEY_MESSAGE_VARIANT_NOT_WAIVED')
    else:raise AssertionError('Changed nonpass needs individual source investigation: '+row['identity'])
    changes.append(delta)
parents=json.loads((HERE/'python-effective-identities-v1.json').read_bytes())['method_outcomes'];eventcounts=dict(Counter(r['outcome'] for r in now))
save('python-nonpass-investigation-v1.json',dict(status='SOURCE_BACKED_CURRENT_NONPASS_HISTORY_NOT_WAIVED',lost_events=lost,new_events=extra,changed_messages=changes,same_clean_messages=same,
    actual_parent_outcomes=parents,actual_event_outcomes=eventcounts,v13_baseline=dict(parents=dict(PASS=830,FAIL=113,ERROR=13),events=dict(FAIL=181,ERROR=13)),
    v13_prior_investigation=pin(PREVIOUS/'python-nonpass-investigation-v1.json'),v12_extra_importer_subtest='UdfDataImportMultivariate.vb retained by exact event key; later hash/message changes are separate.',historical_assertions_changed=0,original_queries=0))
privacy=[r for r in now if r['identity'] in scanids and r['outcome']=='ERROR'];assert len(privacy)==2 and all('JSONDecodeError' in r['traceback'] for r in privacy)
giant=json.loads((W2/'continuation-v13-retained-private-preservation-v1.json').read_bytes())['retained_interrupted_aggregation']
assert giant in json.loads((HERE/'python-scan-input-freeze-v2.json').read_bytes())['files']
save('protected-privacy-error-investigation-v1.json',dict(current_errors=privacy,artifact=giant,
    prior_v13=pin(PREVIOUS/'python-first-failures-v2.json'),prior_v12=pin(W2/'continuation-v12/python-first-failures-v2.json'),prior_v10=pin(W2/'continuation-v10/python-first-failures-v2.json'),prior_v9=pin(W2/'continuation-v9/python-first-failures-v2.json'),
    remedy='No protected artifact/assertion/reference change. The original scanners still parse the malformed immutable JSON and error. Earlier FAIL histories and all subsequent ERROR histories remain. Any authority/assertion remedy requires a separate specific owner decision.',original_queries=0))
aux=[]
for name in ['strict','profile','docs']:
    folder=RUNROOT/'commands'/('w2c14-checkpoint-v1-'+name);cmd=json.loads((folder/'command.json').read_bytes())
    aux.append(dict(identity=name,exit_code=cmd['exit_code'],command=clean(cmd),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),classification='RETAINED_NOT_WAIVED' if cmd['exit_code'] else 'PASS'))
save('nonpasses-final-v1.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['nonpasses'],python=now,auxiliary=aux,parent_outcomes=parents,event_outcomes=eventcounts,scope='Actual first outcomes and all old message/event deltas, no waiver or ERROR-to-PASS rewrite.'))
history=json.loads((HERE/'historical-ownership-v2.json').read_bytes())['members'];current=json.loads((HERE/'current-ownership-v2.json').read_bytes())
new={r['original_id'] for r in json.loads((HERE/'new-closures-final-v1.json').read_bytes())['rows']};oldcurrent={n['symbol_id']:n for n in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['members']}
positions={r['after']:r['before'] for r in json.loads((HERE/'current-semantic-position-crosswalk-v1.json').read_bytes())['rows']}
entries=[dict(population='HISTORICAL',name=r['name'],identity=r['canonical_original_id'],path=r['path'],status='NEW' if r['canonical_original_id'] in new else 'RETAINED' if r['proof'] else 'OPEN') for r in history]
for row in current['members']:
    was=oldcurrent[positions[row['symbol_id']]];entries.append(dict(population='CURRENT',name=row['name'],identity=row['symbol_id'],path=row['path'],status='OPEN' if row['review_status'].startswith('OPEN') else 'NEW' if was['review_status'].startswith('OPEN') else 'RETAINED'))
old_support={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['support']}
for s in current['support']:
    if s['already_in_primary_population']:continue
    was=old_support[s['path'],s['kind'],s['owner'],s['declaration']]
    entries.append(dict(population='SUPPORT',name=s['declaration'],identity=s['support_id'],path=s['path'],status='OPEN' if not s['fully_reconciled'] else 'RETAINED' if was['fully_reconciled'] else 'NEW'))
assert len(entries)==1733
save('canonical-identity-status-v1.json',dict(entries=entries,scope='Every exact NEW/RETAINED/OPEN identity in704+726+303 universes.'))
with (HERE/'canonical-identity-status-v1.csv').open('x',encoding='utf-8',newline='') as stream:
    writer=csv.DictWriter(stream,fieldnames=['population','name','status','identity','path'],lineterminator='\n');writer.writeheader();writer.writerows(entries)
attempts=[]
for folder in sorted((RUNROOT/'commands').glob('w2c14-*')):
    path=folder/'command.json'
    if path.is_file():
        cmd=json.loads(path.read_bytes())
        if cmd['exit_code']:attempts.append(dict(command=pin(path),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),exit_code=cmd['exit_code'],args=clean(cmd['command'])))
save('inherited-failure-mutation-ledger-v1.json',dict(nonzero_commands=attempts,source_preimages_preserved=True,historical_assertions_changed=0,
    current_only_extra_managed_probes=0,scope='All captured raw Runner failures and explicit source/callback/accounting correction receipts retained; ByRef cases are current-only managed unit diagnostics, never original-reference captures.',original_queries=0))
print(json.dumps(dict(parents=parents,events=eventcounts,same_messages=same,changed_messages=len(changes),new_events=len(extra),lost_events=len(lost),canonical_identities=len(entries))))
