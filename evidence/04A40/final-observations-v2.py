"""Sealed current-main48 driver, independently replayed for corrected binaries."""
from common40_v3 import *
import shutil,copy
lock()
for config in ['Debug','Release']:
    folder=PRIVATE/('fixtures-final2-'+config);folder.mkdir()
    for p in (PRIVATE/'final2'/config/'lib').glob('*.dll'):shutil.copy2(p,folder/p.name)
    shutil.copy2(PRIVATE/'fixtures-before-v2/Observe40.exe',folder/'Observe40.exe')
    r=command('final2-'+config+'-48fixtures',[str(folder/'Observe40.exe'),str(folder/'observations.json'),'policy']);assert r['exit_code']==0
    before=load(PRIVATE/'before-v2.json');after=load(folder/'observations.json');assert len(before['rows'])==len(after['rows'])==48
    result=[]
    for a,b in zip(before['rows'],after['rows']):
        assert a['id']==b['id'];left=copy.deepcopy(a);right=copy.deepcopy(b);elapsed=False
        for row in [left,right]:
            state=(row.get('output') or {}).get('state',{})
            if 'GEE.CompTime' in state:state.pop('GEE.CompTime');elapsed=True
        assert left==right,'SUBSTANTIVE_CURRENT_MAIN_BEHAVIOR_DELTA'
        result.append(dict(id=a['id'],input_sha256=sha(json.dumps(a['input'],sort_keys=True).encode()),before_sha256=sha(json.dumps(a,sort_keys=True).encode()),after_sha256=sha(json.dumps(b,sort_keys=True).encode()),contract_exact=True,elapsed_field=elapsed))
    save('fixtures/'+config+'-48-final-v2.json',dict(cases=48,rows=result,statistical_input_exception_contracts_exact=True,before=pin(PRIVATE/'before-v2.json'),after=pin(folder/'observations.json'),
       excluded_field='GEE.CompTime only, Stopwatch.Elapsed source proof in waves/wave2-readback-v3.json; no baseline mutation or substantive normalization.',scientific='NOT_GOLDEN'))
    print(config,'48 corrected-final statistical/input/exception contracts exact',flush=True)
