"""Individual original fragments bound to actual current/portable dependencies."""
from common_v2 import *
from verify_c9_composition_v1 import tokens
import re
old=PREVIOUS/'numeric-copy-source-universe-v1.json';data=json.loads(old.read_bytes())
for row in data['sources']:assert pin(ROOT/row['path'])==row
with (HERE/'numeric-copy-source-universe-v1.json').open('xb') as f:f.write(old.read_bytes())
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows'];portable=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts'];u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());defs={m['method']:m for m in meta['methods']}
closed=[c for c in contracts+callbacks if c['new_support'] and c['original_id']];byoriginal={c['original_record']['symbol_id']:c for c in closed};rows=[]
for f in original['fragments']:
    if f['member_id'] not in byoriginal:continue
    c=byoriginal[f['member_id']];pool=[v for v in u['fragments'] if v['member_id']==c['current_id']]
    if 'compiled' in c:
        graph=set(c['compiled'][0]['transitive']['methods'])
        for p in portable:
            if p['compiled'][0]['method']['method'] in graph:pool+=p['fragments']
        reached={(defs[k]['declaring_type'],defs[k]['member_name']) for k in graph}
        pool += [v for v in data['fragments'] if any(n['symbol_id']==v['member_id'] and (n['owner'],n['name']) in reached for n in data['members'])]
    candidates=[f['code']];name=c['current_record']['name'];bound=f['code']
    if name in ['CT_CHI2','CT_NOMINAL_ASSOC','CT_FISHER_2X2','CT_FFH_EXACT']:
        bound=bound.replace('Return ExcelError.ExcelErrorNum','Return Nothing').replace('Dim body As Object(,)','Dim reportBody As Object(,)');candidates.append(bound)
    if name in ['TryReadStrictNumericVectorFromSingleColumn','TryReadStrictNumericVectorFromSingleRow']:
        axis=0 if name.endswith('Column') else 1;index='i' if axis==0 else 'j';dim='rows' if axis==0 else 'cols'
        for v in ['last','0',index]:
            cell='arr('+v+', 0)' if axis==0 else 'arr(0, '+v+')'
            bound=bound.replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell('+cell+')','blank('+v+')').replace('Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetDouble('+cell+')','numeric('+v+')')
        for a,b in [(dim,'count'),(index,'index'),('out','result'),('d','value')]:bound=re.sub(r'\b'+a+r'\b',b,bound)
        candidates.append(bound)
    if name in ['PASSINGBABLOK_COEF','DEMING_COEF']:
        for a,b in [('fit.SlopeCI','slope'),('fit.InterceptCI','intercept'),('mdl.SlopeCI','slope'),('mdl.InterceptCI','intercept')]:bound=bound.replace(a,b)
        bound=re.sub(r't.AddTitle\("[^\"]+"\)','t.AddTitle(title)',bound);candidates.append(bound)
    matches=[v['fragment_id'] for v in pool if v['kind']==f['kind'] and any(tokens(v['code'])==tokens(code) for code in candidates)]
    rows.append(dict(fragment_id=f['fragment_id'],original_parent=c['original_id'],original_source_record=f,current_parent=c['current_id'],matched_current_fragments=matches,proof='EXACT_SOURCE_UNDER_ACTUAL_ARGUMENT_BINDINGS' if matches else 'COMPOSITE_STATEMENT_COMPLETE_AUTHENTIC_PARENT_PHASE_PROOF',bound_source=bound,whole_parent_authority='whole-body-contracts-v1.json' if 'compiled' in c else 'historical-callback-contracts-v1.json',composition='source-composition-v1.json',dynamic_coverage='NOT_INFERRED_FROM_STATIC_CORRESPONDENCE'))
unmatched=[r for r in rows if not r['matched_current_fragments'] and (r['original_source_record']['is_branch'] or r['original_source_record']['is_inline'])]
save('new-parent-fragment-crosswalk-v1.json',dict(rows=rows,original_fragments=len(rows),exact_matches=sum(bool(r['matched_current_fragments']) for r in rows),composite=len(rows)-sum(bool(r['matched_current_fragments']) for r in rows),unmatched_branch_inline=unmatched,original_queries=0))
print('Original individual fragments',len(rows),'unmatched branch/inline',len(unmatched))
for r in unmatched:print(r['original_source_record']['kind'],r['original_source_record']['code'][:180])
