"""Real isolated receipt reader; identity multisets and genuine nonpasses retained."""
from work import *
from collections import Counter,defaultdict
RUN=ROOT/'.test-rationalization-04A36.tmp';SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def load(p):return json.loads(p.read_bytes())
def clean(value):
    if isinstance(value,list):return [clean(v) for v in value]
    if isinstance(value,dict):return {k:clean(v) for k,v in value.items()}
    if not isinstance(value,str):return value
    for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:value=value.replace(a,b)
    value=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',value,flags=re.I)
    value=re.sub(r'[A-Za-z]:[\\/]+Program Files(?: \(x86\))?[\\/]+','<PROGRAM_FILES>/',value,flags=re.I)
    value=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',value,flags=re.I)
    return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',value)
def display(value):return '<REDACTED_TEST_DISPLAY:'+sha(value.encode())[:24]+'>' if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}',value) else clean(value)
def index(records):
    buckets=defaultdict(list)
    for r in records:buckets[sha(json.dumps((r['class_name'],r['method'],r['test_name'])).encode())].append(r)
    return {(k,i):r for k,rows in buckets.items() for i,r in enumerate(sorted(rows,key=lambda x:x['outcome']))}
def managed():
    summaries=[];identities=[];nonpasses=[];bad=[];deltas=[];newcounts=Counter();totals=defaultdict(Counter)
    accepted=list(csv.DictReader((ROOT/'evidence/04A35/managed-test-identities.csv').open(encoding='utf-8-sig')))
    for suite in SUITES:
        prior={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in accepted if r['suite']==suite};runs={};bases={}
        for config in ['Debug','Release']:
            before=load(RUN/f'executions/entry-{config}-serial-{suite}/cases.json');after=load(RUN/f'executions/final-{config}-serial-{suite}/cases.json')
            b,a=index(before['records']),index(after['records']);bases[config]=b;runs[config]=a
            assert set(b)==set(prior) and all(b[k]['outcome']==prior[k]['final_'+config.lower()] for k in b),('Merged main managed drift',suite,config)
            removed=set(b)-set(a);changed=[k for k in b.keys()&a.keys() if b[k]['outcome']!=a[k]['outcome']];added=set(a)-set(b)
            newbad=[k for k in added if a[k]['outcome']!='Passed' or 'Graphics36' not in a[k]['class_name']]
            if removed or changed or newbad:bad.append(dict(suite=suite,configuration=config,removed=len(removed),changed=len(changed),new_nonpasses=len(newbad)))
            deltas.append(dict(suite=suite,configuration=config,removed=len(removed),changed_baseline_outcomes=len(changed),new_cases=len(added)))
            totals[config].update(after['outcomes']);newcounts[config]+=len(added)
            summaries.append(dict(suite=suite,configuration=config,baseline=before['outcomes'],final=after['outcomes'],exit_code=after['command']['exit_code'],baseline_exit_code=before['command']['exit_code'],baseline_trx_sha256=before['sha256'],final_trx_sha256=after['sha256']))
            for row in after['records']:
                if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,class_name=row['class_name'],method=row['method'],test_name=display(row['test_name']),outcome=row['outcome'],classification='INHERITED_EXACT_BASELINE_IDENTITY_AND_OUTCOME' if not changed else 'REVIEW_REQUIRED',message_sha256=sha(row['message'].encode())))
        assert set(runs['Debug'])==set(runs['Release']) and all(runs['Debug'][k]['outcome']==runs['Release'][k]['outcome'] for k in runs['Debug'])
        for k,r in sorted(runs['Debug'].items()):identities.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=display(r['test_name']),raw_identity_sha256=k[0],raw_occurrence_ordinal=k[1],merged_main_debug=bases['Debug'][k]['outcome'] if k in bases['Debug'] else 'NEW_04A36',merged_main_release=bases['Release'][k]['outcome'] if k in bases['Release'] else 'NEW_04A36',final_debug=r['outcome'],final_release=runs['Release'][k]['outcome']))
    with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(identities[0]));w.writeheader();w.writerows(identities)
    save('nonpasses.json',dict(managed=nonpasses))
    attempts=[]
    for p in (RUN/'executions').glob('*/cases.json'):
        if p.parent.name.startswith(('entry-','final-')):continue
        r=load(p);attempts.append(dict(label=p.parent.name,suite=r['suite'],configuration=r['configuration'],outcomes=r['outcomes'],exit_code=r['command']['exit_code'],trx_sha256=r['sha256'],nonpasses=[dict(class_name=x['class_name'],method=x['method'],test_name=display(x['test_name']),outcome=x['outcome'],message=clean(x['message'])) for x in r['records'] if x['outcome']!='Passed']))
    save('development-attempts.json',dict(runs=attempts,qualification='Every failed attempt retained; early new tests corrected to honor frozen construction rejections and the original private HasValidZones adapter test seam. V1 mostly rejected (6 valid / 378 rejected) SPC fixture corrected explicitly, never overwritten. Historical inherited source pins remain genuine failures.'))
    return summaries,deltas,newcounts,bad,{c:dict(v) for c,v in totals.items()}
def py(label,suffix):
    folder=RUN/'commands'/f'{label}-{suffix}';raw=(folder/'stderr.log').read_bytes()
    try:t=raw.decode('utf-8-sig');encoding='utf-8-sig'
    except UnicodeDecodeError:t=raw.decode('cp1252');encoding='cp1252 log decoding; fixture culture unchanged'
    total=int(re.search(r'Ran (\d+) tests?',t).group(1));n=lambda k:int(re.search(k+r'=(\d+)',t).group(1)) if re.search(k+r'=(\d+)',t) else 0
    fails,errors=n('failures'),n('errors')
    blocks=re.split(r'(?=^(?:FAIL|ERROR): test_)',t,flags=re.M)
    return dict(label=label,suite=suffix,total=total,passed=total-fails-errors,failed=fails,errors=errors,exit_code=load(folder/'command.json')['exit_code'],stderr_sha256=sha(raw),log_decoding=encoding,identities=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M),nonpasses=[dict(outcome=k,identity=i,subtest=display(s.strip())) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)],failure_receipts=[dict(identity=m.group(1),subtest=display(m.group(2).strip()),traceback=clean(b[:12000])) for b in blocks if (m:=re.match(r'(?:FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',b))])
def source_node():
    python=[];node=[];changes=[]
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        b,a=py('entry',suffix),py('final',suffix);assert b['total']==a['total'] and Counter(b['identities'])==Counter(a['identities'])
        key=lambda r:Counter((n['outcome'],n['identity'],n['subtest']) for n in r['nonpasses'])
        for (outcome,identity,subtest),count in (key(a)-key(b)).items():changes.append(dict(outcome=outcome,identity=identity,subtest=subtest,count=count,classification='UNCLASSIFIED',receipt=next((x['traceback'] for x in a['failure_receipts'] if x['identity']==identity and x['subtest']==subtest),'')))
        a['baseline']={k:b[k] for k in ['total','passed','failed','errors','exit_code','stderr_sha256']};a['baseline_nonpasses']=b['nonpasses'];a['removed_nonpasses']=[dict(outcome=k,identity=i,subtest=s,count=n) for (k,i,s),n in (key(b)-key(a)).items()];python.append(a)
    for suffix in ['office-node','resolver','wasm-node']:
        for label in ['entry','final']:
            folder=RUN/'commands'/f'{label}-{suffix}';raw=(folder/'stdout.log').read_bytes();t=raw.decode('utf-8-sig')
            count=lambda k:int(re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t)[-1]);ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',t,re.M)
            node.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=load(folder/'command.json')['exit_code'],raw_identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()),stdout_sha256=sha(raw)))
        assert node[-1]['raw_identity_multiset_sha256']==node[-2]['raw_identity_multiset_sha256']
    save('source-and-node-validation.json',dict(python=python,node=node,new_nonpasses=changes,unclassified=changes))
    return python,[r for r in node if r['label']=='final'],changes
def receipts():save('command-receipts.json',dict(commands=[dict(name=p.parent.name,**clean(load(p))) for p in sorted((RUN/'commands').glob('*/command.json'))],qualification='Real isolated subprocess commands, exits and output hashes, including failed development attempts. Original direct capture/compiler commands are reproducible scripts; no native Excel or R executed.'))
if __name__=='__main__':
    suites,deltas,new,bad,totals=managed();python,node,pybad=source_node();assert new['Debug']==new['Release']
    strict={label:{k:load(RUN/'commands'/(label+'-'+k)/'command.json')['exit_code'] for k in ['strict','profile','docs']} for label in ['entry','final']}
    save('validation.json',dict(decision='NOT_READY_04A36',managed=suites,managed_totals=totals,new_managed_cases_per_configuration=new['Debug'],identity_deltas=deltas,unclassified_managed=bad,unclassified_python=pybad,python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']},node={k:sum(r[k] for r in node) for k in ['total','passed','failed']},strict_profile_mkdocs=strict,accepted35_reference=dict(managed_pass=20374,managed_fail=14,python_pass=813,python_fail=132,python_error=11,node_pass=916,audit_pass=32),native='NATIVE_NOT_RUN',r='R_NOT_RUN',scientific='NOT_GOLDEN',wrappers='NOT_RUN: current wrapper writes shared historical TestResults/Codex; Legacy Build invokes packaging hooks. Accepted isolated equivalents build/test all managed projects with compile-only host targets; no packaging/installer success inferred.'))
    receipts();print('Managed',totals,'new',dict(new),'Python',load(HERE/'validation.json')['python'],'Node',load(HERE/'validation.json')['node'])
