"""Synthetic retained-result validation controls; no historical scanner execution."""
from common import *
from retained_scan_cache_v16_v2 import validated_report
import copy
authority=dict(policy='synthetic-authority',hash_seed='38')
first=dict(index=0,path='one.txt',bytes=1,sha256='1'*64,findings=[],reviewed=[])
failed=dict(path='two.bin',bytes=2,sha256='2'*64)
plan=dict(authority=authority,rows=[{k:first[k] for k in ['index','path','bytes','sha256']},dict(index=1,**failed)])
report=dict(authority=authority,rows=[first],complete=False,exit_code=1,error='UnicodeDecodeError: exact synthetic error precedence')
historical={'one.txt':{k:first[k] for k in ['path','bytes','sha256']}}
assert validated_report(report,plan,failed,historical,authority)==[first]
controls=[]
for label,mutate in [('wrong authority',lambda r,p:r.update(authority={})),('duplicate row',lambda r,p:r['rows'].append(r['rows'][0])),('changed SHA',lambda r,p:r['rows'][0].update(sha256='3'*64)),('missing prefix',lambda r,p:r.update(rows=[])),('false complete',lambda r,p:r.update(complete=True,exit_code=0,error=None)),('translated exception',lambda r,p:r.update(error='AssertionError: helper failed')),('wrong failed file',lambda r,p:p['rows'][1].update(path='elsewhere.bin'))]:
 r=copy.deepcopy(report);p=copy.deepcopy(plan);mutate(r,p)
 try:validated_report(r,p,failed,historical,authority)
 except (AssertionError,KeyError):controls.append(dict(name=label,status='REJECTED'))
 else:raise AssertionError('Invalid cached result accepted '+label)
save('scanner-cache-controls-v2.json',dict(status='ACTUAL_V16_CACHE_VALIDATOR_SYNTHETIC_POSITIVE_AND_ADVERSE_CONTROLS_PASS',kernel=pin(HERE/'retained_scan_cache_v16_v2.py'),controls=controls,historical_scanner_calls=0,Python_parent_tests_run=0,bulk_helper_jobs=0,original_queries=0,scope='Actual validator used by the ordinary controlled scanner lane. Synthetic rows only; real retained5829-row/source/environment/byte binding occurs once inside that lane. Failed binary remains uncached and original assertions/exceptions execute unchanged.'))
print('Synthetic cache validation controls PASS',len(controls),'no historical scanner or test parent executed',flush=True)
