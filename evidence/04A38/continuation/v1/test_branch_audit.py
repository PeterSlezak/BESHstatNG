"""Read-only audit controls; no reference/production invocation or file mutation."""
import json,unittest
from pathlib import Path
import audit_frozen_branches as audit
from propose_missing_cases import cases

class FrozenBranchAuditTests(unittest.TestCase):
 def test_nested_array_argument_boundaries_are_preserved(self):
  parsed=audit.children('System.Object[0:2]{System.Object[0:1,0:2]{D3FF0000000000000|N}|N}')
  self.assertEqual(2,len(parsed['values']))
  matrix=audit.children(parsed['values'][0]);self.assertEqual([(0,1),(0,2)],matrix['bounds'])
 def test_nonzero_lower_bound_preserved(self):
  self.assertEqual([(3,1),(5,1)],audit.children('System.Object[3:1,5:1]{N}')['bounds'])
 def test_wrong_cardinality_rejected(self):
  with self.assertRaises(AssertionError):audit.children('System.Object[0:2]{N}')
 def test_unbalanced_record_rejected(self):
  with self.assertRaises(AssertionError):audit.children('System.Object[0:1]{{N}')
 def test_empty_array_retains_rank(self):
  parsed=audit.children('System.Object[0:0,0:8]{}');self.assertEqual([],parsed['values']);self.assertEqual(2,len(parsed['bounds']))
 def test_nan_and_infinity_are_not_numeric_header_data(self):
  for token in ['D7FF8000000000000','DFFF8000000000000','D7FF0000000000000','DFFF0000000000000']:
   self.assertTrue(audit.nonfinite(token));self.assertFalse(audit.numeric(token))
 def test_signed_zero_is_numeric(self):
  self.assertTrue(audit.numeric('D8000000000000000'));self.assertFalse(audit.nonfinite('D8000000000000000'))
 def test_strings_boolean_errors_and_sentinels_are_not_native_numbers(self):
  for token in ['SMQ==','B1','ENUMExcelDna.Integration.ExcelError:42','N','ExcelDna.Integration.ExcelMissing']:
   self.assertFalse(audit.numeric(token))
 def test_integral_and_decimal_scalar_tokens_remain_numeric(self):
  self.assertTrue(audit.numeric('System.Int32:1'));self.assertTrue(audit.numeric('M00000001,00000000,00000000,00000000'))
 def test_real_frozen_gap_and_guard_counts(self):
  report=audit.inspect();self.assertEqual('NOT_READY_04A38',report['status'])
  for config in ['Debug','Release']:
   for culture in ['Invariant','sk-SK']:
    row=report['counts'][config+'/'+culture+'/calibration']
    self.assertEqual(29,row['cases']);self.assertEqual(1,row['member_entered']);self.assertEqual(28,row['member_not_entered'])
    self.assertEqual(29,row['method_token_N']);self.assertEqual(0,row['nonfinite_CI_rows'])
 def test_proposed_matrix_is_orthogonal_and_has_no_expectations(self):
  matrix=cases();self.assertEqual(432,len(matrix));self.assertEqual(432,len({r['case_id'] for r in matrix}))
  for r in matrix:self.assertEqual('UNKNOWN_NOT_CAPTURED',r['expected_result'])
  for culture in ['Invariant','sk-SK']:
   for method in ['quantile','equalwidth']:
    for weight in ['missing','zero-first-bin','negative']:
     self.assertTrue(any(r['family']=='calibration' and r['culture']==culture and r['parameters']==dict(method=method,bins=2,weights=weight) for r in matrix))
 def test_invalid_bins_never_count_as_report_reachability(self):
  for row in cases():
   if row['family']=='calibration' and row['parameters']['bins']==1:
    self.assertNotEqual('CalibrationBinAndReportPreparation',row['intended_source_phase'])

if __name__=='__main__':unittest.main()
