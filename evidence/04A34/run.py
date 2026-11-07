"""Use the accepted isolated offline runner without modifying historical receipts."""
from pathlib import Path
import importlib.util, sys
from concurrent.futures import ThreadPoolExecutor
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('accepted_runner',ROOT/'scripts/test_suite_rationalization.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
if __name__=='__main__':
    action,config,label=sys.argv[1:4]
    runner=m.Runner(ROOT/'.test-rationalization-04A34.tmp')
    if action=='build': result=runner.build(config,label)
    elif action=='matrix': result=runner.matrix(config,label)
    elif action=='serial': result=runner.serial(label)
    elif action=='managed':
        before=m.source_pins()
        def lane(suites):return [runner.test(config,label,suite,label+'-'+config+'-'+suite) for suite in suites]
        with ThreadPoolExecutor(max_workers=4) as pool:
            lanes=list(pool.map(lane,[['Core','Common'],['Legacy','ControlMitigation'],['OfficeAddin'],['OfficeExecution']]))
        result=dict(configuration=config,lanes=lanes,source_unchanged=before==m.source_pins())
        m.save(runner.run/(label+'-'+config+'-managed.json'),result)
    elif action=='focused':
        result=[runner.test(config,label,suite,label+'-'+config+'-focused-'+suite,'FullyQualifiedName~Graphics34|FullyQualifiedName~Graphics33|FullyQualifiedName~Graphics32|FullyQualifiedName~Graphics31|FullyQualifiedName~Graphics30') for suite in ['Core','Common','Legacy']]
        m.save(runner.run/(label+'-'+config+'-focused.json'),result)
    else: raise ValueError(action)
    if action=='build' and (any(result['exit_codes']) or not result['source_unchanged']): sys.exit(1)
    if m.failed(result): sys.exit(1)
