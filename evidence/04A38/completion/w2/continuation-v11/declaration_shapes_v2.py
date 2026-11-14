"""Build the syntax-only recorder offline, preserving exact declaration parameter shapes."""
from common import *
tool=PRIVATE/'declaration-shapes-v1';tool.mkdir(exist_ok=False)
project=(ROOT/'evidence/04A38/completion/w1/Census.csproj').read_text(encoding='utf-8-sig').replace('Include="Census.cs"','Include="'+str(HERE/'DeclarationShapes-v1.cs')+'"')
with (tool/'Census.csproj').open('x',encoding='utf-8') as f:f.write(project)
assets=RUNROOT/'mmrm38-v6/w1-census-tool-v2/obj/project.assets.json'
r=command('declaration-shapes-build-v1',['dotnet','msbuild',str(tool/'Census.csproj'),'/t:Build','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(tool/'bin')+'\\','/p:IntermediateOutputPath='+str(tool/'obj')+'\\','/p:ProduceReferenceAssembly=false','/v:minimal','/nr:false']);assert r['exit_code']==0
paths=[r['path'] for r in json.loads((HERE/'current-universe-packet-v2.json').read_bytes())['sources']]
paths.extend(r['path'] for r in json.loads((HERE/'portable-universe-v1.json').read_bytes())['sources']);paths.extend(r['path'] for r in json.loads((HERE/'dependency-source-universe-v1.json').read_bytes())['sources']);paths=list(dict.fromkeys(paths))
r=command('declaration-shapes-current-v1',['dotnet',str(tool/'bin/Census.dll'),str(ROOT),str(HERE/'declaration-shapes-v1.json'),*paths]);assert r['exit_code']==0
save('declaration-shapes-authority-v1.json',dict(recorder=pin(HERE/'DeclarationShapes-v1.cs'),artifact=pin(tool/'bin/Census.dll'),assets=pin(assets),result=pin(HERE/'declaration-shapes-v1.json'),original_queries=0,scope='Source parse only; no original or current statistical code compilation/execution by this recorder.'))
