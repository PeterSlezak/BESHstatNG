"""Reconcile 04A.26 executed receipts and explicitly qualified entry references."""
import base64
from collections import Counter
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture26_summary','scripts/capture-04a26-contract.py')
p17=module('summary17_26','scripts/summarize-04a17.py')
p11=module('summary11_26','scripts/summarize-04a11.py')
RUN,OUT,NS=c.RUN,c.OUT,p17.NS
CACHE={};HASH_CACHE={}
def receipt_sha(path):
    st=path.stat();key=(str(path),st.st_size,st.st_mtime_ns,st.st_ctime_ns)
    if key not in HASH_CACHE: HASH_CACHE[key]=c.sha(path)
    return HASH_CACHE[key]
def trx(path):
    if str(path) not in CACHE:CACHE[str(path)]=p11.trx(path)
    return copy.deepcopy(CACHE[str(path)])
def newcase(row):return any(v in row['class_name'] for v in ['Planning26','Agreement26'])
def suite_path(config,suite):
    qualified=RUN/'executions'/('qualified-'+config+'-'+suite)/'results'/(suite+'.trx')
    if suite in ['Common','Legacy'] and qualified.exists():return qualified
    encoding=RUN/'executions/encoding-Debug-Legacy/results/Legacy.trx'
    if config=='Debug' and suite=='Legacy' and encoding.exists():return encoding
    repeated=RUN/'executions'/('requalified-'+config+'-'+suite)/'results'/(suite+'.trx')
    return repeated if suite in ['Common','Legacy'] and repeated.exists() else RUN/'executions'/('final-'+config+'-'+suite)/'results'/(suite+'.trx')
def fingerprint(value):return hashlib.sha256(value.encode()).hexdigest()
def log(name):return '\n'.join((RUN/'commands'/name/(v+'.log')).read_text(encoding='utf-8',errors='replace') for v in ['stdout','stderr'])
def python_receipts(label):
    paths=[RUN/'commands'/(label+'-python-'+n)/'stderr.log' for n in ['tools','help','scripts']]+[RUN/'commands'/(label+'-common-python')/'stderr.log']
    repeated=RUN/'commands'/('requalified-'+label.split('-')[-1]+'-python-scripts')/'stderr.log'
    qualified=RUN/'commands'/('qualified-'+label.split('-')[-1]+'-python-scripts')/'stderr.log'
    if qualified.exists():repeated=qualified
    if label.startswith('final-') and repeated.exists():paths[2]=repeated
    py=p17.python(paths)
    py.update(failure_methods=sum(v=='FAIL' for v in py['nonpasses'].values()),error_methods=sum(v=='ERROR' for v in py['nonpasses'].values()),executed=py['total'],reused_not_run=0)
    traces=[]
    for path in paths:
        for part in re.split(r'^={60,}\s*$',path.read_text(encoding='utf-8',errors='replace'),flags=re.M):
            match=re.match(r'\s*(FAIL|ERROR): ([^\n]+)',part)
            if match:traces.append(dict(receipt='LOCAL_LOG:'+path.parent.name,identity=match[2],outcome=match[1],trace=part.split('\nRan ')[0]))
    c.emit(label+'-python-nonpass-traces.json',dict(traces=traces,receipt_hashes={p.parent.name:receipt_sha(p) for p in paths}))
    return py
def entry():
    old=ROOT/'evidence/04A25';refs={};baseline=json.loads((RUN/'baseline.json').read_bytes())['pins']
    for config in ['Debug','Release']:
        build=ROOT/'TestResults/04A-25/run01/builds'/('delivery-build-'+config+'.json')
        pins=json.loads(build.read_bytes())['source_pins'];different=[]
        assert set(pins)=={p for p in baseline if p.endswith(('.vb','.vbproj','.cs','.csproj')) and not p.startswith(('TestResults/','evidence/'))},'Entry compiled-source membership changed'
        for p,h in pins.items():
            actual=baseline.get(p,c.sha(ROOT/p))
            if actual==h:continue
            # Two accepted publication CRLF/LF differences were observed before edits.
            raw=(ROOT/p).read_bytes().replace(b'\r\n',b'\n')
            assert h in {hashlib.sha256(raw).hexdigest(),hashlib.sha256(raw.replace(b'\n',b'\r\n')).hexdigest()},p
            different.append(dict(path=p,accepted_sha256=h,entry_sha256=actual,classification='LINE_ENDINGS_ONLY'))
        prior=json.loads((old/('final-'+config+'-raw-test-identities.json')).read_bytes())
        receipts=[]
        for suite in prior['suites']:
            assert c.sha(ROOT/suite['path'])==suite['sha256']
            receipts.append(dict(path=suite['path'],sha256=suite['sha256'],raw_cases=len(suite['records'])))
        oldcommands={r['name']:r for r in json.loads((old/'command-outcomes.json').read_bytes())['commands']}
        node_receipts=[]
        for suffix in ['office-js','resolver','wasm-js']:
            name='final-'+config+'-'+suffix
            folder=ROOT/'TestResults/04A-25/run01/commands'/name
            for stream in ['stdout','stderr']:assert c.sha(folder/(stream+'.log'))==oldcommands[name][stream+'_sha256']
            node_receipts.append(dict(name=name,stdout_sha256=c.sha(folder/'stdout.log'),stderr_sha256=c.sha(folder/'stderr.log'),qualification='NOT_RUN_REUSED_EVIDENCE'))
        for suffix in ['logical-test-identities','raw-test-identities']:
            value=json.loads((old/('final-'+config+'-'+suffix+'.json')).read_bytes())
            value['execution_qualification']='NOT_RUN_REUSED_EVIDENCE: hash-verified accepted 04A.25 receipts; all 801 compiled/test inputs entry-qualified (799 exact, two line-ending-only).'
            c.emit('entry-'+config+'-'+suffix+'.json',value)
        accepted=json.loads((old/'validation.json').read_bytes())['configurations'][config]
        refs[config]={k:accepted[k] for k in ['logical_cases','runtime_executions','managed_suites','node_total','node_passed']}
        refs[config].update(qualification='NOT_RUN_REUSED_EVIDENCE / immutable accepted managed+Node receipts; not a fresh full branch-entry execution',receipt_files=receipts,node_receipts=node_receipts,compiled_members=len(pins),line_ending_differences=different,compiled_receipt_sha256=c.sha(build))
        refs[config]['python']=python_receipts('entry-Debug') if config=='Debug' else dict(executed=0,total=None,passed=None,nonpasses={},qualification='NOT_RUN: Release-specific branch-entry Python. Fresh Debug source inventory is the entry comparison; no duplicated pass claim.')
    # Every Node .mjs/.cjs and imported JS source is baseline-identical.
    node_sources={p:h for p,h in baseline.items() if p.endswith(('.mjs','.cjs','.js','.cs','.csproj')) and any(v in p for v in ['Office','Wasm','Help'])}
    for p,h in node_sources.items():assert c.sha(ROOT/p)==h,p
    focused=[]
    for p in sorted((RUN/'commands').glob('entry*/*command.json')):focused.append(json.loads(p.read_bytes()))
    c.emit('entry-validation.json',dict(configurations=refs,qualification='Entry managed identities reused only after SHA qualification of actual main input membership and accepted raw TRX. Focused freshly built main tests and full Debug Python were additionally executed. No unexecuted full matrix is labelled executed.',entry_attempts=focused,node_source_pins=node_sources,full_entry_release_python='NOT_RUN; final complete Python execution is required in each configuration.',prior='evidence/04A25/validation.json'))
    print('Qualified entry receipts; fresh entry Python',refs['Debug']['python']['passed'],refs['Debug']['python']['total'])
def measure(config):
    label='final-'+config
    suites=[trx(suite_path(config,s)) for s in ['Core','OfficeAddin','OfficeExecution','Common','Legacy','ControlMitigation']]
    groups=p17.typed_groups(suites);weight=p17.prior.weight
    def canonical(k,rows):
        if k[0]=='shared-typed-null-cohort':return repr(k)
        r=rows[0];return repr((r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''),tuple(r.get('source_hashes',[])),weight(k)))
    identities=[dict(identity=repr(k),raw_identity_sha256=fingerprint(repr(k)),canonical_case_sha256=fingerprint(canonical(k,rows)),weight=weight(k),**{n:rows[0][n] for n in ['class_name','method','test_name']},outcomes=[r['outcome'] for r in rows],source_hashes=rows[0].get('source_hashes',[])) for k,rows in groups.items()]
    c.emit(label+'-logical-test-identities.json',dict(cases=identities,accounting='Accepted shared-source/null-cohort accounting; linked tests counted once logically.'))
    raw=[]
    for suite in suites:
        tree=ET.parse(ROOT/suite['path']);messages={n.attrib['testName']:n.findtext('.//'+NS+'Message','') for n in tree.findall('.//'+NS+'UnitTestResult') if n.attrib['outcome']!='Passed'}
        for row in suite['nonpasses']:row['message']=messages.get(row['test_name'],'')
        for row in suite['records']:row['raw_identity_sha256']=fingerprint(repr((row['class_name'],row['method'],row['test_name'])))
        raw.append(dict(path=suite['path'],sha256=suite['sha256'],records=suite.pop('records')));suite.pop('batch11',None)
    c.emit(label+'-raw-test-identities.json',dict(suites=raw))
    new={k:rows for k,rows in groups.items() if any(newcase(r) for r in rows)}
    nodes='\n'.join(log(label+'-'+n) for n in ['office-js','resolver','wasm-js'])
    return dict(managed_suites=suites,logical_cases=dict(total=sum(weight(k) for k in groups),passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)),nonpassed=sum(weight(k) for k,rows in groups.items() if any(r['outcome']!='Passed' for r in rows))),runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),nonpassed=sum(int(s['counters']['total'])-int(s['counters']['passed']) for s in suites)),new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,rows in new.items() if all(r['outcome']=='Passed' for r in rows)),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',nodes,re.M))),node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',nodes,re.M))),python=python_receipts(label),lanes=json.loads((RUN/'lanes'/(label+'-summary.json')).read_bytes()))
def inventory():
    cases={}
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=suite_path(config,suite)
            for r in trx(path)['records']:
                if not newcase(r):continue
                key=r['class_name'].split('.')[-1]+'|'+r['test_name'].replace('"','')
                p='BESHStatNG.Test/src/Agreement26AdapterTests.vb' if 'Adapter' in r['class_name'] else 'BESHStat.Excel.Common.Tests/src/'+r['class_name'].split('.')[-1]+'.vb'
                item=cases.setdefault(key,dict(source=p,source_sha256=c.sha(ROOT/p),method=r['method'],requirement='Shipping request/report call graph' if 'Adapter' in p else 'Legacy request, options, input ownership, Core rejection and report parity',executions=[]))
                item['executions'].append(dict(configuration=config,suite=suite,outcome=r['outcome'],raw_trx_sha256=receipt_sha(path)))
    p='scripts/tests/test_04a26_planning_agreement.py';methods=re.findall(r'^    def (test_\w+)\(self\):',(ROOT/p).read_text(encoding='utf-8'),re.M)
    py={n:dict(source=p,sha256=c.sha(ROOT/p),executions=[dict(configuration=cfg,outcome='PASS' if re.search(r'^'+re.escape(n)+r' \(test_04a26_planning_agreement\.[^\n]+\) \.\.\. ok\s*$',log('final-'+cfg+'-python-scripts'),re.M) else 'NONPASS_OR_NOT_FOUND') for cfg in ['Debug','Release']]) for n in methods}
    assert all(e['outcome']=='Passed' for r in cases.values() for e in r['executions'])
    assert all(e['outcome']=='PASS' for r in py.values() for e in r['executions'])
    c.emit('new-test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(py),python_cases=py,counting='Core/Common linked cases counted once logically; runtime executions and corpus records are separate.'))
def xml(node):return dict(tag=node.tag,attributes=node.attrib,text=node.text or '',children=[xml(n) for n in node])
def corpus():
    records=[];payloads={};classes=Counter();families=Counter()
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=suite_path(config,suite)
            for node in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for line in node.findtext('.//'+NS+'StdOut','').splitlines():
                    if not line.startswith('04A26_XML|'):continue
                    root=ET.fromstring(base64.b64decode(line.split('|',1)[1]));payload=xml(root)
                    key=hashlib.sha256(json.dumps(payload,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest()
                    payloads.setdefault(key,payload);classes[root.attrib['classification']]+=1;families[root.attrib['family']]+=1
                    records.append(dict(payload_sha256=key,configuration=config,runtime=suite,test_identity=node.attrib['testName'],test_outcome=node.attrib['outcome'],raw_trx_alias='LOCAL_TRX:'+path.parents[1].name,raw_trx_sha256=receipt_sha(path)))
    buckets=[];keys=sorted(payloads)
    for i in range(0,len(keys),128):
        name='sample-agreement-payloads-'+str(i//128).zfill(3)+'.json';chosen=keys[i:i+128]
        c.emit(name,dict(payloads={k:payloads[k] for k in chosen}));buckets.append(dict(path='evidence/04A26/'+name,sha256=c.sha(OUT/name),payload_count=len(chosen)))
    sources=['Planning26Tests','Planning26LegacyCore','Planning26LegacyText','Agreement26Tests','Agreement26Legacy','Agreement26Reference']
    c.emit('sample-agreement-same-core-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',fixtures={n:c.sha(ROOT/('BESHStat.Excel.Common.Tests/src/'+n+'.vb')) for n in sources},origin='Bounded synthetic arrays; prechange Core argument and report derivatives. Existing Core APIs only.',record_count=len(records),payload_count=len(payloads),classifications=dict(classes),families=dict(families),records=records,payload_files=buckets,exactness='Same-runtime typed result hashes and exact original paragraph strings under invariant/sk-SK. Deterministic test-only Core seed 12345 where exposed; native default random seed unchanged. No independent numerical golden/native chart comparison.',seed=12345,native='NOT_RUN',R='NOT_RUN'))
    outcomes={}
    for record in records:
        payload=payloads[record['payload_sha256']];a=payload['attributes'];key=a['family']+'.'+a['method']
        item=outcomes.setdefault(key,dict(classifications={},exceptions=[]))
        classification=a['classification'];item['classifications'][classification]=item['classifications'].get(classification,0)+1
        for child in payload['children']:
            if child['tag']=='exception':
                failure=dict(type=child['attributes']['type'],message=child['text'])
                if failure not in item['exceptions']:item['exceptions'].append(failure)
    assert all(outcomes['agreement.'+str(i)]['classifications'].get('POSITIVE_PARITY',0)>0 for i in range(5)), 'Missing positive method profile'
    c.emit('same-core-outcome-matrix.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',methods=outcomes,agreement_method_order=['Deming','Lin concordance','Kappa','Bland–Altman','ICC'],all_five_agreement_methods_have_executed_positive_profiles=True))
    for name,scope in [('sample-size-family-and-hypothesis-requests-v1.json','Eight families / thirteen scientific branches; beta remains type-II error, not power; original NI/EQ signed margins and Cox sentinels.'),('sample-size-text-and-allocation-parity-v1.json','Original text derivative compares exact Core-returned allocation/rounding/optional sentinel text and CRLF order; original append/save members preserved.'),('agreement-paired-axis-and-subject-provenance-v1.json','Nonconsecutive physical row IDs, observation order, subject/category labels, optional unknown source identity; defensive copies and old paired policy.'),('agreement-ci-and-variance-option-matrix-v1.json','Deming 96 radio/variance combinations, Kappa 112 weight/CI combinations, Lin 8 precedence masks and ICC 64 selection masks; native resampling path preserved.'),('agreement-result-layout-and-plot-parity-v1.json','Fresh Core ResultTable list, native standard rows+sections-1 versus Bland rows+2sections-1; Deming/Lin/Bland plots only; decision tables retain bias then LoA.')]:
        c.emit(name,dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',scope=scope,corpus='sample-agreement-same-core-v1.json',tests='new-test-inventory.json',shipping='prechange-method-provenance.json and final-source-qualification.json',native='NOT_RUN / no visual or destination acceptance'))
def final():
    configs={cfg:measure(cfg) for cfg in ['Debug','Release']};initial=json.loads((OUT/'entry-validation.json').read_bytes())['configurations']
    for cfg,d in configs.items():
        old=json.loads((OUT/('entry-'+cfg+'-logical-test-identities.json')).read_bytes())['cases'];now=json.loads((OUT/('final-'+cfg+'-logical-test-identities.json')).read_bytes())['cases']
        sig=lambda r:(r['canonical_case_sha256'],r['weight'])
        assert Counter(sig(r) for r in old)==Counter(sig(r) for r in now if not newcase(r)),(cfg,'prior managed inventory drift')
        d['prior_managed_inventory_exact']=True
        identity=lambda r:(r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''))
        before={identity(r) for s in initial[cfg]['managed_suites'] for r in s['nonpasses']};after={identity(r) for s in d['managed_suites'] for r in s['nonpasses']}
        d['new_managed_nonpasses']=sorted(after-before);d['removed_managed_nonpasses']=sorted(before-after)
        entry_py=initial['Debug']['python']['nonpasses']
        d['new_python_nonpasses']={k:v for k,v in d['python']['nonpasses'].items() if k not in entry_py}
        d['removed_python_nonpasses']={k:v for k,v in entry_py.items() if k not in d['python']['nonpasses']}
        print(cfg,d['logical_cases'],d['runtime_executions'],'New',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'],d['python']['failure_methods'],d['python']['error_methods'],flush=True)
    c.emit('validation.json',dict(configurations=configs,status='NONZERO_FULL_AGGREGATES_RETAINED',golden_promotion=False,native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED'))
    c.emit('nonpasses.json',dict(final={cfg:dict(managed=[r for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses'],new_managed=d['new_managed_nonpasses'],new_python=d['new_python_nonpasses']) for cfg,d in configs.items()},entry='entry-validation.json',classification='Inherited and newly triggered protected literal-source failures remain real failures; functional successor checks supplement them. No historical assertion weakened.'))
    c.emit('entry-final-test-identities.json',dict(entry='entry-validation.json',final='validation.json',files=[phase+'-'+cfg+'-'+suffix+'.json' for phase in ['entry','final'] for cfg in configs for suffix in ['logical-test-identities','raw-test-identities']],python_entry='Debug freshly executed; Release-specific entry NOT_RUN. Final Python executed fully in both.',prior_managed_inventory_exact=True))
    inventory();corpus()
    repeats={phase+'-'+cfg:json.loads((RUN/'commands'/(phase+'-'+cfg+'-python-scripts')/'command.json').read_bytes()) for cfg in configs for phase in ['requalified','qualified'] if (RUN/'commands'/(phase+'-'+cfg+'-python-scripts')/'command.json').exists()}
    if repeats:c.emit('python-requalification.json',dict(reason='New dossier-helper prose matched a frozen path scanner. Two new Agreement26 fixture/reference files were strengthened for valid >=200 replicate fits, correctly dimensioned ICC subject axes and full typed numeric/table snapshots. Affected full Common/Windows and scripts discovery repeated per configuration; entry encoding restored. Product/Core and frozen tests unchanged.',initial='initial-full-matrix-validation.json',commands=repeats,repeated_methods_per_scripts_root=719,distinct_final_methods_per_configuration=930,raw_method_invocations_per_configuration=2368,executed_results_reused=0,qualification='All initial receipts/nonpasses retained; repetitions never inflate distinct totals. Other executed source-identical roots remain qualified.'))
    for key,h in HASH_CACHE.items():assert c.sha(Path(key[0]))==h
    c.emit('receipt-hash-cache.json',dict(files=[dict(alias='LOCAL_RECEIPT:'+Path(k[0]).parent.name,sha256=h,bytes=k[1]) for k,h in HASH_CACHE.items()],scope='Metadata-keyed receipt accounting with terminal SHA reread; no test-method result execution omitted or reused in final matrices.'))
def debug():
    d=measure('Debug')
    old=json.loads((OUT/'entry-Debug-logical-test-identities.json').read_bytes())['cases']
    now=json.loads((OUT/'final-Debug-logical-test-identities.json').read_bytes())['cases']
    sig=lambda r:(r['canonical_case_sha256'],r['weight'])
    assert Counter(sig(r) for r in old)==Counter(sig(r) for r in now if not newcase(r)),'Prior managed inventory drift'
    print('Debug',d['logical_cases'],d['runtime_executions'],'New',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'],d['python']['failure_methods'],d['python']['error_methods'])
    before=json.loads((OUT/'entry-validation.json').read_bytes())['configurations']['Debug']['python']['nonpasses']
    print('Additional Python:',[(k,v) for k,v in d['python']['nonpasses'].items() if k not in before])
def snapshot():
    assert not (OUT/'initial-full-matrix-validation.json').exists()
    c.emit('initial-full-matrix-validation.json',json.loads((OUT/'validation.json').read_bytes()))
def profile_snapshot():
    value=json.loads((OUT/'sample-agreement-same-core-v1.json').read_bytes())
    c.emit('initial-profile-coverage.json',dict(record_count=value['record_count'],payload_count=value['payload_count'],classifications=value['classifications'],fixture_hashes=value['fixtures'],qualification='Initial truthful Core rejection comparisons used 30 replicate options, below Deming 50 and Lin/Bland bootstrap 200 minima. Valid positive profiles subsequently use 200; explicit low-replicate rejection retained. ICC probe-axis count corrected from generic 12 to the actual five-subject matrix. Full typed input/result/table snapshots added. Only two new fixture/reference files changed; original full TRX/build receipts retained.'))
if __name__=='__main__':globals()[sys.argv[1]]()
