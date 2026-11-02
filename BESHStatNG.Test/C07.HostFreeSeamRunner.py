"""Compile a NEW isolated .NET48 friend test assembly using cached VS references.
Never edits the original legacy project, rebuilds shipping code or runs a form.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--configuration', choices=['Debug', 'Release'], required=True)
    parser.add_argument('--name', required=True)
    args = parser.parse_args()
    run = args.evidence.resolve()
    if run.parent != ROOT/'TestResults' or not (run/'start.json').is_file() or not (run/'historical-metadata.json').is_file():
        raise ValueError('new protected preflight root required')
    if not args.name.isalnum() or len(args.name) > 6: raise ValueError('short unique execution name required')
    work = run/args.name
    work.mkdir(exist_ok=False)
    base = work/'bin'
    base.mkdir(exist_ok=False)
    source = ROOT/'BESHStatNG.Test/bin'/args.configuration
    copied = []
    # Exclude the old test assembly: the new compiler output must never overwrite
    # even a newly retained copied binary. Other dependencies/TestData are hashed.
    for p in sorted(source.iterdir()):
        if p.is_file() and p.suffix.lower() in ('.dll', '.config') and not p.name.startswith('BESHStatNG.Test.'):
            target = base/p.name
            with target.open('xb') as stream: stream.write(p.read_bytes())
            if sha(target) != sha(p): raise ValueError('dependency copy drift')
            copied.append(dict(path=p.relative_to(ROOT).as_posix(),target=target.relative_to(run).as_posix(),sha256=sha(target)))
    shutil.copytree(ROOT/'BESHStatNG.Test/TestData',base/'TestData')
    for p in sorted((ROOT/'BESHStatNG.Test/TestData').rglob('*')):
        if p.is_file():
            target=base/'TestData'/p.relative_to(ROOT/'BESHStatNG.Test/TestData')
            if sha(target)!=sha(p): raise ValueError('TestData drift')
            copied.append(dict(path=p.relative_to(ROOT).as_posix(),target=target.relative_to(run).as_posix(),sha256=sha(target)))
    vswhere = Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    roots = subprocess.check_output([str(vswhere),'-version','[18.0,19.0)','-products','*','-property','installationPath'],text=True).splitlines()
    if not roots: raise ValueError('VS2026 not found; no installation authorized')
    vs = Path(roots[0]); compiler=vs/'MSBuild/Current/Bin/Roslyn/vbc.exe'
    vstest=vs/'Common7/IDE/Extensions/TestPlatform/vstest.console.exe'
    framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
    refs=[p for p in framework.glob('*.dll') if p.name not in
          ('System.EnterpriseServices.Wrapper.dll','System.EnterpriseServices.Thunk.dll')]+list((framework/'Facades').glob('*.dll'))
    for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','Microsoft.VisualStudio.TestPlatform.TestFramework.dll','Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions.dll','ExcelDna.Integration.dll']:
        path=base/name
        if not path.is_file(): raise ValueError('missing compiled/cached reference '+name)
        refs.append(path)
    src=ROOT/'BESHStatNG.Test/src/MigrationRefEditBoundary07Tests.vb'
    options=['/nologo','/target:library','/optionstrict+','/rootnamespace:BESHStatNG.Test',
             '/sdkpath:'+str(framework),'/out:'+str(base/'BESHStatNG.Test.dll')]
    with (work/'compile.rsp').open('x',encoding='utf-8') as f:
        for option in options: f.write('"'+option+'"\n')
        for p in refs: f.write('/reference:"'+str(p)+'"\n')
        f.write('"'+str(src)+'"\n')
    commands=[]
    command=[str(compiler),'/noconfig','@'+str(work/'compile.rsp')]
    with (work/'build.log').open('x',encoding='utf-8') as log:
        result=subprocess.run(command,cwd=work,stdout=log,stderr=subprocess.STDOUT)
    commands.append(dict(command=command,exit_code=result.returncode))
    if result.returncode:
        with (work/'execution-failure.json').open('x',encoding='utf-8') as f:
            json.dump(dict(commands=commands,copied=copied,source_sha256=sha(src)),f,indent=2)
        print((work/'build.log').read_text()); return result.returncode
    results=work/'results'; results.mkdir(exist_ok=False)
    command=[str(vstest),str(base/'BESHStatNG.Test.dll'),'/Logger:trx;LogFileName=Seams.trx','/ResultsDirectory:'+str(results)]
    env=os.environ.copy();env['BESHSTAT_KR_EXPORT_DIR']=str(work/'csv');env['BESHSTAT_04A02_SOURCE_ROOT']=str(ROOT)
    with (work/'test.log').open('x',encoding='utf-8') as log:
        result=subprocess.run(command,cwd=work,env=env,stdout=log,stderr=subprocess.STDOUT)
    commands.append(dict(command=command,exit_code=result.returncode))
    with (work/'execution.json').open('x',encoding='utf-8') as f:
        json.dump(dict(commands=commands,source_sha256=sha(src),assembly_sha256=sha(base/'BESHStatNG.Test.dll'),copied=copied,scope='ACTUAL_WINDOWS_INJECTABLE_SEAMS_NO_FORM_OR_COM_ACTIVATION'),f,indent=2)
    print((work/'test.log').read_text())
    return result.returncode


if __name__ == '__main__':
    raise SystemExit(main())
