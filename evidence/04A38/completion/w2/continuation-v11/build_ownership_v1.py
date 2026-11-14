"""Exact canonical universes/complements, without count-based closure shortcuts."""
from common import *
from proof_kernel_v3 import map_current,need
import re
u=json.loads((HERE/'current-universe-packet-v2.json').read_bytes())
original=json.loads((W2.parent/'w1/original-universe.json').read_bytes())
prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes())
oldh=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members']
contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows']
host=json.loads((HERE/'host-cache-contexts-v1.json').read_bytes())
mapping=map_current(prior['members'],u['members']);oldm={n['symbol_id']:n for n in prior['members']}
reverse={n['symbol_id']:k for k,n in mapping.items()}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in prior['members'] if not n['review_status'].startswith('OPEN')}
need(all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in prior['members'] if not n['review_status'].startswith('OPEN')),'Previously supported body changed')
closed=inherited|{c['current_id'] for c in contracts+callbacks}
newhist={c['original_id']:c for c in contracts+callbacks if c['new_support'] and c['original_id']}
need(len(newhist)==33 and len(closed-inherited)==33,'Unexpected actual graph closure population')
history=[];new=[]
for h in oldh:
    row=dict(h,current_ids=[mapping[k]['symbol_id'] for k in h['current_ids']])
    if h['canonical_original_id'] in newhist:
        c=newhist[h['canonical_original_id']];need(not h['proof'],'Historical closure counted twice')
        row.update(current_ids=[c['current_id']],proof=dict(kind='C11_COMPLETE_SOURCE_CONSTRUCTED_CIL_CALLBACK_FIELD_FRAGMENT_CONTEXT',
            evidence='callback-proofs-v1.json' if c['current_record']['parent_member'] else 'whole-body-contracts-v1.json',
            original_id=h['canonical_original_id'],current_id=c['current_id']),disposition='FULLY_SUPPORTED_C11_SOURCE_AND_COMPILED_CONTEXT')
        if c['current_record']['parent_member']:
            row['identity_note']='C11 narrows the earlier OPEN signature candidate list using the exact original/current parent plus body/token hashes and real original/current ldftn targets; no same-name lambda alias.'
        new.append(dict(original_id=h['canonical_original_id'],current_ids=row['current_ids'],name=h['name'],proof=row['proof']))
    history.append(row)
current=[dict(n,review_status='FULLY_SUPPORTED_C11_SOURCE_COMPILED_DEPENDENCY_CONTEXT' if n['symbol_id'] in closed else oldm[reverse[n['symbol_id']]]['review_status']) for n in u['members']]
olds={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in prior['support']}
support=[];new_support=[]
newclasses={'Private Class KMeansHandle','Private Class HierarchicalHandle','Private Class CorrespondenceHandle'}
for source in u['supports']:
    old=olds[source['path'],source['kind'],source['owner'],source['declaration']]
    complete=old['fully_reconciled'];proof=old.get('proof');children=old.get('children',[])
    if source['kind']=='ClassBlock' and source['declaration'] in newclasses and source['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs':
        context=next(r for r in host['rows'] if r['source_support']==source)
        need(all(olds[s['path'],s['kind'],s['owner'],s['declaration']]['fully_reconciled'] for s in context['children']),'Unclosed class property')
        need(not complete,'Support closure counted twice');complete=True
        proof=dict(kind='C11_COMPLETE_UNCHANGED_HOST_INSTANCE_FIELDS_CTOR_ACCESSORS_ALL_WRITERS',
            evidence='host-cache-contexts-v1.json',support_id=source['support_id'])
        new_support.append(source['support_id'])
    support.append(dict(source,fully_reconciled=complete,proof=proof,children=children,
        distinct=not source['already_in_primary_population']))
need(len(new_support)==3,'Unexpected independent support population')
classes={'Private Class PcaHandle','Private Class FactorAnalysisHandle'}|newclasses
classowners={'BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.'+s.split()[-1] for s in classes}
def headers(universe):
    found={};supported={(s['path'],s['kind'],s['owner'],s['declaration']) for s in support if s['fully_reconciled']}
    for s in universe['supports']:
        if (s['path'],s['kind'],s['owner'],s['declaration']) not in supported:continue
        if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
        elif s['kind']=='ClassBlock' and s['declaration'] in classes:kinds=['ClassStatement','EndClassStatement']
        elif s['kind']=='PropertyStatement' and s['owner'] in classowners:kinds=['PropertyStatement']
        else:continue
        for f in universe['fragments']:
            if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:
                found[f['fragment_id']]=s['support_id']
    return found
hc=headers(u);ho=headers(original);oc={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='COVERED_BY_REVIEWED_BODY_OR_DECLARATION' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN_SOURCE_FRAGMENT_RECONCILIATION_REQUIRED') for f in u['fragments']]
orows=[dict(fragment_id=f['fragment_id'],parent=f['member_id'],path=f['path'],kind=f['kind'],is_branch=f['is_branch'],is_inline=f['is_inline'],
    status='COVERED_BY_PROVEN_BODY_OR_DECLARATION' if f['member_id'] in oc or f['fragment_id'] in ho else 'OPEN_SOURCE_FRAGMENT_RECONCILIATION_REQUIRED') for f in original['fragments']]
def counts(rows,predicate):return dict(total=len(rows),branches=sum(r['is_branch'] for r in rows),inline=sum(r['is_inline'] for r in rows),
    covered=sum(predicate(r) for r in rows),branches_covered=sum(r['is_branch'] and predicate(r) for r in rows),inline_covered=sum(r['is_inline'] and predicate(r) for r in rows))
summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',
    historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),
    current=dict(total=len(current),closed=len(closed),OPEN=len(current)-len(closed)),
    support=dict(raw=len(support),distinct=sum(s['distinct'] for s in support),distinct_closed=sum(s['distinct'] and s['fully_reconciled'] for s in support),
        distinct_OPEN=sum(s['distinct'] and not s['fully_reconciled'] for s in support)),
    fragments=dict(current=counts(u['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in hc),
        original=counts(original['fragments'],lambda f:f['member_id'] in oc or f['fragment_id'] in ho)),original_queries=0)
save('historical-ownership-v2.json',dict(members=history,original_queries=0))
save('current-ownership-v2.json',dict(members=current,support=support,fragments=fragments))
save('ownership-summary-v2.json',summary)
save('original-fragment-parent-crosswalk-v2.json',dict(rows=orows))
save('support-fragment-crosswalk-v1.json',dict(current=hc,original=ho,qualification='Only supported module headers and five fully proved host classes/properties; unrelated containers remain OPEN.'))
save('new-closures-final-v1.json',dict(rows=new,new_historical=len(new),new_current_supported=len(closed-inherited),new_current_declarations=0,
    newly_supported_existing_callbacks=sum(c['new_support'] for c in callbacks),new_callback_declarations=0,new_support=new_support,reopened=[],original_queries=0))
oldqueue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())
remaining=[r for r in oldqueue['members'] if r['original_id'] not in newhist]
opens=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],reason='Whole-body/source/dependency/branch/inline obligation remains unresolved') for n in u['members'] if n['symbol_id'] not in closed]
save('remaining-work-queue-v1.json',dict(status='NOT_READY_04A38',members=remaining,first_members=remaining[:24],current_OPEN=opens,
    ordering='Exact335-entry v10 historical machine queue filtered only by33 complete proved canonical IDs; every other OPEN identity retained.',original_queries=0))
save('current-semantic-position-crosswalk-v1.json',dict(rows=[dict(before=k,after=n['symbol_id'],body_bytes_equal=oldm[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],
    old=726,current=len(current),changed_existing=sum(oldm[k]['body_sha256']!=n['body_sha256'] for k,n in mapping.items()),new_declarations=[]))
print(json.dumps(summary))
