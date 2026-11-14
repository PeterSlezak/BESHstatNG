"""Six independent processes, exact discovery multiset, guarded private writers.

Assertions and test modules are unchanged. Historical scanner optimizations are
the exact authenticated C6 AST prefix, including positive-regex fallback and
content-keyed cache. Expensive scanner modules occupy one lane only.
"""
from common import *
import ast, collections, importlib.util, inspect, unittest, traceback
GROUPS={'tools':'BESHstatNG/tools/tests','help':'BESHStatNG_Help_MkDocs/tests','scripts':'scripts/tests','common':'BESHStat.Excel.Common.Tests/src'}
EXPECTED={'tools':27,'help':16,'scripts':745,'common':168}
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
        entries.append(dict(id=test.id(),group=group,module=module.__name__,module_path=path.relative_to(ROOT).as_posix(),module_sha256=sha(path.read_bytes()),lane=lane(group,module.__name__)))
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
    prefixes={}
    for label,name in [('scanner','run_scripts_cached_v1.py'),('tools','run_tools_pruned.py')]:
        raw=(PREVIOUS/name).read_bytes();tree=ast.parse(raw.decode('utf-8-sig'))
        branch=next(n for n in tree.body if isinstance(n,ast.If)).orelse
        cut=next(i for i,n in enumerate(branch) if isinstance(n,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='suite' for t in n.targets))
        nodes=branch[:cut];source=ast.unparse(ast.Module(body=nodes,type_ignores=[]))+'\n'
        assert ast.dump(ast.parse(source),include_attributes=False)==ast.dump(ast.Module(body=nodes,type_ignores=[]),include_attributes=False)
        target=HERE/(label+'-hook-ast-v1.py')
        with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
        prefixes[label]=dict(original=pin(PREVIOUS/name),extracted=pin(target),ast_sha256=sha(ast.dump(ast.Module(body=nodes,type_ignores=[]),include_attributes=False).encode()),law='Exact unchanged AST prefix of the C6 worker; execution loop replaced by explicit inventory partition only')
    for name in ['privacy_regex_guard.py','privacy-regex-guard-proof-v1.json','scanner-cache-proof-v1.json','scanner-execution-proof-v1.json']:
        prefixes[name]=pin(PREVIOUS/name)
    save('python-optimization-authentication-v1.json',dict(prefixes=prefixes,retained_regex_equivalence_controls=12068,source_changed=False,assertions_changed=0,positive_matches='Original regex Match objects and ordered spans on every necessary-suffix positive; exact C6 fallback',cache_invalidation='Exact scan/git AST, ROOT, COMPUTERNAME, path and actual SHA256; deepcopy cached outputs; absent files bypass cache'))

def worker(number):
    plan=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());expected=[r for r in plan['entries'] if r['lane']==number]
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
        if not path.is_relative_to(work):raise PermissionError('C7 worker refused write outside disposable lane: '+str(path))
    def audit(event,args):
        if event=='open':
            mode=args[1] or '';flags=args[2] or 0
            if any(c in mode for c in 'wax+') or flags & (os.O_WRONLY|os.O_RDWR|os.O_CREAT|os.O_TRUNC|os.O_APPEND):private(args[0],event)
        elif event in ['os.remove','os.rmdir','os.mkdir','os.chmod','os.truncate']:private(args[0],event)
        elif event in ['os.rename','os.replace']:private(args[0],event);private(args[1],event)
        elif event=='subprocess.Popen':launches.append(dict(executable=str(args[0]),args=list(map(str,args[1])) if not isinstance(args[1],str) else args[1],cwd=str(args[2])))
    sys.addaudithook(audit)
    sys.path.append(str(PREVIOUS))
    hook={**globals()}
    if number==1:exec(compile((HERE/'scanner-hook-ast-v1.py').read_text(encoding='utf-8'),'C7 unchanged C6 scanner hook','exec'),hook)
    if number==2:exec(compile((HERE/'tools-hook-ast-v1.py').read_text(encoding='utf-8'),'C7 unchanged C6 tools hook','exec'),hook)
    selected=[]
    for group in sorted({r['group'] for r in expected}):
        selected.extend(test for test in discover(group) if any(test.id()==r['id'] and r['group']==group for r in expected))
    assert collections.Counter(t.id() for t in selected)==collections.Counter(r['id'] for r in expected),'Missing/duplicate lane test'
    records=[];active={};occurrences=collections.Counter();start=time.monotonic()
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
    report=dict(lane=number,records=records,testsRun=result.testsRun,outcomes=dict(collections.Counter(r['outcome'] for r in records)),unittest_failures=len(result.failures),unittest_errors=len(result.errors),seconds=time.monotonic()-start,exit_code=0 if result.wasSuccessful() else 1,mutations=mutations,launches=launches,discovery_inventory=pin(HERE/'python-discovery-inventory-v1.json'))
    if number==1:report['scanner']=dict(stats=hook['stats'],authority=hook['authority'],proxies=[dict(source=n,skips=p.skips,fallbacks=p.fallbacks) for n,p in hook['proxies']],sources=hook['source_pins'])
    if number==2:report['tools']=dict(counters=hook['counters'],audit_source_unchanged=hook['before']==sha(Path(hook['audit'].__file__).read_bytes()))
    with (work/'raw-result.json').open('x',encoding='utf-8',newline='\n') as stream:json.dump(report,stream,ensure_ascii=False,indent=2);stream.write('\n')
    raise SystemExit(report['exit_code'])

def execute():
    from concurrent.futures import ThreadPoolExecutor
    plan=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());folder=PRIVATE/'python-v1';folder.mkdir(exist_ok=False)
    source_before=m.source_pins()
    semantic=RUNROOT/'executions/w2c7-checkpoint-v1-Debug-serial-Common/observations'
    assert semantic.is_dir(),'Full Debug Common semantic candidates must exist first'
    def run(number):
        work=folder/str(number);work.mkdir();temp=work/'temp';temp.mkdir();cache=work/'cache';cache.mkdir()
        result=command('python-worker-v1-'+str(number),[sys.executable,'-B',str(HERE/'python_workers_v1.py'),'worker',str(number)],cwd=work,env=dict(TEMP=str(temp),TMP=str(temp),PYTHONPYCACHEPREFIX=str(cache),BESHSTAT_04A02_G3_CANDIDATES=str(semantic),PYTHONHASHSEED='38'))
        return dict(lane=number,command=clean(result),raw=pin(work/'raw-result.json') if (work/'raw-result.json').is_file() else None)
    with ThreadPoolExecutor(max_workers=6) as pool:receipts=list(pool.map(run,range(1,7)))
    save('python-worker-receipts-v1.json',dict(workers=6,receipts=receipts,source_unchanged=source_before==m.source_pins(),raw_transcripts='Each command directory retains independent stdout.log/stderr.log/command.json; each lane retains raw-result.json. Nonzero exits remain nonpasses. Missing reports are fatal.'))
    assert all(r['raw'] is not None for r in receipts),'Dropped Python worker report'
    actual=[];reports=[]
    for receipt in receipts:
        report=json.loads((ROOT/receipt['raw']['path']).read_bytes());reports.append(report)
        assert report['exit_code']==receipt['command']['exit_code']
        expected=[r for r in plan['entries'] if r['lane']==receipt['lane']]
        assert collections.Counter(r['id'] for r in expected)==collections.Counter(r['id'] for r in report['records'])
        assert report['testsRun']==sum(report['outcomes'].values())==len(expected)
        actual.extend((receipt['lane'],r['id']) for r in report['records'])
    assert collections.Counter(actual)==collections.Counter((r['lane'],r['id']) for r in plan['entries'])
    assert source_before==m.source_pins(),'Authoritative source mutated by a worker'
    totals=collections.Counter(r['outcome'] for report in reports for r in report['records'])
    assert sum(totals.values())==956
    save('python-execution-summary-v1.json',dict(status='EXACT_EXECUTED_DISCOVERED_MULTISET_RECONCILED',workers=6,outcomes=dict(totals),executed=956,lane_counts=[dict(lane=r['lane'],testsRun=r['testsRun'],outcomes=r['outcomes'],seconds=r['seconds'],exit_code=r['exit_code']) for r in reports],missing=[],duplicated=[],skipped=[],source_unchanged=True))
    print(json.dumps(dict(workers=6,total=956,outcomes=dict(totals))),flush=True)

if __name__=='__main__':
    mode=sys.argv[1]
    if mode=='discover':inventory_worker(sys.argv[2])
    elif mode=='inventory':make_inventory()
    elif mode=='worker':worker(int(sys.argv[2]))
    elif mode=='execute':execute()
    else:raise ValueError(mode)
