"""Bounded complete accounting after the one frozen Python campaign; no test reruns."""
from common import *
from collections import Counter
import csv
import io

assert json.loads((HERE/'validation-report-execution-v1.json').read_bytes())['exit_code']==0 # Reuse actual completed accounting, never rerun tests or overwrite its outputs.
import ast,re
old=json.loads((PREVIOUS/'python-first-failures-v2.json').read_bytes())['records']
now=json.loads((HERE/'python-first-failures-v2.json').read_bytes())['records']
def key(r):
    sub=r.get('subtest');return r['identity'],r['outcome'],sub[len(r['identity']):].strip() if sub else '',r['occurrence']
before={key(r):r for r in old};actual={key(r):r for r in now}
assert len(before)==len(old) and len(actual)==len(now)
lost=[list(k) for k in before if k not in actual];extra=[];changes=[];same=0
transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files']
scanner_ids={'test_04a11_publication.Publication11Tests.test_complete_candidate_privacy_scan',
 'test_04a13_survival_workflow.Survival13SourceContracts.test_complete_existing_publication_set_has_no_unresolved_private_values',
 'test_04a14_mixed_workflow.Mixed14SourceContracts.test_entire_proposed_publication_set_privacy'}
spc='test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact'

importer='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb'
membership={
 'test_04a05_completion_successor.Completion05SuccessorTests.test_no_designer_importer_targets_exports_or_signing_change':('bytes','scripts/tests/test_04a05_completion_successor.py',None),
 'test_04a22_spc.Spc22Contracts.test_publication_membership_hashes_empty_index_and_privacy':('set','scripts/verify-04a22-publication.py',None),
 'test_04a24_multivariate_payload.MultivariatePayload24.test_publication_exact_membership_hashes_and_empty_index':('set','scripts/verify-04a24-publication.py',None),
 'test_04a13_survival_workflow.Survival13SourceContracts.test_all_prechange_protected_bytes_except_exact_approved_scope':('list','scripts/tests/test_04a13_survival_workflow.py','TestResults/04A-13-20261103-run01/baseline.json'),
 'test_04a14_mixed_workflow.Mixed14SourceContracts.test_exact_scope_and_external_owner_plan_are_separate':('list','scripts/tests/test_04a14_mixed_workflow.py','TestResults/04A-14-20261103-run01/baseline.json')}
def membership_delta(row,previous):
    kind,test,baseline=membership[row['identity']];assert row['outcome']=='FAIL'
    before=previous['message'];after=row['message'];proof=dict(assertion_source=pin(ROOT/test),source_transitions=pin(HERE/'source-transitions-final-v1.json'))
    if kind=='bytes':
        a=set(ast.literal_eval(before.removesuffix(' is not false')).decode().splitlines());b=set(ast.literal_eval(after.removesuffix(' is not false')).decode().splitlines())
        assert b-a=={importer} and not a-b
        proof.update(before_paths=sorted(a),after_paths=sorted(b),added=[importer],removed=[])
    elif kind=='set':
        a=ast.literal_eval(before);b=ast.literal_eval(after);assert isinstance(a,set) and isinstance(b,set) and b-a=={importer} and not a-b
        proof.update(before_paths=sorted(a),after_paths=sorted(b),added=[importer],removed=[])
    else:
        pins=json.loads((ROOT/baseline).read_bytes())['source_pins'];changes=[]
        for t in transitions:
            if t['path'] in pins:
                assert t['before'] is not None
                was=t['before']['sha256']!=pins[t['path']];now=t['after']['sha256']!=pins[t['path']]
                if was!=now:changes.append(dict(path=t['path'],before_error=was,after_error=now,baseline_sha256=pins[t['path']]))
        assert changes==[dict(path=importer,before_error=False,after_error=True,baseline_sha256=next(t['before']['sha256'] for t in transitions if t['path']==importer))]
        count=lambda s:int(re.search(r'Second list contains (\d+) additional elements',s)[1])
        assert count(after)==count(before)+1
        proof.update(baseline=pin(ROOT/baseline),before_count=count(before),after_count=count(after),source_backed_changed_error_membership=changes,
            qualification='Unchanged baseline/assertion; only current source transition crossing baseline equality is the importer. Other initial source bytes are independently preserved by the full source-transition guard.')
    return proof

for k,row in actual.items():
    if k not in before:
        assert row['identity'].startswith('test_04a28_output.') and row['outcome']=='FAIL','Uninvestigated new event '+row['identity']
        transition=next(t for t in transitions if t['after']['sha256'] in row['message'])
        assert transition['path'] in (row.get('subtest') or '')
        extra.append(dict(row,classification='AUTHORIZED_C12_SOURCE_PIN_FAIL_NOT_WAIVED',transition=transition))
        continue
    previous=before[k]
    if row['message']==previous['message']:same+=1;continue
    delta=dict(identity=row['identity'],outcome=row['outcome'],subtest=row.get('subtest'),occurrence=row['occurrence'],
        old_message=previous['message'],new_message=row['message'],old_message_sha256=sha(previous['message'].encode()),
        new_message_sha256=sha(row['message'].encode()),old_traceback_sha256=previous['traceback_sha256'],
        new_traceback_sha256=row['traceback_sha256'],first_traceback=row['first_traceback'])
    if row['identity'].startswith('test_04a28_output.'):
        transition=next(t for t in transitions if t['after']['sha256'] in row['message'])
        assert transition['before']['sha256'] in previous['message']
        delta.update(classification='RETAINED_SOURCE_PIN_FAIL_AUTHORIZED_SOURCE_HASH_DELTA_NOT_WAIVED',transition=transition)
    elif row['identity'] in membership:
        delta.update(classification='RETAINED_HISTORICAL_SCOPE_FAIL_CURRENT_IMPORTER_MEMBERSHIP_DELTA_NOT_WAIVED',source_backed_membership=membership_delta(row,previous))
    elif row['identity'] in scanner_ids:
        delta.update(classification='RETAINED_ORIGINAL_SCANNER_NONPASS_ON_ACTUAL_FROZEN_FILESET_NOT_WAIVED')
    elif row['identity']==spc:
        delta.update(classification='RETAINED_UNORDERED_SPC_ERROR_EXACT_FIRST_KEY_MESSAGE_VARIANT_NOT_WAIVED')
    else:raise AssertionError('Changed nonpass needs a source-backed investigation: '+row['identity'])
    changes.append(delta)
assert not lost,'A prior event disappeared; investigate without relabelling'
giant=json.loads((W2/'continuation-v10-retained-private-preservation-v2.json').read_bytes())['retained_interrupted_aggregation']
frozen=json.loads((HERE/'python-scan-input-freeze-v1.json').read_bytes());assert giant in frozen['files']
privacy=[r for r in now if r['identity'] in scanner_ids and r['outcome']=='ERROR']
assert len(privacy)==2 and all('JSONDecodeError' in r['traceback'] for r in privacy)
helpers=[];findings=[]
for path in sorted((PRIVATE/'python-v1/1').glob('parallel-scan-*/*/raw-result.json')):
    helpers.append(pin(path))
    for row in json.loads(path.read_bytes())['rows']:findings.extend(row['findings'])
save('python-nonpass-investigation-v1.json',dict(status='SOURCE_BACKED_CURRENT_NONPASS_HISTORY_NOT_WAIVED',
    baseline_event_keys=[list(key(r)) for r in old],actual_event_keys=[list(key(r)) for r in now],
    lost_events=lost,new_events=extra,changed_messages=changes,same_clean_messages=same,
    actual_parent_outcomes=json.loads((HERE/'python-effective-identities-v1.json').read_bytes())['method_outcomes'],
    actual_event_outcomes=dict(Counter(r['outcome'] for r in now)),historical_assertions_changed=0,original_queries=0))
save('python-message-delta-investigation-v1.json',dict(changed_messages=changes,new_events=extra,
    same_clean_messages=same,raw_helpers=helpers,all_actual_helper_privacy_findings=findings,
    scope='Helper precomputation is not a PASS for original whole scanners that error at the protected malformed giant. Exact parent and event counts remain separate.'))
save('protected-privacy-error-investigation-v1.json',dict(artifact=giant,
    current_errors=[dict(identity=r['identity'],outcome=r['outcome'],message=r['message'],
        raw_message_sha256=r['raw_message_sha256'],raw_traceback_sha256=r['traceback_sha256'],first_traceback=r['first_traceback']) for r in privacy],
    v11=pin(PREVIOUS/'python-first-failures-v2.json'),v10=pin(W2/'continuation-v10/python-first-failures-v2.json'),v9=pin(W2/'continuation-v9/python-first-failures-v2.json'),
    scanner_sources=[pin(ROOT/'scripts/verify-04a13-publication.py'),pin(ROOT/'scripts/verify-04a14-publication.py')],
    remedy='No remedy applied under unchanged assertions/reference authority: original scan uses full JSON parsing, and the protected immutable interrupted aggregation is malformed. Truncating, rewriting, omitting or translating its error would change the protected input or historical semantics. A reference-authority/assertion change requires a separate specific owner decision. Current errors and earlier FAIL histories remain.',
    controls=pin(HERE/'parallel-scanner-equivalence-v1.json'),assertions_changed=0,artifact_modified=False,original_queries=0))
aux=[]
for name in ['strict','profile','docs']:
    folder=RUNROOT/'commands'/('w2c12-checkpoint-v1-'+name)
    command_record=json.loads((folder/'command.json').read_bytes())
    previous_folder=RUNROOT/'commands'/('w2c11-checkpoint-v1-'+name)
    aux.append(dict(identity=name,exit_code=command_record['exit_code'],command=clean(command_record),
        stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
        normalized_stdout=clean((folder/'stdout.log').read_text(encoding='utf-8-sig')),
        normalized_stderr=clean((folder/'stderr.log').read_text(encoding='utf-8-sig')),
        prior_stdout=pin(previous_folder/'stdout.log'),prior_stderr=pin(previous_folder/'stderr.log'),
        classification='RETAINED_NOT_WAIVED' if command_record['exit_code'] else 'PASS'))
save('auxiliary-nonpasses-v1.json',dict(records=aux,strict_reason='Existing stale/ambiguous NormalPlot constructor review decision; no source or historical assertion changed.',
    profile_reason='Existing INTEGRITY_FAILURE; full old/current streams and source pins retained.',docs='Exit0',original_queries=0))
save('nonpasses-final-v1.json',dict(managed=json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['nonpasses'],
    python=now,auxiliary=aux,parent_outcomes=json.loads((HERE/'python-effective-identities-v1.json').read_bytes())['method_outcomes'],
    event_outcomes=dict(Counter(r['outcome'] for r in now)),scope='Every actual current event with exact raw hash and prior link; none waived, dropped or relabelled PASS.'))

history=json.loads((HERE/'historical-ownership-v2.json').read_bytes())['members']
current=json.loads((HERE/'current-ownership-v2.json').read_bytes())
new=set(r['original_id'] for r in json.loads((HERE/'new-closures-final-v1.json').read_bytes())['rows'])
old_current={r['symbol_id']:r for r in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['members']}
position={r['after']:r['before'] for r in json.loads((HERE/'current-semantic-position-crosswalk-v1.json').read_bytes())['rows']}
entries=[]
for row in history:
    entries.append(dict(population='HISTORICAL',name=row['name'],identity=row['canonical_original_id'],path=row['path'],
        status='NEW' if row['canonical_original_id'] in new else 'RETAINED' if row['proof'] else 'OPEN'))
for row in current['members']:
    was=old_current[position[row['symbol_id']]]
    entries.append(dict(population='CURRENT',name=row['name'],identity=row['symbol_id'],path=row['path'],
        status='OPEN' if row['review_status'].startswith('OPEN') else 'NEW' if was['review_status'].startswith('OPEN') else 'RETAINED'))
old_support={(r['path'],r['kind'],r['owner'],r['declaration']):r for r in json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())['support']}
for row in current['support']:
    if row['already_in_primary_population']:continue
    previous=old_support[row['path'],row['kind'],row['owner'],row['declaration']]
    entries.append(dict(population='SUPPORT',name=row['declaration'],identity=row['support_id'],path=row['path'],
        status='OPEN' if not row['fully_reconciled'] else 'RETAINED' if previous['fully_reconciled'] else 'NEW'))
assert len(entries)==704+726+303
save('canonical-identity-status-v1.json',dict(entries=entries,scope='Every NEW/RETAINED/OPEN identity in the exact three distinct universes, with source path. Raw support overlap22 remains separately in current-ownership-v2.json.'))
with (HERE/'canonical-identity-status-v1.csv').open('x',encoding='utf-8',newline='') as stream:
    writer=csv.DictWriter(stream,fieldnames=['population','name','status','identity','path'],lineterminator='\n');writer.writeheader();writer.writerows(entries)
attempts=[]
for folder in sorted((RUNROOT/'commands').glob('w2c12-*')):
    path=folder/'command.json'
    if path.is_file():
        doc=json.loads(path.read_bytes())
        if doc['exit_code']:attempts.append(dict(command=pin(path),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
            exit_code=doc['exit_code'],args=clean(doc['command'])))
save('inherited-failure-mutation-ledger-v1.json',dict(nonzero_commands=attempts,
    direct_observed_tool_attempts=[],
    source_attempts='All source preimages and failed proof/tool versions retained. Initial direct shell diagnostics are explicitly observational; raw Runner streams are separately pinned.',
    production_defects_fixed=0,historical_assertions_changed=0,old_artifacts_rewritten=0,original_queries=0))
print(json.dumps(dict(Python_parents=json.loads((HERE/'python-effective-identities-v1.json').read_bytes())['method_outcomes'],
    Python_events=dict(Counter(r['outcome'] for r in now)),same_messages=same,changed_messages=len(changes),new_events=len(extra),
    canonical_identities=len(entries),new_managed_PASS=31)),flush=True)
