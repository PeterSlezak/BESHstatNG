"""One scheduled focused test execution, raw diagnostics exclusively private."""
from common import *
from scanner_adapter import module as privacy_module, function_ast
import collections, importlib.util, io, unittest

lock()
inventory=load(PRIOR/'python-node-scanners/campaign-v2/python-discovery-inventory-v1.json')
delta=load(PRIOR/'python-node-scanners/python-delta-v2.json')
ids={r['identity'] for r in delta['diagnostic_drifts']}|{r['id'] for r in delta['changes']}
selected=[r for r in inventory['entries'] if r['id'] in ids]
assert {r['id'] for r in selected}==ids
save(PRIVATE/'focused-schedule.json',dict(ids=sorted(ids),expected=len(ids),concurrency=1,source_pins=[pin(r['module_path']) for r in selected],python=pin(sys.executable) if Path(sys.executable).is_relative_to(ROOT) else dict(bytes=Path(sys.executable).stat().st_size,sha256=digest(sys.executable))))
sys.path.insert(0,str(ROOT/'scripts/tests'));sys.path.insert(0,str(ROOT/'BESHStat.Excel.Common.Tests/src'))
cached={};cache_stats=collections.Counter();scanners={p:privacy_module(p) for p in ('13','14')}
assert function_ast(scanners['13'])==function_ast(scanners['14'])
scanner_sources={p:pin(scanners[p].__file__) for p in scanners}
for phase,m in scanners.items():
    original=m.scan
    def scan(candidates, original=original, phase=phase):
        findings=[];reviewed=set()
        for candidate in candidates:
            target=ROOT/candidate
            if not target.is_file():f,r=original([candidate])
            else:
                key=(candidate,digest(target))
                if key not in cached:
                    cached[key]=original([candidate]);cache_stats['fresh']+=1
                    assert digest(target)==key[1]
                else:cache_stats['identical_ast_input_reuse']+=1
                f,r=cached[key]
            findings.extend(f);reviewed.update(r)
        return findings,sorted(reviewed)
    m.scan=scan
loaded={};tests=[]
for row in selected:
    assert digest(ROOT/row['module_path'])==row['module_sha256']
    name=row['module']
    if name not in loaded:
        spec=importlib.util.spec_from_file_location(name,ROOT/row['module_path'])
        m=importlib.util.module_from_spec(spec);sys.modules[name]=m;spec.loader.exec_module(m)
        if name in ('test_04a13_survival_workflow','test_04a14_mixed_workflow'):
            original=m.module
            def factory(name,path, original=original):
                if path in ('verify-04a13-publication.py','verify-04a14-publication.py'):return scanners[path[10:12]]
                return original(name,path)
            m.module=factory
        loaded[name]=m
    _,cls,method=row['id'].rsplit('.',2)
    tests.append(getattr(loaded[name],cls)(method))
records=[];active={}
class Result(unittest.TextTestResult):
    def startTest(self,test):
        super().startTest(test);row=dict(id=test.id(),outcome='PASS',events=[]);records.append(row);active[id(test)]=row
    def event(self,test,error,outcome,subtest=None):
        trace=self._exc_info_to_string(error,test)
        active[id(test)]['events'].append(dict(outcome=outcome,subtest=subtest,message=str(error[1]),message_sha256=sha(str(error[1]).encode()),traceback=trace,traceback_sha256=sha(trace.encode())))
        if outcome=='ERROR' or active[id(test)]['outcome']=='PASS':active[id(test)]['outcome']=outcome
    def addFailure(self,test,error):self.event(test,error,'FAIL');super().addFailure(test,error)
    def addError(self,test,error):self.event(test,error,'ERROR');super().addError(test,error)
    def addSubTest(self,test,subtest,error):
        if error:self.event(test,error,'FAIL' if issubclass(error[0],test.failureException) else 'ERROR',subtest.id())
        super().addSubTest(test,subtest,error)
out=io.StringIO();start=utc()
result=unittest.TextTestRunner(stream=out,verbosity=2,resultclass=Result).run(unittest.TestSuite(tests))
assert len(records)==len(ids)==result.testsRun
save(PRIVATE/'focused-raw.json',dict(start_utc=start,end_utc=utc(),records=records,testsRun=result.testsRun,outcomes=dict(collections.Counter(r['outcome'] for r in records)),stdout=out.getvalue(),exit_code=0 if result.wasSuccessful() else 1,scanner_sources=scanner_sources,cache_stats=dict(cache_stats)))
save(PRIVATE/'focused-cache.json',dict(rows=[dict(path=k[0],sha256=k[1],findings=v[0],reviewed=v[1]) for k,v in cached.items()],scope='Exact source AST, path and bytes; current environment; same process; no assertions suppressed.'))
print(json.dumps(dict(parents=len(records),outcomes=dict(collections.Counter(r['outcome'] for r in records)),command_exit=0 if result.wasSuccessful() else 1)),flush=True)
raise SystemExit(0 if result.wasSuccessful() else 1)
