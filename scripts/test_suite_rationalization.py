"""Offline, isolated review runner. Default/full retains all accepted selections.

No historical results are written. A fresh ignored *.tmp directory holds private
execution receipts; evidence/TestRationalization contains only review metadata.
Builds precede execution; four outer lanes use serial inner test hosts.
"""
import argparse
from collections import Counter
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
BASE='c2ea685e893d5d1379a1bd8c13c390fc6a259f58'
NS='{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
SPC=('SpcShewhartChartTests','SpcTimeWeightedChartTests','SpcMultivariateTests','SpcRuleEvaluatorTests','SpcResultTablesAndDispatchTests')
SPC_FILTER='|'.join('FullyQualifiedName~.'+name+'.' for name in SPC)
SUITES={
    'Core':('BESHStat.Core.Tests','BESHStat.Core.Tests.dll','net10.0'),
    'Common':('BESHStat.Excel.Common.Tests','BESHStat.Excel.Common.Tests.dll','net10.0'),
    'Legacy':('BESHStatNG.Test','BESHStatNG.Test.dll','net48'),
    'OfficeAddin':('BESHStat.OfficeAddin/tests','OfficeAddin.Tests.dll','net10.0'),
    'OfficeExecution':('BESHStat.WasmSmoke/tests/Execution','Execution.Tests.dll','net10.0')}
PROJECTS=['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj','BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj']

def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,text=True).strip()
def fresh(path):
    p=Path(path)
    p.mkdir(parents=True,exist_ok=False)
    return p
def save(path,data):
    path=Path(path);path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(data,f,ensure_ascii=False,allow_nan=False,indent=2);f.write('\n')
def inside(path,parent):return Path(path).resolve().is_relative_to(Path(parent).resolve())
def require_private_run(path,root=ROOT):
    path=Path(path).resolve();root=Path(root).resolve()
    if path.parent!=root or not path.name.startswith('.test-rationalization-') or not path.name.endswith('.tmp'):
        raise ValueError('Run must be a new direct-child .test-rationalization-*.tmp directory')
    return path
def source_pins():
    paths=git('ls-files').splitlines()+git('ls-files','--others','--exclude-standard').splitlines()
    return {p:sha(ROOT/p) for p in sorted(set(paths)) if p.endswith(('.vb','.cs','.vbproj','.csproj','.py','.ps1','.runsettings','.cjs','.mjs','.props','.targets')) and not p.startswith(('evidence/','TestResults/')) and (ROOT/p).is_file()}
def case_counter(records):return Counter((r['class_name'],r['method'],r['test_name'],r['outcome']) for r in records)
def require_identity_preserved(before,after):
    b=Counter((r['class_name'],r['method'],r['test_name']) for r in before)
    a=Counter((r['class_name'],r['method'],r['test_name']) for r in after)
    if b!=a:raise ValueError('Discovered execution identities or multiplicities changed')
def failed(value):
    if isinstance(value,dict):
        if value.get('status')=='NOT_RUN' or value.get('exit_code',0)!=0:return True
        if 'source_unchanged' in value and value['source_unchanged'] is not True:return True
        return any(failed(v) for v in value.values())
    if isinstance(value,list):return any(failed(v) for v in value)
    return False
def trx(path):
    root=ET.parse(path).getroot();definitions={}
    for d in root.findall('.//'+NS+'UnitTest'):
        m=d.find(NS+'TestMethod')
        if m is not None:definitions[d.attrib['id']]=(m.attrib.get('className',''),m.attrib.get('name',''))
    records=[]
    for n in root.findall('.//'+NS+'UnitTestResult'):
        cls,method=definitions.get(n.attrib['testId'],('UNKNOWN','UNKNOWN'))
        records.append(dict(class_name=cls,method=method,test_name=n.attrib['testName'],outcome=n.attrib['outcome'],duration=n.attrib.get('duration'),message=n.findtext('.//'+NS+'Message','')))
    if not records:raise ValueError('ZERO_TESTS: no completed framework cases')
    counters=root.find('.//'+NS+'Counters')
    return dict(records=records,outcomes=dict(Counter(r['outcome'] for r in records)),counters=dict(counters.attrib) if counters is not None else {},sha256=sha(path))
def tools():
    vswhere=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    args=[str(vswhere),'-products','*','-version','[18.0,19.0)']
    msbuild=subprocess.check_output(args+['-requires','Microsoft.Component.MSBuild','-find',r'MSBuild\Current\Bin\MSBuild.exe'],text=True).splitlines()[0]
    vs=subprocess.check_output(args+['-property','installationPath'],text=True).splitlines()[0]
    return msbuild,str(Path(vs)/'Common7/IDE/Extensions/TestPlatform/vstest.console.exe')

class Runner:
    def __init__(self,run):
        self.run=require_private_run(run)
        if not self.run.is_dir():raise ValueError('Run has not been initialized')
        self.msbuild,self.vstest=tools()
    def command(self,name,args,cwd=ROOT,env=None):
        folder=fresh(self.run/'commands'/name);start=time.perf_counter()
        environment=os.environ.copy();environment.update(PYTHONDONTWRITEBYTECODE='1',MSBUILDDISABLENODEREUSE='1',DOTNET_CLI_TELEMETRY_OPTOUT='1',DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1')
        if env:environment.update(env)
        with (folder/'stdout.log').open('x',encoding='utf-8') as out,(folder/'stderr.log').open('x',encoding='utf-8') as err:
            proc=subprocess.run([str(a) for a in args],cwd=cwd,env=environment,stdout=out,stderr=err,creationflags=subprocess.CREATE_NO_WINDOW)
        record=dict(command=[str(a) for a in args],cwd=str(cwd),exit_code=proc.returncode,seconds=time.perf_counter()-start,stdout_sha256=sha(folder/'stdout.log'),stderr_sha256=sha(folder/'stderr.log'),runner_sha256=sha(__file__))
        save(folder/'command.json',record);print(name,record['exit_code'],round(record['seconds'],2),flush=True)
        return record
    def build(self,config,label):
        build=fresh(self.run/'builds'/label/config)
        output=build/'portable';output.mkdir();pins=source_pins();codes=[]
        common=['/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(output)+'\\','/m:4','/v:minimal','/nr:false']
        for i,project in enumerate(PROJECTS):
            intermediate=build/'obj'/str(i)
            args=['dotnet','msbuild',project,'/t:Build',*common,'/p:IntermediateOutputPath='+str(intermediate)+'\\']
            record=self.command(label+'-'+config+'-build-'+str(i),args);codes.append(record['exit_code'])
            if record['exit_code']:break
            if i==1:
                # Freeze production dependencies before MSTest 3/modern test
                # adapters enter portable output. Framework uses MSTest 2.
                shutil.copytree(output,build/'legacy')
        if not any(codes):
            legacy=build/'legacy'
            legacy_common=[x if not x.startswith('/p:OutputPath=') else '/p:OutputPath='+str(legacy)+'\\' for x in common]
            # Compile-only Windows host targets avoid version/help regeneration,
            # XLL packaging, post-build events, installation and signing.
            for i,project,target in [('host','BESHstatNG/BESHstatNG.vbproj','PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory'),('Legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj','Build')]:
                record=self.command(label+'-'+config+'-build-'+i,[self.msbuild,project,'/t:'+target,*legacy_common,'/p:IntermediateOutputPath='+str(build/'obj'/i)+'\\','/p:PythonExe='+sys.executable]);codes.append(record['exit_code'])
                if record['exit_code']:break
        if not any(codes):
            # Read only the exact small, frozen projection fixture; qualify its
            # bytes, then compile an isolated copy. Never generate a successor.
            original=ROOT/'TestResults/04A-06-20261102-run01/projection'
            proof=json.loads((original/'provenance.json').read_bytes())
            projection=fresh(build/'projection')
            for name,pin in proof['generated_hashes'].items():
                if Path(name).name!=name or sha(original/name)!=pin:raise ValueError('Protected projection hash/path mismatch')
                shutil.copy2(original/name,projection/name)
                if sha(projection/name)!=pin:raise ValueError('Projection copy mismatch')
            mitigation=build/'mitigation'
            record=self.command(label+'-'+config+'-build-mitigation',[self.msbuild,'BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj','/t:Build','/p:Configuration='+config,'/p:ProjectionSourceDir='+str(projection),'/p:ReferencePath='+str(legacy),'/p:OutputPath='+str(mitigation)+'\\','/p:IntermediateOutputPath='+str(build/'obj/mitigation')+'\\','/m:4','/v:minimal','/nr:false'])
            codes.append(record['exit_code'])
        result=dict(configuration=config,label=label,exit_codes=codes,source_pins=pins,source_unchanged=pins==source_pins(),output=str(output))
        save(build/'build.json',result)
        return result
    def container(self,config,label,suite,name):
        execution=fresh(self.run/'executions'/name)
        project,dll,tfm=SUITES[suite] if suite!='ControlMitigation' else ('ControlMitigation','BESHStatNG.Test.dll','net48')
        base=execution/'sandbox'/Path(project).name/'bin'/config/tfm
        output=self.run/'builds'/label/config/('mitigation' if suite=='ControlMitigation' else 'legacy' if suite=='Legacy' else 'portable')
        # SDK append-target-framework behavior is explicit; legacy output is
        # directly in OutputPath. Copy dependencies from both build outputs.
        shutil.copytree(output,base)
        for fixture_root in [ROOT/project/'TestData',output/'TestData',output/'net10.0/TestData']:
            if fixture_root.is_dir():shutil.copytree(fixture_root,base/'TestData',dirs_exist_ok=True)
        # Any legacy relative observation paths terminate in this new sandbox.
        for rel in ['../../../../TestResults/Codex/Batch0205/Wire','../../../../TestResults/Codex/Batch0207']:
            if not inside(base/rel,execution):raise ValueError('Relative writer escapes execution sandbox')
        if not (base/dll).is_file():raise FileNotFoundError('Prebuilt test assembly missing: '+suite)
        settings=execution/'serial.runsettings'
        with settings.open('x',encoding='utf-8') as f:f.write('<RunSettings><RunConfiguration><MaxCpuCount>1</MaxCpuCount></RunConfiguration><MSTest><Parallelize><Workers>1</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest></RunSettings>')
        return execution,base/dll,settings
    def test(self,config,label,suite,name,filter=None,discover=False):
        execution,assembly,settings=self.container(config,label,suite,name)
        args=([self.vstest] if suite in ['Legacy','ControlMitigation'] else ['dotnet','vstest'])+[str(assembly),'/Settings:'+str(settings),'/ResultsDirectory:'+str(execution/'results')]
        if suite in ['Legacy','ControlMitigation']:args+=['/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')]
        if discover:args+=['/ListTests']
        else:args+=['/Logger:trx;LogFileName='+suite+'.trx']
        if filter:args+=['/TestCaseFilter:'+filter]
        env=dict(BESHSTAT_04A02_SOURCE_ROOT=str(ROOT),BESHSTAT_KR_EXPORT_DIR=str(execution/'csv-exports'),BESHSTAT_04A02_OBSERVATIONS='',BESHSTAT_04A03_OBSERVATIONS='')
        if filter=='FullyQualifiedName~MigrationSemantic04A02Tests':env['BESHSTAT_04A02_OBSERVATIONS']=str(execution/'observations')
        record=self.command(name,args,env=env)
        if not discover:
            result=trx(execution/'results'/(suite+'.trx'));result.update(configuration=config,suite=suite,command=record)
            save(execution/'cases.json',result)
        else:result=dict(configuration=config,suite=suite,discovery_command=record)
        return result
    def focused(self,config,label):
        before=source_pins();start=time.perf_counter();results=[]
        for suite in ['Core','Legacy']:
            results.append(self.test(config,label,suite,label+'-'+config+'-spc-'+suite,SPC_FILTER))
        summary=dict(seconds=time.perf_counter()-start,results=results,source_pins=before,source_unchanged=before==source_pins())
        save(self.run/(label+'-'+config+'-focused.json'),summary)
        return summary
    def matrix(self,config,label):
        build=json.loads((self.run/'builds'/label/config/'build.json').read_bytes())
        if any(build['exit_codes']) or build['source_pins']!=source_pins():raise ValueError('Missing/stale/failed build')
        start=time.perf_counter()
        def lane(letter):
            begin=time.perf_counter();results=[]
            if letter=='A':
                for suite in ['Core','Common']:results.append(self.test(config,label,suite,label+'-'+config+'-'+suite))
            elif letter=='B':
                results.append(self.test(config,label,'Legacy',label+'-'+config+'-Legacy'))
                results.append(self.test(config,label,'ControlMitigation',label+'-'+config+'-ControlMitigation'))
            elif letter=='C':
                for suite in ['OfficeAddin','OfficeExecution']:results.append(self.test(config,label,suite,label+'-'+config+'-'+suite))
                for suffix,cwd,args in [('office-check','BESHStat.OfficeAddin',['node','--check','wwwroot/js/officeHost.mjs']),('office-node','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-check','BESHStat.WasmSmoke',['node','--check','wwwroot/js/officeBridge.js']),('wasm-node','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:
                    results.append(self.command(label+'-'+config+'-'+suffix,args,cwd=ROOT/cwd))
            else:
                for suffix,args in [('inventory',['tools/verify_stage304_inventory.py']),('catalog',['BESHstatNG/tools/validate_methods_catalog.py']),('metadata',['BESHstatNG/tools/generate_analysis_metadata.py','--check']),('resolver-check',['BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py','--check']),('help-check',['BESHstatNG/tools/generate_help_links.py','--methods-yml','BESHStatNG_Help_MkDocs/docs/_meta/methods.yml','--project-dir','BESHstatNG','--check']),('office-audit',['BESHstatNG/tools/audit_transitional_officejs.py'])]:
                    results.append(self.command(label+'-'+config+'-'+suffix,[sys.executable,'-B',*args]))
                for suffix,path in [('tools','BESHstatNG/tools/tests'),('help','BESHStatNG_Help_MkDocs/tests'),('scripts','scripts/tests')]:
                    results.append(self.command(label+'-'+config+'-python-'+suffix,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p','test_*.py','-v']))
                probe_name=label+'-'+config+'-semantic'
                results.append(self.test(config,label,'Common',probe_name,'FullyQualifiedName~MigrationSemantic04A02Tests'))
                env={'BESHSTAT_04A02_G3_CANDIDATES':str(self.run/'executions'/probe_name/'observations')}
                results.append(self.command(label+'-'+config+'-common-python',[sys.executable,'-B','-m','unittest','discover','-s','BESHStat.Excel.Common.Tests/src','-p','test_*.py','-v'],env=env))
            result=dict(lane=letter,seconds=time.perf_counter()-begin,results=results);save(self.run/(label+'-'+config+'-lane-'+letter+'.json'),result);return result
        with ThreadPoolExecutor(max_workers=4) as pool:lanes=list(pool.map(lane,'ABCD'))
        summary=dict(configuration=config,label=label,seconds=time.perf_counter()-start,lanes=lanes,source_unchanged=build['source_pins']==source_pins(),worker_limit=4,inner_parallelism=1,acceptance='Incomplete if any NOT_RUN; nonzero test exits remain failures')
        save(self.run/(label+'-'+config+'-matrix.json'),summary)
        return summary
    def serial(self,label):
        audit=ROOT/'scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll'
        results=[]
        for suffix,args in [('strict',['dotnet',str(audit),'--check','--strict']),('profile',[sys.executable,'-B','scripts/windows-migration-audit/owner_closeout.py','--profile','inventory-complete-with-approved-deferrals']),('docs',[sys.executable,'-B','-m','mkdocs','build','--config-file','BESHStatNG_Help_MkDocs/mkdocs.latest.yml','--site-dir',str(self.run/label/'site'),'--strict'])]:
            results.append(self.command(label+'-'+suffix,args))
        return results

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action',choices=['init','build','focused','matrix','discover','serial'])
    parser.add_argument('--run',type=Path)
    parser.add_argument('--configuration',choices=['Debug','Release'],default='Debug')
    parser.add_argument('--label',default='baseline')
    args=parser.parse_args()
    if not args.label.replace('-','').isalnum():raise ValueError('Unsafe label')
    if args.action=='init':
        run=require_private_run(args.run or ROOT/('.test-rationalization-'+uuid.uuid4().hex+'.tmp'))
        fresh(run);save(run/'entry.json',dict(base=BASE,head=git('rev-parse','HEAD'),main=git('rev-parse','main'),branch=git('branch','--show-current'),source_pins=source_pins(),owner_exclusivity='Explicit owner confirmation in active conversation',accepted_04a26=True,started_utc=datetime.now(timezone.utc).isoformat()))
        print(run);return
    runner=Runner(args.run)
    if args.action=='build':
        result=runner.build(args.configuration,args.label)
        if any(result['exit_codes']) or not result['source_unchanged']:raise SystemExit(1)
    elif args.action=='focused':
        if failed(runner.focused(args.configuration,args.label)):raise SystemExit(1)
    elif args.action=='matrix':
        result=runner.matrix(args.configuration,args.label)
        if failed(result):raise SystemExit(1)
    elif args.action=='discover':
        results=[runner.test(args.configuration,args.label,suite,args.label+'-'+args.configuration+'-discover-'+suite,discover=True) for suite in SUITES]
        if failed(results):raise SystemExit(1)
    else:
        if failed(runner.serial(args.label)):raise SystemExit(1)
if __name__=='__main__':main()
