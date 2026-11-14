"""Complete source bodies, actual constructed Debug/Release contracts, callbacks and dependency ownership."""
from common import *
from source_views import body, view
from proof_kernel_v8 import bind, definitions, closure, map_current, NESTED_CACHE_DECLARATIONS
from collections import defaultdict, Counter, deque
import re

DRAFT='--draft' in sys.argv
OUT=PRIVATE/'draft-proof-v3' if DRAFT else HERE
OUT.mkdir(parents=True,exist_ok=True)
load=lambda p:json.loads(p.read_bytes())
old=load(PREVIOUS/'current-ownership-v2.json')
history=load(PREVIOUS/'historical-ownership-v2.json')['members']
u=load(HERE/'final-v1/current-universe.json')
o=load(W2.parent/'w1/original-universe.json')
entry=load(HERE/'entry-queue-receipt-v1.json')
mapping=map_current(old['members'],u['members'])
reverse={n['symbol_id']:key for key,n in mapping.items()}
members={n['symbol_id']:n for n in u['members']}
prior={n['symbol_id']:n for n in old['members']}
original_members={n['symbol_id']:n for n in o['members']}
historical_for_current={key:h for h in history for key in h['current_ids']}
snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
shapes={r['symbol_id']:r for r in load(HERE/'final-v1/declaration-shapes.json')['rows']}
meta={cfg:load(HERE/('final-v1/current-'+cfg+'.json')) for cfg in ['Debug','Release']}
orig={cfg:load(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')) for cfg in meta}
before_meta={cfg:load(PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')) for cfg in meta}
old_returns={cfg:load(ROOT/('evidence/04A38/preedit/compiled-'+cfg+'.json'))['methods'] for cfg in meta}
defs={cfg:definitions(m) for cfg,m in meta.items()}
od={cfg:definitions(m) for cfg,m in orig.items()}
bd={cfg:definitions(m) for cfg,m in before_meta.items()}
for owner,name in [('CoxUDFs','CoxModelHandle'),('GEEUDFs','GeeHandle')]:
    NESTED_CACHE_DECLARATIONS['BESHStatNG.WorksheetFunctions.'+owner]={name}
assert len(mapping)==len(members)==726 and len(history)==704
assert all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN'))


def cil(method):
    return {k:v for k,v in method.items() if k not in ['raw_IL_sha256','signature']}


def measured_original_return(cfg,method):
    rows=[r for r in old_returns[cfg] if r['declaring_type']==method['declaring_type'] and r['name']==method['member_name']]
    if len(rows)>1:
        rows=[r for r in rows if r['method'].endswith('('+','.join(method['parameter_types'])+')')]
    if not rows and method['declaring_type']=='BESHStatNG.WorksheetFunctions.CoxUDFs+_Closure$__' and method['member_name'].startswith('_Lambda$__'):
        from proof_kernel_v4 import actual_arguments
        parent=next(r for r in orig[cfg]['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.CoxUDFs' and r['member_name']=='COX_RESID')
        sites=[i for i,x in enumerate(parent['instructions']) if x['op']=='ldftn' and x['operand']=='METHOD:'+method['method']]
        assert len(sites)==1
        index=sites[0];ctor=parent['instructions'][index+1]
        assert ctor['op']=='newobj' and ctor['operand'].startswith('METHOD:System.Func`2[[')
        args=actual_arguments(ctor['operand'].split(' @ ',1)[0])
        assert args==['System.Int32','System.Int32'] and method['parameter_types']==['System.Int32']
        return dict(return_type=args[-1],authority='ACTUAL_ORIGINAL_TYPED_FUNC_INT32_INT32_CONSTRUCTION_AND_CALLBACK_CIL',
                    original_parent=parent['method'],witness_index=index,typed_delegate_witness=parent['instructions'][index:index+2],
                    scope='The sealed original callback metadata plus actual parent Func(Int32,Int32) constructor proves the value-type return. No fabricated reflection row or original execution.')
    assert len(rows)==1, 'Missing exact original measured return '+method['method']
    return rows[0]


def original_method(cfg,method):
    rows=[r for r in orig[cfg]['methods'] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])]
    assert len(rows)==1, 'Missing exact original method '+method['method']
    return rows[0]


required={mapping[r['source']['symbol_id']]['symbol_id'] for r in entry['required_native_parents']}
lifecycles=[]
for owner,name in [('CoxUDFs','COX_DROP'),('GEEUDFs','GEE_DROP')]:
    n=next(n for n in u['members'] if n['owner']=='BESHStatNG.WorksheetFunctions.'+owner and n['name']==name)
    required.add(n['symbol_id']);lifecycles.append(n['symbol_id'])
# Include the three already-supported native declarations that occur only in the original graph.
entry_graphs=load(HERE/'entry-compiled-dependency-catalog-v1.json')['original_graphs']
h_by_original={h['git_original_id']:h for h in history}
original_only=[]
for n in o['members']:
    if n['parent_member'] or n['kind'] not in ['FunctionBlock','SubBlock']:continue
    matched={cfg:[r for r in orig[cfg]['methods'] if (r['declaring_type'],r['member_name'])==(n['owner'],n['name']) and r['method'] in entry_graphs[cfg]['methods']] for cfg in meta}
    if not any(matched.values()):continue
    h=h_by_original[n['symbol_id']]
    for key in h['current_ids']:
        current=mapping[key]
        if current['symbol_id'] not in required:
            assert h['proof'] and not prior[key]['review_status'].startswith('OPEN'), 'Original-only dependency lacks retained support authority'
            required.add(current['symbol_id'])
            original_only.append(dict(original_id=h['canonical_original_id'],original_record=n,current_id=current['symbol_id'],current_record=current,original_compiled=matched,prior_proof=h['proof']))
parents=[n for n in u['members'] if n['symbol_id'] in required and n['parent_member'] is None and n['kind'] in ['FunctionBlock','SubBlock','ConstructorBlock']]
contracts=[];roots={cfg:[] for cfg in meta}
for n in parents:
    h=historical_for_current.get(reverse[n['symbol_id']]);a=original_members[h['git_original_id']] if h else None
    current_text,current_view=body((ROOT/n['path']).read_bytes(),n)
    original_text,original_view=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a) if a else (None,None)
    compiled=[]
    for cfg,m in meta.items():
        method=bind(n,m,shapes);old_method=original_method(cfg,method) if a else None
        old_return=measured_original_return(cfg,old_method) if old_method else None
        if old_return:assert method['signature']['return_type'].split(' @ ',1)[0]==old_return['return_type']
        roots[cfg].append(method['method'])
        compiled.append(dict(configuration=cfg,method=method['method'],complete_method=method,
            measured_signature=method['signature'],original_method=old_method,original_return=old_return,
            transitive=closure(m,[method['method']])))
    contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,
        original_record=a,current_source=pin(ROOT/n['path']),original_source=snapshots[a['path']] if a else None,
        current_whole_body=current_text,original_whole_body=original_text,current_source_view=current_view,original_source_view=original_view,
        compiled=compiled,new_support=prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),
        current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],
        original_fragments=[f for f in o['fragments'] if a and f['member_id']==a['symbol_id']],
        original_queries=0))

callbacks=[];callback_methods={cfg:{} for cfg in meta}
pending=[n for n in u['members'] if n['parent_member'] in {c['current_id'] for c in contracts}]
while pending:
    n=pending.pop(0);parent=members[n['parent_member']]
    siblings=sorted([s for s in u['members'] if s['parent_member']==parent['symbol_id']],key=lambda r:r['span_start'])
    ordinal=siblings.index(n);compiled=[]
    for cfg,m in meta.items():
        pkey=bind(parent,m,shapes)['method'] if parent['parent_member'] is None else callback_methods[cfg][parent['symbol_id']]
        targets=[defs[cfg][i['operand'][7:]] for i in defs[cfg][pkey]['instructions'] if i['op']=='ldftn' and isinstance(i['operand'],str) and i['operand'][7:] in defs[cfg] and defs[cfg][i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
        assert len(targets)==len(siblings), 'Actual callback sibling/ldftn cardinality mismatch'
        method=targets[ordinal]
        assert cil(method)==cil(bd[cfg][method['method']])
        is_new=prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')
        old_method=original_method(cfg,method) if is_new else None
        if is_new:assert cil(method)==cil(old_method)
        callback_methods[cfg][n['symbol_id']]=method['method']
        compiled.append(dict(configuration=cfg,parent_method=pkey,complete_method=method,original_method=old_method,
            prior_complete_method=bd[cfg][method['method']],measured_signature=method['signature'],
            original_return=measured_original_return(cfg,old_method) if old_method else None,
            inherited_authority=None if is_new else 'continuation-v14/callback-proofs-v1.json;current-ownership-v2.json;unchanged sealed predecessor reader'))
    h=historical_for_current.get(reverse[n['symbol_id']]);a=original_members[h['git_original_id']] if h else None
    current_text,current_view=body((ROOT/n['path']).read_bytes(),n)
    original_text,original_view=body((ROOT/snapshots[a['path']]['path']).read_bytes(),a) if a else (None,None)
    if a:assert current_text==original_text
    assert n['body_sha256']==prior[reverse[n['symbol_id']]]['body_sha256']
    assert h or not prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')
    callbacks.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],parent_id=parent['symbol_id'],
        current_record=n,original_record=a,source=pin(ROOT/n['path']),original_source=snapshots[a['path']] if a else None,
        current_whole_body=current_text,original_whole_body=original_text,current_view=current_view,original_view=original_view,
        source_sibling_ordinal=ordinal,compiled=compiled,new_support=prior[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')))
    pending.extend(s for s in u['members'] if s['parent_member']==n['symbol_id'])

properties=[]
for row in entry['rows']:
    if row['original_record']['kind']!='PropertyStatement':continue
    a=row['original_record'];owner,short=a['owner'].rsplit('.',1);native_owner=owner+'+'+short
    portable_owner=row['current']['portable_owner'];compiled=[]
    for cfg,m in meta.items():
        original_methods=[v for v in orig[cfg]['methods'] if v['declaring_type']==native_owner and v['member_name'] in ['get_'+a['name'],'set_'+a['name'],'.ctor']]
        assert len(original_methods)==3
        get_original=next(v for v in original_methods if v['member_name']=='get_'+a['name'])
        old_return=measured_original_return(cfg,get_original)
        native_type=next(t for t in m['types'] if t['type']==native_owner)
        exact_constructed_base=native_type['base_type'].split(' @ ',1)[0]
        getters=[v for v in m['methods'] if v['declaring_type']==exact_constructed_base and v['member_name']=='get_'+a['name'] and v['signature']['return_type'].split(' @ ',1)[0]==old_return['return_type']]
        assert len(getters)==1, 'Missing exact constructed inherited property return'
        getter=getters[0];base_owner=getter['declaring_type']
        accessors=[v for v in m['methods'] if v['declaring_type']==base_owner and v['member_name'] in ['get_'+a['name'],'set_'+a['name'],'.ctor']]
        assert len(accessors)==3
        setter=next(v for v in accessors if v['member_name']=='set_'+a['name'])
        original_setter=next(v for v in original_methods if v['member_name']=='set_'+a['name'])
        assert setter['parameter_types']==original_setter['parameter_types']
        for method,op in [(getter,'ldfld'),(setter,'stfld')]:
            assert sum(i['op']==op for i in method['instructions'])==1 and not method['exceptions']
        native_type=next(t for t in m['types'] if t['type']==native_owner)
        assert portable_owner in native_type['base_type']
        constructors=[v for v in m['methods'] if v['declaring_type']==native_owner and v['member_name']=='.ctor']
        assert len(constructors)==1
        targets={v['method'] for v in accessors}
        uses=[dict(caller=v['method'],instruction=i,target=x['operand'][7:]) for v in m['methods'] for i,x in enumerate(v['instructions']) if x['op'] in ['call','callvirt','newobj'] and isinstance(x['operand'],str) and x['operand'].startswith('METHOD:') and x['operand'][7:] in targets]
        assert uses
        roots[cfg].extend(v['method'] for v in accessors+constructors)
        compiled.append(dict(configuration=cfg,original_methods=original_methods,original_return=old_return,
            actual_inherited_methods=accessors,current_native_type=native_type,current_native_constructors=constructors,
            all_actual_compiled_consumers=uses,transitive=closure(m,[v['method'] for v in accessors+constructors])))
    text,source_view=body((ROOT/row['original_source']['path']).read_bytes(),a)
    properties.append(dict(original_id=row['historical_id'],original_record=a,original_source=row['original_source'],original_whole_body=text,
        original_view=source_view,current_ids=[],current_portable_owner=portable_owner,
        current_portable_source=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb'),
        compiled=compiled,new_support=True,original_queries=0))
assert len(properties)==4

graphs={cfg:closure(m,roots[cfg]) for cfg,m in meta.items()}
original_roots={cfg:[r['compiled'][i]['original_method']['method'] for r in contracts if r['compiled'][i]['original_method']]+[m['method'] for p in properties for m in p['compiled'][i]['original_methods']] for i,cfg in enumerate(meta)}
original_graphs={cfg:closure(m,original_roots[cfg]) for cfg,m in orig.items()}
common=load(HERE/'final-v1/dependency-full-common.json')
portable=[]
for n in common['members']:
    if n['parent_member'] is not None or n['kind'] not in ['FunctionBlock','SubBlock','ConstructorBlock']:continue
    candidates=[v for v in meta['Debug']['methods'] if v['declaring_type']==n['owner'] and v['member_name']==n['name'] and v['method'] in graphs['Debug']['methods']]
    if not candidates:continue
    method=bind(n,meta['Debug'],shapes)
    text,source_view=body((ROOT/n['path']).read_bytes(),n)
    compiled=[]
    for cfg,m in meta.items():
        actual=bind(n,m,shapes)
        callers=[dict(caller=v['method'],instruction=i,op=x['op']) for v in m['methods'] if v['method'] in graphs[cfg]['methods'] for i,x in enumerate(v['instructions']) if isinstance(x['operand'],str) and x['operand']=='METHOD:'+actual['method']]
        compiled.append(dict(configuration=cfg,complete_method=actual,signature=actual['signature'],actual_callers=callers))
    portable.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=source_view,
        parameters=shapes[n['symbol_id']],compiled=compiled,fragments=[f for f in common['fragments'] if f['member_id']==n['symbol_id']]))

contexts=[];deltas=[]
for cfg,m in meta.items():
    owners={defs[cfg][key]['declaring_type'] for key in graphs[cfg]['methods']}
    contexts.append(dict(configuration=cfg,owners=[dict(type=t,constructors_and_initializers=[v for v in m['methods'] if v['declaring_type']==t['type'] and v['member_name'] in ['.ctor','.cctor']]) for t in m['types'] if t['type'] in owners]))
    changed=[dict(method=key,original=bd[cfg][key],current=defs[cfg][key]) for key in bd[cfg] if key in defs[cfg] and cil(bd[cfg][key])!=cil(defs[cfg][key])]
    added=[defs[cfg][key] for key in defs[cfg] if key not in bd[cfg]]
    removed=[key for key in bd[cfg] if key not in defs[cfg]]
    assert not removed
    assert not any(' @ BESHStat.Core,' in r['method'] for r in changed)
    deltas.append(dict(configuration=cfg,changed=changed,added=added,removed=removed,Core_changed=0))

new=[r for r in contracts+callbacks if r['new_support']]
assert all(r['original_id'] for r in new)
outputs={
 'original-only-retained-dependencies-v1.json':dict(rows=original_only,original_queries=0),
 'complete-original-compiled-dependency-catalog-v1.json':dict(graphs=original_graphs,original_methods={cfg:[od[cfg][key] for key in original_graphs[cfg]['methods']] for cfg in meta},metadata=[pin(PREVIOUS/('metadata-all/packet-v1-context-v5/original-'+cfg+'.json')) for cfg in meta],original_queries=0),
 'whole-body-contracts-v1.json':dict(contracts=contracts,original_queries=0),
 'callback-proofs-v1.json':dict(rows=callbacks,original_queries=0),
 'property-body-contracts-v1.json':dict(contracts=properties,original_queries=0),
 'portable-api-contracts-v1.json':dict(contracts=portable,original_queries=0),
 'constructed-owner-proofs-v1.json':dict(configurations=contexts,original_queries=0),
 'compiled-method-deltas-v1.json':dict(configurations=deltas,original_queries=0),
 'cohort-diagnostic-v1.json':dict(roots=roots,graphs=graphs,lifecycle_ids=lifecycles,
     reviewed_current_ids=[r['current_id'] for r in contracts+callbacks],
     new_historical_ids=[r['original_id'] for r in new]+[r['original_id'] for r in properties],
     new_current_ids=[r['current_id'] for r in new],original_queries=0),
 'current-semantic-position-crosswalk-v1.json':dict(rows=[dict(before=key,after=n['symbol_id'],body_bytes_equal=prior[key]['body_sha256']==n['body_sha256']) for key,n in mapping.items()],entries=726,original_queries=0),
 'complete-compiled-dependency-catalog-v1.json':dict(graphs=graphs,
     current_methods={cfg:[defs[cfg][key] for key in graphs[cfg]['methods']] for cfg in meta},
     metadata=[pin(HERE/('final-v1/current-'+cfg+'.json')) for cfg in meta],original_queries=0),
}
for name,value in outputs.items():
    with (OUT/name).open('x',encoding='utf-8',newline='\n') as stream:json.dump(value,stream,indent=2,ensure_ascii=False,allow_nan=False);stream.write('\n')
print(json.dumps(dict(draft=DRAFT,parents=len(contracts),callbacks=len(callbacks),properties=len(properties),portable=len(portable),new_historical=len(new)+4,new_current=len(new),reached={cfg:len(graph['methods']) for cfg,graph in graphs.items()})))
