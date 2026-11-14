from common import *
s=(HERE/'CallDomains.cs').read_text(encoding='utf-8').replace('indirect!=63||choiceCount!=733','indirect!=64||choiceCount!=750').replace('63 CaseToken sites/733','64 CaseToken sites/750')
with (HERE/'CallDomains-v2.cs').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'build_domain_tool.py').read_text(encoding='utf-8').replace('domain-tool-v1','domain-tool-v2').replace('CallDomains.cs','CallDomains-v2.cs').replace("save('domain-tool.json'","save('domain-tool-v2.json'")
with (HERE/'build_domain_tool_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'record_text_domains_v2.py').read_text(encoding='utf-8').replace(" if owner=='AssumptionsUDFsOptions38':", " if owner=='ParametricUdfReports38':normal='which is passed after native NormalizeText(method), names and alpha acquisition; actual typed service may receive null left, but all17 RHS literals remain fixed ASCII; no new comparison policy'\n elif owner=='AssumptionsUDFsOptions38':").replace('AND63_CASETOKEN','AND64_CASETOKEN')
s=s[:s.index("save('attempts/inherited-text-table")]
with (HERE/'record_text_domains_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(HERE/'run_text_domains_v2.py').read_text(encoding='utf-8').replace('text-domains-v2','text-domains-v3').replace('text-domain-v2','text-domain-v3').replace("'-v2-'","'-v3-'").replace("'-v2.json'","'-v3.json'")
with (HERE/'run_text_domains_v3.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
save('attempts/text-frontier-v1.json',dict(exit_code=3762504530,reason='Inherited exact-frontier guard correctly rejected added MCP site:145/59/64/750 vs C3 145/59/63/733',correction='Explicitly authorize only this already-scoped current implementation site and its17 unchanged original aliases; policy/literal checks retained. Initial missing-table recorder/probe attempts remain failed.',original_queries=0))
