"""Independent closeout mutation controls; replace only private fixture files."""
import json,os,unittest,uuid
import verify_closeout as reader
class CloseoutTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('m5-close-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  rows=json.loads((reader.ROOT/(reader.PREFIX+'publication-manifest-closeout-v1.json')).read_bytes())['entries']
  for r in rows:
   p=r['path'];target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/p,target)
 def change(self,name,operation):
  target=self.view/(reader.PREFIX+name);old=target.read_bytes();obj=json.loads(old);operation(obj);temp=self.view/'replacement'
  temp.write_text(json.dumps(obj)+'\n',encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_NOT_READY_UNMATCHED_PROFILE_CLOSEOUT')
 def test_ready(self):self.change('readiness-closeout-v1.json',lambda r:r.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_false_ownership(self):self.change('readiness-closeout-v1.json',lambda r:r.update(ownership_closed=True))
 def test_ready_verifier_invented(self):self.change('readiness-closeout-v1.json',lambda r:r.update(full_ready_verifier='PASS'))
 def test_wrong_blocker(self):self.change('readiness-closeout-v1.json',lambda r:r.update(earliest_blocker_member_id='UNRELATED'))
 def test_production_change(self):self.change('source-transitions.json',lambda r:r.update(new_production_transitions=['UNAPPROVED']))
 def test_extra_reference(self):self.change('readiness-closeout-v1.json',lambda r:r.update(extra_reference_executions=1))
 def test_native_promotion(self):self.change('readiness-closeout-v1.json',lambda r:r['gates'].update(N1='GO'))
 def test_staged_index(self):self.change('git-final.json',lambda r:r.update(index='STAGED'))
 def test_lost_nonpasses(self):self.change('validation-closeout-v1.json',lambda r:r['inherited_receipts_only'].update(Python='PASS'))
 def test_false_full_run(self):self.change('validation-closeout-v1.json',lambda r:r.update(full_suites='PASS'))
 def test_wrong_mask_size(self):self.change('MMRM_UNMATCHED_PROFILE_GATE_PASS.json',lambda r:r.update(mask_false_entries_per_case=53))
if __name__=='__main__':unittest.main()
