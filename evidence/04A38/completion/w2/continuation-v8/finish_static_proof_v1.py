"""Bind all new portable declarations and unchanged text/host policies."""
from common import *
from collections import Counter
from verify_c8_composition_v2 import inspect
save('source-composition-v1.json',inspect())
rows=[]
for c in ['Debug','Release']:
    old=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v3/current-'+c+'.json')).read_bytes());now=json.loads((HERE/('metadata-all/packet-v1-context-v3/current-'+c+'.json')).read_bytes())
    def sites(data):return [(m['method'],i,op['op'],op['operand']) for m in data['methods'] for i,op in enumerate(m['instructions']) if isinstance(op['operand'],str) and op['operand'].startswith('METHOD:BESHStatNG.ExcelCommon.LegacyUdfText38 @ ')]
    assert Counter(sites(old))==Counter(sites(now))
    rows.append(dict(configuration=c,exact_policy_call_edges=len(sites(now)),edge_identity_sha256=sha(json.dumps(sorted(sites(now))).encode()),new_policy_edges=0,removed_policy_edges=0))
save('text-frontier-carryforward-v1.json',dict(status='EXACT_C7_POLICY_CALL_EDGES_AND_RESTRICTED_DOMAIN_RETAINED',configurations=rows,policy=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'),source_domain_proof=pin(W2/'continuation-v6/text-domain-proof-v1.json'),source_domains=pin(W2/'continuation-v6/text-caller-domains-v2.json'),synthetic_out_of_domain_counterexamples_per_configuration=8,observations_per_configuration=1746360,new_execution=False,scope='No LegacyUdfText caller/policy CIL change. New native enum bridge retains the old native Text Select; Common accepts only the explicit four enum outcomes. No global text-equivalence claim.'))
save('enum-bridge-source-domain-v1.json',dict(provider=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/UdfOptionPreparation38.vb'),native=pin(ROOT/'BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb'),prepared=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb'),range=['Nothing','superiority','noninferiority','equivalence'],bridge=['Invalid','Superiority','NonInferiority','Equivalence'],proof='Every provider Return is one of its three exact literals or Nothing; unchanged native AsString/accepted parser run lazily. The old native Text Select is retained by ResolveSampleSizeHypothesisKind38; Common switches on Integer-backed enum only. Existing Legacy policy caller CIL/source restrictions and eight synthetic counterexamples remain exact, not generalized.',original_queries=0))
measurement=json.loads((HERE/'unrelated-static-field-order-measurement-v1.json').read_bytes())
save('unrelated-field-source-qualification-v1.json',dict(source=pin(ROOT/'BESHstatNG/src/DM/PSMdataImport.vb'),measurement=pin(HERE/'unrelated-static-field-order-measurement-v1.json'),changed_order_configurations=[r['configuration'] for r in measurement['rows'] if r['changed_order']],qualification='Named static field types/flags and all constructor/method CIL preserved; observed enumeration order is retained as a distinct raw measurement, outside every claimed packet path. No PSM source or capture was changed.',original_queries=0))
source=(HERE/'verify_c8_checkpoint_template_v4.py').read_text(encoding='utf-8')
needle='    dispositions=[]\n'
addition="""    portable=load(PREFIX+'portable-api-contracts-v1.json');pu=load(PREFIX+'portable-universe-v1.json')
    need(len(portable['contracts'])==len(pu['members'])==15 and portable['public_preparation_apis']==12,'Dropped portable declaration')
    need({r['source_record']['symbol_id'] for r in portable['contracts']}=={n['symbol_id'] for n in pu['members']},'Portable declaration identity mismatch')
    for s in pu['sources']:check(s)
    for row in portable['contracts']:
        n=next(n for n in pu['members'] if n['symbol_id']==row['source_record']['symbol_id']);need(row['source_record']==n,'Changed portable source/span')
        check(row['source']);text,view=body(raw(n['path']),n);need((text,view)==(row['whole_body'],row['source_view']),'Portable full body differs')
        need(row['fragments']==[f for f in pu['fragments'] if f['member_id']==n['symbol_id']],'Missing portable branch/inline')
        for bound in row['compiled']:
            meta=contexts[bound['configuration']];defs=definitions(meta);method=bound['method']
            need(defs.get(method['method'])==method and (method['declaring_type'],method['member_name'])==(n['owner'],n['name']),'Wrong portable compiled declaration')
            context=next(t for t in meta['types'] if t['type']==n['owner']);need(bound['declaring_type']==context and context['fields']==[],'New mutable portable state')
            if n['signature'].startswith('Public Shared'):
                callers=[m for m in meta['methods'] if assembly_of_method(m['method'])=='BESHStatNG' and any(i['op']=='call' and i['operand']=='METHOD:'+method['method'] for i in m['instructions'])]
                need(callers,'Unused extracted portable API lacks a real Windows consumer')
    need(len(portable['enum_types'])==2,'Dropped enum contract')
    for row in portable['enum_types']:
        for bound in row['compiled']:
            context=next(t for t in contexts[bound['configuration']]['types'] if t['type']=='BESHStatNG.ExcelCommon.'+row['name'])
            need(bound['context']==context and {f['name']:f['literal'] for f in context['fields'] if f['is_literal']}==row['values'],'Changed enum representation')
    dispositions=[]
"""
assert needle in source;source=source.replace(needle,addition)
with (HERE/'verify_c8_checkpoint_template_v5.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('static-proof-authority-v1.json',dict(reader=pin(HERE/'verify_c8_checkpoint_template_v5.py'),source_composition=pin(HERE/'source-composition-v1.json'),portable=pin(HERE/'portable-api-contracts-v1.json'),historical_callback=pin(HERE/'historical-callback-contracts-v1.json'),text_frontier=pin(HERE/'text-frontier-carryforward-v1.json'),enum_domain=pin(HERE/'enum-bridge-source-domain-v1.json'),unrelated_field_measurement=pin(HERE/'unrelated-field-source-qualification-v1.json'),original_queries=0))
print('Complete source/portable/enum/text/field authorities pinned; no new original queries')
