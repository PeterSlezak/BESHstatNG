"""Current-only complete dependency syntax, exact parameter shapes and actual return metadata."""
from common import *
from proof_kernel_v7 import map_current,definitions,bind
tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll'
paths_by_role={'common':sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Excel.Common/src').rglob('*.vb')),
    'core':sorted(p.relative_to(ROOT).as_posix() for p in (ROOT/'BESHStat.Core/src').rglob('*.vb'))}
commands=[]
for role,paths in paths_by_role.items():
    result=command('dependency-full-'+role+'-syntax-v1',['dotnet',str(tool),str(ROOT),str(HERE/('dependency-full-'+role+'-v1.json')),*paths]);commands.append(clean(result));assert result['exit_code']==0
portable=paths_by_role['common']
result=command('portable-syntax-v1',['dotnet',str(tool),str(ROOT),str(HERE/'portable-universe-v1.json'),*portable]);commands.append(clean(result));assert result['exit_code']==0
folder=PRIVATE/'declaration-shapes-v1';folder.mkdir(exist_ok=False)
project=(ROOT/'evidence/04A38/completion/w1/Census.csproj').read_text(encoding='utf-8-sig').replace('Include="Census.cs"','Include="'+str(HERE/'DeclarationShapes-v1.cs')+'"')
with (folder/'Census.csproj').open('x',encoding='utf-8') as stream:stream.write(project)
assets=RUNROOT/'mmrm38-v6/w1-census-tool-v2/obj/project.assets.json'
result=command('declaration-shapes-build-v1',['dotnet','msbuild',str(folder/'Census.csproj'),'/t:Build','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(folder/'bin')+'\\','/p:IntermediateOutputPath='+str(folder/'obj')+'\\','/p:ProduceReferenceAssembly=false','/v:minimal','/nr:false']);commands.append(clean(result));assert result['exit_code']==0
universe=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
paths=list(dict.fromkeys([r['path'] for r in universe['sources']]+paths_by_role['common']+paths_by_role['core']))
shapes={};sources={}
for index in range(0,len(paths),70):
    name='declaration-shapes-part-'+str(index//70)+'-v1.json'
    result=command(name[:-5],['dotnet',str(folder/'bin/Census.dll'),str(ROOT),str(HERE/name),*paths[index:index+70]]);commands.append(clean(result));assert result['exit_code']==0
    doc=json.loads((HERE/name).read_bytes())
    for row in doc['rows']:assert shapes.setdefault(row['symbol_id'],row)==row
    for row in doc['sources']:assert sources.setdefault(row['path'],row)==row
save('declaration-shapes-v1.json',dict(scope='CURRENT_SYNTAX_ONLY_EXACT_UNION',sources=[sources[k] for k in sorted(sources)],rows=[shapes[k] for k in sorted(shapes)]))
returns=PRIVATE/'return-metadata-v1';returns.mkdir(exist_ok=False);exe=returns/'ReturnMetadata.exe'
result=command('return-metadata-compile-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'ReturnMetadata-v1.cs')]);commands.append(clean(result));assert result['exit_code']==0
owners=set()
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes())
    owners.update(t['type'] for t in meta['types'] if t['type'])
with (HERE/'return-metadata-owners-v1.txt').open('x',encoding='utf-8',newline='\n') as stream:stream.write('\n'.join(sorted(owners))+'\n')
for config in ['Debug','Release']:
    result=command('return-metadata-'+config+'-v1',[str(exe),str(PRIVATE/'packet-v2'/config/'lib'),str(HERE/('return-metadata-'+config+'-v1.json')),str(HERE/'return-metadata-owners-v1.txt')]);commands.append(clean(result));assert result['exit_code']==0
save('dependency-syntax-authority-v1.json',dict(commands=commands,syntax_tool=pin(tool),shape_source=pin(HERE/'DeclarationShapes-v1.cs'),shape_tool=pin(folder/'bin/Census.dll'),return_source=pin(HERE/'ReturnMetadata-v1.cs'),return_tool=pin(exe),original_queries=0,scope='Actual current whole Common/Core syntax, exact native/portable parameters and measured Debug/Release return metadata. Source/CIL graph and ownership reconciliation are separate mandatory proofs.'))
print('Full current dependency/parameter/return syntax ready')
