"""04A.28 opt-in isolated validation; accepted full runner remains unchanged."""
import argparse
import importlib.util
import os
import shutil
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('accepted_runner', ROOT/'scripts/test_suite_rationalization.py')
r = importlib.util.module_from_spec(spec)
spec.loader.exec_module(r)

class BatchRunner(r.Runner):
    def command(self, name, args, cwd=ROOT, env=None):
        # These opt-in historical capture modes must never rewrite old evidence.
        safe = {key: '' for key in os.environ if key.startswith('BESHSTAT_') and ('CAPTURE' in key or 'OBSERVATION' in key)}
        safe.update(env or {})
        return super().command(name, args, cwd, safe)

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('action', choices=['build','focused','matrix','release','serial','apps','benchmark'])
    p.add_argument('--configuration', choices=['Debug','Release'], default='Debug')
    p.add_argument('--label',required=True)
    p.add_argument('--run',type=Path,default=ROOT/'.test-rationalization-04a28-run01.tmp')
    p.add_argument('--baseline-run',type=Path,default=ROOT/'.test-rationalization-20261105-01.tmp')
    a=p.parse_args()
    if not a.label.replace('-','').isalnum():raise ValueError('Invalid label')
    runner=BatchRunner(a.run)
    if a.action=='build': result=runner.build(a.configuration,a.label)
    elif a.action=='matrix':result=runner.matrix('Debug',a.label)
    elif a.action=='serial':result=runner.serial(a.label)
    elif a.action=='benchmark':
        import json
        baseline=a.baseline_run.resolve()
        proof=json.loads((baseline/'builds/after/Debug/build.json').read_bytes())
        entry=json.loads((runner.run/'source-pins.json').read_bytes())
        checked={p:h for p,h in proof['source_pins'].items() if p.endswith(('.vb','.cs','.vbproj','.csproj'))}
        mismatches=[p for p,h in checked.items() if entry.get(p)!=h]
        if mismatches:raise ValueError('Baseline compiled inputs differ from verified main: '+repr(mismatches))
        selection='FullyQualifiedName~SharedOutputPlanTests|FullyQualifiedName~CompositeOutputPlanTests'
        execution,assembly,settings=runner.container('Debug',a.label,'Legacy',a.label+'-baseline-matched')
        shutil.copytree(baseline/'builds/after/Debug/legacy',assembly.parent,dirs_exist_ok=True)
        command=runner.command(a.label+'-baseline-matched',[runner.vstest,str(assembly),'/Settings:'+str(settings),'/ResultsDirectory:'+str(execution/'results'),'/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common'),'/TestCaseFilter:'+selection,'/Logger:trx;LogFileName=Legacy.trx'],env={'BESHSTAT_04A02_SOURCE_ROOT':str(ROOT)})
        before=r.trx(execution/'results/Legacy.trx');before.update(command=command,baseline_build_receipt_sha256=r.sha(baseline/'builds/after/Debug/build.json'),compiled_input_count=len(checked),compiled_inputs_match_entry=True,assembly_sha256=r.sha(assembly))
        after=runner.test('Debug',a.label,'Legacy',a.label+'-current-matched',selection)
        r.require_identity_preserved(before['records'],after['records'])
        result=[before,after]
        r.save(runner.run/(a.label+'-matched-timing.json'),dict(results=result,selection=selection,qualification='Matched existing orchestration/recording tests; not native Excel timing; one pair cannot establish causal acceleration. Baseline binaries qualified to entry compiled-source hashes.'))
    elif a.action=='apps':
        result=[]
        for name,project in [('OfficeAddin','BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj'),('WasmSmoke','BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj')]:
            target=r.fresh(runner.run/'application-builds'/a.label/a.configuration/name)
            output=r.fresh(target/'bin')
            for dll in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:
                shutil.copy2(runner.run/'builds'/a.label/a.configuration/'portable'/dll,output/dll)
            result.append(runner.command(a.label+'-'+a.configuration+'-app-'+name,['dotnet','msbuild',project,'/t:Build','/p:Configuration='+a.configuration,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(target/'obj')+'\\','/m:4','/v:minimal','/nr:false']))
        r.save(runner.run/(a.label+'-'+a.configuration+'-application-builds.json'),result)
    else:
        selections = {
            'Core':'FullyQualifiedName~ResultTable|FullyQualifiedName~SettingsAndPValue',
            'Common':'FullyQualifiedName~Output28|FullyQualifiedName~OutputStylesTests|FullyQualifiedName~OutputLayoutPlannerTests|FullyQualifiedName~OutputTargetPlannerTests|FullyQualifiedName~WriteResultTests|FullyQualifiedName~PValuePresentationPlanTests',
            'Legacy':'FullyQualifiedName~Output28|FullyQualifiedName~SharedOutputPlanTests|FullyQualifiedName~CompositeOutputPlanTests',
            'OfficeAddin':None, 'OfficeExecution':None}
        configuration='Release' if a.action=='release' else a.configuration
        result=[runner.test(configuration,a.label,suite,a.label+'-'+configuration+'-selected-'+suite,selection) for suite,selection in selections.items()]
        r.save(runner.run/(a.label+'-'+configuration+'-selected.json'),result)
    if isinstance(result,dict) and 'exit_codes' in result:
        bad=any(result['exit_codes']) or not result['source_unchanged']
    else:bad=r.failed(result)
    raise SystemExit(1 if bad else 0)

if __name__=='__main__':main()
