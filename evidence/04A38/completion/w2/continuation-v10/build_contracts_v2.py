"""Full original/current source, declaration, CIL, callback and field contracts."""
from common import *
from source_views import body
from proof_kernel_v2 import *
META='metadata-all/packet-v2-context-v5/'
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes());oldcur=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());diag=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes())
shapes={n['symbol_id']:n for n in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']};mapping=map_current(oldcur['members'],u['members']);reverse={n['symbol_id']:old for old,n in mapping.items()};oldmembers={n['symbol_id']:n for n in oldcur['members']};om={n['symbol_id']:n for n in original['members']};members={n['symbol_id']:n for n in u['members']}
metas={c:json.loads((HERE/(META+'current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']};originals={c:json.loads((HERE/(META+'original-'+c+'.json')).read_bytes()) for c in ['Debug','Release']};defs={c:definitions(metas[c]) for c in metas}
returns={c:json.loads((HERE/('return-metadata-'+c+'-v1.json')).read_bytes())['methods'] for c in metas}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
hmap=defaultdict(list)
for h in history:
    for key in h['current_ids']:hmap[key].append(h)
reviews=json.loads((HERE/'whole-parent-reviews-v1.json').read_bytes());contracts=[];roots=[members[k] for k in diag['roots']];graphs={}
for config in metas:graphs[config]=closure(metas[config],[bind(n,metas[config],shapes)['method'] for n in roots])
for n in u['members']:
    if n['symbol_id'] not in diag['reviewed_candidate_ids']:continue
    hs=hmap[reverse[n['symbol_id']]];need(len(hs)<=1,'Ambiguous historical parent identity');h=hs[0] if hs else None;o=om[h['git_original_id']] if h else None;current_text,current_view=body((ROOT/n['path']).read_bytes(),n);oldtext,oldview=body((ROOT/snapshots[h['path']]['path']).read_bytes(),o) if h else (None,None);compiled=[]
    if not h:need(n['name']=='MaterializePortableReport38' and not oldmembers[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),'Unapproved current-only parent')
    for config,meta in metas.items():
        method=bind(n,meta,shapes);ret=[r for r in returns[config] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])];need(len(ret)==1,'Missing concrete current return declaration')
        original_method=next(r for r in originals[config]['methods'] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])) if h else None
        graph=closure(meta,[method['method']]);compiled.append(dict(configuration=config,method=method['method'],source_parameters=shapes[n['symbol_id']],return_declaration=ret[0],original_method=original_method,transitive=graph,metadata=pin(HERE/(META+'current-'+config+'.json')),original_metadata=pin(HERE/(META+'original-'+config+'.json'))))
    new=oldmembers[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')
    review=reviews[n['name']] if new else 'Retained C9 full-body authority is replayed against authentic entry preimages. This required dependency has exact inherited source bytes and its current/original complete source, parameter/return declaration, branches, callbacks and actual constructed CIL context are retained here. No closure is inferred from a portable call alone.'
    contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,original_record=o,current_source=pin(ROOT/n['path']),original_source=snapshots[h['path']] if h else None,current_whole_body=current_text,original_whole_body=oldtext,current_source_view=current_view,original_source_view=oldview,new_support=new,carried_proof=h['proof'] if h else dict(prior_ledger='../continuation-v9/current-ownership-v2.json',current_id=reverse[n['symbol_id']],scope='Inherited current-only materializer, no historical ID inferred'),review=review,compiled=compiled,current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],original_fragments=[f for f in original['fragments'] if o is not None and f['member_id']==o['symbol_id']],dynamic_qualification='Full source/CIL correspondence, not a claim every branch was dynamically exercised. Native reference acquisition/caches and error materialization remain Windows-owned.',original_queries=0))
callbacks=[];covered_parents={c['current_id']:c for c in contracts};pending=[n for n in u['members'] if n['parent_member'] in covered_parents]
while pending:
    n=pending.pop(0);parent=members[n['parent_member']];text,view=body((ROOT/n['path']).read_bytes(),n);bindings=[]
    siblings=sorted([s for s in u['members'] if s['parent_member']==parent['symbol_id']],key=lambda r:r['span_start']);index=next(i for i,s in enumerate(siblings) if s['symbol_id']==n['symbol_id'])
    for config,meta in metas.items():
        if parent['parent_member'] is None:pkey=bind(parent,meta,shapes)['method']
        else:pkey=next(b['target'] for r in callbacks if r['current_id']==parent['symbol_id'] for b in r['configurations'] if b['configuration']==config)
        targets=[i['operand'][7:] for i in defs[config][pkey]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs[config] and defs[config][i['operand'][7:]]['member_name'].startswith('_Lambda$__')];need(len(targets)==len(siblings),'Source callback/real ldftn cardinality mismatch')
        method=defs[config][targets[index]];context=next(t for t in meta['types'] if t['type']==method['declaring_type']);ctors=[m for m in meta['methods'] if m['declaring_type']==method['declaring_type'] and m['member_name'] in ['.ctor','.cctor']]
        bindings.append(dict(configuration=config,parent_method=pkey,target=method['method'],complete_method=method,declaring_type=context,constructors=ctors))
    old=oldmembers[reverse[n['symbol_id']]];hs=hmap.get(old['symbol_id'],[]);oldnode=om[hs[0]['git_original_id']] if len(hs)==1 else None
    if oldnode:
        need(oldnode['kind'] in ['SingleLineFunctionLambdaExpression','MultiLineFunctionLambdaExpression'],'Historical callback mapped to parent body');oldsrc=snapshots[oldnode['path']];oldtext,oldview=body((ROOT/oldsrc['path']).read_bytes(),oldnode)
    else:oldsrc=oldtext=oldview=None
    callbacks.append(dict(current_id=n['symbol_id'],parent_id=parent['symbol_id'],current_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,new_support=old['review_status'].startswith('OPEN'),new_declaration=False,original_id=hs[0]['canonical_original_id'] if oldnode else None,original_record=oldnode,original_source=oldsrc,original_whole_body=oldtext,original_source_view=oldview,configurations=bindings,qualification='A current-only callback introduced in an earlier extraction is an explicit separate obligation, not an invented original member. Its captured fields/constructors and actual immediate ldftn caller are retained.'))
    pending.extend(s for s in u['members'] if s['parent_member']==n['symbol_id'])
portable=json.loads((HERE/'portable-universe-v1.json').read_bytes());portable_contracts=[]
for n in portable['members']:
    text,view=body((ROOT/n['path']).read_bytes(),n);compiled=[]
    for config,meta in metas.items():
        method=bind(n,meta,shapes);context=next(t for t in meta['types'] if t['type']==n['owner']);need(not context['fields'],'Unexpected new mutable portable field')
        ret=next((r for r in returns[config] if (r['declaring_type'],r['member_name'],r['parameter_types'])==(method['declaring_type'],method['member_name'],method['parameter_types'])),None);need(ret is not None or n['name']=='.ctor','Missing new portable return metadata')
        compiled.append(dict(configuration=config,method=method,return_declaration=ret,context=context))
    portable_contracts.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,parameters=shapes[n['symbol_id']],fragments=[f for f in portable['fragments'] if f['member_id']==n['symbol_id']],compiled=compiled))
save('whole-body-contracts-v1.json',dict(contracts=contracts,original_queries=0));save('callback-proofs-v1.json',dict(rows=callbacks,new_declarations=0,original_queries=0));save('portable-api-contracts-v1.json',dict(contracts=portable_contracts,public_apis=19,new_public_apis=15,portable_methods=len(portable_contracts),new_portable_methods=16,inherited_portable_methods=5,source_universe=pin(HERE/'portable-universe-v1.json'),scope='Complete affected Core/Common declaration census, retaining5 previously supported declarations and distinguishing16 new declarations;, not added to the immutable historical704 or existing Windows current frontier.',original_queries=0));save('transitive-compiled-context-v1.json',dict(configurations=graphs,original_queries=0))
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());trans=[]
for r in plan['source_before']:
    p=r['live']['path'];after=pin(ROOT/p);need(after!=r['live'],'Planned source did not change');target=HERE/'source-after/final-v1'/(p+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/p).read_bytes())
    trans.append(dict(path=p,before=r['live'],preimage=r['preimage'],after=after,snapshot=pin(target)))
for r in plan['new_sources']:
    p=r['path'];target=HERE/'source-after/final-v1'/(p+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/p).read_bytes())
    trans.append(dict(path=p,before=None,preimage=None,after=pin(ROOT/p),snapshot=pin(target),entry_state='ABSENT'))
save('source-transitions-final-v1.json',dict(files=trans,Core_changes=0,original_queries=0));save('proof-inputs-v1.json',dict(metadata_prefix=META,current_universe='current-universe-packet-v2.json',roots=diag['roots'],reviewed_current=[c['current_id'] for c in contracts]+[c['current_id'] for c in callbacks],new_historical_ids=[c['original_id'] for c in contracts if c['new_support']]+[c['original_id'] for c in callbacks if c['new_support'] and c['original_id']],new_current_supported=[c['current_id'] for c in contracts+callbacks if c['new_support']],original_queries=0))
print(json.dumps(dict(parents=len(contracts),callbacks=len(callbacks),new_parents=sum(c['new_support'] for c in contracts),new_callbacks=sum(c['new_support'] for c in callbacks),portable_methods=len(portable_contracts),transitions=len(trans))))
