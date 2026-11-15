from common44 import *
import shutil
lock();folder=PRIVATE/'fixtures-before-v1';folder.mkdir();lib=PRIVATE/'entry-compiled/Debug/lib'
for p in lib.glob('*.dll'):shutil.copy2(p,folder/p.name)
locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']];exe=folder/'Observe44.exe'
r=command('capture-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(HERE/'fixtures/Observe44.cs')]);assert r['exit_code']==0
r=command('capture-execute-v1',[str(exe),str(folder/'observations.json')]);assert r['exit_code']==0
d=load(folder/'observations.json');assert len(d['rows'])>400 and len({r['id'] for r in d['rows']})==len(d['rows'])
for p in folder.rglob('*'):
 if p.is_file():p.chmod(0o444)
save('fixtures/pre-edit-manifest-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A44;SAME_ENGINE_COMPATIBILITY_NOT_GOLDEN',cases=len(d['rows']),raw=pin(folder/'observations.json'),driver=pin(exe),source=pin(HERE/'fixtures/Observe44.cs'),cultures=['Invariant','en-US','sk-SK'],host_actions='No Excel/COM/UI/browsers/updater start;only pure private updater comparator invoked. Installed settings store untouched;missing-store save error frozen.',errors=sum('error' in r for r in d['rows']),gates=GATES))
print('Materialized current-main observations sealed:',len(d['rows']),'cases',flush=True)
