"""Selected Release metadata validation only; no full Release suite or UDF call."""
import json,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
sys.path.insert(0,str(ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'))
from common import command,clean,PRIVATE,RUNROOT,pin,library_pin
exe=PRIVATE/'w1-inspection/Inspect38.exe';build=RUNROOT/'builds/validation-v2/Release/legacy'
result=command('w1-release-exports',[str(exe),str(build),str(HERE/'compiled-current-Release.json'),str(ROOT/'evidence/04A38/entry-members.json')]);assert result['exit_code']==0
fresh=json.loads((HERE/'compiled-current-Release.json').read_bytes());old=json.loads((ROOT/'evidence/04A38/preedit/compiled-Release.json').read_bytes())
assert {r['effective_excel_name']:r for r in fresh['exports']}=={r['effective_excel_name']:r for r in old['exports']} and len(fresh['exports'])==268
with (HERE/'release-export-verification.json').open('x',encoding='utf-8') as f:json.dump(dict(status='268_COMPILED_RELEASE_EXPORTS_EXACT',command=clean(result),current=pin(HERE/'compiled-current-Release.json'),original=pin(ROOT/'evidence/04A38/preedit/compiled-Release.json'),assembly=library_pin(build/'BESHStatNG.dll'),full_Release_suite='NOT_RUN',native='NOT_RUN'),f,indent=2)
print('268/268 Release compiled exports/defaults/attributes exact; no UDF/model execution or full suite')
