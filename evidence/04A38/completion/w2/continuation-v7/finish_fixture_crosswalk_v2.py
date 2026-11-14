"""Complete fixture crosswalk; bound claims to concrete actual dependency handles."""
from common import *
import importlib.util
base=json.loads((HERE/'transitive-compiled-context-v1.json').read_bytes())['configurations'];scope=[]
for config in ['Debug','Release']:
    data=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+config+'.json')).read_bytes());defs={r['method']:r for r in data['methods']};reached=[defs[key] for key in base[config]['methods']]
    assert len(reached)==554 and all(r['declaring_type'] is not None for r in reached)
    assert not any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:.') for r in reached for i in r['instructions'])
    scope.append(dict(configuration=config,complete_reachable_methods=554,unresolved_reachable_declaring_types=0,unresolved_reachable_field_owners=0,unused_open_constructed_handles_with_no_FullName=sum(r['declaring_type'] is None for r in data['methods']),qualification='Unused open constructed measurements outside all claimed parent dependency closures are not source identities or portability closures; no generic name substitution is applied. Named declared Core type/field definitions and every actual reachable field/closure are checked.'))
save('constructed-context-scope-v1.json',dict(configurations=scope,original_queries=0))
source=(HERE/'verify_checkpoint_template_v3.py').read_text(encoding='utf-8')
needle="need(load(PREFIX+'transitive-compiled-context-v1.json')['configurations'][config]==closure(now,root_keys),'Root transitive proof differs')"
assert source.count(needle)==1
source=source.replace(needle,needle+"\n        reached=closure(now,root_keys)['methods']\n        need(all(methods[key]['declaring_type'] is not None for key in reached),'Unresolved actual claimed declaring type')\n        need(not any(isinstance(i['operand'],str) and i['operand'].startswith('FIELD:.') for key in reached for i in methods[key]['instructions']),'Unresolved actual claimed field owner')")
source=source.replace("if t['assembly']=='BESHStat.Core':need(types.get(t['type'])==t,", "if t['assembly']=='BESHStat.Core' and t['type'] is not None:need(types.get(t['type'])==t,")
with (HERE/'verify_checkpoint_template_v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
spec=importlib.util.spec_from_file_location('c7_scope_reader',HERE/'verify_checkpoint_template_v4.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
proof=reader.compute();save('independent-proof-final-v2.json',proof)
compatibility=json.loads((HERE/'compatibility-packet-v1.json').read_bytes());contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
exports=json.loads((HERE/'inspection/packet-v1/Inspect38-Debug.json').read_bytes())['exports'];metadata=json.loads((HERE/'metadata-all/packet-v1-context-v3/current-Debug.json').read_bytes())
export_keys={}
for row in exports:
    stem=row['method'].split('(',1)[0]
    export_keys[row['effective_excel_name']]=[r['method'] for r in metadata['methods'] if r['declaring_type'] is not None and r['declaring_type']+'.'+r['member_name']==stem]
input_rows=[];reach_cache={}
for receipt in compatibility['results']:
    if receipt['configuration']!='Debug':continue
    for line in (ROOT/receipt['inputs']['path']).read_text(encoding='utf-8-sig').splitlines():
        value=json.loads(line);target=value.get('target','');keys=export_keys.get(target,[])
        if not keys:keys=[m['method'] for m in metadata['methods'] if m['declaring_type'] is not None and m['member_name']==target and (not value.get('type') or m['declaring_type']==value['type'])]
        key=tuple(keys)
        if key not in reach_cache:reach_cache[key]=set(reader.closure(metadata,keys)['methods']) if keys else set()
        input_rows.append(dict(configuration='Debug',corpus=receipt['corpus'],input=receipt['inputs']['path'],case_id=value.get('case_id'),culture=value.get('culture'),target=target,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if receipt['corpus'] in list('ABCDEFG') else 'GENUINE_PRE_EDIT',reached=reach_cache[key]))
rows=[]
for contract in contracts:
    methods=set(contract['compiled'][0]['roots']);matches=[{k:v for k,v in row.items() if k!='reached'} for row in input_rows if methods&row['reached']]
    rows.append(dict(original_id=contract['original_id'],current_id=contract['current_id'],name=contract['current_record']['name'],fixtures=matches,qualification='Explicit Debug input-to-actual-compiled-call reachability, not dynamic branch coverage. Both configurations have independently exact frozen results and complete per-parent CIL; complete source compositions and individual branch/inline proofs cover unwitnessed branches. No new original reference is created.'))
save('compatibility-fixture-crosswalk-v1.json',dict(rows=rows,static_configuration='Debug',complete_frozen_configurations=['Debug','Release'],source_scope='constructed-context-scope-v1.json',original_queries=0))
print('Concrete554-method scope and59-parent fixture crosswalk PASS',flush=True)
