"""Resolve the seven shared-copy branch/inline correspondences by actual call bindings."""
from common import *
from verify_c8_composition_v2 import tokens
import re
path='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'
r=command('existing-numeric-copy-source-census-v1',['dotnet',str(PRIVATE/'portable-census-v1/bin/Census.dll'),str(ROOT),str(HERE/'numeric-copy-source-universe-v1.json'),path]);assert r['exit_code']==0
base=json.loads((HERE/'new-parent-fragment-crosswalk-v1.json').read_bytes());p=json.loads((HERE/'portable-universe-v1.json').read_bytes());numeric=json.loads((HERE/'numeric-copy-source-universe-v1.json').read_bytes());moves=json.loads((HERE/'import-layout-extraction-v1.json').read_bytes())['rows']
hist={r['original_id']:r for r in json.loads((HERE/'new-closures-v1.json').read_bytes())['rows']};details={r['original_fragment']:r for r in base['branch_inline_composite_details']}
for row in base['rows']:
    if row['original_fragment'] not in details:continue
    original=details[row['original_fragment']];name=hist[row['original_parent']]['source_contract']['current_record']['name'];move=next(r for r in moves if r['native']==name)
    for splice in move['splices']:
        if splice['portable'].startswith('UdfNumericImportPreparation38.'):
            collection=numeric;helper='CopyTrimmedColumnPrepared';bindings=dict(source='arr',rows='expectedRows',column='col',startRow='startRow')
        else:collection=p;helper=splice['portable'];bindings=splice.get('bindings',{})
        member_ids={n['symbol_id'] for n in collection['members'] if n['name']==helper}
        for f in collection['fragments']:
            if f['member_id'] not in member_ids:continue
            code=f['code']
            for key,value in bindings.items():code=re.sub(r'\b'+key+r'\b',lambda m:value,code)
            if tokens(code)==tokens(original['original_code']):row['actual_current_source_matches'].append(dict(fragment_id=f['fragment_id'],path=f['path'],member_id=f['member_id'],actual_call_bindings=bindings,source_composition='source-composition-v1.json'));row['proof']='EXACT_SOURCE_FRAGMENT_UNDER_ACTUAL_CALL_ARGUMENT_BINDINGS'
    assert row['actual_current_source_matches'],row['original_fragment']
base['branch_inline_composite_details']=[];base['exact_fragment_matches']=sum(bool(r['actual_current_source_matches']) for r in base['rows']);base['composite_fragment_proofs']=sum(not r['actual_current_source_matches'] for r in base['rows'])
save('new-parent-fragment-crosswalk-v2.json',base)
assert all(r['actual_current_source_matches'] for r in base['rows'] if r['is_branch'] or r['is_inline'])
print(json.dumps(dict(original_fragments=len(base['rows']),exact=base['exact_fragment_matches'],every_branch_and_inline_has_actual_source_match=True)))
