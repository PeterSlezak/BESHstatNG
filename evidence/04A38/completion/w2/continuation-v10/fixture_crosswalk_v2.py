"""Both configurations, every genuine/late case and operation; static reachability only."""
from common import *
from proof_kernel_v2 import closure
from collections import Counter
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];comparisons=json.loads((HERE/'compatibility-packet-v2.json').read_bytes())['results'];all_inputs=[];rows=[];counts={}
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+config+'.json')).read_bytes());methods=meta['methods'];exports=json.loads((HERE/('inspection/packet-v2/Inspect38-'+config+'.json')).read_bytes())['exports'];keys={r['effective_excel_name']:[m['method'] for m in methods if m['declaring_type'] is not None and m['declaring_type']+'.'+m['member_name']==r['method'].split('(',1)[0]] for r in exports};cache={};inputs=[];case_keys=set();occurrences=Counter()
    for r in comparisons:
        if r['configuration']!=config:continue
        assert pin(ROOT/r['inputs']['path'])==r['inputs']
        cases=[json.loads(line) for line in (ROOT/r['inputs']['path']).read_text(encoding='utf-8-sig').splitlines() if line.strip()];assert len(cases)==r['cases']
        for case in cases:
            base=(r['corpus'],case['case_id']);occurrence=occurrences[base];occurrences[base]+=1;identity=(*base,occurrence);assert identity not in case_keys;case_keys.add(identity)
            ops=case.get('operations',[case]);ops=ops if isinstance(ops,list) else [ops]
            for number,op in enumerate(ops):
                target=op.get('target',case.get('target',''));roots=keys.get(target,[])
                if not roots:roots=[m['method'] for m in methods if m['member_name']==target and (not op.get('type',case.get('type')) or m['declaring_type']==op.get('type',case.get('type')))]
                key=tuple(roots)
                if key not in cache:cache[key]=set(closure(meta,roots)['methods']) if roots else set()
                inputs.append(dict(configuration=config,corpus=r['corpus'],input=r['inputs'],case_id=case['case_id'],occurrence=occurrence,operation=number,culture=case.get('culture'),target=target,target_roots=roots,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN',reached=cache[key]))
    assert len(case_keys)==29424;counts[config]=dict(cases=len(case_keys),operations=len(inputs),unbound_operations=sum(not r['target_roots'] for r in inputs))
    all_inputs.extend({k:v for k,v in r.items() if k!='reached'} for r in inputs)
    for c in contracts:
        root=next(v['method'] for v in c['compiled'] if v['configuration']==config);matches=[{k:v for k,v in r.items() if k!='reached'} for r in inputs if root in r['reached']]
        rows.append(dict(configuration=config,original_id=c['original_id'],current_id=c['current_id'],fixtures=matches,qualification='Static actual shipping call-graph potential reachability from the saved case/operation target. Not proof a conditional branch, callback, native edge or every graph node executed. Finite current-only output comparisons are separate. Unwitnessed paths retain full source/CIL proof and explicit dynamic limitation.'))
save('compatibility-fixture-crosswalk-v2.json',dict(rows=rows,case_operation_inventory=all_inputs,counts=counts,original_queries=0,supersedes_limited_v1=pin(HERE/'compatibility-fixture-crosswalk-v1.json'),scope='Complete saved input multiset both configurations, including nested late operations; identities/typed-input pins/labels preserved. V1 mapped only top-level Debug targets, an honest limited draft retained, not final comprehensive authority.'))
print('Both-configuration operation fixture crosswalk',json.dumps(counts))
