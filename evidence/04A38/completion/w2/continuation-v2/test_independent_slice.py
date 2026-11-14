"""Mutations exercise the independent reader, never an original semantic query."""
import copy,json,unittest
from pathlib import Path
from verify_slice_v2 import inspect,ROOT,HERE,NUM,SPILL,OPT
BASE=HERE.relative_to(ROOT).as_posix()+'/'
class IndependentSliceControls(unittest.TestCase):
 def reject(self,overrides):
  with self.assertRaises((ValueError,KeyError,StopIteration,TypeError,FileNotFoundError)):inspect(overrides=overrides)
 def mutate_json(self,name,change):
  p=BASE+name;obj=json.loads((ROOT/p).read_bytes());change(obj);self.reject({p:json.dumps(obj).encode()})
 def test_positive_bounded_reader(self):self.assertEqual(inspect()['status'],'BOUNDED_V2_A_B1_PASS_NOT_BATCH_READY')
 def test_changed_original_result(self):
  r=json.loads((HERE/'compatibility-b1-v1.json').read_bytes())['results'][0]['original'];self.reject({r['path']:(ROOT/r['path']).read_bytes()+b'X'})
 def test_forged_current_equality(self):
  r=json.loads((HERE/'compatibility-b1-v1.json').read_bytes())['results'][0]['current'];self.reject({r['path']:(ROOT/r['path']).read_bytes()+b'X'})
 def test_deleted_case_count(self):self.mutate_json('compatibility-b1-v1.json',lambda x:x['results'][0].update(cases=x['results'][0]['cases']-1))
 def test_extra_original_authority(self):self.mutate_json('compatibility-b1-v1.json',lambda x:x.update(new_original_queries=1))
 def test_missing_preimage(self):
  r=json.loads((HERE/'entry.json').read_bytes())['source_before'][0]['preimage'];self.reject({r['path']:b''})
 def test_ByRef_initialization_removed(self):self.reject({'BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb':(ROOT/'BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb').read_bytes().replace(b'useMedian = False',b'useMedian = True')})
 def test_wrong_distribution_error_shape(self):self.reject({'BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb':(ROOT/'BESHstatNG/src/ExcelUDFs/DistributionUDFs.vb').read_bytes().replace(b'Return ExcelError.ExcelErrorNum',b'Return CDbl(ExcelError.ExcelErrorNum)')})
 def test_changed_spill_copy(self):self.reject({SPILL:(ROOT/SPILL).read_bytes().replace(b'out(r, c) = v',b'out(r, c) = String.Empty')})
 def test_text_policy_flag_change(self):
  p='BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb';self.reject({p:(ROOT/p).read_bytes().replace(b'CompareOptions.IgnoreWidth',b'CompareOptions.None')})
 def test_missing_metadata_lambda_site(self):
  self.mutate_json('text-call-sites.json',lambda x:x['calls'].remove(next(r for r in x['calls'] if r['lambda_span'] is not None)))
 def test_null_comparison_choice(self):
  self.mutate_json('text-call-sites.json',lambda x:next(r for r in x['calls'] if r['kind']=='EqualText').update(right_literal=None))
 def test_ignorable_comparison_choice(self):
  self.mutate_json('text-call-sites.json',lambda x:next(r for r in x['calls'] if r['kind']=='CaseToken')['choices'].__setitem__(0,'\u00ad'))
 def test_choice_order_changed(self):
  self.mutate_json('text-call-sites.json',lambda x:next(r for r in x['calls'] if r['kind']=='CaseToken')['choices'].reverse())
 def test_swapped_culture(self):self.mutate_json('text-domains-a1-v2-v2-Debug.json',lambda x:x['results'][0].update(culture='wrong'))
 def test_missing_probe_observation(self):self.mutate_json('text-domains-a1-v2-v2-Release.json',lambda x:x['results'].pop())
 def test_erased_synthetic_failures(self):self.mutate_json('text-domains-a1-v2-v2-Debug.json',lambda x:x['synthetic'][0].update(differences=0,mismatches=[]))
 def test_forged_cil_edge(self):
  def change(x):
   r=next(r for r in x['methods'] if r['member_name']=='PRTRNG' and r['declaring_type'].endswith('.DistributionUDFs'));r['instructions']=[i for i in r['instructions'] if not isinstance(i['operand'],str) or 'PrepareStudentizedRangeCdf' not in i['operand']]
  self.mutate_json('metadata/b1-v1/current-Debug.json',change)
 def test_missing_root_module(self):self.mutate_json('metadata/b1-v1/current-Release.json',lambda x:x['scope'].pop())
 def test_changed_export_default(self):
  self.mutate_json('inspection/b1-v1/Inspect38-Debug.json',lambda x:x['exports'][0].update(effective_excel_name='FORGED'))

if __name__=='__main__':unittest.main(verbosity=2)
