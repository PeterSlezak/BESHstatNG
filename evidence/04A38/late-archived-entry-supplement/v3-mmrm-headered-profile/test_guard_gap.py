"""Independent adverse tests for the sourced stop decision; no reference runs."""
import json,os,unittest,uuid
import verify_guard_gap as reader
from verify_supplement import PREVIOUS

class GuardGapTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('m38-g-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={r['path'] for r in json.loads((reader.ROOT/PREVIOUS).read_bytes())['entries']}
  paths.update(p.relative_to(reader.ROOT).as_posix() for p in (reader.ROOT/reader.PREFIX).rglob('*') if p.is_file())
  for p in paths:
   target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/p,target)
 def change(self,operation):
  target=self.view/reader.REPORT;old=target.read_bytes();obj=json.loads(old);operation(obj);temp=self.view/'replacement'
  temp.write_text(json.dumps(obj)+'\n',encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_NOT_READY_MMRM_DUPLICATE_REFERENCE_GAP')
 def test_forged_ready(self):self.change(lambda r:r.update(status='READY'))
 def test_fake_guard_count(self):self.change(lambda r:r['configurations']['Debug'].update(original_duplicate_guard_outputs=1))
 def test_headered_positive_as_guard(self):self.change(lambda r:r['configurations']['Release'].update(approved_headered_duplicate_guard_witnesses=2))
 def test_lost_witness(self):self.change(lambda r:r['witnesses'].pop())
 def test_duplicate_flag_fabricated(self):self.change(lambda r:r['witnesses'][0].update(duplicate_candidate=True))
 def test_wrong_source_line(self):self.change(lambda r:r['missing_source_branches'][0].update(line=727))
 def test_invented_expected_output(self):self.change(lambda r:r.update(expected_results='PASS'))
 def test_unapproved_new_reference(self):self.change(lambda r:r.update(new_invalid_profile_executions=1))
 def test_production_past_stop(self):self.change(lambda r:r.update(new_production_changes=1))
if __name__=='__main__':unittest.main()
