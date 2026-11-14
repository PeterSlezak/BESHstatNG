"""One full campaign plus explicitly bounded corrections, never stitched as a full run."""
from common import *
from collections import Counter,defaultdict
from baseline_report import index
from validation_report import safe_label
import xml.etree.ElementTree as ET
load=lambda p:json.loads(Path(p).read_bytes())
baseline=load(PREVIOUS/'managed-Debug-identities-v1.json');NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
tests=[];nonpasses=[];suites=[];all_keys=set();totals=Counter()
for suite,part in [('Core','all'),('Common','main'),('Common','semantic'),('Legacy','all'),('ControlMitigation','all'),('OfficeAddin','all'),('OfficeExecution','all')]:
 name='w2c17-checkpoint-v1-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
 execution=RUNROOT/'executions'/name;data=load(execution/'cases.json');current=index(data['records'])
 previous={(r['identity_sha256'],r['occurrence']):r for r in baseline['tests'] if r['suite']==suite}
 trx=execution/'results'/(suite+'.trx');xml=ET.parse(trx).getroot();traces=defaultdict(list)
 for r in xml.findall('.//'+NS+'UnitTestResult'):
  if r.attrib['outcome']!='Passed':traces[r.attrib['testName']].append(r.findtext('.//'+NS+'StackTrace',''))
 for key,value in current.items():
  unique=(suite,*key);assert unique not in all_keys;all_keys.add(unique)
  if key in previous:assert previous[key]['outcome']==value['outcome']
  row=dict(suite=suite,part=part,identity_sha256=key[0],occurrence=key[1],class_name=value['class_name'],method=value['method'],test_name=safe_label(value['test_name']),outcome=value['outcome'],population='RETAINED_C16' if key in previous else 'NEW_C17')
  tests.append(row)
  if value['outcome']!='Passed':
   trace=traces[value['test_name']].pop(0);nonpasses.append(dict(row,message_sha256=sha(value['message'].encode()),message=clean(value['message']),traceback_sha256=sha(trace.encode()),traceback=clean(trace),raw_TRX=pin(trx)))
 totals.update(data['outcomes']);suites.append(dict(suite=suite,part=part,outcomes=data['outcomes'],raw_cases=pin(execution/'cases.json'),TRX=pin(trx),command=clean(data['command'])))
old={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in baseline['tests']}
assert set(old)<=all_keys and len(old)==21053 and len(all_keys)==21092
fields=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'],r['outcome'],r['message_sha256'])
inherited=[r for r in nonpasses if (r['suite'],r['identity_sha256'],r['occurrence']) in old]
assert Counter(map(fields,inherited))==Counter(map(fields,baseline['nonpasses']))
assert totals==Counter(Passed=21076,Failed=16)
save('managed-full-Debug-identities-v1.json',dict(tests=tests,new_tests=[r for r in tests if r['population']=='NEW_C17'],nonpasses=nonpasses,outcomes=dict(totals),source_freeze=pin(HERE/'production-test-freeze-v4.json'),scope='GENUINE_ONE_FULL_CAMPAIGN;PRE_TWO_NEW_FIXTURE_CORRECTIONS'))
effective=copy_tests=json.loads(json.dumps(tests));bykey={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in effective};corrections=[];focused=[]
for suite in ['Common','Legacy']:
 p=RUNROOT/'executions'/('w2c17-new-fixture-correction-v1-'+suite)/'cases.json';data=load(p)
 for key,r in index(data['records']).items():
  fullkey=(suite,*key);assert fullkey in bykey and fullkey not in old and r['outcome']=='Passed'
  before=bykey[fullkey]['outcome'];bykey[fullkey]['first_full_outcome']=before;bykey[fullkey]['outcome']='Passed';bykey[fullkey]['focused_correction']=pin(p)
  focused.append(fullkey)
  if before!='Passed':corrections.append(dict(identity=bykey[fullkey],first_full_outcome=before,focused_outcome='Passed'))
assert len(focused)==len(set(focused))==39 and len(corrections)==2
effective_outcomes=Counter(r['outcome'] for r in effective);assert effective_outcomes==Counter(Passed=21078,Failed=14)
save('managed-Debug-identities-v1.json',dict(tests=effective,new_tests=[r for r in effective if r['population']=='NEW_C17'],nonpasses=inherited,effective_outcomes=dict(effective_outcomes),genuine_full_campaign_outcomes=dict(totals),new_fixture_failures_and_focused_corrections=corrections,not_a_second_full_campaign=True))
save('managed-validation-v1.json',dict(Debug=dict(genuine_full_campaign_outcomes=dict(totals),effective_identity_outcomes=dict(effective_outcomes),inherited_identities=21053,inherited_nonpasses=14,inherited_identity_outcome_message_drift=0,new_tests=39,new_tests_focused_PASS=39,full_campaign_new_FAIL=2,suites=suites),full_campaign=pin(HERE/'managed-full-execution-v1.json'),focused_recheck=pin(HERE/'managed-new-fixture-focused-recheck-v1.json'),source_freeze=pin(HERE/'production-test-freeze-v5.json'),production_source_identical_to_full_campaign=True,qualification='Full campaign used freeze4. Two new test-only fixtures were corrected at freeze5; all39 new identities passed focused Debug rechecks. Full campaign is retained as pre-correction; no claim of a second full campaign or full final-test-freeze run.',campaigns=1,Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
print('Full21076PASS16FAIL retained; effective21078PASS14inheritedFAIL; all39 new focusedPASS; no second full run')
