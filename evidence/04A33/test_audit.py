"""Adversarial receipts; no fixture, historical or product mutation."""
import unittest,copy,hashlib
from unittest.mock import patch
import verify_manifest as v
from audit import ROOT,HERE,CORE,OWNER_PLAN,OWNER_PLAN_SHA,pin,load,validate_transition,validate_identity,validate_gates
class Graphics33EvidenceTests(unittest.TestCase):
 def fixture(self):return [pin(CORE[0]),dict(path=v.SELF,bytes=None,sha256=None)]
 def check(self,rows,scope=None):v.validate_rows(rows,{r['path'] for r in rows} if scope is None else scope)
 def test_exact_bytes_and_nonrecursive_self(self):self.check(self.fixture())
 def test_hash_tamper(self):
  r=self.fixture();r[0]['sha256']='0'*64
  with self.assertRaises(AssertionError):self.check(r)
 def test_size_tamper(self):
  r=self.fixture();r[0]['bytes']+=1
  with self.assertRaises(AssertionError):self.check(r)
 def test_missing_entry(self):
  r=self.fixture()
  with self.assertRaises(AssertionError):self.check(r[:-1],{x['path'] for x in r})
 def test_missing_payload(self):
  r=self.fixture();r[0]['path']='evidence/04A33/missing-never-created.txt'
  with self.assertRaises(AssertionError):self.check(r)
 def test_duplicate(self):
  r=self.fixture();r.append(copy.deepcopy(r[0]))
  with self.assertRaises(AssertionError):self.check(r)
 def test_case_alias(self):
  r=self.fixture();r.append(dict(r[0],path=r[0]['path'].lower()))
  with patch.object(v,'file_pin',side_effect=RuntimeError('No payload read permitted')):
   with self.assertRaises(AssertionError):self.check(r)
 def test_path_escape_and_noncanonical(self):
  for p in ['../escape','/absolute','drive:payload','a//b','a/./b','a\\b','']:
   r=self.fixture();r[0]['path']=p
   with self.subTest(path=p),patch.object(v,'file_pin',side_effect=RuntimeError('No payload read permitted')):
    with self.assertRaises(AssertionError):self.check(r)
 def test_recursive_self_hash(self):
  r=self.fixture();r[1]['sha256']='0'*64
  with self.assertRaises(AssertionError):self.check(r)
 def test_recursive_self_size(self):
  r=self.fixture();r[1]['bytes']=1
  with self.assertRaises(AssertionError):self.check(r)
 def test_null_nonself_hash(self):
  r=self.fixture();r[0]['sha256']=None
  with self.assertRaises(AssertionError):self.check(r)
 def test_source_transition_actual(self):validate_transition(load(HERE/'source-transition-successor.json'))
 def test_source_transition_tamper(self):
  r=load(HERE/'source-transition-successor.json');r['pins'][0]['final_04A33_sha256']='0'*64
  with self.assertRaises(AssertionError):validate_transition(r)
 def test_source_identity_reassignment(self):
  validate_identity(3,[0,1,2])
  with self.assertRaises(AssertionError):validate_identity(3,[1,0,2])
 def test_stale_gate_state(self):
  g=load(HERE/'entry-state.json')['gates'];validate_gates(g);g['C3']='APPROVED'
  with self.assertRaises(AssertionError):validate_gates(g)
 def test_false_eol_qualification(self):
  old=b'alpha\r\nbeta\r\n';merged=b'alpha\nbeta\n'
  def qualify(b):return len(b.replace(b'\n',b'\r\n'))==len(old) and hashlib.sha256(b.replace(b'\n',b'\r\n')).digest()==hashlib.sha256(old).digest()
  self.assertTrue(qualify(merged));self.assertFalse(qualify(merged.replace(b'beta',b'zeta')))
 def test_frozen_pre_edit_authority(self):
  for r in load(HERE/'pre-edit-authority.json')['files']:self.assertEqual(r,pin(r['path']))
 def test_fixture_compilation_adjustment_only(self):
  for name in ['LegacyXyz33','LegacyScatter33','LegacyWire33']:
   original=(HERE/(name+'.vb.txt')).read_text(encoding='utf-8').replace('\ufeff','').replace('Namespace Batch33Fixtures','Namespace Global.Batch33Fixtures')
   actual=(ROOT/('BESHStat.Core.Tests/src/Graphics/'+name+'.vb')).read_text(encoding='utf-8')
   self.assertEqual('Option Strict Off\n'+original,actual)
 def test_protected_history_controls_and_external_owner_plan(self):
  selected=[r for r in load(HERE/'entry-pins.json')['files'] if r['path'].startswith(('evidence/04A29/','evidence/04A30/','evidence/04A31/','evidence/04A32/')) or r['path']=='AGENTS.md']
  self.assertGreater(len(selected),100)
  for r in selected:self.assertEqual(r,pin(r['path']))
  self.assertEqual(OWNER_PLAN_SHA,pin(OWNER_PLAN)['sha256'])
  self.assertEqual(pin(OWNER_PLAN),load(HERE/'external-owner-change.json')['current'])
 def test_full_publication_if_sealed(self):
  if not (HERE/'publication-manifest.json').exists():self.skipTest('Pre-seal;post-seal check required')
  v.verify_manifest()
if __name__=='__main__':unittest.main()
