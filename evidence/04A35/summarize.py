"""Read real isolated receipts, preserve identities/multiplicity and nonzero outcomes."""
from work import *
from collections import Counter,defaultdict
RUN=ROOT/'.test-rationalization-04A35.tmp';SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def load(p):return json.loads(p.read_bytes())
def clean(v):
    if isinstance(v,list):return [clean(x) for x in v]
    if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
    if not isinstance(v,str):return v
    for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:v=v.replace(a,b)
    v=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',v,flags=re.I)
    v=re.sub(r'[A-Za-z]:[\\/]+Program Files(?: \(x86\))?[\\/]+','<PROGRAM_FILES>/',v,flags=re.I)
    v=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',v,flags=re.I)
    return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',v)
def display(v):return '<REDACTED_TEST_DISPLAY:'+sha(v.encode())[:24]+'>' if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}',v) else clean(v)
def index(records):
    buckets=defaultdict(list)
    for r in records:buckets[sha(json.dumps((r['class_name'],r['method'],r['test_name'])).encode())].append(r)
    return {(k,i):r for k,rows in buckets.items() for i,r in enumerate(sorted(rows,key=lambda x:x['outcome']))}
def managed():
    summaries=[];identities=[];nonpasses=[];bad=[];deltas=[];newcounts=Counter();totals=defaultdict(Counter)
    accepted=list(csv.DictReader((ROOT/'evidence/04A34/managed-test-identities-final.csv').open(encoding='utf-8-sig')))
    for suite in SUITES:
        runs={};base={}
        prior={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in accepted if r['suite']==suite}
        for config in ['Debug','Release']:
            before=load(RUN/f'executions/baseline-{config}-{suite}/cases.json');lane='serial' if suite=='Legacy' else 'final';after=load(RUN/f'executions/{lane}-{config}-{suite}/cases.json')
            b,a=index(before['records']),index(after['records']);base[config]=b;runs[config]=a
            assert set(b)==set(prior) and all(b[k]['outcome']==prior[k]['final_'+config.lower()] for k in b),('Fresh main identity/outcome drift',suite,config)
            removed=set(b)-set(a);changed=[k for k in b.keys()&a.keys() if b[k]['outcome']!=a[k]['outcome']];added=set(a)-set(b)
            newbad=[k for k in added if a[k]['outcome']!='Passed' or 'Graphics35' not in a[k]['class_name']]
            if removed or changed or newbad:bad.append(dict(suite=suite,configuration=config,removed=len(removed),changed=len(changed),new_nonpasses=len(newbad)))
            deltas.append(dict(suite=suite,configuration=config,removed=len(removed),changed_baseline_outcomes=len(changed),new_cases=len(added)))
            totals[config].update(after['outcomes']);newcounts[config]+=len(added)
            summaries.append(dict(suite=suite,configuration=config,baseline=before['outcomes'],final=after['outcomes'],final_execution_label=lane,exit_code=after['command']['exit_code'],baseline_exit_code=before['command']['exit_code'],baseline_trx_sha256=before['sha256'],final_trx_sha256=after['sha256']))
            for row in after['records']:
                if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,class_name=row['class_name'],method=row['method'],test_name=display(row['test_name']),outcome=row['outcome'],classification='INHERITED_EXACT_BASELINE_IDENTITY_AND_OUTCOME' if not changed else 'REVIEW_REQUIRED',message_sha256=sha(row['message'].encode())))
        assert set(runs['Debug'])==set(runs['Release']) and all(runs['Debug'][k]['outcome']==runs['Release'][k]['outcome'] for k in runs['Debug'])
        for k,r in sorted(runs['Debug'].items()):identities.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=display(r['test_name']),raw_identity_sha256=k[0],raw_occurrence_ordinal=k[1],merged_main_debug=base['Debug'][k]['outcome'] if k in base['Debug'] else 'NEW_04A35',merged_main_release=base['Release'][k]['outcome'] if k in base['Release'] else 'NEW_04A35',final_debug=r['outcome'],final_release=runs['Release'][k]['outcome']))
    with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(identities[0]));w.writeheader();w.writerows(identities)
    save('nonpasses.json',dict(managed=nonpasses))
    timing=[]
    for config in ['Debug','Release']:
        concurrent=load(RUN/f'executions/final-{config}-Legacy/cases.json');isolated=load(RUN/f'executions/boundary-{config}/cases.json');serial=load(RUN/f'executions/serial-{config}-Legacy/cases.json')
        extras=[r for r in concurrent['records'] if r['outcome']=='Failed' and 'Managed boundary test exceeded deadline' in r['message']]
        assert len(extras)==1 and extras[0]['method']=='ActualFocusEnterContainsThrowingClearAndPreservesAddress'
        assert isolated['outcomes']=={'Passed':1}
        assert all(r['outcome']=='Passed' for r in serial['records'] if r['method']==extras[0]['method'])
        timing.append(dict(configuration=config,concurrent_outcomes=concurrent['outcomes'],concurrent_exit_code=concurrent['command']['exit_code'],actual_failure=dict(class_name=extras[0]['class_name'],method=extras[0]['method'],test_name=extras[0]['test_name'],outcome='Failed',message=extras[0]['message'],duration=extras[0]['duration']),targeted_unchanged_deadline=isolated['outcomes'],serial_full_outcomes=serial['outcomes'],concurrent_trx_sha256=concurrent['sha256'],targeted_trx_sha256=isolated['sha256'],serial_trx_sha256=serial['sha256']))
    save('managed-boundary-timing.json',dict(status='REPRODUCER_PASSES_UNCHANGED_DEADLINE_AND_SERIAL_FULL_LANE',runs=timing,classification='Parallel-run managed thread deadline failures are retained real FAILs. Unchanged source/10-second assertion passes isolated and complete serial Framework rerun; no timing waiver, native/scientific gate discharge or history rewrite. Exact environmental cause is not established; concurrent broad latency is observed.'))
    return summaries,deltas,newcounts,bad,{c:dict(v) for c,v in totals.items()}
def py(label,suffix):
    folder=RUN/'commands'/f'{label}-Debug-{suffix}';raw=(folder/'stderr.log').read_bytes()
    try:t=raw.decode('utf-8-sig');encoding='utf-8-sig'
    except UnicodeDecodeError:t=raw.decode('cp1252');encoding='cp1252 log decoding; fixture locale unchanged'
    total=int(re.search(r'Ran (\d+) tests?',t).group(1));n=lambda k:int(re.search(k+r'=(\d+)',t).group(1)) if re.search(k+r'=(\d+)',t) else 0
    fails,errors=n('failures'),n('errors')
    return dict(label=label,suite=suffix,total=total,passed=total-fails-errors,failed=fails,errors=errors,exit_code=load(folder/'command.json')['exit_code'],stderr_sha256=sha(raw),log_decoding=encoding,identities=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M),nonpasses=[dict(outcome=k,identity=i,subtest=display(s.strip())) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)])
def source_node():
    python=[];node=[];changes=[]
    entry={r['path']:r['sha256'] for r in load(HERE/'entry-tracked-pins.json')['files']};changed=git('diff','--name-only').splitlines()
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        b,a=py('baseline',suffix),py('final',suffix);assert b['total']==a['total'] and Counter(b['identities'])==Counter(a['identities'])
        key=lambda r:Counter((n['outcome'],n['identity'],n['subtest']) for n in r['nonpasses'])
        for (outcome,identity,subtest),count in (key(a)-key(b)).items():
            classification='UNCLASSIFIED';proof='';paths=[p for p in changed if p in subtest]
            if 'test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact' in identity:
                classification='INHERITED_UNORDERED_SOURCE_LOOKUP_ALTERNATIVE';paths=['BESHstatNG/src/Graphics/SpcControlChartExcel.vb'];proof='SPC file unchanged in35. Original unordered old dictionary may encounter old34 HasNaturalZero KeyError first or a changed34 source hash FAIL first. Exact executed outcomes retained; not a product exception.'
            elif 'test_04a25_psm.Psm25SourceContracts.test_legacy_renderer_public_apis_and_native_geometry_remain' in identity:
                classification='AUTHORIZED_SOURCE_LITERAL_TRANSITION_RETAINED_FAIL';paths=['BESHstatNG/src/Graphics/PsmLovePlotExcel.vb','BESHStat.Excel.Common/src/Analysis/PsmLoveChartSpec35.vb'];proof='scripts/tests/test_04a25_psm.py:113-117 requires literal .ReversePlotOrder = True and direct xBefore = projection.Before. Actual assignment is now spec.ReverseY (readonly True) and spec.Before (copy of the accepted projection vector); original assertion remains FAIL. Frozen4480 rows, all8 masks and actual compiled consumer tests preserve policy.'
            changes.append(dict(outcome=outcome,identity=identity,subtest=subtest,count=count,classification=classification,proof=proof,sources=[dict(path=p,entry_sha256=entry.get(p),final_sha256=pin(p)['sha256']) for p in paths]))
        a['baseline']={k:b[k] for k in ['total','passed','failed','errors','exit_code','stderr_sha256']};a['removed_nonpasses']=[dict(outcome=k,identity=i,subtest=s,count=n) for (k,i,s),n in (key(b)-key(a)).items()];python.append(a)
    for suffix in ['office-node','resolver','wasm-node']:
        runs=[]
        for label in ['baseline','final']:
            folder=RUN/'commands'/f'{label}-Debug-{suffix}';raw=(folder/'stdout.log').read_bytes();t=raw.decode('utf-8-sig')
            count=lambda k:int(re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t)[-1])
            ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',t,re.M)
            runs.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=load(folder/'command.json')['exit_code'],identities=[display(i) for i in ids],raw_identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode()),stdout_sha256=sha(raw)))
        assert runs[0]['raw_identity_multiset_sha256']==runs[1]['raw_identity_multiset_sha256'];node+=runs
    save('source-and-node-validation.json',dict(python=python,node=node,new_nonpasses=changes,unclassified=[r for r in changes if r['classification']=='UNCLASSIFIED']))
    return python,[r for r in node if r['label']=='final'],[r for r in changes if r['classification']=='UNCLASSIFIED']
def receipts():
    save('command-receipts.json',dict(commands=[dict(name=p.parent.name,**clean(load(p))) for p in sorted((RUN/'commands').glob('*/command.json'))],qualification='Actual isolated subprocess exits including failed development builds/tests and successful corrections. Direct original-source capture and Roslyn commands are also executable scripts in this packet; no Excel or R invoked.'))
if __name__=='__main__':
    suites,deltas,new,bad,totals=managed();python,node,pybad=source_node();assert new['Debug']==new['Release']
    focused=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            r=load(RUN/f'executions/final-{config}-focused-{suite}/cases.json');focused.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],exit_code=r['command']['exit_code'],trx_sha256=r['sha256']))
    strict={k:load(RUN/'commands'/('final-'+k)/'command.json')['exit_code'] for k in ['strict','profile','docs']}
    save('validation.json',dict(decision='NOT_READY_04A35',managed=suites,managed_totals=totals,new_managed_cases_per_configuration=new['Debug'],identity_deltas=deltas,unclassified_managed=bad,unclassified_python=pybad,focused=focused,python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']},node={k:sum(r[k] for r in node) for k in ['total','passed','failed']},strict_profile_mkdocs=strict,accepted34_reference=dict(managed_pass=20326,managed_fail=14,python_pass=814,python_fail=131,python_error=11,node_pass=916,audit_pass=33),entry34_packet_audits=dict(total=33,passed=31,failed=2,errors=0,exit_code=1,reason='Git EOL normalization; original assertions unmodified. See prior225 artifact exact inverse-EOL checks.'),native='NATIVE_NOT_RUN',r='R_NOT_RUN',scientific='NOT_GOLDEN',wrappers='NOT_RUN: original wrapper hardcodes shared historical TestResults/Codex; Legacy Build also invokes unapproved packaging hooks. Accepted isolated equivalent lanes execute all test suites; no wrapper PASS invented.'))
    receipts();print('Managed',totals,'new',dict(new),'Python',load(HERE/'validation.json')['python'],'Node',load(HERE/'validation.json')['node'])
