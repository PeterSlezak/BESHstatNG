import json,unittest
from verify_scope_addendum_v2 import ROOT,HERE,inspect
BASE=HERE.relative_to(ROOT).as_posix()+'/'
class ScopeControls(unittest.TestCase):
 def mutate(self,p,fn):
  p=BASE+p;x=json.loads((ROOT/p).read_bytes());fn(x)
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(overrides={p:json.dumps(x).encode()})
 def test_positive(self):self.assertEqual(inspect()['host_context'][0]['unchanged_full_method_records'],33)
 def test_missing_original_branch(self):self.mutate('original-fragment-parent-crosswalk.json',lambda x:x['rows'].pop())
 def test_false_parent_closure(self):self.mutate('original-fragment-parent-crosswalk.json',lambda x:next(r for r in x['rows'] if r['status']=='OPEN').update(status='COVERED_BY_PROVEN_WHOLE_PARENT'))
 def test_wrong_count(self):self.mutate('fragment-coverage-addendum.json',lambda x:x['current'].update(branches=1))
 def test_missing_static_initializer(self):self.mutate('metadata-v5/names-v3/current-Debug.json',lambda x:x.update(methods=[r for r in x['methods'] if r['member_name']!='.cctor']))
 def test_host_method_body(self):
  def change(x):
   m=next(r for r in x['methods'] if r['member_name']=='IsMissingArg');m['instructions'][0]['op']='throw'
  self.mutate('metadata-v5/names-v3/current-Debug.json',change)
 def test_host_fields(self):
  def change(x):
   t=next(r for r in x['types'] if r['type'].startswith('BESHStatNG.WorksheetFunctions.ExcelArgPredicates'));t['fields']=[];t['base_type']='System.String'
  self.mutate('metadata-v5/names-v3/current-Debug.json',change)
if __name__=='__main__':unittest.main(verbosity=2)
