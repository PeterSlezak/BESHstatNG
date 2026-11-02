"""Compile standalone .NET48 N1 helper and execute only its nonnative serializer self-test."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[2]


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--evidence',type=Path,required=True)
    p.add_argument('--name',required=True);a=p.parse_args()
    work=a.evidence.absolute()/a.name;work.mkdir()
    where=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    vs=Path(subprocess.check_output([str(where),'-version','[18.0,19.0)','-products','*','-property','installationPath'],text=True).splitlines()[0])
    framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
    names=['mscorlib','System','System.Core','Microsoft.CSharp','System.Web.Extensions','WindowsBase','UIAutomationClient','UIAutomationTypes']
    references=[]
    for name in names:
        found=list(framework.rglob(name+'.dll'))
        if not found:raise ValueError('Cached framework reference missing: '+name)
        references.append(found[0])
    source=ROOT/'scripts/04a02/native_n1_c09.cs';exe=work/'NativeN1.exe'
    command=[str(vs/'MSBuild/Current/Bin/Roslyn/csc.exe'),'/noconfig','/nologo','/nostdlib+',
             '/target:exe','/out:'+str(exe)]+['/reference:'+str(r) for r in references]+[str(source)]
    with (work/'compile.log').open('x') as log:result=subprocess.run(command,cwd=ROOT,stdout=log,stderr=subprocess.STDOUT)
    record=dict(command=command,exit_code=result.returncode,source_sha256=hashlib.sha256(source.read_bytes()).hexdigest())
    with (work/'compile.json').open('x') as f:json.dump(record,f,indent=2)
    if result.returncode:print((work/'compile.log').read_text());return result.returncode
    command=[str(exe),'--self-test']
    with (work/'self-test.log').open('x') as log:result=subprocess.run(command,cwd=ROOT,stdout=log,stderr=subprocess.STDOUT,timeout=15)
    with (work/'self-test.json').open('x') as f:json.dump(dict(command=command,exit_code=result.returncode,
        executable_sha256=hashlib.sha256(exe.read_bytes()).hexdigest(),status='TESTED_HOST_FREE',live_calls=0),f,indent=2)
    print((work/'self-test.log').read_text());return result.returncode


if __name__=='__main__':raise SystemExit(main())
