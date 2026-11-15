"""Targeted Release uses the sealed pre-edit driver and exact field-specific comparator."""
from common40_v3 import *
import shutil,copy
lock();obs=PRIVATE/'fixtures-final-Release';obs.mkdir()
for p in (PRIVATE/'Release/lib').glob('*.dll'):shutil.copy2(p,obs/p.name)
shutil.copy2(PRIVATE/'fixtures-before-v2/Observe40.exe',obs/'Observe40.exe')
receipt=command('final-Release-48fixtures',[str(obs/'Observe40.exe'),str(obs/'observations.json'),'policy']);assert receipt['exit_code']==0
before=load(PRIVATE/'before-v2.json');after=load(obs/'observations.json');assert len(before['rows'])==len(after['rows'])==48
rows=[]
for a,b in zip(before['rows'],after['rows']):
    assert a['id']==b['id'] and a['input']==b['input']
    left=copy.deepcopy(a);right=copy.deepcopy(b);changed=[]
    for x in (left,right):
        state=x.get('output',{}).get('state',{})
        if 'GEE.CompTime' in state:changed.append(state.pop('GEE.CompTime'))
    assert left==right,'SUBSTANTIVE_CURRENT_MAIN_BEHAVIOR_DELTA'
    rows.append(dict(id=a['id'],input_sha256=sha(json.dumps(a['input'],sort_keys=True).encode()),before_sha256=sha(json.dumps(a,sort_keys=True).encode()),after_sha256=sha(json.dumps(b,sort_keys=True).encode()),contract_exact=True,elapsed_field_present=bool(changed)))
save('fixtures/Release-48-final-v1.json',dict(cases=48,statistical_input_exception_contracts_exact=True,rows=rows,before=pin(PRIVATE/'before-v2.json'),after=pin(obs/'observations.json'),raw_unchanged=True,exclusion='Only source-proven GEE.CompTime elapsed runtime; no output/statistical/exception/input normalization.'))
print('Release48 current-main statistical/input/exception contracts exact',flush=True)
