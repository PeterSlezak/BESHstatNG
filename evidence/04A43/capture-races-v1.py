from common43 import *
import shutil
lock();folder=PRIVATE/'races-before-v1';folder.mkdir()
for p in (PRIVATE/'fixtures-before-v3').glob('*.dll'):shutil.copy2(p,folder/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']]
exe=folder/'Race43.exe';sources=[HERE/'fixtures/Race43.cs',HERE/'fixtures/Observe43V3.cs',ROOT/'evidence/04A42/fixtures/Observe42.cs']
r=command('races-before-compile-v1',[csc,'/nologo','/target:exe','/main:Race43','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],*[str(p) for p in sources]]);assert r['exit_code']==0
r=command('races-before-execute-v1',[str(exe),str(folder/'observations.json'),str(HERE/'scope/cache-export-inventory-v1.json')]);assert r['exit_code']==0
d=load(folder/'observations.json');assert len(d['rows'])==864
save('fixtures/pre-edit-races-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A43_BOUNDED_HOST_RACES',raw=pin(folder/'observations.json'),driver=pin(exe),sources=[pin(p) for p in sources],rows=864,families=18,cultures=['Invariant','sk-SK'],iterations_per_event=8,bounded_seconds=10,parallel_FIT_and_read=True,live_Excel='NOT_RUN',Count_then_Clear='Snapshot count race preserved;not promised serializable',pre_production_edit=True,gates=GATES))
print('Pre-edit actual-cache race population864 and parallel FIT/read assertions sealed',flush=True)
