"""Manifest adverse checks in a private hardlink view; replace before mutation."""
import contextlib,json,os,unittest,uuid
import verify_packet as reader

class PacketTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('late38-packet-'+uuid.uuid4().hex);cls.view.mkdir()
  manifest=json.loads((reader.ROOT/reader.SELF).read_bytes());cls.originals=[]
  for row in manifest['entries']:
   source=reader.ROOT/row['path'];target=cls.view/row['path'];target.parent.mkdir(parents=True,exist_ok=True);os.link(source,target)
   cls.originals.append((source,reader.sha(source.read_bytes())))
 @classmethod
 def tearDownClass(cls):
  for source,digest in cls.originals:assert reader.sha(source.read_bytes())==digest,'Original evidence altered'
 @contextlib.contextmanager
 def changed(self,path,mutate):
  target=self.view/path;old=target.read_bytes();temp=target.with_name(target.name+'.'+uuid.uuid4().hex)
  temp.write_bytes(mutate(old));os.replace(temp,target)
  try:yield
  finally:temp.write_bytes(old);os.replace(temp,target)
 def rejects(self,path,mutate):
  with self.changed(path,mutate):
   with self.assertRaises((reader.InvalidPacket,OSError,ValueError,KeyError)):reader.inspect(self.view)
 def change_manifest(self,mutate):
  def operation(data):
   obj=json.loads(data);mutate(obj);return (json.dumps(obj)+'\n').encode()
  self.rejects(reader.SELF,operation)
 def test_intact(self):self.assertEqual('VALID_NOT_READY_LATE_SUPPLEMENT_PACKET',reader.inspect(self.view)['status'])
 def test_forged_ready(self):self.change_manifest(lambda m:m.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_missing_payload(self):self.change_manifest(lambda m:m['entries'].pop(0))
 def test_duplicate(self):self.change_manifest(lambda m:m['entries'].append(m['entries'][0]))
 def test_casefold_duplicate(self):self.change_manifest(lambda m:m['entries'].append(dict(m['entries'][0],path=m['entries'][0]['path'].swapcase())))
 def test_reordered(self):self.change_manifest(lambda m:m['entries'].reverse())
 def test_traversal(self):self.change_manifest(lambda m:m['entries'][0].update(path='../escape'))
 def test_absolute(self):self.change_manifest(lambda m:m['entries'][0].update(path='/escape'))
 def test_reserved(self):self.change_manifest(lambda m:m['entries'][0].update(path='evidence/CON.json'))
 def test_recursive_self(self):self.change_manifest(lambda m:next(r for r in m['entries'] if r['path']==reader.SELF).update(bytes=0,sha256='0'*64))
 def test_altered_output(self):self.rejects(reader.PREFIX+'current-results/Debug/results.tsv',lambda b:b+b'corrupt')
 def test_altered_old_receipt(self):self.rejects('evidence/04A38/preedit/frozen-v2/receipt.json',lambda b:b+b' ')
 def test_false_body_closure(self):self.rejects(reader.PREFIX+'readiness.json',lambda b:b.replace(b'NOT_READY_04A38',b'READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'))
 def test_omitted_ignored_capture(self):self.change_manifest(lambda m:m['entries'].__setitem__(slice(None),[r for r in m['entries'] if r['path']!=reader.PREFIX+'current-results/Release/results.tsv']))

if __name__=='__main__':unittest.main()
