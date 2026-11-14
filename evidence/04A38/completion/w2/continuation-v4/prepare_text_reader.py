from common import *
s=(PREVIOUS/'verify_text_domains.py').read_text(encoding='utf-8').replace('from verify_names import','from verify_slice_v2 import').replace('==208','==209').replace('==416','==418').replace('text-domains-names-v3-v2-','text-domains-parametric-v4-v4-').replace('CaseToken_sites=63,choices=733,comparisons_per_configuration=1568736','CaseToken_sites=64,choices=750,comparisons_per_configuration=1576278')
with (HERE/'verify_text_domains.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
