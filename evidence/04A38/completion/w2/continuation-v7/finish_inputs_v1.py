"""Finalize proof accounting after the scanner lane releases its input view."""
from common import *
from collections import Counter
import re,importlib.util

source=(HERE/'verify_checkpoint_template_v2.py').read_text(encoding='utf-8')
source=source.replace('newly_supported_current=sorted(newcurrent),','newly_supported_current=sorted(closed-inherited),newly_supported_function_bodies=sorted(newcurrent),newly_supported_existing_callbacks=sorted({r[\'current_id\'] for r in callbacks}-inherited),')
source=source.replace("need(len(candidates)==1,'Changed/lost inherited complete source body');inherited.add(candidates[0]['symbol_id'])", "need(len(candidates)==1,'Changed/lost inherited complete source body');need(candidates[0]['body_sha256']==before['body_sha256'],'Changed inherited complete body bytes');inherited.add(candidates[0]['symbol_id'])")
# Final source reader also independently recounts validation when its ledgers exist.
source=source.replace("\n    return result\n\nif __name__", "\n    if check_ledgers:\n        from verify_validation_v1 import inspect as validation\n        result['validation']=validation(load,PREFIX,raw)\n    return result\n\nif __name__")
with (HERE/'verify_checkpoint_template_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
spec=importlib.util.spec_from_file_location('c7_final_draft',HERE/'verify_checkpoint_template_v3.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
proof=reader.compute();assert len(proof['newly_supported_current'])==33 and len(proof['newly_supported_existing_callbacks'])==6
save('independent-proof-final-v1.json',proof)
from verify_composition_v1 import inspect as composition
save('source-composition-v1.json',composition())

# Exact C6 text-policy producer/caller CIL proves the frontier did not change.
rows=[]
for configuration in ['Debug','Release']:
    before=json.loads((PREVIOUS/('metadata-full/families-v2/current-'+configuration+'.json')).read_bytes())
    after=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+configuration+'.json')).read_bytes())
    def sites(data):
        return [(r['method'],i,ins['op'],ins['operand']) for r in data['methods'] for i,ins in enumerate(r['instructions']) if isinstance(ins['operand'],str) and ins['operand'].startswith('METHOD:BESHStatNG.ExcelCommon.LegacyUdfText38 @ ')]
    assert Counter(sites(before))==Counter(sites(after))
    rows.append(dict(configuration=configuration,exact_policy_call_edges=len(sites(after)),edge_identity_sha256=sha(json.dumps(sorted(sites(after))).encode()),new_policy_edges=0,removed_policy_edges=0))
save('text-frontier-carryforward-v1.json',dict(status='EXACT_POLICY_AND_CALLER_METHOD_CIL_NO_FRONTIER_CHANGE',configurations=rows,policy=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'),source_domain_proof=pin(PREVIOUS/'text-domain-proof-v1.json'),source_domains=pin(PREVIOUS/'text-caller-domains-v2.json'),synthetic_out_of_domain_counterexamples_per_configuration=8,observations_per_configuration=1746360,source_domain_differences=0,new_execution=False,scope='Existing authenticated C6 domain proof is carried by exact policy/caller CIL and unchanged native conversion methods. Neither a new domain nor a global text equivalence is asserted.'))

retained=[]
for row in json.loads((PREVIOUS/'command-receipts-v1.json').read_bytes())['commands']:
    for name in ['stdout','stderr']:
        path=RUNROOT/'commands'/row['name']/(name+'.log');data=path.read_bytes();assert sha(data)==row[name+'_sha256']
        retained.append(pin(path))
save('v6-raw-command-preservation-v1.json',dict(commands=281,streams=562,all_exact=True,records=retained,source_receipt=pin(PREVIOUS/'command-receipts-v1.json')))

# Explicit frozen provenance remains distinct from independent scientific truth.
compatibility=json.loads((HERE/'compatibility-packet-v1.json').read_bytes())
late={key:count for key,count in [('A',432),('B',12),('C',2),('D',4),('E',2),('F',2),('G',4)]}
save('frozen-provenance-v1.json',dict(configurations=[dict(configuration=c,genuine_pre_edit=28966,late=[dict(authority=k,cases=v,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT') for k,v in late.items()],total=29424,all_exact=True) for c in ['Debug','Release']],comparison=pin(HERE/'compatibility-packet-v1.json'),original_plain_repeat_trace_typed_branch_and_failed_attempts='Immutable C6 manifest/protection graph preserved; no original execution, compilation, fit, query or supplemental oracle',new_original_queries=0,scientific_truth='NOT_GOLDEN'))

# Per-parent exact frozen fixture-to-target crosswalk, including indirect helpers.
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
exports=json.loads((HERE/'inspection/packet-v1/Inspect38-Debug.json').read_bytes())['exports']
metadata=json.loads((HERE/'metadata-all/packet-v1-context-v3/current-Debug.json').read_bytes())
defs={r['method']:r for r in metadata['methods']}
export_keys={}
for row in exports:
    stem=row['method'].split('(',1)[0]
    export_keys[row['effective_excel_name']]=[r['method'] for r in metadata['methods'] if r['declaring_type']+'.'+r['member_name']==stem]
input_rows=[];reach_cache={}
for r in compatibility['results']:
    if r['configuration']!='Debug':continue
    for line in (ROOT/r['inputs']['path']).read_text(encoding='utf-8-sig').splitlines():
        value=json.loads(line);target=value.get('target','');keys=export_keys.get(target,[])
        if not keys:
            keys=[m['method'] for m in metadata['methods'] if m['member_name']==target and (not value.get('type') or m['declaring_type']==value['type'])]
        cache_key=tuple(keys)
        if cache_key not in reach_cache:reach_cache[cache_key]=set(reader.closure(metadata,keys)['methods']) if keys else set()
        reached=reach_cache[cache_key]
        input_rows.append(dict(corpus=r['corpus'],input=r['inputs']['path'],case_id=value.get('case_id'),culture=value.get('culture'),target=target,classification='LATE_ARCHIVED_ENTRY_SUPPLEMENT' if r['corpus'] in late else 'GENUINE_PRE_EDIT',reached=reached))
crosswalk=[]
for contract in contracts:
    current_methods=set(contract['compiled'][0]['roots'])
    matches=[{k:v for k,v in r.items() if k!='reached'} for r in input_rows if current_methods&r['reached']]
    crosswalk.append(dict(original_id=contract['original_id'],current_id=contract['current_id'],name=contract['current_record']['name'],fixtures=matches,qualification='Actual static compiled call reachability is a fixture-to-dependency crosswalk, not a claim that every branch executed in every fixture. Complete original/current source compositions and branch/inline inventories plus unchanged transitive CIL support branches not dynamically witnessed. No new archived reference is manufactured.'))
save('compatibility-fixture-crosswalk-v1.json',dict(rows=crosswalk,original_queries=0))
print(json.dumps({k:proof[k] for k in ['historical','current','support','fragments']}))
