"""Receipt-producing offline supplementary checks, using the accepted isolated runner."""
from pathlib import Path
import importlib.util,sys
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('accepted_runner',ROOT/'scripts/test_suite_rationalization.py')
m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
runner=m.Runner(ROOT/'.test-rationalization-04A34.tmp')
if __name__=='__main__':
    action=sys.argv[1]
    if action=='python':
        r=runner.command('sealed-Debug-python-scripts',[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p','test_*.py','-v'])
        sys.exit(r['exit_code'])
    elif action=='versions':
        for name,args in [('dotnet',['dotnet','--version']),('node',['node','--version']),('python',[sys.executable,'--version']),('msbuild',[runner.msbuild,'/version','/nologo']),('framework',['powershell','-NoProfile','-Command','[Environment]::Version.ToString()'])]:
            r=runner.command('final-version-'+name,args)
            if r['exit_code']:sys.exit(1)
    elif action=='cjs':
        r=runner.command('final-wasm-cjs',['node','--test','BESHStat.WasmSmoke/tests/*.test.cjs'])
        sys.exit(r['exit_code'])
    elif action=='audits':
        label=sys.argv[2]
        r=runner.command(label+'-packet-audits',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A34','-p','test_*.py','-v'])
        sys.exit(r['exit_code'])
    else:raise ValueError(action)
