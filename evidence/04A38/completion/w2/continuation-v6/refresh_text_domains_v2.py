from common import *
import shutil
save('attempts/text-frontier-entry-tool.json',dict(status='HISTORICAL_GUARD_CORRECTLY_REJECTED_NEW_FRONTIER',
    command=clean(json.loads((RUNROOT/'commands/w2c6-final-text-call-syntax/command.json').read_bytes())),
    reason='Historical C4 tool pins145 direct/59 lines/64 CaseToken/750 choices. C6 adds exactly8 direct comparisons on3 lines plus3 CaseToken sites/38 choices in the reviewed new inference class; observed153/62/67/788.',
    correction='New versioned syntax tool retains all literal/dynamic/null/ASCII guards and pins the specifically reviewed expanded frontier. Historical source/binary unchanged.',
    dependent_attempt='run_text_domains_v6.py was not produced after the guard failed; subsequent shell invocation returned file-not-found before executing any probe',original_queries=0))
old=W2/'continuation-v4/CallDomains-v2.cs';source=old.read_text(encoding='utf-8')
source=source.replace('direct!=145||directLines.Count!=59||indirect!=64||choiceCount!=750','direct!=153||directLines.Count!=62||indirect!=67||choiceCount!=788')
source=source.replace('145 direct invocations/59 lines;64 CaseToken sites/750','153 direct invocations/62 lines;67 CaseToken sites/788')
with (HERE/'CallDomains-v3.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
folder=PRIVATE/'domain-tool-v3';folder.mkdir()
project=folder/'CallDomains-v3.csproj'
xml=(RUNROOT/'completion-w2-cont4/domain-tool-v2/CallDomains-v2.csproj').read_text(encoding='utf-8')
xml=xml.replace(str(ROOT/old.relative_to(ROOT)),str(HERE/'CallDomains-v3.cs'))
with project.open('x',encoding='utf-8',newline='\n') as f:f.write(xml)
assets=RUNROOT/'completion-w2-cont4/domain-tool-v2/obj/project.assets.json'
r=command('domain-tool-v3-build',['dotnet','msbuild',str(project),'/t:Build','/p:ProjectAssetsFile='+str(assets),'/p:OutputPath='+str(folder/'bin')+'\\','/p:IntermediateOutputPath='+str(folder/'obj')+'\\','/p:UseAppHost=false','/nr:false','/v:minimal']);assert r['exit_code']==0
binary=folder/'bin/CallDomains-v3.dll'
r2=command('final-text-call-syntax-v3',['dotnet',str(binary),str(ROOT),str(HERE/'text-call-sites-v2.json')]);assert r2['exit_code']==0
for name in ['TextDomainProbe-v3.cs','run_text_domains_v6.py']:
 with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
source=(PREVIOUS/'record_text_domains_final.py').read_text(encoding='utf-8')
source=source.replace("elif owner=='UdfScalarChoice38':", "elif owner=='UdfInferencePreparation38':normal='Native NormalizeText at its original before/after-fit position; fixed non-null ASCII RHS/choices; nullable-left does not make synthetic null-RHS counterexamples reachable'\n elif owner=='UdfScalarChoice38':")
source=source.replace('ALL59_DIRECT_LINES_AND64_CASETOKEN_SITES_DOMAIN_REVIEWED','ALL_ACTUAL_C6_SITES_DOMAIN_REVIEWED')
with (HERE/'record_text_domains.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('text-domain-syntax-v1.json',dict(build=clean(r),command=clean(r2),binary=library_pin(binary),source=pin(HERE/'CallDomains-v3.cs'),original_queries=0))
exec(compile(source,str(HERE/'record_text_domains.py'),'exec'))
