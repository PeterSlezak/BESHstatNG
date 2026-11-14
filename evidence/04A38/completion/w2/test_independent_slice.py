"""Temporary-copy corruption controls; no reference/model executions."""
import contextlib,json,os,unittest,uuid
from pathlib import Path
import verify_profile_slice as profile
import verify_delegated_bodies_v2 as bodies
import verify_preservation as protected

class SliceTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=profile.ROOT/'.test-rationalization-04A38.tmp'/('w2-adverse-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  paths={r['path'] for r in profile.load(profile.ROOT/profile.PREVIOUS)['entries']}
  paths.update(p.relative_to(profile.ROOT).as_posix() for p in (profile.ROOT/profile.W).rglob('*') if p.is_file())
  paths.update(r['after']['path'] for r in profile.load(profile.ROOT/(profile.W+'source-transitions.json'))['files'])
  cls.hashes={}
  for p in paths:
   src=profile.ROOT/p;dest=cls.view/p;dest.parent.mkdir(parents=True,exist_ok=True);os.link(src,dest);cls.hashes[p]=profile.digest(src.read_bytes())
 @classmethod
 def tearDownClass(cls):
  for p,sha in cls.hashes.items():
   if profile.digest((profile.ROOT/p).read_bytes())!=sha:raise AssertionError('Original source/evidence altered')
 @contextlib.contextmanager
 def mutation(self,path,change):
  dest=self.view/path;old=dest.read_bytes();new=change(old);self.assertNotEqual(old,new)
  temp=self.view/'fixture-replacement';temp.write_bytes(new);os.replace(temp,dest)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,dest)
 def rejects(self,path,change,reader):
  with self.mutation(path,change):
   with self.assertRaises((ValueError,KeyError,TypeError,OSError)):reader(self.view)
 def test_intact_source_result_and_export_proof(self):self.assertEqual(profile.inspect(self.view)['historical_whole_bodies_closed'],8)
 def test_intact_full_body_proof(self):self.assertEqual(bodies.inspect(self.view)['closed'],39)
 def test_intact_predecessor_preservation(self):self.assertEqual(protected.inspect(self.view,False)['predecessor_payloads'],1605)
 def test_weakened_multirow_guard(self):self.rejects(profile.COMMON,lambda b:b.replace(b'If dataRow >= 0 Then',b'If dataRow > 0 Then',1),profile.inspect)
 def test_missing_intercept_rejection(self):self.rejects(profile.COMMON,lambda b:b.replace(b'"(Intercept)"',b'"OTHER"',1),profile.inspect)
 def test_error_text_rewritten(self):self.rejects(profile.COMMON,lambda b:b.replace(b'at specifies time more than once.',b'different error',1),profile.inspect)
 def test_mask_order_changed(self):self.rejects(profile.CORE,lambda b:b.replace(b'rowMask(i) = keep',b'rowMask(n - 1 - i) = keep',1),profile.inspect)
 def test_average_engine_altered(self):self.rejects(profile.CORE,lambda b:b.replace(b'sums(j) /= CDbl(matchedCount)',b'sums(j) /= CDbl(matchedCount + 1)',1),profile.inspect)
 def test_bad_host_cell_policy(self):self.rejects(profile.PATHS['Gee'],lambda b:b.replace(b'AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble',b'AddressOf OtherPolicy',1),profile.inspect)
 def test_postcompile_source_edit(self):self.rejects(profile.PATHS['Gee'],lambda b:b+b'\n\x27Changed\n',bodies.inspect)
 def test_source_preimage_mutation(self):self.rejects(profile.W+'source-before/'+profile.PATHS['Mmrm'],lambda b:b+b' ',lambda r:protected.inspect(r,False))
 def test_new_unapproved_production_edit(self):self.rejects(profile.PATHS['Mmrm'],lambda b:b+b' ',lambda r:protected.inspect(r,False))
 def test_older_NOT_READY_rewritten(self):self.rejects('evidence/04A38/completion/w1/readiness.json',lambda b:b.replace(b'NOT_READY',b'READY',1),lambda r:protected.inspect(r,False))
 def test_original_output_changed(self):self.rejects('evidence/04A38/preedit/frozen-v2/original-Debug.tsv',lambda b:b+b' ',lambda r:protected.inspect(r,False))
 def test_current_output_changed(self):self.rejects(profile.W+'preedit-current-results/Debug/frozen-v2/results.tsv',lambda b:b[:-10],profile.inspect)
 def test_changed_constructor_CIL_stays_open(self):
  p=profile.W+'method-bodies-v2-current-Release.json'
  def change(b):
   rows=json.loads(b)
   row=next(r for r in rows if r['method']=='BESHStatNG.UdfDataImport..cctor()');row['instructions'][0]['op']='INVALID';return json.dumps(rows).encode()
  with self.mutation(p,change):self.assertLess(bodies.inspect(self.view)['closed'],39)
 def test_fabricated_literal_CIL_stays_open(self):
  p=profile.W+'method-bodies-v2-current-Release.json'
  def change(b):
   rows=json.loads(b);row=next(r for r in rows if '.NormalizeOptionKey(' in r['method'] and 'ExcelCommon' in r['method']);row['instructions'][0]['op']='INVALID';return json.dumps(rows).encode()
  with self.mutation(p,change):self.assertLess(bodies.inspect(self.view)['closed'],39)
if __name__=='__main__':unittest.main()
