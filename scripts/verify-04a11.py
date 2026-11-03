"""04A.11 offline successor harness; no historical/native writers or restore."""
import argparse
import importlib.util
from pathlib import Path
import shutil
import sys

ROOT=Path(__file__).resolve().parents[1]
spec=importlib.util.spec_from_file_location('verification10',ROOT/'scripts/verify-04a10.py')
prior=importlib.util.module_from_spec(spec);spec.loader.exec_module(prior)
base=prior.base
BASE='463aaa12a6342cedfa084e266d5a985461de9d3a'

def evidence(value):
    run=Path(value).resolve()
    if run.parent!=ROOT/'TestResults' or not run.name.startswith('04A-11-') or not (run/'start.json').is_file():raise ValueError('Fresh initialized 04A.11 evidence required')
    return run

prior.evidence=evidence
prior.prior.evidence=evidence
base.evidence=evidence
base.isolation.evidence=evidence
base.projection.__globals__['evidence']=evidence
exec(prior.prior.prior.final_source.replace('scripts/verify-04a08.py','scripts/verify-04a11.py'),base.__dict__)

def init(args):
    assert prior.prior.git('branch','--show-current')=='codex/04A-11'
    assert prior.prior.git('rev-parse','HEAD')==prior.prior.git('rev-parse','main')==BASE
    assert not prior.prior.git('diff','--cached','--name-only') and not prior.prior.git('diff','--name-only')
    run=Path(args.evidence).resolve()
    assert run.parent==ROOT/'TestResults' and run.name.startswith('04A-11-')
    assert shutil.disk_usage(ROOT).free>50*1024**3
    run.mkdir(exist_ok=False)
    entries=[]
    for p in (ROOT/'TestResults').rglob('*'):
        if p.is_file() and not p.is_relative_to(run):
            s=p.stat();entries.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev))
    base.isolation.emit(run/'historical-metadata.json',entries)
    names=set(prior.prior.git('ls-files').splitlines())|{p.relative_to(ROOT).as_posix() for p in (ROOT/'docs').rglob('*') if p.is_file()}
    pins={p:base.isolation.sha(ROOT/p) for p in sorted(names) if (ROOT/p).is_file()}
    base.isolation.emit(run/'baseline.json',dict(head=BASE,main=BASE,branch='codex/04A-11',source_pins=pins,status=prior.prior.git('status','--short','--untracked-files=all')))
    protected={p:h for p,h in pins.items() if p.startswith(('docs/','planning/','evidence/')) or p.endswith('VersionAttributes.vb')}
    base.isolation.emit(run/'start.json',dict(source_pins=protected,historical_files=len(entries),disk_free_bytes=shutil.disk_usage(ROOT).free,starting_head=BASE))
    for name in ['BESHstatNG/src/UI/UiGLM.vb','BESHstatNG/src/DM/WindowsRegressionRequestAdapter.vb','BESHStatNG.Test/BESHStatNG.Tests.vbproj','BESHStat.Core/src/RegModels/GLM.vb','BESHStat.Excel.Common/src/Analysis/LinearBinomialRegressionPolicy.vb','BESHStat.Excel.Common/src/Results/LinearBinomialReport.vb']:
        p=run/'preimages'/name;p.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(ROOT/name,p)
    out=ROOT/'evidence/04A11';out.mkdir(exist_ok=False)
    base.isolation.emit(out/'prechange-manifest.json',dict(base=BASE,branch='codex/04A-11',status=prior.prior.git('status','--short'),historical_files=len(entries),scope='Tracked files plus docs, SHA-256 before production edits; TestResults metadata only',source_pins=pins))
    print('Protected historical files',len(entries),'free GiB',round(shutil.disk_usage(ROOT).free/1024**3,2),flush=True)
    return 0

def focused(args):
    run=evidence(args.evidence)
    if base.command(run,args.name+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore']):return 1
    args.action='common';args.filter='FullyQualifiedName~Regression11'
    code=base.isolation.common(args)
    if code or not args.windows:return code
    code=base.windows_build(run,args.configuration,args.name)
    if code:return code
    args.action='legacy';args.name+='-windows';args.vstest=base.tools()[1]
    return base.isolation.common(args)

def capture(args):
    run=evidence(args.evidence)
    if args.windows:
        code=base.windows_build(run,args.configuration,args.name)
        if code:return code
        args.action='legacy';args.filter='FullyQualifiedName~Regression11Baselines';args.vstest=base.tools()[1]
        return base.isolation.common(args)
    return base.command(run,args.name,['dotnet','test','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore','--filter','FullyQualifiedName~Regression11Baselines','--logger','trx;LogFileName=capture.trx','--results-directory',str(run/args.name/'results')])

def docs(args):
    run=evidence(args.evidence)
    return base.command(run,args.name,[sys.executable,'-B','-m','mkdocs','build','--config-file','BESHStatNG_Help_MkDocs/mkdocs.latest.yml','--site-dir',str(run/args.name), '--strict'])

def managed(args):
    run=evidence(args.evidence);prefix=args.name
    code=base.command(run,prefix+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore'])
    if code:return code
    args.action='common';args.name=prefix+'-Common';args.filter=None
    common=base.isolation.common(args)
    code=base.windows_build(run,args.configuration,prefix)
    if code:return code
    args.action='legacy';args.name=prefix+'-Legacy';args.vstest=base.tools()[1]
    legacy=base.isolation.common(args)
    return max(common,legacy)

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('action',choices=['init','capture','focused','docs','managed','final','execute','preserve','historical'])
    p.add_argument('--evidence',required=True);p.add_argument('--configuration',choices=['Debug','Release'],default='Debug');p.add_argument('--name',default='focused-01')
    p.add_argument('--windows',action='store_true');p.add_argument('--legacy',action='store_true');p.add_argument('--vstest');p.add_argument('--candidate-name')
    a=p.parse_args()
    raise SystemExit(dict(init=init,capture=capture,focused=focused,docs=docs,managed=managed,final=base.final,execute=base.execute,preserve=base.isolation.preservation,historical=prior.prior.historical)[a.action](a))
