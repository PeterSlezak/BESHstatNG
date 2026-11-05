"""Offline 04A.24 build-once, prebuilt-test runner. Four bounded command lanes.

Original verifier unchanged. No restores, package changes, native Excel, browser,
R, signing or packaging. All writers are confined to a fresh batch run/sandbox.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import ctypes
from ctypes import wintypes
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import threading
import time
import xml.etree.ElementTree as ET
import importlib.util

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('capture24',ROOT/'scripts/capture-04a24-contract.py');c=importlib.util.module_from_spec(spec);spec.loader.exec_module(c)
RUN,OUT=c.RUN,c.OUT
LOCK=threading.Lock();ACTIVE={};PEAK=0
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def save(p,d):
    p.parent.mkdir(parents=True,exist_ok=True)
    with p.open('x',encoding='utf-8',newline='\n') as f:json.dump(d,f,ensure_ascii=False,allow_nan=False,indent=2)
def tools():
    vswhere=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    args=[str(vswhere),'-version','[18.0,19.0)','-products','*']
    msbuild=subprocess.check_output(args+['-requires','Microsoft.Component.MSBuild','-find',r'MSBuild\Current\Bin\MSBuild.exe'],text=True).splitlines()[0]
    vs=subprocess.check_output(args+['-property','installationPath'],text=True).splitlines()[0]
    return msbuild,str(Path(vs)/'Common7/IDE/Extensions/TestPlatform/vstest.console.exe')

class Memory(ctypes.Structure):
    _fields_=[('cb',wintypes.DWORD),('PageFaultCount',wintypes.DWORD)]+[(v,ctypes.c_size_t) for v in ['PeakWorkingSetSize','WorkingSetSize','QuotaPeakPagedPoolUsage','QuotaPagedPoolUsage','QuotaPeakNonPagedPoolUsage','QuotaNonPagedPoolUsage','PagefileUsage','PeakPagefileUsage']]
def root_metrics(pid):
    """OS process counters only; does not inspect or automate Excel/COM/UI."""
    k=ctypes.windll.kernel32;k.OpenProcess.restype=wintypes.HANDLE
    handle=k.OpenProcess(0x410,False,pid)
    if not handle:return None
    handle=wintypes.HANDLE(handle)
    try:
        memory=Memory();memory.cb=ctypes.sizeof(memory)
        ok=ctypes.windll.psapi.GetProcessMemoryInfo(handle,ctypes.byref(memory),memory.cb)
        times=[wintypes.FILETIME() for _ in range(4)]
        cpu=k.GetProcessTimes(handle,*(ctypes.byref(t) for t in times))
        seconds=sum((t.dwHighDateTime<<32)+t.dwLowDateTime for t in times[2:])/1e7 if cpu else None
        return dict(peak_root_working_set_bytes=int(memory.PeakWorkingSetSize) if ok else None,root_cpu_seconds=seconds)
    finally:k.CloseHandle(handle)

def command(name,args,cwd=ROOT,env=None):
    global PEAK
    folder=RUN/'commands'/name;folder.mkdir(parents=True,exist_ok=False)
    start=time.monotonic();metrics=None;aborted=False
    with (folder/'stdout.log').open('x',encoding='utf-8') as stdout,(folder/'stderr.log').open('x',encoding='utf-8') as stderr:
        process=subprocess.Popen([str(a) for a in args],cwd=cwd,env=env,stdout=stdout,stderr=stderr,creationflags=subprocess.CREATE_NO_WINDOW)
        with LOCK:
            ACTIVE[name]=process.pid;PEAK=max(PEAK,len(ACTIVE));assert len(ACTIVE)<=4
        try:
            while process.poll() is None:
                m=root_metrics(process.pid)
                if m:metrics=m
                if time.monotonic()-start>5400:
                    # Popen still owns this live PID; terminate only its process
                    # tree, wait/reap it, and retain an incomplete/nonpass receipt.
                    aborted=True
                    subprocess.run(['taskkill','/PID',str(process.pid),'/T','/F'],stdout=stderr,stderr=stderr,creationflags=subprocess.CREATE_NO_WINDOW)
                    process.wait()
                    break
                time.sleep(.25)
            code=process.returncode
        finally:
            with LOCK:ACTIVE.pop(name,None)
    record=dict(name=name,command=[str(a) for a in args],cwd=str(cwd),exit_code=code,completed=not aborted,seconds=time.monotonic()-start,pid=process.pid,metrics=metrics,runner_sha256=sha(Path(__file__)),stdout_sha256=sha(folder/'stdout.log'),stderr_sha256=sha(folder/'stderr.log'))
    save(folder/'command.json',record)
    print(name,code,round(record['seconds'],2),flush=True)
    return code

def build(config,name,all_projects=False):
    assert not ACTIVE
    projects=['BESHStat.Core/BESHStat.Core.vbproj','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj']
    if all_projects:projects+=['BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj','BESHStat.OfficeAddin/BESHStat.OfficeAddin.csproj','BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj','BESHStat.WasmSmoke/BESHStat.WasmSmoke.csproj']
    for index,p in enumerate(projects):
        code=command(name+'-'+str(index),['dotnet','build',p,'-c',config,'--no-restore','-m:4'])
        if code:return code
    msbuild,_=tools()
    for suffix,project,target in [('host','BESHstatNG/BESHStatNG.vbproj','/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory'),('legacy','BESHStatNG.Test/BESHStatNG.Tests.vbproj','/t:Build')]:
        code=command(name+'-'+suffix,[msbuild,project,target,'/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:PythonExe='+sys.executable,'/m:4','/v:minimal'])
        if code:return code
    if all_projects:
        parent=ROOT/'TestResults/04A-06-20261102-run01/projection';proof=json.loads((parent/'provenance.json').read_bytes());projection=RUN/'projection'
        if not projection.exists():
            projection.mkdir()
            for f,h in proof['generated_hashes'].items():assert sha(parent/f)==h;shutil.copyfile(parent/f,projection/f)
        output=RUN/'prepared'/config/'ControlMitigation';output.mkdir(parents=True,exist_ok=False)
        code=command(name+'-mitigation',[msbuild,'BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj','/t:Build','/m:4','/v:minimal','/p:Configuration='+config,'/p:ProjectionSourceDir='+str(projection),'/p:OutputPath='+str(output/'bin')+'\\','/p:IntermediateOutputPath='+str(output/'obj')+'\\'])
        if code:return code
    save(RUN/'builds'/(name+'.json'),dict(configuration=config,all_projects=all_projects,source_pins=compiled_pins(),seconds_note='Sum actual build command receipts; compiler commands sequential, /m:4.'))
    return 0

def compiled_pins():
    paths=set(c.git('ls-files').splitlines())|set(c.git('ls-files','--others','--exclude-standard').splitlines())
    return {p:sha(ROOT/p) for p in sorted(paths) if p.endswith(('.vb','.vbproj','.cs','.csproj')) and not p.startswith(('TestResults/','evidence/'))}

SOURCES={'Core':('BESHStat.Core.Tests','BESHStat.Core.Tests.dll','net10.0'),'Common':('BESHStat.Excel.Common.Tests','BESHStat.Excel.Common.Tests.dll','net10.0'),'Legacy':('BESHStatNG.Test','BESHStatNG.Test.dll','net48'),'OfficeAddin':('BESHStat.OfficeAddin/tests','OfficeAddin.Tests.dll','net10.0'),'OfficeExecution':('BESHStat.WasmSmoke/tests/Execution','Execution.Tests.dll','net10.0')}
def container(config,suite,name):
    execution=RUN/'executions'/name;assert not execution.exists()
    base=execution/'sandbox'/suite/'bin'/config/('net48' if suite in ['Legacy','ControlMitigation'] else 'net10.0')
    if suite=='ControlMitigation':source=RUN/'prepared'/config/suite/'bin';dll='BESHStatNG.Test.dll'
    else:
        project,dll,tfm=SOURCES[suite];source=ROOT/project/'bin'/config
        if tfm!='net48':source=source/tfm
    shutil.copytree(source,base)
    fixture_sources={}
    if suite in ['Core','Legacy']:
        fixtures=ROOT/('BESHStat.Core.Tests' if suite=='Core' else 'BESHStatNG.Test')/'TestData'
        for p in fixtures.rglob('*'):
            if not p.is_file():continue
            target=base/'TestData'/p.relative_to(fixtures)
            if not target.exists():target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,target)
            # Bin-local fixtures retain original lookup priority; missing
            # project-relative fixtures are copied, never regenerated.
            fixture_sources[p.relative_to(ROOT).as_posix()]=dict(sha256=sha(p),copied_sha256=sha(target))
            assert sha(p)==sha(target),p
    for relative in ['../../../../TestResults/Codex/Batch0205/Wire','../../../../TestResults/Codex/Batch0207']:
        assert (base/relative).resolve().is_relative_to(execution/'sandbox')
    copies={p.relative_to(base).as_posix():sha(p) for p in base.rglob('*') if p.is_file()}
    for p in source.rglob('*'):
        if p.is_file():assert sha(p)==copies[p.relative_to(source).as_posix()]
    save(execution/'copies.json',dict(source=str(source),copied=copies,fixture_sources=fixture_sources))
    settings=execution/'serial.runsettings';settings.write_text('<RunSettings><RunConfiguration><MaxCpuCount>1</MaxCpuCount></RunConfiguration><MSTest><Parallelize><Workers>1</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest></RunSettings>',encoding='utf-8')
    env=os.environ.copy()
    # Frozen C02 full-suite fixtures permit their optional writer only under
    # C02-named top-level paths. Preserve their assertions and leave that writer
    # unset, as the accepted earlier verifier does. The separate semantic-only
    # probe below explicitly opts in to its own 04A24 observation directory.
    env.pop('BESHSTAT_04A02_OBSERVATIONS',None);env.pop('BESHSTAT_04A03_OBSERVATIONS',None)
    env.update(BESHSTAT_04A02_SOURCE_ROOT=str(ROOT),BESHSTAT_KR_EXPORT_DIR=str(execution/'csv-exports'),PYTHONDONTWRITEBYTECODE='1')
    runner=[tools()[1]] if suite in ['Legacy','ControlMitigation'] else ['dotnet','vstest']
    args=runner+[str(base/dll),'/Settings:'+str(settings),'/Logger:trx;LogFileName='+suite+'.trx','/ResultsDirectory:'+str(execution/'results')]
    if suite=='ControlMitigation':args+=['/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common')]
    return execution,args,env

def test(config,suite,name,filter=None):
    execution,args,env=container(config,suite,name)
    if filter:args+=['/TestCaseFilter:'+filter]
    if filter and 'MigrationSemantic04A02Tests' in filter:env['BESHSTAT_04A02_OBSERVATIONS']=str(execution/'observations')
    code=command(name,args,env=env)
    trx=execution/'results'/(suite+'.trx')
    assert trx.exists(),'Missing TRX '+name
    count=len(ET.parse(trx).getroot().findall('.//{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}UnitTestResult'))
    assert count>0,'ZERO_TESTS: '+name
    save(execution/'execution.json',dict(configuration=config,suite=suite,filter=filter,exit_code=code,raw_count=count,trx_sha256=sha(trx),source_pins=compiled_pins()))
    return code

def focused(config,name,entry=False):
    code=build(config,name+'-build')
    if code:return code
    f='FullyQualifiedName~Spc22|FullyQualifiedName~Spc23' if entry else 'FullyQualifiedName~Spc24'
    codes=[test(config,'Core',name+'-Core','FullyQualifiedName~SpcMultivariateTests'),test(config,'Common',name+'-Common',f),test(config,'Legacy',name+'-Legacy',f)]
    return max(codes)

def matrix(config,name):
    assert compiled_pins()==json.loads((RUN/'builds'/('final-build-'+config+'.json')).read_bytes())['source_pins'],'Stale outputs: rebuild first'
    def lane(label):
        start=time.monotonic();codes=[]
        if label=='A':
            for suite in ['Core','Common']:codes.append(test(config,suite,name+'-'+suite))
        elif label=='B':
            for suite in ['Legacy','ControlMitigation']:codes.append(test(config,suite,name+'-'+suite))
        elif label=='C':
            for suite in ['OfficeAddin','OfficeExecution']:codes.append(test(config,suite,name+'-'+suite))
            for label2,path,args in [('office-check','BESHStat.OfficeAddin',['node','--check','wwwroot/js/officeHost.mjs']),('office-js','BESHStat.OfficeAddin',['node','--test','--test-concurrency=1','tests/*.test.mjs']),('resolver','.', ['node','--test','--test-concurrency=1','BESHStatNG_Help_MkDocs/tests/help-resolver.test.mjs']),('wasm-check','BESHStat.WasmSmoke',['node','--check','wwwroot/js/officeBridge.js']),('wasm-js','BESHStat.WasmSmoke',['node','--test','--test-concurrency=1'])]:codes.append(command(name+'-'+label2,args,cwd=ROOT/path))
        else:
            for label2,args in [('inventory',['tools/verify_stage304_inventory.py']),('catalog',['BESHstatNG/tools/validate_methods_catalog.py']),('metadata',['BESHstatNG/tools/generate_analysis_metadata.py','--check']),('resolver-check',['BESHStatNG_Help_MkDocs/generate_help_resolver_routes.py','--check']),('help-check',['BESHstatNG/tools/generate_help_links.py','--methods-yml','BESHStatNG_Help_MkDocs/docs/_meta/methods.yml','--project-dir','BESHstatNG','--check']),('office-audit',['BESHstatNG/tools/audit_transitional_officejs.py'])]:codes.append(command(name+'-'+label2,[sys.executable,'-B',*args]))
            for label2,path in [('tools','BESHstatNG/tools/tests'),('help','BESHStatNG_Help_MkDocs/tests'),('scripts','scripts/tests')]:codes.append(command(name+'-python-'+label2,[sys.executable,'-B','-m','unittest','discover','-s',path,'-p','test_*.py','-v']))
            codes.append(test(config,'Common',name+'-semantic','FullyQualifiedName~MigrationSemantic04A02Tests'))
            env=os.environ.copy();env['BESHSTAT_04A02_G3_CANDIDATES']=str(RUN/'executions'/(name+'-semantic')/'observations')
            codes.append(command(name+'-common-python',[sys.executable,'-B','-m','unittest','discover','-s','BESHStat.Excel.Common.Tests/src','-p','test_*.py','-v'],env=env))
        record=dict(lane=label,seconds=time.monotonic()-start,exit_codes=codes,completed=True);save(RUN/'lanes'/(name+'-'+label+'.json'),record);return record
    start=time.monotonic();records=[];errors=[]
    with ThreadPoolExecutor(max_workers=4) as pool:
        jobs={label:pool.submit(lane,label) for label in 'ABCD'}
        for label,job in jobs.items():
            try:records.append(job.result())
            except Exception as ex:errors.append(dict(lane=label,type=type(ex).__name__,message=str(ex)))
    save(RUN/'lanes'/(name+'-summary.json'),dict(configuration=config,seconds=time.monotonic()-start,lanes=records,errors=errors,max_active_command_roots=PEAK,worker_limit=4,resource_scope='Root-command OS peak working set and CPU counters where available; testhost/Node/MkDocs child memory not claimed measured. MaxCpuCount=1, MSTest Workers=1, Node concurrency=1; no fifth worker.',serial_lane_sum_estimate=sum(r['seconds'] for r in records),speedup_qualification='Measured concurrent wall clock versus sum of observed lane durations only; not a separate serial benchmark.'))
    assert compiled_pins()==json.loads((RUN/'builds'/('final-build-'+config+'.json')).read_bytes())['source_pins']
    return 1 if errors or any(any(v for v in r['exit_codes']) for r in records) else 0

def serial(name):
    codes=[]
    for suffix,args in [('strict',['dotnet','scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll','--check','--strict']),('profile',[sys.executable,'-B','scripts/windows-migration-audit/owner_closeout.py','--profile','inventory-complete-with-approved-deferrals']),('mkdocs',[sys.executable,'-B','-m','mkdocs','build','--config-file','BESHStatNG_Help_MkDocs/mkdocs.latest.yml','--site-dir',str(RUN/name/'site'),'--strict'])]:codes.append(command(name+'-'+suffix,args))
    return max(codes)

def portable(config,name):
    """Requalify only the affected full portable lane after pilot setup repair."""
    assert not ACTIVE
    assert compiled_pins()==json.loads((RUN/'builds'/('final-build-'+config+'.json')).read_bytes())['source_pins']
    codes=[test(config,suite,name+'-'+suite) for suite in ['Core','Common']]
    return max(codes)

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['entry','focused','build','matrix','serial','guards','portable']);p.add_argument('--configuration',choices=['Debug','Release'],default='Debug');p.add_argument('--name',required=True);a=p.parse_args()
    code=focused(a.configuration,a.name,True) if a.action=='entry' else focused(a.configuration,a.name) if a.action=='focused' else build(a.configuration,a.name,True) if a.action=='build' else matrix(a.configuration,a.name) if a.action=='matrix' else portable(a.configuration,a.name) if a.action=='portable' else command(a.name,[sys.executable,'-B','-m','unittest','discover','-s','scripts/tests','-p','test_04a24_*.py','-v']) if a.action=='guards' else serial(a.name)
    raise SystemExit(code)
