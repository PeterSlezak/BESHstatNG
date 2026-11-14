"""Final publication corruption controls on a complete intact-verified copy."""
import json,os,unittest,uuid
import verify_publication as reader
class PublicationTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('w1-pub-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  rows=json.loads((reader.ROOT/(reader.W+'publication-manifest-v1.json')).read_bytes())['entries']
  for row in rows:
   target=cls.view/row['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/row['path'],target)
  if reader.inspect(cls.view)['status']!='VALID_NOT_READY_W1_CONSOLIDATED_PUBLICATION':raise AssertionError('Incomplete fixture')
 def change(self,op):
  target=self.view/(reader.W+'publication-manifest-v1.json');old=target.read_bytes();obj=json.loads(old);op(obj);temp=self.view/'replacement'
  temp.write_text(json.dumps(obj)+'\n',encoding='utf-8');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError,KeyError,TypeError)):reader.inspect(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect(self.view)['status'],'VALID_NOT_READY_W1_CONSOLIDATED_PUBLICATION')
 def test_duplicate(self):self.change(lambda r:r['entries'].append(r['entries'][0]))
 def test_casefold_collision(self):self.change(lambda r:r['entries'].append(dict(r['entries'][0],path=r['entries'][0]['path'].swapcase())))
 def test_escape(self):self.change(lambda r:r['entries'][0].update(path='../escape'))
 def test_missing_payload(self):self.change(lambda r:r['entries'].pop(0))
 def test_wrong_digest(self):self.change(lambda r:r['entries'][0].update(sha256='0'*64))
 def test_nonnull_self(self):self.change(lambda r:next(e for e in r['entries'] if e['path']==reader.W+'publication-manifest-v1.json').update(bytes=0,sha256='0'*64))
 def test_forged_ready(self):self.change(lambda r:r.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_second_null(self):self.change(lambda r:r['entries'][0].update(bytes=None,sha256=None))
 def test_private_path(self):self.change(lambda r:r['entries'][0].update(path='.git/config'))
 def test_reserved_path(self):self.change(lambda r:r['entries'][0].update(path='evidence/CON.json'))
 def test_missing_current_source(self):self.change(lambda r:r['entries'].__setitem__(slice(None),[e for e in r['entries'] if e['path']!='BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb']))
if __name__=='__main__':unittest.main()
