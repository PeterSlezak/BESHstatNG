from common import *
for n in ['build_domain_tool.py','record_text_domains_v2.py','run_text_domains_v2.py']:
 s=(PREVIOUS/n).read_text(encoding='utf-8')
 if n=='record_text_domains_v2.py':s=s.replace("save('attempts/text-table-recorder-v1.json'", "save('attempts/inherited-text-table-recorder-scope.json'").replace("pin(HERE/'record_text_domains.py')","pin(PREVIOUS/'record_text_domains.py')")
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:f.write(s)
