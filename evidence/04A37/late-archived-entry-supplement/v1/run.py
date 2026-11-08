"""Separate Framework reference/current processes, append-only captures and real receipts."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,importlib.util,json,os,shutil,subprocess,sys
ROOT=Path(__file__).resolve().parents[4];HERE=Path(__file__).resolve().parent
PRIVATE=ROOT/'.test-rationalization-04A37.tmp';LATE=PRIVATE/'late-v1'
spec=importlib.util.spec_from_file_location('accepted_runner_late37',ROOT/'scripts/test_suite_rationalization.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(p):
    p=Path(p);b=p.read_bytes();return dict(path=p.relative_to(ROOT).as_posix(),bytes=len(b),sha256=sha(b))
def save(name,value):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,ensure_ascii=False,indent=2);f.write('\n')
def main():
    mode,config=sys.argv[1:3];assert mode in ['original','current'] and config in ['Debug','Release']
    if mode=='current':
        seal=json.loads((HERE/'reference-seal.json').read_bytes())
        assert seal['provenance']=='CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY'
        for row in seal['files']:assert pin(ROOT/row['path'])==row
    out=LATE/mode/config;out.mkdir(parents=True,exist_ok=False)
    dep=PRIVATE/'builds'/('entry' if mode=='original' else 'final')/config/'legacy'
    shutil.copy2(dep/'BESHStat.Core.dll',out/'BESHStat.Core.dll')
    if mode=='current':shutil.copy2(dep/'BESHStat.Excel.Common.dll',out/'BESHStat.Excel.Common.dll')
    locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
    find=lambda name:subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/'+name],text=True).splitlines()[0]
    vbc,csc=find('vbc.exe'),find('csc.exe')
    facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
    references=['/reference:'+str(out/'BESHStat.Core.dll'),'/reference:'+str(facade),'/reference:System.Core.dll']
    if mode=='current':references.append('/reference:'+str(out/'BESHStat.Excel.Common.dll'))
    vb=HERE/('archived-source/Original37.vb' if mode=='original' else 'current-source/Current37.vb')
    sources=[str(vb)]+([str(HERE/'ArchivedLateExpressions37.vb')] if mode=='original' else [])
    runner=m.Runner(PRIVATE);tag='late-v1-'+mode+'-'+config
    r=runner.command(tag+'-vbc',[vbc,'/nologo','/target:library','/optioncompare:text','/optimize'+('+' if config=='Release' else '-'),'/out:'+str(out/'Prefixes37.dll'),*references,*sources])
    if r['exit_code']:sys.exit(r['exit_code'])
    r=runner.command(tag+'-csc',[csc,'/nologo','/target:exe','/optimize'+('+' if config=='Release' else '-'),'/out:'+str(out/'Harness37.exe'),'/reference:'+str(out/'Prefixes37.dll'),'/reference:System.Web.Extensions.dll',*references,*(['/define:CURRENT'] if mode=='current' else []),str(HERE/'Harness37.cs')])
    if r['exit_code']:sys.exit(r['exit_code'])
    capture=HERE/(mode+'-'+config+'.tsv');receipt=HERE/(mode+'-'+config+'-execution.json')
    r=runner.command(tag+'-capture',[str(out/'Harness37.exe'),str(HERE/'cases.jsonl'),str(capture),str(receipt)])
    if r['exit_code']:sys.exit(r['exit_code'])
    save(mode+'-'+config+'-authority.json',dict(provenance='CAPTURED_AFTER_INITIAL_IMPLEMENTATION_FROM_PINNED_ARCHIVED_ENTRY',configuration=config,mode=mode,
        core=dict(bytes=(out/'BESHStat.Core.dll').stat().st_size,sha256=sha((out/'BESHStat.Core.dll').read_bytes())),
        harness=dict(bytes=(out/'Harness37.exe').stat().st_size,sha256=sha((out/'Harness37.exe').read_bytes())),
        prefix_assembly=dict(bytes=(out/'Prefixes37.dll').stat().st_size,sha256=sha((out/'Prefixes37.dll').read_bytes())),
        capture=pin(capture),execution=pin(receipt),compiler='Installed VS18 Roslyn Framework vbc/csc; exact flags and output hashes retained in isolated command receipts.',
        bindings='Reference loads entry Core only and asserts new37 types and Common absent; current loads final Core/Common and compiled current shipping prefixes.',
        native='NOT_RUN',scientific='NOT_GOLDEN'))
    print('Completed',mode,config,'capture; no live Excel')
if __name__=='__main__':main()
