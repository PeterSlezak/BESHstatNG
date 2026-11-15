from common42 import *
import shutil
lock();folder=PRIVATE/'fixtures-before-v1';folder.mkdir();lib=PRIVATE/'builds/a42-entry-v1/Debug/legacy'
for p in lib.glob('*.dll'):shutil.copy2(p,folder/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']]
exe=folder/'Observe42.exe';source=HERE/'fixtures/Observe42.cs'
r=command('fixture-before-compile-v1',[csc,'/nologo','/target:exe','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(source)]);assert r['exit_code']==0
r=command('fixture-before-execute-v1',[str(exe),str(folder/'observations.json')]);assert r['exit_code']==0
observed=load(folder/'observations.json')
save('fixtures/pre-edit-current-main-v1.json',dict(kind='EXECUTED_CURRENT_MAIN_PRE_EDIT42;NOT_HISTORICAL_PRE_EDIT_FROZEN_OR_GOLDEN',base=MAIN,cases=len(observed['rows']),raw=pin(folder/'observations.json'),driver_source=pin(source),driver=pin(exe),assemblies=[pin(p) for p in sorted(folder.glob('*.dll'))],cultures=['Invariant','sk-SK'],profiles=['ordinary','reverse','filtered','duplicates'],seed=173,pre_production_change=True,gates=GATES))
print('Materialized before-edit42',len(observed['rows']),flush=True)
