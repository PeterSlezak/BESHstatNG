import json,unittest
import verify_names as names,verify_unchanged_v3 as leaves,verify_ownership as owners,verify_text_domains as textpolicy
ROOT=names.ROOT;HERE=names.HERE;BASE=HERE.relative_to(ROOT).as_posix()+'/'
class IndependentControls(unittest.TestCase):
 def reject_json(self,reader,path,change):
  path=BASE+path;x=json.loads((ROOT/path).read_bytes());change(x)
  with self.assertRaises((ValueError,KeyError,StopIteration,TypeError)):reader.inspect(overrides={path:json.dumps(x).encode()})
 def reject_source(self,path,change):
  value=change((ROOT/path).read_bytes())
  with self.assertRaises((ValueError,KeyError,StopIteration)):names.inspect(overrides={path:value})
 def test_valid_names(self):self.assertEqual(names.inspect()['duplicates_removed_from_coordinate_class'],2)
 def test_valid_whole_ownership(self):self.assertEqual(owners.inspect()['historical']['OPEN'],573)
 def test_missing_original_case(self):self.reject_json(names,'compatibility-names-v3.json',lambda x:x.update(totals_per_configuration={'Debug':29423,'Release':29424}))
 def test_changed_original_output(self):
  r=json.loads((HERE/'compatibility-names-v3.json').read_bytes())['results'][0]['original'];
  with self.assertRaises(ValueError):names.inspect(overrides={r['path']:(ROOT/r['path']).read_bytes()+b'X'})
 def test_names_original_query_forgery(self):self.reject_json(names,'compatibility-names-v3.json',lambda x:x.update(new_original_queries=1))
 def test_duplicate_coordinate_helpers(self):self.reject_source(names.SPILL,lambda b:b.replace(b'Public ReadOnly Property RowOrdinal As Integer',b'Public Shared Function StackWithBlankRow() As Object\nReturn Nothing\nEnd Function\nPublic ReadOnly Property RowOrdinal As Integer'))
 def test_pvalue_coordinate_property_loss(self):self.reject_source(names.SPILL,lambda b:b.replace(b'Property RowOrdinal',b'Property Row'))
 def test_wrong_explicit_identity(self):self.reject_source(names.COMMON,lambda b:b.replace(b'Return names',b'Return DirectCast(names.Clone(), String())',1))
 def test_new_negative_count_guard(self):self.reject_source(names.COMMON,lambda b:b.replace(b'Dim fallback(expectedCount - 1)',b'If expectedCount < 0 Then Return Nothing\nDim fallback(expectedCount - 1)',1))
 def test_wrong_comma_grammar(self):self.reject_source(names.COMMON,lambda b:b.replace(b'StringSplitOptions.RemoveEmptyEntries',b'StringSplitOptions.None'))
 def test_conversion_before_acquisition(self):self.reject_source('BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportFormula.vb',lambda b:b.replace(b'Function() TryCast(arg, String)',b'Function() Convert.ToString(arg)'))
 def test_invented_native_edge(self):
  def f(x):next(r for r in x['methods'] if r['declaring_type']=='BESHStatNG.UdfDataImport' and r['member_name']=='GetVariableNames')['instructions']=[]
  self.reject_json(names,'metadata-v5/names-v3/current-Debug.json',f)
 def test_wrong_original_transport(self):
  def f(x):next(r for r in x['methods'] if r['declaring_type']=='BESHStatNG.WindowsSpreadsheetTransport' and r['member_name']=='CopyValues')['instructions'].pop()
  self.reject_json(names,'metadata-v5/names-v3/original-Debug.json',f)
 def test_changed_CI_getter(self):
  def f(x):next(r for r in x['methods'] if r['declaring_type']=='BESHStatNG.ConfidenceIntervalResult' and r['member_name']=='get_strConfidenceInterval')['instructions'].pop()
  self.reject_json(leaves,'metadata-v5/names-v3/current-Debug.json',f)
 def test_changed_CI_enum(self):
  def f(x):next(r for r in x['types'] if r['type']=='BESHStatNG.CIformat')['fields'][1]['literal']=999
  self.reject_json(leaves,'metadata-v5/names-v3/current-Debug.json',f)
 def test_missing_generic_default_context(self):
  def f(x):next(r for r in x['methods'] if r['declaring_type']=='BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38' and r['member_name']=='NormalizeMatchToken')['locals'].append('FORGED')
  self.reject_json(leaves,'metadata-v5/names-v3/current-Debug.json',f)
 def test_unreviewed_host_only(self):self.reject_json(owners,'historical-ownership.json',lambda x:next(r for r in x['members'] if not r['proof']).update(proof={'kind':'HOST_ONLY'}))
 def test_dropped_historical_member(self):self.reject_json(owners,'historical-ownership.json',lambda x:x['members'].pop())
 def test_dropped_current_callback(self):self.reject_json(owners,'current-ownership.json',lambda x:x['callback_proofs'].pop())
 def test_broken_inline_parent(self):self.reject_json(owners,'current-ownership.json',lambda x:next(r for r in x['fragments'] if r['review_status']=='COVERED_BY_REVIEWED_BODY').update(member_id='FORGED'))
 def test_module_header_auto_closure(self):self.reject_json(owners,'current-ownership.json',lambda x:next(r for r in x['support'] if r['kind']=='ModuleBlock').update(fully_reconciled=True))
 def test_dropped_support(self):self.reject_json(owners,'current-ownership.json',lambda x:x['support'].pop())
 def test_forged_ready(self):self.reject_json(owners,'ownership-summary.json',lambda x:x.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW',W2='COMPLETE'))
 def test_null_text_RHS(self):self.reject_json(textpolicy,'text-call-sites.json',lambda x:next(r for r in x['calls'] if r['kind']=='EqualText').update(right_literal=None))
 def test_ignored_synthetic_nonpass(self):self.reject_json(textpolicy,'text-domains-names-v3-v2-Debug.json',lambda x:x['synthetic'][0].update(differences=0,mismatches=[]))
 def test_swapped_text_locale(self):self.reject_json(textpolicy,'text-domains-names-v3-v2-Debug.json',lambda x:x['results'][0].update(culture='wrong'))
if __name__=='__main__':unittest.main(verbosity=2)
