"""Record tooling failures without rerunning successful managed tests."""
import json,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide';sys.path.insert(0,str(G))
from common import RUNROOT,m,clean,pin

s=(HERE/'capture_current.py').read_text(encoding='utf-8')
s=s.replace("folder=run/'udf'","folder=run/'udf-v2'").replace("'GeeCapture38.cs'","'GeeCapture38-v2.cs'").replace("'w2-profile-","'w2-profile-v2-").replace("'profile-current-results'","'profile-current-results-v2'").replace("'profile-compatibility-'","'profile-compatibility-v2-'")
with (HERE/'capture_current_v2.py').open('x',encoding='utf-8') as f:f.write(s)
records=[]
for config in ['Debug','Release']:
 for suite in ['Core','Common']:
  target=RUNROOT/'completion-w2/profiles-v3'/config/('results-profiles-v1-'+suite)/'focused.trx'
  parsed=m.trx(target);label='w2-profiles-v1-'+config+'-focused-'+suite
  cmd=json.loads((RUNROOT/'commands'/label/'command.json').read_bytes());assert cmd['exit_code']==0 and set(parsed['outcomes'])=={'Passed'}
  assert len(parsed['records'])==(6 if suite=='Core' else 21)
  records.append(dict(configuration=config,suite=suite,tests=clean(parsed),command=clean(cmd)))
with (HERE/'focused-validation-corrected.json').open('x',encoding='utf-8') as f:json.dump(dict(status='54_EXECUTED_FOCUSED_TESTS_PASS',results=records,
 failed_summary_attempt='focused_tests.py treated command-vector list as a record after successful runs; both original exception receipts retained; raw tests were not rerun or changed',
 failed_B_recorder_selection='capture_current.py chose historical invalid GeeCapture38.cs; accepted frozen GeeCapture38-v2.cs is used in distinct corrected execution folders; failed compiler receipts retained'),f,indent=2)
print('Existing raw TRX independently read:27PASS/configuration; no successful tests rerun')
