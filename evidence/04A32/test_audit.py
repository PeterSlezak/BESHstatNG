"""Adversarial integrity tests;no product/history mutation or native/oracle calls."""
import copy,json,unittest
from unittest.mock import patch
from pathlib import Path
import verify_manifest as verifier
from audit import ROOT,HERE,CORE_NEW,CORE_OLD,WINDOWS,sha,pin,load,public_path

class Graphics32EvidenceTests(unittest.TestCase):
 def fixture(self):return [pin(ROOT/CORE_NEW[0]),dict(path=verifier.SELF,bytes=None,sha256=None)]
 def verify(self,rows,scope=None):verifier.validate_rows(rows,set(r['path'] for r in rows) if scope is None else scope)
 def test_exact_payload_and_nonrecursive_self(self):self.verify(self.fixture())
 def test_tampered_hash_rejected(self):
  rows=self.fixture();rows[0]['sha256']='0'*64
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_tampered_size_rejected(self):
  rows=self.fixture();rows[0]['bytes']+=1
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_missing_publication_entry_rejected(self):
  rows=self.fixture()
  with self.assertRaises(AssertionError):self.verify(rows[:-1],{r['path'] for r in rows})
 def test_missing_payload_rejected(self):
  rows=self.fixture();rows[0]['path']='evidence/04A32/missing-payload-never-created.txt'
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_duplicate_rejected(self):
  rows=self.fixture();rows.append(copy.deepcopy(rows[0]))
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_windows_case_alias_rejected_before_reads(self):
  rows=self.fixture();alias=copy.deepcopy(rows[0]);alias['path']=alias['path'].lower();rows.append(alias)
  with patch.object(verifier,'file_pin',side_effect=RuntimeError('Must not read')):
   with self.assertRaises(AssertionError):self.verify(rows)
 def test_traversal_absolute_backslash_drive_and_empty_parts_rejected(self):
  for path in ['../outside','/absolute','drive:payload','a//b','a/./b','a\\b','']:
   rows=self.fixture();rows[0]['path']=path
   with patch.object(verifier,'file_pin',side_effect=RuntimeError('Must not read')):
    with self.assertRaises(AssertionError):self.verify(rows)
 def test_hashed_self_rejected(self):
  rows=self.fixture();rows[1]['sha256']='0'*64
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_sized_self_rejected(self):
  rows=self.fixture();rows[1]['bytes']=1
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_null_payload_hash_rejected(self):
  rows=self.fixture();rows[0]['sha256']=None
  with self.assertRaises(AssertionError):self.verify(rows)
 def test_gate_state_and_original_restrictions_preserved(self):
  r=load(HERE/'entry-state.json')['restrictions']
  self.assertEqual(dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN',releases=0,discharges=0),r)
  d=load(HERE/'deferred-issues.json');self.assertTrue(all(x['released']==0 and x['discharged']==0 and x['status']=='OPEN_RETAINED_RESTRICTED' for x in d['issues']))
 def test_accepted_31_source_tests_evidence_and_controls_unchanged(self):
  before=load(HERE/'entry-pins.json')['files']
  selected=[r for r in before if r['path'].startswith(('evidence/04A31/','BESHStat.Core.Tests/src/Graphics/Graphics31','BESHStatNG.Test/src/Charts/Graphics31')) or r['path'] in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','BESHstatNG/src/Graphics/ROC.vb','BESHstatNG/src/Graphics/NormalPlot.vb']]
  self.assertGreater(len(selected),30)
  for r in selected:self.assertEqual(r,pin(ROOT/r['path']))
 def test_source_lineage_and_authorized_transitions_only(self):
  v=load(HERE/'source-transition-successor.json');self.assertEqual(278,len(v['pins']));self.assertEqual(5,len(v['new_transitions']))
  for row in v['pins']:
   self.assertEqual(row['final_04A32_sha256'],sha(ROOT/row['path']))
   if row['transition']=='AUTHORIZED_04A32':self.assertIn(row['path'],WINDOWS+CORE_OLD)
 def test_unchanged_engines_and_native_boundary_source_proof(self):
  guard=load(HERE/'source-ownership.json');self.assertTrue(guard['binning_arithmetic_and_control_flow_preserved']);self.assertTrue(guard['violin_numeric_and_grouping_tokens_preserved']);self.assertTrue(guard['existing_headers_preserved']);self.assertEqual(38,guard['compiled_graphics'])
  p=load(HERE/'source-preservation.json');self.assertTrue(p['all_35_other_windows_graphics_unchanged']);self.assertTrue(p['all_17_other_core_graphics_unchanged']);self.assertTrue(p['all_100_captures_preserved'])
 def test_full_publication_if_sealed(self):
  if not (HERE/'publication-manifest.json').exists():self.skipTest('Pre-seal;post-seal execution required')
  verifier.verify_manifest()

if __name__=='__main__':unittest.main()
