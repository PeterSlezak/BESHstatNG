"""Sanitize actual isolated receipts; retain all inherited and newly triggered failures."""
from entry import ROOT,HERE,BASE,git,pin,sha,save
from verify_manifest import SOURCES
from pathlib import Path
from collections import Counter,defaultdict
import csv,json,os,re,sys
RUN=ROOT/'.test-rationalization-04A34.tmp'
SUITES=['Core','Common','Legacy','OfficeAddin','OfficeExecution','ControlMitigation']
def load(p):return json.loads(Path(p).read_bytes())
def read_log(p):
    b=Path(p).read_bytes()
    try:return b.decode('utf-8-sig'),'utf-8-sig'
    except UnicodeDecodeError:return b.decode('cp1252'),'cp1252 Windows subprocess log; fixture culture unchanged'
def clean(v):
    if isinstance(v,list):return [clean(x) for x in v]
    if isinstance(v,dict):return {k:clean(x) for k,x in v.items()}
    if not isinstance(v,str):return v
    for a,b in [(str(RUN),'<ISOLATED_RUN>'),(RUN.as_posix(),'<ISOLATED_RUN>'),(str(ROOT),'<REPO_ROOT>'),(ROOT.as_posix(),'<REPO_ROOT>'),(sys.executable,'<PYTHON>')]:v=v.replace(a,b)
    v=re.sub(re.escape(os.environ.get('COMPUTERNAME','!unset!')),'<HOST>',v,flags=re.I)
    v=re.sub(r'[A-Za-z]:[\\/]+Program Files(?: \(x86\))?[\\/]+','<PROGRAM_FILES>/',v,flags=re.I)
    v=re.sub(r'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+','<PRIVATE_HOME>',v,flags=re.I)
    return re.sub(r'S-1-5-\d+(?:-\d+)*','<PRIVATE_SID>',v)
def display(v):
    if re.search(r'\b[A-Za-z]:[\\/]|[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}',v):return '<REDACTED_TEST_DISPLAY:'+sha(v.encode())[:24]+'>'
    return clean(v)
def index(records):
    buckets=defaultdict(list)
    for r in records:buckets[sha(json.dumps((r['class_name'],r['method'],r['test_name'])).encode())].append(r)
    return {(k,i):r for k,values in buckets.items() for i,r in enumerate(sorted(values,key=lambda x:x['outcome']))}
def managed():
    accepted=list(csv.DictReader((ROOT/'evidence/04A33/managed-test-identities.csv').open(encoding='utf-8-sig')))
    summaries=[];identities=[];nonpasses=[];deltas=[];new_total={};unclassified=[]
    for suite in SUITES:
        old=load(RUN/f'executions/before-Debug-{suite}/cases.json');before=index(old['records'])
        reference={(r['raw_identity_sha256'],int(r['raw_occurrence_ordinal'])):r for r in accepted if r['suite']==suite}
        assert set(before)==set(reference) and all(before[i]['outcome']==reference[i]['final_debug'] for i in before),'Entry33 identity drift: '+suite
        final={}
        for config in ['Debug','Release']:
            p=RUN/f'executions/final-{config}-{suite}/cases.json';r=load(p);after=index(r['records']);final[config]=after
            removed=set(before)-set(after);changed=[i for i in before.keys()&after.keys() if before[i]['outcome']!=after[i]['outcome']]
            added=set(after)-set(before)
            bad=[i for i in added if 'Graphics34' not in after[i]['class_name'] or after[i]['outcome']!='Passed']
            if removed or changed or bad:unclassified.append(dict(suite=suite,configuration=config,removed=len(removed),changed=len(changed),bad_new=len(bad)))
            summaries.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],counters=r['counters'],baseline_debug=old['outcomes'],exit_code=r['command']['exit_code'],trx_sha256=r['sha256'],raw_receipt_sha256=sha(p.read_bytes())))
            deltas.append(dict(configuration=config,suite=suite,removed=len(removed),changed_baseline_outcomes=len(changed),new_04A34_raw_cases=len(added)))
            new_total[config]=new_total.get(config,0)+len(added)
            for row in r['records']:
                if row['outcome']!='Passed':nonpasses.append(dict(configuration=config,suite=suite,class_name=row['class_name'],method=row['method'],test_name=display(row['test_name']),outcome=row['outcome'],classification='INHERITED_EXACT_BASELINE_IDENTITY_AND_OUTCOME' if not changed else 'REQUIRES_REVIEW',message_sha256=sha(row['message'].encode())))
        assert set(final['Debug'])==set(final['Release']) and all(final['Debug'][i]['outcome']==final['Release'][i]['outcome'] for i in final['Debug'])
        for i,r in sorted(final['Debug'].items()):identities.append(dict(suite=suite,class_name=r['class_name'],method=r['method'],test_name=display(r['test_name']),raw_identity_sha256=i[0],raw_occurrence_ordinal=i[1],current_main_debug=before[i]['outcome'] if i in before else 'NEW_04A34',final_debug=r['outcome'],final_release=final['Release'][i]['outcome']))
    with (HERE/'managed-test-identities.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(identities[0]));w.writeheader();w.writerows(identities)
    save('nonpasses.json',dict(schema='04A34-managed-nonpasses-v1',nonpasses=nonpasses))
    focused=[]
    for config in ['Debug','Release']:
        for suite in ['Core','Common','Legacy']:
            p=RUN/f'executions/final-{config}-focused-{suite}/cases.json';r=load(p)
            focused.append(dict(configuration=config,suite=suite,outcomes=r['outcomes'],exit_code=r['command']['exit_code'],trx_sha256=r['sha256'],identity_sha256=sha(json.dumps(sorted(index(r['records']))).encode())))
    totals={}
    for config in ['Debug','Release']:
        t=Counter()
        for r in summaries:
            if r['configuration']==config:t.update(r['outcomes'])
        totals[config]=dict(t)
    return summaries,deltas,new_total,unclassified,focused,totals
def python_run(label,suffix):
    command_name=f'{label}-Debug-{suffix}'
    if label=='final' and suffix=='python-scripts' and (RUN/'commands/sealed-Debug-python-scripts/command.json').exists():command_name='sealed-Debug-python-scripts'
    p=RUN/'commands'/command_name/'stderr.log';t,encoding=read_log(p)
    total=int(re.search(r'Ran (\d+) tests?',t).group(1))
    def n(k):
        m=re.search(k+r'=(\d+)',t);return int(m.group(1)) if m else 0
    failures=n('failures');errors=n('errors')
    failures_list=[dict(outcome=k,identity=i,subtest=display(s.strip())) for k,i,s in re.findall(r'^(FAIL|ERROR): (test_\S+ \([^)]*\))([^\n]*)',t,re.M)]
    return dict(label=label,suite=suffix,command_receipt=command_name,total=total,passed=total-failures-errors,failed=failures,errors=errors,exit_code=load(p.parent/'command.json')['exit_code'],stderr_sha256=sha(p.read_bytes()),log_decoding=encoding,primary_identities=re.findall(r'^(test_\S+ \([^)]*\))',t,re.M),nonpasses=failures_list)
def source_node():
    python=[];nodes=[];added=[]
    changed={p for p in git('diff','--name-only').splitlines() if p.startswith('BESHstatNG/src/Graphics/')}
    entry={r['path']:r['sha256'] for r in load(HERE/'entry-pins.json')['files']}
    for suffix in ['python-tools','python-help','python-scripts','common-python']:
        old=python_run('before',suffix);new=python_run('final',suffix)
        assert old['total']==new['total'] and Counter(old['primary_identities'])==Counter(new['primary_identities'])
        key=lambda r:Counter((x['outcome'],x['identity'],x['subtest']) for x in r['nonpasses'])
        for (outcome,identity,subtest),count in (key(new)-key(old)).items():
            paths=[p for p in changed if p in subtest]
            classification='UNCLASSIFIED';proof=''
            if paths and 'test_04a28_output.Output28SourceContracts.test_core_numerics' in identity:
                classification='AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL';proof='scripts/tests/test_04a28_output.py:25-29; original expected hash unmodified'
            elif 'test_04a22_spc.Spc22Contracts.test_all_retained_native_methods_and_appearance_remain_exact' in identity:
                paths=['BESHstatNG/src/Graphics/SpcControlChartExcel.vb'];classification='AUTHORIZED_NUMERIC_MEMBER_EXTRACTION_RETAINED_NONPASS';proof='scripts/tests/test_04a22_spc.py:68; ComputeYAxisBounds and private HasNaturalZero moved to SpcAxisBoundsCore. Original unordered source-member loop may first report FAIL for changed ComputeYAxisBounds or ERROR KeyError HasNaturalZero. Both executed outcomes retained; this is historical source-dictionary inspection, not a statistical/runtime exception from the product. Frozen-source bounds comparisons pass.'
            elif 'test_04a25_psm.Psm25SourceContracts.test_common_is_strict_host_free_and_not_an_estimator' in identity:
                paths=['BESHStat.Excel.Common/src/Analysis/PsmLoveSeriesPolicy.vb'];classification='AUTHORIZED_ADDITIVE_POLICY_CENSUS_RETAINED_FAIL';proof='scripts/tests/test_04a25_psm.py:126 expects exactly five Psm*.vb; new consumed series policy makes six; old five policies unchanged'
            elif 'test_04a26_planning_agreement.PlanningAgreement26Contracts.test_old_paired_and_import_transport_authority_unchanged' in identity:
                paths=['BESHstatNG/src/Graphics/AgreementPlotExcel.vb'];classification='AUTHORIZED_WHOLE_FILE_PIN_TRANSITION_RETAINED_FAIL';proof='scripts/tests/test_04a26_planning_agreement.py:37; exact AgreementPlotExcel old/new source hash pair'
            added.append(dict(suite=suffix,outcome=outcome,identity=identity,subtest=subtest,count=count,classification=classification,proof=proof,sources=[dict(path=p,entry_sha256=entry.get(p),final_sha256=pin(p)['sha256']) for p in paths]))
        new['baseline']={k:old[k] for k in ['total','passed','failed','errors','stderr_sha256']}
        new['removed_nonpasses']=[dict(outcome=k,identity=i,subtest=s,count=n) for (k,i,s),n in (key(old)-key(new)).items()]
        python.append(new)
    for label in ['before','final']:
        for suffix in ['office-node','resolver','wasm-node']:
            p=RUN/'commands'/f'{label}-Debug-{suffix}'/'stdout.log';t=p.read_text(encoding='utf-8-sig')
            def count(k):
                a=re.findall(r'(?:# |\u2139 )'+k+r' (\d+)',t);assert a,(suffix,k);return int(a[-1])
            ids=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)',t,re.M)
            nodes.append(dict(label=label,suite=suffix,total=count('tests'),passed=count('pass'),failed=count('fail'),exit_code=load(p.parent/'command.json')['exit_code'],stdout_sha256=sha(p.read_bytes()),identities=[display(x) for x in ids],raw_identity_multiset_sha256=sha(json.dumps(sorted(ids)).encode())))
    for suffix in ['office-node','resolver','wasm-node']:
        a,b=[r for r in nodes if r['suite']==suffix];assert a['raw_identity_multiset_sha256']==b['raw_identity_multiset_sha256']
    save('source-and-node-validation.json',dict(python=python,node=nodes,new_nonpasses=added,unclassified=[r for r in added if r['classification']=='UNCLASSIFIED']))
    return python,[r for r in nodes if r['label']=='final'],added
def receipts():
    result=[]
    for p in sorted((RUN/'commands').glob('*/command.json')):
        r=load(p);result.append(dict(name=p.parent.name,**clean(r)))
    save('command-outcomes.json',dict(commands=result,qualification='Actual subprocess exit codes retained, including intermediate harness/build failures. Unchanged wrappers NOT_RUN: shared TestResults/bin/obj and Legacy packaging hooks. Compiled-source capture and Roslyn guard commands are additionally described in the dossier; no native host launched.'))
def summarize():
    summaries,deltas,new_total,unclassified,focused,totals=managed();python,nodes,added=source_node()
    assert new_total['Debug']==new_total['Release']
    strict={k:load(RUN/'commands'/('final-'+k)/'command.json')['exit_code'] for k in ['strict','profile','docs']}
    pre=pin('evidence/04A34/pre-edit-compatibility-capture.csv');post=pin('evidence/04A34/post-edit-compatibility-capture.csv')
    assert pre['sha256']==post['sha256']
    save('compatibility-casebook.json',dict(scientific='NOT_GOLDEN',bit_policy='Exact IEEE finite/infinite/signed-zero values; algorithmic NaN compared by classification in managed tests; frozen CSV also retains raw NaN bits.',captures=[dict(pin('evidence/04A34/'+name),rows=sum(1 for _ in (HERE/name).open(encoding='utf-8-sig'))-1) for name in ['pre-edit-compatibility-capture.csv','pre-edit-layout-capture.csv','pre-edit-reference-capture.csv']],actual_adapter_capture_equal=True,post_capture=post,authorities=['PRE_EDIT_LEGACY','ACCEPTED_CORE','HAND_DERIVED_INVARIANT','SAME_CORE_WIRING','SOURCE_ONLY'],limitations=['The 544-row post capture proves host-free numeric adapters only. It is not native series/shape execution. Layout/source fixtures supply separate Framework comparisons.','Unfinished member dispositions and unexercised options remain OPEN_BLOCKER. Source/fixture numeric tests are software compatibility, not scientific independence.','No R/native/WASM UI run; no claim of complete seventeen-family extraction.']))
    save('validation.json',dict(schema='04A34-validation-v1',decision='NOT_READY_04A34',managed=summaries,managed_totals=totals,identity_deltas=deltas,new_managed_cases_per_configuration=new_total['Debug'],unclassified_new_managed_nonpasses=unclassified,focused=focused,python={k:sum(r[k] for r in python) for k in ['total','passed','failed','errors']},node={k:sum(r[k] for r in nodes) for k in ['total','passed','failed']},new_python_nonpasses=sum(r['count'] for r in added),unclassified_new_python_nonpasses=[r for r in added if r['classification']=='UNCLASSIFIED'],strict_profile_mkdocs=strict,scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN',r='R_NOT_RUN',entry_full_release='NOT_RUN; fresh entryDebug plus accepted33 historicalRelease only; finalRelease executed',wrappers='NOT_RUN; isolated equivalent lanes executed; shared-output/Legacy packaging avoided',open_blockers=load(HERE/'family-coverage.json')['open_blockers']))
    receipts()
    print('Managed',totals,'new',new_total,'Python',load(HERE/'validation.json')['python'],'Node',load(HERE/'validation.json')['node'])
if __name__=='__main__':summarize()
