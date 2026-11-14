"""Adverse integrity-reader tests; isolated append-only private fixtures."""
import copy,hashlib,json
from pathlib import Path
import unittest,uuid
import verify_review_packet as reader

class ReviewPacketAdverseTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.root=Path(__file__).resolve().parents[2]/'.test-rationalization-04A38.tmp'/('review-reader-adverse-'+uuid.uuid4().hex)
  cls.root.mkdir(parents=True,exist_ok=False)
  cls.path='evidence/04A38/payload.txt'
  target=cls.root/cls.path;target.parent.mkdir(parents=True);target.write_bytes(b'original\x00bytes\n')
  cls.original=target.read_bytes()
  cls.base={'status':'NOT_READY_04A38','entries':[{'path':cls.path,'bytes':len(cls.original),'sha256':hashlib.sha256(cls.original).hexdigest()},{'path':reader.SELF,'bytes':None,'sha256':None}]}
  cls.base['entries'].sort(key=lambda row:row['path'])
 def doc(self):return copy.deepcopy(self.base)
 def payload(self,doc):return next(r for r in doc['entries'] if r['path']!=reader.SELF)
 def rejected(self,doc):
  with self.assertRaises(reader.InvalidPacket):reader.check_entries(self.root,doc)
 def test_valid_integrity_is_not_an_acceptance_claim(self):
  self.assertEqual(1,reader.check_entries(self.root,self.doc())['external_files'])
 def test_missing_self(self):
  x=self.doc();x['entries']=[r for r in x['entries'] if r['path']!=reader.SELF];self.rejected(x)
 def test_duplicate_self(self):
  x=self.doc();x['entries'].append(copy.deepcopy(x['entries'][-1]));x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_nonnull_self(self):
  x=self.doc();next(r for r in x['entries'] if r['path']==reader.SELF)['bytes']=0;self.rejected(x)
 def test_duplicate_payload(self):
  x=self.doc();x['entries'].append(copy.deepcopy(self.payload(x)));x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_casefold_collision(self):
  x=self.doc();r=copy.deepcopy(self.payload(x));r['path']='evidence/04A38/PAYLOAD.txt';x['entries'].append(r);x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_reordered(self):
  x=self.doc();x['entries'].reverse();self.rejected(x)
 def test_missing_payload(self):
  x=self.doc();self.payload(x)['path']='evidence/04A38/absent.txt';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_false_size(self):
  x=self.doc();self.payload(x)['bytes']+=1;self.rejected(x)
 def test_false_digest(self):
  x=self.doc();self.payload(x)['sha256']='0'*64;self.rejected(x)
 def test_boolean_size(self):
  x=self.doc();self.payload(x)['bytes']=True;self.rejected(x)
 def test_malformed_digest(self):
  x=self.doc();self.payload(x)['sha256']='XYZ';self.rejected(x)
 def test_missing_digest(self):
  x=self.doc();del self.payload(x)['sha256'];self.rejected(x)
 def test_traversal(self):
  x=self.doc();self.payload(x)['path']='evidence/04A38/../outside.txt';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_private_drive_path(self):
  x=self.doc();self.payload(x)['path']='Z:/private/payload.txt';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_foreign_owner_document(self):
  x=self.doc();self.payload(x)['path']='planning/BESHStatNG_CrossPlatform_MasterPlan.md';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_reserved_windows_name(self):
  x=self.doc();self.payload(x)['path']='evidence/04A38/aux.txt';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_backslash_path(self):
  x=self.doc();self.payload(x)['path']='evidence\\04A38\\payload.txt';x['entries'].sort(key=lambda r:r['path']);self.rejected(x)
 def test_ready_claim_fails_closed_even_with_intact_payload(self):
  x=self.doc();x['status']='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW';self.rejected(x)
 def test_repeated_json_key(self):
  p=self.root/('malformed-'+uuid.uuid4().hex+'.json');p.write_text('{"status":1,"status":2}',encoding='utf-8')
  with self.assertRaises(reader.InvalidPacket):reader.read_json(p)
 @classmethod
 def tearDownClass(cls):
  assert (cls.root/cls.path).read_bytes()==cls.original

if __name__=='__main__':unittest.main()
