"""Reconcile full raw managed identities, retaining every inherited nonpass."""
from collections import Counter
import csv,json,re,sys
from preflight import HERE,save,sha
from baseline_report import clean,index,python_receipt
from run import PRIVATE

def safe_label(value):
 value=clean(value)
 value=re.sub(r'https?://[^\s"\']+@[^\s"\']+',lambda match:'<SYNTHETIC_URI:'+sha(match.group().encode())[:20]+'>',value)
 return re.sub(r'\b[A-Za-z]:[\\/][^\s"\')]+',lambda match:'<SYNTHETIC_PATH:'+sha(match.group().encode())[:20]+'>',value)

def reported_python(name):
 result=python_receipt(name)
 # unittest setUpClass can report an ERROR with zero executed test cases.
 if result['passed']<0:
  result['passed']=0
  result['suite_setup_error_qualification']='ERROR before any case ran; no negative PASS count and no inference that expected cases executed.'
 return result

def main():
 label=sys.argv[1];source_label=sys.argv[2];all_rows=[];nonpasses=[];summaries=[];totals={}
 with (HERE/'managed-test-identities-entry.csv').open(encoding='utf-8',newline='') as f:entry=list(csv.DictReader(f))
 for config in ['Debug','Release']:
  total=Counter();new_total=Counter()
  summary=json.loads((PRIVATE/(label+'-'+config+'-managed.json')).read_bytes());assert summary['source_unchanged']
  for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
   x=json.loads((PRIVATE/'executions'/(label+'-'+config+'-serial-'+suite)/'cases.json').read_bytes());current=index(x['records']);total.update(x['outcomes'])
   old={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in entry if r['suite']==suite}
   assert all(k in current and current[k]['outcome']==v['entry_'+config.lower()] for k,v in old.items()),('Historical identity/outcome changed',config,suite)
   new={k:v for k,v in current.items() if k not in old};assert all(v['outcome']=='Passed' for v in new.values()),('NEW_NONPASS',config,suite)
   assert all(any(tag in v['class_name'] for tag in ['UdfNumericPreparation38Tests','UdfPreparation38Tests','UdfPreparation38AdapterTests']) for v in new.values()),('Unexplained new identity',config,suite)
   new_total.update(v['outcome'] for v in new.values())
   summaries.append(dict(configuration=config,suite=suite,outcomes=x['outcomes'],new_cases=len(new),historical_identity_outcome_delta=0,trx_sha256=x['sha256'],exit_code=x['command']['exit_code']))
   for key,row in current.items():
    all_rows.append(dict(configuration=config,suite=suite,raw_identity_sha256=key[0],raw_occurrence_ordinal=key[1],class_name=row['class_name'],method=row['method'],test_name=safe_label(row['test_name']),outcome=row['outcome'],population='NEW_04A38' if key in new else 'HISTORICAL_ENTRY'))
    if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,raw_identity_sha256=key[0],occurrence=key[1],class_name=row['class_name'],method=row['method'],test_name=clean(row['test_name']),outcome=row['outcome'],message_sha256=sha(row['message'].encode()),classification='INHERITED_EXACT_ENTRY_IDENTITY_OCCURRENCE_OUTCOME'))
  totals[config]=dict(outcomes=dict(total),new_outcomes=dict(new_total));assert total==Counter(Passed=20556,Failed=14) and new_total==Counter(Passed=49)
 with (HERE/('managed-test-identities-'+label+'.csv')).open('x',encoding='utf-8',newline='') as f:
  writer=csv.DictWriter(f,fieldnames=list(all_rows[0]));writer.writeheader();writer.writerows(all_rows)
 python=[reported_python(source_label+'-'+suffix) for suffix in ['python-tools','python-help','python-scripts','common-python']]
 nodes=[]
 for suffix in ['office-node','resolver','wasm-node']:
  folder=PRIVATE/'commands'/(source_label+'-'+suffix);text=(folder/'stdout.log').read_text(encoding='utf-8-sig')
  count=lambda key:int(re.findall(r'(?:# |\u2139 )'+key+r' (\d+)',text)[-1])
  nodes.append(dict(suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=json.loads((folder/'command.json').read_bytes())['exit_code']))
 serial={s:json.loads((PRIVATE/'commands'/(source_label+'-'+s)/'command.json').read_bytes())['exit_code'] for s in ['strict','profile','docs']}
 packet=[reported_python(source_label+'-accepted'+b+'-audits') for b in ['34','35','36','37']]+[reported_python(source_label+'-accepted37-supplement')]
 save('nonpasses-'+label+'.json',dict(managed=nonpasses,python=[dict(label=r['label'],nonpasses=r['nonpasses']) for r in python],historical_packet_audits=packet,
  development_failures='All attempted command receipts retained, including failed builds, the corrected namespace test row, the corrected interior-gap fixture, and invalid pre-edit attempts. No old test expectation was edited.',
  classification='Python/source/hash nonpasses remain FAIL/ERROR, including qualified authorized-source transitions; no test is rebaselined.'))
 save('validation-'+label+'.json',dict(status='EXECUTED_REGRESSION_RESULTS; NOT_IMPLEMENTATION_READY',managed=totals,suites=summaries,
  python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']},python_suites=python,node=nodes,node_total_pass=sum(r['passed'] for r in nodes),strict_profile_mkdocs=serial,
  historical_packet_audits=packet,compatibility='compatibility/validation-v1/comparison.json; production files unchanged by new test-fixture correction',
  denied_gates={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'},native_excel='NOT_RUN',r_oracle='NOT_RUN',packaging_signing_deployment='NOT_RUN',scientific='NOT_GOLDEN',
  canonical_wrapper_qualification=json.loads((HERE/'entry-baseline.json').read_bytes())['canonical_wrappers']))
 save('command-receipts-'+label+'.json',dict(commands=[dict(name=p.parent.name,**clean(json.loads(p.read_bytes()))) for p in sorted((PRIVATE/'commands').glob('*/command.json'))],
  policy='Actual executed commands/exits/timings and hashes of private raw output; personal paths sanitized. No inferred pass for an unexecuted lane.'))
 print('Managed20556 PASS/14 inherited FAIL each;49 new PASS/configuration; Node',sum(r['passed'] for r in nodes),'Python',{k:sum(r[k] for r in python) for k in ['passed','failed','errors']},'; strict/profile/docs',serial)

if __name__=='__main__':main()
