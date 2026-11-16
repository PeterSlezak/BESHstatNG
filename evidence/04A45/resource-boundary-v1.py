from common45 import *
lock();folder=PRIVATE/'resource-boundary-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0];framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';exe=folder/'ResourceBoundary45.exe'
r=command('resource-boundary-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(framework/p)) for p in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']],str(HERE/'fixtures/ResourceBoundary45.cs')]);assert not r['exit_code']
rows=[]
for c in ['Debug','Release']:
 out=folder/(c+'.json');lib=PRIVATE/'builds/a45-final-v1'/c/'legacy'
 r=command('resource-boundary-'+c+'-v1',[str(exe),str(lib),str(ROOT/'BESHstatNG/Resources/RibbonXML.xml'),str(out)]);assert not r['exit_code']
 d=load(out)
 for x in d['rows']:
  if x['name']!='BESHStatNG':
   assert x['target']==['.NETStandard,Version=v2.0']
   assert not any(any(f in y for f in ['ExcelDna','Interop.Excel','System.Windows.Forms','Microsoft.Win32.Registry','System.Net.Http']) for y in x['references'])
  else:assert x['target']==['.NETFramework,Version=v4.8']
 rows.append(dict(configuration=c,**d))
save('abi-cil/dependency-resource-boundary-v1.json',dict(rows=rows,source=pin('evidence/04A45/fixtures/ResourceBoundary45.cs'),native='NOT_RUN'))
print('Actual compiled Ribbon resource/source exact;Core/Common netstandard2.0 clean references;Windows net48',flush=True)
