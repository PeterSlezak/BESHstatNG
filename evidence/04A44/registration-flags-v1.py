from common44 import *
import shutil
lock();folder=PRIVATE/'registration-flags-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']];exe=folder/'Attributes44.exe'
r=command('attribute-flags-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(HERE/'fixtures/Attributes44.cs')]);assert r['exit_code']==0
records={}
for config in ['Debug','Release']:
    r=command('attribute-flags-'+config+'-v1',[str(exe),str(PRIVATE/'compiled'/config/'lib'),str(folder/(config+'.json'))]);assert r['exit_code']==0
    records[config]=load(folder/(config+'.json'))
assert records['Debug']==records['Release'] and len(records['Debug']['exports'])==268
save('abi-cil/effective-registration-flags-v1.json',dict(kind='ACTUAL_CONSTRUCTED_ATTRIBUTE_DEFAULTS_AND_NAMED_VALUES;NOT_EXCEL_REGISTRATION_EXECUTION',records=records,raw=[pin(folder/(c+'.json')) for c in ['Debug','Release']],source=pin(HERE/'fixtures/Attributes44.cs'),live_Excel='NOT_RUN',gates=GATES))
print('268 actual attribute instances/configuration agree;not a live registration/recalc claim',flush=True)
