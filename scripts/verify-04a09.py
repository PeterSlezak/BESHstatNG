"""04A.09 incremental, offline validation. Never writes historical results or invokes Excel."""
import argparse
import importlib.util
import inspect
import json
from pathlib import Path
import re
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('verification08', ROOT/'scripts/verify-04a08.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
base = prior.base
BASE = 'c25c6954d4e2948e728eebf395970d7ae635d6b6'

def git(*args):
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()

def evidence(value):
    run=Path(value).resolve()
    if run.parent != ROOT/'TestResults' or not run.name.startswith('04A-09-') or not (run/'start.json').is_file():
        raise ValueError('Fresh initialized 04A.09 evidence required')
    return run

base.evidence=evidence
base.isolation.evidence=evidence
prior.evidence=evidence
exec(prior.final_source.replace('scripts/verify-04a08.py','scripts/verify-04a09.py'),base.__dict__)
base.projection=prior.base.projection
base.projection.__globals__['evidence']=evidence

def init(args):
    assert git('branch','--show-current')=='codex/04A-09'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--name-only')
    run=Path(args.evidence).resolve()
    assert run.parent==ROOT/'TestResults' and run.name.startswith('04A-09-')
    assert shutil.disk_usage(ROOT).free > 50*1024**3
    run.mkdir(exist_ok=False)
    entries=[]
    for p in (ROOT/'TestResults').rglob('*'):
        if p.is_file() and not p.is_relative_to(run):
            s=p.stat()
            entries.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev))
    base.isolation.emit(run/'historical-metadata.json',entries)
    names=set(git('ls-files').splitlines())|{p.relative_to(ROOT).as_posix() for p in (ROOT/'docs').rglob('*') if p.is_file()}
    pins={p:base.isolation.sha(ROOT/p) for p in sorted(names) if (ROOT/p).is_file()}
    base.isolation.emit(run/'baseline.json',dict(head=BASE,main=BASE,branch=git('branch','--show-current'),source_pins=pins,status=git('status','--short','--untracked-files=all')))
    protected={p:h for p,h in pins.items() if p.startswith(('docs/','planning/','evidence/')) or p.endswith('VersionAttributes.vb')}
    base.isolation.emit(run/'start.json',dict(source_pins=protected,historical_files=len(entries),disk_free_bytes=shutil.disk_usage(ROOT).free,starting_head=BASE))
    for name in ['BESHstatNG/src/UI/UiGLM.vb','BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        p=run/'preimages'/name;p.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/name,p)
    print('Protected historical files',len(entries),'free GiB',round(shutil.disk_usage(ROOT).free/1024**3,2),flush=True)
    return 0

def focused(args):
    run=evidence(args.evidence)
    if base.command(run,args.name+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore']):return 1
    args.action='common';args.filter='FullyQualifiedName~Regression09'
    code=base.isolation.common(args)
    if code or not args.windows:return code
    code=base.windows_build(run,args.configuration,args.name)
    if code:return code
    args.action='legacy';args.name+='-windows';args.filter='FullyQualifiedName~Regression09';args.vstest=base.tools()[1]
    return base.isolation.common(args)

def baseline48(args):
    evidence(args.evidence)
    args.action='legacy';args.filter='FullyQualifiedName~Regression09Baselines';args.vstest=base.tools()[1]
    return base.isolation.common(args)

def historical(args):
    run=evidence(args.evidence)
    codes=[]
    for name,command in [
        ('toolchain-dotnet',['dotnet','--info']),
        ('historical-strict',['dotnet','scripts/windows-migration-audit/bin/Debug/net10.0/WindowsMigrationAudit.dll','--check','--strict']),
        ('historical-profile',[__import__('sys').executable,'-B','scripts/windows-migration-audit/owner_closeout.py','--profile','inventory-complete-with-approved-deferrals']),
    ]:
        codes.append(base.command(run,name,command))
    return max(codes)

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('action',choices=['init','focused','final','execute','preserve','managed','baseline48','historical'])
    p.add_argument('--evidence',required=True)
    p.add_argument('--configuration',choices=['Debug','Release'],default='Debug')
    p.add_argument('--name',default='focused-01')
    p.add_argument('--windows',action='store_true');p.add_argument('--legacy',action='store_true')
    p.add_argument('--vstest');p.add_argument('--candidate-name')
    a=p.parse_args()
    raise SystemExit(dict(init=init,focused=focused,final=base.final,execute=base.execute,preserve=base.isolation.preservation,managed=prior.managed,baseline48=baseline48,historical=historical)[a.action](a))
