"""Isolated offline runs. Original scripts/toolchain/time limits remain unchanged."""
from pathlib import Path
import importlib.util
import sys
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
spec=importlib.util.spec_from_file_location('accepted_isolated38',ROOT/'scripts/test_suite_rationalization.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
PRIVATE=ROOT/'.test-rationalization-04A38.tmp'

def main():
    PRIVATE.mkdir(exist_ok=True)
    action,label=sys.argv[1:3];r=m.Runner(PRIVATE)
    if action=='build':
        result=r.build(sys.argv[3],label)
        if any(result['exit_codes']):raise SystemExit(1)
    elif action=='managed':
        config=sys.argv[3];before=m.source_pins()
        result=dict(configuration=config,results=[r.test(config,label,s,label+'-'+config+'-serial-'+s) for s in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']],source_unchanged=before==m.source_pins())
        m.save(PRIVATE/(label+'-'+config+'-managed.json'),result)
    elif action=='source':
        results=[]
        for suffix,cwd,args in [('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:results.append(r.command(label+'-'+suffix,args,cwd=ROOT/cwd))
        for suffix,path in [('tools','BESHstatNG/tools/tests'),('help','BESHStatNG_Help_MkDocs/tests'),('scripts','scripts/tests')]:results.append(r.command(label+'-python-'+suffix,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p','test_*.py','-v']))
        probe=label+'-semantic';results.append(r.test('Debug',label,'Common',probe,'FullyQualifiedName~MigrationSemantic04A02Tests'))
        results.append(r.command(label+'-common-python',[sys.executable,'-B','-m','unittest','discover','-s','BESHStat.Excel.Common.Tests/src','-p','test_*.py','-v'],env={'BESHSTAT_04A02_G3_CANDIDATES':str(PRIVATE/'executions'/probe/'observations')}))
        for batch in ['34','35','36','37']:results.append(r.command(label+'-accepted'+batch+'-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A'+batch,'-p','test_audit.py','-v']))
        results.append(r.command(label+'-accepted37-supplement',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A37/late-archived-entry-supplement/v1','-p','test_supplement.py','-v']))
        m.save(PRIVATE/(label+'-source.json'),results)
    elif action=='serial':m.save(PRIVATE/(label+'-serial.json'),r.serial(label))
    else:raise ValueError(action)

if __name__=='__main__':main()
