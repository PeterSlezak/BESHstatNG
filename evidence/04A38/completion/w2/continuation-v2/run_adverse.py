from common import *
from baseline_report import python_receipt
records=[]
for name in ['test_independent_slice.py','test_host_cache.py']:
 label='independent-'+name.removesuffix('.py');r=command(label,[sys.executable,'-B','-m','unittest','discover','-s',str(HERE),'-p',name,'-v']);records.append(dict(command=clean(r),tests=python_receipt('w2c2-'+label)));assert r['exit_code']==0
save('independent-adverse.json',dict(results=records,original_queries=0,scope='Reader corruption fixtures only; no new original semantic reference'))
