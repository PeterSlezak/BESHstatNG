"""Bounded source-check profiling; repetitions never inflate final method totals."""
from collections import defaultdict
import importlib.util
import json
from pathlib import Path
import sys
import time
import unittest

ROOT=Path(__file__).resolve().parents[1]
def module(name,path):
    s=importlib.util.spec_from_file_location(name,ROOT/path);m=importlib.util.module_from_spec(s);s.loader.exec_module(m);return m
c=module('capture26_profile','scripts/capture-04a26-contract.py')
SELECTED=['test_04a14_mixed_workflow','test_04a24_multivariate_payload','test_04a25_psm']
def worker():
    sys.path.insert(0,str(ROOT/'scripts/tests'));rows=[]
    class TimedResult(unittest.TextTestResult):
        def startTest(self,test):
            self.begin=time.perf_counter();self.outcome='PASS';super().startTest(test)
        def addFailure(self,test,err):self.outcome='FAIL';super().addFailure(test,err)
        def addError(self,test,err):self.outcome='ERROR';super().addError(test,err)
        def addSkip(self,test,reason):self.outcome='SKIP';super().addSkip(test,reason)
        def stopTest(self,test):
            rows.append(dict(identity=test.id(),module=test.__class__.__module__,outcome=self.outcome,seconds=time.perf_counter()-self.begin));super().stopTest(test)
    suite=unittest.TestSuite(unittest.defaultTestLoader.loadTestsFromName(n) for n in SELECTED)
    start=time.perf_counter();result=unittest.TextTestRunner(verbosity=2,resultclass=TimedResult).run(suite)
    folder=c.RUN/'profiling';folder.mkdir(exist_ok=False)
    (folder/'python-modules.json').write_text(json.dumps(dict(methods=rows,seconds=time.perf_counter()-start,tests_run=result.testsRun),indent=2),encoding='utf-8')
    return 0 if result.wasSuccessful() else 1
def main():
    runner=module('verify26_profile','scripts/verify-04a26.py')
    code=runner.command('profile-python-modules',[sys.executable,'-B',str(Path(__file__)),'--worker'])
    data=json.loads((c.RUN/'profiling/python-modules.json').read_bytes());groups=defaultdict(lambda:dict(seconds=0,executed=0,passed=0,fail=0,error=0))
    for row in data['methods']:
        g=groups[row['module']];g['seconds']+=row['seconds'];g['executed']+=1
        if row['outcome']=='PASS':g['passed']+=1
        elif row['outcome']=='FAIL':g['fail']+=1
        elif row['outcome']=='ERROR':g['error']+=1
    registry=json.loads((c.OUT/'frozen-capture-hashes.json').read_bytes())['files'];scans=[]
    for attempt in range(2):
        start=time.perf_counter();hashes={p:c.sha(ROOT/p) for p in registry};scans.append(dict(attempt=attempt+1,seconds=time.perf_counter()-start,members=len(hashes),bytes=sum((ROOT/p).stat().st_size for p in registry)))
        if attempt:assert hashes==previous
        previous=hashes
    c.emit('python-module-profile.json',dict(status='MEASURED_BOUNDED_SOURCE_PROFILE',modules=dict(groups),methods=data['methods'],exit_code=code,private_receipt_sha256=c.sha(c.RUN/'profiling/python-modules.json'),module_source_hashes={n:c.sha(ROOT/('scripts/tests/'+n+'.py')) for n in SELECTED},immutable_scan_measurements=scans,scan_registry_sha256=c.sha(c.OUT/'frozen-capture-hashes.json'),scope='Three representative frozen source/corpus/integrity modules profiled after final matrices, serially. Their repetitions are excluded from unique final totals. Other modules have matrix root-command timing only, not invented individual timings.',cache='No historical test/verdict cache enabled. Two fresh immutable-registry byte scans compared with each other; owner plan is an explicitly recorded entry exception.',optimization='Repeated byte hashing is eligible for separately reviewed content-addressed scan reuse; none bypassed in this batch. Slow transition/Help clone suites remain fully executed twice.'))
    print('Profiled',data['tests_run'],'repeated methods;',dict(groups),'exit',code)
if __name__=='__main__':raise SystemExit(worker() if '--worker' in sys.argv else main())
