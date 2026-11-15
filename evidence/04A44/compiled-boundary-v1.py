from common44 import *
lock();folder=PRIVATE/'compiled-boundary-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';exe=folder/'References44.exe'
r=command('dependency-inspector-compile',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(framework/p)) for p in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']],str(HERE/'fixtures/References44.cs')]);assert r['exit_code']==0
rows=[]
for config in ['Debug','Release']:
 for name in ['BESHStat.Excel.Common','BESHStat.Core']:
  observed=[]
  for label,folder0 in [('before','entry-compiled'),('after','compiled')]:
   output=folder/(config+'-'+name+'-'+label+'.json');r=command('dependencies-'+config+'-'+name+'-'+label,[str(exe),str(PRIVATE/folder0/config/'lib'/(name+'.dll')),str(output)]);assert r['exit_code']==0;observed.append(load(output))
  assert observed[0]==observed[1]
  assert not any(any(f in ref for f in ['ExcelDna','Interop.Excel','System.Windows.Forms','Microsoft.Win32.Registry','System.Net.Http']) for ref in observed[1]['references'])
  rows.append(dict(configuration=config,assembly=name,references=observed[1]['references'],exact_entry_references=True,forbidden_host_references=[]))
save('abi-cil/compiled-host-boundary-v1.json',dict(kind='ACTUAL_ASSEMBLY_REFERENCE_READBACK;SOURCE_TARGET_PINS_AND_NORMALIZED_CIL_SEPARATELY_VERIFIED',rows=rows,new_framework_or_package_reference=False,Common_Core_target='netstandard2.0',gates=GATES))
print('Debug/Release Core/Common actual references unchanged;no forbidden host assembly dependencies',flush=True)
