"""Independent reader adverse tests on retained private copies.

Hardlinks are read-only fixture seeds. Every mutation first writes a NEW file
and atomically replaces the fixture path, never writing through a hardlink.
"""
import contextlib,json,os,unittest,uuid
from pathlib import Path
import verify_gee as reader

class GeeAdverseTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('g38-a-'+uuid.uuid4().hex[:8])
  cls.view.mkdir(exist_ok=False);cls.originals=[]
  paths={r['path'] for r in json.loads((reader.ROOT/reader.PREVIOUS).read_bytes())['entries']}
  paths.update(p.relative_to(reader.ROOT).as_posix() for p in (reader.ROOT/reader.PREFIX).rglob('*') if p.is_file())
  for p in sorted(paths):
   # The private raw environment log is deliberately absent in an export view.
   if p==reader.PRIVATE_ENVIRONMENT:continue
   source=reader.ROOT/p;target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(source,target)
   cls.originals.append((source,reader.digest(source.read_bytes())))
 @classmethod
 def tearDownClass(cls):
  for source,digest in cls.originals:
   if reader.digest(source.read_bytes())!=digest:raise AssertionError('Original artifact changed')
 @contextlib.contextmanager
 def changed(self,path,mutate):
  target=self.view/path;old=target.read_bytes();temp=self.view/('mutation-'+uuid.uuid4().hex)
  need=target.resolve().is_relative_to(self.view.resolve())
  if not need:raise AssertionError('Fixture escape')
  temp.write_bytes(mutate(old));os.replace(temp,target)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,target)
 def rejects(self,path,mutate):
  with self.changed(path,mutate):
   with self.assertRaises((reader.InvalidEvidence,OSError,ValueError,KeyError,TypeError)):reader.inspect(self.view)
 def jsonchange(self,name,key,value):
  self.rejects(reader.PREFIX+name,lambda b:(json.dumps(dict(json.loads(b),**{key:value}))+'\n').encode())
 def mutateinput(self,change,config='Debug'):
  def operation(b):
   rows=[json.loads(line) for line in b.splitlines()];change(rows)
   return b''.join((json.dumps(r,separators=(',',':'))+'\n').encode() for r in rows)
  self.rejects(reader.PREFIX+'inputs-'+config+'.jsonl',operation)
 def test_intact_public_packet_without_private_environment_log(self):self.assertEqual(reader.inspect(self.view)['status'],'GEE_EVIDENCE_EXACT_AND_BRANCH_PASS_NOT_BATCH_READY')
 def test_missing_separate_owner(self):self.jsonchange('owner-authorization.json','owner_quote','Yes, I approve')
 def test_wrong_owner_date(self):self.jsonchange('owner-authorization.json','owner_decision_date','2026-11-08')
 def test_wrong_case_authority(self):self.jsonchange('owner-authorization.json','cases_per_configuration',24)
 def test_thirteenth_case(self):self.mutateinput(lambda rows:rows.append(rows[0]))
 def test_missing_case(self):self.mutateinput(lambda rows:rows.pop())
 def test_reordered_cases(self):self.mutateinput(lambda rows:rows.reverse())
 def test_wrong_case_id(self):self.mutateinput(lambda rows:rows[0].update(case_id='UNAPPROVED'))
 def test_wrong_configuration(self):self.mutateinput(lambda rows:rows[0].update(configuration='Release'))
 def test_wrong_culture(self):self.mutateinput(lambda rows:rows[0].update(culture='en-US'))
 def test_string_instead_of_double(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][4]['value']['values'][-1].update(kind='String',value='1.0'))
 def test_wrong_lower_bound(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][4]['value'].update(lower_bounds=[1,0]))
 def test_wrong_shape(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][4]['value'].update(lengths=[1,2]))
 def test_jagged_array(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][4]['value'].update(rank=1,element_type='System.Object[]'))
 def test_changed_at_ieee_bits(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][4]['value']['values'][-1].update(bits='3FF0000000000001'))
 def test_lost_fitted_context(self):self.mutateinput(lambda rows:rows[0].pop('session'))
 def test_fake_handle(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'][0].update(value={'kind':'String','value':'GEE:fake'}))
 def test_changed_fit_input(self):self.mutateinput(lambda rows:rows[0]['operations'][0]['arguments'].pop())
 def test_lost_full_public_argument_order(self):self.mutateinput(lambda rows:rows[0]['operations'][1]['arguments'].reverse())
 def test_extra_public_family(self):self.mutateinput(lambda rows:rows[0]['operations'][1].update(target='BESH.REGR.LM_PRED'))
 def test_lost_seal(self):self.jsonchange('input-seal.json','status','UNSEALED')
 def test_preedit_relabel(self):self.jsonchange('original-output-seal.json','reference_class','PRE_EDIT_FROZEN')
 def test_wrong_original_commit(self):self.jsonchange('original-output-seal.json','source_commit','0'*40)
 def test_current_before_original_seal(self):self.jsonchange('original-output-seal.json','current_implementation_executed',True)
 def test_changed_original_assembly(self):self.rejects(reader.PREFIX+'original-results/Debug/plain-v2/execution.json',lambda b:b.replace(b'"sha256":"',b'"sha256":"0',1))
 def test_original_linked_new_code(self):self.jsonchange('compile-original-Debug-plain.json','current_production_assemblies_consumed',True)
 def test_unsourced_original_output(self):self.rejects(reader.PREFIX+'original-results/Debug/plain-v2/results.tsv',lambda b:b+b'UNSOURCED')
 def test_modified_repeat(self):self.rejects(reader.PREFIX+'original-results/Release/repeat-v2/results.tsv',lambda b:b[:-1])
 def test_changed_source_map(self):self.jsonchange('trace-plan.json','source_map',[])
 def test_weakened_expectations(self):self.jsonchange('trace-plan.json','case_kinds',{})
 def test_fabricated_coverage_flags(self):self.jsonchange('original-coverage.json','configurations',{})
 def test_early_guard_mislabeled_profile(self):self.rejects(reader.PREFIX+'original-results/Debug/trace-v2/results.tsv.branches.json',lambda b:b.replace(b'profile.formed=True',b'profile.formed=False',1))
 def test_missing_helper_entry(self):self.rejects(reader.PREFIX+'original-results/Debug/trace-v2/results.tsv.branches.json',lambda b:b.replace(b'helper.entry=time',b'helper.omitted=time',1))
 def test_changed_duplicate_branch(self):self.rejects(reader.PREFIX+'original-results/Debug/trace-v2/results.tsv.branches.json',lambda b:b.replace(b'guard.duplicate_time=visit',b'guard.unrelated=visit',1))
 def test_current_partial_match(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b[:-100])
 def test_current_reordered(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b.splitlines(keepends=True)[0]+b''.join(reversed(b.splitlines(keepends=True)[1:])))
 def test_changed_current_outcome(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b.replace(b'RETURN_TEXT',b'RETURN_ERROR_CELL',1))
 def test_changed_current_error_text(self):self.rejects(reader.PREFIX+'current-results/Release/results.tsv',lambda b:b.replace(b'SQkVTSC',b'SQkVTSD',1))
 def test_changed_current_double_bits(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv.sessions.json',lambda b:b.replace(b'D3FF',b'D3FE',1))
 def test_changed_current_array_bounds(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b.replace(b'System.Object[0:2,0:1]',b'System.Object[1:2,0:1]',1))
 def test_changed_current_byref(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b.replace(b'"byref_arguments":"',b'"byref_arguments":"ALTERED',1))
 def test_changed_current_exception(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b.replace(b'"error":"None"',b'"error":"FORGED_EXCEPTION"',1))
 def test_changed_current_culture_label(self):self.rejects(reader.PREFIX+'current-results/Release/results.tsv.branches.json',lambda b:b.replace(b'sk-SK',b'en-US',1))
 def test_changed_current_trace(self):self.rejects(reader.PREFIX+'current-results/Release/results.tsv.branches.json',lambda b:b'[]')
 def test_current_missing_original_seal(self):self.jsonchange('current-execution-Debug.json','original_output_seal',{'sha256':'0'*64})
 def test_old_preedit_capture_mutation(self):self.rejects('evidence/04A38/preedit/frozen-v2/original-Debug.tsv',lambda b:b+b' ')
 def test_old_432_capture_mutation(self):self.rejects('evidence/04A38/late-archived-entry-supplement/v1/original-results/Debug/plain-v2/results.tsv',lambda b:b+b' ')
 def test_old_not_ready_mutation(self):self.rejects('evidence/04A38/continuation/v1/readiness.json',lambda b:b.replace(b'NOT_READY',b'READY'))
 def test_old_manifest_mutation(self):self.rejects(reader.PREVIOUS,lambda b:b+b' ')
 def test_archived_snapshot_mutation(self):self.rejects('evidence/04A38/preedit/source/Regression/GeeUDFs.vb.txt',lambda b:b+b' ')
 def test_lost_original_source_provenance(self):self.jsonchange('entry-inspection-public.json','original_tree_files',[])
 def test_redaction_changes_nonprivate_source_identity(self):self.jsonchange('entry-inspection-public.json','source_commit','0'*40)

if __name__=='__main__':unittest.main()
