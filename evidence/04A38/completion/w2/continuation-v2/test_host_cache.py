import json,unittest
from verify_host_cache_v2 import inspect,ROOT,HERE
BASE=HERE.relative_to(ROOT).as_posix()+'/'
class CacheControls(unittest.TestCase):
 def mutation(self,change):
  p=BASE+'metadata/b1-v1/current-Debug.json';x=json.loads((ROOT/p).read_bytes());change(x)
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(overrides={p:json.dumps(x).encode()})
 def test_actual_scope(self):
  r=inspect();self.assertEqual(33,len(r['methods']));self.assertEqual(21,len(r['initializers']))
 def test_missing_cctor(self):self.mutation(lambda x:x.update(methods=[r for r in x['methods'] if r['member_name']!='.cctor']))
 def test_forged_static_context(self):self.mutation(lambda x:next(r for r in x['methods'] if r['member_name']=='.cctor')['instructions'].pop())
 def test_changed_cache_comparer(self):
  def change(x):
   for m in x['methods']:
    if m['member_name']=='.cctor':
     for i in m['instructions']:
      if isinstance(i['operand'],str):i['operand']=i['operand'].replace('OrdinalIgnoreCase','Ordinal')
  self.mutation(change)
 def test_changed_constant_literal(self):self.mutation(lambda x:next(t for t in x['types'] if t['type'].endswith('SampleSizeUDFs'))['fields'][0].update(literal='FORGED'))
 def test_changed_ByRef_handle_write(self):self.mutation(lambda x:next(m for m in x['methods'] if m['member_name']=='TryGetPsmHandle')['instructions'].pop())
 def test_missing_exception_context(self):
  self.mutation(lambda x:next(m for m in x['methods'] if m['member_name']=='PS_CLEANUP').update(exceptions=[]))
 def test_wrong_original_scope(self):self.mutation(lambda x:x['scope'].remove('BESHStatNG.UDFhelpers'))
if __name__=='__main__':unittest.main(verbosity=2)
