import json,hashlib,unittest
from verify_slice_v5 import ROOT,HERE,inspect
from verify_ownership_v2 import inspect as ownership
from verify_text_domains_v2 import inspect as text
BASE=HERE.relative_to(ROOT).as_posix()+'/'
class DeepControls(unittest.TestCase):
 def mutate(self,name,fn,expected,reader=inspect):
  p=BASE+name;x=json.loads((ROOT/p).read_bytes());fn(x)
  with self.assertRaisesRegex(ValueError,expected):reader(overrides={p:json.dumps(x).encode()})
 def current(self,fn,expected):self.mutate('metadata-v6/parametric-v4/current-Debug.json',fn,expected)
 def source(self,change,expected):
  p='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';r=json.loads((ROOT/(BASE+'source-transitions.json')).read_bytes());row=next(r for r in r['files'] if r['path']==p);raw=(ROOT/p).read_bytes();new=change(raw);self.assertNotEqual(raw,new);row['after'].update(bytes=len(new),sha256=hashlib.sha256(new).hexdigest());row['snapshot'].update(bytes=len(new),sha256=hashlib.sha256(new).hexdigest())
  with self.assertRaisesRegex(ValueError,expected):inspect(overrides={p:new,row['snapshot']['path']:new,BASE+'source-transitions.json':json.dumps(r).encode()})
 def test_positive(self):self.assertEqual(inspect()['new_supported_bodies'],35)
 def test_original_binary_substitution(self):self.mutate('metadata-v6/parametric-v4/original-Debug.json',lambda x:x['assemblies'][0].update(sha256='0'*64),'assembly or provenance')
 def test_current_binary_source_mismatch(self):self.current(lambda x:x['assemblies'][0].update(sha256='0'*64),'binary not built')
 def test_missing_catch(self):
  self.current(lambda x:next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.UdfLinearAlgebra' and m['member_name']=='TryInvertMatrix').update(exceptions=[]),'Catch boundary')
 def test_portable_swallowed_exception(self):
  self.current(lambda x:next(m for m in x['methods'] if m['member_name']=='TryInvertPreparedSquareMatrix').update(exceptions=[{}]),'Catch boundary')
 def test_real_VB_helper_changed(self):
  def change(x):
   m=next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.UdfLinearAlgebra' and m['member_name']=='TryInvertMatrix');i=next(i for i in m['instructions'] if 'ClearProjectError' in str(i['operand']));i['operand']='METHOD:Fake.NoOp'
  self.current(change,'Framework VB catch helpers')
 def test_validation_moved_inside_catch(self):self.current(lambda x:next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.UdfLinearAlgebra' and m['member_name']=='TryInvertMatrix')['exceptions'][0].update(try_start=0),'improperly includes validation')
 def test_hidden_facade_initializer(self):self.mutate('metadata-v6/parametric-v4/original-Debug.json',lambda x:next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.Matrix.Matrix' and m['member_name']=='MatInv').update(type_initializer=True),'observable static/catch')
 def test_wrong_Err_state(self):self.mutate('inversion-parametric-v4-Debug.json',lambda x:next(r for r in x['cases'] if r['name']=='singular')['after'].update(number=5),'Err branch')
 def test_original_query_forgery(self):self.mutate('inversion-parametric-v4-Debug.json',lambda x:x.update(original_calls=1),'Unapproved original')
 def test_input_bits_changed(self):self.mutate('inversion-parametric-v4-Debug.json',lambda x:next(r for r in x['cases'] if r['name']=='regular')['input_after']['bits'].__setitem__(0,'0000000000000000'),'output/bits')
 def test_option_missing_observation(self):self.mutate('option-probe-v3-Debug.json',lambda x:x['cases'].pop(),'missing/changed observations')
 def test_option_wrong_exception(self):
  def change(x):
   r=next(r for r in x['cases'] if r['native']['actual_exception']);r['native']['message']='forged'
  self.mutate('option-probe-v3-Debug.json',change,'option boundary drift')
 def test_option_fake_callee(self):
  def change(x):
   m=next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and m['member_name']=='ParsePcaMatrixType');m['instructions'][-2]['operand']='FAKE'
   # Change the actual call, ensuring the intended consumer condition is reached.
   i=next(i for i in m['instructions'] if i['op']=='call' and 'MultivariateAnalysisUDFsOptions38' in str(i['operand']));i['operand']='METHOD:Fake.Target'
  self.current(change,'conversion/callee CIL')
 def test_option_cache_fields(self):
  self.current(lambda x:next(t for t in x['types'] if t['type']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs')['fields'].pop(),'cache fields')
 def test_option_cache_initializer(self):
  def change(x):
   m=next(m for m in x['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and m['member_name']=='.cctor');m['instructions'][0]['op']='throw'
  self.current(change,'cache initializer')
 def test_changed_accepted_engine(self):
  def change(x):
   m=next(m for m in x['methods'] if 'OneWayANOVA' in m['declaring_type'] and m['member_name']=='compute');m['instructions'][0]['op']='throw'
  self.current(change,'Accepted engine/ResultTable')
 def test_welch_header_changed_even_rehashed(self):self.source(lambda b:b.replace(b'df denominator',b'changed denominator'),'Welch compute/Welsh')
 def test_all_calculation_order_even_rehashed(self):
  self.source(lambda b:b.replace(b'mdl.FisherLSD(False, alphaValue)\n                    mdl.FisherLSD(True, alphaValue)',b'mdl.FisherLSD(True, alphaValue)\n                    mdl.FisherLSD(False, alphaValue)'),'MCP ALL numerical order')
 def test_alias_changed_even_rehashed(self):self.source(lambda b:b.replace(b'"TUKEYKRAMER"',b'"FAKE_ALIAS"'),'MCP entire alias/action')
 def test_frozen_output_changed(self):
  c=json.loads((ROOT/(BASE+'compatibility-parametric-v4.json')).read_bytes());p=c['results'][0]['original']['path'];b=(ROOT/p).read_bytes()
  with self.assertRaisesRegex(ValueError,'Frozen compatibility'):inspect(overrides={p:b+b'changed'})
 def test_historical_identity_dropped(self):self.mutate('historical-ownership.json',lambda x:x['members'].pop(),'Historical population',ownership)
 def test_host_only_fabricated(self):self.mutate('historical-ownership.json',lambda x:next(r for r in x['members'] if not r['proof']).update(proof={'kind':'HOST_ONLY'}),'Unsupported historical',ownership)
 def test_false_ready(self):self.mutate('ownership-summary.json',lambda x:x.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'),'False READY',ownership)
 def test_support_auto_closed(self):self.mutate('current-ownership.json',lambda x:next(r for r in x['support'] if r['kind']=='ModuleBlock').update(fully_reconciled=True),'Unsupported support',ownership)
 def test_inline_parent_lost(self):self.mutate('current-ownership.json',lambda x:next(r for r in x['fragments'] if r['review_status'].startswith('COVERED')).update(member_id='fake'),'Forged parent',ownership)
 def test_census_tampered(self):self.mutate('current-universe-parametric-v4.json',lambda x:x['members'].pop(),'syntax universe',ownership)
 def test_export_changed(self):self.mutate('inspection/parametric-v4/Inspect38-Debug.json',lambda x:x['exports'][0].update(effective_excel_name='forged'),'268 compiled exports')
 def test_erased_synthetic_difference(self):self.mutate('text-domains-parametric-v4-v5-Debug.json',lambda x:x['synthetic'][0].update(differences=0),'Erased raw',text)
 def test_site_missing(self):self.mutate('text-domains-parametric-v4-v5-Debug.json',lambda x:x['results'].pop(),'observation missing',text)
if __name__=='__main__':unittest.main(verbosity=2)
