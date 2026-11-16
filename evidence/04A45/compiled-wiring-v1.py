from common45 import *
lock();folder=PRIVATE/'wiring-inspector-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe'
csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';exe=folder/'InspectWiring45.exe'
r=command('wiring-inspector-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(framework/p)) for p in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']],str(HERE/'fixtures/InspectWiring45.cs')]);assert not r['exit_code']
for config in ['Debug','Release']:
 lib=PRIVATE/'builds/a45-final-v1'/config/'legacy';out=folder/(config+'.json')
 r=command('wiring-inspector-'+config+'-v1',[str(exe),str(lib),str(out)]);assert not r['exit_code']
 data=load(out);save('scope/compiled-wiring-'+config+'-v1.json',dict(data,inspector=pin('evidence/04A45/fixtures/InspectWiring45.cs'),assembly=pin(lib/'BESHStatNG.dll'),limitations='Metadata/IL show compiled member/delegate references;no static constructors,method invocation,form creation,Excel registration,COM or native API called. Actual instance/event delivery remains pending.'))
print('Windows wiring metadata/IL inspected without initializing forms or Excel',flush=True)
