"""Exact C6 identity/outcome/message comparison, raw TRX traceback provenance."""
from common import *
from collections import Counter
from baseline_report import index
from validation_report import safe_label
import xml.etree.ElementTree as ET
baseline=json.loads((PREVIOUS/'managed-Debug-identities-v1.json').read_bytes())
tests=[];added=[];nonpasses=[];suites=[];totals=Counter();NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
    execution=RUNROOT/'executions'/('w2c7-checkpoint-v1-Debug-serial-'+suite)
    data=json.loads((execution/'cases.json').read_bytes());current=index(data['records'])
    previous={(r['identity_sha256'],r['occurrence']):r for r in baseline['tests'] if r['suite']==suite}
    assert all(key in current and current[key]['outcome']==r['outcome'] for key,r in previous.items()),'Inherited managed identity/outcome drift'
    trx=execution/'results'/(suite+'.trx');xml=ET.parse(trx).getroot()
    trace_rows={r.attrib['testName']:r.findtext('.//'+NS+'StackTrace','') for r in xml.findall('.//'+NS+'UnitTestResult') if r.attrib['outcome']!='Passed'}
    for key,value in current.items():
        row=dict(suite=suite,identity_sha256=key[0],occurrence=key[1],class_name=value['class_name'],method=value['method'],test_name=safe_label(value['test_name']),outcome=value['outcome'],population='NEW_C7' if key not in previous else 'PRESERVED_C6_IDENTITY')
        tests.append(row)
        if key not in previous:added.append(row)
        if value['outcome']!='Passed':
            trace=trace_rows[value['test_name']]
            nonpasses.append(dict(row,message_sha256=sha(value['message'].encode()),traceback_sha256=sha(trace.encode()),first_traceback=clean(next(iter(trace.splitlines()),'')),message=clean(value['message']),traceback=clean(trace),raw_TRX=pin(trx),classification='INHERITED_MANAGED_NONPASS_EXACT_C6_IDENTITY_OCCURRENCE_OUTCOME_MESSAGE'))
    totals.update(data['outcomes']);suites.append(dict(suite=suite,outcomes=data['outcomes'],exit_code=data['command']['exit_code'],seconds=data['command']['seconds'],trx=pin(trx),raw_cases=pin(execution/'cases.json')))
fields=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'],r['outcome'],r['message_sha256'])
assert Counter(map(fields,nonpasses))==Counter(map(fields,baseline['nonpasses'])),'Inherited managed message/nonpass drift'
assert len(added)==5 and all(r['outcome']=='Passed' for r in added)
assert totals==Counter(Passed=20704,Failed=14)
save('managed-Debug-identities-v1.json',dict(tests=tests,new_tests=added,nonpasses=nonpasses))
save('managed-validation-v1.json',dict(Debug=dict(outcomes=dict(totals),new_passes=5,new_nonpasses=[],inherited_nonpasses=14,suites=suites,identity_occurrence_outcome_message_delta=0),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Release_compile='PASS_NO_RESTORE_COMPILE_ONLY',frozen_comparison='28966_PREEDIT_PLUS_458_SEPARATELY_LATE_EXACT_PER_CONFIGURATION',full_Debug_runs=1,original_queries=0))
print(json.dumps(dict(outcomes=dict(totals),new_passes=5,inherited_nonpasses=14,Release_unit='NOT_RUN_OWNER_PREFERENCE')))
