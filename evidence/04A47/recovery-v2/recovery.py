"""Append-only SPC23 availability recovery. No native or historical-file writes."""
import collections, datetime, hashlib, importlib.util, json, os, stat, subprocess, sys, time, unittest
from pathlib import Path
ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parent
OLD = ROOT/'evidence/04A47'
PRIVATE = ROOT/'.test-rationalization-04A47-recovery-v2.tmp'
sys.path.insert(0, str(OLD))
import common47 as original
os.environ.update(PYTHONDONTWRITEBYTECODE='1', GIT_OPTIONAL_LOCKS='0')
sys.dont_write_bytecode = True
TARGET = 'evidence/04A23/multivariate-spc-same-core-comparisons-v1.json'
EXPECTED = dict(bytes=57758738, sha256='7f20addb9177a48e95b81330aa55acccde4bb6ed8b808cdbad42e09f2b790643')
IDS = ['test_04a23_multivariate_spc.Spc23Contracts.test_comparison_payloads_have_real_six_chart_results_and_explicit_rejections', 'test_04a24_multivariate_payload.MultivariatePayload24.test_frozen_evidence_authority_hashes_and_unchanged_metadata']
V1 = ROOT/'.test-rationalization-04A47.tmp/publication-v1'
ZIP1 = V1/'BESHStatNG_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW_v1.zip'
RECEIPT1 = V1/'external-delivery-receipt-v1.json'
pin, load, sha, git, utc, exact = original.pin, original.load, original.sha, original.git, original.utc, original.exact
def save(name, value):
    p=Path(name);p=p if p.is_absolute() else HERE/p
    p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False,allow_nan=False);f.write('\n')
def command(label,args,**kw):
    return original.module('isolated_recovery_runner',ROOT/'scripts/test_suite_rationalization.py').Runner(PRIVATE).command('recovery-'+label,args,**kw)
def source_lock():
    original.lock()
    actual=original.module('source_recovery_runner',ROOT/'scripts/test_suite_rationalization.py').source_pins()
    assert actual==load(OLD/'matrix/source-freeze-v1.json')['source_pins'],'FROZEN_SOURCE_DRIFT'
    assert not git('diff','--check')
def payload():
    for p in [ROOT/'evidence',ROOT/'evidence/04A23',ROOT/TARGET]:
        assert not p.lstat().st_file_attributes & stat.FILE_ATTRIBUTE_REPARSE_POINT
    assert (ROOT/TARGET).is_file()
    assert {k:pin(TARGET)[k] for k in EXPECTED}==EXPECTED
    assert subprocess.run(['git','--no-optional-locks','-c','safe.directory='+ROOT.as_posix(),'ls-files','--error-unmatch','--',TARGET],cwd=ROOT,capture_output=True).returncode==1
    check=subprocess.run(['git','--no-optional-locks','-c','safe.directory='+ROOT.as_posix(),'check-ignore','-v','--no-index','--',TARGET],cwd=ROOT,capture_output=True)
    assert check.returncode==1 and not check.stdout
    assert pin('.gitignore')['sha256']=='95b233c640d30104d3ea71bad0fdb09ba7a7d4a8e7dcef34e8cc370a0e29346d','UNAPPROVED_ADDITIONAL_IGNORE_CHANGE'
    return pin(TARGET)
def auth():
    source_lock();before=payload()
    assert pin(ZIP1)['sha256']=='0fe14fe9558fadeb4040d2ffc5f28622c920add0bf418aaf9fb7ff7423d05bc8' and pin(ZIP1)['bytes']==6988940
    assert pin(RECEIPT1)['sha256']=='b328b8d298afed783b30cbe14ec98e021252c4a799f2171d2b8f9c92b615539d' and pin(RECEIPT1)['bytes']==141137
    receipt=load(RECEIPT1);old=set()
    for scan in receipt['full_actual_candidate_scans']:
        for p in scan['members']:exact(p);old.add(p['path'])
    names=set(git('diff','--name-only').decode().splitlines()+git('ls-files','--others','--exclude-standard').decode().splitlines())
    assert all(n in old or n in {'.gitignore',TARGET} or n.startswith('evidence/04A47/recovery-v2/') for n in names),'UNEXPECTED_OWNER_CANDIDATE'
    manifest='evidence/04A24/frozen-capture-hashes.json';mp=pin(manifest)
    assert mp['bytes']==344343 and mp['sha256']=='2d0bbea874e22b2d07c919d930e20377dcd31db14267a0df724a64ce760c241e'
    entry=load(ROOT/manifest)['files'][TARGET]
    assert all(entry[k]==EXPECTED[k] for k in EXPECTED)
    vb='BESHStat.Excel.Common.Tests/src/Spc24ExpectedRejections.vb'
    assert EXPECTED['sha256'] in (ROOT/vb).read_text(encoding='utf-8-sig')
    d=load(ROOT/TARGET);assert d['status']=='NOT_GOLDEN' and d['promoted'] is False and len(d['records'])==d['record_count']
    charts=set()
    for k,p in d['payloads'].items():
        assert k==sha(json.dumps(p,sort_keys=True,ensure_ascii=False,separators=(',',':')).encode())
        if p['outcome']=='POSITIVE_PARITY':
            charts.add(p['chart']);assert p['result']['panels'] and p['request_sha256']
        else:assert 'exception' in p
    assert len(charts)==6 and d['outcomes']['EXISTING_CORE_REJECTION']>0 and d['outcomes']['EXISTING_PREPARATION_EXCEPTION']>0
    assert payload()==before
    interpreter=dict(version=sys.version.split()[0],bytes=Path(sys.executable).stat().st_size,sha256=sha(Path(sys.executable).read_bytes()))
    assert interpreter['version']=='3.14.7' and interpreter['sha256']==load(OLD/'python/discovery-v1.json')['interpreter']['sha256']
    sources=[pin('scripts/tests/'+n) for n in ['test_04a23_multivariate_spc.py','test_04a24_multivariate_payload.py']]
    assert [p['sha256'] for p in sources]==['ef04954a158dd6c274eb40cdd20b54fa731b467ba5d91367a690993beb86bbb8','c1abd8affc76f30574a967d191ca147f70d9c7f12033f175d5acf481b8079461']
    save('payload-authentication-v1.json',dict(utc=utc(),source='OWNER_RECOVERED_ALREADY_PRESENT_AT_EXPECTED_LOCATION',payload=before,expected=EXPECTED,regular_nonreparse=True,pre_post_read_exact=True,manifest=mp,manifest_entry=entry,vb_authority=pin(vb),source_tests=sources,structure=dict(status=d['status'],promoted=d['promoted'],records=d['record_count'],positive_chart_families=sorted(charts),payload_keys_exact=True,rejections_retained=True),ignored=False,tracked=False,ignore_policy_exception=dict(owner_statement='.gitignore was modified on purpose and is approved. Please continue.',date='2026-11-16',current=pin('.gitignore'),historical_rule_present=True,full_original_candidate_scans_must_include_payload=True,public_zip_must_exclude_payload=True),historical_bytes_written=0,interpreter=interpreter,v1_archive=pin(ZIP1),v1_receipt=pin(RECEIPT1),source_freeze_exact=True,git=dict(branch='codex/04A-47',HEAD=original.MAIN,main=original.MAIN,merge_base=original.MAIN,index=pin('.git/index'),staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),untracked=git('ls-files','--others','--exclude-standard').decode().splitlines()),earlier_handover_executed=False,gate=original.GATES))
    print('AUTHENTICATED: original bytes, six families, authorities, source freeze, v1 delivery; owner-approved nonignored/untracked policy',flush=True)
def focused():
    source_lock();payload();a=load(HERE/'payload-authentication-v1.json')
    for p in a['source_tests']:exact(p)
    work=PRIVATE/'focused-v1';work.mkdir(parents=True);records=[];active={};start=time.monotonic()
    sys.path.insert(0,str(ROOT/'scripts/tests'))
    suite=unittest.defaultTestLoader.loadTestsFromNames(IDS)
    assert not unittest.defaultTestLoader.errors
    def journal(kind,value):
        with (work/'parent-journal.jsonl').open('a',encoding='utf-8') as f:f.write(json.dumps(dict(kind=kind,value=value))+'\n')
    class Result(unittest.TextTestResult):
        def startTest(self,test):
            super().startTest(test);r=dict(id=test.id(),outcome='PASS',events=[],start_seconds=time.monotonic()-start);records.append(r);active[id(test)]=r;journal('START',r)
        def event(self,test,outcome,error):
            trace=self._exc_info_to_string(error,test);r=active[id(test)];r['outcome']=outcome
            event=dict(outcome=outcome,exception_class=error[0].__name__,message=str(error[1]),message_sha256=sha(str(error[1]).encode()),traceback=trace,traceback_sha256=sha(trace.encode()));r['events'].append(event);journal('EVENT',dict(id=r['id'],event=event))
        def addFailure(self,test,error):self.event(test,'FAIL',error);super().addFailure(test,error)
        def addError(self,test,error):self.event(test,'ERROR',error);super().addError(test,error)
        def stopTest(self,test):active[id(test)]['seconds']=time.monotonic()-start-active[id(test)]['start_seconds'];journal('COMPLETE',active[id(test)]);super().stopTest(test)
    with (work/'stderr.txt').open('x',encoding='utf-8') as stream:result=unittest.TextTestRunner(stream=stream,resultclass=Result,verbosity=2).run(suite)
    save(work/'raw-result.json',dict(records=records,testsRun=result.testsRun,exit_code=0 if result.wasSuccessful() else 1,interpreter=a['interpreter'],source_tests=a['source_tests'],input=a['payload']))
    assert [r['id'] for r in records]==IDS and [r['outcome'] for r in records]==['PASS','FAIL'],'FOCUSED_OUTCOME_MISMATCH'
    old=load(ROOT/'.test-rationalization-04A47.tmp/python-event-pairs-v1.json')['events']
    previous=[r['before'] for r in old if r['key'][0]==IDS[1] and r['before']]
    assert len(previous)==1 and previous[0]['message']==records[1]['events'][0]['message'],'UNEXPECTED_04A24_FAILURE_REASON'
    rawpin=pin(work/'raw-result.json')
    save('focused-parent-comparison-v1.json',dict(classification='FRESH_FOCUSED_NOT_FULL',raw=rawpin,parents=[dict(id=r['id'],source=a['source_tests'][i],accepted46='PASS' if i==0 else 'FAIL',v1='ERROR',v2=r['outcome'],runtime_seconds=r['seconds'],events=[{k:e[k] for k in ['outcome','exception_class','message_sha256','traceback_sha256']} for e in r['events']]) for i,r in enumerate(records)],accepted46_message_exact=True,old_errors_retained=True,input=payload(),interpreter=a['interpreter']))
    print('FOCUSED: 04A23 PASS; 04A24 inherited FAIL with exact accepted46 message; no missing-file ERROR',flush=True)
if __name__=='__main__':
    try:globals()[sys.argv[1]]()
    except BaseException as e:
        if isinstance(e,SystemExit):raise
        import traceback
        save(PRIVATE/('failed-attempt-'+datetime.datetime.now().strftime('%Y%m%d-%H%M%S')+'.json'),dict(action=sys.argv[1],exception=type(e).__name__,traceback=traceback.format_exc()))
        print('RECOVERY STOP: protected failed attempt retained; '+type(e).__name__,flush=True);raise SystemExit(2)
