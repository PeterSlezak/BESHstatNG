"""External manifest adverse fixtures; mutations replace fixture files only."""
import contextlib,json,os,unittest,uuid
import verify_publication as reader

class PacketTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('m38-p-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  for row in json.loads((reader.ROOT/reader.SELF).read_bytes())['entries']:
   p=row['path'];target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/p,target)
 def change(self,operation):
  target=self.view/reader.SELF;old=target.read_bytes();obj=json.loads(old);operation(obj);temp=self.view/'replacement'
  temp.write_text(json.dumps(obj)+'\n',encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_W2_PARTIAL_PUBLICATION_NOT_READY')
 def test_missing_payload(self):self.change(lambda m:m['entries'].pop(0))
 def test_duplicate(self):self.change(lambda m:m['entries'].append(m['entries'][0]))
 def test_casefold_duplicate(self):self.change(lambda m:m['entries'].append(dict(m['entries'][0],path=m['entries'][0]['path'].swapcase())))
 def test_wrong_order(self):self.change(lambda m:m['entries'].reverse())
 def test_path_escape(self):self.change(lambda m:m['entries'][0].update(path='../escape'))
 def test_absolute(self):self.change(lambda m:m['entries'][0].update(path='/escape'))
 def test_windows_path(self):self.change(lambda m:m['entries'][0].update(path='Q:/escape'))
 def test_private_path(self):self.change(lambda m:m['entries'][0].update(path='.git/config'))
 def test_reserved_path(self):self.change(lambda m:m['entries'][0].update(path='evidence/CON.json'))
 def test_wrong_size(self):self.change(lambda m:m['entries'][0].update(bytes=0))
 def test_wrong_digest(self):self.change(lambda m:m['entries'][0].update(sha256='0'*64))
 def test_nonnull_self(self):self.change(lambda m:next(r for r in m['entries'] if r['path']==reader.SELF).update(bytes=0,sha256='0'*64))
 def test_missing_self(self):self.change(lambda m:m['entries'].__setitem__(slice(None),[r for r in m['entries'] if r['path']!=reader.SELF]))
 def test_missing_ignored_capture(self):self.change(lambda m:m['entries'].__setitem__(slice(None),[r for r in m['entries'] if r['path']!=reader.W+'preedit-current-results/Release/frozen-v2/results.tsv']))
 def test_forged_ready(self):self.change(lambda m:m.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
if __name__=='__main__':unittest.main()
