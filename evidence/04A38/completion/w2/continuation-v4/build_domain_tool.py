from common import *
folder=PRIVATE/'domain-tool-v1';folder.mkdir();out=folder/'bin';out.mkdir()
assets=RUNROOT/'mmrm38-v6/w1-census-tool-v2/obj/project.assets.json';before=pin(assets)
project=folder/'CallDomains.csproj'
project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><UseAppHost>false</UseAppHost></PropertyGroup><ItemGroup><Compile Include="'''+str(HERE/'CallDomains.cs')+'''"/><Reference Include="Microsoft.CodeAnalysis"><HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.dll</HintPath></Reference><Reference Include="Microsoft.CodeAnalysis.VisualBasic"><HintPath>$(MSBuildSDKsPath)/../Roslyn/bincore/Microsoft.CodeAnalysis.VisualBasic.dll</HintPath></Reference></ItemGroup></Project>''',encoding='utf-8')
r=command('domain-tool-v1-build',['dotnet','msbuild',str(project),'/t:Build','/p:Configuration=Release','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(out)+'\\','/p:IntermediateOutputPath='+str(folder/'obj')+'\\','/m:4','/v:minimal','/nr:false']);assert r['exit_code']==0
assert pin(assets)==before
r2=command('domain-tool-v1-run',['dotnet',str(out/'CallDomains.dll'),str(ROOT),str(HERE/'text-call-sites.json')]);assert r2['exit_code']==0
save('domain-tool.json',dict(commands=[clean(r),clean(r2)],source=pin(HERE/'CallDomains.cs'),cached_assets=before,assets_unchanged=True,restore_install='NOT_RUN',original_queries=0))
