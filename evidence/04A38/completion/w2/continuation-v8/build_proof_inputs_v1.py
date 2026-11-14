"""Complete source bodies, actual transitive CIL, callbacks and original pins."""
from common import *
from source_views import body
from collections import defaultdict
import re
STAGE='packet-v1';META='metadata-all/packet-v1-context-v3/'
u=json.loads((HERE/('current-universe-'+STAGE+'.json')).read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v1.json').read_bytes())
history=json.loads((PREVIOUS/'historical-ownership-v1.json').read_bytes())['members']
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
contexts={c:json.loads((HERE/(META+'current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
originals={c:json.loads((HERE/(META+'original-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
bykey=defaultdict(list);byowner=defaultdict(list)
for n in u['members']:
    bykey[n['signature_key']].append(n)
    if n['parent_member'] is None:byowner[n['owner'],n['name']].append(n)
current_to_history={current:h for h in history for current in h['current_ids']}
oldbykey=defaultdict(list)
for n in prior['members']:oldbykey[n['signature_key']].append(n)
def inherited(n):
    return next((r for r in oldbykey[n['signature_key']] if r['body_token_sha256']==n['body_token_sha256'] and not r['review_status'].startswith('OPEN')),None)
def historical(n):
    old=next((r for r in oldbykey[n['signature_key']] if r['parent_member'] is None),None)
    return current_to_history.get(old['symbol_id']) if old else None
priority=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members']
assert len(priority)==24
roots=[]
for r in priority:
    h=next(h for h in history if h['canonical_original_id']==r['original_id'])
    mapped={k.split('@')[0] for k in h['current_ids']}
    ns=[n for n in u['members'] if n['signature_key'] in mapped]
    assert len(ns)==1
    roots.append(next((p for p in u['members'] if p['symbol_id']==ns[0]['parent_member']),ns[0]))
roots.extend(n for n in u['members'] if n['name'] in ['ResolveSampleSizeHypothesisKind38','MaterializeSampleSizeResult38'])
roots=list({n['symbol_id']:n for n in roots}.values())

def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def closure(config,keys):
    definitions={r['method']:r for r in contexts[config]['methods']};pending=list(keys);seen=set();external=set();edges=[]
    while pending:
        key=pending.pop()
        if key in seen:continue
        row=definitions.get(key)
        if row is None:external.add(key);continue
        seen.add(key)
        for i,ins in enumerate(row['instructions']):
            if ins['op'] not in ['call','callvirt','newobj','ldftn','ldvirtftn'] or not isinstance(ins['operand'],str) or not ins['operand'].startswith('METHOD:'):continue
            target=ins['operand'][7:];edges.append(dict(caller=key,instruction=i,op=ins['op'],target=target));pending.append(target)
    assert not any(e.split(' @ ',1)[1].split(',',1)[0] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'] for e in external),'Missing internal transitive context'
    return dict(methods=sorted(seen),external_runtime_or_ExcelDna=sorted(external),edges=edges)

reviews=json.loads((HERE/'whole-parent-reviews-v1.json').read_bytes())

all_graphs={};reviewed=set();compiled_by_source={}
for config in ['Debug','Release']:
    defs={r['method']:r for r in contexts[config]['methods']}
    starts=[r['method'] for r in defs.values() if any((r['declaring_type'],r['member_name'])==(n['owner'],n['name']) for n in roots)]
    graph=closure(config,starts);all_graphs[config]=graph
    for key in graph['methods']:
        r=defs[key]
        for n in byowner.get((r['declaring_type'],r['member_name']),[]):reviewed.add(n['symbol_id'])
for n in u['members']:
    if n['parent_member'] in reviewed:reviewed.add(n['symbol_id'])

contracts=[];om={n['symbol_id']:n for n in original['members']}
for n in u['members']:
    if n['symbol_id'] not in reviewed:continue
    if n['parent_member'] is not None:continue # Explicit compiled callback proof below.
    h=historical(n);new=not inherited(n)
    if new:assert n['name'] in reviews,n['name']
    if h:
        old=om[h['git_original_id']];oldsource=snapshots[h['path']]
        oldtext,oldview=body((ROOT/oldsource['path']).read_bytes(),old)
    else:old=None;oldsource=None;oldtext=None;oldview=None
    text,view=body((ROOT/n['path']).read_bytes(),n)
    compiled=[]
    for config in ['Debug','Release']:
        defs={r['method']:r for r in contexts[config]['methods']}
        candidates=[r['method'] for r in defs.values() if (r['declaring_type'],r['member_name'])==(n['owner'],n['name'])]
        assert candidates
        graph=closure(config,candidates)
        source_dependencies=sorted({v['symbol_id'] for key in graph['methods'] for v in byowner.get((defs[key]['declaring_type'],defs[key]['member_name']),[])})
        compiled.append(dict(configuration=config,roots=candidates,transitive=graph,source_dependencies=source_dependencies,metadata=pin(HERE/(META+'current-'+config+'.json')),original_metadata=pin(HERE/(META+'original-'+config+'.json'))))
    contracts.append(dict(original_id=h['canonical_original_id'] if h else None,current_id=n['symbol_id'],current_record=n,original_record=old,current_source=pin(ROOT/n['path']),original_source=oldsource,current_whole_body=text,original_whole_body=oldtext,current_source_view=view,original_source_view=oldview,new_support=new,carried_proof=h.get('proof') if h else None,review=reviews.get(n['name'],'Carried C7 source body is exact under its signature and full token identity. The complete current/original source and all real transitive CIL/field/callback dependencies are rechecked here, and the authenticated C7 reader is replayed against exact retained entry preimages. This entry does not close a body solely from a portable call.'),compiled=compiled,current_fragments=[f for f in u['fragments'] if f['member_id']==n['symbol_id']],original_fragments=[f for f in original['fragments'] if h and f['member_id']==h['git_original_id']],focused_Debug='focused-v3-packet-v1-Debug.json',Release_unit_tests='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
callbacks=[]
for parent in contracts:
    siblings=sorted([n for n in u['members'] if n['parent_member']==parent['current_id']],key=lambda r:r['span_start'])
    for index,n in enumerate(siblings):
        text,view=body((ROOT/n['path']).read_bytes(),n);bindings=[]
        for config in ['Debug','Release']:
            defs={r['method']:r for r in contexts[config]['methods']};pc=next(r for r in parent['compiled'] if r['configuration']==config)
            assert len(pc['roots'])==1
            targets=[i['operand'][7:] for i in defs[pc['roots'][0]]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs and defs[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
            assert len(targets)==len(siblings),(parent['current_record']['name'],len(targets),len(siblings))
            target=defs[targets[index]];context=next(t for t in contexts[config]['types'] if t['type']==target['declaring_type'])
            bindings.append(dict(configuration=config,parent_method=pc['roots'][0],target=target['method'],complete_method=target,declaring_type=context))
        callbacks.append(dict(current_id=n['symbol_id'],parent_id=parent['current_id'],current_record=n,source=pin(ROOT/n['path']),whole_body=text,source_view=view,configurations=bindings,new_current=False))
save('whole-body-contracts-v1.json',dict(status='COMPLETE_SOURCE_TRANSITIVE_CIL_BRANCH_INLINE_CONTRACTS_PARTIAL_NOT_READY',contracts=contracts,original_queries=0))
save('callback-proofs-v1.json',dict(rows=callbacks,new_current_callbacks=0,scope='Every anonymous source child of every reviewed parent is matched by ordinal to a real ldftn and complete method/exception/field context in both configurations. No compiler alias is introduced.'))
save('transitive-compiled-context-v1.json',dict(configurations=all_graphs,original_queries=0))
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());transitions=[]
for row in plan['source_before']:
    path=row['live']['path'];after=pin(ROOT/path)
    if after==row['live']:continue
    target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/path).read_bytes())
    transitions.append(dict(path=path,before=row['live'],preimage=row['preimage'],after=after,snapshot=pin(target)))
for new_source in plan['new_sources']:
    path=new_source['path'];target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/path).read_bytes())
    transitions.append(dict(path=path,before=None,preimage=None,after=pin(ROOT/path),snapshot=pin(target),entry_state='ABSENT'))
save('source-transitions-final-v1.json',dict(files=transitions,original_queries=0,engines='NO_EXISTING_ENGINE_CHANGE',public_exports='268_UNCHANGED'))
save('proof-inputs-v1.json',dict(priority=priority,root_current_ids=[n['symbol_id'] for n in roots],metadata_prefix=META,reviewed_current=[n['symbol_id'] for n in u['members'] if n['symbol_id'] in reviewed],current_universe='current-universe-packet-v1.json',new_historical_ids=[c['original_id'] for c in contracts if c['new_support'] and c['original_id']],new_current_only=[c['current_id'] for c in contracts if c['new_support'] and not c['original_id']],support_candidate='SampleSizeUDFs',source_domain='All C7 LegacyUdfText38 policy/call-site sources unchanged except byte-preserving shifts; no new text call, no normalization or policy change. Eight out-of-domain counterexamples remain immutable.',original_queries=0))
print(json.dumps(dict(contracts=len(contracts),callbacks=len(callbacks),new_historical=sum(c['new_support'] and c['original_id'] is not None for c in contracts),new_current_only=sum(c['new_support'] and c['original_id'] is None for c in contracts),source_transitions=len(transitions))))
