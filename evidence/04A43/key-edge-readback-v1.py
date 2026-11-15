from common43 import *
lock();folder=PRIVATE/'key-edge-readback-v1';folder.mkdir();locator=Path(os.environ['ProgramFiles(x86)'])/'Microsoft Visual Studio/Installer/vswhere.exe';csc=subprocess.check_output([str(locator),'-products','*','-version','[18.0,19.0)','-find','MSBuild/Current/Bin/Roslyn/csc.exe'],text=True).splitlines()[0]
framework=Path(os.environ['ProgramFiles(x86)'])/'Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8';refs=[framework/n for n in ['mscorlib.dll','System.dll','System.Core.dll','System.Web.Extensions.dll']];exe=folder/'KeyEdges43.exe';r=command('key-edges-compile-v1',[csc,'/nologo','/out:'+str(exe),*[('/r:'+str(p)) for p in refs],str(HERE/'fixtures/KeyEdges43.cs')]);assert r['exit_code']==0
results=[]
for config in ['Debug','Release']:
    observed={}
    for stage,lib in [('sealed-entry',PRIVATE/'entry-compiled'/config/'lib'),('current',PRIVATE/'compiled'/config/'lib')]:
        path=folder/(config+'-'+stage+'.json');r=command('key-edges-'+config+'-'+stage+'-v1',[str(exe),str(lib),str(path)]);assert r['exit_code']==0;observed[stage]=load(path)
    assert observed['sealed-entry']==observed['current'] and len(observed['current']['rows'])==30
    results.append(dict(configuration=config,rows=30,exact=True,baseline=pin(folder/(config+'-sealed-entry.json')),current=pin(folder/(config+'-current.json'))))
save('fixtures/key-edge-readback-v1.json',dict(kind='ADDITIONAL_LATE_MEASUREMENT_OF_AUTHENTIC_SEALED_ENTRY;NOT_PRE_EDIT_FROZEN_OR_PART_OF1118',results=results,input_axes=['Invariant/sk-SK numeric/string/bool coercion','Missing/Empty/ExcelError','0/1/nonzero lower-bound arrays','Throwing ToString and ByRef key state on exception'],original1118_and_old29424_unchanged=True,raw_values_private=True,source=pin(HERE/'fixtures/KeyEdges43.cs'),gates=GATES))
print('30 supplementary key edges/configuration exact against actual sealed entry;explicitly late measurement,not an invented pre-edit oracle',flush=True)
