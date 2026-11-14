"""Meaningful source/CIL/ownership mutations; no physical protected file edits."""
import copy,json,unittest
from pathlib import Path
from verify_checkpoint_v5 import ROOT,HERE,PREFIX,V5,compute,verify_edges,verify_context
from verify_source_composition_v2 import inspect as composition
from verify_text_domains_v1 import inspect as text_domains
from source_views import body as source_body

class CheckpointControls(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.baseline=compute()
  cls.history=json.loads((HERE/'historical-ownership-v1.json').read_bytes())
  cls.current=json.loads((HERE/'current-ownership-v1.json').read_bytes())
  cls.meta=json.loads((HERE/'metadata-full/families-v2/current-Debug.json').read_bytes())
  cls.original=json.loads((HERE/'metadata-full/families-v2/original-Debug.json').read_bytes())
  cls.old=json.loads((ROOT/V5/'dependency-metadata-v9/current-Debug.json').read_bytes())
 def reject(self,file,mutate,pattern=None):
  data=json.loads((HERE/file).read_bytes());mutate(data)
  with self.assertRaisesRegex((ValueError,AssertionError),pattern or '.'):
   compute(overrides={PREFIX+file:json.dumps(data,ensure_ascii=False).encode()})
 def changed_method(self,owner,name,mutate):
  data=dict(self.meta);data['methods']=list(self.meta['methods'])
  index=next(i for i,r in enumerate(data['methods']) if r['declaring_type']==owner and r['member_name']==name)
  row=copy.deepcopy(data['methods'][index]);mutate(row);data['methods'][index]=row;return data
 def test_actual_partial_positive(self):
  self.assertEqual(dict(total=704,closed=248,OPEN=456),self.baseline['historical'])
  self.assertEqual(dict(total=724,closed=266,OPEN=458),self.baseline['current'])
  self.assertEqual(64,self.baseline['support']['distinct_OPEN']);self.assertEqual('NOT_STARTED',self.baseline['W3'])
 def test_open_historical_identity(self):
  self.reject('historical-ownership-v1.json',lambda d:next(r for r in d['members'] if r['proof'] and r['proof']['kind'].startswith('C6_')).update(proof=None),'OPEN or unsupported historical')
 def test_unsupported_host_only(self):
  self.reject('historical-ownership-v1.json',lambda d:next(r for r in d['members'] if r['proof']).update(proof={'kind':'HOST_ONLY'}),'host-only')
 def test_missing_historical_identity(self):self.reject('historical-ownership-v1.json',lambda d:d['members'].pop(),'Historical identities')
 def test_changed_historical_source_identity(self):self.reject('historical-ownership-v1.json',lambda d:d['members'][0].update(git_original_id='invented'),'Historical source identity')
 def test_current_open_supported_member(self):
  self.reject('current-ownership-v1.json',lambda d:next(r for r in d['members'] if not r['review_status'].startswith('OPEN')).update(review_status='OPEN'),'OPEN or unsupported current')
 def test_current_body_metadata_forged(self):self.reject('current-ownership-v1.json',lambda d:d['members'][0].update(body_sha256='0'*64),'Current body/source metadata')
 def test_duplicate_current_identity(self):self.reject('current-ownership-v1.json',lambda d:d['members'].append(d['members'][0]),'Current population')
 def test_unsupported_container_closure(self):self.reject('current-ownership-v1.json',lambda d:next(r for r in d['support'] if not r['fully_reconciled']).update(fully_reconciled=True),'support declaration')
 def test_support_declaration_source_forged(self):self.reject('current-ownership-v1.json',lambda d:d['support'][0].update(declaration='fake'),'Support declaration source')
 def test_missing_branch_proof(self):
  self.reject('current-ownership-v1.json',lambda d:next(r for r in d['fragments'] if r['is_branch'] and r['review_status'].startswith('COVERED')).update(review_status='OPEN'),'branch/statement/inline')
 def test_missing_inline_proof(self):
  self.reject('current-ownership-v1.json',lambda d:next(r for r in d['fragments'] if r['is_inline'] and r['review_status'].startswith('COVERED')).update(review_status='OPEN'),'branch/statement/inline')
 def test_forged_fragment_branch_flag(self):self.reject('current-ownership-v1.json',lambda d:next(r for r in d['fragments'] if r['is_branch']).update(is_branch=False),'Fragment source/branch')
 def test_duplicate_fragment(self):self.reject('current-ownership-v1.json',lambda d:d['fragments'].append(d['fragments'][0]),'duplicate fragment')
 def test_original_branch_false_closure(self):
  self.reject('original-fragment-parent-crosswalk-v1.json',lambda d:next(r for r in d['rows'] if r['status']=='OPEN').update(status='COVERED_BY_PROVEN_BODY_OR_DECLARATION'),'Original branch/inline')
 def test_original_fragment_parent_forged(self):self.reject('original-fragment-parent-crosswalk-v1.json',lambda d:d['rows'][0].update(parent='invented'),'Original fragment source/parent')
 def test_false_ready(self):self.reject('ownership-summary-v1.json',lambda d:d.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'),'False READY')
 def test_forged_totals(self):self.reject('ownership-summary-v1.json',lambda d:d['historical'].update(closed=704,OPEN=0),'Forged ownership')
 def test_missing_queue_identity(self):self.reject('remaining-work-queue-v1.json',lambda d:d['members'].pop(),'canonical historical queue')
 def test_invented_priority_identity(self):self.reject('remaining-work-queue-v1.json',lambda d:d['first_members'][0].update(original_id='invented'),'canonical next action')
 def test_wrong_priority_name(self):self.reject('remaining-work-queue-v1.json',lambda d:d['first_members'][0].update(name='invented'),'canonical next action')
 def test_source_census_forged(self):self.reject('current-universe-families-v2.json',lambda d:d['members'].pop(),'Authenticated proof/census')
 def test_missing_whole_body_contract(self):self.reject('whole-body-contracts-v1.json',lambda d:d['contracts'].pop(),'Authenticated proof/census')
 def test_missing_callback_binding(self):self.reject('callback-proofs-v1.json',lambda d:d['rows'].pop(),'Authenticated proof/census')
 def test_export_signature_changed(self):
  self.reject('inspection/families-v2/Inspect38-Debug.json',lambda d:d['exports'][0]['arguments'][0].update(optional=True),'export signature')
 def test_protected_original_bytes_changed(self):
  path='evidence/04A38/preedit/entry-source-snapshot.json'
  with self.assertRaises((ValueError,AssertionError)):compute(overrides={path:(ROOT/path).read_bytes()+b'changed'})
 def test_preimage_bytes_changed(self):
  row=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['preimage'];path=row['path']
  with self.assertRaisesRegex(ValueError,'pin mismatch'):compute(overrides={path:(ROOT/path).read_bytes()+b'changed'})
 def test_current_result_bytes_changed(self):
  row=json.loads((HERE/'compatibility-families-v2.json').read_bytes())['results'][0]['current'];path=row['path']
  with self.assertRaisesRegex((ValueError,AssertionError),'pin mismatch'):compute(overrides={path:(ROOT/path).read_bytes()+b'changed'})
 def test_replay_provenance_receipt_changed(self):self.reject('compatibility-families-v2.json',lambda d:d['results'][0].update(cases=1),'replay receipt')
 def test_actual_compiled_dll_mutation(self):
  path='.test-rationalization-04A38.tmp/completion-w2-cont6/families-v2/Debug/lib/BESHStat.Excel.Common.dll'
  with self.assertRaisesRegex(ValueError,'Actual compiled DLL identity'):compute(overrides={path:(ROOT/path).read_bytes()+b'changed'})
 def test_pure_actual_consumer_edge_removed(self):
  def remove(r):r['instructions']=[i for i in r['instructions'] if not(isinstance(i['operand'],str) and '.ApplyRepeatedCorrection(' in i['operand'])]
  data=self.changed_method('BESHStatNG.WorksheetFunctions.ParametricUDFs','RMANOVA1',remove)
  with self.assertRaisesRegex(ValueError,'Broken actual portable dependency'):verify_edges(data)
 def test_pure_wrong_reset_phase_target(self):
  def alter(r):
   for i in r['instructions']:
    if isinstance(i['operand'],str) and '.TryReadCompleteNumericMatrixWithHeadersPrepared(' in i['operand']:i['operand']=i['operand'].replace('HeadersPrepared(','Headers(')
  data=self.changed_method('BESHStatNG.UdfDataImport','TryReadCompleteNumericMatrixWithHeaders',alter)
  with self.assertRaisesRegex(ValueError,'Broken actual portable dependency'):verify_edges(data)
 def test_pure_missing_source_dependency_definition(self):
  data=dict(self.meta);data['methods']=[r for r in self.meta['methods'] if not(r['declaring_type']=='BESHStatNG.ExcelCommon.UdfInferencePreparation38' and r['member_name']=='MeanEquivalenceBody')]
  with self.assertRaisesRegex(ValueError,'producer/consumer missing'):verify_edges(data)
 def test_pure_extra_box_copy(self):
  def alter(r):
   index=next(i for i,x in enumerate(r['instructions']) if x['op']=='ldarg.0')
   r['instructions'].insert(index+1,dict(op='call',operand='METHOD:System.Runtime.CompilerServices.RuntimeHelpers @ mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089.GetObjectValue(System.Object @ mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089)'))
  data=self.changed_method('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble',alter)
  with self.assertRaisesRegex(ValueError,'Unreviewed complete CIL'):verify_context(self.old,data,self.original)
 def test_pure_real_err_context_changed(self):
  def alter(r):
   index=next(i for i,x in enumerate(r['instructions']) if isinstance(x['operand'],str) and 'ProjectData' in x['operand'])
   r['instructions'][index]['operand']+='WRONG_EMBEDDED_CONTEXT'
  data=self.changed_method('BESHStatNG.WorksheetFunctions.ExcelArgNumeric','TryGetDouble',alter)
  with self.assertRaisesRegex(ValueError,'Unreviewed complete CIL'):verify_context(self.old,data,self.original)
 def test_pure_engine_field_changed(self):
  data=dict(self.meta);data['types']=list(self.meta['types']);index=next(i for i,t in enumerate(data['types']) if t['type'].endswith('OneWayRmANOVA'))
  t=copy.deepcopy(data['types'][index]);t['fields'].append(dict(name='unsupportedState'));data['types'][index]=t
  with self.assertRaisesRegex(ValueError,'fields/types'):verify_context(self.old,data,self.original)
 def test_source_composition_changed_literal(self):
  path='BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb';raw=(ROOT/path).read_bytes().replace(b'Control group',b'Changed group',1)
  with self.assertRaisesRegex(ValueError,'Source composition'):composition(overrides={path:raw})
 def test_original_source_newline_view_requires_full_hash(self):
  d=json.loads((HERE/'authenticated-review-inputs.json').read_bytes())['members'][0];record=copy.deepcopy(d['original_record']);record['source_sha256']='0'*64
  with self.assertRaisesRegex(ValueError,'authenticated source-byte view'):source_body((ROOT/d['original_source']['path']).read_bytes(),record)
 def test_generic_clr_overloads_retained(self):
  methods=[m for m in self.meta['methods'] if m['declaring_type']=='BESHStatNG.Matrix.Matrix' and m['member_name']=='array2str']
  self.assertGreater(len(methods),1);self.assertEqual(len(methods),len({r['method'] for r in methods}))
 def test_text_synthetic_adverse_results_cannot_be_erased(self):
  file='text-domains-families-v2-v6-Debug.json';data=json.loads((HERE/file).read_bytes());data['synthetic'][0]['differences']=0
  with self.assertRaisesRegex(ValueError,'synthetic adverse'):text_domains(overrides={PREFIX+file:json.dumps(data).encode()})
 def test_current_only_materializer_has_no_historical_id(self):
  rows=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];row=next(r for r in rows if r['name']=='MaterializePortableReport38')
  self.assertIsNone(row['original_id']);self.assertTrue(row['new_support'])

if __name__=='__main__':unittest.main(verbosity=2)
