"""Assertion-preserving readback: bound reflection ordering in three all-static generated closure types."""
from common39 import *
import copy
import shutil

source_lock()
old=load(W3/'abi-cil/review-metadata/Debug/current.json')
new=load(HERE/'abi-cil/Debug/current-v1.json')
previous=load(HERE/'abi-cil/Debug-comparison-v2.json')
assert not previous['normalized_method_changes'] and previous['exact_exports'] and previous['exports_count']==268
a={r['type']:r for r in old['types']};b={r['type']:r for r in new['types']}
assert a.keys()==b.keys()
differences=[]
for name in a:
    if a[name]==b[name]:continue
    x=copy.deepcopy(a[name]);y=copy.deepcopy(b[name])
    assert '_Closure$__' in name and x['assembly']=='BESHStat.Core'
    assert all(f['is_static'] for f in x['fields']+y['fields'])
    before=x['fields'];after=y['fields']
    x['fields']=sorted(before,key=lambda f:f['name']);y['fields']=sorted(after,key=lambda f:f['name'])
    assert x==y, 'TYPE_CONTRACT_CHANGE_BEYOND_STATIC_FIELD_ORDER'
    differences.append(dict(type=name,before=before,after=after,reason='Reflection enumeration order of identical all-static compiler-generated fields; every field name/type/flag/value retained; all per-method signatures/instructions/exception metadata exact.'))
assert len(differences)==3
save('abi-cil/Debug-comparison-v3.json',dict(previous,types_exact=False,types_contract_exact=True,
    reflection_static_field_order_differences=differences,report_recovery='Preserved v2 structural-order nonpass; no build/metadata execution repeated. This does not normalize instance layout, flags, fields, method calls or any instruction.'))
folder=PRIVATE/'fixtures-after-Debug-v1';folder.mkdir(exist_ok=False)
for path in (PRIVATE/'shipping-final/Debug').glob('*.dll'):shutil.copy2(path,folder/path.name)
exe=ROOT/load(HERE/'fixtures/frozen-input-manifest-v2.json')['driver_executable']['path']
shutil.copy2(exe,folder/'Observe39.exe')
out=HERE/'fixtures/after-Debug-v1.json'
receipt=command('Debug-fixtures',[str(folder/'Observe39.exe'),str(out)])
before=load(HERE/'fixtures/before-v2.json');after=load(out)
save('fixtures/Debug-comparison-v1.json',dict(configuration='Debug',command=receipt,observed=pin(out),
    before=pin(HERE/'fixtures/before-v2.json'),cases=len(after['rows']),exact=before==after,
    case_rows=[dict(id=a['id'],before_sha256=sha(json.dumps(a,sort_keys=True,ensure_ascii=False).encode()),after_sha256=sha(json.dumps(b,sort_keys=True,ensure_ascii=False).encode()),exact=a==b) for a,b in zip(before['rows'],after['rows'])],
    cases_with_expected_current_exceptions=[r['id'] for r in after['rows'] if r['outcome']=='EXCEPTION'],
    scientific='NOT_GOLDEN;NO_SCIENTIFIC_IMPACT_ESTIMATES',native='NOT_RUN_UNAUTHORIZED'))
assert receipt['exit_code']==0 and len(after['rows'])==77 and before==after
source_lock()
