"""Reconcile actual 04A.25 receipts; shared-case weights use accepted accounting."""
import base64
from collections import Counter
import copy
import importlib.util
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture25_summary','scripts/capture-04a25-contract.py')
p17=module('summary17_25','scripts/summarize-04a17.py')
p11=module('summary11_25','scripts/summarize-04a11.py')
RUN,OUT,NS=c.RUN,c.OUT,p17.NS
CACHE={}
HASH_CACHE={}
HASH_REUSES=0
def receipt_sha(path):
    global HASH_REUSES
    stat=path.stat();key=(str(path),stat.st_size,stat.st_mtime_ns,stat.st_ctime_ns)
    if key not in HASH_CACHE:HASH_CACHE[key]=c.sha(path)
    else:HASH_REUSES+=1
    assert path.stat().st_size==stat.st_size and path.stat().st_mtime_ns==stat.st_mtime_ns
    return HASH_CACHE[key]
def trx(path):
    if str(path) not in CACHE:CACHE[str(path)]=p11.trx(path)
    return copy.deepcopy(CACHE[str(path)])
def log(name):return '\n'.join((RUN/'commands'/name/(v+'.log')).read_text(encoding='utf-8',errors='replace') for v in ['stdout','stderr'])
def measure(phase,config):
    label=phase+'-'+config
    suites=[trx(RUN/'executions'/(label+'-'+s)/'results'/(s+'.trx')) for s in ['Core','OfficeAddin','OfficeExecution','Common','Legacy','ControlMitigation']]
    groups=p17.typed_groups(suites);weight=p17.prior.weight
    fingerprint=lambda v:__import__('hashlib').sha256(v.encode()).hexdigest()
    def canonical_case(k,rows):
        if k[0]=='shared-typed-null-cohort':return repr(k)
        r=rows[0]
        return repr((r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''),tuple(r.get('source_hashes',[])),weight(k)))
    inventory=[dict(identity=repr(k),raw_identity_sha256=fingerprint(repr(k)),canonical_case_sha256=fingerprint(canonical_case(k,rows)),weight=weight(k),**{n:rows[0][n] for n in ['class_name','method','test_name']},outcomes=[r['outcome'] for r in rows],source_hashes=rows[0].get('source_hashes',[])) for k,rows in groups.items()]
    c.emit(label+'-logical-test-identities.json',dict(cases=inventory,accounting='Accepted shared-source dedup, explicit typed null/empty cohort weights; focused/semantic repeats excluded.'))
    raw=[];new={k:rows for k,rows in groups.items() if any('Psm25' in r['class_name'] for r in rows)}
    for suite in suites:
        tree=ET.parse(ROOT/suite['path']);messages={n.attrib['testName']:n.findtext('.//'+NS+'Message','') for n in tree.findall('.//'+NS+'UnitTestResult') if n.attrib['outcome']!='Passed'}
        for row in suite['nonpasses']:row['message']=messages.get(row['test_name'],'')
        for row in suite['records']:row['raw_identity_sha256']=fingerprint(repr((row['class_name'],row['method'],row['test_name'])))
        raw.append(dict(path=suite['path'],sha256=suite['sha256'],records=suite.pop('records')));suite.pop('batch11',None)
    c.emit(label+'-raw-test-identities.json',dict(suites=raw))
    logs=[RUN/'commands'/(label+'-python-'+n)/'stderr.log' for n in ['tools','help','scripts']]+[RUN/'commands'/(label+'-common-python')/'stderr.log']
    requalified=RUN/'commands'/('requalified-'+config+'-python-scripts')/'stderr.log'
    if phase=='final' and requalified.exists():logs[2]=requalified
    py=p17.python(logs)
    py.update(failure_methods=sum(v=='FAIL' for v in py['nonpasses'].values()),error_methods=sum(v=='ERROR' for v in py['nonpasses'].values()),executed=py['total'],reused_not_run=0)
    traces=[]
    for path in logs:
        for part in re.split(r'^={60,}\s*$',path.read_text(encoding='utf-8',errors='replace'),flags=re.M):
            m=re.match(r'\s*(FAIL|ERROR): ([^\n]+)',part)
            if m:traces.append(dict(receipt='LOCAL_LOG:'+path.parent.name,identity=m[2],outcome=m[1],trace=part.split('\nRan ')[0]))
    c.emit(label+'-python-nonpass-traces.json',dict(traces=traces))
    nodes='\n'.join(log(label+'-'+n) for n in ['office-js','resolver','wasm-js'])
    return dict(managed_suites=suites,logical_cases=dict(total=sum(weight(k) for k in groups),passed=sum(weight(k) for k,rows in groups.items() if all(r['outcome']=='Passed' for r in rows)),nonpassed=sum(weight(k) for k,rows in groups.items() if any(r['outcome']!='Passed' for r in rows))),runtime_executions=dict(total=sum(int(s['counters']['total']) for s in suites),passed=sum(int(s['counters']['passed']) for s in suites),nonpassed=sum(int(s['counters']['total'])-int(s['counters']['passed']) for s in suites)),new_logical_total=sum(weight(k) for k in new),new_logical_passed=sum(weight(k) for k,rows in new.items() if all(r['outcome']=='Passed' for r in rows)),node_total=sum(map(int,re.findall(r'\btests (\d+)\s*$',nodes,re.M))),node_passed=sum(map(int,re.findall(r'\bpass (\d+)\s*$',nodes,re.M))),python=py,lanes=json.loads((RUN/'lanes'/(label+'-summary.json')).read_bytes()))
def entry():
    configs={config:measure('entry',config) for config in ['Debug','Release']}
    c.emit('entry-validation.json',dict(configurations=configs,qualification='Actual live entry binary and source runs in both configurations before Windows/source consumers changed. Two new Common contract declarations existed without tests/consumers; their exact source pins are in entry build receipts. No historical totals substituted. All failures retained. Entry strict/profile measured separately.',historical_anchor='evidence/04A24/validation.json; immutable context only'))
    for config,d in configs.items():print('Entry',config,d['logical_cases'],d['runtime_executions'],'Python',d['python']['passed'],d['python']['total'],flush=True)
def debug():
    value=measure('final','Debug');initial=json.loads((OUT/'entry-validation.json').read_bytes())['configurations']['Debug']
    print('Debug',value['logical_cases'],value['runtime_executions'],'New',value['new_logical_passed'],value['new_logical_total'],'Python',value['python']['passed'],value['python']['total'],value['python']['failure_methods'],value['python']['error_methods'])
    print('New Python nonpasses:',[(k,v) for k,v in value['python']['nonpasses'].items() if k not in initial['python']['nonpasses']])
    historical=json.loads((ROOT/'evidence/04A24/validation.json').read_bytes())['configurations']['Debug']['python']['nonpasses']
    print('Additional live entry Python:',[(k,v) for k,v in initial['python']['nonpasses'].items() if k not in historical])
def membership():
    before=json.loads((OUT/'entry-Debug-logical-test-identities.json').read_bytes())['cases']
    after=json.loads((OUT/'final-Debug-logical-test-identities.json').read_bytes())['cases']
    old={r['raw_identity_sha256']:r for r in before};new={r['raw_identity_sha256']:r for r in after if 'Psm25' not in r['class_name']}
    print('Identity hash differences',len(old.keys()-new.keys()),len(new.keys()-old.keys()))
    for label,items in [('entry',[old[k] for k in old.keys()-new.keys()]),('final',[new[k] for k in new.keys()-old.keys()])]:
        for r in items[:20]:print(label,r['class_name'],r['method'],r['test_name'],r['identity'])
def final():
    configs={config:measure('final',config) for config in ['Debug','Release']}
    initial=json.loads((OUT/'entry-validation.json').read_bytes())['configurations']
    ident=lambda r:(r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"',''))
    for config,d in configs.items():
        before_cases=json.loads((OUT/('entry-'+config+'-logical-test-identities.json')).read_bytes())['cases']
        after_cases=json.loads((OUT/('final-'+config+'-logical-test-identities.json')).read_bytes())['cases']
        signature=lambda r:(r['canonical_case_sha256'],r['weight'])
        assert Counter(signature(r) for r in before_cases)==Counter(signature(r) for r in after_cases if 'Psm25' not in r['class_name']),'Entry/final prior managed inventory drift'
        d['prior_managed_inventory_exact']=True
        for suite in ['Common','Legacy']:
            focused='focused-10' if config=='Debug' else 'focused-11'
            identities=lambda path:Counter((r['class_name'].split('.')[-1],r['method'],r['test_name'].replace('"','')) for r in trx(path)['records'] if 'Psm25' in r['class_name'])
            final_path=RUN/'executions'/('final-'+config+'-'+suite)/'results'/(suite+'.trx')
            focused_path=RUN/'executions'/(focused+'-'+suite)/'results'/(suite+'.trx')
            assert identities(final_path)==identities(focused_path),(config,suite,'new focused/full inventory drift')
        before={ident(r) for s in initial[config]['managed_suites'] for r in s['nonpasses']};after={ident(r) for s in d['managed_suites'] for r in s['nonpasses']}
        d['new_managed_nonpasses']=sorted(after-before);d['removed_managed_nonpasses']=sorted(before-after)
        d['new_python_nonpasses']={k:v for k,v in d['python']['nonpasses'].items() if k not in initial[config]['python']['nonpasses']}
        d['removed_python_nonpasses']={k:v for k,v in initial[config]['python']['nonpasses'].items() if k not in d['python']['nonpasses']}
        d['changed_python_nonpass_outcomes']={k:dict(entry=initial[config]['python']['nonpasses'][k],final=v) for k,v in d['python']['nonpasses'].items() if k in initial[config]['python']['nonpasses'] and initial[config]['python']['nonpasses'][k]!=v}
        print('Final',config,d['logical_cases'],d['runtime_executions'],'New',d['new_logical_passed'],d['new_logical_total'],'Node',d['node_passed'],d['node_total'],'Python',d['python']['passed'],d['python']['total'],flush=True)
    c.emit('validation.json',dict(configurations=configs,status='NONZERO_FULL_AGGREGATES_RETAINED',golden_promotion=False,native='NOT_RUN / UNAUTHORIZED',independent_R='NOT_RUN / UNAUTHORIZED'))
    c.emit('nonpasses.json',dict(entry=initial,final={config:dict(managed=[r for s in d['managed_suites'] for r in s['nonpasses']],python=d['python']['nonpasses'],new_managed=d['new_managed_nonpasses'],new_python=d['new_python_nonpasses']) for config,d in configs.items()},classification='Original failures remain real failures; exact traces retained. No old source assertion modified. New functional successor tests supplement extraction-affected pins.'))
    c.emit('entry-final-test-identities.json',dict(entry='entry-validation.json',final='validation.json',files=[phase+'-'+config+'-'+suffix+'.json' for phase in ['entry','final'] for config in configs for suffix in ['logical-test-identities','raw-test-identities','python-nonpass-traces']],counts={config:dict(entry=initial[config]['logical_cases'],final=d['logical_cases'],new_managed_nonpasses=d['new_managed_nonpasses'],new_python_nonpasses=d['new_python_nonpasses']) for config,d in configs.items()}))
    qualified={config:json.loads((RUN/'commands'/('requalified-'+config+'-python-scripts')/'command.json').read_bytes()) for config in configs if (RUN/'commands'/('requalified-'+config+'-python-scripts')/'command.json').exists()}
    if qualified:c.emit('python-requalification.json',dict(reason='Evidence-only privacy sanitization of synthetic fixture paths/credential URLs in managed identities; product/compiled test/Python test source unchanged.',initial='initial-full-matrix-validation.json',commands=qualified,repeat_script_methods_per_configuration=699,unique_python_methods_per_configuration=910,executed_raw_method_invocations_per_configuration=1609,not_run_reused_methods=0,accounting='Initial full roots retained. Replace only the affected complete scripts discovery receipt for final classification; other roots already executed in that same configuration. Additional checks are repetitions, never extra distinct cases.'))
    inventory();corpus();projections();receipts()
def inventory():
    cases={}
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=RUN/'executions'/('final-'+config+'-'+suite)/'results'/(suite+'.trx')
            for row in trx(path)['records']:
                if 'Psm25' not in row['class_name']:continue
                key=row['class_name'].split('.')[-1]+'|'+row['test_name'].replace('"','')
                p='BESHStatNG.Test/src/Psm25AdapterTests.vb' if 'Adapter' in row['class_name'] else 'BESHStat.Excel.Common.Tests/src/Psm25Tests.vb'
                item=cases.setdefault(key,dict(source=p,source_sha256=c.sha(ROOT/p),method=row['method'],executions=[]))
                item['executions'].append(dict(configuration=config,suite=suite,outcome=row['outcome'],raw_trx_sha256=receipt_sha(path)))
    p='scripts/tests/test_04a25_psm.py';methods=re.findall(r'^    def (test_\w+)\(self\):',(ROOT/p).read_text(),re.M)
    c.emit('new-test-inventory.json',dict(logical_count=len(cases),cases=cases,python_count=len(methods),python_cases={name:dict(source=p,sha256=c.sha(ROOT/p),executions=[dict(configuration=config,receipt='LOCAL_LOG:final-'+config+'-python-scripts',outcome='PASS' if re.search(r'^'+re.escape(name)+r' \(test_04a25_psm\.[^\n]+\) \.\.\. ok\s*$',log('final-'+config+'-python-scripts'),re.M) else 'NONPASS_OR_NOT_FOUND') for config in ['Debug','Release']]) for name in methods},counting='Linked Common cases counted once logically, once per runtime raw; corpus observations do not inflate test counts.'))
def xml(node):
    return dict(tag=node.tag,attributes=node.attrib,text=node.text or '',children=[xml(n) for n in node])
def corpus():
    records=[];payloads={};classes=Counter()
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=RUN/'executions'/('final-'+config+'-'+suite)/'results'/(suite+'.trx')
            for node in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for line in node.findtext('.//'+NS+'StdOut','').splitlines():
                    if not line.startswith('04A25_XML|'):continue
                    root=ET.fromstring(base64.b64decode(line.split('|',1)[1]));payload=xml(root)
                    canonical=json.dumps(payload,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode();key=__import__('hashlib').sha256(canonical).hexdigest()
                    payloads.setdefault(key,payload);classes[root.attrib['classification']]+=1
                    records.append(dict(payload_sha256=key,configuration=config,runtime=suite,test_identity=node.attrib['testName'],test_outcome=node.attrib['outcome'],raw_trx_alias='LOCAL_TRX:'+path.parents[1].name,raw_trx_sha256=receipt_sha(path)))
    buckets=[];keys=sorted(payloads)
    for i in range(0,len(keys),128):
        name='psm-same-core-payloads-'+str(i//128).zfill(3)+'.json';chosen=keys[i:i+128]
        c.emit(name,dict(payloads={k:payloads[k] for k in chosen}))
        buckets.append(dict(path='evidence/04A25/'+name,sha256=c.sha(OUT/name),payload_count=len(chosen)))
    c.emit('psm-matching-weighting-balance-same-core-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',fixture=dict(path='BESHStat.Excel.Common.Tests/src/Psm25Fixture.vb',sha256=c.sha(ROOT/'BESHStat.Excel.Common.Tests/src/Psm25Fixture.vb'),origin='Bounded synthetic deterministic values, no private workbook or external oracle.'),record_count=len(records),payload_count=len(payloads),classifications=dict(classes),records=records,payload_files=buckets,exactness='Same-runtime complete typed Core result and options hashes; raw IEEE Double bits, all match indices/order/multiplicity, scores/weights/effects/balance/warnings. No numerical golden or scientific independence.',seed=12345,native='NOT_RUN',R='NOT_RUN'))
def receipts():
    rows=[json.loads(p.read_bytes()) for p in sorted((RUN/'commands').glob('*/command.json'))]
    c.emit('command-outcomes.json',dict(commands=rows,all_attempts_retained=True))
    lanes={phase:{config:json.loads((RUN/'lanes'/(phase+'-'+config+'-summary.json')).read_bytes()) for config in ['Debug','Release']} for phase in ['entry','final']}
    c.emit('parallel-validation-audit.json',dict(phases=lanes,worker_limit=4,final_matrix_seconds=sum(r['seconds'] for r in lanes['final'].values()),root_pid_records=[dict(name=r['name'],pid=r['pid'],seconds=r['seconds'],metrics=r['metrics']) for r in rows],build_seconds=sum(r['seconds'] for r in rows if 'build' in r['name']),end_to_end='Wall-clock timestamps of entry/terminal audit tracked separately; matrix wall excludes builds/serial gates/development.',speedup='No independently measured sequential baseline; none claimed.'))
    cache=[]
    for (path,size,mtime,ctime),h in HASH_CACHE.items():
        p=Path(path);st=p.stat();assert (st.st_size,st.st_mtime_ns,st.st_ctime_ns)==(size,mtime,ctime)
        assert c.sha(p)==h
        cache.append(dict(alias='LOCAL_TRX:'+p.parents[1].name,sha256=h,bytes=size,mtime_ns=mtime,ctime_ns=ctime))
    c.emit('receipt-hash-cache.json',dict(files=cache,reused_hash_lookups=HASH_REUSES,executed_method_results_reused=0,provenance='Only immutable completed local TRX receipt hashing/inventory accounting. Metadata keyed and terminal SHA-256 reread. No historical scanner, test membership, method execution or pass omitted.'))
def projections():
    corpus=json.loads((OUT/'psm-matching-weighting-balance-same-core-v1.json').read_bytes())
    axes=[];methods=Counter();reports=Counter();love=[]
    for bucket in corpus['payload_files']:
        payloads=json.loads((ROOT/bucket['path']).read_bytes())['payloads']
        for key,payload in payloads.items():
            children=payload['children'];values={n['tag']:n['text'] for n in children}
            attrs=payload['attributes'];methods[(attrs['method'],attrs['estimand'],attrs['classification'])]+=1
            axes.append(dict(payload_sha256=key,fixture_sha256=values.get('fixture_sha256'),source_row_ids=values.get('row_ids'),id_label_fingerprint=values.get('id_labels_sha256'),exact_category_axis_fingerprint=values.get('exact_axis_sha256'),runtime=attrs['runtime'],culture=attrs['culture']))
            for node in children:
                if node['tag']=='report':reports[node['attributes']['sheet']]+=1
    c.emit('psm-aligned-row-and-role-axes-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',profiles=axes,role_axes=dict(raw_covariates=['x1','site'],resolved_effects=['x1','site(B)','x1:site(B)'],expanded_covariates=['x','site(B):x']),native_raw_matrix_tests='new-test-inventory.json: shifted first rows 1/20/200, alignment drops, explicit duplicate IDs, no-ID fallback, numeric factor/interaction expansion and multi-column exact labels. No worksheet/COM execution.',ownership='Host design expansion and imported RowIds unchanged; Core ordinals and match sets remain separate. Common never re-expands formulas or trims observations.'))
    method_names=['StandardNearestNeighbor','StandardSubclassification','WeightingOnly','OptimalPairMatching','CoarsenedExactMatching']
    allowed=[['ATT','ATC'],['ATT','ATC','ATE','ATO'],['ATT','ATC','ATE','ATO'],['ATT','ATC'],['ATT','ATC','ATE','ATO']]
    c.emit('psm-option-capability-and-method-matrix-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',authority=dict(path='BESHStat.Core/src/StatTests/PropensityScoreMatching/PSMmethodCapabilities.vb',sha256=c.sha(ROOT/'BESHStat.Core/src/StatTests/PropensityScoreMatching/PSMmethodCapabilities.vb')),methods=[dict(method=name,supported_estimands=allowed[i],matching_controls=i in [0,3],nearest_neighbor=i==0,subclass=i==1,cem=i==4,observed_unique_payload_classifications=[dict(estimand=e,classification=cl,payloads=n) for (m,e,cl),n in sorted(methods.items()) if int(m)==i]) for i,name in enumerate(method_names)],gui_defaults=dict(logistic_tolerance=1e-7,logistic_ridge=1e-7,max_iterations=100,ratio=1,strata=5,cem_bins=5,overlap_bins=20,trim=[0,1],smd_threshold=.1,extreme_weight_cutoff=10,caliper='NaN when empty',random_seed=12345),core_defaults=dict(logistic_ridge=1e-6,cem_covariate_bins=4),no_fallback=True,selected_vs_effective='PsmRequestSelection.SelectedOptions versus CreateEffectiveOptions; native disabled text/event mutation and numeric parsing remain Windows-owned.',udf='Signatures/metadata and UDF parser unchanged; source-observed default disparities are candidate issues, not silently aligned.'))
    for config in ['Debug','Release']:
        for suite in ['Common','Legacy']:
            path=RUN/'executions'/('final-'+config+'-'+suite)/'results'/(suite+'.trx')
            for result in ET.parse(path).getroot().findall('.//'+NS+'UnitTestResult'):
                for line in result.findtext('.//'+NS+'StdOut','').splitlines():
                    if line.startswith('04A25_LOVE_XML|'):
                        node=ET.fromstring(base64.b64decode(line.split('|',1)[1]))
                        love.append(dict(configuration=config,runtime=suite,identity=result.attrib['testName'],outcome=result.attrib['outcome'],projection=xml(node),raw_trx_sha256=receipt_sha(path)))
    c.emit('psm-loveplot-projection-parity-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',record_count=len(love),records=love,reference='legacy-projection-provenance.json; original Windows pure table and series prelude derivative.',native_chart='NOT_RUN / UNAUTHORIZED; preserved COM geometry/style tail source-checked, no visual acceptance.',legacy_quirks='Nonfinite threshold cells box Double zero; all-nonfinite series maximum uses legacy NaN fallback 0.25 even with threshold 10. SMD cells remain Nothing; candidate issues unapplied.'))
    c.emit('psm-report-selection-parity-v1.json',dict(version=1,status='NOT_GOLDEN / SAME_CORE_COMPATIBILITY_ONLY',checkbox_combinations=32,ordered_sheets=['Input Data','Results','Diagnostics','Row Audit','Love Plot','Matched Pairs','Matched Data','CEM'],unique_profile_report_sections=dict(reports),core_table_hashes='Every positive payload in psm-matching-weighting-balance-same-core-v1.json includes exact typed ToOutputModel table hashes for selected sections, compared against original Core API calls.',empty_behavior='Native skip-empty lists unchanged; Core can return message tables for no matches. Love sheet availability follows raw LovePlotRows.Count, preserving a sheet with empty source message if all labels filter out.',report_snapshot='Original second BuildFitOptions call retained; diagnostic, Love, matches and CEM phases preserve native checkbox read chronology. Workbook/summary naming/formatting/activation remain Windows-owned.'))
    c.emit('candidate-issues.json',dict(status='OPEN / CANDIDATE_ONLY / UNAPPLIED',activated_fixes=0,issues=[dict(id='C25-REENTRANT-REQUEST-SNAPSHOT-01',reproducer='RunPSM calls BuildFitOptions, Fit, DoEvents; writer calls BuildFitOptions again. A changed selection could make the displayed options differ from fitted options.',retained='Both captures and checkbox phase chronology retained; no re-entrant UI correction.',owner='Windows workflow owner / 04A.47',gate='Separate native/behavior authorization and verified synchronous/re-entrant test.'),dict(id='C25-LOVE-THRESHOLD-MISSING-01',reproducer='Psm25Tests.LegacyNonfiniteThresholdCellAndAllMissingAxisFallbackAreObservedUnchanged: NaN threshold boxes zero through original VB Double/Nothing If.',retained='Zero threshold cell retained, missing SMD cells remain Nothing.',owner='Windows chart/report owner / 04A.34-36 and 04A.47',gate='Separate output contract approval and native capture.'),dict(id='C25-LOVE-AXIS-NOFINITE-01',reproducer='Same named test: all three SMD channels nonfinite, positive threshold 10 -> x maximum 0.25 via Math.Max with NaN.',retained='Legacy fallback retained.',owner='Windows chart owner / 04A.34-36',gate='Separate geometry/output approval and native appearance.'),dict(id='C25-RAW-TEXT-COVARIATE-IMPORT-01',reproducer='Psm25AdapterTests.RawTextModelCovariatesRetainExistingNumericImportRejection: two raw model rows {1,A}/{0,B} with numeric outcome -> bZeroValid True, Input/AlignedAxis Nothing. Numeric-coded factor and interaction tests pass.',retained='Original CharCols=-1 import branch and early-return boundary unchanged. No native worksheet claim.',owner='Host data import and PSM owners / later separately authorized review',gate='Separate host transport/behavior approval; preserve 04A.27 restrictions.'),dict(id='C25-GUI-UDF-DEFAULT-INTENT-01',reproducer='Source-observed defaults: GUI ridge 1e-7 and CEM bins 5 versus UDF/Core ridge 1e-6 and CEM bins 4; GUI forces AsInput for optimal/nonmatching, UDF normalization retains selected/default order.',retained='UDF unchanged; no inferred statistical equivalence between differing defaults.',owner='Scientific/UDF owner / 04A.42; Windows owner / 04A.47',gate='Separate scientific/default-policy and native authorization.')],predecessors='C09/C11/C12, C13, C17-C24 including C21-STACKED-ROWCOUNT-VALIDATION-01 and C24-HISTORICAL-ORACLE-SCOPE-01 unchanged.',releases=0,discharges=0))
if __name__=='__main__':globals()[sys.argv[1]]()
