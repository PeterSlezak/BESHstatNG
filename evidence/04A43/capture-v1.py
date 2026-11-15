from common43 import *
import shutil
lock();folder=PRIVATE/'fixtures-before-v1';folder.mkdir();lib=PRIVATE/'builds/a43-entry-v1/Debug/legacy'
for p in lib.glob('*.dll'):shutil.copy2(p,folder/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']]
exe=folder/'Observe43.exe';source=HERE/'fixtures/Observe43.cs'
r=command('lifecycle-before-compile-v1',[csc,'/nologo','/target:exe','/main:Observe43','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(source),str(ROOT/'evidence/04A42/fixtures/Observe42.cs')]);assert r['exit_code']==0
r=command('lifecycle-before-execute-v1',[str(exe),str(folder/'observations.json'),str(HERE/'scope/cache-export-inventory-v1.json')]);assert r['exit_code']==0
data=load(folder/'observations.json');assert len(data['graphs'])==36
save('fixtures/pre-edit-current-main-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A43',base=MAIN,operations=len(data['operations']),real_fit_graphs=36,cache_families=18,raw=pin(folder/'observations.json'),driver=pin(exe),driver_source=pin(source),reused_synthetic_input_source=pin(ROOT/'evidence/04A42/fixtures/Observe42.cs'),assemblies=[pin(p) for p in folder.glob('*.dll')],cultures=['Invariant','sk-SK'],seed=173,pre_production_edit=True,independent_scientific_oracle=False,gates=GATES))
print('Frozen actual current-main lifecycle graphs36 and typed operations',len(data['operations']),flush=True)
