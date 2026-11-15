from common41 import *
import shutil
lock()
folder=PRIVATE/'fixtures-before-v1';folder.mkdir()
lib=PRIVATE/'builds/a41-entry-v1/Debug/legacy'
for p in lib.glob('*.dll'):shutil.copy2(p,folder/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8'
refs=[framework/'mscorlib.dll',framework/'System.dll',framework/'System.Core.dll',framework/'System.Web.Extensions.dll']
exe=folder/'Observe41.exe';source=HERE/'fixtures/Observe41.cs'
r=command('fixture-before-compile-v1',[csc,'/nologo','/target:exe','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(source)])
assert r['exit_code']==0
r=command('fixture-before-execute-v1',[str(exe),str(folder/'observations.json')]);assert r['exit_code']==0
observed=load(folder/'observations.json');assert len(observed['rows'])>=350
save('fixtures/pre-edit-current-main-v1.json',dict(kind='MATERIALIZED_CURRENT_MAIN_PRE_EDIT41;NOT_PRE_EDIT_FROZEN_ORIGINAL_CORPUS',base=MAIN,cases=len(observed['rows']),raw=pin(folder/'observations.json'),driver_source=pin(source),driver=pin(exe),assemblies=[pin(p) for p in sorted(folder.glob('*.dll'))],all_inputs_outputs_bits_shapes_errors_materialized=True,cultures=['InvariantCulture','sk-SK'],seed=173,pre_production_change=True,scientific='NOT_GOLDEN',sealed_original_corpus_modified=False))
print('Pre-edit41 current-main materialized cases',len(observed['rows']),flush=True)
