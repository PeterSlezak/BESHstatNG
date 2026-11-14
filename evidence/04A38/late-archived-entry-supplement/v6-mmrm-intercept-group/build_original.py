"""Fresh offline compilation, using the independently reauthenticated original tree."""
import shutil
from common import *

def main():
    config=sys.argv[1];assert config in ['Debug','Release']
    entry=json.loads((HERE/'entry-inspection.json').read_bytes())
    assert entry['status']=='LIVE_ENTRY_AND_ORIGINAL_SOURCE_AUTHENTICATED' and len(entry['original_tree_files'])==4695
    output=PRIVATE/'original-build'/config;output.mkdir(parents=True,exist_ok=False)
    runner=m.Runner(RUNROOT);commands=[]
    # Original projects, original source, original third-party package cache.
    # No restore and no production DLL from the changed worktree is consumed.
    for label,project in [('core','BESHStat.Core/BESHStat.Core.vbproj'),('common','BESHStat.Excel.Common/BESHStat.Excel.Common.vbproj')]:
        args=['dotnet','msbuild',str(TREE/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false',
              '/p:ProduceReferenceAssembly=false','/p:OutputPath='+str(output)+'\\',
              '/p:IntermediateOutputPath='+str(PRIVATE/'obj'/config/label)+'\\','/m:4','/v:minimal','/nr:false']
        result=runner.command('mmrm38-v6-original-'+config+'-'+label,args,cwd=TREE);commands.append(clean(result));assert result['exit_code']==0
    args=[runner.msbuild,str(TREE/'BESHstatNG/BESHstatNG.vbproj'),'/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory',
          '/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false',
          '/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'obj'/config/'host')+'\\',
          '/p:PythonExe='+sys.executable,'/m:4','/v:minimal','/nr:false']
    result=runner.command('mmrm38-v6-original-'+config+'-host',args,cwd=TREE);commands.append(clean(result));assert result['exit_code']==0
    shutil.copy2(TREE/'packages/ExcelDna.Integration.1.9.0/lib/net462/ExcelDna.Integration.dll',output/'ExcelDna.Integration.dll')
    inputs=[]
    for p in sorted((TREE/'packages').rglob('*')):
        if p.is_file():
            data=p.read_bytes();inputs.append(dict(path='packages/'+p.relative_to(TREE/'packages').as_posix(),bytes=len(data),sha256=sha(data)))
    save('build-original-'+config+'.json',dict(status='PINNED_ORIGINAL_COMPILED_OFFLINE',configuration=config,source_commit=BASE,
         reference_class=CLASS,commands=commands,assemblies=[library_pin(p) for p in sorted(output.glob('*.dll'))],
         offline_third_party_inputs=inputs,current_production_assemblies_consumed=False,
         compiler=library_pin(Path(compiler('vbc.exe'))),restore='NOT_RUN',packaging='NOT_RUN',native='NOT_RUN'))

if __name__=='__main__':main()
