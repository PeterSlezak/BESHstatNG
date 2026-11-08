"""Compile extension fixture only; use the same sealed original/current prefix assemblies."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,importlib.util,json,os,shutil,subprocess,sys
ROOT=Path(__file__).resolve().parents[4];HERE=Path(__file__).resolve().parent;EXT=HERE/'label-extension-v1';PRIVATE=ROOT/'.test-rationalization-04A37.tmp'
spec=importlib.util.spec_from_file_location('accepted_runner_labels37',ROOT/'scripts/test_suite_rationalization.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m)
def pin(p):
    p=Path(p);b=p.read_bytes();return dict(path=p.relative_to(ROOT).as_posix(),bytes=len(b),sha256=hashlib.sha256(b).hexdigest())
mode,config=sys.argv[1:3];assert mode in ['original','current'] and config in ['Debug','Release']
if mode=='current':
    seal=json.loads((EXT/'reference-seal.json').read_bytes())
    for r in seal['files']:assert pin(ROOT/r['path'])==r
base=PRIVATE/'late-v1'/mode/config
out=PRIVATE/'late-labels-v1'/mode/config;out.mkdir(parents=True,exist_ok=False)
for name in ['BESHStat.Core.dll','Prefixes37.dll']+(['BESHStat.Excel.Common.dll'] if mode=='current' else []):shutil.copy2(base/name,out/name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
facade=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll'
args=[csc,'/nologo','/target:exe','/optimize'+('+' if config=='Release' else '-'),'/out:'+str(out/'Harness37.exe'),'/reference:'+str(out/'BESHStat.Core.dll'),'/reference:'+str(out/'Prefixes37.dll'),'/reference:'+str(facade),'/reference:System.Core.dll','/reference:System.Web.Extensions.dll']
if mode=='current':args+=['/define:CURRENT','/reference:'+str(out/'BESHStat.Excel.Common.dll')]
args+=[str(EXT/'Harness37.cs')]
runner=m.Runner(PRIVATE);label='late-labels-v1-'+mode+'-'+config
r=runner.command(label+'-csc',args)
if r['exit_code']:sys.exit(r['exit_code'])
r=runner.command(label+'-capture',[str(out/'Harness37.exe'),str(EXT/'cases.jsonl'),str(EXT/(mode+'-'+config+'.tsv')),str(EXT/(mode+'-'+config+'-execution.json'))])
sys.exit(r['exit_code'])
