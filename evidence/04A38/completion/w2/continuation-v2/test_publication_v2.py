"""Fail-closed manifest/privacy/readiness controls; no original oracles."""
import copy,json,hashlib,unittest
from verify_publication import ROOT,HERE,inspect
BASE=HERE.relative_to(ROOT).as_posix()+'/'
MANIFEST=BASE+'publication-manifest-v2.json'
class PublicationControls(unittest.TestCase):
 def mutate_manifest(self,change):
  x=json.loads((ROOT/MANIFEST).read_bytes());change(x)
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(manifest_path=MANIFEST,overrides={MANIFEST:json.dumps(x).encode()})
 def payload(self,p,mutate):
  p=BASE+p;old=(ROOT/p).read_bytes();new=mutate(old);m=json.loads((ROOT/MANIFEST).read_bytes());row=next(r for r in m['entries'] if r['path']==p);row.update(bytes=len(new),sha256=hashlib.sha256(new).hexdigest())
  with self.assertRaises((ValueError,KeyError,StopIteration)):inspect(manifest_path=MANIFEST,overrides={p:new,MANIFEST:json.dumps(m).encode()})
 def test_positive_partial_packet(self):self.assertEqual(inspect(manifest_path=MANIFEST)['status'],'VERIFIED_PARTIAL_CONTINUATION_V2_NOT_READY')
 def test_duplicate_path(self):self.mutate_manifest(lambda x:x['entries'].append(x['entries'][0]))
 def test_casefold_collision(self):self.mutate_manifest(lambda x:x['entries'].append(dict(x['entries'][0],path=x['entries'][0]['path'].upper())))
 def test_traversal(self):self.mutate_manifest(lambda x:x['entries'][0].update(path='../escape'))
 def test_reserved_windows_name(self):self.mutate_manifest(lambda x:x['entries'][0].update(path='NUL.txt'))
 def test_extra_null(self):self.mutate_manifest(lambda x:x['entries'][0].update(bytes=None,sha256=None))
 def test_recursive_self_hash(self):
  self.mutate_manifest(lambda x:next(r for r in x['entries'] if r['path']==MANIFEST).update(bytes=1,sha256='0'*64))
 def test_hash_mismatch(self):self.mutate_manifest(lambda x:x['entries'][0].update(sha256='0'*64))
 def test_truncated_manifest_inventory(self):self.mutate_manifest(lambda x:x['entries'].pop(0))
 def test_false_ready_even_with_rehashed_payload(self):
  self.payload('readiness.json',lambda b:json.dumps(dict(json.loads(b),status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW')).encode())
 def test_scientific_promotion(self):
  def change(b):
   x=json.loads(b);x['gates']['P']='YES';return json.dumps(x).encode()
  self.payload('readiness.json',change)
 def test_private_path_even_with_rehashed_payload(self):
  self.payload('owner-review.md',lambda b:b+('/'.join(['C:','Users','SYNTHETIC_PRIVATE','case'])).encode())
 def test_hidden_inherited_nonpass(self):
  def change(b):
   x=json.loads(b);x['validation']['managed_Debug']['Failed']=0;return json.dumps(x).encode()
  self.payload('readiness.json',change)
 def test_forged_historical_host_only_with_rehash(self):
  def change(b):
   x=json.loads(b);next(r for r in x['members'] if not r['proof'])['proof']={'kind':'HOST_ONLY'};return json.dumps(x).encode()
  self.payload('historical-ownership-v2.json',change)
if __name__=='__main__':unittest.main(verbosity=2)
