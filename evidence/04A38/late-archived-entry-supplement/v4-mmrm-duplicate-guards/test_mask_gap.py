"""Private-copy adverse checks of the independent stop-decision reader."""
import json,os,unittest,uuid
import verify_mask_gap as reader
from verify_supplement import PREVIOUS

class MaskGapTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('m4-mask-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
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
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_NOT_READY_EMPTY_MMRM_MASK_REFERENCE_GAP')
 def test_false_ready(self):self.change(lambda r:r.update(status='READY'))
 def test_report_as_empty_guard(self):self.change(lambda r:r['configurations']['Debug'].update(no_matching_rows_guard_outputs=1))
 def test_borrowed_estimate_guard(self):self.change(lambda r:r['configurations']['Release'].update(late_duplicate_lsmeans_mask_cases=4))
 def test_lost_original_witness(self):self.change(lambda r:r['witnesses'].pop())
 def test_wrong_original_line(self):self.change(lambda r:r['missing_source_branch'].update(line=1425))
 def test_wrong_member_identity(self):self.change(lambda r:r.update(member_id='UNRELATED'))
 def test_invented_reference_output(self):self.change(lambda r:r.update(expected_output='PASS'))
 def test_unapproved_reference(self):self.change(lambda r:r.update(new_reference_executions=1))
 def test_production_after_stop(self):self.change(lambda r:r.update(new_production_changes=1))
if __name__=='__main__':unittest.main()
