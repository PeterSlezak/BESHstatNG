"""Exact universes/complements; unresolved ByRef dependency never becomes supported."""
from common import *
from proof_kernel_v5 import map_current,definitions,semantic
from source_views import view
import re
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());oldh=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']
portable=json.loads((HERE/'portable-historical-callback-proof-v1.json').read_bytes())
mapping=map_current(prior['members'],u['members'],json.loads((HERE/'callback-identity-context-v1.json').read_bytes())['mapping'])
reverse={n['symbol_id']:k for k,n in mapping.items()};oldm={n['symbol_id']:n for n in prior['members']}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in prior['members'] if not n['review_status'].startswith('OPEN')}
assert all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in prior['members'] if not n['review_status'].startswith('OPEN'))
eligible=[c for c in contracts if c['proof_status']!='OPEN_BYREF_PARITY_DEPENDENCY']+callbacks
closed=inherited|{c['current_id'] for c in eligible}
newhist={c['original_id']:c for c in eligible if c['new_support'] and c['original_id']}
assert len(newhist)==25 and len(closed-inherited)==29
assert portable['original_id'] not in newhist
history=[];new=[]
for h in oldh:
    row=dict(h,current_ids=[mapping[k]['symbol_id'] for k in h['current_ids']])
    if h['canonical_original_id'] in newhist:
        c=newhist[h['canonical_original_id']];assert not h['proof']
        row.update(current_ids=[c['current_id']],proof=dict(kind='C13_COMPLETE_SOURCE_CIL_CALLBACK_DEPENDENCY_FRAGMENT_CONTEXT',
            evidence='callback-proofs-v1.json' if c['current_record']['parent_member'] else 'whole-body-contracts-v1.json',original_id=h['canonical_original_id'],current_id=c['current_id']),
            disposition='FULLY_SUPPORTED_C13_COMPLETE_SUPPORTED_BODY_CONTEXT')
        new.append(dict(original_id=h['canonical_original_id'],current_ids=row['current_ids'],portable_current_ids=[],name=h['name'],proof=row['proof']))
    elif h['canonical_original_id']==portable['original_id']:
        assert not h['proof'] and h['current_ids']==[]
        row.update(current_ids=[],portable_current_ids=[portable['current_source_record']['symbol_id']],
            proof=dict(kind='C13_ORIGINAL_CALLBACK_TO_ACTUAL_COMMON_SOURCE_AND_CIL',evidence='portable-historical-callback-proof-v1.json',
                original_id=h['canonical_original_id'],portable_current_id=portable['current_source_record']['symbol_id']),
            disposition='FULLY_SUPPORTED_C13_PORTABLE_CALLBACK_NO_INVENTED_WINDOWS_ID')
        new.append(dict(original_id=h['canonical_original_id'],current_ids=[],portable_current_ids=row['portable_current_ids'],name=h['name'],proof=row['proof']))
    history.append(row)
assert len(new)==26
current=[dict(n,review_status='FULLY_SUPPORTED_C13_SOURCE_COMPILED_DEPENDENCY_CONTEXT' if n['symbol_id'] in closed else oldm[reverse[n['symbol_id']]]['review_status']) for n in u['members']]
olds={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in prior['support']};supports=[];moduleproofs=[];new_support=[]
snapshots={r['source']['path']:r['snapshot'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']}
configs={c:json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+c+'.json')).read_bytes()) for c in ['Debug','Release']}
originals={c:json.loads((HERE/('metadata-all/packet-v1-context-v5/original-'+c+'.json')).read_bytes()) for c in configs}
for s in u['supports']:
    old=olds[s['path'],s['kind'],s['owner'],s['declaration']];complete=old['fully_reconciled'];proof=old.get('proof')
    if s['kind']=='ModuleBlock' and s['declaration'] in ['Public Module MultivariateAnalysisUDFs','Public Module PlotDataUDFs']:
        base=s['owner']+'.'+s['declaration'].split()[-1]
        children=[n for n in u['members'] if n['path']==s['path'] and (n['owner']==base or n['owner'].startswith(base+'.'))]
        nested=[n for n in u['supports'] if n['path']==s['path'] and n['support_id']!=s['support_id'] and s['span_start']<=n['span_start'] and n['span_start']+n['span_length']<=s['span_start']+s['span_length']]
        blocking=[n['symbol_id'] for n in children if n['symbol_id'] not in closed]
        unclosed=[n['support_id'] for n in nested if not olds[n['path'],n['kind'],n['owner'],n['declaration']]['fully_reconciled']]
        measured=[]
        for config,meta in configs.items():
            actual=definitions(meta);od=definitions(originals[config]);types={t['type']:t for t in meta['types'] if t['type']};ot={t['type']:t for t in originals[config]['types'] if t['type']}
            assert types[base]==ot[base]
            constructors=[m for m in meta['methods'] if m['declaring_type']==base and m['member_name'] in ['.ctor','.cctor']]
            assert all(m['method'] in od and semantic(m)==semantic(od[m['method']]) for m in constructors)
            descendants=[t for t in meta['types'] if t['type'] and t['type'].startswith(base+'+')]
            measured.append(dict(configuration=config,module_type=types[base],original_module_type=ot[base],constructors_and_initializers=constructors,
                nested_contexts=descendants,all_declared_module_methods=[m for m in meta['methods'] if m['declaring_type']==base]))
        before=next(n for n in original['supports'] if (n['path'],n['kind'],n['owner'],n['declaration'])==(s['path'],s['kind'],s['owner'],s['declaration']))
        nowtext,nowview=view((ROOT/s['path']).read_bytes(),next(n['sha256'] for n in u['sources'] if n['path']==s['path']))
        oldtext,oldview=view((ROOT/snapshots[s['path']]['path']).read_bytes(),next(n['sha256'] for n in original['sources'] if n['path']==s['path']))
        extract=lambda text,node:text.encode('utf-16-le')[node['span_start']*2:(node['span_start']+node['span_length'])*2].decode('utf-16-le')
        eligible_module=not blocking and not unclosed
        moduleproofs.append(dict(source_support=s,original_support=before,all_primary_children=children,all_nested_support=nested,blocking_primary_ids=blocking,
            blocking_support_ids=unclosed,current_whole_declaration=extract(nowtext,s),original_whole_declaration=extract(oldtext,before),current_view=nowview,original_view=oldview,
            configurations=measured,eligible=eligible_module,scope='Source complete only when every original/current child contract, callback and nested field/property class is already supported. Compiler closures and all ctor/cctor/state contexts retained. No native/scientific acceptance.'))
        if eligible_module:
            assert not complete;complete=True;proof=dict(kind='C13_COMPLETE_MODULE_ALL_CHILD_BODIES_DECLARATIONS_STATE_CIL',evidence='module-container-proofs-v1.json',support_id=s['support_id']);new_support.append(s['support_id'])
    supports.append(dict(s,fully_reconciled=complete,proof=proof,children=old.get('children',[]),distinct=not s['already_in_primary_population']))
assert len(new_support)==1 and next(s for s in supports if s['support_id']==new_support[0])['declaration']=='Public Module MultivariateAnalysisUDFs'
classes={s['declaration'] for s in supports if s['kind']=='ClassBlock' and s['declaration'].startswith('Private Class ') and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs'}
classowners={'BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.'+s.split()[-1] for s in classes}
def headers(universe):
    found={};supported={(s['path'],s['kind'],s['owner'],s['declaration']) for s in supports if s['fully_reconciled']}
    for s in universe['supports']:
        if (s['path'],s['kind'],s['owner'],s['declaration']) not in supported:continue
        if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
        elif s['kind']=='ClassBlock' and s['declaration'] in classes:kinds=['ClassStatement','EndClassStatement']
        elif s['kind']=='PropertyStatement' and s['owner'] in classowners:kinds=['PropertyStatement']
        else:continue
        for f in universe['fragments']:
            if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:found[f['fragment_id']]=s['support_id']
    return found
hc=headers(u);ho=headers(original);oc={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='FULLY_SUPPORTED_SOURCE_AND_COMPILED_CONTEXT' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN',proof_member=f['member_id'] if f['member_id'] in closed else hc.get(f['fragment_id'])) for f in u['fragments']]
historicalfragments=[dict(f,covered=f['member_id'] in oc or f['fragment_id'] in ho,proof_member=f['member_id'] if f['member_id'] in oc else ho.get(f['fragment_id'])) for f in original['fragments']]
save('historical-ownership-v2.json',dict(members=history,total=704))
save('current-ownership-v2.json',dict(members=current,support=supports,fragments=fragments))
save('module-container-proofs-v1.json',dict(rows=moduleproofs,original_queries=0))
save('original-fragment-parent-crosswalk-v2.json',dict(rows=historicalfragments,scope='Exact original callable and fully closed declaration fragment ownership; OPEN parents retain their arms, except independently proved child callbacks.'))
save('support-fragment-crosswalk-v1.json',dict(current=hc,original=ho))
summary=dict(historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),
    current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),support=dict(raw=len(supports),distinct=303,distinct_closed=sum(s['fully_reconciled'] for s in supports if s['distinct']),distinct_OPEN=sum(not s['fully_reconciled'] for s in supports if s['distinct'])))
def fragment_counts(rows,predicate):return dict(total=len(rows),covered=sum(predicate(r) for r in rows),branches=sum(r['is_branch'] for r in rows),branches_covered=sum(r['is_branch'] and predicate(r) for r in rows),inline=sum(r['is_inline'] for r in rows),inline_covered=sum(r['is_inline'] and predicate(r) for r in rows))
summary['fragments']=dict(current=fragment_counts(fragments,lambda r:r['review_status']!='OPEN'),original=fragment_counts(historicalfragments,lambda r:r['covered']))
assert (summary['historical']['closed'],summary['current']['closed'],summary['support']['distinct_closed'])==(461,494,253)
save('ownership-summary-v2.json',summary)
save('new-closures-final-v1.json',dict(rows=new,new_support=new_support,current_only=[c['current_id'] for c in eligible if c['new_support'] and not c['original_id']],
    historical_new=26,current_new=29,distinct_support_new=1,blocked_parents=['GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS'],original_queries=0))
priorqueue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes());newids={r['original_id'] for r in new}
remaining=[r for r in priorqueue['members'] if r['original_id'] not in newids]
save('remaining-work-queue-v1.json',dict(members=remaining,first_members=remaining[:24],current_OPEN=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],next='Complete whole original/current behavior and dependency proof; recorded ByRef reset mismatch is not waived.') for n in current if n['review_status'].startswith('OPEN')],
    support_OPEN=[s for s in supports if s['distinct'] and not s['fully_reconciled']],original_queries=0))
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
    mandatory_blockers=dict(historical_OPEN=243,current_OPEN=232,distinct_support_OPEN=50),owner_acceptance='NOT_ACCEPTED_NOT_MERGED',native='NOT_RUN',scientific_oracle='NOT_RUN',original_queries=0))
print(json.dumps(summary))
