"""04A.06 isolated managed/source validation. No historical writers or native execution."""
import argparse
import importlib.util
import inspect
import json
from pathlib import Path
import hashlib
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('verification06base', ROOT / 'scripts/verify-04a05.py')
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)

def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT).decode().strip()

def evidence(value):
    run = Path(value).resolve()
    if run.parent != ROOT/'TestResults' or not run.name.startswith('04A-06-') or not (run/'start.json').is_file():
        raise ValueError('Fresh initialized 04A.06 evidence required')
    return run

base.evidence = evidence
base.isolation.evidence = evidence
final_source = inspect.getsource(base.final).replace('scripts/verify-04a05.py', 'scripts/verify-04a06.py')
final_source = final_source.replace('relative + "/" + args.configuration + "/results\'"', 'relative + "/" + args.name + "/" + args.configuration + "/results\'"')
exec(final_source, base.__dict__)

def projection(args):
    """Successor of the existing 61-case host-free control projection.
    Only its variance helper is replaced with the actual production delegation;
    the frozen parent projection and its source/byte pins stay untouched.
    """
    run=evidence(args.evidence)
    spec=importlib.util.spec_from_file_location('contract06',ROOT/'scripts/verify-04a06-contract.py')
    contract=importlib.util.module_from_spec(spec);spec.loader.exec_module(contract)
    before,after=contract.verify_extraction()
    previous=ROOT/'TestResults/04A-03-controls-20261102-run01/projection'
    record=json.loads((previous/'source-projection.json').read_bytes())
    pins=json.loads((run/'baseline.json').read_bytes())['source_pins']
    for path,digest in record['source_hashes'].items():
        assert pins[path]==digest, 'Parent input did not match before this batch'
        if path!='BESHstatNG/src/UI/UibyID.vb': assert base.isolation.sha(ROOT/path)==digest
    destination=run/'projection'
    if not destination.exists():
        destination.mkdir()
        for name,digest in record['generated_hashes'].items():
            assert base.isolation.sha(previous/name)==digest
            raw=(previous/name).read_bytes()
            if name=='Projection.vb':
                text=raw.decode('utf-8-sig').replace('\r\n','\n')
                methods=contract.members(text)
                assert methods['ComputeVarianceHomogeneity']==before['ComputeVarianceHomogeneity']
                text=text.replace(methods['ComputeVarianceHomogeneity'],after['GetHomogeneityMethods']+'\n\n'+after['ComputeVarianceHomogeneity'])
                raw=text.encode('utf-8')
            with (destination/name).open('xb') as stream: stream.write(raw)
        base.isolation.emit(destination/'provenance.json',dict(schema='04A06-successor-control-projection-v1',parent_source_projection_sha256=base.isolation.sha(previous/'source-projection.json'),
            parent_source_pins=record['source_hashes'],current_source_pins={p:base.isolation.sha(ROOT/p) for p in record['source_hashes']},
            generated_hashes={n:base.isolation.sha(destination/n) for n in record['generated_hashes']},
            exact_new_methods=['GetHomogeneityMethods','ComputeVarianceHomogeneity'],all_other_control_projection_sources_preserved=True,
            shipping_constructor_executed=False,native_executed=False,scope='Test-only projection; real initialized controls, never shown, no Excel connector. Actual shipping full constructor NOT executed.'))
    msbuild,vstest=base.tools()
    output=run/'projection-executions'/args.name;output.mkdir(parents=True,exist_ok=False)
    code=base.command(run,args.name+'-projection-build',[msbuild,'BESHStatNG.Test/ControlMitigation/ControlMitigation.Tests.vbproj','/t:Build','/m','/v:minimal',
        '/p:Configuration='+args.configuration,'/p:ProjectionSourceDir='+str(destination),'/p:OutputPath='+str(output/'bin')+'\\','/p:IntermediateOutputPath='+str(output/'obj')+'\\'])
    if code:return code
    return base.command(run,args.name+'-projection-tests',[vstest,str(output/'bin/BESHStatNG.Test.dll'),
        '/TestAdapterPath:'+str(ROOT/'BESHStatNG.Test/packages/MSTest.TestAdapter.2.2.10/build/_common'),'/Logger:console;verbosity=minimal',
        '/Logger:trx;LogFileName=ControlMitigation.trx','/ResultsDirectory:'+str(output/'results')])

base.projection=projection

def init(args):
    if git('branch','--show-current') != 'codex/04A-06' or git('rev-parse','HEAD') != '4a3d2e16b4d76a559427d596a424d382bcb0b915':
        raise ValueError('Unexpected baseline')
    if git('diff','--cached','--name-only') or git('diff','--name-only'):
        raise ValueError('Unexpected initial tracked changes')
    free = shutil.disk_usage(ROOT).free
    if free < 50*1024**3:
        raise ValueError('Disk below 50 GiB; owner review required')
    run = Path(args.evidence).resolve()
    if run.parent != ROOT/'TestResults' or not run.name.startswith('04A-06-'):
        raise ValueError('Unexpected evidence path')
    run.mkdir(exist_ok=False)
    entries=[]
    for path in (ROOT/'TestResults').rglob('*'):
        if path.is_file() and not path.is_relative_to(run):
            st=path.stat()
            entries.append(dict(path=path.relative_to(ROOT).as_posix(),bytes=st.st_size,mtime_ns=st.st_mtime_ns,ctime_ns=st.st_ctime_ns,ino=st.st_ino,dev=st.st_dev))
    base.isolation.emit(run/'historical-metadata.json', entries)
    names=set(git('ls-files').splitlines())
    pins={name:base.isolation.sha(ROOT/name) for name in sorted(names) if (ROOT/name).is_file()}
    for name in ['BESHstatNG/src/UI/UibyID.vb','BESHstatNG/BESHStatNG.vbproj','BESHStatNG.Test/BESHStatNG.Tests.vbproj']:
        target=run/'preimages'/name
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.copyfile(ROOT/name,target)
    base.isolation.emit(run/'baseline.json',dict(head=git('rev-parse','HEAD'),main=git('rev-parse','main'),branch=git('branch','--show-current'),status=git('status','--short','--untracked-files=all'),source_pins=pins))
    protected={p:h for p,h in pins.items() if p.startswith(('docs/','planning/')) or p.endswith('VersionAttributes.vb')}
    base.isolation.emit(run/'start.json',dict(source_pins=protected,historical_files=len(entries),disk_free_bytes=free,starting_head=git('rev-parse','HEAD')))
    print('Historical files:',len(entries),'free GiB:',round(free/1024**3,2),flush=True)
    return 0

def focused(args):
    run=evidence(args.evidence)
    if base.command(run,args.name+'-build',['dotnet','build','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj','-c',args.configuration,'--no-restore']): return 1
    args.action='common'
    args.filter='FullyQualifiedName~AssumptionReport06|FullyQualifiedName~UnivariateSelectionPolicy|FullyQualifiedName~PhysicalImporter27'
    code=base.isolation.common(args)
    if code: return code
    code=base.windows_build(run,args.configuration,args.name)
    if code: return code
    args.action='legacy'
    args.name+='-windows'
    args.filter='FullyQualifiedName~AssumptionReport06|FullyQualifiedName~UnivariateSelectionAdapter|FullyQualifiedName~PhysicalImporter27|FullyQualifiedName~MigrationRefEditBoundary09'
    args.vstest=base.tools()[1]
    return base.isolation.common(args)

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action',choices=['init','focused','final','execute','preserve','projection'])
    parser.add_argument('--evidence',required=True)
    parser.add_argument('--configuration',choices=['Debug','Release'],default='Debug')
    parser.add_argument('--name',default='focused-01')
    parser.add_argument('--windows',action='store_true')
    parser.add_argument('--legacy',action='store_true')
    parser.add_argument('--vstest')
    parser.add_argument('--candidate-name')
    args=parser.parse_args()
    raise SystemExit(dict(init=init,focused=focused,final=base.final,execute=base.execute,preserve=base.isolation.preservation,projection=projection)[args.action](args))
