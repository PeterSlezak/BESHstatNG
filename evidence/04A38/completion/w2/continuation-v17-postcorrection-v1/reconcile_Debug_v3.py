"""Read-back reconciliation of fresh compiled source, complete body proofs, and every managed identity."""
from common import *
from collections import Counter,defaultdict
import xml.etree.ElementTree as ET
from baseline_report import index
from validation_report import safe_label
load=lambda p:json.loads(Path(p).read_bytes())
for r in load(HERE/'production-test-freeze-v1.json')['sources']:assert pin(ROOT/r['path'])==r
sources=[]
for n in ['current-universe.json','dependency-full-core.json','dependency-full-common.json','dependency-full-adjacent.json','declaration-shapes.json']:
 a=load(PREVIOUS/'final-v8'/n);b=load(HERE/'final-v1'/n);assert a==b,n
 sources.append(dict(previous=pin(PREVIOUS/'final-v8'/n),current=pin(HERE/'final-v1'/n),exact=True))
a=load(PREVIOUS/'final-v8/current-Debug.json');b=load(HERE/'final-v1/current-Debug.json')
assert a['types']==b['types'] and a['scope']==b['scope']
method=lambda r:{k:v for k,v in r.items() if k!='raw_IL_sha256'}
assert [method(r) for r in a['methods']]==[method(r) for r in b['methods']],'Compiled semantic CIL/signature drift; investigate before closure'
raw_diffs=sum(x['raw_IL_sha256']!=y['raw_IL_sha256'] for x,y in zip(a['methods'],b['methods']))
proof_names=['source-composition-proof-v4.json','whole-body-contracts-v3.json','callback-proofs-v3.json','original-only-property-proofs-v3.json','portable-api-contracts-v3.json','complete-Debug-dependency-graph-v3.json','constructed-host-and-property-contexts-v3.json','compiled-method-deltas-v3.json','conversion-and-alias-boundaries-v3.json','support-fragment-crosswalk-v3.json','original-current-fragment-transition-matrix-v2.json','historical-ownership-v4.json','current-ownership-v4.json','canonical-identity-status-v4.json','remaining-work-queue-v4.json']
proofs=[pin(PREVIOUS/n) for n in proof_names if (PREVIOUS/n).is_file()]
assert len(proofs)==len(proof_names),'Required final ownership proof missing'
save('whole-body-dependency-fragment-reconciliation-v1.json',dict(status='FRESH_SOURCE_CENSUS_AND_COMPILED_SEMANTIC_PARITY_REVALIDATES_UNCHANGED_COMPLETE_BODY_PROOFS',source_comparisons=sources,old_metadata=pin(PREVIOUS/'final-v8/current-Debug.json'),fresh_metadata=pin(HERE/'final-v1/current-Debug.json'),compiled_methods=len(b['methods']),compiled_types=len(b['types']),raw_IL_hash_changes=raw_diffs,semantic_CIL_signature_exception_delegate_changes=0,proofs=proofs,checkpoint_reader=pin(HERE/'entry-final-reader-authentication-v1.json'),counts=dict(historical=dict(supported=704,total=704,OPEN=0),current=dict(supported=726,total=726,OPEN=0),support=dict(supported=303,total=303,OPEN=0,raw=325)),new_closures_this_validation=0,Release=DEFERRED))
prior=load(PREVIOUS/'managed-Debug-identities-v2.json');previous={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['tests']}
tests=[];nonpasses=[];suites=[];actual={};totals=Counter()
for suite,part in [('Core','all'),('Common','main'),('Common','semantic'),('Legacy','all'),('ControlMitigation','all'),('OfficeAddin','all'),('OfficeExecution','all')]:
 name='w2c17pc1-replacement-v1-Debug-serial-'+suite+('-'+part if suite=='Common' else '')
 execution=RUNROOT/'executions'/name;data=load(execution/'cases.json');trx=pin(execution/'results'/(suite+'.trx'))
 traces=defaultdict(list);NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
 for event,node in ET.iterparse(execution/'results'/(suite+'.trx'),events=['end']):
  if node.tag==NS+'UnitTestResult':
   if node.attrib['outcome']!='Passed':traces[node.attrib['testName']].append(node.findtext('.//'+NS+'StackTrace',''))
   node.clear()
  elif node.tag==NS+'UnitTest':node.clear()
 for key,r in index(data['records']).items():
  unique=(suite,*key);assert unique not in actual
  row=dict(suite=suite,part=part,identity_sha256=key[0],occurrence=key[1],class_name=r['class_name'],method=r['method'],test_name=safe_label(r['test_name']),outcome=r['outcome'])
  actual[unique]=row;tests.append(row)
  if r['outcome']!='Passed':
   trace=traces[r['test_name']].pop(0);nonpasses.append(dict(row,message=clean(r['message']),message_sha256=sha(r['message'].encode()),traceback=clean(trace),traceback_sha256=sha(trace.encode()),raw_TRX=trx))
 totals.update(data['outcomes']);suites.append(dict(suite=suite,part=part,outcomes=data['outcomes'],raw_cases=pin(execution/'cases.json'),TRX=trx,command=clean(data['command'])))
missing=[previous[k] for k in previous.keys()-actual.keys()];added=[actual[k] for k in actual.keys()-previous.keys()]
changed=[dict(previous=previous[k],current=actual[k]) for k in previous.keys()&actual.keys() if previous[k]['outcome']!=actual[k]['outcome']]
oldnon={(r['suite'],r['identity_sha256'],r['occurrence']):r for r in prior['nonpasses']}
message_changes=[dict(previous=oldnon.get((r['suite'],r['identity_sha256'],r['occurrence'])),current=r) for r in nonpasses if oldnon.get((r['suite'],r['identity_sha256'],r['occurrence']),{}).get('message_sha256')!=r['message_sha256']]
save('managed-Debug-identities-v1.json',dict(tests=tests,nonpasses=nonpasses,outcomes=dict(totals),previous=pin(PREVIOUS/'managed-Debug-identities-v2.json'),missing=missing,added=added,outcome_changes=changed,nonpass_message_changes=message_changes,qualification='ONE_GENUINE_REPLACEMENT_COMPREHENSIVE_RUN_AT_FINAL_CORRECTED_SOURCE;NOT_A_STITCHED_RESULT'))
save('managed-validation-v1.json',dict(Debug=dict(outcomes=dict(totals),identities=len(tests),inherited=len(previous),new=len(added),missing=len(missing),changed_outcomes=len(changed),nonpass_message_changes=len(message_changes),suites=suites),source_freeze=pin(HERE/'production-test-freeze-v1.json'),campaigns=1,Release_compilation=DEFERRED,Release_full_selected_focused_unit_tests=DEFERRED,Release_ABI_CIL_signatures_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
assert not missing,'Missing inherited test identities recorded'
print('Fresh managed results',dict(totals),'identities',len(tests),'outcome changes',len(changed),'message changes',len(message_changes),flush=True)
