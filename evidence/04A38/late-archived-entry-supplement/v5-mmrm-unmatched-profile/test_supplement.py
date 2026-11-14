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
 def test_intact(self):self.assertEqual('MMRM_UNMATCHED_PROFILE_EVIDENCE_PASS_NOT_BATCH_READY',reader.inspect(self.view)['status'])

def alter_json(key,value):
 return lambda data:(json.dumps(dict(json.loads(data),**{key:value}))+'\n').encode()
def alter_input(operation):
 def mutate(data):
  rows=[json.loads(line) for line in data.splitlines()];operation(rows)
  return b''.join((json.dumps(row,separators=(',',':'))+'\n').encode() for row in rows)
 return mutate

P=reader.PREFIX
CASES={
 'missing_owner_approval':(P+'owner-authorization.json',alter_json('owner_quote','UNAPPROVED')),
 'wrong_approval_date':(P+'owner-authorization.json',alter_json('owner_decision_date','2026-11-08')),
 'wrong_case_scope':(P+'owner-authorization.json',alter_json('cases_per_configuration',5)),
 'unsealed_input':(P+'input-seal.json',alter_json('status','UNSEALED')),
 'wrong_original_commit':(P+'input-seal.json',alter_json('source_commit','0'*40)),
 'preedit_relabel':(P+'original-output-seal.json',alter_json('reference_class','PRE_EDIT_FROZEN')),
 'postcomparison_original_seal':(P+'original-output-seal.json',alter_json('current_executions',2)),
 'changed_output_seal_time':(P+'original-output-seal.json',alter_json('sealed_utc','2000-01-01T00:00:00+00:00')),
 'third_case':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows.append(rows[0]))),
 'missing_case':(P+'inputs-Release.jsonl',alter_input(lambda rows:rows.pop())),
 'reordered_cases':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows.reverse())),
 'wrong_case_id':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0].update(case_id='UNAPPROVED'))),
 'wrong_culture':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0].update(culture='en-US'))),
 'wrong_ui_culture':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0].update(ui_culture='sk-SK'))),
 'wrong_configuration':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0].update(configuration='Release'))),
 'string_instead_of_object_array':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value'].update(element_type='System.String'))),
 'jagged_array':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value'].update(rank=1))),
 'one_based_array':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value'].update(lower_bounds=[1,1]))),
 'wrong_shape':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value'].update(lengths=[1,2]))),
 'lost_literal_header':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value']['values'][0].update(value='treatment_active'))),
 'locale_numeric_string':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value']['values'][1].update(kind='String',value='1,0'))),
 'changed_double_bit':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][1]['value']['values'][1].update(bits='4058C00000000001'))),
 'wrong_argument_position':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'].reverse())),
 'synthetic_handle':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0]['operations'][1]['arguments'][0].update(value=dict(kind='String',value='MMRM:fake')))),
 'lost_fit_context':(P+'inputs-Debug.jsonl',alter_input(lambda rows:rows[0].pop('session'))),
 'changed_fit_factory':(P+'factory-inputs.json',alter_json('fit_arguments',[])),
 'changed_spec':(P+'factory-inputs.json',alter_json('spec',{'kind':'Nothing'})),
 'wrong_original_assembly':(P+'original-results/Debug/plain/execution.json',lambda b:b.replace(b'"sha256":"',b'"sha256":"0',1)),
 'current_source_contamination':(P+'compile-original-Debug-plain.json',lambda b:b.replace(b'<ORIGINAL_TREE>',b'<CURRENT_TREE>',1)),
 'changed_source_map':(P+'trace-plan.json',alter_json('source_map',[])),
 'fabricated_coverage_flags':(P+'original-coverage.json',alter_json('configurations',{})),
 'unsourced_original_result':(P+'original-results/Debug/plain/results.tsv',lambda b:b+b'FABRICATED'),
 'changed_repeat_result':(P+'original-results/Release/repeat/results.tsv',lambda b:b[:-1]),
 'headerless_as_headered':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'headerless.attempted=False',b'headerless.attempted=True',1)),
 'partial_current_result':(P+'current-results/Debug/results.tsv',lambda b:b[:-100]),
 'changed_current_double_bits':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'changed_current_bounds':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'System.Object[0:2,0:1]',b'System.Object[1:2,0:1]',1)),
 'changed_current_error_kind':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'RETURN_TEXT',b'RETURN_ERROR_CELL',1)),
 'changed_current_exception':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"error":"None"',b'"error":"DifferentException"',1)),
 'changed_current_byref':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'"byref_arguments":"',b'"byref_arguments":"MUTATED',1)),
 'changed_current_trace':(P+'current-results/Release/results.tsv.branches.json',lambda b:b'[]'),
 'current_before_seal':(P+'current-results/Debug/execution.json',lambda b:b.replace(b'"started_utc":"2026-',b'"started_utc":"2000-',1)),
 'changed_current_fitted_design':(P+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'D3FF',b'D3FE',1)),
 'old_not_ready_rewritten':('evidence/04A38/late-archived-entry-supplement/v2-gee-profile/readiness-closeout-v1.json',lambda b:b.replace(b'NOT_READY',b'READY')),
 'old_preedit_rewritten':('evidence/04A38/preedit/frozen-v2/original-Debug.tsv',lambda b:b+b' '),
 'old432_rewritten':('evidence/04A38/late-archived-entry-supplement/v1/original-results/Debug/plain-v2/results.tsv',lambda b:b+b' '),
 'oldGEE_rewritten':('evidence/04A38/late-archived-entry-supplement/v2-gee-profile/original-results/Debug/plain-v2/results.tsv',lambda b:b+b' '),
 'changed_public_guard_message':(P+'current-results/Debug/results.tsv',lambda b:b.replace(b'SQkVT',b'SQkVU',1)),
 'old_v3_positive_rewritten':('evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
 'old_manifest_rewritten':(reader.PREVIOUS,lambda b:b+b' '),
 'old_v4_duplicate_rewritten':('evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/original-results/Debug/plain/results.tsv',lambda b:b+b' '),
 'unmatched_true_arm_missing':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.unmatched=True',b'guard.unmatched=False',1)),
 'mask_one_entry_true':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'B0|B0',b'B1|B0',1)),
 'mask_incorrect_length':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'System.Boolean[0:54]',b'System.Boolean[0:53]',1)),
 'mask_one_based':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'System.Boolean[0:54]',b'System.Boolean[1:54]',1)),
 'matched_count_fabricated':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'guard.matched_count=0',b'guard.matched_count=1',1)),
 'row53_omitted':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'row.index=53',b'row.index=52',1)),
 'row_visit_wrong':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'row.visit=D3FF',b'row.visit=D3FE',1)),
 'profile_not_formed':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'profile.formed=True',b'profile.formed=False',1)),
 'range_not_recognized':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'public.range=True',b'public.range=False',1)),
 'wide_as_headered':(P+'original-results/Debug/trace/results.tsv.branches.json',lambda b:b.replace(b'branch.wide=True',b'branch.headered_name_value=True',1)),
 'byref_description_not_null':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'public.description=N',b'public.description=EMPTY',1)),
 'restored_ui_culture_changed':(P+'current-results/Debug/results.tsv.cultures.json',lambda b:b.replace(b'restored_ui_culture',b'wrong_restored_ui_culture',1)),
 'guard_as_report':(P+'current-results/Debug/results.tsv.branches.json',lambda b:b.replace(b'guard.unmatched=True',b'report.continued=True',1))

}
def make_test(path,mutate):
 def test(self):self.rejects(path,mutate)
 return test
for name,(path,mutate) in CASES.items():setattr(SupplementTests,'test_'+name,make_test(path,mutate))
if __name__=='__main__':unittest.main()
