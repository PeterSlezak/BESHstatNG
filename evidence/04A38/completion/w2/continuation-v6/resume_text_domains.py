from common import *
save('attempts/text-tool-assets-v1.json',dict(status='OFFLINE_BUILD_ATTEMPT_RETAINED',
    command=clean(json.loads((RUNROOT/'commands/w2c6-domain-tool-v3-build/command.json').read_bytes())),
    failure='NETSDK1004: the old domain-tool output directory has no assets file; it previously used the separately cached census assets.',
    correction='Fresh isolated output uses the same existing census ProjectAssetsFile, with before/after byte authentication. No restore or install.',
    dependent_attempt='Absent run_text_domains_v6.py invocation failed before execution',original_queries=0))
folder=PRIVATE/'domain-tool-v3b';folder.mkdir()
project=folder/'CallDomains-v3.csproj'
with project.open('x',encoding='utf-8',newline='\n') as f:f.write((PRIVATE/'domain-tool-v3/CallDomains-v3.csproj').read_text(encoding='utf-8'))
assets=RUNROOT/'mmrm38-v6/w1-census-tool-v2/obj/project.assets.json';before=pin(assets)
r=command('domain-tool-v3b-build',['dotnet','msbuild',str(project),'/t:Build','/p:Configuration=Release','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(folder/'bin')+'\\','/p:IntermediateOutputPath='+str(folder/'obj')+'\\','/nr:false','/v:minimal']);assert r['exit_code']==0 and pin(assets)==before
binary=folder/'bin/CallDomains-v3.dll'
r2=command('final-text-call-syntax-v3b',['dotnet',str(binary),str(ROOT),str(HERE/'text-call-sites-v2.json')]);assert r2['exit_code']==0
for name in ['TextDomainProbe-v3.cs','run_text_domains_v6.py']:
 with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
source=(PREVIOUS/'record_text_domains_final.py').read_text(encoding='utf-8')
source=source.replace("elif owner=='UdfScalarChoice38':", "elif owner=='UdfInferencePreparation38':normal='Native NormalizeText at its original before/after-fit position; fixed non-null ASCII RHS/choices; nullable-left does not make synthetic null-RHS counterexamples reachable'\n elif owner=='UdfScalarChoice38':")
source=source.replace('ALL59_DIRECT_LINES_AND64_CASETOKEN_SITES_DOMAIN_REVIEWED','ALL_ACTUAL_C6_SITES_DOMAIN_REVIEWED')
with (HERE/'record_text_domains.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('text-domain-syntax-v1.json',dict(build=clean(r),command=clean(r2),binary=library_pin(binary),source=pin(HERE/'CallDomains-v3.cs'),cached_assets=before,assets_unchanged=True,restore_install='NOT_RUN',original_queries=0))
exec(compile(source,str(HERE/'record_text_domains.py'),'exec'))
