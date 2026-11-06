"""Publish sanitized receipts from actual isolated runs. No historical modification."""
from audit import *
from pathlib import Path
from collections import Counter,defaultdict
import hashlib,csv,json,os,re,sys
RUN=ROOT/'.test-rationalization-04A33.tmp';LABEL='qualified'
DOSSIER='docs/architecture/batch-04A-33-owner-review.md'
SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def file_sha(p):return sha(Path(p).read_bytes())
def text(p):
 b=Path(p).read_bytes()
 try:return b.decode('utf-8-sig')
 except UnicodeDecodeError:return b.decode('cp1252')
def clean(v):
 if isinstance(v,list):return [clean(x) for x in v]
 if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
 if not isinstance(v,str):return v
 for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:v=v.replace(a,b)
 v=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',v,flags=re.I)
 v=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',v,flags=re.I)
 return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',v)
def display(v):
 if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}|https?://[^\s/"<>:@]+:[^\s/"<>@]+@|\\\\[A-Za-z0-9_.-]+\\',v):return '<REDACTED_TEST_DISPLAY:'+sha(v.encode())[:24]+'>'
 return clean(v)
def indexed(records):
 buckets=defaultdict(list)
 for r in records:buckets[sha(json.dumps((r['class_name'],r['method'],r['test_name'])).encode())].append(r)
 return {(k,i):r for k,values in buckets.items() for i,r in enumerate(sorted(values,key=lambda x:x['outcome']))}
def managed():
 suites=[];identities=[];nonpasses=[];deltas=[]
 accepted=list(csv.DictReader((ROOT/'evidence/04A32/managed-test-identities.csv').open(encoding='utf-8-sig')))
 for suite in SUITES:
  old=load(RUN/f'executions/before-Debug-{suite}/cases.json');b=indexed(old['records'])
  reference={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in accepted if r['suite']==suite}
  assert set(b)==set(reference) and all(b[i]['outcome']==reference[i]['final_debug'] for i in b)
  final={}
  for c in ['Debug','Release']:
   p=RUN/f'executions/{LABEL}-{c}-{suite}/cases.json';r=load(p);d=indexed(r['records']);final[c]=d
   assert not set(b)-set(d) and all(d[i]['outcome']==b[i]['outcome'] for i in b)
   added=set(d)-set(b);assert all('Graphics33' in d[i]['class_name'] and d[i]['outcome']=='Passed' for i in added)
   suites.append(dict(configuration=c,suite=suite,outcomes=r['outcomes'],counters=r['counters'],baseline_debug=old['outcomes'],trx_sha256=r['sha256'],raw_receipt_sha256=file_sha(p)))
   deltas.append(dict(configuration=c,suite=suite,removed=0,changed_baseline_outcomes=0,new_04A33_raw_cases=len(added)))
   for row in r['records']:
    if row['outcome']!='Passed':nonpasses.append(dict(configuration=c,suite=suite,class_name=row['class_name'],method=row['method'],test_name=display(row['test_name']),outcome=row['outcome'],classification='INHERITED_EXACT_BASELINE_IDENTITY_AND_OUTCOME',message_sha256=sha(row['message'].encode())))
  assert set(final['Debug'])==set(final['Release']) and all(final['Debug'][i]['outcome']==final['Release'][i]['outcome'] for i in final['Debug'])
  for i,r in sorted(final['Debug'].items()):identities.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=display(r['test_name']),raw_identity_sha256=i[0],raw_occurrence_ordinal=i[1],current_main_debug=b[i]['outcome'] if i in b else 'NEW_04A33',final_debug=r['outcome'],final_release=final['Release'][i]['outcome']))
 with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(identities[0]));w.writeheader();w.writerows(identities)
 focused=[]
 for c in ['Debug','Release']:
  for suite in ['Core','Legacy']:
   r=load(RUN/f'executions/{LABEL}-{c}-graphics33-{suite}/cases.json');assert r['command']['exit_code']==0 and set(r['outcomes'])=={'Passed'}
   focused.append(dict(configuration=c,suite=suite,outcomes=r['outcomes'],trx_sha256=r['sha256'],identities=[dict(class_name=x['class_name'],method=x['method'],test_name=x['test_name'],outcome=x['outcome']) for x in r['records']]))
 save('nonpasses.json',dict(schema='04A33-managed-nonpasses-v1',nonpasses=nonpasses))
 return suites,deltas,focused
def python_run(label,suffix):
 p=RUN/'commands'/f'{label}-Debug-{suffix}'/'stderr.log';t=text(p);total=int(re.search(r'Ran (\d+) tests?',t).group(1))
 def n(k):
  m=re.search(k+r'=(\d+)',t);return int(m.group(1)) if m else 0
 failures=n('failures');errors=n('errors')
 return dict(label=label,suite=suffix,total=total,passed=total-failures-errors,failed=failures,errors=errors,exit_code=load(p.parent/'command.json')['exit_code'],stderr_sha256=file_sha(p),primary_identities=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M),nonpasses=[dict(outcome=k,identity=i,subtest=s.strip()) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)])
def source_node():
 python=[];nodes=[];unclassified=[]
 for suffix in ['python-tools','python-help','python-scripts','common-python']:
  old=python_run('before',suffix);new=python_run(LABEL,suffix)
  assert old['total']==new['total'] and Counter(old['primary_identities'])==Counter(new['primary_identities'])
  def index(r):return Counter((x['outcome'],x['identity'],x['subtest']) for x in r['nonpasses'])
  added=index(new)-index(old);removed=index(old)-index(new);changes=[]
  for (k,i,s),n in added.items():
   classification='UNCLASSIFIED'
   if k=='FAIL' and any(p in s for p in WINDOWS):classification='AUTHORIZED_SOURCE_PIN_TRANSITION_RETAINED_FAIL'
   item=dict(outcome=k,identity=i,subtest=s,count=n,classification=classification);changes.append(item)
   if classification=='UNCLASSIFIED':unclassified.append(item)
  new.update(baseline={k:old[k] for k in ['total','passed','failed','errors','stderr_sha256']},identity_population_preserved=True,added_nonpasses=changes,removed_nonpasses=[dict(outcome=k,identity=i,subtest=s,count=n) for (k,i,s),n in removed.items()])
  python.append(new)
 for label in ['before',LABEL]:
  for suffix in ['office-node','resolver','wasm-node']:
   p=RUN/'commands'/f'{label}-Debug-{suffix}'/'stdout.log';t=text(p)
   def count(k):
    a=re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t);assert a,(suffix,k);return int(a[-1])
   nodes.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),stdout_sha256=file_sha(p),exit_code=load(p.parent/'command.json')['exit_code']))
 save('source-and-node-validation.json',dict(schema='04A33-source-node-v1',python=python,node=nodes,unclassified_new_nonpasses=unclassified,configuration='Configuration-independent source/Node lanes once per before/final Debug;full Release managed separately',node_cjs='Complete node --test includes .cjs suites'))
 return python,[r for r in nodes if r['label']==LABEL],unclassified
def casebook():
 records=[];captures=[]
 for c in ['Debug','Release']:
  for suite in ['Core','Legacy']:
   for p in sorted((RUN/'compatibility-cases'/f'{LABEL}-{c}-graphics33-{suite}').glob('*.csv')):
    r=list(csv.DictReader(p.open(encoding='utf-8-sig')))[0]
    if not any(r['id'].startswith(x) for x in ['XYZ33','SCATTER33','WIRE33']):continue
    assert r['expected_exception']==r['actual_exception']
    a=r['expected_ieee'].split('|');b=r['actual_ieee'].split('|');assert len(a)==len(b)
    def nan(x):
     n=int(x,16);return n&0x7ff0000000000000==0x7ff0000000000000 and n&0xfffffffffffff!=0
    assert all(x==y or x and y and nan(x) and nan(y) for x,y in zip(a,b))
    records.append(dict(configuration=c,suite=suite,**r))
  p=RUN/f'{LABEL}-{c}-adapter-capture.csv';assert p.read_bytes()==(HERE/'pre-edit-adapter-capture.csv').read_bytes()
  captures.append(dict(configuration=c,sha256=file_sha(p),bytes=p.stat().st_size,rows=len(list(csv.DictReader(p.open(encoding='utf-8-sig')))),equals_pre_edit_bytes=True,host_assembly_sha256=file_sha(RUN/f'builds/{LABEL}/{c}/legacy/BESHstatNG.dll'),core_assembly_sha256=file_sha(RUN/f'builds/{LABEL}/{c}/legacy/BESHStat.Core.dll')))
 assert records
 # Full expected/actual vectors once; other lanes are independently checked and digest-linked.
 primary=[r for r in records if r['configuration']=='Debug' and r['suite']=='Core']
 with (HERE/'compatibility-vectors.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(primary[0]));w.writeheader();w.writerows(primary)
 lanes=[]
 for c in ['Debug','Release']:
  for suite in ['Core','Legacy']:
   r=[x for x in records if x['configuration']==c and x['suite']==suite];assert {x['id'] for x in r}=={x['id'] for x in primary}
   lanes.append(dict(configuration=c,suite=suite,records=len(r),sha256=sha(json.dumps(sorted(r,key=lambda x:x['id']),sort_keys=True).encode()),numeric_and_exception_comparison='PASS'))
 save('compatibility-casebook.json',dict(schema='04A33-casebook-v1',authority='COMPATIBILITY_REFERENCE: frozen merged-main numeric fixtures and actual pre-edit Framework captures. HOST_FREE_EXECUTED. NOT_GOLDEN.',source_authority=pin('evidence/04A33/pre-edit-authority.json'),unique_vector_cases=len(primary),executed_vector_comparisons=len(records),vectors=pin('evidence/04A33/compatibility-vectors.csv'),lanes=lanes,actual_adapter_captures=captures,comparison='Exact IEEE finite/infinite/signed-zero bits;algorithmic NaNs by classification;copy tests exact bits. No new tolerance. Pre/post actual adapter CSV byte-identical, including NaN bits.',baseline_capture_limit='Actual adapter numeric members only;wire Draw requires a Chart and was not invoked. Frozen test-only original sampling slice executes ring numerics with the original projection fixture;native tail remains source-guarded.',culture='Invariant actual capture;managed test CurrentCulture explicitly recorded per vector;no worksheet row fabricated',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
 return len(primary),len(records),captures
def commands():
 rows=[]
 for p in sorted((RUN/'commands').glob('*/command.json')):
  r=load(p);exe=Path(r['command'][0]).name.lower();r['command'][0]={'python.exe':'<PYTHON>','msbuild.exe':'<MSBUILD>','vstest.console.exe':'<VSTEST>'}.get(exe,r['command'][0]);r.update(receipt_id=p.parent.name,raw_receipt_sha256=file_sha(p));rows.append(clean(r))
 versions={n:text(RUN/f'commands/qualified-version-{n}/stdout.log').strip() for n in ['dotnet','node','python','msbuild','framework']}
 save('command-outcomes.json',dict(schema='04A33-commands-v1',commands=rows,versions=versions,runner=pin('scripts/test_suite_rationalization.py'),wrapper=pin('evidence/04A33/run.py'),wrappers='NOT_RUN unchanged scripts/verify-beshstatng.ps1 and -Legacy: shared historical TestResults/Codex plus shared bin/obj and packaging Build hooks. Isolated compile-only + complete managed/source/Node lanes execute accepted equivalents without restore/native/package/sign.',intermediate='dev builds and focused failures retained locally: VB naming/BOM/project-include and reflection comparison harness corrections. Only qualified build/test receipts authoritative. No production change after qualified builds.',raw='Ignored isolated command/TRX receipts retained;only sanitized commands and hashes published'))
 return versions
def seal():
 from verify_manifest import expected_scope,file_pin,SELF,verify_manifest
 paths=expected_scope()|{SELF}
 save('publication-manifest.json',dict(schema='04A33-publication-v1',files=[dict(path=p,bytes=None,sha256=None) if p==SELF else file_pin(p) for p in sorted(paths)]))
 print('PASS manifest',verify_manifest())
if __name__=='__main__':
 action=sys.argv[1]
 if action=='collect':
  suites,deltas,focused=managed();py,node,unknown=source_node()
  if (HERE/'compatibility-casebook.json').exists():
   cb=load(HERE/'compatibility-casebook.json');assert cb['vectors']==pin('evidence/04A33/compatibility-vectors.csv');unique=cb['unique_vector_cases'];comparisons=cb['executed_vector_comparisons'];captures=cb['actual_adapter_captures']
  else:unique,comparisons,captures=casebook()
  versions=commands()
  for c in ['Debug','Release']:
   b=load(RUN/f'builds/{LABEL}/{c}/build.json');assert not any(b['exit_codes']) and b['source_unchanged']
  for p in [RUN/'before-Debug-matrix.json',RUN/'qualified-Debug-matrix.json',RUN/'qualified-Release-managed.json']:assert load(p)['source_unchanged']
  serial={n:load(RUN/f'commands/qualified-{n}/command.json')['exit_code'] for n in ['strict','profile','docs']}
  totals={c:dict(sum((Counter(r['outcomes']) for r in suites if r['configuration']==c),Counter())) for c in ['Debug','Release']}
  save('validation.json',dict(schema='04A33-validation-v1',decision='PENDING_FINAL_SEAL',managed=suites,managed_totals=totals,identity_deltas=deltas,focused=focused,python={k:sum(r[k] for r in py) for k in ['total','passed','failed','errors']},node={k:sum(r[k] for r in node) for k in ['total','passed','failed']},strict_profile_mkdocs=serial,unclassified_new_nonpasses=unknown,unique_vector_cases=unique,executed_vector_comparisons=comparisons,adapter_captures=captures,versions=versions,new_audits='PENDING_POSTSEAL',not_run=['Entry full Release;fresh entry Debug and final full Release executed','Unchanged shared-output verifier wrappers;isolated equivalents executed','Native Excel/COM/UIA/RefEdit/chart smoke;R/scientific oracle;manual Office/Mac/browser/WASM UI;packaging/install/sign/deploy'],scientific='NOT_GOLDEN'))
 elif action=='seal':seal()
 else:raise ValueError(action)
