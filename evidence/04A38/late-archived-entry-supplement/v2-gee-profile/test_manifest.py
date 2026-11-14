"""Independent external publication mutation tests; no original writes."""
import contextlib,json,os,unittest,uuid
import verify_gee as reader

class ManifestTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('g38-m-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  for row in json.loads((reader.ROOT/reader.SELF).read_bytes())['entries']:
   p=row['path'];source=reader.ROOT/p;target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(source,target)
 @contextlib.contextmanager
 def changed(self,p,mutate):
  target=self.view/p;old=target.read_bytes();temp=self.view/'mutation'
  temp.write_bytes(mutate(old));os.replace(temp,target)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,target)
 def rejects(self,p,mutate):
  with self.changed(p,mutate):
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.verify_publication(self.view)
 def manifest(self,operation):
  def mutate(b):
   m=json.loads(b);operation(m);return (json.dumps(m)+'\n').encode()
  self.rejects(reader.SELF,mutate)
 def test_intact(self):self.assertEqual(reader.verify_publication(self.view)['status'],'VALID_NOT_READY_GEE_PUBLICATION')
 def test_missing_payload(self):self.manifest(lambda m:m['entries'].pop(0))
 def test_duplicate(self):self.manifest(lambda m:m['entries'].append(m['entries'][0]))
 def test_casefold_collision(self):self.manifest(lambda m:m['entries'].append(dict(m['entries'][0],path=m['entries'][0]['path'].swapcase())))
 def test_manifest_reordered(self):self.manifest(lambda m:m['entries'].reverse())
 def test_path_escape(self):self.manifest(lambda m:m['entries'][0].update(path='../escape'))
 def test_absolute_path(self):self.manifest(lambda m:m['entries'][0].update(path='/escape'))
 def test_windows_path(self):self.manifest(lambda m:m['entries'][0].update(path='Q:/escape'))
 def test_reserved_path(self):self.manifest(lambda m:m['entries'][0].update(path='evidence/CON.json'))
 def test_private_payload(self):self.manifest(lambda m:m['entries'][0].update(path='.git/config'))
 def test_wrong_size(self):self.manifest(lambda m:m['entries'][0].update(bytes=0))
 def test_wrong_digest(self):self.manifest(lambda m:m['entries'][0].update(sha256='0'*64))
 def test_missing_ignored_capture(self):self.manifest(lambda m:m['entries'].__setitem__(slice(None),[r for r in m['entries'] if r['path']!=reader.PREFIX+'current-results/Release/results.tsv']))
 def test_missing_self(self):self.manifest(lambda m:m['entries'].__setitem__(slice(None),[r for r in m['entries'] if r['path']!=reader.SELF]))
 def test_nonnull_self(self):self.manifest(lambda m:next(r for r in m['entries'] if r['path']==reader.SELF).update(bytes=0,sha256='0'*64))
 def test_forged_ready(self):self.manifest(lambda m:m.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_staged_git_receipt(self):self.rejects(reader.PREFIX+'git-final.json',lambda b:b.replace(b'EMPTY',b'STAGED'))
 def test_changed_git_commit(self):self.rejects(reader.PREFIX+'git-final.json',lambda b:b.replace(reader.BASE.encode(),b'0'*40))
 def test_whitespace_failure(self):self.rejects(reader.PREFIX+'git-final.json',lambda b:b.replace(b'"diff_check_exit": 0',b'"diff_check_exit": 1'))
 def test_lost_inventory(self):self.rejects(reader.PREFIX+'file-inventory.json',lambda b:b'{}')
if __name__=='__main__':unittest.main()
