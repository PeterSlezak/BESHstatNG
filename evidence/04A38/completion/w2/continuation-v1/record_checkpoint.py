"""Append verified current source/preservation facts; no READY inference."""
from collections import Counter
from common import *
from verify_preservation_v4 import inspect
from baseline_report import python_receipt
def main():
 proof=inspect();save('preservation.json',proof)
 tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
 baseline=json.loads((ROOT/'evidence/04A38/late-archived-entry-supplement/v1/preservation.json').read_bytes());records=[]
 for r in baseline['records']:
  p=hidden.get(r['path'],r['path']);raw=(ROOT/p).read_bytes();current=dict(path=r['path'],bytes=len(raw),sha256=sha(raw));same=current['bytes']==r['current']['bytes'] and current['sha256']==r['current']['sha256']
  records.append(dict(path=r['path'],prior_entry_disposition=r['status'],prior_checkpoint=r['current'],current=current,continuation='EXACT_PRIOR_CHECKPOINT' if same else 'EXPLICIT_RETAINED_SOURCE_SUCCESSOR',original_entry_exact=r['status']=='EXACT_ENTRY' and same))
 save('tracked-path-preservation.json',dict(scope='LIVE_4695_TRACKED_PATHS_ONLY;NO_IGNORED_TREE_TRAVERSAL',total=len(records),original_entry_exact=sum(r['original_entry_exact'] for r in records),authorized_transitions=sum(not r['original_entry_exact'] for r in records),continuation_further_transitions=sum(r['continuation']!='EXACT_PRIOR_CHECKPOINT' for r in records),records=records))
 managed=[]
 for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
  r=json.loads((RUNROOT/'executions'/('w2c1-validation-v2-Debug-serial-'+suite)/'cases.json').read_bytes());managed.append(dict(suite=suite,outcomes=r['outcomes'],exit_code=r['command']['exit_code']))
 save('managed-Debug-summary.json',dict(suites=managed,outcomes=dict(sum((Counter(r['outcomes']) for r in managed),Counter())),final_source_build='w2c1-validation-v2',earlier_complete_run='w2c1-validation-v1 before calibration compatibility repair, retained; never substituted for final run'))
 save('p1/attempt-summary.json',dict(shipping_fixture_failures=['p1-v2-Debug:2 invalid nested-NaN expectations','p1-v3-Debug and Release:2 invalid calibration #NUM assumptions each'],actual_regression=dict(stage='p1-v4',configuration_results={'Debug':72,'Release':72},corpus='Approval A calibration',required='Double42.0 D4045000000000000 from original Enum/Double If coercion',observed='ExcelErrorNA typed enum',resolution='p1-v5 host CDbl(ExcelErrorNA) passes numeric policy; all29424/config frozen comparisons exact'),reader_attempts='All old reader sources, raw receipts and setup/adverse errors retained',investigative_runtime_policy='Separate8 differences/config, no production change/extra original case; reachability/compatibility review OPEN'))
 save('remaining-boundary-review.json',dict(status='OPEN_CURRENT_04A38_WORK_NOT_FUTURE_DEFERRAL',owner_decision_needed=False,new_reference_needed=False,items=[dict(path='BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb',name='TryParseLeveneCenter',observation='Original resets useMedian/title before NormalizeText/AsString. Current direct delegation evaluates native AsString before portable resets. Exceptional ToString reachability and partial ByRef order require source-backed compatible extraction review.'),dict(path='BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb',name='EqualText',observation='Current runtime comparison control:Nothing versus U+00AD or U+0000 differs from Framework Operators.CompareString Text policy, in both directions and both cultures.8/1352 pair differences/config;0 differences for nonnull pairs. Many callers normalize Nothing to String.Empty; prove every relevant domain before closure. No original BESH query run.'),dict(path='ALL_41_UDF_SOURCES',name='W1 remaining schedule',observation='644 historical and654 current bodies plus303 distinct support declarations remain OPEN. Complete family report/import/preparation and statement/inline dependencies, not only the most recent model profiles. No unsupported HOST_ONLY shortcut.')]))
 print('Recorded live protection and fullDebug',managed)
if __name__=='__main__':main()
