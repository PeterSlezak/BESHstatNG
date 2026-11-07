"""Accepted offline runner; full Framework runs serially without concurrent hosts."""
from pathlib import Path
import sys,importlib.util
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('accepted_runner36',ROOT/'scripts/test_suite_rationalization.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
if __name__=='__main__':
    action,config,label=sys.argv[1:4];runner=m.Runner(ROOT/'.test-rationalization-04A36.tmp')
    if action=='build':r=runner.build(config,label)
    elif action=='matrix':r=runner.matrix(config,label)
    elif action=='serial':r=runner.serial(label)
    elif action=='managed':
        before=m.source_pins();r=dict(configuration=config,results=[runner.test(config,label,s,label+'-'+config+'-serial-'+s) for s in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']],source_unchanged=before==m.source_pins());m.save(runner.run/(label+'-'+config+'-managed.json'),r)
    elif action=='focused':
        r=[runner.test(config,label,s,label+'-'+config+'-focused-'+s,'FullyQualifiedName~Graphics36|FullyQualifiedName~Graphics35|FullyQualifiedName~Graphics34|FullyQualifiedName~Survival13|FullyQualifiedName~Spc') for s in ['Core','Common','Legacy']];m.save(runner.run/(label+'-'+config+'-focused.json'),r)
    else:raise ValueError(action)
    sys.exit(1 if (any(r['exit_codes']) if action=='build' else m.failed(r)) else 0)
