"""Preserve v1 accounting rejection; explain actual new importer membership messages."""
from common import *
source=HERE/'postprocess_v1.py';text=source.read_text(encoding='utf-8')
old="result=command('validation-identity-report-v1',[sys.executable,'-B',str(HERE/'report_validation_v1.py')])\nsave('validation-report-execution-v1.json',clean(result));assert result['exit_code']==0"
assert old in text
text=text.replace(old,"assert json.loads((HERE/'validation-report-execution-v1.json').read_bytes())['exit_code']==0 # Reuse actual completed accounting, never rerun tests or overwrite its outputs.\nimport ast,re")
insert='''
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
        count=lambda s:int(re.search(r'Second list contains (\\d+) additional elements',s)[1])
        assert count(after)==count(before)+1
        proof.update(baseline=pin(ROOT/baseline),before_count=count(before),after_count=count(after),source_backed_changed_error_membership=changes,
            qualification='Unchanged baseline/assertion; only current source transition crossing baseline equality is the importer. Other initial source bytes are independently preserved by the full source-transition guard.')
    return proof
'''
anchor='for k,row in actual.items():';assert anchor in text;text=text.replace(anchor,insert+'\n'+anchor)
anchor="    elif row['identity'] in scanner_ids:"
text=text.replace(anchor,"    elif row['identity'] in membership:\n        delta.update(classification='RETAINED_HISTORICAL_SCOPE_FAIL_CURRENT_IMPORTER_MEMBERSHIP_DELTA_NOT_WAIVED',source_backed_membership=membership_delta(row,previous))\n"+anchor)
target=HERE/'postprocess_v2.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
failed=RUNROOT/'commands/w2c12-postscan-identity-reconciliation-v1'
save('postscan-accounting-correction-v1.json',dict(status='V1_ACCOUNTING_REJECTION_RETAINED_ADDITIVE_V2_NO_TEST_RERUN',before=pin(source),after=pin(target),
    unsuccessful_attempt={name:pin(failed/name) for name in ['command.json','stdout.log','stderr.log']},
    reason='V1 anticipated hash-pin deltas but correctly refused five previously unclassified historical membership/list messages. Actual change is exactly one newly edited authorized importer path. V2 independently checks set/bytes membership or unchanged baseline equality transitions and +1 count. No test/assertion/input or outcome changed.',original_queries=0))
print('Additive v2 postprocessing source prepared; all v1 outputs and failed trace retained')
