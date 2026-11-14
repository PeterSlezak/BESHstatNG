"""Mutation-only independent census/gap/consumer tests; no original execution."""
import json,os,unittest,uuid
import verify_w1_v2 as reader
class W1Tests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('w1-adverse-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={r['path'] for r in json.loads((reader.ROOT/'evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/publication-manifest-closeout-v2.json').read_bytes())['entries']}
  for prefix in [reader.W,reader.V6_PREFIX]:paths.update(p.relative_to(reader.ROOT).as_posix() for p in (reader.ROOT/prefix).rglob('*') if p.is_file())
  for path in paths:
   target=cls.view/path;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/path,target)
 def change(self,name,operation):
  target=self.view/(reader.W+name);old=target.read_bytes();obj=json.loads(old);operation(obj);temp=self.view/'replacement'
  temp.write_text(json.dumps(obj)+'\n',encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_W1_F_PASS_FULL_UNIVERSE_CONSOLIDATED_STOP')
 def test_missing_member(self):self.change('current-universe.json',lambda r:r['members'].pop())
 def test_duplicate_identity(self):self.change('current-universe.json',lambda r:r['members'].__setitem__(1,r['members'][0]))
 def test_missing_branch(self):self.change('complete-branch-adequacy-matrix.json',lambda r:r['branches'].pop())
 def test_false_branch_source(self):self.change('complete-branch-adequacy-matrix.json',lambda r:r['branches'][0].update(condition='True'))
 def test_missing_support(self):self.change('current-universe.json',lambda r:r['supports'].pop())
 def test_missing_inline(self):self.change('current-member-support-inline-plan.json',lambda r:r['current_fragments'].pop())
 def test_fake_consumer(self):self.change('member-behavior-evidence-target-matrix-v3.json',lambda r:r['members'][0]['actual_portable_edges'].append({'target':'FAKE'}))
 def test_false_host_closure(self):self.change('member-behavior-evidence-target-matrix-v3.json',lambda r:r['members'][0].update(whole_body_disposition='HOST_ONLY_WITH_BOUNDARY_PROOF'))
 def test_changed_optional_default(self):self.change('compiled-current.json',lambda r:r['exports'][0]['arguments'][0].update(default_value=42))
 def test_lost_family(self):self.change('w2-implementation-schedule.json',lambda r:r['sources'].pop())
 def test_omitted_gap(self):self.change('consolidated-reference-gaps.json',lambda r:r['essential_gaps'].pop())
 def test_false_ready(self):self.change('consolidated-reference-gaps.json',lambda r:r.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_unapproved_reference(self):self.change('consolidated-reference-gaps.json',lambda r:r.update(new_reference_executions=1))
 def test_proposal_materialized(self):self.change('consolidated-reference-gaps.json',lambda r:r.update(new_runtime_input_packets=1))
 def test_scope_expansion(self):self.change('consolidated-reference-gaps.json',lambda r:r.update(proposed_cases_per_configuration=5))
 def test_fabricated_original_outcome(self):self.change('consolidated-reference-gaps.json',lambda r:r['essential_gaps'][0].update(expected_output='PASS'))
 def test_wrong_original_line(self):self.change('consolidated-reference-gaps.json',lambda r:r['essential_gaps'][0].update(original_lines=[1507]))
if __name__=='__main__':unittest.main()
