"""Explicit measured-time distinction; all statistical/state/input/exception leaves stay exact."""
from common40_v3 import *
import copy
before=load(PRIVATE/'before-v2.json');after=load(PRIVATE/'after-wave2-v2.json')
def differences(a,b,path=()):
    if type(a)!=type(b):return [path]
    if isinstance(a,dict):
        if set(a)!=set(b):return [path]
        return [p for k in a for p in differences(a[k],b[k],path+(k,))]
    if isinstance(a,list):
        if len(a)!=len(b):return [path]
        return [p for i,(x,y) in enumerate(zip(a,b)) for p in differences(x,y,path+(i,))]
    return [] if a==b else [path]
diff=differences(before,after)
assert len(diff)==4 and all(p[0]=='rows' and p[2:]==('output','state','GEE.CompTime','bits') for p in diff)
ids=[before['rows'][p[1]]['id'] for p in diff]
assert set(ids)=={'/GEEfit/False/False','/GEEfit/False/True','sk-SK/GEEfit/False/False','sk-SK/GEEfit/False/True'}
left=copy.deepcopy(before);right=copy.deepcopy(after)
for p in diff:
    del left['rows'][p[1]]['output']['state']['GEE.CompTime']
    del right['rows'][p[1]]['output']['state']['GEE.CompTime']
assert left==right
source=(ROOT/'BESHStat.Core/src/RegModels/GEE.vb').read_text(encoding='utf-8-sig')
timing=[dict(line=n+1,text=line.strip()) for n,line in enumerate(source.splitlines()) if 'CompTime' in line]
assert any('Elapsed' in r['text'] for r in timing)
tests=load(PRIVATE/'wave2-Common-tests-v2.json');assert tests['outcomes']=={'Passed':12} and tests['command']['exit_code']==0
save('waves/wave2-readback-v3.json',dict(status='48_STATISTICAL_CONTRACTS_EXACT;4_ELAPSED_TIME_FIELDS_DIFFER',
    before=pin(PRIVATE/'before-v2.json'),after=pin(PRIVATE/'after-wave2-v2.json'),cases=48,fully_exact_cases=44,elapsed_time_only_cases=4,
    differences=[list(p) for p in diff],source_timing_proof=dict(source=pin('BESHStat.Core/src/RegModels/GEE.vb'),lines=timing),
    statistical_or_input_or_exception_differences=0,focused_tests=12,outcomes=tests['outcomes'],
    raw_reference_unchanged=True,report_only_correction='Original strict whole-object comparison stopped on measured runtime. No value, raw baseline, test or expected estimator output was changed. Subsequent comparisons exclude only this exact source-backed elapsed-time field.',
    inherited_GEE_start_exceptions=4,scientific='NOT_GOLDEN'))
print('Wave2:12 PASS;48 statistical/input/exception contracts exact;4 measured GEE.CompTime leaves differ, source proven. No rerun.',flush=True)
