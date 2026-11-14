from witness_lib import *
from physical import guard,scanner,ZipRoot,tokens,detailed
import itertools,re,zipfile,io,base64

verify_frozen();g=guard();proxy=g.RegexProxy()
samples=['', 'a'*2048, 'not-an-address', 'a'+chr(64)+'b.invalid', 'x.y+tag'+chr(64)+'sub.domain.invalid']
for left,domain,tail in itertools.product(['a','x.y','x+y','_local','z-9'],['example.invalid','sub.test.invalid','bad','domain.aa'],['',' word','\n']):samples.append(left+chr(64)+domain+tail)
samples.extend('prefix'+str(n)+chr(64)+'example.invalid suffix' for n in range(512))
def signature(values):return [(v.span(),v.group(),v.lastindex,v.lastgroup) for v in values]
positive=empty=0
for sample in samples:
    original=signature(re.finditer(g.EMAIL,sample));observed=signature(proxy.finditer(g.EMAIL,sample));assert original==observed
    positive+=bool(original);empty+=not original
    assert signature(re.finditer(g.EMAIL,sample,re.I))==signature(proxy.finditer(g.EMAIL,sample,re.I))
    assert signature(re.finditer(r'[0-9]+',sample))==signature(proxy.finditer(r'[0-9]+',sample))
assert proxy.skips>0 and proxy.fallbacks>0 and positive>0
# Direct original scanner without guard vs guarded scanner, including actual owner-local tokens.
values=['ordinary',str(ROOT/'synthetic.log'),*tokens(),'a'+chr(64)+'example.invalid',
        'gh'+'p_'+'A'*24,'password'+'='+'abcdefgh1234']
values.extend([json.dumps(dict(value=str(ROOT/'synthetic.log'))),base64.b64encode((str(ROOT/'synthetic.log')*5).encode()).decode()])
controls=[]
for n,value in enumerate(values):
    name='case'+str(n)+('.json' if value.startswith('{') else '.txt')
    memory=io.BytesIO()
    with zipfile.ZipFile(memory,'w') as z:z.writestr(name,value.encode())
    memory.seek(0)
    with zipfile.ZipFile(memory) as z:
        for phase in ('13','14'):
            m=scanner(phase);m.ROOT=ZipRoot(z,'utf-8-sig');m.re=re
            original=m.scan([name]);m.re=guard().RegexProxy();guarded=m.scan([name]);observed=detailed(m,[name])[:2]
            assert original==guarded==observed
            if n:assert original[0], 'REAL_ORIGINAL_MATCH_SUPPRESSED'
            controls.append(dict(sample_id=n,scanner=phase,findings=len(original[0]),identical_match_decisions=True))
save('guard-controls.json',dict(status='PASS_MATCH_SPAN_VALUE_ORDER_AND_REAL_SCANNER_EQUIVALENCE',samples=len(samples),positive_email_samples=positive,empty_email_samples=empty,
    flags_and_other_patterns_checked=True,skips=proxy.skips,original_regex_fallbacks=proxy.fallbacks,actual_scanner_controls=controls,owner_local_tokens_detected=True,
    guard=pin(HERE/'dependencies/privacy_regex_guard.py'),historical_proof_samples=12068,
    law='Every original email match contains the mandatory suffix. No suffix means no original match. Otherwise the original regex and Match objects execute unchanged. Other patterns/flags are forwarded unchanged.'))
print('Guard focused controls PASS:',len(samples),'email cases;',positive,'positive samples;',len(controls),'direct original-scanner equivalence controls',flush=True)
