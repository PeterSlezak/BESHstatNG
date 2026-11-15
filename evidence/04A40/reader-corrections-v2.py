"""Additive report-only corrections; no executed identity is repeated."""
from pathlib import Path
here=Path(__file__).parent
p=here/'final-observations-v1.py';s=p.read_text(encoding='utf-8');a=s.index('lock();obs=');b=s.index('before=load(')
s=s[:a]+"lock();obs=PRIVATE/'fixtures-final-Release';assert (obs/'observations.json').is_file()\n"+s[b:]
s=s.replace("x.get('output',{}).get('state',{})","(x.get('output') or {}).get('state',{})")
(here/'final-observations-readback-v2.py').open('x',encoding='utf-8').write(s)
p=here/'node-scanners-v1.py';s=p.read_text(encoding='utf-8')
s=s.replace('for r in runs:',"for suffix,r in zip(['office-check','office-node','resolver','wasm-check','wasm-node'],runs):")
s=s.replace("p=ROOT/r['stdout']","p=PRIVATE/'commands'/('a40-final-'+suffix)/'stdout.log'").replace("command=r['argv']","command=r['command']")
(here/'node-scanners-v2.py').open('x',encoding='utf-8').write(s)
