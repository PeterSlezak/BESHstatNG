"""New mutation controls; never execute an unapproved original case."""
from common import *
s=(V6/'test_supplement.py').read_text(encoding='utf-8')
s=s[:s.index('P=reader.PREFIX')]
s=s.replace('MMRM_INTERCEPT_GROUP_EVIDENCE_PASS_NOT_BATCH_READY','APPROVAL_G_EVIDENCE_PASS_NOT_BATCH_READY')
s+='''P=reader.PREFIX
CASES={
 'fake_G_approval':(P+'owner-authorization.json',alter_json('approval','F')),
 'wrong_quote':(P+'owner-authorization.json',alter_json('owner_quote','NOT_AUTHORIZED')),
 'wrong_date':(P+'owner-authorization.json',alter_json('owner_decision_date','2026-11-08')),
 'extra_fifth_case':(P+'inputs-Debug.jsonl',alter_input(lambda r:r.append(r[0]))),
 'missing_case':(P+'inputs-Release.jsonl',alter_input(lambda r:r.pop())),
 'reordered_cases':(P+'inputs-Debug.jsonl',alter_input(lambda r:r.reverse())),
 'wrong_culture':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(culture='en-US'))),
 'wrong_ui':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(ui_culture='sk-SK'))),
 'wrong_config':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(configuration='Release'))),
 'fake_handle':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][0].update(value={'kind':'String','value':'GEE:fake'}))),
 'wrong_rank':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value'].update(rank=1))),
 'wrong_bounds':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value'].update(lower_bounds=[1,1]))),
 'wrong_element_type':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value'].update(element_type='System.String'))),
 'wrong_double_bits':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value']['values'][4].update(bits='3FF0000000000001'))),
 'wrong_role':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value']['values'][2].update(value='X1'))),
 'missing_literal_header':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value']['values'][0].update(value=''))),
 'wrong_Nothing':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[1]['operations'][1]['arguments'][2].update(value={'kind':'String','value':''}))),
 'swapped_arguments':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'].reverse())),
 'lost_session':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].pop('session'))),
 'invented_expected':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(expected_result='GUESS'))),
 'changed_factory_GEE':(P+'factory-GEE.json',alter_json('fit_arguments',[])),
 'changed_factory_MMRM':(P+'factory-MMRM.json',alter_json('fit_arguments',[])),
 'unsealed_input':(P+'input-seal.json',alter_json('status','UNSEALED')),
 'wrong_commit':(P+'input-seal.json',alter_json('source_commit','0'*40)),
 'preedit_relabel':(P+'original-output-seal.json',alter_json('reference_class','PRE_EDIT_FROZEN')),
 'current_before_seal':(P+'original-output-seal.json',alter_json('current_executions',1)),
 'late_seal':(P+'original-output-seal.json',alter_json('sealed_utc','2000-01-01T00:00:00+00:00')),
 'wrong_original_assembly':(P+'original-results-v2/Debug/plain/execution.json',lambda b:b.replace(b'"sha256":"',b'"sha256":"0',1)),
 'changed_source_map':(P+'trace-plan.json',alter_json('source_map',[])),
 'fabricated_coverage':(P+'original-coverage.json',alter_json('configurations',{})),
 'missing_true_GEE_guard':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G1.guard.intercept=True',b'G1.guard.intercept=False',1)),
 'wrong_GEE_index':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G1.lookup.index=0',b'G1.lookup.index=-1',1)),
 'early_error_as_GEE_guard':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G1.guard.intercept=True',b'G1.guard.unknown=True',1)),
 'wrong_MMRM_arm':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G2.branch.wide=True',b'G2.branch.namevalue=True',1)),
 'missing_headerless_decline':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G2.headerless.column_guard_true=True',b'G2.headerless.column_guard_true=False',1)),
 'missing_first_row':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G2.first_row.accepted=1',b'G2.first_row.accepted=-1',1)),
 'missing_true_MMRM_guard':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'G2.guard.multirow=True',b'G2.guard.multirow=False',1)),
 'private_ByRef_changed':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'G2.public.atProfile_Nothing=True',b'G2.public.atProfile_Nothing=False',1)),
 'missing_original_repeat':(P+'original-results-v2/Release/repeat/results.tsv',lambda b:b[:-1]),
 'unsourced_original':(P+'original-results-v2/Debug/plain/results.tsv',lambda b:b+b'FALSE'),
 'partial_current':(P+'current-results/Debug/results.tsv',lambda b:b[:-100]),
 'wrong_current_type':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'RETURN_TEXT',b'RETURN_ARRAY',1)),
 'wrong_current_error':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'SQkVT',b'SQkVU',1)),
 'wrong_current_bits':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'wrong_current_exception':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"error":"None"',b'"error":"Other"',1)),
 'wrong_current_ByRef':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"byref_arguments":"',b'"byref_arguments":"MUTATED',1)),
 'wrong_current_time':(P+'current-results/Debug/execution.json',lambda b:b.replace(b'"started_utc":"2026-',b'"started_utc":"2000-',1)),
 'current_only_trace':(P+'original-results-v2/Debug/trace/results.tsv.branches.json',lambda b:b'[]'),
 'different_model':(P+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'failed_fit':(P+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'"cached_result_nonnull":true',b'"cached_result_nonnull":false',1)),
 'culture_not_restored':(P+'current-results/Debug/results.tsv.cultures.json',lambda b:b.replace(b'restored_ui_culture',b'wrong_ui_culture',1)),
 'old_original_changed':('evidence/04A38/preedit/frozen-v2/original-Debug.tsv',lambda b:b+b' '),
 'old_F_changed':('evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
 'old_NOT_READY_changed':('evidence/04A38/completion/w1/readiness.json',lambda b:b.replace(b'NOT_READY',b'READY',1)),
 'old_manifest_changed':(reader.PREVIOUS,lambda b:b+b' ')
}
def make_test(path,mutate):
 def test(self):self.rejects(path,mutate)
 return test
for name,(path,mutate) in CASES.items():setattr(SupplementTests,'test_'+name,make_test(path,mutate))
if __name__=='__main__':unittest.main()
'''
with (HERE/'test_supplement.py').open('x',encoding='utf-8') as f:f.write(s)
for name in ['verify_packet.py','test_packet.py','verify_public_copy.py','run_public_copy.py']:
 text=(V6/name).read_text(encoding='utf-8')
 text=text.replace('mmrm38-v6','consolidated38-v7').replace('MMRM_INTERCEPT_GROUP_EVIDENCE_PASS_NOT_BATCH_READY','APPROVAL_G_EVIDENCE_PASS_NOT_BATCH_READY')
 with (HERE/name).open('x',encoding='utf-8') as f:f.write(text)
print('56 supplement controls and generic packet controls prepared')
