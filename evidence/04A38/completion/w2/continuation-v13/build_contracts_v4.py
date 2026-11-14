"""Exact original/current parents, separate callbacks and reached portable declarations."""
from common import *
from source_views import body
from proof_kernel_v5 import map_current, definitions, semantic, closure, bind, need
from collections import defaultdict

META='metadata-all/packet-v1-context-v5/'
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes())
old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
diag=json.loads((HERE/'cohort-diagnostic-v1.json').read_bytes())
shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']}
mapping=map_current(old['members'],u['members'],json.loads((HERE/'callback-identity-context-v1.json').read_bytes())['mapping']);reverse={n['symbol_id']:k for k,n in mapping.items()}
oldm={n['symbol_id']:n for n in old['members']};members={n['symbol_id']:n for n in u['members']}
om={n['symbol_id']:n for n in original['members']};hgit={h['git_original_id']:h for h in history}
metas={c:json.loads((HERE/(META+'current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
originals={c:json.loads((HERE/(META+'original-'+c+'.json')).read_bytes()) for c in metas}
defs={c:definitions(metas[c]) for c in metas};olddefs={c:definitions(originals[c]) for c in metas}
returns={c:json.loads((HERE/('return-metadata-'+c+'-v1.json')).read_bytes())['methods'] for c in metas}
oldreturns={c:json.loads((ROOT/'evidence/04A38/preedit'/('compiled-'+c+'.json')).read_bytes())['methods'] for c in metas}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/
    'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
hmap=defaultdict(list);resolutions=[]
for h in history:
    candidates=[mapping[k] for k in h['current_ids']];o=om[h['git_original_id']]
    if o['parent_member'] is not None:
        parent=hgit[o['parent_member']]
        parent_ids={mapping[k]['symbol_id'] for k in parent['current_ids']}
        candidates=[n for n in candidates if n['parent_member'] in parent_ids and
            n['body_sha256']==o['body_sha256'] and n['body_token_sha256']==o['body_token_sha256']]
    if len(candidates)>1:
        binding=[r for r in json.loads((HERE/'original-km-callback-bindings-v1.json').read_bytes())['rows'] if r['configuration']=='Debug' and r['original_record']['symbol_id']==o['symbol_id']]
        need(len(binding)==1,'Ambiguous historical callback without exact original/current CIL binding')
        candidates=[n for n in candidates if n['symbol_id']==binding[0]['current_id']]
    if len(candidates)==1:
        hmap[candidates[0]['symbol_id']].append(h)
        resolutions.append(dict(original_id=h['canonical_original_id'],current_id=candidates[0]['symbol_id']))
    else:
        # Retired/no-current bodies outside this graph remain under exact sealed v10 authority.
        need(not any(n['symbol_id'] in diag['all_candidate_ids'] for n in candidates),'Ambiguous cohort history')

def same_method(meta,method):
    found=[r for r in meta['methods'] if (r['declaring_type'],r['member_name'],r['parameter_types'])==
        (method['declaring_type'],method['member_name'],method['parameter_types'])]
    need(len(found)==1,'Original actual constructed method missing/ambiguous')
    return found[0]

def return_record(config,method):
    rows=[r for r in returns[config] if (r['declaring_type'],r['member_name'],r['parameter_types'])==
        (method['declaring_type'],method['member_name'],method['parameter_types'])]
    need(len(rows)==1,'Actual current CLR return declaration missing')
    return rows[0]

reviews=json.loads((HERE/'whole-parent-reviews-v1.json').read_bytes());contracts=[]
for n in u['members']:
    if n['symbol_id'] not in diag['reviewed_candidate_ids']:continue
    hs=hmap[n['symbol_id']];need(len(hs)<=1,'Overlapping historical native body')
    h=hs[0] if hs else None;o=om[h['git_original_id']] if h else None
    current_text,current_view=body((ROOT/n['path']).read_bytes(),n)
    original_text,original_view=body((ROOT/snapshots[o['path']]['path']).read_bytes(),o) if o else (None,None)
    compiled=[]
    for config,meta in metas.items():
        method=bind(n,meta,shapes);ret=return_record(config,method)
        before=same_method(originals[config],method) if h else None
        oldret=None
        if h:
            key=n['owner']+'.'+n['name']+'('+','.join(before['parameter_types'])+')'
            found=[r for r in oldreturns[config] if r['method']==key]
            need(len(found)==1,'Missing archived original native return metadata')
            oldret=found[0];need(oldret['return_type']==ret['return_type'],'Original/current native return type changed')
        compiled.append(dict(configuration=config,method=method['method'],source_parameters=shapes[n['symbol_id']],
            return_declaration=ret,original_return=oldret,original_method=before,
            transitive=closure(meta,[method['method']]),metadata=pin(HERE/(META+'current-'+config+'.json')),
            original_metadata=pin(HERE/(META+'original-'+config+'.json'))))
    was_open=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN')
    blocked=n['name'] in {'GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS'}
    new=was_open and not blocked
    need(not new or n['name'] in reviews,'Missing complete parent review')
    contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],
        current_record=n,original_record=o,current_source=pin(ROOT/n['path']),
        original_source=snapshots[o['path']] if o else None,current_whole_body=current_text,
        original_whole_body=original_text,current_source_view=current_view,original_source_view=original_view,
        proof_status='OPEN_BYREF_PARITY_DEPENDENCY' if blocked else 'COMPLETE_SOURCE_AND_COMPILED_CONTEXT',blocked_reason='Existing GuessPositiveClass double-reset alias difference; current-only measured, original prediction source-backed. No complete-parent closure from ordinary frozen compatibility.' if blocked else None,
        new_support=new,carried_proof=h['proof'] if h else dict(prior_current_id=reverse[n['symbol_id']],
            evidence='../continuation-v12/current-ownership-v2.json'),
        review=reviews[n['name']] if new else 'Retained complete v12 authority, authenticated source bytes and actual current/original compiled context replayed. No closure from a portable-call sighting.',
        compiled=compiled,current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],
        original_fragments=[f for f in original['fragments'] if o and f['member_id']==o['symbol_id']],
        qualification='Complete static source/CIL reconciliation; does not claim every dynamic/native arm executed.',original_queries=0))

callbacks=[];parent_contracts={c['current_id']:c for c in contracts}
pending=[n for n in u['members'] if n['parent_member'] in parent_contracts]
while pending:
    n=pending.pop(0);parent=members[n['parent_member']]
    siblings=sorted([s for s in u['members'] if s['parent_member']==parent['symbol_id']],key=lambda s:s['span_start'])
    index=next(i for i,s in enumerate(siblings) if s['symbol_id']==n['symbol_id'])
    text,view=body((ROOT/n['path']).read_bytes(),n)
    hs=hmap[n['symbol_id']];need(len(hs)<=1,'Historical callback overlap')
    h=hs[0] if hs else None;o=om[h['git_original_id']] if h else None
    oldtext,oldview=body((ROOT/snapshots[o['path']]['path']).read_bytes(),o) if o else (None,None)
    bindings=[]
    for config,meta in metas.items():
        if parent['parent_member'] is None:pkey=bind(parent,meta,shapes)['method']
        else:pkey=next(b['target'] for c in callbacks if c['current_id']==parent['symbol_id'] for b in c['configurations'] if b['configuration']==config)
        targets=[x['operand'][7:] for x in defs[config][pkey]['instructions'] if x['op']=='ldftn' and
            x['operand'][7:] in defs[config] and defs[config][x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
        need(len(targets)==len(siblings),'Actual current ldftn/source callback mismatch')
        method=defs[config][targets[index]];context=next(t for t in meta['types'] if t['type']==method['declaring_type'])
        ctors=[m for m in meta['methods'] if m['declaring_type']==method['declaring_type'] and m['member_name'] in ['.ctor','.cctor']]
        original_binding=None
        if o:
            op=om[o['parent_member']];pshapes=dict(shapes);pshapes[op['symbol_id']]=shapes[parent['symbol_id']]
            original_parent=bind(op,originals[config],pshapes)
            siblings_before=sorted([s for s in original['members'] if s['parent_member']==op['symbol_id']],key=lambda s:s['span_start'])
            ordinal=next(i for i,s in enumerate(siblings_before) if s['symbol_id']==o['symbol_id'])
            ots=[x['operand'][7:] for x in original_parent['instructions'] if x['op']=='ldftn' and x['operand'][7:] in olddefs[config]
                and olddefs[config][x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            need(len(ots)==len(siblings_before),'Original actual ldftn/source callback mismatch')
            omethod=olddefs[config][ots[ordinal]]
            original_binding=dict(parent=original_parent['method'],target=omethod,
                context=next(t for t in originals[config]['types'] if t['type']==omethod['declaring_type']),
                constructors=[m for m in originals[config]['methods'] if m['declaring_type']==omethod['declaring_type'] and m['member_name'] in ['.ctor','.cctor']])
        bindings.append(dict(configuration=config,parent_method=pkey,target=method['method'],complete_method=method,
            return_declaration=return_record(config,method),declaring_type=context,constructors=ctors,original_binding=original_binding))
    callbacks.append(dict(current_id=n['symbol_id'],parent_id=parent['symbol_id'],current_record=n,
        source=pin(ROOT/n['path']),whole_body=text,source_view=view,
        new_support=oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),new_declaration=False,
        original_id=h['canonical_original_id'] if h else None,original_record=o,
        original_source=snapshots[o['path']] if o else None,original_whole_body=oldtext,
        original_source_view=oldview,configurations=bindings,
        qualification='Separate exact source callback identity, actual immediate ldftn target, full CIL/exception/fields/constructor and current return metadata. Original real generated target is bound independently in its own parent; no enclosing-parent-only closure.'))
    pending.extend(s for s in u['members'] if s['parent_member']==n['symbol_id'])

pu=json.loads((HERE/'portable-universe-v1.json').read_bytes());portable=[]
reached=set(diag['graphs']['Debug']['methods'])
for n in pu['members']:
    if n['parent_member'] is not None:continue
    if not any((defs['Debug'][k]['declaring_type'],defs['Debug'][k]['member_name'])==(n['owner'],n['name']) for k in reached):continue
    method=bind(n,metas['Debug'],shapes)
    if method['method'] not in reached:continue
    text,view=body((ROOT/n['path']).read_bytes(),n);compiled=[]
    for config,meta in metas.items():
        method=bind(n,meta,shapes);context=next(t for t in meta['types'] if t['type']==n['owner'])
        need(not context['fields'],'Unexpected mutable portable phase fields')
        compiled.append(dict(configuration=config,method=method,return_declaration=return_record(config,method) if n['name']!='.ctor' else None,context=context))
    portable.append(dict(source_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,
        parameters=shapes[n['symbol_id']],fragments=[f for f in pu['fragments'] if f['member_id']==n['symbol_id']],compiled=compiled))
save('whole-body-contracts-v1.json',dict(contracts=contracts,original_queries=0))
save('callback-proofs-v1.json',dict(rows=callbacks,new_declarations=0,original_queries=0))
save('portable-api-contracts-v1.json',dict(contracts=portable,new_public_apis=8,original_queries=0))
save('transitive-compiled-context-v1.json',dict(configurations=diag['graphs'],original_queries=0))
save('historical-current-identity-resolutions-v1.json',dict(rows=resolutions,original_queries=0))
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());trans=[]
for row in plan['source_before']:
    p=row['live']['path'];after=pin(ROOT/p);need(after!=row['live'],'Planned source did not change')
    target=HERE/'source-after/final-v1'/(p+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/p).read_bytes())
    trans.append(dict(path=p,before=row['live'],preimage=row['preimage'],after=after,snapshot=pin(target)))
for row in plan['new_sources']:
    p=row['path'];target=HERE/'source-after/final-v1'/(p+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/p).read_bytes())
    trans.append(dict(path=p,before=None,preimage=None,after=pin(ROOT/p),snapshot=pin(target),entry_state='ABSENT'))
save('source-transitions-final-v1.json',dict(files=trans,Core_changes=0,original_queries=0))
save('proof-inputs-v1.json',dict(metadata_prefix=META,current_universe='current-universe-packet-v1.json',
    roots=diag['roots'],reviewed_current=[c['current_id'] for c in contracts+callbacks],
    new_historical_ids=[c['original_id'] for c in contracts+callbacks if c['new_support'] and c['original_id']]+[json.loads((HERE/'portable-historical-callback-proof-v1.json').read_bytes())['original_id']],
    new_current_supported=[c['current_id'] for c in contracts+callbacks if c['new_support']],original_queries=0))
print(json.dumps(dict(parents=len(contracts),callbacks=len(callbacks),new_parents=sum(c['new_support'] for c in contracts),
    new_callbacks=sum(c['new_support'] for c in callbacks),portable=len(portable))))
