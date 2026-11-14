"""Fresh whole-source syntax census and metadata reads; never run a reference."""
import json,shutil,sys
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
V6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'
sys.path.insert(0,str(V6))
from common import PRIVATE,RUNROOT,TREE,command,clean,pin,library_pin,compiler
def save(name,obj):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:json.dump(obj,stream,indent=2,ensure_ascii=False);stream.write('\n')
def main():
 assert json.loads((V6/'MMRM_INTERCEPT_GROUP_GATE_PASS.json').read_bytes())['status']=='MMRM_INTERCEPT_GROUP_GATE_PASS'
 folder=PRIVATE/'w1-census-tool';folder.mkdir(exist_ok=False)
 obj=folder/'obj';obj.mkdir();assets=RUNROOT/'inventory-v1/obj/project.assets.json';assert assets.is_file()
 shutil.copy2(assets,obj/'project.assets.json')
 built=command('w1-census-build',['dotnet','msbuild',str(HERE/'Census.csproj'),'/t:Build','/p:Configuration=Release','/p:BaseIntermediateOutputPath='+str(obj)+'\\','/p:MSBuildProjectExtensionsPath='+str(obj)+'\\','/p:OutputPath='+str(folder/'bin')+'\\','/m:1','/v:minimal','/nr:false']);assert built['exit_code']==0
 commands=[clean(built)]
 for label,root in [('original',TREE),('current',ROOT)]:
  result=command('w1-census-'+label,['dotnet',str(folder/'bin/Census.dll'),str(root),str(HERE/(label+'-universe.json'))]);commands.append(clean(result));assert result['exit_code']==0
 # Fresh metadata/IL reads of unchanged actual Windows binaries; no method or
 # type initializer is invoked, and these are not extra statistical references.
 inspection=PRIVATE/'w1-inspection';inspection.mkdir()
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 for tool in ['Inspect38','PortableConsumerClosure38']:
  exe=inspection/(tool+'.exe');source=ROOT/'evidence/04A38/helpers'/(tool+'.cs')
  result=command('w1-'+tool+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(facade),'/out:'+str(exe),str(source)]);commands.append(clean(result));assert result['exit_code']==0
  build=RUNROOT/'builds/validation-v2/Debug/legacy'
  args=[str(build),str(HERE/'compiled-current.json'),str(ROOT/'evidence/04A38/entry-members.json')] if tool=='Inspect38' else [str(build),str(ROOT/'evidence/04A38/entry-members.json'),str(HERE/'compiled-current-closure.json')]
  result=command('w1-'+tool+'-read',[str(exe),*args]);commands.append(clean(result));assert result['exit_code']==0
 save('census-command-receipts.json',dict(commands=commands,tool_sources=[pin(HERE/n) for n in ['Census.csproj','Census.cs']],compiler=library_pin(Path(compiler('csc.exe'))),
  offline_asset_identity=library_pin(assets),asset_policy='Existing no-package net10 syntax-tool assets reused byte-for-byte; no restore/network/install',source_role='READ_ONLY_STATIC_AND_COMPILED_METADATA_NOT_REFERENCE_EXECUTION',new_original_queries=0,new_production_changes=0))
 print('Fresh original/current whole syntax and actual Windows metadata/IL census completed')
if __name__=='__main__':main()
