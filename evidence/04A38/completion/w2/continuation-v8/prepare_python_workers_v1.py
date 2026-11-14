"""Version exact discovery/guarded workers, pre-route known writers and bound scan concurrency."""
from common import *
import ast
UNSAFE=['test_clone_shaped_payload_reads_without_any_ignored_originals','test_corrupted_published_copy_fails_closed','test_development_reader_uses_local_original_even_when_publication_exists','test_duplicate_mapping_or_excluded_injection_is_rejected']
source=(PREVIOUS/'python_workers_v1.py').read_text(encoding='utf-8')
source=source.replace("EXPECTED={'tools':27,'help':16,'scripts':745,'common':168}","EXPECTED={'tools':27,'help':16,'scripts':745,'common':168}\nUNSAFE="+repr(UNSAFE))
source=source.replace("lane=lane(group,module.__name__))", "lane=lane(group,module.__name__),phase='DISPOSABLE' if module.__name__=='test_04a08_evidence_relocation' and test._testMethodName in UNSAFE else 'MAIN')")
begin=source.index('    prefixes={}');end=source.index('\n\ndef worker(',begin)
source=source[:begin]+'''    previous=json.loads((PREVIOUS/'python-optimization-authentication-v1.json').read_bytes())
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
    save('python-optimization-authentication-v1.json',dict(prefixes=prefixes,retained_regex_equivalence_controls=12068,source_changed=False,assertions_changed=0,parallel_kernel=pin(HERE/'parallel_scan_v2.py'),parallel_proof=pin(HERE/'parallel-scanner-equivalence-v2.json'),positive_matches='Original regex Match objects and ordered spans; exact unchanged C6 fallback',cache_invalidation='Exact scan/git AST, ROOT, COMPUTERNAME, fixed hash seed38, path and actual SHA256; deepcopy C6 cache outputs; absent or patched domains retain original sequential fallback'))
''' + source[end:]
source=source.replace('def worker(number):','def worker(number,phase=\'MAIN\'):')
source=source.replace("expected=[r for r in plan['entries'] if r['lane']==number]", "expected=[r for r in plan['entries'] if r['lane']==number and r['phase']==phase]")
source=source.replace('C7 worker refused','C8 worker refused')
source=source.replace('    sys.path.append(str(PREVIOUS))','    sys.path.append(str(PREVIOUS));sys.path.append(str(W2/\'continuation-v6\'))')
source=source.replace("    selected=[]\n    for group in sorted({r['group'] for r in expected}):\n        selected.extend(test for test in discover(group) if any(test.id()==r['id'] and r['group']==group for r in expected))", """    accelerator=None
    if number==1:
        from parallel_scan_v2 import Accelerator
        accelerator=Accelerator(hook,work);accelerator.install()
    selected=[]
    if phase=='DISPOSABLE':
        copied=work/'copy';sys.path.insert(0,str(copied/'scripts/tests'))
        import test_04a08_evidence_relocation as relocation
        selected=[relocation.EvidenceRelocation08Tests(r['id'].rsplit('.',1)[1]) for r in expected]
        assert Path(relocation.__file__).resolve().is_relative_to(copied.resolve())
    else:
        for group in sorted({r['group'] for r in expected}):
            selected.extend(test for test in discover(group) if any(test.id()==r['id'] and r['group']==group for r in expected))""")
source=source.replace('report=dict(lane=number,records=', 'report=dict(lane=number,phase=phase,records=')
source=source.replace("    if number==2:report['tools']", "    if accelerator is not None:report['parallel_scanner']=dict(fresh=accelerator.fresh,attempts=accelerator.attempts,kernel=pin(HERE/'parallel_scan_v2.py'),proof=pin(HERE/'parallel-scanner-equivalence-v2.json'),max_helpers=5)\n    if number==2:report['tools']")
begin=source.index('def execute():');end=source.index("\n\nif __name__",begin)
source=source[:begin]+'''def execute():
    from concurrent.futures import ThreadPoolExecutor
    plan=json.loads((HERE/'python-discovery-inventory-v1.json').read_bytes());folder=PRIVATE/'python-v1';folder.mkdir(exist_ok=False)
    source_before=m.source_pins();source_paths=json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'];source_hashes=[pin(ROOT/p) for p in source_paths]
    semantic=RUNROOT/'executions/w2c8-checkpoint-v1-Debug-serial-Common-semantic/observations';assert semantic.is_dir()
    prior_copy=json.loads((PREVIOUS/'python-environment-corrections-v1.json').read_bytes())['copy_provenance'];assert pin(ROOT/prior_copy['path'])==prior_copy
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
''' +source[end:]
source=source.replace("worker(int(sys.argv[2]))", "worker(int(sys.argv[2]),sys.argv[3] if len(sys.argv)>3 else 'MAIN')")
with (HERE/'python_workers_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('python-executor-version-crosswalk-v1.json',dict(before=pin(PREVIOUS/'python_workers_v1.py'),after=pin(HERE/'python_workers_v1.py'),known_disposable_cases=UNSAFE,assertions_changed=0,discovery='Exact unchanged modules,956 actual cases; each assigned one logical lane and one phase. Unsafe4 run once in proven disposable source/fixture copy; C7 first blocked errors remain preserved.',schedule='Five non-scanner lanes then four-case serial copy, then scanner parent plus at most five pure read-only helpers. Peak test/scanner workers <=6; no overlapping full managed campaign.',original_queries=0))
