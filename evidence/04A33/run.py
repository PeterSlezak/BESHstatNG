"""Offline isolated builds/tests using the unchanged accepted repository runner.
No restore, packaging, installer, signing, historical output or native host launch.
"""
from pathlib import Path
import importlib.util,sys
from concurrent.futures import ThreadPoolExecutor
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('runner31',ROOT/'scripts/test_suite_rationalization.py')
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
class Runner(module.Runner):
    def command(self,name,args,cwd=ROOT,env=None):
        environment=dict(env or {})
        environment['BESHSTAT_04A33_CASES']=str(self.run/'compatibility-cases'/name)
        environment['BESHSTAT_04A31_CASES']=str(self.run/'compatibility-cases'/name)
        return super().command(name,args,cwd,environment)
if __name__=='__main__':
    action,config,label=sys.argv[1:4]; runner=Runner(ROOT/'.test-rationalization-04A33.tmp')
    if action=='build':
        r=runner.build(config,label)
        if any(r['exit_codes']) or not r['source_unchanged']:sys.exit(1)
    elif action=='focused':
        results=[runner.test(config,label,suite,label+'-'+config+'-graphics33-'+suite,'FullyQualifiedName~Graphics33|FullyQualifiedName~Graphics30|FullyQualifiedName~ViolinPlot|FullyQualifiedName~HistogramBinning|FullyQualifiedName~ChartAxisScaling') for suite in ['Core','Legacy']]
        module.save(runner.run/(label+'-'+config+'-focused.json'),results)
        if module.failed(results):sys.exit(1)
    elif action=='matrix':
        if module.failed(runner.matrix(config,label)):sys.exit(1)
    elif action=='managed':
        before=module.source_pins()
        def lane(suites):return [runner.test(config,label,suite,label+'-'+config+'-'+suite) for suite in suites]
        with ThreadPoolExecutor(max_workers=4) as pool:lanes=list(pool.map(lane,[['Core','Common'],['Legacy','ControlMitigation'],['OfficeAddin'],['OfficeExecution']]))
        result=dict(configuration=config,lanes=lanes,source_unchanged=before==module.source_pins())
        module.save(runner.run/(label+'-'+config+'-managed.json'),result)
        if module.failed(result):sys.exit(1)
    elif action=='serial':
        if module.failed(runner.serial(label)):sys.exit(1)
    elif action=='source-recheck':
        r=runner.command(label+'-python-scripts',[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p','test_*.py','-v'])
        if r['exit_code']:sys.exit(1)
    elif action=='guard':
        r=runner.command(label+'-guard-build',['dotnet','build','evidence/04A33/helpers/Graphics33Guard.csproj','--no-restore','-p:ProjectAssetsFile='+str(ROOT/'.04a29-audit.tmp/obj/project.assets.json'),'-p:BaseIntermediateOutputPath='+str(runner.run/'guard-obj')+'/', '-p:OutputPath='+str(runner.run/'guard-bin')+'/', '--nologo','--verbosity','quiet'])
        if r['exit_code']:sys.exit(1)
        r=runner.command(label+'-guard-check',['dotnet',str(runner.run/'guard-bin/Graphics33Guard.dll')])
        if r['exit_code']:sys.exit(1)
    elif action=='history':
        r=runner.command(label+'-historical-metadata',['dotnet',str(runner.run/'guard-bin/Graphics33Guard.dll'),'--results-snapshot'])
        if r['exit_code']:sys.exit(1)
    elif action=='contracts':
        r=runner.command(label+'-audit-tests',[sys.executable,'-B','-m','unittest','discover','-s','evidence/04A33','-p','test_*.py','-v'])
        if r['exit_code']:sys.exit(1)
    elif action=='capture':
        r=runner.command(label+'-'+config+'-adapter-capture',['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A33/capture.ps1','-Label',label,'-Configuration',config])
        if r['exit_code']:sys.exit(1)
    elif action=='versions':
        for name,args in [('dotnet',['dotnet','--version']),('node',['node','--version']),('python',[sys.executable,'--version']),('msbuild',[runner.msbuild,'/version','/nologo']),('framework',['powershell','-NoProfile','-Command','[Environment]::Version.ToString()'])]:
            r=runner.command(label+'-version-'+name,args)
            if r['exit_code']:sys.exit(1)
    elif action in ['appearance','appearance2']:
        r=runner.command(label+'-'+config+'-'+action+'-probe',['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File','evidence/04A33/appearance-probe.ps1','-Label',label,'-Configuration',config])
        if r['exit_code']:sys.exit(1)
    else:raise ValueError(action)
