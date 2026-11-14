from common import *
for name in ['CallDomains.cs','build_domain_tool.py','record_text_domains_v2.py','TextDomainProbe-v2.cs','run_text_domains_v2.py']:
 s=(PREVIOUS/name).read_text(encoding='utf-8')
 if name=='record_text_domains_v2.py':s=s.replace("pin(PREVIOUS/'record_text_domains.py')","pin(PREVIOUS.parent/'continuation-v2/record_text_domains.py')")
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
