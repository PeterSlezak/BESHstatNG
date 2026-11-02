"""Sequential C09 validation; no product source generation or historical writer reuse."""
import argparse
import difflib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import time
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('isolation09',ROOT/'scripts/verify-04a02-fixtures.py')
isolation=importlib.util.module_from_spec(spec);spec.loader.exec_module(isolation)


def command(run,name,args,env=None):
    start=time.monotonic()
    with (run/(name+'.log')).open('x',encoding='utf-8') as log:
        result=subprocess.run(args,cwd=ROOT,env=env,stdout=log,stderr=subprocess.STDOUT)
    with (run/(name+'-command.json')).open('x') as f:
        json.dump(dict(command=args,exit_code=result.returncode,seconds=time.monotonic()-start),f,indent=2)
    print(name,result.returncode,flush=True)
    if result.returncode:print((run/(name+'.log')).read_text(encoding='utf-8',errors='replace')[-4000:])
    return result.returncode


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--evidence',required=True)
    p.add_argument('--configuration',choices=['Debug','Release'],required=True)
    p.add_argument('--focused',action='store_true');a=p.parse_args()
    run=isolation.evidence(a.evidence);config=a.configuration;name='c9D' if config=='Debug' else 'c9R'
    if a.focused:
        env=os.environ.copy();env['BESHSTAT_04A02_G3_CANDIDATES']=str(run/'executions'/(name+'-Common')/'observations')
        files=['scripts/tests/test_04a02_checkpoint09.py','scripts/tests/test_04a02_checkpoint08.py',
               'scripts/tests/test_04a02_checkpoint08_integration.py','scripts/tests/test_04a02_checkpoint08_dossier.py',
               'BESHStat.Excel.Common.Tests/src/test_checkpoint07_contracts.py',
               'BESHStat.Excel.Common.Tests/src/test_04a02_source_specs.py',
               'BESHStat.Excel.Common.Tests/src/test_capture_contract04a02.py',
               'BESHStat.Excel.Common.Tests/src/test_investigation06.py']
        for i,file in enumerate(files):
            code=command(run,'focused-'+config+'-'+str(i),[sys.executable,'-B',file],env)
            if code:return code
        return 0
    original=(ROOT/'scripts/verify-beshstatng.ps1').read_text()
    pins=json.loads((run/'start.json').read_bytes())['source_pins']
    if isolation.sha(ROOT/'scripts/verify-beshstatng.ps1')!=pins['scripts/verify-beshstatng.ps1']:raise ValueError('Verifier drift')
    transformed=isolation.transform_script(original,run,config,name)
    anchor=isolation.SCRIPT_ANCHORS['legacy-build']
    if transformed.count(anchor)!=1:raise ValueError('Legacy build anchor drift')
    # All production Windows sources are unchanged and their C08 assembly hashes
    # are separately checked. Rebuild the legacy TEST project with cached product
    # references; do not run shipping GenerateVersionAttributes or Help generators.
    replacement=anchor[:-1]+", '/p:BuildProjectReferences=false')"
    transformed=transformed.replace(anchor,replacement)
    target=run/config/name/'verify-source-preserved.ps1';target.parent.mkdir(parents=True)
    with target.open('x',encoding='utf-8',newline='\n') as f:f.write(transformed)
    with target.with_suffix('.provenance.json').open('x') as f:json.dump(dict(source_sha256=isolation.sha(ROOT/'scripts/verify-beshstatng.ps1'),
        execution_sha256=isolation.sha(target),diff=''.join(difflib.unified_diff(original.splitlines(True),transformed.splitlines(True))),
        scope='Full test project rebuild; cached unchanged C08 shipping Windows references; no product generation or signing target.'),f,indent=2)
    args=['powershell','-NoProfile','-ExecutionPolicy','Bypass','-File',str(target),'-Legacy','-Wasm','-Js','-Python',
          '-VisualStudio','2026','-PythonExe',sys.executable,'-Configuration',config]
    code=command(run,name+'-full',args)
    if code:return code
    old='o9d' if config=='Debug' else 'o9r'
    code=command(run,name+'-old-seams',[sys.executable,'-B','BESHStatNG.Test/C07.HostFreeSeamRunner.py','--evidence',str(run),
                 '--configuration',config,'--name',old])
    if code:return code
    if config=='Release':return command(run,name+'-proposed-seams',[sys.executable,'-B','BESHStatNG.Test/C09.HostFreeSeamRunner.py',
        '--evidence',str(run),'--configuration',config,'--name','s9r'])
    return 0


if __name__=='__main__':raise SystemExit(main())
