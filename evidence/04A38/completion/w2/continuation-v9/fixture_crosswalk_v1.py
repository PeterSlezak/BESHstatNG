"""Actual-source static fixture reachability; never infer dynamic branch coverage."""
from common_v2 import *
from proof_kernel_v1 import closure
meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());methods=meta['methods'];exports=json.loads((HERE/'inspection/packet-v1/Inspect38-Debug.json').read_bytes())['exports'];keys={r['effective_excel_name']:[m['method'] for m in methods if m['declaring_type'] is not None and m['declaring_type']+'.'+m['member_name']==r['method'].split('(',1)[0]] for r in exports};cache={};inputs=[]
for r in json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results']:
    if r['configuration']!='Debug':continue
    for line in (ROOT/r['inputs']['path']).read_text(encoding='utf-8-sig').splitlines():
        case=json.loads(line);target=case.get('target','');roots=keys.get(target,[])
        if not roots:roots=[m['method'] for m in methods if m['member_name']==target and (not case.get('type') or m['declaring_type']==case['type'])]
        key=tuple(roots)
        if key not in cache:cache[key]=set(closure(meta,roots)['methods']) if roots else set()
        inputs.append(dict(corpus=r['corpus'],input=r['inputs']['path'],case_id=case.get('case_id'),culture=case.get('culture'),target=target,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN',reached=cache[key]))
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];rows=[]
for c in contracts:
    root=c['compiled'][0]['method'];matches=[{k:v for k,v in r.items() if k!='reached'} for r in inputs if root in r['reached']]
    rows.append(dict(original_id=c['original_id'],current_id=c['current_id'],fixtures=matches,qualification='Actual compiled shipping source call reachability mapped to the same current-source safe replay target. Does not assert every guard or callback branch executed, nor native Excel or independent scientific truth. Full source/CIL/composition covers unwitnessed paths.'))
save('compatibility-fixture-crosswalk-v1.json',dict(rows=rows,original_queries=0,negative_and_edge_tests=dict(Common='UdfContinuation9Tests18 methods',Windows='Continuation9 six methods; first lower-bound assumption corrected from unchanged transport source',reader_controls='independent-controls-v1.json')))
print('Fixture crosswalk complete',len(rows))
