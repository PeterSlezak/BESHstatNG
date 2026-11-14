"""Six independent processes, exact discovery multiset, guarded private writers.

Assertions and test modules are unchanged. Historical scanner optimizations are
the exact authenticated C6 AST prefix, including positive-regex fallback and
content-keyed cache. Expensive scanner modules occupy one lane only.
"""
from common import *
import ast, collections, importlib.util, inspect, unittest, traceback, datetime
GROUPS={'tools':'BESHstatNG/tools/tests','help':'BESHStatNG_Help_MkDocs/tests','scripts':'scripts/tests','common':'BESHStat.Excel.Common.Tests/src'}
EXPECTED={'tools':27,'help':16,'scripts':745,'common':168}
UNSAFE=['test_clone_shaped_payload_reads_without_any_ignored_originals', 'test_corrupted_published_copy_fails_closed', 'test_development_reader_uses_local_original_even_when_publication_exists', 'test_duplicate_mapping_or_excluded_injection_is_rejected']
COSTLY={'test_04a11_publication','test_04a13_survival_workflow','test_04a14_mixed_workflow'}

def cases(suite):
    for value in suite:
        if isinstance(value,unittest.TestSuite):yield from cases(value)
        else:yield value

def discover(group):
    suite=unittest.defaultTestLoader.discover(str(ROOT/GROUPS[group]),pattern='test_*.py')
    assert not unittest.defaultTestLoader.errors,unittest.defaultTestLoader.errors
    return list(cases(suite))

def lane(group,module):
    if group=='tools':return 2
    if group=='help':return 3
    if group=='common':return 4
    if module in COSTLY:return 1
    return 5+int(sha(module.encode())[:8],16)%2

def inventory_worker(group):
    entries=[]
    for test in discover(group):
        module=sys.modules[type(test).__module__];path=Path(module.__file__).resolve()
        entries.append(dict(id=test.id(),group=group,module=module.__name__,module_path=path.relative_to(ROOT).as_posix(),module_sha256=sha(path.read_bytes()),lane=lane(group,module.__name__),phase='DISPOSABLE' if module.__name__=='test_04a08_evidence_relocation' and test._testMethodName in UNSAFE else 'MAIN'))
    assert len(entries)==EXPECTED[group],(group,len(entries))
    save('discovery/'+group+'-v1.json',dict(entries=entries,discovery_only=True,executed_tests=0))

def make_inventory():
    rows=[];receipts=[]
    for group in GROUPS:
        result=command('python-discovery-'+group,[sys.executable,'-B',str(HERE/'python_workers_v1.py'),'discover',group])
        receipts.append(clean(result));assert result['exit_code']==0
        rows.extend(json.loads((HERE/'discovery'/ (group+'-v1.json')).read_bytes())['entries'])
    keys=[(r['group'],r['id']) for r in rows]
    assert len(keys)==len(set(keys))==956
    modules={r['module_path']:r['module_sha256'] for r in rows}
    source_audit=[]
    # Record every filesystem mutation and process launch site in test sources.
    # Execution additionally refuses writes outside each worker's disposable root.
    for path,digest in sorted(modules.items()):
        text=(ROOT/path).read_text(encoding='utf-8-sig');tree=ast.parse(text);mutators=[]
        for node in ast.walk(tree):
            if not isinstance(node,ast.Call):continue
            name=ast.unparse(node.func)
            if name.split('.')[-1] in {'write_text','write_bytes','unlink','rename','replace','rmtree','mkdir','makedirs','open','run','Popen','chdir'}:
                mutators.append(dict(line=node.lineno,call=ast.unparse(node)))
        source_audit.append(dict(path=path,sha256=digest,sites=mutators))
    save('python-discovery-inventory-v1.json',dict(status='EXACT_956_DISCOVERED_CASES_PARTITIONED_ONCE',entries=rows,modules=modules,lane_counts=dict(collections.Counter(str(r['lane']) for r in rows)),discovery_commands=receipts,executed_tests=0))
    save('python-writer-safety-v1.json',dict(sources=source_audit,private_writers='Each process gets a distinct cwd/TEMP/TMP/cache/result tree; Python audit hook refuses any write/rename/delete outside that tree before the operation. Module-state mutations remain process-local. Child launches are recorded; mkdocs/generator outputs stay in temporary test fixtures; Git commands are read-only or --check.',shared_scanner='Only lane1 loads the costly11/13/14 scan tests. Content-keyed cache remains process-local; no repeated whole-tree scans in other lanes.',tracked_mutation_authorized=False,scanner_roots='Existing ignored components pruned before descent; TestResults never walked',workers=6))
    previous=json.loads((PREVIOUS/'python-optimization-authentication-v1.json').read_bytes())
    prefixes={}
    for label in ['scanner','tools']:
        name=label+'-hook-ast-v1.py';expected=previous['prefixes'][label]['extracted']
        assert pin(PREVIOUS/name)==expected
        data=(PREVIOUS/name).read_bytes();node=ast.parse(data.decode('utf-8-sig'))
        digest=sha(ast.dump(node,include_attributes=False).encode())
        assert digest==previous['prefixes'][label]['ast_sha256']
        with (HERE/name).open('xb') as f:f.write(data)
        prefixes[label]=dict(before=expected,extracted=pin(HERE/name),ast_sha256=digest)
    for name in ['privacy_regex_guard.py','privacy-regex-guard-proof-v1.json','scanner-cache-proof-v1.json','scanner-execution-proof-v1.json']:
        row=previous['prefixes'][name];assert pin(ROOT/row['path'])==row;prefixes[name]=row
    save('python-optimization-authentication-v1.json',dict(prefixes=prefixes,retained_regex_equivalence_controls=12068,source_changed=False,assertions_changed=0,parallel_kernel=pin(HERE/'parallel_scan_v1.py'),parallel_proof=pin(HERE/'parallel-scanner-equivalence-v1.json'),positive_matches='Original regex Match objects and ordered spans; exact unchanged C6 fallback',cache_invalidation='Exact scan/git AST, ROOT, COMPUTERNAME, fixed hash seed38, path and actual SHA256; deepcopy C6 cache outputs; absent or patched domains retain original sequential fallback'))


def worker(number,phase='MAIN'):
    plan=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());expected=[r for r in plan['entries'] if r['lane']==number and r['phase']==phase]
    work=Path.cwd().resolve();assert work.is_relative_to(PRIVATE/'python-v1')
    for row in expected:assert sha((ROOT/row['module_path']).read_bytes())==row['module_sha256']
    mutations=[];launches=[]
    def path_of(value):
        if isinstance(value,(str,bytes,os.PathLike)):
            try:return Path(os.fsdecode(value)).absolute().resolve()
            except (ValueError,OSError):return None
        return None
    def private(value,event):
        path=path_of(value)
        if path is None:return
        mutations.append(dict(event=event,path=str(path)))
        if not path.is_relative_to(work):raise PermissionError('C10 worker refused write outside disposable lane: '+str(path))
    def audit(event,args):
        if event=='open':
            mode=args[1] or '';flags=args[2] or 0
            if any(c in mode for c in 'wax+') or flags & (os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):private(args[0],event)
        elif event in ['os.remove','os.rmdir','os.mkdir','os.chmod','os.truncate']:private(args[0],event)
        elif event in ['os.rename','os.replace']:private(args[0],event);private(args[1],event)
        elif event=='subprocess.Popen':launches.append(dict(executable=str(args[0]),args=list(map(str,args[1])) if not isinstance(args[1],str) else args[1],cwd=str(args[2])))
    sys.addaudithook(audit)
    sys.path.append(str(PREVIOUS));sys.path.append(str(W2/'continuation-v6'))
    hook={**globals()}
    if number==1:exec(compile((HERE/'scanner-hook-ast-v1.py').read_text(encoding='utf-8'),'C7 unchanged C6 scanner hook','exec'),hook)
    if number==2:exec(compile((HERE/'tools-hook-ast-v1.py').read_text(encoding='utf-8'),'C7 unchanged C6 tools hook','exec'),hook)
    accelerator=None
    if number==1:
        from parallel_scan_v1 import Accelerator
        accelerator=Accelerator(hook,work);accelerator.install()
    selected=[]
    if phase=='DISPOSABLE':
        copied=work/'copy';sys.path.insert(0,str(copied/'scripts/tests'))
        import test_04a08_evidence_relocation as relocation
        selected=[relocation.EvidenceRelocation08Tests(r['id'].rsplit('.',1)[1]) for r in expected]
        assert Path(relocation.__file__).resolve().is_relative_to(copied.resolve())
    else:
        for group in sorted({r['group'] for r in expected}):
            selected.extend(test for test in discover(group) if any(test.id()==r['id'] and r['group']==group for r in expected))
    assert collections.Counter(t.id() for t in selected)==collections.Counter(r['id'] for r in expected),'Missing/duplicate lane test'
    records=[];active={};occurrences=collections.Counter();start=time.monotonic();start_utc=datetime.datetime.now(datetime.timezone.utc).isoformat()
    class Result(unittest.TextTestResult):
        def startTest(self,test):
            super().startTest(test);identity=test.id();occurrence=occurrences[identity];occurrences[identity]+=1
            row=dict(id=identity,occurrence=occurrence,outcome='PASS',events=[],start_seconds=time.monotonic()-start)
            records.append(row);active[id(test)]=row
        def event(self,test,outcome,error=None,subtest=None):
            row=active[id(test)];trace=self._exc_info_to_string(error,test) if error else ''
            event=dict(outcome=outcome,subtest=subtest,traceback=trace,traceback_sha256=sha(trace.encode()),message=str(error[1]) if error else '')
            if trace:
                lines=trace.splitlines();event['first_traceback']=next((s.strip() for s in lines if s.lstrip().startswith('File ')),lines[0] if lines else '')
            row['events'].append(event)
            if outcome=='ERROR' or row['outcome']=='PASS':row['outcome']=outcome
        def addFailure(self,test,error):self.event(test,'FAIL',error);super().addFailure(test,error)
        def addError(self,test,error):self.event(test,'ERROR',error);super().addError(test,error)
        def addSkip(self,test,reason):self.event(test,'SKIP');super().addSkip(test,reason)
        def addExpectedFailure(self,test,error):self.event(test,'EXPECTED_FAILURE',error);super().addExpectedFailure(test,error)
        def addUnexpectedSuccess(self,test):self.event(test,'UNEXPECTED_SUCCESS');super().addUnexpectedSuccess(test)
        def addSubTest(self,test,subtest,error):
            if error:self.event(test,'FAIL' if issubclass(error[0],test.failureException) else 'ERROR',error,subtest.id())
            super().addSubTest(test,subtest,error)
        def stopTest(self,test):active[id(test)]['end_seconds']=time.monotonic()-start;super().stopTest(test)
    result=unittest.TextTestRunner(verbosity=2,resultclass=Result).run(unittest.TestSuite(selected))
    assert len(records)==result.testsRun==len(expected)
    assert not result.skipped and not result.expectedFailures and not result.unexpectedSuccesses,'Skipped/stub tests are not executed acceptance'
    for row in expected:assert sha((ROOT/row['module_path']).read_bytes())==row['module_sha256']
    report=dict(lane=number,phase=phase,pid=os.getpid(),start_utc=start_utc,end_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),records=records,testsRun=result.testsRun,outcomes=dict(collections.Counter(r['outcome'] for r in records)),unittest_failures=len(result.failures),unittest_errors=len(result.errors),seconds=time.monotonic()-start,exit_code=0 if result.wasSuccessful() else 1,mutations=mutations,launches=launches,discovery_inventory=pin(HERE/'python-discovery-inventory-v1.json'))
    if number==1:report['scanner']=dict(stats=hook['stats'],authority=hook['authority'],proxies=[dict(source=n,skips=p.skips,fallbacks=p.fallbacks) for n,p in hook['proxies']],sources=hook['source_pins'])
    if accelerator is not None:report['parallel_scanner']=dict(fresh=accelerator.fresh,attempts=accelerator.attempts,kernel=pin(HERE/'parallel_scan_v1.py'),proof=pin(HERE/'parallel-scanner-equivalence-v1.json'),max_helpers=5,deferred_to_original_sequential=accelerator.deferred)
    if number==2:report['tools']=dict(counters=hook['counters'],audit_source_unchanged=hook['before']==sha(Path(hook['audit'].__file__).read_bytes()))
    with (work/'raw-result.json').open('x',encoding='utf-8',newline='\n') as stream:json.dump(report,stream,ensure_ascii=False,indent=2);stream.write('\n')
    raise SystemExit(report['exit_code'])

def execute():
    from concurrent.futures import ThreadPoolExecutor
    plan=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());folder=PRIVATE/'python-v1';folder.mkdir(exist_ok=False)
    source_before=m.source_pins();source_paths=json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'];source_hashes=[pin(ROOT/p) for p in source_paths]
    semantic=RUNROOT/'executions/w2c10-checkpoint-v1-Debug-serial-Common-semantic/observations';assert semantic.is_dir()
    prior_copy=next(r['copy_provenance'] for r in json.loads((PREVIOUS/'python-worker-accounting-v1.json').read_bytes())['executions'] if r['phase']=='DISPOSABLE');assert pin(ROOT/prior_copy['path'])==prior_copy
    provenance=json.loads((ROOT/prior_copy['path']).read_bytes())
    def run_lane(number,phase='MAIN'):
        work=folder/(str(number) if phase=='MAIN' else str(number)+'-disposable');work.mkdir();(work/'temp').mkdir();(work/'cache').mkdir()
        if phase=='DISPOSABLE':
            copied=work/'copy';copied.mkdir()
            for row in provenance['source_files']:
                assert pin(ROOT/row['path'])==row
                target=copied/row['path'];target.parent.mkdir(parents=True,exist_ok=True)
                with target.open('xb') as f:f.write((ROOT/row['path']).read_bytes())
                assert sha(target.read_bytes())==row['sha256']
            with (work/'copy-provenance.json').open('x',encoding='utf-8') as f:json.dump(provenance,f,indent=2)
        result=command('python-worker-v1-'+str(number)+'-'+phase.lower(),[sys.executable,'-B',str(HERE/'python_workers_v1.py'),'worker',str(number),phase],cwd=work,env=dict(TEMP=str(work/'temp'),TMP=str(work/'temp'),PYTHONPYCACHEPREFIX=str(work/'cache'),BESHSTAT_04A02_G3_CANDIDATES=str(semantic),PYTHONHASHSEED='38'))
        return dict(lane=number,phase=phase,command=clean(result),raw=pin(work/'raw-result.json') if (work/'raw-result.json').is_file() else None,copy_provenance=pin(work/'copy-provenance.json') if phase=='DISPOSABLE' else None)
    begin=time.monotonic()
    with ThreadPoolExecutor(max_workers=5) as pool:receipts=list(pool.map(run_lane,range(2,7)))
    non_scanner_end=time.monotonic()-begin
    receipts.append(run_lane(5,'DISPOSABLE'));disposable_end=time.monotonic()-begin
    # Other lanes have ended before scanner parent can create five helpers.
    scanner_start=time.monotonic()-begin;receipts.append(run_lane(1));scanner_end=time.monotonic()-begin
    save('python-worker-receipts-v1.json',dict(workers=6,receipts=receipts,source_unchanged=source_before==m.source_pins(),schedule=dict(non_scanner_end_seconds=non_scanner_end,disposable_end_seconds=disposable_end,scanner_start_seconds=scanner_start,scanner_end_seconds=scanner_end,other_lanes_completed_before_scanner=True,max_non_scanner_workers=5,max_scanner_helpers=5,max_scanner_parent_plus_helpers=6),raw_transcripts='Independent raw command/stdout/stderr/results/cache/temp per execution; lane5 has two disjoint planned parts, no corrective repeat. Pure scan helpers retain separate exact input/output/command/stream hashes.'))
    actual=[];reports=[]
    for receipt in receipts:
        assert receipt['raw'] is not None,'Dropped Python worker report'
        report=json.loads((ROOT/receipt['raw']['path']).read_bytes());reports.append(report)
        assert report['exit_code']==receipt['command']['exit_code']
        expected=[r for r in plan['entries'] if r['lane']==receipt['lane'] and r['phase']==receipt['phase']]
        assert collections.Counter(r['id'] for r in expected)==collections.Counter(r['id'] for r in report['records'])
        assert report['testsRun']==sum(report['outcomes'].values())==len(expected)
        actual.extend((receipt['lane'],r['id']) for r in report['records'])
    assert collections.Counter(actual)==collections.Counter((r['lane'],r['id']) for r in plan['entries'])
    assert source_before==m.source_pins() and source_hashes==[pin(ROOT/p) for p in source_paths]
    totals=collections.Counter(r['outcome'] for report in reports for r in report['records']);assert sum(totals.values())==len(plan['entries'])
    save('python-execution-summary-v1.json',dict(status='EXACT_EXECUTED_DISCOVERED_MULTISET_RECONCILED',workers_at_peak=6,outcomes=dict(totals),executed=len(plan['entries']),lane_counts=[dict(lane=r['lane'],phase=r['phase'],testsRun=r['testsRun'],outcomes=r['outcomes'],seconds=r['seconds'],exit_code=r['exit_code']) for r in reports],missing=[],duplicated=[],skipped=[],source_unchanged=True,raw_attempts=len(actual),corrective_repeats=0))
    print(json.dumps(dict(workers_at_peak=6,total=len(plan['entries']),outcomes=dict(totals))),flush=True)


if __name__=='__main__':
    mode=sys.argv[1]
    if mode=='discover':inventory_worker(sys.argv[2])
    elif mode=='inventory':make_inventory()
    elif mode=='worker':worker(int(sys.argv[2]),sys.argv[3] if len(sys.argv)>3 else 'MAIN')
    elif mode=='execute':execute()
    else:raise ValueError(mode)
