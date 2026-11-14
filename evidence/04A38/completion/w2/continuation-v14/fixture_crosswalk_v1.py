"""All dual-config cases/operations, compact shared identities instead of repeated payloads."""
from common import *
from proof_kernel_v8 import closure
from collections import Counter
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
comparisons=json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results']
operations=[];rows=[];counts={};catalog=[]
for config in ['Debug','Release']:
    meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json')).read_bytes());methods=meta['methods'];methodkeys=sorted(m['method'] for m in methods);methodindex={k:i for i,k in enumerate(methodkeys)}
    exports=json.loads((HERE/('inspection/packet-v1/Inspect38-'+config+'.json')).read_bytes())['exports']
    keys={r['effective_excel_name']:[m['method'] for m in methods if m['declaring_type'] is not None and m['declaring_type']+'.'+m['member_name']==r['method'].split('(',1)[0]] for r in exports}
    cache={};inputs=[];case_keys=set();occurrences=Counter();config_start=len(operations)
    for record in comparisons:
        if record['configuration']!=config:continue
        assert pin(ROOT/record['inputs']['path'])==record['inputs']
        cases=[json.loads(line) for line in (ROOT/record['inputs']['path']).read_text(encoding='utf-8-sig').splitlines() if line.strip()]
        assert len(cases)==record['cases']
        for case in cases:
            base=(record['corpus'],case['case_id']);occurrence=occurrences[base];occurrences[base]+=1
            identity=(*base,occurrence);assert identity not in case_keys;case_keys.add(identity)
            ops=case.get('operations',[case]);ops=ops if isinstance(ops,list) else [ops]
            for number,op in enumerate(ops):
                target=op.get('target',case.get('target',''));roots=keys.get(target,[])
                if not roots:roots=[m['method'] for m in methods if m['member_name']==target and (not op.get('type',case.get('type')) or m['declaring_type']==op.get('type',case.get('type')))]
                key=tuple(roots)
                if key not in cache:
                    graph=closure(meta,roots) if roots else dict(methods=[],external_runtime_or_ExcelDna=[],edges=[])
                    cache[key]=dict(catalog_index=len(catalog),reached=set(graph['methods']))
                    catalog.append(dict(configuration=config,target_roots=roots,reached_method_indexes=[methodindex[k] for k in graph['methods']],metadata=pin(HERE/('metadata-all/packet-v1-context-v5/current-'+config+'.json'))))
                value=dict(configuration=config,corpus=record['corpus'],input=record['inputs'],case_id=case['case_id'],occurrence=occurrence,operation=number,culture=case.get('culture'),target=target,
                    target_roots=roots,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if record['corpus'] in list('ABCDEFG') else 'PRE_EDIT_FROZEN',graph_index=cache[key]['catalog_index'])
                index=len(operations);operations.append(value);inputs.append((index,cache[key]['reached']))
    assert len(case_keys)==29424
    counts[config]=dict(cases=len(case_keys),operations=len(inputs),unbound_operations=sum(not operations[index]['target_roots'] for index,_ in inputs))
    assert counts[config]['operations']==29594 and counts[config]['unbound_operations']==0
    for contract in contracts:
        method=next(r['method'] for r in contract['compiled'] if r['configuration']==config)
        rows.append(dict(configuration=config,original_id=contract['original_id'],current_id=contract['current_id'],operation_indexes=[index for index,reached in inputs if method in reached],
            qualification='Static potential reachability only. No inference that a branch, callback, native acquisition edge or every reached method executed. Full source/CIL covers unwitnessed dynamic arms.'))
save('compatibility-fixture-crosswalk-v1.json',dict(schema='C14_SHARED_COMPLETE_OPERATION_INVENTORY_V1',rows=rows,case_operation_inventory=operations,compiled_graph_catalog=catalog,method_catalogs={c:sorted(m['method'] for m in json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+c+'.json')).read_bytes())['methods']) for c in ['Debug','Release']},counts=counts,
    predecessor=pin(PREVIOUS/'compatibility-fixture-crosswalk-v1.json'),original_queries=0,
    scope='Every genuine/approved-late case and nested operation exactly once/config. Integer references preserve the same complete mapping without hundreds of MiB of repeated fixture dictionaries; the old C13 file remains unchanged.'))
print(json.dumps(dict(counts=counts,operations=len(operations),native_contract_rows=len(rows),compiled_graphs=len(catalog))))
