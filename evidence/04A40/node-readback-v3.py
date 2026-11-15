from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'evidence/04A40'))
from common40_v3 import *
import re,collections
rows=[]
for name in ['office-node','resolver','wasm-node']:
    path=PRIVATE/'commands'/('a40-final-'+name)/'stdout.log';text=path.read_bytes().decode('utf-8-sig')
    names=re.findall(r'^[\u2714\u2716] (.*?) \([\d.]+ms\)\r?$',text,re.M|re.S);ids=[sha(n.encode()) for n in names]
    count=lambda label:int(re.findall(r'(?:# |\u2139 )'+label+r' (\d+)',text)[-1])
    previous=load(ROOT/('evidence/04A38/completion/w3/python-node-scanners/node-'+name+'-v1.json'))
    assert collections.Counter(ids)==collections.Counter(previous['identities']) and count('fail')==0 and len(ids)==count('pass')==count('tests')
    rows.append(dict(suite=name,tests=count('tests'),passed=count('pass'),failed=0,identities=ids,exact_historical_identity_multiset=True,raw=pin(path)))
assert sum(r['passed'] for r in rows)==916
save('python/Node-fresh-identities-v3.json',dict(suites=rows,passed=916,NOT_RUN='Office/Mac/browser/native'))
print('Fresh916 Node PASS; exact historical identity multisets',flush=True)
