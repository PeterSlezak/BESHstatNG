from common43 import *
import shutil
lock();config=sys.argv[1];target=PRIVATE/('policy-simulation-'+config+'-v1');target.mkdir();lib=PRIVATE/'compiled'/config/'lib'
for p in lib.glob('*.dll'):shutil.copy2(p,target/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll','Facades/netstandard.dll']]+[lib/'BESHStat.Excel.Common.dll',lib/'BESHStat.Core.dll'];exe=target/'Policy43.exe'
r=command('policy-'+config+'-compile-v1',[csc,'/nologo','/target:exe','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(HERE/'fixtures/Policy43.cs')]);assert r['exit_code']==0
r=command('policy-'+config+'-execute-v1',[str(exe),str(target/'observations.json')]);assert r['exit_code']==0
d=load(target/'observations.json');assert len(d['rows'])==15 and d['actualWindowsCallbacksInstalled']==0
save('policy/'+config+'-simulation-v1.json',dict(kind='SAFE_SIMULATED_GENERATION_MARKERS;NOT_LIVE_WORKBOOK_IDS_OR_EVENTS',configuration=config,data=d,raw=pin(target/'observations.json'),driver=pin(exe),source=pin(HERE/'fixtures/Policy43.cs'),Common=pin(lib/'BESHStat.Excel.Common.dll'),Core=pin(lib/'BESHStat.Core.dll'),gates=GATES))
print(config,'15 compiled policy simulations captured,not native lifecycle verification',flush=True)
