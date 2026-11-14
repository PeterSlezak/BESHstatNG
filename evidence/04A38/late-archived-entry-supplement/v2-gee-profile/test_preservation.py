"""Protected payload mutation tests; retained isolated fixture, never original writes."""
import json,os,unittest,uuid
from pathlib import Path
import verify_preservation as reader

class PreservationTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.view=reader.ROOT/'.test-rationalization-04A38.tmp'/('g38-p-'+uuid.uuid4().hex[:8]);cls.view.mkdir()
  cls.metadata='evidence/04A38/preservation-validation-v2.json'
  old=json.loads((reader.ROOT/cls.metadata).read_bytes())
  cls.selected=[r['current']['path'] for r in old['lineage_pins']]+[r['path'] for r in old['protected_mixed_model_captures']+old['owner_documents_excluded']]
  cls.lineage=old['lineage_pins'][0]['current']['path'];cls.capture=old['protected_mixed_model_captures'][0]['path'];cls.owner=old['owner_documents_excluded'][0]['path']
  cls.manifest='evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json'
  for p in set(cls.selected+[cls.metadata,cls.manifest]):
   target=cls.view/p;target.parent.mkdir(parents=True,exist_ok=True);os.link(reader.ROOT/p,target)
 def rejects(self,p):
  target=self.view/p;old=target.read_bytes();temp=self.view/'mutation'
  temp.write_bytes(old+b' ');os.replace(temp,target)
  try:
   with self.assertRaises((ValueError,OSError)):reader.inspect_protected(self.view)
  finally:temp.write_bytes(old);os.replace(temp,target)
 def test_intact(self):self.assertEqual(reader.inspect_protected(self.view),dict(lineage=278,captures=100,owner_documents=16))
 def test_lineage_changed(self):self.rejects(self.lineage)
 def test_protected_capture_changed(self):self.rejects(self.capture)
 def test_owner_document_changed(self):self.rejects(self.owner)
 def test_protection_metadata_changed(self):self.rejects(self.metadata)
 def test_protection_authority_changed(self):self.rejects(self.manifest)
if __name__=='__main__':unittest.main()
