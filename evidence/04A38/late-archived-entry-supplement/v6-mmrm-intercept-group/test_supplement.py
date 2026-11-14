"""Adverse packet mutations only: never execute a third reference/input variant."""
import contextlib,json,os,unittest,uuid
import verify_supplement as reader

class SupplementTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('m38-a-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={r['path'] for r in json.loads((reader.ROOT/reader.PREVIOUS).read_bytes())['entries']}
  paths.update(p.relative_to(reader.ROOT).as_posix() for p in (reader.ROOT/reader.PREFIX).rglob('*') if p.is_file())
  cls.originals=[]
  for p in sorted(paths):
   source=reader.ROOT/p;target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(source,target)
   cls.originals.append((source,reader.digest(source.read_bytes())))
 @classmethod
 def tearDownClass(cls):
  for source,digest in cls.originals:
   if reader.digest(source.read_bytes())!=digest:raise AssertionError('Original artifact mutated')
 @contextlib.contextmanager
 def changed(self,path,mutate):
  target=self.view/path;old=target.read_bytes();new=mutate(old)
  self.assertNotEqual(old,new,'Adverse mutation did not change bytes')
  temp=self.view/('replacement-'+uuid.uuid4().hex[:8]);temp.write_bytes(new);os.replace(temp,target)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,target)
 def rejects(self,path,mutate):
  with self.changed(path,mutate):
   with self.assertRaises((reader.InvalidEvidence,OSError,KeyError,TypeError,ValueError)):reader.inspect(self.view)
 def test_intact(self):self.assertEqual('MMRM_INTERCEPT_GROUP_EVIDENCE_PASS_NOT_BATCH_READY',reader.inspect(self.view)['status'])

def alter_json(key,value):
 return lambda data:(json.dumps(dict(json.loads(data),**{key:value}))+'\n').encode()
def alter_input(operation):
 def mutate(data):
  rows=[json.loads(line) for line in data.splitlines()];operation(rows)
  return b''.join((json.dumps(row,separators=(',',':'))+'\n').encode() for row in rows)
 return mutate

P=reader.PREFIX
CASES={
 'fake_F_approval':(P+'owner-authorization.json',alter_json('approval','E')),
 'wrong_approval_quote':(P+'owner-authorization.json',alter_json('owner_quote','NOT_APPROVED')),
 'wrong_approval_date':(P+'owner-authorization.json',alter_json('owner_decision_date','2026-11-08')),
 'third_case':(P+'inputs-Debug.jsonl',alter_input(lambda r:r.append(r[0]))),
 'missing_case':(P+'inputs-Release.jsonl',alter_input(lambda r:r.pop())),
 'reordered_cases':(P+'inputs-Debug.jsonl',alter_input(lambda r:r.reverse())),
 'wrong_culture':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(culture='en-US'))),
 'wrong_ui_culture':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(ui_culture='sk-SK'))),
 'wrong_configuration':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(configuration='Release'))),
 'wrong_case_id':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].update(case_id='UNAPPROVED'))),
 'array_instead_of_scalar':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1].update(value={'kind':'Array','rank':2}))),
 'alias_not_authorized':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value'].update(value='(Intercept)'))),
 'wrong_case_literal':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][1]['value'].update(value='intercept'))),
 'alpha_not_Nothing':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][2].update(value={'kind':'String','value':''}))),
 'swapped_argument_order':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'].reverse())),
 'foreign_handle':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0]['operations'][1]['arguments'][0].update(value={'kind':'String','value':'MMRM:fake'}))),
 'changed_factory':(P+'factory-inputs.json',alter_json('fit_arguments',[])),
 'lost_model_context':(P+'inputs-Debug.jsonl',alter_input(lambda r:r[0].pop('session'))),
 'unsealed_inputs':(P+'input-seal.json',alter_json('status','UNSEALED')),
 'wrong_commit':(P+'input-seal.json',alter_json('source_commit','0'*40)),
 'preedit_relabel':(P+'original-output-seal.json',alter_json('reference_class','PRE_EDIT_FROZEN')),
 'late_original_seal':(P+'original-output-seal.json',alter_json('current_executions',1)),
 'seal_time_changed':(P+'original-output-seal.json',alter_json('sealed_utc','2000-01-01T00:00:00+00:00')),
 'wrong_original_assembly':(P+'original-results/Debug/plain/execution.json',lambda b:b.replace(b'"sha256":"',b'"sha256":"0',1)),
 'current_contamination':(P+'compile-original-Debug-plain.json',lambda b:b.replace(b'<ORIGINAL_TREE>',b'<CURRENT_TREE>',1)),
 'changed_source_map':(P+'trace-plan.json',alter_json('source_map',[])),
 'fabricated_coverage':(P+'original-coverage.json',alter_json('configurations',{})),
 'unsourced_original':(P+'original-results/Debug/plain/results.tsv',lambda b:b+b'FALSE'),
 'missing_repeat':(P+'original-results/Release/repeat/results.tsv',lambda b:b[:-1]),
 'missing_true_guard':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.intercept=True',b'guard.intercept=False',1)),
 'wrong_resolved_index':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'lookup.index=0',b'lookup.index=-1',1)),
 'wrong_resolved_name':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'lookup.resolved_name=Intercept',b'lookup.resolved_name=treatment_active',1)),
 'range_as_scalar':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'public.scalar_group=Intercept',b'public.range=True',1)),
 'early_guard_as_intercept':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.intercept=True',b'guard.unknown_column=True',1)),
 'ByRef_name_empty_not_null':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'public.resolved_name=N',b'public.resolved_name=S',1)),
 'ByRef_values_not_null':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'public.values=N',b'public.values=System.Double[0:0]{}',1)),
 'omitted_error_assignment':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.error_assigned=',b'guard.error_absent=',1)),
 'missing_False_propagation':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'public.group_rejected=True',b'public.group_rejected=False',1)),
 'partial_current_output':(P+'current-results/Debug/results.tsv',lambda b:b[:-100]),
 'wrong_current_return_kind':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'RETURN_TEXT',b'RETURN_ARRAY',1)),
 'changed_current_error_text':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'SQkVT',b'SQkVU',1)),
 'changed_current_double_bits':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'changed_current_exception':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"error":"None"',b'"error":"Other"',1)),
 'changed_current_ByRef':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"byref_arguments":"',b'"byref_arguments":"MUTATED',1)),
 'current_before_seal':(P+'current-results/Debug/execution.json',lambda b:b.replace(b'"started_utc":"2026-',b'"started_utc":"2000-',1)),
 'current_only_branch':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b'[]'),
 'wrong_saved_design':(P+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'wrong_runtime_intercept':(P+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'"intercept_indices":[0]',b'"intercept_indices":[1]',1)),
 'changed_ui_restore':(P+'current-results/Debug/results.tsv.cultures.json',lambda b:b.replace(b'restored_ui_culture',b'wrong_ui_culture',1)),
 'old_preedit_changed':('evidence/04A38/preedit/frozen-v2/original-Debug.tsv',lambda b:b+b' '),
 'old_E_changed':('evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
 'old_NOT_READY_changed':('evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/readiness-closeout-v1.json',lambda b:b.replace(b'NOT_READY',b'READY',1)),
 'old_manifest_changed':(reader.PREVIOUS,lambda b:b+b' ')
}
def make_test(path,mutate):
 def test(self):self.rejects(path,mutate)
 return test
for name,(path,mutate) in CASES.items():setattr(SupplementTests,'test_'+name,make_test(path,mutate))
if __name__=='__main__':unittest.main()
