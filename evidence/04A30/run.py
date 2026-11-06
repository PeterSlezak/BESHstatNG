"""Batch-specific isolated execution over the accepted unchanged runner.

No restore/install. Raw receipts stay under the fresh ignored run root. Each
test execution gets a different optional 04A30 compatibility-export directory.
"""
from pathlib import Path
import sys,importlib.util,json
from concurrent.futures import ThreadPoolExecutor
import time
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('rationalization',ROOT/'scripts/test_suite_rationalization.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
class Runner(module.Runner):
    def command(self,name,args,cwd=ROOT,env=None):
        environment=dict(env or {})
        environment['BESHSTAT_04A30_CASES']=str(self.run/'compatibility-cases'/name)
        return super().command(name,args,cwd,environment)

if __name__=='__main__':
    action,config,label=sys.argv[1:4]
    runner=Runner(ROOT/'.test-rationalization-04A30.tmp')
    if action=='focused':
        results=[]
        for suite in ['Core','Common','Legacy']:
            results.append(runner.test(config,label,suite,label+'-'+config+'-graphics30-'+suite,
                'FullyQualifiedName~Graphics30|FullyQualifiedName~OrderedPlotGroups30|FullyQualifiedName~HistogramBinningCoreTests|FullyQualifiedName~ViolinPlotCoreTests|FullyQualifiedName~CategoricalHistogramCoreTests|FullyQualifiedName~ChartAxisScaling'))
        module.save(runner.run/(label+'-'+config+'-graphics30.json'),results)
        if any(r['command']['exit_code'] for r in results):sys.exit(1)
    elif action=='matrix':
        result=runner.matrix(config,label)
        if module.failed(result):sys.exit(1)
    elif action=='managed':
        before=module.source_pins();start=time.perf_counter()
        def lane(suites):
            return [runner.test(config,label,suite,label+'-'+config+'-'+suite) for suite in suites]
        with ThreadPoolExecutor(max_workers=4) as pool:
            lanes=list(pool.map(lane,[['Core','Common'],['Legacy','ControlMitigation'],['OfficeAddin'],['OfficeExecution']]))
        result=dict(configuration=config,label=label,scope='Full managed; Node/Python are configuration-independent and run in Debug matrix',seconds=time.perf_counter()-start,lanes=lanes,source_unchanged=before==module.source_pins())
        module.save(runner.run/(label+'-'+config+'-managed.json'),result)
        if module.failed(result):sys.exit(1)
    elif action=='serial':
        result=runner.serial(label)
        module.save(runner.run/(label+'-serial.json'),result)
        if module.failed(result):sys.exit(1)
    elif action=='guard':
        args=['dotnet','build','evidence/04A30/helpers/Graphics30Guard.csproj','--no-restore',
            '-p:ProjectAssetsFile='+str(ROOT/'.04a29-audit.tmp/obj/project.assets.json'),
            '-p:BaseIntermediateOutputPath='+str(runner.run/'guard-obj')+'/',
            '-p:OutputPath='+str(runner.run/'guard-bin')+'/', '--nologo','--verbosity','quiet']
        build=runner.command(label+'-guard-build',args)
        if build['exit_code']:sys.exit(1)
        result=runner.command(label+'-guard-check',['dotnet',str(runner.run/'guard-bin/Graphics30Guard.dll')])
        if result['exit_code']:sys.exit(1)
    elif action=='results-snapshot':
        result=runner.command(label+'-historical-metadata',['dotnet',str(runner.run/'guard-bin/Graphics30Guard.dll'),'results-snapshot'])
        if result['exit_code']:sys.exit(1)
    elif action=='source-recheck':
        result=runner.command(label+'-final-python-scripts',[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p','test_*.py','-v'])
        if result['exit_code']:sys.exit(1)
    else:raise ValueError(action)
