"""04A.08 isolated validation; preserves all historical evidence and never launches Excel."""
import argparse
import importlib.util
import inspect
import json
from pathlib import Path
import shutil
import subprocess
import re

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('verification07base', ROOT/'scripts/verify-04a07.py')
prior = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prior)
base = prior.base
BASE = 'b48877406c3c185a992d5dd5c42f951611a52569'

def git(*args):
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode().strip()

def evidence(value):
    run=Path(value).resolve()
    if run.parent != ROOT/'TestResults' or not run.name.startswith('04A-08-') or not (run/'start.json').is_file():
        raise ValueError('Fresh initialized 04A.08 evidence required')
    return run

base.evidence=evidence
base.isolation.evidence=evidence
prior.evidence=evidence
final_source=prior.final_source.replace('scripts/verify-04a07.py','scripts/verify-04a08.py')
exec(final_source,base.__dict__)
base.projection=prior.projection

def init(args):
    assert git('branch','--show-current')=='codex/04A-08'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--cached','--name-only') and not git('diff','--name-only')
    run=Path(args.evidence).resolve()
    assert run.parent==ROOT/'TestResults' and run.name.startswith('04A-08-')
    run.mkdir(exist_ok=False)
    entries=[]
    for p in (ROOT/'TestResults').rglob('*'):
        if p.is_file() and not p.is_relative_to(run):
            s=p.stat()
            entries.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=s.st_size,mtime_ns=s.st_mtime_ns,ctime_ns=s.st_ctime_ns,ino=s.st_ino,dev=s.st_dev))
    base.isolation.emit(run/'historical-metadata.json',entries)
    names=set(git('ls-files').splitlines())|{p.relative_to(ROOT).as_posix() for p in (ROOT/'docs').rglob('*') if p.is_file()}
    pins={p:base.isolation.sha(ROOT/p) for p in sorted(names) if (ROOT/p).is_file()}
    base.isolation.emit(run/'baseline.json',dict(head=BASE,main=git('rev-parse','main'),branch=git('branch','--show-current'),source_pins=pins,status=git('status','--short','--untracked-files=all')))
    protected={p:h for p,h in pins.items() if p.startswith(('docs/','planning/')) or p.endswith('VersionAttributes.vb')}
    base.isolation.emit(run/'start.json',dict(source_pins=protected,historical_files=len(entries),disk_free_bytes=shutil.disk_usage(ROOT).free,starting_head=BASE))
    controller='BESHstatNG/src/UI/RegressionEffectsController.vb'
    text=(ROOT/controller).read_text(encoding='utf-8-sig')
    target=run/'preimages'/controller;target.parent.mkdir(parents=True);target.write_bytes((ROOT/controller).read_bytes())
    # Exact source-derived executable oracle; only dialog presentation replaced by a collector.
    oracle=text.replace('Friend Class RegressionEffectsController','Friend Class LegacyRegressionEffects08')
    oracle=oracle.replace('MsgBox(', 'RecordMessage(')
    oracle=oracle.replace('End Class', '''    Public ReadOnly Messages As New List(Of String)
    Public ReadOnly MessageModels As New List(Of String)
    Private Sub RecordMessage(message As String, style As Object, title As String)
        Messages.Add(message)
        MessageModels.Add(ExcelCommon.RegressionEffectSerialization.Serialize(ExcelCommon.RegressionEffectModel.FromProjection(
            _selectedEffects.Items.Cast(Of String)().ToArray(), _termSpecs)))
    End Sub
End Class''')
    with (ROOT/'BESHStatNG.Test/src/LegacyRegressionEffects08.vb').open('x',encoding='utf-8',newline='\n') as f:f.write(oracle)
    out=ROOT/'docs/architecture/04A08';out.mkdir(exist_ok=False)
    members={m[2]:dict(sha256=__import__('hashlib').sha256(m[0].encode()).hexdigest(),body=m[0]) for m in re.finditer(r'^    (?:Public|Private) (Sub|Function) (\w+)\b.*?^    End \1',text,re.M|re.S)}
    base.isolation.emit(out/'baseline.json',dict(base=BASE,source_pins=pins,members=members,oracle_sha256=base.isolation.sha(ROOT/'BESHStatNG.Test/src/LegacyRegressionEffects08.vb'),provenance='Pre-edit source-derived executable characterization; NOT native and NOT_GOLDEN'))
    print('Protected historical files',len(entries),'free GiB',round(shutil.disk_usage(ROOT).free/1024**3,2),flush=True)
    return 0

def focused(args):
    run=evidence(args.evidence)
    if base.command(run,args.name+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore']):return 1
    args.action='common';args.filter='FullyQualifiedName~RegressionEffectAuthoring08|FullyQualifiedName~ModelInputCompiler'
    code=base.isolation.common(args)
    if code:return code
    code=base.windows_build(run,args.configuration,args.name)
    if code:return code
    args.action='legacy';args.name+='-windows';args.filter='FullyQualifiedName~RegressionEffects08'
    args.vstest=base.tools()[1]
    return base.isolation.common(args)

def managed(args):
    """Refresh all managed suites after canonical metadata hardening; JS/Python sources unchanged."""
    run=evidence(args.evidence)
    prefix=args.name
    codes=[]
    for suite,project in [('Core','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj'),('OfficeAddin','BESHStat.OfficeAddin/tests/OfficeAddin.Tests.csproj'),('OfficeExecution','BESHStat.WasmSmoke/tests/Execution/Execution.Tests.csproj')]:
        codes.append(base.command(run,prefix+'-'+suite,['dotnet','test',project,'-c',args.configuration,'--no-restore','--logger','trx;LogFileName='+suite+'.trx','--results-directory',str(run/prefix/'results')]))
    build=base.command(run,prefix+'-common-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore'])
    codes.append(build)
    if not build:
        args.action='common';args.name=prefix+'-Common';args.filter=None
        codes.append(base.isolation.common(args))
        win=base.windows_build(run,args.configuration,prefix);codes.append(win)
        if not win:
            args.action='legacy';args.name=prefix+'-Legacy';args.vstest=base.tools()[1]
            codes.append(base.isolation.common(args))
    args.name=prefix
    codes.append(prior.projection(args))
    return max(codes)

def authoring(args):
    """Full Common/Windows refresh after the final newline-identity serialization edge check."""
    run=evidence(args.evidence);prefix=args.name;codes=[]
    build=base.command(run,prefix+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore'])
    if build:return build
    args.action='common';args.filter=None;args.name=prefix+'-Common'
    codes.append(base.isolation.common(args))
    build=base.windows_build(run,args.configuration,prefix);codes.append(build)
    if not build:
        args.action='legacy';args.name=prefix+'-Legacy';args.vstest=base.tools()[1]
        codes.append(base.isolation.common(args))
    return max(codes)

if __name__=='__main__':
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('action',choices=['init','focused','final','execute','preserve','managed','authoring'])
    p.add_argument('--evidence',required=True)
    p.add_argument('--configuration',choices=['Debug','Release'],default='Debug')
    p.add_argument('--name',default='focused-01')
    p.add_argument('--windows',action='store_true');p.add_argument('--legacy',action='store_true')
    p.add_argument('--vstest');p.add_argument('--candidate-name')
    a=p.parse_args()
    raise SystemExit(dict(init=init,focused=focused,final=base.final,execute=base.execute,preserve=base.isolation.preservation,managed=managed,authoring=authoring)[a.action](a))
