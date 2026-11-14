import contextlib,json,os,unittest,uuid
from pathlib import Path
import verify_publication_v2 as reader
reader.SELF=reader.PREFIX+'publication-manifest-v3.json'
class PublicationTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=Path('\\\\?\\'+str(reader.ROOT/'.test-rationalization-04A38.tmp'/('w2c1-public-adverse-'+uuid.uuid4().hex[:8])));cls.view.mkdir()
  for row in reader.load(reader.ROOT/reader.SELF)['entries']:
   target=cls.view/row['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/row['path'],target)
  reader.inspect(cls.view)
 @contextlib.contextmanager
 def change(self,path,mutation):
  p=self.view/path;old=p.read_bytes();new=mutation(old);self.assertNotEqual(new,old,'Invalid fixture')
  temp=self.view/'replacement';temp.write_bytes(new);os.replace(temp,p)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,p)
 def manifest_rejects(self,mutate):
  def raw(b):
   d=json.loads(b);mutate(d);return json.dumps(d).encode()
  with self.change(reader.SELF,raw):
   with self.assertRaises((ValueError,OSError,KeyError,TypeError,StopIteration)):reader.inspect(self.view)
 def test_intact(self):self.assertEqual('VALID_PARTIAL_W2_CONTINUATION_NOT_READY',reader.inspect(self.view)['status'])
 def test_ready_promotion(self):self.manifest_rejects(lambda d:d.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_duplicate(self):self.manifest_rejects(lambda d:d['entries'].append(d['entries'][0]))
 def test_casefold_collision(self):
  def mutate(d):
   r=dict(d['entries'][0]);r['path']=r['path'].upper();d['entries'].append(r);d['entries'].sort(key=lambda r:r['path'])
  self.manifest_rejects(mutate)
 def test_traversal(self):self.manifest_rejects(lambda d:d['entries'][0].update(path='../escape'))
 def test_reserved_name(self):self.manifest_rejects(lambda d:d['entries'][0].update(path='evidence/CON.txt'))
 def test_null_payload(self):self.manifest_rejects(lambda d:d['entries'][0].update(bytes=None,sha256=None))
 def test_recursive_self(self):
  def mutate(d):next(r for r in d['entries'] if r['path']==reader.SELF).update(bytes=1,sha256='0'*64)
  self.manifest_rejects(mutate)
 def test_missing_payload(self):self.manifest_rejects(lambda d:d['entries'][0].update(path='evidence/nonexistent.txt'))
 def test_manifest_truncation(self):self.manifest_rejects(lambda d:d['entries'].pop(0))
 def rehashed_mutation(self,path,mutate):
  old=(self.view/path).read_bytes();new=mutate(old)
  with self.change(path,lambda b:new):
   def altered(b):
    d=json.loads(b);r=next(r for r in d['entries'] if r['path']==path);r.update(bytes=len(new),sha256=reader.sha(new));return json.dumps(d).encode()
   with self.change(reader.SELF,altered):
    with self.assertRaises((ValueError,OSError,KeyError,TypeError,StopIteration)):reader.inspect(self.view)
 def test_self_consistent_forged_ownership(self):
  def mutate(b):
   d=json.loads(b);r=next(r for r in d['members'] if not r['proof']);r['proof']={'kind':'HOST_ONLY'};return json.dumps(d).encode()
  self.rehashed_mutation(reader.PREFIX+'historical-ownership.json',mutate)
 def test_self_consistent_native_promotion(self):
  def mutate(b):
   d=json.loads(b);d['gates']['P']='YES';return json.dumps(d).encode()
  self.rehashed_mutation(reader.PREFIX+'readiness-closeout-v2.json',mutate)
 def test_self_consistent_privacy_leak(self):self.rehashed_mutation(reader.PREFIX+'remaining-boundary-review.json',lambda b:b+b'\n'+b'C:'+bytes([92])+b'Users'+bytes([92])+b'EXAMPLE_PRIVATE'+bytes([92])+b'workbook.xlsx')
 def test_prior_manifest_mutation(self):self.rehashed_mutation('evidence/04A38/completion/w2/publication-manifest-closeout-v2.json',lambda b:b+b' ')
 def test_reviewed_synthetic_source_hash_cannot_be_extended(self):
  self.rehashed_mutation(reader.PREFIX+'test_publication.py',lambda b:b+b'\n'+b'C:'+bytes([92])+b'Users'+bytes([92])+b'ANOTHER_PRIVATE'+bytes([92])+b'workbook.xlsx')
if __name__=='__main__':unittest.main()
