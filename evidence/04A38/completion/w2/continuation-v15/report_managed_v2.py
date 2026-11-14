"""Compare every Debug identity/occurrence/message with v12, without stitching."""
from common import *
from collections import Counter,defaultdict
from baseline_report import index
from validation_report import safe_label
import xml.etree.ElementTree as ET
baseline=json.loads((PREVIOUS/'managed-Debug-identities-v2.json').read_bytes());NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
parts=[('Core','all'),('Common','main'),('Common','semantic'),('Legacy','all'),('ControlMitigation','all'),('OfficeAddin','all'),('OfficeExecution','all')]
tests=[];added=[];nonpasses=[];suites=[];all_keys=set();totals=Counter()
for suite,part in parts:
    name='w2c15-checkpoint-v2-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
    execution=RUNROOT/'executions'/name;data=json.loads((execution/'cases.json').read_bytes());current=index(data['records'])
    previous={(r['identity_sha256'],r['occurrence']):r for r in baseline['tests'] if r['suite']==suite}
    trx=execution/'results'/(suite+'.trx');xml=ET.parse(trx).getroot();traces=defaultdict(list)
    for row in xml.findall('.//'+NS+'UnitTestResult'):
        if row.attrib['outcome']!='Passed':traces[row.attrib['testName']].append(row.findtext('.//'+NS+'StackTrace',''))
    for key,value in current.items():
        unique=(suite,*key);assert unique not in all_keys,'Overlapping Common selections or duplicate test occurrence';all_keys.add(unique)
        if key in previous:assert previous[key]['outcome']==value['outcome'],'Inherited managed outcome drift'
        row=dict(suite=suite,part=part,identity_sha256=key[0],occurrence=key[1],class_name=value['class_name'],method=value['method'],test_name=safe_label(value['test_name']),outcome=value['outcome'],population='NEW_C15' if key not in previous else 'PRESERVED_C14_IDENTITY',first_full_outcome=value['outcome'],correction=None)
        tests.append(row)
        if key not in previous:added.append(row)
        if value['outcome']!='Passed':
            trace=traces[value['test_name']].pop(0)
            nonpasses.append(dict(row,message_sha256=sha(value['message'].encode()),traceback_sha256=sha(trace.encode()),first_traceback=clean(next(iter(trace.splitlines()),'')),message=clean(value['message']),traceback=clean(trace),raw_TRX=pin(trx),classification='INHERITED_MANAGED_NONPASS_EXACT_C14_IDENTITY_OCCURRENCE_OUTCOME_MESSAGE'))
    totals.update(data['outcomes']);suites.append(dict(suite=suite,part=part,first_full_outcomes=data['outcomes'],outcomes=data['outcomes'],exit_code=data['command']['exit_code'],seconds=data['command']['seconds'],trx=pin(trx),raw_cases=pin(execution/'cases.json')))
assert all((r['suite'],r['identity_sha256'],r['occurrence']) in all_keys for r in baseline['tests']),'Missing old managed identity'
fields=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'],r['outcome'],r['message_sha256'])
assert Counter(map(fields,nonpasses))==Counter(map(fields,baseline['nonpasses'])),'Inherited managed nonpass/message drift'
assert len(added)==64 and all(r['outcome']=='Passed' for r in added) and totals==Counter(Passed=20990,Failed=14)
save('managed-Debug-identities-v2.json',dict(tests=tests,new_tests=added,nonpasses=nonpasses))
save('managed-validation-v2.json',dict(Debug=dict(first_full_outcomes=dict(totals),outcomes=dict(totals),effective_outcomes=dict(totals),corrective_focused={},new_passes=64,new_nonpasses=[],inherited_nonpasses=14,suites=suites,identity_occurrence_outcome_message_delta=0,Common_partition='Complementary main/semantic filters; no missing/duplicate/skipped IDs. Original runner assigns observations only to its semantic class.'),comparison_C14=dict(first_full=dict(Passed=20926,Failed=14),effective=dict(Passed=20926,Failed=14),interpretation='64 genuinely added PASS; all old C14 identities/outcomes/messages retained, with no correction to this first full campaign'),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Release_compile='PASS_NO_RESTORE_COMPILE_ONLY',frozen_comparison='28966_PREEDIT_PLUS_458_SEPARATELY_LATE_EXACT_PER_CONFIGURATION',Debug_campaigns_this_checkpoint=2,authorized_replacement_Debug_runs=1,final_full_Debug_runs=1,first_campaign=pin(HERE/"managed-full-execution-v1.json"),replacement_authority=pin(HERE/"owner-replacement-debug-authorization-v1.json"),original_queries=0))
print(json.dumps(dict(first_full=dict(totals),effective=dict(totals),new_PASS=64,inherited_FAIL=14,missing=0,duplicate=0)))
