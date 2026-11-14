from common import *
tool=PRIVATE/'domain-tool-v1/bin/CallDomains.dll'
r=command('final-current-domain-syntax',['dotnet',str(tool),str(ROOT),str(HERE/'text-call-sites-final.json')]);assert r['exit_code']==0
old=json.loads((HERE/'text-call-sites.json').read_bytes());new=json.loads((HERE/'text-call-sites-final.json').read_bytes());assert old['calls']==new['calls'],'Caller domain changed; investigate before claiming current proof'
s=(HERE/'run_text_domains_v2.py').read_text(encoding='utf-8').replace("'text-domains-v2'","'text-domains-final'").replace("'text-call-sites.json'","'text-call-sites-final.json'").replace("'text-domain-v2-'","'text-domain-final-'").replace("'-v2-'","'-final-'").replace("'-v2.json'","'-final.json'")
with (HERE/'run_final_domains.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('text-final-syntax-refresh.json',dict(command=clean(r),previous=pin(HERE/'text-call-sites.json'),final=pin(HERE/'text-call-sites-final.json'),all_comparison_sites_identical=True,note='B1 changed a Common spill source with no text comparisons; full final Common syntax/source inventory freshly scanned. Earlier syntax/probe bytes remain unchanged. Fresh runtime comparison uses final current b1-v1 assemblies.',original_queries=0))
