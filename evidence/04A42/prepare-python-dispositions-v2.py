from common42 import *
s=(HERE/'python-dispositions-v1.py').read_text(encoding='utf-8-sig');needle="            else:raise AssertionError('UNEXPLAINED_CURRENT_ASSERTION_ARGUMENT_CHANGE')"
replacement="""            elif any(r['sealed']['sha256']==before and r['current']['sha256']==after for r in load(HERE/'entry/authentication-v1.json')['merged41_newline_transitions']):
                disposition='AUTHENTICATED_OWNER41_MERGE_NEWLINE_SOURCE_HASH_TRANSITION;FAIL_RETAINED'
                authority='Exact sealed41 original bytes/current owner checkout authenticated before42 edits;not a new42 mutation.'
            else:raise AssertionError('UNEXPLAINED_CURRENT_ASSERTION_ARGUMENT_CHANGE')"""
assert needle in s;s=s.replace(needle,replacement).replace('Explicit current owner41 task','Explicit current owner42 task').replace('Authenticated accepted40 merged-main','Authenticated accepted41 merged-main').replace('authorized41 paths','authorized42 paths')
s=s.replace("original32=pin(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json'),historical_outcomes_retained=True","original32=pin(ROOT/'evidence/04A40/python/diagnostic-dispositions-v3.json'),original21=pin(ROOT/'evidence/04A41/python/diagnostic-dispositions-v1.json'),historical_outcomes_retained=True")
with (HERE/'python-dispositions-v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
print('All owner41 newline-source hash arguments now tied to exact pre42 sealed/current source pairs',flush=True)
