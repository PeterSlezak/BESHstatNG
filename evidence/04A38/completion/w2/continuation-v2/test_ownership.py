import json,unittest
from verify_ownership_v4 import ROOT,HERE,inspect
BASE=HERE.relative_to(ROOT).as_posix()+'/'
class OwnershipControls(unittest.TestCase):
 def mutate(self,p,change):
  p=BASE+p;x=json.loads((ROOT/p).read_bytes());change(x)
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(overrides={p:json.dumps(x).encode()})
 def test_true_partial_state(self):self.assertEqual(inspect()['historical']['OPEN'],584)
 def test_missing_historical_identity(self):self.mutate('historical-ownership-v2.json',lambda x:x['members'].pop())
 def test_unsupported_host_only(self):self.mutate('historical-ownership-v2.json',lambda x:next(r for r in x['members'] if not r['proof']).update(proof={'kind':'HOST_ONLY'}))
 def test_missing_current_identity(self):self.mutate('current-ownership-v2.json',lambda x:x['members'].pop())
 def test_missing_support_identity(self):self.mutate('current-ownership-v2.json',lambda x:x['support'].pop())
 def test_duplicate_support_identity(self):self.mutate('current-ownership-v2.json',lambda x:x['support'].__setitem__(0,x['support'][1]))
 def test_wrong_inline_parent(self):
  self.mutate('current-ownership-v2.json',lambda x:next(r for r in x['fragments'] if r['review_status']=='COVERED_BY_REVIEWED_BODY').update(member_id='FORGED'))
 def test_container_closure_from_header(self):self.mutate('current-ownership-v2.json',lambda x:next(r for r in x['support'] if r['kind']=='ModuleBlock').update(fully_reconciled=True))
 def test_forged_ready(self):self.mutate('ownership-summary-v2.json',lambda x:x.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW',W2='COMPLETE',W3='COMPLETE'))
if __name__=='__main__':unittest.main(verbosity=2)
