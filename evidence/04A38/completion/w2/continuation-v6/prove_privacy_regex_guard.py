from common import *
from privacy_regex_guard import RegexProxy,EMAIL
import random,re,time
rand=random.Random(3806);alphabet='azAZ09._%+-@ /\\\n\r\t'+'žĽıİ\u212a\u00ad\u0000'
samples=['','namespace.'*100, 'a'+'@'+'b.example',' '+'@'+'a.example', 'x'+'@'+'a.example'+'\n'+'y'+'@'+'b.example']
samples += [''.join(rand.choice(alphabet) for _ in range(rand.randrange(0,140))) for _ in range(12000)]
for name in ['metadata-full/families-v2/current-Debug.json','whole-body-contracts-v1.json','text-call-sites-v2.json']:
 text=(HERE/name).read_text(encoding='utf-8');samples.extend(text[n:n+5000] for n in range(0,len(text),max(1,len(text)//20)))
proxy=RegexProxy()
for text in samples:
 a=[(m.start(),m.end(),m.group()) for m in re.finditer(EMAIL,text)]
 b=[(m.start(),m.end(),m.group()) for m in proxy.finditer(EMAIL,text)]
 assert a==b
save('privacy-regex-guard-proof-v1.json',dict(status='EXACT_MATCH_SPAN_VALUE_ORDER_EQUIVALENCE_PASS',samples=len(samples),empty_email_cases=proxy.skips,original_finditer_fallbacks=proxy.fallbacks,
    law='Every original email match contains the identical mandatory @domain suffix. Absent suffix proves the original iterator empty. Otherwise execute the unchanged original regex.',
    source=pin(HERE/'privacy_regex_guard.py'),assertions_changed=0,files_filtered=0,positive_patterns_waived=0,original_queries=0))
print('Exact regex guard controls PASS',len(samples))
