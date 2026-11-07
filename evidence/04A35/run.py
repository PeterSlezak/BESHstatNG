"""Reuse the accepted isolated offline runner; original scripts/history unchanged."""
from pathlib import Path
import sys,importlib.util
from concurrent.futures import ThreadPoolExecutor
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('accepted_runner35',ROOT/'scripts/test_suite_rationalization.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
if __name__=='__main__':
    action,config,label=sys.argv[1:4];runner=m.Runner(ROOT/'.test-rationalization-04A35.tmp')
    if action=='build':r=runner.build(config,label)
    elif action=='matrix':r=runner.matrix(config,label)
    elif action=='serial':r=runner.serial(label)
    elif action=='managed':
        before=m.source_pins()
        def lane(suites):return [runner.test(config,label,s,label+'-'+config+'-'+s) for s in suites]
        with ThreadPoolExecutor(max_workers=4) as pool:lanes=list(pool.map(lane,[['Core','Common'],['Legacy','ControlMitigation'],['OfficeAddin'],['OfficeExecution']]))
        r=dict(configuration=config,lanes=lanes,source_unchanged=before==m.source_pins());m.save(runner.run/(label+'-'+config+'-managed.json'),r)
    elif action=='focused':
        r=[runner.test(config,label,s,label+'-'+config+'-focused-'+s,'FullyQualifiedName~Graphics35|FullyQualifiedName~Graphics34|FullyQualifiedName~AgreementPlotPayload|FullyQualifiedName~Agreement26|FullyQualifiedName~Psm25') for s in ['Core','Common','Legacy']];m.save(runner.run/(label+'-'+config+'-focused.json'),r)
    else:raise ValueError(action)
    sys.exit(1 if m.failed(r) else 0)
