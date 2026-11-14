"""Adverse tests for the independently verified stop decision."""
import json,os,unittest,uuid
import verify_remaining_mmrm as reader

class MmrmGapTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('g38-mg-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={r['path'] for r in json.loads((reader.ROOT/reader.PREVIOUS).read_bytes())['entries']}
  paths.add(reader.PREFIX+'remaining-mmrm-profile-coverage.json')
  for p in paths:
   target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/p,target)
 def mutate(self,operation):
  target=self.view/(reader.PREFIX+'remaining-mmrm-profile-coverage.json');old=target.read_bytes();temp=self.view/'mutation'
  obj=json.loads(old);operation(obj);temp.write_text(json.dumps(obj),encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact_stop_decision(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_NOT_READY_MMRM_REFERENCE_GAP')
 def test_forged_ready(self):self.mutate(lambda r:r.update(status='READY'))
 def test_false_headered_witness(self):self.mutate(lambda r:r['counts']['Debug'].update(headered_name_value_candidate=1))
 def test_headerless_mislabeled_headered(self):self.mutate(lambda r:r['witnesses'][0].update(headered_name_value_candidate=True))
 def test_lost_witness(self):self.mutate(lambda r:r['witnesses'].pop())
 def test_wrong_member(self):self.mutate(lambda r:r.update(member_id='UNRELATED'))
 def test_fabricated_expected_output(self):self.mutate(lambda r:r.update(expected_output='PASS'))
 def test_unapproved_capture(self):self.mutate(lambda r:r.update(new_reference_execution='EXECUTED'))
 def test_production_before_owner_decision(self):self.mutate(lambda r:r.update(production_changes_after_gee_gate=1))
if __name__=='__main__':unittest.main()
