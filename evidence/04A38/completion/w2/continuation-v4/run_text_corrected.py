from common import *
exe=PRIVATE/'domain-tool-v2/bin/CallDomains-v2.dll';assert exe.is_file()
r=command('domain-tool-v2-run-corrected',['dotnet',str(exe),str(ROOT),str(HERE/'text-call-sites.json')]);assert r['exit_code']==0
save('domain-tool-corrected.json',dict(command=clean(r),source=pin(HERE/'CallDomains-v2.cs'),binary=library_pin(exe),prior_failed_attempt='Producer project name changed to CallDomains-v2.csproj and DLL followed it; earlier command requested absent CallDomains.dll. No AST rows/outputs were produced until this corrected command.',original_calls=0))
s=(HERE/'run_text_domains_v3.py').read_text(encoding='utf-8').replace('text-domains-v3','text-domains-v4').replace('text-domain-v3','text-domain-v4').replace("'-v3-'","'-v4-'").replace("'-v3.json'","'-v4.json'")
with (HERE/'run_text_domains_v4.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
