from common import *
from baseline_report import python_receipt
records=[]
for label,pattern in [('p1-adverse-v1-reproduction','test_p1.py'),('p1-adverse-v2','test_p1_v2.py'),('p0-adverse-current','test_p0.py')]:
 r=command(label,[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p',pattern,'-v'])
 records.append(dict(label=label,receipt=python_receipt('w2c1-'+label),command=clean(r)))
save('bounded-adverse-tests.json',dict(results=records,initial_v1='13PASS/1FAIL:missing portable preimage check; reproduced unchanged to preserve raw evidence',accepted_v2='14PASS; includes self-consistent corruptions after rehash',p0='9PASS after P1; no original semantic queries',new_original_queries=0))
